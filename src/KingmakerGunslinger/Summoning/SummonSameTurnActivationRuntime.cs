using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Harmony12;
using Kingmaker;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Root;
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
    /// entry survives only until the authoritative command-end callback. This
    /// spans Owlcat's deferred spawn graph and all members of one multi-summon.
    /// It is needed because spending a prepared slot can make a later
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
                ability.Blueprint.IsFullRoundAction &&
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

        internal static void EndRule(RuleCastSpell rule)
        {
            Entry entry;
            if (rule == null || !Rules.TryGetValue(rule, out entry)) return;
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
                Release(entry);
                Record("command-end=released");
            }
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
    /// Corrects the one-round appearance grace that RuleSummonUnit derives
    /// from the immutable blueprint when the exact live invocation has been
    /// accelerated to Standard or Swift. Turn enrollment, initiative, action
    /// resources, AI, and subsequent scheduling remain entirely native.
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
            if (!decision.ShouldRepair) return decision;

            TimeSpan originalEndTime = snapshot.Lifecycle.EndTime;
            bool lifecycleChanged = false;
            try
            {
                if (decision.RemoveLifecycleGrace)
                {
                    snapshot.Lifecycle.EndTime = originalEndTime -
                        TimeSpan.FromSeconds(
                            SummonSameTurnActivationPolicy.NativeGraceSeconds);
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
                            "The canonical summon appearance lock remained " +
                            "after exact-fact removal.");
                }
                return decision;
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

    [HarmonyPatch(typeof(RuleSummonUnit), "OnTrigger",
        new Type[] { typeof(RulebookEventContext) })]
    internal static class SummonSameTurnActivationPatch
    {
        private static void Postfix(RuleSummonUnit __instance)
        {
            try
            {
                SummonSameTurnActivationRuntime.TryRepair(__instance);
            }
            catch (Exception exception)
            {
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
