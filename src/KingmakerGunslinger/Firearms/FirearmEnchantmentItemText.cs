namespace KingmakerGunslinger.Firearms
{
    /// <summary>
    /// Shared player-facing item description sentences for the Reliable and
    /// Seeking firearm weapon properties. One source of truth for the authored
    /// magic items, the merchant catalog and the vendor-progression items.
    /// </summary>
    internal static class FirearmEnchantmentItemText
    {
        internal const string Reliable =
            "This weapon's misfire value is 1 lower than it would otherwise be, to a minimum of 0. A natural 1 still misses.";
        internal const string Seeking =
            "This weapon's attacks ignore the miss chance from concealment. It grants no ability to see or target creatures the wielder could not otherwise target, and other defenses still apply.";
    }
}
