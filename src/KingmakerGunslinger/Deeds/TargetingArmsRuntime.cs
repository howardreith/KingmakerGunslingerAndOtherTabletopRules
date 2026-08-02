using System;
using System.Linq;
using Kingmaker.Blueprints;
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
    internal static class TargetingArmsRuntime
    {
        private const string NativeMainHandDisarmBuff = "DisarmMainHandBuff";
        private static readonly TargetingHeadService Policy =
            new TargetingHeadService();
        private static readonly TargetingArmsRiderService Riders =
            new TargetingArmsRiderService();

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
                ReadGrit(caster, TrueGritDeed.TargetingArms, 1), validTarget));
            reason = decision.Status.ToString();
            return decision;
        }

        internal static TargetingArmsResult Execute(AbilityExecutionContext context,
            UnitEntityData target)
        {
            if (context == null || context.Caster == null || target == null)
                throw new ArgumentNullException("context");
            return Execute(context.Caster.Descriptor, context.Caster, target,
                context, delegate(RuleAttackWithWeapon rule)
                { rule.Reason = context; context.TriggerRule(rule); }, false);
        }

        internal static TargetingArmsResult ExecuteForRuntimeTest(
            UnitEntityData caster, UnitEntityData target,
            MechanicsContext context, bool forceHit)
        {
            if (caster == null || target == null)
                throw new ArgumentNullException("caster");
            return Execute(caster.Descriptor, caster, target, context,
                delegate(RuleAttackWithWeapon rule) { Rulebook.Trigger(rule); },
                forceHit);
        }

        private static TargetingArmsResult Execute(UnitDescriptor caster,
            UnitEntityData casterEntity, UnitEntityData target,
            MechanicsContext context, Action<RuleAttackWithWeapon> triggerAttack,
            bool forceHit)
        {
            ExactEquippedFirearmContext firearm; string reason;
            TargetingHeadDecision decision = Evaluate(caster,
                true, out firearm, out reason);
            if (!decision.ShouldAttack)
                return new TargetingArmsResult(decision, null, null, null);
            GunslingerClassBlueprintSet gunslinger = BlueprintBootstrap.GunslingerClass;
            TrueGritDecision trueGrit = TrueGritRuntime.Evaluate(
                caster, TrueGritDeed.TargetingArms,
                decision.GritCost, false);
            caster.Resources.Spend(gunslinger.Grit.Resource,
                trueGrit.EffectiveCost);
            var attack = new RuleAttackWithWeapon(casterEntity, target,
                firearm.Weapon, 0) { AutoHit = forceHit };
            triggerAttack(attack);
            bool hit = attack.AttackRoll != null && attack.AttackRoll.IsHit;
            bool immune = attack.AttackRoll != null &&
                attack.AttackRoll.ImmuneToSneakAttack;
            TargetingArmsRiderDecision rider = Riders.Evaluate(hit, immune);
            if (!rider.ShouldDisableMainHand)
                return new TargetingArmsResult(decision, attack, rider, null);
            BlueprintBuff buff = BlueprintBootstrap.Library.GetAllBlueprints()
                .OfType<BlueprintBuff>().Single(value => string.Equals(value.name,
                    NativeMainHandDisarmBuff, StringComparison.Ordinal));
            Buff applied = target.Descriptor.Buffs.AddBuff(buff, context,
                TimeSpan.FromSeconds(6));
            if (applied == null)
                applied = target.Descriptor.Buffs.RawFacts.OfType<Buff>()
                    .SingleOrDefault(value => ReferenceEquals(value.Blueprint, buff));
            if (applied == null)
                throw new InvalidOperationException(
                    "Targeting Arms native disarm buff was not created.");
            return new TargetingArmsResult(decision, attack, rider, applied);
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
