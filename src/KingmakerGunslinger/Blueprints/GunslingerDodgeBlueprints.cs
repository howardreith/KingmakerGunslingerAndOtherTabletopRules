using System;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.ElementsSystem;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.CasterCheckers;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
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
                ProneAbilitySymbol, () => CreateAbility(acBuff, grit));
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
            result.IsClassFeature = false;
            result.Stacking = StackingType.Replace;
            var bonus = ScriptableObject.CreateInstance<
                GunslingerDodgeArmorClassBonus>();
            bonus.name = "$KMG_GunslingerDodge_AC";
            result.ComponentsArray = new BlueprintComponent[] { bonus };
            BlueprintUnitFactAccess.Resolve().Configure(result,
                LocalizationService.Create("KMG.Dodge.Buff.Name", "Gunslinger's Dodge"),
                LocalizationService.Create("KMG.Dodge.Buff.Description",
                    "+2 dodge bonus to AC for one round."), null);
            return result;
        }

        private static BlueprintAbility CreateAbility(
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

            var availability = ScriptableObject.CreateInstance<AbilityCasterHasNoFacts>();
            availability.name = "$KMG_GunslingerDodge_NotAlreadyActive";
            availability.Facts = new BlueprintUnitFact[] { armorClassBuff };

            var applyBuff = ScriptableObject.CreateInstance<ContextActionApplyBuff>();
            applyBuff.name = "$KMG_GunslingerDodge_ApplyBuff";
            applyBuff.Buff = armorClassBuff;
            applyBuff.Permanent = false;
            // Match Call of the Wild's complete ContextDurationValue construction.
            // Leaving DiceCountValue unset produces a partially initialized duration
            // object; Kingmaker evaluates both context values before applying the buff.
            applyBuff.DurationValue = new ContextDurationValue
            {
                Rate = DurationRate.Rounds,
                DiceType = Kingmaker.RuleSystem.DiceType.Zero,
                DiceCountValue = 0,
                BonusValue = 1
            };
            applyBuff.IsFromSpell = false;
            applyBuff.IsNotDispelable = true;
            applyBuff.UseDurationSeconds = false;
            applyBuff.AsChild = false;
            applyBuff.ToCaster = true;

            var effect = ScriptableObject.CreateInstance<AbilityEffectRunAction>();
            effect.name = "$KMG_GunslingerDodge_RunAction";
            effect.Actions = new ActionList
            {
                Actions = new GameAction[] { applyBuff }
            };

            result.ComponentsArray = new BlueprintComponent[] {
                resource, calculator, availability, effect };
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
            AbilityCasterHasNoFacts availability = ability.ComponentsArray
                .OfType<AbilityCasterHasNoFacts>().Single();
            AbilityEffectRunAction effect = ability.ComponentsArray
                .OfType<AbilityEffectRunAction>().Single();
            ContextActionApplyBuff applyBuff = effect.Actions.Actions
                .OfType<ContextActionApplyBuff>().Single();
            var grant = feature.ComponentsArray.OfType<AddFacts>().Single();
            if (ability.ActionType != UnitCommand.CommandType.Swift ||
                ability.Animation !=
                    UnitAnimationActionCastSpell.CastAnimationStyle.Immediate ||
                !ability.HasFastAnimation || ability.Range != AbilityRange.Personal ||
                !ability.CanTargetSelf || ability.ComponentsArray.Length != 4 ||
                grant.Facts.Length != 1 ||
                !resource.IsSpendResource || !resource.CostIsCustom ||
                resource.Amount != 0 ||
                !ReferenceEquals(resource.RequiredResource, grit) ||
                calculator == null ||
                availability.Facts.Length != 1 ||
                !ReferenceEquals(availability.Facts[0], acBuff) ||
                effect.Actions.Actions.Length != 1 ||
                !ReferenceEquals(applyBuff.Buff, acBuff) ||
                applyBuff.Permanent || !applyBuff.IsNotDispelable ||
                applyBuff.IsFromSpell || applyBuff.UseDurationSeconds ||
                applyBuff.AsChild || !applyBuff.ToCaster ||
                applyBuff.DurationValue.Rate != DurationRate.Rounds ||
                applyBuff.DurationValue.DiceType !=
                    Kingmaker.RuleSystem.DiceType.Zero ||
                applyBuff.DurationValue.DiceCountValue.Value != 0 ||
                applyBuff.DurationValue.BonusValue.Value != 1 ||
                !ReferenceEquals(grant.Facts[0], ability) || !marker.HideInUI ||
                acBuff.IsClassFeature || acBuff.Stacking != StackingType.Replace ||
                acBuff.ComponentsArray
                    .OfType<GunslingerDodgeArmorClassBonus>().Count() != 1 ||
                GunslingerDodgeArmorClassBonus.Bonus != 2)
                throw new InvalidOperationException("Gunslinger's Dodge blueprints are incomplete.");
        }
    }
}
