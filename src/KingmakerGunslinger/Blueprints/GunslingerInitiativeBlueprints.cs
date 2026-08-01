using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using KingmakerGunslinger.Classes;
using UnityEngine;

namespace KingmakerGunslinger.Blueprints
{
    internal static class GunslingerInitiativeBlueprints
    {
        internal const string FeatureSymbol = "KMG.Deeds.GunslingerInitiativeFeature";

        internal static BlueprintFeature Register(BlueprintRegistry registry,
            BlueprintAbilityResource grit)
        {
            if (registry == null) throw new ArgumentNullException("registry");
            if (grit == null) throw new ArgumentNullException("grit");
            return registry.Register<BlueprintFeature>(FeatureSymbol,
                () => Create(grit));
        }

        private static BlueprintFeature Create(BlueprintAbilityResource grit)
        {
            var result = ScriptableObject.CreateInstance<BlueprintFeature>();
            result.name = "KMG_Gunslinger_Initiative";
            result.Ranks = 1;
            result.IsClassFeature = true;
            result.HideInUI = false;
            var bonus = ScriptableObject.CreateInstance<GunslingerInitiativeBonus>();
            bonus.name = "$KMG_Gunslinger_Initiative_Bonus";
            bonus.GritResource = grit;
            result.ComponentsArray = new BlueprintComponent[] { bonus };
            BlueprintUnitFactAccess.Resolve().Configure(result,
                LocalizationService.Create("KMG.GunslingerInitiative.Name",
                    "Gunslinger Initiative"),
                LocalizationService.Create("KMG.GunslingerInitiative.Description",
                    "While you have at least 1 grit point, you gain a +2 bonus on initiative checks."),
                null);
            return result;
        }
    }
}
