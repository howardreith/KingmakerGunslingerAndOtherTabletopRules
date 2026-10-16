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
