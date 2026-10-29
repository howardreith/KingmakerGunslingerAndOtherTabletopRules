using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.ElementsSystem;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Actions;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        private static IEnumerable<ContextActionApplyBuff> Sprint32ControlApplications(ActionList list, int depth = 0)
        {
            if (depth > 12) throw new InvalidOperationException("Selected native control action depth exceeds the bounded capture.");
            if (list == null) yield break;
            foreach (var action in list.Actions ?? Array.Empty<GameAction>())
            {
                if (action == null) continue;
                var apply = action as ContextActionApplyBuff;
                if (apply != null) yield return apply;
                foreach (var field in ExpandedSummoningFields(action.GetType()).Where(value =>
                    !value.IsStatic && value.FieldType == typeof(ActionList)))
                    foreach (var child in Sprint32ControlApplications((ActionList)field.GetValue(action), depth + 1))
                        yield return child;
            }
        }

        // The original bounded donor capture truncated these exact branches.
        // Reuse its reader on the applications and referenced terminal roots;
        // no extra actor, command, save access or separate launch.
        private void CaptureSprint32RemainingControlLeaves()
        {
            var document = new JObject(); bool passed = false;
            try
            {
                var library = BlueprintBootstrap.Library;
                var rows = new JArray();
                foreach (string guid in new[] { "d2aeac47450c76347aebbc02e4f463e0",
                    "c3893092a333b93499fd0a21845aa265", "c7104f7526c4c524f91474614054547e" })
                {
                    var ability = BlueprintLibraryLookup.RequireExact<BlueprintAbility>(library, guid,
                        "selected remaining Sprint32 control family");
                    var run = ability.GetComponents<AbilityEffectRunAction>().Single();
                    var leaves = Sprint32ControlApplications(run.Actions).ToArray();
                    if (leaves.Length == 0 || leaves.Any(value => value.Buff == null))
                        throw new InvalidOperationException("Selected control has no complete terminal applications: " + ability.name);
                    rows.Add(new JObject { ["guid"] = guid, ["name"] = ability.name,
                        ["save"] = run.SavingThrowType.ToString(),
                        ["actions"] = DescribeGraph(run.Actions, 0, new HashSet<object>(NativeDonorReferenceComparer.Instance), 20),
                        ["applications"] = new JArray(leaves.Select(value => DescribeGraph(value, 0,
                            new HashSet<object>(NativeDonorReferenceComparer.Instance), 12))),
                        ["terminals"] = new JArray(leaves.Select(value => value.Buff).Distinct().Select(value =>
                            DescribeGraph(value, 0, new HashSet<object>(NativeDonorReferenceComparer.Instance), 12))) });
                }
                document["families"] = rows; passed = rows.Count == 3;
            }
            catch (Exception error) { document["exception"] = error.ToString(); }
            RuntimeTestResultWriter.WriteAtomic(Path.Combine(_request.EvidenceDirectory,
                "sprint32-remaining-control-terminals.json"), document.ToString(Formatting.Indented));
            Sprint25Check("remaining-control-terminals", passed, new JObject {
                ["captured"] = passed, ["exception"] = document["exception"] },
                "Exact Fear, Sound Burst and Hold Person applications/terminal components captured read-only; not a gameplay or cleanup verdict.");
        }

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
                // Exact optional provider roots, including their live nested save branches.
                // Missing provider roots are absence evidence; never fabricate a graph.
                var optional = library.GetAllBlueprints().Where(value => value != null &&
                    (value.AssetGuid == KingmakerGunslinger.Summoning.Sprint32Suggestion.AbilityGuid ||
                     value.AssetGuid == KingmakerGunslinger.Summoning.Sprint32Suggestion.BuffGuid)).ToArray();
                receipt["optionalSuggestion"] = new JArray(optional.Select(value => new JObject {
                    ["guid"] = value.AssetGuid, ["name"] = value.name,
                    ["graph"] = DescribeGraph(value, 0,
                        new HashSet<object>(NativeDonorReferenceComparer.Instance), 16) }));
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
        private RuntimeTestResult RunExpandedSummoningSprint32Inputs(bool shadowOutsiders = false)
        {
            string sprint = shadowOutsiders ? "sprint33" : "sprint32";
            var checks = new List<RuntimeTestAssertion>();
            var document = new JObject {
                ["runId"] = _request.RunId,
                ["scope"] = sprint + " fixed installed-consumer audit; research only; native attribution requires standalone profile"
            };
            try
            {
                BlueprintScriptableObject[] all = BlueprintBootstrap.Library
                    .GetAllBlueprints().Where(value => value != null).ToArray();
                string[] species = shadowOutsiders ? new[] { "Succubus", "ShadowDemon" } : new[] { "Satyr", "Lillend" };
                // Retained native presentation references, research only. No
                // proxy or visual-suitability acceptance follows from capture.
                string[] donorGuids = shadowOutsiders ? new[] {
                    "0cc7a2526e4557945b1d8eb277d1fb3a", "676f8b7d0a170674cb6e504e0e30b4f0",
                    "1832be68f9814254dbbdab6df7fd5d0b", "a8cbd4cdb0feaf64eab1452257c3c971",
                    "0c908145873f4b67a188397ca5f46da1", "3cd7b4f2c65b4d35929dd4d969dfaa41",
                    "627c1841a5eb4e32b1a94c0f43ec8a60"
                } : new[] {
                    "afe56099ff6046b40a359b7562c0424e", // native Movanic Deva
                    "e8276e28b2234a745900fed80670bfdb"  // retained humanoid rig
                };
                Func<string, string> normalized = name => new string(
                    (name ?? string.Empty).Where(char.IsLetterOrDigit).ToArray());
                BlueprintUnit[] candidates = all.OfType<BlueprintUnit>()
                    .Where(value => value.name != null &&
                        (shadowOutsiders || !value.name.StartsWith("KMG_", StringComparison.Ordinal)) &&
                        (species.Any(name => normalized(value.name).IndexOf(name,
                            StringComparison.OrdinalIgnoreCase) >= 0) ||
                         donorGuids.Contains(value.AssetGuid)))
                    .OrderBy(value => value.name, StringComparer.Ordinal).ToArray();
                if (candidates.Length > (shadowOutsiders ? 32 : 96))
                    throw new InvalidOperationException("Fixed donor audit exceeded its candidate bound.");
                var units = new JArray();
                var loaded = new HashSet<string>(StringComparer.Ordinal);
                foreach (BlueprintUnit unit in candidates)
                {
                    var row = new JObject { ["nativeUnit"] = DescribeNativeUnit(unit) };
                    string exact = shadowOutsiders ? unit.Prefab?.AssetId : donorGuids.Contains(unit.AssetGuid)
                        ? unit.AssetGuid : species.FirstOrDefault(name => string.Equals(
                            name, normalized(unit.name), StringComparison.OrdinalIgnoreCase));
                    if (exact != null && loaded.Add(exact) && unit.Prefab != null &&
                        !string.IsNullOrEmpty(unit.Prefab.AssetId))
                    {
                        var prefab = unit.Prefab.Load(false);
                        row["view"] = prefab == null ? null :
                            CapturePrimateCensusViewMetadata(prefab, sprint + "-donor",
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
                string[] consumers = shadowOutsiders ? new[] { "NaturalInvisibilityBuff", "Incorporeal", "DominatePerson" } : new[] { "Suggestion", "CharmPerson", "CharmMonster",
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
                checks.Add(Assertion(sprint + "-native-input-capture",
                    "fixed installed-consumer audit completes",
                    "units=" + units.Count + ";consumerQueries=" + graphs.Count,
                    graphs.Count == consumers.Length,
                    "Absent or unsuitable donors remain unresolved; capture is research only."));
            }
            catch (Exception error)
            {
                document["exception"] = DescribeExpandedSummoningCorrectionException(error);
                checks.Add(Assertion(sprint + "-native-input-capture", "bounded audit completes",
                    error.ToString(), false, "Unknown never establishes donor suitability."));
            }
            string path = Path.Combine(_request.EvidenceDirectory, sprint + "-inputs.json");
            RuntimeTestResultWriter.WriteAtomic(path, document.ToString(Formatting.Indented));
            var result = CreateResult(checks.All(value => value.Status == RuntimeTestStatuses.Pass)
                ? RuntimeTestStatuses.Pass : RuntimeTestStatuses.Fail, checks, null);
            result.EvidenceFiles.Add(path);
            return result;
        }
    }
}
