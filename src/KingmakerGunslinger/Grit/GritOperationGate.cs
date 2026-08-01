using System;
using System.Collections.Generic;

namespace KingmakerGunslinger.Grit
{
    internal sealed class GritOperationGate
    {
        private readonly HashSet<string> _applied =
            new HashSet<string>(StringComparer.Ordinal);

        internal bool WasApplied(string operationId)
        {
            Validate(operationId);
            return _applied.Contains(operationId);
        }

        internal void MarkApplied(string operationId)
        {
            Validate(operationId);
            _applied.Add(operationId);
        }

        private static void Validate(string operationId)
        {
            if (string.IsNullOrWhiteSpace(operationId))
                throw new ArgumentException("Operation identity is required.",
                    nameof(operationId));
        }
    }
}
