using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using Harmony12;
using KingmakerGunslinger.Bootstrap;

namespace KingmakerGunslinger.RuntimeTesting
{
    /// <summary>
    /// Request-scoped, read-only observation of a human-initiated save load.
    /// It never invokes a load or save operation and records only allowlisted fields.
    /// </summary>
    internal sealed class ManualSaveLoadObservation
    {
        internal const string WorkingSave = "KMG_AUTOMATION_WORKING";
        internal const string BaselineSave = "KMG_AUTOMATION_BASELINE";

        private static readonly string[] LoadMethods =
        {
            "Kingmaker.Game.LoadGameFromMainMenu",
            "Kingmaker.Game.LoadGameForSmokeTest",
            "Kingmaker.Game.LoadGame",
            "Kingmaker.MainMenu.LoadGame",
            "Kingmaker.EntitySystem.Persistence.SaveManager.LoadRoutine",
            "Kingmaker.EntitySystem.Persistence.SaveManager.AddCallbackAfterLoad"
        };
        private static readonly string[] WriteMethodPrefixes =
        {
            "Save", "AutoSave", "QuickSave", "DeleteSave", "RemoveSave",
            "RenameSave", "MigrateSave", "Overwrite"
        };
        private static ManualSaveLoadObservation _active;

        private readonly ModContext _context;
        private readonly Stopwatch _elapsed;
        private readonly int _gameThreadManagedId;
        private readonly List<MethodBase> _patched = new List<MethodBase>();
        private readonly List<SaveLoadObservationEvent> _events =
            new List<SaveLoadObservationEvent>();
        private string _acceptedName;
        private string _descriptorType;
        private string _saveManagerType;
        private string _lastFingerprint;
        private string _initialGameState;
        private string _loadStartUtc;
        private string _loadCompletionUtc;
        private int _stableSamples;
        private bool _completionCallback;
        private bool _completionCallbackRegistered;
        private bool _writeObserved;
        private bool _identityRejected;
        private bool _identityAmbiguous;
        private bool _removed;
        private bool _sealed;
        private bool _wrongThreadObserved;

        internal ManualSaveLoadObservation(ModContext context, Stopwatch elapsed)
        {
            _context = context;
            _elapsed = elapsed;
            _gameThreadManagedId = Thread.CurrentThread.ManagedThreadId;
        }

        internal bool IdentityRejected { get { return _identityRejected; } }
        internal bool IdentityAmbiguous { get { return _identityAmbiguous; } }
        internal bool WriteObserved { get { return _writeObserved; } }
        internal bool CompletionObserved { get { return _completionCallback; } }
        internal bool IsReadyToComplete
        {
            get
            {
                return !_sealed && _acceptedName == WorkingSave &&
                    _completionCallback && _stableSamples >= 2 && !_writeObserved;
            }
        }

