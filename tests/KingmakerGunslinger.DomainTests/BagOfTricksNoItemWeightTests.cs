using System;
using System.Linq;
using KingmakerGunslinger.ElvenBranchedSpear;

namespace KingmakerGunslinger.DomainTests
{
    /// <summary>
    /// Production spear-profile policy behind the Bag of Tricks No Item Weight
    /// repair: authored weight stays a fatal check, the effective weight getter
    /// (a legitimate runtime setting) never is, and every mechanical predicate
    /// keeps its effective-getter check. Native construction, census, toggles,
    /// persistence and the summon cast are recorded in
    /// docs/BAG-OF-TRICKS-NO-ITEM-WEIGHT-COMPATIBILITY.md.
    /// </summary>
    internal static class BagOfTricksNoItemWeightTests
    {
        private static readonly string[] Predicates =
        {
            "category", "attack-type", "attack-range", "base-damage",
            "damage-type", "critical-edge", "critical-modifier", "fighter-group",
            "two-handed", "light", "monk", "natural", "unarmed",
            "attack-stat-override", "weight", "movement-enchantment",
            "subcategory-policy", "subcategory-list"
        };

        private static SpearProfileObservation[] With(
            params SpearProfileObservation[] replacements)
        {
            return Predicates.Select(predicate => replacements.FirstOrDefault(
                    value => string.Equals(value.Predicate, predicate, StringComparison.Ordinal)) ??
                new SpearProfileObservation(predicate, "x", "x", "x", "x", "x", "x", null,
                    "not-read")).ToArray();
        }

        internal static void ReporterZeroEffectiveWeightIsValid()
        {
            // The reporter's exact 0.0.193 root cause: stored 10, donor field 9,
            // both effective getters 0, getter patched by BagOfTricks.
            var weight = new SpearProfileObservation("weight", "10", "10", "9",
                "10", "0", "0", new[] { "BagOfTricks" }, "read");
            SpearProfileVerdict verdict = ElvenBranchedSpearProfilePolicy.Evaluate(With(weight));
            Assertions.True(verdict.IsValid,
                "A valid authored weight with a zero runtime override failed: " +
                verdict.DescribeFailure());
            Assertions.Equal(string.Empty, verdict.DescribeFailure(),
                "A zero runtime weight produced a diagnostic failure.");
            // A getter that throws (Bag of Tricks' ToToggleBool throws for an
            // invalid setting string) is not a defect of the authored weight.
            var throwing = new SpearProfileObservation("weight", "10", "10", "9",
                "10", "<throws ArgumentOutOfRangeException: StringToBool received an invalid string!>",
                "<unavailable ArgumentOutOfRangeException: x>", new[] { "BagOfTricks" }, "read");
            Assertions.True(ElvenBranchedSpearProfilePolicy.Evaluate(With(throwing)).IsValid,
                "A throwing runtime weight getter poisoned a valid authored weight.");
        }

        internal static void WrongAuthoredWeightStaysFatal()
        {
            // Zero copied into authored data while the setting was on, and a
            // donor's 9 lb inherited instead of the catalog's 10 lb: both are
            // authored defects, whatever the effective getters read (here both
            // zero, so a getter comparison alone could not see them).
            foreach (string stored in new[] { "0", "9" })
            {
                var weight = new SpearProfileObservation("weight", "10", stored, "9",
                    stored, "0", "0", null, "field");
                SpearProfileVerdict verdict = ElvenBranchedSpearProfilePolicy.Evaluate(With(weight));
                Assertions.False(verdict.IsValid,
                    "A wrong authored spear weight " + stored + " passed validation.");
                Assertions.True(verdict.Findings.Single().Kind ==
                    SpearProfileFindingKind.OwnedFieldMismatch,
                    "A wrong authored weight was not an owned-field finding.");
                Assertions.True(verdict.DescribeFailure().Contains(
                    "weight [owned-field]: expected=10 actual=" + stored + " donor=9"),
                    "The authored-weight diagnostic is incomplete: " + verdict.DescribeFailure());
            }
        }

        internal static void MechanicalGetterMismatchStaysFatal()
        {
            // The exemption is weight-only: the same zero-from-a-patch shape on
            // any mechanical predicate is still an effective-getter failure.
            foreach (string predicate in Predicates.Where(value =>
                !string.Equals(value, "weight", StringComparison.Ordinal)))
            {
                Assertions.False(ElvenBranchedSpearProfilePolicy.IsRuntimeModifiable(predicate),
                    predicate + " was treated as a runtime setting.");
                var observation = new SpearProfileObservation(predicate, "10", "10", "10",
                    "10", "0", "10", new[] { "other.mod" }, "read");
                SpearProfileVerdict verdict = ElvenBranchedSpearProfilePolicy.Evaluate(With(observation));
                Assertions.False(verdict.IsValid,
                    "An effective " + predicate + " mismatch was excused.");
                Assertions.True(verdict.Findings.Single().Kind ==
                    SpearProfileFindingKind.EffectiveGetterMismatch,
                    predicate + " getter mismatch was misclassified.");
            }
            Assertions.True(ElvenBranchedSpearProfilePolicy.IsRuntimeModifiable("weight"),
                "Weight must be judged on authored data only.");
            // A wrong weight and a wrong reach together: both are reported.
            SpearProfileVerdict both = ElvenBranchedSpearProfilePolicy.Evaluate(With(
                new SpearProfileObservation("weight", "10", "10", "9", "10", "0", "0",
                    new[] { "BagOfTricks" }, "read"),
                new SpearProfileObservation("attack-range", "10ft", "10ft", "10ft",
                    "6ft", "7ft", "6ft", new[] { "other.mod" }, "read")));
            Assertions.True(!both.IsValid && both.Findings.Length == 1 &&
                both.Findings[0].Observation.Predicate == "attack-range",
                "Only the reach defect must remain alongside a zero runtime weight: " +
                both.DescribeFailure());
        }
    }
}
