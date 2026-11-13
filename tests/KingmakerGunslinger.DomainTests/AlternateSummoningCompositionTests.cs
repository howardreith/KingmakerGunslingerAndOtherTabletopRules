using System.Collections.Generic;
using System.Linq;
using KingmakerGunslinger.Summoning;

namespace KingmakerGunslinger.DomainTests
{
    internal static class AlternateSummoningCompositionTests
    {
        private static ComposedActionView Template(params string[] replaced)
        {
            return new ComposedActionView {
                Kind = ComposedActionKind.Template, Type = "Template",
                CelestialTemplate = "celestial", FiendishTemplate = "fiendish",
                CelestialSmite = "smite-c", FiendishSmite = "smite-f",
                NeutralFiendishMode = "neutral", ReplacedNativeTemplateBuffs = replaced
            };
        }

        private static ComposedActionView Alignment(string mode)
        {
            return new ComposedActionView { Kind = ComposedActionKind.Alignment,
                Type = "Alignment", AlignmentMode = mode };
        }

        /// <summary>
        /// GS-AUD-0026: inherited-template removal, the action that used to be
        /// classified as Other and therefore never compared.
        /// </summary>
        private static ComposedActionView Removal(string owner, params string[] native)
        {
            return new ComposedActionView {
                Kind = ComposedActionKind.TemplateRemoval, Type = "TemplateRemoval",
                OwningUnit = owner, ReplacedNativeTemplateBuffs = native
            };
        }

        private static IReadOnlyList<IReadOnlyList<ComposedActionView>> Branches(
            params ComposedActionView[][] branches)
        {
            return branches.Select(value => (IReadOnlyList<ComposedActionView>)value.ToList())
                .ToList();
        }

        private static string Expect(ComposedActionView model)
        {
            return AlternateSummoningComposition.Describe(model);
        }

        internal static void UnchangedCompositionPasses()
        {
            ComposedActionView expected = Template("native-a", "native-b");
            Assertions.Equal(null, AlternateSummoningComposition.Verify(Branches(
                new[] { ComposedActionView.Other("Host.After"), Template("native-a", "native-b") },
                new[] { Template("native-b", "native-a", "native-b") }), expected),
                "Each branch ending with the canonical template passes; order and repeats of replaced buffs carry no behavior.");
            Assertions.Equal(null, AlternateSummoningComposition.Verify(Branches(
                new[] { Alignment("Caster") }), Alignment("Caster")),
                "The canonical alignment mode passes.");
            Assertions.Equal(null, AlternateSummoningComposition.Verify(Branches(
                new[] { ComposedActionView.Other("Host.After") }), null),
                "A child that composes nothing passes with no composed action.");
        }

        /// <summary>
        /// GS-AUD-0025 / GS-AUD-0026. This is the regression the old comparator
        /// could not express: the canonical child removes the donor's inherited
        /// template, the adapter child does not, and before this repair the
        /// removal action was classified as Other and excluded from comparison,
        /// so a child missing it compared as equal.
        /// </summary>
        internal static void MissingTemplateRemovalFails()
        {
            ComposedActionView expected = Removal("unit-hound-archon",
                "native-a", "native-b", "native-c", "native-d");

            // The defect: the child composes nothing at all.
            Assertions.Equal(
                "placement:branch0:expected-one-final-templateremoval;actual=[Host.After]",
                AlternateSummoningComposition.Verify(Branches(
                    new[] { ComposedActionView.Other("Host.After") }), expected),
                "A child that omits the inherited-template removal must fail.");

            // An empty branch is also a failure, not a pass.
            Assertions.Equal(
                "placement:branch0:expected-one-final-templateremoval;actual=[]",
                AlternateSummoningComposition.Verify(Branches(
                    new ComposedActionView[0]), expected),
                "A child with no after-spawn action at all must fail.");

            // A second branch that omits it is reported even when the first is correct.
            Assertions.Equal(
                "placement:branch1:expected-one-final-templateremoval;actual=[Host.After]",
                AlternateSummoningComposition.Verify(Branches(
                    new[] { Removal("unit-hound-archon", "native-a", "native-b",
                        "native-c", "native-d") },
                    new[] { ComposedActionView.Other("Host.After") }), expected),
                "Every spawn branch is evaluated, not just the first.");
        }

