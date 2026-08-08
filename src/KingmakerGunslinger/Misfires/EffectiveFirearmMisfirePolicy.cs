using System;
using Kingmaker.Items;
using KingmakerGunslinger.Ammunition;
using KingmakerGunslinger.Classes;
using KingmakerGunslinger.Firearms;

namespace KingmakerGunslinger.Misfires
{
    internal static class EffectiveFirearmMisfirePolicy
    {
        internal const int MinimumEffectiveValue =
            EffectiveFirearmMisfireValuePolicy.MinimumEffectiveValue;
        internal const int MaximumEffectiveValue =
            EffectiveFirearmMisfireValuePolicy.MaximumEffectiveValue;

        internal static int Evaluate(int baseValue, FirearmCondition condition,
            bool trained, ItemEntityWeapon exactWeapon)
        {
            return Evaluate(baseValue, condition, trained, exactWeapon,
                ReloadAmmunitionProfileCatalog.LooseBasic.LoadedAmmunition);
        }

        internal static int Evaluate(int baseValue, FirearmCondition condition,
            bool trained, ItemEntityWeapon exactWeapon,
            AmmunitionId loadedAmmunition)
        {
            int reduction = Enchantments.FirearmMisfireReductionResolver.Resolve(exactWeapon);
            return EvaluateWithReduction(baseValue, condition, trained,
                loadedAmmunition, reduction);
        }

        internal static int EvaluateWithReduction(int baseValue,
            FirearmCondition condition, bool trained,
            AmmunitionId loadedAmmunition, int exactWeaponReduction)
        {
            return EffectiveFirearmMisfireValuePolicy.Evaluate(baseValue,
                condition, trained, loadedAmmunition, exactWeaponReduction);
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
