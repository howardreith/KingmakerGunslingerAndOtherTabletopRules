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
    internal sealed class TargetingTorsoBlueprintSet
    {
        internal TargetingTorsoBlueprintSet(BlueprintFeature feature,
            BlueprintAbility ability)
        { Feature = feature; Ability = ability; }
        internal BlueprintFeature Feature { get; private set; }
        internal BlueprintAbility Ability { get; private set; }
        internal int Count { get { return 2; } }
    }

    internal static class TargetingTorsoBlueprints
    {
        internal const string FeatureSymbol = "KMG.Deeds.TargetingTorsoFeature";
        internal const string AbilitySymbol = "KMG.Deeds.TargetingTorsoAbility";

        internal static TargetingTorsoBlueprintSet Register(BlueprintRegistry registry)
        {
            BlueprintAbility ability = registry.Register<BlueprintAbility>(
                AbilitySymbol, CreateAbility);
            BlueprintFeature feature = registry.Register<BlueprintFeature>(
                FeatureSymbol, () => CreateFeature(ability));
            Validate(feature, ability);
            return new TargetingTorsoBlueprintSet(feature, ability);
        }

        private static BlueprintAbility CreateAbility()
        {
            var result = ScriptableObject.CreateInstance<BlueprintAbility>();
            result.name = "KMG_TargetingTorso_Ability";
            BlueprintUnitFactAccess.Resolve().Configure(result,
                LocalizationService.Create("KMG.TargetingTorso.Ability.Name",
                    "Targeting — Torso"),
                LocalizationService.Create("KMG.TargetingTorso.Ability.Description",
                    "Spend 1 grit as a full-round action to make one firearm attack that threatens a critical on 19–20 against a creature not immune to sneak attacks."), null);
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
            var logic = ScriptableObject.CreateInstance<TargetingTorsoAbilityLogic>();
            logic.name = "$KMG_TargetingTorso_Delivery";
            result.ComponentsArray = new BlueprintComponent[] { logic };
            return result;
        }

        private static BlueprintFeature CreateFeature(BlueprintAbility ability)
        {
            var result = ScriptableObject.CreateInstance<BlueprintFeature>();
            result.name = "KMG_TargetingTorso_Feature";
            result.Ranks = 1; result.IsClassFeature = true; result.HideInUI = false;
            var add = ScriptableObject.CreateInstance<AddFacts>();
            add.name = "$KMG_TargetingTorso_Grant";
            add.Facts = new BlueprintUnitFact[] { ability };
            add.DoNotRestoreMissingFacts = false;
            result.ComponentsArray = new BlueprintComponent[] { add };
            BlueprintUnitFactAccess.Resolve().Configure(result,
                LocalizationService.Create("KMG.TargetingTorso.Feature.Name",
                    "Targeting — Torso"),
                LocalizationService.Create("KMG.TargetingTorso.Feature.Description",
                    "Make a precise firearm attack that threatens a critical on 19–20."), null);
            return result;
        }

        private static void Validate(BlueprintFeature feature,
            BlueprintAbility ability)
        {
            AddFacts grant = feature.ComponentsArray.OfType<AddFacts>().Single();
            if (!ability.IsFullRoundAction || ability.Range != AbilityRange.Weapon ||
                !ability.CanTargetEnemies || grant.Facts.Length != 1 ||
                !ReferenceEquals(grant.Facts[0], ability) ||
                ability.ComponentsArray.OfType<TargetingTorsoAbilityLogic>().Count() != 1)
                throw new InvalidOperationException(
                    "Targeting Torso blueprint contract is incomplete.");
        }
    }
}
