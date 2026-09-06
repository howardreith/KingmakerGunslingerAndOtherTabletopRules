using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.RuleSystem;
using Kingmaker.UI.GenericSlot;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.Visual.Animation.Kingmaker.Actions;
using KingmakerGunslinger.Blueprints;
using UnityEngine;

namespace KingmakerGunslinger.ElementalRaces
{
    internal sealed class ElementalHeritageSlaBlueprints
    {
        internal ElementalHeritageSlaBlueprints(
            BlueprintAbilityResource resource, BlueprintAbility ability,
            BlueprintFeature feature,
            IEnumerable<BlueprintScriptableObject> auxiliaryBlueprints)
        {
            Resource = resource ?? throw new ArgumentNullException("resource");
            Ability = ability ?? throw new ArgumentNullException("ability");
            Feature = feature ?? throw new ArgumentNullException("feature");
            AuxiliaryBlueprints = auxiliaryBlueprints == null
                ? new BlueprintScriptableObject[0]
                : auxiliaryBlueprints.ToArray();
            if (AuxiliaryBlueprints.Any(value => value == null))
                throw new ArgumentException(
                    "Heritage SLA auxiliary blueprints must be non-null.");
        }

        internal BlueprintAbilityResource Resource { get; private set; }
        internal BlueprintAbility Ability { get; private set; }
        internal BlueprintFeature Feature { get; private set; }
        internal BlueprintScriptableObject[] AuxiliaryBlueprints
        { get; private set; }
    }

    internal static class ElementalHeritageAbilityFactory
    {
        private const string NativeShockingGraspDeliveryGuid =
            "17451c1327c571641a1345bd31155209";
        private const BindingFlags PrivateInstance = BindingFlags.Instance |
            BindingFlags.NonPublic;

        internal static ElementalHeritageSlaBlueprints Register(
            LibraryScriptableObject library, BlueprintRegistry registry,
            ElementalHeritageDefinition definition, Sprite fallbackIcon)
        {
            if (library == null) throw new ArgumentNullException("library");
            if (registry == null) throw new ArgumentNullException("registry");
            if (definition == null)
                throw new ArgumentNullException("definition");
            if (definition.IsGeneral)
                throw new ArgumentException(
                    "General heritage must reuse the legacy SLA graph.");
            if (fallbackIcon == null)
                throw new ArgumentNullException("fallbackIcon");

            BlueprintAbilityResource resource = registry.Register<
                BlueprintAbilityResource>(definition.SlaResourceSymbol,
                    () => CreateResource(definition));
            BlueprintAbility ability;
            BlueprintScriptableObject[] auxiliary;
            switch (definition.AbilityImplementation)
            {
                case ElementalHeritageAbilityImplementation.NativeSpellClone:
                    RegisterNative(library, registry, definition, resource,
                        fallbackIcon, out ability, out auxiliary);
                    break;
                case ElementalHeritageAbilityImplementation.UnerringWeapon:
                    RegisterUnerring(registry, definition, resource,
                        fallbackIcon, out ability, out auxiliary);
                    break;
                case ElementalHeritageAbilityImplementation.ChillTouch:
                    RegisterChillTouch(library, registry, definition, resource,
                        fallbackIcon, out ability, out auxiliary);
                    break;
                default:
                    throw new InvalidOperationException(
                        "Unsupported alternate heritage SLA implementation: " +
                        definition.AbilityImplementation);
            }
            BlueprintFeature feature = registry.Register<BlueprintFeature>(
                definition.SlaFeatureSymbol,
                () => CreateFeature(definition, resource, ability));
            ValidateCommon(definition, resource, ability, feature,
                fallbackIcon);
            return new ElementalHeritageSlaBlueprints(resource, ability,
                feature, auxiliary);
        }

