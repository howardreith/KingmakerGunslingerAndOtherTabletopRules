using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Stats;
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
using Kingmaker.Utility;

namespace KingmakerGunslinger.Archetypes
{
    public sealed class FocusedAimDamage : RuleInitiatorLogicComponent<RuleCalculateWeaponStats>
    {
        public override void OnEventAboutToTrigger(RuleCalculateWeaponStats evt)
        {
            if (evt == null || evt.Initiator == null || evt.Weapon == null ||
                evt.AttackWithWeapon == null ||
                !FirearmMarkerLookup.ReadFromRuleEvent(evt.AttackWithWeapon).IsExactFirearm) return;
            evt.AddBonusDamage(MysteriousStrangerPolicy.FocusedAimBonus(
                evt.Initiator.Stats.Charisma.Bonus,
                Deeds.DeadShotRuntime.FocusedAimMultiplier(evt.AttackWithWeapon)));
        }
        public override void OnEventDidTrigger(RuleCalculateWeaponStats evt) { }
    }

    [Serializable]
    public sealed class ArmMysteriousStrangerDeed : AbilityCustomLogic,
        IAbilityAvailabilityProvider
    {
        public BlueprintBuff Marker;
        public BlueprintAbilityResource Resource;
        public int Cost;
        public bool SpendOnActivation;

        public bool IsAvailableFor(AbilityData ability)
        {
            return ability != null && ability.Caster != null && Marker != null &&
                Resource != null && ability.Caster.Resources.GetResourceAmount(Resource) >= Cost &&
                !ability.Caster.Buffs.RawFacts.OfType<Buff>().Any(value =>
                    ReferenceEquals(value.Blueprint, Marker));
        }
        public string GetReason() { return "The deed is already armed or lacks its required resource."; }
        public override IEnumerator<AbilityDeliveryTarget> Deliver(
            AbilityExecutionContext context, TargetWrapper target)
        {
            if (context == null || context.Caster == null || !IsAvailableFor(context.Ability))
                throw new InvalidOperationException("Mysterious Stranger deed prerequisites changed.");
            bool spent = SpendOnActivation && Cost > 0;
            if (spent) context.Caster.Descriptor.Resources.Spend(Resource, Cost);
            if (context.Caster.Descriptor.Buffs.AddBuff(Marker, context,
                    TimeSpan.FromSeconds(6d)) == null)
            {
                if (spent) context.Caster.Descriptor.Resources.Restore(Resource, Cost);
                throw new InvalidOperationException("Mysterious Stranger deed marker was rejected.");
            }
            yield return new AbilityDeliveryTarget(target);
        }
        public override void Cleanup(AbilityExecutionContext context) { }
    }

    public sealed class ClippingShotAttackHandler :
        RuleInitiatorLogicComponent<RuleAttackWithWeapon>
    {
        public BlueprintAbilityResource Grit;
        public override void OnEventAboutToTrigger(RuleAttackWithWeapon evt) { }
        public override void OnEventDidTrigger(RuleAttackWithWeapon evt)
        {
            if (evt == null || evt.AttackRoll == null || evt.AttackRoll.IsHit ||
                evt.Initiator == null || evt.Target == null || evt.Weapon == null ||
                Grit == null || Fact == null ||
                !FirearmMarkerLookup.ReadFromRuleEvent(evt.AttackRoll).IsExactFirearm ||
                Deeds.DeadShotRuntime.ShouldBypassDischarge(evt.AttackRoll) ||
                Owner.Resources.GetResourceAmount(Grit) < 1) return;
            Owner.Resources.Spend(Grit, 1);
            Owner.Buffs.RemoveFact(Fact);
            var dice = evt.WeaponStats.WeaponDamageDiceOverride ?? evt.Weapon.Damage;
            int bonus = evt.WeaponStats.BonusDamage;
            var physical = new PhysicalDamage(dice, PhysicalDamageForm.Piercing);
            var damage = new RuleDealDamage(evt.Initiator, evt.Target,
                new DamageBundle(evt.Weapon, evt.WeaponStats.WeaponSize, physical))
            { Modifier = 0.5f, DisablePrecisionDamage = true };
            if (bonus != 0) damage.DamageBundle.Add(new DirectDamage(
                new DiceFormula(0, DiceType.D6), bonus));
            Rulebook.Trigger(damage);
        }
    }
}
