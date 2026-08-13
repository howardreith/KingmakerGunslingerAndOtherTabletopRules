using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.ResourceLinks;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.FactLogic;
using UnityEngine;

namespace KingmakerGunslinger.Blueprints
{
    internal sealed class ExpandedSummoningAlignmentModeBlueprintSet
    {
        internal ExpandedSummoningAlignmentModeBlueprintSet(BlueprintFeature feature,
            BlueprintBuff marker, BlueprintActivatableAbility ability)
        {
            Feature = feature ?? throw new ArgumentNullException("feature");
            Marker = marker ?? throw new ArgumentNullException("marker");
            Ability = ability ?? throw new ArgumentNullException("ability");
        }

        internal BlueprintFeature Feature { get; private set; }
        internal BlueprintBuff Marker { get; private set; }
        internal BlueprintActivatableAbility Ability { get; private set; }
    }

    internal static class ExpandedSummoningAlignmentModeBlueprints
    {
        internal const string FeatureSymbol =
            "KMG.Summoning.AlignmentMode.Feature";
        internal const string MarkerSymbol =
            "KMG.Summoning.AlignmentMode.FiendishMarker";
        internal const string AbilitySymbol =
            "KMG.Summoning.AlignmentMode.Toggle";

        internal static ExpandedSummoningAlignmentModeBlueprintSet Configure(
            BlueprintFeature feature, BlueprintBuff marker,
            BlueprintActivatableAbility ability, Sprite icon)
        {
            if (feature == null || marker == null || ability == null || icon == null)
                throw new ArgumentNullException(
                    "Expanded Summoning alignment-mode blueprints and icon are required.");

            const string name = "Fiendish Summoning for Neutral Casters";
            const string description =
                "Neutral characters summon celestial creatures while this mode is off and fiendish creatures while it is on. Good characters always summon celestial creatures and evil characters always summon fiendish creatures, regardless of this mode.";

            marker.name = "KMG_ExpandedSummoning_FiendishMode_Marker";
            marker.ComponentsArray = Array.Empty<BlueprintComponent>();
            marker.FxOnStart = new PrefabLink();
            marker.FxOnRemove = new PrefabLink();
            marker.ResourceAssetIds = Array.Empty<string>();
            BlueprintUnitFactAccess.Resolve().Configure(marker,
                LocalizationService.Create("KMG.ExpandedSummoning.AlignmentMode.Marker.Name",
                    name),
                LocalizationService.Create(
                    "KMG.ExpandedSummoning.AlignmentMode.Marker.Description",
                    description), icon);

            ability.name = "KMG_ExpandedSummoning_FiendishMode_Toggle";
            ability.Buff = marker;
            ability.Group = ActivatableAbilityGroup.None;
            ability.WeightInGroup = 1;
            ability.IsOnByDefault = false;
            ability.ActivationType = AbilityActivationType.Immediately;
            ability.DeactivateIfCombatEnded = false;
            ability.DeactivateAfterFirstRound = false;
            ability.DeactivateImmediately = false;
            ability.DeactivateIfOwnerDisabled = false;
            ability.DeactivateIfOwnerUnconscious = false;
            ability.OnlyInCombat = false;
            ability.ActionBarAutoFillIgnored = true;
            ability.ComponentsArray = Array.Empty<BlueprintComponent>();
            ability.ResourceAssetIds = Array.Empty<string>();
            BlueprintUnitFactAccess.Resolve().Configure(ability,
                LocalizationService.Create("KMG.ExpandedSummoning.AlignmentMode.Name",
                    name),
                LocalizationService.Create(
                    "KMG.ExpandedSummoning.AlignmentMode.Description", description),
                icon);

            feature.name = "KMG_ExpandedSummoning_AlignmentMode_Feature";
            feature.Ranks = 1;
            feature.HideInUI = true;
            feature.IsClassFeature = false;
            var grant = ScriptableObject.CreateInstance<AddFacts>();
            grant.name = "$KMG_GrantExpandedSummoningAlignmentMode";
            grant.Facts = new BlueprintUnitFact[] { ability };
            grant.DoNotRestoreMissingFacts = false;
            feature.ComponentsArray = new BlueprintComponent[] { grant };
            BlueprintUnitFactAccess.Resolve().Configure(feature,
                LocalizationService.Create(
                    "KMG.ExpandedSummoning.AlignmentMode.Feature.Name", name),
                LocalizationService.Create(
                    "KMG.ExpandedSummoning.AlignmentMode.Feature.Description",
                    description), icon);

            return new ExpandedSummoningAlignmentModeBlueprintSet(feature, marker,
                ability);
        }
    }
}
