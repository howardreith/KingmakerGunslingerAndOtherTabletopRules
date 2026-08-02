using System;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.Visual.Animation.Kingmaker.Actions;
using KingmakerGunslinger.Deeds;
using UnityEngine;

namespace KingmakerGunslinger.Blueprints
{
    internal sealed class StartlingShotBlueprintSet
    {
        internal StartlingShotBlueprintSet(BlueprintFeature feature,
            BlueprintAbility ability, BlueprintBuff flatFootedBuff)
        {
            Feature = feature;
            Ability = ability;
            FlatFootedBuff = flatFootedBuff;
        }
        internal BlueprintFeature Feature { get; private set; }
        internal BlueprintAbility Ability { get; private set; }
        internal BlueprintBuff FlatFootedBuff { get; private set; }
        internal int Count { get { return 3; } }
    }

    internal static class StartlingShotBlueprints
    {
        internal const string FeatureSymbol = "KMG.Deeds.StartlingShotFeature";
        internal const string AbilitySymbol = "KMG.Deeds.StartlingShotAbility";
        internal const string FlatFootedBuffSymbol =
            "KMG.Deeds.StartlingShotFlatFootedBuff";

        internal static StartlingShotBlueprintSet Register(BlueprintRegistry registry)
        {
            if (registry == null) throw new ArgumentNullException("registry");
            BlueprintBuff buff = registry.Register<BlueprintBuff>(
                FlatFootedBuffSymbol, CreateFlatFootedBuff);
            BlueprintAbility ability = registry.Register<BlueprintAbility>(
                AbilitySymbol, () => CreateAbility(buff));
            BlueprintFeature feature = registry.Register<BlueprintFeature>(
                FeatureSymbol, () => CreateFeature(ability));
            Validate(feature, ability, buff);
            return new StartlingShotBlueprintSet(feature, ability, buff);
        }

        private static BlueprintBuff CreateFlatFootedBuff()
        {
            var result = ScriptableObject.CreateInstance<BlueprintBuff>();
            result.name = "KMG_StartlingShot_FlatFooted_Buff";
            result.IsClassFeature = false;
            result.Stacking = StackingType.Replace;
            var condition = ScriptableObject.CreateInstance<AddCondition>();
            condition.name = "$KMG_StartlingShot_LoseDexterityToAC";
            condition.Condition = UnitCondition.LoseDexterityToAC;
            result.ComponentsArray = new BlueprintComponent[] { condition };
            BlueprintUnitFactAccess.Resolve().Configure(result,
                LocalizationService.Create("KMG.StartlingShot.Buff.Name",
                    "Startled"),
                LocalizationService.Create("KMG.StartlingShot.Buff.Description",
                    "Flat-footed until the start of this creature's next turn."),
                null);
            return result;
        }

        private static BlueprintAbility CreateAbility(BlueprintBuff buff)
        {
            var result = ScriptableObject.CreateInstance<BlueprintAbility>();
            result.name = "KMG_StartlingShot_Ability";
            BlueprintUnitFactAccess.Resolve().Configure(result,
                LocalizationService.Create("KMG.StartlingShot.Ability.Name",
                    "Startling Shot"),
                LocalizationService.Create("KMG.StartlingShot.Ability.Description",
                    "While you have grit, intentionally miss an enemy with one loaded firearm shot. The shot deals no damage and leaves the target flat-footed until the start of its next turn."),
                null);
            result.Type = AbilityType.Extraordinary;
            result.Range = AbilityRange.Weapon;
            result.CanTargetEnemies = true;
            result.CanTargetSelf = result.CanTargetFriends =
                result.CanTargetPoint = false;
            result.SpellResistance = false;
            result.Hidden = false;
            result.ActionBarAutoFillIgnored = false;
            result.NeedEquipWeapons = true;
            result.EffectOnEnemy = AbilityEffectOnUnit.Harmful;
            result.EffectOnAlly = AbilityEffectOnUnit.None;
            result.Animation = UnitAnimationActionCastSpell.CastAnimationStyle.Special;
            result.ActionType = UnitCommand.CommandType.Standard;
            result.ResourceAssetIds = Array.Empty<string>();
            var logic = ScriptableObject.CreateInstance<StartlingShotAbilityLogic>();
            logic.name = "$KMG_StartlingShot_Delivery";
            logic.FlatFootedBuff = buff;
            result.ComponentsArray = new BlueprintComponent[] { logic };
            return result;
        }

        private static BlueprintFeature CreateFeature(BlueprintAbility ability)
        {
            var result = ScriptableObject.CreateInstance<BlueprintFeature>();
            result.name = "KMG_StartlingShot_Feature";
            result.Ranks = 1;
            result.IsClassFeature = true;
            result.HideInUI = false;
            var add = ScriptableObject.CreateInstance<AddFacts>();
            add.name = "$KMG_StartlingShot_Grant";
            add.Facts = new BlueprintUnitFact[] { ability };
            add.DoNotRestoreMissingFacts = false;
            result.ComponentsArray = new BlueprintComponent[] { add };
            BlueprintUnitFactAccess.Resolve().Configure(result,
                LocalizationService.Create("KMG.StartlingShot.Feature.Name",
                    "Startling Shot"),
                LocalizationService.Create("KMG.StartlingShot.Feature.Description",
                    "Intentionally miss with a firearm to startle an enemy."), null);
            return result;
        }

        private static void Validate(BlueprintFeature feature,
            BlueprintAbility ability, BlueprintBuff buff)
        {
            AddFacts grant = feature.ComponentsArray.OfType<AddFacts>().Single();
            StartlingShotAbilityLogic logic = ability.ComponentsArray
                .OfType<StartlingShotAbilityLogic>().Single();
            AddCondition condition = buff.ComponentsArray.OfType<AddCondition>().Single();
            if (ability.ActionType != UnitCommand.CommandType.Standard ||
                ability.Range != AbilityRange.Weapon ||
                !ability.CanTargetEnemies || ability.CanTargetFriends ||
                buff.IsClassFeature ||
                grant.Facts.Length != 1 || !ReferenceEquals(grant.Facts[0], ability) ||
                !ReferenceEquals(logic.FlatFootedBuff, buff) ||
                condition.Condition != UnitCondition.LoseDexterityToAC)
                throw new InvalidOperationException(
                    "Startling Shot blueprint contract is incomplete.");
        }
    }
}
