using System;
using KingmakerGunslinger.Firearms;

namespace KingmakerGunslinger.Classes
{
    internal static class GunTrainingPolicy
    {
        internal const int UntrainedBrokenIncrease = 4;
        internal const int TrainedBrokenIncrease = 2;

        internal static bool IsRecognizedKind(FirearmKind kind)
        {
            return OfficialFirearmSupport.IsRecognized(kind);
        }

        internal static int DamageBonus(FirearmKind selectedKind,
            FirearmKind firedKind, int dexterityModifier)
        {
            RequireSupported(selectedKind, "selectedKind");
            RequireSupported(firedKind, "firedKind");
            return selectedKind == firedKind ? dexterityModifier : 0;
        }

        internal static int EffectiveMisfireValue(int baseMisfireValue,
            FirearmCondition condition, bool trained)
        {
            if (baseMisfireValue < FirearmDefinition.MinimumMisfireValue ||
                baseMisfireValue > FirearmDefinition.MaximumMisfireValue)
                throw new ArgumentOutOfRangeException("baseMisfireValue");
            if (!Enum.IsDefined(typeof(FirearmCondition), condition))
                throw new ArgumentOutOfRangeException("condition");
            if (condition == FirearmCondition.Wrecked)
                throw new ArgumentException(
                    "A Wrecked firearm cannot enter misfire evaluation.", "condition");
            if (condition != FirearmCondition.Broken)
                return baseMisfireValue;
            int increase = trained ? TrainedBrokenIncrease : UntrainedBrokenIncrease;
            return Math.Min(FirearmDefinition.MaximumMisfireValue,
                baseMisfireValue + increase);
        }

        private static void RequireSupported(FirearmKind kind, string parameter)
        {
            if (!IsRecognizedKind(kind))
                throw new ArgumentOutOfRangeException(parameter);
        }
    }
}
