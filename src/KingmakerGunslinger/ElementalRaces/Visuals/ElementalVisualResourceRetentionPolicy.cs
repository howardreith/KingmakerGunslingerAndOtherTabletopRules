using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace KingmakerGunslinger.ElementalRaces.Visuals
{
    internal static class ElementalVisualResourceRetentionPolicy
    {
        internal static int Append<T>(ISet<string> retainedIds, IList<T> retainedAssets,
            IEnumerable<KeyValuePair<string, T[]>> ownedResources) where T : class
        {
            if (retainedIds == null) throw new ArgumentNullException("retainedIds");
            if (retainedAssets == null) throw new ArgumentNullException("retainedAssets");
            if (ownedResources == null) throw new ArgumentNullException("ownedResources");
            var plan = ownedResources.ToArray();
            if (plan.Any(value => string.IsNullOrWhiteSpace(value.Key) || value.Value == null ||
                value.Value.Any(asset => ReferenceEquals(asset, null))) ||
                plan.Select(value => value.Key).Distinct(StringComparer.Ordinal).Count() != plan.Length)
                throw new InvalidOperationException("Owned visual retention plan is incomplete or ambiguous.");
            // Preflight the entire plan before extending either native collection.
            var seen = new HashSet<T>(retainedAssets, ReferenceComparer<T>.Instance);
            int additions = 0;
            foreach (var resource in plan)
            {
                if (retainedIds.Add(resource.Key)) additions++;
                foreach (T asset in resource.Value)
                    if (seen.Add(asset)) { retainedAssets.Add(asset); additions++; }
            }
            return additions;
        }

        private sealed class ReferenceComparer<T> : IEqualityComparer<T> where T : class
        {
            internal static readonly ReferenceComparer<T> Instance = new ReferenceComparer<T>();
            public bool Equals(T left, T right) { return ReferenceEquals(left, right); }
            public int GetHashCode(T value) { return ReferenceEquals(value, null) ? 0 : RuntimeHelpers.GetHashCode(value); }
        }
    }
}
