using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Facts;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using KingmakerGunslinger.Actions;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Firearms;
using KingmakerGunslinger.Firing;

namespace KingmakerGunslinger.Deeds
{
    internal static class StopBleedingRuntime
    {
        private static readonly StopBleedingService Policy = new StopBleedingService();
        private static readonly FirearmDischargeService Discharge =
            new FirearmDischargeService();

        internal static StopBleedingDecision Evaluate(UnitDescriptor caster,
            UnitEntityData casterEntity, UnitEntityData target,
            out ExactEquippedFirearmContext firearm, out Buff bleed)
        {
            firearm = null;
            bleed = FirstBleed(target);
            string reason;
            bool exact = ExactEquippedFirearmResolver.TryResolve(caster,
                out firearm, out reason);
            FirearmState state = exact ? firearm.Firearm.Repository.State :
                FirearmState.CreateEmpty();
            double distance = casterEntity == null || target == null
                ? double.MaxValue : casterEntity.DistanceTo(target);
            return Policy.Evaluate(new StopBleedingRequest(exact,
                state.Condition, state.LoadedRounds, ReadGrit(caster), distance,
                bleed == null ? 0 : CountBleeds(target)));
        }

        internal static StopBleedingResult Execute(UnitDescriptor caster,
            UnitEntityData casterEntity, UnitEntityData target)
        {
            ExactEquippedFirearmContext firearm;
            Buff bleed;
            StopBleedingDecision decision = Evaluate(caster, casterEntity, target,
                out firearm, out bleed);
            if (!decision.ShouldApply)
            {
                StopBleedingRuntimeDiagnostics.RecordRejected();
                return new StopBleedingResult(decision, null);
            }

            FirearmState before = firearm.Firearm.Repository.State;
            bool discharged = false;
            try
            {
                FirearmDischargeResult discharge = Discharge.Evaluate(before);
                if (discharge.Status != FirearmDischargeStatus.Fired)
                    throw new InvalidOperationException(
                        "Eligible Stop Bleeding did not produce a firearm discharge.");
                FirearmRuntimeState.Service.Transition(firearm.Weapon, current =>
                {
                    if (current != before)
                        throw new InvalidOperationException(
                            "Firearm state changed before Stop Bleeding delivery.");
                    return discharge.After;
                });
                discharged = true;
                target.Descriptor.Buffs.RemoveFact(bleed);
                StopBleedingRuntimeDiagnostics.RecordApplied();
                return new StopBleedingResult(decision, bleed);
            }
            catch
            {
                if (discharged)
                    FirearmRuntimeState.Service.Transition(firearm.Weapon,
                        current => before);
                StopBleedingRuntimeDiagnostics.RecordFault();
                throw;
            }
        }

        internal static Buff FirstBleed(UnitEntityData target)
        {
            return Bleeds(target).FirstOrDefault();
        }

        internal static int CountBleeds(UnitEntityData target)
        {
            return Bleeds(target).Count();
        }

        private static IEnumerable<Buff> Bleeds(UnitEntityData target)
        {
            if (target == null || target.Descriptor == null ||
                target.Descriptor.Buffs == null)
                return Enumerable.Empty<Buff>();
            return target.Descriptor.Buffs.RawFacts.OfType<Buff>().Where(IsBleed);
        }

        private static bool IsBleed(Buff buff)
        {
            if (buff == null || buff.Blueprint == null) return false;
            SpellDescriptorComponent descriptor = buff.Blueprint.ComponentsArray
                .OfType<SpellDescriptorComponent>().FirstOrDefault();
            return descriptor != null &&
                descriptor.Descriptor.HasAnyFlag(SpellDescriptor.Bleed);
        }

        private static int ReadGrit(UnitDescriptor caster)
        {
            GunslingerClassBlueprintSet gunslinger = BlueprintBootstrap.GunslingerClass;
            return caster == null || gunslinger == null ? 0 :
                caster.Resources.GetResourceAmount(gunslinger.Grit.Resource);
        }
    }
}
