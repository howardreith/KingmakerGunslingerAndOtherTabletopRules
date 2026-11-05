using KingmakerGunslinger.Summoning;
using System.Linq;
using KingmakerGunslinger.RuntimeTesting;

namespace KingmakerGunslinger.DomainTests
{
    internal static class IceDevilRules40Tests
    {
        internal static void NativeHasteAndFallbackHands()
        {
            Assertions.True(IceDevilRules40Policy.RetainBiteAttack(true, true, true, false), "First primary fallback bite remains.");
            Assertions.True(IceDevilRules40Policy.RetainBiteAttack(true, true, true, true), "Native Haste repeats the same primary hand; retain its legitimate extra attack.");
            Assertions.False(IceDevilRules40Policy.RetainBiteAttack(true, true, false, true), "Only the separately enumerated duplicate bite limb is suppressed.");
            Assertions.True(IceDevilRules40Policy.RetainBiteAttack(true, true, false, false), "If the native plan omitted the primary hand, preserve its sole additional bite.");
            Assertions.True(IceDevilRules40Policy.RetainBiteAttack(false, true, false, false), "Armed additional bite remains.");
            Assertions.True(IceDevilRules40Policy.RetainBiteAttack(true, false, false, true), "Tail and other weapon identities remain.");
        }
        internal static void PublicRouteAndDependencies()
        {
            var routes = ExpandedSummoningCatalog.GenerateVariants(SummonFamily.Monster)
                .Where(x => x.Creature.Key == "ice-devil").ToArray();
            Assertions.Equal(1, routes.Length, "One route only.");
            Assertions.Equal(9, routes[0].ParentTier, "SM IX.");
            Assertions.Equal(SummonMultiplicity.One, routes[0].Multiplicity, "One adult.");
            Assertions.True(SummonVisibilityCatalog.IsPublished(routes[0]), "The qualified SM IX root is public.");
            Assertions.False(ExpandedSummoningCatalog.GenerateVariants(SummonFamily.NaturesAlly)
                .Any(x => x.Creature.Key == "ice-devil"), "No Nature route.");
            Assertions.False(SummonDemonIdentityCatalog.IsDemonKey("ice-devil"), "Devil is not a demon.");
            var dependencies = ExpandedSummoningIdentityCatalog.Build().Where(x =>
                x.Symbol.StartsWith("KMG.Summoning.Infernal.IceDevil.")).ToArray();
            Assertions.Equal(SummoningCurrentSurface.Sprint40PrivateIdentityCount, dependencies.Length, "Complete owned private graph.");
            Assertions.True(ExpandedSummoningNativeProfilePolicy.Sprint32Phase(
                ExpandedSummoningNativeProfilePolicy.Sprint40IceDevilEarlyPhase), "Real focused dispatch admission.");
            Assertions.True(ExpandedSummoningNativeProfilePolicy.Sprint32Phase(
                ExpandedSummoningNativeProfilePolicy.Sprint40IceDevilCombatPhase), "Real combat dispatch admission.");
            Assertions.False(ExpandedSummoningNativeProfilePolicy.Sprint32Phase(
                ExpandedSummoningNativeProfilePolicy.Sprint40IceDevilCombatPhase + "-extra"), "No expanded phase suffix.");
            foreach (string phase in new[] { ExpandedSummoningNativeProfilePolicy.Sprint40IceDevilEarlyPhase,
                ExpandedSummoningNativeProfilePolicy.Sprint40IceDevilCombatPhase,
                ExpandedSummoningNativeProfilePolicy.Sprint40IceDevilEffectsPhase,
                ExpandedSummoningNativeProfilePolicy.Sprint40IceDevilWallsPhase,
                ExpandedSummoningNativeProfilePolicy.Sprint40IceDevilSlowPhase,
                ExpandedSummoningNativeProfilePolicy.Sprint40NalfeshneeRangePhase,
                ExpandedSummoningNativeProfilePolicy.Sprint40IceDevilPresentationPhase,
                ExpandedSummoningNativeProfilePolicy.Sprint40IceDevilAiPhase,
                ExpandedSummoningNativeProfilePolicy.Sprint40IceDevilSlowTurnPhase,
                ExpandedSummoningNativeProfilePolicy.Sprint40IceDevilPublicPhase })
            {
                Assertions.True(ExpandedSummoningNativeProfilePolicy.Sprint32Phase(phase), "Exact native phase admission.");
                Assertions.True(ExpandedSummoningNativeProfilePolicy.Sprint40Phase(phase), "One exact Sprint40 declaration predicate.");
                Assertions.True(ExpandedSummoningNativeProfilePolicy.Sprint32NativeDefaultOutdoor(
                    RuntimeTestScenarioCatalog.DisposableExpandedSummoningSprint32NativeReview, phase), "Existing outdoor fixture admission.");
                Assertions.False(ExpandedSummoningNativeProfilePolicy.Sprint32Phase(phase + "-extra"), "No phase suffix expansion.");
                Assertions.False(ExpandedSummoningNativeProfilePolicy.Sprint40Phase(phase.ToUpperInvariant()), "No case-folded phase alias.");
            }
        }
        internal static void HitAdmission()
        {
            Assertions.True(IceDevilRules40Policy.QualifiesForSlow(true, true, true, false, true), "Owner spear hit qualifies.");
            Assertions.True(IceDevilRules40Policy.QualifiesForSlow(true, true, false, true, true), "Owner tail hit qualifies.");
            Assertions.False(IceDevilRules40Policy.QualifiesForSlow(true, true, false, false, true), "Bite and unrelated cold packets do not qualify.");
            Assertions.False(IceDevilRules40Policy.QualifiesForSlow(true, true, true, false, false), "A miss never qualifies.");
            Assertions.False(IceDevilRules40Policy.QualifiesForSlow(false, true, true, false, true), "Another blueprint cannot borrow the rider.");
            Assertions.False(IceDevilRules40Policy.QualifiesForSlow(true, false, true, false, true), "Another wielder cannot borrow the rider.");
            Assertions.Equal(23, IceDevilRules40Policy.SlowDc(6), "Printed Constitution gives DC23.");
            Assertions.Equal(25, IceDevilRules40Policy.SlowDc(8), "Live Constitution changes the save DC.");
        }

