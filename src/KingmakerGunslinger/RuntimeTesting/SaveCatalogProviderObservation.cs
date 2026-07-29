using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using Harmony12;
using KingmakerGunslinger.Bootstrap;

namespace KingmakerGunslinger.RuntimeTesting
{
    /// <summary>
    /// Observes provenance for the exact collection consumed by the normal Load
    /// Game UI. The observer patches existing execution only; it never invokes a
    /// provider, getter, callback, coroutine, save load, or save mutation.
    /// </summary>
    internal sealed class SaveCatalogProviderObservation
    {
        private static readonly string[] RelevantTypeFragments =
            { "SaveManager", "SaveLoad", "ListOfSaves", "MainMenu" };
        private static readonly string[] RelevantMemberFragments =
            { "Save", "List", "Catalog", "Load", "Owner", "Controller", "Model", "View" };
        private static readonly string[] TransformFragments =
            { "Filter", "Sort", "Search", "Visible", "Display", "Group" };
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
        private readonly Dictionary<object, MethodBase> _returned =
            new Dictionary<object, MethodBase>(ReferenceComparer.Instance);
        private readonly Dictionary<object, MethodBase> _received =
            new Dictionary<object, MethodBase>(ReferenceComparer.Instance);
        private readonly List<string> _fingerprints = new List<string>();
        private readonly List<string> _missing = new List<string>();
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
        private string _collectionIdentity = "";
        private string _receiverIdentity = "";
        private string _classification = "unobserved";
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

        internal bool Ready { get { return _initializeHookInstalled &&
            _candidateHookInstalled && _sentinelHookInstalled; } }
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
            MethodInfo initializePrefix = PatchMethod("InitializePrefix");
            MethodInfo candidatePrefix = PatchMethod("CandidatePrefix");
            MethodInfo resultPostfix = PatchMethod("ResultPostfix");
            Assembly assembly = typeof(Kingmaker.Game).Assembly;
            MethodInfo caller = FindOneArgumentInitialize(assembly);
            HashSet<MethodBase> directDependencies = ReadDirectDependencies(caller);
            HashSet<Type> dependencyTypes = new HashSet<Type>(directDependencies
                .Where(x => x.DeclaringType != null).Select(x => x.DeclaringType));

            foreach (Type type in assembly.GetTypes().Where(x =>
                IsRelevantType(x) || dependencyTypes.Contains(x))
                .OrderBy(x => x.FullName, StringComparer.Ordinal))
            {
                foreach (MethodInfo method in type.GetMethods(BindingFlags.Instance |
                    BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                    .OrderBy(FormatSignature, StringComparer.Ordinal))
                {
                    if (IsConsumer(method))
                    {
                        Patch(method, initializePrefix, null);
                        _initializeHookInstalled = true;
                    }
                    else if (IsCandidate(method, directDependencies))
                    {
                        Patch(method, candidatePrefix,
                            IsCompatibleCollection(method.ReturnType)
                                ? resultPostfix : null);
                        _candidateHookInstalled = true;
                    }
                    else if (IsMutationSentinel(method) || IsLoadSentinel(method))
                    {
                        Patch(method, candidatePrefix, null);
                        _sentinelHookInstalled = true;
                    }
                }
            }
            Add("hooks-installed", null, null,
                "patchCount=" + _patched.Count + ";directCallerDependencies=" +
                directDependencies.Count);
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
                Events = new List<SaveLoadObservationEvent>(_events),
                CollectionObjectIdentity = _collectionIdentity,
                ReceiverObjectIdentity = _receiverIdentity,
                SafeEntryFingerprints = new List<string>(_fingerprints),
                CatalogClassification = _classification,
                RemainingEvidenceMissing = new List<string>(_missing)
            };
        }

        private MethodInfo PatchMethod(string name)
        {
            return typeof(SaveCatalogProviderObservation).GetMethod(
                name, BindingFlags.Static | BindingFlags.NonPublic);
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
                _active.CaptureConsumer(__originalMethod, __instance, __args);
        }

        private static void CandidatePrefix(
            MethodBase __originalMethod, object __instance, object[] __args)
        {
            if (_active != null)
                _active.ObserveEntry(__originalMethod, __instance, __args);
        }

        private static void ResultPostfix(
            MethodBase __originalMethod, object __instance, object __result)
        {
            if (_active != null)
                _active.ObserveReturn(__originalMethod, __instance, __result);
        }

