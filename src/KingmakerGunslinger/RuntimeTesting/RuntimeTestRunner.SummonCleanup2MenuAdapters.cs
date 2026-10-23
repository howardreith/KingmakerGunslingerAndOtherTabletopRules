using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using Harmony12;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.RuleSystem.Rules.Abilities;
using Kingmaker.UI.ActionBar;
using Kingmaker.UI.Group;
using Kingmaker.UI.Tooltip;
using Kingmaker.UI.UnitSettings;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics.Actions;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        private sealed class Cleanup2MenuAdapterTarget
        {
            internal SummonVariantSpec Spec;
            internal BlueprintAbility Parent, Child;
            internal string Label { get { return Spec.Family + "-horse-" + Spec.Multiplicity; } }
        }

        // Save-free observations of real native rows. Existing source objects
        // are established before the tab opens, never synthesized into a row.
        // "Existing" here is this fixture's intake, not old-save hydration.
        private IEnumerable<int> ReviewSummonCleanup2MenuAdapters(
            ExpandedSummoningCorrectionFixture fixture, Spellbook book)
        {
            if (!IsSummonCleanup2Review || !_request.ExitAfterCompletion || _request.Parameters == null ||
                _request.Parameters.Count != 1 || (string)_request.Parameters["reviewPhase"] != "menu-adapters")
                throw new InvalidOperationException("Exact automatic-exit menu-adapters request required.");
            var game = Game.Instance; var player = game.Player; var ui = game.UI;
            var group = GroupController.Instance; var bar = ActionBarManager.Instance;
            if (!Cleanup2NoNarrativeOverlay() || group == null || bar?.Group == null || ui?.SelectionManagerPC == null ||
                game.IsControllerGamepad || ui.TooltipsController?.ContextTooltipData != null || !fixture.Caster.Commands.Empty)
                throw new InvalidOperationException("Idle native desktop fixture/action bar required; console transition is not owned here.");
            var party = player.Party.ToArray(); var selected = ui.SelectionManagerPC.SelectedUnits.ToArray();
            var selectedGroup = group.GetCurrentCharacter(); var groups = bar.Group.GroupElements.ToArray();
            bool[] toggles = groups.Select(value => value.ToggleState).ToArray();
            if (toggles.Any(value => value)) throw new InvalidOperationException("Closed native groups required at intake.");
            var originalInputs = party.Select(value => new Cleanup2TooltipInputs(value)).ToArray();
            var originalBars = party.Select(value => new TeleportationUiSettingsFixture(value.UISettings)).ToArray();
            var fixtureCasterAtIntake = fixture.Caster;
            var fixtureBooksBefore = fixture.Caster.Descriptor.Spellbooks.ToArray();
            var fixtureStatsBefore = new[] { fixture.Caster.Stats.Intelligence.BaseValue, fixture.Caster.Stats.Wisdom.BaseValue, fixture.Caster.Stats.Charisma.BaseValue };
            bool fixtureDetachedBefore = fixture.Caster.IsDetached;
            var addedBooks = new TeleportResourceFixtureOwner(fixture.Caster);
            var watch = Stopwatch.StartNew(); bool added = false, selectedFixture = false, bodyCompleted = false;
            string stage = "resolve-native-routes";
            Cleanup2NativeDefaultScope nativeMode = null; NativeIconScreenEvidence screens = null;
            Action removeConversion = null;
            try
            {
                var targets = new List<Cleanup2MenuAdapterTarget>();
                foreach (var family in new[] { SummonFamily.Monster, SummonFamily.NaturesAlly })
                foreach (var quantity in new[] { SummonMultiplicity.One, SummonMultiplicity.OneD3, SummonMultiplicity.OneD4PlusOne })
                {
                    int tier = quantity == SummonMultiplicity.One ? 2 : quantity == SummonMultiplicity.OneD3 ? 3 : 4;
                    var spec = ExpandedSummoningCatalog.GenerateVariants(family).Single(value =>
                        value.Creature.Key == "horse" && value.Multiplicity == quantity && value.ParentTier == tier);
                    var parent = BlueprintLibraryLookup.RequireExact<BlueprintAbility>(BlueprintBootstrap.Library,
                        ExpandedSummoningInventoryObserver.CanonicalParentGuids[(family == SummonFamily.Monster ? 0 : 9) + spec.ParentTier - 1],
                        "native adapter canonical parent");
                    var child = ExpandedSummoningRoot(fixture.Blueprints, spec);
                    if (!SummonVisibilityCatalog.IsPublished(spec) || parent.Variants == null ||
                        parent.Variants.Count(value => ReferenceEquals(value, child)) != 1)
                        throw new InvalidOperationException("Exact published Horse route missing: " + spec.StableKey);
                    targets.Add(new Cleanup2MenuAdapterTarget { Spec = spec, Parent = parent, Child = child });
                }
                var sorcerer = BlueprintLibraryLookup.RequireExact<BlueprintCharacterClass>(BlueprintBootstrap.Library,
                    "b3a505fb61437dc4097f43c3f8f9a4cf", "native known-book adapter Sorcerer");
                stage = "prepare-request-owned-books";
                var knownBook = addedBooks.AddBook(sorcerer.Spellbook);
                if (!knownBook.Blueprint.Spontaneous || knownBook.CasterLevel != 20)
                    throw new InvalidOperationException("Native CL20 spontaneous book required.");
                foreach (var target in targets)
                {
                    PrepareExpandedSummoningPlayerPathSpell(book, target.Parent, target.Parent, target.Spec.ParentTier);
                    knownBook.AddKnown(target.Spec.ParentTier, target.Parent, true);
                }
                knownBook.Rest();
                var knownBefore = Enumerable.Range(0, 10).SelectMany(knownBook.GetKnownSpells).ToArray();
                var preparedBefore = new Cleanup2ElementalBookSnapshot(book);
                if (!player.Party.Contains(fixture.Caster))
                { player.PartyCharacters.Add(fixture.Caster); added = true; player.InvalidateCharacterLists(); player.UpdateCharacterLists(); }
                stage = "acquire-native-default-after-book-setup";
                nativeMode = Cleanup2EnterNativeDefault(fixture, "menu-adapters");
                // Native set_IsPaused(true)06000C9B queues StartMode(Pause).
                // Presentation does not spend preparations, and requesting a
                // Pause here would invalidate the exact Default lease on the
                // next normal frame. Keep setup before acquisition; never
                // remove an unexpected/foreign Pause to retain the lease.
                screens = new NativeIconScreenEvidence(_request);
                stage = "settle-native-fixture-selection";
                ui.SelectionManagerPC.SelectUnit(fixture.Caster.View, true, true, false); group.SelectUnit(fixture.Caster);
                selectedFixture = true;
                for (int frame = 0; frame < 12; frame++) yield return 0;
                Cleanup2Check("menu-adapters-native-default-settled", nativeMode.Current, new JObject {
                    ["stage"] = stage, ["nativeMode"] = nativeMode.Describe(), ["observerRequestedPause"] = false,
                    ["bookSetupCompletedBeforeLease"] = true },
                    "Book preparation precedes strict native Default admission; normal selection frames must preserve its exact stack and controllers without a Pause request.");
                if (!nativeMode.Current) throw new InvalidOperationException("Native menu adapter Default/selection did not settle; inspect exact stack/controller receipt.");
                var spellGroup = groups.Single(value => (ActionBarSlotType)typeof(ActionBarGroupElement)
                    .GetField("SlotType", Cleanup2Fields).GetValue(value) == ActionBarSlotType.Spell);
                var inputs = new Cleanup2TooltipInputs(fixture.Caster);
                var fixturePins = new TeleportationUiSettingsFixture(fixture.Caster.UISettings);
                var rows = new JArray();
                foreach (bool known in new[] { false, true })
                foreach (var target in targets)
                {
                    stage = (known ? "known-" : "prepared-") + target.Label;
                    Cleanup2MenuAdapterBoundary(nativeMode, watch);
                    if (!spellGroup.ToggleState) spellGroup.OnClick();
                    var levels = (ActionBarSubGroupLevels)typeof(ActionBarGroupElement).GetField("m_Levels", Cleanup2Fields).GetValue(spellGroup);
                    if (levels == null) throw new InvalidOperationException("Native spell-level adapter missing.");
                    levels.OnClick(target.Spec.ParentTier);
                    for (int frame = 0; frame < 8; frame++) yield return 0;
                    Spellbook selectedBook = known ? knownBook : book;
                    var sources = spellGroup.GetComponentsInChildren<ActionBarGroupSlot>(true).Where(value => {
                        var data = value.MechanicSlot?.GetContentData() as AbilityData;
                        return value.gameObject.activeInHierarchy && ReferenceEquals(data?.Blueprint, target.Parent) &&
                            ReferenceEquals(data?.Spellbook, selectedBook);
                    }).ToArray();
                    if (sources.Length != 1) throw new InvalidOperationException("Native adapter source count=" + sources.Length + ":" + known + ":" + target.Label);
                    var source = sources[0]; var mechanism = source.MechanicSlot;
                    var original = mechanism.GetContentData() as AbilityData;
                    bool sourceKind = known ? mechanism is MechanicActionBarSlotSpontaneusSpell : mechanism is MechanicActionBarSlotMemorizedSpell;
                    if (!sourceKind) throw new InvalidOperationException("Unexpected real native source mechanism: " + mechanism.GetType().FullName);
                    try
                    {
                        foreach (int frame in Cleanup2AwaitStationaryNativeHud(source, stage)) yield return frame;
                        source.OnToggleGroupClick();
                        for (int frame = 0; frame < 8; frame++) yield return 0;
                        bool capture = !known;
                        foreach (int frame in Cleanup2MenuAdapterPopup(source, target, original, inputs, nativeMode, watch,
                            screens, (known ? "known-" : "prepared-") + target.Label, capture, rows)) yield return frame;
                    }
                    finally { Cleanup2MenuAdapterHide(source, original); }
                }
                bool unchanged = inputs.Unchanged() && fixturePins.IsRestored() && preparedBefore.Unchanged(book) &&
                    Enumerable.Range(0, 10).SelectMany(knownBook.GetKnownSpells).SequenceEqual(knownBefore);
                Cleanup2Check("menu-adapters-twelve-native-horse-routes", rows.Count == 12 && unchanged,
                    new JObject { ["rows"] = rows, ["preparedAndKnownReferencesExact"] = unchanged,
                        ["fixtureAccess"] = "Canonical parents explicitly prepared/known on real native Wizard and spontaneous Sorcerer books attached to the disposable caster before UI intake; not natural learning or old-save proof.",
                        ["scope"] = "Two families, native prepared/known mechanisms, quantities 1/1d3/1d4+1. Targeted Horse rows only, not whole-roster page coverage. No spell activation or resource spending." },
                    "All twelve real native source/quantity rows preserve source/book identities and current labels, icons, slots, resources and pins.");
                if (!unchanged || rows.Count != 12) throw new InvalidOperationException("Native adapter input preservation failed.");
                var pinTarget = targets.Single(value => value.Spec.Family == SummonFamily.Monster && value.Spec.Multiplicity == SummonMultiplicity.One);
                stage = "native-original-horse-pin";
                foreach (int frame in Cleanup2MenuAdapterPin(fixture, book, spellGroup, pinTarget, nativeMode, watch, screens)) yield return frame;
                Cleanup2Check("menu-adapter-pin-array-restored", fixturePins.IsRestored(),
                    new JObject { ["exactOriginalSlotArrayAndContents"] = fixturePins.IsRestored() },
                    "The native pin probe restores the entire fixture hotbar array and metadata before the next book/feature grant.");
                if (!fixturePins.IsRestored()) throw new InvalidOperationException("Native pin probe changed unrelated hotbar state.");
                stage = "native-druid-conversion-and-cast";
                foreach (int frame in Cleanup2MenuAdapterDruid(fixture, addedBooks, spellGroup, nativeMode, watch, screens,
                    value => removeConversion = value)) yield return frame;
                // Actual console activation/learning is owned by a separate
                // native input transaction. No hand-built selector VM is proof.
                Cleanup2Check("menu-adapters-surface-limits", true, new JObject {
                    ["desktopCases"] = rows.Count, ["console"] = "unqualified: this request owns desktop input only; no console surface/VM was constructed or activated",
                    ["oldSave"] = "unqualified: fixture-established known/prepared/pinned objects are not cold-save hydration",
                    ["castCostsAndSummons"] = "Only the actual Druid SNAII Horse ordinary command/cast/debit is exercised; the twelve prepared/known quantity rows remain presentation observations.",
                    ["maximumWallSeconds"] = 240, ["wallSeconds"] = watch.Elapsed.TotalSeconds },
                    "The supported desktop slice and distinct console, persistence and execution limits remain explicit.");
                bodyCompleted = true;
            }
            finally
            {
                var errors = new List<string>();
                Action<string, Action> restore = (name, action) => { try { action(); } catch (Exception error) { errors.Add(name + ":" + error.Message); } };
                bool booksReleaseCompleted = false;
                try
                {
                    JObject casterOwnership;
                    bool ownsCaster = Cleanup2SourcesOwnsCaster(fixture, fixtureCasterAtIntake, out casterOwnership);
                    var replacements = ownsCaster ? addedBooks.Books.Where(value => {
                        var actual = fixture.Caster.Descriptor.GetSpellbook(value.Blueprint);
                        return actual != null && !ReferenceEquals(actual, value);
                    }).Select(value => value.Blueprint.AssetGuid).ToArray() : new string[0];
                    if (!ownsCaster || replacements.Length != 0) errors.Add("Foreign extra-book replacement or unowned fixture caster; not modified.");
                    else
                    {
                        restore("conversion-feature", () => removeConversion?.Invoke());
                        restore("owned-books", () => { addedBooks.Restore(); booksReleaseCompleted = true; });
                    }
                    bool untouchedUi = !selectedFixture && ReferenceEquals(group.GetCurrentCharacter(), selectedGroup) &&
                        ui.SelectionManagerPC.SelectedUnits.SequenceEqual(selected);
                    bool fixtureUi = ReferenceEquals(group.GetCurrentCharacter(), fixture.Caster) &&
                        ui.SelectionManagerPC.SelectedUnits.Count == 1 && ReferenceEquals(ui.SelectionManagerPC.SelectedUnits.First(), fixture.Caster);
                    bool uiOwned = ReferenceEquals(GroupController.Instance, group) && ReferenceEquals(ActionBarManager.Instance, bar) &&
                        bar.Group.GroupElements.SequenceEqual(groups) && ui.TooltipsController?.ContextTooltipData == null &&
                        game.SelectedAbilityHandler?.Ability == null && (untouchedUi || fixtureUi);
                    if (!uiOwned) errors.Add("Native group/tooltip/targeting changed; foreign UI is not overwritten.");
                    else
                    {
                        if (!untouchedUi)
                            for (int i = 0; i < groups.Length; i++) { int index = i; restore("toggle-" + i, () => groups[index].Toggle(toggles[index], true)); }
                        restore("party", () => { if (added) { player.PartyCharacters.Remove(fixture.Caster); player.InvalidateCharacterLists(); player.UpdateCharacterLists(); } });
                        if (!untouchedUi)
                        {
                            restore("group", () => group.SelectUnit(selectedGroup));
                            restore("selection", () => ui.SelectionManagerPC.MultiSelect(selected.Where(value => value?.View != null).Select(value => value.View).ToArray(), false));
                        }
                    }
                    // The disposable Wizard's known/prepared contents were
                    // deliberately exercised after owner intake. Its outer
                    // fixture teardown owns those changes. Do not use the
                    // extra-book owner's original CONTENT fingerprint as a
                    // claim that the native extra books failed to release.
                    bool fixtureBooksExact = ownsCaster && fixture.Caster.Descriptor.Spellbooks.SequenceEqual(fixtureBooksBefore);
                    bool extraBooksAbsent = ownsCaster && addedBooks.Books.All(value => fixture.Caster.Descriptor.GetSpellbook(value.Blueprint) == null);
                    bool fixtureStatsExact = ownsCaster && new[] { fixture.Caster.Stats.Intelligence.BaseValue, fixture.Caster.Stats.Wisdom.BaseValue, fixture.Caster.Stats.Charisma.BaseValue }
                        .SequenceEqual(fixtureStatsBefore) && fixture.Caster.IsDetached == fixtureDetachedBefore;
                    bool ownedReleased = booksReleaseCompleted && fixtureBooksExact && extraBooksAbsent && fixtureStatsExact;
                    bool exact = errors.Count == 0 && player.Party.SequenceEqual(party) &&
                        ui.SelectionManagerPC.SelectedUnits.SequenceEqual(selected) && ReferenceEquals(group.GetCurrentCharacter(), selectedGroup) &&
                        originalInputs.All(value => value.Unchanged()) && originalBars.All(value => value.IsRestored()) && ownedReleased;
                    Cleanup2Check("menu-adapters-owned-ui-restored", exact, new JObject {
                        ["partyExact"] = player.Party.SequenceEqual(party), ["selectionExact"] = ui.SelectionManagerPC.SelectedUnits.SequenceEqual(selected),
                        ["originalInputsAndPinsExact"] = originalInputs.All(value => value.Unchanged()) && originalBars.All(value => value.IsRestored()),
                        ["ownedBooksRemoved"] = ownedReleased, ["nativeExtraBookReleaseCompleted"] = booksReleaseCompleted,
                        ["originalFixtureBookReferencesExact"] = fixtureBooksExact, ["ownedExtraBooksAbsent"] = extraBooksAbsent,
                        ["fixtureStatsAndDetachmentExact"] = fixtureStatsExact, ["casterOwnership"] = casterOwnership,
                        ["foreignBookReplacements"] = new JArray(replacements), ["lastStage"] = stage, ["bodyCompleted"] = bodyCompleted,
                        ["disposableBookContentScope"] = "Intentionally exercised Wizard preparations/known objects are disposed by outer fixture teardown; original party resources and pins remain independently exact.",
                        ["errors"] = new JArray(errors) },
                        "Only this disposable caster's owned books/fact/pin and native UI are changed; original party resources and pins restore exactly.");
                }
                finally
                {
                    restore("native-screens", () => screens?.Dispose());
                    restore("native-default", () => nativeMode?.Dispose());
                }
                Cleanup2Check("menu-adapters-finalizers", errors.Count == 0, new JObject {
                    ["lastStage"] = stage, ["bodyCompleted"] = bodyCompleted, ["errors"] = new JArray(errors),
                    ["originalBodyFailurePreserved"] = !bodyCompleted,
                    ["nativeMode"] = nativeMode == null ? JValue.CreateNull() : (JToken)nativeMode.Describe() },
                    "Finalizer failures are retained independently; they never replace an already failing menu case's original exception.");
                if (errors.Count != 0 && bodyCompleted) throw new InvalidOperationException("Menu adapter cleanup unverified: " + string.Join("|", errors.ToArray()));
            }
        }

        private void Cleanup2MenuAdapterBoundary(Cleanup2NativeDefaultScope mode, Stopwatch watch)
        {
            if (watch.Elapsed.TotalSeconds > 240 || !mode.Current || !Cleanup2NoNarrativeOverlay() ||
                Game.Instance.IsControllerGamepad || Game.Instance.SelectedAbilityHandler.Ability != null)
                throw new InvalidOperationException("Native adapter wall/input/narrative ownership boundary changed.");
        }

        private void Cleanup2MenuAdapterHide(ActionBarGroupSlot source, AbilityData original)
        {
            var game = Game.Instance; var tips = game?.UI?.TooltipsController;
            var popupField = typeof(ActionBarGroupSlot).GetField("SubGroup", Cleanup2Fields);
            var popupSourceField = typeof(ActionBarSpellsGroup).GetField("m_SourceSpell", Cleanup2Fields);
            var activeField = typeof(TooltipTrigger).GetField("IsActive", Cleanup2Fields);
            var showField = typeof(TooltipTrigger).GetField("m_ShowTooltipAction", Cleanup2Fields);
            var hideField = typeof(TooltipTrigger).GetField("m_HideTooltipAction", Cleanup2Fields);
            var popup = source == null || popupField == null ? null : popupField.GetValue(source) as ActionBarSpellsGroup;
            var context = tips?.ContextTooltipData;
            bool sourceExact = source != null && original != null && ReferenceEquals(source.MechanicSlot?.GetContentData(), original) &&
                ReferenceEquals(Cleanup2MenuAdapterSelected(source)?.Descriptor, original.Caster);
            bool popupExact = popup != null && popupSourceField != null && ReferenceEquals(popupSourceField.GetValue(popup), original);
            var popupActiveField = typeof(ActionBarSpellsGroup).GetField("m_IsActive", Cleanup2Fields);
            var popupSelectedField = typeof(ActionBarSpellsGroup).GetField("m_Selected", Cleanup2Fields);
            var popupSlotsField = typeof(ActionBarSpellsGroup).GetField("m_Slots", Cleanup2Fields);
            var nativeSlots = popup == null || popupSlotsField == null ? null : popupSlotsField.GetValue(popup) as System.Collections.ICollection;
            bool alreadyClosed = popup != null && !popup.gameObject.activeSelf && popupActiveField != null && popupSelectedField != null &&
                popupSourceField != null && !(bool)popupActiveField.GetValue(popup) && popupSelectedField.GetValue(popup) == null &&
                popupSourceField.GetValue(popup) == null && nativeSlots != null && nativeSlots.Count == 0;
            var candidates = source == null ? new TooltipTrigger[0] : source.GetComponentsInChildren<TooltipTrigger>(true);
            var triggers = sourceExact && popupExact && context != null && activeField != null && showField != null && hideField != null ?
                candidates.Where(trigger => {
                    var row = trigger.GetComponentInParent<ActionBarSlot>();
                    var ability = row?.MechanicSlot?.GetContentData() as AbilityData;
                    bool ownRow = ReferenceEquals(row, source) || (row != null && row.transform.IsChildOf(popup.transform));
                    bool chain = ReferenceEquals(ability, original) || ReferenceEquals(ability?.ConvertedFrom, original) ||
                        ReferenceEquals(ability?.ConvertedFrom?.ConvertedFrom, original);
                    return trigger.gameObject.activeInHierarchy && trigger.isActiveAndEnabled && (bool)activeField.GetValue(trigger) &&
                        ReferenceEquals(trigger.Data, context) && ReferenceEquals(context.AbilityData, ability) && ownRow && chain &&
                        ReferenceEquals(ability?.Caster, original.Caster) && ReferenceEquals(ability?.Spellbook, original.Spellbook) &&
                        showField.GetValue(trigger) == null && hideField.GetValue(trigger) == null;
                }).ToArray() : new TooltipTrigger[0];
            var receipt = new JObject { ["sourceExact"] = sourceExact, ["popupExact"] = popupExact,
                ["nativePopupAlreadyCleared"] = alreadyClosed,
                ["tooltipControllerPresent"] = tips != null, ["contextAbsent"] = context == null,
                ["contextAbility"] = context?.AbilityData?.Blueprint.AssetGuid,
                ["contextIdentity"] = context == null ? 0 : System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(context),
                ["exactActiveStandardTriggerMatches"] = triggers.Length,
                ["triggerCandidateCount"] = candidates.Length,
                ["triggerCandidates"] = new JArray(candidates.Take(32).Select(trigger => {
                    var row = trigger.GetComponentInParent<ActionBarSlot>(); var ability = row?.MechanicSlot?.GetContentData() as AbilityData;
                    return new JObject { ["instance"] = trigger.GetInstanceID(), ["object"] = trigger.name,
                        ["activeInHierarchy"] = trigger.gameObject.activeInHierarchy, ["enabled"] = trigger.isActiveAndEnabled,
                        ["nativeHoverActive"] = activeField != null && (bool)activeField.GetValue(trigger),
                        ["dataIsContext"] = context != null && ReferenceEquals(trigger.Data, context),
                        ["dataAbilityIsExactRowObject"] = trigger.Data != null && ReferenceEquals(trigger.Data.AbilityData, ability),
                        ["rowInstance"] = row == null ? 0 : row.GetInstanceID(), ["rowAbility"] = ability?.Blueprint.AssetGuid,
                        ["rowSource"] = ability?.ConvertedFrom?.Blueprint.AssetGuid,
                        ["casterExact"] = original != null && ReferenceEquals(ability?.Caster, original.Caster),
                        ["bookExact"] = original != null && ReferenceEquals(ability?.Spellbook, original.Spellbook),
                        ["standardNativeActions"] = showField != null && hideField != null && showField.GetValue(trigger) == null && hideField.GetValue(trigger) == null };
                })),
                ["sourceInstance"] = source == null ? 0 : source.GetInstanceID(), ["popupInstance"] = popup == null ? 0 : popup.GetInstanceID(),
                ["nativeContract"] = "TooltipTrigger.OnPointerEnter06003246 may create incidental native hover. SetTemporaryCooldown06003227 hides visual tooltip but does not clear ContextTooltipData. Only one exact live standard trigger's own Data/source chain admits ShowTooltipManual(false)->StandartHide0600324C." };
            bool admitted = sourceExact && (popupExact || alreadyClosed) && tips != null && (context == null || triggers.Length == 1);
            Cleanup2Check("menu-adapter-popup-close-admission-" + (original?.Blueprint.AssetGuid ?? "missing"), admitted, receipt,
                "Popup closure retains exact source ownership and refuses foreign hover; an incidental hover is owned only by one exact live native trigger and its retained source chain.");
            if (!admitted) throw new InvalidOperationException("Foreign or unproved source/popup/tooltip; popup teardown refused: " + receipt);
            if (context != null)
            {
                // Installed ShowTooltipManual0600324A delegates to the same
                // standard hide as native pointer exit. It clears the context
                // synchronously; visual fade may settle after this popup hides.
                if (!ReferenceEquals(tips, game.UI.TooltipsController) || !ReferenceEquals(tips.ContextTooltipData, context) ||
                    !ReferenceEquals(triggers[0].Data, context)) throw new InvalidOperationException("Owned hover changed before native exit; no tooltip is hidden.");
                triggers[0].ShowTooltipManual(false);
                if (!ReferenceEquals(tips, game.UI.TooltipsController) || tips.ContextTooltipData != null)
                    throw new InvalidOperationException("Exact native popup hover did not release its context.");
            }
            source.HideSubGroup();
        }

        private static UnitEntityData Cleanup2MenuAdapterSelected(ActionBarSlot slot)
        {
            // Installed 2.1.7b stores the rendered slot's owner in a protected
            // field. Read this exact native receipt; do not infer ownership
            // from the mechanism or a currently selected different unit.
            var selected = typeof(ActionBarSlot).GetField("Selected", Cleanup2Fields);
            if (selected == null || selected.FieldType != typeof(UnitEntityData))
                throw new InvalidOperationException("Exact native action-slot owner field unavailable.");
            return slot == null ? null : selected.GetValue(slot) as UnitEntityData;
        }

        private IEnumerable<int> Cleanup2MenuAdapterPopup(ActionBarGroupSlot source, Cleanup2MenuAdapterTarget target,
            AbilityData original, Cleanup2TooltipInputs inputs, Cleanup2NativeDefaultScope mode, Stopwatch watch,
            NativeIconScreenEvidence screens, string label, bool capture, JArray cases)
        {
            Cleanup2MenuAdapterBoundary(mode, watch);
            var popup = (ActionBarSpellsGroup)typeof(ActionBarGroupSlot).GetField("SubGroup", Cleanup2Fields).GetValue(source);
            if (popup == null || !popup.gameObject.activeInHierarchy) throw new InvalidOperationException("Actual adapter popup absent: " + label);
            var rows = popup.GetComponentsInChildren<ActionBarSlot>(true).Where(value => value.gameObject.activeInHierarchy &&
                (value.MechanicSlot?.GetContentData() as AbilityData)?.ConvertedFrom?.Blueprint == target.Parent).ToArray();
            var matches = rows.Where(value => ReferenceEquals((value.MechanicSlot.GetContentData() as AbilityData)?.Blueprint, target.Child)).ToArray();
            if (matches.Length != 1) throw new InvalidOperationException("Actual Horse child count=" + matches.Length + ":" + label);
            var row = matches[0]; var data = (AbilityData)row.MechanicSlot.GetContentData();
            var scroll = Cleanup2AcquirePopupSurface(source, popup, target.Parent, rows, label);
            var position = scroll.IsStationary ? Vector2.zero : scroll.Position;
            var velocity = scroll.IsStationary ? Vector2.zero : scroll.Velocity;
            var anchored = scroll.Content.anchoredPosition;
            bool horizontal = scroll.Horizontal, vertical = scroll.Vertical;
            Func<bool> owns = () => mode.Current && watch.Elapsed.TotalSeconds <= 240 && Cleanup2NoNarrativeOverlay() && inputs.Unchanged() &&
                Game.Instance.SelectedAbilityHandler.Ability == null &&
                ReferenceEquals(source.MechanicSlot?.GetContentData(), original) && popup.gameObject.activeInHierarchy &&
                ReferenceEquals(row.MechanicSlot.GetContentData(), data) && ReferenceEquals(data.ConvertedFrom, original) &&
                ReferenceEquals(data.Spellbook, original.Spellbook) && scroll.Active && scroll.Horizontal == horizontal && scroll.Vertical == vertical;
            try
            {
                foreach (int frame in Cleanup2RevealPopupRow(scroll, row, owns)) yield return frame;
                string expected = SummonChoicePresentationPolicy.For("horse", target.Spec.Multiplicity).Label;
                Image capturedImage = row.Icon; Sprite capturedSprite = capturedImage.sprite, capturedEffective = capturedImage.overrideSprite;
                Texture capturedTexture = capturedImage.mainTexture;
                var captureFrames = new JArray();
                Func<bool> ownsImage = () => owns() && ReferenceEquals(row.Icon, capturedImage) &&
                    ReferenceEquals(capturedImage.sprite, capturedSprite) && ReferenceEquals(capturedImage.overrideSprite, capturedEffective) &&
                    ReferenceEquals(capturedImage.mainTexture, capturedTexture) && Cleanup2FullyVisible(scroll, row);
                JObject renderedHorse = Cleanup2MenuHorseEffectiveImage(row, data, ownsImage);
                int rowObservationFrame = Time.frameCount;
                var evidence = new JObject { ["surface"] = label, ["rowObservationFrame"] = rowObservationFrame,
                    ["sourceMechanism"] = source.MechanicSlot.GetType().FullName,
                    ["source"] = original.Blueprint.AssetGuid, ["child"] = data.Blueprint.AssetGuid,
                    ["book"] = data.Spellbook?.Blueprint.AssetGuid, ["sourceDataIdentity"] = System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(original),
                    ["childDataIdentity"] = System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(data),
                    ["sourceLinkExact"] = ReferenceEquals(data.ConvertedFrom, original), ["nativeTitle"] = data.Name,
                    ["expectedTitle"] = expected, ["renderedText"] = new JArray(row.GetComponentsInChildren<TextMeshProUGUI>(true).Select(value => value.text)),
                    ["iconExact"] = ReferenceEquals(row.Icon.sprite, target.Child.Icon), ["fullyVisible"] = Cleanup2FullyVisible(scroll, row),
                    ["nativeAvailability"] = data.GetAvailableForCastCount(), ["nativeAction"] = data.RuntimeActionType.ToString(),
                    ["nativeFullRound"] = data.RequireFullRoundAction, ["dataLinks"] = DescribeExpandedSummoningAbilityData(data),
                    ["effectiveHorseImage"] = renderedHorse };
                bool pass = owns() && Cleanup2FullyVisible(scroll, row) && data.Name == expected && target.Child.Name == expected &&
                    ReferenceEquals(row.Icon.sprite, target.Child.Icon) && (bool)renderedHorse["passed"];
                Cleanup2Check("menu-adapter-row-" + label, pass, evidence,
                    "The actual native prepared/known/pinned source produces one exact Horse quantity row with approved decoded pixels in its effective sprite/main texture and unchanged source/book/availability.");
                // A mismatched but still positively owned row must remain in
                // the original framebuffer evidence before this case refuses.
                if (capture || !pass)
                {
                    Func<bool> captureOwner = () => {
                        bool current = ownsImage();
                        if (current && (captureFrames.Count == 0 || (int)captureFrames.Last["frame"] != Time.frameCount))
                        {
                            // Capture's own settlement occurs after the row
                            // assertion. Keep the actual subsequent frame's
                            // geometry rather than attach earlier coordinates
                            // to a later native framebuffer.
                            captureFrames.Add(Cleanup2MenuHorseFrame(row, data));
                            if (captureFrames.Count > 32) captureFrames.RemoveAt(0);
                        }
                        return current;
                    };
                    try { foreach (int frame in screens.Capture("menu-adapter-" + label, evidence, captureOwner)) yield return frame; }
                    finally
                    {
                        Cleanup2Check("menu-adapter-image-capture-" + label, ownsImage(), new JObject {
                            ["assertionFrame"] = rowObservationFrame, ["capturedNativeFrames"] = captureFrames,
                            ["currentImageHierarchy"] = ownsImage() ? Cleanup2MenuHorseImageHierarchy(row) : JValue.CreateNull(),
                            ["framebufferPaintingQualified"] = false,
                            ["limit"] = "Native framebuffer must be inspected at these capture-frame rectangles. Image identities and decoded CPU pixels alone do not prove that the painted Horse was drawn." },
                            "The exact source row/image remains owned through normal framebuffer capture; capture-frame geometry and native foreground/background images are retained without changing their presentation.");
                    }
                }
                if (!pass) throw new InvalidOperationException("Native adapter row failed: " + label);
                if (cases != null) cases.Add(evidence);
            }
            finally
            {
                if (!scroll.Active || scroll.Horizontal != horizontal || scroll.Vertical != vertical)
                    throw new InvalidOperationException("Foreign adapter scroll/axes; restore refused.");
                if (!scroll.IsStationary)
                { scroll.Velocity = Vector2.zero; Cleanup2SetPopupPosition(scroll, position); scroll.Content.anchoredPosition = anchored; scroll.Velocity = velocity; }
                if (!scroll.Content.anchoredPosition.Equals(anchored) || (!scroll.IsStationary && !scroll.Velocity.Equals(velocity)))
                    throw new InvalidOperationException("Exact adapter scroll restoration failed.");
            }
        }

        private JObject Cleanup2MenuHorseEffectiveImage(ActionBarSlot row, AbilityData data, Func<bool> owns)
        {
            const string approvedPngSha256 = "3bf6ac8d8c7b08869bf262c4018937d5aa6b72cf3e0b809efec916c6e0cf67c1";
            var result = new JObject { ["passed"] = false, ["approvedPngSha256"] = approvedPngSha256,
                ["pixelEncoding"] = "GetPixels32 bottom-left row order; explicit R,G,B,A bytes; SHA256; no image export",
                ["nativeImageContract"] = "Installed UnityEngine.UI Image.overrideSprite getter0600027C returns activeSprite; activeSprite0600027E prefers m_OverrideSprite; mainTexture06000294 uses activeSprite.texture. sprite alone is insufficient.",
                ["foreignTexturesRead"] = false, ["uiOrTextureModified"] = false };
            Texture2D decoded = null;
            try
            {
                if (owns == null || !owns() || row == null || data == null || row.Icon == null ||
                    !ReferenceEquals(row.MechanicSlot?.GetContentData(), data))
                    throw new InvalidOperationException("Exact current request-owned Horse row required before pixel observation.");
                Image image = row.Icon;
                Sprite cached = ExpandedSummoningProjectIcons.Require("horse");
                Texture2D ownedTexture = cached.texture;
                Sprite sprite = image.sprite, effective = image.overrideSprite;
                Texture main = image.mainTexture;
                Func<UnityEngine.Object, JToken> identity = value => value == null ? JValue.CreateNull() : (JToken)new JObject {
                    ["instance"] = value.GetInstanceID(), ["name"] = value.name, ["type"] = value.GetType().FullName };
                Func<Sprite, JToken> describeSprite = value => value == null ? JValue.CreateNull() : (JToken)new JObject {
                    ["object"] = identity(value), ["texture"] = identity(value.texture),
                    ["rect"] = new JArray(value.rect.x, value.rect.y, value.rect.width, value.rect.height),
                    ["isCachedHorse"] = ReferenceEquals(value, cached), ["textureIsOwnedHorse"] = ReferenceEquals(value.texture, ownedTexture) };
                result["rowInstance"] = row.GetInstanceID(); result["rowObject"] = row.name;
                result["rowDataIdentity"] = System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(data);
                result["childGuid"] = data.Blueprint.AssetGuid; result["sourceGuid"] = data.ConvertedFrom?.Blueprint.AssetGuid;
                result["image"] = identity(image); result["childIcon"] = describeSprite(data.Icon);
                result["imageHierarchy"] = Cleanup2MenuHorseImageHierarchy(row);
                result["sprite"] = describeSprite(sprite); result["effectiveOverrideSprite"] = describeSprite(effective);
                result["mainTexture"] = identity(main); result["cachedHorseSprite"] = describeSprite(cached);
                result["imageColor"] = new JArray(image.color.r, image.color.g, image.color.b, image.color.a);
                result["canvasAlpha"] = image.canvasRenderer.GetAlpha(); result["canvasCull"] = image.canvasRenderer.cull;
                result["imageFullyVisible"] = Cleanup2TooltipGraphicVisible(image);
                var canvas = image.canvas.rootCanvas;
                Camera camera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
                var corners = new Vector3[4]; image.rectTransform.GetWorldCorners(corners);
                result["imageScreenCorners"] = new JArray(corners.Select(value => {
                    Vector2 point = RectTransformUtility.WorldToScreenPoint(camera, value);
                    return new JObject { ["x"] = point.x, ["y"] = point.y }; }));
                string path = Path.Combine(_context.ModEntry.Path, "assets", "icons", "expanded-summoning", "horse.png");
                byte[] png = File.ReadAllBytes(path);
                result["installedPngPath"] = path; result["installedPngBytes"] = png.Length;
                string pngHash = Cleanup2HorseByteSha256(png); result["installedPngSha256"] = pngHash;
                if (pngHash != approvedPngSha256) throw new InvalidOperationException("Installed Horse PNG differs from the approved painting.");
                decoded = new Texture2D(2, 2, TextureFormat.ARGB32, false);
                Type conversion = Type.GetType("UnityEngine.ImageConversion, UnityEngine.ImageConversionModule", false);
                MethodInfo load = conversion?.GetMethod("LoadImage", BindingFlags.Public | BindingFlags.Static, null,
                    new[] { typeof(Texture2D), typeof(byte[]), typeof(bool) }, null);
                if (load == null || !(bool)load.Invoke(null, new object[] { decoded, png, false }) || decoded.width != 128 || decoded.height != 128)
                    throw new InvalidOperationException("Exact installed PNG decoding contract unavailable.");
                string approvedPixels = Cleanup2HorseRgbaSha256(decoded);
                result["approvedDecodedRgbaSha256"] = approvedPixels;
                var textures = new JArray();
                foreach (Texture texture in new Texture[] { data.Icon?.texture, sprite?.texture, effective?.texture, main }.Where(value => value != null).Distinct())
                {
                    bool owned = ReferenceEquals(texture, ownedTexture);
                    var receipt = new JObject { ["texture"] = identity(texture), ["positiveProjectHorseOwnership"] = owned,
                        ["pixelReadDisposition"] = owned ? "read-only exact cached project Horse texture" : "refused: texture is outside exact project Horse ownership" };
                    if (owned) receipt["rgbaSha256"] = Cleanup2HorseRgbaSha256(ownedTexture);
                    textures.Add(receipt);
                }
                result["actualTexturePixels"] = textures;
                string loadedPixels = Cleanup2HorseRgbaSha256(ownedTexture);
                result["cachedHorseRgbaSha256"] = loadedPixels;
                bool unchanged = owns() && ReferenceEquals(row.MechanicSlot?.GetContentData(), data) &&
                    ReferenceEquals(image, row.Icon) && ReferenceEquals(image.sprite, sprite) &&
                    ReferenceEquals(image.overrideSprite, effective) && ReferenceEquals(image.mainTexture, main) &&
                    ReferenceEquals(cached, ExpandedSummoningProjectIcons.Require("horse"));
                bool exact = unchanged && ReferenceEquals(data.Icon, cached) && ReferenceEquals(sprite, cached) &&
                    ReferenceEquals(effective, cached) && ReferenceEquals(main, ownedTexture) && cached.rect == new Rect(0, 0, 128, 128) &&
                    ownedTexture.width == 128 && ownedTexture.height == 128 && loadedPixels == approvedPixels && Cleanup2TooltipGraphicVisible(image);
                result["identitiesUnchangedDuringRead"] = unchanged; result["decodedPixelsMatchApprovedPainting"] = loadedPixels == approvedPixels;
                result["passed"] = exact;
                result["limit"] = "Decoded project texture/effective native Image identity proof; actual framebuffer pixels still require inspection. No override is cleared, no native/foreign texture is read or exported.";
            }
            catch (Exception error) { result["error"] = error.GetType().FullName + ":" + error.Message; }
            finally { if (decoded != null) UnityEngine.Object.Destroy(decoded); }
            return result;
        }

        private static JObject Cleanup2MenuHorseFrame(ActionBarSlot row, AbilityData data)
        {
            return new JObject { ["frame"] = Time.frameCount,
                ["rowInstance"] = row.GetInstanceID(), ["child"] = data.Blueprint.AssetGuid,
                ["rowScreenCorners"] = Cleanup2MenuHorseCorners(row.transform as RectTransform),
                ["iconScreenCorners"] = Cleanup2MenuHorseCorners(row.Icon.rectTransform),
                ["iconDepth"] = row.Icon.depth, ["iconCanvasDepth"] = row.Icon.canvasRenderer.absoluteDepth,
                ["iconInstance"] = row.Icon.GetInstanceID(),
                ["spriteInstance"] = row.Icon.sprite == null ? 0 : row.Icon.sprite.GetInstanceID(),
                ["effectiveSpriteInstance"] = row.Icon.overrideSprite == null ? 0 : row.Icon.overrideSprite.GetInstanceID(),
                ["mainTextureInstance"] = row.Icon.mainTexture == null ? 0 : row.Icon.mainTexture.GetInstanceID() };
        }

        private static JArray Cleanup2MenuHorseCorners(RectTransform rect)
        {
            if (rect == null) return new JArray();
            var canvas = rect.GetComponentInParent<Canvas>()?.rootCanvas;
            Camera camera = canvas == null || canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
            var corners = new Vector3[4]; rect.GetWorldCorners(corners);
            return new JArray(corners.Select(value => {
                Vector2 point = RectTransformUtility.WorldToScreenPoint(camera, value);
                return new JObject { ["x"] = point.x, ["y"] = point.y }; }));
        }

        private static JToken Cleanup2MenuHorseImageHierarchy(ActionBarSlot row)
        {
            // The native Icon field is named BackIcon in the prefab. Its
            // sprite does not establish which foreground graphic is drawn.
            // Retain exact native image layers and renderer-bound materials;
            // no native/foreign texture pixels or assets are read/exported.
            Func<UnityEngine.Object, JToken> identity = value => value == null ? JValue.CreateNull() : (JToken)new JObject {
                ["instance"] = value.GetInstanceID(), ["name"] = value.name, ["type"] = value.GetType().FullName };
            var fields = typeof(ActionBarSlot).GetFields(Cleanup2Fields).Where(value => value.FieldType == typeof(Image)).ToArray();
            var images = row.GetComponentsInChildren<Image>(true);
            return new JObject { ["imageCount"] = images.Length, ["imageLimit"] = 32,
                ["iconIsDescendantOfExactRow"] = row.Icon != null && row.Icon.transform.IsChildOf(row.transform),
                ["nativeImages"] = new JArray(images.Take(32).Select(image => {
                    var chain = new List<string>(); Transform node = image.transform;
                    for (int depth = 0; node != null && depth < 12; depth++, node = node.parent)
                    {chain.Add(node.name + "#" + node.GetInstanceID() + "[" + node.GetSiblingIndex() + "]"); if (ReferenceEquals(node, row.transform)) break;}
                    chain.Reverse();
                    var renderer = image.canvasRenderer;
                    var materials = new JArray();
                    for (int index = 0; index < Math.Min(renderer.materialCount, 4); index++)
                    {
                        var material = renderer.GetMaterial(index);
                        materials.Add(new JObject { ["index"] = index, ["material"] = identity(material),
                            ["shader"] = identity(material?.shader),
                            ["materialMainTexture"] = identity(material != null && material.HasProperty("_MainTex") ? material.GetTexture("_MainTex") : null),
                            ["limit"] = "Renderer-bound material identity only; CanvasRenderer's separate bound texture has no installed public getter." });
                    }
                    return new JObject { ["image"] = identity(image), ["path"] = string.Join("/", chain.ToArray()),
                        ["nativeFieldRoles"] = new JArray(fields.Where(field => ReferenceEquals(field.GetValue(row), image)).Select(field => field.Name)),
                        ["activeSelf"] = image.gameObject.activeSelf, ["activeInHierarchy"] = image.gameObject.activeInHierarchy,
                        ["enabled"] = image.enabled, ["depth"] = image.depth, ["absoluteCanvasDepth"] = renderer.absoluteDepth,
                        ["relativeCanvasDepth"] = renderer.relativeDepth, ["color"] = new JArray(image.color.r, image.color.g, image.color.b, image.color.a),
                        ["inheritedAlpha"] = renderer.GetInheritedAlpha(), ["culled"] = renderer.cull,
                        ["sprite"] = identity(image.sprite), ["spriteTexture"] = identity(image.sprite?.texture),
                        ["effectiveSprite"] = identity(image.overrideSprite), ["effectiveSpriteTexture"] = identity(image.overrideSprite?.texture),
                        ["mainTexture"] = identity(image.mainTexture), ["screenCorners"] = Cleanup2MenuHorseCorners(image.rectTransform),
                        ["rendererMaterialCount"] = renderer.materialCount, ["rendererMaterials"] = materials };
                })) };
        }

        private static string Cleanup2HorseRgbaSha256(Texture2D texture)
        {
            if (texture == null || texture.width != 128 || texture.height != 128)
                throw new InvalidOperationException("Exact 128px project-owned Horse texture required.");
            Color32[] pixels = texture.GetPixels32();
            if (pixels.Length != 128 * 128) throw new InvalidOperationException("Unexpected Horse pixel count.");
            var bytes = new byte[pixels.Length * 4];
            for (int i = 0; i < pixels.Length; i++) {
                bytes[i * 4] = pixels[i].r; bytes[i * 4 + 1] = pixels[i].g;
                bytes[i * 4 + 2] = pixels[i].b; bytes[i * 4 + 3] = pixels[i].a;
            }
            return Cleanup2HorseByteSha256(bytes);
        }

        private static string Cleanup2HorseByteSha256(byte[] bytes)
        {
            using (SHA256 sha = SHA256.Create())
                return BitConverter.ToString(sha.ComputeHash(bytes)).Replace("-", "").ToLowerInvariant();
        }

        private IEnumerable<int> Cleanup2MenuAdapterPin(ExpandedSummoningCorrectionFixture fixture, Spellbook book,
            ActionBarGroupElement spellGroup, Cleanup2MenuAdapterTarget target, Cleanup2NativeDefaultScope mode,
            Stopwatch watch, NativeIconScreenEvidence screens)
        {
            Cleanup2MenuAdapterBoundary(mode, watch);
            var levels = (ActionBarSubGroupLevels)typeof(ActionBarGroupElement).GetField("m_Levels", Cleanup2Fields).GetValue(spellGroup);
            levels.OnClick(target.Spec.ParentTier);
            for (int frame = 0; frame < 8; frame++) yield return 0;
            var source = spellGroup.GetComponentsInChildren<ActionBarGroupSlot>(true).Single(value => {
                var data = value.MechanicSlot?.GetContentData() as AbilityData;
                return value.gameObject.activeInHierarchy && ReferenceEquals(data?.Blueprint, target.Parent) && ReferenceEquals(data?.Spellbook, book);
            });
            var mechanism = source.MechanicSlot; var original = (AbilityData)mechanism.GetContentData();
            var nativeIndex = typeof(ActionBarIndexSlot).GetField("m_Index", Cleanup2Fields);
            var settings = fixture.Caster.UISettings; var slots = settings.Slots;
            var destination = ActionBarManager.Instance.GetComponentsInChildren<ActionBarIndexSlot>(true).FirstOrDefault(value =>
                value.gameObject.activeInHierarchy && ReferenceEquals(Cleanup2MenuAdapterSelected(value), fixture.Caster) && nativeIndex != null &&
                (int)nativeIndex.GetValue(value) >= 0 && (int)nativeIndex.GetValue(value) < slots.Length &&
                slots[(int)nativeIndex.GetValue(value)] is MechanicActionBarSlotEmpty);
            if (destination == null) throw new InvalidOperationException("No native visible empty index slot owned by disposable caster.");
            int index = (int)nativeIndex.GetValue(destination); var previous = slots[index];
            var inputs = new Cleanup2TooltipInputs(fixture.Caster); bool requested = false;
            try
            {
                // Actual native pin handler, with the actual native source
                // mechanism; no hand-built mechanism enters serialized slots.
                requested = true; destination.TryToSet(mechanism);
                for (int frame = 0; frame < 8; frame++) yield return 0;
                if (!ReferenceEquals(settings.Slots, slots) || !ReferenceEquals(slots[index], mechanism) ||
                    !ReferenceEquals(destination.MechanicSlot, mechanism)) throw new InvalidOperationException("Native source pin did not retain exact mechanism.");
                spellGroup.Fill(fixture.Caster);
                for (int frame = 0; frame < 8; frame++) yield return 0;
                foreach (int frame in Cleanup2AwaitStationaryNativeHud(destination, "pinned-parent-" + target.Label)) yield return frame;
                destination.OnToggleGroupClick();
                for (int frame = 0; frame < 8; frame++) yield return 0;
                foreach (int frame in Cleanup2MenuAdapterPopup(destination, target, original, inputs, mode, watch, screens,
                    "pinned-parent-" + target.Label, false, null)) yield return frame;
                Cleanup2Check("menu-adapter-existing-native-pin", inputs.Unchanged() && ReferenceEquals(slots[index], mechanism),
                    new JObject { ["index"] = index, ["source"] = original.Blueprint.AssetGuid, ["mechanismExactAfterTabRebuild"] = ReferenceEquals(slots[index], mechanism),
                        ["persistedSave"] = false, ["scope"] = "Native source pinned before native tab rebuild; exact runtime object survival only, no save write/hydration or cast." },
                    "A native source pin survives native tab rebuild and opens its original quantity menu without changing preparation/resources.");
            }
            finally
            {
                if (requested)
                {
                    if (!ReferenceEquals(settings.Slots, slots) || !ReferenceEquals(Cleanup2MenuAdapterSelected(destination), fixture.Caster) ||
                        (!ReferenceEquals(slots[index], mechanism) && !ReferenceEquals(slots[index], previous)) ||
                        (!ReferenceEquals(destination.MechanicSlot, mechanism) && !ReferenceEquals(destination.MechanicSlot, previous)))
                        throw new InvalidOperationException("Foreign native pin replacement; not overwritten.");
                    if (ReferenceEquals(destination.MechanicSlot, mechanism)) Cleanup2MenuAdapterHide(destination, original);
                    if (ReferenceEquals(slots[index], mechanism)) settings.SetSlot(previous, index);
                    destination.Set(fixture.Caster);
                    if (!ReferenceEquals(slots[index], previous)) throw new InvalidOperationException("Exact original pin restoration failed.");
                }
            }
        }

        private IEnumerable<int> Cleanup2MenuAdapterDruid(ExpandedSummoningCorrectionFixture fixture,
            TeleportResourceFixtureOwner books, ActionBarGroupElement spellGroup, Cleanup2NativeDefaultScope mode,
            Stopwatch watch, NativeIconScreenEvidence screens, Action<Action> receipt)
        {
            Cleanup2MenuAdapterBoundary(mode, watch);
            var druid = BlueprintLibraryLookup.RequireExact<BlueprintCharacterClass>(BlueprintBootstrap.Library,
                "610d836f3a3a9ed42a4349b62f002e96", "native Druid conversion class");
            var book = books.AddBook(druid.Spellbook);
            var progressionFeatures = druid.Progression.LevelEntries.SelectMany(value => value.Features).Where(value => value != null).Distinct().ToArray();
            var features = progressionFeatures.OfType<BlueprintFeature>().Where(value => DruidSummonConversionVariantPatch.ExactNativeFeature(value, druid)).ToArray();
            bool featureGrantAdmitted = features.Length == 1 && !fixture.Caster.Descriptor.HasFact(features[0]);
            var featureIntake = new JObject { ["class"] = druid.AssetGuid, ["progression"] = druid.Progression.AssetGuid,
                ["book"] = book.Blueprint.AssetGuid, ["directProgressionFeatureCount"] = progressionFeatures.Length,
                ["exactConversionFeatureCount"] = features.Length,
                ["exactCandidateAlreadyGranted"] = features.Length == 1 && fixture.Caster.Descriptor.HasFact(features[0]),
                ["exactCandidateGuids"] = new JArray(features.Select(value => value.AssetGuid)),
                ["directProgressionFeatureLimit"] = 64,
                ["directProgressionFeatures"] = new JArray(progressionFeatures.Take(64).Select(value => new JObject {
                    ["guid"] = value.AssetGuid, ["name"] = value.name, ["alreadyGranted"] = fixture.Caster.Descriptor.HasFact(value),
                    ["levels"] = new JArray(druid.Progression.LevelEntries.Where(level => level.Features.Any(item => ReferenceEquals(item, value))).Select(level => level.Level)),
                    ["componentTypes"] = new JArray((value.ComponentsArray ?? new BlueprintComponent[0]).Take(24).Select(component => component?.GetType().FullName)),
                    ["nativeConversionComponents"] = new JArray((value.ComponentsArray ?? new BlueprintComponent[0]).OfType<SpontaneousSpellConversion>().Take(8).Select(component => new JObject {
                        ["characterClass"] = component.CharacterClass?.AssetGuid,
                        ["exactDruidClassReference"] = ReferenceEquals(component.CharacterClass, druid),
                        ["spellsByLevelLength"] = component.SpellsByLevel?.Length ?? -1,
                        ["spellsByLevel"] = new JArray((component.SpellsByLevel ?? new BlueprintAbility[0]).Take(16).Select((spell, index) => new JObject {
                            ["index"] = index, ["guid"] = spell?.AssetGuid, ["name"] = spell?.name,
                            ["expectedCanonicalAtNativeLevel"] = index >= 1 && index <= 9 ? ExpandedSummoningInventoryObserver.CanonicalParentGuids[9 + index - 1] : null })) })) })),
                ["scope"] = "Only the exact current Druid progression's direct features/components are observed. No global feature census, nested grant inference, replacement feature, or component is authored.",
                ["nativeContract"] = "SpontaneousSpellConversion.OnTurnOn0600260B demands CharacterClass's book and adds its exact SpellsByLevel array; OnTurnOff0600260C removes that exact array. Exact feature b296531ffe013c8499ad712f8ae97f6b has three catalog quantity arrays: canonical SNA I, then released native Single/d3/d4plus1 source identities. These arrays and all blueprint references remain unchanged." };
            Cleanup2Check("menu-adapter-druid-conversion-feature-intake", featureGrantAdmitted, featureIntake,
                "Before any feature grant, retain the current native Druid feature/component/class/level-array identities and whether the exact candidate already belongs to the disposable caster.");
            if (!featureGrantAdmitted)
                throw new InvalidOperationException("Exact unique native Druid SNA conversion feature not available for owned fixture grant; candidates=" +
                    features.Length + ";alreadyGranted=" + (features.Length == 1 && fixture.Caster.Descriptor.HasFact(features[0])) + ". Inspect bounded current feature intake.");
            var feature = features[0]; var fact = fixture.Caster.Descriptor.AddFact(feature);
            receipt(() => {
                if (!ReferenceEquals(fixture.Caster.Descriptor.GetFact(feature), fact))
                    throw new InvalidOperationException("Native conversion fact changed; foreign feature is not removed.");
                fixture.Caster.Descriptor.RemoveFact(fact);
            });
            var parent = BlueprintLibraryLookup.RequireExact<BlueprintAbility>(BlueprintBootstrap.Library,
                ExpandedSummoningInventoryObserver.CanonicalParentGuids[10], "canonical SNA II conversion identity");
            var ordinary = book.Blueprint.SpellList.GetSpells(2).Where(value => value != null && !ReferenceEquals(value, parent) &&
                (value.Variants == null || value.Variants.Length == 0)).OrderBy(value => value.AssetGuid, StringComparer.Ordinal).First();
            var prepared = PrepareExpandedSummoningPlayerPathSpell(book, ordinary, ordinary, 2); book.Rest();
            spellGroup.Fill(fixture.Caster);
            var levels = (ActionBarSubGroupLevels)typeof(ActionBarGroupElement).GetField("m_Levels", Cleanup2Fields).GetValue(spellGroup);
            levels.OnClick(2);
            for (int frame = 0; frame < 10; frame++) yield return 0;
            var source = spellGroup.GetComponentsInChildren<ActionBarGroupSlot>(true).Single(value => value.gameObject.activeInHierarchy &&
                ReferenceEquals(value.MechanicSlot?.GetContentData(), prepared));
            var conversions = source.MechanicSlot.GetConvertedAbilityData().ToArray();
            var patches = _context.Harmony.GetPatchInfo(typeof(MechanicActionBarSlotMemorizedSpell).GetMethod("GetConvertedAbilityData"));
            var variantPatches = _context.Harmony.GetPatchInfo(typeof(MechanicActionBarSlotSpontaneusSpell).GetMethod("GetConvertedAbilityData"));
            var membershipPatches = _context.Harmony.GetPatchInfo(typeof(AbilityData).GetMethod("HasVariant"));
            string factoryOutcome = DruidSummonConversionVariantPatch.Outcome;
            var evidence = new JObject { ["class"] = druid.AssetGuid, ["book"] = book.Blueprint.AssetGuid, ["nativeFeature"] = feature.AssetGuid,
                ["source"] = prepared.Blueprint.AssetGuid, ["sourceSlotIdentity"] = System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(prepared.ParamSpellSlot),
                ["bookByBlueprintExact"] = ReferenceEquals(fixture.Caster.Descriptor.GetSpellbook(druid.Spellbook), book),
                ["bookByClassExact"] = ReferenceEquals(fixture.Caster.Descriptor.GetSpellbook(druid), book),
                ["druidClassDataPresent"] = fixture.Caster.Descriptor.Progression.GetClassData(druid) != null,
                ["nativeBookAuthority"] = "SpontaneousSpellConversion0600260B uses DemandSpellbook(class)06001F0C, whose no-ClassData branch uses the exact default book blueprint. GetSpellbook(class)06001F0B instead returns null. Production checks the already-owned exact default book, real native feature, native conversion list and actual preparation; it adds no class or book.",
                ["factoryModuleMvid"] = typeof(MechanicActionBarSlotMemorizedSpell).Module.ModuleVersionId.ToString(),
                ["factoryToken"] = typeof(MechanicActionBarSlotMemorizedSpell).GetMethod("GetConvertedAbilityData").MetadataToken,
                ["patchOwners"] = new JArray(patches == null ? new string[0] : patches.Prefixes.Concat(patches.Postfixes).Concat(patches.Transpilers).Select(value => value.owner).Distinct().ToArray()),
                ["variantFactoryPatchOwners"] = new JArray(variantPatches == null ? new string[0] : variantPatches.Prefixes.Concat(variantPatches.Postfixes).Concat(variantPatches.Transpilers).Select(value => value.owner).Distinct().ToArray()),
                ["variantMembershipPatchOwners"] = new JArray(membershipPatches == null ? new string[0] : membershipPatches.Prefixes.Concat(membershipPatches.Postfixes).Concat(membershipPatches.Transpilers).Select(value => value.owner).Distinct().ToArray()),
                ["conversionAdapterAfterFactory"] = factoryOutcome,
                ["conversions"] = new JArray(conversions.Select(value => new JObject { ["blueprint"] = value.Blueprint.AssetGuid,
                    ["sourceExact"] = Cleanup2DruidSourceChain(value, prepared, parent), ["bookExact"] = ReferenceEquals(value.Spellbook, book),
                    ["immediateSourceIsPreparation"] = ReferenceEquals(value.ConvertedFrom, prepared),
                    ["quantitySourceIsPreparation"] = ReferenceEquals(value.ConvertedFrom?.ConvertedFrom, prepared),
                    ["nativeQuantityRecognizesChild"] = value.ConvertedFrom != null && value.ConvertedFrom.HasVariant(value),
                    ["metamagicMatchesImmediateSource"] = value.ConvertedFrom != null && ReferenceEquals(value.MetamagicData, value.ConvertedFrom.MetamagicData),
                    ["factMatchesImmediateSource"] = value.ConvertedFrom != null && ReferenceEquals(value.Fact, value.ConvertedFrom.Fact),
                    ["nativeQuantityConversion"] = value.ConvertedFrom?.Blueprint.AssetGuid,
                    ["variants"] = value.Blueprint.Variants?.Length ?? 0, ["dataLinks"] = DescribeExpandedSummoningAbilityData(value) })),
                ["fixtureAccess"] = "Real native Druid book and exact class-progression conversion feature granted to disposable caster. Not a naturally levelled Druid or saved character." };
            var inputs = new Cleanup2TooltipInputs(fixture.Caster); AbilityData selected = null; bool castObserved = false;
            Cleanup2SourceDebit debit = null;
            try
            {
                foreach (int frame in Cleanup2AwaitStationaryNativeHud(source, "druid-conversion")) yield return frame;
                source.OnToggleGroupClick();
                for (int frame = 0; frame < 8; frame++) yield return 0;
                var popup = (ActionBarSpellsGroup)typeof(ActionBarGroupSlot).GetField("SubGroup", Cleanup2Fields).GetValue(source);
                if (popup == null || !popup.gameObject.activeInHierarchy) throw new InvalidOperationException("Actual Druid conversion popup absent.");
                var rows = popup.GetComponentsInChildren<ActionBarSlot>(true).Where(value => value.gameObject.activeInHierarchy &&
                    Cleanup2DruidSourceChain(value.MechanicSlot?.GetContentData() as AbilityData, prepared, parent)).ToArray();
                var exact = rows.Where(value => ReferenceEquals((value.MechanicSlot.GetContentData() as AbilityData)?.Blueprint, parent)).ToArray();
                var variants = parent.Variants ?? new BlueprintAbility[0];
                bool flatFactory = variants.Length != 0 && variants.All(variant => conversions.Count(value =>
                    ReferenceEquals(value.Blueprint, variant) && Cleanup2DruidSourceChain(value, prepared, parent)) == 1);
                bool flatRows = flatFactory && variants.All(variant => rows.Count(value =>
                    ReferenceEquals((value.MechanicSlot.GetContentData() as AbilityData)?.Blueprint, variant)) == 1);
                var horseSpec = ExpandedSummoningCatalog.GenerateVariants(SummonFamily.NaturesAlly).Single(value =>
                    value.Creature.Key == "horse" && value.ParentTier == 2 && value.Multiplicity == SummonMultiplicity.One);
                var horse = ExpandedSummoningRoot(fixture.Blueprints, horseSpec);
                var horseRows = rows.Where(value => ReferenceEquals((value.MechanicSlot.GetContentData() as AbilityData)?.Blueprint, horse)).ToArray();
                evidence["popupRows"] = new JArray(rows.Select(value => new JObject { ["rowType"] = value.GetType().FullName,
                    ["ability"] = ((AbilityData)value.MechanicSlot.GetContentData()).Blueprint.AssetGuid,
                    ["nativeTitle"] = ((AbilityData)value.MechanicSlot.GetContentData()).Name,
                    ["nativeGroupControl"] = value is ActionBarGroupSlot }));
                evidence["canonicalParentRows"] = exact.Length;
                evidence["allCanonicalVariantsInLiveFactory"] = flatFactory; evidence["allCanonicalVariantsInNativeRows"] = flatRows;
                evidence["horseRows"] = horseRows.Length;
                evidence["conversionAdapter"] = DruidSummonConversionVariantPatch.Outcome;
                evidence["nestedVariantRoute"] = flatRows ? "actual installed factory/widget expansion; native Horse selection/cast/debit required below" :
                    exact.Length == 1 && exact[0] is ActionBarGroupSlot ? "native-group-present; not navigated in this slice" :
                    "unqualified: parent-only native conversion row is not a nested group; no variant links fabricated";
                bool contract = flatRows && horseRows.Length == 1 && inputs.Unchanged();
                Cleanup2Check("menu-adapter-druid-native-conversion-contract", contract, evidence,
                    "The actual Druid prepared-slot factory must expose the complete canonical SNA II child set with original native source/book links; parent-only conversion does not qualify creature selection.");
                if (!contract) throw new InvalidOperationException("Installed native Druid conversion contract differs; inspect recorded factory/patch/rows.");
                Func<bool> owns = () => mode.Current && Cleanup2NoNarrativeOverlay() && inputs.Unchanged() &&
                    Game.Instance.SelectedAbilityHandler.Ability == null &&
                    ReferenceEquals(source.MechanicSlot?.GetContentData(), prepared) && popup.gameObject.activeInHierarchy;
                var clickedRow = horseRows.Single();
                selected = (AbilityData)clickedRow.MechanicSlot.GetContentData();
                string anchor = selected.TargetAnchor.ToString();
                var scroll = Cleanup2DruidPopupSurface(source, popup, prepared, selected, owns);
                Vector2 position = scroll.IsStationary ? Vector2.zero : scroll.Position;
                Vector2 velocity = scroll.IsStationary ? Vector2.zero : scroll.Velocity;
                Vector2 anchored = scroll.Content.anchoredPosition;
                try
                {
                    foreach (int frame in Cleanup2RevealPopupRow(scroll, clickedRow, owns)) yield return frame;
                    string title = SummonChoicePresentationPolicy.For("horse", SummonMultiplicity.One).Label;
                    bool visible = Cleanup2FullyVisible(scroll, clickedRow);
                    Image capturedImage = clickedRow.Icon; Sprite capturedSprite = capturedImage.sprite, capturedEffective = capturedImage.overrideSprite;
                    Texture capturedTexture = capturedImage.mainTexture;
                    Func<bool> ownsImage = () => owns() && ReferenceEquals(clickedRow.Icon, capturedImage) &&
                        ReferenceEquals(capturedImage.sprite, capturedSprite) && ReferenceEquals(capturedImage.overrideSprite, capturedEffective) &&
                        ReferenceEquals(capturedImage.mainTexture, capturedTexture) && Cleanup2FullyVisible(scroll, clickedRow);
                    JObject renderedHorse = Cleanup2MenuHorseEffectiveImage(clickedRow, selected, ownsImage);
                    bool rowExact = visible && selected.Name == title && horse.Name == title && ReferenceEquals(clickedRow.Icon.sprite, horse.Icon) &&
                        (bool)renderedHorse["passed"] && selected.TargetAnchor != AbilityTargetAnchor.Owner && !(clickedRow is ActionBarGroupSlot) && Cleanup2DruidSourceChain(selected, prepared, parent);
                    evidence["horseVisible"] = visible; evidence["horseTitle"] = selected.Name; evidence["expectedTitle"] = title;
                    evidence["horseIconExact"] = ReferenceEquals(clickedRow.Icon.sprite, horse.Icon); evidence["horseTargetAnchor"] = anchor;
                    evidence["retainedNativeQuantityConversion"] = selected.ConvertedFrom?.Blueprint.AssetGuid;
                    evidence["nativeQuantityConversionRetainedInChain"] = Cleanup2DruidSourceChain(selected, prepared, parent);
                    evidence["effectiveHorseImage"] = renderedHorse;
                    Cleanup2Check("menu-adapter-druid-native-horse-row", rowExact, evidence,
                        "The real Druid conversion popup exposes its exact Horse child with current label/icon, source chain and an actual visible native row.");
                    foreach (int frame in screens.Capture("menu-adapter-druid-native-conversion", evidence, ownsImage)) yield return frame;
                    if (!rowExact) throw new InvalidOperationException("Real Druid Horse row visibility/identity failed.");
                    debit = new Cleanup2SourceDebit(fixture.Caster);
                    foreach (int frame in Cleanup2MenuAdapterDruidCast(fixture, selected, prepared, parent, clickedRow, debit, mode, watch,
                        () => Cleanup2MenuAdapterHide(source, prepared))) yield return frame;
                    castObserved = true;
                }
                finally
                {
                    // A successful row activation closes this exact native
                    // popup through HideSubGroup; the existing layout owner
                    // restores its native hierarchy on that lifecycle. Restore
                    // scroll scalars here only if the popup remains ours/open.
                    if (popup.gameObject.activeInHierarchy)
                    {
                        if (!scroll.Active) throw new InvalidOperationException("Druid popup scroll owner changed before exact restoration.");
                        if (!scroll.IsStationary) { scroll.Velocity = Vector2.zero; Cleanup2SetPopupPosition(scroll, position); scroll.Content.anchoredPosition = anchored; scroll.Velocity = velocity; }
                    }
                }
            }
            finally
            {
                if (selected != null && ReferenceEquals(Game.Instance.SelectedAbilityHandler?.Ability, selected))
                    Game.Instance.SelectedAbilityHandler.DropAbility();
                Cleanup2MenuAdapterHide(source, prepared);
                bool preserved = castObserved && debit != null ? debit.ExactlyOne(prepared.ParamSpellSlot) : inputs.Unchanged();
                Cleanup2Check("menu-adapter-druid-source-debit-restoration", preserved,
                    castObserved && debit != null ? debit.Describe(prepared.ParamSpellSlot) : inputs.Describe(),
                    "The observed ordinary Druid cast spends only its original preparation; a failure before casting must preserve every original input.");
            }
        }

        private static bool Cleanup2DruidSourceChain(AbilityData child, AbilityData source, BlueprintAbility parent)
        {
            var receipt = DruidSummonConversionVariantPatch.For(child);
            return child != null && source != null && receipt != null && ReferenceEquals(receipt.Source, source) && ReferenceEquals(receipt.Parent, parent) &&
                ReferenceEquals(child.ConvertedFrom, receipt.Conversion) && ReferenceEquals(child.ConvertedFrom.Blueprint, receipt.NativeConversionBlueprint) &&
                ReferenceEquals(child.ConvertedFrom.ConvertedFrom, source) && ReferenceEquals(child.Spellbook, source.Spellbook) && ReferenceEquals(child.Caster, source.Caster) &&
                receipt.Conversion.HasVariant(child) && ReferenceEquals(child.MetamagicData, receipt.Conversion.MetamagicData) && ReferenceEquals(child.Fact, receipt.Conversion.Fact);
        }

        private NativeIconScreenEvidence.NativeRowScroll Cleanup2DruidPopupSurface(ActionBarGroupSlot source, ActionBarSpellsGroup popup,
            AbilityData prepared, AbilityData child, Func<bool> owns)
        {
            var receipt = DruidSummonConversionVariantPatch.For(child);
            ExpandedSummoningVariantMenuSnapshot snapshot;
            if (receipt == null || !ExpandedSummoningVariantMenuRuntime.TryGetSnapshot(popup, out snapshot) ||
                snapshot.ParentGuid != prepared.Blueprint.AssetGuid || !snapshot.ExactSourceSlotCaptured)
                throw new InvalidOperationException("Exact source-owned Druid conversion layout receipt required; unknown foreign layout is not adopted.");
            var native = (List<ActionBarSpontaneousConvertedSlot>)typeof(ActionBarSpellsGroup).GetField("m_Slots", Cleanup2Fields).GetValue(popup);
            var captured = native.ToArray(); var capturedData = captured.Select(value => value.MechanicSlot.GetContentData() as AbilityData).ToArray();
            Func<bool> proof = () => owns() && receipt.Current && ReferenceEquals(receipt.Source, prepared) &&
                ReferenceEquals(source.MechanicSlot, receipt.Mechanism) && ReferenceEquals(source.MechanicSlot?.GetContentData(), prepared) &&
                ReferenceEquals(typeof(ActionBarSpellsGroup).GetField("m_SourceSpell", Cleanup2Fields).GetValue(popup), prepared) &&
                ReferenceEquals(typeof(ActionBarSpellsGroup).GetField("m_Slots", Cleanup2Fields).GetValue(popup), native) && native.SequenceEqual(captured) &&
                native.Select(value => value.MechanicSlot.GetContentData() as AbilityData).SequenceEqual(capturedData) &&
                capturedData.SequenceEqual(receipt.ProjectedRows) && snapshot.SlotCount == captured.Length;
            if (!proof()) throw new InvalidOperationException("Native Druid popup source/row ownership is unverified.");
            var scrolls = popup.GetComponents<ScrollRect>().Where(value => value.enabled && value.gameObject.activeInHierarchy).ToArray();
            if (snapshot.ScrollingRequired)
            {
                if (!snapshot.ScrollingInstalled || scrolls.Length != 1 || scrolls[0].content == null || scrolls[0].viewport == null ||
                    !captured.All(value => value.transform.parent == scrolls[0].content)) throw new InvalidOperationException("Actual owned Druid popup viewport unavailable.");
                return new NativeIconScreenEvidence.NativeRowScroll(scrolls[0], proof);
            }
            if (scrolls.Length != 0 || snapshot.ScrollingInstalled || !captured.All(value => value.transform.parent == popup.transform))
                throw new InvalidOperationException("Native Druid stationary popup hierarchy changed.");
            return NativeIconScreenEvidence.NativeRowScroll.StationaryPopup((RectTransform)popup.transform, proof);
        }

        private static AbilityData _cleanup2DruidCastExpected;
        private static readonly List<RuleCastSpell> Cleanup2DruidCastRules = new List<RuleCastSpell>();
        private static void Cleanup2DruidCastRulePostfix(RuleCastSpell __instance)
        {
            ExpandedSummoningPlayerPathRuleCastPostfix(__instance);
            if (_cleanup2DruidCastExpected != null && __instance != null && Cleanup2DruidCastRules.Count < 8) Cleanup2DruidCastRules.Add(__instance);
        }
        private IEnumerable<int> Cleanup2MenuAdapterDruidCast(ExpandedSummoningCorrectionFixture fixture, AbilityData child, AbilityData source,
            BlueprintAbility parent, ActionBarSlot row, Cleanup2SourceDebit debit, Cleanup2NativeDefaultScope mode, Stopwatch phase, Action closeOwnPopup)
        {
            if (_expandedSummoningPlayerPathCaptureActive || _cleanup2DruidCastExpected != null || !Cleanup2DruidSourceChain(child, source, parent) ||
                !ReferenceEquals(row.MechanicSlot.GetContentData(), child) || child.TargetAnchor == AbilityTargetAnchor.Owner)
                throw new InvalidOperationException("Exclusive exact native Druid Horse cast route required.");
            var hooks = HarmonyInstance.Create("kmg.cleanup2.druid-conversion." + _request.RunId); UnitUseAbility command = null;
            bool paused = Game.Instance.IsPaused;
            try
            {
                const BindingFlags statics = BindingFlags.Static | BindingFlags.NonPublic;
                hooks.Patch(typeof(RuleCastSpell).GetMethod("OnTrigger"), null, new HarmonyMethod(typeof(RuntimeTestRunner).GetMethod("Cleanup2DruidCastRulePostfix", statics)), null);
                hooks.Patch(typeof(ContextActionSpawnMonster).GetMethod("RunAction"), new HarmonyMethod(typeof(RuntimeTestRunner).GetMethod("ExpandedSummoningPlayerPathSpawnPrefix", statics)), null, null);
                ExpandedSummoningRuleCapture.Clear(); ExpandedSummoningRuleDurationCapture.Clear(); ExpandedSummoningPlayerPathEvents.Clear(); Cleanup2DruidCastRules.Clear();
                _expandedSummoningPlayerPathRuleCastCount = 0; _expandedSummoningPlayerPathSpawnActionCount = 0;
                _cleanup2DruidCastExpected = child; _expandedSummoningPlayerPathCaptureActive = true;
                row.OnClick();
                bool selected = ReferenceEquals(Game.Instance.SelectedAbilityHandler.Ability, child) && debit.Unspent && fixture.Caster.Commands.Empty;
                if (selected) closeOwnPopup();
                bool targeted = selected && Game.Instance.SelectedAbilityHandler.OnClick(null, fixture.Caster.Position + new Vector3(2, 0, 2), 0);
                command = fixture.Caster.Commands.GetCommand<UnitUseAbility>();
                bool issued = targeted && command != null && ReferenceEquals(command.Spell, child);
                var watch = Stopwatch.StartNew();
                while (issued && watch.Elapsed.TotalSeconds < 25 && phase.Elapsed.TotalSeconds < 240 &&
                    (_expandedSummoningPlayerPathRuleCastCount == 0 || !fixture.Caster.Commands.Empty))
                {
                    if (!mode.Current || !Cleanup2NoNarrativeOverlay() || Game.Instance.IsControllerGamepad ||
                        (Game.Instance.SelectedAbilityHandler.Ability != null && !ReferenceEquals(Game.Instance.SelectedAbilityHandler.Ability, child)))
                        throw new InvalidOperationException("Native Druid cast lost its exact mode/target owner.");
                    Game.Instance.IsPaused = false; yield return 0;
                }
                var units = ExpandedSummoningRuleCapture.Where(value => value != null && ExpandedSummoningPlayerPathUnitExact(value, fixture.Caster)).Distinct().ToArray();
                foreach (var unit in units) if (!fixture.Created.Contains(unit)) fixture.Created.Add(unit);
                var expected = SummonSignatureReader.Read(child.Blueprint).Spawns().ToArray();
                bool exact = issued && command.IsFinished && command.Result.ToString() == "Success" && fixture.Caster.Commands.Empty &&
                    Cleanup2DruidCastRules.Count == 1 && Cleanup2DruidCastRules[0].Success && ReferenceEquals(Cleanup2DruidCastRules[0].Spell, child) &&
                    _expandedSummoningPlayerPathRuleCastCount == 1 && _expandedSummoningPlayerPathSpawnActionCount == 1 &&
                    expected.Length == 1 && units.Length == 1 && ExpandedSummoningRuleCapture.Distinct().Count() == 1 && units[0].Blueprint.AssetGuid == expected[0].Unit &&
                    Cleanup2DruidSourceChain(child, source, parent) && debit.ExactlyOne(source.ParamSpellSlot);
                Cleanup2Check("menu-adapter-druid-native-horse-cast-debit", exact, new JObject {
                    ["source"] = source.Blueprint.AssetGuid, ["canonicalConversion"] = parent.AssetGuid, ["child"] = child.Blueprint.AssetGuid,
                    ["book"] = child.Spellbook.Blueprint.AssetGuid, ["nativeCommandExact"] = issued, ["nativeRuleExactChildReference"] =
                        Cleanup2DruidCastRules.Count == 1 && ReferenceEquals(Cleanup2DruidCastRules[0].Spell, child),
                    ["retainedNativeQuantityConversion"] = child.ConvertedFrom?.Blueprint.AssetGuid,
                    ["nativeQuantityConversionRetainedInChain"] = Cleanup2DruidSourceChain(child, source, parent),
                    ["actualUnit"] = units.FirstOrDefault()?.Blueprint.AssetGuid, ["expectedUnit"] = expected.FirstOrDefault()?.Unit,
                    ["nativePathEvents"] = new JArray(ExpandedSummoningPlayerPathEvents), ["debit"] = debit.Describe(source.ParamSpellSlot),
                    ["wallSeconds"] = watch.Elapsed.TotalSeconds, ["manualTicks"] = 0, ["forcedAct"] = false },
                    "Choosing the visible real Druid Horse row reaches its exact native target/command/event, summons the catalog actor and spends only the actual original prepared source.");
                if (!exact) throw new InvalidOperationException("Native Druid Horse cast/debit failed.");
            }
            finally
            {
                _cleanup2DruidCastExpected = null; _expandedSummoningPlayerPathCaptureActive = false; hooks.UnpatchAll(hooks.Id); Cleanup2DruidCastRules.Clear();
                if (ReferenceEquals(Game.Instance.SelectedAbilityHandler?.Ability, child)) Game.Instance.SelectedAbilityHandler.DropAbility();
                if (command != null && !command.IsFinished && ReferenceEquals(command.Spell, child) && ReferenceEquals(fixture.Caster.Commands.GetCommand<UnitUseAbility>(), command)) command.Interrupt(true);
                var positive = ExpandedSummoningRuleCapture.Where(value => value != null && ExpandedSummoningPlayerPathUnitExact(value, fixture.Caster)).Distinct().ToArray();
                foreach (var unit in positive) if (!fixture.Created.Contains(unit)) fixture.Created.Add(unit);
                if (positive.Length != 0) DisposeExpandedSummoningUnits(fixture.Created, positive);
                Game.Instance.IsPaused = paused;
            }
        }
    }
}