        internal static void TemplateRemovalIdentityIsCompared()
        {
            ComposedActionView expected = Removal("unit-hound-archon",
                "native-a", "native-b", "native-c", "native-d");

            // Correct composition passes, and the native identities carry no order.
            Assertions.Equal(null, AlternateSummoningComposition.Verify(Branches(
                new[] { ComposedActionView.Other("Host.After"),
                    Removal("unit-hound-archon", "native-d", "native-c",
                        "native-b", "native-a") }), expected),
                "A correct removal bound to the same unit and identities passes.");

            // Bound to the wrong unit: a removal that would never run.
            Assertions.Equal(
                "branch0:template-removal-owner:expected=unit-hound-archon;actual=unit-lemure",
                AlternateSummoningComposition.Verify(Branches(
                    new[] { Removal("unit-lemure", "native-a", "native-b",
                        "native-c", "native-d") }), expected),
                "A removal bound to a different unit is reported exactly.");

            // A missing native identity changes which buffs are stripped.
            Assertions.Equal(
                "branch0:template-removal-native-buffs:expected=[native-a,native-b,native-c,native-d];" +
                "actual=[native-a,native-b,native-c]",
                AlternateSummoningComposition.Verify(Branches(
                    new[] { Removal("unit-hound-archon", "native-a", "native-b",
                        "native-c") }), expected),
                "A dropped native template identity is reported exactly.");

            // A DUPLICATED identity is also a failure. The run-time action
            // requires exactly the four native identities and throws on a
            // five-entry array, so a set comparison here would have passed a
            // composition that cannot execute.
            Assertions.Equal(
                "branch0:template-removal-native-buffs:expected=[native-a,native-b,native-c,native-d];" +
                "actual=[native-a,native-a,native-b,native-c,native-d]",
                AlternateSummoningComposition.Verify(Branches(
                    new[] { Removal("unit-hound-archon", "native-a", "native-b",
                        "native-c", "native-d", "native-a") }), expected),
                "A duplicated native template identity must fail, not pass as an equal set.");

            // A repeated identity standing in for a missing one must also fail.
            Assertions.Equal(
                "branch0:template-removal-native-buffs:expected=[native-a,native-b,native-c,native-d];" +
                "actual=[native-a,native-b,native-c,native-c]",
                AlternateSummoningComposition.Verify(Branches(
                    new[] { Removal("unit-hound-archon", "native-a", "native-b",
                        "native-c", "native-c") }), expected),
                "A repeated identity must not substitute for a missing one.");

            Assertions.Equal(
                "branch0:template-removal-native-buffs:null-entry",
                AlternateSummoningComposition.Verify(Branches(
                    new[] { Removal("unit-hound-archon", "native-a", null,
                        "native-c", "native-d") }), expected),
                "A null native template identity is rejected.");

            // Ordering and nesting matter: the removal must be the final action.
            Assertions.Equal(
                "placement:branch0:expected-one-final-templateremoval;actual=[templateremoval,Host.After]",
                AlternateSummoningComposition.Verify(Branches(
                    new[] { Removal("unit-hound-archon", "native-a", "native-b",
                        "native-c", "native-d"),
                        ComposedActionView.Other("Host.After") }), expected),
                "A removal that is not the final after-spawn action is reported.");

            // A removal where none is expected is a divergence too.
            Assertions.Equal("placement:branch0:unexpected-templateremoval",
                AlternateSummoningComposition.Verify(Branches(
                    new[] { Removal("unit-boar", "native-a") }), null),
                "An unexpected removal on a child that should compose nothing is reported.");

            // The removal is distinguished from the other composed kinds rather
            // than collapsing into them.
            Assertions.Equal(
                "placement:branch0:expected-one-final-templateremoval;actual=[alignment]",
                AlternateSummoningComposition.Verify(Branches(
                    new[] { Alignment("Caster") }), expected),
                "An alignment action must not satisfy an expected removal.");
            Assertions.Equal(
                "placement:branch0:expected-one-final-alignment;actual=[templateremoval]",
                AlternateSummoningComposition.Verify(Branches(
                    new[] { Removal("unit-hound-archon", "native-a") }),
                    Alignment("Caster")),
                "A removal action must not satisfy an expected alignment.");
        }

