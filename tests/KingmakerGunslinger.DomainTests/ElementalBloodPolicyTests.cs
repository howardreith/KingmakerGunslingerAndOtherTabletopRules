using System;
using KingmakerGunslinger.ElementalRaces;

namespace KingmakerGunslinger.DomainTests
{
    internal static class ElementalBloodPolicyTests
    {
        internal static void ActualHealingAndNativeBoundariesAreExact()
        {
            foreach (ElementalAlternateTraitId trait in Enum.GetValues(
                typeof(ElementalAlternateTraitId)))
                for (int mask = 0; mask < 8; mask++)
                {
                    bool blood = trait == ElementalAlternateTraitId.FireInTheBlood ||
                        trait == ElementalAlternateTraitId.StoneInTheBlood ||
                        trait == ElementalAlternateTraitId.StormInTheBlood;
                    Assertions.Equal(blood && mask == 3,
                        ElementalBloodPolicy.Triggers(trait, (mask & 1) != 0,
                            (mask & 2) != 0 ? 3 : 0, (mask & 4) != 0),
                        trait + "/" + mask + ": real matching positive packet only.");
                }
            foreach (int level in new[] { 0, 1, 2, 5, 10, 20, 40 })
                for (int spent = 0; spent <= level * 2 + 3; spent++)
                {
                    int remaining = Math.Max(0, level * 2 - spent);
                    Assertions.Equal(remaining,
                        ElementalBloodPolicy.Remaining(level, spent),
                        "Current total level determines maximum, not expenditure.");
                    foreach (float modifier in new[] { 0f, 0.25f, 0.5f, 1f, 1.5f, 3f })
                        for (int wounds = 0; wounds <= 7; wounds++)
                        {
                            float actualModifier = ElementalBloodPolicy
                                .CappedHealingModifier(modifier, remaining);
                            int received = Math.Min(wounds, (int)(2 * actualModifier));
                            Assertions.Equal(Math.Min(wounds,
                                    Math.Min(remaining, (int)(2 * modifier))), received,
                                "Native healing modifiers, wounds and daily cap compose.");
                            Assertions.Equal(spent + received,
                                ElementalBloodPolicy.Spend(level, spent, received),
                                "Only HP actually received consumes daily capacity.");
                        }
                }
            Assertions.Equal(1, ElementalBloodPolicy.Remaining(1, 1),
                "One healed HP leaves one HP, not a spent nominal two-HP tick.");
            Assertions.Equal(3, ElementalBloodPolicy.Remaining(2, 1),
                "Level-up increases maximum without forgetting already healed HP.");
            Assertions.Equal(0, ElementalBloodPolicy.Remaining(1, 7),
                "Level loss cannot reset previous expenditure.");
            Assertions.Equal(0, ElementalBloodPolicy.Remaining(20, -1),
                "Corrupt negative expenditure fails closed.");
            Assertions.Equal(int.MaxValue, ElementalBloodPolicy.Maximum(int.MaxValue),
                "Maximum calculation does not overflow.");
            foreach (float invalid in new[] { float.NaN, float.PositiveInfinity,
                float.NegativeInfinity, -1f })
                Assertions.Equal(0f, ElementalBloodPolicy.CappedHealingModifier(
                    invalid, 4), "Invalid healing modifiers fail closed.");
            bool rejected = false;
            try { ElementalBloodPolicy.Spend(1, 1, 2); }
            catch (ArgumentOutOfRangeException) { rejected = true; }
            Assertions.True(rejected, "Over-cap expenditure is not silently accepted.");
        }
    }
}
