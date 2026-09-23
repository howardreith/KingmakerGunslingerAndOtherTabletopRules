using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using KingmakerGunslinger.Summoning;

namespace KingmakerGunslinger.DomainTests
{
    /// <summary>
    /// Sprint 1 of the Expanded Summoning charter represents the whole approved
    /// target roster deterministically without exposing unfinished creatures.
    /// </summary>
    internal static class ExpandedSummoningIdealRosterTests
    {
        internal static void ManifestMatchesCharterTargets()
        {
            ExpandedSummoningIdealRosterCatalog.Validate();
            Assertions.Equal(145, ExpandedSummoningIdealRosterCatalog.All.Count,
                "The ideal roster must hold 145 unique creatures.");
            Assertions.Equal(120, ExpandedSummoningIdealRosterCatalog.BaseEntries(
                SummonFamily.Monster), "Summon Monster base entries must be 120.");
            Assertions.Equal(110, ExpandedSummoningIdealRosterCatalog.BaseEntries(
                SummonFamily.NaturesAlly), "Summon Nature's Ally base entries must be 110.");
            Assertions.Equal(5, ExpandedSummoningIdealRosterCatalog.VariantElementalFamilies,
                "The stretch phase adds five variant elemental families.");
            Assertions.Equal(50, ExpandedSummoningIdealRosterCatalog.All
                .Count(value => value.CharterBaselineComplete),
                "Charter Appendix B lists 50 already-complete creatures.");
            Assertions.Equal(95, ExpandedSummoningIdealRosterCatalog.All
                .Count(value => !value.CharterBaselineComplete),
                "Charter Appendix A assigns 95 creatures to sprints.");
        }

        /// <summary>
        /// The charter publishes 1,232 core placements. Deriving that from the
        /// manifest independently is the arithmetic the sprint is gated on.
        /// </summary>
        internal static void ProjectedPlacementsReconcileToCharter()
        {
            int monster = ExpandedSummoningIdealRosterCatalog.TotalPlacements(
                SummonFamily.Monster);
            int naturesAlly = ExpandedSummoningIdealRosterCatalog.TotalPlacements(
                SummonFamily.NaturesAlly);
            Assertions.Equal(624, monster, "Projected Summon Monster placements changed.");
            Assertions.Equal(608, naturesAlly,
                "Projected Summon Nature's Ally placements changed.");
            Assertions.Equal(1232, monster + naturesAlly,
                "The charter's 1,232 core placement expectation is not reproduced.");

            // The aggregate is never one menu. The gate is the worst single
            // parent, which is the ninth-level spell in each family.
            Assertions.Equal(120, ExpandedSummoningIdealRosterCatalog
                .Placements(SummonFamily.Monster, 9).Count,
                "Summon Monster IX is the largest projected Monster menu.");
            Assertions.Equal(110, ExpandedSummoningIdealRosterCatalog
                .Placements(SummonFamily.NaturesAlly, 9).Count,
                "Summon Nature's Ally IX is the largest projected Ally menu.");
            Assertions.Equal(7, ExpandedSummoningIdealRosterCatalog
                .Placements(SummonFamily.Monster, 1).Count,
                "Summon Monster I offers only its own-tier creatures.");
        }

        /// <summary>
        /// Quantity semantics must match the shipped catalog's rules exactly so
        /// the projection is comparable with the live surface.
        /// </summary>
        internal static void QuantitySemanticsMatchShippedRules()
        {
            foreach (SummonFamily family in new[] {
                SummonFamily.Monster, SummonFamily.NaturesAlly })
            {
                for (int parent = 1; parent <= 9; parent++)
                {
                    foreach (IdealRosterEntry entry in
                        ExpandedSummoningIdealRosterCatalog.Placements(family, parent))
                    {
                        int source = entry.Tier(family).Value;
                        SummonMultiplicity actual = ExpandedSummoningIdealRosterCatalog
                            .Multiplicity(source, parent);
                        SummonMultiplicity expected = source == parent
                            ? SummonMultiplicity.One
                            : source == parent - 1
                                ? SummonMultiplicity.OneD3
                                : SummonMultiplicity.OneD4PlusOne;
                        Assertions.Equal(expected, actual,
                            "Quantity mapping changed for " + entry.Key +
                            " under parent " + parent + ".");
                    }
                }
            }

            Assertions.Throws<ArgumentOutOfRangeException>(
                () => ExpandedSummoningIdealRosterCatalog.Multiplicity(5, 4),
                "A creature cannot appear under a parent below its own tier.");
            Assertions.Throws<ArgumentOutOfRangeException>(
                () => ExpandedSummoningIdealRosterCatalog.Placements(SummonFamily.Monster, 10),
                "There is no tenth spell level.");
        }

