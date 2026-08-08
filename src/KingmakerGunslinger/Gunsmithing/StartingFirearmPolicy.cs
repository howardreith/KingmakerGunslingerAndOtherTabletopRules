namespace KingmakerGunslinger.Gunsmithing
{
    internal enum StartingFirearmProfile
    {
        BaseDefault,
        Pistolero,
        MusketMaster,
        ExplicitMusket
    }

    internal static class StartingFirearmPolicy
    {
        internal static StartingFirearmProfile Resolve(bool hasMusketMaster,
            bool hasPistolero, bool hasExplicitMusketChoice)
        {
            if (hasMusketMaster) return StartingFirearmProfile.MusketMaster;
            if (hasPistolero) return StartingFirearmProfile.Pistolero;
            if (hasExplicitMusketChoice) return StartingFirearmProfile.ExplicitMusket;
            return StartingFirearmProfile.BaseDefault;
        }

        internal static bool ExpectsMusket(StartingFirearmProfile profile)
        {
            return profile == StartingFirearmProfile.MusketMaster ||
                profile == StartingFirearmProfile.ExplicitMusket;
        }
    }
}
