using System;
using Harmony12;
using Kingmaker.Items;
using KingmakerGunslinger.Firearms;

namespace KingmakerGunslinger.Gunsmithing
{
    internal static class BatteredFirearmSaleValueRuntime
    {
        internal static bool TryGetFixedValue(ItemEntity item, out long value)
        {
            value = 0;
            ItemEntityWeapon weapon = item as ItemEntityWeapon;
            if (weapon == null) return false;

            FirearmItemId itemId;
            string reason;
            if (!new KingmakerFirearmItemIdentityProvider().TryGetIdentity(
                    weapon, out itemId, out reason) || itemId == null)
                return false;

            UnitPartBatteredFirearmOwnership part;
            OriginatingUnitId owner;
            if (!new KingmakerBatteredFirearmOwnershipPartProvider()
                    .TryGetExisting(out part) || part == null ||
                !part.TryGetOwner(itemId, out owner))
                return false;

            value = BatteredFirearmUsePolicy.FixedExpectedScrapValueGold;
            return true;
        }
    }

    [HarmonyPatch(typeof(VendorLogic), "GetItemBuyPrice",
        new Type[] { typeof(ItemEntity) })]
    internal static class BatteredFirearmSaleValuePatch
    {
        private static void Postfix(ItemEntity item, ref long __result)
        {
            long fixedValue;
            if (BatteredFirearmSaleValueRuntime.TryGetFixedValue(
                    item, out fixedValue))
                __result = fixedValue;
        }
    }
}
