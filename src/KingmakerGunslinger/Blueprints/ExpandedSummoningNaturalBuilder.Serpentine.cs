using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Designers.Mechanics.Buffs;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.RuleSystem;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.Components;
using KingmakerGunslinger.Summoning;
using UnityEngine;

namespace KingmakerGunslinger.Blueprints
{
    internal static partial class ExpandedSummoningNaturalBuilder
    {
        private const string ViperPoisonSymbol = "KMG.Summoning.Natural.Viper.Poison";
        private const string ViperVenomSymbol = "KMG.Summoning.Natural.Viper.Venom";

        private static void ConfigureSerpentines(LibraryScriptableObject library,
            IDictionary<string, BlueprintScriptableObject> bySymbol)
        {
            BlueprintFeature poison = Require<BlueprintFeature>(bySymbol, ViperPoisonSymbol);
            ConfigureViperPoison(library, poison,
                Require<BlueprintBuff>(bySymbol, ViperVenomSymbol),
                Require<BlueprintItemWeapon>(bySymbol, Bite1d4Symbol));
            foreach (string key in new[] { "viper", "constrictor-snake" })
            {
                string token = Token(key);
                NaturalSummonProfile profile = ExpandedSummoningNaturalProfiles.For(key);
                BlueprintUnit unit = Require<BlueprintUnit>(bySymbol,
                    "KMG.Summoning.Unit." + token);
                BlueprintUnitType type = Require<BlueprintUnitType>(bySymbol,
                    "KMG.Summoning.Natural." + token + ".UnitType");
                type.name = InternalName("KMG.Summoning.Natural." + token + ".UnitType");
                type.KnowledgeStat = StatType.SkillLoreNature;
                type.Name = LocalizationService.Create(
                    "KMG.ExpandedSummoning." + token + ".UnitType.Name", profile.DisplayName);
                type.Description = LocalizationService.Create(
                    "KMG.ExpandedSummoning." + token + ".UnitType.Description",
                    key == "viper" ? "A venomous Medium snake." :
                        "A muscular Medium snake that grabs with its bite and constricts.");
                type.Image = null;
                type.SignatureAbilities = Array.Empty<BlueprintUnitFact>();
                unit.Type = type;

                var ranks = ScriptableObject.CreateInstance<SummonSerpentineSkillRanks>();
                ranks.CreatureKey = key; ranks.OwningBlueprint = unit;
                unit.ComponentsArray = unit.ComponentsArray.Concat(
                    new BlueprintComponent[] { ranks }).ToArray();
                var weaponStats = ScriptableObject.CreateInstance<SummonSerpentineWeaponStats>();
                weaponStats.OwningBlueprint = unit;
                weaponStats.Bite = (BlueprintItemWeapon)unit.Body.PrimaryHand;
                string symbol = "KMG.Summoning.Natural." + token + ".CombatProfile";
                BlueprintFeature combat = Require<BlueprintFeature>(bySymbol, symbol);
                combat.name = InternalName(symbol);
                combat.HideInUI = true; combat.IsClassFeature = false;
                combat.ComponentsArray = new BlueprintComponent[] {
                    SnakeRacialBonus(StatType.SkillMobility, SerpentineRulesPolicy.MobilityRacialBonus),
                    SnakeRacialBonus(StatType.SkillPerception, SerpentineRulesPolicy.PerceptionRacialBonus),
                    SnakeRacialBonus(StatType.SkillStealth, SerpentineRulesPolicy.StealthRacialBonus),
                    weaponStats };
                BlueprintUnitFactAccess.Resolve().Configure(combat,
                    LocalizationService.Create("KMG.ExpandedSummoning." + token + ".CombatProfile.Name",
                        profile.DisplayName + " Land Profile"),
                    LocalizationService.Create("KMG.ExpandedSummoning." + token + ".CombatProfile.Description",
                        "+8 racial Mobility, +4 racial Perception and +4 racial Stealth. " +
                        "Aquatic skills and movement are omitted under land-use scope; " +
                        "scent and low-light vision are unmodeled engine limitations."), null);
                unit.AddFacts = unit.AddFacts.Concat(key == "viper"
                    ? new BlueprintUnitFact[] { combat, poison }
                    : new BlueprintUnitFact[] { combat }).ToArray();
            }
        }

