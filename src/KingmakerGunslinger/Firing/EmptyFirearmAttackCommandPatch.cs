using System;
using System.Runtime.CompilerServices;
using Harmony12;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using KingmakerGunslinger.Actions;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Reloading;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace KingmakerGunslinger.Firing
{
    /// <summary>
    /// Rejects an empty firearm at UnitAttack.CanStart, before OnStart configures an
    /// animation or TriggerAttackRule constructs RuleAttackWithWeapon/RuleAttackRoll.
    /// </summary>
    [HarmonyPatch]
    internal static class EmptyFirearmAttackCommandPatch
    {
        private static MethodBase _target;
        private static readonly ConditionalWeakTable<UnitAttack, ReportMarker>
            Reported = new ConditionalWeakTable<UnitAttack, ReportMarker>();
        private static long _rejected;
        private static long _autoReloadReplacements;
        internal static long Rejected { get { return Interlocked.Read(ref _rejected); } }
        internal static long AutoReloadReplacements
        { get { return Interlocked.Read(ref _autoReloadReplacements); } }

        private static bool Prepare()
        {
            PropertyInfo property = typeof(UnitCommand).GetProperty("CanStart",
                BindingFlags.Public | BindingFlags.NonPublic |
                BindingFlags.Instance | BindingFlags.DeclaredOnly);
            _target = property == null ? null : property.GetGetMethod(true);
            return _target != null;
        }

        private static MethodBase TargetMethod()
        {
            return _target;
        }

        private static bool Prefix(UnitCommand __instance, ref bool __result)
        {
            UnitAttack attack = __instance as UnitAttack;
            if (attack == null || attack.Executor == null ||
                attack.Executor.Descriptor == null) return true;
            ExactEquippedFirearmContext firearm;
            string reason;
            if (!ExactEquippedFirearmResolver.TryResolve(
                attack.Executor.Descriptor, out firearm, out reason))
            {
                if (reason != null && reason.IndexOf("ambiguous",
                    StringComparison.OrdinalIgnoreCase) >= 0)
                    return Reject(attack,
                        EmptyFirearmCommandDisposition.RejectAmbiguous,
                        "Firearm attack rejected: equipped firearms are ambiguous.",
                        ref __result);
                return true;
            }
            bool autoReload = IsReloadAutoUse(attack);
            bool reloadLegal = autoReload &&
                attack.Executor.GetAvailableAutoUseAbility() != null;
            EmptyFirearmCommandDisposition disposition =
                EmptyFirearmAttackPolicy.Evaluate(true, false,
                    firearm.Firearm.Repository.State, autoReload, reloadLegal);
            if (disposition == EmptyFirearmCommandDisposition.Allow) return true;
            string message = disposition == EmptyFirearmCommandDisposition.QueueReload
                ? firearm.Firearm.ItemDisplayName + " is unloaded; auto-reload will run."
                : disposition == EmptyFirearmCommandDisposition.RejectWrecked
                ? firearm.Firearm.ItemDisplayName + " is Wrecked."
                : firearm.Firearm.ItemDisplayName + " is unloaded.";
            return Reject(attack, disposition, message, ref __result);
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
                Interlocked.Increment(ref _rejected);
                if (disposition == EmptyFirearmCommandDisposition.QueueReload)
                    Interlocked.Increment(ref _autoReloadReplacements);
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
