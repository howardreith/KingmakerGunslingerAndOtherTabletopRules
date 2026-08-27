using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Harmony12;
using Kingmaker;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Root;
using Kingmaker.Controllers.Combat;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Abilities;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using KingmakerGunslinger.Bootstrap;
using TurnBased.Controllers;

namespace KingmakerGunslinger.Summoning
{
    /// <summary>
    /// Carries the exact action-economy decision made when a live
    /// UnitUseAbility starts into the exact RuleCastSpell it constructs. The
    /// entry is normally released at the authoritative command-end callback;
    /// a successful live turn-based enrollment retains it only until every
    /// correlated summon is natively ready. This spans Owlcat's deferred spawn
    /// graph and all members of one multi-summon. It is needed because spending
    /// a prepared slot can make a later
    /// RequireFullRoundAction query describe the now-unavailable spell rather
    /// than the command that is already resolving.
    /// </summary>
    internal static class SummonAcceleratedInvocationRuntime
    {
        private sealed class Entry
        {
            internal UnitUseAbility Command;
            internal AbilityData Ability;
            internal RuleCastSpell Rule;
            internal bool EnrollmentArmed;
        }

        private static readonly Dictionary<UnitUseAbility, Entry> Commands =
            new Dictionary<UnitUseAbility, Entry>(
                ReferenceComparer<UnitUseAbility>.Instance);
        private static readonly Dictionary<RuleCastSpell, Entry> Rules =
            new Dictionary<RuleCastSpell, Entry>(
                ReferenceComparer<RuleCastSpell>.Instance);
        [ThreadStatic] private static Entry _activeCommand;
        [ThreadStatic] private static Entry _activeRule;
        private static string _diagnosticTrace = string.Empty;

        internal static string DiagnosticTrace
        { get { return _diagnosticTrace; } }

        internal static void RecordDiagnostic(string value)
        { Record(value); }

        internal static void ResetDiagnostics()
        {
            Clear();
            _diagnosticTrace = string.Empty;
        }

        internal static void Clear()
        {
            Commands.Clear();
            Rules.Clear();
            _activeCommand = null;
            _activeRule = null;
            SummonCurrentTurnEnrollmentRuntime.Clear();
        }

        internal static void Arm(UnitUseAbility command, AbilityData ability,
            UnitCommand.CommandType commandType)
        {
            if (command == null || ability == null ||
                ability.Blueprint == null)
            {
                Record("arm=invalid");
                return;
            }
            bool actualFullRound = ability.RequireFullRoundAction;
            bool accepted = ability.Spellbook != null &&
                ability.Blueprint.Type == AbilityType.Spell &&
                (ability.Blueprint.SpellDescriptor &
                    SpellDescriptor.Summoning) != 0 &&
                !actualFullRound &&
                (commandType == UnitCommand.CommandType.Standard ||
                    commandType == UnitCommand.CommandType.Swift);
            Record("arm=spellbook:" + (ability.Spellbook != null) +
                ",type:" + ability.Blueprint.Type +
                ",summoning:" + ((ability.Blueprint.SpellDescriptor &
                    SpellDescriptor.Summoning) != 0) +
                ",blueprintFull:" + ability.Blueprint.IsFullRoundAction +
                ",actualFull:" + actualFullRound +
                ",command:" + commandType + ",accepted:" + accepted);
            if (!accepted ||
                commandType != UnitCommand.CommandType.Standard &&
                    commandType != UnitCommand.CommandType.Swift)
                return;
            if (!Commands.ContainsKey(command))
                Commands.Add(command, new Entry {
                    Command = command, Ability = ability
                });
        }

        internal static void BeginCommand(UnitUseAbility command)
        {
            _activeCommand = null;
            Entry entry;
            if (command != null && Commands.TryGetValue(command, out entry))
                _activeCommand = entry;
            Record("begin-command=found:" + (_activeCommand != null));
        }

        internal static void AttachRule(RuleCastSpell rule)
        {
            Entry entry = _activeCommand;
            bool spellMatches = entry != null && rule != null &&
                ReferenceEquals(rule.Spell, entry.Ability);
            Record("attach-rule=active:" + (entry != null) +
                ",spellMatch:" + spellMatches);
            if (entry == null || rule == null || entry.Rule != null ||
                Rules.ContainsKey(rule) ||
                !ReferenceEquals(rule.Spell, entry.Ability)) return;
            entry.Rule = rule;
            Rules.Add(rule, entry);
        }

