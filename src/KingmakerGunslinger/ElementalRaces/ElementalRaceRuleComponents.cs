using System;
using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.RuleSystem.Rules.Abilities;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Actions;

namespace KingmakerGunslinger.ElementalRaces
{
    [Serializable]
    public sealed class ElementalSpellAffinity :
        RuleInitiatorLogicComponent<RuleCalculateAbilityParams>
    {
        // SpellDescriptor is Int64-backed. Unity 2018 rejects that enum as a
        // serialized component field, so retain only the four authorized
        // low-bit elemental values in an ordinary Int32 field.
        public int DescriptorMask;

        public override void OnEventAboutToTrigger(
            RuleCalculateAbilityParams evt)
        {
            BlueprintAbility ability = evt == null ? null :
                evt.Blueprint as BlueprintAbility;
            if (ability == null ||
                !MatchesDescriptor(ability,
                    (SpellDescriptor)DescriptorMask))
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

    /// <summary>
    /// Hydraulic Push can complete synchronously before UnitUseAbility reaches
    /// its ordinary AbilityData.Spend call. Spend at effect commitment only
    /// when that ordinary path has not already consumed the single use.
    /// </summary>
    [Serializable]
    public sealed class ElementalHydraulicResourceCommit : ContextAction
    {
        public BlueprintAbilityResource Resource;

        public override string GetCaption()
        {
            return "Commit one available Hydraulic Push racial use";
        }

        public override void RunAction()
        {
            UnitEntityData caster = Context == null ? null :
                Context.MaybeCaster;
            if (caster == null || caster.Descriptor == null ||
                caster.Descriptor.Resources == null || Resource == null ||
                caster.Descriptor.Resources.GetResourceAmount(Resource) <= 0)
                return;
            caster.Descriptor.Resources.Spend(Resource, 1);
        }
    }
}
