using System;
using System.Collections.Generic;
using System.Linq;
using KingmakerGunslinger.Summoning;

namespace KingmakerGunslinger.DomainTests
{
    /// <summary>
    /// DEF-05 cast observer repair: the controlled d4+1 quantity, the Master Summoner class-rank duration and the
    /// donor carriers across the cast window, each with the owner's named negative controls.
    /// </summary>
    internal static class InheritedCastWindowContractTests
    {
        private static bool Has(IEnumerable<string> failures, string text)
        {
            return failures.Any(value => value.Contains(text));
        }

        // ---- Correction 2: the d4+1 quantity proved by one controlled roll ----

        private static InheritedControlledQuantityResult Quantity(int rank, IList<InheritedDieRoll> rolls, string[] units,
            string[] ids = null, bool recalculated = true)
        {
            ids = ids ?? units.Select((value, index) => "u" + index).ToArray();
            return InheritedControlledQuantityContract.Judge(recalculated, 1, rank, "D4", 2, rolls, "mephit",
                units, ids, units.Select(value => true).ToList());
        }

        private static IList<InheritedDieRoll> Forced(int count = 1)
        {
            return Enumerable.Range(0, count).Select(index => new InheritedDieRoll("D4", 1, 2, index == 0)).ToList();
        }

        internal static void ControlledQuantityIsProvedByTheRoll()
        {
            string[] three = { "mephit", "mephit", "mephit" };
            string[] two = { "mephit", "mephit" };
            InheritedControlledQuantityResult pass = Quantity(1, Forced(), three);
            Assertions.Equal(InheritedQuantityVerdict.Pass, pass.Verdict, "Rank 1 with the d4 forced to 2 and three units passes.");
            Assertions.Equal(3, pass.ExpectedCount, "The expected count is the forced die plus the expected rank, not the output.");
            Assertions.Equal(0, pass.Failures.Count, "The passing controlled quantity has no failure.");

            InheritedControlledQuantityResult rankZero = Quantity(0, Forced(), two);
            Assertions.True(rankZero.Verdict != InheritedQuantityVerdict.Pass && Has(rankZero.Failures, "rank:0/1"),
                "Rank 0 with the d4 forced to 2 and two units must fail.");
            Assertions.True(Has(Quantity(0, Forced(), three).Failures, "rank:0/1"),
                "Rank 0 must fail even when the count happens to be three.");

            InheritedControlledQuantityResult omitted = Quantity(1, Forced(), two);
            Assertions.Equal(InheritedQuantityVerdict.ProductFailure, omitted.Verdict,
                "Rank 1, a forced 2 and only two units is the product defect (the bonus is not applied).");
            Assertions.True(Has(omitted.Failures, "quantity-omits-bonus:2/3"), "The product defect names the missing bonus.");

            InheritedControlledQuantityResult noRoll = Quantity(1, new List<InheritedDieRoll>(), three);
            Assertions.Equal(InheritedQuantityVerdict.ObserverFailure, noRoll.Verdict, "Three units without a captured roll must fail.");
            Assertions.True(Has(noRoll.Failures, "die-roll-missing"), "The missing roll is named.");

            InheritedControlledQuantityResult twoRolls = Quantity(1, Forced(2), three);
            Assertions.Equal(InheritedQuantityVerdict.ObserverFailure, twoRolls.Verdict, "Two relevant rolls must fail.");
            Assertions.True(Has(twoRolls.Failures, "die-rolls:2"), "The repeated roll is named.");

            Assertions.True(Has(Quantity(1, new[] { new InheritedDieRoll("D4", 1, 3, false) }, three).Failures, "die-not-forced:3/2"),
                "An unforced roll cannot prove the path.");
            Assertions.True(Has(Quantity(1, new[] { new InheritedDieRoll("D4", 2, 2, true) }, three).Failures, "die-formula"),
                "A 2d4 roll is not the one 1d4 roll.");

            InheritedControlledQuantityResult foreign = Quantity(1, Forced(), new[] { "mephit", "mephit", "wolf" });
            Assertions.True(foreign.Verdict == InheritedQuantityVerdict.Failure && Has(foreign.Failures, "foreign-unit:wolf"),
                "A foreign unit among three must fail.");
            InheritedControlledQuantityResult duplicate = Quantity(1, Forced(), three, new[] { "a", "a", "b" });
            Assertions.True(duplicate.Verdict == InheritedQuantityVerdict.Failure && Has(duplicate.Failures, "duplicate-spawn-event"),
                "A duplicate capture of one unit must fail.");
            Assertions.True(Has(Quantity(1, Forced(), new[] { "mephit", "mephit", "mephit", "mephit" }).Failures, "quantity:4 outside 3..3"),
                "An extra unit must fail.");
            Assertions.True(Has(Quantity(1, Forced(), three, null, false).Failures, "rank-not-recalculated"),
                "A rank read from an unrecalculated context must fail.");
        }

