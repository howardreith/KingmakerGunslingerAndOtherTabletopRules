using System;
using System.Collections.Generic;

namespace KingmakerGunslinger.Spells.ProtectionFromAlignment
{
    internal sealed class MentalControlCatalog
    {
        private readonly Dictionary<string, MentalControlCatalogEntry> _abilities =
            new Dictionary<string, MentalControlCatalogEntry>(StringComparer.Ordinal);
        private readonly Dictionary<string, MentalControlCatalogEntry> _buffs =
            new Dictionary<string, MentalControlCatalogEntry>(StringComparer.Ordinal);
        private readonly List<MentalControlCatalogEntry> _entries =
            new List<MentalControlCatalogEntry>();

        internal int AbilityCount { get { return _abilities.Count; } }
        internal int BuffCount { get { return _buffs.Count; } }
        internal IReadOnlyList<MentalControlCatalogEntry> Entries
        { get { return _entries.AsReadOnly(); } }

        internal bool Register(MentalControlCatalogEntry entry)
        {
            if (entry == null) throw new ArgumentNullException("entry");
            Dictionary<string, MentalControlCatalogEntry> target =
                entry.Kind == MentalControlBlueprintKind.Ability ?
                    _abilities : _buffs;
            MentalControlCatalogEntry existing;
            if (target.TryGetValue(entry.Guid, out existing))
            {
                if (existing.Equals(entry)) return false;
                throw new InvalidOperationException(
                    "Conflicting mental-control catalog registration for " +
                    entry.Kind + " GUID " + entry.Guid + ".");
            }
            target.Add(entry.Guid, entry);
            _entries.Add(entry);
            return true;
        }

        internal bool TryGetAbility(string guid,
            out MentalControlCatalogEntry entry)
        {
            if (guid == null) { entry = null; return false; }
            return _abilities.TryGetValue(guid, out entry);
        }

        internal bool TryGetBuff(string guid, out MentalControlCatalogEntry entry)
        {
            if (guid == null) { entry = null; return false; }
            return _buffs.TryGetValue(guid, out entry);
        }
    }
}
