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

        /// <summary>The lowest value a favored-class reduction can produce.</summary>
        internal const int FavoredClassFloor = 1;

        internal static int Evaluate(int baseValue, FirearmCondition condition,
            bool trained, AmmunitionId loadedAmmunition,
            int exactWeaponReduction)
        {
            return Evaluate(baseValue, condition, trained, loadedAmmunition,
                exactWeaponReduction, 0);
        }

        /// <summary>
        /// The authoritative threshold including a favored-class misfire
        /// reduction (G01/G18). It applies after every other modifier to the
        /// same unclamped value, never lowers the result below 1 and never
        /// raises a value another rule already reduced to 0; the result is
        /// then clamped to 20 as before.
        /// </summary>
        internal static int Evaluate(int baseValue, FirearmCondition condition,
            bool trained, AmmunitionId loadedAmmunition,
            int exactWeaponReduction, int favoredClassReduction)
        {
            if (exactWeaponReduction < 0)
                throw new ArgumentOutOfRangeException("exactWeaponReduction");
            if (favoredClassReduction < 0)
                throw new ArgumentOutOfRangeException("favoredClassReduction");
            int adjusted = GunTrainingPolicy.EffectiveMisfireValue(
                baseValue, condition, trained);
            int ammunitionModifier = ReloadAmmunitionProfileCatalog
                .Require(loadedAmmunition).MisfireModifier;
            int unclamped = adjusted + ammunitionModifier - exactWeaponReduction;
            if (unclamped <= MinimumEffectiveValue)
                return MinimumEffectiveValue;
            int reduced = favoredClassReduction == 0 ? unclamped :
                Math.Max(FavoredClassFloor, unclamped - favoredClassReduction);
            return Math.Min(MaximumEffectiveValue, reduced);
        }
    }
}
