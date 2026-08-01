using System;
using System.Reflection;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Designers.Mechanics.Facts;
using KingmakerGunslinger.Grit;
using UnityEngine;

namespace KingmakerGunslinger.Blueprints
{
    internal sealed class GritBlueprintSet
    {
        internal GritBlueprintSet(BlueprintAbilityResource resource, BlueprintFeature feature)
        {
            Resource = resource ?? throw new ArgumentNullException("resource");
            Feature = feature ?? throw new ArgumentNullException("feature");
        }

        internal BlueprintAbilityResource Resource { get; private set; }
        internal BlueprintFeature Feature { get; private set; }
        internal int Count { get { return 2; } }
    }

    internal static class GritBlueprints
    {
        internal const string ResourceSymbol = "KMG.Classes.GunslingerGritResource";
        internal const string FeatureSymbol = "KMG.Classes.GunslingerGritFeature";

        internal static GritBlueprintSet Register(BlueprintRegistry registry)
        {
            if (registry == null) throw new ArgumentNullException("registry");
            BlueprintAbilityResource resource = registry.Register<BlueprintAbilityResource>(
                ResourceSymbol, CreateResource);
            BlueprintFeature feature = registry.Register<BlueprintFeature>(
                FeatureSymbol, () => CreateFeature(resource));
            Validate(resource, feature);
            return new GritBlueprintSet(resource, feature);
        }

        private static BlueprintAbilityResource CreateResource()
        {
            var resource = ScriptableObject.CreateInstance<BlueprintAbilityResource>();
            resource.name = "KMG_Gunslinger_Grit_Resource";
            resource.LocalizedName = LocalizationService.Create(
                "KMG.Gunslinger.Grit.Resource.Name", "Grit");
            resource.LocalizedDescription = LocalizationService.Create(
                "KMG.Gunslinger.Grit.Resource.Description",
                "The Gunslinger's current reserve of grit used to perform deeds.");
            ConfigureBaseAmount(resource, 1);
            return resource;
        }

        private static BlueprintFeature CreateFeature(BlueprintAbilityResource resource)
        {
            var feature = ScriptableObject.CreateInstance<BlueprintFeature>();
            feature.name = "KMG_Gunslinger_Grit_Feature";
            feature.Ranks = 1;
            feature.IsClassFeature = true;
            feature.HideInUI = false;

            var addResource = ScriptableObject.CreateInstance<AddAbilityResources>();
            addResource.name = "$KMG_Gunslinger_AddGritResource";
            addResource.UseThisAsResource = false;
            addResource.Resource = resource;
            addResource.Amount = 0;
            addResource.RestoreAmount = true;
            addResource.RestoreOnLevelUp = false;

            var wisdomBonus = ScriptableObject.CreateInstance<GritResourceAmountBonus>();
            wisdomBonus.name = "$KMG_Gunslinger_GritWisdomBonus";
            wisdomBonus.Resource = resource;
            feature.ComponentsArray = new BlueprintComponent[] { addResource, wisdomBonus };
            BlueprintUnitFactAccess.Resolve().Configure(feature,
                LocalizationService.Create("KMG.Gunslinger.Grit.Feature.Name", "Grit"),
                LocalizationService.Create("KMG.Gunslinger.Grit.Feature.Description",
                    "At the start of each day, a Gunslinger has grit equal to their Wisdom modifier (minimum 1). Grit is spent to perform deeds."),
                null);
            return feature;
        }

        private static void ConfigureBaseAmount(BlueprintAbilityResource resource, int baseValue)
        {
            FieldInfo amountField = typeof(BlueprintAbilityResource).GetField(
                "m_MaxAmount", BindingFlags.Instance | BindingFlags.NonPublic);
            if (amountField == null || !amountField.FieldType.IsValueType)
                throw new MissingFieldException(typeof(BlueprintAbilityResource).FullName, "m_MaxAmount");
            object amount = Activator.CreateInstance(amountField.FieldType);
            FieldInfo baseField = amountField.FieldType.GetField("BaseValue",
                BindingFlags.Instance | BindingFlags.Public);
            if (baseField == null || baseField.FieldType != typeof(int))
                throw new MissingFieldException(amountField.FieldType.FullName, "BaseValue");
            baseField.SetValue(amount, baseValue);
            amountField.SetValue(resource, amount);
        }

        private static void Validate(BlueprintAbilityResource resource, BlueprintFeature feature)
        {
            if (feature.ComponentsArray == null || feature.ComponentsArray.Length != 2)
                throw new InvalidOperationException("Grit feature components are incomplete.");
            AddAbilityResources add = feature.ComponentsArray[0] as AddAbilityResources;
            GritResourceAmountBonus bonus = feature.ComponentsArray[1] as GritResourceAmountBonus;
            if (add == null || bonus == null || !ReferenceEquals(add.Resource, resource) ||
                !ReferenceEquals(bonus.Resource, resource) || !add.RestoreAmount ||
                add.RestoreOnLevelUp || add.UseThisAsResource)
                throw new InvalidOperationException("Grit resource ownership contract is incomplete.");
        }
    }
}
