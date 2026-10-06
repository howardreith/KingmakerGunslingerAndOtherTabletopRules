using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Harmony12;
using Kingmaker.View;
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
    /// The hybrid body deliberately contains no spear implementation.</summary>
    internal sealed class SerpentineVisualAttachment : MonoBehaviour
    {
        private sealed class SkinState
        {
            internal SkinnedMeshRenderer Renderer;
            internal Mesh Mesh;
            internal Transform[] Bones;
            internal Material[] Materials;
        }
        private sealed class StaticState
        {
            internal MeshFilter Filter;
            internal Mesh Mesh;
        }

        private UnitEntityView _view;
        private SkinState[] _skins;
        private StaticState[] _statics;
        private Mesh _body, _empty;
        private Texture2D _albedo;
        private Material _material;
        private string _ownedName;
        private bool _swapped, _released;
        internal string Outcome { get; private set; }
        internal SkinnedMeshRenderer Body { get; private set; }
        internal static Action PostSwapFaultForTest { get; set; }

        internal static bool TryAttach(UnitEntityView view, string key, ModContext context,
            out string outcome)
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
                outcome = owned.Attach(key, context);
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

        private string Attach(string key, ModContext context)
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
                (bool?)payload["jawSeparated"] != true)
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
            Transform[] bones;
            Matrix4x4[] bindposes;
            if (!PteranodonAssetRuntime.TryResolveDonorBinding(Body, names, out bones, out bindposes, out reason))
                throw new InvalidDataException(reason);
            _body.bindposes = bindposes;
            _material = new Material(donorMaterial) { name = _ownedName };
            string dressing = ExpandedSummoningPteranodonViewPatch.DressMaterial(_material, _albedo);
            _empty = new Mesh { name = _ownedName + "_SuppressedGeometry" };
            _skins = skins.Select(skin => new SkinState { Renderer = skin, Mesh = skin.sharedMesh,
                Bones = skin.bones, Materials = skin.sharedMaterials }).ToArray();
            _statics = statics.Select(filter => new StaticState { Filter = filter, Mesh = filter.sharedMesh }).ToArray();

            _swapped = true;
            Body.sharedMesh = _body;
            Body.bones = bones;
            Body.sharedMaterials = new[] { _material };
            // Keep native renderer/component identity, activation and enabled
            // state. Native fader/occlusion/appearance locks can continue to
            // drive them without ever re-exposing the stones, armor or club.
            auxiliary.sharedMesh = _empty;
            auxiliary.bones = new Transform[0];
            foreach (StaticState row in _statics) row.Filter.sharedMesh = _empty;
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
                ";bodyOnly=true;" + dressing + ";" + adoption;
        }

        /// <summary>Called before native view destruction, or after a failed
        /// swap. Never destroys native materials/meshes or another view's copy.</summary>
        internal void Release()
        {
            if (_released) return;
            var materials = new HashSet<Material>();
            if (_material != null) materials.Add(_material);
            if (_swapped)
            {
                foreach (SkinState row in _skins)
                    if (row.Renderer != null)
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
                      row.Renderer.sharedMaterials = row.Materials; }
                foreach (StaticState row in _statics)
                    if (row.Filter != null) row.Filter.sharedMesh = row.Mesh;
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
        }

        private void DestroyOwnedResources(IEnumerable<Material> materials)
        {
            foreach (Material material in materials)
                if (material != null) UnityEngine.Object.DestroyImmediate(material);
            if (_body != null) UnityEngine.Object.DestroyImmediate(_body);
            if (_empty != null) UnityEngine.Object.DestroyImmediate(_empty);
            if (_albedo != null) UnityEngine.Object.DestroyImmediate(_albedo);
            _body = null; _empty = null; _material = null; _albedo = null;
            _released = true;
        }

        private bool IsOwned(Material material)
        { return material != null && !string.IsNullOrEmpty(_ownedName) &&
            (material.name == _ownedName || material.name.StartsWith(_ownedName + " (", StringComparison.Ordinal)); }

        private void OnDestroy()
        { try { Release(); } catch (Exception) { /* Native teardown must finish. */ } }
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
