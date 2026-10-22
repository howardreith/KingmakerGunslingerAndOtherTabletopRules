using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Harmony12;
using Kingmaker;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.RuleSystem.Rules.Abilities;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.Visual.Animation.Kingmaker;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Rendering;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        private sealed class Cleanup2ClothSeries
        {
            internal string Name;
            internal int Samples, ValidSamples;
            internal float ClothDisplacement, BakeDisplacement, BoneAngle;
            internal Vector3[] FirstCloth, FirstBake;
            internal Quaternion[] FirstBoneRotations;
            internal HashSet<string> ConsistentCoordinates;
            internal bool Motion { get { return ValidSamples >= 4 && ValidSamples == Samples && ClothDisplacement > .0001f && BakeDisplacement > .0001f && BoneAngle > .1f; } }
            internal JObject Summary()
            {
                return new JObject { ["stage"] = Name, ["samples"] = Samples, ["validSamples"] = ValidSamples,
                    ["maximumRawClothDisplacement"] = ClothDisplacement,
                    ["maximumRawBakeDisplacement"] = BakeDisplacement, ["maximumSourceBoneAngleDegrees"] = BoneAngle,
                    ["positiveMotionProved"] = Motion,
                    ["consistentCoordinateHypotheses"] = new JArray(ConsistentCoordinates ?? new HashSet<string>()),
                    ["motionFloor"] = "At least four valid samples, >0.0001 raw-coordinate displacement in Cloth and BakeMesh, and >0.1 degree native source-bone pose change. Zero coordinate error at a static pose is insufficient." };
            }
        }

        // No scale, proxy, Cloth setter, collider setter, physics driver or save
        // operation. Integration owns the exact new cloth-probe phase/fixture.
        private IEnumerable<int> ReviewSummonCleanup2ClothProbe(
            ExpandedSummoningCorrectionFixture fixture, Spellbook book)
        {
            if (!IsSummonCleanup2Review || !_request.ExitAfterCompletion ||
                (string)_request.Parameters?["reviewPhase"] != "cloth-probe" ||
                fixture == null || book == null || _sprint25Clock == null || _expandedSummoningPlayerPathCaptureActive ||
                _refinementUnit != null || _refinementAction != null)
                throw new InvalidOperationException("Exact idle Cleanup 2 cloth-probe fixture/book/clock required.");
            bool pause = Game.Instance.IsPaused, waiting = _sprint25WaitingForUnityAppearance;
            UnitEntityData[] oldCapture = ExpandedSummoningRuleCapture.ToArray();
            string[] oldEvents = ExpandedSummoningPlayerPathEvents.ToArray();
            string[] oldAnimationRecords = _refinementRecords.ToArray();
            int oldCasts = _expandedSummoningPlayerPathRuleCastCount, oldSpawns = _expandedSummoningPlayerPathSpawnActionCount;
            var hooks = HarmonyInstance.Create("kmg.cleanup2.cloth-probe." + _request.RunId);
            UnitEntityData actor = null;
            SkinnedMeshRenderer cape = null;
            Cloth cloth = null;
            Mesh originalMesh = null;
            Material[] originalMaterials = null;
            Transform[] originalBones = null;
            Transform originalRoot = null;
            Bounds originalBounds = default(Bounds);
            bool originalEnabled = false, expectedEnabled = false, rendererClaimed = false;
            ShadowCastingMode originalShadow = ShadowCastingMode.Off, expectedShadow = ShadowCastingMode.Off;
            UnitAnimationActionHandle animation = null;
            var series = new List<Cleanup2ClothSeries>();
            string originalClothState = null, originalColliderState = null;
            var watch = Stopwatch.StartNew();
            int pulseStart = _sprint25Clock.Pulses;
            double gameStart = Game.Instance.Player.GameTime.TotalSeconds;
            const BindingFlags statics = BindingFlags.Static | BindingFlags.NonPublic;
            try
            {
                hooks.Patch(typeof(RuleCastSpell).GetMethod("OnTrigger"), null,
                    new HarmonyMethod(typeof(RuntimeTestRunner).GetMethod("ExpandedSummoningPlayerPathRuleCastPostfix", statics)), null);
                hooks.Patch(typeof(ContextActionSpawnMonster).GetMethod("RunAction"),
                    new HarmonyMethod(typeof(RuntimeTestRunner).GetMethod("ExpandedSummoningPlayerPathSpawnPrefix", statics)), null, null);
                _expandedSummoningPlayerPathCaptureActive = true;
                Game.Instance.IsPaused = false;
                actor = Cleanup2CastGiant(fixture, book, Cleanup2GiantRoute(fixture, "frost-giant", SummonFamily.NaturesAlly));
                foreach (int frame in Cleanup2WaitAppearance(actor, "cloth-probe-frost")) yield return frame;
                var candidates = actor.View.GetComponentsInChildren<SkinnedMeshRenderer>(true).Where(value =>
                    value.name == "Frozen_cape1" && value.GetComponent<Cloth>() != null).ToArray();
                if (candidates.Length != 1) throw new InvalidOperationException("Expected one exact native Frozen_cape1 Cloth renderer.");
                cape = candidates[0]; cloth = cape.GetComponent<Cloth>();
                originalMesh = cape.sharedMesh; originalBones = cape.bones; originalRoot = cape.rootBone;
                originalMaterials = cape.sharedMaterials;
                originalBounds = cape.localBounds; originalEnabled = expectedEnabled = cape.enabled;
                originalShadow = expectedShadow = cape.shadowCastingMode;
                rendererClaimed = true;
                originalClothState = Cleanup2ClothConfiguration(cloth).ToString();
                originalColliderState = Cleanup2ClothColliderState(actor, false).ToString();
                Cleanup2Check("cloth-probe-intake", originalMesh != null && originalBones != null && originalRoot != null && originalEnabled && cloth.enabled,
                    new JObject { ["unit"] = actor.UniqueId, ["blueprint"] = actor.Blueprint.AssetGuid,
                        ["prefab"] = actor.Blueprint.Prefab?.AssetId,
                        ["renderer"] = cape.name, ["rendererInstance"] = cape.GetInstanceID(),
                        ["path"] = Cleanup2TransformPath(actor.View.transform, cape.transform),
                        ["mesh"] = originalMesh?.name, ["meshInstance"] = originalMesh == null ? 0 : originalMesh.GetInstanceID(),
                        ["bonePaletteCount"] = originalBones?.Length ?? 0,
                        ["rootBone"] = Cleanup2TransformPath(actor.View.transform, originalRoot),
                        ["materials"] = new JArray(originalMaterials.Select(Cleanup2Material)),
                        ["rendererEnabled"] = originalEnabled, ["shadowCastingMode"] = originalShadow.ToString(),
                        ["localBoundsCenter"] = SurveyVector(originalBounds.center), ["localBoundsSize"] = SurveyVector(originalBounds.size),
                        ["cloth"] = Cleanup2ClothConfiguration(cloth), ["colliders"] = Cleanup2ClothColliderState(actor, true),
                        ["shapeHierarchy"] = Cleanup2ShapeHierarchy(actor),
                        ["limits"] = "Actual canonical Frost SNA VII actor; native animation handles are a pose stimulus, not combat resolution. No Cloth, source skeleton, collider or scale field will be written." },
                    "A live enabled exact native cape and Cloth component are prerequisites; unavailable native simulation is unknown.");
                if (originalMesh == null || originalBones == null || originalRoot == null || !originalEnabled || !cloth.enabled)
                {
                    Cleanup2Check("cloth-probe-verdict", false, new JObject { ["verdict"] = "UNKNOWN", ["reason"] = "Native enabled Cloth/render input unavailable; no component was enabled by the probe." },
                        "Positive native simulation and motion must be observed before proposing render-only scale treatment.");
                    yield break;
                }
                _sprint25WaitingForUnityAppearance = false;
                // The existing qualified Dialog-host clock owns native action
                // and movement progression. This probe adds no second driver.
                foreach (string stage in new[] { "visible", "renderer-disabled", "restored", "shadows-only" })
                {
                    Cleanup2ClothRequireIdentity(cape, cloth, originalMesh, originalMaterials, originalBones, originalRoot,
                        expectedEnabled, expectedShadow, originalClothState, actor, originalColliderState);
                    bool enabled = stage != "renderer-disabled";
                    ShadowCastingMode shadow = stage == "shadows-only" ? ShadowCastingMode.ShadowsOnly : originalShadow;
                    JArray before = Cleanup2ClothColliderState(actor, true);
                    Matrix4x4 rootBefore = actor.View.transform.localToWorldMatrix;
                    Matrix4x4[] bonesBefore = originalBones.Select(bone => bone == null ? Matrix4x4.identity : bone.localToWorldMatrix).ToArray();
                    cape.enabled = expectedEnabled = enabled;
                    cape.shadowCastingMode = expectedShadow = shadow;
                    bool noTransformWrite = before.ToString() == Cleanup2ClothColliderState(actor, true).ToString() &&
                        rootBefore.Equals(actor.View.transform.localToWorldMatrix) && originalBones.Select((bone, i) =>
                            bone == null || bonesBefore[i].Equals(bone.localToWorldMatrix)).All(value => value);
                    Cleanup2Check("cloth-probe-transition-" + stage, noTransformWrite && cloth.enabled,
                        new JObject { ["stage"] = stage, ["rendererEnabled"] = cape.enabled,
                            ["shadowCastingMode"] = cape.shadowCastingMode.ToString(), ["clothEnabled"] = cloth.enabled,
                            ["rootSkeletonAndColliderWorldMatricesUnchangedByTransition"] = noTransformWrite,
                            ["colliders"] = before },
                        "Only the owned cape render flags change; same-frame root/skeleton/collider geometry and native Cloth enabled state remain exact.");
                    var current = new Cleanup2ClothSeries { Name = stage };
                    series.Add(current);
                    int updates = 0;
                    for (int segment = 0; segment < 3; segment++)
                    {
                        Cleanup2EndClothAnimation(ref animation);
                        string pose = segment == 0 ? "idle" : segment == 1 ? "MainHandAttack" : "Hit";
                        if (segment > 0)
                        {
                            UnitAnimationType type = segment == 1 ? UnitAnimationType.MainHandAttack : UnitAnimationType.Hit;
                            animation = StartRefinementAnimation(actor, type);
                            Cleanup2Check("cloth-probe-animation-" + stage + "-" + pose, animation != null,
                                new JObject { ["stage"] = stage, ["pose"] = pose, ["handle"] = animation != null,
                                    ["actionType"] = animation?.Action?.GetType().FullName,
                                    ["mechanicalAttackIssued"] = false, ["damageStimulus"] = false },
                                "An actual native animation handle supplies an explicit art-pose stimulus without attack/damage rules.");
                        }
                        int frames = segment == 0 ? 24 : segment == 1 ? 72 : 48;
                        for (int frame = 0; frame < frames; frame++)
                        {
                            if (++updates > 160 || watch.Elapsed.TotalSeconds > 120)
                                throw new TimeoutException("Bounded Cloth probe exceeded frame/wall-clock limit.");
                            Cleanup2ClothRequireIdentity(cape, cloth, originalMesh, originalMaterials, originalBones, originalRoot,
                                expectedEnabled, expectedShadow, originalClothState, actor, originalColliderState);
                            if (frame % 6 == 0)
                            {
                                JObject sample = Cleanup2ClothSample(actor, cape, cloth, current, pose, frame);
                                Cleanup2Check("cloth-probe-sample-" + stage + "-" + pose + "-" + frame,
                                    (bool)sample["finite"] && (bool)sample["countsStable"], sample,
                                    "Transient Cloth.vertices and BakeMesh current-pose arrays yield only counts, finite/motion metrics and aggregate coordinate-hypothesis errors.");
                            }
                            Game.Instance.IsPaused = false;
                            yield return 0;
                        }
                    }
                    Cleanup2EndClothAnimation(ref animation);
                    Cleanup2Check("cloth-probe-series-" + stage, current.Samples >= 4 && current.ValidSamples == current.Samples, current.Summary(),
                        "The full bounded series retains finite stable-count geometry; positive-motion and suppression behavior are evaluated explicitly in the verdict.");
                }
                bool baseline = series[0].Motion && series[2].Motion;
                bool disabled = baseline && series[1].Motion;
                bool shadows = baseline && series[3].Motion;
                var common = new HashSet<string>(series[0].ConsistentCoordinates ?? new HashSet<string>());
                common.IntersectWith(series[1].ConsistentCoordinates ?? new HashSet<string>());
                common.IntersectWith(series[2].ConsistentCoordinates ?? new HashSet<string>());
                var shadowCommon = new HashSet<string>(series[0].ConsistentCoordinates ?? new HashSet<string>());
                shadowCommon.IntersectWith(series[2].ConsistentCoordinates ?? new HashSet<string>());
                shadowCommon.IntersectWith(series[3].ConsistentCoordinates ?? new HashSet<string>());
                string verdict = !baseline ? "UNKNOWN_NO_POSITIVE_VISIBLE_AND_RESTORED_MOTION" :
                    disabled && common.Count > 0 ? "RENDERER_DISABLED_UPDATE_AND_COORDINATES_OBSERVED" :
                    shadows && shadowCommon.Count > 0 ? "SHADOWS_ONLY_UPDATE_AND_COORDINATES_OBSERVED" :
                    !disabled ? "RENDERER_DISABLED_UPDATE_NOT_PROVED" : "UNKNOWN_COORDINATE_RELATIONSHIP";
                Cleanup2Check("cloth-probe-verdict", baseline && ((disabled && common.Count > 0) || (shadows && shadowCommon.Count > 0)), new JObject {
                    ["verdict"] = verdict, ["rendererDisabledPositiveUpdating"] = disabled,
                    ["shadowsOnlyPositiveUpdating"] = shadows, ["commonCoordinateHypotheses"] = new JArray(common),
                    ["shadowsOnlyCommonCoordinateHypotheses"] = new JArray(shadowCommon),
                    ["series"] = new JArray(series.Select(item => item.Summary())),
                    ["clockPulses"] = _sprint25Clock.Pulses - pulseStart,
                    ["gameTimeDeltaSeconds"] = Game.Instance.Player.GameTime.TotalSeconds - gameStart,
                    ["wallSeconds"] = watch.Elapsed.TotalSeconds, ["scaleApplied"] = false,
                    ["scope"] = "This narrow probe observes native update/suppression behavior only. It does not authorize Cloth palette retargeting, cape proxy scaling, shader/highlight equivalence, timing mechanics or final giant visual acceptance. Cloth/collider configuration and all source mesh/palette identities remained exact; source bones/collider poses may move naturally with the requested native animation." },
                    "Positive before/hidden/restored motion plus a consistent <=5mm aggregate Cloth/Bake coordinate relationship is required; unsupported timing remains failure/unknown.");
            }
            finally
            {
                var failures = new List<string>();
                Action<string, Action> restore = (label, action) => { try { action(); } catch (Exception error) { failures.Add(label + ":" + error.GetType().Name + ":" + error.Message); } };
                restore("animation", () => Cleanup2EndClothAnimation(ref animation));
                restore("actor-commands", () => { if (actor != null && !actor.Destroyed) actor.Commands.InterruptAll(true); });
                restore("renderer", () => {
                    if (!rendererClaimed) return;
                    if (cape == null || cape.sharedMesh != originalMesh || cape.rootBone != originalRoot ||
                        !Cleanup2ClothSameBones(cape.bones, originalBones))
                        throw new InvalidOperationException("Foreign/destroyed cape mesh/palette; renderer fields not overwritten.");
                    if (cape.enabled == expectedEnabled) cape.enabled = originalEnabled;
                    else if (cape.enabled != originalEnabled) throw new InvalidOperationException("Foreign cape enabled state preserved.");
                    if (cape.shadowCastingMode == expectedShadow) cape.shadowCastingMode = originalShadow;
                    else if (cape.shadowCastingMode != originalShadow) throw new InvalidOperationException("Foreign cape shadow state preserved.");
                    // Bounds are observed, never assigned. Cloth/Unity may
                    // legitimately recompute them while native animation runs.
                });
                restore("unmodified-native-configuration", () => {
                    if (!rendererClaimed) return;
                    if (cloth == null || actor == null || actor.Destroyed ||
                        Cleanup2ClothConfiguration(cloth).ToString() != originalClothState ||
                        Cleanup2ClothColliderState(actor, false).ToString() != originalColliderState ||
                        !cape.sharedMaterials.SequenceEqual(originalMaterials))
                        throw new InvalidOperationException("Native Cloth/collider/material identity changed; no unowned state overwritten.");
                });
                _expandedSummoningPlayerPathCaptureActive = false;
                restore("capture-hooks", () => hooks.UnpatchAll(hooks.Id));
                restore("capture-state", () => {
                    ExpandedSummoningRuleCapture.Clear(); ExpandedSummoningRuleCapture.AddRange(oldCapture);
                    ExpandedSummoningPlayerPathEvents.Clear(); ExpandedSummoningPlayerPathEvents.AddRange(oldEvents);
                    _expandedSummoningPlayerPathRuleCastCount = oldCasts; _expandedSummoningPlayerPathSpawnActionCount = oldSpawns;
                    _refinementRecords.Clear(); _refinementRecords.AddRange(oldAnimationRecords);
                });
                _sprint25WaitingForUnityAppearance = waiting;
                restore("pause", () => Game.Instance.IsPaused = pause);
                Cleanup2Check("cloth-probe-owned-state-restored", failures.Count == 0, new JObject {
                    ["failures"] = new JArray(failures), ["rendererEnabledRestored"] = cape != null && cape.enabled == originalEnabled,
                    ["shadowModeRestored"] = cape != null && cape.shadowCastingMode == originalShadow,
                    ["clothStateWritten"] = false, ["colliderStateWritten"] = false, ["boundsWritten"] = false, ["geometryExported"] = false,
                    ["originalLocalBoundsCenter"] = SurveyVector(originalBounds.center), ["originalLocalBoundsSize"] = SurveyVector(originalBounds.size),
                    ["finalLocalBoundsCenter"] = cape == null ? null : SurveyVector(cape.localBounds.center),
                    ["finalLocalBoundsSize"] = cape == null ? null : SurveyVector(cape.localBounds.size),
                    ["positiveActorRegisteredForSharedTeardown"] = actor != null && fixture.Created.Contains(actor) },
                    "Owned render flags/animation/capture state restore before exact-positive shared actor teardown; no Cloth/collider/skeleton state is changed.");
                if (failures.Count > 0) throw new InvalidOperationException("Cloth probe restoration failed: " + string.Join("|", failures.ToArray()));
            }
        }

        private static void Cleanup2EndClothAnimation(ref UnitAnimationActionHandle handle)
        {
            if (handle == null) return;
            try { handle.IsActed = true; FinishExpandedSummoningAnimation(handle); }
            finally { handle = null; }
        }

        private static bool Cleanup2ClothSameBones(Transform[] current, Transform[] expected)
        { return current != null && expected != null && current.Length == expected.Length && current.Select((bone, i) => ReferenceEquals(bone, expected[i])).All(value => value); }

        private static JObject Cleanup2ClothConfiguration(Cloth cloth)
        {
            var coefficients = cloth.coefficients;
            return new JObject { ["instance"] = cloth.GetInstanceID(), ["enabled"] = cloth.enabled,
                ["useGravity"] = cloth.useGravity, ["worldVelocityScale"] = cloth.worldVelocityScale,
                ["coefficientCount"] = coefficients.Length, ["pinnedCoefficientCount"] = coefficients.Count(value => value.maxDistance == 0f),
                ["coefficientFingerprint"] = Cleanup2ClothCoefficientFingerprint(coefficients),
                ["maximumAllowedDistance"] = coefficients.Length == 0 ? 0f : coefficients.Max(value => value.maxDistance),
                ["capsuleColliderInstances"] = new JArray(cloth.capsuleColliders.Select(value => value == null ? 0 : value.GetInstanceID())),
                ["sphereColliderPairs"] = new JArray(cloth.sphereColliders.Select(value => new JArray(
                    value.first == null ? 0 : value.first.GetInstanceID(), value.second == null ? 0 : value.second.GetInstanceID()))),
                ["behaviours"] = new JArray(cloth.GetComponents<Behaviour>().Where(value => value != null).Select(value =>
                    new JObject { ["type"] = value.GetType().FullName, ["instance"] = value.GetInstanceID(), ["enabled"] = value.enabled })) };
        }

        private static string Cleanup2ClothCoefficientFingerprint(ClothSkinningCoefficient[] values)
        {
            // Stable content guard, not exported per-vertex simulation data.
            ulong hash = 14695981039346656037UL;
            unchecked
            {
                foreach (var value in values)
                foreach (float number in new[] { value.maxDistance, value.collisionSphereDistance })
                foreach (byte part in BitConverter.GetBytes(number)) hash = (hash ^ part) * 1099511628211UL;
            }
            return hash.ToString("x16", System.Globalization.CultureInfo.InvariantCulture);
        }

        private static JArray Cleanup2ClothColliderState(UnitEntityData actor, bool world)
        {
            return new JArray(actor.View.GetComponentsInChildren<Collider>(true).OrderBy(value => value.GetInstanceID()).Select(value => {
                var row = new JObject { ["instance"] = value.GetInstanceID(), ["type"] = value.GetType().FullName,
                    ["parentInstance"] = value.transform.parent == null ? 0 : value.transform.parent.GetInstanceID(),
                    ["enabled"] = value.enabled, ["activeSelf"] = value.gameObject.activeSelf,
                    ["trigger"] = value.isTrigger, ["layer"] = value.gameObject.layer,
                    ["localScale"] = SurveyVector(value.transform.localScale) };
                if (world) row["localToWorld"] = Cleanup2Matrix(value.transform.localToWorldMatrix);
                var capsule = value as CapsuleCollider;
                if (capsule != null) { row["radius"] = capsule.radius; row["height"] = capsule.height; row["center"] = SurveyVector(capsule.center); row["direction"] = capsule.direction; }
                var sphere = value as SphereCollider;
                if (sphere != null) { row["radius"] = sphere.radius; row["center"] = SurveyVector(sphere.center); }
                var box = value as BoxCollider;
                if (box != null) { row["center"] = SurveyVector(box.center); row["size"] = SurveyVector(box.size); }
                var mesh = value as MeshCollider;
                if (mesh != null) { row["meshInstance"] = mesh.sharedMesh == null ? 0 : mesh.sharedMesh.GetInstanceID(); row["convex"] = mesh.convex; }
                return row;
            }));
        }

        private static void Cleanup2ClothRequireIdentity(SkinnedMeshRenderer skin, Cloth cloth, Mesh mesh, Material[] materials,
            Transform[] bones, Transform root, bool enabled, ShadowCastingMode shadow,
            string configuration, UnitEntityData actor, string colliderConfiguration)
        {
            if (skin == null || cloth == null || skin.sharedMesh != mesh || skin.rootBone != root ||
                !Cleanup2ClothSameBones(skin.bones, bones) ||
                !skin.sharedMaterials.SequenceEqual(materials) ||
                skin.enabled != enabled || skin.shadowCastingMode != shadow ||
                Cleanup2ClothConfiguration(cloth).ToString() != configuration ||
                Cleanup2ClothColliderState(actor, false).ToString() != colliderConfiguration)
                throw new InvalidOperationException("Native Cloth/renderer/collider identity or unowned configuration changed; probe refuses further mutation.");
        }

        private JObject Cleanup2ClothSample(UnitEntityData actor, SkinnedMeshRenderer skin, Cloth cloth,
            Cleanup2ClothSeries series, string pose, int frame)
        {
            Mesh baked = null;
            series.Samples++;
            try
            {
                Vector3[] clothPoints = cloth.vertices;
                baked = new Mesh(); skin.BakeMesh(baked);
                Vector3[] bakePoints = baked.vertices;
                bool finite = clothPoints.Length > 0 && bakePoints.Length > 0 && clothPoints.All(Cleanup2Finite) && bakePoints.All(Cleanup2Finite);
                if (series.FirstCloth == null) { series.FirstCloth = clothPoints; series.FirstBake = bakePoints;
                    series.FirstBoneRotations = skin.bones.Select(bone => bone == null ? Quaternion.identity : bone.rotation).ToArray(); }
                bool counts = clothPoints.Length == series.FirstCloth.Length && bakePoints.Length == series.FirstBake.Length;
                var errors = new JArray();
                var passing = new HashSet<string>(StringComparer.Ordinal);
                if (finite && counts)
                {
                    series.ClothDisplacement = Mathf.Max(series.ClothDisplacement, Cleanup2ClothDisplacement(clothPoints, series.FirstCloth));
                    series.BakeDisplacement = Mathf.Max(series.BakeDisplacement, Cleanup2ClothDisplacement(bakePoints, series.FirstBake));
                    Transform[] bones = skin.bones;
                    for (int i = 0; i < bones.Length; i++) if (bones[i] != null)
                        series.BoneAngle = Mathf.Max(series.BoneAngle, Quaternion.Angle(series.FirstBoneRotations[i], bones[i].rotation));
                    string[] clothNames = { "cloth-as-world", "cloth-renderer-transform", "cloth-renderer-rotation", "cloth-rootbone-transform" };
                    for (int c = 0; c < clothNames.Length; c++)
                    for (int b = 0; b < 2; b++)
                    {
                        Vector3[] cWorld = clothPoints.Select(point => c == 0 ? point : c == 1 ? skin.transform.TransformPoint(point) :
                            c == 2 ? skin.transform.position + skin.transform.rotation * point : skin.rootBone.TransformPoint(point)).ToArray();
                        Vector3[] bWorld = bakePoints.Select(point => b == 0 ? skin.transform.TransformPoint(point) :
                            skin.transform.position + skin.transform.rotation * point).ToArray();
                        string name = clothNames[c] + "/" + (b == 0 ? "bake-renderer-transform" : "bake-renderer-rotation");
                        JObject metric = Cleanup2ClothPointCloudError(cWorld, bWorld);
                        metric["hypothesis"] = name; errors.Add(metric);
                        if ((bool)metric["finite"] && (float)metric["maximumNearestError"] <= .005f) passing.Add(name);
                    }
                    series.ValidSamples++;
                }
                if (series.ConsistentCoordinates == null) series.ConsistentCoordinates = passing;
                else series.ConsistentCoordinates.IntersectWith(passing);
                return new JObject { ["unit"] = actor.UniqueId, ["stage"] = series.Name, ["pose"] = pose, ["segmentFrame"] = frame,
                    ["clothEnabled"] = cloth.enabled, ["rendererEnabled"] = skin.enabled, ["shadowCastingMode"] = skin.shadowCastingMode.ToString(),
                    ["clothVertexCount"] = clothPoints.Length, ["bakeVertexCount"] = bakePoints.Length,
                    ["sourceMeshVertexCount"] = skin.sharedMesh.vertexCount, ["countsStable"] = counts, ["finite"] = finite,
                    ["localBoundsCenter"] = SurveyVector(skin.localBounds.center), ["localBoundsSize"] = SurveyVector(skin.localBounds.size),
                    ["clothCountMayBeWelded"] = true, ["motionSoFar"] = series.Summary(), ["coordinateHypotheses"] = errors,
                    ["currentNativeAction"] = actor.View.AnimationManager?.CurrentAction?.Action?.GetType().FullName,
                    ["geometryPolicy"] = "All native point arrays and correspondence searches remain transient in memory. Only counts, aggregate distances and finite/motion predicates are written." };
            }
            catch (Exception error)
            {
                if (series.ConsistentCoordinates == null) series.ConsistentCoordinates = new HashSet<string>();
                else series.ConsistentCoordinates.Clear();
                return new JObject { ["stage"] = series.Name, ["pose"] = pose, ["segmentFrame"] = frame,
                    ["finite"] = false, ["countsStable"] = false, ["observation"] = "UNKNOWN",
                    ["reason"] = error.GetType().FullName + ":" + error.Message };
            }
            finally { if (baked != null) UnityEngine.Object.Destroy(baked); }
        }

        private static float Cleanup2ClothDisplacement(Vector3[] current, Vector3[] first)
        {
            float maximum = 0f;
            for (int i = 0; i < current.Length; i++) maximum = Mathf.Max(maximum, Vector3.Distance(current[i], first[i]));
            return maximum;
        }

        private static JObject Cleanup2ClothPointCloudError(Vector3[] first, Vector3[] second)
        {
            double squared = 0; float maximum = 0f; int samples = 0;
            bool finite = first.All(Cleanup2Finite) && second.All(Cleanup2Finite);
            // Cloth may weld UV seam vertices. Do not assume equal lengths or
            // export an index map: test both directed nearest distances on a
            // deterministic bounded sample against each complete point cloud.
            if (finite)
                for (int direction = 0; direction < 2; direction++)
                {
                    Vector3[] source = direction == 0 ? first : second, target = direction == 0 ? second : first;
                    int count = Math.Min(64, source.Length);
                    for (int sample = 0; sample < count; sample++)
                    {
                        Vector3 point = source[sample * source.Length / count];
                        float nearest = float.MaxValue;
                        foreach (Vector3 other in target) nearest = Mathf.Min(nearest, (point - other).sqrMagnitude);
                        squared += nearest; maximum = Mathf.Max(maximum, Mathf.Sqrt(nearest)); samples++;
                    }
                }
            return new JObject { ["finite"] = finite && samples > 0, ["sampleCount"] = samples,
                ["maximumNearestError"] = maximum, ["rmsNearestError"] = samples == 0 ? 0 : Math.Sqrt(squared / samples),
                ["comparison"] = "Bidirectional deterministic <=64 samples per cloud against complete opposing cloud; aggregate coordinate evidence, not exported vertex correspondence or full topology equivalence." };
        }
    }
}