        // ---- Correction 3: the Master Summoner duration from the class rank ----

        private static InheritedRankConfigView ClassConfig()
        {
            return new InheritedRankConfigView { Type = "Default", BaseValueType = "ClassLevel", Progression = "AsIs",
                Classes = new[] { "summoner" } };
        }

        private static IList<string> Duration(int classLevel, bool recalculated, int contextRank, string unit, double seconds,
            InheritedRankConfigView config = null, string rate = "Minutes")
        {
            int rank; double expected;
            return InheritedClassRankDurationContract.Judge(rate, "Zero", "Rank", "Default", config ?? ClassConfig(), "summoner",
                classLevel, 7, recalculated, contextRank, new[] { "a" },
                new[] { Tuple.Create(unit, seconds, 0d) }, out rank, out expected);
        }

        internal static void ClassRankDurationIsExact()
        {
            int rank; double expected;
            Assertions.Equal(0, InheritedClassRankDurationContract.Judge("Minutes", "Zero", "Rank", "Default", ClassConfig(), "summoner",
                7, 7, true, 7, new[] { "a" }, new[] { Tuple.Create("a", 420d, 0d) }, out rank, out expected).Count,
                "Rank 7 from the recalculated context and 420 s observed passes.");
            Assertions.True(rank == 7 && expected == 420d, "The independent expectation is 7 x 60 = 420 s.");
            Assertions.True(Has(Duration(7, false, 0, "a", 420), "rank-not-recalculated"), "An unrecalculated rank 0 must fail.");
            IList<string> zero = Duration(7, true, 0, "a", 420);
            Assertions.True(Has(zero, "context-rank:0/7"), "Class level 7 but a context rank of 0 must fail.");
            Assertions.True(Has(Duration(7, true, 7, "a", 0), "duration:a:0+0/420+0"), "Rank 7 with 0 s observed must fail.");
            IList<string> wrongUnit = Duration(7, true, 7, "z", 420);
            Assertions.True(Has(wrongUnit, "duration-for-unit:a:0") && Has(wrongUnit, "duration-for-other-unit:z"),
                "A duration attached to the wrong unit must fail.");
            Assertions.True(Has(Duration(6, true, 6, "a", 360), "class-level:6/7"), "A caster that is not Summoner 7 must fail.");
            Assertions.True(Has(Duration(7, true, 7, "a", 420, null, "Rounds"), "structure:rate:Rounds"),
                "A host row that is not Minutes must fail.");
            var casterLevel = ClassConfig(); casterLevel.BaseValueType = "CasterLevel";
            Assertions.True(Has(Duration(7, true, 7, "a", 420, casterLevel), "structure:base:CasterLevel"),
                "A Default rank not based on class level must fail.");
            var otherClass = ClassConfig(); otherClass.Classes = new[] { "wizard" };
            Assertions.True(Has(Duration(7, true, 7, "a", 420, otherClass), "structure:classes"),
                "A Default rank on another class must fail.");
            var halved = ClassConfig(); halved.Progression = "Div2";
            Assertions.True(Has(Duration(7, true, 3, "a", 180, halved), "structure:progression:Div2"),
                "A progression other than AsIs must fail.");
        }

        // ---- Correction 4: donor carriers across the cast window ----

        private sealed class Donor
        {
            internal object Array = new object();
            internal object[] Components = { new object(), new object() };
            internal object[] Spawns = { new object() };
            internal object[] AfterArrays = { new object() };
            internal object[][] AfterElements = { new[] { new object() } };
            internal object[] Actions = { new object(), new object() };
            internal string[] Types = { "Spawn", "ApplyBuff" };
            internal string[] Buffs = { "buffA" };
            internal string[] Native = { "n1" };

            internal InheritedTemplateDonorReceipt Receipt(string guid)
            {
                return new InheritedTemplateDonorReceipt(guid, "name-" + guid, Array, Components.ToArray(), Spawns.ToArray(),
                    AfterArrays.ToArray(), AfterElements.Select(value => value.ToArray()).ToArray(), Actions.ToArray(),
                    Types.ToArray(), Buffs.ToArray(), Native.ToArray());
            }
        }

        private static List<InheritedDonorWindowEntry> Window(IDictionary<string, Donor> donors, params string[] approved)
        {
            return donors.Select(entry => new InheritedDonorWindowEntry(entry.Key,
                approved.Contains(entry.Key) ? "approved-tier-one-publication" : "unchanged", approved.Contains(entry.Key),
                entry.Value.Receipt(entry.Key), new List<string>())).ToList();
        }

