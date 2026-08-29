using System;
using KingmakerGunslinger.Firearms;
using KingmakerGunslinger.Qualification;
using KingmakerGunslinger.Recovery;

namespace KingmakerGunslinger.DomainTests
{
    internal static partial class Program
    {
        private static void RepairTransactionSuccess()
        {
            FirearmState broken = BrokenState();
            var stateStore = new FakeFirearmRepairStateStore(broken);
            var inventory = new FakeRepairKitInventory(2);
            FirearmRepairResult result = Repair(stateStore, inventory);

            Assertions.Equal(FirearmRepairStatus.Repaired, result.Status, "Repair status mismatch.");
            Assertions.Equal(FirearmCondition.Normal, stateStore.State.Condition, "Repair did not reach Normal.");
            Assertions.True(stateStore.State.IsEmpty, "Repair loaded ammunition unexpectedly.");
            Assertions.Equal(1, inventory.Kits, "Repair did not consume exactly one kit.");
            Assertions.Equal(1, stateStore.ReplaceCalls, "Repair did not write exact state once.");
        }

        private static void RepairTransactionNormalRejected()
        {
            FirearmState normal = LoadedState(2, LeadBall(),
                FirearmCondition.Normal);
            var stateStore = new FakeFirearmRepairStateStore(normal);
            var inventory = new FakeRepairKitInventory(2);
            FirearmRepairResult result = Repair(stateStore, inventory);
            Assertions.Equal(FirearmRepairStatus.NotBroken, result.Status, "Normal rejection status mismatch.");
            Assertions.Equal(0, stateStore.ReplaceCalls, "Normal rejection mutated state.");
            Assertions.Equal(normal, stateStore.State,
                "Normal rejection changed exact loaded state or ammunition identity.");
            Assertions.Equal(2, inventory.Kits, "Normal rejection consumed a kit.");
        }

        private static void RepairTransactionWreckedRejected()
        {
            var stateStore = new FakeFirearmRepairStateStore(WreckedState());
            var inventory = new FakeRepairKitInventory(2);
            FirearmRepairResult result = Repair(stateStore, inventory);
            Assertions.Equal(FirearmRepairStatus.NotBroken, result.Status, "Wrecked rejection status mismatch.");
            Assertions.Equal(0, stateStore.ReplaceCalls, "Wrecked rejection mutated state.");
            Assertions.Equal(2, inventory.Kits, "Wrecked rejection consumed a kit.");
        }

        private static void RepairTransactionLoadedSingleShotSuccess()
        {
            FirearmState loaded = LoadedState(1, LeadBall(), FirearmCondition.Broken);
            var stateStore = new FakeFirearmRepairStateStore(loaded);
            var inventory = new FakeRepairKitInventory(2);
            FirearmRepairResult result = Repair(stateStore, inventory);
            Assertions.Equal(FirearmRepairStatus.Repaired, result.Status,
                "Loaded single-shot repair status mismatch.");
            Assertions.Equal(FirearmCondition.Normal, stateStore.State.Condition,
                "Loaded single-shot repair did not reach Normal.");
            Assertions.Equal(0, stateStore.State.LoadedRounds,
                "Loaded single-shot repair retained a round.");
            Assertions.Equal<AmmunitionId>(null,
                stateStore.State.LoadedAmmunition,
                "Loaded single-shot repair retained ammunition identity.");
            Assertions.Equal(1, inventory.Kits,
                "Loaded single-shot repair did not consume exactly one kit.");
            Assertions.Equal(0, inventory.AddCalls,
                "Successful repair attempted an inventory refund.");
        }

