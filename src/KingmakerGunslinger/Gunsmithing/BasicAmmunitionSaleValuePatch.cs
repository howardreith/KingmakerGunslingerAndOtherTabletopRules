using System;
using Harmony12;
using Kingmaker.Items;
using KingmakerGunslinger.Bootstrap;

namespace KingmakerGunslinger.Gunsmithing
{
    [HarmonyPatch(typeof(VendorLogic), "GetItemSellPrice",
        new Type[] { typeof(ItemEntity) })]
    internal static class BasicAmmunitionSaleValuePatch
    {
        private static void Postfix(ItemEntity item, ref long __result)
        {
            var ammo = BlueprintBootstrap.BasicAmmunition;
            if (item != null && ammo != null &&
                (ReferenceEquals(item.Blueprint, ammo.BlackPowder) ||
                 ReferenceEquals(item.Blueprint, ammo.LeadBall)))
                __result = 0L;
        }
    }
}
