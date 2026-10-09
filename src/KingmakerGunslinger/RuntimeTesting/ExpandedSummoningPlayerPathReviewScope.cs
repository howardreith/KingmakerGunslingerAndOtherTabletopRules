using System.Linq;
using KingmakerGunslinger.Summoning;

namespace KingmakerGunslinger.RuntimeTesting
{
    // Request-local sampling only. The default remains the exhaustive census.
    internal static class ExpandedSummoningPlayerPathReviewScope
    {
        internal const string Parameter = "playerPathScope";
        internal const string Representative = "representative";

        internal static SummonVariantSpec[] Variants(bool representative)
        {
            var all = ExpandedSummoningCatalog.GenerateVariants(SummonFamily.Monster)
                .Concat(ExpandedSummoningCatalog.GenerateVariants(SummonFamily.NaturesAlly))
                .Where(SummonVisibilityCatalog.IsPublished).ToArray();
            return representative ? all.GroupBy(value => new { value.Family, value.Multiplicity })
                .Select(group => group.First()).ToArray() : all;
        }

        internal static SummonNativeExpansionSpec[] Native(bool representative)
        {
            var all = SummonNativeExpansionCatalog.All.ToArray();
            return representative ? all.GroupBy(value => new { value.Family, value.Multiplicity, value.Branch })
                .Select(group => group.First()).ToArray() : all;
        }
    }
}
