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
using Kingmaker.EntitySystem.Persistence.JsonUtility;
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
            private readonly int[][] _spontaneous;
            private readonly BlueprintScriptableObject[] _resources;
            private readonly int[] _amounts;
            internal Cleanup2SourceDebit(UnitEntityData actor)
            {
                _actor = actor; _books = actor.Descriptor.Spellbooks.ToArray();
                _slots = _books.Select(book => Enumerable.Range(0, 10).SelectMany(level => RawSlots(book, level)).ToArray()).ToArray();
                _spells = _slots.Select(slots => slots.Select(slot => slot.Spell).ToArray()).ToArray();
                _available = _slots.Select(slots => slots.Select(slot => slot.Available).ToArray()).ToArray();
                _spontaneous = _books.Select(book => book.Blueprint.Spontaneous ? Enumerable.Range(0, 10).Select(book.GetSpontaneousSlots).ToArray() : new int[0]).ToArray();
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
            private bool SpontaneousUnchanged { get { return _books.Select((book, index) => _spontaneous[index].Length == 0 ||
                Enumerable.Range(0, 10).Select(book.GetSpontaneousSlots).SequenceEqual(_spontaneous[index])).All(value => value); } }
            internal bool Unspent { get { return Identities() && SpontaneousUnchanged && _slots.Select((slots, book) =>
                slots.Select((slot, index) => slot.Available == _available[book][index]).All(value => value)).All(value => value); } }
            internal SpellSlot[] Changed { get { return _slots.SelectMany((slots, book) =>
                slots.Where((slot, index) => slot.Available != _available[book][index])).ToArray(); } }
            internal bool ExactlyOne(SpellSlot expected)
            { return Identities() && SpontaneousUnchanged && expected != null && !expected.Available && Changed.SequenceEqual(new[] { expected }) &&
                _slots.Select((slots, book) => slots.Select((slot, index) => !ReferenceEquals(slot, expected) || _available[book][index]).All(value => value)).All(value => value); }
            internal bool ExactlyOneSpontaneous(Spellbook expected, int level)
            {
                int index = Array.IndexOf(_books, expected);
                return Identities() && Changed.Length == 0 && index >= 0 && expected.Blueprint.Spontaneous && level >= 1 && level <= 9 &&
                    _spontaneous[index].Length == 10 && _spontaneous[index][level] > 0 && _books.Select((book, bookIndex) =>
                        _spontaneous[bookIndex].Length == 0 || Enumerable.Range(0, 10).All(rank => book.GetSpontaneousSlots(rank) ==
                            _spontaneous[bookIndex][rank] - (bookIndex == index && rank == level ? 1 : 0))).All(value => value);
            }
            internal JObject Describe(SpellSlot expected)
            { return new JObject { ["exactlyOneOriginalSlotSpent"] = ExactlyOne(expected), ["allBookSlotSpellKnownCustomAndResourceReferencesExact"] = Identities(),
                ["books"] = new JArray(_books.Select(book => book.Blueprint.AssetGuid)), ["nativeSlotCount"] = _slots.Sum(slots => slots.Length),
                ["allSpontaneousBudgetsUnchanged"] = SpontaneousUnchanged,
                ["changed"] = new JArray(Changed.Select(slot => new JObject { ["source"] = slot.Spell?.Blueprint.AssetGuid,
                    ["book"] = slot.Spell?.Spellbook?.Blueprint.AssetGuid, ["level"] = slot.SpellLevel, ["type"] = slot.Type.ToString(),
                    ["expectedExactReference"] = ReferenceEquals(slot, expected), ["available"] = slot.Available })) }; }
            internal JObject DescribeSpontaneous(Spellbook expected, int level)
            { return new JObject { ["exactlyOneOriginalSpontaneousUseSpent"] = ExactlyOneSpontaneous(expected, level),
                ["allBookSlotSpellKnownCustomAndResourceReferencesExact"] = Identities(), ["allPreparedSlotsUnchanged"] = Changed.Length == 0,
                ["expectedBook"] = expected.Blueprint.AssetGuid, ["expectedLevel"] = level,
                ["spontaneousBooks"] = new JArray(_books.Select((book, index) => new { book, index }).Where(value => _spontaneous[value.index].Length != 0)
                    .Select(value => new JObject { ["book"] = value.book.Blueprint.AssetGuid, ["before"] = new JArray(_spontaneous[value.index]),
                        ["after"] = new JArray(Enumerable.Range(0, 10).Select(value.book.GetSpontaneousSlots)) })) }; }
        }

        private IEnumerable<int> ReviewSummonCleanup2ElementalSources(ExpandedSummoningCorrectionFixture fixture, Spellbook book)
        {
            string reviewPhase = _request.Parameters == null ? null : (string)_request.Parameters["reviewPhase"];
            bool pinOnly = reviewPhase == "elementals-pin";
            if (!IsSummonCleanup2Review || !_request.ExitAfterCompletion || _request.Parameters == null ||
                _request.Parameters.Count != 1 || (reviewPhase != "elementals-sources" && !pinOnly) ||
                !ElementalSourceChooserRuntime.Active || _expandedSummoningPlayerPathCaptureActive)
                throw new InvalidOperationException("Exact active, exclusive automatic-exit elementals-sources or focused elementals-pin phase required.");
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
            var fixtureCasterAtIntake = fixture.Caster;
            Cleanup2SourcesLevelUpIntake levelUpIntake = null;
            var fixtureBooksBefore = fixture.Caster.Descriptor.Spellbooks.ToArray();
            var fixtureStatsBefore = new[] { fixture.Caster.Stats.Intelligence.BaseValue, fixture.Caster.Stats.Wisdom.BaseValue, fixture.Caster.Stats.Charisma.BaseValue };
            bool fixtureDetachedBefore = fixture.Caster.IsDetached;
            var extraBooks = new TeleportResourceFixtureOwner(fixture.Caster); var removeFacts = new List<Action>();
            var hooks = HarmonyInstance.Create("kmg.cleanup2.elemental-sources." + _request.RunId);
            var watch = Stopwatch.StartNew(); var casts = new JArray(); bool added = false, selectedFixture = false, bodyCompleted = false;
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
                // Preserve the full source phase's six-rank preparation setup
                // even in the focused pin reproduction. Favorite intentionally
                // removes rank2 Common access; the remaining ordinary ranks
                // keep its real chooser/refill session present. Skip the 24
                // casts below, not these independent fixture preparations.
                foreach (var parent in StandaloneElementalPresentationPolicy.All.Where(value => value.IsParent))
                    PrepareExpandedSummoningPlayerPathSpell(book, Cleanup2ElementalAbility(fixture, parent.Guid),
                        Cleanup2ElementalAbility(fixture, parent.Guid), parent.CanonicalTier);
                var druid = BlueprintLibraryLookup.RequireExact<BlueprintCharacterClass>(BlueprintBootstrap.Library,
                    "610d836f3a3a9ed42a4349b62f002e96", "second actual native prepared spellbook");
                var otherBook = pinOnly ? null : extraBooks.AddBook(druid.Spellbook);
                var small = ElementalSourceChooserPolicy.Rank(2);
                var smallParent = Cleanup2ElementalAbility(fixture, small.Guid);
                if (!pinOnly) { PrepareExpandedSummoningPlayerPathSpell(otherBook, smallParent, smallParent, 2); otherBook.Rest(); }
                Spellbook knownBook = null;
                if (!pinOnly)
                {
                    var sorcerer = BlueprintLibraryLookup.RequireExact<BlueprintCharacterClass>(BlueprintBootstrap.Library,
                        "b3a505fb61437dc4097f43c3f8f9a4cf", "actual native known-spontaneous source book");
                    knownBook = extraBooks.AddBook(sorcerer.Spellbook);
                    if (!knownBook.Blueprint.Spontaneous || knownBook.CasterLevel != 20 ||
                        !knownBook.Blueprint.SpellList.GetSpells(2).Any(value => ReferenceEquals(value, smallParent)) ||
                        knownBook.GetKnownSpells(2).Any(value => ReferenceEquals(value.Blueprint, smallParent)))
                        throw new InvalidOperationException("Independent ordinary native CL20 Sorcerer book and unknown Small Elemental parent required.");
                    // AddKnown06001E56 creates/stores the real AbilityData in
                    // this book. false preserves ordinary known-spell state;
                    // no ConvertedFrom/ParamSpellSlot is manufactured here.
                    knownBook.AddKnown(2, smallParent, false); knownBook.Rest();
                }
                // Capture the retained controller only after all native book
                // creation and known-spell setup has completed.
                levelUpIntake = new Cleanup2SourcesLevelUpIntake(game);
                const BindingFlags statics = BindingFlags.Static | BindingFlags.NonPublic;
                hooks.Patch(typeof(RuleCastSpell).GetMethod("OnTrigger"), null,
                    new HarmonyMethod(typeof(RuntimeTestRunner).GetMethod("ExpandedSummoningPlayerPathRuleCastPostfix", statics)), null);
                hooks.Patch(typeof(ContextActionSpawnMonster).GetMethod("RunAction"),
                    new HarmonyMethod(typeof(RuntimeTestRunner).GetMethod("ExpandedSummoningPlayerPathSpawnPrefix", statics)), null, null);
                _expandedSummoningPlayerPathCaptureActive = true;
                if (!party.Contains(fixture.Caster))
                { player.PartyCharacters.Add(fixture.Caster); added = true; player.InvalidateCharacterLists(); player.UpdateCharacterLists(); }
                mode = Cleanup2EnterNativeDefault(fixture, reviewPhase); screens = new NativeIconScreenEvidence(_request);
                ui.SelectionManagerPC.SelectUnit(fixture.Caster.View, true, true, false); group.SelectUnit(fixture.Caster); selectedFixture = true;
                ownedParty = player.Party.ToArray();
                for (int frame = 0; frame < 12; frame++) yield return 0;
                var spellGroup = groups.Single(value => (ActionBarSlotType)typeof(ActionBarGroupElement).GetField("SlotType", Cleanup2Fields).GetValue(value) == ActionBarSlotType.Spell);
                if (!spellGroup.ToggleState) spellGroup.OnClick();
                for (int frame = 0; frame < 8; frame++) yield return 0;
                if (!pinOnly)
                {
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
                foreach (int frame in Cleanup2SourcesKnownSpontaneousFire(fixture, knownBook, smallParent, spellGroup, mode, watch, uiOwned, ownedSessions)) yield return frame;

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
                }
                using (var specialSteps = Cleanup2SourcesSpecialAndPin(fixture, book, smallParent, spellGroup, mode, watch, screens, removeFacts, uiOwned, ownedSessions, levelUpIntake).GetEnumerator())
                {
                    while (true)
                    {
                        bool next;
                        try { next = specialSteps.MoveNext(); }
                        catch (Exception error)
                        {
                            Cleanup2Check("elemental-sources-special-or-pin-exception", false, new JObject {
                                ["exception"] = error.ToString(), ["all24CastCasesCompleted"] = casts.Count,
                                ["scope"] = "Original immediate special/pin/hydration iterator failure; retained independently of outer cleanup." },
                                "Any original special-slot/pin failure remains visible before teardown and is never replaced by a cleanup diagnostic.");
                            throw;
                        }
                        if (!next) break;
                        yield return specialSteps.Current;
                    }
                }
                Cleanup2Check("elemental-sources-scope", true, new JObject {
                    ["all24OriginalCasts"] = casts.Count, ["maximumPhaseWallSeconds"] = 360, ["wallSeconds"] = watch.Elapsed.TotalSeconds,
                    ["reviewPhase"] = reviewPhase, ["focusedPinOnly"] = pinOnly,
                    ["fixturePreparationRanks"] = new JArray(StandaloneElementalPresentationPolicy.All.Where(value => value.IsParent).Select(value => value.CanonicalTier)),
                    ["knownSpontaneousSmallFire"] = pinOnly ? "NOT_EXECUTED: focused pin reproduction" : "Actual native Sorcerer known source/chooser/command/spawn and one book-level spontaneous debit observed above",
                    ["all24TwoBookSnaExtendDisposition"] = pinOnly ? "NOT_EXECUTED: focused existing Favorite/pin/hydration/original-pin-cast reproduction only" : "Executed above; individual native assertions retained",
                    ["console"] = "separate native controller transaction; not exercised here", ["coldSave"] = "separate explicitly authorized fresh-load transaction; not proved by runtime pins",
                    ["moduleOff"] = "fresh process with module OFF before startup; Active is restart-only, never toggled by reflection here" },
                    "The original-source cast/source-budget slice and separate controller, cold-save and fresh-OFF obligations are explicit.");
                bodyCompleted = true;
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
                    bool extraBookReleaseCalled = false;
                    restore("own-extra-books", () => {
                        JObject casterOwnership;
                        bool ownCaster = Cleanup2SourcesOwnsCaster(fixture, fixtureCasterAtIntake, out casterOwnership);
                        var replacements = ownCaster ? extraBooks.Books.Where(value => fixtureCasterAtIntake.Descriptor.GetSpellbook(value.Blueprint) != null &&
                            !ReferenceEquals(fixtureCasterAtIntake.Descriptor.GetSpellbook(value.Blueprint), value)).ToArray() : new Spellbook[0];
                        Cleanup2Check("elemental-sources-extra-book-cleanup-admission", ownCaster && replacements.Length == 0, new JObject {
                            ["casterOwnership"] = casterOwnership, ["foreignBookReplacements"] = new JArray(replacements.Select(value => value.Blueprint.AssetGuid)),
                            ["bookReferencesReadUnderOwnedCaster"] = ownCaster, ["ownedExtraBookCount"] = extraBooks.Books.Count },
                            "The separately spawned exact disposable caster and exact added book references authorize cleanup; the summon Created list does not contain the caster.");
                        if (!ownCaster || replacements.Length != 0)
                            throw new InvalidOperationException("Foreign extra-book replacement or unowned fixture caster; not overwritten.");
                        extraBooks.Restore(); extraBookReleaseCalled = true;
                    });
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
                    // TeleportResourceFixtureOwner.IsRestored additionally
                    // compares original book CONTENTS. This phase intentionally
                    // prepares/casts/edits its disposable Wizard book. Those
                    // owned experiment changes must not be confused with an
                    // unreleased extra book or protected original-party data.
                    bool fixtureBooksExact = fixture.Caster.Descriptor.Spellbooks.SequenceEqual(fixtureBooksBefore);
                    bool extraBooksAbsent = extraBooks.Books.All(value => fixture.Caster.Descriptor.GetSpellbook(value.Blueprint) == null);
                    bool fixtureStatsExact = new[] { fixture.Caster.Stats.Intelligence.BaseValue, fixture.Caster.Stats.Wisdom.BaseValue, fixture.Caster.Stats.Charisma.BaseValue }
                        .SequenceEqual(fixtureStatsBefore) && fixture.Caster.IsDetached == fixtureDetachedBefore;
                    bool ownedReleased = extraBookReleaseCalled && fixtureBooksExact && extraBooksAbsent && fixtureStatsExact;
                    bool exact = errors.Count == 0 && player.Party.SequenceEqual(party) && ui.SelectionManagerPC.SelectedUnits.SequenceEqual(selection) &&
                        ReferenceEquals(group.GetCurrentCharacter(), originalGroup) && originalInputs.All(value => value.Unchanged()) &&
                        originalBars.All(value => value.IsRestored()) && ownedReleased;
                    Cleanup2Check("elemental-sources-restored", exact, new JObject {
                        ["partyExact"] = player.Party.SequenceEqual(party), ["originalResourcesSlotsPinsExact"] = originalInputs.All(value => value.Unchanged()) && originalBars.All(value => value.IsRestored()),
                        ["ownedFeaturesAndBooksReleased"] = errors.Count == 0 && ownedReleased, ["originalFixtureBookReferencesExact"] = fixtureBooksExact,
                        ["ownedExtraBooksAbsent"] = extraBooksAbsent, ["nativeExtraBookReleaseCompleted"] = extraBookReleaseCalled,
                        ["fixtureStatsAndDetachmentExact"] = fixtureStatsExact, ["bodyCompleted"] = bodyCompleted,
                        ["disposableBookContentScope"] = "Known/custom/prepared contents deliberately exercised inside this disposable caster are released by fixture teardown; every original party resource/slot/pin remains independently exact.",
                        ["errors"] = new JArray(errors) },
                        "Owned native facts/books/pin/chooser/UI release while every original party input and reference remains exact.");
                    if (!exact) errors.Add("Original UI/resource restoration unverified.");
                }
                finally
                {
                    try { if (screens != null) screens.Dispose(); }
                    finally { if (mode != null) mode.Dispose(); }
                }
                // If the body failed (or its iterator is being disposed), its
                // original error must propagate. Cleanup has its own FAIL row;
                // throwing here would erase the primary native failure.
                if (errors.Count != 0 && bodyCompleted) throw new InvalidOperationException(string.Join("|", errors.ToArray()));
            }
        }

        private void Cleanup2SourcesBoundary(Cleanup2NativeDefaultScope mode, Stopwatch phase)
        {
            if (phase.Elapsed.TotalSeconds > 360 || !mode.Current || !Cleanup2NoNarrativeOverlay() || Game.Instance.IsControllerGamepad)
                throw new InvalidOperationException("Native elemental source phase exceeded wall/input/ownership bound.");
        }

        private static bool Cleanup2SourcesOwnsCaster(ExpandedSummoningCorrectionFixture fixture, UnitEntityData expected, out JObject evidence)
        {
            var caster = fixture?.Caster;
            // BeginExpandedSummoningCorrectionFixture spawns Caster separately;
            // Created holds additional summons. Its teardown explicitly uses
            // Created.Concat(Caster,Hostile), never Created membership as caster
            // authority. Preserve the exact clone/view/census provenance here.
            evidence = new JObject {
                ["sameIntakeCasterReference"] = caster != null && ReferenceEquals(caster, expected),
                ["liveCaster"] = caster != null && !caster.Destroyed,
                ["exactOwnedBlueprintReference"] = caster != null && fixture.CasterBlueprint != null && ReferenceEquals(caster.Blueprint, fixture.CasterBlueprint),
                ["blueprintIsDisposableClone"] = fixture?.CasterBlueprint != null && !ReferenceEquals(fixture.CasterBlueprint, BlueprintRoot.Instance.DefaultPlayerCharacter),
                ["originalUnitSnapshotPresent"] = fixture?.UnitsBefore != null,
                ["notOriginalUnit"] = caster != null && fixture.UnitsBefore != null && !fixture.UnitsBefore.Any(value => ReferenceEquals(value, caster)),
                ["originalPartySnapshotPresent"] = fixture?.PartyBefore != null,
                ["notOriginalPartyMember"] = caster != null && fixture.PartyBefore != null && !fixture.PartyBefore.Any(value => ReferenceEquals(value, caster)),
                ["exactLiveViewData"] = caster?.View != null && ReferenceEquals(caster.View.EntityData, caster),
                ["exactFixtureScene"] = caster != null && fixture.Scene != null && ReferenceEquals(caster.HoldingState, fixture.Scene),
                ["registeredInNativeUnitCensus"] = caster != null && fixture.AllUnits != null && ContainsReference(fixture.AllUnits, caster) };
            bool owned = evidence.Properties().All(value => (bool)value.Value);
            evidence["summonCreatedListContainsCaster"] = caster != null && fixture.Created.Contains(caster);
            evidence["casterOwnedSeparatelyFromCreatedSummons"] = owned;
            return owned;
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

        private IEnumerable<int> Cleanup2SourcesKnownSpontaneousFire(ExpandedSummoningCorrectionFixture fixture, Spellbook book, BlueprintAbility parent,
            ActionBarGroupElement group, Cleanup2NativeDefaultScope mode, Stopwatch phase,
            Func<bool> uiOwned, ICollection<ElementalChooserSession> ownedSessions)
        {
            Cleanup2SourcesBoundary(mode, phase);
            if (!uiOwned() || ElementalSourceChooserView.IsOpen || book == null || !book.Blueprint.Spontaneous ||
                !ReferenceEquals(book.Owner, fixture.Caster.Descriptor))
                throw new InvalidOperationException("Exact owned ordinary native spontaneous book/UI required.");
            var known = book.GetKnownSpells(2).Where(value => ReferenceEquals(value.Blueprint, parent)).ToArray();
            if (known.Length != 1 || known[0].IsSpellCopy || known[0].ConvertedFrom != null || known[0].ParamSpellSlot != null ||
                known[0].SourceItem != null || !ReferenceEquals(known[0].Spellbook, book) || book.GetSpontaneousSlots(2) < 1)
                throw new InvalidOperationException("One native ordinary known source and its own positive level2 spontaneous budget required.");
            group.Fill(fixture.Caster);
            foreach (int frame in Cleanup2SettleChooserNativeRefill(fixture, book, group, mode, uiOwned, ownedSessions,
                ElementalSourceChooserRuntime.Session(group), "sources-known-spontaneous-small-fire", value => { })) yield return frame;
            var source = Cleanup2SourcesFind(group, book, "small-fire-elemental", false);
            bool exact = source.Preparation == null && source.Mechanism is MechanicActionBarSlotSpontaneusSpell &&
                ReferenceEquals(source.Mechanism.GetContentData(), known[0]) && ReferenceEquals(source.Source, known[0]) &&
                ReferenceEquals(source.Child.ConvertedFrom, known[0]) && ReferenceEquals(source.Child.Spellbook, book) &&
                source.Child.ParamSpellSlot == null && source.Validate() == null;
            Cleanup2Check("elemental-sources-native-known-spontaneous-context", exact, new JObject {
                ["book"] = book.Blueprint.AssetGuid, ["source"] = parent.AssetGuid, ["child"] = source.Child.Blueprint.AssetGuid,
                ["nativeMechanism"] = source.Mechanism.GetType().FullName, ["nativeKnownObjectExact"] = ReferenceEquals(source.Source, known[0]),
                ["nativeSourceIdentity"] = System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(known[0]),
                ["noPreparedSlotOrSubstitute"] = source.Preparation == null && source.Child.ParamSpellSlot == null,
                ["spontaneousLevel2Before"] = book.GetSpontaneousSlots(2),
                ["nativeContract"] = "AddKnown06001E56 stores AbilityData in the actual book. Native MechanicActionBarSlotSpontaneusSpell.GetConvertedAbilityData06002FE4 creates its child and original ConvertedFrom link. Spellbook.SpendInternal06001E6E IL0092–00BE decrements only m_SpontaneousSlots[GetSpellLevel(source)].",
                ["fixtureAccess"] = "One explicitly added native Sorcerer known spell/book on the disposable caster, not natural Sorcerer progression or old-save access." },
                "The actual native spontaneous mechanism and its native child retain the exact known-spell/book source; no preparation or invocation chain is manufactured.");
            if (!exact) throw new InvalidOperationException("Native spontaneous source context differs from its actual known spell.");
            foreach (int frame in Cleanup2SourcesReady(group, source, mode, phase)) yield return frame;
            foreach (int frame in Cleanup2SourcesCast(fixture, source.Child, null,
                () => ElementalSourceChooserView.ReviewChoose(2, source.CreatureKey, source, true),
                mode, phase, "small-fire-native-known-spontaneous", null, book)) yield return frame;
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
            Action activate, Cleanup2NativeDefaultScope mode, Stopwatch phase, string label, Action<JObject> result, Spellbook knownSourceBook = null)
        {
            Cleanup2SourcesBoundary(mode, phase);
            var snapshot = new Cleanup2SourceDebit(fixture.Caster); var original = data.ConvertedFrom; var book = data.Spellbook;
            var blueprint = data.Blueprint; var meta = data.MetamagicData; bool extend = data.HasMetamagic(Metamagic.Extend);
            bool spontaneous = preparation == null && knownSourceBook != null && ReferenceEquals(knownSourceBook, book) &&
                book.Blueprint.Spontaneous && original != null && book.GetKnownSpells(original.SpellLevel).Any(value => ReferenceEquals(value, original)) &&
                original.ConvertedFrom == null && original.SourceItem == null && original.ParamSpellSlot == null && data.ParamSpellSlot == null &&
                book.GetSpontaneousSlots(original.SpellLevel) > 0;
            var expected = SummonSignatureReader.Read(blueprint).Spawns().ToArray();
            bool prepared = knownSourceBook == null && preparation != null && ReferenceEquals(preparation.Spell, original) && preparation.Available;
            if (expected.Length != 1 || original == null || (!prepared && !spontaneous) || !ReferenceEquals(data.Caster, fixture.Caster.Descriptor))
                throw new InvalidOperationException("Exact one-spawn original prepared or explicitly observed native known-spontaneous source required: " + label);
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
                    ReferenceEquals(data.Spellbook, book) && ReferenceEquals(data.MetamagicData, meta) &&
                    (spontaneous ? book.GetKnownSpells(original.SpellLevel).Any(value => ReferenceEquals(value, original)) : ReferenceEquals(preparation.Spell, original));
                bool pass = issued && noSpendBeforeTarget && command.IsFinished && command.Result.ToString() == "Success" && fixture.Caster.Commands.Empty &&
                    _expandedSummoningPlayerPathRuleCastCount == 1 && _expandedSummoningPlayerPathSpawnActionCount == 1 &&
                    units.Length == 1 && ExpandedSummoningRuleCapture.Distinct().Count() == 1 && units[0].Blueprint.AssetGuid == expected[0].Unit &&
                    durations.Length == 1 && lifecycle.Length == 1 && (!extend || lifecycle[0].Context.HasMetamagic(Metamagic.Extend)) &&
                    (spontaneous ? snapshot.ExactlyOneSpontaneous(book, original.SpellLevel) : snapshot.ExactlyOne(preparation)) && links;
                var row = new JObject { ["case"] = label, ["source"] = original.Blueprint.AssetGuid, ["child"] = blueprint.AssetGuid,
                    ["book"] = book.Blueprint.AssetGuid, ["sourceDataIdentity"] = System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(original),
                    ["childDataIdentity"] = System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(data), ["sourceLinksExact"] = links,
                    ["nativeCommandExact"] = issued, ["commandResult"] = command?.Result.ToString(), ["selectionSpentNothing"] = noSpendBeforeTarget,
                    ["nativeAction"] = data.RuntimeActionType.ToString(), ["fullRound"] = data.RequireFullRoundAction, ["extend"] = extend,
                    ["ruleCasts"] = _expandedSummoningPlayerPathRuleCastCount, ["spawnActions"] = _expandedSummoningPlayerPathSpawnActionCount,
                    ["expectedUnit"] = expected[0].Unit, ["actualUnit"] = units.FirstOrDefault()?.Blueprint.AssetGuid,
                    ["baseDurationSeconds"] = durations.Length == 1 ? (JToken)durations[0].BaseDuration.TotalSeconds : JValue.CreateNull(),
                    ["bonusDurationSeconds"] = durations.Length == 1 ? (JToken)durations[0].BonusDuration.TotalSeconds : JValue.CreateNull(),
                    ["nativeBudgetKind"] = spontaneous ? "known-spontaneous-book-level-use" : "original-prepared-slot",
                    ["debit"] = spontaneous ? snapshot.DescribeSpontaneous(book, original.SpellLevel) : snapshot.Describe(preparation),
                    ["dataLinks"] = DescribeExpandedSummoningAbilityData(data),
                    ["wallSeconds"] = watch.Elapsed.TotalSeconds, ["manualTicks"] = 0, ["forcedAct"] = false };
                Cleanup2Check("elemental-sources-cast-" + label, pass, row,
                    "Actual native source click/target/command produces its exact provenance-owned summon and exactly its original preparation or spontaneous book-level debit across all books/resources.");
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
                throw new InvalidOperationException("Foreign Extend feature; not removed."); fixture.Caster.Descriptor.RemoveFact(extend);
                if (!extend.IsDisposed || fixture.Caster.Descriptor.HasFact(extendBlueprint)) throw new InvalidOperationException("Owned Extend fact did not release exactly."); });
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
            NativeIconScreenEvidence screens, List<Action> removes, Func<bool> uiOwned, ICollection<ElementalChooserSession> ownedSessions,
            Cleanup2SourcesLevelUpIntake levelUpIntake)
        {
            Cleanup2SourcesBoundary(mode, phase);
            if (!uiOwned() || ElementalSourceChooserView.IsOpen) throw new InvalidOperationException("Foreign source UI; native special-slot preparation refused.");
            var school = BlueprintLibraryLookup.RequireExact<BlueprintFeature>(BlueprintBootstrap.Library,
                "cee0f7edbd874a042952ee150f878b84", "native Conjuration specialist feature");
            var list = BlueprintLibraryLookup.RequireExact<BlueprintSpellList>(BlueprintBootstrap.Library,
                "69a6eba12bc77ea4191f573d63c9df12", "native Conjuration special list");
            var specialListsField = typeof(Spellbook).GetField("m_SpecialLists", Cleanup2Fields);
            var specialLists = specialListsField?.GetValue(book) as List<BlueprintSpellList>;
            bool existingSchool = fixture.Caster.Descriptor.HasFact(school);
            bool parentInList = list.GetSpells(2).Any(value => ReferenceEquals(value, parent));
            Cleanup2Check("elemental-sources-native-favorite-intake", !existingSchool && parentInList && specialLists != null, new JObject {
                ["school"] = school.AssetGuid, ["alreadyHasSchool"] = existingSchool, ["parentInNativeSpecialList"] = parentInList,
                ["nativeSpecialLists"] = specialLists == null ? null : new JArray(specialLists.Select(value => value.AssetGuid)),
                ["exactNativeSchoolListComponents"] = new JArray(school.GetComponents<AddSpecialSpellList>().Select(value => new JObject {
                    ["class"] = value.CharacterClass?.AssetGuid, ["list"] = value.SpellList?.AssetGuid })) },
                "The native school/list/fixture intake is recorded before any specialist mutation, preserving the exact reason for refusal.");
            if (existingSchool || !parentInList || specialLists == null)
                throw new InvalidOperationException("Original elemental has no exact native new Conjuration special-slot access.");
            var fact = fixture.Caster.Descriptor.AddFact(school);
            removes.Add(() => { if (!ReferenceEquals(fixture.Caster.Descriptor.GetFact(school), fact))
                throw new InvalidOperationException("Foreign school feature; not removed."); fixture.Caster.Descriptor.RemoveFact(fact);
                if (!fact.IsDisposed || fixture.Caster.Descriptor.HasFact(school)) throw new InvalidOperationException("Owned school fact did not release exactly."); });
            // Installed AddSpecialSpellList.OnFactActivate06002537 already
            // calls AddSpecialList for a real class book. AddSpecialList06001E5A
            // and AddSpecial06001E59 append without deduplication; repeating it
            // would duplicate the source and make the exact Single below fail.
            bool nativeFeatureAddedList = specialLists.Any(value => ReferenceEquals(value, list));
            if (!nativeFeatureAddedList) book.AddSpecialList(list);
            bool exactSpecialList = ReferenceEquals(specialListsField.GetValue(book), specialLists) &&
                specialLists.Count(value => ReferenceEquals(value, list)) == 1 &&
                book.GetSpecialSpells(2).Count(value => ReferenceEquals(value.Blueprint, parent)) == 1;
            Cleanup2Check("elemental-sources-native-favorite-list-once", exactSpecialList, new JObject {
                ["nativeFeatureAddedList"] = nativeFeatureAddedList, ["observerNativeAddSpecialListCalls"] = nativeFeatureAddedList ? 0 : 1,
                ["exactListOccurrences"] = specialLists.Count(value => ReferenceEquals(value, list)),
                ["exactParentSpecialObjects"] = book.GetSpecialSpells(2).Count(value => ReferenceEquals(value.Blueprint, parent)) },
                "The exact native school/list activation supplies one real specialist source; the observer never appends it a second time.");
            if (!exactSpecialList) throw new InvalidOperationException("Native Conjuration specialist list/source is not exact and unique.");
            book.UpdateAllSlotsSize(false);
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
                Cleanup2SourcesHydrateLegacyPin(fixture, book, favorite, mechanism, index, uiOwned, levelUpIntake);
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

        // UIAccess.LevelUpController is a retained plain field. Native
        // Start06002835 assigns it; Commit0600285D/Cancel0600285E dispose
        // their detached Preview and Stop the producer without clearing it.
        // AutoCommit .ctor06002832 instead aliases Preview=Unit and neither
        // Commit nor Cancel disposes that owner. Its retained field can be
        // inert only after the exact owner itself is natively destroyed,
        // viewless and absent from party/world; a live alias is never admitted.
        // AdvanceDisposableSpellcaster uses StartWithoutAssigningStaticInstance
        // 06002836. Never cancel/clear an intake controller to make it null.
        private sealed class Cleanup2SourcesLevelUpIntake
        {
            private readonly Game _game;
            private readonly Kingmaker.UI.UIAccess _ui;
            private readonly Kingmaker.UI.LevelUp.CharacterBuildController _build;
            private readonly Kingmaker.UnitLogic.Class.LevelUp.LevelUpController _controller;
            private readonly UnitDescriptor _unit, _preview;
            private readonly object _state, _actions, _callback;
            private readonly object[] _actionItems;
            private readonly FieldInfo _onSuccess;
            private readonly FieldInfo _producerLock, _producerSource, _producerPending, _producerQueue, _producerThread;
            private readonly object _intakeThread;
            private readonly bool _gamepad;
            private readonly bool _autoCommit;
            private readonly Func<JObject> _closedConsoleOwner;

            internal Cleanup2SourcesLevelUpIntake(Game game, Func<JObject> closedConsoleOwner = null)
            {
                _game = game; _ui = game.UI; _build = _ui.CharacterBuildController; _controller = _ui.LevelUpController;
                _gamepad = game.IsControllerGamepad; _closedConsoleOwner = closedConsoleOwner;
                _autoCommit = _controller != null && _controller.AutoCommit;
                _unit = _controller?.Unit; _preview = _controller?.Preview;
                _state = _controller?.State; _actions = _controller?.LevelUpActions;
                _actionItems = _controller?.LevelUpActions.Cast<object>().ToArray();
                _onSuccess = typeof(Kingmaker.UnitLogic.Class.LevelUp.LevelUpController).GetField("m_OnSuccess", Cleanup2Fields);
                _callback = _controller == null || _onSuccess == null ? null : _onSuccess.GetValue(_controller);
                Type producer = typeof(Kingmaker.UnitLogic.Class.LevelUp.LevelUpPreviewThread);
                const BindingFlags statics = BindingFlags.Static | BindingFlags.NonPublic;
                _producerLock = producer.GetField("s_Lock", statics); _producerSource = producer.GetField("s_Source", statics);
                _producerPending = producer.GetField("s_RequestPending", statics); _producerQueue = producer.GetField("s_Queue", statics);
                _producerThread = producer.GetField("s_Thread", statics); _intakeThread = _producerThread?.GetValue(null);
            }

            internal bool Current(out JObject evidence)
            {
                bool fields = _onSuccess != null && _producerLock != null && _producerSource != null &&
                    _producerPending?.FieldType == typeof(bool) && _producerQueue != null && _producerThread != null;
                bool locked = false, producerIdle = false; int queued = -1; bool? pending = null; object source = null;
                object sync = fields ? _producerLock.GetValue(null) : null;
                try
                {
                    // Read only under the producer's own lock, without waiting
                    // for or stopping a producer that belongs to another UI.
                    if (sync != null && System.Threading.Monitor.TryEnter(sync))
                    {
                        locked = true; source = _producerSource.GetValue(null); pending = (bool)_producerPending.GetValue(null);
                        var queue = _producerQueue.GetValue(null) as System.Collections.ICollection;
                        queued = queue == null ? -1 : queue.Count;
                        producerIdle = source == null && pending == false && queued == 0 && ReferenceEquals(_producerThread.GetValue(null), _intakeThread);
                    }
                }
                finally { if (locked) System.Threading.Monitor.Exit(sync); }
                bool controllerExact = ReferenceEquals(_ui.LevelUpController, _controller);
                bool disposedEntity = _preview?.Unit != null && _preview.Unit.Destroyed && _preview.Unit.View == null &&
                    !_game.Player.Party.Any(value => ReferenceEquals(value, _preview.Unit)) &&
                    !_game.State.Units.Any(value => ReferenceEquals(value, _preview.Unit));
                bool detachedPreview = _controller != null && !_autoCommit && !ReferenceEquals(_preview, _unit) && disposedEntity;
                bool destroyedAutoCommitOwner = _controller != null && _autoCommit && ReferenceEquals(_preview, _unit) && disposedEntity;
                bool completed = _controller == null || detachedPreview || destroyedAutoCommitOwner;
                bool contextExact = _controller == null || (ReferenceEquals(_controller.Unit, _unit) && ReferenceEquals(_controller.Preview, _preview) &&
                    _controller.AutoCommit == _autoCommit &&
                    ReferenceEquals(_controller.State, _state) && ReferenceEquals(_controller.LevelUpActions, _actions) &&
                    _actionItems != null && _controller.LevelUpActions.Cast<object>().SequenceEqual(_actionItems) && ReferenceEquals(_onSuccess?.GetValue(_controller), _callback));
                JObject consoleOwner = _gamepad && _closedConsoleOwner != null ? _closedConsoleOwner() : null;
                bool consoleExact = consoleOwner != null && (bool?)consoleOwner["passed"] == true;
                var predicates = new JObject {
                    ["exactIntakeGameUi"] = ReferenceEquals(Game.Instance, _game) && ReferenceEquals(_game.UI, _ui),
                    ["inputModeUnchanged"] = _game.IsControllerGamepad == _gamepad,
                    ["positiveNativeConsoleCreatorOwner"] = !_gamepad || consoleExact,
                    ["exactIntakeCharacterBuild"] = ReferenceEquals(_ui.CharacterBuildController, _build) && (_build != null || (_gamepad && consoleExact)),
                    ["characterBuildHidden"] = _build != null ? !_build.IsShow : _gamepad && consoleExact,
                    ["noCharacterBuildController"] = _build != null ? _build.LevelUpController == null : _gamepad && consoleExact,
                    ["nativeDefaultModeCurrent"] = _game.CurrentMode == GameModeType.Default,
                    ["exactIntakeGlobalController"] = controllerExact,
                    ["noControllerOrDisposedDetachedPreview"] = completed,
                    ["intakeControllerContextUnchanged"] = contextExact,
                    ["nativeProducerFieldContracts"] = fields,
                    ["nativeProducerReadLockAcquired"] = locked,
                    ["nativeProducerIdleAndThreadUnchanged"] = producerIdle };
                bool current = predicates.Properties().All(value => (bool)value.Value);
                evidence = new JObject { ["predicates"] = predicates,
                    ["rejectedPredicates"] = new JArray(predicates.Properties().Where(value => !(bool)value.Value).Select(value => value.Name)),
                    ["controllerPresentAtIntake"] = _controller != null,
                    ["intakeControllerIdentity"] = _controller == null ? (JToken)JValue.CreateNull() : System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(_controller),
                    ["currentControllerIdentity"] = _ui.LevelUpController == null ? (JToken)JValue.CreateNull() : System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(_ui.LevelUpController),
                    ["controllerUnit"] = _unit?.Unit?.UniqueId, ["controllerPreviewUnit"] = _preview?.Unit?.UniqueId,
                    ["previewDestroyed"] = _preview?.Unit?.Destroyed, ["controllerAutoCommit"] = _controller?.AutoCommit,
                    ["completionDisposition"] = _controller == null ? "no-retained-controller" : detachedPreview ? "disposed-independent-native-preview" :
                        destroyedAutoCommitOwner ? "destroyed-native-autocommit-owner" : "refused-live-or-unclassified-controller",
                    ["completionPredicates"] = new JObject { ["autoCommitModeUnchanged"] = _controller == null || _controller.AutoCommit == _autoCommit,
                        ["previewAliasesControllerUnit"] = _preview != null && ReferenceEquals(_preview, _unit),
                        ["previewUnitPresent"] = _preview?.Unit != null, ["previewUnitDestroyed"] = _preview?.Unit?.Destroyed == true,
                        ["previewViewAbsent"] = _preview?.Unit != null && _preview.Unit.View == null,
                        ["previewAbsentFromParty"] = _preview?.Unit != null && !_game.Player.Party.Any(value => ReferenceEquals(value, _preview.Unit)),
                        ["previewAbsentFromWorld"] = _preview?.Unit != null && !_game.State.Units.Any(value => ReferenceEquals(value, _preview.Unit)),
                        ["disposedIndependentPreview"] = detachedPreview, ["destroyedAutoCommitOwner"] = destroyedAutoCommitOwner },
                    ["previewProducerSourcePresent"] = source != null, ["previewRequestPending"] = pending, ["queuedPreviews"] = queued,
                    ["closedNativeConsoleOwner"] = consoleOwner,
                    ["globalFieldOrControllerWritten"] = false, ["producerStoppedOrWritten"] = false,
                    ["contract"] = "Only the exact intake field is preserved. NonAutoCommit requires Commit/Cancel's disposed independent preview. AutoCommit .ctor06002832 aliases Preview=Unit, so only that exact natively destroyed/viewless owner absent from party/world is inert. Mode, unit/preview/state/actions/callback stay exact; active builders, producer work and replacements refuse. No controller or producer is written/stopped." };
                return current;
            }
        }

        // Installed 2.1.7b: UnitSerialization.Serialize 0600828E and the
        // LevelUpPreviewThread.Run 0600286B ToObject<UnitDescriptor> call,
        // followed by LevelUpController.RequestPreview 0600283C's exact three
        // calls: Buffs.SetupPreview, Unit.PostLoad, Unit.TurnOn. Retain the
        // detached copy BEFORE those hooks so any failed hook can be disposed.
        // No preview thread, UI controller, save file or detached cast is used.
        private void Cleanup2SourcesHydrateLegacyPin(ExpandedSummoningCorrectionFixture fixture, Spellbook book,
            SpellSlot preparation, MechanicActionBarSlot mechanism, int index, Func<bool> uiOwned, Cleanup2SourcesLevelUpIntake levelUpIntake)
        {
            var game = Game.Instance; var original = fixture.Caster.Descriptor; var ui = game.UI;
            var backpack = typeof(UnitUISettings).GetField("OnChangedBackpackVisibility", Cleanup2Fields);
            var helmet = typeof(UnitUISettings).GetField("OnChangedHelmetVisibility", Cleanup2Fields);
            var serializationCount = typeof(UnitSerialization).GetField("s_Count", BindingFlags.Static | BindingFlags.NonPublic);
            bool fieldsExact = backpack != null && helmet != null && serializationCount != null && serializationCount.FieldType == typeof(int);
            Func<bool> serializationIdle = () => fieldsExact && (int)serializationCount.GetValue(null) == 0;
            JObject casterOwnership; bool ownCaster = Cleanup2SourcesOwnsCaster(fixture, original.Unit, out casterOwnership);
            var registeredCasterBlueprint = ResourcesLibrary.TryGetBlueprint<BlueprintUnit>(original.Blueprint.AssetGuid);
            JObject levelUpReceipt; bool levelUpIdle = levelUpIntake.Current(out levelUpReceipt);
            bool pinIndex = original.UISettings.Slots != null && index >= 0 && index < original.UISettings.Slots.Length;
            var admission = new JObject {
                ["nativeCallbackAndSerializationFieldsExact"] = fieldsExact, ["nativeSerializationIdle"] = serializationIdle(),
                ["exactFixtureUiOwned"] = uiOwned(), ["exactCompletedNativeLevelUpIntakeAndIdleProducer"] = levelUpIdle,
                ["originalCasterTurnedOn"] = original.IsTurnedOn, ["exactFixtureCasterOwned"] = ownCaster,
                ["noForeignMaster"] = original.Master.Value == null, ["noPet"] = original.Pet == null,
                ["nativePinIndexValid"] = pinIndex, ["exactNativePinnedMechanism"] = pinIndex && ReferenceEquals(original.UISettings.Slots[index], mechanism),
                ["exactNativeMechanismUnit"] = mechanism != null && ReferenceEquals(mechanism.Unit, fixture.Caster),
                ["exactBookOwner"] = book != null && ReferenceEquals(book.Owner, original),
                ["nativeCasterBlueprintGuidResolves"] = registeredCasterBlueprint != null && registeredCasterBlueprint.AssetGuid == original.Blueprint.AssetGuid };
            string[] rejected = admission.Properties().Where(value => !(bool)value.Value).Select(value => value.Name).ToArray();
            Cleanup2Check("elemental-sources-native-hydration-admission", rejected.Length == 0, new JObject {
                ["predicates"] = admission, ["rejectedPredicates"] = new JArray(rejected), ["casterOwnership"] = casterOwnership,
                ["nativeLevelUpIntake"] = levelUpReceipt,
                ["unit"] = fixture.Caster.UniqueId, ["pinIndex"] = index, ["book"] = book.Blueprint.AssetGuid,
                ["master"] = original.Master.Value?.UniqueId, ["pet"] = original.Pet?.UniqueId,
                ["saveWrite"] = false, ["coldLoad"] = false },
                "Every original native pin/fixture/serialization/owner admission predicate is recorded separately before any serialization or refusal.");
            if (rejected.Length != 0) throw new InvalidOperationException("Native hydration admission refused: " + string.Join(",", rejected));
            var backpackBefore = backpack.GetValue(original.UISettings); var helmetBefore = helmet.GetValue(original.UISettings);
            Func<bool> callbacksExact = () => ReferenceEquals(backpack.GetValue(original.UISettings), backpackBefore) &&
                ReferenceEquals(helmet.GetValue(original.UISettings), helmetBefore);
            var debit = new Cleanup2SourceDebit(fixture.Caster); var pins = new TeleportationUiSettingsFixture(original.UISettings);
            var originalBooks = original.Spellbooks.ToArray(); var party = game.Player.Party.ToArray(); var world = game.State.Units.ToArray();
            var child = mechanism.GetContentData() as AbilityData;
            if (child == null || !ReferenceEquals(child.ConvertedFrom, preparation.Spell) || !ReferenceEquals(child.Spellbook, book) ||
                child.ParamSpellSlot != null || child.ParamSpellbook != null || preparation.Spell.ParamSpellSlot != null || preparation.Spell.ParamSpellbook != null)
                throw new InvalidOperationException("Exact native persisted pin/source graph without unserialized invocation parameters required.");
            var source = child.ConvertedFrom; var meta = child.MetamagicData; var sourceMeta = source.MetamagicData;
            var title = mechanism.GetTitle(); var icon = mechanism.GetIcon();
            int originalBookIndex = Array.IndexOf(originalBooks, book);
            int originalSlotIndex = Array.IndexOf(Enumerable.Range(0, 10).SelectMany(level => RawSlots(book, level)).ToArray(), preparation);
            if (originalBookIndex < 0 || originalSlotIndex < 0) throw new InvalidOperationException("Pinned native preparation is outside its original book.");
            UnitDescriptor preview = null; JToken token = null;
            bool initializationStarted = false; Exception failure = null; var watch = Stopwatch.StartNew();
            try
            {
                token = UnitSerialization.Serialize(original);
                if (!debit.Unspent || !pins.IsRestored() || !original.IsTurnedOn || !uiOwned())
                    throw new InvalidOperationException("Native preview serialization changed the original fixture context.");
                preview = token.ToObject<UnitDescriptor>();
                if (preview == null || ReferenceEquals(preview, original) || preview.Unit == null || ReferenceEquals(preview.Unit, fixture.Caster) ||
                    preview.Unit.View != null || game.Player.Party.Any(value => ReferenceEquals(value, preview.Unit)) ||
                    game.State.Units.Any(value => ReferenceEquals(value, preview.Unit)))
                    throw new InvalidOperationException("Native deserialization did not return a separate detached owned graph.");
                initializationStarted = true;
                preview.Buffs.SetupPreview(preview);
                preview.Unit.PostLoad();
                preview.Unit.TurnOn();
                bool ownership = levelUpIntake.Current(out levelUpReceipt) && uiOwned() && preview != null && !ReferenceEquals(preview, original) &&
                    !ReferenceEquals(preview.Unit, fixture.Caster) && preview.Unit.View == null &&
                    game.Player.Party.SequenceEqual(party) && game.State.Units.SequenceEqual(world) &&
                    !party.Any(value => ReferenceEquals(value, preview.Unit)) && !world.Any(value => ReferenceEquals(value, preview.Unit));
                if (!ownership) throw new InvalidOperationException("Native hydrated preview is not an exclusive detached fixture copy.");
                var restoredBooks = preview.Spellbooks.ToArray();
                bool books = restoredBooks.Length == originalBooks.Length && originalBooks.Select((value, bookIndex) =>
                    Cleanup2SourcesHydratedBook(value, restoredBooks[bookIndex], preview)).All(value => value);
                var restoredBook = restoredBooks.Length > originalBookIndex ? restoredBooks[originalBookIndex] : null;
                var restoredSlots = restoredBook == null ? new SpellSlot[0] : Enumerable.Range(0, 10).SelectMany(level => RawSlots(restoredBook, level)).ToArray();
                var restoredPreparation = originalSlotIndex < restoredSlots.Length ? restoredSlots[originalSlotIndex] : null;
                var restoredPin = preview.UISettings.Slots != null && index < preview.UISettings.Slots.Length ? preview.UISettings.Slots[index] : null;
                var restoredChild = restoredPin?.GetContentData() as AbilityData;
                bool sourceChain = restoredPreparation != null && restoredChild != null &&
                    !ReferenceEquals(restoredPin, mechanism) && restoredPin.GetType() == mechanism.GetType() &&
                    ReferenceEquals(restoredPin.Unit, preview.Unit) && ReferenceEquals(preview.UISettings.Owner, preview) &&
                    ReferenceEquals(restoredChild.ConvertedFrom, restoredPreparation.Spell) &&
                    Cleanup2SourcesHydratedAbility(child, restoredChild, preview) &&
                    ReferenceEquals(restoredChild.Spellbook, restoredBook) && restoredChild.ParamSpellSlot == null && restoredChild.ParamSpellbook == null &&
                    restoredPreparation.Type == preparation.Type && restoredPreparation.SpellLevel == preparation.SpellLevel &&
                    restoredPreparation.Available == preparation.Available && restoredPreparation.Index == preparation.Index;
                bool presentation = restoredPin != null && !restoredPin.IsBad() && restoredPin.GetTitle() == title && ReferenceEquals(restoredPin.GetIcon(), icon);
                // Native BlueprintConverter0600825F writes GUID only and
                // ReadJson0600825E resolves ResourcesLibrary.TryGetBlueprint.
                // The request's temporary unregistered caster clone therefore
                // hydrates to that exact registered blueprint, not the clone.
                bool ids = preview.Unit.UniqueId == fixture.Caster.UniqueId && preview.Blueprint.AssetGuid == original.Blueprint.AssetGuid &&
                    ReferenceEquals(preview.Blueprint, registeredCasterBlueprint);
                bool unchanged = debit.Unspent && pins.IsRestored() && callbacksExact() && original.IsTurnedOn &&
                    ReferenceEquals(child.ConvertedFrom, source) && ReferenceEquals(child.MetamagicData, meta) && ReferenceEquals(source.MetamagicData, sourceMeta);
                bool pass = books && sourceChain && presentation && ids && unchanged && watch.Elapsed.TotalSeconds < 8;
                Cleanup2Check("elemental-sources-native-owning-graph-hydration", pass, new JObject {
                    ["nativePath"] = "UnitSerialization.Serialize -> JToken.ToObject<UnitDescriptor> -> exact native preview Buffs.SetupPreview/Unit.PostLoad/Unit.TurnOn",
                    ["originalUnitId"] = fixture.Caster.UniqueId, ["hydratedUnitId"] = preview.Unit.UniqueId,
                    ["child"] = restoredChild?.Blueprint.AssetGuid, ["source"] = restoredChild?.ConvertedFrom?.Blueprint.AssetGuid,
                    ["book"] = restoredBook?.Blueprint.AssetGuid, ["pinIndex"] = index, ["pinNativeType"] = restoredPin?.GetType().FullName,
                    ["allKnownCustomAndPreparedGraphs"] = books, ["sameHydratedPreparedSourceReference"] = sourceChain,
                    ["nativeTitleAndIconExact"] = presentation, ["stableUnitAndBlueprintIds"] = ids,
                    ["temporaryCasterCloneInstance"] = original.Blueprint.GetInstanceID(), ["hydratedCasterBlueprintInstance"] = preview.Blueprint.GetInstanceID(),
                    ["hydratedCasterBlueprintIsExactRegisteredReference"] = ReferenceEquals(preview.Blueprint, registeredCasterBlueprint),
                    ["casterBlueprintSerialization"] = "Native GUID reference conversion; request-only unregistered clone identity is not persisted or registered. Released child/source/book references remain exact.",
                    ["originalReferencesResourcesAndPinsExact"] = unchanged, ["previewDetachedFromPartyAndWorld"] = ownership,
                    ["originalUiCallbacksExact"] = callbacksExact(), ["uiControllerUnchanged"] = levelUpIntake.Current(out levelUpReceipt),
                    ["nativeLevelUpIntake"] = levelUpReceipt,
                    ["wallSeconds"] = watch.Elapsed.TotalSeconds,
                    ["saveWrite"] = false, ["coldLoad"] = false, ["oldSaveFixture"] = false, ["hydratedPreviewCast"] = false,
                    ["limits"] = "Native in-memory hydration and native title/icon getters; no old-format save, fresh-process load, rendered cloned action bar, or cast from the detached duplicate-ID preview. The original live native pin is cast separately." },
                    "The original native pin and known/custom/prepared graph survive the installed native serializer and preview post-load lifecycle without migrating their definitions.");
                if (!pass) throw new InvalidOperationException("Native legacy owning-graph hydration failed.");
            }
            catch (Exception error)
            {
                failure = error;
                Cleanup2Check("elemental-sources-native-hydration-exception", false, new JObject {
                    ["exception"] = error.ToString(), ["nativeTokenCreated"] = token != null, ["detachedCopyRetained"] = preview != null,
                    ["nativePostLoadInitializationStarted"] = initializationStarted, ["saveWrite"] = false, ["coldLoad"] = false },
                    "The original native serialization/initialization failure remains recorded independently of cleanup.");
                throw;
            }
            finally
            {
                // The copy is retained before PostLoad/TurnOn. No shared native
                // preview thread/source or UI callback is cleared on failure.
                Exception cleanupError = null;
                try
                {
                    if (preview != null)
                    {
                        if (ReferenceEquals(preview, original) || preview.Unit == null || ReferenceEquals(preview.Unit, fixture.Caster) || preview.Unit.View != null ||
                            game.Player.Party.Any(value => ReferenceEquals(value, preview.Unit)) || game.State.Units.Any(value => ReferenceEquals(value, preview.Unit)))
                            throw new InvalidOperationException("Hydrated copy is no longer exclusively detached; foreign/attached unit disposal refused.");
                        preview.Unit.Dispose();
                    }
                }
                catch (Exception error) { cleanupError = error; }
                // UnitSerialization itself restores its own counter in finally,
                // but its TurnOn is after serialization, not in that finally.
                try
                { JObject stillOwned;
                    if (ReferenceEquals(Game.Instance, game) && ReferenceEquals(game.UI, ui) && Cleanup2SourcesOwnsCaster(fixture, original.Unit, out stillOwned) && !original.IsTurnedOn)
                        original.TurnOn(); }
                catch (Exception error) { cleanupError = cleanupError == null ? error : new AggregateException(cleanupError, error); }
                bool previewDisposed = preview == null || preview.Unit != null && preview.Unit.Destroyed;
                bool restored = cleanupError == null && serializationIdle() && previewDisposed && debit.Unspent && pins.IsRestored() && callbacksExact() &&
                    original.IsTurnedOn && ReferenceEquals(Game.Instance, game) && ReferenceEquals(game.UI, ui) && levelUpIntake.Current(out levelUpReceipt) &&
                    uiOwned() && game.Player.Party.SequenceEqual(party) && game.State.Units.SequenceEqual(world) &&
                    ReferenceEquals(child.ConvertedFrom, source) && ReferenceEquals(child.MetamagicData, meta) && ReferenceEquals(source.MetamagicData, sourceMeta);
                Cleanup2Check("elemental-sources-native-hydration-restored", restored, new JObject {
                    ["ownedPreviewDestroyed"] = previewDisposed, ["nativePostLoadInitializationStarted"] = initializationStarted,
                    ["nativeSerializationScopeExited"] = serializationIdle(), ["previewThreadOrUiControllerWritten"] = false,
                    ["originalResourcesAndSourceReferencesExact"] = debit.Unspent, ["originalPinsExact"] = pins.IsRestored(),
                    ["originalTurnedOn"] = original.IsTurnedOn, ["originalUiCallbacksExact"] = callbacksExact(), ["noSavedPartyRecordLoaded"] = true,
                    ["cleanupError"] = cleanupError?.ToString(),
                    ["nativeLevelUpIntake"] = levelUpReceipt,
                    ["saveWrite"] = false, ["coldLoad"] = false },
                    "Only the retained detached native copy is disposed, including after a failed initialization hook; original live source/pin/book/party/input context remains exact for its independent actual cast.");
                if (!restored)
                {
                    var error = new InvalidOperationException("Native hydration preview cleanup/original context unverified.", cleanupError);
                    if (failure != null) throw new AggregateException(failure, error);
                    throw error;
                }
            }
        }

        private static bool Cleanup2SourcesHydratedAbility(AbilityData original, AbilityData restored, UnitDescriptor owner, int depth = 0)
        {
            if (original == null || restored == null) return original == null && restored == null;
            return depth < 5 && !ReferenceEquals(original, restored) && original.GetType() == restored.GetType() &&
                ReferenceEquals(original.Blueprint, restored.Blueprint) && ReferenceEquals(restored.Caster, owner) &&
                (original.Fact == null ? restored.Fact == null : restored.Fact != null && !ReferenceEquals(original.Fact, restored.Fact) &&
                    ReferenceEquals(original.Fact.Blueprint, restored.Fact.Blueprint) && ReferenceEquals(restored.Fact.Owner, owner)) &&
                Equals(original.MetamagicData, restored.MetamagicData) && original.OverrideDC == restored.OverrideDC &&
                original.OverrideSpellLevel == restored.OverrideSpellLevel && original.DecorationColorNumber == restored.DecorationColorNumber &&
                original.DecorationBorderNumber == restored.DecorationBorderNumber && original.IsSpellCopy == restored.IsSpellCopy &&
                (original.Spellbook == null ? restored.Spellbook == null : restored.Spellbook != null &&
                    !ReferenceEquals(original.Spellbook, restored.Spellbook) && ReferenceEquals(original.Spellbook.Blueprint, restored.Spellbook.Blueprint) &&
                    ReferenceEquals(restored.Spellbook.Owner, owner)) &&
                Cleanup2SourcesHydratedAbility(original.ConvertedFrom, restored.ConvertedFrom, owner, depth + 1);
        }

        private static bool Cleanup2SourcesHydratedBook(Spellbook original, Spellbook restored, UnitDescriptor owner)
        {
            if (restored == null || ReferenceEquals(original, restored) || !ReferenceEquals(original.Blueprint, restored.Blueprint) || !ReferenceEquals(restored.Owner, owner)) return false;
            var known = original.GetAllKnownSpells().ToArray(); var restoredKnown = restored.GetAllKnownSpells().ToArray();
            var custom = Enumerable.Range(0, 10).SelectMany(original.GetCustomSpells).ToArray();
            var restoredCustom = Enumerable.Range(0, 10).SelectMany(restored.GetCustomSpells).ToArray();
            var slots = Enumerable.Range(0, 10).SelectMany(level => RawSlots(original, level)).ToArray();
            var restoredSlots = Enumerable.Range(0, 10).SelectMany(level => RawSlots(restored, level)).ToArray();
            return known.Length == restoredKnown.Length && custom.Length == restoredCustom.Length && slots.Length == restoredSlots.Length &&
                known.Select((value, index) => Cleanup2SourcesHydratedAbility(value, restoredKnown[index], owner)).All(value => value) &&
                custom.Select((value, index) => Cleanup2SourcesHydratedAbility(value, restoredCustom[index], owner)).All(value => value) &&
                slots.Select((value, index) => {
                    var copy = restoredSlots[index];
                    bool links = value.LinkedSlots == null ? copy.LinkedSlots == null : copy.LinkedSlots != null &&
                        value.LinkedSlots.Length == copy.LinkedSlots.Length && value.LinkedSlots.Select((link, linkIndex) =>
                            link == null ? copy.LinkedSlots[linkIndex] == null : Array.IndexOf(slots, link) >= 0 &&
                                ReferenceEquals(copy.LinkedSlots[linkIndex], restoredSlots[Array.IndexOf(slots, link)])).All(exact => exact);
                    return !ReferenceEquals(value, copy) && value.SpellLevel == copy.SpellLevel && value.Type == copy.Type && value.Index == copy.Index &&
                        value.Available == copy.Available && value.IsOpposition == copy.IsOpposition && links && Cleanup2SourcesHydratedAbility(value.Spell, copy.Spell, owner);
                }).All(value => value);
        }
    }
}
