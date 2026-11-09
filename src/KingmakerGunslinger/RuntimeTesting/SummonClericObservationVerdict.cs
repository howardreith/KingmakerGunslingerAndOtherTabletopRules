using System.Collections.Generic;
using System.Linq;

namespace KingmakerGunslinger.RuntimeTesting
{
    /// <summary>Pure verdicts the cleric alignment observer applies to native readings.</summary>
    internal static class SummonClericObservationVerdict
    {
        internal sealed class FixtureReading
        {
            internal int ClassLevel, ExpectedLevel, RegisteredBooks;
            internal string ClassBook, RegisteredBook, BookList, ExpectedList;
            internal bool BookIsClassBook, PreparedSlot, DeityMatchesPrerequisite, AlignmentIsExact;
        }

        /// <summary>Every unmet prerequisite; an empty list means dependent cells may run.</summary>
        internal static IReadOnlyList<string> FixtureProblems(FixtureReading value)
        {
            var problems = new List<string>();
            if (value.ClassLevel != value.ExpectedLevel) problems.Add("class-level:" + value.ClassLevel + "/" + value.ExpectedLevel);
            if (string.IsNullOrEmpty(value.ClassBook)) problems.Add("class-data-has-no-book");
            if (value.RegisteredBooks != 1) problems.Add("registered-books:" + value.RegisteredBooks);
            if (value.RegisteredBook != value.ClassBook) problems.Add("registered-book-differs-from-class-book");
            if (!value.BookIsClassBook) problems.Add("book-not-resolved-from-class-data");
            if (value.BookList != value.ExpectedList) problems.Add("wrong-spell-list");
            if (!value.PreparedSlot) problems.Add("no-valid-prepared-slot");
            if (!value.DeityMatchesPrerequisite) problems.Add("deity-prerequisite-does-not-admit-alignment");
            if (!value.AlignmentIsExact) problems.Add("alignment-not-as-set");
            return problems;
        }

        internal sealed class HeraldConversion
        {
            internal string Guid;
            internal bool SharedParent, Projects, ExecutionGoverned;
        }

        /// <summary>The conversions a Herald cleric could still use unguarded.
        /// Identifying a route is not enough; each must be enforced.</summary>
        internal static IReadOnlyList<string> UncoveredHeraldConversions(IEnumerable<HeraldConversion> conversions)
        {
            return conversions.Where(value => !(value.SharedParent || (value.Projects && value.ExecutionGoverned)))
                .Select(value => value.Guid).ToArray();
        }
    }
}
