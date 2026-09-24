using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker;
using KingmakerGunslinger.Bootstrap;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        private FavoredClassPerformanceVisualProbe _performanceVisuals;

        // O01 observation: the spawned effect of every bardic performance
        // candidate (native, race-scaled and child-scaled), polled across
        // frames in the save-free fixture scene. Returns null until complete.
        private RuntimeTestResult PollFavoredClassPerformanceVisuals()
        {
            if (_performanceVisuals == null)
            {
                object player = ReadExactMember(Game.Instance, "Player");
                object state = ReadExactMember(Game.Instance, "State");
                _performanceVisualsParty = SnapshotReferences(ReadExactMember(player, "Party"));
                _performanceVisualsUnits = SnapshotReferences(ReadExactMember(state, "AllUnits"));
                _performanceVisuals = new FavoredClassPerformanceVisualProbe(BlueprintBootstrap.Library);
            }
            _performanceVisuals.Poll();
            if (!_performanceVisuals.Done)
                return null;
            object playerAfter = ReadExactMember(Game.Instance, "Player");
            object stateAfter = ReadExactMember(Game.Instance, "State");
            bool cleaned = SameReferences(_performanceVisualsParty,
                    SnapshotReferences(ReadExactMember(playerAfter, "Party"))) &&
                SameReferences(_performanceVisualsUnits, SnapshotReferences(ReadExactMember(stateAfter, "AllUnits")));
            JObject evidence = _performanceVisuals.Evidence;
            string path = WriteFavoredClassEvidence("favored-class-performance-visuals.json", evidence);
            var assertions = new List<RuntimeTestAssertion>
            {
                Assertion("fcb-performance-visuals-observed",
                    "every candidate performance area present in the profile was spawned and measured without an exception",
                    "observed=" + _performanceVisuals.Observed + ";failures=" +
                        string.Join(" | ", _performanceVisuals.Failures.ToArray()),
                    _performanceVisuals.Failures.Count == 0 && _performanceVisuals.Observed > 0,
                    "AreaEffectsController.SpawnAttachedToTarget; AreaEffectView spawned effect; FxDecal/SnapToLocator/ParticleSystem"),
                Assertion("external-isolation", "unchanged party and global-unit snapshots",
                    "cleaned=" + cleaned, cleaned, "detached entity disposal and exact reference snapshots"),
                Assertion("loaded-mod-version", _request.ExpectedModVersion, _context.ModEntry.Info.Version,
                    _request.ExpectedModVersion == _context.ModEntry.Info.Version,
                    "Unity Mod Manager ModEntry.Info.Version"),
            };
            RuntimeTestResult result = CreateResult(assertions.All(value => value.Status == "PASS")
                ? RuntimeTestStatuses.Pass : RuntimeTestStatuses.Fail, assertions, null);
            result.EvidenceFiles.Add(path);
            return result;
        }

        private object[] _performanceVisualsParty;
        private object[] _performanceVisualsUnits;
    }
}
