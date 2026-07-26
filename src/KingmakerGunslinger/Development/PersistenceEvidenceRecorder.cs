using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using Kingmaker.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Persistence;
using Newtonsoft.Json;
using UnityEngine;
using UnityModManagerNet;

namespace KingmakerGunslinger.Development
{
    /// <summary>
    /// External diagnostic recorder for the Sprint 17 qualification of the identity-vault persistence candidate. Evidence files
    /// are never read by firearm mechanics and are not a persistence carrier.
    /// </summary>
    internal sealed class PersistenceEvidenceRecorder
    {
        private const string CurrentFileName = "current-session.json";
        private const string SessionPrefix = "persistence-evidence-";
        private readonly object _gate = new object();
        private readonly ModContext _context;
        private readonly string _evidenceDirectory;
        private readonly PersistenceEvidenceBuildFingerprintData _build;
        private PersistenceEvidenceSessionData _session;
        private string _status;

        internal PersistenceEvidenceRecorder(ModContext context)
        {
            _context = context ?? throw new ArgumentNullException("context");
            string assemblyDirectory = Path.GetDirectoryName(context.Assembly.Location);
            if (string.IsNullOrWhiteSpace(assemblyDirectory))
            {
                throw new InvalidOperationException("The installed mod assembly directory could not be resolved.");
            }

            _evidenceDirectory = Path.Combine(assemblyDirectory, "evidence");
            _build = CaptureBuildFingerprint(context);
            ValidateBuildFingerprint(_build);
            _status = "No evidence session is active.";
            TryResumeCompatibleSession();
        }

        internal string Status
        {
            get { lock (_gate) { return _status; } }
        }

        internal bool HasSession
        {
            get { lock (_gate) { return _session != null; } }
        }

        internal string EvidenceDirectory
        {
            get { return _evidenceDirectory; }
        }

        internal PersistenceMatrixStepDefinition CurrentStep
        {
            get
            {
                lock (_gate)
                {
                    if (_session == null)
                    {
                        return null;
                    }

                    return PersistenceMatrixCatalog.All[_session.CurrentStepIndex];
                }
            }
        }

        internal string CurrentRunId
        {
            get { lock (_gate) { return _session == null ? "<none>" : _session.CurrentRunId; } }
        }

        internal bool HasPendingBefore
        {
            get { lock (_gate) { return _session != null && _session.PendingBefore != null; } }
        }

        internal PersistenceEvidenceEvaluation Evaluation
        {
            get
            {
                lock (_gate)
                {
                    return PersistenceEvidenceEvaluator.Evaluate(
                        _session == null
                            ? Enumerable.Empty<PersistenceEvidenceObservation>()
                            : _session.Observations.Select(observation => observation.ToDomain()));
                }
            }
        }

        internal string StartNewSession()
        {
            lock (_gate)
            {
                string now = UtcNow();
                _session = new PersistenceEvidenceSessionData
                {
                    SchemaVersion = PersistenceEvidenceSessionData.CurrentSchemaVersion,
                    SessionId = Guid.NewGuid().ToString("D").ToLowerInvariant(),
                    StartedAtUtc = now,
                    UpdatedAtUtc = now,
                    CurrentRunNumber = 1,
                    CurrentStepIndex = 0,
                    Build = CloneBuild(_build),
                    PendingStepId = string.Empty,
                    PendingNote = string.Empty,
                    PendingSaveBeforeSha256 = string.Empty,
                    PendingBefore = null,
                    Observations = new List<PersistenceEvidenceObservationData>()
                };
                PersistLocked();
                _status = "Started evidence session " + _session.SessionId + ".";
                return _status;
            }
        }

        internal string BeginNextRun()
        {
            lock (_gate)
            {
                RequireSessionLocked();
                if (_session.PendingBefore != null)
                {
                    throw new InvalidOperationException(
                        "Complete or discard the pending BEFORE snapshot before beginning another run.");
                }

                checked { _session.CurrentRunNumber++; }
                _session.CurrentStepIndex = 0;
                TouchLocked();
                PersistLocked();
                _status = "Started " + _session.CurrentRunId + "; current step reset to I01.";
                return _status;
            }
        }

