using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace KingmakerGunslinger.RuntimeTesting
{
    // Request-local evidence only. No game registration, stock or item mutation.
    internal sealed class BoundaryClause
    {
        internal BoundaryClause(string name, string owner, string expected, string actual)
        { Name = name; Owner = owner; Expected = expected; Actual = actual; }
        public string Name { get; private set; }
        public string Owner { get; private set; }
        public string Expected { get; private set; }
        public string Actual { get; private set; }
        public bool Passed => Expected != null && Actual != null &&
            string.Equals(Expected, Actual, StringComparison.Ordinal);
    }

    internal sealed class VendorBoundaryRows
    {
        internal VendorBoundaryRows(string table, IEnumerable<string> expected,
            IEnumerable<string> actual, IEnumerable<string> historicalExpected, string group = "")
        {
            Table = table;
            Group = group;
            Expected = expected.OrderBy(value => value, StringComparer.Ordinal).ToArray();
            Actual = actual.OrderBy(value => value, StringComparer.Ordinal).ToArray();
            HistoricalExpected = historicalExpected.OrderBy(value => value, StringComparer.Ordinal).ToArray();
            Missing = Difference(Expected, Actual);
            Unexpected = Difference(Actual, Expected);
            HistoricalMissing = Difference(HistoricalExpected, Actual);
            HistoricalUnexpected = Difference(Actual, HistoricalExpected);
        }
        public string Table { get; private set; }
        public string Group { get; private set; }
        public string[] Expected { get; private set; }
        public string[] Actual { get; private set; }
        public string[] HistoricalExpected { get; private set; }
        public string[] Missing { get; private set; }
        public string[] Unexpected { get; private set; }
        public string[] HistoricalMissing { get; private set; }
        public string[] HistoricalUnexpected { get; private set; }
        public bool Passed => Missing.Length == 0 && Unexpected.Length == 0;
        private static string[] Difference(string[] left, string[] right)
        {
            var remaining = right.ToList();
            var result = new List<string>();
            foreach (string row in left)
            {
                int index = remaining.FindIndex(value => string.Equals(value, row, StringComparison.Ordinal));
                if (index >= 0) remaining.RemoveAt(index); else result.Add(row);
            }
            return result.ToArray();
        }
    }

    internal static class BoundaryObservationPolicy
    {
        internal static bool BrownFurAbsent(bool providerAbsent, bool unavailable,
            bool contractAbsent, int identities, bool published, int references,
            int guidReferences, int publicationEvents)
        {
            return providerAbsent && unavailable && contractAbsent && identities == 0 &&
                !published && references == 0 && guidReferences == 0 && publicationEvents == 0;
        }

        internal static string Serialize(object report)
        {
            return JsonConvert.SerializeObject(report, Formatting.Indented,
                new JsonSerializerSettings {
                    ContractResolver = new DefaultContractResolver(),
                    PreserveReferencesHandling = PreserveReferencesHandling.None,
                    TypeNameHandling = TypeNameHandling.None
                });
        }
    }
}
