using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using KingmakerGunslinger.Firing;

namespace KingmakerGunslinger.Summoning
{
    /// <summary>
    /// The adult Stegosaurus's printed tail rider. A resolved exact tail hit
    /// starts one ordinary native Trip maneuver; the engine rolls against CMD
    /// and applies prone on success. This carrier neither deals another hit
    /// nor turns failure into a trip against the attacker.
    /// </summary>
    [Serializable]
    public sealed class StegosaurusTailTrip :
        RuleInitiatorLogicComponent<RuleAttackWithWeapon>
    {
        private static readonly ReferenceEventGate Events =
            new ReferenceEventGate();

        public BlueprintUnit OwningBlueprint;
        public BlueprintItemWeapon Tail;

        public override void OnEventAboutToTrigger(RuleAttackWithWeapon evt) { }

        public override void OnEventDidTrigger(RuleAttackWithWeapon evt)
        {
            UnitEntityData owner = Owner == null ? null : Owner.Unit;
            UnitEntityData target = evt == null ? null : evt.Target;
            RuleAttackRoll attack = evt == null ? null : evt.AttackRoll;
            bool exactOwner = owner != null && OwningBlueprint != null &&
                ReferenceEquals(owner.Blueprint, OwningBlueprint) &&
                ReferenceEquals(evt.Initiator, owner);
            bool exactTail = evt != null && evt.Weapon != null && Tail != null &&
                ReferenceEquals(evt.Weapon.Blueprint, Tail);
            bool targetAvailable = target != null && !target.Destroyed &&
                target.Descriptor != null && target.Descriptor.State != null &&
                !target.Descriptor.State.IsDead && owner != null &&
                !owner.Destroyed && owner.Descriptor != null &&
                owner.Descriptor.State != null &&
                !owner.Descriptor.State.IsDead;
            bool first = attack != null && Events.TryMark(attack);
            bool alreadyProne = targetAvailable &&
                target.Descriptor.State.HasCondition(UnitCondition.Prone);
            bool immune = targetAvailable &&
                target.Descriptor.State.HasCondition(
                    UnitCondition.ImmuneToCombatManeuvers);
            int ownerSize = targetAvailable ?
                (int)owner.Descriptor.State.Size : -1;
            int targetSize = targetAvailable ?
                (int)target.Descriptor.State.Size : -1;

            if (!ArmoredDinosaurRulesPolicy.CanAttemptTailTrip(exactOwner,
                    exactTail, attack != null && attack.IsHit,
                    targetAvailable, first, alreadyProne, immune,
                    ownerSize, targetSize)) return;

            var trip = new RuleCombatManeuver(owner, target,
                CombatManeuver.Trip);
            if (Fact != null && Fact.MaybeContext != null)
            {
                trip.Reason = Fact.MaybeContext;
                Fact.MaybeContext.TriggerRule(trip);
            }
            else Rulebook.Trigger(trip);
        }
    }

    /// <summary>
    /// Native Power Attack applies the BAB +9/+10 one-handed +6 damage. Each
    /// Sprint 23 dinosaur has exactly one primary natural weapon, so its own
    /// hidden chassis adds only the missing +3 needed for the native 1.5x
    /// rule. Released creatures and other limbs are excluded by identity.
    /// </summary>
    [Serializable]
    public sealed class ArmoredDinosaurPowerAttackCorrection :
        RuleInitiatorLogicComponent<RuleCalculateWeaponStats>
    {
        public BlueprintUnit OwningBlueprint;
        public BlueprintItemWeapon Weapon;
        public BlueprintBuff PowerAttackBuff;
        public int AdditionalDamage;

        public override void OnEventAboutToTrigger(
            RuleCalculateWeaponStats evt)
        {
            UnitEntityData owner = Owner == null ? null : Owner.Unit;
            if (evt == null || owner == null || OwningBlueprint == null ||
                Weapon == null || PowerAttackBuff == null ||
                !ReferenceEquals(owner.Blueprint, OwningBlueprint) ||
                !ReferenceEquals(evt.Initiator, owner) || evt.Weapon == null ||
                !ReferenceEquals(evt.Weapon.Blueprint, Weapon) ||
                evt.SecondaryWeapon || evt.Weapon.IsSecondary ||
                owner.Descriptor.Buffs.GetBuff(PowerAttackBuff) == null ||
                owner.Body == null || owner.Body.PrimaryHand == null ||
                !ReferenceEquals(owner.Body.PrimaryHand.MaybeWeapon,
                    evt.Weapon) ||
                AdditionalDamage !=
                    ArmoredDinosaurRulesPolicy.PowerAttackAdditionalDamage)
                return;
            evt.AddBonusDamage(AdditionalDamage);
        }

        public override void OnEventDidTrigger(
            RuleCalculateWeaponStats evt) { }
    }
}
