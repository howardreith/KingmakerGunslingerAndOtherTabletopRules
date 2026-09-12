using System;
using KingmakerGunslinger.Firearms;

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
    /// prevent the next real shot and its automatic continuations. A genuine
    /// player-issued attack order or a repaired weapon always passes.
    /// </summary>
    internal static class BrokenSequenceInterruptionPolicy
    {
        /// <summary>
        /// Gate for a newly constructed attack command while the exact
        /// weapon's preceding sequence was interrupted by a committed break.
        /// Automatic recreation (brain re-issue, reload-resume work, confused
        /// stalkers) is rejected; a player-issued order passes and consumes
        /// the suppression. A weapon no longer damaged is never gated, so a
        /// later repair such as Quick Clear cannot lock future firing.
        /// </summary>
        internal static BrokenSequenceConstructionDecision EvaluateConstruction(
            bool sequenceSuppressed,
            bool playerIssuedContext,
            FirearmCondition actualCondition)
        {
            if (!Enum.IsDefined(typeof(FirearmCondition), actualCondition))
            {
                throw new ArgumentOutOfRangeException("actualCondition");
            }

            if (!sequenceSuppressed)
            {
                return BrokenSequenceConstructionDecision.Allow;
            }

            if (actualCondition == FirearmCondition.Normal)
            {
                return BrokenSequenceConstructionDecision.AllowAndConsume;
            }

            return playerIssuedContext
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