        internal string MoveStep(int delta)
        {
            lock (_gate)
            {
                RequireSessionLocked();
                if (_session.PendingBefore != null)
                {
                    throw new InvalidOperationException(
                        "Complete or discard the pending BEFORE snapshot before changing steps.");
                }

                int target = _session.CurrentStepIndex + delta;
                target = Math.Max(0, Math.Min(PersistenceMatrixCatalog.All.Count - 1, target));
                _session.CurrentStepIndex = target;
                TouchLocked();
                PersistLocked();
                _status = "Selected " + PersistenceMatrixCatalog.All[target].Id + ".";
                return _status;
            }
        }

        internal string RecordTrustedRuntimePreflight()
        {
            PersistenceRuntimePreflightReport report = PersistenceRuntimePreflightProbe.Capture();
            lock (_gate)
            {
                RequireSessionLocked();
                if (_session.PendingBefore != null)
                {
                    throw new InvalidOperationException(
                        "Complete or discard the pending BEFORE snapshot before recording the runtime preflight.");
                }

                foreach (PersistenceRuntimePreflightCheck check in report.Checks)
                {
                    long sequence = _session.Observations.Count == 0
                        ? 1
                        : checked(_session.Observations.Max(existingObservation => existingObservation.Sequence) + 1);
                    var observation = new PersistenceEvidenceObservationData
                    {
                        Sequence = sequence,
                        StepId = check.StepId,
                        Status = check.Status.ToString(),
                        ObservedAtUtc = UtcNow(),
                        RunId = _session.CurrentRunId,
                        Note = "Trusted Sprint 17 runtime preflight: " + check.Detail,
                        SaveBeforeSha256 = string.Empty,
                        SaveAfterSha256 = string.Empty,
                        Before = null,
                        After = null
                    };
                    observation.ToDomain();
                    _session.Observations.Add(observation);
                }

                _session.CurrentStepIndex = Math.Max(
                    _session.CurrentStepIndex,
                    2);
                TouchLocked();
                PersistLocked();
                PersistenceEvidenceEvaluation evaluation = EvaluateLocked();
                _status = string.Format(
                    CultureInfo.InvariantCulture,
                    "Recorded trusted I01/I02 runtime preflight in {0}: {1}; {2}.",
                    _session.CurrentRunId,
                    report,
                    evaluation);
                return _status;
            }
        }

        internal string CaptureBefore(string note, string saveBeforeSha256)
        {
            string expectedStepId;
            lock (_gate)
            {
                RequireSessionLocked();
                if (_session.PendingBefore != null)
                {
                    throw new InvalidOperationException(
                        "Complete or discard the existing BEFORE snapshot before capturing another one.");
                }

                expectedStepId = PersistenceMatrixCatalog.All[_session.CurrentStepIndex].Id;
            }

            PersistenceEvidenceCaptureResult capture = DevelopmentControls.CapturePersistenceEvidenceSnapshot();
            if (!capture.Succeeded)
            {
                throw new InvalidOperationException(capture.Message);
            }

            lock (_gate)
            {
                RequireSessionLocked();
                PersistenceMatrixStepDefinition step = PersistenceMatrixCatalog.All[_session.CurrentStepIndex];
                if (!string.Equals(step.Id, expectedStepId, StringComparison.Ordinal))
                {
                    throw new InvalidOperationException(
                        "The selected persistence-matrix row changed while the BEFORE snapshot was being captured.");
                }

                _session.PendingStepId = step.Id;
                _session.PendingNote = Normalize(note);
                _session.PendingSaveBeforeSha256 = NormalizeHash(saveBeforeSha256);
                _session.PendingBefore = capture.Snapshot;
                TouchLocked();
                PersistLocked();
                _status = "Captured BEFORE evidence for " + step.Id + ". Perform the lifecycle operation, then record the result.";
                return _status;
            }
        }

