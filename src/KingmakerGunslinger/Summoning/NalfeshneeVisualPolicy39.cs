namespace KingmakerGunslinger.Summoning
{
    /// <summary>
    /// Sprint 39 original-body identity. The mechanical donor, the animation
    /// donor and the rendered mesh are three separate things and are recorded
    /// separately: the animation carrier is the same measured free-hands rig
    /// the released Sprint 34-38 outsiders use, and the rendered mesh is this
    /// project's own corpulent boar-headed winged demon geometry. The Sprint
    /// 39 input observation found no installed Kingmaker Nalfeshnee unit and
    /// no Nalfeshnee-named asset of any type, so none is reused.
    ///
    /// <para>The measured rig carries two arm chains and no wing chain. The
    /// split is deliberate: the two <b>arms</b> ride the real animated arm
    /// chains - and their claws ride the two digit chains, so the native grip
    /// animation still opens and closes each hand - while the two
    /// <b>wings</b> are project-authored geometry weighted to the upper
    /// torso. Those wings move with the chest and do not articulate
    /// independently. That is a recorded limitation and the same accepted
    /// shape the released Sprint 37 Vrock's wings take; it is not a claim of
    /// wing articulation.</para>
    /// </summary>
    internal static class NalfeshneeVisualPolicy39
    {
        internal const string Nalfeshnee = "nalfeshnee";
        internal const string UnitGuid = "9930f78084f0fe78d1de849aed58b753";
        internal const string BlueprintName = "KMG_Summoning_Unit_Nalfeshnee";

        /// <summary>The printed limbs, and how many of them animate.</summary>
        internal const int VisibleArms = 2;
        internal const int ArticulatedArmChains = 2;
        /// <summary>
        /// Two wings, and neither of them articulates. Recorded as a number
        /// beside the arms so an observer compares like with like rather than
        /// inferring that a winged creature must have wing bones.
        /// </summary>
        internal const int VisibleWings = 2;

        internal static bool Handles(string key)
        { return key == Nalfeshnee; }

        internal static bool IsExactIdentity(string key, string guid,
            string name, string prefab)
        {
            return Handles(key) &&
                prefab == TheropodVisualPolicy.DonorPrefabGuid &&
                guid == UnitGuid && name == BlueprintName;
        }
    }
}
