using System;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Designers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using KingmakerGunslinger.Actions;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;

namespace KingmakerGunslinger.Deeds
{
    internal static class PistolWhipRuntime
    {
        private static readonly PistolWhipService Policy = new PistolWhipService();

        internal static PistolWhipDecision Evaluate(UnitDescriptor caster,
            out ExactEquippedFirearmContext firearm, out string reason)
        {
            firearm = null;
            if (!ExactEquippedFirearmResolver.TryResolve(caster, out firearm, out reason))
                return Policy.Evaluate(new PistolWhipRequest(false, false,
                    Firearms.FirearmCondition.Normal, ReadGrit(caster)));
            PistolWhipDecision decision = Policy.Evaluate(new PistolWhipRequest(
                true, firearm.Definition.Kind == Firearms.FirearmKind.Musket ||
                    firearm.Definition.Kind == Firearms.FirearmKind.Blunderbuss ||
                    firearm.Definition.Kind == Firearms.FirearmKind.Rifle,
                firearm.EffectiveCondition,
                ReadGrit(caster, TrueGritDeed.PistolWhip, 1)));
            reason = decision.Status.ToString();
            return decision;
        }

        internal static PistolWhipResult Execute(AbilityExecutionContext context,
            UnitEntityData target, BlueprintItemWeapon oneHandedSurrogate,
            BlueprintItemWeapon twoHandedSurrogate)
        {
            if (context == null || context.Caster == null || target == null)
                throw new ArgumentNullException("context");
            return Execute(context.Caster.Descriptor, context.Caster, target,
                oneHandedSurrogate, twoHandedSurrogate,
                delegate(RuleAttackWithWeapon rule) {
                    rule.Reason = context; context.TriggerRule(rule); },
                delegate(RuleCombatManeuver rule) {
                    rule.Reason = context; context.TriggerRule(rule); }, false);
        }

        internal static PistolWhipResult ExecuteForRuntimeTest(
            UnitEntityData caster, UnitEntityData target,
            BlueprintItemWeapon oneHandedSurrogate,
            BlueprintItemWeapon twoHandedSurrogate, bool forceHit)
        {
            if (caster == null || target == null) throw new ArgumentNullException("caster");
            return Execute(caster.Descriptor, caster, target, oneHandedSurrogate,
                twoHandedSurrogate,
                delegate(RuleAttackWithWeapon rule) { Rulebook.Trigger(rule); },
                delegate(RuleCombatManeuver rule) { Rulebook.Trigger(rule); },
                forceHit);
        }

        private static PistolWhipResult Execute(UnitDescriptor caster,
            UnitEntityData casterEntity, UnitEntityData target,
            BlueprintItemWeapon oneHandedSurrogate,
            BlueprintItemWeapon twoHandedSurrogate,
            Action<RuleAttackWithWeapon> triggerAttack,
            Action<RuleCombatManeuver> triggerTrip, bool forceHit)
        {
            ExactEquippedFirearmContext firearm;
            string reason;
            PistolWhipDecision decision = Evaluate(caster, out firearm, out reason);
            if (!decision.ShouldAttack)
            {
                PistolWhipRuntimeDiagnostics.RecordRejected();
                return new PistolWhipResult(decision, null, null, 0);
            }
            BlueprintItemWeapon surrogateBlueprint = decision.TwoHanded
                ? twoHandedSurrogate : oneHandedSurrogate;
            if (surrogateBlueprint == null)
                throw new InvalidOperationException("Pistol-Whip surrogate is unavailable.");
            GunslingerClassBlueprintSet gunslinger = BlueprintBootstrap.GunslingerClass;
            TrueGritDecision trueGrit = TrueGritRuntime.Evaluate(caster,
                TrueGritDeed.PistolWhip, decision.GritCost, false);
            bool spent = false;
            try
            {
                caster.Resources.Spend(gunslinger.Grit.Resource,
                    trueGrit.EffectiveCost);
                spent = true;
                int enhancement = GameHelper.GetItemEnhancementBonus(firearm.Weapon);
                var surrogate = new ItemEntityWeapon(surrogateBlueprint);
                var attack = new RuleAttackWithWeapon(casterEntity, target,
                    surrogate, 0);
                attack.WeaponStats.Enhancement = enhancement;
                attack.WeaponStats.EnhancementTotal = enhancement;
                attack.AutoHit = forceHit;
                triggerAttack(attack);
                RuleCombatManeuver trip = null;
                if (attack.AttackRoll != null && attack.AttackRoll.IsHit)
                {
                    trip = new RuleCombatManeuver(casterEntity, target,
                        CombatManeuver.Trip);
                    triggerTrip(trip);
                }
                PistolWhipRuntimeDiagnostics.RecordApplied(
                    attack.AttackRoll != null && attack.AttackRoll.IsHit,
                    trip != null && trip.Success);
                return new PistolWhipResult(decision, attack, trip, enhancement);
            }
            catch
            {
                if (spent)
                    caster.Resources.Restore(gunslinger.Grit.Resource,
                        trueGrit.EffectiveCost);
                PistolWhipRuntimeDiagnostics.RecordFault();
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
