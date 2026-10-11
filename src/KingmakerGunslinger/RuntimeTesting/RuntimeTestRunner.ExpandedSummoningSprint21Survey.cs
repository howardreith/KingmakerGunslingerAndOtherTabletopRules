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
        /// The Sprint 21 two-family donor census.
        ///
        /// <para>Research only, and the same smallest shape Sprints 18 and 20
        /// used: every candidate is a <em>detached</em> view prefab loaded
        /// read-only, so no campaign actor is spawned, no save is touched, no
        /// area membership changes and there is nothing to clean up. It runs
        /// at mod load and needs no save at all.</para>
        ///
        /// <para>What makes it different from both of those is that it is two
        /// censuses in one launch. This sprint authors a Huge demonic
        /// spider-hunter and a Medium lateral crab, and the easiest wrong
        /// decision available is to notice that both are arthropods, survey
        /// once, and author both against whichever rig the single survey
        /// liked. So each family gets its own anchors, its own discovery
        /// terms and its own prefab budget, the written file reports them
        /// separately, and a donor that serves both has to earn it
        /// twice.</para>
        ///
        /// <para>It reports; it does not decide. No donor carries a pass mark
        /// here. What the assertions check is that the census is complete
        /// enough to decide from: that each family's anchors resolved, that
        /// nothing was silently lost, and that the four chains the authoring
        /// depends on were actually counted.</para>
        /// </summary>
        private RuntimeTestResult RunExpandedSummoningSprint21DonorCensus()
        {
            WriteLifecycleStage("expanded-summoning-sprint21-census-start");
            var assertions = new List<RuntimeTestAssertion>();
            try
            {
                Sprint21DonorSurveyPolicy.Validate();
                BlueprintUnit[] all = BlueprintBootstrap.Library.GetAllBlueprints()
                    .OfType<BlueprintUnit>().Where(value => value != null).ToArray();

                var families = new JArray();
                var document = new JObject {
                    ["scope"] = "Sprint 21 two-family donor census: detached read-only view prefabs only. " +
                        "No campaign actor is spawned, no save is read or written, no native asset is modified.",
                    ["nativeData"] = "skeleton names, hierarchy, rest transforms, bind positions and rotations, " +
                        "renderer and material names and flags, view scale and bounds. No vertices, indices, UVs, " +
                        "bind matrices, textures, shader source or animation curves.",
                    ["decision"] = "This file reports. It does not choose a donor and applies no pass mark. " +
                        "The two families are surveyed independently on purpose: a rig that serves both must " +
                        "earn it in both, and sharing one because both creatures are arthropods is the mistake " +
                        "this sprint is forbidden to make.",
                    ["libraryUnitCount"] = all.Length,
                    ["prefabCap"] = Sprint21DonorSurveyPolicy.MaximumSurveyedPrefabs,
                    ["families"] = families
                };

                int totalCaptured = 0;
                int totalSurveyed = 0;
                int totalUnreadable = 0;
                var anchorsResolved = new List<string>();
                var anchorsMissing = new List<string>();
                var familiesWithChains = new List<string>();

                foreach (Sprint21DonorSurveyPolicy.Family family in
                    Sprint21DonorSurveyPolicy.Families)
                {
                    var selected = new List<BlueprintUnit>();
                    foreach (string[] anchor in family.Anchors)
                    {
                        BlueprintUnit unit = all.SingleOrDefault(value =>
                            string.Equals(value.AssetGuid, anchor[1],
                                StringComparison.Ordinal));
                        (unit == null ? anchorsMissing : anchorsResolved)
                            .Add(family.Key + "/" + anchor[0]);
                        assertions.Add(Assertion(
                            "sprint21-census-anchor-" + family.Key + "-" + anchor[0],
                            "exact validated donor blueprint resolves",
                            anchor[1] + ";name=" +
                                (unit == null ? "missing" : unit.name),
                            unit != null, "exact asset id, never a name search"));
                        if (unit != null) selected.Add(unit);
                    }

                    // Everything the installation answers to for this family,
                    // excluding what this mod itself registered: a census of
                    // the game must not count the creature being authored.
                    BlueprintUnit[] discovered = all
                        .Where(value => Sprint21DonorSurveyPolicy.Matches(
                            value.name, family.DiscoveryTerms))
                        .Where(value => !Sprint21DonorSurveyPolicy.IsOurs(value.name))
                        .Where(value => Sprint21DonorSurveyPolicy.AnchorRank(
                            family, value.AssetGuid) == int.MaxValue)
                        .OrderBy(value => value.name, StringComparer.Ordinal)
                        .ToArray();
                    BlueprintUnit[] ours = all
                        .Where(value => Sprint21DonorSurveyPolicy.Matches(
                            value.name, family.PriorityTerms))
                        .Where(value => Sprint21DonorSurveyPolicy.IsOurs(value.name))
                        .ToArray();

                    // One rig per prefab: many blueprints share a handful of
                    // views, and the view is what a body is authored against.
                    var byPrefab = new Dictionary<string, BlueprintUnit>(
                        StringComparer.Ordinal);
                    foreach (BlueprintUnit unit in selected
                        .Concat(discovered.Where(value =>
                            Sprint21DonorSurveyPolicy.IsPriority(family, value.name)))
                        .Concat(discovered.Where(value =>
                            !Sprint21DonorSurveyPolicy.IsPriority(family, value.name))))
                    {
                        string prefab = unit.Prefab == null ? null : unit.Prefab.AssetId;
                        if (string.IsNullOrEmpty(prefab) || byPrefab.ContainsKey(prefab))
                            continue;
                        byPrefab.Add(prefab, unit);
                        if (byPrefab.Count >= family.PrefabBudget) break;
                    }

                    var rows = new JArray();
                    int captured = 0;
                    int unreadable = 0;
                    bool chainsCounted = false;
                    foreach (KeyValuePair<string, BlueprintUnit> pair in byPrefab
                        .OrderBy(value => Sprint21DonorSurveyPolicy.AnchorRank(
                            family, value.Value.AssetGuid))
                        .ThenBy(value => value.Value.name, StringComparer.Ordinal))
                    {
                        BlueprintUnit unit = pair.Value;
                        string key = Sprint21DonorSurveyPolicy.AnchorKey(
                            family, unit.AssetGuid) ?? unit.name;
                        JObject row;
                        try
                        {
                            UnitEntityView prefab = unit.Prefab.Load(false);
                            if (prefab == null)
                                throw new InvalidOperationException(
                                    "No loadable view prefab.");
                            row = CapturePrimateCensusViewMetadata(prefab, key,
                                unit.AssetGuid, unit.name, pair.Key);
                            row["isAnchor"] = Sprint21DonorSurveyPolicy
                                .AnchorRank(family, unit.AssetGuid) != int.MaxValue;
                            row["size"] = unit.Size.ToString();
                            row["unitType"] = unit.Type == null ? null
                                : unit.Type.name;
                            AddSprint21ChainReport(row);
                            captured++;
                            if ((int)row["walkingLegBones"] > 0) chainsCounted = true;
                        }
                        catch (Exception exception)
                        {
                            unreadable++;
                            row = new JObject {
                                ["key"] = key,
                                ["nativeBlueprint"] = unit.AssetGuid,
                                ["blueprintName"] = unit.name,
                                ["prefab"] = pair.Key,
                                ["unreadable"] =
                                    DescribeExpandedSummoningCorrectionException(
                                        exception)
                            };
                        }
                        rows.Add(row);
                    }

                    if (chainsCounted) familiesWithChains.Add(family.Key);
                    totalCaptured += captured;
                    totalSurveyed += byPrefab.Count;
                    totalUnreadable += unreadable;
                    families.Add(new JObject {
                        ["family"] = family.Key,
                        ["question"] = family.Question,
                        ["anchors"] = new JArray(family.Anchors.Select(value =>
                            (JToken)new JObject {
                                ["key"] = value[0], ["nativeBlueprint"] = value[1]
                            })),
                        ["discoveryTerms"] = new JArray(
                            family.DiscoveryTerms.Select(value => (JToken)value)),
                        ["priorityTerms"] = new JArray(
                            family.PriorityTerms.Select(value => (JToken)value)),
                        ["prefabBudget"] = family.PrefabBudget,
                        ["discoveredBlueprintCount"] = discovered.Length,
                        ["discoveredBlueprintNames"] = new JArray(discovered
                            .Select(value => (JToken)value.name).Take(64)),
                        ["registeredByThisMod"] = new JArray(
                            ours.Select(value => (JToken)value.name)),
                        ["surveyedPrefabCount"] = byPrefab.Count,
                        ["capturedPrefabCount"] = captured,
                        ["unreadablePrefabCount"] = unreadable,
                        ["rows"] = rows
                    });
                }

                string fileName = "sprint21-two-family-donor-census.json";
                File.WriteAllText(Path.Combine(_request.EvidenceDirectory, fileName),
                    document.ToString(Formatting.Indented));
                WriteLifecycleStage("expanded-summoning-sprint21-census-written");

                assertions.Add(Assertion("sprint21-census-two-families",
                    "the two bodies are surveyed independently",
                    "families=" + families.Count + ";keys=" + string.Join(",",
                        Sprint21DonorSurveyPolicy.Families.Select(v => v.Key)),
                    families.Count == 2,
                    "a Huge demonic spider-hunter and a Medium lateral crab "
                    + "share nothing but an exoskeleton"));
                assertions.Add(Assertion("sprint21-census-anchors-resolved",
                    "every family's anchors resolve by exact asset id",
                    "resolved=" + string.Join(",", anchorsResolved) +
                        ";missing=" + (anchorsMissing.Count == 0 ? "none"
                            : string.Join(",", anchorsMissing)),
                    anchorsMissing.Count == 0,
                    "exact asset ids, never a name search"));
                assertions.Add(Assertion("sprint21-census-captured",
                    "nothing surveyed is silently lost",
                    "file=" + fileName + ";surveyed=" + totalSurveyed +
                        ";captured=" + totalCaptured +
                        ";unreadable=" + totalUnreadable,
                    totalCaptured > 0 &&
                        totalCaptured + totalUnreadable == totalSurveyed,
                    // Sprint 20's second run demanded every surveyed prefab
                    // capture and failed on two swarms that have no skinned
                    // rig at all. A swarm without a rig is a recorded
                    // finding; what would be a failure is a row that went
                    // missing.
                    "detached read-only prefabs; an unreadable one is "
                    + "recorded with its reason"));
                assertions.Add(Assertion("sprint21-census-bounded",
                    "one launch stays within the fixed prefab cap",
                    totalSurveyed + " <= " +
                        Sprint21DonorSurveyPolicy.MaximumSurveyedPrefabs,
                    totalSurveyed <= Sprint21DonorSurveyPolicy.MaximumSurveyedPrefabs,
                    "targets fixed in source; never request-supplied"));
                assertions.Add(Assertion("sprint21-census-chains-measured",
                    "both families measured the chains their bodies hang on",
                    "withChains=" + string.Join(",", familiesWithChains),
                    familiesWithChains.Count == 2,
                    "a body authored onto a chain nobody measured is a guess"));
            }
            catch (Exception exception)
            {
                assertions.Add(Assertion("sprint21-donor-census-exception",
                    "bounded read-only census without exception",
                    DescribeExpandedSummoningCorrectionException(exception), false,
                    "request-local research only"));
            }
            assertions.Add(Assertion("loaded-mod-version", _request.ExpectedModVersion,
                _context.ModEntry.Info.Version,
                _context.ModEntry.Info.Version == _request.ExpectedModVersion,
                "Unity Mod Manager ModEntry.Info.Version"));
            return CreateResult(
                assertions.All(value => value.Status == "PASS") ? "PASS" : "FAIL",
                assertions, null);
        }

        /// <summary>
        /// Count the four chains this sprint's two bodies depend on, from the
        /// bone names the capture already holds.
        ///
        /// <para>Named chains rather than a raw roster, because that is what
        /// the authoring question actually is. How many walking-leg chains
        /// there are and how long they run, since the crab wants eight and
        /// the Bebelith wants six. Whether there is an articulated forelimb
        /// chain, which both creatures need and neither can use a walking leg
        /// for. Whether there is an abdomen chain, which the Bebelith's web
        /// comes out of and which a crab must not have. And whether the jaw
        /// is a real chain or a painted face, because the Bebelith's bite is
        /// a printed primary attack with its own die.</para>
        /// </summary>
        private static void AddSprint21ChainReport(JObject row)
        {
            var names = new HashSet<string>(StringComparer.Ordinal);
            foreach (JToken renderer in
                (row["skinnedRenderers"] as JArray) ?? new JArray())
            {
                JArray bones = renderer["bones"] as JArray;
                if (bones == null) continue;
                foreach (JToken bone in bones)
                {
                    string name = bone.Type == JTokenType.String
                        ? (string)bone : (string)bone["name"];
                    if (!string.IsNullOrEmpty(name)) names.Add(name);
                }
            }
            row["distinctBoneNames"] = names.Count;
            Report(row, names, "walkingLeg",
                Sprint21DonorSurveyPolicy.WalkingLegCandidates);
            Report(row, names, "forelimb",
                Sprint21DonorSurveyPolicy.ForelimbCandidates);
            Report(row, names, "abdomen",
                Sprint21DonorSurveyPolicy.AbdomenChainCandidates);
            Report(row, names, "jaw",
                Sprint21DonorSurveyPolicy.JawCandidates);
            // How many leg PAIRS the rig actually carries, which is the one
            // number both bodies are authored against and is not the same as
            // the bone count: a rig can have three full pairs and a fourth
            // that is a stub.
            int pairs = 0;
            for (int index = 0; index < 4; index++)
                if (names.Contains("L_Foot" + index) &&
                    names.Contains("R_Foot" + index))
                    pairs++;
            row["walkingLegPairs"] = pairs;
            row["allBoneNames"] = new JArray(names.OrderBy(value => value,
                StringComparer.Ordinal));
        }

        private static void Report(JObject row, HashSet<string> names,
            string prefix, string[] candidates)
        {
            row[prefix + "Bones"] = candidates.Count(names.Contains);
            row[prefix + "Present"] = new JArray(candidates.Where(names.Contains));
        }
    }
}
