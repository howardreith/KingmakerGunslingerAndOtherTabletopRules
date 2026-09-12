using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;

namespace KingmakerGunslinger.Firing
{
    /// <summary>
    /// Runtime bookkeeping for the newly-Broken sequence interruption. Per
    /// (wielder, exact weapon) it counts committed degradations and holds the
    /// interrupted-sequence suppression that is consumed by a genuine
    /// player-issued attack order (or becomes inert when the weapon is
    /// repaired). It also tracks the player-attack click context that
    /// distinguishes deliberate orders from automatic command recreation; the
    /// native click handler sets it around the whole click so constructions
    /// inside it are provably player-issued. All state is process-memory
    /// only and weakly keyed, so scene transitions and save/load cannot leak
    /// it onto unrelated future commands.
    /// </summary>
    internal static class BrokenSequenceSuppressionRuntime
    {
        private static readonly object Gate = new object();
        private static ConditionalWeakTable<object, Dictionary<object, Entry>>
            _suppressions = new ConditionalWeakTable<object, Dictionary<object, Entry>>();
        private const int NoFrame = -1;
        private static int _playerAttackFrame = NoFrame;

        internal sealed class Entry
        {
            internal int Epoch;
            internal bool Suppressed;
        }

        internal static void OnCommittedDegradation(object wielder, object weapon)
        {
            if (wielder == null || weapon == null)
            {
                return;
            }

            lock (Gate)
            {
                Entry entry = GetOrCreate(wielder, weapon);
                entry.Epoch = checked(entry.Epoch + 1);
                entry.Suppressed = true;
            }
        }

        internal static bool IsSuppressed(object wielder, object weapon)
        {
            lock (Gate)
            {
                Entry entry = Find(wielder, weapon);
                return entry != null && entry.Suppressed;
            }
        }

        internal static int GetDegradationEpoch(object wielder, object weapon)
        {
            lock (Gate)
            {
                Entry entry = Find(wielder, weapon);
                return entry == null ? 0 : entry.Epoch;
            }
        }

        internal static void ConsumeSuppression(object wielder, object weapon)
        {
            lock (Gate)
            {
                Entry entry = Find(wielder, weapon);
                if (entry != null)
                {
                    entry.Suppressed = false;
                }
            }
        }

        internal static bool IsPlayerAttackContext
        {
            get
            {
                int marked = Volatile.Read(ref _playerAttackFrame);
                return marked != NoFrame && marked == CurrentFrame();
            }
        }

        /// <summary>
        /// Marks the current engine frame as carrying a genuine player attack
        /// click. Frame scoping keeps the marker leak-proof: Harmony 1.2 has
        /// no finalizer, so an exception inside the click handler can never
        /// leave a persistent player-context behind.
        /// </summary>
        internal static void MarkPlayerAttackFrame()
        {
            Volatile.Write(ref _playerAttackFrame, CurrentFrame());
        }

        internal static int CurrentFrame()
        {
            return UnityEngine.Time.frameCount;
        }

        /// <summary>
        /// Guarded runtime-test seam only: resets all interruption bookkeeping.
        /// Ordinary play never calls it.
        /// </summary>
        internal static void ClearForRuntimeTest()
        {
            lock (Gate)
            {
                _suppressions =
                    new ConditionalWeakTable<object, Dictionary<object, Entry>>();
            }

            Interlocked.Exchange(ref _playerAttackFrame, NoFrame);
        }

        private static Entry Find(object wielder, object weapon)
        {
            if (wielder == null || weapon == null)
            {
                return null;
            }

            Dictionary<object, Entry> perWeapon;
            if (!_suppressions.TryGetValue(wielder, out perWeapon))
            {
                return null;
            }

            Entry entry;
            return perWeapon.TryGetValue(weapon, out entry) ? entry : null;
        }

        private static Entry GetOrCreate(object wielder, object weapon)
        {
            Dictionary<object, Entry> perWeapon;
            if (!_suppressions.TryGetValue(wielder, out perWeapon))
            {
                perWeapon = new Dictionary<object, Entry>();
                _suppressions.Add(wielder, perWeapon);
            }

            Entry entry;
            if (!perWeapon.TryGetValue(weapon, out entry))
            {
                entry = new Entry();
                perWeapon.Add(weapon, entry);
            }

            return entry;
        }
    }
}
