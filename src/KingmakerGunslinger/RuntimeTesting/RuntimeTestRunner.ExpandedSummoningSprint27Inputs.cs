using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kingmaker.Blueprints;
using KingmakerGunslinger.Bootstrap;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        // Detached metadata only; no actor, save, native mesh or animation curves.
        private RuntimeTestResult RunExpandedSummoningSprint27Inputs()
        {
            var checks = new List<RuntimeTestAssertion>();
            var donors = new JArray();
            var graphs = new JArray();
            var doc = new JObject {
                ["runId"] = _request.RunId,
                ["scope"] = "Sprint27 bounded Griffon lookup and Manticore/GiantEagle attachment feasibility; research only, no spawn, save, mutation or gameplay qualification.",
                ["donors"] = donors, ["graphs"] = graphs
            };
            try
            {
                var all = BlueprintBootstrap.Library.GetAllBlueprints()
                    .Where(v => v != null).ToArray();
                var units = all.OfType<BlueprintUnit>().ToArray();
                var exact = units.Where(v => !string.IsNullOrEmpty(v.name) &&
                    v.name.IndexOf("griffon", StringComparison.OrdinalIgnoreCase) >= 0 &&
                    !v.name.StartsWith("KMG_", StringComparison.Ordinal)).ToArray();
                doc["exactGriffonMatches"] = new JArray(exact.Select(v =>
                    new JObject { ["guid"] = v.AssetGuid, ["name"] = v.name }));
                if (exact.Length > 2) throw new InvalidOperationException("More than two exact native Griffon candidates; review the bounded selection.");
                var candidates = exact.Length > 0 ? exact : new[] {
                    units.Single(v => v.AssetGuid == "da807351572c540478b703957d863e68"),
                    units.Single(v => v.AssetGuid == "406c1e1af5400ac4881e330502ccbd9e") };
                foreach (var unit in candidates)
                {
                    var prefab = unit.Prefab.Load(false);
                    if (prefab == null) throw new InvalidOperationException("Missing prefab: " + unit.name);
                    var row = CapturePrimateCensusViewMetadata(prefab, "griffon-candidate",
                        unit.AssetGuid, unit.name, unit.Prefab.AssetId);
                    row["nativeUnit"] = DescribeNativeUnit(unit);
                    row["restHierarchy"] = CaptureSprint24RestHierarchy(prefab);
                    row["animationManagers"] = CaptureSprint24AnimationManagers(prefab);
                    row["colliders"] = CaptureSprint24Colliders(prefab);
                    donors.Add(row);
                }
                string[] names = { "Pounce", "ChargeAbility", "ChargeBuff", "Airborne",
                    "IronWill", "SkillFocusPerception", "WeaponFocus", "TripDefenseFourLegs" };
                foreach (var v in all.Where(v => names.Contains(v.name)).OrderBy(v => v.name))
                    graphs.Add(new JObject { ["guid"] = v.AssetGuid, ["name"] = v.name,
                        ["type"] = v.GetType().FullName, ["graph"] = DescribeGraph(v, 0,
                            new HashSet<object>(NativeDonorReferenceComparer.Instance), 8) });
                checks.Add(Assertion("sprint27-inputs-bounded-capture",
                    "detached donor and native carrier metadata captured",
                    "donors=" + donors.Count + ";graphs=" + graphs.Count,
                    donors.Count > 0 && graphs.Count >= 6,
                    "Research evidence only; no product or functional-body PASS."));
            }
            catch (Exception e)
            {
                doc["exception"] = DescribeExpandedSummoningCorrectionException(e);
                checks.Add(Assertion("sprint27-inputs-capture", "bounded capture completes",
                    e.ToString(), false, "Unknown is never native PASS."));
            }
            string path = Path.Combine(_request.EvidenceDirectory, "sprint27-native-inputs.json");
            RuntimeTestResultWriter.WriteAtomic(path, doc.ToString(Formatting.Indented));
            var result = CreateResult(checks.All(v => v.Status == RuntimeTestStatuses.Pass)
                ? RuntimeTestStatuses.Pass : RuntimeTestStatuses.Fail, checks, null);
            result.EvidenceFiles.Add(path);
            return result;
        }
    }
}
