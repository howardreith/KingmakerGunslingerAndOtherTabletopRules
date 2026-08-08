using System;
using System.Runtime.CompilerServices;
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
    internal static class TargetingTorsoRuntime
    {
        private static readonly object Gate = new object();
        private static readonly ConditionalWeakTable<RuleAttackWithWeapon, object>
            Markers = new ConditionalWeakTable<RuleAttackWithWeapon, object>();
        private static readonly TargetingHeadService Policy = new TargetingHeadService();

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
                true, state.Condition, state.LoadedRounds,
                ReadGrit(caster, TrueGritDeed.TargetingTorso, 1),
                validTarget));
            reason = decision.Status.ToString();
            return decision;
        }

        internal static TargetingTorsoResult Execute(AbilityExecutionContext context,
            UnitEntityData target)
        {
            if (context == null || context.Caster == null || target == null)
                throw new ArgumentNullException("context");
            return Execute(context.Caster.Descriptor, context.Caster, target,
                delegate(RuleAttackWithWeapon rule) { rule.Reason = context;
                    context.TriggerRule(rule); },
                delegate(RuleDealDamage rule) { context.TriggerRule(rule); });
        }

        internal static TargetingTorsoResult ExecuteForRuntimeTest(
            UnitEntityData caster, UnitEntityData target)
        {
            if (caster == null || target == null)
                throw new ArgumentNullException("caster");
            return Execute(caster.Descriptor, caster, target,
                delegate(RuleAttackWithWeapon rule) { Rulebook.Trigger(rule); },
                delegate(RuleDealDamage rule) { Rulebook.Trigger(rule); });
        }

        private static TargetingTorsoResult Execute(UnitDescriptor caster,
            UnitEntityData casterEntity, UnitEntityData target,
            Action<RuleAttackWithWeapon> triggerAttack,
            Action<RuleDealDamage> triggerDamage)
        {
            ExactEquippedFirearmContext firearm; string reason;
            TargetingHeadDecision decision = Evaluate(caster, true, out firearm,
                out reason);
            if (!decision.ShouldAttack)
                return new TargetingTorsoResult(decision, null, null);
            GunslingerClassBlueprintSet gunslinger = BlueprintBootstrap.GunslingerClass;
            TrueGritDecision trueGrit = TrueGritRuntime.Evaluate(caster,
                TrueGritDeed.TargetingTorso, decision.GritCost, false);
            bool spent = false, attackStarted = false;
            try
            {
                caster.Resources.Spend(gunslinger.Grit.Resource,
                    trueGrit.EffectiveCost);
                spent = true;
                var attack = new RuleAttackWithWeapon(casterEntity, target,
                    firearm.Weapon, 0);
                Register(attack);
                attackStarted = true;
                try { triggerAttack(attack); }
                finally { Cancel(attack); }
                RuleDealDamage damage = null;
                if (attack.AttackRoll != null && attack.AttackRoll.IsHit)
                {
                    damage = attack.CreateRuleDealDamage(false);
                    triggerDamage(damage);
                }
                return new TargetingTorsoResult(decision, attack, damage);
            }
            catch
            {
                if (spent && !attackStarted)
                    caster.Resources.Restore(gunslinger.Grit.Resource,
                        trueGrit.EffectiveCost);
                throw;
            }
        }

        internal static void Register(RuleAttackWithWeapon attack)
        {
            if (attack == null) throw new ArgumentNullException("attack");
            lock (Gate) { Markers.Remove(attack); Markers.Add(attack, new object()); }
        }

        internal static void Cancel(RuleAttackWithWeapon attack)
        { if (attack != null) lock (Gate) { Markers.Remove(attack); } }

        internal static bool IsMarked(RuleAttackWithWeapon attack)
        {
            if (attack == null) return false;
            lock (Gate) { object marker; return Markers.TryGetValue(attack, out marker); }
        }

        internal static void ConfigureAttackRoll(RuleAttackRoll attackRoll)
        {
            if (attackRoll == null || !IsMarked(attackRoll.RuleAttackWithWeapon) ||
                attackRoll.ImmuneToSneakAttack || attackRoll.WeaponStats == null)
                return;
            int edge = attackRoll.WeaponStats.CriticalEdge;
            if (edge > 19)
                attackRoll.WeaponStats.CriticalEdgeBonus += edge - 19;
            // Kingmaker caches IsCriticalRoll before this RuleAttackRoll prefix
            // observes the deed-local WeaponStats mutation. Authorize only the
            // newly added natural-19 threat; confirmation remains native.
            if (attackRoll.Roll.Value == 19)
                attackRoll.AutoCriticalThreat = true;
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
