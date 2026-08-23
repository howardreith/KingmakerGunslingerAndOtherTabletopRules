using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace KingmakerGunslinger.AidAnotherCompatibility
{
    /// <summary>
    /// Transaction for KMG-owned mutations of optional-mod arrays. Each surface
    /// retains its exact original array reference and is restored in reverse order
    /// on either a partial failure or an explicit rollback.
    /// </summary>
    internal sealed class HelpfulPublicationTransaction
    {
        private readonly List<IMutation> _mutations = new List<IMutation>();
        private readonly HashSet<string> _keys = new HashSet<string>(
            StringComparer.Ordinal);
        private readonly List<string> _evidence = new List<string>();
        private bool _committed;
        private bool _rolledBack;

        internal IReadOnlyList<string> Evidence
        { get { return _evidence.AsReadOnly(); } }

        internal bool IsCommitted { get { return _committed; } }

        internal HelpfulPublicationTransaction Append<T>(string key,
            Func<T[]> read, Action<T[]> write, T addition,
            Func<T, string> identity, bool allowForeignMultiplicity)
            where T : class
        {
            Add(new AppendMutation<T>(key, read, write, addition, identity,
                allowForeignMultiplicity, Record));
            return this;
        }

        internal void Commit()
        {
            if (_committed) return;
            if (_rolledBack) throw new InvalidOperationException(
                "A rolled-back Helpful transaction cannot be recommitted.");
            int applied = -1;
            try
            {
                for (int index = 0; index < _mutations.Count; index++)
                {
                    applied = index;
                    _mutations[index].Apply();
                }
                _committed = true;
                Record("transaction=committed;surfaces=" + _mutations.Count);
            }
            catch (Exception failure)
            {
                var rollbackFailures = RollbackThrough(applied);
                if (rollbackFailures.Count != 0)
                {
                    rollbackFailures.Insert(0, failure);
                    throw new InvalidOperationException(
                        "Helpful publication failed and rollback was incomplete.",
                        new AggregateException(rollbackFailures));
                }
                throw;
            }
        }

        internal void Rollback()
        {
            if (_rolledBack) return;
            List<Exception> failures = RollbackThrough(_mutations.Count - 1);
            if (failures.Count != 0) throw new InvalidOperationException(
                "Helpful publication rollback was incomplete.",
                new AggregateException(failures));
            _committed = false;
            _rolledBack = true;
            Record("transaction=rolled-back;surfaces=" + _mutations.Count);
        }

        private void Add(IMutation mutation)
        {
            if (mutation == null) throw new ArgumentNullException("mutation");
            if (_committed || _rolledBack) throw new InvalidOperationException(
                "Helpful publication surfaces cannot change after execution.");
            if (!_keys.Add(mutation.Key)) throw new InvalidOperationException(
                "Duplicate Helpful publication surface: " + mutation.Key);
            _mutations.Add(mutation);
        }

        private List<Exception> RollbackThrough(int last)
        {
            var failures = new List<Exception>();
            for (int index = last; index >= 0; index--)
                try { _mutations[index].Rollback(); }
                catch (Exception exception)
                {
                    failures.Add(new InvalidOperationException(
                        "Rollback failed for Helpful surface '" +
                        _mutations[index].Key + "'.", exception));
                }
            return failures;
        }

        private void Record(string value) { _evidence.Add(value); }

        private interface IMutation
        {
            string Key { get; }
            void Apply();
            void Rollback();
        }

        private sealed class AppendMutation<T> : IMutation where T : class
        {
            private readonly Func<T[]> _read;
            private readonly Action<T[]> _write;
            private readonly T _addition;
            private readonly Func<T, string> _identity;
            private readonly bool _allowForeignMultiplicity;
            private readonly Action<string> _record;
            private T[] _before;
            private T[] _after;
            private bool _attempted;

            internal AppendMutation(string key, Func<T[]> read,
                Action<T[]> write, T addition, Func<T, string> identity,
                bool allowForeignMultiplicity, Action<string> record)
            {
                if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException(
                    "A Helpful surface key is required.", "key");
                Key = key;
                _read = read ?? throw new ArgumentNullException("read");
                _write = write ?? throw new ArgumentNullException("write");
                _addition = addition ?? throw new ArgumentNullException("addition");
                _identity = identity ?? throw new ArgumentNullException("identity");
                _allowForeignMultiplicity = allowForeignMultiplicity;
                _record = record;
            }

            public string Key { get; private set; }

            public void Apply()
            {
                _before = _read();
                T[] current = _before ?? new T[0];
                Validate(current);
                string additionId = Identity(_addition);
                T[] matches = current.Where(value => string.Equals(
                    Identity(value), additionId, StringComparison.Ordinal)).ToArray();
                if (matches.Length == 1 && ReferenceEquals(matches[0], _addition))
                {
                    _record("surface=" + Key + ";action=unchanged;count=" +
                        current.Length);
                    return;
                }
                if (matches.Length != 0)
                    throw new InvalidOperationException("Helpful surface '" +
                        Key + "' contains a conflicting or duplicate identity: " +
                        additionId);
                _after = current.Concat(new[] { _addition }).ToArray();
                _attempted = true;
                _write(_after);
                if (!SameReferences(_read(), _after))
                    throw new InvalidOperationException(
                        "Helpful publication write was not retained: " + Key);
                _record("surface=" + Key + ";action=published;before=" +
                    current.Length + ";after=" + _after.Length);
            }

            public void Rollback()
            {
                if (!_attempted) return;
                T[] current = _read();
                if (SameReferences(current, _before))
                {
                    _attempted = false;
                    return;
                }
                if (!StartsWithReferences(current, _after))
                    throw new InvalidOperationException(
                        "Helpful rollback refused after an unrelated mutation: " +
                        Key);
                T[] restored;
                if (SameReferences(current, _after)) restored = _before;
                else restored = (_before ?? new T[0]).Concat(current.Skip(
                    _after.Length)).ToArray();
                Validate(restored ?? new T[0]);
                _write(restored);
                T[] observed = _read();
                bool exactOriginal = ReferenceEquals(restored, _before) &&
                    ReferenceEquals(observed, _before);
                if (!exactOriginal && !SameReferences(observed, restored))
                    throw new InvalidOperationException(
                        "Helpful rollback did not restore its proven array state: " +
                        Key);
                _attempted = false;
                _record("surface=" + Key + ";action=rolled-back;preserved-later=" +
                    (restored == null ? 0 : restored.Length -
                        (_before == null ? 0 : _before.Length)));
            }

            private void Validate(IEnumerable<T> values)
            {
                var identities = new HashSet<string>(StringComparer.Ordinal);
                var references = new HashSet<T>(ReferenceComparer<T>.Instance);
                foreach (T value in values)
                {
                    string id = Identity(value);
                    if (!_allowForeignMultiplicity &&
                        (!identities.Add(id) || !references.Add(value)))
                        throw new InvalidOperationException("Helpful surface '" +
                            Key + "' contains a duplicate identity: " + id);
                }
            }

            private string Identity(T value)
            {
                if (value == null) throw new InvalidOperationException(
                    "Helpful surface '" + Key + "' contains null.");
                string result = _identity(value);
                if (string.IsNullOrWhiteSpace(result))
                    throw new InvalidOperationException("Helpful surface '" +
                        Key + "' contains a blank identity.");
                return result;
            }

            private static bool SameReferences(T[] left, T[] right)
            {
                if (left == null || right == null) return left == right;
                if (ReferenceEquals(left, right)) return true;
                if (left.Length != right.Length)
                    return false;
                for (int index = 0; index < left.Length; index++)
                    if (!ReferenceEquals(left[index], right[index])) return false;
                return true;
            }

            private static bool StartsWithReferences(T[] values, T[] prefix)
            {
                if (values == null || prefix == null ||
                    values.Length < prefix.Length) return false;
                for (int index = 0; index < prefix.Length; index++)
                    if (!ReferenceEquals(values[index], prefix[index]))
                        return false;
                return true;
            }
        }

        private sealed class ReferenceComparer<T> : IEqualityComparer<T>
            where T : class
        {
            internal static readonly ReferenceComparer<T> Instance =
                new ReferenceComparer<T>();
            public bool Equals(T left, T right)
            { return ReferenceEquals(left, right); }
            public int GetHashCode(T value)
            { return RuntimeHelpers.GetHashCode(value); }
        }
    }
}
