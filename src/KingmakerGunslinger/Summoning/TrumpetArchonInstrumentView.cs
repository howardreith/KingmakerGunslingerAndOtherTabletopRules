using System;
using System.Collections.Generic;
using System.Linq;
using Harmony12;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.View;
using KingmakerGunslinger.Bootstrap;
using UnityEngine;

namespace KingmakerGunslinger.Summoning
{
    // Original procedural horn attached only to the measured native owned
    // weapon instance. The same real +4 sword/item remains authoritative;
    // presentation creates no item, action, resource or campaign asset.
    [DefaultExecutionOrder(20010)]
    internal sealed class TrumpetArchonInstrumentView : MonoBehaviour
    {
        internal const string UnitGuid = "b6359294f13a4adab626d14c5ededd50";
        internal const string UnitName = "KMG_Summoning_Unit_TrumpetArchon";
        internal const string SwordGuid = "1182d9ef39c443f5b4ffda85ae1e57b2";
        internal const string BlastGuid = "3bc15285b09d4b4bb9c4cb0d5c2009fb";
        internal const string RootName = "KMG_TrumpetArchon_OwnedHorn";
        internal const string MeshName = "KMG_TrumpetArchon_OriginalHornMesh";
        internal const string MaterialName = "KMG_TrumpetArchon_OwnedHornMaterial";
        private UnitEntityView _view;
        private UnitEntityData _owner;
        private GameObject _model, _root;
        private Mesh _mesh;
        private MeshFilter _filter;
        private MeshRenderer _horn;
        private Renderer[] _blade = new Renderer[0], _body = new Renderer[0];
        private bool[] _bladeEnabled = new bool[0];
        private readonly HashSet<Material> _materials = new HashSet<Material>();
        private bool _released, _swordHidden;
        internal string Status { get; private set; }
        internal int FormTransitions { get; private set; }
        internal int HornFrames { get; private set; }
        internal int SwordFrames { get; private set; }
        internal GameObject BoundModel { get { return _model; } }
        internal GameObject Horn { get { return _root; } }
        internal MeshRenderer HornRenderer { get { return _horn; } }
        internal bool HornVisible { get { return _root != null && _root.activeInHierarchy && _horn != null && _horn.enabled; } }
        internal bool SwordVisible { get { return _blade.Length > 0 && _blade.Any(value => value != null && value.enabled && value.gameObject.activeInHierarchy); } }

