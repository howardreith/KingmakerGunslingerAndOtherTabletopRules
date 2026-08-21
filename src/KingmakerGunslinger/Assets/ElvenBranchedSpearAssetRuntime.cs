using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Kingmaker.Blueprints.Items.Weapons;
using KingmakerGunslinger.Bootstrap;
using UnityEngine;

namespace KingmakerGunslinger.Assets
{
    internal static class ElvenBranchedSpearAssetRuntime
    {
        internal const string BundleName =
            "kingmakergunslinger.elvenbranchedspear";
        private const BindingFlags Fields = BindingFlags.Instance |
            BindingFlags.Public | BindingFlags.NonPublic;
        private static readonly object Sync = new object();
        private static readonly Dictionary<string, GameObject> Prefabs =
            new Dictionary<string, GameObject>(StringComparer.Ordinal);
        private static readonly Dictionary<string, GameObject> BackPrefabs =
            new Dictionary<string, GameObject>(StringComparer.Ordinal);
        private static AssetBundle _bundle;
        private static ModLogger _logger;
        private static string _status = "native-fallback:not-configured";

        private sealed class Contract
        {
            internal Contract(string variant, string assetName,
                string backAssetName)
            {
                Variant = variant;
                AssetName = assetName;
                BackAssetName = backAssetName;
            }
            internal string Variant;
            internal string AssetName;
            internal string BackAssetName;
        }

        private static readonly Contract[] Contracts =
        {
            new Contract(WeaponVisualVariantCatalog.SpearClassic,
                "elvenbranchedspear.prefab",
                "elvenbranchedspearback.prefab"),
            new Contract(WeaponVisualVariantCatalog.SpearThorn,
                "elvenbranchedspearthorn.prefab",
                "elvenbranchedspearthornback.prefab"),
            new Contract(WeaponVisualVariantCatalog.SpearCrown,
                "elvenbranchedspearcrown.prefab",
                "elvenbranchedspearcrownback.prefab")
        };

        internal static bool IsLoaded
        { get { lock (Sync) return _bundle != null; } }
        internal static bool HasValidatedPrefab
        { get { lock (Sync) return Prefabs.Count == Contracts.Length &&
            BackPrefabs.Count == Contracts.Length; } }
        internal static string Status
        { get { lock (Sync) return _status; } }

