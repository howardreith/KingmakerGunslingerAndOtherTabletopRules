using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Blueprints.Root;
using Kingmaker.Enums;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;

namespace KingmakerGunslinger.Summoning
{
    /// <summary>
    /// The native PowerfulCharge adds 1.5 Strength multipliers; the printed
    /// Rhino profiles need their own smaller, exact bonus. This component is
    /// installed only on the creature that owns its configured gore weapon.
    /// </summary>
    [Serializable]
    public sealed class UngulatePowerfulCharge :
        RuleInitiatorLogicComponent<RuleCalculateWeaponStats>
    {
        public BlueprintItemWeapon Gore;
        public int AdditionalDiceRolls;
        public int AdditionalDamageBonus;
        public bool DuplicateLiveDamageDice;
        public bool AddHalfLiveStrengthBonus;

        public override void OnEventAboutToTrigger(RuleCalculateWeaponStats evt)
        {
            if (evt == null || Owner == null || Owner.Unit == null ||
                !ReferenceEquals(evt.Initiator, Owner.Unit) ||
                evt.Weapon == null || Gore == null ||
                !ReferenceEquals(evt.Weapon.Blueprint, Gore) ||
                evt.AttackWithWeapon == null ||
                !evt.AttackWithWeapon.IsCharge ||
                !evt.AttackWithWeapon.IsFirstAttack ||
                evt.AttackWithWeapon.IsAttackOfOpportunity ||
                !Owner.HasFact(BlueprintRoot.Instance.SystemMechanics.ChargeBuff) ||
                (!DuplicateLiveDamageDice && AdditionalDiceRolls <= 0) ||
                (!AddHalfLiveStrengthBonus && AdditionalDamageBonus < 0))
                return;

            DiceFormula baseDice = evt.WeaponDamageDiceOverride ?? evt.Weapon.Damage;
            if (baseDice.Rolls <= 0) return;
            if (DuplicateLiveDamageDice)
                baseDice = WeaponDamageScaleTable.Scale(baseDice, Size.Huge,
                    evt.WeaponSize, evt.Weapon.Blueprint);
            evt.WeaponDamageDiceOverride = new DiceFormula(
                DuplicateLiveDamageDice ? baseDice.Rolls * 2 :
                    baseDice.Rolls + AdditionalDiceRolls, baseDice.Dice);
            // These are the printed final dice for this exact configured summon.
            // Kingmaker otherwise scales an override again after fact logic.
            evt.DoNotScaleDamage = true;
            evt.AddBonusDamage(AddHalfLiveStrengthBonus ?
                ArmoredDinosaurRulesPolicy.PowerfulChargeLiveBonus(
                    Owner.Stats.Strength.Bonus) : AdditionalDamageBonus);
        }

        public override void OnEventDidTrigger(RuleCalculateWeaponStats evt) { }
    }
}
