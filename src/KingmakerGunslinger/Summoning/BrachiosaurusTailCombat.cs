using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Buffs.Blueprints;

namespace KingmakerGunslinger.Summoning
{
    // Same bounded native Power Attack completion as the qualified lone-tail
    // dinosaurs: native BAB +13 supplies -4/+8, and this exact adult's single
    // primary natural tail is owed +12. Native penalty/base damage stay native.
    [Serializable]
    public sealed class BrachiosaurusPowerAttackCorrection :
        RuleInitiatorLogicComponent<RuleCalculateWeaponStats>
    {
        public BlueprintUnit OwningBlueprint;
        public BlueprintItemWeapon Tail;
        public BlueprintBuff PowerAttackBuff;

        public override void OnEventAboutToTrigger(RuleCalculateWeaponStats evt)
        {
            UnitEntityData owner = Owner == null ? null : Owner.Unit;
            if (evt == null || owner == null || OwningBlueprint == null ||
                Tail == null || PowerAttackBuff == null ||
                OwningBlueprint.name != GiantBodyVisualPolicy.BrachiosaurusBlueprintName ||
                !ReferenceEquals(owner.Blueprint, OwningBlueprint) ||
                !ReferenceEquals(evt.Initiator, owner) || evt.Weapon == null ||
                !ReferenceEquals(evt.Weapon.Blueprint, Tail) ||
                evt.SecondaryWeapon || evt.Weapon.IsSecondary ||
                owner.Descriptor.Buffs.GetBuff(PowerAttackBuff) == null ||
                owner.Body == null || owner.Body.PrimaryHand == null ||
                !ReferenceEquals(owner.Body.PrimaryHand.MaybeWeapon, evt.Weapon))
                return;
            evt.AddBonusDamage(GiantBodyRulesPolicy.BrachiosaurusPowerAttackAdditionalDamage);
        }

        public override void OnEventDidTrigger(RuleCalculateWeaponStats evt) { }
    }
}
