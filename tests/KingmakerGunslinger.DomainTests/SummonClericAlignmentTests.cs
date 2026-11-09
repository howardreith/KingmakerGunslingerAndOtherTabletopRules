using System;
using System.Collections.Generic;
using System.Linq;
using KingmakerGunslinger.Summoning;

namespace KingmakerGunslinger.DomainTests
{
    internal static class SummonClericAlignmentTests
    {
        private const string Cleric = SummonClericAlignmentPolicy.ClericClassGuid;
        private const string Sorcerer = "b3a505fb61437dc4097f43c3f8f9a4cf";
        private const string Erastil = "afc775188deb7a44aa4cbde03512c671", Asmodeus = "a3a5ccc9c670e6f4ca4a686d23b89900";
        private const string Pharasma = "458750bc214ab2e44abdeae404ab22e9", Unknown = "ffffffffffffffffffffffffffffffff";
        private const int LG = 10, NG = 3, CG = 18, LN = 9, TN = 1, CN = 17, LE = 12, NE = 5, CE = 20;

        private static SummonClericFacts Facts(int alignment, params string[] deities)
        { return new SummonClericFacts(new[] { Cleric }, alignment, deities); }

        private static SummonSpellAlignment Row(string key, bool starred = false)
        { return SummonClericAlignmentCatalog.For(key, starred); }

        private static bool Allowed(SummonClericFacts facts, string key, bool starred = false)
        { return SummonClericAlignmentPolicy.Permits(SummonClericAlignmentPolicy.Decide(facts, Row(key, starred))); }

        internal static void BothAxesAndDeityRestrictionsApply()
        {
            Assertions.False(Allowed(Facts(LG), "vrock"), "A lawful good cleric cannot summon a chaotic evil demon.");
            Assertions.True(Allowed(Facts(LG), "lantern-archon"), "A lawful good cleric may summon an archon.");
            Assertions.False(Allowed(Facts(LN), "bralani-azata"), "Lawful forbids the chaotic axis alone.");
            Assertions.True(Allowed(Facts(LN), "lantern-archon"), "Lawful neutral has no objection to lawful good: good is not opposed.");
            Assertions.False(Allowed(Facts(NG), "lemure"), "Good forbids the evil axis alone.");
            Assertions.False(Allowed(Facts(TN, Asmodeus), "lantern-archon"),
                "A neutral cleric of a lawful evil deity gains the deity's prohibition on Good.");
            Assertions.False(Allowed(Facts(TN, Asmodeus), "bralani-azata"), "The deity's prohibition covers Chaos too.");
            Assertions.True(Allowed(Facts(TN, Asmodeus), "lemure"), "The deity does not forbid its own alignment.");
            Assertions.False(Allowed(Facts(CN, Erastil), "lemure"), "Deity restrictions add to the character's.");
        }

        internal static void TrueNeutralAndStarredChoicesFollowTheCaster()
        {
            Assertions.True(Allowed(Facts(TN), "vrock"), "A true neutral cleric with no deity has no prohibition.");
            Assertions.True(Allowed(Facts(TN), "lantern-archon"), "Neutral contributes nothing.");
            Assertions.True(Allowed(Facts(TN, Pharasma), "vrock"), "A neutral deity contributes nothing.");
            Assertions.True(Allowed(Facts(TN), "wolf", true), "A neutral caster's starred choice is not a Good or Evil spell.");
            Assertions.True(Allowed(Facts(LE), "wolf", true), "A starred creature matches its evil caster.");
            Assertions.True(Allowed(Facts(LG, Erastil), "wolf", true), "A starred creature matches its good caster.");
            Assertions.True(Row("wolf", true).CasterDependent && Row("wolf", true).EffectiveFor(SummonAlignmentAxes.Evil) == SummonAlignmentAxes.Evil,
                "A starred spell takes the caster's alignment.");
            Assertions.False(Allowed(Facts(CN, Erastil), "wolf", true),
                "A starred spell matches the caster, so a lawful deity still forbids a chaotic caster's choice.");
        }

