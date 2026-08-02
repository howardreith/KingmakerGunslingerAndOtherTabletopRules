using System;
using System.Collections.Generic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.Utility;
using UnityEngine;

namespace KingmakerGunslinger.Deeds
{
    [Serializable]
    public sealed class StopBleedingAbilityLogic : AbilityCustomLogic,
        IAbilityAvailabilityProvider
    {
        internal static StopBleedingAbilityLogic Create()
        {
            return ScriptableObject.CreateInstance<StopBleedingAbilityLogic>();
        }

        public bool IsAvailableFor(AbilityData ability)
        {
            if (ability == null || ability.Caster == null) return false;
            Actions.ExactEquippedFirearmContext firearm;
            string reason;
            if (!Actions.ExactEquippedFirearmResolver.TryResolve(
                    ability.Caster, out firearm, out reason)) return false;
            Firearms.FirearmState state = firearm.Firearm.Repository.State;
            Blueprints.GunslingerClassBlueprintSet gunslinger =
                Bootstrap.BlueprintBootstrap.GunslingerClass;
            return state.Condition != Firearms.FirearmCondition.Wrecked &&
                state.LoadedRounds > 0 && gunslinger != null &&
                TrueGritRuntime.Evaluate(ability.Caster,
                    TrueGritDeed.StopBleeding, 0, true).Available;
        }

        public string GetReason()
        {
            return "Requires positive grit and exactly one equipped loaded, non-Wrecked firearm.";
        }

        public override IEnumerator<AbilityDeliveryTarget> Deliver(
            AbilityExecutionContext context, TargetWrapper target)
        {
            if (context == null || context.Caster == null || target == null ||
                target.Unit == null)
                throw new InvalidOperationException("Stop Bleeding requires a unit target.");
            StopBleedingRuntime.Execute(context.Caster.Descriptor,
                context.Caster, target.Unit);
            yield return new AbilityDeliveryTarget(target);
        }

        public override void Cleanup(AbilityExecutionContext context) { }
    }
}
