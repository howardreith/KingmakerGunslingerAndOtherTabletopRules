using System;
using System.Reflection;
using Kingmaker.Blueprints;
using Kingmaker.ResourceLinks;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using UnityEngine;

namespace KingmakerGunslinger.Blueprints
{
    internal sealed class PaperCartridgeModeBlueprintSet
    {
        internal PaperCartridgeModeBlueprintSet(BlueprintBuff marker,
            BlueprintActivatableAbility ability)
        {
            Marker = marker ?? throw new ArgumentNullException("marker");
            Ability = ability ?? throw new ArgumentNullException("ability");
        }

        internal BlueprintBuff Marker { get; private set; }
        internal BlueprintActivatableAbility Ability { get; private set; }
        internal int Count { get { return 2; } }
    }

    internal static class PaperCartridgeModeBlueprints
    {
        internal const string MarkerSymbol = "KMG.Ammunition.PaperCartridgeModeMarker";
        internal const string AbilitySymbol = "KMG.Ammunition.UsePaperCartridges";
        internal const string DisplayName = "Use Paper Cartridges";
        internal const string Description =
            "Select Paper Cartridges for future firearm reloads. Each cartridge replaces loose powder and shot, reduces reload time by one step, and increases that loaded shot's misfire value by 1. There is no fallback to loose ammunition when cartridges run out. Toggling this mode does not alter an already loaded chamber.";

        internal static PaperCartridgeModeBlueprintSet Register(BlueprintRegistry registry,
            BasicAmmunitionBlueprintSet ammunition)
        {
            if (registry == null) throw new ArgumentNullException("registry");
            if (ammunition == null || ammunition.PaperCartridge == null)
                throw new ArgumentNullException("ammunition");
            BlueprintBuff marker = registry.Register<BlueprintBuff>(MarkerSymbol,
                CreateMarker);
            BlueprintActivatableAbility ability = registry.Register<BlueprintActivatableAbility>(
                AbilitySymbol, () => CreateAbility(marker, ammunition.PaperCartridge.Icon));
            Validate(marker, ability);
            return new PaperCartridgeModeBlueprintSet(marker, ability);
        }

        private static BlueprintBuff CreateMarker()
        {
            var marker = ScriptableObject.CreateInstance<BlueprintBuff>();
            marker.name = "KMG_PaperCartridgeMode_MarkerBuff";
            BlueprintUnitFactAccess.Resolve().Configure(marker,
                LocalizationService.Create("KMG.Mode.PaperCartridge.Marker.Name", DisplayName),
                LocalizationService.Create("KMG.Mode.PaperCartridge.Marker.Description", Description),
                null);
            marker.ComponentsArray = Array.Empty<BlueprintComponent>();
            // Buff.SpawnParticleEffect and Buff.OnRemove call Load(false) on these
            // links without null guards when a persistent buff's unit view is rebuilt.
            // Empty links are the engine-safe representation of an intentionally
            // effect-free buff; they spawn nothing but remain safe across save/load
            // and area-transition view reconstruction.
            marker.FxOnStart = new PrefabLink();
            marker.FxOnRemove = new PrefabLink();
            marker.ResourceAssetIds = Array.Empty<string>();
            FieldInfo flags = typeof(BlueprintBuff).GetField("m_Flags",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (flags == null || !flags.FieldType.IsEnum)
                throw new MissingFieldException(typeof(BlueprintBuff).FullName, "m_Flags");
            flags.SetValue(marker, Enum.Parse(flags.FieldType, "HiddenInUi"));
            return marker;
        }

        private static BlueprintActivatableAbility CreateAbility(BlueprintBuff marker,
            Sprite icon)
        {
            var ability = ScriptableObject.CreateInstance<BlueprintActivatableAbility>();
            ability.name = "KMG_UsePaperCartridges_ActivatableAbility";
            BlueprintUnitFactAccess.Resolve().Configure(ability,
                LocalizationService.Create("KMG.Mode.PaperCartridge.Name", DisplayName),
                LocalizationService.Create("KMG.Mode.PaperCartridge.Description", Description),
                icon);
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

        internal static void Validate(BlueprintBuff marker,
            BlueprintActivatableAbility ability)
        {
            if (marker == null || ability == null || !ReferenceEquals(ability.Buff, marker) ||
                ability.IsOnByDefault || ability.OnlyInCombat ||
                ability.ActivationType != AbilityActivationType.Immediately ||
                ability.ResourceAssetIds == null || ability.ResourceAssetIds.Length != 0 ||
                ability.ComponentsArray == null || ability.ComponentsArray.Length != 0 ||
                marker.ComponentsArray == null || marker.ComponentsArray.Length != 0 ||
                marker.FxOnStart == null || marker.FxOnRemove == null ||
                marker.ResourceAssetIds == null || marker.ResourceAssetIds.Length != 0)
                throw new InvalidOperationException(
                    "Use Paper Cartridges must remain an off-by-default, free, unit-local native mode.");
        }
    }
}
