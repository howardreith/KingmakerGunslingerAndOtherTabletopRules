using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;

namespace KingmakerGunslinger.Explosions
{
    /// <summary>
    /// Process-lifetime counters for the second-misfire burst. Burst-level counters
    /// remain separate from spatial-query and per-target counters so one duplicate,
    /// rejection, or native target fault is visible without obscuring the attack-level
    /// at-most-once gate.
    /// </summary>
    internal static class FirearmExplosionRuntimeDiagnostics
    {
        private static readonly object Gate = new object();
        private static long _scheduled;
        private static long _attempts;
        private static long _applied;
        private static long _notRequired;
        private static long _rejected;
        private static long _duplicates;
        private static long _faults;
        private static long _queries;
        private static long _queryCandidates;
        private static long _plannedTargets;
        private static long _targetAttempts;
        private static long _targetApplied;
        private static long _targetRejected;
        private static long _targetDuplicates;
        private static long _targetFaults;
        private static string _last =
            "No detected firearm misfire has entered the Sprint 26 explosion-burst consequence in this process.";

        internal static long Scheduled { get { return Interlocked.Read(ref _scheduled); } }
        internal static long Attempts { get { return Interlocked.Read(ref _attempts); } }
        internal static long Applied { get { return Interlocked.Read(ref _applied); } }
        internal static long NotRequired { get { return Interlocked.Read(ref _notRequired); } }
        internal static long Rejected { get { return Interlocked.Read(ref _rejected); } }
        internal static long Duplicates { get { return Interlocked.Read(ref _duplicates); } }
        internal static long Faults { get { return Interlocked.Read(ref _faults); } }
        internal static long Queries { get { return Interlocked.Read(ref _queries); } }
        internal static long QueryCandidates { get { return Interlocked.Read(ref _queryCandidates); } }
        internal static long PlannedTargets { get { return Interlocked.Read(ref _plannedTargets); } }
        internal static long TargetAttempts { get { return Interlocked.Read(ref _targetAttempts); } }
        internal static long TargetApplied { get { return Interlocked.Read(ref _targetApplied); } }
        internal static long TargetRejected { get { return Interlocked.Read(ref _targetRejected); } }
        internal static long TargetDuplicates { get { return Interlocked.Read(ref _targetDuplicates); } }
        internal static long TargetFaults { get { return Interlocked.Read(ref _targetFaults); } }

        internal static void RecordDecision(
            FirearmExplosionDecision decision,
            string firearm,
            int burstRadiusFeet)
        {
            if (decision == null)
            {
                throw new ArgumentNullException("decision");
            }

            if (!decision.Condition.Misfire.IsMisfire)
            {
                throw new ArgumentException(
                    "Only a detected misfire may enter explosion consequence diagnostics.",
                    "decision");
            }

            if (burstRadiusFeet < 1)
            {
                throw new ArgumentOutOfRangeException("burstRadiusFeet");
            }

            if (decision.RequiresBurstDamage)
            {
                Interlocked.Increment(ref _scheduled);
                SetLast(string.Format(
                    CultureInfo.InvariantCulture,
                    "SCHEDULED: conditionTransition={0}; burstRadiusFeet={1}; origin=exact-wielder; reflexDC={2}; firearm={3}.",
                    decision.Condition.Transition,
                    burstRadiusFeet,
                    FirearmExplosionService.ReflexSaveDifficultyClass,
                    Normalize(firearm)));
                return;
            }

            Interlocked.Increment(ref _notRequired);
            SetLast(string.Format(
                CultureInfo.InvariantCulture,
                "NOT REQUIRED: conditionTransition={0}; first misfire changes only the exact firearm to Broken; firearm={1}.",
                decision.Condition.Transition,
                Normalize(firearm)));
        }

        internal static void RecordAttempt(
            string firearm,
            string exactWielder,
            string attackRollIdentity,
            string repositoryIdentity,
            string damageFormula,
            int burstRadiusFeet)
        {
            Interlocked.Increment(ref _attempts);
            SetLast(string.Format(
                CultureInfo.InvariantCulture,
                "ATTEMPT: firearm={0}; exactWielder={1}; attackRoll={2}; repositoryIdentity={3}; weaponDamage={4}; burstRadiusFeet={5}; reflexDC={6}.",
                Normalize(firearm),
                Normalize(exactWielder),
                Normalize(attackRollIdentity),
                Normalize(repositoryIdentity),
                Normalize(damageFormula),
                burstRadiusFeet,
                FirearmExplosionService.ReflexSaveDifficultyClass));
        }

        internal static void RecordQuery(
            string origin,
            int burstRadiusFeet,
            FirearmExplosionTargetPlan plan)
        {
            if (plan == null)
            {
                throw new ArgumentNullException("plan");
            }

            Interlocked.Increment(ref _queries);
            Interlocked.Add(ref _queryCandidates, plan.ObservedCandidates);
            Interlocked.Add(ref _plannedTargets, plan.TargetCount);
            Interlocked.Add(ref _targetDuplicates, plan.DuplicateCandidates);
            SetLast(string.Format(
                CultureInfo.InvariantCulture,
                "QUERY: origin={0}; burstRadiusFeet={1}; observedCandidates={2}; duplicateCandidates={3}; plannedTargets={4}; exactWielderLast=True.",
                Normalize(origin),
                burstRadiusFeet,
                plan.ObservedCandidates,
                plan.DuplicateCandidates,
                plan.TargetCount));
        }

