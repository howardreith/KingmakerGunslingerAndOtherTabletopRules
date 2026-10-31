using System;
using System.Linq;
using KingmakerGunslinger.Summoning;

namespace KingmakerGunslinger.DomainTests
{
    /// <summary>
    /// Sprint 0 of the Expanded Summoning charter froze the shipped surface;
    /// these pins move only when a sprint deliberately changes the roster
    /// (Phase 1 Sprint 3: Pony, Horse, Owlbear, Cyclops and the Nature's Ally
    /// Frost Giant wrappers) and prove the observer is inert.
    /// </summary>
    internal static class ExpandedSummoningBaselineInventoryTests
    {
        internal static void ShippedSurfaceMatchesFrozenBaseline()
        {
            Assertions.Equal(SummoningCurrentSurface.RegisteredCreatureCount,
                ExpandedSummoningBaselineInventory.UniqueCreatures,
                "Baseline unique creature count changed.");
            Assertions.Equal(SummoningCurrentSurface.RegisteredMonsterCreatureCount,
                ExpandedSummoningBaselineInventory.RosterEntries(
                SummonFamily.Monster), "Baseline SM roster count changed.");
            Assertions.Equal(SummoningCurrentSurface.RegisteredNatureAllyCreatureCount,
                ExpandedSummoningBaselineInventory.RosterEntries(
                SummonFamily.NaturesAlly), "Baseline SNA roster count changed.");
            Assertions.Equal(SummoningCurrentSurface.RegisteredMonsterPlacements,
                ExpandedSummoningBaselineInventory
                .RegisteredPlacements(SummonFamily.Monster),
                "Baseline SM registered placements changed.");
            // Sprint 21's Giant Crab and Sprint 23's Stegosaurus are Nature's
            // Ally-only creatures. Their placements remain on that side after
            // publication.
            Assertions.Equal(SummoningCurrentSurface.RegisteredNatureAllyPlacements,
                ExpandedSummoningBaselineInventory
                .RegisteredPlacements(SummonFamily.NaturesAlly),
                "Baseline SNA registered placements changed.");
            // The Shadow Mastiff is a Summon Monster creature, so its four
            // placements all land on the SM side when it publishes. Sprint 14's
            // Fire Beetle is reachable from both parents and splits its
            // eighteen evenly, nine each. Sprint 15 is lopsided for a reason
            // that is in the roster rather than in the creatures: the Drone is
            // reachable from both parents and splits six and six, while the
            // Giant Stag Beetle is a Nature's Ally creature alone and puts all
            // six of its placements on that side.
            //
            // Independently qualified Sprint 17 snakes publish sixteen roots
            // per family: 506/502 generated plus 17/12 retained wrappers.
            // Sprint 18 published its thirteen roots per family, and
            // Sprint 19 adds five visible Summon Monster roots for the Xill
            // and five Summon Nature's Ally roots for the Girallon. Sprint 20
            // publishes the Giant Scorpion on both tables: six Summon Monster
            // roots and six Summon Nature's Ally roots.
            // Sprint 24 publishes nine Summon Monster theropod choices:
            // six Deinonychus and three Tyrannosaurus.
            Assertions.Equal(SummoningCurrentSurface.PublishedMonsterPlacements + SummonNativeExpansionCatalog.All.Count(value => value.Family == SummonFamily.Monster), ExpandedSummoningBaselineInventory
                .VisibleChoices(SummonFamily.Monster),
                "Baseline SM visible choice count changed.");
            // Sprint 21 publishes the Giant Crab's seven roots, every
            // one of them a Nature's Ally choice: it is not on the Summon
            // Monster table, which is why that side does not move.
            // Sprint 23 adds four Triceratops and four Stegosaurus choices to
            // the Nature's Ally side; Triceratops adds four to Monster.
            // Sprint 24 publishes nine Nature's Ally theropod choices:
            // six Deinonychus and three Tyrannosaurus.
            Assertions.Equal(SummoningCurrentSurface.PublishedNatureAllyPlacements +
                SummonNativeExpansionCatalog.All.Count(value =>
                    value.Family == SummonFamily.NaturesAlly), ExpandedSummoningBaselineInventory
                .VisibleChoices(SummonFamily.NaturesAlly),
                "Baseline SNA visible choice count changed.");
        }

