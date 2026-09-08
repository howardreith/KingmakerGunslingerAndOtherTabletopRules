using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.Utility;
using KingmakerGunslinger.Blueprints;
using UnityEngine;

namespace KingmakerGunslinger.ElementalRaces
{
    internal static class ElementalSummonInsightFactory
    {
        internal static BlueprintComponent[] ComponentsFor(
            LibraryScriptableObject library, ElementalAlternateTraitId trait)
        {
            string subtype = ElementalSummonInsightPolicy.NativeSubtypeGuid(trait);
            if (subtype == null) return new BlueprintComponent[0];
            var result = ScriptableObject.CreateInstance<ElementalSummonInsight>();
            result.Trait = (int)trait;
            result.Subtype = BlueprintLibraryLookup.RequireExact<BlueprintFeature>(
                library, subtype, "exact native elemental summon subtype");
            result.SpellParents = ElementalSummonInsightPolicy.NativeParentGuids
                .Select(guid => BlueprintLibraryLookup.RequireExact<BlueprintAbility>(
                    library, guid, "native Summon Monster/Nature's Ally parent"))
                .ToArray();
            return new BlueprintComponent[] { result };
        }
    }

    [Serializable]
    public sealed class ElementalSummonInsight :
        RuleInitiatorLogicComponent<RuleSummonUnit>
    {
        private static readonly ConditionalWeakTable<RuleSummonUnit, object>
            Applied = new ConditionalWeakTable<RuleSummonUnit, object>();
        private static readonly object AppliedMarker = new object();

        public int Trait;
        public BlueprintFeature Subtype;
        public BlueprintAbility[] SpellParents;

        public override void OnEventAboutToTrigger(RuleSummonUnit evt)
        {
            if (evt == null || Owner == null ||
                !ReferenceEquals(evt.Initiator, Owner.Unit)) return;
            // The native AddClassLevelToSummonDuration component reads this
            // same RuleReason.Ability and its actual spellbook. No school,
            // descriptor, immunity, display name, or energy type is a subtype.
            AbilityData ability = evt.Reason == null ? null : evt.Reason.Ability;
            bool spell = ability != null && ability.Spellbook != null &&
                ability.Blueprint != null &&
                ability.Blueprint.Type == AbilityType.Spell &&
                ReferenceEquals(ability.Caster, Owner);
            int rounds = ElementalSummonInsightPolicy.BonusRounds(
                (ElementalAlternateTraitId)Trait, spell,
                IsNamedFamily(ability), HasSubtype(evt.Blueprint),
                !evt.DoNotLinkToCaster && evt.Duration.Seconds > TimeSpan.Zero);
            if (rounds == 0) return;
            lock (Applied)
            {
                object ignored;
                if (Applied.TryGetValue(evt, out ignored)) return;
                Applied.Add(evt, AppliedMarker);
            }
            // Native spawning consumes Duration + BonusDuration for each
            // creature's canonical lifecycle buff. Never scale either CL or
            // the base duration and never multiply this flat bonus.
            evt.BonusDuration += rounds.Rounds();
        }

        public override void OnEventDidTrigger(RuleSummonUnit evt) { }

        internal bool IsNamedFamily(AbilityData ability)
        {
            var visitedBlueprints = new HashSet<BlueprintAbility>();
            // ConvertedFrom can name the sacrificed slot rather than the
            // spell actually being cast. Native and project family variants
            // have audited exact BlueprintAbility.Parent links.
            for (BlueprintAbility blueprint = ability == null ? null : ability.Blueprint;
                blueprint != null && visitedBlueprints.Add(blueprint);
                blueprint = blueprint.Parent)
                if ((SpellParents ?? new BlueprintAbility[0]).Any(
                    value => ReferenceEquals(value, blueprint))) return true;
            return false;
        }

        internal bool HasSubtype(BlueprintUnit unit)
        {
            if (unit == null || Subtype == null) return false;
            var pending = new Stack<BlueprintUnitFact>(unit.AddFacts ??
                new BlueprintUnitFact[0]);
            var visited = new HashSet<BlueprintUnitFact>();
            while (pending.Count > 0)
            {
                BlueprintUnitFact fact = pending.Pop();
                if (fact == null || !visited.Add(fact)) continue;
                if (ReferenceEquals(fact, Subtype)) return true;
                foreach (AddFacts add in (fact.ComponentsArray ??
                    new BlueprintComponent[0]).OfType<AddFacts>())
                    foreach (BlueprintUnitFact child in add.Facts ??
                        new BlueprintUnitFact[0]) pending.Push(child);
            }
            return false;
        }
    }
}
