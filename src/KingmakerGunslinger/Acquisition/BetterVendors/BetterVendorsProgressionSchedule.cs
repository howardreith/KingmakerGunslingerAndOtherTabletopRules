using System;
using System.Linq;

namespace KingmakerGunslinger.Acquisition.BetterVendors
{
    /// <summary>One ordinary enhancement-tier stock operation.</summary>
    internal sealed class BetterVendorsProgressionTier
    {
        internal BetterVendorsProgressionTier(int tier, int militaryRank,
            int quantity)
        {
            if (tier < 1 || tier > ProgressionWeaponCatalog.MaximumEnhancement ||
                militaryRank < 1 ||
                militaryRank > BetterVendorsProgressionSchedule.MaximumMilitaryRank ||
                quantity < 1)
                throw new ArgumentOutOfRangeException("tier");
            Tier = tier;
            MilitaryRank = militaryRank;
            Quantity = quantity;
        }

        /// <summary>The actual enhancement bonus stocked at this milestone.</summary>
        internal int Tier { get; private set; }

        /// <summary>The kingdom Military rank whose stock call adds this tier.</summary>
        internal int MilitaryRank { get; private set; }

        /// <summary>
        /// Copies ADDED per matching item by one stock operation. This is an
        /// addition quantity, never a minimum on-hand or target inventory level.
        /// </summary>
        internal int Quantity { get; private set; }
    }

    /// <summary>
    /// The ordinary magic-weapon schedule of the verified Better Vendors 2.0.8
    /// binary (ProgressionLogic.AddMilitaryStock): Military I adds +1 weapons,
    /// III +2, V +3 (five copies each), VII +4 and IX +5 (two copies each).
    /// Ranks II, IV and X add no weapons. Ranks VI and VIII add only elemental
    /// (corrosive, frost longbow, shock shortbow) queries, which this
    /// integration deliberately never supplements.
    /// </summary>
    internal static class BetterVendorsProgressionSchedule
    {
        internal const int MaximumMilitaryRank = 10;

        private static readonly BetterVendorsProgressionTier[] Tiers =
        {
            new BetterVendorsProgressionTier(1, 1, 5),
            new BetterVendorsProgressionTier(2, 3, 5),
            new BetterVendorsProgressionTier(3, 5, 5),
            new BetterVendorsProgressionTier(4, 7, 2),
            new BetterVendorsProgressionTier(5, 9, 2)
        };

        internal static BetterVendorsProgressionTier[] All
        {
            get { return (BetterVendorsProgressionTier[])Tiers.Clone(); }
        }

        /// <summary>
        /// Maps the rank argument of one Better Vendors Military stock call to
        /// the ordinary enhancement tier that call stocks, if any.
        /// </summary>
        internal static bool TryGetTierForMilitaryStockRank(int militaryRank,
            out BetterVendorsProgressionTier tier)
        {
            tier = Tiers.SingleOrDefault(value =>
                value.MilitaryRank == militaryRank);
            return tier != null;
        }

        internal static BetterVendorsProgressionTier RequireTier(int tier)
        {
            BetterVendorsProgressionTier value = Tiers.SingleOrDefault(
                candidate => candidate.Tier == tier);
            if (value == null) throw new ArgumentOutOfRangeException("tier");
            return value;
        }

        /// <summary>
        /// Tiers whose milestone has been reached at the given Military rank.
        /// Future tiers are never included; a negative rank unlocks nothing.
        /// </summary>
        internal static BetterVendorsProgressionTier[] UnlockedTiers(
            int militaryRank)
        {
            return Tiers.Where(value => value.MilitaryRank <= militaryRank)
                .ToArray();
        }
    }
}
