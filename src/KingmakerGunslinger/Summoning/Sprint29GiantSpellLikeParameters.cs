using System;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.RuleSystem.Rules.Abilities;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Blueprints;

namespace KingmakerGunslinger.Summoning
{
    /// <summary>Printed spell-like parameters for the exact summoned owner.
    /// AbilityData.CalculateParams uses RuleCalculateAbilityParams; a
    /// ContextCalculateAbilityParams on the spell only governs its effect
    /// context and does not set the direct command's caster level or DC.</summary>
    [Serializable]
    public sealed class Sprint29GiantSpellLikeParameters :
        RuleInitiatorLogicComponent<RuleCalculateAbilityParams>
    {
        public BlueprintUnit OwningBlueprint;
        public BlueprintAbility[] Abilities;
        public int[] SpellLevels;
        public int CasterLevel;

        public override void OnEventAboutToTrigger(
            RuleCalculateAbilityParams evt)
        {
            if (evt == null || Owner?.Unit == null ||
                !ReferenceEquals(evt.Initiator, Owner.Unit) ||
                !ReferenceEquals(Owner.Unit.Blueprint, OwningBlueprint) ||
                Abilities == null || SpellLevels == null ||
                Abilities.Length != SpellLevels.Length)
                return;
            for (int i = 0; i < Abilities.Length; i++)
            {
                if (!ReferenceEquals(evt.Blueprint, Abilities[i])) continue;
                evt.ReplaceCasterLevel = CasterLevel;
                evt.ReplaceSpellLevel = SpellLevels[i];
                evt.ReplaceStat = StatType.Charisma;
                return;
            }
        }

        public override void OnEventDidTrigger(
            RuleCalculateAbilityParams evt) { }
    }
}
