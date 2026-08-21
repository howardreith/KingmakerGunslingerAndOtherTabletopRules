using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Firearms;
using Kingmaker.View.Equipment;
using UnityEngine;

namespace KingmakerGunslinger.Assets
{
    internal static class FirearmAssetRuntime
    {
        private static readonly object Sync = new object();
        private static AssetBundle _bundle;
        private static readonly Dictionary<FirearmKind, GameObject> Prefabs = new Dictionary<FirearmKind, GameObject>();
        private static readonly Dictionary<FirearmKind, GameObject> BeltPrefabs = new Dictionary<FirearmKind, GameObject>();
        private static readonly Dictionary<FirearmKind, FirearmRigCapability>
            Capabilities = new Dictionary<FirearmKind, FirearmRigCapability>();
        private static readonly Dictionary<string, GameObject> DiagnosticPrefabs =
            new Dictionary<string, GameObject>(StringComparer.Ordinal);
        private static readonly Dictionary<string, FirearmRigCapability>
            DiagnosticCapabilities = new Dictionary<string, FirearmRigCapability>(
                StringComparer.Ordinal);
        private static readonly Dictionary<string, GameObject> ItemVariantPrefabs =
            new Dictionary<string, GameObject>(StringComparer.Ordinal);
        private static readonly Dictionary<string, FirearmRigCapability>
            ItemVariantCapabilities = new Dictionary<string, FirearmRigCapability>(
                StringComparer.Ordinal);
        internal static bool IsLoaded { get { lock (Sync) return _bundle != null; } }

        internal static void Configure(ModContext context)
        {
            if (context == null) throw new ArgumentNullException("context");
            string path = Path.Combine(context.ModEntry.Path, "assets", "bundles",
                "kingmakergunslinger.firearms");
            if (!File.Exists(path))
            {
                context.Logger.Warning("assets", "bundle.missing",
                    "Firearm bundle unavailable; cloned native weapon models remain active: " + path);
                return;
            }

            AssetBundle candidate = null;
            try
            {
                candidate = AssetBundle.LoadFromFile(path);
                if (candidate == null)
                    throw new InvalidDataException("Unity rejected the firearm bundle.");
                string[] names = candidate.GetAllAssetNames();
                var prefabs = new Dictionary<FirearmKind, GameObject>();
                var beltPrefabs = new Dictionary<FirearmKind, GameObject>();
                var capabilities = new Dictionary<FirearmKind,
                    FirearmRigCapability>();
                var diagnosticPrefabs = new Dictionary<string, GameObject>(
                    StringComparer.Ordinal);
                var diagnosticCapabilities = new Dictionary<string,
                    FirearmRigCapability>(StringComparer.Ordinal);
                var itemVariantPrefabs = new Dictionary<string, GameObject>(
                    StringComparer.Ordinal);
                var itemVariantCapabilities = new Dictionary<string,
                    FirearmRigCapability>(StringComparer.Ordinal);

                TryLoadEquippedPrefab(candidate, names, prefabs, capabilities,
                    FirearmKind.Pistol, "pistol", false, context);
                TryLoadEquippedPrefab(candidate, names, prefabs, capabilities,
                    FirearmKind.Musket, "musket", true, context);
                TryLoadEquippedPrefab(candidate, names, prefabs, capabilities,
                    FirearmKind.Blunderbuss, "blunderbuss", true, context);
                TryLoadEquippedPrefab(candidate, names, prefabs, capabilities,
                    FirearmKind.Revolver, "revolver", false, context);
                TryLoadEquippedPrefab(candidate, names, prefabs, capabilities,
                    FirearmKind.Rifle, "rifle", true, context);
                TryLoadPrefab(candidate, names, beltPrefabs, FirearmKind.Pistol,
                    "pistolbelt", context);
                TryLoadBackPrefab(candidate, names, beltPrefabs,
                    FirearmKind.Musket, "musketbelt", context);
                TryLoadBackPrefab(candidate, names, beltPrefabs,
                    FirearmKind.Blunderbuss, "blunderbussbelt", context);
                ValidateIndependentHeldAndStored(prefabs, beltPrefabs,
                    FirearmKind.Musket);
                ValidateIndependentHeldAndStored(prefabs, beltPrefabs,
                    FirearmKind.Blunderbuss);
                TryLoadDiagnosticPrefab(candidate, names, diagnosticPrefabs,
                    diagnosticCapabilities, "MusketPassThrough",
                    "musketpassthrough", context);
                TryLoadDiagnosticPrefab(candidate, names, diagnosticPrefabs,
                    diagnosticCapabilities, "MusketMinimalControl",
                    "musketminimalcontrol", context);
                TryLoadDiagnosticPrefab(candidate, names, diagnosticPrefabs,
                    diagnosticCapabilities, "MusketClearanceStock",
                    "musketclearancestock", context);
                TryLoadItemVariantPrefab(candidate, names, itemVariantPrefabs,
                    itemVariantCapabilities, WeaponVisualVariantCatalog.PistolDuelist,
                    FirearmKind.Pistol, "pistolduelist", false, context);
                TryLoadItemVariantPrefab(candidate, names, itemVariantPrefabs,
                    itemVariantCapabilities, WeaponVisualVariantCatalog.PistolLastWord,
                    FirearmKind.Pistol, "pistollastword", false, context);
                PublishServiceVariants(prefabs, capabilities, itemVariantPrefabs,
                    itemVariantCapabilities);

                AssetBundle previous;
                lock (Sync)
                {
                    previous = _bundle;
                    _bundle = candidate;
                    candidate = null;
                    Replace(Prefabs, prefabs);
                    Replace(BeltPrefabs, beltPrefabs);
                    Replace(Capabilities, capabilities);
                    Replace(DiagnosticPrefabs, diagnosticPrefabs);
                    Replace(DiagnosticCapabilities, diagnosticCapabilities);
                    Replace(ItemVariantPrefabs, itemVariantPrefabs);
                    Replace(ItemVariantCapabilities, itemVariantCapabilities);
                }
                if (previous != null) previous.Unload(false);
                context.Logger.Info("assets", "bundle.loaded",
                    "Published firearm bundle transactionally; equippedPrefabs=" +
                    prefabs.Count + ";beltPrefabs=" + beltPrefabs.Count +
                    ";diagnosticPrefabs=" + diagnosticPrefabs.Count +
                    ";itemVariantPrefabs=" + itemVariantPrefabs.Count +
                    ". Missing or rejected capabilities retain native presentation fallbacks.");
            }
            catch (Exception exception)
            {
                context.Logger.Failure("assets", "bundle.load-failed",
                    "Firearm bundle was not published; cloned native weapon models remain active.",
                    exception);
            }
            finally
            {
                if (candidate != null) candidate.Unload(false);
            }
        }

