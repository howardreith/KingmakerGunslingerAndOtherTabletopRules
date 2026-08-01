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
    internal sealed class QuickClearBlueprintSet
    {
        internal QuickClearBlueprintSet(BlueprintFeature feature,
            BlueprintAbility standard, BlueprintAbility move)
        { Feature = feature; StandardAbility = standard; MoveAbility = move; }
        internal BlueprintFeature Feature { get; private set; }
        internal BlueprintAbility StandardAbility { get; private set; }
        internal BlueprintAbility MoveAbility { get; private set; }
        internal int Count { get { return 3; } }
    }

    internal static class QuickClearBlueprints
    {
        internal const string FeatureSymbol = "KMG.Deeds.QuickClearFeature";
        internal const string StandardSymbol = "KMG.Deeds.QuickClearStandardAbility";
        internal const string MoveSymbol = "KMG.Deeds.QuickClearMoveAbility";
        internal static QuickClearBlueprintSet Register(BlueprintRegistry registry)
        {
            BlueprintAbility standard = registry.Register<BlueprintAbility>(StandardSymbol,
                () => CreateAbility(QuickClearMode.Standard, UnitCommand.CommandType.Standard));
            BlueprintAbility move = registry.Register<BlueprintAbility>(MoveSymbol,
                () => CreateAbility(QuickClearMode.Move, UnitCommand.CommandType.Move));
            BlueprintFeature feature = registry.Register<BlueprintFeature>(FeatureSymbol,
                () => CreateFeature(standard, move));
            Validate(feature, standard, move);
            return new QuickClearBlueprintSet(feature, standard, move);
        }
        private static BlueprintAbility CreateAbility(QuickClearMode mode,
            UnitCommand.CommandType action)
        {
            var result = ScriptableObject.CreateInstance<BlueprintAbility>();
            result.name = "KMG_QuickClear_" + mode;
            string suffix = mode == QuickClearMode.Standard ? "Standard" : "Move";
            string detail = mode == QuickClearMode.Standard ?
                "As a standard action, remove Broken without spending grit; requires at least 1 grit." :
                "Spend 1 grit to remove Broken as a move action.";
            BlueprintUnitFactAccess.Resolve().Configure(result,
                LocalizationService.Create("KMG.QuickClear." + suffix + ".Name", "Quick Clear — " + suffix),
                LocalizationService.Create("KMG.QuickClear." + suffix + ".Description", detail), null);
            result.Type = AbilityType.Extraordinary; result.Range = AbilityRange.Personal;
            result.CanTargetSelf = true; result.CanTargetPoint = result.CanTargetEnemies = result.CanTargetFriends = false;
            result.SpellResistance = false; result.Hidden = false; result.ActionBarAutoFillIgnored = false;
            result.NeedEquipWeapons = false; result.EffectOnAlly = AbilityEffectOnUnit.Helpful;
            result.EffectOnEnemy = AbilityEffectOnUnit.None;
            result.Animation = UnitAnimationActionCastSpell.CastAnimationStyle.Self;
            result.ActionType = action; result.ResourceAssetIds = Array.Empty<string>();
            result.ComponentsArray = new BlueprintComponent[] { QuickClearAbilityLogic.Create(mode) };
            return result;
        }
        private static BlueprintFeature CreateFeature(params BlueprintAbility[] abilities)
        {
            var result = ScriptableObject.CreateInstance<BlueprintFeature>();
            result.name = "KMG_QuickClear_Feature"; result.Ranks = 1;
            result.IsClassFeature = true; result.HideInUI = false;
            var add = ScriptableObject.CreateInstance<AddFacts>();
            add.name = "$KMG_QuickClear_Grant"; add.Facts = abilities;
            add.DoNotRestoreMissingFacts = false;
            result.ComponentsArray = new BlueprintComponent[] { add };
            BlueprintUnitFactAccess.Resolve().Configure(result,
                LocalizationService.Create("KMG.QuickClear.Feature.Name", "Quick Clear"),
                LocalizationService.Create("KMG.QuickClear.Feature.Description",
                    "Remove the Broken condition from the single firearm you wield after a misfire."), null);
            return result;
        }
        private static void Validate(BlueprintFeature feature, BlueprintAbility standard,
            BlueprintAbility move)
        {
            AddFacts grant = feature.ComponentsArray.OfType<AddFacts>().Single();
            if (standard.ActionType != UnitCommand.CommandType.Standard ||
                move.ActionType != UnitCommand.CommandType.Move || grant.Facts.Length != 2 ||
                !ReferenceEquals(grant.Facts[0], standard) || !ReferenceEquals(grant.Facts[1], move))
                throw new InvalidOperationException("Quick Clear blueprints are incomplete.");
        }
    }
}
