using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using UnityEngine;

namespace KingmakerGunslinger.Blueprints
{
    internal static class DeedTierBlueprints
    {
        internal static readonly string[] Symbols = {
            "KMG.Presentation.DeedsLevel1", "KMG.Presentation.DeedsLevel7",
            "KMG.Presentation.DeedsLevel11", "KMG.Presentation.DeedsLevel15",
            "KMG.Presentation.DeedsLevel19" };
        internal static readonly int[] Levels = { 1, 7, 11, 15, 19 };
        internal static readonly string[] Descriptions = {
            "Deadeye, Gunslinger's Dodge, and Quick Clear.",
            "Dead Shot, Startling Shot, and Targeting deeds.",
            "Bleeding Wound, Expert Loading, and Lightning Reload.",
            "Evasive, Menacing Shot, and Slinger's Luck.",
            "Cheat Death, Death's Shot, and Stunning Shot." };

        internal static BlueprintFeature[] Register(BlueprintRegistry registry)
        {
            var result = new BlueprintFeature[Levels.Length];
            for (int i = 0; i < result.Length; i++)
            {
                int level = Levels[i]; string description = Descriptions[i];
                result[i] = registry.Register<BlueprintFeature>(Symbols[i],
                    () => Create(level, description));
            }
            return result;
        }

        private static BlueprintFeature Create(int level, string description)
        {
            var result = ScriptableObject.CreateInstance<BlueprintFeature>();
            result.name = "KMG_Gunslinger_Deeds_Level_" + level;
            result.Ranks = 1; result.IsClassFeature = true; result.HideInUI = false;
            result.ComponentsArray = Array.Empty<BlueprintComponent>();
            BlueprintUnitFactAccess.Resolve().Configure(result,
                LocalizationService.Create("KMG.Deeds.Level" + level + ".Name",
                    "Gunslinger Deeds — Level " + level),
                LocalizationService.Create("KMG.Deeds.Level" + level + ".Description",
                    description + " Individual deed abilities remain available in the action bar."),
                null);
            return result;
        }
    }
}
