using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Controllers.Units;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using KingmakerGunslinger.Firearms;
using KingmakerGunslinger.Diagnostics;
using KingmakerGunslinger.Deeds;
using KingmakerGunslinger.Rules;
using KingmakerGunslinger.Scatter;
using Kingmaker.Utility;

namespace KingmakerGunslinger.Archetypes
{
    [Serializable]
    public sealed class SteadyAimAbilityLogic : AbilityCustomLogic,
        IAbilityAvailabilityProvider
    {
        public BlueprintBuff ArmedMarker;
        public BlueprintAbilityResource Grit;
        public BlueprintUnitFact TrueGritChoice;

        public bool IsAvailableFor(AbilityData ability)
        {
            return ability != null && ability.Caster != null &&
                ArmedMarker != null && Grit != null &&
                SteadyAimRuntime.HasRequiredGrit(ability.Caster,
                    Grit, TrueGritChoice) &&
                !ability.Caster.Buffs.RawFacts.OfType<Buff>().Any(value =>
                    ReferenceEquals(value.Blueprint, ArmedMarker));
        }

        public string GetReason()
        { return "Steady Aim is already armed or requires positive grit."; }

        public override IEnumerator<AbilityDeliveryTarget> Deliver(
            AbilityExecutionContext context, TargetWrapper target)
        {
            if (context == null || context.Caster == null ||
                !IsAvailableFor(context.Ability))
                throw new InvalidOperationException(
                    "Steady Aim prerequisites changed before execution.");
            context.Caster.Descriptor.Buffs.AddBuff(ArmedMarker, context,
                TimeSpan.FromSeconds(6d));
            if (!context.Caster.Descriptor.Buffs.RawFacts.OfType<Buff>().Any(
                    value => ReferenceEquals(value.Blueprint, ArmedMarker)))
                throw new InvalidOperationException(
                    "Steady Aim armed marker was rejected.");
            yield return new AbilityDeliveryTarget(target);
        }

        public override void Cleanup(AbilityExecutionContext context) { }
    }

    public sealed class SteadyAimAttackHandler :
        RuleInitiatorLogicComponent<RuleAttackWithWeapon>
    {
        public BlueprintAbilityResource Grit;
        public BlueprintUnitFact TrueGritChoice;

        public override void OnEventAboutToTrigger(RuleAttackWithWeapon evt)
        {
            if (evt == null || evt.AttackRoll == null || Owner == null ||
                Fact == null) return;
            FirearmMarkerSnapshot marker =
                FirearmMarkerLookup.ReadFromRuleEvent(evt.AttackRoll);
            if (!SteadyAimPolicy.IsQualifyingShot(marker.IsExactFirearm,
                    marker.MarkerCount, marker.Definition == null ?
                        FirearmKind.Unknown : marker.Definition.Kind,
                    ScatterVolleyRuntime.ShouldBypassOrdinaryDischarge(
                        evt.AttackRoll)))
                return;
            bool available = SteadyAimRuntime.HasRequiredGrit(Owner, Grit,
                TrueGritChoice);
            Owner.Buffs.RemoveFact(Fact);
            if (available)
                EffectiveFirearmRangeRuntime.Register(evt.AttackRoll,
                    SteadyAimPolicy.RangeBonusFeet);
        }

        public override void OnEventDidTrigger(RuleAttackWithWeapon evt) { }

    }

    internal static class SteadyAimRuntime
    {
        internal static bool HasRequiredGrit(UnitDescriptor owner,
            BlueprintAbilityResource grit, BlueprintUnitFact trueGritChoice)
        {
            if (owner == null || grit == null) return false;
            return TrueGritRuntime.Evaluate(owner, TrueGritDeed.SteadyAim,
                0, true).Available;
        }
    }
}