        internal string CompleteCurrent(
            PersistenceEvidenceStatus status,
            string note,
            string saveAfterSha256)
        {
            if (!Enum.IsDefined(typeof(PersistenceEvidenceStatus), status))
            {
                throw new ArgumentOutOfRangeException("status");
            }

            PersistenceEvidenceCaptureResult capture = DevelopmentControls.CapturePersistenceEvidenceSnapshot();

            lock (_gate)
            {
                RequireSessionLocked();
                PersistenceMatrixStepDefinition step = PersistenceMatrixCatalog.All[_session.CurrentStepIndex];
                bool hasMatchingBefore = _session.PendingBefore != null &&
                    string.Equals(_session.PendingStepId, step.Id, StringComparison.Ordinal);

                if (status == PersistenceEvidenceStatus.Pass && !hasMatchingBefore)
                {
                    throw new InvalidOperationException(
                        "A PASS requires a BEFORE snapshot for the current step.");
                }

                if (status == PersistenceEvidenceStatus.Pass && !capture.Succeeded)
                {
                    throw new InvalidOperationException(
                        "A PASS requires a valid AFTER snapshot: " + capture.Message);
                }

                string finalNote = JoinNotes(hasMatchingBefore ? _session.PendingNote : string.Empty, note);
                if (!capture.Succeeded)
                {
                    finalNote = JoinNotes(
                        finalNote,
                        "AFTER snapshot unavailable: " + capture.Message);
                }

                if (!hasMatchingBefore && status != PersistenceEvidenceStatus.Blocked &&
                    status != PersistenceEvidenceStatus.Fail)
                {
                    throw new InvalidOperationException(
                        "Capture a BEFORE snapshot for the current step before recording its result.");
                }

                long sequence = _session.Observations.Count == 0
                    ? 1
                    : checked(_session.Observations.Max(existingObservation => existingObservation.Sequence) + 1);
                var observation = new PersistenceEvidenceObservationData
                {
                    Sequence = sequence,
                    StepId = step.Id,
                    Status = status.ToString(),
                    ObservedAtUtc = UtcNow(),
                    RunId = _session.CurrentRunId,
                    Note = finalNote,
                    SaveBeforeSha256 = hasMatchingBefore
                        ? _session.PendingSaveBeforeSha256
                        : string.Empty,
                    SaveAfterSha256 = NormalizeHash(saveAfterSha256),
                    Before = hasMatchingBefore ? _session.PendingBefore : null,
                    After = capture.Succeeded ? capture.Snapshot : null
                };
                observation.ToDomain();
                _session.Observations.Add(observation);
                ClearPendingLocked();
                if (_session.CurrentStepIndex < PersistenceMatrixCatalog.All.Count - 1)
                {
                    _session.CurrentStepIndex++;
                }

                TouchLocked();
                PersistLocked();
                PersistenceEvidenceEvaluation evaluation = EvaluateLocked();
                _status = string.Format(
                    CultureInfo.InvariantCulture,
                    "Recorded {0} for {1} in {2}; before={3}; after={4}; {5}.",
                    status,
                    step.Id,
                    observation.RunId,
                    observation.Before == null ? "missing" : "captured",
                    observation.After == null ? "missing" : "captured",
                    evaluation);
                return _status;
            }
        }

        internal string DiscardPendingBefore()
        {
            lock (_gate)
            {
                RequireSessionLocked();
                ClearPendingLocked();
                TouchLocked();
                PersistLocked();
                _status = "Discarded the pending BEFORE snapshot.";
                return _status;
            }
        }

        internal string ExportMarkdown()
        {
            lock (_gate)
            {
                RequireSessionLocked();
                Directory.CreateDirectory(_evidenceDirectory);
                string path = Path.Combine(
                    _evidenceDirectory,
                    SessionPrefix + _session.SessionId + ".md");
                AtomicWrite(path, BuildMarkdownLocked());
                _status = "Exported persistence evidence summary: " + path;
                return _status;
            }
        }

