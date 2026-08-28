using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using Harmony12;
using Kingmaker;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Cord;

namespace KingmakerGunslinger.Fatigue
{
    internal sealed class CanonicalFatigueApplicationResult
    {
        internal CanonicalFatigueApplicationResult(bool applicationSucceeded,
            bool cordSubstituted, CanonicalFatigueState state, Buff condition,
            string status)
        {
            ApplicationSucceeded = applicationSucceeded;
            CordSubstituted = cordSubstituted;
            State = state;
            Condition = condition;
            Status = status ?? throw new ArgumentNullException("status");
        }

        internal bool ApplicationSucceeded { get; private set; }
        internal bool CordSubstituted { get; private set; }
        internal CanonicalFatigueState State { get; private set; }
        internal Buff Condition { get; private set; }
        internal string Status { get; private set; }
    }

    /// <summary>
    /// Coordinates exact canonical Fatigued and Exhausted applications at the
    /// installed engine's RuleApplyBuff boundary. Native requests retain native
    /// condition semantics. Only a one-shot exact Acadamae request may escalate
    /// Fatigued after the native rule accepts it; Cord remains post-success.
    /// </summary>
    internal static class CanonicalFatigueApplicationRuntime
    {
        internal const string FatiguedGuid =
            "e6f2fc5d73d88064583cb828801212f4";
        internal const string ExhaustedGuid =
            "46d1b9cc3d0fd36469a471b047d773a2";

        private static readonly object ConfigurationGate = new object();
        private static BlueprintBuff _fatigued;
        private static BlueprintBuff _exhausted;
        [ThreadStatic] private static ApplicationScope _activeScope;
        [ThreadStatic] private static int _replacementDepth;
        [ThreadStatic] private static Resolution _lastThreadResolution;
        private static long _sequence;
        private static long _attempts;
        private static long _successful;
        private static long _blocked;
        private static long _escalated;
        private static long _cordSubstitutions;
        private static long _faults;
        private static string _lastResult =
            "No canonical fatigue application has resolved.";

        internal static long Attempts { get { return Interlocked.Read(ref _attempts); } }
        internal static long Successful { get { return Interlocked.Read(ref _successful); } }
        internal static long Blocked { get { return Interlocked.Read(ref _blocked); } }
        internal static long Escalated { get { return Interlocked.Read(ref _escalated); } }
        internal static long CordSubstitutions
        { get { return Interlocked.Read(ref _cordSubstitutions); } }
        internal static long Faults { get { return Interlocked.Read(ref _faults); } }
        internal static string LastResult
        { get { lock (ConfigurationGate) return _lastResult; } }

        internal static void ResetDiagnostics()
        {
            Interlocked.Exchange(ref _attempts, 0L);
            Interlocked.Exchange(ref _successful, 0L);
            Interlocked.Exchange(ref _blocked, 0L);
            Interlocked.Exchange(ref _escalated, 0L);
            Interlocked.Exchange(ref _cordSubstitutions, 0L);
            Interlocked.Exchange(ref _faults, 0L);
            _lastThreadResolution = null;
            lock (ConfigurationGate)
            {
                _lastResult =
                    "No canonical fatigue application has resolved.";
            }
        }

        internal static void Configure(BlueprintBuff fatigued,
            BlueprintBuff exhausted)
        {
            if (fatigued == null) throw new ArgumentNullException("fatigued");
            if (exhausted == null) throw new ArgumentNullException("exhausted");
            if (!string.Equals(fatigued.AssetGuid, FatiguedGuid,
                    StringComparison.Ordinal) ||
                !string.Equals(exhausted.AssetGuid, ExhaustedGuid,
                    StringComparison.Ordinal) ||
                ReferenceEquals(fatigued, exhausted))
            {
                throw new InvalidOperationException(
                    "Canonical fatigue blueprint identities do not match Kingmaker 2.1.7b.");
            }

            lock (ConfigurationGate)
            {
                if ((_fatigued != null && !ReferenceEquals(_fatigued, fatigued)) ||
                    (_exhausted != null && !ReferenceEquals(_exhausted,
                        exhausted)))
                {
                    throw new InvalidOperationException(
                        "Canonical fatigue runtime was reconfigured with different blueprint instances.");
                }
                _fatigued = fatigued;
                _exhausted = exhausted;
            }
        }

