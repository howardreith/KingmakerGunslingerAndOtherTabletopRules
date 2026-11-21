using System;
using System.Linq;
using KingmakerGunslinger.ElvenBranchedSpear;
using KingmakerGunslinger.Gunsmithing;

namespace KingmakerGunslinger.DomainTests
{
    /// <summary>
    /// Production policies behind the spear startup and battered-pricing
    /// repair. Native construction, failed-startup tooltips and summoning are
    /// covered (or recorded as gaps) in docs/SPEAR-BOOTSTRAP-TOOLTIP-REPAIR.md.
    /// </summary>
    internal static class SpearBootstrapTooltipRepairTests
    {
        private const string OwnId = "KingmakerGunslinger";

        private static SpearProfileObservation Healthy(string predicate)
        {
            return new SpearProfileObservation(predicate, "x", "x", "x",
                "x", "x", "x", null, "not-read");
        }

        private static SpearProfileObservation[] HealthyProfile()
        {
            return new[] { "category", "attack-type", "attack-range",
                "base-damage", "damage-type", "critical-edge",
                "critical-modifier", "fighter-group", "two-handed", "light",
                "monk", "natural", "unarmed", "attack-stat-override", "weight" }
                .Select(Healthy).ToArray();
        }

        private static SpearProfileObservation[] With(
            SpearProfileObservation replacement)
        {
            return HealthyProfile().Select(value => string.Equals(
                value.Predicate, replacement.Predicate, StringComparison.Ordinal)
                    ? replacement : value).ToArray();
        }

        internal static void EngineAttackRangeFollowsKingmakerContract()
        {
            // Kingmaker 2.1.7b BlueprintWeaponType.AttackRange: <= 10 ft is
            // reduced by 4 ft (minimum 2 ft); longer ranges are unchanged.
            Assertions.Equal(6, ElvenBranchedSpearProfilePolicy
                .EngineAttackRangeFeet(10f), "Native 10 ft reach must read 6 ft.");
            Assertions.Equal(2, ElvenBranchedSpearProfilePolicy
                .EngineAttackRangeFeet(5f), "5 ft reach must floor at 2 ft.");
            Assertions.Equal(5, ElvenBranchedSpearProfilePolicy
                .EngineAttackRangeFeet(9f), "9 ft reach must read 5 ft.");
            Assertions.Equal(11, ElvenBranchedSpearProfilePolicy
                .EngineAttackRangeFeet(11f), "Ranges above 10 ft are unchanged.");
            Assertions.Equal(15, ElvenBranchedSpearProfilePolicy
                .EngineAttackRangeFeet(15f), "15 ft reach is unchanged.");
            Assertions.Equal(10, ElvenBranchedSpearProfilePolicy.ReachFeet,
                "The owned spear keeps the native Longspear's 10 ft reach.");
        }

        internal static void HealthyProfilePassesWithoutFindings()
        {
            SpearProfileVerdict verdict = ElvenBranchedSpearProfilePolicy.Evaluate(
                HealthyProfile(), OwnId);
            Assertions.True(verdict.IsValid && verdict.Findings.Length == 0 &&
                verdict.CheckedCount == 15, "A healthy profile produced findings.");
            Assertions.Equal(string.Empty, verdict.DescribeFailure(),
                "A healthy profile described a failure.");
            Assertions.Equal(string.Empty, verdict.DescribeExternal(),
                "A healthy profile described an external patch.");
        }

