using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.Enums.Damage;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
using UnityEngine;

namespace KingmakerGunslinger.ElementalRaces
{
    internal static class ElementalAlternateTraitPassiveFactory
    {
        internal static BlueprintComponent[] ComponentsFor(
            ElementalAlternateTraitId trait)
        {
            switch (trait)
            {
                case ElementalAlternateTraitId.WildfireHeart:
                    return Stat(StatType.Initiative, 4,
                        ModifierDescriptor.Racial);
                case ElementalAlternateTraitId.GraniteSkin:
                    return Stat(StatType.AC, 1,
                        ModifierDescriptor.NaturalArmor);
                case ElementalAlternateTraitId.LikeTheWind:
                    return Stat(StatType.Speed, 5,
                        ModifierDescriptor.Racial);
                case ElementalAlternateTraitId.WhisperingWind:
                    return Stat(StatType.SkillStealth, 4,
                        ModifierDescriptor.Racial);
                case ElementalAlternateTraitId.ThunderousResilience:
                    var resistance = ScriptableObject.CreateInstance<
                        AddDamageResistanceEnergy>();
                    resistance.Type = DamageEnergyType.Sonic;
                    resistance.Value = new ContextValue
                    {
                        ValueType = ContextValueType.Simple, Value = 5
                    };
                    return new BlueprintComponent[] { resistance };
                case ElementalAlternateTraitId.BrazenFlame:
                    return new BlueprintComponent[]
                    {
                        ScriptableObject.CreateInstance<
                            ElementalBrazenFlameDamage>()
                    };
                case ElementalAlternateTraitId.ForgeHardened:
                case ElementalAlternateTraitId.Secretive:
                    var saving = ScriptableObject.CreateInstance<
                        ElementalAlternateTraitSaveBonus>();
                    saving.Trait = (int)trait;
                    return new BlueprintComponent[] { saving };
                default:
                    return new BlueprintComponent[0];
            }
        }

        private static BlueprintComponent[] Stat(StatType stat, int value,
            ModifierDescriptor descriptor)
        {
            var bonus = ScriptableObject.CreateInstance<AddStatBonus>();
            bonus.Stat = stat;
            bonus.Value = value;
            bonus.Descriptor = descriptor;
            return new BlueprintComponent[] { bonus };
        }
    }

    [Serializable]
    public sealed class ElementalAlternateTraitSaveBonus :
        RuleInitiatorLogicComponent<RuleSavingThrow>
    {
        // A replay of one rule is not a second save. Distinct features still
        // contribute through the native Racial descriptor's stacking rules.
        private readonly ConditionalWeakTable<RuleSavingThrow, object>
            m_Applied = new ConditionalWeakTable<RuleSavingThrow, object>();
        public int Trait;

        public override void OnEventAboutToTrigger(RuleSavingThrow evt)
        {
            if (evt == null || Owner == null || Owner.Stats == null ||
                Fact == null) return;
            RuleReason reason = evt.Reason;
            MechanicsContext context = reason == null ? null : reason.Context;
            SpellDescriptor descriptors = SpellDescriptor.None;
            bool enchantment = false;
            bool divination = false;
            var contexts = new HashSet<MechanicsContext>();
            while (context != null && contexts.Add(context))
            {
                descriptors |= context.SpellDescriptor;
                enchantment |= context.SpellSchool == SpellSchool.Enchantment;
                divination |= context.SpellSchool == SpellSchool.Divination;
                context = context.ParentContext;
            }
            BlueprintAbility ability = reason != null && reason.Ability != null
                ? reason.Ability.Blueprint : reason == null ||
                    reason.Context == null ? null :
                    reason.Context.SourceAbility;
            var abilities = new HashSet<BlueprintAbility>();
            while (ability != null && abilities.Add(ability))
            {
                descriptors |= ability.SpellDescriptor;
                enchantment |= ability.School == SpellSchool.Enchantment;
                divination |= ability.School == SpellSchool.Divination;
                ability = ability.Parent;
            }
            int bonus = ElementalAlternateTraitPassivePolicy.SavingThrowBonus(
                (ElementalAlternateTraitId)Trait,
                (descriptors & SpellDescriptor.Fatigue) != 0,
                (descriptors & SpellDescriptor.Exhausted) != 0,
                enchantment, divination);
            ModifiableValue stat = Owner.Stats.GetStat(evt.StatType);
            if (bonus == 0 || stat == null) return;
            object prior;
            if (m_Applied.TryGetValue(evt, out prior)) return;
            m_Applied.Add(evt, new object());
            ModifiableValue.Modifier modifier = stat.AddModifier(bonus,
                Fact, GetType().FullName, ModifierDescriptor.Racial);
            if (modifier == null) return;
            stat.UpdateValue();
            evt.AddTemporaryModifier(modifier);
        }

        public override void OnEventDidTrigger(RuleSavingThrow evt) { }
    }

    [Serializable]
    public sealed class ElementalBrazenFlameDamage :
        RuleInitiatorLogicComponent<RulePrepareDamage>
    {
        private static readonly ConditionalWeakTable<RuleDealDamage, object>
            Applied = new ConditionalWeakTable<RuleDealDamage, object>();
        private static readonly object AppliedMarker = new object();

        public override void OnEventAboutToTrigger(RulePrepareDamage evt)
        {
            RuleDealDamage damage = evt == null ? null : evt.ParentRule;
            RuleAttackRoll roll = damage == null ? null : damage.AttackRoll;
            RuleAttackWithWeapon attack = roll == null ? null :
                roll.RuleAttackWithWeapon;
            if (Owner == null || damage == null || evt.DamageBundle == null ||
                attack == null || attack.Weapon == null) return;

            bool exact = ReferenceEquals(attack.Initiator, Owner.Unit) &&
                ReferenceEquals(damage.Initiator, Owner.Unit) &&
                ReferenceEquals(damage.Target, attack.Target) &&
                ReferenceEquals(evt.DamageBundle.Weapon, attack.Weapon);
            bool spell = false;
            var visited = new HashSet<BlueprintAbility>();
            BlueprintAbility ability = damage.SourceAbility;
            while (ability != null && visited.Add(ability))
            {
                spell |= ability.Type == AbilityType.Spell ||
                    ability.Type == AbilityType.SpellLike;
                ability = ability.Parent;
            }
            int bonus = ElementalAlternateTraitPassivePolicy.BrazenFlameDamage(
                roll.IsHit, attack.Weapon.Blueprint.IsMelee, exact, spell);
            if (bonus == 0) return;
            lock (Applied)
            {
                object ignored;
                if (Applied.TryGetValue(damage, out ignored)) return;
                Applied.Add(damage, AppliedMarker);
            }
            evt.DamageBundle.Add(new EnergyDamage(
                new DiceFormula(0, DiceType.D6), DamageEnergyType.Fire)
            {
                PreRolledValue = bonus
            });
        }

        public override void OnEventDidTrigger(RulePrepareDamage evt) { }
    }
}