        internal static CanonicalFatigueApplicationResult
            ApplyPermanentAcadamaeFatigue(
            BuffCollection buffs, BlueprintBuff fatigued,
            UnitEntityData source)
        {
            if (buffs == null) throw new ArgumentNullException("buffs");
            if (fatigued == null) throw new ArgumentNullException("fatigued");
            if (source == null) throw new ArgumentNullException("source");
            BlueprintBuff configuredFatigue;
            BlueprintBuff configuredExhaustion;
            RequireConfigured(out configuredFatigue, out configuredExhaustion);
            if (!ReferenceEquals(fatigued, configuredFatigue))
                throw new InvalidOperationException(
                    "A non-canonical fatigue blueprint reached the canonical adapter.");

            long beforeSequence = Interlocked.Read(ref _sequence);
            Buff returned;
            using (CanonicalFatigueApplicationIntentScope.Request request =
                CanonicalFatigueApplicationIntentScope
                    .EnterAcadamaeEscalation(buffs, fatigued))
            {
                returned = buffs.AddBuff(fatigued, source, null);
            }
            Resolution resolution = _lastThreadResolution;
            bool correlated = resolution != null &&
                resolution.Sequence > beforeSequence &&
                ReferenceEquals(resolution.Buffs, buffs) &&
                resolution.Incoming == CanonicalConditionKind.Fatigued &&
                resolution.Intent == CanonicalFatigueApplicationIntent
                    .EscalateIfAlreadyFatigued;
            if (correlated && !resolution.ApplicationSucceeded)
            {
                return new CanonicalFatigueApplicationResult(false,
                    resolution.CordSubstituted,
                    DetermineState(buffs, configuredFatigue,
                        configuredExhaustion), null, resolution.Status);
            }

            Buff final = FindPreferredCondition(buffs, configuredFatigue,
                configuredExhaustion);
            bool succeeded = correlated ? resolution.ApplicationSucceeded :
                returned != null;
            if (succeeded && final != null) final.MakePermanent();
            CanonicalFatigueState after = DetermineState(buffs,
                configuredFatigue, configuredExhaustion);
            string status = correlated ? resolution.Status :
                (succeeded ? "successful-unobserved" : "suppressed-unobserved");
            return new CanonicalFatigueApplicationResult(succeeded,
                correlated && resolution.CordSubstituted, after, final, status);
        }

        internal static bool IsCanonicalApplication(UnitState state)
        {
            for (ApplicationScope scope = _activeScope; scope != null;
                scope = scope.Parent)
            {
                if (!scope.Ended && ReferenceEquals(scope.State, state))
                    return true;
            }
            return false;
        }

        internal static ApplicationScope Begin(BuffCollection buffs,
            BlueprintBuff blueprint)
        {
            BlueprintBuff fatigued;
            BlueprintBuff exhausted;
            if (_replacementDepth > 0 || buffs == null || blueprint == null ||
                !TryGetConfigured(out fatigued, out exhausted))
            {
                return null;
            }

            CanonicalConditionKind incoming;
            if (ReferenceEquals(blueprint, fatigued))
                incoming = CanonicalConditionKind.Fatigued;
            else if (ReferenceEquals(blueprint, exhausted))
                incoming = CanonicalConditionKind.Exhausted;
            else
                return null;

            CanonicalFatigueApplicationIntent intent =
                CanonicalFatigueApplicationIntentScope.Claim(buffs,
                    blueprint);
            Interlocked.Increment(ref _attempts);
            Buff fatigueBefore = buffs.GetBuff(fatigued);
            Buff exhaustionBefore = buffs.GetBuff(exhausted);
            UnitState state = buffs.Owner == null ? null : buffs.Owner.State;
            bool cord = state != null &&
                CordConditionRuntime.HasExactEquippedCord(state);

            // A nested same-sequence Cord application must observe the logical
            // pre-replacement state, not a provisional native fact that its outer
            // request will remove in the outer postfix.
            if (cord)
            {
                ApplicationScope outer = FindActive(state);
                if (outer != null && outer.CordEquipped &&
                    outer.Incoming == CanonicalConditionKind.Fatigued &&
                    outer.Before == CanonicalFatigueState.Neither)
                {
                    fatigueBefore = outer.FatigueBefore;
                    exhaustionBefore = outer.ExhaustionBefore;
                }
            }

            var scope = new ApplicationScope
            {
                Buffs = buffs,
                State = state,
                Incoming = incoming,
                FatigueBefore = fatigueBefore,
                ExhaustionBefore = exhaustionBefore,
                FatigueSourceBefore = CaptureSource(fatigueBefore, buffs),
                ExhaustionSourceBefore = CaptureSource(exhaustionBefore,
                    buffs),
                Before = StateOf(fatigueBefore, exhaustionBefore),
                Intent = intent,
                CordEquipped = cord,
                Parent = _activeScope
            };
            _activeScope = scope;
            return scope;
        }

