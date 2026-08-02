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
using KingmakerGunslinger.Reloading;
using UnityEngine;

namespace KingmakerGunslinger.Blueprints
{
    /// <summary>
    /// Registers the first player-usable firearm action: a personal, extraordinary,
    /// full-round reload that consumes one powder charge and one Lead Ball only when
    /// its delivery phase completes successfully.
    /// </summary>
    internal static class ReloadTestMusketAbilityBlueprints
    {
        internal const string Symbol = "KMG.Test.ReloadAbility";
        internal const string InternalName = "KMG_ReloadTestMusket_Ability";
        internal const string DisplayName = "Reload Firearm";
        internal const string ComponentName = "$KMG_ReloadTestMusketLogic";

        private const string Description =
            "Load the exact equipped firearm with one Black Powder Charge and one Lead Ball. " +
            "This is a full-round extraordinary action and requires a firearm with a full-round reload profile.";

        internal static BlueprintAbility Register(
            BlueprintRegistry registry,
            ModLogger logger,
            BlueprintItemWeapon testMusket,
            BlueprintItem blackPowder,
            BlueprintItem leadBall)
        {
            if (registry == null)
            {
                throw new ArgumentNullException("registry");
            }

            if (logger == null)
            {
                throw new ArgumentNullException("logger");
            }

            if (testMusket == null || blackPowder == null || leadBall == null)
            {
                throw new ArgumentNullException(
                    "testMusket",
                    "Reload ability blueprint dependencies are incomplete.");
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
                            "KMG.Ability.ReloadTestMusket.Name",
                            DisplayName),
                        LocalizationService.Create(
                            "KMG.Ability.ReloadTestMusket.Description",
                            Description),
                        testMusket.Icon);

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
                        "KMG.Ability.ReloadTestMusket.Duration",
                        "Instantaneous");
                    result.LocalizedSavingThrow = LocalizationService.Create(
                        "KMG.Ability.ReloadTestMusket.SavingThrow",
                        "None");

                    ReloadTestMusketAbilityLogic logic =
                        ReloadTestMusketAbilityLogic.Create(
                            testMusket,
                            blackPowder,
                            leadBall);
                    logic.name = ComponentName;
                    result.ComponentsArray = new BlueprintComponent[] { logic };
                    Validate(result, testMusket, blackPowder, leadBall);
                    return result;
                });

            Validate(ability, testMusket, blackPowder, leadBall);
            logger.Info(
                "reload",
                "ability.ready",
                string.Format(
                    CultureInfo.InvariantCulture,
                    "Registered full-round Reload Firearm ability guid={0}; compatibilityItem={1}; powder={2}; projectile={3}.",
                    registry.ResolveGuid(Symbol),
                    testMusket.name,
                    blackPowder.name,
                    leadBall.name));
            return ability;
        }

        internal static void Validate(
            BlueprintAbility ability,
            BlueprintItemWeapon testMusket,
            BlueprintItem blackPowder,
            BlueprintItem leadBall)
        {
            if (ability == null)
            {
                throw new ArgumentNullException("ability");
            }

            if (!string.Equals(ability.name, InternalName, StringComparison.Ordinal) ||
                !string.Equals(ability.Name, DisplayName, StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    "Reload Firearm has incorrect identity or localization.");
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
                    "Reload Firearm has incorrect action, target, or ability-type settings.");
            }

            ReloadTestMusketAbilityLogic[] components =
                (ability.ComponentsArray ?? Array.Empty<BlueprintComponent>())
                .OfType<ReloadTestMusketAbilityLogic>()
                .ToArray();
            if (components.Length != 1 ||
                ability.ComponentsArray.Length != 1 ||
                !string.Equals(components[0].name, ComponentName, StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    "Reload Firearm must contain exactly one stable reload-logic component.");
            }

            components[0].ValidateConfiguration();
            if (testMusket == null || blackPowder == null || leadBall == null)
            {
                throw new InvalidOperationException(
                    "Reload Firearm validation received incomplete dependencies.");
            }
        }
    }
}
