using System;
using KingmakerGunslinger.Firearms;
using KingmakerGunslinger.Explosions;
using KingmakerGunslinger.Misfires;
using KingmakerGunslinger.Scatter;

namespace KingmakerGunslinger.DomainTests
{
    internal static partial class Program
    {
        private static void ScatterDistanceMissingRejected()
        {
            Assertions.Throws<InvalidOperationException>(() => new ScatterConeDistanceService().Resolve(
                FirearmDefinitions.CreateEarlyBlunderbuss(), null), "Missing authority did not fail closed.");
        }

        private static void ScatterDistanceExactConversion()
        {
            ScatterConeDistanceDecision decision = new ScatterConeDistanceService().Resolve(
                FirearmDefinitions.CreateEarlyBlunderbuss(), 15);
            Assertions.Equal(15, decision.DistanceFeet, "Authorized feet changed.");
            Assertions.Equal(4.572f, decision.DistanceMeters, "Native meter conversion changed.");
        }

        private static void ScatterDistancePnPBlunderbussAuthority()
        {
            ScatterConeDistanceDecision decision =
                new ScatterConeDistanceService().ResolveBlunderbuss(
                    FirearmDefinitions.CreateEarlyBlunderbuss());
            Assertions.Equal(15, ScatterConeDistanceService.BlunderbussConeDistanceFeet,
                "The authorized PnP Blunderbuss cone distance changed.");
            Assertions.Equal(15, decision.DistanceFeet,
                "The PnP Blunderbuss cone did not resolve to 15 feet.");
            Assertions.Equal(4.572f, decision.DistanceMeters,
                "The PnP Blunderbuss cone native conversion changed.");
            Assertions.Throws<ArgumentException>(() =>
                new ScatterConeDistanceService().ResolveBlunderbuss(
                    FirearmDefinitions.CreateEarlyMusket()),
                "PnP Blunderbuss cone authority leaked to another firearm.");
        }

        private static void ScatterDistanceNonScatterRejected()
        {
            Assertions.Throws<ArgumentException>(() => new ScatterConeDistanceService().Resolve(
                FirearmDefinitions.CreateEarlyMusket(), 15), "A non-scatter firearm accepted cone distance.");
        }

        private static void ScatterDistanceStepRejected()
        {
            Assertions.Throws<ArgumentException>(() => new ScatterConeDistanceService().Resolve(
                FirearmDefinitions.CreateEarlyBlunderbuss(), 12), "A non-five-foot distance was accepted.");
        }

        private static void ScatterDistanceBoundsRejected()
        {
            Assertions.Throws<ArgumentOutOfRangeException>(() => new ScatterConeDistanceService().Resolve(
                FirearmDefinitions.CreateEarlyBlunderbuss(), 0), "A zero cone distance was accepted.");
            Assertions.Throws<ArgumentOutOfRangeException>(() => new ScatterConeDistanceService().Resolve(
                FirearmDefinitions.CreateEarlyBlunderbuss(), 1005), "An excessive cone distance was accepted.");
        }

        private static void ScatterPlanEmpty()
        {
            ScatterTargetPlan plan = new ScatterTargetPlanService().Build(
                new object(), new ScatterTargetCandidate[0]);
            Assertions.Equal(0, plan.TargetCount, "Empty cone produced a target.");
            Assertions.Equal(0, plan.ObservedCandidates, "Observed count mismatch.");
        }

        private static void ScatterPlanSingleton()
        {
            object wielder = new object();
            object target = new object();
            ScatterTargetPlan plan = new ScatterTargetPlanService().Build(
                wielder, new[] { ScatterCandidate(target, "b", "Target", 2f,
                    ScatterGeometryDisposition.Inside) });
            Assertions.Equal(1, plan.TargetCount, "Singleton target was lost.");
            Assertions.True(ReferenceEquals(target, plan.Targets[0].Unit),
                "Singleton reference identity changed.");
        }

        private static void ScatterPlanFiltersOutsideAndWielder()
        {
            object wielder = new object();
            ScatterTargetPlan plan = new ScatterTargetPlanService().Build(
                wielder, new[]
                {
                    ScatterCandidate(wielder, "w", "Wielder", 0f,
                        ScatterGeometryDisposition.Inside),
                    ScatterCandidate(new object(), "o", "Outside", 3f,
                        ScatterGeometryDisposition.Outside)
                });
            Assertions.Equal(0, plan.TargetCount, "Rejected candidates entered the plan.");
            Assertions.Equal(1, plan.OutsideCandidates, "Outside count mismatch.");
            Assertions.Equal(1, plan.WielderCandidates, "Wielder count mismatch.");
        }

