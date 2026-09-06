using System;
using KingmakerGunslinger.ElementalRaces;

namespace KingmakerGunslinger.DomainTests
{
    internal static class ElementalAlternateTraitPassivePolicyTests
    {
        internal static void SavesUseOneExactRacialBonus()
        {
            foreach (ElementalAlternateTraitId trait in Enum.GetValues(
                typeof(ElementalAlternateTraitId)))
                for (int mask = 0; mask < 16; mask++)
                {
                    bool fatigue = (mask & 1) != 0;
                    bool exhaustion = (mask & 2) != 0;
                    bool enchantment = (mask & 4) != 0;
                    bool divination = (mask & 8) != 0;
                    int expected = trait == ElementalAlternateTraitId
                            .ForgeHardened && (mask & 3) != 0 ||
                        trait == ElementalAlternateTraitId.Secretive &&
                            (mask & 12) != 0 ? 2 : 0;
                    Assertions.Equal(expected,
                        ElementalAlternateTraitPassivePolicy.SavingThrowBonus(
                            trait, fatigue, exhaustion, enchantment,
                            divination),
                        trait + "/" + mask +
                            " must apply one +2 bonus only to its named sources.");
                }
        }

        internal static void BrazenFlameRequiresOneNativeMeleeHit()
        {
            for (int mask = 0; mask < 16; mask++)
                Assertions.Equal(mask == 7 ? 1 : 0,
                    ElementalAlternateTraitPassivePolicy.BrazenFlameDamage(
                        (mask & 1) != 0, (mask & 2) != 0,
                        (mask & 4) != 0, (mask & 8) != 0),
                    "Brazen Flame must reject misses, ranged/nonweapon " +
                    "damage, uncorrelated events, and spell damage: " + mask);
        }
    }
}
