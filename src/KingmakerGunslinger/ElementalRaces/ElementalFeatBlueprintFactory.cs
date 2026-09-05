using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items.Armors;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.ResourceLinks;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.Visual.Animation.Kingmaker.Actions;
using KingmakerGunslinger.Blueprints;
using UnityEngine;

namespace KingmakerGunslinger.ElementalRaces
{
    internal static class ElementalFeatBlueprintFactory
    {
        private const BindingFlags PrivateInstance = BindingFlags.Instance |
            BindingFlags.NonPublic;

        internal static ElementalFeatBlueprintSet Register(
            BlueprintRegistry registry, ElementalRaceBlueprintSet races)
        {
            if (registry == null) throw new ArgumentNullException("registry");
            if (races == null) throw new ArgumentNullException("races");
            Sprite icon = races.Ifrit.SlaAbility.Icon;
            if (icon == null)
                throw new InvalidOperationException(
                    "Elemental feat publication requires a qualified native icon.");

            var registered = new List<BlueprintScriptableObject>();
            BlueprintBuff strikeBuff = Add(registered, registry.Register<
                BlueprintBuff>(ElementalRaceIdentityCatalog.ElementalStrikeBuff,
                    () => CreateBuff(ElementalRaceIdentityCatalog
                        .ElementalStrikeBuff, "Elemental Strike",
                        "Your qualifying weapon attacks deal the energy damage associated with your exact elemental race.", icon)));
            ConfigureElementalStrike(strikeBuff, races);
            BlueprintAbility strikeAbility = Add(registered, registry.Register<
                BlueprintAbility>(ElementalRaceIdentityCatalog
                    .ElementalStrikeAbility, () => CreateBuffAbility(
                        ElementalRaceIdentityCatalog.ElementalStrikeAbility,
                        "Elemental Strike", Description(
                            ElementalFeatId.ElementalStrike),
                        UnitCommand.CommandType.Swift, strikeBuff, icon)));

            BlueprintWeaponEnchantment scorchingEnchantment = Add(registered,
                registry.Register<BlueprintWeaponEnchantment>(
                    ElementalRaceIdentityCatalog.ScorchingWeaponsEnchantment,
                    CreateEnchantment));
            BlueprintBuff scorchingBuff = Add(registered, registry.Register<
                BlueprintBuff>(ElementalRaceIdentityCatalog.ScorchingWeaponsBuff,
                    () => CreateBuff(ElementalRaceIdentityCatalog
                        .ScorchingWeaponsBuff, "Scorching Weapons",
                        "The exact manufactured metallic weapons selected when this effect began are temporarily empowered.", icon)));
            BlueprintAbility scorchingAbility = Add(registered,
                registry.Register<BlueprintAbility>(
                    ElementalRaceIdentityCatalog.ScorchingWeaponsAbility,
                    () => CreateAbility(ElementalRaceIdentityCatalog
                        .ScorchingWeaponsAbility, "Scorching Weapons",
                        Description(ElementalFeatId.ScorchingWeapons),
                        UnitCommand.CommandType.Swift, AbilityType.Extraordinary,
                        AbilityRange.Personal, icon)));

            BlueprintBuff auraBuff = Add(registered, registry.Register<
                BlueprintBuff>(ElementalRaceIdentityCatalog.BlazingAuraBuff,
                    () => CreateBuff(ElementalRaceIdentityCatalog
                        .BlazingAuraBuff, "Blazing Aura",
                        "Any creature beginning its turn adjacent to you takes 1d6 fire damage.", icon)));
            BlueprintAbility auraAbility = Add(registered, registry.Register<
                BlueprintAbility>(ElementalRaceIdentityCatalog.BlazingAuraAbility,
                    () => CreateBuffAbility(ElementalRaceIdentityCatalog
                        .BlazingAuraAbility, "Blazing Aura", Description(
                            ElementalFeatId.BlazingAura),
                        UnitCommand.CommandType.Free, auraBuff, icon)));
            BlueprintBuff wingsBuff = Add(registered, registry.Register<
                BlueprintBuff>(ElementalRaceIdentityCatalog.WingsOfAirBuff,
                    () => CreateWingsBuff(icon)));

            BlueprintAbility hydraulicBullRush = Add(registered,
                registry.Register<BlueprintAbility>(ElementalRaceIdentityCatalog
                    .HydraulicBullRushAbility, () => CreateHydraulicVariant(
                        ElementalRaceIdentityCatalog.HydraulicBullRushAbility,
                        "Hydraulic Maneuver — Bull Rush", icon)));
            BlueprintAbility hydraulicDisarm = Add(registered,
                registry.Register<BlueprintAbility>(ElementalRaceIdentityCatalog
                    .HydraulicDisarmAbility, () => CreateHydraulicVariant(
                        ElementalRaceIdentityCatalog.HydraulicDisarmAbility,
                        "Hydraulic Maneuver — Disarm", icon)));
            BlueprintAbility hydraulicTrip = Add(registered,
                registry.Register<BlueprintAbility>(ElementalRaceIdentityCatalog
                    .HydraulicTripAbility, () => CreateHydraulicVariant(
                        ElementalRaceIdentityCatalog.HydraulicTripAbility,
                        "Hydraulic Maneuver — Trip", icon)));
            BlueprintAbility hydraulicDirtyTrick = Add(registered,
                registry.Register<BlueprintAbility>(ElementalRaceIdentityCatalog
                    .HydraulicDirtyTrickBlindAbility,
                    () => CreateHydraulicVariant(ElementalRaceIdentityCatalog
                        .HydraulicDirtyTrickBlindAbility,
                        "Hydraulic Maneuver — Dirty Trick (Blind)", icon)));
            BlueprintAbility[] variants = { hydraulicBullRush, hydraulicDisarm,
                hydraulicTrip, hydraulicDirtyTrick };
            BlueprintAbility hydraulicParent = Add(registered,
                registry.Register<BlueprintAbility>(ElementalRaceIdentityCatalog
                    .HydraulicManeuverAbility, () => CreateVariantParent(
                        ElementalRaceIdentityCatalog.HydraulicManeuverAbility,
                        "Hydraulic Maneuver", Description(
                            ElementalFeatId.HydraulicManeuver), variants, icon)));
            foreach (BlueprintAbility variant in variants)
                variant.Parent = hydraulicParent;

            BlueprintAbility tritonPortal = Add(registered,
                registry.Register<BlueprintAbility>(ElementalRaceIdentityCatalog
                    .TritonPortalAbility, () => CreatePointAbility(
                        ElementalRaceIdentityCatalog.TritonPortalAbility,
                        "Triton Portal", Description(
                            ElementalFeatId.TritonPortal),
                        UnitCommand.CommandType.Standard, icon)));
            tritonPortal.SetIsFullRoundAction(true);

            var facts = new Dictionary<ElementalFeatId, BlueprintUnitFact>
            {
                { ElementalFeatId.ElementalStrike, strikeAbility },
                { ElementalFeatId.ScorchingWeapons, scorchingAbility },
                { ElementalFeatId.BlazingAura, auraAbility },
                { ElementalFeatId.HydraulicManeuver, hydraulicParent },
                { ElementalFeatId.TritonPortal, tritonPortal }
            };
            var features = new Dictionary<ElementalFeatId, BlueprintFeature>();
            foreach (ElementalFeatDefinition definition in
                ElementalFeatPolicy.Ordered())
            {
                BlueprintUnitFact granted;
                facts.TryGetValue(definition.Id, out granted);
                BlueprintFeature feature = Add(registered, registry.Register<
                    BlueprintFeature>(FeatureSymbol(definition.Id), () =>
                        CreateFeature(definition, races, features, granted,
                            FeatureIcon(definition, races, icon))));
                features.Add(definition.Id, feature);
            }
            ConfigureWingsFeature(features[ElementalFeatId.WingsOfAir],
                wingsBuff);
            ConfigureScorchingWeapons(
                features[ElementalFeatId.ScorchingWeapons],
                features[ElementalFeatId.InnerFlame], scorchingAbility,
                scorchingBuff, scorchingEnchantment, races.Ifrit.Race);
            ConfigureBlazingAura(auraAbility, auraBuff, scorchingBuff,
                races.Ifrit.Race);
            ConfigureFiresight(features[ElementalFeatId.Firesight]);

            if (scorchingEnchantment.ComponentsArray == null ||
                registered.Count != ElementalRaceIdentityCatalog
                    .FeatIdentityCount)
                throw new InvalidOperationException(
                    "Elemental feat subsidiary registration drifted.");
            return new ElementalFeatBlueprintSet(features, registered);
        }

