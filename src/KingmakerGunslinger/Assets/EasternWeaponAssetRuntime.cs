using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.View.Equipment;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.EasternWeapons;
using UnityEngine;

namespace KingmakerGunslinger.Assets
{
    internal static class EasternWeaponAssetRuntime
    {
        internal const string BundleName =
            "kingmakergunslinger.easternweapons";
        private const string CuttingEdgeMarker = "CuttingEdge";
        private const string StoredMountMarker = "StoredMount";
        private const float NodachiSupportStation = -0.169f;
        private const BindingFlags Fields = BindingFlags.Instance |
            BindingFlags.Public | BindingFlags.NonPublic;
        private static readonly object Sync = new object();
        private static readonly Dictionary<string, GameObject> Prefabs =
            new Dictionary<string, GameObject>(StringComparer.Ordinal);
        private static readonly Dictionary<string, GameObject> StoredPrefabs =
            new Dictionary<string, GameObject>(StringComparer.Ordinal);
        private static AssetBundle _bundle;
        private static ModLogger _logger;
        private static string _status = "native-fallback:not-configured";

        private sealed class DonorFrame
        {
            internal DonorFrame(string name, Vector3 heldPosition,
                Vector3 heldEuler, Vector3 storedPosition,
                Vector3 storedEuler, Vector3 storedScale,
                Vector3 storedRendererCenter)
            {
                Name = name;
                HeldPosition = heldPosition;
                HeldEuler = heldEuler;
                StoredPosition = storedPosition;
                StoredEuler = storedEuler;
                StoredScale = storedScale;
                StoredRendererCenter = storedRendererCenter;
            }

            internal string Name;
            internal Vector3 HeldPosition;
            internal Vector3 HeldEuler;
            internal Vector3 StoredPosition;
            internal Vector3 StoredEuler;
            internal Vector3 StoredScale;
            internal Vector3 StoredRendererCenter;
        }

        private sealed class Contract
        {
            internal Contract(EasternWeaponFamily family, string variant,
                string assetName, string storedAssetName, float minimum,
                float maximum, bool requiresSupport, DonorFrame donor)
            {
                Family = family;
                Variant = variant;
                AssetName = assetName;
                StoredAssetName = storedAssetName;
                Minimum = minimum;
                Maximum = maximum;
                RequiresSupport = requiresSupport;
                Donor = donor;
            }

            internal EasternWeaponFamily Family;
            internal string Variant;
            internal string AssetName;
            internal string StoredAssetName;
            internal float Minimum;
            internal float Maximum;
            internal bool RequiresSupport;
            internal DonorFrame Donor;
        }

        private static readonly DonorFrame Scimitar = new DonorFrame(
            "native-Scimitar",
            new Vector3(-0.008432996f, -0.020028824f, 0.00105414f),
            new Vector3(353.967743f, 291.8186f, 184.377609f),
            new Vector3(-0.06271917f, -0.0416369066f, -0.118480414f),
            new Vector3(4.08384275f, 289.105225f, 289.6144f),
            new Vector3(1.05412471f, 1.05414915f, 1.05414057f),
            new Vector3(-0.000206507742f, 0.2642012f, -0.0341803059f));

        private static readonly DonorFrame BastardSword = new DonorFrame(
            "native-BastardSword", Vector3.zero,
            new Vector3(12.3281841f, 123.021339f, 178.818527f),
            new Vector3(0f, 0.00399958529f, -0.184981823f),
            new Vector3(356.3357f, 270f, 265.4645f),
            new Vector3(1.00008607f, 1.0000869f, 1.0000515f),
            new Vector3(0.0000006817281f, 0.462136239f,
                -0.000000476837158f));

