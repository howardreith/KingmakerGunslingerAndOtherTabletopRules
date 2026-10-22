using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Harmony12;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UI.ActionBar;
using Kingmaker.UI.Group;
using Kingmaker.UI.UnitSettings;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.FactLogic;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json.Linq;
using TMPro;
using UnityEngine;

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
            var addedBooks = new TeleportResourceFixtureOwner(fixture.Caster);
            var watch = Stopwatch.StartNew(); bool added = false, selectedFixture = false;
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
                nativeMode = Cleanup2EnterNativeDefault(fixture, "menu-adapters");
                // These are non-casting presentation observations. Pausing the
                // owned native mode keeps availability unchanged during IO.
                game.IsPaused = true;
                screens = new NativeIconScreenEvidence(_request);
                ui.SelectionManagerPC.SelectUnit(fixture.Caster.View, true, true, false); group.SelectUnit(fixture.Caster);
                selectedFixture = true;
                for (int frame = 0; frame < 12; frame++) yield return 0;
                var spellGroup = groups.Single(value => (ActionBarSlotType)typeof(ActionBarGroupElement)
                    .GetField("SlotType", Cleanup2Fields).GetValue(value) == ActionBarSlotType.Spell);
                var inputs = new Cleanup2TooltipInputs(fixture.Caster);
                var fixturePins = new TeleportationUiSettingsFixture(fixture.Caster.UISettings);
                var rows = new JArray();
                foreach (bool known in new[] { false, true })
                foreach (var target in targets)
                {
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
                        source.OnToggleGroupClick();
                        for (int frame = 0; frame < 8; frame++) yield return 0;
                        bool capture = !known && target.Spec.Multiplicity == SummonMultiplicity.One;
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
                foreach (int frame in Cleanup2MenuAdapterPin(fixture, book, spellGroup, pinTarget, nativeMode, watch, screens)) yield return frame;
                Cleanup2Check("menu-adapter-pin-array-restored", fixturePins.IsRestored(),
                    new JObject { ["exactOriginalSlotArrayAndContents"] = fixturePins.IsRestored() },
                    "The native pin probe restores the entire fixture hotbar array and metadata before the next book/feature grant.");
                if (!fixturePins.IsRestored()) throw new InvalidOperationException("Native pin probe changed unrelated hotbar state.");
                foreach (int frame in Cleanup2MenuAdapterDruid(fixture, addedBooks, spellGroup, nativeMode, watch, screens,
                    value => removeConversion = value)) yield return frame;
                // Actual console activation/learning is owned by a separate
                // native input transaction. No hand-built selector VM is proof.
                Cleanup2Check("menu-adapters-surface-limits", true, new JObject {
                    ["desktopCases"] = rows.Count, ["console"] = "unqualified: this request owns desktop input only; no console surface/VM was constructed or activated",
                    ["oldSave"] = "unqualified: fixture-established known/prepared/pinned objects are not cold-save hydration",
                    ["castCostsAndSummons"] = "unqualified: targeted native menu/targeting observation, not execution acceptance",
                    ["maximumWallSeconds"] = 240, ["wallSeconds"] = watch.Elapsed.TotalSeconds },
                    "The supported desktop slice and distinct console, persistence and execution limits remain explicit.");
            }
            finally
            {
                var errors = new List<string>();
                Action<string, Action> restore = (name, action) => { try { action(); } catch (Exception error) { errors.Add(name + ":" + error.Message); } };
                try
                {
                    restore("conversion-feature", () => removeConversion?.Invoke());
                    restore("owned-books", addedBooks.Restore);
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
                    bool exact = errors.Count == 0 && player.Party.SequenceEqual(party) &&
                        ui.SelectionManagerPC.SelectedUnits.SequenceEqual(selected) && ReferenceEquals(group.GetCurrentCharacter(), selectedGroup) &&
                        originalInputs.All(value => value.Unchanged()) && originalBars.All(value => value.IsRestored()) && addedBooks.IsRestored();
                    Cleanup2Check("menu-adapters-owned-ui-restored", exact, new JObject {
                        ["partyExact"] = player.Party.SequenceEqual(party), ["selectionExact"] = ui.SelectionManagerPC.SelectedUnits.SequenceEqual(selected),
                        ["originalInputsAndPinsExact"] = originalInputs.All(value => value.Unchanged()) && originalBars.All(value => value.IsRestored()),
                        ["ownedBooksRemoved"] = addedBooks.IsRestored(), ["errors"] = new JArray(errors) },
                        "Only this disposable caster's owned books/fact/pin and native UI are changed; original party resources and pins restore exactly.");
                }
                finally
                {
                    try { if (screens != null) screens.Dispose(); }
                    finally { if (nativeMode != null) nativeMode.Dispose(); }
                }
                if (errors.Count != 0) throw new InvalidOperationException("Menu adapter cleanup unverified: " + string.Join("|", errors.ToArray()));
            }
        }

        private void Cleanup2MenuAdapterBoundary(Cleanup2NativeDefaultScope mode, Stopwatch watch)
        {
            if (watch.Elapsed.TotalSeconds > 240 || !mode.Current || !Cleanup2NoNarrativeOverlay() ||
                Game.Instance.IsControllerGamepad || Game.Instance.SelectedAbilityHandler.Ability != null)
                throw new InvalidOperationException("Native adapter wall/input/narrative ownership boundary changed.");
        }

        private static void Cleanup2MenuAdapterHide(ActionBarGroupSlot source, AbilityData original)
        {
            if (source == null || !ReferenceEquals(source.MechanicSlot?.GetContentData(), original) ||
                Game.Instance.UI.TooltipsController.ContextTooltipData != null)
                throw new InvalidOperationException("Foreign source/tooltip; popup teardown refused.");
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
                var evidence = new JObject { ["surface"] = label, ["sourceMechanism"] = source.MechanicSlot.GetType().FullName,
                    ["source"] = original.Blueprint.AssetGuid, ["child"] = data.Blueprint.AssetGuid,
                    ["book"] = data.Spellbook?.Blueprint.AssetGuid, ["sourceDataIdentity"] = System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(original),
                    ["childDataIdentity"] = System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(data),
                    ["sourceLinkExact"] = ReferenceEquals(data.ConvertedFrom, original), ["nativeTitle"] = data.Name,
                    ["expectedTitle"] = expected, ["renderedText"] = new JArray(row.GetComponentsInChildren<TextMeshProUGUI>(true).Select(value => value.text)),
                    ["iconExact"] = ReferenceEquals(row.Icon.sprite, target.Child.Icon), ["fullyVisible"] = Cleanup2FullyVisible(scroll, row),
                    ["nativeAvailability"] = data.GetAvailableForCastCount(), ["nativeAction"] = data.RuntimeActionType.ToString(),
                    ["nativeFullRound"] = data.RequireFullRoundAction, ["dataLinks"] = DescribeExpandedSummoningAbilityData(data) };
                bool pass = owns() && Cleanup2FullyVisible(scroll, row) && data.Name == expected && target.Child.Name == expected &&
                    ReferenceEquals(row.Icon.sprite, target.Child.Icon);
                Cleanup2Check("menu-adapter-row-" + label, pass, evidence,
                    "The actual native prepared/known/pinned source produces one exact Horse quantity row with current title/icon and unchanged source/book/availability.");
                if (!pass) throw new InvalidOperationException("Native adapter row failed: " + label);
                if (cases != null) cases.Add(evidence);
                if (capture) foreach (int frame in screens.Capture("menu-adapter-" + label, evidence, owns)) yield return frame;
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
            var features = druid.Progression.LevelEntries.SelectMany(value => value.Features).Where(value => value != null &&
                value.ComponentsArray.OfType<SpontaneousSpellConversion>().Any(component => ReferenceEquals(component.CharacterClass, druid) &&
                    component.SpellsByLevel != null && component.SpellsByLevel.Length >= 10 && Enumerable.Range(1, 9).All(level =>
                        component.SpellsByLevel[level]?.AssetGuid == ExpandedSummoningInventoryObserver.CanonicalParentGuids[9 + level - 1]))).Distinct().ToArray();
            if (features.Length != 1 || fixture.Caster.Descriptor.HasFact(features[0]))
                throw new InvalidOperationException("Exact unique native Druid SNA conversion feature not available for owned fixture grant.");
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
            var evidence = new JObject { ["class"] = druid.AssetGuid, ["book"] = book.Blueprint.AssetGuid, ["nativeFeature"] = feature.AssetGuid,
                ["source"] = prepared.Blueprint.AssetGuid, ["sourceSlotIdentity"] = System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(prepared.ParamSpellSlot),
                ["factoryModuleMvid"] = typeof(MechanicActionBarSlotMemorizedSpell).Module.ModuleVersionId.ToString(),
                ["factoryToken"] = typeof(MechanicActionBarSlotMemorizedSpell).GetMethod("GetConvertedAbilityData").MetadataToken,
                ["patchOwners"] = new JArray(patches == null ? new string[0] : patches.Prefixes.Concat(patches.Postfixes).Concat(patches.Transpilers).Select(value => value.owner).Distinct().ToArray()),
                ["conversions"] = new JArray(conversions.Select(value => new JObject { ["blueprint"] = value.Blueprint.AssetGuid,
                    ["sourceExact"] = ReferenceEquals(value.ConvertedFrom, prepared), ["bookExact"] = ReferenceEquals(value.Spellbook, book),
                    ["variants"] = value.Blueprint.Variants?.Length ?? 0, ["dataLinks"] = DescribeExpandedSummoningAbilityData(value) })),
                ["fixtureAccess"] = "Real native Druid book and exact class-progression conversion feature granted to disposable caster. Not a naturally levelled Druid or saved character." };
            var inputs = new Cleanup2TooltipInputs(fixture.Caster); AbilityData selected = null;
            try
            {
                source.OnToggleGroupClick();
                for (int frame = 0; frame < 8; frame++) yield return 0;
                var popup = (ActionBarSpellsGroup)typeof(ActionBarGroupSlot).GetField("SubGroup", Cleanup2Fields).GetValue(source);
                if (popup == null || !popup.gameObject.activeInHierarchy) throw new InvalidOperationException("Actual Druid conversion popup absent.");
                var rows = popup.GetComponentsInChildren<ActionBarSlot>(true).Where(value => value.gameObject.activeInHierarchy &&
                    (value.MechanicSlot?.GetContentData() as AbilityData)?.ConvertedFrom == prepared).ToArray();
                var exact = rows.Where(value => ReferenceEquals((value.MechanicSlot.GetContentData() as AbilityData)?.Blueprint, parent)).ToArray();
                bool factoryExact = conversions.Any(value => ReferenceEquals(value.Blueprint, parent) && ReferenceEquals(value.ConvertedFrom, prepared) && ReferenceEquals(value.Spellbook, book));
                var variants = parent.Variants ?? new BlueprintAbility[0];
                bool flatFactory = variants.Length != 0 && variants.All(variant => conversions.Count(value =>
                    ReferenceEquals(value.Blueprint, variant) && ReferenceEquals(value.ConvertedFrom, prepared) && ReferenceEquals(value.Spellbook, book)) == 1);
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
                evidence["nestedVariantRoute"] = flatRows ? "actual installed factory/widget expansion; native Horse visibility and targeting checked separately" :
                    exact.Length == 1 && exact[0] is ActionBarGroupSlot ? "native-group-present; not navigated in this slice" :
                    "unqualified: parent-only native conversion row is not a nested group; no variant links fabricated";
                bool contract = (factoryExact && exact.Length == 1 || flatRows && horseRows.Length == 1) && inputs.Unchanged();
                Cleanup2Check("menu-adapter-druid-native-conversion-contract", contract, evidence,
                    "The actual Druid prepared-slot factory exposes either canonical SNA II or its complete installed native variant expansion with original source/book links; unsupported routing remains explicit.");
                if (!contract) throw new InvalidOperationException("Installed native Druid conversion contract differs; inspect recorded factory/patch/rows.");
                Func<bool> owns = () => mode.Current && Cleanup2NoNarrativeOverlay() && inputs.Unchanged() &&
                    Game.Instance.SelectedAbilityHandler.Ability == null &&
                    ReferenceEquals(source.MechanicSlot?.GetContentData(), prepared) && popup.gameObject.activeInHierarchy;
                foreach (int frame in screens.Capture("menu-adapter-druid-native-conversion", evidence, owns)) yield return frame;
                // Stock native row OnClick passes this exact parent to the
                // targeting handler. Owner-anchored immediate casts are not
                // exercised here; no pending command is invented or advanced.
                var clickedRow = flatRows && horseRows.Length == 1 ? horseRows[0] : exact[0];
                selected = (AbilityData)clickedRow.MechanicSlot.GetContentData();
                string anchor = selected.TargetAnchor.ToString();
                // No scrolling helper is borrowed from a canonical-parent
                // popup when this source is an unrelated prepared spell.
                // An installed expansion outside the screen is recorded, not
                // made reachable with an invented viewport.
                bool visible = Kingmaker.UI.Common.UIUtility.IsTransformInScreen(clickedRow.transform);
                bool mayTarget = visible && selected.TargetAnchor != AbilityTargetAnchor.Owner && !(clickedRow is ActionBarGroupSlot);
                if (mayTarget) clickedRow.OnClick();
                bool handed = mayTarget && ReferenceEquals(Game.Instance.SelectedAbilityHandler.Ability, selected) && fixture.Caster.Commands.Empty;
                Cleanup2Check("menu-adapter-druid-parent-targeting", !mayTarget || handed,
                    new JObject { ["targetAnchor"] = anchor, ["rowClicked"] = mayTarget, ["exactNativeParentSelected"] = handed,
                        ["parent"] = parent.AssetGuid, ["selectedAbility"] = selected.Blueprint.AssetGuid,
                        ["nativeRowOnScreen"] = visible, ["slotAvailable"] = prepared.ParamSpellSlot.Available,
                        ["sourceStillExact"] = ReferenceEquals(selected.ConvertedFrom, prepared),
                        ["variantChoice"] = flatRows ? "installed complete native variant expansion; only this pending Horse targeting handoff observed, not cast/debit" :
                            "unqualified: parent handoff is not evidence that Horse/other nested variants can be selected",
                        ["disposition"] = mayTarget ? "observed-real-row-targeting; no target submitted" : "unqualified: no safe pending-target-only route" },
                    "A safe real conversion-row click hands its exact native parent to targeting without submitting a target or spending; incompatible immediate/nested routes remain unqualified.");
            }
            finally
            {
                if (selected != null && ReferenceEquals(Game.Instance.SelectedAbilityHandler?.Ability, selected))
                    Game.Instance.SelectedAbilityHandler.DropAbility();
                Cleanup2MenuAdapterHide(source, prepared);
                Cleanup2Check("menu-adapter-druid-no-spend", inputs.Unchanged(), inputs.Describe(),
                    "Cancelling only this exact native targeting object preserves all native preparations, resource objects and amounts.");
            }
        }
    }
}
