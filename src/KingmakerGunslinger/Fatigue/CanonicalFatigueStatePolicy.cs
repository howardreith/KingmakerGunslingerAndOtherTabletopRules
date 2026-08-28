using System;

namespace KingmakerGunslinger.Fatigue
{
    internal enum CanonicalFatigueState
    {
        Neither = 0,
        Fatigued = 1,
        Exhausted = 2
    }

    internal enum CanonicalConditionKind
    {
        Fatigued = 1,
        Exhausted = 2
    }

    internal enum CanonicalFatigueApplicationIntent
    {
        NativePassthrough = 0,
        EscalateIfAlreadyFatigued = 1
    }

    internal sealed class CanonicalFatigueStateDecision
    {
        internal CanonicalFatigueStateDecision(CanonicalFatigueState before,
            CanonicalConditionKind incoming, bool applicationSucceeded,
            CanonicalFatigueApplicationIntent intent,
            CanonicalConditionKind effectiveIncoming,
            CanonicalFatigueState after, bool escalated)
        {
            Before = before;
            Incoming = incoming;
            ApplicationSucceeded = applicationSucceeded;
            Intent = intent;
            EffectiveIncoming = effectiveIncoming;
            After = after;
            Escalated = escalated;
        }

        internal CanonicalFatigueState Before { get; private set; }
        internal CanonicalConditionKind Incoming { get; private set; }
        internal bool ApplicationSucceeded { get; private set; }
        internal CanonicalFatigueApplicationIntent Intent { get; private set; }
        internal CanonicalConditionKind EffectiveIncoming { get; private set; }
        internal CanonicalFatigueState After { get; private set; }
        internal bool Escalated { get; private set; }
    }

    internal static class CanonicalFatigueStatePolicy
    {
        internal static CanonicalFatigueStateDecision Decide(
            CanonicalFatigueState before, CanonicalConditionKind incoming,
            bool applicationSucceeded,
            CanonicalFatigueApplicationIntent intent)
        {
            if (!Enum.IsDefined(typeof(CanonicalFatigueState), before))
                throw new ArgumentOutOfRangeException("before");
            if (!Enum.IsDefined(typeof(CanonicalConditionKind), incoming))
                throw new ArgumentOutOfRangeException("incoming");
            if (!Enum.IsDefined(typeof(CanonicalFatigueApplicationIntent),
                    intent))
                throw new ArgumentOutOfRangeException("intent");
            if (!applicationSucceeded)
            {
                return new CanonicalFatigueStateDecision(before, incoming,
                    false, intent, incoming, before, false);
            }

            if (incoming == CanonicalConditionKind.Exhausted)
            {
                return new CanonicalFatigueStateDecision(before, incoming,
                    true, intent, CanonicalConditionKind.Exhausted,
                    CanonicalFatigueState.Exhausted, false);
            }

            if (before == CanonicalFatigueState.Neither)
            {
                return new CanonicalFatigueStateDecision(before, incoming,
                    true, intent, CanonicalConditionKind.Fatigued,
                    CanonicalFatigueState.Fatigued, false);
            }

            if (intent == CanonicalFatigueApplicationIntent.NativePassthrough)
            {
                return new CanonicalFatigueStateDecision(before, incoming,
                    true, intent, CanonicalConditionKind.Fatigued,
                    before == CanonicalFatigueState.Exhausted
                        ? CanonicalFatigueState.Exhausted
                        : CanonicalFatigueState.Fatigued,
                    false);
            }

            return new CanonicalFatigueStateDecision(before, incoming, true,
                intent, CanonicalConditionKind.Exhausted,
                CanonicalFatigueState.Exhausted,
                before == CanonicalFatigueState.Fatigued);
        }
    }

    internal struct CanonicalConditionDuration :
        IEquatable<CanonicalConditionDuration>
    {
        private CanonicalConditionDuration(bool permanent, long endTicks)
        {
            Permanent = permanent;
            EndTicks = endTicks;
        }

        internal bool Permanent { get; private set; }
        internal long EndTicks { get; private set; }

        internal static CanonicalConditionDuration PermanentDuration()
        {
            return new CanonicalConditionDuration(true, long.MaxValue);
        }

        internal static CanonicalConditionDuration Temporary(long endTicks)
        {
            if (endTicks < 0L) throw new ArgumentOutOfRangeException("endTicks");
            return new CanonicalConditionDuration(false, endTicks);
        }

        internal static CanonicalConditionDuration PreserveLongest(
            CanonicalConditionDuration first,
            CanonicalConditionDuration second)
        {
            if (first.Permanent || second.Permanent)
                return PermanentDuration();
            return Temporary(Math.Max(first.EndTicks, second.EndTicks));
        }

        public bool Equals(CanonicalConditionDuration other)
        {
            return Permanent == other.Permanent && EndTicks == other.EndTicks;
        }

        public override bool Equals(object obj)
        {
            return obj is CanonicalConditionDuration &&
                Equals((CanonicalConditionDuration)obj);
        }

        public override int GetHashCode()
        {
            unchecked { return (Permanent.GetHashCode() * 397) ^
                EndTicks.GetHashCode(); }
        }
    }
}