        internal static void Resolve(ApplicationScope scope, ref Buff result)
        {
            if (scope == null || scope.Resolved) return;
            scope.Resolved = true;
            BlueprintBuff fatigued;
            BlueprintBuff exhausted;
            RequireConfigured(out fatigued, out exhausted);
            if (result == null)
            {
                Interlocked.Increment(ref _blocked);
                Record(scope, false, false, scope.Before,
                    "blocked-by-native-rule");
                return;
            }
            if (!NativeConditionPresent(scope))
            {
                RemoveRejectedProvisional(scope, result);
                result = null;
                Interlocked.Increment(ref _blocked);
                Record(scope, false, false, scope.Before,
                    "blocked-by-native-condition-immunity");
                return;
            }

            Interlocked.Increment(ref _successful);
            CanonicalFatigueStateDecision decision =
                CanonicalFatigueStatePolicy.Decide(scope.Before,
                    scope.Incoming, true, scope.Intent);
            bool cordSubstituted = scope.CordEquipped;
            string status;
            if (scope.CordEquipped)
            {
                ResolveCord(scope, decision, fatigued, exhausted, ref result);
                Interlocked.Increment(ref _cordSubstitutions);
                status = decision.EffectiveIncoming ==
                    CanonicalConditionKind.Exhausted
                        ? "cord-substituted-exhaustion"
                        : "cord-substituted-fatigue";
            }
            else
            {
                bool escalated = ResolveCanonical(scope, decision, fatigued,
                    exhausted, ref result);
                if (escalated)
                {
                    Interlocked.Increment(ref _escalated);
                    status = "acadamae-fatigue-escalated-to-exhausted";
                }
                else if (scope.Intent == CanonicalFatigueApplicationIntent
                    .NativePassthrough)
                {
                    status = scope.Incoming ==
                        CanonicalConditionKind.Exhausted
                            ? "native-exhaustion-passthrough"
                            : scope.Before ==
                                CanonicalFatigueState.Exhausted
                                ? "native-fatigue-no-downgrade"
                                : "native-fatigue-passthrough";
                }
                else
                {
                    status = decision.After == CanonicalFatigueState.Exhausted
                        ? "acadamae-exhausted-retained"
                        : "acadamae-fatigue-applied";
                }
            }

            CanonicalFatigueState after = DetermineState(scope.Buffs,
                fatigued, exhausted);
            Record(scope, true, cordSubstituted, after, status);
        }

        internal static void RecordFault(ApplicationScope scope,
            Exception exception)
        {
            if (exception == null) throw new ArgumentNullException("exception");
            Interlocked.Increment(ref _faults);
            lock (ConfigurationGate)
            {
                _lastResult = string.Format(CultureInfo.InvariantCulture,
                    "FAULT {0}: {1}", exception.GetType().Name,
                    exception.Message);
            }
            if (scope != null)
            {
                scope.Resolved = true;
                _lastThreadResolution = new Resolution
                {
                    Sequence = Interlocked.Increment(ref _sequence),
                    Buffs = scope.Buffs,
                    Incoming = scope.Incoming,
                    Intent = scope.Intent,
                    ApplicationSucceeded = false,
                    CordSubstituted = false,
                    Status = "coordination-fault"
                };
            }

            ModContext context;
            if (ModContext.TryGet(out context))
                context.Logger.Failure("fatigue", "canonical-application.failed",
                    "Canonical fatigue coordination failed after the native condition rule; the native result was retained.",
                    exception);
        }