        private static void RegisterNative(LibraryScriptableObject library,
            BlueprintRegistry registry, ElementalHeritageDefinition definition,
            BlueprintAbilityResource resource, Sprite fallbackIcon,
            out BlueprintAbility ability,
            out BlueprintScriptableObject[] auxiliary)
        {
            BlueprintAbility donor = BlueprintLibraryLookup.RequireExact<
                BlueprintAbility>(library, definition.DonorAbilityGuid,
                    "native " + definition.SlaName +
                    " donor for alternate elemental heritage");
            if (definition.Id != ElementalHeritageId.Stormsoul)
            {
                ability = registry.Register<BlueprintAbility>(
                    definition.SlaAbilitySymbol,
                    () => CloneNativeSpell(definition, donor, resource,
                        fallbackIcon, null));
                auxiliary = new BlueprintScriptableObject[0];
                return;
            }

            BlueprintAbility donorDelivery = BlueprintLibraryLookup
                .RequireExact<BlueprintAbility>(library,
                    NativeShockingGraspDeliveryGuid,
                    "native Shocking Grasp held-touch delivery donor");
            BlueprintAbility delivery = registry.Register<BlueprintAbility>(
                ElementalRaceIdentityCatalog.ShockingGraspDeliveryAbility,
                () => CloneNativeDelivery(definition, donorDelivery,
                    fallbackIcon));
            ability = registry.Register<BlueprintAbility>(
                definition.SlaAbilitySymbol,
                () => CloneNativeSpell(definition, donor, resource,
                    fallbackIcon, delivery));
            delivery.Parent = ability;
            auxiliary = new BlueprintScriptableObject[] { delivery };
            AbilityEffectStickyTouch sticky = ability.ComponentsArray
                .OfType<AbilityEffectStickyTouch>().Single();
            if (!ReferenceEquals(sticky.TouchDeliveryAbility, delivery) ||
                !ReferenceEquals(delivery.Parent, ability))
                throw new InvalidOperationException(
                    "Stormsoul Shocking Grasp delivery graph is incomplete.");
        }

        private static void RegisterUnerring(BlueprintRegistry registry,
            ElementalHeritageDefinition definition,
            BlueprintAbilityResource resource, Sprite icon,
            out BlueprintAbility ability,
            out BlueprintScriptableObject[] auxiliary)
        {
            BlueprintWeaponEnchantment enchantment = registry.Register<
                BlueprintWeaponEnchantment>(
                    ElementalRaceIdentityCatalog.UnerringWeaponEnchantment,
                    CreateUnerringEnchantment);
            BlueprintAbility primary = registry.Register<BlueprintAbility>(
                ElementalRaceIdentityCatalog.UnerringWeaponPrimaryAbility,
                () => CreateUnerringVariant(definition, resource,
                    enchantment, false, icon));
            BlueprintAbility secondary = registry.Register<BlueprintAbility>(
                ElementalRaceIdentityCatalog.UnerringWeaponSecondaryAbility,
                () => CreateUnerringVariant(definition, resource,
                    enchantment, true, icon));
            ability = registry.Register<BlueprintAbility>(
                definition.SlaAbilitySymbol,
                () => CreateUnerringParent(definition,
                    new[] { primary, secondary }, icon));
            primary.Parent = ability;
            secondary.Parent = ability;
            auxiliary = new BlueprintScriptableObject[]
            {
                primary, secondary, enchantment
            };
            ValidateUnerring(ability, primary, secondary, enchantment,
                resource);
        }

        private static void RegisterChillTouch(
            LibraryScriptableObject library, BlueprintRegistry registry,
            ElementalHeritageDefinition definition,
            BlueprintAbilityResource resource, Sprite fallbackIcon,
            out BlueprintAbility ability,
            out BlueprintScriptableObject[] auxiliary)
        {
            BlueprintAbility donorDelivery = BlueprintLibraryLookup
                .RequireExact<BlueprintAbility>(library,
                    NativeShockingGraspDeliveryGuid,
                    "native held-touch weapon delivery donor for Chill Touch");
            BlueprintAbility delivery = registry.Register<BlueprintAbility>(
                ElementalRaceIdentityCatalog.ChillTouchDeliveryAbility,
                () => CreateChillTouchDelivery(definition, donorDelivery,
                    fallbackIcon));
            ability = registry.Register<BlueprintAbility>(
                definition.SlaAbilitySymbol,
                () => CreateChillTouchParent(definition, delivery, resource,
                    fallbackIcon));
            delivery.Parent = ability;
            auxiliary = new BlueprintScriptableObject[] { delivery };
            ValidateChillTouch(ability, delivery, resource);
        }