        /// <summary>
        /// The 1037 visible choices (693 at Sprint 0) must decompose
        /// exactly, so a sprint cannot quietly move a choice between the
        /// generated and native pools.
        /// </summary>
        internal static void VisibleChoicesDecomposeExactly()
        {
            int generated = SummonVisibilityCatalog.PublishedLogicalPlacementCount;
            int wrappers = SummonNativeExpansionCatalog.All.Count;
            Assertions.Equal(SummoningCurrentSurface.FinalPublishedLogicalPlacements,
                generated, "Published generated placements changed.");
            Assertions.Equal(GiantBodyRulesPolicy.RetainedNativeWrappers,
                wrappers, "Native wrapper count changed.");
            Assertions.Equal(SummoningCurrentSurface.PublicVisibleChoices,
                generated + wrappers,
                "The combined visible choice total changed.");
            Assertions.Equal(SummoningCurrentSurface.PublicVisibleChoices,
                ExpandedSummoningBaselineInventory.VisibleChoices(SummonFamily.Monster) +
                ExpandedSummoningBaselineInventory.VisibleChoices(SummonFamily.NaturesAlly),
                "Per-parent census disagrees with the catalog totals.");
        }

        /// <summary>
        /// Every parent census must reconcile against the catalog it observes,
        /// and no parent may exceed the roster that feeds it.
        /// </summary>
        internal static void PerParentCensusReconciles()
        {
            foreach (SummonFamily family in new[] {
                SummonFamily.Monster, SummonFamily.NaturesAlly })
            {
                int roster = ExpandedSummoningBaselineInventory.RosterEntries(family);
                foreach (ExpandedSummoningBaselineInventory.ParentCensus census in
                    ExpandedSummoningBaselineInventory.Census(family))
                {
                    Assertions.Equal(census.One + census.OneD3 + census.OneD4PlusOne,
                        census.Registered, "Registered placements must be the quantity sum.");
                    Assertions.True(census.Registered <= roster,
                        "A parent cannot offer more creatures than the family roster.");
                    Assertions.True(census.PublishedGenerated >= 0,
                        "Suppression cannot exceed registration.");
                    Assertions.Equal(census.PublishedGenerated + census.NativeWrappers,
                        census.VisibleChoices, "Visible choices must sum exactly.");
                }
            }

            // Tier 1 offers only own-tier singles; nothing lower exists to scale.
            ExpandedSummoningBaselineInventory.ParentCensus first =
                ExpandedSummoningBaselineInventory.Census(SummonFamily.Monster).First();
            Assertions.Equal(0, first.OneD3, "Summon Monster I cannot offer 1d3 options.");
            Assertions.Equal(0, first.OneD4PlusOne,
                "Summon Monster I cannot offer 1d4+1 options.");
        }

        /// <summary>
        /// Wasp, Stirge and the Sprint 11 ungulates are published, and so now
        /// are the Sprint 12 and Sprint 13 creatures, so nothing is withheld.
        /// </summary>
        internal static void HiddenAndProxyCreaturesAreRecorded()
        {
            // A creature appearing here is a sprint registering ahead of
            // its own qualification, which is allowed and is how Sprints 12,
            // 13, 18, 19 and 20 each ran - but it has to be deliberate rather
            // than a leftover, which is what this pin is for. Naming the
            // creature is the strongest form of it: an unexpected arrival and
            // a forgotten departure both fail.
            // Sprint 29 publication removes the three giant suppressions, so
            // the registered-but-hidden set is empty again.
            Assertions.True(ExpandedSummoningBaselineInventory
                    .RegisteredButHiddenCreatures.SequenceEqual(SummoningCurrentSurface.WithheldKeys,
                    StringComparer.Ordinal),
                "The public candidate has no withheld creatures: "
                + string.Join(", ", ExpandedSummoningBaselineInventory
                    .RegisteredButHiddenCreatures));
            Assertions.Equal(SummoningCurrentSurface.WithheldKeys.Length,
                ExpandedSummoningBaselineInventory
                    .RegisteredButHiddenCreatures.Count,
                "Unexpected registered-but-withheld creature: " + string.Join(", ",
                    ExpandedSummoningBaselineInventory
                        .RegisteredButHiddenCreatures));
            Assertions.True(ExpandedSummoningBaselineInventory.ProxyVisualCreatures
                .Contains("pteranodon<Roc"),
                "Pteranodon must still be recorded as a Roc-policy visual proxy.");

            // A view policy that names the creature itself is an exact visual.
            // Counting those as proxies would overstate the remaining work.
            Assertions.False(ExpandedSummoningBaselineInventory.ProxyVisualCreatures
                .Contains("boar<Boar"),
                "A self-named view policy is an exact visual, not a proxy.");
            Assertions.False(ExpandedSummoningBaselineInventory.ProxyVisualCreatures
                .Contains("dire-tiger<Smilodon"),
                "Smilodon displays under its own name and is not a proxy.");
            string[] batchAdditions = { "lemure", "dretch", "hound-archon", "astral-deva", "trumpet-archon", "lillend-azata", "bearded-devil", "kyton-chain-devil", "barbed-devil", "bone-devil" };
            Assertions.True(batchAdditions.All(key => ExpandedSummoningBaselineInventory.ProxyVisualCreatures
                    .Count(value => value.StartsWith(key + "<", StringComparison.Ordinal)) == 1),
                "Each selected new body retains its actual borrowed-rig disclosure.");
            Assertions.Equal(43 + TheropodRulesPolicy.AppendedCreatureCount +
                    GiantBodyRulesPolicy.AppendedCreatureCount + 1 + GiantHumanoidRulesPolicy.BorrowedVisualCount + 3,
                ExpandedSummoningBaselineInventory.ProxyVisualCreatures.Count(value => !value.StartsWith("griffon<", StringComparison.Ordinal) &&
                    !batchAdditions.Any(key => value.StartsWith(key + "<", StringComparison.Ordinal))),
                "The frozen borrowed-body proxy count changed. A creature counts here while it rides another creature's rig, including both Sprint 23 dinosaurs whose project-owned bodies bind to their measured native animation donors.");
        }

