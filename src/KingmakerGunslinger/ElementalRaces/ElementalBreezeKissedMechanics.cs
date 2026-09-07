using System;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Components;

namespace KingmakerGunslinger.ElementalRaces
{
    [Serializable]
    public sealed class ElementalBreezeReadyRequirement : BlueprintComponent,
        Kingmaker.UnitLogic.Abilities.Components.Base.IAbilityCasterChecker
    {
        public BlueprintAbilityResource Resource;
        public bool CorrectCaster(Kingmaker.EntitySystem.Entities.UnitEntityData caster)
        {
            return caster != null && Resource != null && caster.Descriptor.Resources != null &&
                caster.Descriptor.Resources.GetResourceAmount(Resource) > 0;
        }
        public string GetReason() { return "The winds are exhausted until ordinary rest."; }
    }

    [Serializable]
    public sealed class ElementalBreezeKissedArmorClass : RuleTargetLogicComponent<RuleCalculateAC>
    {
        public BlueprintAbilityResource Resource;
        public BlueprintBuff CalmedBuff;

        public override void OnEventAboutToTrigger(RuleCalculateAC evt)
        {
            RuleAttackRoll roll = Rulebook.CurrentContext == null ? null :
                Rulebook.CurrentContext.LastEvent<RuleAttackRoll>();
            if (evt == null || roll == null || Owner == null || Resource == null || CalmedBuff == null ||
                !ReferenceEquals(evt.Target, Owner.Unit) || !ReferenceEquals(roll.Target, evt.Target) ||
                !ReferenceEquals(roll.Initiator, evt.Initiator) || roll.AttackType != evt.AttackType) return;
            int bonus = ElementalBreezeKissedRuntime.ArmorClassBonus(roll,
                Owner.Resources.GetResourceAmount(Resource) > 0, Owner.HasFact(CalmedBuff));
            if (bonus == 0) return;
            ModifiableValue.Modifier modifier = Owner.Stats.AC.AddModifier(bonus, Fact,
                GetType().FullName, ModifierDescriptor.Racial);
            if (modifier == null) return;
            Owner.Stats.AC.UpdateValue();
            evt.AddTemporaryModifier(modifier);
        }
        public override void OnEventDidTrigger(RuleCalculateAC evt) { }
    }

    internal static class ElementalBreezeKissedRuntime
    {
        internal static int ArmorClassBonus(RuleAttackRoll roll, bool available, bool calmed)
        {
            RuleAttackWithWeapon attack = roll == null ? null : roll.RuleAttackWithWeapon;
            RuleCalculateWeaponStats stats = roll == null ? null : roll.WeaponStats;
            bool exact = attack != null && stats != null &&
                ReferenceEquals(stats, attack.WeaponStats) && ReferenceEquals(stats.Weapon, attack.Weapon) &&
                ReferenceEquals(attack.Initiator, roll.Initiator) && ReferenceEquals(attack.Target, roll.Target);
            DamageTypeDescription physical = exact && stats.DamageDescription != null &&
                stats.DamageDescription.Count > 0 && stats.DamageDescription[0] != null
                ? stats.DamageDescription[0].TypeDescription : null;
            bool known = physical != null && physical.Type == DamageType.Physical && physical.Physical != null;
            bool ranged = exact && attack.Weapon != null && attack.Weapon.Blueprint.IsRanged &&
                attack.Weapon.Blueprint.Category != WeaponCategory.Ray &&
                (roll.AttackType == AttackType.Ranged || roll.AttackType == AttackType.RangedTouch);
            return ElementalBreezeKissedPolicy.ArmorClassBonus(available, calmed, exact, ranged,
                roll != null && (HasAbilitySource(roll.Reason) || (attack != null && HasAbilitySource(attack.Reason))),
                known, known ? physical.Physical.EnhancementTotal : -1);
        }

        private static bool HasAbilitySource(RuleReason reason)
        {
            return reason != null && (reason.Ability != null ||
                (reason.Context != null && reason.Context.SourceAbility != null));
        }

        internal static void RemoveInactive(UnitDescriptor owner, ElementalAlternateTraitBlueprints trait)
        {
            if (trait.Definition.Id != ElementalAlternateTraitId.BreezeKissed) return;
            BlueprintBuff calmed = trait.Mechanics().OfType<BlueprintBuff>().Single();
            foreach (Buff buff in owner.Buffs.Enumerable.Where(value =>
                ReferenceEquals(value.Blueprint, calmed)).ToArray()) owner.RemoveFact(buff);
        }

        internal static bool IsExact(UnitDescriptor owner, ElementalAlternateTraitBlueprints trait, bool active)
        {
            if (trait.Definition.Id != ElementalAlternateTraitId.BreezeKissed) return true;
            BlueprintBuff calmed = trait.Mechanics().OfType<BlueprintBuff>().Single();
            int count = owner.Buffs.Enumerable.Count(value => ReferenceEquals(value.Blueprint, calmed));
            return active ? count <= 1 : count == 0;
        }
    }
}