        private void ObserveEntry(MethodBase method, object receiver, object[] args)
        {
            CheckThread();
            if (IsMutationSentinel(method)) _writeObserved = true;
            if (IsLoadSentinel(method)) _loadObserved = true;
            foreach (object argument in args ?? new object[0])
            {
                if (argument == null || !IsCompatibleCollection(argument.GetType()))
                    continue;
                _received[argument] = method;
                RememberCandidate(method, receiver, "callback-or-state-machine-argument",
                    "collection received; consumer identity not yet known", false);
                Add("catalog-object-received", method, args,
                    "identity=" + ObjectIdentity(argument));
            }
            Add("candidate-provider-entered", method, args,
                IsMutationSentinel(method) ? "save-writing-api-observed" :
                IsLoadSentinel(method) ? "save-load-api-observed" : "");
        }

        private void ObserveReturn(MethodBase method, object receiver, object result)
        {
            CheckThread();
            if (result == null || !IsCompatibleCollection(result.GetType())) return;
            _returned[result] = method;
            RememberCandidate(method, receiver, "method-return",
                "collection returned; consumer identity not yet known", false);
            Add("candidate-provider-returned", method, new[] { result },
                "identity=" + ObjectIdentity(result));
            Add("catalog-object-created-or-returned", method, new[] { result },
                "creation cannot be distinguished from retrieval at a postfix");
        }

        private void CaptureConsumer(MethodBase method, object receiver, object[] args)
        {
            CheckThread();
            object collection = args == null ? null : args.FirstOrDefault();
            IEnumerable enumerable = collection as IEnumerable;
            _initializeSignature = FormatSignature(method);
            _receiverType = receiver == null ? "" : receiver.GetType().FullName;
            _collectionType = collection == null ? "" : collection.GetType().FullName;
            _collectionIdentity = ObjectIdentity(collection);
            _receiverIdentity = ObjectIdentity(receiver);
            _descriptorCount = 0;
            if (enumerable != null)
            {
                foreach (object descriptor in enumerable)
                {
                    if (descriptor == null) continue;
                    _descriptorCount++;
                    if (string.IsNullOrWhiteSpace(_descriptorType))
                        _descriptorType = descriptor.GetType().FullName;
                    if (_fingerprints.Count < 8)
                        _fingerprints.Add(SafeFingerprint(descriptor));
                }
            }
            CaptureCallerChain();
            CaptureOwnerMetadata(receiver);
            Add("load-game-action-observed", method, args,
                "normal Load Game display-model consumer entered");

            MethodBase producer;
            if (collection != null && _returned.TryGetValue(collection, out producer))
                Correlate(producer, "method-return");
            else if (collection != null && _received.TryGetValue(collection, out producer))
            {
                Correlate(producer, "callback-or-state-machine-argument");
                Add("catalog-object-assigned-or-propagated", producer, args,
                    "same reference later passed to Initialize");
            }
            else
            {
                _missing.Add("No observed return or callback argument shared reference " +
                    "identity with the ListOfSaves.Initialize collection.");
                _missing.Add("If the caller loaded a field directly, the field writer " +
                    "must be observed in a further supervised pass.");
            }
            _captured = enumerable != null && _descriptorCount > 0;
            Add("catalog-object-passed-to-initialize", method, args,
                "identity=" + _collectionIdentity + ";receiver=" + _receiverIdentity +
                ";descriptorCount=" + _descriptorCount);
            Add("complete-versus-filtered-classified", method, args,
                "classification=" + _classification);
            Add(_sourceProven ? "provider-correlation-succeeded" :
                "provider-correlation-failed", method, args,
                _sourceProven ? "reference identity proven" :
                string.Join(" ", _missing.ToArray()));
        }