        /// <summary>
        /// The charter requires proof that a baseline observer cannot disturb
        /// the state it measures. Observing twice must be byte-identical and
        /// must leave every catalog instance and count untouched.
        /// </summary>
        internal static void ObserverIsInertAndDeterministic()
        {
            int creaturesBefore = ExpandedSummoningCatalog.All.Count;
            int wrappersBefore = SummonNativeExpansionCatalog.All.Count;
            int optionsBefore = SummonNativeOptionCatalog.All.Count;
            SummonCreatureSpec firstBefore = ExpandedSummoningCatalog.All[0];

            string first = ExpandedSummoningBaselineInventory.Emit();
            string second = ExpandedSummoningBaselineInventory.Emit();

            Assertions.Equal(first, second, "The baseline census is not deterministic.");
            Assertions.Equal(creaturesBefore, ExpandedSummoningCatalog.All.Count,
                "Observation changed the creature catalog.");
            Assertions.Equal(wrappersBefore, SummonNativeExpansionCatalog.All.Count,
                "Observation changed the native expansion catalog.");
            Assertions.Equal(optionsBefore, SummonNativeOptionCatalog.All.Count,
                "Observation changed the native option catalog.");
            Assertions.True(ReferenceEquals(firstBefore, ExpandedSummoningCatalog.All[0]),
                "Observation replaced a frozen creature instance.");

            // The frozen invariants must still hold after observation.
            ExpandedSummoningCatalog.Validate();
            SummonVisibilityCatalog.Validate();
            SummonNativeExpansionCatalog.Validate();

            Assertions.True(first.StartsWith(
                "{\n  \"schema\": \"" + ExpandedSummoningBaselineInventory.BaselineSchema + "\""),
                "The census must declare its schema first so evidence stays comparable.");
            Assertions.True(first.Contains("\"totalVisibleChoices\": " +
                SummoningCurrentSurface.PublicVisibleChoices),
                "The emitted census lost the frozen visible-choice total.");
        }

        // Immutable historical figures retain their own provenance. Current
        // operation is governed by the active record, not an old release ledger.
        internal static void HistoricalInventoryRecordsRetainPublishedFigures()
        {
            string[] records = {
                "EXPANDED-SUMMONING-PHASE2-IMPLEMENTATION-REPORT.md",
                "EXPANDED-SUMMONING-PHASE2-JOURNAL.md",
                "EXPANDED-SUMMONING-PHASE2-INVENTORY-RECONCILIATION.md",
                "planning/EXPANDED-SUMMONING-FIDELITY-MATRIX.md",
            };

            // The released 0.0.141 notes describe a frozen build, not the
            // current source, so they keep that build's own equation. Letting
            // them drift to today's numbers would misreport what shipped.
            string released = System.IO.File.ReadAllText(System.IO.Path.Combine(
                System.Environment.CurrentDirectory, "docs",
                "RELEASE-NOTES-0.0.141.md"));
            Assertions.True(released.Contains("832") && released.Contains("861"),
                "The 0.0.141 release notes must keep the equation that build actually shipped.");

            // The reconciliation record is the one place allowed to quote the
            // superseded figures, because explaining them is its purpose.
            foreach (string record in records)
            {
                if (record.EndsWith("INVENTORY-RECONCILIATION.md",
                        System.StringComparison.Ordinal)) continue;
                string text = System.IO.File.ReadAllText(System.IO.Path.Combine(
                    System.Environment.CurrentDirectory,
                    record.Replace('/', System.IO.Path.DirectorySeparatorChar)));
                foreach (string stale in new[] {
                    "911 visible choices", "911 total choices",
                    "911 visible summon choices"
                })
                    Assertions.True(!text.Contains(stale) ||
                        text.Contains("INVENTORY-RECONCILIATION"),
                        record + " still presents the superseded total \"" +
                        stale + "\" without pointing at the reconciliation " +
                        "record.");
            }
        }
    }
}
