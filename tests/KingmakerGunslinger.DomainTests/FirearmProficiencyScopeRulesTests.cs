using System;
using System.Linq;
using KingmakerGunslinger.Firearms;

namespace KingmakerGunslinger.DomainTests
{
    /// <summary>
    /// RISK-05 / S4 T7: the reconciler's scope rules. No supported independent
    /// provider of the mod's firearm proficiency facts exists, so these tests
    /// fix the boundary: each presentation keeps its own scope, removes the
    /// mod's two obsolete scopes, and spares only the legacy wrapper's full
    /// proficiency.
    /// </summary>
    internal static class FirearmProficiencyScopeRulesTests
    {
        private static readonly FirearmProficiencyPresentation[] Presentations =
            (FirearmProficiencyPresentation[])Enum.GetValues(typeof(FirearmProficiencyPresentation));
        private static readonly FirearmProficiencyScope[] Scopes =
            (FirearmProficiencyScope[])Enum.GetValues(typeof(FirearmProficiencyScope));

        internal static void EachPresentationGrantsOneScope()
        {
            Assertions.Equal(FirearmProficiencyScope.Full,
                FirearmProficiencyScopeRules.Expected(FirearmProficiencyPresentation.Gunslinger), "Gunslinger scope.");
            Assertions.Equal(FirearmProficiencyScope.OneHanded,
                FirearmProficiencyScopeRules.Expected(FirearmProficiencyPresentation.Pistolero), "Pistolero scope.");
            Assertions.Equal(FirearmProficiencyScope.TwoHanded,
                FirearmProficiencyScopeRules.Expected(FirearmProficiencyPresentation.MusketMaster), "Musket Master scope.");
            foreach (FirearmProficiencyPresentation presentation in Presentations)
            {
                FirearmProficiencyScope expected = FirearmProficiencyScopeRules.Expected(presentation);
                Assertions.False(FirearmProficiencyScopeRules.Removes(presentation, expected, false),
                    presentation + " removed its own scope.");
                Assertions.Equal(2, FirearmProficiencyScopeRules.Incompatible(presentation).Length,
                    presentation + " must remove exactly the other two scopes.");
            }
        }

        internal static void ObsoleteModScopesAreReconciled()
        {
            Assertions.True(FirearmProficiencyScopeRules.Incompatible(FirearmProficiencyPresentation.Gunslinger)
                .SequenceEqual(new[] { FirearmProficiencyScope.OneHanded, FirearmProficiencyScope.TwoHanded }),
                "A base Gunslinger drops a stale scoped proficiency.");
            Assertions.True(FirearmProficiencyScopeRules.Incompatible(FirearmProficiencyPresentation.Pistolero)
                .SequenceEqual(new[] { FirearmProficiencyScope.Full, FirearmProficiencyScope.TwoHanded }),
                "A Pistolero drops full and two-handed proficiency (the registered order is preserved).");
            Assertions.True(FirearmProficiencyScopeRules.Incompatible(FirearmProficiencyPresentation.MusketMaster)
                .SequenceEqual(new[] { FirearmProficiencyScope.Full, FirearmProficiencyScope.OneHanded }),
                "A Musket Master drops full and one-handed proficiency (the registered order is preserved).");
            Assertions.True(Presentations.SelectMany(FirearmProficiencyScopeRules.Incompatible)
                .All(scope => Scopes.Contains(scope)), "Only the mod's three scopes are ever removed.");
        }

        internal static void LegacyWrapperKeepsFullProficiency()
        {
            Assertions.False(FirearmProficiencyScopeRules.Removes(FirearmProficiencyPresentation.Pistolero,
                FirearmProficiencyScope.Full, true), "A Pistolero who owns the legacy wrapper keeps full proficiency.");
            Assertions.True(FirearmProficiencyScopeRules.Removes(FirearmProficiencyPresentation.Pistolero,
                FirearmProficiencyScope.Full, false), "Without the wrapper a Pistolero's stale full proficiency goes.");
            Assertions.True(FirearmProficiencyScopeRules.Removes(FirearmProficiencyPresentation.MusketMaster,
                FirearmProficiencyScope.OneHanded, true), "The wrapper spares only full proficiency, not a stale scope.");
        }
    }
}
