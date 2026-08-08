using System;
using Kingmaker.Items;
using KingmakerGunslinger.Classes;
using KingmakerGunslinger.Firearms;

namespace KingmakerGunslinger.Misfires
{
    internal static class EffectiveFirearmMisfirePolicy
    {
        internal const int MinimumEffectiveValue = 0;
        internal const int MaximumEffectiveValue = 20;

        internal static int Evaluate(int baseValue, FirearmCondition condition,
            bool trained, ItemEntityWeapon exactWeapon)
        {
            int adjusted = GunTrainingPolicy.EffectiveMisfireValue(
                baseValue, condition, trained);
            int reduction = Enchantments.FirearmMisfireReductionResolver.Resolve(exactWeapon);
            return Math.Max(MinimumEffectiveValue,
                Math.Min(MaximumEffectiveValue, adjusted - reduction));
        }

        internal static bool IsMisfire(int naturalRoll, int threshold)
        {
            if (naturalRoll < 1 || naturalRoll > 20)
                throw new ArgumentOutOfRangeException("naturalRoll");
            if (threshold < MinimumEffectiveValue || threshold > MaximumEffectiveValue)
                throw new ArgumentOutOfRangeException("threshold");
            return threshold != 0 && naturalRoll <= threshold;
        }
    }
}
