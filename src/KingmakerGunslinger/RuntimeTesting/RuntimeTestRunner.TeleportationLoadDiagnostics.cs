using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Harmony12;
using Kingmaker;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using KingmakerGunslinger.Spells.Teleportation;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        private TeleportationLoadDiagnostics _teleportationLoadDiagnostics;

        private void StartTeleportationLoadDiagnostics()
        {
            bool fixedDispelUi = RuntimeTestRequestParser.Sprints30And31DispelUi(_request);
            if (_request.Scenario != RuntimeTestScenarioCatalog.DisposableTeleportationCasting && !fixedDispelUi) return;
            if (!_request.ExitAfterCompletion || _workingSaveSmoke == null)
                throw new InvalidOperationException("Load diagnostics require the guarded working-save casting request and automatic exit.");
            _teleportationLoadDiagnostics = new TeleportationLoadDiagnostics(_request.RunId, fixedDispelUi);
            _teleportationLoadDiagnostics.Install();
        }

        private void SealDispelUiLoadDiagnostics()
        {
            if (!RuntimeTestRequestParser.Sprints30And31DispelUi(_request) ||
                _teleportationLoadDiagnostics == null)
                throw new InvalidOperationException("The fixed UI requires its native load observer.");
            bool cleaned = _teleportationLoadDiagnostics.Stop();
            if (!cleaned || !_teleportationLoadDiagnostics.LoadCompleted)
                throw new InvalidOperationException("Native Working load/observer cleanup failed before UI interaction.");
        }

        private void StopTeleportationLoadDiagnostics(RuntimeTestResult result)
        {
            if (_teleportationLoadDiagnostics == null) return;
            TeleportationLoadDiagnostics observer = _teleportationLoadDiagnostics;
            _teleportationLoadDiagnostics = null;
            bool cleaned = observer.Stop();
            bool fixedDispelUi = RuntimeTestRequestParser.Sprints30And31DispelUi(_request);
            string path = Path.Combine(_request.EvidenceDirectory,
                fixedDispelUi ? "sprints30-31-save-load.json" : "teleportation-save-load.json");
            RuntimeTestResultWriter.WriteAtomic(path, TeleportationDiagnosticJson.Serialize(observer.Snapshot()));
            if (fixedDispelUi && !result.EvidenceFiles.Contains(path)) result.EvidenceFiles.Add(path);
            if (result.Assertions == null) result.Assertions = new List<RuntimeTestAssertion>();
            result.Assertions.Add(Assertion("save-load-observer-cleanup",
                "Request-local error listener and five diagnostic hooks removed; native settings retained",
                "cleaned=" + cleaned, cleaned, path));
            result.Assertions.Add(Assertion("save-load-native-main-character",
                "Native Player.PostLoad completed with one matching main character before and after cross-scene PostLoad",
                "loadCompleted=" + observer.LoadCompleted, observer.LoadCompleted, path));
            if ((!cleaned || !observer.LoadCompleted) && result.Status == RuntimeTestStatuses.Pass)
                result.Status = RuntimeTestStatuses.Fail;
        }

        // No HarmonyPatch attributes: only the exact disposable casting request installs
        // these observations. They never handle JSON errors, replace a result, skip a
        // native method, write a save, or resolve a unit reference through its Value.
        private sealed class TeleportationLoadDiagnostics
        {
            private static TeleportationLoadDiagnostics _active;
            private readonly string _runId;
            private readonly bool _captureUnityErrors;
            private readonly List<object> _unityErrors = new List<object>();
            private int _unityErrorCount, _blockingUnityErrorCount;
            private bool _unityListenerAttached;
            private bool _unityListenerRemoved;
            private readonly string _owner;
            private readonly HarmonyInstance _harmony;
            private readonly object _sync = new object();
            private readonly List<MethodInfo> _methods = new List<MethodInfo>();
            private readonly List<object> _states = new List<object>();
            private readonly List<object> _jsonErrors = new List<object>();
            private readonly List<string> _faults = new List<string>();
            private readonly List<int> _mainMatches = new List<int>();
            private JsonSerializerSettings _settings;
            private IContractResolver _resolver;
            private JsonConverter[] _converters;
            private Player _loadingPlayer;
            private int _loaderStarts;
            private int _playerPostfixes;
            private int _errorCount;
            private int _installedHooks;
            private bool _sealed;
            private bool _stopSucceeded;
            private string _stoppedUtc;
            private bool _listenerRemoved;
            private bool _settingsRetained;
            private int _remainingHooks = -1;

            internal TeleportationLoadDiagnostics(string runId, bool captureUnityErrors)
            {
                _runId = runId; _captureUnityErrors = captureUnityErrors;
                _owner = "KingmakerGunslinger.teleportation-load-diagnostics." + runId;
                _harmony = HarmonyInstance.Create(_owner);
            }

            internal void Install()
            {
                if (_active != null) throw new InvalidOperationException("A load observer is already active.");
                _active = this;
                if (_captureUnityErrors)
                {
                    UnityEngine.Application.logMessageReceived += OnUnityLog;
                    _unityListenerAttached = true;
                }
                Patch(typeof(ThreadedGameLoader), "Start", typeof(Task), "LoaderPrefix", null);
                Patch(typeof(Player), "PostLoad", typeof(void), "PlayerPrefix", "PlayerPostfix");
                Patch(typeof(SceneEntitiesState), "PostLoad", typeof(void), "ScenePrefix", "ScenePostfix");
                _installedHooks = HookCount();
                if (_installedHooks != 5) throw new InvalidOperationException("Expected five exact request-owned load hooks.");
            }

            private void Patch(Type type, string name, Type returnType, string prefix, string postfix)
            {
                MethodInfo method = type.GetMethod(name, BindingFlags.Instance | BindingFlags.Public,
                    null, Type.EmptyTypes, null);
                if (method == null || method.ReturnType != returnType)
                    throw new MissingMethodException(type.FullName, name);
                _methods.Add(method); // Also remove a partially installed patch on error.
                _harmony.Patch(method, Callback(prefix), Callback(postfix), null);
            }

            private static HarmonyMethod Callback(string name)
            {
                return name == null ? null : new HarmonyMethod(typeof(TeleportationLoadDiagnostics)
                    .GetMethod(name, BindingFlags.Static | BindingFlags.NonPublic));
            }

            private static void Observe(Action<TeleportationLoadDiagnostics> action)
            {
                TeleportationLoadDiagnostics observer = _active;
                if (observer == null) return;
                try { action(observer); }
                catch (Exception exception) { observer.Fault(exception); }
            }

            private static void LoaderPrefix() { Observe(value => value.LoaderStarting()); }
            private static void PlayerPrefix(Player __instance)
            {
                Observe(value => { value._loadingPlayer = __instance; value.Capture("player-prefix", __instance.CrossSceneState); });
            }
            private static void PlayerPostfix(Player __instance)
            {
                Observe(value => { value.Capture("player-postfix", __instance.CrossSceneState); value._playerPostfixes++; });
            }
            private static void ScenePrefix(SceneEntitiesState __instance)
            { Observe(value => value.CaptureScene("cross-scene-prefix", __instance)); }
            private static void ScenePostfix(SceneEntitiesState __instance)
            { Observe(value => value.CaptureScene("cross-scene-postfix", __instance)); }

            private void LoaderStarting()
            {
                _loaderStarts++;
                if (_settings != null) throw new InvalidOperationException("More than one native loader started.");
                // The native LoadRoutine has already used these settings for its header.
                // Subscribe immediately before the two native deserialization tasks start.
                _settings = DefaultJsonSettings.DefaultSettings;
                _resolver = _settings.ContractResolver;
                _converters = _settings.Converters.ToArray();
                _settings.Error += OnJsonError;
            }

            private void OnUnityLog(string message, string trace, UnityEngine.LogType type)
            {
                if (type != UnityEngine.LogType.Error && type != UnityEngine.LogType.Exception &&
                    type != UnityEngine.LogType.Assert) return;
                lock (_sync)
                {
                    if (_sealed) return;
                    _unityErrorCount++;
                    bool presentation = ExpandedSummoningNativeProfilePolicy.WorkingUiPresentationDiagnostic(
                        type == UnityEngine.LogType.Error, message, trace);
                    if (!presentation) _blockingUnityErrorCount++;
                    if (_unityErrors.Count >= 64) return;
                    _unityErrors.Add(new { sequence = _unityErrorCount, presentationDiagnostic = presentation,
                        utc = DateTime.UtcNow.ToString("o"), thread = Thread.CurrentThread.ManagedThreadId,
                        type = type.ToString(), message = message == null || message.Length <= 4096 ?
                            message : message.Substring(0, 4096),
                        trace = trace == null || trace.Length <= 16384 ? trace : trace.Substring(0, 16384) });
                }
            }

            private void OnJsonError(object sender, Newtonsoft.Json.Serialization.ErrorEventArgs args)
            {
                try
                {
                    lock (_sync)
                    {
                        if (_sealed) return;
                        _errorCount++;
                        if (_jsonErrors.Count >= 64) return;
                        ErrorContext context = args.ErrorContext;
                        _jsonErrors.Add(new { sequence = _errorCount,
                            thread = Thread.CurrentThread.ManagedThreadId,
                            path = Limit(context.Path), member = context.Member as string,
                            originalType = context.OriginalObject == null ? null : context.OriginalObject.GetType().AssemblyQualifiedName,
                            currentType = args.CurrentObject == null ? null : args.CurrentObject.GetType().AssemblyQualifiedName,
                            errorType = context.Error.GetType().FullName, message = Limit(context.Error.Message),
                            handledOnObservation = context.Handled });
                    }
                }
                catch (Exception exception) { Fault(exception); }
            }

            private void CaptureScene(string step, SceneEntitiesState state)
            {
                if (_loadingPlayer != null && ReferenceEquals(state, _loadingPlayer.CrossSceneState)) Capture(step, state);
            }

            private void Capture(string step, SceneEntitiesState state)
            {
                string mainId = _loadingPlayer.MainCharacter == null ? null : _loadingPlayer.MainCharacter.UniqueId;
                EntityDataBase[] entities = state == null ? new EntityDataBase[0] : state.AllEntityData.ToArray();
                int matching = entities.Count(value => value != null && value.UniqueId == mainId);
                var snapshot = new { step, thread = Thread.CurrentThread.ManagedThreadId, mainId,
                    crossScenePresent = state != null, sceneName = state == null ? null : state.SceneName,
                    entityCount = entities.Length, mainMatches = matching,
                    entities = entities.Take(32).Select(value => new { id = value == null ? null : value.UniqueId,
                        type = value == null ? null : value.GetType().AssemblyQualifiedName,
                        blueprint = value is UnitEntityData && ((UnitEntityData)value).Blueprint != null ?
                            ((UnitEntityData)value).Blueprint.AssetGuid : null }).ToArray() };
                lock (_sync)
                {
                    if (_sealed) return;
                    _mainMatches.Add(matching);
                    if (_states.Count < 16) _states.Add(snapshot);
                }
            }

            private void Fault(Exception exception)
            { lock (_sync) { if (!_sealed && _faults.Count < 16) _faults.Add(Limit(exception.ToString())); } }
            private static string Limit(string text)
            { return text == null || text.Length <= 2048 ? text : text.Substring(0, 2048); }

            private int HookCount()
            {
                var patched = new HashSet<MethodBase>(_harmony.GetPatchedMethods());
                return _methods.Where(method => patched.Contains(method)).Sum(method => {
                    Patches info = _harmony.GetPatchInfo(method);
                    return info == null ? 0 : info.Prefixes.Concat(info.Postfixes).Concat(info.Transpilers)
                        .Count(patch => patch.owner == _owner);
                });
            }

            internal bool Stop()
            {
                if (_sealed) return _stopSucceeded;
                if (ReferenceEquals(_active, this)) _active = null;
                if (_unityListenerAttached)
                {
                    UnityEngine.Application.logMessageReceived -= OnUnityLog;
                    _unityListenerRemoved = true;
                }
                // If this fixed diagnostic fails before the loader starts,
                // no JSON listener or settings change was installed.
                if (_captureUnityErrors && _settings == null && _loaderStarts == 0)
                { _listenerRemoved = true; _settingsRetained = true; }
                if (_settings != null)
                {
                    _settings.Error -= OnJsonError;
                    _listenerRemoved = true;
                    _settingsRetained = ReferenceEquals(_settings, DefaultJsonSettings.DefaultSettings) &&
                        ReferenceEquals(_resolver, _settings.ContractResolver) && _converters.SequenceEqual(_settings.Converters);
                }
                foreach (MethodInfo method in _methods)
                {
                    try { _harmony.Unpatch(method, HarmonyPatchType.All, _owner); }
                    catch (Exception exception) { Fault(exception); }
                }
                _remainingHooks = HookCount();
                lock (_sync)
                {
                    _stopSucceeded = _remainingHooks == 0 && _listenerRemoved && _settingsRetained && _faults.Count == 0 &&
                        (!_captureUnityErrors || _unityListenerAttached && _unityListenerRemoved);
                    _stoppedUtc = DateTime.UtcNow.ToString("o");
                    _sealed = true;
                    return _stopSucceeded;
                }
            }

            internal bool LoadCompleted
            {
                get { lock (_sync) return _loaderStarts == 1 && _playerPostfixes == 1 &&
                    _mainMatches.Count == 4 && _mainMatches.All(value => value == 1) && _errorCount == 0 && _faults.Count == 0 &&
                    (!_captureUnityErrors || _blockingUnityErrorCount == 0 && _unityErrorCount <= 64); }
            }

            internal object Snapshot()
            {
                lock (_sync) return new { schemaVersion = 1, runId = _runId,
                    stoppedUtc = _stoppedUtc, stopSucceeded = _stopSucceeded,
                    claims = "Read-only native save deserialization and cross-scene main-character observations. No save-format changes or campaign persistence claim.",
                    loaderStarts = _loaderStarts, playerPostfixes = _playerPostfixes,
                    captureUnityErrors = _captureUnityErrors, unityListenerAttached = _unityListenerAttached,
                    unityListenerRemoved = _unityListenerRemoved, unityErrors = _unityErrors.ToArray(),
                    unityErrorCount = _unityErrorCount, blockingUnityErrorCount = _blockingUnityErrorCount,
                    unityErrorOverflow = _unityErrorCount > 64,
                    installedHooks = _installedHooks, remainingHooks = _remainingHooks,
                    listenerRemoved = _listenerRemoved, nativeSettingsRetained = _settingsRetained,
                    resolver = _resolver == null ? null : _resolver.GetType().AssemblyQualifiedName,
                    converters = _converters == null ? new string[0] : _converters.Select(value => value.GetType().AssemblyQualifiedName).ToArray(),
                    loadCompleted = LoadCompleted, errorCount = _errorCount, errors = _jsonErrors.ToArray(),
                    states = _states.ToArray(), observerFaults = _faults.ToArray() };
            }
        }
    }
}
