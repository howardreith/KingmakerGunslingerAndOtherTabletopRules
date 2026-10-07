using System;
using System.Linq;

namespace KingmakerGunslinger.Summoning
{
    // Closed Salamander-only bridge. This does not select commands, targets,
    // attack rolls, reach or damage; it schedules one authored clip event.
    internal static class SalamanderTailAnimationPolicy
    {
        internal const string UnitGuid = "f8fb103168d74b4c93182437e5d2b4e4";
        internal const string PrototypeName = "KMG_Runtime_Sprint17_SalamanderHumanTail";
        internal const string Prefab = "ced3729f4b4abab4da4ef63d8489f857";
        internal const string Spear = "99394d453c6f425f84d4b92f7a8deea0";
        internal const float Duration = 1.4f;
        internal const float ActTime = .60f;
        internal static string[] TailNames
        { get { return Enumerable.Range(0, 10).Select(i => "KMG_SalamanderTail" + i.ToString("00",
            System.Globalization.CultureInfo.InvariantCulture)).ToArray(); } }

        internal static bool PermitsPrototype(bool enabled, string guid, string name,
            string prefab, string spear, bool ownedPlayer, bool ownedBones)
        {
            return enabled && guid == UnitGuid && name == PrototypeName && prefab == Prefab &&
                spear == Spear && ownedPlayer && ownedBones;
        }

        internal static bool ExactTailNames(string[] names)
        {
            return names != null && names.SequenceEqual(TailNames);
        }

        internal static bool Finite(float value)
        { return !float.IsNaN(value) && !float.IsInfinity(value); }
    }

    internal sealed class SalamanderTailPlayback
    {
        private object _handle;
        private float _lastNativeTime, _pendingTime;
        private bool _pending, _closed, _acted;
        internal int Samples { get; private set; }
        internal float LastSampleTime { get; private set; }
        internal bool Closed { get { return _closed; } }
        internal bool Acted { get { return _acted; } }

        internal SalamanderTailPlayback(object handle)
        {
            if (handle == null) throw new ArgumentNullException("handle");
            _handle = handle;
            _lastNativeTime = -1;
        }

        // A foreign callback cannot interrupt this handle. A failed sample or
        // invalid clock for this handle does fail closed, never fake an event.
        internal bool Prepare(object handle, float nativeTime, bool interrupted,
            bool exactOwnedClipReady, out float clipTime)
        {
            clipTime = 0;
            if (!ReferenceEquals(_handle, handle) || _closed) return false;
            if (interrupted || !exactOwnedClipReady ||
                !SalamanderTailAnimationPolicy.Finite(nativeTime) || nativeTime < 0 ||
                nativeTime < _lastNativeTime)
            {
                Close(handle);
                return false;
            }
            _lastNativeTime = nativeTime;
            _pendingTime = clipTime = Math.Min(nativeTime, SalamanderTailAnimationPolicy.Duration);
            _pending = true;
            return true;
        }

        // Called only after the owned legacy Animation component evaluated the
        // exact pending state. This is NOT proof of geometry/contact; the
        // guarded command observer must independently measure both.
        internal bool Sampled(object handle, float clipTime, bool evaluatedExactClip)
        {
            if (!ReferenceEquals(_handle, handle) || _closed || !_pending) return false;
            _pending = false;
            if (!evaluatedExactClip || clipTime != _pendingTime)
            {
                Close(handle);
                return false;
            }
            LastSampleTime = clipTime;
            Samples++;
            if (_acted || clipTime < SalamanderTailAnimationPolicy.ActTime) return false;
            _acted = true;
            return true;
        }

        internal bool Complete
        { get { return !_closed && Samples > 0 && LastSampleTime >= SalamanderTailAnimationPolicy.Duration; } }

        internal void Close(object handle)
        {
            if (!ReferenceEquals(_handle, handle)) return;
            _pending = false;
            _closed = true;
        }
    }
}

