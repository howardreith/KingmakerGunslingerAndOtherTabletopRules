using Harmony12;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.UnitLogic.Buffs;

namespace KingmakerGunslinger.BrownFur
{
    [HarmonyPatch(typeof(ModifiableValue), "AddModifier", new[] {
        typeof(ModifiableValue.Modifier) })]
    [HarmonyAfter("CallOfTheWild")]
    internal static class BrownFurModifierAdjustmentPatch
    {
        private static void Prefix(ModifiableValue __instance,
            ModifiableValue.Modifier __0)
        {
            try
            { BrownFurModifierAdjustmentRuntime.TryAdjust(__instance, __0); }
            catch
            {
                // Optional Brown-Fur adjustment must never break native or CotW
                // modifier registration. An unmatched cast remains ordinary.
            }
        }
    }

    [HarmonyPatch(typeof(Buff), "Remove")]
    [HarmonyAfter("CallOfTheWild")]
    internal static class BrownFurPersistedModifierRemovalPatch
    {
        private static void Prefix(Buff __instance)
        {
            try { BrownFurModifierAdjustmentRuntime.Forget(__instance); }
            catch
            {
                // Native removal remains authoritative even if optional
                // Brown-Fur persistence evidence is malformed.
            }
        }
    }
}
