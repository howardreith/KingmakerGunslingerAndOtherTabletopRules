using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.Visual.Animation.Kingmaker.Actions;
using KingmakerGunslinger.Blueprints;
using UnityEngine;

namespace KingmakerGunslinger.ElementalRaces
{
    internal static class ElementalRaceAbilityFactory
    {
        internal static BlueprintAbilityResource RegisterResource(
            BlueprintRegistry registry, ElementalRaceDefinition definition)
        {
            if (registry == null) throw new ArgumentNullException("registry");
            if (definition == null)
                throw new ArgumentNullException("definition");
            return registry.Register<BlueprintAbilityResource>(
                definition.SlaResourceSymbol,
                () => CreateResource(definition));
        }

        internal static BlueprintAbility RegisterAbility(
            LibraryScriptableObject library, BlueprintRegistry registry,
            ElementalRaceDefinition definition,
            BlueprintAbilityResource resource)
        {
            if (library == null) throw new ArgumentNullException("library");
            if (registry == null) throw new ArgumentNullException("registry");
            if (definition == null)
                throw new ArgumentNullException("definition");
            if (resource == null) throw new ArgumentNullException("resource");
            if (definition.Kind == ElementalRaceKind.Undine)
                return registry.Register<BlueprintAbility>(
                    definition.SlaAbilitySymbol,
                    () => CreateHydraulicPush(definition, resource));

            BlueprintAbility donor = BlueprintLibraryLookup.RequireExact<
                BlueprintAbility>(library, definition.DonorAbilityGuid,
                "native " + definition.SlaName +
                " donor for elemental racial SLA");
            return registry.Register<BlueprintAbility>(
                definition.SlaAbilitySymbol,
                () => CloneNativeSpell(definition, donor, resource));
        }

        internal static BlueprintFeature RegisterFeature(
            BlueprintRegistry registry, ElementalRaceDefinition definition,
            BlueprintAbilityResource resource, BlueprintAbility ability)
        {
            if (registry == null) throw new ArgumentNullException("registry");
            if (definition == null)
                throw new ArgumentNullException("definition");
            if (resource == null) throw new ArgumentNullException("resource");
            if (ability == null) throw new ArgumentNullException("ability");
            return registry.Register<BlueprintFeature>(
                definition.SlaFeatureSymbol,
                () => CreateFeature(definition, resource, ability));
        }

        private static BlueprintAbilityResource CreateResource(
            ElementalRaceDefinition definition)
        {
            var resource = ScriptableObject.CreateInstance<
                BlueprintAbilityResource>();
            resource.name = InternalName(definition.SlaResourceSymbol);
            resource.LocalizedName = LocalizationService.Create(
                LocalizationKey(definition, "Resource.Name"),
                definition.SlaName + " Uses");
            resource.LocalizedDescription = LocalizationService.Create(
                LocalizationKey(definition, "Resource.Description"),
                "One use per ordinary rest.");
            ConfigureBaseAmount(resource, 1);
            return resource;
        }

        private static BlueprintAbility CloneNativeSpell(
            ElementalRaceDefinition definition, BlueprintAbility donor,
            BlueprintAbilityResource resource)
        {
            BlueprintAbility ability = BlueprintCloneService.Clone(donor,
                InternalName(definition.SlaAbilitySymbol));
            BlueprintComponent[] donorComponents = donor.ComponentsArray ??
                Array.Empty<BlueprintComponent>();
            ability.ComponentsArray = (ability.ComponentsArray ??
                Array.Empty<BlueprintComponent>()).Where(IsSafeNativeEffect)
                .Concat(new BlueprintComponent[] { ResourceCost(resource) })
                .ToArray();
            ability.Type = AbilityType.SpellLike;
            ability.Parent = null;
            ability.Hidden = false;
            ability.ActionBarAutoFillIgnored = false;
            ability.MaterialComponent = new BlueprintAbility
                .MaterialComponentData();
            ability.ResourceAssetIds = Array.Empty<string>();
            BlueprintUnitFactAccess.Resolve().Configure(ability,
                LocalizationService.Create(
                    LocalizationKey(definition, "Ability.Name"),
                    definition.SlaName),
                LocalizationService.Create(
                    LocalizationKey(definition, "Ability.Description"),
                    definition.SlaDescription), donor.Icon);
            ValidateClone(definition, donor, donorComponents, ability,
                resource);
            return ability;
        }

