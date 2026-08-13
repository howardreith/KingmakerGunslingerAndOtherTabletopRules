using System;

namespace KingmakerGunslinger.Summoning
{
    internal static class SummonTemplateSmitePolicy
    {
        private const int GoodBit = 2;
        private const int EvilBit = 4;

        internal static bool IsEligible(bool smitesEvil, int targetAlignment)
        {
            return (targetAlignment & (smitesEvil ? EvilBit : GoodBit)) != 0;
        }

        internal static int AttackBonus(int charismaBonus)
        {
            return Math.Max(0, charismaBonus);
        }

        internal static int DamageBonus(int hitDice)
        {
            if (hitDice < 0) throw new ArgumentOutOfRangeException("hitDice");
            return hitDice;
        }
    }
}
