using System;
using System.Collections.Generic;
using System.Linq;

namespace KingmakerGunslinger.Summoning
{
    /// <summary>
    /// Exact ownership contract for the two Sprint 25 original bodies.
    /// Bone names are native animation interfaces; every vertex, UV and
    /// painting is project-owned. Both creatures bind to the same donor
    /// renderer, so the donor identity is shared and the bone sets are
    /// not: the Brachiosaurus neck is driven by <c>Head</c> alone, which
    /// is the owner-authorized interim design and is why its set is the
    /// smaller one.
    /// </summary>
    internal static class GiantBodyVisualPolicy
    {
        internal const string BrachiosaurusBlueprintName =
            "KMG_Summoning_Unit_Brachiosaurus";
        internal const string ElephantBlueprintName =
            "KMG_Summoning_Unit_Elephant";

        /// <summary>MastodonSummon, the measured donor for both.</summary>
        internal const string DonorGuid =
            "028cc6f46e7998f46855a33ffde89567";
        internal const string DonorPrefabGuid =
            "fe9f25fe62e0f3b49a13910a996470b2";
        internal const string BodyRendererName = "mastodont";

        internal static readonly string[] BrachiosaurusBones =
        {
            "Head", "L_Arm_Lower", "L_Arm_Upper", "L_Foot0", "L_Leg0_Lower",
            "L_Leg0_Upper", "L_Palm", "LowerTorso", "Neck", "R_Arm_Lower",
            "R_Arm_Upper", "R_Foot0", "R_Leg0_Lower", "R_Leg0_Upper", "R_Palm",
            "Tail0_M", "Tail1_M", "Tail2_M", "Tail3_M", "Tail4_M", "UpperTorso",
            "backAnkle_L", "backAnkle_R", "backKnee_L", "backKnee_R", "frontAnkle_L",
            "frontAnkle_R", "frontKnee_L", "frontKnee_R"
        };

        internal static readonly string[] ElephantBones =
        {
            "Ear_L", "Ear_R", "Head", "L_Arm_Lower", "L_Arm_Upper", "L_Foot0",
            "L_Leg0_Lower", "L_Leg0_Upper", "L_Palm", "LowerTorso", "Neck",
            "R_Arm_Lower", "R_Arm_Upper", "R_Foot0", "R_Leg0_Lower", "R_Leg0_Upper",
            "R_Palm", "Tail0_M", "Tail11_M", "Tail1_M", "Tail21_M", "Tail2_M",
            "Tail31_M", "Tail3_M", "Tail41_M", "Tail4_M", "Tail5_M", "Tail6_M",
            "Tail7_M", "UpperTorso", "backAnkle_L", "backAnkle_R", "backKnee_L",
            "backKnee_R", "frontAnkle_L", "frontAnkle_R", "frontKnee_L",
            "frontKnee_R"
        };

        internal static readonly string[] Keys =
        {
            GiantBodyRulesPolicy.BrachiosaurusKey,
            GiantBodyRulesPolicy.ElephantKey
        };

        internal static bool Handles(string key)
        { return Keys.Contains(key, StringComparer.Ordinal); }

        internal static string[] BonesFor(string key)
        {
            if (key == GiantBodyRulesPolicy.BrachiosaurusKey)
                return BrachiosaurusBones;
            if (key == GiantBodyRulesPolicy.ElephantKey)
                return ElephantBones;
            return null;
        }

        /// <summary>The donor exposes exactly one skinned renderer, so a
        /// view that presents a different set is not the measured donor
        /// and the original body must not be installed on it.</summary>
        internal static bool IsExactRendererSet(IEnumerable<string> names)
        {
            if (names == null) return false;
            string[] values = names.ToArray();
            return values.Length == 1 &&
                values[0] == BodyRendererName;
        }

        internal static bool IsPermitted(string key,
            IEnumerable<string> names)
        {
            string[] expected = BonesFor(key);
            if (expected == null || names == null) return false;
            string[] values = names.ToArray();
            return values.Length == expected.Length &&
                values.Distinct(StringComparer.Ordinal).Count() ==
                    values.Length &&
                new HashSet<string>(values, StringComparer.Ordinal)
                    .SetEquals(expected);
        }
    }
}
