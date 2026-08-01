using System;
using Kingmaker.UnitLogic;
using KingmakerGunslinger.Actions;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Firearms;
using KingmakerGunslinger.Recovery;

namespace KingmakerGunslinger.Deeds
{
    internal static class QuickClearRuntime
    {
        private static readonly QuickClearService Policy = new QuickClearService();

        internal static QuickClearDecision Evaluate(UnitDescriptor caster,
            QuickClearMode mode, out ExactEquippedFirearmContext firearm,
            out string reason)
        {
            firearm = null;
            if (!ExactEquippedFirearmResolver.TryResolve(caster, out firearm, out reason))
                return Policy.Evaluate(new QuickClearRequest(mode, false,
                    FirearmCondition.Normal, false, ReadGrit(caster)));
            // In the current item-state architecture Broken is written only by the
            // misfire transition. Other damage sources never write this token.
            QuickClearDecision decision = Policy.Evaluate(new QuickClearRequest(mode,
                true, firearm.Firearm.Repository.State.Condition, true,
                ReadGrit(caster)));
            reason = decision.Status.ToString();
            return decision;
        }

        internal static QuickClearDecision Execute(UnitDescriptor caster,
            QuickClearMode mode)
        {
            ExactEquippedFirearmContext firearm;
            string reason;
            QuickClearDecision decision = Evaluate(caster, mode, out firearm, out reason);
            if (!decision.ShouldRepair) { QuickClearRuntimeDiagnostics.Record(decision); return decision; }
            GunslingerClassBlueprintSet gunslinger = BlueprintBootstrap.GunslingerClass;
            bool spent = false;
            FirearmState before = null;
            try
            {
                if (decision.GritCost > 0)
                {
                    caster.Resources.Spend(gunslinger.Grit.Resource, decision.GritCost);
                    spent = true;
                }
                var store = new FirearmItemRepairStateStore(FirearmRuntimeState.Service,
                    firearm.Weapon);
                before = store.Read();
                store.Replace(before, FirearmStateMachine.Repair(before));
                QuickClearRuntimeDiagnostics.Record(decision);
                return decision;
            }
            catch
            {
                if (before != null && firearm != null)
                {
                    FirearmState current = FirearmRuntimeState.Service
                        .GetOrCreate(firearm.Weapon).Repository.State;
                    if (current != before)
                        FirearmRuntimeState.Service.Set(firearm.Weapon, before);
                }
                if (spent) caster.Resources.Restore(gunslinger.Grit.Resource,
                    decision.GritCost);
                QuickClearRuntimeDiagnostics.RecordFault();
                throw;
            }
        }

        private static int ReadGrit(UnitDescriptor caster)
        {
            GunslingerClassBlueprintSet gunslinger = BlueprintBootstrap.GunslingerClass;
            return caster == null || gunslinger == null ? 0 :
                caster.Resources.GetResourceAmount(gunslinger.Grit.Resource);
        }
    }
}
