using System;
using System.Linq;
using KingmakerGunslinger.Spells.Teleportation;

namespace KingmakerGunslinger.DomainTests
{
    internal static partial class TeleportationContextTests
    {
        private sealed class Resource : ITeleportCastResource
        {
            public object Evidence() { return new { kind = "fake" }; }
            internal bool[] Prepared = { true, true };
            internal int Slots = 2;
            internal int SpendCalls, RefundCalls;
            internal bool Spontaneous, ThrowAfterSpend, FailRefund, ExtraSpend;
            internal int Count { get { return Spontaneous ? Slots : Prepared.Count(value => value); } }
            public void Spend()
            {
                SpendCalls++;
                if (Spontaneous) Slots--; else Prepared[1] = false;
                if (ExtraSpend) { if (Spontaneous) Slots--; else Prepared[0] = false; }
                if (ThrowAfterSpend) throw new InvalidOperationException("native spender threw after exact expenditure");
            }
            public TeleportExpenditure ObserveExpenditure()
            { return Count == 2 ? TeleportExpenditure.None : Count == 1 ? TeleportExpenditure.ExactlyOne : TeleportExpenditure.Ambiguous; }
            public bool RestoreAndVerifyExactResource()
            {
                RefundCalls++;
                if (FailRefund || ObserveExpenditure() != TeleportExpenditure.ExactlyOne) return false;
                if (Spontaneous) Slots++; else Prepared[1] = true;
                return Count == 2;
            }
        }
        private sealed class Execution : ITeleportCastExecution
        {
            internal readonly Resource Resource = new Resource();
            internal bool Valid = true, ThrowBeforeEffect, ThrowAfterEffect, ThrowDuringRecording;
            internal int Validations, Executions, Records;
            internal Func<Action, TeleportExecutionResult> Run;
            public ITeleportCastResource RevalidateAndCapture(WorldMapPointSpellAction action, out string diagnostic)
            { Validations++; diagnostic = Valid ? "current" : "stale destination/caster/book"; return Valid ? Resource : null; }
            public void RecordResult(WorldMapPointSpellAction action, TeleportExecutionResult result)
            { Records++; if (ThrowDuringRecording) throw new InvalidOperationException("logging failed after rules result"); }
            public TeleportExecutionResult Execute(WorldMapPointSpellAction action, Action materialEffectStarting)
            {
                Executions++;
                if (ThrowBeforeEffect) throw new InvalidOperationException("pre-effect technical failure");
                if (Run != null) return Run(materialEffectStarting);
                materialEffectStarting();
                if (ThrowAfterEffect) throw new InvalidOperationException("post-effect technical failure");
                return new TeleportExecutionResult(TeleportExecutionStatus.Arrived, TeleportOutcomeKind.OnTarget, Target, 1, 0, null);
            }
        }
        internal static void CancellationAndStalenessSpendNothing()
        {
            var cancelled = new TeleportCastTransaction(ActionRow());
            var execution = new Execution();
            Assertions.Equal(0, execution.Resource.SpendCalls, "Opening/selecting an action does not spend.");
            cancelled.Cancel(); cancelled.Confirm(execution); cancelled.Cancel();
            Assertions.Equal(0, execution.Validations, "Cancel settles the request before any resource capture.");
            Assertions.Equal(0, execution.Resource.SpendCalls, "Cancelled confirmation spends zero.");
            execution = new Execution { Valid = false };
            var transaction = new TeleportCastTransaction(ActionRow()); transaction.Confirm(execution);
            Assertions.Equal(TeleportTransactionState.Invalidated, transaction.State, "Failed current destination/caster/book validation rejects the cast.");
            Assertions.Equal(0, execution.Resource.SpendCalls, "No stale resource expenditure.");
        }
        internal static void PreparedAndSpontaneousSpendOnce()
        {
            foreach (bool spontaneous in new[] { false, true })
            {
                var execution = new Execution(); execution.Resource.Spontaneous = spontaneous;
                var transaction = new TeleportCastTransaction(ActionRow(kind: spontaneous ? TeleportCastSourceKind.Spontaneous : TeleportCastSourceKind.Prepared));
                transaction.Confirm(execution); transaction.Confirm(execution); transaction.Cancel();
                Assertions.Equal(TeleportTransactionState.Completed, transaction.State, "One settled cast.");
                Assertions.Equal(1, execution.Resource.Count, "Exactly one current selected-source use.");
                Assertions.Equal(1, execution.Resource.SpendCalls, "Duplicate callback cannot double-spend.");
                Assertions.Equal(1, execution.Executions, "Duplicate callback cannot duplicate relocation.");
                Assertions.Equal(0, execution.Resource.RefundCalls, "Legitimate cast remains spent.");
            }
        }
        internal static void ProvenPreEffectFailureCompensates()
        {
            foreach (bool spenderThrows in new[] { false, true })
            foreach (bool spontaneous in new[] { false, true })
            {
                var execution = new Execution { ThrowBeforeEffect = true };
                execution.Resource.Spontaneous = spontaneous; execution.Resource.ThrowAfterSpend = spenderThrows;
                var transaction = new TeleportCastTransaction(ActionRow()); transaction.Confirm(execution); transaction.Confirm(execution);
                Assertions.Equal(TeleportTransactionState.TechnicalFailureCompensated, transaction.State, "Exact pre-effect capture is restored and verified.");
                Assertions.Equal(2, execution.Resource.Count, "Exact selected resource restored.");
                Assertions.Equal(1, execution.Resource.RefundCalls, "Compensation is idempotent.");
            }
        }
        internal static void AmbiguousOrPostEffectFailureNeverRefunds()
        {
            var after = new Execution { ThrowAfterEffect = true };
            var transaction = new TeleportCastTransaction(ActionRow()); transaction.Confirm(after);
            Assertions.Equal(TeleportTransactionState.TechnicalFailureSpent, transaction.State, "Possible world-state effect prevents refund.");
            Assertions.Equal(0, after.Resource.RefundCalls, "No post-effect compensation attempt.");
            var ambiguous = new Execution(); ambiguous.Resource.ExtraSpend = true;
            transaction = new TeleportCastTransaction(ActionRow()); transaction.Confirm(ambiguous);
            Assertions.Equal(TeleportTransactionState.AmbiguousExpenditure, transaction.State, "Ambiguous expenditure fails closed.");
            Assertions.Equal(0, ambiguous.Executions, "No free/ambiguous cast effect.");
            Assertions.Equal(0, ambiguous.Resource.RefundCalls, "No guessed compensation.");
            var failed = new Execution { ThrowBeforeEffect = true }; failed.Resource.FailRefund = true;
            transaction = new TeleportCastTransaction(ActionRow()); transaction.Confirm(failed);
            Assertions.Equal(TeleportTransactionState.TechnicalFailureSpent, transaction.State, "Failed restoration is never logged as compensated.");
        }
    }
}