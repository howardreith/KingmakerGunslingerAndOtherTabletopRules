using System;
using System.Collections.Generic;
using System.Linq;

namespace KingmakerGunslinger.BodyguardFeats
{
    internal sealed class BodyguardPublicationSurface<T> where T : class
    {
        internal BodyguardPublicationSurface(string role, Func<T[]> read,
            Action<T[]> write)
        {
            if (string.IsNullOrWhiteSpace(role))
                throw new ArgumentException("A publication role is required.", "role");
            Role = role;
            Read = read ?? throw new ArgumentNullException("read");
            Write = write ?? throw new ArgumentNullException("write");
        }

        internal string Role { get; private set; }
        internal Func<T[]> Read { get; private set; }
        internal Action<T[]> Write { get; private set; }
    }

    internal sealed class BodyguardFeatPublicationTransaction<T> where T : class
    {
        private readonly PublicationRecord[] _records;

        private BodyguardFeatPublicationTransaction(PublicationRecord[] records)
        { _records = records; }

        internal static BodyguardFeatPublicationTransaction<T> Publish(
            BodyguardPublicationSurface<T>[] surfaces, T[] additions,
            Func<T, string> identity, Func<T, string> displayName,
            Action<int> beforeWrite = null)
        {
            if (surfaces == null || surfaces.Length == 0)
                throw new ArgumentException("At least one publication surface is required.",
                    "surfaces");
            if (additions == null || additions.Length == 0 ||
                additions.Any(value => value == null))
                throw new ArgumentException("Publication additions must be nonempty.",
                    "additions");
            if (identity == null) throw new ArgumentNullException("identity");
            if (displayName == null) throw new ArgumentNullException("displayName");
            RequireUniqueAdditions(additions, identity);

            var records = new PublicationRecord[surfaces.Length];
            for (int index = 0; index < surfaces.Length; index++)
            {
                BodyguardPublicationSurface<T> surface = surfaces[index] ??
                    throw new ArgumentException("A publication surface is null.",
                        "surfaces");
                T[] before = surface.Read();
                records[index] = new PublicationRecord(surface,
                    before, Merge(before, additions, identity, displayName));
            }

            int written = 0;
            try
            {
                for (int index = 0; index < records.Length; index++)
                {
                    if (beforeWrite != null) beforeWrite(index);
                    records[index].Surface.Write(records[index].Published);
                    written++;
                    Validate(records[index].Surface.Read(), additions, identity,
                        records[index].Surface.Role);
                }
                return new BodyguardFeatPublicationTransaction<T>(records);
            }
            catch
            {
                for (int index = written - 1; index >= 0; index--)
                    records[index].Surface.Write(records[index].Before);
                throw;
            }
        }

        internal void Rollback()
        {
            for (int index = 0; index < _records.Length; index++)
                if (!ReferenceEquals(_records[index].Surface.Read(),
                    _records[index].Published))
                    throw new InvalidOperationException("Publication surface '" +
                        _records[index].Surface.Role +
                        "' changed after Bodyguard publication; rollback refused.");
            for (int index = _records.Length - 1; index >= 0; index--)
                _records[index].Surface.Write(_records[index].Before);
        }

        private static T[] Merge(T[] current, T[] additions,
            Func<T, string> identity, Func<T, string> displayName)
        {
            current = current ?? Array.Empty<T>();
            if (current.Any(value => value == null))
                throw new InvalidOperationException(
                    "A feat publication surface contains a null entry.");
            var result = new List<T>(current);
            T[] orderedAdditions = additions.OrderBy(value =>
                displayName(value) ?? string.Empty, StringComparer.OrdinalIgnoreCase)
                .ThenBy(value => RequireIdentity(value, identity),
                    StringComparer.Ordinal).ToArray();
            foreach (T addition in orderedAdditions)
            {
                string additionIdentity = RequireIdentity(addition, identity);
                for (int index = result.Count - 1; index >= 0; index--)
                {
                    T existing = result[index];
                    if (!string.Equals(RequireIdentity(existing, identity),
                        additionIdentity, StringComparison.Ordinal)) continue;
                    if (!ReferenceEquals(existing, addition))
                        throw new InvalidOperationException("Feat GUID conflict for '" +
                            additionIdentity + "'.");
                    result.RemoveAt(index);
                }
                int insertion = FindInsertion(result, addition, identity, displayName);
                result.Insert(insertion, addition);
            }
            T[] merged = result.ToArray();
            Validate(merged, additions, identity, "planned");
            return merged;
        }

        private static int FindInsertion(List<T> current, T candidate,
            Func<T, string> identity, Func<T, string> displayName)
        {
            string candidateName = displayName(candidate) ?? string.Empty;
            string candidateIdentity = RequireIdentity(candidate, identity);
            for (int index = 0; index < current.Count; index++)
            {
                int comparison = StringComparer.OrdinalIgnoreCase.Compare(candidateName,
                    displayName(current[index]) ?? string.Empty);
                if (comparison < 0 || comparison == 0 &&
                    StringComparer.Ordinal.Compare(candidateIdentity,
                        RequireIdentity(current[index], identity)) < 0)
                    return index;
            }
            return current.Count;
        }

        private static void Validate(T[] values, T[] additions,
            Func<T, string> identity, string role)
        {
            if (values == null || values.Any(value => value == null))
                throw new InvalidOperationException("Bodyguard publication " + role +
                    " is null or contains null entries.");
            foreach (T addition in additions)
            {
                string expected = RequireIdentity(addition, identity);
                int references = values.Count(value => ReferenceEquals(value, addition));
                int identities = values.Count(value => string.Equals(
                    RequireIdentity(value, identity), expected, StringComparison.Ordinal));
                if (references != 1 || identities != 1)
                    throw new InvalidOperationException("Bodyguard publication " + role +
                        " is not singular by reference and GUID for '" + expected + "'.");
            }
        }

        private static void RequireUniqueAdditions(T[] additions,
            Func<T, string> identity)
        {
            if (additions.Distinct(ReferenceEqualityComparer<T>.Instance).Count() !=
                additions.Length || additions.Select(value => RequireIdentity(value,
                    identity)).Distinct(StringComparer.Ordinal).Count() != additions.Length)
                throw new ArgumentException(
                    "Publication additions must be unique by reference and GUID.",
                    "additions");
        }

        private static string RequireIdentity(T value, Func<T, string> identity)
        {
            string result = identity(value);
            if (string.IsNullOrWhiteSpace(result))
                throw new InvalidOperationException(
                    "A feat publication identity is missing.");
            return result;
        }

        private sealed class PublicationRecord
        {
            internal PublicationRecord(BodyguardPublicationSurface<T> surface,
                T[] before, T[] published)
            { Surface = surface; Before = before; Published = published; }

            internal BodyguardPublicationSurface<T> Surface { get; private set; }
            internal T[] Before { get; private set; }
            internal T[] Published { get; private set; }
        }

        private sealed class ReferenceEqualityComparer<TValue> :
            IEqualityComparer<TValue> where TValue : class
        {
            internal static readonly ReferenceEqualityComparer<TValue> Instance =
                new ReferenceEqualityComparer<TValue>();
            public bool Equals(TValue left, TValue right)
            { return ReferenceEquals(left, right); }
            public int GetHashCode(TValue value)
            { return System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(value); }
        }
    }
}