        internal static void BeginRule(RuleCastSpell rule)
        {
            _activeRule = null;
            Entry entry = null;
            bool found = rule != null && Rules.TryGetValue(rule, out entry);
            bool exact = found &&
                ReferenceEquals(rule.Spell, entry.Ability) &&
                ReferenceEquals(rule.Initiator, entry.Command.Executor);
            Record("begin-rule=found:" + found + ",exact:" + exact);
            if (!found ||
                !ReferenceEquals(rule.Spell, entry.Ability) ||
                !ReferenceEquals(rule.Initiator, entry.Command.Executor))
                return;
            _activeRule = entry;
        }

        internal static bool IsExactAcceleratedCast(AbilityData ability,
            UnitEntityData caster)
        {
            int matches = 0;
            foreach (Entry entry in Rules.Values)
            {
                if (entry == null || ability == null || caster == null ||
                    !ReferenceEquals(entry.Ability, ability) ||
                    !ReferenceEquals(entry.Command.Spell, ability) ||
                    !ReferenceEquals(entry.Command.Executor, caster) ||
                    !ReferenceEquals(entry.Rule.Spell, ability) ||
                    !ReferenceEquals(entry.Rule.Initiator, caster))
                    continue;
                matches++;
            }
            bool result = matches == 1;
            Record("inspect=pendingMatches:" + matches +
                ",exact:" + result);
            return result;
        }

        internal static bool TryTrackSummon(AbilityData ability,
            UnitEntityData caster, UnitEntityData summon,
            CombatController controller, TurnController turn, int round)
        {
            Entry match = null;
            int matches = 0;
            foreach (Entry entry in Rules.Values)
            {
                if (entry == null || ability == null || caster == null ||
                    !ReferenceEquals(entry.Ability, ability) ||
                    !ReferenceEquals(entry.Command.Spell, ability) ||
                    !ReferenceEquals(entry.Command.Executor, caster) ||
                    !ReferenceEquals(entry.Rule.Spell, ability) ||
                    !ReferenceEquals(entry.Rule.Initiator, caster))
                    continue;
                match = entry;
                matches++;
            }
            bool tracked = matches == 1 &&
                SummonCurrentTurnEnrollmentRuntime.Register(
                    match.Command, caster, summon, controller, turn, round);
            Record("track=pendingMatches:" + matches +
                ",tracked:" + tracked);
            return tracked;
        }

        internal static void EndRule(RuleCastSpell rule)
        {
            Entry entry;
            if (rule == null || !Rules.TryGetValue(rule, out entry)) return;
            if (ReferenceEquals(_activeRule, entry) && rule.Success)
            {
                entry.EnrollmentArmed =
                    SummonCurrentTurnEnrollmentRuntime.ArmInvocation(
                        entry.Command, entry.Command.Executor);
                Record("enrollment-arm=success:" +
                    entry.EnrollmentArmed);
            }
            if (ReferenceEquals(_activeRule, entry)) _activeRule = null;
            Record("end-rule=retained-until-command-end");
        }

        internal static void EndCommand(UnitUseAbility command)
        {
            if (_activeCommand != null &&
                ReferenceEquals(_activeCommand.Command, command))
                _activeCommand = null;
        }

        internal static void CancelCommand(UnitUseAbility command)
        {
            Entry entry;
            if (command != null && Commands.TryGetValue(command, out entry))
            {
                if (entry.EnrollmentArmed)
                {
                    SummonCurrentTurnEnrollmentRuntime.SealInvocation(command);
                    Record("command-end=retained-for-enrollment");
                }
                else
                {
                    Release(entry);
                    Record("command-end=released");
                }
            }
        }

        internal static void CompleteInvocations(
            IEnumerable<UnitUseAbility> invocations)
        {
            if (invocations == null) return;
            var completed = new List<Entry>();
            foreach (UnitUseAbility invocation in invocations)
            {
                Entry entry;
                if (invocation != null && Commands.TryGetValue(invocation,
                        out entry) && !completed.Contains(entry))
                    completed.Add(entry);
            }
            foreach (Entry entry in completed) Release(entry);
            if (completed.Count > 0)
                Record("enrollment-complete=invocations:" +
                    completed.Count);
        }

        private static void Release(Entry entry)
        {
            if (entry == null) return;
            Commands.Remove(entry.Command);
            if (entry.Rule != null) Rules.Remove(entry.Rule);
            if (ReferenceEquals(_activeCommand, entry))
                _activeCommand = null;
            if (ReferenceEquals(_activeRule, entry)) _activeRule = null;
        }

        private static void Record(string value)
        {
            string next = string.IsNullOrEmpty(_diagnosticTrace) ? value :
                _diagnosticTrace + "|" + value;
            _diagnosticTrace = next.Length <= 4096 ? next :
                next.Substring(next.Length - 4096);
        }

        private sealed class ReferenceComparer<T> : IEqualityComparer<T>
            where T : class
        {
            internal static readonly ReferenceComparer<T> Instance =
                new ReferenceComparer<T>();
            public bool Equals(T left, T right)
            { return ReferenceEquals(left, right); }
            public int GetHashCode(T value)
            { return RuntimeHelpers.GetHashCode(value); }
        }
    }

