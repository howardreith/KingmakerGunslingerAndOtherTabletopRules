namespace KingmakerGunslinger.Summoning
{
    /// <summary>
    /// Sprint 37 original-body identity. The mechanical donor, the animation
    /// donor and the rendered mesh are three separate things and are recorded
    /// separately: the animation carrier is the same measured free-hands rig
    /// the released Sprint 34–36 outsiders use, and the rendered mesh is this
    /// project's own vulture-headed winged demon geometry. No installed
    /// Kingmaker Vrock body exists, so none is reused.
    ///
    /// <para>The measured rig carries no wing bones, so the wings are
    /// project-authored geometry weighted to the upper torso. Rigid wings are
    /// recorded visual backlog, not a claim of articulated flight.</para>
    /// </summary>
    internal static class VrockVisualPolicy37
    {
        internal const string Vrock = "vrock";
        internal const string UnitGuid = "5f4aa097fc3e4fa4a627ce5d5be6509e";
        internal const string BlueprintName = "KMG_Summoning_Unit_Vrock";

        internal static bool Handles(string key)
        { return key == Vrock; }

        internal static bool IsExactIdentity(string key, string guid,
            string name, string prefab)
        {
            return Handles(key) &&
                prefab == TheropodVisualPolicy.DonorPrefabGuid &&
                guid == UnitGuid && name == BlueprintName;
        }
    }
}
