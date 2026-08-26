using System;
using Harmony12;
using Kingmaker;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs;
using KingmakerGunslinger.Bootstrap;
using TurnBased.Controllers;

namespace KingmakerGunslinger.Summoning
{
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
