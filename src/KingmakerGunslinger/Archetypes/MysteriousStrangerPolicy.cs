using System;

namespace KingmakerGunslinger.Archetypes
{
    internal static class MysteriousStrangerPolicy
    {
        internal static readonly int[] LuckyLevels = { 2, 6, 10, 14, 18 };
        internal static int GritMaximum(int charismaModifier)
        { return Math.Max(1, charismaModifier); }
        internal static int FocusedAimBonus(int charismaModifier, int deadShotHits)
        {
            if (deadShotHits < 1) throw new ArgumentOutOfRangeException("deadShotHits");
            return Math.Max(1, charismaModifier) * deadShotHits;
        }
        internal static bool CanActivateFocusedAim(int currentGrit,
            int effectiveCost, bool ownsAbility, bool alreadyArmed)
        {
            if (currentGrit < 0) throw new ArgumentOutOfRangeException("currentGrit");
            if (effectiveCost < 0) throw new ArgumentOutOfRangeException("effectiveCost");
            return ownsAbility && !alreadyArmed && currentGrit > 0 &&
                currentGrit >= effectiveCost;
        }
        internal static int FocusedAimGritAfter(int currentGrit,
            int effectiveCost)
        {
            if (!CanActivateFocusedAim(currentGrit, effectiveCost, true, false))
                throw new InvalidOperationException(
                    "Focused Aim cannot commit the requested Grit transaction.");
            return currentGrit - effectiveCost;
        }
        internal static int LuckyBonus(int gunslingerLevel)
        {
            if (gunslingerLevel < 1 || gunslingerLevel > 20)
                throw new ArgumentOutOfRangeException("gunslingerLevel");
            return gunslingerLevel < 2 ? 0 : Math.Min(5, 1 + (gunslingerLevel - 2) / 4);
        }
        internal static int FortuneUses(int charismaModifier)
        { return Math.Max(0, charismaModifier); }
        internal static int ClippingShotDamage(int rolledDamage)
        {
            if (rolledDamage < 0) throw new ArgumentOutOfRangeException("rolledDamage");
            return rolledDamage / 2;
        }
    }
}
