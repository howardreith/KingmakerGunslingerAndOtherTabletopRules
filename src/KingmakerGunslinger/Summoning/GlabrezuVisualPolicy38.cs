namespace KingmakerGunslinger.Summoning
{
    /// <summary>
    /// Sprint 38 original-body identity. The mechanical donor, the animation
    /// donor and the rendered mesh are three separate things and are recorded
    /// separately: the animation carrier is the same measured free-hands rig
    /// the released Sprint 34-37 outsiders use, and the rendered mesh is this
    /// project's own four-armed pincered demon geometry. The Sprint 38 input
    /// probe found no installed Kingmaker Glabrezu body, so none is reused.
    ///
    /// <para>The measured rig carries two arm chains and this creature has
    /// four limbs. The split is deliberate: the two large <b>pincer</b> arms
    /// ride the real animated arm chains - and their jaws ride the two digit
    /// chains, so the native grip animation opens and closes each claw -
    /// while the two smaller chest arms are project-authored geometry
    /// weighted to the upper torso. Those smaller arms move with the chest
    /// and do not articulate independently. That is a recorded limitation and
    /// the same accepted shape the released four-armed Girallon and Xill take
    /// on a two-armed rig; it is not a claim of four-limb articulation.</para>
    /// </summary>
    internal static class GlabrezuVisualPolicy38
    {
        internal const string Glabrezu = "glabrezu";
        internal const string UnitGuid = "3824e7589ae548f79a3eb8369f50359f";
        internal const string BlueprintName = "KMG_Summoning_Unit_Glabrezu";

        /// <summary>The printed four limbs, and how many of them animate.</summary>
        internal const int VisibleArms = 4;
        internal const int ArticulatedArmChains = 2;

        internal static bool Handles(string key)
        { return key == Glabrezu; }

        internal static bool IsExactIdentity(string key, string guid,
            string name, string prefab)
        {
            return Handles(key) &&
                prefab == TheropodVisualPolicy.DonorPrefabGuid &&
                guid == UnitGuid && name == BlueprintName;
        }
    }
}
