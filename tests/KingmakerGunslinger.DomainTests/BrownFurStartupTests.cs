using System;
using System.Linq;
using KingmakerGunslinger.BrownFur;
using KingmakerGunslinger.FeatureModules;

namespace KingmakerGunslinger.DomainTests
{
    internal static class BrownFurStartupTests
    {
        internal static void UpdateCotwLibrary()
        {
            var gate = new BrownFurStartupGate();
            Assertions.False(gate.TryBegin(false, false, false), "Early UMM update must stay pending.");
            Assertions.Equal(BrownFurStartupState.WaitingForCotw, gate.State, "Early update is not an incompatibility.");
            Assertions.False(gate.TryBegin(true, true, false), "CotW completion cannot use an uncommitted package library.");
            Assertions.Equal(BrownFurStartupState.WaitingForLibrary, gate.State, "CotW callback must retain pending work.");
            Assertions.True(gate.TryBegin(true, false, true), "Library commit must synchronously admit registration.");
            gate.Complete();
            Assertions.False(gate.TryBegin(true, true, true), "Registration must remain exactly once.");
        }

        internal static void CotwLibrary()
        {
            var gate = new BrownFurStartupGate();
            Assertions.False(gate.TryBegin(true, true, false), "CotW first must wait for package commit.");
            Assertions.True(gate.TryBegin(true, false, true), "Package commit must finish the deferred work.");
            gate.Complete();
            for (int i = 0; i < 10; i++)
                Assertions.False(gate.TryBegin(true, i % 2 == 0, true), "Repeated lifecycle notifications must not reregister.");
        }

        internal static void LibraryCotw()
        {
            var gate = new BrownFurStartupGate();
            Assertions.False(gate.TryBegin(false, false, true), "Ready package alone is insufficient.");
            Assertions.True(gate.TryBegin(true, true, true), "CotW creation must finish before returning, not on a later frame.");
            gate.Complete();
            Assertions.False(gate.TryBegin(true, false, true), "No duplicate initialization.");
        }

        internal static void UnsupportedRemainsRejected()
        {
            var gate = new BrownFurStartupGate();
            Assertions.True(gate.TryBegin(false, true, true), "A completed but missing Arcanist must reach the real contract rejection.");
            var invalid = new CotwArcanistContractCandidate {
                CotwDetected = true, CotwActive = true, AssemblyIdentityResolved = true,
                ArcanistClassResolved = false };
            var decision = CotwArcanistContractPolicy.Evaluate(invalid);
            Assertions.False(decision.IsCompatible, "Do not weaken the compatibility decision.");
            Assertions.Equal("arcanist-class", decision.FailedCheck, "Preserve the actual rejection.");
            gate.Reject();
            Assertions.False(gate.TryBegin(true, true, true), "A genuine rejection is not a retry loop.");
        }

        internal static void ReentrancyAndDeferredCommit()
        {
            var gate = new BrownFurStartupGate();
            Assertions.True(gate.TryBegin(true, true, true), "Begin reconciliation.");
            Assertions.False(gate.TryBegin(true, true, true), "Reentrant notification must not register twice.");
            gate.AwaitLibrary();
            Assertions.False(gate.TryBegin(true, false, false), "Lost readiness must stay pending.");
            Assertions.True(gate.TryBegin(true, false, true), "A later real ready boundary can retry.");
        }

        internal static void MissingGuidAndPendingStatus()
        {
            var identity = BrownFurIdentityCatalog.All.Single(value => value.Guid == "16c06d016437be9e9e6dac6211ff30a5");
            Assertions.Equal("KMG.BrownFur.PowerfulChange.Strength.Activatable", identity.Symbol, "Missing identity changed.");
            Assertions.Equal("BlueprintActivatableAbility", identity.PlannedType, "Missing identity must retain its serialized type.");
            Assertions.Equal(25, BrownFurIdentityCatalog.All.Count, "All original identities remain required.");
            var status = new BrownFurFeatureStatus(BrownFurDependencyAvailability.Pending, false, "awaiting lifecycle");
            Assertions.True(status.DependencyStatus.StartsWith("Pending", StringComparison.Ordinal), "Pending must not look incompatible or absent.");
        }
    }
}