        private static T Add<T>(ICollection<BlueprintScriptableObject> all,
            T value) where T : BlueprintScriptableObject
        {
            all.Add(value);
            return value;
        }

        private static BlueprintFeature CreateFeature(
            ElementalFeatDefinition definition, ElementalRaceBlueprintSet races,
            IDictionary<ElementalFeatId, BlueprintFeature> prior,
            BlueprintUnitFact granted, Sprite icon)
        {
            var result = ScriptableObject.CreateInstance<BlueprintFeature>();
            result.name = InternalName(FeatureSymbol(definition.Id));
            result.Ranks = 1;
            result.IsClassFeature = false;
            result.HideInUI = false;
            result.Groups = definition.IsCombat
                ? new[] { FeatureGroup.Feat, FeatureGroup.CombatFeat }
                : new[] { FeatureGroup.Feat };
            var components = new List<BlueprintComponent>();
            BlueprintRace[] allowed = definition.AllowedRaces.Select(value =>
                Race(races, value)).ToArray();
            foreach (BlueprintRace race in allowed)
                components.Add(FeaturePrerequisite(race, allowed.Length == 1
                    ? Kingmaker.Blueprints.Classes.Prerequisites.Prerequisite
                        .GroupType.All
                    : Kingmaker.Blueprints.Classes.Prerequisites.Prerequisite
                        .GroupType.Any));
            foreach (ElementalFeatId prerequisite in definition.RequiredFeats)
                components.Add(FeaturePrerequisite(prior[prerequisite],
                    Kingmaker.Blueprints.Classes.Prerequisites.Prerequisite
                        .GroupType.All));
            if (definition.MinimumCharacterLevel > 0)
            {
                var level = ScriptableObject.CreateInstance<
                    PrerequisiteCharacterLevel>();
                level.Level = definition.MinimumCharacterLevel;
                level.Group = Kingmaker.Blueprints.Classes.Prerequisites
                    .Prerequisite.GroupType.All;
                components.Add(level);
            }
            if (definition.RequiresHydraulicPush)
                components.Add(FeaturePrerequisite(races.Undine.SlaFeature,
                    Kingmaker.Blueprints.Classes.Prerequisites.Prerequisite
                        .GroupType.All));
            if (granted != null)
            {
                var add = ScriptableObject.CreateInstance<AddFacts>();
                add.Facts = new[] { granted };
                add.DoNotRestoreMissingFacts = false;
                components.Add(add);
            }
            result.ComponentsArray = components.ToArray();
            BlueprintUnitFactAccess.Resolve().Configure(result,
                LocalizationService.Create("KMG.ElementalRaces.Feats." +
                    definition.Id + ".Name", definition.Name),
                LocalizationService.Create("KMG.ElementalRaces.Feats." +
                    definition.Id + ".Description", Description(definition.Id)),
                icon);
            return result;
        }