        internal static void ChangedAlignmentModeFails()
        {
            string result = AlternateSummoningComposition.Verify(Branches(
                new[] { Alignment("Celestial") }), Alignment("Caster"));
            Assertions.Equal("branch0:alignment-mode:expected=Caster;actual=Celestial", result,
                "A changed mode under the same action type is reported exactly.");
        }

        internal static void ChangedReplacementBuffsFail()
        {
            ComposedActionView expected = Template("native-a", "native-b");
            Assertions.Equal("branch0:replaced-native-template-buffs:missing=[native-b];extra=[]",
                AlternateSummoningComposition.Verify(Branches(new[] { Template("native-a") }),
                    expected), "A removed replacement buff is reported.");
            Assertions.Equal("branch0:replaced-native-template-buffs:missing=[native-b];extra=[other]",
                AlternateSummoningComposition.Verify(Branches(new[] { Template("native-a", "other") }),
                    expected), "A changed replacement buff is reported.");
            Assertions.Equal("branch0:replaced-native-template-buffs:null-entry",
                AlternateSummoningComposition.Verify(Branches(new[] {
                    Template("native-a", "native-b", null) }), expected),
                "A null replacement entry is reported.");
        }

        internal static void ChangedTemplateReferenceFails()
        {
            ComposedActionView changed = Template("native-a");
            changed.CelestialTemplate = "other";
            Assertions.Equal("branch0:template-buffs:celestial-template:expected=celestial;actual=other",
                AlternateSummoningComposition.Verify(Branches(new[] { changed }),
                    Template("native-a")), "A changed template reference is reported.");
            ComposedActionView neutral = Template("native-a");
            neutral.NeutralFiendishMode = null;
            Assertions.Equal("branch0:template-buffs:neutral-fiendish-mode:expected=neutral;actual=",
                AlternateSummoningComposition.Verify(Branches(new[] { neutral }),
                    Template("native-a")), "A dropped neutral-mode buff is reported.");
        }

        internal static void MisplacedCompositionFails()
        {
            ComposedActionView expected = Template();
            string missing = AlternateSummoningComposition.Verify(Branches(
                new[] { Template() }, new[] { ComposedActionView.Other("Host.After") }), expected);
            Assertions.True(missing != null && missing.StartsWith("placement:branch1:",
                System.StringComparison.Ordinal), "A spawn branch without the action fails: " + missing);
            string twice = AlternateSummoningComposition.Verify(Branches(
                new[] { Template(), Template() }), expected);
            Assertions.True(twice != null && twice.StartsWith("placement:branch0:",
                System.StringComparison.Ordinal), "A duplicated action fails: " + twice);
            string early = AlternateSummoningComposition.Verify(Branches(
                new[] { Template(), ComposedActionView.Other("Host.After") }), expected);
            Assertions.True(early != null && early.StartsWith("placement:branch0:",
                System.StringComparison.Ordinal), "An action that is not last fails: " + early);
            string kind = AlternateSummoningComposition.Verify(Branches(
                new[] { Alignment("Caster") }), expected);
            Assertions.True(kind != null && kind.StartsWith("placement:branch0:",
                System.StringComparison.Ordinal), "The other composed kind fails: " + kind);
            string unexpected = AlternateSummoningComposition.Verify(Branches(
                new[] { Alignment("Caster") }), null);
            Assertions.Equal("placement:branch0:unexpected-alignment", unexpected,
                "A child that should compose nothing may carry no composed action.");
        }

        internal static void CanonicalMustBeConsistent()
        {
            string inconsistency;
            ComposedActionView model = AlternateSummoningComposition.Expected(Branches(
                new[] { Template("a") }, new[] { Template("b") }), out inconsistency);
            Assertions.True(model == null && inconsistency == "canonical-composition-inconsistent",
                "Disagreeing canonical branches are refused.");
            model = AlternateSummoningComposition.Expected(Branches(
                new[] { Template("a") }, new[] { ComposedActionView.Other("Host.After") }),
                out inconsistency);
            Assertions.True(model == null && inconsistency == "canonical-composition-inconsistent",
                "A canonical branch without its composed action is refused.");
            model = AlternateSummoningComposition.Expected(Branches(
                new[] { Template("a", "b") }, new[] { Template("b", "a") }), out inconsistency);
            Assertions.True(inconsistency == null && Expect(model) == Expect(Template("a", "b")),
                "Branches that agree up to replaced-buff order give one model.");
        }
    }
}
