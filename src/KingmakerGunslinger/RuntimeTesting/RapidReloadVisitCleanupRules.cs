using System;

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
    }
}
