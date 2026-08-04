using System;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.Enums;
using Kingmaker.Visual.Animation.Kingmaker.Actions;
using KingmakerGunslinger.Deeds;
using UnityEngine;

namespace KingmakerGunslinger.Blueprints
{
    internal sealed class DeadeyeBlueprintSet
    {
        internal DeadeyeBlueprintSet(BlueprintFeature feature, BlueprintAbility ability,
            BlueprintFeature armedMarker, BlueprintBuff armedBuff)
        {
            Feature = feature; Ability = ability; ArmedMarker = armedMarker;
            ArmedBuff = armedBuff;
        }
        internal BlueprintFeature Feature { get; private set; }
        internal BlueprintAbility Ability { get; private set; }
        internal BlueprintFeature ArmedMarker { get; private set; }
        internal BlueprintBuff ArmedBuff { get; private set; }
        internal int Count { get { return 4; } }
    }

    internal static class DeadeyeBlueprints
    {
        internal const string FeatureSymbol = "KMG.Deeds.DeadeyeFeature";
        internal const string AbilitySymbol = "KMG.Deeds.DeadeyeAbility";
        internal const string ArmedMarkerSymbol = "KMG.Deeds.DeadeyeArmed";
        internal const string ArmedBuffSymbol = "KMG.Deeds.DeadeyeArmedBuff";

        internal static DeadeyeBlueprintSet Register(BlueprintRegistry registry)
        {
            if (registry == null) throw new ArgumentNullException("registry");
            BlueprintFeature marker = registry.Register<BlueprintFeature>(
                ArmedMarkerSymbol, CreateMarker);
            BlueprintBuff armedBuff = registry.Register<BlueprintBuff>(
                ArmedBuffSymbol, CreateArmedBuff);
            BlueprintAbility ability = registry.Register<BlueprintAbility>(
                AbilitySymbol, () => CreateAbility(marker, armedBuff));
            BlueprintFeature feature = registry.Register<BlueprintFeature>(
                FeatureSymbol, () => CreateFeature(ability));
            Validate(feature, ability, marker, armedBuff);
            return new DeadeyeBlueprintSet(feature, ability, marker, armedBuff);
        }

        private static BlueprintFeature CreateMarker()
        {
            var result = ScriptableObject.CreateInstance<BlueprintFeature>();
            result.name = "KMG_Deadeye_Armed";
            result.Ranks = 1; result.IsClassFeature = true; result.HideInUI = true;
            result.ComponentsArray = Array.Empty<BlueprintComponent>();
            return result;
        }

        private static BlueprintBuff CreateArmedBuff()
        {
            var result = ScriptableObject.CreateInstance<BlueprintBuff>();
            result.name = "KMG_Deadeye_Armed_Buff";
            result.IsClassFeature = true;
            result.Stacking = StackingType.Replace;
            result.ComponentsArray = Array.Empty<BlueprintComponent>();
            BlueprintUnitFactAccess.Resolve().Configure(result,
                LocalizationService.Create("KMG.Deadeye.Armed.Name", "Deadeye Armed"),
                LocalizationService.Create("KMG.Deadeye.Armed.Description",
                    "You paid 1 Grit when this buff was activated. Your next valid firearm attack before the end of one round uses Deadeye's touch-AC range behavior; the buff is consumed even if that attack misses."), null);
            return result;
        }

        private static BlueprintAbility CreateAbility(BlueprintFeature marker,
            BlueprintBuff armedBuff)
        {
            var result = ScriptableObject.CreateInstance<BlueprintAbility>();
            result.name = "KMG_Deadeye_Ability";
            BlueprintUnitFactAccess.Resolve().Configure(result,
                LocalizationService.Create("KMG.Deadeye.Ability.Name", "Deadeye"),
                LocalizationService.Create("KMG.Deadeye.Ability.Description",
                    "As a free action, immediately spend 1 Grit and gain Deadeye Armed for one round. Your next valid firearm shot consumes the buff, even on a miss, and resolves range increments against touch AC; normal range penalties still apply."), null);
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
            result.LocalizedDuration = LocalizationService.Create(
                "KMG.Deadeye.Ability.Duration", "1 round or until the next firearm shot");
            result.LocalizedSavingThrow = LocalizationService.Create(
                "KMG.Deadeye.Ability.SavingThrow", "None");
            result.ComponentsArray = new BlueprintComponent[]
                { DeadeyeAbilityLogic.Create(marker, armedBuff) };
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
                    "Immediately spend 1 Grit to arm your next firearm attack for one round. That shot uses Deadeye's touch-AC range behavior and consumes the armed buff even if it misses."), null);
            return result;
        }

        private static void Validate(BlueprintFeature feature, BlueprintAbility ability,
            BlueprintFeature marker, BlueprintBuff armedBuff)
        {
            DeadeyeAbilityLogic logic = ability.ComponentsArray.OfType<DeadeyeAbilityLogic>().Single();
            AddFacts grant = feature.ComponentsArray.OfType<AddFacts>().Single();
            if (ability.ActionType != UnitCommand.CommandType.Free ||
                !ReferenceEquals(logic.ArmedMarker, marker) || grant.Facts.Length != 1 ||
                !ReferenceEquals(logic.ArmedBuff, armedBuff) ||
                !ReferenceEquals(grant.Facts[0], ability) || !marker.HideInUI)
                throw new InvalidOperationException("Deadeye blueprint contract is incomplete.");
        }
    }
}
