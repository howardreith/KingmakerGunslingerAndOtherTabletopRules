using System;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.Visual.Animation.Kingmaker.Actions;
using KingmakerGunslinger.Deeds;
using UnityEngine;

namespace KingmakerGunslinger.Blueprints
{
    internal sealed class LightningReloadBlueprintSet
    {
        internal LightningReloadBlueprintSet(BlueprintFeature feature,
            BlueprintAbility ability, BlueprintBuff usedMarker)
        { Feature = feature; Ability = ability; UsedMarker = usedMarker; }
        internal BlueprintFeature Feature { get; private set; }
        internal BlueprintAbility Ability { get; private set; }
        internal BlueprintBuff UsedMarker { get; private set; }
        internal int Count { get { return 3; } }
    }

    internal static class LightningReloadBlueprints
    {
        internal const string FeatureSymbol = "KMG.Deeds.LightningReloadFeature";
        internal const string AbilitySymbol = "KMG.Deeds.LightningReloadAbility";
        internal const string UsedMarkerSymbol = "KMG.Deeds.LightningReloadUsed";

        internal static LightningReloadBlueprintSet Register(
            BlueprintRegistry registry, BlueprintItem blackPowder,
            BlueprintItem leadBall)
        {
            if (registry == null) throw new ArgumentNullException("registry");
            if (blackPowder == null) throw new ArgumentNullException("blackPowder");
            if (leadBall == null) throw new ArgumentNullException("leadBall");
            BlueprintBuff marker = registry.Register<BlueprintBuff>(
                UsedMarkerSymbol, CreateMarker);
            BlueprintAbility ability = registry.Register<BlueprintAbility>(
                AbilitySymbol, () => CreateAbility(blackPowder, leadBall, marker));
            BlueprintFeature feature = registry.Register<BlueprintFeature>(
                FeatureSymbol, () => CreateFeature(ability));
            Validate(feature, ability, marker);
            return new LightningReloadBlueprintSet(feature, ability, marker);
        }

        private static BlueprintBuff CreateMarker()
        {
            var result = ScriptableObject.CreateInstance<BlueprintBuff>();
            result.name = "KMG_LightningReload_Used";
            result.IsClassFeature = true;
            result.Stacking = StackingType.Replace;
            var reset = ScriptableObject.CreateInstance<LightningReloadRoundMarker>();
            reset.name = "$KMG_LightningReload_RoundReset";
            result.ComponentsArray = new BlueprintComponent[] { reset };
            return result;
        }

        private static BlueprintAbility CreateAbility(BlueprintItem blackPowder,
            BlueprintItem leadBall, BlueprintBuff marker)
        {
            var result = ScriptableObject.CreateInstance<BlueprintAbility>();
            result.name = "KMG_LightningReload_Ability";
            BlueprintUnitFactAccess.Resolve().Configure(result,
                LocalizationService.Create("KMG.LightningReload.Ability.Name",
                    "Lightning Reload"),
                LocalizationService.Create("KMG.LightningReload.Ability.Description",
                    "While you have at least 1 grit, reload one chamber once per round without spending grit. This is a swift action with loose ammunition, free with matching Rapid Reload, and free when Use Paper Cartridges is active. Paper mode never falls back to loose ammunition."), null);
            result.Type = AbilityType.Extraordinary;
            result.Range = AbilityRange.Personal;
            result.CanTargetSelf = true;
            result.CanTargetPoint = result.CanTargetEnemies =
                result.CanTargetFriends = false;
            result.SpellResistance = false;
            result.Hidden = false;
            result.ActionBarAutoFillIgnored = false;
            result.NeedEquipWeapons = true;
            result.EffectOnAlly = AbilityEffectOnUnit.Helpful;
            result.EffectOnEnemy = AbilityEffectOnUnit.None;
            result.Animation = UnitAnimationActionCastSpell.CastAnimationStyle.Self;
            result.ActionType = UnitCommand.CommandType.Swift;
            result.ResourceAssetIds = Array.Empty<string>();
            var logic = ScriptableObject.CreateInstance<LightningReloadAbilityLogic>();
            logic.name = "$KMG_LightningReload_Deliver";
            logic.BlackPowder = blackPowder; logic.LeadBall = leadBall;
            logic.UsedMarker = marker;
            result.ComponentsArray = new BlueprintComponent[] { logic };
            return result;
        }

        private static BlueprintFeature CreateFeature(BlueprintAbility ability)
        {
            var result = ScriptableObject.CreateInstance<BlueprintFeature>();
            result.name = "KMG_LightningReload_Feature";
            result.Ranks = 1; result.IsClassFeature = true; result.HideInUI = false;
            var add = ScriptableObject.CreateInstance<AddFacts>();
            add.name = "$KMG_LightningReload_Grant";
            add.Facts = new BlueprintUnitFact[] { ability };
            add.DoNotRestoreMissingFacts = false;
            result.ComponentsArray = new BlueprintComponent[] { add };
            BlueprintUnitFactAccess.Resolve().Configure(result,
                LocalizationService.Create("KMG.LightningReload.Feature.Name",
                    "Lightning Reload"),
                LocalizationService.Create("KMG.LightningReload.Feature.Description",
                    "While grit remains, reload one equipped firearm chamber once per round as a swift or free action according to Rapid Reload and the selected ammunition."), null);
            return result;
        }

        private static void Validate(BlueprintFeature feature,
            BlueprintAbility ability, BlueprintBuff marker)
        {
            AddFacts add = feature.ComponentsArray.OfType<AddFacts>().Single();
            LightningReloadAbilityLogic logic = ability.ComponentsArray
                .OfType<LightningReloadAbilityLogic>().Single();
            if (add.Facts.Length != 1 || !ReferenceEquals(add.Facts[0], ability) ||
                !ReferenceEquals(logic.UsedMarker, marker) ||
                ability.ActionType != UnitCommand.CommandType.Swift ||
                marker.ComponentsArray.OfType<LightningReloadRoundMarker>().Count() != 1)
                throw new InvalidOperationException(
                    "Lightning Reload blueprint contract is incomplete.");
        }
    }
}
