using System;
using System.Collections.Generic;
using KingmakerGunslinger.ElementalRaces;

namespace KingmakerGunslinger.DomainTests
{
    internal static class ElementalNereidPolicyTests
    {
        internal static void SavedResponsesPreserveExposureDecisions()
        {
            var entries = new List<KeyValuePair<string, int>> {
                new KeyValuePair<string, int>("affected", 1), new KeyValuePair<string, int>("resisted", 2),
                new KeyValuePair<string, int>("interrupted", 3), new KeyValuePair<string, int>("future", 999),
                new KeyValuePair<string, int>("Affected", 2) };
            var restored = ElementalNereidPolicy.RestoreResponses(entries);
            entries.Clear();
            Assertions.Equal(5, restored.Count, "Reconstructed exposure state owns its independent, case-sensitive identity map.");
            foreach (var entry in restored)
                Assertions.Equal(entry.Key == "affected" ? ElementalNereidEntry.ApplyRemaining : ElementalNereidEntry.Ignore,
                    ElementalNereidPolicy.Enter((ElementalNereidResponse)entry.Value, true, true),
                    "Restoration cannot reroll, re-fascinate a terminal target, or reinterpret unknown state.");
            Assertions.Throws<ArgumentException>(() => ElementalNereidPolicy.RestoreResponses(new[] {
                new KeyValuePair<string, int>("unit", 1), new KeyValuePair<string, int>("unit", 3) }),
                "Contradictory duplicate ownership cannot be guessed.");
            Assertions.Throws<ArgumentException>(() => ElementalNereidPolicy.RestoreResponses(new[] {
                new KeyValuePair<string, int>(" ", 1) }), "Missing target identity cannot become an unowned effect.");
        }
        internal static void LevelAndCharismaBoundaries()
        {
            foreach (int level in new[] { 1, 2, 5, 6, 9, 10, 19, 20 })
            foreach (int charisma in new[] { -5, -2, 0, 1, 4, 10 })
            {
                Assertions.Equal(10 + level / 2 + charisma, ElementalNereidPolicy.DifficultyClass(level, charisma),
                    "Nereid uses total level and current Charisma without a spell-level DC or level cap.");
                Assertions.Equal(Math.Max(1, level / 2), ElementalNereidPolicy.DurationRounds(level),
                    "Duration rounds down, with the printed minimum of one round.");
            }
            Assertions.Equal(20, ElementalNereidPolicy.RadiusFeet, "The aura radius is twenty feet.");
        }
        internal static void ChangedCharismaAndMulticlassLevel()
        {
            int fighter = 3, wizard = 4;
            Assertions.Equal(15, ElementalNereidPolicy.DifficultyClass(fighter + wizard, 2),
                "Multiclass scaling must use total character level.");
            Assertions.Equal(19, ElementalNereidPolicy.DifficultyClass(fighter + wizard, 6),
                "A temporary Charisma change must change the next activation's DC.");
            Assertions.Equal(3, ElementalNereidPolicy.DurationRounds(fighter + wizard),
                "Charisma must not change duration.");
            foreach (int invalid in new[] { int.MinValue, -1, 0 })
                Assertions.Equal(1, ElementalNereidPolicy.DurationRounds(invalid),
                    "Construction must not create a nonpositive duration.");
        }
        internal static void SavedOrInterruptedTargetsCannotBeReapplied()
        {
            SavedResponsesPreserveExposureDecisions();
            foreach (var terminal in new[] { ElementalNereidResponse.Resisted, ElementalNereidResponse.Interrupted })
            for (int entries = 0; entries < 10; ++entries)
                Assertions.Equal(ElementalNereidEntry.Ignore, ElementalNereidPolicy.Enter(terminal, true, true),
                    "Threats and successful saves remain terminal across ticks, re-entry, and restored exposure state.");
            Assertions.Equal(ElementalNereidEntry.Ignore,
                ElementalNereidPolicy.Enter((ElementalNereidResponse)999, true, true),
                "Unknown saved response state fails closed.");
        }
        internal static void ReentryCannotRerollOrExtendExpiredEffects()
        {
            Assertions.Equal(ElementalNereidEntry.Save,
                ElementalNereidPolicy.Enter(ElementalNereidResponse.Unseen, true, true),
                "A newly exposed eligible target receives one save.");
            Assertions.Equal(ElementalNereidEntry.ApplyRemaining,
                ElementalNereidPolicy.Enter(ElementalNereidResponse.Affected, true, true),
                "A previously failed target re-enters only for the original remaining duration.");
            foreach (ElementalNereidResponse response in Enum.GetValues(typeof(ElementalNereidResponse)))
            {
                Assertions.Equal(ElementalNereidEntry.Ignore, ElementalNereidPolicy.Enter(response, true, false),
                    "Expiration cannot restart an activation.");
                Assertions.Equal(ElementalNereidEntry.Ignore, ElementalNereidPolicy.Enter(response, false, true),
                    "Caster and native nonhumanoid exclusions never request a save or buff.");
            }
        }
    }
}
