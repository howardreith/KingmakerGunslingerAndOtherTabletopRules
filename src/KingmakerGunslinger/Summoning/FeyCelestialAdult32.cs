using System;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;

namespace KingmakerGunslinger.Summoning
{
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