        private static readonly DonorFrame Greatsword = new DonorFrame(
            "native-Greatsword", Vector3.zero,
            new Vector3(5.23646832f, 124.490906f, 179.792526f),
            new Vector3(0f, 0.0110000232f, -0.221000314f),
            new Vector3(353.2058f, 270.347778f, 267.0627f),
            new Vector3(1.00000072f, 1.00000215f, 1.00000143f),
            new Vector3(0.00370623916f, 0.5212074f, 0.004406467f));

        private static readonly Contract[] Contracts =
        {
            C(EasternWeaponFamily.Wakizashi,
                WeaponVisualVariantCatalog.WakizashiClassic,
                "wakizashi.prefab", "wakizashistored.prefab"),
            C(EasternWeaponFamily.Wakizashi,
                WeaponVisualVariantCatalog.WakizashiPetal,
                "wakizashipetal.prefab", "wakizashipetalstored.prefab"),
            C(EasternWeaponFamily.Wakizashi,
                WeaponVisualVariantCatalog.WakizashiMoon,
                "wakizashimoon.prefab", "wakizashimoonstored.prefab"),
            C(EasternWeaponFamily.Wakizashi,
                WeaponVisualVariantCatalog.WakizashiCapstone,
                "wakizashicapstone.prefab",
                "wakizashicapstonestored.prefab"),
            C(EasternWeaponFamily.Katana,
                WeaponVisualVariantCatalog.KatanaClassic,
                "katana.prefab", "katanastored.prefab"),
            C(EasternWeaponFamily.Katana,
                WeaponVisualVariantCatalog.KatanaReed,
                "katanareed.prefab", "katanareedstored.prefab"),
            C(EasternWeaponFamily.Katana,
                WeaponVisualVariantCatalog.KatanaRegal,
                "katanaregal.prefab", "katanaregalstored.prefab"),
            C(EasternWeaponFamily.Katana,
                WeaponVisualVariantCatalog.KatanaCapstone,
                "katanacapstone.prefab", "katanacapstonestored.prefab"),
            C(EasternWeaponFamily.Nodachi,
                WeaponVisualVariantCatalog.NodachiClassic,
                "nodachi.prefab", "nodachistored.prefab"),
            C(EasternWeaponFamily.Nodachi,
                WeaponVisualVariantCatalog.NodachiCleaver,
                "nodachicleaver.prefab", "nodachicleaverstored.prefab"),
            C(EasternWeaponFamily.Nodachi,
                WeaponVisualVariantCatalog.NodachiTitan,
                "nodachititan.prefab", "nodachititanstored.prefab"),
            C(EasternWeaponFamily.Nodachi,
                WeaponVisualVariantCatalog.NodachiCapstone,
                "nodachicapstone.prefab",
                "nodachicapstonestored.prefab")
        };

        private static Contract C(EasternWeaponFamily family, string variant,
            string assetName, string storedAssetName)
        {
            if (family == EasternWeaponFamily.Wakizashi)
                return new Contract(family, variant, assetName,
                    storedAssetName, 0.55f, 0.95f, false, Scimitar);
            if (family == EasternWeaponFamily.Katana)
                return new Contract(family, variant, assetName,
                    storedAssetName, 0.85f, 1.25f, false, BastardSword);
            return new Contract(family, variant, assetName, storedAssetName,
                1.30f, 1.90f, true, Greatsword);
        }

        internal static bool IsLoaded
        { get { lock (Sync) return _bundle != null; } }

        internal static bool HasValidatedPrefabs
        {
            get
            {
                lock (Sync) return Prefabs.Count == Contracts.Length &&
                    StoredPrefabs.Count == Contracts.Length;
            }
        }

        internal static string Status
        { get { lock (Sync) return _status; } }

