using System;

namespace KingmakerGunslinger.ElementalRaces
{
    internal static class ElementalRacialSpellLikePolicy
    {
        internal static int CasterLevel(int totalCharacterLevel)
        {
            return Math.Max(1, totalCharacterLevel);
        }

        internal static int SpellLevel(int configuredSpellLevel)
        {
            return Math.Max(1, configuredSpellLevel);
        }

        internal static int DifficultyClass(int configuredSpellLevel,
            int currentAbilityModifier)
        {
            return 10 + SpellLevel(configuredSpellLevel) +
                currentAbilityModifier;
        }
    }
}
