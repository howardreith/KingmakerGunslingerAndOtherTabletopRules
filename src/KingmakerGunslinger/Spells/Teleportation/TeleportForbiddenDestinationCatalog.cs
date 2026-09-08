using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace KingmakerGunslinger.Spells.Teleportation
{
    // Explicit exclusions supplement positive native state/placement checks.
    // Production entries are supplied by the versioned, curated inventory audit.
    internal sealed class TeleportForbiddenDestinationCatalog
    {
        internal TeleportForbiddenDestinationCatalog(int version, IEnumerable<KeyValuePair<string, string>> entries)
        {
            if (version < 1) throw new ArgumentOutOfRangeException("version");
            if (entries == null) throw new ArgumentNullException("entries");
            Version = version;
            var items = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (var item in entries)
            {
                if (!TeleportDestinationPolicy.IsStableId(item.Key) || string.IsNullOrWhiteSpace(item.Value))
                    throw new ArgumentException("Exact stable IDs and technical exclusion reasons are required.", "entries");
                items.Add(item.Key, item.Value);
            }
            Entries = new ReadOnlyDictionary<string, string>(items);
        }
        internal int Version { get; private set; }
        internal IReadOnlyDictionary<string, string> Entries { get; private set; }
        internal bool Contains(string id) { return id != null && Entries.ContainsKey(id); }
    }
}
