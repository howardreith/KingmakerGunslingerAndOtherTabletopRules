using System;
using System.Linq;
using KingmakerGunslinger.Compatibility;

namespace KingmakerGunslinger.DomainTests
{
    /// <summary>
    /// GS-AUD-0034. The selection-boundary rule used by both
    /// <c>BlueprintParametrizedFeature.ExtractSelectionItems</c> postfixes.
    ///
    /// A synthetic distinct implementation proves that an unfamiliar entry
    /// survives the merge. It deliberately does NOT claim any installed mod was
    /// previously broken: that exposure check was done separately and came back
    /// negative - across the eleven mods on the supported host no type
    /// implements the selection interface, and the only other mod patching the
    /// same method supplies only entries this project understands.
    /// </summary>
    internal static class SelectionBoundaryPreservationTests
    {
        private abstract class Row
        {
            protected Row(string name) { Name = name; }
            internal string Name { get; private set; }
        }

        /// <summary>An entry this project understands and may reorder.</summary>
        private sealed class Understood : Row
        {
            internal Understood(string name) : base(name) { }
        }

        /// <summary>
        /// Another publisher's entry: it belongs in the sequence but this
        /// project knows nothing about it. This is the case the old narrowing
        /// silently dropped.
        /// </summary>
        private sealed class Foreign : Row
        {
            internal Foreign(string name) : base(name) { }
        }

        private static bool IsUnderstood(Row value) { return value is Understood; }

        private static string Describe(Row[] rows)
        {
            return string.Join(",", rows.Select(value =>
                (value is Understood ? "U:" : "F:") + value.Name).ToArray());
        }

        private static Row[] Alphabetize(Row[] understood, params Row[] additions)
        {
            return understood.Concat(additions)
                .OrderBy(value => value.Name, StringComparer.OrdinalIgnoreCase)
                .ToArray();
        }

        internal static void ForeignEntriesSurviveAnAddition()
        {
            var ours = new Understood("bravo");
            var foreign = new Foreign("alpha");
            var addition = new Understood("alfa");
            Row[] host = { foreign, ours };

            Row[] result = SelectionBoundaryPreservation.Merge(host, IsUnderstood,
                understood => Alphabetize(understood, addition));

            Assertions.Equal("U:alfa,U:bravo,F:alpha", Describe(result),
                "A foreign entry must survive, and our entries keep their own alphabetical contract.");
            Assertions.Equal(3, result.Length,
                "The merge must neither drop nor duplicate an entry.");
            Assertions.True(result.Any(value => ReferenceEquals(value, foreign)),
                "The foreign entry must be preserved by reference, not recreated.");
        }

        internal static void ContributingNothingLeavesTheHostSequenceUntouched()
        {
            var foreign = new Foreign("alpha");
            var ours = new Understood("bravo");
            var secondForeign = new Foreign("charlie");
            Row[] host = { foreign, ours, secondForeign };

            // The common case: this project adds nothing for this selector.
            // Previously this still returned a narrowed copy, dropping both
            // foreign entries.
            Row[] result = SelectionBoundaryPreservation.Merge(host, IsUnderstood,
                understood => understood);

            Assertions.Equal("F:alpha,U:bravo,F:charlie", Describe(result),
                "With no contribution of ours the host sequence must be returned exactly, order included.");
            Assertions.True(ReferenceEquals(result[0], foreign) &&
                ReferenceEquals(result[1], ours) &&
                ReferenceEquals(result[2], secondForeign),
                "Every original entry must be preserved by reference and position.");
        }

        internal static void ForeignEntriesKeepTheirRelativeOrder()
        {
            var first = new Foreign("one");
            var second = new Foreign("two");
            var third = new Foreign("three");
            var ours = new Understood("zulu");
            var addition = new Understood("alfa");
            Row[] host = { first, ours, second, third };

            Row[] result = SelectionBoundaryPreservation.Merge(host, IsUnderstood,
                understood => Alphabetize(understood, addition));

            Assertions.Equal("U:alfa,U:zulu,F:one,F:two,F:three", Describe(result),
                "Foreign entries must keep their original relative order among themselves.");
        }

        internal static void AnAllForeignSequenceIsNeverEmptied()
        {
            var first = new Foreign("one");
            var second = new Foreign("two");
            Row[] host = { first, second };

            Row[] untouched = SelectionBoundaryPreservation.Merge(host, IsUnderstood,
                understood => understood);
            Assertions.Equal("F:one,F:two", Describe(untouched),
                "A sequence of only foreign entries must not be emptied.");

            var addition = new Understood("alfa");
            Row[] contributed = SelectionBoundaryPreservation.Merge(host, IsUnderstood,
                understood => new Row[] { addition });
            Assertions.Equal("U:alfa,F:one,F:two", Describe(contributed),
                "Adding to an all-foreign sequence must preserve every foreign entry.");
        }

        internal static void UnderstoodEntriesMayStillBeFilteredByTheComposer()
        {
            // The composer owns the understood entries, so it may drop one of
            // ours. That must not affect foreign entries.
            var ourKept = new Understood("keep");
            var ourDropped = new Understood("drop");
            var foreign = new Foreign("other");
            var addition = new Understood("added");
            Row[] host = { ourKept, foreign, ourDropped };

            Row[] result = SelectionBoundaryPreservation.Merge(host, IsUnderstood,
                understood => new Row[] { ourKept, addition });

            Assertions.Equal("U:keep,U:added,F:other", Describe(result),
                "The composer may filter our own entries while every foreign entry survives.");
        }

        internal static void NullsAndEmptyInputsAreHandled()
        {
            Assertions.Equal(0, SelectionBoundaryPreservation.Merge<Row>(
                null, IsUnderstood, understood => understood).Length,
                "A null host sequence must merge to an empty sequence.");

            var foreign = new Foreign("alpha");
            Row[] withNull = { null, foreign, null };
            Assertions.Equal("F:alpha", Describe(SelectionBoundaryPreservation.Merge(
                withNull, IsUnderstood, understood => understood)),
                "Null entries are dropped; real entries are kept.");

            Assertions.Equal(0, SelectionBoundaryPreservation.Merge(
                new Row[0], IsUnderstood, understood => null).Length,
                "A composer returning null must not throw.");

            Assertions.Throws<ArgumentNullException>(() =>
                SelectionBoundaryPreservation.Merge(new Row[0], null,
                    understood => understood),
                "A missing predicate must fail closed.");
            Assertions.Throws<ArgumentNullException>(() =>
                SelectionBoundaryPreservation.Merge<Row>(new Row[0], IsUnderstood, null),
                "A missing composer must fail closed.");
        }
    }
}
