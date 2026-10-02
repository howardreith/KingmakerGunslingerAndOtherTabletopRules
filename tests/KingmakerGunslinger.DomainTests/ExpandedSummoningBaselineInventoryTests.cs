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
            Assertions.Equal(89, ExpandedSummoningBaselineInventory.UniqueCreatures,
                "Baseline unique creature count changed.");
            Assertions.Equal(81, ExpandedSummoningBaselineInventory.RosterEntries(
                SummonFamily.Monster), "Baseline SM roster count changed.");
            Assertions.Equal(78, ExpandedSummoningBaselineInventory.RosterEntries(
                SummonFamily.NaturesAlly), "Baseline SNA roster count changed.");
            Assertions.Equal(457, ExpandedSummoningBaselineInventory
                .RegisteredPlacements(SummonFamily.Monster),
                "Baseline SM registered placements changed.");
            Assertions.Equal(447, ExpandedSummoningBaselineInventory
                .RegisteredPlacements(SummonFamily.NaturesAlly),
                "Baseline SNA registered placements changed.");
            Assertions.Equal(470, ExpandedSummoningBaselineInventory
                .VisibleChoices(SummonFamily.Monster),
                "Baseline SM visible choice count changed.");
            Assertions.Equal(459, ExpandedSummoningBaselineInventory
                .VisibleChoices(SummonFamily.NaturesAlly),
                "Baseline SNA visible choice count changed.");
        }

        /// <summary>
        /// The 929 visible choices (693 at Sprint 0) must decompose
        /// exactly, so a sprint cannot quietly move a choice between the
        /// generated and native pools.
        /// </summary>
        internal static void VisibleChoicesDecomposeExactly()
        {
            int generated = SummonVisibilityCatalog.PublishedLogicalPlacementCount;
            int wrappers = SummonNativeExpansionCatalog.All.Count;
            Assertions.Equal(900, generated, "Published generated placements changed.");
            Assertions.Equal(29, wrappers, "Native wrapper count changed.");
            Assertions.Equal(929, generated + wrappers,
                "The combined visible choice total changed.");
            Assertions.Equal(929,
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
        /// Wasp, Stirge and the Sprint 11 ungulates are published while the
        /// four Sprint 12 creature families remain hidden pending qualification.
        /// </summary>
        internal static void HiddenAndProxyCreaturesAreRecorded()
        {
            // Sprint 12 published its four creatures. Sprint 13 registers the
            // Shadow Mastiff ahead of its own qualification, so exactly one
            // creature is registered and withheld, which is this pin moving
            // deliberately rather than drifting.
            Assertions.Equal(1,
                ExpandedSummoningBaselineInventory.RegisteredButHiddenCreatures.Count,
                "The registered-but-hidden creature set changed.");
            Assertions.Equal("shadow-mastiff", string.Join("|",
                ExpandedSummoningBaselineInventory.RegisteredButHiddenCreatures),
                "Only the unqualified Sprint 13 Shadow Mastiff may be hidden.");
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
            Assertions.Equal(27,
                ExpandedSummoningBaselineInventory.ProxyVisualCreatures.Count,
                "The frozen borrowed-body proxy count changed.");
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
            Assertions.True(first.Contains("\"totalVisibleChoices\": 929"),
                "The emitted census lost the frozen visible-choice total.");
        }

        /// <summary>
        /// The 0.0.141 records once claimed 882 generated and 911 visible
        /// choices, which were the totals at the Sprint 11 publication commit
        /// before Sprint 12 registered Dire Rat and suppressed four creatures.
        /// Every mutable current record must now state the derived equation,
        /// so a future registration or suppression change cannot leave a stale
        /// published total behind.
        /// </summary>
        internal static void PublishedInventoryRecordsMatchTheDerivedEquation()
        {
            int generated = SummonVisibilityCatalog.PublishedLogicalPlacementCount;
            int wrappers = SummonNativeExpansionCatalog.All.Count;
            int visible = generated + wrappers;
            string generatedText = generated.ToString(
                System.Globalization.CultureInfo.InvariantCulture);
            string visibleText = visible.ToString(
                System.Globalization.CultureInfo.InvariantCulture);

            string[] records = {
                "EXPANDED-SUMMONING-PHASE2-AUTONOMOUS-STATE.md",
                "EXPANDED-SUMMONING-PHASE2-IMPLEMENTATION-REPORT.md",
                "EXPANDED-SUMMONING-PHASE2-JOURNAL.md",
                "EXPANDED-SUMMONING-PHASE2-INVENTORY-RECONCILIATION.md",
                "planning/EXPANDED-SUMMONING-FIDELITY-MATRIX.md",
            };

            foreach (string record in records)
            {
                string text = System.IO.File.ReadAllText(System.IO.Path.Combine(
                    System.Environment.CurrentDirectory,
                    record.Replace('/', System.IO.Path.DirectorySeparatorChar)));
                Assertions.True(text.Contains(generatedText),
                    record + " must state the derived published generated " +
                    "placement count " + generatedText + ".");
                Assertions.True(text.Contains(visibleText),
                    record + " must state the derived visible choice total " +
                    visibleText + ".");
            }

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
