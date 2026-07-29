using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Harmony12;
using KingmakerGunslinger.Bootstrap;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace KingmakerGunslinger.RuntimeTesting
{
    /// <summary>
    /// Request-scoped, non-initiating observation of the proven normal main-menu
    /// Load Game handler and the catalog boundary which follows it.
    /// </summary>
    internal sealed class LoadGameButtonActionObservation
    {
        private const string ButtonsTypeName =
            "Kingmaker.UI.MainMenuUI.MainMenuButtons";
        private const string CatalogTypeName =
            "Kingmaker.UI.SaveLoadWindow.ListOfSaves";
        private const string HandlerName = "OnButtonLoadGame";
        private static LoadGameButtonActionObservation _active;

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
        private List<LoadGameButtonCandidateEvidence> _candidates =
            new List<LoadGameButtonCandidateEvidence>();
        private int _handlerCount;
        private int _handlerSequence;
        private int _catalogSequence;
        private bool _wrongThread;
        private bool _removed;
        private Exception _exception;

        internal LoadGameButtonActionObservation(ModContext context,
            Stopwatch elapsed, string runId,
            Action<SaveLoadObservationEvent> sink)
        {
            _context = context;
            _elapsed = elapsed;
            _runId = runId;
            _sink = sink;
            _gameThreadId = System.Threading.Thread.CurrentThread.ManagedThreadId;
        }

        internal bool Ready { get { return _patched.Count == 2; } }
        internal bool CatalogObserved { get { return _catalogSequence > 0; } }
        internal bool ActionProven
        {
            get
            {
                return _handlerCount == 1 && _candidates.Count == 1 &&
                    _candidates[0].ActiveInHierarchy &&
                    _candidates[0].Interactable &&
                    _catalogSequence > _handlerSequence &&
                    !_wrongThread;
            }
        }
        internal Exception ObservationException { get { return _exception; } }
        internal List<string> HookIdentifiers
        {
            get { return _patched.Select(FormatSignature).OrderBy(x => x).ToList(); }
        }

        internal void Install()
        {
            if (_active != null) throw new InvalidOperationException(
                "A Load Game button-action observation is already active.");
            Assembly assembly = typeof(Kingmaker.Game).Assembly;
            Type buttonsType = assembly.GetType(ButtonsTypeName, true);
            _handler = buttonsType.GetMethods(BindingFlags.Instance |
                    BindingFlags.Public | BindingFlags.NonPublic)
                .Single(method => method.Name == HandlerName &&
                    method.GetParameters().Length == 0 &&
                    method.ReturnType == typeof(void));
            Type listType = assembly.GetType(CatalogTypeName, true);
            _initialize = listType.GetMethods(BindingFlags.Instance |
                    BindingFlags.Public | BindingFlags.NonPublic)
                .Single(method => method.Name == "Initialize" &&
                    method.GetParameters().Length == 2 &&
                    IsSaveInfoList(method.GetParameters()[0].ParameterType) &&
                    method.GetParameters()[1].ParameterType == typeof(bool) &&
                    method.ReturnType == typeof(void));

            MethodInfo prefix = typeof(LoadGameButtonActionObservation).GetMethod(
                "Prefix", BindingFlags.Static | BindingFlags.NonPublic);
            MethodInfo postfix = typeof(LoadGameButtonActionObservation).GetMethod(
                "Postfix", BindingFlags.Static | BindingFlags.NonPublic);
            Patch(_handler, prefix, postfix);
            Patch(_initialize, prefix, postfix);
            _active = this;
            Add("hooks-installed", null, null,
                "exactHandler=" + FormatSignature(_handler) +
                ";catalogBoundary=" + FormatSignature(_initialize));
        }

        internal LoadGameButtonActionEvidence Stop()
        {
            foreach (MethodBase method in _patched)
                _context.Harmony.Unpatch(method, HarmonyPatchType.All, _context.ModId);
            _patched.Clear();
            _removed = true;
            if (ReferenceEquals(_active, this)) _active = null;
            Add("observer-removed", null, null,
                "all request-scoped patches removed");
            return new LoadGameButtonActionEvidence
            {
                ActionProven = ActionProven,
                HandlerSignature = FormatSignature(_handler),
                HandlerInvocationCount = _handlerCount,
                Candidates = new List<LoadGameButtonCandidateEvidence>(_candidates),
                CatalogInitializeSignature = FormatSignature(_initialize),
                CatalogObservedAfterAction = _catalogSequence > _handlerSequence &&
                    _handlerSequence > 0,
                GameThreadManagedId = _gameThreadId,
                AllCallbacksOnGameThread = !_wrongThread,
                HooksRemoved = _removed,
                ProbeInvokedAction = false,
                Events = new List<SaveLoadObservationEvent>(_events)
            };
        }

        private void Patch(MethodBase method, MethodInfo prefix, MethodInfo postfix)
        {
            _context.Harmony.Patch(method, new HarmonyMethod(prefix),
                new HarmonyMethod(postfix), null);
            _patched.Add(method);
        }

        private static void Prefix(MethodBase __originalMethod, object __instance,
            object[] __args)
        {
            LoadGameButtonActionObservation active = _active;
            if (active != null) active.Enter(__originalMethod, __instance, __args);
        }

        private static void Postfix(MethodBase __originalMethod, object[] __args)
        {
            LoadGameButtonActionObservation active = _active;
            if (active != null) active.Exit(__originalMethod, __args);
        }

        private void Enter(MethodBase method, object receiver, object[] arguments)
        {
            int thread = System.Threading.Thread.CurrentThread.ManagedThreadId;
            if (thread != _gameThreadId) _wrongThread = true;
            if (method == _handler)
            {
                _handlerCount++;
                _handlerSequence = _events.Count + 1;
                _candidates = FindCandidates(receiver);
                Add("load-game-handler-enter", method, arguments,
                    "candidateCount=" + _candidates.Count +
                    ";receiver=" + RuntimeType(receiver));
                foreach (LoadGameButtonCandidateEvidence candidate in _candidates)
                    Add("load-game-button-candidate", method, null,
                        CandidateDetail(candidate));
                return;
            }
            _catalogSequence = _events.Count + 1;
            Add("catalog-initialize-enter", method, arguments,
                "afterHandler=" + (_handlerSequence > 0) +
                ";handlerCount=" + _handlerCount);
        }

        private void Exit(MethodBase method, object[] arguments)
        {
            Add(method == _handler ? "load-game-handler-exit" :
                "catalog-initialize-exit", method, arguments, "");
        }

        private List<LoadGameButtonCandidateEvidence> FindCandidates(object receiver)
        {
            Component owner = receiver as Component;
            if (owner == null) return new List<LoadGameButtonCandidateEvidence>();
            Transform root = owner.transform;
            while (root.parent != null && root.parent.gameObject.activeInHierarchy)
                root = root.parent;
            var matches = new List<LoadGameButtonCandidateEvidence>();
            foreach (Button button in root.GetComponentsInChildren<Button>(true))
            {
                List<LoadGameListenerEvidence> listeners = ReadListeners(button.onClick);
                if (!listeners.Any(item =>
                    item.MethodName == HandlerName &&
                    (item.TargetType == ButtonsTypeName ||
                     item.TargetType == receiver.GetType().FullName))) continue;
                Transform transform = button.transform;
                matches.Add(new LoadGameButtonCandidateEvidence
                {
                    ComponentType = button.GetType().FullName,
                    GameObjectPath = HierarchyPath(transform),
                    ActiveSelf = button.gameObject.activeSelf,
                    ActiveInHierarchy = button.gameObject.activeInHierarchy,
                    Interactable = button.interactable,
                    SiblingIndex = transform.GetSiblingIndex(),
                    SiblingCount = transform.parent == null ? 1 :
                        transform.parent.childCount,
                    OwnerType = receiver.GetType().FullName,
                    MainMenuRootName = root.gameObject.name,
                    MainMenuRootPath = HierarchyPath(root),
                    ComponentIdentities = ComponentIdentities(button.gameObject),
                    SafeLabelIdentities = SafeLabelIdentities(button.gameObject),
                    Listeners = listeners
                });
            }
            return matches;
        }

        private static List<LoadGameListenerEvidence> ReadListeners(UnityEvent action)
        {
            var listeners = new List<LoadGameListenerEvidence>();
            int persistentCount = action.GetPersistentEventCount();
            for (int index = 0; index < persistentCount; index++)
            {
                UnityEngine.Object target = action.GetPersistentTarget(index);
                listeners.Add(new LoadGameListenerEvidence
                {
                    Kind = "persistent",
                    TargetType = target == null ? "<null>" : target.GetType().FullName,
                    MethodName = action.GetPersistentMethodName(index)
                });
            }
            object calls = ReadField(action, "m_Calls");
            foreach (object invokable in EnumerateCallLists(calls))
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
                    if (!listeners.Any(x => x.Kind == evidence.Kind &&
                        x.TargetType == evidence.TargetType &&
                        x.MethodName == evidence.MethodName)) listeners.Add(evidence);
                }
            }
            return listeners.OrderBy(x => x.Kind).ThenBy(x => x.TargetType)
                .ThenBy(x => x.MethodName).ToList();
        }

        private static IEnumerable<object> EnumerateCallLists(object calls)
        {
            if (calls == null) yield break;
            foreach (string name in new[] { "m_PersistentCalls", "m_RuntimeCalls",
                "m_ExecutingCalls" })
            {
                IEnumerable values = ReadField(calls, name) as IEnumerable;
                if (values == null) continue;
                foreach (object value in values) if (value != null) yield return value;
            }
        }

        private static Delegate FindDelegate(object value)
        {
            if (value == null) return null;
            Type type = value.GetType();
            while (type != null)
            {
                foreach (FieldInfo field in type.GetFields(BindingFlags.Instance |
                    BindingFlags.Public | BindingFlags.NonPublic |
                    BindingFlags.DeclaredOnly))
                {
                    if (!typeof(Delegate).IsAssignableFrom(field.FieldType)) continue;
                    try { return field.GetValue(value) as Delegate; } catch { return null; }
                }
                type = type.BaseType;
            }
            return null;
        }

        private static object ReadField(object value, string name)
        {
            if (value == null) return null;
            Type type = value.GetType();
            while (type != null)
            {
                FieldInfo field = type.GetField(name, BindingFlags.Instance |
                    BindingFlags.Public | BindingFlags.NonPublic |
                    BindingFlags.DeclaredOnly);
                if (field != null) try { return field.GetValue(value); } catch { return null; }
                type = type.BaseType;
            }
            return null;
        }

        private static List<string> ComponentIdentities(GameObject gameObject)
        {
            return gameObject.GetComponents<Component>()
                .Where(item => item != null)
                .Select(item => item.GetType().FullName)
                .OrderBy(item => item).ToList();
        }

        private static List<string> SafeLabelIdentities(GameObject gameObject)
        {
            var values = new List<string>();
            foreach (Component component in gameObject.GetComponentsInChildren<Component>(true))
            {
                if (component == null) continue;
                Type type = component.GetType();
                foreach (MemberInfo member in type.GetMembers(BindingFlags.Instance |
                    BindingFlags.Public | BindingFlags.NonPublic))
                {
                    string name = member.Name;
                    if (name.IndexOf("key", StringComparison.OrdinalIgnoreCase) < 0 &&
                        name.IndexOf("text", StringComparison.OrdinalIgnoreCase) < 0 &&
                        name.IndexOf("label", StringComparison.OrdinalIgnoreCase) < 0) continue;
                    object value = null;
                    try
                    {
                        FieldInfo field = member as FieldInfo;
                        PropertyInfo property = member as PropertyInfo;
                        if (field != null && field.FieldType == typeof(string))
                            value = field.GetValue(component);
                        else if (property != null && property.PropertyType == typeof(string) &&
                            property.GetIndexParameters().Length == 0 && property.CanRead)
                            value = property.GetValue(component, null);
                    }
                    catch { continue; }
                    string text = value as string;
                    if (string.IsNullOrWhiteSpace(text) || text.Length > 160) continue;
                    values.Add(type.FullName + "." + name + "=" + text);
                }
            }
            return values.Distinct().OrderBy(x => x).Take(32).ToList();
        }

        private void Add(string kind, MethodBase method, object[] arguments,
            string detail)
        {
            var item = new SaveLoadObservationEvent
            {
                RunId = _runId,
                Sequence = _events.Count + 1,
                ElapsedMilliseconds = _elapsed.ElapsedMilliseconds,
                Utc = DateTime.UtcNow.ToString("o"),
                Kind = kind,
                DeclaringType = method == null || method.DeclaringType == null ? "" :
                    method.DeclaringType.FullName,
                MethodSignature = method == null ? "" : FormatSignature(method),
                ArgumentTypes = Types(arguments),
                ManagedThreadId = System.Threading.Thread.CurrentThread.ManagedThreadId,
                DisplayName = "",
                SafeSaveIdentifier = "",
                Detail = detail ?? "",
                Exception = ""
            };
            _events.Add(item);
            if (_sink != null) try { _sink(item); } catch (Exception ex) { _exception = ex; }
        }

        private static string CandidateDetail(LoadGameButtonCandidateEvidence value)
        {
            return "component=" + value.ComponentType +
                ";path=" + value.GameObjectPath +
                ";activeSelf=" + value.ActiveSelf +
                ";activeInHierarchy=" + value.ActiveInHierarchy +
                ";interactable=" + value.Interactable +
                ";sibling=" + value.SiblingIndex + "/" + value.SiblingCount +
                ";root=" + value.MainMenuRootPath;
        }

        private static string HierarchyPath(Transform transform)
        {
            var names = new Stack<string>();
            for (Transform item = transform; item != null; item = item.parent)
                names.Push(item.gameObject.name);
            return string.Join("/", names.ToArray());
        }

        private static bool IsSaveInfoList(Type type)
        {
            return type.IsGenericType &&
                type.GetGenericTypeDefinition() == typeof(List<>) &&
                type.GetGenericArguments()[0].FullName ==
                    "Kingmaker.EntitySystem.Persistence.SaveInfo";
        }

        private static string RuntimeType(object value)
        {
            return value == null ? "<null>" : value.GetType().FullName;
        }

        private static List<string> Types(object[] arguments)
        {
            return arguments == null ? new List<string>() :
                arguments.Select(RuntimeType).ToList();
        }

        private static string FormatSignature(MethodBase method)
        {
            if (method == null) return "";
            return (method.DeclaringType == null ? "" :
                method.DeclaringType.FullName + ".") + method.Name + "(" +
                string.Join(",", method.GetParameters()
                    .Select(x => x.ParameterType.FullName).ToArray()) + ")" +
                (method is MethodInfo ? ":" +
                    ((MethodInfo)method).ReturnType.FullName : "");
        }
    }
}
