using System;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.ResourceLinks;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.TargetCheckers;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.Utility;
using Kingmaker.Visual.Animation.Kingmaker.Actions;
using KingmakerGunslinger.Blueprints;
using UnityEngine;

namespace KingmakerGunslinger.ElementalRaces
{
    internal static class ElementalNereidFactory
    {
        internal const string Prefix = "KMG.ElementalRaces.Traits.Undine.NereidFascination";
        internal const string ShakeFreeGuid = "e118e1e0a17a4acec001000000000004";
        internal const string PersonDonorGuid = "c7104f7526c4c524f91474614054547e";
        internal static readonly string[] NativeExcludedTypes = {
            "3bec99efd9a363242a6c8d9957b75e91",
            "a95311b3dc996964cbaa30ff9965aaf6",
            "fd389783027d63343b4a5634bd81645f",
            "455ac88e22f55804ab87c2467deff1d6",
            "018af8005220ac94a9a4f47b3e9c2b4e",
            "625827490ea69d84d8e599a33929fdc6",
            "57614b50e8d86b24395931fffc5e409b",
            "9054d3988d491d944ac144e27b6bc318",
            "706e61781d692a042b35941f14bc41c5",
            "09478937695300944a179530664e42ec",
            "734a29b693e9ec346ba2951b27987e33"
        };
        internal static readonly string[] Suffixes = { "Resource", "Ability", "FascinatedBuff",
            "ShakeFreeAbility", "AssistanceBuff", "AuraBuff", "Area" };

