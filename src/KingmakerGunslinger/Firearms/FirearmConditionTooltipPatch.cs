using Harmony12;
using Kingmaker.UI.Tooltip;

namespace KingmakerGunslinger.Firearms
{
    [HarmonyPatch(typeof(DescriptionTemplatesItem), "ItemHeader")]
    internal static class FirearmConditionTooltipPatch
    {
        [HarmonyPostfix]
        private static void Postfix(DescriptionBricksBox box, TooltipData data)
        {
            if (box == null || data == null || data.Item == null ||
                !FirearmRuntimeState.IsConfigured)
            {
                return;
            }

            FirearmItemStateSnapshot firearm;
            string ignored;
            if (!FirearmRuntimeState.Service.TryGetOrCreate(
                data.Item, out firearm, out ignored))
            {
                return;
            }

            box.Add(DescriptionTemplatesBase.Bricks.CursiveText).SetText(
                FirearmConditionPresentation.Describe(
                    firearm.Repository.State.Condition));
        }
    }
}
