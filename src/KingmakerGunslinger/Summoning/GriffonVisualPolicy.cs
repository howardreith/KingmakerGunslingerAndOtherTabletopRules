using System;
using System.Collections.Generic;
using System.Linq;
namespace KingmakerGunslinger.Summoning
{
    internal static class GriffonVisualPolicy
    {
        internal const string Key="griffon";
        internal const string BlueprintName="KMG_Summoning_Unit_Griffon";
        internal const string BlueprintGuid="4b55d1f8da5e4a3ea23f9a95191f8737";
        internal const string DonorGuid="da807351572c540478b703957d863e68";
        internal const string PrefabGuid="e667b3d46fad6ac44bf36cdd5ab33af8";
        internal static readonly string[] Bones = {
            "Head",
            "Jaw_M",
            "L_Arm_Lower",
            "L_Arm_Upper",
            "L_Foot0",
            "L_Leg0_Lower",
            "L_Leg0_Upper",
            "L_Palm",
            "Neck_M",
            "R_Arm_Lower",
            "R_Arm_Upper",
            "R_Foot0",
            "R_Leg0_Lower",
            "R_Leg0_Upper",
            "R_Palm",
            "Spine1_M",
            "Tail0_M",
            "Tail1_M",
            "Tail2_M",
            "Tail3_M",
            "Torso_Lower",
            "Torso_Upper",
            "backAnkle_L",
            "backAnkle_R",
            "backToes_L",
            "backToes_R",
            "wing01_L",
            "wing01_R",
            "wing02_L",
            "wing02_R",
            "wing03_L",
            "wing03_R",
            "wingD2_L",
            "wingD2_R"
        };
        internal static bool IsPermitted(string key,IEnumerable<string> names)
        {
            if(key!=Key || names==null)return false;
            var actual=names.ToArray();
            return actual.Length==Bones.Length && actual.Distinct(StringComparer.Ordinal).Count()==actual.Length && actual.OrderBy(v=>v,StringComparer.Ordinal).SequenceEqual(Bones.OrderBy(v=>v,StringComparer.Ordinal));
        }
    }
}
