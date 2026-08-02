using System;
using KingmakerGunslinger.Deeds;

namespace KingmakerGunslinger.DomainTests
{
    internal static partial class Program
    {
        private static void BleedingWoundAllChoices()
        {
            var service = new BleedingWoundService();
            BleedingWoundDecision hp = service.Evaluate(Request(
                BleedingWoundKind.HitPoints, grit: 1, dexterityModifier: 4));
            Assertions.True(hp.Apply, "Eligible HP bleed was rejected.");
            Assertions.Equal(1, hp.GritCost, "HP bleed cost changed.");
            Assertions.Equal(4, hp.BleedAmount, "HP bleed amount changed.");
            foreach (BleedingWoundKind kind in new[] { BleedingWoundKind.Strength,
                BleedingWoundKind.Dexterity, BleedingWoundKind.Constitution })
            {
                BleedingWoundDecision stat = service.Evaluate(Request(kind,
                    grit: 2, dexterityModifier: -3));
                Assertions.True(stat.Apply, kind + " bleed was rejected.");
                Assertions.Equal(2, stat.GritCost, kind + " cost changed.");
                Assertions.Equal(1, stat.BleedAmount, kind + " amount changed.");
            }
        }

        private static void BleedingWoundMarkerConsumption()
        {
            var service = new BleedingWoundService();
            Assertions.False(service.Evaluate(Request(BleedingWoundKind.HitPoints,
                exact: false)).ConsumeMarker,
                "Non-firearm action consumed Bleeding Wound marker.");
            BleedingWoundDecision miss = service.Evaluate(Request(
                BleedingWoundKind.HitPoints, hit: false));
            Assertions.True(miss.ConsumeMarker,
                "Exact firearm miss retained Bleeding Wound marker.");
            Assertions.False(miss.Apply, "Miss applied Bleeding Wound.");
        }

        private static void BleedingWoundGates()
        {
            var service = new BleedingWoundService();
            Assertions.False(service.Evaluate(Request(BleedingWoundKind.HitPoints,
                living: false)).Apply, "Nonliving target received bleed.");
            Assertions.False(service.Evaluate(Request(BleedingWoundKind.HitPoints,
                immune: true)).Apply, "Sneak-immune target received bleed.");
            Assertions.False(service.Evaluate(Request(BleedingWoundKind.HitPoints,
                grit: 0)).Apply, "Insufficient grit applied HP bleed.");
            Assertions.False(service.Evaluate(Request(BleedingWoundKind.Strength,
                grit: 1)).Apply, "One grit applied ability bleed.");
            Assertions.False(service.Evaluate(Request(BleedingWoundKind.HitPoints,
                dexterityModifier: 0)).Apply,
                "Nonpositive Dexterity modifier applied HP bleed.");
        }

        private static void BleedingWoundInvalid()
        {
            var service = new BleedingWoundService();
            Assertions.Throws<ArgumentNullException>(() => service.Evaluate(null),
                "Null Bleeding Wound request was accepted.");
            Assertions.Throws<ArgumentOutOfRangeException>(() =>
                new BleedingWoundRequest((BleedingWoundKind)99, true, true,
                    true, true, false, 1, 1),
                "Unknown Bleeding Wound choice was accepted.");
            Assertions.Throws<ArgumentOutOfRangeException>(() =>
                new BleedingWoundRequest(BleedingWoundKind.HitPoints, true, true,
                    true, true, false, -1, 1),
                "Negative grit was accepted.");
        }

        private static BleedingWoundRequest Request(BleedingWoundKind kind,
            bool exact = true, bool eligible = true, bool hit = true,
            bool living = true, bool immune = false, int grit = 3,
            int dexterityModifier = 3)
        {
            return new BleedingWoundRequest(kind, exact, eligible, hit, living,
                immune, grit, dexterityModifier);
        }
    }
}
