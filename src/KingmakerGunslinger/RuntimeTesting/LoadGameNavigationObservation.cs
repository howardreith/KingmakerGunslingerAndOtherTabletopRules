using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Harmony12;
using KingmakerGunslinger.Bootstrap;

namespace KingmakerGunslinger.RuntimeTesting
{
    /// <summary>
    /// Request-scoped observation of the UI action which precedes the normal
    /// ListOfSaves.Initialize catalog boundary. It never invokes UI or save APIs.
    /// </summary>
    internal sealed class LoadGameNavigationObservation
    {
        private static LoadGameNavigationObservation _active;
        private readonly ModContext _context;
        private readonly Stopwatch _elapsed;
        private readonly string _runId;
        private readonly int _gameThreadId;
        private readonly Action<SaveLoadObservationEvent> _sink;
        private readonly List<MethodBase> _patched = new List<MethodBase>();
        private readonly List<SaveLoadObservationEvent> _events =
            new List<SaveLoadObservationEvent>();
        private readonly Stack<NavigationFrame> _entered =
            new Stack<NavigationFrame>();
        private MethodBase _initialize;
        private NavigationFrame _navigation;
        private bool _catalogObserved;
        private bool _wrongThread;
        private bool _removed;
        private Exception _exception;

        internal LoadGameNavigationObservation(ModContext context, Stopwatch elapsed,
            string runId, Action<SaveLoadObservationEvent> sink)
        {
            _context = context;
            _elapsed = elapsed;
            _runId = runId;
            _sink = sink;
            _gameThreadId = System.Threading.Thread.CurrentThread.ManagedThreadId;
        }

        internal bool Ready { get { return _patched.Count > 1; } }
        internal bool CatalogObserved { get { return _catalogObserved; } }
        internal bool NavigationProven { get { return _navigation != null; } }
        internal Exception ObservationException { get { return _exception; } }
        internal List<string> HookIdentifiers
        {
            get { return _patched.Select(FormatSignature).OrderBy(x => x).ToList(); }
        }

        internal void Install()
        {
            if (_active != null) throw new InvalidOperationException(
                "A Load Game navigation observation is already active.");
            Assembly assembly = typeof(Kingmaker.Game).Assembly;
            Type listType = assembly.GetType(
                "Kingmaker.UI.SaveLoadWindow.ListOfSaves", true);
            _initialize = listType.GetMethods(BindingFlags.Instance |
                    BindingFlags.Public | BindingFlags.NonPublic)
                .Single(method => method.Name == "Initialize" &&
                    method.GetParameters().Length == 2 &&
                    IsSaveInfoList(method.GetParameters()[0].ParameterType) &&
                    method.GetParameters()[1].ParameterType == typeof(bool));

            MethodInfo prefix = typeof(LoadGameNavigationObservation).GetMethod(
                "Prefix", BindingFlags.Static | BindingFlags.NonPublic);
            MethodInfo postfix = typeof(LoadGameNavigationObservation).GetMethod(
                "Postfix", BindingFlags.Static | BindingFlags.NonPublic);
            Patch(_initialize, prefix, postfix);

            foreach (Type type in assembly.GetTypes())
            {
                string name = type.FullName ?? "";
                if (!(name.StartsWith("Kingmaker.UI.", StringComparison.Ordinal) ||
                    name == "Kingmaker.MainMenu")) continue;
                foreach (MethodInfo method in type.GetMethods(BindingFlags.Instance |
                    BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic |
                    BindingFlags.DeclaredOnly))
                {
                    if (method.IsAbstract || method.ContainsGenericParameters ||
                        method == _initialize || IsSaveMutationOrLoad(method)) continue;
                    if (Calls(method, _initialize) ||
                        (method.Name.IndexOf("LoadGame", StringComparison.OrdinalIgnoreCase) >= 0 &&
                         method.GetParameters().All(p => !IsSaveInfo(p.ParameterType))))
                        Patch(method, prefix, postfix);
                }
            }
            if (_patched.Count <= 1) throw new MissingMethodException(
                "No exact UI caller or Load Game navigation candidate was found.");
            _active = this;
            Add("hooks-installed", null, null, "patchCount=" + _patched.Count);
        }

        internal LoadGameNavigationEvidence Stop()
        {
            foreach (MethodBase method in _patched)
                _context.Harmony.Unpatch(method, HarmonyPatchType.All, _context.ModId);
            _patched.Clear();
            _removed = true;
            if (ReferenceEquals(_active, this)) _active = null;
            Add("observer-removed", null, null, "all request-scoped patches removed");
            return new LoadGameNavigationEvidence
            {
                NavigationProven = _navigation != null,
                DeclaringType = _navigation == null ? "" :
                    _navigation.Method.DeclaringType.FullName,
                MethodSignature = _navigation == null ? "" :
                    FormatSignature(_navigation.Method),
                ReceiverType = _navigation == null ? "" : _navigation.ReceiverType,
                ArgumentTypes = _navigation == null ? new List<string>() :
                    _navigation.ArgumentTypes,
                ManagedThreadId = _navigation == null ? 0 : _navigation.ThreadId,
                CatalogInitializeSignature = FormatSignature(_initialize),
                CatalogObserved = _catalogObserved,
                AllCallbacksOnGameThread = !_wrongThread,
                HooksRemoved = _removed,
                ProbeInvokedNavigation = false,
                Events = new List<SaveLoadObservationEvent>(_events)
            };
        }

