using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Controllers.Units;
using Kingmaker.Enums.Damage;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using KingmakerGunslinger.Diagnostics;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Firearms;
using KingmakerGunslinger.Misfires;
using KingmakerGunslinger.Rules;
using KingmakerGunslinger.Scatter;
using Kingmaker.Utility;

namespace KingmakerGunslinger.Archetypes
{
    [Serializable]
    public sealed class UpCloseAndDeadlyAbilityLogic : AbilityCustomLogic,
        IAbilityAvailabilityProvider
    {
        public BlueprintBuff ArmedMarker;
        public BlueprintAbilityResource Grit;

        public bool IsAvailableFor(AbilityData ability)
        {
            return ability != null && ability.Caster != null &&
                ArmedMarker != null && Grit != null &&
                ability.Caster.Resources.GetResourceAmount(Grit) >=
                    UpCloseAndDeadlyPolicy.FixedGritCost &&
                !ability.Caster.Buffs.RawFacts.OfType<Buff>().Any(value =>
                    ReferenceEquals(value.Blueprint, ArmedMarker));
        }

        public string GetReason()
        { return "Up Close and Deadly is already armed or requires 1 grit."; }

        public override IEnumerator<AbilityDeliveryTarget> Deliver(
            AbilityExecutionContext context, TargetWrapper target)
        {
            if (context == null || context.Caster == null ||
                !IsAvailableFor(context.Ability))
                throw new InvalidOperationException(
                    "Up Close and Deadly prerequisites changed before execution.");
            context.Caster.Descriptor.Buffs.AddBuff(ArmedMarker, context,
                TimeSpan.FromSeconds(6d));
            if (!context.Caster.Descriptor.Buffs.RawFacts.OfType<Buff>().Any(
                    value => ReferenceEquals(value.Blueprint, ArmedMarker)))
                throw new InvalidOperationException(
                    "Up Close and Deadly armed marker was rejected.");
            yield return new AbilityDeliveryTarget(target);
        }

        public override void Cleanup(AbilityExecutionContext context) { }
    }

    public sealed class UpCloseAndDeadlyAttackHandler :
        RuleInitiatorLogicComponent<RuleAttackWithWeapon>
    {
        private static readonly ConditionalWeakTable<RuleAttackWithWeapon, object>
            ConsumedAttacks = new ConditionalWeakTable<RuleAttackWithWeapon, object>();
        private static readonly object ConsumedMarker = new object();

        public BlueprintAbilityResource Grit;
        public BlueprintCharacterClass GunslingerClass;

        public override void OnEventAboutToTrigger(RuleAttackWithWeapon evt) { }

        public override void OnEventDidTrigger(RuleAttackWithWeapon evt)
        {
            if (evt == null || evt.AttackRoll == null || evt.Initiator == null ||
                evt.Target == null || Owner == null || Fact == null ||
                Grit == null || GunslingerClass == null) return;
            try
            {
                FirearmMarkerSnapshot marker =
                    FirearmMarkerLookup.ReadFromRuleEvent(evt.AttackRoll);
                var decision = UpCloseAndDeadlyPolicy.Evaluate(
                    marker.IsExactFirearm, marker.MarkerCount,
                    marker.Definition == null ? FirearmKind.Unknown :
                        marker.Definition.Kind,
                    ScatterVolleyRuntime.ShouldBypassOrdinaryDischarge(
                        evt.AttackRoll),
                    FirearmMisfireRuntime.WasCompletedNonMisfire(evt.AttackRoll),
                    evt.AttackRoll.IsHit, evt.AttackRoll.ImmuneToSneakAttack,
                    Owner.Progression.GetClassLevel(GunslingerClass),
                    Owner.Resources.GetResourceAmount(Grit));
                if (!decision.ConsumeMarker) return;
                lock (ConsumedAttacks)
                {
                    object ignored;
                    if (ConsumedAttacks.TryGetValue(evt, out ignored)) return;
                    ConsumedAttacks.Add(evt, ConsumedMarker);
                }
                Owner.Buffs.RemoveFact(Fact);
                if (!decision.ApplyDamage) return;

                Owner.Resources.Spend(Grit,
                    UpCloseAndDeadlyPolicy.FixedGritCost);
                try
                {
                    BaseDamage packet;
                    if (decision.Modifier < 1f)
                    {
                        int fullRoll = 0;
                        for (int index = 0; index < decision.Dice; index++)
                            fullRoll += RulebookEvent.Dice.D6;
                        packet = new DirectDamage(
                            new DiceFormula(0, DiceType.D6), fullRoll / 2);
                    }
                    else
                    {
                        packet = new PhysicalDamage(
                            new DiceFormula(decision.Dice, DiceType.D6),
                            PhysicalDamageForm.Piercing);
                    }
                    var damage = new RuleDealDamage(evt.Initiator, evt.Target,
                        new DamageBundle(evt.Weapon,
                            evt.WeaponStats.WeaponSize, packet))
                    {
                        Modifier = 1f,
                        // This isolated packet is itself the deed's precision
                        // damage; suppress native sneak/precision discovery so
                        // the independent event cannot add another source.
                        DisablePrecisionDamage = true
                    };
                    Rulebook.Trigger(damage);
                }
                catch
                {
                    Owner.Resources.Restore(Grit,
                        UpCloseAndDeadlyPolicy.FixedGritCost);
                    throw;
                }
            }
            catch (Exception exception)
            {
                ModContext context;
                if (ModContext.TryGet(out context))
                    context.Logger.Failure("up-close-and-deadly",
                        "delivery.failed",
                        "Up Close and Deadly failed closed after its qualifying attack.",
                        exception);
            }
        }

    }
}
