using System;
using System.Collections.Generic;
using System.Linq;

namespace KingmakerGunslinger.Spells.Teleportation
{
    internal sealed class TeleportCanonicalUnit
    {
        internal TeleportCanonicalUnit(string id, string masterId, bool detached)
        { Id = id; MasterId = masterId; Detached = detached; }
        internal string Id { get; private set; }
        internal string MasterId { get; private set; }
        internal bool Detached { get; private set; }
    }

    internal static class TeleportTravelerRosterPolicy
    {
        // Canonical native party order is retained. Associated units follow in
        // stable order; association can pass through another traveling companion.
        // Life state is deliberately absent: dead members still travel together.
        internal static IReadOnlyList<string> Resolve(IEnumerable<string> activeParty,
            IEnumerable<TeleportCanonicalUnit> canonicalUnits)
        {
            if (activeParty == null || canonicalUnits == null) throw new ArgumentNullException("activeParty/canonicalUnits");
            string[] party = activeParty.ToArray();
            TeleportCanonicalUnit[] units = canonicalUnits.ToArray();
            if (party.Length == 0 || party.Any(string.IsNullOrWhiteSpace) ||
                party.Distinct(StringComparer.Ordinal).Count() != party.Length ||
                units.Any(value => value == null || string.IsNullOrWhiteSpace(value.Id)) ||
                units.Select(value => value.Id).Distinct(StringComparer.Ordinal).Count() != units.Length)
                throw new InvalidOperationException("Canonical traveling identities are missing or ambiguous.");
            var byId = units.ToDictionary(value => value.Id, StringComparer.Ordinal);
            if (party.Any(id => !byId.ContainsKey(id) || byId[id].Detached))
                throw new InvalidOperationException("An active party member is absent or detached in canonical state.");
            var included = new HashSet<string>(party, StringComparer.Ordinal);
            bool changed;
            do
            {
                changed = false;
                foreach (var unit in units)
                    if (!unit.Detached && unit.MasterId != null && included.Contains(unit.MasterId))
                        changed |= included.Add(unit.Id);
            } while (changed);
            return Array.AsReadOnly(party.Concat(included.Except(party, StringComparer.Ordinal)
                .OrderBy(id => id, StringComparer.Ordinal)).ToArray());
        }
    }
}
