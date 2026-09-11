using System;
using KingmakerGunslinger.Spells.Teleportation;

namespace KingmakerGunslinger.DomainTests
{
    internal static class TeleportationPolicyTests
    {
        private const string Point = "11111111111111111111111111111111";
        private const string Other = "22222222222222222222222222222222";
        internal static void ExactD100Tables()
        {
            Check(TeleportFamiliarity.VeryFamiliar, 97, 2, 1, 0);
            Check(TeleportFamiliarity.StudiedCarefully, 94, 3, 2, 1);
            Check(TeleportFamiliarity.SeenCasually, 88, 6, 4, 2);
            Check(TeleportFamiliarity.ViewedOnce, 76, 12, 8, 4);
        }
        private static void Check(TeleportFamiliarity familiarity, int on, int off, int similar, int mishap)
        {
            TeleportOutcomeDecision table = TeleportRollTable.For(familiarity);
            Assertions.Equal(on, table.OnTargetPercent, "Exact displayed on-target odds.");
            Assertions.Equal(off, table.OffTargetPercent, "Exact displayed off-target odds.");
            Assertions.Equal(similar, table.SimilarLocationPercent, "Exact displayed similar odds.");
            Assertions.Equal(mishap, table.MishapPercent, "Exact displayed mishap odds.");
            for (int roll = 1; roll <= 100; roll++)
            {
                TeleportOutcomeKind expected = roll <= on ? TeleportOutcomeKind.OnTarget :
                    roll <= on + off ? TeleportOutcomeKind.OffTarget :
                    roll <= on + off + similar ? TeleportOutcomeKind.SimilarLocation : TeleportOutcomeKind.Mishap;
                Assertions.Equal(expected, TeleportRollTable.Resolve(familiarity, roll),
                    familiarity + " d100=" + roll);
            }
        }
        internal static void InvalidRollsFailClosed()
        {
            foreach (int roll in new[] { int.MinValue, 0, 101, int.MaxValue })
                Assertions.Throws<ArgumentOutOfRangeException>(() =>
                    TeleportRollTable.Resolve(TeleportFamiliarity.ViewedOnce, roll), "Reject invalid die.");
            Assertions.Throws<ArgumentOutOfRangeException>(() =>
                TeleportRollTable.For(TeleportFamiliarity.Unvisited), "Unvisited is no table.");
        }
        internal static void SeverityIsMonotonic()
        {
            foreach (int max in new[] { 76, 88, 94, 97 })
                foreach (int count in new[] { 1, 2, 3, 9, 10, 11, 19, 101 })
                {
                    int previous = -1;
                    for (int roll = max + 1; roll <= 100; roll++)
                    {
                        int bucket = TeleportFailureSeverityPolicy.Bucket(
                            TeleportFailureSeverityPolicy.Calculate(roll, max), count);
                        Assertions.True(bucket >= previous, "Worse rolls never select nearer rank buckets.");
                        previous = bucket;
                    }
                    Assertions.Equal(Math.Min(10, count) - 1, previous, "Worst tail reaches farthest bucket.");
                }
            Assertions.Equal(0, TeleportFailureSeverityPolicy.Bucket(1.0 / 24, 100), "Near failure.");
            Assertions.Equal(9, TeleportFailureSeverityPolicy.Bucket(1, 100), "Far failure.");
        }
        internal static void BucketsPartitionAllRanks()
        {
            for (int count = 1; count <= 120; count++)
            {
                int covered = 0;
                for (int bucket = 0; bucket < Math.Min(10, count); bucket++)
                {
                    int first = TeleportFailureSeverityPolicy.FirstRank(bucket, count);
                    int size = TeleportFailureSeverityPolicy.RankCount(bucket, count);
                    Assertions.Equal(covered, first, "Contiguous rank partition.");
                    Assertions.True(size > 0, "No empty selectable bucket.");
                    covered += size;
                }
                Assertions.Equal(count, covered, "Every candidate has exactly one bucket.");
            }
            Assertions.Throws<ArgumentOutOfRangeException>(() =>
                TeleportFailureSeverityPolicy.Bucket(double.NaN, 5), "No ambiguous severity.");
            Assertions.Throws<ArgumentOutOfRangeException>(() =>
                TeleportFailureSeverityPolicy.Bucket(0.5, 0), "No false alternate when none exist.");
        }
        internal static void FamiliarityUsesOnlyRecordedArrivals()
        {
            var state = new TeleportFamiliarityState();
            var expected = new[] { TeleportFamiliarity.Unvisited, TeleportFamiliarity.ViewedOnce,
                TeleportFamiliarity.SeenCasually, TeleportFamiliarity.StudiedCarefully,
                TeleportFamiliarity.StudiedCarefully, TeleportFamiliarity.VeryFamiliar,
                TeleportFamiliarity.VeryFamiliar };
            for (int count = 0; count < expected.Length; count++)
            {
                Assertions.Equal(expected[count], state.Familiarity(Point, false, false), "Arrival category.");
                Assertions.Equal(count, state.Count(Point), "Reading familiarity cannot increment it.");
                if (count + 1 < expected.Length) state.RecordOrdinaryArrival(Point);
            }
            Assertions.Equal(0, state.Count(Other), "Unrecorded point remains unvisited.");
            Assertions.Equal(TeleportFamiliarity.VeryFamiliar, state.Familiarity(Other, true, false), "Oleg override.");
            Assertions.Equal(TeleportFamiliarity.VeryFamiliar, state.Familiarity(Other, false, true), "Capital override.");
        }
        internal static void MigrationIsIdempotentAndRoundTrips()
        {
            var state = new TeleportFamiliarityState();
            state.RecordOrdinaryArrival(Point);
            state.RecordOrdinaryArrival(Point);
            state.MigrateLegacy(new[] { Point, Other, Other });
            Assertions.Equal(2, state.Count(Point), "Migration preserves known arrival count.");
            Assertions.Equal(1, state.Count(Other), "Legacy native visit seeds once.");
            string before = state.Serialize();
            const string newlyUnlocked = "33333333333333333333333333333333";
            state.MigrateLegacy(new[] { Other, newlyUnlocked });
            Assertions.Equal(before, state.Serialize(), "Migration is idempotent.");
            Assertions.Equal(0, state.Count(newlyUnlocked),
                "Later reveal/open/explore mutations never become legacy arrivals, including a module-off visit.");
            TeleportFamiliarityState loaded = TeleportFamiliarityState.Parse(before);
            Assertions.Equal(before, loaded.Serialize(), "Exact deterministic serialization round trip.");
            Assertions.True(loaded.LegacyMigrationComplete, "Migration ownership survives save/load.");
            Assertions.Equal(1, loaded.Count(Other), "Retained counts need no currently active map object.");
        }
        internal static void CorruptLedgerFailsClosed()
        {
            foreach (string value in new[] { "", "2|1", "1|2", "1|1|" + Point + ":0",
                "1|1|" + Point + ":-1", "1|1|" + Point + ":1|" + Point + ":2" })
                Assertions.Throws<FormatException>(() => TeleportFamiliarityState.Parse(value), "Malformed ledger.");
            var state = new TeleportFamiliarityState();
            Assertions.Throws<ArgumentException>(() => state.RecordOrdinaryArrival("Ancient Tomb"),
                "Display names never identify persisted points.");
            Assertions.Throws<ArgumentException>(() => state.MigrateLegacy(new[] { Point, "bad" }),
                "Invalid migration is atomic.");
            Assertions.Equal(0, state.Count(Point), "Failed migration left no partial count.");
            var saturated = TeleportFamiliarityState.Parse("1|1|" + Point + ":2147483647");
            saturated.RecordOrdinaryArrival(Point);
            Assertions.Equal(int.MaxValue, saturated.Count(Point), "No integer wraparound.");
        }
        internal static void SpecialistCacheRestoresOnlyTheNativeInvariant()
        {
            Assertions.True(TeleportSpecialistSpellCachePolicy.ShouldRestoreSpecialMembership(
                false, true, true, false), "Known spell in an attached special list with a stale cache is restored.");
            Assertions.False(TeleportSpecialistSpellCachePolicy.ShouldRestoreSpecialMembership(
                false, false, true, false), "A spell the book does not know is never auto-learned.");
            Assertions.False(TeleportSpecialistSpellCachePolicy.ShouldRestoreSpecialMembership(
                false, true, false, false), "A spell outside the book's attached school lists is untouched.");
            Assertions.False(TeleportSpecialistSpellCachePolicy.ShouldRestoreSpecialMembership(
                false, true, true, true), "Already-special membership is idempotent.");
            Assertions.False(TeleportSpecialistSpellCachePolicy.ShouldRestoreSpecialMembership(
                true, true, true, false), "AllSpellsKnown books self-heal natively and are left alone.");
        }
        internal static void BeginPolicySeparatesModalBlockingFromPresenterAvailability()
        {
            // Case 1: an unrelated active modal blocks every action.
            Assertions.False(TeleportBeginPolicy.CanExecuteAction(false, true, true, TeleportSpellKind.GreaterTeleport),
                "an unrelated modal blocks the direct cast");
            Assertions.False(TeleportBeginPolicy.CanExecuteAction(false, true, true, TeleportSpellKind.Teleport),
                "an unrelated modal blocks confirmed spells");
            Assertions.False(TeleportBeginPolicy.OffersAnyAction(false, true, true,
                new[] { TeleportSpellKind.GreaterTeleport, TeleportSpellKind.Teleport }),
                "an unrelated modal removes the whole offer");
            // Case 2: no modal, no confirmation presenter: Greater Teleport stays
            // usable; confirmed spells do not bypass their confirmation.
            Assertions.True(TeleportBeginPolicy.CanExecuteAction(false, false, false, TeleportSpellKind.GreaterTeleport),
                "Greater Teleport never needs the presenter");
            Assertions.False(TeleportBeginPolicy.CanExecuteAction(false, false, false, TeleportSpellKind.Teleport),
                "ordinary Teleport cannot bypass its confirmation");
            Assertions.False(TeleportBeginPolicy.CanExecuteAction(false, false, false, TeleportSpellKind.WordOfRecall),
                "Word of Recall cannot bypass its confirmation");
            // Case 3: a mixed list with no presenter still offers Greater Teleport.
            Assertions.True(TeleportBeginPolicy.OffersAnyAction(false, false, false,
                new[] { TeleportSpellKind.Teleport, TeleportSpellKind.GreaterTeleport }),
                "a mixed list keeps its Greater Teleport action when the presenter is unavailable");
            Assertions.False(TeleportBeginPolicy.OffersAnyAction(false, false, false,
                new[] { TeleportSpellKind.Teleport, TeleportSpellKind.WordOfRecall }),
                "a list of only confirmed spells composes nothing without the presenter");
            // Presenter available and idle: everything is offered.
            Assertions.True(TeleportBeginPolicy.OffersAnyAction(false, false, true,
                new[] { TeleportSpellKind.Teleport, TeleportSpellKind.GreaterTeleport, TeleportSpellKind.WordOfRecall }),
                "an idle presenter offers every spell");
            // An in-flight cast suspends every offer.
            Assertions.False(TeleportBeginPolicy.OffersAnyAction(true, false, true,
                new[] { TeleportSpellKind.GreaterTeleport }),
                "the single in-flight guard suspends new offers");
            Assertions.False(TeleportBeginPolicy.OffersAnyAction(false, false, true, new TeleportSpellKind[0]),
                "an empty offer composes nothing");
        }
        internal static void ViewportHeightIncludesSeparatorContent()
        {
            Assertions.Equal(72f, TeleportContextLayoutPolicy.ViewportHeight(72f, 300f),
                "Two rows plus a separator are fully shown when room exists.");
            Assertions.Equal(300f, TeleportContextLayoutPolicy.ViewportHeight(480f, 300f),
                "A genuinely long list still clamps to the measured maximum.");
            Assertions.Equal(7f, TeleportContextLayoutPolicy.ViewportHeight(7f, 7f), "Exact fit is accepted.");
            foreach (float value in new[] { 0f, -1f, float.NaN, float.PositiveInfinity })
            {
                Assertions.Throws<InvalidOperationException>(() => TeleportContextLayoutPolicy.ViewportHeight(value, 300f),
                    "Unproven content height fails closed.");
                Assertions.Throws<InvalidOperationException>(() => TeleportContextLayoutPolicy.ViewportHeight(72f, value),
                    "Unproven maximum height fails closed.");
            }
        }
    }
}
