using System;
using System.Runtime.CompilerServices;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items.Armors;
using Kingmaker.Enums.Damage;
using Kingmaker.Items;
using Kingmaker.Items.Slots;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility;

namespace KingmakerGunslinger.ElementalRaces
{
    [Serializable]
    public sealed class ElementalStrikeDamage :
        RuleInitiatorLogicComponent<RulePrepareDamage>
    {
        private static readonly ConditionalWeakTable<RuleDealDamage, object>
            AppliedRules = new ConditionalWeakTable<RuleDealDamage, object>();
        private static readonly object AppliedMarker = new object();

        public BlueprintRace Ifrit;
        public BlueprintRace Oread;
        public BlueprintRace Sylph;
        public BlueprintRace Undine;

        public override void OnEventAboutToTrigger(RulePrepareDamage evt)
        {
            RuleDealDamage damage = evt == null ? null : evt.ParentRule;
            RuleAttackRoll roll = damage == null ? null : damage.AttackRoll;
            RuleAttackWithWeapon attack = roll == null ? null :
                roll.RuleAttackWithWeapon;
            if (Owner == null || Owner.Progression == null || damage == null ||
                evt.DamageBundle == null || roll == null || !roll.IsHit ||
                attack == null || attack.Weapon == null ||
                !ReferenceEquals(attack.Initiator, Owner.Unit) ||
                !ReferenceEquals(damage.Initiator, Owner.Unit) ||
                !ReferenceEquals(damage.Target, attack.Target) ||
                !ReferenceEquals(evt.DamageBundle.Weapon, attack.Weapon) ||
                IsSpellDamage(damage)) return;

            DamageEnergyType energy;
            if (!TryEnergy(Owner.Progression.Race, out energy)) return;
            int bonus = ElementalFeatPolicy.ElementalStrikeBonus(
                Owner.Progression.CharacterLevel);
            if (bonus <= 0 || !TryClaim(damage)) return;

            evt.DamageBundle.Add(new EnergyDamage(
                new DiceFormula(0, DiceType.D6), energy)
            {
                PreRolledValue = bonus
            });
        }

        public override void OnEventDidTrigger(RulePrepareDamage evt) { }

        private bool TryEnergy(BlueprintRace race,
            out DamageEnergyType energy)
        {
            if (ReferenceEquals(race, Ifrit))
            {
                energy = DamageEnergyType.Fire;
                return true;
            }
            if (ReferenceEquals(race, Oread))
            {
                energy = DamageEnergyType.Acid;
                return true;
            }
            if (ReferenceEquals(race, Sylph))
            {
                energy = DamageEnergyType.Electricity;
                return true;
            }
            if (ReferenceEquals(race, Undine))
            {
                energy = DamageEnergyType.Cold;
                return true;
            }
            energy = default(DamageEnergyType);
            return false;
        }

        private static bool IsSpellDamage(RuleDealDamage damage)
        {
            BlueprintAbility ability = damage == null ? null :
                damage.SourceAbility;
            while (ability != null)
            {
                if (ability.Type == AbilityType.Spell) return true;
                ability = ability.Parent;
            }
            return false;
        }

        private static bool TryClaim(RuleDealDamage damage)
        {
            lock (AppliedRules)
            {
                object ignored;
                if (AppliedRules.TryGetValue(damage, out ignored))
                    return false;
                AppliedRules.Add(damage, AppliedMarker);
                return true;
            }
        }
    }

    [Serializable]
    public sealed class ElementalWingsOfAirController :
        OwnedGameLogicComponent<UnitDescriptor>, IUnitEquipmentHandler,
        IUnitActiveEquipmentSetHandler, IUnitSubscriber
    {
        public BlueprintBuff FlightBuff;

        public override void OnTurnOn() { Refresh(); }
        public override void OnTurnOff() { Remove(); }

        public void HandleEquipmentSlotUpdated(ItemSlot slot,
            ItemEntity previousItem)
        {
            if (Owner != null && Owner.Body != null &&
                ReferenceEquals(slot, Owner.Body.Armor)) Refresh();
        }

        public void HandleUnitChangeActiveEquipmentSet(UnitDescriptor unit)
        {
            if (ReferenceEquals(unit, Owner)) Refresh();
        }

        internal void Refresh()
        {
            if (Owner == null || Owner.Body == null || FlightBuff == null)
                return;
            Buff existing = Owner.Buffs.GetBuff(FlightBuff);
            bool eligible = !Owner.Body.Armor.HasArmor ||
                Owner.Body.Armor.Armor.Blueprint.Type.ProficiencyGroup ==
                    ArmorProficiencyGroup.Light;
            if (eligible && existing == null)
            {
                var context = new MechanicsContext(Owner.Unit, Owner,
                    Fact.Blueprint, null, new TargetWrapper(Owner.Unit));
                Buff applied = Owner.Buffs.AddBuff(FlightBuff, context, null);
                if (applied == null)
                    throw new InvalidOperationException(
                        "Kingmaker rejected the owned Wings of Air buff.");
            }
            else if (!eligible && existing != null)
                Owner.Buffs.RemoveFact(existing);
        }

        private void Remove()
        {
            if (Owner == null || FlightBuff == null) return;
            Buff existing = Owner.Buffs.GetBuff(FlightBuff);
            if (existing != null) Owner.Buffs.RemoveFact(existing);
        }
    }
}
