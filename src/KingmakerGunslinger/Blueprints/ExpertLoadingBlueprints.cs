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
    internal sealed class ExpertLoadingBlueprintSet
    {
        internal ExpertLoadingBlueprintSet(BlueprintFeature feature,
            BlueprintAbility ability, BlueprintBuff armedMarker)
        {
            Feature = feature; Ability = ability; ArmedMarker = armedMarker;
        }
        internal BlueprintFeature Feature { get; private set; }
        internal BlueprintAbility Ability { get; private set; }
        internal BlueprintBuff ArmedMarker { get; private set; }
        internal int Count { get { return 3; } }
    }

    internal static class ExpertLoadingBlueprints
    {
        internal const string FeatureSymbol = "KMG.Deeds.ExpertLoadingFeature";
        internal const string AbilitySymbol = "KMG.Deeds.ExpertLoadingAbility";
        internal const string MarkerSymbol = "KMG.Deeds.ExpertLoadingArmed";

        internal static ExpertLoadingBlueprintSet Register(BlueprintRegistry registry)
        {
            if (registry == null) throw new ArgumentNullException("registry");
            BlueprintBuff marker = registry.Register<BlueprintBuff>(MarkerSymbol,
                CreateMarker);
            BlueprintAbility ability = registry.Register<BlueprintAbility>(
                AbilitySymbol, () => CreateAbility(marker));
            BlueprintFeature feature = registry.Register<BlueprintFeature>(
                FeatureSymbol, () => CreateFeature(ability));
            Validate(feature, ability, marker);
            return new ExpertLoadingBlueprintSet(feature, ability, marker);
        }

        private static BlueprintBuff CreateMarker()
        {
            var result = ScriptableObject.CreateInstance<BlueprintBuff>();
            result.name = "KMG_ExpertLoading_Armed";
            result.IsClassFeature = true;
            result.Stacking = StackingType.Replace;
            result.ComponentsArray = Array.Empty<BlueprintComponent>();
            return result;
        }

        private static BlueprintAbility CreateAbility(BlueprintBuff marker)
        {
            var result = ScriptableObject.CreateInstance<BlueprintAbility>();
            result.name = "KMG_ExpertLoading_Ability";
            BlueprintUnitFactAccess.Resolve().Configure(result,
                LocalizationService.Create("KMG.ExpertLoading.Ability.Name",
                    "Expert Loading"),
                LocalizationService.Create("KMG.ExpertLoading.Ability.Description",
                    "As a free action, arm your next firearm attack. If a misfire with a Broken early firearm would make it explode, spend 1 grit to keep it Broken and prevent the explosion."), null);
            result.Type = AbilityType.Extraordinary;
            result.Range = AbilityRange.Personal;
            result.CanTargetSelf = true;
            result.CanTargetPoint = result.CanTargetEnemies =
                result.CanTargetFriends = false;
            result.SpellResistance = false;
            result.Hidden = false;
            result.ActionBarAutoFillIgnored = false;
            result.NeedEquipWeapons = false;
            result.EffectOnAlly = AbilityEffectOnUnit.Helpful;
            result.EffectOnEnemy = AbilityEffectOnUnit.None;
            result.Animation = UnitAnimationActionCastSpell.CastAnimationStyle.Self;
            result.ActionType = UnitCommand.CommandType.Free;
            result.ResourceAssetIds = Array.Empty<string>();
            var logic = ScriptableObject.CreateInstance<ExpertLoadingAbilityLogic>();
            logic.name = "$KMG_ExpertLoading_Arm";
            logic.ArmedMarker = marker;
            result.ComponentsArray = new BlueprintComponent[] { logic };
            return result;
        }

        private static BlueprintFeature CreateFeature(BlueprintAbility ability)
        {
            var result = ScriptableObject.CreateInstance<BlueprintFeature>();
            result.name = "KMG_ExpertLoading_Feature";
            result.Ranks = 1; result.IsClassFeature = true; result.HideInUI = false;
            var add = ScriptableObject.CreateInstance<AddFacts>();
            add.name = "$KMG_ExpertLoading_Grant";
            add.Facts = new BlueprintUnitFact[] { ability };
            add.DoNotRestoreMissingFacts = false;
            result.ComponentsArray = new BlueprintComponent[] { add };
            BlueprintUnitFactAccess.Resolve().Configure(result,
                LocalizationService.Create("KMG.ExpertLoading.Feature.Name",
                    "Expert Loading"),
                LocalizationService.Create("KMG.ExpertLoading.Feature.Description",
                    "Spend 1 grit to prevent an armed Broken-firearm misfire from wrecking and exploding the firearm."), null);
            return result;
        }

        private static void Validate(BlueprintFeature feature,
            BlueprintAbility ability, BlueprintBuff marker)
        {
            AddFacts add = feature.ComponentsArray.OfType<AddFacts>().Single();
            ExpertLoadingAbilityLogic logic = ability.ComponentsArray
                .OfType<ExpertLoadingAbilityLogic>().Single();
            if (add.Facts.Length != 1 || !ReferenceEquals(add.Facts[0], ability) ||
                !ReferenceEquals(logic.ArmedMarker, marker) ||
                ability.ActionType != UnitCommand.CommandType.Free)
                throw new InvalidOperationException(
                    "Expert Loading blueprint contract is incomplete.");
        }
    }
}
