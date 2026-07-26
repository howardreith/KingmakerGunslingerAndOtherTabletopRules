using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Threading;
using KingmakerGunslinger.Bootstrap;

namespace KingmakerGunslinger.Diagnostics
{
    /// <summary>
    /// Exception-contained runtime adapter between Harmony callbacks and the pure correlator.
    /// It never alters a rule event, returns a patch-control value, or retains game objects.
    /// </summary>
    internal static class CombatTraceRuntime
    {
        [ThreadStatic]
        private static RuntimeState _threadState;

        private static long _nextTraceId;
        private static long _completedTraceCount;
        private static long _faultCount;

        internal static long CompletedTraceCount
        {
            get { return Interlocked.Read(ref _completedTraceCount); }
        }

        internal static long FaultCount
        {
            get { return Interlocked.Read(ref _faultCount); }
        }

        internal static int ActiveTraceCount
        {
            get { return _threadState == null ? 0 : _threadState.Correlator.ActiveTraceCount; }
        }

        internal static void Before(CombatTraceStage stage, object ruleEvent)
        {
            if (!CombatTraceSettings.Enabled || ruleEvent == null)
            {
                return;
            }

            try
            {
                RuntimeState state = GetState();
                int eventIdentity = GetEventIdentity(ruleEvent);
                EventFrame parent = state.Frames.Count == 0
                    ? null
                    : state.Frames.Peek();
                FirearmMarkerSnapshot localMarker =
                    FirearmMarkerLookup.ReadFromRuleEvent(ruleEvent);
                bool localConflict = localMarker.HasWeapon && !localMarker.IsExactFirearm;
                FirearmMarkerSnapshot effectiveMarker = localMarker.HasWeapon
                    ? localMarker
                    : parent == null
                        ? localMarker
                        : parent.Marker;
                int? parentIdentity = localConflict || parent == null
                    ? (int?)null
                    : parent.EventIdentity;
                string markerSource = localMarker.HasWeapon
                    ? "event"
                    : parent == null
                        ? "none"
                        : "parent";

                CombatTraceObservation observation = RuleEventSnapshotReader.Read(
                    stage,
                    CombatTracePhase.Before,
                    ruleEvent,
                    eventIdentity,
                    parentIdentity,
                    effectiveMarker,
                    markerSource);
                CombatTraceDecision decision = state.Correlator.Observe(observation, false);
                state.Frames.Push(new EventFrame(
                    eventIdentity,
                    parentIdentity,
                    stage,
                    effectiveMarker));
                EmitDecision(decision);
            }
            catch (Exception exception)
            {
                HandleFault("before", stage, exception);
            }
        }

        internal static void After(CombatTraceStage stage, object ruleEvent)
        {
            if (!CombatTraceSettings.Enabled || ruleEvent == null)
            {
                return;
            }

            try
            {
                RuntimeState state = GetState();
                int eventIdentity = GetEventIdentity(ruleEvent);
                EventFrame frame = PopMatchingFrame(state, stage, eventIdentity);
                if (frame == null)
                {
                    LogWarning(
                        "trace.stack-mismatch",
                        string.Format(
                            CultureInfo.InvariantCulture,
                            "No matching prefix frame was found for stage={0}; event={1}. The current thread trace state was reset.",
                            stage,
                            eventIdentity));
                    state.Reset();
                    return;
                }

                CombatTraceObservation observation = RuleEventSnapshotReader.Read(
                    stage,
                    CombatTracePhase.After,
                    ruleEvent,
                    eventIdentity,
                    frame.ParentEventIdentity,
                    frame.Marker,
                    "prefix");
                bool mayCloseRoot = stage == CombatTraceStage.WeaponAttack ||
                    stage == CombatTraceStage.AttackRoll;
                CombatTraceDecision decision = state.Correlator.Observe(
                    observation,
                    mayCloseRoot);
                EmitDecision(decision);
            }
            catch (Exception exception)
            {
                HandleFault("after", stage, exception);
            }
        }