        private static PrerequisiteFeature FeaturePrerequisite(
            BlueprintFeature feature,
            Kingmaker.Blueprints.Classes.Prerequisites.Prerequisite.GroupType
                group)
        {
            var result = ScriptableObject.CreateInstance<PrerequisiteFeature>();
            result.Feature = feature;
            result.Group = group;
            return result;
        }

        private static BlueprintAbility CreateBuffAbility(string symbol,
            string name, string description, UnitCommand.CommandType action,
            BlueprintBuff buff, Sprite icon)
        {
            BlueprintAbility result = CreateAbility(symbol, name, description,
                action, AbilityType.Extraordinary, AbilityRange.Personal, icon);
            var apply = ScriptableObject.CreateInstance<ContextActionApplyBuff>();
            apply.Buff = buff;
            apply.DurationValue = new ContextDurationValue
            {
                Rate = DurationRate.Rounds,
                DiceType = Kingmaker.RuleSystem.DiceType.Zero,
                DiceCountValue = 0,
                BonusValue = 1
            };
            apply.IsFromSpell = false;
            apply.IsNotDispelable = true;
            apply.ToCaster = true;
            var effect = ScriptableObject.CreateInstance<AbilityEffectRunAction>();
            effect.Actions = new ActionList { Actions = new GameAction[] { apply } };
            result.ComponentsArray = new BlueprintComponent[] { effect };
            return result;
        }

