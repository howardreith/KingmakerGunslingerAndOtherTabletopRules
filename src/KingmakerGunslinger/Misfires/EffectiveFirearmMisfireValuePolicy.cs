using System;
using KingmakerGunslinger.Ammunition;
using KingmakerGunslinger.Classes;
using KingmakerGunslinger.Firearms;

namespace KingmakerGunslinger.Misfires
{
    internal static class EffectiveFirearmMisfireValuePolicy
    {
        internal const int MinimumEffectiveValue = 0;
        internal const int MaximumEffectiveValue = 20;

        internal static int Evaluate(int baseValue, FirearmCondition condition,
            bool trained, AmmunitionId loadedAmmunition,
            int exactWeaponReduction)
        {
            if (exactWeaponReduction < 0)
                throw new ArgumentOutOfRangeException("exactWeaponReduction");
            int adjusted = GunTrainingPolicy.EffectiveMisfireValue(
                baseValue, condition, trained);
            int ammunitionModifier = ReloadAmmunitionProfileCatalog
                .Require(loadedAmmunition).MisfireModifier;
            return Math.Max(MinimumEffectiveValue,
                Math.Min(MaximumEffectiveValue,
                    adjusted + ammunitionModifier - exactWeaponReduction));
        }
    }
}