        private void TryResumeCompatibleSession()
        {
            lock (_gate)
            {
                string path = Path.Combine(_evidenceDirectory, CurrentFileName);
                if (!File.Exists(path))
                {
                    return;
                }

                try
                {
                    PersistenceEvidenceSessionData candidate = JsonConvert.DeserializeObject<PersistenceEvidenceSessionData>(
                        File.ReadAllText(path, Encoding.UTF8));
                    ValidateSession(candidate);
                    if (!string.Equals(candidate.Build.StableKey(), _build.StableKey(), StringComparison.Ordinal))
                    {
                        _status = "An evidence session exists for a different compiled build or game fingerprint; it was not resumed.";
                        return;
                    }

                    _session = candidate;
                    _status = "Resumed compatible evidence session " + candidate.SessionId + ".";
                }
                catch (Exception exception)
                {
                    _status = "Could not resume the current evidence session: " + exception.GetType().Name + ": " + exception.Message;
                    _context.Logger.Failure(
                        "persistence-evidence",
                        "session.resume-failed",
                        "The external evidence file was preserved and ignored.",
                        exception);
                }
            }
        }

        private void PersistLocked()
        {
            ValidateSession(_session);
            Directory.CreateDirectory(_evidenceDirectory);
            string json = JsonConvert.SerializeObject(_session, Formatting.Indented) + Environment.NewLine;
            AtomicWrite(Path.Combine(_evidenceDirectory, CurrentFileName), json);
            AtomicWrite(
                Path.Combine(_evidenceDirectory, SessionPrefix + _session.SessionId + ".json"),
                json);
        }

        private string BuildMarkdownLocked()
        {
            PersistenceEvidenceEvaluation evaluation = EvaluateLocked();
            var builder = new StringBuilder();
            builder.AppendLine("# Kingmaker Gunslinger persistence evidence");
            builder.AppendLine();
            builder.AppendLine("- Session: `" + _session.SessionId + "`");
            builder.AppendLine("- Build: `" + _session.Build.ModInformationalVersion + "`");
            builder.AppendLine("- Game: `" + _session.Build.GameVersion + "`");
            builder.AppendLine("- Mod DLL SHA-256: `" + _session.Build.ModAssemblySha256 + "`");
            builder.AppendLine("- Blueprint manifest SHA-256: `" + _session.Build.BlueprintManifestSha256 + "`");
            builder.AppendLine("- Game assembly: `" + EscapeCode(_session.Build.GameAssemblyIdentity) + "`");
            builder.AppendLine("- Game assembly SHA-256: `" + _session.Build.GameAssemblySha256 + "`");
            builder.AppendLine("- UMM assembly: `" + EscapeCode(_session.Build.UnityModManagerAssemblyIdentity) + "`");
            builder.AppendLine("- UMM assembly SHA-256: `" + _session.Build.UnityModManagerAssemblySha256 + "`");
            builder.AppendLine("- Harmony assembly: `" + EscapeCode(_session.Build.HarmonyAssemblyIdentity) + "`");
            builder.AppendLine("- Harmony assembly SHA-256: `" + _session.Build.HarmonyAssemblySha256 + "`");
            builder.AppendLine("- Decision: **" + evaluation.Decision + "**");
            builder.AppendLine("- Evaluation: `" + evaluation + "`");
            builder.AppendLine("- Blocking rows: `" + (evaluation.BlockingStepIds.Count == 0 ? "<none>" : string.Join(",", evaluation.BlockingStepIds.ToArray())) + "`");
            builder.AppendLine("- Warnings: `" + (evaluation.Warnings.Count == 0 ? "<none>" : EscapeCode(string.Join(" | ", evaluation.Warnings.ToArray()))) + "`");
            builder.AppendLine();
            builder.AppendLine("| ID | Severity | Latest | Pass runs | Operation | Required result |");
            builder.AppendLine("|---|---|---|---:|---|---|");
            foreach (PersistenceMatrixStepDefinition step in PersistenceMatrixCatalog.All)
            {
                List<PersistenceEvidenceObservationData> observations = _session.Observations
                    .Where(observation => string.Equals(observation.StepId, step.Id, StringComparison.Ordinal))
                    .OrderBy(observation => observation.Sequence)
                    .ToList();
                string latest = observations.Count == 0 ? "UNOBSERVED" : observations[observations.Count - 1].Status;
                int passRuns = observations
                    .Where(observation => string.Equals(observation.Status, PersistenceEvidenceStatus.Pass.ToString(), StringComparison.Ordinal))
                    .Select(observation => observation.RunId)
                    .Distinct(StringComparer.Ordinal)
                    .Count();
                builder.AppendLine(string.Format(
                    CultureInfo.InvariantCulture,
                    "| {0} | {1} | {2} | {3} | {4} | {5} |",
                    step.Id,
                    step.Severity,
                    latest,
                    passRuns,
                    EscapeMarkdown(step.Operation),
                    EscapeMarkdown(step.RequiredResult)));
            }

            builder.AppendLine();
            builder.AppendLine("## Observations");
            foreach (PersistenceEvidenceObservationData observation in _session.Observations.OrderBy(value => value.Sequence))
            {
                builder.AppendLine();
                builder.AppendLine("### " + observation.Sequence + ". " + observation.StepId + " — " + observation.Status);
                builder.AppendLine();
                builder.AppendLine("- Run: `" + observation.RunId + "`");
                builder.AppendLine("- Observed: `" + observation.ObservedAtUtc + "`");
                builder.AppendLine("- Save before SHA-256: `" + Display(observation.SaveBeforeSha256) + "`");
                builder.AppendLine("- Save after SHA-256: `" + Display(observation.SaveAfterSha256) + "`");
                builder.AppendLine("- Note: " + EscapeMarkdown(Display(observation.Note)));
                builder.AppendLine("- Before: `" + EscapeCode(observation.Before == null ? "<missing>" : observation.Before.ToCanonicalSummary()) + "`");
                builder.AppendLine("- After: `" + EscapeCode(observation.After == null ? "<missing>" : observation.After.ToCanonicalSummary()) + "`");
            }

            builder.AppendLine();
            builder.AppendLine("This report is external diagnostic evidence only. It is never used as firearm save data.");
            return builder.ToString();
        }

