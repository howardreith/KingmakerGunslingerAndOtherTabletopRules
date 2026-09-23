using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.RuntimeTesting
{
    /// <summary>
    /// Decides what a failed Rapid Reload gate visit initialization throws once
    /// the helper has tried to cancel the level-up controller it created. The
    /// original setup failure is always preserved; a cleanup failure is reported
    /// alongside it and can never be discarded, replace it, or turn a failed
    /// initialization into a success. The production failure path calls this,
    /// and the domain suite exercises the same function.
    /// </summary>
    internal static class RapidReloadVisitCleanupRules
    {
        internal const string CombinedFailureMessage =
            "Rapid Reload gate visit initialization failed and the native " +
            "level-up controller could not be cancelled.";

        /// <summary>
        /// Returns the exception to throw when cancellation also failed, or
        /// <c>null</c> when the caller should rethrow the original setup failure
        /// so its stack and diagnostics survive untouched.
        /// </summary>
        internal static Exception Compose(Exception setupError,
            Exception cleanupError)
        {
            if (setupError == null)
                throw new ArgumentNullException("setupError",
                    "A failed visit initialization must carry its original error.");
            if (cleanupError == null) return null;
            return new AggregateException(CombinedFailureMessage, setupError,
                cleanupError);
        }

        internal const string CleanupFailurePrefix = ":controller-cleanup-failed:";

        /// <summary>
        /// R6: the reporting half of the caller-side cleanup boundary, kept
        /// apart from the native <c>Cancel()</c> call so a cancellation failure
        /// on a successfully initialised controller reaches the collection that
        /// actually decides the assertion instead of only being described on the
        /// evidence row. Returns <c>true</c> when cleanup was clean; a null
        /// cleanup error is a no-op. It never throws, so a caller can invoke it
        /// from a <c>finally</c> that must still dispose its fixture unit.
        /// </summary>
        internal static bool Report(Exception cleanupError, JObject row,
            IList<string> failures, string label)
        {
            if (cleanupError == null) return true;
            if (row != null)
            {
                row["cleanupError"] = cleanupError.Message;
                row["cleanupErrorDetail"] = cleanupError.ToString();
            }
            if (failures != null)
                failures.Add((string.IsNullOrEmpty(label) ? "visit" : label) +
                    CleanupFailurePrefix + cleanupError.Message);
            return false;
        }
    }
}
