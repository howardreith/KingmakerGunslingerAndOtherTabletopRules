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

        /// <summary>
        /// A held observer resource is released only when every release ran
        /// without failure and none of the observer's own hooks remains.
        /// </summary>
        internal static bool Released(ICollection<string> failures, int remainingOwned)
        {
            return (failures == null || failures.Count == 0) && remainingOwned == 0;
        }

        /// <summary>
        /// R1-F3: a gameplay-distance party capture counts only when the shared
        /// helper wrote an image (png named, not &lt;none&gt;), the unit was in frame,
        /// the screen was lit with the native fade cleared, the helper's renderer
        /// is enabled and carries the expected body mesh, and the body is intact.
        /// </summary>
        internal static bool PartyCaptureValid(string readback, string expectedMesh, out string reason)
        {
            var fields = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (string part in (readback ?? string.Empty).Split(';'))
            {
                int equals = part.IndexOf('=');
                if (equals > 0 && !fields.ContainsKey(part.Substring(0, equals))) fields[part.Substring(0, equals)] = part.Substring(equals + 1);
            }
            Func<string, string> field = name => fields.ContainsKey(name) ? fields[name] : null;
            double fade;
            if (field("png") == null || field("png") == "<none>") reason = "no-image" + (field("reason") == null ? string.Empty : ":" + field("reason"));
            else if (field("inFrame") != "true") reason = "out-of-frame";
            else if (field("screenLit") != "true") reason = "unlit";
            else if (!double.TryParse(field("screenFade"), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out fade) || fade > .05) reason = "screen-fade";
            else if (field("rendererEnabled") != "true") reason = "renderer-disabled";
            else if (string.IsNullOrEmpty(expectedMesh) || field("mesh") != expectedMesh) reason = "wrong-body";
            else if (field("intact") != "true") reason = "dissolved";
            else reason = null;
            return reason == null;
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
    /// <summary>
    /// R3-F3: the conditions a cell's own executions introduced. Instances
    /// already present when the cell began are never acquired; an instance is
    /// acquired when it is seen with an owned origin (for Dust: a Dust breath
    /// execution by one of the cell's own Dust units), however its cast ended,
    /// and release removes exactly the acquired instances still present.
    /// </summary>
    internal sealed class AcquisitionLedger<T> where T : class
    {
        private readonly List<T> _preexisting;
        internal readonly List<T> Acquired = new List<T>();

        /// <summary>How many acquired instances the last release removed.</summary>
        internal int LastRemoved { get; private set; }

        internal AcquisitionLedger(IEnumerable<T> present)
        {
            _preexisting = new List<T>(present ?? Enumerable.Empty<T>());
        }

        private bool Contains(List<T> list, T value) { return list.Any(item => ReferenceEquals(item, value)); }

        /// <summary>Acquires every present instance with an owned origin that was not there at the start; returns how many were new.</summary>
        internal int Observe(IEnumerable<T> present, Func<T, bool> ownedOrigin)
        {
            int added = 0;
            foreach (T value in (present ?? Enumerable.Empty<T>()).ToList())
                if (value != null && !Contains(_preexisting, value) && !Contains(Acquired, value) && ownedOrigin(value)) { Acquired.Add(value); added++; }
            return added;
        }

        /// <summary>Observes once more, then removes each acquired instance still present; every removal is attempted.</summary>
        internal List<string> Release(IEnumerable<T> present, Func<T, bool> ownedOrigin, Action<T> remove)
        {
            List<T> now = (present ?? Enumerable.Empty<T>()).ToList();
            Observe(now, ownedOrigin);
            var failures = new List<string>();
            LastRemoved = 0;
            foreach (T value in now.Where(item => item != null && Contains(Acquired, item)))
            {
                try { remove(value); LastRemoved++; }
                catch (Exception error) { failures.Add(error.GetType().Name + ": " + error.Message); }
            }
            return failures;
        }

        /// <summary>Present instances with an owned origin that were not there at the start.</summary>
        internal int Remaining(IEnumerable<T> present, Func<T, bool> ownedOrigin)
        {
            return (present ?? Enumerable.Empty<T>()).Count(value => value != null && !Contains(_preexisting, value) && ownedOrigin(value));
        }
    }

}
