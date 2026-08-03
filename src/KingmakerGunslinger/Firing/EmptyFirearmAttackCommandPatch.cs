using System;
using Harmony12;
using Kingmaker.EntitySystem.Entities;
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
    /// Rejects an empty firearm while UnitAttack.CreateAttackCommand is constructing
    /// the command, before a UnitAttack, animation, or attack rule exists.
    /// </summary>
    internal static class EmptyFirearmAttackCommandPatch
    {
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
            MethodInfo create = typeof(UnitAttack).GetMethod("CreateAttackCommand",
                BindingFlags.Public | BindingFlags.Static, null,
                new[] { typeof(UnitEntityData), typeof(UnitEntityData) }, null);
            if (create == null || prefix == null)
                throw new MissingMethodException(
                    "Exact UnitAttack.CreateAttackCommand contract was unavailable.");
            harmony.Patch(create, new HarmonyMethod(prefix), null, null);
        }

        private static bool Prefix(UnitEntityData __0, UnitEntityData __1,
            ref UnitCommand __result)
        {
            UnitEntityData executor = __0;
            if (executor == null || executor.Descriptor == null) return true;
            Interlocked.Increment(ref _evaluatedAttacks);
            ExactEquippedFirearmContext firearm;
            string reason;
            if (!ExactEquippedFirearmResolver.TryResolve(
                executor.Descriptor, out firearm, out reason))
            {
                if (reason != null && reason.IndexOf("ambiguous",
                    StringComparison.OrdinalIgnoreCase) >= 0)
                    return Reject(executor, __1,
                        EmptyFirearmCommandDisposition.RejectAmbiguous,
                        "Firearm attack rejected: equipped firearms are ambiguous.",
                        ref __result);
                return true;
            }
            bool autoReload = IsReloadAutoUse(executor);
            bool reloadLegal = autoReload &&
                executor.GetAvailableAutoUseAbility() != null;
            EmptyFirearmCommandDisposition disposition =
                EmptyFirearmAttackPolicy.Evaluate(true, false,
                    firearm.Firearm.Repository.State, autoReload, reloadLegal);
            if (disposition == EmptyFirearmCommandDisposition.Allow) return true;
            string message = disposition == EmptyFirearmCommandDisposition.QueueReload
                ? firearm.Firearm.ItemDisplayName + " is unloaded; auto-reload will run."
                : disposition == EmptyFirearmCommandDisposition.RejectWrecked
                ? firearm.Firearm.ItemDisplayName + " is Wrecked."
                : firearm.Firearm.ItemDisplayName + " is unloaded.";
            return Reject(executor, __1, disposition, message, ref __result);
        }

        private static bool IsReloadAutoUse(UnitEntityData executor)
        {
            if (executor == null || executor.AutoUseAbility == null ||
                BlueprintBootstrap.ReloadTestMusketAbility == null) return false;
            var variants = BlueprintBootstrap.ReloadTestMusketAbility
                .ComponentsArray.OfType<Kingmaker.UnitLogic.Abilities.Components.AbilityVariants>()
                .Single().Variants;
            return variants.Any(value => ReferenceEquals(value,
                executor.AutoUseAbility.Blueprint));
        }

        private static bool Reject(UnitEntityData executor, UnitEntityData target,
            EmptyFirearmCommandDisposition disposition, string message,
            ref UnitCommand result)
        {
            Interlocked.Increment(ref _rejected);
            if (disposition == EmptyFirearmCommandDisposition.QueueReload)
                Interlocked.Increment(ref _autoReloadReplacements);
            ModContext context;
            if (ModContext.TryGet(out context))
                context.Logger.Info("firearms", "attack.command-rejected",
                    message + " disposition=" + disposition);
            result = null;
            if (disposition == EmptyFirearmCommandDisposition.QueueReload &&
                executor != null)
            {
                var reload = executor.GetAvailableAutoUseAbility();
                if (reload != null)
                    result = new UnitUseAbility(reload,
                        new Kingmaker.Utility.TargetWrapper(executor));
            }
            return false;
        }
    }
}
