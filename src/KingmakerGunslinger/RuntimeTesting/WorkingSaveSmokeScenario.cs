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
    /// Guarded, request-scoped execution of the three observed main-menu contracts.
    /// It invokes one normal Unity button event, consumes Kingmaker's exact catalog
    /// List instance, and passes its exact working SaveInfo instance to MainMenu.
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
        private string _stage = "runtime-readiness";
        private long _stageStarted;
        private Button _button;
        private LoadGameButtonCandidateEvidence _buttonEvidence;
        private object _mainMenuButtons;
        private object _loadEntryReceiver;
        private object _catalogObject;
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
        private Exception _exception;
        private string _lastCompletedStage = "runtime-readiness";

        internal WorkingSaveSmokeScenario(ModContext context, Stopwatch elapsed,
            string runId, Action<SaveLoadObservationEvent> sink)
        {
            _context = context;
            _elapsed = elapsed;
            _runId = runId;
            _sink = sink;
            _gameThreadId = Thread.CurrentThread.ManagedThreadId;
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
                    _descriptorCorrelated && !_writeObserved && !_wrongThread;
            }
        }
        internal bool WriteObserved { get { return _writeObserved; } }
        internal Exception ScenarioException { get { return _exception; } }
        internal string LastCompletedStage { get { return _lastCompletedStage; } }
        internal int WorkingCount { get { return _workingCount; } }
        internal int BaselineCount { get { return _baselineCount; } }
        internal int ButtonCandidateCount { get { return _buttonCandidates; } }
        internal bool CatalogComplete { get { return _catalogComplete; } }
        internal bool MainMenuReady
        {
            get
            {
                return _mainMenuButtons != null && _loadEntryReceiver != null &&
                    _button != null &&
                    _buttonCandidates == 1 && _stage == "action-invocation";
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
                MethodInfo prefix = typeof(WorkingSaveSmokeScenario).GetMethod(
                    "Prefix", BindingFlags.Static | BindingFlags.NonPublic);
                MethodInfo postfix = typeof(WorkingSaveSmokeScenario).GetMethod(
                    "Postfix", BindingFlags.Static | BindingFlags.NonPublic);
                Patch(_handler, prefix, null);
                Patch(_initialize, prefix, postfix);
                Patch(_loadEntry, prefix, null);
                foreach (MethodInfo method in assembly.GetType(
                    "Kingmaker.EntitySystem.Persistence.SaveManager", true)
                    .GetMethods(AllInstance))
                {
                    if (WritePrefixes.Any(prefixValue =>
                        method.Name.StartsWith(prefixValue, StringComparison.Ordinal)) &&
                        method.Name != "SaveList" && method.Name != "SaveInfo")
                        Patch(method, prefix, null);
                }
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
                if (_mainMenuButtons != null && _loadEntryReceiver != null)
                {
                    Transition("load-game-action-resolution",
                        "exact active Kingmaker MainMenu receiver resolved; overlay was not treated as readiness");
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
                    Transition("load-entry-invocation",
                        "unique working descriptor and distinct baseline proven");
                    return;
                }
            }
            if (_stage == "load-entry-invocation")
            {
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
                Events = new List<SaveLoadObservationEvent>(_events)
            };
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
            {
                _mainMenuButtons = exact[0];
                _loadEntryReceiver = ResolveLoadEntryReceiver(
                    _mainMenuButtons as Component,
                    typeof(Kingmaker.Game).Assembly.GetType(MainMenuType, true));
                if (_loadEntryReceiver == null)
                    throw new MissingMemberException(
                        "The exact Kingmaker.MainMenu load-entry receiver could not be resolved.");
            }
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
            if (method == _handler)
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
                Add("catalog-initialize-enter", method, args,
                    "capturedExactList=True;count=" + _catalogInvocations);
            }
            else if (method == _loadEntry)
            {
                object argument = args == null || args.Length == 0 ? null : args[0];
                _descriptorCorrelated = _descriptorCorrelated &&
                    ReferenceEquals(argument, _workingDescriptor);
                Add("load-entry-enter", method, args,
                    "objectReferenceCorrelated=" + _descriptorCorrelated);
            }
            else
            {
                _writeObserved = true;
                Add("unexpected-save-write", method, args,
                    "native save-writing or migration entry observed");
            }
        }

        private void OnLoadCompleted()
        {
            RequireGameThread();
            _completionCallback = true;
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
                        MethodName = item.Method.Name
                    };
                    if (!result.Any(x => x.Kind == evidence.Kind &&
                        x.TargetType == evidence.TargetType &&
                        x.MethodName == evidence.MethodName)) result.Add(evidence);
                }
            }
            return result;
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
