using System;
using System.Collections.Generic;
using System.Linq;

namespace KingmakerGunslinger.Summoning
{
    internal static class DruidSummonConversionPolicy
    {
        internal const string DruidClassGuid = "610d836f3a3a9ed42a4349b62f002e96";
        internal const string FeatureGuid = "b296531ffe013c8499ad712f8ae97f6b";
        internal const string TierOneParentGuid = "c6147854641924442a3bb736080cfeb6";

        // Installed DruidSpontaneousSummon has three exact native quantity
        // arrays. Only tier I uses its menu parent; II-IX use the released
        // native selectable identities already recorded in the option catalog.
        internal static string Expected(int level, SummonMultiplicity quantity)
        {
            if (level < 1 || level > 9 || !Enum.IsDefined(typeof(SummonMultiplicity), quantity)) return null;
            if (level == 1 && quantity == SummonMultiplicity.One) return TierOneParentGuid;
            return SummonNativeOptionCatalog.All.SingleOrDefault(value => value.Family == SummonFamily.NaturesAlly &&
                value.Tier == level && value.Multiplicity == quantity)?.Guid;
        }

        internal static bool MatchesArray(IEnumerable<string> guids, SummonMultiplicity quantity)
        {
            if (guids == null || !Enum.IsDefined(typeof(SummonMultiplicity), quantity)) return false;
            var values = guids.ToArray();
            return values.Length == 10 && values[0] == null && Enumerable.Range(1, 9).All(level => values[level] == Expected(level, quantity));
        }

        internal static bool MatchesArrays(IEnumerable<IEnumerable<string>> arrays)
        {
            if (arrays == null) return false;
            var values = arrays.Select(value => value == null ? null : value.ToArray()).ToArray();
            return values.Length == 3 && Enum.GetValues(typeof(SummonMultiplicity)).Cast<SummonMultiplicity>()
                .All(quantity => values.Count(value => MatchesArray(value, quantity)) == 1);
        }

        internal static bool TryResolve(string guid, out int tier, out SummonMultiplicity quantity)
        {
            tier = 0; quantity = SummonMultiplicity.One;
            if (guid == TierOneParentGuid) { tier = 1; return true; }
            var native = SummonNativeOptionCatalog.All.SingleOrDefault(value => value.Family == SummonFamily.NaturesAlly &&
                value.Tier >= 2 && value.Guid == guid);
            if (native == null) return false;
            tier = native.Tier; quantity = native.Multiplicity; return true;
        }

        // Commit a fully verified projection without modifying the original
        // native list, its foreign rows, or the native conversion objects.
        internal static T[] Project<T>(IEnumerable<T> original, IDictionary<T, T[]> replacements) where T : class
        {
            if (original == null || replacements == null) throw new ArgumentNullException();
            var rows = original.ToArray();
            if (replacements.Any(pair => pair.Key == null || rows.Count(row => ReferenceEquals(row, pair.Key)) != 1 ||
                pair.Value == null || pair.Value.Length == 0 || pair.Value.Any(value => value == null)))
                throw new InvalidOperationException("Incomplete exact native Druid quantity projection.");
            var result = new List<T>();
            foreach (var row in rows)
            {
                var replacement = replacements.FirstOrDefault(value => ReferenceEquals(value.Key, row));
                if (row != null && replacement.Key != null) result.AddRange(replacement.Value);
                else result.Add(row);
            }
            return result.ToArray();
        }
    }
}
