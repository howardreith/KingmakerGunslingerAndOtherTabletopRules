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
    /// Rejects an empty firearm at UnitCommands.Run, before OnStart configures an
    /// animation or TriggerAttackRule constructs RuleAttackWithWeapon/RuleAttackRoll.
    /// </summary>
    internal static class EmptyFirearmAttackCommandPatch
    {
        private static readonly ConditionalWeakTable<UnitAttack, ReportMarker>
            Reported = new ConditionalWeakTable<UnitAttack, ReportMarker>();
        private static long _rejected;
        private static long _autoReloadReplacements;
        private static long _evaluatedAttacks;
        internal static long Rejected { get { return Interlocked.Read(ref _rejected); } }
        internal static long AutoReloadReplacements
        { get { return Interlocked.Read(ref _autoReloadReplacements); } }
        internal static long EvaluatedAttacks
        { get { return Interlocked.Read(ref _evaluatedAttacks); } }

        internal static void Install(HarmonyInstance harmony)
        {
            if (harmony == null) throw new ArgumentNullException("harmony");
            MethodInfo prefix = typeof(EmptyFirearmAttackCommandPatch).GetMethod(
                "Prefix", BindingFlags.NonPublic | BindingFlags.Static);
            MethodInfo run = typeof(UnitCommands).GetMethod("Run",
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null,
                new[] { typeof(UnitCommand) }, null);
            MethodInfo runDetailed = typeof(UnitCommands).GetMethod("Run",
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null,
                new[] { typeof(UnitCommand), typeof(bool), typeof(bool) }, null);
            if (run == null || runDetailed == null || prefix == null)
                throw new MissingMethodException(
                    "Exact UnitCommands.Run empty-firearm patch contract was unavailable.");
            harmony.Patch(run, new HarmonyMethod(prefix), null, null);
            harmony.Patch(runDetailed, new HarmonyMethod(prefix), null, null);
        }

        private static bool Prefix(UnitCommands __instance, UnitCommand __0)
        {
            UnitAttack attack = __0 as UnitAttack;
            if (attack == null || attack.Executor == null ||
                attack.Executor.Descriptor == null) return true;
            Interlocked.Increment(ref _evaluatedAttacks);
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
                        __instance);
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
            return Reject(attack, disposition, message, __instance);
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
            UnitCommands commands)
        {
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
                if (disposition == EmptyFirearmCommandDisposition.QueueReload &&
                    commands != null && command.Executor != null)
                {
                    var reload = command.Executor.GetAvailableAutoUseAbility();
                    if (reload != null)
                        commands.Run(new UnitUseAbility(reload,
                            new Kingmaker.Utility.TargetWrapper(command.Executor)));
                }
            }
            return false;
        }

        private sealed class ReportMarker { }
    }
}