        internal static void End(ApplicationScope scope)
        {
            if (scope == null || scope.Ended) return;
            scope.Ended = true;
            if (ReferenceEquals(_activeScope, scope))
            {
                _activeScope = scope.Parent;
                return;
            }

            for (ApplicationScope current = _activeScope; current != null;
                current = current.Parent)
            {
                if (ReferenceEquals(current.Parent, scope))
                {
                    current.Parent = scope.Parent;
                    return;
                }
            }
        }

        private static bool ResolveCanonical(ApplicationScope scope,
            CanonicalFatigueStateDecision decision, BlueprintBuff fatigued,
            BlueprintBuff exhausted, ref Buff result)
        {
            if (decision.Intent == CanonicalFatigueApplicationIntent
                .NativePassthrough)
            {
                return ResolveNativePassthrough(scope, fatigued, exhausted,
                    ref result);
            }

            Buff liveExhaustion = scope.Buffs.GetBuff(exhausted);
            ConditionSource incomingSource = CaptureSource(result,
                scope.Buffs);
            if (scope.Incoming == CanonicalConditionKind.Exhausted)
            {
                if (liveExhaustion != null)
                {
                    PreserveLongestDuration(liveExhaustion,
                        scope.FatigueSourceBefore,
                        scope.ExhaustionSourceBefore, incomingSource);
                    RemoveAll(scope.Buffs, fatigued, null);
                    Normalize(scope.Buffs, exhausted, liveExhaustion);
                    result = liveExhaustion;
                }
                return false;
            }

            if (decision.EffectiveIncoming == CanonicalConditionKind.Fatigued &&
                liveExhaustion == null)
            {
                Buff liveFatigue = scope.Buffs.GetBuff(fatigued) ?? result;
                PreserveLongestDuration(liveFatigue,
                    scope.FatigueSourceBefore, incomingSource);
                Normalize(scope.Buffs, fatigued, liveFatigue);
                result = liveFatigue;
                return false;
            }

            if (liveExhaustion != null)
            {
                PreserveLongestDuration(liveExhaustion,
                    scope.ExhaustionSourceBefore,
                    scope.FatigueSourceBefore, incomingSource);
                RemoveAll(scope.Buffs, fatigued, null);
                Normalize(scope.Buffs, exhausted, liveExhaustion);
                result = liveExhaustion;
                return decision.Escalated;
            }

            ConditionSource source = SelectLongest(
                scope.FatigueSourceBefore, incomingSource);
            Buff replacement = ApplyRelated(scope.Buffs, exhausted, source);
            if (replacement == null)
            {
                // Exhaustion immunity or another native rejection must not erase
                // the successful fatigue fact.
                Buff liveFatigue = scope.Buffs.GetBuff(fatigued) ?? result;
                PreserveLongestDuration(liveFatigue,
                    scope.FatigueSourceBefore, incomingSource);
                Normalize(scope.Buffs, fatigued, liveFatigue);
                result = liveFatigue;
                return false;
            }
            PreserveLongestDuration(replacement, source);
            RemoveAll(scope.Buffs, fatigued, null);
            Normalize(scope.Buffs, exhausted, replacement);
            result = replacement;
            return decision.Escalated;
        }

        private static bool ResolveNativePassthrough(ApplicationScope scope,
            BlueprintBuff fatigued, BlueprintBuff exhausted, ref Buff result)
        {
            Buff liveExhaustion = scope.Buffs.GetBuff(exhausted);
            if (scope.Incoming == CanonicalConditionKind.Fatigued)
            {
                if (liveExhaustion != null)
                {
                    // Preserve an already Exhausted unit without extending or
                    // replacing its native exhaustion fact or duration.
                    RemoveAll(scope.Buffs, fatigued, scope.FatigueBefore);
                    result = liveExhaustion;
                }
                return false;
            }

            if (liveExhaustion != null)
            {
                // Canonical Exhausted is the stronger native condition. Remove
                // only its exact weaker counterpart and exact duplicates.
                RemoveAll(scope.Buffs, fatigued, null);
                Normalize(scope.Buffs, exhausted, liveExhaustion);
                result = liveExhaustion;
            }
            return false;
        }

