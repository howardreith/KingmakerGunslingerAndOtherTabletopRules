using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using KingmakerGunslinger.Classes;
using KingmakerGunslinger.Firearms;
using UnityEngine;

namespace KingmakerGunslinger.Blueprints
{
    internal sealed class GunTrainingBlueprintSet
    {
        internal GunTrainingBlueprintSet(BlueprintFeatureSelection selection,
            BlueprintFeature[] choices, FirearmKind[] kinds)
        {
            Selection = selection ?? throw new ArgumentNullException("selection");
            Choices = choices ?? throw new ArgumentNullException("choices");
            Kinds = kinds ?? throw new ArgumentNullException("kinds");
            if (choices.Length != kinds.Length)
                throw new ArgumentException("Gun Training choice metadata changed.");
        }
        internal BlueprintFeatureSelection Selection { get; private set; }
        internal BlueprintFeature[] Choices { get; private set; }
        internal FirearmKind[] Kinds { get; private set; }
        internal int Count { get { return 1 + Choices.Length; } }

        internal BlueprintFeature ChoiceFor(FirearmKind kind)
        {
            for (int index = 0; index < Kinds.Length; index++)
                if (Kinds[index] == kind) return Choices[index];
            throw new ArgumentOutOfRangeException("kind");
        }
    }

    internal static class GunTrainingBlueprints
    {
        internal const string SelectionSymbol = "KMG.Classes.GunTrainingSelection";
        internal static readonly string[] ChoiceSymbols = {
            "KMG.Classes.GunTrainingPistol", "KMG.Classes.GunTrainingMusket",
            "KMG.Classes.GunTrainingBlunderbuss", "KMG.Classes.GunTrainingRifle",
            "KMG.Classes.GunTrainingRevolver" };
        internal static readonly FirearmKind[] Kinds = {
            FirearmKind.Pistol, FirearmKind.Musket, FirearmKind.Blunderbuss,
            FirearmKind.Rifle, FirearmKind.Revolver };

        internal static GunTrainingBlueprintSet Register(BlueprintRegistry registry)
        {
            if (registry == null) throw new ArgumentNullException("registry");
            var choices = new BlueprintFeature[Kinds.Length];
            for (int index = 0; index < Kinds.Length; index++)
            {
                FirearmKind kind = Kinds[index];
                choices[index] = registry.Register<BlueprintFeature>(
                    ChoiceSymbols[index], () => CreateChoice(kind));
            }
            BlueprintFeatureSelection selection =
                registry.Register<BlueprintFeatureSelection>(SelectionSymbol,
                    () => CreateSelection(choices));
            return new GunTrainingBlueprintSet(selection, choices,
                (FirearmKind[])Kinds.Clone());
        }

        private static BlueprintFeature CreateChoice(FirearmKind kind)
        {
            var feature = ScriptableObject.CreateInstance<BlueprintFeature>();
            feature.name = "KMG_GunTraining_" + kind;
            feature.Ranks = 1;
            feature.IsClassFeature = true;
            feature.HideInUI = false;
            var damage = ScriptableObject.CreateInstance<GunTrainingDamage>();
            damage.name = "$KMG_GunTraining_Damage_" + kind;
            damage.Kind = kind;
            feature.ComponentsArray = new BlueprintComponent[] { damage };
            BlueprintUnitFactAccess.Resolve().Configure(feature,
                LocalizationService.Create("KMG.GunTraining." + kind + ".Name",
                    "Gun Training: " + kind),
                LocalizationService.Create("KMG.GunTraining." + kind + ".Description",
                    "Add your Dexterity modifier to damage when firing a " + kind +
                    ". While it is Broken, its misfire value increases by 2 instead of 4."),
                null);
            return feature;
        }

        private static BlueprintFeatureSelection CreateSelection(
            BlueprintFeature[] choices)
        {
            var selection = ScriptableObject.CreateInstance<BlueprintFeatureSelection>();
            selection.name = "KMG_GunTraining_Selection";
            selection.Ranks = 1;
            selection.IsClassFeature = true;
            selection.HideInUI = false;
            selection.IgnorePrerequisites = false;
            selection.Obligatory = true;
            selection.Group = FeatureGroup.None;
            selection.Group2 = FeatureGroup.None;
            selection.Features = (BlueprintFeature[])choices.Clone();
            selection.AllFeatures = (BlueprintFeature[])choices.Clone();
            BlueprintUnitFactAccess.Resolve().Configure(selection,
                LocalizationService.Create("KMG.GunTraining.Selection.Name",
                    "Gun Training"),
                LocalizationService.Create("KMG.GunTraining.Selection.Description",
                    "Select one firearm type. You add your Dexterity modifier to damage when firing that type, and its Broken-state misfire increase is reduced from 4 to 2."),
                null);
            return selection;
        }
    }
}
