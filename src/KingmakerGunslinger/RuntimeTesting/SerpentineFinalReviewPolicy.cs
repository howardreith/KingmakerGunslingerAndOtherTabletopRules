using System;
using System.Linq;
using KingmakerGunslinger.Summoning;

namespace KingmakerGunslinger.RuntimeTesting
{
    // Fixed request-local evidence surface; no production gameplay consumers.
    internal static class SerpentineFinalReviewPolicy
    {
        internal static bool ValidExit(string scenario, bool exitAfterCompletion)
        {
            return scenario != RuntimeTestScenarioCatalog.DisposableExpandedSummoningSnakeFinalReview ||
                exitAfterCompletion;
        }

        internal static SummonVariantSpec[] Routes()
        {
            return ExpandedSummoningCatalog.GenerateVariants(SummonFamily.Monster)
                .Concat(ExpandedSummoningCatalog.GenerateVariants(SummonFamily.NaturesAlly))
                .Where(value => value.Creature.Key == "viper" ||
                    value.Creature.Key == "constrictor-snake").ToArray();
        }

        internal static bool Quantity(SummonMultiplicity kind, int count)
        {
            switch (kind)
            {
                case SummonMultiplicity.One: return count == 1;
                case SummonMultiplicity.OneD3: return count >= 1 && count <= 3;
                case SummonMultiplicity.OneD4PlusOne: return count >= 2 && count <= 5;
                default: return false;
            }
        }

        internal static bool PlayedNativeClip(bool exactNativeAction, bool started,
            string clipName, float duration, double time, float weight)
        {
            return exactNativeAction && started && !string.IsNullOrEmpty(clipName) &&
                !float.IsNaN(duration) && !float.IsInfinity(duration) && duration > 0 &&
                !double.IsNaN(time) && !double.IsInfinity(time) && time > 0 &&
                !float.IsNaN(weight) && !float.IsInfinity(weight) && weight > 0 && weight <= 1.001f;
        }
    }
}
