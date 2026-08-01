namespace KingmakerGunslinger.Grit
{
    internal enum GritRecoveryStatus
    {
        Eligible = 0,
        NotQualifyingOutcome = 1,
        NotExactFirearm = 2,
        NotInCombat = 3,
        InvalidTarget = 4,
        HelplessOrUnawareTarget = 5,
        InsignificantTarget = 6
    }
}