        private static void ScatterPlanDeduplicatesReference()
        {
            object target = new object();
            ScatterTargetPlan plan = new ScatterTargetPlanService().Build(
                new object(), new[]
                {
                    ScatterCandidate(target, "a", "First", 2f, ScatterGeometryDisposition.Inside),
                    ScatterCandidate(target, "z", "Duplicate", 2f, ScatterGeometryDisposition.Inside)
                });
            Assertions.Equal(1, plan.TargetCount, "Duplicate reference was retained.");
            Assertions.Equal(1, plan.DuplicateCandidates, "Duplicate count mismatch.");
        }

        private static void ScatterPlanDoesNotDeduplicateValueEquality()
        {
            var first = new ValueEqualTarget(7);
            var second = new ValueEqualTarget(7);
            ScatterTargetPlan plan = new ScatterTargetPlanService().Build(
                new object(), new[]
                {
                    ScatterCandidate(first, "a", "First", 2f, ScatterGeometryDisposition.Inside),
                    ScatterCandidate(second, "b", "Second", 2f, ScatterGeometryDisposition.Inside)
                });
            Assertions.Equal(2, plan.TargetCount,
                "Distinct value-equal unit references were collapsed.");
        }

        private static void ScatterPlanStableOrder()
        {
            ScatterTargetPlan plan = new ScatterTargetPlanService().Build(
                new object(), new[]
                {
                    ScatterCandidate(new object(), "z", "Far", 4f, ScatterGeometryDisposition.Inside),
                    ScatterCandidate(new object(), "b", "Tie B", 2f, ScatterGeometryDisposition.Inside),
                    ScatterCandidate(new object(), "a", "Tie A", 2f, ScatterGeometryDisposition.Inside)
                });
            Assertions.Equal("a", plan.Targets[0].StableIdentity, "First order mismatch.");
            Assertions.Equal("b", plan.Targets[1].StableIdentity, "Second order mismatch.");
            Assertions.Equal("z", plan.Targets[2].StableIdentity, "Distance order mismatch.");
        }

        private static void ScatterPlanUnknownGeometryFailsClosed()
        {
            Assertions.Throws<InvalidOperationException>(
                () => new ScatterTargetPlanService().Build(new object(), new[]
                {
                    ScatterCandidate(new object(), "u", "Unknown", 1f,
                        ScatterGeometryDisposition.Unknown)
                }), "Unknown geometry produced a partial target plan.");
        }

        private static void ScatterPlanNullCandidateRejected()
        {
            Assertions.Throws<ArgumentException>(
                () => new ScatterTargetPlanService().Build(
                    new object(), new ScatterTargetCandidate[] { null }),
                "Null native candidate was accepted.");
        }

        private static void ScatterCandidateValueUnitRejected()
        {
            Assertions.Throws<ArgumentException>(
                () => ScatterCandidate(1, "v", "Value", 1f,
                    ScatterGeometryDisposition.Inside),
                "Value-type unit identity was accepted.");
        }

        private static void ScatterCandidateInvalidDistanceRejected()
        {
            Assertions.Throws<ArgumentOutOfRangeException>(
                () => ScatterCandidate(new object(), "n", "NaN", float.NaN,
                    ScatterGeometryDisposition.Inside), "NaN distance was accepted.");
            Assertions.Throws<ArgumentOutOfRangeException>(
                () => ScatterCandidate(new object(), "i", "Infinite",
                    float.PositiveInfinity, ScatterGeometryDisposition.Inside),
                "Infinite distance was accepted.");
            Assertions.Throws<ArgumentOutOfRangeException>(
                () => ScatterCandidate(new object(), "m", "Negative", -1f,
                ScatterGeometryDisposition.Inside), "Negative distance was accepted.");
        }

        private static void ScatterVolleyEmpty()
        {
            ScatterAttackVolleyDecision decision = new ScatterAttackVolleyService().Evaluate(
                FirearmDefinitions.CreateEarlyBlunderbuss(),
                new ScatterTargetPlanService().Build(new object(), new ScatterTargetCandidate[0]),
                new ScatterAttackRollObservation[0]);
            Assertions.Equal(0, decision.TargetCount, "Empty volley target count mismatch.");
            Assertions.False(decision.AllRollsMisfire, "An empty volley misfired vacuously.");
        }

