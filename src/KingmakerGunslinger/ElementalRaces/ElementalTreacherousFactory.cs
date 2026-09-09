using System;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.ElementsSystem;
using Kingmaker.ResourceLinks;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.Utility;
using Kingmaker.Visual.Animation.Kingmaker.Actions;
using KingmakerGunslinger.Blueprints;
using UnityEngine;

namespace KingmakerGunslinger.ElementalRaces
{
    internal static class ElementalTreacherousFactory
    {
        internal const string Prefix = "KMG.ElementalRaces.Traits.Oread.TreacherousEarth";
        internal static ElementalTraitDailyAbilityBlueprints Register(BlueprintRegistry registry,
            ElementalAlternateTraitId trait, Sprite icon)
        {
            if (trait != ElementalAlternateTraitId.TreacherousEarth) return null;
            var resource = registry.Register<BlueprintAbilityResource>(Prefix + ".Resource", () => {
                var value = ScriptableObject.CreateInstance<BlueprintAbilityResource>();
                value.name = Name("Resource");
                value.LocalizedName = LocalizationService.Create(Prefix + ".Resource.Name", "Treacherous Earth Uses");
                value.LocalizedDescription = LocalizationService.Create(Prefix + ".Resource.Description", ElementalTreacherousPolicy.Description);
                ElementalRaceAbilityFactory.ConfigureBaseAmount(value, 1);
                return value;
            });
            var terrain = registry.Register<BlueprintBuff>(Prefix + ".TerrainBuff", () => {
                var value = ScriptableObject.CreateInstance<BlueprintBuff>();
                value.name = Name("TerrainBuff");
                value.Stacking = StackingType.Stack;
                value.FxOnStart = new PrefabLink(); value.FxOnRemove = new PrefabLink();
                value.ResourceAssetIds = Array.Empty<string>();
                var condition = ScriptableObject.CreateInstance<AddCondition>();
                condition.Condition = UnitCondition.DifficultTerrain;
                value.ComponentsArray = new BlueprintComponent[] { Ground(), condition };
                BlueprintUnitFactAccess.Resolve().Configure(value,
                    LocalizationService.Create(Prefix + ".Terrain.Name", "Treacherous Earth"),
                    LocalizationService.Create(Prefix + ".Terrain.Description", ElementalTreacherousPolicy.Description), icon);
                return ElementalComponentIdentity.Prepare(value);
            });
            var area = registry.Register<BlueprintAbilityAreaEffect>(Prefix + ".Area", () => {
                var value = ScriptableObject.CreateInstance<BlueprintAbilityAreaEffect>();
                value.name = Name("Area"); value.Shape = AreaEffectShape.Cylinder;
                value.Size = ElementalTreacherousPolicy.RadiusFeet.Feet();
                value.SpellResistance = false; value.AffectEnemies = true; value.AggroEnemies = false;
                value.AffectDead = false; value.IgnoreSleepingUnits = false;
                value.Fx = new PrefabLink();
                var logic = ScriptableObject.CreateInstance<ElementalTreacherousArea>();
                logic.Terrain = terrain;
                value.ComponentsArray = new BlueprintComponent[] { Ground(), logic };
                return ElementalComponentIdentity.Prepare(value);
            });
            var ability = registry.Register<BlueprintAbility>(Prefix + ".Ability", () => {
                var value = ScriptableObject.CreateInstance<BlueprintAbility>();
                value.name = Name("Ability");
                value.Type = AbilityType.Supernatural;
                value.ActionType = UnitCommand.CommandType.Standard; value.SetIsFullRoundAction(false);
                value.Range = AbilityRange.Touch;
                value.CanTargetPoint = true;
                value.CanTargetSelf = value.CanTargetFriends = value.CanTargetEnemies = false;
                value.SpellResistance = false; value.NeedEquipWeapons = false;
                value.Animation = UnitAnimationActionCastSpell.CastAnimationStyle.Immediate;
                value.MaterialComponent = new BlueprintAbility.MaterialComponentData();
                value.ResourceAssetIds = Array.Empty<string>();
                value.LocalizedDuration = LocalizationService.Create(Prefix + ".Duration", "1 minute per character level");
                value.LocalizedSavingThrow = LocalizationService.Create(Prefix + ".SavingThrow", "None");
                BlueprintUnitFactAccess.Resolve().Configure(value,
                    LocalizationService.Create(Prefix + ".Ability.Name", "Treacherous Earth"),
                    LocalizationService.Create(Prefix + ".Ability.Description", ElementalTreacherousPolicy.Description), icon);
                var action = ScriptableObject.CreateInstance<ElementalTreacherousActivate>(); action.Area = area; action.Ability = value;
                var ground = ScriptableObject.CreateInstance<ElementalTreacherousGroundTarget>(); ground.Ability = value;
                var commit = ScriptableObject.CreateInstance<ElementalHydraulicResourceCommit>(); commit.Resource = resource;
                var effect = ScriptableObject.CreateInstance<AbilityEffectRunAction>();
                effect.Actions = new ActionList { Actions = new GameAction[] { commit, action } };
                value.ComponentsArray = new BlueprintComponent[] {
                    ground,
                    ElementalRaceAbilityFactory.ResourceCost(resource, true), effect
                };
                return ElementalComponentIdentity.Prepare(value);
            });
            var parameters = ScriptableObject.CreateInstance<ElementalTreacherousParameters>();
            parameters.Ability = ability; parameters.Area = area;
            return new ElementalTraitDailyAbilityBlueprints {
                Resource = resource, Ability = ability, ParameterComponent = parameters,
                Mechanics = new BlueprintScriptableObject[] { resource, ability, terrain, area }
            };
        }
        internal static void Bind(ElementalTraitDailyAbilityBlueprints daily, BlueprintFeature marker)
        {
            if (daily == null || !(daily.ParameterComponent is ElementalTreacherousParameters)) return;
            ((ElementalTreacherousParameters)daily.ParameterComponent).Marker = marker;
            daily.Mechanics.OfType<BlueprintAbilityAreaEffect>().Single().GetComponent<ElementalTreacherousArea>().Marker = marker;
        }
        internal static bool IsExactEffect(ElementalAlternateTraitBlueprints trait)
        {
            var all = trait.Mechanics().ToArray();
            if (all.Length != 4 || all.OfType<BlueprintAbility>().Count() != 1 ||
                all.OfType<BlueprintAbilityResource>().Count() != 1 || all.OfType<BlueprintBuff>().Count() != 1 ||
                all.OfType<BlueprintAbilityAreaEffect>().Count() != 1) return false;
            var ability = all.OfType<BlueprintAbility>().Single();
            var resource = all.OfType<BlueprintAbilityResource>().Single();
            var buff = all.OfType<BlueprintBuff>().Single();
            var area = all.OfType<BlueprintAbilityAreaEffect>().Single();
            var cost = ability.GetComponent<AbilityResourceLogic>();
            var effect = ability.GetComponent<AbilityEffectRunAction>();
            var parameters = trait.Provider.GetComponent<ElementalTreacherousParameters>();
            var provider = trait.Provider.ComponentsArray;
            var grants = provider.OfType<AddFacts>().SingleOrDefault();
            var addResource = provider.OfType<AddAbilityResources>().SingleOrDefault();
            var ledger = provider.OfType<ElementalTraitDailyResourceState>().SingleOrDefault();
            if (provider.Length != 5 || grants == null || addResource == null || ledger == null || parameters == null)
                return false;
            return ability.Type == AbilityType.Supernatural && ability.Range == AbilityRange.Touch &&
                ability.ActionType == UnitCommand.CommandType.Standard && !ability.IsFullRoundAction &&
                ability.CanTargetPoint && !ability.SpellResistance && ability.GetComponent<SpellComponent>() == null &&
                ability.GetComponent<ElementalTreacherousGroundTarget>() != null &&
                ReferenceEquals(ability.GetComponent<ElementalTreacherousGroundTarget>().Ability, ability) &&
                ability.ComponentsArray.Length == 3 && !ability.CanTargetSelf && !ability.CanTargetFriends && !ability.CanTargetEnemies &&
                cost != null && ReferenceEquals(cost.RequiredResource, resource) && cost.Amount == 1 && cost.IsSpendResource &&
                effect != null && effect.Actions.Actions.Length == 2 &&
                effect.Actions.Actions[0] is ElementalHydraulicResourceCommit &&
                effect.Actions.Actions[1] is ElementalTreacherousActivate &&
                ReferenceEquals(((ElementalTreacherousActivate)effect.Actions.Actions[1]).Area, area) &&
                ReferenceEquals(((ElementalTreacherousActivate)effect.Actions.Actions[1]).Ability, ability) &&
                ReferenceEquals(((ElementalHydraulicResourceCommit)effect.Actions.Actions[0]).Resource, resource) &&
                area.Size.Equals(10.Feet()) && area.Shape == AreaEffectShape.Cylinder && !area.SpellResistance &&
                area.AffectEnemies && !area.AggroEnemies && !area.AffectDead && !area.IgnoreSleepingUnits &&
                area.GetComponent<SpellDescriptorComponent>().Descriptor == SpellDescriptor.Ground &&
                area.ComponentsArray.Length == 2 && area.GetComponent<ElementalTreacherousArea>() != null &&
                ReferenceEquals(area.GetComponent<ElementalTreacherousArea>().Terrain, buff) &&
                ReferenceEquals(area.GetComponent<ElementalTreacherousArea>().Marker, trait.Marker) &&
                buff.Stacking == StackingType.Stack && buff.ComponentsArray.Length == 2 &&
                buff.GetComponent<AddCondition>().Condition == UnitCondition.DifficultTerrain &&
                buff.GetComponent<SpellDescriptorComponent>().Descriptor == SpellDescriptor.Ground &&
                parameters != null && ReferenceEquals(parameters.Ability, ability) &&
                ReferenceEquals(parameters.Area, area) && ReferenceEquals(parameters.Marker, trait.Marker) &&
                grants.Facts != null && grants.Facts.Length == 1 && ReferenceEquals(grants.Facts[0], ability) &&
                !grants.DoNotRestoreMissingFacts && ReferenceEquals(addResource.Resource, resource) &&
                !addResource.UseThisAsResource && addResource.Amount == 0 && addResource.RestoreAmount && !addResource.RestoreOnLevelUp &&
                ReferenceEquals(ledger.Resource, resource) &&
                ReferenceEquals(provider[0], grants) && ReferenceEquals(provider[1], addResource) &&
                ReferenceEquals(provider[2], ledger) && ReferenceEquals(provider[3], parameters) &&
                provider[4] is ElementalAlternateTraitProviderController &&
                ((ElementalAlternateTraitProviderController)provider[4]).Trait == (int)ElementalAlternateTraitId.TreacherousEarth &&
                ExactSingleUseResource(resource);
        }
        private static bool ExactSingleUseResource(BlueprintAbilityResource resource)
        {
            var field = typeof(BlueprintAbilityResource).GetField("m_MaxAmount",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            if (field == null) return false;
            object amount = field.GetValue(resource);
            var type = field.FieldType;
            var baseValue = type.GetField("BaseValue");
            if (baseValue == null || !Equals(baseValue.GetValue(amount), 1)) return false;
            foreach (string name in new[] { "IncreasedByLevel", "IncreasedByLevelStartPlusDivStep", "IncreasedByStat" })
            {
                var scale = type.GetField(name);
                if (scale == null || !Equals(scale.GetValue(amount), false)) return false;
            }
            return true;
        }

        private static SpellDescriptorComponent Ground()
        {
            var value = ScriptableObject.CreateInstance<SpellDescriptorComponent>();
            value.Descriptor = SpellDescriptor.Ground; return value;
        }
        private static string Name(string suffix) { return (Prefix + "." + suffix).Replace('.', '_'); }
    }
}
