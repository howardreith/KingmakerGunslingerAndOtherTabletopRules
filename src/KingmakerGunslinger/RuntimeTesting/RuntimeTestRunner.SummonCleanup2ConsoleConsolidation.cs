using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Harmony12;
using Kingmaker;
using Kingmaker.Assets.Console.GamepadInput;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.RuleSystem.Rules.Abilities;
using Kingmaker.UI._ConsoleUI.ActionBar;
using Kingmaker.UI._ConsoleUI.Context;
using Kingmaker.UI._ConsoleUI.Context.InGame;
using Kingmaker.UI._ConsoleUI.Models;
using Kingmaker.UI._ConsoleUI.InputLayers.InGameLayer;
using Kingmaker.UI._ConsoleUI.Config.InGame;
using Kingmaker.UI._ConsoleUI.SpellBook;
using Kingmaker.UI._ConsoleUI.Utils.MultiNavigationTool;
using Kingmaker.UI._ConsoleUI.Utils.NavigationTool;
using Kingmaker.UI.UnitSettings;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Mechanics.Actions;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json.Linq;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Kingmaker.Blueprints;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using KingmakerGunslinger.Bootstrap;

namespace KingmakerGunslinger.RuntimeTesting
{
    /// <summary>
    /// S5 T2 / DEF-06 native lanes of the elementals-console review. The actual
    /// console ActionBarSelectorVm rows are snapshotted by a first-priority
    /// CollectSpells postfix, before the product's own collection postfix, so
    /// the published list is compared with the exact native list it came from.
    /// </summary>
    internal sealed partial class RuntimeTestRunner
    {
        internal const string Cleanup2ConsoleMagicMissile = "4ac47ddb9fa1eaf43a1b6809980cfbd2";

        private static List<KeyValuePair<ActionBarSelectorVm, ActionBarSelectorItemVm[]>> _cleanup2ConsoleCollections;

        private static void Cleanup2ConsoleCollectionSnapshotPostfix(ActionBarSelectorVm __instance)
        {
            var collections = _cleanup2ConsoleCollections;
            if (collections != null && __instance != null)
                collections.Add(new KeyValuePair<ActionBarSelectorVm, ActionBarSelectorItemVm[]>(__instance, __instance.ItemVms.ToArray()));
        }

        private static ActionBarSelectorItemVm[] Cleanup2ConsoleNativeRows(ActionBarSelectorVm selector)
        {
            var rows = (_cleanup2ConsoleCollections ?? new List<KeyValuePair<ActionBarSelectorVm, ActionBarSelectorItemVm[]>>())
                .Where(value => ReferenceEquals(value.Key, selector)).ToArray();
            if (rows.Length != 1) throw new InvalidOperationException("The native CollectSpells rows of the exact selector were captured " + rows.Length + " times.");
            return rows[0].Value;
        }

        private static string Cleanup2ConsoleRowGuid(ActionBarSelectorItemVm row)
        {
            var data = row?.MechanicActionBarSlot?.GetContentData() as AbilityData;
            return data?.Blueprint?.AssetGuid;
        }

        private static bool Cleanup2ConsoleStandaloneRow(ActionBarSelectorItemVm row)
        {
            string guid = Cleanup2ConsoleRowGuid(row);
            return guid != null && ElementalSourceChooserPolicy.Parent(guid) != null;
        }

        private static JArray Cleanup2ConsoleDescribeRows(IEnumerable<ActionBarSelectorItemVm> rows)
        {
            return new JArray(rows.Select(row => {
                var node = row?.MechanicActionBarSlot as ElementalConsoleNode;
                if (node != null) return (JToken)("launcher:" + node.Title + "#" + System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(row));
                var data = row?.MechanicActionBarSlot?.GetContentData() as AbilityData;
                return (JToken)((data == null ? row?.MechanicActionBarSlot?.GetType().Name : data.Blueprint.AssetGuid + ":" + data.Blueprint.name) +
                    "#" + System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(row));
            }));
        }

