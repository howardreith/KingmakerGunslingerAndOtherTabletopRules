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
            Assertions.Equal(71, ExpandedSummoningBaselineInventory.UniqueCreatures,
                "Baseline unique creature count changed.");
            Assertions.Equal(68, ExpandedSummoningBaselineInventory.RosterEntries(
                SummonFamily.Monster), "Baseline SM roster count changed.");
            Assertions.Equal(61, ExpandedSummoningBaselineInventory.RosterEntries(
                SummonFamily.NaturesAlly), "Baseline SNA roster count changed.");
            Assertions.Equal(378, ExpandedSummoningBaselineInventory
                .RegisteredPlacements(SummonFamily.Monster),
                "Baseline SM registered placements changed.");
            Assertions.Equal(348, ExpandedSummoningBaselineInventory
                .RegisteredPlacements(SummonFamily.NaturesAlly),
                "Baseline SNA registered placements changed.");
            Assertions.Equal(388, ExpandedSummoningBaselineInventory
                .VisibleChoices(SummonFamily.Monster),
                "Baseline SM visible choice count changed.");
            Assertions.Equal(353, ExpandedSummoningBaselineInventory
                .VisibleChoices(SummonFamily.NaturesAlly),
                "Baseline SNA visible choice count changed.");
        }

        /// <summary>
        /// The 741 headline figure (693 at the Sprint 0 freeze) must decompose
        /// exactly, so a sprint cannot quietly move a choice between the
        /// generated and native pools.
        /// </summary>
        internal static void VisibleChoicesDecomposeExactly()
        {
            int generated = SummonVisibilityCatalog.PublishedLogicalPlacementCount;
            int wrappers = SummonNativeExpansionCatalog.All.Count;
            Assertions.Equal(712, generated, "Published generated placements changed.");
            Assertions.Equal(29, wrappers, "Native wrapper count changed.");
            Assertions.Equal(741, generated + wrappers,
                "The combined visible choice total changed.");
            Assertions.Equal(741,
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
        /// Dire Bat is the one frozen registered-but-hidden identity. Recording
        /// it keeps Sprint 9's unhide from looking like a new creature.
        /// </summary>
        internal static void HiddenAndProxyCreaturesAreRecorded()
        {
            Assertions.Equal(1,
                ExpandedSummoningBaselineInventory.RegisteredButHiddenCreatures.Count,
                "The registered-but-hidden creature set changed.");
            Assertions.Equal("dire-bat",
                ExpandedSummoningBaselineInventory.RegisteredButHiddenCreatures[0],
                "Dire Bat is the only frozen suppressed identity.");
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
            Assertions.Equal(20,
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
            Assertions.True(first.Contains("\"totalVisibleChoices\": 741"),
                "The emitted census lost the frozen visible-choice total.");
        }
    }
}
