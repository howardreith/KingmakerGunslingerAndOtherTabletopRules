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
    /// Registers player-facing ordinary firearm maintenance: a personal, extraordinary,
    /// full-round same-item repair that consumes one repair kit only when delivery completes
    /// and changes empty/Broken to empty/Normal.
    /// </summary>
    internal static class RepairTestMusketAbilityBlueprints
    {
        internal const string Symbol = "KMG.Test.RepairAbility";
        internal const string InternalName = "KMG_RepairTestMusket_Ability";
        internal const string DisplayName = "Repair Firearm";
        internal const string ComponentName = "$KMG_RepairTestMusketLogic";

        private const string Description =
            "Spend a full-round action and consume one Firearm Repair Kit to repair the exact equipped empty Broken firearm to empty Normal. A Wrecked firearm must be Overhauled first; this action does not load ammunition or replace the item.";

        internal static BlueprintAbility Register(
            BlueprintRegistry registry,
            ModLogger logger,
            BlueprintItemWeapon testMusket,
            BlueprintItem repairKit)
        {
            if (registry == null)
            {
                throw new ArgumentNullException("registry");
            }

            if (logger == null)
            {
                throw new ArgumentNullException("logger");
            }

            if (testMusket == null || repairKit == null)
            {
                throw new ArgumentNullException(
                    "testMusket",
                    "Repair ability blueprint dependencies are incomplete.");
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
                            "KMG.Ability.RepairTestMusket.Name",
                            DisplayName),
                        LocalizationService.Create(
                            "KMG.Ability.RepairTestMusket.Description",
                            Description),
                        repairKit.Icon ?? testMusket.Icon);

                    result.Type = AbilityType.Extraordinary;
                    result.Range = AbilityRange.Personal;
                    result.CanTargetPoint = false;
                    result.CanTargetEnemies = false;
                    result.CanTargetFriends = false;
                    result.CanTargetSelf = true;
                    result.SpellResistance = false;
                    result.ActionBarAutoFillIgnored = false;
                    result.Hidden = false;
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
                        "KMG.Ability.RepairTestMusket.Duration",
                        "Instantaneous");
                    result.LocalizedSavingThrow = LocalizationService.Create(
                        "KMG.Ability.RepairTestMusket.SavingThrow",
                        "None");

                    RepairTestMusketAbilityLogic logic =
                        RepairTestMusketAbilityLogic.Create(
                            testMusket,
                            repairKit);
                    logic.name = ComponentName;
                    result.ComponentsArray = new BlueprintComponent[] { logic };
                    Validate(result, testMusket, repairKit);
                    return result;
                });

            Validate(ability, testMusket, repairKit);
            logger.Info(
                "recovery",
                "repair-ability.ready",
                string.Format(
                    CultureInfo.InvariantCulture,
                    "Registered full-round Repair Firearm ability guid={0}; compatibilityItem={1}; repairKit={2}.",
                    registry.ResolveGuid(Symbol),
                    testMusket.name,
                    repairKit.name));
            return ability;
        }

        internal static void Validate(
            BlueprintAbility ability,
            BlueprintItemWeapon testMusket,
            BlueprintItem repairKit)
        {
            if (ability == null)
            {
                throw new ArgumentNullException("ability");
            }

            if (!string.Equals(ability.name, InternalName, StringComparison.Ordinal) ||
                !string.Equals(ability.Name, DisplayName, StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    "Repair Firearm has incorrect identity or localization.");
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
                ability.Hidden ||
                !ability.NeedEquipWeapons)
            {
                throw new InvalidOperationException(
                    "Repair Firearm has incorrect action, target, or ability-type settings.");
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
                    "Repair Firearm must contain exactly one stable repair-logic component.");
            }

            components[0].ValidateConfiguration();
            if (testMusket == null || repairKit == null)
            {
                throw new InvalidOperationException(
                    "Repair Firearm validation received incomplete dependencies.");
            }
        }
    }
}
