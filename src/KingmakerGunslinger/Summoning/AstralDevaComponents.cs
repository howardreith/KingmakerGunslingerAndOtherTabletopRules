using System;
using System.Reflection;
using Kingmaker.Blueprints;
using Kingmaker.Controllers.Brain;
using Kingmaker.Controllers.Brain.Blueprints.Considerations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.Utility;
using UnityEngine;

namespace KingmakerGunslinger.Summoning
{
    /// <summary>The lesser-globe half of one Astral Deva protective aura.
    /// The recipient buff's context identifies the exact emitter. Spells and
    /// SLAs of level three or lower that originate outside that emitter's
    /// twenty-foot aura are immune; an origin inside remains unaffected.</summary>
    [Serializable]
    public sealed class AstralProtectiveGlobe :
        RuleTargetLogicComponent<RuleSpellResistanceCheck>
    {
        public BlueprintUnit OwningBlueprint;
        public int MaximumSpellLevel;
        public int RadiusFeet;
        private static readonly FieldInfo TargetIsImmune =
            typeof(RuleSpellResistanceCheck).GetField(
                "<TargetIsImmune>k__BackingField",
                BindingFlags.Instance | BindingFlags.NonPublic);

        public override void OnEventAboutToTrigger(RuleSpellResistanceCheck evt)
        { Apply(evt); }

        public override void OnEventDidTrigger(RuleSpellResistanceCheck evt)
        { Apply(evt); }

        private void Apply(RuleSpellResistanceCheck evt)
        {
            UnitEntityData target = Owner == null ? null : Owner.Unit;
            UnitEntityData emitter = Fact == null || Fact.MaybeContext == null ?
                null : Fact.MaybeContext.MaybeCaster;
            UnitEntityData source = evt == null || evt.Context == null ?
                null : evt.Context.MaybeCaster;
            BlueprintAbility ability = evt == null || evt.Context == null ?
                null : evt.Context.SourceAbility;
            if (evt == null || target == null || emitter == null || source == null ||
                ability == null || Fact == null || !Fact.Active ||
                OwningBlueprint == null || !ReferenceEquals(emitter.Blueprint, OwningBlueprint) ||
                MaximumSpellLevel != AstralDevaRulesPolicy.LesserGlobeMaximumSpellLevel ||
                RadiusFeet != AstralDevaRulesPolicy.AuraRadiusFeet ||
                evt.Context.Params == null ||
                !AstralDevaRulesPolicy.BlocksLesserGlobe(
                    evt.Context.Params.SpellLevel,
                    ability.Type == AbilityType.Spell,
                    ability.Type == AbilityType.SpellLike,
                    Vector3.Distance(emitter.Position, source.Position),
                    source.Corpulence))
                return;
            if (TargetIsImmune == null || TargetIsImmune.FieldType != typeof(bool) ||
                TargetIsImmune.IsInitOnly)
                throw new MissingFieldException(typeof(RuleSpellResistanceCheck).FullName,
                    "<TargetIsImmune>k__BackingField");
            TargetIsImmune.SetValue(evt, true);
        }
    }

    /// <summary>Use the exact resource-backed Heal SLA only for a living,
    /// friendly target with meaningful damage. The ability owns its action,
    /// targeting, resource debit and native healing effect.</summary>
    public sealed class AstralHealingEligibility : Consideration
    {
        public BlueprintUnit OwningBlueprint;
        public BlueprintAbility Heal;
        public override float Score(DecisionContext context)
        {
            UnitEntityData actor = context == null ? null : context.Unit;
            UnitEntityData target = context == null || context.Target == null ?
                null : context.Target.Unit;
            if (actor == null || target == null || Heal == null ||
                !ReferenceEquals(actor.Blueprint, OwningBlueprint) ||
                actor.IsEnemy(target) || target.Destroyed ||
                target.Descriptor.State.IsDead || target.Descriptor.IsUndead ||
                target.Damage < 50) return 0f;
            var granted = actor.Descriptor.Abilities.GetAbility(Heal);
            if (granted == null) return 0f;
            var data = new AbilityData(granted);
            return data.IsAvailable && data.CanTarget(new TargetWrapper(target)) ? 1f : 0f;
        }
    }
}
