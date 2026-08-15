using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Kingmaker.Blueprints.Items.Weapons;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.EasternWeapons;
using UnityEngine;

namespace KingmakerGunslinger.Assets
{
    internal static class EasternWeaponAssetRuntime
    {
        internal const string BundleName = "kingmakergunslinger.easternweapons";
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
            internal Contract(EasternWeaponFamily family, string variant,
                string assetName, float support, float tip, float butt,
                float minimum, float maximum)
            { Family = family; Variant = variant; AssetName = assetName;
              Support = support; Tip = tip; Butt = butt; Minimum = minimum;
              Maximum = maximum; }
            internal EasternWeaponFamily Family;
            internal string Variant, AssetName;
            internal float Support, Tip, Butt, Minimum, Maximum;
        }

        private static readonly Contract[] Contracts =
        {
            C(EasternWeaponFamily.Wakizashi,
                WeaponVisualVariantCatalog.WakizashiClassic, "wakizashi.prefab"),
            C(EasternWeaponFamily.Wakizashi,
                WeaponVisualVariantCatalog.WakizashiPetal, "wakizashipetal.prefab"),
            C(EasternWeaponFamily.Wakizashi,
                WeaponVisualVariantCatalog.WakizashiMoon, "wakizashimoon.prefab"),
            C(EasternWeaponFamily.Wakizashi,
                WeaponVisualVariantCatalog.WakizashiCapstone, "wakizashicapstone.prefab"),
            C(EasternWeaponFamily.Katana,
                WeaponVisualVariantCatalog.KatanaClassic, "katana.prefab"),
            C(EasternWeaponFamily.Katana,
                WeaponVisualVariantCatalog.KatanaReed, "katanareed.prefab"),
            C(EasternWeaponFamily.Katana,
                WeaponVisualVariantCatalog.KatanaRegal, "katanaregal.prefab"),
            C(EasternWeaponFamily.Katana,
                WeaponVisualVariantCatalog.KatanaCapstone, "katanacapstone.prefab"),
            C(EasternWeaponFamily.Nodachi,
                WeaponVisualVariantCatalog.NodachiClassic, "nodachi.prefab"),
            C(EasternWeaponFamily.Nodachi,
                WeaponVisualVariantCatalog.NodachiCleaver, "nodachicleaver.prefab"),
            C(EasternWeaponFamily.Nodachi,
                WeaponVisualVariantCatalog.NodachiTitan, "nodachititan.prefab"),
            C(EasternWeaponFamily.Nodachi,
                WeaponVisualVariantCatalog.NodachiCapstone, "nodachicapstone.prefab")
        };

        private static Contract C(EasternWeaponFamily family, string variant,
            string assetName)
        {
            if (family == EasternWeaponFamily.Wakizashi)
                return new Contract(family, variant, assetName, 0.07f, 0.56f,
                    -0.20f, 0.55f, 0.95f);
            if (family == EasternWeaponFamily.Katana)
                return new Contract(family, variant, assetName, 0.10f, 0.76f,
                    -0.29f, 0.85f, 1.25f);
            return new Contract(family, variant, assetName, 0.13f, 1.16f,
                -0.42f, 1.30f, 1.90f);
        }

        internal static bool IsLoaded
        { get { lock (Sync) return _bundle != null; } }
        internal static bool HasValidatedPrefabs
        { get { lock (Sync) return Prefabs.Count == Contracts.Length; } }
        internal static string Status
        { get { lock (Sync) return _status; } }

