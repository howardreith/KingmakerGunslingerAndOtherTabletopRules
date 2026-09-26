using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Summoning;

namespace KingmakerGunslinger.RuntimeTesting
{
    /// <summary>
    /// Measures where the Expanded Summoning feature module's boundary actually
    /// falls, by reading the eighteen native summon parents the publisher writes
    /// to.
    ///
    /// The feature-module gate this feeds used to assert only that the active
    /// flag equalled the requested one. That is a tautology: both sides come
    /// from the settings object the launcher had just written, so the assertion
    /// would have passed unchanged if the module had published its entire
    /// roster with the module turned off, or published nothing with it turned
    /// on. Nothing else in the suite covered the disabled direction either -
    /// every other summoning scenario requires the module enabled, because they
    /// are about what it publishes.
    ///
    /// The census is deliberately built from the publisher's own catalogs and
    /// its own parent list rather than from a local copy, so it cannot drift
    /// into agreeing with itself while disagreeing with what shipped.
    /// </summary>
    internal sealed partial class RuntimeTestRunner
    {
        private const string ExpandedSummoningModulePrefix = "KMG_Summoning_";
        private const string ExpandedSummoningPlacementPrefix =
            "KMG_Summoning_Ability_";
        private const string ExpandedSummoningNativeOptionPrefix =
            "KMG_Summoning_NativeOption_";
        private const string ExpandedSummoningPreservationPrefix =
            "KMG_Summoning_Native_";

        private sealed class ExpandedSummoningBoundary
        {
            internal int PublishedParents;
            internal int Placements;
            internal int NativeOptions;
            internal int Preservation;
            internal int Unclassified;
            internal int NativeVariants;
            internal bool PlacementsExact;

            internal string Describe()
            {
                return string.Format(CultureInfo.InvariantCulture,
                    "publishedParents={0};placements={1};nativeOptions={2};" +
                    "preservation={3};unclassified={4};placementsExact={5};" +
                    "nativeVariants={6}",
                    PublishedParents, Placements, NativeOptions, Preservation,
                    Unclassified, PlacementsExact, NativeVariants);
            }
        }

        /// <summary>
        /// What the eighteen parents must look like for a given module state.
        ///
        /// Every number is derived from the frozen catalogs the publisher reads,
        /// never written down here, so a deliberate roster change moves the
        /// expectation and the observation together and this check stays honest
        /// about the surface rather than about a remembered total.
        ///
        /// <paramref name="nativeVariants"/> is reported but not constrained:
        /// the untouched native variant count is a property of the base game
        /// that no single run can establish. It is carried in the string so the
        /// enabled and disabled runs of the compatibility matrix can be compared
        /// against each other, which is where that fact actually lives.
        /// </summary>
        private static string ExpandedSummoningBoundaryExpectation(
            bool published, int nativeVariants)
        {
            return string.Format(CultureInfo.InvariantCulture,
                "publishedParents={0};placements={1};nativeOptions={2};" +
                "preservation=0;unclassified=0;placementsExact=True;" +
                "nativeVariants={3}",
                published ? ExpandedSummoningPublisher.CanonicalParentGuids.Count : 0,
                published ? SummonVisibilityCatalog.PublishedLogicalPlacementCount : 0,
                published ? SummonNativeExpansionCatalog.All.Count : 0,
                nativeVariants);
        }

        private static ExpandedSummoningBoundary ObserveExpandedSummoningBoundary(
            bool expectedPublished)
        {
            var result = new ExpandedSummoningBoundary();
            int placementReferences;
            result.PlacementsExact = ExpandedSummoningPublisher
                .RequiredBasePublicationIsExact(BlueprintBootstrap.Library,
                    expectedPublished, out placementReferences);
            foreach (string guid in
                ExpandedSummoningPublisher.CanonicalParentGuids)
            {
                BlueprintAbility parent = BlueprintLibraryLookup
                    .RequireExact<BlueprintAbility>(BlueprintBootstrap.Library,
                        guid, "canonical summon parent boundary probe");
                if (ExpandedSummoningPublisher.IsPublishedExpandedParent(parent))
                    result.PublishedParents++;

                // Summon Monster I and Nature's Ally I are direct summons with
                // no variant list at all until this module publishes one, so an
                // absent component is the correct disabled reading rather than a
                // fault.
                AbilityVariants variants = (parent.ComponentsArray ??
                    Array.Empty<BlueprintComponent>())
                    .OfType<AbilityVariants>().SingleOrDefault();
                BlueprintAbility[] live = variants == null ?
                    Array.Empty<BlueprintAbility>() :
                    variants.Variants ?? Array.Empty<BlueprintAbility>();
                foreach (BlueprintAbility variant in live)
                {
                    string name = variant == null ? string.Empty : variant.name;
                    if (!name.StartsWith(ExpandedSummoningModulePrefix,
                        StringComparison.Ordinal))
                    {
                        result.NativeVariants++;
                    }
                    else if (name.StartsWith(ExpandedSummoningPlacementPrefix,
                        StringComparison.Ordinal))
                    {
                        result.Placements++;
                    }
                    else if (name.StartsWith(ExpandedSummoningNativeOptionPrefix,
                        StringComparison.Ordinal))
                    {
                        result.NativeOptions++;
                    }
                    else if (name.StartsWith(ExpandedSummoningPreservationPrefix,
                        StringComparison.Ordinal))
                    {
                        // The two tier-one preservation children exist so the
                        // original direct-summon behaviour survives being turned
                        // into a variant list; they are never themselves offered.
                        result.Preservation++;
                    }
                    else
                    {
                        result.Unclassified++;
                    }
                }
            }

            return result;
        }
    }
}