    /// <summary>
    /// Keeps the exact caster TurnController from advancing only while
    /// Owlcat's deferred entity and initiative controllers enroll the exact
    /// successful summons from that turn. Kingmaker's native combat-join
    /// controller deliberately skips its unit scan while a turn is active, so
    /// the exact correlated summon is passed through UnitEntityData.JoinCombat
    /// once it is live. The native combat and initiative event handlers do the
    /// rest; no initiative or action resource is edited here.
    /// </summary>
    internal static class SummonCurrentTurnEnrollmentRuntime
    {
        private const int MaxHoldAttempts = 240;
        private static readonly List<Window> Windows = new List<Window>();

        internal static bool ArmInvocation(UnitUseAbility invocation,
            UnitEntityData caster)
        {
            CombatController controller = Game.Instance == null ? null :
                Game.Instance.TurnBasedCombatController;
            TurnController turn = controller == null ? null :
                controller.CurrentTurn;
            if (invocation == null || caster == null || controller == null ||
                turn == null || !CombatController.IsInTurnBasedCombat() ||
                caster.CombatState == null ||
                !caster.CombatState.IsInCombat || turn.IsEnding ||
                !ReferenceEquals(invocation.Executor, caster) ||
                !ReferenceEquals(turn.Unit, caster) ||
                !ReferenceEquals(controller.CurrentTurn, turn))
                return false;

            Window window = FindWindow(controller, turn);
            if (window == null)
            {
                window = new Window
                {
                    Controller = controller,
                    CasterTurn = turn,
                    Caster = caster,
                    Round = controller.RoundNumber
                };
                Windows.Add(window);
            }
            else if (!ReferenceEquals(window.Caster, caster) ||
                window.Round != controller.RoundNumber)
            {
                return false;
            }

            AddReference(window.Invocations, invocation);
            SummonAcceleratedInvocationRuntime.RecordDiagnostic(
                "enrollment-arm=invocation:" + window.Invocations.Count +
                ",round:" + window.Round);
            return true;
        }

        internal static bool Register(UnitUseAbility invocation,
            UnitEntityData caster, UnitEntityData summon,
            CombatController controller, TurnController turn, int round)
        {
            if (invocation == null || caster == null || summon == null ||
                controller == null || turn == null || round < 0 ||
                summon.Destroyed || !ReferenceEquals(turn.Unit, caster) ||
                !ReferenceEquals(controller.CurrentTurn, turn))
                return false;

            Window window = FindWindow(controller, turn);
            if (window == null ||
                !ContainsReference(window.Invocations, invocation) ||
                !ReferenceEquals(window.Caster, caster) ||
                window.Round != round)
            {
                return false;
            }

            if (ContainsReference(window.Summons, summon))
            {
                SummonAcceleratedInvocationRuntime.RecordDiagnostic(
                    "enrollment-register=duplicate-unit");
                return true;
            }
            window.Summons.Add(summon);
            SummonAcceleratedInvocationRuntime.RecordDiagnostic(
                "enrollment-register=unit:" + window.Summons.Count +
                ",round:" + round);
            return true;
        }

        internal static void SealInvocation(UnitUseAbility invocation)
        {
            if (invocation == null) return;
            foreach (Window window in Windows)
            {
                if (!ContainsReference(window.Invocations, invocation))
                    continue;
                AddReference(window.SealedInvocations, invocation);
                SummonAcceleratedInvocationRuntime.RecordDiagnostic(
                    "enrollment-invocation=sealed");
            }
        }

        internal static void ObserveInitiative(
            CombatController controller, UnitEntityData unit)
        {
            if (controller == null || unit == null) return;
            foreach (Window window in Windows)
            {
                if (!ReferenceEquals(window.Controller, controller) ||
                    !ContainsReference(window.Summons, unit))
                    continue;
                if (!ContainsReference(window.InitiativeObserved, unit))
                {
                    window.InitiativeObserved.Add(unit);
                    SummonAcceleratedInvocationRuntime.RecordDiagnostic(
                        "enrollment-initiative=observed");
                }
            }
        }

