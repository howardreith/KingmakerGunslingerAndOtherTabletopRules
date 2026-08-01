using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Items;
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
using KingmakerGunslinger.Grit;
using KingmakerGunslinger.Deeds;
using KingmakerGunslinger.Firing;
using Kingmaker.Items;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules.Damage;
using UnityEngine;
using UnityModManagerNet;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic;
using Kingmaker;
using Kingmaker.UnitLogic.Class.LevelUp.Actions;
using Kingmaker.Blueprints.Items.Armors;
using Kingmaker.PubSubSystem;
using KingmakerGunslinger.Classes;

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
                    _request.Scenario != RuntimeTestScenarioCatalog.GunslingerStartingItems &&
                    _request.Scenario != RuntimeTestScenarioCatalog.ObserveCharacterCreationContracts &&
                    _request.Scenario != RuntimeTestScenarioCatalog.DisposableDescriptorConstruction &&
                    _request.Scenario != RuntimeTestScenarioCatalog.DisposableGunslingerSelection &&
                    _request.Scenario != RuntimeTestScenarioCatalog.DisposableGunslingerPreviewApplication &&
                    _request.Scenario != RuntimeTestScenarioCatalog.DisposableGunslingerLevelUpPreview &&
                    _request.Scenario != RuntimeTestScenarioCatalog.DisposableGunslingerMulticlassPreview &&
                    _request.Scenario != RuntimeTestScenarioCatalog.DisposableGunslingerRespecPreview &&
                    _request.Scenario != RuntimeTestScenarioCatalog.DisposableGunslingerGritResource &&
                    _request.Scenario != RuntimeTestScenarioCatalog.DisposableGunslingerGritRest &&
                    _request.Scenario != RuntimeTestScenarioCatalog.DisposableGunslingerGritPersistence &&
                    _request.Scenario != RuntimeTestScenarioCatalog.DisposableGunslingerGritRecovery &&
                    _request.Scenario != RuntimeTestScenarioCatalog.DisposableGunslingerDeadeye &&
                    _request.Scenario != RuntimeTestScenarioCatalog.DisposableGunslingerDodge &&
                    _request.Scenario != RuntimeTestScenarioCatalog.DisposableGunslingerQuickClear &&
                    _request.Scenario != RuntimeTestScenarioCatalog.DisposableGunslingerNimble &&
                    _request.Scenario != RuntimeTestScenarioCatalog.DisposableGunslingerInitiative &&
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
                    RuntimeTestScenarioCatalog.ObserveCharacterCreationContracts)
                {
                    Complete(RunCharacterCreationContractObservation());
                    return;
                }
                if (_request.Scenario ==
                    RuntimeTestScenarioCatalog.DisposableDescriptorConstruction)
                {
                    Complete(RunDisposableDescriptorConstruction());
                    return;
                }
                if (_request.Scenario ==
                    RuntimeTestScenarioCatalog.DisposableGunslingerSelection)
                {
                    Complete(RunDisposableGunslingerSelection());
                    return;
                }
                if (_request.Scenario ==
                    RuntimeTestScenarioCatalog.DisposableGunslingerPreviewApplication)
                {
                    Complete(RunDisposableGunslingerPreviewApplication());
                    return;
                }
                if (_request.Scenario ==
                    RuntimeTestScenarioCatalog.DisposableGunslingerLevelUpPreview)
                {
                    Complete(RunDisposableGunslingerLevelUpPreview());
                    return;
                }
                if (_request.Scenario ==
                    RuntimeTestScenarioCatalog.DisposableGunslingerMulticlassPreview)
                {
                    Complete(RunDisposableGunslingerMulticlassPreview());
                    return;
                }
                if (_request.Scenario ==
                    RuntimeTestScenarioCatalog.DisposableGunslingerRespecPreview)
                {
                    Complete(RunDisposableGunslingerRespecPreview());
                    return;
                }
                if (_request.Scenario ==
                    RuntimeTestScenarioCatalog.DisposableGunslingerGritResource)
                {
                    Complete(RunDisposableGunslingerGritResource());
                    return;
                }
                if (_request.Scenario ==
                    RuntimeTestScenarioCatalog.DisposableGunslingerGritRest)
                {
                    Complete(RunDisposableGunslingerGritRest());
                    return;
                }
                if (_request.Scenario ==
                    RuntimeTestScenarioCatalog.DisposableGunslingerGritPersistence)
                {
                    Complete(RunDisposableGunslingerGritPersistence());
                    return;
                }
                if (_request.Scenario ==
                    RuntimeTestScenarioCatalog.DisposableGunslingerGritRecovery)
                {
                    Complete(RunDisposableGunslingerGritRecovery());
                    return;
                }
                if (_request.Scenario ==
                    RuntimeTestScenarioCatalog.DisposableGunslingerDeadeye)
                {
                    Complete(RunDisposableGunslingerDeadeye());
                    return;
                }
                if (_request.Scenario ==
                    RuntimeTestScenarioCatalog.DisposableGunslingerDodge)
                {
                    Complete(RunDisposableGunslingerDodge());
                    return;
                }
                if (_request.Scenario ==
                    RuntimeTestScenarioCatalog.DisposableGunslingerQuickClear)
                {
                    Complete(RunDisposableGunslingerQuickClear());
                    return;
                }
                if (_request.Scenario == RuntimeTestScenarioCatalog.DisposableGunslingerNimble)
                {
                    Complete(RunDisposableGunslingerNimble());
                    return;
                }
                if (_request.Scenario ==
                    RuntimeTestScenarioCatalog.DisposableGunslingerInitiative)
                {
                    Complete(RunDisposableGunslingerInitiative());
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
                    _request.Scenario == RuntimeTestScenarioCatalog.GunslingerStartingItems ||
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
                    _request.Scenario == RuntimeTestScenarioCatalog.GunslingerStartingItems ||
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
                else if (_request.Scenario == RuntimeTestScenarioCatalog.GunslingerStartingItems)
                {
                    RunGunslingerStartingItems();
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
            receiverBoundPath = receiverBoundPath ||
                _request.Scenario == RuntimeTestScenarioCatalog.GunslingerStartingItems;
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

        private void RunGunslingerStartingItems()
        {
            _trace.Record("feature-acceptance-start",
                "native Gunslinger starting items; exact request-local rollback");
            WorkingSaveSmokeEvidence evidence = _workingSaveSmoke.Stop();
            var diagnostics = new List<string>();
            bool exactGrant = false;
            bool exactRollback = false;
            bool moneyStable = false;
            bool classRestored = false;
            bool startingGoldRestored = false;
            int addedCount = -1;
            int pistolCount = -1;
            int powderCount = -1;
            int ballCount = -1;

            Player player = null;
            Kingmaker.UnitLogic.ClassData classData = null;
            BlueprintCharacterClass originalClass = null;
            BlueprintCharacterClass gunslinger = null;
            int originalStartingGold = 0;
            long moneyBefore = 0;
            FieldInfo classField = null;
            List<object> before = null;
            BlueprintItem[] expected = null;
            int[] beforeQuantities = null;
            var added = new List<object>();
            try
            {
                player = Game.Instance == null ? null : Game.Instance.Player;
                if (player == null || player.Inventory == null ||
                    player.MainCharacter.Value == null)
                    throw new InvalidOperationException(
                        "The exact loaded player, inventory, or main character is unavailable.");

                gunslinger = BlueprintBootstrap.GunslingerClass == null
                    ? null : BlueprintBootstrap.GunslingerClass.CharacterClass;
                if (gunslinger == null)
                    throw new InvalidOperationException(
                        "The production Gunslinger class blueprint is unavailable.");

                var descriptor = player.MainCharacter.Value.Descriptor;
                BlueprintCharacterClass maximum = descriptor.Progression.GetMaxClass();
                classData = descriptor.Progression.GetClassData(maximum);
                if (classData == null)
                    throw new InvalidOperationException(
                        "The main character has no exact maximum ClassData receiver.");
                if (descriptor.Progression.GetClassData(gunslinger) != null)
                    throw new InvalidOperationException(
                        "The working character already has Gunslinger ClassData; identity substitution refused.");

                before = EnumerateRuntimeInventory(player.Inventory);
                expected = gunslinger.StartingItems ?? Array.Empty<BlueprintItem>();
                if (expected.Length != 3 || expected.Any(item => item == null) ||
                    expected.Distinct().Count() != 3)
                    throw new InvalidOperationException(
                        "The production Gunslinger starting-item array is not three exact distinct items.");
                beforeQuantities = expected.Select(item =>
                    player.Inventory.Count(item)).ToArray();
                moneyBefore = player.Money;
                originalClass = classData.CharacterClass;
                originalStartingGold = gunslinger.StartingGold;
                classField = typeof(Kingmaker.UnitLogic.ClassData).GetField(
                    "CharacterClass", BindingFlags.Instance | BindingFlags.Public);
                if (classField == null || !classField.IsInitOnly ||
                    !ReferenceEquals(classField.GetValue(classData), originalClass))
                    throw new MissingFieldException(
                        "The exact readonly ClassData.CharacterClass field is unavailable.");

                gunslinger.StartingGold = 0;
                classField.SetValue(classData, gunslinger);
                LevelUpHelper.AddStartingItems(descriptor);

                List<object> afterGrant = EnumerateRuntimeInventory(player.Inventory);
                added.AddRange(afterGrant.Where(item =>
                    !before.Any(existing => ReferenceEquals(existing, item))));
                addedCount = added.Count;
                pistolCount = player.Inventory.Count(expected[0]) -
                    beforeQuantities[0];
                powderCount = player.Inventory.Count(expected[1]) -
                    beforeQuantities[1];
                ballCount = player.Inventory.Count(expected[2]) -
                    beforeQuantities[2];
                exactGrant = added.All(item => expected.Any(blueprint =>
                        ItemUsesRuntimeBlueprint(item, blueprint))) &&
                    pistolCount == 1 && powderCount == 1 && ballCount == 1;
                moneyStable = player.Money == moneyBefore;
            }
            catch (Exception exception)
            {
                diagnostics.Add(exception.GetType().Name + ": " + exception.Message);
            }
            finally
            {
                if (classData != null && originalClass != null && classField != null)
                    classField.SetValue(classData, originalClass);
                if (gunslinger != null)
                    gunslinger.StartingGold = originalStartingGold;
                if (player != null && player.Inventory != null)
                {
                    foreach (object item in added)
                    {
                        object ignored;
                        string method;
                        try
                        {
                            if (!ReflectionAccess.TryInvokeAny(player.Inventory,
                                new[] { "Remove", "RemoveItem" },
                                new[] { new object[] { item, 1, false },
                                    new object[] { item, 1 }, new object[] { item } },
                                out ignored, out method))
                                diagnostics.Add("Exact added item removal failed.");
                        }
                        catch (Exception exception)
                        {
                            diagnostics.Add("Exact added item removal failed: " +
                                exception.GetType().Name + ".");
                        }
                    }
                    if (expected != null && beforeQuantities != null)
                    {
                        for (int index = 0; index < expected.Length; index++)
                        {
                            int excess = player.Inventory.Count(expected[index]) -
                                beforeQuantities[index];
                            if (excess > 0)
                            {
                                try
                                {
                                    player.Inventory.Remove(expected[index], excess);
                                }
                                catch (Exception exception)
                                {
                                    diagnostics.Add("Exact quantity rollback failed: " +
                                        exception.GetType().Name + ".");
                                }
                            }
                            else if (excess < 0)
                                diagnostics.Add(
                                    "Starting-item rollback observed a negative exact quantity delta.");
                        }
                    }
                }

                classRestored = classData == null ||
                    ReferenceEquals(classData.CharacterClass, originalClass);
                startingGoldRestored = gunslinger == null ||
                    gunslinger.StartingGold == originalStartingGold;
                if (player != null)
                    moneyStable = player.Money == moneyBefore;
                if (before != null && player != null && player.Inventory != null)
                {
                    List<object> afterRollback =
                        EnumerateRuntimeInventory(player.Inventory);
                    exactRollback = before.Count == afterRollback.Count &&
                        before.All(item => afterRollback.Any(current =>
                            ReferenceEquals(item, current))) &&
                        afterRollback.All(item => before.Any(previous =>
                            ReferenceEquals(item, previous))) &&
                        expected.Select(item => player.Inventory.Count(item))
                            .SequenceEqual(beforeQuantities);
                }
            }

            var assertions = new List<RuntimeTestAssertion>
            {
                Assertion("exact-working-save", "stable exact working-save load",
                    evidence.StableFingerprint,
                    evidence.CompletionCallbackObserved &&
                        evidence.DescriptorReferenceCorrelated &&
                        !string.IsNullOrWhiteSpace(evidence.StableFingerprint),
                    "qualified receiver-bound working-save path"),
                Assertion("native-starting-item-grant", "pistol=1;powder=1;ball=1",
                    "added=" + addedCount + ";pistol=" + pistolCount +
                        ";powder=" + powderCount + ";ball=" + ballCount,
                    exactGrant,
                    "LevelUpHelper.AddStartingItems on the exact main descriptor"),
                Assertion("exact-in-memory-rollback",
                    "inventory references, class identity, gold, and money restored",
                    "inventory=" + exactRollback + ";class=" + classRestored +
                        ";startingGold=" + startingGoldRestored +
                        ";money=" + moneyStable,
                    exactRollback && classRestored && startingGoldRestored && moneyStable,
                    "exact newly created item references and finally restoration"),
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
            foreach (string diagnostic in diagnostics)
                result.Diagnostics.Add(diagnostic);
            _trace.Record("feature-acceptance-complete", "status=" + result.Status);
            Complete(result);
        }

        private static List<object> EnumerateRuntimeInventory(object inventory)
        {
            if (!ReflectionAccess.CanEnumerate(inventory))
                throw new MissingMemberException(
                    "The exact shared inventory is not enumerable.");
            return ReflectionAccess.Enumerate(inventory).ToList();
        }

        private static bool ItemUsesRuntimeBlueprint(object item,
            BlueprintItem expected)
        {
            if (item == null || expected == null)
                return false;
            object actual;
            string member;
            return ReflectionAccess.TryGetFirstNonNullMember(item,
                new[] { "Blueprint", "m_Blueprint", "BlueprintItem", "ItemBlueprint" },
                out actual, out member) && ReferenceEquals(actual, expected);
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

        private RuntimeTestResult RunCharacterCreationContractObservation()
        {
            string[] typeNames =
            {
                "Kingmaker.UI.LevelUp.ChargenUnit",
                "Kingmaker.UI.LevelUp.ChargenUnitData",
                "Kingmaker.Blueprints.Root.BlueprintRoot",
                "Kingmaker.Blueprints.Root.CharGenRoot",
                "Kingmaker.Blueprints.BlueprintUnit",
                "Kingmaker.UnitLogic.Class.LevelUp.LevelUpController",
                "Kingmaker.UnitLogic.Class.LevelUp.LevelUpState",
                "Kingmaker.UnitLogic.Class.LevelUp.Actions.SelectClass",
                "Kingmaker.UnitLogic.Class.LevelUp.Actions.ApplyClassMechanics",
                "Kingmaker.UnitLogic.Class.LevelUp.Actions.LevelUpHelper",
                "Kingmaker.UnitLogic.UnitDescriptor",
                "Kingmaker.UnitLogic.UnitProgressionData",
                "Kingmaker.UnitLogic.ClassData",
                "Kingmaker.EntitySystem.Entities.UnitEntityData"
            };
            Assembly assembly = typeof(Kingmaker.UnitLogic.UnitDescriptor).Assembly;
            var records = new List<string>();
            bool complete = true;
            foreach (string typeName in typeNames)
            {
                Type type = assembly.GetType(typeName, false, false);
                if (type == null) { complete = false; continue; }
                records.Add(DescribeCreationType(type));
            }
            Type controllerType = assembly.GetType(
                "Kingmaker.UnitLogic.Class.LevelUp.LevelUpController", false, false);
            MethodInfo startWithoutStatic = controllerType == null ? null :
                controllerType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic |
                    BindingFlags.Static).SingleOrDefault(value =>
                        value.Name == "StartWithoutAssigningStaticInstance" &&
                        value.GetParameters().Length == 5);
            Type buildModeType = startWithoutStatic == null ? null :
                startWithoutStatic.GetParameters()[4].ParameterType;
            if (buildModeType == null) complete = false;
            else records.Add(DescribeCreationType(buildModeType));
            Type descriptorType = typeof(Kingmaker.UnitLogic.UnitDescriptor);
            Type entityType = typeof(Kingmaker.EntitySystem.Entities.UnitEntityData);
            Type selectActionType = assembly.GetType(
                "Kingmaker.UnitLogic.Class.LevelUp.Actions.SelectClass", false, false);
            Type mechanicsActionType = assembly.GetType(
                "Kingmaker.UnitLogic.Class.LevelUp.Actions.ApplyClassMechanics", false, false);
            Type helperType = assembly.GetType(
                "Kingmaker.UnitLogic.Class.LevelUp.Actions.LevelUpHelper", false, false);
            string callGraph = "Commit=" + DescribeCalledMethods(
                controllerType.GetMethod("Commit", BindingFlags.Public |
                    BindingFlags.NonPublic | BindingFlags.Instance)) +
                " | SetupNewCharacher=" + DescribeCalledMethods(
                    controllerType.GetMethod("SetupNewCharacher", BindingFlags.Public |
                        BindingFlags.NonPublic | BindingFlags.Instance)) +
                " | AddStartingInventory=" + DescribeCalledMethods(
                    descriptorType.GetMethod("AddStartingInventory", BindingFlags.Public |
                        BindingFlags.NonPublic | BindingFlags.Instance)) +
                " | SelectClass.Apply=" + DescribeCalledMethods(
                    RequireExactApplyMethod(selectActionType)) +
                " | ApplyClassMechanics.Apply=" + DescribeCalledMethods(
                    RequireExactApplyMethod(mechanicsActionType)) +
                " | SetupNewCharacher.delegate=" + DescribeCalledMethods(
                    controllerType.GetMethod("<SetupNewCharacher>b__71_0",
                        BindingFlags.Public | BindingFlags.NonPublic |
                        BindingFlags.Instance)) +
                " | LevelUpHelper.AddStartingItems=" + DescribeCalledMethods(
                    helperType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic |
                        BindingFlags.Static).SingleOrDefault(value =>
                            value.Name == "AddStartingItems")) +
                " | UnitDescriptor.PrepareRespec=" + DescribeCalledMethods(
                    descriptorType.GetMethod("PrepareRespec", BindingFlags.Public |
                        BindingFlags.NonPublic | BindingFlags.Instance)) +
                " | UnitEntityData.PrepareRespec=" + DescribeCalledMethods(
                    entityType.GetMethod("PrepareRespec", BindingFlags.Public |
                        BindingFlags.NonPublic | BindingFlags.Instance)) +
                " | UnitDescriptor.Body.set=" + DescribeCalledMethods(
                    descriptorType.GetProperty("Body", BindingFlags.Public |
                        BindingFlags.NonPublic | BindingFlags.Instance).GetSetMethod(true)) +
                " | UnitDescriptor.Dispose=" + DescribeCalledMethods(
                    descriptorType.GetMethod("Dispose", BindingFlags.Public |
                        BindingFlags.NonPublic | BindingFlags.Instance)) +
                " | UnitEntityData.Dispose=" + DescribeCalledMethods(
                    entityType.GetMethod("Dispose", BindingFlags.Public |
                        BindingFlags.NonPublic | BindingFlags.Instance)) +
                " | LevelUpController.StartWithoutStatic=" +
                    DescribeCalledMethods(startWithoutStatic) +
                " | LevelUpController.ctor=" + DescribeCalledMethods(
                    controllerType.GetConstructors(BindingFlags.Public |
                        BindingFlags.NonPublic | BindingFlags.Instance)
                        .SingleOrDefault(value => value.GetParameters().Length == 4)) +
                " | LevelUpController.RequestPreview=" + DescribeCalledMethods(
                    controllerType.GetMethod("RequestPreview", BindingFlags.Public |
                        BindingFlags.NonPublic | BindingFlags.Instance));
            string observed = string.Join(" | ", records.ToArray());
            BlueprintRoot root = BlueprintRoot.Instance;
            BlueprintUnit defaultPlayer = root == null ? null : root.DefaultPlayerCharacter;
            BlueprintUnit[] pregens = root == null || root.CharGen == null
                ? null : root.CharGen.Pregens;
            string rootedUnits = "default=" + DescribeBlueprintUnit(defaultPlayer) +
                ";pregens=" + string.Join(",", (pregens ?? new BlueprintUnit[0])
                    .Where(value => value != null).Select(DescribeBlueprintUnit)
                    .OrderBy(value => value, StringComparer.Ordinal).ToArray());
            var assertions = new List<RuntimeTestAssertion>
            {
                Assertion("creation-contracts", "all exact creation types and declared contracts",
                    observed, complete && records.Count == typeNames.Length + 1,
                    "Assembly-CSharp runtime Type constructors, methods, fields, and properties"),
                Assertion("rooted-unit-contracts", "exact default-player and pregen identities",
                    rootedUnits, defaultPlayer != null && pregens != null && pregens.Length > 0 &&
                        pregens.All(value => value != null),
                    "BlueprintRoot.Instance direct fields; no CharGenRoot method invocation"),
                Assertion("creation-call-graph", "exact commit/setup/inventory called methods",
                    callGraph, !callGraph.Contains("<unavailable>"),
                    "MethodBody IL call/callvirt tokens resolved without method invocation"),
                Assertion("respec-call-graph", "exact descriptor and entity respec calls",
                    callGraph, callGraph.Contains("UnitDescriptor.PrepareRespec=") &&
                        callGraph.Contains("UnitEntityData.PrepareRespec=") &&
                        callGraph.Contains("UnitDescriptor.Body.set=") &&
                        callGraph.Contains("UnitDescriptor.Dispose=") &&
                        callGraph.Contains("UnitEntityData.Dispose=") &&
                        callGraph.Contains("LevelUpController.StartWithoutStatic=") &&
                        callGraph.Contains("LevelUpController.ctor=") &&
                        callGraph.Contains("LevelUpController.RequestPreview=") &&
                        !callGraph.Contains("PrepareRespec=<unavailable>"),
                    "metadata-only MethodBody IL; no respec or cleanup method invoked"),
                Assertion("observation-only", "no unit construction or game-state mutation",
                    "metadata-only reflection", true,
                    "scenario invokes no constructor, method, save, input, or registry mutation"),
                Assertion("loaded-mod-version", _request.ExpectedModVersion,
                    _context.ModEntry.Info.Version,
                    _request.ExpectedModVersion == _context.ModEntry.Info.Version,
                    "Unity Mod Manager ModEntry.Info.Version")
            };
            bool pass = assertions.TrueForAll(value => value.Status == "PASS");
            return CreateResult(pass ? RuntimeTestStatuses.Pass :
                RuntimeTestStatuses.Fail, assertions, null);
        }

        private RuntimeTestResult RunDisposableDescriptorConstruction()
        {
            BlueprintRoot root = BlueprintRoot.Instance;
            BlueprintUnit source = root == null ? null : root.DefaultPlayerCharacter;
            object player = ReadExactMember(Kingmaker.Game.Instance, "Player");
            object state = ReadExactMember(Kingmaker.Game.Instance, "State");
            object party = ReadExactMember(player, "Party");
            object allUnits = ReadExactMember(state, "AllUnits");
            object[] partyBefore = SnapshotReferences(party);
            object[] unitsBefore = SnapshotReferences(allUnits);
            Kingmaker.UnitLogic.UnitDescriptor descriptor = null;
            string observed = "";
            bool isolated = false;
            bool cleaned = false;
            try
            {
                if (source == null || source.AssetGuid !=
                    "4391e8b9afbb0cf43aeba700c089f56d")
                    throw new InvalidOperationException(
                        "Exact default-player blueprint source is unavailable.");
                descriptor = (Kingmaker.UnitLogic.UnitDescriptor)Activator.CreateInstance(
                    typeof(Kingmaker.UnitLogic.UnitDescriptor),
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,
                    null, new object[] { source }, null);
                isolated = descriptor != null &&
                    ReferenceEquals(descriptor.Blueprint, source) && descriptor.Unit == null &&
                    !ContainsReference(party, descriptor) && !ContainsReference(allUnits, descriptor) &&
                    SameReferences(partyBefore, SnapshotReferences(party)) &&
                    SameReferences(unitsBefore, SnapshotReferences(allUnits));
                observed = "source=" + DescribeBlueprintUnit(source) +
                    ";descriptor=" + (descriptor == null ? "missing" : "constructed") +
                    ";unit=" + (descriptor == null || descriptor.Unit == null ? "none" : "attached") +
                    ";partyBefore=" + partyBefore.Length + ";unitsBefore=" + unitsBefore.Length;
            }
            finally
            {
                if (descriptor != null) descriptor.Dispose();
                cleaned = SameReferences(partyBefore, SnapshotReferences(party)) &&
                    SameReferences(unitsBefore, SnapshotReferences(allUnits)) &&
                    (descriptor == null || !ContainsReference(party, descriptor)) &&
                    (descriptor == null || !ContainsReference(allUnits, descriptor));
            }
            var assertions = new List<RuntimeTestAssertion>
            {
                Assertion("disposable-construction", "detached exact-source unit descriptor",
                    observed, isolated,
                    "exact reflected UnitDescriptor(BlueprintUnit); no entity, party, or Game.State.AllUnits attachment"),
                Assertion("cleanup", "unchanged party and global unit snapshots after Dispose",
                    "cleaned=" + cleaned, cleaned,
                    "UnitDescriptor.Dispose plus reference-identity snapshots"),
                Assertion("loaded-mod-version", _request.ExpectedModVersion,
                    _context.ModEntry.Info.Version,
                    _request.ExpectedModVersion == _context.ModEntry.Info.Version,
                    "Unity Mod Manager ModEntry.Info.Version")
            };
            bool pass = assertions.TrueForAll(value => value.Status == "PASS");
            return CreateResult(pass ? RuntimeTestStatuses.Pass :
                RuntimeTestStatuses.Fail, assertions, null);
        }

        private RuntimeTestResult RunDisposableGunslingerSelection()
        {
            BlueprintUnit source = BlueprintRoot.Instance.DefaultPlayerCharacter;
            BlueprintCharacterClass gunslinger = BlueprintBootstrap.GunslingerClass.CharacterClass;
            object player = ReadExactMember(Kingmaker.Game.Instance, "Player");
            object state = ReadExactMember(Kingmaker.Game.Instance, "State");
            object party = ReadExactMember(player, "Party");
            object allUnits = ReadExactMember(state, "AllUnits");
            object[] partyBefore = SnapshotReferences(party);
            object[] unitsBefore = SnapshotReferences(allUnits);
            Kingmaker.UI.LevelUp.ChargenUnit chargen = null;
            Kingmaker.EntitySystem.Entities.UnitEntityData entity = null;
            Kingmaker.UnitLogic.UnitDescriptor descriptor = null;
            object controller = null;
            int beforeLevel = -1;
            int selectedLevel = -1;
            int canceledLevel = -1;
            bool selected = false;
            bool stateSelected = false;
            string queuedActions = "";
            bool cleaned = false;
            try
            {
                chargen = new Kingmaker.UI.LevelUp.ChargenUnit(source);
                entity = chargen.Unit;
                descriptor = entity.Descriptor;
                beforeLevel = descriptor.Progression.GetClassLevel(gunslinger);
                Type controllerType = typeof(Kingmaker.UnitLogic.Class.LevelUp.LevelUpController);
                MethodInfo start = controllerType.GetMethods(BindingFlags.Public |
                    BindingFlags.NonPublic | BindingFlags.Static).Single(value =>
                        value.Name == "StartWithoutAssigningStaticInstance" &&
                        value.GetParameters().Length == 5);
                Type modeType = start.GetParameters()[4].ParameterType;
                object charGen = Enum.Parse(modeType, "CharGen", false);
                controller = start.Invoke(null,
                    new object[] { descriptor, false, null, null, charGen });
                MethodInfo selectClass = controllerType.GetMethod("SelectClass",
                    BindingFlags.Public | BindingFlags.Instance, null,
                    new[] { typeof(BlueprintCharacterClass), typeof(bool) }, null);
                selected = (bool)selectClass.Invoke(controller,
                    new object[] { gunslinger, false });
                controllerType.GetMethod("ApplyClassMechanics",
                    BindingFlags.Public | BindingFlags.Instance).Invoke(controller, null);
                selectedLevel = descriptor.Progression.GetClassLevel(gunslinger);
                object levelUpState = ReadExactMember(controller, "State");
                stateSelected = ReferenceEquals(
                    ReadExactMember(levelUpState, "SelectedClass"), gunslinger);
                queuedActions = string.Join(",", SnapshotReferences(
                    ReadExactMember(controller, "LevelUpActions"))
                    .Select(value => value.GetType().FullName).ToArray());
            }
            finally
            {
                if (controller != null)
                    controller.GetType().GetMethod("Cancel",
                        BindingFlags.Public | BindingFlags.Instance).Invoke(controller, null);
                if (descriptor != null)
                    canceledLevel = descriptor.Progression.GetClassLevel(gunslinger);
                if (entity != null) entity.Dispose();
                cleaned = SameReferences(partyBefore, SnapshotReferences(party)) &&
                    SameReferences(unitsBefore, SnapshotReferences(allUnits)) &&
                    (entity == null || !ContainsReference(party, entity)) &&
                    (entity == null || !ContainsReference(allUnits, entity));
            }
            string observed = "before=" + beforeLevel + ";selected=" + selected +
                ";stateSelected=" + stateSelected + ";queued=" + queuedActions +
                ";sourceLevel=" + selectedLevel + ";canceledLevel=" + canceledLevel;
            var assertions = new List<RuntimeTestAssertion>
            {
                Assertion("gunslinger-selection", "exact isolated selection state and queued mechanics",
                    observed, selected && stateSelected && beforeLevel == 0 &&
                        selectedLevel == 0 &&
                        queuedActions.Contains(".Actions.SelectClass") &&
                        queuedActions.Contains(".Actions.ApplyClassMechanics"),
                    "controller State.SelectedClass and exact LevelUpActions type identities"),
                Assertion("cancel-rollback", "zero Gunslinger levels after Cancel",
                    "canceledLevel=" + canceledLevel, canceledLevel == 0,
                    "LevelUpController.Cancel then exact GetClassLevel"),
                Assertion("external-isolation", "unchanged party and global-unit snapshots",
                    "cleaned=" + cleaned, cleaned,
                    "native ChargenUnit preview owner; reference snapshots after entity disposal"),
                Assertion("loaded-mod-version", _request.ExpectedModVersion,
                    _context.ModEntry.Info.Version,
                    _request.ExpectedModVersion == _context.ModEntry.Info.Version,
                    "Unity Mod Manager ModEntry.Info.Version")
            };
            bool pass = assertions.TrueForAll(value => value.Status == "PASS");
            return CreateResult(pass ? RuntimeTestStatuses.Pass :
                RuntimeTestStatuses.Fail, assertions, null);
        }

        private RuntimeTestResult RunDisposableGunslingerPreviewApplication()
        {
            BlueprintUnit source = BlueprintRoot.Instance.DefaultPlayerCharacter;
            BlueprintCharacterClass gunslinger = BlueprintBootstrap.GunslingerClass.CharacterClass;
            object player = ReadExactMember(Kingmaker.Game.Instance, "Player");
            object state = ReadExactMember(Kingmaker.Game.Instance, "State");
            object party = ReadExactMember(player, "Party");
            object allUnits = ReadExactMember(state, "AllUnits");
            object[] partyBefore = SnapshotReferences(party);
            object[] unitsBefore = SnapshotReferences(allUnits);
            Kingmaker.UI.LevelUp.ChargenUnit chargen = null;
            Kingmaker.EntitySystem.Entities.UnitEntityData sourceEntity = null;
            Kingmaker.UnitLogic.UnitDescriptor sourceDescriptor = null;
            Kingmaker.UnitLogic.UnitDescriptor preview = null;
            Kingmaker.UnitLogic.UnitDescriptor refreshedPreview = null;
            object controller = null;
            bool selected = false;
            int sourceBefore = -1;
            int previewBefore = -1;
            int previewAfterSelection = -1;
            int previewAfter = -1;
            int sourceAfter = -1;
            int queuedCount = -1;
            int classDataLevel = -1;
            string baseAttack = "";
            string fortitude = "";
            string reflex = "";
            string will = "";
            string featureStoreContract = "";
            string refreshedFeatures = "";
            bool cleaned = false;
            try
            {
                if (source == null || gunslinger == null)
                    throw new InvalidOperationException(
                        "Exact source or production Gunslinger class is unavailable.");
                chargen = new Kingmaker.UI.LevelUp.ChargenUnit(source);
                sourceEntity = chargen.Unit;
                if (sourceEntity == null || sourceEntity.Descriptor == null ||
                    sourceEntity.Descriptor.Progression == null)
                    throw new InvalidOperationException(
                        "Native ChargenUnit source descriptor/progression is unavailable.");
                sourceDescriptor = sourceEntity.Descriptor;
                sourceBefore = sourceDescriptor.Progression.GetClassLevel(gunslinger);
                Type controllerType = typeof(Kingmaker.UnitLogic.Class.LevelUp.LevelUpController);
                MethodInfo start = controllerType.GetMethods(BindingFlags.Public |
                    BindingFlags.NonPublic | BindingFlags.Static).Single(value =>
                        value.Name == "StartWithoutAssigningStaticInstance" &&
                        value.GetParameters().Length == 5);
                object charGen = Enum.Parse(start.GetParameters()[4].ParameterType,
                    "CharGen", false);
                controller = start.Invoke(null,
                    new object[] { sourceDescriptor, false, null, null, charGen });
                preview = ReadExactMember(controller, "Preview") as
                    Kingmaker.UnitLogic.UnitDescriptor;
                if (preview == null || ReferenceEquals(preview, sourceDescriptor))
                    throw new InvalidOperationException(
                        "Controller-owned preview descriptor is unavailable or aliases source.");
                if (preview.Progression == null)
                    throw new InvalidOperationException(
                        "Controller-owned preview progression is unavailable.");
                previewBefore = preview.Progression.GetClassLevel(gunslinger);
                MethodInfo selectClass = controllerType.GetMethod("SelectClass",
                    BindingFlags.Public | BindingFlags.Instance, null,
                    new[] { typeof(BlueprintCharacterClass), typeof(bool) }, null);
                if (selectClass == null)
                    throw new InvalidOperationException("Exact SelectClass method is unavailable.");
                selected = (bool)selectClass.Invoke(controller,
                    new object[] { gunslinger, false });
                previewAfterSelection = preview.Progression.GetClassLevel(gunslinger);
                MethodInfo applyClassMechanics = controllerType.GetMethod(
                    "ApplyClassMechanics", BindingFlags.Public | BindingFlags.Instance);
                if (applyClassMechanics == null)
                    throw new InvalidOperationException(
                        "Exact ApplyClassMechanics method is unavailable.");
                applyClassMechanics.Invoke(controller, null);
                queuedCount = SnapshotReferences(
                    ReadExactMember(controller, "LevelUpActions")).Length;
                previewAfter = preview.Progression.GetClassLevel(gunslinger);
                sourceAfter = sourceDescriptor.Progression.GetClassLevel(gunslinger);
                MethodInfo updatePreview = controllerType.GetMethod("UpdatePreview",
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (updatePreview == null)
                    throw new InvalidOperationException("Exact UpdatePreview method is unavailable.");
                updatePreview.Invoke(controller, null);
                refreshedPreview = ReadExactMember(controller, "Preview") as
                    Kingmaker.UnitLogic.UnitDescriptor;
                if (refreshedPreview == null || refreshedPreview.Progression == null)
                    throw new InvalidOperationException(
                        "Refreshed controller preview/progression is unavailable.");
                object featureStore = refreshedPreview.Progression.Features;
                if (featureStore == null)
                    throw new InvalidOperationException(
                        "Refreshed preview feature store is unavailable.");
                featureStoreContract = DescribeCreationType(featureStore.GetType());
                PropertyInfo enumerableProperty = featureStore.GetType().GetProperty(
                    "Enumerable", BindingFlags.Public | BindingFlags.NonPublic |
                    BindingFlags.Instance | BindingFlags.DeclaredOnly);
                if (enumerableProperty == null)
                    throw new InvalidOperationException(
                        "Exact declared FeatureCollection.Enumerable is unavailable.");
                object enumerableFeatures = enumerableProperty.GetValue(featureStore, null);
                refreshedFeatures = string.Join(",", SnapshotReferences(
                    enumerableFeatures).Select(value =>
                    {
                        BlueprintScriptableObject blueprint =
                            ReadExactMember(value, "Blueprint") as BlueprintScriptableObject;
                        return blueprint == null ? "<missing>" : blueprint.AssetGuid;
                    }).OrderBy(value => value, StringComparer.Ordinal).ToArray());
                Kingmaker.UnitLogic.ClassData classData =
                    preview.Progression.GetClassData(gunslinger);
                if (classData == null || !ReferenceEquals(classData.CharacterClass, gunslinger))
                    throw new InvalidOperationException(
                        "Exact Gunslinger preview ClassData is unavailable.");
                classDataLevel = classData.Level;
                baseAttack = classData.BaseAttackBonus.AssetGuid;
                fortitude = classData.FortitudeSave.AssetGuid;
                reflex = classData.ReflexSave.AssetGuid;
                will = classData.WillSave.AssetGuid;
            }
            finally
            {
                if (controller != null)
                    controller.GetType().GetMethod("Cancel",
                        BindingFlags.Public | BindingFlags.Instance).Invoke(controller, null);
                if (sourceEntity != null) sourceEntity.Dispose();
                cleaned = SameReferences(partyBefore, SnapshotReferences(party)) &&
                    SameReferences(unitsBefore, SnapshotReferences(allUnits)) &&
                    (sourceEntity == null || !ContainsReference(party, sourceEntity)) &&
                    (sourceEntity == null || !ContainsReference(allUnits, sourceEntity));
            }
            string observed = "selected=" + selected + ";sourceBefore=" + sourceBefore +
                ";previewBefore=" + previewBefore +
                ";previewAfterSelection=" + previewAfterSelection +
                ";previewAfterMechanics=" + previewAfter +
                ";sourceAfter=" + sourceAfter + ";queuedCount=" + queuedCount +
                ";classData=" + classDataLevel + "/" + baseAttack + "/" +
                fortitude + "/" + reflex + "/" + will;
            var assertions = new List<RuntimeTestAssertion>
            {
                Assertion("preview-application", "level-one Gunslinger only on controller preview",
                    observed, selected && sourceBefore == 0 && previewBefore == 0 &&
                        previewAfterSelection == 1 && previewAfter == 1 &&
                        sourceAfter == 0 && queuedCount == 2,
                    "native controller preview refresh and exact GetClassLevel identities"),
                Assertion("preview-class-data", "level 1 and exact BAB/save identities",
                    "classData=" + classDataLevel + "/" + baseAttack + "/" +
                        fortitude + "/" + reflex + "/" + will,
                    classDataLevel == 1 && baseAttack ==
                        "b3057560ffff3514299e8b93e7648a9d" && fortitude ==
                        "ff4662bde9e75f145853417313842751" && reflex ==
                        "ff4662bde9e75f145853417313842751" && will ==
                        "dc0c7c1aba755c54f96c089cdf7d14a3",
                    "ClassData exact progression identities"),
                Assertion("preview-feature-store-contract", "exact runtime feature-store metadata",
                    featureStoreContract,
                    refreshedPreview != null &&
                        refreshedPreview.Progression.GetClassLevel(gunslinger) == 1 &&
                        !string.IsNullOrEmpty(featureStoreContract) &&
                        sourceDescriptor.Progression.GetClassLevel(gunslinger) == 0,
                    "single exact Progression.Features receiver; no feature-store method invocation"),
                Assertion("preview-proficiency-aggregate", "one exact proficiency aggregate feature",
                    "features=" + refreshedFeatures,
                    new[]
                    {
                        "b9b6769f8a654a58a6bd55e10801ea22",
                        "e70ecf1ed95ca2f40b754f1adb22bbdd",
                        "203992ef5b35c864390b4e4a1e200629",
                        "6d3728d4e9c9898458fe5e9532951132",
                        "5148f69223044799800b65732b6cabea"
                    }.All(required => refreshedFeatures.Split(new[] { ',' },
                        StringSplitOptions.RemoveEmptyEntries).Count(value =>
                            value == required) == 1),
                    "FeatureCollection.Enumerable and Feature.Blueprint identities"),
                Assertion("external-isolation", "unchanged party and global-unit snapshots",
                    "cleaned=" + cleaned, cleaned,
                    "Cancel plus source preview-entity disposal and reference snapshots"),
                Assertion("loaded-mod-version", _request.ExpectedModVersion,
                    _context.ModEntry.Info.Version,
                    _request.ExpectedModVersion == _context.ModEntry.Info.Version,
                    "Unity Mod Manager ModEntry.Info.Version")
            };
            bool pass = assertions.TrueForAll(value => value.Status == "PASS");
            return CreateResult(pass ? RuntimeTestStatuses.Pass :
                RuntimeTestStatuses.Fail, assertions, null);
        }

        private RuntimeTestResult RunDisposableGunslingerLevelUpPreview()
        {
            BlueprintUnit source = BlueprintRoot.Instance.DefaultPlayerCharacter;
            BlueprintCharacterClass gunslinger = BlueprintBootstrap.GunslingerClass.CharacterClass;
            object player = ReadExactMember(Kingmaker.Game.Instance, "Player");
            object state = ReadExactMember(Kingmaker.Game.Instance, "State");
            object party = ReadExactMember(player, "Party");
            object allUnits = ReadExactMember(state, "AllUnits");
            object[] partyBefore = SnapshotReferences(party);
            object[] unitsBefore = SnapshotReferences(allUnits);
            Kingmaker.UI.LevelUp.ChargenUnit chargen = null;
            Kingmaker.EntitySystem.Entities.UnitEntityData entity = null;
            Kingmaker.UnitLogic.UnitDescriptor descriptor = null;
            object seedController = null;
            object levelController = null;
            int initialLevel = -1;
            int seededLevel = -1;
            int previewBefore = -1;
            int previewAfter = -1;
            int sourceAfter = -1;
            int queuedCount = -1;
            bool selected = false;
            bool cleaned = false;
            try
            {
                chargen = new Kingmaker.UI.LevelUp.ChargenUnit(source);
                entity = chargen.Unit;
                descriptor = entity == null ? null : entity.Descriptor;
                if (descriptor == null || descriptor.Progression == null)
                    throw new InvalidOperationException(
                        "Disposable level-up source descriptor is unavailable.");
                initialLevel = descriptor.Progression.GetClassLevel(gunslinger);
                Type controllerType = typeof(Kingmaker.UnitLogic.Class.LevelUp.LevelUpController);
                MethodInfo start = controllerType.GetMethods(BindingFlags.Public |
                    BindingFlags.NonPublic | BindingFlags.Static).Single(value =>
                        value.Name == "StartWithoutAssigningStaticInstance" &&
                        value.GetParameters().Length == 5);
                MethodInfo selectClass = controllerType.GetMethod("SelectClass",
                    BindingFlags.Public | BindingFlags.Instance, null,
                    new[] { typeof(BlueprintCharacterClass), typeof(bool) }, null);
                MethodInfo mechanics = controllerType.GetMethod("ApplyClassMechanics",
                    BindingFlags.Public | BindingFlags.Instance);
                MethodInfo applyLevelup = controllerType.GetMethod("ApplyLevelup",
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                MethodInfo cancel = controllerType.GetMethod("Cancel",
                    BindingFlags.Public | BindingFlags.Instance);
                if (selectClass == null || mechanics == null || applyLevelup == null ||
                    cancel == null)
                    throw new MissingMethodException(
                        "An exact native level-up controller method is unavailable.");

                object charGen = Enum.Parse(start.GetParameters()[4].ParameterType,
                    "CharGen", false);
                seedController = start.Invoke(null,
                    new object[] { descriptor, false, null, null, charGen });
                if (!(bool)selectClass.Invoke(seedController,
                    new object[] { gunslinger, false }))
                    throw new InvalidOperationException(
                        "Disposable Gunslinger level-one seed selection was rejected.");
                mechanics.Invoke(seedController, null);
                applyLevelup.Invoke(seedController, new object[] { descriptor });
                cancel.Invoke(seedController, null);
                seedController = null;
                seededLevel = descriptor.Progression.GetClassLevel(gunslinger);

                object levelUp = Enum.Parse(start.GetParameters()[4].ParameterType,
                    "LevelUp", false);
                levelController = start.Invoke(null,
                    new object[] { descriptor, false, null, null, levelUp });
                Kingmaker.UnitLogic.UnitDescriptor preview =
                    ReadExactMember(levelController, "Preview") as
                        Kingmaker.UnitLogic.UnitDescriptor;
                if (preview == null || preview.Progression == null ||
                    ReferenceEquals(preview, descriptor))
                    throw new InvalidOperationException(
                        "Exact isolated LevelUp preview is unavailable.");
                previewBefore = preview.Progression.GetClassLevel(gunslinger);
                selected = (bool)selectClass.Invoke(levelController,
                    new object[] { gunslinger, false });
                mechanics.Invoke(levelController, null);
                queuedCount = SnapshotReferences(
                    ReadExactMember(levelController, "LevelUpActions")).Length;
                previewAfter = preview.Progression.GetClassLevel(gunslinger);
                sourceAfter = descriptor.Progression.GetClassLevel(gunslinger);
            }
            finally
            {
                MethodInfo cancel = typeof(Kingmaker.UnitLogic.Class.LevelUp.LevelUpController)
                    .GetMethod("Cancel", BindingFlags.Public | BindingFlags.Instance);
                if (levelController != null && cancel != null)
                    cancel.Invoke(levelController, null);
                if (seedController != null && cancel != null)
                    cancel.Invoke(seedController, null);
                if (entity != null) entity.Dispose();
                cleaned = SameReferences(partyBefore, SnapshotReferences(party)) &&
                    SameReferences(unitsBefore, SnapshotReferences(allUnits)) &&
                    (entity == null || !ContainsReference(party, entity)) &&
                    (entity == null || !ContainsReference(allUnits, entity));
            }
            string observed = "initial=" + initialLevel + ";seeded=" + seededLevel +
                ";previewBefore=" + previewBefore + ";selected=" + selected +
                ";previewAfter=" + previewAfter + ";sourceAfter=" + sourceAfter +
                ";queued=" + queuedCount;
            var assertions = new List<RuntimeTestAssertion>
            {
                Assertion("level-one-seed", "isolated source Gunslinger 0 -> 1",
                    observed, initialLevel == 0 && seededLevel == 1,
                    "exact ApplyLevelup on disposable source only"),
                Assertion("same-class-levelup-preview",
                    "source remains 1; isolated LevelUp preview reaches Gunslinger 2",
                    observed, selected && previewBefore == 1 && previewAfter == 2 &&
                        sourceAfter == 1 && queuedCount == 2,
                    "native LevelUp mode, SelectClass, and ApplyClassMechanics"),
                Assertion("external-isolation", "unchanged party and global-unit snapshots",
                    "cleaned=" + cleaned, cleaned,
                    "both controllers canceled and disposable entity disposed"),
                Assertion("loaded-mod-version", _request.ExpectedModVersion,
                    _context.ModEntry.Info.Version,
                    _request.ExpectedModVersion == _context.ModEntry.Info.Version,
                    "Unity Mod Manager ModEntry.Info.Version")
            };
            bool pass = assertions.TrueForAll(value => value.Status == "PASS");
            return CreateResult(pass ? RuntimeTestStatuses.Pass :
                RuntimeTestStatuses.Fail, assertions, null);
        }

        private RuntimeTestResult RunDisposableGunslingerMulticlassPreview()
        {
            BlueprintUnit source = BlueprintRoot.Instance.DefaultPlayerCharacter;
            BlueprintCharacterClass gunslinger = BlueprintBootstrap.GunslingerClass.CharacterClass;
            BlueprintCharacterClass fighter = BlueprintRoot.Instance.Progression.CharacterClasses
                .Single(value => value != null && value.AssetGuid ==
                    "48ac8db94d5de7645906c7d0ad3bcfbd");
            object player = ReadExactMember(Kingmaker.Game.Instance, "Player");
            object state = ReadExactMember(Kingmaker.Game.Instance, "State");
            object party = ReadExactMember(player, "Party");
            object allUnits = ReadExactMember(state, "AllUnits");
            object[] partyBefore = SnapshotReferences(party);
            object[] unitsBefore = SnapshotReferences(allUnits);
            Kingmaker.UI.LevelUp.ChargenUnit chargen = null;
            Kingmaker.EntitySystem.Entities.UnitEntityData entity = null;
            Kingmaker.UnitLogic.UnitDescriptor descriptor = null;
            object seedController = null;
            object levelController = null;
            int fighterBefore = -1;
            int fighterSeeded = -1;
            int sourceGunslingerBefore = -1;
            int previewFighter = -1;
            int previewGunslingerBefore = -1;
            int previewGunslingerAfter = -1;
            int sourceFighterAfter = -1;
            int sourceGunslingerAfter = -1;
            int queuedCount = -1;
            bool selected = false;
            bool cleaned = false;
            try
            {
                chargen = new Kingmaker.UI.LevelUp.ChargenUnit(source);
                entity = chargen.Unit;
                descriptor = entity == null ? null : entity.Descriptor;
                if (descriptor == null || descriptor.Progression == null)
                    throw new InvalidOperationException(
                        "Disposable multiclass source descriptor is unavailable.");
                fighterBefore = descriptor.Progression.GetClassLevel(fighter);
                sourceGunslingerBefore = descriptor.Progression.GetClassLevel(gunslinger);
                Type controllerType = typeof(Kingmaker.UnitLogic.Class.LevelUp.LevelUpController);
                MethodInfo start = controllerType.GetMethods(BindingFlags.Public |
                    BindingFlags.NonPublic | BindingFlags.Static).Single(value =>
                        value.Name == "StartWithoutAssigningStaticInstance" &&
                        value.GetParameters().Length == 5);
                MethodInfo selectClass = controllerType.GetMethod("SelectClass",
                    BindingFlags.Public | BindingFlags.Instance, null,
                    new[] { typeof(BlueprintCharacterClass), typeof(bool) }, null);
                MethodInfo mechanics = controllerType.GetMethod("ApplyClassMechanics",
                    BindingFlags.Public | BindingFlags.Instance);
                MethodInfo applyLevelup = controllerType.GetMethod("ApplyLevelup",
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                MethodInfo cancel = controllerType.GetMethod("Cancel",
                    BindingFlags.Public | BindingFlags.Instance);
                if (selectClass == null || mechanics == null || applyLevelup == null ||
                    cancel == null)
                    throw new MissingMethodException(
                        "An exact native multiclass controller method is unavailable.");

                object charGen = Enum.Parse(start.GetParameters()[4].ParameterType,
                    "CharGen", false);
                seedController = start.Invoke(null,
                    new object[] { descriptor, false, null, null, charGen });
                if (!(bool)selectClass.Invoke(seedController,
                    new object[] { fighter, false }))
                    throw new InvalidOperationException(
                        "Disposable Fighter level-one seed selection was rejected.");
                mechanics.Invoke(seedController, null);
                applyLevelup.Invoke(seedController, new object[] { descriptor });
                cancel.Invoke(seedController, null);
                seedController = null;
                fighterSeeded = descriptor.Progression.GetClassLevel(fighter);

                object levelUp = Enum.Parse(start.GetParameters()[4].ParameterType,
                    "LevelUp", false);
                levelController = start.Invoke(null,
                    new object[] { descriptor, false, null, null, levelUp });
                Kingmaker.UnitLogic.UnitDescriptor preview =
                    ReadExactMember(levelController, "Preview") as
                        Kingmaker.UnitLogic.UnitDescriptor;
                if (preview == null || preview.Progression == null ||
                    ReferenceEquals(preview, descriptor))
                    throw new InvalidOperationException(
                        "Exact isolated multiclass preview is unavailable.");
                previewFighter = preview.Progression.GetClassLevel(fighter);
                previewGunslingerBefore = preview.Progression.GetClassLevel(gunslinger);
                selected = (bool)selectClass.Invoke(levelController,
                    new object[] { gunslinger, false });
                mechanics.Invoke(levelController, null);
                queuedCount = SnapshotReferences(
                    ReadExactMember(levelController, "LevelUpActions")).Length;
                previewGunslingerAfter = preview.Progression.GetClassLevel(gunslinger);
                sourceFighterAfter = descriptor.Progression.GetClassLevel(fighter);
                sourceGunslingerAfter = descriptor.Progression.GetClassLevel(gunslinger);
            }
            finally
            {
                MethodInfo cancel = typeof(Kingmaker.UnitLogic.Class.LevelUp.LevelUpController)
                    .GetMethod("Cancel", BindingFlags.Public | BindingFlags.Instance);
                if (levelController != null && cancel != null)
                    cancel.Invoke(levelController, null);
                if (seedController != null && cancel != null)
                    cancel.Invoke(seedController, null);
                if (entity != null) entity.Dispose();
                cleaned = SameReferences(partyBefore, SnapshotReferences(party)) &&
                    SameReferences(unitsBefore, SnapshotReferences(allUnits)) &&
                    (entity == null || !ContainsReference(party, entity)) &&
                    (entity == null || !ContainsReference(allUnits, entity));
            }
            string observed = "fighterBefore=" + fighterBefore +
                ";fighterSeeded=" + fighterSeeded +
                ";gunslingerBefore=" + sourceGunslingerBefore +
                ";previewFighter=" + previewFighter +
                ";previewGunslingerBefore=" + previewGunslingerBefore +
                ";selected=" + selected +
                ";previewGunslingerAfter=" + previewGunslingerAfter +
                ";sourceFighterAfter=" + sourceFighterAfter +
                ";sourceGunslingerAfter=" + sourceGunslingerAfter +
                ";queued=" + queuedCount;
            var assertions = new List<RuntimeTestAssertion>
            {
                Assertion("fighter-level-one-seed", "isolated Fighter 0 -> 1",
                    observed, fighterBefore == 0 && fighterSeeded == 1 &&
                        sourceGunslingerBefore == 0,
                    "exact Fighter blueprint and disposable ApplyLevelup"),
                Assertion("gunslinger-multiclass-preview",
                    "preview Fighter 1/Gunslinger 1; source Fighter 1/Gunslinger 0",
                    observed, selected && previewFighter == 1 &&
                        previewGunslingerBefore == 0 && previewGunslingerAfter == 1 &&
                        sourceFighterAfter == 1 && sourceGunslingerAfter == 0 &&
                        queuedCount == 2,
                    "native LevelUp mode Gunslinger class selection"),
                Assertion("external-isolation", "unchanged party and global-unit snapshots",
                    "cleaned=" + cleaned, cleaned,
                    "both controllers canceled and disposable entity disposed"),
                Assertion("loaded-mod-version", _request.ExpectedModVersion,
                    _context.ModEntry.Info.Version,
                    _request.ExpectedModVersion == _context.ModEntry.Info.Version,
                    "Unity Mod Manager ModEntry.Info.Version")
            };
            bool pass = assertions.TrueForAll(value => value.Status == "PASS");
            return CreateResult(pass ? RuntimeTestStatuses.Pass :
                RuntimeTestStatuses.Fail, assertions, null);
        }

        private RuntimeTestResult RunDisposableGunslingerRespecPreview()
        {
            BlueprintUnit source = BlueprintRoot.Instance.DefaultPlayerCharacter;
            BlueprintCharacterClass gunslinger = BlueprintBootstrap.GunslingerClass.CharacterClass;
            BlueprintCharacterClass fighter = BlueprintRoot.Instance.Progression.CharacterClasses
                .Single(value => value != null && value.AssetGuid ==
                    "48ac8db94d5de7645906c7d0ad3bcfbd");
            object player = ReadExactMember(Kingmaker.Game.Instance, "Player");
            object state = ReadExactMember(Kingmaker.Game.Instance, "State");
            object party = ReadExactMember(player, "Party");
            object allUnits = ReadExactMember(state, "AllUnits");
            object[] partyBefore = SnapshotReferences(party);
            object[] unitsBefore = SnapshotReferences(allUnits);
            Kingmaker.EntitySystem.Entities.UnitEntityData entity = null;
            Kingmaker.UnitLogic.UnitDescriptor descriptor = null;
            Kingmaker.EntitySystem.Entities.UnitEntityData respecEntity = null;
            Kingmaker.UnitLogic.UnitDescriptor respecDescriptor = null;
            object seedController = null;
            object respecController = null;
            int fighterSeeded = -1;
            int previewFighterBefore = -1;
            int previewGunslingerBefore = -1;
            int previewGunslingerAfter = -1;
            int sourceFighterAfter = -1;
            int sourceGunslingerAfter = -1;
            int queuedCount = -1;
            bool bodyPreserved = false;
            bool selected = false;
            bool cleaned = false;
            string stage = "construct-disposable";
            try
            {
                var chargen = new Kingmaker.UI.LevelUp.ChargenUnit(source);
                entity = chargen.Unit;
                descriptor = entity == null ? null : entity.Descriptor;
                if (descriptor == null || descriptor.Progression == null)
                    throw new InvalidOperationException(
                        "Disposable respec source descriptor is unavailable.");
                Type controllerType = typeof(Kingmaker.UnitLogic.Class.LevelUp.LevelUpController);
                MethodInfo start = controllerType.GetMethods(BindingFlags.Public |
                    BindingFlags.NonPublic | BindingFlags.Static).Single(value =>
                        value.Name == "StartWithoutAssigningStaticInstance" &&
                        value.GetParameters().Length == 5);
                MethodInfo selectClass = controllerType.GetMethod("SelectClass",
                    BindingFlags.Public | BindingFlags.Instance, null,
                    new[] { typeof(BlueprintCharacterClass), typeof(bool) }, null);
                MethodInfo mechanics = controllerType.GetMethod("ApplyClassMechanics",
                    BindingFlags.Public | BindingFlags.Instance);
                MethodInfo applyLevelup = controllerType.GetMethod("ApplyLevelup",
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                MethodInfo cancel = controllerType.GetMethod("Cancel",
                    BindingFlags.Public | BindingFlags.Instance);
                if (selectClass == null || mechanics == null || applyLevelup == null ||
                    cancel == null)
                    throw new MissingMethodException(
                        "An exact native respec controller method is unavailable.");

                stage = "seed-fighter";
                object charGen = Enum.Parse(start.GetParameters()[4].ParameterType,
                    "CharGen", false);
                seedController = start.Invoke(null,
                    new object[] { descriptor, false, null, null, charGen });
                if (!(bool)selectClass.Invoke(seedController,
                    new object[] { fighter, false }))
                    throw new InvalidOperationException(
                        "Disposable respec Fighter seed selection was rejected.");
                mechanics.Invoke(seedController, null);
                applyLevelup.Invoke(seedController, new object[] { descriptor });
                cancel.Invoke(seedController, null);
                seedController = null;
                fighterSeeded = descriptor.Progression.GetClassLevel(fighter);

                var replacement = new Kingmaker.UI.LevelUp.ChargenUnit(source);
                respecEntity = replacement.Unit;
                respecDescriptor = respecEntity == null ? null : respecEntity.Descriptor;
                if (respecDescriptor == null || respecDescriptor.Progression == null)
                    throw new InvalidOperationException(
                        "Disposable respec replacement descriptor is unavailable.");
                Kingmaker.Items.UnitBody originalBody = respecDescriptor.Body;
                stage = "start-respec-controller";
                object respec = Enum.Parse(start.GetParameters()[4].ParameterType,
                    "Respec", false);
                respecController = start.Invoke(null,
                    new object[] { respecDescriptor, false, null, null, respec });
                Kingmaker.UnitLogic.UnitDescriptor preview =
                    ReadExactMember(respecController, "Preview") as
                        Kingmaker.UnitLogic.UnitDescriptor;
                if (preview == null || preview.Progression == null ||
                    ReferenceEquals(preview, descriptor))
                    throw new InvalidOperationException(
                        "Exact isolated Respec preview is unavailable.");
                bodyPreserved = originalBody != null &&
                    ReferenceEquals(originalBody, respecDescriptor.Body);
                stage = "read-respec-preview";
                previewFighterBefore = preview.Progression.GetClassLevel(fighter);
                previewGunslingerBefore = preview.Progression.GetClassLevel(gunslinger);
                stage = "select-gunslinger";
                selected = (bool)selectClass.Invoke(respecController,
                    new object[] { gunslinger, false });
                mechanics.Invoke(respecController, null);
                queuedCount = SnapshotReferences(
                    ReadExactMember(respecController, "LevelUpActions")).Length;
                previewGunslingerAfter = preview.Progression.GetClassLevel(gunslinger);
                sourceFighterAfter = descriptor.Progression.GetClassLevel(fighter);
                sourceGunslingerAfter = descriptor.Progression.GetClassLevel(gunslinger);
            }
            catch (Exception exception)
            {
                throw new InvalidOperationException(
                    "Disposable respec preview failed at stage " + stage + ".", exception);
            }
            finally
            {
                MethodInfo cancel = typeof(Kingmaker.UnitLogic.Class.LevelUp.LevelUpController)
                    .GetMethod("Cancel", BindingFlags.Public | BindingFlags.Instance);
                if (respecController != null && cancel != null)
                    cancel.Invoke(respecController, null);
                if (seedController != null && cancel != null)
                    cancel.Invoke(seedController, null);
                if (respecEntity != null) respecEntity.Dispose();
                if (entity != null) entity.Dispose();
                cleaned = SameReferences(partyBefore, SnapshotReferences(party)) &&
                    SameReferences(unitsBefore, SnapshotReferences(allUnits)) &&
                    (entity == null || !ContainsReference(party, entity)) &&
                    (entity == null || !ContainsReference(allUnits, entity)) &&
                    (respecEntity == null || !ContainsReference(party, respecEntity)) &&
                    (respecEntity == null || !ContainsReference(allUnits, respecEntity));
            }
            string observed = "fighterSeeded=" + fighterSeeded +
                ";bodyPreserved=" + bodyPreserved +
                ";previewFighterBefore=" + previewFighterBefore +
                ";previewGunslingerBefore=" + previewGunslingerBefore +
                ";selected=" + selected +
                ";previewGunslingerAfter=" + previewGunslingerAfter +
                ";sourceFighterAfter=" + sourceFighterAfter +
                ";sourceGunslingerAfter=" + sourceGunslingerAfter +
                ";queued=" + queuedCount;
            var assertions = new List<RuntimeTestAssertion>
            {
                Assertion("respec-source", "disposable Fighter 1 with intact source body",
                    observed, fighterSeeded == 1 && bodyPreserved,
                    "fresh detached replacement mirrors native Player.RespecCompanion"),
                Assertion("gunslinger-respec-preview",
                    "preview resets Fighter and reaches Gunslinger 1; source remains Fighter 1",
                    observed, selected && previewFighterBefore == 0 &&
                        previewGunslingerBefore == 0 && previewGunslingerAfter == 1 &&
                        sourceFighterAfter == 1 && sourceGunslingerAfter == 0 &&
                        queuedCount == 2,
                    "native Respec mode Gunslinger class selection without Commit"),
                Assertion("external-isolation", "unchanged party and global-unit snapshots",
                    "cleaned=" + cleaned, cleaned,
                    "controllers canceled and both disposable entities disposed"),
                Assertion("loaded-mod-version", _request.ExpectedModVersion,
                    _context.ModEntry.Info.Version,
                    _request.ExpectedModVersion == _context.ModEntry.Info.Version,
                    "Unity Mod Manager ModEntry.Info.Version")
            };
            bool pass = assertions.TrueForAll(value => value.Status == "PASS");
            return CreateResult(pass ? RuntimeTestStatuses.Pass :
                RuntimeTestStatuses.Fail, assertions, null);
        }

        private RuntimeTestResult RunDisposableGunslingerGritResource()
        {
            BlueprintUnit source = BlueprintRoot.Instance.DefaultPlayerCharacter;
            GunslingerClassBlueprintSet gunslingerSet = BlueprintBootstrap.GunslingerClass;
            BlueprintCharacterClass gunslinger = gunslingerSet.CharacterClass;
            BlueprintAbilityResource grit = gunslingerSet.Grit.Resource;
            object player = ReadExactMember(Kingmaker.Game.Instance, "Player");
            object state = ReadExactMember(Kingmaker.Game.Instance, "State");
            object party = ReadExactMember(player, "Party");
            object allUnits = ReadExactMember(state, "AllUnits");
            object[] partyBefore = SnapshotReferences(party);
            object[] unitsBefore = SnapshotReferences(allUnits);
            Kingmaker.EntitySystem.Entities.UnitEntityData entity = null;
            Kingmaker.UnitLogic.UnitDescriptor descriptor = null;
            object firstController = null;
            object secondController = null;
            int maximumAfterGrant = -1;
            int currentAfterGrant = -1;
            int currentAfterSpend = -1;
            int currentAfterLevelUp = -1;
            int currentAfterRestore = -1;
            int gunslingerLevel = -1;
            bool cleaned = false;
            string stage = "construct-disposable";
            try
            {
                var chargen = new Kingmaker.UI.LevelUp.ChargenUnit(source);
                entity = chargen.Unit;
                descriptor = entity == null ? null : entity.Descriptor;
                if (descriptor == null || descriptor.Progression == null ||
                    descriptor.Resources == null)
                    throw new InvalidOperationException(
                        "Disposable grit descriptor or resource collection is unavailable.");
                Type controllerType = typeof(Kingmaker.UnitLogic.Class.LevelUp.LevelUpController);
                MethodInfo start = controllerType.GetMethods(BindingFlags.Public |
                    BindingFlags.NonPublic | BindingFlags.Static).Single(value =>
                        value.Name == "StartWithoutAssigningStaticInstance" &&
                        value.GetParameters().Length == 5);
                MethodInfo selectClass = controllerType.GetMethod("SelectClass",
                    BindingFlags.Public | BindingFlags.Instance, null,
                    new[] { typeof(BlueprintCharacterClass), typeof(bool) }, null);
                MethodInfo mechanics = controllerType.GetMethod("ApplyClassMechanics",
                    BindingFlags.Public | BindingFlags.Instance);
                MethodInfo applyLevelup = controllerType.GetMethod("ApplyLevelup",
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                MethodInfo cancel = controllerType.GetMethod("Cancel",
                    BindingFlags.Public | BindingFlags.Instance);
                if (selectClass == null || mechanics == null || applyLevelup == null ||
                    cancel == null)
                    throw new MissingMethodException(
                        "An exact native grit level-up controller method is unavailable.");

                stage = "grant-level-one-grit";
                object charGen = Enum.Parse(start.GetParameters()[4].ParameterType,
                    "CharGen", false);
                firstController = start.Invoke(null,
                    new object[] { descriptor, false, null, null, charGen });
                if (!(bool)selectClass.Invoke(firstController,
                    new object[] { gunslinger, false }))
                    throw new InvalidOperationException(
                        "Disposable grit Gunslinger selection was rejected.");
                mechanics.Invoke(firstController, null);
                applyLevelup.Invoke(firstController, new object[] { descriptor });
                cancel.Invoke(firstController, null);
                firstController = null;
                maximumAfterGrant = grit.GetMaxAmount(descriptor);
                currentAfterGrant = descriptor.Resources.GetResourceAmount(grit);

                stage = "spend-and-level";
                if (!descriptor.Resources.HasEnoughResource(grit, 1))
                    throw new InvalidOperationException(
                        "Fresh disposable grit resource cannot fund one point.");
                descriptor.Resources.Spend(grit, 1);
                currentAfterSpend = descriptor.Resources.GetResourceAmount(grit);
                object levelUp = Enum.Parse(start.GetParameters()[4].ParameterType,
                    "LevelUp", false);
                secondController = start.Invoke(null,
                    new object[] { descriptor, false, null, null, levelUp });
                if (!(bool)selectClass.Invoke(secondController,
                    new object[] { gunslinger, false }))
                    throw new InvalidOperationException(
                        "Disposable second Gunslinger level selection was rejected.");
                mechanics.Invoke(secondController, null);
                applyLevelup.Invoke(secondController, new object[] { descriptor });
                cancel.Invoke(secondController, null);
                secondController = null;
                gunslingerLevel = descriptor.Progression.GetClassLevel(gunslinger);
                currentAfterLevelUp = descriptor.Resources.GetResourceAmount(grit);

                stage = "restore-grit";
                descriptor.Resources.Restore(grit, 1);
                currentAfterRestore = descriptor.Resources.GetResourceAmount(grit);
            }
            catch (Exception exception)
            {
                throw new InvalidOperationException(
                    "Disposable grit resource failed at stage " + stage + ".", exception);
            }
            finally
            {
                MethodInfo cancel = typeof(Kingmaker.UnitLogic.Class.LevelUp.LevelUpController)
                    .GetMethod("Cancel", BindingFlags.Public | BindingFlags.Instance);
                if (secondController != null && cancel != null)
                    cancel.Invoke(secondController, null);
                if (firstController != null && cancel != null)
                    cancel.Invoke(firstController, null);
                if (entity != null) entity.Dispose();
                cleaned = SameReferences(partyBefore, SnapshotReferences(party)) &&
                    SameReferences(unitsBefore, SnapshotReferences(allUnits)) &&
                    (entity == null || !ContainsReference(party, entity)) &&
                    (entity == null || !ContainsReference(allUnits, entity));
            }
            string observed = "maximumAfterGrant=" + maximumAfterGrant +
                ";currentAfterGrant=" + currentAfterGrant +
                ";currentAfterSpend=" + currentAfterSpend +
                ";gunslingerLevel=" + gunslingerLevel +
                ";currentAfterLevelUp=" + currentAfterLevelUp +
                ";currentAfterRestore=" + currentAfterRestore;
            var assertions = new List<RuntimeTestAssertion>
            {
                Assertion("grit-initial-grant", "maximum=1;current=1",
                    observed, maximumAfterGrant == 1 && currentAfterGrant == 1,
                    "native AddAbilityResources on disposable Gunslinger level one"),
                Assertion("grit-spend-no-level-refill",
                    "spend reaches zero and Gunslinger level two remains zero",
                    observed, currentAfterSpend == 0 && gunslingerLevel == 2 &&
                        currentAfterLevelUp == 0,
                    "native Spend plus RestoreOnLevelUp=false"),
                Assertion("grit-capped-restore", "current=maximum=1",
                    observed, currentAfterRestore == maximumAfterGrant,
                    "native UnitAbilityResourceCollection.Restore"),
                Assertion("external-isolation", "unchanged party and global-unit snapshots",
                    "cleaned=" + cleaned, cleaned,
                    "controllers canceled and disposable entity disposed"),
                Assertion("loaded-mod-version", _request.ExpectedModVersion,
                    _context.ModEntry.Info.Version,
                    _request.ExpectedModVersion == _context.ModEntry.Info.Version,
                    "Unity Mod Manager ModEntry.Info.Version")
            };
            bool pass = assertions.TrueForAll(value => value.Status == "PASS");
            return CreateResult(pass ? RuntimeTestStatuses.Pass :
                RuntimeTestStatuses.Fail, assertions, null);
        }

        private RuntimeTestResult RunDisposableGunslingerGritRest()
        {
            BlueprintUnit source = BlueprintRoot.Instance.DefaultPlayerCharacter;
            GunslingerClassBlueprintSet gunslingerSet = BlueprintBootstrap.GunslingerClass;
            BlueprintCharacterClass gunslinger = gunslingerSet.CharacterClass;
            BlueprintAbilityResource grit = gunslingerSet.Grit.Resource;
            object player = ReadExactMember(Kingmaker.Game.Instance, "Player");
            object state = ReadExactMember(Kingmaker.Game.Instance, "State");
            object party = ReadExactMember(player, "Party");
            object allUnits = ReadExactMember(state, "AllUnits");
            object[] partyBefore = SnapshotReferences(party);
            object[] unitsBefore = SnapshotReferences(allUnits);
            Kingmaker.EntitySystem.Entities.UnitEntityData entity = null;
            Kingmaker.UnitLogic.UnitDescriptor descriptor = null;
            object controller = null;
            int maximum = -1;
            int initial = -1;
            int spent = -1;
            int rested = -1;
            bool cleaned = false;
            string stage = "construct-disposable";
            try
            {
                var chargen = new Kingmaker.UI.LevelUp.ChargenUnit(source);
                entity = chargen.Unit;
                descriptor = entity == null ? null : entity.Descriptor;
                if (descriptor == null || descriptor.Progression == null ||
                    descriptor.Resources == null)
                    throw new InvalidOperationException(
                        "Disposable grit-rest descriptor is unavailable.");
                Type controllerType = typeof(Kingmaker.UnitLogic.Class.LevelUp.LevelUpController);
                MethodInfo start = controllerType.GetMethods(BindingFlags.Public |
                    BindingFlags.NonPublic | BindingFlags.Static).Single(value =>
                        value.Name == "StartWithoutAssigningStaticInstance" &&
                        value.GetParameters().Length == 5);
                MethodInfo selectClass = controllerType.GetMethod("SelectClass",
                    BindingFlags.Public | BindingFlags.Instance, null,
                    new[] { typeof(BlueprintCharacterClass), typeof(bool) }, null);
                MethodInfo mechanics = controllerType.GetMethod("ApplyClassMechanics",
                    BindingFlags.Public | BindingFlags.Instance);
                MethodInfo applyLevelup = controllerType.GetMethod("ApplyLevelup",
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                MethodInfo cancel = controllerType.GetMethod("Cancel",
                    BindingFlags.Public | BindingFlags.Instance);
                if (selectClass == null || mechanics == null || applyLevelup == null ||
                    cancel == null)
                    throw new MissingMethodException(
                        "An exact native grit-rest controller method is unavailable.");

                stage = "grant-and-spend";
                object charGen = Enum.Parse(start.GetParameters()[4].ParameterType,
                    "CharGen", false);
                controller = start.Invoke(null,
                    new object[] { descriptor, false, null, null, charGen });
                if (!(bool)selectClass.Invoke(controller,
                    new object[] { gunslinger, false }))
                    throw new InvalidOperationException(
                        "Disposable grit-rest Gunslinger selection was rejected.");
                mechanics.Invoke(controller, null);
                applyLevelup.Invoke(controller, new object[] { descriptor });
                cancel.Invoke(controller, null);
                controller = null;
                maximum = grit.GetMaxAmount(descriptor);
                initial = descriptor.Resources.GetResourceAmount(grit);
                descriptor.Resources.Spend(grit, 1);
                spent = descriptor.Resources.GetResourceAmount(grit);

                stage = "apply-native-rest";
                Kingmaker.Controllers.Rest.RestController.ApplyRest(descriptor);
                rested = descriptor.Resources.GetResourceAmount(grit);
            }
            catch (Exception exception)
            {
                throw new InvalidOperationException(
                    "Disposable grit rest failed at stage " + stage + ".", exception);
            }
            finally
            {
                MethodInfo cancel = typeof(Kingmaker.UnitLogic.Class.LevelUp.LevelUpController)
                    .GetMethod("Cancel", BindingFlags.Public | BindingFlags.Instance);
                if (controller != null && cancel != null) cancel.Invoke(controller, null);
                if (entity != null) entity.Dispose();
                cleaned = SameReferences(partyBefore, SnapshotReferences(party)) &&
                    SameReferences(unitsBefore, SnapshotReferences(allUnits)) &&
                    (entity == null || !ContainsReference(party, entity)) &&
                    (entity == null || !ContainsReference(allUnits, entity));
            }
            string observed = "maximum=" + maximum + ";initial=" + initial +
                ";spent=" + spent + ";rested=" + rested;
            var assertions = new List<RuntimeTestAssertion>
            {
                Assertion("grit-rest-fixture", "maximum=1;initial=1;spent=0",
                    observed, maximum == 1 && initial == 1 && spent == 0,
                    "native level-one grant and resource spend"),
                Assertion("native-rest-refill", "rested=maximum=1",
                    observed, rested == maximum,
                    "RestController.ApplyRest restores registered unit resources"),
                Assertion("external-isolation", "unchanged party and global-unit snapshots",
                    "cleaned=" + cleaned, cleaned,
                    "controller canceled and disposable entity disposed"),
                Assertion("loaded-mod-version", _request.ExpectedModVersion,
                    _context.ModEntry.Info.Version,
                    _request.ExpectedModVersion == _context.ModEntry.Info.Version,
                    "Unity Mod Manager ModEntry.Info.Version")
            };
            bool pass = assertions.TrueForAll(value => value.Status == "PASS");
            return CreateResult(pass ? RuntimeTestStatuses.Pass :
                RuntimeTestStatuses.Fail, assertions, null);
        }

        private RuntimeTestResult RunDisposableGunslingerGritPersistence()
        {
            BlueprintUnit source = BlueprintRoot.Instance.DefaultPlayerCharacter;
            GunslingerClassBlueprintSet gunslingerSet = BlueprintBootstrap.GunslingerClass;
            BlueprintCharacterClass gunslinger = gunslingerSet.CharacterClass;
            BlueprintAbilityResource grit = gunslingerSet.Grit.Resource;
            object player = ReadExactMember(Kingmaker.Game.Instance, "Player");
            object state = ReadExactMember(Kingmaker.Game.Instance, "State");
            object party = ReadExactMember(player, "Party");
            object allUnits = ReadExactMember(state, "AllUnits");
            object[] partyBefore = SnapshotReferences(party);
            object[] unitsBefore = SnapshotReferences(allUnits);
            Kingmaker.EntitySystem.Entities.UnitEntityData originalEntity = null;
            Kingmaker.EntitySystem.Entities.UnitEntityData replacementEntity = null;
            object controller = null;
            int maximum = -1;
            int originalCurrent = -1;
            int currentAfterLaterReapply = -1;
            int replacementCurrent = -1;
            int serializedRecordCount = -1;
            bool distinctRecord = false;
            bool exactBlueprint = false;
            bool cleaned = false;
            string json = string.Empty;
            string stage = "construct-disposables";
            try
            {
                var original = new Kingmaker.UI.LevelUp.ChargenUnit(source);
                var replacement = new Kingmaker.UI.LevelUp.ChargenUnit(source);
                originalEntity = original.Unit;
                replacementEntity = replacement.Unit;
                Kingmaker.UnitLogic.UnitDescriptor originalDescriptor =
                    originalEntity == null ? null : originalEntity.Descriptor;
                Kingmaker.UnitLogic.UnitDescriptor replacementDescriptor =
                    replacementEntity == null ? null : replacementEntity.Descriptor;
                if (originalDescriptor == null || replacementDescriptor == null ||
                    originalDescriptor.Resources == null || replacementDescriptor.Resources == null)
                    throw new InvalidOperationException(
                        "Disposable grit persistence descriptors are unavailable.");
                originalDescriptor.Stats.Wisdom.BaseValue = 14;
                Type controllerType = typeof(Kingmaker.UnitLogic.Class.LevelUp.LevelUpController);
                MethodInfo start = controllerType.GetMethods(BindingFlags.Public |
                    BindingFlags.NonPublic | BindingFlags.Static).Single(value =>
                        value.Name == "StartWithoutAssigningStaticInstance" &&
                        value.GetParameters().Length == 5);
                MethodInfo selectClass = controllerType.GetMethod("SelectClass",
                    BindingFlags.Public | BindingFlags.Instance, null,
                    new[] { typeof(BlueprintCharacterClass), typeof(bool) }, null);
                MethodInfo mechanics = controllerType.GetMethod("ApplyClassMechanics",
                    BindingFlags.Public | BindingFlags.Instance);
                MethodInfo applyLevelup = controllerType.GetMethod("ApplyLevelup",
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                MethodInfo cancel = controllerType.GetMethod("Cancel",
                    BindingFlags.Public | BindingFlags.Instance);
                if (selectClass == null || mechanics == null || applyLevelup == null ||
                    cancel == null)
                    throw new MissingMethodException(
                        "An exact native grit persistence controller method is unavailable.");

                stage = "grant-and-spend";
                object charGen = Enum.Parse(start.GetParameters()[4].ParameterType,
                    "CharGen", false);
                controller = start.Invoke(null,
                    new object[] { originalDescriptor, false, null, null, charGen });
                if (!(bool)selectClass.Invoke(controller,
                    new object[] { gunslinger, false }))
                    throw new InvalidOperationException(
                        "Disposable grit persistence Gunslinger selection was rejected.");
                mechanics.Invoke(controller, null);
                applyLevelup.Invoke(controller, new object[] { originalDescriptor });
                cancel.Invoke(controller, null);
                controller = null;
                maximum = grit.GetMaxAmount(originalDescriptor);
                originalDescriptor.Resources.Spend(grit, 1);
                originalCurrent = originalDescriptor.Resources.GetResourceAmount(grit);
                originalDescriptor.Progression.ReapplyFeaturesOnLevelUp();
                currentAfterLaterReapply =
                    originalDescriptor.Resources.GetResourceAmount(grit);

                stage = "native-json-roundtrip";
                Kingmaker.UnitLogic.UnitAbilityResource record =
                    originalDescriptor.Resources.PersistantResources.Single(value =>
                        value != null && ReferenceEquals(value.Blueprint, grit));
                json = JsonConvert.SerializeObject(record, Formatting.None,
                    Kingmaker.EntitySystem.Persistence.JsonUtility.DefaultJsonSettings.DefaultSettings);
                Kingmaker.UnitLogic.UnitAbilityResource clone =
                    JsonConvert.DeserializeObject<Kingmaker.UnitLogic.UnitAbilityResource>(json,
                        Kingmaker.EntitySystem.Persistence.JsonUtility.DefaultJsonSettings.DefaultSettings);
                if (clone == null)
                    throw new InvalidOperationException(
                        "Native grit persistence JSON round trip returned null.");
                distinctRecord = !ReferenceEquals(record, clone);
                exactBlueprint = ReferenceEquals(clone.Blueprint, grit);
                replacementDescriptor.Resources.PersistantResources =
                    new List<Kingmaker.UnitLogic.UnitAbilityResource> { clone };
                serializedRecordCount = replacementDescriptor.Resources.PersistantResources.Count;
                replacementCurrent = replacementDescriptor.Resources.GetResourceAmount(grit);
            }
            catch (Exception exception)
            {
                throw new InvalidOperationException(
                    "Disposable grit persistence failed at stage " + stage + ".", exception);
            }
            finally
            {
                MethodInfo cancel = typeof(Kingmaker.UnitLogic.Class.LevelUp.LevelUpController)
                    .GetMethod("Cancel", BindingFlags.Public | BindingFlags.Instance);
                if (controller != null && cancel != null) cancel.Invoke(controller, null);
                if (replacementEntity != null) replacementEntity.Dispose();
                if (originalEntity != null) originalEntity.Dispose();
                cleaned = SameReferences(partyBefore, SnapshotReferences(party)) &&
                    SameReferences(unitsBefore, SnapshotReferences(allUnits)) &&
                    (originalEntity == null || !ContainsReference(allUnits, originalEntity)) &&
                    (replacementEntity == null || !ContainsReference(allUnits, replacementEntity));
            }
            string observed = "maximum=" + maximum + ";originalCurrent=" +
                originalCurrent + ";replacementCurrent=" + replacementCurrent +
                ";afterLaterReapply=" + currentAfterLaterReapply +
                ";records=" + serializedRecordCount + ";jsonLength=" + json.Length;
            var assertions = new List<RuntimeTestAssertion>
            {
                Assertion("nontrivial-grit-fixture", "maximum=2;current=1",
                    observed, maximum == 2 && originalCurrent == 1,
                    "detached Wisdom 14 Gunslinger plus native Spend"),
                Assertion("native-json-roundtrip", "distinct record; exact grit blueprint",
                    observed, distinctRecord && exactBlueprint && json.Length > 0,
                    "DefaultJsonSettings and UnitAbilityResource JsonConstructor"),
                Assertion("later-level-no-refill", "current remains 1",
                    observed, currentAfterLaterReapply == 1,
                    "persistent initialization marker across exact feature reapply"),
                Assertion("persistent-collection-reconstruction", "one grit record at current=1",
                    observed, serializedRecordCount == 1 && replacementCurrent == 1,
                    "UnitAbilityResourceCollection.PersistantResources setter"),
                Assertion("external-isolation", "unchanged party and global-unit snapshots",
                    "cleaned=" + cleaned, cleaned,
                    "controller canceled and both disposable entities disposed"),
                Assertion("loaded-mod-version", _request.ExpectedModVersion,
                    _context.ModEntry.Info.Version,
                    _request.ExpectedModVersion == _context.ModEntry.Info.Version,
                    "Unity Mod Manager ModEntry.Info.Version")
            };
            bool pass = assertions.TrueForAll(value => value.Status == "PASS");
            return CreateResult(pass ? RuntimeTestStatuses.Pass :
                RuntimeTestStatuses.Fail, assertions, null);
        }

        private RuntimeTestResult RunDisposableGunslingerNimble()
        {
            BlueprintUnit source = BlueprintRoot.Instance.DefaultPlayerCharacter;
            GunslingerClassBlueprintSet gunslinger = BlueprintBootstrap.GunslingerClass;
            BlueprintItemArmor lightArmor = BlueprintBootstrap.Library.GetAllBlueprints()
                .OfType<BlueprintItemArmor>().Where(value => value.Type != null &&
                    value.Type.IsArmor && value.Type.ProficiencyGroup ==
                    ArmorProficiencyGroup.Light)
                .OrderBy(value => value.AssetGuid, StringComparer.Ordinal).FirstOrDefault();
            BlueprintItemArmor mediumArmor = BlueprintBootstrap.Library.GetAllBlueprints()
                .OfType<BlueprintItemArmor>().Where(value => value.Type != null &&
                    value.Type.IsArmor && value.Type.ProficiencyGroup ==
                    ArmorProficiencyGroup.Medium)
                .OrderBy(value => value.AssetGuid, StringComparer.Ordinal).FirstOrDefault();
            object player = ReadExactMember(Kingmaker.Game.Instance, "Player");
            object state = ReadExactMember(Kingmaker.Game.Instance, "State");
            object party = ReadExactMember(player, "Party");
            object allUnits = ReadExactMember(state, "AllUnits");
            object[] partyBefore = SnapshotReferences(party);
            object[] unitsBefore = SnapshotReferences(allUnits);
            Kingmaker.EntitySystem.Entities.UnitEntityData unit = null;
            int baseAc = -1, noArmorAc = -1, baseFlat = -1, nimbleFlat = -1,
                lightWith = -1, lightWithout = -1, mediumWith = -1,
                mediumWithout = -1;
            bool cleaned = false; string stage = "construct-disposable";
            try
            {
                if (lightArmor == null || mediumArmor == null)
                    throw new InvalidOperationException("Native light/medium armor fixtures are unavailable.");
                unit = new Kingmaker.UI.LevelUp.ChargenUnit(source).Unit;
                baseAc = unit.Descriptor.Stats.AC.ModifiedValue;
                baseFlat = unit.Descriptor.Stats.AC.FlatFooted;

                stage = "no-armor-five-ranks";
                foreach (BlueprintFeature feature in gunslinger.Nimble.Features)
                    unit.Descriptor.AddFact(feature);
                noArmorAc = unit.Descriptor.Stats.AC.ModifiedValue;
                nimbleFlat = unit.Descriptor.Stats.AC.FlatFooted;

                stage = "light-armor";
                unit.Body.Armor.InsertItem(new ItemEntityArmor(lightArmor));
                lightWith = unit.Descriptor.Stats.AC.ModifiedValue;
                foreach (BlueprintFeature feature in gunslinger.Nimble.Features)
                    unit.Descriptor.RemoveFact(feature);
                lightWithout = unit.Descriptor.Stats.AC.ModifiedValue;
                foreach (BlueprintFeature feature in gunslinger.Nimble.Features)
                    unit.Descriptor.AddFact(feature);

                stage = "medium-armor";
                unit.Body.Armor.RemoveItem(false);
                unit.Body.Armor.InsertItem(new ItemEntityArmor(mediumArmor));
                mediumWith = unit.Descriptor.Stats.AC.ModifiedValue;
                foreach (BlueprintFeature feature in gunslinger.Nimble.Features)
                    unit.Descriptor.RemoveFact(feature);
                mediumWithout = unit.Descriptor.Stats.AC.ModifiedValue;
            }
            catch (Exception exception)
            {
                throw new InvalidOperationException(
                    "Disposable Nimble failed at stage " + stage + ".", exception);
            }
            finally
            {
                if (unit != null)
                {
                    foreach (BlueprintFeature feature in gunslinger.Nimble.Features)
                        if (unit.Descriptor.HasFact(feature)) unit.Descriptor.RemoveFact(feature);
                    if (unit.Body != null && unit.Body.Armor.HasArmor)
                        unit.Body.Armor.RemoveItem(false);
                    unit.Dispose();
                }
                cleaned = SameReferences(partyBefore, SnapshotReferences(party)) &&
                    SameReferences(unitsBefore, SnapshotReferences(allUnits)) &&
                    (unit == null || !ContainsReference(allUnits, unit));
            }
            string observed = "base=" + baseAc + ";noArmor=" + noArmorAc +
                ";baseFlat=" + baseFlat + ";nimbleFlat=" + nimbleFlat +
                ";lightWith=" + lightWith + ";lightWithout=" + lightWithout +
                ";mediumWith=" + mediumWith + ";mediumWithout=" + mediumWithout;
            var assertions = new List<RuntimeTestAssertion>
            {
                Assertion("nimble-no-armor", "+5 ordinary AC", observed,
                    noArmorAc == baseAc + 5, "five cumulative native Dodge modifiers"),
                Assertion("nimble-flat-footed", "no Nimble bonus", observed,
                    nimbleFlat == baseFlat, "ModifiableValueArmorClass.FlatFooted"),
                Assertion("nimble-light-armor", "+5 ordinary AC", observed,
                    lightWith == lightWithout + 5, "exact native light armor slot"),
                Assertion("nimble-medium-armor", "+0 ordinary AC", observed,
                    mediumWith == mediumWithout, "exact native medium armor slot"),
                Assertion("external-isolation", "unchanged party and global-unit snapshots",
                    "cleaned=" + cleaned, cleaned, "armor/facts removed and detached unit disposed"),
                Assertion("loaded-mod-version", _request.ExpectedModVersion,
                    _context.ModEntry.Info.Version,
                    _request.ExpectedModVersion == _context.ModEntry.Info.Version,
                    "Unity Mod Manager ModEntry.Info.Version")
            };
            return CreateResult(assertions.TrueForAll(value => value.Status == "PASS")
                ? RuntimeTestStatuses.Pass : RuntimeTestStatuses.Fail, assertions, null);
        }

        private RuntimeTestResult RunDisposableGunslingerInitiative()
        {
            BlueprintUnit source = BlueprintRoot.Instance.DefaultPlayerCharacter;
            GunslingerClassBlueprintSet gunslinger = BlueprintBootstrap.GunslingerClass;
            object player = ReadExactMember(Kingmaker.Game.Instance, "Player");
            object state = ReadExactMember(Kingmaker.Game.Instance, "State");
            object party = ReadExactMember(player, "Party");
            object allUnits = ReadExactMember(state, "AllUnits");
            object[] partyBefore = SnapshotReferences(party);
            object[] unitsBefore = SnapshotReferences(allUnits);
            Kingmaker.EntitySystem.Entities.UnitEntityData unit = null;
            int initialGrit = -1, afterPositiveGrit = -1,
                withBefore = -1, withAfter = -1,
                withDuplicate = -1, emptyGrit = -1, emptyBefore = -1,
                emptyAfter = -1;
            bool cleaned = false; string stage = "construct-disposable";
            GunslingerInitiativeRuntimeDiagnostics.Reset();
            try
            {
                unit = new Kingmaker.UI.LevelUp.ChargenUnit(source).Unit;
                unit.Descriptor.Stats.Wisdom.BaseValue = 14;
                unit.Descriptor.AddFact(gunslinger.Grit.Feature);
                unit.Descriptor.AddFact(gunslinger.Initiative);
                initialGrit = unit.Descriptor.Resources.GetResourceAmount(
                    gunslinger.Grit.Resource);

                stage = "positive-grit-roll";
                var withGrit = new RuleInitiativeRoll(unit);
                Rulebook.Trigger(withGrit);
                withBefore = withGrit.Modifier;
                EventBus.RaiseEvent<IUnitInitiativeHandler>(handler =>
                    handler.HandleUnitRollsInitiative(withGrit));
                withAfter = withGrit.Modifier;
                EventBus.RaiseEvent<IUnitInitiativeHandler>(handler =>
                    handler.HandleUnitRollsInitiative(withGrit));
                withDuplicate = withGrit.Modifier;
                afterPositiveGrit = unit.Descriptor.Resources.GetResourceAmount(
                    gunslinger.Grit.Resource);

                stage = "zero-grit-roll";
                unit.Descriptor.Resources.Spend(gunslinger.Grit.Resource, initialGrit);
                emptyGrit = unit.Descriptor.Resources.GetResourceAmount(
                    gunslinger.Grit.Resource);
                var withoutGrit = new RuleInitiativeRoll(unit);
                Rulebook.Trigger(withoutGrit);
                emptyBefore = withoutGrit.Modifier;
                EventBus.RaiseEvent<IUnitInitiativeHandler>(handler =>
                    handler.HandleUnitRollsInitiative(withoutGrit));
                emptyAfter = withoutGrit.Modifier;
            }
            catch (Exception exception)
            {
                throw new InvalidOperationException(
                    "Disposable Gunslinger Initiative failed at stage " + stage + ".",
                    exception);
            }
            finally
            {
                if (unit != null)
                {
                    if (unit.Descriptor.HasFact(gunslinger.Initiative))
                        unit.Descriptor.RemoveFact(gunslinger.Initiative);
                    if (unit.Descriptor.HasFact(gunslinger.Grit.Feature))
                        unit.Descriptor.RemoveFact(gunslinger.Grit.Feature);
                    unit.Dispose();
                }
                cleaned = SameReferences(partyBefore, SnapshotReferences(party)) &&
                    SameReferences(unitsBefore, SnapshotReferences(allUnits)) &&
                    (unit == null || !ContainsReference(allUnits, unit));
            }
            string observed = "initialGrit=" + initialGrit + ";afterPositiveGrit=" +
                afterPositiveGrit + ";withBefore=" +
                withBefore + ";withAfter=" + withAfter + ";withDuplicate=" +
                withDuplicate + ";emptyGrit=" + emptyGrit + ";emptyBefore=" +
                emptyBefore + ";emptyAfter=" + emptyAfter + ";applied=" +
                GunslingerInitiativeRuntimeDiagnostics.Applied + ";rejected=" +
                GunslingerInitiativeRuntimeDiagnostics.Rejected + ";duplicates=" +
                GunslingerInitiativeRuntimeDiagnostics.Duplicates + ";faults=" +
                GunslingerInitiativeRuntimeDiagnostics.Faults;
            var assertions = new List<RuntimeTestAssertion>
            {
                Assertion("initiative-positive-grit", "+2 native modifier; no spend",
                    observed, initialGrit == 2 && afterPositiveGrit == 2 &&
                    withAfter == withBefore + 2,
                    "exact RuleInitiativeRoll handler boundary"),
                Assertion("initiative-duplicate-stability", "same modifier after replay",
                    observed, withDuplicate == withAfter,
                    "weak rule-identity duplicate guard"),
                Assertion("initiative-zero-grit", "+0 native modifier",
                    observed, emptyGrit == 0 && emptyAfter == emptyBefore,
                    "native Gunslinger grit resource gate"),
                Assertion("initiative-diagnostics",
                    "applied=1;rejected=1;duplicates=1;faults=0", observed,
                    GunslingerInitiativeRuntimeDiagnostics.Applied == 1 &&
                    GunslingerInitiativeRuntimeDiagnostics.Rejected == 1 &&
                    GunslingerInitiativeRuntimeDiagnostics.Duplicates == 1 &&
                    GunslingerInitiativeRuntimeDiagnostics.Faults == 0,
                    "production initiative diagnostics"),
                Assertion("external-isolation", "unchanged party and global-unit snapshots",
                    "cleaned=" + cleaned, cleaned,
                    "facts removed and detached unit disposed"),
                Assertion("loaded-mod-version", _request.ExpectedModVersion,
                    _context.ModEntry.Info.Version,
                    _request.ExpectedModVersion == _context.ModEntry.Info.Version,
                    "Unity Mod Manager ModEntry.Info.Version")
            };
            return CreateResult(assertions.TrueForAll(value => value.Status == "PASS")
                ? RuntimeTestStatuses.Pass : RuntimeTestStatuses.Fail, assertions, null);
        }

        private RuntimeTestResult RunDisposableGunslingerQuickClear()
        {
            BlueprintUnit source = BlueprintRoot.Instance.DefaultPlayerCharacter;
            GunslingerClassBlueprintSet gunslinger = BlueprintBootstrap.GunslingerClass;
            BlueprintAbilityResource grit = gunslinger.Grit.Resource;
            BlueprintItemWeapon pistol = BlueprintBootstrap.ProductionFirearms.Pistol.Item;
            object player = ReadExactMember(Kingmaker.Game.Instance, "Player");
            object state = ReadExactMember(Kingmaker.Game.Instance, "State");
            object party = ReadExactMember(player, "Party");
            object allUnits = ReadExactMember(state, "AllUnits");
            object[] partyBefore = SnapshotReferences(party);
            object[] unitsBefore = SnapshotReferences(allUnits);
            Kingmaker.EntitySystem.Entities.UnitEntityData unit = null;
            ItemEntityWeapon weapon = null; object controller = null;
            int initial = -1, afterStandard = -1, afterMove = -1,
                afterRejected = -1;
            FirearmCondition standardCondition = FirearmCondition.Wrecked,
                moveCondition = FirearmCondition.Wrecked,
                rejectedCondition = FirearmCondition.Wrecked;
            bool cleaned = false; string stage = "construct-disposable";
            QuickClearRuntimeDiagnostics.Reset();
            try
            {
                unit = new Kingmaker.UI.LevelUp.ChargenUnit(source).Unit;
                unit.Descriptor.Stats.Wisdom.BaseValue = 14;
                Type controllerType = typeof(Kingmaker.UnitLogic.Class.LevelUp.LevelUpController);
                MethodInfo start = controllerType.GetMethods(BindingFlags.Public |
                    BindingFlags.NonPublic | BindingFlags.Static).Single(value =>
                        value.Name == "StartWithoutAssigningStaticInstance" &&
                        value.GetParameters().Length == 5);
                MethodInfo selectClass = controllerType.GetMethod("SelectClass",
                    BindingFlags.Public | BindingFlags.Instance, null,
                    new[] { typeof(BlueprintCharacterClass), typeof(bool) }, null);
                MethodInfo mechanics = controllerType.GetMethod("ApplyClassMechanics",
                    BindingFlags.Public | BindingFlags.Instance);
                MethodInfo applyLevelup = controllerType.GetMethod("ApplyLevelup",
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                MethodInfo cancel = controllerType.GetMethod("Cancel",
                    BindingFlags.Public | BindingFlags.Instance);
                object charGen = Enum.Parse(start.GetParameters()[4].ParameterType,
                    "CharGen", false);
                controller = start.Invoke(null,
                    new object[] { unit.Descriptor, false, null, null, charGen });
                if (!(bool)selectClass.Invoke(controller,
                    new object[] { gunslinger.CharacterClass, false }))
                    throw new InvalidOperationException("Quick Clear Gunslinger selection failed.");
                mechanics.Invoke(controller, null);
                applyLevelup.Invoke(controller, new object[] { unit.Descriptor });
                cancel.Invoke(controller, null); controller = null;

                stage = "equip-exact-firearm";
                weapon = new ItemEntityWeapon(pistol);
                unit.Body.PrimaryHand.InsertItem(weapon);
                if (!ReferenceEquals(unit.Body.PrimaryHand.MaybeWeapon, weapon))
                    throw new InvalidOperationException("Exact pistol was not equipped.");
                initial = unit.Descriptor.Resources.GetResourceAmount(grit);

                stage = "standard-action";
                FirearmRuntimeState.Service.Set(weapon, FirearmStateMachine.ApplyMisfireDamage(
                    FirearmState.CreateEmpty()));
                QuickClearRuntime.Execute(unit.Descriptor, QuickClearMode.Standard);
                afterStandard = unit.Descriptor.Resources.GetResourceAmount(grit);
                standardCondition = FirearmRuntimeState.Service.GetOrCreate(weapon)
                    .Repository.State.Condition;

                stage = "move-action";
                FirearmRuntimeState.Service.Set(weapon, FirearmStateMachine.ApplyMisfireDamage(
                    FirearmState.CreateEmpty()));
                QuickClearRuntime.Execute(unit.Descriptor, QuickClearMode.Move);
                afterMove = unit.Descriptor.Resources.GetResourceAmount(grit);
                moveCondition = FirearmRuntimeState.Service.GetOrCreate(weapon)
                    .Repository.State.Condition;

                stage = "insufficient-rejection";
                FirearmRuntimeState.Service.Set(weapon, FirearmStateMachine.ApplyMisfireDamage(
                    FirearmState.CreateEmpty()));
                unit.Descriptor.Resources.Spend(grit, afterMove);
                QuickClearRuntime.Execute(unit.Descriptor, QuickClearMode.Standard);
                afterRejected = unit.Descriptor.Resources.GetResourceAmount(grit);
                rejectedCondition = FirearmRuntimeState.Service.GetOrCreate(weapon)
                    .Repository.State.Condition;
            }
            catch (Exception exception)
            {
                throw new InvalidOperationException(
                    "Disposable Quick Clear failed at stage " + stage + ".", exception);
            }
            finally
            {
                MethodInfo cancel = typeof(Kingmaker.UnitLogic.Class.LevelUp.LevelUpController)
                    .GetMethod("Cancel", BindingFlags.Public | BindingFlags.Instance);
                if (controller != null && cancel != null) cancel.Invoke(controller, null);
                if (weapon != null)
                {
                    FirearmRuntimeState.Service.Forget(weapon);
                    if (unit != null && unit.Body.PrimaryHand.MaybeItem != null)
                        unit.Body.PrimaryHand.RemoveItem(false);
                }
                if (unit != null) unit.Dispose();
                cleaned = SameReferences(partyBefore, SnapshotReferences(party)) &&
                    SameReferences(unitsBefore, SnapshotReferences(allUnits)) &&
                    (unit == null || !ContainsReference(allUnits, unit));
            }
            string observed = "initial=" + initial + ";afterStandard=" + afterStandard +
                ";standard=" + standardCondition + ";afterMove=" + afterMove +
                ";move=" + moveCondition + ";afterRejected=" + afterRejected +
                ";rejected=" + rejectedCondition + ";applied=" +
                QuickClearRuntimeDiagnostics.Applied + ";rejectedCount=" +
                QuickClearRuntimeDiagnostics.Rejected + ";faults=" +
                QuickClearRuntimeDiagnostics.Faults;
            var assertions = new List<RuntimeTestAssertion>
            {
                Assertion("quick-clear-standard", "grit 2 unchanged; Broken -> Normal",
                    observed, initial == 2 && afterStandard == 2 &&
                    standardCondition == FirearmCondition.Normal,
                    "production exact-equipped state transition"),
                Assertion("quick-clear-move", "grit 2 -> 1; Broken -> Normal",
                    observed, afterMove == 1 && moveCondition == FirearmCondition.Normal,
                    "production exact-equipped state and native grit transition"),
                Assertion("quick-clear-insufficient-atomic", "grit 0; remains Broken",
                    observed, afterRejected == 0 && rejectedCondition == FirearmCondition.Broken,
                    "production fail-closed policy"),
                Assertion("quick-clear-diagnostics", "applied=2;rejected=1;faults=0",
                    observed, QuickClearRuntimeDiagnostics.Applied == 2 &&
                    QuickClearRuntimeDiagnostics.Rejected == 1 &&
                    QuickClearRuntimeDiagnostics.Faults == 0,
                    "production deed diagnostics"),
                Assertion("external-isolation", "unchanged party and global-unit snapshots",
                    "cleaned=" + cleaned, cleaned, "firearm forgotten and detached unit disposed"),
                Assertion("loaded-mod-version", _request.ExpectedModVersion,
                    _context.ModEntry.Info.Version,
                    _request.ExpectedModVersion == _context.ModEntry.Info.Version,
                    "Unity Mod Manager ModEntry.Info.Version")
            };
            return CreateResult(assertions.TrueForAll(value => value.Status == "PASS")
                ? RuntimeTestStatuses.Pass : RuntimeTestStatuses.Fail, assertions, null);
        }

        private RuntimeTestResult RunDisposableGunslingerDodge()
        {
            BlueprintUnit source = BlueprintRoot.Instance.DefaultPlayerCharacter;
            GunslingerClassBlueprintSet gunslinger = BlueprintBootstrap.GunslingerClass;
            BlueprintAbilityResource grit = gunslinger.Grit.Resource;
            BlueprintItemWeapon pistol = BlueprintBootstrap.ProductionFirearms.Pistol.Item;
            BlueprintItemArmor lightArmor = BlueprintBootstrap.Library.GetAllBlueprints()
                .OfType<BlueprintItemArmor>()
                .Where(value => value.Type != null && value.Type.IsArmor &&
                    value.Type.ProficiencyGroup == ArmorProficiencyGroup.Light)
                .OrderBy(value => value.AssetGuid, StringComparer.Ordinal).FirstOrDefault();
            object player = ReadExactMember(Kingmaker.Game.Instance, "Player");
            object state = ReadExactMember(Kingmaker.Game.Instance, "State");
            object party = ReadExactMember(player, "Party");
            object allUnits = ReadExactMember(state, "AllUnits");
            object[] partyBefore = SnapshotReferences(party);
            object[] unitsBefore = SnapshotReferences(allUnits);
            Kingmaker.EntitySystem.Entities.UnitEntityData attacker = null;
            Kingmaker.EntitySystem.Entities.UnitEntityData defender = null;
            object controller = null;
            int initial = -1, afterApplied = -1, acAfter = -1,
                acDuplicate = -1, afterRejected = -1, rejectedAc = -1;
            bool armedBefore = false, armedAfter = true, proneAfter = false,
                rejectedProne = true, rejectedConsumed = false, cleaned = false;
            string stage = "construct-disposables";
            try
            {
                if (lightArmor == null) throw new InvalidOperationException(
                    "No exact native light armor blueprint was available.");
                attacker = new Kingmaker.UI.LevelUp.ChargenUnit(source).Unit;
                defender = new Kingmaker.UI.LevelUp.ChargenUnit(source).Unit;
                defender.Descriptor.Stats.Wisdom.BaseValue = 14;
                Type controllerType = typeof(Kingmaker.UnitLogic.Class.LevelUp.LevelUpController);
                MethodInfo start = controllerType.GetMethods(BindingFlags.Public |
                    BindingFlags.NonPublic | BindingFlags.Static).Single(value =>
                        value.Name == "StartWithoutAssigningStaticInstance" &&
                        value.GetParameters().Length == 5);
                MethodInfo selectClass = controllerType.GetMethod("SelectClass",
                    BindingFlags.Public | BindingFlags.Instance, null,
                    new[] { typeof(BlueprintCharacterClass), typeof(bool) }, null);
                MethodInfo mechanics = controllerType.GetMethod("ApplyClassMechanics",
                    BindingFlags.Public | BindingFlags.Instance);
                MethodInfo applyLevelup = controllerType.GetMethod("ApplyLevelup",
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                MethodInfo cancel = controllerType.GetMethod("Cancel",
                    BindingFlags.Public | BindingFlags.Instance);
                object charGen = Enum.Parse(start.GetParameters()[4].ParameterType,
                    "CharGen", false);
                controller = start.Invoke(null,
                    new object[] { defender.Descriptor, false, null, null, charGen });
                if (!(bool)selectClass.Invoke(controller,
                    new object[] { gunslinger.CharacterClass, false }))
                    throw new InvalidOperationException("Dodge Gunslinger selection failed.");
                mechanics.Invoke(controller, null);
                applyLevelup.Invoke(controller, new object[] { defender.Descriptor });
                cancel.Invoke(controller, null); controller = null;

                stage = "equip-light-armor";
                defender.Body.Armor.InsertItem(new ItemEntityArmor(lightArmor));
                if (!defender.Body.Armor.HasArmor ||
                    defender.Body.Armor.Armor.Blueprint.Type.ProficiencyGroup !=
                        ArmorProficiencyGroup.Light ||
                    defender.Descriptor.Encumbrance != Encumbrance.Light)
                    throw new InvalidOperationException(
                        "Detached light-armor/light-load contract was not observable.");

                stage = "apply-prone-reaction";
                GunslingerDodgeRuntimeDiagnostics.Reset();
                initial = defender.Descriptor.Resources.GetResourceAmount(grit);
                defender.Descriptor.AddFact(gunslinger.Dodge.ArmedProneMarker);
                armedBefore = defender.Descriptor.HasFact(
                    gunslinger.Dodge.ArmedProneMarker);
                var roll = new RuleAttackRoll(attacker, defender,
                    new ItemEntityWeapon(pistol), 0);
                GunslingerDodgeRuntime.BeforeAttackRoll(roll);
                afterApplied = defender.Descriptor.Resources.GetResourceAmount(grit);
                armedAfter = defender.Descriptor.HasFact(
                    gunslinger.Dodge.ArmedProneMarker);
                proneAfter = defender.Descriptor.State.HasCondition(UnitCondition.Prone);
                var calculate = new RuleCalculateAC(attacker, defender, AttackType.Ranged);
                SetExactProperty(calculate, "TargetAC", 20);
                GunslingerDodgeRuntime.AfterCalculateArmorClass(calculate);
                acAfter = calculate.TargetAC;
                GunslingerDodgeRuntime.AfterCalculateArmorClass(calculate);
                acDuplicate = calculate.TargetAC;
                GunslingerDodgeRuntime.AfterAttackRoll(roll);

                stage = "insufficient-rejection";
                defender.Descriptor.State.RemoveCondition(UnitCondition.Prone);
                defender.Descriptor.Resources.Spend(grit, afterApplied);
                defender.Descriptor.AddFact(gunslinger.Dodge.ArmedProneMarker);
                var rejected = new RuleAttackRoll(attacker, defender,
                    new ItemEntityWeapon(pistol), 0);
                GunslingerDodgeRuntime.BeforeAttackRoll(rejected);
                afterRejected = defender.Descriptor.Resources.GetResourceAmount(grit);
                rejectedProne = defender.Descriptor.State.HasCondition(UnitCondition.Prone);
                rejectedConsumed = !defender.Descriptor.HasFact(
                    gunslinger.Dodge.ArmedProneMarker);
                var rejectedCalculate = new RuleCalculateAC(attacker, defender,
                    AttackType.Ranged);
                SetExactProperty(rejectedCalculate, "TargetAC", 20);
                GunslingerDodgeRuntime.AfterCalculateArmorClass(rejectedCalculate);
                rejectedAc = rejectedCalculate.TargetAC;
                GunslingerDodgeRuntime.AfterAttackRoll(rejected);
            }
            catch (Exception exception)
            {
                throw new InvalidOperationException(
                    "Disposable Gunslinger's Dodge failed at stage " + stage + ".",
                    exception);
            }
            finally
            {
                MethodInfo cancel = typeof(Kingmaker.UnitLogic.Class.LevelUp.LevelUpController)
                    .GetMethod("Cancel", BindingFlags.Public | BindingFlags.Instance);
                if (controller != null && cancel != null) cancel.Invoke(controller, null);
                if (defender != null && defender.Body != null &&
                    defender.Body.Armor != null && defender.Body.Armor.HasArmor)
                    defender.Body.Armor.RemoveItem(false);
                if (defender != null) defender.Dispose();
                if (attacker != null) attacker.Dispose();
                cleaned = SameReferences(partyBefore, SnapshotReferences(party)) &&
                    SameReferences(unitsBefore, SnapshotReferences(allUnits)) &&
                    (attacker == null || !ContainsReference(allUnits, attacker)) &&
                    (defender == null || !ContainsReference(allUnits, defender));
            }
            string observed = "initial=" + initial + ";armedBefore=" + armedBefore +
                ";afterApplied=" + afterApplied + ";armedAfter=" + armedAfter +
                ";proneAfter=" + proneAfter + ";acAfter=" + acAfter +
                ";acDuplicate=" + acDuplicate + ";afterRejected=" + afterRejected +
                ";rejectedProne=" + rejectedProne + ";rejectedConsumed=" +
                rejectedConsumed + ";rejectedAc=" + rejectedAc + ";applied=" +
                GunslingerDodgeRuntimeDiagnostics.Applied + ";rejected=" +
                GunslingerDodgeRuntimeDiagnostics.Rejected + ";duplicates=" +
                GunslingerDodgeRuntimeDiagnostics.Duplicates + ";faults=" +
                GunslingerDodgeRuntimeDiagnostics.Faults;
            var assertions = new List<RuntimeTestAssertion>
            {
                Assertion("dodge-native-reaction", "armed; grit 2 -> 1; prone",
                    observed, initial == 2 && armedBefore && afterApplied == 1 &&
                    !armedAfter && proneAfter,
                    "native light armor/load, persisted marker, UnitState prone"),
                Assertion("dodge-trigger-ac", "20 -> 24; duplicate remains 24",
                    observed, acAfter == 24 && acDuplicate == 24,
                    "exact RuleCalculateAC TargetAC mutation"),
                Assertion("dodge-insufficient-atomic",
                    "grit 0; marker consumed; standing; AC remains 20", observed,
                    afterRejected == 0 && rejectedConsumed && !rejectedProne &&
                    rejectedAc == 20, "production fail-closed reaction adapter"),
                Assertion("dodge-diagnostics",
                    "applied=1;rejected=1;duplicates=1;faults=0", observed,
                    GunslingerDodgeRuntimeDiagnostics.Applied == 1 &&
                    GunslingerDodgeRuntimeDiagnostics.Rejected == 1 &&
                    GunslingerDodgeRuntimeDiagnostics.Duplicates == 1 &&
                    GunslingerDodgeRuntimeDiagnostics.Faults == 0,
                    "production reaction diagnostics"),
                Assertion("external-isolation", "unchanged party and global-unit snapshots",
                    "cleaned=" + cleaned, cleaned,
                    "armor removed and detached units disposed"),
                Assertion("loaded-mod-version", _request.ExpectedModVersion,
                    _context.ModEntry.Info.Version,
                    _request.ExpectedModVersion == _context.ModEntry.Info.Version,
                    "Unity Mod Manager ModEntry.Info.Version")
            };
            return CreateResult(assertions.TrueForAll(value => value.Status == "PASS")
                ? RuntimeTestStatuses.Pass : RuntimeTestStatuses.Fail, assertions, null);
        }

        private RuntimeTestResult RunDisposableGunslingerDeadeye()
        {
            BlueprintUnit source = BlueprintRoot.Instance.DefaultPlayerCharacter;
            GunslingerClassBlueprintSet gunslingerSet = BlueprintBootstrap.GunslingerClass;
            BlueprintAbilityResource grit = gunslingerSet.Grit.Resource;
            BlueprintItemWeapon pistolBlueprint = BlueprintBootstrap.ProductionFirearms.Pistol.Item;
            object player = ReadExactMember(Kingmaker.Game.Instance, "Player");
            object state = ReadExactMember(Kingmaker.Game.Instance, "State");
            object party = ReadExactMember(player, "Party");
            object allUnits = ReadExactMember(state, "AllUnits");
            object[] partyBefore = SnapshotReferences(party);
            object[] unitsBefore = SnapshotReferences(allUnits);
            Kingmaker.EntitySystem.Entities.UnitEntityData attacker = null;
            Kingmaker.EntitySystem.Entities.UnitEntityData target = null;
            ItemEntityWeapon weapon = null;
            object controller = null;
            int initial = -1, afterApplied = -1, afterDuplicate = -1,
                afterInsufficient = -1;
            bool armedBefore = false, armedAfter = true, authorized = false,
                duplicateAuthorized = false, insufficientAuthorized = true,
                insufficientConsumed = false, cleaned = false;
            string stage = "construct-disposables";
            try
            {
                attacker = new Kingmaker.UI.LevelUp.ChargenUnit(source).Unit;
                target = new Kingmaker.UI.LevelUp.ChargenUnit(source).Unit;
                attacker.Descriptor.Stats.Wisdom.BaseValue = 14;
                Type controllerType = typeof(Kingmaker.UnitLogic.Class.LevelUp.LevelUpController);
                MethodInfo start = controllerType.GetMethods(BindingFlags.Public |
                    BindingFlags.NonPublic | BindingFlags.Static).Single(value =>
                        value.Name == "StartWithoutAssigningStaticInstance" &&
                        value.GetParameters().Length == 5);
                MethodInfo selectClass = controllerType.GetMethod("SelectClass",
                    BindingFlags.Public | BindingFlags.Instance, null,
                    new[] { typeof(BlueprintCharacterClass), typeof(bool) }, null);
                MethodInfo mechanics = controllerType.GetMethod("ApplyClassMechanics",
                    BindingFlags.Public | BindingFlags.Instance);
                MethodInfo applyLevelup = controllerType.GetMethod("ApplyLevelup",
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                MethodInfo cancel = controllerType.GetMethod("Cancel",
                    BindingFlags.Public | BindingFlags.Instance);
                object charGen = Enum.Parse(start.GetParameters()[4].ParameterType,
                    "CharGen", false);
                controller = start.Invoke(null,
                    new object[] { attacker.Descriptor, false, null, null, charGen });
                if (!(bool)selectClass.Invoke(controller,
                    new object[] { gunslingerSet.CharacterClass, false }))
                    throw new InvalidOperationException("Deadeye Gunslinger selection failed.");
                mechanics.Invoke(controller, null);
                applyLevelup.Invoke(controller, new object[] { attacker.Descriptor });
                cancel.Invoke(controller, null); controller = null;

                stage = "arm-and-fire-second-increment";
                DeadeyeRuntimeDiagnostics.Reset();
                initial = attacker.Descriptor.Resources.GetResourceAmount(grit);
                SetExactProperty(attacker, "Position", Vector3.zero);
                SetExactProperty(target, "Position", new Vector3(7f, 0f, 0f));
                weapon = new ItemEntityWeapon(pistolBlueprint);
                FirearmRuntimeState.Service.Set(weapon, new FirearmState(
                    FirearmState.CurrentSchemaVersion, 1,
                    FirearmStateTokenCatalog.DiagnosticLeadBall,
                    FirearmCondition.Normal));
                attacker.Descriptor.AddFact(gunslingerSet.Deadeye.ArmedMarker);
                armedBefore = attacker.Descriptor.HasFact(gunslingerSet.Deadeye.ArmedMarker);
                var roll = new RuleAttackRoll(attacker, target, weapon, 0);
                FirearmDischargeRuntime.BeforeAttackRoll(roll);
                DeadeyeRuntime.BeforeAttackRoll(roll);
                authorized = DeadeyeRuntime.IsAuthorized(roll);
                afterApplied = attacker.Descriptor.Resources.GetResourceAmount(grit);
                armedAfter = attacker.Descriptor.HasFact(gunslingerSet.Deadeye.ArmedMarker);
                DeadeyeRuntime.BeforeAttackRoll(roll);
                duplicateAuthorized = DeadeyeRuntime.IsAuthorized(roll);
                afterDuplicate = attacker.Descriptor.Resources.GetResourceAmount(grit);
                FirearmMisfireRuntime.FinishAttack(roll);

                stage = "insufficient-third-increment";
                SetExactProperty(target, "Position", new Vector3(13f, 0f, 0f));
                FirearmRuntimeState.Service.Set(weapon, new FirearmState(
                    FirearmState.CurrentSchemaVersion, 1,
                    FirearmStateTokenCatalog.DiagnosticLeadBall,
                    FirearmCondition.Normal));
                attacker.Descriptor.AddFact(gunslingerSet.Deadeye.ArmedMarker);
                var insufficient = new RuleAttackRoll(attacker, target, weapon, 0);
                FirearmDischargeRuntime.BeforeAttackRoll(insufficient);
                DeadeyeRuntime.BeforeAttackRoll(insufficient);
                insufficientAuthorized = DeadeyeRuntime.IsAuthorized(insufficient);
                afterInsufficient = attacker.Descriptor.Resources.GetResourceAmount(grit);
                insufficientConsumed = !attacker.Descriptor.HasFact(
                    gunslingerSet.Deadeye.ArmedMarker);
                FirearmMisfireRuntime.FinishAttack(insufficient);
            }
            catch (Exception exception)
            {
                throw new InvalidOperationException(
                    "Disposable Deadeye failed at stage " + stage + ".", exception);
            }
            finally
            {
                MethodInfo cancel = typeof(Kingmaker.UnitLogic.Class.LevelUp.LevelUpController)
                    .GetMethod("Cancel", BindingFlags.Public | BindingFlags.Instance);
                if (controller != null && cancel != null) cancel.Invoke(controller, null);
                if (weapon != null) FirearmRuntimeState.Service.Forget(weapon);
                if (target != null) target.Dispose();
                if (attacker != null) attacker.Dispose();
                cleaned = SameReferences(partyBefore, SnapshotReferences(party)) &&
                    SameReferences(unitsBefore, SnapshotReferences(allUnits)) &&
                    (attacker == null || !ContainsReference(allUnits, attacker)) &&
                    (target == null || !ContainsReference(allUnits, target));
            }
            string observed = "initial=" + initial + ";armedBefore=" + armedBefore +
                ";authorized=" + authorized + ";afterApplied=" + afterApplied +
                ";armedAfter=" + armedAfter + ";duplicateAuthorized=" + duplicateAuthorized +
                ";afterDuplicate=" + afterDuplicate + ";insufficientAuthorized=" +
                insufficientAuthorized + ";afterInsufficient=" + afterInsufficient +
                ";insufficientConsumed=" + insufficientConsumed +
                ";applied=" + DeadeyeRuntimeDiagnostics.Applied +
                ";rejected=" + DeadeyeRuntimeDiagnostics.Rejected +
                ";faults=" + DeadeyeRuntimeDiagnostics.Faults;
            var assertions = new List<RuntimeTestAssertion>
            {
                Assertion("deadeye-native-fact", "armed true -> false", observed,
                    armedBefore && !armedAfter, "native persisted BlueprintFeature fact"),
                Assertion("deadeye-second-increment", "grit 2 -> 1; authorized", observed,
                    initial == 2 && afterApplied == 1 && authorized,
                    "exact loaded pistol discharge and 7-meter target distance"),
                Assertion("deadeye-idempotent", "grit remains 1", observed,
                    afterDuplicate == 1 && duplicateAuthorized,
                    "same RuleAttackRoll retained one authorization without another spend"),
                Assertion("deadeye-insufficient-atomic", "grit remains 1; unauthorized; marker consumed",
                    observed, afterInsufficient == 1 && !insufficientAuthorized &&
                    insufficientConsumed, "third increment requires two grit"),
                Assertion("deadeye-diagnostics", "applied=1;rejected=1;faults=0", observed,
                    DeadeyeRuntimeDiagnostics.Applied == 1 &&
                    DeadeyeRuntimeDiagnostics.Rejected == 1 &&
                    DeadeyeRuntimeDiagnostics.Faults == 0,
                    "production Deadeye adapter diagnostics"),
                Assertion("external-isolation", "unchanged party and global-unit snapshots",
                    "cleaned=" + cleaned, cleaned,
                    "controller canceled, firearm state forgotten, disposables disposed"),
                Assertion("loaded-mod-version", _request.ExpectedModVersion,
                    _context.ModEntry.Info.Version,
                    _request.ExpectedModVersion == _context.ModEntry.Info.Version,
                    "Unity Mod Manager ModEntry.Info.Version")
            };
            return CreateResult(assertions.TrueForAll(value => value.Status == "PASS")
                ? RuntimeTestStatuses.Pass : RuntimeTestStatuses.Fail, assertions, null);
        }

        private RuntimeTestResult RunDisposableGunslingerGritRecovery()
        {
            BlueprintUnit source = BlueprintRoot.Instance.DefaultPlayerCharacter;
            GunslingerClassBlueprintSet gunslingerSet = BlueprintBootstrap.GunslingerClass;
            BlueprintCharacterClass gunslinger = gunslingerSet.CharacterClass;
            BlueprintAbilityResource grit = gunslingerSet.Grit.Resource;
            BlueprintItemWeapon pistolBlueprint =
                BlueprintBootstrap.ProductionFirearms.Pistol.Item;
            object player = ReadExactMember(Kingmaker.Game.Instance, "Player");
            object state = ReadExactMember(Kingmaker.Game.Instance, "State");
            object party = ReadExactMember(player, "Party");
            object allUnits = ReadExactMember(state, "AllUnits");
            object[] partyBefore = SnapshotReferences(party);
            object[] unitsBefore = SnapshotReferences(allUnits);
            Kingmaker.EntitySystem.Entities.UnitEntityData attacker = null;
            Kingmaker.EntitySystem.Entities.UnitEntityData target = null;
            object controller = null;
            int maximum = -1;
            int initial = -1;
            int spent = -1;
            int afterCritical = -1;
            int afterCriticalDuplicate = -1;
            int afterKillingBlow = -1;
            int afterKillingDuplicate = -1;
            int afterUnaware = -1;
            int targetDamageBefore = 0;
            bool cleaned = false;
            string stage = "construct-disposables";
            try
            {
                var attackerChargen = new Kingmaker.UI.LevelUp.ChargenUnit(source);
                var targetChargen = new Kingmaker.UI.LevelUp.ChargenUnit(source);
                attacker = attackerChargen.Unit;
                target = targetChargen.Unit;
                if (attacker == null || target == null || attacker.Descriptor == null ||
                    target.Descriptor == null || attacker.Descriptor.Resources == null)
                    throw new InvalidOperationException(
                        "Disposable grit-recovery entities are unavailable.");
                attacker.Descriptor.Stats.Wisdom.BaseValue = 14;
                SetExactProperty(target.Descriptor.Progression,
                    "CharacterLevel", 1);

                Type controllerType = typeof(Kingmaker.UnitLogic.Class.LevelUp.LevelUpController);
                MethodInfo start = controllerType.GetMethods(BindingFlags.Public |
                    BindingFlags.NonPublic | BindingFlags.Static).Single(value =>
                        value.Name == "StartWithoutAssigningStaticInstance" &&
                        value.GetParameters().Length == 5);
                MethodInfo selectClass = controllerType.GetMethod("SelectClass",
                    BindingFlags.Public | BindingFlags.Instance, null,
                    new[] { typeof(BlueprintCharacterClass), typeof(bool) }, null);
                MethodInfo mechanics = controllerType.GetMethod("ApplyClassMechanics",
                    BindingFlags.Public | BindingFlags.Instance);
                MethodInfo applyLevelup = controllerType.GetMethod("ApplyLevelup",
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                MethodInfo cancel = controllerType.GetMethod("Cancel",
                    BindingFlags.Public | BindingFlags.Instance);
                if (selectClass == null || mechanics == null || applyLevelup == null ||
                    cancel == null)
                    throw new MissingMethodException(
                        "An exact native grit-recovery level-up method is unavailable.");

                stage = "grant-and-empty-grit";
                object charGen = Enum.Parse(start.GetParameters()[4].ParameterType,
                    "CharGen", false);
                controller = start.Invoke(null,
                    new object[] { attacker.Descriptor, false, null, null, charGen });
                if (!(bool)selectClass.Invoke(controller,
                    new object[] { gunslinger, false }))
                    throw new InvalidOperationException(
                        "Disposable grit-recovery Gunslinger selection was rejected.");
                mechanics.Invoke(controller, null);
                applyLevelup.Invoke(controller, new object[] { attacker.Descriptor });
                cancel.Invoke(controller, null);
                controller = null;
                maximum = grit.GetMaxAmount(attacker.Descriptor);
                initial = attacker.Descriptor.Resources.GetResourceAmount(grit);
                attacker.Descriptor.Resources.Spend(grit, initial);
                spent = attacker.Descriptor.Resources.GetResourceAmount(grit);

                stage = "install-detached-combat-state";
                var attackerCombat =
                    new Kingmaker.Controllers.Combat.UnitCombatState(attacker);
                var targetCombat =
                    new Kingmaker.Controllers.Combat.UnitCombatState(target);
                SetExactField(attackerCombat, "m_InCombat", true);
                SetExactField(targetCombat, "m_InCombat", true);
                SetExactProperty(attacker, "CombatState", attackerCombat);
                SetExactProperty(target, "CombatState", targetCombat);
                if (!attacker.IsInCombat || !target.IsInCombat)
                    throw new InvalidOperationException(
                        "Detached native combat-state flags did not become observable.");

                stage = "confirmed-critical";
                var weapon = new ItemEntityWeapon(pistolBlueprint);
                var attackRoll = new RuleAttackRoll(attacker, target, weapon, 0);
                var weaponAttack = new RuleAttackWithWeapon(attacker, target, weapon, 0);
                attackRoll.RuleAttackWithWeapon = weaponAttack;
                SetExactProperty(weaponAttack, "AttackRoll", attackRoll);
                SetExactProperty(attackRoll, "IsCriticalConfirmed", true);
                FirearmGritRecoveryRuntimeDiagnostics.Reset();
                FirearmGritRecoveryRuntime.AfterAttackRoll(attackRoll);
                afterCritical = attacker.Descriptor.Resources.GetResourceAmount(grit);
                FirearmGritRecoveryRuntime.AfterAttackRoll(attackRoll);
                afterCriticalDuplicate =
                    attacker.Descriptor.Resources.GetResourceAmount(grit);

                stage = "killing-blow";
                var bundle = new DamageBundle(new BaseDamage[0]);
                var damage = new RuleDealDamage(attacker, target, bundle);
                damage.AttackRoll = attackRoll;
                SetExactProperty(weaponAttack, "MeleeDamage", damage);
                targetDamageBefore = target.Damage;
                FirearmGritRecoveryRuntime.BeforeDamage(damage);
                target.Damage = target.MaxHP;
                SetExactProperty(damage, "Damage", 1);
                FirearmGritRecoveryRuntime.AfterDamage(damage);
                afterKillingBlow = attacker.Descriptor.Resources.GetResourceAmount(grit);
                FirearmGritRecoveryRuntime.AfterDamage(damage);
                afterKillingDuplicate =
                    attacker.Descriptor.Resources.GetResourceAmount(grit);

                stage = "unaware-target-exclusion";
                attacker.Descriptor.Resources.Spend(grit, 1);
                target.Damage = targetDamageBefore;
                SetExactProperty(target, "CombatState", null);
                var unawareRoll = new RuleAttackRoll(attacker, target, weapon, 0);
                var unawareAttack = new RuleAttackWithWeapon(attacker, target, weapon, 0);
                unawareRoll.RuleAttackWithWeapon = unawareAttack;
                SetExactProperty(unawareAttack, "AttackRoll", unawareRoll);
                SetExactProperty(unawareRoll, "IsCriticalConfirmed", true);
                FirearmGritRecoveryRuntime.AfterAttackRoll(unawareRoll);
                afterUnaware = attacker.Descriptor.Resources.GetResourceAmount(grit);
            }
            catch (Exception exception)
            {
                throw new InvalidOperationException(
                    "Disposable grit recovery failed at stage " + stage + ".", exception);
            }
            finally
            {
                MethodInfo cancel = typeof(Kingmaker.UnitLogic.Class.LevelUp.LevelUpController)
                    .GetMethod("Cancel", BindingFlags.Public | BindingFlags.Instance);
                if (controller != null && cancel != null) cancel.Invoke(controller, null);
                if (target != null)
                {
                    target.Damage = targetDamageBefore;
                    SetExactProperty(target, "CombatState", null);
                }
                if (attacker != null) SetExactProperty(attacker, "CombatState", null);
                if (target != null) target.Dispose();
                if (attacker != null) attacker.Dispose();
                cleaned = SameReferences(partyBefore, SnapshotReferences(party)) &&
                    SameReferences(unitsBefore, SnapshotReferences(allUnits)) &&
                    (attacker == null || !ContainsReference(allUnits, attacker)) &&
                    (target == null || !ContainsReference(allUnits, target));
            }
            string observed = "maximum=" + maximum + ";initial=" + initial +
                ";spent=" + spent + ";afterCritical=" + afterCritical +
                ";afterCriticalDuplicate=" + afterCriticalDuplicate +
                ";afterKillingBlow=" + afterKillingBlow +
                ";afterKillingDuplicate=" + afterKillingDuplicate +
                ";afterUnaware=" + afterUnaware +
                ";criticalApplied=" + FirearmGritRecoveryRuntimeDiagnostics.CriticalApplied +
                ";killingApplied=" + FirearmGritRecoveryRuntimeDiagnostics.KillingBlowApplied +
                ";duplicates=" + FirearmGritRecoveryRuntimeDiagnostics.Duplicates +
                ";ignored=" + FirearmGritRecoveryRuntimeDiagnostics.Ignored +
                ";faults=" + FirearmGritRecoveryRuntimeDiagnostics.Faults;
            var assertions = new List<RuntimeTestAssertion>
            {
                Assertion("recovery-fixture", "maximum=2;initial=2;spent=0",
                    observed, maximum == 2 && initial == 2 && spent == 0,
                    "detached Wisdom 14 Gunslinger and exact native grit spend"),
                Assertion("confirmed-critical-restores-once", "0 -> 1; duplicate remains 1",
                    observed, afterCritical == 1 && afterCriticalDuplicate == 1,
                    "exact production firearm RuleAttackRoll reference identity"),
                Assertion("killing-blow-restores-once", "1 -> 2; duplicate remains 2",
                    observed, afterKillingBlow == 2 && afterKillingDuplicate == 2,
                    "exact RuleAttackWithWeapon.MeleeDamage and target crossing zero"),
                Assertion("unaware-target-rejected", "spent to 1; remains 1",
                    observed, afterUnaware == 1,
                    "target lacked native combat state at exact attack observation"),
                Assertion("recovery-diagnostics", "critical=1;kill=1;duplicates=2;ignored=1;faults=0",
                    observed,
                    FirearmGritRecoveryRuntimeDiagnostics.CriticalApplied == 1 &&
                    FirearmGritRecoveryRuntimeDiagnostics.KillingBlowApplied == 1 &&
                    FirearmGritRecoveryRuntimeDiagnostics.Duplicates == 2 &&
                    FirearmGritRecoveryRuntimeDiagnostics.Ignored == 1 &&
                    FirearmGritRecoveryRuntimeDiagnostics.Faults == 0,
                    "production recovery adapter diagnostics"),
                Assertion("external-isolation", "unchanged party and global-unit snapshots",
                    "cleaned=" + cleaned, cleaned,
                    "controller canceled, combat states cleared, disposable entities disposed"),
                Assertion("loaded-mod-version", _request.ExpectedModVersion,
                    _context.ModEntry.Info.Version,
                    _request.ExpectedModVersion == _context.ModEntry.Info.Version,
                    "Unity Mod Manager ModEntry.Info.Version")
            };
            bool pass = assertions.TrueForAll(value => value.Status == "PASS");
            return CreateResult(pass ? RuntimeTestStatuses.Pass :
                RuntimeTestStatuses.Fail, assertions, null);
        }

        private static void SetExactProperty(object value, string name,
            object propertyValue)
        {
            if (value == null) throw new ArgumentNullException("value");
            PropertyInfo property = value.GetType().GetProperty(name,
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            MethodInfo setter = property == null ? null : property.GetSetMethod(true);
            if (setter == null)
                throw new MissingMemberException(value.GetType().FullName, name);
            setter.Invoke(value, new[] { propertyValue });
        }

        private static void SetExactField(object value, string name,
            object fieldValue)
        {
            if (value == null) throw new ArgumentNullException("value");
            FieldInfo field = value.GetType().GetField(name,
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (field == null)
                throw new MissingFieldException(value.GetType().FullName, name);
            field.SetValue(value, fieldValue);
        }

        private static object ReadExactMember(object value, string name)
        {
            if (value == null) return null;
            const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic |
                BindingFlags.Instance;
            PropertyInfo property = value.GetType().GetProperty(name, flags);
            if (property != null) return property.GetValue(value, null);
            FieldInfo field = value.GetType().GetField(name, flags);
            return field == null ? null : field.GetValue(value);
        }

        private static object[] SnapshotReferences(object collection)
        {
            var values = new List<object>();
            var enumerable = collection as System.Collections.IEnumerable;
            if (enumerable != null)
                foreach (object value in enumerable) values.Add(value);
            return values.ToArray();
        }

        private static bool ContainsReference(object collection, object target)
        {
            return SnapshotReferences(collection).Any(value => ReferenceEquals(value, target));
        }

        private static bool SameReferences(object[] left, object[] right)
        {
            if (left.Length != right.Length) return false;
            for (int index = 0; index < left.Length; index++)
                if (!ReferenceEquals(left[index], right[index])) return false;
            return true;
        }

        private static string DescribeBlueprintUnit(BlueprintUnit unit)
        {
            return unit == null ? "<missing>" : unit.name + "@" + unit.AssetGuid;
        }

        private static string DescribeCreationType(Type type)
        {
            const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic |
                BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
            string constructors = string.Join(",", type.GetConstructors(flags)
                .Select(value => value.ToString()).OrderBy(value => value,
                    StringComparer.Ordinal).ToArray());
            string methods = string.Join(",", type.GetMethods(flags)
                .Where(value => !value.IsSpecialName)
                .Select(value => value.ToString()).OrderBy(value => value,
                    StringComparer.Ordinal).ToArray());
            string members = string.Join(",", type.GetMembers(flags)
                .Where(value => value.MemberType == MemberTypes.Field ||
                    value.MemberType == MemberTypes.Property)
                .Select(value => value.MemberType + ":" + value.Name)
                .OrderBy(value => value, StringComparer.Ordinal).ToArray());
            string enumValues = type.IsEnum ? string.Join(",", Enum.GetNames(type)) : "";
            return "type=" + type.FullName + ";constructors=" + constructors +
                ";methods=" + methods + ";members=" + members +
                ";enumValues=" + enumValues;
        }

        private static string DescribeCalledMethods(MethodBase method)
        {
            if (method == null || method.GetMethodBody() == null) return "<unavailable>";
            byte[] il = method.GetMethodBody().GetILAsByteArray();
            var oneByte = new Dictionary<byte, OpCode>();
            var twoByte = new Dictionary<byte, OpCode>();
            foreach (FieldInfo field in typeof(OpCodes).GetFields(
                BindingFlags.Public | BindingFlags.Static))
            {
                if (field.FieldType != typeof(OpCode)) continue;
                OpCode code = (OpCode)field.GetValue(null);
                ushort value = unchecked((ushort)code.Value);
                if (value <= byte.MaxValue) oneByte[(byte)value] = code;
                else if ((value & 0xff00) == 0xfe00) twoByte[(byte)(value & 0xff)] = code;
            }
            var calls = new List<string>();
            int offset = 0;
            while (offset < il.Length)
            {
                byte first = il[offset++];
                OpCode code;
                if (first == 0xfe)
                {
                    if (offset >= il.Length || !twoByte.TryGetValue(il[offset++], out code))
                        break;
                }
                else if (!oneByte.TryGetValue(first, out code)) break;
                int operandStart = offset;
                if (code.OperandType == OperandType.InlineMethod && offset + 4 <= il.Length)
                {
                    int token = BitConverter.ToInt32(il, offset);
                    try
                    {
                        MethodBase called = method.Module.ResolveMethod(token);
                        calls.Add(called.DeclaringType.FullName + "." + called.Name);
                    }
                    catch (ArgumentException) { calls.Add("<unresolved-method-token>"); }
                }
                offset = operandStart + OperandSize(code.OperandType, il, operandStart);
            }
            return string.Join(",", calls.Distinct().ToArray());
        }

        private static MethodInfo RequireExactApplyMethod(Type actionType)
        {
            if (actionType == null) return null;
            return actionType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic |
                BindingFlags.Instance).SingleOrDefault(value =>
                    value.Name == "Apply" && value.GetParameters().Length == 2);
        }

        private static int OperandSize(OperandType type, byte[] il, int offset)
        {
            if (type == OperandType.InlineNone) return 0;
            if (type == OperandType.ShortInlineBrTarget || type == OperandType.ShortInlineI ||
                type == OperandType.ShortInlineVar) return 1;
            if (type == OperandType.InlineVar) return 2;
            if (type == OperandType.InlineI8 || type == OperandType.InlineR) return 8;
            if (type == OperandType.InlineSwitch)
            {
                if (offset + 4 > il.Length) return il.Length - offset;
                return 4 + (BitConverter.ToInt32(il, offset) * 4);
            }
            return 4;
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
