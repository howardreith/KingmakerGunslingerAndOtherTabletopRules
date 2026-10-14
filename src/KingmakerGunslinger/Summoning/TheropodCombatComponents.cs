using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility;
using KingmakerGunslinger.Firing;

namespace KingmakerGunslinger.Summoning
{
    /// <summary>
    /// The Tyrannosaurus's always-applicable Powerful Bite. Kingmaker's lone
    /// primary natural weapon already contributes one-and-a-half times a
    /// positive Strength modifier, but only the plain modifier for zero or a
    /// penalty. This exact owner/weapon carrier adds only the signed
    /// difference needed to reach twice the current modifier.
    /// </summary>
    [Serializable]
    public sealed class TyrannosaurusPowerfulBite :
        RuleInitiatorLogicComponent<RuleCalculateWeaponStats>
    {
        public BlueprintUnit OwningBlueprint;
        public BlueprintItemWeapon Bite;

        public override void OnEventAboutToTrigger(
            RuleCalculateWeaponStats evt)
        {
            UnitEntityData owner = Owner == null ? null : Owner.Unit;
            if (evt == null || owner == null || OwningBlueprint == null ||
                Bite == null || !ReferenceEquals(evt.Initiator, owner) ||
                !ReferenceEquals(owner.Blueprint, OwningBlueprint) ||
                evt.Weapon == null ||
                !ReferenceEquals(evt.Weapon.Blueprint, Bite) ||
                !evt.Weapon.Blueprint.IsNatural || evt.Weapon.IsSecondary ||
                evt.SecondaryWeapon || !evt.DamageBonusStat.HasValue ||
                evt.DamageBonusStat.Value != StatType.Strength ||
                owner.Body == null || owner.Body.PrimaryHand == null ||
                !ReferenceEquals(owner.Body.PrimaryHand.MaybeWeapon,
                    evt.Weapon)) return;

            int correction = TheropodRulesPolicy.PowerfulBiteCorrection(
                owner.Descriptor.Stats.Strength.Bonus);
            if (correction != 0) evt.AddBonusDamage(correction);
        }

        public override void OnEventDidTrigger(
            RuleCalculateWeaponStats evt) { }
    }

    /// <summary>
    /// Bleeding Critical's missing project predicate. The standard native
    /// 2d6 bleed buff continues to own its damage cadence, stacking, healing
    /// removal and combat cleanup; this component does only the exact
    /// Tyrannosaurus/bite/confirmed-critical decision once per native rule
    /// event.
    /// </summary>
    [Serializable]
    public sealed class TyrannosaurusBleedingCritical :
        RuleInitiatorLogicComponent<RuleAttackWithWeapon>
    {
        private static readonly ReferenceEventGate Events =
            new ReferenceEventGate();

        public BlueprintUnit OwningBlueprint;
        public BlueprintItemWeapon Bite;
        public BlueprintBuff Bleed;

        public override void OnEventAboutToTrigger(
            RuleAttackWithWeapon evt) { }

        public override void OnEventDidTrigger(RuleAttackWithWeapon evt)
        {
            UnitEntityData owner = Owner == null ? null : Owner.Unit;
            UnitEntityData target = evt == null ? null : evt.Target;
            RuleAttackRoll attack = evt == null ? null : evt.AttackRoll;
            bool exactOwner = owner != null && OwningBlueprint != null &&
                evt != null && ReferenceEquals(evt.Initiator, owner) &&
                ReferenceEquals(owner.Blueprint, OwningBlueprint);
            bool exactBite = evt != null && evt.Weapon != null &&
                Bite != null &&
                ReferenceEquals(evt.Weapon.Blueprint, Bite);
            bool available = target != null && target.Descriptor != null &&
                !target.Destroyed && !target.Descriptor.State.IsDead;
            if (!exactOwner || !exactBite || attack == null || Bleed == null ||
                !TheropodRulesPolicy.ShouldApplyBleedingCritical(
                    exactOwner, exactBite, attack.IsHit,
                    attack.IsCriticalConfirmed, attack.ImmuneToCriticalHit,
                    available, true)) return;

            bool first = Events.TryMark(attack);
            if (!TheropodRulesPolicy.ShouldApplyBleedingCritical(
                    exactOwner, exactBite, attack.IsHit,
                    attack.IsCriticalConfirmed, attack.ImmuneToCriticalHit,
                    available, first)) return;
            var context = new MechanicsContext(owner, target.Descriptor,
                Bleed, Fact == null ? null : Fact.MaybeContext,
                new TargetWrapper(target));
            target.Descriptor.Buffs.AddBuff(Bleed, context, null);
        }
    }
}