        private static void RepairTransactionLoadedMultiRoundSuccess()
        {
            FirearmState loaded = LoadedState(6, LeadBall(),
                FirearmCondition.Broken);
            var stateStore = new FakeFirearmRepairStateStore(loaded);
            var inventory = new FakeRepairKitInventory(3);
            FirearmRepairResult result = Repair(stateStore, inventory);
            Assertions.Equal(FirearmRepairStatus.Repaired, result.Status,
                "Loaded multi-round repair status mismatch.");
            Assertions.Equal(FirearmCondition.Normal, stateStore.State.Condition,
                "Loaded multi-round repair did not reach Normal.");
            Assertions.Equal(0, stateStore.State.LoadedRounds,
                "Loaded multi-round repair retained rounds.");
            Assertions.Equal<AmmunitionId>(null,
                stateStore.State.LoadedAmmunition,
                "Loaded multi-round repair retained ammunition identity.");
            Assertions.Equal(2, inventory.Kits,
                "Loaded multi-round repair did not consume exactly one kit.");
            Assertions.Equal(0, inventory.AddCalls,
                "Successful multi-round repair attempted an inventory refund.");
        }

        private static void RepairTransactionMissingKit()
        {
            FirearmState loaded = LoadedState(4, LeadBall(),
                FirearmCondition.Broken);
            var stateStore = new FakeFirearmRepairStateStore(loaded);
            var inventory = new FakeRepairKitInventory(0);
            FirearmRepairResult result = Repair(stateStore, inventory);
            Assertions.Equal(FirearmRepairStatus.InsufficientRepairKit, result.Status, "Missing-kit status mismatch.");
            Assertions.Equal(0, stateStore.ReplaceCalls, "Missing-kit rejection mutated state.");
            Assertions.Equal(loaded, stateStore.State,
                "Missing-kit rejection changed loaded rounds or ammunition identity.");
            Assertions.True(FirearmRepairTransactionService.GetRejection(
                    loaded, new RepairKitInventorySnapshot(1)) == null,
                "Loaded ordinary repair still returns a rejection status.");
        }

        private static void RepairTransactionNullStateStore()
        {
            Assertions.Throws<ArgumentNullException>(
                () => new FirearmRepairTransactionService().TryRepairBrokenToNormal(
                    null,
                    new FakeRepairKitInventory(1)),
                "Null repair state store must be rejected.");
        }

        private static void RepairTransactionNullInventory()
        {
            Assertions.Throws<ArgumentNullException>(
                () => new FirearmRepairTransactionService().TryRepairBrokenToNormal(
                    new FakeFirearmRepairStateStore(BrokenState()),
                    null),
                "Null repair inventory must be rejected.");
        }

        private static void RepairTransactionNullState()
        {
            var store = new FakeFirearmRepairStateStore(BrokenState()) { ReturnNullOnRead = true };
            Assertions.Throws<InvalidOperationException>(
                () => Repair(store, new FakeRepairKitInventory(1)),
                "Null repair state must be rejected.");
        }

        private static void RepairTransactionStateWriteFailureRestoresKit()
        {
            FirearmState broken = LoadedState(1, LeadBall(),
                FirearmCondition.Broken);
            var store = new FakeFirearmRepairStateStore(broken) { ThrowOnReplaceCall = 1 };
            var inventory = new FakeRepairKitInventory(2);
            FirearmRepairTransactionException exception = Assertions.Throws<FirearmRepairTransactionException>(
                () => Repair(store, inventory),
                "State-write failure must surface transaction exception.");
            Assertions.True(exception.RollbackSucceeded, "State-write rollback should succeed.");
            Assertions.Equal(broken, store.State, "State-write failure changed state.");
            Assertions.Equal(2, inventory.Kits, "State-write failure did not restore kit.");
        }

        private static void RepairTransactionPostStateMutationFailureRestoresBoth()
        {
            FirearmState broken = LoadedState(5, LeadBall(),
                FirearmCondition.Broken);
            var store = new FakeFirearmRepairStateStore(broken)
            {
                ThrowOnReplaceCall = 1,
                MutateBeforeReplaceFailure = true
            };
            var inventory = new FakeRepairKitInventory(2);
            FirearmRepairTransactionException exception = Assertions.Throws<FirearmRepairTransactionException>(
                () => Repair(store, inventory),
                "Post-mutation state failure must surface transaction exception.");
            Assertions.True(exception.RollbackSucceeded, "Post-mutation rollback should succeed.");
            Assertions.Equal(broken, store.State, "Post-mutation failure did not restore state.");
            Assertions.Equal(2, inventory.Kits, "Post-mutation failure did not restore kit.");
        }