        private static void ResolveCord(ApplicationScope scope,
            CanonicalFatigueStateDecision decision, BlueprintBuff fatigued,
            BlueprintBuff exhausted, ref Buff result)
        {
            CordConditionRuntime.ResolveCanonical(scope.State,
                decision.EffectiveIncoming == CanonicalConditionKind.Exhausted
                    ? CordConditionKind.Exhaustion
                    : CordConditionKind.Fatigue);

            if (decision.EffectiveIncoming == CanonicalConditionKind.Fatigued)
            {
                if (scope.FatigueBefore == null)
                    RemoveAll(scope.Buffs, fatigued, null);
                else
                    PreserveLongestDuration(scope.FatigueBefore,
                        scope.FatigueSourceBefore,
                        CaptureSource(result, scope.Buffs));
                result = FindPreferredCondition(scope.Buffs, fatigued,
                    exhausted);
                return;
            }

            if (scope.ExhaustionBefore != null)
            {
                PreserveLongestDuration(scope.ExhaustionBefore,
                    scope.ExhaustionSourceBefore,
                    CaptureSource(result, scope.Buffs));
                RemoveAll(scope.Buffs, fatigued, scope.FatigueBefore);
                Normalize(scope.Buffs, exhausted, scope.ExhaustionBefore);
                result = scope.ExhaustionBefore;
                return;
            }

            ConditionSource incomingSource = CaptureSource(result,
                scope.Buffs);
            Buff residual = scope.Buffs.GetBuff(fatigued) ??
                scope.FatigueBefore;
            if (residual != null)
            {
                PreserveLongestDuration(residual,
                    scope.FatigueSourceBefore, incomingSource);
            }
            else if (scope.Incoming == CanonicalConditionKind.Exhausted)
            {
                residual = ApplyRelated(scope.Buffs, fatigued,
                    incomingSource);
            }
            RemoveAll(scope.Buffs, exhausted, null);
            if (residual != null) Normalize(scope.Buffs, fatigued, residual);
            result = residual;
        }

        private static Buff ApplyRelated(BuffCollection buffs,
            BlueprintBuff blueprint, ConditionSource source)
        {
            if (buffs == null || blueprint == null || source == null ||
                !source.Exists) return null;
            TimeSpan? duration = null;
            if (!source.Permanent)
            {
                TimeSpan remaining = source.EndTime - EffectiveGameTime();
                if (remaining <= TimeSpan.Zero)
                    remaining = TimeSpan.FromTicks(1L);
                duration = remaining;
            }

            Buff applied;
            _replacementDepth++;
            try
            {
                if (source.Context != null)
                    applied = buffs.AddBuff(blueprint, source.Context, duration);
                else if (source.SourceUnit != null)
                    applied = buffs.AddBuff(blueprint, source.SourceUnit,
                        duration);
                else
                    return null;
            }
            finally { _replacementDepth--; }
            if (applied != null && source.Permanent) applied.MakePermanent();
            return applied;
        }

        private static ConditionSource CaptureSource(Buff buff,
            BuffCollection buffs)
        {
            if (buff == null) return ConditionSource.Missing;
            return new ConditionSource
            {
                Exists = true,
                Permanent = buff.IsPermanent ||
                    buff.EndTime == TimeSpan.MaxValue,
                EndTime = buff.EndTime,
                Context = buff.Context,
                SourceUnit = buffs == null || buffs.Owner == null ? null :
                    buffs.Owner.Unit
            };
        }

        private static ConditionSource SelectLongest(
            params ConditionSource[] sources)
        {
            ConditionSource selected = ConditionSource.Missing;
            foreach (ConditionSource source in sources ??
                new ConditionSource[0])
            {
                if (source == null || !source.Exists) continue;
                if (!selected.Exists || source.Permanent &&
                        !selected.Permanent ||
                    source.Permanent == selected.Permanent &&
                        source.EndTime > selected.EndTime)
                    selected = source;
            }
            return selected;
        }

