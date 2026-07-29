using System;
using System.Collections;
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
    /// Observes, but never invokes, the narrow managed path which supplies the
    /// complete SaveInfo list to the normal Load Game display model.
    /// </summary>
    internal sealed class SaveCatalogProviderObservation
    {
        private static readonly string[] RelevantTypeFragments =
            { "SaveManager", "SaveLoad", "ListOfSaves", "MainMenu" };
        private static readonly string[] RelevantMemberFragments =
            { "Save", "List", "Catalog", "Load", "Owner", "Controller", "Model", "View" };
        private static readonly string[] WritePrefixes =
            { "Save", "AutoSave", "QuickSave", "DeleteSave", "RemoveSave",
              "RenameSave", "MigrateSave", "Overwrite" };
        private static SaveCatalogProviderObservation _active;

        private readonly ModContext _context;
        private readonly Stopwatch _elapsed;
        private readonly string _runId;
        private readonly Action<SaveLoadObservationEvent> _sink;
        private readonly int _gameThreadId;
        private readonly List<MethodBase> _patched = new List<MethodBase>();
        private readonly List<SaveLoadObservationEvent> _events =
            new List<SaveLoadObservationEvent>();
        private readonly List<CatalogProviderCandidateEvidence> _candidates =
            new List<CatalogProviderCandidateEvidence>();
        private readonly List<CatalogOwnerMemberEvidence> _members =
            new List<CatalogOwnerMemberEvidence>();
        private readonly Dictionary<object, MethodBase> _returnedCollections =
            new Dictionary<object, MethodBase>(ReferenceEqualityComparer.Instance);
        private readonly Dictionary<object, MethodBase> _argumentCollections =
            new Dictionary<object, MethodBase>(ReferenceEqualityComparer.Instance);
        private bool _initializeHookInstalled;
        private bool _candidateHookInstalled;
        private bool _sentinelHookInstalled;
        private bool _captured;
        private bool _sourceProven;
        private bool _wrongThread;
        private bool _writeObserved;
        private bool _loadObserved;
        private bool _removed;
        private string _initializeSignature = "";
        private string _collectionType = "";
        private string _descriptorType = "";
        private string _receiverType = "";
        private string _immediateCaller = "";
        private string _sourceKind = "";
        private int _descriptorCount;
        private List<string> _callerChain = new List<string>();
        private Exception _exception;

        internal SaveCatalogProviderObservation(ModContext context, Stopwatch elapsed,
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
            get { return _initializeHookInstalled && _candidateHookInstalled &&
                _sentinelHookInstalled; }
        }
        internal bool CatalogCaptured { get { return _captured; } }
        internal bool SourceProven { get { return _sourceProven; } }
        internal bool WriteObserved { get { return _writeObserved; } }
        internal bool LoadObserved { get { return _loadObserved; } }
        internal Exception ObservationException { get { return _exception; } }
        internal List<string> HookIdentifiers
        {
            get { return _patched.Select(FormatSignature).OrderBy(x => x).ToList(); }
        }

        internal void Install()
        {
            if (_active != null)
                throw new InvalidOperationException("Catalog provider observer active.");
            _active = this;
            MethodInfo initializePrefix = typeof(SaveCatalogProviderObservation).GetMethod(
                "InitializePrefix", BindingFlags.Static | BindingFlags.NonPublic);
            MethodInfo candidatePrefix = typeof(SaveCatalogProviderObservation).GetMethod(
                "CandidatePrefix", BindingFlags.Static | BindingFlags.NonPublic);
            MethodInfo resultPostfix = typeof(SaveCatalogProviderObservation).GetMethod(
                "ResultPostfix", BindingFlags.Static | BindingFlags.NonPublic);
            Assembly assembly = typeof(Kingmaker.Game).Assembly;
            foreach (Type type in assembly.GetTypes().Where(IsRelevantType)
                .OrderBy(x => x.FullName, StringComparer.Ordinal))
            {
                foreach (MethodInfo method in type.GetMethods(
                    BindingFlags.Instance | BindingFlags.Static |
                    BindingFlags.Public | BindingFlags.NonPublic)
                    .OrderBy(FormatSignature, StringComparer.Ordinal))
                {
                    if (IsInitialize(method))
                    {
                        Patch(method, initializePrefix, null);
                        _initializeHookInstalled = true;
                    }
                    else if (IsCandidate(method))
                    {
                        Patch(method, candidatePrefix,
                            IsSaveInfoCollection(method.ReturnType) ? resultPostfix : null);
                        _candidateHookInstalled = true;
                    }
                    else if (IsMutationSentinel(method) || IsLoadSentinel(method))
                    {
                        Patch(method, candidatePrefix, null);
                        _sentinelHookInstalled = true;
                    }
                }
            }
            Add("hooks-installed", null, null, "patchCount=" + _patched.Count);
        }

        internal SaveCatalogProviderObservationEvidence Stop()
        {
            foreach (MethodBase method in _patched)
                _context.Harmony.Unpatch(method, HarmonyPatchType.All, _context.ModId);
            _patched.Clear();
            _removed = true;
            if (ReferenceEquals(_active, this)) _active = null;
            Add("observer-removed", null, null, "all request-scoped patches removed");
            return Snapshot();
        }

        internal SaveCatalogProviderObservationEvidence Snapshot()
        {
            return new SaveCatalogProviderObservationEvidence
            {
                InitializeSignature = _initializeSignature,
                CollectionType = _collectionType,
                DescriptorType = _descriptorType,
                DescriptorCount = _descriptorCount,
                CompleteListObserved = _captured,
                ReceiverRuntimeType = _receiverType,
                ImmediateCaller = _immediateCaller,
                CallerChain = new List<string>(_callerChain),
                OwnerMembers = new List<CatalogOwnerMemberEvidence>(_members),
                ProviderCandidates = new List<CatalogProviderCandidateEvidence>(_candidates),
                SourceProven = _sourceProven,
                SourceKind = _sourceKind,
                AllCallbacksOnGameThread = !_wrongThread,
                LifecycleState = _captured
                    ? "normal Load Game display-model initialization" : "not observed",
                ProviderInvokedByProbe = false,
                SaveLoadObserved = _loadObserved,
                SaveWritingObserved = _writeObserved,
                HooksRemoved = _removed,
                Events = new List<SaveLoadObservationEvent>(_events)
            };
        }

        private void Patch(MethodInfo method, MethodInfo prefix, MethodInfo postfix)
        {
            _context.Harmony.Patch(method,
                prefix == null ? null : new HarmonyMethod(prefix),
                postfix == null ? null : new HarmonyMethod(postfix), null);
            _patched.Add(method);
        }

        private static void InitializePrefix(
            MethodBase __originalMethod, object __instance, object[] __args)
        {
            if (_active != null)
                _active.CaptureInitialize(__originalMethod, __instance, __args);
        }

        private static void CandidatePrefix(MethodBase __originalMethod, object[] __args)
        {
            if (_active != null) _active.ObserveCandidate(__originalMethod, __args);
        }

        private static void ResultPostfix(MethodBase __originalMethod, object __result)
        {
            if (_active != null) _active.ObserveResult(__originalMethod, __result);
        }

        private void ObserveCandidate(MethodBase method, object[] args)
        {
            CheckThread();
            if (IsMutationSentinel(method)) _writeObserved = true;
            if (IsLoadSentinel(method)) _loadObserved = true;
            if (args != null)
            {
                foreach (object argument in args)
                {
                    if (!IsSaveInfoCollection(argument == null ? null : argument.GetType()))
                        continue;
                    _argumentCollections[argument] = method;
                    RememberCandidate(method, "callback-argument",
                        "same collection object observed as method argument");
                }
            }
            Add("candidate-enter", method, args, IsMutationSentinel(method)
                ? "save-writing-api-observed" :
                IsLoadSentinel(method) ? "save-load-api-observed" : "");
        }

        private void ObserveResult(MethodBase method, object result)
        {
            CheckThread();
            if (result != null && IsSaveInfoCollection(result.GetType()))
            {
                _returnedCollections[result] = method;
                Add("candidate-return", method, new[] { result },
                    "SaveInfo collection return observed");
            }
        }

        private void CaptureInitialize(MethodBase method, object instance, object[] args)
        {
            CheckThread();
            object collection = args == null ? null : args.FirstOrDefault();
            var enumerable = collection as IEnumerable;
            _initializeSignature = FormatSignature(method);
            _receiverType = instance == null ? "" : instance.GetType().FullName;
            _collectionType = collection == null ? "" : collection.GetType().FullName;
            _descriptorCount = 0;
            if (enumerable != null)
            {
                foreach (object descriptor in enumerable)
                {
                    if (descriptor == null) continue;
                    _descriptorCount++;
                    if (string.IsNullOrWhiteSpace(_descriptorType))
                        _descriptorType = descriptor.GetType().FullName;
                }
            }
            CaptureCallerChain();
            CaptureOwnerMetadata(instance);
            MethodBase producer;
            if (collection != null && _returnedCollections.TryGetValue(collection, out producer))
            {
                RememberCandidate(producer, "method-return",
                    "object-reference equals ListOfSaves.Initialize argument");
                _sourceKind = "method-return";
                _sourceProven = true;
            }
            else if (collection != null &&
                _argumentCollections.TryGetValue(collection, out producer))
            {
                RememberCandidate(producer, "callback-argument",
                    "object-reference equals ListOfSaves.Initialize argument");
                _sourceKind = "callback-argument";
                _sourceProven = true;
            }
            _captured = enumerable != null && _descriptorCount > 0;
            Add("catalog-provider-captured", method, args,
                "descriptorCount=" + _descriptorCount + ";sourceProven=" + _sourceProven);
        }

        private void CaptureCallerChain()
        {
            var chain = new List<string>();
            foreach (StackFrame frame in new StackTrace(1, false).GetFrames() ??
                new StackFrame[0])
            {
                MethodBase method = frame.GetMethod();
                if (method == null || method.DeclaringType == null) continue;
                string typeName = method.DeclaringType.FullName ?? "";
                if (typeName.StartsWith("KingmakerGunslinger.", StringComparison.Ordinal) ||
                    typeName.StartsWith("Harmony", StringComparison.Ordinal) ||
                    typeName.StartsWith("System.", StringComparison.Ordinal)) continue;
                if (!typeName.StartsWith("Kingmaker.", StringComparison.Ordinal)) continue;
                string signature = typeName + "." + FormatSignature(method);
                if (chain.Count == 0 || chain[chain.Count - 1] != signature)
                    chain.Add(signature);
                if (chain.Count == 12) break;
            }
            _callerChain = chain;
            _immediateCaller = chain.Count == 0 ? "" : chain[0];
        }

        private void CaptureOwnerMetadata(object receiver)
        {
            if (receiver == null) return;
            Type type = receiver.GetType();
            foreach (FieldInfo field in type.GetFields(BindingFlags.Instance |
                BindingFlags.Public | BindingFlags.NonPublic)
                .Where(x => IsRelevantMember(x.Name, x.FieldType)))
                AddMember(type, field.Name, field.FieldType, "field");
            foreach (PropertyInfo property in type.GetProperties(BindingFlags.Instance |
                BindingFlags.Public | BindingFlags.NonPublic)
                .Where(x => x.GetIndexParameters().Length == 0 &&
                    IsRelevantMember(x.Name, x.PropertyType)))
                AddMember(type, property.Name, property.PropertyType, "property-metadata-only");
        }

        private void AddMember(Type owner, string name, Type memberType, string kind)
        {
            if (_members.Any(x => x.OwnerType == owner.FullName &&
                x.MemberName == name && x.MemberKind == kind)) return;
            _members.Add(new CatalogOwnerMemberEvidence
            {
                OwnerType = owner.FullName, MemberName = name,
                MemberType = memberType.FullName, MemberKind = kind
            });
        }

        private void RememberCandidate(MethodBase method, string sourceKind, string correlation)
        {
            if (_candidates.Any(x => x.MethodSignature == FormatSignature(method) &&
                x.DeclaringType == method.DeclaringType.FullName &&
                x.SourceKind == sourceKind)) return;
            string typeName = method.DeclaringType.FullName;
            _candidates.Add(new CatalogProviderCandidateEvidence
            {
                DeclaringType = typeName,
                MethodSignature = FormatSignature(method),
                SourceKind = sourceKind,
                Correlation = correlation,
                CanInvokeWithoutUi = !typeName.Contains(".UI."),
                AppearsReadOnly = !IsMutationSentinel(method)
            });
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
            catch (Exception exception) { _exception = exception; }
        }

        private static bool IsRelevantType(Type type)
        {
            string name = type.FullName ?? "";
            return name.StartsWith("Kingmaker.", StringComparison.Ordinal) &&
                RelevantTypeFragments.Any(name.Contains);
        }

        private static bool IsInitialize(MethodInfo method)
        {
            ParameterInfo[] parameters = method.GetParameters();
            return method.DeclaringType.FullName ==
                "Kingmaker.UI.SaveLoadWindow.ListOfSaves" &&
                method.Name == "Initialize" && parameters.Length == 2 &&
                IsSaveInfoCollection(parameters[0].ParameterType) &&
                parameters[1].ParameterType == typeof(bool);
        }

        private static bool IsCandidate(MethodInfo method)
        {
            if (IsInitialize(method) || IsMutationSentinel(method) ||
                IsLoadSentinel(method)) return false;
            bool collectionSignature = IsSaveInfoCollection(method.ReturnType) ||
                method.GetParameters().Any(x => IsSaveInfoCollection(x.ParameterType));
            if (!collectionSignature) return false;
            return RelevantMemberFragments.Any(method.Name.Contains);
        }

        private static bool IsSaveInfoCollection(Type type)
        {
            if (type == null || type == typeof(string)) return false;
            string name = type.FullName ?? "";
            return name.Contains("System.Collections.Generic.List`1") &&
                name.Contains("Kingmaker.EntitySystem.Persistence.SaveInfo");
        }

        private static bool IsMutationSentinel(MethodBase method)
        {
            string type = method.DeclaringType == null ? "" : method.DeclaringType.FullName;
            return type.EndsWith(".SaveManager", StringComparison.Ordinal) &&
                WritePrefixes.Any(x => method.Name.StartsWith(x, StringComparison.Ordinal)) &&
                method.Name != "SaveList" && method.Name != "SaveInfo";
        }

        private static bool IsLoadSentinel(MethodBase method)
        {
            return (method.DeclaringType.FullName == "Kingmaker.MainMenu" ||
                method.DeclaringType.FullName == "Kingmaker.Game") &&
                method.Name.StartsWith("LoadGame", StringComparison.Ordinal) &&
                method.GetParameters().Any(x =>
                    (x.ParameterType.FullName ?? "").Contains(
                        "Kingmaker.EntitySystem.Persistence.SaveInfo"));
        }

        private static bool IsRelevantMember(string name, Type type)
        {
            return RelevantMemberFragments.Any(name.Contains) ||
                IsSaveInfoCollection(type) ||
                RelevantTypeFragments.Any((type.FullName ?? "").Contains);
        }

        private static string FormatSignature(MethodBase method)
        {
            return method.Name + "(" + string.Join(",", method.GetParameters()
                .Select(x => x.ParameterType.FullName).ToArray()) + ")" +
                (method is MethodInfo ? ":" + ((MethodInfo)method).ReturnType.FullName : "");
        }

        private sealed class ReferenceEqualityComparer : IEqualityComparer<object>
        {
            internal static readonly ReferenceEqualityComparer Instance =
                new ReferenceEqualityComparer();
            public new bool Equals(object left, object right)
            {
                return ReferenceEquals(left, right);
            }
            public int GetHashCode(object value)
            {
                return System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(value);
            }
        }
    }
}
