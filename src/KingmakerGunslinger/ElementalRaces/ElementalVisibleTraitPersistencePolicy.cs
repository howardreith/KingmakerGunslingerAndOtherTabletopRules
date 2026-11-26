using System;
using System.Collections.Generic;
using System.Linq;

namespace KingmakerGunslinger.ElementalRaces
{
    internal static class ElementalVisibleTraitPersistencePolicy
    {
        internal const string MatrixId = "release-c-visible-nineteen-traits-v5";
        internal static ElementalAlternateTraitId[] Traits(ElementalHeritageRace race, int genderIndex, int heritageIndex, bool nereidQualification = false)
        {
            if (genderIndex < 0 || genderIndex > 1) throw new ArgumentOutOfRangeException("genderIndex");
            if (heritageIndex < 0 || heritageIndex > 2) throw new ArgumentOutOfRangeException("heritageIndex");
            if (nereidQualification && race == ElementalHeritageRace.Undine)
                return new[] { ElementalAlternateTraitId.NereidFascination };
            int row = genderIndex * 3 + heritageIndex;
            switch (race)
            {
                case ElementalHeritageRace.Ifrit:
                    switch (row)
                    {
                        case 0: return new[] { ElementalAlternateTraitId.WildfireHeart, ElementalAlternateTraitId.FireInTheBlood, ElementalAlternateTraitId.EfreetiMagic };
                        case 1: return new[] { ElementalAlternateTraitId.BrazenFlame, ElementalAlternateTraitId.FireInsight };
                        case 2: return new[] { ElementalAlternateTraitId.WildfireHeart, ElementalAlternateTraitId.FireInTheBlood, ElementalAlternateTraitId.ForgeHardened };
                        case 3: return new[] { ElementalAlternateTraitId.BrazenFlame, ElementalAlternateTraitId.FireInTheBlood };
                        case 4: return new[] { ElementalAlternateTraitId.WildfireHeart, ElementalAlternateTraitId.FireInsight, ElementalAlternateTraitId.EfreetiMagic };
                        case 5: return new[] { ElementalAlternateTraitId.FireInsight, ElementalAlternateTraitId.ForgeHardened };
                    }
                    break;
                case ElementalHeritageRace.Oread:
                    switch (row)
                    {
                        case 0: return new[] { ElementalAlternateTraitId.GraniteSkin, ElementalAlternateTraitId.StoneInTheBlood };
                        case 1: return new[] { ElementalAlternateTraitId.GraniteSkin, ElementalAlternateTraitId.EarthInsight };
                        case 2: return new[] { ElementalAlternateTraitId.GraniteSkin, ElementalAlternateTraitId.CrystallineForm };
                        case 3: return new[] { ElementalAlternateTraitId.StoneInTheBlood };
                        case 4: return new[] { ElementalAlternateTraitId.GraniteSkin, ElementalAlternateTraitId.EarthInsight };
                        case 5: return new[] { ElementalAlternateTraitId.GraniteSkin, ElementalAlternateTraitId.CrystallineForm };
                    }
                    break;
                case ElementalHeritageRace.Sylph:
                    switch (row)
                    {
                        case 0: return new[] { ElementalAlternateTraitId.LikeTheWind, ElementalAlternateTraitId.StormInTheBlood, ElementalAlternateTraitId.WhisperingWind };
                        case 1: return new[] { ElementalAlternateTraitId.ThunderousResilience, ElementalAlternateTraitId.BreezeKissed, ElementalAlternateTraitId.WhisperingWind };
                        case 2: return new[] { ElementalAlternateTraitId.Secretive, ElementalAlternateTraitId.AirInsight };
                        case 3: return new[] { ElementalAlternateTraitId.ThunderousResilience, ElementalAlternateTraitId.StormInTheBlood, ElementalAlternateTraitId.WhisperingWind };
                        case 4: return new[] { ElementalAlternateTraitId.LikeTheWind, ElementalAlternateTraitId.BreezeKissed, ElementalAlternateTraitId.WhisperingWind };
                        case 5: return new[] { ElementalAlternateTraitId.Secretive, ElementalAlternateTraitId.AirInsight };
                    }
                    break;
                case ElementalHeritageRace.Undine:
                    switch (row)
                    {
                        case 0: return new[] { ElementalAlternateTraitId.AcidBreath };
                        case 1: return new[] { ElementalAlternateTraitId.OozeBreath };
                        case 2: return new[] { ElementalAlternateTraitId.AcidBreath };
                        case 3: return new[] { ElementalAlternateTraitId.OozeBreath };
                        case 4: return new[] { ElementalAlternateTraitId.AcidBreath };
                        case 5: return new[] { ElementalAlternateTraitId.OozeBreath };
                    }
                    break;
            }
            throw new ArgumentOutOfRangeException("race");
        }
    }

