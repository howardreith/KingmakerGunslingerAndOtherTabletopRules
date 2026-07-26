using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Kingmaker.Items;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using KingmakerGunslinger.Ammunition;
using KingmakerGunslinger.Diagnostics;
using KingmakerGunslinger.Explosions;
using KingmakerGunslinger.Firearms;
using KingmakerGunslinger.Firing;
using KingmakerGunslinger.Misfires;
using KingmakerGunslinger.Qualification;
using KingmakerGunslinger.Recovery;
using KingmakerGunslinger.Reloading;
using KingmakerGunslinger.Rules;

namespace KingmakerGunslinger.Development
{
    /// <summary>
    /// Sprint 29 player-facing ordinary repair and accelerated maintenance-loop
    /// qualification controls. This partial keeps the large development bridge focused
    /// while sharing its exact runtime, inventory, and item-resolution helpers.
    /// </summary>
    internal sealed partial class KingmakerDevelopmentBridge
    {
        internal DevelopmentActionResult DescribeRepairReadiness()
        {
            RuntimeContext runtime = ResolveRuntime(requireUnit: true);
            UnitDescriptor descriptor = RequireConcreteDescriptor(runtime);
            Ability ability = descriptor.Abilities.GetAbility(_repairAbility);
            FirearmRepairAvailability availability = RepairTestMusketRuntime.Evaluate(
                descriptor,
                _testMusketItem,
                _repairKitItem);
            return DevelopmentActionResult.Success(
                string.Format(
                    CultureInfo.InvariantCulture,
                    "Selected unit={0}; hasRepairAbility={1}; readiness=[{2}]; runtime=[{3}].",
                    runtime.UnitName,
                    ability != null,
                    availability,
                    RepairRuntimeDiagnostics.Describe()));
        }

        internal DevelopmentActionResult RepairEquippedTestMusketNowForDebug()
        {
            RuntimeContext runtime = ResolveRuntime(requireUnit: true);
            UnitDescriptor descriptor = RequireConcreteDescriptor(runtime);
            EnsureRepairAbility(descriptor);
            FirearmRepairAvailability availability = RepairTestMusketRuntime.Evaluate(
                descriptor,
                _testMusketItem,
                _repairKitItem);
            if (!availability.IsAvailable)
            {
                return DevelopmentActionResult.Failure(
                    "Immediate diagnostic repair was rejected without mutation: " +
                    availability.Reason);
            }

            FirearmRepairRuntimeResult result = RepairTestMusketRuntime.Execute(
                descriptor,
                _testMusketItem,
                _repairKitItem);
            RepairRuntimeDiagnostics.Record(result);
            return DevelopmentActionResult.Success(
                "Immediate diagnostic ordinary repair completed; this bypassed full-round action economy: " +
                result + ".");
        }

