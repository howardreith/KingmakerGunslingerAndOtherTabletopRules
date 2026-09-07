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
            state.MigrateLegacy(new[] { Other });
            Assertions.Equal(before, state.Serialize(), "Migration is idempotent.");
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
    }
}
