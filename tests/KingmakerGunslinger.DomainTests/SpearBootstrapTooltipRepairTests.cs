using System;
using System.Linq;
using KingmakerGunslinger.ElvenBranchedSpear;
using KingmakerGunslinger.Gunsmithing;

namespace KingmakerGunslinger.DomainTests
{
    /// <summary>
    /// Production policies behind the spear startup and battered-pricing
    /// repair. Native construction, failed-startup tooltips and summoning are
    /// covered in docs/SPEAR-BOOTSTRAP-TOOLTIP-REPAIR.md.
    /// </summary>
    internal static class SpearBootstrapTooltipRepairTests
    {
        private static readonly string[] Predicates =
        {
            "category", "attack-type", "attack-range", "base-damage",
            "damage-type", "critical-edge", "critical-modifier", "fighter-group",
            "two-handed", "light", "monk", "natural", "unarmed",
            "attack-stat-override", "weight", "movement-enchantment",
            "subcategory-policy"
        };

        private static SpearProfileObservation Healthy(string predicate)
        {
            return new SpearProfileObservation(predicate, "x", "x", "x",
                "x", "x", "x", null, "not-read");
        }

        private static SpearProfileObservation[] HealthyProfile()
        {
            return Predicates.Select(Healthy).ToArray();
        }

