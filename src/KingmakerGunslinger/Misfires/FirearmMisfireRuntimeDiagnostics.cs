using System;
using System.Globalization;
using System.Threading;
using KingmakerGunslinger.Firearms;

namespace KingmakerGunslinger.Misfires
{
    /// <summary>
    /// Process-local observations for Sprint 24 natural-roll handling and exact-item
    /// condition transitions. These counters are diagnostic only and never
    /// participate in attack resolution.
    /// </summary>
    internal static class FirearmMisfireRuntimeDiagnostics
    {
        private static readonly object Gate = new object();
        private static long _eligibleAttacks;
        private static long _naturalRolls;
        private static long _ordinaryRolls;
        private static long _misfires;
        private static long _normalToBroken;
        private static long _brokenToWrecked;
        private static long _forcedRollsApplied;
        private static long _duplicateRollAssignments;
        private static long _duplicateEvaluations;
        private static long _completedWithoutNaturalRoll;
        private static long _faults;
        private static string _last =
            "No eligible firearm natural roll has been evaluated in this process.";

        internal static long EligibleAttacks
        {
            get { return Interlocked.Read(ref _eligibleAttacks); }
        }

        internal static long NaturalRolls
        {
            get { return Interlocked.Read(ref _naturalRolls); }
        }

        internal static long OrdinaryRolls
        {
            get { return Interlocked.Read(ref _ordinaryRolls); }
        }

        internal static long Misfires
        {
            get { return Interlocked.Read(ref _misfires); }
        }

        internal static long NormalToBroken
        {
            get { return Interlocked.Read(ref _normalToBroken); }
        }

        internal static long BrokenToWrecked
        {
            get { return Interlocked.Read(ref _brokenToWrecked); }
        }

        internal static long ForcedRollsApplied
        {
            get { return Interlocked.Read(ref _forcedRollsApplied); }
        }

        internal static long DuplicateRollAssignments
        {
            get { return Interlocked.Read(ref _duplicateRollAssignments); }
        }

        internal static long DuplicateEvaluations
        {
            get { return Interlocked.Read(ref _duplicateEvaluations); }
        }

        internal static long CompletedWithoutNaturalRoll
        {
            get { return Interlocked.Read(ref _completedWithoutNaturalRoll); }
        }

        internal static long Faults
        {
            get { return Interlocked.Read(ref _faults); }
        }

        internal static void RecordEligible(
            string firearm,
            int misfireValue,
            FirearmCondition postDischargeCondition)
        {
            Interlocked.Increment(ref _eligibleAttacks);
            SetLast(string.Format(
                CultureInfo.InvariantCulture,
                "ELIGIBLE: firearm={0}; misfireRange=1-{1}; postDischargeCondition={2}; awaiting natural d20.",
                Normalize(firearm),
                misfireValue,
                postDischargeCondition));
        }

        internal static void RecordNaturalRoll(
            string firearm,
            int originalNaturalRoll,
            int finalNaturalRoll,
            bool forced)
        {
            Interlocked.Increment(ref _naturalRolls);
            if (forced)
            {
                Interlocked.Increment(ref _forcedRollsApplied);
            }

            SetLast(string.Format(
                CultureInfo.InvariantCulture,
                "ROLL: firearm={0}; originalNaturalD20={1}; finalNaturalD20={2}; forced={3}.",
                Normalize(firearm),
                originalNaturalRoll,
                finalNaturalRoll,
                forced));
        }

        internal static void RecordDecision(
            FirearmMisfireDecision decision,
            FirearmMisfireConditionDecision condition,
            string firearm,
            bool forced)
        {
            if (decision == null)
            {
                throw new ArgumentNullException("decision");
            }

            if (condition == null)
            {
                throw new ArgumentNullException("condition");
            }

            if (!ReferenceEquals(condition.Misfire, decision))
            {
                throw new ArgumentException(
                    "The condition decision does not belong to the supplied natural-roll decision.",
                    "condition");
            }

            if (decision.IsMisfire)
            {
                Interlocked.Increment(ref _misfires);
            }
            else
            {
                Interlocked.Increment(ref _ordinaryRolls);
            }

            switch (condition.Transition)
            {
                case FirearmMisfireConditionTransition.None:
                    break;
                case FirearmMisfireConditionTransition.NormalToBroken:
                    Interlocked.Increment(ref _normalToBroken);
                    break;
                case FirearmMisfireConditionTransition.BrokenToWrecked:
                    Interlocked.Increment(ref _brokenToWrecked);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(
                        "condition",
                        condition.Transition,
                        "Unsupported condition transition.");
            }

            SetLast(string.Format(
                CultureInfo.InvariantCulture,
                "{0}: {1}; forced={2}; firearm={3}; {4}.",
                decision.IsMisfire ? "MISFIRE" : "ORDINARY",
                decision,
                forced,
                Normalize(firearm),
                condition));
        }

        internal static void RecordDuplicateRollAssignment(string firearm)
        {
            Interlocked.Increment(ref _duplicateRollAssignments);
            SetLast(
                "DUPLICATE ROLL ASSIGNMENT: ignored for firearm=" +
                Normalize(firearm) + ".");
        }

        internal static void RecordDuplicateEvaluation(string firearm)
        {
            Interlocked.Increment(ref _duplicateEvaluations);
            SetLast(
                "DUPLICATE MISFIRE EVALUATION: the final miss decision was enforced without recounting or repeating condition damage for firearm=" +
                Normalize(firearm) + ".");
        }

        internal static void RecordQueueChange(string message)
        {
            SetLast("FORCED-ROLL DIAGNOSTIC: " + Normalize(message));
        }

        internal static void RecordCompletedWithoutNaturalRoll(string firearm)
        {
            Interlocked.Increment(ref _completedWithoutNaturalRoll);
            SetLast(
                "NO NATURAL ROLL: eligible firearm attack completed before the exact Roll setter; pending forced roll was preserved and no condition damage occurred; firearm=" +
                Normalize(firearm) + ".");
        }

        internal static void RecordFault(Exception exception, string phase)
        {
            if (exception == null)
            {
                throw new ArgumentNullException("exception");
            }

            Interlocked.Increment(ref _faults);
            SetLast(string.Format(
                CultureInfo.InvariantCulture,
                "FAULT phase={0}; {1}: {2}",
                Normalize(phase),
                exception.GetType().Name,
                exception.Message));
        }

        internal static string Describe(int? pendingForcedRoll)
        {
            string last;
            lock (Gate)
            {
                last = _last;
            }

            return string.Format(
                CultureInfo.InvariantCulture,
                "eligible={0}; naturalRolls={1}; ordinary={2}; misfires={3}; normalToBroken={4}; brokenToWrecked={5}; forcedApplied={6}; duplicateAssignments={7}; duplicateEvaluations={8}; noNaturalRoll={9}; faults={10}; pendingForcedRoll={11}; last={12}",
                EligibleAttacks,
                NaturalRolls,
                OrdinaryRolls,
                Misfires,
                NormalToBroken,
                BrokenToWrecked,
                ForcedRollsApplied,
                DuplicateRollAssignments,
                DuplicateEvaluations,
                CompletedWithoutNaturalRoll,
                Faults,
                pendingForcedRoll.HasValue
                    ? pendingForcedRoll.Value.ToString(CultureInfo.InvariantCulture)
                    : "<none>",
                last);
        }

        private static void SetLast(string value)
        {
            lock (Gate)
            {
                _last = value;
            }
        }

        private static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? "<unavailable>" : value.Trim();
        }
    }
}
