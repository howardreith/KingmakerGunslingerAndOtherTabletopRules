using System;
using System.Collections.Generic;
using System.Linq;

namespace KingmakerGunslinger.Summoning
{
    /// <summary>The reviewed original mesh contract on the Monitor Lizard rig.
    /// Bone names only; native bind transforms stay private and are resolved
    /// from the actual donor renderer at attach time.</summary>
    internal static class CrocodilianVisualPolicy
    {
        internal static readonly string[] Keys = { "crocodile", "dire-crocodile" };
        internal static readonly string[] Bones =
        {
            "cent_ass1_jnt", "cent_head1_jnt", "cent_jaw1_jnt",
            "cent_neck1_jnt", "cent_neck2_jnt",
            "cent_spine1_jnt", "cent_spine2_jnt", "cent_spine3_jnt",
            "cent_tail1_jnt", "cent_tail2_jnt", "cent_tail3_jnt",
            "cent_tail4_jnt", "cent_tail5_jnt", "cent_tail6_jnt", "cent_tail7_jnt",
            "left_arm1_jnt", "left_foot1_jnt", "left_hand1_jnt",
            "left_hand2_jnt", "left_leg1_jnt", "left_leg2_jnt",
            "right_arm1_jnt", "right_foot1_jnt", "right_hand1_jnt",
            "right_hand2_jnt", "right_leg1_jnt", "right_leg2_jnt"
        };

        // A cosmetic approach, never mechanical reach or unit movement.
        internal static float ContactApproach(float gap, float weight)
        {
            if (float.IsNaN(gap) || float.IsInfinity(gap) || gap <= 0f ||
                float.IsNaN(weight) || float.IsInfinity(weight) || weight <= 0f) return 0f;
            return Math.Min(gap, 0.25f * Math.Min(weight, 1f));
        }

        internal static bool IsPermitted(string key, IEnumerable<string> bones)
        {
            if (!Keys.Contains(key, StringComparer.Ordinal) || bones == null)
                return false;
            string[] actual = bones.ToArray();
            // Missing jaw/limb/tail drivers are as invalid as extra ones.
            return actual.Length == Bones.Length &&
                actual.Distinct(StringComparer.Ordinal).Count() == actual.Length &&
                new HashSet<string>(actual, StringComparer.Ordinal).SetEquals(Bones);
        }
    }
}
