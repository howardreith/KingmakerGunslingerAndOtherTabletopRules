using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace KingmakerGunslinger.RuntimeTesting
{
    // Observation bookkeeping only. No value here authorizes damage or changes
    // the existing no-damage expiry assertion.
    internal static class CrocodilianDamageAttributionPolicy
    {
        internal static bool IsExactWindow(bool ownedPair, string key, bool active, string boundary)
        { return ownedPair && key == "crocodile" && !active && boundary == "expiry"; }

        internal static bool IsExactTarget(object expected, object observed)
        { return expected != null && ReferenceEquals(expected, observed); }

        internal static string Source(bool crocodile, bool prey, bool fixture, bool nativeUnit, bool buff)
        {
            return crocodile ? "crocodilian-source" : prey ? "prey" : fixture ? "other-fixture-unit" :
                buff ? "lingering-buff" : nativeUnit ? "other-native-unit" : "unknown-native-or-direct";
        }
    }

    internal sealed class CrocodilianDamageTransition
    {
        internal readonly int Before;
        internal readonly int After;
        internal readonly string Source;
        internal readonly bool BoundaryReached;
        internal CrocodilianDamageTransition(int before, int after, string source, bool boundaryReached)
        { Before = before; After = after; Source = source; BoundaryReached = boundaryReached; }
    }

    internal sealed class CrocodilianDamageAttributionLedger
    {
        private readonly List<CrocodilianDamageTransition> _changes = new List<CrocodilianDamageTransition>();
        internal readonly ReadOnlyCollection<CrocodilianDamageTransition> Changes;
        internal int LastDamage { get; private set; }
        internal CrocodilianDamageAttributionLedger(int initial)
        { LastDamage = initial; Changes = _changes.AsReadOnly(); }

        internal void Record(int before, int after, string source, bool boundaryReached)
        {
            if (before != LastDamage)
                _changes.Add(new CrocodilianDamageTransition(LastDamage, before, "unobserved-direct-mutation", boundaryReached));
            if (before != after)
                _changes.Add(new CrocodilianDamageTransition(before, after, source ?? "unknown-native-or-direct", boundaryReached));
            LastDamage = after;
        }

        internal void Poll(int current, bool boundaryReached)
        { if (current != LastDamage) Record(LastDamage, current, "unobserved-direct-mutation", boundaryReached); }
    }
}
