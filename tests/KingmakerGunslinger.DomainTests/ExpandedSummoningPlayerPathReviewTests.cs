using System.Linq;
using KingmakerGunslinger.RuntimeTesting;
using KingmakerGunslinger.Summoning;

namespace KingmakerGunslinger.DomainTests
{
    internal static class ExpandedSummoningPlayerPathReviewTests
    {
        internal static void DefaultIsExhaustiveAndRepresentativeCoversContracts()
        {
            var all = ExpandedSummoningPlayerPathReviewScope.Variants(false);
            var sample = ExpandedSummoningPlayerPathReviewScope.Variants(true);
            Assertions.Equal(SummonVisibilityCatalog.PublishedLogicalPlacementCount, all.Length, "Default census remains exhaustive.");
            Assertions.Equal(6, sample.Length, "Both spell families and all three quantities.");
            Assertions.True(sample.All(v => all.Any(a => a.Family == v.Family &&
                a.ParentTier == v.ParentTier && a.Creature.Key == v.Creature.Key &&
                a.Multiplicity == v.Multiplicity)), "No private/unpublished route introduced.");
            Assertions.True(sample.Select(v => v.Family + ":" + v.Multiplicity).Distinct().Count() == 6,
                "No quantity or family omitted.");
            Assertions.True(sample.Select(v => v.Family + ":" + v.ParentTier + ":" + v.Creature.Key + ":" + v.Multiplicity)
                .SequenceEqual(ExpandedSummoningPlayerPathReviewScope.Variants(true)
                    .Select(v => v.Family + ":" + v.ParentTier + ":" + v.Creature.Key + ":" + v.Multiplicity)), "Stable sampling.");
            var natives = ExpandedSummoningPlayerPathReviewScope.Native(false);
            var nativeSample = ExpandedSummoningPlayerPathReviewScope.Native(true);
            Assertions.Equal(SummonNativeExpansionCatalog.All.Count, natives.Length, "Default wrappers remain exhaustive.");
            Assertions.True(nativeSample.Length > 0 && nativeSample.Length < natives.Length && nativeSample.All(natives.Contains),
                "Representative wrappers are a proper published subset.");
            Assertions.True(natives.Select(v => v.Family + ":" + v.Multiplicity + ":" + v.Branch).Distinct().OrderBy(v => v)
                .SequenceEqual(nativeSample.Select(v => v.Family + ":" + v.Multiplicity + ":" + v.Branch).OrderBy(v => v)),
                "Every native family, quantity and alignment branch is represented.");
        }
    }
}