        internal static void JoinMissingSummons()
        {
            CombatController controller = Game.Instance == null ? null :
                Game.Instance.TurnBasedCombatController;
            TurnController turn = controller == null ? null :
                controller.CurrentTurn;
            if (controller == null || turn == null ||
                !CombatController.IsInTurnBasedCombat()) return;

            for (int windowIndex = Windows.Count - 1;
                windowIndex >= 0; windowIndex--)
            {
                Window window = Windows[windowIndex];
                if (!ReferenceEquals(window.Controller, controller) ||
                    !ReferenceEquals(window.CasterTurn, turn)) continue;

                bool removed = RemoveDestroyed(window);
                if (removed && window.Summons.Count == 0)
                {
                    SummonAcceleratedInvocationRuntime.RecordDiagnostic(
                        "enrollment-destroyed=window-released");
                    SummonAcceleratedInvocationRuntime.CompleteInvocations(
                        window.Invocations);
                    Windows.RemoveAt(windowIndex);
                    continue;
                }
                SummonTurnEnrollmentRequest request = Capture(window,
                    controller, turn);
                SummonTurnEnrollmentDecision decision =
                    SummonTurnEnrollmentPolicy.Evaluate(request);
                if (decision.Disposition !=
                    SummonTurnEnrollmentDisposition.AwaitCombatEnrollment)
                    continue;

                bool failed = false;
                foreach (UnitEntityData summon in window.Summons)
                {
                    if (summon == null || summon.Destroyed ||
                        !summon.IsInGame || summon.CombatState == null ||
                        summon.CombatState.IsInCombat ||
                        ContainsReference(window.JoinAttempted, summon))
                        continue;

                    window.JoinAttempted.Add(summon);
                    SummonAcceleratedInvocationRuntime.RecordDiagnostic(
                        "enrollment-native-join=attempt:" +
                        RuntimeHelpers.GetHashCode(summon));
                    summon.JoinCombat();
                    bool joined = summon.CombatState.IsInCombat;
                    SummonAcceleratedInvocationRuntime.RecordDiagnostic(
                        "enrollment-native-join=joined:" + joined);
                    if (!joined) failed = true;
                }

                if (!failed) continue;
                SummonAcceleratedInvocationRuntime.RecordDiagnostic(
                    "enrollment-native-join=failed-open");
                SummonAcceleratedInvocationRuntime.CompleteInvocations(
                    window.Invocations);
                Windows.RemoveAt(windowIndex);
            }
        }

        internal static bool AllowTurnTick(TurnController turn)
        {
            if (turn == null) return true;
            CombatController controller = Game.Instance == null ? null :
                Game.Instance.TurnBasedCombatController;
            Window window = FindWindow(controller, turn);
            if (window == null) return true;

            bool removed = RemoveDestroyed(window);
            if (removed && window.Summons.Count == 0)
            {
                SummonAcceleratedInvocationRuntime.RecordDiagnostic(
                    "enrollment-destroyed=window-released");
                SummonAcceleratedInvocationRuntime.CompleteInvocations(
                    window.Invocations);
                Windows.Remove(window);
                return true;
            }
            SummonTurnEnrollmentRequest request = Capture(window, controller,
                turn);
            SummonTurnEnrollmentDecision decision =
                SummonTurnEnrollmentPolicy.Evaluate(request);
            if (!window.HasLastDisposition ||
                window.LastDisposition != decision.Disposition)
            {
                window.HasLastDisposition = true;
                window.LastDisposition = decision.Disposition;
                SummonAcceleratedInvocationRuntime.RecordDiagnostic(
                    "enrollment-turn-tick=" + decision.Disposition +
                    ",units:" + request.SuccessfulSummonCount +
                    ",live:" + request.LiveSummonCount +
                    ",combat:" + request.CombatEnrolledCount +
                    ",order:" + request.TurnOrderMemberCount +
                    ",prepared:" + request.InitiativePreparedCount +
                    ",holds:" + request.HoldAttemptCount);
            }
            if (decision.HoldCasterEnd)
            {
                window.HoldAttempts++;
                return false;
            }

            SummonAcceleratedInvocationRuntime.CompleteInvocations(
                window.Invocations);
            Windows.Remove(window);
            return true;
        }

        internal static void Clear()
        { Windows.Clear(); }