        private PersistenceEvidenceEvaluation EvaluateLocked()
        {
            return PersistenceEvidenceEvaluator.Evaluate(
                _session.Observations.Select(observation => observation.ToDomain()));
        }

        private static PersistenceEvidenceBuildFingerprintData CaptureBuildFingerprint(ModContext context)
        {
            Assembly modAssembly = context.Assembly;
            Assembly gameAssembly = typeof(BlueprintScriptableObject).Assembly;
            Assembly unityModManagerAssembly = typeof(UnityModManager).Assembly;
            Assembly harmonyAssembly = context.Harmony == null
                ? null
                : context.Harmony.GetType().Assembly;
            string modDirectory = Path.GetDirectoryName(modAssembly.Location);
            string manifestPath = Path.Combine(modDirectory ?? string.Empty, "blueprints", "blueprints.json");
            return new PersistenceEvidenceBuildFingerprintData
            {
                ModInformationalVersion = ReadInformationalVersion(modAssembly),
                ModAssemblySha256 = HashFile(modAssembly.Location),
                BlueprintManifestSha256 = HashFile(manifestPath),
                GameVersion = Application.version ?? string.Empty,
                GameAssemblyIdentity = gameAssembly.FullName ?? string.Empty,
                GameAssemblySha256 = HashFile(gameAssembly.Location),
                UnityModManagerAssemblyIdentity = unityModManagerAssembly.FullName ?? string.Empty,
                UnityModManagerAssemblySha256 = HashFile(unityModManagerAssembly.Location),
                HarmonyAssemblyIdentity = harmonyAssembly == null
                    ? string.Empty
                    : harmonyAssembly.FullName ?? string.Empty,
                HarmonyAssemblySha256 = harmonyAssembly == null
                    ? string.Empty
                    : HashFile(harmonyAssembly.Location)
            };
        }

