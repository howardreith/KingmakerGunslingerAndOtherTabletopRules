using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.Utility;
using KingmakerGunslinger.Actions;
using KingmakerGunslinger.Firearms;

namespace KingmakerGunslinger.Deeds
{
    [Serializable]
    public sealed class DeathsShotAbilityLogic : AbilityCustomLogic,
        IAbilityAvailabilityProvider
    {
        public BlueprintBuff ArmedMarker;
        public BlueprintAbilityResource Grit;
        public BlueprintCharacterClass GunslingerClass;
        public bool IsAvailableFor(AbilityData ability)
        {
            if (ability == null || ability.Caster == null || ArmedMarker == null ||
                Grit == null || GunslingerClass == null) return false;
            ExactEquippedFirearmContext firearm; string reason;
            if (!ExactEquippedFirearmResolver.TryResolve(ability.Caster,
                out firearm, out reason)) return false;
            FirearmState state = firearm.Firearm.Repository.State;
            TrueGritDecision grit = TrueGritRuntime.Evaluate(ability.Caster,
                TrueGritDeed.DeathsShot, 1, false);
            return ability.Caster.Progression.GetClassLevel(GunslingerClass) >= 19 &&
                grit.Available && state.Condition != FirearmCondition.Wrecked &&
                state.LoadedRounds >= 1 && !ability.Caster.Buffs.RawFacts
                    .OfType<Buff>().Any(value => ReferenceEquals(
                        value.Blueprint, ArmedMarker));
        }
        public string GetReason()
        { return "Requires level 19, grit, and one loaded exact firearm."; }
        public override IEnumerator<AbilityDeliveryTarget> Deliver(
            AbilityExecutionContext context, TargetWrapper target)
        {
            if (context == null || !IsAvailableFor(context.Ability))
                throw new InvalidOperationException("Death's Shot became unavailable.");
            context.Caster.Descriptor.Buffs.AddBuff(ArmedMarker, context, null);
            yield return new AbilityDeliveryTarget(target);
        }
        public override void Cleanup(AbilityExecutionContext context) { }
    }
}
