using System;
using System.Runtime.CompilerServices;
using System.Linq;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Diagnostics;
using KingmakerGunslinger.Misfires;
using KingmakerGunslinger.Blueprints;

namespace KingmakerGunslinger.Deeds
{
    internal static class DeadeyeRuntime
    {
        private static readonly DeadeyeService Service = new DeadeyeService();
        private static readonly ConditionalWeakTable<RuleAttackRoll, DeadeyeDecision>
            Decisions = new ConditionalWeakTable<RuleAttackRoll, DeadeyeDecision>();

        internal static void BeforeAttackRoll(RuleAttackRoll attackRoll)
        {
            if (DeadShotRuntime.IsProbe(attackRoll)) return;
            if (attackRoll == null) return;
            try
            {
                DeadeyeBlueprintSet blueprints = BlueprintBootstrap.Deadeye;
                UnitEntityData initiator = attackRoll.Initiator;
                UnitEntityData target = attackRoll.Target;
                if (blueprints == null || initiator == null || target == null ||
                    initiator.Descriptor == null) return;
                UnitDescriptor descriptor = initiator.Descriptor;
                if (descriptor.HasFact(blueprints.ArmedMarker))
                    descriptor.RemoveFact(blueprints.ArmedMarker);
                var armedBuff = descriptor.Buffs.RawFacts.FirstOrDefault(value =>
                    ReferenceEquals(value.Blueprint, blueprints.ArmedBuff));
                bool armed = armedBuff != null;
                if (!armed) return;

                FirearmMarkerSnapshot marker = FirearmMarkerLookup.ReadFromRuleEvent(attackRoll);
                int grit = descriptor.Resources.GetResourceAmount(
                    BlueprintBootstrap.GunslingerClass.Grit.Resource);
                DeadeyeDecision decision = Service.Evaluate(new DeadeyeRequest(
                    true, marker.IsExactFirearm, marker.MarkerCount, marker.Definition,
                    initiator.DistanceTo(target), int.MaxValue,
                    Rules.EffectiveFirearmRangeRuntime.GetBonusFeet(attackRoll)));

                // The native persisted marker applies to the next firearm attack only.
                // Non-firearm actions leave it armed; any exact firearm shot consumes it.
                if (!marker.IsExactFirearm) return;
                if (!FirearmMisfireRuntime.IsEligibleAttack(attackRoll)) return;
                descriptor.Buffs.RemoveFact(armedBuff);
                if (decision.UsesTouchArmorClass)
                {
                    lock (Decisions) { Decisions.Add(attackRoll, decision); }
                }
                DeadeyeRuntimeDiagnostics.Record(decision);
            }
            catch (Exception exception)
            {
                DeadeyeRuntimeDiagnostics.RecordFault(exception);
                ModContext context;
                if (ModContext.TryGet(out context))
                    context.Logger.Failure("deadeye", "attack.failed",
                        "Deadeye failed closed before firearm AC selection.", exception);
            }
        }

        internal static bool IsAuthorized(RuleAttackRoll attackRoll)
        {
            if (attackRoll == null) return false;
            lock (Decisions)
            {
                DeadeyeDecision decision;
                return Decisions.TryGetValue(attackRoll, out decision) &&
                    decision.UsesTouchArmorClass;
            }
        }
    }
}
