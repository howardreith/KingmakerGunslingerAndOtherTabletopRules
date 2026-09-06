using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Class.LevelUp;
using KingmakerGunslinger.ElementalRaces;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal static partial class GunslingerOutfitRenderScenario
    {
        internal sealed partial class ElementalRacePersistenceSession
        {
            private ElementalAlternateTraitBlueprints[] ExpectedPersistenceTraits(
                ElementalPersistenceFixture fixture,
                ElementalHeritageBlueprints heritage, bool includeTraits = true)
            {
                if (_legacyMigration || !includeTraits ||
                    !ReferenceEquals(heritage, fixture.Heritage)) return new ElementalAlternateTraitBlueprints[0];
                int heritageIndex = Array.IndexOf(fixture.Blueprints.Heritages
                    .Choices().ToArray(), heritage);
                ElementalAlternateTraitId[] ids = ElementalBloodInsightPersistencePolicy.Traits(
                    fixture.Blueprints.AlternateTraits.Race,
                    fixture.Gender == Gender.Male ? 0 : 1, heritageIndex);
                return ids.Select(fixture.Blueprints.AlternateTraits.Require).ToArray();
            }

            private ElementalAlternateTraitBlueprints PersistenceSlaTrait(
                ElementalPersistenceFixture fixture, ElementalHeritageBlueprints heritage)
            {
                return ExpectedPersistenceTraits(fixture, heritage).SingleOrDefault(value =>
                    value.Definition.Replaces(ElementalRacialTraitSlot.RacialSpellLikeAbility));
            }

            private BlueprintAbility PersistenceSlaAbility(ElementalPersistenceFixture fixture,
                ElementalHeritageBlueprints heritage)
            {
                ElementalAlternateTraitBlueprints trait = PersistenceSlaTrait(fixture, heritage);
                return trait == null ? heritage.SlaAbility : trait.Mechanics()
                    .OfType<BlueprintAbility>().Single(value => value.Parent == null);
            }

            private BlueprintAbilityResource PersistenceSlaResource(ElementalPersistenceFixture fixture,
                ElementalHeritageBlueprints heritage)
            {
                ElementalAlternateTraitBlueprints trait = PersistenceSlaTrait(fixture, heritage);
                return trait == null ? heritage.SlaResource : trait.Mechanics()
                    .OfType<BlueprintAbilityResource>().Single();
            }

            private static bool TraitProvidersExact(UnitDescriptor owner,
                ElementalPersistenceFixture fixture,
                ICollection<ElementalAlternateTraitBlueprints> desired)
            {
                return owner != null && fixture.Blueprints.AlternateTraits.Traits()
                    .All(value => owner.Progression.Features.GetRank(value.Marker) ==
                            (desired.Contains(value) ? 1 : 0) &&
                        owner.Progression.Features.GetRank(value.Provider) ==
                            (desired.Contains(value) ? 1 : 0)) &&
                    ElementalTraitDailyResourceRuntime.IsExact(owner, fixture.Blueprints.AlternateTraits);
            }

            private static bool LegacyAlternateTraitsAbsent(
                UnitDescriptor owner, ElementalPersistenceFixture fixture)
            {
                return owner != null && fixture.Blueprints.AlternateTraits
                    .Selections().All(value => !owner.HasFact(value.Selection) &&
                        !owner.HasFact(value.RetainMarker)) &&
                    fixture.Blueprints.AlternateTraits.Traits().All(value =>
                        !owner.HasFact(value.Marker) && !owner.HasFact(value.Provider));
            }

            private static bool AlternateTraitsExact(
                UnitDescriptor owner, ElementalPersistenceFixture fixture,
                ICollection<ElementalAlternateTraitBlueprints> desired)
            {
                return owner != null && fixture.Blueprints.AlternateTraits
                    .Selections().All(value => owner.Progression.Features
                        .GetRank(value.RetainMarker) == (desired.Any(trait =>
                            trait.Definition.PrimarySlot == value.Definition.Slot) ? 0 : 1)) &&
                    TraitProvidersExact(owner, fixture, desired);
            }

            private static bool NativeAlternateSelectionsExact(JObject record)
            {
                JArray choices = record == null ? null :
                    record["alternateTraitSelections"] as JArray;
                int expected = record == null ? 0 :
                    record.Value<int>("alternateTraitSelectionsExpected");
                return choices != null && expected > 0 &&
                    choices.Count == expected && choices.OfType<JObject>()
                        .Count() == expected && choices.OfType<JObject>()
                        .All(value => TokenBool(value, "nativeStateExact") &&
                            TokenBool(value, "menuExact") &&
                            TokenBool(value, "selectable") &&
                            TokenBool(value, "selected") &&
                            TokenBool(value, "traitProvidersExact") &&
                            value.Value<int>("selectedRank") == 1 &&
                            value.Value<int>("retainedRank") ==
                                value.Value<int>("expectedRetainedRank")) &&
                    choices.OfType<JObject>().Select(value =>
                        value.Value<string>("selectionGuid")).Distinct(
                            StringComparer.Ordinal).Count() == expected;
            }

            internal static void AddAlternateTraitIdentities(
                ElementalRaceBlueprints race,
                ICollection<BlueprintScriptableObject> owned)
            {
                foreach (ElementalAlternateTraitSelectionBlueprints selection
                    in race.AlternateTraits.Selections())
                {
                    owned.Add(selection.Selection);
                    owned.Add(selection.RetainMarker);
                }
                foreach (ElementalAlternateTraitBlueprints trait in
                    race.AlternateTraits.Traits())
                {
                    owned.Add(trait.Marker);
                    owned.Add(trait.Provider);
                    foreach (BlueprintScriptableObject mechanic in
                        trait.Mechanics())
                        owned.Add(mechanic);
                }
            }

            // Use the real level-up controller for all slot choices. Source
            // creation and restored respec retain base traits; the persisted
            // target uses the explicit seven-trait, disjoint-slot matrix.
            private static JArray SelectAlternateTraits(
                LevelUpController controller,
                ElementalPersistenceFixture fixture, string phase,
                ICollection<ElementalAlternateTraitBlueprints> desired)
            {
                var records = new JArray();
                var selectedTraits = new List<ElementalAlternateTraitBlueprints>();
                foreach (ElementalAlternateTraitSelectionBlueprints choice in
                    fixture.Blueprints.AlternateTraits.Selections())
                {
                    BlueprintFeatureSelection selection = choice.Selection;
                    FeatureSelectionState state = controller.State
                        .FindSelection(selection, true);
                    bool nativeStateExact = state != null &&
                        ReferenceEquals(state.Selection, selection) &&
                        ReferenceEquals(state.Source, fixture.Blueprints.Race)
                        && state.Parent == null && state.Level == 0 &&
                        state.Index == 0 && controller.State.HasSelection(state);
                    IFeatureSelectionItem[] items = selection
                        .ExtractSelectionItems(controller.Preview,
                            controller.Preview).ToArray();
                    BlueprintFeature[] expected = new[] { choice.RetainMarker }
                        .Concat(choice.Choices.Select(value => value.Marker))
                        .ToArray();
                    bool menuExact = items.Select(value => value == null
                        ? null : value.Feature).SequenceEqual(expected);
                    ElementalAlternateTraitBlueprints selectedChoice = desired.SingleOrDefault(value =>
                        value.Definition.PrimarySlot == choice.Definition.Slot);
                    BlueprintFeature expectedMarker = selectedChoice == null
                        ? choice.RetainMarker : selectedChoice.Marker;
                    IFeatureSelectionItem item = items.SingleOrDefault(value =>
                        value != null && ReferenceEquals(value.Feature, expectedMarker));
                    bool selectable = nativeStateExact && item != null &&
                        selection.CanSelect(controller.Preview,
                            controller.State, state, item);
                    bool selected = selectable && controller.SelectFeature(
                        state, item);
                    int retainedRank = controller.Preview.Progression.Features
                        .GetRank(choice.RetainMarker);
                    if (selected && selectedChoice != null)
                        selectedTraits.Add(selectedChoice);
                    int selectedRank = controller.Preview.Progression.Features.GetRank(expectedMarker);
                    int expectedRetainedRank = ReferenceEquals(expectedMarker, choice.RetainMarker) ? 1 : 0;
                    bool traitProvidersExact = TraitProvidersExact(controller.Preview, fixture, selectedTraits);
                    var record = new JObject
                    {
                        { "phase", phase },
                        { "slot", choice.Definition.Slot.ToString() },
                        { "selectionGuid", selection.AssetGuid },
                        { "retainedMarkerGuid", choice.RetainMarker.AssetGuid },
                        { "selectedMarkerGuid", expectedMarker.AssetGuid },
                        { "nativeStateExact", nativeStateExact },
                        { "menuCount", items.Length },
                        { "menuExact", menuExact },
                        { "selectable", selectable },
                        { "selected", selected },
                        { "retainedRank", retainedRank },
                        { "expectedRetainedRank", expectedRetainedRank },
                        { "selectedRank", selectedRank },
                        { "traitProvidersExact", traitProvidersExact }
                    };
                    records.Add(record);
                    if (!nativeStateExact || !menuExact || !selected ||
                        retainedRank != expectedRetainedRank || selectedRank != 1 || !traitProvidersExact)
                        throw new InvalidOperationException(fixture.Label +
                            " failed its exact native alternate-trait selection: "
                            + record.ToString(Newtonsoft.Json.Formatting.None));
                }
                return records;
            }
        }
    }
}
