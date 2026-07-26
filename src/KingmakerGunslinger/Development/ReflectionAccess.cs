using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace KingmakerGunslinger.Development
{
    /// <summary>
    /// Narrow reflection helpers for development-only game controls. Runtime API
    /// differences fail closed with explicit diagnostics instead of guessing silently.
    /// </summary>
    internal static class ReflectionAccess
    {
        internal static bool TryGetMember(object source, string name, out object value)
        {
            value = null;
            if (source == null || string.IsNullOrWhiteSpace(name))
            {
                return false;
            }

            Type type = source as Type ?? source.GetType();
            BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic |
                (source is Type ? BindingFlags.Static : BindingFlags.Instance);

            for (Type current = type; current != null; current = current.BaseType)
            {
                PropertyInfo property = current.GetProperty(
                    name,
                    flags | BindingFlags.DeclaredOnly);
                if (property != null && property.GetIndexParameters().Length == 0)
                {
                    MethodInfo getter = property.GetGetMethod(true);
                    if (getter != null)
                    {
                        try
                        {
                            value = getter.Invoke(source is Type ? null : source, null);
                            return true;
                        }
                        catch (TargetInvocationException)
                        {
                            return false;
                        }
                    }
                }

                FieldInfo field = current.GetField(
                    name,
                    flags | BindingFlags.DeclaredOnly);
                if (field != null)
                {
                    value = field.GetValue(source is Type ? null : source);
                    return true;
                }
            }

            return false;
        }

        internal static bool TryGetFirstMember(
            object source,
            IEnumerable<string> names,
            out object value,
            out string resolvedName)
        {
            value = null;
            resolvedName = null;
            if (names == null)
            {
                return false;
            }

            foreach (string name in names)
            {
                if (TryGetMember(source, name, out value))
                {
                    resolvedName = name;
                    return true;
                }
            }

            return false;
        }

        internal static bool TryGetFirstNonNullMember(
            object source,
            IEnumerable<string> names,
            out object value,
            out string resolvedName)
        {
            value = null;
            resolvedName = null;
            if (names == null)
            {
                return false;
            }

            foreach (string name in names)
            {
                object candidate;
                if (TryGetMember(source, name, out candidate) && candidate != null)
                {
                    value = candidate;
                    resolvedName = name;
                    return true;
                }
            }

            return false;
        }

        internal static bool TryGetPath(object source, string path, out object value)
        {
            value = source;
            if (source == null || string.IsNullOrWhiteSpace(path))
            {
                value = null;
                return false;
            }

            string[] segments = path.Split('.');
            foreach (string segment in segments)
            {
                object next;
                if (!TryGetMember(value, segment, out next) || next == null)
                {
                    value = null;
                    return false;
                }

                value = next;
            }

            return true;
        }

        internal static bool CanEnumerate(object source)
        {
            return source != null && !(source is string) && source is IEnumerable;
        }

        internal static IEnumerable<object> Enumerate(object source)
        {
            if (!CanEnumerate(source))
            {
                yield break;
            }

            IEnumerable enumerable = (IEnumerable)source;
            if (enumerable == null)
            {
                yield break;
            }

            IEnumerator enumerator = null;
            try
            {
                enumerator = enumerable.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    if (enumerator.Current != null)
                    {
                        yield return enumerator.Current;
                    }
                }
            }
            finally
            {
                IDisposable disposable = enumerator as IDisposable;
                if (disposable != null)
                {
                    disposable.Dispose();
                }
            }
        }

        internal static bool TryInvokeAny(
            object target,
            IEnumerable<string> methodNames,
            IEnumerable<object[]> argumentSets,
            out object result,
            out string resolvedMethod)
        {
            result = null;
            resolvedMethod = null;
            if (target == null || methodNames == null || argumentSets == null)
            {
                return false;
            }

            string[] names = methodNames.ToArray();
            object[][] sets = argumentSets.ToArray();
            foreach (string name in names)
            {
                if (string.IsNullOrWhiteSpace(name))
                {
                    continue;
                }

                foreach (object[] supplied in sets)
                {
                    MethodInfo method;
                    object[] invocationArguments;
                    if (!TryResolveMethod(
                        target.GetType(),
                        name,
                        supplied ?? Array.Empty<object>(),
                        out method,
                        out invocationArguments))
                    {
                        continue;
                    }

                    try
                    {
                        result = method.Invoke(target, invocationArguments);
                        resolvedMethod = string.Format(
                            CultureInfo.InvariantCulture,
                            "{0}.{1}",
                            method.DeclaringType == null ? "<unknown>" : method.DeclaringType.FullName,
                            method.Name);
                        return true;
                    }
                    catch (TargetInvocationException exception)
                    {
                        Exception inner = exception.InnerException ?? exception;
                        throw new InvalidOperationException(
                            string.Format(
                                CultureInfo.InvariantCulture,
                                "Runtime method {0}.{1} threw {2}: {3}",
                                method.DeclaringType == null ? "<unknown>" : method.DeclaringType.FullName,
                                method.Name,
                                inner.GetType().FullName,
                                inner.Message),
                            inner);
                    }
                }
            }

            return false;
        }

        private static bool TryResolveMethod(
            Type type,
            string name,
            object[] supplied,
            out MethodInfo resolved,
            out object[] invocationArguments)
        {
            resolved = null;
            invocationArguments = null;
            var candidates = new List<Tuple<MethodInfo, object[], int>>();

            for (Type current = type; current != null; current = current.BaseType)
            {
                MethodInfo[] methods = current.GetMethods(
                    BindingFlags.Instance |
                    BindingFlags.Public |
                    BindingFlags.NonPublic |
                    BindingFlags.DeclaredOnly);
                foreach (MethodInfo method in methods)
                {
                    if (!string.Equals(method.Name, name, StringComparison.Ordinal) ||
                        method.IsGenericMethodDefinition)
                    {
                        continue;
                    }

                    ParameterInfo[] parameters = method.GetParameters();
                    if (supplied.Length > parameters.Length ||
                        parameters.Any(parameter => parameter.IsOut || parameter.ParameterType.IsByRef))
                    {
                        continue;
                    }

                    object[] values = new object[parameters.Length];
                    int score = 0;
                    bool compatible = true;
                    for (int index = 0; index < parameters.Length; index++)
                    {
                        if (index < supplied.Length)
                        {
                            object argument = supplied[index];
                            if (!CanAssign(parameters[index].ParameterType, argument))
                            {
                                compatible = false;
                                break;
                            }

                            values[index] = argument;
                            score += argument != null && parameters[index].ParameterType == argument.GetType()
                                ? 4
                                : 2;
                        }
                        else if (parameters[index].HasDefaultValue)
                        {
                            values[index] = parameters[index].DefaultValue;
                            score += 1;
                        }
                        else if (!parameters[index].ParameterType.IsValueType ||
                            Nullable.GetUnderlyingType(parameters[index].ParameterType) != null)
                        {
                            values[index] = null;
                        }
                        else
                        {
                            compatible = false;
                            break;
                        }
                    }

                    if (compatible)
                    {
                        candidates.Add(Tuple.Create(method, values, score));
                    }
                }
            }

            if (candidates.Count == 0)
            {
                return false;
            }

            Tuple<MethodInfo, object[], int> selected = candidates
                .OrderByDescending(candidate => candidate.Item3)
                .ThenBy(candidate => candidate.Item1.GetParameters().Length)
                .ThenBy(candidate => candidate.Item1.MetadataToken)
                .First();
            resolved = selected.Item1;
            invocationArguments = selected.Item2;
            return true;
        }

        private static bool CanAssign(Type parameterType, object argument)
        {
            if (argument == null)
            {
                return !parameterType.IsValueType || Nullable.GetUnderlyingType(parameterType) != null;
            }

            Type argumentType = argument.GetType();
            if (parameterType.IsAssignableFrom(argumentType))
            {
                return true;
            }

            Type nullable = Nullable.GetUnderlyingType(parameterType);
            return nullable != null && nullable.IsAssignableFrom(argumentType);
        }
    }
}
