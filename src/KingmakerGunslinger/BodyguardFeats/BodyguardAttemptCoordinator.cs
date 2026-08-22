using System;

namespace KingmakerGunslinger.BodyguardFeats
{
    internal sealed class BodyguardAttemptExecution
    {
        internal BodyguardAttemptExecution(bool spent, bool rollAttempted,
            BodyguardAidResult result, Exception fault)
        {
            Spent = spent;
            RollAttempted = rollAttempted;
            Result = result;
            Fault = fault;
        }
        internal bool Spent { get; private set; }
        internal bool RollAttempted { get; private set; }
        internal BodyguardAidResult Result { get; private set; }
        internal Exception Fault { get; private set; }
    }

    /// <summary>
    /// Enforces the irreversible Bodyguard order: native AoO spend first, Aid
    /// result second. A committed spend is never refunded because the roll fails,
    /// throws, or the incoming attack later misses.
    /// </summary>
    internal static class BodyguardAttemptCoordinator
    {
        internal static BodyguardAttemptExecution Execute(string protectorId,
            int attackBonus, Func<bool> trySpend, Func<int> rollD20)
        {
            if (string.IsNullOrWhiteSpace(protectorId))
                throw new ArgumentException("A protector identity is required.",
                    "protectorId");
            if (trySpend == null) throw new ArgumentNullException("trySpend");
            if (rollD20 == null) throw new ArgumentNullException("rollD20");

            bool spent;
            try { spent = trySpend(); }
            catch (Exception exception)
            { return new BodyguardAttemptExecution(false, false, null, exception); }
            if (!spent)
                return new BodyguardAttemptExecution(false, false, null, null);
            try
            {
                int natural = rollD20();
                return new BodyguardAttemptExecution(true, true,
                    new BodyguardAidResult(protectorId, natural, attackBonus),
                    null);
            }
            catch (Exception exception)
            { return new BodyguardAttemptExecution(true, true, null, exception); }
        }
    }
}
