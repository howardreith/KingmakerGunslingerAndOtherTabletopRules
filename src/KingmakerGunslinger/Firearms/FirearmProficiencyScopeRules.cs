using System;
using System.Linq;

namespace KingmakerGunslinger.Firearms
{
    internal enum FirearmProficiencyScope
    {
        Full = 0,
        OneHanded = 1,
        TwoHanded = 2
    }

    internal enum FirearmProficiencyPresentation
    {
        Gunslinger = 0,
        Pistolero = 1,
        MusketMaster = 2
    }

    /// <summary>
    /// RISK-05 / S4 T7: the scope rules FirearmProficiencyScopeReconciler is
    /// built from. Each class or archetype proficiency presentation grants one
    /// scope of this mod's firearm proficiency and removes the other two when
    /// it turns on, so a pending archetype change or an older save cannot leave
    /// a stale scope. The legacy Exotic Weapon Proficiency (Firearms) wrapper
    /// keeps its full proficiency.
    ///
    /// Extension boundary: the reconciler removes these three mod-owned
    /// proficiency blueprints by identity, whoever granted them, because
    /// Kingmaker 2.1.7b facts carry no general provenance the reconciler could
    /// read. No supported independent provider of them exists: they are hidden
    /// features granted only by the Gunslinger, Pistolero and Musket Master
    /// presentations and by the legacy wrapper. A future provider must either
    /// grant its own fact, which the firearm proficiency policy then has to
    /// recognize, or bring a recorded provenance for the reconciler to honour
    /// (as the legacy wrapper is honoured here). It must not grant these
    /// blueprints directly and expect them to survive on a Gunslinger.
    /// </summary>
    internal static class FirearmProficiencyScopeRules
    {
        private static readonly FirearmProficiencyScope[] AllScopes =
        {
            FirearmProficiencyScope.Full, FirearmProficiencyScope.OneHanded,
            FirearmProficiencyScope.TwoHanded
        };

        internal static FirearmProficiencyScope Expected(FirearmProficiencyPresentation presentation)
        {
            switch (presentation)
            {
                case FirearmProficiencyPresentation.Gunslinger: return FirearmProficiencyScope.Full;
                case FirearmProficiencyPresentation.Pistolero: return FirearmProficiencyScope.OneHanded;
                case FirearmProficiencyPresentation.MusketMaster: return FirearmProficiencyScope.TwoHanded;
                default: throw new ArgumentOutOfRangeException("presentation");
            }
        }

        /// <summary>The scopes a presentation removes, in a fixed order (full first, then one-, then two-handed).</summary>
        internal static FirearmProficiencyScope[] Incompatible(FirearmProficiencyPresentation presentation)
        {
            FirearmProficiencyScope expected = Expected(presentation);
            return AllScopes.Where(scope => scope != expected).ToArray();
        }

        internal static bool Removes(FirearmProficiencyPresentation presentation,
            FirearmProficiencyScope owned, bool ownsLegacyWrapper)
        {
            if (owned == FirearmProficiencyScope.Full && ownsLegacyWrapper) return false;
            return Incompatible(presentation).Contains(owned);
        }
    }
}
