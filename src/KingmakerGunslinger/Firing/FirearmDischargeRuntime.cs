using System;
using Kingmaker.RuleSystem.Rules;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Diagnostics;
using KingmakerGunslinger.Firearms;
using KingmakerGunslinger.Misfires;
using KingmakerGunslinger.Gunsmithing;
using Kingmaker.Items;

namespace KingmakerGunslinger.Firing
{
    /// <summary>
    /// Kingmaker adapter that consumes one round at the beginning of the exact
    /// RuleAttackRoll for a marked firearm. Empty, wrecked, or untrustworthy firearms
    /// are forced to miss without touching inventory ammunition.
    /// </summary>
    internal static class FirearmDischargeRuntime
    {
        private static readonly ReferenceEventGate EventGate = new ReferenceEventGate();
        private static readonly FirearmDischargeService Service = new FirearmDischargeService();

        internal static void BeforeAttackRoll(object ruleEvent)
        {
            if (ruleEvent == null)
            {
                return;
            }

            RuleAttackRoll attackRoll = ruleEvent as RuleAttackRoll;
            if (attackRoll == null)
            {
                FirearmDischargeRuntimeDiagnostics.RecordIgnored(
                    "The patched rule event was not a RuleAttackRoll.");
                return;
            }

            if (Deeds.DeadShotRuntime.ShouldBypassDischarge(attackRoll) ||
                Scatter.ScatterVolleyRuntime.ShouldBypassOrdinaryDischarge(attackRoll))
            {
                return;
            }

            if (!EventGate.TryMark(ruleEvent))
            {
                FirearmDischargeRuntimeDiagnostics.RecordDuplicate();
                return;
            }

            bool recognizedFirearm = false;
            try
            {
                FirearmMarkerSnapshot marker = FirearmMarkerLookup.ReadFromRuleEvent(ruleEvent);
                if (!marker.IsExactFirearm)
                {
                    FirearmDischargeRuntimeDiagnostics.RecordIgnored(
                        marker.HasWeapon
                            ? "The concrete weapon did not contain exactly one firearm marker."
                            : "No concrete weapon was available on the attack roll.");
                    return;
                }

                recognizedFirearm = true;
                object weapon;
                if (!FirearmMarkerLookup.TryResolveWeapon(ruleEvent, out weapon) || weapon == null)
                {
                    throw new InvalidOperationException(
                        "A marked firearm attack did not expose its concrete runtime weapon item.");
                }

                FirearmItemStateSnapshot before;
                string rejection;
                if (!FirearmRuntimeState.Service.TryGetOrCreate(
                    weapon,
                    out before,
                    out rejection))
                {
                    throw new InvalidOperationException(rejection);
                }

                ItemEntityWeapon exactWeapon = weapon as ItemEntityWeapon;
                if (exactWeapon == null)
                    throw new InvalidOperationException(
                        "The exact marked firearm was not an ItemEntityWeapon.");
                BatteredFirearmUseDecision use =
                    new BatteredFirearmRuntimeUseResolver().Evaluate(
                        exactWeapon, attackRoll.Initiator,
                        before.Repository.State.Condition, 0);
                FirearmDischargeResult result = Service.Evaluate(
                    before.Repository.State, use.EffectiveCondition);
                if (result.Status == FirearmDischargeStatus.Fired)
                {
                    FirearmItemStateSnapshot after = FirearmRuntimeState.Service.Transition(
                        weapon,
                        current =>
                        {
                            if (current != result.Before)
                            {
                                throw new InvalidOperationException(
                                    "The firearm state changed between discharge inspection and commit.");
                            }

                            return result.After;
                        });
                    if (after.Repository.State != result.After)
                    {
                        throw new InvalidOperationException(
                            "The loaded-round discharge transition did not verify after commit.");
                    }

                    if (!FirearmMisfireRuntime.TryRegisterEligibleAttack(
                            attackRoll,
                            weapon,
                            after,
                            result.EffectiveCondition,
                            before.Repository.State.LoadedAmmunition))
                    {
                        // The round has already discharged. Fail the unresolved attack
                        // closed rather than allowing a hit without misfire evaluation.
                        ForceMiss(attackRoll);
                    }
                }
                else
                {
                    ForceMiss(attackRoll);
                }

                FirearmDischargeRuntimeDiagnostics.Record(
                    result,
                    before.ItemDisplayName);
                LogDecision(result, before);
            }
            catch (Exception exception)
            {
                if (recognizedFirearm)
                {
                    ForceMiss(attackRoll);
                }

                FirearmDischargeRuntimeDiagnostics.RecordFault(
                    exception,
                    recognizedFirearm);
                LogFault(exception, recognizedFirearm);
            }
        }

        private static void ForceMiss(RuleAttackRoll attackRoll)
        {
            if (attackRoll == null)
            {
                return;
            }

            // RuleAttackRoll evaluates AutoHit before AutoMiss. Clearing AutoHit is
            // therefore required for an empty firearm to fail even on auto-hit attacks.
            attackRoll.AutoHit = false;
            attackRoll.AutoMiss = true;
        }

        private static void LogDecision(
            FirearmDischargeResult result,
            FirearmItemStateSnapshot firearm)
        {
            ModContext context;
            if (!ModContext.TryGet(out context))
            {
                return;
            }

            context.Logger.Info(
                "firearms",
                result.Status == FirearmDischargeStatus.Fired
                    ? "attack.round-consumed"
                    : "attack.forced-miss",
                result + "; item=" + firearm.ItemDisplayName +
                "; itemBlueprint=" + firearm.ItemBlueprintName);
        }

        private static void LogFault(Exception exception, bool recognizedFirearm)
        {
            ModContext context;
            if (ModContext.TryGet(out context))
            {
                context.Logger.Failure(
                    "firearms",
                    "attack.enforcement-failed",
                    recognizedFirearm
                        ? "Loaded-round attack enforcement failed for a marked firearm; the attack was forced to miss."
                        : "Attack enforcement inspection failed before a firearm was established; the native attack was left unchanged.",
                    exception);
            }
        }
    }
}