        private static bool IsSafeNativeEffect(BlueprintComponent component)
        {
            if (component == null) return false;
            Type type = component.GetType();
            string fullName = type.FullName ?? string.Empty;
            if (!fullName.StartsWith("Kingmaker.",
                StringComparison.Ordinal)) return false;
            if (component is SpellListComponent ||
                component is AbilityResourceLogic ||
                component is ContextCalculateAbilityParams ||
                component is AbilityVariants ||
                fullName.IndexOf(".Recommendations.",
                    StringComparison.Ordinal) >= 0)
                return false;
            return true;
        }

        private static BlueprintAbility CreateHydraulicPush(
            ElementalRaceDefinition definition,
            BlueprintAbilityResource resource)
        {
            var ability = ScriptableObject.CreateInstance<BlueprintAbility>();
            ability.name = InternalName(definition.SlaAbilitySymbol);
            ability.Type = AbilityType.SpellLike;
            ability.Parent = null;
            ability.Hidden = false;
            ability.ActionBarAutoFillIgnored = false;
            ability.Range = AbilityRange.Close;
            ability.CanTargetEnemies = true;
            ability.CanTargetSelf = false;
            ability.CanTargetFriends = false;
            ability.CanTargetPoint = false;
            ability.SpellResistance = true;
            ability.NeedEquipWeapons = false;
            ability.EffectOnEnemy = AbilityEffectOnUnit.Harmful;
            ability.EffectOnAlly = AbilityEffectOnUnit.None;
            ability.ActionType = UnitCommand.CommandType.Standard;
            ability.Animation = UnitAnimationActionCastSpell
                .CastAnimationStyle.Point;
            ability.MaterialComponent = new BlueprintAbility
                .MaterialComponentData();
            ability.ResourceAssetIds = Array.Empty<string>();
            ability.LocalizedDuration = LocalizationService.Create(
                LocalizationKey(definition, "Ability.Duration"),
                "Instantaneous");
            ability.LocalizedSavingThrow = LocalizationService.Create(
                LocalizationKey(definition, "Ability.SavingThrow"), "None");

            var spell = ScriptableObject.CreateInstance<SpellComponent>();
            spell.School = SpellSchool.Evocation;
            var maneuver = ScriptableObject.CreateInstance<
                ContextActionCombatManeuver>();
            maneuver.Type = CombatManeuver.BullRush;
            maneuver.UseCasterLevelAsBaseAttack = true;
            maneuver.UseBestMentalStat = true;
            maneuver.OnSuccess = new ActionList
            {
                Actions = Array.Empty<GameAction>()
            };
            var effect = ScriptableObject.CreateInstance<
                AbilityEffectRunAction>();
            effect.SavingThrowType = SavingThrowType.Unknown;
            effect.Actions = new ActionList
            {
                Actions = new GameAction[] { maneuver }
            };
            ability.ComponentsArray = new BlueprintComponent[]
            {
                spell, ResourceCost(resource), effect
            };
            BlueprintUnitFactAccess.Resolve().Configure(ability,
                LocalizationService.Create(
                    LocalizationKey(definition, "Ability.Name"),
                    definition.SlaName),
                LocalizationService.Create(
                    LocalizationKey(definition, "Ability.Description"),
                    definition.SlaDescription), null);
            return ability;
        }

