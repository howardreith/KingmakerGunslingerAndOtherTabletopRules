using System;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.ElementsSystem;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
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

        internal static DeadeyeBlueprintSet Register(BlueprintRegistry registry,
            BlueprintAbilityResource grit)
        {
            if (registry == null) throw new ArgumentNullException("registry");
            if (grit == null) throw new ArgumentNullException("grit");
            BlueprintFeature marker = registry.Register<BlueprintFeature>(
                ArmedMarkerSymbol, CreateMarker);
            BlueprintBuff armedBuff = registry.Register<BlueprintBuff>(
                ArmedBuffSymbol, CreateArmedBuff);
            BlueprintAbility ability = registry.Register<BlueprintAbility>(
                AbilitySymbol, () => CreateAbility(marker, armedBuff, grit));
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
                    "Your next firearm attack targets touch AC even beyond the weapon's first range increment. Normal range penalties still apply. This effect ends after 1 round or after that firearm attack, even if it misses."), null);
            return result;
        }

        private static BlueprintAbility CreateAbility(BlueprintFeature marker,
            BlueprintBuff armedBuff, BlueprintAbilityResource grit)
        {
            var result = ScriptableObject.CreateInstance<BlueprintAbility>();
            result.name = "KMG_Deadeye_Ability";
            BlueprintUnitFactAccess.Resolve().Configure(result,
                LocalizationService.Create("KMG.Deadeye.Ability.Name", "Deadeye"),
                LocalizationService.Create("KMG.Deadeye.Ability.Description",
                    "As a free action, spend 1 grit to gain Deadeye Armed for 1 round. Your next firearm attack targets touch AC even beyond the weapon's first range increment. Normal range penalties still apply. The effect ends after that firearm attack, even if it misses."), null);
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
            var apply = new ContextActionApplyBuff
            {
                Buff = armedBuff,
                DurationValue = new ContextDurationValue
                {
                    Rate = DurationRate.Rounds,
                    BonusValue = 1
                },
                IsNotDispelable = true
            };
            var effect = ScriptableObject.CreateInstance<AbilityEffectRunAction>();
            effect.name = "$KMG_Deadeye_ApplyArmedBuff";
            effect.Actions = new ActionList { Actions = new GameAction[] { apply } };
            result.ComponentsArray = new BlueprintComponent[] {
                DeadeyeGritResourceLogic.Create(grit),
                effect };
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
                    "As a free action, spend 1 grit. For 1 round, your next firearm attack targets touch AC even beyond the weapon's first range increment. Normal range penalties still apply. The effect ends after that firearm attack, even if it misses."), null);
            return result;
        }

        private static void Validate(BlueprintFeature feature, BlueprintAbility ability,
            BlueprintFeature marker, BlueprintBuff armedBuff)
        {
            AbilityEffectRunAction effect = ability.ComponentsArray
                .OfType<AbilityEffectRunAction>().Single();
            AddFacts grant = feature.ComponentsArray.OfType<AddFacts>().Single();
            if (ability.ActionType != UnitCommand.CommandType.Free ||
                effect.Actions.Actions.Length != 1 || grant.Facts.Length != 1 ||
                !ReferenceEquals(grant.Facts[0], ability) || !marker.HideInUI)
                throw new InvalidOperationException("Deadeye blueprint contract is incomplete.");
        }
    }
}
