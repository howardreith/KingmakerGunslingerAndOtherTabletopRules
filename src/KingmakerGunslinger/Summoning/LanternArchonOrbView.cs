using System;
using System.Linq;
using Harmony12;
using Kingmaker.View;
using KingmakerGunslinger.Bootstrap;
using UnityEngine;

namespace KingmakerGunslinger.Summoning
{
    /// <summary>
    /// A small owned golden orb carried by the released Lantern Archon view.
    /// The Will-o'-Wisp donor continues to supply movement, fading and native
    /// effects; no donor renderer, mesh, material or prefab is modified.
    /// </summary>
    internal sealed class LanternArchonOrbView : MonoBehaviour
    {
        internal const string UnitGuid = "02f8e9c6c91549deaded9ef667399449";
        internal const string UnitName = "KMG_Summoning_Unit_LanternArchon";
        internal const string RootName = "KMG_LanternArchon_OwnedOrbRoot";
        internal const string OrbName = "KMG_LanternArchon_OwnedOrb";
        internal const string MaterialName = "KMG_LanternArchon_OwnedOrbMaterial";
        private static readonly Color Surface = new Color(1f, .82f, .31f, 1f);
        private static readonly Color Emission = new Color(2.4f, 1.55f, .34f, 1f);

        private UnitEntityView _view;
        private Renderer[] _nativeRenderers = new Renderer[0];
        private GameObject _root;
        private GameObject _orb;
        private MeshRenderer _renderer;
        private Material _material;
        private Light _light;
        private bool _released;
        private int _enables;
        private int _disables;

        internal GameObject Root { get { return _root; } }
        internal GameObject Orb { get { return _orb; } }
        internal MeshRenderer OrbRenderer { get { return _renderer; } }
        internal Material OrbMaterial { get { return _material; } }
        internal Light OrbLight { get { return _light; } }
        internal int NativeRendererCount { get { return _nativeRenderers.Length; } }
        internal string Status { get; private set; }

        internal static bool IsExactIdentity(UnitEntityView view)
        {
            return view != null && view.EntityData != null &&
                view.EntityData.Blueprint != null &&
                view.EntityData.Blueprint.AssetGuid == UnitGuid &&
                view.EntityData.Blueprint.name == UnitName &&
                view.EntityData.Blueprint.Prefab != null;
        }

        internal void Configure(UnitEntityView view)
        {
            if (!IsExactIdentity(view))
                throw new InvalidOperationException(
                    "Owned Lantern orb identity or donor prefab mismatch.");
            _view = view;
            _nativeRenderers = view.GetComponentsInChildren<Renderer>(true)
                .Where(value => value != null).ToArray();
            if (_nativeRenderers.Length == 0)
                throw new InvalidOperationException(
                    "Lantern donor view has no native renderer anchor.");

            Renderer anchor = _nativeRenderers.FirstOrDefault(value =>
                value is SkinnedMeshRenderer || value is MeshRenderer) ??
                _nativeRenderers[0];
            Vector3 center = view.transform.InverseTransformPoint(anchor.bounds.center);
            if (!Finite(center) || Math.Abs(center.x) > 1.5f ||
                Math.Abs(center.z) > 1.5f || center.y < .25f || center.y > 2.5f)
                center = new Vector3(0f, .9f, 0f);
            float anchorDiameter = Mathf.Max(anchor.bounds.size.x,
                Mathf.Max(anchor.bounds.size.y, anchor.bounds.size.z));
            float worldDiameter = Mathf.Clamp(anchorDiameter * .55f, .65f, 1.05f);
            Vector3 lossy = view.transform.lossyScale;
            float parentScale = Mathf.Max(Mathf.Abs(lossy.x),
                Mathf.Max(Mathf.Abs(lossy.y), Mathf.Abs(lossy.z)));
            if (float.IsNaN(parentScale) || float.IsInfinity(parentScale) ||
                parentScale <= .001f) parentScale = 1f;

            _root = new GameObject(RootName);
            _root.transform.SetParent(view.transform, false);
            _root.transform.localPosition = center;
            _orb = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            _orb.name = OrbName;
            _orb.transform.SetParent(_root.transform, false);
            _orb.transform.localPosition = Vector3.zero;
            _orb.transform.localRotation = Quaternion.identity;
            _orb.transform.localScale = Vector3.one * (worldDiameter / parentScale);
            Collider collider = _orb.GetComponent<Collider>();
            if (collider != null) UnityEngine.Object.DestroyImmediate(collider);
            _renderer = _orb.GetComponent<MeshRenderer>();
            if (_renderer == null)
                throw new InvalidOperationException(
                    "Lantern owned sphere has no mesh renderer.");

            Shader shader = Shader.Find("Standard") ??
                Shader.Find("Legacy Shaders/Diffuse");
            if (shader == null)
                throw new InvalidOperationException(
                    "Lantern owned orb has no visible shader.");
            _material = new Material(shader) { name = MaterialName, color = Surface };
            if (_material.HasProperty("_EmissionColor"))
            {
                _material.SetColor("_EmissionColor", Emission);
                _material.EnableKeyword("_EMISSION");
            }
            _renderer.sharedMaterial = _material;

            _light = _root.AddComponent<Light>();
            _light.type = LightType.Point;
            _light.color = Surface;
            _light.range = 2.4f;
            _light.intensity = 1.55f;
            _light.shadows = LightShadows.None;
            _light.renderMode = LightRenderMode.ForceVertex;
            Status = "attached:owned-golden-orb";
            Apply(false);
        }

