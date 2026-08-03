using System;
using System.Globalization;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.Visual.Animation.Kingmaker.Actions;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Reloading;
using UnityEngine;

namespace KingmakerGunslinger.Blueprints
{
    internal static class ReloadTestMusketAbilityBlueprints
    {
        internal const string Symbol = "KMG.Test.ReloadAbility";
        internal const string InternalName = "KMG_ReloadTestMusket_Ability";
        internal const string DisplayName = "Reload Firearm";
        internal const string ComponentName = "$KMG_ReloadTestMusketLogic";
        internal static readonly string[] VariantSymbols = {
            "KMG.Actions.ReloadFree", "KMG.Actions.ReloadMove",
            "KMG.Actions.ReloadStandard", "KMG.Actions.ReloadFullRound" };

        private const string Description =
            "Reload the exact equipped firearm with compatible Black Powder Charges and Lead Balls. " +
            "The required action is determined by the firearm and a matching Rapid Reload feat.";

        internal static BlueprintAbility Register(BlueprintRegistry registry,
            ModLogger logger, BlueprintItemWeapon iconSource,
            BlueprintItem blackPowder, BlueprintItem leadBall)
        {
            if (registry == null || logger == null || iconSource == null ||
                blackPowder == null || leadBall == null)
                throw new ArgumentNullException("Reload ability dependencies are incomplete.");
            EffectiveReloadAction[] actions = {
                EffectiveReloadAction.Free, EffectiveReloadAction.Move,
                EffectiveReloadAction.Standard, EffectiveReloadAction.FullRound };
            var variants = new BlueprintAbility[actions.Length];
            for (int index = 0; index < actions.Length; index++)
            {
                EffectiveReloadAction action = actions[index];
                variants[index] = registry.Register<BlueprintAbility>(
                    VariantSymbols[index], () => CreateVariant(iconSource,
                        blackPowder, leadBall, action));
            }
            BlueprintAbility parent = registry.Register<BlueprintAbility>(Symbol,
                () => CreateParent(iconSource, variants));
            ValidateParent(parent, variants);
            logger.Info("reload", "ability.ready", string.Format(
                CultureInfo.InvariantCulture,
                "Registered Reload Firearm variant parent guid={0}; variants={1}.",
                registry.ResolveGuid(Symbol), variants.Length));
            return parent;
        }

        private static BlueprintAbility CreateParent(BlueprintItemWeapon iconSource,
            BlueprintAbility[] variants)
        {
            var result = ScriptableObject.CreateInstance<BlueprintAbility>();
            result.name = InternalName;
            ConfigureCommon(result, iconSource, false);
            var component = ScriptableObject.CreateInstance<AbilityVariants>();
            component.name = "$KMG_ReloadVariants";
            component.Variants = (BlueprintAbility[])variants.Clone();
            result.ComponentsArray = new BlueprintComponent[] { component };
            return result;
        }

        private static BlueprintAbility CreateVariant(BlueprintItemWeapon iconSource,
            BlueprintItem blackPowder, BlueprintItem leadBall,
            EffectiveReloadAction action)
        {
            var result = ScriptableObject.CreateInstance<BlueprintAbility>();
            result.name = InternalName + "_" + action;
            ConfigureCommon(result, iconSource, true);
            // Variants must remain eligible for native right-click autocast. They are
            // not granted as separate facts and are excluded from action-bar autofill,
            // so the player still receives one coherent Reload Firearm parent action.
            result.Hidden = true;
            result.ActionType = action == EffectiveReloadAction.Move
                ? UnitCommand.CommandType.Move : action == EffectiveReloadAction.Free
                    ? UnitCommand.CommandType.Free : UnitCommand.CommandType.Standard;
            result.SetIsFullRoundAction(action == EffectiveReloadAction.FullRound);
            var logic = ReloadTestMusketAbilityLogic.Create(iconSource,
                blackPowder, leadBall, action);
            logic.name = ComponentName + "_" + action;
            result.ComponentsArray = new BlueprintComponent[] { logic };
            return result;
        }

        private static void ConfigureCommon(BlueprintAbility result,
            BlueprintItemWeapon iconSource, bool variant)
        {
            BlueprintUnitFactAccess.Resolve().Configure(result,
                LocalizationService.Create("KMG.Ability.ReloadTestMusket.Name", DisplayName),
                LocalizationService.Create("KMG.Ability.ReloadTestMusket.Description", Description),
                iconSource.Icon);
            result.Type = AbilityType.Extraordinary;
            result.Range = AbilityRange.Personal;
            result.CanTargetPoint = false;
            result.CanTargetEnemies = false;
            result.CanTargetFriends = false;
            result.CanTargetSelf = true;
            result.SpellResistance = false;
            result.ActionBarAutoFillIgnored = variant;
            result.Hidden = false;
            result.NeedEquipWeapons = true;
            result.EffectOnAlly = AbilityEffectOnUnit.Helpful;
            result.EffectOnEnemy = AbilityEffectOnUnit.None;
            result.Animation = UnitAnimationActionCastSpell.CastAnimationStyle.Self;
            result.HasFastAnimation = false;
            result.ActionType = UnitCommand.CommandType.Free;
            result.SetIsFullRoundAction(false);
            result.DisableLog = false;
            result.ResourceAssetIds = Array.Empty<string>();
            result.LocalizedDuration = LocalizationService.Create(
                "KMG.Ability.ReloadTestMusket.Duration", "Instantaneous");
            result.LocalizedSavingThrow = LocalizationService.Create(
                "KMG.Ability.ReloadTestMusket.SavingThrow", "None");
        }

        internal static void ValidateParent(BlueprintAbility ability,
            BlueprintAbility[] variants)
        {
            if (ability == null || variants == null || variants.Length != 4)
                throw new InvalidOperationException("Reload Firearm variant graph is incomplete.");
            AbilityVariants[] components = (ability.ComponentsArray ??
                Array.Empty<BlueprintComponent>()).OfType<AbilityVariants>().ToArray();
            if (ability.Type != AbilityType.Extraordinary || ability.Hidden ||
                ability.ActionType != UnitCommand.CommandType.Free ||
                ability.IsFullRoundAction || components.Length != 1 ||
                ability.ComponentsArray.Length != 1 ||
                !components[0].Variants.SequenceEqual(variants))
                throw new InvalidOperationException(
                    "Reload Firearm parent has incorrect presentation or variants.");
            foreach (BlueprintAbility variant in variants)
            {
                if (!variant.Hidden || !variant.ActionBarAutoFillIgnored)
                    throw new InvalidOperationException(
                        "Internal Reload Firearm variants must remain hidden implementation details.");
                ReloadTestMusketAbilityLogic logic = variant.ComponentsArray
                    .OfType<ReloadTestMusketAbilityLogic>().Single();
                logic.ValidateConfiguration();
            }
        }
    }
}
