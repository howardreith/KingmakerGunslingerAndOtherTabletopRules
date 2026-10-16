using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.View;
using KingmakerGunslinger.Bootstrap;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        /// <summary>
        /// The one bounded Sprint 25 quadruped donor census. It reuses the
        /// Sprint 24 capture helpers unchanged: those already emit complete
        /// local and world rest transforms, the renderer bind frame with per
        /// bone parent/bindPosition/bindRotation, anatomy chains, animation
        /// actions with their clips, colliders and materials. Nothing new is
        /// invented to read the donor.
        ///
        /// <para>Two questions this answers that a metadata-only survey could
        /// not. First, whether the Mastodon rig's neck, trunk, tail and leg
        /// chains can carry a credible long-necked sauropod and a trunked
        /// elephant, or whether a bounded alternative is needed. Second,
        /// whether the installed library exposes concrete Great Cleave and
        /// Greater Overrun feats at all, which is what separates "this project
        /// never mapped it" from "the engine does not implement it".</para>
        ///
        /// <para>Research evidence only. It proves donor structure and native
        /// feat presence; it qualifies neither Sprint 25 creature, and this
        /// candidate is never published. Detached read-only prefabs: no spawn,
        /// save, actor, asset mutation or gameplay event.</para>
        /// </summary>
        private RuntimeTestResult RunExpandedSummoningSprint25DonorCensus()
        {
            WriteLifecycleStage("expanded-summoning-sprint25-donor-census-start");
            var assertions = new List<RuntimeTestAssertion>();
            var rows = new JArray();
            var facts = new JArray();
            var candidates = new JArray();
            var document = new JObject {
                ["scope"] = "Sprint 25 closed quadruped donor census; detached read-only native prefabs and exact native feat graphs; no spawn, save, asset mutation or gameplay event.",
                ["decision"] = "Research evidence only. It proves donor structure and native feat presence, not creature qualification, and this candidate is never published.",
                ["runId"] = _request.RunId,
                ["donors"] = rows,
                ["factAnchors"] = facts,
                ["featCandidates"] = candidates
            };

            try
            {
                Sprint25DonorCensusPolicy.Validate();
                BlueprintScriptableObject[] all = BlueprintBootstrap.Library
                    .GetAllBlueprints().Where(value => value != null).ToArray();
                BlueprintUnit[] units = all.OfType<BlueprintUnit>().ToArray();
                int captured = 0;
                int discoveredPrefabs = 0;

                foreach (string[] anchor in Sprint25DonorCensusPolicy.Donors)
                {
                    BlueprintUnit unit = units.SingleOrDefault(value =>
                        string.Equals(value.AssetGuid, anchor[1],
                            StringComparison.Ordinal));
                    if (unit == null)
                        throw new InvalidOperationException("creature=" + anchor[0] +
                            ";predicate=exact-donor;guid=" + anchor[1]);
                    string actualPrefab = unit.Prefab == null ? null : unit.Prefab.AssetId;
                    bool discovering = string.Equals(anchor[2],
                        Sprint25DonorCensusPolicy.DiscoverPrefab,
                        StringComparison.Ordinal);
                    if (discovering)
                    {
                        if (string.IsNullOrEmpty(actualPrefab))
                            throw new InvalidOperationException("creature=" + anchor[0] +
                                ";predicate=discoverable-prefab;actual=null");
                        discoveredPrefabs++;
                    }
                    else if (!string.Equals(actualPrefab, anchor[2],
                        StringComparison.Ordinal))
                        throw new InvalidOperationException("creature=" + anchor[0] +
                            ";predicate=exact-prefab;expected=" + anchor[2] +
                            ";actual=" + actualPrefab);
                    UnitEntityView prefab = unit.Prefab.Load(false);
                    if (prefab == null)
                        throw new InvalidOperationException("creature=" + anchor[0] +
                            ";predicate=detached-prefab-load");

                    JObject row = CapturePrimateCensusViewMetadata(prefab,
                        anchor[0], unit.AssetGuid, unit.name, actualPrefab);
                    row["prefabWasDiscovered"] = discovering;
                    row["nativeUnit"] = DescribeNativeUnit(unit);
                    row["size"] = unit.Size.ToString();
                    row["restHierarchy"] = CaptureSprint24RestHierarchy(prefab);
                    row["anatomy"] = CaptureSprint24Anatomy(prefab);
                    row["animationManagers"] =
                        CaptureSprint24AnimationManagers(prefab);
                    row["colliders"] = CaptureSprint24Colliders(prefab);
                    rows.Add(row);
                    captured++;
                }

                foreach (string[] anchor in Sprint25DonorCensusPolicy.FactAnchors)
                {
                    BlueprintScriptableObject fact = all.SingleOrDefault(value =>
                        string.Equals(value.AssetGuid, anchor[1],
                            StringComparison.Ordinal));
                    facts.Add(new JObject {
                        ["key"] = anchor[0], ["guid"] = anchor[1],
                        ["found"] = fact != null,
                        ["name"] = fact == null ? null : fact.name,
                        ["type"] = fact == null ? null : fact.GetType().FullName,
                        ["graph"] = fact == null ? JValue.CreateNull() :
                            DescribeGraph(fact, 0, new HashSet<object>(
                                NativeDonorReferenceComparer.Instance), 8)
                    });
                }

                BlueprintScriptableObject[] discovered = all.Where(value =>
                        Sprint25DonorCensusPolicy.MatchesFeatCandidate(value.name))
                    .OrderBy(value => value.name, StringComparer.Ordinal)
                    .ThenBy(value => value.AssetGuid, StringComparer.Ordinal)
                    .Take(Sprint25DonorCensusPolicy
                        .MaximumDiscoveredFeatBlueprints + 1).ToArray();
                if (discovered.Length > Sprint25DonorCensusPolicy
                    .MaximumDiscoveredFeatBlueprints)
                    throw new InvalidOperationException(
                        "predicate=bounded-feat-discovery;actual=" +
                        discovered.Length);
                foreach (BlueprintScriptableObject value in discovered)
                    candidates.Add(new JObject {
                        ["guid"] = value.AssetGuid, ["name"] = value.name,
                        ["type"] = value.GetType().FullName,
                        ["graph"] = DescribeGraph(value, 0,
                            new HashSet<object>(
                                NativeDonorReferenceComparer.Instance), 8)
                    });

                bool anchorsFound = facts.OfType<JObject>()
                    .All(value => (bool)value["found"]);
                assertions.Add(Assertion("sprint25-census-donors",
                    "both exact detached quadruped-census candidates are captured",
                    "captured=" + captured + ";expected=" +
                        Sprint25DonorCensusPolicy.Donors.Length +
                        ";discoveredPrefabs=" + discoveredPrefabs,
                    captured == Sprint25DonorCensusPolicy.Donors.Length,
                    "read-only prefabs; no actor or save"));
                assertions.Add(Assertion("sprint25-census-positive-controls",
                    "every already-mapped native carrier resolves exactly",
                    "anchors=" + string.Join(",",
                        Sprint25DonorCensusPolicy.FactAnchors.Select(v => v[0])),
                    anchorsFound,
                    "a census that cannot find Endurance or Cleave is measuring nothing"));
                assertions.Add(Assertion("sprint25-census-bounded",
                    "closed inputs and bounded feat-name results",
                    "donors=" + rows.Count + ";facts=" + facts.Count +
                        ";candidates=" + candidates.Count,
                    rows.Count == Sprint25DonorCensusPolicy.Donors.Length &&
                        facts.Count == Sprint25DonorCensusPolicy.FactAnchors.Length &&
                        candidates.Count <= Sprint25DonorCensusPolicy
                            .MaximumDiscoveredFeatBlueprints,
                    "fixed source-owned identities and terms"));
            }
            catch (Exception exception)
            {
                document["exception"] =
                    DescribeExpandedSummoningCorrectionException(exception);
                assertions.Add(Assertion("sprint25-census-capture",
                    "bounded read-only census completes",
                    document["exception"].ToString(), false,
                    "unknown metadata is not qualification"));
            }

            string fileName = "sprint25-donor-census.json";
            RuntimeTestResultWriter.WriteAtomic(Path.Combine(
                _request.EvidenceDirectory, fileName),
                document.ToString(Formatting.Indented));
            WriteLifecycleStage("expanded-summoning-sprint25-donor-census-written");
            assertions.Add(Assertion("loaded-mod-version",
                _request.ExpectedModVersion, _context.ModEntry.Info.Version,
                _context.ModEntry.Info.Version == _request.ExpectedModVersion,
                "Unity Mod Manager ModEntry.Info.Version"));
            return CreateResult(assertions.All(value => value.Status == "PASS")
                ? "PASS" : "FAIL", assertions, null);
        }
    }
}
