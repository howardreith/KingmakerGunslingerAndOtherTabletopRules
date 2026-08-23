using System;
using System.Collections.Generic;
using System.Linq;

namespace KingmakerGunslinger.EasternWeapons
{
    /// <summary>
    /// Pure classification and normalization rules for the late broad-martial
    /// publication.  The numeric value is part of the checked-in custom weapon
    /// category contract and intentionally does not depend on enum name lookup.
    /// </summary>
    internal static class EasternWeaponMartialPublicationPolicy
    {
        internal const int NodachiCategoryValue = 4934986;
        internal const int MinimumNativeMartialCategoryCount = 20;

        internal static bool IsBroadGrant(IEnumerable<int> authority,
            IEnumerable<int> candidate)
        {
            int[] required = NormalizeAuthority(authority);
            int[] available = (candidate ?? Enumerable.Empty<int>())
                .Distinct().ToArray();
            return required.Length >= MinimumNativeMartialCategoryCount &&
                required.All(available.Contains);
        }

        internal static int[] AppendNodachiExactlyOnce(
            IEnumerable<int> categories)
        {
            int[] source = (categories ?? Enumerable.Empty<int>()).ToArray();
            int count = source.Count(value => value == NodachiCategoryValue);
            if (count > 1)
                throw new InvalidOperationException(
                    "A broad martial grant already contains duplicate Nodachi categories.");
            return count == 1 ? source : source.Concat(new[]
                { NodachiCategoryValue }).ToArray();
        }

        internal static int[] NormalizeAuthority(IEnumerable<int> categories)
        {
            return (categories ?? Enumerable.Empty<int>()).Where(value =>
                value != NodachiCategoryValue).Distinct().ToArray();
        }
    }
}
