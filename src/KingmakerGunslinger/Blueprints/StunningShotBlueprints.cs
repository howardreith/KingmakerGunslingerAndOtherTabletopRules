using System;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
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
    internal sealed class StunningShotBlueprintSet
    {
        internal StunningShotBlueprintSet(BlueprintFeature feature,
            BlueprintAbility ability, BlueprintBuff marker, BlueprintBuff stunned)
        { Feature = feature; Ability = ability; ArmedMarker = marker; Stunned = stunned; }
        internal BlueprintFeature Feature { get; private set; }
        internal BlueprintAbility Ability { get; private set; }
        internal BlueprintBuff ArmedMarker { get; private set; }
        internal BlueprintBuff Stunned { get; private set; }
        internal int Count { get { return 4; } }
    }

    internal static class StunningShotBlueprints
    {
        internal const string FeatureSymbol = "KMG.Deeds.StunningShotFeature";
        internal const string AbilitySymbol = "KMG.Deeds.StunningShotAbility";
        internal const string MarkerSymbol = "KMG.Deeds.StunningShotArmed";
        internal const string StunnedSymbol = "KMG.Deeds.StunningShotStunned";
        private const string NativeStunnedGuid =
            "09d39b38bb7c6014394b6daced9bacd3";

        internal static StunningShotBlueprintSet Register(
            LibraryScriptableObject library, BlueprintRegistry registry,
            BlueprintAbilityResource grit, BlueprintCharacterClass gunslingerClass)
        {
            BlueprintBuff native = BlueprintLibraryLookup.RequireExact<BlueprintBuff>(
                library, NativeStunnedGuid, "native Stunned buff");
            BlueprintBuff stunned = registry.Register<BlueprintBuff>(StunnedSymbol,
                () => CreateStunned(native));
            BlueprintBuff marker = registry.Register<BlueprintBuff>(MarkerSymbol,
                () => CreateMarker(grit, gunslingerClass, stunned));
            BlueprintAbility ability = registry.Register<BlueprintAbility>(
                AbilitySymbol, () => CreateAbility(marker, grit, gunslingerClass));
            BlueprintFeature feature = registry.Register<BlueprintFeature>(
                FeatureSymbol, () => CreateFeature(ability));
            Validate(feature, ability, marker, stunned, native);
            return new StunningShotBlueprintSet(feature, ability, marker, stunned);
        }

        private static BlueprintBuff CreateStunned(BlueprintBuff native)
        {
            BlueprintBuff result = BlueprintCloneService.Clone(native,
                "KMG_StunningShot_Stunned");
            result.IsClassFeature = false;
            result.Stacking = StackingType.Replace;
            BlueprintUnitFactAccess.Resolve().Configure(result,
                LocalizationService.Create("KMG.StunningShot.Stunned.Name", "Stunned"),
                LocalizationService.Create("KMG.StunningShot.Stunned.Description",
                    "Stunned for 1 round by Stunning Shot."), null);
            return result;
        }

        private static BlueprintBuff CreateMarker(BlueprintAbilityResource grit,
            BlueprintCharacterClass gunslingerClass, BlueprintBuff stunned)
        {
            var result = ScriptableObject.CreateInstance<BlueprintBuff>();
            result.name = "KMG_StunningShot_Armed";
            result.IsClassFeature = true;
            result.Stacking = StackingType.Replace;
            var handler = ScriptableObject.CreateInstance<StunningShotAttackHandler>();
            handler.name = "$KMG_StunningShot_AttackHandler";
            handler.Grit = grit; handler.GunslingerClass = gunslingerClass;
            handler.StunnedBuff = stunned;
            result.ComponentsArray = new BlueprintComponent[] { handler };
            return result;
        }

        private static BlueprintAbility CreateAbility(BlueprintBuff marker,
            BlueprintAbilityResource grit, BlueprintCharacterClass gunslingerClass)
        {
            var result = ScriptableObject.CreateInstance<BlueprintAbility>();
            result.name = "KMG_StunningShot_Ability";
            result.Type = AbilityType.Extraordinary;
            result.Range = AbilityRange.Personal;
            result.CanTargetSelf = true;
            result.CanTargetPoint = result.CanTargetEnemies =
                result.CanTargetFriends = false;
            result.SpellResistance = false; result.Hidden = false;
            result.ActionBarAutoFillIgnored = false; result.NeedEquipWeapons = false;
            result.EffectOnAlly = AbilityEffectOnUnit.Helpful;
            result.EffectOnEnemy = AbilityEffectOnUnit.None;
            result.Animation = UnitAnimationActionCastSpell.CastAnimationStyle.Self;
            result.ActionType = UnitCommand.CommandType.Free;
            result.ResourceAssetIds = Array.Empty<string>();
            var logic = ScriptableObject.CreateInstance<StunningShotAbilityLogic>();
            logic.name = "$KMG_StunningShot_Arm";
            logic.ArmedMarker = marker; logic.Grit = grit;
            logic.GunslingerClass = gunslingerClass;
            result.ComponentsArray = new BlueprintComponent[] { logic };
            BlueprintUnitFactAccess.Resolve().Configure(result,
                LocalizationService.Create("KMG.StunningShot.Ability.Name",
                    "Stunning Shot"),
                LocalizationService.Create("KMG.StunningShot.Ability.Description",
                    "Arm your next firearm attack. On an eligible hit, spend 2 grit; Fortitude negates being Stunned for 1 round."), null);
            return result;
        }

        private static BlueprintFeature CreateFeature(BlueprintAbility ability)
        {
            var result = ScriptableObject.CreateInstance<BlueprintFeature>();
            result.name = "KMG_StunningShot_Feature";
            result.Ranks = 1; result.IsClassFeature = true; result.HideInUI = false;
            var add = ScriptableObject.CreateInstance<AddFacts>();
            add.name = "$KMG_StunningShot_Grant";
            add.Facts = new BlueprintUnitFact[] { ability };
            add.DoNotRestoreMissingFacts = false;
            result.ComponentsArray = new BlueprintComponent[] { add };
            BlueprintUnitFactAccess.Resolve().Configure(result,
                LocalizationService.Create("KMG.StunningShot.Feature.Name",
                    "Stunning Shot"),
                LocalizationService.Create("KMG.StunningShot.Feature.Description",
                    "Spend 2 grit after a firearm hit to attempt to Stun a target that is not immune to critical hits."), null);
            return result;
        }

        private static void Validate(BlueprintFeature feature,
            BlueprintAbility ability, BlueprintBuff marker, BlueprintBuff stunned,
            BlueprintBuff native)
        {
            AddFacts grant = feature.ComponentsArray.OfType<AddFacts>().Single();
            StunningShotAbilityLogic logic = ability.ComponentsArray
                .OfType<StunningShotAbilityLogic>().Single();
            StunningShotAttackHandler handler = marker.ComponentsArray
                .OfType<StunningShotAttackHandler>().Single();
            if (grant.Facts.Length != 1 || !ReferenceEquals(grant.Facts[0], ability) ||
                !ReferenceEquals(logic.ArmedMarker, marker) ||
                !ReferenceEquals(handler.StunnedBuff, stunned) ||
                ability.ActionType != UnitCommand.CommandType.Free ||
                ReferenceEquals(stunned, native) ||
                DescribeComponentTypes(stunned) != DescribeComponentTypes(native))
                throw new InvalidOperationException(
                    "Stunning Shot exact native contract is incomplete.");
        }

        private static string DescribeComponentTypes(BlueprintBuff buff)
        { return string.Join("|", buff.ComponentsArray.Select(value =>
            value.GetType().FullName).ToArray()); }
    }
}
