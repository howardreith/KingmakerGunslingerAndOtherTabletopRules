using System;
using System.Reflection;
using System.Threading;
using Harmony12;
using Kingmaker.Items;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using KingmakerGunslinger.Actions;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Firearms;
using KingmakerGunslinger.Reloading;
using KingmakerGunslinger.Deeds;

namespace KingmakerGunslinger.Firing
{
    /// <summary>
    /// Extends Kingmaker's native full-attack command immediately before each
    /// iterative attack.  At this point Kingmaker has already selected any
    /// replacement target and exposed the exact next PlannedAttack, while
    /// LastAttackRule still identifies the previous completed shot.
    ///
    /// The existing atomic reload transaction runs only when the same exact
    /// firearm is empty and its effective reload action is genuinely Free.
    /// Otherwise the remaining full attack ends before an empty firearm can
    /// create a harmless projectile or consume unrelated action economy.
    /// </summary>
    internal static class FreeActionFullAttackReloadPatch
    {
        private static long _attempted;
        private static long _succeeded;
        private static long _interrupted;
        private static long _unavailable;
        private static long _failed;

        internal static long Attempted { get { return Interlocked.Read(ref _attempted); } }
        internal static long Succeeded { get { return Interlocked.Read(ref _succeeded); } }
        internal static long Interrupted { get { return Interlocked.Read(ref _interrupted); } }
        internal static long Unavailable { get { return Interlocked.Read(ref _unavailable); } }
        internal static long Failed { get { return Interlocked.Read(ref _failed); } }

        internal static void Install(HarmonyInstance harmony)
        {
            if (harmony == null) throw new ArgumentNullException("harmony");
            MethodInfo target = typeof(UnitAttack).GetMethod("OnAction",
                BindingFlags.Instance | BindingFlags.NonPublic, null,
                Type.EmptyTypes, null);
            MethodInfo prefix = typeof(FreeActionFullAttackReloadPatch).GetMethod(
                "Prefix", BindingFlags.Static | BindingFlags.NonPublic);
            if (target == null || target.ReturnType !=
                    typeof(UnitCommand.ResultType) || prefix == null)
                throw new MissingMethodException(
                    "Exact UnitAttack.OnAction full-attack boundary was unavailable.");
            harmony.Patch(target, new HarmonyMethod(prefix), null, null);
        }