        private static void ScatterVolleySeparateRolls()
        {
            object first = new object();
            object second = new object();
            ScatterTargetPlan plan = ScatterPlan(first, second);
            ScatterAttackVolleyDecision decision = new ScatterAttackVolleyService().Evaluate(
                FirearmDefinitions.CreateEarlyBlunderbuss(), plan, new[]
                {
                    ScatterRoll(first, "a", 10, true, false, false),
                    ScatterRoll(second, "b", 12, false, false, false)
                });
            Assertions.Equal(2, decision.TargetCount, "Volley target count mismatch.");
            Assertions.Equal(1, decision.HitCount, "Volley hit count mismatch.");
            Assertions.Equal(-2, ScatterAttackVolleyDecision.AttackPenalty,
                "Scatter attack penalty changed.");
        }

        private static void ScatterVolleyAllMisfire()
        {
            object first = new object();
            object second = new object();
            ScatterAttackVolleyDecision decision = new ScatterAttackVolleyService().Evaluate(
                FirearmDefinitions.CreateEarlyBlunderbuss(), ScatterPlan(first, second), new[]
                {
                    ScatterRoll(first, "a", 1, false, false, false),
                    ScatterRoll(second, "b", 2, false, false, false)
                });
            Assertions.True(decision.AllRollsMisfire, "All qualifying rolls did not misfire.");
            Assertions.Equal(2, decision.MisfireRollCount, "Misfire count mismatch.");
        }

        private static void ScatterVolleySomeMisfire()
        {
            object first = new object();
            object second = new object();
            ScatterAttackVolleyDecision decision = new ScatterAttackVolleyService().Evaluate(
                FirearmDefinitions.CreateEarlyBlunderbuss(), ScatterPlan(first, second), new[]
                {
                    ScatterRoll(first, "a", 1, false, false, false),
                    ScatterRoll(second, "b", 3, true, false, false)
                });
            Assertions.False(decision.AllRollsMisfire,
                "One misfire incorrectly misfired the complete volley.");
        }

        private static void ScatterVolleyCriticalCounts()
        {
            object first = new object();
            object second = new object();
            ScatterAttackVolleyDecision decision = new ScatterAttackVolleyService().Evaluate(
                FirearmDefinitions.CreateEarlyBlunderbuss(), ScatterPlan(first, second), new[]
                {
                    ScatterRoll(first, "a", 20, true, true, true),
                    ScatterRoll(second, "b", 20, true, true, false)
                });
            Assertions.Equal(2, decision.CriticalThreatCount, "Threat count mismatch.");
            Assertions.Equal(1, decision.ConfirmedCriticalCount,
                "Per-target confirmation count mismatch.");
        }

        private static void ScatterVolleyDamageExclusions()
        {
            ScatterAttackVolleyDecision decision = new ScatterAttackVolleyDecision(0, 0, 0, 0, 0);
            Assertions.False(decision.AllowsPrecisionDamage,
                "Scatter volley allowed precision damage.");
            Assertions.False(decision.AllowsVitalStrikeDamage,
                "Scatter volley allowed Vital Strike damage.");
        }

        private static void ScatterVolleyNonScatterRejected()
        {
            Assertions.Throws<ArgumentException>(
                () => new ScatterAttackVolleyService().Evaluate(
                    FirearmDefinitions.CreateEarlyMusket(),
                    new ScatterTargetPlanService().Build(new object(), new ScatterTargetCandidate[0]),
                    new ScatterAttackRollObservation[0]),
                "A non-scatter firearm entered volley aggregation.");
        }

        private static void ScatterVolleyMissingRollRejected()
        {
            object target = new object();
            Assertions.Throws<InvalidOperationException>(
                () => new ScatterAttackVolleyService().Evaluate(
                    FirearmDefinitions.CreateEarlyBlunderbuss(), ScatterPlan(target),
                    new ScatterAttackRollObservation[0]),
                "A planned target without a roll was accepted.");
        }

        private static void ScatterVolleyDuplicateRollRejected()
        {
            object target = new object();
            ScatterAttackRollObservation roll = ScatterRoll(
                target, "a", 10, true, false, false);
            Assertions.Throws<ArgumentException>(
                () => new ScatterAttackVolleyService().Evaluate(
                    FirearmDefinitions.CreateEarlyBlunderbuss(), ScatterPlan(target),
                    new[] { roll, roll }),
                "A target received duplicate attack rolls.");
        }

        private static void ScatterVolleyUnplannedRollRejected()
        {
            object planned = new object();
            Assertions.Throws<ArgumentException>(
                () => new ScatterAttackVolleyService().Evaluate(
                    FirearmDefinitions.CreateEarlyBlunderbuss(), ScatterPlan(planned),
                    new[] { ScatterRoll(new object(), "x", 10, true, false, false) }),
                "An unplanned target received a scatter roll.");
        }

