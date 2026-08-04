using Harmony12;
using Kingmaker.Controllers.Rest;
using Kingmaker.UnitLogic;
using KingmakerGunslinger.Bootstrap;

namespace KingmakerGunslinger.Gunsmithing
{
    [HarmonyPatch(typeof(RestController), "ApplyRest")]
    internal static class CraftingRestResetPatch
    {
        private static void Postfix(UnitDescriptor unit)
        {
            var set = BlueprintBootstrap.GunsmithingCrafting;
            if (unit != null && set != null && unit.HasFact(set.UsedMarker))
                unit.RemoveFact(set.UsedMarker);
        }
    }
}