        private static void ValidateSession(PersistenceEvidenceSessionData session)
        {
            if (session == null)
            {
                throw new InvalidOperationException("The persistence-evidence session is null.");
            }

            if (session.SchemaVersion != PersistenceEvidenceSessionData.CurrentSchemaVersion)
            {
                throw new NotSupportedException("Unsupported persistence-evidence session schema.");
            }

            Guid parsed;
            if (!Guid.TryParseExact(session.SessionId, "D", out parsed) || parsed == Guid.Empty)
            {
                throw new InvalidOperationException("The persistence-evidence session ID is invalid.");
            }

            ValidateBuildFingerprint(session.Build);
            RequireUtcTimestamp(session.StartedAtUtc, "startedAtUtc");
            RequireUtcTimestamp(session.UpdatedAtUtc, "updatedAtUtc");

            if (session.CurrentRunNumber <= 0)
            {
                throw new InvalidOperationException("The persistence-evidence run number is invalid.");
            }

            if (session.CurrentStepIndex < 0 || session.CurrentStepIndex >= PersistenceMatrixCatalog.All.Count)
            {
                throw new InvalidOperationException("The persistence-evidence current-step index is invalid.");
            }

            if (session.Observations == null)
            {
                throw new InvalidOperationException("The persistence-evidence observation collection is missing.");
            }

            PersistenceEvidenceEvaluator.Evaluate(session.Observations.Select(observation => observation.ToDomain()));
            if (session.PendingBefore == null)
            {
                session.PendingStepId = string.Empty;
                session.PendingNote = string.Empty;
                session.PendingSaveBeforeSha256 = string.Empty;
            }
            else
            {
                PersistenceMatrixCatalog.Require(session.PendingStepId);
            }
        }


        private static void ValidateBuildFingerprint(PersistenceEvidenceBuildFingerprintData build)
        {
            if (build == null)
            {
                throw new InvalidOperationException("The persistence-evidence build fingerprint is missing.");
            }

            RequireText(build.ModInformationalVersion, "modInformationalVersion");
            RequireSha256(build.ModAssemblySha256, "modAssemblySha256");
            RequireSha256(build.BlueprintManifestSha256, "blueprintManifestSha256");
            RequireText(build.GameVersion, "gameVersion");
            RequireText(build.GameAssemblyIdentity, "gameAssemblyIdentity");
            RequireSha256(build.GameAssemblySha256, "gameAssemblySha256");
            RequireText(build.UnityModManagerAssemblyIdentity, "unityModManagerAssemblyIdentity");
            RequireSha256(build.UnityModManagerAssemblySha256, "unityModManagerAssemblySha256");
            RequireText(build.HarmonyAssemblyIdentity, "harmonyAssemblyIdentity");
            RequireSha256(build.HarmonyAssemblySha256, "harmonyAssemblySha256");
        }

