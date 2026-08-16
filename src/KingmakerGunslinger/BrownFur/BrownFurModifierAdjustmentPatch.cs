using System;
using Harmony12;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics;

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

    [HarmonyPatch(typeof(BuffCollection), "AddBuffInternal", new[] {
        typeof(BlueprintBuff), typeof(MechanicsContext), typeof(TimeSpan?) })]
    [HarmonyAfter("CallOfTheWild")]
    internal static class BrownFurOrdinaryRecastPatch
    {
        private static void Postfix(MechanicsContext __1, Buff __result)
        {
            try
            {
                BrownFurModifierAdjustmentRuntime.RestoreOrdinaryRecast(
                    __result, __1);
            }
            catch
            {
                // An optional reconciliation failure must not break native
                // buff application. The persisted carrier remains fail closed.
            }
        }
    }
}
