using System;
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
}