        /// <summary>Focus, pause, selection, selector and input owners, compared before and after a native menu.</summary>
        private sealed class Cleanup2ConsoleOwnership
        {
            private readonly ActionBarVM _bar;
            private readonly object _currentSlot;
            private readonly bool _active, _paused;
            private readonly UnitEntityData _unit;
            private readonly InputLayer[] _layers;
            private readonly AbilityData _dispatched;
            internal Cleanup2ConsoleOwnership(ActionBarVM bar)
            {
                _bar = bar; _currentSlot = bar.CurrentSlot.Value; _active = bar.Active.Value; _paused = Game.Instance.IsPaused;
                _unit = UnitSelectionManager.Instance.CurrentSelectUnitValue;
                _layers = GamePad.Instance.Layers.ToArray(); _dispatched = ElementalSourceChooserRuntime.LastDispatched;
            }
            internal JObject Compare(bool expectActive = true)
            {
                return new JObject {
                    ["focusSlotExact"] = ReferenceEquals(_bar.CurrentSlot.Value, _currentSlot),
                    ["actionBarActiveExact"] = !expectActive || _bar.Active.Value == _active,
                    ["pauseExact"] = Game.Instance.IsPaused == _paused, ["paused"] = Game.Instance.IsPaused,
                    ["selectedUnitExact"] = ReferenceEquals(UnitSelectionManager.Instance.CurrentSelectUnitValue, _unit),
                    ["selectorClosed"] = _bar.SelectorVm.Value == null,
                    ["inputLayersExact"] = GamePad.Instance.Layers.SequenceEqual(_layers),
                    ["targetingFree"] = Game.Instance.SelectedAbilityHandler?.Ability == null,
                    ["noNewDispatch"] = ReferenceEquals(ElementalSourceChooserRuntime.LastDispatched, _dispatched) };
            }
            internal static bool Exact(JObject row) { return row.Properties().Where(value => value.Name != "paused").All(value => (bool)value.Value); }
        }

        private static ActionBarSelectorView Cleanup2ConsoleBoundView(ActionBarSelectorVm selector)
        {
            var views = Resources.FindObjectsOfTypeAll<ActionBarSelectorView>().Where(value => value.gameObject.scene.IsValid() &&
                value.gameObject.scene.isLoaded && value.gameObject.activeInHierarchy &&
                ReferenceEquals(ElementalConsoleBookPresentation.Model(value), selector)).ToArray();
            return views.Length == 1 ? views[0] : null;
        }