        internal void Install()
        {
            if (_active != null) throw new InvalidOperationException(
                "A save-load observation is already active.");
            _active = this;
            Assembly gameAssembly = typeof(Kingmaker.Game).Assembly;
            MethodInfo prefix = typeof(ManualSaveLoadObservation).GetMethod(
                "ObservePrefix", BindingFlags.Static | BindingFlags.NonPublic);
            MethodInfo postfix = typeof(ManualSaveLoadObservation).GetMethod(
                "ObservePostfix", BindingFlags.Static | BindingFlags.NonPublic);
            foreach (Type type in gameAssembly.GetTypes())
            {
                string typeName = type.FullName ?? string.Empty;
                bool candidateType = typeName == "Kingmaker.Game" ||
                    typeName == "Kingmaker.MainMenu" ||
                    typeName == "Kingmaker.EntitySystem.Persistence.SaveManager";
                if (!candidateType) continue;
                foreach (MethodInfo method in type.GetMethods(
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                {
                    string qualified = typeName + "." + method.Name;
                    bool load = LoadMethods.Contains(qualified);
                    bool write = typeName.EndsWith(".SaveManager", StringComparison.Ordinal) &&
                        WriteMethodPrefixes.Any(value =>
                            method.Name.StartsWith(value, StringComparison.Ordinal));
                    if (!load && !write) continue;
                    _context.Harmony.Patch(
                        method,
                        new HarmonyMethod(prefix),
                        new HarmonyMethod(postfix),
                        null);
                    _patched.Add(method);
                }
            }
            AddEvent("observer-installed", null, null, "patchCount=" + _patched.Count);
            if (_patched.Count == 0)
                throw new MissingMethodException("No allowlisted save lifecycle methods were found.");
            _initialGameState = ReadFingerprint(Kingmaker.Game.Instance);
        }

        internal void PollLoadedState()
        {
            if (_sealed) return;
            if (Thread.CurrentThread.ManagedThreadId != _gameThreadManagedId)
            {
                _wrongThreadObserved = true;
                _identityAmbiguous = true;
                return;
            }
            object game = Kingmaker.Game.Instance;
            if (game == null) return;
            if (!_completionCallbackRegistered)
            {
                object manager = ReadProperty(game, "SaveManager");
                if (manager == null) return;
                MethodInfo callbackMethod = manager.GetType().GetMethod(
                    "AddCallbackAfterLoad",
                    BindingFlags.Instance | BindingFlags.Public,
                    null,
                    new[] { typeof(Action) },
                    null);
                if (callbackMethod == null)
                {
                    _identityAmbiguous = true;
                    return;
                }
                _completionCallbackRegistered = true;
                callbackMethod.Invoke(manager, new object[] { new Action(OnLoadCompleted) });
                AddEvent("completion-callback-registered", callbackMethod, null,
                    "read-only callback registration");
            }
            if (_acceptedName != WorkingSave) return;
            if (!_completionCallback) return;
            string fingerprint = ReadFingerprint(game);
            if (string.IsNullOrWhiteSpace(fingerprint)) return;
            if (string.Equals(fingerprint, _lastFingerprint, StringComparison.Ordinal))
                _stableSamples++;
            else
            {
                _lastFingerprint = fingerprint;
                _stableSamples = 1;
            }
            if (_stableSamples == 2)
                AddEvent("stable-loaded-state", null, null, fingerprint);
        }

        internal SaveLoadObservationEvidence Stop()
        {
            _sealed = true;
            foreach (MethodBase method in _patched)
                _context.Harmony.Unpatch(method, HarmonyPatchType.All, _context.ModId);
            _patched.Clear();
            _removed = true;
            if (ReferenceEquals(_active, this)) _active = null;
            AddEvent("observer-removed", null, null, "all request-scoped patches removed");
            return Snapshot();
        }

        internal SaveLoadObservationEvidence Snapshot()
        {
            return new SaveLoadObservationEvidence
            {
                SaveManagerType = _saveManagerType ?? string.Empty,
                SaveDescriptorType = _descriptorType ?? string.Empty,
                AcceptedSaveName = _acceptedName ?? string.Empty,
                CompletionCallbackObserved = _completionCallback,
                GameLoadedStateObserved = _stableSamples >= 2,
                StableFingerprint = _stableSamples >= 2 ? _lastFingerprint : string.Empty,
                SaveWritingApiObserved = _writeObserved,
                ObservationPatchesRemoved = _removed,
                LoadStartUtc = _loadStartUtc ?? string.Empty,
                LoadCompletionUtc = _loadCompletionUtc ?? string.Empty,
                InitialGameState = _initialGameState ?? string.Empty,
                StableGameState = _stableSamples >= 2 ? _lastFingerprint : string.Empty,
                GameThreadManagedId = _gameThreadManagedId,
                AllCallbacksOnGameThread = !_wrongThreadObserved,
                Events = new List<SaveLoadObservationEvent>(_events)
            };
        }

        private static void ObservePrefix(MethodBase __originalMethod, object[] __args)
        {
            ManualSaveLoadObservation active = _active;
            if (active == null || active._sealed) return;
            active.Observe(__originalMethod, __args, "method-enter");
        }

        private static void ObservePostfix(MethodBase __originalMethod, object[] __args)
        {
            ManualSaveLoadObservation active = _active;
            if (active == null || active._sealed) return;
            active.Observe(__originalMethod, __args, "method-exit");
        }

        private void Observe(MethodBase method, object[] arguments, string phase)
        {
            if (Thread.CurrentThread.ManagedThreadId != _gameThreadManagedId)
            {
                _wrongThreadObserved = true;
                _identityAmbiguous = true;
            }
            string declaring = method.DeclaringType == null
                ? string.Empty : method.DeclaringType.FullName;
            if (declaring.EndsWith(".SaveManager", StringComparison.Ordinal))
                _saveManagerType = declaring;
            bool write = WriteMethodPrefixes.Any(value =>
                method.Name.StartsWith(value, StringComparison.Ordinal)) &&
                method.Name != "SaveList" && method.Name != "SaveInfo";
            if (write) _writeObserved = true;

            object descriptor = FindSaveDescriptor(arguments);
            string displayName = descriptor == null ? string.Empty :
                ReadSafeString(descriptor, "Name");
            string gameName = descriptor == null ? string.Empty :
                ReadSafeString(descriptor, "GameName");
            string gameId = descriptor == null ? string.Empty :
                ReadSafeString(descriptor, "GameId");
            string fileLeaf = descriptor == null ? string.Empty :
                SafeLeaf(ReadSafeString(descriptor, "FileName"));
            string safeId = FirstNonEmpty(gameName, gameId, fileLeaf);
            if (descriptor != null)
            {
                _descriptorType = descriptor.GetType().FullName;
                if (phase == "method-enter" && method.Name != "AddCallbackAfterLoad")
                {
                    if (_loadStartUtc == null)
                        _loadStartUtc = DateTime.UtcNow.ToString("o");
                    AcceptIdentity(displayName, gameName, gameId, fileLeaf);
                }
            }
            AddEvent(phase, method, arguments,
                (write ? "forbidden-save-write-observed;" : string.Empty) +
                    FormatSafePrimitiveArguments(arguments),
                displayName, safeId);
        }

        private void AcceptIdentity(params string[] identities)
        {
            if (identities.Any(identity =>
                string.Equals(identity, BaselineSave, StringComparison.Ordinal)))
            {
                _identityRejected = true;
                return;
            }
            if (_acceptedName != null || _identityRejected || _identityAmbiguous) return;
            if (identities.Any(identity =>
                string.Equals(identity, WorkingSave, StringComparison.Ordinal)))
                _acceptedName = WorkingSave;
            else if (identities.All(string.IsNullOrWhiteSpace))
                _identityAmbiguous = true;
            else
                _identityRejected = true;
        }

        private void OnLoadCompleted()
        {
            if (_sealed) return;
            _completionCallback = true;
            _loadCompletionUtc = DateTime.UtcNow.ToString("o");
            AddEvent("load-completion-callback", null, null,
                "SaveManager after-load callback invoked");
        }

        private void AddEvent(
            string kind,
            MethodBase method,
            object[] arguments,
            string detail,
            string displayName = "",
            string safeId = "")
        {
            _events.Add(new SaveLoadObservationEvent
            {
                Sequence = _events.Count + 1,
                ElapsedMilliseconds = _elapsed.ElapsedMilliseconds,
                Utc = DateTime.UtcNow.ToString("o"),
                Kind = kind,
                DeclaringType = method == null || method.DeclaringType == null
                    ? string.Empty : method.DeclaringType.FullName,
                MethodSignature = method == null ? string.Empty : FormatSignature(method),
                ArgumentTypes = arguments == null
                    ? new List<string>()
                    : arguments.Select(value => value == null
                        ? "null" : value.GetType().FullName).ToList(),
                ManagedThreadId = Thread.CurrentThread.ManagedThreadId,
                DisplayName = displayName ?? string.Empty,
                SafeSaveIdentifier = safeId ?? string.Empty,
                Detail = detail ?? string.Empty
            });
        }

        private static object FindSaveDescriptor(object[] arguments)
        {
            if (arguments == null) return null;
            return arguments.FirstOrDefault(value => value != null &&
                string.Equals(value.GetType().FullName,
                    "Kingmaker.EntitySystem.Persistence.SaveInfo",
                    StringComparison.Ordinal));
        }

        private static string ReadFingerprint(object game)
        {
            object player = ReadProperty(game, "Player");
            object area = ReadProperty(game, "CurrentlyLoadedArea");
            object scene = ReadProperty(game, "CurrentScene");
            if (player == null || area == null || scene == null) return string.Empty;
            object party = ReadProperty(player, "Party");
            object main = ReadProperty(player, "MainCharacter");
            string gameId = Convert.ToString(ReadProperty(player, "GameId")) ?? string.Empty;
            int partyCount = ReadCount(party);
            string mainType = main == null ? string.Empty : main.GetType().FullName;
            return "areaType=" + area.GetType().FullName +
                ";sceneType=" + scene.GetType().FullName +
                ";gameId=" + gameId +
                ";partyCount=" + partyCount +
                ";mainCharacterType=" + mainType;
        }

        private static int ReadCount(object value)
        {
            object count = ReadProperty(value, "Count");
            int parsed;
            return int.TryParse(Convert.ToString(count), out parsed) ? parsed : -1;
        }

        private static object ReadProperty(object value, string name)
        {
            if (value == null) return null;
            PropertyInfo property = value.GetType().GetProperty(
                name, BindingFlags.Instance | BindingFlags.Public);
            return property == null || property.GetIndexParameters().Length != 0
                ? null : property.GetValue(value, null);
        }

        private static string ReadSafeString(object value, string name)
        {
            object field = ReadProperty(value, name);
            return field == null ? string.Empty : Convert.ToString(field);
        }

        private static string SafeLeaf(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return string.Empty;
            try { return System.IO.Path.GetFileName(path); }
            catch { return string.Empty; }
        }

        private static string FirstNonEmpty(params string[] values)
        {
            return values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value)) ??
                string.Empty;
        }

        private static string FormatSafePrimitiveArguments(object[] arguments)
        {
            if (arguments == null) return string.Empty;
            var values = new List<string>();
            for (int index = 0; index < arguments.Length; index++)
            {
                object value = arguments[index];
                if (value is bool)
                    values.Add("arg" + index + ".Boolean=" + value);
            }
            return string.Join(";", values.ToArray());
        }

        private static string FormatSignature(MethodBase method)
        {
            return method.Name + "(" + string.Join(",",
                method.GetParameters().Select(value =>
                    value.ParameterType.FullName).ToArray()) + ")" +
                (method is MethodInfo
                    ? ":" + ((MethodInfo)method).ReturnType.FullName : string.Empty);
        }
    }
}
