namespace KingmakerGunslinger.Spells.Teleportation
{
    // Pure offer/execute policy for destination actions. Three native facts
    // drive it, kept separate on purpose:
    //   - an unrelated active modal blocks EVERY action, direct or confirmed;
    //   - a merely unavailable confirmation presenter blocks only spells that
    //     need a confirmation (Greater Teleport settles directly);
    //   - an in-flight cast blocks everything until it settles.
    internal static class TeleportBeginPolicy
    {
        internal static bool CanExecuteAction(bool castInFlight, bool unrelatedModalShown,
            bool confirmationPresenterAvailable, TeleportSpellKind spell)
        {
            if (castInFlight || unrelatedModalShown) return false;
            if (spell == TeleportSpellKind.GreaterTeleport) return true;
            return confirmationPresenterAvailable;
        }
        // Rows compose when at least one offered action can execute; actions
        // that cannot (e.g. ordinary Teleport with no presenter) are filtered
        // out rather than rejecting the whole collection.
        internal static bool OffersAnyAction(bool castInFlight, bool unrelatedModalShown,
            bool confirmationPresenterAvailable, TeleportSpellKind[] spells)
        {
            if (spells == null || spells.Length == 0) return false;
            foreach (TeleportSpellKind spell in spells)
                if (CanExecuteAction(castInFlight, unrelatedModalShown, confirmationPresenterAvailable, spell)) return true;
            return false;
        }
    }
}