        private static BlueprintAbility CreateHydraulicVariant(string symbol,
            string name, Sprite icon)
        {
            BlueprintAbility result = CreateAbility(symbol, name,
                "Spend the active racial Hydraulic Push use to attempt this exact native combat maneuver using total character level plus the current best mental ability modifier.",
                UnitCommand.CommandType.Standard, AbilityType.SpellLike,
                AbilityRange.Close, icon);
            result.CanTargetSelf = false;
            result.CanTargetEnemies = true;
            result.EffectOnAlly = AbilityEffectOnUnit.None;
            result.EffectOnEnemy = AbilityEffectOnUnit.Harmful;
            result.SpellResistance = true;
            return result;
        }

        private static BlueprintAbility CreateVariantParent(string symbol,
            string name, string description, BlueprintAbility[] variants,
            Sprite icon)
        {
            BlueprintAbility result = CreateAbility(symbol, name, description,
                UnitCommand.CommandType.Standard, AbilityType.SpellLike,
                AbilityRange.Close, icon);
            var component = ScriptableObject.CreateInstance<AbilityVariants>();
            component.Variants = (BlueprintAbility[])variants.Clone();
            result.ComponentsArray = new BlueprintComponent[] { component };
            return result;
        }

        private static BlueprintAbility CreatePointAbility(string symbol,
            string name, string description, UnitCommand.CommandType action,
            Sprite icon)
        {
            BlueprintAbility result = CreateAbility(symbol, name, description,
                action, AbilityType.SpellLike, AbilityRange.Close, icon);
            result.CanTargetSelf = false;
            result.CanTargetPoint = true;
            result.EffectOnAlly = AbilityEffectOnUnit.None;
            return result;
        }

        private static BlueprintAbility CreateAbility(string symbol,
            string name, string description, UnitCommand.CommandType action,
            AbilityType type, AbilityRange range, Sprite icon)
        {
            var result = ScriptableObject.CreateInstance<BlueprintAbility>();
            result.name = InternalName(symbol);
            result.Type = type;
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
            result.ActionType = action;
            result.Animation = UnitAnimationActionCastSpell.CastAnimationStyle
                .Immediate;
            result.MaterialComponent = new BlueprintAbility
                .MaterialComponentData();
            result.ResourceAssetIds = Array.Empty<string>();
            result.ComponentsArray = Array.Empty<BlueprintComponent>();
            result.LocalizedDuration = LocalizationService.Create(
                "KMG.ElementalRaces.Feats.Duration", "See description");
            result.LocalizedSavingThrow = LocalizationService.Create(
                "KMG.ElementalRaces.Feats.SavingThrow", "None");
            BlueprintUnitFactAccess.Resolve().Configure(result,
                LocalizationService.Create(symbol + ".Name", name),
                LocalizationService.Create(symbol + ".Description",
                    description), icon);
            return result;
        }

        private static BlueprintBuff CreateBuff(string symbol, string name,
            string description, Sprite icon)
        {
            var result = ScriptableObject.CreateInstance<BlueprintBuff>();
            result.name = InternalName(symbol);
            result.IsClassFeature = false;
            result.Stacking = StackingType.Replace;
            result.ComponentsArray = Array.Empty<BlueprintComponent>();
            result.FxOnStart = new PrefabLink();
            result.FxOnRemove = new PrefabLink();
            result.ResourceAssetIds = Array.Empty<string>();
            BlueprintUnitFactAccess.Resolve().Configure(result,
                LocalizationService.Create(symbol + ".Name", name),
                LocalizationService.Create(symbol + ".Description",
                    description), icon);
            return result;
        }

