using System;
using KingmakerGunslinger.ElementalRaces;

namespace KingmakerGunslinger.DomainTests
{
    internal static class ElementalHeritageSlaPolicyTests
    {
        internal static void UnerringWeaponUsesExactBreakpointAndCap()
        {
            Assertions.Equal(0,
                ElementalHeritageSlaPolicy.UnerringConfirmationBonus(0),
                "An invalid caster level must grant no confirmation bonus.");
            Assertions.Equal(2,
                ElementalHeritageSlaPolicy.UnerringConfirmationBonus(1),
                "Caster level 1 must grant the printed base +2.");
            Assertions.Equal(2,
                ElementalHeritageSlaPolicy.UnerringConfirmationBonus(3),
                "The first scaling point must not arrive before level 4.");
            Assertions.Equal(3,
                ElementalHeritageSlaPolicy.UnerringConfirmationBonus(4),
                "Caster level 4 must add exactly one point.");
            Assertions.Equal(6,
                ElementalHeritageSlaPolicy.UnerringConfirmationBonus(19),
                "Caster level 19 must grant +6 total.");
            Assertions.Equal(7,
                ElementalHeritageSlaPolicy.UnerringConfirmationBonus(20),
                "Caster level 20 must reach the printed +7 cap.");
            Assertions.Equal(7,
                ElementalHeritageSlaPolicy.UnerringConfirmationBonus(100),
                "The printed +7 cap must hold above level 20.");
        }

        internal static void ChillTouchUsesAndUndeadDurationAreExact()
        {
            Assertions.Equal(1,
                ElementalHeritageSlaPolicy.ChillTouchCount(0),
                "A construction preview must fail closed to one touch.");
            Assertions.Equal(1,
                ElementalHeritageSlaPolicy.ChillTouchCount(1),
                "Caster level 1 must grant one touch.");
            Assertions.Equal(5,
                ElementalHeritageSlaPolicy.ChillTouchCount(5),
                "Caster level 5 must grant five touches.");
            Assertions.Equal(20,
                ElementalHeritageSlaPolicy.ChillTouchCount(20),
                "Chill Touch must retain one touch per caster level.");

            Assertions.Equal(2,
                ElementalHeritageSlaPolicy.ChillTouchUndeadPanicRounds(1, 1),
                "The minimum undead panic duration must be 1d4 + level.");
            Assertions.Equal(14,
                ElementalHeritageSlaPolicy.ChillTouchUndeadPanicRounds(4, 10),
                "Caster level 10 must add all ten rounds.");
            Assertions.Equal(12,
                ElementalHeritageSlaPolicy.ChillTouchUndeadPanicRounds(1, 11),
                "The caster-level contribution must not cap at ten.");
            Assertions.Equal(24,
                ElementalHeritageSlaPolicy.ChillTouchUndeadPanicRounds(4, 20),
                "Caster level 20 must add all twenty rounds.");
            Assertions.Throws<ArgumentOutOfRangeException>(() =>
                ElementalHeritageSlaPolicy.ChillTouchUndeadPanicRounds(0, 5),
                "A non-d4 result must fail closed.");
            Assertions.Throws<ArgumentOutOfRangeException>(() =>
                ElementalHeritageSlaPolicy.ChillTouchUndeadPanicRounds(5, 5),
                "A result above the d4 range must fail closed.");

            string guid = "e115e1e0a17a4aceb001000000000052";
            string reconstructed = new string(guid.ToCharArray());
            Assertions.True(!object.ReferenceEquals(guid, reconstructed),
                "The regression fixture must use distinct string instances.");
            Assertions.True(ElementalHeritageSlaPolicy.ExactDeliveryMatch(
                    guid, reconstructed, guid),
                "Distinct runtime wrappers for the same exact stable identity " +
                "must retain the remaining Chill Touch charges.");
            Assertions.False(ElementalHeritageSlaPolicy.ExactDeliveryMatch(
                    guid, guid, "e115e1e0a17a4aceb001000000000053"),
                "A different executing delivery identity must fail closed.");
            Assertions.False(ElementalHeritageSlaPolicy.ExactDeliveryMatch(
                    guid, guid.ToUpperInvariant(), guid),
                "Delivery identity matching must remain ordinal and exact.");
            Assertions.False(ElementalHeritageSlaPolicy.ExactDeliveryMatch(
                    guid, string.Empty, guid),
                "A missing held-delivery identity must fail closed.");
        }
    }
}
