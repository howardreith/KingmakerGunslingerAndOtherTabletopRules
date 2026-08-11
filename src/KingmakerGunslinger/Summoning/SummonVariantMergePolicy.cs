using System;
using System.Collections.Generic;

namespace KingmakerGunslinger.Summoning
{
    internal static class SummonVariantMergePolicy
    {
        internal static IList<T> Merge<T>(IList<T> current, IEnumerable<T> additions,
            Func<T, string> guid)
            where T : class
        {
            if (current == null) throw new ArgumentNullException("current");
            if (additions == null) throw new ArgumentNullException("additions");
            if (guid == null) throw new ArgumentNullException("guid");
            var result = new List<T>();
            var references = new HashSet<T>(ReferenceComparer<T>.Instance);
            var guids = new HashSet<string>(StringComparer.Ordinal);
            AddUnique(current, result, references, guids, guid, false);
            AddUnique(additions, result, references, guids, guid, true);
            bool same = result.Count == current.Count;
            if (same) for (int index = 0; index < result.Count; index++)
                if (!ReferenceEquals(result[index], current[index])) { same = false; break; }
            return same ? current : result;
        }

        internal static bool SameReferences<T>(IList<T> left, IList<T> right)
            where T : class
        {
            if (left == null || right == null || left.Count != right.Count) return false;
            for (int index = 0; index < left.Count; index++)
                if (!ReferenceEquals(left[index], right[index])) return false;
            return true;
        }

        private static void AddUnique<T>(IEnumerable<T> source, IList<T> result,
            ISet<T> references, ISet<string> guids, Func<T, string> guid,
            bool rejectDuplicateAdditions) where T : class
        {
            foreach (T value in source)
            {
                if (value == null) throw new InvalidOperationException("Variant collection contains null.");
                string id = guid(value);
                if (string.IsNullOrWhiteSpace(id)) throw new InvalidOperationException("Variant GUID is missing.");
                bool newReference = references.Add(value);
                bool newGuid = guids.Add(id);
                if (newReference && newGuid) { result.Add(value); continue; }
                if (rejectDuplicateAdditions && (newReference != newGuid))
                    throw new InvalidOperationException("Addition conflicts by reference or GUID.");
            }
        }

        private sealed class ReferenceComparer<T> : IEqualityComparer<T> where T : class
        {
            internal static readonly ReferenceComparer<T> Instance = new ReferenceComparer<T>();
            public bool Equals(T x, T y) { return ReferenceEquals(x, y); }
            public int GetHashCode(T obj) { return System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(obj); }
        }
    }
}
