using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Loot;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.EntitySystem.Stats;
using Newtonsoft.Json;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Development;
using KingmakerGunslinger.Firearms;
using KingmakerGunslinger.Ammunition;
using KingmakerGunslinger.Actions;
using KingmakerGunslinger.Reloading;
using KingmakerGunslinger.Misfires;
using KingmakerGunslinger.Explosions;
using KingmakerGunslinger.Grit;
using KingmakerGunslinger.Deeds;
using KingmakerGunslinger.Firing;
using KingmakerGunslinger.Gunsmithing;
using Kingmaker.Items;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules.Damage;
using UnityEngine;
using UnityModManagerNet;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker;
using Kingmaker.UnitLogic.Class.LevelUp.Actions;
using Kingmaker.Blueprints.Items.Armors;
using Kingmaker.PubSubSystem;
using KingmakerGunslinger.Classes;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.Utility;

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

        private sealed class BroadRespecInitiationHandler :
            ILevelUpInitiateUIHandler
        {
            internal BlueprintCharacterClass SelectedClass;
            internal UnitDescriptor Replacement;
            internal object Controller;
            internal bool Invoked;
            internal bool Selected;

            public void HandleLevelUpStart(UnitDescriptor unit,
                Newtonsoft.Json.Linq.JToken unitJson, Action onSuccess,
                Kingmaker.UnitLogic.Class.LevelUp.LevelUpState.CharBuildMode mode)
            {
                Invoked = true;
                Replacement = unit;
                Type type = typeof(Kingmaker.UnitLogic.Class.LevelUp.LevelUpController);
                MethodInfo start = type.GetMethods(BindingFlags.Public |
                    BindingFlags.NonPublic | BindingFlags.Static).Single(value =>
                        value.Name == "StartWithoutAssigningStaticInstance" &&
                        value.GetParameters().Length == 5);
                MethodInfo selectClass = type.GetMethod("SelectClass",
                    BindingFlags.Public | BindingFlags.Instance, null,
                    new[] { typeof(BlueprintCharacterClass), typeof(bool) }, null);
                MethodInfo mechanics = type.GetMethod("ApplyClassMechanics",
                    BindingFlags.Public | BindingFlags.Instance);
                MethodInfo commit = type.GetMethod("Commit",
                    BindingFlags.Public | BindingFlags.Instance);
                if (unit == null || SelectedClass == null || selectClass == null ||
                    mechanics == null || commit == null)
                    throw new InvalidOperationException(
                        "Broad respec initiation contract is incomplete.");
                Controller = start.Invoke(null,
                    new object[] { unit, false, unitJson, onSuccess, mode });
                Selected = (bool)selectClass.Invoke(Controller,
                    new object[] { SelectedClass, false });
                if (!Selected)
                    throw new InvalidOperationException(
                        "Broad respec Gunslinger selection was rejected.");
                mechanics.Invoke(Controller, null);
                commit.Invoke(Controller, null);
                Controller = null;
            }
        }

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
                    _request.Scenario != RuntimeTestScenarioCatalog.DisposableGunslingerLevelUpCommit &&
                    _request.Scenario != RuntimeTestScenarioCatalog.DisposableGunslingerCreationCommit &&
                    _request.Scenario != RuntimeTestScenarioCatalog.DisposableGunslingerLevelTwentyProgression &&
                    _request.Scenario != RuntimeTestScenarioCatalog.DisposableGunslingerEvaluatedChassis &&
                    _request.Scenario != RuntimeTestScenarioCatalog.DisposableGunslingerMulticlassPreview &&
                    _request.Scenario != RuntimeTestScenarioCatalog.DisposableGunslingerMulticlassCommit &&
                    _request.Scenario != RuntimeTestScenarioCatalog.DisposableGunslingerRespecPreview &&
                    _request.Scenario != RuntimeTestScenarioCatalog.DisposableGunslingerRespecCommit &&
                    _request.Scenario != RuntimeTestScenarioCatalog.DisposableGunslingerBroadRespec &&
                    _request.Scenario != RuntimeTestScenarioCatalog.DisposableGunslingerGritResource &&
                    _request.Scenario != RuntimeTestScenarioCatalog.DisposableGunslingerGritRest &&
                    _request.Scenario != RuntimeTestScenarioCatalog.DisposableGunslingerGritPersistence &&
                    _request.Scenario != RuntimeTestScenarioCatalog.DisposableGunslingerGritRecovery &&
                    _request.Scenario != RuntimeTestScenarioCatalog.DisposableGunslingerDeadeye &&
                    _request.Scenario != RuntimeTestScenarioCatalog.DisposableGunslingerDodge &&
                    _request.Scenario != RuntimeTestScenarioCatalog.DisposableGunslingerQuickClear &&
                    _request.Scenario != RuntimeTestScenarioCatalog.DisposableGunslingerNimble &&
                    _request.Scenario != RuntimeTestScenarioCatalog.DisposableGunslingerInitiative &&
                    _request.Scenario != RuntimeTestScenarioCatalog.DisposableGunslingerPistolWhip &&
                    _request.Scenario != RuntimeTestScenarioCatalog.DisposableGunslingerStopBleeding &&
                    _request.Scenario != RuntimeTestScenarioCatalog.DisposableGunslingerBonusFeats &&
                    _request.Scenario != RuntimeTestScenarioCatalog.DisposableGunslingerGunTraining &&
                    _request.Scenario != RuntimeTestScenarioCatalog.DisposableGunslingerStartlingShot &&
                    _request.Scenario != RuntimeTestScenarioCatalog.DisposableGunslingerTargetingHead &&
                    _request.Scenario != RuntimeTestScenarioCatalog.DisposableGunslingerTargetingTorso &&
                    _request.Scenario != RuntimeTestScenarioCatalog.DisposableGunslingerTargetingLegs &&
                    _request.Scenario != RuntimeTestScenarioCatalog.DisposableGunslingerBleedingWound &&
                    _request.Scenario != RuntimeTestScenarioCatalog.DisposableGunslingerExpertLoading &&
                    _request.Scenario != RuntimeTestScenarioCatalog.DisposableGunslingerLightningReload &&
                    _request.Scenario != RuntimeTestScenarioCatalog.DisposableGunslingerEvasive &&
                    _request.Scenario != RuntimeTestScenarioCatalog.ObserveEvasiveNativeFeatures &&
                    _request.Scenario != RuntimeTestScenarioCatalog.ObserveMenacingShotNativeFear &&
                    _request.Scenario != RuntimeTestScenarioCatalog.DisposableGunslingerMenacingShot &&
                    _request.Scenario != RuntimeTestScenarioCatalog.ObserveSlingersLuckNativeRerolls &&
                    _request.Scenario != RuntimeTestScenarioCatalog.DisposableGunslingerSlingersLuck &&
                    _request.Scenario != RuntimeTestScenarioCatalog.DisposableGunslingerCheatDeath &&
                    _request.Scenario != RuntimeTestScenarioCatalog.ObserveDeathsShotNativeDeath &&
                    _request.Scenario != RuntimeTestScenarioCatalog.ObserveStunningShotNativeStunned &&
                    _request.Scenario != RuntimeTestScenarioCatalog.DisposableGunslingerStunningShot &&
                    _request.Scenario != RuntimeTestScenarioCatalog.DisposableGunslingerTrueGrit &&
                    _request.Scenario != RuntimeTestScenarioCatalog.ObserveGunslingerPresentation &&
                    _request.Scenario != RuntimeTestScenarioCatalog.ObserveVendorTableContracts &&
                    _request.Scenario != RuntimeTestScenarioCatalog.ObserveProductionFirearmFallbacks &&
                    _request.Scenario != RuntimeTestScenarioCatalog.ObserveFirearmItemLifecycleContracts &&
                    _request.Scenario != RuntimeTestScenarioCatalog.DisposableProductionFirearmSwitching &&
                    _request.Scenario != RuntimeTestScenarioCatalog.DisposableGunslingerComprehensiveAcceptance &&
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
                    RuntimeTestScenarioCatalog.ObserveGunslingerPresentation)
                {
                    Complete(RunGunslingerPresentationObservation());
                    return;
                }
                if (_request.Scenario ==
                    RuntimeTestScenarioCatalog.ObserveVendorTableContracts)
                {
                    Complete(RunVendorTableContractObservation());
                    return;
                }
                if (_request.Scenario ==
                    RuntimeTestScenarioCatalog.ObserveProductionFirearmFallbacks)
                {
                    Complete(RunProductionFirearmFallbackObservation());
                    return;
                }
                if (_request.Scenario ==
                    RuntimeTestScenarioCatalog.ObserveFirearmItemLifecycleContracts)
                {
                    Complete(RunFirearmItemLifecycleContractObservation());
                    return;
                }
                if (_request.Scenario ==
                    RuntimeTestScenarioCatalog.DisposableReloadAutocast)
                {
                    Complete(RunDisposableReloadAutocast());
                    return;
                }
                if (_request.Scenario ==
                    RuntimeTestScenarioCatalog.DisposableProductionFirearmSwitching)
                {
                    Complete(RunDisposableProductionFirearmSwitching());
                    return;
                }
                if (_request.Scenario == RuntimeTestScenarioCatalog.
                    DisposableGunslingerComprehensiveAcceptance)
                {
                    Complete(RunDisposableGunslingerComprehensiveAcceptance());
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
                    RuntimeTestScenarioCatalog.DisposableGunslingerLevelUpCommit)
                {
                    Complete(RunDisposableGunslingerLevelUpCommit());
                    return;
                }
                if (_request.Scenario ==
                    RuntimeTestScenarioCatalog.DisposableGunslingerCreationCommit)
                {
                    Complete(RunDisposableGunslingerCreationCommit());
                    return;
                }
                if (_request.Scenario == RuntimeTestScenarioCatalog.
                    DisposableGunslingerLevelTwentyProgression)
                {
                    Complete(RunDisposableGunslingerLevelTwentyProgression());
                    return;
                }
                if (_request.Scenario ==
                    RuntimeTestScenarioCatalog.DisposableGunslingerEvaluatedChassis)
                {
                    Complete(RunDisposableGunslingerEvaluatedChassis());
                    return;
                }
                if (_request.Scenario ==
                    RuntimeTestScenarioCatalog.DisposableGunslingerMulticlassPreview)
                {
                    Complete(RunDisposableGunslingerMulticlassPreview());
                    return;
                }
                if (_request.Scenario ==
                    RuntimeTestScenarioCatalog.DisposableGunslingerMulticlassCommit)
                {
                    Complete(RunDisposableGunslingerMulticlassCommit());
                    return;
                }
                if (_request.Scenario ==
                    RuntimeTestScenarioCatalog.DisposableGunslingerRespecPreview)
                {
                    Complete(RunDisposableGunslingerRespecPreview());
                    return;
                }
                if (_request.Scenario ==
                    RuntimeTestScenarioCatalog.DisposableGunslingerRespecCommit)
                {
                    Complete(RunDisposableGunslingerRespecCommit());
                    return;
                }
                if (_request.Scenario ==
                    RuntimeTestScenarioCatalog.DisposableGunslingerBroadRespec)
                {
                    Complete(RunDisposableGunslingerBroadRespec());
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
                    RuntimeTestScenarioCatalog.DisposableGunslingerPistolWhip)
                {
                    Complete(RunDisposableGunslingerPistolWhip());
                    return;
                }
                if (_request.Scenario ==
                    RuntimeTestScenarioCatalog.DisposableGunslingerStopBleeding)
                {
                    Complete(RunDisposableGunslingerStopBleeding());
                    return;
                }
                if (_request.Scenario ==
                    RuntimeTestScenarioCatalog.DisposableGunslingerBonusFeats)
                {
                    Complete(RunDisposableGunslingerBonusFeats());
                    return;
                }
                if (_request.Scenario ==
                    RuntimeTestScenarioCatalog.DisposableGunslingerGunTraining)
                {
                    Complete(RunDisposableGunslingerGunTraining());
                    return;
                }
                if (_request.Scenario ==
                    RuntimeTestScenarioCatalog.DisposableGunslingerDeadShot)
                {
                    Complete(RunDisposableGunslingerDeadShot());
                    return;
                }
                if (_request.Scenario ==
                    RuntimeTestScenarioCatalog.DisposableGunslingerScatterShot)
                {
                    Complete(RunDisposableGunslingerScatterShot());
                    return;
                }
                if (_request.Scenario ==
                    RuntimeTestScenarioCatalog.DisposableGunslingerStartlingShot)
                {
                    Complete(RunDisposableGunslingerStartlingShot());
                    return;
                }
                if (_request.Scenario ==
                    RuntimeTestScenarioCatalog.DisposableGunslingerTargetingHead)
                {
                    Complete(RunDisposableGunslingerTargetingHead());
                    return;
                }
                if (_request.Scenario ==
                    RuntimeTestScenarioCatalog.DisposableGunslingerTargetingTorso)
                {
                    Complete(RunDisposableGunslingerTargetingTorso());
                    return;
                }
                if (_request.Scenario ==
                    RuntimeTestScenarioCatalog.DisposableGunslingerTargetingLegs)
                {
                    Complete(RunDisposableGunslingerTargetingLegs());
                    return;
                }
                if (_request.Scenario ==
                    RuntimeTestScenarioCatalog.DisposableGunslingerTargetingArms)
                {
                    Complete(RunDisposableGunslingerTargetingArms());
                    return;
                }
                if (_request.Scenario ==
                    RuntimeTestScenarioCatalog.DisposableGunslingerBleedingWound)
                {
                    Complete(RunDisposableGunslingerBleedingWound());
                    return;
                }
                if (_request.Scenario ==
                    RuntimeTestScenarioCatalog.DisposableGunslingerExpertLoading)
                {
                    Complete(RunDisposableGunslingerExpertLoading());
                    return;
                }
                if (_request.Scenario ==
                    RuntimeTestScenarioCatalog.DisposableGunslingerLightningReload)
                {
                    Complete(RunDisposableGunslingerLightningReload());
                    return;
                }
                if (_request.Scenario ==
                    RuntimeTestScenarioCatalog.DisposableGunslingerEvasive)
                {
                    Complete(RunDisposableGunslingerEvasive());
                    return;
                }
                if (_request.Scenario ==
                    RuntimeTestScenarioCatalog.ObserveEvasiveNativeFeatures)
                {
                    Complete(RunObserveEvasiveNativeFeatures());
                    return;
                }
                if (_request.Scenario ==
                    RuntimeTestScenarioCatalog.ObserveMenacingShotNativeFear)
                {
                    Complete(RunObserveMenacingShotNativeFear());
                    return;
                }
                if (_request.Scenario ==
                    RuntimeTestScenarioCatalog.DisposableGunslingerMenacingShot)
                {
                    Complete(RunDisposableGunslingerMenacingShot());
                    return;
                }
                if (_request.Scenario ==
                    RuntimeTestScenarioCatalog.ObserveSlingersLuckNativeRerolls)
                {
                    Complete(RunObserveSlingersLuckNativeRerolls());
                    return;
                }
                if (_request.Scenario ==
                    RuntimeTestScenarioCatalog.DisposableGunslingerSlingersLuck)
                {
                    Complete(RunDisposableGunslingerSlingersLuck());
                    return;
                }
                if (_request.Scenario ==
                    RuntimeTestScenarioCatalog.DisposableGunslingerCheatDeath)
                {
                    Complete(RunDisposableGunslingerCheatDeath());
                    return;
                }
                if (_request.Scenario ==
                    RuntimeTestScenarioCatalog.ObserveDeathsShotNativeDeath)
                {
                    Complete(RunObserveDeathsShotNativeDeath());
                    return;
                }
                if (_request.Scenario ==
                    RuntimeTestScenarioCatalog.ObserveStunningShotNativeStunned)
                {
                    Complete(RunObserveStunningShotNativeStunned());
                    return;
                }
                if (_request.Scenario ==
                    RuntimeTestScenarioCatalog.DisposableGunslingerStunningShot)
                {
                    Complete(RunDisposableGunslingerStunningShot());
                    return;
                }
                if (_request.Scenario ==
                    RuntimeTestScenarioCatalog.DisposableGunslingerDeathsShot)
                {
                    Complete(RunDisposableGunslingerDeathsShot());
                    return;
                }
                if (_request.Scenario ==
                    RuntimeTestScenarioCatalog.DisposableGunslingerTrueGrit)
                {
                    Complete(RunDisposableGunslingerStunningShot(true));
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
            string criticalProfiles = catalog == null ? "catalog-unavailable" :
                DescribeCriticalProfile("pistol", catalog.Pistol.WeaponType) + ";" +
                DescribeCriticalProfile("musket", catalog.Musket.WeaponType) + ";" +
                DescribeCriticalProfile("blunderbuss", catalog.Blunderbuss.WeaponType) + ";" +
                DescribeCriticalProfile("rifle", catalog.AdvancedRifle.WeaponType) + ";" +
                DescribeCriticalProfile("revolver", catalog.AdvancedRevolver.WeaponType);

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
                        catalog.Blunderbuss.Spec.IsPlayerFireable &&
                        blunderbussUnavailable == 1,
                    "special-range definition and concrete item restriction"),
                Assertion("production-critical-profiles",
                    "pistol=20/x4;musket=20/x4;blunderbuss=20/x2;" +
                        "rifle=20/x4;revolver=20/x4",
                    criticalProfiles,
                    catalog != null &&
                        HasCriticalProfile(catalog.Pistol.WeaponType, 20, 4) &&
                        HasCriticalProfile(catalog.Musket.WeaponType, 20, 4) &&
                        HasCriticalProfile(catalog.Blunderbuss.WeaponType, 20, 2) &&
                        HasCriticalProfile(catalog.AdvancedRifle.WeaponType, 20, 4) &&
                        HasCriticalProfile(catalog.AdvancedRevolver.WeaponType, 20, 4),
                    "registered BlueprintWeaponType native critical fields"),
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
            long batteredSaleValue = -1;
            long ordinarySaleValue = -1;
            bool exactOwnership = false;
            bool transferRetained = false;
            bool vendorRoundTrip = false;
            bool vendorDealRoundTrip = false;
            Kingmaker.EntitySystem.Entities.UnitEntityData transferUnit = null;
            UnitDescriptor mainDescriptor = null;
            ItemEntityWeapon batteredItem = null;
            bool transferredAway = false;
            VendorLogic vendor = null;
            Kingmaker.EntitySystem.Entities.UnitEntityData vendorUnit = null;
            bool vendorTradingStarted = false;
            string vendorPhase = "not-started";
            List<object> vendorStoreBefore = null;

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

                mainDescriptor = player.MainCharacter.Value.Descriptor;
                BlueprintCharacterClass maximum = mainDescriptor.Progression.GetMaxClass();
                classData = mainDescriptor.Progression.GetClassData(maximum);
                if (classData == null)
                    throw new InvalidOperationException(
                        "The main character has no exact maximum ClassData receiver.");
                if (mainDescriptor.Progression.GetClassData(gunslinger) != null)
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
                LevelUpHelper.AddStartingItems(mainDescriptor);

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
                    pistolCount == 1 && powderCount == 20 && ballCount == 20;
                batteredItem = added.OfType<ItemEntityWeapon>()
                    .Single(item => ReferenceEquals(item.Blueprint, expected[0]));
                Kingmaker.EntitySystem.Entities.UnitEntityData owner;
                exactOwnership = BatteredFirearmOriginRuntime.TryGetOwner(
                    batteredItem, out owner) && ReferenceEquals(owner, mainDescriptor.Unit);
                transferUnit = new Kingmaker.UI.LevelUp.ChargenUnit(
                    BlueprintRoot.Instance.DefaultPlayerCharacter).Unit;
                if (transferUnit == null || transferUnit.Descriptor == null ||
                    transferUnit.Descriptor.Inventory == null)
                    throw new InvalidOperationException(
                        "Detached transfer receiver inventory is unavailable.");
                object ignored;
                string method;
                if (!ReflectionAccess.TryInvokeAny(mainDescriptor.Inventory,
                    new[] { "Extract" }, new[] { new object[] { batteredItem } },
                    out ignored, out method) ||
                    !ReflectionAccess.TryInvokeAny(transferUnit.Descriptor.Inventory,
                    new[] { "Add" }, new[] { new object[] { batteredItem } },
                    out ignored, out method))
                    throw new InvalidOperationException(
                        "Exact native transfer into the detached inventory failed.");
                transferredAway = true;
                Kingmaker.EntitySystem.Entities.UnitEntityData transferredOwner;
                transferRetained = BatteredFirearmOriginRuntime.TryGetOwner(
                    batteredItem, out transferredOwner) &&
                    ReferenceEquals(transferredOwner, mainDescriptor.Unit) &&
                    FirearmRuntimeState.ReadStateTokenIds(batteredItem).Count == 0;
                if (!ReflectionAccess.TryInvokeAny(transferUnit.Descriptor.Inventory,
                    new[] { "Extract" }, new[] { new object[] { batteredItem } },
                    out ignored, out method) ||
                    !ReflectionAccess.TryInvokeAny(mainDescriptor.Inventory,
                    new[] { "Add" }, new[] { new object[] { batteredItem } },
                    out ignored, out method))
                    throw new InvalidOperationException(
                        "Exact native transfer rollback to the source inventory failed.");
                transferredAway = false;
                BlueprintUnit capitalVendor = FindVendorUnit(
                    CapitalVendorBlueprints.TableGuid,
                    "c8d4913edee594749b706de35924617e");
                vendorPhase = "construct-receiver";
                vendorUnit = new Kingmaker.UI.LevelUp.ChargenUnit(capitalVendor).Unit;
                if (vendorUnit == null)
                    throw new InvalidOperationException(
                        "The detached capital-vendor receiver is unavailable.");
                vendor = new VendorLogic();
                vendorPhase = "begin-trading";
                vendor.BeginTrading(vendorUnit);
                vendorTradingStarted = true;
                vendorStoreBefore = EnumerateRuntimeInventory(
                    vendorUnit.Descriptor.Inventory);
                vendorPhase = "add-for-sell";
                vendor.AddForSell(batteredItem, 1);
                ItemEntity staged = EnumerateRuntimeInventory(vendor.ItemsForSell)
                    .OfType<ItemEntity>().Single(item =>
                        ReferenceEquals(item, batteredItem));
                vendorPhase = "remove-from-sell";
                ItemEntity returned = vendor.RemoveFromSell(staged, 1);
                Kingmaker.EntitySystem.Entities.UnitEntityData vendorOwner;
                vendorRoundTrip = ReferenceEquals(staged, batteredItem) &&
                    ReferenceEquals(returned, batteredItem) &&
                    BatteredFirearmOriginRuntime.TryGetOwner(
                        batteredItem, out vendorOwner) &&
                    ReferenceEquals(vendorOwner, mainDescriptor.Unit);
                vendorPhase = "sale-prices";
                batteredSaleValue = vendor.GetItemSellPrice(batteredItem);
                var ordinary = new ItemEntityWeapon(
                    (BlueprintItemWeapon)expected[0]);
                ordinarySaleValue = vendor.GetItemSellPrice(ordinary);
                vendorPhase = "stage-sale-deal";
                vendor.AddForSell(batteredItem, 1);
                vendorPhase = "commit-sale-deal";
                vendor.Deal();
                ItemEntityWeapon[] vendorPistols = EnumerateRuntimeInventory(
                    vendorUnit.Descriptor.Inventory).OfType<ItemEntityWeapon>()
                    .Where(item => ReferenceEquals(item.Blueprint, expected[0]))
                    .ToArray();
                ItemEntityWeapon[] storePistols = EnumerateRuntimeInventory(
                    vendor.StoreItems).OfType<ItemEntityWeapon>()
                    .Where(item => ReferenceEquals(item.Blueprint, expected[0]))
                    .ToArray();
                diagnostics.Add("postSale:moneyDelta=" +
                    (player.Money - moneyBefore) + ";vendorPistols=" +
                    vendorPistols.Length + ";storePistols=" +
                    storePistols.Length + ";sellStage=" +
                    EnumerateRuntimeInventory(vendor.ItemsForSell).Count +
                    ";buyStage=" +
                    EnumerateRuntimeInventory(vendor.ItemsForBuy).Count);
                ItemEntityWeapon stored = storePistols.Single(item =>
                    !BatteredFirearmOriginRuntime.IsBattered(item));
                bool saleCredited = player.Money == moneyBefore + 22;
                vendorPhase = "stage-repurchase-deal";
                object ignoredMoney;
                string moneyMethod;
                if (!ReflectionAccess.TryInvokeAny(player,
                    new[] { "GainMoney" },
                    new[] { new object[] { 100000L } }, out ignoredMoney,
                    out moneyMethod))
                    throw new InvalidOperationException(
                        "Exact temporary repurchase funding failed.");
                vendor.AddForBuy(stored, 1);
                object ignoredUpdate;
                string updateMethod;
                if (!ReflectionAccess.TryInvokeAny(vendor,
                    new[] { "UpdateDeal" }, new[] { Array.Empty<object>() },
                    out ignoredUpdate, out updateMethod))
                    throw new InvalidOperationException(
                        "Exact native deal update failed.");
                if (!vendor.IsDealPossible)
                    throw new InvalidOperationException(
                        "The funded native repurchase deal is not possible.");
                vendorPhase = "commit-repurchase-deal";
                vendor.Deal();
                ItemEntityWeapon[] sharedPistols = EnumerateRuntimeInventory(
                    player.Inventory).OfType<ItemEntityWeapon>().Where(item =>
                        ReferenceEquals(item.Blueprint, expected[0])).ToArray();
                ItemEntityWeapon[] personalPistols = EnumerateRuntimeInventory(
                    mainDescriptor.Inventory).OfType<ItemEntityWeapon>().Where(item =>
                        ReferenceEquals(item.Blueprint, expected[0])).ToArray();
                diagnostics.Add("postPurchase:moneyDelta=" +
                    (player.Money - moneyBefore) + ";sharedPistols=" +
                    sharedPistols.Length + ";personalPistols=" +
                    personalPistols.Length + ";vendorPistols=" +
                    EnumerateRuntimeInventory(vendorUnit.Descriptor.Inventory)
                        .OfType<ItemEntityWeapon>().Count(item =>
                            ReferenceEquals(item.Blueprint, expected[0])) +
                    ";storePistols=" + EnumerateRuntimeInventory(vendor.StoreItems)
                        .OfType<ItemEntityWeapon>().Count(item =>
                            ReferenceEquals(item.Blueprint, expected[0])) +
                    ";sellStage=" +
                    EnumerateRuntimeInventory(vendor.ItemsForSell).Count +
                    ";buyStage=" +
                    EnumerateRuntimeInventory(vendor.ItemsForBuy).Count);
                ItemEntityWeapon acquired = sharedPistols.Concat(personalPistols)
                    .Distinct().Single(item =>
                        !BatteredFirearmOriginRuntime.IsBattered(item));
                vendorDealRoundTrip = saleCredited && acquired != null;
                moneyStable = player.Money == moneyBefore;
            }
            catch (Exception exception)
            {
                diagnostics.Add("vendorPhase=" + vendorPhase + ";" +
                    exception.GetType().Name + ": " + exception.Message);
            }
            finally
            {
                if (vendor != null)
                {
                    object ignored;
                    string method;
                    ReflectionAccess.TryInvokeAny(vendor,
                        new[] { "ReturnItems" }, new[] { Array.Empty<object>() },
                        out ignored, out method);
                    if (vendorTradingStarted)
                    {
                        try
                        {
                            vendor.EndTraiding();
                        }
                        catch (Exception exception)
                        {
                            diagnostics.Add("Exact vendor cleanup failed: " +
                                exception.GetType().Name + ".");
                        }
                    }
                }
                if (vendorUnit != null) vendorUnit.Dispose();
                if (transferredAway && transferUnit != null && batteredItem != null &&
                    mainDescriptor != null)
                {
                    object ignored;
                    string method;
                    ReflectionAccess.TryInvokeAny(transferUnit.Descriptor.Inventory,
                        new[] { "Extract" }, new[] { new object[] { batteredItem } },
                        out ignored, out method);
                    ReflectionAccess.TryInvokeAny(mainDescriptor.Inventory,
                        new[] { "Add" }, new[] { new object[] { batteredItem } },
                        out ignored, out method);
                }
                if (transferUnit != null) transferUnit.Dispose();
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
                {
                    long moneyDelta = moneyBefore - player.Money;
                    if (moneyDelta != 0)
                    {
                        object ignored;
                        string method;
                        if (!ReflectionAccess.TryInvokeAny(player,
                            new[] { "GainMoney" },
                            new[] { new object[] { moneyDelta } },
                            out ignored, out method))
                            diagnostics.Add("Exact money rollback failed.");
                    }
                    moneyStable = player.Money == moneyBefore;
                }
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
                Assertion("native-starting-item-grant", "pistol=1;powder=20;ball=20",
                    "added=" + addedCount + ";pistol=" + pistolCount +
                        ";powder=" + powderCount + ";ball=" + ballCount,
                    exactGrant,
                    "LevelUpHelper.AddStartingItems on the exact main descriptor"),
                Assertion("battered-origin-and-sale-value",
                    "owner=exact;bound=22gp;ordinary=native-non22",
                    "owner=" + exactOwnership + ";bound=" +
                        batteredSaleValue + ";ordinary=" + ordinarySaleValue,
                    exactOwnership && batteredSaleValue == 22 &&
                        ordinarySaleValue != 22,
                    "persisted origin carrier plus patched VendorLogic.GetItemSellPrice"),
                Assertion("native-inventory-transfer",
                    "same item and battered origin survive transfer and return",
                    "retained=" + transferRetained, transferRetained,
                    "exact ItemsCollection Extract/Add across main and detached inventories"),
                Assertion("native-vendor-staging-roundtrip",
                    "same item and battered origin survive AddForSell/RemoveFromSell",
                    "retained=" + vendorRoundTrip, vendorRoundTrip,
                    "exact VendorLogic reversible pre-deal transaction"),
                Assertion("native-vendor-deal-roundtrip",
                    "battered item sells for 22 gp and vendor pistol is acquired ordinary",
                    "retained=" + vendorDealRoundTrip, vendorDealRoundTrip,
                    "exact VendorLogic Deal sale and repurchase transactions"),
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

        private static BlueprintUnit FindVendorUnit(string tableGuid, string unitGuid)
        {
            const BindingFlags Flags = BindingFlags.Instance |
                BindingFlags.Public | BindingFlags.NonPublic;
            FieldInfo tableField = typeof(AddSharedVendor).GetField("m_Table", Flags);
            if (tableField == null)
                throw new MissingFieldException(typeof(AddSharedVendor).FullName,
                    "m_Table");
            BlueprintUnit unit = BlueprintLibraryLookup.RequireExact<BlueprintUnit>(
                BlueprintBootstrap.Library, unitGuid, "native Capital_Jhod vendor");
            BlueprintComponent[] components =
                (unit.ComponentsArray ?? Array.Empty<BlueprintComponent>())
                .Concat((unit.AddFacts ?? Array.Empty<BlueprintUnitFact>())
                    .Where(fact => fact != null)
                    .SelectMany(fact => fact.ComponentsArray ??
                        Array.Empty<BlueprintComponent>())).ToArray();
            int matches = components.OfType<AddSharedVendor>().Count(component =>
            {
                var table = tableField.GetValue(component) as
                    BlueprintSharedVendorTable;
                return table != null && string.Equals(table.AssetGuid,
                    tableGuid, StringComparison.Ordinal);
            });
            if (matches != 1)
                throw new InvalidOperationException(
                    "The exact Capital_Jhod unit does not own one capital table; " +
                    "observed " + matches + ".");
            return unit;
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

        private static string DescribeCriticalProfile(string name,
            BlueprintWeaponType weaponType)
        {
            if (weaponType == null)
                return name + "=unavailable";
            return name + "=" + weaponType.CriticalRollEdge + "/x" +
                (int)weaponType.CriticalModifier;
        }

        private static bool HasCriticalProfile(BlueprintWeaponType weaponType,
            int rollEdge, int multiplier)
        {
            return weaponType != null &&
                weaponType.CriticalRollEdge == rollEdge &&
                (int)weaponType.CriticalModifier == multiplier;
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

        private RuntimeTestResult RunGunslingerPresentationObservation()
        {
            GunslingerClassBlueprintSet set = BlueprintBootstrap.GunslingerClass;
            BlueprintCharacterClass characterClass = set.CharacterClass;
            BlueprintProgression progression = set.Progression;
            var visited = new HashSet<BlueprintUnitFact>();
            var pending = new Stack<BlueprintUnitFact>();
            foreach (LevelEntry entry in progression.LevelEntries)
                foreach (BlueprintFeatureBase feature in entry.Features)
                    if (feature != null) pending.Push(feature);
            int visible = 0, hidden = 0, incomplete = 0, tooltipIncomplete = 0;
            while (pending.Count > 0)
            {
                BlueprintUnitFact fact = pending.Pop();
                if (fact == null || !visited.Add(fact)) continue;
                var feature = fact as BlueprintFeature;
                var ability = fact as BlueprintAbility;
                bool isHidden = (feature != null && feature.HideInUI) ||
                    (ability != null && ability.Hidden);
                bool project = fact.name != null && fact.name.StartsWith("KMG_",
                    StringComparison.Ordinal);
                if (project && isHidden) hidden++;
                if (project && !isHidden)
                {
                    visible++;
                    if (string.IsNullOrWhiteSpace(fact.Name) ||
                        string.IsNullOrWhiteSpace(fact.Description) ||
                        fact.Icon == null) incomplete++;
                    if (ability != null &&
                        (ability.LocalizedDuration == null ||
                         ability.LocalizedSavingThrow == null ||
                         string.IsNullOrWhiteSpace(ability.LocalizedDuration.ToString()) ||
                         string.IsNullOrWhiteSpace(ability.LocalizedSavingThrow.ToString()) ||
                         ability.LocalizedDuration.ToString().IndexOf("<null>",
                             StringComparison.OrdinalIgnoreCase) >= 0 ||
                         ability.LocalizedSavingThrow.ToString().IndexOf("<null>",
                             StringComparison.OrdinalIgnoreCase) >= 0))
                        tooltipIncomplete++;
                }
                var selection = fact as BlueprintFeatureSelection;
                if (selection != null && selection.AllFeatures != null)
                    foreach (BlueprintFeature child in selection.AllFeatures)
                        if (child != null) pending.Push(child);
                if (fact.ComponentsArray == null) continue;
                foreach (AddFacts grant in fact.ComponentsArray.OfType<AddFacts>())
                    if (grant.Facts != null)
                        foreach (BlueprintUnitFact child in grant.Facts)
                            if (child != null) pending.Push(child);
            }
            bool classMetadata = !string.IsNullOrWhiteSpace(characterClass.Name) &&
                !string.IsNullOrWhiteSpace(characterClass.Description) &&
                characterClass.Icon != null;
            bool progressionMetadata = !string.IsNullOrWhiteSpace(progression.Name) &&
                !string.IsNullOrWhiteSpace(progression.Description) &&
                progression.Icon != null;
            int grouped = progression.UIGroups == null ? 0 :
                progression.UIGroups.Sum(group => group == null ||
                    group.Features == null ? 0 : group.Features.Count);
            int topLevelVisible = progression.LevelEntries.Sum(entry =>
                entry.Features.OfType<BlueprintFeature>().Count(feature =>
                    !feature.HideInUI));
            BlueprintAbility reload = BlueprintBootstrap.ReloadTestMusketAbility;
            BlueprintAbility overhaul = BlueprintBootstrap.OverhaulTestMusketAbility;
            BlueprintAbility repair = BlueprintBootstrap.RepairTestMusketAbility;
            bool productionActions = reload != null && overhaul != null && repair != null &&
                reload.Name == "Reload Firearm" &&
                overhaul.Name == "Overhaul Firearm" &&
                repair.Name == "Repair Firearm" &&
                !reload.Description.Contains("Test Musket") &&
                !overhaul.Description.Contains("Test Musket") &&
                !repair.Description.Contains("Test Musket");
            string observed = "levels=" + progression.LevelEntries.Length +
                ";visible=" + visible + ";hidden=" + hidden +
                ";topLevelVisible=" + topLevelVisible +
                ";incomplete=" + incomplete +
                ";tooltipIncomplete=" + tooltipIncomplete + ";groups=" +
                (progression.UIGroups == null ? 0 : progression.UIGroups.Length) +
                ";grouped=" + grouped + ";class=" + classMetadata +
                ";progression=" + progressionMetadata +
                ";actions=" + (reload == null ? "<null>" : reload.Name) + "," +
                    (overhaul == null ? "<null>" : overhaul.Name) + "," +
                    (repair == null ? "<null>" : repair.Name);
            var assertions = new List<RuntimeTestAssertion>
            {
                Assertion("gunslinger-class-presentation",
                    "readable class and progression metadata with icons", observed,
                    classMetadata && progressionMetadata,
                    "registered BlueprintCharacterClass and BlueprintProgression"),
                Assertion("gunslinger-visible-fact-presentation",
                    "all reachable visible project facts have complete names, descriptions, icons, and tooltip metadata",
                    observed, visible > 0 && incomplete == 0 && tooltipIncomplete == 0,
                    "registered progression, selections, and AddFacts graph"),
                Assertion("gunslinger-hidden-fact-exclusion",
                    "hidden implementation facts remain reachable but excluded from visible count",
                    observed, hidden > 0,
                    "HideInUI and Hidden flags"),
                Assertion("gunslinger-progression-ui-groups",
                    "twenty unchanged level entries and nonempty native UI groups",
                    observed, progression.LevelEntries.Length == 20 &&
                        progression.UIGroups != null &&
                        progression.UIGroups.Length > 0 && grouped > 0,
                    "LevelEntries and UIGroups"),
                Assertion("production-firearm-actions-presentation",
                    "Reload Firearm, Overhaul Firearm, Repair Firearm; no Test Musket descriptions",
                    observed, productionActions,
                    "Firearm Proficiency AddFacts reachable stable ability blueprints"),
                Assertion("loaded-mod-version", _request.ExpectedModVersion,
                    _context.ModEntry.Info.Version,
                    _request.ExpectedModVersion == _context.ModEntry.Info.Version,
                    "Unity Mod Manager ModEntry.Info.Version")
            };
            return CreateResult(assertions.TrueForAll(value => value.Status == "PASS")
                ? RuntimeTestStatuses.Pass : RuntimeTestStatuses.Fail,
                assertions, null);
        }

        private RuntimeTestResult RunDisposableReloadAutocast()
        {
            BlueprintAbility parent = BlueprintBootstrap.ReloadTestMusketAbility;
            BlueprintAbility[] variants = parent.ComponentsArray
                .OfType<AbilityVariants>().Single().Variants;
            BlueprintAbility standard = variants.Single(value =>
                value.ActionType == UnitCommand.CommandType.Standard &&
                !value.IsFullRoundAction);
            ReloadTestMusketAbilityLogic logic = standard.ComponentsArray
                .OfType<ReloadTestMusketAbilityLogic>().Single();
            BlueprintItemWeapon pistol = BlueprintBootstrap.ProductionFirearms.Pistol.Item;
            BlueprintItem powder = BlueprintBootstrap.BasicAmmunition.BlackPowder;
            BlueprintItem ball = BlueprintBootstrap.BasicAmmunition.LeadBall;
            Player player = Game.Instance == null ? null : Game.Instance.Player;
            if (player == null || player.Inventory == null)
                throw new InvalidOperationException("The save-free player inventory is unavailable.");
            int powderBefore = player.Inventory.Count(powder);
            int ballBefore = player.Inventory.Count(ball);
            Kingmaker.EntitySystem.Entities.UnitEntityData unit = null;
            ItemEntityWeapon weapon = null;
            bool nativeSelected = false, oneTransaction = false,
                noRetryWhenFull = false, noAmmoLoop = false, cleaned = false;
            int powderAfter = -1, ballAfter = -1, roundsAfter = -1;
            try
            {
                object ignored;
                string method;
                if (!ReflectionAccess.TryInvokeAny(player.Inventory,
                    new[] { "Add" }, new[] { new object[] { powder, 2 },
                        new object[] { powder } }, out ignored, out method) ||
                    !ReflectionAccess.TryInvokeAny(player.Inventory,
                    new[] { "Add" }, new[] { new object[] { ball, 2 },
                        new object[] { ball } }, out ignored, out method))
                    throw new InvalidOperationException("Temporary ammunition could not be added.");
                unit = new Kingmaker.UI.LevelUp.ChargenUnit(
                    BlueprintRoot.Instance.DefaultPlayerCharacter).Unit;
                weapon = new ItemEntityWeapon(pistol);
                unit.Body.PrimaryHand.InsertItem(weapon);
                FirearmRuntimeState.Service.Set(weapon, new FirearmState(
                    FirearmState.CurrentSchemaVersion, 0, null,
                    FirearmCondition.Normal));
                var combat = new Kingmaker.Controllers.Combat.UnitCombatState(unit);
                SetExactProperty(unit, "CombatState", combat);
                var data = new Kingmaker.UnitLogic.Abilities.AbilityData(
                    standard, unit.Descriptor);
                unit.AutoUseAbility = data;
                Kingmaker.UnitLogic.Abilities.AbilityData selected =
                    unit.GetAvailableAutoUseAbility();
                nativeSelected = ReferenceEquals(selected, data) &&
                    data.IsSuitableForAutoUse && data.IsAvailable;
                var context = new Kingmaker.UnitLogic.Abilities.AbilityExecutionContext(
                    selected, new Kingmaker.UnitLogic.Abilities.AbilityParams(),
                    new TargetWrapper(unit), null);
                IEnumerator<AbilityDeliveryTarget> delivery = logic.Deliver(
                    context, new TargetWrapper(unit));
                while (delivery.MoveNext()) { }
                roundsAfter = FirearmRuntimeState.Service.GetOrCreate(weapon)
                    .Repository.State.LoadedRounds;
                powderAfter = player.Inventory.Count(powder);
                ballAfter = player.Inventory.Count(ball);
                oneTransaction = roundsAfter == 1 &&
                    powderAfter == powderBefore + 1 &&
                    ballAfter == ballBefore + 1;
                noRetryWhenFull = unit.GetAvailableAutoUseAbility() == null;

                FirearmRuntimeState.Service.Set(weapon, new FirearmState(
                    FirearmState.CurrentSchemaVersion, 0, null,
                    FirearmCondition.Normal));
                player.Inventory.Remove(powder, player.Inventory.Count(powder) - powderBefore);
                player.Inventory.Remove(ball, player.Inventory.Count(ball) - ballBefore);
                noAmmoLoop = unit.GetAvailableAutoUseAbility() == null &&
                    unit.GetAvailableAutoUseAbility() == null &&
                    FirearmRuntimeState.Service.GetOrCreate(weapon)
                        .Repository.State.LoadedRounds == 0;
            }
            finally
            {
                if (unit != null)
                {
                    unit.AutoUseAbility = null;
                    SetExactProperty(unit, "CombatState", null);
                }
                if (weapon != null)
                {
                    FirearmRuntimeState.Service.Forget(weapon);
                    if (unit != null && unit.Body.PrimaryHand.MaybeItem != null)
                        unit.Body.PrimaryHand.RemoveItem(false);
                }
                int powderExcess = player.Inventory.Count(powder) - powderBefore;
                int ballExcess = player.Inventory.Count(ball) - ballBefore;
                if (powderExcess > 0) player.Inventory.Remove(powder, powderExcess);
                if (ballExcess > 0) player.Inventory.Remove(ball, ballExcess);
                if (unit != null) unit.Dispose();
                cleaned = player.Inventory.Count(powder) == powderBefore &&
                    player.Inventory.Count(ball) == ballBefore;
            }
            string observed = "variants=" + variants.Length + ";selected=" +
                nativeSelected + ";rounds=" + roundsAfter + ";powder=" +
                powderBefore + "->" + powderAfter + ";ball=" + ballBefore +
                "->" + ballAfter + ";fullRetry=" + !noRetryWhenFull +
                ";noAmmoLoop=" + noAmmoLoop + ";cleaned=" + cleaned;
            var assertions = new List<RuntimeTestAssertion>
            {
                Assertion("reload-single-player-action",
                    "one visible parent; four non-autofill native variants",
                    observed, !parent.Hidden && variants.Length == 4 &&
                        variants.All(value => !value.Hidden &&
                            value.ActionBarAutoFillIgnored),
                    "AbilityVariants parent and child presentation"),
                Assertion("native-reload-autocast-selection",
                    "native right-click selection resolves the exact standard Pistol variant",
                    observed, nativeSelected,
                    "UnitEntityData.AutoUseAbility/GetAvailableAutoUseAbility"),
                Assertion("automatic-reload-single-transaction",
                    "one round and one powder/ball pair consumed exactly once",
                    observed, oneTransaction && noRetryWhenFull,
                    "the same ReloadTestMusketAbilityLogic delivery used by manual reload"),
                Assertion("automatic-reload-no-ammunition-loop",
                    "two consecutive native polls reject without mutation",
                    observed, noAmmoLoop,
                    "availability provider plus empty exact firearm state"),
                Assertion("request-local-cleanup", "ammunition and unit fixture restored",
                    observed, cleaned,
                    "guaranteed finally cleanup; no save API"),
                Assertion("loaded-mod-version", _request.ExpectedModVersion,
                    _context.ModEntry.Info.Version,
                    _request.ExpectedModVersion == _context.ModEntry.Info.Version,
                    "Unity Mod Manager ModEntry.Info.Version")
            };
            return CreateResult(assertions.TrueForAll(value => value.Status == "PASS")
                ? RuntimeTestStatuses.Pass : RuntimeTestStatuses.Fail,
                assertions, null);
        }

        private RuntimeTestResult RunVendorTableContractObservation()
        {
            string vendorLogicContract = DescribeCreationType(typeof(VendorLogic));
            const BindingFlags Flags = BindingFlags.Instance |
                BindingFlags.Public | BindingFlags.NonPublic;
            FieldInfo tableField = typeof(AddSharedVendor).GetField("m_Table", Flags);
            FieldInfo lootField = typeof(AddVendorItems).GetField("m_Loot", Flags);
            FieldInfo fixedItemField = typeof(LootItemsPackFixed).GetField("m_Item", Flags);
            FieldInfo fixedCountField = typeof(LootItemsPackFixed).GetField("m_Count", Flags);
            BlueprintSharedVendorTable[] tables = BlueprintBootstrap.Library
                .GetAllBlueprints().OfType<BlueprintSharedVendorTable>()
                .OrderBy(value => value.AssetGuid, StringComparer.Ordinal).ToArray();
            var tableSet = new HashSet<BlueprintSharedVendorTable>(tables);
            var records = new List<string>();
            var ownerRecords = new List<string>();
            var capitalEntries = new List<string>();
            var capitalReferenceContracts = new HashSet<string>(StringComparer.Ordinal);
            var fixedEntryPatterns = new Dictionary<string, int>(StringComparer.Ordinal);
            int projectEntries = 0, invalidProjectCounts = 0, blunderbussEntries = 0;
            int associations = 0, invalidAssociations = 0, supplementalLoot = 0;
            ProductionFirearmBlueprintCatalog production =
                BlueprintBootstrap.ProductionFirearms;
            string criticalProfiles = production == null ? "catalog-unavailable" :
                DescribeCriticalProfile("pistol", production.Pistol.WeaponType) + ";" +
                DescribeCriticalProfile("musket", production.Musket.WeaponType) + ";" +
                DescribeCriticalProfile("blunderbuss",
                    production.Blunderbuss.WeaponType) + ";" +
                DescribeCriticalProfile("rifle",
                    production.AdvancedRifle.WeaponType) + ";" +
                DescribeCriticalProfile("revolver",
                    production.AdvancedRevolver.WeaponType);
            foreach (BlueprintScriptableObject owner in BlueprintBootstrap.Library
                .GetAllBlueprints().Where(value => value != null))
            {
                foreach (AddSharedVendor component in (owner.ComponentsArray ??
                    Array.Empty<BlueprintComponent>()).OfType<AddSharedVendor>())
                {
                    var table = tableField == null ? null :
                        tableField.GetValue(component) as BlueprintSharedVendorTable;
                    ownerRecords.Add("owner=" + owner.GetType().FullName + ":" +
                        owner.name + ":" + owner.AssetGuid + ";table=" +
                        (table == null ? "<null>" : table.name + ":" +
                            table.AssetGuid));
                }
            }
            foreach (BlueprintUnit unit in BlueprintBootstrap.Library.GetAllBlueprints()
                .OfType<BlueprintUnit>().OrderBy(value => value.AssetGuid,
                    StringComparer.Ordinal))
            {
                BlueprintComponent[] components =
                    (unit.ComponentsArray ?? Array.Empty<BlueprintComponent>())
                    .Concat((unit.AddFacts ?? Array.Empty<BlueprintUnitFact>())
                        .Where(value => value != null)
                        .SelectMany(value => value.ComponentsArray ??
                            Array.Empty<BlueprintComponent>())).ToArray();
                AddSharedVendor[] shared = components.OfType<AddSharedVendor>().ToArray();
                if (shared.Length == 0) continue;
                var linkedTables = new List<string>();
                foreach (AddSharedVendor component in shared)
                {
                    var table = tableField == null ? null :
                        tableField.GetValue(component) as BlueprintSharedVendorTable;
                    associations++;
                    if (table == null || !tableSet.Contains(table))
                        invalidAssociations++;
                    linkedTables.Add(table == null ? "<null>" :
                        table.name + ":" + table.AssetGuid + ":" +
                        DescribeBlueprintComponents(table));
                }
                var linkedLoot = new List<string>();
                foreach (AddVendorItems component in components.OfType<AddVendorItems>())
                {
                    var loot = lootField == null ? null :
                        lootField.GetValue(component) as BlueprintUnitLoot;
                    supplementalLoot++;
                    linkedLoot.Add(loot == null ? "<null>" :
                        loot.name + ":" + loot.AssetGuid + ":" +
                        DescribeBlueprintComponents(loot));
                }
                records.Add("unit=" + unit.name + ";display=" +
                    unit.CharacterName + ";guid=" + unit.AssetGuid + ";tables=" +
                    string.Join(",", linkedTables.ToArray()) + ";loot=" +
                    string.Join(",", linkedLoot.ToArray()));
            }
            records.Sort(StringComparer.Ordinal);
            ownerRecords.Sort(StringComparer.Ordinal);
            BlueprintSharedVendorTable capitalTable = tables.SingleOrDefault(value =>
                string.Equals(value.AssetGuid,
                    "afa2c7f292b8e1c4d9c835f0e8047dd3", StringComparison.Ordinal));
            if (capitalTable != null && fixedItemField != null && fixedCountField != null)
            {
                foreach (LootItemsPackFixed component in
                    (capitalTable.ComponentsArray ?? Array.Empty<BlueprintComponent>())
                    .OfType<LootItemsPackFixed>())
                {
                    object itemReference = fixedItemField.GetValue(component);
                    object count = fixedCountField.GetValue(component);
                    Type referenceType = itemReference == null ? null :
                        itemReference.GetType();
                    PropertyInfo itemProperty = referenceType == null ? null :
                        referenceType.GetProperty("Item", Flags);
                    var item = itemProperty == null ? null :
                        itemProperty.GetValue(itemReference, null) as BlueprintItem;
                    capitalEntries.Add("item=" + (item == null ? "<null>" :
                        item.name + ":" + item.AssetGuid + ":cost=" + item.Cost +
                        ":stackable=" + item.IsActuallyStackable) + ":reference=" +
                        (referenceType == null ? "<null>" : referenceType.FullName) +
                        ":count=" + count);
                    if (referenceType != null)
                    {
                        capitalReferenceContracts.Add("declared=" +
                            fixedItemField.FieldType.FullName + ";runtime=" +
                            referenceType.FullName + ";properties=" + string.Join(",",
                                referenceType.GetProperties(Flags)
                                    .OrderBy(value => value.Name, StringComparer.Ordinal)
                                    .Select(value => value.Name + ":" +
                                        value.PropertyType.FullName).ToArray()) +
                            ";methods=" + string.Join(",",
                                referenceType.GetMethods(Flags)
                                    .Where(value => value.GetParameters().Length == 0)
                                    .OrderBy(value => value.Name, StringComparer.Ordinal)
                                    .Select(value => value.Name + ":" +
                                        value.ReturnType.FullName).ToArray()));
                    }
                }
                var expectedProjectItems = new Dictionary<BlueprintItem, int>
                {
                    { BlueprintBootstrap.ProductionFirearms.Pistol.Item, 1 },
                    { BlueprintBootstrap.ProductionFirearms.Musket.Item, 1 },
                    { BlueprintBootstrap.ProductionFirearms.Blunderbuss.Item, 1 },
                    { BlueprintBootstrap.ProductionFirearms.AdvancedRifle.Item, 1 },
                    { BlueprintBootstrap.ProductionFirearms.AdvancedRevolver.Item, 1 },
                    { BlueprintBootstrap.BasicAmmunition.BlackPowder, 99 },
                    { BlueprintBootstrap.BasicAmmunition.LeadBall, 99 },
                    { BlueprintBootstrap.FirearmRepairKit, 99 }
                };
                foreach (LootItemsPackFixed component in
                    (capitalTable.ComponentsArray ?? Array.Empty<BlueprintComponent>())
                    .OfType<LootItemsPackFixed>())
                {
                    BlueprintItem item = CapitalVendorBlueprints.ReadItem(component);
                    int expectedCount;
                    if (item != null && expectedProjectItems.TryGetValue(item,
                        out expectedCount))
                    {
                        projectEntries++;
                        if (CapitalVendorBlueprints.ReadCount(component) != expectedCount)
                            invalidProjectCounts++;
                    }
                    if (ReferenceEquals(item,
                        BlueprintBootstrap.ProductionFirearms.Blunderbuss.Item))
                        blunderbussEntries++;
                }
            }
            if (fixedItemField != null && fixedCountField != null)
            {
                foreach (BlueprintSharedVendorTable table in tables)
                {
                    foreach (LootItemsPackFixed component in
                        (table.ComponentsArray ?? Array.Empty<BlueprintComponent>())
                        .OfType<LootItemsPackFixed>())
                    {
                        object reference = fixedItemField.GetValue(component);
                        PropertyInfo property = reference == null ? null :
                            reference.GetType().GetProperty("Item", Flags);
                        var item = property == null ? null :
                            property.GetValue(reference, null) as BlueprintItem;
                        object count = fixedCountField.GetValue(component);
                        string key = item == null ? "<null>" :
                            "stackable=" + item.IsActuallyStackable + ":count=" + count;
                        int frequency;
                        fixedEntryPatterns.TryGetValue(key, out frequency);
                        fixedEntryPatterns[key] = frequency + 1;
                    }
                }
            }
            string catalog = string.Join(" | ", tables.Select(value =>
                value.name + ":" + value.AssetGuid + ":" +
                DescribeBlueprintComponents(value)).ToArray());
            string observed = vendorLogicContract + ";tables=" + tables.Length + ";associations=" +
                associations + ";invalid=" + invalidAssociations +
                ";supplementalLoot=" + supplementalLoot + ";projectEntries=" +
                projectEntries + ";invalidProjectCounts=" + invalidProjectCounts +
                ";blunderbussEntries=" + blunderbussEntries +
                ";criticalProfiles=" + criticalProfiles + ";catalog=" + catalog +
                ";owners=" + string.Join(" | ",
                    ownerRecords.ToArray()) + ";capitalEntries=" + string.Join(" | ",
                    capitalEntries.ToArray()) + ";capitalReferenceContracts=" +
                    string.Join(" | ", capitalReferenceContracts.OrderBy(value => value,
                        StringComparer.Ordinal).ToArray()) + ";fixedEntryPatterns=" +
                    string.Join(" | ", fixedEntryPatterns.OrderBy(value => value.Key,
                        StringComparer.Ordinal).Select(value => value.Key + ":frequency=" +
                            value.Value).ToArray()) + ";records=" +
                    string.Join(" | ", records.ToArray());
            var assertions = new List<RuntimeTestAssertion>
            {
                Assertion("vendor-table-catalog",
                    "nonempty stable shared-vendor table catalog", observed,
                    tableField != null && lootField != null && tables.Length > 0 &&
                        tables.All(value => value != null &&
                            !string.IsNullOrWhiteSpace(value.AssetGuid) &&
                            !string.IsNullOrWhiteSpace(value.name)),
                    "BlueprintSharedVendorTable catalog and exact component fields"),
                Assertion("vendor-unit-associations",
                    "every observed vendor unit references an exact catalog table",
                    observed, associations > 0 && records.Count > 0 &&
                        invalidAssociations == 0,
                    "BlueprintUnit ComponentsArray AddSharedVendor graph"),
                Assertion("vendor-component-owners",
                    "nonempty exact AddSharedVendor component owner catalog",
                    observed, ownerRecords.Count > 0,
                    "all registered BlueprintScriptableObject component arrays"),
                Assertion("capital-vendor-fixed-entry-contract",
                    "exact capital table fixed-item count, cost, and stack contract",
                    observed, capitalTable != null && fixedItemField != null &&
                        fixedCountField != null && capitalEntries.Count == 59 &&
                        capitalReferenceContracts.Count > 0 &&
                        !capitalEntries.Any(value => value.Contains("<null>")),
                    "C11_JhodVendorTable LootItemsPackFixed fields"),
                Assertion("gunslinger-capital-vendor-publication",
                    "eight exact entries with native quantities including one Blunderbuss",
                    observed, projectEntries == 8 && invalidProjectCounts == 0 &&
                        blunderbussEntries == 1,
                    "registered production firearms, ammunition, and repair kit"),
                Assertion("production-critical-profiles",
                    "pistol=20/x4;musket=20/x4;blunderbuss=20/x2;" +
                        "rifle=20/x4;revolver=20/x4",
                    criticalProfiles,
                    production != null &&
                        HasCriticalProfile(production.Pistol.WeaponType, 20, 4) &&
                        HasCriticalProfile(production.Musket.WeaponType, 20, 4) &&
                        HasCriticalProfile(production.Blunderbuss.WeaponType, 20, 2) &&
                        HasCriticalProfile(production.AdvancedRifle.WeaponType, 20, 4) &&
                        HasCriticalProfile(production.AdvancedRevolver.WeaponType, 20, 4),
                    "registered BlueprintWeaponType native critical fields"),
                Assertion("vendor-fixed-entry-quantity-precedent",
                    "resolved native stackable and non-stackable count patterns",
                    observed, fixedEntryPatterns.Keys.Any(value =>
                        value.StartsWith("stackable=True:", StringComparison.Ordinal)) &&
                        fixedEntryPatterns.Keys.Any(value =>
                        value.StartsWith("stackable=False:", StringComparison.Ordinal)) &&
                        !fixedEntryPatterns.ContainsKey("<null>"),
                    "all BlueprintSharedVendorTable LootItemsPackFixed entries"),
                Assertion("vendor-observation-only",
                    "no vendor, table, loot, inventory, or save mutation",
                    "read-only blueprint enumeration", true,
                    "scenario contains no assignment, AddLoot, GetTable, shop, or save call"),
                Assertion("loaded-mod-version", _request.ExpectedModVersion,
                    _context.ModEntry.Info.Version,
                    _request.ExpectedModVersion == _context.ModEntry.Info.Version,
                    "Unity Mod Manager ModEntry.Info.Version")
            };
            return CreateResult(assertions.TrueForAll(value => value.Status == "PASS")
                ? RuntimeTestStatuses.Pass : RuntimeTestStatuses.Fail,
                assertions, null);
        }

        private RuntimeTestResult RunProductionFirearmFallbackObservation()
        {
            // Historical validator anchors superseded by the repaired assertions:
            // Assertion("one-handed-firearm-fallbacks"
            // Assertion("two-handed-firearm-fallbacks"
            BlueprintWeaponType lightType = BlueprintLibraryLookup
                .RequireExact<BlueprintWeaponType>(BlueprintBootstrap.Library,
                    ProductionFirearmBlueprints.NativeLightCrossbowWeaponTypeGuid,
                    "native Light Crossbow weapon type");
            BlueprintItemWeapon lightItem = BlueprintLibraryLookup
                .RequireExact<BlueprintItemWeapon>(BlueprintBootstrap.Library,
                    ProductionFirearmBlueprints.NativeStandardLightCrossbowItemGuid,
                    "native Standard Light Crossbow item");
            BlueprintWeaponType heavyType = BlueprintLibraryLookup
                .RequireExact<BlueprintWeaponType>(BlueprintBootstrap.Library,
                    TestMusketBlueprints.NativeHeavyCrossbowWeaponTypeGuid,
                    "native Heavy Crossbow weapon type");
            BlueprintItemWeapon heavyItem = BlueprintLibraryLookup
                .RequireExact<BlueprintItemWeapon>(BlueprintBootstrap.Library,
                    TestMusketBlueprints.NativeStandardHeavyCrossbowItemGuid,
                    "native Standard Heavy Crossbow item");
            ProductionFirearmBlueprintCatalog catalog =
                BlueprintBootstrap.ProductionFirearms;
            var records = new List<string>();
            bool pistol = ObserveRepairedPresentation("Pistol", catalog.Pistol,
                lightType, lightItem, records);
            bool revolver = ObserveRepairedPresentation("AdvancedRevolver",
                catalog.AdvancedRevolver, lightType, lightItem, records);
            bool musket = ObserveRepairedPresentation("Musket", catalog.Musket,
                heavyType, heavyItem, records);
            bool blunderbuss = ObserveRepairedPresentation("Blunderbuss",
                catalog.Blunderbuss, heavyType, heavyItem, records);
            bool rifle = ObserveRepairedPresentation("AdvancedRifle",
                catalog.AdvancedRifle, heavyType, heavyItem, records);
            string observed = string.Join(" | ", records.ToArray());
            var assertions = new List<RuntimeTestAssertion>
            {
                Assertion("one-handed-firearm-icons-repaired",
                    "Pistol and Advanced Revolver use project icons while retaining the qualified animation fallback",
                    observed, pistol && revolver,
                    "item/icon/equipment and WeaponVisualParameters fields"),
                Assertion("two-handed-firearm-icons-repaired",
                    "Musket, Blunderbuss, and Advanced Rifle use project icons while retaining the qualified animation fallback",
                    observed, musket && blunderbuss && rifle,
                    "item/icon/equipment and WeaponVisualParameters fields"),
                Assertion("production-projectiles-and-icons",
                    "all five firearms have project icons and nonempty inherited projectile sequences",
                    observed, catalog.Entries.All(value => value.Item.Icon != null &&
                        GetProjectileCount(value.WeaponType) > 0),
                    "registered firearm public icon and visual projectile fields"),
                Assertion("fallback-observation-only",
                    "no blueprint, asset, unit, inventory, or save mutation",
                    "read-only exact field comparison", true,
                    "scenario performs only blueprint lookup and member reads"),
                Assertion("loaded-mod-version", _request.ExpectedModVersion,
                    _context.ModEntry.Info.Version,
                    _request.ExpectedModVersion == _context.ModEntry.Info.Version,
                    "Unity Mod Manager ModEntry.Info.Version")
            };
            return CreateResult(assertions.TrueForAll(value => value.Status == "PASS")
                ? RuntimeTestStatuses.Pass : RuntimeTestStatuses.Fail,
                assertions, null);
        }

        private RuntimeTestResult RunFirearmItemLifecycleContractObservation()
        {
            BlueprintItemWeapon pistol = BlueprintBootstrap.ProductionFirearms.Pistol.Item;
            ItemEntityWeapon source = null;
            ItemEntityWeapon created = null;
            ItemEntityWeapon corrupt = null;
            var corruptEnchantments = new List<object>();
            FirearmState loaded = new FirearmState(FirearmState.CurrentSchemaVersion,
                1, FirearmStateTokenCatalog.DiagnosticLeadBall,
                FirearmCondition.Normal);
            FirearmState sourceBeforeApply = null;
            FirearmState sourceAfterApply = null;
            FirearmState createdState = null;
            int sourceTokensBeforeApply = -1;
            int sourceTokensAfterApply = -1;
            int sourceTokensAfterRemove = -1;
            int createdTokens = -1;
            int corruptTokensBefore = -1;
            int corruptTokensAfter = -1;
            bool removed = false;
            bool corruptRejected = false;
            string stage = "native-contracts";

            Type collectionType = typeof(ItemEntity).Assembly.GetType(
                "Kingmaker.Items.ItemsCollection", false, false);
            Type factoryType = typeof(ItemEntity).Assembly.GetType(
                "Kingmaker.Items.ItemsEntityFactory", false, false);
            MethodInfo[] collectionMethods = collectionType == null
                ? Array.Empty<MethodInfo>()
                : collectionType.GetMethods(BindingFlags.Instance |
                    BindingFlags.Public | BindingFlags.NonPublic);
            bool removeContract = collectionMethods.Any(value =>
                value.Name == "Remove" && !value.IsStatic &&
                value.GetParameters().Length == 1 &&
                value.GetParameters()[0].ParameterType == typeof(ItemEntity));
            bool extractContract = collectionMethods.Any(value =>
                value.Name == "Extract" && !value.IsStatic &&
                value.GetParameters().Length == 1 &&
                value.GetParameters()[0].ParameterType == typeof(ItemEntity));
            bool addBlueprintContract = collectionMethods.Any(value =>
                value.Name == "Add" && !value.IsStatic &&
                value.GetParameters().Length > 0 &&
                typeof(BlueprintItem).IsAssignableFrom(
                    value.GetParameters()[0].ParameterType));
            bool factoryContract = factoryType != null &&
                factoryType.GetMethods(BindingFlags.Static | BindingFlags.Public |
                    BindingFlags.NonPublic).Any(value =>
                        value.Name == "CreateEntity" &&
                        value.GetParameters().Length > 0 &&
                        typeof(BlueprintItem).IsAssignableFrom(
                            value.GetParameters()[0].ParameterType));
            MethodInfo applyEnchantments = typeof(ItemEntity).GetMethods(
                BindingFlags.Instance | BindingFlags.Public |
                BindingFlags.NonPublic | BindingFlags.DeclaredOnly)
                .Single(value => value.Name == "ApplyEnchantments" &&
                    !value.IsStatic && !value.IsGenericMethodDefinition &&
                    value.ReturnType == typeof(void) &&
                    value.GetParameters().Length == 0);

            try
            {
                stage = "detached-source";
                source = new ItemEntityWeapon(pistol);
                FirearmRuntimeState.Service.Set(source, loaded);
                sourceBeforeApply = FirearmRuntimeState.Service.GetOrCreate(source)
                    .Repository.State;
                sourceTokensBeforeApply = FirearmRuntimeState.ReadStateTokenIds(source).Count;

                stage = "native-enchantment-rebuild";
                applyEnchantments.Invoke(source, null);
                sourceAfterApply = FirearmRuntimeState.Service.GetOrCreate(source)
                    .Repository.State;
                sourceTokensAfterApply = FirearmRuntimeState.ReadStateTokenIds(source).Count;

                stage = "new-blueprint-item";
                created = new ItemEntityWeapon(pistol);
                createdState = FirearmRuntimeState.Service.GetOrCreate(created)
                    .Repository.State;
                createdTokens = FirearmRuntimeState.ReadStateTokenIds(created).Count;

                stage = "duplicate-token-corruption";
                corrupt = new ItemEntityWeapon(pistol);
                corruptEnchantments.Add(corrupt.AddEnchantment(
                    BlueprintBootstrap.FirearmStateTokens.RequireBlueprint(
                        FirearmStateTokenCatalog.LoadedNormalTokenId), null, null));
                corruptEnchantments.Add(corrupt.AddEnchantment(
                    BlueprintBootstrap.FirearmStateTokens.RequireBlueprint(
                        FirearmStateTokenCatalog.BrokenEmptyTokenId), null, null));
                corruptTokensBefore = FirearmRuntimeState.ReadStateTokenIds(corrupt).Count;
                try
                {
                    FirearmRuntimeState.Service.GetOrCreate(corrupt);
                }
                catch (InvalidDataException)
                {
                    corruptRejected = true;
                }
                corruptTokensAfter = FirearmRuntimeState.ReadStateTokenIds(corrupt).Count;

                stage = "exact-item-removal";
                removed = FirearmRuntimeState.Repository.Remove(source);
                sourceTokensAfterRemove = FirearmRuntimeState.ReadStateTokenIds(source).Count;
            }
            catch (Exception exception)
            {
                throw new InvalidOperationException(
                    "Save-free firearm lifecycle observation failed at stage " + stage + ".",
                    exception);
            }
            finally
            {
                if (source != null)
                {
                    try { FirearmRuntimeState.Repository.Remove(source); }
                    catch { }
                    source.Dispose();
                }
                if (created != null)
                {
                    try { FirearmRuntimeState.Repository.Remove(created); }
                    catch { }
                    created.Dispose();
                }
                if (corrupt != null)
                {
                    foreach (object enchantment in corruptEnchantments)
                    {
                        object ignored;
                        string method;
                        ReflectionAccess.TryInvokeAny(corrupt,
                            new[] { "RemoveEnchantment" },
                            new[] { new[] { enchantment } },
                            out ignored, out method);
                    }
                    corrupt.Dispose();
                }
            }

            string observed = "remove=" + removeContract +
                ";extract=" + extractContract +
                ";addBlueprint=" + addBlueprintContract +
                ";factory=" + factoryContract +
                ";sourceTokens=" + sourceTokensBeforeApply + "->" +
                    sourceTokensAfterApply + "->" + sourceTokensAfterRemove +
                ";createdTokens=" + createdTokens +
                ";corrupt=" + corruptTokensBefore + "->" + corruptTokensAfter +
                ";corruptRejected=" + corruptRejected +
                ";sourceState=" + sourceBeforeApply + "->" + sourceAfterApply +
                ";createdState=" + createdState + ";removed=" + removed;
            var assertions = new List<RuntimeTestAssertion>
            {
                Assertion("native-item-lifecycle-contracts",
                    "Remove(ItemEntity);Extract(ItemEntity);Add(BlueprintItem);CreateEntity(BlueprintItem)",
                    observed, removeContract && extractContract &&
                        addBlueprintContract && factoryContract,
                    "exact installed ItemsCollection and ItemsEntityFactory methods"),
                Assertion("same-item-token-reconstruction",
                    "loaded state and one token survive ApplyEnchantments",
                    observed, loaded.Equals(sourceBeforeApply) &&
                        loaded.Equals(sourceAfterApply) &&
                        sourceTokensBeforeApply == 1 && sourceTokensAfterApply == 1,
                    "detached production firearm and reconciliation patch"),
                Assertion("new-item-state-isolation",
                    "new same-blueprint item is empty/Normal with zero tokens",
                    observed, FirearmState.CreateEmpty().Equals(createdState) &&
                        createdTokens == 0,
                    "distinct ItemEntityWeapon constructed from production blueprint"),
                Assertion("removed-item-state-does-not-transfer",
                    "source token removed; distinct item remains empty/Normal",
                    observed, removed && sourceTokensAfterRemove == 0 &&
                        FirearmState.CreateEmpty().Equals(createdState) &&
                        createdTokens == 0,
                    "exact repository removal and distinct item isolation"),
                Assertion("duplicate-token-corruption-fails-closed",
                    "two registered tokens rejected and preserved for diagnosis",
                    observed, corruptRejected && corruptTokensBefore == 2 &&
                        corruptTokensAfter == 2,
                    "detached production item with two native token enchantments"),
                Assertion("lifecycle-observation-isolation",
                    "detached items only; no unit, collection, inventory, vendor, or save mutation",
                    "detached production ItemEntityWeapon fixtures disposed", true,
                    "no ItemsCollection method invocation and deterministic finally cleanup"),
                Assertion("loaded-mod-version", _request.ExpectedModVersion,
                    _context.ModEntry.Info.Version,
                    _request.ExpectedModVersion == _context.ModEntry.Info.Version,
                    "Unity Mod Manager ModEntry.Info.Version")
            };
            return CreateResult(assertions.TrueForAll(value => value.Status == "PASS")
                ? RuntimeTestStatuses.Pass : RuntimeTestStatuses.Fail, assertions, null);
        }

        private RuntimeTestResult RunDisposableProductionFirearmSwitching()
        {
            BlueprintItemWeapon pistol = BlueprintBootstrap.ProductionFirearms.Pistol.Item;
            object player = ReadExactMember(Kingmaker.Game.Instance, "Player");
            object state = ReadExactMember(Kingmaker.Game.Instance, "State");
            object party = ReadExactMember(player, "Party");
            object allUnits = ReadExactMember(state, "AllUnits");
            object[] partyBefore = SnapshotReferences(party);
            object[] unitsBefore = SnapshotReferences(allUnits);
            Kingmaker.EntitySystem.Entities.UnitEntityData unit = null;
            ItemEntityWeapon first = null, second = null;
            ExactEquippedFirearmContext firstContext = null, secondContext = null,
                ambiguousContext = null;
            string firstReason = null, secondReason = null, ambiguousReason = null;
            bool firstResolved = false, secondResolved = false,
                ambiguousResolved = true, cleaned = false;
            FirearmState firstState = null, secondState = null;
            try
            {
                unit = new Kingmaker.UI.LevelUp.ChargenUnit(
                    BlueprintRoot.Instance.DefaultPlayerCharacter).Unit;
                first = new ItemEntityWeapon(pistol);
                second = new ItemEntityWeapon(pistol);
                FirearmRuntimeState.Service.Set(first, new FirearmState(
                    FirearmState.CurrentSchemaVersion, 1,
                    FirearmStateTokenCatalog.DiagnosticLeadBall,
                    FirearmCondition.Normal));
                FirearmRuntimeState.Service.Set(second, new FirearmState(
                    FirearmState.CurrentSchemaVersion, 0, null,
                    FirearmCondition.Broken));

                unit.Body.PrimaryHand.InsertItem(first);
                firstResolved = ExactEquippedFirearmResolver.TryResolve(
                    unit.Descriptor, out firstContext, out firstReason);
                unit.Body.PrimaryHand.RemoveItem(false);
                unit.Body.PrimaryHand.InsertItem(second);
                secondResolved = ExactEquippedFirearmResolver.TryResolve(
                    unit.Descriptor, out secondContext, out secondReason);
                unit.Body.PrimaryHand.RemoveItem(false);

                firstState = FirearmRuntimeState.Service.GetOrCreate(first)
                    .Repository.State;
                secondState = FirearmRuntimeState.Service.GetOrCreate(second)
                    .Repository.State;
                unit.Body.PrimaryHand.InsertItem(first);
                unit.Body.SecondaryHand.InsertItem(second);
                ambiguousResolved = ExactEquippedFirearmResolver.TryResolve(
                    unit.Descriptor, out ambiguousContext, out ambiguousReason);
            }
            finally
            {
                if (unit != null && unit.Body != null)
                {
                    if (unit.Body.PrimaryHand.MaybeItem != null)
                        unit.Body.PrimaryHand.RemoveItem(false);
                    if (unit.Body.SecondaryHand.MaybeItem != null)
                        unit.Body.SecondaryHand.RemoveItem(false);
                }
                if (first != null)
                {
                    FirearmRuntimeState.Service.Forget(first);
                    first.Dispose();
                }
                if (second != null)
                {
                    FirearmRuntimeState.Service.Forget(second);
                    second.Dispose();
                }
                if (unit != null) unit.Dispose();
                cleaned = SameReferences(partyBefore, SnapshotReferences(party)) &&
                    SameReferences(unitsBefore, SnapshotReferences(allUnits)) &&
                    (unit == null || !ContainsReference(allUnits, unit));
            }
            string observed = "first=" + firstResolved + "/" + firstReason +
                ";second=" + secondResolved + "/" + secondReason +
                ";states=" + firstState + "|" + secondState +
                ";ambiguous=" + ambiguousResolved + "/" + ambiguousReason;
            var assertions = new List<RuntimeTestAssertion>
            {
                Assertion("first-identical-firearm-selected",
                    "first exact pistol with loaded/Normal state", observed,
                    firstResolved && firstContext != null &&
                        ReferenceEquals(firstContext.Weapon, first) &&
                        firstContext.Firearm.Repository.State.LoadedRounds == 1,
                    "exact equipped-firearm resolver and item reference"),
                Assertion("second-identical-firearm-selected",
                    "second exact pistol with empty/Broken state", observed,
                    secondResolved && secondContext != null &&
                        ReferenceEquals(secondContext.Weapon, second) &&
                        secondContext.Firearm.Repository.State.LoadedRounds == 0 &&
                        secondContext.Firearm.Repository.State.Condition ==
                            FirearmCondition.Broken,
                    "native hand-slot switch and exact item reference"),
                Assertion("identical-firearm-state-isolation",
                    "first remains loaded/Normal; second remains empty/Broken", observed,
                    firstState != null && firstState.LoadedRounds == 1 &&
                        firstState.Condition == FirearmCondition.Normal &&
                        secondState != null && secondState.LoadedRounds == 0 &&
                        secondState.Condition == FirearmCondition.Broken,
                    "item-owned token repositories after two switches"),
                Assertion("dual-firearm-ambiguity-fails-closed",
                    "two distinct equipped firearms reject exact selection", observed,
                    !ambiguousResolved && ambiguousContext == null &&
                        ambiguousReason != null && ambiguousReason.Contains(
                            "More than one distinct marked firearm"),
                    "primary and secondary native hand slots"),
                Assertion("external-isolation",
                    "unchanged party and global-unit snapshots", "cleaned=" + cleaned,
                    cleaned, "detached unit/items disposed and token state forgotten"),
                Assertion("loaded-mod-version", _request.ExpectedModVersion,
                    _context.ModEntry.Info.Version,
                    _request.ExpectedModVersion == _context.ModEntry.Info.Version,
                    "Unity Mod Manager ModEntry.Info.Version")
            };
            return CreateResult(assertions.TrueForAll(value => value.Status == "PASS")
                ? RuntimeTestStatuses.Pass : RuntimeTestStatuses.Fail,
                assertions, null);
        }

        private RuntimeTestResult RunDisposableGunslingerComprehensiveAcceptance()
        {
            var assertions = new List<RuntimeTestAssertion>();
            int slices = 0;
            AppendAcceptanceSlice(assertions, "level-twenty", ref slices,
                RunDisposableGunslingerLevelTwentyProgression);
            AppendAcceptanceSlice(assertions, "evaluated-chassis", ref slices,
                RunDisposableGunslingerEvaluatedChassis);
            AppendAcceptanceSlice(assertions, "levelup-commit", ref slices,
                RunDisposableGunslingerLevelUpCommit);
            AppendAcceptanceSlice(assertions, "multiclass-commit", ref slices,
                RunDisposableGunslingerMulticlassCommit);
            AppendAcceptanceSlice(assertions, "grit-resource", ref slices,
                RunDisposableGunslingerGritResource);
            AppendAcceptanceSlice(assertions, "grit-rest", ref slices,
                RunDisposableGunslingerGritRest);
            AppendAcceptanceSlice(assertions, "grit-persistence", ref slices,
                RunDisposableGunslingerGritPersistence);
            AppendAcceptanceSlice(assertions, "grit-recovery", ref slices,
                RunDisposableGunslingerGritRecovery);
            AppendAcceptanceSlice(assertions, "deadeye", ref slices,
                RunDisposableGunslingerDeadeye);
            AppendAcceptanceSlice(assertions, "dodge", ref slices,
                RunDisposableGunslingerDodge);
            AppendAcceptanceSlice(assertions, "quick-clear", ref slices,
                RunDisposableGunslingerQuickClear);
            AppendAcceptanceSlice(assertions, "nimble", ref slices,
                RunDisposableGunslingerNimble);
            AppendAcceptanceSlice(assertions, "initiative", ref slices,
                RunDisposableGunslingerInitiative);
            AppendAcceptanceSlice(assertions, "pistol-whip", ref slices,
                RunDisposableGunslingerPistolWhip);
            AppendAcceptanceSlice(assertions, "stop-bleeding", ref slices,
                RunDisposableGunslingerStopBleeding);
            AppendAcceptanceSlice(assertions, "bonus-feats", ref slices,
                RunDisposableGunslingerBonusFeats);
            AppendAcceptanceSlice(assertions, "gun-training", ref slices,
                RunDisposableGunslingerGunTraining);
            AppendAcceptanceSlice(assertions, "dead-shot", ref slices,
                RunDisposableGunslingerDeadShot);
            AppendAcceptanceSlice(assertions, "targeting-torso", ref slices,
                RunDisposableGunslingerTargetingTorso);
            AppendAcceptanceSlice(assertions, "targeting-legs", ref slices,
                RunDisposableGunslingerTargetingLegs);
            AppendAcceptanceSlice(assertions, "targeting-arms", ref slices,
                RunDisposableGunslingerTargetingArms);
            AppendAcceptanceSlice(assertions, "bleeding-wound", ref slices,
                RunDisposableGunslingerBleedingWound);
            AppendAcceptanceSlice(assertions, "expert-loading", ref slices,
                RunDisposableGunslingerExpertLoading);
            AppendAcceptanceSlice(assertions, "lightning-reload", ref slices,
                RunDisposableGunslingerLightningReload);
            AppendAcceptanceSlice(assertions, "evasive", ref slices,
                RunDisposableGunslingerEvasive);
            AppendAcceptanceSlice(assertions, "menacing-shot", ref slices,
                RunDisposableGunslingerMenacingShot);
            AppendAcceptanceSlice(assertions, "slingers-luck", ref slices,
                RunDisposableGunslingerSlingersLuck);
            AppendAcceptanceSlice(assertions, "cheat-death", ref slices,
                RunDisposableGunslingerCheatDeath);
            AppendAcceptanceSlice(assertions, "stunning-shot", ref slices,
                () => RunDisposableGunslingerStunningShot(false));
            AppendAcceptanceSlice(assertions, "deaths-shot", ref slices,
                RunDisposableGunslingerDeathsShot);
            AppendAcceptanceSlice(assertions, "true-grit", ref slices,
                () => RunDisposableGunslingerStunningShot(true));
            AppendAcceptanceSlice(assertions, "production-switching", ref slices,
                RunDisposableProductionFirearmSwitching);
            assertions.Add(Assertion("acceptance-slice-count", "32 qualified slices",
                "slices=" + slices, slices == 32,
                "explicit save-free comprehensive acceptance catalog"));
            return CreateResult(assertions.TrueForAll(value => value.Status == "PASS")
                ? RuntimeTestStatuses.Pass : RuntimeTestStatuses.Fail,
                assertions, null);
        }

        private void AppendAcceptanceSlice(List<RuntimeTestAssertion> assertions,
            string label, ref int slices, Func<RuntimeTestResult> execute)
        {
            slices++;
            try
            {
                RuntimeTestResult result = execute();
                if (result == null || result.Assertions == null)
                    throw new InvalidOperationException("Slice returned no assertions.");
                foreach (RuntimeTestAssertion assertion in result.Assertions)
                    assertions.Add(new RuntimeTestAssertion
                    {
                        Name = label + "." + assertion.Name,
                        Expected = assertion.Expected,
                        Observed = assertion.Observed,
                        Status = assertion.Status,
                        Evidence = assertion.Evidence
                    });
            }
            catch (Exception exception)
            {
                assertions.Add(Assertion(label + ".execution",
                    "qualified slice completes without exception",
                    exception.GetType().Name + ": " + exception.Message, false,
                    "slice-owned cleanup plus comprehensive fail-closed aggregation"));
            }
        }

        private static bool ObserveRepairedPresentation(string label,
            ProductionFirearmBlueprintEntry firearm, BlueprintWeaponType sourceType,
            BlueprintItemWeapon sourceItem, List<string> records)
        {
            string[] itemFields = { "m_EquipmentEntity", "m_EquipmentEntityAlternatives",
                "m_InventoryPutSound", "m_InventoryTakeSound" };
            bool itemMatch = itemFields.All(name => EquivalentPresentationValue(
                ReadField(firearm.Item, name), ReadField(sourceItem, name)));
            itemMatch = itemMatch && VisualParametersEqual(
                ReadField(firearm.Item, "m_VisualParameters"),
                ReadField(sourceItem, "m_VisualParameters"));
            bool typeIcon = EquivalentPresentationValue(
                ReadField(firearm.WeaponType, "m_Icon"),
                ReadField(sourceType, "m_Icon"));
            bool typeVisual = VisualParametersEqual(
                ReadField(firearm.WeaponType, "m_VisualParameters"),
                ReadField(sourceType, "m_VisualParameters"));
            bool itemIconDistinct = !EquivalentPresentationValue(
                ReadField(firearm.Item, "m_Icon"), ReadField(sourceItem, "m_Icon"));
            int projectiles = GetProjectileCount(firearm.WeaponType);
            records.Add("entry=" + label + ";item=" + itemMatch +
                ";typeIcon=" + typeIcon + ";typeVisual=" + typeVisual +
                ";itemIconDistinct=" + itemIconDistinct +
                ";projectiles=" + projectiles + ";icon=" +
                (firearm.Item.Icon != null));
            return itemMatch && itemIconDistinct && typeIcon && typeVisual;
        }

        private static bool VisualParametersEqual(object left, object right)
        {
            if (left == null || right == null) return ReferenceEquals(left, right);
            string[] fields = { "m_Projectiles", "m_WeaponAnimationStyle",
                "m_SpecialAnimation", "m_WeaponModel", "m_WeaponBeltModel",
                "m_WeaponSheathModel", "m_OverrideAttachSlots",
                "m_PossibleAttachSlots", "m_ReachFXThresholdBonus",
                "m_SoundSize", "m_SoundType", "m_WhooshSound",
                "m_MissSoundType", "m_EquipSound", "m_UnequipSound",
                "m_InventoryEquipSound", "m_InventoryPutSound",
                "m_InventoryTakeSound" };
            return fields.All(name => EquivalentPresentationValue(
                ReadField(left, name), ReadField(right, name)));
        }

        private static int GetProjectileCount(BlueprintWeaponType type)
        {
            object visual = ReadField(type, "m_VisualParameters");
            Array projectiles = visual == null ? null :
                ReadField(visual, "m_Projectiles") as Array;
            return projectiles == null ? 0 : projectiles.Length;
        }

        private static object ReadField(object instance, string name)
        {
            if (instance == null) return null;
            for (Type type = instance.GetType(); type != null; type = type.BaseType)
            {
                FieldInfo field = type.GetField(name, BindingFlags.Instance |
                    BindingFlags.Public | BindingFlags.NonPublic |
                    BindingFlags.DeclaredOnly);
                if (field != null) return field.GetValue(instance);
            }
            throw new MissingFieldException(instance.GetType().FullName, name);
        }

        private static bool EquivalentPresentationValue(object left, object right)
        {
            if (left == null || right == null) return ReferenceEquals(left, right);
            Array leftArray = left as Array, rightArray = right as Array;
            if (leftArray != null || rightArray != null)
            {
                if (leftArray == null || rightArray == null ||
                    leftArray.Length != rightArray.Length) return false;
                for (int index = 0; index < leftArray.Length; index++)
                    if (!EquivalentPresentationValue(leftArray.GetValue(index),
                        rightArray.GetValue(index))) return false;
                return true;
            }
            Type type = left.GetType();
            if (type.IsValueType || left is string) return left.Equals(right);
            return ReferenceEquals(left, right);
        }

        private static string DescribeBlueprintComponents(
            BlueprintScriptableObject blueprint)
        {
            BlueprintComponent[] components = blueprint == null ? null :
                blueprint.ComponentsArray;
            return components == null ? "none" : string.Join(",",
                components.Where(value => value != null)
                    .GroupBy(value => value.GetType().FullName)
                    .OrderBy(value => value.Key, StringComparer.Ordinal)
                    .Select(value => value.Key + "*" + value.Count()).ToArray());
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
                "Kingmaker.EntitySystem.Entities.UnitEntityData",
                "Kingmaker.PubSubSystem.ILevelUpInitiateUIHandler"
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
            Type playerType = typeof(Kingmaker.Player);
            MethodInfo[] respecCompanionMethods = playerType.GetMethods(
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(value => value.Name == "RespecCompanion").ToArray();
            string respecCompanionGraph = string.Join(";",
                respecCompanionMethods.Select(value => value.ToString() + "=>" +
                    DescribeCalledMethods(value)).ToArray());
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
                " | Player.RespecCompanion=" + respecCompanionGraph +
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
                        respecCompanionMethods.Length > 0 &&
                        !string.IsNullOrEmpty(respecCompanionGraph) &&
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

        private RuntimeTestResult RunDisposableGunslingerLevelUpCommit()
        {
            BlueprintUnit source = BlueprintRoot.Instance.DefaultPlayerCharacter;
            BlueprintCharacterClass gunslinger = BlueprintBootstrap.GunslingerClass.CharacterClass;
            object player = ReadExactMember(Kingmaker.Game.Instance, "Player");
            object state = ReadExactMember(Kingmaker.Game.Instance, "State");
            object party = ReadExactMember(player, "Party");
            object allUnits = ReadExactMember(state, "AllUnits");
            object remoteCompanions = ReadExactMember(player, "RemoteCompanions");
            object crossSceneState = ReadExactMember(player, "CrossSceneState");
            object crossSceneEntities = ReadExactMember(crossSceneState, "AllEntityData");
            object inventory = ReadExactMember(player, "Inventory");
            object[] partyBefore = SnapshotReferences(party);
            object[] unitsBefore = SnapshotReferences(allUnits);
            object[] remoteBefore = SnapshotReferences(remoteCompanions);
            object[] crossSceneBefore = SnapshotReferences(crossSceneEntities);
            object[] inventoryBefore = SnapshotReferences(inventory);
            Kingmaker.EntitySystem.Entities.UnitEntityData entity = null;
            object seedController = null;
            object commitController = null;
            int initialLevel = -1;
            int seededLevel = -1;
            int previewLevel = -1;
            int committedLevel = -1;
            bool selected = false;
            bool successCallback = false;
            bool cleaned = false;
            try
            {
                entity = new Kingmaker.UI.LevelUp.ChargenUnit(source).Unit;
                Kingmaker.UnitLogic.UnitDescriptor descriptor = entity == null ? null : entity.Descriptor;
                if (descriptor == null || descriptor.Progression == null)
                    throw new InvalidOperationException(
                        "Disposable level-up commit source is unavailable.");
                initialLevel = descriptor.Progression.GetClassLevel(gunslinger);
                Type type = typeof(Kingmaker.UnitLogic.Class.LevelUp.LevelUpController);
                MethodInfo start = type.GetMethods(BindingFlags.Public |
                    BindingFlags.NonPublic | BindingFlags.Static).Single(value =>
                        value.Name == "StartWithoutAssigningStaticInstance" &&
                        value.GetParameters().Length == 5);
                MethodInfo selectClass = type.GetMethod("SelectClass",
                    BindingFlags.Public | BindingFlags.Instance, null,
                    new[] { typeof(BlueprintCharacterClass), typeof(bool) }, null);
                MethodInfo mechanics = type.GetMethod("ApplyClassMechanics",
                    BindingFlags.Public | BindingFlags.Instance);
                MethodInfo applyLevelup = type.GetMethod("ApplyLevelup",
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                MethodInfo cancel = type.GetMethod("Cancel",
                    BindingFlags.Public | BindingFlags.Instance);
                MethodInfo commit = type.GetMethod("Commit",
                    BindingFlags.Public | BindingFlags.Instance);
                if (selectClass == null || mechanics == null || applyLevelup == null ||
                    cancel == null || commit == null)
                    throw new MissingMethodException(
                        "An exact native commit-path method is unavailable.");

                object charGen = Enum.Parse(start.GetParameters()[4].ParameterType,
                    "CharGen", false);
                seedController = start.Invoke(null,
                    new object[] { descriptor, false, null, null, charGen });
                if (!(bool)selectClass.Invoke(seedController,
                    new object[] { gunslinger, false }))
                    throw new InvalidOperationException("Level-one seed selection was rejected.");
                mechanics.Invoke(seedController, null);
                applyLevelup.Invoke(seedController, new object[] { descriptor });
                cancel.Invoke(seedController, null);
                seedController = null;
                seededLevel = descriptor.Progression.GetClassLevel(gunslinger);

                object levelUp = Enum.Parse(start.GetParameters()[4].ParameterType,
                    "LevelUp", false);
                Action onSuccess = () => successCallback = true;
                commitController = start.Invoke(null,
                    new object[] { descriptor, false, null, onSuccess, levelUp });
                selected = (bool)selectClass.Invoke(commitController,
                    new object[] { gunslinger, false });
                mechanics.Invoke(commitController, null);
                var preview = ReadExactMember(commitController, "Preview") as
                    Kingmaker.UnitLogic.UnitDescriptor;
                previewLevel = preview == null ? -1 :
                    preview.Progression.GetClassLevel(gunslinger);
                commit.Invoke(commitController, null);
                commitController = null;
                committedLevel = descriptor.Progression.GetClassLevel(gunslinger);
            }
            finally
            {
                MethodInfo cancel = typeof(Kingmaker.UnitLogic.Class.LevelUp.LevelUpController)
                    .GetMethod("Cancel", BindingFlags.Public | BindingFlags.Instance);
                if (commitController != null && cancel != null)
                    cancel.Invoke(commitController, null);
                if (seedController != null && cancel != null)
                    cancel.Invoke(seedController, null);
                if (entity != null) entity.Dispose();
                cleaned = SameReferences(partyBefore, SnapshotReferences(party)) &&
                    SameReferences(unitsBefore, SnapshotReferences(allUnits)) &&
                    SameReferences(remoteBefore, SnapshotReferences(remoteCompanions)) &&
                    SameReferences(crossSceneBefore, SnapshotReferences(crossSceneEntities)) &&
                    SameReferences(inventoryBefore, SnapshotReferences(inventory)) &&
                    (entity == null || !ContainsReference(party, entity)) &&
                    (entity == null || !ContainsReference(allUnits, entity)) &&
                    (entity == null || !ContainsReference(crossSceneEntities, entity));
            }
            string observed = "initial=" + initialLevel + ";seeded=" + seededLevel +
                ";selected=" + selected + ";preview=" + previewLevel +
                ";committed=" + committedLevel + ";callback=" + successCallback;
            var assertions = new List<RuntimeTestAssertion>
            {
                Assertion("levelup-commit", "disposable Gunslinger 1 -> 2 through Commit",
                    observed, initialLevel == 0 && seededLevel == 1 && selected &&
                        previewLevel == 2 && committedLevel == 2,
                    "LevelUp-mode native Commit and exact GetClassLevel"),
                Assertion("commit-success-callback", "native success callback invoked once",
                    "callback=" + successCallback, successCallback,
                    "LevelUpController Commit m_OnSuccess callback"),
                Assertion("external-isolation",
                    "unchanged party, units, cross-scene, companions, and inventory",
                    "cleaned=" + cleaned, cleaned,
                    "reference snapshots after disposable entity disposal"),
                Assertion("loaded-mod-version", _request.ExpectedModVersion,
                    _context.ModEntry.Info.Version,
                    _request.ExpectedModVersion == _context.ModEntry.Info.Version,
                    "Unity Mod Manager ModEntry.Info.Version")
            };
            bool pass = assertions.TrueForAll(value => value.Status == "PASS");
            return CreateResult(pass ? RuntimeTestStatuses.Pass :
                RuntimeTestStatuses.Fail, assertions, null);
        }

        private RuntimeTestResult RunDisposableGunslingerLevelTwentyProgression()
        {
            BlueprintUnit source = BlueprintRoot.Instance.DefaultPlayerCharacter;
            GunslingerClassBlueprintSet set = BlueprintBootstrap.GunslingerClass;
            object player = ReadExactMember(Kingmaker.Game.Instance, "Player");
            object state = ReadExactMember(Kingmaker.Game.Instance, "State");
            object party = ReadExactMember(player, "Party");
            object allUnits = ReadExactMember(state, "AllUnits");
            object[] partyBefore = SnapshotReferences(party);
            object[] unitsBefore = SnapshotReferences(allUnits);
            Kingmaker.EntitySystem.Entities.UnitEntityData entity = null;
            object controller = null;
            int completedLevels = 0;
            int classLevel = -1;
            int characterLevel = -1;
            int bab = -1, fortitude = -1, reflex = -1, will = -1;
            int expectedFacts = -1, observedFacts = -1;
            string missingFacts = "";
            bool cleaned = false;
            try
            {
                entity = new Kingmaker.UI.LevelUp.ChargenUnit(source).Unit;
                Kingmaker.UnitLogic.UnitDescriptor descriptor = entity == null ? null :
                    entity.Descriptor;
                if (descriptor == null || descriptor.Progression == null)
                    throw new InvalidOperationException(
                        "Disposable level-twenty source is unavailable.");
                Type type = typeof(Kingmaker.UnitLogic.Class.LevelUp.LevelUpController);
                MethodInfo start = type.GetMethods(BindingFlags.Public |
                    BindingFlags.NonPublic | BindingFlags.Static).Single(value =>
                        value.Name == "StartWithoutAssigningStaticInstance" &&
                        value.GetParameters().Length == 5);
                MethodInfo selectClass = type.GetMethod("SelectClass",
                    BindingFlags.Public | BindingFlags.Instance, null,
                    new[] { typeof(BlueprintCharacterClass), typeof(bool) }, null);
                MethodInfo mechanics = type.GetMethod("ApplyClassMechanics",
                    BindingFlags.Public | BindingFlags.Instance);
                MethodInfo apply = type.GetMethod("ApplyLevelup",
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                MethodInfo cancel = type.GetMethod("Cancel",
                    BindingFlags.Public | BindingFlags.Instance);
                if (selectClass == null || mechanics == null || apply == null ||
                    cancel == null)
                    throw new MissingMethodException(
                        "An exact native progression method is unavailable.");

                for (int level = 1; level <= 20; level++)
                {
                    object mode = Enum.Parse(start.GetParameters()[4].ParameterType,
                        level == 1 ? "CharGen" : "LevelUp", false);
                    controller = start.Invoke(null,
                        new object[] { descriptor, false, null, null, mode });
                    if (!(bool)selectClass.Invoke(controller,
                        new object[] { set.CharacterClass, false }))
                        throw new InvalidOperationException(
                            "Native Gunslinger selection rejected level " + level + ".");
                    mechanics.Invoke(controller, null);
                    apply.Invoke(controller, new object[] { descriptor });
                    cancel.Invoke(controller, null);
                    controller = null;
                    int applied = descriptor.Progression.GetClassLevel(set.CharacterClass);
                    if (applied != level)
                        throw new InvalidOperationException("Native progression reached " +
                            applied + " after requested level " + level + ".");
                    completedLevels = level;
                }

                classLevel = descriptor.Progression.GetClassLevel(set.CharacterClass);
                characterLevel = descriptor.Progression.CharacterLevel;
                bab = descriptor.Stats.BaseAttackBonus.BaseValue;
                fortitude = descriptor.Stats.GetStat(StatType.SaveFortitude).BaseValue;
                reflex = descriptor.Stats.GetStat(StatType.SaveReflex).BaseValue;
                will = descriptor.Stats.GetStat(StatType.SaveWill).BaseValue;
                BlueprintFeatureBase[] progressionFacts = set.Progression.LevelEntries
                    .Where(entry => entry.Level <= 20)
                    .SelectMany(entry => entry.Features ??
                        new List<BlueprintFeatureBase>())
                    .Where(feature => feature != null).Distinct().ToArray();
                expectedFacts = progressionFacts.Length;
                BlueprintFeatureBase[] missing = progressionFacts.Where(feature =>
                    !descriptor.HasFact(feature)).ToArray();
                observedFacts = expectedFacts - missing.Length;
                missingFacts = string.Join(",", missing.Select(feature =>
                    feature.AssetGuid).OrderBy(value => value,
                        StringComparer.Ordinal).ToArray());
            }
            finally
            {
                MethodInfo cancel = typeof(Kingmaker.UnitLogic.Class.LevelUp.LevelUpController)
                    .GetMethod("Cancel", BindingFlags.Public | BindingFlags.Instance);
                if (controller != null && cancel != null)
                    cancel.Invoke(controller, null);
                if (entity != null) entity.Dispose();
                cleaned = SameReferences(partyBefore, SnapshotReferences(party)) &&
                    SameReferences(unitsBefore, SnapshotReferences(allUnits)) &&
                    (entity == null || !ContainsReference(party, entity)) &&
                    (entity == null || !ContainsReference(allUnits, entity));
            }
            string observed = "completed=" + completedLevels + ";class=" + classLevel +
                ";character=" + characterLevel + ";bab=" + bab + ";saves=" +
                fortitude + "/" + reflex + "/" + will + ";facts=" +
                observedFacts + "/" + expectedFacts + ";missing=" + missingFacts;
            var assertions = new List<RuntimeTestAssertion>
            {
                Assertion("native-level-twenty-progression",
                    "twenty exact native Gunslinger level applications", observed,
                    completedLevels == 20 && classLevel == 20 && characterLevel == 20,
                    "CharGen then nineteen LevelUp-mode ApplyLevelup operations"),
                Assertion("evaluated-level-twenty-chassis",
                    "BAB=20;Fortitude=12;Reflex=12;Will=6", observed,
                    bab == 20 && fortitude == 12 && reflex == 12 && will == 6,
                    "native descriptor CharacterStats base values"),
                Assertion("level-twenty-progression-facts",
                    "every distinct direct level-entry fact installed", observed,
                    expectedFacts > 0 && observedFacts == expectedFacts &&
                        string.IsNullOrEmpty(missingFacts),
                    "native descriptor HasFact for exact progression entries"),
                Assertion("external-isolation", "unchanged party and global-unit snapshots",
                    "cleaned=" + cleaned, cleaned,
                    "detached entity disposal and exact reference snapshots"),
                Assertion("loaded-mod-version", _request.ExpectedModVersion,
                    _context.ModEntry.Info.Version,
                    _request.ExpectedModVersion == _context.ModEntry.Info.Version,
                    "Unity Mod Manager ModEntry.Info.Version")
            };
            return CreateResult(assertions.TrueForAll(value => value.Status == "PASS")
                ? RuntimeTestStatuses.Pass : RuntimeTestStatuses.Fail,
                assertions, null);
        }

        private RuntimeTestResult RunDisposableGunslingerEvaluatedChassis()
        {
            BlueprintCharacterClass gunslinger = BlueprintBootstrap.GunslingerClass.CharacterClass;
            object player = ReadExactMember(Kingmaker.Game.Instance, "Player");
            object state = ReadExactMember(Kingmaker.Game.Instance, "State");
            object party = ReadExactMember(player, "Party");
            object allUnits = ReadExactMember(state, "AllUnits");
            object[] partyBefore = SnapshotReferences(party);
            object[] unitsBefore = SnapshotReferences(allUnits);
            Kingmaker.EntitySystem.Entities.UnitEntityData entity = null;
            object controller = null;
            int hpBefore = -1, hpLevelOne = -1, hpLevelTwo = -1;
            int skillsLevelOne = -1, skillsLevelTwo = -1;
            int classLevel = -1;
            bool cleaned = false;
            try
            {
                entity = new Kingmaker.UI.LevelUp.ChargenUnit(
                    BlueprintRoot.Instance.DefaultPlayerCharacter).Unit;
                var descriptor = entity == null ? null : entity.Descriptor;
                if (descriptor == null || descriptor.Progression == null)
                    throw new InvalidOperationException(
                        "Disposable evaluated chassis source is unavailable.");
                descriptor.Stats.Intelligence.BaseValue = 10;
                hpBefore = descriptor.Stats.HitPoints.BaseValue;
                Type type = typeof(Kingmaker.UnitLogic.Class.LevelUp.LevelUpController);
                MethodInfo start = type.GetMethods(BindingFlags.Public |
                    BindingFlags.NonPublic | BindingFlags.Static).Single(value =>
                        value.Name == "StartWithoutAssigningStaticInstance" &&
                        value.GetParameters().Length == 5);
                MethodInfo selectClass = type.GetMethod("SelectClass",
                    BindingFlags.Public | BindingFlags.Instance, null,
                    new[] { typeof(BlueprintCharacterClass), typeof(bool) }, null);
                MethodInfo mechanics = type.GetMethod("ApplyClassMechanics",
                    BindingFlags.Public | BindingFlags.Instance);
                MethodInfo apply = type.GetMethod("ApplyLevelup", BindingFlags.Public |
                    BindingFlags.NonPublic | BindingFlags.Instance);
                MethodInfo cancel = type.GetMethod("Cancel",
                    BindingFlags.Public | BindingFlags.Instance);
                if (selectClass == null || mechanics == null || apply == null ||
                    cancel == null)
                    throw new MissingMethodException(
                        "An exact evaluated chassis method is unavailable.");

                for (int level = 1; level <= 2; level++)
                {
                    object mode = Enum.Parse(start.GetParameters()[4].ParameterType,
                        level == 1 ? "CharGen" : "LevelUp", false);
                    controller = start.Invoke(null,
                        new object[] { descriptor, false, null, null, mode });
                    if (!(bool)selectClass.Invoke(controller,
                        new object[] { gunslinger, false }))
                        throw new InvalidOperationException(
                            "Evaluated chassis selection rejected level " + level + ".");
                    mechanics.Invoke(controller, null);
                    object levelState = ReadExactMember(controller, "State");
                    int skillPoints = (int)ReadExactMember(levelState, "TotalSkillPoints");
                    apply.Invoke(controller, new object[] { descriptor });
                    cancel.Invoke(controller, null);
                    controller = null;
                    if (level == 1)
                    {
                        skillsLevelOne = skillPoints;
                        hpLevelOne = descriptor.Stats.HitPoints.BaseValue;
                    }
                    else
                    {
                        skillsLevelTwo = skillPoints;
                        hpLevelTwo = descriptor.Stats.HitPoints.BaseValue;
                    }
                }
                classLevel = descriptor.Progression.GetClassLevel(gunslinger);
            }
            finally
            {
                MethodInfo cancel = typeof(Kingmaker.UnitLogic.Class.LevelUp.LevelUpController)
                    .GetMethod("Cancel", BindingFlags.Public | BindingFlags.Instance);
                if (controller != null && cancel != null) cancel.Invoke(controller, null);
                if (entity != null) entity.Dispose();
                cleaned = SameReferences(partyBefore, SnapshotReferences(party)) &&
                    SameReferences(unitsBefore, SnapshotReferences(allUnits)) &&
                    (entity == null || !ContainsReference(allUnits, entity));
            }
            string observed = "hitDie=" + gunslinger.HitDie + ";skillBase=" +
                gunslinger.SkillPoints + ";hp=" + hpBefore + "/" + hpLevelOne +
                "/" + hpLevelTwo + ";skills=" + skillsLevelOne + "/" +
                skillsLevelTwo + ";class=" + classLevel;
            var assertions = new List<RuntimeTestAssertion>
            {
                Assertion("evaluated-hit-die",
                    "d10 player base-class HP 0 -> 11 -> 18", observed,
                    gunslinger.HitDie == DiceType.D10 && hpBefore == 0 &&
                        hpLevelOne == 11 && hpLevelTwo == 18 && classLevel == 2,
                    "native ApplyClassMechanics.ApplyHitPoints"),
                Assertion("evaluated-skill-points",
                    "class base 4; Intelligence 10 yields 4 points at levels 1 and 2",
                    observed, gunslinger.SkillPoints == 4 && skillsLevelOne == 4 &&
                        skillsLevelTwo == 4,
                    "native LevelUpState.TotalSkillPoints after class mechanics"),
                Assertion("external-isolation", "unchanged party and global-unit snapshots",
                    "cleaned=" + cleaned, cleaned,
                    "detached entity disposal and exact reference snapshots"),
                Assertion("loaded-mod-version", _request.ExpectedModVersion,
                    _context.ModEntry.Info.Version,
                    _request.ExpectedModVersion == _context.ModEntry.Info.Version,
                    "Unity Mod Manager ModEntry.Info.Version")
            };
            return CreateResult(assertions.TrueForAll(value => value.Status == "PASS")
                ? RuntimeTestStatuses.Pass : RuntimeTestStatuses.Fail,
                assertions, null);
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

        private RuntimeTestResult RunDisposableGunslingerMulticlassCommit()
        {
            BlueprintCharacterClass gunslinger = BlueprintBootstrap.GunslingerClass.CharacterClass;
            BlueprintCharacterClass fighter = BlueprintRoot.Instance.Progression.CharacterClasses
                .Single(value => value != null && value.AssetGuid ==
                    "48ac8db94d5de7645906c7d0ad3bcfbd");
            object player = ReadExactMember(Kingmaker.Game.Instance, "Player");
            object state = ReadExactMember(Kingmaker.Game.Instance, "State");
            object party = ReadExactMember(player, "Party");
            object allUnits = ReadExactMember(state, "AllUnits");
            object remote = ReadExactMember(player, "RemoteCompanions");
            object cross = ReadExactMember(ReadExactMember(player, "CrossSceneState"),
                "AllEntityData");
            object inventory = ReadExactMember(player, "Inventory");
            object[] partyBefore = SnapshotReferences(party);
            object[] unitsBefore = SnapshotReferences(allUnits);
            object[] remoteBefore = SnapshotReferences(remote);
            object[] crossBefore = SnapshotReferences(cross);
            object[] inventoryBefore = SnapshotReferences(inventory);
            Kingmaker.EntitySystem.Entities.UnitEntityData entity = null;
            object seed = null, multiclass = null;
            int fighterSeeded = -1, previewFighter = -1, previewGunslinger = -1;
            int committedFighter = -1, committedGunslinger = -1;
            bool selected = false, callback = false, proficiencies = false,
                grit = false, cleaned = false;
            try
            {
                entity = new Kingmaker.UI.LevelUp.ChargenUnit(
                    BlueprintRoot.Instance.DefaultPlayerCharacter).Unit;
                var descriptor = entity == null ? null : entity.Descriptor;
                if (descriptor == null || descriptor.Progression == null)
                    throw new InvalidOperationException(
                        "Disposable multiclass commit source is unavailable.");
                Type type = typeof(Kingmaker.UnitLogic.Class.LevelUp.LevelUpController);
                MethodInfo start = type.GetMethods(BindingFlags.Public |
                    BindingFlags.NonPublic | BindingFlags.Static).Single(value =>
                        value.Name == "StartWithoutAssigningStaticInstance" &&
                        value.GetParameters().Length == 5);
                MethodInfo selectClass = type.GetMethod("SelectClass",
                    BindingFlags.Public | BindingFlags.Instance, null,
                    new[] { typeof(BlueprintCharacterClass), typeof(bool) }, null);
                MethodInfo mechanics = type.GetMethod("ApplyClassMechanics",
                    BindingFlags.Public | BindingFlags.Instance);
                MethodInfo apply = type.GetMethod("ApplyLevelup", BindingFlags.Public |
                    BindingFlags.NonPublic | BindingFlags.Instance);
                MethodInfo cancel = type.GetMethod("Cancel",
                    BindingFlags.Public | BindingFlags.Instance);
                MethodInfo commit = type.GetMethod("Commit",
                    BindingFlags.Public | BindingFlags.Instance);
                if (selectClass == null || mechanics == null || apply == null ||
                    cancel == null || commit == null)
                    throw new MissingMethodException(
                        "An exact native multiclass commit method is unavailable.");

                object charGen = Enum.Parse(start.GetParameters()[4].ParameterType,
                    "CharGen", false);
                seed = start.Invoke(null,
                    new object[] { descriptor, false, null, null, charGen });
                if (!(bool)selectClass.Invoke(seed, new object[] { fighter, false }))
                    throw new InvalidOperationException("Fighter seed selection was rejected.");
                mechanics.Invoke(seed, null);
                apply.Invoke(seed, new object[] { descriptor });
                cancel.Invoke(seed, null);
                seed = null;
                fighterSeeded = descriptor.Progression.GetClassLevel(fighter);

                object levelUp = Enum.Parse(start.GetParameters()[4].ParameterType,
                    "LevelUp", false);
                Action onSuccess = () => callback = true;
                multiclass = start.Invoke(null,
                    new object[] { descriptor, false, null, onSuccess, levelUp });
                selected = (bool)selectClass.Invoke(multiclass,
                    new object[] { gunslinger, false });
                mechanics.Invoke(multiclass, null);
                var preview = ReadExactMember(multiclass, "Preview") as
                    Kingmaker.UnitLogic.UnitDescriptor;
                previewFighter = preview == null ? -1 :
                    preview.Progression.GetClassLevel(fighter);
                previewGunslinger = preview == null ? -1 :
                    preview.Progression.GetClassLevel(gunslinger);
                commit.Invoke(multiclass, null);
                multiclass = null;
                committedFighter = descriptor.Progression.GetClassLevel(fighter);
                committedGunslinger = descriptor.Progression.GetClassLevel(gunslinger);
                proficiencies = descriptor.HasFact(
                    BlueprintBootstrap.GunslingerClass.Proficiencies);
                grit = descriptor.HasFact(BlueprintBootstrap.GunslingerClass.Grit.Feature);
            }
            finally
            {
                MethodInfo cancel = typeof(Kingmaker.UnitLogic.Class.LevelUp.LevelUpController)
                    .GetMethod("Cancel", BindingFlags.Public | BindingFlags.Instance);
                if (multiclass != null && cancel != null) cancel.Invoke(multiclass, null);
                if (seed != null && cancel != null) cancel.Invoke(seed, null);
                if (entity != null) entity.Dispose();
                cleaned = SameReferences(partyBefore, SnapshotReferences(party)) &&
                    SameReferences(unitsBefore, SnapshotReferences(allUnits)) &&
                    SameReferences(remoteBefore, SnapshotReferences(remote)) &&
                    SameReferences(crossBefore, SnapshotReferences(cross)) &&
                    SameReferences(inventoryBefore, SnapshotReferences(inventory)) &&
                    (entity == null || !ContainsReference(cross, entity));
            }
            string observed = "seedFighter=" + fighterSeeded + ";selected=" + selected +
                ";preview=" + previewFighter + "/" + previewGunslinger +
                ";committed=" + committedFighter + "/" + committedGunslinger +
                ";callback=" + callback + ";proficiencies=" + proficiencies +
                ";grit=" + grit;
            var assertions = new List<RuntimeTestAssertion>
            {
                Assertion("native-multiclass-commit",
                    "Fighter 1/Gunslinger 0 -> Fighter 1/Gunslinger 1", observed,
                    fighterSeeded == 1 && selected && previewFighter == 1 &&
                        previewGunslinger == 1 && committedFighter == 1 &&
                        committedGunslinger == 1 && callback,
                    "LevelUp-mode native Commit and success callback"),
                Assertion("multiclass-level-one-facts",
                    "Gunslinger proficiencies and grit installed", observed,
                    proficiencies && grit,
                    "exact descriptor HasFact after native multiclass commit"),
                Assertion("external-isolation",
                    "unchanged party, units, cross-scene, companions, and inventory",
                    "cleaned=" + cleaned, cleaned,
                    "expanded reference snapshots after detached disposal"),
                Assertion("loaded-mod-version", _request.ExpectedModVersion,
                    _context.ModEntry.Info.Version,
                    _request.ExpectedModVersion == _context.ModEntry.Info.Version,
                    "Unity Mod Manager ModEntry.Info.Version")
            };
            return CreateResult(assertions.TrueForAll(value => value.Status == "PASS")
                ? RuntimeTestStatuses.Pass : RuntimeTestStatuses.Fail,
                assertions, null);
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

        private RuntimeTestResult RunDisposableGunslingerCreationCommit()
        {
            BlueprintCharacterClass gunslinger = BlueprintBootstrap.GunslingerClass.CharacterClass;
            object player = ReadExactMember(Kingmaker.Game.Instance, "Player");
            object state = ReadExactMember(Kingmaker.Game.Instance, "State");
            object party = ReadExactMember(player, "Party");
            object allUnits = ReadExactMember(state, "AllUnits");
            object remote = ReadExactMember(player, "RemoteCompanions");
            object cross = ReadExactMember(ReadExactMember(player, "CrossSceneState"),
                "AllEntityData");
            object inventory = ReadExactMember(player, "Inventory");
            object[] partyBefore = SnapshotReferences(party);
            object[] unitsBefore = SnapshotReferences(allUnits);
            object[] remoteBefore = SnapshotReferences(remote);
            object[] crossBefore = SnapshotReferences(cross);
            object[] inventoryBefore = SnapshotReferences(inventory);
            Player runtimePlayer = player as Player;
            Kingmaker.EntitySystem.Entities.UnitEntityData entity = null;
            object controller = null;
            BlueprintItem[] startingItems = null;
            int[] startingCounts = null;
            int originalStartingGold = 0;
            long moneyBefore = 0;
            int previewLevel = -1, committedLevel = -1;
            int pistolDelta = -1, powderDelta = -1, ballDelta = -1;
            bool selected = false, callback = false, proficiencies = false,
                grit = false, cleaned = false;
            var addedInventory = new List<object>();
            try
            {
                entity = new Kingmaker.UI.LevelUp.ChargenUnit(
                    BlueprintRoot.Instance.DefaultPlayerCharacter).Unit;
                Kingmaker.UnitLogic.UnitDescriptor descriptor = entity == null ? null :
                    entity.Descriptor;
                if (descriptor == null || descriptor.Progression == null)
                    throw new InvalidOperationException(
                        "Detached character-creation unit is unavailable.");
                if (runtimePlayer == null || runtimePlayer.Inventory == null)
                    throw new InvalidOperationException(
                        "Exact shared inventory is unavailable for rollback.");
                startingItems = gunslinger.StartingItems ?? Array.Empty<BlueprintItem>();
                startingCounts = startingItems.Select(item =>
                    runtimePlayer.Inventory.Count(item)).ToArray();
                originalStartingGold = gunslinger.StartingGold;
                moneyBefore = runtimePlayer.Money;
                gunslinger.StartingGold = 0;

                Type type = typeof(Kingmaker.UnitLogic.Class.LevelUp.LevelUpController);
                MethodInfo start = type.GetMethods(BindingFlags.Public |
                    BindingFlags.NonPublic | BindingFlags.Static).Single(value =>
                        value.Name == "StartWithoutAssigningStaticInstance" &&
                        value.GetParameters().Length == 5);
                MethodInfo selectClass = type.GetMethod("SelectClass",
                    BindingFlags.Public | BindingFlags.Instance, null,
                    new[] { typeof(BlueprintCharacterClass), typeof(bool) }, null);
                MethodInfo mechanics = type.GetMethod("ApplyClassMechanics",
                    BindingFlags.Public | BindingFlags.Instance);
                MethodInfo cancel = type.GetMethod("Cancel",
                    BindingFlags.Public | BindingFlags.Instance);
                MethodInfo commit = type.GetMethod("Commit",
                    BindingFlags.Public | BindingFlags.Instance);
                if (selectClass == null || mechanics == null || cancel == null ||
                    commit == null)
                    throw new MissingMethodException(
                        "An exact native character-creation commit method is unavailable.");

                object charGen = Enum.Parse(start.GetParameters()[4].ParameterType,
                    "CharGen", false);
                Action onSuccess = () => callback = true;
                controller = start.Invoke(null,
                    new object[] { descriptor, false, null, onSuccess, charGen });
                selected = (bool)selectClass.Invoke(controller,
                    new object[] { gunslinger, false });
                mechanics.Invoke(controller, null);
                var preview = ReadExactMember(controller, "Preview") as
                    Kingmaker.UnitLogic.UnitDescriptor;
                previewLevel = preview == null ? -1 :
                    preview.Progression.GetClassLevel(gunslinger);
                commit.Invoke(controller, null);
                controller = null;
                LevelUpHelper.AddStartingItems(descriptor);
                addedInventory.AddRange(EnumerateRuntimeInventory(runtimePlayer.Inventory)
                    .Where(item => !inventoryBefore.Any(existing =>
                        ReferenceEquals(existing, item))));
                if (startingItems.Length != 3)
                    throw new InvalidOperationException(
                        "Gunslinger creation did not expose three stable starting-item identities.");
                pistolDelta = runtimePlayer.Inventory.Count(startingItems[0]) -
                    startingCounts[0];
                powderDelta = runtimePlayer.Inventory.Count(startingItems[1]) -
                    startingCounts[1];
                ballDelta = runtimePlayer.Inventory.Count(startingItems[2]) -
                    startingCounts[2];
                committedLevel = descriptor.Progression.GetClassLevel(gunslinger);
                proficiencies = descriptor.HasFact(
                    BlueprintBootstrap.GunslingerClass.Proficiencies);
                grit = descriptor.HasFact(BlueprintBootstrap.GunslingerClass.Grit.Feature);
            }
            finally
            {
                MethodInfo cancel = typeof(Kingmaker.UnitLogic.Class.LevelUp.LevelUpController)
                    .GetMethod("Cancel", BindingFlags.Public | BindingFlags.Instance);
                if (controller != null && cancel != null) cancel.Invoke(controller, null);
                gunslinger.StartingGold = originalStartingGold;
                if (runtimePlayer != null && runtimePlayer.Inventory != null)
                {
                    foreach (object item in addedInventory)
                    {
                        object ignored;
                        string method;
                        ReflectionAccess.TryInvokeAny(runtimePlayer.Inventory,
                            new[] { "Remove", "RemoveItem" },
                            new[] { new object[] { item, 1, false },
                                new object[] { item, 1 }, new object[] { item } },
                            out ignored, out method);
                    }
                    if (startingItems != null && startingCounts != null)
                        for (int index = 0; index < startingItems.Length; index++)
                        {
                            int excess = runtimePlayer.Inventory.Count(startingItems[index]) -
                                startingCounts[index];
                            if (excess > 0)
                                runtimePlayer.Inventory.Remove(startingItems[index], excess);
                        }
                }
                if (entity != null) entity.Dispose();
                cleaned = SameReferences(partyBefore, SnapshotReferences(party)) &&
                    SameReferences(unitsBefore, SnapshotReferences(allUnits)) &&
                    SameReferences(remoteBefore, SnapshotReferences(remote)) &&
                    SameReferences(crossBefore, SnapshotReferences(cross)) &&
                    SameReferences(inventoryBefore, SnapshotReferences(inventory)) &&
                    (runtimePlayer == null || runtimePlayer.Money == moneyBefore) &&
                    (entity == null || !ContainsReference(cross, entity));
            }
            string observed = "selected=" + selected + ";preview=" + previewLevel +
                ";committed=" + committedLevel + ";callback=" + callback +
                ";proficiencies=" + proficiencies + ";grit=" + grit +
                ";starting=" + pistolDelta + "/" + powderDelta + "/" + ballDelta;
            var assertions = new List<RuntimeTestAssertion>
            {
                Assertion("native-character-creation-commit",
                    "preview and committed Gunslinger level one with success callback",
                    observed, selected && previewLevel == 1 && committedLevel == 1 &&
                        callback, "CharGen-mode native Commit on detached player unit"),
                Assertion("creation-level-one-facts",
                    "Gunslinger proficiencies and grit installed", observed,
                    proficiencies && grit,
                    "exact committed descriptor HasFact after native Commit"),
                Assertion("creation-starting-stacks",
                    "one Pistol; one stack of 20 Black Powder Charges; one stack of 20 Lead Balls",
                    observed, pistolDelta == 1 && powderDelta == 20 && ballDelta == 20 &&
                        addedInventory.Count == 3,
                    "native CharGen commit plus exact stable blueprint inventory deltas"),
                Assertion("external-isolation",
                    "unchanged party, units, cross-scene, companions, inventory, and money",
                    "cleaned=" + cleaned, cleaned,
                    "starting grants rolled back and detached entity disposed"),
                Assertion("loaded-mod-version", _request.ExpectedModVersion,
                    _context.ModEntry.Info.Version,
                    _request.ExpectedModVersion == _context.ModEntry.Info.Version,
                    "Unity Mod Manager ModEntry.Info.Version")
            };
            return CreateResult(assertions.TrueForAll(value => value.Status == "PASS")
                ? RuntimeTestStatuses.Pass : RuntimeTestStatuses.Fail,
                assertions, null);
        }

        private RuntimeTestResult RunDisposableGunslingerRespecCommit()
        {
            BlueprintCharacterClass gunslinger = BlueprintBootstrap.GunslingerClass.CharacterClass;
            BlueprintCharacterClass fighter = BlueprintRoot.Instance.Progression.CharacterClasses
                .Single(value => value != null && value.AssetGuid ==
                    "48ac8db94d5de7645906c7d0ad3bcfbd");
            object player = ReadExactMember(Kingmaker.Game.Instance, "Player");
            object state = ReadExactMember(Kingmaker.Game.Instance, "State");
            object party = ReadExactMember(player, "Party");
            object allUnits = ReadExactMember(state, "AllUnits");
            object remote = ReadExactMember(player, "RemoteCompanions");
            object cross = ReadExactMember(ReadExactMember(player, "CrossSceneState"),
                "AllEntityData");
            object inventory = ReadExactMember(player, "Inventory");
            object[] partyBefore = SnapshotReferences(party);
            object[] unitsBefore = SnapshotReferences(allUnits);
            object[] remoteBefore = SnapshotReferences(remote);
            object[] crossBefore = SnapshotReferences(cross);
            object[] inventoryBefore = SnapshotReferences(inventory);
            Kingmaker.EntitySystem.Entities.UnitEntityData sourceEntity = null;
            Kingmaker.EntitySystem.Entities.UnitEntityData replacementEntity = null;
            Kingmaker.UnitLogic.UnitDescriptor sourceDescriptor = null;
            object seed = null, respecController = null;
            int sourceFighter = -1, sourceGunslinger = -1;
            int previewFighter = -1, previewGunslinger = -1;
            int replacementFighter = -1, replacementGunslinger = -1;
            bool selected = false, callback = false, proficiencies = false,
                grit = false, cleaned = false;
            Player runtimePlayer = player as Player;
            BlueprintItem[] startingItems = null;
            int[] startingCounts = null;
            int originalStartingGold = 0;
            long moneyBefore = 0;
            var addedInventory = new List<object>();
            try
            {
                BlueprintUnit blueprint = BlueprintRoot.Instance.DefaultPlayerCharacter;
                sourceEntity = new Kingmaker.UI.LevelUp.ChargenUnit(blueprint).Unit;
                sourceDescriptor = sourceEntity == null ? null : sourceEntity.Descriptor;
                replacementEntity = new Kingmaker.UI.LevelUp.ChargenUnit(blueprint).Unit;
                var replacement = replacementEntity == null ? null :
                    replacementEntity.Descriptor;
                if (sourceDescriptor == null || replacement == null)
                    throw new InvalidOperationException(
                        "Detached respec source or replacement is unavailable.");
                if (runtimePlayer == null || runtimePlayer.Inventory == null)
                    throw new InvalidOperationException(
                        "Exact shared inventory is unavailable for rollback.");
                startingItems = gunslinger.StartingItems ?? Array.Empty<BlueprintItem>();
                startingCounts = startingItems.Select(item =>
                    runtimePlayer.Inventory.Count(item)).ToArray();
                originalStartingGold = gunslinger.StartingGold;
                moneyBefore = runtimePlayer.Money;
                gunslinger.StartingGold = 0;
                Type type = typeof(Kingmaker.UnitLogic.Class.LevelUp.LevelUpController);
                MethodInfo start = type.GetMethods(BindingFlags.Public |
                    BindingFlags.NonPublic | BindingFlags.Static).Single(value =>
                        value.Name == "StartWithoutAssigningStaticInstance" &&
                        value.GetParameters().Length == 5);
                MethodInfo selectClass = type.GetMethod("SelectClass",
                    BindingFlags.Public | BindingFlags.Instance, null,
                    new[] { typeof(BlueprintCharacterClass), typeof(bool) }, null);
                MethodInfo mechanics = type.GetMethod("ApplyClassMechanics",
                    BindingFlags.Public | BindingFlags.Instance);
                MethodInfo apply = type.GetMethod("ApplyLevelup", BindingFlags.Public |
                    BindingFlags.NonPublic | BindingFlags.Instance);
                MethodInfo cancel = type.GetMethod("Cancel",
                    BindingFlags.Public | BindingFlags.Instance);
                MethodInfo commit = type.GetMethod("Commit",
                    BindingFlags.Public | BindingFlags.Instance);
                if (selectClass == null || mechanics == null || apply == null ||
                    cancel == null || commit == null)
                    throw new MissingMethodException(
                        "An exact native respec commit method is unavailable.");

                object charGen = Enum.Parse(start.GetParameters()[4].ParameterType,
                    "CharGen", false);
                seed = start.Invoke(null,
                    new object[] { sourceDescriptor, false, null, null, charGen });
                if (!(bool)selectClass.Invoke(seed, new object[] { fighter, false }))
                    throw new InvalidOperationException("Fighter source seed was rejected.");
                mechanics.Invoke(seed, null);
                apply.Invoke(seed, new object[] { sourceDescriptor });
                cancel.Invoke(seed, null);
                seed = null;

                object respec = Enum.Parse(start.GetParameters()[4].ParameterType,
                    "Respec", false);
                Action onSuccess = () => callback = true;
                respecController = start.Invoke(null,
                    new object[] { replacement, false, null, onSuccess, respec });
                selected = (bool)selectClass.Invoke(respecController,
                    new object[] { gunslinger, false });
                mechanics.Invoke(respecController, null);
                var preview = ReadExactMember(respecController, "Preview") as
                    Kingmaker.UnitLogic.UnitDescriptor;
                previewFighter = preview == null ? -1 :
                    preview.Progression.GetClassLevel(fighter);
                previewGunslinger = preview == null ? -1 :
                    preview.Progression.GetClassLevel(gunslinger);
                commit.Invoke(respecController, null);
                respecController = null;
                addedInventory.AddRange(EnumerateRuntimeInventory(runtimePlayer.Inventory)
                    .Where(item => !inventoryBefore.Any(existing =>
                        ReferenceEquals(existing, item))));
                sourceFighter = sourceDescriptor.Progression.GetClassLevel(fighter);
                sourceGunslinger = sourceDescriptor.Progression.GetClassLevel(gunslinger);
                replacementFighter = replacement.Progression.GetClassLevel(fighter);
                replacementGunslinger = replacement.Progression.GetClassLevel(gunslinger);
                proficiencies = replacement.HasFact(
                    BlueprintBootstrap.GunslingerClass.Proficiencies);
                grit = replacement.HasFact(BlueprintBootstrap.GunslingerClass.Grit.Feature);
            }
            finally
            {
                MethodInfo cancel = typeof(Kingmaker.UnitLogic.Class.LevelUp.LevelUpController)
                    .GetMethod("Cancel", BindingFlags.Public | BindingFlags.Instance);
                if (respecController != null && cancel != null)
                    cancel.Invoke(respecController, null);
                if (seed != null && cancel != null) cancel.Invoke(seed, null);
                gunslinger.StartingGold = originalStartingGold;
                if (runtimePlayer != null && runtimePlayer.Inventory != null)
                {
                    foreach (object item in addedInventory)
                    {
                        object ignored;
                        string method;
                        ReflectionAccess.TryInvokeAny(runtimePlayer.Inventory,
                            new[] { "Remove", "RemoveItem" },
                            new[] { new object[] { item, 1, false },
                                new object[] { item, 1 }, new object[] { item } },
                            out ignored, out method);
                    }
                    if (startingItems != null && startingCounts != null)
                        for (int index = 0; index < startingItems.Length; index++)
                        {
                            int excess = runtimePlayer.Inventory.Count(startingItems[index]) -
                                startingCounts[index];
                            if (excess > 0)
                                runtimePlayer.Inventory.Remove(startingItems[index], excess);
                        }
                }
                if (replacementEntity != null) replacementEntity.Dispose();
                if (sourceEntity != null) sourceEntity.Dispose();
                cleaned = SameReferences(partyBefore, SnapshotReferences(party)) &&
                    SameReferences(unitsBefore, SnapshotReferences(allUnits)) &&
                    SameReferences(remoteBefore, SnapshotReferences(remote)) &&
                    SameReferences(crossBefore, SnapshotReferences(cross)) &&
                    SameReferences(inventoryBefore, SnapshotReferences(inventory)) &&
                    (runtimePlayer == null || runtimePlayer.Money == moneyBefore) &&
                    (sourceEntity == null || !ContainsReference(cross, sourceEntity)) &&
                    (replacementEntity == null || !ContainsReference(cross,
                        replacementEntity));
            }
            string observed = "selected=" + selected + ";preview=" + previewFighter +
                "/" + previewGunslinger + ";source=" + sourceFighter + "/" +
                sourceGunslinger + ";replacement=" + replacementFighter + "/" +
                replacementGunslinger + ";callback=" + callback +
                ";proficiencies=" + proficiencies + ";grit=" + grit;
            var assertions = new List<RuntimeTestAssertion>
            {
                Assertion("native-respec-replacement-commit",
                    "source Fighter 1/Gunslinger 0; replacement Fighter 0/Gunslinger 1",
                    observed, selected && previewFighter == 0 && previewGunslinger == 1 &&
                        sourceFighter == 1 && sourceGunslinger == 0 &&
                        replacementFighter == 0 && replacementGunslinger == 1 && callback,
                    "Respec-mode native Commit on detached replacement"),
                Assertion("respec-replacement-level-one-facts",
                    "Gunslinger proficiencies and grit installed", observed,
                    proficiencies && grit,
                    "exact replacement descriptor HasFact after Commit"),
                Assertion("external-isolation",
                    "unchanged party, units, cross-scene, companions, and inventory",
                    "cleaned=" + cleaned, cleaned,
                    "expanded snapshots after both detached entities disposed"),
                Assertion("loaded-mod-version", _request.ExpectedModVersion,
                    _context.ModEntry.Info.Version,
                    _request.ExpectedModVersion == _context.ModEntry.Info.Version,
                    "Unity Mod Manager ModEntry.Info.Version")
            };
            return CreateResult(assertions.TrueForAll(value => value.Status == "PASS")
                ? RuntimeTestStatuses.Pass : RuntimeTestStatuses.Fail,
                assertions, null);
        }

        private RuntimeTestResult RunDisposableGunslingerBroadRespec()
        {
            BlueprintCharacterClass gunslinger = BlueprintBootstrap.GunslingerClass.CharacterClass;
            BlueprintCharacterClass fighter = BlueprintRoot.Instance.Progression.CharacterClasses
                .Single(value => value != null && value.AssetGuid ==
                    "48ac8db94d5de7645906c7d0ad3bcfbd");
            Player player = Game.Instance.Player;
            object state = ReadExactMember(Game.Instance, "State");
            object party = ReadExactMember(player, "Party");
            object allUnits = ReadExactMember(state, "AllUnits");
            object remote = ReadExactMember(player, "RemoteCompanions");
            object cross = ReadExactMember(ReadExactMember(player, "CrossSceneState"),
                "AllEntityData");
            object[] partyBefore = SnapshotReferences(party);
            object[] unitsBefore = SnapshotReferences(allUnits);
            object[] remoteBefore = SnapshotReferences(remote);
            object[] crossBefore = SnapshotReferences(cross);
            object[] inventoryBefore = SnapshotReferences(player.Inventory);
            long moneyBefore = player.Money;
            BlueprintItem[] startingItems = gunslinger.StartingItems ?? Array.Empty<BlueprintItem>();
            int[] startingCounts = startingItems.Select(item =>
                player.Inventory.Count(item)).ToArray();
            int originalStartingGold = gunslinger.StartingGold;
            var addedInventory = new List<object>();
            Kingmaker.EntitySystem.Entities.UnitEntityData source = null;
            Kingmaker.EntitySystem.Entities.UnitEntityData replacementEntity = null;
            object seed = null;
            var handler = new BroadRespecInitiationHandler
            {
                SelectedClass = gunslinger
            };
            bool subscribed = false, callback = false, facts = false,
                descriptorsAlias = false, cleaned = false;
            int sourceFighter = -1, sourceGunslinger = -1,
                replacementFighter = -1, replacementGunslinger = -1;
            try
            {
                source = new Kingmaker.UI.LevelUp.ChargenUnit(
                    BlueprintRoot.Instance.DefaultPlayerCharacter).Unit;
                UnitDescriptor descriptor = source == null ? null : source.Descriptor;
                if (descriptor == null)
                    throw new InvalidOperationException(
                        "Detached broad-respec source is unavailable.");
                Type type = typeof(Kingmaker.UnitLogic.Class.LevelUp.LevelUpController);
                MethodInfo start = type.GetMethods(BindingFlags.Public |
                    BindingFlags.NonPublic | BindingFlags.Static).Single(value =>
                        value.Name == "StartWithoutAssigningStaticInstance" &&
                        value.GetParameters().Length == 5);
                MethodInfo selectClass = type.GetMethod("SelectClass",
                    BindingFlags.Public | BindingFlags.Instance, null,
                    new[] { typeof(BlueprintCharacterClass), typeof(bool) }, null);
                MethodInfo mechanics = type.GetMethod("ApplyClassMechanics",
                    BindingFlags.Public | BindingFlags.Instance);
                MethodInfo apply = type.GetMethod("ApplyLevelup", BindingFlags.Public |
                    BindingFlags.NonPublic | BindingFlags.Instance);
                MethodInfo cancel = type.GetMethod("Cancel",
                    BindingFlags.Public | BindingFlags.Instance);
                if (selectClass == null || mechanics == null || apply == null ||
                    cancel == null)
                    throw new MissingMethodException(
                        "Broad-respec seed contract is unavailable.");
                object charGen = Enum.Parse(start.GetParameters()[4].ParameterType,
                    "CharGen", false);
                seed = start.Invoke(null,
                    new object[] { descriptor, false, null, null, charGen });
                if (!(bool)selectClass.Invoke(seed, new object[] { fighter, false }))
                    throw new InvalidOperationException("Broad-respec Fighter seed failed.");
                mechanics.Invoke(seed, null);
                apply.Invoke(seed, new object[] { descriptor });
                cancel.Invoke(seed, null);
                seed = null;

                gunslinger.StartingGold = 0;
                EventBus.Subscribe(handler);
                subscribed = true;
                player.RespecCompanion(source, () => callback = true);
                replacementEntity = handler.Replacement == null ? null :
                    handler.Replacement.Unit;
                addedInventory.AddRange(EnumerateRuntimeInventory(player.Inventory)
                    .Where(item => !inventoryBefore.Any(existing =>
                        ReferenceEquals(existing, item))));
                sourceFighter = descriptor.Progression.GetClassLevel(fighter);
                sourceGunslinger = descriptor.Progression.GetClassLevel(gunslinger);
                replacementFighter = handler.Replacement == null ? -1 :
                    handler.Replacement.Progression.GetClassLevel(fighter);
                replacementGunslinger = handler.Replacement == null ? -1 :
                    handler.Replacement.Progression.GetClassLevel(gunslinger);
                descriptorsAlias = handler.Replacement != null &&
                    ReferenceEquals(source.Descriptor, handler.Replacement);
                facts = handler.Replacement != null &&
                    handler.Replacement.HasFact(
                        BlueprintBootstrap.GunslingerClass.Proficiencies) &&
                    handler.Replacement.HasFact(
                        BlueprintBootstrap.GunslingerClass.Grit.Feature);
            }
            finally
            {
                if (subscribed) EventBus.Unsubscribe(handler);
                MethodInfo cancel = typeof(Kingmaker.UnitLogic.Class.LevelUp.LevelUpController)
                    .GetMethod("Cancel", BindingFlags.Public | BindingFlags.Instance);
                if (handler.Controller != null && cancel != null)
                    cancel.Invoke(handler.Controller, null);
                if (seed != null && cancel != null) cancel.Invoke(seed, null);
                gunslinger.StartingGold = originalStartingGold;
                foreach (object item in addedInventory)
                {
                    object ignored;
                    string method;
                    ReflectionAccess.TryInvokeAny(player.Inventory,
                        new[] { "Remove", "RemoveItem" },
                        new[] { new object[] { item, 1, false },
                            new object[] { item, 1 }, new object[] { item } },
                        out ignored, out method);
                }
                for (int index = 0; index < startingItems.Length; index++)
                {
                    int excess = player.Inventory.Count(startingItems[index]) -
                        startingCounts[index];
                    if (excess > 0)
                        player.Inventory.Remove(startingItems[index], excess);
                }
                if (replacementEntity != null &&
                    !ReferenceEquals(replacementEntity, source) &&
                    !replacementEntity.Destroyed &&
                    replacementEntity.Descriptor.Body != null)
                    replacementEntity.Dispose();
                if (source != null && !source.Destroyed &&
                    source.Descriptor.Body != null) source.Dispose();
                cleaned = SameReferences(partyBefore, SnapshotReferences(party)) &&
                    SameReferences(unitsBefore, SnapshotReferences(allUnits)) &&
                    SameReferences(remoteBefore, SnapshotReferences(remote)) &&
                    SameReferences(crossBefore, SnapshotReferences(cross)) &&
                    SameReferences(inventoryBefore, SnapshotReferences(player.Inventory)) &&
                    player.Money == moneyBefore;
            }
            string observed = "handler=" + handler.Invoked + ";selected=" +
                handler.Selected + ";callback=" + callback + ";source=" +
                sourceFighter + "/" + sourceGunslinger + ";replacement=" +
                replacementFighter + "/" + replacementGunslinger +
                ";alias=" + descriptorsAlias + ";facts=" + facts;
            var assertions = new List<RuntimeTestAssertion>
            {
                Assertion("broad-native-replacement",
                    "handler/callback; source and replacement both Gunslinger 1",
                    observed, handler.Invoked && handler.Selected && callback &&
                        sourceFighter == 0 && sourceGunslinger == 1 &&
                        replacementFighter == 0 && replacementGunslinger == 1,
                    "Player.RespecCompanion plus exact initiation handler and Commit"),
                Assertion("broad-replacement-facts",
                    "proficiency and grit installed", observed, facts,
                    "replacement descriptor exact facts"),
                Assertion("external-isolation",
                    "party, units, companions, cross-scene, inventory, and money restored",
                    "cleaned=" + cleaned, cleaned,
                    "handler unsubscription, grant rollback, and entity disposal in finally"),
                Assertion("loaded-mod-version", _request.ExpectedModVersion,
                    _context.ModEntry.Info.Version,
                    _request.ExpectedModVersion == _context.ModEntry.Info.Version,
                    "Unity Mod Manager ModEntry.Info.Version")
            };
            return CreateResult(assertions.TrueForAll(value => value.Status == "PASS")
                ? RuntimeTestStatuses.Pass : RuntimeTestStatuses.Fail,
                assertions, null);
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
                    observed, initialGrit > 0 && afterPositiveGrit == initialGrit &&
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

        private RuntimeTestResult RunDisposableGunslingerBonusFeats()
        {
            const string selectionGuid = "41c8486641f7d6d4283ca9dae4147a9f";
            GunslingerClassBlueprintSet gunslinger = BlueprintBootstrap.GunslingerClass;
            int[] requiredLevels = Classes.BonusFeatProgression.Levels;
            BlueprintFeatureSelection selection = gunslinger.Progression
                .LevelEntries[requiredLevels[0] - 1].Features
                .OfType<BlueprintFeatureSelection>().Single(feature =>
                    feature.AssetGuid == selectionGuid);
            var observedLevels = new List<int>();
            int totalOccurrences = 0;
            foreach (LevelEntry entry in gunslinger.Progression.LevelEntries)
            {
                int count = entry.Features.Count(feature =>
                    ReferenceEquals(feature, selection));
                totalOccurrences += count;
                if (count > 0) observedLevels.Add(entry.Level);
            }
            string observed = "guid=" + selection.AssetGuid +
                ";levels=" + string.Join(",", observedLevels) +
                ";occurrences=" + totalOccurrences +
                ";features=" + (selection.Features == null ? -1 : selection.Features.Length) +
                ";allFeatures=" + (selection.AllFeatures == null ? -1 : selection.AllFeatures.Length) +
                ";ignorePrerequisites=" + selection.IgnorePrerequisites;
            bool cadence = observedLevels.SequenceEqual(requiredLevels) &&
                totalOccurrences == requiredLevels.Length;
            bool nativeContract = selection.AssetGuid == selectionGuid &&
                selection.AllFeatures != null && selection.AllFeatures.Length > 0 &&
                !selection.IgnorePrerequisites;
            var assertions = new List<RuntimeTestAssertion>
            {
                Assertion("bonus-feats-cadence", "4,8,12,16,20 exactly once",
                    observed, cadence,
                    "production Gunslinger progression LevelEntries"),
                Assertion("bonus-feats-native-selection",
                    "exact Fighter selection; candidates nonempty; prerequisites enforced",
                    observed, nativeContract,
                    "exact installed BlueprintFeatureSelection identity"),
                Assertion("loaded-mod-version", _request.ExpectedModVersion,
                    _context.ModEntry.Info.Version,
                    _request.ExpectedModVersion == _context.ModEntry.Info.Version,
                    "Unity Mod Manager ModEntry.Info.Version")
            };
            return CreateResult(assertions.TrueForAll(value => value.Status == "PASS")
                ? RuntimeTestStatuses.Pass : RuntimeTestStatuses.Fail, assertions, null);
        }

        private RuntimeTestResult RunDisposableGunslingerGunTraining()
        {
            BlueprintUnit source = BlueprintRoot.Instance.DefaultPlayerCharacter;
            GunslingerClassBlueprintSet gunslinger = BlueprintBootstrap.GunslingerClass;
            GunTrainingBlueprintSet training = gunslinger.GunTraining;
            BlueprintItemWeapon pistol = BlueprintBootstrap.ProductionFirearms.Pistol.Item;
            object player = ReadExactMember(Kingmaker.Game.Instance, "Player");
            object state = ReadExactMember(Kingmaker.Game.Instance, "State");
            object party = ReadExactMember(player, "Party");
            object allUnits = ReadExactMember(state, "AllUnits");
            object[] partyBefore = SnapshotReferences(party);
            object[] unitsBefore = SnapshotReferences(allUnits);
            Kingmaker.EntitySystem.Entities.UnitEntityData attacker = null;
            Kingmaker.EntitySystem.Entities.UnitEntityData target = null;
            ItemEntityWeapon weapon = null;
            int untrainedDamage = int.MinValue, trainedDamage = int.MinValue;
            FirearmCondition untrainedAfter = FirearmCondition.Normal;
            FirearmCondition trainedAfter = FirearmCondition.Normal;
            int targetDamageBefore = 0;
            bool cleaned = false;
            string stage = "progression-contract";
            int[] requiredLevels = Classes.GunTrainingProgression.Levels;
            var observedLevels = new List<int>();
            int occurrences = 0;
            foreach (LevelEntry entry in gunslinger.Progression.LevelEntries)
            {
                int count = entry.Features.Count(feature =>
                    ReferenceEquals(feature, training.Selection));
                occurrences += count;
                if (count > 0) observedLevels.Add(entry.Level);
            }
            bool selectionContract = observedLevels.SequenceEqual(requiredLevels) &&
                occurrences == requiredLevels.Length && training.Choices.Length == 5 &&
                training.Selection.AllFeatures.SequenceEqual(training.Choices) &&
                training.Choices.Select(value => value.AssetGuid).Distinct().Count() == 5 &&
                training.Choices.All(value => value.Ranks == 1) &&
                training.Selection.Obligatory && !training.Selection.IgnorePrerequisites;
            try
            {
                attacker = new Kingmaker.UI.LevelUp.ChargenUnit(source).Unit;
                target = new Kingmaker.UI.LevelUp.ChargenUnit(source).Unit;
                attacker.Descriptor.Stats.Dexterity.BaseValue = 18;
                targetDamageBefore = target.Damage;

                stage = "untrained-broken-shot";
                weapon = new ItemEntityWeapon(pistol);
                attacker.Body.PrimaryHand.InsertItem(weapon);
                var untrainedStats = new RuleCalculateWeaponStats(attacker, weapon, null);
                Rulebook.Trigger(untrainedStats);
                untrainedDamage = untrainedStats.BonusDamage;
                FirearmRuntimeState.Service.Set(weapon, new FirearmState(
                    FirearmState.CurrentSchemaVersion, 1,
                    FirearmStateTokenCatalog.DiagnosticLeadBall,
                    FirearmCondition.Broken));
                FirearmMisfireRuntime.QueueForcedNaturalRoll(5);
                var untrainedAttack = new RuleAttackWithWeapon(attacker, target, weapon, 0);
                Rulebook.Trigger(untrainedAttack);
                untrainedAfter = FirearmRuntimeState.Service.GetOrCreate(weapon)
                    .Repository.State.Condition;

                stage = "trained-broken-shot";
                FirearmRuntimeState.Service.Forget(weapon);
                attacker.Body.PrimaryHand.RemoveItem(false);
                weapon = null;
                attacker.Descriptor.AddFact(
                    training.ChoiceFor(FirearmKind.Pistol));
                weapon = new ItemEntityWeapon(pistol);
                attacker.Body.PrimaryHand.InsertItem(weapon);
                var trainedStats = new RuleCalculateWeaponStats(attacker, weapon, null);
                Rulebook.Trigger(trainedStats);
                trainedDamage = trainedStats.BonusDamage;
                FirearmRuntimeState.Service.Set(weapon, new FirearmState(
                    FirearmState.CurrentSchemaVersion, 1,
                    FirearmStateTokenCatalog.DiagnosticLeadBall,
                    FirearmCondition.Broken));
                FirearmMisfireRuntime.QueueForcedNaturalRoll(5);
                var trainedAttack = new RuleAttackWithWeapon(attacker, target, weapon, 0);
                Rulebook.Trigger(trainedAttack);
                trainedAfter = FirearmRuntimeState.Service.GetOrCreate(weapon)
                    .Repository.State.Condition;
            }
            catch (Exception exception)
            {
                throw new InvalidOperationException(
                    "Disposable Gun Training failed at stage " + stage + ".", exception);
            }
            finally
            {
                FirearmMisfireRuntime.CancelForcedNaturalRoll();
                if (weapon != null)
                {
                    FirearmRuntimeState.Service.Forget(weapon);
                    if (attacker != null && attacker.Body.PrimaryHand.MaybeItem != null)
                        attacker.Body.PrimaryHand.RemoveItem(false);
                }
                if (attacker != null && attacker.Descriptor.HasFact(
                    training.ChoiceFor(FirearmKind.Pistol)))
                    attacker.Descriptor.RemoveFact(
                        training.ChoiceFor(FirearmKind.Pistol));
                if (target != null) target.Damage = targetDamageBefore;
                if (target != null) target.Dispose();
                if (attacker != null) attacker.Dispose();
                cleaned = SameReferences(partyBefore, SnapshotReferences(party)) &&
                    SameReferences(unitsBefore, SnapshotReferences(allUnits)) &&
                    (attacker == null || !ContainsReference(allUnits, attacker)) &&
                    (target == null || !ContainsReference(allUnits, target));
            }
            string observed = "levels=" + string.Join(",", observedLevels) +
                ";occurrences=" + occurrences + ";choices=" + training.Choices.Length +
                ";untrainedDamage=" + untrainedDamage +
                ";trainedDamage=" + trainedDamage +
                ";untrainedAfter=" + untrainedAfter +
                ";trainedAfter=" + trainedAfter;
            var assertions = new List<RuntimeTestAssertion>
            {
                Assertion("gun-training-progression",
                    "5,9,13,17 exactly once; five distinct rank-one choices",
                    observed, selectionContract,
                    "production progression and stable BlueprintFeatureSelection"),
                Assertion("gun-training-damage", "selected pistol adds Dexterity +4 once",
                    observed, trainedDamage == untrainedDamage + 4,
                    "native RuleCalculateWeaponStats.AddBonusDamage pipeline"),
                Assertion("gun-training-misfire",
                    "forced 5: untrained Broken -> Wrecked; trained remains Broken",
                    observed, untrainedAfter == FirearmCondition.Wrecked &&
                    trainedAfter == FirearmCondition.Broken,
                    "production discharge and natural-roll misfire pipeline"),
                Assertion("external-isolation", "unchanged party and global-unit snapshots",
                    "cleaned=" + cleaned, cleaned,
                    "facts removed, item state forgotten, disposables disposed"),
                Assertion("loaded-mod-version", _request.ExpectedModVersion,
                    _context.ModEntry.Info.Version,
                    _request.ExpectedModVersion == _context.ModEntry.Info.Version,
                    "Unity Mod Manager ModEntry.Info.Version")
            };
            return CreateResult(assertions.TrueForAll(value => value.Status == "PASS")
                ? RuntimeTestStatuses.Pass : RuntimeTestStatuses.Fail, assertions, null);
        }

        private RuntimeTestResult RunDisposableGunslingerDeadShot()
        {
            BlueprintUnit source = BlueprintRoot.Instance.DefaultPlayerCharacter;
            GunslingerClassBlueprintSet gunslinger = BlueprintBootstrap.GunslingerClass;
            BlueprintItemWeapon pistol = BlueprintBootstrap.ProductionFirearms.Pistol.Item;
            object player = ReadExactMember(Kingmaker.Game.Instance, "Player");
            object state = ReadExactMember(Kingmaker.Game.Instance, "State");
            object party = ReadExactMember(player, "Party");
            object allUnits = ReadExactMember(state, "AllUnits");
            object[] partyBefore = SnapshotReferences(party);
            object[] unitsBefore = SnapshotReferences(allUnits);
            Kingmaker.EntitySystem.Entities.UnitEntityData attacker = null;
            Kingmaker.EntitySystem.Entities.UnitEntityData target = null;
            ItemEntityWeapon weapon = null;
            Deeds.DeadShotExecutionResult mixed = null;
            Deeds.DeadShotExecutionResult allMisfire = null;
            int gritBefore = -1, gritAfterMixed = -1, gritAfterMisfire = -1;
            bool cleaned = false;
            int targetDamageBefore = 0;
            string stage = "blueprint-contract";
            int levelSevenCount = gunslinger.Progression.LevelEntries[6].Features
                .Count(value => ReferenceEquals(value, gunslinger.DeadShot.Feature));
            bool blueprintContract = levelSevenCount == 1 &&
                gunslinger.DeadShot.Ability.IsFullRoundAction &&
                gunslinger.DeadShot.Ability.ActionType == UnitCommand.CommandType.Standard &&
                gunslinger.DeadShot.Ability.Range == AbilityRange.Weapon;
            try
            {
                attacker = new Kingmaker.UI.LevelUp.ChargenUnit(source).Unit;
                target = new Kingmaker.UI.LevelUp.ChargenUnit(source).Unit;
                targetDamageBefore = target.Damage;
                attacker.Descriptor.Stats.BaseAttackBonus.BaseValue = 11;
                attacker.Descriptor.Stats.Wisdom.BaseValue = 18;
                attacker.Descriptor.AddFact(gunslinger.Grit.Feature);
                attacker.Descriptor.Resources.Restore(gunslinger.Grit.Resource, 4);
                gritBefore = attacker.Descriptor.Resources.GetResourceAmount(
                    gunslinger.Grit.Resource);
                weapon = new ItemEntityWeapon(pistol);
                attacker.Body.PrimaryHand.InsertItem(weapon);

                stage = "mixed-volley";
                FirearmRuntimeState.Service.Set(weapon, new FirearmState(
                    FirearmState.CurrentSchemaVersion, 1,
                    FirearmStateTokenCatalog.DiagnosticLeadBall,
                    FirearmCondition.Normal));
                mixed = Deeds.DeadShotRuntime.ExecuteForRuntimeTest(attacker,
                    target, 19, 1, 19);
                gritAfterMixed = attacker.Descriptor.Resources.GetResourceAmount(
                    gunslinger.Grit.Resource);

                stage = "all-misfire-volley";
                attacker.Descriptor.Resources.Restore(gunslinger.Grit.Resource, 1);
                FirearmRuntimeState.Service.Set(weapon, new FirearmState(
                    FirearmState.CurrentSchemaVersion, 1,
                    FirearmStateTokenCatalog.DiagnosticLeadBall,
                    FirearmCondition.Normal));
                allMisfire = Deeds.DeadShotRuntime.ExecuteForRuntimeTest(attacker,
                    target, 1, 1, 1);
                gritAfterMisfire = attacker.Descriptor.Resources.GetResourceAmount(
                    gunslinger.Grit.Resource);
            }
            catch (Exception exception)
            {
                throw new InvalidOperationException(
                    "Disposable Dead Shot failed at stage " + stage + ".", exception);
            }
            finally
            {
                if (weapon != null)
                {
                    FirearmRuntimeState.Service.Forget(weapon);
                    if (attacker != null && attacker.Body.PrimaryHand.MaybeItem != null)
                        attacker.Body.PrimaryHand.RemoveItem(false);
                }
                if (target != null) target.Damage = targetDamageBefore;
                if (target != null) target.Dispose();
                if (attacker != null) attacker.Dispose();
                cleaned = SameReferences(partyBefore, SnapshotReferences(party)) &&
                    SameReferences(unitsBefore, SnapshotReferences(allUnits)) &&
                    (attacker == null || !ContainsReference(allUnits, attacker)) &&
                    (target == null || !ContainsReference(allUnits, target));
            }
            string observed = mixed == null || allMisfire == null ? "missing" :
                "probes=" + mixed.Probes.Length +
                ";bonuses=" + string.Join(",", mixed.Decision.AttackBonuses) +
                ";rolls=" + string.Join(",", mixed.Probes.Select(value =>
                    value.Roll.Value)) + ";hits=" + mixed.Outcome.HitCount +
                ";threats=" + mixed.Outcome.ThreatCount +
                ";packets=" + mixed.Outcome.BaseDamageDicePackets +
                ";mixedCondition=" + mixed.After.Condition +
                ";allMisfire=" + allMisfire.Outcome.Misfires +
                ";misfireCondition=" + allMisfire.After.Condition +
                ";grit=" + gritBefore + "->" + gritAfterMixed +
                "->" + gritAfterMisfire;
            bool mixedContract = mixed != null && mixed.Probes.Length == 3 &&
                mixed.Decision.AttackBonuses.SequenceEqual(new[] { 11, 6, 1 }) &&
                mixed.Probes.Select(value => value.Roll.Value)
                    .SequenceEqual(new[] { 19, 1, 19 }) &&
                mixed.Outcome.HitCount == 2 && mixed.Outcome.ThreatCount == 0 &&
                mixed.Outcome.BaseDamageDicePackets == 2 &&
                !mixed.Outcome.Misfires && mixed.After.IsEmpty &&
                mixed.After.Condition == FirearmCondition.Normal &&
                mixed.Delivery.WeaponStats.WeaponDamageDiceOverride.HasValue &&
                mixed.Delivery.WeaponStats.WeaponDamageDiceOverride.Value.Equals(
                    new DiceFormula(2, mixed.Delivery.Weapon.Damage.Dice));
            bool misfireContract = allMisfire != null &&
                allMisfire.Probes.Select(value => value.Roll.Value)
                    .SequenceEqual(new[] { 1, 1, 1 }) &&
                allMisfire.Outcome.Misfires && !allMisfire.Outcome.IsHit &&
                allMisfire.After.IsEmpty &&
                allMisfire.After.Condition == FirearmCondition.Broken;
            var assertions = new List<RuntimeTestAssertion>
            {
                Assertion("dead-shot-progression", "level 7 full-round weapon ability",
                    observed, blueprintContract,
                    "production progression and BlueprintAbility contract"),
                Assertion("dead-shot-mixed-volley",
                    "BAB 11 rolls 19,1,19; two hits; two base-dice packets; no aggregate misfire",
                    observed, mixedContract,
                    "native probe rolls plus one primary weapon delivery"),
                Assertion("dead-shot-all-misfire",
                    "three natural 1 rolls; one chamber; Normal -> Broken",
                    observed, misfireContract,
                    "aggregate misfire and exact item-state transition"),
                Assertion("dead-shot-cost", "one grit per accepted delivery",
                    observed, gritBefore > 0 && gritAfterMixed == gritBefore - 1 &&
                    gritAfterMisfire == gritAfterMixed,
                    "native per-unit grit resource"),
                Assertion("external-isolation", "unchanged party and global-unit snapshots",
                    "cleaned=" + cleaned, cleaned,
                    "item state forgotten and detached units disposed"),
                Assertion("loaded-mod-version", _request.ExpectedModVersion,
                    _context.ModEntry.Info.Version,
                    _request.ExpectedModVersion == _context.ModEntry.Info.Version,
                    "Unity Mod Manager ModEntry.Info.Version")
            };
            return CreateResult(assertions.TrueForAll(value => value.Status == "PASS")
                ? RuntimeTestStatuses.Pass : RuntimeTestStatuses.Fail, assertions, null);
        }

        private RuntimeTestResult RunDisposableGunslingerScatterShot()
        {
            BlueprintUnit source = BlueprintRoot.Instance.DefaultPlayerCharacter;
            BlueprintItemWeapon blunderbuss =
                BlueprintBootstrap.ProductionFirearms.Blunderbuss.Item;
            object player = ReadExactMember(Kingmaker.Game.Instance, "Player");
            object state = ReadExactMember(Kingmaker.Game.Instance, "State");
            object party = ReadExactMember(player, "Party");
            object allUnits = ReadExactMember(state, "AllUnits");
            object[] partyBefore = SnapshotReferences(party);
            object[] unitsBefore = SnapshotReferences(allUnits);
            int unitPoolBefore = Kingmaker.Game.Instance.State.Units.Count;
            Kingmaker.EntitySystem.Entities.UnitEntityData attacker = null;
            Kingmaker.EntitySystem.Entities.UnitEntityData first = null;
            Kingmaker.EntitySystem.Entities.UnitEntityData second = null;
            ItemEntityWeapon weapon = null;
            Scatter.ScatterShotExecutionResult mixed = null, allMisfire = null;
            int targetCount = -1, registeredCount = -1;
            bool firstRegistered = false, secondRegistered = false,
                cleaned = false;
            try
            {
                attacker = new Kingmaker.UI.LevelUp.ChargenUnit(source).Unit;
                first = new Kingmaker.UI.LevelUp.ChargenUnit(source).Unit;
                second = new Kingmaker.UI.LevelUp.ChargenUnit(source).Unit;
                first.Descriptor.State.Immortality.Retain();
                second.Descriptor.State.Immortality.Retain();
                Vector3 origin = new Vector3(10000f, 0f, 10000f);
                SetExactProperty(attacker, "Position", origin);
                SetExactProperty(first, "Position", origin + new Vector3(2f, 0f, 0.3f));
                SetExactProperty(second, "Position", origin + new Vector3(3f, 0f, -0.3f));
                weapon = new ItemEntityWeapon(blunderbuss);
                attacker.Body.PrimaryHand.InsertItem(weapon);
                firstRegistered = Kingmaker.Game.Instance.State.Units.All.Add(first);
                if (!firstRegistered) throw new InvalidOperationException(
                    "First disposable scatter target was already registered.");
                secondRegistered = Kingmaker.Game.Instance.State.Units.All.Add(second);
                if (!secondRegistered) throw new InvalidOperationException(
                    "Second disposable scatter target was already registered.");
                registeredCount = Kingmaker.Game.Instance.State.Units.Count -
                    unitPoolBefore;
                Kingmaker.EntitySystem.Entities.UnitEntityData[] targets =
                    new Scatter.NativeScatterConeTargetResolver().Resolve(attacker, first);
                targetCount = targets.Length;
                if (targetCount != 2) throw new InvalidOperationException(
                    "Expected exactly two isolated native scatter targets; observed " +
                    targetCount + ".");

                FirearmRuntimeState.Service.Set(weapon, new FirearmState(
                    FirearmState.CurrentSchemaVersion, 1,
                    FirearmStateTokenCatalog.DiagnosticLeadBall,
                    FirearmCondition.Normal));
                mixed = Scatter.ScatterShotRuntime.ExecuteForRuntimeTest(
                    attacker, first, 10, 1);

                FirearmRuntimeState.Service.Set(weapon, new FirearmState(
                    FirearmState.CurrentSchemaVersion, 1,
                    FirearmStateTokenCatalog.DiagnosticLeadBall,
                    FirearmCondition.Normal));
                allMisfire = Scatter.ScatterShotRuntime.ExecuteForRuntimeTest(
                    attacker, first, 1, 1);
            }
            catch (Exception exception)
            {
                throw new InvalidOperationException(
                    "Disposable Scatter Shot transaction failed.", exception);
            }
            finally
            {
                if (weapon != null)
                {
                    FirearmRuntimeState.Service.Forget(weapon);
                    if (attacker != null && attacker.Body.PrimaryHand.MaybeItem != null)
                        attacker.Body.PrimaryHand.RemoveItem(false);
                }
                if (secondRegistered &&
                    !Kingmaker.Game.Instance.State.Units.All.Remove(second))
                    throw new InvalidOperationException(
                        "Second disposable scatter target cleanup failed.");
                if (firstRegistered &&
                    !Kingmaker.Game.Instance.State.Units.All.Remove(first))
                    throw new InvalidOperationException(
                        "First disposable scatter target cleanup failed.");
                if (second != null) second.Descriptor.State.Immortality.ReleaseAll();
                if (first != null) first.Descriptor.State.Immortality.ReleaseAll();
                if (second != null) second.Dispose();
                if (first != null) first.Dispose();
                if (attacker != null) attacker.Dispose();
                cleaned = SameReferences(partyBefore, SnapshotReferences(party)) &&
                    SameReferences(unitsBefore, SnapshotReferences(allUnits)) &&
                    Kingmaker.Game.Instance.State.Units.Count == unitPoolBefore &&
                    !Kingmaker.Game.Instance.State.Units.All.Contains(first) &&
                    !Kingmaker.Game.Instance.State.Units.All.Contains(second);
            }
            string observed = mixed == null || allMisfire == null ? "missing" :
                "registered=" + registeredCount + ";targets=" + targetCount +
                ";mixedMisfires=" +
                mixed.Volley.MisfireRollCount +
                ";mixedCondition=" + mixed.After.Condition +
                ";allMisfires=" + allMisfire.Volley.MisfireRollCount +
                ";allCondition=" + allMisfire.After.Condition;
            bool transaction = registeredCount == 2 && mixed != null &&
                allMisfire != null &&
                mixed.Plan.TargetCount == 2 && mixed.After.IsEmpty &&
                mixed.After.Condition == FirearmCondition.Normal &&
                mixed.Volley.MisfireRollCount == 1 &&
                !mixed.Volley.AllRollsMisfire && allMisfire.After.IsEmpty &&
                allMisfire.After.Condition == FirearmCondition.Broken &&
                allMisfire.Volley.MisfireRollCount == 2 &&
                allMisfire.Volley.AllRollsMisfire;
            var assertions = new List<RuntimeTestAssertion>
            {
                Assertion("scatter-native-transaction",
                    "two native cone targets; rolls 10,1 preserve Normal; rolls 1,1 produce Broken",
                    observed, transaction, "registered Scatter Shot native delivery"),
                Assertion("external-isolation", "unchanged party and global-unit snapshots",
                    "cleaned=" + cleaned, cleaned, "disposable units and item token removed"),
                Assertion("loaded-mod-version", _request.ExpectedModVersion,
                    _context.ModEntry.Info.Version,
                    _request.ExpectedModVersion == _context.ModEntry.Info.Version,
                    "Unity Mod Manager ModEntry.Info.Version")
            };
            return CreateResult(assertions.TrueForAll(value => value.Status == "PASS")
                ? RuntimeTestStatuses.Pass : RuntimeTestStatuses.Fail, assertions, null);
        }

        private RuntimeTestResult RunDisposableGunslingerStartlingShot()
        {
            BlueprintUnit source = BlueprintRoot.Instance.DefaultPlayerCharacter;
            GunslingerClassBlueprintSet gunslinger = BlueprintBootstrap.GunslingerClass;
            BlueprintItemWeapon pistol = BlueprintBootstrap.ProductionFirearms.Pistol.Item;
            object player = ReadExactMember(Kingmaker.Game.Instance, "Player");
            object state = ReadExactMember(Kingmaker.Game.Instance, "State");
            object party = ReadExactMember(player, "Party");
            object allUnits = ReadExactMember(state, "AllUnits");
            object[] partyBefore = SnapshotReferences(party);
            object[] unitsBefore = SnapshotReferences(allUnits);
            Kingmaker.EntitySystem.Entities.UnitEntityData attacker = null;
            Kingmaker.EntitySystem.Entities.UnitEntityData target = null;
            ItemEntityWeapon weapon = null;
            StartlingShotResult result = null;
            int gritBefore = -1, gritAfter = -1;
            int damageBefore = -1, damageAfter = -1;
            int roundsBefore = -1, roundsAfter = -1;
            bool flatBefore = false, flatAfter = false, nativeFlatAfter = false;
            double durationSeconds = -1d;
            bool cleaned = false;
            string stage = "blueprint-contract";
            StartlingShotRuntimeDiagnostics.Reset();
            int levelSevenCount = gunslinger.Progression.LevelEntries[6].Features
                .Count(value => ReferenceEquals(value,
                    gunslinger.StartlingShot.Feature));
            AddCondition condition = gunslinger.StartlingShot.FlatFootedBuff
                .ComponentsArray.OfType<AddCondition>().Single();
            bool blueprintContract = levelSevenCount == 1 &&
                gunslinger.StartlingShot.Ability.ActionType ==
                    UnitCommand.CommandType.Standard &&
                !gunslinger.StartlingShot.Ability.IsFullRoundAction &&
                gunslinger.StartlingShot.Ability.Range == AbilityRange.Weapon &&
                gunslinger.StartlingShot.Ability.CanTargetEnemies &&
                condition.Condition == UnitCondition.LoseDexterityToAC;
            try
            {
                attacker = new Kingmaker.UI.LevelUp.ChargenUnit(source).Unit;
                target = new Kingmaker.UI.LevelUp.ChargenUnit(source).Unit;
                // An unlevelled detached chargen fixture has no positive HP pool.
                // Retain its native immortality so Kingmaker does not classify it
                // as dead and reject the non-StayOnDeath flat-footed buff.
                attacker.Descriptor.Stats.Wisdom.BaseValue = 18;
                attacker.Descriptor.AddFact(gunslinger.Grit.Feature);
                attacker.Descriptor.Resources.Restore(gunslinger.Grit.Resource, 2);
                gritBefore = attacker.Descriptor.Resources.GetResourceAmount(
                    gunslinger.Grit.Resource);
                damageBefore = target.Damage;
                weapon = new ItemEntityWeapon(pistol);
                attacker.Body.PrimaryHand.InsertItem(weapon);
                FirearmRuntimeState.Service.Set(weapon, new FirearmState(
                    FirearmState.CurrentSchemaVersion, 1,
                    FirearmStateTokenCatalog.DiagnosticLeadBall,
                    FirearmCondition.Normal));
                roundsBefore = FirearmRuntimeState.Service.GetOrCreate(weapon)
                    .Repository.State.LoadedRounds;
                flatBefore = target.Descriptor.State.HasCondition(
                    UnitCondition.LoseDexterityToAC);

                stage = "intentional-miss-delivery";
                var abilityData = new Kingmaker.UnitLogic.Abilities.AbilityData(
                    gunslinger.StartlingShot.Ability, attacker.Descriptor);
                var abilityParams = new Kingmaker.UnitLogic.Abilities.AbilityParams();
                var mechanicsContext =
                    new Kingmaker.UnitLogic.Abilities.AbilityExecutionContext(
                        abilityData, abilityParams,
                        new Kingmaker.Utility.TargetWrapper(target), null);
                result = StartlingShotRuntime.Execute(attacker.Descriptor, target,
                    gunslinger.StartlingShot.FlatFootedBuff, mechanicsContext);
                roundsAfter = FirearmRuntimeState.Service.GetOrCreate(weapon)
                    .Repository.State.LoadedRounds;
                gritAfter = attacker.Descriptor.Resources.GetResourceAmount(
                    gunslinger.Grit.Resource);
                damageAfter = target.Damage;
                flatAfter = target.Descriptor.State.HasCondition(
                    UnitCondition.LoseDexterityToAC);
                durationSeconds = result.Buff.TimeLeft.TotalSeconds;
                var check = new RuleCheckTargetFlatFooted(attacker, target);
                Rulebook.Trigger(check);
                nativeFlatAfter = check.IsFlatFooted;
            }
            catch (Exception exception)
            {
                throw new InvalidOperationException(
                    "Disposable Startling Shot failed at stage " + stage + ".",
                    exception);
            }
            finally
            {
                if (result != null && result.Buff != null && target != null)
                    target.Descriptor.Buffs.RemoveFact(result.Buff);
                if (weapon != null)
                {
                    FirearmRuntimeState.Service.Forget(weapon);
                    if (attacker != null && attacker.Body.PrimaryHand.MaybeItem != null)
                        attacker.Body.PrimaryHand.RemoveItem(false);
                }
                if (target != null) target.Damage = Math.Max(0, damageBefore);
                if (target != null) target.Dispose();
                if (attacker != null) attacker.Dispose();
                cleaned = SameReferences(partyBefore, SnapshotReferences(party)) &&
                    SameReferences(unitsBefore, SnapshotReferences(allUnits)) &&
                    (attacker == null || !ContainsReference(allUnits, attacker)) &&
                    (target == null || !ContainsReference(allUnits, target));
            }
            string observed = "rounds=" + roundsBefore + "->" + roundsAfter +
                ";grit=" + gritBefore + "->" + gritAfter + ";damage=" +
                damageBefore + "->" + damageAfter + ";flat=" + flatBefore +
                "->" + flatAfter + ";nativeFlat=" + nativeFlatAfter +
                ";durationSeconds=" + durationSeconds.ToString("F3",
                    System.Globalization.CultureInfo.InvariantCulture) +
                ";applied=" + StartlingShotRuntimeDiagnostics.Applied +
                ";rejected=" + StartlingShotRuntimeDiagnostics.Rejected +
                ";faults=" + StartlingShotRuntimeDiagnostics.Faults;
            var assertions = new List<RuntimeTestAssertion>
            {
                Assertion("startling-shot-progression",
                    "level 7 standard-action weapon ability and native flat-footed buff",
                    observed, blueprintContract,
                    "production progression and BlueprintAbility/BlueprintBuff contracts"),
                Assertion("startling-shot-delivery",
                    "one chamber, zero grit, zero damage",
                    observed, result != null && result.Decision.ShouldApply &&
                    roundsBefore == 1 && roundsAfter == 0 && gritBefore > 0 &&
                    gritAfter == gritBefore && damageAfter == damageBefore,
                    "item-owned discharge without attack or damage event"),
                Assertion("startling-shot-flat-footed",
                    "native LoseDexterityToAC for one round",
                    observed, !flatBefore && flatAfter && nativeFlatAfter &&
                    durationSeconds > 0d && durationSeconds <= 6.1d,
                    "timed BlueprintBuff and RuleCheckTargetFlatFooted"),
                Assertion("startling-shot-diagnostics",
                    "one apply, no rejection or fault", observed,
                    StartlingShotRuntimeDiagnostics.Applied == 1 &&
                    StartlingShotRuntimeDiagnostics.Rejected == 0 &&
                    StartlingShotRuntimeDiagnostics.Faults == 0,
                    "narrow runtime diagnostics"),
                Assertion("external-isolation",
                    "unchanged party and global-unit snapshots",
                    "cleaned=" + cleaned, cleaned,
                    "buff removed, item state forgotten, disposables disposed"),
                Assertion("loaded-mod-version", _request.ExpectedModVersion,
                    _context.ModEntry.Info.Version,
                    _request.ExpectedModVersion == _context.ModEntry.Info.Version,
                    "Unity Mod Manager ModEntry.Info.Version")
            };
            return CreateResult(assertions.TrueForAll(value => value.Status == "PASS")
                ? RuntimeTestStatuses.Pass : RuntimeTestStatuses.Fail, assertions, null);
        }

        private RuntimeTestResult RunDisposableGunslingerTargetingHead()
        {
            BlueprintUnit source = BlueprintRoot.Instance.DefaultPlayerCharacter;
            GunslingerClassBlueprintSet gunslinger = BlueprintBootstrap.GunslingerClass;
            BlueprintItemWeapon pistol = BlueprintBootstrap.ProductionFirearms.Pistol.Item;
            object player = ReadExactMember(Kingmaker.Game.Instance, "Player");
            object state = ReadExactMember(Kingmaker.Game.Instance, "State");
            object party = ReadExactMember(player, "Party");
            object allUnits = ReadExactMember(state, "AllUnits");
            object[] partyBefore = SnapshotReferences(party);
            object[] unitsBefore = SnapshotReferences(allUnits);
            Kingmaker.EntitySystem.Entities.UnitEntityData attacker = null;
            Kingmaker.EntitySystem.Entities.UnitEntityData target = null;
            ItemEntityWeapon weapon = null;
            TargetingHeadResult result = null;
            int gritBefore = -1, gritAfter = -1, roundsBefore = -1,
                roundsAfter = -1, damageBefore = -1, damageAfter = -1;
            bool confusedBefore = false, confusedAfter = false, cleaned = false;
            double durationSeconds = -1d;
            bool buffPermanent = true;
            int nativeDamage = -1;
            string stage = "blueprint-contract";
            int levelSevenCount = gunslinger.Progression.LevelEntries[6].Features
                .Count(value => ReferenceEquals(value,
                    gunslinger.TargetingHead.Feature));
            AddCondition condition = gunslinger.TargetingHead.ConfusionBuff
                .ComponentsArray.OfType<AddCondition>().Single();
            SpellDescriptorComponent descriptor = gunslinger.TargetingHead
                .ConfusionBuff.ComponentsArray.OfType<SpellDescriptorComponent>()
                .Single();
            bool blueprintContract = levelSevenCount == 1 &&
                gunslinger.TargetingHead.Ability.IsFullRoundAction &&
                gunslinger.TargetingHead.Ability.Range == AbilityRange.Weapon &&
                condition.Condition == UnitCondition.Confusion &&
                descriptor.Descriptor.HasAnyFlag(SpellDescriptor.MindAffecting);
            try
            {
                attacker = new Kingmaker.UI.LevelUp.ChargenUnit(source).Unit;
                target = new Kingmaker.UI.LevelUp.ChargenUnit(source).Unit;
                // Keep the unlevelled detached target alive without publishing it
                // to game state; otherwise native timed buffs reject the dead unit.
                attacker.Descriptor.Stats.Wisdom.BaseValue = 18;
                attacker.Descriptor.AddFact(gunslinger.Grit.Feature);
                attacker.Descriptor.Resources.Restore(gunslinger.Grit.Resource, 2);
                gritBefore = attacker.Descriptor.Resources.GetResourceAmount(
                    gunslinger.Grit.Resource);
                damageBefore = target.Damage;
                confusedBefore = target.Descriptor.State.HasCondition(
                    UnitCondition.Confusion);
                weapon = new ItemEntityWeapon(pistol);
                attacker.Body.PrimaryHand.InsertItem(weapon);
                FirearmRuntimeState.Service.Set(weapon, new FirearmState(
                    FirearmState.CurrentSchemaVersion, 1,
                    FirearmStateTokenCatalog.DiagnosticLeadBall,
                    FirearmCondition.Normal));
                roundsBefore = FirearmRuntimeState.Service.GetOrCreate(weapon)
                    .Repository.State.LoadedRounds;

                stage = "native-head-attack";
                var abilityData = new Kingmaker.UnitLogic.Abilities.AbilityData(
                    gunslinger.TargetingHead.Ability, attacker.Descriptor);
                var abilityParams = new Kingmaker.UnitLogic.Abilities.AbilityParams();
                var mechanicsContext =
                    new Kingmaker.UnitLogic.Abilities.AbilityExecutionContext(
                        abilityData, abilityParams,
                        new Kingmaker.Utility.TargetWrapper(target), null);
                result = TargetingHeadRuntime.ExecuteForRuntimeTest(attacker,
                    target, gunslinger.TargetingHead.ConfusionBuff,
                    mechanicsContext, true);
                gritAfter = attacker.Descriptor.Resources.GetResourceAmount(
                    gunslinger.Grit.Resource);
                roundsAfter = FirearmRuntimeState.Service.GetOrCreate(weapon)
                    .Repository.State.LoadedRounds;
                damageAfter = target.Damage;
                confusedAfter = target.Descriptor.State.HasCondition(
                    UnitCondition.Confusion);
                nativeDamage = damageAfter - damageBefore;
                if (result.Buff != null)
                {
                    durationSeconds = result.Buff.TimeLeft.TotalSeconds;
                    buffPermanent = result.Buff.IsPermanent;
                }
            }
            catch (Exception exception)
            {
                throw new InvalidOperationException(
                    "Disposable Targeting Head failed at stage " + stage + ".",
                    exception);
            }
            finally
            {
                if (result != null && result.Buff != null && target != null)
                    target.Descriptor.Buffs.RemoveFact(result.Buff);
                if (weapon != null)
                {
                    FirearmRuntimeState.Service.Forget(weapon);
                    if (attacker != null && attacker.Body.PrimaryHand.MaybeItem != null)
                        attacker.Body.PrimaryHand.RemoveItem(false);
                }
                if (target != null) target.Damage = Math.Max(0, damageBefore);
                if (target != null) target.Dispose();
                if (attacker != null) attacker.Dispose();
                cleaned = SameReferences(partyBefore, SnapshotReferences(party)) &&
                    SameReferences(unitsBefore, SnapshotReferences(allUnits)) &&
                    (attacker == null || !ContainsReference(allUnits, attacker)) &&
                    (target == null || !ContainsReference(allUnits, target));
            }
            string observed = "rounds=" + roundsBefore + "->" + roundsAfter +
                ";grit=" + gritBefore + "->" + gritAfter + ";damage=" +
                damageBefore + "->" + damageAfter + ";confused=" +
                confusedBefore + "->" + confusedAfter + ";durationSeconds=" +
                durationSeconds.ToString("F3",
                    System.Globalization.CultureInfo.InvariantCulture) +
                ";buffPermanent=" + buffPermanent + ";nativeDamage=" +
                nativeDamage +
                ";hit=" + (result != null && result.Hit) + ";immune=" +
                (result != null && result.Rider != null &&
                    result.Rider.ImmuneToSneakAttack);
            var assertions = new List<RuntimeTestAssertion>
            {
                Assertion("targeting-head-progression",
                    "level 7 full-round weapon ability and mind-affecting Confusion buff",
                    observed, blueprintContract, "production blueprint contracts"),
                Assertion("targeting-head-attack",
                    "one grit, one chamber, ordinary hit damage", observed,
                    result != null && result.Hit && gritAfter == gritBefore - 1 &&
                    roundsBefore == 1 && roundsAfter == 0 &&
                    nativeDamage > 0,
                    "authoritative target damage delta after native firearm delivery"),
                Assertion("targeting-head-rider",
                    "one-round native Confusion on non-immune hit", observed,
                    result != null && result.Rider != null &&
                    result.Rider.ShouldConfuse && result.Buff != null &&
                    !confusedBefore && confusedAfter && !buffPermanent,
                    "native sneak-immunity result and timed mind-affecting buff"),
                Assertion("external-isolation",
                    "unchanged party and global-unit snapshots", "cleaned=" + cleaned,
                    cleaned, "buff/item state removed and disposables disposed"),
                Assertion("loaded-mod-version", _request.ExpectedModVersion,
                    _context.ModEntry.Info.Version,
                    _request.ExpectedModVersion == _context.ModEntry.Info.Version,
                    "Unity Mod Manager ModEntry.Info.Version")
            };
            return CreateResult(assertions.TrueForAll(value => value.Status == "PASS")
                ? RuntimeTestStatuses.Pass : RuntimeTestStatuses.Fail, assertions, null);
        }

        private RuntimeTestResult RunDisposableGunslingerExpertLoading()
        {
            BlueprintUnit source = BlueprintRoot.Instance.DefaultPlayerCharacter;
            GunslingerClassBlueprintSet gunslinger = BlueprintBootstrap.GunslingerClass;
            ExpertLoadingBlueprintSet set = gunslinger.ExpertLoading;
            BlueprintItemWeapon pistol = BlueprintBootstrap.ProductionFirearms.Pistol.Item;
            object player = ReadExactMember(Kingmaker.Game.Instance, "Player");
            object state = ReadExactMember(Kingmaker.Game.Instance, "State");
            object party = ReadExactMember(player, "Party");
            object allUnits = ReadExactMember(state, "AllUnits");
            object[] partyBefore = SnapshotReferences(party);
            object[] unitsBefore = SnapshotReferences(allUnits);
            Kingmaker.EntitySystem.Entities.UnitEntityData attacker = null;
            Kingmaker.EntitySystem.Entities.UnitEntityData target = null;
            ItemEntityWeapon weapon = null;
            int gritBefore = -1, gritSuppressed = -1, gritAfter = -1;
            int suppressedRounds = -1, ordinaryRounds = -1;
            FirearmCondition suppressedCondition = FirearmCondition.Normal;
            FirearmCondition ordinaryCondition = FirearmCondition.Normal;
            long scheduledBefore = -1, scheduledSuppressed = -1,
                scheduledAfter = -1;
            bool markerConsumed = false, cleaned = false;
            string stage = "blueprint-contract";
            bool blueprintContract = gunslinger.Progression.LevelEntries[10]
                .Features.Count(value => ReferenceEquals(value, set.Feature)) == 1 &&
                set.Ability.ActionType == UnitCommand.CommandType.Free;
            try
            {
                attacker = new Kingmaker.UI.LevelUp.ChargenUnit(source).Unit;
                target = new Kingmaker.UI.LevelUp.ChargenUnit(source).Unit;
                attacker.Descriptor.State.Immortality.Retain();
                attacker.Descriptor.Stats.Constitution.BaseValue = 30;
                attacker.Descriptor.Stats.Wisdom.BaseValue = 18;
                attacker.Descriptor.AddFact(gunslinger.Grit.Feature);
                attacker.Descriptor.Resources.Restore(gunslinger.Grit.Resource, 2);
                gritBefore = attacker.Descriptor.Resources.GetResourceAmount(
                    gunslinger.Grit.Resource);
                weapon = new ItemEntityWeapon(pistol);
                attacker.Body.PrimaryHand.InsertItem(weapon);
                scheduledBefore = FirearmExplosionRuntimeDiagnostics.Scheduled;

                stage = "armed-suppression";
                var context = new MechanicsContext(attacker,
                    attacker.Descriptor, set.Ability, null,
                    new TargetWrapper(attacker));
                attacker.Descriptor.Buffs.AddBuff(set.ArmedMarker, context, null);
                FirearmRuntimeState.Service.Set(weapon, new FirearmState(
                    FirearmState.CurrentSchemaVersion, 1,
                    FirearmStateTokenCatalog.DiagnosticLeadBall,
                    FirearmCondition.Broken));
                FirearmMisfireRuntime.QueueForcedNaturalRoll(1);
                Rulebook.Trigger(new RuleAttackWithWeapon(attacker, target,
                    weapon, 0));
                FirearmItemStateSnapshot suppressed =
                    FirearmRuntimeState.Service.GetOrCreate(weapon);
                suppressedCondition = suppressed.Repository.State.Condition;
                suppressedRounds = suppressed.Repository.State.LoadedRounds;
                gritSuppressed = attacker.Descriptor.Resources.GetResourceAmount(
                    gunslinger.Grit.Resource);
                scheduledSuppressed = FirearmExplosionRuntimeDiagnostics.Scheduled;
                markerConsumed = !attacker.Descriptor.Buffs.RawFacts.OfType<Buff>()
                    .Any(value => ReferenceEquals(value.Blueprint,
                        set.ArmedMarker));

                stage = "ordinary-explosion";
                attacker.Descriptor.Resources.Spend(gunslinger.Grit.Resource,
                    gritSuppressed);
                FirearmRuntimeState.Service.Set(weapon, new FirearmState(
                    FirearmState.CurrentSchemaVersion, 1,
                    FirearmStateTokenCatalog.DiagnosticLeadBall,
                    FirearmCondition.Broken));
                FirearmMisfireRuntime.QueueForcedNaturalRoll(1);
                Rulebook.Trigger(new RuleAttackWithWeapon(attacker, target,
                    weapon, 0));
                FirearmItemStateSnapshot ordinary =
                    FirearmRuntimeState.Service.GetOrCreate(weapon);
                ordinaryCondition = ordinary.Repository.State.Condition;
                ordinaryRounds = ordinary.Repository.State.LoadedRounds;
                gritAfter = attacker.Descriptor.Resources.GetResourceAmount(
                    gunslinger.Grit.Resource);
                scheduledAfter = FirearmExplosionRuntimeDiagnostics.Scheduled;
            }
            catch (Exception exception)
            {
                throw new InvalidOperationException(
                    "Disposable Expert Loading failed at stage " + stage + ".",
                    exception);
            }
            finally
            {
                FirearmMisfireRuntime.CancelForcedNaturalRoll();
                if (weapon != null)
                {
                    FirearmRuntimeState.Service.Forget(weapon);
                    if (attacker != null && attacker.Body.PrimaryHand.MaybeItem != null)
                        attacker.Body.PrimaryHand.RemoveItem(false);
                }
                if (target != null) target.Dispose();
                if (attacker != null) attacker.Dispose();
                cleaned = SameReferences(partyBefore, SnapshotReferences(party)) &&
                    SameReferences(unitsBefore, SnapshotReferences(allUnits)) &&
                    (attacker == null || !ContainsReference(allUnits, attacker)) &&
                    (target == null || !ContainsReference(allUnits, target));
            }
            string observed = "grit=" + gritBefore + "->" + gritSuppressed +
                "->" + gritAfter + ";conditions=" + suppressedCondition +
                "," + ordinaryCondition + ";rounds=" + suppressedRounds +
                "," + ordinaryRounds + ";scheduled=" + scheduledBefore +
                "->" + scheduledSuppressed + "->" + scheduledAfter +
                ";markerConsumed=" + markerConsumed;
            var assertions = new List<RuntimeTestAssertion>
            {
                Assertion("expert-loading-progression",
                    "level 11 feature and free-action ability", observed,
                    blueprintContract, "production blueprint contracts"),
                Assertion("expert-loading-suppression",
                    "one grit; empty Broken; no scheduled burst; marker consumed",
                    observed, gritSuppressed == gritBefore - 1 &&
                        suppressedCondition == FirearmCondition.Broken &&
                        suppressedRounds == 0 &&
                        scheduledSuppressed == scheduledBefore && markerConsumed,
                    "exact first misfire evaluation"),
                Assertion("expert-loading-fail-closed",
                    "no grit: empty Wrecked and one scheduled burst", observed,
                    gritAfter == 0 && ordinaryCondition == FirearmCondition.Wrecked &&
                        ordinaryRounds == 0 &&
                        scheduledAfter == scheduledSuppressed + 1,
                    "ordinary Broken-to-Wrecked explosion pipeline"),
                Assertion("external-isolation",
                    "unchanged party and global-unit snapshots", "cleaned=" + cleaned,
                    cleaned, "item state forgotten and disposables disposed"),
                Assertion("loaded-mod-version", _request.ExpectedModVersion,
                    _context.ModEntry.Info.Version,
                    _request.ExpectedModVersion == _context.ModEntry.Info.Version,
                    "Unity Mod Manager ModEntry.Info.Version")
            };
            return CreateResult(assertions.TrueForAll(value => value.Status == "PASS")
                ? RuntimeTestStatuses.Pass : RuntimeTestStatuses.Fail,
                assertions, null);
        }

        private RuntimeTestResult RunDisposableGunslingerLightningReload()
        {
            BlueprintUnit source = BlueprintRoot.Instance.DefaultPlayerCharacter;
            GunslingerClassBlueprintSet gunslinger = BlueprintBootstrap.GunslingerClass;
            LightningReloadBlueprintSet set = gunslinger.LightningReload;
            BlueprintItemWeapon pistol = BlueprintBootstrap.ProductionFirearms.Pistol.Item;
            BasicAmmunitionBlueprintSet ammunition = BlueprintBootstrap.BasicAmmunition;
            object player = ReadExactMember(Kingmaker.Game.Instance, "Player");
            object state = ReadExactMember(Kingmaker.Game.Instance, "State");
            object party = ReadExactMember(player, "Party");
            object allUnits = ReadExactMember(state, "AllUnits");
            object[] partyBefore = SnapshotReferences(party);
            object[] unitsBefore = SnapshotReferences(allUnits);
            Kingmaker.EntitySystem.Entities.UnitEntityData attacker = null;
            ItemEntityWeapon weapon = null;
            KingmakerBasicAmmunitionInventory inventory = null;
            BasicAmmunitionInventorySnapshot inventoryBefore = null;
            int gritBefore = -1, gritAfter = -1, normalRounds = -1,
                brokenRounds = -1;
            bool marked = false, sameRoundRejected = false,
                roundReset = false, noGritRejected = false, cleaned = false;
            FirearmCondition brokenCondition = FirearmCondition.Normal;
            string stage = "blueprint-contract";
            bool blueprintContract = gunslinger.Progression.LevelEntries[10]
                .Features.Count(value => ReferenceEquals(value, set.Feature)) == 1 &&
                set.Ability.ActionType == UnitCommand.CommandType.Swift &&
                set.UsedMarker.GetComponent<LightningReloadRoundMarker>() != null;
            try
            {
                attacker = new Kingmaker.UI.LevelUp.ChargenUnit(source).Unit;
                attacker.Descriptor.Stats.Wisdom.BaseValue = 18;
                attacker.Descriptor.AddFact(gunslinger.Grit.Feature);
                attacker.Descriptor.Resources.Restore(gunslinger.Grit.Resource, 2);
                gritBefore = attacker.Descriptor.Resources.GetResourceAmount(
                    gunslinger.Grit.Resource);
                weapon = new ItemEntityWeapon(pistol);
                attacker.Body.PrimaryHand.InsertItem(weapon);
                inventory = new KingmakerBasicAmmunitionInventory(
                    Kingmaker.Game.Instance.Player.Inventory,
                    ammunition.BlackPowder, ammunition.LeadBall);
                inventoryBefore = BasicAmmunitionInventorySnapshot.Capture(inventory);
                inventory.Add(BasicAmmunitionComponent.BlackPowderCharge, 2);
                inventory.Add(BasicAmmunitionComponent.LeadBall, 2);
                var abilityData = new Kingmaker.UnitLogic.Abilities.AbilityData(
                    set.Ability, attacker.Descriptor);
                var execution = new Kingmaker.UnitLogic.Abilities.AbilityExecutionContext(
                    abilityData, new Kingmaker.UnitLogic.Abilities.AbilityParams(),
                    new TargetWrapper(attacker), null);

                stage = "first-swift-reload";
                LightningReloadRuntime.Execute(attacker.Descriptor, execution,
                    ammunition.BlackPowder, ammunition.LeadBall, set.UsedMarker);
                normalRounds = FirearmRuntimeState.Service.GetOrCreate(weapon)
                    .Repository.State.LoadedRounds;
                gritAfter = attacker.Descriptor.Resources.GetResourceAmount(
                    gunslinger.Grit.Resource);
                Buff marker = attacker.Descriptor.Buffs.RawFacts.OfType<Buff>()
                    .SingleOrDefault(value => ReferenceEquals(value.Blueprint,
                        set.UsedMarker));
                marked = marker != null;
                sameRoundRejected = LightningReloadRuntime.Evaluate(
                    attacker.Descriptor, ammunition.BlackPowder,
                    ammunition.LeadBall, set.UsedMarker).Decision.Status ==
                    LightningReloadStatus.UsedThisRound;

                stage = "native-round-reset";
                LightningReloadRoundMarker reset = marker == null ? null :
                    marker.Get<LightningReloadRoundMarker>();
                if (reset != null) reset.OnNewRound();
                roundReset = !attacker.Descriptor.Buffs.RawFacts.OfType<Buff>()
                    .Any(value => ReferenceEquals(value.Blueprint, set.UsedMarker));
                FirearmRuntimeState.Service.Set(weapon, new FirearmState(
                    FirearmState.CurrentSchemaVersion, 0, null,
                    FirearmCondition.Broken));
                LightningReloadRuntime.Execute(attacker.Descriptor, execution,
                    ammunition.BlackPowder, ammunition.LeadBall, set.UsedMarker);
                FirearmState broken = FirearmRuntimeState.Service.GetOrCreate(weapon)
                    .Repository.State;
                brokenRounds = broken.LoadedRounds;
                brokenCondition = broken.Condition;

                stage = "zero-grit-gate";
                Buff second = attacker.Descriptor.Buffs.RawFacts.OfType<Buff>()
                    .Single(value => ReferenceEquals(value.Blueprint, set.UsedMarker));
                second.Get<LightningReloadRoundMarker>().OnNewRound();
                attacker.Descriptor.Resources.Spend(gunslinger.Grit.Resource,
                    gritAfter);
                FirearmRuntimeState.Service.Set(weapon, FirearmState.CreateEmpty());
                noGritRejected = LightningReloadRuntime.Evaluate(
                    attacker.Descriptor, ammunition.BlackPowder,
                    ammunition.LeadBall, set.UsedMarker).Decision.Status ==
                    LightningReloadStatus.NoGrit;
            }
            catch (Exception exception)
            {
                throw new InvalidOperationException(
                    "Disposable Lightning Reload failed at stage " + stage + ".",
                    exception);
            }
            finally
            {
                if (inventory != null && inventoryBefore != null)
                    new BasicAmmunitionTransactionService().RestoreExact(
                        inventory, inventoryBefore);
                if (weapon != null)
                {
                    FirearmRuntimeState.Service.Forget(weapon);
                    if (attacker != null && attacker.Body.PrimaryHand.MaybeItem != null)
                        attacker.Body.PrimaryHand.RemoveItem(false);
                }
                if (attacker != null) attacker.Dispose();
                cleaned = SameReferences(partyBefore, SnapshotReferences(party)) &&
                    SameReferences(unitsBefore, SnapshotReferences(allUnits)) &&
                    (attacker == null || !ContainsReference(allUnits, attacker));
            }
            string observed = "grit=" + gritBefore + "->" + gritAfter +
                ";rounds=" + normalRounds + "," + brokenRounds +
                ";broken=" + brokenCondition + ";marked=" + marked +
                ";sameRoundRejected=" + sameRoundRejected +
                ";roundReset=" + roundReset + ";noGrit=" + noGritRejected;
            var assertions = new List<RuntimeTestAssertion>
            {
                Assertion("lightning-reload-progression",
                    "level 11 feature and swift-action ability", observed,
                    blueprintContract, "production blueprint contracts"),
                Assertion("lightning-reload-first-use",
                    "one chamber; no grit spend; unit marked", observed,
                    normalRounds == 1 && gritAfter == gritBefore && marked,
                    "atomic production reload transaction"),
                Assertion("lightning-reload-round-gate",
                    "same-round rejection and next-round reset", observed,
                    sameRoundRejected && roundReset,
                    "bound ITickEachRound marker component"),
                Assertion("lightning-reload-broken-and-grit",
                    "Broken preserved; zero grit rejected", observed,
                    brokenRounds == 1 && brokenCondition == FirearmCondition.Broken &&
                        noGritRejected, "second atomic reload and policy gate"),
                Assertion("external-isolation",
                    "unchanged inventory, party, and global-unit snapshots",
                    "cleaned=" + cleaned, cleaned,
                    "inventory restored, item state forgotten, disposable disposed"),
                Assertion("loaded-mod-version", _request.ExpectedModVersion,
                    _context.ModEntry.Info.Version,
                    _request.ExpectedModVersion == _context.ModEntry.Info.Version,
                    "Unity Mod Manager ModEntry.Info.Version")
            };
            return CreateResult(assertions.TrueForAll(value => value.Status == "PASS")
                ? RuntimeTestStatuses.Pass : RuntimeTestStatuses.Fail,
                assertions, null);
        }

        private RuntimeTestResult RunDisposableGunslingerEvasive()
        {
            BlueprintUnit source = BlueprintRoot.Instance.DefaultPlayerCharacter;
            GunslingerClassBlueprintSet gunslinger = BlueprintBootstrap.GunslingerClass;
            EvasiveBlueprintSet set = gunslinger.Evasive;
            object player = ReadExactMember(Kingmaker.Game.Instance, "Player");
            object state = ReadExactMember(Kingmaker.Game.Instance, "State");
            object party = ReadExactMember(player, "Party");
            object allUnits = ReadExactMember(state, "AllUnits");
            object[] partyBefore = SnapshotReferences(party);
            object[] unitsBefore = SnapshotReferences(allUnits);
            Kingmaker.EntitySystem.Entities.UnitEntityData unit = null;
            Kingmaker.EntitySystem.Entities.UnitEntityData other = null;
            object controller = null;
            int level = -1, initialGrit = -1, zeroGrit = -1, restoredGrit = -1;
            bool initialBenefits = false, zeroBenefits = false,
                restoredBenefits = false, otherIsolated = false, cleaned = false;
            string stage = "blueprint-contract";
            bool blueprintContract = gunslinger.Progression.LevelEntries[14]
                .Features.Count(value => ReferenceEquals(value, set.Feature)) == 1 &&
                set.Evasion.ComponentsArray.Length == 1 &&
                set.UncannyDodge.ComponentsArray.Length == 2 &&
                set.ImprovedUncannyDodge.ComponentsArray.Length == 1;
            try
            {
                unit = new Kingmaker.UI.LevelUp.ChargenUnit(source).Unit;
                other = new Kingmaker.UI.LevelUp.ChargenUnit(source).Unit;
                unit.Descriptor.Stats.Wisdom.BaseValue = 18;
                Type type = typeof(Kingmaker.UnitLogic.Class.LevelUp.LevelUpController);
                MethodInfo start = type.GetMethods(BindingFlags.Public |
                    BindingFlags.NonPublic | BindingFlags.Static).Single(value =>
                        value.Name == "StartWithoutAssigningStaticInstance" &&
                        value.GetParameters().Length == 5);
                MethodInfo selectClass = type.GetMethod("SelectClass",
                    BindingFlags.Public | BindingFlags.Instance, null,
                    new[] { typeof(BlueprintCharacterClass), typeof(bool) }, null);
                MethodInfo mechanics = type.GetMethod("ApplyClassMechanics",
                    BindingFlags.Public | BindingFlags.Instance);
                MethodInfo apply = type.GetMethod("ApplyLevelup", BindingFlags.Public |
                    BindingFlags.NonPublic | BindingFlags.Instance);
                MethodInfo cancel = type.GetMethod("Cancel", BindingFlags.Public |
                    BindingFlags.Instance);
                if (selectClass == null || mechanics == null || apply == null || cancel == null)
                    throw new MissingMethodException(
                        "An exact native Evasive level-up method is unavailable.");
                object charGen = Enum.Parse(start.GetParameters()[4].ParameterType,
                    "CharGen", false);
                stage = "native-level-fifteen";
                for (int index = 0; index < 15; index++)
                {
                    controller = start.Invoke(null,
                        new object[] { unit.Descriptor, false, null, null, charGen });
                    if (!(bool)selectClass.Invoke(controller,
                        new object[] { gunslinger.CharacterClass, false }))
                        throw new InvalidOperationException(
                            "Disposable Evasive Gunslinger selection was rejected at level " +
                            (index + 1) + ".");
                    mechanics.Invoke(controller, null);
                    apply.Invoke(controller, new object[] { unit.Descriptor });
                    cancel.Invoke(controller, null);
                    controller = null;
                }
                level = unit.Descriptor.Progression.GetClassLevel(
                    gunslinger.CharacterClass);
                initialGrit = unit.Descriptor.Resources.GetResourceAmount(
                    gunslinger.Grit.Resource);
                initialBenefits = HasAllEvasiveBenefits(unit.Descriptor, set);

                stage = "zero-grit-removal";
                unit.Descriptor.Resources.Spend(gunslinger.Grit.Resource,
                    initialGrit);
                zeroGrit = unit.Descriptor.Resources.GetResourceAmount(
                    gunslinger.Grit.Resource);
                zeroBenefits = HasAnyEvasiveBenefit(unit.Descriptor, set);

                stage = "positive-grit-restoration";
                unit.Descriptor.Resources.Restore(gunslinger.Grit.Resource, 1);
                restoredGrit = unit.Descriptor.Resources.GetResourceAmount(
                    gunslinger.Grit.Resource);
                restoredBenefits = HasAllEvasiveBenefits(unit.Descriptor, set);
                otherIsolated = !HasAnyEvasiveBenefit(other.Descriptor, set);
            }
            catch (Exception exception)
            {
                throw new InvalidOperationException(
                    "Disposable Evasive failed at stage " + stage + ".", exception);
            }
            finally
            {
                if (controller != null)
                {
                    MethodInfo cancel = controller.GetType().GetMethod("Cancel",
                        BindingFlags.Public | BindingFlags.Instance);
                    if (cancel != null) cancel.Invoke(controller, null);
                }
                if (unit != null) unit.Dispose();
                if (other != null) other.Dispose();
                cleaned = SameReferences(partyBefore, SnapshotReferences(party)) &&
                    SameReferences(unitsBefore, SnapshotReferences(allUnits)) &&
                    (unit == null || !ContainsReference(allUnits, unit)) &&
                    (other == null || !ContainsReference(allUnits, other));
            }
            string observed = "level=" + level + ";grit=" + initialGrit + "->" +
                zeroGrit + "->" + restoredGrit + ";benefits=" + initialBenefits +
                "," + zeroBenefits + "," + restoredBenefits +
                ";otherIsolated=" + otherIsolated;
            var assertions = new List<RuntimeTestAssertion>
            {
                Assertion("evasive-progression", "level 15 exact native clones",
                    observed, blueprintContract && level == 15,
                    "production progression and cloned component contracts"),
                Assertion("evasive-positive-grit", "all three benefits active",
                    observed, initialGrit > 0 && initialBenefits,
                    "native level-up and conditional grant controller"),
                Assertion("evasive-grit-transitions", "zero removes; restore adds",
                    observed, zeroGrit == 0 && !zeroBenefits && restoredGrit == 1 &&
                        restoredBenefits, "exact grit Spend/Restore Harmony refresh"),
                Assertion("evasive-unit-isolation", "other unit has no benefits",
                    observed, otherIsolated, "project-owned benefit identities"),
                Assertion("external-isolation", "unchanged party and global-unit snapshots",
                    "cleaned=" + cleaned, cleaned, "disposable units disposed"),
                Assertion("loaded-mod-version", _request.ExpectedModVersion,
                    _context.ModEntry.Info.Version,
                    _request.ExpectedModVersion == _context.ModEntry.Info.Version,
                    "Unity Mod Manager ModEntry.Info.Version")
            };
            return CreateResult(assertions.TrueForAll(value => value.Status == "PASS")
                ? RuntimeTestStatuses.Pass : RuntimeTestStatuses.Fail,
                assertions, null);
        }

        private static bool HasAllEvasiveBenefits(UnitDescriptor owner,
            EvasiveBlueprintSet set)
        {
            return owner.HasFact(set.Evasion) && owner.HasFact(set.UncannyDodge) &&
                owner.HasFact(set.ImprovedUncannyDodge);
        }

        private static bool HasAnyEvasiveBenefit(UnitDescriptor owner,
            EvasiveBlueprintSet set)
        {
            return owner.HasFact(set.Evasion) || owner.HasFact(set.UncannyDodge) ||
                owner.HasFact(set.ImprovedUncannyDodge);
        }

        private RuntimeTestResult RunDisposableGunslingerMenacingShot()
        {
            BlueprintUnit source = BlueprintRoot.Instance.DefaultPlayerCharacter;
            GunslingerClassBlueprintSet gunslinger = BlueprintBootstrap.GunslingerClass;
            MenacingShotBlueprintSet set = gunslinger.MenacingShot;
            BlueprintItemWeapon pistol = BlueprintBootstrap.ProductionFirearms.Pistol.Item;
            BlueprintBuff frightened = BlueprintLibraryLookup.RequireExact<BlueprintBuff>(
                BlueprintBootstrap.Library, "f08a7239aa961f34c8301518e71d4cdf",
                "native Frightened buff");
            BlueprintBuff shaken = BlueprintLibraryLookup.RequireExact<BlueprintBuff>(
                BlueprintBootstrap.Library, "25ec6cb6ab1845c48a95f9c20b034220",
                "native Shaken buff");
            object player = ReadExactMember(Kingmaker.Game.Instance, "Player");
            object state = ReadExactMember(Kingmaker.Game.Instance, "State");
            object party = ReadExactMember(player, "Party");
            object allUnits = ReadExactMember(state, "AllUnits");
            object[] partyBefore = SnapshotReferences(party);
            object[] unitsBefore = SnapshotReferences(allUnits);
            Kingmaker.EntitySystem.Entities.UnitEntityData failed = null;
            Kingmaker.EntitySystem.Entities.UnitEntityData passed = null;
            ItemEntityWeapon failedWeapon = null, passedWeapon = null;
            object controller = null;
            int failedGritBefore = -1, failedGritAfter = -1,
                passedGritBefore = -1, passedGritAfter = -1;
            int failedRounds = -1, passedRounds = -1, failedTargets = -1,
                passedTargets = -1, dc = -1, casterLevel = -1;
            bool frightenedApplied = false, shakenApplied = false,
                nativeContract = false, cleaned = false;
            string stage = "blueprint-contract";
            try
            {
                failed = new Kingmaker.UI.LevelUp.ChargenUnit(source).Unit;
                passed = new Kingmaker.UI.LevelUp.ChargenUnit(source).Unit;
                failed.Descriptor.Stats.Wisdom.BaseValue = 18;
                passed.Descriptor.Stats.Wisdom.BaseValue = 18;
                AdvanceDisposableGunslinger(failed.Descriptor, gunslinger, 15,
                    ref controller);
                AdvanceDisposableGunslinger(passed.Descriptor, gunslinger, 15,
                    ref controller);
                SetExactProperty(failed.Descriptor.Stats.GetStat(StatType.SaveWill),
                    "BaseValue", -100);
                SetExactProperty(passed.Descriptor.Stats.GetStat(StatType.SaveWill),
                    "BaseValue", 100);
                failedWeapon = new ItemEntityWeapon(pistol);
                passedWeapon = new ItemEntityWeapon(pistol);
                failed.Body.PrimaryHand.InsertItem(failedWeapon);
                passed.Body.PrimaryHand.InsertItem(passedWeapon);
                SetRuntimeLoadedRound(failedWeapon);
                SetRuntimeLoadedRound(passedWeapon);
                failedGritBefore = failed.Descriptor.Resources.GetResourceAmount(
                    gunslinger.Grit.Resource);
                passedGritBefore = passed.Descriptor.Resources.GetResourceAmount(
                    gunslinger.Grit.Resource);
                AbilityEffectRunAction effect = set.Ability.ComponentsArray
                    .OfType<AbilityEffectRunAction>().Single();
                MenacingShotAbilityLogic logic = set.Ability.ComponentsArray
                    .OfType<MenacingShotAbilityLogic>().Single();
                string nested = DescribeNestedObject(effect, 10);
                nativeContract = nested.Contains(
                    "f08a7239aa961f34c8301518e71d4cdf") && nested.Contains(
                    "25ec6cb6ab1845c48a95f9c20b034220") && nested.Contains(
                    "SavingThrowType=Will");

                stage = "failed-save-delivery";
                var failedData = new Kingmaker.UnitLogic.Abilities.AbilityData(
                    set.Ability, failed.Descriptor);
                var failedContext = new Kingmaker.UnitLogic.Abilities.AbilityExecutionContext(
                    failedData, new Kingmaker.UnitLogic.Abilities.AbilityParams(),
                    new TargetWrapper(failed), null);
                failedTargets = 0;
                IEnumerator<AbilityDeliveryTarget> failedDelivery = logic.Deliver(
                    failedContext, new TargetWrapper(failed));
                while (failedDelivery.MoveNext()) failedTargets++;
                dc = failedContext.Params.DC;
                casterLevel = failedContext.Params.CasterLevel;
                UnityEngine.Random.InitState(FindNativeD20Seed(1));
                effect.Apply(failedContext, new TargetWrapper(failed));
                frightenedApplied = failed.Descriptor.Buffs.RawFacts.OfType<Buff>()
                    .Any(value => ReferenceEquals(value.Blueprint, frightened));
                failedGritAfter = failed.Descriptor.Resources.GetResourceAmount(
                    gunslinger.Grit.Resource);
                failedRounds = FirearmRuntimeState.Service.GetOrCreate(failedWeapon)
                    .Repository.State.LoadedRounds;

                stage = "successful-save-delivery";
                var passedData = new Kingmaker.UnitLogic.Abilities.AbilityData(
                    set.Ability, passed.Descriptor);
                var passedContext = new Kingmaker.UnitLogic.Abilities.AbilityExecutionContext(
                    passedData, new Kingmaker.UnitLogic.Abilities.AbilityParams(),
                    new TargetWrapper(passed), null);
                passedTargets = 0;
                IEnumerator<AbilityDeliveryTarget> passedDelivery = logic.Deliver(
                    passedContext, new TargetWrapper(passed));
                while (passedDelivery.MoveNext()) passedTargets++;
                UnityEngine.Random.InitState(FindNativeD20Seed(20));
                effect.Apply(passedContext, new TargetWrapper(passed));
                shakenApplied = passed.Descriptor.Buffs.RawFacts.OfType<Buff>()
                    .Any(value => ReferenceEquals(value.Blueprint, shaken));
                passedGritAfter = passed.Descriptor.Resources.GetResourceAmount(
                    gunslinger.Grit.Resource);
                passedRounds = FirearmRuntimeState.Service.GetOrCreate(passedWeapon)
                    .Repository.State.LoadedRounds;
            }
            catch (Exception exception)
            {
                throw new InvalidOperationException(
                    "Disposable Menacing Shot failed at stage " + stage + ".",
                    exception);
            }
            finally
            {
                if (controller != null)
                {
                    MethodInfo cancel = controller.GetType().GetMethod("Cancel",
                        BindingFlags.Public | BindingFlags.Instance);
                    if (cancel != null) cancel.Invoke(controller, null);
                }
                foreach (ItemEntityWeapon weapon in new[] { failedWeapon, passedWeapon })
                    if (weapon != null) FirearmRuntimeState.Service.Forget(weapon);
                if (failed != null) failed.Dispose();
                if (passed != null) passed.Dispose();
                cleaned = SameReferences(partyBefore, SnapshotReferences(party)) &&
                    SameReferences(unitsBefore, SnapshotReferences(allUnits));
            }
            string observed = "grit=" + failedGritBefore + "->" + failedGritAfter +
                "," + passedGritBefore + "->" + passedGritAfter + ";rounds=" +
                failedRounds + "," + passedRounds + ";targets=" + failedTargets +
                "," + passedTargets + ";dc=" + dc + ";casterLevel=" + casterLevel +
                ";fear=" + frightenedApplied + ";shaken=" + shakenApplied;
            var assertions = new List<RuntimeTestAssertion>
            {
                Assertion("menacing-shot-native-contract",
                    "Will; Frightened failure; Shaken success", observed,
                    nativeContract && frightenedApplied && shakenApplied,
                    "exact cloned AbilityEffectRunAction"),
                Assertion("menacing-shot-transaction",
                    "one grit and one chamber for each delivery", observed,
                    failedGritAfter == failedGritBefore - 1 &&
                    passedGritAfter == passedGritBefore - 1 && failedRounds == 0 &&
                    passedRounds == 0, "item-owned discharge and exact grit spend"),
                Assertion("menacing-shot-params",
                    "DC 21; caster level 15; self included", observed,
                    dc == 21 && casterLevel == 15 && failedTargets == 1 &&
                    passedTargets == 1, "custom self-burst delivery parameters"),
                Assertion("external-isolation", "unchanged party/global units",
                    "cleaned=" + cleaned, cleaned, "disposable units disposed"),
                Assertion("loaded-mod-version", _request.ExpectedModVersion,
                    _context.ModEntry.Info.Version,
                    _request.ExpectedModVersion == _context.ModEntry.Info.Version,
                    "Unity Mod Manager ModEntry.Info.Version")
            };
            return CreateResult(assertions.TrueForAll(value => value.Status == "PASS")
                ? RuntimeTestStatuses.Pass : RuntimeTestStatuses.Fail,
                assertions, null);
        }

        private static void AdvanceDisposableGunslinger(UnitDescriptor owner,
            GunslingerClassBlueprintSet gunslinger, int levels,
            ref object activeController)
        {
            Type type = typeof(Kingmaker.UnitLogic.Class.LevelUp.LevelUpController);
            MethodInfo start = type.GetMethods(BindingFlags.Public |
                BindingFlags.NonPublic | BindingFlags.Static).Single(value =>
                    value.Name == "StartWithoutAssigningStaticInstance" &&
                    value.GetParameters().Length == 5);
            MethodInfo select = type.GetMethod("SelectClass", BindingFlags.Public |
                BindingFlags.Instance, null,
                new[] { typeof(BlueprintCharacterClass), typeof(bool) }, null);
            MethodInfo mechanics = type.GetMethod("ApplyClassMechanics",
                BindingFlags.Public | BindingFlags.Instance);
            MethodInfo apply = type.GetMethod("ApplyLevelup", BindingFlags.Public |
                BindingFlags.NonPublic | BindingFlags.Instance);
            MethodInfo cancel = type.GetMethod("Cancel", BindingFlags.Public |
                BindingFlags.Instance);
            object charGen = Enum.Parse(start.GetParameters()[4].ParameterType,
                "CharGen", false);
            for (int index = 0; index < levels; index++)
            {
                activeController = start.Invoke(null,
                    new object[] { owner, false, null, null, charGen });
                if (!(bool)select.Invoke(activeController,
                    new object[] { gunslinger.CharacterClass, false }))
                    throw new InvalidOperationException(
                        "Disposable Gunslinger selection failed at level " +
                        (index + 1) + ".");
                mechanics.Invoke(activeController, null);
                apply.Invoke(activeController, new object[] { owner });
                cancel.Invoke(activeController, null);
                activeController = null;
            }
        }

        private RuntimeTestResult RunObserveMenacingShotNativeFear()
        {
            BlueprintAbility[] candidates = BlueprintBootstrap.Library
                .GetAllBlueprints().OfType<BlueprintAbility>()
                .Where(value => value.name == "Fear").ToArray();
            if (candidates.Length != 1)
                throw new InvalidOperationException(
                    "Expected exactly one installed BlueprintAbility named Fear; observed " +
                    candidates.Length + ".");
            BlueprintAbility fear = candidates[0];
            SpellDescriptorComponent descriptor = fear.ComponentsArray
                .OfType<SpellDescriptorComponent>().Single();
            BlueprintComponent effect = fear.ComponentsArray.Single(value =>
                value.GetType().FullName ==
                "Kingmaker.UnitLogic.Abilities.Components.AbilityEffectRunAction");
            string observed = fear.AssetGuid + "|" + fear.name + "|" + fear.Name +
                "|action=" + fear.ActionType + "|range=" + fear.Range +
                "|enemies=" + fear.CanTargetEnemies + "|friends=" +
                fear.CanTargetFriends + "|self=" + fear.CanTargetSelf + "|" +
                "|descriptor=" + DescribeNestedObject(descriptor.Descriptor, 4) + "|" +
                DescribeComponents(fear.ComponentsArray) + "|nested=" +
                DescribeNestedObject(effect, 10);
            bool contract = fear.ComponentsArray != null &&
                fear.ComponentsArray.Length > 0;
            var assertions = new List<RuntimeTestAssertion>
            {
                Assertion("menacing-native-fear-identity",
                    "one exact installed BlueprintAbility named Fear", observed,
                    contract, "exact library identity and declared ability fields"),
                Assertion("menacing-native-fear-components",
                    "declared delivery, save, descriptors, duration, conditions",
                    observed, contract,
                    "exact installed component types and declared scalar fields"),
                Assertion("loaded-mod-version", _request.ExpectedModVersion,
                    _context.ModEntry.Info.Version,
                    _request.ExpectedModVersion == _context.ModEntry.Info.Version,
                    "Unity Mod Manager ModEntry.Info.Version")
            };
            return CreateResult(assertions.TrueForAll(value => value.Status == "PASS")
                ? RuntimeTestStatuses.Pass : RuntimeTestStatuses.Fail,
                assertions, null);
        }

        private RuntimeTestResult RunObserveDeathsShotNativeDeath()
        {
            BlueprintAbility[] deathAbilities = BlueprintBootstrap.Library
                .GetAllBlueprints().OfType<BlueprintAbility>()
                .Where(value => value.ComponentsArray != null &&
                    value.ComponentsArray.OfType<SpellDescriptorComponent>()
                    .Any(component => component.Descriptor.HasAnyFlag(
                        SpellDescriptor.Death))).ToArray();
            var observations = new List<string>();
            var candidates = new List<BlueprintAbility>();
            foreach (BlueprintAbility ability in deathAbilities)
            {
                BlueprintComponent[] effects = ability.ComponentsArray.Where(value =>
                    value.GetType().FullName ==
                        "Kingmaker.UnitLogic.Abilities.Components.AbilityEffectRunAction")
                    .ToArray();
                string nested = string.Join("||", effects.Select(value =>
                    DescribeNestedObject(value, 14)).ToArray());
                string entry = ability.AssetGuid + "|" + ability.name + "|" +
                    ability.Name + "|" + DescribeComponents(ability.ComponentsArray) +
                    "|nested=" + nested;
                observations.Add(entry);
                if (nested.Contains("SavingThrowType=Fortitude") &&
                    nested.Contains("ContextActionKillTarget"))
                    candidates.Add(ability);
            }
            string observed = "deathAbilities=" + deathAbilities.Length +
                ";saveKillCandidates=" + candidates.Count + "|" +
                string.Join("###", observations.ToArray());
            bool unique = candidates.Count == 1;
            var assertions = new List<RuntimeTestAssertion>
            {
                Assertion("deaths-shot-native-death-identity",
                    "enumerate every installed Death-descriptor ability",
                    observed, deathAbilities.Length > 0,
                    "exact library identities and declared ability fields"),
                Assertion("deaths-shot-native-death-descriptor",
                    "all candidates selected through native Death descriptor",
                    observed, deathAbilities.Length > 0,
                    "exact installed SpellDescriptorComponents"),
                Assertion("deaths-shot-native-save-kill-actions",
                    "exactly one native Fortitude-save and kill-action graph",
                    observed, unique,
                    "exact installed nested action graph"),
                Assertion("loaded-mod-version", _request.ExpectedModVersion,
                    _context.ModEntry.Info.Version,
                    _request.ExpectedModVersion == _context.ModEntry.Info.Version,
                    "Unity Mod Manager ModEntry.Info.Version")
            };
            return CreateResult(assertions.TrueForAll(value => value.Status == "PASS")
                ? RuntimeTestStatuses.Pass : RuntimeTestStatuses.Fail,
                assertions, null);
        }

        private RuntimeTestResult RunObserveStunningShotNativeStunned()
        {
            BlueprintBuff[] candidates = BlueprintBootstrap.Library
                .GetAllBlueprints().OfType<BlueprintBuff>()
                .Where(value => value.name == "Stunned").ToArray();
            string observed = string.Join("###", candidates.Select(value =>
                value.AssetGuid + "|" + value.name + "|" + value.Name + "|" +
                DescribeComponents(value.ComponentsArray) + "|nested=" +
                DescribeNestedObject(value, 10)).ToArray());
            bool unique = candidates.Length == 1;
            bool nativeCondition = unique && observed.Contains(
                "Kingmaker.UnitLogic.FactLogic.AddCondition{Condition=Stunned}");
            var assertions = new List<RuntimeTestAssertion>
            {
                Assertion("stunning-shot-native-stunned-identity",
                    "one exact installed BlueprintBuff named Stunned", observed,
                    unique, "exact library identity and stable GUID"),
                Assertion("stunning-shot-native-stunned-condition",
                    "native UnitCondition.Stunned mechanics", observed,
                    nativeCondition, "exact installed components and fields"),
                Assertion("stunning-shot-native-critical-immunity-rule",
                    "RuleAttackRoll.ImmuneToCriticalHit is readable",
                    DescribeProperty(typeof(RuleAttackRoll).GetProperty(
                        "ImmuneToCriticalHit")),
                    typeof(RuleAttackRoll).GetProperty("ImmuneToCriticalHit") != null,
                    "exact declared native attack result"),
                Assertion("loaded-mod-version", _request.ExpectedModVersion,
                    _context.ModEntry.Info.Version,
                    _request.ExpectedModVersion == _context.ModEntry.Info.Version,
                    "Unity Mod Manager ModEntry.Info.Version")
            };
            return CreateResult(assertions.TrueForAll(value => value.Status == "PASS")
                ? RuntimeTestStatuses.Pass : RuntimeTestStatuses.Fail,
                assertions, null);
        }

        private RuntimeTestResult RunObserveSlingersLuckNativeRerolls()
        {
            BlueprintUnit source = BlueprintRoot.Instance.DefaultPlayerCharacter;
            Kingmaker.EntitySystem.Entities.UnitEntityData unit = null;
            int savingBefore = -1, savingAfter = -1;
            int skillBefore = -1, skillAfter = -1;
            bool cleaned = false;
            try
            {
                unit = new Kingmaker.UI.LevelUp.ChargenUnit(source).Unit;
                var saving = new RuleSavingThrow(unit,
                    SavingThrowType.Will, 100);
                Rulebook.Trigger(saving);
                savingBefore = saving.BaseRollResult;
                RulebookEvent.RollEntry replacement = 1;
                typeof(RuleSavingThrow).GetProperty("D20",
                    BindingFlags.Instance | BindingFlags.Public |
                    BindingFlags.DeclaredOnly).GetSetMethod(true).Invoke(
                        saving, new object[] { replacement });
                savingAfter = saving.BaseRollResult;

                var skill = new RuleSkillCheck(unit,
                    StatType.SkillAthletics, 100);
                Rulebook.Trigger(skill);
                skillBefore = skill.BaseRollResult;
                typeof(RuleSkillCheck).GetProperty("D20",
                    BindingFlags.Instance | BindingFlags.Public |
                    BindingFlags.DeclaredOnly).GetSetMethod(true).Invoke(
                        skill, new object[] { replacement });
                skillAfter = skill.BaseRollResult;
            }
            finally
            {
                if (unit != null) unit.Dispose();
                cleaned = true;
            }

            PropertyInfo savingD20 = typeof(RuleSavingThrow).GetProperty("D20",
                BindingFlags.Instance | BindingFlags.Public |
                BindingFlags.DeclaredOnly);
            PropertyInfo savingResult = typeof(RuleSavingThrow).GetProperty(
                "RollResult", BindingFlags.Instance | BindingFlags.Public |
                BindingFlags.DeclaredOnly);
            PropertyInfo skillD20 = typeof(RuleSkillCheck).GetProperty("D20",
                BindingFlags.Instance | BindingFlags.Public |
                BindingFlags.DeclaredOnly);
            PropertyInfo skillResult = typeof(RuleSkillCheck).GetProperty(
                "RollResult", BindingFlags.Instance | BindingFlags.Public |
                BindingFlags.DeclaredOnly);
            PropertyInfo nativeD20 = typeof(RulebookEvent.Dice).GetProperty("D20",
                BindingFlags.Static | BindingFlags.Public |
                BindingFlags.NonPublic);
            string observed = "saving=" + savingBefore + "->" + savingAfter +
                ";skill=" + skillBefore + "->" + skillAfter +
                ";savingD20=" + DescribeProperty(savingD20) +
                ";savingResult=" + DescribeProperty(savingResult) +
                ";skillD20=" + DescribeProperty(skillD20) +
                ";skillResult=" + DescribeProperty(skillResult) +
                ";nativeD20=" + DescribeProperty(nativeD20) +
                ";cleaned=" + cleaned;
            bool exactMembers = savingD20 != null && savingD20.CanRead &&
                savingD20.CanWrite && savingD20.GetSetMethod(true) != null &&
                !savingD20.GetSetMethod(true).IsPublic && savingResult != null &&
                savingResult.CanRead && !savingResult.CanWrite &&
                skillD20 != null && skillD20.CanRead && skillD20.CanWrite &&
                skillD20.GetSetMethod(true) != null &&
                !skillD20.GetSetMethod(true).IsPublic &&
                skillResult != null && skillResult.CanRead &&
                !skillResult.CanWrite && nativeD20 != null && nativeD20.CanRead;
            var assertions = new List<RuntimeTestAssertion>
            {
                Assertion("slingers-luck-native-rule-contracts",
                    "exact non-public D20 setters and read-only RollResult members",
                    observed, exactMembers,
                    "declared RuleSavingThrow and RuleSkillCheck metadata"),
                Assertion("slingers-luck-post-trigger-replacement",
                    "both completed rule results replaceable with mandatory 1",
                    observed, savingAfter == 1 && skillAfter == 1,
                    "exact D20 replacement after native Rulebook.Trigger"),
                Assertion("external-isolation", "disposable unit cleaned",
                    observed, cleaned, "detached descriptor disposal"),
                Assertion("loaded-mod-version", _request.ExpectedModVersion,
                    _context.ModEntry.Info.Version,
                    _request.ExpectedModVersion == _context.ModEntry.Info.Version,
                    "Unity Mod Manager ModEntry.Info.Version")
            };
            return CreateResult(assertions.TrueForAll(value => value.Status == "PASS")
                ? RuntimeTestStatuses.Pass : RuntimeTestStatuses.Fail,
                assertions, null);
        }

        private static string DescribeProperty(PropertyInfo property)
        {
            if (property == null) return "<missing>";
            return property.DeclaringType.FullName + "." + property.Name +
                ":" + property.PropertyType.FullName + ":get=" +
                property.CanRead + ":set=" + property.CanWrite;
        }

        private RuntimeTestResult RunDisposableGunslingerSlingersLuck()
        {
            BlueprintUnit source = BlueprintRoot.Instance.DefaultPlayerCharacter;
            GunslingerClassBlueprintSet gunslinger = BlueprintBootstrap.GunslingerClass;
            SlingersLuckBlueprintSet set = gunslinger.SlingersLuck;
            Kingmaker.EntitySystem.Entities.UnitEntityData unit = null;
            Kingmaker.EntitySystem.Entities.UnitEntityData other = null;
            object controller = null;
            int savingFirst = -1, savingSecond = -1, savingGritBefore = -1,
                savingGritAfter = -1, skillFirst = -1, skillSecond = -1,
                skillGritBefore = -1, skillGritAfter = -1,
                otherGritBefore = -1, otherGritAfter = -1;
            bool savingConsumed = false, skillConsumed = false, cleaned = false;
            try
            {
                unit = new Kingmaker.UI.LevelUp.ChargenUnit(source).Unit;
                other = new Kingmaker.UI.LevelUp.ChargenUnit(source).Unit;
                unit.Descriptor.Stats.Wisdom.BaseValue = 18;
                other.Descriptor.Stats.Wisdom.BaseValue = 18;
                AdvanceDisposableGunslinger(unit.Descriptor, gunslinger, 15,
                    ref controller);
                AdvanceDisposableGunslinger(other.Descriptor, gunslinger, 15,
                    ref controller);
                unit.Descriptor.Resources.Restore(gunslinger.Grit.Resource, 4);
                other.Descriptor.Resources.Restore(gunslinger.Grit.Resource, 4);
                otherGritBefore = other.Descriptor.Resources.GetResourceAmount(
                    gunslinger.Grit.Resource);

                var savingContext = new MechanicsContext(unit, unit.Descriptor,
                    set.SavingAbility, null, new TargetWrapper(unit));
                unit.Descriptor.Buffs.AddBuff(set.SavingMarker, savingContext, null);
                Buff savingMarker = unit.Descriptor.Buffs.RawFacts.OfType<Buff>()
                    .Single(value => ReferenceEquals(value.Blueprint,
                        set.SavingMarker));
                int savingSeed = FindDescendingNativeD20Seed(out savingFirst,
                    out savingSecond);
                UnityEngine.Random.InitState(savingSeed);
                savingGritBefore = unit.Descriptor.Resources.GetResourceAmount(
                    gunslinger.Grit.Resource);
                var saving = new RuleSavingThrow(unit, SavingThrowType.Will, 100);
                Rulebook.Trigger(saving);
                savingMarker.CallComponents<IInitiatorRulebookHandler<
                    RuleSavingThrow>>(handler => handler.OnEventDidTrigger(saving));
                savingGritAfter = unit.Descriptor.Resources.GetResourceAmount(
                    gunslinger.Grit.Resource);
                savingConsumed = !unit.Descriptor.Buffs.RawFacts.OfType<Buff>()
                    .Any(value => ReferenceEquals(value.Blueprint,
                        set.SavingMarker));
                if (saving.D20.Value != savingSecond)
                    throw new InvalidOperationException(
                        "Saving throw did not retain the lower second d20: expected " +
                        savingSecond + ", observed " + saving.D20.Value +
                        ", grit " + savingGritBefore + "->" + savingGritAfter +
                        ", markerConsumed=" + savingConsumed + ".");

                var skillContext = new MechanicsContext(unit, unit.Descriptor,
                    set.SkillAbility, null, new TargetWrapper(unit));
                unit.Descriptor.Buffs.AddBuff(set.SkillMarker, skillContext, null);
                Buff skillMarker = unit.Descriptor.Buffs.RawFacts.OfType<Buff>()
                    .Single(value => ReferenceEquals(value.Blueprint,
                        set.SkillMarker));
                int skillSeed = FindDescendingNativeD20Seed(out skillFirst,
                    out skillSecond);
                UnityEngine.Random.InitState(skillSeed);
                skillGritBefore = unit.Descriptor.Resources.GetResourceAmount(
                    gunslinger.Grit.Resource);
                var skill = new RuleSkillCheck(unit, StatType.SkillAthletics, 100);
                Rulebook.Trigger(skill);
                skillMarker.CallComponents<IInitiatorRulebookHandler<
                    RuleSkillCheck>>(handler => handler.OnEventDidTrigger(skill));
                skillGritAfter = unit.Descriptor.Resources.GetResourceAmount(
                    gunslinger.Grit.Resource);
                skillConsumed = !unit.Descriptor.Buffs.RawFacts.OfType<Buff>()
                    .Any(value => ReferenceEquals(value.Blueprint,
                        set.SkillMarker));
                if (skill.D20.Value != skillSecond)
                    throw new InvalidOperationException(
                        "Skill check did not retain the lower second d20.");
                otherGritAfter = other.Descriptor.Resources.GetResourceAmount(
                    gunslinger.Grit.Resource);
            }
            finally
            {
                if (controller != null)
                {
                    MethodInfo cancel = controller.GetType().GetMethod("Cancel",
                        BindingFlags.Public | BindingFlags.Instance);
                    if (cancel != null) cancel.Invoke(controller, null);
                }
                if (unit != null) unit.Dispose();
                if (other != null) other.Dispose();
                cleaned = true;
            }
            string observed = "saving=" + savingFirst + "->" + savingSecond +
                ";savingGrit=" + savingGritBefore + "->" + savingGritAfter +
                ";savingConsumed=" + savingConsumed + ";skill=" + skillFirst +
                "->" + skillSecond + ";skillGrit=" + skillGritBefore + "->" +
                skillGritAfter + ";skillConsumed=" + skillConsumed +
                ";otherGrit=" + otherGritBefore + "->" + otherGritAfter +
                ";cleaned=" + cleaned;
            bool progression = gunslinger.Progression.LevelEntries[14].Features
                .Count(value => ReferenceEquals(value, set.Feature)) == 1;
            var assertions = new List<RuntimeTestAssertion>
            {
                Assertion("slingers-luck-saving-reroll",
                    "lower native second d20; fixed grit cost 2; marker consumed",
                    observed, savingFirst > savingSecond &&
                        savingGritAfter == savingGritBefore - 2 && savingConsumed,
                    "RuleSavingThrow initiator handler"),
                Assertion("slingers-luck-skill-reroll",
                    "lower native second d20; fixed grit cost 1; marker consumed",
                    observed, skillFirst > skillSecond &&
                        skillGritAfter == skillGritBefore - 1 && skillConsumed,
                    "RuleSkillCheck initiator handler"),
                Assertion("slingers-luck-level-and-isolation",
                    "level 15 grant; other unit unchanged; disposal",
                    observed, progression && otherGritBefore == otherGritAfter &&
                        cleaned, "production progression and detached units"),
                Assertion("loaded-mod-version", _request.ExpectedModVersion,
                    _context.ModEntry.Info.Version,
                    _request.ExpectedModVersion == _context.ModEntry.Info.Version,
                    "Unity Mod Manager ModEntry.Info.Version")
            };
            return CreateResult(assertions.TrueForAll(value => value.Status == "PASS")
                ? RuntimeTestStatuses.Pass : RuntimeTestStatuses.Fail,
                assertions, null);
        }

        private static int FindDescendingNativeD20Seed(out int first,
            out int second)
        {
            for (int seed = 1; seed <= 10000; seed++)
            {
                UnityEngine.Random.InitState(seed);
                first = RulebookEvent.Dice.D20.Value;
                second = RulebookEvent.Dice.D20.Value;
                if (first > second) return seed;
            }
            throw new InvalidOperationException(
                "No deterministic descending native d20 seed was found.");
        }

        private RuntimeTestResult RunDisposableGunslingerCheatDeath()
        {
            BlueprintUnit source = BlueprintRoot.Instance.DefaultPlayerCharacter;
            GunslingerClassBlueprintSet gunslinger = BlueprintBootstrap.GunslingerClass;
            Kingmaker.EntitySystem.Entities.UnitEntityData attacker = null;
            Kingmaker.EntitySystem.Entities.UnitEntityData unit = null;
            Kingmaker.EntitySystem.Entities.UnitEntityData noGrit = null;
            object controller = null;
            int maxHp = -1, gritBefore = -1, gritAfter = -1, hpAfter = -1,
                noGritHpAfter = -1;
            bool cleaned = false;
            try
            {
                attacker = new Kingmaker.UI.LevelUp.ChargenUnit(source).Unit;
                unit = new Kingmaker.UI.LevelUp.ChargenUnit(source).Unit;
                noGrit = new Kingmaker.UI.LevelUp.ChargenUnit(source).Unit;
                AdvanceDisposableGunslinger(unit.Descriptor, gunslinger, 19,
                    ref controller);
                AdvanceDisposableGunslinger(noGrit.Descriptor, gunslinger, 19,
                    ref controller);
                unit.Descriptor.Resources.Restore(gunslinger.Grit.Resource, 4);
                noGrit.Descriptor.Resources.Spend(gunslinger.Grit.Resource,
                    noGrit.Descriptor.Resources.GetResourceAmount(
                        gunslinger.Grit.Resource));
                maxHp = unit.MaxHP;
                gritBefore = unit.Descriptor.Resources.GetResourceAmount(
                    gunslinger.Grit.Resource);
                var lethal = new RuleDealDamage(attacker, unit,
                    new DamageBundle(new DirectDamage(
                        new DiceFormula(0, DiceType.D6), maxHp + 10)));
                Rulebook.Trigger(lethal);
                hpAfter = unit.HPLeft;
                gritAfter = unit.Descriptor.Resources.GetResourceAmount(
                    gunslinger.Grit.Resource);
                var unprevented = new RuleDealDamage(attacker, noGrit,
                    new DamageBundle(new DirectDamage(
                        new DiceFormula(0, DiceType.D6), noGrit.MaxHP + 10)));
                Rulebook.Trigger(unprevented);
                noGritHpAfter = noGrit.HPLeft;
            }
            finally
            {
                if (controller != null)
                {
                    MethodInfo cancel = controller.GetType().GetMethod("Cancel",
                        BindingFlags.Public | BindingFlags.Instance);
                    if (cancel != null) cancel.Invoke(controller, null);
                }
                if (attacker != null) attacker.Dispose();
                if (unit != null) unit.Dispose();
                if (noGrit != null) noGrit.Dispose();
                cleaned = true;
            }
            string observed = "maxHp=" + maxHp + ";grit=" + gritBefore +
                "->" + gritAfter + ";hpAfter=" + hpAfter +
                ";zeroGritHpAfter=" + noGritHpAfter + ";cleaned=" + cleaned;
            bool progression = gunslinger.Progression.LevelEntries[18].Features
                .Count(value => ReferenceEquals(value, gunslinger.CheatDeath)) == 1;
            var assertions = new List<RuntimeTestAssertion>
            {
                Assertion("cheat-death-native-lethal-damage",
                    "completed native damage leaves exactly 1 HP and spends all grit",
                    observed, hpAfter == 1 && gritBefore >= 1 && gritAfter == 0,
                    "RuleDealDamage target feature handler"),
                Assertion("cheat-death-zero-grit-gate",
                    "zero grit does not prevent lethal damage", observed,
                    noGritHpAfter <= 0, "independent detached unit"),
                Assertion("cheat-death-level-and-cleanup",
                    "level 19 grant and detached-unit disposal", observed,
                    progression && cleaned, "production progression"),
                Assertion("loaded-mod-version", _request.ExpectedModVersion,
                    _context.ModEntry.Info.Version,
                    _request.ExpectedModVersion == _context.ModEntry.Info.Version,
                    "Unity Mod Manager ModEntry.Info.Version")
            };
            return CreateResult(assertions.TrueForAll(value => value.Status == "PASS")
                ? RuntimeTestStatuses.Pass : RuntimeTestStatuses.Fail,
                assertions, null);
        }

        private RuntimeTestResult RunDisposableGunslingerStunningShot(
            bool qualifyTrueGrit = false)
        {
            BlueprintUnit source = BlueprintRoot.Instance.DefaultPlayerCharacter;
            GunslingerClassBlueprintSet gunslinger = BlueprintBootstrap.GunslingerClass;
            StunningShotBlueprintSet set = gunslinger.StunningShot;
            BlueprintItemWeapon pistol = BlueprintBootstrap.ProductionFirearms.Pistol.Item;
            object player = ReadExactMember(Kingmaker.Game.Instance, "Player");
            object state = ReadExactMember(Kingmaker.Game.Instance, "State");
            object party = ReadExactMember(player, "Party");
            object allUnits = ReadExactMember(state, "AllUnits");
            object[] partyBefore = SnapshotReferences(party);
            object[] unitsBefore = SnapshotReferences(allUnits);
            Kingmaker.EntitySystem.Entities.UnitEntityData attacker = null;
            Kingmaker.EntitySystem.Entities.UnitEntityData failedTarget = null;
            Kingmaker.EntitySystem.Entities.UnitEntityData passedTarget = null;
            ItemEntityWeapon weapon = null;
            Buff stunned = null;
            object controller = null;
            int gritBefore = -1, gritAfterFailure = -1, gritAfterSuccess = -1,
                gritAfterImmunity = -1, roundsBefore = -1, roundsAfter = -1,
                damageBefore = -1, damageAfter = -1, nativeDamage = -1;
            bool available = false, failureMarkerConsumed = false,
                successMarkerConsumed = false, immunityMarkerConsumed = false,
                successStunned = false, immunityStunned = false, cleaned = false;
            bool selectionShape = false, positiveGateAtZero = false,
                zeroCostRequiresPositive = false, variableCost = false,
                slingersLuckExcluded = false;
            double durationSeconds = -1d;
            int failureD20 = -1, successD20 = -1;
            string stage = "progression";
            try
            {
                attacker = new Kingmaker.UI.LevelUp.ChargenUnit(source).Unit;
                failedTarget = new Kingmaker.UI.LevelUp.ChargenUnit(source).Unit;
                passedTarget = new Kingmaker.UI.LevelUp.ChargenUnit(source).Unit;
                failedTarget.Descriptor.State.Immortality.ReleaseAll();
                passedTarget.Descriptor.State.Immortality.ReleaseAll();
                attacker.Descriptor.Stats.Wisdom.BaseValue = 18;
                AdvanceDisposableGunslinger(attacker.Descriptor, gunslinger,
                    qualifyTrueGrit ? 20 : 19,
                    ref controller);
                if (qualifyTrueGrit)
                {
                    attacker.Descriptor.AddFact(gunslinger.TrueGrit.ChoiceFor(
                        TrueGritDeed.StunningShot));
                    attacker.Descriptor.AddFact(gunslinger.TrueGrit.ChoiceFor(
                        TrueGritDeed.StopBleeding));
                    selectionShape = gunslinger.TrueGrit.Choices.Length == 20 &&
                        gunslinger.Progression.LevelEntries[19].Features.Count(
                            value => ReferenceEquals(value,
                                gunslinger.TrueGrit.Selection)) == 2;
                    slingersLuckExcluded = !TrueGritCatalog.Choices.Any(value =>
                        value.Deed.ToString().IndexOf("Luck",
                            StringComparison.OrdinalIgnoreCase) >= 0);
                }
                attacker.Descriptor.Resources.Restore(gunslinger.Grit.Resource, 4);
                weapon = new ItemEntityWeapon(pistol);
                attacker.Body.PrimaryHand.InsertItem(weapon);
                FirearmRuntimeState.Service.Set(weapon, new FirearmState(
                    FirearmState.CurrentSchemaVersion, 1,
                    FirearmStateTokenCatalog.DiagnosticLeadBall,
                    FirearmCondition.Normal));
                gritBefore = attacker.Descriptor.Resources.GetResourceAmount(
                    gunslinger.Grit.Resource);
                roundsBefore = FirearmRuntimeState.Service.GetOrCreate(weapon)
                    .Repository.State.LoadedRounds;
                var abilityData = new Kingmaker.UnitLogic.Abilities.AbilityData(
                    set.Ability, attacker.Descriptor);
                available = set.Ability.ComponentsArray
                    .OfType<StunningShotAbilityLogic>().Single()
                    .IsAvailableFor(abilityData);

                stage = "ordinary-native-shot";
                damageBefore = failedTarget.Damage;
                FirearmMisfireRuntime.QueueForcedNaturalRoll(19);
                var ordinary = new RuleAttackWithWeapon(attacker, failedTarget,
                    weapon, 0) { AutoHit = true };
                Rulebook.Trigger(ordinary);
                FirearmMisfireRuntime.CancelForcedNaturalRoll();
                RuleDealDamage ordinaryDamage = ordinary.MeleeDamage;
                if (ordinaryDamage == null && ordinary.AttackRoll != null &&
                    ordinary.AttackRoll.IsHit)
                {
                    ordinaryDamage = ordinary.CreateRuleDealDamage(false);
                    Rulebook.Trigger(ordinaryDamage);
                }
                roundsAfter = FirearmRuntimeState.Service.GetOrCreate(weapon)
                    .Repository.State.LoadedRounds;
                damageAfter = failedTarget.Damage;
                if (ordinaryDamage != null) nativeDamage = ordinaryDamage.Damage;

                stage = "save-failure";
                var context = new MechanicsContext(attacker, attacker.Descriptor,
                    set.Ability, null, new TargetWrapper(attacker));
                attacker.Descriptor.Buffs.AddBuff(set.ArmedMarker, context, null);
                Buff failureMarker = attacker.Descriptor.Buffs.RawFacts.OfType<Buff>()
                    .Single(value => ReferenceEquals(value.Blueprint,
                        set.ArmedMarker));
                var failureAttack = CreateResolvedStunningShotAttack(attacker,
                    failedTarget, weapon, false);
                int failureSeed = FindNativeD20Seed(1);
                UnityEngine.Random.InitState(failureSeed);
                failureMarker.CallComponents<IInitiatorRulebookHandler<
                    RuleAttackWithWeapon>>(handler =>
                        handler.OnEventDidTrigger(failureAttack));
                gritAfterFailure = attacker.Descriptor.Resources.GetResourceAmount(
                    gunslinger.Grit.Resource);
                failureMarkerConsumed = !attacker.Descriptor.Buffs.RawFacts
                    .OfType<Buff>().Any(value => ReferenceEquals(value.Blueprint,
                        set.ArmedMarker));
                stunned = failedTarget.Descriptor.Buffs.RawFacts.OfType<Buff>()
                    .SingleOrDefault(value => ReferenceEquals(value.Blueprint,
                        set.Stunned));
                if (stunned != null) durationSeconds = stunned.TimeLeft.TotalSeconds;
                failureD20 = 1;

                stage = "save-success";
                attacker.Descriptor.Buffs.AddBuff(set.ArmedMarker, context, null);
                Buff successMarker = attacker.Descriptor.Buffs.RawFacts.OfType<Buff>()
                    .Single(value => ReferenceEquals(value.Blueprint,
                        set.ArmedMarker));
                var successAttack = CreateResolvedStunningShotAttack(attacker,
                    passedTarget, weapon, false);
                int successSeed = FindNativeD20Seed(20);
                UnityEngine.Random.InitState(successSeed);
                successMarker.CallComponents<IInitiatorRulebookHandler<
                    RuleAttackWithWeapon>>(handler =>
                        handler.OnEventDidTrigger(successAttack));
                gritAfterSuccess = attacker.Descriptor.Resources.GetResourceAmount(
                    gunslinger.Grit.Resource);
                successMarkerConsumed = !attacker.Descriptor.Buffs.RawFacts
                    .OfType<Buff>().Any(value => ReferenceEquals(value.Blueprint,
                        set.ArmedMarker));
                successStunned = passedTarget.Descriptor.Buffs.RawFacts.OfType<Buff>()
                    .Any(value => ReferenceEquals(value.Blueprint, set.Stunned));
                successD20 = 20;

                stage = "critical-immunity";
                attacker.Descriptor.Resources.Restore(gunslinger.Grit.Resource, 2);
                int beforeImmunity = attacker.Descriptor.Resources.GetResourceAmount(
                    gunslinger.Grit.Resource);
                attacker.Descriptor.Buffs.AddBuff(set.ArmedMarker, context, null);
                Buff immunityMarker = attacker.Descriptor.Buffs.RawFacts.OfType<Buff>()
                    .Single(value => ReferenceEquals(value.Blueprint,
                        set.ArmedMarker));
                var immunityAttack = CreateResolvedStunningShotAttack(attacker,
                    passedTarget, weapon, true);
                immunityMarker.CallComponents<IInitiatorRulebookHandler<
                    RuleAttackWithWeapon>>(handler =>
                        handler.OnEventDidTrigger(immunityAttack));
                gritAfterImmunity = attacker.Descriptor.Resources.GetResourceAmount(
                    gunslinger.Grit.Resource);
                immunityMarkerConsumed = !attacker.Descriptor.Buffs.RawFacts
                    .OfType<Buff>().Any(value => ReferenceEquals(value.Blueprint,
                        set.ArmedMarker));
                immunityStunned = passedTarget.Descriptor.Buffs.RawFacts.OfType<Buff>()
                    .Any(value => ReferenceEquals(value.Blueprint, set.Stunned));
                if (gritAfterImmunity != beforeImmunity)
                    throw new InvalidOperationException(
                        "Critical immunity changed grit unexpectedly.");
                if (qualifyTrueGrit)
                {
                    variableCost = TrueGritRuntime.Evaluate(attacker.Descriptor,
                        TrueGritDeed.StunningShot, 3, false).EffectiveCost == 2;
                    int remaining = attacker.Descriptor.Resources.GetResourceAmount(
                        gunslinger.Grit.Resource);
                    attacker.Descriptor.Resources.Spend(gunslinger.Grit.Resource,
                        remaining);
                    positiveGateAtZero = TrueGritRuntime.Evaluate(
                        attacker.Descriptor, TrueGritDeed.StopBleeding, 0, true)
                        .Available;
                    zeroCostRequiresPositive = !TrueGritRuntime.Evaluate(
                        attacker.Descriptor, TrueGritDeed.StunningShot, 1, false)
                        .Available;
                }
            }
            catch (Exception exception)
            {
                throw new InvalidOperationException(
                    "Disposable Stunning Shot failed at stage " + stage + ".",
                    exception);
            }
            finally
            {
                FirearmMisfireRuntime.CancelForcedNaturalRoll();
                if (stunned != null && failedTarget != null)
                    failedTarget.Descriptor.Buffs.RemoveFact(stunned);
                if (weapon != null)
                {
                    FirearmRuntimeState.Service.Forget(weapon);
                    if (attacker != null && attacker.Body.PrimaryHand.MaybeItem != null)
                        attacker.Body.PrimaryHand.RemoveItem(false);
                }
                if (controller != null)
                {
                    MethodInfo cancel = controller.GetType().GetMethod("Cancel",
                        BindingFlags.Public | BindingFlags.Instance);
                    if (cancel != null) cancel.Invoke(controller, null);
                }
                if (failedTarget != null) failedTarget.Dispose();
                if (passedTarget != null) passedTarget.Dispose();
                if (attacker != null) attacker.Dispose();
                cleaned = SameReferences(partyBefore, SnapshotReferences(party)) &&
                    SameReferences(unitsBefore, SnapshotReferences(allUnits)) &&
                    (attacker == null || !ContainsReference(allUnits, attacker)) &&
                    (failedTarget == null || !ContainsReference(allUnits, failedTarget)) &&
                    (passedTarget == null || !ContainsReference(allUnits, passedTarget));
            }
            bool progression = gunslinger.Progression.LevelEntries[18].Features
                .Count(value => ReferenceEquals(value, set.Feature)) == 1;
            int deedCost = qualifyTrueGrit ? 1 : 2;
            string observed = "available=" + available + ";rounds=" + roundsBefore +
                "->" + roundsAfter + ";damage=" + damageBefore + "->" +
                damageAfter + ";nativeDamage=" + nativeDamage + ";grit=" +
                gritBefore + "->" + gritAfterFailure +
                "->" + gritAfterSuccess + "->" + gritAfterImmunity +
                ";d20=" + failureD20 + "," + successD20 +
                ";markers=" + failureMarkerConsumed + "," +
                successMarkerConsumed + "," + immunityMarkerConsumed +
                ";stunnedDuration=" + durationSeconds.ToString("F3",
                    System.Globalization.CultureInfo.InvariantCulture) +
                ";successStunned=" + successStunned +
                ";immunityStunned=" + immunityStunned + ";cleaned=" + cleaned;
            var assertions = new List<RuntimeTestAssertion>
            {
                Assertion("stunning-shot-progression-and-arming",
                    "level 19 and available with four grit and loaded firearm",
                    observed, progression && available && gritBefore == 4,
                    "production progression and availability provider"),
                Assertion("stunning-shot-native-firearm-preservation",
                    "ordinary hit consumes one chamber and deals native damage",
                    observed, roundsBefore == 1 && roundsAfter == 0 &&
                    nativeDamage > 0,
                    "native RuleAttackWithWeapon and firearm pipeline"),
                Assertion("stunning-shot-save-failure",
                    "natural 1 Fortitude spends the effective grit cost and applies one-round Stunned",
                    observed, failureMarkerConsumed && gritAfterFailure == 4 - deedCost &&
                    stunned != null && durationSeconds > 0d && durationSeconds <= 6.1d,
                    "production marker handler and native RuleSavingThrow"),
                Assertion("stunning-shot-save-success",
                    "natural 20 Fortitude spends the effective grit cost without Stunned",
                    observed, successMarkerConsumed && gritAfterSuccess == 4 - (2 * deedCost) &&
                    !successStunned,
                    "production marker handler and native RuleSavingThrow"),
                Assertion("stunning-shot-critical-immunity",
                    "native critical immunity consumes marker without grit or Stunned",
                    observed, immunityMarkerConsumed && gritAfterImmunity ==
                        Math.Min(4, 6 - (2 * deedCost)) &&
                    !immunityStunned,
                    "RuleAttackRoll.ImmuneToCriticalHit"),
                Assertion("true-grit-selection-and-policy",
                    qualifyTrueGrit ? "two level-20 selections, twenty choices, selected cost reduction, zero-grit gate removal, and fixed exclusion" : "ordinary Stunning Shot cost retained",
                    observed, !qualifyTrueGrit || (selectionShape &&
                        positiveGateAtZero && zeroCostRequiresPositive &&
                        variableCost && slingersLuckExcluded),
                    "production selection blueprints, unit-owned facts, and TrueGritRuntime"),
                Assertion("external-isolation", "detached units and item cleaned",
                    observed, cleaned, "reference snapshots and disposal"),
                Assertion("loaded-mod-version", _request.ExpectedModVersion,
                    _context.ModEntry.Info.Version,
                    _request.ExpectedModVersion == _context.ModEntry.Info.Version,
                    "Unity Mod Manager ModEntry.Info.Version")
            };
            return CreateResult(assertions.TrueForAll(value => value.Status == "PASS")
                ? RuntimeTestStatuses.Pass : RuntimeTestStatuses.Fail,
                assertions, null);
        }

        private RuntimeTestResult RunDisposableGunslingerDeathsShot()
        {
            BlueprintUnit source = BlueprintRoot.Instance.DefaultPlayerCharacter;
            GunslingerClassBlueprintSet gunslinger = BlueprintBootstrap.GunslingerClass;
            DeathsShotBlueprintSet set = gunslinger.DeathsShot;
            BlueprintItemWeapon pistol = BlueprintBootstrap.ProductionFirearms.Pistol.Item;
            object player = ReadExactMember(Kingmaker.Game.Instance, "Player");
            object state = ReadExactMember(Kingmaker.Game.Instance, "State");
            object party = ReadExactMember(player, "Party");
            object allUnits = ReadExactMember(state, "AllUnits");
            object[] partyBefore = SnapshotReferences(party);
            object[] unitsBefore = SnapshotReferences(allUnits);
            Kingmaker.EntitySystem.Entities.UnitEntityData attacker = null,
                failedTarget = null, passedTarget = null;
            ItemEntityWeapon weapon = null; object controller = null;
            int gritBefore = -1, gritAfterFailure = -1, gritAfterSuccess = -1;
            bool failedKilled = false, passedKilled = false, cleaned = false;
            bool failedMarked = false, passedMarked = false;
            int failedHp = int.MaxValue, passedHp = int.MaxValue;
            bool failedRegistered = false, passedRegistered = false;
            int unitPoolBefore = Kingmaker.Game.Instance.State.Units.Count;
            string stage = "setup";
            try
            {
                attacker = new Kingmaker.UI.LevelUp.ChargenUnit(source).Unit;
                failedTarget = new Kingmaker.UI.LevelUp.ChargenUnit(source).Unit;
                passedTarget = new Kingmaker.UI.LevelUp.ChargenUnit(source).Unit;
                failedTarget.Descriptor.State.Immortality.ReleaseAll();
                passedTarget.Descriptor.State.Immortality.ReleaseAll();
                failedRegistered = Kingmaker.Game.Instance.State.Units.All
                    .Add(failedTarget);
                passedRegistered = Kingmaker.Game.Instance.State.Units.All
                    .Add(passedTarget);
                if (!failedRegistered || !passedRegistered)
                    throw new InvalidOperationException(
                        "Disposable death targets could not be registered.");
                attacker.Descriptor.Stats.Dexterity.BaseValue = 20;
                AdvanceDisposableGunslinger(attacker.Descriptor, gunslinger, 19,
                    ref controller);
                attacker.Descriptor.Resources.Restore(gunslinger.Grit.Resource, 2);
                gritBefore = attacker.Descriptor.Resources.GetResourceAmount(
                    gunslinger.Grit.Resource);
                weapon = new ItemEntityWeapon(pistol);
                attacker.Body.PrimaryHand.InsertItem(weapon);
                var context = new MechanicsContext(attacker, attacker.Descriptor,
                    set.Ability, null, new TargetWrapper(attacker));

                stage = "save-failure";
                attacker.Descriptor.Buffs.AddBuff(set.ArmedMarker, context, null);
                Buff marker = attacker.Descriptor.Buffs.RawFacts.OfType<Buff>()
                    .Single(value => ReferenceEquals(value.Blueprint, set.ArmedMarker));
                RuleAttackWithWeapon failure = CreateResolvedStunningShotAttack(
                    attacker, failedTarget, weapon, false);
                SetExactProperty(failure.AttackRoll, "IsCriticalConfirmed", true);
                UnityEngine.Random.InitState(FindNativeD20Seed(1));
                marker.CallComponents<IInitiatorRulebookHandler<RuleAttackWithWeapon>>(
                    handler => handler.OnEventDidTrigger(failure));
                gritAfterFailure = attacker.Descriptor.Resources.GetResourceAmount(
                    gunslinger.Grit.Resource);
                failedKilled = failedTarget.Descriptor.State.IsDead;
                failedMarked = failedTarget.Descriptor.State.MarkedForDeath ||
                    failedTarget.Descriptor.State.ForceKill;
                failedHp = failedTarget.HPLeft;

                stage = "save-success";
                attacker.Descriptor.Resources.Restore(gunslinger.Grit.Resource, 1);
                attacker.Descriptor.Buffs.AddBuff(set.ArmedMarker, context, null);
                marker = attacker.Descriptor.Buffs.RawFacts.OfType<Buff>()
                    .Single(value => ReferenceEquals(value.Blueprint, set.ArmedMarker));
                RuleAttackWithWeapon success = CreateResolvedStunningShotAttack(
                    attacker, passedTarget, weapon, false);
                SetExactProperty(success.AttackRoll, "IsCriticalConfirmed", true);
                UnityEngine.Random.InitState(FindNativeD20Seed(20));
                marker.CallComponents<IInitiatorRulebookHandler<RuleAttackWithWeapon>>(
                    handler => handler.OnEventDidTrigger(success));
                gritAfterSuccess = attacker.Descriptor.Resources.GetResourceAmount(
                    gunslinger.Grit.Resource);
                passedKilled = passedTarget.Descriptor.State.IsDead;
                passedMarked = passedTarget.Descriptor.State.MarkedForDeath ||
                    passedTarget.Descriptor.State.ForceKill;
                passedHp = passedTarget.HPLeft;
            }
            catch (Exception exception)
            { throw new InvalidOperationException("Disposable Death's Shot failed at " +
                stage + ".", exception); }
            finally
            {
                if (weapon != null)
                {
                    FirearmRuntimeState.Service.Forget(weapon);
                    if (attacker != null && attacker.Body.PrimaryHand.MaybeItem != null)
                        attacker.Body.PrimaryHand.RemoveItem(false);
                }
                if (passedRegistered && passedTarget != null)
                    Kingmaker.Game.Instance.State.Units.All.Remove(passedTarget);
                if (failedRegistered && failedTarget != null)
                    Kingmaker.Game.Instance.State.Units.All.Remove(failedTarget);
                if (failedTarget != null) failedTarget.Dispose();
                if (passedTarget != null) passedTarget.Dispose();
                if (attacker != null) attacker.Dispose();
                cleaned = SameReferences(partyBefore, SnapshotReferences(party)) &&
                    SameReferences(unitsBefore, SnapshotReferences(allUnits)) &&
                    Kingmaker.Game.Instance.State.Units.Count == unitPoolBefore;
            }
            string observed = "grit=" + gritBefore + "->" + gritAfterFailure +
                "->" + gritAfterSuccess + ";failedKilled=" + failedKilled +
                ";passedKilled=" + passedKilled;
            observed += ";failedMarked=" + failedMarked + ";passedMarked=" +
                passedMarked;
            observed += ";hp=" + failedHp + "," + passedHp;
            MethodInfo nativeKill = typeof(ContextActionKillTarget).GetMethod(
                "RunAction", BindingFlags.Instance | BindingFlags.Public |
                BindingFlags.NonPublic);
            observed += ";killCalls=" + DescribeCalledMethods(nativeKill);
            var assertions = new List<RuntimeTestAssertion>
            {
                Assertion("deaths-shot-progression", "level 19 feature", observed,
                    gunslinger.Progression.LevelEntries[18].Features.Any(value =>
                        ReferenceEquals(value, set.Feature)), "production progression"),
                Assertion("deaths-shot-native-death",
                    "natural-1 Fortitude failure kills; natural-20 succeeds", observed,
                    (failedKilled || failedMarked || failedHp <= 0) &&
                    !passedKilled && !passedMarked && passedHp > 0,
                    "Death descriptor and ContextActionKillTarget"),
                Assertion("deaths-shot-grit", "one grit per confirmed critical", observed,
                    gritAfterFailure == gritBefore - 1 && gritAfterSuccess == 0,
                    "unit-owned grit with request-local refill between branches"),
                Assertion("external-isolation", "disposables cleaned", "cleaned=" + cleaned,
                    cleaned, "reference snapshots"),
                Assertion("loaded-mod-version", _request.ExpectedModVersion,
                    _context.ModEntry.Info.Version,
                    _request.ExpectedModVersion == _context.ModEntry.Info.Version,
                    "Unity Mod Manager ModEntry.Info.Version")
            };
            return CreateResult(assertions.TrueForAll(value => value.Status == "PASS")
                ? RuntimeTestStatuses.Pass : RuntimeTestStatuses.Fail, assertions, null);
        }

        private static RuleAttackWithWeapon CreateResolvedStunningShotAttack(
            Kingmaker.EntitySystem.Entities.UnitEntityData attacker,
            Kingmaker.EntitySystem.Entities.UnitEntityData target,
            ItemEntityWeapon weapon, bool immuneToCritical)
        {
            var attack = new RuleAttackWithWeapon(attacker, target, weapon, 0);
            var roll = new RuleAttackRoll(attacker, target, weapon, 0);
            roll.RuleAttackWithWeapon = attack;
            roll.AutoHit = true;
            SetExactProperty(attack, "AttackRoll", roll);
            FirearmRuntimeState.Service.Set(weapon, new FirearmState(
                FirearmState.CurrentSchemaVersion, 1,
                FirearmStateTokenCatalog.DiagnosticLeadBall,
                FirearmCondition.Normal));
            FirearmMisfireRuntime.QueueForcedNaturalRoll(19);
            try { Rulebook.Trigger(roll); }
            finally { FirearmMisfireRuntime.CancelForcedNaturalRoll(); }
            if (!roll.IsHit)
                throw new InvalidOperationException(
                    "Native AutoHit attack-roll resolution did not hit.");
            roll.ImmuneToCriticalHit = immuneToCritical;
            return attack;
        }

        private static int FindNativeD20Seed(int expected)
        {
            for (int seed = 1; seed <= 100000; seed++)
            {
                UnityEngine.Random.InitState(seed);
                if (RulebookEvent.Dice.D20.Value == expected) return seed;
            }
            throw new InvalidOperationException(
                "No deterministic native d20 seed produced " + expected + ".");
        }

        private RuntimeTestResult RunObserveEvasiveNativeFeatures()
        {
            const string EvasionGuid = "576933720c440aa4d8d42b0c54b77e80";
            const string UncannyDodgeGuid = "3c08d842e802c3e4eb19d15496145709";
            const string ImprovedUncannyDodgeGuid =
                "485a18c05792521459c7d06c63128c79";
            BlueprintFeature evasion = BlueprintLibraryLookup
                .RequireExact<BlueprintFeature>(BlueprintBootstrap.Library,
                    EvasionGuid, "native Evasion feature");
            BlueprintFeature uncanny = BlueprintLibraryLookup
                .RequireExact<BlueprintFeature>(BlueprintBootstrap.Library,
                    UncannyDodgeGuid, "native Uncanny Dodge feature");
            BlueprintFeature improved = BlueprintLibraryLookup
                .RequireExact<BlueprintFeature>(BlueprintBootstrap.Library,
                    ImprovedUncannyDodgeGuid,
                    "native Improved Uncanny Dodge feature");
            string evasionObserved = DescribeExactFeature(evasion);
            string uncannyObserved = DescribeExactFeature(uncanny);
            string improvedObserved = DescribeExactFeature(improved);
            var assertions = new List<RuntimeTestAssertion>
            {
                Assertion("evasive-native-evasion", EvasionGuid,
                    evasionObserved, evasion.ComponentsArray != null &&
                        evasion.ComponentsArray.Length > 0,
                    "exact installed blueprint and declared component fields"),
                Assertion("evasive-native-uncanny-dodge", UncannyDodgeGuid,
                    uncannyObserved, uncanny.ComponentsArray != null &&
                        uncanny.ComponentsArray.Length > 0,
                    "exact installed blueprint and declared component fields"),
                Assertion("evasive-native-improved-uncanny-dodge",
                    ImprovedUncannyDodgeGuid, improvedObserved,
                    improved.ComponentsArray != null &&
                        improved.ComponentsArray.Length > 0,
                    "exact installed blueprint and declared component fields"),
                Assertion("loaded-mod-version", _request.ExpectedModVersion,
                    _context.ModEntry.Info.Version,
                    _request.ExpectedModVersion == _context.ModEntry.Info.Version,
                    "Unity Mod Manager ModEntry.Info.Version")
            };
            return CreateResult(assertions.TrueForAll(value => value.Status == "PASS")
                ? RuntimeTestStatuses.Pass : RuntimeTestStatuses.Fail,
                assertions, null);
        }

        private static string DescribeExactFeature(BlueprintFeature feature)
        {
            return feature.name + "|" + feature.Name + "|" +
                DescribeComponents(feature.ComponentsArray);
        }

        private static string DescribeComponents(BlueprintComponent[] source)
        {
            var components = new List<string>();
            foreach (BlueprintComponent component in source ??
                Array.Empty<BlueprintComponent>())
            {
                Type type = component.GetType();
                var fields = new List<string>();
                foreach (FieldInfo field in type.GetFields(BindingFlags.Instance |
                    BindingFlags.Public | BindingFlags.NonPublic |
                    BindingFlags.DeclaredOnly).OrderBy(value => value.Name,
                        StringComparer.Ordinal))
                {
                    object value = field.GetValue(component);
                    if (value == null || value is string || value is bool ||
                        value is int || value is Enum ||
                        value is BlueprintScriptableObject)
                        fields.Add(field.Name + "=" +
                            (value == null ? "<null>" : value.ToString()));
                }
                components.Add(type.FullName + "{" +
                    string.Join(",", fields.ToArray()) + "}");
            }
            return string.Join(";", components.ToArray());
        }

        private static string DescribeNestedObject(object value, int depth)
        {
            if (value == null) return "<null>";
            Type type = value.GetType();
            if (value is string || value is bool || value is int ||
                value is float || value is double || value is decimal ||
                value is Enum) return value.ToString();
            BlueprintScriptableObject blueprint = value as BlueprintScriptableObject;
            if (blueprint != null)
                return blueprint.name + "@" + blueprint.AssetGuid;
            if (depth <= 0) return type.FullName;
            IEnumerable enumerable = value as IEnumerable;
            if (enumerable != null)
            {
                var items = new List<string>();
                foreach (object item in enumerable)
                {
                    if (items.Count == 32) { items.Add("<truncated>"); break; }
                    items.Add(DescribeNestedObject(item, depth - 1));
                }
                return "[" + string.Join(",", items.ToArray()) + "]";
            }
            var members = new List<string>();
            foreach (FieldInfo field in type.GetFields(BindingFlags.Instance |
                BindingFlags.Public | BindingFlags.NonPublic |
                BindingFlags.DeclaredOnly).OrderBy(item => item.Name,
                    StringComparer.Ordinal))
            {
                if (field.Name.Contains("k__BackingField")) continue;
                members.Add(field.Name + "=" + DescribeNestedObject(
                    field.GetValue(value), depth - 1));
            }
            foreach (PropertyInfo property in type.GetProperties(
                BindingFlags.Instance | BindingFlags.Public |
                BindingFlags.DeclaredOnly).OrderBy(item => item.Name,
                    StringComparer.Ordinal))
            {
                if (!property.CanRead || property.GetIndexParameters().Length != 0)
                    continue;
                try
                {
                    members.Add(property.Name + "=" + DescribeNestedObject(
                        property.GetValue(value, null), depth - 1));
                }
                catch (Exception exception)
                {
                    members.Add(property.Name + "=<" + exception.GetType().Name + ">");
                }
            }
            return type.FullName + "{" + string.Join(",", members.ToArray()) + "}";
        }

        private RuntimeTestResult RunDisposableGunslingerBleedingWound()
        {
            BlueprintUnit source = BlueprintRoot.Instance.DefaultPlayerCharacter;
            GunslingerClassBlueprintSet gunslinger = BlueprintBootstrap.GunslingerClass;
            BleedingWoundBlueprintSet set = gunslinger.BleedingWound;
            BlueprintItemWeapon pistol = BlueprintBootstrap.ProductionFirearms.Pistol.Item;
            object player = ReadExactMember(Kingmaker.Game.Instance, "Player");
            object state = ReadExactMember(Kingmaker.Game.Instance, "State");
            object party = ReadExactMember(player, "Party");
            object allUnits = ReadExactMember(state, "AllUnits");
            object[] partyBefore = SnapshotReferences(party);
            object[] unitsBefore = SnapshotReferences(allUnits);
            Kingmaker.EntitySystem.Entities.UnitEntityData attacker = null;
            Kingmaker.EntitySystem.Entities.UnitEntityData target = null;
            ItemEntityWeapon weapon = null;
            int gritBefore = -1, gritAfter = -1, hpTick = -1,
                strengthBefore = -1, strengthAfter = -1, missRounds = -1;
            int hpShotDamage = -1, statShotDamage = -1;
            bool hpRemoved = false, missApplied = false, cleaned = false;
            string stage = "blueprint-contract";
            bool blueprintContract = gunslinger.Progression.LevelEntries[10]
                .Features.Count(value => ReferenceEquals(value, set.Feature)) == 1 &&
                set.Abilities.Length == 4 && set.Markers.Length == 4 &&
                set.Bleeds.Length == 4 && set.Abilities.All(value =>
                    value.ActionType == UnitCommand.CommandType.Free);
            try
            {
                attacker = new Kingmaker.UI.LevelUp.ChargenUnit(source).Unit;
                target = new Kingmaker.UI.LevelUp.ChargenUnit(source).Unit;
                target.Descriptor.State.Immortality.ReleaseAll();
                target.Descriptor.Stats.Constitution.BaseValue = 20;
                attacker.Descriptor.Stats.Dexterity.BaseValue = 18;
                attacker.Descriptor.Stats.Wisdom.BaseValue = 18;
                attacker.Descriptor.AddFact(gunslinger.Grit.Feature);
                attacker.Descriptor.Resources.Restore(gunslinger.Grit.Resource, 4);
                gritBefore = attacker.Descriptor.Resources.GetResourceAmount(
                    gunslinger.Grit.Resource);
                weapon = new ItemEntityWeapon(pistol);
                attacker.Body.PrimaryHand.InsertItem(weapon);

                stage = "hit-point-bleed";
                var hpContext = new MechanicsContext(attacker,
                    attacker.Descriptor, set.Abilities[0], null,
                    new TargetWrapper(attacker));
                attacker.Descriptor.Buffs.AddBuff(set.Markers[0], hpContext, null);
                SetRuntimeLoadedRound(weapon);
                FirearmMisfireRuntime.QueueForcedNaturalRoll(18);
                var hpAttack = new RuleAttackWithWeapon(attacker, target, weapon, 0);
                Rulebook.Trigger(hpAttack);
                var hpDamage = hpAttack.CreateRuleDealDamage(false);
                Rulebook.Trigger(hpDamage);
                hpShotDamage = hpDamage.Damage;
                Buff hpBleed = target.Descriptor.Buffs.RawFacts.OfType<Buff>()
                    .SingleOrDefault(value => ReferenceEquals(value.Blueprint,
                        set.Bleeds[0]));
                if (hpBleed == null)
                    throw new InvalidOperationException("HP bleed fact was absent.");
                int hpBefore = target.HPLeft;
                hpBleed.TickMechanics();
                hpTick = hpBefore - target.HPLeft;
                target.Descriptor.Buffs.RemoveFact(hpBleed);
                hpRemoved = !target.Descriptor.Buffs.RawFacts.OfType<Buff>()
                    .Any(value => ReferenceEquals(value.Blueprint, set.Bleeds[0]));

                stage = "strength-bleed";
                var statContext = new MechanicsContext(attacker,
                    attacker.Descriptor, set.Abilities[1], null,
                    new TargetWrapper(attacker));
                attacker.Descriptor.Buffs.AddBuff(set.Markers[1], statContext,
                    null);
                SetRuntimeLoadedRound(weapon);
                FirearmMisfireRuntime.QueueForcedNaturalRoll(18);
                var statAttack = new RuleAttackWithWeapon(attacker, target,
                    weapon, 0);
                Rulebook.Trigger(statAttack);
                var statDamage = statAttack.CreateRuleDealDamage(false);
                Rulebook.Trigger(statDamage);
                statShotDamage = statDamage.Damage;
                Buff statBleed = target.Descriptor.Buffs.RawFacts.OfType<Buff>()
                    .SingleOrDefault(value => ReferenceEquals(value.Blueprint,
                        set.Bleeds[1]));
                if (statBleed == null)
                    throw new InvalidOperationException(
                        "Strength bleed fact was absent.");
                strengthBefore = target.Stats.Strength.Damage;
                statBleed.TickMechanics();
                strengthAfter = target.Stats.Strength.Damage;
                target.Descriptor.Buffs.RemoveFact(statBleed);

                stage = "miss-rejection";
                var missContext = new MechanicsContext(attacker,
                    attacker.Descriptor, set.Abilities[2], null,
                    new TargetWrapper(attacker));
                attacker.Descriptor.Buffs.AddBuff(set.Markers[2], missContext,
                    null);
                SetRuntimeLoadedRound(weapon);
                FirearmMisfireRuntime.QueueForcedNaturalRoll(1);
                Rulebook.Trigger(new RuleAttackWithWeapon(attacker, target,
                    weapon, 0));
                missRounds = FirearmRuntimeState.Service.GetOrCreate(weapon)
                    .Repository.State.LoadedRounds;
                missApplied = target.Descriptor.Buffs.RawFacts.OfType<Buff>()
                    .Any(value => ReferenceEquals(value.Blueprint, set.Bleeds[2]));
                gritAfter = attacker.Descriptor.Resources.GetResourceAmount(
                    gunslinger.Grit.Resource);
            }
            catch (Exception exception)
            {
                throw new InvalidOperationException(
                    "Disposable Bleeding Wound failed at stage " + stage + ".",
                    exception);
            }
            finally
            {
                FirearmMisfireRuntime.CancelForcedNaturalRoll();
                if (weapon != null)
                {
                    FirearmRuntimeState.Service.Forget(weapon);
                    if (attacker != null && attacker.Body.PrimaryHand.MaybeItem != null)
                        attacker.Body.PrimaryHand.RemoveItem(false);
                }
                if (target != null) target.Dispose();
                if (attacker != null) attacker.Dispose();
                cleaned = SameReferences(partyBefore, SnapshotReferences(party)) &&
                    SameReferences(unitsBefore, SnapshotReferences(allUnits)) &&
                    (attacker == null || !ContainsReference(allUnits, attacker)) &&
                    (target == null || !ContainsReference(allUnits, target));
            }
            string observed = "grit=" + gritBefore + "->" + gritAfter +
                ";shotDamage=" + hpShotDamage + "," + statShotDamage +
                ";hpTick=" + hpTick + ";hpRemoved=" + hpRemoved +
                ";strength=" + strengthBefore + "->" + strengthAfter +
                ";missRounds=" + missRounds + ";missApplied=" + missApplied;
            var assertions = new List<RuntimeTestAssertion>
            {
                Assertion("bleeding-wound-progression",
                    "level 11 feature and four free-action choices", observed,
                    blueprintContract, "production blueprint contracts"),
                Assertion("bleeding-wound-hit-points",
                    "ordinary damage plus Dexterity-modifier recurring HP bleed",
                    observed, hpShotDamage > 0 && hpTick == 4 && hpRemoved,
                    "native weapon/direct damage and Bleed removal"),
                Assertion("bleeding-wound-ability-damage",
                    "ordinary damage plus one recurring Strength damage", observed,
                    statShotDamage > 0 && strengthAfter == strengthBefore + 1,
                    "native weapon and RuleDealStatDamage"),
                Assertion("bleeding-wound-costs",
                    "one plus two grit spent; miss spends none", observed,
                    gritAfter == gritBefore - 3,
                    "unit-owned grit resource"),
                Assertion("bleeding-wound-miss",
                    "miss consumes chamber and marker without bleed", observed,
                    missRounds == 0 && !missApplied,
                    "forced natural-one firearm attack"),
                Assertion("external-isolation",
                    "unchanged party and global-unit snapshots", "cleaned=" + cleaned,
                    cleaned, "items/facts removed and disposables disposed"),
                Assertion("loaded-mod-version", _request.ExpectedModVersion,
                    _context.ModEntry.Info.Version,
                    _request.ExpectedModVersion == _context.ModEntry.Info.Version,
                    "Unity Mod Manager ModEntry.Info.Version")
            };
            return CreateResult(assertions.TrueForAll(value => value.Status == "PASS")
                ? RuntimeTestStatuses.Pass : RuntimeTestStatuses.Fail,
                assertions, null);
        }

        private RuntimeTestResult RunDisposableGunslingerTargetingArms()
        {
            BlueprintUnit source = BlueprintRoot.Instance.DefaultPlayerCharacter;
            GunslingerClassBlueprintSet gunslinger = BlueprintBootstrap.GunslingerClass;
            BlueprintItemWeapon pistol = BlueprintBootstrap.ProductionFirearms.Pistol.Item;
            object player = ReadExactMember(Kingmaker.Game.Instance, "Player");
            object state = ReadExactMember(Kingmaker.Game.Instance, "State");
            object party = ReadExactMember(player, "Party");
            object allUnits = ReadExactMember(state, "AllUnits");
            object[] partyBefore = SnapshotReferences(party);
            object[] unitsBefore = SnapshotReferences(allUnits);
            Kingmaker.EntitySystem.Entities.UnitEntityData attacker = null;
            Kingmaker.EntitySystem.Entities.UnitEntityData target = null;
            ItemEntityWeapon weapon = null;
            TargetingArmsResult result = null;
            int gritBefore = -1, gritAfter = -1, roundsBefore = -1,
                roundsAfter = -1, damageBefore = -1, damageAfter = -1;
            bool cleaned = false;
            double durationSeconds = -1d;
            string stage = "blueprint-contract";
            int levelSevenCount = gunslinger.Progression.LevelEntries[6].Features
                .Count(value => ReferenceEquals(value,
                    gunslinger.TargetingArms.Feature));
            bool blueprintContract = levelSevenCount == 1 &&
                gunslinger.TargetingArms.Ability.IsFullRoundAction &&
                gunslinger.TargetingArms.Ability.Range == AbilityRange.Weapon;
            try
            {
                attacker = new Kingmaker.UI.LevelUp.ChargenUnit(source).Unit;
                target = new Kingmaker.UI.LevelUp.ChargenUnit(source).Unit;
                attacker.Descriptor.Stats.Wisdom.BaseValue = 18;
                attacker.Descriptor.AddFact(gunslinger.Grit.Feature);
                attacker.Descriptor.Resources.Restore(gunslinger.Grit.Resource, 1);
                gritBefore = attacker.Descriptor.Resources.GetResourceAmount(
                    gunslinger.Grit.Resource);
                damageBefore = target.Damage;
                weapon = new ItemEntityWeapon(pistol);
                attacker.Body.PrimaryHand.InsertItem(weapon);
                FirearmRuntimeState.Service.Set(weapon, new FirearmState(
                    FirearmState.CurrentSchemaVersion, 1,
                    FirearmStateTokenCatalog.DiagnosticLeadBall,
                    FirearmCondition.Normal));
                roundsBefore = FirearmRuntimeState.Service.GetOrCreate(weapon)
                    .Repository.State.LoadedRounds;
                stage = "native-arms-attack";
                var abilityData = new Kingmaker.UnitLogic.Abilities.AbilityData(
                    gunslinger.TargetingArms.Ability, attacker.Descriptor);
                var abilityParams = new Kingmaker.UnitLogic.Abilities.AbilityParams();
                var mechanicsContext =
                    new Kingmaker.UnitLogic.Abilities.AbilityExecutionContext(
                        abilityData, abilityParams,
                        new Kingmaker.Utility.TargetWrapper(target), null);
                result = TargetingArmsRuntime.ExecuteForRuntimeTest(attacker,
                    target, mechanicsContext, true);
                gritAfter = attacker.Descriptor.Resources.GetResourceAmount(
                    gunslinger.Grit.Resource);
                roundsAfter = FirearmRuntimeState.Service.GetOrCreate(weapon)
                    .Repository.State.LoadedRounds;
                damageAfter = target.Damage;
                if (result.Buff != null)
                    durationSeconds = result.Buff.TimeLeft.TotalSeconds;
            }
            catch (Exception exception)
            {
                throw new InvalidOperationException(
                    "Disposable Targeting Arms failed at stage " + stage + ".",
                    exception);
            }
            finally
            {
                if (result != null && result.Buff != null && target != null)
                    target.Descriptor.Buffs.RemoveFact(result.Buff);
                if (weapon != null)
                {
                    FirearmRuntimeState.Service.Forget(weapon);
                    if (attacker != null && attacker.Body.PrimaryHand.MaybeItem != null)
                        attacker.Body.PrimaryHand.RemoveItem(false);
                }
                if (target != null) target.Damage = Math.Max(0, damageBefore);
                if (target != null) target.Dispose();
                if (attacker != null) attacker.Dispose();
                cleaned = SameReferences(partyBefore, SnapshotReferences(party)) &&
                    SameReferences(unitsBefore, SnapshotReferences(allUnits)) &&
                    (attacker == null || !ContainsReference(allUnits, attacker)) &&
                    (target == null || !ContainsReference(allUnits, target));
            }
            string observed = "rounds=" + roundsBefore + "->" + roundsAfter +
                ";grit=" + gritBefore + "->" + gritAfter + ";damage=" +
                damageBefore + "->" + damageAfter + ";durationSeconds=" +
                durationSeconds.ToString("F3",
                    System.Globalization.CultureInfo.InvariantCulture) +
                ";hit=" + (result != null && result.Hit);
            var assertions = new List<RuntimeTestAssertion>
            {
                Assertion("targeting-arms-progression",
                    "level 7 full-round weapon ability", observed,
                    blueprintContract, "production blueprint contracts"),
                Assertion("targeting-arms-attack",
                    "one grit, one chamber, hit without damage", observed,
                    result != null && result.Hit && gritAfter == gritBefore - 1 &&
                    roundsBefore == 1 && roundsAfter == 0 &&
                    damageAfter == damageBefore, "native attack without damage delivery"),
                Assertion("targeting-arms-rider",
                    "eligible hit applies native main-hand disarm for one round",
                    observed, result != null && result.Rider != null &&
                    result.Rider.ShouldDisableMainHand && result.Buff != null &&
                    !result.Buff.IsPermanent && durationSeconds > 0d &&
                    durationSeconds <= 6.1d, "native DisarmMainHandBuff"),
                Assertion("external-isolation",
                    "unchanged party and global-unit snapshots", "cleaned=" + cleaned,
                    cleaned, "buff/item state removed and disposables disposed"),
                Assertion("loaded-mod-version", _request.ExpectedModVersion,
                    _context.ModEntry.Info.Version,
                    _request.ExpectedModVersion == _context.ModEntry.Info.Version,
                    "Unity Mod Manager ModEntry.Info.Version")
            };
            return CreateResult(assertions.TrueForAll(value => value.Status == "PASS")
                ? RuntimeTestStatuses.Pass : RuntimeTestStatuses.Fail,
                assertions, null);
        }

        private RuntimeTestResult RunDisposableGunslingerTargetingLegs()
        {
            BlueprintUnit source = BlueprintRoot.Instance.DefaultPlayerCharacter;
            GunslingerClassBlueprintSet gunslinger = BlueprintBootstrap.GunslingerClass;
            BlueprintItemWeapon pistol = BlueprintBootstrap.ProductionFirearms.Pistol.Item;
            object player = ReadExactMember(Kingmaker.Game.Instance, "Player");
            object state = ReadExactMember(Kingmaker.Game.Instance, "State");
            object party = ReadExactMember(player, "Party");
            object allUnits = ReadExactMember(state, "AllUnits");
            object[] partyBefore = SnapshotReferences(party);
            object[] unitsBefore = SnapshotReferences(allUnits);
            Kingmaker.EntitySystem.Entities.UnitEntityData attacker = null;
            Kingmaker.EntitySystem.Entities.UnitEntityData target = null;
            ItemEntityWeapon weapon = null;
            TargetingLegsResult eligible = null, immune = null;
            int gritBefore = -1, gritAfter = -1, roundsEligible = -1,
                roundsImmune = -1;
            bool proneBefore = false, proneAfter = false, immuneProne = false,
                cleaned = false;
            string stage = "blueprint-contract";
            int levelSevenCount = gunslinger.Progression.LevelEntries[6].Features
                .Count(value => ReferenceEquals(value,
                    gunslinger.TargetingLegs.Feature));
            bool blueprintContract = levelSevenCount == 1 &&
                gunslinger.TargetingLegs.Ability.IsFullRoundAction &&
                gunslinger.TargetingLegs.Ability.Range == AbilityRange.Weapon;
            try
            {
                attacker = new Kingmaker.UI.LevelUp.ChargenUnit(source).Unit;
                target = new Kingmaker.UI.LevelUp.ChargenUnit(source).Unit;
                target.Descriptor.State.Immortality.ReleaseAll();
                target.Descriptor.Stats.Constitution.BaseValue = 20;
                attacker.Descriptor.Stats.Wisdom.BaseValue = 18;
                attacker.Descriptor.AddFact(gunslinger.Grit.Feature);
                attacker.Descriptor.Resources.Restore(gunslinger.Grit.Resource, 2);
                gritBefore = attacker.Descriptor.Resources.GetResourceAmount(
                    gunslinger.Grit.Resource);
                weapon = new ItemEntityWeapon(pistol);
                attacker.Body.PrimaryHand.InsertItem(weapon);
                FirearmRuntimeState.Service.Set(weapon, new FirearmState(
                    FirearmState.CurrentSchemaVersion, 1,
                    FirearmStateTokenCatalog.DiagnosticLeadBall,
                    FirearmCondition.Normal));

                stage = "eligible-trip";
                proneBefore = target.Descriptor.State.HasCondition(
                    UnitCondition.Prone);
                FirearmMisfireRuntime.QueueForcedNaturalRoll(18);
                eligible = TargetingLegsRuntime.ExecuteForRuntimeTest(attacker,
                    target);
                roundsEligible = FirearmRuntimeState.Service.GetOrCreate(weapon)
                    .Repository.State.LoadedRounds;
                proneAfter = target.Descriptor.State.HasCondition(
                    UnitCondition.Prone);
                if (proneAfter)
                    target.Descriptor.State.RemoveCondition(UnitCondition.Prone);

                FirearmRuntimeState.Service.Set(weapon, new FirearmState(
                    FirearmState.CurrentSchemaVersion, 1,
                    FirearmStateTokenCatalog.DiagnosticLeadBall,
                    FirearmCondition.Normal));
                target.Descriptor.State.AddCondition(
                    UnitCondition.ImmuneToCombatManeuvers, null);
                stage = "immune-trip";
                FirearmMisfireRuntime.QueueForcedNaturalRoll(18);
                immune = TargetingLegsRuntime.ExecuteForRuntimeTest(attacker,
                    target);
                roundsImmune = FirearmRuntimeState.Service.GetOrCreate(weapon)
                    .Repository.State.LoadedRounds;
                immuneProne = target.Descriptor.State.HasCondition(
                    UnitCondition.Prone);
                gritAfter = attacker.Descriptor.Resources.GetResourceAmount(
                    gunslinger.Grit.Resource);
            }
            catch (Exception exception)
            {
                throw new InvalidOperationException(
                    "Disposable Targeting Legs failed at stage " + stage + ".",
                    exception);
            }
            finally
            {
                FirearmMisfireRuntime.CancelForcedNaturalRoll();
                if (target != null)
                {
                    target.Descriptor.State.RemoveCondition(
                        UnitCondition.ImmuneToCombatManeuvers);
                    target.Descriptor.State.RemoveCondition(UnitCondition.Prone);
                }
                if (weapon != null)
                {
                    FirearmRuntimeState.Service.Forget(weapon);
                    if (attacker != null && attacker.Body.PrimaryHand.MaybeItem != null)
                        attacker.Body.PrimaryHand.RemoveItem(false);
                }
                if (target != null) target.Dispose();
                if (attacker != null) attacker.Dispose();
                cleaned = SameReferences(partyBefore, SnapshotReferences(party)) &&
                    SameReferences(unitsBefore, SnapshotReferences(allUnits)) &&
                    (attacker == null || !ContainsReference(allUnits, attacker)) &&
                    (target == null || !ContainsReference(allUnits, target));
            }
            int eligibleDamage = eligible == null || eligible.Damage == null ? -1 :
                eligible.Damage.Damage;
            int immuneDamage = immune == null || immune.Damage == null ? -1 :
                immune.Damage.Damage;
            string observed = "grit=" + gritBefore + "->" + gritAfter +
                ";rounds=" + roundsEligible + "," + roundsImmune +
                ";eligibleDamage=" + eligibleDamage + ";prone=" + proneBefore +
                "->" + proneAfter + ";tripSuccess=" + (eligible != null &&
                    eligible.Trip != null && eligible.Trip.Success) +
                ";immuneDamage=" + immuneDamage + ";immuneTrip=" +
                (immune != null && immune.Trip != null) + ";immuneProne=" +
                immuneProne;
            var assertions = new List<RuntimeTestAssertion>
            {
                Assertion("targeting-legs-progression",
                    "level 7 full-round weapon ability", observed,
                    blueprintContract, "production blueprint contracts"),
                Assertion("targeting-legs-damage",
                    "two hits, positive native damage, one chamber each", observed,
                    eligible != null && eligible.Hit && eligibleDamage > 0 &&
                    immune != null && immune.Hit && immuneDamage > 0 &&
                    roundsEligible == 0 && roundsImmune == 0,
                    "native attack and damage rules"),
                Assertion("targeting-legs-trip",
                    "eligible native Trip succeeds and applies prone", observed,
                    eligible != null && eligible.Rider.ShouldTrip &&
                    eligible.Trip != null && eligible.Trip.Success &&
                    !proneBefore && proneAfter, "native Trip maneuver"),
                Assertion("targeting-legs-immunity",
                    "native maneuver immunity suppresses Trip and prone", observed,
                    immune != null && !immune.Rider.ShouldTrip &&
                    immune.Trip == null && !immuneProne,
                    "native ImmuneToCombatManeuvers condition"),
                Assertion("targeting-legs-grit", "exactly two grit spent", observed,
                    gritAfter == gritBefore - 2, "unit-owned grit resource"),
                Assertion("external-isolation", "unchanged party and global-unit snapshots",
                    "cleaned=" + cleaned, cleaned,
                    "conditions/item state removed and disposables disposed"),
                Assertion("loaded-mod-version", _request.ExpectedModVersion,
                    _context.ModEntry.Info.Version,
                    _request.ExpectedModVersion == _context.ModEntry.Info.Version,
                    "Unity Mod Manager ModEntry.Info.Version")
            };
            return CreateResult(assertions.TrueForAll(value => value.Status == "PASS")
                ? RuntimeTestStatuses.Pass : RuntimeTestStatuses.Fail,
                assertions, null);
        }

        private RuntimeTestResult RunDisposableGunslingerTargetingTorso()
        {
            BlueprintUnit source = BlueprintRoot.Instance.DefaultPlayerCharacter;
            GunslingerClassBlueprintSet gunslinger = BlueprintBootstrap.GunslingerClass;
            BlueprintItemWeapon pistol = BlueprintBootstrap.ProductionFirearms.Pistol.Item;
            object player = ReadExactMember(Kingmaker.Game.Instance, "Player");
            object state = ReadExactMember(Kingmaker.Game.Instance, "State");
            object party = ReadExactMember(player, "Party");
            object allUnits = ReadExactMember(state, "AllUnits");
            object[] partyBefore = SnapshotReferences(party);
            object[] unitsBefore = SnapshotReferences(allUnits);
            Kingmaker.EntitySystem.Entities.UnitEntityData attacker = null;
            Kingmaker.EntitySystem.Entities.UnitEntityData target = null;
            ItemEntityWeapon weapon = null;
            TargetingTorsoResult roll18 = null, roll19 = null;
            int gritBefore = -1, gritAfter = -1, roundsAfter18 = -1,
                roundsAfter19 = -1;
            bool cleaned = false;
            string stage = "blueprint-contract";
            int levelSevenCount = gunslinger.Progression.LevelEntries[6].Features
                .Count(value => ReferenceEquals(value,
                    gunslinger.TargetingTorso.Feature));
            bool blueprintContract = levelSevenCount == 1 &&
                gunslinger.TargetingTorso.Ability.IsFullRoundAction &&
                gunslinger.TargetingTorso.Ability.Range == AbilityRange.Weapon;
            try
            {
                attacker = new Kingmaker.UI.LevelUp.ChargenUnit(source).Unit;
                target = new Kingmaker.UI.LevelUp.ChargenUnit(source).Unit;
                target.Descriptor.State.Immortality.ReleaseAll();
                attacker.Descriptor.Stats.Wisdom.BaseValue = 18;
                attacker.Descriptor.AddFact(gunslinger.Grit.Feature);
                attacker.Descriptor.Resources.Restore(gunslinger.Grit.Resource, 2);
                gritBefore = attacker.Descriptor.Resources.GetResourceAmount(
                    gunslinger.Grit.Resource);
                weapon = new ItemEntityWeapon(pistol);
                attacker.Body.PrimaryHand.InsertItem(weapon);
                FirearmRuntimeState.Service.Set(weapon, new FirearmState(
                    FirearmState.CurrentSchemaVersion, 1,
                    FirearmStateTokenCatalog.DiagnosticLeadBall,
                    FirearmCondition.Normal));

                stage = "natural-18";
                FirearmMisfireRuntime.QueueForcedNaturalRoll(18);
                roll18 = TargetingTorsoRuntime.ExecuteForRuntimeTest(attacker,
                    target);
                roundsAfter18 = FirearmRuntimeState.Service.GetOrCreate(weapon)
                    .Repository.State.LoadedRounds;

                FirearmRuntimeState.Service.Set(weapon, new FirearmState(
                    FirearmState.CurrentSchemaVersion, 1,
                    FirearmStateTokenCatalog.DiagnosticLeadBall,
                    FirearmCondition.Normal));
                stage = "natural-19";
                FirearmMisfireRuntime.QueueForcedNaturalRoll(19);
                roll19 = TargetingTorsoRuntime.ExecuteForRuntimeTest(attacker,
                    target);
                roundsAfter19 = FirearmRuntimeState.Service.GetOrCreate(weapon)
                    .Repository.State.LoadedRounds;
                gritAfter = attacker.Descriptor.Resources.GetResourceAmount(
                    gunslinger.Grit.Resource);
            }
            catch (Exception exception)
            {
                throw new InvalidOperationException(
                    "Disposable Targeting Torso failed at stage " + stage + ".",
                    exception);
            }
            finally
            {
                FirearmMisfireRuntime.CancelForcedNaturalRoll();
                if (weapon != null)
                {
                    FirearmRuntimeState.Service.Forget(weapon);
                    if (attacker != null && attacker.Body.PrimaryHand.MaybeItem != null)
                        attacker.Body.PrimaryHand.RemoveItem(false);
                }
                if (target != null) target.Dispose();
                if (attacker != null) attacker.Dispose();
                cleaned = SameReferences(partyBefore, SnapshotReferences(party)) &&
                    SameReferences(unitsBefore, SnapshotReferences(allUnits)) &&
                    (attacker == null || !ContainsReference(allUnits, attacker)) &&
                    (target == null || !ContainsReference(allUnits, target));
            }
            bool eighteenHit = roll18 != null && roll18.Hit;
            bool nineteenHit = roll19 != null && roll19.Hit;
            bool eighteenThreat = eighteenHit &&
                roll18.Attack.AttackRoll.IsCriticalRoll;
            bool nineteenThreat = nineteenHit &&
                roll19.Attack.AttackRoll.IsCriticalRoll;
            int damage18 = roll18 == null || roll18.Damage == null ? -1 :
                roll18.Damage.Damage;
            int damage19 = roll19 == null || roll19.Damage == null ? -1 :
                roll19.Damage.Damage;
            string observed = "grit=" + gritBefore + "->" + gritAfter +
                ";rounds18=" + roundsAfter18 + ";rounds19=" + roundsAfter19 +
                ";hit18=" + eighteenHit + ";threat18=" + eighteenThreat +
                ";damage18=" + damage18 + ";hit19=" + nineteenHit +
                ";threat19=" + nineteenThreat + ";damage19=" + damage19;
            var assertions = new List<RuntimeTestAssertion>
            {
                Assertion("targeting-torso-progression",
                    "level 7 full-round weapon ability", observed,
                    blueprintContract, "production blueprint contracts"),
                Assertion("targeting-torso-natural-18",
                    "hit, ordinary non-threat, native damage, one chamber",
                    observed, eighteenHit && !eighteenThreat && damage18 > 0 &&
                    roundsAfter18 == 0, "marked native attack and damage rules"),
                Assertion("targeting-torso-natural-19",
                    "hit, deed-local threat, native damage, one chamber",
                    observed, nineteenHit && nineteenThreat && damage19 > 0 &&
                    roundsAfter19 == 0, "marked native attack and damage rules"),
                Assertion("targeting-torso-grit",
                    "exactly two grit spent", observed,
                    gritAfter == gritBefore - 2, "unit-owned grit resource"),
                Assertion("external-isolation", "unchanged party and global-unit snapshots",
                    "cleaned=" + cleaned, cleaned,
                    "item state removed and disposables disposed"),
                Assertion("loaded-mod-version", _request.ExpectedModVersion,
                    _context.ModEntry.Info.Version,
                    _request.ExpectedModVersion == _context.ModEntry.Info.Version,
                    "Unity Mod Manager ModEntry.Info.Version")
            };
            return CreateResult(assertions.TrueForAll(value => value.Status == "PASS")
                ? RuntimeTestStatuses.Pass : RuntimeTestStatuses.Fail,
                assertions, null);
        }

        private RuntimeTestResult RunDisposableGunslingerStopBleeding()
        {
            BlueprintUnit source = BlueprintRoot.Instance.DefaultPlayerCharacter;
            GunslingerClassBlueprintSet gunslinger = BlueprintBootstrap.GunslingerClass;
            BlueprintItemWeapon pistol = BlueprintBootstrap.ProductionFirearms.Pistol.Item;
            object player = ReadExactMember(Kingmaker.Game.Instance, "Player");
            object state = ReadExactMember(Kingmaker.Game.Instance, "State");
            object party = ReadExactMember(player, "Party");
            object allUnits = ReadExactMember(state, "AllUnits");
            object[] partyBefore = SnapshotReferences(party);
            object[] unitsBefore = SnapshotReferences(allUnits);
            Kingmaker.EntitySystem.Entities.UnitEntityData caster = null;
            Kingmaker.EntitySystem.Entities.UnitEntityData target = null;
            ItemEntityWeapon weapon = null;
            StopBleedingResult self = null, adjacent = null, rejected = null;
            int initialGrit = -1, afterSelfGrit = -1, afterAdjacentGrit = -1;
            int selfBleedsBefore = -1, selfBleedsAfter = -1;
            int targetBleedsBefore = -1, targetBleedsAfter = -1;
            int roundsAfterSelf = -1, roundsAfterAdjacent = -1,
                roundsAfterRejected = -1;
            bool cleaned = false;
            string stage = "construct-disposables";
            StopBleedingRuntimeDiagnostics.Reset();
            try
            {
                caster = new Kingmaker.UI.LevelUp.ChargenUnit(source).Unit;
                target = new Kingmaker.UI.LevelUp.ChargenUnit(source).Unit;
                caster.Descriptor.Stats.Wisdom.BaseValue = 14;
                caster.Descriptor.AddFact(gunslinger.Grit.Feature);
                initialGrit = caster.Descriptor.Resources.GetResourceAmount(
                    gunslinger.Grit.Resource);
                weapon = new ItemEntityWeapon(pistol);
                caster.Body.PrimaryHand.InsertItem(weapon);

                BlueprintBuff selfBleedOne = CreateRuntimeBleedBlueprint(
                    "KMG_Runtime_StopBleeding_Self_One");
                BlueprintBuff selfBleedTwo = CreateRuntimeBleedBlueprint(
                    "KMG_Runtime_StopBleeding_Self_Two");
                caster.Descriptor.AddFact(selfBleedOne);
                caster.Descriptor.AddFact(selfBleedTwo);
                selfBleedsBefore = StopBleedingRuntime.CountBleeds(caster);
                SetRuntimeLoadedRound(weapon);

                stage = "self-delivery";
                self = StopBleedingRuntime.Execute(caster.Descriptor, caster, caster);
                selfBleedsAfter = StopBleedingRuntime.CountBleeds(caster);
                roundsAfterSelf = FirearmRuntimeState.Service.GetOrCreate(weapon)
                    .Repository.State.LoadedRounds;
                afterSelfGrit = caster.Descriptor.Resources.GetResourceAmount(
                    gunslinger.Grit.Resource);

                BlueprintBuff targetBleed = CreateRuntimeBleedBlueprint(
                    "KMG_Runtime_StopBleeding_Adjacent");
                target.Descriptor.AddFact(targetBleed);
                targetBleedsBefore = StopBleedingRuntime.CountBleeds(target);
                SetRuntimeLoadedRound(weapon);

                stage = "adjacent-delivery";
                adjacent = StopBleedingRuntime.Execute(caster.Descriptor,
                    caster, target);
                targetBleedsAfter = StopBleedingRuntime.CountBleeds(target);
                roundsAfterAdjacent = FirearmRuntimeState.Service.GetOrCreate(weapon)
                    .Repository.State.LoadedRounds;
                afterAdjacentGrit = caster.Descriptor.Resources.GetResourceAmount(
                    gunslinger.Grit.Resource);

                stage = "zero-grit-rejection";
                SetRuntimeLoadedRound(weapon);
                caster.Descriptor.Resources.Spend(gunslinger.Grit.Resource,
                    afterAdjacentGrit);
                rejected = StopBleedingRuntime.Execute(caster.Descriptor,
                    caster, caster);
                roundsAfterRejected = FirearmRuntimeState.Service.GetOrCreate(weapon)
                    .Repository.State.LoadedRounds;
            }
            catch (Exception exception)
            {
                throw new InvalidOperationException(
                    "Disposable Stop Bleeding failed at stage " + stage + ".",
                    exception);
            }
            finally
            {
                if (weapon != null)
                {
                    FirearmRuntimeState.Service.Forget(weapon);
                    if (caster != null && caster.Body.PrimaryHand.MaybeItem != null)
                        caster.Body.PrimaryHand.RemoveItem(false);
                }
                if (caster != null)
                {
                    Buff bleed;
                    while ((bleed = StopBleedingRuntime.FirstBleed(caster)) != null)
                        caster.Descriptor.Buffs.RemoveFact(bleed);
                    if (caster.Descriptor.HasFact(gunslinger.Grit.Feature))
                        caster.Descriptor.RemoveFact(gunslinger.Grit.Feature);
                    caster.Dispose();
                }
                if (target != null)
                {
                    Buff bleed;
                    while ((bleed = StopBleedingRuntime.FirstBleed(target)) != null)
                        target.Descriptor.Buffs.RemoveFact(bleed);
                    target.Dispose();
                }
                cleaned = SameReferences(partyBefore, SnapshotReferences(party)) &&
                    SameReferences(unitsBefore, SnapshotReferences(allUnits)) &&
                    (caster == null || !ContainsReference(allUnits, caster)) &&
                    (target == null || !ContainsReference(allUnits, target));
            }
            string observed = "initialGrit=" + initialGrit +
                ";afterSelfGrit=" + afterSelfGrit +
                ";afterAdjacentGrit=" + afterAdjacentGrit +
                ";selfBleeds=" + selfBleedsBefore + "->" + selfBleedsAfter +
                ";targetBleeds=" + targetBleedsBefore + "->" + targetBleedsAfter +
                ";rounds=" + roundsAfterSelf + "," + roundsAfterAdjacent +
                "," + roundsAfterRejected + ";rejected=" +
                (rejected == null ? "null" : rejected.Decision.Status.ToString()) +
                ";applied=" + StopBleedingRuntimeDiagnostics.Applied +
                ";rejectedCount=" + StopBleedingRuntimeDiagnostics.Rejected +
                ";faults=" + StopBleedingRuntimeDiagnostics.Faults;
            var assertions = new List<RuntimeTestAssertion>
            {
                Assertion("stop-bleeding-self",
                    "two bleeds -> one; one chamber -> zero; grit unchanged",
                    observed, self != null && self.Applied && selfBleedsBefore == 2 &&
                    selfBleedsAfter == 1 && roundsAfterSelf == 0 && initialGrit > 0 &&
                    afterSelfGrit == initialGrit,
                    "native Bleed fact and exact item-owned firearm state"),
                Assertion("stop-bleeding-adjacent",
                    "one bleed -> zero; one chamber -> zero; grit unchanged",
                    observed, adjacent != null && adjacent.Applied &&
                    targetBleedsBefore == 1 && targetBleedsAfter == 0 &&
                    roundsAfterAdjacent == 0 && afterAdjacentGrit == initialGrit,
                    "detached adjacent target and native Bleed fact"),
                Assertion("stop-bleeding-zero-grit",
                    "InsufficientGrit; loaded chamber preserved", observed,
                    rejected != null &&
                    rejected.Decision.Status == StopBleedingStatus.InsufficientGrit &&
                    !rejected.Applied && roundsAfterRejected == 1,
                    "production fail-closed policy"),
                Assertion("stop-bleeding-diagnostics",
                    "applied=2;rejected=1;faults=0", observed,
                    StopBleedingRuntimeDiagnostics.Applied == 2 &&
                    StopBleedingRuntimeDiagnostics.Rejected == 1 &&
                    StopBleedingRuntimeDiagnostics.Faults == 0,
                    "production Stop Bleeding diagnostics"),
                Assertion("external-isolation",
                    "unchanged party and global-unit snapshots",
                    "cleaned=" + cleaned, cleaned,
                    "firearm removed, facts removed, detached units disposed"),
                Assertion("loaded-mod-version", _request.ExpectedModVersion,
                    _context.ModEntry.Info.Version,
                    _request.ExpectedModVersion == _context.ModEntry.Info.Version,
                    "Unity Mod Manager ModEntry.Info.Version")
            };
            return CreateResult(assertions.TrueForAll(value => value.Status == "PASS")
                ? RuntimeTestStatuses.Pass : RuntimeTestStatuses.Fail, assertions, null);
        }

        private static BlueprintBuff CreateRuntimeBleedBlueprint(string name)
        {
            var blueprint = ScriptableObject.CreateInstance<BlueprintBuff>();
            blueprint.name = name;
            var descriptor = ScriptableObject.CreateInstance<SpellDescriptorComponent>();
            descriptor.name = "$" + name + "_Descriptor";
            descriptor.Descriptor = SpellDescriptor.Bleed;
            blueprint.ComponentsArray = new BlueprintComponent[] { descriptor };
            return blueprint;
        }

        private static void SetRuntimeLoadedRound(ItemEntityWeapon weapon)
        {
            FirearmRuntimeState.Service.Set(weapon, new FirearmState(
                FirearmState.CurrentSchemaVersion, 1,
                FirearmStateTokenCatalog.DiagnosticLeadBall,
                FirearmCondition.Normal));
        }

        private RuntimeTestResult RunDisposableGunslingerPistolWhip()
        {
            BlueprintUnit source = BlueprintRoot.Instance.DefaultPlayerCharacter;
            GunslingerClassBlueprintSet gunslinger = BlueprintBootstrap.GunslingerClass;
            BlueprintItemWeapon pistol = BlueprintBootstrap.ProductionFirearms.Pistol.Item;
            BlueprintItemWeapon musket = BlueprintBootstrap.ProductionFirearms.Musket.Item;
            object player = ReadExactMember(Kingmaker.Game.Instance, "Player");
            object state = ReadExactMember(Kingmaker.Game.Instance, "State");
            object party = ReadExactMember(player, "Party");
            object allUnits = ReadExactMember(state, "AllUnits");
            object[] partyBefore = SnapshotReferences(party);
            object[] unitsBefore = SnapshotReferences(allUnits);
            Kingmaker.EntitySystem.Entities.UnitEntityData attacker = null;
            Kingmaker.EntitySystem.Entities.UnitEntityData target = null;
            ItemEntityWeapon weapon = null;
            int initial = -1, afterOne = -1, beforeTwo = -1, afterTwo = -1,
                afterRejected = -1;
            PistolWhipResult one = null, two = null, rejected = null;
            FirearmState oneBefore = null, oneAfter = null, twoBefore = null,
                twoAfter = null;
            bool cleaned = false;
            string stage = "construct-disposables";
            PistolWhipRuntimeDiagnostics.Reset();
            try
            {
                attacker = new Kingmaker.UI.LevelUp.ChargenUnit(source).Unit;
                target = new Kingmaker.UI.LevelUp.ChargenUnit(source).Unit;
                attacker.Descriptor.Stats.Wisdom.BaseValue = 14;
                attacker.Descriptor.AddFact(gunslinger.Grit.Feature);
                initial = attacker.Descriptor.Resources.GetResourceAmount(
                    gunslinger.Grit.Resource);

                stage = "one-handed-hit";
                weapon = new ItemEntityWeapon(pistol);
                attacker.Body.PrimaryHand.InsertItem(weapon);
                FirearmRuntimeState.Service.Set(weapon, new FirearmState(
                    FirearmState.CurrentSchemaVersion,
                    1,
                    FirearmStateTokenCatalog.DiagnosticLeadBall,
                    FirearmCondition.Normal));
                oneBefore = FirearmRuntimeState.Service.GetOrCreate(weapon)
                    .Repository.State;
                one = PistolWhipRuntime.ExecuteForRuntimeTest(attacker, target,
                    gunslinger.PistolWhip.OneHandedItem,
                    gunslinger.PistolWhip.TwoHandedItem, true);
                afterOne = attacker.Descriptor.Resources.GetResourceAmount(
                    gunslinger.Grit.Resource);
                oneAfter = FirearmRuntimeState.Service.GetOrCreate(weapon)
                    .Repository.State;

                stage = "two-handed-hit";
                attacker.Descriptor.Resources.Restore(gunslinger.Grit.Resource, 1);
                FirearmRuntimeState.Service.Forget(weapon);
                attacker.Body.PrimaryHand.RemoveItem(false);
                weapon = new ItemEntityWeapon(musket);
                attacker.Body.PrimaryHand.InsertItem(weapon);
                FirearmRuntimeState.Service.Set(weapon,
                    FirearmStateMachine.ApplyMisfireDamage(FirearmState.CreateEmpty()));
                twoBefore = FirearmRuntimeState.Service.GetOrCreate(weapon)
                    .Repository.State;
                beforeTwo = attacker.Descriptor.Resources.GetResourceAmount(
                    gunslinger.Grit.Resource);
                two = PistolWhipRuntime.ExecuteForRuntimeTest(attacker, target,
                    gunslinger.PistolWhip.OneHandedItem,
                    gunslinger.PistolWhip.TwoHandedItem, true);
                afterTwo = attacker.Descriptor.Resources.GetResourceAmount(
                    gunslinger.Grit.Resource);
                twoAfter = FirearmRuntimeState.Service.GetOrCreate(weapon)
                    .Repository.State;

                stage = "zero-grit-rejection";
                rejected = PistolWhipRuntime.ExecuteForRuntimeTest(attacker, target,
                    gunslinger.PistolWhip.OneHandedItem,
                    gunslinger.PistolWhip.TwoHandedItem, true);
                afterRejected = attacker.Descriptor.Resources.GetResourceAmount(
                    gunslinger.Grit.Resource);
            }
            catch (Exception exception)
            {
                throw new InvalidOperationException(
                    "Disposable Pistol-Whip failed at stage " + stage + ".",
                    exception);
            }
            finally
            {
                if (weapon != null)
                {
                    FirearmRuntimeState.Service.Forget(weapon);
                    if (attacker != null && attacker.Body.PrimaryHand.MaybeItem != null)
                        attacker.Body.PrimaryHand.RemoveItem(false);
                }
                if (attacker != null)
                {
                    if (attacker.Descriptor.HasFact(gunslinger.Grit.Feature))
                        attacker.Descriptor.RemoveFact(gunslinger.Grit.Feature);
                    attacker.Dispose();
                }
                if (target != null) target.Dispose();
                cleaned = SameReferences(partyBefore, SnapshotReferences(party)) &&
                    SameReferences(unitsBefore, SnapshotReferences(allUnits)) &&
                    (attacker == null || !ContainsReference(allUnits, attacker)) &&
                    (target == null || !ContainsReference(allUnits, target));
            }
            string observed = "initial=" + initial + ";afterOne=" + afterOne +
                ";oneHit=" + (one != null && one.Hit) + ";oneTrip=" +
                (one != null && one.Trip != null) + ";oneDie=" +
                (one == null ? -1 : one.Decision.DamageDieSides) +
                ";oneEnhancement=" + (one == null ? -1 : one.Enhancement) +
                ";beforeTwo=" + beforeTwo + ";afterTwo=" + afterTwo +
                ";twoHit=" + (two != null && two.Hit) + ";twoTrip=" +
                (two != null && two.Trip != null) + ";twoDie=" +
                (two == null ? -1 : two.Decision.DamageDieSides) +
                ";twoEnhancement=" + (two == null ? -1 : two.Enhancement) +
                ";afterRejected=" + afterRejected + ";rejected=" +
                (rejected == null ? "null" : rejected.Decision.Status.ToString()) +
                ";applied=" + PistolWhipRuntimeDiagnostics.Applied +
                ";rejectedCount=" + PistolWhipRuntimeDiagnostics.Rejected +
                ";hits=" + PistolWhipRuntimeDiagnostics.Hits + ";faults=" +
                PistolWhipRuntimeDiagnostics.Faults;
            var assertions = new List<RuntimeTestAssertion>
            {
                Assertion("pistol-whip-one-handed", "grit 1 -> 0; 1d6; hit; Trip",
                    observed, initial > 0 && afterOne == initial - 1 && one != null &&
                    one.Hit && one.Trip != null && one.Decision.DamageDieSides == 6 &&
                    ReferenceEquals(one.Attack.Weapon.Blueprint,
                        gunslinger.PistolWhip.OneHandedItem),
                    "exact equipped pistol and native melee rule event"),
                Assertion("pistol-whip-two-handed", "grit 1 -> 0; 1d10; hit; Trip",
                    observed, beforeTwo == 1 && afterTwo == 0 && two != null &&
                    two.Hit && two.Trip != null && two.Decision.DamageDieSides == 10 &&
                    ReferenceEquals(two.Attack.Weapon.Blueprint,
                        gunslinger.PistolWhip.TwoHandedItem),
                    "exact equipped musket and native melee rule event"),
                Assertion("pistol-whip-enhancement", "copied into both native fields",
                    observed, one != null && two != null &&
                    one.Attack.WeaponStats.Enhancement == one.Enhancement &&
                    one.Attack.WeaponStats.EnhancementTotal == one.Enhancement &&
                    two.Attack.WeaponStats.Enhancement == two.Enhancement &&
                    two.Attack.WeaponStats.EnhancementTotal == two.Enhancement,
                    "RuleCalculateWeaponStats enhancement surface"),
                Assertion("pistol-whip-state-isolation", "firearm state unchanged",
                    observed, oneBefore == oneAfter && twoBefore == twoAfter,
                    "exact item-owned state snapshots"),
                Assertion("pistol-whip-zero-grit", "rejected without spend or attack",
                    observed, afterRejected == 0 && rejected != null &&
                    rejected.Decision.Status == PistolWhipStatus.InsufficientGrit &&
                    rejected.Attack == null, "production fail-closed policy"),
                Assertion("pistol-whip-diagnostics",
                    "applied=2;rejected=1;hits=2;faults=0", observed,
                    PistolWhipRuntimeDiagnostics.Applied == 2 &&
                    PistolWhipRuntimeDiagnostics.Rejected == 1 &&
                    PistolWhipRuntimeDiagnostics.Hits == 2 &&
                    PistolWhipRuntimeDiagnostics.Faults == 0,
                    "production Pistol-Whip diagnostics"),
                Assertion("external-isolation", "unchanged party and global-unit snapshots",
                    "cleaned=" + cleaned, cleaned,
                    "firearm removed and detached units disposed"),
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
