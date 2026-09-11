namespace KingmakerGunslinger.Spells.Teleportation
{
    // Native invariant restored by the specialist cache reconciliation: a spell
    // that a memorization book already knows and that sits in one of the book's
    // attached school special lists belongs in the book's cached special spells.
    // Native derives this membership only at feature activation (AddSpecialList)
    // and at learn time (AddKnown); a save whose knowledge predates a later list
    // publication keeps a stale cache, so the invariant is re-derived on load.
    internal static class TeleportSpecialistSpellCachePolicy
    {
        // AllSpellsKnown books self-heal in native PostLoad (TryRestoreKnownSpells
        // restores missing special-list spells), so they never need repair here.
        internal static bool ShouldRestoreSpecialMembership(bool allSpellsKnown, bool spellKnown,
            bool inAttachedSpecialList, bool alreadySpecial)
        { return !allSpellsKnown && spellKnown && inAttachedSpecialList && !alreadySpecial; }
    }
}
