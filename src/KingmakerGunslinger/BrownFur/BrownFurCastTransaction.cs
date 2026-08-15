using System;

namespace KingmakerGunslinger.BrownFur
{
    internal enum BrownFurCastTransactionState
    {
        Created = 0,
        Validated = 1,
        Rejected = 2,
        Committed = 3,
        Completed = 4,
        Cancelled = 5,
        Interrupted = 6,
        Failed = 7
    }

    internal sealed class BrownFurCastTransaction
    {
        internal BrownFurCastTransaction(BrownFurCastIntent intent)
        {
            Intent = intent ?? throw new ArgumentNullException("intent");
            State = BrownFurCastTransactionState.Created;
        }

        internal BrownFurCastIntent Intent { get; private set; }
        internal BrownFurCastDecision Decision { get; private set; }
        internal BrownFurCastTransactionState State { get; private set; }
        internal int DebitedReservoirPoints { get; private set; }

        internal bool Validate(BrownFurCastDecision decision)
        {
            if (decision == null || State != BrownFurCastTransactionState.Created)
                return false;
            Decision = decision;
            State = decision.Eligible ? BrownFurCastTransactionState.Validated :
                BrownFurCastTransactionState.Rejected;
            return decision.Eligible;
        }

        internal bool Commit(Func<int, bool> tryDebitExactly)
        {
            if (State != BrownFurCastTransactionState.Validated ||
                tryDebitExactly == null) return false;
            int cost = Decision.ReservoirCost;
            if (cost < 0 || cost != Intent.ExpectedReservoirCost ||
                !tryDebitExactly(cost))
            {
                State = BrownFurCastTransactionState.Rejected;
                return false;
            }
            DebitedReservoirPoints = cost;
            State = BrownFurCastTransactionState.Committed;
            return true;
        }

        internal bool Complete()
        {
            if (State != BrownFurCastTransactionState.Committed) return false;
            State = BrownFurCastTransactionState.Completed;
            return true;
        }

        internal bool Cancel()
        {
            if (State != BrownFurCastTransactionState.Created &&
                State != BrownFurCastTransactionState.Validated) return false;
            State = BrownFurCastTransactionState.Cancelled;
            return true;
        }

        internal bool Interrupt()
        {
            if (State == BrownFurCastTransactionState.Created ||
                State == BrownFurCastTransactionState.Validated)
            {
                State = BrownFurCastTransactionState.Cancelled;
                return true;
            }
            if (State != BrownFurCastTransactionState.Committed) return false;
            State = BrownFurCastTransactionState.Interrupted;
            return true;
        }

        internal bool Fail()
        {
            if (State == BrownFurCastTransactionState.Completed ||
                State == BrownFurCastTransactionState.Cancelled ||
                State == BrownFurCastTransactionState.Interrupted ||
                State == BrownFurCastTransactionState.Rejected) return false;
            State = BrownFurCastTransactionState.Failed;
            return true;
        }
    }
}
