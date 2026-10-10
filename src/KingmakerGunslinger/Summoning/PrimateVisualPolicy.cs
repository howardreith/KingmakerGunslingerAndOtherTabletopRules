using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace KingmakerGunslinger.Summoning
{
    /// <summary>
    /// The closed Sprint 18 original-body binding contract.
    ///
    /// <para>Both apes are original project-owned geometry on one donor rig.
    /// The guarded primate donor census surveyed twenty-eight rigs and found
    /// no primate unit type and no primate body in the installed library, so
    /// there was nothing to clone; it chose the Troll guard because it is the
    /// only Large rig with real hands, a separate jaw and toed feet, which is
    /// what ape geometry has to hang on.</para>
    ///
    /// <para>No native transform, mesh, texture or animation curve is
    /// redistributed. What ships is our vertices, our painting and the donor's
    /// bone NAMES; the bind poses are read from the live donor at attach
    /// time.</para>
    /// </summary>
    internal static class PrimateVisualPolicy
    {
        internal const string TrollPrefab = "0bc98460fca38964aae3af6ad5c655ee";
        internal const string TrollBlueprint = "b98735a1737ae494dbe5cbeca1c7c083";
        internal const string BodyRenderer = "Troll_base";
        internal const string EquipmentRenderer = "polySurface1";
        internal const string RootBone = "Pelvis";
        internal const int BodyBoneCount = 61;
        internal const int EquipmentBoneCount = 62;
        internal const string AuthoredWinding = "shared-exporter-sprint18";
        internal const string ApeAssetDirectory = "sprint18-primates";
        internal const string FourArmedAssetDirectory = "sprint19-fourarmed";
        internal const string ApeGuid = "c53c3e23097e4f25a411c50ce2868c60";
        internal const string DireApeGuid = "5482b49785a3492aa7d29a4ee575cd66";
        internal const string ApeBlueprintName = "KMG_Summoning_Unit_Ape";
        internal const string DireApeBlueprintName = "KMG_Summoning_Unit_DireApe";
        internal const string GirallonGuid = "ce4a1d1c1b7a4cd1b8f1e7c3b9d6a240";
        internal const string XillGuid = "7d5b8e2a6f3c4e1a9b0d2f8c4a6e1357";
        internal const string GirallonBlueprintName = "KMG_Summoning_Unit_Girallon";
        internal const string XillBlueprintName = "KMG_Summoning_Unit_Xill";

        /// <summary>The Sprint 18 pair: two-armed apes.</summary>
        internal static string[] ApeKeys
        { get { return new[] { PrimateRulesPolicy.ApeKey, PrimateRulesPolicy.DireApeKey }; } }

        /// <summary>
        /// The Sprint 19 pair: four-armed bodies on the same two-armed donor
        /// rig, which is the authored limitation
        /// FOUR_ARMS_SHARE_TWO_DRIVER_CHAINS.
        /// </summary>
        internal static string[] FourArmedKeys
        {
            get
            {
                return new[] { GirallonRulesPolicy.GirallonKey,
                    XillRulesPolicy.XillKey };
            }
        }

        /// <summary>
        /// Every creature rebodied on this donor rig. The rig, the drivers and
        /// the exporter are the same for all four, which is why they share one
        /// policy rather than two.
        /// </summary>
        internal static string[] Keys
        { get { return ApeKeys.Concat(FourArmedKeys).ToArray(); } }

        internal static bool IsFourArmed(string key)
        { return FourArmedKeys.Contains(key, StringComparer.Ordinal); }

        /// <summary>Where this creature's shipped body files live.</summary>
        internal static string AssetDirectory(string key)
        {
            return IsFourArmed(key) ? FourArmedAssetDirectory :
                ApeKeys.Contains(key, StringComparer.Ordinal) ?
                    ApeAssetDirectory : null;
        }

        /// <summary>
        /// Every donor bone the original bodies may weight geometry to.
        ///
        /// <para>Five donor bones are deliberately absent. Tail_01 and
        /// Tail_02 carry nothing because an ape has no tail, and Tongue_01
        /// through Tongue_03 carry nothing because neither printed routine has
        /// a tongue attack. A weight arriving on one of them would mean the
        /// generator had started inventing anatomy, so the loader refuses the
        /// mesh rather than showing it.</para>
        /// </summary>
        internal static string[] Bones(string key)
        {
            if (!Keys.Contains(key, StringComparer.Ordinal)) return new string[0];
            var sides = new[] { "L", "R" };
            return new[] { "Pelvis", "Spine_01", "Spine_02", "Spine_03", "Neck_01",
                    "Head", "Jaw_01", "Stomach_01" }
                .Concat(sides.SelectMany(side => new[] { "Up_lip_01", "Eyebrow_01",
                    "Ear_01", "Clavicle_01", "Up_Arm_01", "Forearm_01", "Forearm_02",
                    "Hand_01", "Thumb_01", "Thumb_02", "Thumb_03",
                    "Fore_Finger_01", "Fore_Finger_02", "Fore_Finger_03",
                    "Midle_Finger_01", "Midle_Finger_02", "Midle_Finger_03",
                    "Little_Finger_01", "Little_Finger_02", "Little_Finger_03",
                    "UpLeg_01", "Leg_01", "Foot_01", "Foot_Toe_01" }
                    .Select(part => side + "_" + part)))
                .ToArray();
        }

        /// <summary>The five donor branches that must stay empty.</summary>
        internal static string[] ExcludedBones
        {
            get
            {
                return new[] { "Tail_01", "Tail_02", "Tongue_01", "Tongue_02",
                    "Tongue_03" };
            }
        }

        /// <summary>
        /// The production hook, which is narrower than the key helper: it
        /// matches immutable identity AND name AND prefab, so no native troll
        /// and no other borrowed-rig creature can ever be rebodied by it.
        /// </summary>
        internal static bool TryProductionPrimate(bool moduleEnabled, string guid,
            string blueprintName, string prefab, out string key)
        {
            key = null;
            if (!moduleEnabled || prefab != TrollPrefab) return false;
            if (guid == ApeGuid && blueprintName == ApeBlueprintName)
                key = PrimateRulesPolicy.ApeKey;
            else if (guid == DireApeGuid && blueprintName == DireApeBlueprintName)
                key = PrimateRulesPolicy.DireApeKey;
            else if (guid == GirallonGuid && blueprintName == GirallonBlueprintName)
                key = GirallonRulesPolicy.GirallonKey;
            else if (guid == XillGuid && blueprintName == XillBlueprintName)
                key = XillRulesPolicy.XillKey;
            return key != null;
        }

        /// <summary>
        /// Exact instance-delimited resource names, for the guarded crowd's
        /// read-only resource census. A neighbouring ape or a native asset
        /// must not be counted as this instance's resource.
        /// </summary>
        internal static bool IsPrimateInstanceResource(string key, string meshName,
            string resourceName)
        {
            if (!Keys.Contains(key, StringComparer.Ordinal) ||
                string.IsNullOrEmpty(meshName) || string.IsNullOrEmpty(resourceName))
                return false;
            string stem = "KMG_" + key + "_Original_";
            int instance;
            if (!meshName.StartsWith(stem, StringComparison.Ordinal) ||
                !int.TryParse(meshName.Substring(stem.Length), NumberStyles.AllowLeadingSign,
                    CultureInfo.InvariantCulture, out instance) ||
                meshName != stem + instance.ToString(CultureInfo.InvariantCulture))
                return false;
            return resourceName == meshName ||
                resourceName.StartsWith(meshName + "_", StringComparison.Ordinal) ||
                resourceName.StartsWith(meshName + " (", StringComparison.Ordinal);
        }

        internal static bool PermitsOriginalWinding(string key, string marker)
        { return Keys.Contains(key, StringComparer.Ordinal) && marker == AuthoredWinding; }

        /// <summary>
        /// The anatomy the reviewed mesh declares about itself, checked
        /// against what the printed entries say. The Dire Ape has two primary
        /// claw attacks and therefore visible claws; the Ape has neither.
        /// </summary>
        internal static bool PermitsAnatomy(string key, bool? tailGeometry,
            bool? tongueGeometry, bool? jawSeparated, int? visibleLimbs,
            bool? clawedHands, bool? opposableThumbs, bool? knuckleWalkAuthored)
        {
            if (!ApeKeys.Contains(key, StringComparer.Ordinal)) return false;
            return tailGeometry == false && tongueGeometry == false &&
                jawSeparated == true && visibleLimbs == 4 &&
                opposableThumbs == true && knuckleWalkAuthored == false &&
                clawedHands == (key == PrimateRulesPolicy.DireApeKey);
        }

        /// <summary>
        /// The anatomy a four-armed body declares about itself, checked
        /// against what the printed entries say and against what the donor rig
        /// can actually do.
        ///
        /// <para>Six visible limbs: four arms and two legs. Four visible arms
        /// on exactly two animation driver chains, with the lower arms
        /// skinned to the upper arms' bones - which the mesh must declare,
        /// because a body claiming four independent chains would be claiming
        /// a rig this project did not build. Both creatures have clawed hands:
        /// the Girallon prints four claw attacks and the Xill four.</para>
        /// </summary>
        internal static bool PermitsFourArmedAnatomy(string key,
            int? visibleLimbs, int? visibleArms, int? armDriverChains,
            bool? lowerArmsShareUpperArmDrivers, bool? clawedHands,
            string printedSize)
        {
            if (!IsFourArmed(key)) return false;
            return visibleLimbs == 6 && visibleArms == 4 &&
                armDriverChains == 2 &&
                lowerArmsShareUpperArmDrivers == true && clawedHands == true &&
                printedSize == (key == GirallonRulesPolicy.GirallonKey ?
                    "Large" : "Medium");
        }

        internal static bool ExactSet(IEnumerable<string> actual, IEnumerable<string> expected)
        {
            if (actual == null || expected == null) return false;
            string[] rows = actual.ToArray(), wanted = expected.ToArray();
            return rows.Length == wanted.Length && !rows.Any(string.IsNullOrEmpty) &&
                rows.Distinct(StringComparer.Ordinal).Count() == rows.Length &&
                new HashSet<string>(rows, StringComparer.Ordinal).SetEquals(wanted);
        }

        internal static bool PermitsBones(string key, IEnumerable<string> actual)
        { return Keys.Contains(key, StringComparer.Ordinal) && ExactSet(actual, Bones(key)); }

        /// <summary>
        /// The exact donor this was authored against: the Troll guard's body
        /// skin and its equipment skin, the measured bone counts, the root
        /// bone, and no static weapon mesh at all.
        /// </summary>
        internal static bool PermitsDonor(string key, string prefab,
            IEnumerable<string> skinNames, int bodyBoneCount, int equipmentBoneCount,
            string rootBone, IEnumerable<string> staticMeshes)
        {
            return Keys.Contains(key, StringComparer.Ordinal) && prefab == TrollPrefab &&
                ExactSet(skinNames, new[] { BodyRenderer, EquipmentRenderer }) &&
                bodyBoneCount == BodyBoneCount &&
                equipmentBoneCount == EquipmentBoneCount && rootBone == RootBone &&
                ExactSet(staticMeshes, new string[0]);
        }

        /// <summary>
        /// Map each authored driver onto the donor bone of the same name.
        ///
        /// <para>Unlike the Sprint 17 hybrid there is no support slot: every
        /// vertex of both apes is driven by a real donor bone, and every
        /// bindpose comes from the donor. A missing name fails the whole
        /// attach rather than falling back to the renderer frame.</para>
        /// </summary>
        internal static bool TryResolveDriverSlots(string key, string[] originalNames,
            string[] nativeNames, out int[] slots)
        {
            slots = null;
            if (!PermitsBones(key, originalNames) || nativeNames == null ||
                nativeNames.Length != BodyBoneCount ||
                nativeNames.Any(string.IsNullOrEmpty) ||
                nativeNames.Distinct(StringComparer.Ordinal).Count() != nativeNames.Length)
                return false;
            // The donor branches the bodies deliberately leave empty must
            // still be present: their absence would mean a different rig.
            if (ExcludedBones.Any(name => !nativeNames.Contains(name, StringComparer.Ordinal)))
                return false;
            var found = new int[originalNames.Length];
            for (int index = 0; index < originalNames.Length; index++)
            {
                found[index] = Array.IndexOf(nativeNames, originalNames[index]);
                if (found[index] < 0) return false;
            }
            slots = found;
            return true;
        }

        internal static void Validate()
        {
            foreach (string key in Keys)
            {
                string[] bones = Bones(key);
                if (bones.Length != 56)
                    throw new InvalidOperationException(
                        "The Sprint 18 driver set must be the 56 reviewed donor bones.");
                if (bones.Distinct(StringComparer.Ordinal).Count() != bones.Length)
                    throw new InvalidOperationException("A Sprint 18 driver repeats.");
                if (bones.Intersect(ExcludedBones, StringComparer.Ordinal).Any())
                    throw new InvalidOperationException(
                        "A Sprint 18 driver is one of the deliberately empty branches.");
                if (AssetDirectory(key) == null)
                    throw new InvalidOperationException(
                        "A rebodied creature has no shipped body directory: " + key);
                if (IsFourArmed(key))
                {
                    if (!PermitsFourArmedAnatomy(key, 6, 4, 2, true, true,
                            key == GirallonRulesPolicy.GirallonKey ?
                                "Large" : "Medium"))
                        throw new InvalidOperationException(
                            "The Sprint 19 anatomy contract rejects its own creature: "
                            + key);
                    // A four-armed body must never satisfy the ape contract:
                    // the two schemas describe different anatomies and a mesh
                    // that passed both would mean one of them had stopped
                    // saying anything.
                    if (PermitsAnatomy(key, false, false, true, 4, true, true, false))
                        throw new InvalidOperationException(
                            "A four-armed body must not pass the ape anatomy contract: "
                            + key);
                }
                else if (!PermitsAnatomy(key, false, false, true, 4,
                        key == PrimateRulesPolicy.DireApeKey, true, false))
                    throw new InvalidOperationException(
                        "The Sprint 18 anatomy contract rejects its own creature: " + key);
            }
            string[] guids = ProductionGuids;
            if (guids.Length != Keys.Length ||
                guids.Distinct(StringComparer.Ordinal).Count() != guids.Length ||
                guids.Any(guid => guid == null || guid.Length != 32))
                throw new InvalidOperationException(
                    "Each rebodied creature needs its own distinct identity.");
            if (DriverAndEmptyBranchCount() != BodyBoneCount)
                throw new InvalidOperationException(
                    "The reviewed and deliberately empty donor branches must together " +
                    "account for every bone of the donor body skin.");
        }

        private static int DriverAndEmptyBranchCount()
        { return Bones(PrimateRulesPolicy.ApeKey).Length + ExcludedBones.Length; }

        /// <summary>
        /// The four identities this policy may rebody, so a duplicated or
        /// mistyped constant cannot quietly make two creatures the same one.
        /// </summary>
        internal static string[] ProductionGuids
        { get { return new[] { ApeGuid, DireApeGuid, GirallonGuid, XillGuid }; } }
    }
}
