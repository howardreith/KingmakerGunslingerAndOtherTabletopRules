using System;
using System.Reflection;
using Kingmaker.Blueprints;
using Kingmaker.ResourceLinks;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using UnityEngine;

namespace KingmakerGunslinger.Blueprints
{
    internal sealed class BodyguardModeBlueprintSet
    {
        internal BodyguardModeBlueprintSet(BlueprintBuff bodyguardMarker,
            BlueprintActivatableAbility bodyguardAbility,
            BlueprintBuff inHarmsWayMarker,
            BlueprintActivatableAbility inHarmsWayAbility)
        {
            BodyguardMarker = bodyguardMarker ??
                throw new ArgumentNullException("bodyguardMarker");
            BodyguardAbility = bodyguardAbility ??
                throw new ArgumentNullException("bodyguardAbility");
            InHarmsWayMarker = inHarmsWayMarker ??
                throw new ArgumentNullException("inHarmsWayMarker");
            InHarmsWayAbility = inHarmsWayAbility ??
                throw new ArgumentNullException("inHarmsWayAbility");
        }

        internal BlueprintBuff BodyguardMarker { get; private set; }
        internal BlueprintActivatableAbility BodyguardAbility { get; private set; }
        internal BlueprintBuff InHarmsWayMarker { get; private set; }
        internal BlueprintActivatableAbility InHarmsWayAbility { get; private set; }
        internal int Count { get { return 4; } }
    }

    internal static class BodyguardModeBlueprints
    {
        internal const string BodyguardMarkerSymbol =
            "KMG.Feats.BodyguardModeMarker";
        internal const string BodyguardAbilitySymbol = "KMG.Feats.UseBodyguard";
        internal const string InHarmsWayMarkerSymbol =
            "KMG.Feats.InHarmsWayModeMarker";
        internal const string InHarmsWayAbilitySymbol =
            "KMG.Feats.UseInHarmsWay";
        internal const string BodyguardDisplayName = "Use Bodyguard";
        internal const string BodyguardDescription =
            "When an adjacent ally is attacked by an enemy you threaten, automatically expend one available attack of opportunity to attempt an Aid Another melee attack roll against AC 10. On success, the ally gains your normal Aid Another AC bonus against that attack (normally +2, and increased by effects such as Helpful). The attack of opportunity is spent even if the attempt fails.";
        internal const string InHarmsWayDisplayName = "Use In Harm's Way";
        internal const string InHarmsWayDescription =
            "When your Bodyguard attempt succeeds and the protected ally is still hit, automatically expend an available immediate action to receive that attack's full damage and associated effects. Only one protector can intercept each attack.";

        internal static BodyguardModeBlueprintSet Register(BlueprintRegistry registry,
            Sprite icon)
        {
            if (registry == null) throw new ArgumentNullException("registry");
            if (icon == null) throw new ArgumentNullException("icon");
            BlueprintBuff bodyguardMarker = registry.Register<BlueprintBuff>(
                BodyguardMarkerSymbol, () => CreateMarker(
                    "KMG_BodyguardMode_MarkerBuff", BodyguardDisplayName,
                    BodyguardDescription, "Bodyguard"));
            BlueprintActivatableAbility bodyguardAbility = registry.Register<
                BlueprintActivatableAbility>(BodyguardAbilitySymbol, () =>
                    CreateAbility("KMG_UseBodyguard_ActivatableAbility",
                        BodyguardDisplayName, BodyguardDescription, "Bodyguard",
                        bodyguardMarker, icon));
            BlueprintBuff inHarmsWayMarker = registry.Register<BlueprintBuff>(
                InHarmsWayMarkerSymbol, () => CreateMarker(
                    "KMG_InHarmsWayMode_MarkerBuff", InHarmsWayDisplayName,
                    InHarmsWayDescription, "InHarmsWay"));
            BlueprintActivatableAbility inHarmsWayAbility = registry.Register<
                BlueprintActivatableAbility>(InHarmsWayAbilitySymbol, () =>
                    CreateAbility("KMG_UseInHarmsWay_ActivatableAbility",
                        InHarmsWayDisplayName, InHarmsWayDescription, "InHarmsWay",
                        inHarmsWayMarker, icon));
            Validate(bodyguardMarker, bodyguardAbility);
            Validate(inHarmsWayMarker, inHarmsWayAbility);
            if (bodyguardAbility.Group != ActivatableAbilityGroup.None ||
                inHarmsWayAbility.Group != ActivatableAbilityGroup.None)
                throw new InvalidOperationException(
                    "Bodyguard modes must not share an activatable-ability group.");
            return new BodyguardModeBlueprintSet(bodyguardMarker, bodyguardAbility,
                inHarmsWayMarker, inHarmsWayAbility);
        }

