using System;
using System.Linq;
using KingmakerGunslinger.Spells.Teleportation;

namespace KingmakerGunslinger.DomainTests
{
    internal static class TeleportationReaderTests
    {
        private static TeleportCastSourceSnapshot Reader(string id, int order, TeleportScrollActivationChance chance)
        { return new TeleportCastSourceSnapshot(id, order, id, "11111111111111111111111111111111", "Scroll", TeleportSpellKind.WordOfRecall,
            TeleportCastSourceKind.Scroll, 6, 11, 3, TeleportCastSourceFacts.RequiredScroll, chance, "recall-6-11"); }
        private static TeleportScrollActivationChance Umd(int modifier, int failure = 0)
        { return TeleportScrollActivationChance.Native(true, modifier, 26, failure, false, 0, "native UMD DC 26"); }
        internal static void NativeChecksDetermineSuccess()
        {
            Assertions.Equal(0m, Umd(5).Probability, "Even twenty cannot meet DC 26 with modifier 5.");
            Assertions.Equal(0.05m, Umd(6).Probability, "Only twenty meets DC 26 with modifier 6.");
            Assertions.Equal(0.75m, Umd(20).Probability, "Six through twenty meet native DC 26.");
            Assertions.Equal(1m, Umd(25).Probability, "Native skill checks have no automatic natural-one failure.");
            Assertions.Equal(0.375m, Umd(20, 50).Probability, "A subsequent item failure check applies to successful UMD activation.");
            Assertions.Equal(1m, TeleportScrollActivationChance.Native(true, 16, 26, 0, true, 0, "take ten").Probability,
                "Native take-ten for this scroll category guarantees success only when ten meets the DC.");
            Assertions.Equal(0.5m, TeleportScrollActivationChance.Native(true, 15, 26, 0, true, 0, "take ten").Probability,
                "Take-ten that cannot pass retains the native d20 check.");
            Assertions.Equal(0.85m, TeleportScrollActivationChance.Native(true, 20, 26, 0, false, 2, "conditional bonus").Probability,
                "Native conditional success bonus rescues the two additional successful die faces.");
        }
        internal static void GuaranteedReaderBeatsFallibleAndWinsTies()
        {
            var fallible = Reader("first", 0, Umd(24));
            var guaranteed = Reader("second", 1, TeleportScrollActivationChance.Native(false, 0, 26, 0, false, 0, "no check"));
            Assertions.True(ReferenceEquals(guaranteed, TeleportScrollReaderPolicy.Select(new[] { fallible, guaranteed })),
                "A proven no-check reader beats a fallible reader irrespective of party order.");
            var highUmd = Reader("first", 0, Umd(40));
            Assertions.True(ReferenceEquals(guaranteed, TeleportScrollReaderPolicy.Select(new[] { highUmd, guaranteed })),
                "Equal guaranteed probability prefers the no-check route.");
        }
        internal static void BestActualChanceBeatsHighestDisplayedSkill()
        {
            var highSkill = Reader("high", 0, Umd(40, 60));
            var betterChance = Reader("other", 1, Umd(20));
            Assertions.True(ReferenceEquals(betterChance, TeleportScrollReaderPolicy.Select(new[] { highSkill, betterChance })),
                "The item failure chance can make the highest UMD reader worse.");
            var noUmdButFailure = Reader("class-list", 0, TeleportScrollActivationChance.Native(false, 0, 26, 50, false, 0, "item failure"));
            Assertions.False(noUmdButFailure.ActivationChance.NoCheck, "Class-list membership alone is not a no-check route under an item failure effect.");
            Assertions.True(ReferenceEquals(betterChance, TeleportScrollReaderPolicy.Select(new[] { noUmdButFailure, betterChance })),
                "Native activation chance determines the best reader, not class-list membership alone.");
        }
        internal static void TiesRemainStableAndUnsupportedChancesAreNotInvented()
        {
            var later = Reader("later", 2, Umd(18)); var first = Reader("first", 0, Umd(18));
            for (int i = 0; i < 10; i++)
                Assertions.True(ReferenceEquals(first, TeleportScrollReaderPolicy.Select(i % 2 == 0 ? new[] { later, first } : new[] { first, later })),
                    "Equal probability keeps party-order focus independent of enumeration order.");
            var unknown = Reader("unknown", 1, TeleportScrollActivationChance.Unsupported("unverified active activation hook"));
            Assertions.True(TeleportScrollReaderPolicy.Select(new[] { first, unknown }) == null,
                "Unverified activation behavior does not receive a guessed probability.");
            Assertions.True(ReferenceEquals(unknown, TeleportScrollReaderPolicy.Select(new[] { unknown })),
                "A sole eligible reader needs no probability comparison.");
            Assertions.Equal(3, first.Uses, "Repeated ranking cannot mutate its immutable resource snapshot.");
        }
    }
    internal static partial class TeleportationContextTests
    {
        private static TeleportCastSourceSnapshot GroupReader(string id, int order, string group, int modifier,
            TeleportSpellKind spell = TeleportSpellKind.Teleport, int casterLevel = 9, int uses = 3)
        { return new TeleportCastSourceSnapshot(id, order, id, "11111111111111111111111111111111", "Scroll", spell,
            TeleportCastSourceKind.Scroll, 5, casterLevel, uses, TeleportCastSourceFacts.RequiredScroll,
            TeleportScrollActivationChance.Native(modifier < 100, modifier, 25, 0, false, 0, "fixture native inputs"), group); }
        internal static void EquivalentScrollReadersProduceOneStockRow()
        {
            var low = GroupReader("low", 0, "variant", 10); var best = GroupReader("best", 1, "variant", 100);
            var result = Compose(Point(), new[] { low, best, low });
            Assertions.Equal(1, result.SpellActions.Count, "Equivalent shared stock produces one action despite several eligible readers.");
            var row = result.SpellActions.Single();
            Assertions.True(ReferenceEquals(best, row.Source) && row.ReaderResolved, "The group binds a specific best native reader.");
            Assertions.Equal(3, row.Source.Uses, "Shared inventory is counted once.");
            Assertions.Equal("Use Scroll of Teleport\n3 available", TeleportContextPresentation.CompactRow(row, English), "No routine reader detail.");
        }
        internal static void ScrollReaderChangesPreserveActionIdentity()
        {
            var first = GroupReader("first", 0, "variant", 20); var later = GroupReader("later", 1, "variant", 15);
            var before = Compose(Point(), new[] { first, later }).SpellActions.Single();
            var after = Compose(Point(), new[] { later }).SpellActions.Single();
            Assertions.Equal(before.Key, after.Key, "Reader availability changes keep the same UI action and controller focus key.");
            Assertions.False(before.Source.Key == after.Source.Key, "The actual transaction keeps its real reader identity.");
            NativeOnly(Compose(Point(), new[] { GroupReader("none", 0, "variant", 20, uses: 0) }));
        }
        internal static void BestReaderIsSelectedForEachSpellAndVariant()
        {
            var rows = Compose(Point(WordOfRecallDestinationPolicy.OlegId), new[] {
                GroupReader("a", 0, "teleport-low", 10), GroupReader("b", 1, "teleport-low", 20),
                GroupReader("a", 0, "teleport-high", 21, casterLevel: 11), GroupReader("b", 1, "teleport-high", 9, casterLevel: 11),
                GroupReader("a", 0, "recall", 100, TeleportSpellKind.WordOfRecall), GroupReader("b", 1, "recall", 20, TeleportSpellKind.WordOfRecall)
            }).SpellActions;
            Assertions.Equal(3, rows.Count, "Each distinct spell/variant has one action.");
            Assertions.Equal("b", rows.Single(value => value.Source.ScrollGroupId == "teleport-low").Source.CasterId, "First variant chooses its best native chance.");
            Assertions.Equal("a", rows.Single(value => value.Source.ScrollGroupId == "teleport-high").Source.CasterId, "Another variant chooses independently.");
            Assertions.Equal("a", rows.Single(value => value.Source.Spell == TeleportSpellKind.WordOfRecall).Source.CasterId, "Recall uses its own native reader eligibility and chance.");
            Assertions.True(rows.Where(value => value.Source.Spell == TeleportSpellKind.Teleport).All(value => value.ScrollVariant > 0), "Only materially distinct choices need compact variant qualifiers.");
            Assertions.Equal(0, rows.Single(value => value.Source.Spell == TeleportSpellKind.WordOfRecall).ScrollVariant, "The sole Recall variant has no redundant qualifier.");
        }
        internal static void UnsupportedReaderComparisonStaysExplicit()
        {
            var known = GroupReader("known", 0, "variant", 15);
            var unknown = new TeleportCastSourceSnapshot("unknown", 1, "unknown", known.BookId, "Scroll", known.Spell,
                known.Kind, known.SpellLevel, known.CasterLevel, known.Uses, known.Facts,
                TeleportScrollActivationChance.Unsupported("unverified active effect"), "variant");
            var row = Compose(Point(), new[] { known, unknown }).SpellActions.Single();
            Assertions.False(row.ReaderResolved, "Unscored activation behavior cannot silently choose a heuristic reader.");
            var guaranteed = GroupReader("guaranteed", 2, "variant", 100);
            var resolved = Compose(Point(), new[] { known, unknown, guaranteed }).SpellActions.Single();
            Assertions.True(resolved.ReaderResolved && ReferenceEquals(guaranteed, resolved.Source), "A proven no-check success has the maximum possible activation chance.");
            Assertions.Equal(row.Key, resolved.Key, "Resolving current effects does not replace the UI identity.");
        }
    }

}
