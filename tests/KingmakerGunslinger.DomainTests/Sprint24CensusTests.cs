using System;
using System.IO;
using System.Linq;
using KingmakerGunslinger.RuntimeTesting;

namespace KingmakerGunslinger.DomainTests
{
    internal static class Sprint24CensusTests
    {
        internal static void TheCensusSelectionIsClosedAndExact()
        {
            Sprint24TheropodCensusPolicy.Validate();
            string[][] donors = Sprint24TheropodCensusPolicy.Donors;
            Assertions.Equal(2, donors.Length,
                "Only the recorded free-hands biped and the troll negative control are surveyed.");
            Assertions.True(donors.Select(value => value[0]).SequenceEqual(
                    new[] { "lizardfolk-free-hands", "troll-negative-control" }),
                "The closed donor order is stable and reviewable.");
            Assertions.Equal("a8cbd4cdb0feaf64eab1452257c3c971",
                donors[0][1], "The free-hands Lizardfolk blueprint is exact.");
            Assertions.Equal("d6db6ba3b2c459a48ba0730beef0bac8",
                donors[0][2], "The free-hands Lizardfolk prefab is exact.");
            Assertions.Equal(9,
                Sprint24TheropodCensusPolicy.FactAnchors.Length,
                "The exact fact audit is finite and includes the native bleed cleanup graph.");
            Assertions.True(Sprint24TheropodCensusPolicy
                    .MatchesCriticalCandidate("BleedingCriticalFeature") &&
                !Sprint24TheropodCensusPolicy
                    .MatchesCriticalCandidate("PowerfulChargeFeature"),
                "Critical candidate discovery is narrow and deterministic.");
        }

        internal static void TheCensusIsWiredAsSaveFreeMetadata()
        {
            const string scenario =
                "observe-expanded-summoning-sprint24-theropod-census";
            Assertions.Equal(scenario, RuntimeTestScenarioCatalog
                    .ObserveExpandedSummoningSprint24TheropodCensus,
                "The source catalog exposes the exact orchestrator name.");
            Assertions.True(RuntimeTestScenarioCatalog.IsAllowed(scenario),
                "The production request validator admits the bounded census.");

            string automation = File.ReadAllText(
                "scripts/RuntimeAutomation.Common.ps1");
            int at = automation.IndexOf("'" + scenario + "'",
                StringComparison.Ordinal);
            Assertions.True(at >= 0, "The runtime orchestrator has a descriptor.");
            string block = automation.Substring(at,
                Math.Min(420, automation.Length - at));
            Assertions.True(block.Contains("RequiresSaveName = $false") &&
                    block.Contains("RequiresManualInteraction = $false") &&
                    block.Contains("ReadinessBehavior = 'mod-load'"),
                "The census is a save-free, input-free mod-load observation.");

            string runner = File.ReadAllText(
                "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.cs");
            Assertions.True(runner.Contains(
                    "RunExpandedSummoningSprint24TheropodCensus()"),
                "The runtime runner dispatches the exact census.");
            string observer = File.ReadAllText(
                "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.ExpandedSummoningSprint24Census.cs");
            foreach (string required in new[] { "restHierarchy", "anatomy",
                "animationManagers", "colliders", "factAnchors",
                "criticalCandidates", "Sprint17ClipMetadata" })
                Assertions.True(observer.Contains(required),
                    "The observer captures required metadata: " + required);
            Assertions.False(observer.Contains("SpawnUnit("),
                "The detached census cannot spawn a campaign actor.");
        }
    }
}
