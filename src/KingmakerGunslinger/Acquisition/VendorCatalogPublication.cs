using System;
using System.Collections.Generic;
using System.Linq;

namespace KingmakerGunslinger.Acquisition
{
    internal sealed class VendorCatalogPublication<T> where T : class
    {
        private readonly T[] _original;
        private readonly T[] _published;
        private bool _rolledBack;

        private VendorCatalogPublication(T[] original, T[] published, bool changed)
        {
            _original = original;
            _published = published;
            Changed = changed;
        }

        internal bool Changed { get; private set; }
        internal T[] Published { get { return (T[])_published.Clone(); } }

        internal static VendorCatalogPublication<T> Create(
            T[] existing, T[] additions)
        {
            if (existing == null) throw new ArgumentNullException("existing");
            if (additions == null) throw new ArgumentNullException("additions");
            var seen = new HashSet<T>(ReferenceComparer<T>.Instance);
            foreach (T value in existing)
            {
                if (value == null || !seen.Add(value))
                    throw new InvalidOperationException(
                        "The native vendor catalog contains a null or duplicate reference.");
            }
            bool allPresent = true;
            foreach (T value in additions)
            {
                if (value == null)
                    throw new InvalidOperationException(
                        "A vendor addition cannot be null.");
                if (!seen.Add(value))
                {
                    if (Array.IndexOf(existing, value) < 0)
                        throw new InvalidOperationException(
                            "The requested vendor additions contain a duplicate reference.");
                }
                else allPresent = false;
            }
            if (allPresent)
                return new VendorCatalogPublication<T>(
                    (T[])existing.Clone(), (T[])existing.Clone(), false);
            foreach (T value in additions)
            {
                if (Array.IndexOf(existing, value) >= 0)
                    throw new InvalidOperationException(
                        "A partially published vendor catalog is ambiguous.");
            }
            var published = new T[existing.Length + additions.Length];
            Array.Copy(existing, published, existing.Length);
            Array.Copy(additions, 0, published, existing.Length, additions.Length);
            return new VendorCatalogPublication<T>(
                (T[])existing.Clone(), published, true);
        }

        // Normalize only exact owned blueprint identities. Detached construction
        // completes before the caller assigns a table; rollback retains every row.
        internal static VendorCatalogPublication<T> NormalizeOwned<TItem>(
            T[] existing, TItem[] owned, TItem[] items, int[] counts,
            Func<T, TItem> readItem, Func<T, int> readCount,
            Func<TItem, int, T> create) where TItem : class
        {
            if (existing == null || owned == null || items == null || counts == null ||
                readItem == null || readCount == null || create == null)
                throw new ArgumentNullException("Vendor normalization input");
            // Reuse the native-catalog null/duplicate-reference conflict guard.
            Create(existing, Array.Empty<T>());
            var ownedSet = new HashSet<TItem>(owned, ReferenceComparer<TItem>.Instance);
            var desired = new HashSet<TItem>(ReferenceComparer<TItem>.Instance);
            if (ownedSet.Contains(null) || items.Length != counts.Length)
                throw new InvalidOperationException("Invalid owned vendor stock contract.");
            for (int index = 0; index < items.Length; index++)
                if (items[index] == null || !ownedSet.Contains(items[index]) ||
                    !desired.Add(items[index]) || counts[index] <= 0)
                    throw new InvalidOperationException("Invalid desired vendor stock contract.");
            bool exact = !existing.Any(row => ownedSet.Contains(readItem(row)) &&
                !desired.Contains(readItem(row)));
            for (int index = 0; index < items.Length && exact; index++)
            {
                TItem item = items[index];
                T[] matches = existing.Where(row => ReferenceEquals(readItem(row), item)).ToArray();
                exact = matches.Length == 1 && readCount(matches[0]) == counts[index];
            }
            if (exact) return Create(existing, Array.Empty<T>());
            T[] retained = existing.Where(row => !ownedSet.Contains(readItem(row))).ToArray();
            T[] additions = items.Select((item, index) => create(item, counts[index])).ToArray();
            // Fail before publication if a native row factory cannot round-trip.
            for (int index = 0; index < additions.Length; index++)
                if (additions[index] == null ||
                    !ReferenceEquals(readItem(additions[index]), items[index]) ||
                    readCount(additions[index]) != counts[index])
                    throw new InvalidOperationException("Vendor row factory did not round-trip.");
            T[] published = Create(retained, additions).Published;
            return new VendorCatalogPublication<T>((T[])existing.Clone(), published, true);
        }

        internal static VendorCatalogPublication<T> CreateIntegrated(
            T[] existing, T[] additions, Func<T, string> sortKey)
        {
            if (existing == null) throw new ArgumentNullException("existing");
            if (additions == null) throw new ArgumentNullException("additions");
            if (sortKey == null) throw new ArgumentNullException("sortKey");
            var seen = new HashSet<T>(ReferenceComparer<T>.Instance);
            foreach (T value in existing)
            {
                if (value == null || !seen.Add(value))
                    throw new InvalidOperationException(
                        "The native vendor catalog contains a null or duplicate reference.");
            }
            int present = 0;
            foreach (T value in additions)
            {
                if (value == null)
                    throw new InvalidOperationException(
                        "A vendor addition cannot be null.");
                if (!seen.Add(value))
                {
                    if (Array.IndexOf(existing, value) < 0)
                        throw new InvalidOperationException(
                            "The requested vendor additions contain a duplicate reference.");
                    present++;
                }
            }
            if (present == additions.Length)
                return new VendorCatalogPublication<T>((T[])existing.Clone(),
                    (T[])existing.Clone(), false);
            if (present != 0)
                throw new InvalidOperationException(
                    "A partially published vendor catalog is ambiguous.");

            var published = new List<T>(existing);
            foreach (T addition in additions.OrderBy(value => sortKey(value),
                StringComparer.Ordinal))
            {
                string additionKey = sortKey(addition);
                int insertion = published.Count;
                for (int index = 0; index < published.Count; index++)
                {
                    string existingKey = sortKey(published[index]);
                    if (existingKey != null && additionKey != null &&
                        string.Compare(existingKey, additionKey,
                            StringComparison.Ordinal) > 0)
                    {
                        insertion = index;
                        break;
                    }
                }
                published.Insert(insertion, addition);
            }
            return new VendorCatalogPublication<T>((T[])existing.Clone(),
                published.ToArray(), true);
        }

        internal T[] Rollback()
        {
            if (_rolledBack)
                throw new InvalidOperationException("Vendor catalog rollback was already consumed.");
            _rolledBack = true;
            return (T[])_original.Clone();
        }

        private sealed class ReferenceComparer<TValue> : IEqualityComparer<TValue>
            where TValue : class
        {
            internal static readonly ReferenceComparer<TValue> Instance =
                new ReferenceComparer<TValue>();
            public bool Equals(TValue x, TValue y) { return ReferenceEquals(x, y); }
            public int GetHashCode(TValue obj)
            {
                return System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(obj);
            }
        }
    }
}
