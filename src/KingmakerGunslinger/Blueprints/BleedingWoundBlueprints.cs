using System;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Facts;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.Visual.Animation.Kingmaker.Actions;
using KingmakerGunslinger.Deeds;
using UnityEngine;

namespace KingmakerGunslinger.Blueprints
{
    internal sealed class BleedingWoundBlueprintSet
    {
        internal BleedingWoundBlueprintSet(BlueprintFeature feature,
            BlueprintAbility[] abilities, BlueprintBuff[] markers,
            BlueprintBuff[] bleeds)
        {
            Feature = feature; Abilities = abilities; Markers = markers;
            Bleeds = bleeds;
        }
        internal BlueprintFeature Feature { get; private set; }
        internal BlueprintAbility[] Abilities { get; private set; }
        internal BlueprintBuff[] Markers { get; private set; }
        internal BlueprintBuff[] Bleeds { get; private set; }
        internal int Count { get { return 13; } }

        internal BleedingWoundKind? TryGetKind(BlueprintBuff marker)
        {
            if (marker == null) return null;
            for (int i = 0; i < Markers.Length; i++)
                if (ReferenceEquals(Markers[i], marker))
                    return (BleedingWoundKind)i;
            return null;
        }

        internal BlueprintBuff GetBleed(BleedingWoundKind kind)
        {
            int index = (int)kind;
            if (index < 0 || index >= Bleeds.Length)
                throw new ArgumentOutOfRangeException("kind");
            return Bleeds[index];
        }
    }

    internal static class BleedingWoundBlueprints
    {
        internal const string FeatureSymbol = "KMG.Deeds.BleedingWoundFeature";
        private static readonly string[] AbilitySymbols = {
            "KMG.Deeds.BleedingWoundHitPointsAbility",
            "KMG.Deeds.BleedingWoundStrengthAbility",
            "KMG.Deeds.BleedingWoundDexterityAbility",
            "KMG.Deeds.BleedingWoundConstitutionAbility" };
        private static readonly string[] MarkerSymbols = {
            "KMG.Deeds.BleedingWoundHitPointsArmed",
            "KMG.Deeds.BleedingWoundStrengthArmed",
            "KMG.Deeds.BleedingWoundDexterityArmed",
            "KMG.Deeds.BleedingWoundConstitutionArmed" };
        private static readonly string[] BleedSymbols = {
            "KMG.Deeds.BleedingWoundHitPointsBuff",
            "KMG.Deeds.BleedingWoundStrengthBuff",
            "KMG.Deeds.BleedingWoundDexterityBuff",
            "KMG.Deeds.BleedingWoundConstitutionBuff" };
        private static readonly string[] Labels = {
            "Hit Points", "Strength", "Dexterity", "Constitution" };

        internal static BleedingWoundBlueprintSet Register(
            BlueprintRegistry registry)
        {
            if (registry == null) throw new ArgumentNullException("registry");
            var markers = new BlueprintBuff[4];
            var bleeds = new BlueprintBuff[4];
            for (int i = 0; i < 4; i++)
            {
                int index = i;
                markers[i] = registry.Register<BlueprintBuff>(MarkerSymbols[i],
                    () => CreateMarker((BleedingWoundKind)index));
                bleeds[i] = registry.Register<BlueprintBuff>(BleedSymbols[i],
                    () => CreateBleed((BleedingWoundKind)index));
            }
            var abilities = new BlueprintAbility[4];
            for (int i = 0; i < 4; i++)
            {
                int index = i;
                abilities[i] = registry.Register<BlueprintAbility>(
                    AbilitySymbols[i], () => CreateAbility(
                        (BleedingWoundKind)index, markers[index], markers));
            }
            BlueprintFeature feature = registry.Register<BlueprintFeature>(
                FeatureSymbol, () => CreateFeature(abilities));
            Validate(feature, abilities, markers, bleeds);
            return new BleedingWoundBlueprintSet(feature, abilities, markers,
                bleeds);
        }

        private static BlueprintBuff CreateMarker(BleedingWoundKind kind)
        {
            var result = ScriptableObject.CreateInstance<BlueprintBuff>();
            result.name = "KMG_BleedingWound_" + kind + "_Armed";
            result.IsClassFeature = true;
            result.Stacking = StackingType.Replace;
            result.ComponentsArray = Array.Empty<BlueprintComponent>();
            return result;
        }

