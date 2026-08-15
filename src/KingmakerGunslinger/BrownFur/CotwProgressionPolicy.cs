using System;
using System.Collections.Generic;
using System.Linq;

namespace KingmakerGunslinger.BrownFur
{
    internal static class CotwProgressionPolicy
    {
        private static readonly int[] NormalLevels =
            { 1, 3, 5, 7, 9, 11, 13, 15, 17, 19 };
        private static readonly int[] BalanceFixesLevels =
            { 1, 4, 7, 10, 13, 16, 19 };

        internal static CotwProgressionDecision Resolve(
            IEnumerable<int> resolvedExploitBearingLevels)
        {
            if (resolvedExploitBearingLevels == null)
                return CotwProgressionDecision.Reject(
                    "exploit-bearing levels were not resolved");

            int[] levels = resolvedExploitBearingLevels.ToArray();
            if (levels.Length == 0)
                return CotwProgressionDecision.Reject(
                    "no exploit-bearing levels were resolved");
            if (levels.Any(value => value < 1 || value > 20))
                return CotwProgressionDecision.Reject(
                    "an exploit-bearing level is outside 1..20");
            if (levels.Distinct().Count() != levels.Length)
                return CotwProgressionDecision.Reject(
                    "duplicate exploit-bearing levels were resolved");
            if (!levels.SequenceEqual(levels.OrderBy(value => value)))
                return CotwProgressionDecision.Reject(
                    "exploit-bearing levels are not in deterministic ascending order");

            if (levels.SequenceEqual(NormalLevels))
                return CotwProgressionDecision.Accept(
                    CotwProgressionShape.Normal, 3, 9);
            if (levels.SequenceEqual(BalanceFixesLevels))
                return CotwProgressionDecision.Accept(
                    CotwProgressionShape.BalanceFixes, 4, 10);

            return CotwProgressionDecision.Reject(
                "the resolved exploit schedule is not a supported CotW progression shape");
        }
    }
}
