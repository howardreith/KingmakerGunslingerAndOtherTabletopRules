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
            var inventory = new FakeRepairKitInventory(1);
            FirearmRepairResult result = Repair(stateStore, inventory);

            Assertions.Equal(FirearmRepairStatus.Repaired, result.Status, "Repair status mismatch.");
            Assertions.Equal(FirearmCondition.Normal, stateStore.State.Condition, "Repair did not reach Normal.");
            Assertions.True(stateStore.State.IsEmpty, "An empty Broken firearm must repair to empty Normal.");
            Assertions.Equal(1, inventory.Kits, "Repair must not spend or create Gunsmith's Kits.");
            Assertions.Equal(0, inventory.RemoveCalls,
                "Unified repair attempted a tool removal.");
            Assertions.Equal(0, inventory.AddCalls,
                "Unified repair attempted a tool refund.");
            Assertions.Equal(1, stateStore.ReplaceCalls, "Repair did not write exact state once.");
        }

        private static void RepairTransactionNormalRejected()
        {
            FirearmState normal = LoadedState(2, LeadBall(),
                FirearmCondition.Normal);
            var stateStore = new FakeFirearmRepairStateStore(normal);
            var inventory = new FakeRepairKitInventory(1);
            FirearmRepairResult result = Repair(stateStore, inventory);
            Assertions.Equal(FirearmRepairStatus.NotBroken, result.Status, "Normal rejection status mismatch.");
            Assertions.Equal(0, stateStore.ReplaceCalls, "Normal rejection mutated state.");
            Assertions.Equal(normal, stateStore.State,
                "Normal rejection changed exact loaded state or ammunition identity.");
            Assertions.Equal(1, inventory.Kits, "Normal rejection mutated the tool count.");
        }

        private static void RepairTransactionWreckedToNormal()
        {
            // Mission Z-FIREARM-MAINTENANCE: field repair is Broken-only; a
            // Wrecked firearm is restored by a completed full rest, never by
            // the ordinary repair action or the legacy Overhaul alias.
            var stateStore = new FakeFirearmRepairStateStore(WreckedState());
            var inventory = new FakeRepairKitInventory(1);
            FirearmRepairResult result = Repair(stateStore, inventory);
            Assertions.Equal(
                FirearmRepairStatus.WreckedRequiresRest, result.Status,
                "Wrecked field repair must fail with the rest-only status.");
            Assertions.Equal(FirearmCondition.Wrecked,
                stateStore.State.Condition,
                "A rejected Wrecked repair must not change the condition.");
            Assertions.True(stateStore.State.IsEmpty,
                "A rejected Wrecked repair must leave the firearm empty.");
            Assertions.Equal(1, inventory.Kits,
                "A rejected Wrecked repair must not spend the reusable tool.");
            Assertions.Equal(0, inventory.RemoveCalls,
                "A rejected Wrecked repair attempted a tool removal.");
            Assertions.Equal(0, stateStore.ReplaceCalls,
                "A rejected Wrecked repair mutated the exact item state.");
        }

        private static void RepairTransactionLoadedSingleShotSuccess()
        {
            FirearmState loaded = LoadedState(1, LeadBall(), FirearmCondition.Broken);
            var stateStore = new FakeFirearmRepairStateStore(loaded);
            var inventory = new FakeRepairKitInventory(1);
            FirearmRepairResult result = Repair(stateStore, inventory);
            Assertions.Equal(FirearmRepairStatus.Repaired, result.Status,
                "Loaded single-shot repair status mismatch.");
            Assertions.Equal(FirearmCondition.Normal, stateStore.State.Condition,
                "Loaded single-shot repair did not reach Normal.");
            Assertions.Equal(1, stateStore.State.LoadedRounds,
                "Loaded single-shot repair destroyed a surviving round.");
            Assertions.Equal(LeadBall(), stateStore.State.LoadedAmmunition,
                "Loaded single-shot repair lost ammunition identity.");
            Assertions.Equal(1, inventory.Kits,
                "Loaded repair must not spend the reusable tool.");
            Assertions.Equal(0, inventory.AddCalls,
                "Successful repair attempted an inventory refund.");
        }

        private static void RepairTransactionLoadedMultiRoundSuccess()
        {
            FirearmState loaded = LoadedState(6, LeadBall(),
                FirearmCondition.Broken);
            var stateStore = new FakeFirearmRepairStateStore(loaded);
            var inventory = new FakeRepairKitInventory(1);
            FirearmRepairResult result = Repair(stateStore, inventory);
            Assertions.Equal(FirearmRepairStatus.Repaired, result.Status,
                "Loaded multi-round repair status mismatch.");
            Assertions.Equal(FirearmCondition.Normal, stateStore.State.Condition,
                "Loaded multi-round repair did not reach Normal.");
            Assertions.Equal(6, stateStore.State.LoadedRounds,
                "Loaded multi-round repair lost surviving rounds.");
            Assertions.Equal(LeadBall(), stateStore.State.LoadedAmmunition,
                "Loaded multi-round repair lost ammunition identity.");
            Assertions.Equal(1, inventory.Kits,
                "Loaded multi-round repair must not spend the reusable tool.");
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
            Assertions.Equal(FirearmRepairStatus.InsufficientRepairKit, result.Status, "Missing-tool status mismatch.");
            Assertions.Equal(0, stateStore.ReplaceCalls, "Missing-tool rejection mutated state.");
            Assertions.Equal(loaded, stateStore.State,
                "Missing-tool rejection changed loaded rounds or ammunition identity.");
            Assertions.Equal(0, inventory.Kits,
                "Missing-tool rejection mutated the inventory.");
            Assertions.Equal(0, inventory.RemoveCalls,
                "Missing-tool rejection attempted an inventory removal.");
            Assertions.True(FirearmRepairTransactionService.GetRejection(
                    loaded, new RepairKitInventorySnapshot(1)) == null,
                "Loaded ordinary repair still returns a rejection status.");
        }

        private static void RepairTransactionRepeatedCyclesReuseTool()
        {
            var inventory = new FakeRepairKitInventory(1);
            for (int cycle = 0; cycle < 3; cycle++)
            {
                // Every cycle misfires from Normal to Broken; a Broken-to-Wrecked
                // second misfire is rest-only under the mission contract.
                FirearmState loaded = LoadedState(2, LeadBall(),
                    FirearmCondition.Normal);
                FirearmState damaged =
                    FirearmStateMachine.ApplyMisfireDamage(loaded);
                var stateStore = new FakeFirearmRepairStateStore(damaged);
                FirearmRepairResult result = Repair(stateStore, inventory);
                Assertions.Equal(FirearmRepairStatus.Repaired, result.Status,
                    "Damage/repair cycle " + cycle + " did not repair.");
                Assertions.Equal(FirearmCondition.Normal,
                    stateStore.State.Condition,
                    "Damage/repair cycle " + cycle + " did not reach Normal.");
            }

            Assertions.Equal(1, inventory.Kits,
                "Repeated damage/repair cycles spent or created Gunsmith's Kits.");
            Assertions.Equal(0, inventory.RemoveCalls,
                "Repeated damage/repair cycles removed tool inventory.");
            Assertions.Equal(0, inventory.AddCalls,
                "Repeated damage/repair cycles added tool inventory.");
        }

        private static void RepairTransactionNullStateStore()
        {
            Assertions.Throws<ArgumentNullException>(
                () => new FirearmRepairTransactionService().TryRepairToNormal(
                    null,
                    new FakeRepairKitInventory(1)),
                "Null repair state store must be rejected.");
        }

        private static void RepairTransactionNullInventory()
        {
            Assertions.Throws<ArgumentNullException>(
                () => new FirearmRepairTransactionService().TryRepairToNormal(
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

        private static void RepairTransactionStateWriteFailureLeavesInventory()
        {
            FirearmState broken = LoadedState(1, LeadBall(),
                FirearmCondition.Broken);
            var store = new FakeFirearmRepairStateStore(broken) { ThrowOnReplaceCall = 1 };
            var inventory = new FakeRepairKitInventory(1);
            FirearmRepairTransactionException exception = Assertions.Throws<FirearmRepairTransactionException>(
                () => Repair(store, inventory),
                "State-write failure must surface transaction exception.");
            Assertions.True(exception.RollbackSucceeded, "State-write rollback should succeed.");
            Assertions.Equal(broken, store.State, "State-write failure changed state.");
            Assertions.Equal(1, inventory.Kits, "State-write failure mutated tool inventory.");
            Assertions.Equal(0, inventory.RemoveCalls,
                "State-write failure attempted a tool removal.");
            Assertions.Equal(0, inventory.AddCalls,
                "State-write failure attempted a tool refund.");
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
            var inventory = new FakeRepairKitInventory(1);
            FirearmRepairTransactionException exception = Assertions.Throws<FirearmRepairTransactionException>(
                () => Repair(store, inventory),
                "Post-mutation state failure must surface transaction exception.");
            Assertions.True(exception.RollbackSucceeded, "Post-mutation rollback should succeed.");
            Assertions.Equal(broken, store.State, "Post-mutation failure did not restore state.");
            Assertions.Equal(5, store.State.LoadedRounds,
                "Post-mutation rollback lost surviving loaded rounds.");
            Assertions.Equal(1, inventory.Kits, "Post-mutation failure mutated the tool count.");
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
            var inventory = new FakeRepairKitInventory(1);
            FirearmRepairTransactionException exception =
                Assertions.Throws<FirearmRepairTransactionException>(
                    () => Repair(store, inventory),
                    "Post-write verification failure must surface a transaction exception.");
            Assertions.True(exception.RollbackSucceeded,
                "Post-write verification rollback should succeed.");
            Assertions.Equal(loaded, store.State,
                "Verification failure did not restore exact rounds and ammunition identity.");
            Assertions.Equal(1, inventory.Kits,
                "Verification failure mutated the reusable tool count.");
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
                () => Repair(store, new FakeRepairKitInventory(1)),
                "State rollback failure must be surfaced.");
            Assertions.False(exception.RollbackSucceeded, "Rollback failure was hidden.");
            Assertions.True(exception.StateRollbackException != null, "State rollback exception was not retained.");
        }

        private static void RepairTransactionExternalToolDriftFailsClosed()
        {
            FirearmState broken = LoadedState(1, LeadBall(),
                FirearmCondition.Broken);
            // The tool count drops externally after the transaction captured its
            // baseline; even a no-consumption repair must notice and fail closed
            // rather than commit a result whose inventory proof is false.
            var store = new FakeFirearmRepairStateStore(broken);
            var inventory = new FakeRepairKitInventory(2)
            {
                RemoveSilentlyAfterReads = 1
            };
            FirearmRepairTransactionException exception =
                Assertions.Throws<FirearmRepairTransactionException>(
                    () => Repair(store, inventory),
                    "External tool drift during repair must surface a transaction exception.");
            Assertions.True(exception.RollbackSucceeded,
                "Drift failure rollback should restore state.");
            Assertions.Equal(broken, store.State,
                "Drift failure changed the exact firearm state.");
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
                new RepairKitInventorySnapshot(1),
                new RepairKitInventorySnapshot(1));
            Assertions.True(result.Succeeded, "Successful repair result did not report success.");
            Assertions.True(result.ToString().Contains("status=Repaired"), "Repair result format lost status.");
            Assertions.Throws<ArgumentException>(
                () => new FirearmRepairResult(
                    FirearmRepairStatus.Repaired,
                    broken,
                    normal,
                    new RepairKitInventorySnapshot(1),
                    new RepairKitInventorySnapshot(0)),
                "A successful repair must not change the reusable-tool count.");
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
                "unload before repair", "repair requires an empty",
                "is destroyed", "will be destroyed", "consume one",
                "overhaul first", "firearm repair kit" })
                Assertions.False(playerText.Contains(obsolete),
                    "Player-facing repair text retains obsolete instruction: " +
                    obsolete);
            Assertions.True(ability.Contains(
                    "ammunition still loaded in that firearm is preserved") &&
                runtime.Contains("every surviving loaded round is preserved") &&
                action.Contains("every surviving loaded round is preserved") &&
                ability.Contains("reusable Gunsmith's Kit") &&
                runtime.Contains("reusable Gunsmith's Kit"),
                "Unified no-cost repair and ammunition preservation are not stated consistently.");
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
                new RepairKitInventorySnapshot(1));
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
                new RepairKitInventorySnapshot(1));
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
                new RepairKitInventorySnapshot(1));
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
                BrokenState(),
                5,
                0,
                1,
                1,
                1,
                10,
                20,
                0,
                0);
            Assertions.True(report.Passed, "Fixture-ready matrix failed.");
            Assertions.Equal(MaintenanceQualificationStage.FixtureReady, report.Stage, "Fixture stage mismatch.");
        }

        private static void MaintenanceRemovedOverhaulStageRejected()
        {
            // A Wrecked firearm is rest-only (mission Z-FIREARM-MAINTENANCE):
            // it can never pass through the field-repair maintenance loop.
            MaintenanceQualificationBaseline baseline = MaintenanceBaseline();
            MaintenanceQualificationReport report = EvaluateMaintenance(
                baseline,
                WreckedState(),
                5,
                0,
                1,
                1,
                1,
                10,
                20,
                0,
                0);
            Assertions.False(report.Passed,
                "A rest-only Wrecked state still passes the field-repair loop.");
            Assertions.Equal(MaintenanceQualificationStage.Failed, report.Stage,
                "A Wrecked observation must classify as Failed, not a loop stage.");
        }

        private static void MaintenanceRepairPass()
        {
            MaintenanceQualificationBaseline baseline = MaintenanceBaseline();
            MaintenanceQualificationReport report = EvaluateMaintenance(
                baseline,
                FirearmState.CreateEmpty(),
                6,
                0,
                1,
                1,
                1,
                11,
                20,
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
                7,
                0,
                1,
                0,
                0,
                11,
                21,
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
                10,
                20,
                0,
                0,
                1);
            MaintenanceQualificationReport report = new MaintenanceQualificationService().Evaluate(baseline, observation);
            Assertions.False(report.Passed, "Second-item mutation was not detected.");
        }

        private static void MaintenanceToolSpendingFails()
        {
            MaintenanceQualificationBaseline baseline = MaintenanceBaseline();
            MaintenanceQualificationReport report = EvaluateMaintenance(
                baseline,
                FirearmState.CreateEmpty(),
                6,
                0,
                0,
                1,
                1,
                11,
                20,
                0,
                0);
            Assertions.False(report.Passed,
                "A spent or removed Gunsmith's Kit was not detected.");
        }

        private static void MaintenanceFaultDeltaFails()
        {
            MaintenanceQualificationBaseline baseline = MaintenanceBaseline();
            MaintenanceQualificationReport report = EvaluateMaintenance(
                baseline,
                BrokenState(),
                5,
                0,
                1,
                1,
                1,
                10,
                20,
                1,
                0);
            Assertions.False(report.Passed, "New fault was not detected.");
        }

        private static void MaintenanceDuplicateDeltaFails()
        {
            MaintenanceQualificationBaseline baseline = MaintenanceBaseline();
            MaintenanceQualificationReport report = EvaluateMaintenance(
                baseline,
                BrokenState(),
                5,
                0,
                1,
                1,
                1,
                10,
                20,
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
                BrokenState(),
                baseline.VisibleFirearms,
                baseline.SecondRepositoryIdentity,
                baseline.SecondRuntimeReferenceHash,
                baseline.SecondRevision,
                baseline.SecondItemState,
                baseline.GunsmithKits,
                baseline.BlackPowder,
                baseline.LeadBalls,
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
                    BrokenState(),
                    5,
                    FirearmState.CreateEmpty(),
                    1,
                    1,
                    1,
                    10,
                    20,
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
                .TryRepairToNormal(stateStore, inventory);
        }

        private static MaintenanceQualificationBaseline MaintenanceBaseline()
        {
            return new MaintenanceQualificationBaseline(
                "kmg-item-000001",
                0x1111,
                5,
                BrokenState(),
                2,
                "kmg-item-000002",
                0x2222,
                0,
                FirearmState.CreateEmpty(),
                1,
                1,
                1,
                10,
                20,
                0,
                0);
        }

        private static MaintenanceQualificationReport EvaluateMaintenance(
            MaintenanceQualificationBaseline baseline,
            FirearmState exactState,
            long revision,
            long secondRevision,
            int gunsmithKits,
            int powder,
            int lead,
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
                    gunsmithKits,
                    powder,
                    lead,
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
            int gunsmithKits,
            int powder,
            int lead,
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
                gunsmithKits,
                powder,
                lead,
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
