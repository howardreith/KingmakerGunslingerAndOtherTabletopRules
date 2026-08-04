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

        internal static ScatterShotExecutionResult ExecuteForRuntimeTest(
            UnitEntityData caster, UnitEntityData aimedTarget,
            params int[] forcedNaturalRolls)
        {
            return Execute(caster, aimedTarget,
                delegate(RuleAttackWithWeapon attack) { Rulebook.Trigger(attack); },
                forcedNaturalRolls);
        }

        internal static ScatterShotExecutionResult Execute(
            UnitEntityData caster, UnitEntityData aimedTarget,
            Action<RuleAttackWithWeapon> trigger,
            int[] forcedNaturalRolls = null)
        {
            if (caster == null) throw new ArgumentNullException("caster");
            if (aimedTarget == null) throw new ArgumentNullException("aimedTarget");
            if (trigger == null) throw new ArgumentNullException("trigger");
            string reason;
            ExactEquippedFirearmContext firearm;
            if (!IsAvailable(caster, out reason) ||
                !ExactEquippedFirearmResolver.TryResolve(caster.Descriptor,
                    out firearm, out reason))
                throw new InvalidOperationException(
                    "Scatter Shot is unavailable: " + reason);

            UnitEntityData[] nativeTargets = Targets.Resolve(caster, aimedTarget);
            var candidates = new List<ScatterTargetCandidate>(nativeTargets.Length);
            foreach (UnitEntityData unit in nativeTargets)
            {
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
            if (plan.TargetCount == 0)
                throw new InvalidOperationException(
                    "The selected direction contains no exact native cone target.");
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
            bool transitioned = false;
            try
            {
                Transition(firearm, expected, discharge.After);
                expected = discharge.After;
                transitioned = true;

                int threshold = Classes.GunTrainingPolicy.EffectiveMisfireValue(
                    firearm.Definition.MisfireValue, firearm.EffectiveCondition,
                    HasGunTraining(caster, firearm.Definition.Kind));
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
                    // Scatter is one qualified discharge even though it resolves
                    // one attack event per cone target.  Audio therefore belongs
                    // here, after the volley proves that a discharge occurred,
                    // rather than inside the per-target loop.
                    Assets.FirearmAssetRuntime.PlayShot(
                        FirearmKind.Blunderbuss, caster);
                }
                return new ScatterShotExecutionResult(plan, discharge, volley,
                    attacks, condition, before, expected);
            }
            catch
            {
                if (transitioned) Transition(firearm, expected, before);
                throw;
            }
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

        private static bool HasGunTraining(UnitEntityData caster,
            FirearmKind kind)
        {
            GunslingerClassBlueprintSet gunslinger = BlueprintBootstrap.GunslingerClass;
            return caster != null && gunslinger != null &&
                gunslinger.GunTraining != null &&
                Classes.GunTrainingPolicy.IsSupportedKind(kind) &&
                caster.Descriptor.HasFact(gunslinger.GunTraining.ChoiceFor(kind));
        }
    }
}
