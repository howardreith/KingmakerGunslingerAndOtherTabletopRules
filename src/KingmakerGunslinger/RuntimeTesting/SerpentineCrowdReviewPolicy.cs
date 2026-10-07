using System;
using System.Collections.Generic;
using System.Linq;
using KingmakerGunslinger.Summoning;

namespace KingmakerGunslinger.RuntimeTesting
{
    // Closed snake quantity fixture only. Never repairs unrelated scheduler
    // membership, and never turns a foreign-list change into a passing result.
    internal static class SerpentineCrowdReviewPolicy
    {
        private static bool ContainsReference<T>(IEnumerable<T> values, T item) where T : class
        { return values.Any(value => ReferenceEquals(value, item)); }

        private static bool UniqueReferences<T>(IEnumerable<T> values) where T : class
        {
            var seen = new List<T>();
            foreach (T value in values)
            {
                if (value == null || ContainsReference(seen, value)) return false;
                seen.Add(value);
            }
            return true;
        }

        private static bool SameReferences<T>(IEnumerable<T> first, IEnumerable<T> second) where T : class
        {
            T[] left = first.ToArray(), right = second.ToArray();
            return left.Length == right.Length && left.Select((value, index) =>
                ReferenceEquals(value, right[index])).All(value => value);
        }

        internal static bool RestoreOwnedAwake<T>(string key, IList<T> live, T[] before,
            T[] owned, Func<T, bool> validOwned, out string disposition) where T : class
        {
            disposition = "invalid-closed-scope";
            if (!SerpentineVisualPolicy.IsSnake(key) || live == null || live.IsReadOnly ||
                before == null || owned == null || owned.Length < 2 || owned.Length > 5 ||
                validOwned == null || !UniqueReferences(before) || !UniqueReferences(live) ||
                !UniqueReferences(owned) || owned.Any(value => !validOwned(value))) return false;
            Func<T, bool> isOwned = value => ContainsReference(owned, value);
            if (!SameReferences(before.Where(value => !isOwned(value)),
                    live.Where(value => !isOwned(value))))
            {
                disposition = "unrelated-awake-sequence-changed-no-mutation";
                return false;
            }
            bool alreadyExact = SameReferences(before, live);
            // Remove and insert ONLY exact owned actors. Unrelated actors are
            // never removed, added or reordered. Native sleep/reordering of
            // preexisting owned entries must be restored as well as additions.
            for (int index = live.Count - 1; index >= 0; index--)
                if (isOwned(live[index])) live.RemoveAt(index);
            for (int index = 0; index < before.Length; index++)
                if (isOwned(before[index])) live.Insert(index, before[index]);
            bool exact = SameReferences(before, live);
            disposition = exact ? (alreadyExact ? "already-exact" : "owned-only-restored") : "restoration-mismatch";
            return exact;
        }
    }
}