        private static SummonTurnEnrollmentRequest Capture(Window window,
            CombatController controller, TurnController turn)
        {
            int live = 0;
            int combat = 0;
            int order = 0;
            int prepared = 0;
            int acted = 0;
            foreach (UnitEntityData summon in window.Summons)
            {
                if (summon == null || summon.Destroyed) continue;
                if (summon.IsInGame) live++;
                if (summon.CombatState != null &&
                    summon.CombatState.IsInCombat)
                    combat++;
                if (ContainsUnit(controller == null ? null :
                        controller.SortedUnits, summon))
                    order++;
                if (summon.CombatState != null &&
                    summon.CombatState.Prepared &&
                    ContainsReference(window.InitiativeObserved, summon))
                    prepared++;
                if (controller != null && controller.CurrentTurn != null &&
                    ReferenceEquals(controller.CurrentTurn.Unit, summon) &&
                    controller.CurrentTurn.IsActed())
                    acted++;
            }
            bool casterInCombat = window.Caster != null &&
                !window.Caster.Destroyed &&
                window.Caster.CombatState != null &&
                window.Caster.CombatState.IsInCombat;
            return new SummonTurnEnrollmentRequest
            {
                InCombat = casterInCombat,
                TurnBased = CombatController.IsInTurnBasedCombat(),
                GenuineSummon = true,
                CreatedDuringCasterTurn = true,
                SameCombatController = controller != null &&
                    ReferenceEquals(controller, window.Controller),
                SameRound = controller != null &&
                    controller.RoundNumber == window.Round,
                CasterTurnStillCurrent = controller != null &&
                    ReferenceEquals(controller.CurrentTurn, turn) &&
                    ReferenceEquals(turn, window.CasterTurn) &&
                    ReferenceEquals(turn.Unit, window.Caster),
                InvocationSealed = AllInvocationsSealed(window),
                SuccessfulSummonCount = window.Summons.Count,
                LiveSummonCount = live,
                CombatEnrolledCount = combat,
                TurnOrderMemberCount = order,
                InitiativePreparedCount = prepared,
                AlreadyActedCount = acted,
                HoldAttemptCount = window.HoldAttempts,
                MaxHoldAttempts = MaxHoldAttempts
            };
        }

        private static bool AllInvocationsSealed(Window window)
        {
            if (window.Invocations.Count == 0) return false;
            foreach (UnitUseAbility invocation in window.Invocations)
                if (!ContainsReference(window.SealedInvocations, invocation))
                    return false;
            return true;
        }

        private static bool ContainsUnit(IEnumerable<UnitEntityData> units,
            UnitEntityData expected)
        {
            if (units == null || expected == null) return false;
            foreach (UnitEntityData unit in units)
                if (ReferenceEquals(unit, expected)) return true;
            return false;
        }

        private static Window FindWindow(CombatController controller,
            TurnController turn)
        {
            if (controller == null || turn == null) return null;
            foreach (Window window in Windows)
                if (ReferenceEquals(window.Controller, controller) &&
                    ReferenceEquals(window.CasterTurn, turn))
                    return window;
            return null;
        }

        private static bool RemoveDestroyed(Window window)
        {
            bool removed = false;
            for (int index = window.Summons.Count - 1; index >= 0; index--)
            {
                UnitEntityData summon = window.Summons[index];
                if (summon != null && !summon.Destroyed) continue;
                window.Summons.RemoveAt(index);
                RemoveReference(window.JoinAttempted, summon);
                RemoveReference(window.InitiativeObserved, summon);
                removed = true;
            }
            return removed;
        }

        private static bool ContainsReference<T>(List<T> values, T expected)
            where T : class
        {
            foreach (T value in values)
                if (ReferenceEquals(value, expected)) return true;
            return false;
        }

        private static void AddReference<T>(List<T> values, T value)
            where T : class
        {
            if (!ContainsReference(values, value)) values.Add(value);
        }

        private static void RemoveReference<T>(List<T> values, T expected)
            where T : class
        {
            for (int index = values.Count - 1; index >= 0; index--)
                if (ReferenceEquals(values[index], expected))
                    values.RemoveAt(index);
        }

        private sealed class Window
        {
            internal CombatController Controller;
            internal TurnController CasterTurn;
            internal UnitEntityData Caster;
            internal int Round;
            internal readonly List<UnitUseAbility> Invocations =
                new List<UnitUseAbility>();
            internal readonly List<UnitUseAbility> SealedInvocations =
                new List<UnitUseAbility>();
            internal readonly List<UnitEntityData> Summons =
                new List<UnitEntityData>();
            internal readonly List<UnitEntityData> InitiativeObserved =
                new List<UnitEntityData>();
            internal readonly List<UnitEntityData> JoinAttempted =
                new List<UnitEntityData>();
            internal int HoldAttempts;
            internal bool HasLastDisposition;
            internal SummonTurnEnrollmentDisposition LastDisposition;
        }
    }

    /// <summary>
    /// Corrects the one-round appearance grace that RuleSummonUnit derives
    /// from the immutable blueprint when the exact live invocation has been
    /// accelerated to Standard or Swift. The correlated enrollment runtime
    /// also preserves the caster as the native current-turn anchor while the
    /// exact live summon passes through Owlcat's native combat-join and
    /// initiative handlers. Initiative, action resources, AI, and subsequent
    /// scheduling remain native.
    /// </summary>
    internal static class SummonSameTurnActivationRuntime
    {
        internal static SummonSameTurnActivationDecision Inspect(
            RuleSummonUnit rule,
            out SummonSameTurnActivationRequest request)
        {
            RuntimeSnapshot snapshot = Capture(rule);
            request = snapshot.Request;
            return SummonSameTurnActivationPolicy.Evaluate(request);
        }

