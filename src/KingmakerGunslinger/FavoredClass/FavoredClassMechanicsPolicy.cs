using System;

namespace KingmakerGunslinger.FavoredClass
{
    /// <summary>
    /// The amount fields of a native ability resource (its private m_MaxAmount
    /// with m_UseMax/m_Max), and whether each class-level sum counts the
    /// oracle class.
    /// </summary>
    internal sealed class FavoredClassResourceAmount
    {
        internal int BaseValue;
        internal bool IncreasedByLevel;
        internal int LevelIncrease;
        internal bool IncreasedByStat;
        internal bool IncreasedByLevelStartPlusDivStep;
        internal int StartingLevel;
        internal int StartingIncrease;
        internal int LevelStep = 1;
        internal int PerStepIncrease;
        internal int MinClassLevelIncrease;
        internal float OtherClassesModifier;
        internal bool UseMax;
        internal int Max;

        /// <summary>The per-level classes include the oracle class.</summary>
        internal bool LevelScalesWithOracle;

        /// <summary>The start-plus-step classes include the oracle class.</summary>
        internal bool DivScalesWithOracle;

        /// <summary>
        /// Whether the maximum depends on oracle level at all (per level, per
        /// step, or through a single start-level threshold): the charter
        /// traces every level-dependent value of an owned power, including
        /// its thresholds. Resources that mix in other classes' character
        /// levels are never scaled.
        /// </summary>
        internal bool ScalesWithOracle
        {
            get
            {
                bool perLevel = IncreasedByLevel && LevelScalesWithOracle && LevelIncrease > 0;
                bool perStep = IncreasedByLevelStartPlusDivStep && DivScalesWithOracle && LevelStep > 0 &&
                    OtherClassesModifier == 0f &&
                    (PerStepIncrease > 0 || StartingIncrease > 0 || MinClassLevelIncrease > 0);
                return perLevel || perStep;
            }
        }
    }

    /// <summary>
    /// Pure arithmetic of Gunslinger favored-class mechanics that combine
    /// with an existing rule rather than stand alone.
    /// </summary>
    internal static class FavoredClassMechanicsPolicy
    {
        /// <summary>
        /// I08/S06: the change of a DC bound to half the class level when the
        /// effective level rises by the earned steps (floor((L+e)/2) - floor(L/2)).
        /// </summary>
        internal static int HalfLevelDelta(int level, int earned)
        {
            if (earned <= 0)
                return 0;
            int baseLevel = Math.Max(0, level);
            return (baseLevel + earned) / 2 - baseLevel / 2;
        }

        /// <summary>
        /// I06/S04: the native maximum of an ability resource before handler
        /// bonuses (BlueprintAbilityResource.GetMaxAmount), with the unit's
        /// counted class-level sums raised by <paramref name="effective"/>
        /// wherever the amount depends on oracle level, including a single
        /// start-level threshold of the owned revelation's own resource.
        /// Mirrors the native integer arithmetic exactly.
        /// </summary>
        internal static int ResourceMaximum(FavoredClassResourceAmount amount, int levelSum, int divSum,
            int characterLevel, int statBonus, int effective)
        {
            if (amount == null)
                throw new ArgumentNullException("amount");
            int shift = Math.Max(0, effective);
            int result = amount.BaseValue;
            if (amount.IncreasedByLevel)
                result += amount.LevelIncrease * (levelSum + (amount.LevelScalesWithOracle ? shift : 0));
            if (amount.IncreasedByStat)
                result += statBonus;
            if (amount.IncreasedByLevelStartPlusDivStep)
            {
                bool steps = amount.DivScalesWithOracle && amount.OtherClassesModifier == 0f;
                int counted = divSum + (steps ? shift : 0) +
                    (int)((float)(characterLevel - divSum) * amount.OtherClassesModifier);
                if (amount.StartingLevel <= counted)
                    result += Math.Max(amount.StartingIncrease +
                        amount.PerStepIncrease * (counted - amount.StartingLevel) / amount.LevelStep,
                        amount.MinClassLevelIncrease);
            }
            return amount.UseMax ? Math.Min(result, amount.Max) : result;
        }

        /// <summary>
        /// I06/S04 Family B: the handler bonus that makes a resource's maximum
        /// equal its native value at the effective oracle level. Handler
        /// bonuses are added after the native clamp, so both sides are clamped.
        /// </summary>
        internal static int ResourceDelta(FavoredClassResourceAmount amount, int levelSum, int divSum,
            int characterLevel, int statBonus, int effective)
        {
            if (effective <= 0)
                return 0;
            return ResourceMaximum(amount, levelSum, divSum, characterLevel, statBonus, effective) -
                ResourceMaximum(amount, levelSum, divSum, characterLevel, statBonus, 0);
        }

        /// <summary>O01: one investment widens the chosen performance by five feet.</summary>
        internal const int PerformanceFeetPerStep = 5;

        /// <summary>Kingmaker's feet-to-meters ratio (Kingmaker.Utility.Feet).</summary>
        internal const float FeetToMeters = 0.3048f;

        /// <summary>
        /// O01: the per-instance radius of a performance area whose bard has
        /// earned <paramref name="steps"/> steps (base radius plus 5 feet each).
        /// </summary>
        internal static float PerformanceRadiusMeters(float baseMeters, int steps)
        {
            return baseMeters + Math.Max(0, steps) * PerformanceFeetPerStep * FeetToMeters;
        }

        /// <summary>
        /// G02/G08/G16: the earned firearm confirmation bonus does not stack
        /// with Critical Focus, and the better contribution is preserved.
        /// Critical Focus applies its own bonus natively, so this adds only
        /// the excess of the earned bonus over it (never a negative amount).
        /// </summary>
        internal static int ConfirmationContribution(int earnedSteps, int criticalFocusBonus)
        {
            if (earnedSteps < 0)
                throw new ArgumentOutOfRangeException("earnedSteps");
            return Math.Max(0, earnedSteps - Math.Max(0, criticalFocusBonus));
        }
    }
}
