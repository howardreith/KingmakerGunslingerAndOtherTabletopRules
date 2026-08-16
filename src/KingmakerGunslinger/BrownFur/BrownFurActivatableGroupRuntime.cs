using Harmony12;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Parts;

namespace KingmakerGunslinger.BrownFur
{
    internal static class BrownFurActivatableGroupRuntime
    {
        // Blueprint enum values are serialized as Int32. This project-owned
        // value is deliberately outside Kingmaker's and CotW's known ranges;
        // the exact GetGroupSize patch below supplies its one-slot capacity
        // without indexing the engine's fixed native-group array.
        internal const int PowerfulChangeGroupValue = 81082;
        internal static readonly ActivatableAbilityGroup PowerfulChangeGroup =
            (ActivatableAbilityGroup)PowerfulChangeGroupValue;
    }

    [HarmonyPatch(typeof(UnitPartActivatableAbility), "GetGroupSize", new[] {
        typeof(ActivatableAbilityGroup) })]
    internal static class BrownFurActivatableGroupSizePatch
    {
        private static bool Prefix(ActivatableAbilityGroup __0,
            ref int __result)
        {
            if ((int)__0 != BrownFurActivatableGroupRuntime
                    .PowerfulChangeGroupValue)
                return true;
            __result = 1;
            return false;
        }
    }
}
