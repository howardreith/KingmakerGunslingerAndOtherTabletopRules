using KingmakerGunslinger.ElementalRaces;

namespace KingmakerGunslinger.DomainTests
{
    internal static class ElementalBreathPolicyTests
    {
        internal static void DamageBreakpointsAndCapAreExact()
        {
            int[] expected = { 0, 0, 1, 1, 2, 2, 3, 3, 4, 4, 5, 5, 5 };
            for (int level = 0; level < expected.Length; level++)
                Assertions.Equal(expected[level], ElementalBreathPolicy.DamageDice(level),
                    "Breath dice use half total level, rounded down, capped at five.");
            for (int level = 13; level <= 40; level++)
                Assertions.Equal(5, ElementalBreathPolicy.DamageDice(level),
                    "High level must not exceed five damage dice.");
        }

        internal static void DifficultyUsesCurrentConstitutionAndUncappedHalfLevel()
        {
            foreach (int modifier in new[] { -5, -2, 0, 1, 4, 10 })
            {
                Assertions.Equal(10 + modifier, ElementalBreathPolicy.DifficultyClass(1, modifier),
                    "Level-one DC has no invented spell-level contribution.");
                Assertions.Equal(12 + modifier, ElementalBreathPolicy.DifficultyClass(5, modifier),
                    "Odd total levels round down and use the current modifier.");
                Assertions.Equal(20 + modifier, ElementalBreathPolicy.DifficultyClass(20, modifier),
                    "The damage-dice cap does not cap the DC's half-level contribution.");
            }
        }

        internal static void ConstructionDoesNotInventMinimumDamage()
        {
            foreach (int level in new[] { int.MinValue, -1, 0, 1 })
            {
                Assertions.Equal(0, ElementalBreathPolicy.DamageDice(level),
                    "A zero-dice state must not invent a minimum die.");
                Assertions.Equal(0, ElementalBreathPolicy.HalfLevel(level),
                    "Invalid construction levels fail closed without negative scaling.");
            }
            Assertions.Equal(5, ElementalBreathPolicy.ConeFeet, "Both printed cones are five feet.");
            Assertions.Equal(3, ElementalBreathPolicy.SickenedRounds,
                "Ooze sickened duration is fixed, independent of damage dice and level.");
        }
    }
}
