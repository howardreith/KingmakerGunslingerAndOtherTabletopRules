using System;
using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.RuleSystem.Rules.Abilities;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Blueprints;

namespace KingmakerGunslinger.ElementalRaces
{
    [Serializable]
    public sealed class ElementalSpellAffinity :
        RuleInitiatorLogicComponent<RuleCalculateAbilityParams>
    {
        public SpellDescriptor Descriptor;

        public override void OnEventAboutToTrigger(
            RuleCalculateAbilityParams evt)
        {
            BlueprintAbility ability = evt == null ? null :
                evt.Blueprint as BlueprintAbility;
            if (ability == null ||
                !MatchesDescriptor(ability, Descriptor))
                return;
            evt.AddBonusDC(1);
        }

        public override void OnEventDidTrigger(
            RuleCalculateAbilityParams evt) { }

        internal static bool MatchesDescriptor(BlueprintAbility ability,
            SpellDescriptor descriptor)
        {
            if (ability == null || descriptor == SpellDescriptor.None)
                return false;
            var visited = new HashSet<BlueprintAbility>();
            BlueprintAbility current = ability;
            while (current != null && visited.Add(current))
            {
                if ((current.SpellDescriptor & descriptor) != 0)
                    return true;
                current = current.Parent;
            }
            return false;
        }
    }

    [Serializable]
    public sealed class ElementalRacialSpellLikeParameters :
        RuleInitiatorLogicComponent<RuleCalculateAbilityParams>
    {
        public BlueprintAbility Ability;
        public StatType Stat = StatType.Charisma;
        public int SpellLevel = 1;

        public override void OnEventAboutToTrigger(
            RuleCalculateAbilityParams evt)
        {
            BlueprintAbility ability = evt == null ? null :
                evt.Blueprint as BlueprintAbility;
            if (ability == null || Owner == null ||
                !MatchesAbility(ability, Ability))
                return;
            evt.ReplaceCasterLevel = Math.Max(1,
                Owner.Progression.CharacterLevel);
            evt.ReplaceSpellLevel = Math.Max(1, SpellLevel);
            evt.ReplaceStat = Stat;
        }

        public override void OnEventDidTrigger(
            RuleCalculateAbilityParams evt) { }

        internal static bool MatchesAbility(BlueprintAbility candidate,
            BlueprintAbility expected)
        {
            if (candidate == null || expected == null) return false;
            var visited = new HashSet<BlueprintAbility>();
            BlueprintAbility current = candidate;
            while (current != null && visited.Add(current))
            {
                if (ReferenceEquals(current, expected) ||
                    string.Equals(current.AssetGuid, expected.AssetGuid,
                        StringComparison.Ordinal))
                    return true;
                current = current.Parent;
            }
            return false;
        }
    }
}
