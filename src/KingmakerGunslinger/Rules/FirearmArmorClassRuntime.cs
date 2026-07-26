using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Threading;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Diagnostics;

namespace KingmakerGunslinger.Rules
{
    /// <summary>
    /// Exception-contained Kingmaker adapter for the Sprint 9 touch-AC rule.
    /// It keeps only immutable firearm metadata on a thread-local attack stack and
    /// uses a weak event stamp to prevent applying the delta twice to one AC event.
    /// </summary>
    internal static class FirearmArmorClassRuntime
    {
        [ThreadStatic]
        private static Stack<AttackFrame> _attackFrames;

        private static readonly object StampGate = new object();
        private static readonly ConditionalWeakTable<object, MutationStamp> AppliedEvents =
            new ConditionalWeakTable<object, MutationStamp>();
        private static readonly object WarningGate = new object();
        private static readonly HashSet<string> LoggedWarnings =
            new HashSet<string>(StringComparer.Ordinal);

        private static long _appliedCount;
        private static long _ordinaryCount;
        private static long _duplicateCount;
        private static long _faultCount;

        internal static long AppliedCount
        {
            get { return Interlocked.Read(ref _appliedCount); }
        }

        internal static long OrdinaryCount
        {
            get { return Interlocked.Read(ref _ordinaryCount); }
        }

        internal static long DuplicateCount
        {
            get { return Interlocked.Read(ref _duplicateCount); }
        }

        internal static long FaultCount
        {
            get { return Interlocked.Read(ref _faultCount); }
        }

        internal static int ActiveAttackDepth
        {
            get { return _attackFrames == null ? 0 : _attackFrames.Count; }
        }

        internal static void BeforeAttackRoll(object ruleAttackRoll)
        {
            if (ruleAttackRoll == null)
            {
                return;
            }

            try
            {
                Stack<AttackFrame> frames = GetFrames();
                FirearmMarkerSnapshot marker = FirearmMarkerLookup.ReadFromRuleEvent(ruleAttackRoll);
                frames.Push(new AttackFrame(
                    RuntimeHelpers.GetHashCode(ruleAttackRoll),
                    marker));
            }
            catch (Exception exception)
            {
                HandleFault(
                    "attack-context.begin.failed",
                    "Failed to inspect the attack-roll firearm context; this attack will retain ordinary AC.",
                    exception,
                    true);
            }
        }

        internal static void AfterAttackRoll(object ruleAttackRoll)
        {
            if (ruleAttackRoll == null || _attackFrames == null)
            {
                return;
            }

            try
            {
                int identity = RuntimeHelpers.GetHashCode(ruleAttackRoll);
                if (_attackFrames.Count == 0 ||
                    _attackFrames.Peek().EventIdentity != identity)
                {
                    int cleared = _attackFrames.Count;
                    _attackFrames.Clear();
                    LogWarningOnce(
                        "attack-context.stack-mismatch",
                        string.Format(
                            CultureInfo.InvariantCulture,
                            "Firearm AC attack-context stack mismatch; cleared {0} frame(s) and retained ordinary AC.",
                            cleared));
                    return;
                }

                _attackFrames.Pop();
                if (_attackFrames.Count == 0)
                {
                    _attackFrames = null;
                }
            }
            catch (Exception exception)
            {
                HandleFault(
                    "attack-context.end.failed",
                    "Failed to close the attack-roll firearm context; later AC events will fail closed.",
                    exception,
                    true);
            }
        }

        internal static void AfterCalculateArmorClass(object ruleCalculateArmorClass)
        {
            if (ruleCalculateArmorClass == null)
            {
                return;
            }

            try
            {
                FirearmMarkerSnapshot marker = ResolveMarker(ruleCalculateArmorClass);
                if (!marker.IsExactFirearm)
                {
                    return;
                }

                if (IsStamped(ruleCalculateArmorClass))
                {
                    Interlocked.Increment(ref _duplicateCount);
                    LogDuplicate(marker);
                    return;
                }

                object initiator;
                object target;
                double distanceMeters;
                int ordinaryArmorClass;
                int touchArmorClass;
                int currentTargetArmorClass;
                string targetAcMember;

                if (!KingmakerArmorClassAccess.TryReadParticipants(
                        ruleCalculateArmorClass,
                        out initiator,
                        out target) ||
                    !KingmakerArmorClassAccess.TryReadDistanceMeters(
                        initiator,
                        target,
                        out distanceMeters) ||
                    !KingmakerArmorClassAccess.TryReadTargetArmorClasses(
                        target,
                        out ordinaryArmorClass,
                        out touchArmorClass) ||
                    !KingmakerArmorClassAccess.TryReadTargetArmorClass(
                        ruleCalculateArmorClass,
                        out currentTargetArmorClass,
                        out targetAcMember))
                {
                    Interlocked.Increment(ref _ordinaryCount);
                    LogWarningOnce(
                        "ac.contract-unavailable",
                        "The installed RuleCalculateAC/UnitEntityData contract did not expose the required participants, DistanceTo, ordinary/touch AC, and one writable Int32 TargetAC member. Firearm attacks retain ordinary AC.");
                    return;
                }

                FirearmArmorClassDecision decision = FirearmArmorClassService.Select(
                    new FirearmArmorClassRequest(
                        true,
                        marker.MarkerCount,
                        marker.Definition,
                        distanceMeters,
                        ordinaryArmorClass,
                        touchArmorClass,
                        currentTargetArmorClass,
                        false));

                if (!decision.UsesTouchArmorClass)
                {
                    Interlocked.Increment(ref _ordinaryCount);
                    LogDecision(marker, distanceMeters, currentTargetArmorClass, decision, targetAcMember);
                    return;
                }

                if (decision.ShouldWriteTargetArmorClass)
                {
                    string writtenMember;
                    if (!KingmakerArmorClassAccess.TryWriteTargetArmorClass(
                            ruleCalculateArmorClass,
                            decision.SelectedTargetArmorClass,
                            out writtenMember))
                    {
                        Interlocked.Increment(ref _ordinaryCount);
                        LogWarningOnce(
                            "ac.write-unavailable",
                            "The installed RuleCalculateAC TargetAC member could be read but not written. Firearm attacks retain ordinary AC.");
                        return;
                    }

                    targetAcMember = writtenMember;
                }

                Stamp(ruleCalculateArmorClass);
                Interlocked.Increment(ref _appliedCount);
                LogDecision(marker, distanceMeters, currentTargetArmorClass, decision, targetAcMember);
            }
            catch (Exception exception)
            {
                HandleFault(
                    "ac.apply.failed",
                    "The firearm touch-AC adapter failed; the current event was left on the ordinary Kingmaker path wherever possible.",
                    exception,
                    false);
            }
        }