        internal static void TableExceptionsUseTheirPrintedSubtypes()
        {
            Assertions.True(Row("astral-deva").Descriptors == SummonAlignmentAxes.Good, "Astral Deva is Good only.");
            Assertions.True(Allowed(Facts(CG), "astral-deva"), "A chaotic good cleric may summon the Good-only Astral Deva.");
            Assertions.False(Allowed(Facts(CG), "trumpet-archon"), "A lawful archon stays forbidden to a chaotic cleric.");
            Assertions.True(Allowed(Facts(LG), "astral-deva"), "A lawful good cleric may summon it.");
            Assertions.False(Allowed(Facts(LG), "ghaele-azata"), "A chaotic azata stays forbidden to a lawful cleric.");
            Assertions.True(Row("salamander").Descriptors == SummonAlignmentAxes.Evil, "Salamander is printed Evil only.");
            Assertions.True(Allowed(Facts(CE), "salamander"), "A chaotic evil cleric may summon a Salamander.");
            Assertions.True(Row("xill").Descriptors == (SummonAlignmentAxes.Evil | SummonAlignmentAxes.Law), "Xill is Evil, Lawful.");
            Assertions.False(Allowed(Facts(CE), "xill"), "Chaotic evil opposes Xill's lawful subtype.");
            Assertions.True(Allowed(Facts(LG), "air-mephit") && Allowed(Facts(CE), "small-fire-elemental") && Allowed(Facts(LG), "invisible-stalker"),
                "Elemental and Air subtypes carry no alignment.");
            Assertions.True(Allowed(Facts(LG), "redcap"), "A fey with no alignment subtype adds no descriptor, whatever its alignment.");
            Assertions.False(Allowed(Facts(LG), "soul-eater"), "An Evil-subtype outsider is an Evil spell.");
            Assertions.False(Allowed(Facts(CG), "axiomite"), "A Lawful-subtype outsider is a Lawful spell.");
            Assertions.True(Row("gorgon").Authority.StartsWith("Bestiary", StringComparison.Ordinal), "An addition records its authority.");
        }

        internal static void EveryCurrentChoiceIsClassifiedAcrossQuantities()
        {
            SummonClericAlignmentCatalog.Validate();
            SummonVariantSpec[] variants = ExpandedSummoningCatalog.GenerateVariants(SummonFamily.Monster).ToArray();
            Assertions.True(variants.Length > 0, "The census found no Summon Monster placements.");
            foreach (var group in variants.GroupBy(value => value.Creature.Key))
            {
                SummonSpellAlignment[] rows = group.Select(value => SummonClericAlignmentCatalog.For(value.Creature.Key, value.Creature.MonsterTemplated)).ToArray();
                Assertions.True(rows.All(value => value.CasterDependent == rows[0].CasterDependent && value.Descriptors == rows[0].Descriptors),
                    "A creature's legality is identical across its quantity variants: " + group.Key);
            }
            foreach (SummonNativeExpansionSpec native in SummonNativeExpansionCatalog.All.Where(value => value.Family == SummonFamily.Monster))
                Assertions.False(string.IsNullOrEmpty(SummonClericAlignmentCatalog.For(native.IconKey, false).Authority), "Native addition needs authority.");
            Assertions.Throws<InvalidOperationException>(() => SummonClericAlignmentCatalog.For("no-such-creature", false),
                "An unclassified row must not silently count as neutral.");
            Assertions.False(variants.Any(value => value.Creature.Key == "stirge"), "Stirge has no Summon Monster route.");
        }

        internal static void ClericMembershipComesFromRealClassLevels()
        {
            var multiclass = new SummonClericFacts(new[] { Sorcerer, Cleric }, LG, new string[0]);
            Assertions.False(SummonClericAlignmentPolicy.Permits(SummonClericAlignmentPolicy.Decide(multiclass, Row("vrock"))),
                "A multiclass cleric stays restricted, whichever spellbook is used.");
            var sorcerer = new SummonClericFacts(new[] { Sorcerer }, LG, new[] { Erastil });
            Assertions.Equal(SummonClericDecision.NotCleric, SummonClericAlignmentPolicy.Decide(sorcerer, Row("vrock")),
                "A deity does not make a non-cleric a cleric.");
            var none = new SummonClericFacts(new string[0], LG, new[] { Erastil });
            Assertions.True(SummonClericAlignmentPolicy.Permits(SummonClericAlignmentPolicy.Decide(none, Row("vrock"))),
                "A unit with no cleric class levels, even with a cloned cleric book, is unrestricted.");
        }

