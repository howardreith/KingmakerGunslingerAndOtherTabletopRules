using System;

namespace KingmakerGunslinger.Summoning
{
    internal enum SummonTemplateBand
    {
        Low,
        Mid,
        High
    }

    internal static class SummonTemplateBandPolicy
    {
        internal static SummonTemplateBand Select(int hitDice)
        {
            if (hitDice < 0) throw new ArgumentOutOfRangeException("hitDice");
            return hitDice < 5 ? SummonTemplateBand.Low :
                hitDice <= 10 ? SummonTemplateBand.Mid : SummonTemplateBand.High;
        }

        internal static int ResistanceValue(SummonTemplateBand band)
        {
            return band == SummonTemplateBand.High ? 10 : 5;
        }

        internal static bool GrantsSpellResistance(SummonTemplateBand band)
        {
            return band != SummonTemplateBand.Low;
        }
    }
}