        private static void ConfigureElementalStrike(BlueprintBuff buff,
            ElementalRaceBlueprintSet races)
        {
            if (buff == null || races == null)
                throw new ArgumentNullException();
            var damage = ScriptableObject.CreateInstance<
                ElementalStrikeDamage>();
            damage.Ifrit = races.Ifrit.Race;
            damage.Oread = races.Oread.Race;
            damage.Sylph = races.Sylph.Race;
            damage.Undine = races.Undine.Race;
            buff.ComponentsArray = new BlueprintComponent[] { damage };
        }

        private static BlueprintBuff CreateWingsBuff(Sprite icon)
        {
            BlueprintBuff result = CreateBuff(ElementalRaceIdentityCatalog
                .WingsOfAirBuff, "Wings of Air",
                "Native Kingmaker flight benefits are active while you wear no armor or light armor.", icon);
            var armorClass = ScriptableObject.CreateInstance<
                ACBonusAgainstAttacks>();
            armorClass.AgainstMeleeOnly = true;
            armorClass.AgainstRangedOnly = false;
            armorClass.ArmorClassBonus = 3;
            armorClass.CheckArmorCategory = false;
            armorClass.Descriptor = ModifierDescriptor.Dodge;
            armorClass.NoShield = false;
            armorClass.NotArmorCategory = Array.Empty<
                ArmorProficiencyGroup>();
            armorClass.NotTouch = false;
            armorClass.OnlyAttacksOfOpportunity = false;

            var terrain = ScriptableObject.CreateInstance<
                AddConditionImmunity>();
            terrain.Condition = UnitCondition.DifficultTerrain;
            var ground = ScriptableObject.CreateInstance<
                BuffDescriptorImmunity>();
            ground.CheckFact = false;
            ground.Descriptor = SpellDescriptor.Ground;
            ground.FactToCheck = null;
            ground.IgnoreFeature = null;
            result.ComponentsArray = new BlueprintComponent[]
            {
                armorClass,
                terrain,
                ground
            };
            return result;
        }

        private static void ConfigureWingsFeature(BlueprintFeature feature,
            BlueprintBuff buff)
        {
            if (feature == null || buff == null)
                throw new ArgumentNullException();
            var controller = ScriptableObject.CreateInstance<
                ElementalWingsOfAirController>();
            controller.FlightBuff = buff;
            feature.ComponentsArray = (feature.ComponentsArray ??
                Array.Empty<BlueprintComponent>()).Where(value =>
                    !(value is ElementalWingsOfAirController)).Concat(
                        new BlueprintComponent[] { controller }).ToArray();
        }

        private static void ConfigureScorchingWeapons(
            BlueprintFeature scorchingFeature, BlueprintFeature innerFlame,
            BlueprintAbility ability, BlueprintBuff marker,
            BlueprintWeaponEnchantment enchantment, BlueprintRace ifrit)
        {
            if (scorchingFeature == null || innerFlame == null ||
                ability == null || marker == null || enchantment == null ||
                ifrit == null) throw new ArgumentNullException();

            var saveBonus = ScriptableObject.CreateInstance<
                ElementalScorchingWeaponsSaveBonus>();
            saveBonus.InnerFlame = innerFlame;
            scorchingFeature.ComponentsArray = (scorchingFeature
                .ComponentsArray ?? Array.Empty<BlueprintComponent>())
                .Where(value => !(value is
                    ElementalScorchingWeaponsSaveBonus))
                .Concat(new BlueprintComponent[] { saveBonus }).ToArray();

            var delivery = ScriptableObject.CreateInstance<
                ElementalScorchingWeaponsAbilityLogic>();
            delivery.Ifrit = ifrit;
            delivery.Marker = marker;
            delivery.WeaponEnchantment = enchantment;
            ability.ComponentsArray = new BlueprintComponent[] { delivery };

            var damage = ScriptableObject.CreateInstance<
                ElementalScorchingWeaponsDamage>();
            damage.InnerFlame = innerFlame;
            enchantment.ComponentsArray = new BlueprintComponent[] { damage };
        }

