using System;
using KingmakerGunslinger.Scatter;

namespace KingmakerGunslinger.DomainTests
{
    internal static partial class Program
    {
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