        internal static void Configure(ModContext context)
        {
            if (context == null) throw new ArgumentNullException("context");
            lock (Sync) _logger = context.Logger;
            if (!context.FeatureModules.Active.ElvenBranchedSpears)
            {
                lock (Sync) _status = "native-fallback:module-disabled";
                context.Logger.Info("elven-branched-spear", "bundle.skipped",
                    "Presentation module is disabled; native Longspear fallback remains active.");
                return;
            }
            lock (Sync)
            {
                if (_bundle != null && Prefabs.Count == Contracts.Length &&
                    BackPrefabs.Count == Contracts.Length)
                {
                    context.Logger.Info("elven-branched-spear", "bundle.reused",
                        "The three validated held/back spear pairs are already published.");
                    return;
                }
            }
            string path = Path.Combine(context.ModEntry.Path, "assets", "bundles",
                BundleName);
            if (!File.Exists(path))
            {
                lock (Sync) _status = "native-fallback:bundle-missing";
                context.Logger.Warning("elven-branched-spear", "bundle.missing",
                    "Dedicated bundle is unavailable; native Longspear fallback remains active: " + path);
                return;
            }
            AssetBundle candidate = null;
            try
            {
                candidate = AssetBundle.LoadFromFile(path);
                if (candidate == null) throw new InvalidDataException(
                    "Unity rejected the dedicated spear bundle.");
                string[] prefabs = candidate.GetAllAssetNames().Where(value =>
                    value.EndsWith(".prefab", StringComparison.OrdinalIgnoreCase))
                    .ToArray();
                if (prefabs.Length != Contracts.Length * 2)
                    throw new InvalidDataException(
                        "Expected exactly three held/back spear pairs; observed " +
                        prefabs.Length + ".");
                var validated = new Dictionary<string, GameObject>(
                    StringComparer.Ordinal);
                var validatedBack = new Dictionary<string, GameObject>(
                    StringComparer.Ordinal);
                foreach (Contract contract in Contracts)
                {
                    string[] matches = prefabs.Where(value => value.EndsWith(
                        "/" + contract.AssetName,
                        StringComparison.OrdinalIgnoreCase)).ToArray();
                    if (matches.Length != 1) throw new InvalidDataException(
                        "Expected one " + contract.AssetName + "; observed " +
                        matches.Length + ".");
                    GameObject prefab = candidate.LoadAsset<GameObject>(matches[0]);
                    Validate(prefab, contract.Variant, false);
                    validated.Add(contract.Variant, prefab);
                    string[] backMatches = prefabs.Where(value => value.EndsWith(
                        "/" + contract.BackAssetName,
                        StringComparison.OrdinalIgnoreCase)).ToArray();
                    if (backMatches.Length != 1) throw new InvalidDataException(
                        "Expected one " + contract.BackAssetName + "; observed " +
                        backMatches.Length + ".");
                    GameObject backPrefab = candidate.LoadAsset<GameObject>(
                        backMatches[0]);
                    Validate(backPrefab, contract.Variant, true);
                    Transform heldVisual = prefab.transform.Find("Visual");
                    Transform storedVisual = backPrefab.transform.Find("Visual");
                    if (ReferenceEquals(prefab, backPrefab) ||
                        (Approximately(heldVisual.localPosition,
                            storedVisual.localPosition) &&
                         Approximately(heldVisual.localRotation,
                            storedVisual.localRotation)))
                        throw new InvalidDataException(contract.Variant +
                            " held and stored presentations share an incompatible transform.");
                    validatedBack.Add(contract.Variant, backPrefab);
                }
                AssetBundle previous;
                lock (Sync)
                {
                    previous = _bundle;
                    _bundle = candidate;
                    Prefabs.Clear();
                    foreach (KeyValuePair<string, GameObject> pair in validated)
                        Prefabs.Add(pair.Key, pair.Value);
                    BackPrefabs.Clear();
                    foreach (KeyValuePair<string, GameObject> pair in validatedBack)
                        BackPrefabs.Add(pair.Key, pair.Value);
                    _status = "custom:validated:3-pairs";
                    candidate = null;
                }
                if (previous != null) previous.Unload(false);
                context.Logger.Info("elven-branched-spear", "bundle.loaded",
                    "Published three exact held/back spear pairs transactionally; native donor animation, sockets, timing, trails, and sounds remain inherited.");
            }
            catch (Exception exception)
            {
                lock (Sync)
                {
                    Prefabs.Clear();
                    BackPrefabs.Clear();
                    _status = "native-fallback:bundle-rejected:" +
                        exception.GetType().Name;
                }
                context.Logger.Failure("elven-branched-spear",
                    "bundle.load-failed",
                    "Dedicated spear presentation was rejected; native Longspear fallback remains active.",
                    exception);
            }
            finally { if (candidate != null) candidate.Unload(false); }
        }

        internal static bool ApplyTo(BlueprintWeaponType weaponType)
        {
            if (weaponType == null) throw new ArgumentNullException("weaponType");
            GameObject prefab = GetPrefab(WeaponVisualVariantCatalog.SpearClassic);
            GameObject backPrefab = GetBackPrefab(
                WeaponVisualVariantCatalog.SpearClassic);
            if (prefab == null || backPrefab == null) return false;
            WeaponVisualParameters source = weaponType.VisualParameters;
            if (source == null || source.Model == null)
                return RejectTypeAssignment(weaponType, source,
                    new InvalidOperationException(
                        "Native Longspear fallback presentation is unavailable."));
            try
            {
                WeaponVisualParameters visual = CloneWithModels(source, prefab,
                    backPrefab);
                Find(typeof(BlueprintWeaponType), "m_VisualParameters")
                    .SetValue(weaponType, visual);
                if (!ReferenceEquals(weaponType.VisualParameters.Model, prefab) ||
                    !ReferenceEquals(weaponType.VisualParameters.BeltModel,
                        backPrefab))
                    throw new InvalidOperationException(
                        "Validated spear type fallback did not round-trip.");
                return true;
            }
            catch (Exception exception)
            { return RejectTypeAssignment(weaponType, source, exception); }
        }

