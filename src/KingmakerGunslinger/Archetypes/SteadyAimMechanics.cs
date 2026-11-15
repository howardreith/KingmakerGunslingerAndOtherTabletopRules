using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Controllers.Units;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Firearms;
using KingmakerGunslinger.Firing;
using KingmakerGunslinger.Misfires;
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

    /// <summary>
    /// The armed state's references. It handles no rule itself: the weapon
    /// attack rule has no attack roll until its own trigger has resolved one,
    /// so a handler on that rule sees the roll too late to change its range.
    /// The shot is resolved by <see cref="SteadyAimRuntime.BeforeAttackRoll"/>.
    /// </summary>
    public sealed class SteadyAimAttackHandler : BlueprintComponent
    {
        public BlueprintAbilityResource Grit;
        public BlueprintUnitFact TrueGritChoice;
    }

    internal static class SteadyAimRuntime
    {
        private static readonly ReferenceEventGate ResolvedShots =
            new ReferenceEventGate();

        internal static bool HasRequiredGrit(UnitDescriptor owner,
            BlueprintAbilityResource grit, BlueprintUnitFact trueGritChoice)
        {
            if (owner == null || grit == null) return false;
            return TrueGritRuntime.Evaluate(owner, TrueGritDeed.SteadyAim,
                0, true).Available;
        }

        /// <summary>
        /// Resolves the armed state against one attack roll, from the
        /// RuleAttackRoll.OnTrigger prefix. It runs after the discharge step,
        /// which decides whether this roll fires a loaded round, and before
        /// Deadeye and the firearm armor-class frame read the roll's range.
        /// </summary>
        internal static void BeforeAttackRoll(RuleAttackRoll attackRoll)
        {
            if (attackRoll == null || DeadShotRuntime.IsProbe(attackRoll)) return;
            try
            {
                GunslingerClassBlueprintSet gunslinger =
                    BlueprintBootstrap.GunslingerClass;
                MusketMasterBlueprintSet musketMaster = gunslinger == null
                    ? null : gunslinger.MusketMaster;
                UnitEntityData initiator = attackRoll.Initiator;
                if (musketMaster == null || musketMaster.SteadyAimArmed == null ||
                    initiator == null || initiator.Descriptor == null) return;
                UnitDescriptor owner = initiator.Descriptor;
                Buff armed = owner.Buffs.RawFacts.OfType<Buff>().FirstOrDefault(
                    value => ReferenceEquals(value.Blueprint,
                        musketMaster.SteadyAimArmed));
                if (armed == null) return;

                FirearmMarkerSnapshot marker =
                    FirearmMarkerLookup.ReadFromRuleEvent(attackRoll);
                SteadyAimAttackHandler references = armed.Blueprint
                    .ComponentsArray.OfType<SteadyAimAttackHandler>()
                    .FirstOrDefault();
                SteadyAimShotDecision decision = SteadyAimPolicy.ResolveShot(
                    true, FirearmMisfireRuntime.IsEligibleAttack(attackRoll),
                    marker.IsExactFirearm, marker.MarkerCount,
                    marker.Definition == null ? FirearmKind.Unknown :
                        marker.Definition.Kind,
                    ScatterVolleyRuntime.ShouldBypassOrdinaryDischarge(
                        attackRoll),
                    references != null && HasRequiredGrit(owner,
                        references.Grit, references.TrueGritChoice));
                if (!decision.ConsumesArmedState ||
                    !ResolvedShots.TryMark(attackRoll)) return;
                owner.Buffs.RemoveFact(armed);
                if (decision.RangeBonusFeet > 0)
                    EffectiveFirearmRangeRuntime.Register(attackRoll,
                        decision.RangeBonusFeet);
            }
            catch (Exception exception)
            {
                ModContext context;
                if (ModContext.TryGet(out context))
                    context.Logger.Failure("steady-aim", "attack.failed",
                        "Steady Aim failed closed before firearm range selection.",
                        exception);
            }
        }
    }
}
