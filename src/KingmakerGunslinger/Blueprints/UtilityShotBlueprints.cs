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
    internal sealed class UtilityShotBlueprintSet
    {
        internal UtilityShotBlueprintSet(BlueprintFeature feature,
            BlueprintAbility stopBleeding)
        {
            Feature = feature;
            StopBleeding = stopBleeding;
        }
        internal BlueprintFeature Feature { get; private set; }
        internal BlueprintAbility StopBleeding { get; private set; }
        internal int Count { get { return 2; } }
    }

    internal static class UtilityShotBlueprints
    {
        internal const string FeatureSymbol = "KMG.Deeds.UtilityShotFeature";
        internal const string StopBleedingSymbol =
            "KMG.Deeds.UtilityShotStopBleedingAbility";

        internal static UtilityShotBlueprintSet Register(BlueprintRegistry registry)
        {
            if (registry == null) throw new ArgumentNullException("registry");
            BlueprintAbility ability = registry.Register<BlueprintAbility>(
                StopBleedingSymbol, CreateStopBleeding);
            BlueprintFeature feature = registry.Register<BlueprintFeature>(
                FeatureSymbol, () => CreateFeature(ability));
            Validate(feature, ability);
            return new UtilityShotBlueprintSet(feature, ability);
        }

        private static BlueprintAbility CreateStopBleeding()
        {
            var result = ScriptableObject.CreateInstance<BlueprintAbility>();
            result.name = "KMG_UtilityShot_StopBleeding_Ability";
            BlueprintUnitFactAccess.Resolve().Configure(result,
                LocalizationService.Create("KMG.UtilityShot.StopBleeding.Name",
                    "Utility Shot — Stop Bleeding"),
                LocalizationService.Create("KMG.UtilityShot.StopBleeding.Description",
                    "While you have grit, fire one loaded chamber without an attack roll to end one bleed effect on yourself or an adjacent ally."), null);
            result.Type = AbilityType.Extraordinary;
            result.Range = AbilityRange.Touch;
            result.CanTargetSelf = true;
            result.CanTargetFriends = true;
            result.CanTargetEnemies = result.CanTargetPoint = false;
            result.SpellResistance = false;
            result.Hidden = false;
            result.ActionBarAutoFillIgnored = false;
            result.NeedEquipWeapons = false;
            result.EffectOnAlly = AbilityEffectOnUnit.Helpful;
            result.EffectOnEnemy = AbilityEffectOnUnit.None;
            result.Animation = UnitAnimationActionCastSpell.CastAnimationStyle.Touch;
            result.ActionType = UnitCommand.CommandType.Standard;
            result.ResourceAssetIds = Array.Empty<string>();
            result.ComponentsArray = new BlueprintComponent[] {
                StopBleedingAbilityLogic.Create() };
            return result;
        }

        private static BlueprintFeature CreateFeature(BlueprintAbility ability)
        {
            var result = ScriptableObject.CreateInstance<BlueprintFeature>();
            result.name = "KMG_UtilityShot_Feature";
            result.Ranks = 1;
            result.IsClassFeature = true;
            result.HideInUI = false;
            var add = ScriptableObject.CreateInstance<AddFacts>();
            add.name = "$KMG_UtilityShot_Grant";
            add.Facts = new BlueprintUnitFact[] { ability };
            add.DoNotRestoreMissingFacts = false;
            result.ComponentsArray = new BlueprintComponent[] { add };
            BlueprintUnitFactAccess.Resolve().Configure(result,
                LocalizationService.Create("KMG.UtilityShot.Feature.Name",
                    "Utility Shot"),
                LocalizationService.Create("KMG.UtilityShot.Feature.Description",
                    "Use a firearm shot to stop one adjacent creature's bleeding. Lock and unattended-object branches have no supported Kingmaker interaction."), null);
            return result;
        }

        private static void Validate(BlueprintFeature feature,
            BlueprintAbility ability)
        {
            AddFacts grant = feature.ComponentsArray.OfType<AddFacts>().Single();
            if (grant.Facts.Length != 1 ||
                !ReferenceEquals(grant.Facts[0], ability) ||
                ability.ActionType != UnitCommand.CommandType.Standard ||
                ability.Range != AbilityRange.Touch ||
                !ability.CanTargetSelf || !ability.CanTargetFriends ||
                ability.CanTargetEnemies ||
                ability.ComponentsArray.OfType<StopBleedingAbilityLogic>().Count() != 1)
                throw new InvalidOperationException(
                    "Utility Shot blueprint contract is incomplete.");
        }
    }
}