        internal static bool IsExactIdentity(UnitEntityView view)
        {
            var owner = view?.EntityData as UnitEntityData;
            return owner != null && owner.Blueprint.AssetGuid == UnitGuid && owner.Blueprint.name == UnitName &&
                owner.Blueprint.Body?.PrimaryHand != null && owner.Blueprint.Body.PrimaryHand.AssetGuid == SwordGuid;
        }
        internal void Configure(UnitEntityView view)
        {
            if (!IsExactIdentity(view)) throw new InvalidOperationException("Trumpet instrument view identity mismatch.");
            _view = view; _owner = view.EntityData as UnitEntityData;
            Status = "pending:owned-native-main-hand";
            Sync();
        }
        private void LateUpdate()
        {
            if (_released) return;
            try { Sync(); }
            catch (Exception error) { Release(); Status = "failed:" + error.Message; Debug.LogException(error); }
        }
        internal void Sync()
        {
            if (_released || _view == null || _owner == null) return;
            var item = _owner.Body?.PrimaryHand?.MaybeWeapon;
            bool held = TrumpetBlastRulesPolicy.HoldsInstrument(_owner, _owner.Blueprint,
                _owner.Blueprint.Body.PrimaryHand as Kingmaker.Blueprints.Items.Weapons.BlueprintItemWeapon);
            var model = held ? _view.HandsEquipment?.GetWeaponModel(false) : null;
            // Model is the active native instance, never VisualParameters.Model.
            if (model != null && (!model.transform.IsChildOf(_view.transform) ||
                ReferenceEquals(model, item.Blueprint.VisualParameters.Model)))
                throw new InvalidOperationException("Trumpet instrument is not an owned native weapon instance.");
            if (model == null && !ReferenceEquals(_model, null) ||
                model != null && (model != _model || _root == null))
            {
                ReleaseBinding();
                if (model != null) Bind(model);
            }
            if (_root == null) { Status = "idle:no-owned-held-instrument"; return; }
            bool visible = _owner.IsInState && !_owner.Destroyed && !_owner.Descriptor.State.IsDead &&
                _view.IsVisible && _model.activeInHierarchy && _body.Any(value => value != null && value.enabled && value.gameObject.activeInHierarchy);
            bool sounding = visible && _owner.Commands.Raw.OfType<UnitUseAbility>().Any(command =>
                ReferenceEquals(command.Executor, _owner) && command.IsStarted && !command.IsFinished &&
                command.Spell?.Blueprint?.AssetGuid == BlastGuid);
            AdoptAssignedMaterials();
            if (_root.activeSelf != sounding) { _root.SetActive(sounding); FormTransitions++; }
            _horn.enabled = sounding;
            if (sounding)
            {
                if (!_swordHidden) { _bladeEnabled = _blade.Select(value => value != null && value.enabled).ToArray(); _swordHidden = true; }
                for (int i = 0; i < _blade.Length; i++)
                    if (_blade[i] != null)
                    {
                        // Retain a native fader's later enable request while
                        // this exact owned blade is temporarily hidden.
                        if (_blade[i].enabled) _bladeEnabled[i] = true;
                        _blade[i].enabled = false;
                    }
            }
            else if (visible) RestoreBlade();
            if (sounding) HornFrames++; else if (SwordVisible) SwordFrames++;
            Status = sounding ? "horn:owned-native-blast-command" : visible ? "sword:owned-native-instrument" : "hidden:native-body-visibility";
        }
        private void Bind(GameObject model)
        {
            _model = model;
            _blade = model.GetComponentsInChildren<Renderer>(true);
            if (_blade.Length == 0) throw new InvalidOperationException("Trumpet native sword has no renderer.");
            _bladeEnabled = _blade.Select(value => value.enabled).ToArray();
            _body = _view.GetComponentsInChildren<SkinnedMeshRenderer>(true)
                .Where(value => !value.transform.IsChildOf(model.transform)).Cast<Renderer>().ToArray();
            if (_body.Length == 0) throw new InvalidOperationException("Trumpet has no native body visibility anchor.");
            _root = new GameObject(RootName);
            _root.layer = model.layer;
            _root.transform.SetParent(model.transform, false);
            _root.transform.localPosition = Vector3.zero; _root.transform.localRotation = Quaternion.identity;
            _root.transform.localScale = Vector3.one;
            _mesh = CreateHornMesh();
            _filter = _root.AddComponent<MeshFilter>(); _filter.sharedMesh = _mesh;
            _horn = _root.AddComponent<MeshRenderer>();
            var shader = Shader.Find("Standard") ?? Shader.Find("Legacy Shaders/Diffuse");
            if (shader == null) throw new InvalidOperationException("Trumpet horn has no visible shader.");
            var material = new Material(shader) { name = MaterialName, color = new Color(.76f, .83f, .9f, 1f) };
            if (material.HasProperty("_Metallic")) material.SetFloat("_Metallic", .8f);
            if (material.HasProperty("_Glossiness")) material.SetFloat("_Glossiness", .65f);
            if (material.HasProperty("_EmissionColor"))
            { material.SetColor("_EmissionColor", new Color(.04f, .06f, .09f, 1f)); material.EnableKeyword("_EMISSION"); }
            _materials.Add(material); _horn.sharedMaterial = material;
            _root.SetActive(false); Status = "bound:original-horn-and-native-sword";
        }
        // Authored here from a curved tube and hollow flared bell. No native
        // mesh, texture, private rig capture or external asset is embedded.
        private static Mesh CreateHornMesh()
        {
            const int rings = 25, sides = 16;
            var vertices = new Vector3[rings * sides * 2];
            var uv = new Vector2[vertices.Length]; var triangles = new List<int>();
            Func<float, Vector3> center = t => new Vector3(.1f * Mathf.Sin(t * Mathf.PI * 2f), .8f * t, .1f * Mathf.Sin(t * Mathf.PI));
            for (int ring = 0; ring < rings; ring++)
            {
                float t = ring / (float)(rings - 1);
                var tangent = (center(Mathf.Min(1f, t + .005f)) - center(Mathf.Max(0f, t - .005f))).normalized;
                var right = Vector3.Cross(tangent, Vector3.forward).normalized;
                var forward = Vector3.Cross(right, tangent).normalized;
                float radius = .022f + .14f * Mathf.Pow(t, 6f);
                for (int shell = 0; shell < 2; shell++)
                    for (int side = 0; side < sides; side++)
                    {
                        int i = (shell * rings + ring) * sides + side;
                        float angle = side * Mathf.PI * 2f / sides;
                        vertices[i] = center(t) + (right * Mathf.Cos(angle) + forward * Mathf.Sin(angle)) * (radius - shell * .004f);
                        uv[i] = new Vector2(side / (float)sides, t);
                    }
            }
            Action<int,int,int> triangle = (a,b,c) => { triangles.Add(a); triangles.Add(b); triangles.Add(c); };
            for (int shell = 0; shell < 2; shell++)
                for (int ring = 0; ring < rings - 1; ring++)
                    for (int side = 0; side < sides; side++)
                    {
                        int a = (shell * rings + ring) * sides + side, b = (shell * rings + ring) * sides + (side + 1) % sides;
                        int c = a + sides, d = b + sides;
                        if (shell == 0) { triangle(a,c,b); triangle(b,c,d); }
                        else { triangle(a,b,c); triangle(b,d,c); }
                    }
            for (int side = 0; side < sides; side++)
            {
                int next = (side + 1) % sides;
                int a = (rings - 1) * sides + side, b = (rings - 1) * sides + next;
                int c = a + rings * sides, d = b + rings * sides;
                triangle(a,c,b); triangle(b,c,d);
                a = side; b = next; c = a + rings * sides; d = b + rings * sides;
                triangle(a,b,c); triangle(b,d,c);
            }
            var mesh = new Mesh { name = MeshName, vertices = vertices, uv = uv, triangles = triangles.ToArray() };
            mesh.RecalculateNormals(); mesh.RecalculateBounds(); return mesh;
        }
        private void AdoptAssignedMaterials()
        {
            if (_horn == null) return;
            foreach (var material in _horn.sharedMaterials.Where(value => value != null))
            {
                _materials.Add(material);
                if (!material.name.StartsWith(MaterialName, StringComparison.Ordinal)) material.name = MaterialName + "_NativeFaderInstance";
            }
        }
        internal UnityEngine.Object[] CaptureOwnedResources()
        {
            return new UnityEngine.Object[] { this, _root, _mesh, _filter, _horn }.Where(value => value != null)
                .Concat(_materials.Where(value => value != null).Cast<UnityEngine.Object>())
                .Concat(_horn == null ? Array.Empty<UnityEngine.Object>() : _horn.sharedMaterials
                    .Where(value => value != null).Cast<UnityEngine.Object>()).Distinct().ToArray();
        }
        private void RestoreBlade()
        {
            if (!_swordHidden) return;
            for (int i = 0; i < _blade.Length; i++) if (_blade[i] != null) _blade[i].enabled = _bladeEnabled[i];
            _swordHidden = false;
        }
        private void ReleaseBinding()
        {
            AdoptAssignedMaterials();
            RestoreBlade();
            if (_root != null) UnityEngine.Object.DestroyImmediate(_root);
            if (_mesh != null) UnityEngine.Object.DestroyImmediate(_mesh);
            foreach (var material in _materials) if (material != null) UnityEngine.Object.DestroyImmediate(material);
            _materials.Clear(); _model = null; _root = null; _mesh = null; _filter = null; _horn = null;
            _blade = new Renderer[0]; _body = new Renderer[0]; _bladeEnabled = new bool[0];
        }
        internal void Release()
        {
            if (_released) return;
            ReleaseBinding(); _released = true; enabled = false; _owner = null; _view = null; Status = "released";
        }
        private void OnDestroy() { Release(); }
    }

    [HarmonyPatch(typeof(UnitEntityView), "OnDataAttached")]
    internal static class TrumpetArchonInstrumentViewPatch
    {
        private static void Postfix(UnitEntityView __instance)
        {
            ModContext context;
            if (!TrumpetArchonInstrumentView.IsExactIdentity(__instance) || !ModContext.TryGet(out context) ||
                context.FeatureModules?.Active == null || !context.FeatureModules.Active.ExpandedSummoning ||
                __instance.GetComponent<TrumpetArchonInstrumentView>() != null) return;
            var owned = __instance.gameObject.AddComponent<TrumpetArchonInstrumentView>();
            try { owned.Configure(__instance); }
            catch (Exception error) { owned.Release(); Debug.LogException(error); }
        }
    }
    [HarmonyPatch(typeof(UnitEntityView), "OnDestroy")]
    internal static class TrumpetArchonInstrumentViewTeardownPatch
    {
        private static void Prefix(UnitEntityView __instance)
        {
            var owned = __instance == null ? null : __instance.GetComponent<TrumpetArchonInstrumentView>();
            if (owned == null) return;
            try { owned.Release(); } catch (Exception) { /* Native destruction must complete. */ }
        }
    }
}
