using System.Runtime.CompilerServices;
using System.Threading;
using Harmony12;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.Utility;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Commands.Base;
using KingmakerGunslinger.Bootstrap;

namespace KingmakerGunslinger.Reloading
{
    internal static class ReloadAbilityPresentation
    {
        private static int _paperModeRevision;

        internal static int PaperModeRevision
        { get { return Interlocked.CompareExchange(ref _paperModeRevision, 0, 0); } }

        internal static void InvalidatePaperMode()
        { Interlocked.Increment(ref _paperModeRevision); }

        internal static bool TryAction(AbilityData ability,
            out EffectiveReloadAction action)
        {
            ReloadTestMusketAvailability availability;
            return TryAction(ability, out action, out availability);
        }

        internal static bool TryAction(AbilityData ability,
            out EffectiveReloadAction action,
            out ReloadTestMusketAvailability availability)
        {
            action = EffectiveReloadAction.Unknown;
            availability = null;
            if (ability == null || ability.Caster == null ||
                !ReferenceEquals(ability.Blueprint,
                    BlueprintBootstrap.ReloadTestMusketAbility)) return false;
            availability = ReloadTestMusketRuntime.Evaluate(ability.Caster,
                BlueprintBootstrap.ProductionFirearms.Musket.Item,
                BlueprintBootstrap.BasicAmmunition.BlackPowder,
                BlueprintBootstrap.BasicAmmunition.LeadBall);
            if (!availability.IsAvailable || availability.Plan == null)
                return false;
            action = availability.Plan.Action;
            return action != EffectiveReloadAction.Unknown;
        }

        internal static UnitCommand.CommandType Command(
            EffectiveReloadAction action)
        {
            return action == EffectiveReloadAction.Free ?
                UnitCommand.CommandType.Free :
                action == EffectiveReloadAction.Move ?
                UnitCommand.CommandType.Move : UnitCommand.CommandType.Standard;
        }
    }

    /// <summary>
    /// Binds the action selected while a reload command is queued. Delivery
    /// cancels rather than mixing action economy and ammunition when the user
    /// changes the paper-cartridge mode before that command executes.
    /// </summary>
    internal static class ReloadQueuedPlanBinding
    {
        private static readonly ConditionalWeakTable<AbilityData, Binding>
            Bindings = new ConditionalWeakTable<AbilityData, Binding>();

        internal static void Bind(AbilityData ability, FirearmReloadPlan plan)
        {
            if (ability == null || plan == null || plan.Profile == null) return;
            Bindings.Remove(ability);
            Bindings.Add(ability, new Binding(plan.Profile, plan.Action,
                ReloadAbilityPresentation.PaperModeRevision));
        }

        internal static bool IsCurrent(AbilityData ability,
            FirearmReloadPlan plan)
        {
            if (ability == null || plan == null || plan.Profile == null)
                return false;
            Binding binding;
            if (!Bindings.TryGetValue(ability, out binding)) return true;
            return binding.Revision == ReloadAbilityPresentation
                .PaperModeRevision && ReferenceEquals(binding.Profile,
                    plan.Profile) && binding.Action == plan.Action;
        }

        internal static void Forget(AbilityData ability)
        { if (ability != null) Bindings.Remove(ability); }

        private sealed class Binding
        {
            internal Binding(object profile, EffectiveReloadAction action,
                int revision)
            {
                Profile = profile;
                Action = action;
                Revision = revision;
            }

            internal object Profile { get; private set; }
            internal EffectiveReloadAction Action { get; private set; }
            internal int Revision { get; private set; }
        }
    }

    // The two-argument convenience constructor chains into this authoritative
    // constructor. Preserve the granted AbilityData and alter only the action
    // argument consumed by UnitUseAbility.
    [HarmonyPatch(typeof(UnitUseAbility), MethodType.Constructor,
        typeof(UnitCommand.CommandType), typeof(AbilityData), typeof(TargetWrapper))]
    internal static class ReloadAbilityCommandTypePatch
    {
        private static void Prefix(ref UnitCommand.CommandType __0, AbilityData __1)
        {
            EffectiveReloadAction action;
            ReloadTestMusketAvailability availability;
            if (ReloadAbilityPresentation.TryAction(__1, out action,
                    out availability))
            {
                __0 = ReloadAbilityPresentation.Command(action);
                ReloadQueuedPlanBinding.Bind(__1, availability.Plan);
            }
        }
    }

    [HarmonyPatch(typeof(AbilityData), "get_ActionType")]
    internal static class ReloadAbilityActionTypePatch
    {
        private static void Postfix(AbilityData __instance,
            ref UnitCommand.CommandType __result)
        {
            EffectiveReloadAction action;
            if (ReloadAbilityPresentation.TryAction(__instance, out action))
                __result = ReloadAbilityPresentation.Command(action);
        }
    }

    [HarmonyPatch(typeof(AbilityData), "get_RuntimeActionType")]
    internal static class ReloadAbilityRuntimeActionTypePatch
    {
        private static void Postfix(AbilityData __instance,
            ref UnitCommand.CommandType __result)
        {
            EffectiveReloadAction action;
            if (ReloadAbilityPresentation.TryAction(__instance, out action))
                __result = ReloadAbilityPresentation.Command(action);
        }
    }

    [HarmonyPatch(typeof(AbilityData), "get_RequireFullRoundAction")]
    internal static class ReloadAbilityFullRoundPatch
    {
        private static void Postfix(AbilityData __instance, ref bool __result)
        {
            EffectiveReloadAction action;
            if (ReloadAbilityPresentation.TryAction(__instance, out action))
                __result = action == EffectiveReloadAction.FullRound;
        }
    }
}
