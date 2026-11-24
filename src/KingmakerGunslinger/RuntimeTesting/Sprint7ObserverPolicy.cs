using System;
using System.Collections.Generic;
using System.Linq;

namespace KingmakerGunslinger.RuntimeTesting
{
    /// <summary>
    /// Sprint 7 review observers act only on what they acquired: a description
    /// window opens only when no foreign window, hover or modal is showing; the
    /// window is the observer's only while it shows the exact data object the
    /// observer built; a fact, buff or effect is released only as the exact
    /// instance the observer itself created, and only while it is still that
    /// instance; every release is attempted and a failed one is reported
    /// beside the original failure, never in its place.
    /// </summary>
    internal static class Sprint7ObserverPolicy
    {
        internal static bool CanOpenWindow(bool windowOpen, bool hoverOpen, bool modalOpen)
        {
            return !windowOpen && !hoverOpen && !modalOpen;
        }

        internal static bool OwnsWindow(object shown, object opened)
        {
            return opened != null && ReferenceEquals(shown, opened);
        }

        /// <summary>Grant only when the unit does not already hold the fact.</summary>
        internal static bool ShouldGrant(bool presentBefore)
        {
            return !presentBefore;
        }

        /// <summary>
        /// Release a grant only if the observer made it and the unit still holds
        /// that exact instance (a replacement or an original is left alone).
        /// </summary>
        internal static bool ShouldRelease(object granted, object current)
        {
            return granted != null && ReferenceEquals(granted, current);
        }

        /// <summary>The present instances that are exactly ones the observer acquired.</summary>
        internal static List<T> Acquired<T>(IEnumerable<T> present, IEnumerable<T> acquired) where T : class
        {
            var mine = new List<T>(acquired ?? Enumerable.Empty<T>());
            return (present ?? Enumerable.Empty<T>()).Where(value => value != null && mine.Any(own => ReferenceEquals(own, value))).ToList();
        }

        /// <summary>Runs every release; a release that throws is recorded and the rest still run.</summary>
        internal static List<string> RunReleases(IEnumerable<KeyValuePair<string, Action>> releases)
        {
            var failures = new List<string>();
            foreach (KeyValuePair<string, Action> release in releases ?? Enumerable.Empty<KeyValuePair<string, Action>>())
            {
                try { if (release.Value != null) release.Value(); }
                catch (Exception error) { failures.Add(release.Key + ": " + error.GetType().Name + ": " + error.Message); }
            }
            return failures;
        }

        /// <summary>The failure a partly built observer reports: the original cause, with any incomplete rollback.</summary>
        internal static InvalidOperationException SetupFailure(string observer, Exception original, IList<string> rollbackFailures, int remainingOwned)
        {
            string rollback = (rollbackFailures == null || rollbackFailures.Count == 0) && remainingOwned == 0
                ? "rollback complete"
                : "rollback INCOMPLETE: " + remainingOwned + " owned item(s) remain; " + string.Join("; ", (rollbackFailures ?? new List<string>()).ToArray());
            return new InvalidOperationException(observer + " setup failed (" + rollback + "): " +
                (original == null ? "unknown" : original.GetType().Name + ": " + original.Message), original);
        }
    }
}
