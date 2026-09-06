using System;

namespace KingmakerGunslinger.ElementalRaces
{
    internal static class ElementalBloodPolicy
    {
        internal const int HealingPerRound = 2;
        internal const int SchemaVersion = 1;

        internal static bool IsBloodTrait(ElementalAlternateTraitId trait)
        {
            return trait == ElementalAlternateTraitId.FireInTheBlood ||
                trait == ElementalAlternateTraitId.StoneInTheBlood ||
                trait == ElementalAlternateTraitId.StormInTheBlood;
        }

        internal static int Maximum(int totalLevel)
        {
            return totalLevel <= 0 ? 0 : (int)Math.Min(int.MaxValue,
                (long)HealingPerRound * totalLevel);
        }

        internal static int Remaining(int totalLevel, int actuallyHealed)
        {
            // Corrupt negative expenditure must not refill a daily allowance.
            return actuallyHealed < 0 ? 0 : Math.Max(0,
                Maximum(totalLevel) - actuallyHealed);
        }

        internal static bool Triggers(ElementalAlternateTraitId trait,
            bool matchingEnergy, int damageBeforeResistance, bool fakeDamage)
        {
            return IsBloodTrait(trait) && matchingEnergy &&
                damageBeforeResistance > 0 && !fakeDamage;
        }

        internal static float CappedHealingModifier(float? nativeModifier,
            int remaining)
        {
            float modifier = nativeModifier ?? 1f;
            if (remaining <= 0 || float.IsNaN(modifier) ||
                float.IsInfinity(modifier) || modifier <= 0f) return 0f;
            // Apply after native AboutToTrigger handlers and before HP changes.
            // Leave ordinary healing suppression/amplification intact below cap.
            return Math.Min(modifier, (float)remaining / HealingPerRound);
        }

        internal static int Spend(int totalLevel, int previous, int received)
        {
            if (received < 0 || received > Remaining(totalLevel, previous))
                throw new ArgumentOutOfRangeException("received",
                    "Only actual healing within the remaining cap may be spent.");
            return checked(previous + received);
        }
    }
}
