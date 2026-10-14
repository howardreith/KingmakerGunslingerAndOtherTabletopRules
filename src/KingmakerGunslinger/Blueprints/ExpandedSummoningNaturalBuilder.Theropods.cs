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
            BlueprintUnit expiryEffectDonor = BlueprintLibraryLookup
                .RequireExact<BlueprintUnit>(library,
                    TheropodVisualPolicy.ExpiryEffectDonorGuid,
                    "qualified native theropod expiry dissolve donor");
            ConfigureTheropodExpiryVisual(Require<BlueprintUnit>(bySymbol,
                    "KMG.Summoning.Unit.Deinonychus"), expiryEffectDonor);
            ConfigureTheropodExpiryVisual(Require<BlueprintUnit>(bySymbol,
                    "KMG.Summoning.Unit.Tyrannosaurus"), expiryEffectDonor);

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
            ConfigureDeinonychusChassis(Require<BlueprintUnit>(bySymbol,
                    "KMG.Summoning.Unit.Deinonychus"), talon,
                deinonychusChassis);
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

        private static void ConfigureTheropodExpiryVisual(BlueprintUnit unit,
            BlueprintUnit donor)
        {
            if (unit == null || unit.Visual == null || donor == null ||
                donor.Visual == null || donor.Visual.BloodPuddleFx == null ||
                string.IsNullOrEmpty(donor.Visual.BloodPuddleFx.AssetId))
                throw new InvalidOperationException(
                    "The exact native theropod expiry carrier is unavailable.");
            // Clone the nested visual record before replacing one link. Unit
            // shells shallow-copy their donor's visual record, so assigning on
            // that shared object would mutate the native Lizardfolk blueprint.
            var visual = new UnitVisualParams();
            CopyFields(unit.Visual, visual);
            visual.BloodPuddleFx = donor.Visual.BloodPuddleFx;
            unit.Visual = visual;
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
            BlueprintUnit owner, BlueprintItemWeapon talon,
            BlueprintFeature feature)
        {
            var strength = ScriptableObject.CreateInstance<
                DeinonychusPrimaryTalonStrength>();
            strength.OwningBlueprint = owner;
            strength.Talon = talon;
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
                    TheropodRulesPolicy.DeinonychusRacialSkillBonus),
                strength
            };
            BlueprintUnitFactAccess.Resolve().Configure(feature,
                LocalizationService.Create(
                    "KMG.ExpandedSummoning.Deinonychus.Chassis.Name",
                    "Deinonychus Chassis"),
                LocalizationService.Create(
                    "KMG.ExpandedSummoning.Deinonychus.Chassis.Description",
                    "Carries the adult deinonychus's exact racial bonuses to Acrobatics, Perception and Stealth plus the first talon's ordinary full-Strength term, without changing its mixed primary/secondary attack roles or adding quadruped trip defense."),
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