    /// <summary>
    /// Which published alternate traits each elemental persistence scope's
    /// lifecycle matrix must exercise, declared independently of the fixture
    /// rows and of the evidence. A published trait outside a scope's matrix must
    /// name the route that covers it instead, so no published trait can fall out
    /// of every coverage contract.
    /// </summary>
    internal static class ElementalPersistenceTraitCoverage
    {
        // The ordinary 24-fixture matrix (ElementalVisibleTraitPersistencePolicy.MatrixId).
        private static readonly ElementalAlternateTraitId[] OrdinaryMatrix =
        {
            ElementalAlternateTraitId.WildfireHeart, ElementalAlternateTraitId.BrazenFlame,
            ElementalAlternateTraitId.FireInTheBlood, ElementalAlternateTraitId.EfreetiMagic,
            ElementalAlternateTraitId.ForgeHardened, ElementalAlternateTraitId.FireInsight,
            ElementalAlternateTraitId.CrystallineForm, ElementalAlternateTraitId.EarthInsight,
            ElementalAlternateTraitId.GraniteSkin, ElementalAlternateTraitId.StoneInTheBlood,
            ElementalAlternateTraitId.AirInsight, ElementalAlternateTraitId.BreezeKissed,
            ElementalAlternateTraitId.LikeTheWind, ElementalAlternateTraitId.Secretive,
            ElementalAlternateTraitId.StormInTheBlood, ElementalAlternateTraitId.ThunderousResilience,
            ElementalAlternateTraitId.WhisperingWind, ElementalAlternateTraitId.AcidBreath,
            ElementalAlternateTraitId.OozeBreath
        };

        // The qualificationTrait=NereidFascination scope: the six Undine rows carry
        // Nereid Fascination instead of the two breath replacements.
        private static readonly ElementalAlternateTraitId[] NereidMatrix =
        {
            ElementalAlternateTraitId.WildfireHeart, ElementalAlternateTraitId.BrazenFlame,
            ElementalAlternateTraitId.FireInTheBlood, ElementalAlternateTraitId.EfreetiMagic,
            ElementalAlternateTraitId.ForgeHardened, ElementalAlternateTraitId.FireInsight,
            ElementalAlternateTraitId.CrystallineForm, ElementalAlternateTraitId.EarthInsight,
            ElementalAlternateTraitId.GraniteSkin, ElementalAlternateTraitId.StoneInTheBlood,
            ElementalAlternateTraitId.AirInsight, ElementalAlternateTraitId.BreezeKissed,
            ElementalAlternateTraitId.LikeTheWind, ElementalAlternateTraitId.Secretive,
            ElementalAlternateTraitId.StormInTheBlood, ElementalAlternateTraitId.ThunderousResilience,
            ElementalAlternateTraitId.WhisperingWind, ElementalAlternateTraitId.NereidFascination
        };

        internal const string NereidRoute =
            "elemental race persistence with qualificationTrait=NereidFascination (six Undine rows)";
        internal const string OrdinaryRoute =
            "ordinary elemental race persistence matrix (six Undine breath rows)";
        internal const string TreacherousRoute =
            "disposable-elemental-treacherous-earth mechanics and the qualificationEffect=TreacherousEarth " +
            "fixed-area effect persistence probe; Player-selection/save persistence explicitly waived";

        internal static ElementalAlternateTraitId[] RequiredLifecycleTraits(bool nereidQualification)
        {
            return (nereidQualification ? NereidMatrix : OrdinaryMatrix).ToArray();
        }

        /// <summary>Published traits this scope's matrix does not exercise, with the route covering each.</summary>
        internal static IDictionary<ElementalAlternateTraitId, string> CoveredElsewhere(bool nereidQualification)
        {
            var result = new Dictionary<ElementalAlternateTraitId, string>();
            if (nereidQualification)
            {
                result[ElementalAlternateTraitId.AcidBreath] = OrdinaryRoute;
                result[ElementalAlternateTraitId.OozeBreath] = OrdinaryRoute;
            }
            else result[ElementalAlternateTraitId.NereidFascination] = NereidRoute;
            result[ElementalAlternateTraitId.TreacherousEarth] = TreacherousRoute;
            return result;
        }

