using System;

namespace KingmakerGunslinger.ElementalRaces
{
    /// <summary>Printed Undine breath scaling, independent of class split or spell level.</summary>
    internal static class ElementalBreathPolicy
    {
        internal const int MaximumDamageDice = 5;
        internal const int ConeFeet = 5;
        internal const int SickenedRounds = 3;

        internal static int HalfLevel(int totalLevel)
        {
            return Math.Max(0, totalLevel) / 2;
        }

        internal static int DamageDice(int totalLevel)
        {
            return Math.Min(MaximumDamageDice, HalfLevel(totalLevel));
        }

        internal static int DifficultyClass(int totalLevel, int constitutionModifier)
        {
            return 10 + HalfLevel(totalLevel) + constitutionModifier;
        }

        internal static string Description(bool ooze)
        {
            string dice = ooze ? "d4" : "d8";
            return "Once per ordinary rest, use a standard action to breathe a 5-foot cone. " +
                "It deals 1" + dice + " acid damage per two total character levels (maximum 5" + dice +
                "). Reflex DC 10 + half your total character level + your current Constitution modifier halves the damage. " +
                (ooze ? "A failed save also sickens the target for 3 rounds, even if acid resistance or immunity prevents damage. " : "") +
                "Half levels round down: at level 1 there is no damage die. " +
                (ooze ? "The failed-save sickened effect still applies. " : "") +
                "This is a supernatural breath weapon, not a spell or a racial spell-like ability.";
        }
    }
}
