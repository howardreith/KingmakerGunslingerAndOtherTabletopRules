using System;
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
        private static AssetBundle _bundle;
        private static GameObject _prefab;
        private static ModLogger _logger;
        private static string _status = "native-fallback:not-configured";

        internal static bool IsLoaded { get { lock (Sync) return _bundle != null; } }
        internal static bool HasValidatedPrefab
        { get { lock (Sync) return _prefab != null; } }
        internal static string Status { get { lock (Sync) return _status; } }

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
                if (_bundle != null && _prefab != null)
                {
                    context.Logger.Info("elven-branched-spear", "bundle.reused",
                        "The validated dedicated spear prefab is already published.");
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
                string[] names = candidate.GetAllAssetNames();
                string[] matches = names.Where(value => value.EndsWith(
                    "/elvenbranchedspear.prefab",
                    StringComparison.OrdinalIgnoreCase)).ToArray();
                if (matches.Length != 1) throw new InvalidDataException(
                    "Expected one spear prefab; observed " + matches.Length + ".");
                GameObject prefab = candidate.LoadAsset<GameObject>(matches[0]);
                Validate(prefab);
                AssetBundle previous;
                lock (Sync)
                {
                    previous = _bundle;
                    _bundle = candidate;
                    _prefab = prefab;
                    _status = "custom:validated:" + matches[0];
                    candidate = null;
                }
                if (previous != null) previous.Unload(false);
                context.Logger.Info("elven-branched-spear", "bundle.loaded",
                    "Published one validated dedicated spear prefab transactionally; native donor animation, sockets, timing, trails, and sounds remain inherited.");
            }
            catch (Exception exception)
            {
                lock (Sync) _status = "native-fallback:bundle-rejected:" +
                    exception.GetType().Name;
                context.Logger.Failure("elven-branched-spear",
                    "bundle.load-failed",
                    "Dedicated spear presentation was rejected; native Longspear fallback remains active.",
                    exception);
            }
            finally
            {
                if (candidate != null) candidate.Unload(false);
            }
        }

        internal static bool ApplyTo(BlueprintWeaponType weaponType)
        {
            if (weaponType == null) throw new ArgumentNullException("weaponType");
            GameObject prefab;
            lock (Sync) prefab = _prefab;
            if (prefab == null) return false;
            WeaponVisualParameters source = weaponType.VisualParameters;
            if (source == null || source.Model == null)
                return RejectAssignment(weaponType, source,
                    new InvalidOperationException(
                        "Native Longspear fallback presentation is unavailable."));
            try
            {
                var visual = new WeaponVisualParameters();
                foreach (FieldInfo field in typeof(WeaponVisualParameters).GetFields(Fields))
                    if (!field.IsStatic && !field.IsInitOnly)
                        field.SetValue(visual, field.GetValue(source));
                Find(typeof(WeaponVisualParameters), "m_WeaponModel")
                    .SetValue(visual, prefab);
                Find(typeof(BlueprintWeaponType), "m_VisualParameters")
                    .SetValue(weaponType, visual);
                if (!ReferenceEquals(weaponType.VisualParameters.Model, prefab))
                    throw new InvalidOperationException(
                        "Validated spear prefab assignment did not round-trip.");
                return true;
            }
            catch (Exception exception)
            {
                return RejectAssignment(weaponType, source, exception);
            }
        }

        internal static GameObject InstantiatePrefab()
        {
            GameObject prefab;
            lock (Sync) prefab = _prefab;
            return prefab == null ? null : UnityEngine.Object.Instantiate(prefab);
        }

        private static void Validate(GameObject prefab)
        {
            if (prefab == null) throw new InvalidDataException("Spear prefab is null.");
            Transform root = prefab.transform;
            if (!Approximately(root.localPosition, Vector3.zero) ||
                !Approximately(root.localRotation, Quaternion.identity) ||
                !Approximately(root.localScale, Vector3.one))
                throw new InvalidDataException("Spear root transform is not identity.");
            Transform visual = root.Find("Visual");
            Transform grip = root.Find("Grip");
            Transform support = root.Find("SupportHandTarget");
            Transform tip = root.Find("Tip");
            Transform butt = root.Find("Butt");
            if (visual == null || grip == null || support == null || tip == null ||
                butt == null) throw new InvalidDataException(
                    "Spear semantic anchors are incomplete.");
            if (!Finite(grip.localPosition) || !Finite(support.localPosition) ||
                !Finite(tip.localPosition) || !Finite(butt.localPosition) ||
                support.localPosition.z <= 0f || tip.localPosition.z <=
                    support.localPosition.z || butt.localPosition.z >= 0f ||
                Vector3.Distance(tip.localPosition, butt.localPosition) < 2.5f)
                throw new InvalidDataException(
                    "Spear grip/support/tip/butt geometry is implausible.");
            Renderer[] renderers = visual.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0 || renderers.Any(value => value == null ||
                !value.enabled || !value.gameObject.activeSelf ||
                value.sharedMaterials == null || value.sharedMaterials.Length == 0 ||
                value.sharedMaterials.Any(material => material == null ||
                    material.shader == null)))
                throw new InvalidDataException(
                    "Spear renderers or materials are incomplete.");
            if (prefab.GetComponentsInChildren<Camera>(true).Length != 0 ||
                prefab.GetComponentsInChildren<Light>(true).Length != 0)
                throw new InvalidDataException(
                    "Spear prefab contains a camera or light.");
        }

        private static bool RejectAssignment(BlueprintWeaponType weaponType,
            WeaponVisualParameters fallback, Exception exception)
        {
            try
            {
                if (fallback != null)
                    Find(typeof(BlueprintWeaponType), "m_VisualParameters")
                        .SetValue(weaponType, fallback);
            }
            catch
            {
                // The caller's donor clone still owns its original visual parameters.
            }
            ModLogger logger;
            lock (Sync)
            {
                _prefab = null;
                _status = "native-fallback:model-assignment-rejected:" +
                    exception.GetType().Name;
                logger = _logger;
            }
            if (logger != null) logger.Failure("elven-branched-spear",
                "model.assignment-failed",
                "Custom model assignment was rejected; native Longspear fallback remains active.",
                exception);
            return false;
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
