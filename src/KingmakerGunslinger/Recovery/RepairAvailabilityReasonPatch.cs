using System.Linq;
using Harmony12;
using Kingmaker.UI.UnitSettings;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Components.Base;

namespace KingmakerGunslinger.Recovery
{
    /// <summary>
    /// Native GetUnavailableReason calls IsAvailableFor(this), then the
    /// provider's parameterless GetReason. Recover the concrete AbilityData
    /// here, without caching a caster or failure on the shared blueprint.
    /// Native checks that precede providers (such as a staggered full-round
    /// action) also receive this ability's brief contextual fallback.
    /// Availability, other abilities and the global notification UI are unchanged.
    /// </summary>
    [HarmonyPatch(typeof(AbilityData), "GetUnavailableReason")]
    internal static class RepairAvailabilityReasonPatch
    {
        private static void Postfix(AbilityData __instance, ref string __result)
        {
            if (__instance == null || __instance.Blueprint == null) return;
            var repair = __instance.Blueprint.ComponentsArray
                .OfType<RepairTestMusketAbilityLogic>().FirstOrDefault();
            if (repair != null) __result = GetContextualReason(repair, __instance);
        }
        // Native action-bar turn-economy warnings precede AbilityData's
        // provider route. Scope the same reason to this repair slot only;
        // availability and the notification publisher remain native.
        [HarmonyPatch(typeof(MechanicActionBarSlotAbility), "WarningMessage")]
        private static class RepairSlotReasonPatch
        {
            private static void Postfix(MechanicActionBarSlotAbility __instance, ref string __result)
            {
                if (__instance != null)
                    RepairAvailabilityReasonPatch.Postfix(__instance.Ability, ref __result);
            }
        }
        private static string GetContextualReason(
            IAbilityAvailabilityProvider provider, AbilityData ability)
        {
            RepairTestMusketAbilityLogic repair = provider as RepairTestMusketAbilityLogic;
            return repair == null ? provider.GetReason() : repair.GetReasonFor(ability);
        }
    }
}
