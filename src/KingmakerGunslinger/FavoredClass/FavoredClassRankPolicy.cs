using System;

namespace KingmakerGunslinger.FavoredClass
{
    /// <summary>
    /// A favored-class reward rate: one whole benefit step per
    /// <see cref="Divisor"/> investments, optionally limited by a printed cap
    /// expressed in benefit steps. A divisor of one means every investment
    /// yields a whole step and no partial feature exists.
    /// </summary>
    internal sealed class FavoredClassRate
    {
        internal FavoredClassRate(int divisor, int? capSteps)
        {
            if (divisor < 1)
                throw new ArgumentOutOfRangeException("divisor", divisor,
                    "A favored-class divisor must be at least one.");
            if (capSteps.HasValue && capSteps.Value < 1)
                throw new ArgumentOutOfRangeException("capSteps", capSteps,
                    "A printed cap must allow at least one benefit step.");
            Divisor = divisor;
            CapSteps = capSteps;
        }

        internal int Divisor { get; private set; }

        /// <summary>Printed cap in benefit steps; null when uncapped.</summary>
        internal int? CapSteps { get; private set; }

        internal bool HasPartial { get { return Divisor > 1; } }

        public override string ToString()
        {
            return CapSteps.HasValue
                ? string.Format("1/{0} cap {1}", Divisor, CapSteps.Value)
                : string.Format("1/{0} uncapped", Divisor);
        }
    }

    internal enum FavoredClassInvestmentRole
    {
        Full,
        Partial
    }

    /// <summary>
    /// Pure integer arithmetic for one favored-class target counter.
    ///
    /// The installed host (Favored Class 1.3.1 with Call of the Wild's
    /// PrerequisiteFeatureFullRank) keeps separate full and partial facts. A
    /// pick is a full pick exactly when the investment it creates is a
    /// multiple of the divisor, so after N investments the full rank is
    /// floor(N/d), the partial rank is N - floor(N/d), and N is
    /// fullRank + partialRank (never d * fullRank + partialRank).
    ///
    /// KMG sizes its own leaves from an allowed investment ceiling T instead
    /// of copying the host's partial capacity of Ranks * (d - 1), which would
    /// stop an uncapped divisor-six reward at eighteen investments. For an
    /// uncapped reward T is the twenty-level class progression; for a printed
    /// cap C it is min(20, d * C), so the target closes as soon as its cap
    /// is reached.
    /// </summary>
    internal static class FavoredClassRankPolicy
    {
        /// <summary>Favored-class levels available in one base class.</summary>
        internal const int ProgressionInvestmentLimit = 20;

        internal static int InvestmentCeiling(FavoredClassRate rate)
        {
            if (rate == null)
                throw new ArgumentNullException("rate");
            if (!rate.CapSteps.HasValue)
                return ProgressionInvestmentLimit;
            long capped = (long)rate.Divisor * rate.CapSteps.Value;
            return capped < ProgressionInvestmentLimit
                ? (int)capped
                : ProgressionInvestmentLimit;
        }

        internal static int FullCapacity(FavoredClassRate rate)
        {
            return InvestmentCeiling(rate) / rate.Divisor;
        }

        internal static int PartialCapacity(FavoredClassRate rate)
        {
            return InvestmentCeiling(rate) - FullCapacity(rate);
        }

        internal static int FullRankAfter(int investments, int divisor)
        {
            RequireInvestments(investments);
            RequireDivisor(divisor);
            return investments / divisor;
        }

        internal static int PartialRankAfter(int investments, int divisor)
        {
            return investments - FullRankAfter(investments, divisor);
        }

        internal static int Investments(int fullRank, int partialRank)
        {
            if (fullRank < 0)
                throw new ArgumentOutOfRangeException("fullRank");
            if (partialRank < 0)
                throw new ArgumentOutOfRangeException("partialRank");
            return fullRank + partialRank;
        }

        /// <summary>
        /// Whether the next investment after <paramref name="investments"/>
        /// completes a whole step. Mirrors the host's
        /// (partial + full + 1) % d == 0 alternation.
        /// </summary>
        internal static bool NextInvestmentIsFull(int investments, int divisor)
        {
            RequireInvestments(investments);
            RequireDivisor(divisor);
            return (investments + 1) % divisor == 0;
        }

        /// <summary>
        /// Whether a leaf with the given role may be chosen next. Exactly one
        /// role is open for any investment count below the ceiling, and none
        /// is open once the ceiling (or printed cap) is reached.
        /// </summary>
        internal static bool CanInvest(FavoredClassRate rate, int fullRank,
            int partialRank, FavoredClassInvestmentRole role)
        {
            if (rate == null)
                throw new ArgumentNullException("rate");
            if (role == FavoredClassInvestmentRole.Partial && !rate.HasPartial)
                return false;
            int investments = Investments(fullRank, partialRank);
            if (investments >= InvestmentCeiling(rate))
                return false;
            if (fullRank > FullRankAfter(investments, rate.Divisor) ||
                partialRank > PartialRankAfter(investments, rate.Divisor))
                return false;
            bool full = NextInvestmentIsFull(investments, rate.Divisor);
            return role == FavoredClassInvestmentRole.Full ? full : !full;
        }

        /// <summary>
        /// Whole benefit steps earned. The full rank already equals
        /// floor(N/d); the printed cap is applied defensively so an
        /// out-of-contract rank (for example from an edited save) can never
        /// exceed the printed maximum.
        /// </summary>
        internal static int BenefitSteps(FavoredClassRate rate, int fullRank)
        {
            if (rate == null)
                throw new ArgumentNullException("rate");
            if (fullRank <= 0)
                return 0;
            int bounded = Math.Min(fullRank, FullCapacity(rate));
            return rate.CapSteps.HasValue
                ? Math.Min(bounded, rate.CapSteps.Value)
                : bounded;
        }

        /// <summary>
        /// Investment toward the next whole step, for disclosure text such
        /// as "3/4 toward the next grit point".
        /// </summary>
        internal static int ProgressTowardNextStep(int investments, int divisor)
        {
            RequireInvestments(investments);
            RequireDivisor(divisor);
            return investments % divisor;
        }

        /// <summary>
        /// A mixed-rate bundle component whose benefit advances at its own
        /// divisor from the shared investment count, such as Undine Monk's
        /// grapple defense (every investment) and extra stunning attacks
        /// (every third investment).
        /// </summary>
        internal static int MixedComponentSteps(int investments,
            int componentDivisor, int? componentCapSteps)
        {
            RequireInvestments(investments);
            RequireDivisor(componentDivisor);
            int steps = investments / componentDivisor;
            return componentCapSteps.HasValue
                ? Math.Min(steps, componentCapSteps.Value)
                : steps;
        }

        private static void RequireInvestments(int investments)
        {
            if (investments < 0)
                throw new ArgumentOutOfRangeException("investments", investments,
                    "Investment counts cannot be negative.");
        }

        private static void RequireDivisor(int divisor)
        {
            if (divisor < 1)
                throw new ArgumentOutOfRangeException("divisor", divisor,
                    "A favored-class divisor must be at least one.");
        }
    }
}
