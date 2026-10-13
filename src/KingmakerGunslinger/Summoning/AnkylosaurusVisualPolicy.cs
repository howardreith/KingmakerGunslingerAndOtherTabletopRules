using System;
using System.Collections.Generic;
using System.Linq;

namespace KingmakerGunslinger.Summoning
{
    internal static class AnkylosaurusVisualPolicy
    {
        internal const string BlueprintName = "KMG_Summoning_Unit_Ankylosaurus";
        internal const string DonorGuid = "c3524f96954a1d94f8525b86e7626633";
        internal static readonly string[] Bones = new[] {
            "LowerTorso", "Spine1_M", "UpperTorso", "Neck_M", "Head", "Jaw_M",
            "Tail0_M", "Tail1_M", "Tail2_M", "Tail3_M", "Tail4_M", "Tail5_M",
            "LeftUpperArm", "LeftLowerArm", "LeftHand", "LeftUpperLeg0", "LeftLowerLeg0", "LeftFoot0",
            "RightUpperArm", "RightLowerArm", "RightHand", "RightUpperLeg0", "RightLowerLeg0", "RightFoot0"
        };
        internal static bool IsPermitted(string key, IEnumerable<string> names)
        {
            if (key != AnkylosaurusRulesPolicy.Key || names == null) return false;
            var values = names.ToArray();
            return values.Length == Bones.Length && values.Distinct(StringComparer.Ordinal).Count() == values.Length &&
                new HashSet<string>(values, StringComparer.Ordinal).SetEquals(Bones);
        }
    }
}