        private static BlueprintAbilityResource CreateResource(
            ElementalHeritageDefinition definition)
        {
            var result = ScriptableObject.CreateInstance<
                BlueprintAbilityResource>();
            result.name = InternalName(definition.SlaResourceSymbol);
            result.LocalizedName = LocalizationService.Create(
                LocalizationKey(definition, "Resource.Name"),
                definition.SlaName + " Uses");
            result.LocalizedDescription = LocalizationService.Create(
                LocalizationKey(definition, "Resource.Description"),
                "One use per ordinary rest.");
            ElementalRaceAbilityFactory.ConfigureBaseAmount(result, 1);
            return result;
        }

        private static BlueprintAbility CloneNativeSpell(
            ElementalHeritageDefinition definition, BlueprintAbility donor,
            BlueprintAbilityResource resource, Sprite fallbackIcon,
            BlueprintAbility replacementDelivery)
        {
            BlueprintAbility result = BlueprintCloneService.Clone(donor,
                InternalName(definition.SlaAbilitySymbol));
            var components = new List<BlueprintComponent>();
            foreach (BlueprintComponent component in result.ComponentsArray ??
                Array.Empty<BlueprintComponent>())
            {
                if (!ElementalRaceAbilityFactory.IsSafeNativeEffect(component))
                    continue;
                AbilityEffectStickyTouch sticky = component as
                    AbilityEffectStickyTouch;
                if (sticky != null && replacementDelivery != null)
                {
                    sticky = UnityEngine.Object.Instantiate(sticky);
                    sticky.TouchDeliveryAbility = replacementDelivery;
                    components.Add(sticky);
                }
                else components.Add(component);
            }
            components.Add(ElementalRaceAbilityFactory.ResourceCost(resource,
                true));
            result.ComponentsArray = components.ToArray();
            ConfigureSpellLike(result, definition, donor.Icon ?? fallbackIcon);
            if (replacementDelivery != null && result.ComponentsArray
                    .OfType<AbilityEffectStickyTouch>().Count() != 1)
                throw new InvalidOperationException(
                    "The native sticky-touch donor graph drifted.");
            return result;
        }

        private static BlueprintAbility CloneNativeDelivery(
            ElementalHeritageDefinition definition, BlueprintAbility donor,
            Sprite fallbackIcon)
        {
            BlueprintAbility result = BlueprintCloneService.Clone(donor,
                InternalName(ElementalRaceIdentityCatalog
                    .ShockingGraspDeliveryAbility));
            result.ComponentsArray = (result.ComponentsArray ??
                Array.Empty<BlueprintComponent>()).Where(
                    ElementalRaceAbilityFactory.IsSafeNativeEffect).ToArray();
            ConfigureSpellLike(result, definition, donor.Icon ?? fallbackIcon);
            if (result.ComponentsArray.OfType<AbilityResourceLogic>().Any())
                throw new InvalidOperationException(
                    "A held Shocking Grasp delivery must not spend a second use.");
            return result;
        }

        private static BlueprintWeaponEnchantment CreateUnerringEnchantment()
        {
            var result = ScriptableObject.CreateInstance<
                BlueprintWeaponEnchantment>();
            result.name = InternalName(ElementalRaceIdentityCatalog
                .UnerringWeaponEnchantment);
            SetEnchantment(result, "m_EnchantName", LocalizationService.Create(
                "KMG.ElementalRaces.Oread.Ironsoul.UnerringWeapon.Enchantment.Name",
                "Unerring Weapon"));
            SetEnchantment(result, "m_Description", LocalizationService.Create(
                "KMG.ElementalRaces.Oread.Ironsoul.UnerringWeapon.Enchantment.Description",
                "This exact weapon gains a +2 critical-confirmation bonus, plus +1 per four caster levels, to a maximum of +7."));
            SetEnchantment(result, "m_EnchantmentCost", 0);
            var logic = ScriptableObject.CreateInstance<
                ElementalUnerringWeaponEnchantment>();
            logic.name = "$KMG_UnerringWeapon_ExactItem";
            result.ComponentsArray = new BlueprintComponent[] { logic };
            return result;
        }

