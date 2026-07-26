using System;
using System.Collections.Generic;
using System.Globalization;

namespace KingmakerGunslinger.Persistence
{
    /// <summary>
    /// Pure evaluator for the first two persistence-matrix rows. It never marks an
    /// unavailable measurement as PASS and never inspects Kingmaker directly.
    /// </summary>
    internal static class PersistenceRuntimePreflightEvaluator
    {
        private const string GuidTypeName = "System.Guid";
        private const string StringTypeName = "System.String";

        internal static PersistenceRuntimePreflightReport Evaluate(
            PersistenceRuntimePreflightProbeData probe)
        {
            if (probe == null)
            {
                throw new ArgumentNullException("probe");
            }

            return new PersistenceRuntimePreflightReport(new[]
            {
                EvaluateBootstrap(probe),
                EvaluateIdentity(probe)
            });
        }

        private static PersistenceRuntimePreflightCheck EvaluateBootstrap(
            PersistenceRuntimePreflightProbeData probe)
        {
            if (!probe.BootstrapInspectionSucceeded ||
                probe.BootstrapInitializationCount < 0 ||
                probe.RegisteredBlueprintCount < 0)
            {
                return new PersistenceRuntimePreflightCheck(
                    "I01",
                    PersistenceEvidenceStatus.Blocked,
                    JoinDetail(
                        "Bootstrap inspection was unavailable.",
                        probe.BootstrapDetail));
            }

            bool passed = probe.BootstrapInitialized &&
                probe.BootstrapInitializationCount == 1 &&
                probe.RegisteredBlueprintCount == probe.ExpectedRegisteredBlueprintCount;
            string detail = string.Format(
                CultureInfo.InvariantCulture,
                "initialized={0}; initializationCount={1}; registeredBlueprints={2}; expectedBlueprints={3}{4}",
                probe.BootstrapInitialized,
                probe.BootstrapInitializationCount,
                probe.RegisteredBlueprintCount,
                probe.ExpectedRegisteredBlueprintCount,
                Suffix(probe.BootstrapDetail));
            return new PersistenceRuntimePreflightCheck(
                "I01",
                passed ? PersistenceEvidenceStatus.Pass : PersistenceEvidenceStatus.Fail,
                detail);
        }

        private static PersistenceRuntimePreflightCheck EvaluateIdentity(
            PersistenceRuntimePreflightProbeData probe)
        {
            if (!probe.IdentityInspectionSucceeded || probe.IdentityMemberCount < 0)
            {
                return new PersistenceRuntimePreflightCheck(
                    "I02",
                    PersistenceEvidenceStatus.Blocked,
                    JoinDetail(
                        "ItemEntityWeapon.UniqueId inspection was unavailable.",
                        probe.IdentityDetail));
            }

            bool acceptedType = string.Equals(
                probe.IdentityMemberValueType,
                GuidTypeName,
                StringComparison.Ordinal) ||
                string.Equals(
                    probe.IdentityMemberValueType,
                    StringTypeName,
                    StringComparison.Ordinal);
            bool passed = probe.IdentityMemberCount == 1 &&
                probe.IdentityMemberReadable &&
                acceptedType;
            string detail = string.Format(
                CultureInfo.InvariantCulture,
                "matchingMembers={0}; readable={1}; valueType={2}; acceptedTypes={3},{4}{5}",
                probe.IdentityMemberCount,
                probe.IdentityMemberReadable,
                string.IsNullOrWhiteSpace(probe.IdentityMemberValueType)
                    ? "<unavailable>"
                    : probe.IdentityMemberValueType,
                GuidTypeName,
                StringTypeName,
                Suffix(probe.IdentityDetail));
            return new PersistenceRuntimePreflightCheck(
                "I02",
                passed ? PersistenceEvidenceStatus.Pass : PersistenceEvidenceStatus.Fail,
                detail);
        }

        private static string JoinDetail(string first, string second)
        {
            return string.IsNullOrWhiteSpace(second)
                ? first
                : first + " " + second.Trim();
        }

        private static string Suffix(string value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? string.Empty
                : "; detail=" + value.Trim();
        }
    }
}