        internal static int ResetCurrentThread()
        {
            if (_attackFrames == null)
            {
                return 0;
            }

            int count = _attackFrames.Count;
            _attackFrames.Clear();
            _attackFrames = null;
            return count;
        }

        private static Stack<AttackFrame> GetFrames()
        {
            if (_attackFrames == null)
            {
                _attackFrames = new Stack<AttackFrame>();
            }

            return _attackFrames;
        }

        private static FirearmMarkerSnapshot ResolveMarker(object ruleCalculateArmorClass)
        {
            FirearmMarkerSnapshot direct =
                FirearmMarkerLookup.ReadFromRuleEvent(ruleCalculateArmorClass);
            if (direct.HasWeapon)
            {
                return direct;
            }

            if (_attackFrames == null || _attackFrames.Count == 0)
            {
                return FirearmMarkerSnapshot.NoWeapon();
            }

            return _attackFrames.Peek().Marker;
        }

        private static bool IsStamped(object ruleCalculateArmorClass)
        {
            lock (StampGate)
            {
                MutationStamp ignored;
                return AppliedEvents.TryGetValue(ruleCalculateArmorClass, out ignored);
            }
        }

        private static void Stamp(object ruleCalculateArmorClass)
        {
            lock (StampGate)
            {
                MutationStamp ignored;
                if (!AppliedEvents.TryGetValue(ruleCalculateArmorClass, out ignored))
                {
                    AppliedEvents.Add(ruleCalculateArmorClass, MutationStamp.Instance);
                }
            }
        }


        private static void LogDuplicate(FirearmMarkerSnapshot marker)
        {
            if (!CombatTraceSettings.Enabled)
            {
                return;
            }

            ModContext context;
            if (ModContext.TryGet(out context))
            {
                context.Logger.Info(
                    "firearms",
                    "ac.duplicate-skipped",
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "weaponType={0}; reason=already-applied",
                        marker.WeaponType));
            }
        }

        private static void LogDecision(
            FirearmMarkerSnapshot marker,
            double distanceMeters,
            int previousTargetArmorClass,
            FirearmArmorClassDecision decision,
            string targetAcMember)
        {
            if (!CombatTraceSettings.Enabled)
            {
                return;
            }

            ModContext context;
            if (!ModContext.TryGet(out context))
            {
                return;
            }

            context.Logger.Info(
                "firearms",
                decision.UsesTouchArmorClass ? "ac.touch-selected" : "ac.ordinary-selected",
                string.Format(
                    CultureInfo.InvariantCulture,
                    "weaponType={0}; distanceMeters={1:0.###}; rangeIncrement={2}; previousTargetAC={3}; selectedTargetAC={4}; adjustment={5}; targetMember={6}; reason={7}",
                    marker.WeaponType,
                    distanceMeters,
                    decision.RangeIncrement,
                    previousTargetArmorClass,
                    decision.SelectedTargetArmorClass,
                    decision.Adjustment,
                    string.IsNullOrWhiteSpace(targetAcMember) ? "<unavailable>" : targetAcMember,
                    decision.Reason));
        }

        private static void LogWarningOnce(string eventName, string message)
        {
            string key = eventName + "\n" + message;
            lock (WarningGate)
            {
                if (!LoggedWarnings.Add(key))
                {
                    return;
                }
            }

            ModContext context;
            if (ModContext.TryGet(out context))
            {
                context.Logger.Warning("firearms", eventName, message);
            }
        }

        private static void HandleFault(
            string eventName,
            string message,
            Exception exception,
            bool clearFrames)
        {
            Interlocked.Increment(ref _faultCount);
            if (clearFrames && _attackFrames != null)
            {
                _attackFrames.Clear();
                _attackFrames = null;
            }

            ModContext context;
            if (ModContext.TryGet(out context))
            {
                context.Logger.Failure("firearms", eventName, message, exception);
            }
        }

        private sealed class AttackFrame
        {
            internal AttackFrame(int eventIdentity, FirearmMarkerSnapshot marker)
            {
                EventIdentity = eventIdentity;
                Marker = marker ?? FirearmMarkerSnapshot.NoWeapon();
            }

            internal int EventIdentity { get; private set; }

            internal FirearmMarkerSnapshot Marker { get; private set; }
        }

        private sealed class MutationStamp
        {
            internal static readonly MutationStamp Instance = new MutationStamp();

            private MutationStamp()
            {
            }
        }
    }
}
