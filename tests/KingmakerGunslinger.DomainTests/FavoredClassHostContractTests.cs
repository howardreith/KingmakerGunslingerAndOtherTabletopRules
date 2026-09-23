using System;
using KingmakerGunslinger.FavoredClass;

namespace KingmakerGunslinger.DomainTests
{
    internal static class FavoredClassHostContractTests
    {
        private const string GunslingerClassGuid = "abca4797366d4df0831a418eee39069a";

        // The host's per-class identities are CotW MergeIds of the class GUID;
        // values checked against the installed host's loaded_blueprints dump.
        internal static void HostIdentitiesDeriveFromTheClassGuid()
        {
            Assertions.Equal("cbe4e1941a5f0fa8229919016a6b283b",
                FavoredClassHostContract.ExpectedProgressionGuid(GunslingerClassGuid),
                "FavoredClassKMG_Gunslinger_ClassProgression.");
            Assertions.Equal("5ffbec509d160a812695bef00c968c9b",
                FavoredClassHostContract.ExpectedBonusSelectionGuid(GunslingerClassGuid),
                "FavoredClassKMG_Gunslinger_ClassFeatureSelecion.");
            const string alchemist = "0937bec61c0dabc468428f496580c721";
            Assertions.Equal("691918c5303fe99cc9c1d7c6e1d2e980",
                FavoredClassHostContract.ExpectedProgressionGuid(alchemist),
                "FavoredClassAlchemistClassProgression.");
            Assertions.Equal("fd061501b776ecb5cdcd7037872f4d20",
                FavoredClassHostContract.ExpectedBonusSelectionGuid(alchemist),
                "FavoredClassAlchemistClassFeatureSelecion.");
            Assertions.Equal(GunslingerClassGuid, FavoredClassHostContract.MergeIds(
                FavoredClassHostContract.ProgressionSeed,
                FavoredClassHostContract.ExpectedProgressionGuid(GunslingerClassGuid)),
                "MergeIds is an involution.");
            try
            {
                FavoredClassHostContract.MergeIds("abc", GunslingerClassGuid);
            }
            catch (FormatException)
            {
                return;
            }
            throw new InvalidOperationException("Malformed identities must be rejected.");
        }

        internal static void ExactQualifiedBinaryIsAccepted()
        {
            FavoredClassHostDecision decision = FavoredClassHostContract.EvaluateBinary(Qualified());
            Assertions.Equal(FavoredClassHostState.Ready, decision.State, decision.ToString());
        }

        // H01/H02: absent, disabled or unloaded hosts are diagnosed specifically.
        internal static void AbsentDisabledAndUnloadedHostsAreInactive()
        {
            Assertions.Equal(FavoredClassHostState.Absent,
                FavoredClassHostContract.EvaluateBinary(null).State, "Null observation.");
            FavoredClassHostObservation absent = Qualified();
            absent.HostModInstalled = false;
            Assertions.Equal(FavoredClassHostState.Absent,
                FavoredClassHostContract.EvaluateBinary(absent).State, "Not installed.");
            FavoredClassHostObservation disabled = Qualified();
            disabled.HostModEnabled = false;
            Assertions.Equal(FavoredClassHostState.Disabled,
                FavoredClassHostContract.EvaluateBinary(disabled).State, "Disabled.");
            FavoredClassHostObservation unloaded = Qualified();
            unloaded.HostAssemblyLoaded = false;
            Assertions.Equal(FavoredClassHostState.Absent,
                FavoredClassHostContract.EvaluateBinary(unloaded).State, "Not loaded.");
        }

        // H05: a matching version string never qualifies a different binary.
        internal static void SameVersionDifferentBinaryIsRejected()
        {
            AssertUnsupported(o => o.HostFileSha256 = new string('0', 64), "binary-sha256");
            AssertUnsupported(o => o.HostModuleVersionId = Guid.Empty.ToString("D"), "binary-mvid");
            AssertUnsupported(o => o.HostAssemblyName = "ZFavoredClassFork", "assembly-name");
            AssertUnsupported(o => o.CallOfTheWildAssemblyLoaded = false, "dependency-not-loaded");
            AssertUnsupported(o => o.CallOfTheWildFileSha256 = new string('1', 64), "dependency-sha256");
            AssertUnsupported(o => o.CallOfTheWildModuleVersionId = Guid.Empty.ToString("D"),
                "dependency-mvid");
            AssertUnsupported(o => o.MissingMember = "ZFavoredClass.Core.class_guid_bonus_selection_map",
                "required-member");
            AssertUnsupported(o => o.MethodIlSha256[FavoredClassHostContract.PrerequisiteRaceCheckKey] =
                new string('2', 64), "unverified-behavior:" +
                FavoredClassHostContract.PrerequisiteRaceCheckKey);
            AssertUnsupported(o => o.MethodIlSha256.Remove(FavoredClassHostContract.CoreLoadKey),
                "unverified-behavior:" + FavoredClassHostContract.CoreLoadKey);
            FavoredClassHostObservation relabeled = Qualified();
            relabeled.HostModVersion = "9.9.9";
            Assertions.Equal(FavoredClassHostState.Ready,
                FavoredClassHostContract.EvaluateBinary(relabeled).State,
                "The version label is diagnostic only; exact bytes decide.");
        }

