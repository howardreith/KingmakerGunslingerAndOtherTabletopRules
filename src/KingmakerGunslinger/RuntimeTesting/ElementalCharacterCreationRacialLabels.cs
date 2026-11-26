using System.Linq;
using Kingmaker.EntitySystem.Entities;
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

        // The committed-sheet capture admits only a registered native
        // CustomCompanion: the rolled loaded-game creator route, and the ordinary
        // native respec route (Fighter point-buy), whose committed owner is the
        // registered original after every Player.RespecCompanion visit.
        private bool RacialLabelSheetCase =>
            (_request.Scenario == RuntimeTestScenarioCatalog.WorkingSaveElementalCharacterCreationRegression &&
             (string)_request.Parameters["allocation"] == "roll") ||
            _request.Scenario == RuntimeTestScenarioCatalog.WorkingSaveElementalNativeRespec;

        // The registered actor whose committed state the native sheet shows.
        private UnitEntityData SheetOwner => _nativeRespec && _respecOriginal != null ? _respecOriginal : _unit;

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
                // The creator panel shows PermanentValue (CharBScoresEntry passes
                // showPermanentValue true); compare the rendered total with it.
                string shown = entry?.Value?.text;
                string wantedValue = preview.Stats.GetStat(stat).PermanentValue.ToString();
                bool same = label == wanted && shown == wantedValue;
                exact &= same;
                rendered.Add(new JObject { ["stat"] = stat.ToString(), ["renderedRaceBonus"] = label,
                    ["expectedRaceBonus"] = wanted, ["renderedValue"] = shown,
                    ["livePermanentValue"] = wantedValue, ["exact"] = same });
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
            bool exact;
            JObject observation = NativeSheetRacialLabelProbe.ObserveSheet(sheet, owner, HeritageRacial(heritage), out exact);
            int markers = race.Heritages.Choices().Count(value => owner.HasFact(value.Marker));
            // The creator selects an explicit marker for every heritage, General included.
            bool markerExact = markers == 1 && owner.HasFact(heritage.Marker);
            exact &= markerExact;
            observation["checkpoint"] = "native-character-sheet";
            observation["visit"] = _raceIndex;
            observation["heritage"] = heritage.Definition.Id.ToString();
            observation["markerGuid"] = heritage.Marker.AssetGuid;
            observation["ownedHeritageMarkers"] = markers;
            observation["markerExact"] = markerExact;
            observation["exact"] = exact;
            _character["sheetRacialLabels"] = observation;
            if (!exact)
                AcceptanceFailure("Rendered character-sheet racial labels diverged for " + heritage.Definition.Id + ": " +
                    observation["rendered"].ToString(Newtonsoft.Json.Formatting.None));
            Write();
        }
    }
}