        internal static void RecordTargetAttempt(
            FirearmExplosionTargetCandidate target)
        {
            if (target == null)
            {
                throw new ArgumentNullException("target");
            }

            Interlocked.Increment(ref _targetAttempts);
            SetLast(string.Format(
                CultureInfo.InvariantCulture,
                "TARGET ATTEMPT: target={0}; unitId={1}; distanceMeters={2:0.###}; exactWielder={3}.",
                target.DisplayName,
                target.StableIdentity,
                target.DistanceMeters,
                target.IsExactWielder));
        }

        internal static void RecordTargetApplied(
            FirearmExplosionTargetResult result)
        {
            if (result == null)
            {
                throw new ArgumentNullException("result");
            }

            Interlocked.Increment(ref _targetApplied);
            SetLast("TARGET APPLIED: " + result + ".");
        }

        internal static void RecordTargetDuplicate(
            FirearmExplosionTargetCandidate target)
        {
            if (target == null)
            {
                throw new ArgumentNullException("target");
            }

            Interlocked.Increment(ref _targetDuplicates);
            SetLast(string.Format(
                CultureInfo.InvariantCulture,
                "TARGET DUPLICATE: a second application to the same unit reference was blocked; target={0}; unitId={1}; exactWielder={2}.",
                target.DisplayName,
                target.StableIdentity,
                target.IsExactWielder));
        }

        internal static void RecordTargetRejected(
            string target,
            string stableIdentity,
            string reason)
        {
            Interlocked.Increment(ref _targetRejected);
            SetLast(
                "TARGET REJECTED: " + Normalize(reason) +
                "; target=" + Normalize(target) +
                "; unitId=" + Normalize(stableIdentity) + ".");
        }

        internal static void RecordTargetFault(
            Exception exception,
            string target,
            string stableIdentity)
        {
            if (exception == null)
            {
                throw new ArgumentNullException("exception");
            }

            Interlocked.Increment(ref _targetFaults);
            SetLast(string.Format(
                CultureInfo.InvariantCulture,
                "TARGET FAULT: target={0}; unitId={1}; {2}: {3}",
                Normalize(target),
                Normalize(stableIdentity),
                exception.GetType().Name,
                exception.Message));
        }

        internal static void RecordApplied(
            string firearm,
            string exactWielder,
            string attackRollIdentity,
            string repositoryIdentity,
            string damageFormula,
            int burstRadiusFeet,
            FirearmExplosionTargetPlan plan,
            IReadOnlyList<FirearmExplosionTargetResult> results)
        {
            if (plan == null)
            {
                throw new ArgumentNullException("plan");
            }

            if (results == null)
            {
                throw new ArgumentNullException("results");
            }

            Interlocked.Increment(ref _applied);
            string targetEvidence = string.Join(
                " | ",
                results.Select(result => "[" + result + "]").ToArray());
            SetLast(string.Format(
                CultureInfo.InvariantCulture,
                "APPLIED: firearm={0}; exactWielder={1}; attackRoll={2}; repositoryIdentity={3}; weaponDamage={4}; burstRadiusFeet={5}; reflexDC={6}; observedCandidates={7}; duplicateCandidates={8}; plannedTargets={9}; appliedTargets={10}; targets={11}; finalState=empty/Wrecked.",
                Normalize(firearm),
                Normalize(exactWielder),
                Normalize(attackRollIdentity),
                Normalize(repositoryIdentity),
                Normalize(damageFormula),
                burstRadiusFeet,
                FirearmExplosionService.ReflexSaveDifficultyClass,
                plan.ObservedCandidates,
                plan.DuplicateCandidates,
                plan.TargetCount,
                results.Count,
                string.IsNullOrWhiteSpace(targetEvidence) ? "<none>" : targetEvidence));
        }

        internal static void RecordRejected(string firearm, string reason)
        {
            Interlocked.Increment(ref _rejected);
            SetLast(
                "REJECTED: " + Normalize(reason) +
                "; firearm=" + Normalize(firearm) + ".");
        }

        internal static void RecordDuplicate(string firearm)
        {
            Interlocked.Increment(ref _duplicates);
            SetLast(
                "DUPLICATE: a second burst application for the same attack-roll object was blocked; firearm=" +
                Normalize(firearm) + ".");
        }

        internal static void RecordFault(
            Exception exception,
            string phase,
            string firearm)
        {
            if (exception == null)
            {
                throw new ArgumentNullException("exception");
            }

            Interlocked.Increment(ref _faults);
            SetLast(string.Format(
                CultureInfo.InvariantCulture,
                "FAULT phase={0}; firearm={1}; {2}: {3}",
                Normalize(phase),
                Normalize(firearm),
                exception.GetType().Name,
                exception.Message));
        }

        internal static string Describe()
        {
            string last;
            lock (Gate)
            {
                last = _last;
            }

            return string.Format(
                CultureInfo.InvariantCulture,
                "scheduled={0}; attempts={1}; applied={2}; notRequired={3}; rejected={4}; duplicates={5}; faults={6}; queries={7}; queryCandidates={8}; plannedTargets={9}; targetAttempts={10}; targetApplied={11}; targetRejected={12}; targetDuplicates={13}; targetFaults={14}; last={15}",
                Scheduled,
                Attempts,
                Applied,
                NotRequired,
                Rejected,
                Duplicates,
                Faults,
                Queries,
                QueryCandidates,
                PlannedTargets,
                TargetAttempts,
                TargetApplied,
                TargetRejected,
                TargetDuplicates,
                TargetFaults,
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
            return string.IsNullOrWhiteSpace(value)
                ? "<unavailable>"
                : value.Trim();
        }
    }
}
