namespace KingmakerGunslinger.RuntimeTesting
{
    /// <summary>
    /// Sprint 7 review observers act only on what they acquired: a description
    /// window opens only when no foreign window, hover or modal is showing; the
    /// window is the observer's only while it shows the exact data object the
    /// observer built; and a control fact is removed only if the observer's own
    /// grant created it.
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

        internal static bool OwnsFact(bool presentBefore, bool presentAfter)
        {
            return !presentBefore && presentAfter;
        }
    }
}
