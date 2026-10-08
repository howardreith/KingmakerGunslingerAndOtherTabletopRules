namespace KingmakerGunslinger.Summoning
{
    /// <summary>
    /// The Giant Ant (Soldier)'s printed sting poison, as pure policy.
    ///
    /// <para>Primary source: sting, injury; Fortitude DC 14; frequency
    /// 1/round for 4 rounds; effect 1d2 Strength damage; cure 1 save. Unlike
    /// the Giant Wasp, the ant has no racial bonus to its poison's save DC, so
    /// the printed 14 is exactly what the standard formula produces on a
    /// 2-hit-die creature with a +3 Constitution modifier: ten, plus half its
    /// hit dice, plus that modifier.</para>
    ///
    /// <para>Scaling the DC rather than writing 14 down is deliberate. If a
    /// later change to the chassis moved the creature's Constitution or hit
    /// dice, a constant would keep printing 14 and quietly stop matching the
    /// stat block it came from, whereas this stops matching the rules gate
    /// instead, which is where it should be caught.</para>
    /// </summary>
    internal static class GiantAntPoisonPolicy
    {
        internal const int HitDice = 2;
        internal const int RacialDcBonus = 0;
        internal const int Exposures = 4;
        internal const int SavesToCure = 1;

        internal static int DifficultyClass(int constitutionBonus)
        {
            return 10 + HitDice / 2 + constitutionBonus + RacialDcBonus;
        }
    }
}
