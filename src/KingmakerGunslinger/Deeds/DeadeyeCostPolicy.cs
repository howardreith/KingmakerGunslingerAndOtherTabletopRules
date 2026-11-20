namespace KingmakerGunslinger.Deeds
{
    /// <summary>
    /// POL-02 / S4 T4, owner decision: Deadeye keeps the documented flat-cost
    /// CRPG adaptation. Tabletop Deadeye charges one grit per range increment
    /// beyond the first, decided at the shot; this mod arms Deadeye as a free
    /// action before the attack's target and distance are known, so arming
    /// costs one ordinary grit, adjusted only by True Grit, and is charged
    /// once when the activation commits. The shot's range increment decides
    /// only whether the touch-AC benefit applies (DeadeyeService); it never
    /// changes the cost. This is the single authority for availability,
    /// debit and any displayed cost.
    /// </summary>
    internal static class DeadeyeCostPolicy
    {
        /// <summary>The ordinary arming cost, before True Grit.</summary>
        internal const int OrdinaryArmingCost = 1;

        /// <summary>The arming decision for a caster with this much grit, with or without True Grit (Deadeye).</summary>
        internal static TrueGritDecision Arming(int currentGrit, bool trueGritSelected)
        {
            return new TrueGritService().Evaluate(new TrueGritRequest(
                currentGrit, OrdinaryArmingCost, trueGritSelected, false));
        }
    }
}
