using KingmakerGunslinger.Enchantments;

namespace KingmakerGunslinger.DomainTests
{
    internal static class RareFirearmSeekingTests
    {
        internal static void ExactFailedConcealmentBypasses()
        {
            Assertions.True(SeekingConcealmentPolicy.ShouldBypass(
                false, true, true, true, true), "exact failed concealment");
        }

        internal static void NativeSuccessRemainsNative()
        {
            Assertions.False(SeekingConcealmentPolicy.ShouldBypass(
                true, true, true, true, true), "native success");
        }

        internal static void WrongCheckFailsClosed()
        {
            Assertions.False(SeekingConcealmentPolicy.ShouldBypass(
                false, true, false, true, true), "wrong nested check");
        }

        internal static void WrongItemFailsClosed()
        {
            Assertions.False(SeekingConcealmentPolicy.ShouldBypass(
                false, true, true, true, false), "wrong exact item");
        }

        internal static void MissingContextFailsClosed()
        {
            Assertions.False(SeekingConcealmentPolicy.ShouldBypass(
                false, false, false, false, true), "missing parent context");
        }
    }
}
