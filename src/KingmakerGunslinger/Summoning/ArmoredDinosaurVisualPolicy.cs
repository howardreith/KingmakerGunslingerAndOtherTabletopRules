using System;
using System.Collections.Generic;
using System.Linq;

namespace KingmakerGunslinger.Summoning
{
    /// <summary>
    /// Exact ownership contract for the two Sprint 23 original skins. Bone
    /// names are native animation interfaces; every visible vertex, UV and
    /// painting remains project-owned.
    /// </summary>
    internal static class ArmoredDinosaurVisualPolicy
    {
        internal const string TriceratopsBlueprintName =
            "KMG_Summoning_Unit_Triceratops";
        internal const string StegosaurusBlueprintName =
            "KMG_Summoning_Unit_Stegosaurus";
        internal const string TriceratopsDonorGuid =
            "028cc6f46e7998f46855a33ffde89567";
        internal const string StegosaurusDonorGuid =
            "c3524f96954a1d94f8525b86e7626633";

        internal static readonly string[] TriceratopsBones =
        {
            "Head", "L_Arm_Lower", "L_Arm_Upper", "L_Foot0",
            "L_Leg0_Lower", "L_Leg0_Upper", "L_Palm", "LowerTorso",
            "Neck", "R_Arm_Lower", "R_Arm_Upper", "R_Foot0",
            "R_Leg0_Lower", "R_Leg0_Upper", "R_Palm", "Spine",
            "Tail0_M", "Tail1_M", "Tail2_M", "Tail3_M", "Tail4_M",
            "UpperTorso", "backKnee_L", "backKnee_R", "frontKnee_L",
            "frontKnee_R"
        };

        internal static readonly string[] StegosaurusBones =
        {
            "Head", "Jaw_M", "LeftFoot0", "LeftHand", "LeftLowerArm",
            "LeftLowerLeg0", "LeftUpperArm", "LeftUpperLeg0", "LowerTorso",
            "Neck_M", "RightFoot0", "RightHand", "RightLowerArm",
            "RightLowerLeg0", "RightUpperArm", "RightUpperLeg0", "Spine1_M",
            "Tail0_M", "Tail1_M", "Tail2_M", "Tail3_M", "Tail4_M",
            "Tail5_M", "UpperTorso"
        };

        internal static readonly string[] Keys =
        {
            ArmoredDinosaurRulesPolicy.TriceratopsKey,
            ArmoredDinosaurRulesPolicy.StegosaurusKey
        };

        internal static bool Handles(string key)
        { return Keys.Contains(key, StringComparer.Ordinal); }

        internal static string DonorGuidFor(string key)
        {
            if (key == ArmoredDinosaurRulesPolicy.TriceratopsKey)
                return TriceratopsDonorGuid;
            if (key == ArmoredDinosaurRulesPolicy.StegosaurusKey)
                return StegosaurusDonorGuid;
            return null;
        }

        internal static string[] BonesFor(string key)
        {
            if (key == ArmoredDinosaurRulesPolicy.TriceratopsKey)
                return TriceratopsBones;
            if (key == ArmoredDinosaurRulesPolicy.StegosaurusKey)
                return StegosaurusBones;
            return null;
        }

        internal static bool IsPermitted(string key,
            IEnumerable<string> names)
        {
            string[] expected = BonesFor(key);
            if (expected == null || names == null) return false;
            string[] values = names.ToArray();
            return values.Length == expected.Length &&
                values.Distinct(StringComparer.Ordinal).Count() == values.Length &&
                new HashSet<string>(values, StringComparer.Ordinal)
                    .SetEquals(expected);
        }
    }
}
