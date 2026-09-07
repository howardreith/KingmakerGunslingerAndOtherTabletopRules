using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace KingmakerGunslinger.RuntimeTesting
{
    // Process-local observation IDs use reference equality even for objects that
    // implement semantic equality. These are evidence IDs, never saved GUIDs.
    internal sealed class CharacterCreationObservationIdentity
    {
        private readonly Dictionary<object, string> _ids =
            new Dictionary<object, string>(ReferenceComparer.Instance);
        internal string Get(object value)
        {
            if (ReferenceEquals(value, null)) return null;
            string id;
            if (!_ids.TryGetValue(value, out id))
            {
                id = "ref-" + (_ids.Count + 1).ToString(
                    System.Globalization.CultureInfo.InvariantCulture);
                _ids.Add(value, id);
            }
            return id;
        }
        internal static bool SameOrderedReferences<T>(T[] left, T[] right) where T : class
        {
            if (ReferenceEquals(left, right)) return true;
            if (left == null || right == null || left.Length != right.Length) return false;
            for (int i = 0; i < left.Length; i++)
                if (!ReferenceEquals(left[i], right[i])) return false;
            return true;
        }
        private sealed class ReferenceComparer : IEqualityComparer<object>
        {
            internal static readonly ReferenceComparer Instance = new ReferenceComparer();
            public new bool Equals(object left, object right) { return ReferenceEquals(left, right); }
            public int GetHashCode(object value) { return RuntimeHelpers.GetHashCode(value); }
        }
    }
}
