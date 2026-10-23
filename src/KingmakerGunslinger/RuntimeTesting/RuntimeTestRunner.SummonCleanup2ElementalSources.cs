using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Harmony12;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.GameModes;
using Kingmaker.RuleSystem.Rules.Abilities;
using Kingmaker.UI.ActionBar;
using Kingmaker.UI.Group;
using Kingmaker.UI.ServiceWindow;
using Kingmaker.UI.UnitSettings;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics.Actions;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        // All debit observations include every book, every native raw slot
        // (including Favorite), known/custom objects and resource identities.
        // Fixture preparation/rest occurs before this snapshot, never during
        // the observed choose/target/native command interval.
        private sealed class Cleanup2SourceDebit
        {
            private readonly UnitEntityData _actor;
            private readonly Spellbook[] _books;
            private readonly SpellSlot[][] _slots;
            private readonly AbilityData[][] _spells, _known, _custom;
            private readonly bool[][] _available;
            private readonly BlueprintScriptableObject[] _resources;
            private readonly int[] _amounts;
            internal Cleanup2SourceDebit(UnitEntityData actor)
            {
                _actor = actor; _books = actor.Descriptor.Spellbooks.ToArray();
                _slots = _books.Select(book => Enumerable.Range(0, 10).SelectMany(level => RawSlots(book, level)).ToArray()).ToArray();
                _spells = _slots.Select(slots => slots.Select(slot => slot.Spell).ToArray()).ToArray();
                _available = _slots.Select(slots => slots.Select(slot => slot.Available).ToArray()).ToArray();
                _known = _books.Select(book => book.GetAllKnownSpells().ToArray()).ToArray();
                _custom = _books.Select(book => Enumerable.Range(0, 10).SelectMany(book.GetCustomSpells).ToArray()).ToArray();
                var resources = new List<BlueprintScriptableObject>();
                foreach (BlueprintScriptableObject resource in actor.Descriptor.Resources) resources.Add(resource);
                _resources = resources.ToArray(); _amounts = _resources.Select(actor.Descriptor.Resources.GetResourceAmount).ToArray();
            }
            private bool Identities()
            {
                var resources = new List<BlueprintScriptableObject>();
                foreach (BlueprintScriptableObject resource in _actor.Descriptor.Resources) resources.Add(resource);
                return _actor.Descriptor.Spellbooks.SequenceEqual(_books) && resources.SequenceEqual(_resources) &&
                    _resources.Select((resource, index) => _actor.Descriptor.Resources.GetResourceAmount(resource) == _amounts[index]).All(value => value) &&
                    _books.Select((book, index) => Enumerable.Range(0, 10).SelectMany(level => RawSlots(book, level)).SequenceEqual(_slots[index]) &&
                        _slots[index].Select(slot => slot.Spell).SequenceEqual(_spells[index]) && book.GetAllKnownSpells().SequenceEqual(_known[index]) &&
                        Enumerable.Range(0, 10).SelectMany(book.GetCustomSpells).SequenceEqual(_custom[index])).All(value => value);
            }
            internal bool Unspent { get { return Identities() && _slots.Select((slots, book) =>
                slots.Select((slot, index) => slot.Available == _available[book][index]).All(value => value)).All(value => value); } }
            internal SpellSlot[] Changed { get { return _slots.SelectMany((slots, book) =>
                slots.Where((slot, index) => slot.Available != _available[book][index])).ToArray(); } }
            internal bool ExactlyOne(SpellSlot expected)
            { return Identities() && expected != null && !expected.Available && Changed.SequenceEqual(new[] { expected }) &&
                _slots.Select((slots, book) => slots.Select((slot, index) => !ReferenceEquals(slot, expected) || _available[book][index]).All(value => value)).All(value => value); }
            internal JObject Describe(SpellSlot expected)
            { return new JObject { ["exactlyOneOriginalSlotSpent"] = ExactlyOne(expected), ["allBookSlotSpellKnownCustomAndResourceReferencesExact"] = Identities(),
                ["books"] = new JArray(_books.Select(book => book.Blueprint.AssetGuid)), ["nativeSlotCount"] = _slots.Sum(slots => slots.Length),
                ["changed"] = new JArray(Changed.Select(slot => new JObject { ["source"] = slot.Spell?.Blueprint.AssetGuid,
                    ["book"] = slot.Spell?.Spellbook?.Blueprint.AssetGuid, ["level"] = slot.SpellLevel, ["type"] = slot.Type.ToString(),
                    ["expectedExactReference"] = ReferenceEquals(slot, expected), ["available"] = slot.Available })) }; }
        }

        private IEnumerable<int> ReviewSummonCleanup2ElementalSources(ExpandedSummoningCorrectionFixture fixture, Spellbook book)
        {
            if (!IsSummonCleanup2Review || !_request.ExitAfterCompletion || _request.Parameters == null ||
                _request.Parameters.Count != 1 || (string)_request.Parameters["reviewPhase"] != "elementals-sources" ||
                !ElementalSourceChooserRuntime.Active || _expandedSummoningPlayerPathCaptureActive)
                throw new InvalidOperationException("Exact active, exclusive automatic-exit elementals-sources phase required.");
            var game = Game.Instance; var player = game.Player; var ui = game.UI; var group = GroupController.Instance; var bar = ActionBarManager.Instance;
            if (!Cleanup2NoNarrativeOverlay() || game.IsControllerGamepad || group == null || bar?.Group == null ||
                ui?.SelectionManagerPC == null || ui.ServiceWindow == null || ui.ServiceWindow.WindowTabs.IsShow ||
                ui.DescriptionController.DescWindow.gameObject.activeInHierarchy || ui.TooltipsController.ContextTooltipData != null || !fixture.Caster.Commands.Empty)
                throw new InvalidOperationException("Closed idle native desktop fixture with no foreign service/tooltip/input owner required.");
            var party = player.Party.ToArray(); var selection = ui.SelectionManagerPC.SelectedUnits.ToArray();
            var originalGroup = group.GetCurrentCharacter(); var groups = bar.Group.GroupElements.ToArray();
            var toggles = groups.Select(value => value.ToggleState).ToArray();
            if (toggles.Any(value => value)) throw new InvalidOperationException("Closed native action groups required.");
            var originalInputs = party.Select(value => new Cleanup2TooltipInputs(value)).ToArray();
            var originalBars = party.Select(value => new TeleportationUiSettingsFixture(value.UISettings)).ToArray();
            var extraBooks = new TeleportResourceFixtureOwner(fixture.Caster); var removeFacts = new List<Action>();
            var hooks = HarmonyInstance.Create("kmg.cleanup2.elemental-sources." + _request.RunId);
            var watch = Stopwatch.StartNew(); var casts = new JArray(); bool added = false, selectedFixture = false;
            Cleanup2NativeDefaultScope mode = null; NativeIconScreenEvidence screens = null;
            AbilityData pendingOwned = null;
            UnitEntityData[] ownedParty = null;
            var ownedSessions = new HashSet<ElementalChooserSession>();
            Func<bool> uiOwned = () => ownedParty != null && player.Party.SequenceEqual(ownedParty) &&
                ReferenceEquals(Game.Instance, game) && ReferenceEquals(game.UI, ui) && ReferenceEquals(ActionBarManager.Instance, bar) &&
                ReferenceEquals(GroupController.Instance, group) && bar.Group.GroupElements.SequenceEqual(groups) &&
                ReferenceEquals(group.GetCurrentCharacter(), fixture.Caster) && ui.SelectionManagerPC.SelectedUnits.Count == 1 &&
                ReferenceEquals(ui.SelectionManagerPC.SelectedUnits.First(), fixture.Caster) &&
                ui.TooltipsController.ContextTooltipData == null && game.SelectedAbilityHandler?.Ability == null;
            try
            {
                foreach (var parent in StandaloneElementalPresentationPolicy.All.Where(value => value.IsParent))
                    PrepareExpandedSummoningPlayerPathSpell(book, Cleanup2ElementalAbility(fixture, parent.Guid),
                        Cleanup2ElementalAbility(fixture, parent.Guid), parent.CanonicalTier);
                var druid = BlueprintLibraryLookup.RequireExact<BlueprintCharacterClass>(BlueprintBootstrap.Library,
                    "610d836f3a3a9ed42a4349b62f002e96", "second actual native prepared spellbook");
                var otherBook = extraBooks.AddBook(druid.Spellbook);
                var small = ElementalSourceChooserPolicy.Rank(2);
                var smallParent = Cleanup2ElementalAbility(fixture, small.Guid);
                PrepareExpandedSummoningPlayerPathSpell(otherBook, smallParent, smallParent, 2); otherBook.Rest();
                const BindingFlags statics = BindingFlags.Static | BindingFlags.NonPublic;
                hooks.Patch(typeof(RuleCastSpell).GetMethod("OnTrigger"), null,
                    new HarmonyMethod(typeof(RuntimeTestRunner).GetMethod("ExpandedSummoningPlayerPathRuleCastPostfix", statics)), null);
                hooks.Patch(typeof(ContextActionSpawnMonster).GetMethod("RunAction"),
                    new HarmonyMethod(typeof(RuntimeTestRunner).GetMethod("ExpandedSummoningPlayerPathSpawnPrefix", statics)), null, null);
                _expandedSummoningPlayerPathCaptureActive = true;
                if (!party.Contains(fixture.Caster))
                { player.PartyCharacters.Add(fixture.Caster); added = true; player.InvalidateCharacterLists(); player.UpdateCharacterLists(); }
                mode = Cleanup2EnterNativeDefault(fixture, "elementals-sources"); screens = new NativeIconScreenEvidence(_request);
                ui.SelectionManagerPC.SelectUnit(fixture.Caster.View, true, true, false); group.SelectUnit(fixture.Caster); selectedFixture = true;
                ownedParty = player.Party.ToArray();
                for (int frame = 0; frame < 12; frame++) yield return 0;
                var spellGroup = groups.Single(value => (ActionBarSlotType)typeof(ActionBarGroupElement).GetField("SlotType", Cleanup2Fields).GetValue(value) == ActionBarSlotType.Spell);
                if (!spellGroup.ToggleState) spellGroup.OnClick();
                for (int frame = 0; frame < 8; frame++) yield return 0;
                double smallFireBaseSeconds = double.NaN;
                foreach (int rank in ElementalSourceChooserPolicy.Ranks)
                foreach (string key in ElementalSourceChooserPolicy.Rank(rank).CreatureKeys)
                {
                    Cleanup2SourcesBoundary(mode, watch);
                    if (!uiOwned() || ElementalSourceChooserView.IsOpen) throw new InvalidOperationException("Foreign source UI; independent preparation refresh refused.");
                    // Independent native fixture cases. Rest is deliberately
                    // before the source/debit snapshot, not part of a cast.
                    book.Rest(); spellGroup.Fill(fixture.Caster);
                    foreach (int frame in Cleanup2SettleChooserNativeRefill(fixture, book, spellGroup, mode, uiOwned, ownedSessions,
                        ElementalSourceChooserRuntime.Session(spellGroup), "sources-rank" + rank + "-" + key, value => { })) yield return frame;
                    var source = Cleanup2SourcesFind(spellGroup, book, key, false);
                    var prep = source.Preparation; var data = source.Child;
                    foreach (int frame in Cleanup2SourcesReady(spellGroup, source, mode, watch)) yield return frame;
                    pendingOwned = data;
                    foreach (int frame in Cleanup2SourcesCast(fixture, data, prep, () => ElementalSourceChooserView.ReviewChoose(rank, key, source, true),
                        mode, watch, "rank" + rank + "-" + key, row => {
                            casts.Add(row); if (key == "small-fire-elemental") smallFireBaseSeconds = (double)row["baseDurationSeconds"];
                        })) yield return frame;
                    pendingOwned = null;
                }
                Cleanup2Check("elemental-sources-all24-original-casts", casts.Count == 24,
                    new JObject { ["casts"] = casts, ["independentNativeRestBeforeEachCase"] = true,
                        ["fixtureAccess"] = "Explicit real native Wizard preparations; not natural learning, old-save hydration or uninterrupted-combat endurance.",
                        ["manualPulse"] = false, ["forcedAct"] = false },
                    "Every original six-rank/four-element child casts once through the actual chooser and native target/command, spending exactly its original native source preparation.");

                // Two different physical spellbooks expose the same real
                // native parent. Exhausting one must never borrow the other.
                if (!uiOwned() || ElementalSourceChooserView.IsOpen) throw new InvalidOperationException("Foreign source UI; two-book refresh refused.");
                book.Rest(); otherBook.Rest(); spellGroup.Fill(fixture.Caster);
                foreach (int frame in Cleanup2SettleChooserNativeRefill(fixture, book, spellGroup, mode, uiOwned, ownedSessions,
                    ElementalSourceChooserRuntime.Session(spellGroup), "sources-two-books", value => { })) yield return frame;
                var wizardFire = Cleanup2SourcesFind(spellGroup, book, "small-fire-elemental", false);
                var druidFire = Cleanup2SourcesFind(spellGroup, otherBook, "small-fire-elemental", false);
                bool separate = !ReferenceEquals(wizardFire.Source, druidFire.Source) && !ReferenceEquals(wizardFire.Child, druidFire.Child) &&
                    !ReferenceEquals(wizardFire.Preparation, druidFire.Preparation) && !ReferenceEquals(wizardFire.Book, druidFire.Book);
                Cleanup2Check("elemental-sources-two-native-books", separate, new JObject {
                    ["firstBook"] = book.Blueprint.AssetGuid, ["secondBook"] = otherBook.Blueprint.AssetGuid,
                    ["sourceReferencesDistinct"] = separate, ["sameOriginalChildGuid"] = wizardFire.Child.Blueprint.AssetGuid == druidFire.Child.Blueprint.AssetGuid },
                    "Small Fire has two real native book/source/child/preparation contexts with the same original child identity.");
                if (!separate) throw new InvalidOperationException("Two native source books were conflated.");
                foreach (int frame in Cleanup2SourcesReady(spellGroup, wizardFire, mode, watch)) yield return frame;
                foreach (int frame in Cleanup2SourcesCast(fixture, wizardFire.Child, wizardFire.Preparation,
                    () => ElementalSourceChooserView.ReviewChoose(2, wizardFire.CreatureKey, wizardFire, true), mode, watch, "small-fire-first-book", null)) yield return frame;
                foreach (int frame in Cleanup2SettleChooserNativeRefill(fixture, book, spellGroup, mode, uiOwned, ownedSessions,
                    ElementalSourceChooserRuntime.Session(spellGroup), "sources-first-book-exhausted", value => { })) yield return frame;
                wizardFire = Cleanup2SourcesFind(spellGroup, book, "small-fire-elemental", false);
                druidFire = Cleanup2SourcesFind(spellGroup, otherBook, "small-fire-elemental", false);
                foreach (int frame in Cleanup2SourcesReady(spellGroup, wizardFire, mode, watch)) yield return frame;
                var noSpend = new Cleanup2SourceDebit(fixture.Caster);
                ElementalSourceChooserView.ReviewChoose(2, wizardFire.CreatureKey, wizardFire, false);
                bool refused = !ElementalSourceChooserView.ReviewTryCast() && noSpend.Unspent && !wizardFire.Preparation.Available &&
                    druidFire.Preparation.Available && game.SelectedAbilityHandler.Ability == null;
                foreach (int frame in screens.Capture("elementals-sources-two-books-exhausted", new JObject {
                    ["renderedText"] = new JArray(ElementalSourceChooserView.RenderedText), ["firstExhausted"] = !wizardFire.Preparation.Available,
                    ["secondAvailable"] = druidFire.Preparation.Available }, () => mode.Current && ElementalSourceChooserView.IsOpen && noSpend.Unspent)) yield return frame;
                ElementalSourceChooserView.CloseOwned("exhausted-book-cancel-no-spend");
                Cleanup2Check("elemental-sources-exhaustion-isolated", refused, new JObject {
                    ["refused"] = refused, ["firstAvailable"] = wizardFire.Preparation.Available, ["secondAvailable"] = druidFire.Preparation.Available },
                    "Selecting the exhausted first native book cannot borrow the second book's preparation or hand off targeting.");
                if (!refused) throw new InvalidOperationException("Exhausted native source borrowed another book.");
                foreach (int frame in Cleanup2SourcesReady(spellGroup, druidFire, mode, watch)) yield return frame;
                foreach (int frame in Cleanup2SourcesCast(fixture, druidFire.Child, druidFire.Preparation,
                    () => ElementalSourceChooserView.ReviewChoose(2, druidFire.CreatureKey, druidFire, true), mode, watch, "small-fire-second-book", null)) yield return frame;

                foreach (int frame in Cleanup2SourcesNaturesAllyFire(fixture, otherBook, spellGroup, mode, watch, uiOwned, ownedSessions)) yield return frame;

                AbilityData authored = null;
                foreach (int frame in Cleanup2SourcesExtend(fixture, book, smallParent, mode, watch, screens, removeFacts,
                    value => authored = value)) yield return frame;
                if (!uiOwned() || ElementalSourceChooserView.IsOpen) throw new InvalidOperationException("Foreign source UI; native metamagic refresh refused.");
                spellGroup.Fill(fixture.Caster);
                foreach (int frame in Cleanup2SettleChooserNativeRefill(fixture, book, spellGroup, mode, uiOwned, ownedSessions,
                    ElementalSourceChooserRuntime.Session(spellGroup), "sources-native-extend", value => { })) yield return frame;
                var extended = Cleanup2SourcesFind(spellGroup, book, "small-fire-elemental", true);
                bool nativeMeta = authored != null && extended.Source.HasMetamagic(Metamagic.Extend) && extended.Child.HasMetamagic(Metamagic.Extend) &&
                    extended.Source.SpellLevel == 3 && extended.Child.SpellLevel == 3 && extended.Preparation.Spell.HasMetamagic(Metamagic.Extend);
                Cleanup2Check("elemental-sources-native-extend-context", nativeMeta, new JObject {
                    ["editorCustomAbility"] = authored?.Blueprint.AssetGuid, ["sourceLinks"] = DescribeExpandedSummoningAbilityData(extended.Child),
                    ["nativeEditorProducedCustomSpell"] = authored != null, ["sourceLevel"] = extended.Source.SpellLevel,
                    ["runtimeAction"] = extended.Child.RuntimeActionType.ToString(), ["fullRound"] = extended.Child.RequireFullRoundAction },
                    "Native Extend editor/Write/preparation produces the exact adjusted source and the actual native factory retains its metamagic on Small Fire.");
                if (!nativeMeta) throw new InvalidOperationException("Native metamagic source context was not retained.");
                foreach (int frame in Cleanup2SourcesReady(spellGroup, extended, mode, watch)) yield return frame;
                double extendedSeconds = double.NaN;
                foreach (int frame in Cleanup2SourcesCast(fixture, extended.Child, extended.Preparation,
                    () => ElementalSourceChooserView.ReviewChoose(2, extended.CreatureKey, extended, true), mode, watch, "small-fire-native-extend",
                    row => extendedSeconds = (double)row["baseDurationSeconds"])) yield return frame;
                var signature = SummonSignatureReader.Read(extended.Child.Blueprint).Spawns().Single();
                double multiplier = signature.DurationExtendable ? 2 : 1;
                bool duration = !double.IsNaN(smallFireBaseSeconds) && Math.Abs(extendedSeconds - smallFireBaseSeconds * multiplier) <= .001;
                Cleanup2Check("elemental-sources-native-extend-duration", duration, new JObject {
                    ["ordinaryNativeBaseSeconds"] = smallFireBaseSeconds, ["extendedNativeBaseSeconds"] = extendedSeconds,
                    ["originalActionExtendable"] = signature.DurationExtendable, ["nativeExpectedMultiplier"] = multiplier },
                    "Actual RuleSummonUnit duration follows the original native action's Extend eligibility; no duration/rank field is edited.");
                if (!duration) throw new InvalidOperationException("Native Extend duration differs from original action contract.");
                foreach (int frame in Cleanup2SourcesSpecialAndPin(fixture, book, smallParent, spellGroup, mode, watch, screens, removeFacts, uiOwned, ownedSessions)) yield return frame;
                Cleanup2Check("elemental-sources-scope", true, new JObject {
                    ["all24OriginalCasts"] = casts.Count, ["maximumPhaseWallSeconds"] = 360, ["wallSeconds"] = watch.Elapsed.TotalSeconds,
                    ["console"] = "separate native controller transaction; not exercised here", ["coldSave"] = "separate explicitly authorized fresh-load transaction; not proved by runtime pins",
                    ["moduleOff"] = "fresh process with module OFF before startup; Active is restart-only, never toggled by reflection here" },
                    "The original-source cast/source-budget slice and separate controller, cold-save and fresh-OFF obligations are explicit.");
            }
            finally
            {
                var errors = new List<string>();
                Action<string, Action> restore = (name, action) => { try { action(); } catch (Exception error) { errors.Add(name + ":" + error.Message); } };
                try
                {
                    ElementalSourceChooserView.CloseOwned("elemental-sources-owned-cleanup");
                    if (pendingOwned != null && ReferenceEquals(game.SelectedAbilityHandler?.Ability, pendingOwned))
                        restore("own-target", () => game.SelectedAbilityHandler.DropAbility());
                    restore("capture-hooks", () => { _expandedSummoningPlayerPathCaptureActive = false; hooks.UnpatchAll(hooks.Id); });
                    foreach (var remove in removeFacts.AsEnumerable().Reverse()) restore("own-native-feature", remove);
                    restore("own-extra-books", extraBooks.Restore);
                    bool untouched = !selectedFixture && ReferenceEquals(group.GetCurrentCharacter(), originalGroup) && ui.SelectionManagerPC.SelectedUnits.SequenceEqual(selection);
                    bool ownsUi = ReferenceEquals(GroupController.Instance, group) && ReferenceEquals(ActionBarManager.Instance, bar) &&
                        ui.TooltipsController.ContextTooltipData == null && game.SelectedAbilityHandler?.Ability == null &&
                        (untouched || ReferenceEquals(group.GetCurrentCharacter(), fixture.Caster) &&
                            ui.SelectionManagerPC.SelectedUnits.Count == 1 && ReferenceEquals(ui.SelectionManagerPC.SelectedUnits.First(), fixture.Caster));
                    if (!ownsUi) errors.Add("Foreign group/selection/tooltip/target owner; not overwritten.");
                    else
                    {
                        if (!untouched) for (int i = 0; i < groups.Length; i++) { int index = i; restore("group-toggle", () => groups[index].Toggle(toggles[index], true)); }
                        if (added) restore("own-party-entry", () => { player.PartyCharacters.Remove(fixture.Caster); player.InvalidateCharacterLists(); player.UpdateCharacterLists(); });
                        if (!untouched)
                        {
                            restore("native-group", () => group.SelectUnit(originalGroup));
                            restore("native-selection", () => ui.SelectionManagerPC.MultiSelect(selection.Where(value => value?.View != null).Select(value => value.View).ToArray(), false));
                        }
                    }
                    bool exact = errors.Count == 0 && player.Party.SequenceEqual(party) && ui.SelectionManagerPC.SelectedUnits.SequenceEqual(selection) &&
                        ReferenceEquals(group.GetCurrentCharacter(), originalGroup) && originalInputs.All(value => value.Unchanged()) &&
                        originalBars.All(value => value.IsRestored()) && extraBooks.IsRestored();
                    Cleanup2Check("elemental-sources-restored", exact, new JObject {
                        ["partyExact"] = player.Party.SequenceEqual(party), ["originalResourcesSlotsPinsExact"] = originalInputs.All(value => value.Unchanged()) && originalBars.All(value => value.IsRestored()),
                        ["ownedFeaturesAndBooksReleased"] = errors.Count == 0 && extraBooks.IsRestored(), ["errors"] = new JArray(errors) },
                        "Owned native facts/books/pin/chooser/UI release while every original party input and reference remains exact.");
                    if (!exact) errors.Add("Original UI/resource restoration unverified.");
                }
                finally
                {
                    try { if (screens != null) screens.Dispose(); }
                    finally { if (mode != null) mode.Dispose(); }
                }
                if (errors.Count != 0) throw new InvalidOperationException(string.Join("|", errors.ToArray()));
            }
        }

        private void Cleanup2SourcesBoundary(Cleanup2NativeDefaultScope mode, Stopwatch phase)
        {
            if (phase.Elapsed.TotalSeconds > 360 || !mode.Current || !Cleanup2NoNarrativeOverlay() || Game.Instance.IsControllerGamepad)
                throw new InvalidOperationException("Native elemental source phase exceeded wall/input/ownership bound.");
        }

        private static AbilityData _cleanup2SnaFireExpected;
        private static readonly List<RuleCastSpell> Cleanup2SnaFireRules = new List<RuleCastSpell>();
        private static void Cleanup2SourcesSnaFireRulePostfix(RuleCastSpell __instance)
        {
            if (_cleanup2SnaFireExpected == null || __instance == null) return;
            if (Cleanup2SnaFireRules.Count < 8) Cleanup2SnaFireRules.Add(__instance);
        }

        private IEnumerable<int> Cleanup2SourcesNaturesAllyFire(ExpandedSummoningCorrectionFixture fixture, Spellbook book,
            ActionBarGroupElement group, Cleanup2NativeDefaultScope mode, Stopwatch phase,
            Func<bool> uiOwned, ICollection<ElementalChooserSession> ownedSessions)
        {
            Cleanup2SourcesBoundary(mode, phase);
            if (!uiOwned() || ElementalSourceChooserView.IsOpen) throw new InvalidOperationException("Foreign source UI; native SNA preparation refused.");
            var rank = ElementalSourceChooserPolicy.Rank(2);
            var parent = Cleanup2ElementalAbility(fixture, rank.NaturesAllyParentGuid);
            var spec = ExpandedSummoningCatalog.GenerateVariants(SummonFamily.NaturesAlly).Single(value => value.ParentTier == 2 &&
                value.Multiplicity == SummonMultiplicity.One && value.Creature.Key == "small-fire-elemental");
            var expectedChild = ExpandedSummoningRoot(fixture.Blueprints, spec);
            if (!book.Blueprint.SpellList.GetSpells(2).Any(value => ReferenceEquals(value, parent)) || !SummonVisibilityCatalog.IsPublished(spec) ||
                parent.Variants == null || parent.Variants.Count(value => ReferenceEquals(value, expectedChild)) != 1)
                throw new InvalidOperationException("Exact ordinary native Druid SNAII parent and published one-Small-Fire child required.");
            // This is the actual Druid list/book, not SNA granted to a Wizard
            // list or an inferred equivalent standalone/SM invocation.
            PrepareExpandedSummoningPlayerPathSpell(book, parent, parent, 2); book.Rest(); group.Fill(fixture.Caster);
            foreach (int frame in Cleanup2SettleChooserNativeRefill(fixture, book, group, mode, uiOwned, ownedSessions,
                ElementalSourceChooserRuntime.Session(group), "sources-native-snaII", value => { })) yield return frame;
            var session = ElementalSourceChooserRuntime.Session(group);
            if (session == null || !session.Current) throw new InvalidOperationException("Current native SNA source chooser session required.");
            var source = session.Sources.Single(value => value.CreatureKey == "small-fire-elemental" && ReferenceEquals(value.Book, book) &&
                ReferenceEquals(value.Source.Blueprint, parent) && ReferenceEquals(value.Child.Blueprint, expectedChild) && !value.Source.HasMetamagic(Metamagic.Extend));
            if (!ReferenceEquals(source.PresentationParent, parent) || source.Preparation == null || source.Validate() != null || _cleanup2SnaFireExpected != null)
                throw new InvalidOperationException("Exact independent native SNAII source/preparation context required.");
            string parentRules = SummonSignatureReader.Read(parent).Render();
            string childRules = SummonSignatureReader.Read(expectedChild).Render();
            var action = source.Child.RuntimeActionType; bool fullRound = source.Child.RequireFullRoundAction;
            var meta = source.Child.MetamagicData; var nativeSlot = source.Child.ParamSpellSlot;
            var sourceData = source.Source; var childData = source.Child;
            var hooks = HarmonyInstance.Create("kmg.cleanup2.sna-fire-source." + _request.RunId);
            JObject cast = null;
            try
            {
                Cleanup2SnaFireRules.Clear(); _cleanup2SnaFireExpected = childData;
                hooks.Patch(typeof(RuleCastSpell).GetMethod("OnTrigger"), null, new HarmonyMethod(typeof(RuntimeTestRunner).GetMethod(
                    "Cleanup2SourcesSnaFireRulePostfix", BindingFlags.Static | BindingFlags.NonPublic)), null);
                foreach (int frame in Cleanup2SourcesReady(group, source, mode, phase)) yield return frame;
                foreach (int frame in Cleanup2SourcesCast(fixture, childData, source.Preparation,
                    () => ElementalSourceChooserView.ReviewChoose(2, source.CreatureKey, source, true), mode, phase, "small-fire-native-snaII",
                    value => cast = value)) yield return frame;
                bool exact = cast != null && (bool)cast["sourceLinksExact"] && Cleanup2SnaFireRules.Count == 1 && Cleanup2SnaFireRules[0].Success &&
                    ReferenceEquals(Cleanup2SnaFireRules[0].Spell, childData) && ReferenceEquals(childData.ConvertedFrom, sourceData) &&
                    ReferenceEquals(childData.Spellbook, book) && ReferenceEquals(childData.Blueprint, expectedChild) &&
                    ReferenceEquals(childData.MetamagicData, meta) && ReferenceEquals(childData.ParamSpellSlot, nativeSlot) &&
                    childData.RuntimeActionType == action && childData.RequireFullRoundAction == fullRound &&
                    SummonSignatureReader.Read(parent).Render() == parentRules && SummonSignatureReader.Read(expectedChild).Render() == childRules;
                Cleanup2Check("elemental-sources-native-snaII-small-fire-identity", exact, new JObject {
                    ["family"] = "NaturesAlly", ["parent"] = parent.AssetGuid, ["child"] = expectedChild.AssetGuid, ["stableKey"] = spec.StableKey,
                    ["book"] = book.Blueprint.AssetGuid, ["nativeSourceMechanism"] = source.Mechanism.GetType().FullName,
                    ["sourceDataIdentity"] = System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(sourceData),
                    ["childDataIdentity"] = System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(childData),
                    ["paramSlotIdentity"] = nativeSlot == null ? (JToken)JValue.CreateNull() : System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(nativeSlot),
                    ["sourceSpecificParentRules"] = parentRules, ["sourceSpecificChildRules"] = childRules,
                    ["nativeAction"] = action.ToString(), ["fullRound"] = fullRound,
                    ["ruleEvents"] = new JArray(Cleanup2SnaFireRules.Select(value => new JObject { ["exactChildDataReference"] = ReferenceEquals(value.Spell, childData),
                        ["child"] = value.Spell?.Blueprint.AssetGuid, ["source"] = value.Spell?.ConvertedFrom?.Blueprint.AssetGuid, ["success"] = value.Success,
                        ["contextIdentity"] = value.Context == null ? (JToken)JValue.CreateNull() : System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(value.Context) })),
                    ["nativePathEvents"] = new JArray(ExpandedSummoningPlayerPathEvents), ["castAndExactAllBookDebit"] = cast,
                    ["scope"] = "Actual Druid-book canonical SNAII chooser source/cast. Its own original rules, action, child and native event are observed; no equality to standalone or SM rules is assumed." },
                    "The canonical SNAII source dispatches its exact native generated Small Fire child and RuleCastSpell object with its own unchanged rules/context and exact native preparation debit.");
                if (!exact) throw new InvalidOperationException("Native SNAII Small Fire event/source/rule identity failed.");
            }
            finally
            {
                _cleanup2SnaFireExpected = null;
                hooks.UnpatchAll(hooks.Id); Cleanup2SnaFireRules.Clear();
            }
        }

        private static ElementalNativeSource Cleanup2SourcesFind(ActionBarGroupElement group, Spellbook book, string key, bool extend)
        {
            var session = ElementalSourceChooserRuntime.Session(group);
            if (session == null || !session.Current) throw new InvalidOperationException("No current actual elemental chooser session.");
            return session.Sources.Single(value => value.CreatureKey == key && ReferenceEquals(value.Book, book) &&
                ElementalSourceChooserPolicy.Parent(value.Source.Blueprint.AssetGuid) != null &&
                value.Source.HasMetamagic(Metamagic.Extend) == extend);
        }

        private IEnumerable<int> Cleanup2SourcesReady(ActionBarGroupElement group, ElementalNativeSource source,
            Cleanup2NativeDefaultScope mode, Stopwatch phase)
        {
            Cleanup2SourcesBoundary(mode, phase);
            var session = ElementalSourceChooserRuntime.Session(group); session.Levels.OnClick(session.MinimumLevel);
            for (int frame = 0; frame < 4; frame++) yield return 0;
            var launcher = group.GetComponentsInChildren<ActionBarGroupSlot>(true).Single(value => value.gameObject.activeInHierarchy && value.MechanicSlot is ElementalChooserLauncher);
            launcher.OnClick();
            for (int frame = 0; frame < 3; frame++) yield return 0;
            if (!ElementalSourceChooserView.IsOpen || !session.Current || !session.Sources.Any(value => ReferenceEquals(value, source)))
                throw new InvalidOperationException("Actual native source launcher did not open its exact session.");
        }

        private IEnumerable<int> Cleanup2SourcesCast(ExpandedSummoningCorrectionFixture fixture, AbilityData data, SpellSlot preparation,
            Action activate, Cleanup2NativeDefaultScope mode, Stopwatch phase, string label, Action<JObject> result)
        {
            Cleanup2SourcesBoundary(mode, phase);
            var snapshot = new Cleanup2SourceDebit(fixture.Caster); var original = data.ConvertedFrom; var book = data.Spellbook;
            var blueprint = data.Blueprint; var meta = data.MetamagicData; bool extend = data.HasMetamagic(Metamagic.Extend);
            var expected = SummonSignatureReader.Read(blueprint).Spawns().ToArray();
            if (expected.Length != 1 || original == null || preparation == null || !ReferenceEquals(preparation.Spell, original) ||
                !ReferenceEquals(data.Caster, fixture.Caster.Descriptor) || !preparation.Available)
                throw new InvalidOperationException("Exact one-spawn original prepared source required: " + label);
            ExpandedSummoningRuleCapture.Clear(); ExpandedSummoningRuleDurationCapture.Clear(); ExpandedSummoningPlayerPathEvents.Clear();
            _expandedSummoningPlayerPathRuleCastCount = 0; _expandedSummoningPlayerPathSpawnActionCount = 0;
            var watch = Stopwatch.StartNew(); var units = new UnitEntityData[0]; UnitUseAbility command = null;
            try
            {
                activate();
                bool selected = ReferenceEquals(Game.Instance.SelectedAbilityHandler.Ability, data);
                bool noSpendBeforeTarget = snapshot.Unspent && fixture.Caster.Commands.Empty;
                bool clicked = selected && noSpendBeforeTarget && Game.Instance.SelectedAbilityHandler.OnClick(null, fixture.Caster.Position + new Vector3(2, 0, 2), 0);
                command = fixture.Caster.Commands.GetCommand<UnitUseAbility>();
                bool issued = clicked && command != null && ReferenceEquals(command.Spell, data);
                while (issued && watch.Elapsed.TotalSeconds < 25 && phase.Elapsed.TotalSeconds < 360 &&
                    (_expandedSummoningPlayerPathRuleCastCount == 0 || !fixture.Caster.Commands.Empty))
                {
                    Cleanup2SourcesBoundary(mode, phase); Game.Instance.IsPaused = false; yield return 0;
                }
                units = ExpandedSummoningRuleCapture.Where(value => value != null && ExpandedSummoningPlayerPathUnitExact(value, fixture.Caster)).Distinct().ToArray();
                foreach (var unit in units) if (!fixture.Created.Contains(unit)) fixture.Created.Add(unit);
                var durations = ExpandedSummoningRuleDurationCapture.Where(value => units.Any(unit => ReferenceEquals(unit, value.Unit))).ToArray();
                var lifecycle = units.SelectMany(unit => unit.Buffs.Enumerable.Where(buff => ReferenceEquals(buff.Blueprint, BlueprintRoot.Instance.SystemMechanics.SummonedUnitBuff))).ToArray();
                bool links = ReferenceEquals(data.Blueprint, blueprint) && ReferenceEquals(data.ConvertedFrom, original) &&
                    ReferenceEquals(data.Spellbook, book) && ReferenceEquals(data.MetamagicData, meta) && ReferenceEquals(preparation.Spell, original);
                bool pass = issued && noSpendBeforeTarget && command.IsFinished && command.Result.ToString() == "Success" && fixture.Caster.Commands.Empty &&
                    _expandedSummoningPlayerPathRuleCastCount == 1 && _expandedSummoningPlayerPathSpawnActionCount == 1 &&
                    units.Length == 1 && ExpandedSummoningRuleCapture.Distinct().Count() == 1 && units[0].Blueprint.AssetGuid == expected[0].Unit &&
                    durations.Length == 1 && lifecycle.Length == 1 && (!extend || lifecycle[0].Context.HasMetamagic(Metamagic.Extend)) &&
                    snapshot.ExactlyOne(preparation) && links;
                var row = new JObject { ["case"] = label, ["source"] = original.Blueprint.AssetGuid, ["child"] = blueprint.AssetGuid,
                    ["book"] = book.Blueprint.AssetGuid, ["sourceDataIdentity"] = System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(original),
                    ["childDataIdentity"] = System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(data), ["sourceLinksExact"] = links,
                    ["nativeCommandExact"] = issued, ["commandResult"] = command?.Result.ToString(), ["selectionSpentNothing"] = noSpendBeforeTarget,
                    ["nativeAction"] = data.RuntimeActionType.ToString(), ["fullRound"] = data.RequireFullRoundAction, ["extend"] = extend,
                    ["ruleCasts"] = _expandedSummoningPlayerPathRuleCastCount, ["spawnActions"] = _expandedSummoningPlayerPathSpawnActionCount,
                    ["expectedUnit"] = expected[0].Unit, ["actualUnit"] = units.FirstOrDefault()?.Blueprint.AssetGuid,
                    ["baseDurationSeconds"] = durations.Length == 1 ? (JToken)durations[0].BaseDuration.TotalSeconds : JValue.CreateNull(),
                    ["bonusDurationSeconds"] = durations.Length == 1 ? (JToken)durations[0].BonusDuration.TotalSeconds : JValue.CreateNull(),
                    ["debit"] = snapshot.Describe(preparation), ["dataLinks"] = DescribeExpandedSummoningAbilityData(data),
                    ["wallSeconds"] = watch.Elapsed.TotalSeconds, ["manualTicks"] = 0, ["forcedAct"] = false };
                Cleanup2Check("elemental-sources-cast-" + label, pass, row,
                    "Actual native source click/target/command produces its exact provenance-owned summon and one unchanged-context native preparation debit across all books/resources.");
                result?.Invoke(row);
                if (!pass) throw new InvalidOperationException("Native elemental source cast/debit failed: " + label);
            }
            finally
            {
                if (ReferenceEquals(Game.Instance.SelectedAbilityHandler?.Ability, data)) Game.Instance.SelectedAbilityHandler.DropAbility();
                if (command != null && !command.IsFinished && ReferenceEquals(command.Spell, data) &&
                    ReferenceEquals(fixture.Caster.Commands.GetCommand<UnitUseAbility>(), command)) command.Interrupt(true);
                var positive = ExpandedSummoningRuleCapture.Where(value => value != null && ExpandedSummoningPlayerPathUnitExact(value, fixture.Caster)).Distinct().ToArray();
                foreach (var unit in positive) if (!fixture.Created.Contains(unit)) fixture.Created.Add(unit);
                if (positive.Length != 0) DisposeExpandedSummoningUnits(fixture.Created, positive);
            }
        }

        private IEnumerable<int> Cleanup2SourcesExtend(ExpandedSummoningCorrectionFixture fixture, Spellbook book, BlueprintAbility parent,
            Cleanup2NativeDefaultScope mode, Stopwatch phase, NativeIconScreenEvidence screens, List<Action> removes, Action<AbilityData> result)
        {
            Cleanup2SourcesBoundary(mode, phase);
            var ui = Game.Instance.UI; var controller = ui.SpellBookController;
            var extendBlueprint = BlueprintLibraryLookup.RequireExact<BlueprintFeature>(BlueprintBootstrap.Library,
                "f180e72e4a9cbaa4da8be9bc958132ef", "native Extend Spell editor feature");
            if (extendBlueprint.GetComponent<AddMetamagicFeat>()?.Metamagic != Metamagic.Extend || fixture.Caster.Descriptor.HasFact(extendBlueprint))
                throw new InvalidOperationException("Exact new native Extend feat required.");
            var extend = (Feature)fixture.Caster.Descriptor.AddFact(extendBlueprint);
            removes.Add(() => { if (!ReferenceEquals(fixture.Caster.Descriptor.GetFact(extendBlueprint), extend))
                throw new InvalidOperationException("Foreign Extend feature; not removed."); fixture.Caster.Descriptor.RemoveFact(extend); });
            var game = Game.Instance;
            var modesField = typeof(Game).GetField("m_GameModes", Cleanup2Fields);
            var countsField = typeof(Game).GetField("m_ModesCount", Cleanup2Fields);
            var stack = (Stack<GameMode>)modesField.GetValue(game); var beforeModes = stack.ToArray();
            var beforeCounts = ((int[])countsField.GetValue(game)).ToArray();
            var beforeHandler = game.SelectedAbilityHandler; var beforePointer = game.ClickEventsController;
            GameMode ownedWindow = null; bool opened = false;
            Func<bool> windowStackOwned = () => ownedWindow != null && game.CurrentMode == GameModeType.FullScreenUi &&
                ReferenceEquals(modesField.GetValue(game), stack) && stack.Count == beforeModes.Length + 1 &&
                ReferenceEquals(stack.Peek(), ownedWindow) && stack.Skip(1).SequenceEqual(beforeModes) &&
                ((int[])countsField.GetValue(game)).Length == beforeCounts.Length &&
                ((int[])countsField.GetValue(game)).Select((count, index) =>
                    count == beforeCounts[index] + (index == (int)GameModeType.FullScreenUi ? 1 : 0)).All(value => value);
            Func<bool> editorOwned = () => windowStackOwned() && ReferenceEquals(ui.SpellBookController, controller) &&
                ui.ServiceWindow.WindowTabs.IsShow && ReferenceEquals(controller.CurrentCharacter, fixture.Caster) &&
                Cleanup2NoNarrativeOverlay();
            try
            {
                if (ui.ServiceWindow.WindowTabs.IsShow) throw new InvalidOperationException("Native service window already has another owner.");
                ui.ServiceWindow.HandleOpenSpellbook(); opened = true;
                if (stack.Count == beforeModes.Length + 1 && stack.Skip(1).SequenceEqual(beforeModes)) ownedWindow = stack.Peek();
                if (!windowStackOwned()) throw new InvalidOperationException("Actual native fullscreen window did not preserve the owned Default stack/counts.");
                for (int frame = 0; frame < 24; frame++) yield return 0;
                if (!editorOwned()) throw new InvalidOperationException("Actual native metamagic editor input/character ownership did not settle.");
                foreach (int frame in Cleanup2SourcesBookRow(controller, book, parent, 2, false, null)) yield return frame;
                var known = book.GetAllKnownSpells().ToArray(); var customBefore = book.GetCustomSpells(3).ToArray();
                controller.OnSwitchMetamagic();
                for (int frame = 0; frame < 8; frame++) yield return 0;
                var mixer = controller.GetComponentsInChildren<SpellBookMetamagicMixer>(true).Single(value => value.IsShowed);
                var selector = mixer.GetComponentsInChildren<MetamagicSelectorSlotItem>(true).Single(value => value.gameObject.activeInHierarchy && ReferenceEquals(value.MetamagicFeature, extend));
                selector.OnPointerClick(new PointerEventData(EventSystem.current) { clickCount = 2 });
                for (int frame = 0; frame < 8; frame++) yield return 0;
                var temporary = controller.CurrentTemporarySpell;
                bool native = temporary != null && ReferenceEquals(temporary.Blueprint, parent) && ReferenceEquals(temporary.Spellbook, book) &&
                    temporary.HasMetamagic(Metamagic.Extend) && temporary.SpellLevel == 3;
                if (!native) throw new InvalidOperationException("Native editor did not create adjusted original elemental source.");
                var write = (Button)typeof(SpellBookMetamagicMixer).GetField("m_WriteButton", Cleanup2Fields).GetValue(mixer);
                if (!write.gameObject.activeInHierarchy || !write.interactable) throw new InvalidOperationException("Native metamagic Write is unavailable.");
                write.onClick.Invoke();
                for (int frame = 0; frame < 12; frame++) yield return 0;
                var created = book.GetCustomSpells(3).Where(value => !customBefore.Contains(value)).ToArray();
                if (created.Length != 1 || !ReferenceEquals(created[0].Blueprint, parent) || !created[0].HasMetamagic(Metamagic.Extend) ||
                    !book.GetAllKnownSpells().SequenceEqual(known)) throw new InvalidOperationException("Native Write identity/custom/known contract differs.");
                if (mixer.IsShowed) controller.OnSwitchMetamagic();
                SpellItem row = null;
                foreach (int frame in Cleanup2SourcesBookRow(controller, book, parent, 3, true, value => row = value)) yield return frame;
                row.Memorize();
                for (int frame = 0; frame < 6; frame++) yield return 0;
                book.Rest();
                var prepared = book.GetMemorizedSpells(3).Where(value => value.Available && value.Spell.Blueprint == parent && value.Spell.HasMetamagic(Metamagic.Extend)).ToArray();
                bool exact = prepared.Length == 1 && prepared[0].Spell.SpellLevel == 3;
                Cleanup2Check("elemental-sources-native-extend-editor", exact, new JObject {
                    ["feat"] = extendBlueprint.AssetGuid, ["parent"] = parent.AssetGuid, ["customSource"] = created[0].Blueprint.AssetGuid,
                    ["nativeWriteCreatedOneCustom"] = true, ["ordinaryKnownReferencesExact"] = book.GetAllKnownSpells().SequenceEqual(known),
                    ["preparationCount"] = prepared.Length, ["level"] = prepared.FirstOrDefault()?.Spell.SpellLevel },
                    "The real native Extend feat, editor selector, Write and native row preparation create one adjusted original source; no authored MetamagicData/ConvertedFrom is supplied.");
                if (!exact) throw new InvalidOperationException("Native extended preparation is ambiguous.");
                foreach (int frame in screens.Capture("elementals-sources-native-extend-editor", new JObject {
                    ["parent"] = parent.AssetGuid, ["book"] = book.Blueprint.AssetGuid, ["adjustedLevel"] = 3,
                    ["nativeFullscreenChildModeOwned"] = windowStackOwned(), ["manualClockSuspended"] = _sprint25WaitingForUnityAppearance,
                    ["nativeUnitClock"] = "Default gate refuses native owned-unit ticks during this exact native FullScreenUi child; UI/editor callbacks only" },
                    () => editorOwned() && phase.Elapsed.TotalSeconds <= 360 && controller.IsShow && ReferenceEquals(controller.CurrentSpellbook, book))) yield return frame;
                result(created[0]);
            }
            finally
            {
                if (opened)
                {
                    if (!editorOwned())
                        throw new InvalidOperationException("Foreign spellbook controller; not closed.");
                    if (controller.GetComponentsInChildren<SpellBookMetamagicMixer>(true).Any(value => value.IsShowed)) controller.OnSwitchMetamagic();
                    if (controller.IsShow && ui.ServiceWindow.WindowTabs.IsShow) ui.ServiceWindow.HandleOpenSpellbook();
                    bool restored = stack.SequenceEqual(beforeModes) && ((int[])countsField.GetValue(game)).SequenceEqual(beforeCounts) &&
                        mode.Current && ReferenceEquals(game.SelectedAbilityHandler, beforeHandler) && ReferenceEquals(game.ClickEventsController, beforePointer);
                    Cleanup2Check("elemental-sources-native-editor-mode-restored", restored, new JObject {
                        ["stackAndCountsExact"] = stack.SequenceEqual(beforeModes) && ((int[])countsField.GetValue(game)).SequenceEqual(beforeCounts),
                        ["originalDefaultInputControllersExact"] = ReferenceEquals(game.SelectedAbilityHandler, beforeHandler) && ReferenceEquals(game.ClickEventsController, beforePointer),
                        ["manualClockSuspended"] = _sprint25WaitingForUnityAppearance },
                        "The real native service-window close removes only its exact FullScreenUi child and restores Default/input before ordinary casts resume.");
                    if (!restored) throw new InvalidOperationException("Native editor mode/input restoration unverified.");
                }
            }
        }

        private IEnumerable<int> Cleanup2SourcesBookRow(SpellBookController controller, Spellbook book, BlueprintAbility parent,
            int level, bool extend, Action<SpellItem> result)
        {
            // Same actual class/level toggles and row selection as the qualified
            // CircleGroupedMetamagicUi seam, without its Circle-only captures.
            var tabs = controller.SpellBookView.ClassToggle; int index = tabs.Spellbooks.IndexOf(book);
            tabs.GetComponentsInChildren<SpellbookClassTab>(true).Single(value => value.Index == index && value.gameObject.activeInHierarchy).Toggle.isOn = true;
            if (!ReferenceEquals(controller.CurrentSpellbook, book)) throw new InvalidOperationException("Native class tab selected another book.");
            var levelTab = controller.SpellBookView.LevelBookSwitcher.m_Tabs[level];
            if (!levelTab.gameObject.activeInHierarchy || !levelTab.Toggle.interactable) throw new InvalidOperationException("Native metamagic source level unavailable.");
            levelTab.Toggle.isOn = true;
            var expected = (extend ? book.GetCustomSpells(level) : book.GetKnownSpells(level)).Single(value =>
                ReferenceEquals(value.Blueprint, parent) && value.HasMetamagic(Metamagic.Extend) == extend);
            for (int back = 0; controller.CurrentPageIndex > 0 && back < 30; back++) controller.SpellBookView.GoPrevPage();
            if (controller.CurrentPageIndex != 0) throw new InvalidOperationException("Native book pagination origin did not settle.");
            SpellItem row = null;
            for (int page = 0; page < 30; page++)
            {
                if (!ReferenceEquals(controller.CurrentSpellbook, book) || controller.CurrentBookLevel != level || !controller.IsShow)
                    throw new InvalidOperationException("Native elemental source book/page ownership changed.");
                // Native pagination owns the actual rows. The production
                // disclosure owns only visibility, so reveal this exact page's
                // original row through its real button instead of treating an
                // intentionally folded row as an absent known/custom source.
                var view = controller.SpellBookView;
                var fold = view.GetComponent<ElementalNativeSourceFold>();
                var folded = fold == null ? null : typeof(ElementalNativeSourceFold).GetField("_rows", Cleanup2Fields)
                    .GetValue(fold) as ElementalNativeSourceFold.Row[];
                var owned = folded == null ? new ElementalNativeSourceFold.Row[0] : folded.Where(value => value.Current &&
                    value.Component is SpellItem item && ReferenceEquals(item.SpellData, expected)).ToArray();
                if (owned.Length > 1) throw new InvalidOperationException("Native source disclosure owns duplicate original row references.");
                if (owned.Length == 1 && !owned[0].Component.gameObject.activeInHierarchy)
                {
                    int actualPage = controller.CurrentPageIndex;
                    var originalRow = (SpellItem)owned[0].Component;
                    var header = Cleanup2DesktopDisclosure(view);
                    header.onClick.Invoke();
                    for (int frame = 0; frame < 4; frame++) yield return 0;
                    bool revealed = ReferenceEquals(controller.CurrentSpellbook, book) && controller.CurrentBookLevel == level &&
                        controller.CurrentPageIndex == actualPage && originalRow.gameObject.activeInHierarchy &&
                        ReferenceEquals(originalRow.SpellData, expected) && owned[0].Current;
                    Cleanup2Check("elemental-sources-native-book-disclosure-" + (extend ? "extend" : "ordinary"), revealed,
                        new JObject { ["source"] = parent.AssetGuid, ["book"] = book.Blueprint.AssetGuid, ["level"] = level,
                            ["page"] = actualPage, ["exactNativeAbilityData"] = ReferenceEquals(originalRow.SpellData, expected),
                            ["originalRowVisible"] = originalRow.gameObject.activeInHierarchy, ["actualDisclosureButton"] = header.GetInstanceID(),
                            ["directVisibilityWrites"] = 0 },
                        "The actual Elementals disclosure reveals this native page's exact known/custom AbilityData before original-source metamagic selection; no row is unhidden directly.");
                    if (!revealed) throw new InvalidOperationException("Owned native elemental source disclosure did not reveal its exact original row.");
                }
                row = controller.SpellBookView.GetComponentsInChildren<SpellItem>(true).SingleOrDefault(value => value.gameObject.activeInHierarchy &&
                    ReferenceEquals(value.SpellData, expected));
                if (row != null) break;
                controller.SpellBookView.GoNextPage();
                if (controller.CurrentBookLevel != level) break;
            }
            if (row == null) throw new InvalidOperationException("Actual native original elemental book row absent.");
            controller.SpellBookView.ResetSelection(); row.Toggle.isOn = true;
            for (int frame = 0; frame < 6; frame++) yield return 0;
            if (!ReferenceEquals(controller.SelectedSpell, expected) || !ReferenceEquals(row.SpellData, expected) || !ReferenceEquals(row.SpellData.Spellbook, book))
                throw new InvalidOperationException("Native book selection did not retain exact data.");
            result?.Invoke(row);
        }

        private IEnumerable<int> Cleanup2SourcesSpecialAndPin(ExpandedSummoningCorrectionFixture fixture, Spellbook book,
            BlueprintAbility parent, ActionBarGroupElement spellGroup, Cleanup2NativeDefaultScope mode, Stopwatch phase,
            NativeIconScreenEvidence screens, List<Action> removes, Func<bool> uiOwned, ICollection<ElementalChooserSession> ownedSessions)
        {
            Cleanup2SourcesBoundary(mode, phase);
            if (!uiOwned() || ElementalSourceChooserView.IsOpen) throw new InvalidOperationException("Foreign source UI; native special-slot preparation refused.");
            var school = BlueprintLibraryLookup.RequireExact<BlueprintFeature>(BlueprintBootstrap.Library,
                "cee0f7edbd874a042952ee150f878b84", "native Conjuration specialist feature");
            var list = BlueprintLibraryLookup.RequireExact<BlueprintSpellList>(BlueprintBootstrap.Library,
                "69a6eba12bc77ea4191f573d63c9df12", "native Conjuration special list");
            if (fixture.Caster.Descriptor.HasFact(school) || !list.GetSpells(2).Any(value => ReferenceEquals(value, parent)))
                throw new InvalidOperationException("Original elemental has no exact native new Conjuration special-slot access.");
            var fact = fixture.Caster.Descriptor.AddFact(school);
            removes.Add(() => { if (!ReferenceEquals(fixture.Caster.Descriptor.GetFact(school), fact))
                throw new InvalidOperationException("Foreign school feature; not removed."); fixture.Caster.Descriptor.RemoveFact(fact); });
            book.AddSpecialList(list); book.UpdateAllSlotsSize(false);
            foreach (var common in RawSlots(book, 2).Where(value => value.Type == SpellSlotType.Common && value.Spell?.Blueprint == parent).ToArray())
                book.ForgetMemorized(common);
            var favorite = RawSlots(book, 2).Single(value => value.Type == SpellSlotType.Favorite);
            var special = book.GetSpecialSpells(2).Single(value => ReferenceEquals(value.Blueprint, parent));
            if (!book.Memorize(special, favorite)) throw new InvalidOperationException("Native favorite-only memorization refused.");
            book.Rest(); spellGroup.Fill(fixture.Caster);
            foreach (int frame in Cleanup2SettleChooserNativeRefill(fixture, book, spellGroup, mode, uiOwned, ownedSessions,
                ElementalSourceChooserRuntime.Session(spellGroup), "sources-native-favorite", value => { })) yield return frame;
            var session = ElementalSourceChooserRuntime.Session(spellGroup); session.Levels.OnClick(2);
            for (int frame = 0; frame < 8; frame++) yield return 0;
            if (!uiOwned() || !mode.Current || !session.Current) throw new InvalidOperationException("Native favorite source presentation changed after settled level selection.");
            var nativeRow = spellGroup.GetComponentsInChildren<ActionBarGroupSlot>(true).Single(value => value.gameObject.activeInHierarchy &&
                value.MechanicSlot is MechanicActionBarSlotMemorizedSpell &&
                ReferenceEquals(((MechanicActionBarSlotMemorizedSpell)value.MechanicSlot).SpellSlot, favorite));
            bool retained = !session.Sources.Any(value => ReferenceEquals(value.Preparation, favorite)) &&
                !session.Consolidated.Any(value => ReferenceEquals(value, nativeRow.MechanicSlot)) && favorite.Type == SpellSlotType.Favorite;
            Cleanup2Check("elemental-sources-native-favorite-exception", retained, new JObject {
                ["nativeFeature"] = school.AssetGuid, ["nativeSpecialList"] = list.AssetGuid, ["slotType"] = favorite.Type.ToString(),
                ["source"] = favorite.Spell.Blueprint.AssetGuid, ["originalNativeRowRetained"] = retained,
                ["chooserDoesNotClaimSpecialPreparation"] = !session.Sources.Any(value => ReferenceEquals(value.Preparation, favorite)) },
                "A real native specialist-only preparation retains its original native row/mechanism and is not consolidated into ordinary chooser sources.");
            if (!retained) throw new InvalidOperationException("Special-slot exception was hidden or adopted.");
            ActionBarSlot childRow = null; AbilityData child = null; MechanicActionBarSlot childMechanism = null;
            nativeRow.OnToggleGroupClick();
            try
            {
                for (int frame = 0; frame < 8; frame++) yield return 0;
                var popup = (ActionBarSpellsGroup)typeof(ActionBarGroupSlot).GetField("SubGroup", Cleanup2Fields).GetValue(nativeRow);
                var childSpec = StandaloneElementalPresentationPolicy.All.Single(value => value.ParentGuid == parent.AssetGuid && value.CreatureKeys.Contains("small-fire-elemental"));
                childRow = popup.GetComponentsInChildren<ActionBarSlot>(true).Single(value => value.gameObject.activeInHierarchy &&
                    (value.MechanicSlot?.GetContentData() as AbilityData)?.Blueprint.AssetGuid == childSpec.Guid);
                child = (AbilityData)childRow.MechanicSlot.GetContentData();
                childMechanism = childRow.MechanicSlot;
                if (!ReferenceEquals(child.ConvertedFrom, favorite.Spell) || !ReferenceEquals(child.Spellbook, book) ||
                    !Kingmaker.UI.Common.UIUtility.IsTransformInScreen(childRow.transform)) throw new InvalidOperationException("Native special-slot converted row/context is not visible/exact.");
                foreach (int frame in screens.Capture("elementals-sources-native-favorite-exception", new JObject {
                    ["parent"] = parent.AssetGuid, ["child"] = child.Blueprint.AssetGuid, ["slotType"] = favorite.Type.ToString() },
                    () => mode.Current && popup.gameObject.activeInHierarchy && ReferenceEquals(childRow.MechanicSlot.GetContentData(), child) && favorite.Available)) yield return frame;
                foreach (int frame in Cleanup2SourcesCast(fixture, child, favorite, childRow.OnClick, mode, phase, "small-fire-native-favorite", null)) yield return frame;
            }
            finally { Cleanup2MenuAdapterHide(nativeRow, favorite.Spell); }

            // Pin the exact real native converted mechanism just observed;
            // it remains its original definition/source after UI reconstruction.
            book.Rest(); var mechanism = childMechanism;
            if (!ReferenceEquals(mechanism.GetContentData(), child)) throw new InvalidOperationException("Native legacy child mechanism refreshed before pin ownership.");
            var settings = fixture.Caster.UISettings; var slots = settings.Slots; var baseline = new TeleportationUiSettingsFixture(settings);
            var indexField = typeof(ActionBarIndexSlot).GetField("m_Index", Cleanup2Fields);
            var destination = ActionBarManager.Instance.GetComponentsInChildren<ActionBarIndexSlot>(true).First(value => value.gameObject.activeInHierarchy &&
                ReferenceEquals(Cleanup2MenuAdapterSelected(value), fixture.Caster) && (int)indexField.GetValue(value) >= 0 &&
                (int)indexField.GetValue(value) < slots.Length && slots[(int)indexField.GetValue(value)] is MechanicActionBarSlotEmpty);
            int index = (int)indexField.GetValue(destination); var previous = slots[index]; bool requested = false;
            try
            {
                requested = true; destination.TryToSet(mechanism);
                if (!uiOwned() || ElementalSourceChooserView.IsOpen) throw new InvalidOperationException("Foreign source UI; pinned native tab refresh refused.");
                spellGroup.Fill(fixture.Caster);
                foreach (int frame in Cleanup2SettleChooserNativeRefill(fixture, book, spellGroup, mode, uiOwned, ownedSessions,
                    ElementalSourceChooserRuntime.Session(spellGroup), "sources-original-native-pin", value => { })) yield return frame;
                bool pin = ReferenceEquals(settings.Slots, slots) && ReferenceEquals(slots[index], mechanism) && ReferenceEquals(destination.MechanicSlot, mechanism) &&
                    ReferenceEquals(destination.MechanicSlot.GetContentData(), child) && ReferenceEquals(child.ConvertedFrom, favorite.Spell);
                Cleanup2Check("elemental-sources-native-legacy-pin-retained", pin, new JObject {
                    ["index"] = index, ["originalChildGuid"] = child.Blueprint.AssetGuid, ["originalSourceGuid"] = child.ConvertedFrom?.Blueprint.AssetGuid,
                    ["sameNativeMechanismAfterRebuild"] = pin, ["sourceLinks"] = DescribeExpandedSummoningAbilityData(child),
                    ["saveWrite"] = false, ["coldHydration"] = "separate transaction" },
                    "A real native converted child pin keeps its exact original mechanism/child/source across native tab reconstruction, without transient-node serialization.");
                if (!pin) throw new InvalidOperationException("Native legacy child pin changed identity.");
                foreach (int frame in Cleanup2SourcesCast(fixture, child, favorite, destination.OnClick, mode, phase, "small-fire-original-pinned-child", null)) yield return frame;
            }
            finally
            {
                if (requested)
                {
                    if (!ReferenceEquals(settings.Slots, slots) || !ReferenceEquals(Cleanup2MenuAdapterSelected(destination), fixture.Caster) ||
                        (!ReferenceEquals(slots[index], mechanism) && !ReferenceEquals(slots[index], previous)) ||
                        (!ReferenceEquals(destination.MechanicSlot, mechanism) && !ReferenceEquals(destination.MechanicSlot, previous)))
                        throw new InvalidOperationException("Foreign legacy pin replacement; not overwritten.");
                    if (ReferenceEquals(slots[index], mechanism)) settings.SetSlot(previous, index);
                    destination.Set(fixture.Caster);
                    Cleanup2Check("elemental-sources-legacy-pin-restored", baseline.IsRestored(), new JObject { ["exactPinArrayAndMetadata"] = baseline.IsRestored() },
                        "Only this owned native pin write restores; all prior serialized slot references and metadata remain exact.");
                    if (!baseline.IsRestored()) throw new InvalidOperationException("Original pin array restoration unverified.");
                }
            }
        }
    }
}