        private static BlueprintBuff CreateBleed(BleedingWoundKind kind)
        {
            var result = ScriptableObject.CreateInstance<BlueprintBuff>();
            result.name = "KMG_BleedingWound_" + kind + "_Buff";
            result.IsClassFeature = false;
            result.Stacking = StackingType.Replace;
            var descriptor = ScriptableObject.CreateInstance<SpellDescriptorComponent>();
            descriptor.name = "$KMG_BleedingWound_Bleed";
            descriptor.Descriptor = SpellDescriptor.Bleed;
            var tick = ScriptableObject.CreateInstance<BleedingWoundTick>();
            tick.name = "$KMG_BleedingWound_Tick";
            tick.Kind = kind;
            result.ComponentsArray = new BlueprintComponent[] { descriptor, tick };
            BlueprintUnitFactAccess.Resolve().Configure(result,
                LocalizationService.Create("KMG.BleedingWound." + kind +
                    ".Buff.Name", "Bleeding Wound — " + Labels[(int)kind]),
                LocalizationService.Create("KMG.BleedingWound." + kind +
                    ".Buff.Description", "Takes recurring bleed damage each round until the bleeding is removed."),
                null);
            return result;
        }

        private static BlueprintAbility CreateAbility(BleedingWoundKind kind,
            BlueprintBuff marker, BlueprintBuff[] allMarkers)
        {
            var result = ScriptableObject.CreateInstance<BlueprintAbility>();
            result.name = "KMG_BleedingWound_" + kind + "_Ability";
            int cost = kind == BleedingWoundKind.HitPoints ? 1 : 2;
            BlueprintUnitFactAccess.Resolve().Configure(result,
                LocalizationService.Create("KMG.BleedingWound." + kind +
                    ".Ability.Name", "Bleeding Wound — " + Labels[(int)kind]),
                LocalizationService.Create("KMG.BleedingWound." + kind +
                    ".Ability.Description", "As a free action, arm your next firearm attack. On an eligible hit, spend " + cost + " grit to inflict recurring " + Labels[(int)kind] + " bleed."), null);
            result.Type = AbilityType.Extraordinary;
            result.Range = AbilityRange.Personal;
            result.CanTargetSelf = true;
            result.CanTargetPoint = result.CanTargetEnemies =
                result.CanTargetFriends = false;
            result.SpellResistance = false;
            result.Hidden = false;
            result.ActionBarAutoFillIgnored = false;
            result.NeedEquipWeapons = false;
            result.EffectOnAlly = AbilityEffectOnUnit.Helpful;
            result.EffectOnEnemy = AbilityEffectOnUnit.None;
            result.Animation = UnitAnimationActionCastSpell.CastAnimationStyle.Self;
            result.ActionType = UnitCommand.CommandType.Free;
            result.ResourceAssetIds = Array.Empty<string>();
            var logic = ScriptableObject.CreateInstance<BleedingWoundAbilityLogic>();
            logic.name = "$KMG_BleedingWound_Arm";
            logic.ArmedMarker = marker;
            logic.AllMarkers = allMarkers;
            result.ComponentsArray = new BlueprintComponent[] { logic };
            return result;
        }

        private static BlueprintFeature CreateFeature(BlueprintAbility[] abilities)
        {
            var result = ScriptableObject.CreateInstance<BlueprintFeature>();
            result.name = "KMG_BleedingWound_Feature";
            result.Ranks = 1; result.IsClassFeature = true; result.HideInUI = false;
            var add = ScriptableObject.CreateInstance<AddFacts>();
            add.name = "$KMG_BleedingWound_Grant";
            add.Facts = abilities.Cast<BlueprintUnitFact>().ToArray();
            add.DoNotRestoreMissingFacts = false;
            result.ComponentsArray = new BlueprintComponent[] { add };
            BlueprintUnitFactAccess.Resolve().Configure(result,
                LocalizationService.Create("KMG.BleedingWound.Feature.Name",
                    "Bleeding Wound"),
                LocalizationService.Create("KMG.BleedingWound.Feature.Description",
                    "Arm a firearm shot to spend grit and inflict recurring hit-point or ability-score bleed on an eligible hit."), null);
            return result;
        }

        private static void Validate(BlueprintFeature feature,
            BlueprintAbility[] abilities, BlueprintBuff[] markers,
            BlueprintBuff[] bleeds)
        {
            AddFacts grant = feature.ComponentsArray.OfType<AddFacts>().Single();
            if (abilities.Length != 4 || markers.Length != 4 || bleeds.Length != 4 ||
                grant.Facts.Length != 4 || abilities.Any(a =>
                    a.ActionType != UnitCommand.CommandType.Free) ||
                bleeds.Any(b => b.ComponentsArray
                    .OfType<SpellDescriptorComponent>().Single().Descriptor !=
                        SpellDescriptor.Bleed))
                throw new InvalidOperationException(
                    "Bleeding Wound blueprint contract is incomplete.");
        }
    }
}
