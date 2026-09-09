using System;
using KingmakerGunslinger.Spells.Teleportation;

namespace KingmakerGunslinger.DomainTests
{
    internal static class TeleportationTravelerTests
    {
        private static TeleportCanonicalUnit Unit(string id, string master = null, bool detached = false)
        { return new TeleportCanonicalUnit(id, master, detached); }
        internal static void ActivePartyOrderIsPreserved()
        {
            var result = TeleportTravelerRosterPolicy.Resolve(new[] { "b", "a" }, new[] { Unit("a"), Unit("b"), Unit("inactive") });
            Assertions.Equal("b,a", string.Join(",", result), "Inactive companions are excluded and active party order is retained.");
        }
        internal static void AssociatedUnitsTravelWithTheirOwners()
        {
            var result = TeleportTravelerRosterPolicy.Resolve(new[] { "owner" },
                new[] { Unit("owner"), Unit("pet", "owner"), Unit("mount", "owner"), Unit("familiar", "owner") });
            Assertions.Equal("owner,familiar,mount,pet", string.Join(",", result), "All native associated unit types are included in stable order.");
        }
        internal static void ChainedAssociationDoesNotDependOnEnumerationOrder()
        {
            var result = TeleportTravelerRosterPolicy.Resolve(new[] { "owner" },
                new[] { Unit("familiar", "pet"), Unit("pet", "owner"), Unit("owner") });
            Assertions.Equal("owner,familiar,pet", string.Join(",", result), "Canonical associations resolve transitively.");
        }
        internal static void DetachedAndInactiveAssociationsAreExcluded()
        {
            var result = TeleportTravelerRosterPolicy.Resolve(new[] { "owner" },
                new[] { Unit("owner"), Unit("inactive"), Unit("inactive-pet", "inactive"),
                    Unit("detached", "owner", true), Unit("child", "detached") });
            Assertions.Equal("owner", string.Join(",", result), "Detached units and units of inactive companions are not traveling.");
        }
        internal static void MissingActiveCanonicalUnitFailsClosed()
        {
            Reject(() => TeleportTravelerRosterPolicy.Resolve(new[] { "owner" }, new[] { Unit("other") }));
            Reject(() => TeleportTravelerRosterPolicy.Resolve(new[] { "owner" }, new[] { Unit("owner", null, true) }));
        }
        internal static void AmbiguousCanonicalIdentityFailsClosed()
        {
            Reject(() => TeleportTravelerRosterPolicy.Resolve(new[] { "owner", "owner" }, new[] { Unit("owner") }));
            Reject(() => TeleportTravelerRosterPolicy.Resolve(new[] { "owner" }, new[] { Unit("owner"), Unit("owner") }));
            Reject(() => TeleportTravelerRosterPolicy.Resolve(new[] { "owner" }, new[] { Unit("owner"), Unit(null) }));
        }
        internal static void UnrelatedCyclesCannotCreateTravelers()
        {
            var result = TeleportTravelerRosterPolicy.Resolve(new[] { "owner" },
                new[] { Unit("owner"), Unit("x", "y"), Unit("y", "x"), Unit("missing", "absent") });
            Assertions.Equal("owner", string.Join(",", result), "Unrelated invalid associations cannot be promoted into the canonical party.");
        }
        private static void Reject(Action action)
        {
            bool rejected = false;
            try { action(); } catch (InvalidOperationException) { rejected = true; }
            Assertions.True(rejected, "Unproven canonical roster fails before a cast resource can be spent.");
        }
    }
}
