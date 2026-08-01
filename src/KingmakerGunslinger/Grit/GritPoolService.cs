using System;

namespace KingmakerGunslinger.Grit
{
    internal sealed class GritPoolService
    {
        internal int CalculateMaximum(int wisdomModifier, int bonusMaximum)
        {
            if (bonusMaximum < 0)
                throw new ArgumentOutOfRangeException(nameof(bonusMaximum));
            return checked(Math.Max(1, wisdomModifier) + bonusMaximum);
        }

        internal GritPoolState ResetDaily(int wisdomModifier, int bonusMaximum)
        {
            int maximum = CalculateMaximum(wisdomModifier, bonusMaximum);
            return new GritPoolState(maximum, maximum);
        }

        internal GritPoolState ReconcileMaximum(GritPoolState state,
            int wisdomModifier, int bonusMaximum)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));
            int maximum = CalculateMaximum(wisdomModifier, bonusMaximum);
            return new GritPoolState(Math.Min(state.Current, maximum), maximum);
        }

        internal GritTransactionResult Spend(GritPoolState state, int cost,
            string operationId, GritOperationGate gate)
        {
            ValidateTransaction(state, cost, operationId, gate, nameof(cost));
            if (gate.WasApplied(operationId))
                return new GritTransactionResult(GritTransactionStatus.Duplicate,
                    state, state, operationId);
            if (state.Current < cost)
                return new GritTransactionResult(GritTransactionStatus.Insufficient,
                    state, state, operationId);
            var after = new GritPoolState(state.Current - cost, state.Maximum);
            gate.MarkApplied(operationId);
            return new GritTransactionResult(GritTransactionStatus.Applied,
                state, after, operationId);
        }

        internal GritTransactionResult Restore(GritPoolState state, int amount,
            string operationId, GritOperationGate gate)
        {
            ValidateTransaction(state, amount, operationId, gate, nameof(amount));
            if (gate.WasApplied(operationId))
                return new GritTransactionResult(GritTransactionStatus.Duplicate,
                    state, state, operationId);
            if (state.Current == state.Maximum)
                return new GritTransactionResult(GritTransactionStatus.AtMaximum,
                    state, state, operationId);
            var after = new GritPoolState(
                (int)Math.Min((long)state.Maximum, (long)state.Current + amount),
                state.Maximum);
            gate.MarkApplied(operationId);
            return new GritTransactionResult(GritTransactionStatus.Applied,
                state, after, operationId);
        }

        private static void ValidateTransaction(GritPoolState state, int amount,
            string operationId, GritOperationGate gate, string amountName)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));
            if (amount <= 0) throw new ArgumentOutOfRangeException(amountName);
            if (gate == null) throw new ArgumentNullException(nameof(gate));
            gate.WasApplied(operationId);
        }
    }
}
