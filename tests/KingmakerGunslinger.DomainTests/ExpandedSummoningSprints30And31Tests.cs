using System.Linq;
using KingmakerGunslinger.RuntimeTesting;
using KingmakerGunslinger.Summoning;

namespace KingmakerGunslinger.DomainTests
{
    internal static class ExpandedSummoningSprints30And31Tests
    {
        // Exact post-v0.0.163 foundation delta for the current hidden slice.
        // Abilities are23 scoped roots plus eight private abilities (Dretch2,
        // Lantern full ray/Aid, Hound Aid, Trumpet Purge/Aid, Astral Heal).
        internal const int RegisteredAbilityIdentityCount = 23 + 8;
        internal const int RegisteredAreaIdentityCount = 3;
        internal const int RegisteredResourceIdentityCount = 3;
        internal const int RegisteredAiIdentityCount = 8;
        internal const int RegisteredBrainIdentityCount = 4;
        internal const int RegisteredBuffIdentityCount = 7;
        internal const int RegisteredFeatureIdentityCount = 7;
        internal const int RegisteredWeaponIdentityCount = 9;
        internal const int RegisteredSpellbookIdentityCount = 1;
        internal const int RegisteredSpellListIdentityCount = 1;
        internal const int RegisteredUnitTypeIdentityCount = 5;
        internal const int RegisteredWeaponTypeIdentityCount = 9;
        internal const int RegisteredAidEligibilityIdentityCount = 3;
        internal const int RegisteredDretchEligibilityIdentityCount = 2;
        internal const int RegisteredFogSafetyIdentityCount = 1;
        internal const int RegisteredBanishmentEligibilityIdentityCount = 1;
        internal const int RegisteredHealingEligibilityIdentityCount = 1;
        internal const int RegisteredAstralHealingEligibilityIdentityCount = 1;

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
            var astral = all.Where(value => value.Creature.Key == "astral-deva").ToArray();
            Assertions.True(lemure.Select(value => value.ParentTier).SequenceEqual(Enumerable.Range(2, 8)), "Exact SM II-IX Lemure routes.");
            Assertions.True(lemure.All(value => value.Family == SummonFamily.Monster && !value.Creature.MonsterTemplated && !SummonVisibilityCatalog.IsPublished(value)), "No SNA/template/public Lemure root.");
            Assertions.True(all.Where(value => value.Creature.Key != "lemure" && value.Creature.Key != "dretch" && value.Creature.Key != "hound-archon" && value.Creature.Key != "trumpet-archon" && value.Creature.Key != "astral-deva").All(SummonVisibilityCatalog.IsPublished), "Every released creature stays public.");
            Assertions.True(dretch.Select(value => value.ParentTier).SequenceEqual(Enumerable.Range(3, 7)) && dretch.All(value => value.Family == SummonFamily.Monster && !value.Creature.MonsterTemplated && !SummonVisibilityCatalog.IsPublished(value)), "Dretch owns exactly seven hidden untemplated SM III-IX roots.");
            Assertions.Equal(SummonMultiplicity.One, dretch[0].Multiplicity, "SM III Dretch is one.");
            Assertions.Equal(SummonMultiplicity.OneD3, dretch[1].Multiplicity, "SM IV Dretch is 1d3.");
            Assertions.True(dretch.Skip(2).All(value => value.Multiplicity == SummonMultiplicity.OneD4PlusOne), "SM V-IX Dretch are 1d4+1.");
            Assertions.Equal(lemure.Length + dretch.Length + hound.Length + trumpet.Length + astral.Length, SummonVisibilityCatalog.SuppressedLogicalPlacementCount, "Only the implemented first slice is suppressed.");
            Assertions.Equal(all.Length - lemure.Length - dretch.Length - hound.Length - trumpet.Length - astral.Length, SummonVisibilityCatalog.PublishedLogicalPlacementCount, "Published count derives from actual routes.");
            Assertions.True(hound.Select(value => value.ParentTier).SequenceEqual(Enumerable.Range(4, 6)) && hound.All(value => value.Family == SummonFamily.Monster && !value.Creature.MonsterTemplated && !SummonVisibilityCatalog.IsPublished(value)), "Hound owns exactly six hidden untemplated SM IV-IX roots.");
            Assertions.True(astral.Length == 1 && astral[0].ParentTier == 9 && astral[0].Family == SummonFamily.Monster &&
                astral[0].Multiplicity == SummonMultiplicity.One && !astral[0].Creature.MonsterTemplated &&
                !SummonVisibilityCatalog.IsPublished(astral[0]), "Astral owns exactly one hidden untemplated SM IX root.");
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
        internal static void AstralCoreRulesKeepDistinctLevelsAndAuraBoundary()
        {
            AstralDevaRulesPolicy.Validate();
            Assertions.Equal(172, AstralDevaRulesPolicy.AdultHitPoints(), "Adult racial dice, Constitution and Toughness remain distinct.");
            Assertions.Equal(25, AstralDevaRulesPolicy.StunDifficultyClass(), "Two-hit stun uses racial HD and live Strength.");
            Assertions.Equal(26, AstralDevaRulesPolicy.WarhammerAttackBonus(), "Enhanced focused warhammer attack.");
            Assertions.Equal(14, AstralDevaRulesPolicy.WarhammerDamageBonus(), "Two-handed hammer uses live one-and-a-half Strength plus enhancement.");
            Assertions.Equal(23, AstralDevaRulesPolicy.SlamAttackBonus(), "Alternative slam has no hammer enhancement or focus.");
            Assertions.Equal(12, AstralDevaRulesPolicy.SlamDamageBonus(), "Lone primary slam uses live one-and-a-half Strength.");
            Assertions.True(AstralDevaRulesPolicy.BlocksLesserGlobe(3, true, false, 6.7f, 0.5f), "Outside level-three spell is blocked.");
            Assertions.True(AstralDevaRulesPolicy.BlocksLesserGlobe(0, false, true, 7f, 0f), "Outside level-zero SLA is blocked.");
            Assertions.False(AstralDevaRulesPolicy.BlocksLesserGlobe(3, true, false, 6.596f, 0.5f), "Origin on the aura boundary remains inside.");
            Assertions.False(AstralDevaRulesPolicy.BlocksLesserGlobe(4, true, false, 20f, 0f), "Level-four spell passes the lesser globe.");
            Assertions.False(AstralDevaRulesPolicy.BlocksLesserGlobe(3, false, false, 20f, 0f), "Non-spell and non-SLA effects pass.");
            Assertions.False(AstralDevaRulesPolicy.BlocksLesserGlobe(-1, true, false, 20f, 0f), "Missing spell-level attribution cannot manufacture immunity.");
            Assertions.Equal(13, AstralDevaRulesPolicy.SpellLikeCasterLevel, "Astral SLAs stay CL13.");
            Assertions.Equal(15, AstralDevaRulesPolicy.AuraCasterLevel, "Angel aura and shared circle stay CL15/HD15.");
            Assertions.True(AstralDevaRulesPolicy.HasGoodSummoningDescriptor, "Neutral-good Astral routes retain the Good descriptor.");
            Assertions.False(AstralDevaRulesPolicy.HasLawSummoningDescriptor, "Neutral-good Astral routes do not acquire the archons' Law descriptor.");
        }

