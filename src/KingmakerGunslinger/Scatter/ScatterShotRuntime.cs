using System;
using System.Collections.Generic;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using KingmakerGunslinger.Actions;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Explosions;
using KingmakerGunslinger.Firearms;
using KingmakerGunslinger.Misfires;

namespace KingmakerGunslinger.Scatter
{
    internal static class ScatterShotRuntime
    {
        internal static ScatterShotExecutionResult LastAbilityResult { get; private set; }
        private static readonly NativeScatterConeTargetResolver Targets =
            new NativeScatterConeTargetResolver();
        private static readonly ScatterTargetPlanService Plans =
            new ScatterTargetPlanService();
        private static readonly ScatterDischargeService Discharge =
            new ScatterDischargeService();
        private static readonly ScatterAttackVolleyService Volleys =
            new ScatterAttackVolleyService();
        private static readonly FirearmMisfireService Misfires =
            new FirearmMisfireService();
        private static readonly FirearmMisfireConditionService Conditions =
            new FirearmMisfireConditionService();
        private static readonly FirearmExplosionService Explosions =
            new FirearmExplosionService();

        internal static bool IsAvailable(UnitEntityData caster, out string reason)
        {
            return IsAvailable(caster == null ? null : caster.Descriptor,
                out reason);
        }

        internal static bool IsAvailable(UnitDescriptor caster, out string reason)
        {
            reason = null;
            ExactEquippedFirearmContext firearm;
            if (caster == null || !ExactEquippedFirearmResolver.TryResolve(
                    caster, out firearm, out reason)) return false;
            if (firearm.Definition.Kind != FirearmKind.Blunderbuss ||
                !firearm.Definition.IsScatter)
            { reason = "Equip exactly one production Blunderbuss."; return false; }
            FirearmState state = firearm.Firearm.Repository.State;
            if (firearm.EffectiveCondition == FirearmCondition.Wrecked)
            { reason = "The equipped Blunderbuss is Wrecked."; return false; }
            if (state.LoadedRounds != 1)
            { reason = "The equipped Blunderbuss must contain one loaded shot."; return false; }
            reason = null;
            return true;
        }

        internal static ScatterShotExecutionResult ExecuteFromAbility(
            Kingmaker.UnitLogic.Abilities.AbilityExecutionContext context,
            UnityEngine.Vector3 aimedPoint)
        {
            if (context == null || context.MaybeCaster == null)
                throw new ArgumentNullException("context");
            // A failed activation must never leave a stale success result from an
            // earlier shot available to diagnostics or continuation code.
            LastAbilityResult = null;
            LastAbilityResult = Execute(context.MaybeCaster, aimedPoint,
                delegate(RuleAttackWithWeapon attack) { Rulebook.Trigger(attack); });
            return LastAbilityResult;
        }

        internal static ScatterShotExecutionResult Execute(
            UnitEntityData caster, UnitEntityData aimedTarget,
            Action<RuleAttackWithWeapon> trigger,
            int[] forcedNaturalRolls = null)
        {
            if (caster == null) throw new ArgumentNullException("caster");
            if (aimedTarget == null) throw new ArgumentNullException("aimedTarget");
            return ExecuteResolved(caster, Targets.Resolve(caster, aimedTarget),
                trigger, forcedNaturalRolls);
        }

        internal static ScatterShotExecutionResult Execute(
            UnitEntityData caster, UnityEngine.Vector3 aimedPoint,
            Action<RuleAttackWithWeapon> trigger,
            int[] forcedNaturalRolls = null)
        {
            if (caster == null) throw new ArgumentNullException("caster");
            return ExecuteResolved(caster, Targets.Resolve(caster, aimedPoint),
                trigger, forcedNaturalRolls);
        }

