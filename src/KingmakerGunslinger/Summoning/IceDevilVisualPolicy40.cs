namespace KingmakerGunslinger.Summoning
{
    internal static class IceDevilVisualPolicy40
    {
        internal const string Key = "ice-devil";
        internal const string BlueprintName = "KMG_Summoning_Unit_IceDevil";
        internal static bool Handles(string key) { return key == Key; }
        internal static bool IsExactIdentity(string key, string guid, string name, string prefab)
        {
            return Handles(key) && guid == IceDevilRules40Policy.UnitGuid &&
                name == BlueprintName && prefab == TheropodVisualPolicy.DonorPrefabGuid;
        }
    }
}