        private static void TryLoadEquippedPrefab(AssetBundle bundle,
            string[] names, IDictionary<FirearmKind, GameObject> destination,
            IDictionary<FirearmKind, FirearmRigCapability> capabilities,
            FirearmKind kind, string name, bool requiresTwoHandRig,
            ModContext context)
        {
            GameObject prefab = TryLoadPrefab(bundle, names, kind, name, context);
            if (prefab == null) return;
            FirearmRigCapability capability;
            if (!TryPrepareRig(prefab, kind, requiresTwoHandRig, out capability))
            {
                capabilities[kind] = capability;
                context.Logger.Warning("assets", "rig.rejected",
                    capability.Describe() + ";nativeFallback=true");
                return;
            }
            destination[kind] = prefab;
            capabilities[kind] = capability;
            context.Logger.Info("assets", "rig.validated", capability.Describe());
        }

        private static void TryLoadBackPrefab(AssetBundle bundle,
            string[] names, IDictionary<FirearmKind, GameObject> destination,
            FirearmKind kind, string name, ModContext context)
        {
            GameObject prefab = TryLoadPrefab(bundle, names, kind, name, context);
            Transform back = prefab == null ? null : prefab.transform.Find("BackMount");
            Transform visual = prefab == null ? null : prefab.transform.Find("Visual");
            Renderer[] renderers = prefab == null
                ? new Renderer[0] : prefab.GetComponentsInChildren<Renderer>(true);
            bool valid = prefab != null && back != null && visual != null &&
                renderers.Length > 0 && Finite(back.localPosition) &&
                prefab.transform.localPosition == Vector3.zero &&
                prefab.transform.localRotation == Quaternion.identity &&
                prefab.transform.localScale == Vector3.one;
            string semanticFailure = null;
            if (valid)
            {
                try
                {
                    WeaponPresentationSemanticFrame frame =
                        WeaponPresentationFrameContract.RequireWithForwardMarker(
                            prefab.transform, prefab.name, "Muzzle",
                            WeaponPresentationFrameContract.WeaponUpMarker,
                            WeaponPresentationFrameContract.WeaponForwardMarker,
                            false, 0.40f, 2.00f);
                    WeaponPresentationFrameContract.ValidateRendererEndpoints(
                        prefab.transform, visual, frame, prefab.name, 0.25f);
                }
                catch (Exception exception)
                {
                    valid = false;
                    semanticFailure = exception.GetType().Name + ":" +
                        exception.Message;
                }
            }
            if (!valid)
            {
                context.Logger.Warning("assets", "back-rig.rejected",
                    "kind=" + kind + ";asset=" + name +
                    ";requires=identity-root+Visual+BackMount+renderer+semantic-frame" +
                    ";failure=" + (semanticFailure ?? "structural") +
                    ";nativeFallback=true");
                return;
            }
            destination[kind] = prefab;
            context.Logger.Info("assets", "back-rig.validated",
                "kind=" + kind + ";asset=" + name +
                ";independentBackFrame=true;rendererCount=" + renderers.Length);
        }

