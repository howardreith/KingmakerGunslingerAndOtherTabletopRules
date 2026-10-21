using System.Linq;
using KingmakerGunslinger.RuntimeTesting;
using KingmakerGunslinger.Summoning;

namespace KingmakerGunslinger.DomainTests
{
    internal static class ExpandedSummoningSprints30And31Tests
    {
        internal static void FirstSliceKeepsExactHiddenRoutesAndReleasedSurface()
        {
            ExpandedSummoningCatalog.Validate();
            SummonVisibilityCatalog.Validate();
            ExpandedSummoningIdentityCatalog.Build();
            var monster = ExpandedSummoningCatalog.GenerateVariants(SummonFamily.Monster);
            var all = monster.Concat(ExpandedSummoningCatalog.GenerateVariants(SummonFamily.NaturesAlly)).ToArray();
            var lemure = all.Where(value => value.Creature.Key == "lemure").ToArray();
            Assertions.True(lemure.Select(value => value.ParentTier).SequenceEqual(Enumerable.Range(2, 8)), "Exact SM II-IX Lemure routes.");
            Assertions.True(lemure.All(value => value.Family == SummonFamily.Monster && !value.Creature.MonsterTemplated && !SummonVisibilityCatalog.IsPublished(value)), "No SNA/template/public Lemure root.");
            Assertions.True(all.Where(value => value.Creature.Key != "lemure").All(SummonVisibilityCatalog.IsPublished), "Every released creature stays public.");
            Assertions.Equal(lemure.Length, SummonVisibilityCatalog.SuppressedLogicalPlacementCount, "Only the implemented first slice is suppressed.");
            Assertions.Equal(all.Length - lemure.Length, SummonVisibilityCatalog.PublishedLogicalPlacementCount, "Published count derives from actual routes.");
            Assertions.Equal(SummonMultiplicity.One, lemure[0].Multiplicity, "SM II is one.");
            Assertions.Equal(SummonMultiplicity.OneD3, lemure[1].Multiplicity, "SM III is 1d3.");
            Assertions.True(lemure.Skip(2).All(value => value.Multiplicity == SummonMultiplicity.OneD4PlusOne), "SM IV-IX are 1d4+1.");
        }
        internal static void BootstrapCannotAdmitSaveOrUnsupportedPhases()
        {
            string scenario = RuntimeTestScenarioCatalog.DisposableExpandedSummoningSprints30And31NativeReview;
            Assertions.True(ExpandedSummoningNativeProfilePolicy.CanStart(true, scenario, 1, "bootstrap", 0, 0, 0, false, false, false, false, true), "Empty guarded native world is admitted.");
            foreach (string phase in new[] { "Bootstrap", "profiles", "routes", "", null })
                Assertions.False(ExpandedSummoningNativeProfilePolicy.CanStart(true, scenario, 1, phase, 0, 0, 0, false, false, false, false, true), "Unimplemented or differently cased phase is refused.");
            Assertions.False(ExpandedSummoningNativeProfilePolicy.CanStart(false, scenario, 1, "bootstrap", 0, 0, 0, false, false, false, false, true), "Automatic exit is required.");
            Assertions.False(ExpandedSummoningNativeProfilePolicy.CanStart(true, scenario, 2, "bootstrap", 0, 0, 0, false, false, false, false, true), "Extra parameters are refused.");
            Assertions.False(ExpandedSummoningNativeProfilePolicy.CanStart(true, scenario, 1, "bootstrap", 1, 0, 0, false, false, false, false, true), "A populated world is refused.");
            Assertions.False(ExpandedSummoningNativeProfilePolicy.CanStart(true, scenario, 1, "bootstrap", 0, 1, 0, false, false, false, false, true), "A human party is refused.");
            Assertions.False(ExpandedSummoningNativeProfilePolicy.CanStart(true, scenario, 1, "bootstrap", 0, 0, 0, false, false, true, false, true), "A save import is refused.");
        }
    }
}
