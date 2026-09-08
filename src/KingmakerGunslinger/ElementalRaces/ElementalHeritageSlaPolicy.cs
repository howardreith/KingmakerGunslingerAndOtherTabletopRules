using System;

namespace KingmakerGunslinger.ElementalRaces
{
    internal static class ElementalHeritageSlaPolicy
    {
        internal static int UnerringConfirmationBonus(int casterLevel)
        {
            if (casterLevel < 1) return 0;
            return 2 + Math.Min(5, casterLevel / 4);
        }

        internal static int ChillTouchCount(int casterLevel)
        {
            return Math.Max(1, casterLevel);
        }

        internal static int ChillTouchUndeadPanicRounds(int d4Result,
            int casterLevel)
        {
            if (d4Result < 1 || d4Result > 4)
                throw new ArgumentOutOfRangeException("d4Result");
            return d4Result + Math.Max(1, casterLevel);
        }

        internal static bool ExactDeliveryMatch(string armedGuid,
            string heldGuid, string executingGuid)
        {
            if (string.IsNullOrWhiteSpace(armedGuid) ||
                string.IsNullOrWhiteSpace(heldGuid) ||
                string.IsNullOrWhiteSpace(executingGuid))
                return false;
            return string.Equals(armedGuid, heldGuid,
                       StringComparison.Ordinal) &&
                string.Equals(armedGuid, executingGuid,
                    StringComparison.Ordinal);
        }
    }
}
