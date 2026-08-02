using System;
using System.Linq;
using KingmakerGunslinger.Deeds;

namespace KingmakerGunslinger.DomainTests
{
    internal static partial class Program
    {
        private static void TrueGritCatalogExact()
        {
            TrueGritChoice[] values = TrueGritCatalog.Choices;
            Assertions.Equal(19, values.Length, "True Grit catalog count changed.");
            Assertions.True(values.Any(value =>
                    value.Deed == TrueGritDeed.TargetingArms),
                "Targeting Arms is missing from the True Grit catalog.");
            Assertions.Equal(values.Length, values.Select(value => value.Deed)
                .Distinct().Count(), "True Grit catalog contains duplicates.");
            Assertions.True(!values.Any(value => value.DisplayName.Contains("Luck")),
                "Slinger's Luck entered the True Grit catalog.");
        }

        private static void TrueGritPairUniqueness()
        {
            Assertions.True(TrueGritCatalog.IsValidPair(TrueGritDeed.Deadeye,
                TrueGritDeed.StunningShot), "Distinct choices were rejected.");
            Assertions.True(!TrueGritCatalog.IsValidPair(TrueGritDeed.Deadeye,
                TrueGritDeed.Deadeye), "Duplicate choice was accepted.");
        }

        private static void TrueGritOneCostBoundary()
        {
            TrueGritDecision selected = TrueGrit(1, 1, true, false);
            Assertions.True(selected.Available, "Selected one-cost deed rejected.");
            Assertions.Equal(0, selected.EffectiveCost, "One-cost deed not reduced.");
            Assertions.True(selected.RequiresPositiveGrit,
                "Reduced-to-zero deed lost its positive-grit gate.");
            Assertions.True(!TrueGrit(0, 1, true, false).Available,
                "Reduced-to-zero deed accepted zero grit.");
        }

        private static void TrueGritTwoCostReduction()
        {
            Assertions.Equal(1, TrueGrit(1, 2, true, false).EffectiveCost,
                "Selected two-cost deed did not cost one.");
            Assertions.Equal(2, TrueGrit(2, 2, false, false).EffectiveCost,
                "Unselected two-cost deed changed.");
        }

        private static void TrueGritPositiveGateRemoval()
        {
            Assertions.True(TrueGrit(0, 0, true, true).Available,
                "Selected no-spend deed rejected zero grit.");
            Assertions.True(!TrueGrit(0, 0, false, true).Available,
                "Unselected no-spend deed accepted zero grit.");
        }

        private static void TrueGritVariableAndCheatDeath()
        {
            Assertions.Equal(3, TrueGrit(4, 4, true, false).EffectiveCost,
                "Computed all-remaining cost did not reduce after calculation.");
            TrueGritDecision one = TrueGrit(1, 1, true, false);
            Assertions.Equal(0, one.EffectiveCost,
                "One remaining grit did not reduce to zero.");
            Assertions.True(one.Available && one.RequiresPositiveGrit,
                "Cheat Death one-grit boundary changed.");
        }

        private static void TrueGritUnselectedIsolation()
        {
            TrueGritDecision value = TrueGrit(1, 1, false, false);
            Assertions.True(value.Available, "Unselected ordinary deed rejected.");
            Assertions.Equal(1, value.EffectiveCost,
                "Unselected ordinary deed was reduced.");
        }

        private static void TrueGritInvalidInput()
        {
            var service = new TrueGritService();
            Assertions.Throws<ArgumentNullException>(() => service.Evaluate(null),
                "Null True Grit request accepted.");
            Assertions.Throws<ArgumentOutOfRangeException>(() =>
                new TrueGritRequest(-1, 0, false, false),
                "Negative current grit accepted.");
            Assertions.Throws<ArgumentException>(() =>
                new TrueGritRequest(1, 1, true, true),
                "Positive-gate request with a cost accepted.");
        }

        private static TrueGritDecision TrueGrit(int current, int ordinary,
            bool selected, bool positiveNoSpend)
        { return new TrueGritService().Evaluate(new TrueGritRequest(current,
            ordinary, selected, positiveNoSpend)); }
    }
}
