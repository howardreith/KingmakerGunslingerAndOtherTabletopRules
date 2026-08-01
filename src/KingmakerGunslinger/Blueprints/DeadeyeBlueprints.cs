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
    internal sealed class DeadeyeBlueprintSet
    {
        internal DeadeyeBlueprintSet(BlueprintFeature feature, BlueprintAbility ability,
            BlueprintFeature armedMarker)
        {
            Feature = feature; Ability = ability; ArmedMarker = armedMarker;
        }
        internal BlueprintFeature Feature { get; private set; }
        internal BlueprintAbility Ability { get; private set; }
        internal BlueprintFeature ArmedMarker { get; private set; }
        internal int Count { get { return 3; } }
    }

    internal static class DeadeyeBlueprints
    {
        internal const string FeatureSymbol = "KMG.Deeds.DeadeyeFeature";
        internal const string AbilitySymbol = "KMG.Deeds.DeadeyeAbility";
        internal const string ArmedMarkerSymbol = "KMG.Deeds.DeadeyeArmed";

        internal static DeadeyeBlueprintSet Register(BlueprintRegistry registry)
        {
            if (registry == null) throw new ArgumentNullException("registry");
            BlueprintFeature marker = registry.Register<BlueprintFeature>(
                ArmedMarkerSymbol, CreateMarker);
            BlueprintAbility ability = registry.Register<BlueprintAbility>(
                AbilitySymbol, () => CreateAbility(marker));
            BlueprintFeature feature = registry.Register<BlueprintFeature>(
                FeatureSymbol, () => CreateFeature(ability));
            Validate(feature, ability, marker);
            return new DeadeyeBlueprintSet(feature, ability, marker);
        }

        private static BlueprintFeature CreateMarker()
        {
            var result = ScriptableObject.CreateInstance<BlueprintFeature>();
            result.name = "KMG_Deadeye_Armed";
            result.Ranks = 1; result.IsClassFeature = true; result.HideInUI = true;
            result.ComponentsArray = Array.Empty<BlueprintComponent>();
            return result;
        }

        private static BlueprintAbility CreateAbility(BlueprintFeature marker)
        {
            var result = ScriptableObject.CreateInstance<BlueprintAbility>();
            result.name = "KMG_Deadeye_Ability";
            BlueprintUnitFactAccess.Resolve().Configure(result,
                LocalizationService.Create("KMG.Deadeye.Ability.Name", "Deadeye"),
                LocalizationService.Create("KMG.Deadeye.Ability.Description",
                    "As a free action, arm Deadeye for your next firearm shot. Beyond the first range increment, spend 1 grit per additional increment to resolve the shot against touch AC; normal range penalties still apply."), null);
            result.Type = AbilityType.Extraordinary;
            result.Range = AbilityRange.Personal;
            result.CanTargetSelf = true;
            result.CanTargetPoint = result.CanTargetEnemies = result.CanTargetFriends = false;
            result.SpellResistance = false; result.Hidden = false;
            result.ActionBarAutoFillIgnored = false; result.NeedEquipWeapons = false;
            result.EffectOnAlly = AbilityEffectOnUnit.Helpful;
            result.EffectOnEnemy = AbilityEffectOnUnit.None;
            result.Animation = UnitAnimationActionCastSpell.CastAnimationStyle.Self;
            result.ActionType = UnitCommand.CommandType.Free;
            result.ResourceAssetIds = Array.Empty<string>();
            result.ComponentsArray = new BlueprintComponent[] { DeadeyeAbilityLogic.Create(marker) };
            return result;
        }

        private static BlueprintFeature CreateFeature(BlueprintAbility ability)
        {
            var result = ScriptableObject.CreateInstance<BlueprintFeature>();
            result.name = "KMG_Deadeye_Feature"; result.Ranks = 1;
            result.IsClassFeature = true; result.HideInUI = false;
            var add = ScriptableObject.CreateInstance<AddFacts>();
            add.name = "$KMG_Deadeye_Grant"; add.Facts = new BlueprintUnitFact[] { ability };
            add.DoNotRestoreMissingFacts = false;
            result.ComponentsArray = new BlueprintComponent[] { add };
            BlueprintUnitFactAccess.Resolve().Configure(result,
                LocalizationService.Create("KMG.Deadeye.Feature.Name", "Deadeye"),
                LocalizationService.Create("KMG.Deadeye.Feature.Description",
                    "Spend grit to resolve firearm attacks beyond the first range increment against touch AC."), null);
            return result;
        }

        private static void Validate(BlueprintFeature feature, BlueprintAbility ability,
            BlueprintFeature marker)
        {
            DeadeyeAbilityLogic logic = ability.ComponentsArray.OfType<DeadeyeAbilityLogic>().Single();
            AddFacts grant = feature.ComponentsArray.OfType<AddFacts>().Single();
            if (ability.ActionType != UnitCommand.CommandType.Free ||
                !ReferenceEquals(logic.ArmedMarker, marker) || grant.Facts.Length != 1 ||
                !ReferenceEquals(grant.Facts[0], ability) || !marker.HideInUI)
                throw new InvalidOperationException("Deadeye blueprint contract is incomplete.");
        }
    }
}
