using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using KingmakerGunslinger.Bootstrap;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        // Fixed research scope: no actor, save, input, native mutation or geometry export.
        // Presence/capture is not a gameplay or donor-suitability verdict.
        private RuntimeTestResult RunExpandedSummoningSprints30And31Inputs()
        {
            var checks = new List<RuntimeTestAssertion>();
            var document = new JObject {
                ["runId"] = _request.RunId,
                ["scope"] = "S30/S31 fixed installed-consumer audit; research only"
            };
            try
            {
                BlueprintScriptableObject[] all = BlueprintBootstrap.Library
                    .GetAllBlueprints().Where(value => value != null).ToArray();
                string[] species = { "Lemure", "Dretch", "HoundArchon",
                    "LanternArchon", "AstralDeva", "TrumpetArchon" };
                // Existing native deva/wisp and small biped rigs are the bounded
                // alternatives only when an exact species is absent.
                string[] donors = { "MovanicDeva", "WillOWisp", "WilloWisp",
                    "Will_o_Wisp", "Goblin", "Lizardfolk" };
                BlueprintUnit[] candidates = all.OfType<BlueprintUnit>()
                    .Where(value => value.name != null &&
                        !value.name.StartsWith("KMG_", StringComparison.Ordinal) &&
                        (species.Any(name => value.name.IndexOf(name,
                            StringComparison.OrdinalIgnoreCase) >= 0) ||
                         donors.Any(name => string.Equals(value.name, name,
                            StringComparison.OrdinalIgnoreCase))))
                    .OrderBy(value => value.name, StringComparer.Ordinal).ToArray();
                if (candidates.Length > 96)
                    throw new InvalidOperationException("Fixed outsider donor audit exceeded 96 candidates.");
                var units = new JArray();
                var loaded = new HashSet<string>(StringComparer.Ordinal);
                foreach (BlueprintUnit unit in candidates)
                {
                    var row = new JObject { ["nativeUnit"] = DescribeNativeUnit(unit) };
                    string exact = species.Concat(donors).FirstOrDefault(name =>
                        string.Equals(name, unit.name, StringComparison.OrdinalIgnoreCase));
                    if (exact != null && loaded.Add(exact) && unit.Prefab != null &&
                        !string.IsNullOrEmpty(unit.Prefab.AssetId))
                    {
                        var prefab = unit.Prefab.Load(false);
                        row["view"] = prefab == null ? null :
                            CapturePrimateCensusViewMetadata(prefab, "s30-31-donor",
                                unit.AssetGuid, unit.name, unit.Prefab.AssetId);
                        row["graph"] = DescribeGraph(unit, 0,
                            new HashSet<object>(NativeDonorReferenceComparer.Instance), 4);
                    }
                    units.Add(row);
                }
                document["units"] = units;
                document["speciesCounts"] = new JObject(species.Select(name =>
                    new JProperty(name, candidates.Count(value => value.name.IndexOf(
                        name, StringComparison.OrdinalIgnoreCase) >= 0))));
                string[] consumers = { "StinkingCloud", "CauseFear", "Aid",
                    "AuraOfMenace", "Archon", "Angel", "ProtectionFromEvil",
                    "MagicCircleAgainstEvil", "ProtectiveAura", "Disruption",
                    "Disrupting", "Warhammer", "Greatsword", "Stunning",
                    "ClericSpellbook", "ClericSpellsPerDay", "ClericSpellsKnown",
                    "ClericClass", "Mindless", "OutsiderType", "DemonType",
                    "DevilType", "PoisonImmunity", "ElectricityImmunity",
                    "PetrificationImmunity", "HolyWord", "HolyAura",
                    "BladeBarrier", "SpellImmunity", "InvisibilityPurge" };
                var graphs = new JArray();
                foreach (string name in consumers)
                {
                    BlueprintScriptableObject[] matches = all.Where(value =>
                        value.name != null && !value.name.StartsWith("KMG_", StringComparison.Ordinal) &&
                        value.name.IndexOf(name, StringComparison.OrdinalIgnoreCase) >= 0)
                        .OrderBy(value => value.name, StringComparer.Ordinal).Take(16).ToArray();
                    graphs.Add(new JObject {
                        ["query"] = name,
                        ["matches"] = new JArray(matches.Select(value => new JObject {
                            ["name"] = value.name, ["guid"] = value.AssetGuid,
                            ["graph"] = DescribeGraph(value, 0,
                                new HashSet<object>(NativeDonorReferenceComparer.Instance), 5)
                        }).ToArray())
                    });
                }
                document["consumers"] = graphs;
                checks.Add(Assertion("s30-31-native-input-capture",
                    "fixed installed-consumer audit completes",
                    "units=" + units.Count + ";consumerQueries=" + graphs.Count,
                    graphs.Count == consumers.Length,
                    "Absent or unsuitable donors remain unresolved; capture is research only."));
            }
            catch (Exception error)
            {
                document["exception"] = DescribeExpandedSummoningCorrectionException(error);
                checks.Add(Assertion("s30-31-native-input-capture", "bounded audit completes",
                    error.ToString(), false, "Unknown never establishes donor suitability."));
            }
            string path = Path.Combine(_request.EvidenceDirectory, "s30-31-inputs.json");
            RuntimeTestResultWriter.WriteAtomic(path, document.ToString(Formatting.Indented));
            var result = CreateResult(checks.All(value => value.Status == RuntimeTestStatuses.Pass)
                ? RuntimeTestStatuses.Pass : RuntimeTestStatuses.Fail, checks, null);
            result.EvidenceFiles.Add(path);
            return result;
        }
    }
}