        private static BlueprintAbility CreateUnerringParent(
            ElementalHeritageDefinition definition,
            BlueprintAbility[] variants, Sprite icon)
        {
            BlueprintAbility result = BaseAbility(
                definition.SlaAbilitySymbol, AbilityRange.Personal,
                UnitAnimationActionCastSpell.CastAnimationStyle.EnchantWeapon,
                icon);
            var choices = ScriptableObject.CreateInstance<AbilityVariants>();
            choices.Variants = (BlueprintAbility[])variants.Clone();
            result.ComponentsArray = new BlueprintComponent[] { choices };
            ConfigureText(result, definition, icon);
            return result;
        }

        private static BlueprintAbility CreateUnerringVariant(
            ElementalHeritageDefinition definition,
            BlueprintAbilityResource resource,
            BlueprintWeaponEnchantment enchantment, bool secondary,
            Sprite icon)
        {
            string symbol = secondary
                ? ElementalRaceIdentityCatalog.UnerringWeaponSecondaryAbility
                : ElementalRaceIdentityCatalog.UnerringWeaponPrimaryAbility;
            BlueprintAbility result = BaseAbility(symbol,
                AbilityRange.Personal,
                UnitAnimationActionCastSpell.CastAnimationStyle.EnchantWeapon,
                icon);
            var spell = ScriptableObject.CreateInstance<SpellComponent>();
            spell.School = SpellSchool.Transmutation;
            ContextRankConfig rank = CasterLevelRank(
                "$KMG_UnerringWeapon_CasterLevel");
            var checker = ScriptableObject.CreateInstance<
                ElementalUnerringWeaponTargetChecker>();
            checker.SecondaryHand = secondary;
            var enchant = ScriptableObject.CreateInstance<
                ContextActionEnchantWornItem>();
            enchant.Enchantment = enchantment;
            enchant.Slot = secondary ? EquipSlotBase.SlotType.SecondaryHand :
                EquipSlotBase.SlotType.PrimaryHand;
            enchant.Permanent = false;
            enchant.ToCaster = true;
            enchant.RemoveOnUnequip = false;
            enchant.DurationValue = new ContextDurationValue
            {
                Rate = DurationRate.Rounds,
                DiceType = DiceType.Zero,
                DiceCountValue = 0,
                BonusValue = new ContextValue
                {
                    ValueType = ContextValueType.Rank,
                    ValueRank = AbilityRankType.Default
                }
            };
            var effect = ScriptableObject.CreateInstance<
                AbilityEffectRunAction>();
            effect.Actions = new ActionList
            {
                Actions = new GameAction[] { enchant }
            };
            result.ComponentsArray = new BlueprintComponent[]
            {
                spell, rank,
                ElementalRaceAbilityFactory.ResourceCost(resource, true),
                checker, effect
            };
            string hand = secondary ? "Secondary Hand" : "Primary Hand";
            BlueprintUnitFactAccess.Resolve().Configure(result,
                LocalizationService.Create(LocalizationKey(definition,
                    "Ability." + hand.Replace(" ", string.Empty) + ".Name"),
                    definition.SlaName + " — " + hand),
                LocalizationService.Create(LocalizationKey(definition,
                    "Ability." + hand.Replace(" ", string.Empty) +
                    ".Description"), definition.SlaDescription), icon);
            result.LocalizedDuration = LocalizationService.Create(
                LocalizationKey(definition, "Ability.Duration"),
                "1 round per character level");
            return result;
        }