        private static void ScatterDischargeZeroTargetsOnce()
        {
            ScatterTargetPlan plan = ScatterPlan();
            AssertScatterFiredOnce(plan);
            ScatterAttackVolleyDecision volley =
                new ScatterAttackVolleyService().Evaluate(
                    FirearmDefinitions.CreateEarlyBlunderbuss(), plan,
                    new ScatterAttackRollObservation[0]);
            Assertions.Equal(0, volley.TargetCount,
                "An empty Scatter direction invented an attack target.");
            Assertions.False(volley.AllRollsMisfire,
                "An empty Scatter direction was misclassified as an all-roll misfire.");
        }

        private static void ScatterDischargeOneTargetOnce()
        {
            AssertScatterFiredOnce(ScatterPlan(new object()));
        }

        private static void ScatterDischargeManyTargetsOnce()
        {
            AssertScatterFiredOnce(ScatterPlan(new object(), new object(), new object()));
        }

        private static void ScatterDischargePrerequisiteRejection()
        {
            FirearmState before = LoadedState(1, FirearmCondition.Normal);
            ScatterDischargeDecision decision = new ScatterDischargeService().Evaluate(
                FirearmDefinitions.CreateEarlyBlunderbuss(), before,
                ScatterPlan(new object()), false);
            Assertions.Equal(ScatterDischargeStatus.RejectedBeforeDelivery, decision.Status,
                "Pre-delivery rejection status mismatch.");
            Assertions.Equal(0, decision.RoundsConsumed,
                "Pre-delivery rejection consumed a chamber.");
            Assertions.Equal(before, decision.After,
                "Pre-delivery rejection mutated firearm state.");
            Assertions.False(decision.ShouldForceMiss,
                "Pre-delivery rejection became an attempted miss.");
        }

        private static void ScatterDischargeEmpty()
        {
            FirearmState before = FirearmState.CreateEmpty();
            ScatterDischargeDecision decision = new ScatterDischargeService().Evaluate(
                FirearmDefinitions.CreateEarlyBlunderbuss(), before, ScatterPlan(), true);
            Assertions.Equal(ScatterDischargeStatus.Empty, decision.Status,
                "Empty scatter status mismatch.");
            Assertions.Equal(0, decision.RoundsConsumed, "Empty scatter consumed a chamber.");
            Assertions.True(decision.ShouldForceMiss, "Empty scatter did not force a miss.");
        }

        private static void ScatterDischargeWrecked()
        {
            FirearmState before = new FirearmState(
                FirearmState.CurrentSchemaVersion, 0, null, FirearmCondition.Wrecked);
            ScatterDischargeDecision decision = new ScatterDischargeService().Evaluate(
                FirearmDefinitions.CreateEarlyBlunderbuss(), before, ScatterPlan(), true);
            Assertions.Equal(ScatterDischargeStatus.Wrecked, decision.Status,
                "Wrecked scatter status mismatch.");
            Assertions.Equal(before, decision.After, "Wrecked scatter mutated state.");
        }

        private static void ScatterDischargeNonScatterRejected()
        {
            Assertions.Throws<ArgumentException>(
                () => new ScatterDischargeService().Evaluate(
                    FirearmDefinitions.CreateEarlyMusket(),
                    LoadedState(1, FirearmCondition.Normal), ScatterPlan(), true),
                "A non-scatter firearm used scatter discharge.");
        }

        private static void AssertScatterFiredOnce(ScatterTargetPlan plan)
        {
            FirearmState before = LoadedState(1, FirearmCondition.Normal);
            ScatterDischargeDecision decision = new ScatterDischargeService().Evaluate(
                FirearmDefinitions.CreateEarlyBlunderbuss(), before, plan, true);
            Assertions.Equal(ScatterDischargeStatus.Fired, decision.Status,
                "Loaded scatter status mismatch.");
            Assertions.Equal(1, decision.RoundsConsumed,
                "Scatter target count changed chamber consumption.");
            Assertions.Equal(plan.TargetCount, decision.TargetCount,
                "Scatter discharge lost target count evidence.");
            Assertions.Equal(FirearmState.CreateEmpty(), decision.After,
                "Scatter discharge did not perform one canonical Fire transition.");
        }

        private static void ScatterExplosionTriple()
        {
            ScatterExplosionDamageDecision decision =
                new ScatterExplosionDamageService().Evaluate(
                    FirearmDefinitions.CreateEarlyBlunderbuss(), BurstExplosion(),
                    new ScatterAttackVolleyDecision(2, 0, 2, 0, 0));
            Assertions.True(decision.ShouldApply, "All-roll scatter explosion was suppressed.");
            Assertions.Equal(3, decision.BaseDamageMultiplier,
                "Scatter explosion did not triple base damage.");
        }

