using System;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs.Blueprints;
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
            BlueprintAbility proneAbility, BlueprintFeature armedProneMarker,
            BlueprintBuff armorClassBuff)
        {
            Feature = feature; ProneAbility = proneAbility;
            ArmedProneMarker = armedProneMarker;
            ArmorClassBuff = armorClassBuff;
        }
        internal BlueprintFeature Feature { get; private set; }
        internal BlueprintAbility ProneAbility { get; private set; }
        internal BlueprintFeature ArmedProneMarker { get; private set; }
        internal BlueprintBuff ArmorClassBuff { get; private set; }
        internal int Count { get { return 4; } }
    }

    internal static class GunslingerDodgeBlueprints
    {
        internal const string FeatureSymbol = "KMG.Deeds.GunslingerDodgeFeature";
        internal const string ProneAbilitySymbol = "KMG.Deeds.GunslingerDodgeProneAbility";
        internal const string ArmedProneSymbol = "KMG.Deeds.GunslingerDodgeProneArmed";
        internal const string ArmorClassBuffSymbol =
            "KMG.Deeds.GunslingerDodgeArmorClassBuff";

        internal static GunslingerDodgeBlueprintSet Register(BlueprintRegistry registry)
        {
            if (registry == null) throw new ArgumentNullException("registry");
            BlueprintFeature marker = registry.Register<BlueprintFeature>(
                ArmedProneSymbol, CreateMarker);
            BlueprintBuff acBuff = registry.Register<BlueprintBuff>(
                ArmorClassBuffSymbol, CreateArmorClassBuff);
            BlueprintAbility ability = registry.Register<BlueprintAbility>(
                ProneAbilitySymbol, () => CreateAbility(marker));
            BlueprintFeature feature = registry.Register<BlueprintFeature>(
                FeatureSymbol, () => CreateFeature(ability));
            Validate(feature, ability, marker, acBuff);
            return new GunslingerDodgeBlueprintSet(feature, ability, marker, acBuff);
        }

        private static BlueprintFeature CreateMarker()
        {
            var result = ScriptableObject.CreateInstance<BlueprintFeature>();
            result.name = "KMG_GunslingerDodge_ProneArmed";
            result.Ranks = 1; result.IsClassFeature = true; result.HideInUI = true;
            result.ComponentsArray = Array.Empty<BlueprintComponent>();
            return result;
        }

        private static BlueprintBuff CreateArmorClassBuff()
        {
            var result = ScriptableObject.CreateInstance<BlueprintBuff>();
            result.name = "KMG_GunslingerDodge_ArmorClass_Buff";
            result.IsClassFeature = true;
            result.Stacking = StackingType.Replace;
            var bonus = ScriptableObject.CreateInstance<AddStatBonus>();
            bonus.name = "$KMG_GunslingerDodge_AC";
            bonus.Stat = StatType.AC;
            bonus.Value = 2;
            bonus.Descriptor = ModifierDescriptor.Dodge;
            result.ComponentsArray = new BlueprintComponent[] { bonus };
            BlueprintUnitFactAccess.Resolve().Configure(result,
                LocalizationService.Create("KMG.Dodge.Buff.Name", "Gunslinger's Dodge"),
                LocalizationService.Create("KMG.Dodge.Buff.Description",
                    "+2 dodge bonus to AC for one round."), null);
            return result;
        }

        private static BlueprintAbility CreateAbility(BlueprintFeature marker)
        {
            var result = ScriptableObject.CreateInstance<BlueprintAbility>();
            result.name = "KMG_GunslingerDodge_ProneAbility";
            BlueprintUnitFactAccess.Resolve().Configure(result,
                LocalizationService.Create("KMG.Dodge.Reaction.Name",
                    "Gunslinger's Dodge"),
                LocalizationService.Create("KMG.Dodge.Reaction.Description",
                    "Arm a reaction to the next ranged weapon attack. When triggered, spend 1 grit and gain a +2 dodge bonus to AC for one round. This adaptation causes no movement and does not make you prone."),
                null);
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
                "KMG.Dodge.Prone.Duration", "Armed until triggered; bonus lasts 1 round");
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
                LocalizationService.Create("KMG.Dodge.Feature.AdaptedDescription",
                    "Spend 1 grit when the armed reaction is triggered by a ranged weapon attack to gain a +2 dodge bonus to AC for one round. You remain standing and may act normally."), null);
            return result;
        }

        private static void Validate(BlueprintFeature feature, BlueprintAbility ability,
            BlueprintFeature marker, BlueprintBuff acBuff)
        {
            var logic = ability.ComponentsArray.OfType<GunslingerDodgeProneAbilityLogic>().Single();
            var grant = feature.ComponentsArray.OfType<AddFacts>().Single();
            if (ability.ActionType != UnitCommand.CommandType.Free ||
                !ReferenceEquals(logic.ArmedMarker, marker) || grant.Facts.Length != 1 ||
                !ReferenceEquals(grant.Facts[0], ability) || !marker.HideInUI ||
                acBuff.Stacking != StackingType.Replace ||
                acBuff.ComponentsArray.OfType<AddStatBonus>().Single().Value != 2)
                throw new InvalidOperationException("Gunslinger's Dodge blueprints are incomplete.");
        }
    }
}