        internal static SummonSameTurnActivationDecision TryRepair(
            RuleSummonUnit rule)
        {
            RuntimeSnapshot snapshot = Capture(rule);
            SummonSameTurnActivationDecision decision =
                SummonSameTurnActivationPolicy.Evaluate(snapshot.Request);
            if (decision.ShouldRepair)
            {
                TimeSpan originalEndTime = snapshot.Lifecycle.EndTime;
                bool lifecycleChanged = false;
                try
                {
                    if (decision.RemoveLifecycleGrace)
                    {
                        snapshot.Lifecycle.EndTime = originalEndTime -
                            TimeSpan.FromSeconds(SummonSameTurnActivationPolicy
                                .NativeGraceSeconds);
                        snapshot.Summon.Descriptor.Buffs.UpdateNextEvent();
                        lifecycleChanged = true;
                    }

                    if (decision.RemoveAppearanceLock)
                    {
                        snapshot.Summon.Descriptor.Buffs.RemoveFact(
                            snapshot.Appearance);
                        if (ReferenceEquals(snapshot.Summon.Descriptor.Buffs
                                .GetBuff(snapshot.Appearance.Blueprint),
                                snapshot.Appearance))
                            throw new InvalidOperationException(
                                "The canonical summon appearance lock " +
                                "remained after exact-fact removal.");
                    }
                }
                catch
                {
                    if (lifecycleChanged)
                    {
                        snapshot.Lifecycle.EndTime = originalEndTime;
                        snapshot.Summon.Descriptor.Buffs.UpdateNextEvent();
                    }
                    throw;
                }
            }

            if (CanTrackEnrollment(snapshot, decision))
            {
                SummonAcceleratedInvocationRuntime.TryTrackSummon(
                    snapshot.Ability, snapshot.Caster, snapshot.Summon,
                    snapshot.Controller, snapshot.Turn,
                    snapshot.Controller.RoundNumber);
            }
            return decision;
        }

        private static bool CanTrackEnrollment(RuntimeSnapshot snapshot,
            SummonSameTurnActivationDecision decision)
        {
            SummonSameTurnActivationRequest request = snapshot.Request;
            bool normalizedOrNative = decision.ShouldRepair ||
                decision.Disposition ==
                    SummonSameTurnActivationDisposition.AlreadyEligible ||
                decision.Disposition == SummonSameTurnActivationDisposition
                    .NativeAlreadyImmediate;
            return normalizedOrNative && request.InCombat &&
                request.TurnBased && request.GenuineSummonRule &&
                request.SummoningSpell && request.HasLiveSummon &&
                request.CasterMatchesInvocation &&
                request.CasterOwnsCurrentTurn &&
                request.AcceleratedCommandCorrelated &&
                request.HasLifecycle && request.LifecycleContextMatches &&
                (!request.HasAppearanceLock ||
                    request.AppearanceContextMatches) &&
                snapshot.Controller != null && snapshot.Turn != null;
        }