        internal static void Configure(ModContext context)
        {
            if (context == null) throw new ArgumentNullException("context");
            lock (Sync) _logger = context.Logger;
            if (!context.FeatureModules.Active.EasternWeapons)
            {
                lock (Sync) _status = "native-fallback:module-disabled";
                context.Logger.Info("eastern-weapons", "bundle.skipped",
                    "Presentation module is disabled; native Scimitar, Bastard Sword, and Greatsword fallbacks remain active.");
                return;
            }
            lock (Sync)
            {
                if (_bundle != null && Prefabs.Count == Contracts.Length)
                {
                    context.Logger.Info("eastern-weapons", "bundle.reused",
                        "The twelve validated Eastern Weapon prefabs are already published.");
                    return;
                }
            }
            string path = Path.Combine(context.ModEntry.Path, "assets", "bundles",
                BundleName);
            if (!File.Exists(path))
            {
                lock (Sync) _status = "native-fallback:bundle-missing";
                context.Logger.Warning("eastern-weapons", "bundle.missing",
                    "Dedicated bundle is unavailable; native family donors remain active: " + path);
                return;
            }
            AssetBundle candidate = null;
            try
            {
                candidate = AssetBundle.LoadFromFile(path);
                if (candidate == null) throw new InvalidDataException(
                    "Unity rejected the Eastern Weapons bundle.");
                string[] prefabs = candidate.GetAllAssetNames().Where(value =>
                    value.EndsWith(".prefab", StringComparison.OrdinalIgnoreCase))
                    .ToArray();
                if (prefabs.Length != Contracts.Length)
                    throw new InvalidDataException(
                        "Expected exactly twelve Eastern Weapon prefabs; observed " +
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
                    Validate(prefab, contract);
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
                    _status = "custom:validated:12";
                    candidate = null;
                }
                if (previous != null) previous.Unload(false);
                context.Logger.Info("eastern-weapons", "bundle.loaded",
                    "Published twelve exact Eastern Weapon variants transactionally; native family animation, sockets, timing, trails, and sounds remain inherited.");
            }
            catch (Exception exception)
            {
                lock (Sync)
                {
                    Prefabs.Clear();
                    _status = "native-fallback:bundle-rejected:" +
                        exception.GetType().Name;
                }
                context.Logger.Failure("eastern-weapons", "bundle.load-failed",
                    "Dedicated Eastern Weapon presentation was rejected; native family donors remain active.",
                    exception);
            }
            finally { if (candidate != null) candidate.Unload(false); }
        }

        internal static bool ApplyTo(BlueprintWeaponType weaponType,
            EasternWeaponFamily family)
        {
            if (weaponType == null) throw new ArgumentNullException("weaponType");
            string variant = Classic(family);
            GameObject prefab = GetPrefab(variant);
            if (prefab == null) return false;
            WeaponVisualParameters source = weaponType.VisualParameters;
            if (source == null || source.Model == null)
                return RejectTypeAssignment(weaponType, family, source,
                    new InvalidOperationException(
                        "Native family fallback presentation is unavailable."));
            try
            {
                Find(typeof(BlueprintWeaponType), "m_VisualParameters")
                    .SetValue(weaponType, CloneWithModel(source, prefab));
                if (!ReferenceEquals(weaponType.VisualParameters.Model, prefab))
                    throw new InvalidOperationException(
                        "Validated Eastern type fallback did not round-trip.");
                return true;
            }
            catch (Exception exception)
            { return RejectTypeAssignment(weaponType, family, source, exception); }
        }

        internal static bool ApplyTo(BlueprintItemWeapon item,
            string blueprintSymbol, EasternWeaponFamily family)
        {
            if (item == null) throw new ArgumentNullException("item");
            string variant = WeaponVisualVariantCatalog.Require(blueprintSymbol);
            if (!variant.StartsWith(family.ToString() + ".",
                StringComparison.Ordinal))
                throw new InvalidOperationException(blueprintSymbol +
                    " maps across its qualified Eastern family boundary.");
            GameObject prefab = GetPrefab(variant);
            if (prefab == null) return false;
            FieldInfo field = Find(item.GetType(), "m_VisualParameters");
            object original = field.GetValue(item);
            WeaponVisualParameters source = item.VisualParameters ??
                (item.Type == null ? null : item.Type.VisualParameters);
            if (source == null || source.Model == null)
                return RejectItemAssignment(item, field, original, variant,
                    new InvalidOperationException(
                        "Eastern item/type fallback presentation is unavailable."));
            try
            {
                field.SetValue(item, CloneWithModel(source, prefab));
                if (item.VisualParameters == null ||
                    !ReferenceEquals(item.VisualParameters.Model, prefab))
                    throw new InvalidOperationException(
                        "Exact Eastern item variant did not round-trip.");
                return true;
            }
            catch (Exception exception)
            { return RejectItemAssignment(item, field, original, variant, exception); }
        }

        internal static bool HasExactVisual(BlueprintItemWeapon item,
            string blueprintSymbol)
        {
            if (item == null) return false;
            GameObject prefab = GetPrefab(WeaponVisualVariantCatalog.Require(
                blueprintSymbol));
            return prefab != null && item.VisualParameters != null &&
                ReferenceEquals(item.VisualParameters.Model, prefab);
        }