        /// <summary>
        /// Charter decision D-01: one creature, one identity. Every creature the
        /// mod already ships must be reused in place, at the same tiers, rather
        /// than allocating a second identity for the same creature.
        /// </summary>
        internal static void ShippedIdentitiesAreReusedNotDuplicated()
        {
            var shipped = ExpandedSummoningCatalog.All
                .ToDictionary(value => value.Key, StringComparer.Ordinal);
            int reused = 0;
            foreach (SummonCreatureSpec creature in ExpandedSummoningCatalog.All)
            {
                IdealRosterEntry entry = ExpandedSummoningIdealRosterCatalog
                    .Find(creature.Key);
                Assertions.True(entry != null,
                    "Shipped creature is missing from the ideal roster: " + creature.Key);
                Assertions.Equal(creature.MonsterTier, entry.MonsterTier,
                    "Ideal roster changed the shipped Monster tier for " + creature.Key);
                Assertions.Equal(creature.NaturesAllyTier, entry.NaturesAllyTier,
                    "Ideal roster changed the shipped Ally tier for " + creature.Key);
                reused++;
            }

            Assertions.Equal(67, reused,
                "Every project-owned creature must be reused.");

            // The retained native wrappers are identities too. Counting only
            // the project-owned catalog here is what hid eleven creatures.
            foreach (string key in ExpandedSummoningCoveragePolicy.NativeWrapperCreatures)
            {
                Assertions.True(ExpandedSummoningIdealRosterCatalog.Find(key) != null,
                    "A retained native creature must be reused, not re-planned: " + key);
                Assertions.False(shipped.ContainsKey(key),
                    "A wrapper creature must not also be project-owned: " + key);
            }

            Assertions.Equal(67, ExpandedSummoningIdealRosterCatalog.All
                .Count(value => ExpandedSummoningCoveragePolicy.Provenance(value.Key) ==
                    SummonUnitProvenance.None),
                "The remaining ideal roster needs 67 new creature identities.");
        }

        /// <summary>
        /// The manifest is a plan, not a publication. Coverage is derived from
        /// the union of both shipped catalogs, so a creature that ships only as
        /// a retained native wrapper counts as represented and a creature with
        /// no live option anywhere counts as planned.
        /// </summary>
        internal static void PlannedRowsPublishNothing()
        {
            foreach (IdealRosterEntry entry in ExpandedSummoningIdealRosterCatalog.All)
            {
                SummonUnitProvenance provenance =
                    ExpandedSummoningCoveragePolicy.Provenance(entry.Key);
                bool anyPlacementLive =
                    ExpandedSummoningCoveragePolicy.Coverage(entry.Key,
                        SummonFamily.Monster, entry.MonsterTier) >=
                        SummonFamilyCoverage.Registered ||
                    ExpandedSummoningCoveragePolicy.Coverage(entry.Key,
                        SummonFamily.NaturesAlly, entry.NaturesAllyTier) >=
                        SummonFamilyCoverage.Registered;

                if (provenance == SummonUnitProvenance.None)
                {
                    Assertions.False(anyPlacementLive,
                        "A creature with no unit identity cannot own a live placement: " +
                        entry.Key);
                }
                else
                {
                    Assertions.True(anyPlacementLive,
                        "A represented creature must own at least one live placement: " +
                        entry.Key);
                }
            }

            Assertions.Equal(67, ExpandedSummoningIdealRosterCatalog.All.Count(
                    value => ExpandedSummoningCoveragePolicy.Provenance(value.Key) ==
                        SummonUnitProvenance.None),
                "67 ideal-roster creatures have no unit identity yet.");
            Assertions.Equal(78,
                ExpandedSummoningCoveragePolicy.RepresentedCreatures.Count,
                "78 creatures already own a unit identity.");

            // The live player-visible surface must be untouched by Sprint 1.
            Assertions.Equal(693,
                ExpandedSummoningBaselineInventory.VisibleChoices(SummonFamily.Monster) +
                ExpandedSummoningBaselineInventory.VisibleChoices(SummonFamily.NaturesAlly),
                "Sprint 1 must not change the shipped visible choice count.");
        }

        /// <summary>
        /// Structural proof that the planning manifest cannot reach publication:
        /// no runtime publication, registration, or menu source may reference it.
        /// </summary>
        internal static void NoPublicationPathReferencesTheManifest()
        {
            const string manifest = "ExpandedSummoningIdealRosterCatalog";
            string[] publicationSources = {
                "src/KingmakerGunslinger/Summoning/SummonPublicationTransaction.cs",
                "src/KingmakerGunslinger/Summoning/ExpandedSummoningIdentityCatalog.cs",
                "src/KingmakerGunslinger/Summoning/SummonVisibilityCatalog.cs",
                "src/KingmakerGunslinger/Summoning/ExpandedSummoningCatalog.cs",
                "src/KingmakerGunslinger/Summoning/SummonNativeExpansionCatalog.cs",
                "src/KingmakerGunslinger/Summoning/ExpandedSummoningVariantMenuPatch.cs",
                "src/KingmakerGunslinger/Summoning/ExpandedSummoningVariantMenuRuntime.cs",
                "src/KingmakerGunslinger/Bootstrap/BlueprintBootstrap.cs",
            };

            foreach (string relative in publicationSources)
            {
                string path = Path.Combine(Environment.CurrentDirectory,
                    relative.Replace('/', Path.DirectorySeparatorChar));
                Assertions.True(File.Exists(path),
                    "Publication source is missing; the guard would silently pass: " + relative);
                Assertions.False(File.ReadAllText(path).Contains(manifest),
                    "A publication path references the planning manifest: " + relative);
            }
        }
    }
}
