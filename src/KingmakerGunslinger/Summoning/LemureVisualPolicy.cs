using System;
using System.Collections.Generic;
using System.Linq;

namespace KingmakerGunslinger.Summoning
{
    internal static class LemureVisualPolicy
    {
        internal const string Key = "lemure";
        internal const string BlueprintName = "KMG_Summoning_Unit_Lemure";
        internal const string BlueprintGuid = "80fd27e965b943b5b307a8c0a45cfb96";
        internal const string RigSha256 = "727645bba689fc6623b8a690d524d751c96d9072809d4c8ce22e8ed6191ad7a4";
        internal static readonly string[] Bones = {
            "Head", "L_Arm_Lower", "L_Arm_Upper", "L_Bfinger2", "L_Palm",
            "L_finger1", "L_finger2", "R_Arm_Lower", "R_Arm_Upper",
            "R_Bfinger2", "R_Palm", "R_finger1", "R_finger2",
            "Torso_Lower", "Torso_Upper", "neck", "neck1"
        };
        internal static bool IsPermitted(string key, IEnumerable<string> bones)
        {
            if (key != Key || bones == null) return false;
            string[] names = bones.ToArray();
            return names.Length == Bones.Length &&
                names.Distinct(StringComparer.Ordinal).Count() == names.Length &&
                new HashSet<string>(names, StringComparer.Ordinal).SetEquals(Bones);
        }
        internal static bool IsExactIdentity(string guid, string name, string prefab)
        {
            return guid == BlueprintGuid && name == BlueprintName &&
                prefab == TheropodVisualPolicy.DonorPrefabGuid;
        }
    }
}
