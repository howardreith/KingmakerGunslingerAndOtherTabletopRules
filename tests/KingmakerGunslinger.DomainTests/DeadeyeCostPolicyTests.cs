using KingmakerGunslinger.Deeds;
using KingmakerGunslinger.Firearms;

namespace KingmakerGunslinger.DomainTests
{
    /// <summary>
    /// POL-02 / S4 T4, owner decision: Deadeye's flat one-grit arming cost is
    /// the single authority; True Grit adjusts it; the shot's range decides
    /// only the touch-AC benefit. Activation, consumption, cancellation and
    /// expiry are native behaviour and are observed by the loader-compat
    /// deadeyeCost probe.
    /// </summary>
    internal static class DeadeyeCostPolicyTests
    {
        private static DeadeyeDecision Shot(double feet)
        {
            return new DeadeyeService().Evaluate(new DeadeyeRequest(true, true, 1,
                ProductionFirearmCatalog.CreatePistol().Definition, feet * 0.3048d));
        }

        internal static void FlatCostAtEveryRangeBoundary()
        {
            // Pistol: 20 ft increments. The boundary tolerance keeps exactly 20 ft in the first increment.
            foreach (double feet in new[] { 5d, 20d, 20.5d, 40d, 40.5d, 60.5d, 100.5d })
            {
                DeadeyeDecision shot = Shot(feet);
                Assertions.Equal(1, DeadeyeCostPolicy.Arming(10, false).EffectiveCost,
                    "Arming costs one grit whatever the later shot's range (" + feet + " ft).");
                Assertions.Equal(feet > 20.01d, shot.UsesTouchArmorClass,
                    "Touch AC applies exactly beyond the first increment (" + feet + " ft): " + shot.Status);
            }
            Assertions.Equal(DeadeyeStatus.WithinFirstIncrement, Shot(20d).Status, "20 ft is the first increment.");
            Assertions.Equal(2, Shot(20.5d).RangeIncrement, "20.5 ft is the second increment.");
            Assertions.Equal(6, Shot(100.5d).RangeIncrement, "100.5 ft is the sixth increment.");
        }

        internal static void ArmingNeedsOneGrit()
        {
            TrueGritDecision none = DeadeyeCostPolicy.Arming(0, false);
            TrueGritDecision one = DeadeyeCostPolicy.Arming(1, false);
            Assertions.False(none.Available, "No grit refuses arming.");
            Assertions.Equal(1, none.NativeCheckCost, "The native check compares one grit with the pool.");
            Assertions.True(one.Available && one.EffectiveCost == 1, "One grit arms and is charged one.");
        }

        internal static void TrueGritReducesTheArmingCost()
        {
            TrueGritDecision selected = DeadeyeCostPolicy.Arming(1, true);
            Assertions.True(selected.Available && selected.EffectiveCost == 0,
                "True Grit (Deadeye) arms for no grit while the caster has some.");
            TrueGritDecision empty = DeadeyeCostPolicy.Arming(0, true);
            Assertions.False(empty.Available, "True Grit still needs positive grit to arm.");
            Assertions.Equal(1, empty.NativeCheckCost, "The native check keeps one grit while it is unavailable.");
        }
    }
}