        private static BlueprintAbility CreateChillTouchParent(
            ElementalHeritageDefinition definition,
            BlueprintAbility delivery, BlueprintAbilityResource resource,
            Sprite icon)
        {
            BlueprintAbility result = BaseAbility(definition.SlaAbilitySymbol,
                AbilityRange.Touch,
                UnitAnimationActionCastSpell.CastAnimationStyle.Touch, icon);
            result.CanTargetSelf = false;
            result.CanTargetEnemies = true;
            result.EffectOnAlly = AbilityEffectOnUnit.None;
            result.EffectOnEnemy = AbilityEffectOnUnit.Harmful;
            result.SpellResistance = true;
            var spell = ScriptableObject.CreateInstance<SpellComponent>();
            spell.School = SpellSchool.Necromancy;
            var sticky = ScriptableObject.CreateInstance<
                ElementalChillTouchStickyTouch>();
            sticky.TouchDeliveryAbility = delivery;
            result.ComponentsArray = new BlueprintComponent[]
            {
                spell,
                ElementalRaceAbilityFactory.ResourceCost(resource, true),
                sticky
            };
            ConfigureText(result, definition, icon);
            result.LocalizedDuration = LocalizationService.Create(
                LocalizationKey(definition, "Ability.Duration"),
                "One touch per character level; see text");
            result.LocalizedSavingThrow = LocalizationService.Create(
                LocalizationKey(definition, "Ability.SavingThrow"),
                "Fortitude partial; Will negates for undead");
            return result;
        }

        private static BlueprintAbility CreateChillTouchDelivery(
            ElementalHeritageDefinition definition, BlueprintAbility donor,
            Sprite fallbackIcon)
        {
            AbilityDeliverTouch nativeDelivery = (donor.ComponentsArray ??
                Array.Empty<BlueprintComponent>()).OfType<
                    AbilityDeliverTouch>().Single();
            BlueprintAbility result = BlueprintCloneService.Clone(donor,
                InternalName(ElementalRaceIdentityCatalog
                    .ChillTouchDeliveryAbility));
            result.Type = AbilityType.SpellLike;
            result.CanTargetSelf = false;
            result.CanTargetFriends = false;
            result.CanTargetEnemies = true;
            result.CanTargetPoint = false;
            result.SpellResistance = true;
            result.EffectOnAlly = AbilityEffectOnUnit.None;
            result.EffectOnEnemy = AbilityEffectOnUnit.Harmful;
            result.MaterialComponent = new BlueprintAbility
                .MaterialComponentData();
            result.ResourceAssetIds = Array.Empty<string>();
            var spell = ScriptableObject.CreateInstance<SpellComponent>();
            spell.School = SpellSchool.Necromancy;
            var delivery = ScriptableObject.CreateInstance<
                AbilityDeliverTouch>();
            delivery.TouchWeapon = nativeDelivery.TouchWeapon;
            var effect = ScriptableObject.CreateInstance<
                AbilityEffectRunAction>();
            effect.SavingThrowType = SavingThrowType.Unknown;
            effect.Actions = new ActionList
            {
                Actions = new GameAction[]
                {
                    ScriptableObject.CreateInstance<
                        ContextActionElementalChillTouch>()
                }
            };
            result.ComponentsArray = new BlueprintComponent[]
            {
                spell, delivery, effect
            };
            ConfigureText(result, definition, donor.Icon ?? fallbackIcon);
            result.LocalizedDuration = LocalizationService.Create(
                LocalizationKey(definition, "Ability.Delivery.Duration"),
                "Instantaneous");
            result.LocalizedSavingThrow = LocalizationService.Create(
                LocalizationKey(definition, "Ability.Delivery.SavingThrow"),
                "Fortitude partial; Will negates for undead");
            return result;
        }

