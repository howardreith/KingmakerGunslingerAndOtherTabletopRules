using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Items.Weapons;
using Newtonsoft.Json;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Development;
using KingmakerGunslinger.Firearms;
using KingmakerGunslinger.Ammunition;
using KingmakerGunslinger.Reloading;
using KingmakerGunslinger.Misfires;
using KingmakerGunslinger.Explosions;
using UnityEngine;
using UnityModManagerNet;
using Kingmaker.UnitLogic.FactLogic;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed class RuntimeTestRunner
    {
        private readonly RuntimeTestRequest _request;
        private readonly ModContext _context;
        private readonly Stopwatch _elapsed;
        private readonly DateTime _startedUtc;
        private readonly RuntimeObservationTraceWriter _trace;
        private Stopwatch _manualElapsed;
        private bool _completed;
        private ManualSaveLoadObservation _saveLoadObservation;
        private SaveCatalogSelectionObservation _catalogObservation;
        private SaveCatalogProviderObservation _catalogProviderObservation;
        private LoadGameButtonActionObservation _buttonActionObservation;
        private WorkingSaveSmokeScenario _workingSaveSmoke;
        private Stopwatch _catalogElapsed;
        private Stopwatch _selectionElapsed;
        private Stopwatch _completionElapsed;
        private bool _catalogMarkerWritten;
        private int _updateCallbackCount;
        private bool _workingReadyWritten;
        private string _workingStartupStage = "request-accepted";
        private static string _earlyEvidenceDirectory;
        private static RuntimeBuildIdentity _loadedBuildIdentity;

        private RuntimeTestRunner(RuntimeTestRequest request, ModContext context)
        {
            _request = request;
            _context = context;
            _startedUtc = DateTime.UtcNow;
            _elapsed = Stopwatch.StartNew();
            _trace = new RuntimeObservationTraceWriter(
                request.RunId, request.EvidenceDirectory, _elapsed);
            _trace.Record("request-accepted",
                "scenario=" + request.Scenario + "; loadedVersion=" +
                context.ModEntry.Info.Version);
            WriteLifecycleStage("request-argument-observed");
            WriteLifecycleStage("request-argument-seen");
            WriteLifecycleStage("request-path-normalized");
            WriteLifecycleStage("request-file-opened");
            WriteLifecycleStage("request-json-parsed");
            WriteLifecycleStage("request-schema-valid");
            WriteLifecycleStage("expected-version-valid");
            WriteLifecycleStage("scenario-allowlisted");
            WriteLifecycleStage("save-name-valid");
            WriteLifecycleStage("request-accepted");
            WriteLifecycleStage("runner-created");
        }

        internal static void TryAttach(ModContext context)
        {
            string loadedVersion = context.ModEntry.Info.Version;
            RuntimeTestRequestDecision decision = RuntimeTestRequestParser.TryActivate(
                Environment.GetCommandLineArgs(),
                loadedVersion);
            if (!decision.Accepted)
            {
                if (decision.ReasonCode != "flag-absent")
                {
                    context.Logger.Warning(
                        "runtime-test",
                        "request.rejected",
                        "reason=" + decision.ReasonCode +
                        "; requestFile=" + decision.SafeRequestName);
                    WriteRejectedRequest(context, decision);
                }
                return;
            }

            string marker = System.IO.Path.Combine(
                RuntimeTestRequest.EvidenceRoot,
                ".kmg-run-" + decision.Request.RunId);
            try
            {
                using (new System.IO.FileStream(
                    marker,
                    System.IO.FileMode.CreateNew,
                    System.IO.FileAccess.Write,
                    System.IO.FileShare.None))
                {
                }
            }
            catch (Exception exception)
            {
                context.Logger.Failure(
                    "runtime-test",
                    "request.claim-failed",
                    "The validated run ID could not be claimed.",
                    exception);
                return;
            }

            var runner = new RuntimeTestRunner(decision.Request, context);
            context.ModEntry.OnUpdate += runner.OnUpdate;
            context.Logger.Info(
                "runtime-test",
                "request.accepted",
                "runId=" + decision.Request.RunId +
                "; scenario=" + decision.Request.Scenario);
        }

        internal static void RecordEarlyIdentity(ModContext context)
        {
            string requestPath;
            string reason;
            if (!RuntimeTestRequestParser.TryGetRequestPath(
                Environment.GetCommandLineArgs(), out requestPath, out reason))
                return;
            try
            {
                string normalized = Path.GetFullPath(requestPath);
                string directory = Path.GetDirectoryName(normalized);
                string root = Path.GetFullPath(RuntimeTestRequest.EvidenceRoot)
                    .TrimEnd('\\');
                if (string.IsNullOrWhiteSpace(directory) ||
                    !directory.StartsWith(root + "\\",
                        StringComparison.OrdinalIgnoreCase) ||
                    !Directory.Exists(directory))
                    return;
                _earlyEvidenceDirectory = directory;
                _loadedBuildIdentity = RuntimeBuildIdentity.Capture(
                    context.Assembly, context.ModEntry.Info.Version);
                RuntimeTestResultWriter.WriteAtomic(
                    Path.Combine(directory, "runtime-loaded-build-identity.json"),
                    JsonConvert.SerializeObject(_loadedBuildIdentity,
                        Formatting.Indented) + Environment.NewLine);
                WriteEarlyStage(context, "request-argument-seen");
                WriteEarlyStage(context, "request-path-normalized");
            }
            catch (Exception exception)
            {
                context.Logger.Failure("runtime-test", "identity.write-failed",
                    "Guarded loaded-build identity could not be committed.", exception);
            }
        }

        private static void WriteEarlyStage(ModContext context, string stage)
        {
            if (string.IsNullOrWhiteSpace(_earlyEvidenceDirectory)) return;
            RuntimeTestResultWriter.WriteAtomic(
                Path.Combine(_earlyEvidenceDirectory,
                    "runtime-stage-" + stage + ".json"),
                JsonConvert.SerializeObject(new
                {
                    schemaVersion = 1,
                    stage = stage,
                    loadedBuildIdentity = _loadedBuildIdentity,
                    processId = Process.GetCurrentProcess().Id,
                    timestampUtc = DateTime.UtcNow.ToString("o")
                }, Formatting.Indented) + Environment.NewLine);
        }

        private static void WriteRejectedRequest(
            ModContext context, RuntimeTestRequestDecision decision)
        {
            if (string.IsNullOrWhiteSpace(_earlyEvidenceDirectory)) return;
            var result = new RuntimeTestResult
            {
                SchemaVersion = 1,
                RunId = string.Empty,
                Scenario = decision.RequestedScenario,
                Status = RuntimeTestStatuses.Error,
                LoadedModVersion = context.ModEntry.Info.Version,
                RuntimeIdentity = context.Assembly.FullName,
                GitCommit = _loadedBuildIdentity == null ? string.Empty :
                    _loadedBuildIdentity.GitCommit,
                GameVersion = Application.version ?? string.Empty,
                StartUtc = DateTime.UtcNow.ToString("o"),
                EndUtc = DateTime.UtcNow.ToString("o"),
                Assertions = new List<RuntimeTestAssertion>(),
                Diagnostics = new List<string>
                {
                    "rejectionStage=" + decision.FailedStage,
                    "sanitizedReason=" + decision.ReasonCode,
                    "hookInstalled=false",
                    "uiActionOccurred=false",
                    "saveActionOccurred=false"
                },
                Warnings = new List<string>(),
                ExceptionSummary = string.Empty,
                EvidenceFiles = new List<string>(),
                AutomaticExitRequested = false,
                AutomaticExitInitiated = false
            };
            WriteEarlyStage(context, decision.FailedStage);
            RuntimeTestResultWriter.Write(result, _earlyEvidenceDirectory);
        }

        private void OnUpdate(UnityModManager.ModEntry modEntry, float deltaTime)
        {
            if (_completed) return;
            try
            {
                _updateCallbackCount++;
                if (_updateCallbackCount == 1)
                {
                    WriteLifecycleStage("onupdate-entered");
                    WriteLifecycleStage("runner-onupdate-entered");
                }
                if (_manualElapsed == null &&
                    _elapsed.Elapsed.TotalSeconds >= _request.StartupTimeoutSeconds)
                {
                    _trace.Record("startup-timeout",
                        "stage=" + _workingStartupStage + "; observer was not ready");
                    RuntimeTestResult startupTimeout = CreateResult("TIMEOUT", null, null);
                    startupTimeout.Diagnostics.Add("timeoutStage=" + _workingStartupStage);
                    Complete(startupTimeout);
                    return;
                }
                if (_manualElapsed != null &&
                    _request.Scenario != RuntimeTestScenarioCatalog.ObserveSaveCatalogAndSelection &&
                    _request.Scenario != RuntimeTestScenarioCatalog.ObserveSaveCatalogProvider &&
                    _request.Scenario != RuntimeTestScenarioCatalog.ObserveLoadGameButtonAction &&
                    _request.Scenario != RuntimeTestScenarioCatalog.WorkingSaveSmoke &&
                    _request.Scenario != RuntimeTestScenarioCatalog.GenericFirearmActions &&
                    _request.Scenario != RuntimeTestScenarioCatalog.ProductionFirearmCatalog &&
                    _request.Scenario != RuntimeTestScenarioCatalog.AdvancedCapacity &&
                    _request.Scenario != RuntimeTestScenarioCatalog.ObserveWorkingSaveEntryAction &&
                    _request.Scenario != RuntimeTestScenarioCatalog.ObserveWorkingSaveSelectionLoadAction &&
                    _request.Scenario != RuntimeTestScenarioCatalog.ObserveWorkingSaveReceiverBoundAction &&
                    _manualElapsed.Elapsed.TotalSeconds >= _request.TimeoutSeconds)
                {
                    _trace.Record("manual-interaction-timeout",
                        "stage=manual-save-load-observation");
                    RuntimeTestResult interactionTimeout = CreateResult("TIMEOUT", null, null);
                    interactionTimeout.Diagnostics.Add("timeoutStage=manual-save-load-observation");
                    Complete(interactionTimeout);
                    return;
                }
                if (!_context.IsReady) return;
                if (_request.Scenario == RuntimeTestScenarioCatalog.ModLoadSmoke)
                {
                    Complete(RunModLoadSmoke());
                    return;
                }
                if (_request.Scenario ==
                    RuntimeTestScenarioCatalog.ObserveClassBlueprintContracts)
                {
                    Complete(RunClassBlueprintContractObservation());
                    return;
                }
                if (_request.Scenario ==
                    RuntimeTestScenarioCatalog.ObserveSaveCatalogAndSelection)
                {
                    RunSaveCatalogObservation();
                    return;
                }
                if (_request.Scenario ==
                    RuntimeTestScenarioCatalog.ObserveSaveCatalogProvider)
                {
                    RunSaveCatalogProviderObservation();
                    return;
                }
                if (_request.Scenario ==
                    RuntimeTestScenarioCatalog.ObserveLoadGameButtonAction)
                {
                    RunLoadGameButtonActionObservation();
                    return;
                }
                if (_request.Scenario == RuntimeTestScenarioCatalog.WorkingSaveSmoke ||
                    _request.Scenario == RuntimeTestScenarioCatalog.GenericFirearmActions ||
                    _request.Scenario == RuntimeTestScenarioCatalog.ProductionFirearmCatalog ||
                    _request.Scenario == RuntimeTestScenarioCatalog.AdvancedCapacity ||
                    _request.Scenario ==
                        RuntimeTestScenarioCatalog.ObserveWorkingSaveEntryAction ||
                    _request.Scenario ==
                        RuntimeTestScenarioCatalog.ObserveWorkingSaveSelectionLoadAction ||
                    _request.Scenario ==
                        RuntimeTestScenarioCatalog.ObserveWorkingSaveReceiverBoundAction)
                {
                    RunWorkingSaveSmoke();
                    return;
                }
                RunManualSaveLoadObservation();
            }
            catch (Exception exception)
            {
                if ((_request.Scenario == RuntimeTestScenarioCatalog.WorkingSaveSmoke ||
                    _request.Scenario == RuntimeTestScenarioCatalog.GenericFirearmActions ||
                    _request.Scenario == RuntimeTestScenarioCatalog.ProductionFirearmCatalog ||
                    _request.Scenario == RuntimeTestScenarioCatalog.AdvancedCapacity ||
                    _request.Scenario ==
                        RuntimeTestScenarioCatalog.ObserveWorkingSaveEntryAction ||
                    _request.Scenario ==
                        RuntimeTestScenarioCatalog.ObserveWorkingSaveSelectionLoadAction ||
                    _request.Scenario ==
                        RuntimeTestScenarioCatalog.ObserveWorkingSaveReceiverBoundAction) &&
                    _workingReadyWritten && _workingSaveSmoke != null)
                    CompletePostReadinessError(exception);
                else
                    CompleteStartupError(_workingSaveSmoke != null &&
                        !string.IsNullOrWhiteSpace(_workingSaveSmoke.ObserverArmingSubstage)
                            ? _workingSaveSmoke.ObserverArmingSubstage
                            : _workingStartupStage, exception);
            }
        }

        private void RunWorkingSaveSmoke()
        {
            if (_workingSaveSmoke == null)
            {
                _workingStartupStage = "scenario-selected";
                WriteLifecycleStage(_workingStartupStage);
                _trace.Record("scenario-activated",
                    _request.Scenario);
                _workingSaveSmoke = new WorkingSaveSmokeScenario(
                    _context, _elapsed, _request.RunId, _trace.Record,
                    _request.Scenario ==
                        RuntimeTestScenarioCatalog.ObserveWorkingSaveEntryAction ||
                    _request.Scenario ==
                        RuntimeTestScenarioCatalog.ObserveWorkingSaveSelectionLoadAction ||
                    _request.Scenario ==
                        RuntimeTestScenarioCatalog.ObserveWorkingSaveReceiverBoundAction,
                    _request.Scenario ==
                        RuntimeTestScenarioCatalog.ObserveWorkingSaveSelectionLoadAction,
                    _request.Scenario ==
                        RuntimeTestScenarioCatalog.ObserveWorkingSaveReceiverBoundAction);
                _workingStartupStage = "hooks-install-start";
                WriteLifecycleStage(_workingStartupStage);
                _workingSaveSmoke.Install();
                _workingStartupStage = "hooks-install-complete";
                WriteLifecycleStage(_workingStartupStage);
                _manualElapsed = Stopwatch.StartNew();
                return;
            }
            if (_workingSaveSmoke.ScenarioException != null)
                throw new InvalidOperationException(
                    "A request-scoped observation hook failed; original game behavior was preserved.",
                    _workingSaveSmoke.ScenarioException);
            if (_workingSaveSmoke.Stage == "main-menu-readiness" &&
                _workingStartupStage != "main-menu-search-start")
            {
                _workingStartupStage = "main-menu-search-start";
                WriteLifecycleStage(_workingStartupStage);
            }
            _workingSaveSmoke.Poll();
            if (_workingSaveSmoke.Stage == "load-game-action-resolution" &&
                _workingStartupStage != "main-menu-ready")
            {
                _workingStartupStage = "main-menu-ready";
                WriteLifecycleStage(_workingStartupStage);
            }
            bool supervisedEntry = _request.Scenario ==
                RuntimeTestScenarioCatalog.ObserveWorkingSaveEntryAction ||
                _request.Scenario ==
                    RuntimeTestScenarioCatalog.ObserveWorkingSaveSelectionLoadAction ||
                _request.Scenario ==
                    RuntimeTestScenarioCatalog.ObserveWorkingSaveReceiverBoundAction;
            if (supervisedEntry && _workingSaveSmoke.MainMenuReady &&
                _workingStartupStage != "observer-armed")
            {
                _workingStartupStage = "observer-armed";
                WriteLifecycleStage(_workingStartupStage);
            }
            bool requestedReadiness = supervisedEntry
                ? _workingSaveSmoke.WorkingEntryReady
                : _workingSaveSmoke.MainMenuReady;
            if (!_workingReadyWritten && requestedReadiness &&
                _updateCallbackCount >= 2)
            {
                _workingStartupStage = _workingSaveSmoke.ReceiverBoundObservation
                    ? "working-receiver-bound-action-ready"
                    : supervisedEntry ? "working-entry-ready"
                    : "load-game-action-resolved";
                WriteLifecycleStage(_workingStartupStage);
                _trace.Record("runtime-ready",
                    "runner active; update callbacks continuing; exact main-menu root active; UMM overlay is not used as readiness");
                _trace.WriteReady(new RuntimeReadyMarker
                {
                    SchemaVersion = 1, RunId = _request.RunId,
                    Scenario = _request.Scenario,
                    LoadedModVersion = _context.ModEntry.Info.Version,
                    RuntimeIdentity = _context.Assembly.FullName,
                    ReadinessTimestampUtc = DateTime.UtcNow.ToString("o"),
                    InstalledObservationHookIdentifiers =
                        _workingSaveSmoke.HookIdentifiers,
                    ProcessId = Process.GetCurrentProcess().Id,
                    RuntimeRunnerActive = true,
                    UpdateCallbackCount = _updateCallbackCount,
                    MainMenuLifecycleReady = true,
                    UmmStartupState = "initialized; overlay nonblocking-or-absent",
                    ReadinessStage = _workingStartupStage,
                    SaveName = supervisedEntry
                        ? WorkingSaveSmokeScenario.ExpectedName : "",
                    ExactSlotIdentity = _workingSaveSmoke.ReceiverBoundObservation
                        ? _workingSaveSmoke.ExactSlotIdentity : "",
                    ExactWindowIdentity = _workingSaveSmoke.ReceiverBoundObservation
                        ? _workingSaveSmoke.ExactWindowIdentity : ""
                });
                _workingStartupStage = "working-save-ready";
                WriteLifecycleStage(_workingStartupStage);
                _workingReadyWritten = true;
                return;
            }
            if (!_workingReadyWritten)
            {
                if (_workingSaveSmoke.ReceiverBoundScopeResolutionFailed)
                {
                    bool multipleReceivers =
                        _workingSaveSmoke.EntryCandidateCount > 1 ||
                        _workingSaveSmoke.EntryActionCandidateCount > 1;
                    CompleteWorkingSaveSmoke(multipleReceivers
                            ? RuntimeTestStatuses.Ambiguous
                            : RuntimeTestStatuses.Fail,
                        "receiver-bound-readiness",
                        "The exact working SaveSlot or owning SaveLoadWindow had zero or multiple matches.");
                    return;
                }
                int readinessTimeout = WorkingSaveStageTimeout(
                    _workingSaveSmoke.Stage);
                if (_workingSaveSmoke.StageElapsedMilliseconds >=
                    readinessTimeout * 1000L)
                    CompleteWorkingSaveSmoke(RuntimeTestStatuses.Timeout,
                        _workingSaveSmoke.Stage,
                        "Stage-specific startup timeout expired.");
                return;
            }
            if (_workingSaveSmoke.WriteObserved)
            {
                CompleteWorkingSaveSmoke(RuntimeTestStatuses.Fail,
                    "unexpected-save-write",
                    "A native save-writing or migration method was observed.");
                return;
            }
            if ((supervisedEntry || _workingSaveSmoke.AutonomousReceiverBoundAction) &&
                (_workingSaveSmoke.BaselineLoadObserved ||
                _workingSaveSmoke.OtherLoadObserved))
            {
                CompleteWorkingSaveSmoke(RuntimeTestStatuses.Fail,
                    "forbidden-save-selection",
                    _workingSaveSmoke.BaselineLoadObserved
                        ? "The baseline descriptor entered MainMenu.LoadGame."
                        : "A descriptor other than the working save entered MainMenu.LoadGame.");
                return;
            }
            if (_workingSaveSmoke.ButtonCandidateCount > 1)
            {
                CompleteWorkingSaveSmoke(RuntimeTestStatuses.Ambiguous,
                    "load-game-action-resolution",
                    "Multiple exact Load Game action candidates were resolved.");
                return;
            }
            if (_workingSaveSmoke.WorkingCount > 1)
            {
                CompleteWorkingSaveSmoke(RuntimeTestStatuses.Ambiguous,
                    "descriptor-resolution",
                    "Multiple exact working descriptors were captured.");
                return;
            }
            if ((supervisedEntry || _workingSaveSmoke.AutonomousReceiverBoundAction) &&
                !_workingSaveSmoke.SelectionLoadObservation &&
                (_workingSaveSmoke.EntryCandidateCount > 1 ||
                 _workingSaveSmoke.EntryActionCandidateCount > 1))
            {
                CompleteWorkingSaveSmoke(RuntimeTestStatuses.Ambiguous,
                    "entry-action-resolution",
                    "Multiple working entries or entry actions matched.");
                return;
            }
            if (_workingSaveSmoke.Stage == "descriptor-resolution" &&
                _workingSaveSmoke.CatalogComplete)
            {
                if (_workingSaveSmoke.WorkingCount == 0)
                {
                    CompleteWorkingSaveSmoke(RuntimeTestStatuses.Fail,
                        "descriptor-resolution",
                        "No exact working descriptor was captured.");
                    return;
                }
                if (_workingSaveSmoke.BaselineCount == 0)
                {
                    CompleteWorkingSaveSmoke(RuntimeTestStatuses.Fail,
                        "descriptor-resolution",
                        "The baseline descriptor could not be distinguished.");
                    return;
                }
                if (_workingSaveSmoke.BaselineCount > 1)
                {
                    CompleteWorkingSaveSmoke(RuntimeTestStatuses.Ambiguous,
                        "descriptor-resolution",
                        "Multiple exact baseline descriptors were captured.");
                    return;
                }
            }
            if (_workingSaveSmoke.Complete)
            {
                if (_request.Scenario ==
                    RuntimeTestScenarioCatalog.GenericFirearmActions)
                {
                    RunSprint30GenericActions();
                }
                else if (_request.Scenario ==
                    RuntimeTestScenarioCatalog.ProductionFirearmCatalog)
                {
                    RunSprint31ProductionFirearmCatalog();
                }
                else if (_request.Scenario == RuntimeTestScenarioCatalog.AdvancedCapacity)
                {
                    RunSprint33AdvancedCapacity();
                }
                else
                {
                    CompleteWorkingSaveSmoke(RuntimeTestStatuses.Pass, "", "");
                }
                return;
            }
            if (supervisedEntry && _workingSaveSmoke.ObservationComplete)
            {
                CompleteWorkingSaveSmoke(RuntimeTestStatuses.Ambiguous,
                    "entry-action-correlation",
                    _workingSaveSmoke.ReceiverBoundObservation
                        ? "Load completed, but exact receiver references, invocation counts, or strict ordering were not uniquely proven."
                        : _workingSaveSmoke.SelectionLoadObservation
                        ? "Load completed for the exact working descriptor, but one caller and one compatible scoped receiver were not uniquely proven."
                        : "Load completed for the exact working descriptor, but the clicked UnityEvent and listener could not both be proven.");
                return;
            }
            int timeout = WorkingSaveStageTimeout(_workingSaveSmoke.Stage);
            if (_workingSaveSmoke.StageElapsedMilliseconds >= timeout * 1000L)
            {
                string status = _workingSaveSmoke.Stage == "descriptor-resolution" &&
                    _workingSaveSmoke.WorkingCount > 1
                    ? RuntimeTestStatuses.Ambiguous :
                    _workingSaveSmoke.ReceiverBoundObservation &&
                    (_workingSaveSmoke.Stage == "working-entry-click" ||
                     _workingSaveSmoke.Stage == "slot-action-invocation" ||
                     _workingSaveSmoke.Stage == "window-handler-invocation" ||
                     _workingSaveSmoke.Stage == "load-entry-invocation")
                        ? RuntimeTestStatuses.Ambiguous
                        : RuntimeTestStatuses.Timeout;
                CompleteWorkingSaveSmoke(status, _workingSaveSmoke.Stage,
                    "Stage-specific timeout expired.");
            }
        }

        private int WorkingSaveStageTimeout(string stage)
        {
            if (stage == "main-menu-readiness") return _request.MainMenuTimeoutSeconds;
            if (stage == "load-game-action-resolution")
                return _request.ActionResolutionTimeoutSeconds;
            if (stage == "action-invocation")
                return _request.ActionInvocationTimeoutSeconds;
            if (stage == "catalog-initialization")
                return _request.CatalogTimeoutSeconds;
            if (stage == "descriptor-resolution")
                return _request.DescriptorResolutionTimeoutSeconds;
            if (stage == "working-entry-readiness")
                return _request.DescriptorResolutionTimeoutSeconds;
            if (stage == "working-entry-click")
                return _request.SelectionTimeoutSeconds;
            if (stage == "receiver-bound-action-invocation")
                return _request.ActionInvocationTimeoutSeconds;
            if (stage == "slot-action-invocation" ||
                stage == "window-handler-invocation")
                return _request.LoadEntryTimeoutSeconds;
            if (stage == "load-entry-invocation")
                return _request.LoadEntryTimeoutSeconds;
            if (stage == "load-completion")
                return _request.CompletionTimeoutSeconds;
            if (stage == "post-load-fingerprint")
                return _request.FingerprintTimeoutSeconds;
            return _request.TimeoutSeconds;
        }

        private void WriteLifecycleStage(string stage)
        {
            _trace.WriteStage("runtime-stage-" + stage + ".json",
                new RuntimeStageMarker
                {
                    SchemaVersion = 1,
                    RunId = _request.RunId,
                    Scenario = _request.Scenario,
                    Stage = stage,
                    LoadedModVersion = _context.ModEntry.Info.Version,
                    TimestampUtc = DateTime.UtcNow.ToString("o"),
                    ProcessId = Process.GetCurrentProcess().Id
                });
            _trace.Record(stage, "atomic lifecycle stage marker committed");
        }

        private void CompleteStartupError(string stage, Exception exception)
        {
            _workingStartupStage = "startup-error";
            try { WriteLifecycleStage(_workingStartupStage); }
            catch (Exception markerException)
            {
                exception = new AggregateException(
                    "Startup failed and the startup-error marker could not be committed.",
                    exception, markerException);
            }
            _trace.Record("runtime-exception", "stage=" + stage, exception);
            RuntimeTestResult result = CreateResult(
                RuntimeTestStatuses.Error, null, ExceptionSummary(exception));
            result.Diagnostics.Add("startupErrorStage=" + stage);
            result.ErrorStage = _workingSaveSmoke == null
                ? stage : _workingSaveSmoke.Stage;
            result.LastCompletedStage = _workingSaveSmoke == null
                ? "" : _workingSaveSmoke.LastCompletedStage;
            result.ExceptionType = exception.GetType().FullName;
            result.ExceptionMessage = exception.Message;
            result.ExceptionStack = SanitizeExceptionStack(exception);
            result.ExceptionManagedThreadId =
                System.Threading.Thread.CurrentThread.ManagedThreadId;
            if (_workingSaveSmoke != null)
                result.WorkingSaveSmoke = _workingSaveSmoke.Stop();
            Complete(result);
        }

        private void CompletePostReadinessError(Exception exception)
        {
            string stage = _workingSaveSmoke.Stage;
            try { WriteLifecycleStage("post-readiness-error"); }
            catch (Exception markerException)
            {
                exception = new AggregateException(
                    "Post-readiness execution failed and its marker could not be committed.",
                    exception, markerException);
            }
            _trace.Record("post-readiness-error",
                "stage=" + stage + ";lastCompletedStage=" +
                _workingSaveSmoke.LastCompletedStage, exception);
            RuntimeTestResult result = CreateResult(
                RuntimeTestStatuses.Error, null, ExceptionSummary(exception));
            result.ErrorStage = stage;
            result.LastCompletedStage = _workingSaveSmoke.LastCompletedStage;
            result.ExceptionType = exception.GetType().FullName;
            result.ExceptionMessage = exception.Message;
            result.ExceptionStack = SanitizeExceptionStack(exception);
            result.ExceptionManagedThreadId =
                System.Threading.Thread.CurrentThread.ManagedThreadId;
            result.WorkingSaveSmoke = _workingSaveSmoke.Stop();
            Complete(result);
        }

        private string SanitizeExceptionStack(Exception exception)
        {
            string value = exception == null ? "" : exception.ToString();
            if (_request != null &&
                !string.IsNullOrWhiteSpace(_request.EvidenceDirectory))
                value = value.Replace(
                    _request.EvidenceDirectory, "<evidence-directory>");
            string user = Environment.UserName;
            if (!string.IsNullOrWhiteSpace(user))
                value = value.Replace(user, "<user>");
            return value;
        }

        private void CompleteWorkingSaveSmoke(
            string status, string stage, string warning)
        {
            WorkingSaveSmokeEvidence evidence = _workingSaveSmoke.Stop();
            var assertions = new List<RuntimeTestAssertion>
            {
                Assertion("exact-ui-action", "one exact onClick invocation",
                    "candidates=" + evidence.ButtonCandidateCount +
                        ";onClick=" + evidence.ButtonEventInvocationCount +
                        ";handler=" + evidence.HandlerInvocationCount,
                    evidence.ButtonCandidateCount == 1 &&
                        evidence.ButtonEventInvocationCount == 1 &&
                        evidence.HandlerInvocationCount == 1,
                    "observed button hierarchy, components, and listeners"),
                Assertion("complete-catalog", "one Initialize with complete List<SaveInfo>",
                    "initialize=" + evidence.CatalogInitializeCount +
                        ";count=" + evidence.CatalogDescriptorCount +
                        ";complete=" + evidence.CatalogComplete,
                    evidence.CatalogInitializeCount == 1 && evidence.CatalogComplete,
                    "exact ListOfSaves.Initialize argument"),
                Assertion("unique-working-and-distinct-baseline", "working=1;baseline=1",
                    "working=" + evidence.WorkingMatchCount +
                        ";baseline=" + evidence.BaselineMatchCount,
                    evidence.WorkingMatchCount == 1 &&
                        evidence.BaselineMatchCount == 1,
                    "stable descriptor identity combinations"),
                Assertion("descriptor-load-correlation", "same object reference",
                    evidence.DescriptorReferenceCorrelated ? "correlated" : "not-correlated",
                    evidence.DescriptorReferenceCorrelated &&
                        evidence.LoadEntryInvocationCount == 1,
                    "catalog entry to MainMenu.LoadGame argument"),
                Assertion("load-completion-and-fingerprint",
                    "after-load callback and stable expected fingerprint",
                    "callback=" + evidence.CompletionCallbackObserved +
                        ";fingerprint=" + evidence.StableFingerprint,
                    evidence.CompletionCallbackObserved &&
                        !string.IsNullOrWhiteSpace(evidence.StableFingerprint),
                    "SaveManager callback and two stable game-thread samples"),
                Assertion("no-save-writing-api", "none",
                    evidence.SaveWritingApiObserved ? "observed" : "none",
                    !evidence.SaveWritingApiObserved,
                    "request-scoped native save-write sentinels"),
                Assertion("loaded-mod-version", _request.ExpectedModVersion,
                    _context.ModEntry.Info.Version,
                    _context.ModEntry.Info.Version == _request.ExpectedModVersion,
                    "Unity Mod Manager ModEntry.Info.Version")
            };
            RuntimeTestResult result = CreateResult(status, assertions, null);
            bool supervisedEntry = _request.Scenario ==
                RuntimeTestScenarioCatalog.ObserveWorkingSaveEntryAction;
            bool selectionLoadObservation = _request.Scenario ==
                RuntimeTestScenarioCatalog.ObserveWorkingSaveSelectionLoadAction;
            bool receiverBoundObservation = _request.Scenario ==
                RuntimeTestScenarioCatalog.ObserveWorkingSaveReceiverBoundAction;
            bool receiverBoundPath = receiverBoundObservation ||
                _request.Scenario == RuntimeTestScenarioCatalog.WorkingSaveSmoke ||
                _request.Scenario == RuntimeTestScenarioCatalog.GenericFirearmActions ||
                _request.Scenario == RuntimeTestScenarioCatalog.ProductionFirearmCatalog;
            receiverBoundPath = receiverBoundPath ||
                _request.Scenario == RuntimeTestScenarioCatalog.AdvancedCapacity;
            if (receiverBoundPath)
            {
                result.WorkingSaveReceiverBoundActionObservation = evidence;
                assertions.Add(Assertion("unique-working-slot",
                    "one exact object-reference-correlated SaveSlot",
                    "entries=" + evidence.EntryCandidateCount,
                    evidence.EntryCandidateCount == 1,
                    "exact captured working SaveInfo reference"));
                assertions.Add(Assertion("exact-receiver-bound-path",
                    "one exact slot, window, and load invocation",
                    "slot=" + evidence.SlotActionInvocationCount +
                        ";window=" + evidence.WindowHandlerInvocationCount +
                        ";load=" + evidence.LoadEntryInvocationCount,
                    evidence.SlotActionInvocationCount == 1 &&
                        evidence.WindowHandlerInvocationCount == 1 &&
                        evidence.LoadEntryInvocationCount == 1 &&
                        evidence.SlotReceiverReferenceCorrelated &&
                        evidence.WindowReceiverReferenceCorrelated &&
                        evidence.WindowArgumentReferenceCorrelated,
                    "exact receiver and SaveInfo object references"));
                assertions.Add(Assertion("strict-receiver-bound-order",
                    "slot < window < load < callback < fingerprint",
                    evidence.SlotActionSequence + "<" +
                        evidence.WindowHandlerSequence + "<" +
                        evidence.LoadEntrySequence + "<" +
                        evidence.CompletionSequence + "<" +
                        evidence.FingerprintSequence,
                    evidence.SlotActionSequence > 0 &&
                        evidence.SlotActionSequence < evidence.WindowHandlerSequence &&
                        evidence.WindowHandlerSequence < evidence.LoadEntrySequence &&
                        evidence.LoadEntrySequence < evidence.CompletionSequence &&
                        evidence.CompletionSequence < evidence.FingerprintSequence,
                    "monotonic request-scoped event sequence"));
                assertions.Add(Assertion(receiverBoundObservation
                        ? "probe-non-initiating" : "autonomous-action-invoked",
                    receiverBoundObservation ? "false" : "true",
                    evidence.ProbeInvokedEntryAction.ToString(),
                    receiverBoundObservation
                        ? !evidence.ProbeInvokedEntryAction
                        : evidence.ProbeInvokedEntryAction,
                    receiverBoundObservation
                        ? "observer invokes neither selection nor loading"
                        : "guarded autonomous scenario invokes the exact receiver-bound action"));
            }
            else if (selectionLoadObservation)
            {
                result.WorkingSaveSelectionLoadActionObservation = evidence;
                assertions.Add(Assertion("unique-working-slot",
                    "one exact object-reference-correlated SaveSlot",
                    "entries=" + evidence.EntryCandidateCount,
                    evidence.EntryCandidateCount == 1,
                    "exact captured working SaveInfo reference"));
                assertions.Add(Assertion("selected-working-state",
                    "exact working SaveInfo in selected-save storage",
                    evidence.SelectedWorkingStateObserved ? "correlated" : "not-correlated",
                    evidence.SelectedWorkingStateObserved,
                    "field reference identity on exact scoped owner"));
                assertions.Add(Assertion("unique-final-load-action",
                    "one exact caller into MainMenu.LoadGame",
                    "count=" + evidence.FinalLoadActionCount + ";caller=" +
                        evidence.ImmediateLoadCaller,
                    evidence.FinalLoadActionCount == 1 &&
                        evidence.CompatibleCallerReceiverCount == 1 &&
                        !string.IsNullOrWhiteSpace(evidence.ImmediateLoadCaller) &&
                        !string.IsNullOrWhiteSpace(
                            evidence.ImmediateLoadCallerReceiverIdentity),
                    "managed caller chain and scoped receiver identity"));
                assertions.Add(Assertion("visible-text-not-identity",
                    "no text-only correlation", "object-reference and receiver identity",
                    true, "labels are supporting evidence only"));
                assertions.Add(Assertion("observer-non-initiating",
                    "false", evidence.ProbeInvokedEntryAction.ToString(),
                    !evidence.ProbeInvokedEntryAction,
                    "observer invokes neither selection nor loading"));
            }
            else if (supervisedEntry)
            {
                result.WorkingSaveEntryActionObservation = evidence;
                assertions.Add(Assertion("unique-working-entry",
                    "one object-reference-correlated UI entry",
                    "entries=" + evidence.EntryCandidateCount,
                    evidence.EntryCandidateCount == 1,
                    "component field reference equals exact catalog SaveInfo"));
                assertions.Add(Assertion("unique-working-entry-action",
                    "one active interactable action within exact entry",
                    "actions=" + evidence.EntryActionCandidateCount,
                    evidence.EntryActionCandidateCount == 1,
                    "entry-local UnityEvent runtime delegate captures descriptor owner"));
                assertions.Add(Assertion("human-action-and-listener",
                    "exact action once and exact listener once",
                    "action=" + evidence.HumanActionInvocationCount +
                        ";listener=" + evidence.ListenerInvocationCount,
                    evidence.HumanActionInvocationCount == 1 &&
                        evidence.ListenerInvocationCount == 1 &&
                        !string.IsNullOrWhiteSpace(evidence.ListenerMethod),
                    "passive Harmony entry and listener observation"));
                assertions.Add(Assertion("probe-never-invoked-entry-action",
                    "false", evidence.ProbeInvokedEntryAction.ToString(),
                    !evidence.ProbeInvokedEntryAction,
                    "observer contains no entry-action invocation"));
            }
            else result.WorkingSaveSmoke = evidence;
            if (!string.IsNullOrWhiteSpace(stage))
                result.Diagnostics.Add("timeoutStage=" + stage);
            if (!string.IsNullOrWhiteSpace(warning)) result.Warnings.Add(warning);
            Complete(result);
        }

        private void RunSprint30GenericActions()
        {
            _trace.Record("feature-acceptance-start",
                "sprint30 generic marker-first maintenance actions; in-memory only");
            DevelopmentActionResult maintenance =
                DevelopmentControls.RunMaintenanceQualificationImmediately();
            bool completeLoop = maintenance.Succeeded &&
                maintenance.Message.IndexOf("MaintenanceLoopPassed",
                    StringComparison.Ordinal) >= 0;
            BlueprintWeaponType nativeHeavyCrossbow =
                BlueprintBootstrap.NativeHeavyCrossbowWeaponType;
            BlueprintWeaponType markedTestMusket =
                BlueprintBootstrap.TestMusketWeaponType;
            int nativeMarkerCount = CountFirearmMarkers(nativeHeavyCrossbow);
            int markedMarkerCount = CountFirearmMarkers(markedTestMusket);

            WorkingSaveSmokeEvidence evidence = _workingSaveSmoke.Stop();
            var assertions = new List<RuntimeTestAssertion>
            {
                Assertion("exact-working-save", "stable exact working-save load",
                    evidence.StableFingerprint,
                    evidence.CompletionCallbackObserved &&
                        evidence.DescriptorReferenceCorrelated &&
                        !string.IsNullOrWhiteSpace(evidence.StableFingerprint),
                    "qualified receiver-bound working-save path"),
                Assertion("generic-maintenance-loop", "MaintenanceLoopPassed",
                    maintenance.Message, completeLoop,
                    "definition-driven exact-equipped Overhaul, Repair, and Reload diagnostics"),
                Assertion("native-heavy-crossbow-isolation",
                    "nativeMarkers=0;markedMarkers=1",
                    "nativeMarkers=" + nativeMarkerCount +
                        ";markedMarkers=" + markedMarkerCount,
                    nativeHeavyCrossbow != null && markedTestMusket != null &&
                        !ReferenceEquals(nativeHeavyCrossbow, markedTestMusket) &&
                        nativeMarkerCount == 0 && markedMarkerCount == 1,
                    "concrete runtime BlueprintWeaponType component arrays"),
                Assertion("no-save-writing-api", "none",
                    evidence.SaveWritingApiObserved ? "observed" : "none",
                    !evidence.SaveWritingApiObserved,
                    "request-scoped native save-write sentinels"),
                Assertion("loaded-mod-version", _request.ExpectedModVersion,
                    _context.ModEntry.Info.Version,
                    _request.ExpectedModVersion == _context.ModEntry.Info.Version,
                    "Unity Mod Manager ModEntry.Info.Version")
            };
            bool pass = assertions.TrueForAll(value => value.Status == "PASS");
            RuntimeTestResult result = CreateResult(
                pass ? RuntimeTestStatuses.Pass : RuntimeTestStatuses.Fail,
                assertions, null);
            result.WorkingSaveSmoke = evidence;
            if (!maintenance.Succeeded)
                result.Warnings.Add("Sprint 30 maintenance diagnostic failed closed.");
            _trace.Record("feature-acceptance-complete",
                "status=" + result.Status);
            Complete(result);
        }

        private void RunSprint31ProductionFirearmCatalog()
        {
            _trace.Record("feature-acceptance-start",
                "sprint31 production firearm catalog; blueprint observation only");
            ProductionFirearmBlueprintCatalog catalog =
                BlueprintBootstrap.ProductionFirearms;
            bool catalogValidated = false;
            string validation = "catalog unavailable";
            if (catalog != null)
            {
                try
                {
                    ProductionFirearmBlueprints.Validate(
                        catalog,
                        WeaponBlueprintAccess.Resolve(),
                        WeaponTypeMechanicalAccess.Resolve());
                    catalogValidated = true;
                    validation = "five distinct entries; ten registered blueprints";
                }
                catch (Exception exception)
                {
                    validation = exception.GetType().Name + ": " + exception.Message;
                }
            }

            int nativeHeavyMarkers = CountFirearmMarkers(
                BlueprintBootstrap.NativeHeavyCrossbowWeaponType);
            int pistolMarkers = catalog == null ? -1 :
                CountFirearmMarkers(catalog.Pistol.WeaponType);
            int musketMarkers = catalog == null ? -1 :
                CountFirearmMarkers(catalog.Musket.WeaponType);
            int blunderbussMarkers = catalog == null ? -1 :
                CountFirearmMarkers(catalog.Blunderbuss.WeaponType);
            int rifleMarkers = catalog == null ? -1 :
                CountFirearmMarkers(catalog.AdvancedRifle.WeaponType);
            int revolverMarkers = catalog == null ? -1 :
                CountFirearmMarkers(catalog.AdvancedRevolver.WeaponType);
            int blunderbussUnavailable = catalog == null ? -1 :
                (catalog.Blunderbuss.Item.ComponentsArray ??
                    Array.Empty<BlueprintComponent>())
                .OfType<UnavailableProductionFirearmRestriction>().Count();

            WorkingSaveSmokeEvidence evidence = _workingSaveSmoke.Stop();
            var assertions = new List<RuntimeTestAssertion>
            {
                Assertion("exact-working-save", "stable exact working-save load",
                    evidence.StableFingerprint,
                    evidence.CompletionCallbackObserved &&
                        evidence.DescriptorReferenceCorrelated &&
                        !string.IsNullOrWhiteSpace(evidence.StableFingerprint),
                    "qualified receiver-bound working-save path"),
                Assertion("production-catalog-runtime-contract",
                    "five exact entries; count=10", validation,
                    catalogValidated && catalog.Count == 10 &&
                        catalog.Pistol.Spec.Equals(ProductionFirearmCatalog.CreatePistol()) &&
                        catalog.Musket.Spec.Equals(ProductionFirearmCatalog.CreateMusket()) &&
                        catalog.Blunderbuss.Spec.Equals(
                            ProductionFirearmCatalog.CreateBlunderbuss()) &&
                        catalog.AdvancedRifle.Spec.Equals(
                            ProductionFirearmCatalog.CreateAdvancedRifle()) &&
                        catalog.AdvancedRevolver.Spec.Equals(
                            ProductionFirearmCatalog.CreateAdvancedRevolver()),
                    "concrete registered runtime blueprints and exact mechanical access"),
                Assertion("marker-and-native-source-isolation",
                    "nativeHeavy=0;pistol=1;musket=1;blunderbuss=1;rifle=1;revolver=1",
                    "nativeHeavy=" + nativeHeavyMarkers +
                        ";pistol=" + pistolMarkers + ";musket=" + musketMarkers +
                        ";blunderbuss=" + blunderbussMarkers +
                        ";rifle=" + rifleMarkers + ";revolver=" + revolverMarkers,
                    nativeHeavyMarkers == 0 &&
                        pistolMarkers == 1 && musketMarkers == 1 &&
                        blunderbussMarkers == 1 && rifleMarkers == 1 && revolverMarkers == 1,
                    "concrete BlueprintWeaponType component arrays"),
                Assertion("special-range-fails-closed", "unavailableRestrictions=1",
                    "unavailableRestrictions=" + blunderbussUnavailable,
                    catalog != null &&
                        !catalog.Blunderbuss.Spec.Definition.HasFixedRangeIncrement &&
                        !catalog.Blunderbuss.Spec.IsPlayerFireable &&
                        blunderbussUnavailable == 1,
                    "special-range definition and concrete item restriction"),
                Assertion("no-save-writing-api", "none",
                    evidence.SaveWritingApiObserved ? "observed" : "none",
                    !evidence.SaveWritingApiObserved,
                    "request-scoped native save-write sentinels"),
                Assertion("loaded-mod-version", _request.ExpectedModVersion,
                    _context.ModEntry.Info.Version,
                    _request.ExpectedModVersion == _context.ModEntry.Info.Version,
                    "Unity Mod Manager ModEntry.Info.Version")
            };
            bool pass = assertions.TrueForAll(value => value.Status == "PASS");
            RuntimeTestResult result = CreateResult(
                pass ? RuntimeTestStatuses.Pass : RuntimeTestStatuses.Fail,
                assertions, null);
            result.WorkingSaveSmoke = evidence;
            _trace.Record("feature-acceptance-complete", "status=" + result.Status);
            Complete(result);
        }

        private void RunSprint33AdvancedCapacity()
        {
            _trace.Record("feature-acceptance-start",
                "sprint33 advanced capacity; request-local fixtures only");
            FirearmDefinition definition = FirearmDefinitions.CreateAdvancedRevolver();
            var rules = new FirearmStateRules(definition.Capacity,
                new[] { definition.Reload.Ammunition });
            var vault = new RuntimeCapacityVaultStore();
            var repository = new VaultBackedFirearmStateRepository(vault, rules);
            var inventory = new RuntimeCapacityInventory(12, 12);
            object first = new object();
            object second = new object();
            var reload = new FirearmReloadTransactionService();
            FirearmReloadResult firstLoad = reload.TryReloadBasicRounds(
                new RuntimeCapacityReloadStore(repository, first), inventory, rules,
                definition.Reload.Ammunition, definition.Reload.RoundsPerAction);
            FirearmReloadResult secondLoad = reload.TryReloadBasicRounds(
                new RuntimeCapacityReloadStore(repository, second), inventory, rules,
                definition.Reload.Ammunition, definition.Reload.RoundsPerAction);
            repository.Transition(first, FirearmStateMachine.Fire);
            repository.Transition(first, FirearmStateMachine.Fire);
            FirearmState firstAfterFire = repository.GetOrCreate(first).State;
            FirearmState secondAfterFire = repository.GetOrCreate(second).State;

            FirearmMisfireDecision misfire = new FirearmMisfireService().Evaluate(
                1, definition.MisfireValue, true);
            FirearmMisfireConditionDecision firstMisfire =
                new FirearmMisfireConditionService().Evaluate(
                    definition, misfire, firstAfterFire);
            repository.Set(first, firstMisfire.After);
            FirearmMisfireConditionDecision repeatedMisfire =
                new FirearmMisfireConditionService().Evaluate(
                    definition, misfire, firstMisfire.After);
            FirearmExplosionDecision explosion =
                new FirearmExplosionService().Evaluate(repeatedMisfire);

            WorkingSaveSmokeEvidence evidence = _workingSaveSmoke.Stop();
            var assertions = new List<RuntimeTestAssertion>
            {
                Assertion("exact-working-save", "stable exact working-save load",
                    evidence.StableFingerprint,
                    evidence.CompletionCallbackObserved && evidence.DescriptorReferenceCorrelated &&
                        !string.IsNullOrWhiteSpace(evidence.StableFingerprint),
                    "qualified receiver-bound working-save path"),
                Assertion("batch-reload", "first=6;second=6;inventory=0/0",
                    "first=" + firstLoad.AfterState.LoadedRounds +
                        ";second=" + secondLoad.AfterState.LoadedRounds +
                        ";inventory=" + inventory.Powder + "/" + inventory.Balls,
                    firstLoad.RoundsLoaded == 6 && secondLoad.RoundsLoaded == 6 &&
                        inventory.Powder == 0 && inventory.Balls == 0,
                    "compiled atomic multi-round transaction services"),
                Assertion("repeated-discharge-isolation", "first=4;second=6;records=2",
                    "first=" + firstAfterFire.LoadedRounds +
                        ";second=" + secondAfterFire.LoadedRounds +
                        ";records=" + repository.PersistedRecordCount,
                    firstAfterFire.LoadedRounds == 4 && secondAfterFire.LoadedRounds == 6 &&
                        repository.PersistedRecordCount == 2,
                    "reference-distinct save-vault records and canonical Fire transitions"),
                Assertion("advanced-misfire-no-explosion",
                    "NormalToBroken then AdvancedBrokenRemainsBroken; rounds=4;burst=false",
                    firstMisfire.Transition + " then " + repeatedMisfire.Transition +
                        ";rounds=" + repeatedMisfire.After.LoadedRounds +
                        ";burst=" + explosion.RequiresBurstDamage,
                    firstMisfire.Transition == FirearmMisfireConditionTransition.NormalToBroken &&
                        repeatedMisfire.Transition ==
                            FirearmMisfireConditionTransition.AdvancedBrokenRemainsBroken &&
                        repeatedMisfire.After.LoadedRounds == 4 &&
                        !explosion.RequiresBurstDamage,
                    "compiled era-aware misfire and explosion policies"),
                Assertion("no-save-writing-api", "none",
                    evidence.SaveWritingApiObserved ? "observed" : "none",
                    !evidence.SaveWritingApiObserved,
                    "request-scoped native save-write sentinels"),
                Assertion("loaded-mod-version", _request.ExpectedModVersion,
                    _context.ModEntry.Info.Version,
                    _request.ExpectedModVersion == _context.ModEntry.Info.Version,
                    "Unity Mod Manager ModEntry.Info.Version")
            };
            bool pass = assertions.TrueForAll(value => value.Status == "PASS");
            RuntimeTestResult result = CreateResult(
                pass ? RuntimeTestStatuses.Pass : RuntimeTestStatuses.Fail,
                assertions, null);
            result.WorkingSaveSmoke = evidence;
            _trace.Record("feature-acceptance-complete", "status=" + result.Status);
            Complete(result);
        }

        private static int CountFirearmMarkers(BlueprintWeaponType weaponType)
        {
            if (weaponType == null)
                return -1;
            int count = 0;
            foreach (BlueprintComponent component in
                weaponType.ComponentsArray ?? Array.Empty<BlueprintComponent>())
            {
                if (component is FirearmDefinitionComponent)
                    count++;
            }
            return count;
        }

        private void RunLoadGameButtonActionObservation()
        {
            if (_buttonActionObservation == null)
            {
                _trace.Record("scenario-activated",
                    RuntimeTestScenarioCatalog.ObserveLoadGameButtonAction);
                _buttonActionObservation = new LoadGameButtonActionObservation(
                    _context, _elapsed, _request.RunId, _trace.Record);
                _buttonActionObservation.Install();
                return;
            }
            if (_buttonActionObservation.ObservationException != null)
                throw new InvalidOperationException(
                    "Incremental button-action evidence could not be flushed.",
                    _buttonActionObservation.ObservationException);
            if (_manualElapsed == null && _buttonActionObservation.Ready)
            {
                _trace.Record("observer-ready",
                    "non-initiating exact Load Game action hooks active");
                _trace.WriteReady(new RuntimeReadyMarker
                {
                    SchemaVersion = 1, RunId = _request.RunId,
                    Scenario = _request.Scenario,
                    LoadedModVersion = _context.ModEntry.Info.Version,
                    RuntimeIdentity = _context.Assembly.FullName,
                    ReadinessTimestampUtc = DateTime.UtcNow.ToString("o"),
                    InstalledObservationHookIdentifiers =
                        _buttonActionObservation.HookIdentifiers,
                    ProcessId = Process.GetCurrentProcess().Id
                });
                _manualElapsed = Stopwatch.StartNew();
                _catalogElapsed = Stopwatch.StartNew();
            }
            if (_catalogElapsed != null && !_buttonActionObservation.CatalogObserved &&
                _catalogElapsed.Elapsed.TotalSeconds >= _request.CatalogTimeoutSeconds)
            {
                CompleteButtonAction(RuntimeTestStatuses.Timeout,
                    "The exact Load Game action was not correlated with catalog initialization.",
                    "load-game-button-action");
                return;
            }
            if (!_buttonActionObservation.CatalogObserved) return;
            CompleteButtonAction(_buttonActionObservation.ActionProven
                ? RuntimeTestStatuses.Pass : RuntimeTestStatuses.Ambiguous,
                _buttonActionObservation.ActionProven ? "" :
                    "Exactly one active, interactable Load Game button/event was not proven.");
        }

        private void CompleteButtonAction(string status, string warning,
            string timeoutStage = "")
        {
            LoadGameButtonActionEvidence evidence = _buttonActionObservation.Stop();
            var assertions = new List<RuntimeTestAssertion>
            {
                Assertion("exact-button-action-proven",
                    "one active interactable button bound to OnButtonLoadGame",
                    "candidates=" + evidence.Candidates.Count +
                        ";invocations=" + evidence.HandlerInvocationCount,
                    evidence.ActionProven, evidence.HandlerSignature),
                Assertion("catalog-transition",
                    "ListOfSaves.Initialize(List<SaveInfo>,Boolean) after action",
                    evidence.CatalogInitializeSignature,
                    evidence.CatalogObservedAfterAction,
                    "ordered in-process call observation"),
                Assertion("game-thread-only", "all callbacks on game thread",
                    evidence.AllCallbacksOnGameThread ? "confirmed" : "contradicted",
                    evidence.AllCallbacksOnGameThread, "managed thread identity"),
                Assertion("observer-non-initiating", "false",
                    evidence.ProbeInvokedAction.ToString(),
                    !evidence.ProbeInvokedAction, "Harmony observation only")
            };
            RuntimeTestResult result = CreateResult(status, assertions, null);
            result.LoadGameButtonActionObservation = evidence;
            if (!string.IsNullOrWhiteSpace(timeoutStage))
                result.Diagnostics.Add("timeoutStage=" + timeoutStage);
            if (!string.IsNullOrWhiteSpace(warning)) result.Warnings.Add(warning);
            Complete(result);
        }

        private void RunSaveCatalogProviderObservation()
        {
            if (_catalogProviderObservation == null)
            {
                _trace.Record("scenario-activated",
                    RuntimeTestScenarioCatalog.ObserveSaveCatalogProvider);
                _catalogProviderObservation = new SaveCatalogProviderObservation(
                    _context, _elapsed, _request.RunId, _trace.Record);
                _catalogProviderObservation.Install();
                return;
            }
            if (_catalogProviderObservation.ObservationException != null)
                throw new InvalidOperationException(
                    "Incremental provider evidence could not be flushed.",
                    _catalogProviderObservation.ObservationException);
            if (_manualElapsed == null && _catalogProviderObservation.Ready)
            {
                _trace.Record("observer-ready",
                    "catalog provider hooks active on game thread");
                _trace.WriteReady(new RuntimeReadyMarker
                {
                    SchemaVersion = 1,
                    RunId = _request.RunId,
                    Scenario = _request.Scenario,
                    LoadedModVersion = _context.ModEntry.Info.Version,
                    RuntimeIdentity = _context.Assembly.FullName,
                    ReadinessTimestampUtc = DateTime.UtcNow.ToString("o"),
                    InstalledObservationHookIdentifiers =
                        _catalogProviderObservation.HookIdentifiers,
                    ProcessId = Process.GetCurrentProcess().Id
                });
                _manualElapsed = Stopwatch.StartNew();
                _catalogElapsed = Stopwatch.StartNew();
            }
            if (_catalogProviderObservation.WriteObserved ||
                _catalogProviderObservation.LoadObserved)
            {
                CompleteCatalogProvider(RuntimeTestStatuses.Fail,
                    "A save-load or save-writing API was observed.");
                return;
            }
            if (_catalogElapsed != null &&
                !_catalogProviderObservation.CatalogCaptured &&
                _catalogElapsed.Elapsed.TotalSeconds >= _request.CatalogTimeoutSeconds)
            {
                CompleteCatalogProvider(RuntimeTestStatuses.Timeout,
                    "The Load Game catalog provider was not observed.",
                    "catalog-provider-observation");
                return;
            }
            if (!_catalogProviderObservation.CatalogCaptured) return;
            WriteProviderStage();
            CompleteCatalogProvider(_catalogProviderObservation.SourceProven
                ? RuntimeTestStatuses.Pass : RuntimeTestStatuses.Ambiguous,
                _catalogProviderObservation.SourceProven ? string.Empty :
                    "The list was observed but its immediate producer was not proven.");
        }

        private void WriteProviderStage()
        {
            _trace.WriteStage("runtime-catalog-provider-captured.json",
                new RuntimeStageMarker
                {
                    SchemaVersion = 1, RunId = _request.RunId,
                    Scenario = _request.Scenario,
                    Stage = "catalog-provider-captured",
                    LoadedModVersion = _context.ModEntry.Info.Version,
                    TimestampUtc = DateTime.UtcNow.ToString("o"),
                    ProcessId = Process.GetCurrentProcess().Id
                });
            _trace.Record("catalog-provider-captured",
                "atomic provider marker committed");
        }

        private void CompleteCatalogProvider(
            string status, string warning, string timeoutStage = "")
        {
            SaveCatalogProviderObservationEvidence evidence =
                _catalogProviderObservation.Stop();
            var assertions = new List<RuntimeTestAssertion>
            {
                Assertion("complete-catalog-observed", "List<SaveInfo>",
                    evidence.CollectionType, evidence.CompleteListObserved,
                    evidence.InitializeSignature),
                Assertion("catalog-source-proven", "concrete provider",
                    evidence.SourceKind, evidence.SourceProven,
                    evidence.ImmediateCaller),
                Assertion("game-thread-only", "all callbacks on game thread",
                    evidence.AllCallbacksOnGameThread ? "confirmed" : "contradicted",
                    evidence.AllCallbacksOnGameThread, evidence.LifecycleState),
                Assertion("no-save-load-or-write", "none",
                    evidence.SaveLoadObserved || evidence.SaveWritingObserved
                        ? "observed" : "none",
                    !evidence.SaveLoadObserved && !evidence.SaveWritingObserved,
                    "request-scoped sentinels"),
                Assertion("observer-non-invoking", "false",
                    evidence.ProviderInvokedByProbe.ToString(),
                    !evidence.ProviderInvokedByProbe,
                    "Harmony observation only")
            };
            RuntimeTestResult result = CreateResult(status, assertions, null);
            result.SaveCatalogProviderObservation = evidence;
            if (!string.IsNullOrWhiteSpace(timeoutStage))
                result.Diagnostics.Add("timeoutStage=" + timeoutStage);
            if (!string.IsNullOrWhiteSpace(warning)) result.Warnings.Add(warning);
            Complete(result);
        }

        private void RunSaveCatalogObservation()
        {
            if (_catalogObservation == null)
            {
                _trace.Record("scenario-activated",
                    RuntimeTestScenarioCatalog.ObserveSaveCatalogAndSelection);
                _catalogObservation = new SaveCatalogSelectionObservation(
                    _context, _elapsed, _request.RunId, _trace.Record);
                _catalogObservation.Install();
                return;
            }
            _catalogObservation.Poll();
            if (_catalogObservation.ObservationException != null)
                throw new InvalidOperationException("Catalog evidence flush failed.",
                    _catalogObservation.ObservationException);
            if (_manualElapsed == null && _catalogObservation.Ready)
            {
                WriteCatalogStage("runtime-catalog-ready.json", "catalog-observer-ready");
                _manualElapsed = Stopwatch.StartNew();
                _catalogElapsed = Stopwatch.StartNew();
            }
            if (_catalogElapsed != null && !_catalogObservation.CatalogCaptured &&
                _catalogElapsed.Elapsed.TotalSeconds >= _request.CatalogTimeoutSeconds)
            {
                CompleteCatalog(RuntimeTestStatuses.Timeout, "catalog-capture",
                    "The Load Game catalog was not captured.");
                return;
            }
            if (_catalogObservation.CatalogCaptured && !_catalogMarkerWritten)
            {
                WriteCatalogStage("runtime-catalog-captured.json", "catalog-captured");
                _catalogMarkerWritten = true;
                _catalogElapsed.Stop();
                _selectionElapsed = Stopwatch.StartNew();
                if (!_catalogObservation.CatalogComplete)
                {
                    CompleteCatalog(RuntimeTestStatuses.Ambiguous, "catalog-completeness",
                        "Catalog enumeration could not be proven complete.");
                    return;
                }
                if (_catalogObservation.WorkingCount == 0)
                {
                    CompleteCatalog(RuntimeTestStatuses.Fail, "catalog-working-match",
                        "No working-save descriptor exists.");
                    return;
                }
                if (_catalogObservation.WorkingCount > 1)
                {
                    CompleteCatalog(RuntimeTestStatuses.Ambiguous, "catalog-working-match",
                        "Multiple working-save descriptors exist.");
                    return;
                }
            }
            if (_selectionElapsed != null && !_catalogObservation.SelectionObserved &&
                _selectionElapsed.Elapsed.TotalSeconds >= _request.SelectionTimeoutSeconds)
            {
                CompleteCatalog(RuntimeTestStatuses.Timeout, "save-selection",
                    "No save selection was observed.");
                return;
            }
            if (_catalogObservation.SelectionObserved && _completionElapsed == null)
            {
                _selectionElapsed.Stop();
                _completionElapsed = Stopwatch.StartNew();
                if (_catalogObservation.SelectedClassification == "baseline" ||
                    _catalogObservation.SelectedClassification == "other")
                {
                    CompleteCatalog(RuntimeTestStatuses.Fail, "selected-descriptor",
                        "The selected descriptor was not the working save.");
                    return;
                }
                if (string.IsNullOrWhiteSpace(_catalogObservation.CorrelationMethod))
                {
                    CompleteCatalog(RuntimeTestStatuses.Ambiguous, "descriptor-correlation",
                        "Selection could not be correlated to the unique catalog entry.");
                    return;
                }
            }
            if (_completionElapsed != null &&
                !_catalogObservation.StableFingerprintAvailable &&
                _completionElapsed.Elapsed.TotalSeconds >= _request.CompletionTimeoutSeconds)
            {
                string stage = _catalogObservation.CompletionObserved
                    ? "post-load-fingerprint" : "load-completion";
                CompleteCatalog(RuntimeTestStatuses.Timeout, stage,
                    "Positive load completion and stable fingerprint were not both available.");
                return;
            }
            if (_catalogObservation.WriteObserved)
            {
                CompleteCatalog(RuntimeTestStatuses.Fail, "save-write-observation",
                    "A save-writing or migration API was observed.");
                return;
            }
            if (_catalogObservation.StableFingerprintAvailable)
                CompleteCatalog(RuntimeTestStatuses.Pass, "", "");
        }

        private void WriteCatalogStage(string fileName, string stage)
        {
            _trace.WriteStage(fileName, new RuntimeStageMarker
            {
                SchemaVersion = 1, RunId = _request.RunId,
                Scenario = _request.Scenario, Stage = stage,
                LoadedModVersion = _context.ModEntry.Info.Version,
                TimestampUtc = DateTime.UtcNow.ToString("o"),
                ProcessId = Process.GetCurrentProcess().Id,
                WorkingMatchCount = _catalogObservation.WorkingCount,
                BaselineMatchCount = _catalogObservation.BaselineCount
            });
            _trace.Record(stage, "atomic stage marker committed");
        }

        private void CompleteCatalog(string status, string stage, string warning)
        {
            SaveCatalogObservationEvidence evidence = _catalogObservation.Stop();
            var assertions = new List<RuntimeTestAssertion>
            {
                Assertion("unique-working-descriptor", "1",
                    evidence.WorkingMatchCount.ToString(),
                    evidence.WorkingMatchCount == 1, "captured SaveManager catalog"),
                Assertion("selected-working-descriptor", "working",
                    evidence.SelectedClassification,
                    evidence.SelectedClassification == "working" &&
                        evidence.SelectedCorrelates,
                    "catalog-to-selection correlation"),
                Assertion("load-completion", "callback and stable fingerprint",
                    "callback=" + evidence.CompletionObserved +
                        "; fingerprint=" + !string.IsNullOrWhiteSpace(evidence.StableFingerprint),
                    evidence.CompletionObserved &&
                        !string.IsNullOrWhiteSpace(evidence.StableFingerprint),
                    "after-load callback and two stable game-thread samples"),
                Assertion("no-save-writing-api", "none",
                    evidence.SaveWritingApiObserved ? "observed" : "none",
                    !evidence.SaveWritingApiObserved, "request-scoped lifecycle hooks"),
                Assertion("loaded-mod-version", _request.ExpectedModVersion,
                    _context.ModEntry.Info.Version,
                    _request.ExpectedModVersion == _context.ModEntry.Info.Version,
                    "Unity Mod Manager ModEntry.Info.Version")
            };
            RuntimeTestResult result = CreateResult(status, assertions, null);
            result.SaveCatalogObservation = evidence;
            if (!string.IsNullOrWhiteSpace(stage))
                result.Diagnostics.Add("timeoutStage=" + stage);
            if (!string.IsNullOrWhiteSpace(warning)) result.Warnings.Add(warning);
            Complete(result);
        }

        private void RunManualSaveLoadObservation()
        {
            if (_saveLoadObservation == null)
            {
                _trace.Record("scenario-activated", "observe-manual-save-load");
                _saveLoadObservation = new ManualSaveLoadObservation(
                    _context, _elapsed, _request.RunId, _trace.Record);
                _saveLoadObservation.Install();
                return;
            }
            _saveLoadObservation.PollLoadedState();
            if (_saveLoadObservation.ObservationException != null)
                throw new InvalidOperationException(
                    "Incremental observation evidence could not be flushed.",
                    _saveLoadObservation.ObservationException);
            if (_manualElapsed == null && _saveLoadObservation.ObserverReady)
            {
                _trace.Record("observer-ready",
                    "main-thread callback active; completion callback registered");
                _trace.WriteReady(new RuntimeReadyMarker
                {
                    SchemaVersion = 1,
                    RunId = _request.RunId,
                    Scenario = _request.Scenario,
                    LoadedModVersion = _context.ModEntry.Info.Version,
                    RuntimeIdentity = _context.Assembly.FullName,
                    ReadinessTimestampUtc = DateTime.UtcNow.ToString("o"),
                    InstalledObservationHookIdentifiers =
                        _saveLoadObservation.InstalledHookIdentifiers,
                    ProcessId = Process.GetCurrentProcess().Id
                });
                _manualElapsed = Stopwatch.StartNew();
            }
            if (_saveLoadObservation.WriteObserved)
            {
                CompleteManualObservation(RuntimeTestStatuses.Fail,
                    "A forbidden save-writing API was observed.");
                return;
            }
            if (_saveLoadObservation.IdentityRejected)
            {
                CompleteManualObservation(RuntimeTestStatuses.Fail,
                    "The manually loaded save was not the allowlisted working save.");
                return;
            }
            if (_saveLoadObservation.IdentityAmbiguous)
            {
                CompleteManualObservation(RuntimeTestStatuses.Ambiguous,
                    "The manually loaded save identity or callback contract was ambiguous.");
                return;
            }
            if (_saveLoadObservation.IsReadyToComplete)
                CompleteManualObservation(RuntimeTestStatuses.Pass, string.Empty);
        }

        private void CompleteManualObservation(string status, string warning)
        {
            SaveLoadObservationEvidence evidence = _saveLoadObservation.Stop();
            var assertions = new List<RuntimeTestAssertion>
            {
                Assertion("loaded-save-identity", ManualSaveLoadObservation.WorkingSave,
                    evidence.AcceptedSaveName,
                    evidence.AcceptedSaveName == ManualSaveLoadObservation.WorkingSave,
                    "allowlisted SaveInfo.Name/GameName/GameId/file leaf"),
                Assertion("load-completion", "after-load callback and stable loaded state",
                    "callback=" + evidence.CompletionCallbackObserved +
                        "; stable=" + evidence.GameLoadedStateObserved,
                    evidence.CompletionCallbackObserved &&
                        evidence.GameLoadedStateObserved,
                    "SaveManager callback plus two identical game-thread samples"),
                Assertion("loaded-mod-version", _request.ExpectedModVersion,
                    _context.ModEntry.Info.Version,
                    _request.ExpectedModVersion == _context.ModEntry.Info.Version,
                    "Unity Mod Manager ModEntry.Info.Version"),
                Assertion("no-save-writing-api", "none", evidence.SaveWritingApiObserved
                    ? "observed" : "none", !evidence.SaveWritingApiObserved,
                    "request-scoped SaveManager observation"),
                Assertion("observation-disabled", "all patches removed",
                    evidence.ObservationPatchesRemoved ? "removed" : "active",
                    evidence.ObservationPatchesRemoved,
                    "Harmony owner-scoped unpatch"),
                Assertion("game-thread-only", "all observation callbacks on Unity update thread",
                    evidence.AllCallbacksOnGameThread ? "confirmed" : "contradicted",
                    evidence.AllCallbacksOnGameThread,
                    "managed thread identity captured for every event")
            };
            RuntimeTestResult result = CreateResult(status, assertions, null);
            result.SaveLoadObservation = evidence;
            if (!string.IsNullOrWhiteSpace(warning)) result.Warnings.Add(warning);
            Complete(result);
        }

        private RuntimeTestResult RunModLoadSmoke()
        {
            Assembly assembly = _context.Assembly;
            string expectedVersion = _request.ExpectedModVersion;
            string observedVersion = _context.ModEntry.Info.Version;
            string runtimeIdentity = assembly.FullName +
                "; pid=" + Process.GetCurrentProcess().Id;
            var assertions = new List<RuntimeTestAssertion>
            {
                Assertion(
                    "loaded-mod-version",
                    expectedVersion,
                    observedVersion,
                    string.Equals(expectedVersion, observedVersion, StringComparison.Ordinal),
                    "Unity Mod Manager ModEntry.Info.Version"),
                Assertion(
                    "runtime-identity",
                    "executing KingmakerGunslinger assembly in current process",
                    runtimeIdentity,
                    assembly == Assembly.GetExecutingAssembly() &&
                        assembly.GetName().Name == "KingmakerGunslinger",
                    "ModContext.Assembly and Process.GetCurrentProcess().Id"),
                Assertion(
                    "core-initialization",
                    "published context with Harmony patches installed",
                    _context.IsReady ? "ready" : "not-ready",
                    _context.IsReady,
                    "ModContext.IsReady")
            };
            bool pass = assertions.TrueForAll(value => value.Status == "PASS");
            RuntimeTestResult result = CreateResult(pass ? "PASS" : "FAIL", assertions, null);
            result.RuntimeIdentity = runtimeIdentity;
            return result;
        }

        private RuntimeTestResult RunClassBlueprintContractObservation()
        {
            BlueprintRoot root = BlueprintRoot.Instance;
            BlueprintCharacterClass[] classes = root == null || root.Progression == null
                ? null : root.Progression.CharacterClasses;
            var records = new List<string>();
            bool complete = classes != null && classes.Length > 0;
            if (classes != null)
            {
                foreach (BlueprintCharacterClass characterClass in classes)
                {
                    if (characterClass == null || characterClass.Progression == null ||
                        characterClass.BaseAttackBonus == null ||
                        characterClass.FortitudeSave == null ||
                        characterClass.ReflexSave == null || characterClass.WillSave == null)
                    {
                        complete = false;
                        continue;
                    }
                    records.Add(string.Join(";", new[]
                    {
                        "name=" + characterClass.name,
                        "guid=" + characterClass.AssetGuid,
                        "progression=" + characterClass.Progression.AssetGuid,
                        "bab=" + characterClass.BaseAttackBonus.AssetGuid,
                        "fort=" + characterClass.FortitudeSave.AssetGuid,
                        "ref=" + characterClass.ReflexSave.AssetGuid,
                        "will=" + characterClass.WillSave.AssetGuid,
                        "hitDie=" + characterClass.HitDie,
                        "skills=" + string.Join(",", (characterClass.ClassSkills ??
                            new Kingmaker.EntitySystem.Stats.StatType[0])
                            .Select(value => value.ToString()).ToArray()),
                        "startingItems=" + DescribeStartingItems(characterClass),
                        "level1=" + DescribeLevelOneFeatures(characterClass.Progression)
                    }));
                }
            }
            records.Sort(StringComparer.Ordinal);
            string observed = string.Join(" | ", records.ToArray());
            var assertions = new List<RuntimeTestAssertion>
            {
                Assertion("native-class-root", "nonempty complete native class catalog",
                    "count=" + (classes == null ? -1 : classes.Length) + "; " + observed,
                    complete && records.Count == classes.Length,
                    "BlueprintRoot.Instance.Progression.CharacterClasses exact references"),
                Assertion("observation-only", "no native blueprint mutation",
                    "read-only field access", true,
                    "scenario contains no registry, array assignment, save, or input call"),
                Assertion("loaded-mod-version", _request.ExpectedModVersion,
                    _context.ModEntry.Info.Version,
                    _request.ExpectedModVersion == _context.ModEntry.Info.Version,
                    "Unity Mod Manager ModEntry.Info.Version")
            };
            bool pass = assertions.TrueForAll(value => value.Status == "PASS");
            return CreateResult(pass ? RuntimeTestStatuses.Pass :
                RuntimeTestStatuses.Fail, assertions, null);
        }

        private static string DescribeStartingItems(BlueprintCharacterClass characterClass)
        {
            return string.Join(",", (characterClass.StartingItems ??
                new Kingmaker.Blueprints.Items.BlueprintItem[0])
                .Where(value => value != null)
                .Select(value => value.name + "@" + value.AssetGuid)
                .OrderBy(value => value, StringComparer.Ordinal).ToArray());
        }

        private static string DescribeLevelOneFeatures(BlueprintProgression progression)
        {
            LevelEntry entry = (progression.LevelEntries ?? new LevelEntry[0])
                .SingleOrDefault(value => value != null && value.Level == 1);
            if (entry == null || entry.Features == null) return "<missing>";
            return string.Join(",", entry.Features
                .Where(value => value != null)
                .Select(value => value.name + "@" + value.AssetGuid +
                    DescribeProficiencies(value))
                .OrderBy(value => value, StringComparer.Ordinal).ToArray());
        }

        private static string DescribeProficiencies(BlueprintFeatureBase feature)
        {
            BlueprintComponent[] components = feature.ComponentsArray ??
                new BlueprintComponent[0];
            AddProficiencies[] direct = components.OfType<AddProficiencies>().ToArray();
            if (direct.Length > 0) return string.Join("&", direct
                .Select(DescribeProficiencyComponent).ToArray());
            AddFacts[] addFacts = components.OfType<AddFacts>().ToArray();
            if (addFacts.Length == 0) return string.Empty;
            return "{facts=" + string.Join("+", addFacts.SelectMany(value => value.Facts ??
                new Kingmaker.Blueprints.Facts.BlueprintUnitFact[0])
                .Where(value => value != null)
                .Select(value => value.name + "@" + value.AssetGuid +
                    DescribeDirectProficiency(value as BlueprintFeatureBase))
                .OrderBy(value => value, StringComparer.Ordinal).ToArray()) + "}";
        }

        private static string DescribeDirectProficiency(BlueprintFeatureBase feature)
        {
            if (feature == null) return string.Empty;
            AddProficiencies[] components = (feature.ComponentsArray ??
                new BlueprintComponent[0]).OfType<AddProficiencies>().ToArray();
            return components.Length == 0 ? string.Empty : string.Join("&",
                components.Select(DescribeProficiencyComponent).ToArray());
        }

        private static string DescribeProficiencyComponent(AddProficiencies component)
        {
            string armor = string.Join("+", (component.ArmorProficiencies ??
                new Kingmaker.Blueprints.Items.Armors.ArmorProficiencyGroup[0])
                .Select(value => value.ToString()).OrderBy(value => value,
                    StringComparer.Ordinal).ToArray());
            string weapons = string.Join("+", (component.WeaponProficiencies ??
                new Kingmaker.Enums.WeaponCategory[0])
                .Select(value => value.ToString()).OrderBy(value => value,
                    StringComparer.Ordinal).ToArray());
            return "{armor=" + armor + ";weapons=" + weapons + "}";
        }

        private void Complete(RuntimeTestResult result)
        {
            if (_saveLoadObservation != null && result.SaveLoadObservation == null)
                result.SaveLoadObservation = _saveLoadObservation.Stop();
            if (_catalogObservation != null && result.SaveCatalogObservation == null)
                result.SaveCatalogObservation = _catalogObservation.Stop();
            if (_workingSaveSmoke != null && result.WorkingSaveSmoke == null)
                result.WorkingSaveSmoke = _workingSaveSmoke.Stop();
            _completed = true;
            _context.ModEntry.OnUpdate -= OnUpdate;
            _elapsed.Stop();
            result.EndUtc = DateTime.UtcNow.ToString("o");
            result.DurationMilliseconds = _elapsed.ElapsedMilliseconds;
            result.AutomaticExitRequested = _request.ExitAfterCompletion;
            result.AutomaticExitInitiated = _request.ExitAfterCompletion;
            result.EvidenceDirectory = _request.EvidenceDirectory;
            try
            {
                // This record atomically commits and closes the immutable event
                // trace before the summary and final result are published.
                _trace.Record("final-result-created", "status=" + result.Status);
                RuntimeTestResultWriter.Write(result, _request.EvidenceDirectory);
                WriteLifecycleStage("final-result-flushed");
                string flushedPath = Path.Combine(_request.EvidenceDirectory,
                    "runtime-stage-final-result-flushed.json");
                string flushedContent = File.ReadAllText(flushedPath);
                if (flushedContent.IndexOf(_request.RunId,
                    StringComparison.Ordinal) < 0)
                    throw new IOException(
                        "The final-result-flushed marker was not safely visible.");
            }
            catch (Exception exception)
            {
                TryWriteEvidenceFailure(exception);
                _context.Logger.Failure(
                    "runtime-test",
                    "result.write-failed",
                    "Runtime evidence could not be committed; automatic exit was suppressed.",
                    exception);
                return;
            }
            _context.Logger.Info(
                "runtime-test",
                "scenario.complete",
                "runId=" + _request.RunId + "; status=" + result.Status);
            if (_request.ExitAfterCompletion)
                Application.Quit();
        }

        private void TryWriteEvidenceFailure(Exception exception)
        {
            try
            {
                RuntimeTestResult failure = CreateResult(
                    RuntimeTestStatuses.Error, null,
                    "Evidence write failure: " + ExceptionSummary(exception));
                failure.EndUtc = DateTime.UtcNow.ToString("o");
                failure.DurationMilliseconds = _elapsed.ElapsedMilliseconds;
                failure.ErrorStage = "final-evidence-write";
                failure.LastCompletedStage = "assertions-complete";
                failure.ExceptionType = exception.GetType().FullName;
                failure.ExceptionMessage = exception.Message;
                failure.ExceptionStack = exception.ToString();
                failure.ExceptionManagedThreadId =
                    System.Threading.Thread.CurrentThread.ManagedThreadId;
                failure.AutomaticExitRequested = _request.ExitAfterCompletion;
                failure.AutomaticExitInitiated = false;
                failure.EvidenceDirectory = _request.EvidenceDirectory;
                RuntimeTestResultWriter.Write(failure, _request.EvidenceDirectory);
                WriteLifecycleStage("final-evidence-error-flushed");
            }
            catch (Exception fallbackException)
            {
                _context.Logger.Failure(
                    "runtime-test", "result.error-write-failed",
                    "Structured ERROR evidence also could not be committed; the game remains running.",
                    fallbackException);
            }
        }

        private sealed class RuntimeCapacityInventory : IBasicAmmunitionInventory
        {
            internal RuntimeCapacityInventory(int powder, int balls)
            {
                Powder = powder;
                Balls = balls;
            }
            internal int Powder { get; private set; }
            internal int Balls { get; private set; }
            public int Count(BasicAmmunitionComponent component)
            {
                return component == BasicAmmunitionComponent.BlackPowderCharge ? Powder : Balls;
            }
            public void Add(BasicAmmunitionComponent component, int amount)
            {
                if (component == BasicAmmunitionComponent.BlackPowderCharge) Powder += amount;
                else Balls += amount;
            }
            public void Remove(BasicAmmunitionComponent component, int amount)
            {
                if (component == BasicAmmunitionComponent.BlackPowderCharge) Powder -= amount;
                else Balls -= amount;
            }
        }

        private sealed class RuntimeCapacityReloadStore : IFirearmReloadStateStore
        {
            private readonly VaultBackedFirearmStateRepository _repository;
            private readonly object _item;
            internal RuntimeCapacityReloadStore(
                VaultBackedFirearmStateRepository repository, object item)
            {
                _repository = repository;
                _item = item;
            }
            public FirearmState Read() { return _repository.GetOrCreate(_item).State; }
            public void Replace(FirearmState expectedCurrent, FirearmState replacement)
            {
                if (Read() != expectedCurrent)
                    throw new InvalidOperationException("Runtime capacity state changed concurrently.");
                _repository.Set(_item, replacement);
            }
        }

        private sealed class RuntimeCapacityVaultStore : IFirearmStateVaultStore
        {
            private readonly Dictionary<object, FirearmStateData> _records =
                new Dictionary<object, FirearmStateData>(ReferenceIdentityComparer.Instance);
            public int RecordCount { get { return _records.Count; } }
            public bool TryRead(object itemInstance, out FirearmStateData data)
            {
                FirearmStateData value;
                if (!_records.TryGetValue(itemInstance, out value))
                {
                    data = null;
                    return false;
                }
                data = FirearmStateDataUtility.Clone(value);
                return true;
            }
            public void Replace(object itemInstance, FirearmStateData expectedData,
                FirearmStateData targetData)
            {
                FirearmStateData current;
                _records.TryGetValue(itemInstance, out current);
                if (!FirearmStateDataUtility.AreEqual(current, expectedData))
                    throw new InvalidOperationException("Runtime capacity vault compare failed.");
                if (targetData == null) _records.Remove(itemInstance);
                else _records[itemInstance] = FirearmStateDataUtility.Clone(targetData);
            }
            public bool Remove(object itemInstance) { return _records.Remove(itemInstance); }
        }

        private RuntimeTestResult CreateResult(
            string status,
            List<RuntimeTestAssertion> assertions,
            string exceptionSummary)
        {
            return new RuntimeTestResult
            {
                SchemaVersion = 1,
                RunId = _request.RunId,
                Scenario = _request.Scenario,
                Status = status,
                LoadedModVersion = _context.ModEntry.Info.Version,
                RuntimeIdentity = _context.Assembly.FullName,
                GitCommit = ReadAssemblyMetadata(_context.Assembly, "GitCommit"),
                GameVersion = Application.version ?? string.Empty,
                StartUtc = _startedUtc.ToString("o"),
                EndUtc = string.Empty,
                DurationMilliseconds = 0,
                Assertions = assertions ?? new List<RuntimeTestAssertion>(),
                Diagnostics = new List<string>
                {
                    "mainThreadManagedId=" +
                    System.Threading.Thread.CurrentThread.ManagedThreadId
                },
                Warnings = new List<string>(),
                ExceptionSummary = exceptionSummary ?? string.Empty,
                EvidenceFiles = new List<string>(),
                AutomaticExitRequested = _request.ExitAfterCompletion,
                AutomaticExitInitiated = false,
                EvidenceDirectory = _request.EvidenceDirectory
            };
        }

        private static RuntimeTestAssertion Assertion(
            string name,
            string expected,
            string observed,
            bool passed,
            string evidence)
        {
            return new RuntimeTestAssertion
            {
                Name = name,
                Expected = expected,
                Observed = observed,
                Status = passed ? "PASS" : "FAIL",
                Evidence = evidence
            };
        }

        private static string ExceptionSummary(Exception exception)
        {
            return exception.GetType().FullName + ": " + exception.Message;
        }

        private static string ReadAssemblyMetadata(Assembly assembly, string key)
        {
            foreach (AssemblyMetadataAttribute attribute in assembly.GetCustomAttributes(
                typeof(AssemblyMetadataAttribute), false))
            {
                if (string.Equals(attribute.Key, key, StringComparison.Ordinal))
                    return attribute.Value ?? string.Empty;
            }
            return string.Empty;
        }
    }
}
