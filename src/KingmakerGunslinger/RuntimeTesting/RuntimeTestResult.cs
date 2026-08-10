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
        [JsonProperty("errorStage", Order = 19, NullValueHandling = NullValueHandling.Ignore)]
        public string ErrorStage { get; set; }
        [JsonProperty("lastCompletedStage", Order = 20, NullValueHandling = NullValueHandling.Ignore)]
        public string LastCompletedStage { get; set; }
        [JsonProperty("exceptionType", Order = 21, NullValueHandling = NullValueHandling.Ignore)]
        public string ExceptionType { get; set; }
        [JsonProperty("exceptionMessage", Order = 22, NullValueHandling = NullValueHandling.Ignore)]
        public string ExceptionMessage { get; set; }
        [JsonProperty("exceptionStack", Order = 23, NullValueHandling = NullValueHandling.Ignore)]
        public string ExceptionStack { get; set; }
        [JsonProperty("exceptionManagedThreadId", Order = 24)]
        public int ExceptionManagedThreadId { get; set; }
        [JsonProperty("saveLoadObservation", Order = 25, NullValueHandling = NullValueHandling.Ignore)]
        public SaveLoadObservationEvidence SaveLoadObservation { get; set; }
        [JsonProperty("saveCatalogObservation", Order = 26, NullValueHandling = NullValueHandling.Ignore)]
        public SaveCatalogObservationEvidence SaveCatalogObservation { get; set; }
        [JsonProperty("saveCatalogProviderObservation", Order = 27,
            NullValueHandling = NullValueHandling.Ignore)]
        public SaveCatalogProviderObservationEvidence SaveCatalogProviderObservation { get; set; }
        [JsonProperty("loadGameButtonActionObservation", Order = 28,
            NullValueHandling = NullValueHandling.Ignore)]
        public LoadGameButtonActionEvidence LoadGameButtonActionObservation { get; set; }
        [JsonProperty("workingSaveSmoke", Order = 29,
            NullValueHandling = NullValueHandling.Ignore)]
        public WorkingSaveSmokeEvidence WorkingSaveSmoke { get; set; }
        [JsonProperty("workingSaveEntryActionObservation", Order = 30,
            NullValueHandling = NullValueHandling.Ignore)]
        public WorkingSaveSmokeEvidence WorkingSaveEntryActionObservation { get; set; }
        [JsonProperty("workingSaveSelectionLoadActionObservation", Order = 31,
            NullValueHandling = NullValueHandling.Ignore)]
        public WorkingSaveSmokeEvidence WorkingSaveSelectionLoadActionObservation { get; set; }
        [JsonProperty("workingSaveReceiverBoundActionObservation", Order = 32,
            NullValueHandling = NullValueHandling.Ignore)]
        public WorkingSaveSmokeEvidence WorkingSaveReceiverBoundActionObservation { get; set; }
        [JsonProperty("evidenceDirectory", Order = 31)]
        public string EvidenceDirectory { get; set; }
    }

    internal sealed class WorkingSaveSmokeEvidence
    {
        [JsonProperty("stage", Order = 1)] public string Stage { get; set; }
        [JsonProperty("button", Order = 2)] public LoadGameButtonCandidateEvidence Button { get; set; }
        [JsonProperty("buttonCandidateCount", Order = 3)] public int ButtonCandidateCount { get; set; }
        [JsonProperty("buttonEventInvocationCount", Order = 4)] public int ButtonEventInvocationCount { get; set; }
        [JsonProperty("handlerInvocationCount", Order = 5)] public int HandlerInvocationCount { get; set; }
        [JsonProperty("catalogInitializeCount", Order = 6)] public int CatalogInitializeCount { get; set; }
        [JsonProperty("catalogDescriptorCount", Order = 7)] public int CatalogDescriptorCount { get; set; }
        [JsonProperty("catalogComplete", Order = 8)] public bool CatalogComplete { get; set; }
        [JsonProperty("workingMatchCount", Order = 9)] public int WorkingMatchCount { get; set; }
        [JsonProperty("baselineMatchCount", Order = 10)] public int BaselineMatchCount { get; set; }
        [JsonProperty("resolvedDescriptor", Order = 11)] public SaveCatalogDescriptorEvidence ResolvedDescriptor { get; set; }
        [JsonProperty("descriptorReferenceCorrelated", Order = 12)] public bool DescriptorReferenceCorrelated { get; set; }
        [JsonProperty("loadEntryInvocationCount", Order = 13)] public int LoadEntryInvocationCount { get; set; }
        [JsonProperty("completionCallbackObserved", Order = 14)] public bool CompletionCallbackObserved { get; set; }
        [JsonProperty("stableFingerprint", Order = 15)] public string StableFingerprint { get; set; }
        [JsonProperty("saveWritingApiObserved", Order = 16)] public bool SaveWritingApiObserved { get; set; }
        [JsonProperty("allCallbacksOnGameThread", Order = 17)] public bool AllCallbacksOnGameThread { get; set; }
        [JsonProperty("hooksRemoved", Order = 18)] public bool HooksRemoved { get; set; }
        [JsonProperty("hooksInstalled", Order = 19)] public bool HooksInstalled { get; set; }
        [JsonProperty("uiActionOccurred", Order = 20)] public bool UiActionOccurred { get; set; }
        [JsonProperty("descriptorResolved", Order = 21)] public bool DescriptorResolved { get; set; }
        [JsonProperty("loadingBegan", Order = 22)] public bool LoadingBegan { get; set; }
        [JsonProperty("loadingCompleted", Order = 23)] public bool LoadingCompleted { get; set; }
        [JsonProperty("events", Order = 24)] public List<SaveLoadObservationEvent> Events { get; set; }
        [JsonProperty("entryCandidateCount", Order = 25)] public int EntryCandidateCount { get; set; }
        [JsonProperty("entryActionCandidateCount", Order = 26)] public int EntryActionCandidateCount { get; set; }
        [JsonProperty("entryAction", Order = 27)] public LoadGameButtonCandidateEvidence EntryAction { get; set; }
        [JsonProperty("entryObjectIdentity", Order = 28)] public string EntryObjectIdentity { get; set; }
        [JsonProperty("actionObjectIdentity", Order = 29)] public string ActionObjectIdentity { get; set; }
        [JsonProperty("humanActionInvocationCount", Order = 30)] public int HumanActionInvocationCount { get; set; }
        [JsonProperty("listenerInvocationCount", Order = 31)] public int ListenerInvocationCount { get; set; }
        [JsonProperty("listenerTargetIdentity", Order = 32)] public string ListenerTargetIdentity { get; set; }
        [JsonProperty("listenerMethod", Order = 33)] public string ListenerMethod { get; set; }
        [JsonProperty("loadEntryReceiverIdentity", Order = 34)] public string LoadEntryReceiverIdentity { get; set; }
        [JsonProperty("probeInvokedEntryAction", Order = 35)] public bool ProbeInvokedEntryAction { get; set; }
        [JsonProperty("ownerObjectIdentity", Order = 36)] public string OwnerObjectIdentity { get; set; }
        [JsonProperty("listObjectIdentity", Order = 37)] public string ListObjectIdentity { get; set; }
        [JsonProperty("scopedActionCandidates", Order = 38)] public List<string> ScopedActionCandidates { get; set; }
        [JsonProperty("observedScopedInvocations", Order = 39)] public List<string> ObservedScopedInvocations { get; set; }
        [JsonProperty("selectedSaveStorage", Order = 40)] public List<string> SelectedSaveStorage { get; set; }
        [JsonProperty("selectedWorkingStateObserved", Order = 41)] public bool SelectedWorkingStateObserved { get; set; }
        [JsonProperty("finalLoadActionCount", Order = 42)] public int FinalLoadActionCount { get; set; }
        [JsonProperty("immediateLoadCaller", Order = 43)] public string ImmediateLoadCaller { get; set; }
        [JsonProperty("immediateLoadCallerType", Order = 44)] public string ImmediateLoadCallerType { get; set; }
        [JsonProperty("loadCallerChain", Order = 45)] public List<string> LoadCallerChain { get; set; }
        [JsonProperty("immediateLoadCallerReceiverIdentity", Order = 46)] public string ImmediateLoadCallerReceiverIdentity { get; set; }
        [JsonProperty("compatibleCallerReceiverCount", Order = 47)] public int CompatibleCallerReceiverCount { get; set; }
        [JsonProperty("candidateRejections", Order = 48)] public List<string> CandidateRejections { get; set; }
        [JsonProperty("descriptorMemberIdentity", Order = 49)] public string DescriptorMemberIdentity { get; set; }
        [JsonProperty("entryHierarchyPath", Order = 50)] public string EntryHierarchyPath { get; set; }
        [JsonProperty("gameThreadManagedId", Order = 51)] public int GameThreadManagedId { get; set; }
        [JsonProperty("receiverBoundHookIdentifiers", Order = 52)] public List<string> ReceiverBoundHookIdentifiers { get; set; }
        [JsonProperty("slotActionInvocationCount", Order = 53)] public int SlotActionInvocationCount { get; set; }
        [JsonProperty("slotReceiverReferenceCorrelated", Order = 54)] public bool SlotReceiverReferenceCorrelated { get; set; }
        [JsonProperty("windowHandlerInvocationCount", Order = 55)] public int WindowHandlerInvocationCount { get; set; }
        [JsonProperty("windowReceiverReferenceCorrelated", Order = 56)] public bool WindowReceiverReferenceCorrelated { get; set; }
        [JsonProperty("windowArgumentReferenceCorrelated", Order = 57)] public bool WindowArgumentReferenceCorrelated { get; set; }
        [JsonProperty("slotActionSequence", Order = 58)] public int SlotActionSequence { get; set; }
        [JsonProperty("windowHandlerSequence", Order = 59)] public int WindowHandlerSequence { get; set; }
        [JsonProperty("loadEntrySequence", Order = 60)] public int LoadEntrySequence { get; set; }
        [JsonProperty("completionSequence", Order = 61)] public int CompletionSequence { get; set; }
        [JsonProperty("fingerprintSequence", Order = 62)] public int FingerprintSequence { get; set; }
        [JsonProperty("expectedWorkingSaveRoutineCount", Order = 63)]
        public int ExpectedWorkingSaveRoutineCount { get; set; }
        [JsonProperty("expectedWorkingStashedAreaCount", Order = 64)]
        public int ExpectedWorkingStashedAreaCount { get; set; }
    }

    internal sealed class LoadGameButtonActionEvidence
    {
        [JsonProperty("actionProven", Order = 1)] public bool ActionProven { get; set; }
        [JsonProperty("handlerSignature", Order = 2)] public string HandlerSignature { get; set; }
        [JsonProperty("handlerInvocationCount", Order = 3)] public int HandlerInvocationCount { get; set; }
        [JsonProperty("candidates", Order = 4)] public List<LoadGameButtonCandidateEvidence> Candidates { get; set; }
        [JsonProperty("catalogInitializeSignature", Order = 5)] public string CatalogInitializeSignature { get; set; }
        [JsonProperty("catalogObservedAfterAction", Order = 6)] public bool CatalogObservedAfterAction { get; set; }
        [JsonProperty("gameThreadManagedId", Order = 7)] public int GameThreadManagedId { get; set; }
        [JsonProperty("allCallbacksOnGameThread", Order = 8)] public bool AllCallbacksOnGameThread { get; set; }
        [JsonProperty("hooksRemoved", Order = 9)] public bool HooksRemoved { get; set; }
        [JsonProperty("probeInvokedAction", Order = 10)] public bool ProbeInvokedAction { get; set; }
        [JsonProperty("events", Order = 11)] public List<SaveLoadObservationEvent> Events { get; set; }
    }

    internal sealed class LoadGameButtonCandidateEvidence
    {
        [JsonProperty("componentType", Order = 1)] public string ComponentType { get; set; }
        [JsonProperty("gameObjectPath", Order = 2)] public string GameObjectPath { get; set; }
        [JsonProperty("activeSelf", Order = 3)] public bool ActiveSelf { get; set; }
        [JsonProperty("activeInHierarchy", Order = 4)] public bool ActiveInHierarchy { get; set; }
        [JsonProperty("interactable", Order = 5)] public bool Interactable { get; set; }
        [JsonProperty("siblingIndex", Order = 6)] public int SiblingIndex { get; set; }
        [JsonProperty("siblingCount", Order = 7)] public int SiblingCount { get; set; }
        [JsonProperty("ownerType", Order = 8)] public string OwnerType { get; set; }
        [JsonProperty("mainMenuRootName", Order = 9)] public string MainMenuRootName { get; set; }
        [JsonProperty("mainMenuRootPath", Order = 10)] public string MainMenuRootPath { get; set; }
        [JsonProperty("componentIdentities", Order = 11)] public List<string> ComponentIdentities { get; set; }
        [JsonProperty("safeLabelIdentities", Order = 12)] public List<string> SafeLabelIdentities { get; set; }
        [JsonProperty("listeners", Order = 13)] public List<LoadGameListenerEvidence> Listeners { get; set; }
    }

    internal sealed class LoadGameListenerEvidence
    {
        [JsonProperty("kind", Order = 1)] public string Kind { get; set; }
        [JsonProperty("targetType", Order = 2)] public string TargetType { get; set; }
        [JsonProperty("methodName", Order = 3)] public string MethodName { get; set; }
        [JsonProperty("safeCapturedFields", Order = 4)]
        public List<string> SafeCapturedFields { get; set; }
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
        [JsonProperty("runtimeRunnerActive", Order = 9)]
        public bool RuntimeRunnerActive { get; set; }
        [JsonProperty("updateCallbackCount", Order = 10)]
        public int UpdateCallbackCount { get; set; }
        [JsonProperty("mainMenuLifecycleReady", Order = 11)]
        public bool MainMenuLifecycleReady { get; set; }
        [JsonProperty("ummStartupState", Order = 12)]
        public string UmmStartupState { get; set; }
        [JsonProperty("readinessStage", Order = 13)]
        public string ReadinessStage { get; set; }
        [JsonProperty("saveName", Order = 14)] public string SaveName { get; set; }
        [JsonProperty("exactSlotIdentity", Order = 15)] public string ExactSlotIdentity { get; set; }
        [JsonProperty("exactWindowIdentity", Order = 16)] public string ExactWindowIdentity { get; set; }
    }

    internal sealed class RuntimeStageMarker
    {
        [JsonProperty("schemaVersion", Order = 1)] public int SchemaVersion { get; set; }
        [JsonProperty("runId", Order = 2)] public string RunId { get; set; }
        [JsonProperty("scenario", Order = 3)] public string Scenario { get; set; }
        [JsonProperty("stage", Order = 4)] public string Stage { get; set; }
        [JsonProperty("loadedModVersion", Order = 5)] public string LoadedModVersion { get; set; }
        [JsonProperty("timestampUtc", Order = 6)] public string TimestampUtc { get; set; }
        [JsonProperty("processId", Order = 7)] public int ProcessId { get; set; }
        [JsonProperty("workingMatchCount", Order = 8)] public int WorkingMatchCount { get; set; }
        [JsonProperty("baselineMatchCount", Order = 9)] public int BaselineMatchCount { get; set; }
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

        internal void WriteStage(string fileName, RuntimeStageMarker marker)
        {
            RuntimeTestResultWriter.WriteAtomic(
                Path.Combine(_directory, fileName),
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
                }, Formatting.Indented, new JsonSerializerSettings
                {
                    PreserveReferencesHandling = PreserveReferencesHandling.None,
                    ReferenceLoopHandling = ReferenceLoopHandling.Error
                }) + Environment.NewLine);
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

    internal sealed class SaveCatalogDescriptorEvidence
    {
        [JsonProperty("classification", Order = 1)] public string Classification { get; set; }
        [JsonProperty("displayName", Order = 2)] public string DisplayName { get; set; }
        [JsonProperty("identityHash", Order = 3)] public string IdentityHash { get; set; }
        [JsonProperty("safeFields", Order = 4)] public Dictionary<string, string> SafeFields { get; set; }
    }

    internal sealed class SaveCatalogObservationEvidence
    {
        [JsonProperty("catalogManagerType", Order = 1)] public string CatalogManagerType { get; set; }
        [JsonProperty("collectionType", Order = 2)] public string CollectionType { get; set; }
        [JsonProperty("descriptorType", Order = 3)] public string DescriptorType { get; set; }
        [JsonProperty("descriptorCount", Order = 4)] public int DescriptorCount { get; set; }
        [JsonProperty("workingMatchCount", Order = 5)] public int WorkingMatchCount { get; set; }
        [JsonProperty("baselineMatchCount", Order = 6)] public int BaselineMatchCount { get; set; }
        [JsonProperty("catalogComplete", Order = 7)] public bool CatalogComplete { get; set; }
        [JsonProperty("selectedCorrelates", Order = 8)] public bool SelectedCorrelates { get; set; }
        [JsonProperty("correlationMethod", Order = 9)] public string CorrelationMethod { get; set; }
        [JsonProperty("selectedClassification", Order = 10)] public string SelectedClassification { get; set; }
        [JsonProperty("completionObserved", Order = 11)] public bool CompletionObserved { get; set; }
        [JsonProperty("stableFingerprint", Order = 12)] public string StableFingerprint { get; set; }
        [JsonProperty("saveWritingApiObserved", Order = 13)] public bool SaveWritingApiObserved { get; set; }
        [JsonProperty("hooksRemoved", Order = 14)] public bool HooksRemoved { get; set; }
        [JsonProperty("allCallbacksOnGameThread", Order = 15)] public bool AllCallbacksOnGameThread { get; set; }
        [JsonProperty("descriptors", Order = 16)] public List<SaveCatalogDescriptorEvidence> Descriptors { get; set; }
        [JsonProperty("events", Order = 17)] public List<SaveLoadObservationEvent> Events { get; set; }
        [JsonProperty("probeInitiatedSaveWriting", Order = 18)]
        public bool ProbeInitiatedSaveWriting { get; set; }
    }

    internal sealed class CatalogProviderCandidateEvidence
    {
        [JsonProperty("declaringType", Order = 1)] public string DeclaringType { get; set; }
        [JsonProperty("methodSignature", Order = 2)] public string MethodSignature { get; set; }
        [JsonProperty("sourceKind", Order = 3)] public string SourceKind { get; set; }
        [JsonProperty("correlation", Order = 4)] public string Correlation { get; set; }
        [JsonProperty("canInvokeWithoutUi", Order = 5)] public bool CanInvokeWithoutUi { get; set; }
        [JsonProperty("appearsReadOnly", Order = 6)] public bool AppearsReadOnly { get; set; }
        [JsonProperty("isStatic", Order = 7)] public bool IsStatic { get; set; }
        [JsonProperty("receiverType", Order = 8)] public string ReceiverType { get; set; }
        [JsonProperty("requiredArguments", Order = 9)] public List<string> RequiredArguments { get; set; }
        [JsonProperty("returnType", Order = 10)] public string ReturnType { get; set; }
        [JsonProperty("managedThreadId", Order = 11)] public int ManagedThreadId { get; set; }
        [JsonProperty("requiresLoadGameUi", Order = 12)] public bool RequiresLoadGameUi { get; set; }
        [JsonProperty("catalogRole", Order = 13)] public string CatalogRole { get; set; }
        [JsonProperty("sideEffects", Order = 14)] public string SideEffects { get; set; }
        [JsonProperty("contractStable", Order = 15)] public bool ContractStable { get; set; }
        [JsonProperty("proofMissing", Order = 16)] public string ProofMissing { get; set; }
    }

    internal sealed class CatalogOwnerMemberEvidence
    {
        [JsonProperty("ownerType", Order = 1)] public string OwnerType { get; set; }
        [JsonProperty("memberName", Order = 2)] public string MemberName { get; set; }
        [JsonProperty("memberType", Order = 3)] public string MemberType { get; set; }
        [JsonProperty("memberKind", Order = 4)] public string MemberKind { get; set; }
    }

    internal sealed class SaveCatalogProviderObservationEvidence
    {
        [JsonProperty("initializeSignature", Order = 1)] public string InitializeSignature { get; set; }
        [JsonProperty("collectionType", Order = 2)] public string CollectionType { get; set; }
        [JsonProperty("descriptorType", Order = 3)] public string DescriptorType { get; set; }
        [JsonProperty("descriptorCount", Order = 4)] public int DescriptorCount { get; set; }
        [JsonProperty("completeListObserved", Order = 5)] public bool CompleteListObserved { get; set; }
        [JsonProperty("receiverRuntimeType", Order = 6)] public string ReceiverRuntimeType { get; set; }
        [JsonProperty("immediateCaller", Order = 7)] public string ImmediateCaller { get; set; }
        [JsonProperty("callerChain", Order = 8)] public List<string> CallerChain { get; set; }
        [JsonProperty("ownerMembers", Order = 9)] public List<CatalogOwnerMemberEvidence> OwnerMembers { get; set; }
        [JsonProperty("providerCandidates", Order = 10)]
        public List<CatalogProviderCandidateEvidence> ProviderCandidates { get; set; }
        [JsonProperty("sourceProven", Order = 11)] public bool SourceProven { get; set; }
        [JsonProperty("sourceKind", Order = 12)] public string SourceKind { get; set; }
        [JsonProperty("allCallbacksOnGameThread", Order = 13)]
        public bool AllCallbacksOnGameThread { get; set; }
        [JsonProperty("lifecycleState", Order = 14)] public string LifecycleState { get; set; }
        [JsonProperty("providerInvokedByProbe", Order = 15)] public bool ProviderInvokedByProbe { get; set; }
        [JsonProperty("saveLoadObserved", Order = 16)] public bool SaveLoadObserved { get; set; }
        [JsonProperty("saveWritingObserved", Order = 17)] public bool SaveWritingObserved { get; set; }
        [JsonProperty("hooksRemoved", Order = 18)] public bool HooksRemoved { get; set; }
        [JsonProperty("events", Order = 19)] public List<SaveLoadObservationEvent> Events { get; set; }
        [JsonProperty("collectionObjectIdentity", Order = 20)] public string CollectionObjectIdentity { get; set; }
        [JsonProperty("receiverObjectIdentity", Order = 21)] public string ReceiverObjectIdentity { get; set; }
        [JsonProperty("safeEntryFingerprints", Order = 22)] public List<string> SafeEntryFingerprints { get; set; }
        [JsonProperty("catalogClassification", Order = 23)] public string CatalogClassification { get; set; }
        [JsonProperty("remainingEvidenceMissing", Order = 24)] public List<string> RemainingEvidenceMissing { get; set; }
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
            string catalogReadyPath = Path.Combine(evidenceDirectory, "runtime-catalog-ready.json");
            string catalogCapturedPath = Path.Combine(evidenceDirectory, "runtime-catalog-captured.json");
            string providerCapturedPath = Path.Combine(
                evidenceDirectory, "runtime-catalog-provider-captured.json");
            WriteAtomic(summaryPath, BuildSummary(result));
            result.EvidenceFiles = new List<string> { summaryPath, resultPath };
            if (File.Exists(readyPath)) result.EvidenceFiles.Add(readyPath);
            if (File.Exists(eventsPath)) result.EvidenceFiles.Add(eventsPath);
            if (File.Exists(catalogReadyPath)) result.EvidenceFiles.Add(catalogReadyPath);
            if (File.Exists(catalogCapturedPath)) result.EvidenceFiles.Add(catalogCapturedPath);
            if (File.Exists(providerCapturedPath)) result.EvidenceFiles.Add(providerCapturedPath);
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
