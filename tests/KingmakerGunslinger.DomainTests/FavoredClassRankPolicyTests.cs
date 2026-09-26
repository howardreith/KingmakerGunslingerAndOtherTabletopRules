using System;
using KingmakerGunslinger.FavoredClass;

namespace KingmakerGunslinger.DomainTests
{
    internal static class FavoredClassRankPolicyTests
    {
        // Charter section 7.1: divisor four at N = 1, 3, 4, 5, 8.
        internal static void DivisorFourDecompositionMatchesHostFacts()
        {
            int[] investments = { 1, 3, 4, 5, 8 };
            int[] full = { 0, 0, 1, 1, 2 };
            int[] partial = { 1, 3, 3, 4, 6 };
            for (int i = 0; i < investments.Length; i++)
            {
                Assertions.Equal(full[i],
                    FavoredClassRankPolicy.FullRankAfter(investments[i], 4),
                    "Full rank after " + investments[i] + " investments.");
                Assertions.Equal(partial[i],
                    FavoredClassRankPolicy.PartialRankAfter(investments[i], 4),
                    "Partial rank after " + investments[i] + " investments.");
                Assertions.Equal(investments[i], FavoredClassRankPolicy.Investments(
                    full[i], partial[i]), "N must be fullRank + partialRank.");
            }
        }

        // Charter section 7.5 golden examples, walked through the same
        // alternation the leaves' prerequisites enforce.
        internal static void GoldenExamplesMatchTheCharter()
        {
            AssertWalk(new FavoredClassRate(4, null), new[] { 3, 4, 8, 20 },
                new[] { 0, 1, 2, 5 }, 20, "grit 1/4");
            AssertWalk(new FavoredClassRate(3, 5), new[] { 2, 3, 15 },
                new[] { 0, 1, 5 }, 15, "firearm confirmation 1/3 cap +5");
            AssertWalk(new FavoredClassRate(4, 2), new[] { 8 },
                new[] { 2 }, 8, "halfling Nimble 1/4 cap +2");
            AssertWalk(new FavoredClassRate(2, null), new[] { 1, 2, 20 },
                new[] { 0, 1, 10 }, 20, "ifrit initiative 1/2");
            AssertWalk(new FavoredClassRate(6, 2), new[] { 5, 6, 12 },
                new[] { 0, 1, 2 }, 12, "sorcerer selected power 1/6 cap +2");
            AssertWalk(new FavoredClassRate(6, null), new[] { 18, 19, 20 },
                new[] { 3, 3, 3 }, 20, "oracle selected revelation 1/6");
            AssertWalk(new FavoredClassRate(1, 6), new[] { 6 },
                new[] { 6 }, 6, "oread performance +5 ft cap +30 ft");

            FavoredClassRate oracle = new FavoredClassRate(6, null);
            Assertions.Equal(17, FavoredClassRankPolicy.PartialRankAfter(20, 6),
                "The final two oracle investments must remain partial.");
            Assertions.Equal(17, FavoredClassRankPolicy.PartialCapacity(oracle),
                "Uncapped divisor-six partial capacity must allow twenty investments.");
        }

        // Charter section 7.2: Undine Monk grapple CMD N, stunning floor(N/3).
        internal static void UndineMonkMixedRateBundle()
        {
            int[] investments = { 1, 2, 3, 20 };
            int[] cmd = { 1, 2, 3, 20 };
            int[] stunning = { 0, 0, 1, 6 };
            for (int i = 0; i < investments.Length; i++)
            {
                Assertions.Equal(cmd[i], FavoredClassRankPolicy.MixedComponentSteps(
                    investments[i], 1, null), "Grapple CMD at N=" + investments[i]);
                Assertions.Equal(stunning[i], FavoredClassRankPolicy.MixedComponentSteps(
                    investments[i], 3, null), "Stunning uses at N=" + investments[i]);
                int full = FavoredClassRankPolicy.FullRankAfter(investments[i], 3);
                int partial = FavoredClassRankPolicy.PartialRankAfter(investments[i], 3);
                Assertions.Equal(cmd[i], full + partial,
                    "Both full and partial picks must advance the CMD portion.");
                Assertions.Equal(stunning[i], full,
                    "Only full picks may advance stunning uses.");
            }
        }

