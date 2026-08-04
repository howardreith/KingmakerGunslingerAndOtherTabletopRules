using Harmony12;
using Kingmaker.UI.Tooltip;

namespace KingmakerGunslinger.Firearms
{
    [HarmonyPatch(typeof(DescriptionTemplatesItem), "ItemQualities")]
    internal static class FirearmQualitiesTooltipPatch
    {
        [HarmonyPrefix]
        private static bool Prefix(DescriptionBricksBox box, TooltipData data,
            ref bool __result)
        {
            if (box == null || data == null || data.Item == null ||
                !FirearmRuntimeState.IsConfigured)
                return true;

            FirearmItemStateSnapshot firearm;
            string ignored;
            if (!FirearmRuntimeState.Service.TryGetOrCreate(
                data.Item, out firearm, out ignored))
                return true;

            box.Add(DescriptionTemplatesBase.Bricks.TitleH2)
                .SetText("Qualities");
            box.Add(DescriptionTemplatesBase.Bricks.Separator2);
            box.Add(DescriptionTemplatesBase.Bricks.SimpleTextBold).SetText(
                FirearmConditionPresentation.DescribeQualities(
                    firearm.Definition, firearm.Repository.State));
            __result = true;
            return false;
        }
    }
}
