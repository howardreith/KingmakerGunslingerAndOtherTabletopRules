using System;
using System.Collections.Generic;
using System.Linq;

namespace KingmakerGunslinger.BrownFur
{
    /// <summary>
    /// Brown-Fur-owned transaction for optional blueprint registration and
    /// publication surfaces.  It preserves foreign ordering, records every
    /// mutation, and rolls back only state that it can still prove it owns.
    /// </summary>
    internal sealed class BrownFurPublicationTransaction
    {
        private readonly List<IMutation> _mutations = new List<IMutation>();
        private readonly List<string> _evidence = new List<string>();
        private readonly HashSet<string> _keys = new HashSet<string>(
            StringComparer.Ordinal);
        private bool _committed;
        private bool _rolledBack;

        internal IReadOnlyList<string> Evidence
        { get { return _evidence.AsReadOnly(); } }

        internal bool IsCommitted { get { return _committed; } }

        internal BrownFurPublicationTransaction Append<T>(string key,
            Func<IList<T>> read, Action<IList<T>> write,
            IEnumerable<T> additions, Func<T, string> identity)
            where T : class
        {
            Add(new AppendMutation<T>(key, read, write, additions, identity,
                Record));
            return this;
        }

        internal BrownFurPublicationTransaction InsertBefore<T>(string key,
            Func<IList<T>> read, Action<IList<T>> write,
            IEnumerable<T> additions, Func<T, string> identity,
            Func<T, bool> boundary)
            where T : class
        {
            Add(new InsertBeforeMutation<T>(key, read, write, additions,
                identity, boundary, Record));
            return this;
        }

        internal BrownFurPublicationTransaction Configure<T>(string key,
            Func<T> read, Action<T> write, T configured,
            IEqualityComparer<T> comparer)
        {
            Add(new ValueMutation<T>(key, read, write, configured,
                comparer ?? EqualityComparer<T>.Default, Record));
            return this;
        }

        internal BrownFurPublicationTransaction Step(string key,
            Action apply, Action rollback)
        {
            Add(new ActionMutation(key, apply, rollback, Record));
            return this;
        }