        internal static bool ApplyTo(BlueprintItemWeapon item,
            string blueprintSymbol)
        {
            if (item == null) throw new ArgumentNullException("item");
            string variant = WeaponVisualVariantCatalog.Require(blueprintSymbol);
            GameObject prefab = GetPrefab(variant);
            GameObject backPrefab = GetBackPrefab(variant);
            if (prefab == null || backPrefab == null) return false;
            FieldInfo field = Find(item.GetType(), "m_VisualParameters");
            object original = field.GetValue(item);
            WeaponVisualParameters source = item.VisualParameters ??
                (item.Type == null ? null : item.Type.VisualParameters);
            if (source == null || source.Model == null)
                return RejectItemAssignment(item, field, original, variant,
                    new InvalidOperationException(
                        "Spear item/type fallback presentation is unavailable."));
            try
            {
                field.SetValue(item, CloneWithModels(source, prefab, backPrefab));
                if (item.VisualParameters == null ||
                    !ReferenceEquals(item.VisualParameters.Model, prefab) ||
                    !ReferenceEquals(item.VisualParameters.BeltModel, backPrefab))
                    throw new InvalidOperationException(
                        "Exact spear item variant did not round-trip.");
                return true;
            }
            catch (Exception exception)
            { return RejectItemAssignment(item, field, original, variant, exception); }
        }

        internal static bool HasExactVisual(BlueprintItemWeapon item,
            string blueprintSymbol)
        {
            if (item == null) return false;
            string variant = WeaponVisualVariantCatalog.Require(blueprintSymbol);
            GameObject prefab = GetPrefab(variant);
            GameObject backPrefab = GetBackPrefab(variant);
            return prefab != null && item.VisualParameters != null &&
                ReferenceEquals(item.VisualParameters.Model, prefab) &&
                ReferenceEquals(item.VisualParameters.BeltModel, backPrefab);
        }

        internal static GameObject InstantiatePrefab()
        { return InstantiatePrefab(WeaponVisualVariantCatalog.SpearClassic); }

        internal static GameObject InstantiatePrefab(string variant)
        {
            GameObject prefab = GetPrefab(variant);
            return prefab == null ? null : UnityEngine.Object.Instantiate(prefab);
        }

        internal static GameObject InstantiateBackPrefab(string variant)
        {
            GameObject prefab = GetBackPrefab(variant);
            return prefab == null ? null : UnityEngine.Object.Instantiate(prefab);
        }

        private static WeaponVisualParameters CloneWithModels(
            WeaponVisualParameters source, GameObject prefab,
            GameObject backPrefab)
        {
            var visual = new WeaponVisualParameters();
            foreach (FieldInfo field in typeof(WeaponVisualParameters)
                .GetFields(Fields))
                if (!field.IsStatic && !field.IsInitOnly)
                    field.SetValue(visual, field.GetValue(source));
            Find(typeof(WeaponVisualParameters), "m_WeaponModel")
                .SetValue(visual, prefab);
            Find(typeof(WeaponVisualParameters), "m_WeaponBeltModel")
                .SetValue(visual, backPrefab);
            return visual;
        }

        private static GameObject GetPrefab(string variant)
        {
            GameObject prefab;
            lock (Sync) Prefabs.TryGetValue(variant, out prefab);
            return prefab;
        }

        internal static GameObject GetBackPrefab(string variant)
        {
            GameObject prefab;
            lock (Sync) BackPrefabs.TryGetValue(variant, out prefab);
            return prefab;
        }

