namespace KingmakerGunslinger.Firing
{
    internal enum BrokenSequenceConstructionDecision
    {
        Allow = 0,
        AllowAndConsume = 1,
        RejectInterrupted = 2
    }

    /// <summary>
    /// Pure decisions for stopping an attack sequence after a verified
    /// committed degradation of the exact firearm. The misfiring shot itself
    /// always finishes resolving under the existing rules; these gates only
    /// prevent the next real shot and its automatic continuations.
    /// Suppression is released ONLY by a genuine new player-issued attack
    /// order for the same executor and target — repairing or reloading the
    /// firearm restores readiness but never resurrects the cancelled order,
    /// while any later deliberate order works normally and re-enables
    /// ordinary automatic behavior for that weapon.
    /// </summary>
    internal static class BrokenSequenceInterruptionPolicy
    {
        internal static BrokenSequenceConstructionDecision EvaluateConstruction(
            bool sequenceSuppressed,
            bool playerIssuedOrder)
        {
            if (!sequenceSuppressed)
            {
                return BrokenSequenceConstructionDecision.Allow;
            }

            return playerIssuedOrder
                ? BrokenSequenceConstructionDecision.AllowAndConsume
                : BrokenSequenceConstructionDecision.RejectInterrupted;
        }

        /// <summary>
        /// A native full attack ends its remaining iterations once the exact
        /// firearm suffered a committed degradation during this command; the
        /// committed break's own misfire consequence has already resolved.
        /// </summary>
        internal static bool ShouldEndFullAttack(bool committedBreakDuringCommand)
        {
            return committedBreakDuringCommand;
        }

        /// <summary>
        /// A captured reload-resume continuation may run only while the exact
        /// weapon has suffered no committed degradation since the
        /// continuation was captured.
        /// </summary>
        internal static bool MayResumeCapturedAttack(
            int degradationEpochAtCapture,
            int degradationEpochNow)
        {
            return degradationEpochAtCapture == degradationEpochNow;
        }
    }
}