        internal void Commit()
        {
            if (_committed) return;
            if (_rolledBack) throw new InvalidOperationException(
                "A rolled-back Brown-Fur publication transaction cannot be recommitted.");
            int attempted = -1;
            try
            {
                for (int index = 0; index < _mutations.Count; index++)
                {
                    attempted = index;
                    _mutations[index].Apply();
                }
                _committed = true;
                Record("transaction;action=committed;surfaces=" +
                    _mutations.Count);
            }
            catch (Exception publicationFailure)
            {
                List<Exception> rollbackFailures = RollbackThrough(attempted);
                if (rollbackFailures.Count != 0)
                {
                    rollbackFailures.Insert(0, publicationFailure);
                    throw new InvalidOperationException(
                        "Brown-Fur publication failed and rollback was incomplete.",
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
                "Brown-Fur publication rollback was incomplete.",
                new AggregateException(failures));
            _committed = false;
            _rolledBack = true;
            Record("transaction;action=rolled-back;surfaces=" +
                _mutations.Count);
        }

        private void Add(IMutation mutation)
        {
            if (mutation == null) throw new ArgumentNullException("mutation");
            if (_committed || _rolledBack) throw new InvalidOperationException(
                "Brown-Fur publication surfaces cannot change after execution.");
            if (!_keys.Add(mutation.Key)) throw new InvalidOperationException(
                "Duplicate Brown-Fur publication surface: " + mutation.Key);
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
                        "Rollback failed for Brown-Fur surface '" +
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
            private readonly Func<IList<T>> _read;
            private readonly Action<IList<T>> _write;
            private readonly T[] _additions;
            private readonly Func<T, string> _identity;
            private readonly Action<string> _record;
            private IList<T> _before;
            private IList<T> _after;
            private bool _attempted;

            internal AppendMutation(string key, Func<IList<T>> read,
                Action<IList<T>> write, IEnumerable<T> additions,
                Func<T, string> identity, Action<string> record)
            {
                if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException(
                    "A publication surface key is required.", "key");
                Key = key;
                _read = read ?? throw new ArgumentNullException("read");
                _write = write ?? throw new ArgumentNullException("write");
                if (additions == null) throw new ArgumentNullException("additions");
                _additions = additions.ToArray();
                _identity = identity ?? throw new ArgumentNullException("identity");
                _record = record;
            }

            public string Key { get; private set; }

            public void Apply()
            {
                _before = _read();
                ValidateUnique(_before, "current");
                ValidateUnique(_additions, "additions");
                var byIdentity = _before.ToDictionary(_identity,
                    StringComparer.Ordinal);
                var next = new List<T>(_before);
                foreach (T addition in _additions)
                {
                    string id = Identity(addition);
                    T existing;
                    if (!byIdentity.TryGetValue(id, out existing))
                    {
                        next.Add(addition);
                        byIdentity.Add(id, addition);
                    }
                    else if (!ReferenceEquals(existing, addition))
                        throw new InvalidOperationException("Brown-Fur surface '" +
                            Key + "' contains a conflicting identity: " + id);
                }
                if (SameReferences(_before, next))
                {
                    _record("surface=" + Key + ";action=unchanged;count=" +
                        _before.Count);
                    return;
                }
                _after = next;
                _attempted = true;
                _write(_after);
                if (!SameReferences(_read(), _after)) throw new
                    InvalidOperationException("Brown-Fur publication write was not retained: " + Key);
                _record("surface=" + Key + ";action=published;before=" +
                    _before.Count + ";after=" + _after.Count);
            }

            public void Rollback()
            {
                if (!_attempted) return;
                IList<T> current = _read();
                if (SameReferences(current, _before))
                {
                    _attempted = false;
                    return;
                }
                if (!StartsWithReferences(current, _after)) throw new
                    InvalidOperationException("Rollback refused after an unrelated mutation: " + Key);
                IList<T> restored;
                if (SameReferences(current, _after)) restored = _before;
                else
                {
                    var withLaterAppends = new List<T>(_before);
                    for (int index = _after.Count; index < current.Count; index++)
                        withLaterAppends.Add(current[index]);
                    restored = withLaterAppends;
                }
                ValidateUnique(restored, "rollback");
                _write(restored);
                if (!SameReferences(_read(), restored)) throw new
                    InvalidOperationException("Brown-Fur rollback write was not retained: " + Key);
                _attempted = false;
                _record("surface=" + Key + ";action=rolled-back;restored=" +
                    _before.Count + ";preserved-later=" +
                    (restored.Count - _before.Count));
            }

            private void ValidateUnique(IEnumerable<T> values, string source)
            {
                if (values == null) throw new InvalidOperationException(
                    "Brown-Fur surface '" + Key + "' has no " + source + " collection.");
                var identities = new HashSet<string>(StringComparer.Ordinal);
                var references = new HashSet<T>(ReferenceComparer<T>.Instance);
                foreach (T value in values)
                {
                    string id = Identity(value);
                    if (!identities.Add(id) || !references.Add(value)) throw new
                        InvalidOperationException("Brown-Fur surface '" + Key +
                            "' contains a duplicate " + source + " identity: " + id);
                }
            }

            private string Identity(T value)
            {
                if (value == null) throw new InvalidOperationException(
                    "Brown-Fur surface '" + Key + "' contains null.");
                string id = _identity(value);
                if (string.IsNullOrWhiteSpace(id)) throw new InvalidOperationException(
                    "Brown-Fur surface '" + Key + "' contains a blank identity.");
                return id;
            }
        }

        private sealed class InsertBeforeMutation<T> : IMutation
            where T : class
        {
            private readonly Func<IList<T>> _read;
            private readonly Action<IList<T>> _write;
            private readonly T[] _additions;
            private readonly Func<T, string> _identity;
            private readonly Func<T, bool> _boundary;
            private readonly Action<string> _record;
            private IList<T> _before;
            private IList<T> _after;
            private bool _attempted;

            internal InsertBeforeMutation(string key, Func<IList<T>> read,
                Action<IList<T>> write, IEnumerable<T> additions,
                Func<T, string> identity, Func<T, bool> boundary,
                Action<string> record)
            {
                if (string.IsNullOrWhiteSpace(key))
                    throw new ArgumentException(
                        "A publication surface key is required.", "key");
                if (read == null || write == null || additions == null ||
                    identity == null || boundary == null)
                    throw new ArgumentNullException(
                        "Ordered publication inputs are incomplete.");
                Key = key;
                _read = read;
                _write = write;
                _additions = additions.ToArray();
                _identity = identity;
                _boundary = boundary;
                _record = record;
            }

            public string Key { get; private set; }

            public void Apply()
            {
                _before = _read();
                ValidateUnique(_before, "current");
                ValidateUnique(_additions, "additions");
                var additionsByIdentity = _additions.ToDictionary(Identity,
                    StringComparer.Ordinal);
                foreach (T current in _before)
                {
                    T addition;
                    string currentIdentity = Identity(current);
                    if (additionsByIdentity.TryGetValue(currentIdentity,
                            out addition) && !ReferenceEquals(current, addition))
                        throw new InvalidOperationException(
                            "Brown-Fur surface '" + Key +
                            "' contains a conflicting identity: " +
                            currentIdentity);
                }

                var foreign = _before.Where(value =>
                    !additionsByIdentity.ContainsKey(Identity(value))).ToList();
                int insertion = foreign.FindIndex(value => _boundary(value));
                if (insertion >= 0)
                {
                    foreign.InsertRange(insertion, _additions);
                    PublishIfChanged(foreign, insertion, true);
                    return;
                }

                var appendOnly = new List<T>(_before);
                var present = new HashSet<string>(_before.Select(Identity),
                    StringComparer.Ordinal);
                foreach (T addition in _additions)
                    if (present.Add(Identity(addition)))
                        appendOnly.Add(addition);
                PublishIfChanged(appendOnly, _before.Count, false);
            }

            public void Rollback()
            {
                if (!_attempted) return;
                IList<T> current = _read();
                if (SameReferences(current, _before))
                {
                    _attempted = false;
                    return;
                }
                if (!StartsWithReferences(current, _after))
                    throw new InvalidOperationException(
                        "Rollback refused after an unrelated mutation: " + Key);
                IList<T> restored;
                if (SameReferences(current, _after)) restored = _before;
                else
                {
                    var withLaterAppends = new List<T>(_before);
                    for (int index = _after.Count; index < current.Count; index++)
                        withLaterAppends.Add(current[index]);
                    restored = withLaterAppends;
                }
                ValidateUnique(restored, "rollback");
                _write(restored);
                if (!SameReferences(_read(), restored))
                    throw new InvalidOperationException(
                        "Brown-Fur rollback write was not retained: " + Key);
                _attempted = false;
                _record("surface=" + Key + ";action=rolled-back;restored=" +
                    _before.Count + ";preserved-later=" +
                    (restored.Count - _before.Count));
            }

            private void PublishIfChanged(IList<T> next, int insertion,
                bool boundaryFound)
            {
                if (SameReferences(_before, next))
                {
                    _record("surface=" + Key + ";action=unchanged;count=" +
                        _before.Count + ";boundary=" + boundaryFound +
                        ";index=" + insertion);
                    return;
                }
                _after = next;
                _attempted = true;
                _write(_after);
                if (!SameReferences(_read(), _after))
                    throw new InvalidOperationException(
                        "Brown-Fur publication write was not retained: " + Key);
                _record("surface=" + Key + ";action=published;before=" +
                    _before.Count + ";after=" + _after.Count + ";boundary=" +
                    boundaryFound + ";index=" + insertion);
            }

            private void ValidateUnique(IEnumerable<T> values, string source)
            {
                if (values == null)
                    throw new InvalidOperationException(
                        "Brown-Fur surface '" + Key + "' has no " + source +
                        " collection.");
                var identities = new HashSet<string>(StringComparer.Ordinal);
                var references = new HashSet<T>(ReferenceComparer<T>.Instance);
                foreach (T value in values)
                {
                    string id = Identity(value);
                    if (!identities.Add(id) || !references.Add(value))
                        throw new InvalidOperationException(
                            "Brown-Fur surface '" + Key +
                            "' contains a duplicate " + source +
                            " identity: " + id);
                }
            }

            private string Identity(T value)
            {
                if (value == null)
                    throw new InvalidOperationException(
                        "Brown-Fur surface '" + Key + "' contains null.");
                string id = _identity(value);
                if (string.IsNullOrWhiteSpace(id))
                    throw new InvalidOperationException(
                        "Brown-Fur surface '" + Key +
                        "' contains a blank identity.");
                return id;
            }
        }

        private sealed class ValueMutation<T> : IMutation
        {
            private readonly Func<T> _read;
            private readonly Action<T> _write;
            private readonly T _configured;
            private readonly IEqualityComparer<T> _comparer;
            private readonly Action<string> _record;
            private T _before;
            private bool _attempted;

            internal ValueMutation(string key, Func<T> read, Action<T> write,
                T configured, IEqualityComparer<T> comparer,
                Action<string> record)
            {
                if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException(
                    "A publication surface key is required.", "key");
                Key = key;
                _read = read ?? throw new ArgumentNullException("read");
                _write = write ?? throw new ArgumentNullException("write");
                _configured = configured;
                _comparer = comparer;
                _record = record;
            }

            public string Key { get; private set; }

            public void Apply()
            {
                _before = _read();
                if (_comparer.Equals(_before, _configured))
                {
                    _record("surface=" + Key + ";action=unchanged");
                    return;
                }
                _attempted = true;
                _write(_configured);
                if (!_comparer.Equals(_read(), _configured)) throw new
                    InvalidOperationException("Brown-Fur configuration write was not retained: " + Key);
                _record("surface=" + Key + ";action=configured");
            }

            public void Rollback()
            {
                if (!_attempted) return;
                T current = _read();
                if (_comparer.Equals(current, _before))
                {
                    _attempted = false;
                    return;
                }
                if (!_comparer.Equals(current, _configured)) throw new
                    InvalidOperationException("Rollback refused after an unrelated mutation: " + Key);
                _write(_before);
                if (!_comparer.Equals(_read(), _before)) throw new
                    InvalidOperationException("Brown-Fur configuration rollback was not retained: " + Key);
                _attempted = false;
                _record("surface=" + Key + ";action=rolled-back");
            }
        }

        private sealed class ActionMutation : IMutation
        {
            private readonly Action _apply;
            private readonly Action _rollback;
            private readonly Action<string> _record;
            private bool _attempted;

            internal ActionMutation(string key, Action apply, Action rollback,
                Action<string> record)
            {
                if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException(
                    "A publication surface key is required.", "key");
                Key = key;
                _apply = apply ?? throw new ArgumentNullException("apply");
                _rollback = rollback ?? throw new ArgumentNullException("rollback");
                _record = record;
            }

            public string Key { get; private set; }

            public void Apply()
            {
                _attempted = true;
                _apply();
                _record("surface=" + Key + ";action=applied");
            }

            public void Rollback()
            {
                if (!_attempted) return;
                _rollback();
                _attempted = false;
                _record("surface=" + Key + ";action=rolled-back");
            }
        }

        private static bool SameReferences<T>(IList<T> left, IList<T> right)
            where T : class
        {
            if (left == null || right == null || left.Count != right.Count)
                return false;
            for (int index = 0; index < left.Count; index++)
                if (!ReferenceEquals(left[index], right[index])) return false;
            return true;
        }

        private static bool StartsWithReferences<T>(IList<T> values,
            IList<T> prefix) where T : class
        {
            if (values == null || prefix == null || values.Count < prefix.Count)
                return false;
            for (int index = 0; index < prefix.Count; index++)
                if (!ReferenceEquals(values[index], prefix[index])) return false;
            return true;
        }

        private sealed class ReferenceComparer<T> : IEqualityComparer<T>
            where T : class
        {
            internal static readonly ReferenceComparer<T> Instance =
                new ReferenceComparer<T>();
            public bool Equals(T x, T y) { return ReferenceEquals(x, y); }
            public int GetHashCode(T value)
            {
                return System.Runtime.CompilerServices.RuntimeHelpers
                    .GetHashCode(value);
            }
        }
    }
}
