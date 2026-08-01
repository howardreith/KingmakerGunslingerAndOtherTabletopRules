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
        internal GritBlueprintSet(BlueprintAbilityResource resource, BlueprintFeature feature,
            BlueprintFeature initializedMarker)
        {
            Resource = resource ?? throw new ArgumentNullException("resource");
            Feature = feature ?? throw new ArgumentNullException("feature");
            InitializedMarker = initializedMarker ??
                throw new ArgumentNullException("initializedMarker");
        }

        internal BlueprintAbilityResource Resource { get; private set; }
        internal BlueprintFeature Feature { get; private set; }
        internal BlueprintFeature InitializedMarker { get; private set; }
        internal int Count { get { return 3; } }
    }

    internal static class GritBlueprints
    {
        internal const string ResourceSymbol = "KMG.Classes.GunslingerGritResource";
        internal const string FeatureSymbol = "KMG.Classes.GunslingerGritFeature";
        internal const string InitializedMarkerSymbol =
            "KMG.Classes.GunslingerGritInitialized";

        internal static GritBlueprintSet Register(BlueprintRegistry registry,
            BlueprintCharacterClass gunslingerClass)
        {
            if (registry == null) throw new ArgumentNullException("registry");
            if (gunslingerClass == null) throw new ArgumentNullException("gunslingerClass");
            BlueprintAbilityResource resource = registry.Register<BlueprintAbilityResource>(
                ResourceSymbol, CreateResource);
            BlueprintFeature marker = registry.Register<BlueprintFeature>(
                InitializedMarkerSymbol, CreateInitializedMarker);
            BlueprintFeature feature = registry.Register<BlueprintFeature>(
                FeatureSymbol, () => CreateFeature(resource, gunslingerClass, marker));
            Validate(resource, feature);
            return new GritBlueprintSet(resource, feature, marker);
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

        private static BlueprintFeature CreateInitializedMarker()
        {
            var marker = ScriptableObject.CreateInstance<BlueprintFeature>();
            marker.name = "KMG_Gunslinger_Grit_Initialized";
            marker.Ranks = 1;
            marker.IsClassFeature = false;
            marker.HideInUI = true;
            marker.ComponentsArray = Array.Empty<BlueprintComponent>();
            return marker;
        }

        private static BlueprintFeature CreateFeature(BlueprintAbilityResource resource,
            BlueprintCharacterClass gunslingerClass, BlueprintFeature marker)
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
            var initialRestore = ScriptableObject.CreateInstance<GritInitialLevelRestore>();
            initialRestore.name = "$KMG_Gunslinger_GritInitialLevelRestore";
            initialRestore.Resource = resource;
            initialRestore.CharacterClass = gunslingerClass;
            initialRestore.InitializedMarker = marker;
            feature.ComponentsArray = new BlueprintComponent[]
                { addResource, wisdomBonus, initialRestore };
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
            ConfigureEmptyArray(amountField.FieldType, amount, "Class");
            ConfigureEmptyArray(amountField.FieldType, amount, "Archetypes");
            ConfigureEmptyArray(amountField.FieldType, amount, "ClassDiv");
            ConfigureEmptyArray(amountField.FieldType, amount, "ArchetypesDiv");
            amountField.SetValue(resource, amount);
        }

        private static void ConfigureEmptyArray(Type amountType, object amount,
            string fieldName)
        {
            FieldInfo field = amountType.GetField(fieldName,
                BindingFlags.Instance | BindingFlags.Public);
            if (field == null || !field.FieldType.IsArray)
                throw new MissingFieldException(amountType.FullName, fieldName);
            field.SetValue(amount, Array.CreateInstance(
                field.FieldType.GetElementType(), 0));
        }

        private static void Validate(BlueprintAbilityResource resource, BlueprintFeature feature)
        {
            if (feature.ComponentsArray == null || feature.ComponentsArray.Length != 3)
                throw new InvalidOperationException("Grit feature components are incomplete.");
            AddAbilityResources add = feature.ComponentsArray[0] as AddAbilityResources;
            GritResourceAmountBonus bonus = feature.ComponentsArray[1] as GritResourceAmountBonus;
            GritInitialLevelRestore initial = feature.ComponentsArray[2] as
                GritInitialLevelRestore;
            if (add == null || bonus == null || !ReferenceEquals(add.Resource, resource) ||
                !ReferenceEquals(bonus.Resource, resource) || !add.RestoreAmount ||
                initial == null || !ReferenceEquals(initial.Resource, resource) ||
                initial.CharacterClass == null || initial.InitializedMarker == null ||
                add.RestoreOnLevelUp ||
                add.UseThisAsResource)
                throw new InvalidOperationException("Grit resource ownership contract is incomplete.");
        }
    }
}
