using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items.Armors;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.Enums.Damage;
using Kingmaker.Items;
using Kingmaker.Items.Slots;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.Base;
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
                IsSpellDamage(damage) || !ElementalFeatTransientRuntime
                    .IsElementalStrikeActive(Owner)) return;

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
    public sealed class ElementalScorchingWeaponsAbilityLogic :
        AbilityCustomLogic, IAbilityAvailabilityProvider
    {
        public BlueprintRace Ifrit;
        public BlueprintBuff Marker;
        public BlueprintWeaponEnchantment WeaponEnchantment;

        public bool IsAvailableFor(AbilityData ability)
        {
            UnitEntityData caster = ability == null || ability.Caster == null
                ? null : ability.Caster.Unit;
            return IsExactIfrit(caster) && Marker != null &&
                WeaponEnchantment != null &&
                caster.Descriptor.Buffs.GetBuff(Marker) == null;
        }

        public string GetReason()
        {
            return "Scorching Weapons requires an Ifrit and cannot be activated again while its one-round effect is active.";
        }

        public override IEnumerator<AbilityDeliveryTarget> Deliver(
            AbilityExecutionContext context, TargetWrapper target)
        {
            if (context == null || context.Caster == null ||
                !IsAvailableFor(context.Ability))
                throw new InvalidOperationException(
                    "Scorching Weapons prerequisites changed before execution.");

            UnitEntityData caster = context.Caster;
            ItemEntityWeapon[] weapons = Snapshot(caster);
            var added = new List<ItemEnchantment>();
            Buff marker = null;
            try
            {
                marker = caster.Descriptor.Buffs.AddBuff(Marker, context,
                    TimeSpan.FromSeconds(6d));
                if (marker == null || !ReferenceEquals(
                        caster.Descriptor.Buffs.GetBuff(Marker), marker))
                    throw new InvalidOperationException(
                        "Kingmaker rejected the Scorching Weapons round marker.");

                foreach (ItemEntityWeapon weapon in weapons)
                {
                    ItemEnchantment enchantment = weapon.AddEnchantment(
                        WeaponEnchantment, context, new Rounds(1));
                    if (enchantment == null)
                        throw new InvalidOperationException(
                            "Kingmaker rejected a Scorching Weapons item enchantment.");
                    enchantment.RemoveOnUnequipItem = false;
                    added.Add(enchantment);
                }
                ElementalFeatTransientRuntime.BeginScorchingWeapons(
                    caster.Descriptor, marker, weapons);
            }
            catch
            {
                for (int index = added.Count - 1; index >= 0; index--)
                {
                    ItemEnchantment enchantment = added[index];
                    if (enchantment != null && enchantment.Owner != null)
                        enchantment.Owner.RemoveEnchantment(enchantment);
                }
                if (marker != null && caster.Descriptor.Buffs.GetBuff(Marker) !=
                        null)
                    caster.Descriptor.Buffs.RemoveFact(marker);
                ElementalFeatTransientRuntime.RemoveScorchingWeapons(
                    caster.Descriptor);
                throw;
            }

            yield return new AbilityDeliveryTarget(target);
        }

        public override void Cleanup(AbilityExecutionContext context) { }

        private bool IsExactIfrit(UnitEntityData caster)
        {
            return caster != null && caster.Descriptor != null &&
                caster.Descriptor.Progression != null && Ifrit != null &&
                ReferenceEquals(caster.Descriptor.Progression.Race, Ifrit);
        }

        private static ItemEntityWeapon[] Snapshot(UnitEntityData caster)
        {
            if (caster == null || caster.Body == null)
                return Array.Empty<ItemEntityWeapon>();
            ItemEntityWeapon primary = caster.Body.PrimaryHand == null ? null :
                caster.Body.PrimaryHand.MaybeWeapon;
            ItemEntityWeapon secondary = caster.Body.SecondaryHand == null ?
                null : caster.Body.SecondaryHand.MaybeWeapon;
            return new[] { primary, secondary }
                .Where(IsManufacturedMetalWeapon)
                .Distinct()
                .Take(2)
                .ToArray();
        }

        internal static bool IsManufacturedMetalWeapon(
            ItemEntityWeapon weapon)
        {
            return weapon != null && weapon.Blueprint != null &&
                !weapon.Blueprint.IsNatural && !weapon.Blueprint.IsUnarmed &&
                weapon.Blueprint.Category.HasSubCategory(
                    WeaponSubCategory.Metal);
        }
    }

    [Serializable]
    public sealed class ElementalScorchingWeaponsDamage :
        WeaponEnchantmentLogic,
        IInitiatorRulebookHandler<RulePrepareDamage>
    {
        private sealed class OwnedPacket
        {
            internal ItemEnchantment Enchantment;
            internal EnergyDamage Damage;
        }

        private static readonly ConditionalWeakTable<RuleDealDamage, OwnedPacket>
            AppliedRules = new ConditionalWeakTable<RuleDealDamage, OwnedPacket>();

        public BlueprintFeature InnerFlame;

        public void OnEventAboutToTrigger(RulePrepareDamage evt)
        {
            RuleDealDamage damage = evt == null ? null : evt.ParentRule;
            RuleAttackRoll roll = damage == null ? null : damage.AttackRoll;
            RuleAttackWithWeapon attack = roll == null ? null :
                roll.RuleAttackWithWeapon;
            ItemEnchantment enchantment = Fact as ItemEnchantment;
            UnitEntityData caster = enchantment == null ||
                enchantment.ParentContext == null ? null :
                enchantment.ParentContext.MaybeCaster;
            if (Owner == null || damage == null || evt.DamageBundle == null ||
                roll == null || !roll.IsHit || attack == null ||
                caster == null || caster.Descriptor == null ||
                !ReferenceEquals(attack.Weapon, Owner) ||
                !ReferenceEquals(evt.DamageBundle.Weapon, Owner) ||
                !ReferenceEquals(attack.Initiator, caster) ||
                !ReferenceEquals(damage.Initiator, caster) ||
                !ReferenceEquals(damage.Target, attack.Target) ||
                !ElementalFeatTransientRuntime.IsScorchingWeaponsActive(
                    caster.Descriptor, Owner as ItemEntityWeapon) ||
                HasOtherFireDamage(evt.DamageBundle, enchantment)) return;

            bool inner = InnerFlame != null &&
                caster.Descriptor.HasFact(InnerFlame);
            ElementalFeatDamageAmount amount = ElementalFeatPolicy
                .ScorchingWeaponsDamage(true, inner, false);
            if (amount.IsEmpty) return;

            EnergyDamage packet = amount.DiceCount > 0
                ? new EnergyDamage(new DiceFormula(amount.DiceCount,
                    DiceType.D6), DamageEnergyType.Fire)
                : new EnergyDamage(new DiceFormula(0, DiceType.D6),
                    DamageEnergyType.Fire)
                    { PreRolledValue = amount.FlatBonus };
            if (!TryClaim(damage, enchantment, packet)) return;
            evt.DamageBundle.Add(packet);
        }

        public void OnEventDidTrigger(RulePrepareDamage evt)
        {
            // Native RuleDealDamage finishes all preparation handlers before
            // constructing RuleCalculateDamage. A later-acquired effect can
            // therefore add fire after our About handler. Recheck here, and
            // remove only the exact packet contributed by this enchantment.
            RuleDealDamage damage = evt == null ? null : evt.ParentRule;
            if (damage == null || evt.DamageBundle == null) return;
            OwnedPacket own;
            lock (AppliedRules)
                if (!AppliedRules.TryGetValue(damage, out own)) return;
            if (!ReferenceEquals(own.Enchantment, Fact) || Owner == null ||
                !ReferenceEquals(evt.DamageBundle.Weapon, Owner) ||
                !HasOtherFireDamage(evt.DamageBundle, own.Enchantment,
                    own.Damage)) return;
            evt.DamageBundle.Remove(value => ReferenceEquals(value, own.Damage));
        }

        private bool HasOtherFireDamage(DamageBundle bundle,
            ItemEnchantment own, EnergyDamage ownPacket = null)
        {
            if (bundle.OfType<EnergyDamage>().Any(value =>
                    !ReferenceEquals(value, ownPacket) &&
                    value.EnergyType == DamageEnergyType.Fire)) return true;
            return Owner.Enchantments.Any(value => value != null &&
                !ReferenceEquals(value, own) && !value.IsEnded &&
                value.Blueprint != null &&
                (value.Blueprint.ComponentsArray ??
                    Array.Empty<BlueprintComponent>())
                .OfType<WeaponEnergyDamageDice>().Any(component =>
                    component.Element == DamageEnergyType.Fire));
        }

        private static bool TryClaim(RuleDealDamage damage,
            ItemEnchantment enchantment, EnergyDamage packet)
        {
            lock (AppliedRules)
            {
                OwnedPacket ignored;
                if (AppliedRules.TryGetValue(damage, out ignored))
                    return false;
                AppliedRules.Add(damage, new OwnedPacket
                {
                    Enchantment = enchantment, Damage = packet
                });
                return true;
            }
        }
    }

    [Serializable]
    public sealed class ElementalScorchingWeaponsSaveBonus :
        RuleInitiatorLogicComponent<RuleSavingThrow>
    {
        private static readonly ConditionalWeakTable<RuleSavingThrow, object>
            AppliedRules = new ConditionalWeakTable<RuleSavingThrow, object>();
        private static readonly object AppliedMarker = new object();

        public BlueprintFeature InnerFlame;

        public override void OnEventAboutToTrigger(RuleSavingThrow evt)
        {
            if (evt == null || Owner == null || Owner.Stats == null ||
                Fact == null) return;
            SpellDescriptor descriptor = SourceDescriptor(evt);
            bool hasFire = (descriptor & SpellDescriptor.Fire) != 0;
            bool hasLight = IsNativeLightSpell(SourceAbility(evt));
            bool fireAttack = SourceDealsFireDamage(evt);
            bool inner = InnerFlame != null && Owner.HasFact(InnerFlame);
            int bonus = ElementalFeatPolicy.ScorchingWeaponsSaveBonus(true,
                inner, fireAttack, hasFire, hasLight);
            if (bonus <= 0 || !TryClaim(evt)) return;

            ModifiableValue stat = Owner.Stats.GetStat(evt.StatType);
            if (stat == null) return;
            ModifiableValue.Modifier modifier = stat.AddModifier(bonus, Fact,
                GetType().FullName, ModifierDescriptor.Racial);
            if (modifier == null) return;
            stat.UpdateValue();
            evt.AddTemporaryModifier(modifier);
        }

        public override void OnEventDidTrigger(RuleSavingThrow evt) { }

        private static SpellDescriptor SourceDescriptor(RuleSavingThrow evt)
        {
            SpellDescriptor result = SpellDescriptor.None;
            RuleReason reason = evt == null ? null : evt.Reason;
            MechanicsContext context = reason == null ? null : reason.Context;
            if (context != null) result |= context.SpellDescriptor;
            BlueprintAbility ability = SourceAbility(evt);
            var visited = new HashSet<BlueprintAbility>();
            while (ability != null && visited.Add(ability))
            {
                result |= ability.SpellDescriptor;
                ability = ability.Parent;
            }
            return result;
        }

        private static BlueprintAbility SourceAbility(RuleSavingThrow evt)
        {
            RuleReason reason = evt == null ? null : evt.Reason;
            MechanicsContext context = reason == null ? null : reason.Context;
            return reason != null && reason.Ability != null ?
                reason.Ability.Blueprint : context == null ? null :
                context.SourceAbility;
        }

        private static bool IsNativeLightSpell(BlueprintAbility ability)
        {
            if (ability == null || ability.Type != AbilityType.Spell)
                return false;
            var visited = new HashSet<BlueprintAbility>();
            BlueprintAbility current = ability;
            while (current != null && visited.Add(current))
            {
                if (ElementalFeatPolicy.IsExactNativeLightSpellGuid(
                        current.AssetGuid)) return true;
                current = current.Parent;
            }
            return false;
        }

        private static bool SourceDealsFireDamage(RuleSavingThrow evt)
        {
            RuleReason reason = evt == null ? null : evt.Reason;
            RuleDealDamage damage = reason == null ? null :
                reason.Rule as RuleDealDamage;
            return damage != null && damage.DamageBundle != null &&
                damage.DamageBundle.OfType<EnergyDamage>().Any(value =>
                    value.EnergyType == DamageEnergyType.Fire);
        }

        private static bool TryClaim(RuleSavingThrow rule)
        {
            lock (AppliedRules)
            {
                object ignored;
                if (AppliedRules.TryGetValue(rule, out ignored)) return false;
                AppliedRules.Add(rule, AppliedMarker);
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
