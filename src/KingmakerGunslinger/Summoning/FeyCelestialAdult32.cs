using System;
using System.Reflection;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;

namespace KingmakerGunslinger.Summoning
{
    // Knowledge (nature) uses native Lore Nature's identification consumers,
    // with the printed Intelligence ability rather than native Wisdom. This
    // does not merge Survival into Knowledge or add a new skill system.
    [Serializable]
    public sealed class FeyCelestialNatureKnowledge32 : OwnedGameLogicComponent<UnitDescriptor>,
        IHandleEntityComponent<UnitEntityData>
    {
        public BlueprintUnit OwningBlueprint;
        public int TrainedContribution;
        public bool FeyClassSkill;

        public void OnEntityCreated(UnitEntityData unit)
        {
            if (unit == null || !ReferenceEquals(unit.Blueprint, OwningBlueprint) ||
                TrainedContribution < 3)
                throw new InvalidOperationException("Nature knowledge requires its exact adult owner.");
            var stats = unit.Stats;
            var skill = stats.GetStat(StatType.SkillLoreNature) as ModifiableValueSkill;
            if (skill == null || skill.BaseValue != 0 || !ReferenceEquals(skill.BaseStat, stats.Wisdom))
                throw new InvalidOperationException("Nature knowledge requires an unallocated native Wisdom skill.");
            // Fey explicitly has Knowledge (nature) as a class skill. Six ranks
            // plus three class points stay within the Satyr's eight racial HD.
            if (FeyClassSkill && !skill.ClassSkill) skill.ClassSkill.Retain();
            // Native Json.NET persists this public readonly reference. The
            // bounded creation adapter writes that exact instance field once;
            // direct C# assignment is unavailable on the installed contract.
            var baseStat = typeof(ModifiableValueSkill).GetField("BaseStat", BindingFlags.Public | BindingFlags.Instance);
            if (baseStat == null || baseStat.FieldType != typeof(ModifiableValueAttributeStat) || !baseStat.IsInitOnly)
                throw new InvalidOperationException("Native serialized skill ability reference changed.");
            skill.BaseStat.RemoveDependentValue(skill);
            baseStat.SetValue(skill, stats.Intelligence);
            skill.BaseStat.AddDependentValue(skill);
            skill.BaseValue = TrainedContribution - (skill.ClassSkill ? 3 : 0);
            skill.UpdateValue();
            // BaseStat, ClassSkill and dependent links are native serialized
            // state. No load callback reranks the skill or reapplies the link.
        }

        public void OnEntityRemoved(UnitEntityData unit) { }
    }

    // The shared Persuasion stat represents the printed Bluff/Diplomacy+15.
    // Native CheckIntimidate is a distinct rule consumer: its printed+9 has
    // six fewer rank/class-skill points, with the same live Charisma modifier.
    // This follows the existing Cloud Giant's check-specific rank correction.
    [Serializable]
    public sealed class SatyrIntimidateRanks32 : RuleInitiatorLogicComponent<RuleSkillCheck>
    {
        public BlueprintUnit OwningBlueprint;
        public override void OnEventAboutToTrigger(RuleSkillCheck evt)
        {
            if (evt?.StatType != StatType.CheckIntimidate || Owner?.Unit == null ||
                Fact?.Active != true || !ReferenceEquals(evt.Initiator, Owner.Unit) ||
                !ReferenceEquals(Owner.Unit.Blueprint, OwningBlueprint)) return;
            evt.Bonus.AddModifier(-6, this, ModifierDescriptor.UntypedStackable);
        }
        public override void OnEventDidTrigger(RuleSkillCheck evt) { }
    }

}
