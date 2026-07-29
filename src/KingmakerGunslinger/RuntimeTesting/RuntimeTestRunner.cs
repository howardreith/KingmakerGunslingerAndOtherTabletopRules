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
                RunManualSaveLoadObservation();
            }
            catch (Exception exception)
            {
                _trace.Record("runtime-exception", "stage=runtime-update", exception);
                Complete(CreateResult("ERROR", null, ExceptionSummary(exception)));
            }
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