        private void Correlate(MethodBase producer, string sourceKind)
        {
            bool transformed = TransformFragments.Any(producer.Name.Contains);
            _sourceKind = sourceKind;
            _classification = transformed ? "filtered-or-sorted-ui-collection" :
                "complete-catalog-source-and-final-consumer-list";
            _sourceProven = !transformed;
            RememberCandidate(producer, null, sourceKind,
                "object-reference equals ListOfSaves.Initialize argument", true);
            if (transformed)
                _missing.Add("The correlated producer is a filtering/sorting member, " +
                    "so the upstream complete catalog remains unproven.");
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
                AddMember(type, field.Name, field.FieldType, "field-metadata-only");
            foreach (PropertyInfo property in type.GetProperties(BindingFlags.Instance |
                BindingFlags.Public | BindingFlags.NonPublic)
                .Where(x => x.GetIndexParameters().Length == 0 &&
                    IsRelevantMember(x.Name, x.PropertyType)))
                AddMember(type, property.Name, property.PropertyType,
                    "property-metadata-only");
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

        private void RememberCandidate(MethodBase method, object receiver,
            string sourceKind, string correlation, bool correlated)
        {
            CatalogProviderCandidateEvidence candidate = _candidates.FirstOrDefault(x =>
                x.MethodSignature == FormatSignature(method) &&
                x.DeclaringType == method.DeclaringType.FullName &&
                x.SourceKind == sourceKind);
            string typeName = method.DeclaringType.FullName;
            bool ui = typeName.Contains(".UI.");
            bool mutation = IsMutationSentinel(method);
            if (candidate == null)
            {
                candidate = new CatalogProviderCandidateEvidence
                {
                    DeclaringType = typeName,
                    MethodSignature = FormatSignature(method),
                    SourceKind = sourceKind,
                    IsStatic = method.IsStatic,
                    ReceiverType = receiver == null ? "" : receiver.GetType().FullName,
                    RequiredArguments = method.GetParameters().Select(
                        x => x.ParameterType.FullName).ToList(),
                    ReturnType = method is MethodInfo
                        ? ((MethodInfo)method).ReturnType.FullName : "System.Void",
                    ManagedThreadId = Thread.CurrentThread.ManagedThreadId,
                    RequiresLoadGameUi = ui,
                    CanInvokeWithoutUi = !ui,
                    AppearsReadOnly = !mutation,
                    SideEffects = mutation ? "save mutation sentinel" : "none observed"
                };
                _candidates.Add(candidate);
            }
            candidate.Correlation = correlation;
            candidate.CatalogRole = correlated
                ? (_classification.StartsWith("complete", StringComparison.Ordinal)
                    ? "complete-catalog-source" : "ui-transform")
                : "unresolved";
            candidate.ContractStable = correlated && !ui && !mutation;
            candidate.ProofMissing = correlated ? "" :
                "reference identity with consumer has not been observed";
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
                DeclaringType = method == null || method.DeclaringType == null
                    ? "" : method.DeclaringType.FullName,
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

        private static bool IsConsumer(MethodInfo method)
        {
            ParameterInfo[] parameters = method.GetParameters();
            return method.DeclaringType.FullName ==
                "Kingmaker.UI.SaveLoadWindow.ListOfSaves" &&
                method.Name == "Initialize" && parameters.Length == 2 &&
                IsCompatibleCollection(parameters[0].ParameterType) &&
                parameters[1].ParameterType == typeof(bool);
        }

        private static bool IsCandidate(
            MethodInfo method, HashSet<MethodBase> directDependencies)
        {
            if (IsConsumer(method) || IsMutationSentinel(method) ||
                IsLoadSentinel(method)) return false;
            bool signature = IsCompatibleCollection(method.ReturnType) ||
                method.GetParameters().Any(
                    x => IsCompatibleCollection(x.ParameterType));
            if (!signature) return false;
            return directDependencies.Contains(method) ||
                (IsRelevantType(method.DeclaringType) &&
                    RelevantMemberFragments.Any(method.Name.Contains)) ||
                method.Name == "MoveNext" || method.Name == "Invoke";
        }

        private static bool IsCompatibleCollection(Type type)
        {
            if (type == null || type == typeof(string)) return false;
            if (type.IsArray) return IsSaveInfo(type.GetElementType());
            if (type.IsGenericType && type.GetGenericArguments().Any(IsSaveInfo) &&
                typeof(IEnumerable).IsAssignableFrom(type)) return true;
            return type.GetInterfaces().Any(x => x.IsGenericType &&
                x.GetGenericArguments().Any(IsSaveInfo) &&
                typeof(IEnumerable).IsAssignableFrom(x));
        }

        private static bool IsSaveInfo(Type type)
        {
            return type != null && type.FullName ==
                "Kingmaker.EntitySystem.Persistence.SaveInfo";
        }

        private static MethodInfo FindOneArgumentInitialize(Assembly assembly)
        {
            Type type = assembly.GetType("Kingmaker.UI.SaveLoadWindow.ListOfSaves", false);
            return type == null ? null : type.GetMethods(BindingFlags.Instance |
                BindingFlags.Public | BindingFlags.NonPublic).FirstOrDefault(x =>
                    x.Name == "Initialize" && x.GetParameters().Length == 1 &&
                    x.GetParameters()[0].ParameterType == typeof(bool));
        }

        private static HashSet<MethodBase> ReadDirectDependencies(MethodInfo method)
        {
            var result = new HashSet<MethodBase>();
            MethodBody body = method == null ? null : method.GetMethodBody();
            byte[] il = body == null ? null : body.GetILAsByteArray();
            if (il == null) return result;
            Dictionary<short, OpCode> opcodes = typeof(OpCodes).GetFields(
                BindingFlags.Public | BindingFlags.Static)
                .Where(x => x.FieldType == typeof(OpCode))
                .Select(x => (OpCode)x.GetValue(null))
                .ToDictionary(x => x.Value);
            int index = 0;
            while (index < il.Length)
            {
                short value = il[index++];
                if (value == 0xfe && index < il.Length)
                    value = (short)(0xfe00 | il[index++]);
                OpCode opcode;
                if (!opcodes.TryGetValue(value, out opcode)) break;
                int operandSize = OperandSize(opcode.OperandType, il, index);
                if ((opcode == OpCodes.Call || opcode == OpCodes.Callvirt) &&
                    operandSize == 4 && index + 4 <= il.Length)
                {
                    try
                    {
                        MethodBase dependency = method.Module.ResolveMethod(
                            BitConverter.ToInt32(il, index),
                            method.DeclaringType.GetGenericArguments(), Type.EmptyTypes);
                        if (dependency != null) result.Add(dependency);
                    }
                    catch (ArgumentException) { }
                    catch (BadImageFormatException) { }
                }
                index += operandSize;
            }
            return result;
        }

        private static int OperandSize(OperandType type, byte[] il, int index)
        {
            switch (type)
            {
                case OperandType.InlineNone: return 0;
                case OperandType.ShortInlineBrTarget:
                case OperandType.ShortInlineI:
                case OperandType.ShortInlineVar: return 1;
                case OperandType.InlineVar: return 2;
                case OperandType.InlineI8:
                case OperandType.InlineR: return 8;
                case OperandType.InlineSwitch:
                    return index + 4 <= il.Length
                        ? 4 + 4 * BitConverter.ToInt32(il, index) : 0;
                default: return 4;
            }
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
            return method.DeclaringType != null &&
                (method.DeclaringType.FullName == "Kingmaker.MainMenu" ||
                 method.DeclaringType.FullName == "Kingmaker.Game") &&
                method.Name.StartsWith("LoadGame", StringComparison.Ordinal) &&
                method.GetParameters().Any(x => IsSaveInfo(x.ParameterType));
        }

        private static bool IsRelevantMember(string name, Type type)
        {
            return RelevantMemberFragments.Any(name.Contains) ||
                IsCompatibleCollection(type) ||
                RelevantTypeFragments.Any((type.FullName ?? "").Contains);
        }

        private static string ObjectIdentity(object value)
        {
            return value == null ? "" : value.GetType().FullName + "#" +
                RuntimeHelpers.GetHashCode(value).ToString("x8");
        }

        private static string SafeFingerprint(object descriptor)
        {
            string value = descriptor.GetType().FullName + "|" +
                RuntimeHelpers.GetHashCode(descriptor).ToString("x8");
            using (SHA256 sha = SHA256.Create())
                return BitConverter.ToString(sha.ComputeHash(
                    Encoding.UTF8.GetBytes(value))).Replace("-", "")
                    .Substring(0, 16).ToLowerInvariant();
        }

        private static string FormatSignature(MethodBase method)
        {
            return method.Name + "(" + string.Join(",", method.GetParameters()
                .Select(x => x.ParameterType.FullName).ToArray()) + ")" +
                (method is MethodInfo ? ":" + ((MethodInfo)method).ReturnType.FullName : "");
        }

        private sealed class ReferenceComparer : IEqualityComparer<object>
        {
            internal static readonly ReferenceComparer Instance = new ReferenceComparer();
            public new bool Equals(object left, object right)
            {
                return ReferenceEquals(left, right);
            }
            public int GetHashCode(object value)
            {
                return RuntimeHelpers.GetHashCode(value);
            }
        }
    }
}
