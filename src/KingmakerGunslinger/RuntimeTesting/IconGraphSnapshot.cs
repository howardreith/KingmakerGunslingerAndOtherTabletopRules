using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace KingmakerGunslinger.RuntimeTesting
{
    // Iterative, read-only observation of explicitly permitted action containers.
    // Terminal references are described but never traversed. Shared/cyclic
    // containers point to their first path instead of exhausting a depth limit.
    internal static class IconGraphSnapshot
    {
        private sealed class IdentityComparer : IEqualityComparer<object>
        {
            public new bool Equals(object left, object right) => ReferenceEquals(left, right);
            public int GetHashCode(object value) => RuntimeHelpers.GetHashCode(value);
        }

        internal static IEnumerable<string> Capture(object root, string rootPath,
            Func<Type, bool> isReference, Func<object, string> describeReference,
            Func<Type, bool> isContainer, int maximumNodes = 4096)
        {
            var rows = new List<string>();
            var seen = new Dictionary<object, string>(new IdentityComparer());
            var pending = new Queue<KeyValuePair<string, object>>();
            Action<object, string> enqueue = (value, path) => {
                if (value == null) { rows.Add(path + "=null"); return; }
                string first;
                if (seen.TryGetValue(value, out first))
                {
                    rows.Add(path + "=@" + first);
                    return;
                }
                if (seen.Count >= maximumNodes)
                    throw new InvalidOperationException("Icon graph node budget exceeded at " + path +
                        " (" + value.GetType().FullName + ").");
                seen.Add(value, path);
                rows.Add(path + ".type=" + value.GetType().FullName);
                pending.Enqueue(new KeyValuePair<string, object>(path, value));
            };
            Action<object, Type, string> observe = (value, declaredType, path) => {
                if (isReference(declaredType) || (value != null && isReference(value.GetType())))
                    rows.Add(path + "=" + (value == null ? "null" : describeReference(value)));
                else if (isContainer(declaredType) || (value != null && isContainer(value.GetType())))
                    enqueue(value, path);
            };
            enqueue(root, rootPath);
            while (pending.Count != 0)
            {
                var current = pending.Dequeue();
                foreach (var field in current.Value.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public)
                    .OrderBy(value => value.Name, StringComparer.Ordinal))
                {
                    object child = field.GetValue(current.Value);
                    string path = current.Key + "." + field.Name;
                    if (field.FieldType.IsArray)
                    {
                        var array = child as Array;
                        Type element = field.FieldType.GetElementType();
                        if (!isReference(element) && !isContainer(element)) continue;
                        rows.Add(path + ".count=" + (array == null ? "null" : array.Length.ToString()));
                        if (array == null) continue;
                        if (array.Rank != 1) throw new InvalidOperationException("Unexpected icon graph array rank at " + path);
                        for (int index = 0; index < array.Length; index++)
                            observe(array.GetValue(index), element, path + "[" + index + "]");
                    }
                    else observe(child, field.FieldType, path);
                }
            }
            return rows;
        }
    }
}
