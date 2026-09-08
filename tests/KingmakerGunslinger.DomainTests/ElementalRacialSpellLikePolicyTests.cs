using KingmakerGunslinger.ElementalRaces;

namespace KingmakerGunslinger.DomainTests
{
    internal static class ElementalRacialSpellLikePolicyTests
    {
        internal static void ParametersUseExactConfiguredLevels()
        {
            Assertions.Equal(1,
                ElementalRacialSpellLikePolicy.CasterLevel(0),
                "A level-zero construction preview must fail closed to caster level one.");
            Assertions.Equal(5,
                ElementalRacialSpellLikePolicy.CasterLevel(5),
                "The total character level must be the racial SLA caster level.");
            Assertions.Equal(1,
                ElementalRacialSpellLikePolicy.SpellLevel(0),
                "An invalid configured spell level must fail closed to one.");
            Assertions.Equal(3,
                ElementalRacialSpellLikePolicy.SpellLevel(3),
                "A valid configured spell level must be preserved exactly.");
        }

        internal static void DcUsesCurrentAbilityModifierExactly()
        {
            Assertions.Equal(6,
                ElementalRacialSpellLikePolicy.DifficultyClass(1, -5),
                "A current Charisma penalty must reduce a first-level SLA DC.");
            Assertions.Equal(11,
                ElementalRacialSpellLikePolicy.DifficultyClass(1, 0),
                "A first-level SLA with a zero modifier must have DC 11.");
            Assertions.Equal(15,
                ElementalRacialSpellLikePolicy.DifficultyClass(1, 4),
                "A current +4 Charisma modifier must produce DC 15.");
            Assertions.Equal(18,
                ElementalRacialSpellLikePolicy.DifficultyClass(1, 7),
                "A temporary increase to the current modifier must be reflected exactly.");
        }
    }
}