        private static BlueprintAbility BaseAbility(string symbol,
            AbilityRange range,
            UnitAnimationActionCastSpell.CastAnimationStyle animation,
            Sprite icon)
        {
            var result = ScriptableObject.CreateInstance<BlueprintAbility>();
            result.name = InternalName(symbol);
            result.Type = AbilityType.SpellLike;
            result.Parent = null;
            result.Hidden = false;
            result.ActionBarAutoFillIgnored = false;
            result.Range = range;
            result.CanTargetSelf = true;
            result.CanTargetFriends = false;
            result.CanTargetEnemies = false;
            result.CanTargetPoint = false;
            result.SpellResistance = false;
            result.NeedEquipWeapons = false;
            result.EffectOnAlly = AbilityEffectOnUnit.Helpful;
            result.EffectOnEnemy = AbilityEffectOnUnit.None;
            result.ActionType = UnitCommand.CommandType.Standard;
            result.Animation = animation;
            result.MaterialComponent = new BlueprintAbility
                .MaterialComponentData();
            result.ResourceAssetIds = Array.Empty<string>();
            result.LocalizedDuration = LocalizationService.Create(
                "KMG.ElementalRaces.Heritage.Ability.Instantaneous",
                "Instantaneous");
            result.LocalizedSavingThrow = LocalizationService.Create(
                "KMG.ElementalRaces.Heritage.Ability.NoSave", "None");
            if (icon == null)
                throw new InvalidOperationException(
                    "Every heritage ability requires a non-null icon.");
            return result;
        }

        private static void ConfigureSpellLike(BlueprintAbility result,
            ElementalHeritageDefinition definition, Sprite icon)
        {
            result.Type = AbilityType.SpellLike;
            result.Parent = null;
            result.Hidden = false;
            result.ActionBarAutoFillIgnored = false;
            result.MaterialComponent = new BlueprintAbility
                .MaterialComponentData();
            result.ResourceAssetIds = Array.Empty<string>();
            ConfigureText(result, definition, icon);
        }

        private static void ConfigureText(BlueprintAbility result,
            ElementalHeritageDefinition definition, Sprite icon)
        {
            if (icon == null)
                throw new InvalidOperationException(
                    definition.SlaName + " has no safe native icon.");
            BlueprintUnitFactAccess.Resolve().Configure(result,
                LocalizationService.Create(LocalizationKey(definition,
                    "Ability.Name"), definition.SlaName),
                LocalizationService.Create(LocalizationKey(definition,
                    "Ability.Description"), definition.SlaDescription), icon);
        }

        private static BlueprintFeature CreateFeature(
            ElementalHeritageDefinition definition,
            BlueprintAbilityResource resource, BlueprintAbility ability)
        {
            var result = ScriptableObject.CreateInstance<BlueprintFeature>();
            result.name = InternalName(definition.SlaFeatureSymbol);
            result.Ranks = 1;
            result.IsClassFeature = false;
            result.HideInUI = false;
            result.Groups = Array.Empty<FeatureGroup>();
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
            result.ComponentsArray = new BlueprintComponent[]
            {
                facts, add, parameters,
                ScriptableObject.CreateInstance<
                    ElementalOwnedProviderController>()
            };
            BlueprintUnitFactAccess.Resolve().Configure(result,
                LocalizationService.Create(LocalizationKey(definition,
                    "Feature.Name"), definition.SlaName),
                LocalizationService.Create(LocalizationKey(definition,
                    "Feature.Description"), definition.SlaDescription),
                ability.Icon);
            return ElementalComponentIdentity.Prepare(result);
        }

        private static ContextRankConfig CasterLevelRank(string name)
        {
            var result = ScriptableObject.CreateInstance<ContextRankConfig>();
            result.name = name;
            SetPrivate(result, "m_Type", AbilityRankType.Default);
            SetPrivate(result, "m_BaseValueType",
                ContextRankBaseValueType.CasterLevel);
            SetPrivate(result, "m_Progression", ContextRankProgression.AsIs);
            return result;
        }

        private static void ValidateCommon(
            ElementalHeritageDefinition definition,
            BlueprintAbilityResource resource, BlueprintAbility ability,
            BlueprintFeature feature, Sprite fallbackIcon)
        {
            AddFacts facts = feature.ComponentsArray.OfType<AddFacts>().Single();
            AddAbilityResources add = feature.ComponentsArray.OfType<
                AddAbilityResources>().Single();
            ElementalRacialSpellLikeParameters parameters =
                feature.ComponentsArray.OfType<
                    ElementalRacialSpellLikeParameters>().Single();
            if (ability.Type != AbilityType.SpellLike || ability.Icon == null ||
                fallbackIcon == null || ability.MaterialComponent == null ||
                ability.MaterialComponent.Item != null ||
                facts.Facts.Length != 1 ||
                !ReferenceEquals(facts.Facts[0], ability) ||
                !ReferenceEquals(add.Resource, resource) ||
                !add.RestoreAmount || add.RestoreOnLevelUp ||
                !ReferenceEquals(parameters.Ability, ability) ||
                parameters.Stat != StatType.Charisma ||
                parameters.SpellLevel != definition.SpellLevel)
                throw new InvalidOperationException(definition.Name +
                    " racial SLA graph failed deterministic validation.");
        }

