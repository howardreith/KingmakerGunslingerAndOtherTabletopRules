using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using KingmakerGunslinger.Classes;
using UnityEngine;

namespace KingmakerGunslinger.Blueprints
{
    internal sealed class FirearmTrainingBlueprintSet
    {
        internal FirearmTrainingBlueprintSet(BlueprintFeature pistol,
            BlueprintFeature musket)
        {
            Pistol = pistol ?? throw new ArgumentNullException("pistol");
            Musket = musket ?? throw new ArgumentNullException("musket");
        }
        internal BlueprintFeature Pistol { get; private set; }
        internal BlueprintFeature Musket { get; private set; }
        internal int Count { get { return 2; } }
    }

    internal static class FirearmTrainingBlueprints
    {
        internal const string PistolSymbol = "KMG.Archetypes.PistolTraining";
        internal const string MusketSymbol = "KMG.Archetypes.MusketTraining";

        internal static FirearmTrainingBlueprintSet Register(
            BlueprintRegistry registry)
        {
            if (registry == null) throw new ArgumentNullException("registry");
            BlueprintFeature pistol = registry.Register<BlueprintFeature>(
                PistolSymbol, () => Create("Pistol Training", "one-handed"));
            BlueprintFeature musket = registry.Register<BlueprintFeature>(
                MusketSymbol, () => Create("Musket Training", "two-handed"));
            FirearmTrainingRuntime.Configure(pistol, musket);
            return new FirearmTrainingBlueprintSet(pistol, musket);
        }

        private static BlueprintFeature Create(string name, string family)
        {
            var feature = ScriptableObject.CreateInstance<BlueprintFeature>();
            feature.name = "KMG_" + name.Replace(" ", "_");
            feature.Ranks = 4;
            feature.IsClassFeature = true;
            feature.HideInUI = false;
            var damage = ScriptableObject.CreateInstance<GunTrainingDamage>();
            feature.ComponentsArray = new BlueprintComponent[] { damage };
            BlueprintUnitFactAccess.Resolve().Configure(feature,
                LocalizationService.Create("KMG.Archetypes." +
                    name.Replace(" ", "") + ".Name", name),
                LocalizationService.Create("KMG.Archetypes." +
                    name.Replace(" ", "") + ".Description",
                    "Add your Dexterity modifier to damage with " + family +
                    " firearms. At ranks 2, 3, and 4, add another +1 damage. " +
                    "While eligible firearms are Broken, their misfire value " +
                    "increases by 2 instead of 4."), null);
            return feature;
        }
    }
}
