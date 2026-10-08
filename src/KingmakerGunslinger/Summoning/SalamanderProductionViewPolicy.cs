using System;

namespace KingmakerGunslinger.Summoning
{
    internal static class SalamanderProductionViewPolicy
    {
        internal const string DonorGuid = "86dc43534645e234eb35431131e3b669";
        internal static bool Permits(bool enabled, string guid, string name, string prefab, string spear,
            bool polymorphed)
        {
            return enabled && !polymorphed && SalamanderRulesPolicy.IsOwner(guid, name) &&
                prefab == SalamanderTailAnimationPolicy.Prefab && spear == SalamanderTailAnimationPolicy.Spear;
        }
    }

    internal enum SalamanderViewSettlementResult { Waiting, Ready, Expired, Closed }

    // One bounded native-settlement observation, not a rebuild/retry loop.
    // Two consecutive LateUpdate frames must expose the same ready mesh.
    internal sealed class SalamanderViewSettlement
    {
        internal const int MaximumObservations = 600;
        private int _lastFrame = -1, _observations;
        private object _mesh;
        private bool _ready, _closed;

        internal SalamanderViewSettlementResult Observe(int frame, object mesh, bool ready)
        {
            if (_closed) return SalamanderViewSettlementResult.Closed;
            if (frame < 0 || frame < _lastFrame) { _closed = true; return SalamanderViewSettlementResult.Expired; }
            if (frame == _lastFrame) return SalamanderViewSettlementResult.Waiting;
            bool consecutive = frame == _lastFrame + 1 && _ready && ready && mesh != null && ReferenceEquals(mesh, _mesh);
            _lastFrame = frame; _mesh = mesh; _ready = ready && mesh != null;
            if (++_observations > MaximumObservations) { _closed = true; return SalamanderViewSettlementResult.Expired; }
            if (!consecutive) return SalamanderViewSettlementResult.Waiting;
            _closed = true;
            return SalamanderViewSettlementResult.Ready;
        }
    }
}
