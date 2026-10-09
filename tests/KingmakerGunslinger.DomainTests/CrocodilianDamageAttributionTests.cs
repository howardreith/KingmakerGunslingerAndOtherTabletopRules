using System;
using System.Linq;
using KingmakerGunslinger.RuntimeTesting;

namespace KingmakerGunslinger.DomainTests
{
    internal static class CrocodilianDamageAttributionTests
    {
        internal static void WindowAndTargetAreExact()
        {
            Assertions.True(CrocodilianDamageAttributionPolicy.IsExactWindow(true, "crocodile", false, "expiry"), "The named request-owned failing window is observed.");
            foreach (var row in new[] { new { Owned=false, Key="crocodile", Active=false, Boundary="expiry" },
                new { Owned=true, Key="dire-crocodile", Active=false, Boundary="expiry" },
                new { Owned=true, Key="crocodile", Active=true, Boundary="expiry" },
                new { Owned=true, Key="crocodile", Active=false, Boundary="dismissal" } })
                Assertions.True(!CrocodilianDamageAttributionPolicy.IsExactWindow(row.Owned, row.Key, row.Active, row.Boundary), "No broader observation scope.");
            object target=new object();
            Assertions.True(CrocodilianDamageAttributionPolicy.IsExactTarget(target, target) &&
                !CrocodilianDamageAttributionPolicy.IsExactTarget(target, new object()) &&
                !CrocodilianDamageAttributionPolicy.IsExactTarget(null, null), "Identity, not a name/id coincidence, scopes mutation hooks.");
        }

        internal static void EveryMutationIsRetainedWithoutNetting()
        {
            var ledger=new CrocodilianDamageAttributionLedger(0);
            ledger.Record(0, 2, "other-fixture-unit", false);
            ledger.Record(2, 0, "prey", true);
            ledger.Record(0, 2, "crocodilian-source", true);
            Assertions.Equal(3, ledger.Changes.Count, "A heal and same-frame damage cannot hide earlier events in an aggregate comparison.");
            Assertions.True(ledger.Changes[0].Before == 0 && ledger.Changes[0].After == 2 &&
                ledger.Changes[2].Source == "crocodilian-source" && ledger.Changes[2].BoundaryReached,
                "Source and boundary are evidence, not permission to waive an event.");
            Assertions.Equal(2, ledger.LastDamage, "Bookkeeping changes no supplied engine value.");
            bool readOnly=false;
            try { ((System.Collections.Generic.ICollection<CrocodilianDamageTransition>)ledger.Changes).Clear(); } catch (NotSupportedException) { readOnly=true; }
            Assertions.True(readOnly, "Published transition snapshots cannot be rewritten.");
        }

        internal static void DirectAndMissedMutationRemainUnknown()
        {
            var ledger=new CrocodilianDamageAttributionLedger(0);
            ledger.Poll(2, true);
            ledger.Record(3, 4, "other-native-unit", true);
            Assertions.True(ledger.Changes.Count == 3 && ledger.Changes.Take(2).All(value => value.Source == "unobserved-direct-mutation"),
                "Neither polling nor a gap can invent a RuleDealDamage source.");
            ledger.Record(4, 4, "crocodilian-source", true);
            Assertions.Equal(3, ledger.Changes.Count, "A zero-delta event does not duplicate damage.");
        }

        internal static void CrocodilianOwnershipTakesPrecedence()
        {
            Assertions.Equal("crocodilian-source", CrocodilianDamageAttributionPolicy.Source(true, true, true, true, true), "A crocodile-owned buff must not be mislabeled unrelated interference.");
            Assertions.Equal("prey", CrocodilianDamageAttributionPolicy.Source(false, true, true, true, false), "Prey is distinct from other fixture actors.");
            Assertions.Equal("other-fixture-unit", CrocodilianDamageAttributionPolicy.Source(false, false, true, true, false), "Fixture identity is explicit.");
            Assertions.Equal("lingering-buff", CrocodilianDamageAttributionPolicy.Source(false, false, false, false, true), "Buff source remains visible.");
            Assertions.Equal("unknown-native-or-direct", CrocodilianDamageAttributionPolicy.Source(false, false, false, false, false), "Unknown evidence is not recategorized as legitimate.");
        }
    }
}
