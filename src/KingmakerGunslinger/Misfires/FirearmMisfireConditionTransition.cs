namespace KingmakerGunslinger.Misfires
{
    /// <summary>
    /// The bounded item-owned condition mutation caused by one detected firearm
    /// misfire after the already-loaded round has discharged.
    /// </summary>
    internal enum FirearmMisfireConditionTransition
    {
        None = 0,
        NormalToBroken = 1,
        BrokenToWrecked = 2
    }
}
