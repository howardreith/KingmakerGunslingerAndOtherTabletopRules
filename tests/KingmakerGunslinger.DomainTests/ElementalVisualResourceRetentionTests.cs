using System;
using System.Collections.Generic;
using System.Linq;
using KingmakerGunslinger.ElementalRaces.Visuals;

namespace KingmakerGunslinger.DomainTests
{
    internal static class ElementalVisualResourceRetentionTests
    {
        private sealed class EqualAssets
        {
            public override bool Equals(object other) { return other is EqualAssets; }
            public override int GetHashCode() { return 1; }
        }

        internal static void RetainsOwnedIdentitiesAndSharedAssetsWithoutChangingForeignEntries()
        {
            var foreign = new EqualAssets();
            var shared = new EqualAssets();
            var second = new EqualAssets();
            var ids = new HashSet<string>(StringComparer.Ordinal) { "native", "foreign" };
            var assets = new List<EqualAssets> { foreign };
            var plan = new[] {
                new KeyValuePair<string, EqualAssets[]>("owned-body", new[] { shared, second }),
                new KeyValuePair<string, EqualAssets[]>("owned-head", new[] { shared }) };
            Assertions.Equal(4, ElementalVisualResourceRetentionPolicy.Append(ids, assets, plan),
                "Both owned proxy identities and both distinct inner-asset references must be retained.");
            Assertions.True(ids.SetEquals(new[] { "native", "foreign", "owned-body", "owned-head" }) &&
                assets.Count == 3 && ReferenceEquals(assets[0], foreign) &&
                ReferenceEquals(assets[1], shared) && ReferenceEquals(assets[2], second),
                "Foreign identities, ordered references, and equality-colliding assets must be preserved.");
            Assertions.Equal(0, ElementalVisualResourceRetentionPolicy.Append(ids, assets, plan),
                "Repeated native doll rebuild callbacks must be idempotent.");
        }

        internal static void InvalidRetentionPlansCannotPartiallyMutateNativeCollections()
        {
            var retained = new object();
            var ids = new HashSet<string>(StringComparer.Ordinal) { "native" };
            var assets = new List<object> { retained };
            foreach (var invalid in new[] {
                new[] { new KeyValuePair<string, object[]>("owned", new[] { new object() }),
                    new KeyValuePair<string, object[]>("bad", new object[] { null }) },
                new[] { new KeyValuePair<string, object[]>("owned", new[] { new object() }),
                    new KeyValuePair<string, object[]>("owned", new[] { new object() }) } })
            {
                Assertions.Throws<InvalidOperationException>(() =>
                    ElementalVisualResourceRetentionPolicy.Append(ids, assets, invalid),
                    "An incomplete or duplicate ownership plan must fail before mutation.");
                Assertions.True(ids.SetEquals(new[] { "native" }) && assets.Count == 1 &&
                    ReferenceEquals(assets.Single(), retained), "Preflight failure must preserve exact original collections.");
            }
        }
    }
}
