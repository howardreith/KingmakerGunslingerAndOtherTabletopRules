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
    internal sealed class DeadShotBlueprintSet
    {
        internal DeadShotBlueprintSet(BlueprintFeature feature,
            BlueprintAbility ability) { Feature = feature; Ability = ability; }
        internal BlueprintFeature Feature { get; private set; }
        internal BlueprintAbility Ability { get; private set; }
        internal int Count { get { return 2; } }
    }

    internal static class DeadShotBlueprints
    {
        internal const string FeatureSymbol = "KMG.Deeds.DeadShotFeature";
        internal const string AbilitySymbol = "KMG.Deeds.DeadShotAbility";

        internal static DeadShotBlueprintSet Register(BlueprintRegistry registry)
        {
            if (registry == null) throw new ArgumentNullException("registry");
            BlueprintAbility ability = registry.Register<BlueprintAbility>(
                AbilitySymbol, CreateAbility);
            BlueprintFeature feature = registry.Register<BlueprintFeature>(
                FeatureSymbol, () => CreateFeature(ability));
            Validate(feature, ability);
            return new DeadShotBlueprintSet(feature, ability);
        }

        private static BlueprintAbility CreateAbility()
        {
            var result = ScriptableObject.CreateInstance<BlueprintAbility>();
            result.name = "KMG_DeadShot_Ability";
            BlueprintUnitFactAccess.Resolve().Configure(result,
                LocalizationService.Create("KMG.DeadShot.Ability.Name", "Dead Shot"),
                LocalizationService.Create("KMG.DeadShot.Ability.Description",
                    "Spend 1 grit and one loaded chamber as a full-round action. Roll each BAB iterative against one target; every hit after the first adds the firearm's base damage dice, and the shot misfires only if every roll misfires."), null);
            result.Type = AbilityType.Extraordinary;
            result.Range = AbilityRange.Weapon;
            result.CanTargetEnemies = true;
            result.CanTargetSelf = result.CanTargetFriends = result.CanTargetPoint = false;
            result.SpellResistance = false; result.Hidden = false;
            result.ActionBarAutoFillIgnored = false; result.NeedEquipWeapons = true;
            result.EffectOnEnemy = AbilityEffectOnUnit.Harmful;
            result.EffectOnAlly = AbilityEffectOnUnit.None;
            result.Animation = UnitAnimationActionCastSpell.CastAnimationStyle.Special;
            result.ActionType = UnitCommand.CommandType.Standard;
            result.SetIsFullRoundAction(true);
            result.ResourceAssetIds = Array.Empty<string>();
            var logic = ScriptableObject.CreateInstance<DeadShotAbilityLogic>();
            logic.name = "$KMG_DeadShot_Delivery";
            result.ComponentsArray = new BlueprintComponent[] { logic };
            return result;
        }

        private static BlueprintFeature CreateFeature(BlueprintAbility ability)
        {
            var result = ScriptableObject.CreateInstance<BlueprintFeature>();
            result.name = "KMG_DeadShot_Feature";
            result.Ranks = 1; result.IsClassFeature = true; result.HideInUI = false;
            var add = ScriptableObject.CreateInstance<AddFacts>();
            add.name = "$KMG_DeadShot_Grant";
            add.Facts = new BlueprintUnitFact[] { ability };
            add.DoNotRestoreMissingFacts = false;
            result.ComponentsArray = new BlueprintComponent[] { add };
            BlueprintUnitFactAccess.Resolve().Configure(result,
                LocalizationService.Create("KMG.DeadShot.Feature.Name", "Dead Shot"),
                LocalizationService.Create("KMG.DeadShot.Feature.Description",
                    "Pool your iterative firearm attacks into one deadly shot."), null);
            return result;
        }

        private static void Validate(BlueprintFeature feature,
            BlueprintAbility ability)
        {
            AddFacts grant = feature.ComponentsArray.OfType<AddFacts>().Single();
            if (ability.ActionType != UnitCommand.CommandType.Standard ||
                !ability.IsFullRoundAction || ability.Range != AbilityRange.Weapon ||
                !ability.CanTargetEnemies || grant.Facts.Length != 1 ||
                !ReferenceEquals(grant.Facts[0], ability) ||
                ability.ComponentsArray.OfType<DeadShotAbilityLogic>().Count() != 1)
                throw new InvalidOperationException(
                    "Dead Shot blueprint contract is incomplete.");
        }
    }
}
