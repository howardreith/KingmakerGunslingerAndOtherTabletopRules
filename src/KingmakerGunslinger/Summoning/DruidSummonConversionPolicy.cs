using System;
using System.Collections.Generic;
using System.Linq;

namespace KingmakerGunslinger.Summoning
{
    /// <summary>One native spontaneous summon conversion feature and the
    /// published spell family its quantity arrays stand for.</summary>
    internal sealed class SummonConversionRoute
    {
        internal SummonConversionRoute(string classGuid, string featureGuid, SummonFamily family, string tierOneParentGuid)
        { ClassGuid = classGuid; FeatureGuid = featureGuid; Family = family; TierOneParentGuid = tierOneParentGuid; }
        internal string ClassGuid { get; private set; }
        internal string FeatureGuid { get; private set; }
        internal SummonFamily Family { get; private set; }
        internal string TierOneParentGuid { get; private set; }
    }

    internal static class DruidSummonConversionPolicy
    {
        internal const string DruidClassGuid = "610d836f3a3a9ed42a4349b62f002e96";
        internal const string FeatureGuid = "b296531ffe013c8499ad712f8ae97f6b";
        internal const string TierOneParentGuid = "c6147854641924442a3bb736080cfeb6";
        internal const string HeraldCallerFeatureGuid = "f6c112a579a81c0439c597f9ee222243";
        internal const string HeraldTierOneParentGuid = "8fd74eddd9b6c224693d9ab241f25e84";

        internal static readonly SummonConversionRoute Druid =
            new SummonConversionRoute(DruidClassGuid, FeatureGuid, SummonFamily.NaturesAlly, TierOneParentGuid);
        internal static readonly SummonConversionRoute Herald =
            new SummonConversionRoute(SummonClericAlignmentPolicy.ClericClassGuid, HeraldCallerFeatureGuid, SummonFamily.Monster, HeraldTierOneParentGuid);

        internal static SummonConversionRoute RouteForClass(string classGuid)
        { return classGuid == DruidClassGuid ? Druid : classGuid == Herald.ClassGuid ? Herald : null; }

        internal static SummonConversionRoute RouteForFeature(string featureGuid)
        { return featureGuid == FeatureGuid ? Druid : featureGuid == HeraldCallerFeatureGuid ? Herald : null; }

        internal static string Expected(int level, SummonMultiplicity quantity) { return Expected(Druid, level, quantity); }
        internal static bool MatchesArray(IEnumerable<string> guids, SummonMultiplicity quantity) { return MatchesArray(Druid, guids, quantity); }
        internal static bool MatchesArrays(IEnumerable<IEnumerable<string>> arrays) { return MatchesArrays(Druid, arrays); }
        internal static bool TryResolve(string guid, out int tier, out SummonMultiplicity quantity) { return TryResolve(Druid, guid, out tier, out quantity); }

        // Each installed conversion feature has three exact native quantity
        // arrays. Only tier I uses its menu parent; II-IX use the released
        // native selectable identities already recorded in the option catalog.
        internal static string Expected(SummonConversionRoute route, int level, SummonMultiplicity quantity)
        {
            if (level < 1 || level > 9 || !Enum.IsDefined(typeof(SummonMultiplicity), quantity)) return null;
            if (level == 1 && quantity == SummonMultiplicity.One) return route.TierOneParentGuid;
            if (level == 1) return null;
            return SummonNativeOptionCatalog.All.SingleOrDefault(value => value.Family == route.Family &&
                value.Tier == level && value.Multiplicity == quantity)?.Guid;
        }

        internal static bool MatchesArray(SummonConversionRoute route, IEnumerable<string> guids, SummonMultiplicity quantity)
        {
            if (guids == null || !Enum.IsDefined(typeof(SummonMultiplicity), quantity)) return false;
            var values = guids.ToArray();
            return values.Length == 10 && values[0] == null && Enumerable.Range(1, 9).All(level => values[level] == Expected(route, level, quantity));
        }

        internal static bool MatchesArrays(SummonConversionRoute route, IEnumerable<IEnumerable<string>> arrays)
        {
            if (arrays == null) return false;
            var values = arrays.Select(value => value == null ? null : value.ToArray()).ToArray();
            return values.Length == 3 && Enum.GetValues(typeof(SummonMultiplicity)).Cast<SummonMultiplicity>()
                .All(quantity => values.Count(value => MatchesArray(route, value, quantity)) == 1);
        }

        internal static bool TryResolve(SummonConversionRoute route, string guid, out int tier, out SummonMultiplicity quantity)
        {
            tier = 0; quantity = SummonMultiplicity.One;
            if (guid == route.TierOneParentGuid) { tier = 1; return true; }
            var native = SummonNativeOptionCatalog.All.SingleOrDefault(value => value.Family == route.Family &&
                value.Tier >= 2 && value.Guid == guid);
            if (native == null) return false;
            tier = native.Tier; quantity = native.Multiplicity; return true;
        }

        // Commit a fully verified projection without modifying the original
        // native list, its foreign rows, or the native conversion objects.
        // A removal names an exact native row whose every child the same menu
        // already lists, so it is dropped instead of replaced.
        internal static T[] Project<T>(IEnumerable<T> original, IDictionary<T, T[]> replacements,
            IEnumerable<T> removals = null) where T : class
        {
            if (original == null || replacements == null) throw new ArgumentNullException();
            var rows = original.ToArray();
            var removed = removals == null ? new T[0] : removals.ToArray();
            if (replacements.Any(pair => pair.Key == null || rows.Count(row => ReferenceEquals(row, pair.Key)) != 1 ||
                pair.Value == null || pair.Value.Length == 0 || pair.Value.Any(value => value == null)))
                throw new InvalidOperationException("Incomplete exact native Druid quantity projection.");
            if (removed.Any(value => value == null || rows.Count(row => ReferenceEquals(row, value)) != 1 ||
                    removed.Count(other => ReferenceEquals(other, value)) != 1 ||
                    replacements.Keys.Any(key => ReferenceEquals(key, value))))
                throw new InvalidOperationException("Inexact native Druid quantity removal.");
            var result = new List<T>();
            foreach (var row in rows)
            {
                if (row != null && removed.Any(value => ReferenceEquals(value, row))) continue;
                var replacement = replacements.FirstOrDefault(value => ReferenceEquals(value.Key, row));
                if (row != null && replacement.Key != null) result.AddRange(replacement.Value);
                else result.Add(row);
            }
            return result.ToArray();
        }

        // Each quantity row projects its own children, so the flattened list
        // would stay grouped by quantity. The ranked rows take the positions
        // those rows already hold, in rank order; every other row, including
        // a foreign one, keeps its exact index.
        internal static T[] OrderProjectedChoices<T>(IEnumerable<T> projected, Func<T, int?> rank) where T : class
        {
            if (projected == null || rank == null) throw new ArgumentNullException();
            var rows = projected.ToArray();
            int[] positions = Enumerable.Range(0, rows.Length).Where(index => rows[index] != null && rank(rows[index]).HasValue).ToArray();
            T[] ordered = positions.OrderBy(index => rank(rows[index]).Value).ThenBy(index => index).Select(index => rows[index]).ToArray();
            var result = rows.ToArray();
            for (int index = 0; index < positions.Length; index++) result[positions[index]] = ordered[index];
            return result;
        }
    }
}
