using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Items;
using Kingmaker.UnitLogic;
using KingmakerGunslinger.Ammunition;
using KingmakerGunslinger.Actions;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Firearms;

namespace KingmakerGunslinger.Reloading
{
    /// <summary>
    /// Stable compatibility adapter for the production reload ability. It resolves one exact
    /// equipped firearm, reads the proven item-token state, and coordinates the
    /// cross-resource transaction with the player's shared inventory.
    /// </summary>
    internal static class ReloadTestMusketRuntime
    {
        internal static ReloadTestMusketAvailability Evaluate(
            UnitDescriptor caster,
            BlueprintItemWeapon testMusket,
            BlueprintItem blackPowder,
            BlueprintItem leadBall)
        {
            if (caster == null)
            {
                return Unavailable("No concrete caster descriptor is available.");
            }

            BasicAmmunitionBlueprintSet ammunition = BlueprintBootstrap.BasicAmmunition;
            PaperCartridgeModeBlueprintSet mode = BlueprintBootstrap.PaperCartridgeMode;
            if (testMusket == null || blackPowder == null || leadBall == null ||
                ammunition == null || ammunition.PaperCartridge == null || mode == null)
            {
                return Unavailable("Reload blueprint dependencies are not initialized.");
            }

            ExactEquippedFirearmContext context;
            string rejection;
            if (!ExactEquippedFirearmResolver.TryResolve(caster, out context, out rejection))
            {
                return Unavailable(rejection);
            }

            ItemEntityWeapon weapon = context.Weapon;
            FirearmItemStateSnapshot firearm = context.Firearm;

            KingmakerReloadAmmunitionInventory inventoryAdapter;
            string inventoryReason;
            if (!TryResolveInventory(
                blackPowder,
                leadBall,
                ammunition.PaperCartridge,
                out inventoryAdapter,
                out inventoryReason))
            {
                return new ReloadTestMusketAvailability(
                    false,
                    inventoryReason,
                    weapon,
                    firearm,
                    null,
                    null);
            }

            ReloadAmmunitionInventorySnapshot inventory =
                ReloadAmmunitionInventorySnapshot.Capture(inventoryAdapter);
            FirearmState actualState = firearm.Repository.State;
            FirearmState state = actualState.Condition == context.EffectiveCondition
                ? actualState
                : new FirearmState(actualState.SchemaVersion,
                    actualState.LoadedRounds, actualState.LoadedAmmunition,
                    context.EffectiveCondition);
            ReloadAmmunitionProfile profile = PaperCartridgeModeRuntime.IsActive(caster,
                mode.Marker) ? ReloadAmmunitionProfileCatalog.PaperCartridge :
                ReloadAmmunitionProfileCatalog.LooseBasic;
            FirearmReloadPlan plan = FirearmReloadPlanner.Evaluate(caster, weapon,
                firearm.Definition, state, profile, inventory,
                FastMusketRuntime.IsAvailable(caster),
                RapidReloadRuntime.HasMatchingChoice(caster, firearm.Definition.Kind),
                firearm.Definition.Reload.RoundsPerAction);
            if (!plan.IsAvailable)
                return Rejected(plan.Reason, weapon, firearm, inventory, plan);
            return new ReloadTestMusketAvailability(
                true,
                state.Condition == FirearmCondition.Broken
                    ? plan.Reason + " The firearm will remain Broken."
                    : plan.Reason,
                weapon,
                firearm,
                inventory,
                plan);
        }

        internal static FirearmReloadResult Execute(
            UnitDescriptor caster,
            BlueprintItemWeapon testMusket,
            BlueprintItem blackPowder,
            BlueprintItem leadBall)
        {
            ReloadTestMusketAvailability availability = Evaluate(
                caster,
                testMusket,
                blackPowder,
                leadBall);
            if (!availability.IsAvailable)
            {
                throw new InvalidOperationException(availability.Reason);
            }

            var inventory = new KingmakerReloadAmmunitionInventory(
                Game.Instance.Player.Inventory,
                blackPowder,
                leadBall,
                BlueprintBootstrap.BasicAmmunition.PaperCartridge);
            var stateStore = new FirearmItemReloadStateStore(
                FirearmRuntimeState.Service,
                availability.Weapon);
            FirearmStateRules rules = FirearmStateRules.CreateForDefinition(
                availability.Firearm.Definition);

            return new FirearmReloadTransactionService().TryReloadRounds(
                stateStore,
                inventory,
                rules,
                availability.Plan.Profile,
                availability.Plan.RoundsLoadable);
        }

        private static bool TryResolveSingleEquippedTestMusket(
            UnitDescriptor caster,
            BlueprintItemWeapon testMusket,
            out ItemEntityWeapon weapon,
            out string reason)
        {
            weapon = null;
            reason = null;
            if (caster.Body == null)
            {
                reason = "The caster has no equipment body.";
                return false;
            }

            var candidates = new List<ItemEntityWeapon>();
            AddDistinct(candidates, caster.Body.PrimaryHand == null
                ? null
                : caster.Body.PrimaryHand.MaybeWeapon);
            AddDistinct(candidates, caster.Body.SecondaryHand == null
                ? null
                : caster.Body.SecondaryHand.MaybeWeapon);

            ItemEntityWeapon[] matches = candidates
                .Where(candidate => candidate != null &&
                    ReferenceEquals(candidate.Blueprint, testMusket))
                .ToArray();
            if (matches.Length == 0)
            {
                reason = "Equip exactly one firearm before reloading.";
                return false;
            }

            if (matches.Length != 1)
            {
                reason = "More than one distinct firearm is equipped; reload is ambiguous.";
                return false;
            }

            weapon = matches[0];
            return true;
        }

        private static bool TryResolveInventory(
            BlueprintItem blackPowder,
            BlueprintItem leadBall,
            BlueprintItem paperCartridge,
            out KingmakerReloadAmmunitionInventory inventory,
            out string reason)
        {
            inventory = null;
            reason = null;
            Game game = Game.Instance;
            if (game == null || game.Player == null || game.Player.Inventory == null)
            {
                reason = "The active campaign has no shared inventory.";
                return false;
            }

            inventory = new KingmakerReloadAmmunitionInventory(
                game.Player.Inventory,
                blackPowder,
                leadBall,
                paperCartridge);
            return true;
        }

        private static ReloadTestMusketAvailability Unavailable(string reason)
        {
            return new ReloadTestMusketAvailability(
                false,
                reason,
                null,
                null,
                null,
                null);
        }

        private static ReloadTestMusketAvailability Rejected(
            string reason,
            ItemEntityWeapon weapon,
            FirearmItemStateSnapshot firearm,
            ReloadAmmunitionInventorySnapshot inventory,
            FirearmReloadPlan plan)
        {
            return new ReloadTestMusketAvailability(
                false,
                reason,
                weapon,
                firearm,
                inventory,
                plan);
        }

        private static void AddDistinct(
            ICollection<ItemEntityWeapon> candidates,
            ItemEntityWeapon candidate)
        {
            if (candidate == null)
            {
                return;
            }

            foreach (ItemEntityWeapon existing in candidates)
            {
                if (ReferenceEquals(existing, candidate))
                {
                    return;
                }
            }

            candidates.Add(candidate);
        }
    }
}
