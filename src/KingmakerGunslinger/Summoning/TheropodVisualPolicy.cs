using System;
using System.Collections.Generic;
using System.Linq;

namespace KingmakerGunslinger.Summoning
{
    /// <summary>
    /// Exact ownership and renderer-selection contract for the two Sprint 24
    /// original bodies. Bone names are native animation interfaces; every
    /// vertex, UV and painted pixel remains project-owned.
    /// </summary>
    internal static class TheropodVisualPolicy
    {
        internal const string DeinonychusBlueprintName =
            "KMG_Summoning_Unit_Deinonychus";
        internal const string TyrannosaurusBlueprintName =
            "KMG_Summoning_Unit_Tyrannosaurus";
        internal const string DeinonychusBlueprintGuid =
            "10a13f87e3664e30b642a3c59cc8c700";
        internal const string TyrannosaurusBlueprintGuid =
            "04b1a75a76544a9581ee832045f8227a";
        internal const string DonorGuid =
            "a8cbd4cdb0feaf64eab1452257c3c971";
        internal const string DonorPrefabGuid =
            "d6db6ba3b2c459a48ba0730beef0bac8";
        // Sprint 17's released Purple Worm lifecycle proved this exact native
        // unit owns a DismemberUnitFX-backed BloodPuddleFx. Theropods borrow
        // only that immutable effect link so their original materials receive
        // the mandatory native expiry dissolve; no worm mesh, animation,
        // stomach payload or other blueprint state is copied.
        internal const string ExpiryEffectDonorGuid =
            "bf2216f48b3f4d24c9c502007649340d";
        internal const string BodyRendererName = "_lizardman001";
        internal const string AccessoryRendererName = "_ammunition04";
        internal const string RootBoneName = "Torso_Lower";
        internal const int BodyBoneCount = 39;
        internal const int AccessoryBoneCount = 19;
        // UnitEntityView.Corpulence is a radius. These are the tabletop 5-foot
        // and 20-foot spaces expressed as radii in native metres. The engine's
        // live size multiplier remains authoritative for later size changes.
        internal const float DeinonychusBaseCorpulence = 0.762f;
        internal const float TyrannosaurusBaseCorpulence = 3.048f;

        internal static readonly string[] Bones =
        {
            "Head", "L_Arm_Lower", "L_Arm_Upper", "L_Bfinger1",
            "L_Bfinger2", "L_Foot0", "L_Leg0_Lower", "L_Leg0_Upper",
            "L_Palm", "L_claw", "L_finger1", "L_finger2", "L_toe",
            "R_Arm_Lower", "R_Arm_Upper", "R_Bfinger1", "R_Bfinger2",
            "R_Foot0", "R_Leg0_Lower", "R_Leg0_Upper", "R_Palm",
            "R_claw", "R_finger1", "R_finger2", "R_toe",
            "Torso_Lower", "Torso_Upper", "jaw", "jaw1", "neck",
            "neck1", "tail", "tail1", "tail2", "tail3"
        };

        internal static readonly string[] Keys =
        {
            TheropodRulesPolicy.DeinonychusKey,
            TheropodRulesPolicy.TyrannosaurusKey
        };

        internal static bool Handles(string key)
        { return Keys.Contains(key, StringComparer.Ordinal); }

        internal static bool TryProductionTheropod(bool moduleEnabled,
            string guid, string blueprintName, string prefab, out string key)
        {
            key = null;
            if (!moduleEnabled || prefab != DonorPrefabGuid) return false;
            if (guid == DeinonychusBlueprintGuid &&
                blueprintName == DeinonychusBlueprintName)
                key = TheropodRulesPolicy.DeinonychusKey;
            else if (guid == TyrannosaurusBlueprintGuid &&
                blueprintName == TyrannosaurusBlueprintName)
                key = TheropodRulesPolicy.TyrannosaurusKey;
            return key != null;
        }

        internal static bool TryBaseCorpulence(string key, out float radius)
        {
            radius = key == TheropodRulesPolicy.DeinonychusKey
                ? DeinonychusBaseCorpulence
                : key == TheropodRulesPolicy.TyrannosaurusKey
                    ? TyrannosaurusBaseCorpulence : 0f;
            return radius > 0f;
        }

        internal static bool IsPermitted(string key,
            IEnumerable<string> names)
        {
            if (!Handles(key) || names == null) return false;
            string[] values = names.ToArray();
            return values.Length == Bones.Length &&
                values.Distinct(StringComparer.Ordinal).Count() == values.Length &&
                new HashSet<string>(values, StringComparer.Ordinal)
                    .SetEquals(Bones);
        }

        internal static bool IsExactRendererSet(
            IEnumerable<string> rendererNames)
        {
            if (rendererNames == null) return false;
            string[] values = rendererNames.ToArray();
            return values.Length == 2 &&
                values.Distinct(StringComparer.Ordinal).Count() == 2 &&
                values.Contains(BodyRendererName, StringComparer.Ordinal) &&
                values.Contains(AccessoryRendererName, StringComparer.Ordinal);
        }
    }
}
