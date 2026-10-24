using System;
using System.Linq;
using Harmony12;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.UnitLogic.FactLogic;

namespace KingmakerGunslinger.Summoning
{
    // Native SubtypeExtraplanar is the exact identity consumed by Dismissal
    // and Banishment, but also grants SR5+CR, which PF1 extraplanar status does
    // not grant. Keep the native fact, excluding only its SR on selected units.
    [Serializable]
    public sealed class OutsiderExtraplanarIdentity : BlueprintComponent
    {
        public BlueprintUnit OwningBlueprint;
        public BlueprintFeature NativeMarker;
    }

    [HarmonyPatch(typeof(AddSpellResistance), "OnTurnOn")]
    internal static class OutsiderExtraplanarResistancePatch
    {
        private static bool Prefix(AddSpellResistance __instance)
        {
            if (__instance == null || __instance.Owner == null ||
                __instance.Owner.Blueprint == null || __instance.Fact == null) return true;
            var unit = __instance.Owner.Blueprint;
            bool exact = (unit.ComponentsArray ?? new BlueprintComponent[0])
                .OfType<OutsiderExtraplanarIdentity>().Any(marker =>
                    ReferenceEquals(marker.OwningBlueprint, unit) && marker.NativeMarker != null &&
                    ReferenceEquals(marker.NativeMarker, __instance.Fact.Blueprint));
            if (!exact) return true;
            // Native source-ID cleanup is idempotent and clears its own saved
            // applied ID. Never clear the whole part or another fact's grant.
            __instance.OnTurnOff();
            return false;
        }
    }
}
