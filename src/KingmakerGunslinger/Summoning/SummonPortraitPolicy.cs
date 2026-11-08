using System;
using System.Collections.Generic;
using System.Linq;

namespace KingmakerGunslinger.Summoning
{
    /// <summary>
    /// Repair R3 UI-01. The turn-based initiative tracker shows
    /// <c>unit.Portrait.SmallPortrait</c>, which for a summon falls back to
    /// its blueprint's <c>PortraitSafe</c>. KMG summon units are cloned from
    /// native donor units and so inherited the donor's portrait (Dire Rat a
    /// dog/wolf, Lantern Archon a helmet, most outsiders a Lizardfolk). Each
    /// registered KMG summon unit now owns a portrait made from its own
    /// approved 128 px creature art; donor portrait objects are never touched.
    /// </summary>
    internal static class SummonPortraitPolicy
    {
        internal const int IconSize = 128;
        // Native small-portrait aspect (185 x 242). The square creature art is
        // cropped to a centred column of that aspect so the portrait frame
        // does not stretch it.
        internal const double NativeSmallPortraitAspect = 185d / 242d;
        internal const string PortraitNamePrefix = "KMG_SummonPortrait_";

        internal static string PortraitName(string creatureKey)
        {
            if (string.IsNullOrEmpty(creatureKey))
                throw new ArgumentException("A creature key is required.", "creatureKey");
            return PortraitNamePrefix + creatureKey;
        }

        /// <summary>x, y, width, height of the centred portrait-aspect crop.</summary>
        internal static int[] CropRect(int size)
        {
            if (size < 8) throw new ArgumentOutOfRangeException("size");
            int width = (int)Math.Round(size * NativeSmallPortraitAspect);
            if (width % 2 != size % 2) width -= 1;
            return new[] { (size - width) / 2, 0, width, size };
        }

        /// <summary>The art key each summon unit's portrait uses: its own.</summary>
        internal static string ArtKey(SummonCreatureSpec creature)
        {
            if (creature == null) throw new ArgumentNullException("creature");
            return creature.Key;
        }

        /// <summary>Every catalog creature with a registered KMG unit.</summary>
        internal static IEnumerable<SummonCreatureSpec> PortraitCreatures()
        {
            return ExpandedSummoningCatalog.All.Where(SummonIconCatalog.IsRegisteredSomewhere);
        }
    }
}