        private static void RepairTransactionVerificationFailureRestoresLoadedState()
        {
            FirearmState loaded = LoadedState(5, LeadBall(),
                FirearmCondition.Broken);
            var store = new FakeFirearmRepairStateStore(loaded)
            {
                OverrideReadCall = 2,
                OverrideReadState = loaded
            };
            var inventory = new FakeRepairKitInventory(2);
            FirearmRepairTransactionException exception =
                Assertions.Throws<FirearmRepairTransactionException>(
                    () => Repair(store, inventory),
                    "Post-write verification failure must surface a transaction exception.");
            Assertions.True(exception.RollbackSucceeded,
                "Post-write verification rollback should succeed.");
            Assertions.Equal(loaded, store.State,
                "Verification failure did not restore exact rounds and ammunition identity.");
            Assertions.Equal(2, inventory.Kits,
                "Verification failure did not restore the repair kit.");
        }

        private static void RepairTransactionStateRollbackFailureSurfaced()
        {
            var store = new FakeFirearmRepairStateStore(BrokenState())
            {
                ThrowOnReplaceCall = 1,
                MutateBeforeReplaceFailure = true,
                ThrowOnSecondReplace = true
            };
            FirearmRepairTransactionException exception = Assertions.Throws<FirearmRepairTransactionException>(
                () => Repair(store, new FakeRepairKitInventory(2)),
                "State rollback failure must be surfaced.");
            Assertions.False(exception.RollbackSucceeded, "Rollback failure was hidden.");
            Assertions.True(exception.StateRollbackException != null, "State rollback exception was not retained.");
        }

        private static void RepairTransactionInventoryRollbackFailureSurfaced()
        {
            var store = new FakeFirearmRepairStateStore(BrokenState()) { ThrowOnReplaceCall = 1 };
            var inventory = new FakeRepairKitInventory(2) { ThrowOnAdd = true };
            FirearmRepairTransactionException exception = Assertions.Throws<FirearmRepairTransactionException>(
                () => Repair(store, inventory),
                "Inventory rollback failure must be surfaced.");
            Assertions.False(exception.RollbackSucceeded, "Inventory rollback failure was hidden.");
            Assertions.True(exception.InventoryRollbackException != null, "Inventory rollback exception was not retained.");
        }

        private static void RepairTransactionPostRemoveFailureRestoresKit()
        {
            FirearmState loaded = LoadedState(3, LeadBall(),
                FirearmCondition.Broken);
            var inventory = new FakeRepairKitInventory(2)
            {
                ThrowOnRemoveCall = 1,
                MutateBeforeRemoveFailure = true
            };
            var store = new FakeFirearmRepairStateStore(loaded);
            FirearmRepairTransactionException exception = Assertions.Throws<FirearmRepairTransactionException>(
                () => Repair(store, inventory),
                "Post-remove failure must surface transaction exception.");
            Assertions.True(exception.RollbackSucceeded, "Post-remove rollback should succeed.");
            Assertions.Equal(loaded, store.State,
                "Inventory failure changed the exact loaded firearm state.");
            Assertions.Equal(0, store.ReplaceCalls,
                "Inventory failure reached the firearm-state write.");
            Assertions.Equal(2, inventory.Kits, "Post-remove failure did not restore kit.");
        }

        private static void RepairResultSuccess()
        {
            FirearmState broken = LoadedState(4, LeadBall(),
                FirearmCondition.Broken);
            FirearmState normal = FirearmStateMachine.Repair(broken);
            var result = new FirearmRepairResult(
                FirearmRepairStatus.Repaired,
                broken,
                normal,
                new RepairKitInventorySnapshot(2),
                new RepairKitInventorySnapshot(1));
            Assertions.True(result.Succeeded, "Successful repair result did not report success.");
            Assertions.True(result.ToString().Contains("status=Repaired"), "Repair result format lost status.");
            Assertions.Throws<ArgumentException>(
                () => new FirearmRepairResult(
                    FirearmRepairStatus.Repaired,
                    broken,
                    normal,
                    new RepairKitInventorySnapshot(2),
                    new RepairKitInventorySnapshot(2)),
                "Successful repair without exact kit consumption must be rejected.");
        }