        internal DevelopmentActionResult PrepareMaintenanceQualificationFixture()
        {
            RuntimeContext runtime = ResolveRuntime(requireUnit: true);
            UnitDescriptor descriptor = RequireConcreteDescriptor(runtime);
            EnsureReloadAbility(descriptor);
            EnsureOverhaulAbility(descriptor);
            EnsureRepairAbility(descriptor);

            object targetItem = RequireSingleEquippedTestMusket(runtime);
            EnsureSecondVisibleTestMusket(runtime, targetItem);

            List<VisibleFirearmEntry> entries = CollectVisibleFirearmEntries(runtime);
            VisibleFirearmEntry target = entries.SingleOrDefault(
                entry => ReferenceEquals(entry.Item, targetItem));
            if (target == null)
            {
                throw new InvalidOperationException(
                    "The exact equipped Test Musket disappeared while preparing the maintenance fixture.");
            }

            VisibleFirearmEntry second = entries
                .Where(entry => !ReferenceEquals(entry.Item, targetItem))
                .OrderBy(entry => entry.Snapshot.Repository.EntryId)
                .ThenBy(entry => entry.Snapshot.Repository.RuntimeReferenceHash)
                .FirstOrDefault();
            if (second == null)
            {
                throw new InvalidOperationException(
                    "The Sprint 29 fixture requires a second independently tracked Test Musket.");
            }

            _stateService.Set(
                target.Item,
                FirearmStateMachine.Wreck(FirearmState.CreateEmpty()));
            _stateService.Set(second.Item, FirearmState.CreateEmpty());

            KingmakerRepairKitInventory repairKits = ResolveRepairKitInventory();
            int repairKitCount = repairKits.Count();
            if (repairKitCount < 2)
            {
                repairKits.Add(2 - repairKitCount);
            }

            KingmakerBasicAmmunitionInventory ammunition =
                ResolveBasicAmmunitionInventory();
            int powder = ammunition.Count(BasicAmmunitionComponent.BlackPowderCharge);
            int lead = ammunition.Count(BasicAmmunitionComponent.LeadBall);
            if (powder < 1)
            {
                ammunition.Add(BasicAmmunitionComponent.BlackPowderCharge, 1 - powder);
            }

            if (lead < 1)
            {
                ammunition.Add(BasicAmmunitionComponent.LeadBall, 1 - lead);
            }

            entries = CollectVisibleFirearmEntries(runtime);
            target = RequireMatchingEntry(entries, target.Item);
            second = RequireMatchingEntry(entries, second.Item);
            RepairKitInventorySnapshot kitSnapshot =
                RepairKitInventorySnapshot.Capture(repairKits);
            BasicAmmunitionInventorySnapshot ammunitionSnapshot =
                BasicAmmunitionInventorySnapshot.Capture(ammunition);

            MaintenanceQualificationBaseline baseline =
                new MaintenanceQualificationBaseline(
                    target.Snapshot.Repository.RepositoryIdentity,
                    target.Snapshot.Repository.RuntimeReferenceHash,
                    target.Snapshot.Repository.Revision,
                    target.Snapshot.Repository.State,
                    entries.Count,
                    second.Snapshot.Repository.RepositoryIdentity,
                    second.Snapshot.Repository.RuntimeReferenceHash,
                    second.Snapshot.Repository.Revision,
                    second.Snapshot.Repository.State,
                    kitSnapshot.RepairKits,
                    ammunitionSnapshot.BlackPowderCharges,
                    ammunitionSnapshot.LeadBalls,
                    OverhaulRuntimeDiagnostics.Completed,
                    RepairRuntimeDiagnostics.Completed,
                    ReloadRuntimeDiagnostics.Loaded,
                    GetTotalFaults(),
                    GetTotalDuplicates());
            MaintenanceQualificationSession.Begin(baseline);

            MaintenanceQualificationReport report =
                MaintenanceQualificationSession.Evaluate(
                    CaptureMaintenanceObservation(runtime, baseline));
            return DevelopmentActionResult.Success(
                string.Format(
                    CultureInfo.InvariantCulture,
                    "Prepared Sprint 29 maintenance fixture for {0}; target={1}/0x{2:x8}; second={3}/0x{4:x8}; visibleFirearms={5}; repairKits={6}; powder={7}; leadBalls={8}; matrix=[{9}]. Next: complete Overhaul, print the matrix, complete Repair, print the matrix, then Reload and print the final matrix.",
                    runtime.UnitName,
                    baseline.RepositoryIdentity,
                    baseline.RuntimeReferenceHash,
                    baseline.SecondRepositoryIdentity,
                    baseline.SecondRuntimeReferenceHash,
                    baseline.VisibleFirearms,
                    baseline.RepairKits,
                    baseline.BlackPowder,
                    baseline.LeadBalls,
                    report));
        }

