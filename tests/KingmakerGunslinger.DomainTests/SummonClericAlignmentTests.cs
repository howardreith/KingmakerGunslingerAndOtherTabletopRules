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
            Assertions.Equal(21, SummonClericDeityCatalog.Guids.Count(), "Twenty deities and Atheism.");
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