        private static ScatterShotExecutionResult ExecuteResolved(
            UnitEntityData caster, UnitEntityData[] nativeTargets,
            Action<RuleAttackWithWeapon> trigger,
            int[] forcedNaturalRolls)
        {
            if (caster == null) throw new ArgumentNullException("caster");
            if (nativeTargets == null) throw new ArgumentNullException("nativeTargets");
            if (trigger == null) throw new ArgumentNullException("trigger");
            string reason;
            ExactEquippedFirearmContext firearm;
            if (!IsAvailable(caster, out reason) ||
                !ExactEquippedFirearmResolver.TryResolve(caster.Descriptor,
                    out firearm, out reason))
                throw new InvalidOperationException(
                    "Scatter Shot is unavailable: " + reason);

            var candidates = new List<ScatterTargetCandidate>(nativeTargets.Length);
            foreach (UnitEntityData unit in nativeTargets)
            {
                if (unit == null) continue;
                if (string.IsNullOrWhiteSpace(unit.UniqueId))
                    throw new InvalidOperationException(
                        "A native scatter target exposed no stable unit identity.");
                string display = string.IsNullOrWhiteSpace(unit.CharacterName)
                    ? unit.ToString() : unit.CharacterName;
                candidates.Add(new ScatterTargetCandidate(unit, unit.UniqueId,
                    display, unit.DistanceTo(caster.Position),
                    ScatterGeometryDisposition.Inside));
            }
            ScatterTargetPlan plan = Plans.Build(caster, candidates);
            // Firing into an empty direction is still a completed discharge.
            // Native cone abilities permit an empty area, and the tabletop weapon
            // expends its loaded shot even when no creature happens to be caught.
            // The pure ScatterDischargeService already qualifies this exact case.
            if (forcedNaturalRolls != null &&
                forcedNaturalRolls.Length != plan.TargetCount)
                throw new ArgumentException(
                    "Forced scatter roll count must match the exact target plan.",
                    "forcedNaturalRolls");

            FirearmState before = firearm.Firearm.Repository.State;
            FirearmState expected = before;
            ScatterDischargeDecision discharge = Discharge.Evaluate(
                firearm.Definition, before, firearm.EffectiveCondition, plan, true);
            if (discharge.Status != ScatterDischargeStatus.Fired)
                throw new InvalidOperationException(
                    "Eligible Scatter Shot did not produce one discharge.");
            // Committing the chamber transition is the point of discharge. Once
            // it succeeds, later projectile/rule failures must not manufacture the
            // loaded round back into the weapon after targets may already have taken
            // irreversible damage. All reversible target-plan validation occurs above.
            Transition(firearm, expected, discharge.After);
            expected = discharge.After;

            int threshold = global::KingmakerGunslinger.Misfires.EffectiveFirearmMisfirePolicy.Evaluate(
                firearm.Definition.MisfireValue, firearm.EffectiveCondition,
                Classes.FirearmTrainingRuntime.Resolve(caster,
                    firearm.Definition.Kind).ReducedBrokenMisfire,
                firearm.Weapon);
            var attacks = new RuleAttackWithWeapon[plan.TargetCount];
            var observations = new ScatterAttackRollObservation[plan.TargetCount];
            for (int index = 0; index < plan.TargetCount; index++)
            {
                ScatterTargetCandidate target = plan.Targets[index];
                var attack = new RuleAttackWithWeapon(caster,
                    (UnitEntityData)target.Unit, firearm.Weapon,
                    ScatterAttackVolleyDecision.AttackPenalty);
                ScatterVolleyRuntime.Register(attack, target.Unit,
                    target.StableIdentity, threshold,
                    forcedNaturalRolls == null ? (int?)null :
                        forcedNaturalRolls[index]);
                try
                {
                    trigger(attack);
                    observations[index] = ScatterVolleyRuntime.Consume(attack);
                }
                catch
                {
                    ScatterVolleyRuntime.Cancel(attack);
                    throw;
                }
                attacks[index] = attack;
            }

            ScatterAttackVolleyDecision volley = Volleys.Evaluate(
                firearm.Definition, plan, observations);
            FirearmMisfireConditionDecision condition = null;
            if (volley.AllRollsMisfire)
            {
                condition = Conditions.Evaluate(firearm.Definition,
                    Misfires.Evaluate(observations[0].NaturalRoll,
                        threshold, false), expected,
                    firearm.EffectiveCondition);
                if (condition.ChangesCondition)
                {
                    Transition(firearm, expected, condition.After);
                    expected = condition.After;
                    FirearmConditionCombatLog.Publish(
                        firearm.Firearm.ItemDisplayName,
                        condition.Before.Condition,
                        condition.After.Condition,
                        "scatter misfire");
                }
                FirearmExplosionDecision explosion =
                    Explosions.Evaluate(condition);
                ScatterExplosionDamageDecision scatterExplosion =
                    new ScatterExplosionDamageService().Evaluate(
                        firearm.Definition, explosion, volley);
                if (scatterExplosion.ShouldApply)
                    FirearmExplosionRuntime.Apply(
                        attacks[attacks.Length - 1].AttackRoll,
                        firearm.Weapon, caster,
                        firearm.Firearm.Repository.RepositoryIdentity,
                        firearm.Definition.MisfireBurstRadiusFeet,
                        firearm.Firearm.ItemDisplayName,
                        scatterExplosion.BaseDamageMultiplier);
            }
            else
            {
                Audio.FirearmSoundRuntime.TryPostCommittedDischarge(
                    FirearmKind.Blunderbuss, caster, "scatter-shot");
            }
            return new ScatterShotExecutionResult(plan, discharge, volley,
                attacks, condition, before, expected);
        }

        private static void Transition(ExactEquippedFirearmContext firearm,
            FirearmState expected, FirearmState replacement)
        {
            FirearmRuntimeState.Service.Transition(firearm.Weapon, current =>
            {
                if (current != expected) throw new InvalidOperationException(
                    "Blunderbuss state changed during Scatter Shot delivery.");
                return replacement;
            });
        }

    }
}