        private static Dictionary<string, Donor> Donors()
        {
            return new Dictionary<string, Donor>(StringComparer.Ordinal) {
                { "sm1", new Donor() }, { "sna1", new Donor() }, { "o1", new Donor() }, { "o2", new Donor() } };
        }

        internal static void DonorsAreUnchangedAcrossTheCasts()
        {
            Dictionary<string, Donor> donors = Donors();
            List<InheritedDonorWindowEntry> before = Window(donors, "sm1", "sna1");
            List<InheritedDonorWindowEntry> after = Window(donors, "sm1", "sna1");
            Assertions.Equal(0, InheritedDonorWindowContract.Validation("before", before, 4, 2).Count,
                "Two approved withheld transitions and two unchanged carriers pass validation.");
            Assertions.Equal(0, InheritedDonorWindowContract.Compare(before, after).Count,
                "Approved transitions unchanged across the cast window pass.");

            donors["sm1"].Components[1] = new object();
            donors["sm1"].Array = new object();
            IList<string> replaced = InheritedDonorWindowContract.Compare(before, Window(donors, "sm1", "sna1"));
            Assertions.True(Has(replaced, "sm1:components") && Has(replaced, "sm1:components-array"),
                "A new component replacement during the casts must fail.");

            donors = Donors(); before = Window(donors, "sm1", "sna1");
            donors["o1"].Buffs[0] = "buffB";
            IList<string> buff = InheritedDonorWindowContract.Compare(before, Window(donors, "sm1", "sna1"));
            Assertions.True(Has(buff, "o1:apply-buff-ids") && !Has(buff, "o1:actions"),
                "An in-place buff identity mutation (same action objects) must fail.");

            donors = Donors(); before = Window(donors, "sm1", "sna1");
            Array.Reverse(donors["o2"].Actions); Array.Reverse(donors["o2"].Types);
            IList<string> reorder = InheritedDonorWindowContract.Compare(before, Window(donors, "sm1", "sna1"));
            Assertions.True(Has(reorder, "o2:actions") && Has(reorder, "o2:action-types"), "An action reorder must fail.");

            donors = Donors(); before = Window(donors, "sm1", "sna1");
            List<InheritedDonorWindowEntry> mutated = Window(donors, "sna1");
            mutated[0] = new InheritedDonorWindowEntry("sm1", "mutated", false, donors["sm1"].Receipt("sm1"),
                new List<string> { "component-changed" });
            Assertions.True(Has(InheritedDonorWindowContract.Compare(before, mutated), "sm1:disposition:approved-tier-one-publication->mutated"),
                "A donor changing from approved to mutated must fail the window.");
            IList<string> afterValidation = InheritedDonorWindowContract.Validation("after", mutated, 4, 2);
            Assertions.True(Has(afterValidation, "after:approved:1/2") && Has(afterValidation, "after:mutated:1"),
                "The mutated side fails the census donor validation.");

            // Both sides differ from the pre-clone receipt in the same named categories (components-array, components),
            // but the underlying arrays differ: the window comparison of the actual receipts must still fail.
            donors = Donors(); before = Window(donors, "sm1", "sna1");
            donors["sna1"].Array = new object();
            donors["sna1"].Components = new[] { donors["sna1"].Components[0], new object() };
            IList<string> sameNames = InheritedDonorWindowContract.Compare(before, Window(donors, "sm1", "sna1"));
            Assertions.True(Has(sameNames, "sna1:components-array") && Has(sameNames, "sna1:components"),
                "Equal category names with a different underlying mutation must still fail.");

            Assertions.True(Has(InheritedDonorWindowContract.Validation("before", Window(Donors(), "sm1"), 4, 2), "before:approved:1/2"),
                "Only one approved transition fails validation.");
            List<InheritedDonorWindowEntry> notWithheld = Window(Donors(), "sm1", "sna1");
            notWithheld[0] = new InheritedDonorWindowEntry("sm1", "approved-tier-one-publication", false,
                new Donor().Receipt("sm1"), new List<string>());
            Assertions.True(Has(InheritedDonorWindowContract.Validation("before", notWithheld, 4, 2), "before:withheld:1/2"),
                "An approved transition that is not WithholdCompleteReplacement fails validation.");
            Assertions.True(Has(InheritedDonorWindowContract.Compare(before, Window(Donors(), "sm1", "sna1").Take(3).ToList()),
                "carrier-missing-after"), "A carrier missing after the casts must fail.");
        }
    }
}
