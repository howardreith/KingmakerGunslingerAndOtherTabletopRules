using System;
using KingmakerGunslinger.Ammunition;
using KingmakerGunslinger.Firearms;
using KingmakerGunslinger.Reloading;
using KingmakerGunslinger.Actions;

namespace KingmakerGunslinger.DomainTests
{
    internal static partial class Program
    {
        private static FirearmStateRules RevolverRules()
        {
            return new FirearmStateRules(6, new[] { FirearmStateTokenCatalog.DiagnosticLeadBall });
        }

        private static FirearmReloadResult ReloadRevolver(
            FakeFirearmReloadStateStore store,
            FakeBasicAmmunitionInventory inventory)
        {
            return new FirearmReloadTransactionService().TryReloadBasicRounds(
                store, inventory, RevolverRules(),
                FirearmStateTokenCatalog.DiagnosticLeadBall, 6);
        }

        private static void CapacityReloadEmptyToFull()
        {
            var store = new FakeFirearmReloadStateStore(FirearmState.CreateEmpty());
            var inventory = new FakeBasicAmmunitionInventory(8, 7);
            FirearmReloadResult result = ReloadRevolver(store, inventory);
            Assertions.True(result.Succeeded, "Empty revolver did not load.");
            Assertions.Equal(6, result.RoundsLoaded, "Reload batch size changed.");
            Assertions.Equal(6, store.State.LoadedRounds, "Revolver did not reach capacity.");
            Assertions.Equal(2, inventory.Powder, "Powder batch delta changed.");
            Assertions.Equal(1, inventory.Balls, "Projectile batch delta changed.");
            Assertions.Equal(1, store.ReplaceCalls, "Batch reload wrote state more than once.");
        }

        private static void CapacityReloadPartialTopUp()
        {
            FirearmState partial = FirearmStateMachine.Load(FirearmState.CreateEmpty(),
                RevolverRules(), FirearmStateTokenCatalog.DiagnosticLeadBall, 4);
            var store = new FakeFirearmReloadStateStore(partial);
            var inventory = new FakeBasicAmmunitionInventory(3, 3);
            FirearmReloadResult result = ReloadRevolver(store, inventory);
            Assertions.Equal(2, result.RoundsLoaded, "Top-up did not use remaining capacity.");
            Assertions.Equal(6, store.State.LoadedRounds, "Top-up did not reach capacity.");
            Assertions.Equal(1, inventory.Powder, "Top-up powder delta changed.");
            Assertions.Equal(1, inventory.Balls, "Top-up projectile delta changed.");
        }

        private static void CapacityReloadFullRejected()
        {
            FirearmState full = FirearmStateMachine.Load(FirearmState.CreateEmpty(),
                RevolverRules(), FirearmStateTokenCatalog.DiagnosticLeadBall, 6);
            var store = new FakeFirearmReloadStateStore(full);
            var inventory = new FakeBasicAmmunitionInventory(6, 6);
            FirearmReloadResult result = ReloadRevolver(store, inventory);
            Assertions.Equal(FirearmReloadStatus.AlreadyLoaded, result.Status, "Full status changed.");
            Assertions.Equal(0, store.ReplaceCalls, "Full rejection wrote state.");
            Assertions.Equal(0, inventory.RemoveCalls, "Full rejection consumed inventory.");
        }

        private static void CapacityReloadInsufficientAtomic()
        {
            FirearmState partial = FirearmStateMachine.Load(FirearmState.CreateEmpty(),
                RevolverRules(), FirearmStateTokenCatalog.DiagnosticLeadBall, 2);
            var store = new FakeFirearmReloadStateStore(partial);
            var inventory = new FakeBasicAmmunitionInventory(3, 4);
            FirearmReloadResult result = ReloadRevolver(store, inventory);
            Assertions.Equal(FirearmReloadStatus.InsufficientBlackPowder, result.Status,
                "Under-resourced batch status changed.");
            Assertions.Equal(partial, store.State, "Rejected batch changed state.");
            Assertions.Equal(0, inventory.RemoveCalls, "Rejected batch consumed inventory.");
        }

        private static void CapacityReloadMixedAmmunitionRejected()
        {
            AmmunitionId other = new AmmunitionId("kmg.ammunition.other");
            var rules = new FirearmStateRules(6,
                new[] { FirearmStateTokenCatalog.DiagnosticLeadBall, other });
            FirearmState partial = FirearmStateMachine.Load(FirearmState.CreateEmpty(),
                rules, other, 1);
            var store = new FakeFirearmReloadStateStore(partial);
            var inventory = new FakeBasicAmmunitionInventory(5, 5);
            Assertions.Throws<FirearmStateTransitionException>(() =>
                new FirearmReloadTransactionService().TryReloadBasicRounds(
                    store, inventory, rules, FirearmStateTokenCatalog.DiagnosticLeadBall, 6),
                "Mixed ammunition was accepted.");
            Assertions.Equal(0, inventory.RemoveCalls, "Mixed-ammunition rejection consumed inventory.");
            Assertions.Equal(0, store.ReplaceCalls, "Mixed-ammunition rejection wrote state.");
        }

        private static void CapacityReloadWriteFailureRollsBackBatch()
        {
            var store = new FakeFirearmReloadStateStore(FirearmState.CreateEmpty())
            {
                ThrowOnReplaceCall = 1
            };
            var inventory = new FakeBasicAmmunitionInventory(8, 8);
            FirearmReloadTransactionException exception = Assertions.Throws<FirearmReloadTransactionException>(
                () => ReloadRevolver(store, inventory), "Batch write failure was not surfaced.");
            Assertions.True(exception.RollbackSucceeded, "Batch rollback failed.");
            Assertions.Equal(FirearmState.CreateEmpty(), store.State, "Batch rollback changed state.");
            Assertions.Equal(8, inventory.Powder, "Batch rollback did not restore powder.");
            Assertions.Equal(8, inventory.Balls, "Batch rollback did not restore projectiles.");
        }

        private static FirearmDefinition AdvancedRevolverDefinition()
        {
            return new FirearmDefinition(FirearmEra.Advanced, FirearmKind.Revolver,
                6, 20, 1, 5, new ReloadProfile(ReloadActionType.Move, true, 6), false);
        }

        private static void CapacityPolicyPartialAvailable()
        {
            FirearmState partial = FirearmStateMachine.Load(FirearmState.CreateEmpty(),
                RevolverRules(), FirearmStateTokenCatalog.DiagnosticLeadBall, 3);
            FirearmActionDecision decision = FirearmActionPolicy.Evaluate(
                FirearmActionKind.Reload, AdvancedRevolverDefinition(), partial, true);
            Assertions.True(decision.IsAvailable, "Partial advanced firearm was not reloadable.");
        }

        private static void CapacityPolicyFullRejected()
        {
            FirearmState full = FirearmStateMachine.Load(FirearmState.CreateEmpty(),
                RevolverRules(), FirearmStateTokenCatalog.DiagnosticLeadBall, 6);
            FirearmActionDecision decision = FirearmActionPolicy.Evaluate(
                FirearmActionKind.Reload, AdvancedRevolverDefinition(), full, true);
            Assertions.False(decision.IsAvailable, "Full advanced firearm remained reloadable.");
        }
    }
}
