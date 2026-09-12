using System;
using System.IO;
using KingmakerGunslinger.Firearms;
using KingmakerGunslinger.Gunsmithing;

namespace KingmakerGunslinger.DomainTests
{
    /// <summary>
    /// Regression coverage for completed-rest firearm maintenance (mission
    /// Z-FIREARM-MAINTENANCE §4.2): only a genuine successful full rest with
    /// a participating capable gunsmith and one reusable kit restores carried
    /// Broken and Wrecked firearms to Normal, once per rest, without
    /// consuming anything; every non-completion route and missing-prereq
    /// combination performs no repair and reports honestly.
    /// </summary>
    internal static class CompletedRestMaintenanceTests
    {
        internal static void GenuineCompletionQualifies()
        {
            Assertions.Equal(CompletedRestMaintenanceStatus.MaintenanceDue,
                CompletedRestMaintenancePolicy.EvaluateRest(
                    true, false, false, true, true),
                "A successful, uninterrupted, non-skip-time rest with gunsmith and kit must run maintenance.");
        }

        internal static void NonCompletionNeverQualifies()
        {
            foreach (bool repairer in new[] { false, true })
            {
                foreach (bool kit in new[] { false, true })
                {
                    Assertions.Equal(
                        CompletedRestMaintenanceStatus.NotARestCompletion,
                        CompletedRestMaintenancePolicy.EvaluateRest(
                            false, false, false, repairer, kit),
                        "A rest that did not report success never maintains firearms.");
                    Assertions.Equal(
                        CompletedRestMaintenanceStatus.NotARestCompletion,
                        CompletedRestMaintenancePolicy.EvaluateRest(
                            true, true, false, repairer, kit),
                        "An encounter-interrupted rest termination never maintains firearms.");
                    Assertions.Equal(
                        CompletedRestMaintenanceStatus.NotARestCompletion,
                        CompletedRestMaintenancePolicy.EvaluateRest(
                            true, false, true, repairer, kit),
                        "Time skipping is not a full rest and never maintains firearms.");
                }
            }
        }

        internal static void MissingPrerequisitesDoNotRepair()
        {
            Assertions.Equal(
                CompletedRestMaintenanceStatus.NoCapableRepairer,
                CompletedRestMaintenancePolicy.EvaluateRest(
                    true, false, false, false, true),
                "Without a participating capable gunsmith nothing is repaired.");
            Assertions.Equal(
                CompletedRestMaintenanceStatus.NoRepairKit,
                CompletedRestMaintenancePolicy.EvaluateRest(
                    true, false, false, true, false),
                "Without a reusable kit nothing is repaired.");
        }

        internal static void ItemDecisionsFollowActualCondition()
        {
            Assertions.Equal(CompletedRestItemDecision.SkipNormal,
                CompletedRestMaintenancePolicy.EvaluateItem(
                    FirearmCondition.Normal),
                "A Normal firearm is untouched by rest maintenance.");
            Assertions.Equal(CompletedRestItemDecision.RestoreBroken,
                CompletedRestMaintenancePolicy.EvaluateItem(
                    FirearmCondition.Broken),
                "A Broken firearm is restored to Normal.");
            Assertions.Equal(CompletedRestItemDecision.RestoreWrecked,
                CompletedRestMaintenancePolicy.EvaluateItem(
                    FirearmCondition.Wrecked),
                "A Wrecked firearm is restored to Normal (staying unloaded).");
            bool threw = false;
            try
            {
                CompletedRestMaintenancePolicy.EvaluateItem(
                    (FirearmCondition)77);
            }
            catch (ArgumentOutOfRangeException)
            {
                threw = true;
            }
            Assertions.True(threw,
                "An unknown condition must fail closed instead of guessing a rest-restoration decision.");
        }

        internal static void BlockedReportRequiresDamage()
        {
            Assertions.False(
                CompletedRestMaintenancePolicy.ShouldReportBlocked(
                    CompletedRestMaintenanceStatus.NoRepairKit, 0),
                "No explanation is owed when nothing is damaged.");
            Assertions.False(
                CompletedRestMaintenancePolicy.ShouldReportBlocked(
                    CompletedRestMaintenanceStatus.NotARestCompletion, 3),
                "Non-completion terminations explain nothing.");
            Assertions.True(
                CompletedRestMaintenancePolicy.ShouldReportBlocked(
                    CompletedRestMaintenanceStatus.NoCapableRepairer, 1),
                "A genuine rest with damaged guns and no gunsmith explains itself once.");
            Assertions.True(
                CompletedRestMaintenancePolicy.ShouldReportBlocked(
                    CompletedRestMaintenanceStatus.NoRepairKit, 2),
                "A genuine rest with damaged guns and no kit explains itself once.");
        }

