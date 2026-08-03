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
    internal sealed class GunslingerDodgeBlueprintSet
    {
        internal GunslingerDodgeBlueprintSet(BlueprintFeature feature,
            BlueprintAbility proneAbility, BlueprintFeature armedProneMarker)
        {
            Feature = feature; ProneAbility = proneAbility;
            ArmedProneMarker = armedProneMarker;
        }
        internal BlueprintFeature Feature { get; private set; }
        internal BlueprintAbility ProneAbility { get; private set; }
        internal BlueprintFeature ArmedProneMarker { get; private set; }
        internal int Count { get { return 3; } }
    }

    internal static class GunslingerDodgeBlueprints
    {
        internal const string FeatureSymbol = "KMG.Deeds.GunslingerDodgeFeature";
        internal const string ProneAbilitySymbol = "KMG.Deeds.GunslingerDodgeProneAbility";
        internal const string ArmedProneSymbol = "KMG.Deeds.GunslingerDodgeProneArmed";

        internal static GunslingerDodgeBlueprintSet Register(BlueprintRegistry registry)
        {
            if (registry == null) throw new ArgumentNullException("registry");
            BlueprintFeature marker = registry.Register<BlueprintFeature>(
                ArmedProneSymbol, CreateMarker);
            BlueprintAbility ability = registry.Register<BlueprintAbility>(
                ProneAbilitySymbol, () => CreateAbility(marker));
            BlueprintFeature feature = registry.Register<BlueprintFeature>(
                FeatureSymbol, () => CreateFeature(ability));
            Validate(feature, ability, marker);
            return new GunslingerDodgeBlueprintSet(feature, ability, marker);
        }

        private static BlueprintFeature CreateMarker()
        {
            var result = ScriptableObject.CreateInstance<BlueprintFeature>();
            result.name = "KMG_GunslingerDodge_ProneArmed";
            result.Ranks = 1; result.IsClassFeature = true; result.HideInUI = true;
            result.ComponentsArray = Array.Empty<BlueprintComponent>();
            return result;
        }

        private static BlueprintAbility CreateAbility(BlueprintFeature marker)
        {
            var result = ScriptableObject.CreateInstance<BlueprintAbility>();
            result.name = "KMG_GunslingerDodge_ProneAbility";
            BlueprintUnitFactAccess.Resolve().Configure(result,
                LocalizationService.Create("KMG.Dodge.Prone.Name",
                    "Gunslinger's Dodge — Drop Prone"),
                LocalizationService.Create("KMG.Dodge.Prone.Description",
                    "Arm a reaction to the next ranged weapon attack. If wearing light or medium armor and carrying a light load, spend 1 grit, drop prone, and gain +4 AC against that attack."), null);
            result.Type = AbilityType.Extraordinary; result.Range = AbilityRange.Personal;
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
                "KMG.Dodge.Prone.Duration", "Until triggered");
            result.LocalizedSavingThrow = LocalizationService.Create(
                "KMG.Dodge.Prone.SavingThrow", "None");
            result.ComponentsArray = new BlueprintComponent[]
                { GunslingerDodgeProneAbilityLogic.Create(marker) };
            return result;
        }

        private static BlueprintFeature CreateFeature(BlueprintAbility ability)
        {
            var result = ScriptableObject.CreateInstance<BlueprintFeature>();
            result.name = "KMG_GunslingerDodge_Feature"; result.Ranks = 1;
            result.IsClassFeature = true; result.HideInUI = false;
            var add = ScriptableObject.CreateInstance<AddFacts>();
            add.name = "$KMG_GunslingerDodge_Grant";
            add.Facts = new BlueprintUnitFact[] { ability };
            add.DoNotRestoreMissingFacts = false;
            result.ComponentsArray = new BlueprintComponent[] { add };
            BlueprintUnitFactAccess.Resolve().Configure(result,
                LocalizationService.Create("KMG.Dodge.Feature.Name", "Gunslinger's Dodge"),
                LocalizationService.Create("KMG.Dodge.Feature.Description",
                    "Spend grit to react defensively to ranged attacks. The drop-prone branch is available; the five-foot movement branch remains pending a safe destination-selection adaptation."), null);
            return result;
        }

        private static void Validate(BlueprintFeature feature, BlueprintAbility ability,
            BlueprintFeature marker)
        {
            var logic = ability.ComponentsArray.OfType<GunslingerDodgeProneAbilityLogic>().Single();
            var grant = feature.ComponentsArray.OfType<AddFacts>().Single();
            if (ability.ActionType != UnitCommand.CommandType.Free ||
                !ReferenceEquals(logic.ArmedMarker, marker) || grant.Facts.Length != 1 ||
                !ReferenceEquals(grant.Facts[0], ability) || !marker.HideInUI)
                throw new InvalidOperationException("Gunslinger's Dodge blueprints are incomplete.");
        }
    }
}
