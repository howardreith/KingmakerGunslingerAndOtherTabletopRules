using System;
namespace KingmakerGunslinger.Summoning
{
    internal static class InfernalVisualPolicy34
    {
        internal const string Barbazu = "bearded-devil";
        internal const string Kyton = "kyton-chain-devil";
        internal const string RigSha256 = "727645bba689fc6623b8a690d524d751c96d9072809d4c8ce22e8ed6191ad7a4";
        internal static readonly string[] Bones = { "Head", "L_Arm_Lower", "L_Arm_Upper", "L_Bfinger1", "L_Bfinger2", "L_Foot0", "L_Leg0_Lower", "L_Leg0_Upper", "L_Palm", "L_finger1", "L_finger2", "L_toe", "R_Arm_Lower", "R_Arm_Upper", "R_Bfinger1", "R_Bfinger2", "R_Foot0", "R_Leg0_Lower", "R_Leg0_Upper", "R_Palm", "R_finger1", "R_finger2", "R_toe", "Torso_Lower", "Torso_Upper", "neck", "neck1" };
        internal static bool Handles(string key) { return key == Barbazu || key == Kyton; }
        internal static bool IsExactIdentity(string key, string guid, string name, string prefab)
        {
            return Handles(key) && prefab == TheropodVisualPolicy.DonorPrefabGuid &&
                guid == (key == Barbazu ? "182032f380314a7581d0016c443945a2" : "96045f5c18ef4f4396a6c466a2be1a92") &&
                name == (key == Barbazu ? "KMG_Summoning_Unit_BeardedDevil" : "KMG_Summoning_Unit_KytonChainDevil");
        }
    }
}
