using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using KingmakerGunslinger.Summoning;

namespace KingmakerGunslinger.DomainTests
{
    internal static class Cleanup2CreatureReviewRosterTests
    {
        internal static void EveryAdvertisedWorkingKeyResolvesToItsPublishedNativeObserverVariant()
        {
            string wrapper = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "scripts", "SummonCleanup2CurrentStack.Common.ps1"));
            Match assignment = Regex.Match(wrapper, @"\$allowed\s*=\s*@\((.*?)\)", RegexOptions.Singleline);
            Assertions.True(assignment.Success, "The wrapper must retain an explicit bounded mission roster.");
            string literal = assignment.Groups[1].Value;
            string[] advertised = Regex.Matches(literal, "'([^']*)'").Cast<Match>().Select(value => value.Groups[1].Value).ToArray();
            Assertions.True(Regex.Replace(literal, "'[^']*'|[\\s,]", "").Length == 0,
                "The advertised roster must be literal, not an unevaluated expression.");
            Assertions.True(advertised.SequenceEqual(Cleanup2CreatureReviewRoster.WorkingSaveKeys),
                "Wrapper and native observer authority must expose the same ordered mission scope.");
            Assertions.True(Cleanup2CreatureReviewRoster.ValidateWorkingSaveRequest(advertised).SequenceEqual(advertised),
                "An entire advertised batch must resolve before any execution.");
            foreach (string key in advertised)
            {
                SummonVariantSpec variant = Cleanup2CreatureReviewRoster.ResolveGeneratedVariant(key, SummonMultiplicity.One);
                SummonCreatureSpec source = ExpandedSummoningCatalog.All.Single(value => value.Key == key);
                SummonFamily expectedFamily = source.NaturesAllyTier.HasValue ? SummonFamily.NaturesAlly : SummonFamily.Monster;
                Assertions.True(ReferenceEquals(source, variant.Creature), "Resolver must return the actual catalog creature.");
                Assertions.Equal(expectedFamily, variant.Family, "Native review prefers the existing Nature's Ally route.");
                Assertions.Equal((expectedFamily == SummonFamily.NaturesAlly ? source.NaturesAllyTier : source.MonsterTier).Value,
                    variant.ParentTier, "One-creature review must use its actual first published tier.");
                Assertions.Equal(SummonMultiplicity.One, variant.Multiplicity, "The wrapper advertises only quantity One.");
                Assertions.True(SummonVisibilityCatalog.IsPublished(variant), "No advertised mission route may rely on suppression exceptions.");
            }
        }

        internal static void FrostUnknownDuplicatesAndUnsupportedQuantitiesFailBeforeAnyBatchIsReturned()
        {
            foreach (string[] keys in new[] { new[] { "frost-giant" }, new[] { "giant-crab", "frost-giant", "ettin" },
                new[] { "Frost-Giant" }, new[] { "giant-crab", "giant-crab" }, new[] { "wolf" },
                new[] { "ettin ", "gorgon" }, new string[0], new string[] { null } })
                Assertions.Throws<InvalidOperationException>(() => Cleanup2CreatureReviewRoster.ValidateWorkingSaveRequest(keys),
                    "Invalid mission requests must reject the complete batch.");
            Assertions.Throws<InvalidOperationException>(() => Cleanup2CreatureReviewRoster.ValidateWorkingSaveRequest(null), "Null request must reject.");
            Assertions.Throws<InvalidOperationException>(() => Cleanup2CreatureReviewRoster.ResolveGeneratedVariant("frost-giant", SummonMultiplicity.One),
                "Retained Frost must never be silently projected into the generated catalog.");
            Assertions.Throws<InvalidOperationException>(() => Cleanup2CreatureReviewRoster.ResolveGeneratedVariant("ettin", SummonMultiplicity.OneD3),
                "Unsupported native review quantities must stay rejected.");
        }
    }
}
