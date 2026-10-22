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
            SummonIconCatalog.Validate();
            SummonVisibilityCatalog.Validate();
            ExpandedSummoningIdentityCatalog.Build();
            var monster = ExpandedSummoningCatalog.GenerateVariants(SummonFamily.Monster);
            var all = monster.Concat(ExpandedSummoningCatalog.GenerateVariants(SummonFamily.NaturesAlly)).ToArray();
            var dretch = all.Where(value => value.Creature.Key == "dretch").ToArray();
            var trumpet = all.Where(value => value.Creature.Key == "trumpet-archon").ToArray();
            var hound = all.Where(value => value.Creature.Key == "hound-archon").ToArray();
            var lemure = all.Where(value => value.Creature.Key == "lemure").ToArray();
            Assertions.True(lemure.Select(value => value.ParentTier).SequenceEqual(Enumerable.Range(2, 8)), "Exact SM II-IX Lemure routes.");
            Assertions.True(lemure.All(value => value.Family == SummonFamily.Monster && !value.Creature.MonsterTemplated && !SummonVisibilityCatalog.IsPublished(value)), "No SNA/template/public Lemure root.");
            Assertions.True(all.Where(value => value.Creature.Key != "lemure" && value.Creature.Key != "dretch" && value.Creature.Key != "hound-archon" && value.Creature.Key != "trumpet-archon").All(SummonVisibilityCatalog.IsPublished), "Every released creature stays public.");
            Assertions.True(dretch.Select(value => value.ParentTier).SequenceEqual(Enumerable.Range(3, 7)) && dretch.All(value => value.Family == SummonFamily.Monster && !value.Creature.MonsterTemplated && !SummonVisibilityCatalog.IsPublished(value)), "Dretch owns exactly seven hidden untemplated SM III-IX roots.");
            Assertions.Equal(SummonMultiplicity.One, dretch[0].Multiplicity, "SM III Dretch is one.");
            Assertions.Equal(SummonMultiplicity.OneD3, dretch[1].Multiplicity, "SM IV Dretch is 1d3.");
            Assertions.True(dretch.Skip(2).All(value => value.Multiplicity == SummonMultiplicity.OneD4PlusOne), "SM V-IX Dretch are 1d4+1.");
            Assertions.Equal(lemure.Length + dretch.Length + hound.Length + trumpet.Length, SummonVisibilityCatalog.SuppressedLogicalPlacementCount, "Only the implemented first slice is suppressed.");
            Assertions.Equal(all.Length - lemure.Length - dretch.Length - hound.Length - trumpet.Length, SummonVisibilityCatalog.PublishedLogicalPlacementCount, "Published count derives from actual routes.");
            Assertions.True(hound.Select(value => value.ParentTier).SequenceEqual(Enumerable.Range(4, 6)) && hound.All(value => value.Family == SummonFamily.Monster && !value.Creature.MonsterTemplated && !SummonVisibilityCatalog.IsPublished(value)), "Hound owns exactly six hidden untemplated SM IV-IX roots.");
            Assertions.True(trumpet.Length == 1 && trumpet[0].ParentTier == 9 && trumpet[0].Family == SummonFamily.Monster &&
                trumpet[0].Multiplicity == SummonMultiplicity.One && !trumpet[0].Creature.MonsterTemplated &&
                !SummonVisibilityCatalog.IsPublished(trumpet[0]), "Trumpet owns exactly one hidden untemplated SM IX root.");
            Assertions.Equal(SummonMultiplicity.One, hound[0].Multiplicity, "SM IV Hound is one.");
            Assertions.Equal(SummonMultiplicity.OneD3, hound[1].Multiplicity, "SM V Hound is 1d3.");
            Assertions.True(hound.Skip(2).All(value => value.Multiplicity == SummonMultiplicity.OneD4PlusOne), "SM VI-IX Hound are1d4+1.");
            Assertions.Equal(SummonMultiplicity.One, lemure[0].Multiplicity, "SM II is one.");
            Assertions.Equal(SummonMultiplicity.OneD3, lemure[1].Multiplicity, "SM III is 1d3.");
            Assertions.True(lemure.Skip(2).All(value => value.Multiplicity == SummonMultiplicity.OneD4PlusOne), "SM IV-IX are 1d4+1.");
        }
        internal static void BootstrapCannotAdmitSaveOrUnsupportedPhases()
        {
            string scenario = RuntimeTestScenarioCatalog.DisposableExpandedSummoningSprints30And31NativeReview;
            Assertions.True(ExpandedSummoningNativeProfilePolicy.CanStart(true, scenario, 1, "bootstrap", 0, 0, 0, false, false, false, false, true), "Empty guarded native world is admitted.");
            Assertions.True(ExpandedSummoningNativeProfilePolicy.CanStart(true, scenario, 1, "lemure-core", 0, 0, 0, false, false, false, false, true), "Implemented core consumer phase is admitted only in the same empty guarded fixture.");
            Assertions.False(ExpandedSummoningNativeProfilePolicy.CanStart(true, scenario, 1, "lemure-core", 0, 0, 0, false, false, true, false, true), "Core phase cannot import a save.");
            Assertions.False(ExpandedSummoningNativeProfilePolicy.CanStart(false, scenario, 1, "lemure-core", 0, 0, 0, false, false, false, false, true), "Core phase requires automatic exit.");
            foreach (string isolated in new[] { "lemure-full", "lemure-ai", "dretch-cloud", "dretch-ai", "dretch-ai-ally", "archon-core", "archon-hound", "trumpet-book" })
            {
                Assertions.True(ExpandedSummoningNativeProfilePolicy.CanStart(true, scenario, 1, isolated, 0, 0, 0, false, false, false, false, true), "A fixed isolated command phase is admitted in a fresh empty world.");
                Assertions.False(ExpandedSummoningNativeProfilePolicy.CanStart(false, scenario, 1, isolated, 0, 0, 0, false, false, false, false, true), "Isolated phases require automatic exit.");
                Assertions.False(ExpandedSummoningNativeProfilePolicy.CanStart(true, scenario, 1, isolated, 1, 0, 0, false, false, false, false, true), "Isolated phases refuse a prior unit.");
                Assertions.False(ExpandedSummoningNativeProfilePolicy.CanStart(true, scenario, 1, isolated, 0, 1, 0, false, false, false, false, true), "Isolated phases refuse a prior party.");
                Assertions.False(ExpandedSummoningNativeProfilePolicy.CanStart(true, scenario, 1, isolated, 0, 0, 0, false, false, true, false, true), "Isolated phases refuse a save import.");
                Assertions.False(ExpandedSummoningNativeProfilePolicy.CanStart(true, scenario, 2, isolated, 0, 0, 0, false, false, false, false, true), "Isolated phases refuse extra parameters.");
            }
            foreach (string phase in new[] { "Bootstrap", "Lemure-Core", "lemure-core ", "Lemure-Full", "lemure-ai ", "Dretch-Cloud", "dretch-cloud ", "Archon-Core", "archon-core ", "Archon-Hound", "archon-hound ", "Trumpet-Book", "trumpet-book ", "profiles", "routes", "", null })
                Assertions.False(ExpandedSummoningNativeProfilePolicy.CanStart(true, scenario, 1, phase, 0, 0, 0, false, false, false, false, true), "Unimplemented or differently cased phase is refused.");
            Assertions.False(ExpandedSummoningNativeProfilePolicy.CanStart(false, scenario, 1, "bootstrap", 0, 0, 0, false, false, false, false, true), "Automatic exit is required.");
            Assertions.False(ExpandedSummoningNativeProfilePolicy.CanStart(true, scenario, 2, "bootstrap", 0, 0, 0, false, false, false, false, true), "Extra parameters are refused.");
            Assertions.False(ExpandedSummoningNativeProfilePolicy.CanStart(true, scenario, 1, "bootstrap", 1, 0, 0, false, false, false, false, true), "A populated world is refused.");
            Assertions.False(ExpandedSummoningNativeProfilePolicy.CanStart(true, scenario, 1, "bootstrap", 0, 1, 0, false, false, false, false, true), "A human party is refused.");
            Assertions.False(ExpandedSummoningNativeProfilePolicy.CanStart(true, scenario, 1, "bootstrap", 0, 0, 0, false, false, true, false, true), "A save import is refused.");
        }
    }
}