        internal static void BiteStrength()
        {
            Assertions.Equal(3, IceDevilRules40Policy.BiteStrengthCorrection(6, .5f, true), "Restore the printed full bite contribution beside a manufactured weapon.");
            Assertions.Equal(0, IceDevilRules40Policy.BiteStrengthCorrection(6, 1f, true), "Do not double an already-correct native contribution.");
            Assertions.Equal(0, IceDevilRules40Policy.BiteStrengthCorrection(6, .5f, false), "Only secondary-role mismatch is corrected.");
            Assertions.Equal(-3, IceDevilRules40Policy.BiteStrengthCorrection(6, 1.5f, false), "Disarmed bite keeps its full contribution beside the tail, without the native lone-primary excess.");
            Assertions.Equal(0, IceDevilRules40Policy.BiteStrengthCorrection(-2, .5f, true), "Do not invent a penalty correction.");
        }

        internal static void WallParameters()
        {
            Assertions.Equal(InfernalRules35.WallOfIceSectionHitPoints,
                InfernalRules35.WallSectionHitPoints(12), "Bone HP stays released.");
            Assertions.Equal(InfernalRules35.WallOfIceDurationRounds,
                InfernalRules35.WallDurationRounds(12), "Bone duration stays released.");
            Assertions.Equal(39, InfernalRules35.WallSectionHitPoints(
                IceDevilRules40Policy.SpellLikeCasterLevel), "Ice section uses CL13.");
            Assertions.Equal(130, InfernalRules35.WallDurationRounds(
                IceDevilRules40Policy.SpellLikeCasterLevel), "Ice duration uses CL13.");
            Assertions.Throws<System.ArgumentOutOfRangeException>(() =>
                InfernalRules35.WallDurationRounds(0), "Uninitialized caster context is rejected.");
        }
    }
}
