using System;
using KingmakerGunslinger.Ammunition;
using KingmakerGunslinger.Firearms;
using KingmakerGunslinger.Reloading;
using KingmakerGunslinger.Actions;
using KingmakerGunslinger.Misfires;
using KingmakerGunslinger.Explosions;

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

        private static void AdvancedFactoryRifleExact()
        {
            FirearmDefinition value = FirearmDefinitions.CreateAdvancedRifle();
            Assertions.Equal(FirearmEra.Advanced, value.Era, "Rifle era changed.");
            Assertions.Equal(FirearmKind.Rifle, value.Kind, "Rifle kind changed.");
            Assertions.Equal(1, value.Capacity, "Rifle capacity changed.");
            Assertions.Equal(80, value.RangeIncrementFeet, "Rifle range changed.");
            Assertions.Equal(1, value.MisfireValue, "Rifle misfire changed.");
            Assertions.Equal(ReloadActionType.Move, value.Reload.BaseAction, "Rifle reload changed.");
            Assertions.Equal(1, value.Reload.RoundsPerAction, "Rifle reload batch changed.");
        }

        private static void AdvancedFactoryRevolverExact()
        {
            FirearmDefinition value = FirearmDefinitions.CreateAdvancedRevolver();
            Assertions.Equal(FirearmEra.Advanced, value.Era, "Revolver era changed.");
            Assertions.Equal(FirearmKind.Revolver, value.Kind, "Revolver kind changed.");
            Assertions.Equal(6, value.Capacity, "Revolver capacity changed.");
            Assertions.Equal(20, value.RangeIncrementFeet, "Revolver range changed.");
            Assertions.Equal(1, value.MisfireValue, "Revolver misfire changed.");
            Assertions.Equal(ReloadActionType.Move, value.Reload.BaseAction, "Revolver reload changed.");
            Assertions.Equal(6, value.Reload.RoundsPerAction, "Revolver did not load all chambers.");
        }

        private static void AdvancedCatalogRifleExact()
        {
            ProductionFirearmWeaponSpec value = ProductionFirearmCatalog.CreateAdvancedRifle();
            Assertions.Equal("advanced-rifle", value.Key, "Rifle key changed.");
            Assertions.Equal("Advanced Rifle", value.DisplayName, "Rifle name changed.");
            Assertions.Equal(1, value.DamageDiceCount, "Rifle dice count changed.");
            Assertions.Equal(10, value.DamageDieSides, "Rifle damage die changed.");
            Assertions.Equal(4, value.CriticalMultiplier, "Rifle critical changed.");
            Assertions.True(value.IsTwoHanded, "Rifle handedness changed.");
            Assertions.Equal(5000, value.CostGold, "Rifle cost changed.");
            Assertions.Equal(12f, value.WeightPounds, "Rifle weight changed.");
            Assertions.True(value.IsPlayerFireable, "Rifle was unexpectedly unavailable.");
        }

        private static void AdvancedCatalogRevolverExact()
        {
            ProductionFirearmWeaponSpec value = ProductionFirearmCatalog.CreateAdvancedRevolver();
            Assertions.Equal("advanced-revolver", value.Key, "Revolver key changed.");
            Assertions.Equal("Advanced Revolver", value.DisplayName, "Revolver name changed.");
            Assertions.Equal(1, value.DamageDiceCount, "Revolver dice count changed.");
            Assertions.Equal(8, value.DamageDieSides, "Revolver damage die changed.");
            Assertions.Equal(4, value.CriticalMultiplier, "Revolver critical changed.");
            Assertions.False(value.IsTwoHanded, "Revolver handedness changed.");
            Assertions.Equal(4000, value.CostGold, "Revolver cost changed.");
            Assertions.Equal(4f, value.WeightPounds, "Revolver weight changed.");
            Assertions.True(value.IsPlayerFireable, "Revolver was unexpectedly unavailable.");
        }

        private static void AdvancedFactoriesFresh()
        {
            Assertions.False(ReferenceEquals(FirearmDefinitions.CreateAdvancedRifle(),
                FirearmDefinitions.CreateAdvancedRifle()), "Rifle factory reused identity.");
            Assertions.True(FirearmDefinitions.CreateAdvancedRifle().Equals(
                FirearmDefinitions.CreateAdvancedRifle()), "Rifle factory values changed.");
            Assertions.False(ReferenceEquals(ProductionFirearmCatalog.CreateAdvancedRevolver(),
                ProductionFirearmCatalog.CreateAdvancedRevolver()), "Revolver catalog reused identity.");
            Assertions.True(ProductionFirearmCatalog.CreateAdvancedRevolver().Equals(
                ProductionFirearmCatalog.CreateAdvancedRevolver()), "Revolver catalog values changed.");
        }

        private static void CapacityTokensSixRoundComplete()
        {
            FirearmStateTokenCatalog catalog = FirearmStateTokenCatalog.CreateBasicCapacity(6);
            Assertions.Equal(14, catalog.Definitions.Count,
                "Six-round finite catalog does not cover normal/broken counts plus empty broken/Wrecked.");
            for (int rounds = 1; rounds <= 6; rounds++)
            {
                FirearmState normal = new FirearmState(FirearmState.CurrentSchemaVersion,
                    rounds, FirearmStateTokenCatalog.DiagnosticLeadBall, FirearmCondition.Normal);
                FirearmState broken = new FirearmState(FirearmState.CurrentSchemaVersion,
                    rounds, FirearmStateTokenCatalog.DiagnosticLeadBall, FirearmCondition.Broken);
                Assertions.True(catalog.Encode(normal) != null, "Normal round count was not finite-encoded.");
                Assertions.True(catalog.Encode(broken) != null, "Broken round count was not finite-encoded.");
            }
        }

        private static void CapacityTokensRoundTrip()
        {
            FirearmStateTokenCatalog catalog = FirearmStateTokenCatalog.CreateBasicCapacity(6);
            foreach (FirearmStateTokenDefinition definition in catalog.Definitions)
            {
                Assertions.Equal(definition.State, catalog.Decode(new[] { definition.TokenId }),
                    "Capacity token did not decode to its exact state.");
                Assertions.Equal(definition.TokenId, catalog.Encode(definition.State),
                    "Capacity state did not encode to its exact token.");
            }
        }

        private static void CapacityTokensLegacyStable()
        {
            FirearmStateTokenCatalog catalog = FirearmStateTokenCatalog.CreateBasicCapacity(6);
            Assertions.Equal(FirearmStateTokenCatalog.LoadedNormalTokenId,
                catalog.Encode(new FirearmState(FirearmState.CurrentSchemaVersion, 1,
                    FirearmStateTokenCatalog.DiagnosticLeadBall, FirearmCondition.Normal)),
                "Legacy loaded-normal token changed.");
            Assertions.Equal(FirearmStateTokenCatalog.BrokenLoadedTokenId,
                catalog.Encode(new FirearmState(FirearmState.CurrentSchemaVersion, 1,
                    FirearmStateTokenCatalog.DiagnosticLeadBall, FirearmCondition.Broken)),
                "Legacy broken-loaded token changed.");
        }

        private static void CapacityTokensInvalidCapacity()
        {
            Assertions.Throws<ArgumentOutOfRangeException>(
                () => FirearmStateTokenCatalog.CreateBasicCapacity(0),
                "Zero finite-token capacity was accepted.");
            Assertions.Throws<ArgumentOutOfRangeException>(
                () => FirearmStateTokenCatalog.CreateBasicCapacity(65),
                "Oversized finite-token capacity was accepted.");
        }

        private static FirearmMisfireDecision MisfireRoll()
        {
            return new FirearmMisfireService().Evaluate(1, 1, true);
        }

        private static void AdvancedMisfireNormalPreservesRounds()
        {
            FirearmState postDischarge = FirearmStateMachine.Load(FirearmState.CreateEmpty(),
                RevolverRules(), FirearmStateTokenCatalog.DiagnosticLeadBall, 5);
            FirearmMisfireConditionDecision decision = new FirearmMisfireConditionService().Evaluate(
                FirearmDefinitions.CreateAdvancedRevolver(), MisfireRoll(), postDischarge);
            Assertions.Equal(FirearmMisfireConditionTransition.NormalToBroken, decision.Transition,
                "First advanced misfire transition changed.");
            Assertions.Equal(5, decision.After.LoadedRounds, "First advanced misfire lost remaining chambers.");
            Assertions.Equal(FirearmCondition.Broken, decision.After.Condition, "First advanced misfire did not break firearm.");
        }

        private static void AdvancedMisfireBrokenNoExplosion()
        {
            FirearmState normal = FirearmStateMachine.Load(FirearmState.CreateEmpty(),
                RevolverRules(), FirearmStateTokenCatalog.DiagnosticLeadBall, 4);
            FirearmState broken = FirearmStateMachine.ApplyMisfireDamage(normal);
            FirearmMisfireConditionDecision condition = new FirearmMisfireConditionService().Evaluate(
                FirearmDefinitions.CreateAdvancedRevolver(), MisfireRoll(), broken);
            Assertions.Equal(FirearmMisfireConditionTransition.AdvancedBrokenRemainsBroken,
                condition.Transition, "Repeated advanced misfire used early-firearm damage.");
            Assertions.Equal(broken, condition.After, "Repeated advanced misfire changed remaining chambers.");
            Assertions.False(new FirearmExplosionService().Evaluate(condition).RequiresBurstDamage,
                "Advanced firearm exploded on a repeated misfire.");
        }

        private static void CapacityEarlyBrokenMisfireWrecks()
        {
            var rules = new FirearmStateRules(2,
                new[] { FirearmStateTokenCatalog.DiagnosticLeadBall });
            FirearmState normal = FirearmStateMachine.Load(FirearmState.CreateEmpty(), rules,
                FirearmStateTokenCatalog.DiagnosticLeadBall, 1);
            FirearmState broken = FirearmStateMachine.ApplyMisfireDamage(normal);
            FirearmDefinition early = new FirearmDefinition(FirearmEra.Early, FirearmKind.Pistol,
                2, 20, 1, 5, new ReloadProfile(ReloadActionType.Standard, true, 1), false);
            FirearmMisfireConditionDecision condition = new FirearmMisfireConditionService().Evaluate(
                early, MisfireRoll(), broken);
            Assertions.Equal(FirearmMisfireConditionTransition.BrokenToWrecked, condition.Transition,
                "Repeated early misfire did not wreck firearm.");
            Assertions.True(condition.After.IsEmpty, "Early explosion retained loaded chambers.");
            Assertions.True(new FirearmExplosionService().Evaluate(condition).RequiresBurstDamage,
                "Repeated early misfire did not schedule explosion.");
        }

        private static void CapacityVaultSixRoundRestart()
        {
            var store = new FakeFirearmStateVaultStore();
            object item = new object();
            FirearmState full = FirearmStateMachine.Load(FirearmState.CreateEmpty(),
                RevolverRules(), FirearmStateTokenCatalog.DiagnosticLeadBall, 6);
            new VaultBackedFirearmStateRepository(store, RevolverRules()).Set(item, full);
            var reconstructed = new VaultBackedFirearmStateRepository(store, RevolverRules());
            FirearmStateRepositorySnapshot snapshot;
            Assertions.True(reconstructed.TryGet(item, out snapshot),
                "A reconstructed repository did not find six-round persisted state.");
            Assertions.Equal(full, snapshot.State, "Six-round state changed across repository reconstruction.");
        }

        private static void CapacityVaultTwoItemIsolation()
        {
            var store = new FakeFirearmStateVaultStore();
            var repository = new VaultBackedFirearmStateRepository(store, RevolverRules());
            object first = new object();
            object second = new object();
            FirearmState six = FirearmStateMachine.Load(FirearmState.CreateEmpty(), RevolverRules(),
                FirearmStateTokenCatalog.DiagnosticLeadBall, 6);
            FirearmState two = FirearmStateMachine.Load(FirearmState.CreateEmpty(), RevolverRules(),
                FirearmStateTokenCatalog.DiagnosticLeadBall, 2);
            repository.Set(first, six);
            repository.Set(second, two);
            Assertions.Equal(6, repository.GetOrCreate(first).State.LoadedRounds,
                "First revolver lost its exact count.");
            Assertions.Equal(2, repository.GetOrCreate(second).State.LoadedRounds,
                "Second revolver shared the first count.");
            Assertions.Equal(2, repository.PersistedRecordCount,
                "Two revolvers did not retain independent persisted records.");
        }

        private static void CapacityRepeatedDischargeIsolated()
        {
            var store = new FakeFirearmStateVaultStore();
            var repository = new VaultBackedFirearmStateRepository(store, RevolverRules());
            object first = new object();
            object second = new object();
            FirearmState full = FirearmStateMachine.Load(FirearmState.CreateEmpty(), RevolverRules(),
                FirearmStateTokenCatalog.DiagnosticLeadBall, 6);
            repository.Set(first, full);
            repository.Set(second, full);
            repository.Transition(first, FirearmStateMachine.Fire);
            repository.Transition(first, FirearmStateMachine.Fire);
            Assertions.Equal(4, repository.GetOrCreate(first).State.LoadedRounds,
                "Two valid projectiles did not consume two chambers.");
            Assertions.Equal(6, repository.GetOrCreate(second).State.LoadedRounds,
                "Discharging one revolver changed an identical second revolver.");
        }
    }
}