        private static void ValidateIndependentHeldAndStored(
            IDictionary<FirearmKind, GameObject> held,
            IDictionary<FirearmKind, GameObject> stored, FirearmKind kind)
        {
            GameObject heldPrefab;
            GameObject storedPrefab;
            if (!held.TryGetValue(kind, out heldPrefab) || heldPrefab == null ||
                !stored.TryGetValue(kind, out storedPrefab) ||
                storedPrefab == null)
                return;
            Transform heldVisual = heldPrefab.transform.Find("Visual");
            Transform storedVisual = storedPrefab.transform.Find("Visual");
            if (ReferenceEquals(heldPrefab, storedPrefab) ||
                heldVisual == null || storedVisual == null ||
                (Approximately(heldVisual.localPosition,
                    storedVisual.localPosition) &&
                 Approximately(heldVisual.localRotation,
                    storedVisual.localRotation) &&
                 Approximately(heldVisual.localScale,
                    storedVisual.localScale)))
                throw new InvalidDataException(kind +
                    " held and stored presentations share an incompatible transform.");
        }

        private static void TryLoadDiagnosticPrefab(AssetBundle bundle,
            string[] names, IDictionary<string, GameObject> destination,
            IDictionary<string, FirearmRigCapability> capabilities,
            string identity, string assetName, ModContext context)
        {
            GameObject prefab = TryLoadPrefab(bundle, names, FirearmKind.Musket,
                assetName, context);
            if (prefab == null) return;
            FirearmRigCapability capability;
            if (!TryPrepareRig(prefab, FirearmKind.Musket, true, out capability))
            {
                capabilities[identity] = capability;
                context.Logger.Warning("assets", "diagnostic-rig.rejected",
                    "identity=" + identity + ";" + capability.Describe());
                return;
            }
            destination[identity] = prefab;
            capabilities[identity] = capability;
            context.Logger.Info("assets", "diagnostic-rig.validated",
                "identity=" + identity + ";productionBinding=false;" +
                capability.Describe());
        }

        private static void TryLoadItemVariantPrefab(AssetBundle bundle,
            string[] names, IDictionary<string, GameObject> destination,
            IDictionary<string, FirearmRigCapability> capabilities,
            string variant, FirearmKind kind, string assetName,
            bool requiresTwoHandRig, ModContext context)
        {
            GameObject prefab = TryLoadPrefab(bundle, names, kind, assetName,
                context);
            if (prefab == null) return;
            FirearmRigCapability capability;
            if (!TryPrepareRig(prefab, kind, requiresTwoHandRig, out capability))
            {
                capabilities[variant] = capability;
                context.Logger.Warning("assets", "item-variant-rig.rejected",
                    "variant=" + variant + ";" + capability.Describe());
                return;
            }
            destination[variant] = prefab;
            capabilities[variant] = capability;
            context.Logger.Info("assets", "item-variant-rig.validated",
                "variant=" + variant + ";" + capability.Describe());
        }

