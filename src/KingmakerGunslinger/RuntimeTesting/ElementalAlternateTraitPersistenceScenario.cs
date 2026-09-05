using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Class.LevelUp;
using KingmakerGunslinger.ElementalRaces;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal static partial class GunslingerOutfitRenderScenario
    {
        internal sealed partial class ElementalRacePersistenceSession
        {
            private static bool RetainedAlternateTraitsExact(
                UnitDescriptor owner, ElementalPersistenceFixture fixture)
            {
                return owner != null && fixture.Blueprints.AlternateTraits
                    .Selections().All(value => owner.Progression.Features
                        .GetRank(value.RetainMarker) == 1) &&
                    fixture.Blueprints.AlternateTraits.Traits().All(value =>
                        !owner.HasFact(value.Marker) &&
                        !owner.HasFact(value.Provider));
            }

            private static bool NativeRetainedSelectionsExact(JObject record)
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
                            TokenBool(value, "noAlternateProviders") &&
                            value.Value<int>("retainedRank") == 1) &&
                    choices.OfType<JObject>().Select(value =>
                        value.Value<string>("selectionGuid")).Distinct(
                            StringComparer.Ordinal).Count() == expected;
            }

            private static void AddAlternateTraitIdentities(
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
                }
            }

            // The inherited 24-fixture regression retains base racial traits.
            // Choose each new obligatory slot through the actual native
            // level-up controller; do not bypass it with direct AddFact calls.
            private static JArray SelectRetainedAlternateTraits(
                LevelUpController controller,
                ElementalPersistenceFixture fixture, string phase)
            {
                var records = new JArray();
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
                    IFeatureSelectionItem item = items.SingleOrDefault(value =>
                        value != null && ReferenceEquals(value.Feature,
                            choice.RetainMarker));
                    bool selectable = nativeStateExact && item != null &&
                        selection.CanSelect(controller.Preview,
                            controller.State, state, item);
                    bool selected = selectable && controller.SelectFeature(
                        state, item);
                    int retainedRank = controller.Preview.Progression.Features
                        .GetRank(choice.RetainMarker);
                    bool noAlternateProviders = fixture.Blueprints
                        .AlternateTraits.Traits().All(value =>
                            !controller.Preview.HasFact(value.Marker) &&
                            !controller.Preview.HasFact(value.Provider));
                    var record = new JObject
                    {
                        { "phase", phase },
                        { "slot", choice.Definition.Slot.ToString() },
                        { "selectionGuid", selection.AssetGuid },
                        { "retainedMarkerGuid", choice.RetainMarker.AssetGuid },
                        { "nativeStateExact", nativeStateExact },
                        { "menuCount", items.Length },
                        { "menuExact", menuExact },
                        { "selectable", selectable },
                        { "selected", selected },
                        { "retainedRank", retainedRank },
                        { "noAlternateProviders", noAlternateProviders }
                    };
                    records.Add(record);
                    if (!nativeStateExact || !menuExact || !selected ||
                        retainedRank != 1 || !noAlternateProviders)
                        throw new InvalidOperationException(fixture.Label +
                            " failed its native retain-base trait selection: "
                            + record.ToString(Newtonsoft.Json.Formatting.None));
                }
                return records;
            }
        }
    }
}