        internal static void BootstrapCannotAdmitSaveOrUnsupportedPhases()
        {
            string scenario = RuntimeTestScenarioCatalog.DisposableExpandedSummoningSprints30And31NativeReview;
            Assertions.True(ExpandedSummoningNativeProfilePolicy.CanStart(true, scenario, 1, "bootstrap", 0, 0, 0, false, false, false, false, true), "Empty guarded native world is admitted.");
            Assertions.True(ExpandedSummoningNativeProfilePolicy.CanStart(true, scenario, 1, "lemure-core", 0, 0, 0, false, false, false, false, true), "Implemented core consumer phase is admitted only in the same empty guarded fixture.");
            Assertions.False(ExpandedSummoningNativeProfilePolicy.CanStart(true, scenario, 1, "lemure-core", 0, 0, 0, false, false, true, false, true), "Core phase cannot import a save.");
            Assertions.False(ExpandedSummoningNativeProfilePolicy.CanStart(false, scenario, 1, "lemure-core", 0, 0, 0, false, false, false, false, true), "Core phase requires automatic exit.");
            Assertions.True(RuntimeTestScenarioCatalog.Sprints30And31OutsiderBodySequence.SequenceEqual(new[] { "lemure", "dretch" }), "Only the two supported original body subcases share this fixed phase.");
            foreach (string isolated in new[] { "lemure-full", "lemure-ai", "dretch-cloud", "dretch-ai", "dretch-ai-ally", "archon-core", "archon-hound", "hound-armed", "hound-natural", "lantern-rays", "lantern-body", "trumpet-book", "trumpet-book-turn", "trumpet-caster", "outsider-bodies" })
            {
                Assertions.True(ExpandedSummoningNativeProfilePolicy.CanStart(true, scenario, 1, isolated, 0, 0, 0, false, false, false, false, true), "A fixed isolated command phase is admitted in a fresh empty world.");
                Assertions.False(ExpandedSummoningNativeProfilePolicy.CanStart(false, scenario, 1, isolated, 0, 0, 0, false, false, false, false, true), "Isolated phases require automatic exit.");
                Assertions.False(ExpandedSummoningNativeProfilePolicy.CanStart(true, scenario, 1, isolated, 1, 0, 0, false, false, false, false, true), "Isolated phases refuse a prior unit.");
                Assertions.False(ExpandedSummoningNativeProfilePolicy.CanStart(true, scenario, 1, isolated, 0, 1, 0, false, false, false, false, true), "Isolated phases refuse a prior party.");
                Assertions.False(ExpandedSummoningNativeProfilePolicy.CanStart(true, scenario, 1, isolated, 0, 0, 0, false, false, true, false, true), "Isolated phases refuse a save import.");
                Assertions.False(ExpandedSummoningNativeProfilePolicy.CanStart(true, scenario, 2, isolated, 0, 0, 0, false, false, false, false, true), "Isolated phases refuse extra parameters.");
            }
            foreach (string phase in new[] { "Bootstrap", "Lemure-Core", "lemure-core ", "Lemure-Full", "lemure-ai ", "Dretch-Cloud", "dretch-cloud ", "Archon-Core", "archon-core ", "Archon-Hound", "archon-hound ", "Hound-Armed", "hound-armed ", "Hound-Natural", "hound-natural ", "Lantern-Rays", "lantern-rays ", "Lantern-Body", "lantern-body ", "Trumpet-Book", "trumpet-book ", "Trumpet-Book-Turn", "trumpet-book-turn ", "Trumpet-Caster", "trumpet-caster ", "Outsider-Bodies", "outsider-bodies ", "profiles", "routes", "", null })
                Assertions.False(ExpandedSummoningNativeProfilePolicy.CanStart(true, scenario, 1, phase, 0, 0, 0, false, false, false, false, true), "Unimplemented or differently cased phase is refused.");
            Assertions.False(ExpandedSummoningNativeProfilePolicy.CanStart(false, scenario, 1, "bootstrap", 0, 0, 0, false, false, false, false, true), "Automatic exit is required.");
            Assertions.False(ExpandedSummoningNativeProfilePolicy.CanStart(true, scenario, 2, "bootstrap", 0, 0, 0, false, false, false, false, true), "Extra parameters are refused.");
            Assertions.False(ExpandedSummoningNativeProfilePolicy.CanStart(true, scenario, 1, "bootstrap", 1, 0, 0, false, false, false, false, true), "A populated world is refused.");
            Assertions.False(ExpandedSummoningNativeProfilePolicy.CanStart(true, scenario, 1, "bootstrap", 0, 1, 0, false, false, false, false, true), "A human party is refused.");
            Assertions.False(ExpandedSummoningNativeProfilePolicy.CanStart(true, scenario, 1, "bootstrap", 0, 0, 0, false, false, true, false, true), "A save import is refused.");
        }
    }
}
