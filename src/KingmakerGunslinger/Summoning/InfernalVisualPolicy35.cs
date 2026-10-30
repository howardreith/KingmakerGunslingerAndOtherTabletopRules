namespace KingmakerGunslinger.Summoning
{
    internal static class InfernalVisualPolicy35
    {
        internal const string Barbed = "barbed-devil";
        internal const string Bone = "bone-devil";
        internal const string RigSha256 =
            "727645bba689fc6623b8a690d524d751c96d9072809d4c8ce22e8ed6191ad7a4";
        internal static readonly string[] Bones = {
            "Head", "L_Arm_Lower", "L_Arm_Upper", "L_Bfinger1",
            "L_Bfinger2", "L_Foot0", "L_Leg0_Lower", "L_Leg0_Upper",
            "L_Palm", "L_finger1", "L_finger2", "L_toe",
            "R_Arm_Lower", "R_Arm_Upper", "R_Bfinger1", "R_Bfinger2",
            "R_Foot0", "R_Leg0_Lower", "R_Leg0_Upper", "R_Palm",
            "R_finger1", "R_finger2", "R_toe", "Torso_Lower",
            "Torso_Upper", "neck", "neck1" };

        internal static bool Handles(string key)
        { return key == Barbed || key == Bone; }

        internal static bool IsExactIdentity(string key, string guid,
            string name, string prefab)
        {
            return Handles(key) &&
                prefab == TheropodVisualPolicy.DonorPrefabGuid &&
                guid == (key == Barbed
                    ? "a8ed4e16ec4b48a5bdfdc47f7f1b9cd0"
                    : "74652de839a94feba3c2fb67b536437c") &&
                name == (key == Barbed
                    ? "KMG_Summoning_Unit_BarbedDevil"
                    : "KMG_Summoning_Unit_BoneDevil");
        }
    }
}
