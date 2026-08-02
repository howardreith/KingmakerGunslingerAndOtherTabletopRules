using System;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.Visual.Animation.Kingmaker.Actions;
using KingmakerGunslinger.Deeds;
using UnityEngine;

namespace KingmakerGunslinger.Blueprints
{
    internal sealed class SlingersLuckBlueprintSet
    {
        internal SlingersLuckBlueprintSet(BlueprintFeature feature,
            BlueprintAbility savingAbility, BlueprintAbility skillAbility,
            BlueprintBuff savingMarker, BlueprintBuff skillMarker)
        {
            Feature = feature; SavingAbility = savingAbility;
            SkillAbility = skillAbility; SavingMarker = savingMarker;
            SkillMarker = skillMarker;
        }
        internal BlueprintFeature Feature { get; private set; }
        internal BlueprintAbility SavingAbility { get; private set; }
        internal BlueprintAbility SkillAbility { get; private set; }
        internal BlueprintBuff SavingMarker { get; private set; }
        internal BlueprintBuff SkillMarker { get; private set; }
        internal int Count { get { return 5; } }
    }

    internal static class SlingersLuckBlueprints
    {
        internal const string FeatureSymbol = "KMG.Deeds.SlingersLuckFeature";
        internal const string SavingAbilitySymbol =
            "KMG.Deeds.SlingersLuckSavingThrowAbility";
        internal const string SkillAbilitySymbol =
            "KMG.Deeds.SlingersLuckSkillCheckAbility";
        internal const string SavingMarkerSymbol =
            "KMG.Deeds.SlingersLuckSavingThrowArmed";
        internal const string SkillMarkerSymbol =
            "KMG.Deeds.SlingersLuckSkillCheckArmed";

        internal static SlingersLuckBlueprintSet Register(
            BlueprintRegistry registry, BlueprintAbilityResource grit,
            BlueprintCharacterClass gunslingerClass)
        {
            BlueprintBuff savingMarker = registry.Register<BlueprintBuff>(
                SavingMarkerSymbol, () => CreateSavingMarker(grit, gunslingerClass));
            BlueprintBuff skillMarker = registry.Register<BlueprintBuff>(
                SkillMarkerSymbol, () => CreateSkillMarker(grit, gunslingerClass));
            BlueprintAbility savingAbility = registry.Register<BlueprintAbility>(
                SavingAbilitySymbol, () => CreateAbility("Saving Throw", 2,
                    savingMarker, grit));
            BlueprintAbility skillAbility = registry.Register<BlueprintAbility>(
                SkillAbilitySymbol, () => CreateAbility("Skill Check", 1,
                    skillMarker, grit));
            BlueprintFeature feature = registry.Register<BlueprintFeature>(
                FeatureSymbol, () => CreateFeature(savingAbility, skillAbility));
            Validate(feature, savingAbility, skillAbility, savingMarker,
                skillMarker, grit, gunslingerClass);
            return new SlingersLuckBlueprintSet(feature, savingAbility,
                skillAbility, savingMarker, skillMarker);
        }

        private static BlueprintBuff CreateSavingMarker(
            BlueprintAbilityResource grit, BlueprintCharacterClass gunslingerClass)
        {
            var result = CreateMarker("SavingThrow");
            var logic = ScriptableObject.CreateInstance<
                SlingersLuckSavingThrowReroll>();
            logic.name = "$KMG_SlingersLuck_SavingReroll";
            logic.Grit = grit; logic.GunslingerClass = gunslingerClass;
            result.ComponentsArray = new BlueprintComponent[] { logic };
            return result;
        }

        private static BlueprintBuff CreateSkillMarker(BlueprintAbilityResource grit,
            BlueprintCharacterClass gunslingerClass)
        {
            var result = CreateMarker("SkillCheck");
            var logic = ScriptableObject.CreateInstance<SlingersLuckSkillCheckReroll>();
            logic.name = "$KMG_SlingersLuck_SkillReroll";
            logic.Grit = grit; logic.GunslingerClass = gunslingerClass;
            result.ComponentsArray = new BlueprintComponent[] { logic };
            return result;
        }

        private static BlueprintBuff CreateMarker(string suffix)
        {
            var result = ScriptableObject.CreateInstance<BlueprintBuff>();
            result.name = "KMG_SlingersLuck_" + suffix + "_Armed";
            result.IsClassFeature = true;
            result.Stacking = StackingType.Replace;
            return result;
        }

