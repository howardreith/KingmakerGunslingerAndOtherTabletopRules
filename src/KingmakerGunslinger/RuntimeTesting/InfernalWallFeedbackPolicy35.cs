using System;

namespace KingmakerGunslinger.RuntimeTesting
{
    /// <summary>
    /// Wall of Ice rejection-feedback acceptance. NativeCombatLog sets its
    /// last message before publication is attempted and counts an attempt
    /// for a fault as well as a success, so neither the message nor the
    /// attempt count proves the player saw it. Delivery is exactly one
    /// successful publication of the expected message with no fault.
    /// </summary>
    internal static class InfernalWallFeedbackPolicy35
    {
        internal static bool Delivered(long attemptsDelta, long publishedDelta,
            long faultsDelta, string lastMessage, string expectedMessage)
        {
            if (expectedMessage == null)
                throw new ArgumentNullException("expectedMessage");
            return publishedDelta == 1 && faultsDelta == 0 &&
                attemptsDelta == 1 &&
                String.Equals(lastMessage, expectedMessage,
                    StringComparison.Ordinal);
        }
    }
}