        // M01/M02: every divisor and cap at N = 0..20, alternation, ceilings.
        internal static void EveryRateAlternatesAndClosesAtItsCeiling()
        {
            int[] divisors = { 1, 2, 3, 4, 6 };
            int?[] caps = { null, 1, 2, 4, 5, 6, 10, 20 };
            foreach (int divisor in divisors)
            {
                foreach (int? cap in caps)
                {
                    FavoredClassRate rate = new FavoredClassRate(divisor, cap);
                    int ceiling = FavoredClassRankPolicy.InvestmentCeiling(rate);
                    int expectedCeiling = cap.HasValue
                        ? Math.Min(20, divisor * cap.Value) : 20;
                    Assertions.Equal(expectedCeiling, ceiling, "Ceiling for " + rate);
                    Assertions.Equal(ceiling, FavoredClassRankPolicy.FullCapacity(rate) +
                        FavoredClassRankPolicy.PartialCapacity(rate),
                        "Capacities must add up to the ceiling for " + rate);
                    int full = 0;
                    int partial = 0;
                    for (int n = 0; n <= 20; n++)
                    {
                        bool fullOpen = FavoredClassRankPolicy.CanInvest(rate, full,
                            partial, FavoredClassInvestmentRole.Full);
                        bool partialOpen = FavoredClassRankPolicy.CanInvest(rate, full,
                            partial, FavoredClassInvestmentRole.Partial);
                        if (n >= ceiling)
                        {
                            Assertions.False(fullOpen || partialOpen,
                                "A closed target must offer no leaf: " + rate + " N=" + n);
                            break;
                        }
                        Assertions.True(fullOpen ^ partialOpen,
                            "Exactly one leaf must be open: " + rate + " N=" + n);
                        if (!rate.HasPartial)
                            Assertions.True(fullOpen, "Divisor one has only full leaves.");
                        if (fullOpen)
                            full++;
                        else
                            partial++;
                        Assertions.Equal(FavoredClassRankPolicy.FullRankAfter(n + 1, divisor),
                            full, "Full rank after " + (n + 1) + " for " + rate);
                        Assertions.Equal(FavoredClassRankPolicy.PartialRankAfter(n + 1, divisor),
                            partial, "Partial rank after " + (n + 1) + " for " + rate);
                        Assertions.True(full <= FavoredClassRankPolicy.FullCapacity(rate),
                            "Full capacity exceeded for " + rate);
                        Assertions.True(partial <= FavoredClassRankPolicy.PartialCapacity(rate),
                            "Partial capacity exceeded for " + rate);
                        int steps = FavoredClassRankPolicy.BenefitSteps(rate, full);
                        Assertions.Equal(cap.HasValue ? Math.Min(full, cap.Value) : full,
                            steps, "Benefit for " + rate + " at N=" + (n + 1));
                    }
                    Assertions.Equal(ceiling, full + partial,
                        "The walk must reach the ceiling for " + rate);
                }
            }
        }

        // M02: the host's Ranks*(d-1) sizing would stop divisor six at 18.
        internal static void UncappedDivisorSixDoesNotStopAtEighteen()
        {
            FavoredClassRate rate = new FavoredClassRate(6, null);
            Assertions.Equal(3, FavoredClassRankPolicy.FullCapacity(rate),
                "Full capacity for an uncapped 1/6 reward.");
            Assertions.Equal(17, FavoredClassRankPolicy.PartialCapacity(rate),
                "Partial capacity for an uncapped 1/6 reward.");
            Assertions.True(FavoredClassRankPolicy.CanInvest(rate, 3, 15,
                FavoredClassInvestmentRole.Partial),
                "The nineteenth investment must remain available.");
            Assertions.True(FavoredClassRankPolicy.CanInvest(rate, 3, 16,
                FavoredClassInvestmentRole.Partial),
                "The twentieth investment must remain available.");
            Assertions.False(FavoredClassRankPolicy.CanInvest(rate, 3, 17,
                FavoredClassInvestmentRole.Partial),
                "No twenty-first investment exists.");
        }

