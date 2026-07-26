using System;

namespace KingmakerGunslinger.Diagnostics
{
    /// <summary>
    /// Pure range-increment arithmetic. Distances are supplied in the same unit by
    /// the caller; this type contains no Kingmaker or Unity object references.
    /// </summary>
    internal static class FirearmRangeMath
    {
        internal static int CalculateIncrement(double distance, double incrementLength)
        {
            if (double.IsNaN(distance) || double.IsInfinity(distance) || distance < 0d)
            {
                throw new ArgumentOutOfRangeException("distance", "Distance must be a finite non-negative value.");
            }

            if (double.IsNaN(incrementLength) ||
                double.IsInfinity(incrementLength) ||
                incrementLength <= 0d)
            {
                throw new ArgumentOutOfRangeException(
                    "incrementLength",
                    "Range increment length must be a finite positive value.");
            }

            if (distance == 0d)
            {
                return 1;
            }

            double raw = Math.Ceiling(distance / incrementLength);
            if (raw > int.MaxValue)
            {
                throw new OverflowException("The calculated range increment exceeds Int32.MaxValue.");
            }

            return Math.Max(1, (int)raw);
        }
    }
}
