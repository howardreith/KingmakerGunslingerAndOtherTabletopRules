using System;
using System.Collections.Generic;
using System.Linq;

namespace KingmakerGunslinger.Summoning
{
    internal static class SummonDisplayOrderPolicy
    {
        internal static IReadOnlyList<T> Order<T>(IEnumerable<T> originals,
            IEnumerable<T> additions, Func<T, SummonMultiplicity?> originalKind,
            Func<T, SummonMultiplicity> additionKind)
        {
            if (originals == null || additions == null || originalKind == null ||
                additionKind == null) throw new ArgumentNullException();
            T[] old = originals.ToArray();
            T[] added = additions.ToArray();
            var result = new List<T>();
            result.AddRange(added.Where(value => additionKind(value) ==
                SummonMultiplicity.One));
            result.AddRange(old.Where(value => originalKind(value) ==
                SummonMultiplicity.One));
            result.AddRange(added.Where(value => additionKind(value) ==
                SummonMultiplicity.OneD3));
            result.AddRange(old.Where(value => originalKind(value) ==
                SummonMultiplicity.OneD3));
            result.AddRange(added.Where(value => additionKind(value) ==
                SummonMultiplicity.OneD4PlusOne));
            result.AddRange(old.Where(value => originalKind(value) ==
                SummonMultiplicity.OneD4PlusOne));
            result.AddRange(old.Where(value => !originalKind(value).HasValue));
            return result.AsReadOnly();
        }
    }
}
