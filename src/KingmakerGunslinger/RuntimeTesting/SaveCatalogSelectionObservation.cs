using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using Harmony12;
using KingmakerGunslinger.Bootstrap;

namespace KingmakerGunslinger.RuntimeTesting
{
    /// <summary>
    /// Request-scoped observation of the normal save catalog and a human selection.
    /// It never invokes catalog refresh, selection, load, or save APIs.
    /// </summary>
    internal sealed class SaveCatalogSelectionObservation
    {
        internal const string WorkingSave = "KMG_AUTOMATION_WORKING";
        internal const string BaselineSave = "KMG_AUTOMATION_BASELINE";
        private static readonly string[] CatalogMethods =
            { "UpdateSaveListAsync", "UpdateSaveListIfNeeded", "UpdateSaveListTask" };
        private static readonly string[] SafeFields =
            { "Name", "FolderName", "FileName", "GameName", "GameId", "Area",
              "AreaNameOverride", "GameSaveTime", "GameTotalTime" };
        private static readonly string[] WritePrefixes =
            { "Save", "AutoSave", "QuickSave", "DeleteSave", "RemoveSave",
              "RenameSave", "MigrateSave", "Overwrite" };
        private static SaveCatalogSelectionObservation _active;

        private readonly ModContext _context;
        private readonly Stopwatch _elapsed;
        private readonly string _runId;
        private readonly Action<SaveLoadObservationEvent> _sink;
        private readonly int _gameThreadId;
        private readonly List<MethodBase> _patched = new List<MethodBase>();
        private readonly List<SaveLoadObservationEvent> _events =
            new List<SaveLoadObservationEvent>();
        private readonly List<object> _catalogObjects = new List<object>();
        private readonly List<SaveCatalogDescriptorEvidence> _descriptors =
            new List<SaveCatalogDescriptorEvidence>();
        private bool _callbackRegistered;
        private bool _catalogCaptured;
        private bool _catalogComplete;
        private bool _completion;
        private bool _writeObserved;
        private bool _wrongThread;
        private bool _removed;
        private object _selected;
        private string _selectedClass = "";
        private string _correlation = "";
        private string _fingerprint = "";
        private int _stableSamples;
        private int _workingCount;
        private int _baselineCount;
        private string _managerType = "";
        private string _collectionType = "";
        private string _descriptorType = "";
        private Exception _exception;
        private bool _managerCatalogHookInstalled;
        private bool _uiCatalogHookInstalled;
        private bool _selectionHookInstalled;
        private bool _callbackHookInstalled;
        private bool _writeHookInstalled;

        internal SaveCatalogSelectionObservation(ModContext context, Stopwatch elapsed,
            string runId, Action<SaveLoadObservationEvent> sink)
        {
            _context = context;
            _elapsed = elapsed;
            _runId = runId;
            _sink = sink;
            _gameThreadId = Thread.CurrentThread.ManagedThreadId;
        }

        internal bool Ready
        {
            get
            {
                return _callbackRegistered && _managerCatalogHookInstalled &&
                    _uiCatalogHookInstalled &&
                    _selectionHookInstalled && _callbackHookInstalled &&
                    _writeHookInstalled;
            }
        }
        internal bool CatalogCaptured { get { return _catalogCaptured; } }
        internal bool CatalogComplete { get { return _catalogComplete; } }
        internal int WorkingCount { get { return _workingCount; } }
        internal int BaselineCount { get { return _baselineCount; } }
        internal bool SelectionObserved { get { return _selected != null; } }
        internal bool CompletionObserved { get { return _completion; } }
        internal bool StableFingerprintAvailable { get { return _stableSamples >= 2; } }
        internal bool WriteObserved { get { return _writeObserved; } }
        internal bool WrongThread { get { return _wrongThread; } }
        internal string SelectedClassification { get { return _selectedClass; } }
        internal string CorrelationMethod { get { return _correlation; } }
        internal Exception ObservationException { get { return _exception; } }
        internal List<string> HookIdentifiers
        {
            get { return _patched.Select(FormatSignature).OrderBy(x => x).ToList(); }
        }

