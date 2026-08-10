using System;
using System.Reflection;
using Kingmaker.Blueprints;
using Kingmaker.ResourceLinks;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using UnityEngine;

namespace KingmakerGunslinger.Blueprints
{
    internal sealed class AcadamaeGraduateModeBlueprintSet
    {
        internal AcadamaeGraduateModeBlueprintSet(BlueprintBuff marker,
            BlueprintActivatableAbility ability)
        {
            Marker = marker ?? throw new ArgumentNullException("marker");
            Ability = ability ?? throw new ArgumentNullException("ability");
        }

        internal BlueprintBuff Marker { get; private set; }
        internal BlueprintActivatableAbility Ability { get; private set; }
        internal int Count { get { return 2; } }
    }

    internal static class AcadamaeGraduateModeBlueprints
    {
        internal const string MarkerSymbol = "KMG.Feats.AcadamaeGraduateModeMarker";
        internal const string AbilitySymbol = "KMG.Feats.UseAcadamaeGraduate";
        internal const string DisplayName = "Use Acadamae Graduate";
        internal const string Description =
            "When active, eligible prepared arcane Conjuration (Summoning) spells that normally take a full-round action are cast as a standard action. Each accelerated cast requires a Fortitude save (DC 15 + spell level); failure causes fatigue. Turn this mode off to cast those spells using their normal casting time without the Acadamae save or fatigue risk.";

        internal static AcadamaeGraduateModeBlueprintSet Register(
            BlueprintRegistry registry, Sprite icon)
        {
            if (registry == null) throw new ArgumentNullException("registry");
            if (icon == null) throw new ArgumentNullException("icon");
            BlueprintBuff marker = registry.Register<BlueprintBuff>(MarkerSymbol,
                CreateMarker);
            BlueprintActivatableAbility ability = registry.Register<BlueprintActivatableAbility>(
                AbilitySymbol, () => CreateAbility(marker, icon));
            Validate(marker, ability);
            return new AcadamaeGraduateModeBlueprintSet(marker, ability);
        }

        private static BlueprintBuff CreateMarker()
        {
            var marker = ScriptableObject.CreateInstance<BlueprintBuff>();
            marker.name = "KMG_AcadamaeGraduateMode_MarkerBuff";
            BlueprintUnitFactAccess.Resolve().Configure(marker,
                LocalizationService.Create("KMG.Mode.AcadamaeGraduate.Marker.Name", DisplayName),
                LocalizationService.Create("KMG.Mode.AcadamaeGraduate.Marker.Description", Description),
                null);
            marker.ComponentsArray = Array.Empty<BlueprintComponent>();
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
            ability.name = "KMG_UseAcadamaeGraduate_ActivatableAbility";
            BlueprintUnitFactAccess.Resolve().Configure(ability,
                LocalizationService.Create("KMG.Mode.AcadamaeGraduate.Name", DisplayName),
                LocalizationService.Create("KMG.Mode.AcadamaeGraduate.Description", Description),
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
                ability.DeactivateIfCombatEnded || ability.DeactivateAfterFirstRound ||
                ability.DeactivateImmediately || ability.DeactivateIfOwnerDisabled ||
                ability.DeactivateIfOwnerUnconscious || ability.ActionBarAutoFillIgnored ||
                ability.ResourceAssetIds == null || ability.ResourceAssetIds.Length != 0 ||
                ability.ComponentsArray == null || ability.ComponentsArray.Length != 0 ||
                marker.ComponentsArray == null || marker.ComponentsArray.Length != 0 ||
                marker.FxOnStart == null || marker.FxOnRemove == null ||
                marker.ResourceAssetIds == null || marker.ResourceAssetIds.Length != 0)
                throw new InvalidOperationException(
                    "Use Acadamae Graduate must remain an off-by-default, persistent, unit-local native mode.");
        }
    }
}