        private static BlueprintFeature CreateFeature(
            ElementalRaceDefinition definition,
            BlueprintAbilityResource resource, BlueprintAbility ability)
        {
            var feature = ScriptableObject.CreateInstance<BlueprintFeature>();
            feature.name = InternalName(definition.SlaFeatureSymbol);
            feature.Ranks = 1;
            feature.IsClassFeature = false;
            feature.HideInUI = false;
            feature.Groups = Array.Empty<FeatureGroup>();
            var facts = ScriptableObject.CreateInstance<AddFacts>();
            facts.Facts = new BlueprintUnitFact[] { ability };
            facts.DoNotRestoreMissingFacts = false;
            var add = ScriptableObject.CreateInstance<AddAbilityResources>();
            add.UseThisAsResource = false;
            add.Resource = resource;
            add.Amount = 0;
            add.RestoreAmount = true;
            add.RestoreOnLevelUp = false;
            var parameters = ScriptableObject.CreateInstance<
                ElementalRacialSpellLikeParameters>();
            parameters.Ability = ability;
            parameters.Stat = StatType.Charisma;
            parameters.SpellLevel = definition.SpellLevel;
            feature.ComponentsArray = new BlueprintComponent[]
            {
                facts, add, parameters
            };
            BlueprintUnitFactAccess.Resolve().Configure(feature,
                LocalizationService.Create(
                    LocalizationKey(definition, "Feature.Name"),
                    definition.SlaName),
                LocalizationService.Create(
                    LocalizationKey(definition, "Feature.Description"),
                    definition.SlaDescription), ability.Icon);
            return feature;
        }

        private static AbilityResourceLogic ResourceCost(
            BlueprintAbilityResource resource)
        {
            var result = ScriptableObject.CreateInstance<
                AbilityResourceLogic>();
            result.RequiredResource = resource;
            result.IsSpendResource = true;
            result.CostIsCustom = false;
            result.Amount = 1;
            return result;
        }

        private static void ValidateClone(ElementalRaceDefinition definition,
            BlueprintAbility donor, BlueprintComponent[] donorComponents,
            BlueprintAbility ability, BlueprintAbilityResource resource)
        {
            if (ReferenceEquals(donor, ability) ||
                ability.Type != AbilityType.SpellLike || ability.Parent != null ||
                ability.ComponentsArray.OfType<SpellListComponent>().Any() ||
                ability.ComponentsArray.OfType<ContextCalculateAbilityParams>()
                    .Any() ||
                ability.ComponentsArray.Any(value => value != null &&
                    !(value.GetType().FullName ?? string.Empty).StartsWith(
                        "Kingmaker.", StringComparison.Ordinal)) ||
                ability.ComponentsArray.OfType<AbilityResourceLogic>().Count() != 1 ||
                !ReferenceEquals(ability.ComponentsArray.OfType<
                    AbilityResourceLogic>().Single().RequiredResource, resource) ||
                !ability.ComponentsArray.OfType<SpellComponent>().Any() ||
                !ability.ComponentsArray.OfType<AbilityEffectRunAction>().Any())
                throw new InvalidOperationException(definition.SlaName +
                    " racial SLA clone is incomplete or contains a foreign component.");
            if (donorComponents.Length == 0)
                throw new InvalidOperationException(
                    "The native racial SLA donor has no components.");
        }

        private static void ConfigureBaseAmount(
            BlueprintAbilityResource resource, int baseValue)
        {
            FieldInfo amountField = typeof(BlueprintAbilityResource).GetField(
                "m_MaxAmount", BindingFlags.Instance | BindingFlags.NonPublic);
            if (amountField == null || !amountField.FieldType.IsValueType)
                throw new MissingFieldException(
                    typeof(BlueprintAbilityResource).FullName, "m_MaxAmount");
            object amount = Activator.CreateInstance(amountField.FieldType);
            FieldInfo baseField = amountField.FieldType.GetField("BaseValue",
                BindingFlags.Instance | BindingFlags.Public);
            if (baseField == null || baseField.FieldType != typeof(int))
                throw new MissingFieldException(amountField.FieldType.FullName,
                    "BaseValue");
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

        private static string LocalizationKey(
            ElementalRaceDefinition definition, string suffix)
        {
            return "KMG.ElementalRaces." + definition.Kind + "." + suffix;
        }

        private static string InternalName(string symbol)
        {
            return symbol.Replace('.', '_');
        }
    }
}