        // H02/H04: incomplete initialization and a missing Gunslinger entry.
        internal static void ReadinessRequiresCompleteInitializationAndGunslinger()
        {
            FavoredClassHostDecision binary = FavoredClassHostContract.EvaluateBinary(Qualified());
            Assertions.Equal(FavoredClassHostState.Ready,
                FavoredClassHostContract.EvaluateReadiness(binary, Ready()).State, "Ready host.");
            AssertReadiness(r => r.LibraryAssigned = false,
                FavoredClassHostState.IncompleteInitialization, "library-unassigned");
            AssertReadiness(r => r.CoreLoadCompleted = false,
                FavoredClassHostState.IncompleteInitialization, "core-load-incomplete");
            AssertReadiness(r => r.FavoredClassSelectionPresent = false,
                FavoredClassHostState.IncompleteInitialization, "favored-class-selection-missing");
            AssertReadiness(r => r.GenericSkillLeavesPresent = false,
                FavoredClassHostState.IncompleteInitialization, "generic-rewards-missing");
            AssertReadiness(r => { r.GunslingerProgressionGuid = null; r.GunslingerBonusSelectionGuid = null; },
                FavoredClassHostState.GunslingerMissing, "gunslinger-not-scanned");
            AssertReadiness(r => r.GunslingerBonusSelectionGuid = "fd061501b776ecb5cdcd7037872f4d20",
                FavoredClassHostState.GunslingerMissing, "gunslinger-identity-mismatch");
            AssertReadiness(r => r.GunslingerProgressionOffered = false,
                FavoredClassHostState.GunslingerMissing, "gunslinger-progression-not-offered");
            AssertReadiness(r => r.GunslingerProgressionLevels = 10,
                FavoredClassHostState.GunslingerMissing, "gunslinger-progression-shape");
            FavoredClassHostObservation unsupported = Qualified();
            unsupported.HostFileSha256 = new string('3', 64);
            Assertions.Equal(FavoredClassHostState.UnsupportedBinary,
                FavoredClassHostContract.EvaluateReadiness(
                    FavoredClassHostContract.EvaluateBinary(unsupported), Ready()).State,
                "Readiness never overrides a failed binary gate.");
        }

        private static FavoredClassHostObservation Qualified()
        {
            FavoredClassHostObservation observed = new FavoredClassHostObservation
            {
                HostModInstalled = true,
                HostModEnabled = true,
                HostModVersion = FavoredClassHostContract.VerifiedHostModVersion,
                HostAssemblyLoaded = true,
                HostAssemblyName = FavoredClassHostContract.HostAssemblyName,
                HostModuleVersionId = FavoredClassHostContract.VerifiedHostModuleVersionId,
                HostFileSha256 = FavoredClassHostContract.VerifiedHostFileSha256.ToUpperInvariant(),
                CallOfTheWildAssemblyLoaded = true,
                CallOfTheWildModuleVersionId = FavoredClassHostContract.VerifiedCallOfTheWildModuleVersionId,
                CallOfTheWildFileSha256 = FavoredClassHostContract.VerifiedCallOfTheWildFileSha256,
            };
            foreach (string key in FavoredClassHostContract.FingerprintKeys)
                observed.MethodIlSha256[key] = FavoredClassHostContract.VerifiedIlSha256(key);
            return observed;
        }

        private static FavoredClassHostReadinessObservation Ready()
        {
            return new FavoredClassHostReadinessObservation
            {
                LibraryAssigned = true,
                CoreLoadCompleted = true,
                FavoredClassSelectionPresent = true,
                GunslingerClassGuid = GunslingerClassGuid,
                GunslingerProgressionGuid = "cbe4e1941a5f0fa8229919016a6b283b",
                GunslingerBonusSelectionGuid = "5ffbec509d160a812695bef00c968c9b",
                GunslingerProgressionOffered = true,
                GunslingerProgressionLevels = 20,
                GunslingerLevelsGrantBonusSelection = true,
                GenericHitPointLeafPresent = true,
                GenericSkillLeavesPresent = true,
            };
        }

        private static void AssertUnsupported(Action<FavoredClassHostObservation> mutate, string reason)
        {
            FavoredClassHostObservation observed = Qualified();
            mutate(observed);
            FavoredClassHostDecision decision = FavoredClassHostContract.EvaluateBinary(observed);
            Assertions.Equal(FavoredClassHostState.UnsupportedBinary, decision.State, decision.ToString());
            Assertions.Equal(reason, decision.Reason, "Reason for " + decision);
        }

        private static void AssertReadiness(Action<FavoredClassHostReadinessObservation> mutate,
            FavoredClassHostState state, string reason)
        {
            FavoredClassHostReadinessObservation observed = Ready();
            mutate(observed);
            FavoredClassHostDecision decision = FavoredClassHostContract.EvaluateReadiness(
                FavoredClassHostContract.EvaluateBinary(Qualified()), observed);
            Assertions.Equal(state, decision.State, decision.ToString());
            Assertions.Equal(reason, decision.Reason, "Reason for " + decision);
        }
    }
}