        internal static void UnknownDeityIsNeverNeutral()
        {
            Assertions.Equal(SummonClericDecision.ForbiddenUnknownDeity, SummonClericAlignmentPolicy.Decide(Facts(TN, Unknown), Row("vrock")),
                "An unidentified deity must not read as neutral.");
            Assertions.Equal(SummonClericDecision.Allowed, SummonClericAlignmentPolicy.Decide(Facts(TN, Unknown), Row("air-mephit")),
                "A row with no alignment needs no deity knowledge.");
            Assertions.True(Allowed(Facts(LG, Unknown), "wolf", true), "A starred row still matches its caster.");
            Assertions.True(Allowed(Facts(TN, SummonClericDeityCatalog.AtheismGuid), "vrock"), "Atheism is a known absence of deity.");
            Assertions.Equal(26, SummonClericDeityCatalog.Guids.Count(), "The installed selection's 25 deities and Atheism.");
            Assertions.False(Allowed(Facts(TN, "8b535b6842e063d48a571a042c3c6e8f"), "lantern-archon"), "Gyronna is chaotic evil.");
            Assertions.False(Allowed(Facts(TN, "0a111217f49248eabdf8e9abe2537973"), "vrock"), "Ragathiel is lawful good.");
        }

        internal static void TwoCastersShareRowsAndOrderIsPreserved()
        {
            string[] shared = { "air-mephit", "lantern-archon", "lemure", "salamander", "vrock", "wolf" };
            string[] before = shared.ToArray();
            Func<SummonClericFacts, string[]> menu = facts => SummonClericAlignmentPolicy.RetainPermitted(shared,
                key => SummonClericAlignmentPolicy.Permits(SummonClericAlignmentPolicy.Decide(facts, Row(key, key == "wolf")))).ToArray();
            Assertions.Equal("air-mephit|lantern-archon|wolf", string.Join("|", menu(Facts(LG, Erastil))), "The lawful good cleric's own menu keeps order.");
            Assertions.Equal("air-mephit|salamander|vrock|wolf", string.Join("|", menu(Facts(CE, SummonClericDeityCatalog.AtheismGuid))),
                "The second caster gets their own menu from the same shared rows.");
            Assertions.True(shared.SequenceEqual(before), "Filtering must not mutate the shared rows.");
            Assertions.Equal("", string.Join("|", SummonClericAlignmentPolicy.RetainPermitted(new string[0], key => true)), "An empty list is handled.");
        }

        private static readonly SummonConversionRoute Herald = DruidSummonConversionPolicy.Herald;

