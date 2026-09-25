using System;
using System.IO;
using System.Linq;
using KingmakerGunslinger.FavoredClass;

namespace KingmakerGunslinger.DomainTests
{
    /// <summary>
    /// Review finding 1: owned numerical effects need the enabled integration
    /// and a committed publication of the exact host, which starts inactive
    /// and is cleared for every other host state.
    /// </summary>
    internal static class FavoredClassHostGateTests
    {
        private static string Source(params string[] parts)
        {
            return File.ReadAllText(Path.Combine(new[] { Environment.CurrentDirectory, "src",
                "KingmakerGunslinger" }.Concat(parts).ToArray()));
        }

        internal static void ActivationStartsInactiveAndFollowsTransitions()
        {
            var activation = new FavoredClassHostActivation();
            Assertions.False(activation.IsActive, "The host starts inactive.");
            activation.Activate("published");
            Assertions.True(activation.IsActive, "A committed publication activates.");
            Assertions.Equal("published", activation.Reason, "The reason is recorded.");
            foreach (string state in new[] { "host-Absent", "host-Disabled", "host-UnsupportedBinary",
                "host-IncompleteInitialization", "integration-disabled", "publication-failed", "rolled-back" })
            {
                activation.Activate("published");
                activation.Deactivate(state);
                Assertions.False(activation.IsActive, state + " deactivates.");
                Assertions.Equal(state, activation.Reason, state + " is recorded.");
            }
            // A retry after a failure activates only through a new commit.
            activation.Deactivate("publication-failed");
            activation.Activate("republished");
            Assertions.True(activation.IsActive, "A fresh commit reactivates.");
        }

        internal static void MechanicsNeedIntegrationAndPublishedHost()
        {
            Assertions.True(FavoredClassHostActivation.MechanicsEnabled(true, true),
                "Enabled integration and published host: effects apply.");
            Assertions.False(FavoredClassHostActivation.MechanicsEnabled(true, false),
                "Without the published host the effects are zero.");
            Assertions.False(FavoredClassHostActivation.MechanicsEnabled(false, true),
                "A disabled integration suppresses the effects.");
            Assertions.False(FavoredClassHostActivation.MechanicsEnabled(false, false), "Neither: zero.");
        }

        // Only the coordinator's commit (or the qualification re-publication)
        // activates; every other branch and every rollback deactivates.
        internal static void OnlyACommitActivatesTheHost()
        {
            string runtime = Source("FavoredClass", "FavoredClassRuntime.cs");
            Assertions.True(runtime.Contains(
                "return FavoredClassHostActivation.MechanicsEnabled(integration, Activation.IsActive);"),
                "MechanicsEnabled requires the integration and the host activation.");
            string coordinator = Source("FavoredClass", "FavoredClassIntegrationCoordinator.cs");
            foreach (string token in new[] {
                "FavoredClassRuntime.DeactivateHost(\"resolving\");",
                "FavoredClassRuntime.DeactivateHost(\"registration-failed\");",
                "FavoredClassRuntime.DeactivateHost(\"host-\" + host.Decision.State);",
                "FavoredClassRuntime.DeactivateHost(\"integration-disabled\");",
                "FavoredClassRuntime.DeactivateHost(\"publication-failed\");",
                "FavoredClassRuntime.ActivateHost(\"published\");",
                "FavoredClassRuntime.ActivateHost(\"republished\");" })
                Assertions.True(coordinator.Contains(token), "The coordinator lacks: " + token);
            Assertions.Equal(2, coordinator.Split(new[] { "FavoredClassRuntime.ActivateHost(" },
                StringSplitOptions.None).Length - 1, "Exactly two activation sites.");
            int commit = coordinator.IndexOf("publication.Commit();", StringComparison.Ordinal);
            int activate = coordinator.IndexOf("FavoredClassRuntime.ActivateHost(\"published\");",
                StringComparison.Ordinal);
            Assertions.True(commit > 0 && activate > commit, "Activation follows the commit.");
            string publication = Source("FavoredClass", "FavoredClassPublication.cs");
            Assertions.True(publication.Contains("FavoredClassRuntime.DeactivateHost(\"rolled-back\");"),
                "Every rollback deactivates the host.");
        }
    }
}