        private static void RepairResultRejected()
        {
            FirearmState normal = FirearmState.CreateEmpty();
            var inventory = new RepairKitInventorySnapshot(3);
            var result = new FirearmRepairResult(
                FirearmRepairStatus.NotBroken,
                normal,
                normal,
                inventory,
                inventory);
            Assertions.False(result.Succeeded, "Rejected repair result reported success.");
            Assertions.Throws<ArgumentException>(
                () => new FirearmRepairResult(
                    FirearmRepairStatus.NotBroken,
                    normal,
                    BrokenState(),
                    inventory,
                    inventory),
                "Rejected repair state mutation must be rejected.");
        }

        private static void RepairResultUnknownStatus()
        {
            FirearmState state = FirearmState.CreateEmpty();
            var inventory = new RepairKitInventorySnapshot(1);
            Assertions.Throws<ArgumentOutOfRangeException>(
                () => new FirearmRepairResult(
                    (FirearmRepairStatus)99,
                    state,
                    state,
                    inventory,
                    inventory),
                "Unknown repair status must be rejected.");
        }

        private static void RepairPlayerFacingTextPermitsLoaded()
        {
            string ability = ThirdPlaytestSource(
                "src/KingmakerGunslinger/Blueprints/RepairTestMusketAbilityBlueprints.cs");
            string logic = ThirdPlaytestSource(
                "src/KingmakerGunslinger/Recovery/RepairTestMusketAbilityLogic.cs");
            string runtime = ThirdPlaytestSource(
                "src/KingmakerGunslinger/Recovery/RepairTestMusketRuntime.cs");
            string action = ThirdPlaytestSource(
                "src/KingmakerGunslinger/Actions/FirearmActionPolicy.cs");
            string playerText = (ability + logic + runtime + action).ToLowerInvariant();
            foreach (string obsolete in new[] {
                "must be unloaded", "unload the firearm",
                "unload before repair", "repair requires an empty" })
                Assertions.False(playerText.Contains(obsolete),
                    "Player-facing repair text retains obsolete instruction: " +
                    obsolete);
            Assertions.True(ability.Contains(
                    "All ammunition loaded in that firearm is destroyed") &&
                runtime.Contains("Every loaded round will be destroyed") &&
                action.Contains("any loaded ammunition will be destroyed"),
                "Loaded-repair consequence is not stated consistently.");
        }

        private static void RepairRuntimeResultSuccess()
        {
            FirearmState broken = BrokenState();
            FirearmState normal = FirearmStateMachine.Repair(broken);
            var transaction = new FirearmRepairResult(
                FirearmRepairStatus.Repaired,
                broken,
                normal,
                new RepairKitInventorySnapshot(1),
                new RepairKitInventorySnapshot(0));
            var result = new FirearmRepairRuntimeResult(
                transaction,
                OverhaulRuntimeSnapshot(22, 4, 0x5432, broken),
                OverhaulRuntimeSnapshot(22, 5, 0x5432, normal));
            Assertions.True(result.Succeeded, "Repair runtime result did not report success.");
            Assertions.True(result.ToString().Contains("revision=4->5"), "Repair runtime result lost revision proof.");
        }

        private static void RepairRuntimeResultIdentityMismatch()
        {
            FirearmState broken = BrokenState();
            FirearmState normal = FirearmStateMachine.Repair(broken);
            var transaction = new FirearmRepairResult(
                FirearmRepairStatus.Repaired,
                broken,
                normal,
                new RepairKitInventorySnapshot(1),
                new RepairKitInventorySnapshot(0));
            Assertions.Throws<ArgumentException>(
                () => new FirearmRepairRuntimeResult(
                    transaction,
                    OverhaulRuntimeSnapshot(22, 4, 0x5432, broken),
                    OverhaulRuntimeSnapshot(23, 5, 0x5432, normal)),
                "Changed repair repository identity must be rejected.");
        }