        internal static void SummariesAreHonest()
        {
            Assertions.True(
                CompletedRestMaintenancePolicy.DescribeRestored(0, 0, 0) == null,
                "A rest with nothing to do reports no maintenance.");
            Assertions.True(
                CompletedRestMaintenancePolicy.DescribeRestored(1, 0, 0)
                    .Contains("1 firearm") &&
                CompletedRestMaintenancePolicy.DescribeRestored(1, 0, 0)
                    .Contains("1 Broken"),
                "A single Broken restoration is reported precisely.");
            Assertions.True(
                CompletedRestMaintenancePolicy.DescribeRestored(2, 3, 0)
                    .Contains("5 firearms") &&
                CompletedRestMaintenancePolicy.DescribeRestored(2, 3, 0)
                    .Contains("2 Broken, 3 Wrecked"),
                "Aggregate restorations are reported precisely.");
            Assertions.True(
                CompletedRestMaintenancePolicy.DescribeRestored(0, 0, 2)
                    .Contains("every restoration failed") &&
                !CompletedRestMaintenancePolicy.DescribeRestored(0, 0, 2)
                    .Contains("restored 1"),
                "Failures are reported without inventing successes.");
            Assertions.True(
                CompletedRestMaintenancePolicy.DescribeRestored(1, 1, 1)
                    .Contains("2 restorations failed") ||
                CompletedRestMaintenancePolicy.DescribeRestored(1, 1, 1)
                    .Contains("1 restoration failed"),
                "Mixed outcomes name the failed count.");
            Assertions.True(
                CompletedRestMaintenancePolicy.DescribeBlocked(
                    CompletedRestMaintenanceStatus.NoRepairKit, 2)
                    .Contains("Gunsmith's Kit"),
                "The blocked explanation names the missing kit.");
            Assertions.True(
                CompletedRestMaintenancePolicy.DescribeBlocked(
                    CompletedRestMaintenanceStatus.NoCapableRepairer, 1)
                    .Contains("gunsmith"),
                "The blocked explanation names the missing gunsmith.");
            Assertions.True(
                CompletedRestMaintenancePolicy.DescribeBlocked(
                    CompletedRestMaintenanceStatus.MaintenanceDue, 4) == null,
                "No blocked explanation appears when maintenance can run.");
        }

        internal static void WiringUsesCompletionBoundaryAndOnceOnly()
        {
            string source = Read("src/KingmakerGunslinger/Gunsmithing",
                "CompletedRestMaintenancePatch.cs");
            Assertions.True(source.Contains(
                    "[HarmonyPatch(typeof(RestController), \"StopRestProcess\")]") &&
                source.Contains("status.RestSucceeded") &&
                source.Contains("!status.NightRandomEncounter") &&
                source.Contains("!status.SkipTime"),
                "Maintenance must run only at the genuine completed-rest termination boundary.");
            Assertions.True(source.Contains("ReferenceEquals(_processedStatus, status)"),
                "A repeated termination of the same rest session must never run maintenance twice.");
            Assertions.True(source.Contains(
                    "CompletedRestMaintenancePolicy.EvaluateRest(") &&
                source.Contains("CompletedRestMaintenanceStatus.MaintenanceDue"),
                "The runtime must delegate qualification to the pure policy.");
        }

        internal static void WiringScopeAndCapabilityAreEvidenceBased()
        {
            string source = Read("src/KingmakerGunslinger/Gunsmithing",
                "CompletedRestMaintenancePatch.cs");
            Assertions.True(source.Contains("game.Player.AllCharacters") &&
                source.Contains("HasFact(gunsmithFeature)") &&
                source.Contains("BlueprintBootstrap.GunslingerClass.Gunsmithing") &&
                source.Contains("FirearmMaintenanceCapability.IsLivingParticipant"),
                "Participation and capability must come from the native rest party filtered to living participants, holding the real Gunsmithing feature fact (review R5).");
            Assertions.True(source.Contains(
                    "new KingmakerRepairKitInventory(") &&
                source.Contains("kitInventory.Count() > 0"),
                "Kit access must reuse the shared-inventory adapter with at-least-one semantics.");
            Assertions.True(source.Contains("sharedInventory.Items") &&
                source.Contains("body.AllSlots") &&
                source.Contains("ReferenceEquals(existing, weapon)"),
                "Scope must be shared carried inventory plus participant equipment slots, deduplicated by concrete reference.");
            Assertions.True(source.Contains("FirearmRuntimeState.Service.Transition") &&
                source.Contains("FirearmStateMachine.Repair(beforeState)") &&
                !source.Contains("new ItemEntityWeapon"),
                "Restoration must reuse the exact-item guarded transition and the shared repair state transition, never a fresh item.");
        }

        internal static void WiringFailuresNeverBreakTheRest()
        {
            string source = Read("src/KingmakerGunslinger/Gunsmithing",
                "CompletedRestMaintenancePatch.cs");
            Assertions.True(source.Contains(
                    "catch (Exception exception)") &&
                source.Contains("the native rest completion is unaffected") &&
                source.Contains("failed++"),
                "A per-item or coordinator fault must be isolated from the native rest completion and reported honestly.");
            string crafting = Read("src/KingmakerGunslinger/Gunsmithing",
                "CraftingRestResetPatch.cs");
            Assertions.True(crafting.Contains("ApplyRest") &&
                crafting.Contains("UsedMarker"),
                "The ammunition crafting once-per-rest reset must remain independent.");
        }

        private static string Read(params string[] parts)
        {
            string path = Environment.CurrentDirectory;
            foreach (string part in parts) path = Path.Combine(path, part);
            return File.ReadAllText(path);
        }
    }
}
