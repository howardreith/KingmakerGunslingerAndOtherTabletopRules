using System;

namespace KingmakerGunslinger.BrownFur
{
    /// <summary>
    /// Couples queued-point reservation, exact commit debit, and cast
    /// lifecycle cleanup so no caller can retain only one half of the protocol.
    /// </summary>
    internal sealed class BrownFurCastCommitCoordinator<TOwner, TCommand,
        TAbility, TRule, TContext, TProcess>
        where TOwner : class where TCommand : class where TAbility : class
        where TRule : class where TContext : class where TProcess : class
    {
        private readonly BrownFurReservoirReservationLedger<TOwner>
            _reservations = new BrownFurReservoirReservationLedger<TOwner>();
        private readonly BrownFurCastLifecycleTracker<TCommand, TAbility,
            TRule, TContext, TProcess> _lifecycle;
        private readonly Action<BrownFurCastTransaction> _onRelease;

        internal BrownFurCastCommitCoordinator() : this(null) { }

        internal BrownFurCastCommitCoordinator(
            Action<BrownFurCastTransaction> onRelease)
        {
            _onRelease = onRelease;
            _lifecycle = new BrownFurCastLifecycleTracker<TCommand, TAbility,
                TRule, TContext, TProcess>(Release);
        }

        internal int ActiveTransactionCount
        { get { return _lifecycle.ActiveTransactionCount; } }
        internal int ReservationCount
        { get { return _reservations.ReservationCount; } }
        internal int ReservedPoints(TOwner owner)
        { return _reservations.ReservedPoints(owner); }

        internal bool Begin(TOwner owner, TCommand command, TAbility ability,
            BrownFurCastTransaction transaction, int availablePoints)
        {
            if (owner == null || transaction == null ||
                transaction.State != BrownFurCastTransactionState.Validated)
                return false;
            string identity = transaction.Intent.TransactionIdentity;
            int cost = transaction.Decision.ReservoirCost;
            if (!_reservations.TryReserve(owner, identity, cost,
                    availablePoints)) return false;
            if (_lifecycle.Begin(command, ability, transaction)) return true;
            _reservations.Release(owner, identity);
            return false;
        }

        internal bool AttachRule(TAbility ability, TRule rule,
            TContext context)
        { return _lifecycle.AttachRule(ability, rule, context); }

        internal bool AttachProcess(TRule rule, TProcess process)
        { return _lifecycle.AttachProcess(rule, process); }

        internal bool Commit(TOwner owner, TAbility ability,
            Func<int, bool> debitExactly)
        {
            if (owner == null || ability == null ||
                debitExactly == null) return false;
            BrownFurCastTransaction transaction;
            if (!_lifecycle.TryGetByAbility(ability, out transaction))
                return false;
            string identity = transaction.Intent.TransactionIdentity;
            return _lifecycle.Commit(ability, expectedCost =>
                _reservations.TryCommit(owner, identity, reservedCost =>
                    reservedCost == expectedCost && debitExactly(reservedCost)));
        }

        internal bool EndCommand(TCommand command, bool interrupted)
        { return _lifecycle.EndCommand(command, interrupted); }

        internal bool ProcessTerminal(TProcess process, bool failed)
        { return _lifecycle.ProcessTerminal(process, failed); }

        internal bool TryGetByContext(TContext context,
            out BrownFurCastTransaction transaction)
        { return _lifecycle.TryGetByContext(context, out transaction); }

        internal bool TryGetByAbility(TAbility ability,
            out BrownFurCastTransaction transaction)
        { return _lifecycle.TryGetByAbility(ability, out transaction); }

        internal bool TryGetByRule(TRule rule,
            out BrownFurCastTransaction transaction)
        { return _lifecycle.TryGetByRule(rule, out transaction); }

        internal bool FailRule(TRule rule)
        { return _lifecycle.FailRule(rule); }

        internal void Clear()
        {
            try { _lifecycle.Clear(); }
            finally { _reservations.Clear(); }
        }

        private void Release(BrownFurCastTransaction transaction)
        {
            if (transaction == null) return;
            _reservations.Release(transaction.Intent.TransactionIdentity);
            if (_onRelease != null) _onRelease(transaction);
        }
    }
}