        internal static void OwnedFieldFailureNamesPredicateAndValues()
        {
            // Simulated: an owned clone carrying a mutated donor's 15 ft reach.
            var reach = new SpearProfileObservation("attack-range", "10ft", "15ft",
                "15ft", "15", "15", "15", new[] { "SomeOtherMod" }, "read");
            var group = new SpearProfileObservation("fighter-group", "Spears",
                "Polearms", "Polearms", "Polearms", "Polearms", "Polearms", null,
                "unavailable:InvalidOperationException");
            SpearProfileObservation[] profile = With(reach).Select(value =>
                value.Predicate == "fighter-group" ? group : value).ToArray();
            SpearProfileVerdict verdict = ElvenBranchedSpearProfilePolicy.Evaluate(
                profile, OwnId);
            string message = verdict.DescribeFailure();
            Assertions.False(verdict.IsValid, "An owned-field mismatch passed.");
            Assertions.Equal(2, verdict.Fatal.Length, "Both failures must be fatal.");
            Assertions.True(message.StartsWith(
                "Elven Branched Spear type profile is invalid: 2 of 15 checks failed.",
                StringComparison.Ordinal), "Failure summary is wrong: " + message);
            foreach (string token in new[] {
                "attack-range [owned-field]: expected=10ft actual=15ft donor=15ft",
                "getter expected=15 actual=15 donor=15",
                "getterPatches=SomeOtherMod",
                "fighter-group [owned-field]: expected=Spears actual=Polearms donor=Polearms",
                "getterPatches=unavailable:InvalidOperationException" })
                Assertions.True(message.Contains(token),
                    "Failure message lacks '" + token + "': " + message);
            Assertions.False(message.Contains("critical-edge"),
                "Failure message names a passing predicate: " + message);
        }

        internal static void UnpatchedGetterMismatchIsFatal()
        {
            var range = new SpearProfileObservation("attack-range", "10ft", "10ft",
                "10ft", "6ft", "10ft", "10ft", null,
                "unavailable:InvalidOperationException");
            SpearProfileVerdict verdict = ElvenBranchedSpearProfilePolicy.Evaluate(
                With(range), OwnId);
            Assertions.False(verdict.IsValid, "An unexplained getter change passed.");
            Assertions.True(verdict.Fatal.Single().Kind ==
                SpearProfileFindingKind.UnpatchedEngineMismatch,
                "Unpatched getter mismatch was misclassified.");
            Assertions.True(verdict.DescribeFailure().Contains(
                "attack-range [unpatched-getter]: expected=10ft actual=10ft donor=10ft; getter expected=6ft actual=10ft donor=10ft"),
                "Unpatched getter diagnostics are incomplete: " +
                verdict.DescribeFailure());
        }

        internal static void OwnPatchOnlyGetterMismatchIsFatal()
        {
            var group = new SpearProfileObservation("fighter-group", "Spears",
                "Spears", "Spears", "Spears", "Polearms", "Polearms",
                new[] { OwnId }, "read");
            SpearProfileVerdict verdict = ElvenBranchedSpearProfilePolicy.Evaluate(
                With(group), OwnId);
            Assertions.False(verdict.IsValid,
                "A getter changed only by this mod's own patch passed.");
            Assertions.True(verdict.Fatal.Single().Kind ==
                SpearProfileFindingKind.UnpatchedEngineMismatch,
                "Own-patch getter mismatch was misclassified.");
        }

        internal static void ExternalGetterPatchIsReportedNotFatal()
        {
            // Simulated: another mod postfixes the shared AttackRange getter
            // for every weapon; the owned 10 ft field is still exact.
            var range = new SpearProfileObservation("attack-range", "10ft", "10ft",
                "10ft", "6ft", "11ft", "11ft",
                new[] { OwnId, "zeta.reach", "alpha.reach" }, "read");
            SpearProfileVerdict verdict = ElvenBranchedSpearProfilePolicy.Evaluate(
                With(range), OwnId);
            Assertions.True(verdict.IsValid,
                "Another mod's shared getter patch failed the owned profile.");
            Assertions.Equal(1, verdict.External.Length,
                "The external getter patch was not reported.");
            string note = verdict.DescribeExternal();
            Assertions.True(note.Contains("attack-range [external-getter-patch]") &&
                note.Contains("getter expected=6ft actual=11ft donor=11ft") &&
                note.Contains("getterPatches=" + OwnId + ",alpha.reach,zeta.reach"),
                "External patch note is incomplete: " + note);
            Assertions.Equal(string.Empty, verdict.DescribeFailure(),
                "An external-only verdict described a failure.");
        }

