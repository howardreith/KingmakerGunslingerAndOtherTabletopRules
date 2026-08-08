using System;
using KingmakerGunslinger.Firearms;

namespace KingmakerGunslinger.DomainTests
{
    internal static class ArchetypeFoundationTests
    {
        internal static void HandednessCatalogExact()
        {
            Assertions.Equal(FirearmHandedness.OneHanded,
                FirearmHandednessPolicy.Require(FirearmKind.Pistol),
                "Pistol family changed.");
            Assertions.Equal(FirearmHandedness.OneHanded,
                FirearmHandednessPolicy.Require(FirearmKind.Revolver),
                "Revolver family changed.");
            Assertions.Equal(FirearmHandedness.TwoHanded,
                FirearmHandednessPolicy.Require(FirearmKind.Musket),
                "Musket family changed.");
            Assertions.Equal(FirearmHandedness.TwoHanded,
                FirearmHandednessPolicy.Require(FirearmKind.Blunderbuss),
                "Blunderbuss family changed.");
            Assertions.Equal(FirearmHandedness.TwoHanded,
                FirearmHandednessPolicy.Require(FirearmKind.Rifle),
                "Rifle family changed.");
        }

        internal static void HandednessFamilyMatching()
        {
            Assertions.True(FirearmHandednessPolicy.Matches(FirearmKind.Pistol,
                FirearmHandedness.OneHanded), "Pistol did not match one-handed scope.");
            Assertions.True(FirearmHandednessPolicy.Matches(FirearmKind.Revolver,
                FirearmHandedness.OneHanded), "Revolver did not match one-handed scope.");
            Assertions.False(FirearmHandednessPolicy.Matches(FirearmKind.Musket,
                FirearmHandedness.OneHanded), "Musket leaked into one-handed scope.");
            Assertions.True(FirearmHandednessPolicy.Matches(FirearmKind.Musket,
                FirearmHandedness.TwoHanded), "Musket did not match two-handed scope.");
            Assertions.True(FirearmHandednessPolicy.Matches(FirearmKind.Blunderbuss,
                FirearmHandedness.TwoHanded), "Blunderbuss did not match two-handed scope.");
            Assertions.True(FirearmHandednessPolicy.Matches(FirearmKind.Rifle,
                FirearmHandedness.TwoHanded), "Rifle did not match two-handed scope.");
            Assertions.False(FirearmHandednessPolicy.Matches(FirearmKind.Revolver,
                FirearmHandedness.TwoHanded), "Revolver leaked into two-handed scope.");
        }

        internal static void HandednessUnknownFailsClosed()
        {
            FirearmHandedness actual;
            Assertions.False(FirearmHandednessPolicy.TryGet(FirearmKind.Unknown,
                out actual), "Unknown kind received a family.");
            Assertions.Equal(FirearmHandedness.Unknown, actual,
                "Unknown kind returned a non-fail-closed family.");
            Assertions.False(FirearmHandednessPolicy.Matches(FirearmKind.Unknown,
                FirearmHandedness.OneHanded), "Unknown kind matched one-handed scope.");
            Assertions.False(FirearmHandednessPolicy.Matches(FirearmKind.Unknown,
                FirearmHandedness.TwoHanded), "Unknown kind matched two-handed scope.");
            Assertions.False(FirearmHandednessPolicy.Matches(FirearmKind.Pistol,
                FirearmHandedness.Unknown), "Unknown scope matched a firearm.");
            Assertions.Throws<ArgumentOutOfRangeException>(() =>
                FirearmHandednessPolicy.Require(FirearmKind.Unknown),
                "Unknown kind did not fail closed.");
            Assertions.Throws<ArgumentOutOfRangeException>(() =>
                FirearmHandednessPolicy.Require((FirearmKind)999),
                "Undefined kind did not fail closed.");
        }
    }
}
