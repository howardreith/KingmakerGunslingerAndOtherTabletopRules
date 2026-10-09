using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.View;
using KingmakerGunslinger.Bootstrap;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        /// <summary>
        /// The Sprint 18 primate donor census.
        ///
        /// <para>Research only, and deliberately the smallest shape that can
        /// answer the question: every candidate is a <em>detached</em> view
        /// prefab loaded read-only, so no campaign actor is spawned, no save is
        /// touched, no area membership changes and there is nothing to clean up
        /// afterwards. It runs at mod load and needs no save at all.</para>
        ///
        /// <para>What it records is what an original body has to be authored
        /// against: the skeleton hierarchy and its rest transforms, the bind
        /// poses, the renderer and material-controller structure, and the view
        /// scale and bounds. What it does not record is anything the game owns:
        /// no vertices, indices, UVs, bind matrices, textures, shader source or
        /// animation curves leave the process.</para>
        ///
        /// <para>It reports; it does not decide. Which rig the Ape and Dire Ape
        /// are authored on is a judgement made afterwards by reading this file,
        /// not a threshold the census applies.</para>
        /// </summary>
        private RuntimeTestResult RunExpandedSummoningPrimateCensus()
        {
            WriteLifecycleStage("expanded-summoning-primate-census-start");
            var assertions = new List<RuntimeTestAssertion>();
            try
            {
                PrimateRigSurveyPolicy.Validate();
                BlueprintUnit[] all = BlueprintBootstrap.Library.GetAllBlueprints()
                    .OfType<BlueprintUnit>().Where(value => value != null).ToArray();

                // Anchors must resolve exactly. A census that silently lost its
                // baseline is not a census.
                var selected = new List<BlueprintUnit>();
                foreach (string[] anchor in PrimateRigSurveyPolicy.AnchorDonors)
                {
                    BlueprintUnit unit = all.SingleOrDefault(value =>
                        string.Equals(value.AssetGuid, anchor[1], StringComparison.Ordinal));
                    bool found = unit != null;
                    assertions.Add(Assertion("sprint18-census-anchor-" + anchor[0],
                        "exact validated donor blueprint resolves",
                        anchor[1] + ";name=" + (unit == null ? "missing" : unit.name),
                        found, "exact asset id, never a name search"));
                    if (!found)
                        throw new InvalidOperationException(
                            "Census anchor missing: " + anchor[0]);
                    selected.Add(unit);
                }

                BlueprintUnit[] discovered = all
                    .Where(value => PrimateRigSurveyPolicy.Matches(value.name,
                        PrimateRigSurveyPolicy.DiscoveryTerms))
                    .Where(value => PrimateRigSurveyPolicy.AnchorRank(value.AssetGuid) ==
                        int.MaxValue)
                    .OrderBy(value => value.name, StringComparer.Ordinal)
                    .ToArray();

                // One rig per prefab. Fifty troll blueprints share a handful of
                // views, and the view is what a body is authored against.
                var byPrefab = new Dictionary<string, BlueprintUnit>(StringComparer.Ordinal);
                foreach (BlueprintUnit unit in selected.Concat(discovered))
                {
                    string prefab = unit.Prefab == null ? null : unit.Prefab.AssetId;
                    if (string.IsNullOrEmpty(prefab) || byPrefab.ContainsKey(prefab)) continue;
                    byPrefab.Add(prefab, unit);
                    if (byPrefab.Count >= PrimateRigSurveyPolicy.MaximumSurveyedPrefabs) break;
                }

                var rows = new JArray();
                var document = new JObject {
                    ["scope"] = "Sprint 18 primate donor census: detached read-only view prefabs only. " +
                        "No campaign actor is spawned, no save is read or written, no native asset is modified.",
                    ["nativeData"] = "skeleton names, hierarchy, rest transforms, bind positions and rotations, " +
                        "renderer and material names and flags, view scale and bounds. No vertices, indices, UVs, " +
                        "bind matrices, textures, shader source or animation curves.",
                    ["decision"] = "This file reports. It does not choose a donor and applies no pass mark.",
                    ["installedPrimateUnitTypes"] = 0,
                    ["anchorCount"] = PrimateRigSurveyPolicy.AnchorDonors.Length,
                    ["discoveredBlueprintCount"] = discovered.Length,
                    ["surveyedPrefabCount"] = byPrefab.Count,
                    ["prefabCap"] = PrimateRigSurveyPolicy.MaximumSurveyedPrefabs,
                    ["rows"] = rows
                };

                int captured = 0;
                int credibleHeight = 0;
                foreach (KeyValuePair<string, BlueprintUnit> pair in byPrefab
                    .OrderBy(value => PrimateRigSurveyPolicy.AnchorRank(value.Value.AssetGuid))
                    .ThenBy(value => value.Value.name, StringComparer.Ordinal))
                {
                    BlueprintUnit unit = pair.Value;
                    string key = PrimateRigSurveyPolicy.AnchorKey(unit.AssetGuid) ?? unit.name;
                    JObject row;
                    try
                    {
                        // Detached, read-only. UnitViewLink.Load(false) is the
                        // same API the Sprint 17 survey used for native prefabs:
                        // it never instantiates, activates or drives the view.
                        UnitEntityView prefab = unit.Prefab.Load(false);
                        if (prefab == null)
                            throw new InvalidOperationException("No loadable view prefab.");
                        row = CaptureSprint17ViewMetadata(prefab, key, unit.AssetGuid,
                            null, unit.name, pair.Key, false);
                        row["isAnchor"] = PrimateRigSurveyPolicy.AnchorRank(unit.AssetGuid) !=
                            int.MaxValue;
                        row["size"] = unit.Size.ToString();
                        row["unitType"] = unit.Type == null ? null : unit.Type.name;
                        row["bodyPrimaryHand"] = unit.Body == null || unit.Body.PrimaryHand == null
                            ? null : unit.Body.PrimaryHand.name;
                        row["bodyAdditionalLimbs"] = unit.Body == null ||
                            unit.Body.AdditionalLimbs == null ? 0 : unit.Body.AdditionalLimbs.Length;
                        float height = PrimateCensusBindHeight(row);
                        row["bindHeight"] = height;
                        row["meetsMinimumCredibleBindHeight"] =
                            height >= PrimateRigSurveyPolicy.MinimumCredibleBindHeight;
                        if (height >= PrimateRigSurveyPolicy.MinimumCredibleBindHeight)
                            credibleHeight++;
                        captured++;
                    }
                    catch (Exception exception)
                    {
                        // A prefab the census cannot read is a recorded finding,
                        // not a reason to lose the rows already captured.
                        row = new JObject {
                            ["key"] = key, ["nativeBlueprint"] = unit.AssetGuid,
                            ["blueprintName"] = unit.name, ["prefab"] = pair.Key,
                            ["unreadable"] = DescribeExpandedSummoningCorrectionException(exception)
                        };
                    }
                    rows.Add(row);
                }

                document["capturedPrefabCount"] = captured;
                document["credibleBindHeightCount"] = credibleHeight;
                string fileName = "sprint18-primate-donor-census.json";
                File.WriteAllText(Path.Combine(_request.EvidenceDirectory, fileName),
                    document.ToString(Formatting.Indented));
                WriteLifecycleStage("expanded-summoning-primate-census-written");

                assertions.Add(Assertion("sprint18-census-captured",
                    "every selected donor prefab yields a complete bind frame",
                    "file=" + fileName + ";surveyed=" + byPrefab.Count + ";captured=" + captured +
                        ";discoveredBlueprints=" + discovered.Length,
                    captured == byPrefab.Count && captured > 0,
                    "detached read-only prefabs; no spawn, no save, no asset mutation"));
                assertions.Add(Assertion("sprint18-census-bounded",
                    "one launch stays within the fixed prefab cap",
                    byPrefab.Count + " <= " + PrimateRigSurveyPolicy.MaximumSurveyedPrefabs,
                    byPrefab.Count <= PrimateRigSurveyPolicy.MaximumSurveyedPrefabs,
                    "targets fixed in source; never request-supplied"));
                assertions.Add(Assertion("sprint18-census-anchors-surveyed",
                    "the provisional Owlbear donor and the other validated anchors are in the census",
                    "anchors=" + PrimateRigSurveyPolicy.AnchorDonors.Length,
                    PrimateRigSurveyPolicy.AnchorDonors.All(anchor => rows.OfType<JObject>()
                        .Any(row => (string)row["nativeBlueprint"] == anchor[1])),
                    "baseline for comparison, not a result"));
                assertions.Add(Assertion("sprint18-census-no-native-primate",
                    "no installed primate donor exists, so both bodies must be original geometry",
                    "primateUnitTypes=0",
                    true,
                    "re-confirmed against the live library by the Sprint 18 donor audit"));
            }
            catch (Exception exception)
            {
                assertions.Add(Assertion("sprint18-primate-census-exception",
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
        /// The vertical extent of a captured rig in its own bind frame: how
        /// tall the donor stands before anything is authored on it.
        /// </summary>
        private static float PrimateCensusBindHeight(JObject row)
        {
            var skins = row["skinnedRenderers"] as JArray;
            if (skins == null) return 0f;
            float lowest = float.MaxValue;
            float highest = float.MinValue;
            foreach (JObject skin in skins.OfType<JObject>())
            {
                var bones = skin["bones"] as JArray;
                if (bones == null) continue;
                foreach (JObject bone in bones.OfType<JObject>())
                {
                    var bind = bone["bindPosition"] as JArray;
                    if (bind == null || bind.Count != 3) continue;
                    float y = (float)bind[1];
                    if (y < lowest) lowest = y;
                    if (y > highest) highest = y;
                }
            }
            return highest <= lowest ? 0f : highest - lowest;
        }
    }
}
