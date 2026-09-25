using System;
using System.Collections.Generic;
using System.Linq;
using KingmakerGunslinger.FavoredClass;

namespace KingmakerGunslinger.DomainTests
{
    /// <summary>
    /// Review finding 2 (charter 8.10): an owned selected power's own effect
    /// thresholds follow the effective level.
    /// </summary>
    internal static class FavoredClassThresholdTests
    {
        // The native AddFeatureOnClassLevel.IsFeatureShouldBeApplied branches,
        // reproduced verbatim so the policy is checked against them.
        private static bool Native(int num, int level, bool before)
        {
            if (before && num >= level)
                return false;
            if (num >= level || !before)
            {
                if (num >= level)
                    return !before;
                return false;
            }
            return true;
        }

        internal static void GateDecisionMatchesTheNativeComponent()
        {
            foreach (bool before in new[] { false, true })
                for (int gate = 1; gate <= 20; gate++)
                    for (int level = 0; level <= 23; level++)
                        Assertions.Equal(Native(level, gate, before),
                            FavoredClassMechanicsPolicy.GateApplies(level, gate, before),
                            "gate " + gate + (before ? " before" : "") + " at " + level);
            // Touch of Flame's flaming weapon (11): an Oracle 10 with one step.
            Assertions.False(FavoredClassMechanicsPolicy.GateApplies(10, 11, false), "Native at 10: no weapon.");
            Assertions.True(FavoredClassMechanicsPolicy.GateApplies(10 + 1, 11, false), "One step: the weapon.");
            // Form of the Beast's pair (before 11 / at 11) swaps at the effective level.
            Assertions.True(FavoredClassMechanicsPolicy.GateApplies(10, 11, true) &&
                !FavoredClassMechanicsPolicy.GateApplies(11, 11, true), "The before-11 form leaves at 11.");
            // Breath Weapon's 20th-level form at an effective 20 (Oracle 18 with two steps).
            Assertions.True(FavoredClassMechanicsPolicy.GateApplies(18 + 2, 20, false), "The 20th-level form.");
        }

        internal static void PowerUseThresholdsFollowTheEffectiveLevel()
        {
            var blast = new[] { new KeyValuePair<int, int>(17, 1), new KeyValuePair<int, int>(20, 1) };
            Func<int, int, int> uses = (real, steps) =>
                FavoredClassMechanicsPolicy.ThresholdUsesBetween(blast, real, real + steps);
            Assertions.Equal(0, uses(15, 0), "No steps, no uses.");
            Assertions.Equal(0, uses(15, 1), "16 reaches no threshold.");
            Assertions.Equal(1, uses(15, 2), "17: the second use early.");
            Assertions.Equal(1, uses(16, 1), "One step at 16 reaches 17.");
            Assertions.Equal(0, uses(17, 2), "The 17th-level use is already native at 17.");
            Assertions.Equal(1, uses(18, 2), "20: the third use early.");
            Assertions.Equal(0, uses(20, 2), "Nothing beyond the power's own thresholds.");
            Assertions.Equal(2, FavoredClassMechanicsPolicy.ThresholdUsesBetween(blast, 16, 20),
                "Both thresholds between 16 and 20.");
            Assertions.Equal(0, FavoredClassMechanicsPolicy.ThresholdUsesBetween(blast, 18, 18), "No change.");
            Assertions.Equal(0, FavoredClassMechanicsPolicy.ThresholdUsesBetween(null, 1, 20), "No thresholds.");
            var weighted = new[] { new KeyValuePair<int, int>(10, 2) };
            Assertions.Equal(2, FavoredClassMechanicsPolicy.ThresholdUsesBetween(weighted, 9, 10),
                "A threshold's own amount is added.");
        }

        // Call of the Wild moves the Blast's extra uses into the power's own
        // class-level gates (17 and 20): the same arithmetic as the native
        // level-entry layout, decided like the native gate.
        internal static void PowerUseGatesFollowTheEffectiveLevel()
        {
            Func<int, int, int> uses = (real, steps) =>
                FavoredClassMechanicsPolicy.GateUsesDelta(real, steps, 17, false, 1) +
                FavoredClassMechanicsPolicy.GateUsesDelta(real, steps, 20, false, 1);
            Assertions.Equal(0, uses(15, 0), "No steps, no uses.");
            Assertions.Equal(0, uses(15, 1), "16 reaches no gate.");
            Assertions.Equal(1, uses(15, 2), "17: the second use early.");
            Assertions.Equal(1, uses(16, 1), "One step at 16 reaches 17.");
            Assertions.Equal(0, uses(17, 2), "The 17th-level use is already native at 17.");
            Assertions.Equal(1, uses(18, 2), "20: the third use early.");
            Assertions.Equal(0, uses(20, 2), "Nothing beyond the power's own gates.");
            Assertions.Equal(-1, FavoredClassMechanicsPolicy.GateUsesDelta(9, 2, 10, true, 1),
                "A before-gate is lost at the effective level exactly as natively.");
            Assertions.Equal(0, FavoredClassMechanicsPolicy.GateUsesDelta(12, 2, 10, true, 1),
                "A before-gate already passed natively changes nothing.");
        }
    }
}
