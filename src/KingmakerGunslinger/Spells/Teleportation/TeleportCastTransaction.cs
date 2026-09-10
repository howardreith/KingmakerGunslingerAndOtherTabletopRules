using System;
using System.Threading;

namespace KingmakerGunslinger.Spells.Teleportation
{
    internal enum TeleportExpenditure { None, ExactlyOne, Ambiguous }
    internal enum TeleportTransactionState
    {
        Pending, Cancelled, Invalidated, Validating, Committed, Completed,
        TechnicalFailureUnspent, TechnicalFailureCompensated, TechnicalFailureSpent, AmbiguousExpenditure
    }
    internal interface ITeleportCastResource
    {
        // Capture happens only after current destination/state/caster/book revalidation.
        // This lease owns the exact slot/pool snapshot and can prove its post-state.
        void Spend();
        TeleportExpenditure ObserveExpenditure();
        bool RestoreAndVerifyExactResource();
        object Evidence();
    }
    internal interface ITeleportCastExecution
    {
        ITeleportCastResource RevalidateAndCapture(WorldMapPointSpellAction action, out string diagnostic);
        TeleportExecutionResult Execute(WorldMapPointSpellAction action, Action materialEffectStarting);
        void RecordResult(WorldMapPointSpellAction action, TeleportExecutionResult result);
    }
    internal sealed class TeleportCastTransaction
    {
        private readonly WorldMapPointSpellAction _action;
        private int _settled;
        internal TeleportCastTransaction(WorldMapPointSpellAction action)
        { _action = action ?? throw new ArgumentNullException("action"); State = TeleportTransactionState.Pending; }
        internal TeleportTransactionState State { get; private set; }
        internal string Diagnostic { get; private set; }
        internal bool MaterialEffectStarted { get; private set; }
        internal TeleportExecutionResult Result { get; private set; }
        internal void Cancel()
        {
            if (Interlocked.CompareExchange(ref _settled, 1, 0) == 0) State = TeleportTransactionState.Cancelled;
        }
        internal void Confirm(ITeleportCastExecution execution)
        {
            if (execution == null) throw new ArgumentNullException("execution");
            if (Interlocked.CompareExchange(ref _settled, 1, 0) != 0) return;
            State = TeleportTransactionState.Validating;
            ITeleportCastResource resource = null;
            bool spendAttempted = false;
            try
            {
                string diagnostic;
                resource = execution.RevalidateAndCapture(_action, out diagnostic);
                Diagnostic = diagnostic;
                if (resource == null) { State = TeleportTransactionState.Invalidated; return; }
                spendAttempted = true;
                resource.Spend();
                TeleportExpenditure expenditure = resource.ObserveExpenditure();
                if (expenditure != TeleportExpenditure.ExactlyOne)
                {
                    State = expenditure == TeleportExpenditure.None ? TeleportTransactionState.TechnicalFailureUnspent :
                        TeleportTransactionState.AmbiguousExpenditure;
                    Diagnostic = "Resource expenditure was not exactly one; no spell effect executed.";
                    return;
                }
                State = TeleportTransactionState.Committed;
                Result = execution.Execute(_action, () => MaterialEffectStarted = true);
                if (Result == null) throw new InvalidOperationException("Cast execution returned no result.");
                // Rules-level failures (including no alternate or defensive cap) stay spent.
                State = TeleportTransactionState.Completed;
                execution.RecordResult(_action, Result);
            }
            catch (Exception exception)
            {
                Diagnostic = exception.GetType().FullName + ": " + exception.Message;
                if (!spendAttempted) { State = TeleportTransactionState.TechnicalFailureUnspent; return; }
                // Mark before invoking any potentially effectful native call. Uncertain partial
                // effects must never be refunded, even if their native call subsequently throws.
                if (MaterialEffectStarted || Result != null) { State = TeleportTransactionState.TechnicalFailureSpent; return; }
                try
                {
                    TeleportExpenditure expenditure = resource.ObserveExpenditure();
                    if (expenditure == TeleportExpenditure.None) State = TeleportTransactionState.TechnicalFailureUnspent;
                    else if (expenditure == TeleportExpenditure.ExactlyOne)
                        State = resource.RestoreAndVerifyExactResource() ? TeleportTransactionState.TechnicalFailureCompensated :
                            TeleportTransactionState.TechnicalFailureSpent;
                    else State = TeleportTransactionState.AmbiguousExpenditure;
                }
                catch (Exception compensationException)
                {
                    State = TeleportTransactionState.AmbiguousExpenditure;
                    Diagnostic += "; compensation=" + compensationException.GetType().FullName + ": " + compensationException.Message;
                }
            }
        }
    }
}
