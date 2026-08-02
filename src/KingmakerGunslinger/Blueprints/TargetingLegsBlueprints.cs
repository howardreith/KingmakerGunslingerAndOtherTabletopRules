using System;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.Visual.Animation.Kingmaker.Actions;
using KingmakerGunslinger.Deeds;
using UnityEngine;

namespace KingmakerGunslinger.Blueprints
{
    internal sealed class TargetingLegsBlueprintSet
    {
        internal TargetingLegsBlueprintSet(BlueprintFeature feature,
            BlueprintAbility ability)
        { Feature = feature; Ability = ability; }
        internal BlueprintFeature Feature { get; private set; }
        internal BlueprintAbility Ability { get; private set; }
        internal int Count { get { return 2; } }
    }

    internal static class TargetingLegsBlueprints
    {
        internal const string FeatureSymbol = "KMG.Deeds.TargetingLegsFeature";
        internal const string AbilitySymbol = "KMG.Deeds.TargetingLegsAbility";

        internal static TargetingLegsBlueprintSet Register(BlueprintRegistry registry)
        {
            BlueprintAbility ability = registry.Register<BlueprintAbility>(
                AbilitySymbol, CreateAbility);
            BlueprintFeature feature = registry.Register<BlueprintFeature>(
                FeatureSymbol, () => CreateFeature(ability));
            Validate(feature, ability);
            return new TargetingLegsBlueprintSet(feature, ability);
        }

        private static BlueprintAbility CreateAbility()
        {
            var result = ScriptableObject.CreateInstance<BlueprintAbility>();
            result.name = "KMG_TargetingLegs_Ability";
            BlueprintUnitFactAccess.Resolve().Configure(result,
                LocalizationService.Create("KMG.TargetingLegs.Ability.Name",
                    "Targeting — Legs"),
                LocalizationService.Create("KMG.TargetingLegs.Ability.Description",
                    "Spend 1 grit as a full-round action to make one firearm attack. On a hit, an eligible creature is damaged normally and knocked prone."), null);
            result.Type = AbilityType.Extraordinary;
            result.Range = AbilityRange.Weapon;
            result.CanTargetEnemies = true;
            result.CanTargetSelf = result.CanTargetFriends =
                result.CanTargetPoint = false;
            result.SpellResistance = false;
            result.Hidden = false;
            result.ActionBarAutoFillIgnored = false;
            result.NeedEquipWeapons = true;
            result.EffectOnEnemy = AbilityEffectOnUnit.Harmful;
            result.EffectOnAlly = AbilityEffectOnUnit.None;
            result.Animation = UnitAnimationActionCastSpell.CastAnimationStyle.Special;
            result.ActionType = UnitCommand.CommandType.Standard;
            result.SetIsFullRoundAction(true);
            result.ResourceAssetIds = Array.Empty<string>();
            var logic = ScriptableObject.CreateInstance<TargetingLegsAbilityLogic>();
            logic.name = "$KMG_TargetingLegs_Delivery";
            result.ComponentsArray = new BlueprintComponent[] { logic };
            return result;
        }

        private static BlueprintFeature CreateFeature(BlueprintAbility ability)
        {
            var result = ScriptableObject.CreateInstance<BlueprintFeature>();
            result.name = "KMG_TargetingLegs_Feature";
            result.Ranks = 1; result.IsClassFeature = true; result.HideInUI = false;
            var add = ScriptableObject.CreateInstance<AddFacts>();
            add.name = "$KMG_TargetingLegs_Grant";
            add.Facts = new BlueprintUnitFact[] { ability };
            add.DoNotRestoreMissingFacts = false;
            result.ComponentsArray = new BlueprintComponent[] { add };
            BlueprintUnitFactAccess.Resolve().Configure(result,
                LocalizationService.Create("KMG.TargetingLegs.Feature.Name",
                    "Targeting — Legs"),
                LocalizationService.Create("KMG.TargetingLegs.Feature.Description",
                    "Make a precise firearm attack that can knock its target prone."), null);
            return result;
        }

        private static void Validate(BlueprintFeature feature,
            BlueprintAbility ability)
        {
            AddFacts grant = feature.ComponentsArray.OfType<AddFacts>().Single();
            if (!ability.IsFullRoundAction || ability.Range != AbilityRange.Weapon ||
                !ability.CanTargetEnemies || grant.Facts.Length != 1 ||
                !ReferenceEquals(grant.Facts[0], ability) ||
                ability.ComponentsArray.OfType<TargetingLegsAbilityLogic>().Count() != 1)
                throw new InvalidOperationException(
                    "Targeting Legs blueprint contract is incomplete.");
        }
    }
}
