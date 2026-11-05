using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using KingmakerGunslinger.Bootstrap;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        // Bounded metadata-only intake for unresolved Sprint40 native inputs.
        // No actor, save, blueprint mutation or proprietary asset export.
        private RuntimeTestResult RunExpandedSummoningSprint40Inputs()
        {
            var checks = new List<RuntimeTestAssertion>();
            var document = new JObject {
                ["runId"] = _request.RunId,
                ["scope"] = "Sprint40 Ice Devil body and cold/illusion consumers; exact S39 Daze condition verification; research only",
                ["searchLimits"] = "Fixed names capped at twenty matches each. A name miss proves only absence from this search, not engine impossibility. Exact retained identities are also read independently of names."
            };
            try
            {
                BlueprintScriptableObject[] all = BlueprintBootstrap.Library
                    .GetAllBlueprints().Where(value => value != null).ToArray();
                Func<string, string> normalized = name => new string(
                    (name ?? string.Empty).Where(char.IsLetterOrDigit)
                        .ToArray());

                // 1. Is there a native IceDevil at all? The released
                // four-armed Girallon and Xill both authored original bodies
                // because the library has neither creature; this asks the
                // same question for this creature rather than assuming it.
                string[] species = { "IceDevil", "Gelugon" };
                BlueprintUnit[] units = all.OfType<BlueprintUnit>()
                    .Where(value => value.name != null &&
                        !value.name.StartsWith("KMG_",
                            StringComparison.Ordinal) &&
                        species.Any(name => normalized(value.name).IndexOf(
                            name, StringComparison.OrdinalIgnoreCase) >= 0))
                    .OrderBy(value => value.name, StringComparer.Ordinal)
                    .ToArray();
                if (units.Length > 16)
                    throw new InvalidOperationException(
                        "Fixed Sprint40 exact-species audit exceeded 16 candidates.");
                var unitRows = new JArray();
                foreach (BlueprintUnit unit in units)
                {
                    var row = new JObject {
                        ["nativeUnit"] = DescribeNativeUnit(unit) };
                    if (unit.Prefab != null &&
                        !string.IsNullOrEmpty(unit.Prefab.AssetId))
                    {
                        var prefab = unit.Prefab.Load(false);
                        row["view"] = prefab == null ? null :
                            CapturePrimateCensusViewMetadata(prefab,
                                "sprint40-exact-species", unit.AssetGuid,
                                unit.name, unit.Prefab.AssetId);
                    }
                    row["graph"] = DescribeGraph(unit, 0,
                        new HashSet<object>(
                            NativeDonorReferenceComparer.Instance), 5);
                    unitRows.Add(row);
                }
                document["units"] = unitRows;
                document["nativeIceDevilUnitCount"] = unitRows.Count;


                string[] consumers = { "ConeOfCold", "IceStorm", "PersistentImage",
                    "MajorImage", "SilentImage", "FearAura", "Regeneration", "Fly", "Spear" };
                var graphs = new JArray();
                foreach (string name in consumers)
                {
                    BlueprintScriptableObject[] matches = all.Where(value =>
                        value.name != null &&
                        !value.name.StartsWith("KMG_",
                            StringComparison.Ordinal) &&
                        value.name.IndexOf(name,
                            StringComparison.OrdinalIgnoreCase) >= 0)
                        .OrderBy(value => !string.Equals(value.name, name,
                            StringComparison.OrdinalIgnoreCase))
                        .ThenBy(value => value.name, StringComparer.Ordinal)
                        .Take(20).ToArray();
                    graphs.Add(new JObject {
                        ["query"] = name,
                        ["exactNameMatches"] = new JArray(matches
                            .Where(value => string.Equals(value.name, name,
                                StringComparison.OrdinalIgnoreCase))
                            .Select(value => new JObject {
                                ["name"] = value.name,
                                ["guid"] = value.AssetGuid,
                                ["type"] = value.GetType().FullName
                            })),
                        ["matches"] = new JArray(matches.Select(value =>
                            new JObject {
                                ["name"] = value.name,
                                ["guid"] = value.AssetGuid,
                                ["type"] = value.GetType().FullName,
                                ["isAbility"] = value is BlueprintAbility,
                                ["graph"] = DescribeGraph(value, 0,
                                    new HashSet<object>(
                                        NativeDonorReferenceComparer.Instance),
                                    8)
                            }).ToArray())
                    });
                }
                document["consumers"] = graphs;


                var retained = new JArray();
                foreach (string guid in new[] {
                    "9934fedff1b14994ea90205d189c8759", // S39 native Daze condition
                    "0bc608c3f2b548b44b7146b7530613ac"  // S36 native Slow condition
                })
                {
                    BlueprintScriptableObject exact = all.Single(value => value.AssetGuid == guid);
                    retained.Add(new JObject {
                        ["guid"] = guid, ["name"] = exact.name,
                        ["graph"] = DescribeGraph(exact, 0,
                            new HashSet<object>(NativeDonorReferenceComparer.Instance), 8)
                    });
                }
                document["retainedConditions"] = retained;
                document["illusionComponentTypes"] = new JArray(all
                    .Where(value => value.ComponentsArray != null)
                    .SelectMany(value => value.ComponentsArray)
                    .Where(value => value != null)
                    .Select(value => value.GetType().FullName)
                    .Distinct().Where(value => value.IndexOf("Figment", StringComparison.OrdinalIgnoreCase) >= 0 ||
                        value.IndexOf("Illusion", StringComparison.OrdinalIgnoreCase) >= 0)
                    .OrderBy(value => value, StringComparer.Ordinal).Take(40));
                checks.Add(Assertion("sprint40-native-input-capture",
                    "all fixed queries and retained conditions captured",
                    "queries=" + graphs.Count + ";retained=" + retained.Count,
                    graphs.Count == consumers.Length && retained.Count == 2,
                    "Metadata is not gameplay proof; absence is bounded to the captured scope."));
            }
            catch (Exception error)
            {
                document["exception"] = DescribeExpandedSummoningCorrectionException(error);
                checks.Add(Assertion("sprint40-native-input-capture",
                    "bounded audit completes", error.ToString(), false,
                    "Unknown never establishes native suitability."));
            }
            string path = Path.Combine(_request.EvidenceDirectory, "sprint40-inputs.json");
            RuntimeTestResultWriter.WriteAtomic(path, document.ToString(Formatting.Indented));
            var result = CreateResult(checks.All(value => value.Status == RuntimeTestStatuses.Pass) ?
                RuntimeTestStatuses.Pass : RuntimeTestStatuses.Fail, checks, null);
            result.EvidenceFiles.Add(path);
            return result;
        }
    }
}