        internal DevelopmentActionResult RunMaintenanceQualificationImmediately()
        {
            DevelopmentActionResult fixture = PrepareMaintenanceQualificationFixture();
            if (!fixture.Succeeded)
            {
                return DevelopmentActionResult.Failure(
                    "Sprint 29 immediate maintenance qualification could not prepare its fixture: " +
                    fixture.Message);
            }

            var checkpoints = new List<string>();
            checkpoints.Add(DescribeMaintenanceQualification().Message);

            DevelopmentActionResult overhaul = OverhaulEquippedTestMusketNowForDebug();
            if (!overhaul.Succeeded)
            {
                return DevelopmentActionResult.Failure(
                    "Sprint 29 immediate maintenance qualification stopped at Overhaul: " +
                    overhaul.Message + "; checkpoints=[" +
                    string.Join(" || ", checkpoints) + "].");
            }

            DevelopmentActionResult overhaulReport = DescribeMaintenanceQualification();
            checkpoints.Add(overhaulReport.Message);
            if (!overhaulReport.Succeeded)
            {
                return DevelopmentActionResult.Failure(
                    "Sprint 29 immediate maintenance qualification failed after Overhaul; checkpoints=[" +
                    string.Join(" || ", checkpoints) + "].");
            }

            DevelopmentActionResult repair = RepairEquippedTestMusketNowForDebug();
            if (!repair.Succeeded)
            {
                return DevelopmentActionResult.Failure(
                    "Sprint 29 immediate maintenance qualification stopped at Repair: " +
                    repair.Message + "; checkpoints=[" +
                    string.Join(" || ", checkpoints) + "].");
            }

            DevelopmentActionResult repairReport = DescribeMaintenanceQualification();
            checkpoints.Add(repairReport.Message);
            if (!repairReport.Succeeded)
            {
                return DevelopmentActionResult.Failure(
                    "Sprint 29 immediate maintenance qualification failed after Repair; checkpoints=[" +
                    string.Join(" || ", checkpoints) + "].");
            }

            DevelopmentActionResult reload = ReloadEquippedTestMusketNowForDebug();
            if (!reload.Succeeded)
            {
                return DevelopmentActionResult.Failure(
                    "Sprint 29 immediate maintenance qualification stopped at Reload: " +
                    reload.Message + "; checkpoints=[" +
                    string.Join(" || ", checkpoints) + "].");
            }

            DevelopmentActionResult finalReport = DescribeMaintenanceQualification();
            checkpoints.Add(finalReport.Message);
            string message =
                "Sprint 29 one-command immediate maintenance qualification " +
                (finalReport.Succeeded ? "PASSED" : "FAILED") +
                "; checkpoints=[" + string.Join(" || ", checkpoints) + "]. " +
                "This diagnostic bypasses action economy; use the action-bar abilities separately to qualify full-round delivery and interruption.";
            return finalReport.Succeeded
                ? DevelopmentActionResult.Success(message)
                : DevelopmentActionResult.Failure(message);
        }

        internal DevelopmentActionResult DescribeMaintenanceQualification()
        {
            MaintenanceQualificationBaseline baseline;
            if (!MaintenanceQualificationSession.TryGetBaseline(out baseline))
            {
                return DevelopmentActionResult.Failure(
                    "No Sprint 29 maintenance qualification fixture is active. Prepare the fixture first.");
            }

            RuntimeContext runtime = ResolveRuntime(requireUnit: true);
            MaintenanceQualificationReport report =
                MaintenanceQualificationSession.Evaluate(
                    CaptureMaintenanceObservation(runtime, baseline));
            string message = string.Format(
                CultureInfo.InvariantCulture,
                "Sprint 29 maintenance qualification for {0}: {1}. Required sequence: FixtureReady -> OverhaulPassed -> RepairPassed -> MaintenanceLoopPassed.",
                runtime.UnitName,
                report);
            return report.Passed
                ? DevelopmentActionResult.Success(message)
                : DevelopmentActionResult.Failure(message);
        }

        internal DevelopmentActionResult ResetMaintenanceQualification()
        {
            bool wasActive = MaintenanceQualificationSession.IsActive;
            MaintenanceQualificationSession.Reset();
            return DevelopmentActionResult.Success(
                wasActive
                    ? "Cleared the process-local Sprint 29 maintenance qualification baseline. No item, resource, or ability state was mutated."
                    : "No Sprint 29 maintenance qualification baseline was active. No mutation was requested.");
        }