        internal void Install()
        {
            if (_active != null) throw new InvalidOperationException("Catalog observer active.");
            _active = this;
            Assembly assembly = typeof(Kingmaker.Game).Assembly;
            MethodInfo prefix = typeof(SaveCatalogSelectionObservation).GetMethod(
                "Prefix", BindingFlags.Static | BindingFlags.NonPublic);
            MethodInfo postfix = typeof(SaveCatalogSelectionObservation).GetMethod(
                "Postfix", BindingFlags.Static | BindingFlags.NonPublic);
            foreach (Type type in assembly.GetTypes())
            {
                string name = type.FullName ?? "";
                if (name != "Kingmaker.EntitySystem.Persistence.SaveManager" &&
                    name != "Kingmaker.MainMenu" &&
                    name != "Kingmaker.UI.SaveLoadWindow.ListOfSaves") continue;
                foreach (MethodInfo method in type.GetMethods(
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                {
                    bool catalog = name.EndsWith(".SaveManager", StringComparison.Ordinal) &&
                        CatalogMethods.Contains(method.Name);
                    bool uiCatalog = name ==
                        "Kingmaker.UI.SaveLoadWindow.ListOfSaves" &&
                        method.Name == "Initialize" &&
                        method.GetParameters().Length == 2 &&
                        method.GetParameters()[0].ParameterType.FullName.StartsWith(
                            "System.Collections.Generic.List`1[[Kingmaker.EntitySystem.Persistence.SaveInfo,",
                            StringComparison.Ordinal) &&
                        method.GetParameters()[1].ParameterType == typeof(bool);
                    bool selection = name == "Kingmaker.MainMenu" && method.Name == "LoadGame" &&
                        method.GetParameters().Length == 1 &&
                        method.GetParameters()[0].ParameterType.FullName ==
                            "Kingmaker.EntitySystem.Persistence.SaveInfo";
                    bool callback = name.EndsWith(".SaveManager", StringComparison.Ordinal) &&
                        method.Name == "AddCallbackAfterLoad";
                    bool write = name.EndsWith(".SaveManager", StringComparison.Ordinal) &&
                        WritePrefixes.Any(p => method.Name.StartsWith(p, StringComparison.Ordinal));
                    if (!catalog && !uiCatalog && !selection && !callback && !write)
                        continue;
                    _context.Harmony.Patch(method, new HarmonyMethod(prefix),
                        new HarmonyMethod(postfix), null);
                    _patched.Add(method);
                    if (catalog) _managerCatalogHookInstalled = true;
                    if (uiCatalog) _uiCatalogHookInstalled = true;
                    if (selection) _selectionHookInstalled = true;
                    if (callback) _callbackHookInstalled = true;
                    if (write) _writeHookInstalled = true;
                }
            }
            object game = Kingmaker.Game.Instance;
            object manager = ReadMember(game, "SaveManager");
            if (manager == null) throw new InvalidOperationException("SaveManager unavailable.");
            MethodInfo add = manager.GetType().GetMethod("AddCallbackAfterLoad",
                BindingFlags.Instance | BindingFlags.Public, null,
                new[] { typeof(Action) }, null);
            if (add == null) throw new MissingMethodException("AddCallbackAfterLoad(Action)");
            add.Invoke(manager, new object[] { new Action(OnCompleted) });
            _callbackRegistered = true;
            Add("hooks-installed", null, null, "patchCount=" + _patched.Count);
        }

        internal void Poll()
        {
            CheckThread();
            if (!_completion) return;
            string current = ReadFingerprint(Kingmaker.Game.Instance);
            if (string.IsNullOrWhiteSpace(current)) return;
            if (current == _fingerprint) _stableSamples++;
            else { _fingerprint = current; _stableSamples = 1; }
            if (_stableSamples == 2) Add("stable-loaded-state", null, null, current);
        }

        internal SaveCatalogObservationEvidence Stop()
        {
            foreach (MethodBase method in _patched)
                _context.Harmony.Unpatch(method, HarmonyPatchType.All, _context.ModId);
            _patched.Clear();
            _removed = true;
            if (ReferenceEquals(_active, this)) _active = null;
            Add("observer-removed", null, null, "all request-scoped patches removed");
            return Snapshot();
        }

        internal SaveCatalogObservationEvidence Snapshot()
        {
            return new SaveCatalogObservationEvidence
            {
                CatalogManagerType = _managerType, CollectionType = _collectionType,
                DescriptorType = _descriptorType, DescriptorCount = _catalogObjects.Count,
                WorkingMatchCount = _workingCount, BaselineMatchCount = _baselineCount,
                CatalogComplete = _catalogComplete,
                SelectedCorrelates = !string.IsNullOrWhiteSpace(_correlation),
                CorrelationMethod = _correlation, SelectedClassification = _selectedClass,
                CompletionObserved = _completion,
                StableFingerprint = _stableSamples >= 2 ? _fingerprint : "",
                SaveWritingApiObserved = _writeObserved, HooksRemoved = _removed,
                AllCallbacksOnGameThread = !_wrongThread,
                Descriptors = new List<SaveCatalogDescriptorEvidence>(_descriptors),
                Events = new List<SaveLoadObservationEvent>(_events),
                ProbeInitiatedSaveWriting = false
            };
        }

        private static void Prefix(MethodBase __originalMethod, object __instance, object[] __args)
        {
            if (_active != null) _active.Observe(__originalMethod, __instance, __args, true);
        }

        private static void Postfix(MethodBase __originalMethod, object __instance, object[] __args)
        {
            if (_active != null) _active.Observe(__originalMethod, __instance, __args, false);
        }

        private void Observe(MethodBase method, object instance, object[] args, bool entering)
        {
            CheckThread();
            string phase = entering ? "method-enter" : "method-exit";
            bool write = method.DeclaringType.FullName.EndsWith(".SaveManager",
                StringComparison.Ordinal) && WritePrefixes.Any(p =>
                    method.Name.StartsWith(p, StringComparison.Ordinal)) &&
                method.Name != "SaveList" && method.Name != "SaveInfo";
            if (write) _writeObserved = true;
            if (method.DeclaringType.FullName == "Kingmaker.MainMenu" &&
                method.Name == "LoadGame" && entering)
                ObserveSelection(args == null ? null : args.FirstOrDefault());
            Add(phase, method, args, write ? "save-writing-api-observed" : "");
            if (!entering && method.DeclaringType.FullName ==
                "Kingmaker.UI.SaveLoadWindow.ListOfSaves" &&
                method.Name == "Initialize")
                CaptureCatalog(args == null ? null : args.FirstOrDefault(), method);
        }

        private void CaptureCatalog(object collection, MethodBase source)
        {
            var enumerable = collection as IEnumerable;
            if (collection == null || enumerable == null)
            {
                _catalogComplete = false;
                Add("catalog-ambiguous", source, null,
                    "displayed SaveInfo collection unavailable");
                return;
            }
            _managerType = source.DeclaringType.FullName;
            _collectionType = collection.GetType().FullName;
            _catalogObjects.Clear(); _descriptors.Clear();
            _workingCount = 0; _baselineCount = 0;
            foreach (object descriptor in enumerable)
            {
                if (descriptor == null) continue;
                _descriptorType = descriptor.GetType().FullName;
                string name = ReadString(descriptor, "Name");
                string classification = name == WorkingSave ? "working" :
                    name == BaselineSave ? "baseline" : "unrelated";
                if (classification == "working") _workingCount++;
                if (classification == "baseline") _baselineCount++;
                string identity = BuildIdentity(descriptor);
                _catalogObjects.Add(descriptor);
                _descriptors.Add(new SaveCatalogDescriptorEvidence
                {
                    Classification = classification,
                    DisplayName = classification == "unrelated" ? "" : name,
                    IdentityHash = Hash(identity),
                    SafeFields = classification == "unrelated"
                        ? new Dictionary<string, string>()
                        : ReadSafeFields(descriptor)
                });
            }
            _catalogCaptured = true;
            _catalogComplete = true;
            Add("catalog-captured", source, null, "descriptorCount=" +
                _catalogObjects.Count + ";working=" + _workingCount +
                ";baseline=" + _baselineCount);
        }

        private void ObserveSelection(object descriptor)
        {
            _selected = descriptor;
            string name = ReadString(descriptor, "Name");
            _selectedClass = name == WorkingSave ? "working" :
                name == BaselineSave ? "baseline" : "other";
            int referenceMatches = _catalogObjects.Count(x => ReferenceEquals(x, descriptor));
            if (referenceMatches == 1) _correlation = "object-reference";
            else
            {
                string hash = Hash(BuildIdentity(descriptor));
                int hashMatches = _descriptors.Count(x => x.IdentityHash == hash);
                if (hashMatches == 1) _correlation = "safe-field-identity-hash";
            }
            Add("descriptor-selected", null, new[] { descriptor },
                "classification=" + _selectedClass + ";correlation=" + _correlation);
        }

        private void OnCompleted()
        {
            CheckThread();
            _completion = true;
            Add("load-completion-callback", null, null,
                "SaveManager after-load callback invoked");
        }

        private void CheckThread()
        {
            if (Thread.CurrentThread.ManagedThreadId != _gameThreadId)
                _wrongThread = true;
        }

        private void Add(string kind, MethodBase method, object[] args, string detail)
        {
            var value = new SaveLoadObservationEvent
            {
                RunId = _runId, Sequence = _events.Count + 1,
                ElapsedMilliseconds = _elapsed.ElapsedMilliseconds,
                Utc = DateTime.UtcNow.ToString("o"), Kind = kind,
                DeclaringType = method == null ? "" : method.DeclaringType.FullName,
                MethodSignature = method == null ? "" : FormatSignature(method),
                ArgumentTypes = args == null ? new List<string>() :
                    args.Select(x => x == null ? "null" : x.GetType().FullName).ToList(),
                ManagedThreadId = Thread.CurrentThread.ManagedThreadId,
                DisplayName = "", SafeSaveIdentifier = "", Detail = detail ?? "",
                Exception = ""
            };
            _events.Add(value);
            try { if (_sink != null) _sink(value); }
            catch (Exception ex) { _exception = ex; }
        }

        private static Dictionary<string, string> ReadSafeFields(object descriptor)
        {
            var fields = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (string name in SafeFields)
            {
                string value = ReadString(descriptor, name);
                if (name == "FileName" || name == "FolderName")
                    value = SafeLeaf(value);
                fields[name] = value;
            }
            return fields;
        }

        private static string BuildIdentity(object descriptor)
        {
            Dictionary<string, string> fields = ReadSafeFields(descriptor);
            return string.Join("\u001f", SafeFields.Select(x => x + "=" + fields[x]).ToArray());
        }

        private static string Hash(string value)
        {
            using (SHA256 sha = SHA256.Create())
                return BitConverter.ToString(sha.ComputeHash(
                    Encoding.UTF8.GetBytes(value ?? ""))).Replace("-", "").ToLowerInvariant();
        }

        private static string ReadFingerprint(object game)
        {
            object player = ReadMember(game, "Player");
            object area = ReadMember(game, "CurrentlyLoadedArea");
            object scene = ReadMember(game, "CurrentScene");
            if (player == null || area == null || scene == null) return "";
            object party = ReadMember(player, "Party");
            object main = ReadMember(player, "MainCharacter");
            return "areaType=" + area.GetType().FullName +
                ";sceneType=" + scene.GetType().FullName +
                ";gameId=" + Convert.ToString(ReadMember(player, "GameId")) +
                ";partyCount=" + Convert.ToString(ReadMember(party, "Count")) +
                ";mainCharacterType=" + (main == null ? "" : main.GetType().FullName);
        }

        private static object ReadMember(object value, string name)
        {
            if (value == null) return null;
            PropertyInfo property = value.GetType().GetProperty(name,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (property != null && property.GetIndexParameters().Length == 0)
                return property.GetValue(value, null);
            FieldInfo field = value.GetType().GetField(name,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            return field == null ? null : field.GetValue(value);
        }

        private static string ReadString(object value, string name)
        {
            return Convert.ToString(ReadMember(value, name)) ?? "";
        }

        private static string SafeLeaf(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return "";
            try { return System.IO.Path.GetFileName(path); } catch { return ""; }
        }

        private static string FormatSignature(MethodBase method)
        {
            return method.Name + "(" + string.Join(",", method.GetParameters()
                .Select(x => x.ParameterType.FullName).ToArray()) + ")" +
                (method is MethodInfo ? ":" + ((MethodInfo)method).ReturnType.FullName : "");
        }
    }
}
