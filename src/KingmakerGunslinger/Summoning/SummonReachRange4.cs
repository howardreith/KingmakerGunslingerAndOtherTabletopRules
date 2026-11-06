using Harmony12;
using Kingmaker.Items;
using Kingmaker.Utility;
using KingmakerGunslinger.Bootstrap;

namespace KingmakerGunslinger.Summoning
{
    /// <summary>
    /// The exact-owner, exact-item range correction for the Sprint 4 rows in
    /// SummonReachRepresentation4. It is the same mechanism the released
    /// Griffon, Gorgon, Bulette, Elephant, Triceratops, Barbazu and Babau
    /// corrections use. It adds back the range a row's type lost when its
    /// stored raw range became the ordinary five feet. The weapon type stays
    /// at the native minimum, so ProperFlanking2 no longer treats it as a
    /// reach weapon. Every other owner and item, including genuine reach
    /// weapons and every campaign weapon, is untouched.
    /// </summary>
    [HarmonyPatch(typeof(ItemEntityWeapon), "get_AttackRange")]
    internal static class SummonReachRange4
    {
        private static void Postfix(ItemEntityWeapon __instance, ref Feet __result)
        {
            var owner = __instance?.Wielder?.Unit?.Blueprint;
            var item = __instance?.Blueprint;
            if (owner == null || item == null) return;
            ModContext context;
            if (!ModContext.TryGet(out context) ||
                context.FeatureModules?.Active?.ExpandedSummoning != true) return;
            int added = SummonReachRepresentation4.CorrectionFeet(owner.AssetGuid, item.AssetGuid);
            if (added != 0) __result = new Feet(__result.Value + added);
        }
    }
}