        private static void RepairRuntimeResultRevisionMismatch()
        {
            FirearmState broken = BrokenState();
            FirearmState normal = FirearmStateMachine.Repair(broken);
            var transaction = new FirearmRepairResult(
                FirearmRepairStatus.Repaired,
                broken,
                normal,
                new RepairKitInventorySnapshot(1),
                new RepairKitInventorySnapshot(0));
            Assertions.Throws<ArgumentException>(
                () => new FirearmRepairRuntimeResult(
                    transaction,
                    OverhaulRuntimeSnapshot(22, 4, 0x5432, broken),
                    OverhaulRuntimeSnapshot(22, 6, 0x5432, normal)),
                "Repair revision jumps other than one must be rejected.");
        }

        private static void MaintenanceFixturePass()
        {
            MaintenanceQualificationBaseline baseline = MaintenanceBaseline();
            MaintenanceQualificationReport report = EvaluateMaintenance(
                baseline,
                WreckedState(),
                5,
                0,
                2,
                1,
                1,
                10,
                20,
                30,
                0,
                0);
            Assertions.True(report.Passed, "Fixture-ready matrix failed.");
            Assertions.Equal(MaintenanceQualificationStage.FixtureReady, report.Stage, "Fixture stage mismatch.");
        }

        private static void MaintenanceOverhaulPass()
        {
            MaintenanceQualificationBaseline baseline = MaintenanceBaseline();
            MaintenanceQualificationReport report = EvaluateMaintenance(
                baseline,
                BrokenState(),
                6,
                0,
                1,
                1,
                1,
                11,
                20,
                30,
                0,
                0);
            Assertions.True(report.Passed, "Overhaul matrix failed.");
            Assertions.Equal(MaintenanceQualificationStage.OverhaulPassed, report.Stage, "Overhaul stage mismatch.");
        }

        private static void MaintenanceRepairPass()
        {
            MaintenanceQualificationBaseline baseline = MaintenanceBaseline();
            MaintenanceQualificationReport report = EvaluateMaintenance(
                baseline,
                FirearmState.CreateEmpty(),
                7,
                0,
                0,
                1,
                1,
                11,
                21,
                30,
                0,
                0);
            Assertions.True(report.Passed, "Repair matrix failed.");
            Assertions.Equal(MaintenanceQualificationStage.RepairPassed, report.Stage, "Repair stage mismatch.");
        }

        private static void MaintenanceLoopPass()
        {
            MaintenanceQualificationBaseline baseline = MaintenanceBaseline();
            FirearmState loaded = LoadedState(1, LeadBall(), FirearmCondition.Normal);
            MaintenanceQualificationReport report = EvaluateMaintenance(
                baseline,
                loaded,
                8,
                0,
                0,
                0,
                0,
                11,
                21,
                31,
                0,
                0);
            Assertions.True(report.Passed, "Full maintenance-loop matrix failed.");
            Assertions.Equal(MaintenanceQualificationStage.MaintenanceLoopPassed, report.Stage, "Loop stage mismatch.");
        }

        private static void MaintenanceSecondItemMutationFails()
        {
            MaintenanceQualificationBaseline baseline = MaintenanceBaseline();
            MaintenanceQualificationObservation observation = MaintenanceObservation(
                baseline,
                BrokenState(),
                6,
                BrokenState(),
                1,
                1,
                1,
                11,
                20,
                30,
                0,
                0);
            MaintenanceQualificationReport report = new MaintenanceQualificationService().Evaluate(baseline, observation);
            Assertions.False(report.Passed, "Second-item mutation was not detected.");
        }

        private static void MaintenanceResourceDriftFails()
        {
            MaintenanceQualificationBaseline baseline = MaintenanceBaseline();
            MaintenanceQualificationReport report = EvaluateMaintenance(
                baseline,
                BrokenState(),
                6,
                0,
                0,
                1,
                1,
                11,
                20,
                30,
                0,
                0);
            Assertions.False(report.Passed, "Unexpected extra kit consumption was not detected.");
        }

