namespace KingmakerGunslinger.Summoning
{
    internal static class GiantWaspPoisonPolicy
    {
        internal const int HitDice = 4;
        internal const int RacialDcBonus = 2;
        internal const int Exposures = 6;
        internal const int SavesToCure = 1;

        internal static int DifficultyClass(int constitutionBonus)
        {
            return 10 + HitDice / 2 + constitutionBonus + RacialDcBonus;
        }
    }
}
