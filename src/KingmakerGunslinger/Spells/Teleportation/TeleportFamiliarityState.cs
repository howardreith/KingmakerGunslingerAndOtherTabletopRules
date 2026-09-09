using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace KingmakerGunslinger.Spells.Teleportation
{
    // Versioned payload for a campaign-owned UnitPart, never machine-wide state.
    internal sealed class TeleportFamiliarityState
    {
        private readonly SortedDictionary<string, int> _counts =
            new SortedDictionary<string, int>(StringComparer.Ordinal);
        internal bool LegacyMigrationComplete { get; private set; }

        internal int Count(string stableId)
        {
            RequireId(stableId);
            int value;
            return _counts.TryGetValue(stableId, out value) ? value : 0;
        }

        internal void MigrateLegacy(IEnumerable<string> visitedIds)
        {
            if (visitedIds == null) throw new ArgumentNullException("visitedIds");
            if (LegacyMigrationComplete) return;
            string[] ids = visitedIds.Distinct(StringComparer.Ordinal).ToArray();
            foreach (string id in ids) RequireId(id);
            foreach (string id in ids) if (!_counts.ContainsKey(id)) _counts.Add(id, 1);
            LegacyMigrationComplete = true;
        }

        internal void RecordOrdinaryArrival(string stableId)
        {
            int before = Count(stableId);
            _counts[stableId] = before == int.MaxValue ? before : before + 1;
        }

        internal TeleportFamiliarity Familiarity(string stableId, bool isOleg, bool isEstablishedCapital)
        {
            int count = Count(stableId);
            if (isOleg || isEstablishedCapital || count >= 5) return TeleportFamiliarity.VeryFamiliar;
            if (count >= 3) return TeleportFamiliarity.StudiedCarefully;
            if (count == 2) return TeleportFamiliarity.SeenCasually;
            return count == 1 ? TeleportFamiliarity.ViewedOnce : TeleportFamiliarity.Unvisited;
        }

        internal string Serialize()
        {
            return "1|" + (LegacyMigrationComplete ? "1" : "0") +
                string.Concat(_counts.Select(pair => "|" + pair.Key + ":" +
                    pair.Value.ToString(CultureInfo.InvariantCulture)));
        }

        internal static TeleportFamiliarityState Parse(string serialized)
        {
            if (serialized == null) throw new ArgumentNullException("serialized");
            string[] fields = serialized.Split('|');
            if (fields.Length < 2 || fields[0] != "1" || (fields[1] != "0" && fields[1] != "1"))
                throw new FormatException("Unsupported teleport familiarity state.");
            var state = new TeleportFamiliarityState { LegacyMigrationComplete = fields[1] == "1" };
            foreach (string field in fields.Skip(2))
            {
                string[] pair = field.Split(':');
                int count;
                if (pair.Length != 2 || !int.TryParse(pair[1], NumberStyles.None,
                    CultureInfo.InvariantCulture, out count) || count <= 0)
                    throw new FormatException("Invalid teleport familiarity entry.");
                RequireId(pair[0]);
                if (state._counts.ContainsKey(pair[0])) throw new FormatException("Duplicate familiarity identity.");
                state._counts.Add(pair[0], count);
            }
            return state;
        }

        private static void RequireId(string stableId)
        {
            Guid guid;
            if (stableId == null || stableId.Length != 32 ||
                stableId != stableId.ToLowerInvariant() || !Guid.TryParseExact(stableId, "N", out guid) ||
                guid == Guid.Empty)
                throw new ArgumentException("An exact nonempty blueprint GUID is required.", "stableId");
        }
    }
}