        /// <summary>Lane A: a native selector whose sources come only from rows that must stay native.</summary>
        private IEnumerable<int> Cleanup2ConsoleNoConsolidation(ExpandedSummoningCorrectionFixture fixture, Spellbook book, ActionBarVM bar,
            Func<bool> uiOwned, Func<bool> selectorInputOwned, ICollection<ActionBarSelectorVm> ownedSelectors)
        {
            var caster = fixture.Caster;
            if (!uiOwned() || bar.SelectorVm.Value != null || Game.Instance.SelectedAbilityHandler?.Ability != null)
                throw new InvalidOperationException("Exact idle owned console action bar required for the no-consolidation lane.");
            var input = new Cleanup2TooltipInputs(caster); var pins = new TeleportationUiSettingsFixture(caster.UISettings);
            var debit = new Cleanup2SourceDebit(caster); var ownership = new Cleanup2ConsoleOwnership(bar);
            _cleanup2ConsoleCollections.Clear();
            typeof(ActionBarVM).GetMethod("ShowSpellMenu", Cleanup2Fields).Invoke(bar, null);
            var selector = bar.SelectorVm.Value;
            if (selector != null) ownedSelectors.Add(selector);
            for (int frame = 0; frame < 8; frame++) yield return 0;
            if (selector == null || !ReferenceEquals(bar.SelectorVm.Value, selector))
                throw new InvalidOperationException("The native console spell selector did not open for the no-consolidation lane.");
            ActionBarSelectorItemVm[] native = Cleanup2ConsoleNativeRows(selector);
            ActionBarSelectorItemVm[] published = selector.ItemVms.ToArray();
            string collection = ElementalSourceChooserConsole.LastCollection;
            int sources = 0;
            const string prefix = "preserve-native-console-rows:no-original-row-consolidated;sources=";
            if (collection.StartsWith(prefix, StringComparison.Ordinal))
                int.TryParse(collection.Substring(prefix.Length).Split(';')[0], out sources);
            string monsterII = ElementalSourceChooserPolicy.Rank(2).MonsterParentGuid;
            string allyII = ElementalSourceChooserPolicy.Rank(2).NaturesAllyParentGuid;
            var view = Cleanup2ConsoleBoundView(selector);
            var row = new JObject {
                ["nativeRows"] = Cleanup2ConsoleDescribeRows(native), ["publishedRows"] = Cleanup2ConsoleDescribeRows(published),
                ["rowReferencesAndOrderExact"] = published.Length == native.Length && published.Select((value, index) => ReferenceEquals(value, native[index])).All(value => value),
                ["syntheticElementalsRows"] = published.Count(value => value.MechanicActionBarSlot is ElementalConsoleNode),
                ["consoleContext"] = ElementalSourceChooserConsole.Context(selector) != null,
                ["nativeRowsDisposed"] = native.Count(value => value.IsDisposed),
                ["standaloneElementalRows"] = native.Count(Cleanup2ConsoleStandaloneRow),
                ["summonMonsterIIRows"] = native.Count(value => Cleanup2ConsoleRowGuid(value) == monsterII),
                ["summonNaturesAllyIIRows"] = native.Count(value => Cleanup2ConsoleRowGuid(value) == allyII),
                ["unrelatedMagicMissileRows"] = native.Count(value => Cleanup2ConsoleRowGuid(value) == Cleanup2ConsoleMagicMissile),
                ["collectionOutcome"] = collection, ["sourcesFromNativeRows"] = sources,
                ["boundNativeView"] = view != null, ["selectorInputOwned"] = selectorInputOwned(),
                ["renderedText"] = view == null ? new JArray() : new JArray(view.GetComponentsInChildren<TextMeshProUGUI>(false).Select(value => value.text)),
                ["scope"] = "Actual console ActionBarSelectorVm built by native CollectSpells for the owned Wizard; rows captured by a first-priority postfix before the product collection." };
            bool exact = (bool)row["rowReferencesAndOrderExact"] && (int)row["syntheticElementalsRows"] == 0 && !(bool)row["consoleContext"] &&
                (int)row["nativeRowsDisposed"] == 0 && (int)row["standaloneElementalRows"] == 0 && (int)row["summonMonsterIIRows"] == 1 &&
                sources > 0 && view != null && (bool)row["selectorInputOwned"];
            Cleanup2Check("elemental-console-no-consolidation-native-rows", exact, row,
                "Sources found only through Summon Monster rows leave the actual console selector exactly native: no Elementals entry, no context, no disposed row.");
            if (!exact) throw new InvalidOperationException("The no-consolidation console selector was not exactly native: " + collection);
            InvokeTeleportGamepadInput(view, "<GetInputLayer>b__11_1");
            for (int frame = 0; frame < 5; frame++) yield return 0;
            JObject owned = ownership.Compare();
            bool closed = Cleanup2ConsoleOwnership.Exact(owned) && selector.IsDisposed && debit.Unspent && input.Unchanged() && pins.IsRestored() && uiOwned();
            owned["selectorDisposedByNativeClose"] = selector.IsDisposed; owned["sourceBudgetsUnspent"] = debit.Unspent;
            owned["originalInputsExact"] = input.Unchanged(); owned["pinsExact"] = pins.IsRestored();
            owned["nativeClose"] = "ActionBarSelectorView decline input (native OnDecline)";
            Cleanup2Check("elemental-console-no-consolidation-native-close", closed, owned,
                "The untouched native selector closes through native decline, spends nothing and restores focus, pause, selection, selector and input layers.");
            if (!closed) throw new InvalidOperationException("The no-consolidation console selector did not close natively with exact ownership.");
        }