        internal static void HeraldRouteMatchesItsTierAndQuantityArrays()
        {
            Assertions.Equal("8fd74eddd9b6c224693d9ab241f25e84", DruidSummonConversionPolicy.Expected(Herald, 1, SummonMultiplicity.One), "Tier I converts to the shared parent.");
            Assertions.Equal<string>(null, DruidSummonConversionPolicy.Expected(Herald, 1, SummonMultiplicity.OneD3), "Tier I has no group row.");
            Assertions.Equal("7ab27a0d547742741beb5d089f1c3852", DruidSummonConversionPolicy.Expected(Herald, 2, SummonMultiplicity.One), "Tier II single is the exact native row.");
            Assertions.Equal("6223958f3615eeb4d91e52ba57820878", DruidSummonConversionPolicy.Expected(Herald, 2, SummonMultiplicity.OneD3), "Tier II 1d3 is the exact native row.");
            Assertions.Equal("22b9580c961c5e64f95f10ca9a82c564", DruidSummonConversionPolicy.Expected(Herald, 9, SummonMultiplicity.OneD4PlusOne), "Tier IX 1d4+1 is the exact native row.");
            foreach (SummonMultiplicity quantity in Enum.GetValues(typeof(SummonMultiplicity)).Cast<SummonMultiplicity>())
            {
                var array = Enumerable.Range(0, 10).Select(level => level == 0 ? null : DruidSummonConversionPolicy.Expected(Herald, level, quantity)).ToArray();
                Assertions.True(DruidSummonConversionPolicy.MatchesArray(Herald, array, quantity), "The route accepts its own array: " + quantity);
                Assertions.False(DruidSummonConversionPolicy.MatchesArray(DruidSummonConversionPolicy.Druid, array, quantity), "A Herald array is not a Druid array.");
            }
            int tier; SummonMultiplicity resolved;
            Assertions.True(DruidSummonConversionPolicy.TryResolve(Herald, "22b9580c961c5e64f95f10ca9a82c564", out tier, out resolved) && tier == 9 && resolved == SummonMultiplicity.OneD4PlusOne, "Native rows resolve to tier and quantity.");
            Assertions.False(DruidSummonConversionPolicy.TryResolve(Herald, "b8ac9c653789b2a46ad85a075734c0e2", out tier, out resolved), "A Nature's Ally row is not a Herald conversion.");
            Assertions.True(DruidSummonConversionPolicy.RouteForFeature("f6c112a579a81c0439c597f9ee222243") == Herald && DruidSummonConversionPolicy.RouteForClass(Cleric) == Herald, "The route is keyed by exact identity.");
        }

        private sealed class ProjectionRow { internal string Id; }

        internal static void HeraldProjectionKeepsTheChainWithoutDuplicates()
        {
            var single = new ProjectionRow { Id = "native-single" }; var group = new ProjectionRow { Id = "native-d3" }; var other = new ProjectionRow { Id = "cure" };
            var wolf = new ProjectionRow { Id = "wolf" }; var bear = new ProjectionRow { Id = "bear" }; var ape = new ProjectionRow { Id = "ape" };
            ProjectionRow[] original = { single, other, group };
            ProjectionRow[] projected = DruidSummonConversionPolicy.Project(original, new Dictionary<ProjectionRow, ProjectionRow[]> { { single, new[] { wolf } }, { group, new[] { ape, bear } } });
            ProjectionRow[] parentOrder = { ape, bear, wolf };
            ProjectionRow[] ordered = DruidSummonConversionPolicy.OrderProjectedChoices(projected, row => { int index = Array.IndexOf(parentOrder, row); return index < 0 ? (int?)null : index; });
            Assertions.Equal("ape|cure|bear|wolf", string.Join("|", ordered.Select(value => value.Id)), "Children interleave alphabetically; an unrelated conversion keeps its place.");
            Assertions.Equal(ordered.Length, ordered.Distinct().Count(), "No row is repeated.");
            ProjectionRow[] preparedParent = DruidSummonConversionPolicy.Project(original, new Dictionary<ProjectionRow, ProjectionRow[]>(), new[] { single, group });
            Assertions.Equal("cure", string.Join("|", preparedParent.Select(value => value.Id)), "A prepared Summon Monster drops its own quantity rows, so no creature is listed twice.");
            Assertions.Equal("native-single|cure|native-d3", string.Join("|", original.Select(value => value.Id)), "The native list is unchanged.");
        }

