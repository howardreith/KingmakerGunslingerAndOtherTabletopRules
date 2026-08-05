using System;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.ElementsSystem;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics.Actions;
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

        internal static GunslingerDodgeBlueprintSet Register(BlueprintRegistry registry,
            BlueprintAbilityResource grit)
        {
            if (registry == null) throw new ArgumentNullException("registry");
            if (grit == null) throw new ArgumentNullException("grit");
            BlueprintFeature marker = registry.Register<BlueprintFeature>(
                ArmedProneSymbol, CreateMarker);
            BlueprintBuff acBuff = registry.Register<BlueprintBuff>(
                ArmorClassBuffSymbol, CreateArmorClassBuff);
            BlueprintAbility ability = registry.Register<BlueprintAbility>(
                ProneAbilitySymbol, () => CreateAbility(marker, acBuff, grit));
            BlueprintFeature feature = registry.Register<BlueprintFeature>(
                FeatureSymbol, () => CreateFeature(ability));
            Validate(feature, ability, marker, acBuff, grit);
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
            var bonus = ScriptableObject.CreateInstance<
                GunslingerDodgeArmorClassBonus>();
            bonus.name = "$KMG_GunslingerDodge_AC";
            var removeSelf = ScriptableObject.CreateInstance<
                ContextActionRemoveSelf>();
            removeSelf.name = "$KMG_GunslingerDodge_RemoveSelf";
            var expireNextRound = ScriptableObject.CreateInstance<NewRoundTrigger>();
            expireNextRound.name = "$KMG_GunslingerDodge_ExpireNextRound";
            expireNextRound.NewRoundActions = new ActionList {
                Actions = new GameAction[] { removeSelf }
            };
            // Native Assembly-CSharp contract: NewRoundTrigger implements
            // IUnitNewCombatRoundHandler.HandleNewCombatRound(UnitEntityData),
            // exposes ActionList NewRoundActions, and runs it only from that
            // combat-round event (not OnTurnOn). ContextActionRemoveSelf.RunAction()
            // resolves Buff.Data from the action context and calls Buff.Remove().
            // This event is the shared real-time/turn-based native round boundary.
            // Construction precedent: KingmakerRebalance 1332fb0, Rebalance.cs
            // (MIT), using the same native NewRoundTrigger/remove-self graph.
            result.ComponentsArray = new BlueprintComponent[] {
                bonus, expireNextRound };
            BlueprintUnitFactAccess.Resolve().Configure(result,
                LocalizationService.Create("KMG.Dodge.Buff.Name", "Gunslinger's Dodge"),
                LocalizationService.Create("KMG.Dodge.Buff.Description",
                    "+2 dodge bonus to AC for one round."), null);
            return result;
        }

        private static BlueprintAbility CreateAbility(BlueprintFeature marker,
            BlueprintBuff armorClassBuff, BlueprintAbilityResource grit)
        {
            var result = ScriptableObject.CreateInstance<BlueprintAbility>();
            result.name = "KMG_GunslingerDodge_ProneAbility";
            BlueprintUnitFactAccess.Resolve().Configure(result,
                LocalizationService.Create("KMG.Dodge.Reaction.Name",
                    "Gunslinger's Dodge"),
                LocalizationService.Create("KMG.Dodge.Reaction.Description",
                    "As a swift action, immediately spend 1 grit and gain a +2 dodge bonus to AC for one round. This adaptation causes no movement and does not make you prone."),
                null);
            result.Type = AbilityType.Extraordinary; result.Range = AbilityRange.Personal;
            result.CanTargetSelf = true;
            result.CanTargetPoint = result.CanTargetEnemies = result.CanTargetFriends = false;
            result.SpellResistance = false; result.Hidden = false;
            result.ActionBarAutoFillIgnored = false; result.NeedEquipWeapons = false;
            result.EffectOnAlly = AbilityEffectOnUnit.Helpful;
            result.EffectOnEnemy = AbilityEffectOnUnit.None;
            result.Animation = UnitAnimationActionCastSpell.CastAnimationStyle.Immediate;
            result.HasFastAnimation = true;
            result.ActionType = UnitCommand.CommandType.Swift;
            result.ResourceAssetIds = Array.Empty<string>();
            result.LocalizedDuration = LocalizationService.Create(
                "KMG.Dodge.Prone.Duration", "1 round");
            result.LocalizedSavingThrow = LocalizationService.Create(
                "KMG.Dodge.Prone.SavingThrow", "None");
            var resource = ScriptableObject.CreateInstance<AbilityResourceLogic>();
            resource.name = "$KMG_GunslingerDodge_NativeGrit";
            resource.RequiredResource = grit;
            resource.IsSpendResource = true;
            resource.CostIsCustom = true;
            resource.Amount = 0;
            var calculator = ScriptableObject.CreateInstance<DodgeGritCostCalculator>();
            calculator.name = "$KMG_GunslingerDodge_TrueGritCost";
            result.ComponentsArray = new BlueprintComponent[] {
                resource, calculator,
                GunslingerDodgeProneAbilityLogic.Create(marker, armorClassBuff) };
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
                    "As a swift action, immediately spend 1 grit to gain a +2 dodge bonus to AC for one round. You remain standing and may act normally."), null);
            return result;
        }

        private static void Validate(BlueprintFeature feature, BlueprintAbility ability,
            BlueprintFeature marker, BlueprintBuff acBuff,
            BlueprintAbilityResource grit)
        {
            AbilityResourceLogic resource = ability.ComponentsArray
                .OfType<AbilityResourceLogic>().Single();
            DodgeGritCostCalculator calculator = ability.ComponentsArray
                .OfType<DodgeGritCostCalculator>().Single();
            GunslingerDodgeProneAbilityLogic delivery = ability.ComponentsArray
                .OfType<GunslingerDodgeProneAbilityLogic>().Single();
            NewRoundTrigger roundTrigger = acBuff.ComponentsArray
                .OfType<NewRoundTrigger>().Single();
            var grant = feature.ComponentsArray.OfType<AddFacts>().Single();
            if (ability.ActionType != UnitCommand.CommandType.Swift ||
                ability.Animation !=
                    UnitAnimationActionCastSpell.CastAnimationStyle.Immediate ||
                !ability.HasFastAnimation || ability.Range != AbilityRange.Personal ||
                !ability.CanTargetSelf || ability.ComponentsArray.Length != 3 ||
                grant.Facts.Length != 1 ||
                !resource.IsSpendResource || !resource.CostIsCustom ||
                resource.Amount != 0 ||
                !ReferenceEquals(resource.RequiredResource, grit) ||
                calculator == null || delivery == null ||
                !ReferenceEquals(delivery.ArmedMarker, marker) ||
                !ReferenceEquals(delivery.ArmorClassBuff, acBuff) ||
                delivery.Duration != TimeSpan.FromSeconds(6d) ||
                !ReferenceEquals(grant.Facts[0], ability) || !marker.HideInUI ||
                acBuff.Stacking != StackingType.Replace ||
                acBuff.ComponentsArray
                    .OfType<GunslingerDodgeArmorClassBonus>().Count() != 1 ||
                acBuff.ComponentsArray.Length != 2 ||
                acBuff.ComponentsArray.OfType<NewRoundTrigger>().Count() != 1 ||
                roundTrigger.NewRoundActions == null ||
                roundTrigger.NewRoundActions.Actions == null ||
                roundTrigger.NewRoundActions.Actions.Length != 1 ||
                !(roundTrigger.NewRoundActions.Actions[0] is
                    ContextActionRemoveSelf) ||
                GunslingerDodgeArmorClassBonus.Bonus != 2)
                throw new InvalidOperationException("Gunslinger's Dodge blueprints are incomplete.");
        }
    }
}
