using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using Harmony12;
using Kingmaker.Blueprints.Root;
using Kingmaker.View;
using Kingmaker.Visual.Animation;
using Kingmaker.Visual.Animation.Actions;
using Kingmaker.Visual.Animation.Kingmaker;
using Kingmaker.Visual.Animation.Kingmaker.Actions;
using Kingmaker.Visual.MaterialEffects;
using KingmakerGunslinger.Assets;
using KingmakerGunslinger.Bootstrap;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.Summoning
{
    // Exact Salamander production view and its closed research prototype only.
    // No native rig mutation, weapon remount, global animator/AI patch or
    // arbitrary asset/driver input. Production qualification remains separate.
    internal sealed class SalamanderHumanVisualAttachment : MonoBehaviour
    {
        private sealed class Skin
        {
            internal SkinnedMeshRenderer Renderer;
            internal Mesh Mesh;
            internal Transform[] Bones;
            internal Material[] Materials;
            internal SkinQuality Quality;
        }
        private UnitEntityView _view;
        private Skin[] _skins;
        private Mesh _mesh;
        private Texture2D _paint;
        private Material _material;
        private GameObject _tailRoot;
        private Animation _player;
        private AnimationClip _clip;
        private AnimationSet _nativeSet, _ownedSet;
        private AnimationActionBase[] _nativeActions, _expectedActions;
        private string _name;
        private bool _swapped, _released;
        internal SkinnedMeshRenderer Body { get; private set; }
        internal SalamanderTailAction TailAction { get; private set; }
        internal string Outcome { get; private set; }
        internal static Action PostSwapFaultForTest { get; set; }
        internal bool Live { get { return !_released && _swapped && Body != null &&
            ReferenceEquals(Body.sharedMesh, _mesh) && NativeActionsUnchanged; } }
        internal bool AuxiliaryGeometrySuppressed { get { return _swapped && _skins != null &&
            _skins.Where(skin => skin.Renderer != Body).All(skin => skin.Renderer != null &&
                skin.Renderer.sharedMesh == null && skin.Renderer.bones.SequenceEqual(skin.Bones)); } }
        internal bool NativeActionsUnchanged { get { return _nativeSet != null && _ownedSet != null &&
            _view != null && _view.AnimationManager != null &&
            ReferenceEquals(_view.AnimationManager.AnimationSet, _ownedSet) &&
            _nativeSet.Actions.SequenceEqual(_nativeActions) &&
            _ownedSet.Actions.SequenceEqual(_expectedActions) &&
            _ownedSet.Transitions.SequenceEqual(_nativeSet.Transitions) &&
            ReferenceEquals(_ownedSet.StartupAction, _nativeSet.StartupAction); } }

        internal static bool TryAttach(UnitEntityView view, ModContext context, out string outcome)
        {
            outcome = "human-tail:not-permitted";
            var unit = view == null ? null : view.EntityData;
            var primary = unit == null ? null : unit.Body.PrimaryHand.MaybeWeapon;
            if (unit == null || context == null || !SalamanderTailAnimationPolicy.PermitsBinding(
                context.FeatureModules.Active.ExpandedSummoning, unit.Blueprint.AssetGuid, unit.Blueprint.name,
                unit.Blueprint.Prefab.AssetId, primary == null ? null : primary.Blueprint.AssetGuid, true, true) ||
                view.GetComponent<SalamanderHumanVisualAttachment>() != null) return false;
            var owned = view.gameObject.AddComponent<SalamanderHumanVisualAttachment>();
            owned._view = view; owned._name = "KMG_SalamanderHuman_" + view.GetInstanceID();
            try { owned.Attach(context); outcome = owned.Outcome; return true; }
            catch (Exception error)
            {
                outcome = owned.Outcome = "human-tail:failed:" + error.GetType().Name + ":" + error.Message;
                try { owned.Release(); }
                finally { UnityEngine.Object.Destroy(owned); }
                return false;
            }
        }

        private void Attach(ModContext context)
        {
            var skins = _view.GetComponentsInChildren<SkinnedMeshRenderer>(true)
                .Where(value => value.sharedMesh != null).ToArray();
            Body = skins.SingleOrDefault(value => value.name == SalamanderHumanBindingPolicy.BodyName);
            if (Body == null || Body.rootBone == null || Body.rootBone.name != "Pelvis" ||
                Body.bones.Length != 1776 || Body.bones.Distinct().Count() != 177 ||
                Body.sharedMesh.bindposes.Length != 1776 ||
                skins.Any(skin => !skin.transform.IsChildOf(_view.transform) ||
                    skin.bones.Any(bone => bone == null || !bone.IsChildOf(_view.transform))))
                throw new InvalidDataException("Exact settled human anatomy palette unavailable.");
            var auxiliary = skins.Where(skin => skin != Body).ToArray();
            if (auxiliary.Length != 1 || auxiliary.Any(skin => !SalamanderHumanBindingPolicy.IsReviewedAuxiliary(
                skin.name, skin.sharedMesh.name, skin.bones.Length)))
                throw new InvalidDataException("Unreviewed human auxiliary renderer; no geometry suppression permitted.");
            Transform[] palette = Body.bones;
            Matrix4x4[] binds = Body.sharedMesh.bindposes;
            int[] slots;
            if (!SalamanderHumanBindingPolicy.TrySlots(palette.Select(b => b.name).ToArray(),
                palette.Select(b => b.GetInstanceID()).ToArray(),
                palette.Select(b => b.parent == null ? null : b.parent.name).ToArray(),
                binds.Select(b => Enumerable.Range(0, 16).Select(i => b[i]).ToArray()).ToArray(), out slots) ||
                slots.Any(i => !Finite(binds[i].determinant) || Math.Abs(binds[i].determinant) < .0000001f ||
                    Enumerable.Range(0, 16).Any(k => !Finite(binds[i].inverse[k]))))
                throw new InvalidDataException("Missing, singular or disagreeing selected human anatomical binds.");
            Material donor = Body.sharedMaterial;
            if (donor == null || !donor.HasProperty("_MainTex")) throw new InvalidDataException("No human paint material.");
            var manager = _view.AnimationManager;
            _nativeSet = manager == null ? null : manager.AnimationSet;
            bool exactHuman = ReferenceEquals(_nativeSet, BlueprintRoot.Instance.HumanAnimationSet);
            // A patched lookup can return native Slam when raw Tail is absent.
            // Keep the exact human carrier and reject any existing real Tail;
            // accept only null or the exact observed native-Slam fallback.
            var specials = exactHuman ? _nativeSet.Actions.OfType<UnitAnimationActionSpecialAttack>().ToArray() :
                new UnitAnimationActionSpecialAttack[0];
            var effectiveTail = exactHuman ? manager.GetAction(UnitAnimationSpecialAttackType.Tail) : null;
            var nativeSlam = specials.SingleOrDefault(action =>
                action.AttackType == UnitAnimationSpecialAttackType.Slam && action.name == "MyAnimationSet_Slam");
            string rejection = SalamanderHumanBindingPolicy.NativeSetRejection(exactHuman,
                specials.Any(action => action.AttackType == UnitAnimationSpecialAttackType.Tail),
                SalamanderHumanBindingPolicy.IsReviewedEffectiveTail(effectiveTail, nativeSlam));
            if (rejection != null) throw new InvalidDataException("Native human action guard: " + rejection);
            _nativeActions = _nativeSet.Actions.ToArray();

            string directory = Path.Combine(context.ModEntry.Path, "assets", "sprint17-serpents");
            byte[] bytes = File.ReadAllBytes(Path.Combine(directory, "salamander-human-mesh.json"));
            using (var sha = SHA256.Create())
                if (BitConverter.ToString(sha.ComputeHash(bytes)).Replace("-", "").ToLowerInvariant() !=
                    SalamanderHumanBindingPolicy.AssetHash) throw new InvalidDataException("Unreviewed original human/tail data.");
            string json = System.Text.Encoding.UTF8.GetString(bytes);
            JObject payload = JObject.Parse(json);
            string[] names;
            PteranodonAssetRuntime.AlbedoRequirement paint;
            _mesh = PteranodonAssetRuntime.BuildMesh(json, out names, out paint, SalamanderHumanBindingPolicy.Names);
            _mesh.name = _name + "_Mesh";
            if (!names.SequenceEqual(SalamanderHumanBindingPolicy.Names)) throw new InvalidDataException("Original driver ordering changed.");
            string reason;
            _paint = PteranodonAssetRuntime.LoadAlbedo(directory, paint, out reason);
            if (_paint == null) throw new InvalidDataException("Original paint: " + reason);
            _paint.name = _name + "_Paint";
            _material = new Material(donor) { name = _name + "_Material" };
            ExpandedSummoningPteranodonViewPatch.DressMaterial(_material, _paint);

            Vector3[] rest = Points(payload["originalTailRest"]);
            JObject animation = (JObject)payload["originalTailSlap"];
            if (rest.Length != 11 || (float)animation["duration"] != SalamanderTailAnimationPolicy.Duration ||
                (float)animation["actTime"] != SalamanderTailAnimationPolicy.ActTime)
                throw new InvalidDataException("Original tail contract changed.");
            _tailRoot = new GameObject(_name + "_TailRoot");
            _tailRoot.transform.SetParent(Body.transform, false);
            Transform[] tail = SalamanderTailAnimationPolicy.TailNames.Select((name, index) => {
                var bone = new GameObject(name).transform;
                bone.SetParent(_tailRoot.transform, false); bone.localPosition = rest[index];
                return bone;
            }).ToArray();
            _player = _tailRoot.AddComponent<Animation>();
            _player.playAutomatically = false;
            _player.cullingType = AnimationCullingType.AlwaysAnimate;
            _clip = BuildClip(animation, rest);
            _player.AddClip(_clip, _clip.name);
            TailAction = SalamanderTailAction.Create(_view, _player, _clip, tail,
                context.FeatureModules.Active.ExpandedSummoning);
            _expectedActions = SalamanderHumanBindingPolicy.AppendOneTail(_nativeActions, (AnimationActionBase)TailAction);
            _ownedSet = UnityEngine.Object.Instantiate(_nativeSet);
            _ownedSet.name = _name + "_AnimationSet";
            var actions = typeof(AnimationSet).GetField("m_Actions", BindingFlags.Instance | BindingFlags.NonPublic);
            if (actions == null) throw new InvalidDataException("Native action-list field unavailable.");
            actions.SetValue(_ownedSet, new List<AnimationActionBase>(_expectedActions));
            if (ReferenceEquals(_ownedSet.Actions, _nativeSet.Actions))
                throw new InvalidOperationException("Owned action list aliases native data.");

            Matrix4x4[] logicalBinds = slots.Select(i => binds[i]).Concat(rest.Take(10).Select(point =>
                Matrix4x4.TRS(point, Quaternion.identity, Vector3.one).inverse)).ToArray();
            Transform[] logicalBones = slots.Select(i => palette[i]).Concat(tail).ToArray();
            _mesh.bindposes = names.Select(name => logicalBinds[SalamanderHumanBindingPolicy.BindingIndex(name)]).ToArray();
            _skins = skins.Select(skin => new Skin { Renderer = skin, Mesh = skin.sharedMesh, Bones = skin.bones,
                Materials = skin.sharedMaterials, Quality = skin.quality }).ToArray();
            _swapped = true;
            Body.sharedMesh = _mesh;
            Body.bones = names.Select(name => logicalBones[SalamanderHumanBindingPolicy.BindingIndex(name)]).ToArray();
            Body.quality = SkinQuality.Bone4; Body.sharedMaterials = new[] { _material };
            // An allocated zero-vertex skinned mesh can request a zero-sized
            // native graphics buffer. Null geometry is the no-mesh state;
            // retain native bones, root and renderer flags, and restore the
            // exact borrowed mesh on rollback/teardown. No visibility forcing.
            foreach (var skin in auxiliary) skin.sharedMesh = null;
            // Human equipment meshes, snaps, native Animator and all native
            // bone transforms remain wholly native. No spear mesh transplant.
            manager.AnimationSet = _ownedSet;
            if (!NativeActionsUnchanged || !ReferenceEquals(manager.GetAction(UnitAnimationSpecialAttackType.Tail), TailAction))
                throw new InvalidOperationException("Native human action identity changed.");
            string adopted = ExpandedSummoningPteranodonViewPatch.AdoptByMaterialController(_view, Body, _material);
            var driven = ExpandedSummoningPteranodonViewPatch.ControllerMaterials(
                _view.GetComponentInChildren<StandardMaterialController>(true));
            if (driven == null || !driven.Contains(Body.sharedMaterial) || !Owned(Body.sharedMaterial))
                throw new InvalidOperationException("Original human-body material was not adopted.");
            if (PostSwapFaultForTest != null) PostSwapFaultForTest();
            Outcome = "human-tail:attached;nativeDrivers=26;originalDrivers=10;nativeActions=24;ownedTail=1;" + adopted;
        }

        private AnimationClip BuildClip(JObject animation, Vector3[] rest)
        {
            var frames = animation["frames"].OfType<JObject>().ToArray();
            if (frames.Length != 11) throw new InvalidDataException("Original frame count.");
            var clip = new AnimationClip { name = _name + "_TailSlap", legacy = true,
                frameRate = 60, wrapMode = WrapMode.ClampForever };
            try
            {
                for (int bone = 0; bone < 10; bone++)
                {
                    var channels = Enumerable.Range(0, 7).Select(_ => new List<Keyframe>()).ToArray();
                    Quaternion previous = Quaternion.identity;
                    foreach (var frame in frames)
                    {
                        float time = (float)frame["time"];
                        Vector3[] points = Points(frame["points"]);
                        Quaternion rotation = Quaternion.FromToRotation(rest[bone + 1] - rest[bone], points[bone + 1] - points[bone]);
                        if (Quaternion.Dot(previous, rotation) < 0)
                            rotation = new Quaternion(-rotation.x, -rotation.y, -rotation.z, -rotation.w);
                        previous = rotation;
                        float[] values = { points[bone].x, points[bone].y, points[bone].z,
                            rotation.x, rotation.y, rotation.z, rotation.w };
                        for (int axis = 0; axis < 7; axis++) channels[axis].Add(new Keyframe(time, values[axis]));
                    }
                    string[] properties = { "localPosition.x", "localPosition.y", "localPosition.z",
                        "localRotation.x", "localRotation.y", "localRotation.z", "localRotation.w" };
                    for (int axis = 0; axis < 7; axis++)
                    {
                        Keyframe[] keys = channels[axis].ToArray();
                        // Explicit linear interpolation; no editor/tangent or
                        // target-dependent runtime deformation.
                        for (int i = 0; i < keys.Length; i++)
                        {
                            if (i > 0) keys[i].inTangent = (keys[i].value - keys[i-1].value) / (keys[i].time - keys[i-1].time);
                            if (i + 1 < keys.Length) keys[i].outTangent = (keys[i+1].value - keys[i].value) / (keys[i+1].time - keys[i].time);
                        }
                        clip.SetCurve(SalamanderTailAnimationPolicy.TailNames[bone], typeof(Transform), properties[axis],
                            new AnimationCurve(keys));
                    }
                }
                clip.EnsureQuaternionContinuity();
                return clip;
            }
            catch { UnityEngine.Object.DestroyImmediate(clip); throw; }
        }

        private static Vector3[] Points(JToken value)
        {
            var rows = value as JArray;
            if (rows == null || rows.Count != 11) throw new InvalidDataException("Original tail point count.");
            return rows.Select(row => {
                float[] p = row.Values<float>().ToArray();
                if (p.Length != 3 || p.Any(v => !Finite(v))) throw new InvalidDataException("Original finite tail point.");
                return new Vector3(p[0], p[1], p[2]);
            }).ToArray();
        }
        private static bool Finite(float value) { return SalamanderTailAnimationPolicy.Finite(value); }
        private bool Owned(Material material)
        { return material != null && material.name.StartsWith(_name + "_Material", StringComparison.Ordinal); }

        internal UnityEngine.Object[] BorrowedResources()
        { return _nativeSet == null ? new UnityEngine.Object[0] : new UnityEngine.Object[] { _nativeSet }
            .Concat(_nativeActions).Concat(_nativeActions.SelectMany(action => action.Clips ?? new AnimationClip[0])
                .Where(clip => clip != null)).Distinct().ToArray(); }

        internal UnityEngine.Object[] CaptureOwnedResources()
        {
            var result = new HashSet<UnityEngine.Object>(new UnityEngine.Object[] {
                _mesh, _paint, _material, _tailRoot, _player, _clip, TailAction, _ownedSet }.Where(value => value != null));
            if (_tailRoot != null) foreach (Transform t in _tailRoot.GetComponentsInChildren<Transform>(true))
            { result.Add(t); result.Add(t.gameObject); }
            if (_view != null)
            {
                foreach (var renderer in _view.GetComponentsInChildren<Renderer>(true))
                    foreach (Material material in renderer.sharedMaterials.Where(Owned)) result.Add(material);
                var driven = ExpandedSummoningPteranodonViewPatch.ControllerMaterials(
                    _view.GetComponentInChildren<StandardMaterialController>(true));
                if (driven != null) foreach (var material in driven.Where(Owned)) result.Add(material);
            }
            return result.ToArray();
        }

        internal void Release()
        {
            if (_released) return;
            var resources = CaptureOwnedResources();
            Exception failure = null;
            try { if (TailAction != null) TailAction.StopOwnedPlayback(); }
            catch (Exception error) { failure = error; }
            var manager = _view == null ? null : _view.AnimationManager;
            try { if (manager != null && ReferenceEquals(manager.AnimationSet, _ownedSet)) manager.AnimationSet = _nativeSet; }
            catch (Exception error) { failure = error; }
            if (_swapped)
            {
                foreach (Skin row in _skins.Where(row => row.Renderer != null))
                {
                    row.Renderer.sharedMesh = row.Mesh; row.Renderer.bones = row.Bones;
                    row.Renderer.sharedMaterials = row.Materials; row.Renderer.quality = row.Quality;
                }
                _swapped = false;
                try { ExpandedSummoningPteranodonViewPatch.ReinitializeMaterialController(_view); }
                catch (Exception error) { failure = error; }
            }
            if (manager != null && _ownedSet != null && ReferenceEquals(manager.AnimationSet, _ownedSet))
                throw new InvalidOperationException("Live manager still references owned action set; cleanup remains retryable.", failure);
            // Never destroy native body/equipment/set/action/clip references.
            foreach (var material in resources.OfType<Material>()) if (material != null) UnityEngine.Object.DestroyImmediate(material);
            foreach (var value in new UnityEngine.Object[] { _ownedSet, TailAction, _clip, _tailRoot, _mesh, _paint })
                if (value != null) UnityEngine.Object.DestroyImmediate(value);
            _released = true;
            if (failure != null) throw new InvalidOperationException("Human/tail cleanup encountered a native error.", failure);
        }

        private void OnDestroy() { try { Release(); } catch (Exception) { /* Let native teardown finish. */ } }
    }

    [HarmonyPatch(typeof(UnitEntityView), "OnDestroy")]
    internal static class SalamanderHumanVisualTeardown
    {
        private static void Prefix(UnitEntityView __instance)
        {
            var owned = __instance == null ? null : __instance.GetComponent<SalamanderHumanVisualAttachment>();
            if (owned != null) try { owned.Release(); } catch (Exception) { /* Native teardown must complete. */ }
        }
    }
}