        private static void ScatterExplosionPartialMisfireRejected()
        {
            Assertions.Throws<InvalidOperationException>(
                () => new ScatterExplosionDamageService().Evaluate(
                    FirearmDefinitions.CreateEarlyBlunderbuss(), BurstExplosion(),
                    new ScatterAttackVolleyDecision(2, 1, 1, 0, 0)),
                "A partial-misfire scatter volley exploded.");
        }

        private static void ScatterExplosionEmptyVolleyRejected()
        {
            Assertions.Throws<InvalidOperationException>(
                () => new ScatterExplosionDamageService().Evaluate(
                    FirearmDefinitions.CreateEarlyBlunderbuss(), BurstExplosion(),
                    new ScatterAttackVolleyDecision(0, 0, 0, 0, 0)),
                "An empty volley caused a vacuous scatter explosion.");
        }

        private static void ScatterExplosionNone()
        {
            ScatterExplosionDamageDecision decision =
                new ScatterExplosionDamageService().Evaluate(
                    FirearmDefinitions.CreateEarlyBlunderbuss(), NoExplosion(), null);
            Assertions.False(decision.ShouldApply, "A non-explosion applied damage.");
            Assertions.Equal(0, decision.BaseDamageMultiplier,
                "A non-explosion retained a damage multiplier.");
        }

        private static void ScatterExplosionOrdinarySingle()
        {
            ScatterExplosionDamageDecision decision =
                new ScatterExplosionDamageService().Evaluate(
                    FirearmDefinitions.CreateEarlyMusket(), BurstExplosion(), null);
            Assertions.True(decision.ShouldApply, "Ordinary explosion was suppressed.");
            Assertions.Equal(1, decision.BaseDamageMultiplier,
                "Ordinary explosion damage was multiplied.");
        }

        private static void ScatterExplosionOrdinaryVolleyRejected()
        {
            Assertions.Throws<ArgumentException>(
                () => new ScatterExplosionDamageService().Evaluate(
                    FirearmDefinitions.CreateEarlyMusket(), BurstExplosion(),
                    new ScatterAttackVolleyDecision(1, 0, 1, 0, 0)),
                "A non-scatter explosion accepted scatter volley evidence.");
        }

        private static FirearmExplosionDecision BurstExplosion()
        {
            FirearmMisfireDecision roll = new FirearmMisfireService().Evaluate(1, 2, true);
            FirearmState broken = FirearmStateMachine.ApplyMisfireDamage(
                FirearmState.CreateEmpty());
            FirearmMisfireConditionDecision condition =
                new FirearmMisfireConditionService().Evaluate(roll, broken);
            return new FirearmExplosionService().Evaluate(condition);
        }

        private static FirearmExplosionDecision NoExplosion()
        {
            FirearmMisfireDecision roll = new FirearmMisfireService().Evaluate(3, 2, true);
            FirearmMisfireConditionDecision condition =
                new FirearmMisfireConditionService().Evaluate(
                    roll, FirearmState.CreateEmpty());
            return new FirearmExplosionService().Evaluate(condition);
        }

        private static ScatterTargetPlan ScatterPlan(params object[] targets)
        {
            var candidates = new ScatterTargetCandidate[targets.Length];
            for (int index = 0; index < targets.Length; index++)
            {
                candidates[index] = ScatterCandidate(
                    targets[index], ((char)('a' + index)).ToString(), "Target", index + 1,
                    ScatterGeometryDisposition.Inside);
            }
            return new ScatterTargetPlanService().Build(new object(), candidates);
        }

        private static ScatterAttackRollObservation ScatterRoll(
            object target, string identity, int naturalRoll, bool hit,
            bool threat, bool confirmed)
        {
            return new ScatterAttackRollObservation(
                target, identity, naturalRoll, hit, threat, confirmed);
        }

        private static ScatterTargetCandidate ScatterCandidate(
            object unit, string identity, string name, float distance,
            ScatterGeometryDisposition geometry)
        {
            return new ScatterTargetCandidate(unit, identity, name, distance, geometry);
        }

        private sealed class ValueEqualTarget
        {
            private readonly int _value;
            internal ValueEqualTarget(int value) { _value = value; }
            public override bool Equals(object obj)
            {
                ValueEqualTarget other = obj as ValueEqualTarget;
                return other != null && other._value == _value;
            }
            public override int GetHashCode() { return _value; }
        }
    }
}
