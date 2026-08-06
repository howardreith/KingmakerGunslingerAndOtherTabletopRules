using System;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using System.Linq;
using KingmakerGunslinger.Actions;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Firearms;
using KingmakerGunslinger.Firing;

namespace KingmakerGunslinger.Deeds
{
    internal static class StartlingShotRuntime
    {
        private static readonly StartlingShotService Policy =
            new StartlingShotService();
        private static readonly FirearmDischargeService Discharge =
            new FirearmDischargeService();

        internal static StartlingShotDecision Evaluate(UnitDescriptor caster,
            bool validTarget, out ExactEquippedFirearmContext firearm)
        {
            string reason;
            bool exact = ExactEquippedFirearmResolver.TryResolve(caster,
                out firearm, out reason);
            FirearmState state = exact ? firearm.Firearm.Repository.State :
                FirearmState.CreateEmpty();
            return Policy.Evaluate(new StartlingShotRequest(exact, state.Condition,
                state.LoadedRounds, ReadGrit(caster), validTarget));
        }

        internal static StartlingShotResult Execute(UnitDescriptor caster,
            UnitEntityData target, BlueprintBuff flatFootedBuff,
            MechanicsContext context)
        {
            if (flatFootedBuff == null)
                throw new ArgumentNullException("flatFootedBuff");
            ExactEquippedFirearmContext firearm;
            StartlingShotDecision decision = Evaluate(caster, target != null &&
                target.Descriptor != null, out firearm);
            if (!decision.ShouldApply)
            {
                StartlingShotRuntimeDiagnostics.RecordRejected();
                return new StartlingShotResult(decision, null);
            }

            FirearmState before = firearm.Firearm.Repository.State;
            bool discharged = false;
            Buff applied = null;
            try
            {
                FirearmDischargeResult discharge = Discharge.Evaluate(before);
                if (discharge.Status != FirearmDischargeStatus.Fired)
                    throw new InvalidOperationException(
                        "Eligible Startling Shot did not produce a discharge.");
                FirearmRuntimeState.Service.Transition(firearm.Weapon, current =>
                {
                    if (current != before)
                        throw new InvalidOperationException(
                            "Firearm state changed before Startling Shot delivery.");
                    return discharge.After;
                });
                discharged = true;
                applied = target.Descriptor.Buffs.AddBuff(flatFootedBuff, context,
                    TimeSpan.FromSeconds(6d * decision.DurationRounds));
                if (applied == null)
                    applied = target.Descriptor.Buffs.RawFacts.OfType<Buff>()
                        .SingleOrDefault(value => ReferenceEquals(
                            value.Blueprint, flatFootedBuff));
                if (applied == null)
                    throw new InvalidOperationException(
                        "Startling Shot flat-footed buff was not created.");
                StartlingShotRuntimeDiagnostics.RecordApplied();
                Audio.FirearmSoundRuntime.TryPostCommittedDischarge(
                    firearm.Definition.Kind, caster.Unit, "startling-shot");
                return new StartlingShotResult(decision, applied);
            }
            catch
            {
                if (applied != null && target != null && target.Descriptor != null)
                    target.Descriptor.Buffs.RemoveFact(applied);
                if (discharged)
                    FirearmRuntimeState.Service.Transition(firearm.Weapon,
                        current => before);
                StartlingShotRuntimeDiagnostics.RecordFault();
                throw;
            }
        }

        private static int ReadGrit(UnitDescriptor caster)
        {
            GunslingerClassBlueprintSet gunslinger = BlueprintBootstrap.GunslingerClass;
            if (caster == null || gunslinger == null) return 0;
            int current = caster.Resources.GetResourceAmount(gunslinger.Grit.Resource);
            return TrueGritRuntime.Evaluate(caster, TrueGritDeed.StartlingShot,
                0, true).Available ? Math.Max(1, current) : current;
        }
    }
}