        private static void PreserveLongestDuration(Buff target,
            params ConditionSource[] sources)
        {
            if (target == null) return;
            var all = new List<ConditionSource> {
                CaptureSource(target, target.Owner == null ? null :
                    target.Owner.Buffs)
            };
            if (sources != null) all.AddRange(sources);
            ConditionSource selected = SelectLongest(all.ToArray());
            if (!selected.Exists) return;
            if (selected.Permanent)
                target.MakePermanent();
            else if (target.EndTime < selected.EndTime)
                target.EndTime = selected.EndTime;
        }

        private static TimeSpan EffectiveGameTime()
        {
            Game game = Game.Instance;
            if (game == null || game.TimeController == null)
                return TimeSpan.Zero;
            if (TurnBased.Controllers.CombatController.IsInTurnBasedCombat() &&
                game.TurnBasedCombatController != null)
                return game.TurnBasedCombatController.TurnStartTime;
            return game.TimeController.GameTime;
        }

        private static void Normalize(BuffCollection buffs,
            BlueprintBuff blueprint, Buff keep)
        {
            foreach (Buff duplicate in buffs.Enumerable.Where(value =>
                    value != null && ReferenceEquals(value.Blueprint, blueprint) &&
                    !ReferenceEquals(value, keep)).ToArray())
                duplicate.Remove();
        }

        private static void RemoveAll(BuffCollection buffs,
            BlueprintBuff blueprint, Buff keep)
        {
            foreach (Buff fact in buffs.Enumerable.Where(value => value != null &&
                    ReferenceEquals(value.Blueprint, blueprint) &&
                    !ReferenceEquals(value, keep)).ToArray())
                fact.Remove();
        }

        private static bool NativeConditionPresent(ApplicationScope scope)
        {
            if (scope == null || scope.State == null) return false;
            UnitCondition condition = scope.Incoming ==
                CanonicalConditionKind.Exhausted
                    ? UnitCondition.Exhausted : UnitCondition.Fatigued;
            return scope.State.HasCondition(condition);
        }

        private static void RemoveRejectedProvisional(ApplicationScope scope,
            Buff result)
        {
            if (scope == null || result == null) return;
            Buff before = scope.Incoming == CanonicalConditionKind.Exhausted
                ? scope.ExhaustionBefore : scope.FatigueBefore;
            if (!ReferenceEquals(result, before)) result.Remove();
        }

        private static CanonicalFatigueState DetermineState(
            BuffCollection buffs, BlueprintBuff fatigued,
            BlueprintBuff exhausted)
        {
            return StateOf(buffs == null ? null : buffs.GetBuff(fatigued),
                buffs == null ? null : buffs.GetBuff(exhausted));
        }

        private static CanonicalFatigueState StateOf(Buff fatigue,
            Buff exhaustion)
        {
            if (exhaustion != null) return CanonicalFatigueState.Exhausted;
            return fatigue == null ? CanonicalFatigueState.Neither :
                CanonicalFatigueState.Fatigued;
        }

        private static Buff FindPreferredCondition(BuffCollection buffs,
            BlueprintBuff fatigued, BlueprintBuff exhausted)
        {
            if (buffs == null) return null;
            return buffs.GetBuff(exhausted) ?? buffs.GetBuff(fatigued);
        }

        private static ApplicationScope FindActive(UnitState state)
        {
            for (ApplicationScope scope = _activeScope; scope != null;
                scope = scope.Parent)
            {
                if (!scope.Ended && ReferenceEquals(scope.State, state))
                    return scope;
            }
            return null;
        }

        private static void Record(ApplicationScope scope, bool succeeded,
            bool cordSubstituted, CanonicalFatigueState after, string status)
        {
            long sequence = Interlocked.Increment(ref _sequence);
            var resolution = new Resolution
            {
                Sequence = sequence,
                Buffs = scope.Buffs,
                Incoming = scope.Incoming,
                Intent = scope.Intent,
                ApplicationSucceeded = succeeded,
                CordSubstituted = cordSubstituted,
                Status = status
            };
            _lastThreadResolution = resolution;
            lock (ConfigurationGate)
            {
                _lastResult = string.Format(CultureInfo.InvariantCulture,
                    "sequence={0};before={1};incoming={2};intent={3};success={4};cord={5};after={6};status={7}",
                    sequence, scope.Before, scope.Incoming, scope.Intent,
                    succeeded, cordSubstituted, after, status);
            }
        }