        private MaintenanceQualificationObservation CaptureMaintenanceObservation(
            RuntimeContext runtime,
            MaintenanceQualificationBaseline baseline)
        {
            List<VisibleFirearmEntry> entries = CollectVisibleFirearmEntries(runtime);
            VisibleFirearmEntry target = entries.SingleOrDefault(
                entry =>
                    string.Equals(
                        entry.Snapshot.Repository.RepositoryIdentity,
                        baseline.RepositoryIdentity,
                        StringComparison.Ordinal) &&
                    entry.Snapshot.Repository.RuntimeReferenceHash ==
                        baseline.RuntimeReferenceHash);
            if (target == null)
            {
                throw new InvalidOperationException(
                    "The exact fixture firearm is no longer visible in this process.");
            }

            VisibleFirearmEntry second = entries.SingleOrDefault(
                entry =>
                    string.Equals(
                        entry.Snapshot.Repository.RepositoryIdentity,
                        baseline.SecondRepositoryIdentity,
                        StringComparison.Ordinal) &&
                    entry.Snapshot.Repository.RuntimeReferenceHash ==
                        baseline.SecondRuntimeReferenceHash);
            if (second == null)
            {
                throw new InvalidOperationException(
                    "The independent second fixture firearm is no longer visible in this process.");
            }

            RepairKitInventorySnapshot kits =
                RepairKitInventorySnapshot.Capture(ResolveRepairKitInventory());
            BasicAmmunitionInventorySnapshot ammunition =
                BasicAmmunitionInventorySnapshot.Capture(
                    ResolveBasicAmmunitionInventory());
            return new MaintenanceQualificationObservation(
                target.Snapshot.Repository.RepositoryIdentity,
                target.Snapshot.Repository.RuntimeReferenceHash,
                target.Snapshot.Repository.Revision,
                target.Snapshot.Repository.State,
                entries.Count,
                second.Snapshot.Repository.RepositoryIdentity,
                second.Snapshot.Repository.RuntimeReferenceHash,
                second.Snapshot.Repository.Revision,
                second.Snapshot.Repository.State,
                kits.RepairKits,
                ammunition.BlackPowderCharges,
                ammunition.LeadBalls,
                OverhaulRuntimeDiagnostics.Completed,
                RepairRuntimeDiagnostics.Completed,
                ReloadRuntimeDiagnostics.Loaded,
                GetTotalFaults(),
                GetTotalDuplicates());
        }

        private void EnsureSecondVisibleTestMusket(
            RuntimeContext runtime,
            object targetItem)
        {
            int attempts = 0;
            while (CollectVisibleFirearmEntries(runtime)
                .Count(entry => !ReferenceEquals(entry.Item, targetItem)) < 1 &&
                attempts < 2)
            {
                AddTestMusket();
                attempts++;
            }

            if (CollectVisibleFirearmEntries(runtime)
                .Count(entry => !ReferenceEquals(entry.Item, targetItem)) < 1)
            {
                throw new InvalidOperationException(
                    "Could not prepare a second visible Test Musket for isolation evidence.");
            }
        }

        private object RequireSingleEquippedTestMusket(RuntimeContext runtime)
        {
            object[] matches = CollectEquippedRuntimeWeaponItems(runtime)
                .Where(item => ItemUsesBlueprint(item, _testMusketItem))
                .Distinct(ReferenceIdentityComparer.Instance)
                .ToArray();
            if (matches.Length == 0)
            {
                throw new InvalidOperationException(
                    "Equip exactly one Test Musket before preparing the Sprint 29 fixture.");
            }

            if (matches.Length != 1)
            {
                throw new InvalidOperationException(
                    "More than one distinct Test Musket is equipped; the Sprint 29 fixture target is ambiguous.");
            }

            return matches[0];
        }

