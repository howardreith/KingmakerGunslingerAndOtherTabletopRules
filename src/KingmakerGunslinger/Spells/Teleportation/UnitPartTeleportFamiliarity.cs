using System;
using System.Collections.Generic;
using Kingmaker.UnitLogic;
using Newtonsoft.Json;

namespace KingmakerGunslinger.Spells.Teleportation
{
    // The canonical main character owns this part in the campaign save. Loading,
    // saving and module disablement do not normalize or replace the stored payload.
    public sealed class UnitPartTeleportFamiliarity : UnitPart
    {
        [JsonProperty]
        private string _state;
        internal bool DiagnosticEmitted;
        [JsonProperty]
        private string _explorationBoundary;
        internal bool ExplorationDiagnosticEmitted;
        // One-time finite scroll-stock grant markers per vendor target. The
        // marker, never the current count, decides whether a batch applies.
        [JsonProperty]
        private List<string> _scrollVendorGrants;
        internal TeleportExplorationBoundary ReadExplorationBoundary()
        { return TeleportExplorationBoundary.Parse(_explorationBoundary); }
        internal bool SuppressExploration(string mapId, string pointId, float miles, bool ordinaryWalking)
        { return TeleportExplorationBoundary.SuppressSaved(_explorationBoundary, mapId, pointId, miles, ordinaryWalking); }
        internal void MarkMagicalArrival(TeleportExplorationBoundary boundary)
        { _explorationBoundary = boundary.Serialize(); }
        internal void ClearExplorationBoundary() { _explorationBoundary = null; }
        internal bool HasScrollVendorGrant(string target)
        { return _scrollVendorGrants != null && _scrollVendorGrants.Contains(target); }
        internal void RecordScrollVendorGrant(string target)
        {
            if (HasScrollVendorGrant(target)) return;
            if (_scrollVendorGrants == null) _scrollVendorGrants = new List<string>();
            _scrollVendorGrants.Add(target);
            _scrollVendorGrants.Sort(StringComparer.Ordinal);
        }

        internal TeleportFamiliarityState Read()
        { return _state == null ? new TeleportFamiliarityState() : TeleportFamiliarityState.Parse(_state); }

        internal void MigrateLegacy(IEnumerable<string> visitedIds)
        {
            TeleportFamiliarityState state = Read();
            if (state.LegacyMigrationComplete) return;
            state.MigrateLegacy(visitedIds);
            _state = state.Serialize();
        }

        internal void RecordOrdinaryArrivals(IEnumerable<string> pointIds)
        {
            TeleportFamiliarityState state = Read();
            foreach (string id in pointIds) state.RecordOrdinaryArrival(id);
            // Validate the entire batch before replacing the prior serialized state.
            _state = state.Serialize();
        }
    }
}
