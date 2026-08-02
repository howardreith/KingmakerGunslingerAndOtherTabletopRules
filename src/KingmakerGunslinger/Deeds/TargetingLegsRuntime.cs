using System;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using KingmakerGunslinger.Actions;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Firearms;

namespace KingmakerGunslinger.Deeds
{
    internal static class TargetingLegsRuntime
    {
        private static readonly TargetingHeadService Policy = new TargetingHeadService();
        private static readonly TargetingLegsRiderService Riders =
            new TargetingLegsRiderService();

        internal static TargetingHeadDecision Evaluate(UnitDescriptor caster,
            bool validTarget, out ExactEquippedFirearmContext firearm,
            out string reason)
        {
            firearm = null;
            if (!ExactEquippedFirearmResolver.TryResolve(caster, out firearm,
                    out reason))
                return Policy.Evaluate(new TargetingHeadRequest(false,
                    FirearmCondition.Normal, 0, ReadGrit(caster), validTarget));
            FirearmState state = firearm.Firearm.Repository.State;
            TargetingHeadDecision decision = Policy.Evaluate(new TargetingHeadRequest(
                true, state.Condition, state.LoadedRounds, ReadGrit(caster),
                validTarget));
            reason = decision.Status.ToString();
            return decision;
        }

        internal static TargetingLegsResult Execute(AbilityExecutionContext context,
            UnitEntityData target)
        {
            if (context == null || context.Caster == null || target == null)
                throw new ArgumentNullException("context");
            return Execute(context.Caster.Descriptor, context.Caster, target,
                delegate(RuleAttackWithWeapon rule) { rule.Reason = context;
                    context.TriggerRule(rule); },
                delegate(RuleDealDamage rule) { context.TriggerRule(rule); },
                delegate(RuleCombatManeuver rule) { rule.Reason = context;
                    context.TriggerRule(rule); });
        }

        internal static TargetingLegsResult ExecuteForRuntimeTest(
            UnitEntityData caster, UnitEntityData target)
        {
            if (caster == null || target == null)
                throw new ArgumentNullException("caster");
            return Execute(caster.Descriptor, caster, target,
                delegate(RuleAttackWithWeapon rule) { Rulebook.Trigger(rule); },
                delegate(RuleDealDamage rule) { Rulebook.Trigger(rule); },
                delegate(RuleCombatManeuver rule) { Rulebook.Trigger(rule); });
        }

        private static TargetingLegsResult Execute(UnitDescriptor caster,
            UnitEntityData casterEntity, UnitEntityData target,
            Action<RuleAttackWithWeapon> triggerAttack,
            Action<RuleDealDamage> triggerDamage,
            Action<RuleCombatManeuver> triggerTrip)
        {
            ExactEquippedFirearmContext firearm; string reason;
            TargetingHeadDecision decision = Evaluate(caster, true, out firearm,
                out reason);
            if (!decision.ShouldAttack)
                return new TargetingLegsResult(decision, null, null, null, null);
            GunslingerClassBlueprintSet gunslinger = BlueprintBootstrap.GunslingerClass;
            bool spent = false, attackStarted = false;
            try
            {
                caster.Resources.Spend(gunslinger.Grit.Resource, decision.GritCost);
                spent = true;
                var attack = new RuleAttackWithWeapon(casterEntity, target,
                    firearm.Weapon, 0);
                attackStarted = true;
                triggerAttack(attack);
                bool hit = attack.AttackRoll != null && attack.AttackRoll.IsHit;
                RuleDealDamage damage = null;
                if (hit)
                {
                    damage = attack.CreateRuleDealDamage(false);
                    triggerDamage(damage);
                }
                bool sneakImmune = attack.AttackRoll != null &&
                    attack.AttackRoll.ImmuneToSneakAttack;
                bool tripImmune = target.Descriptor.State.HasCondition(
                    UnitCondition.ImmuneToCombatManeuvers);
                TargetingLegsRiderDecision rider = Riders.Evaluate(hit,
                    sneakImmune, tripImmune);
                RuleCombatManeuver trip = null;
                if (rider.ShouldTrip)
                {
                    trip = new RuleCombatManeuver(casterEntity, target,
                        CombatManeuver.Trip) {
                        ReplaceAttackBonus = 1000,
                        IgnoreConcealment = true
                    };
                    triggerTrip(trip);
                }
                return new TargetingLegsResult(decision, attack, damage, rider,
                    trip);
            }
            catch
            {
                if (spent && !attackStarted)
                    caster.Resources.Restore(gunslinger.Grit.Resource,
                        decision.GritCost);
                throw;
            }
        }

        private static int ReadGrit(UnitDescriptor caster)
        {
            GunslingerClassBlueprintSet gunslinger = BlueprintBootstrap.GunslingerClass;
            return caster == null || gunslinger == null ? 0 :
                caster.Resources.GetResourceAmount(gunslinger.Grit.Resource);
        }
    }
}
