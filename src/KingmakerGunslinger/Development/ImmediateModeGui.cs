using System;
using System.Globalization;
using System.Reflection;
using KingmakerGunslinger.Bootstrap;

namespace KingmakerGunslinger.Development
{
    /// <summary>
    /// Reflection-only adapter for Unity's immediate-mode GUILayout surface.
    ///
    /// Kingmaker splits GUILayout into UnityEngine.IMGUIModule.dll. The private
    /// build-reference handoff intentionally omits that optional module, so the
    /// development panel resolves it from the running game instead of creating
    /// a compile-time dependency. Missing or incompatible IMGUI contracts fail
    /// closed: labels become no-ops, buttons remain unpressed, toggles retain
    /// their current value, and text fields retain their current text.
    /// </summary>
    internal static class ImmediateModeGui
    {
        private static readonly object Gate = new object();

        private static bool _initializationAttempted;
        private static bool _available;
        private static bool _failureLogged;
        private static object _emptyOptions;
        private static MethodInfo _label;
        private static MethodInfo _toggle;
        private static MethodInfo _space;
        private static MethodInfo _beginHorizontal;
        private static MethodInfo _endHorizontal;
        private static MethodInfo _button;
        private static MethodInfo _textField;
        private static string _failureMessage = string.Empty;

        internal static void Label(string text)
        {
            InvokeVoid(_label, new object[] { text ?? string.Empty, _emptyOptions });
        }

        internal static bool Toggle(bool value, string text)
        {
            object result = Invoke(
                _toggle,
                value,
                new object[] { value, text ?? string.Empty, _emptyOptions });
            return result is bool && (bool)result;
        }

        internal static void Space(float pixels)
        {
            InvokeVoid(_space, new object[] { pixels });
        }

        internal static void BeginHorizontal()
        {
            InvokeVoid(_beginHorizontal, new object[] { _emptyOptions });
        }

        internal static void EndHorizontal()
        {
            InvokeVoid(_endHorizontal, new object[0]);
        }

        internal static bool Button(string text)
        {
            object result = Invoke(
                _button,
                false,
                new object[] { text ?? string.Empty, _emptyOptions });
            return result is bool && (bool)result;
        }

        internal static string TextField(string text)
        {
            string fallback = text ?? string.Empty;
            object result = Invoke(
                _textField,
                fallback,
                new object[] { fallback, _emptyOptions });
            return result as string ?? fallback;
        }

        private static void InvokeVoid(MethodInfo method, object[] arguments)
        {
            Invoke(method, null, arguments);
        }

        private static object Invoke(MethodInfo method, object fallback, object[] arguments)
        {
            EnsureInitialized();
            lock (Gate)
            {
                if (!_available || method == null)
                {
                    LogFailureOnceLocked();
                    return fallback;
                }

                try
                {
                    return method.Invoke(null, arguments);
                }
                catch (Exception exception)
                {
                    _available = false;
                    _failureMessage = string.Format(
                        CultureInfo.InvariantCulture,
                        "Unity IMGUI invocation failed closed. Method={0}; Exception={1}: {2}",
                        method.Name,
                        exception.GetType().FullName,
                        exception.Message);
                    LogFailureOnceLocked();
                    return fallback;
                }
            }
        }

        private static void EnsureInitialized()
        {
            lock (Gate)
            {
                if (_initializationAttempted)
                {
                    return;
                }

                _initializationAttempted = true;
                try
                {
                    Type layoutType = ResolveType("UnityEngine.GUILayout", "UnityEngine.IMGUIModule");
                    Type optionType = ResolveType("UnityEngine.GUILayoutOption", "UnityEngine.IMGUIModule");
                    Type optionArrayType = optionType.MakeArrayType();
                    _emptyOptions = Array.CreateInstance(optionType, 0);

                    const BindingFlags Flags = BindingFlags.Public | BindingFlags.Static;
                    _label = RequireMethod(
                        layoutType,
                        "Label",
                        Flags,
                        new[] { typeof(string), optionArrayType });
                    _toggle = RequireMethod(
                        layoutType,
                        "Toggle",
                        Flags,
                        new[] { typeof(bool), typeof(string), optionArrayType });
                    _space = RequireMethod(
                        layoutType,
                        "Space",
                        Flags,
                        new[] { typeof(float) });
                    _beginHorizontal = RequireMethod(
                        layoutType,
                        "BeginHorizontal",
                        Flags,
                        new[] { optionArrayType });
                    _endHorizontal = RequireMethod(
                        layoutType,
                        "EndHorizontal",
                        Flags,
                        Type.EmptyTypes);
                    _button = RequireMethod(
                        layoutType,
                        "Button",
                        Flags,
                        new[] { typeof(string), optionArrayType });
                    _textField = RequireMethod(
                        layoutType,
                        "TextField",
                        Flags,
                        new[] { typeof(string), optionArrayType });

                    _available = true;
                }
                catch (Exception exception)
                {
                    _available = false;
                    _failureMessage = string.Format(
                        CultureInfo.InvariantCulture,
                        "Unity IMGUI contract is unavailable; the development panel is disabled. Exception={0}: {1}",
                        exception.GetType().FullName,
                        exception.Message);
                    LogFailureOnceLocked();
                }
            }
        }

        private static Type ResolveType(string fullName, string assemblyName)
        {
            Type type = Type.GetType(fullName + ", " + assemblyName, false);
            if (type != null)
            {
                return type;
            }

            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            for (int index = 0; index < assemblies.Length; index++)
            {
                type = assemblies[index].GetType(fullName, false);
                if (type != null)
                {
                    return type;
                }
            }

            Assembly assembly = Assembly.Load(assemblyName);
            type = assembly.GetType(fullName, false);
            if (type == null)
            {
                throw new TypeLoadException(
                    "Assembly '" + assemblyName + "' does not expose type '" + fullName + "'.");
            }

            return type;
        }

        private static MethodInfo RequireMethod(
            Type declaringType,
            string name,
            BindingFlags flags,
            Type[] parameterTypes)
        {
            MethodInfo method = declaringType.GetMethod(
                name,
                flags,
                null,
                parameterTypes,
                null);
            if (method == null)
            {
                throw new MissingMethodException(
                    declaringType.FullName,
                    name + "(" + FormatTypes(parameterTypes) + ")");
            }

            return method;
        }

        private static string FormatTypes(Type[] parameterTypes)
        {
            if (parameterTypes == null || parameterTypes.Length == 0)
            {
                return string.Empty;
            }

            string[] names = new string[parameterTypes.Length];
            for (int index = 0; index < parameterTypes.Length; index++)
            {
                names[index] = parameterTypes[index].FullName;
            }

            return string.Join(", ", names);
        }

        private static void LogFailureOnceLocked()
        {
            if (_failureLogged || string.IsNullOrWhiteSpace(_failureMessage))
            {
                return;
            }

            _failureLogged = true;
            ModContext context;
            if (ModContext.TryGet(out context))
            {
                context.Logger.Warning("development", "imgui.unavailable", _failureMessage);
            }
        }
    }
}
