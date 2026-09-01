using System;

namespace KingmakerGunslinger.Ammunition
{
    /// <summary>
    /// The one project-owned economy rule for batches of firearm ammunition.
    /// Ammunition is made at ten percent of its normal batch retail value,
    /// rounded up so a valid batch can never be free.
    /// </summary>
    internal static class AmmunitionCraftingCostPolicy
    {
        internal const int DiscountPercent = 10;
        internal const float CraftMagicItemsPriceScale = 0.60f;

        internal static int ForBatch(int unitRetailValue, int count)
        {
            if (unitRetailValue < 0) throw new ArgumentOutOfRangeException(
                "unitRetailValue");
            if (count < 1) throw new ArgumentOutOfRangeException("count");
            long retail = checked((long)unitRetailValue * count);
            return Math.Max(1, checked((int)((retail * DiscountPercent +
                99L) / 100L)));
        }

        internal static int ForCombinedBatch(int firstUnitRetailValue,
            int secondUnitRetailValue, int count)
        {
            if (firstUnitRetailValue < 0 || secondUnitRetailValue < 0)
                throw new ArgumentOutOfRangeException("unitRetailValue");
            return ForBatch(checked(firstUnitRetailValue +
                secondUnitRetailValue), count);
        }
    }
}