        internal static AssetBundle GetLoadedBundleForGuardedAttribution()
        { lock (Sync) return _bundle; }

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
                if (_bundle != null && Prefabs.Count == Contracts.Length &&
                    StoredPrefabs.Count == Contracts.Length)
                {
                    context.Logger.Info("eastern-weapons", "bundle.reused",
                        "The twelve validated held/stored Eastern Weapon pairs are already published.");
                    return;
                }
            }
            string path = Path.Combine(context.ModEntry.Path, "assets",
                "bundles", BundleName);
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
                    value.EndsWith(".prefab",
                        StringComparison.OrdinalIgnoreCase)).ToArray();
                if (prefabs.Length != Contracts.Length * 2)
                    throw new InvalidDataException(
                        "Expected exactly twelve held/stored Eastern Weapon pairs; observed " +
                        prefabs.Length + ".");
                var validated = new Dictionary<string, GameObject>(
                    StringComparer.Ordinal);
                var validatedStored = new Dictionary<string, GameObject>(
                    StringComparer.Ordinal);
                foreach (Contract contract in Contracts)
                {
                    GameObject held = LoadExact(candidate, prefabs,
                        contract.AssetName);
                    GameObject stored = LoadExact(candidate, prefabs,
                        contract.StoredAssetName);
                    Validate(held, contract, false);
                    Validate(stored, contract, true);
                    Transform heldVisual = held.transform.Find("Visual");
                    Transform storedVisual = stored.transform.Find("Visual");
                    if (ReferenceEquals(held, stored) ||
                        (Approximately(heldVisual.localPosition,
                            storedVisual.localPosition) &&
                         Approximately(heldVisual.localRotation,
                            storedVisual.localRotation)))
                        throw new InvalidDataException(contract.Variant +
                            " held and stored presentations share an incompatible transform.");
                    validated.Add(contract.Variant, held);
                    validatedStored.Add(contract.Variant, stored);
                }
                AssetBundle previous;
                lock (Sync)
                {
                    previous = _bundle;
                    _bundle = candidate;
                    Prefabs.Clear();
                    foreach (KeyValuePair<string, GameObject> pair in validated)
                        Prefabs.Add(pair.Key, pair.Value);
                    StoredPrefabs.Clear();
                    foreach (KeyValuePair<string, GameObject> pair in
                        validatedStored)
                        StoredPrefabs.Add(pair.Key, pair.Value);
                    _status = "custom:validated:12-pairs";
                    candidate = null;
                }
                if (previous != null) previous.Unload(false);
                context.Logger.Info("eastern-weapons", "bundle.loaded",
                    "Published twelve exact held/stored Eastern Weapon pairs transactionally; native family animation, sockets, timing, trails, and sounds remain inherited while complete custom stored prefabs replace donor sheaths on custom clones.");
            }
            catch (Exception exception)
            {
                lock (Sync)
                {
                    Prefabs.Clear();
                    StoredPrefabs.Clear();
                    _status = "native-fallback:bundle-rejected:" +
                        exception.GetType().Name;
                }
                context.Logger.Failure("eastern-weapons",
                    "bundle.load-failed",
                    "Dedicated Eastern Weapon presentation was rejected; native family donors remain active.",
                    exception);
            }
            finally
            {
                if (candidate != null) candidate.Unload(false);
            }
        }

        internal static bool ApplyTo(BlueprintWeaponType weaponType,
            EasternWeaponFamily family)
        {
            if (weaponType == null) throw new ArgumentNullException(
                "weaponType");
            string variant = Classic(family);
            GameObject prefab = GetPrefab(variant);
            GameObject storedPrefab = GetStoredPrefab(variant);
            if (prefab == null || storedPrefab == null) return false;
            WeaponVisualParameters source = weaponType.VisualParameters;
            if (source == null || source.Model == null)
                return RejectTypeAssignment(weaponType, family, source,
                    new InvalidOperationException(
                        "Native family fallback presentation is unavailable."));
            try
            {
                WeaponVisualParameters visual = CloneWithModels(source,
                    prefab, storedPrefab);
                Find(typeof(BlueprintWeaponType), "m_VisualParameters")
                    .SetValue(weaponType, visual);
                if (!ExactModels(weaponType.VisualParameters, prefab,
                        storedPrefab) ||
                    !PreservesUnreplacedDonorFields(source,
                        weaponType.VisualParameters))
                    throw new InvalidOperationException(
                        "Validated Eastern type fallback did not round-trip without altering donor presentation fields.");
                return true;
            }
            catch (Exception exception)
            {
                return RejectTypeAssignment(weaponType, family, source,
                    exception);
            }
        }

        internal static bool ApplyTo(BlueprintItemWeapon item,
            string blueprintSymbol, EasternWeaponFamily family)
        {
            if (item == null) throw new ArgumentNullException("item");
            string variant = WeaponVisualVariantCatalog.Require(
                blueprintSymbol);
            if (!variant.StartsWith(family.ToString() + ".",
                StringComparison.Ordinal))
                throw new InvalidOperationException(blueprintSymbol +
                    " maps across its qualified Eastern family boundary.");
            GameObject prefab = GetPrefab(variant);
            GameObject storedPrefab = GetStoredPrefab(variant);
            if (prefab == null || storedPrefab == null) return false;
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
                field.SetValue(item, CloneWithModels(source, prefab,
                    storedPrefab));
                if (!ExactModels(item.VisualParameters, prefab,
                        storedPrefab) ||
                    !PreservesUnreplacedDonorFields(source,
                        item.VisualParameters))
                    throw new InvalidOperationException(
                        "Exact Eastern item variant did not round-trip without altering donor presentation fields.");
                return true;
            }
            catch (Exception exception)
            {
                return RejectItemAssignment(item, field, original, variant,
                    exception);
            }
        }

        internal static bool HasExactVisual(BlueprintItemWeapon item,
            string blueprintSymbol)
        {
            if (item == null) return false;
            string variant = WeaponVisualVariantCatalog.Require(
                blueprintSymbol);
            return ExactModels(item.VisualParameters, GetPrefab(variant),
                GetStoredPrefab(variant));
        }

        internal static bool HasApprovedVisualOrNativeFallback(
            BlueprintItemWeapon item, string blueprintSymbol)
        {
            if (item == null) return false;
            string variant = WeaponVisualVariantCatalog.Require(
                blueprintSymbol);
            GameObject prefab = GetPrefab(variant);
            GameObject storedPrefab = GetStoredPrefab(variant);
            if (prefab != null || storedPrefab != null)
                return ExactModels(item.VisualParameters, prefab,
                    storedPrefab);
            return item.Type != null && ReferenceEquals(item.VisualParameters,
                item.Type.VisualParameters);
        }

        internal static GameObject InstantiatePrefab(
            EasternWeaponFamily family)
        {
            return InstantiatePrefab(Classic(family));
        }

        internal static GameObject InstantiatePrefab(string variant)
        {
            GameObject prefab = GetPrefab(variant);
            return prefab == null ? null :
                UnityEngine.Object.Instantiate(prefab);
        }

        internal static GameObject InstantiateStoredPrefab(string variant)
        {
            GameObject prefab = GetStoredPrefab(variant);
            return prefab == null ? null :
                UnityEngine.Object.Instantiate(prefab);
        }

        internal static GameObject GetStoredPrefab(string variant)
        {
            GameObject prefab;
            lock (Sync) StoredPrefabs.TryGetValue(variant, out prefab);
            return prefab;
        }

        internal static bool HasCalibratedDonorFrame(GameObject prefab,
            EasternWeaponFamily family, bool stored)
        {
            if (prefab == null) return false;
            try
            {
                RequireCalibratedDonorFrame(prefab,
                    ContractFor(family), stored,
                    prefab.name ?? family.ToString());
                return true;
            }
            catch
            {
                return false;
            }
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

        private static Contract ContractFor(EasternWeaponFamily family)
        {
            return Contracts.First(value => value.Family == family);
        }

        private static GameObject GetPrefab(string variant)
        {
            GameObject prefab;
            lock (Sync) Prefabs.TryGetValue(variant, out prefab);
            return prefab;
        }

        private static GameObject LoadExact(AssetBundle candidate,
            string[] prefabs, string assetName)
        {
            string[] matches = prefabs.Where(value => value.EndsWith(
                "/" + assetName,
                StringComparison.OrdinalIgnoreCase)).ToArray();
            if (matches.Length != 1)
                throw new InvalidDataException("Expected one " + assetName +
                    "; observed " + matches.Length + ".");
            GameObject prefab = candidate.LoadAsset<GameObject>(matches[0]);
            if (prefab == null)
                throw new InvalidDataException(assetName + " is null.");
            return prefab;
        }

        private static WeaponVisualParameters CloneWithModels(
            WeaponVisualParameters source, GameObject prefab,
            GameObject storedPrefab)
        {
            var visual = new WeaponVisualParameters();
            foreach (FieldInfo field in typeof(WeaponVisualParameters)
                .GetFields(Fields))
                if (!field.IsStatic && !field.IsInitOnly)
                    field.SetValue(visual, field.GetValue(source));
            Find(typeof(WeaponVisualParameters), "m_WeaponModel")
                .SetValue(visual, prefab);
            Find(typeof(WeaponVisualParameters), "m_WeaponBeltModel")
                .SetValue(visual, storedPrefab);
            // Every accepted variant has a separately validated complete stored
            // prefab. Retaining the donor sheath alongside that replacement
            // duplicates the stored presentation and can leave the donor
            // scabbard detached during held/transition states.
            Find(typeof(WeaponVisualParameters), "m_WeaponSheathModel")
                .SetValue(visual, null);
            if (!ExactModels(visual, prefab, storedPrefab) ||
                !PreservesUnreplacedDonorFields(source, visual))
                throw new InvalidOperationException(
                    "Eastern visual clone did not preserve the native donor contract outside the replaced held/stored/sheath presentation fields.");
            return visual;
        }

        private static bool ExactModels(WeaponVisualParameters visual,
            GameObject prefab, GameObject storedPrefab)
        {
            return visual != null && prefab != null &&
                storedPrefab != null &&
                ReferenceEquals(visual.Model, prefab) &&
                ReferenceEquals(visual.BeltModel, storedPrefab) &&
                visual.SheathModel == null;
        }

        private static bool PreservesUnreplacedDonorFields(
            WeaponVisualParameters source, WeaponVisualParameters value)
        {
            if (source == null || value == null) return false;
            foreach (FieldInfo field in typeof(WeaponVisualParameters)
                .GetFields(Fields))
            {
                if (field.IsStatic ||
                    field.Name == "m_WeaponModel" ||
                    field.Name == "m_WeaponBeltModel" ||
                    field.Name == "m_WeaponSheathModel")
                    continue;
                if (!object.Equals(field.GetValue(source),
                    field.GetValue(value)))
                    return false;
            }
            return true;
        }

        private static void Validate(GameObject prefab, Contract contract,
            bool stored)
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
            Transform grip = root.Find(
                WeaponPresentationFrameContract.GripMarker);
            Transform support = root.Find(
                WeaponPresentationFrameContract.SupportMarker);
            Transform tip = root.Find("Tip");
            Transform butt = root.Find(
                WeaponPresentationFrameContract.ButtMarker);
            Transform forward = root.Find(
                WeaponPresentationFrameContract.WeaponForwardMarker);
            Transform bladeNormal = root.Find(
                WeaponPresentationFrameContract.BladeNormalMarker);
            Transform cuttingEdge = root.Find(CuttingEdgeMarker);
            Transform storedMount = root.Find(StoredMountMarker);
            bool needsSupport = !stored && contract.RequiresSupport;
            if (visual == null || grip == null || tip == null || butt == null ||
                forward == null || bladeNormal == null ||
                cuttingEdge == null || needsSupport != (support != null) ||
                stored != (storedMount != null))
                throw new InvalidDataException(contract.Variant +
                    " " + (stored ? "stored" : "held") +
                    " semantic anchors are incomplete.");
            if (!Finite(grip.localPosition) || !Finite(tip.localPosition) ||
                !Finite(butt.localPosition) ||
                !Finite(forward.localPosition) ||
                !Finite(bladeNormal.localPosition) ||
                !Finite(cuttingEdge.localPosition) ||
                !Finite(visual.localPosition) ||
                !Approximately(visual.localScale, Vector3.one) ||
                (support != null && !Finite(support.localPosition)) ||
                (storedMount != null && !Finite(storedMount.localPosition)))
                throw new InvalidDataException(contract.Variant +
                    " semantic frame is nonfinite or scaled.");
            if (prefab.GetComponentsInChildren<Transform>(true).Any(value =>
                    value != null && value.name.StartsWith("KMG_",
                        StringComparison.Ordinal)))
                throw new InvalidDataException(contract.Variant +
                    " contains build-only source markers.");

            WeaponPresentationSemanticFrame frame =
                RequireCalibratedDonorFrame(prefab, contract, stored,
                    contract.Variant);
            WeaponPresentationFrameContract.ValidateRendererEndpoints(root,
                visual, frame, contract.Variant, 0.08f);
            WeaponPresentationFrameContract.ValidateSecondaryAsPlaneNormal(
                root, visual, frame, contract.Variant, 0.20f);

            EquipmentOffsets offsets = prefab.GetComponent<EquipmentOffsets>();
            if (needsSupport)
            {
                if (offsets == null)
                    offsets = prefab.AddComponent<EquipmentOffsets>();
                // EquipmentOffsets.GetOffsets may enumerate this collection for
                // native equipment slots. A runtime-added component has no
                // serialized collection, so represent the intentional absence
                // of custom slot corrections with an empty array.
                if (offsets.m_SlotOffsets == null)
                    offsets.m_SlotOffsets = new EquipmentOffsets.Offsets[0];
                offsets.IkTargetLeftHand = support;
                if (offsets.m_SlotOffsets == null ||
                    !ReferenceEquals(offsets.IkTargetLeftHand, support))
                    throw new InvalidDataException(contract.Variant +
                        " held support-hand offset initialization failed.");
            }
            else if (offsets != null && offsets.IkTargetLeftHand != null)
                throw new InvalidDataException(contract.Variant +
                    " presentation unexpectedly drives left-hand IK.");

            Renderer[] renderers = visual.GetComponentsInChildren<Renderer>(
                true);
            if (renderers.Length == 0 || renderers.Any(value => value == null ||
                !value.enabled || !value.gameObject.activeSelf ||
                value.sharedMaterials == null ||
                value.sharedMaterials.Length == 0 ||
                value.sharedMaterials.Any(material => material == null ||
                    material.shader == null)))
                throw new InvalidDataException(contract.Variant +
                    " renderers or materials are incomplete.");
            if (prefab.GetComponentsInChildren<Camera>(true).Length != 0 ||
                prefab.GetComponentsInChildren<Light>(true).Length != 0)
                throw new InvalidDataException(contract.Variant +
                    " prefab contains a camera or light.");
        }

        private static WeaponPresentationSemanticFrame
            RequireCalibratedDonorFrame(GameObject prefab,
                Contract contract, bool stored, string label)
        {
            Transform root = prefab.transform;
            Transform grip = root.Find(
                WeaponPresentationFrameContract.GripMarker);
            Transform edge = root.Find(CuttingEdgeMarker);
            Transform storedMount = root.Find(StoredMountMarker);
            bool needsSupport = !stored && contract.RequiresSupport;
            WeaponPresentationSemanticFrame frame = needsSupport
                ? WeaponPresentationFrameContract.
                    RequireWithForwardMarkerAndButtSupport(root, label,
                        "Tip",
                        WeaponPresentationFrameContract.BladeNormalMarker,
                        WeaponPresentationFrameContract.WeaponForwardMarker,
                        contract.Minimum, contract.Maximum)
                : WeaponPresentationFrameContract.RequireWithForwardMarker(
                    root, label, "Tip",
                    WeaponPresentationFrameContract.BladeNormalMarker,
                    WeaponPresentationFrameContract.WeaponForwardMarker,
                    false, contract.Minimum, contract.Maximum);
            Quaternion donorRotation = Quaternion.Euler(stored ?
                contract.Donor.StoredEuler : contract.Donor.HeldEuler);
            Vector3 targetForward = donorRotation * Vector3.up;
            Vector3 targetBladeNormal = donorRotation * Vector3.right;
            Vector3 targetCuttingEdge = donorRotation * Vector3.back;
            Vector3 edgeDirection = (edge.localPosition -
                grip.localPosition).normalized;
            if (Vector3.Dot(frame.Forward, targetForward.normalized) <
                    0.99999f ||
                Vector3.Dot(frame.Up, targetBladeNormal.normalized) <
                    0.99999f ||
                Vector3.Dot(edgeDirection,
                    targetCuttingEdge.normalized) < 0.99999f ||
                Vector3.Dot(edgeDirection, -frame.Right) < 0.99999f)
                throw new InvalidDataException(label +
                    " semantic frame does not match the measured " +
                    contract.Donor.Name + " " +
                    (stored ? "stored" : "held") + " blade basis.");
            if (needsSupport)
            {
                float supportStation = Vector3.Dot(frame.Support -
                    frame.Grip, frame.Forward);
                if (Mathf.Abs(supportStation - NodachiSupportStation) >
                    0.001f)
                    throw new InvalidDataException(label +
                        " support station does not match the native Greatsword handle interval.");
            }
            if (!stored && !Approximately(frame.Grip,
                    contract.Donor.HeldPosition))
                throw new InvalidDataException(label +
                    " held grip no longer lands on the measured native donor grip.");
            Vector3 storedAnchor = StoredRendererAnchor(contract.Donor);
            if (stored && (storedMount == null ||
                !Approximately(storedMount.localPosition, storedAnchor)))
                throw new InvalidDataException(label +
                    " independent StoredMount does not match the measured native donor renderer anchor.");
            return frame;
        }

        private static Vector3 StoredRendererAnchor(DonorFrame donor)
        {
            Quaternion rotation = Quaternion.Euler(donor.StoredEuler);
            return donor.StoredPosition + rotation * Vector3.Scale(
                donor.StoredRendererCenter, donor.StoredScale);
        }

        private static bool RejectTypeAssignment(
            BlueprintWeaponType weaponType, EasternWeaponFamily family,
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
            }
            Reject(family.ToString(), exception);
            return false;
        }

        private static bool RejectItemAssignment(BlueprintItemWeapon item,
            FieldInfo field, object fallback, string variant,
            Exception exception)
        {
            try
            {
                field.SetValue(item, fallback);
            }
            catch
            {
            }
            Reject(variant, exception);
            return false;
        }

        private static void Reject(string scope, Exception exception)
        {
            ModLogger logger;
            lock (Sync)
            {
                _status = "native-fallback:model-assignment-rejected:" +
                    scope + ":" + exception.GetType().Name;
                logger = _logger;
            }
            if (logger != null) logger.Failure("eastern-weapons",
                "model.assignment-failed",
                "Exact Eastern held/stored model assignment was rejected; the native/type fallback remains active.",
                exception);
        }

        private static FieldInfo Find(Type type, string name)
        {
            for (Type current = type; current != null;
                current = current.BaseType)
            {
                FieldInfo field = current.GetField(name, Fields |
                    BindingFlags.DeclaredOnly);
                if (field != null) return field;
            }
            throw new MissingFieldException(type.FullName, name);
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

        private static bool Finite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }
    }
}