        private List<VisibleFirearmEntry> CollectVisibleFirearmEntries(
            RuntimeContext runtime)
        {
            object inventory = RequireInventory(runtime.Player);
            var candidates = new List<object>();
            var seen = new HashSet<object>(ReferenceIdentityComparer.Instance);
            AddReferenceDistinct(
                CollectEquippedRuntimeWeaponItems(runtime),
                candidates,
                seen);
            AddReferenceDistinct(
                EnumerateInventoryItems(inventory),
                candidates,
                seen);

            var entries = new List<VisibleFirearmEntry>();
            foreach (object item in candidates)
            {
                if (!ItemUsesBlueprint(item, _testMusketItem))
                {
                    continue;
                }

                FirearmItemStateSnapshot snapshot;
                string reason;
                if (!_stateService.TryGetOrCreate(item, out snapshot, out reason))
                {
                    continue;
                }

                entries.Add(new VisibleFirearmEntry(item, snapshot));
            }

            return entries
                .OrderBy(entry => entry.Snapshot.Repository.EntryId)
                .ThenBy(entry => entry.Snapshot.Repository.RuntimeReferenceHash)
                .ToList();
        }

        private static VisibleFirearmEntry RequireMatchingEntry(
            IEnumerable<VisibleFirearmEntry> entries,
            object item)
        {
            VisibleFirearmEntry match = entries.SingleOrDefault(
                entry => ReferenceEquals(entry.Item, item));
            if (match == null)
            {
                throw new InvalidOperationException(
                    "An exact fixture firearm disappeared during state preparation.");
            }

            return match;
        }

        private static UnitDescriptor RequireConcreteDescriptor(RuntimeContext runtime)
        {
            UnitDescriptor descriptor = runtime.UnitDescriptor as UnitDescriptor;
            if (descriptor == null)
            {
                throw new InvalidOperationException(
                    "The selected unit did not expose a concrete Kingmaker UnitDescriptor.");
            }

            return descriptor;
        }

        private Ability EnsureRepairAbility(UnitDescriptor descriptor)
        {
            if (descriptor == null)
            {
                throw new ArgumentNullException("descriptor");
            }

            if (descriptor.Abilities == null)
            {
                throw new InvalidOperationException(
                    "The selected unit has no AbilityCollection.");
            }

            Ability ability = descriptor.Abilities.GetAbility(_repairAbility);
            if (ability == null)
            {
                descriptor.Abilities.AddFact(_repairAbility, null);
                ability = descriptor.Abilities.GetAbility(_repairAbility);
            }

            if (ability == null)
            {
                throw new InvalidOperationException(
                    "Kingmaker did not retain the Repair Test Musket ability after the grant.");
            }

            return ability;
        }

        private static long GetTotalFaults()
        {
            return CombatTraceRuntime.FaultCount +
                FirearmArmorClassRuntime.FaultCount +
                ReloadRuntimeDiagnostics.Faults +
                OverhaulRuntimeDiagnostics.Faults +
                RepairRuntimeDiagnostics.Faults +
                FirearmDischargeRuntimeDiagnostics.Faults +
                FirearmMisfireRuntimeDiagnostics.Faults +
                FirearmExplosionRuntimeDiagnostics.Faults +
                FirearmExplosionRuntimeDiagnostics.TargetFaults +
                FirearmStateTokenReconciliationDiagnostics.Conflicts +
                FirearmStateTokenReconciliationDiagnostics.Faults;
        }

        private static long GetTotalDuplicates()
        {
            return FirearmArmorClassRuntime.DuplicateCount +
                FirearmDischargeRuntimeDiagnostics.Duplicates +
                FirearmMisfireRuntimeDiagnostics.DuplicateRollAssignments +
                FirearmMisfireRuntimeDiagnostics.DuplicateEvaluations +
                FirearmExplosionRuntimeDiagnostics.Duplicates;
        }

        private sealed class VisibleFirearmEntry
        {
            internal VisibleFirearmEntry(
                object item,
                FirearmItemStateSnapshot snapshot)
            {
                Item = item ?? throw new ArgumentNullException("item");
                Snapshot = snapshot ?? throw new ArgumentNullException("snapshot");
            }

            internal object Item { get; private set; }

            internal FirearmItemStateSnapshot Snapshot { get; private set; }
        }
    }
}