        internal static ElementalAlternateTraitId[] Published()
        {
            return Enum.GetValues(typeof(ElementalAlternateTraitId)).Cast<ElementalAlternateTraitId>()
                .Where(ElementalAlternateTraitPolicy.IsPublished).OrderBy(value => value).ToArray();
        }

        /// <summary>
        /// Null when the scope's required matrix and its covered-elsewhere routes
        /// partition the published catalog exactly; otherwise the reason.
        /// </summary>
        internal static string ContractFailure(IEnumerable<ElementalAlternateTraitId> published,
            IEnumerable<ElementalAlternateTraitId> required, IEnumerable<ElementalAlternateTraitId> elsewhere)
        {
            var catalog = new HashSet<ElementalAlternateTraitId>(published);
            var matrix = new HashSet<ElementalAlternateTraitId>(required);
            var routed = new HashSet<ElementalAlternateTraitId>(elsewhere);
            var overlap = matrix.Intersect(routed).ToArray();
            var unpublished = matrix.Union(routed).Where(value => !catalog.Contains(value)).ToArray();
            var uncovered = catalog.Where(value => !matrix.Contains(value) && !routed.Contains(value)).ToArray();
            if (overlap.Length != 0) return "traits both required and routed elsewhere: " + string.Join(",", overlap);
            if (unpublished.Length != 0) return "unpublished traits in the coverage contract: " + string.Join(",", unpublished);
            if (uncovered.Length != 0) return "published traits with no coverage contract: " + string.Join(",", uncovered);
            return null;
        }

        /// <summary>
        /// Null when the lifecycle evidence is complete and exact for this scope:
        /// one row per fixture and phase in phase order, every row individually
        /// exact, and the union of row traits equal to the scope's declared
        /// required traits (not derived from the rows). Otherwise the reason.
        /// </summary>
        internal static string LifecycleFailure(bool nereidQualification, IList<string> fixtures,
            IList<string> phases, IList<LifecycleCoverageRow> rows)
        {
            if (fixtures == null || phases == null || rows == null || fixtures.Count == 0 || phases.Count == 0)
                return "fixtures, phases and rows are required";
            string contract = ContractFailure(Published(), RequiredLifecycleTraits(nereidQualification),
                CoveredElsewhere(nereidQualification).Keys);
            if (contract != null) return "coverage contract: " + contract;
            if (rows.Count != fixtures.Count * phases.Count)
                return "expected " + fixtures.Count * phases.Count + " rows, observed " + rows.Count;
            foreach (LifecycleCoverageRow row in rows)
                if (row == null || !row.Exact)
                    return "inexact observation: " + (row == null ? "<null>" : row.Fixture + "/" + row.Phase);
            foreach (string fixture in fixtures)
            {
                string[] observed = rows.Where(row => row.Fixture == fixture).Select(row => row.Phase).ToArray();
                if (!observed.SequenceEqual(phases))
                    return "fixture " + fixture + " phases [" + string.Join(",", observed) + "]";
            }
            string[] expected = RequiredLifecycleTraits(nereidQualification).Select(value => value.ToString())
                .OrderBy(value => value, StringComparer.Ordinal).ToArray();
            string[] seen = rows.SelectMany(row => row.Traits ?? new string[0]).Distinct()
                .OrderBy(value => value, StringComparer.Ordinal).ToArray();
            string[] missing = expected.Except(seen).ToArray();
            string[] unexpected = seen.Except(expected).ToArray();
            if (missing.Length != 0 || unexpected.Length != 0)
                return "trait coverage missing [" + string.Join(",", missing) + "] unexpected [" +
                    string.Join(",", unexpected) + "]";
            return null;
        }
    }

    internal sealed class LifecycleCoverageRow
    {
        internal LifecycleCoverageRow(string fixture, string phase, bool exact, IEnumerable<string> traits)
        {
            Fixture = fixture;
            Phase = phase;
            Exact = exact;
            Traits = traits == null ? null : traits.ToArray();
        }

        internal string Fixture { get; private set; }
        internal string Phase { get; private set; }
        internal bool Exact { get; private set; }
        internal string[] Traits { get; private set; }
    }
}
