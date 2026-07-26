using System;

namespace KingmakerGunslinger.Persistence
{
    /// <summary>
    /// Immutable, engine-independent input for the Sprint 17 I01/I02 runtime
    /// qualification checks. Negative counts represent unavailable measurements.
    /// </summary>
    internal sealed class PersistenceRuntimePreflightProbeData
    {
        internal PersistenceRuntimePreflightProbeData(
            bool bootstrapInspectionSucceeded,
            bool bootstrapInitialized,
            int bootstrapInitializationCount,
            int registeredBlueprintCount,
            int expectedRegisteredBlueprintCount,
            string bootstrapDetail,
            bool identityInspectionSucceeded,
            int identityMemberCount,
            bool identityMemberReadable,
            string identityMemberValueType,
            string identityDetail)
        {
            if (bootstrapInitializationCount < -1)
            {
                throw new ArgumentOutOfRangeException("bootstrapInitializationCount");
            }

            if (registeredBlueprintCount < -1)
            {
                throw new ArgumentOutOfRangeException("registeredBlueprintCount");
            }

            if (expectedRegisteredBlueprintCount <= 0)
            {
                throw new ArgumentOutOfRangeException("expectedRegisteredBlueprintCount");
            }

            if (identityMemberCount < -1)
            {
                throw new ArgumentOutOfRangeException("identityMemberCount");
            }

            BootstrapInspectionSucceeded = bootstrapInspectionSucceeded;
            BootstrapInitialized = bootstrapInitialized;
            BootstrapInitializationCount = bootstrapInitializationCount;
            RegisteredBlueprintCount = registeredBlueprintCount;
            ExpectedRegisteredBlueprintCount = expectedRegisteredBlueprintCount;
            BootstrapDetail = Normalize(bootstrapDetail);
            IdentityInspectionSucceeded = identityInspectionSucceeded;
            IdentityMemberCount = identityMemberCount;
            IdentityMemberReadable = identityMemberReadable;
            IdentityMemberValueType = Normalize(identityMemberValueType);
            IdentityDetail = Normalize(identityDetail);
        }

        internal bool BootstrapInspectionSucceeded { get; private set; }

        internal bool BootstrapInitialized { get; private set; }

        internal int BootstrapInitializationCount { get; private set; }

        internal int RegisteredBlueprintCount { get; private set; }

        internal int ExpectedRegisteredBlueprintCount { get; private set; }

        internal string BootstrapDetail { get; private set; }

        internal bool IdentityInspectionSucceeded { get; private set; }

        internal int IdentityMemberCount { get; private set; }

        internal bool IdentityMemberReadable { get; private set; }

        internal string IdentityMemberValueType { get; private set; }

        internal string IdentityDetail { get; private set; }

        private static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }
    }
}
