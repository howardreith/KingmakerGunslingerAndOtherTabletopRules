using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.View;
using Kingmaker.Visual.Animation.Kingmaker;
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
                CaptureSprint17HybridWeaponPrefab();
                CaptureSprint17ManufacturedActionPrefabs();
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
            JObject document = CaptureSprint17ViewMetadata(unit.View, key,
                SerpentineRigSurveyPolicy.NativeBlueprint(key), unit.Blueprint.AssetGuid,
                unit.Blueprint.name, unit.Blueprint.Prefab.AssetId, true);
            document["unitWorldPosition"] = SurveyVector(unit.Position);
            document["viewWorldPosition"] = SurveyVector(unit.View.transform.position);
            return document;
        }

        private void CaptureSprint17HybridWeaponPrefab()
        {
            BlueprintUnit native = _sprint17SurveyFixture.Blueprints.OfType<BlueprintUnit>()
                .Single(value => value.AssetGuid == SerpentineRigSurveyPolicy.HybridWeaponBlueprint);
            BlueprintItemWeapon weapon = native.Body == null ? null : native.Body.PrimaryHand as BlueprintItemWeapon;
            string prefabId = native.Prefab == null ? null : native.Prefab.AssetId;
            bool exact = SerpentineRigSurveyPolicy.MatchesHybridWeaponSource(native.AssetGuid,
                prefabId, weapon == null ? null : weapon.AssetGuid,
                native.Body == null || native.Body.SecondaryHand != null);
            _sprint17SurveyAssertions.Add(Assertion("sprint17-native-hybrid-weapon-source",
                "exact archived greatclub/no-offhand native source and prefab",
                native.name + ";prefab=" + prefabId + ";weapon=" + (weapon == null ? "missing" : weapon.AssetGuid),
                exact, "read-only donor metadata; no native campaign NPC spawned"));
            if (!exact) throw new InvalidOperationException("Hybrid weapon source differs from archived census.");

            // Load and inspect the shared prefab read-only. Do not instantiate,
            // activate, attach data, drive an action or mutate any native asset.
            // Existing summon-view observation uses this exact UnitViewLink API.
            UnitEntityView prefab = native.Prefab.Load(false);
            if (prefab == null) throw new InvalidOperationException("No native hybrid weapon prefab.");
            JObject document = CaptureSprint17ViewMetadata(prefab, SerpentineRigSurveyPolicy.HybridWeaponKey,
                native.AssetGuid, null, native.name, prefabId, false);
            document["nativePrimaryWeapon"] = weapon.AssetGuid;
            document["nativePrimaryWeaponName"] = weapon.name;
            document["nativePrimaryWeaponCategory"] = weapon.Category.ToString();
            string fileName = "sprint17-" + SerpentineRigSurveyPolicy.HybridWeaponKey + "-rig-survey.json";
            File.WriteAllText(Path.Combine(_request.EvidenceDirectory, fileName), document.ToString(Formatting.Indented));
            _sprint17SurveyAssertions.Add(Assertion("sprint17-hybrid-weapon-prefab-metadata",
                "complete bind frames and static weapon anchors from an unmodified detached prefab",
                "file=" + fileName + ";skins=" + ((JArray)document["skinnedRenderers"]).Count,
                true, "not attached animation, two-hand grip, spear contact or gameplay qualification"));
        }

        private void CaptureSprint17ManufacturedActionPrefabs()
        {
            var unitsBefore = Game.Instance.State.Units.ToArray();
            var ownedBefore = _sprint17SurveyFixture.Created.ToArray();
            var rows = new JArray();
            string path = Path.Combine(_request.EvidenceDirectory, "sprint17-native-manufactured-actions.json");
            var document = new JObject {
                ["scope"] = "eleven fixed archived Lizardfolk prefabs; detached read-only metadata; no adoption or gameplay qualification",
                ["nativeData"] = "names, types, counts, paths, durations and event times only; no vertices, bind matrices, textures or curves",
                ["rows"] = rows };
            // Preserve completed rows even if a later native source cannot be
            // observed. Such an incomplete census remains a failed scenario.
            try
            {
                foreach (string[] source in SerpentineRigSurveyPolicy.ManufacturedPrefabSources)
                {
                    BlueprintUnit native = _sprint17SurveyFixture.Blueprints.OfType<BlueprintUnit>()
                        .Single(value => value.AssetGuid == source[0]);
                    if (native.Prefab == null || !SerpentineRigSurveyPolicy.MatchesManufacturedPrefab(
                        native.AssetGuid, native.Prefab.AssetId) || native.Prefab.AssetId != source[1])
                        throw new InvalidOperationException("Manufactured-action source differs from archived identity.");
                    UnitEntityView prefab = native.Prefab.Load(false);
                    if (prefab == null) throw new InvalidOperationException("Native manufactured-action prefab missing.");
                    var managers = prefab.GetComponentsInChildren<UnitAnimationManager>(true);
                    if (managers.Length != 1 || managers[0].GetComponent<Animator>() == null)
                        throw new InvalidOperationException("Expected one native Animator manager: " + native.name);
                    var manager = managers[0];
                    var set = manager.AnimationSet;
                    if (set == null) throw new InvalidOperationException("Missing native action set: " + native.name);
                    var actions = set.Actions.ToArray();
                    if (actions.Length == 0 || actions.Length > 128 || actions.Any(value => value == null))
                        throw new InvalidOperationException("Native action-list census is incomplete or unbounded.");
                    var skins = prefab.GetComponentsInChildren<SkinnedMeshRenderer>(true);
                    var skinReferences = skins.Select(value => new { Skin = value, Mesh = value.sharedMesh,
                        Bones = value.bones, Materials = value.sharedMaterials }).ToArray();
                    var row = new JObject { ["blueprint"] = native.AssetGuid, ["name"] = native.name,
                        ["prefab"] = source[1], ["set"] = set.name,
                        ["humanFallback"] = ReferenceEquals(set, BlueprintRoot.Instance.HumanAnimationSet),
                        ["actionCount"] = actions.Length, ["transitionCount"] = set.Transitions.Count(),
                        ["hands"] = Sprint17NativeHandAttackCensus(kind => actions.OfType<UnitAnimationAction>()
                            .SingleOrDefault(value => value.Type == kind)),
                        ["actions"] = new JArray(actions.Select(value => new JObject {
                            ["name"] = value.name, ["class"] = value.GetType().FullName,
                            ["kind"] = value is UnitAnimationAction ? ((UnitAnimationAction)value).Type.ToString() : null })),
                        ["skins"] = new JArray(skins.Select(value => new JObject {
                            ["name"] = value.name, ["mesh"] = value.sharedMesh == null ? null : value.sharedMesh.name,
                            ["bones"] = value.bones.Length,
                            ["binds"] = value.sharedMesh == null ? -1 : value.sharedMesh.bindposes.Length,
                            ["bonePaths"] = new JArray(value.bones.Select(bone => {
                                if (bone == null) return (JToken)JValue.CreateNull();
                                var names = new List<string>();
                                for (Transform next = bone; next != null && next != prefab.transform; next = next.parent)
                                    names.Add(next.name);
                                names.Reverse(); return (JToken)new JValue(string.Join("/", names.ToArray())); })) })) };
                    bool unchanged = ReferenceEquals(manager.AnimationSet, set) && set.Actions.SequenceEqual(actions) &&
                        skinReferences.All(value => ReferenceEquals(value.Skin.sharedMesh, value.Mesh) &&
                            value.Skin.bones.SequenceEqual(value.Bones) && value.Skin.sharedMaterials.SequenceEqual(value.Materials));
                    row["nativeReferencesUnchanged"] = unchanged;
                    rows.Add(row);
                    if (!unchanged) throw new InvalidOperationException("Read-only prefab reference census changed: " + native.name);
                }
                bool noActors = Game.Instance.State.Units.SequenceEqual(unitsBefore) &&
                    _sprint17SurveyFixture.Created.SequenceEqual(ownedBefore);
                document["actorReferencesUnchanged"] = noActors;
                _sprint17SurveyAssertions.Add(Assertion("sprint17-native-manufactured-action-census",
                    "eleven exact detached prefab/action/skin censuses; native references and actor membership unchanged",
                    "rows=" + rows.Count + ";noActors=" + noActors, rows.Count == 11 && noActors,
                    "Negative/missing style is a finding, not compatibility, adopted playback, grip or contact proof."));
            }
            finally { File.WriteAllText(path, document.ToString(Formatting.Indented)); }
        }

        private static JObject CaptureSprint17ViewMetadata(UnitEntityView view, string key,
            string nativeBlueprint, string projectBlueprint, string blueprintName, string prefabId, bool attached)
        {
            SkinnedMeshRenderer[] skins = view.GetComponentsInChildren<SkinnedMeshRenderer>(true)
                .Where(value => value != null && value.sharedMesh != null).ToArray();
            if (skins.Length == 0) throw new InvalidOperationException("No skinned native rig for " + key);
            var document = new JObject {
                ["scope"] = "Sprint 17 research only; no gameplay, AI, intact-frame or contact qualification",
                ["space"] = "renderer-local bind frame; no native vertices, indices, UVs, textures or animation curves",
                ["sourceMode"] = attached ? "attached disposable summon; current-frame pose only" :
                    "detached native prefab; read-only, never instantiated or activated",
                ["key"] = key, ["nativeBlueprint"] = nativeBlueprint,
                ["projectBlueprint"] = projectBlueprint, ["blueprintName"] = blueprintName,
                ["prefab"] = prefabId, ["view"] = view.name,
                ["viewScale"] = SurveyVector(view.transform.localScale),
                ["viewActive"] = view.gameObject.activeInHierarchy,
                ["components"] = new JArray(view.GetComponentsInChildren<Component>(true)
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
                    ["rendererToViewPosition"] = SurveyVector(view.transform.InverseTransformPoint(skin.transform.position)),
                    ["rendererToViewScale"] = SurveyVector(new Vector3(
                        skin.transform.lossyScale.x / view.transform.lossyScale.x,
                        skin.transform.lossyScale.y / view.transform.lossyScale.y,
                        skin.transform.lossyScale.z / view.transform.lossyScale.z)),
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
                        ["sampledViewPosition"] = SurveyVector(view.transform.InverseTransformPoint(bones[index].position)),
                        ["bindPosition"] = SurveyVector(bind.MultiplyPoint3x4(Vector3.zero)),
                        ["bindRotation"] = new JArray(rotation.x, rotation.y, rotation.z, rotation.w)
                    });
                }
                entry["bones"] = frames;
                // Names/flags only. No proprietary material, texture pixels,
                // shader source, native mesh or animation data is exported.
                entry["materials"] = new JArray(skin.sharedMaterials.Select(material =>
                    material == null ? JValue.CreateNull() : (JToken)new JObject {
                        ["name"] = material.name,
                        ["shader"] = material.shader == null ? null : material.shader.name,
                        ["renderQueue"] = material.renderQueue,
                        ["hasMainTextureSlot"] = material.HasProperty("_MainTex")
                    }));
                entries.Add(entry);
            }
            document["skinnedRenderers"] = entries;
            document["renderers"] = new JArray(view.GetComponentsInChildren<Renderer>(true)
                .Where(value => value != null).Select(value => new JObject {
                    ["name"] = value.name, ["type"] = value.GetType().FullName,
                    ["enabled"] = value.enabled, ["active"] = value.gameObject.activeInHierarchy
                }));
            document["staticMeshAnchors"] = new JArray(view.GetComponentsInChildren<MeshRenderer>(true)
                .Where(value => value != null).Select(value => {
                    MeshFilter filter = value.GetComponent<MeshFilter>();
                    return new JObject {
                        ["name"] = value.name,
                        ["parent"] = value.transform.parent == null ? null : value.transform.parent.name,
                        ["viewPosition"] = SurveyVector(view.transform.InverseTransformPoint(value.transform.position)),
                        ["meshName"] = filter == null || filter.sharedMesh == null ? null : filter.sharedMesh.name,
                        ["enabled"] = value.enabled, ["active"] = value.gameObject.activeInHierarchy
                    };
                }));
            // Some native views use Owlcat's custom animation manager rather
            // than a Unity controller. An empty clip-name list is a finding,
            // not permission to load or export animation assets.
            document["unityControllerClipNames"] = new JArray(view
                .GetComponentsInChildren<Animator>(true).Where(value => value != null &&
                    value.runtimeAnimatorController != null)
                .SelectMany(value => value.runtimeAnimatorController.animationClips)
                .Where(value => value != null).Distinct().Select(value => new JObject {
                    ["name"] = value.name, ["durationSeconds"] = value.length
                }));
            var actions = new JArray();
            // A detached prefab does not prove attach-time action binding.
            // Do not invoke its animation manager just to fill this array.
            if (attached && view.AnimationManager != null)
                foreach (UnitAnimationType type in Enum.GetValues(typeof(UnitAnimationType)))
                {
                    var action = view.AnimationManager.GetAction(type);
                    if (action == null) continue;
                    var enumeration = action.Clips;
                    var clips = enumeration == null ? null : enumeration.ToArray();
                    int? clipCount = SerpentineRigSurveyPolicy.CountPresentClips(
                        clips == null ? null : clips.Select(value => value != null));
                    actions.Add(new JObject {
                        ["type"] = type.ToString(), ["actionClass"] = action.GetType().FullName,
                        ["clipEnumerationPresent"] = clips != null,
                        ["clipCount"] = clipCount.HasValue ? new JValue(clipCount.Value) : JValue.CreateNull(),
                        ["clips"] = clips == null ? JValue.CreateNull() : (JToken)new JArray(
                            clips.Where(value => value != null).Select(value => new JObject {
                                ["name"] = value.name, ["durationSeconds"] = value.length
                            }))
                    });
                }
            document["nativeAnimationActions"] = actions;
            return document;
        }

        private static JArray SurveyVector(Vector3 value)
        {
            return new JArray(value.x, value.y, value.z);
        }
    }
}
