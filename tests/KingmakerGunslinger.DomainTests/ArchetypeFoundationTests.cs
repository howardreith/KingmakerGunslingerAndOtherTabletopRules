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

        internal static void FullProficiencyPermitsAll()
        {
            foreach (FirearmKind kind in new[] { FirearmKind.Pistol,
                FirearmKind.Revolver, FirearmKind.Musket,
                FirearmKind.Blunderbuss, FirearmKind.Rifle })
                Assertions.True(FirearmProficiencyPolicy.CanUse(1, kind,
                    true, false, false), "Full proficiency rejected " + kind + ".");
        }

        internal static void OneHandedProficiencyExact()
        {
            Assertions.True(FirearmProficiencyPolicy.CanUse(1, FirearmKind.Pistol,
                false, true, false), "Pistolero scope rejected Pistol.");
            Assertions.True(FirearmProficiencyPolicy.CanUse(1, FirearmKind.Revolver,
                false, true, false), "Pistolero scope rejected Revolver.");
            foreach (FirearmKind kind in new[] { FirearmKind.Musket,
                FirearmKind.Blunderbuss, FirearmKind.Rifle })
                Assertions.False(FirearmProficiencyPolicy.CanUse(1, kind,
                    false, true, false), "Pistolero scope admitted " + kind + ".");
        }

        internal static void TwoHandedProficiencyExact()
        {
            foreach (FirearmKind kind in new[] { FirearmKind.Musket,
                FirearmKind.Blunderbuss, FirearmKind.Rifle })
                Assertions.True(FirearmProficiencyPolicy.CanUse(1, kind,
                    false, false, true), "Musket Master scope rejected " + kind + ".");
            Assertions.False(FirearmProficiencyPolicy.CanUse(1, FirearmKind.Pistol,
                false, false, true), "Musket Master scope admitted Pistol.");
            Assertions.False(FirearmProficiencyPolicy.CanUse(1, FirearmKind.Revolver,
                false, false, true), "Musket Master scope admitted Revolver.");
        }

        internal static void ProficiencyFailsClosed()
        {
            Assertions.False(FirearmProficiencyPolicy.CanUse(1, FirearmKind.Pistol,
                false, false, false), "Absent proficiency admitted a firearm.");
            Assertions.False(FirearmProficiencyPolicy.CanUse(0, FirearmKind.Pistol,
                true, true, true), "Missing marker admitted a firearm.");
            Assertions.False(FirearmProficiencyPolicy.CanUse(2, FirearmKind.Pistol,
                true, true, true), "Multiple markers admitted a firearm.");
            Assertions.False(FirearmProficiencyPolicy.CanUse(1, FirearmKind.Unknown,
                true, true, true), "Unknown kind was admitted.");
            Assertions.False(FirearmProficiencyPolicy.CanUse(1, (FirearmKind)999,
                true, true, true), "Undefined kind was admitted.");
        }

        internal static void ScopedActionAccess()
        {
            Assertions.False(FirearmProficiencyPolicy.GrantsScatter(
                FirearmHandedness.OneHanded),
                "One-handed proficiency incorrectly grants Scatter Shot.");
            Assertions.True(FirearmProficiencyPolicy.GrantsScatter(
                FirearmHandedness.TwoHanded),
                "Two-handed proficiency did not grant Scatter Shot.");
            Assertions.Throws<ArgumentOutOfRangeException>(() =>
                FirearmProficiencyPolicy.GrantsScatter(FirearmHandedness.Unknown),
                "Unknown proficiency scope did not fail closed.");
        }
    }
}