        private static void ValidateUnerring(BlueprintAbility parent,
            BlueprintAbility primary, BlueprintAbility secondary,
            BlueprintWeaponEnchantment enchantment,
            BlueprintAbilityResource resource)
        {
            BlueprintAbility[] variants = parent.ComponentsArray.OfType<
                AbilityVariants>().Single().Variants;
            if (!variants.SequenceEqual(new[] { primary, secondary }) ||
                !ReferenceEquals(primary.Parent, parent) ||
                !ReferenceEquals(secondary.Parent, parent) ||
                enchantment.ComponentsArray.OfType<
                    ElementalUnerringWeaponEnchantment>().Count() != 1)
                throw new InvalidOperationException(
                    "Unerring Weapon parent or enchantment graph drifted.");
            foreach (BlueprintAbility variant in variants)
            {
                AbilityResourceLogic cost = variant.ComponentsArray.OfType<
                    AbilityResourceLogic>().Single();
                ContextActionEnchantWornItem action = variant.ComponentsArray
                    .OfType<AbilityEffectRunAction>().Single().Actions.Actions
                    .OfType<ContextActionEnchantWornItem>().Single();
                if (!ReferenceEquals(cost.RequiredResource, resource) ||
                    !ReferenceEquals(action.Enchantment, enchantment) ||
                    action.RemoveOnUnequip || action.Permanent ||
                    !action.ToCaster ||
                    action.DurationValue.Rate != DurationRate.Rounds)
                    throw new InvalidOperationException(
                        "Unerring Weapon variant graph drifted.");
            }
        }

        private static void ValidateChillTouch(BlueprintAbility parent,
            BlueprintAbility delivery, BlueprintAbilityResource resource)
        {
            ElementalChillTouchStickyTouch sticky = parent.ComponentsArray
                .OfType<ElementalChillTouchStickyTouch>().Single();
            AbilityResourceLogic cost = parent.ComponentsArray.OfType<
                AbilityResourceLogic>().Single();
            if (!ReferenceEquals(sticky.TouchDeliveryAbility, delivery) ||
                !ReferenceEquals(delivery.Parent, parent) ||
                !ReferenceEquals(cost.RequiredResource, resource) ||
                delivery.ComponentsArray.OfType<AbilityResourceLogic>().Any() ||
                delivery.ComponentsArray.OfType<AbilityEffectRunAction>()
                    .Single().Actions.Actions.OfType<
                        ContextActionElementalChillTouch>().Count() != 1)
                throw new InvalidOperationException(
                    "Chill Touch held-touch graph drifted.");
        }

        private static void SetEnchantment(BlueprintItemEnchantment target,
            string name, object value)
        {
            FieldInfo field = typeof(BlueprintItemEnchantment).GetField(name,
                PrivateInstance);
            if (field == null || (value != null &&
                    !field.FieldType.IsInstanceOfType(value)))
                throw new MissingFieldException(
                    typeof(BlueprintItemEnchantment).FullName, name);
            field.SetValue(target, value);
        }

        private static void SetPrivate(object target, string name,
            object value)
        {
            FieldInfo field = target.GetType().GetField(name,
                PrivateInstance);
            if (field == null)
                throw new MissingFieldException(target.GetType().FullName,
                    name);
            field.SetValue(target, value);
        }

        private static string LocalizationKey(
            ElementalHeritageDefinition definition, string suffix)
        {
            return "KMG.ElementalRaces.Heritage." + definition.Id + "." +
                suffix;
        }

        private static string InternalName(string symbol)
        {
            return symbol.Replace('.', '_');
        }
    }
}
