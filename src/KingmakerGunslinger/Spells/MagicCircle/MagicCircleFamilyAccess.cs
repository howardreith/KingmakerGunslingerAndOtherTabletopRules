using System;
using System.Collections.Generic;
using System.Linq;

namespace KingmakerGunslinger.Spells.MagicCircle
{
    // Native list membership checks raw learnable entries. Our exact owned
    // children inherit eligibility only from an actually published family that
    // exposes that child. Names, GUID lookalikes and a child's Parent alone
    // cannot confer access (the restricted parents share the same children).
    internal static class MagicCircleFamilyAccess
    {
        internal static bool ContainsVariant<T>(T child, IEnumerable<T> ownedChildren,
            IEnumerable<T> ownedFamilies, IEnumerable<T> published,
            Func<T, IEnumerable<T>> variants) where T : class
        {
            if (child == null || ownedChildren == null || ownedFamilies == null || published == null || variants == null ||
                !ownedChildren.Any(value => ReferenceEquals(value, child))) return false;
            return published.Any(entry => entry != null && ownedFamilies.Any(family => ReferenceEquals(family, entry)) &&
                (variants(entry) ?? Enumerable.Empty<T>()).Any(value => ReferenceEquals(value, child)));
        }
    }
}
