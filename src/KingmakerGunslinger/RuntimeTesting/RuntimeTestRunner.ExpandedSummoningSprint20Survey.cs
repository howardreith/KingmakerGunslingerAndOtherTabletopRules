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
        /// The Sprint 20 arachnid donor census.
        ///
        /// <para>Research only, and the same smallest shape Sprint 18 used:
        /// every candidate is a <em>detached</em> view prefab loaded read-only,
        /// so no campaign actor is spawned, no save is touched, no area
        /// membership changes and there is nothing to clean up. It runs at mod
        /// load and needs no save at all.</para>
        ///
        /// <para>It asks a narrower question than Sprint 18's did. The donor is
        /// already decided: the Sprint 14 census established that Kingmaker has
        /// no scorpion and that the Giant Spider is the only compact
        /// many-legged arthropod in the game. What this census is for is the
        /// rig's measurements - the full bone roster, the bind frame, and how
        /// many segments the abdomen chain has, because a scorpion's metasoma
        /// is authored onto that chain and the Sprint 14 capture that would
        /// have answered it did not survive.</para>
        ///
        /// <para>It reports; it does not decide. It also re-checks the Sprint
        /// 14 finding against the live library rather than against a memory of
        /// it: if some other arthropod rig has appeared, the discovery terms
        /// will find it and the written file will say so.</para>
        /// </summary>
        private RuntimeTestResult RunExpandedSummoningArachnidCensus()
        {
            WriteLifecycleStage("expanded-summoning-arachnid-census-start");
            var assertions = new List<RuntimeTestAssertion>();
            JArray document0ScorpionRows = new JArray();
            try
            {
                ArachnidRigSurveyPolicy.Validate();
                BlueprintUnit[] all = BlueprintBootstrap.Library.GetAllBlueprints()
                    .OfType<BlueprintUnit>().Where(value => value != null).ToArray();

                var selected = new List<BlueprintUnit>();
                foreach (string[] anchor in ArachnidRigSurveyPolicy.AnchorDonors)
                {
                    BlueprintUnit unit = all.SingleOrDefault(value =>
                        string.Equals(value.AssetGuid, anchor[1],
                            StringComparison.Ordinal));
                    bool found = unit != null;
                    assertions.Add(Assertion("sprint20-census-anchor-" + anchor[0],
                        "exact validated donor blueprint resolves",
                        anchor[1] + ";name=" + (unit == null ? "missing" : unit.name),
                        found, "exact asset id, never a name search"));
                    if (!found)
                        throw new InvalidOperationException(
                            "Census anchor missing: " + anchor[0]);
                    selected.Add(unit);
                }

                BlueprintUnit[] discovered = all
                    .Where(value => ArachnidRigSurveyPolicy.Matches(value.name,
                        ArachnidRigSurveyPolicy.DiscoveryTerms))
                    .Where(value => ArachnidRigSurveyPolicy.AnchorRank(value.AssetGuid)
                        == int.MaxValue)
                    .OrderBy(value => value.name, StringComparer.Ordinal)
                    .ToArray();
                // A scorpion in the live library would change this sprint's
                // whole premise, so it is looked for by name and reported
                // either way rather than assumed absent.
                BlueprintUnit[] scorpions = all.Where(value =>
                    ArachnidRigSurveyPolicy.Matches(value.name,
                        new[] { "scorpion" })).ToArray();
                // The first run reported one and could not say what it was,
                // which is half a finding. Name every one, with the prefab it
                // would be authored against.
                document0ScorpionRows = new JArray(scorpions.Select(value =>
                    (JToken)new JObject {
                        ["blueprintName"] = value.name,
                        ["nativeBlueprint"] = value.AssetGuid,
                        ["size"] = value.Size.ToString(),
                        ["prefab"] = value.Prefab == null ? null
                            : value.Prefab.AssetId,
                        ["unitType"] = value.Type == null ? null : value.Type.name
                    }));

                // One rig per prefab: many blueprints share a handful of views,
                // and the view is what a body is authored against.
                var byPrefab = new Dictionary<string, BlueprintUnit>(
                    StringComparer.Ordinal);
                foreach (BlueprintUnit unit in selected
                    .Concat(discovered.Where(value =>
                        ArachnidRigSurveyPolicy.IsPriority(value.name)))
                    .Concat(discovered.Where(value =>
                        !ArachnidRigSurveyPolicy.IsPriority(value.name))))
                {
                    string prefab = unit.Prefab == null ? null : unit.Prefab.AssetId;
                    if (string.IsNullOrEmpty(prefab) || byPrefab.ContainsKey(prefab))
                        continue;
                    byPrefab.Add(prefab, unit);
                    if (byPrefab.Count >= ArachnidRigSurveyPolicy.MaximumSurveyedPrefabs)
                        break;
                }

                var rows = new JArray();
                var document = new JObject {
                    ["installedScorpions"] = document0ScorpionRows,
                    ["scope"] = "Sprint 20 arachnid donor census: detached read-only view prefabs only. " +
                        "No campaign actor is spawned, no save is read or written, no native asset is modified.",
                    ["nativeData"] = "skeleton names, hierarchy, rest transforms, bind positions and rotations, " +
                        "renderer and material names and flags, view scale and bounds. No vertices, indices, UVs, " +
                        "bind matrices, textures, shader source or animation curves.",
                    ["question"] = "The donor is already decided. This census measures it: the full bone roster, " +
                        "the bind frame, and the length of the abdomen chain a scorpion's metasoma is authored onto.",
                    ["decision"] = "This file reports. It does not choose a donor and applies no pass mark.",
                    ["recordedInsectRigSha256"] =
                        ArachnidRigSurveyPolicy.RecordedInsectRigSha256,
                    ["installedScorpionBlueprints"] = scorpions.Length,
                    ["anchorCount"] = ArachnidRigSurveyPolicy.AnchorDonors.Length,
                    ["discoveredBlueprintCount"] = discovered.Length,
                    ["surveyedPrefabCount"] = byPrefab.Count,
                    ["prefabCap"] = ArachnidRigSurveyPolicy.MaximumSurveyedPrefabs,
                    ["rows"] = rows
                };

                int captured = 0;
                JObject anchorRow = null;
                foreach (KeyValuePair<string, BlueprintUnit> pair in byPrefab
                    .OrderBy(value => ArachnidRigSurveyPolicy.AnchorRank(
                        value.Value.AssetGuid))
                    .ThenBy(value => value.Value.name, StringComparer.Ordinal))
                {
                    BlueprintUnit unit = pair.Value;
                    string key = ArachnidRigSurveyPolicy.AnchorKey(unit.AssetGuid)
                        ?? unit.name;
                    JObject row;
                    try
                    {
                        UnitEntityView prefab = unit.Prefab.Load(false);
                        if (prefab == null)
                            throw new InvalidOperationException("No loadable view prefab.");
                        row = CapturePrimateCensusViewMetadata(prefab, key,
                            unit.AssetGuid, unit.name, pair.Key);
                        row["isAnchor"] = ArachnidRigSurveyPolicy.AnchorRank(
                            unit.AssetGuid) != int.MaxValue;
                        row["size"] = unit.Size.ToString();
                        row["unitType"] = unit.Type == null ? null : unit.Type.name;
                        AddArachnidChainReport(row);
                        captured++;
                        if ((bool)row["isAnchor"]) anchorRow = row;
                    }
                    catch (Exception exception)
                    {
                        row = new JObject {
                            ["key"] = key, ["nativeBlueprint"] = unit.AssetGuid,
                            ["blueprintName"] = unit.name, ["prefab"] = pair.Key,
                            ["unreadable"] =
                                DescribeExpandedSummoningCorrectionException(exception)
                        };
                    }
                    rows.Add(row);
                }

                document["capturedPrefabCount"] = captured;
                string fileName = "sprint20-arachnid-donor-census.json";
                File.WriteAllText(Path.Combine(_request.EvidenceDirectory, fileName),
                    document.ToString(Formatting.Indented));
                WriteLifecycleStage("expanded-summoning-arachnid-census-written");

                assertions.Add(Assertion("sprint20-census-captured",
                    "every selected donor prefab yields a complete bind frame",
                    "file=" + fileName + ";surveyed=" + byPrefab.Count +
                        ";captured=" + captured + ";discoveredBlueprints=" +
                        discovered.Length,
                    captured == byPrefab.Count && captured > 0,
                    "detached read-only prefabs; no spawn, no save, no asset mutation"));
                assertions.Add(Assertion("sprint20-census-bounded",
                    "one launch stays within the fixed prefab cap",
                    byPrefab.Count + " <= " +
                        ArachnidRigSurveyPolicy.MaximumSurveyedPrefabs,
                    byPrefab.Count <= ArachnidRigSurveyPolicy.MaximumSurveyedPrefabs,
                    "targets fixed in source; never request-supplied"));
                assertions.Add(Assertion("sprint20-census-anchor-surveyed",
                    "the Giant Spider rig every shipped insect mesh rides is in the census",
                    "anchor=" + ArachnidRigSurveyPolicy.GiantSpiderDonorGuid,
                    anchorRow != null,
                    "baseline for the scorpion's geometry, not a result"));
                // The whole premise of reusing this rig. Reported as a measured
                // finding rather than inherited from Sprint 14's notes.
                // The first run failed here, and the assertion was wrong
                // rather than the installation. "The game has no scorpion" is
                // a conclusion Sprint 14 drew, not a bound this census may
                // impose: a census reports what is there. What it must do is
                // name and measure whatever it finds, so the donor decision
                // is made by reading this file.
                assertions.Add(Assertion("sprint20-census-scorpions-named",
                    "every installed scorpion blueprint is named with its prefab",
                    "scorpionBlueprints=" + scorpions.Length + ";named=" +
                        document0ScorpionRows.Count,
                    document0ScorpionRows.Count == scorpions.Length,
                    "counting one without naming it is half a finding"));
                assertions.Add(Assertion("sprint20-census-scorpion-rig-surveyed",
                    "any scorpion prefab the library has is measured, not just counted",
                    string.Join(";", scorpions.Select(value =>
                        value.name + "=" + (value.Prefab == null ? "none" :
                            (rows.OfType<JObject>().Any(row =>
                                (string)row["prefab"] == value.Prefab.AssetId)
                                ? "surveyed" : "not-surveyed")))),
                    scorpions.All(value => value.Prefab == null ||
                        rows.OfType<JObject>().Any(row =>
                            (string)row["prefab"] == value.Prefab.AssetId)),
                    "a rig that changes the donor decision must be measured"));
                assertions.Add(Assertion("sprint20-census-abdomen-chain-measured",
                    "the abdomen chain a metasoma is authored onto is counted",
                    anchorRow == null ? "no anchor row" :
                        "abdomenChain=" + anchorRow["abdomenChainBones"] +
                        ";chelaChain=" + anchorRow["chelaChainBones"] +
                        ";fourthLeg=" + anchorRow["fourthLegBones"],
                    anchorRow != null &&
                        (int)anchorRow["abdomenChainBones"] > 0,
                    "a tail authored onto a chain nobody measured is a guess"));
            }
            catch (Exception exception)
            {
                assertions.Add(Assertion("sprint20-arachnid-census-exception",
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
        /// Count the three chains this sprint's geometry depends on, from the
        /// bone names the capture already holds.
        ///
        /// <para>Named chains rather than a raw roster because that is what
        /// the authoring question actually is: how many metasomal segments the
        /// abdomen chain can drive, how much articulation the pedipalps offer
        /// a chela, and whether the fourth leg pair - empty on the ants, wings
        /// on the beetles - is a real leg on this rig.</para>
        /// </summary>
        private static void AddArachnidChainReport(JObject row)
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
            row["abdomenChainBones"] = ArachnidRigSurveyPolicy
                .AbdomenChainCandidates.Count(names.Contains);
            row["abdomenChainPresent"] = new JArray(ArachnidRigSurveyPolicy
                .AbdomenChainCandidates.Where(names.Contains));
            row["chelaChainBones"] = ArachnidRigSurveyPolicy
                .ChelaChainCandidates.Count(names.Contains);
            row["chelaChainPresent"] = new JArray(ArachnidRigSurveyPolicy
                .ChelaChainCandidates.Where(names.Contains));
            row["fourthLegBones"] = ArachnidRigSurveyPolicy
                .FourthLegCandidates.Count(names.Contains);
            row["fourthLegPresent"] = new JArray(ArachnidRigSurveyPolicy
                .FourthLegCandidates.Where(names.Contains));
            row["allBoneNames"] = new JArray(names.OrderBy(value => value,
                StringComparer.Ordinal));
        }
    }
}