        // Out-of-contract ranks (edited saves, foreign tools) fail closed.
        internal static void InconsistentRanksFailClosedAndBenefitStaysCapped()
        {
            FavoredClassRate confirmation = new FavoredClassRate(3, 5);
            Assertions.False(FavoredClassRankPolicy.CanInvest(confirmation, 2, 0,
                FavoredClassInvestmentRole.Full),
                "Two full ranks without partials are not a reachable state.");
            Assertions.False(FavoredClassRankPolicy.CanInvest(confirmation, 2, 0,
                FavoredClassInvestmentRole.Partial),
                "An unreachable state must not accept further investment.");
            Assertions.Equal(5, FavoredClassRankPolicy.BenefitSteps(confirmation, 9),
                "An edited full rank must never exceed the printed cap.");
            Assertions.Equal(0, FavoredClassRankPolicy.BenefitSteps(confirmation, 0),
                "No full rank means no benefit.");
            Assertions.Equal(3, FavoredClassRankPolicy.ProgressTowardNextStep(3, 4),
                "Disclosure of 3/4 toward the next step.");
            Assertions.Equal(0, FavoredClassRankPolicy.ProgressTowardNextStep(4, 4),
                "A completed step shows no pending fraction.");
        }

        internal static void InvalidRatesAndCountsAreRejected()
        {
            AssertThrows(() => new FavoredClassRate(0, null), "divisor zero");
            AssertThrows(() => new FavoredClassRate(-2, null), "negative divisor");
            AssertThrows(() => new FavoredClassRate(3, 0), "zero cap");
            AssertThrows(() => FavoredClassRankPolicy.FullRankAfter(-1, 4),
                "negative investments");
            AssertThrows(() => FavoredClassRankPolicy.Investments(-1, 0),
                "negative full rank");
            AssertThrows(() => FavoredClassRankPolicy.MixedComponentSteps(3, 0, null),
                "zero component divisor");
        }

        private static void AssertWalk(FavoredClassRate rate, int[] checkpoints,
            int[] expectedSteps, int expectedCeiling, string label)
        {
            Assertions.Equal(expectedCeiling, FavoredClassRankPolicy.InvestmentCeiling(rate),
                "Ceiling for " + label);
            int full = 0;
            int partial = 0;
            int checkpoint = 0;
            for (int n = 1; n <= expectedCeiling; n++)
            {
                if (FavoredClassRankPolicy.CanInvest(rate, full, partial,
                    FavoredClassInvestmentRole.Full))
                    full++;
                else if (FavoredClassRankPolicy.CanInvest(rate, full, partial,
                    FavoredClassInvestmentRole.Partial))
                    partial++;
                else
                    throw new InvalidOperationException(
                        "No leaf open before the ceiling for " + label + " at N=" + n);
                while (checkpoint < checkpoints.Length && checkpoints[checkpoint] == n)
                {
                    Assertions.Equal(expectedSteps[checkpoint],
                        FavoredClassRankPolicy.BenefitSteps(rate, full),
                        label + " benefit at N=" + n);
                    checkpoint++;
                }
            }
            Assertions.Equal(checkpoints.Length, checkpoint,
                "Every checkpoint must be reached for " + label);
            Assertions.False(FavoredClassRankPolicy.CanInvest(rate, full, partial,
                FavoredClassInvestmentRole.Full) || FavoredClassRankPolicy.CanInvest(
                rate, full, partial, FavoredClassInvestmentRole.Partial),
                label + " must close at its ceiling.");
        }

        private static void AssertThrows(Action action, string label)
        {
            try
            {
                action();
            }
            catch (ArgumentException)
            {
                return;
            }
            throw new InvalidOperationException("Expected rejection: " + label);
        }
    }
}
