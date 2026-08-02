using System;
using System.Collections.Generic;

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
