using System;
using KingmakerGunslinger.ElementalRaces;

namespace KingmakerGunslinger.DomainTests
{
    internal static class ElementalTreacherousPolicyTests
    {
        internal static void TotalLevelDuration()
        {
            foreach (int level in new[] { 1, 2, 5, 6, 9, 10, 19, 20 })
                Assertions.Equal(level, ElementalTreacherousPolicy.DurationMinutes(level),
                    "The native patch lasts one minute per total character level.");
            Assertions.Equal(7, ElementalTreacherousPolicy.DurationMinutes(3 + 4),
                "Multiclass levels add; no class-specific caster-level bonus changes the duration.");
            Assertions.Equal(10, ElementalTreacherousPolicy.RadiusFeet, "The patch has a ten-foot radius.");
        }
        internal static void ExactMaterialAndGroundRequirements()
        {
            foreach (bool contact in new[] { false, true })
                foreach (bool touch in new[] { false, true })
                    foreach (bool effect in new[] { false, true })
                        foreach (bool path in new[] { false, true })
                            Assertions.Equal(contact && touch && effect && path,
                                ElementalTreacherousPolicy.Eligible(contact, touch, effect, path),
                                "The delegated ground adaptation requires actual contact, touch reach, line of effect and a continuous local ground path.");
        }
        internal static void UnknownEvidenceFailsClosed()
        {
            Assertions.False(ElementalTreacherousPolicy.Eligible(false, true, true, true),
                "Unknown ground geometry never qualifies merely because a target is close and visible.");
            Assertions.True(ElementalAlternateTraitPolicy.Find(ElementalAlternateTraitId.TreacherousEarth).IsPublished,
                "Owner-authorized publication does not relax the actual-ground eligibility policy.");
        }
    }
}