        internal static void RetainedNativeRowsFollowTheirEffectiveBranch()
        {
            SummonNativeOptionSpec[] rows = SummonNativeOptionCatalog.All.Where(value => value.Family == SummonFamily.Monster && value.Tier >= 2).ToArray();
            Assertions.Equal(23, rows.Length, "All 23 native Herald rows above tier I are classified.");
            foreach (var row in rows)
                foreach (SummonAlignmentAxes caster in new[] { SummonAlignmentAxes.None, SummonAlignmentAxes.Good, SummonAlignmentAxes.Evil })
                    Assertions.False(SummonClericAlignmentCatalog.ForNativeOption(row, caster) == null, "Every native row classifies for every caster branch.");
            SummonNativeOptionSpec nineOne = rows.Single(value => value.Guid == "e96593e67d206ab49ad1b567327d1e75");
            Assertions.False(SummonClericAlignmentPolicy.Permits(SummonClericAlignmentPolicy.Decide(Facts(LG, Erastil), SummonClericAlignmentCatalog.ForNativeOption(nineOne, SummonAlignmentAxes.Good | SummonAlignmentAxes.Law))),
                "A lawful good cleric's branch of the tier IX umbrella summons a Ghaele, a chaotic spell.");
            Assertions.True(SummonClericAlignmentPolicy.Permits(SummonClericAlignmentPolicy.Decide(Facts(CE), SummonClericAlignmentCatalog.ForNativeOption(nineOne, SummonAlignmentAxes.Evil | SummonAlignmentAxes.Chaos))),
                "An evil cleric's branch of the same row is the Evil-subtype Thanadaemon, which is legal for them.");
            SummonNativeOptionSpec two = rows.Single(value => value.Guid == "7ab27a0d547742741beb5d089f1c3852");
            Assertions.True(SummonClericAlignmentCatalog.ForNativeOption(two, SummonAlignmentAxes.None).CasterDependent, "A semantic duplicate uses its starred creature's rule, not a union of branches.");
        }

        internal static void DruidRouteIsUnchanged()
        {
            Assertions.Equal(DruidSummonConversionPolicy.TierOneParentGuid, DruidSummonConversionPolicy.Expected(1, SummonMultiplicity.One), "The Druid route keeps its tier I parent.");
            Assertions.Equal("848bd9df8b2643143a7020be7cde8800", DruidSummonConversionPolicy.Expected(2, SummonMultiplicity.One), "The Druid route keeps its Nature's Ally rows.");
            Assertions.True(DruidSummonConversionPolicy.RouteForFeature(DruidSummonConversionPolicy.FeatureGuid) == DruidSummonConversionPolicy.Druid, "The Druid feature keeps its route.");
            Assertions.True(DruidSummonConversionPolicy.RouteForClass("b3a505fb61437dc4097f43c3f8f9a4cf") == null, "Other classes have no conversion route.");
        }

        internal static void ThirdPartyRowsAreCoveredByWhatTheyAre()
        {
            Assertions.Equal(RuntimeTesting.SummonClericThirdPartyOutcome.VerifiedLegal, RuntimeTesting.SummonClericObservationVerdict.ClassifyThirdParty(true, true, 1, true), "A row meeting every condition is covered.");
            Assertions.Equal(RuntimeTesting.SummonClericThirdPartyOutcome.Unresolved, RuntimeTesting.SummonClericObservationVerdict.ClassifyThirdParty(true, false, 1, true), "A known name with alignment descriptors is unresolved.");
            Assertions.Equal(RuntimeTesting.SummonClericThirdPartyOutcome.Unresolved, RuntimeTesting.SummonClericObservationVerdict.ClassifyThirdParty(true, true, 0, true), "A row with no resolved spawn is unresolved.");
            Assertions.Equal(RuntimeTesting.SummonClericThirdPartyOutcome.Unresolved, RuntimeTesting.SummonClericObservationVerdict.ClassifyThirdParty(true, true, 1, false), "A row without its visibility gate is unresolved.");
            Assertions.Equal(RuntimeTesting.SummonClericThirdPartyOutcome.Unresolved, RuntimeTesting.SummonClericObservationVerdict.ClassifyThirdParty(false, true, 1, true), "An unknown row is unresolved, however clean it looks.");
        }