        internal static ElementalTraitDailyAbilityBlueprints Register(LibraryScriptableObject library,
            BlueprintRegistry registry, ElementalAlternateTraitId trait, Sprite icon)
        {
            if (trait != ElementalAlternateTraitId.NereidFascination) return null;
            // The qualified native person contract excludes these exact native
            // creature-type facts. Keep this supernatural checker independent
            // of mutable spell components and their bloodline/pet exceptions.
            var excludedTypes = NativeExcludedTypes.Select(guid =>
                BlueprintLibraryLookup.RequireExact<BlueprintFeature>(library, guid,
                    "native humanoid targeting exclusion")).ToArray();
            var person = ScriptableObject.CreateInstance<AbilityTargetHasFact>();
            person.name = "$KMG.Elemental.Nereid.HumanoidTypes";
            person.Inverted = true;
            person.CheckedFacts = excludedTypes;

            BlueprintAbilityResource resource = registry.Register<BlueprintAbilityResource>(Prefix + ".Resource", () => {
                var value = ScriptableObject.CreateInstance<BlueprintAbilityResource>();
                value.name = Name("Resource");
                value.LocalizedName = LocalizationService.Create(Prefix + ".Resource.Name", "Nereid Fascination Uses");
                value.LocalizedDescription = LocalizationService.Create(Prefix + ".Resource.Description", ElementalNereidPolicy.Description);
                ElementalRaceAbilityFactory.ConfigureBaseAmount(value, 1);
                return value;
            });
            BlueprintBuff fascinated = registry.Register<BlueprintBuff>(Prefix + ".FascinatedBuff", () => {
                var value = Buff("FascinatedBuff", "Nereid Fascination", icon);
                var condition = ScriptableObject.CreateInstance<AddCondition>();
                condition.Condition = UnitCondition.Dazed; // Exact native Fascinate condition implementation.
                var perception = ScriptableObject.CreateInstance<ElementalNereidPerception>();
                perception.name = "$KMG.Elemental.Nereid.Perception";
                value.ComponentsArray = new BlueprintComponent[] { Descriptor(), condition, perception,
                    ScriptableObject.CreateInstance<ElementalNereidFascinated>() };
                return ElementalComponentIdentity.Prepare(value);
            });
            BlueprintAbility shake = registry.Register<BlueprintAbility>(Prefix + ".ShakeFreeAbility", () => {
                var value = Ability("ShakeFreeAbility", "Shake Free", false, icon,
                    "As a standard action at touch range, shake an ally free of Nereid Fascination.");
                value.Type = AbilityType.Extraordinary;
                value.LocalizedDuration = LocalizationService.Create(Prefix + ".ShakeDuration", "Instantaneous");
                value.LocalizedSavingThrow = LocalizationService.Create(Prefix + ".ShakeSavingThrow", "None");
                var target = ScriptableObject.CreateInstance<ElementalNereidShakeTarget>();
                target.Fascinated = fascinated;
                var action = ScriptableObject.CreateInstance<ElementalNereidShakeFree>();
                action.Fascinated = fascinated;
                value.ComponentsArray = new BlueprintComponent[] { target, Effect(action) };
                return ElementalComponentIdentity.Prepare(value);
            });
            BlueprintBuff assistance = registry.Register<BlueprintBuff>(Prefix + ".AssistanceBuff", () => {
                var value = Buff("AssistanceBuff", "Shake Free", icon);
                var facts = ScriptableObject.CreateInstance<AddFacts>();
                facts.Facts = new BlueprintUnitFact[] { shake };
                facts.DoNotRestoreMissingFacts = false;
                var ownership = ScriptableObject.CreateInstance<ElementalNereidAssistance>();
                ownership.name = "$KMG.Elemental.Nereid.AssistanceOwner";
                value.ComponentsArray = new BlueprintComponent[] { facts, ownership };
                return ElementalComponentIdentity.Prepare(value);
            });
            BlueprintBuff aura = registry.Register<BlueprintBuff>(Prefix + ".AuraBuff", () => {
                var value = Buff("AuraBuff", "Nereid Fascination Aura", icon);
                value.Stacking = StackingType.Replace;
                var state = ScriptableObject.CreateInstance<ElementalNereidAuraState>();
                state.name = "$KMG.Elemental.Nereid.AuraResponses";
                state.Fascinated = fascinated;
                state.Assistance = assistance;
                var attached = ScriptableObject.CreateInstance<AddAreaEffect>();
                attached.name = "$KMG.Elemental.Nereid.AttachedArea";
                value.ComponentsArray = new BlueprintComponent[] { state, attached };
                return ElementalComponentIdentity.Prepare(value);
            });
            BlueprintAbilityAreaEffect area = registry.Register<BlueprintAbilityAreaEffect>(Prefix + ".Area", () => {
                var value = ScriptableObject.CreateInstance<BlueprintAbilityAreaEffect>();
                value.name = Name("Area");
                value.Shape = AreaEffectShape.Cylinder;
                value.Size = ElementalNereidPolicy.RadiusFeet.Feet();
                value.SpellResistance = false;
                value.AffectEnemies = true;
                value.AggroEnemies = false;
                value.AffectDead = false;
                value.IgnoreSleepingUnits = false;
                value.Fx = new PrefabLink();
                var logic = ScriptableObject.CreateInstance<ElementalNereidArea>();
                logic.Aura = aura;
                logic.Fascinated = fascinated;
                logic.Assistance = assistance;
                logic.PersonCheck = person;
                value.ComponentsArray = new BlueprintComponent[] { Descriptor(), logic };
                return ElementalComponentIdentity.Prepare(value);
            });
            assistance.GetComponent<ElementalNereidAssistance>().Aura = aura;
            aura.GetComponent<AddAreaEffect>().AreaEffect = area;
            fascinated.GetComponent<ElementalNereidFascinated>().Aura = aura;
            BlueprintAbility ability = registry.Register<BlueprintAbility>(Prefix + ".Ability", () => {
                var value = Ability("Ability", "Nereid Fascination", true, icon);
                var action = ScriptableObject.CreateInstance<ElementalNereidActivate>();
                action.Aura = aura;
                var commit = ScriptableObject.CreateInstance<ElementalHydraulicResourceCommit>();
                commit.Resource = resource;
                value.ComponentsArray = new BlueprintComponent[] { Descriptor(),
                    ElementalRaceAbilityFactory.ResourceCost(resource, true), Effect(commit, action) };
                return ElementalComponentIdentity.Prepare(value);
            });
            var parameters = ScriptableObject.CreateInstance<ElementalNereidParameters>();
            parameters.Ability = ability;
            return new ElementalTraitDailyAbilityBlueprints {
                Resource = resource, Ability = ability, ParameterComponent = parameters,
                Mechanics = new BlueprintScriptableObject[] { resource, ability, fascinated, shake, assistance, aura, area }
            };
        }

        internal static void Bind(ElementalTraitDailyAbilityBlueprints daily, BlueprintFeature marker)
        {
            if (daily == null) return;
            foreach (var buff in daily.Mechanics.OfType<BlueprintBuff>())
            {
                var state = buff.GetComponent<ElementalNereidAuraState>();
                if (state != null) state.Marker = marker;
            }
        }