        /// <summary>Lane B: the first consolidating selector's rows, against the exact native rows it came from.</summary>
        private void Cleanup2ConsoleConsolidationRows(ActionBarSelectorVm selector, ElementalConsoleContext context, UnitEntityData caster)
        {
            ActionBarSelectorItemVm[] native = Cleanup2ConsoleNativeRows(selector);
            ActionBarSelectorItemVm[] published = selector.ItemVms.ToArray();
            ActionBarSelectorItemVm[] removed = native.Where(Cleanup2ConsoleStandaloneRow).ToArray();
            int first = Array.FindIndex(native, Cleanup2ConsoleStandaloneRow);
            var launchers = published.Where(value => value.MechanicActionBarSlot is ElementalConsoleNode).ToArray();
            var retainedNative = native.Where(value => !removed.Contains(value)).ToArray();
            var retainedPublished = published.Where(value => !(value.MechanicActionBarSlot is ElementalConsoleNode)).ToArray();
            // The approved source set: the one shared policy over the exact native rows, independently of the context.
            var session = new ElementalChooserSession { Unit = caster };
            foreach (var nativeRow in native) ElementalSourceChooserRuntime.Collect(session, nativeRow.MechanicActionBarSlot, 0);
            Func<ElementalNativeSource, string> key = value => System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(value.Source) + "|" +
                value.Child.Blueprint.AssetGuid + "|" + value.CreatureKey + "|" + value.Book.Blueprint.AssetGuid;
            var approved = session.Sources.Select(key).OrderBy(value => value, StringComparer.Ordinal).ToArray();
            var actual = context.Sources.Select(key).OrderBy(value => value, StringComparer.Ordinal).ToArray();
            int providerSources = context.Sources.Count(value => ElementalSourceChooserPolicy.Parent(value.Source.Blueprint.AssetGuid) == null);
            string monsterII = ElementalSourceChooserPolicy.Rank(2).MonsterParentGuid;
            var row = new JObject {
                ["nativeRows"] = Cleanup2ConsoleDescribeRows(native), ["publishedRows"] = Cleanup2ConsoleDescribeRows(published),
                ["removedOriginalStandaloneRows"] = removed.Length, ["firstRemovedPosition"] = first,
                ["launchers"] = launchers.Length, ["launcherPosition"] = launchers.Length == 1 ? Array.IndexOf(published, launchers[0]) : -1,
                ["retainedReferencesAndOrderExact"] = retainedPublished.SequenceEqual(retainedNative),
                ["removedNativeRowsDisposed"] = removed.Count(value => value.IsDisposed),
                ["retainedNativeRowsDisposed"] = retainedNative.Count(value => value.IsDisposed),
                ["retainedSummonMonsterII"] = retainedPublished.Count(value => Cleanup2ConsoleRowGuid(value) == monsterII),
                ["retainedMagicMissile"] = retainedPublished.Count(value => Cleanup2ConsoleRowGuid(value) == Cleanup2ConsoleMagicMissile),
                ["approvedSources"] = approved.Length, ["contextSources"] = actual.Length, ["providerRowSources"] = providerSources,
                ["sourceSetExact"] = approved.SequenceEqual(actual, StringComparer.Ordinal),
                ["collectionOutcome"] = ElementalSourceChooserConsole.LastCollection };
            bool exact = removed.Length == 6 && first >= 0 && launchers.Length == 1 && (int)row["launcherPosition"] == first &&
                (bool)row["retainedReferencesAndOrderExact"] && (int)row["removedNativeRowsDisposed"] == removed.Length &&
                (int)row["retainedNativeRowsDisposed"] == 0 && (int)row["retainedSummonMonsterII"] == 1 && (int)row["retainedMagicMissile"] == 1 &&
                (bool)row["sourceSetExact"] && providerSources > 0 &&
                ElementalSourceChooserConsole.LastCollection == "native-console-elementals-ready;removed=6;position=" + first;
            Cleanup2Check("elemental-console-consolidation-one-launcher", exact, row,
                "The six original standalone rows leave the actual selector; one launcher takes the first one's place; every other native row keeps its object and order; only removed rows are disposed; the source set is the shared policy's.");
            if (!exact) throw new InvalidOperationException("The consolidating console selector did not stage exactly one launcher in place.");
        }

