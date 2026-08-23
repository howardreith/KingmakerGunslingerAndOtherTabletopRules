using System;
using System.Collections.Generic;
using System.Linq;

namespace KingmakerGunslinger.BodyguardFeats
{
    internal sealed class BodyguardAttackCandidate<T> where T : class
    {
        internal BodyguardAttackCandidate(T attack, string identity,
            int targetAwareAttackBonus, int stableSlotOrder, bool qualifies)
        {
            Attack = attack ?? throw new ArgumentNullException("attack");
            if (string.IsNullOrWhiteSpace(identity))
                throw new ArgumentException("An attack identity is required.",
                    "identity");
            if (stableSlotOrder < 0) throw new ArgumentOutOfRangeException(
                "stableSlotOrder");
            Identity = identity;
            TargetAwareAttackBonus = targetAwareAttackBonus;
            StableSlotOrder = stableSlotOrder;
            Qualifies = qualifies;
        }

        internal T Attack { get; private set; }
        internal string Identity { get; private set; }
        internal int TargetAwareAttackBonus { get; private set; }
        internal int StableSlotOrder { get; private set; }
        internal bool Qualifies { get; private set; }
    }

    internal static class BodyguardAttackSelectionPolicy
    {
        internal static BodyguardAttackCandidate<T> Select<T>(
            IEnumerable<BodyguardAttackCandidate<T>> candidates) where T : class
        {
            if (candidates == null) throw new ArgumentNullException("candidates");
            BodyguardAttackCandidate<T>[] values = candidates.ToArray();
            if (values.Any(value => value == null))
                throw new ArgumentException("An attack candidate is null.",
                    "candidates");
            return values.Where(value => value.Qualifies)
                .OrderByDescending(value => value.TargetAwareAttackBonus)
                .ThenBy(value => value.StableSlotOrder)
                .ThenBy(value => value.Identity, StringComparer.Ordinal)
                .FirstOrDefault();
        }
    }
}
