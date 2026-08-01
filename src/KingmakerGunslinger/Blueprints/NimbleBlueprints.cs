using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using KingmakerGunslinger.Classes;
using UnityEngine;

namespace KingmakerGunslinger.Blueprints
{
    internal sealed class NimbleBlueprintSet
    {
        internal NimbleBlueprintSet(BlueprintFeature[] features)
        { Features = features ?? throw new ArgumentNullException("features"); }
        internal BlueprintFeature[] Features { get; private set; }
        internal int Count { get { return Features.Length; } }
    }
    internal static class NimbleBlueprints
    {
        internal static readonly string[] Symbols = {
            "KMG.Classes.Nimble1", "KMG.Classes.Nimble2",
            "KMG.Classes.Nimble3", "KMG.Classes.Nimble4", "KMG.Classes.Nimble5" };
        internal static NimbleBlueprintSet Register(BlueprintRegistry registry)
        {
            var features = new BlueprintFeature[5];
            for (int index = 0; index < features.Length; index++)
            {
                int rank = index + 1;
                features[index] = registry.Register<BlueprintFeature>(Symbols[index],
                    () => Create(rank));
            }
            return new NimbleBlueprintSet(features);
        }
        private static BlueprintFeature Create(int rank)
        {
            var result = ScriptableObject.CreateInstance<BlueprintFeature>();
            result.name = "KMG_Nimble_" + rank; result.Ranks = 1;
            result.IsClassFeature = true; result.HideInUI = false;
            var bonus = ScriptableObject.CreateInstance<NimbleArmorClassBonus>();
            bonus.name = "$KMG_Nimble_AC_" + rank;
            result.ComponentsArray = new BlueprintComponent[] { bonus };
            BlueprintUnitFactAccess.Resolve().Configure(result,
                LocalizationService.Create("KMG.Nimble." + rank + ".Name",
                    "Nimble +" + rank),
                LocalizationService.Create("KMG.Nimble." + rank + ".Description",
                    "Gain a cumulative +" + rank + " dodge bonus to AC while wearing light or no armor. This bonus is lost whenever the Dexterity bonus to AC is lost."), null);
            return result;
        }
    }
}