        private static RuntimeSnapshot Capture(RuleSummonUnit rule)
        {
            UnitEntityData summon = rule == null ? null : rule.SummonedUnit;
            AbilityData ability = rule == null || rule.Context == null ||
                rule.Context.SourceAbilityContext == null ? null :
                rule.Context.SourceAbilityContext.Ability;
            UnitEntityData caster = rule == null ? null : rule.Initiator;
            var controller = Game.Instance == null ? null :
                Game.Instance.TurnBasedCombatController;
            var turn = controller == null ? null : controller.CurrentTurn;
            var mechanics = BlueprintRoot.Instance == null ? null :
                BlueprintRoot.Instance.SystemMechanics;
            Buff lifecycle = summon == null || summon.Descriptor == null ||
                mechanics == null || mechanics.SummonedUnitBuff == null ?
                null : summon.Descriptor.Buffs.GetBuff(
                    mechanics.SummonedUnitBuff);
            Buff appearance = summon == null || summon.Descriptor == null ||
                mechanics == null || mechanics.SummonedUnitAppearBuff == null ?
                null : summon.Descriptor.Buffs.GetBuff(
                    mechanics.SummonedUnitAppearBuff);
            double expectedSeconds = rule == null ? -1d :
                (rule.Duration.Seconds + rule.BonusDuration.Seconds)
                    .TotalSeconds;
            var sourceAbilityContext = rule == null || rule.Context == null ?
                null : rule.Context.SourceAbilityContext;
            bool exactSpell = ability != null && ability.Blueprint != null &&
                ability.Spellbook != null &&
                ability.Blueprint.Type == AbilityType.Spell &&
                (ability.Blueprint.SpellDescriptor &
                    SpellDescriptor.Summoning) != 0;

            return new RuntimeSnapshot
            {
                Ability = ability,
                Caster = caster,
                Controller = controller,
                Turn = turn,
                Summon = summon,
                Lifecycle = lifecycle,
                Appearance = appearance,
                Request = new SummonSameTurnActivationRequest
                {
                    InCombat = caster != null && caster.CombatState != null &&
                        caster.CombatState.IsInCombat,
                    TurnBased = CombatController.IsInTurnBasedCombat(),
                    GenuineSummonRule = rule != null && rule.Context != null,
                    SummoningSpell = exactSpell,
                    HasLiveSummon = summon != null && !summon.Destroyed &&
                        summon.Descriptor != null,
                    CasterMatchesInvocation = ability != null &&
                        ability.Caster != null && caster != null &&
                        ReferenceEquals(ability.Caster, caster.Descriptor) &&
                        ReferenceEquals(ability.Caster.Unit, caster) &&
                        ReferenceEquals(rule.Context.MaybeCaster, caster),
                    CasterOwnsCurrentTurn = turn != null &&
                        !turn.IsEnding &&
                        ReferenceEquals(turn.Unit, caster),
                    AcceleratedCommandCorrelated =
                        SummonAcceleratedInvocationRuntime
                            .IsExactAcceleratedCast(ability, caster),
                    ActualRequiresFullRound = ability == null ||
                        ability.RequireFullRoundAction,
                    BlueprintRequiresFullRound = ability != null &&
                        ability.Blueprint != null &&
                        ability.Blueprint.IsFullRoundAction,
                    SummonAlreadyActed = turn != null && summon != null &&
                        ReferenceEquals(turn.Unit, summon) && turn.IsActed(),
                    HasLifecycle = lifecycle != null,
                    LifecycleContextMatches = lifecycle != null &&
                        lifecycle.Context != null &&
                        sourceAbilityContext != null &&
                        ReferenceEquals(lifecycle.Context
                            .SourceAbilityContext, sourceAbilityContext) &&
                        ReferenceEquals(lifecycle.Context
                            .SourceAbilityContext.Ability, ability) &&
                        ReferenceEquals(lifecycle.Context.MaybeCaster, caster),
                    HasAppearanceLock = appearance != null,
                    AppearanceContextMatches = appearance != null &&
                        appearance.Context != null &&
                        sourceAbilityContext != null &&
                        ReferenceEquals(appearance.Context
                            .SourceAbilityContext, sourceAbilityContext) &&
                        ReferenceEquals(appearance.Context
                            .SourceAbilityContext.Ability, ability) &&
                        ReferenceEquals(appearance.Context.MaybeCaster, caster),
                    ExpectedLifecycleSeconds = expectedSeconds,
                    ObservedLifecycleSeconds = lifecycle == null ? -1d :
                        lifecycle.TimeLeft.TotalSeconds
                }
            };
        }

        private sealed class RuntimeSnapshot
        {
            internal AbilityData Ability { get; set; }
            internal UnitEntityData Caster { get; set; }
            internal CombatController Controller { get; set; }
            internal TurnController Turn { get; set; }
            internal UnitEntityData Summon { get; set; }
            internal Buff Lifecycle { get; set; }
            internal Buff Appearance { get; set; }
            internal SummonSameTurnActivationRequest Request { get; set; }
        }
    }

    [HarmonyPatch(typeof(UnitUseAbility), MethodType.Constructor,
        typeof(UnitCommand.CommandType), typeof(AbilityData),
        typeof(Kingmaker.Utility.TargetWrapper))]
    internal static class SummonAcceleratedInvocationCommandConstructorPatch
    {
        private static void Postfix(UnitUseAbility __instance,
            UnitCommand.CommandType __0, AbilityData __1)
        {
            SummonAcceleratedInvocationRuntime.Arm(__instance, __1, __0);
        }
    }

    [HarmonyPatch(typeof(UnitUseAbility), "OnAction", new Type[0])]
    internal static class SummonAcceleratedInvocationCommandPatch
    {
        private static void Prefix(UnitUseAbility __instance)
        { SummonAcceleratedInvocationRuntime.BeginCommand(__instance); }

        private static void Postfix(UnitUseAbility __instance)
        { SummonAcceleratedInvocationRuntime.EndCommand(__instance); }
    }

    [HarmonyPatch(typeof(RuleCastSpell), MethodType.Constructor,
        typeof(AbilityData), typeof(Kingmaker.Utility.TargetWrapper))]
    internal static class SummonAcceleratedInvocationRuleConstructorPatch
    {
        private static void Postfix(RuleCastSpell __instance)
        { SummonAcceleratedInvocationRuntime.AttachRule(__instance); }
    }

    [HarmonyPatch(typeof(RuleCastSpell), "OnTrigger",
        new Type[] { typeof(RulebookEventContext) })]
    internal static class SummonAcceleratedInvocationRulePatch
    {
        private static void Prefix(RuleCastSpell __instance)
        { SummonAcceleratedInvocationRuntime.BeginRule(__instance); }