        private static void PublishServiceVariants(
            IDictionary<FirearmKind, GameObject> prefabs,
            IDictionary<FirearmKind, FirearmRigCapability> capabilities,
            IDictionary<string, GameObject> destination,
            IDictionary<string, FirearmRigCapability> variantCapabilities)
        {
            PublishServiceVariant(WeaponVisualVariantCatalog.PistolService,
                FirearmKind.Pistol, prefabs, capabilities, destination,
                variantCapabilities);
            PublishServiceVariant(WeaponVisualVariantCatalog.MusketService,
                FirearmKind.Musket, prefabs, capabilities, destination,
                variantCapabilities);
            PublishServiceVariant(WeaponVisualVariantCatalog.BlunderbussService,
                FirearmKind.Blunderbuss, prefabs, capabilities, destination,
                variantCapabilities);
            PublishServiceVariant(WeaponVisualVariantCatalog.RifleService,
                FirearmKind.Rifle, prefabs, capabilities, destination,
                variantCapabilities);
            PublishServiceVariant(WeaponVisualVariantCatalog.RevolverService,
                FirearmKind.Revolver, prefabs, capabilities, destination,
                variantCapabilities);
        }

        private static void PublishServiceVariant(string variant,
            FirearmKind kind, IDictionary<FirearmKind, GameObject> prefabs,
            IDictionary<FirearmKind, FirearmRigCapability> capabilities,
            IDictionary<string, GameObject> destination,
            IDictionary<string, FirearmRigCapability> variantCapabilities)
        {
            GameObject prefab;
            FirearmRigCapability capability;
            if (prefabs.TryGetValue(kind, out prefab) && prefab != null &&
                capabilities.TryGetValue(kind, out capability) &&
                capability.IsValidated)
            {
                destination[variant] = prefab;
                variantCapabilities[variant] = capability;
            }
        }

        private static void TryLoadPrefab(AssetBundle bundle, string[] names,
            IDictionary<FirearmKind, GameObject> destination,
            FirearmKind kind, string name, ModContext context)
        {
            GameObject prefab = TryLoadPrefab(bundle, names, kind, name, context);
            if (prefab != null) destination[kind] = prefab;
        }

        private static GameObject TryLoadPrefab(AssetBundle bundle,
            string[] names, FirearmKind kind, string name, ModContext context)
        {
            string suffix = "/" + name + ".prefab";
            string[] matches = names.Where(value => value.EndsWith(
                suffix, StringComparison.OrdinalIgnoreCase)).ToArray();
            if (matches.Length != 1)
            {
                context.Logger.Warning("assets", "prefab.skipped",
                    "kind=" + kind + ";name=" + name +
                    ";matches=" + matches.Length + ";nativeFallback=true");
                return null;
            }
            GameObject prefab = bundle.LoadAsset<GameObject>(matches[0]);
            Renderer[] renderers = prefab == null
                ? Array.Empty<Renderer>()
                : prefab.GetComponentsInChildren<Renderer>(true);
            bool renderable = renderers.Any(renderer => renderer != null &&
                renderer.sharedMaterials != null &&
                renderer.sharedMaterials.Any(material => material != null &&
                    material.shader != null));
            if (!renderable)
            {
                context.Logger.Warning("assets", "prefab.skipped",
                    "kind=" + kind + ";name=" + name +
                    ";renderable=false;nativeFallback=true");
                return null;
            }
            return prefab;
        }

