using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Items.Weapons;
using KingmakerGunslinger.Bootstrap;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        // One bounded read-only installed-library audit. No actor, save,
        // equipment instance or native asset is modified or redistributed.
        private RuntimeTestResult RunExpandedSummoningSprint28Inputs()
        {
            var checks = new List<RuntimeTestAssertion>();
            var units = new JArray();
            var carrierGraphs = new JArray();
            var weapons = new JArray();
            var document = new JObject {
                ["runId"] = _request.RunId,
                ["scope"] = "Sprint 28 giant donor and rock/feat input audit; detached native metadata only, not gameplay qualification",
                ["units"] = units,
                ["carrierGraphs"] = carrierGraphs,
                ["weapons"] = weapons
            };
            try
            {
                BlueprintScriptableObject[] all = BlueprintBootstrap.Library
                    .GetAllBlueprints().Where(value => value != null).ToArray();
                string[] species = { "HillGiant", "StoneGiant", "FireGiant" };
                var candidates = all.OfType<BlueprintUnit>().Where(value =>
                    value.name != null && !value.name.StartsWith("KMG_", StringComparison.Ordinal) &&
                    species.Any(name => value.name.IndexOf(name,
                        StringComparison.OrdinalIgnoreCase) >= 0))
                    .OrderBy(value => value.name, StringComparer.Ordinal).ToArray();
                document["candidateCount"] = candidates.Length;
                document["speciesCounts"] = new JObject(species.Select(name =>
                    new JProperty(name, candidates.Count(value => value.name.IndexOf(
                        name, StringComparison.OrdinalIgnoreCase) >= 0))));
                if (candidates.Length > 64)
                    throw new InvalidOperationException("Native giant name search exceeded the bounded 64-unit audit cap.");
                foreach (BlueprintUnit unit in candidates)
                {
                    var row = new JObject {
                        ["nativeUnit"] = DescribeNativeUnit(unit),
                        ["startingInventoryCount"] = unit.StartingInventory == null ? 0 : unit.StartingInventory.Length
                    };
                    if (unit.Prefab != null && !string.IsNullOrEmpty(unit.Prefab.AssetId))
                    {
                        var prefab = unit.Prefab.Load(false);
                        row["view"] = prefab == null ? null :
                            CapturePrimateCensusViewMetadata(prefab, "sprint28-giant-donor",
                                unit.AssetGuid, unit.name, unit.Prefab.AssetId);
                    }
                    units.Add(row);
                }
                string[] carriers = { "RockThrow", "RockCatch", "HeatedRock",
                    "QuickDraw", "PointBlankShot", "PreciseShot", "IntimidatingProwess",
                    "ImprovedSunder", "ImprovedOverrun", "GreatCleave" };
                var features = all.OfType<BlueprintFeature>().Where(value =>
                    value.name != null && carriers.Any(name => value.name.IndexOf(
                        name, StringComparison.OrdinalIgnoreCase) >= 0))
                    .OrderBy(value => value.name, StringComparer.Ordinal).Take(64).ToArray();
                foreach (BlueprintFeature feature in features)
                    carrierGraphs.Add(new JObject { ["name"] = feature.name,
                        ["guid"] = feature.AssetGuid,
                        ["graph"] = DescribeGraph(feature, 0,
                            new HashSet<object>(NativeDonorReferenceComparer.Instance), 4) });
                foreach (BlueprintItemWeapon weapon in all.OfType<BlueprintItemWeapon>()
                    .Where(value => value.name != null &&
                        (value.name.IndexOf("Greatclub", StringComparison.OrdinalIgnoreCase) >= 0 ||
                         value.name.IndexOf("Greatsword", StringComparison.OrdinalIgnoreCase) >= 0 ||
                         value.name.IndexOf("Rock", StringComparison.OrdinalIgnoreCase) >= 0))
                    .OrderBy(value => value.name, StringComparer.Ordinal).Take(64))
                    weapons.Add(new JObject { ["name"] = weapon.name,
                        ["guid"] = weapon.AssetGuid,
                        ["weapon"] = DescribeWeapon(weapon) });
                checks.Add(Assertion("sprint28-native-input-capture",
                    "bounded detached library metadata captured",
                    "units=" + units.Count + ";carriers=" + carrierGraphs.Count +
                    ";weapons=" + weapons.Count, candidates.Length > 0,
                    "PASS means only the read-only audit completed; an absent named donor remains an observed absence."));
            }
            catch (Exception error)
            {
                document["exception"] = DescribeExpandedSummoningCorrectionException(error);
                checks.Add(Assertion("sprint28-native-input-capture",
                    "bounded audit completes", error.ToString(), false,
                    "Unknown is never donor or gameplay PASS."));
            }
            string path = Path.Combine(_request.EvidenceDirectory, "s28-native-inputs.json");
            RuntimeTestResultWriter.WriteAtomic(path, document.ToString(Formatting.Indented));
            var result = CreateResult(checks.All(value => value.Status == RuntimeTestStatuses.Pass)
                ? RuntimeTestStatuses.Pass : RuntimeTestStatuses.Fail, checks, null);
            result.EvidenceFiles.Add(path);
            return result;
        }
    }
}
