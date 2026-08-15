using System;

namespace KingmakerGunslinger.BrownFur
{
    internal sealed class BrownFurReservoirDebitResult
    {
        internal BrownFurReservoirDebitResult(bool success, string failure,
            int before, int observedAfterSpend, int finalAmount,
            bool rollbackAttempted, bool rollbackSucceeded)
        {
            Success = success;
            Failure = failure ?? string.Empty;
            Before = before;
            ObservedAfterSpend = observedAfterSpend;
            FinalAmount = finalAmount;
            RollbackAttempted = rollbackAttempted;
            RollbackSucceeded = rollbackSucceeded;
        }

        internal bool Success { get; private set; }
        internal string Failure { get; private set; }
        internal int Before { get; private set; }
        internal int ObservedAfterSpend { get; private set; }
        internal int FinalAmount { get; private set; }
        internal bool RollbackAttempted { get; private set; }
        internal bool RollbackSucceeded { get; private set; }
    }

    internal static class BrownFurExactDebitPolicy
    {
        internal static BrownFurReservoirDebitResult TryDebitExact(int cost,
            Func<bool> containsResource, Func<int> readAmount,
            Func<int, bool> hasEnough, Action<int> spend, Action<int> restore)
        {
            if (cost < 0) return Reject("reservoir-cost-invalid");
            if (cost == 0)
                return new BrownFurReservoirDebitResult(true, string.Empty,
                    0, 0, 0, false, false);
            if (containsResource == null || readAmount == null ||
                hasEnough == null || spend == null || restore == null)
                return Reject("reservoir-accessor-missing");

            int before = -1;
            int observed = -1;
            try
            {
                if (!containsResource())
                    return Reject("reservoir-not-owned");
                before = readAmount();
                if (before < cost || !hasEnough(cost))
                    return new BrownFurReservoirDebitResult(false,
                        "reservoir-insufficient", before, before, before,
                        false, false);
                spend(cost);
                observed = readAmount();
                if (observed == before - cost)
                    return new BrownFurReservoirDebitResult(true, string.Empty,
                        before, observed, observed, false, false);
                return RollBack("reservoir-debit-mismatch", before, observed,
                    readAmount, spend, restore);
            }
            catch (Exception exception)
            {
                if (before < 0)
                    return Reject("reservoir-debit-exception:" +
                        exception.GetType().FullName);
                try { observed = readAmount(); }
                catch { observed = -1; }
                return RollBack("reservoir-debit-exception:" +
                    exception.GetType().FullName, before, observed,
                    readAmount, spend, restore);
            }
        }

        private static BrownFurReservoirDebitResult RollBack(string failure,
            int before, int observed, Func<int> readAmount, Action<int> spend,
            Action<int> restore)
        {
            bool succeeded = false;
            int final = observed;
            try
            {
                if (observed >= 0 && observed < before)
                    restore(before - observed);
                else if (observed > before)
                    spend(observed - before);
                final = readAmount();
                succeeded = final == before;
            }
            catch
            {
                try { final = readAmount(); }
                catch { final = -1; }
            }
            return new BrownFurReservoirDebitResult(false, failure, before,
                observed, final, true, succeeded);
        }

        private static BrownFurReservoirDebitResult Reject(string failure)
        {
            return new BrownFurReservoirDebitResult(false, failure, -1, -1,
                -1, false, false);
        }
    }
}