        internal static int ResetCurrentThread()
        {
            RuntimeState state = _threadState;
            if (state == null)
            {
                return 0;
            }

            int active = state.Correlator.Reset();
            state.Frames.Clear();
            _threadState = null;
            return active;
        }

        private static RuntimeState GetState()
        {
            RuntimeState state = _threadState;
            if (state == null)
            {
                state = new RuntimeState();
                _threadState = state;
            }

            return state;
        }

        private static int GetEventIdentity(object ruleEvent)
        {
            int identity = RuntimeHelpers.GetHashCode(ruleEvent);
            return identity == 0 ? int.MinValue : identity;
        }

        private static EventFrame PopMatchingFrame(
            RuntimeState state,
            CombatTraceStage stage,
            int eventIdentity)
        {
            if (state.Frames.Count == 0)
            {
                return null;
            }

            EventFrame frame = state.Frames.Pop();
            return frame.EventIdentity == eventIdentity && frame.Stage == stage
                ? frame
                : null;
        }

        private static void EmitDecision(CombatTraceDecision decision)
        {
            if (decision == null || !decision.Accepted)
            {
                return;
            }

            ModContext context;
            if (!ModContext.TryGet(out context))
            {
                return;
            }

            if (decision.Created)
            {
                context.Logger.Info(
                    "combat",
                    "trace.begin",
                    CombatTraceFormatter.FormatBegin(
                        decision.TraceId,
                        decision.Record.Observation));
            }

            context.Logger.Info(
                "combat",
                "trace.event",
                CombatTraceFormatter.FormatRecord(decision.TraceId, decision.Record));

            if (decision.Completed)
            {
                Interlocked.Increment(ref _completedTraceCount);
                context.Logger.Info(
                    "combat",
                    "trace.complete",
                    CombatTraceFormatter.FormatComplete(decision.CompletedTrace));
            }
        }

        private static void HandleFault(
            string phase,
            CombatTraceStage stage,
            Exception exception)
        {
            Interlocked.Increment(ref _faultCount);
            RuntimeState state = _threadState;
            if (state != null)
            {
                state.Reset();
            }

            ModContext context;
            if (ModContext.TryGet(out context))
            {
                context.Logger.Failure(
                    "combat",
                    "trace." + phase + ".failed",
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "Read-only combat tracing failed at stage={0}; the event was not modified and this thread's active diagnostic state was cleared.",
                        stage),
                    exception);
            }
        }

        private static void LogWarning(string eventName, string message)
        {
            ModContext context;
            if (ModContext.TryGet(out context))
            {
                context.Logger.Warning("combat", eventName, message);
            }
        }

        private static long NextTraceId()
        {
            return Interlocked.Increment(ref _nextTraceId);
        }

        private sealed class RuntimeState
        {
            internal RuntimeState()
            {
                Correlator = new CombatTraceCorrelator(NextTraceId);
                Frames = new Stack<EventFrame>();
            }

            internal CombatTraceCorrelator Correlator { get; private set; }

            internal Stack<EventFrame> Frames { get; private set; }

            internal void Reset()
            {
                Correlator.Reset();
                Frames.Clear();
            }
        }

        private sealed class EventFrame
        {
            internal EventFrame(
                int eventIdentity,
                int? parentEventIdentity,
                CombatTraceStage stage,
                FirearmMarkerSnapshot marker)
            {
                EventIdentity = eventIdentity;
                ParentEventIdentity = parentEventIdentity;
                Stage = stage;
                Marker = marker ?? FirearmMarkerSnapshot.NoWeapon();
            }

            internal int EventIdentity { get; private set; }

            internal int? ParentEventIdentity { get; private set; }

            internal CombatTraceStage Stage { get; private set; }

            internal FirearmMarkerSnapshot Marker { get; private set; }
        }
    }
}