        private static void Postfix(RuleCastSpell __instance)
        { SummonAcceleratedInvocationRuntime.EndRule(__instance); }
    }

    [HarmonyPatch(typeof(UnitUseAbility), "OnEnded",
        new Type[] { typeof(bool) })]
    internal static class SummonAcceleratedInvocationCleanupPatch
    {
        private static void Postfix(UnitUseAbility __instance)
        { SummonAcceleratedInvocationRuntime.CancelCommand(__instance); }
    }

    [HarmonyPatch(typeof(SceneEntitiesState), "Dispose", new Type[0])]
    internal static class SummonAcceleratedInvocationSceneCleanupPatch
    {
        private static void Prefix()
        { SummonAcceleratedInvocationRuntime.Clear(); }
    }

    [HarmonyPatch(typeof(CombatController),
        "HandleUnitRollsInitiative",
        new Type[] { typeof(RuleInitiativeRoll) })]
    internal static class SummonCurrentTurnInitiativeObservationPatch
    {
        private static void Postfix(CombatController __instance,
            RuleInitiativeRoll rule)
        {
            SummonCurrentTurnEnrollmentRuntime.ObserveInitiative(__instance,
                rule == null ? null : rule.Initiator);
        }
    }

    [HarmonyPatch(typeof(UnitCombatJoinController), "Tick", new Type[0])]
    internal static class SummonCurrentTurnCombatJoinPatch
    {
        private static void Postfix()
        {
            try
            {
                SummonCurrentTurnEnrollmentRuntime.JoinMissingSummons();
            }
            catch (Exception exception)
            {
                SummonAcceleratedInvocationRuntime.Clear();
                SummonAcceleratedInvocationRuntime.RecordDiagnostic(
                    "enrollment-native-join=exception:" +
                    exception.GetType().Name);
                ModContext context;
                if (ModContext.TryGet(out context))
                    context.Logger.Warning("summoning",
                        "same-turn-combat-enrollment.failed",
                        exception.GetType().Name + ": " +
                        exception.Message);
            }
        }
    }

    [HarmonyPatch(typeof(TurnController), "Tick", new Type[0])]
    internal static class SummonCurrentTurnTickGatePatch
    {
        private static bool Prefix(TurnController __instance)
        {
            try
            {
                return SummonCurrentTurnEnrollmentRuntime.AllowTurnTick(
                    __instance);
            }
            catch (Exception exception)
            {
                SummonAcceleratedInvocationRuntime.Clear();
                SummonAcceleratedInvocationRuntime.RecordDiagnostic(
                    "enrollment-turn-tick=exception:" +
                    exception.GetType().Name);
                ModContext context;
                if (ModContext.TryGet(out context))
                    context.Logger.Warning("summoning",
                        "same-turn-enrollment.failed",
                        exception.GetType().Name + ": " +
                        exception.Message);
                return true;
            }
        }
    }

    [HarmonyPatch(typeof(CombatController),
        "HandlePartyCombatStateChanged", new Type[] { typeof(bool) })]
    internal static class SummonCurrentTurnCombatCleanupPatch
    {
        private static void Prefix(bool inCombat)
        {
            if (!inCombat) SummonAcceleratedInvocationRuntime.Clear();
        }
    }

    [HarmonyPatch(typeof(CombatController),
        "HandleTurnBasedModeStateChanged", new Type[] { typeof(bool) })]
    internal static class SummonCurrentTurnModeCleanupPatch
    {
        private static void Prefix(bool enabled)
        {
            if (!enabled) SummonAcceleratedInvocationRuntime.Clear();
        }
    }

    [HarmonyPatch(typeof(RuleSummonUnit), "OnTrigger",
        new Type[] { typeof(RulebookEventContext) })]
    internal static class SummonSameTurnActivationPatch
    {
        private static void Postfix(RuleSummonUnit __instance)
        {
            try
            {
                SummonAcceleratedInvocationRuntime.RecordDiagnostic(
                    "repair-postfix=enter");
                SummonSameTurnActivationDecision decision =
                    SummonSameTurnActivationRuntime.TryRepair(__instance);
                SummonAcceleratedInvocationRuntime.RecordDiagnostic(
                    "repair-postfix=" + decision.Disposition);
            }
            catch (Exception exception)
            {
                SummonAcceleratedInvocationRuntime.RecordDiagnostic(
                    "repair-postfix=exception:" +
                    exception.GetType().Name);
                ModContext context;
                if (ModContext.TryGet(out context))
                    context.Logger.Warning("summoning",
                        "same-turn-activation.failed",
                        exception.GetType().Name + ": " +
                        exception.Message);
            }
        }
    }
}