        internal static void OwnedFieldMismatchStaysFatalUnderExternalPatch()
        {
            var range = new SpearProfileObservation("attack-range", "10ft", "15ft",
                "15ft", "15ft", "16ft", "16ft", new[] { "alpha.reach" }, "read");
            SpearProfileVerdict verdict = ElvenBranchedSpearProfilePolicy.Evaluate(
                With(range), OwnId);
            Assertions.False(verdict.IsValid,
                "An external getter patch excused a wrong owned field.");
            Assertions.True(verdict.Fatal.Single().Kind ==
                SpearProfileFindingKind.OwnedFieldMismatch,
                "Owned mismatch was not classified first.");
        }

        internal static void DiagnosticsAreBounded()
        {
            string longValue = new string('v', 500);
            var owners = Enumerable.Range(0, 20).Select(index => "mod" +
                index.ToString("D2")).ToArray();
            var range = new SpearProfileObservation("attack-range", "10ft",
                longValue, longValue, "6ft", "7ft", "7ft", owners, "read");
            string message = ElvenBranchedSpearProfilePolicy.Evaluate(With(range),
                OwnId).DescribeFailure();
            Assertions.False(message.Contains(new string('v',
                ElvenBranchedSpearProfilePolicy.MaxDescribedValueLength + 1)),
                "A described value was not truncated.");
            Assertions.True(message.Contains("mod07,+12") &&
                !message.Contains("mod08"), "Patch owners were not capped: " + message);
            Assertions.True(message.Length < 1200,
                "One failed predicate produced an unbounded message.");

            var many = Enumerable.Range(0, 40).Select(index =>
                new SpearProfileObservation("p" + index.ToString("D2"), "a", "b",
                    "c", "d", "d", "d", null, "not-read")).ToArray();
            string capped = ElvenBranchedSpearProfilePolicy.Evaluate(many, OwnId)
                .DescribeFailure();
            Assertions.True(capped.Contains("40 of 40 checks failed") &&
                capped.Contains("(+16 more)") && !capped.Contains("p24 ["),
                "Described findings were not capped: " + capped);
        }

        internal static void ObservationsMustBeUniqueAndComplete()
        {
            bool refused = false;
            try
            {
                ElvenBranchedSpearProfilePolicy.Evaluate(new[] { Healthy("weight"),
                    Healthy("weight") }, OwnId);
            }
            catch (ArgumentException) { refused = true; }
            Assertions.True(refused, "Duplicate predicates were accepted.");
            refused = false;
            try
            {
                ElvenBranchedSpearProfilePolicy.Evaluate(
                    new SpearProfileObservation[0], OwnId);
            }
            catch (ArgumentException) { refused = true; }
            Assertions.True(refused, "An empty profile was accepted as valid.");
        }

        internal static void BatteredInspectionIsExemptOnlyWhenUnavailable()
        {
            // Token registered: always inspect (normal pricing, any module state).
            Assertions.True(BatteredFirearmOriginInspectionPolicy.Decide(true, false,
                true) == BatteredOriginInspection.Inspect,
                "A registered token was not inspected.");
            // Failed initialization without the token: native price.
            Assertions.True(BatteredFirearmOriginInspectionPolicy.Decide(false, true,
                false) == BatteredOriginInspection.Unavailable,
                "A failed initialization still demanded the token.");
            // Initialization not yet complete (main menu, before load).
            Assertions.True(BatteredFirearmOriginInspectionPolicy.Decide(false, false,
                false) == BatteredOriginInspection.Unavailable,
                "A pending initialization still demanded the token.");
            // Completed initialization without the token is an invariant
            // violation: inspect, so the strict requirement throws.
            Assertions.True(BatteredFirearmOriginInspectionPolicy.Decide(true, false,
                false) == BatteredOriginInspection.Inspect,
                "A missing token after successful initialization was suppressed.");
        }
    }
}
