using System;

namespace KingmakerGunslinger.Acquisition
{
    /// <summary>
    /// Tabletop magic-weapon price for a normal-material weapon: mundane base,
    /// one masterwork premium, and 2,000 gp times the square of the complete
    /// enchantment package's equivalent bonus. The existing authored items
    /// (Pistol +1 3,300 gp, Duelist's Rebuttal 19,300 gp, Roadwarden
    /// 33,800 gp, the Eastern and spear +1 items) already follow this rule;
    /// the helper only makes it explicit for derived variants.
    /// </summary>
    internal static class MagicWeaponPricing
    {
        internal const int MasterworkPremium = 300;
        internal const int EnhancementPriceFactor = 2000;
        internal const int MaximumActualEnhancement = 5;
        internal const int MaximumEquivalentBonus = 10;

        internal static int EquivalentBonus(int actualEnhancement,
            int additionalEquivalentBonus)
        {
            if (actualEnhancement < 1 ||
                actualEnhancement > MaximumActualEnhancement)
                throw new ArgumentOutOfRangeException("actualEnhancement");
            if (additionalEquivalentBonus < 0)
                throw new ArgumentOutOfRangeException(
                    "additionalEquivalentBonus");
            int total = actualEnhancement + additionalEquivalentBonus;
            if (total > MaximumEquivalentBonus)
                throw new ArgumentOutOfRangeException(
                    "additionalEquivalentBonus");
            return total;
        }

        /// <summary>
        /// Price of a masterwork-quality magic weapon. The masterwork premium
        /// is counted exactly once; pass the MUNDANE base cost, never the cost
        /// of an already-priced variant.
        /// </summary>
        internal static int Cost(int mundaneBaseCost, int equivalentBonus)
        {
            if (mundaneBaseCost < 0)
                throw new ArgumentOutOfRangeException("mundaneBaseCost");
            if (equivalentBonus < 1 || equivalentBonus > MaximumEquivalentBonus)
                throw new ArgumentOutOfRangeException("equivalentBonus");
            checked
            {
                return mundaneBaseCost + MasterworkPremium +
                    EnhancementPriceFactor * equivalentBonus * equivalentBonus;
            }
        }
    }
}
