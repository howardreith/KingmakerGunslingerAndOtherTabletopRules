using System;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Spells;
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
    internal sealed class TargetingHeadBlueprintSet
    {
        internal TargetingHeadBlueprintSet(BlueprintFeature feature,
            BlueprintAbility ability, BlueprintBuff buff)
        { Feature = feature; Ability = ability; ConfusionBuff = buff; }
        internal BlueprintFeature Feature { get; private set; }
        internal BlueprintAbility Ability { get; private set; }
        internal BlueprintBuff ConfusionBuff { get; private set; }
        internal int Count { get { return 3; } }
    }

    internal static class TargetingHeadBlueprints
    {
        internal const string FeatureSymbol = "KMG.Deeds.TargetingHeadFeature";
        internal const string AbilitySymbol = "KMG.Deeds.TargetingHeadAbility";
        internal const string BuffSymbol = "KMG.Deeds.TargetingHeadConfusionBuff";

        internal static TargetingHeadBlueprintSet Register(BlueprintRegistry registry)
        {
            BlueprintBuff buff = registry.Register<BlueprintBuff>(BuffSymbol,
                CreateBuff);
            BlueprintAbility ability = registry.Register<BlueprintAbility>(
                AbilitySymbol, () => CreateAbility(buff));
            BlueprintFeature feature = registry.Register<BlueprintFeature>(
                FeatureSymbol, () => CreateFeature(ability));
            Validate(feature, ability, buff);
            return new TargetingHeadBlueprintSet(feature, ability, buff);
        }

        private static BlueprintBuff CreateBuff()
        {
            var result = ScriptableObject.CreateInstance<BlueprintBuff>();
            result.name = "KMG_TargetingHead_Confusion_Buff";
            result.IsClassFeature = false;
            result.Stacking = StackingType.Replace;
            var condition = ScriptableObject.CreateInstance<AddCondition>();
            condition.name = "$KMG_TargetingHead_Confusion";
            condition.Condition = UnitCondition.Confusion;
            var descriptor = ScriptableObject.CreateInstance<SpellDescriptorComponent>();
            descriptor.name = "$KMG_TargetingHead_MindAffecting";
            descriptor.Descriptor = SpellDescriptor.MindAffecting;
            result.ComponentsArray = new BlueprintComponent[] { condition, descriptor };
            BlueprintUnitFactAccess.Resolve().Configure(result,
                LocalizationService.Create("KMG.TargetingHead.Buff.Name", "Targeted Head"),
                LocalizationService.Create("KMG.TargetingHead.Buff.Description",
                    "Confused for 1 round by a successful Targeting shot."), null);
            return result;
        }

        private static BlueprintAbility CreateAbility(BlueprintBuff buff)
        {
            var result = ScriptableObject.CreateInstance<BlueprintAbility>();
            result.name = "KMG_TargetingHead_Ability";
            BlueprintUnitFactAccess.Resolve().Configure(result,
                LocalizationService.Create("KMG.TargetingHead.Ability.Name",
                    "Targeting — Head"),
                LocalizationService.Create("KMG.TargetingHead.Ability.Description",
                    "Spend 1 grit as a full-round action to make one firearm attack. On a hit, a creature not immune to sneak attacks is confused for 1 round."), null);
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
            result.SetIsFullRoundAction(true);
            result.ResourceAssetIds = Array.Empty<string>();
            var logic = ScriptableObject.CreateInstance<TargetingHeadAbilityLogic>();
            logic.name = "$KMG_TargetingHead_Delivery";
            logic.ConfusionBuff = buff;
            result.ComponentsArray = new BlueprintComponent[] { logic };
            return result;
        }

        private static BlueprintFeature CreateFeature(BlueprintAbility ability)
        {
            var result = ScriptableObject.CreateInstance<BlueprintFeature>();
            result.name = "KMG_TargetingHead_Feature";
            result.Ranks = 1; result.IsClassFeature = true; result.HideInUI = false;
            var add = ScriptableObject.CreateInstance<AddFacts>();
            add.name = "$KMG_TargetingHead_Grant";
            add.Facts = new BlueprintUnitFact[] { ability };
            add.DoNotRestoreMissingFacts = false;
            result.ComponentsArray = new BlueprintComponent[] { add };
            BlueprintUnitFactAccess.Resolve().Configure(result,
                LocalizationService.Create("KMG.TargetingHead.Feature.Name",
                    "Targeting — Head"),
                LocalizationService.Create("KMG.TargetingHead.Feature.Description",
                    "Make a precise firearm attack that can confuse its target."), null);
            return result;
        }

        private static void Validate(BlueprintFeature feature,
            BlueprintAbility ability, BlueprintBuff buff)
        {
            AddFacts grant = feature.ComponentsArray.OfType<AddFacts>().Single();
            TargetingHeadAbilityLogic logic = ability.ComponentsArray
                .OfType<TargetingHeadAbilityLogic>().Single();
            AddCondition condition = buff.ComponentsArray.OfType<AddCondition>().Single();
            SpellDescriptorComponent descriptor = buff.ComponentsArray
                .OfType<SpellDescriptorComponent>().Single();
            if (!ability.IsFullRoundAction || ability.Range != AbilityRange.Weapon ||
                !ability.CanTargetEnemies || grant.Facts.Length != 1 ||
                !ReferenceEquals(grant.Facts[0], ability) ||
                !ReferenceEquals(logic.ConfusionBuff, buff) ||
                condition.Condition != UnitCondition.Confusion ||
                !descriptor.Descriptor.HasAnyFlag(SpellDescriptor.MindAffecting))
                throw new InvalidOperationException(
                    "Targeting Head blueprint contract is incomplete.");
        }
    }
}
