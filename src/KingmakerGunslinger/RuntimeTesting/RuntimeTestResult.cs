using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Diagnostics;
using System.Threading;
using Newtonsoft.Json;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal static class RuntimeTestStatuses
    {
        internal const string Pass = "PASS";
        internal const string Fail = "FAIL";
        internal const string Ambiguous = "AMBIGUOUS";
        internal const string Error = "ERROR";
        internal const string Timeout = "TIMEOUT";

        internal static bool IsValid(string status)
        {
            return status == Pass || status == Fail || status == Ambiguous ||
                status == Error || status == Timeout;
        }
    }

    internal sealed class RuntimeTestAssertion
    {
        [JsonProperty("name", Order = 1)] public string Name { get; set; }
        [JsonProperty("expected", Order = 2)] public string Expected { get; set; }
        [JsonProperty("observed", Order = 3)] public string Observed { get; set; }
        [JsonProperty("status", Order = 4)] public string Status { get; set; }
        [JsonProperty("evidence", Order = 5)] public string Evidence { get; set; }
    }

    internal sealed class RuntimeTestResult
    {
        [JsonProperty("schemaVersion", Order = 1)] public int SchemaVersion { get; set; }
        [JsonProperty("runId", Order = 2)] public string RunId { get; set; }
        [JsonProperty("scenario", Order = 3)] public string Scenario { get; set; }
        [JsonProperty("status", Order = 4)] public string Status { get; set; }
        [JsonProperty("loadedModVersion", Order = 5)] public string LoadedModVersion { get; set; }
        [JsonProperty("runtimeIdentity", Order = 6)] public string RuntimeIdentity { get; set; }
        [JsonProperty("gitCommit", Order = 7)] public string GitCommit { get; set; }
        [JsonProperty("gameVersion", Order = 8)] public string GameVersion { get; set; }
        [JsonProperty("startUtc", Order = 9)] public string StartUtc { get; set; }
        [JsonProperty("endUtc", Order = 10)] public string EndUtc { get; set; }
        [JsonProperty("durationMilliseconds", Order = 11)] public long DurationMilliseconds { get; set; }
        [JsonProperty("assertions", Order = 12)] public List<RuntimeTestAssertion> Assertions { get; set; }
        [JsonProperty("diagnostics", Order = 13)] public List<string> Diagnostics { get; set; }
        [JsonProperty("warnings", Order = 14)] public List<string> Warnings { get; set; }
        [JsonProperty("exceptionSummary", Order = 15)] public string ExceptionSummary { get; set; }
        [JsonProperty("evidenceFiles", Order = 16)] public List<string> EvidenceFiles { get; set; }
        [JsonProperty("automaticExitRequested", Order = 17)] public bool AutomaticExitRequested { get; set; }
        [JsonProperty("automaticExitInitiated", Order = 18)] public bool AutomaticExitInitiated { get; set; }
        [JsonProperty("saveLoadObservation", Order = 19, NullValueHandling = NullValueHandling.Ignore)]
        public SaveLoadObservationEvidence SaveLoadObservation { get; set; }
    }

    internal sealed class SaveLoadObservationEvent
    {
        [JsonProperty("runId", Order = 1)] public string RunId { get; set; }
        [JsonProperty("sequence", Order = 2)] public int Sequence { get; set; }
        [JsonProperty("elapsedMilliseconds", Order = 3)] public long ElapsedMilliseconds { get; set; }
        [JsonProperty("utc", Order = 4)] public string Utc { get; set; }
        [JsonProperty("eventName", Order = 5)] public string Kind { get; set; }
        [JsonProperty("declaringType", Order = 5)] public string DeclaringType { get; set; }
        [JsonProperty("methodSignature", Order = 6)] public string MethodSignature { get; set; }
        [JsonProperty("argumentTypes", Order = 7)] public List<string> ArgumentTypes { get; set; }
        [JsonProperty("managedThreadId", Order = 8)] public int ManagedThreadId { get; set; }
        [JsonProperty("displayName", Order = 9)] public string DisplayName { get; set; }
        [JsonProperty("safeSaveIdentifier", Order = 10)] public string SafeSaveIdentifier { get; set; }
        [JsonProperty("detail", Order = 12)] public string Detail { get; set; }
        [JsonProperty("exception", Order = 13)] public string Exception { get; set; }
    }

    internal sealed class RuntimeReadyMarker
    {
        [JsonProperty("schemaVersion", Order = 1)] public int SchemaVersion { get; set; }
        [JsonProperty("runId", Order = 2)] public string RunId { get; set; }
        [JsonProperty("scenario", Order = 3)] public string Scenario { get; set; }
        [JsonProperty("loadedModVersion", Order = 4)] public string LoadedModVersion { get; set; }
        [JsonProperty("runtimeIdentity", Order = 5)] public string RuntimeIdentity { get; set; }
        [JsonProperty("readinessTimestampUtc", Order = 6)] public string ReadinessTimestampUtc { get; set; }
        [JsonProperty("installedObservationHookIdentifiers", Order = 7)]
        public List<string> InstalledObservationHookIdentifiers { get; set; }
        [JsonProperty("processId", Order = 8)] public int ProcessId { get; set; }
    }

    internal sealed class RuntimeObservationTraceWriter
    {
        private readonly string _runId;
        private readonly string _directory;
        private readonly Stopwatch _elapsed;
        private readonly List<SaveLoadObservationEvent> _events =
            new List<SaveLoadObservationEvent>();

        internal RuntimeObservationTraceWriter(string runId, string directory, Stopwatch elapsed)
        {
            _runId = runId;
            _directory = directory;
            _elapsed = elapsed;
        }

        internal void Record(SaveLoadObservationEvent value)
        {
            value.RunId = _runId;
            value.Sequence = _events.Count + 1;
            if (string.IsNullOrWhiteSpace(value.Utc))
                value.Utc = DateTime.UtcNow.ToString("o");
            value.ElapsedMilliseconds = _elapsed.ElapsedMilliseconds;
            value.Exception = value.Exception ?? string.Empty;
            _events.Add(value);
            Flush();
        }

        internal void Record(string eventName, string detail, Exception exception = null)
        {
            Record(new SaveLoadObservationEvent
            {
                Kind = eventName,
                ManagedThreadId = Thread.CurrentThread.ManagedThreadId,
                DeclaringType = string.Empty,
                MethodSignature = string.Empty,
                ArgumentTypes = new List<string>(),
                DisplayName = string.Empty,
                SafeSaveIdentifier = string.Empty,
                Detail = detail ?? string.Empty,
                Exception = exception == null ? string.Empty :
                    exception.GetType().FullName + ": " + exception.Message
            });
        }

        internal void WriteReady(RuntimeReadyMarker marker)
        {
            RuntimeTestResultWriter.WriteAtomic(
                Path.Combine(_directory, "runtime-ready.json"),
                JsonConvert.SerializeObject(marker, Formatting.Indented) + Environment.NewLine);
        }

        private void Flush()
        {
            RuntimeTestResultWriter.WriteAtomic(
                Path.Combine(_directory, "runtime-events.json"),
                JsonConvert.SerializeObject(new
                {
                    schemaVersion = 1,
                    runId = _runId,
                    events = _events
                }, Formatting.Indented) + Environment.NewLine);
        }
    }

    internal sealed class SaveLoadObservationEvidence
    {
        [JsonProperty("saveManagerType", Order = 1)] public string SaveManagerType { get; set; }
        [JsonProperty("saveDescriptorType", Order = 2)] public string SaveDescriptorType { get; set; }
        [JsonProperty("acceptedSaveName", Order = 3)] public string AcceptedSaveName { get; set; }
        [JsonProperty("completionCallbackObserved", Order = 4)] public bool CompletionCallbackObserved { get; set; }
        [JsonProperty("gameLoadedStateObserved", Order = 5)] public bool GameLoadedStateObserved { get; set; }
        [JsonProperty("stableFingerprint", Order = 6)] public string StableFingerprint { get; set; }
        [JsonProperty("saveWritingApiObserved", Order = 7)] public bool SaveWritingApiObserved { get; set; }
        [JsonProperty("observationPatchesRemoved", Order = 8)] public bool ObservationPatchesRemoved { get; set; }
        [JsonProperty("loadStartUtc", Order = 9)] public string LoadStartUtc { get; set; }
        [JsonProperty("loadCompletionUtc", Order = 10)] public string LoadCompletionUtc { get; set; }
        [JsonProperty("initialGameState", Order = 11)] public string InitialGameState { get; set; }
        [JsonProperty("stableGameState", Order = 12)] public string StableGameState { get; set; }
        [JsonProperty("gameThreadManagedId", Order = 13)] public int GameThreadManagedId { get; set; }
        [JsonProperty("allCallbacksOnGameThread", Order = 14)] public bool AllCallbacksOnGameThread { get; set; }
        [JsonProperty("events", Order = 15)] public List<SaveLoadObservationEvent> Events { get; set; }
    }

    internal static class RuntimeTestResultWriter
    {
        internal static void Write(RuntimeTestResult result, string evidenceDirectory)
        {
            if (result == null) throw new ArgumentNullException("result");
            if (!RuntimeTestStatuses.IsValid(result.Status))
                throw new InvalidOperationException("Unknown runtime-test status.");
            string summaryPath = Path.Combine(evidenceDirectory, "runtime-summary.txt");
            string resultPath = Path.Combine(evidenceDirectory, "runtime-result.json");
            string readyPath = Path.Combine(evidenceDirectory, "runtime-ready.json");
            string eventsPath = Path.Combine(evidenceDirectory, "runtime-events.json");
            WriteAtomic(summaryPath, BuildSummary(result));
            result.EvidenceFiles = new List<string> { summaryPath, resultPath };
            if (File.Exists(readyPath)) result.EvidenceFiles.Add(readyPath);
            if (File.Exists(eventsPath)) result.EvidenceFiles.Add(eventsPath);
            WriteAtomic(resultPath, JsonConvert.SerializeObject(result, Formatting.Indented) + Environment.NewLine);
        }

        internal static void WriteAtomic(string path, string content)
        {
            string directory = Path.GetDirectoryName(path);
            if (string.IsNullOrWhiteSpace(directory) || !Directory.Exists(directory))
                throw new DirectoryNotFoundException(directory);
            string temporary = Path.Combine(
                directory,
                "." + Path.GetFileName(path) + "." + Guid.NewGuid().ToString("N") + ".tmp");
            byte[] bytes = new UTF8Encoding(false).GetBytes(content ?? string.Empty);
            try
            {
                using (var stream = new FileStream(
                    temporary, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                {
                    stream.Write(bytes, 0, bytes.Length);
                    stream.Flush(true);
                }
                if (File.Exists(path))
                    File.Replace(temporary, path, null);
                else
                    File.Move(temporary, path);
            }
            finally
            {
                if (File.Exists(temporary)) File.Delete(temporary);
            }
        }

        private static string BuildSummary(RuntimeTestResult result)
        {
            var builder = new StringBuilder();
            builder.AppendLine("Kingmaker Gunslinger runtime test");
            builder.AppendLine("Run ID: " + result.RunId);
            builder.AppendLine("Scenario: " + result.Scenario);
            builder.AppendLine("Status: " + result.Status);
            builder.AppendLine("Loaded mod: " + result.LoadedModVersion);
            builder.AppendLine("Runtime: " + result.RuntimeIdentity);
            builder.AppendLine("Started UTC: " + result.StartUtc);
            builder.AppendLine("Ended UTC: " + result.EndUtc);
            builder.AppendLine("Duration ms: " + result.DurationMilliseconds);
            foreach (RuntimeTestAssertion assertion in result.Assertions)
                builder.AppendLine(assertion.Status + " " + assertion.Name +
                    " expected=" + assertion.Expected + " observed=" + assertion.Observed);
            if (!string.IsNullOrWhiteSpace(result.ExceptionSummary))
                builder.AppendLine("Exception: " + result.ExceptionSummary);
            return builder.ToString();
        }
    }
}