        private static BlueprintBuff CreateMarker(string internalName,
            string displayName, string description, string localizationStem)
        {
            var marker = ScriptableObject.CreateInstance<BlueprintBuff>();
            marker.name = internalName;
            BlueprintUnitFactAccess.Resolve().Configure(marker,
                LocalizationService.Create("KMG.Mode." + localizationStem +
                    ".Marker.Name", displayName),
                LocalizationService.Create("KMG.Mode." + localizationStem +
                    ".Marker.Description", description), null);
            marker.ComponentsArray = Array.Empty<BlueprintComponent>();
            marker.FxOnStart = new PrefabLink();
            marker.FxOnRemove = new PrefabLink();
            marker.ResourceAssetIds = Array.Empty<string>();
            FieldInfo flags = typeof(BlueprintBuff).GetField("m_Flags",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (flags == null || !flags.FieldType.IsEnum)
                throw new MissingFieldException(typeof(BlueprintBuff).FullName,
                    "m_Flags");
            flags.SetValue(marker, Enum.Parse(flags.FieldType, "HiddenInUi"));
            return marker;
        }

        private static BlueprintActivatableAbility CreateAbility(string internalName,
            string displayName, string description, string localizationStem,
            BlueprintBuff marker, Sprite icon)
        {
            var ability = ScriptableObject.CreateInstance<BlueprintActivatableAbility>();
            ability.name = internalName;
            BlueprintUnitFactAccess.Resolve().Configure(ability,
                LocalizationService.Create("KMG.Mode." + localizationStem + ".Name",
                    displayName),
                LocalizationService.Create("KMG.Mode." + localizationStem +
                    ".Description", description), icon);
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
            ability.ActionBarAutoFillIgnored = false;
            ability.ComponentsArray = Array.Empty<BlueprintComponent>();
            ability.ResourceAssetIds = Array.Empty<string>();
            return ability;
        }

        private static void Validate(BlueprintBuff marker,
            BlueprintActivatableAbility ability)
        {
            if (marker == null || ability == null ||
                !ReferenceEquals(ability.Buff, marker) || ability.IsOnByDefault ||
                ability.Group != ActivatableAbilityGroup.None || ability.OnlyInCombat ||
                ability.ActivationType != AbilityActivationType.Immediately ||
                ability.DeactivateIfCombatEnded || ability.DeactivateAfterFirstRound ||
                ability.DeactivateImmediately || ability.DeactivateIfOwnerDisabled ||
                ability.DeactivateIfOwnerUnconscious || ability.ActionBarAutoFillIgnored ||
                ability.ResourceAssetIds == null || ability.ResourceAssetIds.Length != 0 ||
                ability.ComponentsArray == null || ability.ComponentsArray.Length != 0 ||
                marker.ComponentsArray == null || marker.ComponentsArray.Length != 0 ||
                marker.FxOnStart == null || marker.FxOnRemove == null ||
                marker.ResourceAssetIds == null || marker.ResourceAssetIds.Length != 0)
                throw new InvalidOperationException(
                    "Bodyguard automation modes must remain free, off-by-default, persistent, and mechanically inert markers.");
        }
    }
}
