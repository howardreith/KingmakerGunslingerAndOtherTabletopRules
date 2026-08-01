using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using Harmony12;
using KingmakerGunslinger.Bootstrap;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace KingmakerGunslinger.RuntimeTesting
{
    /// <summary>
    /// Guarded, request-scoped working-save execution and supervised observation.
    /// Autonomous mode invokes only its qualified path; supervised modes consume
    /// the exact catalog and passively correlate the human's normal save action.
    /// </summary>
    internal sealed class WorkingSaveSmokeScenario
    {
        internal const string ExpectedName = "KMG_AUTOMATION_WORKING";
        internal const string ForbiddenName = "KMG_AUTOMATION_BASELINE";
        internal const string ExpectedFile = "Manual_299_KMG_AUTOMATION_WORKING.zks";
        internal const string BaselineFile = "Manual_298_KMG_AUTOMATION_BASELINE.zks";
        internal const string ExpectedGameName = "Hedwirg";
        internal const string ExpectedGameId = "dce769e0-229c-4bfd-b8ea-e2d572bf8472";
        internal const string ExpectedArea = "JamandisMansion";
        internal const int ExpectedPartyCount = 3;
        internal const string ExpectedMainCharacterType =
            "Kingmaker.EntitySystem.Entities.UnitReference";
        internal const string ExpectedAreaType =
            "Kingmaker.Blueprints.Area.BlueprintArea";
        internal const string ExpectedSceneType =
            "Kingmaker.EntitySystem.AreaPersistentState";

        private const string OwnerType = "Kingmaker.UI.MainMenuUI.MainMenuButtons";
        private const string CatalogType = "Kingmaker.UI.SaveLoadWindow.ListOfSaves";
        private const string DescriptorType =
            "Kingmaker.EntitySystem.Persistence.SaveInfo";
        private const string MainMenuType = "Kingmaker.MainMenu";
        private const string SaveSlotType =
            "Kingmaker.UI.SaveLoadWindow.SaveSlot";
        private const string SaveLoadWindowType =
            "Kingmaker.UI.SaveLoadWindow.SaveLoadWindow";
        private const string ButtonPath =
            "!LIGHT_SETUP/SceneUICanvas/SideBar/Buttons/LoadGame";
        private static readonly string[] ExactComponents =
        {
            "UnityEngine.CanvasRenderer", "UnityEngine.RectTransform",
            "UnityEngine.UI.Button"
        };
        private const string ExactLabel =
            "<font=\"Saber_Dist32\"><color=#983F1D><size=140%>L</size></color></font>oad game";
        private static readonly string[] ExactLabelIdentities =
        {
            "TMPro.TextMeshProUGUI.m_text=" + ExactLabel,
            "TMPro.TextMeshProUGUI.old_text=" + ExactLabel,
            "TMPro.TextMeshProUGUI.text=" + ExactLabel
        };
        private static readonly string[] WritePrefixes =
        {
            "Save", "AutoSave", "QuickSave", "DeleteSave", "RemoveSave",
            "RenameSave", "MigrateSave", "Overwrite"
        };
        private static WorkingSaveSmokeScenario _active;

        private readonly ModContext _context;
        private readonly Stopwatch _elapsed;
        private readonly string _runId;
        private readonly int _gameThreadId;
        private readonly Action<SaveLoadObservationEvent> _sink;
        private readonly List<MethodBase> _patched = new List<MethodBase>();
        private readonly List<SaveLoadObservationEvent> _events =
            new List<SaveLoadObservationEvent>();
        private MethodInfo _handler;
        private MethodInfo _initialize;
        private MethodInfo _loadEntry;
        private MethodInfo _slotAction;
        private MethodInfo _windowHandler;
        private string _stage = "runtime-readiness";
        private long _stageStarted;
        private Button _button;
        private LoadGameButtonCandidateEvidence _buttonEvidence;
        private object _mainMenuButtons;
        private object _loadEntryReceiver;
        private object _catalogObject;
        private object _catalogReceiver;
        private object _workingDescriptor;
        private SaveCatalogDescriptorEvidence _workingEvidence;
        private int _buttonCandidates;
        private int _buttonInvocations;
        private int _handlerInvocations;
        private int _catalogInvocations;
        private int _descriptorCount;
        private int _workingCount;
        private int _baselineCount;
        private int _loadEntryInvocations;
        private int _stableSamples;
        private string _lastFingerprint;
        private bool _catalogComplete;
        private bool _descriptorCorrelated;
        private bool _completionCallback;
        private bool _callbackRegistered;
        private bool _writeObserved;
        private bool _wrongThread;
        private bool _removed;
        private bool _sealed;
        private bool _buttonResolutionAttempted;
        private readonly bool _observeEntryAction;
        private readonly bool _observeSelectionLoadAction;
        private readonly bool _observeReceiverBoundAction;
        private readonly bool _autonomousReceiverBoundAction;
        private Button _entryAction;
        private UnityEvent _entryUnityEvent;
        private Component _entryOwner;
        private LoadGameButtonCandidateEvidence _entryEvidence;
        private int _entryCandidates;
        private int _entryActionCandidates;
        private int _humanActionInvocations;
        private int _listenerInvocations;
        private object _listenerTarget;
        private MethodInfo _listenerMethod;
        private object _observedLoadReceiver;
        private readonly List<object> _scopedReceivers = new List<object>();
        private readonly List<MethodInfo> _scopedActionMethods = new List<MethodInfo>();
        private readonly List<string> _scopedActionCandidates = new List<string>();
        private readonly List<string> _observedScopedInvocations = new List<string>();
        private readonly List<string> _loadCallerChain = new List<string>();
        private readonly List<string> _selectedSaveStorage = new List<string>();
        private readonly List<string> _candidateRejections = new List<string>();
        private string _immediateLoadCaller = "";
        private string _immediateLoadCallerType = "";
        private string _immediateLoadCallerReceiverIdentity = "";
        private int _compatibleCallerReceiverCount;
        private string _observerArmingSubstage = "";
        private string _ownerObjectIdentity = "";
        private string _listObjectIdentity = "";
        private bool _selectedWorkingStateObserved;
        private int _finalLoadActionCount;
        private bool _baselineLoadObserved;
        private bool _otherLoadObserved;
        private Exception _exception;
        private string _lastCompletedStage = "runtime-readiness";
        private object _receiverBoundSlot;
        private object _receiverBoundWindow;
        private string _descriptorMemberIdentity = "";
        private string _entryHierarchyPath = "";
        private int _slotActionInvocations;
        private int _windowHandlerInvocations;
        private bool _slotReceiverCorrelated;
        private bool _windowReceiverCorrelated;
        private bool _windowArgumentCorrelated;
        private int _slotActionSequence;
        private int _windowHandlerSequence;
        private int _loadEntrySequence;
        private int _completionSequence;
        private int _fingerprintSequence;
        private bool _receiverScopeResolutionAttempted;

        internal WorkingSaveSmokeScenario(ModContext context, Stopwatch elapsed,
            string runId, Action<SaveLoadObservationEvent> sink,
            bool observeEntryAction = false,
            bool observeSelectionLoadAction = false,
            bool observeReceiverBoundAction = false)
        {
            _context = context;
            _elapsed = elapsed;
            _runId = runId;
            _sink = sink;
            _gameThreadId = Thread.CurrentThread.ManagedThreadId;
            _observeEntryAction = observeEntryAction;
            _observeSelectionLoadAction = observeSelectionLoadAction;
            _observeReceiverBoundAction = observeReceiverBoundAction;
            _autonomousReceiverBoundAction = !observeEntryAction &&
                !observeSelectionLoadAction && !observeReceiverBoundAction;
            _stageStarted = elapsed.ElapsedMilliseconds;
        }

        internal string Stage { get { return _stage; } }
        internal long StageElapsedMilliseconds
        {
            get { return _elapsed.ElapsedMilliseconds - _stageStarted; }
        }
        internal bool Complete
        {
            get
            {
                return _completionCallback && _stableSamples >= 2 &&
                    _descriptorCorrelated && !_writeObserved && !_wrongThread &&
                    (_autonomousReceiverBoundAction
                         ? (_entryCandidates == 1 &&
                            _slotActionInvocations == 1 &&
                            _windowHandlerInvocations == 1 &&
                            _loadEntryInvocations == 1 &&
                            _slotReceiverCorrelated &&
                            _windowReceiverCorrelated &&
                            _windowArgumentCorrelated &&
                            StrictReceiverBoundOrder())
                         : !_observeEntryAction ||
                     (_observeReceiverBoundAction
                         ? (_entryCandidates == 1 &&
                            _slotActionInvocations == 1 &&
                            _windowHandlerInvocations == 1 &&
                            _loadEntryInvocations == 1 &&
                            _slotReceiverCorrelated &&
                            _windowReceiverCorrelated &&
                            _windowArgumentCorrelated &&
                            StrictReceiverBoundOrder())
                         : _observeSelectionLoadAction
                         ? (_entryCandidates == 1 &&
                            _selectedWorkingStateObserved &&
                            _finalLoadActionCount == 1 &&
                            _loadEntryInvocations == 1 &&
                            _loadCallerChain.Count != 0)
                         :
                     (_entryCandidates == 1 && _entryActionCandidates == 1 &&
                      _humanActionInvocations == 1 &&
                      _listenerInvocations == 1 &&
                      _loadEntryInvocations == 1)));
            }
        }
        internal bool ObservationComplete
        {
            get
            {
                return _completionCallback && _stableSamples >= 2 &&
                    _descriptorCorrelated && !_writeObserved && !_wrongThread;
            }
        }
        internal bool WriteObserved { get { return _writeObserved; } }
        internal bool SelectionLoadObservation { get { return _observeSelectionLoadAction; } }
        internal bool ReceiverBoundObservation { get { return _observeReceiverBoundAction; } }
        internal bool AutonomousReceiverBoundAction
        {
            get { return _autonomousReceiverBoundAction; }
        }
        internal Exception ScenarioException { get { return _exception; } }
        internal string LastCompletedStage { get { return _lastCompletedStage; } }
        internal string ObserverArmingSubstage { get { return _observerArmingSubstage; } }
        internal int WorkingCount { get { return _workingCount; } }
        internal int BaselineCount { get { return _baselineCount; } }
        internal int ButtonCandidateCount { get { return _buttonCandidates; } }
        internal int EntryCandidateCount { get { return _entryCandidates; } }
        internal int EntryActionCandidateCount { get { return _entryActionCandidates; } }
        internal bool BaselineLoadObserved { get { return _baselineLoadObserved; } }
        internal bool OtherLoadObserved { get { return _otherLoadObserved; } }
        internal bool CatalogComplete { get { return _catalogComplete; } }
        internal bool MainMenuReady
        {
            get
            {
                return _mainMenuButtons != null && _button != null &&
                    _buttonCandidates == 1 && _stage == "action-invocation";
            }
        }
        internal bool WorkingEntryReady
        {
            get
            {
                return _observeEntryAction && _stage == "working-entry-click" &&
                    _workingCount == 1 && _baselineCount == 1 &&
                    _catalogComplete &&
                    (_observeReceiverBoundAction
                        ? (_entryCandidates == 1 &&
                           _entryActionCandidates == 1 &&
                           _receiverBoundSlot != null &&
                           _receiverBoundWindow != null)
                        : _entryCandidates <= 1) &&
                    (_observeReceiverBoundAction ||
                     (_observeSelectionLoadAction || _entryActionCandidates <= 1)) &&
                    _loadEntry != null;
            }
        }
        internal string ExactSlotIdentity { get { return ObjectIdentity(_receiverBoundSlot); } }
        internal string ExactWindowIdentity { get { return ObjectIdentity(_receiverBoundWindow); } }
        internal bool ReceiverBoundScopeResolutionFailed
        {
            get
            {
                return (_observeReceiverBoundAction ||
                    _autonomousReceiverBoundAction) &&
                    (_stage == "working-entry-click" ||
                     _stage == "receiver-bound-action-invocation") &&
                    (_entryCandidates != 1 || _entryActionCandidates != 1 ||
                     _receiverBoundSlot == null || _receiverBoundWindow == null);
            }
        }
        internal List<string> ReceiverBoundHookIdentifiers
        {
            get
            {
                return new[] { _slotAction, _windowHandler, _loadEntry }
                    .Where(x => x != null).Select(FormatSignature)
                    .OrderBy(x => x).ToList();
            }
        }
        internal List<string> HookIdentifiers
        {
            get { return _patched.Select(FormatSignature).OrderBy(x => x).ToList(); }
        }

        internal void Install()
        {
            if (_active != null)
                throw new InvalidOperationException("A working-save smoke is active.");
            try
            {
                Assembly assembly = typeof(Kingmaker.Game).Assembly;
                Type owner = assembly.GetType(OwnerType, true);
                _handler = owner.GetMethods(AllInstance).Single(method =>
                    method.Name == "OnButtonLoadGame" &&
                    method.GetParameters().Length == 0 &&
                    method.ReturnType == typeof(void));
                Type list = assembly.GetType(CatalogType, true);
                _initialize = list.GetMethods(AllInstance).Single(method =>
                    method.Name == "Initialize" &&
                    method.GetParameters().Length == 2 &&
                    IsSaveInfoList(method.GetParameters()[0].ParameterType) &&
                    method.GetParameters()[1].ParameterType == typeof(bool) &&
                    method.ReturnType == typeof(void));
                Type menu = assembly.GetType(MainMenuType, true);
                _loadEntry = menu.GetMethods(AllInstance).Single(method =>
                    method.Name == "LoadGame" &&
                    method.GetParameters().Length == 1 &&
                    method.GetParameters()[0].ParameterType.FullName == DescriptorType &&
                    method.ReturnType == typeof(void));
                if (_observeReceiverBoundAction || _autonomousReceiverBoundAction)
                {
                    Type slot = assembly.GetType(SaveSlotType, true);
                    Type window = assembly.GetType(SaveLoadWindowType, true);
                    Type descriptor = assembly.GetType(DescriptorType, true);
                    _slotAction = ExactPatchableMethod(slot,
                        "OnButtonSaveLoad", Type.EmptyTypes, typeof(void));
                    _windowHandler = ExactPatchableMethod(window,
                        "HandleHardcodeMainMenuSaveLoad",
                        new[] { descriptor }, typeof(void));
                    RequirePatchableContract(_loadEntry, menu, "LoadGame",
                        new[] { descriptor }, typeof(void));
                }
                MethodInfo prefix = typeof(WorkingSaveSmokeScenario).GetMethod(
                    "Prefix", BindingFlags.Static | BindingFlags.NonPublic);
                MethodInfo postfix = typeof(WorkingSaveSmokeScenario).GetMethod(
                    "Postfix", BindingFlags.Static | BindingFlags.NonPublic);
                Patch(_handler, prefix, null);
                Patch(_initialize, prefix, postfix);
                Patch(_loadEntry, prefix, null);
                if (_observeReceiverBoundAction || _autonomousReceiverBoundAction)
                {
                    Patch(_slotAction, prefix, null);
                    Patch(_windowHandler, prefix, null);
                }
                if (_observeReceiverBoundAction || _autonomousReceiverBoundAction)
                    InstallExactSaveWriteSentinels(assembly, prefix);
                else
                    InstallLegacySaveWriteSentinels(assembly, prefix);
                _active = this;
                Transition("main-menu-readiness",
                    "contracts installed; no action invoked");
            }
            catch
            {
                RemoveHooks();
                throw;
            }
        }

        internal void Poll()
        {
            RequireGameThread();
            if (_sealed) return;
            if (!_callbackRegistered) RegisterCompletionCallback();
            if (_stage == "main-menu-readiness")
            {
                ResolveMainMenu();
                if (_mainMenuButtons != null)
                {
                    Transition("load-game-action-resolution",
                        "exact active MainMenuButtons lifecycle receiver resolved; overlay was not treated as readiness");
                    return;
                }
            }
            if (_stage == "load-game-action-resolution")
            {
                if (_buttonResolutionAttempted)
                    throw new InvalidOperationException(
                        "The exact Load Game action could not be proven at the main-menu lifecycle point.");
                _buttonResolutionAttempted = true;
                ResolveButton();
                if (_buttonCandidates == 1 && _button != null)
                {
                    Transition("action-invocation", "one exact button/action resolved");
                    return;
                }
            }
            if (_stage == "action-invocation")
            {
                Add("load-game-action-invoke-start", null, null, "");
                _buttonInvocations++;
                Add("button-onclick-invoke", null, null,
                    "route=UnityEngine.UI.Button.onClick.Invoke;count=1");
                _button.onClick.Invoke();
                Add("load-game-action-invoked", null, null, "count=1");
                if (_buttonInvocations != 1 || _handlerInvocations != 1)
                    throw new InvalidOperationException(
                        "Normal Load Game action did not invoke exactly once.");
                Transition("catalog-initialization",
                    "normal Unity event returned after one handler invocation");
                return;
            }
            if (_stage == "catalog-initialization" && _catalogInvocations == 1)
            {
                Add("catalog-complete", _initialize, null,
                    "descriptorCount=" + ReadCount(_catalogObject));
                Transition("descriptor-resolution", "exact catalog argument captured");
                return;
            }
            if (_stage == "descriptor-resolution")
            {
                Add("descriptor-resolution-start", null, null, "");
                ResolveDescriptors();
                if (_workingCount > 1) return;
                if (_workingCount == 1 && _baselineCount == 1 && _catalogComplete)
                {
                    Add("working-descriptor-resolved", null, null,
                        "immutable scalar descriptor evidence captured");
                    Add("baseline-excluded", null, null,
                        "working and baseline object references are distinct");
                    Transition((_observeEntryAction || _autonomousReceiverBoundAction)
                            ? "working-entry-readiness" : "load-entry-invocation",
                        "unique working descriptor and distinct baseline proven");
                    return;
                }
            }
            if (_stage == "working-entry-readiness")
            {
                if (_observeReceiverBoundAction || _autonomousReceiverBoundAction)
                    ResolveWorkingReceiverBoundScope();
                else if (_observeSelectionLoadAction)
                    ResolveWorkingSelectionLoadActions();
                else ResolveWorkingEntryAction();
                if (((_observeReceiverBoundAction || _autonomousReceiverBoundAction) &&
                     _receiverScopeResolutionAttempted) ||
                    (_entryCandidates <= 1 &&
                    (_observeSelectionLoadAction || _observeReceiverBoundAction ||
                     _autonomousReceiverBoundAction ||
                     _entryActionCandidates <= 1)))
                    Transition(_autonomousReceiverBoundAction
                            ? "receiver-bound-action-invocation"
                            : "working-entry-click",
                        _autonomousReceiverBoundAction
                            ? "exact receiver-bound action contract proven for autonomous invocation"
                            : "pre-click descriptor identity proven and exact LoadGame observation armed; entry action correlation may complete after the human click");
                return;
            }
            if (_stage == "receiver-bound-action-invocation")
            {
                RequireGameThread();
                Add("autonomous-receiver-bound-action-invoke-start", _slotAction,
                    null, "receiver=" + ObjectIdentity(_receiverBoundSlot) +
                    ";descriptor=" + ObjectIdentity(_workingDescriptor));
                _slotAction.Invoke(_receiverBoundSlot, null);
                Add("autonomous-receiver-bound-action-invoke-return", _slotAction,
                    null, "exact normal receiver-bound action invoked once");
                if (_slotActionInvocations != 1 || !_slotReceiverCorrelated)
                    throw new InvalidOperationException(
                        "Autonomous receiver-bound action did not enter exactly once on the exact working slot.");
                return;
            }
            if (_stage == "load-entry-invocation")
            {
                _loadEntryReceiver = ResolveLoadEntryReceiver(
                    _mainMenuButtons as Component,
                    typeof(Kingmaker.Game).Assembly.GetType(MainMenuType, true));
                if (_loadEntryReceiver == null)
                    throw new MissingMemberException(
                        "The exact Kingmaker.MainMenu load-entry receiver could not be resolved after catalog initialization.");
                _descriptorCorrelated = ContainsReference(
                    _catalogObject, _workingDescriptor);
                RemoveUiHooks();
                Add("ui-hooks-removed", null, null,
                    "button and catalog hooks removed before load entry");
                _button = null;
                _mainMenuButtons = null;
                _catalogObject = null;
                _loadEntryInvocations++;
                Add("load-entry-start", _loadEntry,
                    new[] { _workingDescriptor },
                    "correlation=object-reference;count=1");
                _loadEntry.Invoke(_loadEntryReceiver, new[] { _workingDescriptor });
                if (_loadEntryInvocations != 1 || !_descriptorCorrelated)
                    throw new InvalidOperationException(
                        "Load entry correlation or invocation count failed.");
                Add("load-entry-complete", _loadEntry, null,
                    "exact MainMenu.LoadGame returned");
                Transition("load-completion", "exact MainMenu.LoadGame invoked once");
                return;
            }
            if (_stage == "working-entry-click" && _loadEntryInvocations == 1)
            {
                Transition("load-completion",
                    "human invoked exact working-entry action once");
                return;
            }
            if (_stage == "load-completion" && _completionCallback)
            {
                Transition("post-load-fingerprint", "after-load callback observed");
                Add("fingerprint-start", null, null,
                    "post-load evidence reads persistent game state only");
                return;
            }
            if (_stage == "post-load-fingerprint") PollFingerprint();
        }

        internal WorkingSaveSmokeEvidence Stop()
        {
            _sealed = true;
            bool hooksInstalled = _patched.Count != 0;
            RemoveHooks();
            Add("scenario-hooks-removed", null, null,
                "all scenario-specific hooks removed");
            return new WorkingSaveSmokeEvidence
            {
                Stage = _stage,
                Button = _buttonEvidence,
                ButtonCandidateCount = _buttonCandidates,
                ButtonEventInvocationCount = _buttonInvocations,
                HandlerInvocationCount = _handlerInvocations,
                CatalogInitializeCount = _catalogInvocations,
                CatalogDescriptorCount = _descriptorCount,
                CatalogComplete = _catalogComplete,
                WorkingMatchCount = _workingCount,
                BaselineMatchCount = _baselineCount,
                ResolvedDescriptor = _workingEvidence,
                DescriptorReferenceCorrelated = _descriptorCorrelated,
                LoadEntryInvocationCount = _loadEntryInvocations,
                CompletionCallbackObserved = _completionCallback,
                StableFingerprint = _stableSamples >= 2 ? _lastFingerprint : "",
                SaveWritingApiObserved = _writeObserved,
                AllCallbacksOnGameThread = !_wrongThread,
                HooksRemoved = _removed,
                HooksInstalled = hooksInstalled,
                UiActionOccurred = _buttonInvocations != 0,
                DescriptorResolved = _workingDescriptor != null,
                LoadingBegan = _loadEntryInvocations != 0,
                LoadingCompleted = _completionCallback,
                Events = new List<SaveLoadObservationEvent>(_events),
                EntryCandidateCount = _entryCandidates,
                EntryActionCandidateCount = _entryActionCandidates,
                EntryAction = _entryEvidence,
                EntryObjectIdentity = ObjectIdentity(_entryOwner),
                ActionObjectIdentity = ObjectIdentity(_entryUnityEvent),
                HumanActionInvocationCount = _humanActionInvocations,
                ListenerInvocationCount = _listenerInvocations,
                ListenerTargetIdentity = ObjectIdentity(_listenerTarget),
                ListenerMethod = FormatSignature(_listenerMethod),
                LoadEntryReceiverIdentity = ObjectIdentity(_observedLoadReceiver),
                ProbeInvokedEntryAction = _autonomousReceiverBoundAction &&
                    _slotActionInvocations != 0
                , OwnerObjectIdentity = _ownerObjectIdentity
                , ListObjectIdentity = _listObjectIdentity
                , ScopedActionCandidates = new List<string>(_scopedActionCandidates)
                , ObservedScopedInvocations = new List<string>(_observedScopedInvocations)
                , SelectedSaveStorage = new List<string>(_selectedSaveStorage)
                , SelectedWorkingStateObserved = _selectedWorkingStateObserved
                , FinalLoadActionCount = _finalLoadActionCount
                , ImmediateLoadCaller = _immediateLoadCaller
                , ImmediateLoadCallerType = _immediateLoadCallerType
                , ImmediateLoadCallerReceiverIdentity = _immediateLoadCallerReceiverIdentity
                , CompatibleCallerReceiverCount = _compatibleCallerReceiverCount
                , LoadCallerChain = new List<string>(_loadCallerChain)
                , CandidateRejections = new List<string>(_candidateRejections)
                , DescriptorMemberIdentity = _descriptorMemberIdentity
                , EntryHierarchyPath = _entryHierarchyPath
                , GameThreadManagedId = _gameThreadId
                , ReceiverBoundHookIdentifiers = ReceiverBoundHookIdentifiers
                , SlotActionInvocationCount = _slotActionInvocations
                , SlotReceiverReferenceCorrelated = _slotReceiverCorrelated
                , WindowHandlerInvocationCount = _windowHandlerInvocations
                , WindowReceiverReferenceCorrelated = _windowReceiverCorrelated
                , WindowArgumentReferenceCorrelated = _windowArgumentCorrelated
                , SlotActionSequence = _slotActionSequence
                , WindowHandlerSequence = _windowHandlerSequence
                , LoadEntrySequence = _loadEntrySequence
                , CompletionSequence = _completionSequence
                , FingerprintSequence = _fingerprintSequence
            };
        }

        private void ResolveWorkingReceiverBoundScope()
        {
            if (_receiverScopeResolutionAttempted) return;
            _receiverScopeResolutionAttempted = true;
            _observerArmingSubstage = "working-receiver-bound-slot-resolution";
            Type slotType = typeof(Kingmaker.Game).Assembly.GetType(SaveSlotType, true);
            var slots = Resources.FindObjectsOfTypeAll(slotType)
                .OfType<Component>()
                .Where(component => component != null &&
                    component.GetType() == slotType &&
                    component.gameObject.activeInHierarchy &&
                    HasExactDescriptorMember(component, _workingDescriptor))
                .ToList();
            slots = UniqueComponents(slots);
            _entryCandidates = slots.Count;
            if (slots.Count != 1) return;
            _entryOwner = slots[0];
            _receiverBoundSlot = slots[0];
            string memberIdentity;
            if (!TryFindExactDescriptorMember(
                _receiverBoundSlot, _workingDescriptor, out memberIdentity)) return;
            _descriptorMemberIdentity = memberIdentity;
            _entryHierarchyPath = HierarchyPath(_entryOwner.transform);

            _observerArmingSubstage = "working-receiver-bound-window-resolution";
            Type windowType = typeof(Kingmaker.Game).Assembly.GetType(
                SaveLoadWindowType, true);
            var windows = new List<Component>();
            for (Transform current = _entryOwner.transform;
                current != null; current = current.parent)
                windows.AddRange(current.gameObject.GetComponents<Component>()
                    .Where(component => component != null &&
                        component.GetType() == windowType));
            windows = UniqueComponents(windows);
            if (windows.Count != 1)
            {
                _entryActionCandidates = windows.Count;
                return;
            }
            _receiverBoundWindow = windows[0];
            _ownerObjectIdentity = ObjectIdentity(_receiverBoundWindow);
            var lists = ((Component)_receiverBoundWindow).gameObject
                .GetComponentsInChildren<Component>(true)
                .Where(component => component != null &&
                    component.GetType().FullName == CatalogType &&
                    ReferenceEquals(component, _catalogReceiver)).ToList();
            lists = UniqueComponents(lists);
            if (lists.Count != 1)
            {
                _entryActionCandidates = lists.Count;
                return;
            }
            _listObjectIdentity = ObjectIdentity(lists[0]);
            _entryActionCandidates = 1;
            _observerArmingSubstage = "working-receiver-bound-readiness-writing";
            Add("working-receiver-bound-action-ready", null, null,
                "slot=" + ObjectIdentity(_receiverBoundSlot) +
                ";window=" + ObjectIdentity(_receiverBoundWindow) +
                ";list=" + _listObjectIdentity +
                ";descriptorMember=" + _descriptorMemberIdentity +
                ";path=" + _entryHierarchyPath);
        }

        private static bool TryFindExactDescriptorMember(
            object owner, object expected, out string identity)
        {
            identity = "";
            for (Type type = owner == null ? null : owner.GetType();
                type != null; type = type.BaseType)
            {
                foreach (FieldInfo field in type.GetFields(BindingFlags.Instance |
                    BindingFlags.Public | BindingFlags.NonPublic |
                    BindingFlags.DeclaredOnly))
                {
                    if (field.FieldType.IsValueType || field.FieldType == typeof(string))
                        continue;
                    object value;
                    try { value = field.GetValue(owner); } catch { continue; }
                    if (!ReferenceEquals(value, expected)) continue;
                    identity = field.DeclaringType.FullName + "." + field.Name;
                    return true;
                }
                foreach (PropertyInfo property in type.GetProperties(
                    BindingFlags.Instance | BindingFlags.Public |
                    BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
                {
                    if (!property.CanRead || property.GetIndexParameters().Length != 0 ||
                        property.PropertyType.IsValueType ||
                        property.PropertyType == typeof(string)) continue;
                    object value;
                    try { value = property.GetValue(owner, null); } catch { continue; }
                    if (!ReferenceEquals(value, expected)) continue;
                    identity = property.DeclaringType.FullName + "." + property.Name;
                    return true;
                }
            }
            return false;
        }

        private static bool HasExactDescriptorMember(object owner, object expected)
        {
            string ignored;
            return TryFindExactDescriptorMember(owner, expected, out ignored);
        }

        private bool StrictReceiverBoundOrder()
        {
            return _slotActionSequence > 0 &&
                _slotActionSequence < _windowHandlerSequence &&
                _windowHandlerSequence < _loadEntrySequence &&
                _loadEntrySequence < _completionSequence &&
                _completionSequence < _fingerprintSequence;
        }

        private static MethodInfo ExactPatchableMethod(Type declaringType,
            string name, Type[] parameterTypes, Type returnType)
        {
            MethodInfo method = declaringType.GetMethod(name,
                BindingFlags.Instance | BindingFlags.Public |
                BindingFlags.NonPublic | BindingFlags.DeclaredOnly,
                null, parameterTypes, null);
            RequirePatchableContract(method, declaringType, name,
                parameterTypes, returnType);
            return method;
        }

        private void InstallExactSaveWriteSentinels(
            Assembly assembly, MethodInfo prefix)
        {
            Type manager = assembly.GetType(
                "Kingmaker.EntitySystem.Persistence.SaveManager", true);
            Type descriptor = assembly.GetType(DescriptorType, true);
            Type areaState = assembly.GetType(
                "Kingmaker.EntitySystem.AreaPersistentState", true);
            Patch(ExactPatchableMethod(manager, "DeleteSave",
                new[] { descriptor }, typeof(void)), prefix, null);
            Patch(ExactPatchableMethod(manager, "DeleteSave",
                new[] { typeof(string) }, typeof(void)), prefix, null);
            Patch(ExactPatchableMethod(manager, "RemoveSaveFromList",
                new[] { descriptor }, typeof(void)), prefix, null);
            Patch(ExactPatchableMethod(manager, "SaveStashedArea",
                new[] { descriptor, areaState }, typeof(void)), prefix, null);
            MethodInfo saveRoutine = manager.GetMethod("SaveRoutine",
                BindingFlags.Instance | BindingFlags.Public |
                BindingFlags.NonPublic | BindingFlags.DeclaredOnly,
                null, new[] { descriptor, typeof(bool) }, null);
            if (saveRoutine == null || saveRoutine.DeclaringType != manager ||
                saveRoutine.ReturnType.FullName !=
                    "System.Collections.Generic.IEnumerator`1[[System.Object, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]]")
                throw new MissingMethodException(manager.FullName,
                    "SaveRoutine exact save-write sentinel contract");
            RequirePatchableContract(saveRoutine, manager, "SaveRoutine",
                new[] { descriptor, typeof(bool) }, saveRoutine.ReturnType);
            Patch(saveRoutine, prefix, null);
        }

        private void InstallLegacySaveWriteSentinels(
            Assembly assembly, MethodInfo prefix)
        {
            foreach (MethodInfo method in assembly.GetType(
                "Kingmaker.EntitySystem.Persistence.SaveManager", true)
                .GetMethods(AllInstance))
            {
                if (WritePrefixes.Any(prefixValue =>
                    method.Name.StartsWith(prefixValue, StringComparison.Ordinal)) &&
                    method.Name != "SaveList" && method.Name != "SaveInfo")
                    Patch(method, prefix, null);
            }
        }

        private static void RequirePatchableContract(MethodInfo method,
            Type declaringType, string name, Type[] parameterTypes,
            Type returnType)
        {
            if (method == null || method.DeclaringType != declaringType ||
                method.Name != name || method.ReturnType != returnType ||
                method.IsAbstract || method.IsGenericMethodDefinition ||
                method.ContainsGenericParameters || method.GetMethodBody() == null ||
                !method.GetParameters().Select(value => value.ParameterType)
                    .SequenceEqual(parameterTypes))
                throw new MissingMethodException(declaringType.FullName,
                    name + " exact managed patchable contract");
        }

        private void ResolveWorkingSelectionLoadActions()
        {
            _observerArmingSubstage = "working-selection-component-discovery";
            var entries = Resources.FindObjectsOfTypeAll(typeof(Component))
                .OfType<Component>()
                .Where(component => component != null &&
                    component.gameObject.activeInHierarchy &&
                    ObjectContainsReference(component, _workingDescriptor))
                .ToList();
            entries = UniqueComponents(entries);
            _entryCandidates = entries.Count;
            if (entries.Count != 1) return;
            _entryOwner = entries[0];

            _observerArmingSubstage = "working-selection-owner-discovery";
            Transform root = _entryOwner.transform;
            while (root.parent != null)
            {
                Component[] components = root.gameObject.GetComponents<Component>();
                if (components.Any(component => component != null &&
                    component.GetType().FullName == "Kingmaker.UI.SaveLoadWindow.SaveLoadWindow"))
                    break;
                root = root.parent;
            }
            foreach (Component component in root.gameObject.GetComponentsInChildren<Component>(true))
            {
                if (component == null) continue;
                string typeName = component.GetType().FullName;
                bool relevant = ReferenceEquals(component, _entryOwner) ||
                    typeName == CatalogType ||
                    typeName == "Kingmaker.UI.SaveLoadWindow.SaveLoadWindow" ||
                    component is Selectable ||
                    typeName == "UnityEngine.EventSystems.EventTrigger" ||
                    ImplementsUiActionHandler(component.GetType());
                if (!relevant) continue;
                AddUniqueReference(_scopedReceivers, component);
                if (typeName == CatalogType) _listObjectIdentity = ObjectIdentity(component);
                if (typeName == "Kingmaker.UI.SaveLoadWindow.SaveLoadWindow")
                    _ownerObjectIdentity = ObjectIdentity(component);
                DiscoverCandidateMethods(component);
            }
            _observerArmingSubstage = "working-selection-unityevent-inspection";
            foreach (Button button in root.gameObject.GetComponentsInChildren<Button>(true))
            {
                if (button == null || !button.gameObject.activeInHierarchy ||
                    !button.interactable) continue;
                foreach (Delegate listener in RuntimeDelegates(button.onClick))
                {
                    AddUniqueReference(_scopedReceivers, listener.Target);
                    RecordCandidateMethod(listener.Method,
                        "button=" + ObjectIdentity(button) + ";event=" +
                        ObjectIdentity(button.onClick) + ";target=" +
                        ObjectIdentity(listener.Target));
                }
            }
            _entryActionCandidates = _scopedActionCandidates.Count;
            _observerArmingSubstage = "working-selection-selected-state-inspection";
            CaptureSelectedSaveStorage("readiness");
            _observerArmingSubstage = "working-selection-readiness-writing";
            Add("working-selection-load-observer-ready", null, null,
                "entry=" + ObjectIdentity(_entryOwner) + ";owner=" +
                _ownerObjectIdentity + ";list=" + _listObjectIdentity +
                ";candidates=" + _entryActionCandidates);
        }

        private void DiscoverCandidateMethods(object receiver)
        {
            _observerArmingSubstage = "working-selection-candidate-method-enumeration";
            foreach (MethodInfo method in receiver.GetType().GetMethods(AllInstance))
            {
                bool interfaceAction = IsUiActionMethod(receiver.GetType(), method);
                if (!interfaceAction) continue;
                RecordCandidateMethod(method, "receiver=" + ObjectIdentity(receiver));
            }
        }

        private static bool ImplementsUiActionHandler(Type type)
        {
            return type.GetInterfaces().Any(value => IsUiActionInterface(value.FullName));
        }

        private static bool IsUiActionMethod(Type type, MethodInfo method)
        {
            foreach (Type contract in type.GetInterfaces())
            {
                if (!IsUiActionInterface(contract.FullName)) continue;
                InterfaceMapping map;
                try { map = type.GetInterfaceMap(contract); } catch { continue; }
                if (map.TargetMethods.Any(value => value == method)) return true;
            }
            return false;
        }

        private static bool IsUiActionInterface(string name)
        {
            return name == "UnityEngine.EventSystems.IPointerClickHandler" ||
                name == "UnityEngine.EventSystems.ISubmitHandler" ||
                name == "UnityEngine.EventSystems.ISelectHandler" ||
                name == "UnityEngine.EventSystems.IDeselectHandler" ||
                name == "UnityEngine.EventSystems.IPointerDownHandler" ||
                name == "UnityEngine.EventSystems.IPointerUpHandler";
        }

        private void RecordCandidateMethod(MethodInfo method, string source)
        {
            if (method == null || method.DeclaringType == typeof(object)) return;
            string rejection = CandidateRejection(method);
            if (rejection.Length != 0)
            {
                string rejected = FormatSignature(method) + ";reason=" + rejection +
                    ";" + source;
                if (!_candidateRejections.Contains(rejected))
                    _candidateRejections.Add(rejected);
                Add("optional-action-candidate-rejected", method, null, rejected);
                return;
            }
            if (!_scopedActionMethods.Contains(method)) _scopedActionMethods.Add(method);
            string candidate = FormatSignature(method) + ";" + source;
            if (!_scopedActionCandidates.Contains(candidate))
                _scopedActionCandidates.Add(candidate);
        }

        private static string CandidateRejection(MethodInfo method)
        {
            if (method.IsAbstract) return "abstract";
            if (method.IsGenericMethodDefinition || method.ContainsGenericParameters)
                return "generic";
            if ((method.Attributes & MethodAttributes.PinvokeImpl) != 0) return "pinvoke";
            MethodImplAttributes implementation = method.GetMethodImplementationFlags();
            if ((implementation & MethodImplAttributes.InternalCall) != 0 ||
                (implementation & MethodImplAttributes.Runtime) != 0)
                return "runtime-implemented";
            if (method.GetMethodBody() == null) return "no-managed-body";
            if (method.IsSpecialName) return "property-or-event-accessor";
            if (method.DeclaringType != null &&
                method.DeclaringType.FullName.StartsWith("UnityEngine.", StringComparison.Ordinal))
                return "unity-framework-method";
            return "";
        }

        private static void AddUniqueReference(List<object> values, object value)
        {
            if (value != null && !values.Any(item => ReferenceEquals(item, value)))
                values.Add(value);
        }

        private void CaptureSelectedSaveStorage(string phase)
        {
            foreach (object receiver in _scopedReceivers.ToArray())
            {
                if (receiver == null) continue;
                if (ReferenceEquals(receiver, _entryOwner)) continue;
                for (Type type = receiver.GetType(); type != null; type = type.BaseType)
                foreach (FieldInfo field in type.GetFields(BindingFlags.Instance |
                    BindingFlags.Public | BindingFlags.NonPublic |
                    BindingFlags.DeclaredOnly))
                {
                    if (field.FieldType.IsValueType || field.FieldType == typeof(string))
                        continue;
                    object value;
                    try { value = field.GetValue(receiver); } catch { continue; }
                    if (!ReferenceEquals(value, _workingDescriptor)) continue;
                    _selectedWorkingStateObserved = true;
                    string storage = phase + ";owner=" + ObjectIdentity(receiver) +
                        ";field=" + field.DeclaringType.FullName + "." + field.Name +
                        ";descriptor=" + ObjectIdentity(value);
                    if (!_selectedSaveStorage.Contains(storage))
                        _selectedSaveStorage.Add(storage);
                }
                for (Type propertyType = receiver.GetType(); propertyType != null;
                    propertyType = propertyType.BaseType)
                foreach (PropertyInfo property in propertyType.GetProperties(
                    BindingFlags.Instance | BindingFlags.Public |
                    BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
                {
                    if (!property.CanRead || property.GetIndexParameters().Length != 0 ||
                        property.PropertyType.IsValueType ||
                        property.PropertyType == typeof(string)) continue;
                    object value;
                    try { value = property.GetValue(receiver, null); } catch { continue; }
                    if (!ReferenceEquals(value, _workingDescriptor)) continue;
                    _selectedWorkingStateObserved = true;
                    string storage = phase + ";owner=" + ObjectIdentity(receiver) +
                        ";property=" + property.DeclaringType.FullName + "." +
                        property.Name + ";descriptor=" + ObjectIdentity(value);
                    if (!_selectedSaveStorage.Contains(storage))
                        _selectedSaveStorage.Add(storage);
                }
            }
        }

        private void ResolveWorkingEntryAction()
        {
            var entries = new List<Component>();
            foreach (Component component in Resources.FindObjectsOfTypeAll(
                typeof(Component)).OfType<Component>())
            {
                if (component == null || !component.gameObject.activeInHierarchy)
                    continue;
                if (ObjectContainsReference(component, _workingDescriptor))
                    entries.Add(component);
            }
            entries = UniqueComponents(entries);
            _entryCandidates = entries.Count;
            if (entries.Count != 1) return;
            _entryOwner = entries[0];
            var matches = new List<Button>();
            foreach (Button button in EntryButtons(_entryOwner))
            {
                if (button == null || !button.gameObject.activeInHierarchy ||
                    !button.interactable) continue;
                List<Delegate> delegates = RuntimeDelegates(button.onClick);
                if (!delegates.Any(value =>
                    ReferenceEquals(value.Target, _entryOwner) ||
                    ObjectContainsReference(value.Target, _workingDescriptor)))
                    continue;
                matches.Add(button);
            }
            matches = matches.Distinct().ToList();
            _entryActionCandidates = matches.Count;
            if (matches.Count != 1) return;
            _entryAction = matches[0];
            _entryUnityEvent = _entryAction.onClick;
            List<Delegate> listeners = RuntimeDelegates(_entryUnityEvent).Where(
                value => ReferenceEquals(value.Target, _entryOwner) ||
                    ObjectContainsReference(value.Target, _workingDescriptor)).ToList();
            if (listeners.Count != 1)
            {
                _entryActionCandidates = listeners.Count;
                return;
            }
            _listenerTarget = listeners[0].Target;
            _listenerMethod = listeners[0].Method;
            _entryEvidence = ButtonEvidence(_entryAction, _entryOwner);
            MethodInfo eventInvoke = typeof(UnityEvent).GetMethod("Invoke",
                BindingFlags.Instance | BindingFlags.Public, null, Type.EmptyTypes, null);
            MethodInfo prefix = typeof(WorkingSaveSmokeScenario).GetMethod(
                "Prefix", BindingFlags.Static | BindingFlags.NonPublic);
            Patch(eventInvoke, prefix, null);
            if (_listenerMethod != null && !_patched.Contains(_listenerMethod))
                Patch(_listenerMethod, prefix, null);
            Add("working-entry-action-ready", _listenerMethod, null,
                "descriptorCorrelation=object-reference;entry=" +
                ObjectIdentity(_entryOwner) + ";action=" +
                ObjectIdentity(_entryUnityEvent));
        }

        private static LoadGameButtonCandidateEvidence ButtonEvidence(
            Button button, Component owner)
        {
            return new LoadGameButtonCandidateEvidence
            {
                ComponentType = button.GetType().FullName,
                GameObjectPath = HierarchyPath(button.transform),
                ActiveSelf = button.gameObject.activeSelf,
                ActiveInHierarchy = button.gameObject.activeInHierarchy,
                Interactable = button.interactable,
                SiblingIndex = button.transform.GetSiblingIndex(),
                SiblingCount = button.transform.parent == null ? 0 :
                    button.transform.parent.childCount,
                OwnerType = owner.GetType().FullName,
                MainMenuRootName = Root(button.transform).gameObject.name,
                MainMenuRootPath = HierarchyPath(Root(button.transform)),
                ComponentIdentities = button.gameObject.GetComponents<Component>()
                    .Where(x => x != null).Select(x => x.GetType().FullName)
                    .OrderBy(x => x).ToList(),
                SafeLabelIdentities = SafeLabelIdentities(button.gameObject),
                Listeners = ReadListeners(button.onClick)
            };
        }

        private static IEnumerable<Button> EntryButtons(Component owner)
        {
            var result = new List<Button>();
            Transform current = owner.transform;
            for (int depth = 0; current != null && depth < 3;
                depth++, current = current.parent)
                result.AddRange(current.gameObject.GetComponentsInChildren<Button>(true));
            return result.Distinct();
        }

        private static List<Component> UniqueComponents(List<Component> values)
        {
            var result = new List<Component>();
            foreach (Component value in values)
                if (!result.Any(item => ReferenceEquals(item, value)))
                    result.Add(value);
            return result;
        }

        private static bool ObjectContainsReference(object owner, object expected)
        {
            if (owner == null || expected == null) return false;
            if (ReferenceEquals(owner, expected)) return true;
            for (Type type = owner.GetType(); type != null; type = type.BaseType)
                foreach (FieldInfo field in type.GetFields(BindingFlags.Instance |
                    BindingFlags.Public | BindingFlags.NonPublic |
                    BindingFlags.DeclaredOnly))
                {
                    if (field.FieldType.IsValueType ||
                        field.FieldType == typeof(string)) continue;
                    object value;
                    try { value = field.GetValue(owner); } catch { continue; }
                    if (ReferenceEquals(value, expected)) return true;
                }
            return false;
        }

        private static List<Delegate> RuntimeDelegates(UnityEvent action)
        {
            var result = new List<Delegate>();
            object calls = ReadField(action, "m_Calls");
            foreach (object invokable in EnumerateCalls(calls))
            {
                Delegate callback = FindDelegate(invokable);
                if (callback == null) continue;
                result.AddRange(callback.GetInvocationList());
            }
            return result;
        }

        private static string ObjectIdentity(object value)
        {
            if (value == null) return "";
            return value.GetType().FullName + "#" +
                System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(value);
        }

        private void ResolveMainMenu()
        {
            // The earlier supervised observation proved the active
            // MainMenuButtons receiver and exact hierarchy, not a separate
            // Kingmaker.MainMenu component. Resolve that proven lifecycle
            // receiver once; button/listener invariants remain a separate gate.
            Type type = typeof(Kingmaker.Game).Assembly.GetType(OwnerType, true);
            UnityEngine.Object[] candidates = Resources.FindObjectsOfTypeAll(type);
            var exact = candidates.Where(value =>
            {
                Component component = value as Component;
                return component != null && component.gameObject.activeInHierarchy &&
                    Root(component.transform).gameObject.name == "!LIGHT_SETUP";
            }).Cast<object>().ToList();
            if (exact.Count == 1)
                _mainMenuButtons = exact[0];
            else if (exact.Count > 1)
                throw new AmbiguousMatchException("Multiple exact MainMenu receivers.");
        }

        private void ResolveButton()
        {
            var matches = new List<Tuple<Button, LoadGameButtonCandidateEvidence>>();
            foreach (Button button in Resources.FindObjectsOfTypeAll(typeof(Button))
                .OfType<Button>())
            {
                LoadGameButtonCandidateEvidence evidence;
                if (TryExactButton(button, out evidence))
                    matches.Add(Tuple.Create(button, evidence));
            }
            _buttonCandidates = matches.Count;
            if (matches.Count == 1)
            {
                _button = matches[0].Item1;
                _buttonEvidence = matches[0].Item2;
            }
        }

        private bool TryExactButton(Button button,
            out LoadGameButtonCandidateEvidence evidence)
        {
            evidence = null;
            if (button == null || !button.gameObject.activeSelf ||
                !button.gameObject.activeInHierarchy || !button.interactable ||
                button.GetType().FullName != "UnityEngine.UI.Button" ||
                HierarchyPath(button.transform) != ButtonPath ||
                button.transform.GetSiblingIndex() != 2 ||
                button.transform.parent == null ||
                button.transform.parent.childCount != 7 ||
                Root(button.transform).gameObject.name != "!LIGHT_SETUP")
                return false;
            List<string> components = button.gameObject.GetComponents<Component>()
                .Where(x => x != null).Select(x => x.GetType().FullName)
                .OrderBy(x => x).ToList();
            if (!components.SequenceEqual(ExactComponents.OrderBy(x => x)))
                return false;
            List<LoadGameListenerEvidence> listeners = ReadListeners(button.onClick);
            List<string> labels = SafeLabelIdentities(button.gameObject);
            if (!ExactLabelIdentities.All(labels.Contains)) return false;
            bool persistent = listeners.Any(x => x.Kind == "persistent" &&
                x.TargetType == "<null>" && x.MethodName == "OnButtonLoadGame");
            List<LoadGameListenerEvidence> runtime = listeners.Where(x =>
                x.Kind == "runtime" && x.TargetType == OwnerType &&
                x.MethodName == "OnButtonLoadGame").ToList();
            if (!persistent || runtime.Count != 1) return false;
            evidence = new LoadGameButtonCandidateEvidence
            {
                ComponentType = button.GetType().FullName,
                GameObjectPath = ButtonPath,
                ActiveSelf = true,
                ActiveInHierarchy = true,
                Interactable = true,
                SiblingIndex = 2,
                SiblingCount = 7,
                OwnerType = OwnerType,
                MainMenuRootName = "!LIGHT_SETUP",
                MainMenuRootPath = "!LIGHT_SETUP",
                ComponentIdentities = components,
                SafeLabelIdentities = labels,
                Listeners = listeners
            };
            return true;
        }

        private void ResolveDescriptors()
        {
            IEnumerable values = _catalogObject as IEnumerable;
            if (values == null) return;
            var entries = values.Cast<object>().ToList();
            _descriptorCount = entries.Count;
            int declaredCount = ReadCount(_catalogObject);
            _catalogComplete = declaredCount >= 0 && declaredCount == entries.Count &&
                IsSaveInfoList(_catalogObject.GetType());
            var working = entries.Where(IsWorking).ToList();
            var baseline = entries.Where(IsBaseline).ToList();
            _workingCount = working.Count;
            _baselineCount = baseline.Count;
            if (_workingCount == 1)
            {
                _workingDescriptor = working[0];
                _workingEvidence = DescriptorEvidence(_workingDescriptor, "working");
            }
        }

        private static object ResolveLoadEntryReceiver(
            Component lifecycleReceiver, Type expectedType)
        {
            var matches = new List<object>();
            if (lifecycleReceiver != null)
            {
                Transform root = Root(lifecycleReceiver.transform);
                foreach (Component component in root.gameObject
                    .GetComponentsInChildren(expectedType, true))
                {
                    if (component != null &&
                        expectedType.IsAssignableFrom(component.GetType()))
                        matches.Add(component);
                }
            }
            AddTypedMembers(null, expectedType, expectedType, matches);
            AddTypedMembers(Kingmaker.Game.Instance,
                Kingmaker.Game.Instance.GetType(), expectedType, matches);
            matches = matches.Where(value => value != null).Aggregate(
                new List<object>(), (unique, value) =>
                {
                    if (!unique.Any(item => ReferenceEquals(item, value)))
                        unique.Add(value);
                    return unique;
                });
            if (matches.Count > 1)
                throw new AmbiguousMatchException(
                    "Multiple exact Kingmaker.MainMenu load-entry receivers were resolved.");
            return matches.Count == 1 ? matches[0] : null;
        }

        private static void AddTypedMembers(object receiver, Type declaringType,
            Type expectedType, List<object> matches)
        {
            BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic |
                (receiver == null ? BindingFlags.Static : BindingFlags.Instance);
            foreach (FieldInfo field in declaringType.GetFields(flags))
            {
                if (!expectedType.IsAssignableFrom(field.FieldType)) continue;
                try { matches.Add(field.GetValue(receiver)); } catch { }
            }
            foreach (PropertyInfo property in declaringType.GetProperties(flags))
            {
                if (!expectedType.IsAssignableFrom(property.PropertyType) ||
                    property.GetIndexParameters().Length != 0 || !property.CanRead)
                    continue;
                try { matches.Add(property.GetValue(receiver, null)); } catch { }
            }
        }

        private static bool IsWorking(object value)
        {
            return value != null && value.GetType().FullName == DescriptorType &&
                Read(value, "Name") == ExpectedName &&
                Leaf(Read(value, "FolderName")) == ExpectedFile &&
                Leaf(Read(value, "FileName")) == ExpectedFile &&
                Read(value, "GameName") == ExpectedGameName &&
                Read(value, "GameId") == ExpectedGameId &&
                Read(value, "Area") == ExpectedArea;
        }

        private static bool IsBaseline(object value)
        {
            return value != null && value.GetType().FullName == DescriptorType &&
                Read(value, "Name") == ForbiddenName &&
                Leaf(Read(value, "FolderName")) == BaselineFile &&
                Leaf(Read(value, "FileName")) == BaselineFile;
        }

        private void RegisterCompletionCallback()
        {
            object manager = ReadMember(Kingmaker.Game.Instance, "SaveManager");
            if (manager == null) return;
            MethodInfo method = manager.GetType().GetMethod("AddCallbackAfterLoad",
                BindingFlags.Instance | BindingFlags.Public, null,
                new[] { typeof(Action) }, null);
            if (method == null) return;
            method.Invoke(manager, new object[] { new Action(OnLoadCompleted) });
            _callbackRegistered = true;
            Add("completion-callback-registered", method, null,
                "read-only callback registration");
        }

        private void PollFingerprint()
        {
            string value = Fingerprint(Kingmaker.Game.Instance);
            if (value.Length == 0) return;
            if (value == _lastFingerprint) _stableSamples++;
            else { _lastFingerprint = value; _stableSamples = 1; }
            if (_stableSamples == 2)
            {
                _fingerprintSequence = _events.Count + 1;
                Add("stable-post-load-fingerprint", null, null, value);
                Add("fingerprint-complete", null, null, value);
            }
        }

        private static void Prefix(MethodBase __originalMethod, object __instance,
            object[] __args)
        {
            WorkingSaveSmokeScenario active = _active;
            if (active == null) return;
            try
            {
                active.ObserveEnter(__originalMethod, __instance, __args);
            }
            catch (Exception exception)
            {
                active.CaptureHookException("prefix", __originalMethod, exception);
            }
        }

        private static void Postfix(MethodBase __originalMethod, object[] __args)
        {
            WorkingSaveSmokeScenario active = _active;
            if (active == null || __originalMethod != active._initialize) return;
            try
            {
                active.Add("catalog-initialize-exit", __originalMethod, __args, "");
            }
            catch (Exception exception)
            {
                active.CaptureHookException("postfix", __originalMethod, exception);
            }
        }

        private void CaptureHookException(
            string hook, MethodBase method, Exception exception)
        {
            if (_exception == null) _exception = exception;
            try
            {
                Add("observation-hook-error", method, null,
                    "hook=" + hook + "; original execution preserved; " +
                    exception.GetType().FullName + ": " + exception.Message);
            }
            catch
            {
                // A diagnostic failure must never escape into the game handler.
            }
        }

        private void ObserveEnter(MethodBase method, object receiver, object[] args)
        {
            RequireGameThread();
            if ((_observeReceiverBoundAction || _autonomousReceiverBoundAction) &&
                method == _slotAction)
            {
                _slotActionInvocations++;
                bool exact = ReferenceEquals(receiver, _receiverBoundSlot);
                _slotReceiverCorrelated = _slotReceiverCorrelated || exact;
                _slotActionSequence = _events.Count + 1;
                Add("receiver-bound-slot-action-enter", method, args,
                    "count=" + _slotActionInvocations + ";receiver=" +
                    ObjectIdentity(receiver) + ";exactWorkingSlot=" + exact);
                if (_stage == "working-entry-click" ||
                    _stage == "receiver-bound-action-invocation")
                    Transition("slot-action-invocation",
                        _autonomousReceiverBoundAction
                            ? "autonomous exact normal action entered SaveSlot boundary"
                            : "human normal action entered exact SaveSlot boundary");
            }
            else if ((_observeReceiverBoundAction || _autonomousReceiverBoundAction) &&
                method == _windowHandler)
            {
                _windowHandlerInvocations++;
                object argument = args == null || args.Length != 1 ? null : args[0];
                bool exactReceiver = ReferenceEquals(receiver, _receiverBoundWindow);
                bool exactArgument = ReferenceEquals(argument, _workingDescriptor);
                _windowReceiverCorrelated =
                    _windowReceiverCorrelated || exactReceiver;
                _windowArgumentCorrelated =
                    _windowArgumentCorrelated || exactArgument;
                _baselineLoadObserved = _baselineLoadObserved || IsBaseline(argument);
                _otherLoadObserved = _otherLoadObserved ||
                    (!IsBaseline(argument) && !exactArgument);
                _windowHandlerSequence = _events.Count + 1;
                Add("receiver-bound-window-handler-enter", method, args,
                    "count=" + _windowHandlerInvocations + ";receiver=" +
                    ObjectIdentity(receiver) + ";exactWindow=" + exactReceiver +
                    ";argument=" + ObjectIdentity(argument) +
                    ";exactWorkingDescriptor=" + exactArgument);
                if (_stage == "slot-action-invocation")
                    Transition("window-handler-invocation",
                        "exact owning SaveLoadWindow handler observed");
            }
            else if (method == _handler)
            {
                _handlerInvocations++;
                Add("load-game-handler-enter", method, args,
                    "count=" + _handlerInvocations);
            }
            else if (method == _initialize)
            {
                _catalogInvocations++;
                Add("catalog-enter", method, args, "");
                if (_buttonInvocations != 1 || _handlerInvocations != 1)
                    throw new InvalidOperationException(
                        "Catalog initialization was not ordered after one action.");
                _catalogObject = args == null || args.Length == 0 ? null : args[0];
                _catalogReceiver = receiver;
                Add("catalog-initialize-enter", method, args,
                    "capturedExactList=True;count=" + _catalogInvocations);
            }
            else if (method == _loadEntry)
            {
                object argument = args == null || args.Length == 0 ? null : args[0];
                _baselineLoadObserved = IsBaseline(argument);
                _otherLoadObserved = !_baselineLoadObserved &&
                    !ReferenceEquals(argument, _workingDescriptor);
                _descriptorCorrelated = ReferenceEquals(argument, _workingDescriptor) &&
                    ContainsReference(_catalogObject, _workingDescriptor);
                _observedLoadReceiver = receiver;
                if (_observeEntryAction || _autonomousReceiverBoundAction)
                    _loadEntryInvocations++;
                if (_observeReceiverBoundAction || _autonomousReceiverBoundAction)
                    _loadEntrySequence = _events.Count + 1;
                if (_observeSelectionLoadAction)
                {
                    CaptureSelectedSaveStorage("load-entry");
                    CaptureLoadCallerChain();
                }
                Add("load-entry-enter", method, args,
                    "objectReferenceCorrelated=" + _descriptorCorrelated);
                if ((_observeEntryAction || _autonomousReceiverBoundAction) &&
                    _descriptorCorrelated &&
                    (_stage == "working-entry-click" ||
                     _stage == "window-handler-invocation" ||
                     _stage == "load-entry-invocation"))
                    Transition("load-completion",
                        "human-selected load entry correlated to exact working descriptor");
            }
            else if (_observeEntryAction &&
                method.DeclaringType == typeof(UnityEvent) &&
                ReferenceEquals(receiver, _entryUnityEvent))
            {
                _humanActionInvocations++;
                Add("working-entry-unityevent-enter", method, args,
                    "count=" + _humanActionInvocations + ";action=" +
                    ObjectIdentity(receiver));
                if (_stage == "working-entry-click")
                    Transition("load-entry-invocation",
                        "human invoked the pre-click-correlated working entry action");
            }
            else if (_observeEntryAction && method == _listenerMethod &&
                (ReferenceEquals(receiver, _listenerTarget) ||
                 (_listenerTarget == null && receiver == null)))
            {
                _listenerInvocations++;
                Add("working-entry-listener-enter", method, args,
                    "count=" + _listenerInvocations + ";target=" +
                    ObjectIdentity(receiver));
            }
            else if (_observeSelectionLoadAction &&
                _scopedActionMethods.Contains(method as MethodInfo) &&
                _scopedReceivers.Any(value => ReferenceEquals(value, receiver)))
            {
                string invocation = FormatSignature(method) + ";receiver=" +
                    ObjectIdentity(receiver) + ";sequence=" +
                    (_observedScopedInvocations.Count + 1);
                _observedScopedInvocations.Add(invocation);
                CaptureSelectedSaveStorage("before-action");
                Add("scoped-ui-action-enter", method, args, invocation);
            }
            else if (_observeSelectionLoadAction &&
                _scopedActionMethods.Contains(method as MethodInfo))
            {
                // The same method may execute on an unrelated row or window.
                // It is outside this request's exact receiver scope.
            }
            else
            {
                _writeObserved = true;
                Add("unexpected-save-write", method, args,
                    "native save-writing or migration entry observed");
            }
        }

        private void CaptureLoadCallerChain()
        {
            _loadCallerChain.Clear();
            StackFrame[] frames = new StackTrace(1, false).GetFrames() ??
                new StackFrame[0];
            foreach (StackFrame frame in frames)
            {
                MethodBase caller = frame.GetMethod();
                if (caller == null) continue;
                string signature = FormatSignature(caller);
                _loadCallerChain.Add(signature);
                if (_immediateLoadCaller.Length != 0 ||
                    caller.DeclaringType == typeof(WorkingSaveSmokeScenario) ||
                    (caller.DeclaringType != null &&
                     caller.DeclaringType.FullName.StartsWith("Harmony", StringComparison.Ordinal)))
                    continue;
                _immediateLoadCaller = signature;
                _immediateLoadCallerType = caller.DeclaringType == null ? "" :
                    caller.DeclaringType.FullName;
                Type callerType = caller.DeclaringType;
                List<object> receivers = callerType == null ? new List<object>() :
                    _scopedReceivers.Where(value => value != null &&
                        callerType.IsInstanceOfType(value)).ToList();
                _compatibleCallerReceiverCount = receivers.Count;
                if (receivers.Count == 1)
                {
                    _immediateLoadCallerReceiverIdentity = ObjectIdentity(receivers[0]);
                    _finalLoadActionCount = 1;
                }
            }
            Add("load-entry-caller-chain", _loadEntry, null,
                "immediate=" + _immediateLoadCaller + ";frames=" +
                _loadCallerChain.Count + ";finalActionCount=" +
                _finalLoadActionCount + ";compatibleReceivers=" +
                _compatibleCallerReceiverCount + ";receiver=" +
                _immediateLoadCallerReceiverIdentity);
        }

        private void OnLoadCompleted()
        {
            RequireGameThread();
            _completionCallback = true;
            _completionSequence = _events.Count + 1;
            Add("after-load-callback", null, null,
                "SaveManager after-load callback invoked");
        }

        private void Transition(string stage, string detail)
        {
            _lastCompletedStage = _stage;
            _stage = stage;
            _stageStarted = _elapsed.ElapsedMilliseconds;
            Add("stage-enter", null, null, "stage=" + stage + ";" + detail);
        }

        private void RequireGameThread()
        {
            if (Thread.CurrentThread.ManagedThreadId == _gameThreadId) return;
            _wrongThread = true;
            throw new InvalidOperationException("Scenario callback was off game thread.");
        }

        private void Patch(MethodBase method, MethodInfo prefix, MethodInfo postfix)
        {
            _context.Harmony.Patch(method,
                prefix == null ? null : new HarmonyMethod(prefix),
                postfix == null ? null : new HarmonyMethod(postfix), null);
            _patched.Add(method);
        }

        private void RemoveHooks()
        {
            foreach (MethodBase method in _patched.ToArray())
            {
                try
                {
                    _context.Harmony.Unpatch(
                        method, HarmonyPatchType.All, _context.ModId);
                }
                catch (Exception exception)
                {
                    if (_exception == null) _exception = exception;
                }
            }
            _patched.Clear();
            _removed = true;
            if (ReferenceEquals(_active, this)) _active = null;
        }

        private void RemoveUiHooks()
        {
            foreach (MethodBase method in new MethodBase[] { _handler, _initialize })
            {
                if (method == null || !_patched.Contains(method)) continue;
                _context.Harmony.Unpatch(method, HarmonyPatchType.All,
                    _context.ModId);
                _patched.Remove(method);
            }
        }

        private void Add(string kind, MethodBase method, object[] args, string detail)
        {
            var item = new SaveLoadObservationEvent
            {
                RunId = _runId, Sequence = _events.Count + 1,
                ElapsedMilliseconds = _elapsed.ElapsedMilliseconds,
                Utc = DateTime.UtcNow.ToString("o"), Kind = kind,
                DeclaringType = method == null || method.DeclaringType == null
                    ? "" : method.DeclaringType.FullName,
                MethodSignature = method == null ? "" : FormatSignature(method),
                ArgumentTypes = args == null ? new List<string>() :
                    args.Select(x => x == null ? "null" : x.GetType().FullName).ToList(),
                ManagedThreadId = Thread.CurrentThread.ManagedThreadId,
                DisplayName = "", SafeSaveIdentifier = "",
                Detail = detail ?? "", Exception = ""
            };
            _events.Add(item);
            if (_sink != null) try { _sink(item); } catch (Exception ex) { _exception = ex; }
        }

        private static List<LoadGameListenerEvidence> ReadListeners(UnityEvent action)
        {
            var result = new List<LoadGameListenerEvidence>();
            for (int index = 0; index < action.GetPersistentEventCount(); index++)
            {
                UnityEngine.Object target = action.GetPersistentTarget(index);
                result.Add(new LoadGameListenerEvidence
                {
                    Kind = "persistent",
                    TargetType = target == null ? "<null>" : target.GetType().FullName,
                    MethodName = action.GetPersistentMethodName(index)
                });
            }
            object calls = ReadField(action, "m_Calls");
            foreach (object invokable in EnumerateCalls(calls))
            {
                Delegate callback = FindDelegate(invokable);
                if (callback == null) continue;
                foreach (Delegate item in callback.GetInvocationList())
                {
                    var evidence = new LoadGameListenerEvidence
                    {
                        Kind = "runtime",
                        TargetType = item.Target == null ? "<static>" :
                            item.Target.GetType().FullName,
                        MethodName = item.Method.Name,
                        SafeCapturedFields = SafeCapturedFields(item.Target)
                    };
                    if (!result.Any(x => x.Kind == evidence.Kind &&
                        x.TargetType == evidence.TargetType &&
                        x.MethodName == evidence.MethodName)) result.Add(evidence);
                }
            }
            return result;
        }

        private static List<string> SafeCapturedFields(object target)
        {
            var result = new List<string>();
            for (Type type = target == null ? null : target.GetType();
                type != null; type = type.BaseType)
                foreach (FieldInfo field in type.GetFields(BindingFlags.Instance |
                    BindingFlags.Public | BindingFlags.NonPublic |
                    BindingFlags.DeclaredOnly))
                    result.Add(type.FullName + "." + field.Name + ":" +
                        field.FieldType.FullName);
            return result.Distinct().OrderBy(x => x).ToList();
        }

        private static List<string> SafeLabelIdentities(GameObject gameObject)
        {
            var values = new List<string>();
            foreach (Component component in
                gameObject.GetComponentsInChildren<Component>(true))
            {
                if (component == null) continue;
                Type type = component.GetType();
                foreach (MemberInfo member in type.GetMembers(BindingFlags.Instance |
                    BindingFlags.Public | BindingFlags.NonPublic))
                {
                    if (member.Name.IndexOf("text",
                        StringComparison.OrdinalIgnoreCase) < 0) continue;
                    string text = null;
                    try
                    {
                        FieldInfo field = member as FieldInfo;
                        PropertyInfo property = member as PropertyInfo;
                        if (field != null && field.FieldType == typeof(string))
                            text = field.GetValue(component) as string;
                        else if (property != null &&
                            property.PropertyType == typeof(string) &&
                            property.GetIndexParameters().Length == 0 &&
                            property.CanRead)
                            text = property.GetValue(component, null) as string;
                    }
                    catch { continue; }
                    if (!string.IsNullOrWhiteSpace(text) && text.Length <= 160)
                        values.Add(type.FullName + "." + member.Name + "=" + text);
                }
            }
            return values.Distinct().OrderBy(x => x).ToList();
        }

        private static IEnumerable<object> EnumerateCalls(object calls)
        {
            if (calls == null) yield break;
            foreach (string name in new[] { "m_PersistentCalls", "m_RuntimeCalls",
                "m_ExecutingCalls" })
            {
                IEnumerable list = ReadField(calls, name) as IEnumerable;
                if (list == null) continue;
                foreach (object value in list) if (value != null) yield return value;
            }
        }

        private static Delegate FindDelegate(object value)
        {
            for (Type type = value == null ? null : value.GetType();
                type != null; type = type.BaseType)
                foreach (FieldInfo field in type.GetFields(BindingFlags.Instance |
                    BindingFlags.Public | BindingFlags.NonPublic |
                    BindingFlags.DeclaredOnly))
                    if (typeof(Delegate).IsAssignableFrom(field.FieldType))
                        try { return field.GetValue(value) as Delegate; } catch { return null; }
            return null;
        }

        private static object ReadField(object value, string name)
        {
            for (Type type = value == null ? null : value.GetType();
                type != null; type = type.BaseType)
            {
                FieldInfo field = type.GetField(name, BindingFlags.Instance |
                    BindingFlags.Public | BindingFlags.NonPublic |
                    BindingFlags.DeclaredOnly);
                if (field != null) try { return field.GetValue(value); } catch { return null; }
            }
            return null;
        }

        private static object ReadMember(object value, string name)
        {
            if (value == null) return null;
            PropertyInfo property = value.GetType().GetProperty(
                name, BindingFlags.Instance | BindingFlags.Public);
            if (property != null && property.GetIndexParameters().Length == 0)
                return property.GetValue(value, null);
            FieldInfo field = value.GetType().GetField(
                name, BindingFlags.Instance | BindingFlags.Public);
            return field == null ? null : field.GetValue(value);
        }

        private static string Read(object value, string name)
        {
            return Convert.ToString(ReadMember(value, name)) ?? "";
        }

        private static int ReadCount(object value)
        {
            int count;
            return int.TryParse(Read(value, "Count"), out count) ? count : -1;
        }

        private static bool ContainsReference(object collection, object item)
        {
            IEnumerable values = collection as IEnumerable;
            return values != null && values.Cast<object>().Any(
                value => ReferenceEquals(value, item));
        }

        private static SaveCatalogDescriptorEvidence DescriptorEvidence(
            object value, string classification)
        {
            return new SaveCatalogDescriptorEvidence
            {
                Classification = classification,
                DisplayName = Read(value, "Name"),
                IdentityHash = "",
                SafeFields = new Dictionary<string, string>
                {
                    { "Name", Read(value, "Name") },
                    { "FolderName", Leaf(Read(value, "FolderName")) },
                    { "FileName", Leaf(Read(value, "FileName")) },
                    { "GameName", Read(value, "GameName") },
                    { "GameId", Read(value, "GameId") },
                    { "Area", Read(value, "Area") }
                }
            };
        }

        private static string Fingerprint(object game)
        {
            object player = ReadMember(game, "Player");
            object area = ReadMember(game, "CurrentlyLoadedArea");
            object scene = ReadMember(game, "CurrentScene");
            object party = ReadMember(player, "Party");
            object main = ReadMember(player, "MainCharacter");
            string value = "areaType=" + TypeName(area) +
                ";sceneType=" + TypeName(scene) +
                ";gameId=" + Read(player, "GameId") +
                ";partyCount=" + ReadCount(party) +
                ";mainCharacterType=" + TypeName(main);
            return TypeName(area) == ExpectedAreaType &&
                TypeName(scene) == ExpectedSceneType &&
                Read(player, "GameId") == ExpectedGameId &&
                ReadCount(party) == ExpectedPartyCount &&
                TypeName(main) == ExpectedMainCharacterType ? value : "";
        }

        private static string TypeName(object value)
        {
            return value == null ? "" : value.GetType().FullName;
        }

        private static Transform Root(Transform value)
        {
            while (value.parent != null) value = value.parent;
            return value;
        }

        private static string HierarchyPath(Transform value)
        {
            var names = new Stack<string>();
            for (Transform item = value; item != null; item = item.parent)
                names.Push(item.gameObject.name);
            return string.Join("/", names.ToArray());
        }

        private static string Leaf(string value)
        {
            try { return System.IO.Path.GetFileName(value) ?? ""; }
            catch { return ""; }
        }

        private static bool IsSaveInfoList(Type type)
        {
            return type != null && type.IsGenericType &&
                type.GetGenericTypeDefinition() == typeof(List<>) &&
                type.GetGenericArguments()[0].FullName == DescriptorType;
        }

        private static string FormatSignature(MethodBase method)
        {
            if (method == null) return "";
            return (method.DeclaringType == null ? "" :
                method.DeclaringType.FullName + ".") + method.Name + "(" +
                string.Join(",", method.GetParameters().Select(
                    x => x.ParameterType.FullName).ToArray()) + ")" +
                (method is MethodInfo ? ":" +
                    ((MethodInfo)method).ReturnType.FullName : "");
        }

        private const BindingFlags AllInstance = BindingFlags.Instance |
            BindingFlags.Public | BindingFlags.NonPublic;
    }
}
