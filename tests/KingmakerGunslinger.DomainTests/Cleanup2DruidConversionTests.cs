using System;
using System.Collections.Generic;
using System.Linq;
using KingmakerGunslinger.Summoning;

namespace KingmakerGunslinger.DomainTests
{
    internal static class Cleanup2DruidConversionTests
    {
        private static string[][] NativeArrays()
        {
            return Enum.GetValues(typeof(SummonMultiplicity)).Cast<SummonMultiplicity>().Select(quantity =>
                Enumerable.Range(0, 10).Select(level => level == 0 ? null : level == 1 && quantity == SummonMultiplicity.One ?
                    "c6147854641924442a3bb736080cfeb6" : SummonNativeOptionCatalog.All.SingleOrDefault(value =>
                        value.Family == SummonFamily.NaturesAlly && value.Tier == level && value.Multiplicity == quantity)?.Guid).ToArray()).ToArray();
        }

        internal static void ExactNativeQuantityArraysReplaceTheIncorrectParentAssumption()
        {
            var arrays = NativeArrays();
            Assertions.Equal("848bd9df8b2643143a7020be7cde8800", arrays[0][2], "Native rank2 Single identity is the released source leaf.");
            Assertions.Equal("b8ac9c653789b2a46ad85a075734c0e2", arrays[1][2], "Native rank2 d3 retains its distinct source identity.");
            Assertions.True(DruidSummonConversionPolicy.MatchesArrays(arrays), "Actual three native quantity arrays admit the current Druid feature.");
            var parentInsteadOfNative = arrays.Select(value => value.ToArray()).ToArray();
            parentInsteadOfNative[0][2] = "298148133cdc3fd42889b99c82711986";
            Assertions.False(DruidSummonConversionPolicy.MatchesArrays(parentInsteadOfNative), "A canonical menu parent cannot stand in for the native quantity source.");
            Assertions.False(DruidSummonConversionPolicy.MatchesArrays(new[] { arrays[0], arrays[0], arrays[2] }), "Duplicate quantity arrays cannot supply missing d3 authority.");
            var foreign = arrays.Select(value => value.ToArray()).ToArray(); foreign[2][8] = "0123456789abcdef0123456789abcdef";
            Assertions.False(DruidSummonConversionPolicy.MatchesArrays(foreign), "Unknown conversion access is preserved instead of reclassified.");
        }

        internal static void EveryNativeSourceResolvesItsOwnRankAndQuantity()
        {
            var arrays = NativeArrays();
            foreach (SummonMultiplicity expected in Enum.GetValues(typeof(SummonMultiplicity)))
            for (int level = 1; level <= 9; level++)
            {
                string guid = arrays[(int)expected][level]; if (guid == null) continue;
                int tier; SummonMultiplicity quantity;
                Assertions.True(DruidSummonConversionPolicy.TryResolve(guid, out tier, out quantity), "A real native quantity source must resolve.");
                Assertions.Equal(level, tier, "Conversion cannot borrow another rank.");
                Assertions.Equal(expected, quantity, "Conversion cannot pool Single, d3 and d4plus1 choices.");
            }
            int ignoredTier; SummonMultiplicity ignoredQuantity;
            Assertions.False(DruidSummonConversionPolicy.TryResolve("298148133cdc3fd42889b99c82711986", out ignoredTier, out ignoredQuantity),
                "Canonical SNA II is not the native feature's quantity source.");
            Assertions.False(DruidSummonConversionPolicy.TryResolve("6c7915c9dc494849918e958618f61db0", out ignoredTier, out ignoredQuantity),
                "A Monster source cannot enter a Druid Nature's Ally projection.");
        }

        private sealed class Row
        {
            internal string Id;
            public override bool Equals(object value) { return value is Row && ((Row)value).Id == Id; }
            public override int GetHashCode() { return Id.GetHashCode(); }
        }

        internal static void ProjectionRetainsOriginalSourceAndForeignObjectAuthority()
        {
            var single = new Row { Id = "native-single" }; var d3 = new Row { Id = "native-d3" };
            var foreign = new Row { Id = "native-single" }; var singleChild = new Row { Id = "published-single" }; var d3Child = new Row { Id = "published-d3" };
            var original = new[] { foreign, single, d3, (Row)null }; var before = original.ToArray();
            var replacement = new Dictionary<Row, Row[]> { { single, new[] { singleChild } }, { d3, new[] { d3Child } } };
            var projected = DruidSummonConversionPolicy.Project(original, replacement);
            Assertions.True(projected.SequenceEqual(new[] { foreign, singleChild, d3Child, null }) && ReferenceEquals(projected[0], foreign),
                "Only the exact native source objects project; equal-looking foreign rows retain their position and identity.");
            Assertions.True(original.SequenceEqual(before) && ReferenceEquals(original[1], single) && ReferenceEquals(original[2], d3),
                "Projection leaves the original native list and quantity objects intact for their invocation chains.");
            Assertions.Throws<InvalidOperationException>(() => DruidSummonConversionPolicy.Project(original,
                new Dictionary<Row, Row[]> { { single, new Row[0] } }), "An incomplete projection cannot silently erase a native source.");
            Assertions.True(original.SequenceEqual(before), "Refusal leaves every original row untouched.");
        }

        // A prepared Summon Nature's Ally lists its own variants after the
        // conversion rows. Its own quantity rows are dropped, never projected,
        // so no creature appears twice; other rows keep identity and order.
        internal static void PreparedParentDropsItsOwnQuantityRowsWithoutDuplicates()
        {
            var foreign = new Row { Id = "foreign" }; var self = new Row { Id = "native-single" }; var d3 = new Row { Id = "native-d3" };
            var d3Child = new Row { Id = "published-d3" };
            var original = new[] { foreign, self, d3 }; var before = original.ToArray();
            var projected = DruidSummonConversionPolicy.Project(original, new Dictionary<Row, Row[]> { { d3, new[] { d3Child } } }, new[] { self });
            Assertions.True(projected.SequenceEqual(new[] { foreign, d3Child }) && ReferenceEquals(projected[0], foreign),
                "The prepared parent's own quantity row is removed; foreign rows and other projections are kept in order.");
            var dropped = DruidSummonConversionPolicy.Project(original, new Dictionary<Row, Row[]>(), new[] { self, d3 });
            Assertions.True(dropped.SequenceEqual(new[] { foreign }), "Every self quantity row can be removed when the source lists all children.");
            Assertions.True(original.SequenceEqual(before), "Removal leaves the original native list intact.");
            var lookalike = new Row { Id = "native-single" };
            Assertions.Throws<InvalidOperationException>(() => DruidSummonConversionPolicy.Project(original, new Dictionary<Row, Row[]>(), new[] { lookalike }),
                "Only the exact native row object can be removed, not an equal-looking one.");
            Assertions.Throws<InvalidOperationException>(() => DruidSummonConversionPolicy.Project(original, new Dictionary<Row, Row[]>(), new[] { self, self }),
                "A row cannot be removed twice.");
            Assertions.Throws<InvalidOperationException>(() => DruidSummonConversionPolicy.Project(original,
                new Dictionary<Row, Row[]> { { self, new[] { d3Child } } }, new[] { self }), "A row cannot be both projected and removed.");
            Assertions.True(original.SequenceEqual(before), "Refusal leaves every original row untouched.");
        }
    }
}
