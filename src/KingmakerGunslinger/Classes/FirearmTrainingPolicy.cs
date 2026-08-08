using System;
using KingmakerGunslinger.Firearms;

namespace KingmakerGunslinger.Classes
{
    internal sealed class FirearmTrainingEntitlement
    {
        internal FirearmTrainingEntitlement(bool eligible, int damageBonus,
            bool reducedBrokenMisfire)
        {
            Eligible = eligible;
            DamageBonus = damageBonus;
            ReducedBrokenMisfire = reducedBrokenMisfire;
        }

        internal bool Eligible { get; private set; }
        internal int DamageBonus { get; private set; }
        internal bool ReducedBrokenMisfire { get; private set; }
    }

    internal static class FirearmTrainingPolicy
    {
        internal static FirearmTrainingEntitlement Evaluate(FirearmKind kind,
            int dexterityModifier, bool hasExactKindTraining,
            int pistolTrainingRank, int musketTrainingRank)
        {
            if (!GunTrainingPolicy.IsSupportedKind(kind))
                throw new ArgumentOutOfRangeException("kind");
            if (pistolTrainingRank < 0 || pistolTrainingRank > 4)
                throw new ArgumentOutOfRangeException("pistolTrainingRank");
            if (musketTrainingRank < 0 || musketTrainingRank > 4)
                throw new ArgumentOutOfRangeException("musketTrainingRank");
            int best = int.MinValue;
            if (hasExactKindTraining) best = dexterityModifier;
            if (pistolTrainingRank > 0 && FirearmHandednessPolicy.Matches(kind,
                    FirearmHandedness.OneHanded))
                best = Math.Max(best, dexterityModifier + pistolTrainingRank - 1);
            if (musketTrainingRank > 0 && FirearmHandednessPolicy.Matches(kind,
                    FirearmHandedness.TwoHanded))
                best = Math.Max(best, dexterityModifier + musketTrainingRank - 1);
            bool eligible = best != int.MinValue;
            return new FirearmTrainingEntitlement(eligible,
                eligible ? best : 0, eligible);
        }
    }
}