        private static void ConfigureBlazingAura(BlueprintAbility ability,
            BlueprintBuff aura, BlueprintBuff scorchingMarker,
            BlueprintRace ifrit)
        {
            if (ability == null || aura == null || scorchingMarker == null ||
                ifrit == null) throw new ArgumentNullException();
            var delivery = ScriptableObject.CreateInstance<
                ElementalBlazingAuraAbilityLogic>();
            delivery.Ifrit = ifrit;
            delivery.ScorchingWeaponsMarker = scorchingMarker;
            delivery.Aura = aura;
            ability.ComponentsArray = new BlueprintComponent[] { delivery };
        }

        private static void ConfigureFiresight(BlueprintFeature feature)
        {
            if (feature == null) throw new ArgumentNullException("feature");
            var dazzled = ScriptableObject.CreateInstance<
                AddConditionImmunity>();
            dazzled.Condition = UnitCondition.Dazzled;
            feature.ComponentsArray = (feature.ComponentsArray ??
                Array.Empty<BlueprintComponent>()).Where(value =>
                    !(value is AddConditionImmunity &&
                      ((AddConditionImmunity)value).Condition ==
                        UnitCondition.Dazzled)).Concat(
                            new BlueprintComponent[] { dazzled }).ToArray();
        }

        private static BlueprintWeaponEnchantment CreateEnchantment()
        {
            var result = ScriptableObject.CreateInstance<
                BlueprintWeaponEnchantment>();
            result.name = InternalName(ElementalRaceIdentityCatalog
                .ScorchingWeaponsEnchantment);
            result.ComponentsArray = Array.Empty<BlueprintComponent>();
            SetEnchantment(result, "m_EnchantName", LocalizationService.Create(
                "KMG.ElementalRaces.Feats.ScorchingWeapons.Enchantment.Name",
                "Scorching Weapons"));
            SetEnchantment(result, "m_Description", LocalizationService.Create(
                "KMG.ElementalRaces.Feats.ScorchingWeapons.Enchantment.Description",
                "This exact weapon was selected when Scorching Weapons was activated."));
            SetEnchantment(result, "m_EnchantmentCost", 0);
            return result;
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

        private static BlueprintRace Race(ElementalRaceBlueprintSet set,
            ElementalHeritageRace race)
        {
            switch (race)
            {
                case ElementalHeritageRace.Ifrit: return set.Ifrit.Race;
                case ElementalHeritageRace.Oread: return set.Oread.Race;
                case ElementalHeritageRace.Sylph: return set.Sylph.Race;
                case ElementalHeritageRace.Undine: return set.Undine.Race;
                default: throw new ArgumentOutOfRangeException("race");
            }
        }

        private static Sprite FeatureIcon(ElementalFeatDefinition definition,
            ElementalRaceBlueprintSet races, Sprite fallback)
        {
            ElementalHeritageRace race = definition.AllowedRaces[0];
            Sprite result = Race(races, race) == null ? null :
                (race == ElementalHeritageRace.Ifrit ? races.Ifrit.SlaAbility.Icon :
                race == ElementalHeritageRace.Oread ? races.Oread.SlaAbility.Icon :
                race == ElementalHeritageRace.Sylph ? races.Sylph.SlaAbility.Icon :
                races.Undine.SlaAbility.Icon);
            return result ?? fallback;
        }

        private static string FeatureSymbol(ElementalFeatId id)
        {
            switch (id)
            {
                case ElementalFeatId.ElementalStrike: return ElementalRaceIdentityCatalog.ElementalStrikeFeat;
                case ElementalFeatId.ScorchingWeapons: return ElementalRaceIdentityCatalog.ScorchingWeaponsFeat;
                case ElementalFeatId.InnerFlame: return ElementalRaceIdentityCatalog.InnerFlameFeat;
                case ElementalFeatId.BlazingAura: return ElementalRaceIdentityCatalog.BlazingAuraFeat;
                case ElementalFeatId.Firesight: return ElementalRaceIdentityCatalog.FiresightFeat;
                case ElementalFeatId.AiryStep: return ElementalRaceIdentityCatalog.AiryStepFeat;
                case ElementalFeatId.WingsOfAir: return ElementalRaceIdentityCatalog.WingsOfAirFeat;
                case ElementalFeatId.CloudGazer: return ElementalRaceIdentityCatalog.CloudGazerFeat;
                case ElementalFeatId.InnerBreath: return ElementalRaceIdentityCatalog.InnerBreathFeat;
                case ElementalFeatId.HydraulicManeuver: return ElementalRaceIdentityCatalog.HydraulicManeuverFeat;
                case ElementalFeatId.TritonPortal: return ElementalRaceIdentityCatalog.TritonPortalFeat;
                default: throw new ArgumentOutOfRangeException("id");
            }
        }

        private static string Description(ElementalFeatId id)
        {
            switch (id)
            {
                case ElementalFeatId.ElementalStrike:
                    return "As a swift action, empower qualifying weapon attacks for 1 round. They deal +1 race-linked energy damage, increasing by +1 at character levels 5, 10, 15, and 20 (maximum +5).";
                case ElementalFeatId.ScorchingWeapons:
                    return "You gain a +2 racial bonus on saves against fire attacks and spells with the fire or light descriptor. As a swift action, select up to two currently held manufactured metallic weapons for 1 round; each deals 1 additional fire damage, which does not stack with another fire-damage weapon effect.";
                case ElementalFeatId.InnerFlame:
                    return "Your Scorching Weapons save bonus becomes +4 and its weapon damage becomes 1d6 fire instead of +1. Kingmaker has no ordinary player-facing grapple state, so the printed grapple clause has no mechanical effect.";
                case ElementalFeatId.BlazingAura:
                    return "As a free action on your turn while Scorching Weapons is active, create a 1-round aura. Any creature beginning its turn adjacent to you takes 1d6 fire damage.";
                case ElementalFeatId.Firesight:
                    return "You are immune to dazzled and can see through an exact catalog of fire and smoke effects. This does not negate unrelated concealment, fog, invisibility, displacement, blindness, darkness, Blur, or Mirror Image.";
                case ElementalFeatId.AiryStep:
                    return "You gain a +2 racial bonus on saves against air- or electricity-descriptor effects and effects that deal electricity damage, applied once per effect. Kingmaker has no ordinary falling-damage system, so the printed falling clause has no mechanical effect.";
                case ElementalFeatId.WingsOfAir:
                    return "Your Airy Step save bonus becomes +4. While wearing no armor or light armor, you gain Kingmaker's native flight abstraction: +3 dodge AC against melee attacks and immunity to difficult terrain and ground-descriptor effects. This does not grant free three-dimensional navigation or blanket trip, prone, or melee immunity.";
                case ElementalFeatId.CloudGazer:
                    return "You ignore concealment from an exact catalog of fog, mist, and cloud effects within ordinary game sight range, but not smoke-only effects, darkness, blindness, invisibility, Blur, displacement, or Mirror Image.";
                case ElementalFeatId.InnerBreath:
                    return "You do not need to breathe and are immune only to inhaled poisons and effects that explicitly require respiration. This is not blanket poison, gas, cloud, disease, Fortitude, or hazard immunity.";
                case ElementalFeatId.HydraulicManeuver:
                    return "When using your active racial Hydraulic Push, choose Bull Rush, Disarm, Trip, or native Dirty Trick (blind). Each option shares the same racial use, range, action, spell resistance, total-level scaling, and current best mental ability modifier. Kingmaker exposes no native Dirty Trick (dazzle) maneuver.";
                case ElementalFeatId.TritonPortal:
                    return "As a full-round action, expend your active racial Hydraulic Push use to summon 1d3 Small Water Elementals for 1 round per total character level. The Kingmaker adaptation omits unavailable aquatic creature choices.";
                default: throw new ArgumentOutOfRangeException("id");
            }
        }

        private static string InternalName(string symbol)
        {
            return symbol.Replace('.', '_');
        }
    }
}
