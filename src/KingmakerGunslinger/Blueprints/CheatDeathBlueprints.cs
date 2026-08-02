using System;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using KingmakerGunslinger.Deeds;
using UnityEngine;

namespace KingmakerGunslinger.Blueprints
{
    internal static class CheatDeathBlueprints
    {
        internal const string FeatureSymbol = "KMG.Deeds.CheatDeathFeature";

        internal static BlueprintFeature Register(BlueprintRegistry registry,
            BlueprintAbilityResource grit, BlueprintCharacterClass gunslingerClass)
        {
            BlueprintFeature feature = registry.Register<BlueprintFeature>(
                FeatureSymbol, () => Create(grit, gunslingerClass));
            CheatDeathDamageHandler handler = feature.ComponentsArray.OfType<
                CheatDeathDamageHandler>().Single();
            if (!ReferenceEquals(handler.Grit, grit) ||
                !ReferenceEquals(handler.GunslingerClass, gunslingerClass))
                throw new InvalidOperationException(
                    "Cheat Death exact damage-handler contract is incomplete.");
            return feature;
        }

        private static BlueprintFeature Create(BlueprintAbilityResource grit,
            BlueprintCharacterClass gunslingerClass)
        {
            var feature = ScriptableObject.CreateInstance<BlueprintFeature>();
            feature.name = "KMG_CheatDeath_Feature";
            feature.Ranks = 1;
            feature.IsClassFeature = true;
            feature.HideInUI = false;
            var handler = ScriptableObject.CreateInstance<CheatDeathDamageHandler>();
            handler.name = "$KMG_CheatDeath_DamageHandler";
            handler.Grit = grit;
            handler.GunslingerClass = gunslingerClass;
            feature.ComponentsArray = new BlueprintComponent[] { handler };
            BlueprintUnitFactAccess.Resolve().Configure(feature,
                LocalizationService.Create("KMG.CheatDeath.Name", "Cheat Death"),
                LocalizationService.Create("KMG.CheatDeath.Description",
                    "When damage reduces you to 0 or fewer hit points, spend all remaining grit (minimum 1) to remain at 1 hit point."),
                null);
            return feature;
        }
    }
}
