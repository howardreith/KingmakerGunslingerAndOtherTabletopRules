namespace KingmakerGunslinger.Reloading
{
    internal enum FirearmReloadPlanStatus
    {
        Unknown = 0, Available = 1, MissingContext = 2,
        IncompatibleAmmunition = 3, Wrecked = 4, AlreadyLoaded = 5,
        MixedAmmunition = 6, MissingAmmunition = 7
    }
}
