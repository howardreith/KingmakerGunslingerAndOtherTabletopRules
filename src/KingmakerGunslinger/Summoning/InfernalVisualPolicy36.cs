namespace KingmakerGunslinger.Summoning
{
    internal static class InfernalVisualPolicy36
    {
        internal const string Babau = "babau";
        internal const string Hezrou = "hezrou";
        internal const string RigSha256 =
            "727645bba689fc6623b8a690d524d751c96d9072809d4c8ce22e8ed6191ad7a4";
        // The locally retained capture regenerates the released Nalfeshnee
        // vertex/normal/UV/index/weight blob byte-for-byte. Its raw receipt is
        // different; retain truthful capture provenance without admitting an
        // arbitrary rig or rewriting the historical capture identity.
        internal const string EquivalentRigSha256 =
            "6443b580c8019d2a32de616ef610df3c2aa851c7c998044b56e0c29b4494e65d";

        internal static bool IsReviewedRig(string hash)
        { return hash == RigSha256 || hash == EquivalentRigSha256; }
        internal static readonly string[] Bones = {
            "Head", "L_Arm_Lower", "L_Arm_Upper", "L_Bfinger1",
            "L_Bfinger2", "L_Foot0", "L_Leg0_Lower", "L_Leg0_Upper",
            "L_Palm", "L_finger1", "L_finger2", "L_toe",
            "R_Arm_Lower", "R_Arm_Upper", "R_Bfinger1", "R_Bfinger2",
            "R_Foot0", "R_Leg0_Lower", "R_Leg0_Upper", "R_Palm",
            "R_finger1", "R_finger2", "R_toe", "Torso_Lower",
            "Torso_Upper", "neck", "neck1" };

        internal static bool Handles(string key)
        { return key == Babau || key == Hezrou; }

        internal static bool IsExactIdentity(string key, string guid,
            string name, string prefab)
        {
            return Handles(key) &&
                prefab == TheropodVisualPolicy.DonorPrefabGuid &&
                guid == (key == Babau
                    ? "1512476cf6674704b6b0bcc3b67b48d9"
                    : "0c97e35a3e8e4d2db937a23775ea13ac") &&
                name == (key == Babau
                    ? "KMG_Summoning_Unit_Babau"
                    : "KMG_Summoning_Unit_Hezrou");
        }
    }
}