        private static void MaintenanceFaultDeltaFails()
        {
            MaintenanceQualificationBaseline baseline = MaintenanceBaseline();
            MaintenanceQualificationReport report = EvaluateMaintenance(
                baseline,
                WreckedState(),
                5,
                0,
                2,
                1,
                1,
                10,
                20,
                30,
                1,
                0);
            Assertions.False(report.Passed, "New fault was not detected.");
        }

        private static void MaintenanceDuplicateDeltaFails()
        {
            MaintenanceQualificationBaseline baseline = MaintenanceBaseline();
            MaintenanceQualificationReport report = EvaluateMaintenance(
                baseline,
                WreckedState(),
                5,
                0,
                2,
                1,
                1,
                10,
                20,
                30,
                0,
                1);
            Assertions.False(report.Passed, "New duplicate application was not detected.");
        }

        private static void MaintenanceIdentityChangeFails()
        {
            MaintenanceQualificationBaseline baseline = MaintenanceBaseline();
            MaintenanceQualificationObservation observation = new MaintenanceQualificationObservation(
                "kmg-item-999999",
                baseline.RuntimeReferenceHash,
                baseline.Revision,
                WreckedState(),
                baseline.VisibleFirearms,
                baseline.SecondRepositoryIdentity,
                baseline.SecondRuntimeReferenceHash,
                baseline.SecondRevision,
                baseline.SecondItemState,
                baseline.RepairKits,
                baseline.BlackPowder,
                baseline.LeadBalls,
                baseline.OverhaulCompleted,
                baseline.RepairCompleted,
                baseline.ReloadCompleted,
                baseline.TotalFaults,
                baseline.TotalDuplicates);
            MaintenanceQualificationReport report = new MaintenanceQualificationService().Evaluate(baseline, observation);
            Assertions.False(report.Passed, "Changed exact-item identity was not detected.");
        }

        private static void MaintenanceSessionLifecycle()
        {
            MaintenanceQualificationSession.Reset();
            Assertions.False(MaintenanceQualificationSession.IsActive, "Session reset failed.");
            MaintenanceQualificationBaseline baseline = MaintenanceBaseline();
            MaintenanceQualificationSession.Begin(baseline);
            Assertions.True(MaintenanceQualificationSession.IsActive, "Session did not activate.");
            MaintenanceQualificationBaseline observed;
            Assertions.True(MaintenanceQualificationSession.TryGetBaseline(out observed), "Session did not expose baseline.");
            Assertions.True(ReferenceEquals(baseline, observed), "Session changed baseline reference.");
            MaintenanceQualificationReport report = MaintenanceQualificationSession.Evaluate(
                MaintenanceObservation(
                    baseline,
                    WreckedState(),
                    5,
                    FirearmState.CreateEmpty(),
                    2,
                    1,
                    1,
                    10,
                    20,
                    30,
                    0,
                    0));
            Assertions.True(report.Passed, "Active session did not evaluate fixture.");
            MaintenanceQualificationSession.Reset();
            Assertions.False(MaintenanceQualificationSession.IsActive, "Session did not reset.");
        }

        private static void MaintenanceReportFormat()
        {
            var report = new MaintenanceQualificationReport(
                MaintenanceQualificationStage.RepairPassed,
                true,
                new[] { "identity=PASS", "repair=PASS" });
            string text = report.ToString();
            Assertions.True(text.Contains("overall=PASS"), "Report lost overall status.");
            Assertions.True(text.Contains("stage=RepairPassed"), "Report lost stage.");
            Assertions.True(text.Contains("identity=PASS"), "Report lost checks.");
        }

        private static FirearmRepairResult Repair(
            IFirearmRepairStateStore stateStore,
            IRepairKitInventory inventory)
        {
            return new FirearmRepairTransactionService()
                .TryRepairBrokenToNormal(stateStore, inventory);
        }