        /// <summary>Lane B: a node of a closed selector reaches the native Result and is refused with no spend.</summary>
        private void Cleanup2ConsoleStaleNode(ActionBarVM bar, ElementalConsoleNode stale, UnitEntityData caster)
        {
            if (stale == null || stale.Source == null || bar.SelectorVm.Value != null)
                throw new InvalidOperationException("A retained leaf of a closed selector is required for the stale-node check.");
            var debit = new Cleanup2SourceDebit(caster); var ownership = new Cleanup2ConsoleOwnership(bar);
            bool current = stale.Context.Current;
            MechanicActionBarSlot mechanism = stale;
            bar.Result(mechanism, true);
            JObject row = ownership.Compare();
            row["contextCurrentBefore"] = current; row["outcome"] = ElementalSourceChooserConsole.Outcome;
            row["sourceBudgetsUnspent"] = debit.Unspent; row["commandQueued"] = !caster.Commands.Empty;
            row["nativeEntry"] = "ActionBarVM.Result(staleNode, execute=true): a delayed use of a leaf whose selector was declined";
            bool refused = !current && ElementalSourceChooserConsole.Outcome == "refuse-stale-console-node-no-spend" &&
                Cleanup2ConsoleOwnership.Exact(row) && debit.Unspent && caster.Commands.Empty;
            Cleanup2Check("elemental-console-stale-node-refused", refused, row,
                "A leaf of a declined selector is refused at the native Result: nothing is dispatched or spent and every owner stays exact.");
            if (!refused) throw new InvalidOperationException("A stale console node was not refused with no spend.");
        }

        /// <summary>Lane B: the exhausted original source is refused at native Use; the menu stays owned and closes natively.</summary>
        private IEnumerable<int> Cleanup2ConsoleExhaustedSource(ExpandedSummoningCorrectionFixture fixture, Spellbook book, ActionBarVM bar,
            Func<bool> uiOwned, Func<bool> selectorInputOwned, Action rememberSelector, ICollection<ActionBarSelectorVm> ownedSelectors,
            ICollection<AbilityData> ownedDispatched)
        {
            var caster = fixture.Caster;
            if (!uiOwned() || bar.SelectorVm.Value != null || Game.Instance.SelectedAbilityHandler?.Ability != null)
                throw new InvalidOperationException("Exact idle owned console action bar required for the exhausted-source lane.");
            var debit = new Cleanup2SourceDebit(caster); var ownership = new Cleanup2ConsoleOwnership(bar);
            int available = CountAvailableExpandedSummoningSlots(book, 2);
            _cleanup2ConsoleCollections.Clear();
            typeof(ActionBarVM).GetMethod("ShowSpellMenu", Cleanup2Fields).Invoke(bar, null);
            if (bar.SelectorVm.Value != null) ownedSelectors.Add(bar.SelectorVm.Value);
            rememberSelector();
            for (int frame = 0; frame < 6; frame++) yield return 0;
            rememberSelector();
            foreach (int frame in Cleanup2ConsoleChoose(bar, value => value.Rank == 0 && value.Source == null, false, uiOwned, selectorInputOwned, rememberSelector, ownedDispatched)) yield return frame;
            foreach (int frame in Cleanup2ConsoleChoose(bar, value => value.Rank == 2 && value.Creature == null, false, uiOwned, selectorInputOwned, rememberSelector, ownedDispatched)) yield return frame;
            foreach (int frame in Cleanup2ConsoleChoose(bar, value => value.Creature == "small-fire-elemental" && value.Source == null, false, uiOwned, selectorInputOwned, rememberSelector, ownedDispatched)) yield return frame;
            var selector = bar.SelectorVm.Value; var context = ElementalSourceChooserConsole.Context(selector);
            var spent = context.Sources.Single(value => value.CreatureKey == "small-fire-elemental" && ReferenceEquals(value.Book, book) &&
                ElementalSourceChooserPolicy.Parent(value.Source.Blueprint.AssetGuid) != null);
            var provider = context.Sources.Where(value => value.CreatureKey == "small-fire-elemental" &&
                ElementalSourceChooserPolicy.Parent(value.Source.Blueprint.AssetGuid) == null).ToArray();
            string validation = spent.Validate();
            foreach (int frame in Cleanup2ConsoleChoose(bar, value => ReferenceEquals(value.Source, spent), true, uiOwned, selectorInputOwned, rememberSelector, ownedDispatched)) yield return frame;
            string outcome = ElementalSourceChooserConsole.Outcome;
            var stillOpen = bar.SelectorVm.Value;
            var row = new JObject {
                ["exhaustedSource"] = spent.Label, ["validation"] = validation, ["outcome"] = outcome,
                ["providerSourcesStillOffered"] = provider.Length, ["providerSourceValid"] = provider.Length == 1 && provider[0].Validate() == null,
                ["selectorStillOwned"] = ReferenceEquals(stillOpen, selector) && ElementalSourceChooserConsole.Context(stillOpen) != null && ElementalSourceChooserConsole.Context(stillOpen).Current,
                ["targetingFree"] = Game.Instance.SelectedAbilityHandler?.Ability == null, ["commandQueued"] = !caster.Commands.Empty,
                ["sourceBudgetsUnspent"] = debit.Unspent, ["availableLevel2Slots"] = CountAvailableExpandedSummoningSlots(book, 2),
                ["availableBefore"] = available, ["nativeInput"] = "ActionBarSelectorView Use (OnFunc01) on the exhausted original leaf" };
            bool refused = validation != null && validation.Contains("exhausted") &&
                outcome.StartsWith("refuse-console-source-no-spend:", StringComparison.Ordinal) && outcome.Contains("exhausted") &&
                (bool)row["selectorStillOwned"] && (bool)row["targetingFree"] && caster.Commands.Empty && debit.Unspent &&
                CountAvailableExpandedSummoningSlots(book, 2) == available && (bool)row["providerSourceValid"];
            Cleanup2Check("elemental-console-exhausted-source-refused", refused, row,
                "Use on the spent original source is refused before the native Result: no dispatch, command or debit, the other available source stays offered and the selector stays owned.");
            if (!refused) throw new InvalidOperationException("The exhausted console source was not refused with no spend.");
            InvokeTeleportGamepadInput(context.BoundView, "<GetInputLayer>b__11_1");
            for (int frame = 0; frame < 5; frame++) yield return 0;
            JObject owned = ownership.Compare(false);
            owned["selectorDisposed"] = selector.IsDisposed; owned["sourceBudgetsUnspent"] = debit.Unspent;
            bool closed = Cleanup2ConsoleOwnership.Exact(owned) && selector.IsDisposed && debit.Unspent;
            Cleanup2Check("elemental-console-exhausted-source-closed", closed, owned,
                "After the refusal the native decline closes the selector with focus, pause, selection and input layers exact and nothing spent.");
            if (!closed) throw new InvalidOperationException("The console selector did not close after the exhausted-source refusal.");
        }