        private static SpearProfileObservation[] With(
            params SpearProfileObservation[] replacements)
        {
            return HealthyProfile().Select(value => replacements.FirstOrDefault(
                replacement => string.Equals(replacement.Predicate,
                    value.Predicate, StringComparison.Ordinal)) ?? value).ToArray();
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

        // Stand-in for Kingmaker's WeaponSubCategory (the domain tests do not
        // load game assemblies); only names and the None value matter here.
        private enum SubCategory
        {
            None, Light, Melee, Ranged, Finessable, Thrown, Natural, Monk,
            TwoHanded, Exotic, Simple, Martial, Metal, OneHandedPiercing,
            OneHandedSlashing
        }

        private static readonly SubCategory[] IntendedSubCategories =
        {
            SubCategory.Melee, SubCategory.Finessable, SubCategory.TwoHanded,
            SubCategory.Exotic, SubCategory.Metal
        };

        private static string SubCategoryText(Func<SubCategory, bool> grants)
        {
            return ElvenBranchedSpearProfilePolicy.SubCategoryText(
                IntendedSubCategories,
                (SubCategory[])Enum.GetValues(typeof(SubCategory)),
                SubCategory.None, grants);
        }

        internal static void SubCategoryTextCoversEverySupportedValue()
        {
            Func<SubCategory, bool> policy = value => value == SubCategory.None ||
                IntendedSubCategories.Contains(value);
            string intended = SubCategoryText(policy);
            Assertions.Equal("Melee,Finessable,TwoHanded,Exotic,Metal", intended,
                "The intended policy must read exactly the intended five.");
            // An extra classification outside the intended five (R5-F1).
            string withSimple = SubCategoryText(value => policy(value) ||
                value == SubCategory.Simple);
            Assertions.Equal("Melee,Finessable,TwoHanded,Exotic,Metal,Simple",
                withSimple, "An extra Simple classification must be visible.");
            string withMonk = SubCategoryText(value => policy(value) ||
                value == SubCategory.Monk);
            Assertions.True(withMonk.EndsWith(",Monk", StringComparison.Ordinal),
                "Any other extra classification must be visible.");
            // A missing intended classification.
            Assertions.Equal("Melee,TwoHanded,Exotic,Metal", SubCategoryText(
                value => policy(value) && value != SubCategory.Finessable),
                "A removed Finessable must be visible.");
            // The None wildcard every category grants.
            Assertions.Equal("Melee,Finessable,TwoHanded,Exotic,Metal," +
                ElvenBranchedSpearProfilePolicy.WildcardDenied, SubCategoryText(
                    value => policy(value) && value != SubCategory.None),
                "A denied None wildcard must be visible.");
            // As an observation, an extra classification fails validation.
            SpearProfileVerdict verdict = ElvenBranchedSpearProfilePolicy.Evaluate(
                With(new SpearProfileObservation("subcategory-policy", intended,
                    intended, "<not-applicable>", intended, withSimple,
                    "Melee,TwoHanded,Metal", new[] { "other.mod" }, "read")));
            Assertions.False(verdict.IsValid,
                "An extra effective Simple classification passed validation.");
            Assertions.True(verdict.DescribeFailure().Contains(
                "actual=Melee,Finessable,TwoHanded,Exotic,Metal,Simple") &&
                verdict.DescribeFailure().Contains("getterPatches=other.mod"),
                "The diagnostic must name the extra classification and the patch owner.");
        }

        internal static void ReturnedSubCategoryListIsComparedExactly()
        {
            Func<SubCategory[], string> text = list => ElvenBranchedSpearProfilePolicy
                .ReturnedSubCategoryText(IntendedSubCategories, list);
            string intended = "Melee,Finessable,TwoHanded,Exotic,Metal";
            Assertions.Equal(intended, text(IntendedSubCategories),
                "The intended list must read as the intended five.");
            Assertions.Equal(intended, text(IntendedSubCategories.Reverse().ToArray()),
                "Order of the returned list must not matter.");
            // R6-F3: an extra entry only in the returned list.
            string withSimple = text(IntendedSubCategories.Concat(new[] { SubCategory.Simple }).ToArray());
            Assertions.Equal(intended + ",Simple", withSimple,
                "An extra Simple entry must be visible.");
            Assertions.Equal("Melee,TwoHanded,Exotic,Metal", text(
                IntendedSubCategories.Where(value => value != SubCategory.Finessable).ToArray()),
                "A missing Finessable entry must be visible.");
            Assertions.Equal(intended + ",Metal", text(
                IntendedSubCategories.Concat(new[] { SubCategory.Metal }).ToArray()),
                "A repeated entry must be visible.");
            Assertions.Equal("<none>", text(new SubCategory[0]), "An empty list must be visible.");
            Assertions.Equal("<null>", text(null), "A null list must be visible.");
            SpearProfileVerdict verdict = ElvenBranchedSpearProfilePolicy.Evaluate(
                HealthyProfile().Concat(new[] { new SpearProfileObservation("subcategory-list", intended,
                    intended, "<not-applicable>", intended, withSimple,
                    "Melee,TwoHanded,Metal,Simple", new[] { "other.mod" }, "read") }));
            Assertions.False(verdict.IsValid, "An extra returned Simple entry passed validation.");
            Assertions.True(verdict.DescribeFailure().Contains("subcategory-list [effective-getter]") &&
                verdict.DescribeFailure().Contains("actual=" + withSimple) &&
                verdict.DescribeFailure().Contains("getterPatches=other.mod"),
                "The diagnostic must name the list, the extra entry and the patch owner.");
        }

        internal static void ItemDamageFailureIsOneBoundedDiagnostic()
        {
            // Healthy: override off, effective 1d8.
            SpearProfileVerdict healthy = ElvenBranchedSpearProfilePolicy.Evaluate(new[] {
                new SpearProfileObservation("item-damage", "override=False", "override=False",
                    "override=True", "1d8", "1d8", "2d6", null, "not-read") });
            Assertions.True(healthy.IsValid && ElvenBranchedSpearProfilePolicy
                .DescribeItemFailure("Elven Branched Spear", healthy).Length == 0,
                "A healthy item produced an item-damage failure.");
            // A patched BaseDamage that returns the wrong dice.
            SpearProfileVerdict wrong = ElvenBranchedSpearProfilePolicy.Evaluate(new[] {
                new SpearProfileObservation("item-damage", "override=False", "override=False",
                    "override=True", "1d8", "2d6", "2d6", new[] { "other.mod" }, "read") });
            string message = ElvenBranchedSpearProfilePolicy.DescribeItemFailure("Boughkeeper", wrong);
            Assertions.Equal("Elven Branched Spear item damage is invalid: Boughkeeper: item-damage [effective-getter]: " +
                "expected=override=False actual=override=False donor=override=True; getter expected=1d8 actual=2d6 donor=2d6; " +
                "getterPatches=other.mod", message, "The item-damage diagnostic must name item, values, donor and owner.");
            // A throwing getter with a very long message stays bounded, and an
            // unavailable donor read does not hide the owned failure.
            string thrown = ElvenBranchedSpearProfilePolicy.Threw(new InvalidOperationException(new string('x', 500)));
            string unavailable = ElvenBranchedSpearProfilePolicy.Unavailable(new InvalidOperationException("donor"));
            SpearProfileVerdict throws = ElvenBranchedSpearProfilePolicy.Evaluate(new[] {
                new SpearProfileObservation("item-damage", "override=False", "override=False",
                    unavailable, "1d8", thrown, unavailable, new[] { "other.mod" }, "read") });
            string bounded = ElvenBranchedSpearProfilePolicy.DescribeItemFailure(new string('n', 300), throws);
            Assertions.False(throws.IsValid, "A throwing item BaseDamage passed.");
            Assertions.True(bounded.Length < 600 && bounded.Contains("actual=<throws InvalidOperationException") &&
                bounded.Contains("donor=<unavailable InvalidOperationException: donor>") &&
                bounded.Contains("getterPatches=other.mod"),
                "The throwing item-damage diagnostic must stay bounded and keep donor and owner context.");
        }

        internal static void HealthyProfilePassesWithoutFindings()
        {
            SpearProfileVerdict verdict = ElvenBranchedSpearProfilePolicy.Evaluate(
                HealthyProfile());
            Assertions.True(verdict.IsValid && verdict.Findings.Length == 0 &&
                verdict.CheckedCount == 17, "A healthy profile produced findings.");
            Assertions.Equal(string.Empty, verdict.DescribeFailure(),
                "A healthy profile described a failure.");
        }

        internal static void OwnedFieldFailureNamesPredicateAndValues()
        {
            // Simulated: an owned clone carrying a mutated donor's 15 ft reach.
            var reach = new SpearProfileObservation("attack-range", "10ft", "15ft",
                "15ft", "15ft", "15ft", "15ft", new[] { "SomeOtherMod" }, "read");
            var group = new SpearProfileObservation("fighter-group", "Spears",
                "Polearms", "Polearms", "Polearms", "Polearms", "Polearms", null,
                "unavailable:NullReferenceException");
            SpearProfileVerdict verdict = ElvenBranchedSpearProfilePolicy.Evaluate(
                With(reach, group));
            string message = verdict.DescribeFailure();
            Assertions.False(verdict.IsValid, "An owned-field mismatch passed.");
            Assertions.True(message.StartsWith(
                "Elven Branched Spear type profile is invalid: 2 of 17 checks failed.",
                StringComparison.Ordinal), "Failure summary is wrong: " + message);
            foreach (string token in new[] {
                "attack-range [owned-field]: expected=10ft actual=15ft donor=15ft",
                "getter expected=15ft actual=15ft donor=15ft",
                "getterPatches=SomeOtherMod",
                "fighter-group [owned-field]: expected=Spears actual=Polearms donor=Polearms",
                "getterPatches=unavailable:NullReferenceException" })
                Assertions.True(message.Contains(token),
                    "Failure message lacks '" + token + "': " + message);
            Assertions.False(message.Contains("critical-edge"),
                "Failure message names a passing predicate: " + message);
        }

        internal static void ForeignPatchedGetterMismatchIsFatal()
        {
            // A foreign patch on the shared getter that gives the owned spear a
            // wrong effective reach: the serialized field and donor are right.
            var range = new SpearProfileObservation("attack-range", "10ft", "10ft",
                "10ft", "6ft", "7ft", "6ft",
                new[] { "KingmakerGunslinger", "zeta.reach", "alpha.reach" }, "read");
            SpearProfileVerdict verdict = ElvenBranchedSpearProfilePolicy.Evaluate(
                With(range));
            Assertions.False(verdict.IsValid,
                "A foreign getter patch excused a wrong effective spear value.");
            Assertions.True(verdict.Findings.Single().Kind ==
                SpearProfileFindingKind.EffectiveGetterMismatch,
                "Foreign-patched getter mismatch was misclassified.");
            string message = verdict.DescribeFailure();
            Assertions.True(message.Contains(
                "attack-range [effective-getter]: expected=10ft actual=10ft donor=10ft; getter expected=6ft actual=7ft donor=6ft; getterPatches=KingmakerGunslinger,alpha.reach,zeta.reach"),
                "Foreign-patched getter diagnostics are incomplete: " + message);
        }

        internal static void UnpatchedGetterMismatchIsFatal()
        {
            var group = new SpearProfileObservation("fighter-group", "Spears",
                "Spears", "Spears", "Spears", "Polearms", "Polearms", null,
                "unavailable:NullReferenceException");
            SpearProfileVerdict verdict = ElvenBranchedSpearProfilePolicy.Evaluate(
                With(group));
            Assertions.False(verdict.IsValid,
                "A getter mismatch without readable patch owners passed.");
            Assertions.True(verdict.Findings.Single().Kind ==
                SpearProfileFindingKind.EffectiveGetterMismatch,
                "Unpatched getter mismatch was misclassified.");
        }

        internal static void ThrowingOwnedGetterIsFatalAndReported()
        {
            string threw = ElvenBranchedSpearProfilePolicy.Threw(
                new InvalidOperationException("boom"));
            var light = new SpearProfileObservation("light", "false", "false",
                "false", "false", threw, "false", new[] { "other.mod" }, "read");
            var group = new SpearProfileObservation("fighter-group", "Spears",
                "Polearms", "Spears", "Polearms", "Polearms", "Spears", null,
                "unavailable:NullReferenceException");
            SpearProfileVerdict verdict = ElvenBranchedSpearProfilePolicy.Evaluate(
                With(light, group));
            string message = verdict.DescribeFailure();
            Assertions.Equal(2, verdict.Findings.Length,
                "A throwing getter and a second invalid predicate were not both reported.");
            Assertions.True(message.StartsWith(
                "Elven Branched Spear type profile is invalid: 2 of 17 checks failed.",
                StringComparison.Ordinal) &&
                message.Contains("light [effective-getter]: expected=false actual=false donor=false; getter expected=false actual=<throws InvalidOperationException: boom> donor=false; getterPatches=other.mod") &&
                message.Contains("fighter-group [owned-field]"),
                "Throwing getter diagnostics are incomplete: " + message);
        }

        internal static void UnavailableDonorComparisonIsNotFatal()
        {
            string unavailable = ElvenBranchedSpearProfilePolicy.Unavailable(
                new InvalidOperationException("donor getter"));
            var edge = new SpearProfileObservation("critical-edge", "20", "20",
                "20", "20", "20", unavailable, null, "not-read");
            SpearProfileVerdict verdict = ElvenBranchedSpearProfilePolicy.Evaluate(
                With(edge));
            Assertions.True(verdict.IsValid && verdict.Findings.Length == 0,
                "An unreadable donor comparison failed a correct owned spear.");
            Assertions.True(unavailable.StartsWith("<unavailable InvalidOperationException",
                StringComparison.Ordinal), "Unavailable donor text changed: " + unavailable);
        }

        internal static void DeferredEffectiveValueIsNotJudged()
        {
            string deferred = ElvenBranchedSpearProfilePolicy.Deferred;
            var range = new SpearProfileObservation("attack-range", "10ft", "10ft",
                "15ft", "6ft", deferred, deferred, null, "not-read");
            Assertions.True(ElvenBranchedSpearProfilePolicy.Evaluate(With(range)).IsValid,
                "A deferred effective value failed a correct serialized profile.");
            var group = new SpearProfileObservation("fighter-group", "Spears",
                "Polearms", "Spears", "Polearms", deferred, deferred,
                new[] { "other.mod" }, "read");
            string message = ElvenBranchedSpearProfilePolicy.Evaluate(With(group))
                .DescribeFailure();
            Assertions.True(message.Contains("fighter-group [owned-field]: expected=Spears actual=Polearms donor=Spears; getter expected=Polearms actual=<deferred> donor=<deferred>; getterPatches=other.mod"),
                "A serialized-only failure did not say its getter was not read: " + message);
        }

        internal static void FieldOnlyPredicatesReportEnchantmentAndSubcategories()
        {
            var enchantment = new SpearProfileObservation("movement-enchantment",
                "KMG_ElvenBranchedSpear_MovementOpportunityAccuracy", "<none>",
                "<none>", "KMG_ElvenBranchedSpear_MovementOpportunityAccuracy",
                "<none>", "<none>", null, "field");
            var subcategories = new SpearProfileObservation("subcategory-policy",
                "Melee,Finessable,TwoHanded,Exotic,Metal",
                "Melee,Finessable,TwoHanded,Exotic,Metal,Light", "<not-applicable>",
                "Melee,Finessable,TwoHanded,Exotic,Metal",
                "Melee,Finessable,TwoHanded,Exotic,Metal,Light", "<not-applicable>",
                null, "field");
            string message = ElvenBranchedSpearProfilePolicy.Evaluate(
                With(enchantment, subcategories)).DescribeFailure();
            Assertions.True(message.Contains("2 of 17 checks failed.") &&
                message.Contains("movement-enchantment [owned-field]: expected=KMG_ElvenBranchedSpear_MovementOpportunityAccuracy actual=<none>") &&
                message.Contains("subcategory-policy [owned-field]") &&
                message.Contains("getterPatches=field"),
                "Field-only predicates were not reported: " + message);
        }

        internal static void DiagnosticsAreBounded()
        {
            string longValue = new string('v', 500);
            var owners = Enumerable.Range(0, 20).Select(index => "mod" +
                index.ToString("D2")).ToArray();
            var range = new SpearProfileObservation("attack-range", "10ft",
                longValue, longValue, "6ft", "7ft", "7ft", owners, "read");
            string message = ElvenBranchedSpearProfilePolicy.Evaluate(With(range))
                .DescribeFailure();
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
            string capped = ElvenBranchedSpearProfilePolicy.Evaluate(many)
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
                    Healthy("weight") });
            }
            catch (ArgumentException) { refused = true; }
            Assertions.True(refused, "Duplicate predicates were accepted.");
            refused = false;
            try
            {
                ElvenBranchedSpearProfilePolicy.Evaluate(
                    new SpearProfileObservation[0]);
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
