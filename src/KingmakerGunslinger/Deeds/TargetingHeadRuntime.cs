using System;
using System.Linq;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using KingmakerGunslinger.Actions;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Firearms;

namespace KingmakerGunslinger.Deeds
{
    internal static class TargetingHeadRuntime
    {
        private static readonly TargetingHeadService Policy = new TargetingHeadService();

        internal static TargetingHeadDecision Evaluate(UnitDescriptor caster,
            bool validTarget, out ExactEquippedFirearmContext firearm,
            out string reason)
        {
            firearm = null;
            if (!ExactEquippedFirearmResolver.TryResolve(caster, out firearm, out reason))
                return Policy.Evaluate(new TargetingHeadRequest(false,
                    FirearmCondition.Normal, 0, ReadGrit(caster), validTarget));
            FirearmState state = firearm.Firearm.Repository.State;
            TargetingHeadDecision decision = Policy.Evaluate(new TargetingHeadRequest(
                true, state.Condition, state.LoadedRounds,
                ReadGrit(caster, TrueGritDeed.TargetingHead, 1),
                validTarget));
            reason = decision.Status.ToString();
            return decision;
        }

        internal static TargetingHeadResult Execute(AbilityExecutionContext context,
            UnitEntityData target, BlueprintBuff confusionBuff)
        {
            if (context == null || context.Caster == null || target == null)
                throw new ArgumentNullException("context");
            return Execute(context.Caster.Descriptor, context.Caster, target,
                confusionBuff, context, delegate(RuleAttackWithWeapon rule)
                { rule.Reason = context; context.TriggerRule(rule); }, false);
        }

        internal static TargetingHeadResult ExecuteForRuntimeTest(
            UnitEntityData caster, UnitEntityData target,
            BlueprintBuff confusionBuff, MechanicsContext context,
            bool forceHit)
        {
            if (caster == null || target == null) throw new ArgumentNullException("caster");
            return Execute(caster.Descriptor, caster, target, confusionBuff,
                context, delegate(RuleAttackWithWeapon rule)
                {
                    Rulebook.Trigger(rule);
                    if (rule.MeleeDamage == null && rule.AttackRoll != null &&
                        rule.AttackRoll.IsHit)
                        Rulebook.Trigger(rule.CreateRuleDealDamage(false));
                }, forceHit);
        }

        private static TargetingHeadResult Execute(UnitDescriptor caster,
            UnitEntityData casterEntity, UnitEntityData target,
            BlueprintBuff confusionBuff, MechanicsContext context,
            Action<RuleAttackWithWeapon> triggerAttack, bool forceHit)
        {
            if (confusionBuff == null) throw new ArgumentNullException("confusionBuff");
            ExactEquippedFirearmContext firearm;
            string reason;
            TargetingHeadDecision decision = Evaluate(caster, true, out firearm,
                out reason);
            if (!decision.ShouldAttack)
                return new TargetingHeadResult(decision, null, null, null);
            GunslingerClassBlueprintSet gunslinger = BlueprintBootstrap.GunslingerClass;
            TrueGritDecision trueGrit = TrueGritRuntime.Evaluate(caster,
                TrueGritDeed.TargetingHead, decision.GritCost, false);
            bool spent = false, attackStarted = false;
            try
            {
                caster.Resources.Spend(gunslinger.Grit.Resource,
                    trueGrit.EffectiveCost);
                spent = true;
                var attack = new RuleAttackWithWeapon(casterEntity, target,
                    firearm.Weapon, 0) { AutoHit = forceHit };
                attackStarted = true;
                triggerAttack(attack);
                bool hit = attack.AttackRoll != null && attack.AttackRoll.IsHit;
                bool immune = attack.AttackRoll != null &&
                    attack.AttackRoll.ImmuneToSneakAttack;
                TargetingHeadRiderDecision rider = Policy.EvaluateRider(hit, immune);
                Buff buff = rider.ShouldConfuse ? target.Descriptor.Buffs.AddBuff(
                    confusionBuff, context, TimeSpan.FromSeconds(6d)) : null;
                if (rider.ShouldConfuse && buff == null)
                    buff = target.Descriptor.Buffs.RawFacts.OfType<Buff>()
                        .SingleOrDefault(value => ReferenceEquals(
                            value.Blueprint, confusionBuff));
                if (rider.ShouldConfuse && buff == null)
                    throw new InvalidOperationException(
                        "Targeting Head Confusion buff was not created.");
                return new TargetingHeadResult(decision, attack, rider, buff);
            }
            catch
            {
                if (spent && !attackStarted)
                    caster.Resources.Restore(gunslinger.Grit.Resource,
                        trueGrit.EffectiveCost);
                throw;
            }
        }

        private static int ReadGrit(UnitDescriptor caster)
        {
            GunslingerClassBlueprintSet gunslinger = BlueprintBootstrap.GunslingerClass;
            return caster == null || gunslinger == null ? 0 :
                caster.Resources.GetResourceAmount(gunslinger.Grit.Resource);
        }

        private static int ReadGrit(UnitDescriptor caster, TrueGritDeed deed,
            int ordinaryCost)
        {
            int current = ReadGrit(caster);
            return TrueGritRuntime.Evaluate(caster, deed, ordinaryCost, false)
                .Available ? Math.Max(ordinaryCost, current) : current;
        }
    }
}
