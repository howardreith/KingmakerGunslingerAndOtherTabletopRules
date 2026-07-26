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
    /// Registers the first player-facing firearm recovery action: a personal,
    /// extraordinary, full-round same-item overhaul that consumes one repair kit only
    /// when delivery completes and changes Wrecked to empty/Broken.
    /// </summary>
    internal static class OverhaulTestMusketAbilityBlueprints
    {
        internal const string Symbol = "KMG.Test.OverhaulAbility";
        internal const string InternalName = "KMG_OverhaulTestMusket_Ability";
        internal const string DisplayName = "Overhaul Test Musket";
        internal const string ComponentName = "$KMG_OverhaulTestMusketLogic";

        private const string Description =
            "Spend a full-round action and consume one Firearm Repair Kit to overhaul the exact equipped Wrecked Test Musket into an empty Broken firearm. This preserves the same item and does not perform ordinary Broken-to-Normal repair.";

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
                    "Overhaul ability blueprint dependencies are incomplete.");
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
                        "KMG.Ability.OverhaulTestMusket.Duration",
                        "Instantaneous");
                    result.LocalizedSavingThrow = LocalizationService.Create(
                        "KMG.Ability.OverhaulTestMusket.SavingThrow",
                        "None");

                    OverhaulTestMusketAbilityLogic logic =
                        OverhaulTestMusketAbilityLogic.Create(
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
                "overhaul-ability.ready",
                string.Format(
                    CultureInfo.InvariantCulture,
                    "Registered full-round Overhaul Test Musket ability guid={0}; item={1}; repairKit={2}.",
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
                    "Overhaul Test Musket has incorrect identity or localization.");
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
                    "Overhaul Test Musket has incorrect action, target, or ability-type settings.");
            }

            OverhaulTestMusketAbilityLogic[] components =
                (ability.ComponentsArray ?? Array.Empty<BlueprintComponent>())
                .OfType<OverhaulTestMusketAbilityLogic>()
                .ToArray();
            if (components.Length != 1 ||
                ability.ComponentsArray.Length != 1 ||
                !string.Equals(components[0].name, ComponentName, StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    "Overhaul Test Musket must contain exactly one stable overhaul-logic component.");
            }

            components[0].ValidateConfiguration();
            if (testMusket == null || repairKit == null)
            {
                throw new InvalidOperationException(
                    "Overhaul Test Musket validation received incomplete dependencies.");
            }
        }
    }
}