        private static BlueprintAbility CreateAbility(string label, int cost,
            BlueprintBuff marker, BlueprintAbilityResource grit)
        {
            var result = ScriptableObject.CreateInstance<BlueprintAbility>();
            result.name = "KMG_SlingersLuck_" + label.Replace(" ", "") +
                "_Ability";
            BlueprintUnitFactAccess.Resolve().Configure(result,
                LocalizationService.Create("KMG.SlingersLuck." +
                    label.Replace(" ", "") + ".Name",
                    "Slinger's Luck - " + label),
                LocalizationService.Create("KMG.SlingersLuck." +
                    label.Replace(" ", "") + ".Description",
                    "Arm the next " + label.ToLowerInvariant() +
                    " reroll. It spends exactly " + cost +
                    " grit and always keeps the second result, even if lower."),
                null);
            result.Type = AbilityType.Extraordinary;
            result.Range = AbilityRange.Personal;
            result.CanTargetSelf = true;
            result.CanTargetPoint = result.CanTargetEnemies =
                result.CanTargetFriends = false;
            result.SpellResistance = false;
            result.Hidden = false;
            result.ActionBarAutoFillIgnored = false;
            result.EffectOnAlly = AbilityEffectOnUnit.Helpful;
            result.EffectOnEnemy = AbilityEffectOnUnit.None;
            result.Animation = UnitAnimationActionCastSpell.CastAnimationStyle.Self;
            result.ActionType = UnitCommand.CommandType.Free;
            result.ResourceAssetIds = Array.Empty<string>();
            var logic = ScriptableObject.CreateInstance<SlingersLuckAbilityLogic>();
            logic.name = "$KMG_SlingersLuck_Arm";
            logic.ArmedMarker = marker; logic.Grit = grit; logic.Cost = cost;
            result.ComponentsArray = new BlueprintComponent[] { logic };
            return result;
        }

        private static BlueprintFeature CreateFeature(BlueprintAbility saving,
            BlueprintAbility skill)
        {
            var result = ScriptableObject.CreateInstance<BlueprintFeature>();
            result.name = "KMG_SlingersLuck_Feature";
            result.Ranks = 1; result.IsClassFeature = true; result.HideInUI = false;
            var add = ScriptableObject.CreateInstance<AddFacts>();
            add.name = "$KMG_SlingersLuck_Grant";
            add.Facts = new BlueprintUnitFact[] { saving, skill };
            add.DoNotRestoreMissingFacts = false;
            result.ComponentsArray = new BlueprintComponent[] { add };
            BlueprintUnitFactAccess.Resolve().Configure(result,
                LocalizationService.Create("KMG.SlingersLuck.Feature.Name",
                    "Slinger's Luck"),
                LocalizationService.Create("KMG.SlingersLuck.Feature.Description",
                    "Arm a mandatory reroll of your next saving throw for 2 grit or skill check for 1 grit. These fixed costs cannot be reduced."),
                null);
            return result;
        }

        private static void Validate(BlueprintFeature feature,
            BlueprintAbility saving, BlueprintAbility skill,
            BlueprintBuff savingMarker, BlueprintBuff skillMarker,
            BlueprintAbilityResource grit, BlueprintCharacterClass gunslingerClass)
        {
            AddFacts grant = feature.ComponentsArray.OfType<AddFacts>().Single();
            SlingersLuckAbilityLogic savingAbility = saving.ComponentsArray
                .OfType<SlingersLuckAbilityLogic>().Single();
            SlingersLuckAbilityLogic skillAbility = skill.ComponentsArray
                .OfType<SlingersLuckAbilityLogic>().Single();
            var savingReroll = savingMarker.ComponentsArray.OfType<
                SlingersLuckSavingThrowReroll>().Single();
            var skillReroll = skillMarker.ComponentsArray.OfType<
                SlingersLuckSkillCheckReroll>().Single();
            if (grant.Facts.Length != 2 || !grant.Facts.Contains(saving) ||
                !grant.Facts.Contains(skill) || savingAbility.Cost != 2 ||
                skillAbility.Cost != 1 || savingAbility.Grit != grit ||
                skillAbility.Grit != grit || savingReroll.Grit != grit ||
                skillReroll.Grit != grit ||
                savingReroll.GunslingerClass != gunslingerClass ||
                skillReroll.GunslingerClass != gunslingerClass)
                throw new InvalidOperationException(
                    "Slinger's Luck fixed-cost reroll contract is incomplete.");
        }
    }
}
