using System;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Facts;
using Kingmaker.ElementsSystem;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.Visual.Animation.Kingmaker.Actions;
using KingmakerGunslinger.Deeds;
using UnityEngine;

namespace KingmakerGunslinger.Blueprints
{
    internal sealed class DeathsShotBlueprintSet
    {
        internal DeathsShotBlueprintSet(BlueprintFeature feature,
            BlueprintAbility ability, BlueprintBuff marker, BlueprintBuff death)
        { Feature = feature; Ability = ability; ArmedMarker = marker;
            DeathEffect = death; }
        internal BlueprintFeature Feature { get; private set; }
        internal BlueprintAbility Ability { get; private set; }
        internal BlueprintBuff ArmedMarker { get; private set; }
        internal BlueprintBuff DeathEffect { get; private set; }
        internal int Count { get { return 4; } }
    }

    internal static class DeathsShotBlueprints
    {
        internal const string FeatureSymbol = "KMG.Deeds.DeathsShotFeature";
        internal const string AbilitySymbol = "KMG.Deeds.DeathsShotAbility";
        internal const string MarkerSymbol = "KMG.Deeds.DeathsShotArmed";
        internal const string DeathSymbol = "KMG.Deeds.DeathsShotDeathEffect";
        internal static DeathsShotBlueprintSet Register(BlueprintRegistry registry,
            BlueprintAbilityResource grit, BlueprintCharacterClass gunslingerClass)
        {
            BlueprintBuff death = registry.Register<BlueprintBuff>(DeathSymbol,
                CreateDeathEffect);
            BlueprintBuff marker = registry.Register<BlueprintBuff>(MarkerSymbol,
                () => CreateMarker(grit, gunslingerClass, death));
            BlueprintAbility ability = registry.Register<BlueprintAbility>(
                AbilitySymbol, () => CreateAbility(marker, grit, gunslingerClass));
            BlueprintFeature feature = registry.Register<BlueprintFeature>(
                FeatureSymbol, () => CreateFeature(ability));
            return new DeathsShotBlueprintSet(feature, ability, marker, death);
        }
        private static BlueprintBuff CreateDeathEffect()
        {
            var result = ScriptableObject.CreateInstance<BlueprintBuff>();
            result.name = "KMG_DeathsShot_DeathEffect";
            result.Stacking = StackingType.Replace;
            var descriptor = ScriptableObject.CreateInstance<SpellDescriptorComponent>();
            descriptor.Descriptor = SpellDescriptor.Death;
            var actions = ScriptableObject.CreateInstance<AddFactContextActions>();
            actions.Activated = new ActionList
                { Actions = new GameAction[] { new ContextActionKillTarget() } };
            result.ComponentsArray = new BlueprintComponent[] { descriptor, actions };
            return result;
        }
        private static BlueprintBuff CreateMarker(BlueprintAbilityResource grit,
            BlueprintCharacterClass gunslingerClass, BlueprintBuff death)
        {
            var result = ScriptableObject.CreateInstance<BlueprintBuff>();
            result.name = "KMG_DeathsShot_Armed";
            result.Stacking = StackingType.Replace;
            var handler = ScriptableObject.CreateInstance<DeathsShotAttackHandler>();
            handler.Grit = grit; handler.GunslingerClass = gunslingerClass;
            handler.DeathEffect = death;
            result.ComponentsArray = new BlueprintComponent[] { handler };
            return result;
        }
        private static BlueprintAbility CreateAbility(BlueprintBuff marker,
            BlueprintAbilityResource grit, BlueprintCharacterClass gunslingerClass)
        {
            var result = ScriptableObject.CreateInstance<BlueprintAbility>();
            result.name = "KMG_DeathsShot_Ability"; result.Type = AbilityType.Extraordinary;
            result.Range = AbilityRange.Personal; result.CanTargetSelf = true;
            result.ActionType = UnitCommand.CommandType.Free;
            result.Animation = UnitAnimationActionCastSpell.CastAnimationStyle.Self;
            var logic = ScriptableObject.CreateInstance<DeathsShotAbilityLogic>();
            logic.ArmedMarker = marker; logic.Grit = grit;
            logic.GunslingerClass = gunslingerClass;
            result.ComponentsArray = new BlueprintComponent[] { logic };
            BlueprintUnitFactAccess.Resolve().Configure(result,
                LocalizationService.Create("KMG.DeathsShot.Ability.Name", "Death's Shot"),
                LocalizationService.Create("KMG.DeathsShot.Ability.Description",
                    "Arm the next firearm attack. A confirmed critical may spend 1 grit; Fortitude negates death."), null);
            return result;
        }
        private static BlueprintFeature CreateFeature(BlueprintAbility ability)
        {
            var result = ScriptableObject.CreateInstance<BlueprintFeature>();
            result.name = "KMG_DeathsShot_Feature"; result.Ranks = 1;
            result.IsClassFeature = true;
            var add = ScriptableObject.CreateInstance<AddFacts>();
            add.Facts = new BlueprintUnitFact[] { ability };
            result.ComponentsArray = new BlueprintComponent[] { add };
            BlueprintUnitFactAccess.Resolve().Configure(result,
                LocalizationService.Create("KMG.DeathsShot.Feature.Name", "Death's Shot"),
                LocalizationService.Create("KMG.DeathsShot.Feature.Description",
                    "On an armed confirmed firearm critical, spend 1 grit to force a Fortitude save against death."), null);
            return result;
        }
    }
}
