using System;
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
    /// Typed Kingmaker adapter for the player-facing unified Repair Firearm action.
    /// It resolves one exact equipped firearm, requires a Broken or Wrecked state and
    /// one reusable Gunsmith's Kit in the shared inventory, and executes the atomic
    /// same-item repair to Normal only during ability delivery. The tool is never
    /// consumed and surviving loaded ammunition is preserved.
    /// </summary>
    internal static class RepairTestMusketRuntime
    {
        internal static FirearmRepairAvailability Evaluate(
            UnitDescriptor caster,
            BlueprintItemWeapon testMusket,
            BlueprintItem gunsmithKit)
        {
            if (caster == null)
            {
                return Unavailable("No concrete caster descriptor is available.");
            }

            if (testMusket == null || gunsmithKit == null)
            {
                return Unavailable("Repair blueprint dependencies are not initialized.");
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
                gunsmithKit,
                out inventoryAdapter,
                out inventoryReason))
            {
                return new FirearmRepairAvailability(
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
                FirearmActionKind.Repair,
                firearm.Definition,
                state,
                inventory.RepairKits > 0);
            if (!action.IsAvailable)
            {
                return Rejected(action.Reason, weapon, firearm, inventory);
            }

            return new FirearmRepairAvailability(
                true,
                "Ready to repair this exact Broken or Wrecked firearm to Normal with the reusable Gunsmith's Kit. Nothing is consumed and every surviving loaded round is preserved; the item will not be replaced.",
                weapon,
                firearm,
                inventory);
        }

        internal static FirearmRepairRuntimeResult Execute(
            UnitDescriptor caster,
            BlueprintItemWeapon testMusket,
            BlueprintItem gunsmithKit)
        {
            FirearmRepairAvailability availability = Evaluate(
                caster,
                testMusket,
                gunsmithKit);
            if (!availability.IsAvailable)
            {
                throw new InvalidOperationException(availability.Reason);
            }

            Game game = Game.Instance;
            if (game == null || game.Player == null || game.Player.Inventory == null)
            {
                throw new InvalidOperationException(
                    "The active campaign has no shared inventory at repair delivery time.");
            }

            var inventory = new KingmakerRepairKitInventory(
                game.Player.Inventory,
                gunsmithKit);
            var stateStore = new FirearmItemRepairStateStore(
                FirearmRuntimeState.Service,
                availability.Weapon);
            FirearmRepairResult transaction =
                new FirearmRepairTransactionService()
                .TryRepairToNormal(stateStore, inventory);
            FirearmItemStateSnapshot after =
                FirearmRuntimeState.Service.GetOrCreate(availability.Weapon);
            var result = new FirearmRepairRuntimeResult(
                transaction,
                availability.Firearm,
                after);
            if (result.Succeeded)
                FirearmConditionCombatLog.Publish(
                    after.ItemDisplayName,
                    result.Transaction.BeforeState.Condition,
                    result.Transaction.AfterState.Condition,
                    "Repair Firearm");
            return result;
        }

        private static bool TryResolveInventory(
            BlueprintItem gunsmithKit,
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
                gunsmithKit);
            return true;
        }

        private static FirearmRepairAvailability Unavailable(string reason)
        {
            return new FirearmRepairAvailability(
                false,
                reason,
                null,
                null,
                null);
        }

        private static FirearmRepairAvailability Rejected(
            string reason,
            ItemEntityWeapon weapon,
            FirearmItemStateSnapshot firearm,
            RepairKitInventorySnapshot inventory)
        {
            return new FirearmRepairAvailability(
                false,
                reason,
                weapon,
                firearm,
                inventory);
        }
    }
}
