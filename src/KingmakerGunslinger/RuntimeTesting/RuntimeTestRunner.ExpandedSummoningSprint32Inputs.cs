using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        // The first input receipt stopped inside nested conditional actions.
        // Capture only these selected existing control graphs more deeply in
        // the next owned native review; no new actor or separate launch.
        private void CaptureSprint32ControlGraphs()
        {
            var receipt = new JObject(); bool passed = false;
            try
            {
                var library = BlueprintBootstrap.Library;
                // RequireExact deliberately rejects a base type. The concrete
                // types below are in the retained first native input receipt.
                BlueprintScriptableObject[] selected = {
                    BlueprintLibraryLookup.RequireExact<BlueprintAbility>(library, "1af9d5995090e5a4185a30decf0959ad", "native Charm Person"),
                    BlueprintLibraryLookup.RequireExact<BlueprintAbility>(library, "bb7ecad2d3d2c8247a38f44855c99061", "native Sleep"),
                    BlueprintLibraryLookup.RequireExact<BlueprintAbilityAreaEffect>(library, "a4fc1c0798359974e99e1d790935501d", "native Fascinate area"),
                    BlueprintLibraryLookup.RequireExact<BlueprintBuff>(library, "2d4bd347dec7d8648afd502ee40ae661", "native Fascinate effect"),
                    BlueprintLibraryLookup.RequireExact<BlueprintBuff>(library, "a50373fa77d30d34c8c6efb198b36921", "native Fascinate immunity"),
                    BlueprintLibraryLookup.RequireExact<BlueprintBuff>(library, "8a59ee053e804e2429355eecd749e4e7", "native Fascinate combat marker"),
                    BlueprintLibraryLookup.RequireExact<BlueprintBuff>(library, "6477ae917b0ec7a4ca76bc9f36b023ac", "native Rainbow Pattern terminal donor")
                };
                receipt["controls"] = new JArray(selected.Select(value => new JObject {
                    ["guid"] = value.AssetGuid, ["name"] = value.name,
                    ["graph"] = DescribeGraph(value, 0,
                        new HashSet<object>(NativeDonorReferenceComparer.Instance), 12) }));
                // Fixed remaining Lillend native consumers, research only.
                // An absent mapping remains an explicit implementation question.
                string[] names = { "Daze", "Lullaby", "Invisibility", "Light", "CombatCasting", "Hover" };
                var native = library.GetAllBlueprints().Where(value => value != null &&
                    value.name != null && !value.name.StartsWith("KMG_", StringComparison.Ordinal)).ToArray();
                receipt["remainingConsumers"] = new JArray(names.Select(name => new JObject {
                    ["query"] = name, ["matches"] = new JArray(native.Where(value =>
                        value.name.Equals(name, StringComparison.OrdinalIgnoreCase) ||
                        value.name.StartsWith(name + "Ability", StringComparison.OrdinalIgnoreCase) ||
                        value.name.StartsWith(name + "Buff", StringComparison.OrdinalIgnoreCase))
                        .OrderBy(value => value.name, StringComparer.Ordinal).Take(8).Select(value => new JObject {
                            ["guid"] = value.AssetGuid, ["name"] = value.name,
                            ["graph"] = DescribeGraph(value, 0,
                                new HashSet<object>(NativeDonorReferenceComparer.Instance), 8) })) }));
                passed = true;
            }
            catch (Exception error) { receipt["exception"] = error.ToString(); }
            RuntimeTestResultWriter.WriteAtomic(Path.Combine(_request.EvidenceDirectory,
                "sprint32-control-graphs.json"), receipt.ToString(Formatting.Indented));
            // A research-reader failure remains FAIL but does not prevent the
            // independent owned gameplay cases from producing useful evidence.
            Sprint25Check("native-control-graphs", passed, new JObject {
                ["captured"] = passed, ["exception"] = receipt["exception"] },
                "Selected existing native control graphs captured; metadata is not a gameplay verdict.");
        }

        // Fixed research scope: no actor, save, input, native mutation or geometry export.
        // Presence/capture is not a gameplay or donor-suitability verdict.
        private RuntimeTestResult RunExpandedSummoningSprint32Inputs()
        {
            var checks = new List<RuntimeTestAssertion>();
            var document = new JObject {
                ["runId"] = _request.RunId,
                ["scope"] = "Sprint32 fixed installed-consumer audit; research only"
            };
            try
            {
                BlueprintScriptableObject[] all = BlueprintBootstrap.Library
                    .GetAllBlueprints().Where(value => value != null).ToArray();
                string[] species = { "Satyr", "Lillend" };
                // Retained native presentation references, research only. No
                // proxy or visual-suitability acceptance follows from capture.
                string[] donorGuids = {
                    "afe56099ff6046b40a359b7562c0424e", // native Movanic Deva
                    "e8276e28b2234a745900fed80670bfdb"  // retained humanoid rig
                };
                Func<string, string> normalized = name => new string(
                    (name ?? string.Empty).Where(char.IsLetterOrDigit).ToArray());
                BlueprintUnit[] candidates = all.OfType<BlueprintUnit>()
                    .Where(value => value.name != null &&
                        !value.name.StartsWith("KMG_", StringComparison.Ordinal) &&
                        (species.Any(name => normalized(value.name).IndexOf(name,
                            StringComparison.OrdinalIgnoreCase) >= 0) ||
                         donorGuids.Contains(value.AssetGuid)))
                    .OrderBy(value => value.name, StringComparer.Ordinal).ToArray();
                if (candidates.Length > 96)
                    throw new InvalidOperationException("Fixed Sprint32 donor audit exceeded 96 candidates.");
                var units = new JArray();
                var loaded = new HashSet<string>(StringComparer.Ordinal);
                foreach (BlueprintUnit unit in candidates)
                {
                    var row = new JObject { ["nativeUnit"] = DescribeNativeUnit(unit) };
                    string exact = donorGuids.Contains(unit.AssetGuid)
                        ? unit.AssetGuid : species.FirstOrDefault(name => string.Equals(
                            name, normalized(unit.name), StringComparison.OrdinalIgnoreCase));
                    if (exact != null && loaded.Add(exact) && unit.Prefab != null &&
                        !string.IsNullOrEmpty(unit.Prefab.AssetId))
                    {
                        var prefab = unit.Prefab.Load(false);
                        row["view"] = prefab == null ? null :
                            CapturePrimateCensusViewMetadata(prefab, "sprint32-donor",
                                unit.AssetGuid, unit.name, unit.Prefab.AssetId);
                        row["graph"] = DescribeGraph(unit, 0,
                            new HashSet<object>(NativeDonorReferenceComparer.Instance), 4);
                    }
                    units.Add(row);
                }
                document["units"] = units;
                document["speciesCounts"] = new JObject(species.Select(name =>
                    new JProperty(name, candidates.Count(value => normalized(value.name).IndexOf(
                        name, StringComparison.OrdinalIgnoreCase) >= 0))));
                string[] consumers = { "Suggestion", "CharmPerson", "CharmMonster",
                    "Fear", "Sleep", "BardClass", "BardSpellbook", "BardSpellsPerDay",
                    "BardicPerformance", "BardicResource", "InspireCourage",
                    "InspireCompetence", "Fascinate", "Countersong", "CureSeriousWounds",
                    "CureLightWounds", "SoundBurst", "HoldPerson", "Dagger", "Shortbow",
                    "Longsword", "Tail", "FeyType", "Azata", "OutsiderType" };
                var graphs = new JArray();
                foreach (string name in consumers)
                {
                    BlueprintScriptableObject[] matches = all.Where(value =>
                        value.name != null && !value.name.StartsWith("KMG_", StringComparison.Ordinal) &&
                        value.name.IndexOf(name, StringComparison.OrdinalIgnoreCase) >= 0)
                        .OrderBy(value => !string.Equals(value.name, name, StringComparison.OrdinalIgnoreCase))
                        .ThenBy(value => value.name, StringComparer.Ordinal).Take(16).ToArray();
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
                checks.Add(Assertion("sprint32-native-input-capture",
                    "fixed installed-consumer audit completes",
                    "units=" + units.Count + ";consumerQueries=" + graphs.Count,
                    graphs.Count == consumers.Length,
                    "Absent or unsuitable donors remain unresolved; capture is research only."));
            }
            catch (Exception error)
            {
                document["exception"] = DescribeExpandedSummoningCorrectionException(error);
                checks.Add(Assertion("sprint32-native-input-capture", "bounded audit completes",
                    error.ToString(), false, "Unknown never establishes donor suitability."));
            }
            string path = Path.Combine(_request.EvidenceDirectory, "sprint32-inputs.json");
            RuntimeTestResultWriter.WriteAtomic(path, document.ToString(Formatting.Indented));
            var result = CreateResult(checks.All(value => value.Status == RuntimeTestStatuses.Pass)
                ? RuntimeTestStatuses.Pass : RuntimeTestStatuses.Fail, checks, null);
            result.EvidenceFiles.Add(path);
            return result;
        }
    }
}