        internal static bool IsExact(ElementalAlternateTraitBlueprints trait)
        {
            var inventory = trait.Mechanics().ToArray();
            var resources = inventory.OfType<BlueprintAbilityResource>().ToArray();
            var abilities = inventory.OfType<BlueprintAbility>().ToArray();
            var buffs = inventory.OfType<BlueprintBuff>().ToArray();
            var areas = inventory.OfType<BlueprintAbilityAreaEffect>().ToArray();
            if (inventory.Length != 7 || resources.Length != 1 || abilities.Length != 2 ||
                buffs.Length != 3 || areas.Length != 1) return false;
            var main = abilities.SingleOrDefault(value => value.Type == AbilityType.Supernatural);
            var shake = abilities.SingleOrDefault(value => value.Type == AbilityType.Extraordinary);
            var aura = buffs.SingleOrDefault(value => value.GetComponent<ElementalNereidAuraState>() != null);
            var target = buffs.SingleOrDefault(value => value.GetComponent<ElementalNereidFascinated>() != null);
            var area = areas[0];
            if (main == null || shake == null || aura == null || target == null) return false;
            var cost = main.GetComponent<AbilityResourceLogic>();
            var effect = main.GetComponent<AbilityEffectRunAction>();
            var state = aura.GetComponent<ElementalNereidAuraState>();
            var logic = area.GetComponent<ElementalNereidArea>();
            var provider = trait.Provider.ComponentsArray;
            var assistance = state.Assistance;
            var attached = aura.GetComponent<AddAreaEffect>();
            var condition = target.GetComponent<AddCondition>();
            var shakeTarget = shake.GetComponent<ElementalNereidShakeTarget>();
            var shakeEffect = shake.GetComponent<AbilityEffectRunAction>();
            var grants = provider.OfType<AddFacts>().SingleOrDefault();
            var addResource = provider.OfType<AddAbilityResources>().SingleOrDefault();
            var parameters = provider.OfType<ElementalNereidParameters>().SingleOrDefault();
            var ledger = provider.OfType<ElementalTraitDailyResourceState>().SingleOrDefault();
            var targetOwner = target.GetComponent<ElementalNereidFascinated>();
            var helpers = assistance == null ? null : assistance.GetComponent<AddFacts>();
            if (provider.Length != 5 || grants == null || addResource == null || parameters == null ||
                ledger == null || targetOwner == null || assistance == null || helpers == null ||
                attached == null || condition == null || shakeTarget == null || shakeEffect == null) return false;

            return main.ActionType == UnitCommand.CommandType.Standard && !main.IsFullRoundAction &&
                main.Range == AbilityRange.Personal && main.CanTargetSelf && !main.CanTargetFriends && !main.CanTargetEnemies &&
                !main.CanTargetPoint && !main.NeedEquipWeapons && !main.SpellResistance &&
                main.SpellDescriptor == SpellDescriptor.MindAffecting && main.GetComponent<SpellComponent>() == null &&
                cost != null && ReferenceEquals(cost.RequiredResource, resources[0]) && cost.Amount == 1 && cost.IsSpendResource &&
                effect != null && effect.Actions.Actions.Length == 2 &&
                effect.Actions.Actions[0] is ElementalHydraulicResourceCommit &&
                ReferenceEquals(((ElementalHydraulicResourceCommit)effect.Actions.Actions[0]).Resource, resources[0]) &&
                effect.Actions.Actions[1] is ElementalNereidActivate &&
                ReferenceEquals(((ElementalNereidActivate)effect.Actions.Actions[1]).Aura, aura) &&
                area.Size.Equals(20.Feet()) && area.Shape == AreaEffectShape.Cylinder && !area.SpellResistance &&
                logic != null && logic.PersonCheck != null && logic.PersonCheck.Inverted &&
                logic.PersonCheck.CheckedFacts != null && logic.PersonCheck.CheckedFacts.Length == 11 &&
                logic.PersonCheck.name == "$KMG.Elemental.Nereid.HumanoidTypes" &&
                logic.PersonCheck.CheckedFacts.All(value => value != null) &&
                logic.PersonCheck.CheckedFacts.Select(value => value.AssetGuid).OrderBy(value => value, StringComparer.Ordinal)
                    .SequenceEqual(NativeExcludedTypes.OrderBy(value => value, StringComparer.Ordinal)) &&
                ReferenceEquals(logic.Aura, aura) && ReferenceEquals(logic.Fascinated, target) &&
                ReferenceEquals(logic.Assistance, assistance) && area.AffectEnemies && !area.AggroEnemies && !area.AffectDead &&
                ReferenceEquals(attached.AreaEffect, area) &&
                state.name == "$KMG.Elemental.Nereid.AuraResponses" &&
                attached.name == "$KMG.Elemental.Nereid.AttachedArea" &&
                ReferenceEquals(state.Marker, trait.Marker) && ReferenceEquals(state.Fascinated, target) &&
                state.Assistance != null && state.Assistance.GetComponent<ElementalNereidAssistance>() != null &&
                ReferenceEquals(state.Assistance.GetComponent<ElementalNereidAssistance>().Aura, aura) &&
                condition.Condition == UnitCondition.Dazed && ReferenceEquals(targetOwner.Aura, aura) &&
                aura.Stacking == StackingType.Replace && target.Stacking == StackingType.Stack &&
                assistance.Stacking == StackingType.Stack && helpers.Facts != null && helpers.Facts.Length == 1 &&
                ReferenceEquals(helpers.Facts[0], shake) && !helpers.DoNotRestoreMissingFacts &&
                target.GetComponent<ElementalNereidPerception>() != null &&
                target.GetComponent<AddStatBonus>() == null &&
                target.GetComponent<SpellDescriptorComponent>().Descriptor == SpellDescriptor.MindAffecting &&
                shake.Range == AbilityRange.Touch && shake.ActionType == UnitCommand.CommandType.Standard &&
                !shake.CanTargetSelf && shake.CanTargetFriends && !shake.CanTargetEnemies && !shake.CanTargetPoint &&
                !shake.SpellResistance && !shake.IsFullRoundAction && shake.GetComponent<AbilityResourceLogic>() == null &&
                ReferenceEquals(shakeTarget.Fascinated, target) && shakeEffect.Actions.Actions.Length == 1 &&
                shakeEffect.Actions.Actions[0] is ElementalNereidShakeFree &&
                ReferenceEquals(((ElementalNereidShakeFree)shakeEffect.Actions.Actions[0]).Fascinated, target) &&
                grants.Facts != null && grants.Facts.Length == 1 && ReferenceEquals(grants.Facts[0], main) &&
                !grants.DoNotRestoreMissingFacts && ReferenceEquals(addResource.Resource, resources[0]) &&
                !addResource.UseThisAsResource && addResource.Amount == 0 && addResource.RestoreAmount && !addResource.RestoreOnLevelUp &&
                ReferenceEquals(parameters.Ability, main) && ReferenceEquals(ledger.Resource, resources[0]) &&
                ReferenceEquals(provider[0], grants) && ReferenceEquals(provider[1], addResource) &&
                ReferenceEquals(provider[2], ledger) && ReferenceEquals(provider[3], parameters) &&
                provider[4] is ElementalAlternateTraitProviderController &&
                ((ElementalAlternateTraitProviderController)provider[4]).Trait == (int)ElementalAlternateTraitId.NereidFascination &&
                ExactSingleUseResource(resources[0]);
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

        private static SpellDescriptorComponent Descriptor()
        {
            var value = ScriptableObject.CreateInstance<SpellDescriptorComponent>();
            value.Descriptor = SpellDescriptor.MindAffecting;
            return value;
        }
        private static BlueprintBuff Buff(string suffix, string title, Sprite icon)
        {
            var value = ScriptableObject.CreateInstance<BlueprintBuff>();
            value.name = Name(suffix);
            value.Stacking = StackingType.Stack;
            value.FxOnStart = new PrefabLink(); value.FxOnRemove = new PrefabLink();
            value.ResourceAssetIds = Array.Empty<string>();
            BlueprintUnitFactAccess.Resolve().Configure(value,
                LocalizationService.Create(Prefix + "." + suffix + ".Name", title),
                LocalizationService.Create(Prefix + "." + suffix + ".Description", ElementalNereidPolicy.Description), icon);
            return value;
        }
        private static BlueprintAbility Ability(string suffix, string title, bool personal, Sprite icon,
            string description = null)
        {
            var value = ScriptableObject.CreateInstance<BlueprintAbility>();
            value.name = Name(suffix);
            value.Type = AbilityType.Supernatural;
            value.ActionType = UnitCommand.CommandType.Standard;
            value.SetIsFullRoundAction(false);
            value.Range = personal ? AbilityRange.Personal : AbilityRange.Touch;
            value.CanTargetSelf = personal;
            value.CanTargetFriends = !personal;
            value.CanTargetEnemies = value.CanTargetPoint = false;
            value.SpellResistance = false;
            value.NeedEquipWeapons = false;
            value.EffectOnAlly = AbilityEffectOnUnit.Helpful;
            value.EffectOnEnemy = AbilityEffectOnUnit.None;
            value.Animation = UnitAnimationActionCastSpell.CastAnimationStyle.Immediate;
            value.MaterialComponent = new BlueprintAbility.MaterialComponentData();
            value.ResourceAssetIds = Array.Empty<string>();
            value.LocalizedDuration = LocalizationService.Create(Prefix + ".Duration", "Half character level in rounds (minimum 1)");
            value.LocalizedSavingThrow = LocalizationService.Create(Prefix + ".SavingThrow", "Will negates");
            BlueprintUnitFactAccess.Resolve().Configure(value,
                LocalizationService.Create(Prefix + "." + suffix + ".Name", title),
                LocalizationService.Create(Prefix + "." + suffix + ".Description", description ?? ElementalNereidPolicy.Description), icon);
            return value;
        }
        private static AbilityEffectRunAction Effect(params GameAction[] actions)
        {
            var effect = ScriptableObject.CreateInstance<AbilityEffectRunAction>();
            effect.Actions = new ActionList { Actions = actions };
            return effect;
        }
        private static string Name(string suffix) { return (Prefix + "." + suffix).Replace('.', '_'); }
    }
}