        internal static void FixturePrerequisitesRejectMissingClassOrBook()
        {
            Func<RuntimeTesting.SummonClericObservationVerdict.FixtureReading> good = () => new RuntimeTesting.SummonClericObservationVerdict.FixtureReading {
                ClassLevel = 20, ExpectedLevel = 20, ClassBook = "book", RegisteredBooks = 1, RegisteredBook = "book", BookIsClassBook = true, BookList = "list", ExpectedList = "list",
                PreparedSlot = true, DeityMatchesPrerequisite = true, AlignmentIsExact = true };
            Assertions.Equal(0, RuntimeTesting.SummonClericObservationVerdict.FixtureProblems(good()).Count, "A complete fixture has no problems.");
            var noBook = good(); noBook.RegisteredBooks = 0; noBook.RegisteredBook = null; noBook.BookIsClassBook = false;
            Assertions.True(RuntimeTesting.SummonClericObservationVerdict.FixtureProblems(noBook).Count >= 2, "A cleric with no book is rejected, as on the failed candidate.");
            var noClass = good(); noClass.ClassLevel = 0;
            Assertions.True(RuntimeTesting.SummonClericObservationVerdict.FixtureProblems(noClass).Any(value => value.StartsWith("class-level", StringComparison.Ordinal)), "Missing class data is rejected.");
            var wrongList = good(); wrongList.BookList = "other";
            Assertions.True(RuntimeTesting.SummonClericObservationVerdict.FixtureProblems(wrongList).Contains("wrong-spell-list"), "A wrong spell list is rejected.");
            var noSlot = good(); noSlot.PreparedSlot = false;
            Assertions.True(RuntimeTesting.SummonClericObservationVerdict.FixtureProblems(noSlot).Contains("no-valid-prepared-slot"), "A missing prepared slot is rejected.");
            var fallback = good(); fallback.RegisteredBook = "default-book"; fallback.BookIsClassBook = false;
            Assertions.True(RuntimeTesting.SummonClericObservationVerdict.FixtureProblems(fallback).Contains("registered-book-differs-from-class-book"), "A fallback book that is not the class data's book is rejected.");
        }

        internal static void ValidDeityDerivedCaseIsolatesTheDeity()
        {
            string Abadar = "6122dacf418611540a3c91e67197ee4e";
            Assertions.True(Allowed(Facts(LN, Abadar), "astral-deva"), "A lawful neutral character under a lawful neutral deity may summon the Good-only Astral Deva.");
            Assertions.False(Allowed(Facts(LN, Asmodeus), "astral-deva"), "The same character under a lawful evil deity may not: the deity is the only source.");
        }

        internal static void HeraldCoverageFailsWhenAConversionIsUncovered()
        {
            var parent = new RuntimeTesting.SummonClericObservationVerdict.HeraldConversion { Guid = "parent", SharedParent = true };
            var covered = new RuntimeTesting.SummonClericObservationVerdict.HeraldConversion { Guid = "covered", Projects = true, ExecutionGoverned = true };
            var unguarded = new RuntimeTesting.SummonClericObservationVerdict.HeraldConversion { Guid = "unguarded", Projects = true, ExecutionGoverned = false };
            var unprojected = new RuntimeTesting.SummonClericObservationVerdict.HeraldConversion { Guid = "unprojected", ExecutionGoverned = true };
            Assertions.Equal(0, RuntimeTesting.SummonClericObservationVerdict.UncoveredHeraldConversions(new[] { parent, covered }).Count, "Enforced conversions pass.");
            Assertions.Equal("unguarded|unprojected", string.Join("|", RuntimeTesting.SummonClericObservationVerdict.UncoveredHeraldConversions(new[] { parent, unguarded, unprojected })), "Route identification is not acceptance.");
        }

        internal static void ChangesAfterAChoiceWasObtainedAreReevaluated()
        {
            SummonSpellAlignment archon = Row("lantern-archon");
            Assertions.True(SummonClericAlignmentPolicy.Permits(SummonClericAlignmentPolicy.Decide(Facts(LG, Erastil), archon)), "Legal before the shift.");
            Assertions.False(SummonClericAlignmentPolicy.Permits(SummonClericAlignmentPolicy.Decide(Facts(LE, Erastil), archon)),
                "The same held choice is refused once the caster's alignment moves.");
            Assertions.False(SummonClericAlignmentPolicy.Permits(SummonClericAlignmentPolicy.Decide(Facts(TN, Asmodeus), archon)),
                "The same held choice is refused after a deity change.");
        }
    }
}
