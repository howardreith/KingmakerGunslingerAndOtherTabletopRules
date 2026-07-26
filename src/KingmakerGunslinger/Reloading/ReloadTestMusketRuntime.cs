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
using KingmakerGunslinger.Firearms;

namespace KingmakerGunslinger.Reloading
{
    /// <summary>
    /// Typed Kingmaker adapter for the Test Musket reload ability. It resolves one exact
    /// equipped Test Musket, reads the proven item-token state, and coordinates the
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

            if (testMusket == null || blackPowder == null || leadBall == null)
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

            KingmakerBasicAmmunitionInventory inventoryAdapter;
            string inventoryReason;
            if (!TryResolveInventory(
                blackPowder,
                leadBall,
                out inventoryAdapter,
                out inventoryReason))
            {
                return new ReloadTestMusketAvailability(
                    false,
                    inventoryReason,
                    weapon,
                    firearm,
                    null);
            }

            BasicAmmunitionInventorySnapshot inventory =
                BasicAmmunitionInventorySnapshot.Capture(inventoryAdapter);
            FirearmState state = firearm.Repository.State;
            FirearmActionDecision action = FirearmActionPolicy.Evaluate(
                FirearmActionKind.Reload,
                firearm.Definition,
                state,
                inventory.BlackPowderCharges > 0 && inventory.LeadBalls > 0);
            if (!action.IsAvailable)
            {
                return Rejected(action.Reason, weapon, firearm, inventory);
            }

            if (firearm.Definition.Reload.BaseAction != ReloadActionType.FullRound)
            {
                return Rejected(
                    "The equipped firearm does not use the full-round reload profile implemented by this ability.",
                    weapon,
                    firearm,
                    inventory);
            }

            if (state.Condition == FirearmCondition.Wrecked)
            {
                return Rejected("The equipped Test Musket is wrecked and cannot be reloaded.", weapon, firearm, inventory);
            }

            if (!state.IsEmpty)
            {
                return Rejected("The equipped Test Musket is already loaded.", weapon, firearm, inventory);
            }

            if (inventory.BlackPowderCharges == 0)
            {
                return Rejected("A Black Powder Charge is required.", weapon, firearm, inventory);
            }

            if (inventory.LeadBalls == 0)
            {
                return Rejected("A Lead Ball is required.", weapon, firearm, inventory);
            }

            return new ReloadTestMusketAvailability(
                true,
                state.Condition == FirearmCondition.Broken
                    ? "Ready to load one Lead Ball with one Black Powder Charge; the firearm will remain Broken."
                    : "Ready to load one Lead Ball with one Black Powder Charge.",
                weapon,
                firearm,
                inventory);
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

            var inventory = new KingmakerBasicAmmunitionInventory(
                Game.Instance.Player.Inventory,
                blackPowder,
                leadBall);
            var stateStore = new FirearmItemReloadStateStore(
                FirearmRuntimeState.Service,
                availability.Weapon);
            var rules = new FirearmStateRules(
                availability.Firearm.Definition.Capacity,
                new[] { availability.Firearm.Definition.Reload.Ammunition });

            return new FirearmReloadTransactionService().TryReloadOneBasicRound(
                stateStore,
                inventory,
                rules,
                availability.Firearm.Definition.Reload.Ammunition);
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
                reason = "Equip exactly one Test Musket before reloading.";
                return false;
            }

            if (matches.Length != 1)
            {
                reason = "More than one distinct Test Musket is equipped; reload is ambiguous.";
                return false;
            }

            weapon = matches[0];
            return true;
        }

        private static bool TryResolveInventory(
            BlueprintItem blackPowder,
            BlueprintItem leadBall,
            out KingmakerBasicAmmunitionInventory inventory,
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

            inventory = new KingmakerBasicAmmunitionInventory(
                game.Player.Inventory,
                blackPowder,
                leadBall);
            return true;
        }

        private static ReloadTestMusketAvailability Unavailable(string reason)
        {
            return new ReloadTestMusketAvailability(
                false,
                reason,
                null,
                null,
                null);
        }

        private static ReloadTestMusketAvailability Rejected(
            string reason,
            ItemEntityWeapon weapon,
            FirearmItemStateSnapshot firearm,
            BasicAmmunitionInventorySnapshot inventory)
        {
            return new ReloadTestMusketAvailability(
                false,
                reason,
                weapon,
                firearm,
                inventory);
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