        internal static bool HasApprovedVisualOrNativeFallback(
            BlueprintItemWeapon item, string blueprintSymbol)
        {
            if (item == null) return false;
            GameObject prefab = GetPrefab(WeaponVisualVariantCatalog.Require(
                blueprintSymbol));
            if (prefab != null)
                return item.VisualParameters != null &&
                    ReferenceEquals(item.VisualParameters.Model, prefab);
            return item.Type != null && ReferenceEquals(item.VisualParameters,
                item.Type.VisualParameters);
        }

        internal static GameObject InstantiatePrefab(EasternWeaponFamily family)
        { return InstantiatePrefab(Classic(family)); }

        internal static GameObject InstantiatePrefab(string variant)
        {
            GameObject prefab = GetPrefab(variant);
            return prefab == null ? null : UnityEngine.Object.Instantiate(prefab);
        }

        private static string Classic(EasternWeaponFamily family)
        {
            if (family == EasternWeaponFamily.Wakizashi)
                return WeaponVisualVariantCatalog.WakizashiClassic;
            if (family == EasternWeaponFamily.Katana)
                return WeaponVisualVariantCatalog.KatanaClassic;
            if (family == EasternWeaponFamily.Nodachi)
                return WeaponVisualVariantCatalog.NodachiClassic;
            throw new ArgumentOutOfRangeException("family", family,
                "Unknown Eastern family.");
        }

        private static GameObject GetPrefab(string variant)
        {
            GameObject prefab;
            lock (Sync) Prefabs.TryGetValue(variant, out prefab);
            return prefab;
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

        private static void Validate(GameObject prefab, Contract contract)
        {
            if (prefab == null) throw new InvalidDataException(
                contract.Variant + " prefab is null.");
            Transform root = prefab.transform;
            if (!Approximately(root.localPosition, Vector3.zero) ||
                !Approximately(root.localRotation, Quaternion.identity) ||
                !Approximately(root.localScale, Vector3.one))
                throw new InvalidDataException(contract.Variant +
                    " root transform is not identity.");
            Transform visual = root.Find("Visual");
            Transform grip = root.Find("Grip");
            Transform support = root.Find("SupportHandTarget");
            Transform tip = root.Find("Tip");
            Transform butt = root.Find("Butt");
            if (visual == null || grip == null || support == null ||
                tip == null || butt == null)
                throw new InvalidDataException(contract.Variant +
                    " semantic anchors are incomplete.");
            float length = Vector3.Distance(tip.localPosition,
                butt.localPosition);
            if (!Finite(grip.localPosition) || !Finite(support.localPosition) ||
                !Finite(tip.localPosition) || !Finite(butt.localPosition) ||
                !Approximately(grip.localPosition, Vector3.zero) ||
                Mathf.Abs(support.localPosition.z - contract.Support) > 0.002f ||
                Mathf.Abs(tip.localPosition.z - contract.Tip) > 0.002f ||
                Mathf.Abs(butt.localPosition.z - contract.Butt) > 0.002f ||
                length < contract.Minimum || length > contract.Maximum)
                throw new InvalidDataException(contract.Variant +
                    " grip/support/tip/butt geometry is implausible.");
            Renderer[] renderers = visual.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0 || renderers.Any(value => value == null ||
                !value.enabled || !value.gameObject.activeSelf ||
                value.sharedMaterials == null || value.sharedMaterials.Length == 0 ||
                value.sharedMaterials.Any(material => material == null ||
                    material.shader == null)))
                throw new InvalidDataException(contract.Variant +
                    " renderers or materials are incomplete.");
            if (prefab.GetComponentsInChildren<Camera>(true).Length != 0 ||
                prefab.GetComponentsInChildren<Light>(true).Length != 0)
                throw new InvalidDataException(contract.Variant +
                    " prefab contains a camera or light.");
        }

        private static bool RejectTypeAssignment(BlueprintWeaponType weaponType,
            EasternWeaponFamily family, WeaponVisualParameters fallback,
            Exception exception)
        {
            try
            {
                if (fallback != null) Find(typeof(BlueprintWeaponType),
                    "m_VisualParameters").SetValue(weaponType, fallback);
            }
            catch { }
            Reject(family.ToString(), exception);
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
            if (logger != null) logger.Failure("eastern-weapons",
                "model.assignment-failed",
                "Exact Eastern model assignment was rejected; the native/type fallback remains active.",
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
