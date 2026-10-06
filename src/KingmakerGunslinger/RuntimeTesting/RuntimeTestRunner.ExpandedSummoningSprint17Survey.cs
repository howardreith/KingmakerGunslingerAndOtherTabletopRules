using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Entities;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        private ExpandedSummoningCorrectionFixture _sprint17SurveyFixture;
        private readonly List<RuntimeTestAssertion> _sprint17SurveyAssertions =
            new List<RuntimeTestAssertion>();
        private int _sprint17SurveyLoadingFrames;
        private int _sprint17SurveySpawnFrame;
        private DateTime _sprint17SurveyStartedUtc;
        private bool _sprint17SurveyComplete;

        /// <summary>
        /// Research only: exact native source/view pairs and private bind-frame
        /// metadata. No new creatures, arbitrary asset requests, native geometry
        /// export, visual qualification, gameplay qualification or save writes.
        /// </summary>
        private void PollSprint17RigSurvey()
        {
            if (_sprint17SurveyComplete) return;
            try
            {
                if (_sprint17SurveyFixture == null)
                {
                    string loading;
                    if (ExpandedSummoningLoadingActive(out loading))
                    {
                        if (++_sprint17SurveyLoadingFrames < ExpandedSummoningLoadingGateFrames)
                            return;
                        throw new InvalidOperationException("Native loading did not settle: " + loading);
                    }
                    _sprint17SurveyFixture = BeginExpandedSummoningCorrectionFixture(
                        "KMG_Runtime_Sprint17_RigSurveyCaster");
                    foreach (string key in SerpentineRigSurveyPolicy.Keys)
                    {
                        BlueprintUnit native = _sprint17SurveyFixture.Blueprints.OfType<BlueprintUnit>()
                            .Single(value => value.AssetGuid == SerpentineRigSurveyPolicy.NativeBlueprint(key));
                        string prefab = native.Prefab == null ? null : native.Prefab.AssetId;
                        bool exactNative = SerpentineRigSurveyPolicy.MatchesNativeSource(
                            key, native.AssetGuid, prefab);
                        _sprint17SurveyAssertions.Add(Assertion("sprint17-native-source-" + key,
                            "exact recorded native blueprint/view pair, not optional Eidolon",
                            native.name + ";guid=" + native.AssetGuid + ";prefab=" + prefab,
                            exactNative, "actual loaded blueprint, not a name search or fallback"));
                        if (!exactNative) throw new InvalidOperationException("Native rig source changed: " + key);

                        // Use the existing own-tier summon action without the
                        // convenience helper that removes appearance buffs.
                        UnitEntityData unit = CastExpandedSummoningVariant(
                            _sprint17SurveyFixture.Blueprints, _sprint17SurveyFixture.Caster,
                            ExpandedSummoningOwnTierVariant(key, SummonMultiplicity.One), null,
                            _sprint17SurveyFixture.Evidence).Single();
                        _sprint17SurveyFixture.Created.Add(unit);
                        SetExpandedSummoningBrainActive(unit, false);
                        string projectPrefab = unit.Blueprint.Prefab == null ? null :
                            unit.Blueprint.Prefab.AssetId;
                        bool exactProject = projectPrefab == prefab;
                        _sprint17SurveyAssertions.Add(Assertion("sprint17-survey-view-source-" + key,
                            "existing project summon retains the exact native view reference",
                            unit.Blueprint.name + ";prefab=" + projectPrefab, exactProject,
                            "existing public route; no new blueprint or view registration"));
                        if (!exactProject) throw new InvalidOperationException("Project rig source changed: " + key);
                    }
                    _sprint17SurveySpawnFrame = Time.frameCount;
                    _sprint17SurveyStartedUtc = DateTime.UtcNow;
                    return;
                }
                if (DateTime.UtcNow - _sprint17SurveyStartedUtc > TimeSpan.FromSeconds(45))
                    throw new TimeoutException("Bounded native rig settlement exceeded 45 seconds.");
                if (Time.frameCount - _sprint17SurveySpawnFrame < 60) return;
                for (int index = 0; index < SerpentineRigSurveyPolicy.Keys.Length; index++)
                {
                    string key = SerpentineRigSurveyPolicy.Keys[index];
                    UnitEntityData unit = _sprint17SurveyFixture.Created[index];
                    JObject metadata = CaptureSprint17RigMetadata(unit, key);
                    string fileName = "sprint17-" + key + "-rig-survey.json";
                    File.WriteAllText(Path.Combine(_request.EvidenceDirectory, fileName),
                        metadata.ToString(Formatting.Indented));
                    _sprint17SurveyAssertions.Add(Assertion("sprint17-rig-metadata-" + key,
                        "nonempty complete native bind frames; metadata only",
                        "file=" + fileName + ";skinnedRenderers=" +
                            ((JArray)metadata["skinnedRenderers"]).Count +
                            ";settlementFrames=" + (Time.frameCount - _sprint17SurveySpawnFrame),
                        true, "No visibility forcing or appearance-buff removal. Research, not visual/AI qualification."));
                }
            }
            catch (Exception exception)
            {
                _sprint17SurveyAssertions.Add(Assertion("sprint17-rig-survey-exception",
                    "bounded metadata capture without exception",
                    DescribeExpandedSummoningCorrectionException(exception), false,
                    "request-local research only"));
            }
            FinishSprint17RigSurvey();
        }

        private void FinishSprint17RigSurvey()
        {
            bool cleaned = false;
            try { EndExpandedSummoningCorrectionFixture(_sprint17SurveyFixture, out cleaned); }
            catch (Exception exception)
            {
                _sprint17SurveyAssertions.Add(Assertion("sprint17-rig-survey-cleanup-exception",
                    "exact owned-only cleanup", DescribeExpandedSummoningCorrectionException(exception),
                    false, "native destruction; no inferred ownership of unrelated actors"));
            }
            _sprint17SurveyAssertions.Add(Assertion("sprint17-rig-survey-cleanup",
                "original unit/party references and area membership exactly restored",
                "cleaned=" + cleaned + ";nativeUnits=" + (_sprint17SurveyFixture == null ? 0 :
                    _sprint17SurveyFixture.UnitsBefore.Length) + ";party=" +
                    (_sprint17SurveyFixture == null ? 0 : _sprint17SurveyFixture.PartyBefore.Length),
                cleaned, "owned fixture units/caster only; no save write"));
            _sprint17SurveyAssertions.Add(Assertion("loaded-mod-version", _request.ExpectedModVersion,
                _context.ModEntry.Info.Version, _context.ModEntry.Info.Version == _request.ExpectedModVersion,
                "Unity Mod Manager ModEntry.Info.Version"));
            _sprint17SurveyComplete = true;
            Complete(CreateResult(_sprint17SurveyAssertions.All(value => value.Status == "PASS") ?
                "PASS" : "FAIL", _sprint17SurveyAssertions, null));
        }

        private static JObject CaptureSprint17RigMetadata(UnitEntityData unit, string key)
        {
            if (unit == null || unit.Destroyed || !unit.IsInState || unit.View == null)
                throw new InvalidOperationException("No live native survey view for " + key);
            SkinnedMeshRenderer[] skins = unit.View.GetComponentsInChildren<SkinnedMeshRenderer>(true)
                .Where(value => value != null && value.sharedMesh != null).ToArray();
            if (skins.Length == 0) throw new InvalidOperationException("No skinned native rig for " + key);
            var document = new JObject {
                ["scope"] = "Sprint 17 research only; no gameplay, AI, intact-frame or contact qualification",
                ["space"] = "renderer-local bind frame; no native vertices, indices, UVs, textures or animation curves",
                ["key"] = key, ["nativeBlueprint"] = SerpentineRigSurveyPolicy.NativeBlueprint(key),
                ["projectBlueprint"] = unit.Blueprint.AssetGuid, ["blueprintName"] = unit.Blueprint.name,
                ["prefab"] = unit.Blueprint.Prefab.AssetId, ["view"] = unit.View.name,
                ["viewScale"] = SurveyVector(unit.View.transform.localScale),
                ["viewActive"] = unit.View.gameObject.activeInHierarchy,
                ["components"] = new JArray(unit.View.GetComponentsInChildren<Component>(true)
                    .Where(value => value != null).Select(value => value.GetType().FullName)
                    .Distinct().OrderBy(value => value, StringComparer.Ordinal).ToArray())
            };
            var entries = new JArray();
            foreach (SkinnedMeshRenderer skin in skins)
            {
                Transform[] bones = skin.bones ?? new Transform[0];
                Matrix4x4[] poses = skin.sharedMesh.bindposes ?? new Matrix4x4[0];
                if (bones.Length == 0 || bones.Length != poses.Length || bones.Any(value => value == null))
                    throw new InvalidOperationException("Incomplete native bind frame: " + key + "/" + skin.name);
                var entry = new JObject {
                    ["renderer"] = skin.name, ["meshName"] = skin.sharedMesh.name,
                    ["vertexCount"] = skin.sharedMesh.vertexCount, ["enabled"] = skin.enabled,
                    ["active"] = skin.gameObject.activeInHierarchy,
                    ["localBoundsCenter"] = SurveyVector(skin.localBounds.center),
                    ["localBoundsSize"] = SurveyVector(skin.localBounds.size),
                    ["rootBone"] = skin.rootBone == null ? null : skin.rootBone.name,
                    ["boneCount"] = bones.Length, ["bindPoseCount"] = poses.Length
                };
                var frames = new JArray();
                for (int index = 0; index < bones.Length; index++)
                {
                    Matrix4x4 bind = poses[index].inverse;
                    Quaternion rotation = Quaternion.LookRotation(bind.GetColumn(2), bind.GetColumn(1));
                    frames.Add(new JObject {
                        ["index"] = index, ["name"] = bones[index].name,
                        ["parent"] = bones[index].parent == null ? null : bones[index].parent.name,
                        ["bindPosition"] = SurveyVector(bind.MultiplyPoint3x4(Vector3.zero)),
                        ["bindRotation"] = new JArray(rotation.x, rotation.y, rotation.z, rotation.w)
                    });
                }
                entry["bones"] = frames;
                entries.Add(entry);
            }
            document["skinnedRenderers"] = entries;
            document["renderers"] = new JArray(unit.View.GetComponentsInChildren<Renderer>(true)
                .Where(value => value != null).Select(value => new JObject {
                    ["name"] = value.name, ["type"] = value.GetType().FullName,
                    ["enabled"] = value.enabled, ["active"] = value.gameObject.activeInHierarchy
                }));
            // Some native views use Owlcat's custom animation manager rather
            // than a Unity controller. An empty clip-name list is a finding,
            // not permission to load or export animation assets.
            document["unityControllerClipNames"] = new JArray(unit.View
                .GetComponentsInChildren<Animator>(true).Where(value => value != null &&
                    value.runtimeAnimatorController != null)
                .SelectMany(value => value.runtimeAnimatorController.animationClips)
                .Where(value => value != null).Distinct().Select(value => new JObject {
                    ["name"] = value.name, ["durationSeconds"] = value.length
                }));
            return document;
        }

        private static JArray SurveyVector(Vector3 value)
        {
            return new JArray(value.x, value.y, value.z);
        }
    }
}
