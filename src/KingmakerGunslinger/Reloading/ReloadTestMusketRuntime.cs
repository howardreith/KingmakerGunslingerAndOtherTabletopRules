using System;
using Kingmaker;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Items;
using Kingmaker.UnitLogic;
using KingmakerGunslinger.Ammunition;
using KingmakerGunslinger.Actions;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Firearms;

namespace KingmakerGunslinger.Reloading
{
    /// <summary>
    /// Evaluates one exact firearm and one current ammunition profile. The
    /// same immutable plan drives availability, command presentation, and the
    /// eventual inventory/state transaction.
    /// </summary>
    internal static class ReloadTestMusketRuntime
    {
        internal static ReloadTestMusketAvailability Evaluate(
            UnitDescriptor caster, BlueprintItemWeapon testMusket,
            BlueprintItem blackPowder, BlueprintItem leadBall)
        {
            if (caster == null) return Unavailable(
                ReloadPlayerFacingReasonPolicy.ForExactFirearmFailure(),
                "No concrete caster descriptor is available.");

            BasicAmmunitionBlueprintSet ammunition = BlueprintBootstrap
                .BasicAmmunition;
            PaperCartridgeModeBlueprintSet mode = BlueprintBootstrap
                .PaperCartridgeMode;
            if (testMusket == null || blackPowder == null || leadBall == null ||
                ammunition == null || ammunition.PaperCartridge == null ||
                mode == null) return Unavailable("Cannot reload now.",
                "Reload blueprint dependencies are not initialized.");

            ExactEquippedFirearmContext context;
            string rejection;
            if (!ExactEquippedFirearmResolver.TryResolve(caster, out context,
                    out rejection)) return Unavailable(
                ReloadPlayerFacingReasonPolicy.ForExactFirearmFailure(),
                rejection);

            ItemEntityWeapon weapon = context.Weapon;
            FirearmItemStateSnapshot firearm = context.Firearm;
            KingmakerReloadAmmunitionInventory inventoryAdapter;
            string inventoryReason;
            if (!TryResolveInventory(blackPowder, leadBall,
                    ammunition.PaperCartridge, out inventoryAdapter,
                    out inventoryReason)) return new ReloadTestMusketAvailability(
                false, "Cannot reload now.", weapon, firearm, null, null,
                inventoryReason);

            ReloadAmmunitionInventorySnapshot inventory =
                ReloadAmmunitionInventorySnapshot.Capture(inventoryAdapter);
            FirearmState actualState = firearm.Repository.State;
            FirearmState state = actualState.Condition ==
                context.EffectiveCondition ? actualState : new FirearmState(
                    actualState.SchemaVersion, actualState.LoadedRounds,
                    actualState.LoadedAmmunition,
                    context.EffectiveCondition);
            ReloadAmmunitionProfile profile = PaperCartridgeModeRuntime.IsActive(
                caster, mode.Ability, mode.Marker) ?
                ReloadAmmunitionProfileCatalog.PaperCartridge :
                ReloadAmmunitionProfileCatalog.LooseBasic;
            FirearmReloadPlan plan = FirearmReloadPlanner.Evaluate(caster,
                weapon, firearm.Definition, state, profile, inventory,
                FastMusketRuntime.IsAvailable(caster), RapidReloadRuntime
                    .HasMatchingChoice(caster, firearm.Definition.Kind),
                firearm.Definition.Reload.RoundsPerAction);
            if (!plan.IsAvailable) return Rejected(plan.Reason, weapon,
                firearm, inventory, plan);
            return new ReloadTestMusketAvailability(true, "Ready to reload.",
                weapon, firearm, inventory, plan, plan.Reason);
        }

        internal static FirearmReloadResult Execute(UnitDescriptor caster,
            BlueprintItemWeapon testMusket, BlueprintItem blackPowder,
            BlueprintItem leadBall)
        {
            ReloadTestMusketAvailability availability = Evaluate(caster,
                testMusket, blackPowder, leadBall);
            if (!availability.IsAvailable) throw new InvalidOperationException(
                availability.TechnicalReason);
            return Execute(caster, blackPowder, leadBall, availability);
        }

        internal static FirearmReloadResult Execute(UnitDescriptor caster,
            BlueprintItem blackPowder, BlueprintItem leadBall,
            ReloadTestMusketAvailability availability)
        {
            if (caster == null || availability == null ||
                !availability.IsAvailable || availability.Plan == null ||
                availability.Weapon == null || availability.Firearm == null)
                throw new InvalidOperationException(
                    "Reload execution received no coherent available plan.");
            var inventory = new KingmakerReloadAmmunitionInventory(
                Game.Instance.Player.Inventory, blackPowder, leadBall,
                BlueprintBootstrap.BasicAmmunition.PaperCartridge);
            var stateStore = new FirearmItemReloadStateStore(
                FirearmRuntimeState.Service, availability.Weapon);
            FirearmStateRules rules = FirearmStateRules.CreateForDefinition(
                availability.Firearm.Definition);
            return new FirearmReloadTransactionService().TryReloadRounds(
                stateStore, inventory, rules, availability.Plan.Profile,
                availability.Plan.RoundsLoadable);
        }

        private static bool TryResolveInventory(BlueprintItem blackPowder,
            BlueprintItem leadBall, BlueprintItem paperCartridge,
            out KingmakerReloadAmmunitionInventory inventory,
            out string reason)
        {
            inventory = null;
            reason = null;
            Game game = Game.Instance;
            if (game == null || game.Player == null ||
                game.Player.Inventory == null)
            {
                reason = "The active campaign has no shared inventory.";
                return false;
            }
            inventory = new KingmakerReloadAmmunitionInventory(
                game.Player.Inventory, blackPowder, leadBall, paperCartridge);
            return true;
        }

        private static ReloadTestMusketAvailability Unavailable(
            string playerReason, string technicalReason)
        {
            return new ReloadTestMusketAvailability(false, playerReason,
                null, null, null, null, technicalReason);
        }

        private static ReloadTestMusketAvailability Rejected(
            string technicalReason, ItemEntityWeapon weapon,
            FirearmItemStateSnapshot firearm,
            ReloadAmmunitionInventorySnapshot inventory,
            FirearmReloadPlan plan)
        {
            return new ReloadTestMusketAvailability(false,
                ReloadPlayerFacingReasonPolicy.ForPlan(plan), weapon, firearm,
                inventory, plan, technicalReason);
        }
    }
}