        private static void Validate(GameObject prefab, string variant, bool back)
        {
            if (prefab == null) throw new InvalidDataException(
                variant + " prefab is null.");
            Transform root = prefab.transform;
            if (!Approximately(root.localPosition, Vector3.zero) ||
                !Approximately(root.localRotation, Quaternion.identity) ||
                !Approximately(root.localScale, Vector3.one))
                throw new InvalidDataException(variant +
                    " root transform is not identity.");
            Transform visual = root.Find("Visual");
            Transform grip = root.Find("Grip");
            Transform support = root.Find("SupportHandTarget");
            Transform tip = root.Find("Tip");
            Transform butt = root.Find("Butt");
            Transform headUp = root.Find(
                WeaponPresentationFrameContract.HeadUpMarker);
            Transform backMount = root.Find("BackMount");
            if (visual == null || grip == null || support == null || tip == null ||
                butt == null || headUp == null || (back && backMount == null))
                throw new InvalidDataException(
                    variant + " semantic anchors are incomplete.");
            Vector3 expectedPosition = back
                ? new Vector3(0f, -0.18f, 0.06f) : Vector3.zero;
            Quaternion expectedRotation = back
                ? Quaternion.AngleAxis(35f, Vector3.forward) *
                    Quaternion.Euler(-90f, 0f, 0f)
                : Quaternion.Euler(90f, 0f, 0f);
            if (!Approximately(visual.localPosition, expectedPosition) ||
                !Approximately(visual.localRotation, expectedRotation) ||
                !Approximately(visual.localScale, Vector3.one))
                throw new InvalidDataException(variant +
                    (back ? " back visual transform is not the exact diagonal frame." :
                    " held visual transform differs from its declared source-frame mapping."));
            Vector3 expectedGrip = expectedPosition;
            Vector3 expectedSupport = expectedPosition + expectedRotation *
                new Vector3(0f, 0f, 0.37f);
            Vector3 expectedTip = expectedPosition + expectedRotation *
                new Vector3(0f, 0f, 1.14f);
            Vector3 expectedButt = expectedPosition + expectedRotation *
                new Vector3(0f, 0f, -1.14f);
            if (!Finite(grip.localPosition) || !Finite(support.localPosition) ||
                !Finite(tip.localPosition) || !Finite(butt.localPosition) ||
                !Approximately(grip.localPosition, expectedGrip) ||
                !Approximately(support.localPosition, expectedSupport) ||
                !Approximately(tip.localPosition, expectedTip) ||
                !Approximately(butt.localPosition, expectedButt) ||
                Vector3.Distance(tip.localPosition, butt.localPosition) < 2.25f ||
                Vector3.Distance(tip.localPosition, butt.localPosition) > 2.32f ||
                (!back && (tip.localPosition.y >= 0f ||
                    butt.localPosition.y <= 0f)) ||
                (back && (tip.localPosition.y <= butt.localPosition.y ||
                    Mathf.Abs(tip.localPosition.x - butt.localPosition.x) < 1f)))
                throw new InvalidDataException(variant +
                    " grip/support/tip/butt geometry does not match the native Longspear frame.");
            WeaponPresentationSemanticFrame frame =
                WeaponPresentationFrameContract.Require(root, variant,
                    "Tip", WeaponPresentationFrameContract.HeadUpMarker,
                    true, 2.25f, 2.32f);
            WeaponPresentationFrameContract.ValidateRendererEndpoints(root,
                visual, frame, variant, 0.08f);
            WeaponPresentationFrameContract.ValidateSecondaryAsPlaneNormal(
                root, visual, frame, variant, 0.20f);
            Renderer[] renderers = visual.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0 || renderers.Any(value => value == null ||
                !value.enabled || !value.gameObject.activeSelf ||
                value.sharedMaterials == null || value.sharedMaterials.Length == 0 ||
                value.sharedMaterials.Any(material => material == null ||
                    material.shader == null)))
                throw new InvalidDataException(variant +
                    " renderers or materials are incomplete.");
            if (prefab.GetComponentsInChildren<Camera>(true).Length != 0 ||
                prefab.GetComponentsInChildren<Light>(true).Length != 0)
                throw new InvalidDataException(variant +
                    " prefab contains a camera or light.");
        }

        private static bool RejectTypeAssignment(BlueprintWeaponType weaponType,
            WeaponVisualParameters fallback, Exception exception)
        {
            try
            {
                if (fallback != null)
                    Find(typeof(BlueprintWeaponType), "m_VisualParameters")
                        .SetValue(weaponType, fallback);
            }
            catch { }
            Reject("type", exception);
            return false;
        }

        private static bool RejectItemAssignment(BlueprintItemWeapon item,
            FieldInfo field, object fallback, string variant, Exception exception)
        {
            try { field.SetValue(item, fallback); }
            catch { }
            Reject(variant, exception);
            return false;
        }

        private static void Reject(string scope, Exception exception)
        {
            ModLogger logger;
            lock (Sync)
            {
                _status = "native-fallback:model-assignment-rejected:" + scope +
                    ":" + exception.GetType().Name;
                logger = _logger;
            }
            if (logger != null) logger.Failure("elven-branched-spear",
                "model.assignment-failed",
                "Exact spear model assignment was rejected; the native/type fallback remains active.",
                exception);
        }

        private static FieldInfo Find(Type type, string name)
        {
            for (Type current = type; current != null; current = current.BaseType)
            {
                FieldInfo field = current.GetField(name, Fields |
                    BindingFlags.DeclaredOnly);
                if (field != null) return field;
            }
            throw new MissingFieldException(type.FullName, name);
        }
        private static bool Approximately(Vector3 left, Vector3 right)
        { return (left - right).sqrMagnitude <= 0.000001f; }
        private static bool Approximately(Quaternion left, Quaternion right)
        { return Mathf.Abs(Quaternion.Dot(left, right)) >= 0.999999f; }
        private static bool Finite(Vector3 value)
        { return Finite(value.x) && Finite(value.y) && Finite(value.z); }
        private static bool Finite(float value)
        { return !float.IsNaN(value) && !float.IsInfinity(value); }
    }
}