        private static void RequireText(string value, string name)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new InvalidOperationException(
                    "The persistence-evidence build fingerprint is missing " + name + ".");
            }
        }

        private static void RequireSha256(string value, string name)
        {
            if (string.IsNullOrWhiteSpace(value) || value.Length != 64 || value.Any(character =>
                !((character >= '0' && character <= '9') || (character >= 'a' && character <= 'f'))))
            {
                throw new InvalidOperationException(
                    "The persistence-evidence build fingerprint contains an invalid " + name + ".");
            }
        }

        private static void RequireUtcTimestamp(string value, string name)
        {
            DateTimeOffset parsed;
            if (string.IsNullOrWhiteSpace(value) ||
                !DateTimeOffset.TryParse(
                    value,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.RoundtripKind,
                    out parsed) ||
                parsed.Offset != TimeSpan.Zero)
            {
                throw new InvalidOperationException(
                    "The persistence-evidence session contains an invalid UTC " + name + ".");
            }
        }

        private static void AtomicWrite(string path, string content)
        {
            string temp = path + ".tmp";
            File.WriteAllText(temp, content, new UTF8Encoding(false));
            if (File.Exists(path))
            {
                File.Replace(temp, path, null);
            }
            else
            {
                File.Move(temp, path);
            }
        }

        private void RequireSessionLocked()
        {
            if (_session == null)
            {
                throw new InvalidOperationException("Start a persistence-evidence session first.");
            }
        }

        private void ClearPendingLocked()
        {
            _session.PendingStepId = string.Empty;
            _session.PendingNote = string.Empty;
            _session.PendingSaveBeforeSha256 = string.Empty;
            _session.PendingBefore = null;
        }

        private void TouchLocked()
        {
            _session.UpdatedAtUtc = UtcNow();
        }

        private static string UtcNow()
        {
            return DateTimeOffset.UtcNow.ToString("O", CultureInfo.InvariantCulture);
        }

        private static string JoinNotes(string left, string right)
        {
            string first = Normalize(left);
            string second = Normalize(right);
            if (first.Length == 0) return second;
            if (second.Length == 0) return first;
            return first + " | " + second;
        }

        private static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }

        private static string NormalizeHash(string value)
        {
            string normalized = Normalize(value).ToLowerInvariant();
            if (normalized.Length == 0)
            {
                return string.Empty;
            }

            if (normalized.Length != 64 || normalized.Any(character =>
                !((character >= '0' && character <= '9') || (character >= 'a' && character <= 'f'))))
            {
                throw new ArgumentException("A save SHA-256 must contain exactly 64 hexadecimal characters.");
            }

            return normalized;
        }

        private static string ReadInformationalVersion(Assembly assembly)
        {
            var attribute = assembly.GetCustomAttributes(typeof(AssemblyInformationalVersionAttribute), false)
                .OfType<AssemblyInformationalVersionAttribute>()
                .SingleOrDefault();
            return attribute == null ? assembly.GetName().Version.ToString() : attribute.InformationalVersion;
        }

        private static string HashFile(string path)
        {
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
            {
                return string.Empty;
            }

            using (FileStream stream = File.OpenRead(path))
            using (SHA256 algorithm = SHA256.Create())
            {
                return string.Concat(algorithm.ComputeHash(stream).Select(value => value.ToString("x2", CultureInfo.InvariantCulture)));
            }
        }

        private static PersistenceEvidenceBuildFingerprintData CloneBuild(PersistenceEvidenceBuildFingerprintData value)
        {
            return new PersistenceEvidenceBuildFingerprintData
            {
                ModInformationalVersion = value.ModInformationalVersion,
                ModAssemblySha256 = value.ModAssemblySha256,
                BlueprintManifestSha256 = value.BlueprintManifestSha256,
                GameVersion = value.GameVersion,
                GameAssemblyIdentity = value.GameAssemblyIdentity,
                GameAssemblySha256 = value.GameAssemblySha256,
                UnityModManagerAssemblyIdentity = value.UnityModManagerAssemblyIdentity,
                UnityModManagerAssemblySha256 = value.UnityModManagerAssemblySha256,
                HarmonyAssemblyIdentity = value.HarmonyAssemblyIdentity,
                HarmonyAssemblySha256 = value.HarmonyAssemblySha256
            };
        }

        private static string EscapeMarkdown(string value)
        {
            return (value ?? string.Empty).Replace("|", "\\|").Replace("\r", " ").Replace("\n", " ");
        }

        private static string EscapeCode(string value)
        {
            return (value ?? string.Empty).Replace("`", "'").Replace("\r", " ").Replace("\n", " ");
        }

        private static string Display(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? "<not recorded>" : value;
        }
    }

    internal static class PersistenceEvidenceRuntime
    {
        private static readonly object Gate = new object();
        private static PersistenceEvidenceRecorder _recorder;

        internal static PersistenceEvidenceRecorder Recorder
        {
            get
            {
                lock (Gate)
                {
                    if (_recorder == null)
                    {
                        throw new InvalidOperationException("The persistence-evidence recorder is not configured.");
                    }

                    return _recorder;
                }
            }
        }

        internal static bool IsConfigured
        {
            get { lock (Gate) { return _recorder != null; } }
        }

        internal static void Configure(ModContext context)
        {
            lock (Gate)
            {
                if (_recorder != null)
                {
                    throw new InvalidOperationException("The persistence-evidence recorder is already configured.");
                }

                _recorder = new PersistenceEvidenceRecorder(context);
            }
        }
    }
}
