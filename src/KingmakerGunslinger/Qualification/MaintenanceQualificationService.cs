using System;
using System.Collections.Generic;
using KingmakerGunslinger.Firearms;

namespace KingmakerGunslinger.Qualification
{
    /// <summary>
    /// Pure deterministic evaluator for the accelerated unified maintenance loop.
    /// It recognizes fixture, Repair, and Reload checkpoints and validates exact-item
    /// isolation, reusable-tool and ammunition deltas, revisions, counters, faults,
    /// and duplicates.
    /// </summary>
    internal sealed class MaintenanceQualificationService
    {
        internal MaintenanceQualificationReport Evaluate(
            MaintenanceQualificationBaseline baseline,
            MaintenanceQualificationObservation observation)
        {
            if (baseline == null)
            {
                throw new ArgumentNullException("baseline");
            }

            if (observation == null)
            {
                throw new ArgumentNullException("observation");
            }

            MaintenanceQualificationStage stage = DetermineStage(observation.ExactState);
            var checks = new List<string>();
            bool passed = true;

            passed &= AddCheck(
                checks,
                "exactItem",
                string.Equals(
                    baseline.RepositoryIdentity,
                    observation.RepositoryIdentity,
                    StringComparison.Ordinal) &&
                baseline.RuntimeReferenceHash == observation.RuntimeReferenceHash);
            passed &= AddCheck(
                checks,
                "visibleItems",
                observation.VisibleFirearms == baseline.VisibleFirearms &&
                observation.VisibleFirearms >= 2);
            passed &= AddCheck(
                checks,
                "secondItem",
                string.Equals(
                    baseline.SecondRepositoryIdentity,
                    observation.SecondRepositoryIdentity,
                    StringComparison.Ordinal) &&
                baseline.SecondRuntimeReferenceHash ==
                    observation.SecondRuntimeReferenceHash &&
                baseline.SecondRevision == observation.SecondRevision &&
                baseline.SecondItemState == observation.SecondItemState &&
                observation.SecondItemState.IsEmpty &&
                observation.SecondItemState.Condition == FirearmCondition.Normal);
            passed &= AddCheck(
                checks,
                "faults",
                observation.TotalFaults == baseline.TotalFaults);
            passed &= AddCheck(
                checks,
                "duplicates",
                observation.TotalDuplicates == baseline.TotalDuplicates);

            switch (stage)
            {
                case MaintenanceQualificationStage.FixtureReady:
                    passed &= AddCheck(checks, "revision", observation.Revision == baseline.Revision);
                    passed &= AddCheck(checks, "tools", observation.GunsmithKits == baseline.GunsmithKits);
                    passed &= AddCheck(checks, "powder", observation.BlackPowder == baseline.BlackPowder);
                    passed &= AddCheck(checks, "lead", observation.LeadBalls == baseline.LeadBalls);
                    passed &= AddCheck(checks, "repair", observation.RepairCompleted == baseline.RepairCompleted);
                    passed &= AddCheck(checks, "reload", observation.ReloadCompleted == baseline.ReloadCompleted);
                    break;

                case MaintenanceQualificationStage.RepairPassed:
                    passed &= AddCheck(checks, "revision", observation.Revision == baseline.Revision + 1);
                    passed &= AddCheck(checks, "tools", observation.GunsmithKits == baseline.GunsmithKits);
                    passed &= AddCheck(checks, "powder", observation.BlackPowder == baseline.BlackPowder);
                    passed &= AddCheck(checks, "lead", observation.LeadBalls == baseline.LeadBalls);
                    passed &= AddCheck(checks, "repair", observation.RepairCompleted == baseline.RepairCompleted + 1);
                    passed &= AddCheck(checks, "reload", observation.ReloadCompleted == baseline.ReloadCompleted);
                    break;

                case MaintenanceQualificationStage.MaintenanceLoopPassed:
                    passed &= AddCheck(checks, "revision", observation.Revision == baseline.Revision + 2);
                    passed &= AddCheck(checks, "tools", observation.GunsmithKits == baseline.GunsmithKits);
                    passed &= AddCheck(checks, "powder", observation.BlackPowder == baseline.BlackPowder - 1);
                    passed &= AddCheck(checks, "lead", observation.LeadBalls == baseline.LeadBalls - 1);
                    passed &= AddCheck(checks, "repair", observation.RepairCompleted == baseline.RepairCompleted + 1);
                    passed &= AddCheck(checks, "reload", observation.ReloadCompleted == baseline.ReloadCompleted + 1);
                    break;

                default:
                    passed &= AddCheck(checks, "state", false);
                    break;
            }

            return new MaintenanceQualificationReport(stage, passed, checks.ToArray());
        }

        private static MaintenanceQualificationStage DetermineStage(FirearmState state)
        {
            if (state == null)
            {
                return MaintenanceQualificationStage.Failed;
            }

            if (state.IsEmpty && state.Condition == FirearmCondition.Wrecked)
            {
                return MaintenanceQualificationStage.FixtureReady;
            }

            if (state.IsEmpty && state.Condition == FirearmCondition.Normal)
            {
                return MaintenanceQualificationStage.RepairPassed;
            }

            if (state.LoadedRounds == 1 && state.Condition == FirearmCondition.Normal)
            {
                return MaintenanceQualificationStage.MaintenanceLoopPassed;
            }

            return MaintenanceQualificationStage.Failed;
        }

        private static bool AddCheck(
            ICollection<string> checks,
            string name,
            bool passed)
        {
            checks.Add(name + "=" + (passed ? "PASS" : "FAIL"));
            return passed;
        }
    }
}