        private void Patch(MethodBase method, MethodInfo prefix, MethodInfo postfix)
        {
            if (_patched.Contains(method)) return;
            _context.Harmony.Patch(method, new HarmonyMethod(prefix),
                new HarmonyMethod(postfix), null);
            _patched.Add(method);
        }

        private static void Prefix(MethodBase __originalMethod, object __instance,
            object[] __args)
        {
            LoadGameNavigationObservation active = _active;
            if (active == null) return;
            active.Enter(__originalMethod, __instance, __args);
        }

        private static void Postfix(MethodBase __originalMethod, object[] __args)
        {
            LoadGameNavigationObservation active = _active;
            if (active == null) return;
            active.Exit(__originalMethod, __args);
        }

        private void Enter(MethodBase method, object receiver, object[] arguments)
        {
            int thread = System.Threading.Thread.CurrentThread.ManagedThreadId;
            if (thread != _gameThreadId) _wrongThread = true;
            if (method == _initialize)
            {
                _catalogObserved = true;
                _navigation = _entered.FirstOrDefault(candidate =>
                    candidate.Method.Name.IndexOf("LoadGame",
                        StringComparison.OrdinalIgnoreCase) >= 0 &&
                    (candidate.Method.DeclaringType == null ||
                     candidate.Method.DeclaringType.FullName !=
                        "Kingmaker.UI.SaveLoadWindow.ListOfSaves"));
                Add("catalog-initialize-enter", method, arguments,
                    _navigation == null ? "no active navigation caller" :
                    "precededBy=" + FormatSignature(_navigation.Method));
                return;
            }
            var frame = new NavigationFrame
            {
                Method = method,
                ReceiverType = receiver == null ? "<static-or-null>" :
                    receiver.GetType().FullName,
                ArgumentTypes = Types(arguments),
                ThreadId = thread
            };
            _entered.Push(frame);
            Add("navigation-candidate-enter", method, arguments,
                "receiver=" + frame.ReceiverType);
        }

        private void Exit(MethodBase method, object[] arguments)
        {
            Add(method == _initialize ? "catalog-initialize-exit" :
                "navigation-candidate-exit", method, arguments, "");
            if (method != _initialize && _entered.Count > 0 &&
                _entered.Peek().Method == method) _entered.Pop();
        }

        private void Add(string kind, MethodBase method, object[] arguments, string detail)
        {
            var item = new SaveLoadObservationEvent
            {
                RunId = _runId, Sequence = _events.Count + 1,
                ElapsedMilliseconds = _elapsed.ElapsedMilliseconds,
                Utc = DateTime.UtcNow.ToString("o"), Kind = kind,
                DeclaringType = method == null || method.DeclaringType == null ? "" :
                    method.DeclaringType.FullName,
                MethodSignature = method == null ? "" : FormatSignature(method),
                ArgumentTypes = Types(arguments),
                ManagedThreadId = System.Threading.Thread.CurrentThread.ManagedThreadId,
                DisplayName = "", SafeSaveIdentifier = "", Detail = detail ?? "",
                Exception = ""
            };
            _events.Add(item);
            if (_sink != null) try { _sink(item); } catch (Exception ex) { _exception = ex; }
        }

        private static bool Calls(MethodInfo method, MethodBase target)
        {
            MethodBody body;
            try { body = method.GetMethodBody(); } catch { return false; }
            if (body == null) return false;
            byte[] il = body.GetILAsByteArray();
            int token = target.MetadataToken;
            byte[] bytes = BitConverter.GetBytes(token);
            for (int i = 0; i <= il.Length - 5; i++)
                if ((il[i] == 0x28 || il[i] == 0x6f) &&
                    il[i + 1] == bytes[0] && il[i + 2] == bytes[1] &&
                    il[i + 3] == bytes[2] && il[i + 4] == bytes[3]) return true;
            return false;
        }

        private static bool IsSaveInfoList(Type type)
        {
            return type.IsGenericType &&
                type.GetGenericTypeDefinition() == typeof(List<>) &&
                IsSaveInfo(type.GetGenericArguments()[0]);
        }

        private static bool IsSaveInfo(Type type)
        {
            return type != null && type.FullName ==
                "Kingmaker.EntitySystem.Persistence.SaveInfo";
        }

        private static bool IsSaveMutationOrLoad(MethodInfo method)
        {
            string name = method.Name;
            bool mutation = name.StartsWith("Save", StringComparison.Ordinal) ||
                name.StartsWith("Delete", StringComparison.Ordinal) ||
                name.StartsWith("Remove", StringComparison.Ordinal) ||
                name.StartsWith("Rename", StringComparison.Ordinal) ||
                name.StartsWith("Migrate", StringComparison.Ordinal);
            return mutation || method.GetParameters().Any(p => IsSaveInfo(p.ParameterType));
        }

        private static List<string> Types(object[] arguments)
        {
            return arguments == null ? new List<string>() :
                arguments.Select(x => x == null ? "null" : x.GetType().FullName).ToList();
        }

        private static string FormatSignature(MethodBase method)
        {
            if (method == null) return "";
            return method.Name + "(" + string.Join(",", method.GetParameters()
                .Select(x => x.ParameterType.FullName).ToArray()) + ")" +
                (method is MethodInfo ? ":" + ((MethodInfo)method).ReturnType.FullName : "");
        }

        private sealed class NavigationFrame
        {
            internal MethodBase Method;
            internal string ReceiverType;
            internal List<string> ArgumentTypes;
            internal int ThreadId;
        }
    }
}
