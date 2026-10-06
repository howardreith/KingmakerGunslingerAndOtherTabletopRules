using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Harmony12;
using Kingmaker.View;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.Visual.MaterialEffects;
using KingmakerGunslinger.Assets;
using KingmakerGunslinger.Bootstrap;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.Summoning
{
    /// <summary>Instance-owned Sprint 17 body swap on the two audited skins.
    /// No automatic production attachment yet: the guarded donor acceptance
    /// slice must prove movement, ground, contacts, fades and cleanup first.
    /// Existing Purple Worm, Water Elemental and Salamander are unchanged.
    /// The optional spear seam is closed two-hand research only.</summary>
    [DefaultExecutionOrder(10010)]
    internal sealed class SerpentineVisualAttachment : MonoBehaviour
    {
        private sealed class SkinState
        {
            internal SkinnedMeshRenderer Renderer;
            internal Mesh Mesh;
            internal Transform[] Bones;
            internal Material[] Materials;
            internal SkinQuality Quality;
        }
        private sealed class StaticState
        {
            internal MeshFilter Filter;
            internal Mesh Mesh;
            internal MeshRenderer Renderer;
            internal Material[] Materials;
            internal Vector3 Position, Scale;
            internal Quaternion Rotation;
        }

        private UnitEntityView _view;
        private SkinState[] _skins;
        private StaticState[] _statics;
        private Mesh _body, _empty;
        private Texture2D _albedo;
        private Material _material;
        private Material _spearMaterial;
        private Transform _spearRightPalm, _spearLeftPalm;
        private Quaternion _nativeSpearRotation;
        private SerpentineNativeSpearAnimation _spearAnimation;
        private string _ownedName;
        private bool _swapped, _released;
        internal string Outcome { get; private set; }
        internal SkinnedMeshRenderer Body { get; private set; }
        internal string[] DriverNames { get; private set; }
        internal MeshFilter SpearFilter { get; private set; }
        internal Mesh NativeSpearMesh { get; private set; }
        internal string SpearMountStatus { get; private set; }
        internal int SpearMountFrame { get; private set; }
        internal bool NativeSpearAnimationBound { get { return _spearAnimation != null && _spearAnimation.BoundAndNativeUnchanged; } }
        internal string NativeSpearAnimationObservation { get { return _spearAnimation == null ? null : _spearAnimation.Observation; } }
        internal UnityEngine.Object[] CaptureBorrowedAnimationResources()
        { return _spearAnimation == null ? new UnityEngine.Object[0] : _spearAnimation.BorrowedResources(); }
        internal static Action PostSwapFaultForTest { get; set; }

        internal UnityEngine.Object[] CaptureOwnedResources()
        {
            var owned = new HashSet<UnityEngine.Object>();
            foreach (UnityEngine.Object value in new UnityEngine.Object[] { _body, _empty, _albedo, _material, _spearMaterial })
                if (value != null) owned.Add(value);
            if (_spearAnimation != null && _spearAnimation.OwnedSet != null) owned.Add(_spearAnimation.OwnedSet);
            if (Body != null)
                foreach (Material material in Body.sharedMaterials)
                    if (IsOwned(material)) owned.Add(material);
            if (_statics != null)
                foreach (StaticState row in _statics.Where(value => value.Renderer != null))
                    foreach (Material material in row.Renderer.sharedMaterials)
                        if (IsOwned(material)) owned.Add(material);
            var controller = _view == null ? null : _view.GetComponentInChildren<StandardMaterialController>(true);
            var driven = ExpandedSummoningPteranodonViewPatch.ControllerMaterials(controller);
            if (driven != null)
                foreach (Material material in driven)
                    if (IsOwned(material)) owned.Add(material);
            return owned.ToArray();
        }

        internal static bool TryAttach(UnitEntityView view, string key, ModContext context,
            out string outcome, bool nativeSpearResearch = false)
        {
            outcome = "donor-visual:not-permitted";
            if (view == null || view.EntityData == null || view.EntityData.Blueprint == null || context == null ||
                !context.FeatureModules.Active.ExpandedSummoning ||
                !SerpentineVisualPolicy.Keys.Contains(key, StringComparer.Ordinal)) return false;
            if (view.GetComponent<SerpentineVisualAttachment>() != null)
            { outcome = "donor-visual:already-attempted"; return false; }
            var owned = view.gameObject.AddComponent<SerpentineVisualAttachment>();
            owned._view = view;
            owned._ownedName = "KMG_" + key + "_Original_" + view.GetInstanceID();
            try
            {
                outcome = owned.Attach(key, context, nativeSpearResearch);
                owned.Outcome = outcome;
                return true;
            }
            catch (Exception error)
            {
                outcome = "donor-visual:attach-failed:" + error.GetType().Name + ":" + error.Message;
                owned.Outcome = outcome;
                try { owned.Release(); }
                finally { UnityEngine.Object.Destroy(owned); }
                return false;
            }
        }

        private string Attach(string key, ModContext context, bool nativeSpearResearch)
        {
            SkinnedMeshRenderer[] skins = _view.GetComponentsInChildren<SkinnedMeshRenderer>(true)
                .Where(value => value != null && value.sharedMesh != null).ToArray();
            MeshFilter[] statics = _view.GetComponentsInChildren<MeshFilter>(true)
                .Where(value => value != null && value.sharedMesh != null).ToArray();
            Body = skins.SingleOrDefault(value => value.name == SerpentineVisualPolicy.BodyRenderer(key));
            SkinnedMeshRenderer auxiliary = skins.SingleOrDefault(value =>
                value.name == SerpentineVisualPolicy.AuxiliaryRenderer(key));
            if (Body == null || auxiliary == null || !SerpentineVisualPolicy.PermitsDonor(key,
                _view.EntityData.Blueprint.Prefab == null ? null : _view.EntityData.Blueprint.Prefab.AssetId,
                skins.Select(value => value.name), Body.bones.Length, auxiliary.bones.Length,
                Body.rootBone == null ? null : Body.rootBone.name, statics.Select(value => value.name)))
                throw new InvalidDataException("unreviewed renderer/bone/static-weapon set");
            foreach (SkinnedMeshRenderer skin in skins)
                if (!skin.transform.IsChildOf(_view.transform) || skin.rootBone == null ||
                    !skin.rootBone.IsChildOf(_view.transform) ||
                    skin.sharedMesh.bindposes.Length != skin.bones.Length ||
                    skin.bones.Any(bone => bone == null || !bone.IsChildOf(_view.transform)))
                    throw new InvalidDataException("donor binding is not wholly inside this view");
            Material donorMaterial = Body.sharedMaterial;
            if (donorMaterial == null || !donorMaterial.HasProperty("_MainTex"))
                throw new InvalidDataException("donor lacks its standard painted material");

            // The closed key is the only path input. No request can supply an
            // asset path, another rig, an arbitrary bone or a native export.
            string directory = Path.Combine(context.ModEntry.Path, "assets", "sprint17-serpents");
            string json = File.ReadAllText(Path.Combine(directory, key + "-mesh.json"));
            JObject payload = JObject.Parse(json);
            if (!SerpentineVisualPolicy.PermitsBones(key, payload["bones"] == null ? null :
                payload["bones"].Values<string>()) || (int?)payload["visibleLegs"] != 0 ||
                (bool?)payload["jawSeparated"] != true ||
                !SerpentineVisualPolicy.PermitsOriginalWinding(key, (string)payload["triangleWinding"]))
                throw new InvalidDataException("original anatomy/driver contract");
            string[] names;
            PteranodonAssetRuntime.AlbedoRequirement requirement;
            _body = PteranodonAssetRuntime.BuildMesh(json, out names, out requirement,
                SerpentineVisualPolicy.Bones(key));
            _body.name = _ownedName;
            string reason;
            _albedo = PteranodonAssetRuntime.LoadAlbedo(directory, requirement, out reason);
            if (_albedo == null) throw new InvalidDataException("albedo:" + reason);
            _albedo.name = _ownedName + "_Albedo";
            Transform[] nativeBones = Body.bones;
            Matrix4x4[] nativeBind = Body.sharedMesh.bindposes;
            int[] slots;
            if (!SerpentineVisualPolicy.TryResolveDriverSlots(key, names,
                nativeBones.Select(bone => bone.name).ToArray(), out slots))
                throw new InvalidDataException("original driver mapping is not the closed measured set");
            Transform[] bones = slots.Select(slot => slot < 0 ? Body.transform : nativeBones[slot]).ToArray();
            // The original support frame has a half-turn around original +Z:
            // native torso and renderer forward axes are opposite. This is a
            // static authored bindpose, not a copied native transform or a
            // per-frame ground/animation override. No Transform is created.
            Matrix4x4 supportBind = Matrix4x4.Scale(new Vector3(-1, -1, 1));
            Matrix4x4[] bindposes = slots.Select(slot => slot < 0 ? supportBind : nativeBind[slot]).ToArray();
            DriverNames = names;
            _body.bindposes = bindposes;
            _material = new Material(donorMaterial) { name = _ownedName };
            string dressing = ExpandedSummoningPteranodonViewPatch.DressMaterial(_material, _albedo);
            _empty = new Mesh { name = _ownedName + "_SuppressedGeometry" };
            _skins = skins.Select(skin => new SkinState { Renderer = skin, Mesh = skin.sharedMesh,
                Bones = skin.bones, Materials = skin.sharedMaterials, Quality = skin.quality }).ToArray();
            _statics = statics.Select(filter => {
                var renderer = filter.GetComponent<MeshRenderer>();
                return new StaticState { Filter = filter, Mesh = filter.sharedMesh, Renderer = renderer,
                    Materials = renderer == null ? null : renderer.sharedMaterials,
                    Position = filter.transform.localPosition, Rotation = filter.transform.localRotation,
                    Scale = filter.transform.localScale };
            }).ToArray();

            if (nativeSpearResearch)
            {
                // Check the original 39-bone native frame before its original
                // body replacement. Failure restores the exact instance set.
                _spearAnimation = new SerpentineNativeSpearAnimation(_view);
                _spearAnimation.Bind(key, Body, _ownedName);
            }
            _swapped = true;
            Body.sharedMesh = _body;
            Body.bones = bones;
            // The hybrid's blended support adds a third influence to some
            // original vertices. Preserve all three on this renderer only;
            // restore its exact native quality on rollback/destruction.
            if (key == "salamander") Body.quality = SkinQuality.Bone4;
            Body.sharedMaterials = new[] { _material };
            // Keep native renderer/component identity, activation and enabled
            // state. Native fader/occlusion/appearance locks can continue to
            // drive them without ever re-exposing the stones, armor or club.
            auxiliary.sharedMesh = _empty;
            auxiliary.bones = new Transform[0];
            foreach (StaticState row in _statics) row.Filter.sharedMesh = _empty;
            if (nativeSpearResearch) AttachNativeSpearResearch(key);
            string adoption = ExpandedSummoningPteranodonViewPatch.AdoptByMaterialController(_view, Body, _material);
            StandardMaterialController controller = _view.GetComponentInChildren<StandardMaterialController>(true);
            IList<Material> driven = ExpandedSummoningPteranodonViewPatch.ControllerMaterials(controller);
            if (driven == null || Body.sharedMaterial == null || !driven.Contains(Body.sharedMaterial) ||
                !Body.sharedMaterial.name.StartsWith(_ownedName, StringComparison.Ordinal))
                throw new InvalidOperationException("native material controller did not adopt the original body");
            Action fault = PostSwapFaultForTest;
            if (fault != null) fault();
            return "visual:attached;key=" + key + ";vertices=" + _body.vertexCount +
                ";bones=" + names.Length + ";suppressedSkins=1;suppressedStatic=" + statics.Length +
                ";nativeSpearResearch=" + nativeSpearResearch + ";" + dressing + ";" + adoption;
        }

        private void AttachNativeSpearResearch(string key)
        {
            var weapon = _view.EntityData.Body.PrimaryHand.MaybeWeapon;
            var blueprint = weapon == null ? null : weapon.Blueprint;
            GameObject model = blueprint == null || blueprint.VisualParameters == null ? null : blueprint.VisualParameters.Model;
            MeshFilter source = model == null ? null : model.GetComponentsInChildren<MeshFilter>(true).SingleOrDefault();
            MeshRenderer sourceRenderer = source == null ? null : source.GetComponent<MeshRenderer>();
            StaticState slot = _statics.SingleOrDefault(value => value.Filter.name == "lizardman_club");
            var snap = slot == null ? null : slot.Filter.GetComponentInParent<Kingmaker.Assets.Visual.WeaponSnap>();
            if (source == null || source.sharedMesh == null || sourceRenderer == null ||
                sourceRenderer.sharedMaterials.Length != 1 || sourceRenderer.sharedMaterial == null ||
                slot == null || slot.Renderer == null || snap == null || snap.SnapTo == null ||
                !SerpentineVisualPolicy.PermitsNativeSpearResearch(key, _view.EntityData.Blueprint.Prefab.AssetId,
                    blueprint.AssetGuid, blueprint.Category.ToString(), model.name, source.sharedMesh.name,
                    snap.name, snap.SnapTo.name))
                throw new InvalidDataException("unreviewed native spear/two-hand/palm seam");
            // Reuse the existing native weapon renderer and R_Palm snap. Only
            // its instance's mesh/material/local mounting change. No prefab,
            // bone, animation, visibility state, mesh data or texture is edited.
            NativeSpearMesh = source.sharedMesh;
            _spearMaterial = new Material(sourceRenderer.sharedMaterial) { name = _ownedName + "_Spear" };
            slot.Filter.sharedMesh = NativeSpearMesh;
            slot.Renderer.sharedMaterials = new[] { _spearMaterial };
            slot.Filter.transform.localPosition = Vector3.zero;
            slot.Filter.transform.localRotation = model.transform.localRotation;
            slot.Filter.transform.localScale = model.transform.localScale;
            SpearFilter = slot.Filter;
            _nativeSpearRotation = model.transform.localRotation;
            _spearRightPalm = Body.bones.Single(value => value.name == "R_Palm");
            _spearLeftPalm = Body.bones.Single(value => value.name == "L_Palm");
            SynchronizeSpearMount();
        }

        /// <summary>Orient only this instance's existing weapon renderer
        /// between its two native palms. No bone/clip/controller, scale,
        /// actor collision, reach, target position or rule input is changed.
        /// Called after native WeaponSnap, and at the actual rule boundary
        /// so same-frame contact does not measure the previous render pose.</summary>
        internal void SynchronizeSpearMount()
        {
            if (_released || !_swapped || SpearFilter == null || NativeSpearMesh == null) return;
            Transform weapon = SpearFilter.transform;
            var primary = _view == null || _view.EntityData == null ? null :
                _view.EntityData.Body.PrimaryHand.MaybeWeapon;
            SpearMountFrame = Time.frameCount;
            SpearMountStatus = "native-right-palm-fallback";
            weapon.localPosition = Vector3.zero;
            weapon.localRotation = _nativeSpearRotation;
            if (primary == null || primary.Blueprint.AssetGuid != SerpentineVisualPolicy.ProjectSpear ||
                _view.EntityData.Blueprint.Prefab.AssetId != SerpentineVisualPolicy.TwoHandPrefab ||
                SpearFilter.sharedMesh != NativeSpearMesh || _spearRightPalm == null || _spearLeftPalm == null ||
                !_spearRightPalm.IsChildOf(_view.transform) || !_spearLeftPalm.IsChildOf(_view.transform)) return;
            Bounds bounds = NativeSpearMesh.bounds;
            Vector3 scale = weapon.lossyScale;
            // Reject a changed/nonuniform mount instead of stretching a spear
            // to fit its hands. The captured native mesh is not readable.
            if (!Finite(scale) || scale.x <= 0 || scale.y <= 0 || scale.z <= 0 ||
                Mathf.Abs(scale.x - scale.y) > .0001f || Mathf.Abs(scale.z - scale.y) > .0001f ||
                bounds.extents.y <= bounds.extents.x || bounds.extents.y <= bounds.extents.z) return;
            Vector3 right = _spearRightPalm.position, left = _spearLeftPalm.position;
            if (!Finite(right) || !Finite(left)) return;
            float spacing = Vector3.Distance(right, left), rear;
            float length = bounds.size.y * scale.y;
            if (!SerpentineVisualPolicy.TrySpearRearGrip(length, spacing, out rear)) return;
            Vector3 axis = (left - right) / spacing;
            Quaternion native = weapon.rotation;
            weapon.rotation = Quaternion.FromToRotation(native * Vector3.up, axis) * native;
            Vector3 localGrip = bounds.center + Vector3.up * (-bounds.extents.y + rear / scale.y);
            weapon.position += right - weapon.TransformPoint(localGrip);
            SpearMountStatus = "two-native-palms:instance-weapon-only";
        }

        private static bool Finite(Vector3 value)
        {
            return !float.IsNaN(value.x) && !float.IsInfinity(value.x) &&
                !float.IsNaN(value.y) && !float.IsInfinity(value.y) &&
                !float.IsNaN(value.z) && !float.IsInfinity(value.z);
        }

        private void LateUpdate()
        {
            try { SynchronizeSpearMount(); }
            catch (Exception error)
            {
                SpearMountStatus = "failed:" + error.GetType().Name;
                enabled = false;
                Debug.LogException(error);
            }
        }

        /// <summary>Called before native view destruction, or after a failed
        /// swap. Never destroys native materials/meshes or another view's copy.</summary>
        internal void Release()
        {
            if (_released) return;
            Exception animationFailure = null;
            try { if (_spearAnimation != null) _spearAnimation.Release(); }
            catch (Exception error) { animationFailure = error; }
            var materials = new HashSet<Material>();
            if (_material != null) materials.Add(_material);
            if (_spearMaterial != null) materials.Add(_spearMaterial);
            if (_swapped)
            {
                foreach (SkinState row in _skins)
                    if (row.Renderer != null)
                        foreach (Material material in row.Renderer.sharedMaterials)
                            if (IsOwned(material)) materials.Add(material);
                foreach (StaticState row in _statics.Where(value => value.Renderer != null))
                    foreach (Material material in row.Renderer.sharedMaterials)
                        if (IsOwned(material)) materials.Add(material);
                StandardMaterialController controller = _view == null ? null :
                    _view.GetComponentInChildren<StandardMaterialController>(true);
                IList<Material> driven = ExpandedSummoningPteranodonViewPatch.ControllerMaterials(controller);
                if (driven != null)
                    foreach (Material material in driven)
                        if (IsOwned(material)) materials.Add(material);
                foreach (SkinState row in _skins)
                    if (row.Renderer != null)
                    { row.Renderer.sharedMesh = row.Mesh; row.Renderer.bones = row.Bones;
                      row.Renderer.sharedMaterials = row.Materials; row.Renderer.quality = row.Quality; }
                foreach (StaticState row in _statics)
                    if (row.Filter != null)
                    {
                        row.Filter.sharedMesh = row.Mesh;
                        row.Filter.transform.localPosition = row.Position;
                        row.Filter.transform.localRotation = row.Rotation;
                        row.Filter.transform.localScale = row.Scale;
                        if (row.Renderer != null) row.Renderer.sharedMaterials = row.Materials;
                    }
                _swapped = false;
                // Even a controller exception must not strand project-owned
                // clones. Native references are already back on every renderer.
                try { ExpandedSummoningPteranodonViewPatch.ReinitializeMaterialController(_view); }
                catch (Exception)
                {
                    DestroyOwnedResources(materials);
                    throw;
                }
            }
            DestroyOwnedResources(materials);
            if (animationFailure != null) throw new InvalidOperationException("Native spear action restoration failed.", animationFailure);
        }

        private void DestroyOwnedResources(IEnumerable<Material> materials)
        {
            foreach (Material material in materials)
                if (material != null) UnityEngine.Object.DestroyImmediate(material);
            if (_body != null) UnityEngine.Object.DestroyImmediate(_body);
            if (_empty != null) UnityEngine.Object.DestroyImmediate(_empty);
            if (_albedo != null) UnityEngine.Object.DestroyImmediate(_albedo);
            _body = null; _empty = null; _material = null; _spearMaterial = null; _albedo = null;
            // A failed native animation restoration keeps a retryable owner
            // until native view teardown; never silently strand its container.
            _released = _spearAnimation == null || _spearAnimation.OwnedSet == null;
        }

        private bool IsOwned(Material material)
        { return material != null && !string.IsNullOrEmpty(_ownedName) &&
            (material.name == _ownedName || material.name.StartsWith(_ownedName + " (", StringComparison.Ordinal) ||
             material.name == _ownedName + "_Spear" || material.name.StartsWith(_ownedName + "_Spear (", StringComparison.Ordinal)); }

        private void OnDestroy()
        { try { Release(); } catch (Exception) { /* Native teardown must finish. */ } }
    }

    [HarmonyPatch(typeof(RuleAttackWithWeapon), "OnTrigger", new[] { typeof(RulebookEventContext) })]
    internal static class SerpentineSpearMountContactPatch
    {
        private static void Prefix(RuleAttackWithWeapon __instance)
        {
            if (__instance == null || __instance.Weapon == null ||
                __instance.Weapon.Blueprint.AssetGuid != SerpentineVisualPolicy.ProjectSpear ||
                __instance.Initiator == null || __instance.Initiator.View == null) return;
            var owned = __instance.Initiator.View.GetComponent<SerpentineVisualAttachment>();
            if (owned == null || !owned.enabled) return;
            try { owned.SynchronizeSpearMount(); }
            catch (Exception error) { owned.enabled = false; Debug.LogException(error); }
            // Never suppress or replay the authoritative attack.
        }
    }

    [HarmonyPatch(typeof(UnitEntityView), "OnDestroy")]
    internal static class SerpentineVisualTeardownPatch
    {
        private static void Prefix(UnitEntityView __instance)
        {
            var owned = __instance == null ? null : __instance.GetComponent<SerpentineVisualAttachment>();
            if (owned == null) return;
            try { owned.Release(); } catch (Exception) { /* Never interrupt native destruction. */ }
        }
    }
}
