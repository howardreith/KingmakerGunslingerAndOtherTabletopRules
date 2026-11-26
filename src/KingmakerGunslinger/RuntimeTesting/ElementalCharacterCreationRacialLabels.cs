using System.Linq;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.UI.LevelUp;
using Kingmaker.UI.ServiceWindow.CharacterScreen;
using Kingmaker.UnitLogic;
using KingmakerGunslinger.ElementalRaces;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.RuntimeTesting
{
    // Rendered racial-bonus labels in the actual native creator ability-score
    // panel and on the committed character sheet. Expected labels derive from the
    // selected heritage's final racial modifiers (0 => "-"), never from the label
    // code under test.
    internal sealed partial class ElementalCharacterCreationBaselineScenario
    {
        private int _racialLabelWait;

        // The committed-sheet capture admits only the registered native
        // CustomCompanion that the rolled loaded-game creator route produces.
        private bool RacialLabelSheetCase =>
            _request.Scenario == RuntimeTestScenarioCatalog.WorkingSaveElementalCharacterCreationRegression &&
            (string)_request.Parameters["allocation"] == "roll";

        private ElementalHeritageBlueprints SelectedRegressionHeritage()
        {
            ElementalRaceBlueprints race = RegressionRace;
            var selected = ExactOwnedSelection(race.Heritages.Selection).SelectedItem?.Feature;
            return race.Heritages.Choices().SingleOrDefault(value => ReferenceEquals(value.Marker, selected)) ??
                race.Heritages.General;
        }

        private static int[] HeritageRacial(ElementalHeritageBlueprints heritage)
        {
            return Enumerable.Range(0, 6).Select(index =>
                heritage.Definition.ModifierFor((ElementalHeritageStat)index)).ToArray();
        }

        /// <summary>
        /// Called once allocation is complete in the native Skills phase (initial
        /// pass and every post-revision pass). Returns false while the native panel
        /// has not yet bound the current preview; records and continues otherwise.
        /// </summary>
        private bool RenderedCreatorRacialLabelsObserved()
        {
            if (!_regression) return true;
            UnitDescriptor preview = _controller.Preview;
            var entries = _build.GetComponentsInChildren<CharBScoresEntry>(true)
                .Where(value => value.gameObject.activeInHierarchy).ToArray();
            bool bound = entries.Length == AbilityStats.Length && AbilityStats.All(stat =>
                entries.Count(value => value.Type == stat && ReferenceEquals(value.Stat, preview.Stats.GetStat(stat))) == 1);
            if (!bound && ++_racialLabelWait < 45) { _settle = 2; return false; }
            _racialLabelWait = 0;
            ElementalHeritageBlueprints heritage = SelectedRegressionHeritage();
            int[] expected = HeritageRacial(heritage);
            bool exact = bound;
            var rendered = new JArray();
            for (int index = 0; index < AbilityStats.Length; index++)
            {
                StatType stat = AbilityStats[index];
                var entry = entries.FirstOrDefault(value => value.Type == stat);
                string label = entry?.RaceBonus?.text;
                string wanted = ElementalRacialLabelObservation.ExpectedLabel(expected[index]);
                bool same = label == wanted;
                exact &= same;
                rendered.Add(new JObject { ["stat"] = stat.ToString(), ["renderedRaceBonus"] = label,
                    ["renderedValue"] = entry?.Value?.text, ["expectedRaceBonus"] = wanted, ["exact"] = same });
            }
            bool probeExact;
            JObject probe = ElementalRacialLabelObservation.Observe(preview, expected, null, out probeExact);
            exact &= probeExact;
            var observation = new JObject { ["checkpoint"] = "native-ability-score-panel", ["revision"] = _revision,
                ["heritage"] = heritage.Definition.Id.ToString(), ["markerGuid"] = heritage.Marker.AssetGuid,
                ["previewBound"] = bound, ["activeEntries"] = entries.Length, ["rendered"] = rendered,
                ["stats"] = probe, ["exact"] = exact };
            if (_character["renderedRacialLabels"] == null) _character["renderedRacialLabels"] = new JArray();
            ((JArray)_character["renderedRacialLabels"]).Add(observation);
            if (!exact)
                AcceptanceFailure("Rendered creator racial labels diverged for " + heritage.Definition.Id +
                    " at revision " + _revision + ": " + rendered.ToString(Newtonsoft.Json.Formatting.None));
            Write();
            return true;
        }

        /// <summary>
        /// At every native final review: the labels the native score-entry code
        /// produces for the current preview. The native creator can skip the
        /// ability-score phase after a revision that leaves skill points unchanged
        /// (Smokesoul -> Stormsoul), so this covers every planned heritage state.
        /// </summary>
        private void ObserveFinalReviewRacialLabels()
        {
            if (!_regression) return;
            ElementalHeritageBlueprints heritage = SelectedRegressionHeritage();
            bool exact;
            JObject probe = ElementalRacialLabelObservation.Observe(_controller.Preview, HeritageRacial(heritage), null, out exact);
            probe["checkpoint"] = "native-final-review"; probe["revision"] = _revision;
            probe["heritage"] = heritage.Definition.Id.ToString();
            if (_character["finalReviewRacialLabels"] == null) _character["finalReviewRacialLabels"] = new JArray();
            ((JArray)_character["finalReviewRacialLabels"]).Add(probe);
            if (!exact)
                AcceptanceFailure("Native racial labels diverged at final review for " + heritage.Definition.Id +
                    " (revision " + _revision + "): " + ElementalRacialLabelObservation.Compact(probe));
        }

        private void CaptureSheetRacialLabels(CharacterScreenController sheet, UnitDescriptor owner)
        {
            ElementalRaceBlueprints race = RegressionRace;
            ElementalHeritageBlueprints heritage = race.Heritages.Choices().ToArray()[_revisionRoute[_revisionRoute.Length - 1]];
            int[] expected = HeritageRacial(heritage);
            var entries = sheet.GetComponentsInChildren<CharSScoreEntry>(true)
                .Where(value => !(value is CharBScoresEntry) && value.gameObject.activeInHierarchy).ToArray();
            bool bound = AbilityStats.All(stat => entries.Count(value => value.Type == stat &&
                ReferenceEquals(value.Stat, owner.Stats.GetStat(stat))) == 1);
            bool exact = bound;
            var rendered = new JArray();
            for (int index = 0; index < AbilityStats.Length; index++)
            {
                StatType stat = AbilityStats[index];
                var entry = entries.FirstOrDefault(value => value.Type == stat && ReferenceEquals(value.Stat, owner.Stats.GetStat(stat)));
                string label = entry?.RaceBonus?.text;
                string wanted = ElementalRacialLabelObservation.ExpectedLabel(expected[index]);
                bool same = label == wanted;
                exact &= same;
                rendered.Add(new JObject { ["stat"] = stat.ToString(), ["renderedRaceBonus"] = label,
                    ["renderedValue"] = entry?.Value?.text, ["modifiedValue"] = owner.Stats.GetStat(stat).ModifiedValue,
                    ["expectedRaceBonus"] = wanted, ["exact"] = same });
            }
            bool probeExact;
            JObject probe = ElementalRacialLabelObservation.Observe(owner, expected, null, out probeExact);
            exact &= probeExact;
            int markers = race.Heritages.Choices().Count(value => owner.HasFact(value.Marker));
            // The creator selects an explicit marker for every heritage, General included.
            bool markerExact = markers == 1 && owner.HasFact(heritage.Marker);
            exact &= markerExact;
            _character["sheetRacialLabels"] = new JObject { ["checkpoint"] = "native-character-sheet",
                ["heritage"] = heritage.Definition.Id.ToString(), ["markerGuid"] = heritage.Marker.AssetGuid,
                ["ownedHeritageMarkers"] = markers, ["markerExact"] = markerExact,
                ["sheetBound"] = bound, ["activeEntries"] = entries.Length, ["rendered"] = rendered,
                ["stats"] = probe, ["exact"] = exact };
            if (!exact)
                AcceptanceFailure("Rendered character-sheet racial labels diverged for " + heritage.Definition.Id + ": " +
                    rendered.ToString(Newtonsoft.Json.Formatting.None));
            Write();
        }
    }
}