        private static bool TryGetConfigured(out BlueprintBuff fatigued,
            out BlueprintBuff exhausted)
        {
            lock (ConfigurationGate)
            {
                fatigued = _fatigued;
                exhausted = _exhausted;
            }
            if (fatigued != null && exhausted != null) return true;

            BlueprintRoot root = BlueprintRoot.Instance;
            if (root == null || root.SystemMechanics == null) return false;
            BlueprintBuff rootFatigue = root.SystemMechanics.FatigueBuff;
            BlueprintBuff rootExhaustion = root.SystemMechanics.ExhaustedBuff;
            if (rootFatigue == null || rootExhaustion == null ||
                !string.Equals(rootFatigue.AssetGuid, FatiguedGuid,
                    StringComparison.Ordinal) ||
                !string.Equals(rootExhaustion.AssetGuid, ExhaustedGuid,
                    StringComparison.Ordinal))
                return false;
            Configure(rootFatigue, rootExhaustion);
            fatigued = rootFatigue;
            exhausted = rootExhaustion;
            return true;
        }

        private static void RequireConfigured(out BlueprintBuff fatigued,
            out BlueprintBuff exhausted)
        {
            if (!TryGetConfigured(out fatigued, out exhausted))
                throw new InvalidOperationException(
                    "Canonical Fatigued and Exhausted blueprints are unavailable.");
        }

        internal sealed class ApplicationScope
        {
            internal BuffCollection Buffs;
            internal UnitState State;
            internal CanonicalConditionKind Incoming;
            internal Buff FatigueBefore;
            internal Buff ExhaustionBefore;
            internal ConditionSource FatigueSourceBefore;
            internal ConditionSource ExhaustionSourceBefore;
            internal CanonicalFatigueState Before;
            internal CanonicalFatigueApplicationIntent Intent;
            internal bool CordEquipped;
            internal ApplicationScope Parent;
            internal bool Resolved;
            internal bool Ended;
        }

        internal sealed class ConditionSource
        {
            internal static readonly ConditionSource Missing =
                new ConditionSource();

            internal bool Exists;
            internal bool Permanent;
            internal TimeSpan EndTime;
            internal MechanicsContext Context;
            internal UnitEntityData SourceUnit;
        }

        private sealed class Resolution
        {
            internal long Sequence;
            internal BuffCollection Buffs;
            internal CanonicalConditionKind Incoming;
            internal CanonicalFatigueApplicationIntent Intent;
            internal bool ApplicationSucceeded;
            internal bool CordSubstituted;
            internal string Status;
        }
    }

    [HarmonyPatch(typeof(BuffCollection), "TriggerRuleApplyBuff",
        new[] { typeof(BlueprintBuff), typeof(MechanicsContext),
            typeof(TimeSpan?) })]
    [HarmonyAfter("CallOfTheWild")]
    internal static class CanonicalFatigueRuleApplyBuffPatch
    {
        private static void Prefix(BuffCollection __instance,
            BlueprintBuff __0,
            ref CanonicalFatigueApplicationRuntime.ApplicationScope __state)
        {
            __state = CanonicalFatigueApplicationRuntime.Begin(__instance, __0);
        }

        private static void Postfix(ref Buff __result,
            CanonicalFatigueApplicationRuntime.ApplicationScope __state)
        {
            try
            {
                CanonicalFatigueApplicationRuntime.Resolve(__state,
                    ref __result);
            }
            catch (Exception exception)
            {
                CanonicalFatigueApplicationRuntime.RecordFault(__state,
                    exception);
            }
        }

        private static Exception Finalizer(Exception __exception,
            CanonicalFatigueApplicationRuntime.ApplicationScope __state)
        {
            CanonicalFatigueApplicationRuntime.End(__state);
            return __exception;
        }
    }
}
