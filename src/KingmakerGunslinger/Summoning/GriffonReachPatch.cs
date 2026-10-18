using Harmony12;
using Kingmaker.Items;
using Kingmaker.Utility;
using KingmakerGunslinger.Bootstrap;
namespace KingmakerGunslinger.Summoning
{
    [HarmonyPatch(typeof(ItemEntityWeapon), "get_AttackRange")]
    internal static class GriffonReachPatch
    {
        private static void Postfix(ItemEntityWeapon __instance, ref Feet __result)
        {
            ModContext context;
            if (__instance?.Wielder?.Unit?.Blueprint == null || __instance.Blueprint == null ||
                !ModContext.TryGet(out context) || context.FeatureModules?.Active == null ||
                !context.FeatureModules.Active.ExpandedSummoning) return;
            __result = new Feet(GriffonRules.Reach(__instance.Wielder.Unit.Blueprint.AssetGuid,
                __instance.Blueprint.AssetGuid, __result.Value));
        }
    }
}
