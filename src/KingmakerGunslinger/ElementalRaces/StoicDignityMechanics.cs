using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Facts;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Components;
using UnityEngine;

namespace KingmakerGunslinger.ElementalRaces
{
    internal static class StoicDignityRuntime
    {
        internal static void VerifyContract() { VerifyContract(typeof(MechanicsContext)); }
        internal static void VerifyContract(Type contextType)
        {
            if (contextType != typeof(MechanicsContext) ||
                contextType.GetField("AssociatedBlueprint")?.FieldType != typeof(BlueprintScriptableObject) ||
                contextType.GetField("ParentContext")?.FieldType != typeof(MechanicsContext) ||
                typeof(RuleReason).GetProperty("Fact")?.PropertyType != typeof(Fact) ||
                typeof(Buff).GetProperty("Context")?.PropertyType != typeof(MechanicsContext) ||
                typeof(BlueprintAbility).GetField("Parent")?.FieldType != typeof(BlueprintAbility))
                throw new InvalidOperationException("Exact Stoic Dignity source-lineage contracts absent.");
        }
        // Bounded explicit walk, never a last-rule guess or descriptor/name match.
        internal static StoicEffectIdentity Identity(RuleReason reason)
        {
            if (reason == null) return new StoicEffectIdentity(null, null);
            return Identity(reason.Context, reason.Fact?.Blueprint, reason.Ability?.Blueprint);
        }
        internal static StoicEffectIdentity Identity(MechanicsContext context, BlueprintScriptableObject fact = null, BlueprintAbility reasonAbility = null)
        {
            object effect = fact is BlueprintUnitFact && !(fact is BlueprintAbility) ? fact : null;
            BlueprintAbility ability = reasonAbility;
            var visited = new HashSet<MechanicsContext>(); bool ambiguous = false;
            for (var current = context; current != null; current = current.ParentContext)
            {
                if (visited.Count >= 32 || !visited.Add(current)) { ambiguous = true; break; }
                var blueprint = current.AssociatedBlueprint;
                if (effect == null && blueprint is BlueprintUnitFact && !(blueprint is BlueprintAbility)) effect = blueprint;
                if (ability == null) ability = blueprint as BlueprintAbility;
            }
            var parents = new List<object>(); var seen = new HashSet<BlueprintAbility>();
            for (var current = ability; current != null; current = current.Parent)
            {
                if (seen.Count >= 32 || !seen.Add(current)) { ambiguous = true; break; }
                if (!ReferenceEquals(current, ability)) parents.Add(current);
            }
            return new StoicEffectIdentity(effect, ability, parents, ambiguous);
        }
        internal static bool MindAffecting(RuleReason reason)
        {
            if (reason == null) return false;
            SpellDescriptor descriptors = SpellDescriptor.None;
            var visited = new HashSet<MechanicsContext>();
            for (var current = reason.Context; current != null && visited.Count < 32 && visited.Add(current); current = current.ParentContext)
            {
                descriptors |= current.SpellDescriptor;
                descriptors |= Descriptor(current.AssociatedBlueprint);
            }
            descriptors |= Descriptor(reason.Fact?.Blueprint);
            var abilities = new HashSet<BlueprintAbility>();
            for (var ability = reason.Ability?.Blueprint; ability != null && abilities.Count < 32 && abilities.Add(ability); ability = ability.Parent)
                descriptors |= ability.SpellDescriptor;
            return (descriptors & SpellDescriptor.MindAffecting) != 0;
        }
        private static SpellDescriptor Descriptor(BlueprintScriptableObject blueprint)
        {
            var ability = blueprint as BlueprintAbility;
            if (ability != null)
            {
                var seen = new HashSet<BlueprintAbility>(); var result = SpellDescriptor.None;
                for (var current = ability; current != null && seen.Count < 32 && seen.Add(current); current = current.Parent)
                    result |= current.SpellDescriptor;
                return result;
            }
            var component = blueprint?.GetComponent<SpellDescriptorComponent>();
            return component == null ? SpellDescriptor.None : (SpellDescriptor)component.Descriptor;
        }
        internal static bool AlreadySuffering(UnitDescriptor beneficiary, RuleReason incoming)
        {
            var identity = Identity(incoming);
            return beneficiary.Buffs.Enumerable.Where(buff => buff.Active && !buff.IsSuppressed)
                .Any(buff => StoicDignityPolicy.SameEffect(identity, Identity(buff.Context, buff.Blueprint)));
        }
    }

    [Serializable]
    public sealed class StoicDignitySaveBonus : RuleInitiatorLogicComponent<RuleSavingThrow>
    {
        public bool AllyRecipient;
        [NonSerialized] private ConditionalWeakTable<RuleSavingThrow, object> _applied;
        public override void OnEventAboutToTrigger(RuleSavingThrow evt)
        {
            if (evt == null || Owner == null || Fact == null || !ReferenceEquals(evt.Initiator, Owner.Unit)) return;
            var holder = AllyRecipient ? (Fact as Buff)?.Context?.MaybeCaster : Owner.Unit;
            if (holder == null || holder.Descriptor == null) return;
            bool self = ReferenceEquals(holder, Owner.Unit);
            if (!StoicDignityPolicy.Eligible(true, holder.Descriptor.State.IsConscious, holder.Descriptor.State.IsDead,
                StoicDignityRuntime.MindAffecting(evt.Reason), AllyRecipient, self, holder.IsAlly(Owner.Unit),
                Vector3.Distance(holder.Position, Owner.Unit.Position), StoicDignityRuntime.AlreadySuffering(Owner, evt.Reason), true)) return;
            var stat = Owner.Stats.GetStat(evt.StatType); if (stat == null) return;
            if (_applied == null) _applied = new ConditionalWeakTable<RuleSavingThrow, object>();
            object ignored; if (_applied.TryGetValue(evt, out ignored)) return;
            var modifier = stat.AddModifier(1, Fact, GetType().FullName,
                AllyRecipient ? ModifierDescriptor.Morale : ModifierDescriptor.Trait);
            if (modifier == null) return;
            _applied.Add(evt, new object()); stat.UpdateValue(); evt.AddTemporaryModifier(modifier);
        }
        public override void OnEventDidTrigger(RuleSavingThrow evt) { }
    }
}
