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
        private static AssetBundle _bundle;
        private static ModLogger _logger;
        private static string _status = "native-fallback:not-configured";

        private sealed class Contract
        {
            internal Contract(string variant, string assetName)
            { Variant = variant; AssetName = assetName; }
            internal string Variant;
            internal string AssetName;
        }

        private static readonly Contract[] Contracts =
        {
            new Contract(WeaponVisualVariantCatalog.SpearClassic,
                "elvenbranchedspear.prefab"),
            new Contract(WeaponVisualVariantCatalog.SpearThorn,
                "elvenbranchedspearthorn.prefab"),
            new Contract(WeaponVisualVariantCatalog.SpearCrown,
                "elvenbranchedspearcrown.prefab")
        };

        internal static bool IsLoaded
        { get { lock (Sync) return _bundle != null; } }
        internal static bool HasValidatedPrefab
        { get { lock (Sync) return Prefabs.Count == Contracts.Length; } }
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
                if (_bundle != null && Prefabs.Count == Contracts.Length)
                {
                    context.Logger.Info("elven-branched-spear", "bundle.reused",
                        "The three validated dedicated spear prefabs are already published.");
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
                if (prefabs.Length != Contracts.Length)
                    throw new InvalidDataException(
                        "Expected exactly three spear prefabs; observed " +
                        prefabs.Length + ".");
                var validated = new Dictionary<string, GameObject>(
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
                    Validate(prefab, contract.Variant);
                    validated.Add(contract.Variant, prefab);
                }
                AssetBundle previous;
                lock (Sync)
                {
                    previous = _bundle;
                    _bundle = candidate;
                    Prefabs.Clear();
                    foreach (KeyValuePair<string, GameObject> pair in validated)
                        Prefabs.Add(pair.Key, pair.Value);
                    _status = "custom:validated:3";
                    candidate = null;
                }
                if (previous != null) previous.Unload(false);
                context.Logger.Info("elven-branched-spear", "bundle.loaded",
                    "Published three exact spear variants transactionally; native donor animation, sockets, timing, trails, and sounds remain inherited.");
            }
            catch (Exception exception)
            {
                lock (Sync)
                {
                    Prefabs.Clear();
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
            if (prefab == null) return false;
            WeaponVisualParameters source = weaponType.VisualParameters;
            if (source == null || source.Model == null)
                return RejectTypeAssignment(weaponType, source,
                    new InvalidOperationException(
                        "Native Longspear fallback presentation is unavailable."));
            try
            {
                WeaponVisualParameters visual = CloneWithModel(source, prefab);
                Find(typeof(BlueprintWeaponType), "m_VisualParameters")
                    .SetValue(weaponType, visual);
                if (!ReferenceEquals(weaponType.VisualParameters.Model, prefab))
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
            if (prefab == null) return false;
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
                field.SetValue(item, CloneWithModel(source, prefab));
                if (item.VisualParameters == null ||
                    !ReferenceEquals(item.VisualParameters.Model, prefab))
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
            return prefab != null && item.VisualParameters != null &&
                ReferenceEquals(item.VisualParameters.Model, prefab);
        }

        internal static GameObject InstantiatePrefab()
        { return InstantiatePrefab(WeaponVisualVariantCatalog.SpearClassic); }

        internal static GameObject InstantiatePrefab(string variant)
        {
            GameObject prefab = GetPrefab(variant);
            return prefab == null ? null : UnityEngine.Object.Instantiate(prefab);
        }

        private static WeaponVisualParameters CloneWithModel(
            WeaponVisualParameters source, GameObject prefab)
        {
            var visual = new WeaponVisualParameters();
            foreach (FieldInfo field in typeof(WeaponVisualParameters)
                .GetFields(Fields))
                if (!field.IsStatic && !field.IsInitOnly)
                    field.SetValue(visual, field.GetValue(source));
            Find(typeof(WeaponVisualParameters), "m_WeaponModel")
                .SetValue(visual, prefab);
            return visual;
        }

        private static GameObject GetPrefab(string variant)
        {
            GameObject prefab;
            lock (Sync) Prefabs.TryGetValue(variant, out prefab);
            return prefab;
        }

        private static void Validate(GameObject prefab, string variant)
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
            if (visual == null || grip == null || support == null || tip == null ||
                butt == null) throw new InvalidDataException(
                    variant + " semantic anchors are incomplete.");
            if (!Approximately(visual.localPosition, Vector3.zero) ||
                !Approximately(visual.localRotation,
                    Quaternion.Euler(-90f, 0f, 0f)) ||
                !Approximately(visual.localScale, Vector3.one))
                throw new InvalidDataException(variant +
                    " visual transform does not map source +Z to native Longspear +Y.");
            if (!Finite(grip.localPosition) || !Finite(support.localPosition) ||
                !Finite(tip.localPosition) || !Finite(butt.localPosition) ||
                !Approximately(grip.localPosition, Vector3.zero) ||
                Mathf.Abs(support.localPosition.y - 0.37f) > 0.002f ||
                Mathf.Abs(tip.localPosition.y - 1.14f) > 0.002f ||
                Mathf.Abs(butt.localPosition.y + 1.14f) > 0.002f ||
                Mathf.Abs(support.localPosition.x) > 0.002f ||
                Mathf.Abs(support.localPosition.z) > 0.002f ||
                Vector3.Distance(tip.localPosition, butt.localPosition) < 2.25f ||
                Vector3.Distance(tip.localPosition, butt.localPosition) > 2.32f)
                throw new InvalidDataException(variant +
                    " grip/support/tip/butt geometry does not match the native Longspear frame.");
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
