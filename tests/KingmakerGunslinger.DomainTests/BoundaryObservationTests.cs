using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using KingmakerGunslinger.RuntimeTesting;

namespace KingmakerGunslinger.DomainTests
{
    internal static class BoundaryObservationTests
    {
        internal static void AbsentProviderRequiresCompleteIsolation()
        {
            Assertions.True(BoundaryObservationPolicy.BrownFurAbsent(true, true, true, 0, false, 0, 0, 0), "Absent provider has no contract or publication.");
            Assertions.False(BoundaryObservationPolicy.BrownFurAbsent(false, true, true, 0, false, 0, 0, 0), "Present provider cannot use absent branch.");
            Assertions.False(BoundaryObservationPolicy.BrownFurAbsent(true, false, true, 0, false, 0, 0, 0), "Blocked is not unavailable.");
            Assertions.False(BoundaryObservationPolicy.BrownFurAbsent(true, true, false, 0, false, 0, 0, 0), "Contract must be absent.");
            Assertions.False(BoundaryObservationPolicy.BrownFurAbsent(true, true, true, 25, false, 0, 0, 0), "Unexpected registered identities fail.");
            Assertions.False(BoundaryObservationPolicy.BrownFurAbsent(true, true, true, 0, true, 0, 0, 0), "Accidental publication fails.");
            Assertions.False(BoundaryObservationPolicy.BrownFurAbsent(true, true, true, 0, false, 1, 0, 0), "Selector reference fails.");
            Assertions.False(BoundaryObservationPolicy.BrownFurAbsent(true, true, true, 0, false, 0, 1, 0), "Selector GUID fails.");
            Assertions.False(BoundaryObservationPolicy.BrownFurAbsent(true, true, true, 0, false, 0, 0, 1), "Publication event fails.");
        }
        internal static void StockComparisonPreservesMultiplicityAndCounts()
        {
            var exact = new VendorBoundaryRows("table", new[] { "item:200", "kit:1" }, new[] { "kit:1", "item:200" }, new[] { "item:200", "kit:1", "retired:1" });
            Assertions.True(exact.Passed && exact.HistoricalMissing.Length == 1 && exact.HistoricalMissing[0] == "retired:1", "Current contract and historical difference remain separate.");
            var invalid = new VendorBoundaryRows("table", new[] { "item:200", "kit:1" }, new[] { "item:199", "kit:1", "kit:1" }, new string[0]);
            Assertions.False(invalid.Passed, "Wrong count and duplicate stock fail.");
            Assertions.Equal(1, invalid.Missing.Length, "Missing intended count remains explicit.");
            Assertions.Equal(2, invalid.Unexpected.Length, "Duplicate and wrong-count rows retained.");
        }
        internal static void ClauseSerializationRetainsExpectedActualAndOwner()
        {
            Func<JsonSerializerSettings> previous = JsonConvert.DefaultSettings;
            try {
                JsonConvert.DefaultSettings = () => new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All, PreserveReferencesHandling = PreserveReferencesHandling.All };
                var clauses = new[] { new BoundaryClause("icon", "scroll-guid", "owned-art", "donor-art"), new BoundaryClause("charges", "scroll-guid", "1", "1") };
                var report = JObject.Parse(BoundaryObservationPolicy.Serialize(new { schemaVersion = 1, clauses }));
                Assertions.Equal("scroll-guid", (string)report["clauses"][0]["Owner"], "Owning blueprint survives serialization.");
                Assertions.Equal("owned-art", (string)report["clauses"][0]["Expected"], "Expected contract is retained.");
                Assertions.Equal("donor-art", (string)report["clauses"][0]["Actual"], "Adverse actual is retained.");
                Assertions.False((bool)report["clauses"][0]["Passed"], "Failed clause stays failed.");
                Assertions.True((bool)report["clauses"][1]["Passed"], "Passing neighbor survives.");
                Assertions.False(report.ToString().Contains("$type") || report.ToString().Contains("$id"), "Ambient game serializer cannot add type/reference metadata.");
            } finally { JsonConvert.DefaultSettings = previous; }
        }
    }
}
