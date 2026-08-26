using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Items;
using Kingmaker.UnitLogic;
using KingmakerGunslinger.Firearms;
using KingmakerGunslinger.Actions;

namespace KingmakerGunslinger.Recovery
{
    /// <summary>
    /// Typed Kingmaker adapter for the player-facing overhaul ability. It resolves one
    /// exact equipped firearm, requires Wrecked state and one repair kit, and executes
    /// the atomic same-item Wrecked-to-Broken transaction only during ability delivery.
    /// </summary>
    internal static class OverhaulTestMusketRuntime
    {
        internal static FirearmOverhaulAvailability Evaluate(
            UnitDescriptor caster,
            BlueprintItemWeapon testMusket,
            BlueprintItem repairKit)
        {
            if (caster == null)
            {
                return Unavailable("No concrete caster descriptor is available.");
            }

            if (testMusket == null || repairKit == null)
            {
                return Unavailable(
                    "Overhaul blueprint dependencies are not initialized.");
            }

            if (caster.Unit != null && caster.Unit.IsInCombat)
            {
                return Unavailable(
                    "Overhaul Firearm is unavailable during active combat.");
            }

            ExactEquippedFirearmContext context;
            string rejection;
            if (!ExactEquippedFirearmResolver.TryResolve(caster, out context, out rejection))
            {
                return Unavailable(rejection);
            }

            ItemEntityWeapon weapon = context.Weapon;
            FirearmItemStateSnapshot firearm = context.Firearm;

            KingmakerRepairKitInventory inventoryAdapter;
            string inventoryReason;
            if (!TryResolveInventory(
                repairKit,
                out inventoryAdapter,
                out inventoryReason))
            {
                return new FirearmOverhaulAvailability(
                    false,
                    inventoryReason,
                    weapon,
                    firearm,
                    null);
            }

            RepairKitInventorySnapshot inventory =
                RepairKitInventorySnapshot.Capture(inventoryAdapter);
            FirearmState state = firearm.Repository.State;
            FirearmActionDecision action = FirearmActionPolicy.Evaluate(
                FirearmActionKind.Overhaul,
                firearm.Definition,
                state,
                inventory.RepairKits > 0);
            if (!action.IsAvailable)
            {
                return Rejected(action.Reason, weapon, firearm, inventory);
            }

            if (state.Condition != FirearmCondition.Wrecked)
            {
                return Rejected(
                    "Only an equipped Wrecked firearm can be overhauled. Broken-to-Normal repair remains a separate operation.",
                    weapon,
                    firearm,
                    inventory);
            }

            if (inventory.RepairKits == 0)
            {
                return Rejected(
                    "One Firearm Repair Kit is required.",
                    weapon,
                    firearm,
                    inventory);
            }

            return new FirearmOverhaulAvailability(
                true,
                "Ready to consume one Firearm Repair Kit and overhaul this exact Wrecked firearm to empty/Broken. The item will not be replaced and will not become Normal.",
                weapon,
                firearm,
                inventory);
        }

        internal static FirearmOverhaulRuntimeResult Execute(
            UnitDescriptor caster,
            BlueprintItemWeapon testMusket,
            BlueprintItem repairKit)
        {
            FirearmOverhaulAvailability availability = Evaluate(
                caster,
                testMusket,
                repairKit);
            if (!availability.IsAvailable)
            {
                throw new InvalidOperationException(availability.Reason);
            }

            Game game = Game.Instance;
            if (game == null || game.Player == null || game.Player.Inventory == null)
            {
                throw new InvalidOperationException(
                    "The active campaign has no shared inventory at overhaul delivery time.");
            }

            var inventory = new KingmakerRepairKitInventory(
                game.Player.Inventory,
                repairKit);
            var stateStore = new FirearmItemOverhaulStateStore(
                FirearmRuntimeState.Service,
                availability.Weapon);
            FirearmOverhaulResult transaction =
                new FirearmOverhaulTransactionService()
                .TryOverhaulWreckedToBroken(stateStore, inventory);
            FirearmItemStateSnapshot after =
                FirearmRuntimeState.Service.GetOrCreate(availability.Weapon);
            var result = new FirearmOverhaulRuntimeResult(
                transaction,
                availability.Firearm,
                after);
            if (result.Succeeded)
                FirearmConditionCombatLog.Publish(
                    after.ItemDisplayName,
                    result.Transaction.BeforeState.Condition,
                    result.Transaction.AfterState.Condition,
                    "Overhaul Firearm");
            return result;
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
            AddDistinct(
                candidates,
                caster.Body.PrimaryHand == null
                    ? null
                    : caster.Body.PrimaryHand.MaybeWeapon);
            AddDistinct(
                candidates,
                caster.Body.SecondaryHand == null
                    ? null
                    : caster.Body.SecondaryHand.MaybeWeapon);

            ItemEntityWeapon[] matches = candidates
                .Where(candidate => candidate != null &&
                    ReferenceEquals(candidate.Blueprint, testMusket))
                .ToArray();
            if (matches.Length == 0)
            {
                reason = "Equip exactly one Wrecked firearm before overhauling.";
                return false;
            }

            if (matches.Length != 1)
            {
                reason = "More than one distinct firearm is equipped; overhaul target selection is ambiguous.";
                return false;
            }

            weapon = matches[0];
            return true;
        }

        private static bool TryResolveInventory(
            BlueprintItem repairKit,
            out KingmakerRepairKitInventory inventory,
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

            inventory = new KingmakerRepairKitInventory(
                game.Player.Inventory,
                repairKit);
            return true;
        }

        private static FirearmOverhaulAvailability Unavailable(string reason)
        {
            return new FirearmOverhaulAvailability(
                false,
                reason,
                null,
                null,
                null);
        }

        private static FirearmOverhaulAvailability Rejected(
            string reason,
            ItemEntityWeapon weapon,
            FirearmItemStateSnapshot firearm,
            RepairKitInventorySnapshot inventory)
        {
            return new FirearmOverhaulAvailability(
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
