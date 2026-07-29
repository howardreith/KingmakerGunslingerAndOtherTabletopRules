using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using KingmakerGunslinger.Bootstrap;
using UnityEngine;
using UnityModManagerNet;

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
        private Stopwatch _catalogElapsed;
        private Stopwatch _selectionElapsed;
        private Stopwatch _completionElapsed;
        private bool _catalogMarkerWritten;

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
                    context.Logger.Warning(
                        "runtime-test",
                        "request.rejected",
                        "reason=" + decision.ReasonCode +
                        "; requestFile=" + decision.SafeRequestName);
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

        private void OnUpdate(UnityModManager.ModEntry modEntry, float deltaTime)
        {
            if (_completed) return;
            if (_manualElapsed == null &&
                _elapsed.Elapsed.TotalSeconds >= _request.StartupTimeoutSeconds)
            {
                _trace.Record("startup-timeout",
                    "stage=observer-readiness; observer was not ready");
                RuntimeTestResult startupTimeout = CreateResult("TIMEOUT", null, null);
                startupTimeout.Diagnostics.Add("timeoutStage=observer-readiness");
                Complete(startupTimeout);
                return;
            }
            if (_manualElapsed != null &&
                _request.Scenario != RuntimeTestScenarioCatalog.ObserveSaveCatalogAndSelection &&
                _request.Scenario != RuntimeTestScenarioCatalog.ObserveSaveCatalogProvider &&
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

            try
            {
                if (_request.Scenario == RuntimeTestScenarioCatalog.ModLoadSmoke)
                {
                    Complete(RunModLoadSmoke());
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
                RunManualSaveLoadObservation();
            }
            catch (Exception exception)
            {
                _trace.Record("runtime-exception", "stage=runtime-update", exception);
                Complete(CreateResult("ERROR", null, ExceptionSummary(exception)));
            }
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

        private void Complete(RuntimeTestResult result)
        {
            if (_saveLoadObservation != null && result.SaveLoadObservation == null)
                result.SaveLoadObservation = _saveLoadObservation.Stop();
            if (_catalogObservation != null && result.SaveCatalogObservation == null)
                result.SaveCatalogObservation = _catalogObservation.Stop();
            _completed = true;
            _context.ModEntry.OnUpdate -= OnUpdate;
            _elapsed.Stop();
            result.EndUtc = DateTime.UtcNow.ToString("o");
            result.DurationMilliseconds = _elapsed.ElapsedMilliseconds;
            result.AutomaticExitRequested = _request.ExitAfterCompletion;
            result.AutomaticExitInitiated = _request.ExitAfterCompletion;
            try
            {
                _trace.Record("result-flush-started", "status=" + result.Status);
                RuntimeTestResultWriter.Write(result, _request.EvidenceDirectory);
                _trace.Record("result-flushed", "status=" + result.Status);
            }
            catch (Exception exception)
            {
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
                AutomaticExitInitiated = false
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
