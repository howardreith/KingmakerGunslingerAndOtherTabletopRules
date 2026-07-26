using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using KingmakerGunslinger.Persistence;
using Newtonsoft.Json;

namespace KingmakerGunslinger.Development
{
    [JsonObject(MemberSerialization.OptIn)]
    internal sealed class PersistenceEvidenceBuildFingerprintData
    {
        [JsonProperty(Order = 1)] public string ModInformationalVersion { get; set; }
        [JsonProperty(Order = 2)] public string ModAssemblySha256 { get; set; }
        [JsonProperty(Order = 3)] public string BlueprintManifestSha256 { get; set; }
        [JsonProperty(Order = 4)] public string GameVersion { get; set; }
        [JsonProperty(Order = 5)] public string GameAssemblyIdentity { get; set; }
        [JsonProperty(Order = 6)] public string GameAssemblySha256 { get; set; }
        [JsonProperty(Order = 7)] public string UnityModManagerAssemblyIdentity { get; set; }
        [JsonProperty(Order = 8)] public string UnityModManagerAssemblySha256 { get; set; }
        [JsonProperty(Order = 9)] public string HarmonyAssemblyIdentity { get; set; }
        [JsonProperty(Order = 10)] public string HarmonyAssemblySha256 { get; set; }

        internal string StableKey()
        {
            return string.Join("|", new[]
            {
                ModInformationalVersion ?? string.Empty,
                ModAssemblySha256 ?? string.Empty,
                BlueprintManifestSha256 ?? string.Empty,
                GameVersion ?? string.Empty,
                GameAssemblyIdentity ?? string.Empty,
                GameAssemblySha256 ?? string.Empty,
                UnityModManagerAssemblyIdentity ?? string.Empty,
                UnityModManagerAssemblySha256 ?? string.Empty,
                HarmonyAssemblyIdentity ?? string.Empty,
                HarmonyAssemblySha256 ?? string.Empty
            });
        }
    }

    [JsonObject(MemberSerialization.OptIn)]
    internal sealed class PersistenceFirearmEvidenceData
    {
        [JsonProperty(Order = 1)] public string RepositoryIdentity { get; set; }
        [JsonProperty(Order = 2)] public int RepositoryRevision { get; set; }
        [JsonProperty(Order = 3)] public string EngineItemId { get; set; }
        [JsonProperty(Order = 4)] public string RuntimeType { get; set; }
        [JsonProperty(Order = 5)] public string ItemBlueprintId { get; set; }
        [JsonProperty(Order = 6)] public string WeaponTypeId { get; set; }
        [JsonProperty(Order = 7)] public int LoadedRounds { get; set; }
        [JsonProperty(Order = 8)] public string LoadedAmmunitionId { get; set; }
        [JsonProperty(Order = 9)] public string Condition { get; set; }

