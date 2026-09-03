using System;
using System.Collections.Generic;
using KingmakerGunslinger.ElementalRaces.Visuals;

namespace KingmakerGunslinger.DomainTests
{
    internal static class ElementalVisualResourceRollbackPolicyTests
    {
        private sealed class Registration
        {
            internal Registration(string id)
            {
                Id = id;
            }

            internal string Id { get; private set; }
        }

        internal static void ForeignReplacementRefusesBeforeAnyRemoval()
        {
            var first = new Registration("first");
            var conflicted = new Registration("conflicted");
            var last = new Registration("last");
            var order = new[] { first, conflicted, last };
            var current = new Dictionary<Registration, object>
            {
                { first, first },
                { conflicted, new object() },
                { last, last }
            };
            var removed = new List<Registration>();

            InvalidOperationException exception =
                Assertions.Throws<InvalidOperationException>(() =>
                {
                    Registration[] plan =
                        ElementalVisualResourceRollbackPolicy.CreateRemovalPlan(
                            order, value => current.ContainsKey(value),
                            value => ReferenceEquals(current[value], value),
                            value => value.Id);
                    foreach (Registration value in plan)
                    {
                        current.Remove(value);
                        removed.Add(value);
                    }
                }, "A foreign replacement must fail the entire visual rollback.");

            Assertions.True(exception.Message.Contains("conflicted"),
                "The ownership conflict must identify the exact resource.");
            Assertions.Equal(0, removed.Count,
                "Ownership preflight must occur before the first removal.");
            Assertions.Equal(3, current.Count,
                "A refused rollback must leave every cache entry intact.");
        }

        internal static void OwnedPlanIsReverseOrderedAndSkipsAbsentEntries()
        {
            var first = new Registration("first");
            var absent = new Registration("absent");
            var third = new Registration("third");
            var last = new Registration("last");
            var order = new[] { first, absent, third, last };
            var current = new Dictionary<Registration, object>
            {
                { first, first },
                { third, third },
                { last, last }
            };

            Registration[] plan =
                ElementalVisualResourceRollbackPolicy.CreateRemovalPlan(
                    order, value => current.ContainsKey(value),
                    value => ReferenceEquals(current[value], value),
                    value => value.Id);

            Assertions.Equal(3, plan.Length,
                "Only present owned resources belong in the removal plan.");
            Assertions.True(ReferenceEquals(last, plan[0]) &&
                ReferenceEquals(third, plan[1]) &&
                ReferenceEquals(first, plan[2]),
                "Owned resources must roll back in exact reverse registration order.");
            Assertions.Equal(3, current.Count,
                "Planning must not mutate the cache.");
        }
    }
}
