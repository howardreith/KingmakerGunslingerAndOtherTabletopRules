using System;

namespace KingmakerGunslinger.Grit
{
    internal sealed class GritTransactionResult
    {
        internal GritTransactionResult(GritTransactionStatus status,
            GritPoolState before, GritPoolState after, string operationId)
        {
            if (before == null) throw new ArgumentNullException(nameof(before));
            if (after == null) throw new ArgumentNullException(nameof(after));
            Status = status;
            Before = before;
            After = after;
            OperationId = operationId ?? "";
        }

        internal GritTransactionStatus Status { get; private set; }
        internal GritPoolState Before { get; private set; }
        internal GritPoolState After { get; private set; }
        internal string OperationId { get; private set; }

        public override string ToString()
        {
            return "status=" + Status + ";before=" + Before + ";after=" + After +
                ";operation=" + OperationId;
        }
    }
}
