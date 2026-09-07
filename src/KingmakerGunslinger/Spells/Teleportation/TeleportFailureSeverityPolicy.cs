using System;

namespace KingmakerGunslinger.Spells.Teleportation
{
    internal static class TeleportFailureSeverityPolicy
    {
        internal static double Calculate(int d100, int onTargetMax)
        {
            if (d100 < 1 || d100 > 100) throw new ArgumentOutOfRangeException("d100");
            if (onTargetMax < 1 || onTargetMax >= 100)
                throw new ArgumentOutOfRangeException("onTargetMax");
            return Math.Max(0, Math.Min(1, (double)(d100 - onTargetMax) / (100 - onTargetMax)));
        }

        internal static int Bucket(double severity, int candidateCount)
        {
            if (double.IsNaN(severity) || double.IsInfinity(severity))
                throw new ArgumentOutOfRangeException("severity");
            if (candidateCount < 1) throw new ArgumentOutOfRangeException("candidateCount");
            int count = Math.Min(10, candidateCount);
            return Math.Min(count - 1, (int)(Math.Max(0, Math.Min(1, severity)) * count));
        }

        internal static int FirstRank(int bucket, int candidateCount)
        {
            int buckets = Math.Min(10, candidateCount);
            if (candidateCount < 1 || bucket < 0 || bucket >= buckets)
                throw new ArgumentOutOfRangeException("bucket");
            return (int)((long)bucket * candidateCount / buckets);
        }

        internal static int RankCount(int bucket, int candidateCount)
        {
            int first = FirstRank(bucket, candidateCount);
            return (int)((long)(bucket + 1) * candidateCount / Math.Min(10, candidateCount)) - first;
        }
    }
}