        public override string ToString()
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "repository={0}; revision={1}; engineItemId={2}; runtimeType={3}; itemBlueprint={4}; weaponType={5}; rounds={6}; ammunition={7}; condition={8}",
                RepositoryIdentity,
                RepositoryRevision,
                EngineItemId,
                RuntimeType,
                ItemBlueprintId,
                WeaponTypeId,
                LoadedRounds,
                string.IsNullOrWhiteSpace(LoadedAmmunitionId) ? "<none>" : LoadedAmmunitionId,
                Condition);
        }
    }

    [JsonObject(MemberSerialization.OptIn)]
    internal sealed class PersistenceEvidenceSnapshotData
    {
        [JsonProperty(Order = 1)] public int SchemaVersion { get; set; }
        [JsonProperty(Order = 2)] public string CapturedAtUtc { get; set; }
        [JsonProperty(Order = 3)] public string SelectedUnitName { get; set; }
        [JsonProperty(Order = 4)] public int IdentityRecordCount { get; set; }
        [JsonProperty(Order = 5)] public int LegacyReferenceRecordCount { get; set; }
        [JsonProperty(Order = 6)] public long RepositoryEntriesCreated { get; set; }
        [JsonProperty(Order = 7)] public long RepositoryMutations { get; set; }
        [JsonProperty(Order = 8)] public long RepositoryRemovals { get; set; }
        [JsonProperty(Order = 9)] public string IdentityMigration { get; set; }
        [JsonProperty(Order = 10)] public string TokenMigration { get; set; }
        [JsonProperty(Order = 11)] public List<PersistenceFirearmEvidenceData> Firearms { get; set; }

        internal string ToCanonicalSummary()
        {
            string firearms = Firearms == null || Firearms.Count == 0
                ? "none"
                : string.Join(" || ", Firearms
                    .OrderBy(item => item.EngineItemId, StringComparer.Ordinal)
                    .ThenBy(item => item.RepositoryIdentity, StringComparer.Ordinal)
                    .Select(item => item.ToString())
                    .ToArray());
            return string.Format(
                CultureInfo.InvariantCulture,
                "captured={0}; selectedUnit={1}; identityRecords={2}; legacyReferenceRecords={3}; repositoryCreated={4}; mutations={5}; removals={6}; identityMigration=[{7}]; tokenMigration=[{8}]; firearms={9}",
                CapturedAtUtc,
                SelectedUnitName,
                IdentityRecordCount,
                LegacyReferenceRecordCount,
                RepositoryEntriesCreated,
                RepositoryMutations,
                RepositoryRemovals,
                IdentityMigration,
                TokenMigration,
                firearms);
        }
    }

    [JsonObject(MemberSerialization.OptIn)]
    internal sealed class PersistenceEvidenceObservationData
    {
        [JsonProperty(Order = 1)] public long Sequence { get; set; }
        [JsonProperty(Order = 2)] public string StepId { get; set; }
        [JsonProperty(Order = 3)] public string Status { get; set; }
        [JsonProperty(Order = 4)] public string ObservedAtUtc { get; set; }
        [JsonProperty(Order = 5)] public string RunId { get; set; }
        [JsonProperty(Order = 6)] public string Note { get; set; }
        [JsonProperty(Order = 7)] public string SaveBeforeSha256 { get; set; }
        [JsonProperty(Order = 8)] public string SaveAfterSha256 { get; set; }
        [JsonProperty(Order = 9)] public PersistenceEvidenceSnapshotData Before { get; set; }
        [JsonProperty(Order = 10)] public PersistenceEvidenceSnapshotData After { get; set; }

        internal PersistenceEvidenceObservation ToDomain()
        {
            PersistenceEvidenceStatus status;
            if (!Enum.TryParse(Status, false, out status) ||
                !Enum.IsDefined(typeof(PersistenceEvidenceStatus), status) ||
                !string.Equals(Status, status.ToString(), StringComparison.Ordinal))
            {
                throw new InvalidOperationException("Unknown persistence-evidence status: " + Status);
            }

            return new PersistenceEvidenceObservation(
                Sequence,
                StepId,
                status,
                ObservedAtUtc,
                RunId,
                Note,
                Before == null ? string.Empty : Before.ToCanonicalSummary(),
                After == null ? string.Empty : After.ToCanonicalSummary(),
                SaveBeforeSha256,
                SaveAfterSha256);
        }
    }

    [JsonObject(MemberSerialization.OptIn)]
    internal sealed class PersistenceEvidenceSessionData
    {
        internal const int CurrentSchemaVersion = 1;

        [JsonProperty(Order = 1)] public int SchemaVersion { get; set; }
        [JsonProperty(Order = 2)] public string SessionId { get; set; }
        [JsonProperty(Order = 3)] public string StartedAtUtc { get; set; }
        [JsonProperty(Order = 4)] public string UpdatedAtUtc { get; set; }
        [JsonProperty(Order = 5)] public int CurrentRunNumber { get; set; }
        [JsonProperty(Order = 6)] public int CurrentStepIndex { get; set; }
        [JsonProperty(Order = 7)] public PersistenceEvidenceBuildFingerprintData Build { get; set; }
        [JsonProperty(Order = 8)] public string PendingStepId { get; set; }
        [JsonProperty(Order = 9)] public string PendingNote { get; set; }
        [JsonProperty(Order = 10)] public string PendingSaveBeforeSha256 { get; set; }
        [JsonProperty(Order = 11)] public PersistenceEvidenceSnapshotData PendingBefore { get; set; }
        [JsonProperty(Order = 12)] public List<PersistenceEvidenceObservationData> Observations { get; set; }

        internal string CurrentRunId
        {
            get { return string.Format(CultureInfo.InvariantCulture, "run-{0:D3}", CurrentRunNumber); }
        }
    }

    internal sealed class PersistenceEvidenceCaptureResult
    {
        private PersistenceEvidenceCaptureResult(
            bool succeeded,
            string message,
            PersistenceEvidenceSnapshotData snapshot)
        {
            Succeeded = succeeded;
            Message = message ?? string.Empty;
            Snapshot = snapshot;
        }

        internal bool Succeeded { get; private set; }
        internal string Message { get; private set; }
        internal PersistenceEvidenceSnapshotData Snapshot { get; private set; }

        internal static PersistenceEvidenceCaptureResult Success(PersistenceEvidenceSnapshotData snapshot)
        {
            return new PersistenceEvidenceCaptureResult(true, "Captured persistence evidence snapshot.", snapshot);
        }

        internal static PersistenceEvidenceCaptureResult Failure(string message)
        {
            return new PersistenceEvidenceCaptureResult(false, message, null);
        }
    }
}