        private static bool Finite(Vector3 value)
        {
            return !float.IsNaN(value.x) && !float.IsInfinity(value.x) &&
                !float.IsNaN(value.y) && !float.IsInfinity(value.y) &&
                !float.IsNaN(value.z) && !float.IsInfinity(value.z);
        }

        private bool ShouldShow()
        {
            return !_released && _view != null && _view.IsVisible &&
                _nativeRenderers.Any(value => value != null && value.enabled &&
                    value.gameObject != null && value.gameObject.activeInHierarchy);
        }

        private void Apply(bool visible)
        {
            if (_root != null && _root.activeSelf != visible)
            {
                _root.SetActive(visible);
                if (visible) _enables++; else _disables++;
            }
            // The native fader discovers renderers after OnDataAttached and can
            // disable this dynamically added renderer during appearance. It
            // does not later own/re-enable that renderer. The native donor
            // renderer remains the visibility authority; this component owns
            // only the matching state of its private renderer and light.
            if (_renderer != null) _renderer.enabled = visible;
            if (_light != null) _light.enabled = visible;
        }

        private void LateUpdate() { SyncToNativeVisibility(); }
        internal void SyncToNativeVisibility() { Apply(ShouldShow()); }

        internal bool Ready
        {
            get
            {
                return !_released && _root != null && _root.activeInHierarchy &&
                    _orb != null && _renderer != null && _renderer.enabled &&
                    _material != null && _renderer.sharedMaterial == _material &&
                    _light != null && _light.enabled;
            }
        }

        internal string Describe()
        {
            return "status=" + (Status ?? "<none>") +
                ";ready=" + Ready + ";nativeRenderers=" +
                _nativeRenderers.Length + ";root=" + (_root != null) +
                ";rootActive=" + (_root != null && _root.activeInHierarchy) +
                ";orb=" + (_orb != null) + ";renderer=" +
                (_renderer != null && _renderer.enabled) + ";material=" +
                (_material == null ? "<none>" : _material.name) +
                ";light=" + (_light != null && _light.enabled) +
                ";viewVisible=" + (_view != null && _view.IsVisible) +
                ";nativeVisible=" + _nativeRenderers.Any(value => value != null &&
                    value.enabled && value.gameObject != null &&
                    value.gameObject.activeInHierarchy) +
                ";range=" + (_light == null ? 0f : _light.range) +
                ";intensity=" + (_light == null ? 0f : _light.intensity) +
                ";enables=" + _enables + ";disables=" + _disables;
        }

        internal UnityEngine.Object[] CaptureOwnedResources()
        {
            return new UnityEngine.Object[] { this, _root, _orb, _renderer,
                _material, _light }.Where(value => value != null).Distinct().ToArray();
        }

        internal void Fail(Exception error)
        {
            Release();
            Status = "failed:" + error.GetType().Name + ":" + error.Message;
        }

        internal void Release()
        {
            if (_released) return;
            _released = true;
            if (_light != null) _light.enabled = false;
            GameObject root = _root;
            Material material = _material;
            _root = null; _orb = null; _renderer = null; _light = null;
            _material = null; _view = null; _nativeRenderers = new Renderer[0];
            if (root != null) UnityEngine.Object.DestroyImmediate(root);
            if (material != null) UnityEngine.Object.DestroyImmediate(material);
            Status = "released";
        }

        internal static string CountOwnedObjects(out int components,
            out int roots, out int orbs, out int materials, out int lights)
        {
            components = Resources.FindObjectsOfTypeAll<LanternArchonOrbView>()
                .Count(value => value != null);
            GameObject[] objects = Resources.FindObjectsOfTypeAll<GameObject>();
            roots = objects.Count(value => value != null && value.name == RootName);
            orbs = objects.Count(value => value != null && value.name == OrbName);
            materials = Resources.FindObjectsOfTypeAll<Material>().Count(value =>
                value != null && value.name == MaterialName);
            lights = Resources.FindObjectsOfTypeAll<Light>().Count(value =>
                value != null && value.gameObject != null &&
                value.gameObject.name == RootName);
            return "components=" + components + ";roots=" + roots +
                ";orbs=" + orbs + ";materials=" + materials +
                ";lights=" + lights;
        }

        private void OnDestroy() { Release(); }
    }

    [HarmonyPatch(typeof(UnitEntityView), "OnDataAttached")]
    internal static class LanternArchonOrbViewPatch
    {
        private static void Postfix(UnitEntityView __instance)
        {
            ModContext context;
            if (!LanternArchonOrbView.IsExactIdentity(__instance) ||
                !ModContext.TryGet(out context) || context.FeatureModules == null ||
                context.FeatureModules.Active == null ||
                !context.FeatureModules.Active.ExpandedSummoning ||
                __instance.GetComponent<LanternArchonOrbView>() != null) return;
            LanternArchonOrbView owned =
                __instance.gameObject.AddComponent<LanternArchonOrbView>();
            try { owned.Configure(__instance); }
            catch (Exception error)
            {
                owned.Fail(error);
                Debug.LogException(error);
            }
        }
    }

    [HarmonyPatch(typeof(UnitEntityView), "OnDestroy")]
    internal static class LanternArchonOrbViewTeardownPatch
    {
        private static void Prefix(UnitEntityView __instance)
        {
            LanternArchonOrbView owned = __instance == null ? null :
                __instance.GetComponent<LanternArchonOrbView>();
            if (owned == null) return;
            try { owned.Release(); }
            catch (Exception) { /* Native view destruction must complete. */ }
        }
    }
}