        private static bool Prefix(UnitAttack __instance,
            ref UnitCommand.ResultType __result)
        {
            if (__instance == null || !__instance.IsFullAttack)
                return true;

            bool recognizedFirearm = false;
            try
            {
                RuleAttackWithWeapon previous = __instance.LastAttackRule;
                AttackHandInfo planned = __instance.PlannedAttack;
                ItemEntityWeapon previousWeapon = previous == null
                    ? null
                    : previous.Weapon;
                ItemEntityWeapon plannedWeapon = planned == null
                    ? null
                    : planned.Weapon;
                bool sameWeapon = previousWeapon != null &&
                    ReferenceEquals(previousWeapon, plannedWeapon);

                ExactEquippedFirearmContext firearm = null;
                string reason;
                bool exactEquipped = plannedWeapon != null &&
                    __instance.Executor != null &&
                    __instance.Executor.Descriptor != null &&
                    ExactEquippedFirearmResolver.TryResolve(
                        __instance.Executor.Descriptor, out firearm, out reason) &&
                    ReferenceEquals(firearm.Weapon, plannedWeapon);
                if (!exactEquipped) return true;
                recognizedFirearm = true;

                FirearmState currentState = firearm.Firearm.Repository.State;
                ReloadTestMusketAvailability availability = null;
                EffectiveReloadAction action = EffectiveReloadAction.Unknown;
                bool normalAvailable = !currentState.IsEmpty;
                if (currentState.IsEmpty)
                {
                    availability = ReloadTestMusketRuntime.Evaluate(
                        __instance.Executor.Descriptor, firearm.Weapon.Blueprint,
                        BlueprintBootstrap.BasicAmmunition.BlackPowder,
                        BlueprintBootstrap.BasicAmmunition.LeadBall);
                    normalAvailable = availability.IsAvailable;
                    if (availability.Plan != null) action = availability.Plan.Action;
                }
                bool exactAutoUse = __instance.Executor.AutoUseAbility != null &&
                    ReferenceEquals(__instance.Executor.AutoUseAbility.Blueprint,
                        BlueprintBootstrap.ReloadTestMusketAbility);
                LightningReloadBlueprintSet lightning =
                    BlueprintBootstrap.GunslingerClass.LightningReload;
                bool lightningGranted = lightning != null &&
                    __instance.Executor.Descriptor.Abilities.GetAbility(
                        lightning.Ability) != null;
                LightningReloadAvailability lightningAvailability =
                    currentState.IsEmpty && normalAvailable &&
                    action != EffectiveReloadAction.Free && lightningGranted
                    ? LightningReloadRuntime.Evaluate(
                        __instance.Executor.Descriptor,
                        BlueprintBootstrap.BasicAmmunition.BlackPowder,
                        BlueprintBootstrap.BasicAmmunition.LeadBall,
                        lightning.UsedMarker) : null;
                bool freeLightning = lightningAvailability != null &&
                    lightningAvailability.Decision.IsAvailable &&
                    lightningAvailability.Decision.Action == LightningReloadAction.Free;
                bool targetAlive = __instance.Target != null &&
                    __instance.Target.Descriptor != null &&
                    __instance.Target.Descriptor.State != null &&
                    !__instance.Target.Descriptor.State.IsDead;
                FullAttackReloadDecision decision =
                    FullAttackAutoReloadPolicy.Evaluate(
                        true,
                        previous != null,
                        planned != null,
                        sameWeapon,
                        targetAlive,
                        exactAutoUse,
                        normalAvailable,
                        action,
                        freeLightning,
                        currentState,
                        firearm.EffectiveCondition);

                if (decision == FullAttackReloadDecision.None ||
                    decision == FullAttackReloadDecision.ContinueLoaded)
                    return true;

                if (decision == FullAttackReloadDecision.EndFullAttack)
                {
                    EndRemainingAttacks(ref __result,
                        "full-attack.ended-before-empty-shot",
                        "weapon=" + firearm.Firearm.ItemDisplayName +
                        ";kind=" + firearm.Definition.Kind +
                        ";condition=" + firearm.EffectiveCondition +
                        ";reloadAction=" + action);
                    return false;
                }

                Interlocked.Increment(ref _attempted);
                if (!availability.IsAvailable)
                {
                    Interlocked.Increment(ref _unavailable);
                    EndRemainingAttacks(ref __result,
                        "full-attack.reload-unavailable",
                        "weapon=" + firearm.Firearm.ItemDisplayName +
                        ";kind=" + firearm.Definition.Kind +
                        ";reason=" + availability.Reason);
                    return false;
                }

                FirearmReloadResult result = decision ==
                    FullAttackReloadDecision.LightningReload
                    ? LightningReloadRuntime.ExecuteInline(__instance.Executor,
                        BlueprintBootstrap.BasicAmmunition.BlackPowder,
                        BlueprintBootstrap.BasicAmmunition.LeadBall,
                        lightning.UsedMarker)
                    : ReloadTestMusketRuntime.Execute(
                        __instance.Executor.Descriptor,
                        firearm.Weapon.Blueprint,
                        BlueprintBootstrap.BasicAmmunition.BlackPowder,
                        BlueprintBootstrap.BasicAmmunition.LeadBall);
                if (!result.Succeeded)
                    throw new InvalidOperationException(
                        "Free-action full-attack reload was rejected: " + result);

                Interlocked.Increment(ref _succeeded);
                Log("full-attack.reload-applied",
                    "weapon=" + firearm.Firearm.ItemDisplayName +
                    ";kind=" + firearm.Definition.Kind +
                    ";rounds=" + result.RoundsLoaded +
                    ";action=" + action +
                    ";source=" + (decision == FullAttackReloadDecision.LightningReload
                        ? "lightning" : "normal") +
                    ";nextAttackContinues=true");
                return true;
            }
            catch (Exception exception)
            {
                Interlocked.Increment(ref _failed);
                ModContext context;
                if (ModContext.TryGet(out context))
                    context.Logger.Failure("reload",
                        "full-attack.reload-failed",
                        "The free reload before an iterative firearm attack failed; the remaining full attack ended before an empty shot.",
                        exception);
                if (recognizedFirearm)
                {
                    __result = UnitCommand.ResultType.Success;
                    Interlocked.Increment(ref _interrupted);
                    return false;
                }
                return true;
            }
        }

        private static void EndRemainingAttacks(
            ref UnitCommand.ResultType result, string eventName, string detail)
        {
            result = UnitCommand.ResultType.Success;
            Interlocked.Increment(ref _interrupted);
            Log(eventName, detail + ";remainingFullAttackEnded=true");
        }

        private static void Log(string eventName, string message)
        {
            ModContext context;
            if (ModContext.TryGet(out context))
                context.Logger.Info("reload", eventName, message);
        }
    }
}
