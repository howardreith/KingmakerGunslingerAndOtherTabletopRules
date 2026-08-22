using System;
using System.Collections.Generic;
using System.Linq;

namespace KingmakerGunslinger.BodyguardFeats
{
    internal enum BodyguardAttackStage
    {
        Begun = 0,
        BodyguardResolved,
        AttackResolved,
        Intercepted,
        Completed,
        Faulted
    }

    internal sealed class BodyguardAttackFrame
    {
        private readonly HashSet<string> _attempted =
            new HashSet<string>(StringComparer.Ordinal);
        private readonly List<BodyguardAidResult> _attempts =
            new List<BodyguardAidResult>();
        private readonly HashSet<object> _armorClassEvents =
            new HashSet<object>(ReferenceEqualityComparer.Instance);

        internal BodyguardAttackFrame(string attackIdentity, string attackerId,
            string originalTargetId)
        {
            if (string.IsNullOrWhiteSpace(attackIdentity) ||
                string.IsNullOrWhiteSpace(attackerId) ||
                string.IsNullOrWhiteSpace(originalTargetId))
                throw new ArgumentException(
                    "Attack, attacker, and original-target identities are required.");
            AttackIdentity = attackIdentity;
            AttackerId = attackerId;
            OriginalTargetId = originalTargetId;
            FinalTargetId = originalTargetId;
            Stage = BodyguardAttackStage.Begun;
        }

        internal string AttackIdentity { get; private set; }
        internal string AttackerId { get; private set; }
        internal string OriginalTargetId { get; private set; }
        internal string FinalTargetId { get; private set; }
        internal BodyguardAttackStage Stage { get; private set; }
        internal bool? AttackHit { get; private set; }
        internal string InterceptorId { get; private set; }
        internal IReadOnlyList<BodyguardAidResult> Attempts { get { return _attempts; } }
        internal int ArmorClassBonus
        { get { return BodyguardAidPolicy.StackArmorClassBonus(_attempts); } }

        internal bool TryRecordAttempt(BodyguardAidResult attempt)
        {
            if (attempt == null) throw new ArgumentNullException("attempt");
            if (Stage != BodyguardAttackStage.Begun ||
                !_attempted.Add(attempt.ProtectorId)) return false;
            _attempts.Add(attempt);
            return true;
        }

        internal void FinishBodyguard()
        {
            if (Stage != BodyguardAttackStage.Begun)
                throw new InvalidOperationException(
                    "Bodyguard resolution can finish only once.");
            Stage = BodyguardAttackStage.BodyguardResolved;
        }

        internal bool TryApplyArmorClass(object armorClassEvent)
        {
            if (armorClassEvent == null) throw new ArgumentNullException(
                "armorClassEvent");
            if (Stage != BodyguardAttackStage.BodyguardResolved) return false;
            return _armorClassEvents.Add(armorClassEvent);
        }

        internal bool TryResolveAttack(bool hit)
        {
            if (Stage != BodyguardAttackStage.BodyguardResolved) return false;
            AttackHit = hit;
            Stage = BodyguardAttackStage.AttackResolved;
            return true;
        }

        internal bool TryIntercept(string interceptorId)
        {
            if (string.IsNullOrWhiteSpace(interceptorId))
                throw new ArgumentException("An interceptor identity is required.",
                    "interceptorId");
            if (Stage != BodyguardAttackStage.AttackResolved || AttackHit != true ||
                InterceptorId != null || !_attempts.Any(value => value.Success &&
                    string.Equals(value.ProtectorId, interceptorId,
                        StringComparison.Ordinal))) return false;
            InterceptorId = interceptorId;
            FinalTargetId = interceptorId;
            Stage = BodyguardAttackStage.Intercepted;
            return true;
        }

        internal void Complete()
        {
            if (Stage != BodyguardAttackStage.AttackResolved &&
                Stage != BodyguardAttackStage.Intercepted)
                throw new InvalidOperationException(
                    "Only a resolved attack can complete.");
            Stage = BodyguardAttackStage.Completed;
        }

        internal void Fault()
        {
            FinalTargetId = OriginalTargetId;
            InterceptorId = null;
            Stage = BodyguardAttackStage.Faulted;
        }

        private sealed class ReferenceEqualityComparer : IEqualityComparer<object>
        {
            internal static readonly ReferenceEqualityComparer Instance =
                new ReferenceEqualityComparer();
            public new bool Equals(object left, object right)
            { return ReferenceEquals(left, right); }
            public int GetHashCode(object value)
            { return System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(value); }
        }
    }
}
