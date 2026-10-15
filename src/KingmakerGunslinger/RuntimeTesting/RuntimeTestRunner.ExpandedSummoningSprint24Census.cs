using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.View;
using Kingmaker.Visual.Animation.Kingmaker;
using KingmakerGunslinger.Bootstrap;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        /// <summary>
        /// The one bounded Sprint 24 theropod census. Both inputs were already
        /// identified by prior recorded surveys. This run closes only the
        /// missing questions: complete local/world rest transforms, bind frame,
        /// jaw/limb/tail chains, action clips and contact-event metadata,
        /// selection/collision/fader component inventory, and exact native feat
        /// graphs needed by the two printed profiles.
        /// </summary>
        private RuntimeTestResult RunExpandedSummoningSprint24TheropodCensus()
        {
            WriteLifecycleStage("expanded-summoning-sprint24-theropod-census-start");
            var assertions = new List<RuntimeTestAssertion>();
            var rows = new JArray();
            var facts = new JArray();
            var candidates = new JArray();
            var document = new JObject {
                ["scope"] = "Sprint 24 closed theropod census; two detached read-only native prefabs and exact native feat graphs; no spawn, save, asset mutation or gameplay event.",
                ["nativeData"] = "Hierarchy/rest/bind transforms, bounds, component/material/action/clip/contact metadata and blueprint graphs only. No geometry, pixels, animation curves, assemblies or private bundles are copied.",
                ["decision"] = "Research evidence only. Donor and carrier decisions are frozen after inspection, not asserted by this observer.",
                ["runId"] = _request.RunId,
                ["donors"] = rows,
                ["factAnchors"] = facts,
                ["criticalCandidates"] = candidates
            };

            try
            {
                Sprint24TheropodCensusPolicy.Validate();
                BlueprintScriptableObject[] all = BlueprintBootstrap.Library
                    .GetAllBlueprints().Where(value => value != null).ToArray();
                BlueprintUnit[] units = all.OfType<BlueprintUnit>().ToArray();
                int captured = 0;

                foreach (string[] anchor in Sprint24TheropodCensusPolicy.Donors)
                {
                    BlueprintUnit unit = units.SingleOrDefault(value =>
                        string.Equals(value.AssetGuid, anchor[1],
                            StringComparison.Ordinal));
                    if (unit == null)
                        throw new InvalidOperationException("creature=" + anchor[0] +
                            ";predicate=exact-donor;guid=" + anchor[1]);
                    string actualPrefab = unit.Prefab == null ? null : unit.Prefab.AssetId;
                    if (!string.Equals(actualPrefab, anchor[2],
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

                foreach (string[] anchor in
                    Sprint24TheropodCensusPolicy.FactAnchors)
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
                        Sprint24TheropodCensusPolicy.MatchesCriticalCandidate(
                            value.name))
                    .OrderBy(value => value.name, StringComparer.Ordinal)
                    .ThenBy(value => value.AssetGuid, StringComparer.Ordinal)
                    .Take(Sprint24TheropodCensusPolicy
                        .MaximumDiscoveredCriticalBlueprints + 1).ToArray();
                if (discovered.Length > Sprint24TheropodCensusPolicy
                    .MaximumDiscoveredCriticalBlueprints)
                    throw new InvalidOperationException(
                        "predicate=bounded-critical-discovery;actual=" +
                        discovered.Length);
                foreach (BlueprintScriptableObject value in discovered)
                    candidates.Add(new JObject {
                        ["guid"] = value.AssetGuid, ["name"] = value.name,
                        ["type"] = value.GetType().FullName,
                        ["graph"] = DescribeGraph(value, 0,
                            new HashSet<object>(
                                NativeDonorReferenceComparer.Instance), 8)
                    });

                string[] required = { "Pounce", "CriticalFocus",
                    "ImprovedCriticalBite", "ImprovedInitiative", "IronWill",
                    "SkillFocusPerception" };
                bool requiredFound = facts.OfType<JObject>()
                    .Where(value => required.Contains((string)value["key"]))
                    .All(value => (bool)value["found"]);
                assertions.Add(Assertion("sprint24-census-donors",
                    "both exact detached biped candidates are captured",
                    "captured=" + captured + ";expected=" +
                        Sprint24TheropodCensusPolicy.Donors.Length,
                    captured == Sprint24TheropodCensusPolicy.Donors.Length,
                    "read-only prefabs; no actor or save"));
                assertions.Add(Assertion("sprint24-census-required-facts",
                    "all already-released native carriers resolve exactly",
                    "required=" + string.Join(",", required), requiredFound,
                    "Diehard, Endurance and bleed absence remain reportable findings"));
                assertions.Add(Assertion("sprint24-census-bounded",
                    "closed inputs and bounded critical-name results",
                    "donors=" + rows.Count + ";facts=" + facts.Count +
                        ";candidates=" + candidates.Count,
                    rows.Count == 2 && facts.Count == 9 &&
                        candidates.Count <= Sprint24TheropodCensusPolicy
                            .MaximumDiscoveredCriticalBlueprints,
                    "fixed source-owned identities and terms"));
            }
            catch (Exception exception)
            {
                document["exception"] =
                    DescribeExpandedSummoningCorrectionException(exception);
                assertions.Add(Assertion("sprint24-census-capture",
                    "bounded read-only census completes",
                    document["exception"].ToString(), false,
                    "unknown metadata is not qualification"));
            }

            string fileName = "sprint24-theropod-census.json";
            RuntimeTestResultWriter.WriteAtomic(Path.Combine(
                _request.EvidenceDirectory, fileName),
                document.ToString(Formatting.Indented));
            WriteLifecycleStage("expanded-summoning-sprint24-theropod-census-written");
            assertions.Add(Assertion("loaded-mod-version",
                _request.ExpectedModVersion, _context.ModEntry.Info.Version,
                _context.ModEntry.Info.Version == _request.ExpectedModVersion,
                "Unity Mod Manager ModEntry.Info.Version"));
            return CreateResult(assertions.All(value => value.Status == "PASS")
                ? "PASS" : "FAIL", assertions, null);
        }

        private static JArray CaptureSprint24RestHierarchy(UnitEntityView view)
        {
            Transform[] transforms = view.GetComponentsInChildren<Transform>(true);
            return new JArray(transforms.Select(value => new JObject {
                ["path"] = Sprint24TransformPath(view.transform, value),
                ["name"] = value.name,
                ["parent"] = value.parent == null ? null : value.parent.name,
                ["active"] = value.gameObject.activeSelf,
                ["localPosition"] = SurveyVector(value.localPosition),
                ["localRotation"] = new JArray(value.localRotation.x,
                    value.localRotation.y, value.localRotation.z,
                    value.localRotation.w),
                ["localScale"] = SurveyVector(value.localScale),
                ["worldPosition"] = SurveyVector(value.position),
                ["worldRotation"] = new JArray(value.rotation.x,
                    value.rotation.y, value.rotation.z, value.rotation.w),
                ["worldScale"] = SurveyVector(value.lossyScale)
            }));
        }

        private static string Sprint24TransformPath(Transform root,
            Transform value)
        {
            var names = new List<string>();
            for (Transform next = value; next != null; next = next.parent)
            {
                names.Add(next.name);
                if (next == root) break;
            }
            names.Reverse();
            return string.Join("/", names.ToArray());
        }

        private static JObject CaptureSprint24Anatomy(UnitEntityView view)
        {
            string[] names = view.GetComponentsInChildren<Transform>(true)
                .Select(value => value.name).ToArray();
            return new JObject {
                ["pelvisOrHip"] = new JArray(names.Where(value =>
                    Sprint24NameHas(value, "pelvis", "hip"))),
                ["spineOrTorso"] = new JArray(names.Where(value =>
                    Sprint24NameHas(value, "spine", "torso"))),
                ["jawOrMouth"] = new JArray(names.Where(value =>
                    Sprint24NameHas(value, "jaw", "mouth"))),
                ["leftLegOrFoot"] = new JArray(names.Where(value =>
                    Sprint24NameHas(value, "leg_l", "foot_l", "l_leg",
                        "l_foot", "leftleg", "leftfoot"))),
                ["rightLegOrFoot"] = new JArray(names.Where(value =>
                    Sprint24NameHas(value, "leg_r", "foot_r", "r_leg",
                        "r_foot", "rightleg", "rightfoot"))),
                ["forelimbsOrHands"] = new JArray(names.Where(value =>
                    Sprint24NameHas(value, "arm", "hand", "claw"))),
                ["tail"] = new JArray(names.Where(value =>
                    Sprint24NameHas(value, "tail")))
            };
        }

        private static bool Sprint24NameHas(string value, params string[] terms)
        {
            return !string.IsNullOrEmpty(value) && terms.Any(term =>
                value.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0);
        }

        private static JArray CaptureSprint24AnimationManagers(
            UnitEntityView view)
        {
            return new JArray(view.GetComponentsInChildren<UnitAnimationManager>(
                true).Select(manager => {
                    var set = manager.AnimationSet;
                    var actions = set == null || set.Actions == null
                        ? null : set.Actions.ToArray();
                    return new JObject {
                        ["name"] = manager.name,
                        ["set"] = set == null ? null : set.name,
                        ["actionCount"] = actions == null
                            ? JValue.CreateNull() : new JValue(actions.Length),
                        ["nullActionSlots"] = actions == null
                            ? (JToken)JValue.CreateNull() : new JArray(actions
                                .Select((value, index) => new { value, index })
                                .Where(value => value.value == null)
                                .Select(value => value.index)),
                        ["actions"] = actions == null
                            ? (JToken)JValue.CreateNull() : new JArray(actions
                                .Select((action, index) => action == null
                                    ? (JToken)new JObject {
                                        ["slot"] = index, ["null"] = true }
                                    : new JObject {
                                        ["slot"] = index,
                                        ["name"] = action.name,
                                        ["class"] = action.GetType().FullName,
                                        ["type"] = action is UnitAnimationAction
                                            ? ((UnitAnimationAction)action).Type
                                                .ToString() : null,
                                        ["clips"] = action is UnitAnimationAction &&
                                            ((UnitAnimationAction)action).Clips != null
                                            ? (JToken)new JArray(
                                                ((UnitAnimationAction)action)
                                                    .Clips.Select(
                                                        Sprint17ClipMetadata))
                                            : JValue.CreateNull()
                                    }))
                    };
                }));
        }

        private static JArray CaptureSprint24Colliders(UnitEntityView view)
        {
            return new JArray(view.GetComponentsInChildren<Collider>(true)
                .Select(value => new JObject {
                    ["name"] = value.name,
                    ["path"] = Sprint24TransformPath(view.transform,
                        value.transform),
                    ["type"] = value.GetType().FullName,
                    ["enabled"] = value.enabled,
                    ["isTrigger"] = value.isTrigger,
                    ["boundsCenter"] = SurveyVector(value.bounds.center),
                    ["boundsSize"] = SurveyVector(value.bounds.size)
                }));
        }
    }
}