        private static bool TryPrepareRig(GameObject prefab, FirearmKind kind,
            bool requiresTwoHandRig, out FirearmRigCapability capability)
        {
            string failure = null;
            Transform root = prefab == null ? null : prefab.transform;
            Transform visual = root == null ? null : root.Find("Visual");
            Transform grip = root == null ? null : root.Find(
                WeaponPresentationFrameContract.GripMarker);
            Transform muzzle = root == null ? null : root.Find("Muzzle");
            Transform butt = root == null ? null : root.Find("Butt");
            Transform weaponUp = root == null ? null : root.Find(
                WeaponPresentationFrameContract.WeaponUpMarker);
            Transform weaponForward = root == null ? null : root.Find(
                WeaponPresentationFrameContract.WeaponForwardMarker);
            Transform support = root == null ? null : root.Find(
                "SupportHandTarget");
            EquipmentOffsets offsets = null;
            try
            {
                if (root == null) failure = "prefab-null";
                else if (!Approximately(root.localPosition, Vector3.zero) ||
                    !Approximately(root.localRotation, Quaternion.identity) ||
                    !Approximately(root.localScale, Vector3.one))
                    failure = "root-not-identity";
                else if (visual == null) failure = "visual-missing";
                else if (grip == null) failure = "grip-missing";
                else if (muzzle == null) failure = "muzzle-missing";
                else if (butt == null) failure = "butt-target-missing";
                else if (weaponUp == null) failure = "weapon-up-missing";
                else if (weaponForward == null)
                    failure = "weapon-forward-missing";
                else if (!Finite(visual.localPosition) ||
                    !Finite(visual.localRotation) || !Finite(visual.localScale) ||
                    !Finite(muzzle.localPosition) || !Finite(muzzle.localRotation))
                    failure = "transform-nonfinite";
                else if (muzzle.localPosition.z <= 0f)
                    failure = "muzzle-not-forward-positive-z";
                else if (requiresTwoHandRig && support == null)
                    failure = "support-target-missing";
                else if (!requiresTwoHandRig && support != null)
                    failure = "one-handed-support-target-present";
                else if (requiresTwoHandRig && (!Finite(support.localPosition) ||
                    !Finite(support.localRotation) ||
                    support.localPosition.z <= 0f ||
                    support.localPosition.z >= muzzle.localPosition.z))
                    failure = "support-target-implausible";
                else if (requiresTwoHandRig && (!Finite(butt.localPosition) ||
                    butt.localPosition.z >= 0f ||
                    Vector3.Distance(butt.localPosition, muzzle.localPosition) < 0.4f))
                    failure = "semantic-length-or-butt-implausible";
                else if (prefab.GetComponentsInChildren<Camera>(true).Length != 0 ||
                    prefab.GetComponentsInChildren<Light>(true).Length != 0)
                    failure = "camera-or-light-present";
                else if (prefab.GetComponentsInChildren<LODGroup>(true).Length != 0)
                    failure = "lod-group-present";
                else if (prefab.GetComponentsInChildren<Transform>(true).Any(
                    child => !Finite(child.localScale) || child.localScale.x <= 0f ||
                        child.localScale.y <= 0f || child.localScale.z <= 0f))
                    failure = "negative-mirrored-zero-or-nonfinite-scale";
                else
                {
                    float minimumLength = requiresTwoHandRig ? 0.40f : 0.15f;
                    float maximumLength = requiresTwoHandRig ? 2.00f : 0.60f;
                    WeaponPresentationSemanticFrame frame =
                        WeaponPresentationFrameContract.RequireWithForwardMarker(
                            root,
                            prefab.name, "Muzzle",
                            WeaponPresentationFrameContract.WeaponUpMarker,
                            WeaponPresentationFrameContract.WeaponForwardMarker,
                            requiresTwoHandRig, minimumLength, maximumLength);
                    WeaponPresentationFrameContract.ValidateRendererEndpoints(
                        root, visual, frame, prefab.name, 0.25f);
                    Renderer[] renderers = prefab.GetComponentsInChildren<Renderer>(
                        true);
                    bool renderable = renderers.Any(renderer => renderer != null &&
                        renderer.enabled && renderer.gameObject.activeSelf &&
                        renderer.sharedMaterials != null &&
                        renderer.sharedMaterials.Length > 0 &&
                        renderer.sharedMaterials.All(material => material != null &&
                            material.shader != null));
                    if (!renderable) failure = "renderable-materials-missing";
                    else if (renderers.Any(renderer => !renderer.enabled ||
                        !renderer.gameObject.activeSelf))
                        failure = "renderer-disabled-or-inactive";
                    else if (renderers.Any(renderer =>
                        renderer.sharedMaterials.Any(material =>
                            material.shader.name != "Standard")))
                        failure = "non-opaque-standard-shader";
                }
                if (failure == null && requiresTwoHandRig)
                {
                    offsets = prefab.GetComponent<EquipmentOffsets>();
                    if (offsets == null) offsets = prefab.AddComponent<EquipmentOffsets>();
                    offsets.IkTargetLeftHand = support;
                    if (!ReferenceEquals(offsets.IkTargetLeftHand, support))
                        failure = "left-hand-ik-assignment-failed";
                }
            }
            catch (Exception exception)
            {
                failure = exception.GetType().Name + ":" + exception.Message;
            }
            capability = new FirearmRigCapability(kind, failure == null,
                requiresTwoHandRig, prefab == null ? null : prefab.name,
                visual == null ? null : visual.name,
                visual == null ? (Vector3?)null : visual.localPosition,
                visual == null ? (Vector3?)null : visual.localEulerAngles,
                visual == null ? (float?)null : visual.localScale.x,
                muzzle == null ? (Vector3?)null : muzzle.localPosition,
                support == null ? null : (Vector3?)support.localPosition,
                butt == null ? null : (Vector3?)butt.localPosition,
                offsets != null && ReferenceEquals(offsets.IkTargetLeftHand, support),
                failure);
            return capability.IsValidated;
        }

