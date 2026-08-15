using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace KingmakerGunslinger.BrownFur
{
    internal sealed class BrownFurModifierAdjustmentTracker<TModifier>
        where TModifier : class
    {
        private readonly object _gate = new object();
        private readonly Dictionary<string, HashSet<TModifier>> _adjusted =
            new Dictionary<string, HashSet<TModifier>>(StringComparer.Ordinal);

        internal int ActiveTransactionCount
        { get { lock (_gate) return _adjusted.Count; } }

        internal bool TryAdjust(string transactionIdentity, TModifier modifier,
            BrownFurModifierAdjustmentRequest request,
            out BrownFurModifierAdjustmentDecision decision)
        {
            decision = BrownFurModifierAdjustmentPolicy.Decide(request);
            if (!decision.Eligible) return false;
            if (string.IsNullOrWhiteSpace(transactionIdentity))
            {
                decision = BrownFurModifierAdjustmentDecision.Reject(
                    "modifier-transaction-missing", request);
                return false;
            }
            if (modifier == null)
            {
                decision = BrownFurModifierAdjustmentDecision.Reject(
                    "modifier-instance-missing", request);
                return false;
            }
            lock (_gate)
            {
                HashSet<TModifier> values;
                if (!_adjusted.TryGetValue(transactionIdentity, out values))
                {
                    values = new HashSet<TModifier>(ReferenceComparer.Instance);
                    _adjusted.Add(transactionIdentity, values);
                }
                if (!values.Add(modifier))
                {
                    decision = BrownFurModifierAdjustmentDecision.Reject(
                        "modifier-already-adjusted", request);
                    return false;
                }
            }
            return true;
        }

        internal int AdjustedModifierCount(string transactionIdentity)
        {
            if (string.IsNullOrWhiteSpace(transactionIdentity)) return 0;
            lock (_gate)
            {
                HashSet<TModifier> values;
                return _adjusted.TryGetValue(transactionIdentity, out values) ?
                    values.Count : 0;
            }
        }

        internal bool Release(string transactionIdentity)
        {
            if (string.IsNullOrWhiteSpace(transactionIdentity)) return false;
            lock (_gate) return _adjusted.Remove(transactionIdentity);
        }

        internal void Clear()
        { lock (_gate) _adjusted.Clear(); }

        private sealed class ReferenceComparer : IEqualityComparer<TModifier>
        {
            internal static readonly ReferenceComparer Instance =
                new ReferenceComparer();
            public bool Equals(TModifier left, TModifier right)
            { return ReferenceEquals(left, right); }
            public int GetHashCode(TModifier value)
            { return RuntimeHelpers.GetHashCode(value); }
        }
    }
}