        private static AddStatBonus SnakeRacialBonus(StatType stat, int amount)
        {
            var bonus = ScriptableObject.CreateInstance<AddStatBonus>();
            bonus.Stat = stat; bonus.Value = amount; bonus.Descriptor = ModifierDescriptor.Racial;
            return bonus;
        }

        private static void ConfigureViperPoison(LibraryScriptableObject library,
            BlueprintFeature feature, BlueprintBuff venom, BlueprintItemWeapon bite)
        {
            BlueprintBuff native = BlueprintLibraryLookup.RequireExact<BlueprintBuff>(
                library, NativeSpiderPoisonBuffGuid, "native saved poison lifecycle");
            CopyFields(native, venom);
            venom.name = InternalName(ViperVenomSymbol);
            venom.ComponentsArray = native.ComponentsArray.Select(
                ExpandedSummoningAbilityBuilder.DeepCloneComponent).ToArray();
            venom.Stacking = StackingType.Poison;
            BuffPoisonStatDamage damage = venom.ComponentsArray.OfType<BuffPoisonStatDamage>().Single();
            damage.Stat = StatType.Constitution;
            damage.Value = new DiceFormula(1, DiceType.D2);
            damage.Ticks = SerpentineRulesPolicy.ViperPoisonExposures;
            damage.SuccesfullSaves = SerpentineRulesPolicy.ViperPoisonSavesToCure;
            damage.SaveType = SavingThrowType.Fortitude;
            BlueprintUnitFactAccess.Resolve().Configure(venom,
                LocalizationService.Create("KMG.ExpandedSummoning.Viper.Venom.Name", "Viper Venom"),
                LocalizationService.Create("KMG.ExpandedSummoning.Viper.Venom.Description",
                    "Injury poison: Fortitude DC 13 at baseline (10 + half its racial Hit Dice + live Constitution modifier); " +
                    "1d2 Constitution damage each round for six total exposures; one successful save cures it."), native.Icon);
            BlueprintFeature nativeFeature = BlueprintLibraryLookup.RequireExact<BlueprintFeature>(
                library, NativeSpiderPoisonFeatureGuid, "native poison-on-hit feature");
            CopyFields(nativeFeature, feature);
            feature.name = InternalName(ViperPoisonSymbol);
            feature.HideInUI = true; feature.IsClassFeature = false;
            feature.ComponentsArray = nativeFeature.ComponentsArray.Select(
                ExpandedSummoningAbilityBuilder.DeepCloneComponent).ToArray();
            AddInitiatorAttackWithWeaponTrigger trigger = feature.ComponentsArray
                .OfType<AddInitiatorAttackWithWeaponTrigger>().Single();
            trigger.WeaponType = bite.Type; trigger.OnlyHit = true;
            ContextActionSavingThrow save = trigger.Action.Actions.OfType<ContextActionSavingThrow>().Single();
            ContextActionConditionalSaved outcome = save.Actions.Actions.OfType<ContextActionConditionalSaved>().Single();
            outcome.Failed.Actions.OfType<ContextActionApplyBuff>().Single().Buff = venom;
            trigger.Action.Actions = new GameAction[] { new ContextActionOnlyIfWeaponWounded {
                Actions = new ActionList { Actions = new GameAction[] {
                    new ContextActionSetViperPoisonDc() }.Concat(trigger.Action.Actions).ToArray() } } };
            BlueprintUnitFactAccess.Resolve().Configure(feature,
                LocalizationService.Create("KMG.ExpandedSummoning.Viper.Poison.Name", "Viper Poison"),
                LocalizationService.Create("KMG.ExpandedSummoning.Viper.Poison.Description",
                    "A bite that hits and deals damage delivers the viper's own Constitution poison."), null);
        }
    }
}