        private static bool Approximately(Vector3 left, Vector3 right)
        {
            return (left - right).sqrMagnitude <= 0.000001f;
        }

        private static bool Approximately(Quaternion left, Quaternion right)
        {
            return Mathf.Abs(Quaternion.Dot(left, right)) >= 0.999999f;
        }

        private static bool Finite(Vector3 value)
        {
            return Finite(value.x) && Finite(value.y) && Finite(value.z);
        }

        private static bool Finite(Quaternion value)
        {
            return Finite(value.x) && Finite(value.y) && Finite(value.z) &&
                Finite(value.w);
        }

        private static bool Finite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }

        private static void Replace<T>(IDictionary<FirearmKind, T> destination,
            IDictionary<FirearmKind, T> source)
        {
            destination.Clear();
            foreach (KeyValuePair<FirearmKind, T> entry in source)
                destination[entry.Key] = entry.Value;
        }
        private static void Replace<T>(IDictionary<string, T> destination,
            IDictionary<string, T> source)
        {
            destination.Clear();
            foreach (KeyValuePair<string, T> entry in source)
                destination[entry.Key] = entry.Value;
        }
        internal static GameObject InstantiatePrefab(FirearmKind kind)
        {
            lock (Sync) { GameObject prefab; return Prefabs.TryGetValue(kind, out prefab) && prefab != null ? UnityEngine.Object.Instantiate(prefab) : null; }
        }
        internal static GameObject GetPrefab(FirearmKind kind)
        {
            lock (Sync)
            {
                GameObject prefab;
                return Prefabs.TryGetValue(kind, out prefab) ? prefab : null;
            }
        }
        internal static FirearmRigCapability GetCapability(FirearmKind kind)
        {
            lock (Sync)
            {
                FirearmRigCapability capability;
                return Capabilities.TryGetValue(kind, out capability)
                    ? capability : FirearmRigCapability.Missing(kind);
            }
        }
        internal static bool HasValidatedPrefab(FirearmKind kind)
        {
            lock (Sync)
            {
                FirearmRigCapability capability;
                GameObject prefab;
                return Capabilities.TryGetValue(kind, out capability) &&
                    capability.IsValidated && Prefabs.TryGetValue(kind, out prefab) &&
                    prefab != null;
            }
        }
        internal static GameObject GetBeltPrefab(FirearmKind kind)
        {
            lock (Sync)
            {
                GameObject prefab;
                return BeltPrefabs.TryGetValue(kind, out prefab) ? prefab : null;
            }
        }
        internal static GameObject InstantiateDiagnosticPrefab(string identity)
        {
            lock (Sync)
            {
                GameObject prefab;
                return identity != null && DiagnosticPrefabs.TryGetValue(identity,
                    out prefab) && prefab != null
                    ? UnityEngine.Object.Instantiate(prefab) : null;
            }
        }
        internal static GameObject GetDiagnosticPrefab(string identity)
        {
            lock (Sync)
            {
                GameObject prefab;
                return identity != null && DiagnosticPrefabs.TryGetValue(identity,
                    out prefab) ? prefab : null;
            }
        }
        internal static FirearmRigCapability GetDiagnosticCapability(string identity)
        {
            lock (Sync)
            {
                FirearmRigCapability capability;
                return identity != null && DiagnosticCapabilities.TryGetValue(
                    identity, out capability)
                    ? capability : FirearmRigCapability.Missing(FirearmKind.Musket);
            }
        }
        internal static bool HasValidatedDiagnosticPrefab(string identity)
        {
            lock (Sync)
            {
                FirearmRigCapability capability;
                GameObject prefab;
                return identity != null && DiagnosticCapabilities.TryGetValue(
                    identity, out capability) && capability.IsValidated &&
                    DiagnosticPrefabs.TryGetValue(identity, out prefab) &&
                    prefab != null;
            }
        }
        internal static GameObject GetItemVariantPrefab(string variant)
        {
            lock (Sync)
            {
                GameObject prefab;
                return variant != null && ItemVariantPrefabs.TryGetValue(variant,
                    out prefab) ? prefab : null;
            }
        }
        internal static GameObject InstantiateItemVariantPrefab(string variant)
        {
            GameObject prefab = GetItemVariantPrefab(variant);
            return prefab == null ? null : UnityEngine.Object.Instantiate(prefab);
        }
        internal static FirearmRigCapability GetItemVariantCapability(
            string variant)
        {
            lock (Sync)
            {
                FirearmRigCapability capability;
                return variant != null && ItemVariantCapabilities.TryGetValue(
                    variant, out capability) ? capability :
                    FirearmRigCapability.Missing(FirearmKind.Pistol);
            }
        }
        internal static bool HasValidatedItemVariant(string variant)
        {
            lock (Sync)
            {
                FirearmRigCapability capability;
                GameObject prefab;
                return variant != null && ItemVariantCapabilities.TryGetValue(
                    variant, out capability) && capability.IsValidated &&
                    ItemVariantPrefabs.TryGetValue(variant, out prefab) &&
                    prefab != null;
            }
        }
    }

    internal sealed class FirearmRigCapability
    {
        internal FirearmRigCapability(FirearmKind kind, bool isValidated,
            bool requiresTwoHandRig, string prefabName, string visualName,
            Vector3? visualPosition, Vector3? visualEuler, float? visualScale,
            Vector3? muzzlePosition, Vector3? supportPosition,
            Vector3? buttPosition, bool leftHandIkAssigned, string failure)
        {
            Kind = kind;
            IsValidated = isValidated;
            RequiresTwoHandRig = requiresTwoHandRig;
            PrefabName = prefabName;
            VisualName = visualName;
            VisualPosition = visualPosition;
            VisualEuler = visualEuler;
            VisualScale = visualScale;
            MuzzlePosition = muzzlePosition;
            SupportPosition = supportPosition;
            ButtPosition = buttPosition;
            LeftHandIkAssigned = leftHandIkAssigned;
            Failure = failure;
        }
        internal FirearmKind Kind { get; private set; }
        internal bool IsValidated { get; private set; }
        internal bool RequiresTwoHandRig { get; private set; }
        internal string PrefabName { get; private set; }
        internal string VisualName { get; private set; }
        internal Vector3? VisualPosition { get; private set; }
        internal Vector3? VisualEuler { get; private set; }
        internal float? VisualScale { get; private set; }
        internal Vector3? MuzzlePosition { get; private set; }
        internal Vector3? SupportPosition { get; private set; }
        internal Vector3? ButtPosition { get; private set; }
        internal bool LeftHandIkAssigned { get; private set; }
        internal string Failure { get; private set; }
        internal static FirearmRigCapability Missing(FirearmKind kind)
        {
            return new FirearmRigCapability(kind, false, false, null, null,
                null, null, null, null, null, null, false,
                "capability-missing");
        }
        internal string Describe()
        {
            return "kind=" + Kind + ";validated=" + IsValidated +
                ";prefab=" + (PrefabName ?? "<null>") +
                ";visual=" + (VisualName ?? "<null>") +
                ";visualPosition=" + (VisualPosition.HasValue
                    ? VisualPosition.Value.ToString("R") : "<null>") +
                ";visualEuler=" + (VisualEuler.HasValue
                    ? VisualEuler.Value.ToString("R") : "<null>") +
                ";visualScale=" + (VisualScale.HasValue
                    ? VisualScale.Value.ToString("R") : "<null>") +
                ";twoHand=" + RequiresTwoHandRig +
                ";muzzle=" + (MuzzlePosition.HasValue
                    ? MuzzlePosition.Value.ToString("R") : "<null>") +
                ";support=" + (SupportPosition.HasValue
                    ? SupportPosition.Value.ToString("R") : "<null>") +
                ";butt=" + (ButtPosition.HasValue
                    ? ButtPosition.Value.ToString("R") : "<null>") +
                ";semanticLength=" + (ButtPosition.HasValue && MuzzlePosition.HasValue
                    ? Vector3.Distance(ButtPosition.Value,
                        MuzzlePosition.Value).ToString("R") : "<null>") +
                ";ikLeft=" + LeftHandIkAssigned +
                ";failure=" + (Failure ?? "<none>");
        }
    }
}
