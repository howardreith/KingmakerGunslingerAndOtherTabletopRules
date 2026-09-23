using System.Linq;
using KingmakerGunslinger.Firearms;
using KingmakerGunslinger.Reloading;

namespace KingmakerGunslinger.DomainTests
{
    /// <summary>
    /// Dependency-free coverage of the shared Rapid Reload gate rules that
    /// production uses to build the parent selection's prerequisites. These
    /// assertions constrain the rule set only; the registered blueprints and
    /// the native selection flow are proved by the guarded
    /// <c>disposable-rapid-reload-proficiency-gate</c> runtime scenario.
    /// </summary>
    internal static class RapidReloadPrerequisiteTests
    {
        internal static void ParentGateCoversExactlyTheOfficialCatalog()
        {
            FirearmKind[] gate = RapidReloadPrerequisiteRules.ParentGateKinds;
            Assertions.Equal(3, gate.Length,
                "The Rapid Reload parent gate changed size.");
            Assertions.True(gate.SequenceEqual(new[] { FirearmKind.Pistol,
                    FirearmKind.Musket, FirearmKind.Blunderbuss }),
                "The Rapid Reload parent gate is not the official firearm catalog.");
            Assertions.False(gate.Contains(FirearmKind.Rifle),
                "The compatibility-only Rifle kind entered the parent gate.");
            Assertions.False(gate.Contains(FirearmKind.Revolver),
                "The compatibility-only Revolver kind entered the parent gate.");
            Assertions.True(gate.All(OfficialFirearmSupport.IsOfficial),
                "A non-official firearm kind entered the parent gate.");
        }

        internal static void ParentGateIsTheDisjunctionOfItsOfficialChildren()
        {
            foreach (bool full in new[] { false, true })
                foreach (bool one in new[] { false, true })
                    foreach (bool two in new[] { false, true })
                    {
                        bool expected = RapidReloadPrerequisiteRules
                            .ParentGateKinds.Any(kind =>
                                RapidReloadPrerequisiteRules.ChildQualifies(
                                    kind, full, one, two));
                        Assertions.Equal(expected,
                            RapidReloadPrerequisiteRules.ParentQualifies(
                                full, one, two),
                            "The parent gate stopped matching its official children for full=" +
                            full + ";one=" + one + ";two=" + two + ".");
                    }
        }

        internal static void ParentGateTruthTable()
        {
            Assertions.False(RapidReloadPrerequisiteRules.ParentQualifies(
                    false, false, false),
                "A character without firearm proficiency qualified for Rapid Reload.");
            Assertions.True(RapidReloadPrerequisiteRules.ParentQualifies(
                    true, false, false),
                "Full firearm proficiency did not qualify for Rapid Reload.");
            Assertions.True(RapidReloadPrerequisiteRules.ParentQualifies(
                    false, true, false),
                "One-handed firearm proficiency did not qualify for Rapid Reload.");
            Assertions.True(RapidReloadPrerequisiteRules.ParentQualifies(
                    false, false, true),
                "Two-handed firearm proficiency did not qualify for Rapid Reload.");
            Assertions.True(RapidReloadPrerequisiteRules.ParentQualifies(
                    false, true, true),
                "Both scoped proficiencies did not qualify for Rapid Reload.");
            Assertions.True(RapidReloadPrerequisiteRules.ParentQualifies(
                    true, true, true),
                "Every proficiency source together did not qualify for Rapid Reload.");
        }

        internal static void ChildGatesStayFirearmExact()
        {
            Assertions.True(RapidReloadPrerequisiteRules.ChildQualifies(
                    FirearmKind.Pistol, false, true, false),
                "One-handed proficiency rejected the Pistol choice.");
            Assertions.False(RapidReloadPrerequisiteRules.ChildQualifies(
                    FirearmKind.Musket, false, true, false),
                "One-handed proficiency unlocked the Musket choice.");
            Assertions.False(RapidReloadPrerequisiteRules.ChildQualifies(
                    FirearmKind.Blunderbuss, false, true, false),
                "One-handed proficiency unlocked the Blunderbuss choice.");
            Assertions.True(RapidReloadPrerequisiteRules.ChildQualifies(
                    FirearmKind.Musket, false, false, true),
                "Two-handed proficiency rejected the Musket choice.");
            Assertions.True(RapidReloadPrerequisiteRules.ChildQualifies(
                    FirearmKind.Blunderbuss, false, false, true),
                "Two-handed proficiency rejected the Blunderbuss choice.");
            Assertions.False(RapidReloadPrerequisiteRules.ChildQualifies(
                    FirearmKind.Pistol, false, false, true),
                "Two-handed proficiency unlocked the Pistol choice.");
            foreach (FirearmKind kind in RapidReloadPrerequisiteRules.ParentGateKinds)
            {
                Assertions.True(RapidReloadPrerequisiteRules.ChildQualifies(
                        kind, true, false, false),
                    "Full proficiency rejected the " + kind + " choice.");
                Assertions.False(RapidReloadPrerequisiteRules.ChildQualifies(
                        kind, false, false, false),
                    "Absent proficiency admitted the " + kind + " choice.");
            }
        }
    }
}
