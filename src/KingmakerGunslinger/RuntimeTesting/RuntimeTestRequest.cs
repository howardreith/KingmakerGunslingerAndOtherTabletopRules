using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed class RuntimeTestRequest
    {
        internal const int CurrentSchemaVersion = 1;
        internal const string CommandLineFlag = "-kmgRuntimeTestRequest";
        internal const string EvidenceRoot = @"C:\Dev\KingmakerGunslingerLab\runtime-evidence";

        [JsonProperty("schemaVersion", Required = Required.Always)]
        public int SchemaVersion { get; set; }
        [JsonProperty("enabled", Required = Required.Always)]
        public bool Enabled { get; set; }
        [JsonProperty("runId", Required = Required.Always)]
        public string RunId { get; set; }
        [JsonProperty("scenario", Required = Required.Always)]
        public string Scenario { get; set; }
        [JsonProperty("expectedModVersion", Required = Required.Always)]
        public string ExpectedModVersion { get; set; }
        [JsonProperty("evidenceDirectory", Required = Required.Always)]
        public string EvidenceDirectory { get; set; }
        [JsonProperty("timeoutSeconds", Required = Required.Always)]
        public int TimeoutSeconds { get; set; }
        [JsonProperty("startupTimeoutSeconds", Required = Required.Always)]
        public int StartupTimeoutSeconds { get; set; }
        [JsonProperty("catalogTimeoutSeconds", Required = Required.Default)]
        public int CatalogTimeoutSeconds { get; set; }
        [JsonProperty("selectionTimeoutSeconds", Required = Required.Default)]
        public int SelectionTimeoutSeconds { get; set; }
        [JsonProperty("completionTimeoutSeconds", Required = Required.Default)]
        public int CompletionTimeoutSeconds { get; set; }
        [JsonProperty("mainMenuTimeoutSeconds", Required = Required.Default)]
        public int MainMenuTimeoutSeconds { get; set; }
        [JsonProperty("actionResolutionTimeoutSeconds", Required = Required.Default)]
        public int ActionResolutionTimeoutSeconds { get; set; }
        [JsonProperty("actionInvocationTimeoutSeconds", Required = Required.Default)]
        public int ActionInvocationTimeoutSeconds { get; set; }
        [JsonProperty("descriptorResolutionTimeoutSeconds", Required = Required.Default)]
        public int DescriptorResolutionTimeoutSeconds { get; set; }
        [JsonProperty("loadEntryTimeoutSeconds", Required = Required.Default)]
        public int LoadEntryTimeoutSeconds { get; set; }
        [JsonProperty("fingerprintTimeoutSeconds", Required = Required.Default)]
        public int FingerprintTimeoutSeconds { get; set; }
        [JsonProperty("exitAfterCompletion", Required = Required.Always)]
        public bool ExitAfterCompletion { get; set; }
        [JsonProperty("parameters", Required = Required.Always)]
        public JObject Parameters { get; set; }
    }

    internal sealed class RuntimeTestRequestDecision
    {
        internal RuntimeTestRequestDecision(
            bool accepted,
            string reasonCode,
            string safeRequestName,
            RuntimeTestRequest request,
            string failedStage = "",
            string requestedScenario = "")
        {
            Accepted = accepted;
            ReasonCode = reasonCode ?? string.Empty;
            SafeRequestName = safeRequestName ?? string.Empty;
            Request = request;
            FailedStage = failedStage ?? string.Empty;
            RequestedScenario = requestedScenario ?? string.Empty;
        }

        internal bool Accepted { get; private set; }
        internal string ReasonCode { get; private set; }
        internal string SafeRequestName { get; private set; }
        internal RuntimeTestRequest Request { get; private set; }
        internal string FailedStage { get; private set; }
        internal string RequestedScenario { get; private set; }
    }

    internal static class RuntimeTestRequestParser
    {
        private static readonly string[] AllowedMembers =
        {
            "schemaVersion", "enabled", "runId", "scenario",
            "expectedModVersion", "evidenceDirectory", "timeoutSeconds",
            "startupTimeoutSeconds", "exitAfterCompletion", "parameters"
            , "catalogTimeoutSeconds", "selectionTimeoutSeconds",
            "completionTimeoutSeconds", "mainMenuTimeoutSeconds",
            "actionResolutionTimeoutSeconds", "actionInvocationTimeoutSeconds",
            "descriptorResolutionTimeoutSeconds", "loadEntryTimeoutSeconds",
            "fingerprintTimeoutSeconds"
        };

        internal static RuntimeTestRequestDecision TryActivate(
            string[] arguments,
            string loadedModVersion)
        {
            string requestPath;
            string commandReason;
            if (!TryGetRequestPath(arguments, out requestPath, out commandReason))
            {
                return Reject(commandReason, null);
            }

            string safeName = SafeFileName(requestPath);
            try
            {
                if (!Path.IsPathRooted(requestPath))
                {
                    return Reject("request-path-not-absolute", requestPath);
                }
                if (!File.Exists(requestPath))
                {
                    return Reject("request-file-missing", requestPath);
                }

                string json = File.ReadAllText(requestPath);
                EnsureNoDuplicateProperties(json);
                JObject document = JObject.Parse(json);
                string requestedScenario = document.Value<string>("scenario") ?? string.Empty;
                RejectUnknownMembers(document);
                var serializer = JsonSerializer.Create(new JsonSerializerSettings
                {
                    MissingMemberHandling = MissingMemberHandling.Error
                });
                RuntimeTestRequest request = document.ToObject<RuntimeTestRequest>(serializer);
                string validation = Validate(request, loadedModVersion);
                if (validation != null)
                {
                    return new RuntimeTestRequestDecision(false, validation, safeName,
                        null, FailedStage(validation), requestedScenario);
                }
                return new RuntimeTestRequestDecision(true, "accepted", safeName, request,
                    string.Empty, requestedScenario);
            }
            catch (JsonException)
            {
                return new RuntimeTestRequestDecision(false, "invalid-json", safeName, null);
            }
            catch (IOException)
            {
                return new RuntimeTestRequestDecision(false, "request-read-failed", safeName, null);
            }
            catch (UnauthorizedAccessException)
            {
                return new RuntimeTestRequestDecision(false, "request-read-failed", safeName, null);
            }
            catch (ArgumentException)
            {
                return new RuntimeTestRequestDecision(false, "request-invalid", safeName, null);
            }
            catch (Exception)
            {
                return new RuntimeTestRequestDecision(false, "request-invalid", safeName, null);
            }
        }

        internal static bool TryGetRequestPath(
            string[] arguments,
            out string requestPath,
            out string reason)
        {
            requestPath = null;
            reason = "flag-absent";
            if (arguments == null)
            {
                return false;
            }

            int match = -1;
            for (int index = 0; index < arguments.Length; index++)
            {
                if (!string.Equals(
                    arguments[index],
                    RuntimeTestRequest.CommandLineFlag,
                    StringComparison.Ordinal))
                {
                    continue;
                }
                if (match >= 0)
                {
                    reason = "duplicate-flag";
                    return false;
                }
                match = index;
            }
            if (match < 0)
            {
                return false;
            }
            if (match + 1 >= arguments.Length ||
                string.IsNullOrWhiteSpace(arguments[match + 1]))
            {
                reason = "request-path-missing";
                return false;
            }
            requestPath = arguments[match + 1];
            reason = null;
            return true;
        }

        private static string Validate(RuntimeTestRequest request, string loadedVersion)
        {
            if (request == null || request.SchemaVersion != RuntimeTestRequest.CurrentSchemaVersion)
                return "schema-version-invalid";
            if (!request.Enabled) return "request-disabled";
            if (!IsValidRunId(request.RunId)) return "run-id-invalid";
            if (!RuntimeTestScenarioCatalog.IsAllowed(request.Scenario))
                return "scenario-not-allowed";
            if (string.IsNullOrWhiteSpace(loadedVersion) ||
                !string.Equals(request.ExpectedModVersion, loadedVersion, StringComparison.Ordinal))
                return "mod-version-mismatch";
            if (request.TimeoutSeconds < 5 || request.TimeoutSeconds > 1800)
                return "timeout-invalid";
            if (request.StartupTimeoutSeconds < 5 || request.StartupTimeoutSeconds > 600)
                return "startup-timeout-invalid";
            bool workingSmoke = request.Scenario ==
                RuntimeTestScenarioCatalog.WorkingSaveSmoke;
            bool workingEntryObservation = request.Scenario ==
                RuntimeTestScenarioCatalog.ObserveWorkingSaveEntryAction ||
                request.Scenario == RuntimeTestScenarioCatalog.ObserveWorkingSaveSelectionLoadAction ||
                request.Scenario == RuntimeTestScenarioCatalog.ObserveWorkingSaveReceiverBoundAction;
            bool catalogScenario = request.Scenario ==
                RuntimeTestScenarioCatalog.ObserveSaveCatalogAndSelection ||
                request.Scenario == RuntimeTestScenarioCatalog.ObserveSaveCatalogProvider ||
                request.Scenario == RuntimeTestScenarioCatalog.ObserveLoadGameButtonAction ||
                workingSmoke || workingEntryObservation;
            if (catalogScenario && (request.CatalogTimeoutSeconds < 5 ||
                request.CatalogTimeoutSeconds > 1800))
                return "catalog-timeout-invalid";
            bool selectionScenario = request.Scenario ==
                RuntimeTestScenarioCatalog.ObserveSaveCatalogAndSelection ||
                workingSmoke || workingEntryObservation;
            if (selectionScenario && (request.SelectionTimeoutSeconds < 5 ||
                request.SelectionTimeoutSeconds > 1800))
                return "selection-timeout-invalid";
            if (selectionScenario && (request.CompletionTimeoutSeconds < 5 ||
                request.CompletionTimeoutSeconds > 1800))
                return "completion-timeout-invalid";
            if (!selectionScenario && (request.SelectionTimeoutSeconds != 0 ||
                request.CompletionTimeoutSeconds != 0))
                return "scenario-timeouts-not-allowed";
            if (!catalogScenario && request.CatalogTimeoutSeconds != 0)
                return "scenario-timeouts-not-allowed";
            if (workingSmoke || workingEntryObservation)
            {
                if (!ValidStageTimeout(request.MainMenuTimeoutSeconds) ||
                    !ValidStageTimeout(request.ActionResolutionTimeoutSeconds) ||
                    !ValidStageTimeout(request.ActionInvocationTimeoutSeconds) ||
                    !ValidStageTimeout(request.DescriptorResolutionTimeoutSeconds) ||
                    !ValidStageTimeout(request.LoadEntryTimeoutSeconds) ||
                    !ValidStageTimeout(request.FingerprintTimeoutSeconds))
                    return "scenario-timeout-invalid";
                if (request.Parameters == null || request.Parameters.Count != 1 ||
                    request.Parameters.Property("saveName") == null ||
                    request.Parameters["saveName"].Type != JTokenType.String)
                    return "save-name-required";
                string saveName = (string)request.Parameters["saveName"];
                if (!string.Equals(saveName, ManualSaveLoadObservation.WorkingSave,
                    StringComparison.Ordinal))
                    return string.Equals(saveName, ManualSaveLoadObservation.BaselineSave,
                        StringComparison.Ordinal)
                        ? "baseline-save-forbidden" : "save-name-not-allowed";
            }
            else
            {
                if (request.MainMenuTimeoutSeconds != 0 ||
                    request.ActionResolutionTimeoutSeconds != 0 ||
                    request.ActionInvocationTimeoutSeconds != 0 ||
                    request.DescriptorResolutionTimeoutSeconds != 0 ||
                    request.LoadEntryTimeoutSeconds != 0 ||
                    request.FingerprintTimeoutSeconds != 0)
                    return "scenario-timeouts-not-allowed";
                if (request.Parameters == null || request.Parameters.Count != 0)
                    return "parameters-not-allowed";
            }

            string evidence;
            try
            {
                evidence = Path.GetFullPath(request.EvidenceDirectory).TrimEnd('\\');
            }
            catch
            {
                return "evidence-path-invalid";
            }
            string root = Path.GetFullPath(RuntimeTestRequest.EvidenceRoot).TrimEnd('\\');
            if (!evidence.StartsWith(root + "\\", StringComparison.OrdinalIgnoreCase))
                return "evidence-path-outside-root";
            if (!Directory.Exists(evidence))
                return "evidence-directory-missing";
            if (ContainsReparsePoint(evidence, root))
                return "evidence-path-reparse-point";
            if (File.Exists(Path.Combine(evidence, "runtime-result.json")) ||
                File.Exists(Path.Combine(root, ".kmg-run-" + request.RunId)))
                return "run-id-duplicate";

            request.EvidenceDirectory = evidence;
            return null;
        }

        private static bool ValidStageTimeout(int seconds)
        {
            return seconds >= 5 && seconds <= 1800;
        }

        private static bool ContainsReparsePoint(string path, string root)
        {
            var current = new DirectoryInfo(path);
            while (current != null)
            {
                if ((current.Attributes & FileAttributes.ReparsePoint) != 0)
                    return true;
                if (current.FullName.TrimEnd('\\').Equals(
                    root, StringComparison.OrdinalIgnoreCase))
                    return false;
                current = current.Parent;
            }
            return true;
        }

        private static bool IsValidRunId(string runId)
        {
            if (string.IsNullOrWhiteSpace(runId) || runId.Length > 100)
                return false;
            foreach (char character in runId)
            {
                bool valid = character >= 'a' && character <= 'z' ||
                    character >= 'A' && character <= 'Z' ||
                    character >= '0' && character <= '9' ||
                    character == '.' || character == '_' || character == '-';
                if (!valid) return false;
            }
            return true;
        }

        private static void RejectUnknownMembers(JObject document)
        {
            var allowed = new HashSet<string>(AllowedMembers, StringComparer.Ordinal);
            foreach (JProperty property in document.Properties())
            {
                if (!allowed.Contains(property.Name))
                    throw new JsonSerializationException("Unknown request member.");
            }
        }

        private static void EnsureNoDuplicateProperties(string json)
        {
            using (var text = new StringReader(json))
            using (var reader = new JsonTextReader(text))
            {
                var scopes = new Stack<HashSet<string>>();
                while (reader.Read())
                {
                    if (reader.TokenType == JsonToken.StartObject)
                        scopes.Push(new HashSet<string>(StringComparer.Ordinal));
                    else if (reader.TokenType == JsonToken.EndObject)
                        scopes.Pop();
                    else if (reader.TokenType == JsonToken.PropertyName)
                    {
                        if (scopes.Count == 0 || !scopes.Peek().Add((string)reader.Value))
                            throw new JsonReaderException("Duplicate JSON property.");
                    }
                }
            }
        }

        private static RuntimeTestRequestDecision Reject(string reason, string path)
        {
            return new RuntimeTestRequestDecision(false, reason, SafeFileName(path), null,
                FailedStage(reason), string.Empty);
        }

        private static string FailedStage(string reason)
        {
            if (reason == "request-file-missing" || reason == "request-read-failed")
                return "request-file-opened";
            if (reason == "invalid-json" || reason == "request-invalid")
                return "request-json-parsed";
            if (reason == "schema-version-invalid")
                return "request-schema-valid";
            if (reason == "mod-version-mismatch")
                return "expected-version-valid";
            if (reason == "scenario-not-allowed")
                return "scenario-allowlisted";
            if (reason == "save-name-required" ||
                reason == "save-name-not-allowed" ||
                reason == "baseline-save-forbidden")
                return "save-name-valid";
            return "request-accepted";
        }

        private static string SafeFileName(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return string.Empty;
            try { return Path.GetFileName(path); }
            catch { return string.Empty; }
        }
    }
}
