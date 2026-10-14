using System;
using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.FactLogic;
using KingmakerGunslinger.Summoning;
using UnityEngine;

namespace KingmakerGunslinger.Blueprints
{
    internal static partial class ExpandedSummoningNaturalBuilder
    {
        private static void ConfigureTheropods(
            LibraryScriptableObject library,
            IDictionary<string, BlueprintScriptableObject> bySymbol)
        {
            BlueprintItemWeapon talon = Require<BlueprintItemWeapon>(bySymbol,
                TheropodRulesPolicy.DeinonychusTalonSymbol);
            BlueprintItemWeapon bite = Require<BlueprintItemWeapon>(bySymbol,
                TheropodRulesPolicy.TyrannosaurusBiteSymbol);
            ConfigureWeapon(BlueprintLibraryLookup.RequireExact<
                    BlueprintItemWeapon>(library, NativeClaw1d4Guid,
                        "native claw animation weapon"), talon,
                TheropodRulesPolicy.DeinonychusTalonSymbol, 1,
                Kingmaker.RuleSystem.DiceType.D8);
            ConfigureWeapon(BlueprintLibraryLookup.RequireExact<
                    BlueprintItemWeapon>(library, NativeBiteLarge2d6Guid,
                        "native large bite animation weapon"), bite,
                TheropodRulesPolicy.TyrannosaurusBiteSymbol, 4,
                Kingmaker.RuleSystem.DiceType.D6);

            BlueprintFeature deinonychusChassis = Require<BlueprintFeature>(
                bySymbol, TheropodRulesPolicy.DeinonychusChassisSymbol);
            ConfigureDeinonychusChassis(deinonychusChassis);
            BlueprintFeature tyrannosaurusChassis = Require<BlueprintFeature>(
                bySymbol, TheropodRulesPolicy.TyrannosaurusChassisSymbol);
            ConfigureTyrannosaurusChassis(tyrannosaurusChassis);

            ConfigureTheropodUnitType(Require<BlueprintUnitType>(bySymbol,
                    TheropodRulesPolicy.DeinonychusUnitTypeSymbol),
                TheropodRulesPolicy.DeinonychusUnitTypeSymbol,
                "Deinonychus",
                "A light, agile bipedal dinosaur with sickle talons, a balancing tail, jaws and grasping foreclaws.",
                Array.Empty<BlueprintUnitFact>());
            ConfigureTheropodUnitType(Require<BlueprintUnitType>(bySymbol,
                    TheropodRulesPolicy.TyrannosaurusUnitTypeSymbol),
                TheropodRulesPolicy.TyrannosaurusUnitTypeSymbol,
                "Tyrannosaurus",
                "A gargantuan bipedal predator with massive jaws, powerful hind limbs, small forearms and a counterbalancing tail.",
                new BlueprintUnitFact[] {
                    Require<BlueprintFeature>(bySymbol,
                        TheropodRulesPolicy.TyrannosaurusPowerfulBiteSymbol),
                    Require<BlueprintBuff>(bySymbol,
                        TheropodRulesPolicy.TyrannosaurusCombatTraitsSymbol),
                    Require<BlueprintFeature>(bySymbol,
                        TheropodRulesPolicy.TyrannosaurusBleedingCriticalSymbol)
                });
        }

        private static void ConfigureTheropodUnitType(BlueprintUnitType type,
            string symbol, string displayName, string description,
            BlueprintUnitFact[] signatures)
        {
            type.name = InternalName(symbol);
            type.KnowledgeStat = StatType.SkillLoreNature;
            type.Name = LocalizationService.Create(
                "KMG.ExpandedSummoning." + displayName + ".Type.Name",
                displayName);
            type.Description = LocalizationService.Create(
                "KMG.ExpandedSummoning." + displayName + ".Type.Description",
                description);
            type.Image = null;
            type.SignatureAbilities = signatures ?? Array.Empty<BlueprintUnitFact>();
        }

        private static void ConfigureDeinonychusChassis(
            BlueprintFeature feature)
        {
            feature.name = InternalName(
                TheropodRulesPolicy.DeinonychusChassisSymbol);
            feature.IsClassFeature = false;
            feature.HideInUI = true;
            feature.ComponentsArray = new BlueprintComponent[] {
                RacialSkill(StatType.SkillMobility,
                    TheropodRulesPolicy.DeinonychusRacialSkillBonus),
                RacialSkill(StatType.SkillPerception,
                    TheropodRulesPolicy.DeinonychusRacialSkillBonus),
                RacialSkill(StatType.SkillStealth,
                    TheropodRulesPolicy.DeinonychusRacialSkillBonus)
            };
            BlueprintUnitFactAccess.Resolve().Configure(feature,
                LocalizationService.Create(
                    "KMG.ExpandedSummoning.Deinonychus.Chassis.Name",
                    "Deinonychus Chassis"),
                LocalizationService.Create(
                    "KMG.ExpandedSummoning.Deinonychus.Chassis.Description",
                    "Carries the adult deinonychus's exact racial bonuses to Acrobatics, Perception and Stealth without a quadruped trip-defense fact."),
                null);
        }

        private static void ConfigureTyrannosaurusChassis(
            BlueprintFeature feature)
        {
            var reach = ScriptableObject.CreateInstance<AddStatBonus>();
            reach.Stat = StatType.Reach;
            reach.Value = TheropodRulesPolicy.TyrannosaurusReachBonusFeet;
            reach.Descriptor = ModifierDescriptor.UntypedStackable;
            feature.name = InternalName(
                TheropodRulesPolicy.TyrannosaurusChassisSymbol);
            feature.IsClassFeature = false;
            feature.HideInUI = true;
            feature.ComponentsArray = new BlueprintComponent[] {
                reach,
                RacialSkill(StatType.SkillPerception,
                    TheropodRulesPolicy.TyrannosaurusRacialPerceptionBonus),
                UntypedSkill(StatType.SkillPerception,
                    TheropodRulesPolicy.TyrannosaurusSkillFocusCompletionBonus)
            };
            BlueprintUnitFactAccess.Resolve().Configure(feature,
                LocalizationService.Create(
                    "KMG.ExpandedSummoning.Tyrannosaurus.Chassis.Name",
                    "Tyrannosaurus Chassis"),
                LocalizationService.Create(
                    "KMG.ExpandedSummoning.Tyrannosaurus.Chassis.Description",
                    "Carries the adult tyrannosaurus's twenty-foot reach, racial Perception bonus and the high-rank half of Skill Focus (Perception)."),
                null);
        }

        private static AddStatBonus RacialSkill(StatType stat, int value)
        {
            var result = ScriptableObject.CreateInstance<AddStatBonus>();
            result.Stat = stat;
            result.Value = value;
            result.Descriptor = ModifierDescriptor.Racial;
            return result;
        }

        private static AddStatBonus UntypedSkill(StatType stat, int value)
        {
            var result = ScriptableObject.CreateInstance<AddStatBonus>();
            result.Stat = stat;
            result.Value = value;
            result.Descriptor = ModifierDescriptor.UntypedStackable;
            return result;
        }
    }
}
