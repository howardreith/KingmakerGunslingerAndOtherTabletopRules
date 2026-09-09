using System;
using System.Globalization;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.Visual.Animation.Kingmaker.Actions;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Recovery;
using UnityEngine;

namespace KingmakerGunslinger.Blueprints
{
    /// <summary>
    /// Keeps the historical Overhaul Firearm blueprint identity registered for
    /// save compatibility with characters that already hold the fact, but the
    /// blueprint is now hidden and carries the unified repair logic: invoking it
    /// performs exactly the same reusable-tool Repair Firearm operation. It never
    /// restores a second maintenance mechanic and never consumes a kit.
    /// </summary>
    internal static class OverhaulTestMusketAbilityBlueprints
    {
        internal const string Symbol = "KMG.Test.OverhaulAbility";
        internal const string InternalName = "KMG_OverhaulTestMusket_Ability";
        internal const string DisplayName = "Overhaul Firearm";
        internal const string ComponentName = "$KMG_LegacyOverhaulRepairAlias";

        private const string Description =
            "Legacy maintenance action kept for save compatibility. It now performs the unified Repair Firearm operation: one full-round use with a reusable Gunsmith's Kit repairs the exact equipped Broken or Wrecked firearm to Normal, preserving loaded ammunition and consuming nothing.";

        internal static BlueprintAbility Register(
            BlueprintRegistry registry,
            ModLogger logger,
            BlueprintItemWeapon testMusket,
            BlueprintItem gunsmithKit)
        {
            if (registry == null)
            {
                throw new ArgumentNullException("registry");
            }

            if (logger == null)
            {
                throw new ArgumentNullException("logger");
            }

            if (testMusket == null || gunsmithKit == null)
            {
                throw new ArgumentNullException(
                    "testMusket",
                    "Legacy overhaul alias blueprint dependencies are incomplete.");
            }

            BlueprintAbility ability = registry.Register<BlueprintAbility>(
                Symbol,
                delegate
                {
                    BlueprintAbility result = ScriptableObject.CreateInstance<BlueprintAbility>();
                    result.name = InternalName;

                    BlueprintUnitFactAccess.Resolve().Configure(
                        result,
                        LocalizationService.Create(
                            "KMG.Ability.OverhaulTestMusket.Name",
                            DisplayName),
                        LocalizationService.Create(
                            "KMG.Ability.OverhaulTestMusket.Description",
                            Description),
                        gunsmithKit.Icon ?? testMusket.Icon);

                    result.Type = AbilityType.Extraordinary;
                    result.Range = AbilityRange.Personal;
                    result.CanTargetPoint = false;
                    result.CanTargetEnemies = false;
                    result.CanTargetFriends = false;
                    result.CanTargetSelf = true;
                    result.SpellResistance = false;
                    result.ActionBarAutoFillIgnored = true;
                    result.Hidden = true;
                    result.NeedEquipWeapons = true;
                    result.EffectOnAlly = AbilityEffectOnUnit.Helpful;
                    result.EffectOnEnemy = AbilityEffectOnUnit.None;
                    result.Animation = UnitAnimationActionCastSpell.CastAnimationStyle.Self;
                    result.HasFastAnimation = false;
                    result.ActionType = UnitCommand.CommandType.Standard;
                    result.SetIsFullRoundAction(true);
                    result.DisableLog = false;
                    result.ResourceAssetIds = Array.Empty<string>();
                    result.LocalizedDuration = LocalizationService.Create(
                        "KMG.Ability.OverhaulTestMusket.Duration",
                        "Instantaneous");
                    result.LocalizedSavingThrow = LocalizationService.Create(
                        "KMG.Ability.OverhaulTestMusket.SavingThrow",
                        "None");

                    RepairTestMusketAbilityLogic logic =
                        RepairTestMusketAbilityLogic.Create(
                            testMusket,
                            gunsmithKit);
                    logic.name = ComponentName;
                    result.ComponentsArray = new BlueprintComponent[] { logic };
                    Validate(result, testMusket, gunsmithKit);
                    return result;
                });

            Validate(ability, testMusket, gunsmithKit);
            logger.Info(
                "recovery",
                "overhaul-ability.legacy-alias",
                string.Format(
                    CultureInfo.InvariantCulture,
                    "Registered hidden legacy Overhaul Firearm alias guid={0} delegating to the unified full-round Repair Firearm ability; reusableTool={1}.",
                    registry.ResolveGuid(Symbol),
                    gunsmithKit.name));
            return ability;
        }

        internal static void Validate(
            BlueprintAbility ability,
            BlueprintItemWeapon testMusket,
            BlueprintItem gunsmithKit)
        {
            if (ability == null)
            {
                throw new ArgumentNullException("ability");
            }

            if (!string.Equals(ability.name, InternalName, StringComparison.Ordinal) ||
                !string.Equals(ability.Name, DisplayName, StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    "The legacy Overhaul Firearm alias has incorrect identity or localization.");
            }

            if (ability.Type != AbilityType.Extraordinary ||
                ability.Range != AbilityRange.Personal ||
                ability.ActionType != UnitCommand.CommandType.Standard ||
                !ability.IsFullRoundAction ||
                !ability.CanTargetSelf ||
                ability.CanTargetPoint ||
                ability.CanTargetEnemies ||
                ability.CanTargetFriends ||
                ability.SpellResistance ||
                !ability.Hidden ||
                !ability.ActionBarAutoFillIgnored ||
                !ability.NeedEquipWeapons)
            {
                throw new InvalidOperationException(
                    "The legacy Overhaul Firearm alias must stay hidden, autofill-ignored, and full-round.");
            }

            RepairTestMusketAbilityLogic[] components =
                (ability.ComponentsArray ?? Array.Empty<BlueprintComponent>())
                .OfType<RepairTestMusketAbilityLogic>()
                .ToArray();
            if (components.Length != 1 ||
                ability.ComponentsArray.Length != 1 ||
                !string.Equals(components[0].name, ComponentName, StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    "The legacy Overhaul Firearm alias must contain exactly one unified repair-logic component.");
            }

            components[0].ValidateConfiguration();
            if (testMusket == null || gunsmithKit == null)
            {
                throw new InvalidOperationException(
                    "Legacy overhaul alias validation received incomplete dependencies.");
            }
        }
    }
}
