using System;
using KingmakerGunslinger.Deeds;
using KingmakerGunslinger.Firearms;

namespace KingmakerGunslinger.DomainTests
{
    internal static partial class Program
    {
        private static void DeadShotBabCadence()
        {
            AssertBonuses(1, 1);
            AssertBonuses(5, 5);
            AssertBonuses(6, 6, 1);
            AssertBonuses(10, 10, 5);
            AssertBonuses(11, 11, 6, 1);
            AssertBonuses(15, 15, 10, 5);
            AssertBonuses(16, 16, 11, 6, 1);
            AssertBonuses(20, 20, 15, 10, 5);
            Assertions.Equal(DeadShotStatus.NoAttacks, Plan(0).Status,
                "BAB zero exposed a Dead Shot roll.");
        }

        private static void DeadShotPreconditionsAtomic()
        {
            AssertRejected(new DeadShotRequest(false, false, FirearmCondition.Normal,
                1, 1, 7), DeadShotStatus.NotExactEquippedFirearm);
            AssertRejected(new DeadShotRequest(true, true, FirearmCondition.Normal,
                1, 1, 7), DeadShotStatus.ScatterWeapon);
            AssertRejected(new DeadShotRequest(true, false, FirearmCondition.Wrecked,
                1, 1, 7), DeadShotStatus.Wrecked);
            AssertRejected(new DeadShotRequest(true, false, FirearmCondition.Normal,
                0, 1, 7), DeadShotStatus.Empty);
            AssertRejected(new DeadShotRequest(true, false, FirearmCondition.Normal,
                1, 0, 7), DeadShotStatus.InsufficientGrit);
        }

        private static void DeadShotHitAndDiceAggregation()
        {
            DeadShotDecision plan = Plan(16);
            DeadShotOutcome none = Outcome(plan, R(false), R(false), R(false), R(false));
            Assertions.Equal(0, none.HitCount, "Missed Dead Shot dealt damage.");
            Assertions.Equal(0, none.BaseDamageDicePackets, "Missed shot added dice.");
            DeadShotOutcome one = Outcome(plan, R(true), R(false), R(false), R(false));
            Assertions.Equal(1, one.BaseDamageDicePackets, "Single hit changed base dice.");
            Assertions.Equal(0, one.AdditionalBaseDamageDicePackets,
                "First hit was treated as additional damage.");
            DeadShotOutcome three = Outcome(plan, R(true), R(false), R(true), R(true));
            Assertions.Equal(3, three.HitCount, "Hit aggregation changed.");
            Assertions.Equal(3, three.BaseDamageDicePackets, "Base dice packets changed.");
            Assertions.Equal(2, three.AdditionalBaseDamageDicePackets,
                "Additional base dice count changed.");
        }

        private static void DeadShotMisfireAggregation()
        {
            DeadShotDecision plan = Plan(11);
            Assertions.True(Outcome(plan, M(), M(), M()).Misfires,
                "All-misfire Dead Shot did not misfire.");
            Assertions.False(Outcome(plan, M(), R(false), M()).Misfires,
                "A mixed Dead Shot incorrectly misfired.");
            Assertions.False(Outcome(plan, M(), R(true), M()).Misfires,
                "A successful ordinary roll did not suppress misfire.");
        }

        private static void DeadShotCriticalAggregation()
        {
            DeadShotDecision plan = Plan(16);
            DeadShotOutcome none = Outcome(plan, R(true), R(false), R(false), R(false));
            Assertions.Equal(null, none.ConfirmationPenalty,
                "Non-threat requested confirmation.");
            DeadShotOutcome one = Outcome(plan, T(), R(true), R(false), R(false));
            Assertions.Equal(1, one.ThreatCount, "Threat count changed.");
            Assertions.Equal(-5, one.ConfirmationPenalty.Value,
                "Single-threat confirmation penalty changed.");
            DeadShotOutcome four = Outcome(plan, T(), T(), T(), T());
            Assertions.Equal(-2, four.ConfirmationPenalty.Value,
                "Multiple-threat confirmation penalty changed.");
        }

        private static void DeadShotInvalidInputs()
        {
            var service = new DeadShotService();
            Assertions.Throws<ArgumentNullException>(() => service.Evaluate(null),
                "Null Dead Shot request was accepted.");
            Assertions.Throws<ArgumentOutOfRangeException>(() =>
                new DeadShotRequest(true, false, FirearmCondition.Normal, -1, 1, 7),
                "Negative chambers were accepted.");
            Assertions.Throws<ArgumentOutOfRangeException>(() =>
                new DeadShotRequest(true, false, FirearmCondition.Normal, 1, -1, 7),
                "Negative grit was accepted.");
            Assertions.Throws<ArgumentOutOfRangeException>(() =>
                new DeadShotRequest(true, false, FirearmCondition.Normal, 1, 1, 21),
                "BAB above 20 was accepted.");
            DeadShotDecision plan = Plan(6);
            var outcomes = new DeadShotOutcomeService();
            Assertions.Throws<ArgumentException>(() => outcomes.Evaluate(plan,
                new[] { R(true) }), "Wrong roll count was accepted.");
            Assertions.Throws<ArgumentException>(() => outcomes.Evaluate(plan,
                new DeadShotRollObservation[] { R(true), null }),
                "Missing roll was accepted.");
            Assertions.Throws<ArgumentException>(() => new DeadShotRollObservation(
                true, true, false), "Hit misfire was accepted.");
            Assertions.Throws<ArgumentException>(() => new DeadShotRollObservation(
                false, false, true), "Missed threat was accepted.");
        }

        private static DeadShotDecision Plan(int bab)
        {
            return new DeadShotService().Evaluate(new DeadShotRequest(true, false,
                FirearmCondition.Normal, 1, 1, bab));
        }

        private static void AssertBonuses(int bab, params int[] expected)
        {
            DeadShotDecision result = Plan(bab);
            Assertions.Equal(DeadShotStatus.Eligible, result.Status,
                "Eligible Dead Shot was rejected.");
            Assertions.Equal(1, result.GritCost, "Dead Shot grit cost changed.");
            Assertions.Equal(1, result.ChamberCost, "Dead Shot chamber cost changed.");
            Assertions.Equal(string.Join(",", expected),
                string.Join(",", result.AttackBonuses), "Iterative bonuses changed.");
        }

        private static void AssertRejected(DeadShotRequest request, DeadShotStatus status)
        {
            DeadShotDecision result = new DeadShotService().Evaluate(request);
            Assertions.Equal(status, result.Status, "Dead Shot rejection changed.");
            Assertions.Equal(0, result.AttackBonuses.Length,
                "Rejected Dead Shot exposed attack rolls.");
            Assertions.Equal(0, result.GritCost, "Rejected Dead Shot exposed grit cost.");
            Assertions.Equal(0, result.ChamberCost,
                "Rejected Dead Shot exposed chamber cost.");
        }

        private static DeadShotOutcome Outcome(DeadShotDecision plan,
            params DeadShotRollObservation[] rolls)
        {
            return new DeadShotOutcomeService().Evaluate(plan, rolls);
        }

        private static DeadShotRollObservation R(bool hit)
        {
            return new DeadShotRollObservation(hit, false, false);
        }

        private static DeadShotRollObservation M()
        {
            return new DeadShotRollObservation(false, true, false);
        }

        private static DeadShotRollObservation T()
        {
            return new DeadShotRollObservation(true, false, true);
        }
    }
}
