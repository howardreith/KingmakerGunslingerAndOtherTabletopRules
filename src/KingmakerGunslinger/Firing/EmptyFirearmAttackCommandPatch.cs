using System;
using System.Runtime.CompilerServices;
using Harmony12;
using Kingmaker.UnitLogic.Commands;
using KingmakerGunslinger.Actions;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Reloading;
using System.Linq;

namespace KingmakerGunslinger.Firing
{
    /// <summary>
    /// Rejects an empty firearm at UnitAttack.CanStart, before OnStart configures an
    /// animation or TriggerAttackRule constructs RuleAttackWithWeapon/RuleAttackRoll.
    /// </summary>
    [HarmonyPatch(typeof(UnitAttack), "get_CanStart")]
    internal static class EmptyFirearmAttackCommandPatch
    {
        private static readonly ConditionalWeakTable<UnitAttack, ReportMarker>
            Reported = new ConditionalWeakTable<UnitAttack, ReportMarker>();

        private static bool Prefix(UnitAttack __instance, ref bool __result)
        {
            if (__instance == null || __instance.Executor == null ||
                __instance.Executor.Descriptor == null) return true;
            ExactEquippedFirearmContext firearm;
            string reason;
            if (!ExactEquippedFirearmResolver.TryResolve(
                __instance.Executor.Descriptor, out firearm, out reason))
            {
                if (reason != null && reason.IndexOf("ambiguous",
                    StringComparison.OrdinalIgnoreCase) >= 0)
                    return Reject(__instance,
                        EmptyFirearmCommandDisposition.RejectAmbiguous,
                        "Firearm attack rejected: equipped firearms are ambiguous.",
                        ref __result);
                return true;
            }
            bool autoReload = IsReloadAutoUse(__instance);
            bool reloadLegal = autoReload &&
                __instance.Executor.GetAvailableAutoUseAbility() != null;
            EmptyFirearmCommandDisposition disposition =
                EmptyFirearmAttackPolicy.Evaluate(true, false,
                    firearm.Firearm.Repository.State, autoReload, reloadLegal);
            if (disposition == EmptyFirearmCommandDisposition.Allow) return true;
            string message = disposition == EmptyFirearmCommandDisposition.QueueReload
                ? firearm.Firearm.ItemDisplayName + " is unloaded; auto-reload will run."
                : disposition == EmptyFirearmCommandDisposition.RejectWrecked
                ? firearm.Firearm.ItemDisplayName + " is Wrecked."
                : firearm.Firearm.ItemDisplayName + " is unloaded.";
            return Reject(__instance, disposition, message, ref __result);
        }

        private static bool IsReloadAutoUse(UnitAttack command)
        {
            if (command == null || command.Executor == null ||
                command.Executor.AutoUseAbility == null ||
                BlueprintBootstrap.ReloadTestMusketAbility == null) return false;
            var variants = BlueprintBootstrap.ReloadTestMusketAbility
                .ComponentsArray.OfType<Kingmaker.UnitLogic.Abilities.Components.AbilityVariants>()
                .Single().Variants;
            return variants.Any(value => ReferenceEquals(value,
                command.Executor.AutoUseAbility.Blueprint));
        }

        private static bool Reject(UnitAttack command,
            EmptyFirearmCommandDisposition disposition, string message,
            ref bool result)
        {
            result = false;
            if (!Reported.TryGetValue(command, out _))
            {
                Reported.Add(command, new ReportMarker());
                ModContext context;
                if (ModContext.TryGet(out context))
                    context.Logger.Info("firearms", "attack.command-rejected",
                        message + " disposition=" + disposition);
            }
            return false;
        }

        private sealed class ReportMarker { }
    }
}