        private static MaintenanceQualificationBaseline MaintenanceBaseline()
        {
            return new MaintenanceQualificationBaseline(
                "kmg-item-000001",
                0x1111,
                5,
                WreckedState(),
                2,
                "kmg-item-000002",
                0x2222,
                0,
                FirearmState.CreateEmpty(),
                2,
                1,
                1,
                10,
                20,
                30,
                0,
                0);
        }

        private static MaintenanceQualificationReport EvaluateMaintenance(
            MaintenanceQualificationBaseline baseline,
            FirearmState exactState,
            long revision,
            long secondRevision,
            int repairKits,
            int powder,
            int lead,
            long overhaulCompleted,
            long repairCompleted,
            long reloadCompleted,
            long faults,
            long duplicates)
        {
            return new MaintenanceQualificationService().Evaluate(
                baseline,
                MaintenanceObservation(
                    baseline,
                    exactState,
                    revision,
                    FirearmState.CreateEmpty(),
                    repairKits,
                    powder,
                    lead,
                    overhaulCompleted,
                    repairCompleted,
                    reloadCompleted,
                    faults,
                    duplicates,
                    secondRevision));
        }

        private static MaintenanceQualificationObservation MaintenanceObservation(
            MaintenanceQualificationBaseline baseline,
            FirearmState exactState,
            long revision,
            FirearmState secondState,
            int repairKits,
            int powder,
            int lead,
            long overhaulCompleted,
            long repairCompleted,
            long reloadCompleted,
            long faults,
            long duplicates,
            long? secondRevision = null)
        {
            return new MaintenanceQualificationObservation(
                baseline.RepositoryIdentity,
                baseline.RuntimeReferenceHash,
                revision,
                exactState,
                baseline.VisibleFirearms,
                baseline.SecondRepositoryIdentity,
                baseline.SecondRuntimeReferenceHash,
                secondRevision ?? baseline.SecondRevision,
                secondState,
                repairKits,
                powder,
                lead,
                overhaulCompleted,
                repairCompleted,
                reloadCompleted,
                faults,
                duplicates);
        }

        private sealed class FakeFirearmRepairStateStore : IFirearmRepairStateStore
        {
            private FirearmState _state;

            internal FakeFirearmRepairStateStore(FirearmState state)
            {
                _state = state ?? throw new ArgumentNullException("state");
            }

            internal FirearmState State { get { return _state; } }
            internal int ReadCalls { get; private set; }
            internal int ReplaceCalls { get; private set; }
            internal int ThrowOnReplaceCall { get; set; }
            internal bool MutateBeforeReplaceFailure { get; set; }
            internal bool ThrowOnSecondReplace { get; set; }
            internal bool ReturnNullOnRead { get; set; }
            internal int OverrideReadCall { get; set; }
            internal FirearmState OverrideReadState { get; set; }

            public FirearmState Read()
            {
                ReadCalls++;
                if (ReturnNullOnRead) return null;
                return ReadCalls == OverrideReadCall
                    ? OverrideReadState
                    : _state;
            }

            public void Replace(FirearmState expectedCurrent, FirearmState replacement)
            {
                if (expectedCurrent == null || replacement == null)
                {
                    throw new ArgumentNullException(
                        expectedCurrent == null ? "expectedCurrent" : "replacement");
                }

                ReplaceCalls++;
                if (_state != expectedCurrent)
                {
                    throw new InvalidOperationException(
                        "Synthetic repair expected-current mismatch.");
                }

                bool shouldThrow = ReplaceCalls == ThrowOnReplaceCall ||
                    (ReplaceCalls == 2 && ThrowOnSecondReplace);
                if (shouldThrow && !MutateBeforeReplaceFailure)
                {
                    throw new InvalidOperationException(
                        "Synthetic repair state-replace failure.");
                }

                _state = replacement;
                if (shouldThrow)
                {
                    throw new InvalidOperationException(
                        "Synthetic repair post-mutation state-replace failure.");
                }
            }
        }
    }
}
