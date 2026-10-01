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
            Assertions.Equal(88, ExpandedSummoningBaselineInventory.UniqueCreatures,
                "Baseline unique creature count changed.");
            Assertions.Equal(80, ExpandedSummoningBaselineInventory.RosterEntries(
                SummonFamily.Monster), "Baseline SM roster count changed.");
            Assertions.Equal(78, ExpandedSummoningBaselineInventory.RosterEntries(
                SummonFamily.NaturesAlly), "Baseline SNA roster count changed.");
            Assertions.Equal(453, ExpandedSummoningBaselineInventory
                .RegisteredPlacements(SummonFamily.Monster),
                "Baseline SM registered placements changed.");
            Assertions.Equal(447, ExpandedSummoningBaselineInventory
                .RegisteredPlacements(SummonFamily.NaturesAlly),
                "Baseline SNA registered placements changed.");
            Assertions.Equal(436, ExpandedSummoningBaselineInventory
                .VisibleChoices(SummonFamily.Monster),
                "Baseline SM visible choice count changed.");
            Assertions.Equal(425, ExpandedSummoningBaselineInventory
                .VisibleChoices(SummonFamily.NaturesAlly),
                "Baseline SNA visible choice count changed.");
        }

        /// <summary>
        /// The 861 visible choices (693 at Sprint 0) must decompose
        /// exactly, so a sprint cannot quietly move a choice between the
        /// generated and native pools.
        /// </summary>
        internal static void VisibleChoicesDecomposeExactly()
        {
            int generated = SummonVisibilityCatalog.PublishedLogicalPlacementCount;
            int wrappers = SummonNativeExpansionCatalog.All.Count;
            Assertions.Equal(832, generated, "Published generated placements changed.");
            Assertions.Equal(29, wrappers, "Native wrapper count changed.");
            Assertions.Equal(861, generated + wrappers,
                "The combined visible choice total changed.");
            Assertions.Equal(861,
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
            Assertions.Equal(4,
                ExpandedSummoningBaselineInventory.RegisteredButHiddenCreatures.Count,
                "The registered-but-hidden creature set changed.");
            Assertions.Equal("dire-rat|dog|goblin-dog|hyena", string.Join("|",
                ExpandedSummoningBaselineInventory.RegisteredButHiddenCreatures),
                "Only the authorized Sprint 12 creature families may be hidden.");
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
            Assertions.Equal(26,
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
            Assertions.True(first.Contains("\"totalVisibleChoices\": 861"),
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
                "docs/RELEASE-NOTES-0.0.141.md",
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