        private void Cleanup2ConsoleNoLeaks(ActionBarVM bar, ICollection<ActionBarSelectorVm> selectors, ICollection<ElementalConsoleContext> contexts)
        {
            var row = new JObject {
                ["selectorClosed"] = bar.SelectorVm.Value == null, ["ownedSelectors"] = selectors.Count,
                ["ownedSelectorsDisposed"] = selectors.Count(value => value.IsDisposed),
                ["ownedContexts"] = contexts.Count, ["contextsStillCurrent"] = contexts.Count(value => value.Current),
                ["itemVmsLeftOnDisposedSelectors"] = selectors.Where(value => value.IsDisposed).Sum(value => value.ItemVms.Count) };
            bool exact = bar.SelectorVm.Value == null && selectors.All(value => value.IsDisposed) && contexts.All(value => !value.Current) &&
                (int)row["itemVmsLeftOnDisposedSelectors"] == 0;
            Cleanup2Check("elemental-console-no-leaked-selector-node-or-context", exact, row,
                "Every selector the phase opened is disposed with its item VMs and nodes; no console context remains current.");
            if (!exact) throw new InvalidOperationException("A console selector, node VM or context leaked.");
        }

        private static BlueprintAbility Cleanup2ConsoleAbility(string guid, string name)
        { return BlueprintLibraryLookup.RequireExact<BlueprintAbility>(BlueprintBootstrap.Library, guid, name); }
    }
}
