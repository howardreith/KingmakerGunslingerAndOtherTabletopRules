using System;
using System.Linq;
using KingmakerGunslinger.Summoning;

namespace KingmakerGunslinger.DomainTests
{
    /// <summary>
    /// Repair sprint R5: the Pixie's four at-will spell-likes come from the
    /// exact native spells, the optional Call of the Wild provider gate omits
    /// cleanly, and the custom Cloud Giant Fog Cloud route is retired.
    /// </summary>
    internal static class SummonPlaytestR5Tests
    {
        internal static void PixieAtWillUsesTheNativeSpells()
        {
            var specs = SummonPlaytestR5Policy.PixieAtWill;
            Assertions.True(specs.Select(spec => spec.DisplayName).SequenceEqual(new[] { "Entangle", "Confusion", "Dispel Magic", "Shield" }),
                "Exactly the four owner-requested spell-likes, full Confusion (not Lesser).");
            Assertions.Equal("cf6c901fb7acc904e85c63b342e9c949", specs.Single(spec => spec.Token == "Confusion").NativeGuid,
                "Confusion is the native full Confusion spell.");
            var dispel = specs.Single(spec => spec.Token == "DispelMagic");
            Assertions.True(dispel.VariantTokens.Length == 2 && dispel.NativeVariantGuids.Length == 2 &&
                dispel.NativeVariantGuids.Distinct().Count() == 2, "Dispel Magic keeps both native variants as owned variants.");
            Assertions.Equal(1 + specs.Count + dispel.VariantTokens.Length + specs.Count, SummonPlaytestR5Policy.NewIdentities().Count(),
                "The Mastodon trample, plus one ability and one AI action per Pixie spell and Dispel Magic's two variants.");
            Assertions.Equal(SummonPlaytestR5Policy.NewIdentities().Count(),
                SummonPlaytestR5Policy.NewIdentities().Select(pair => pair.Key).Distinct().Count(), "New identities are unique.");
            Assertions.True(specs.All(spec => spec.Symbol.StartsWith(SummonPlaytestR5Policy.PixiePrefix, StringComparison.Ordinal)),
                "Owned Pixie identities only.");
            Assertions.True(specs.Where(spec => spec.NativeAiGuid == null).All(spec => spec.OwnedAiCooldownRounds > 0),
                "An owned AI action for an at-will spell rests between casts (no continuous recasting).");
        }

        internal static void OptionalProviderOmitsUnlessExactAndActive()
        {
            Func<int, bool, bool, bool, bool, string, string> disposition = SummonPlaytestR5Policy.ProviderDisposition;
            Assertions.Equal("provider-active", disposition(1, true, true, true, false, "CallOfTheWild"), "Exact active provider binds.");
            Assertions.Equal("provider-absent", disposition(0, false, false, false, false, null), "Absent provider omits.");
            Assertions.Equal("provider-ambiguous", disposition(2, true, true, true, false, "CallOfTheWild"), "Two entries omit.");
            Assertions.Equal("provider-inactive-or-mismatched", disposition(1, true, false, true, false, "CallOfTheWild"), "Disabled provider omits.");
            Assertions.Equal("provider-inactive-or-mismatched", disposition(1, true, true, true, true, "CallOfTheWild"), "Provider load error omits.");
            Assertions.Equal("provider-inactive-or-mismatched", disposition(1, true, true, true, false, "Other"), "Mismatched assembly omits.");
            Assertions.Equal("provider-inactive-or-mismatched", disposition(1, false, true, true, false, "CallOfTheWild"), "Unloaded provider omits.");
        }

        internal static void MastodonTrampleUsesItsOwnPrintedValues()
        {
            var mastodon = UngulateRulesPolicy.For("mastodon");
            Assertions.True(mastodon.HasTrample && mastodon.TrampleDiceCount == 2 && mastodon.TrampleDieSides == 8 &&
                mastodon.TrampleBonus == 18 && mastodon.TrampleDc == 29,
                "Mastodon trample is its printed 2d8+18, Reflex DC 29 from 14 HD and Strength 34.");
            var elephant = UngulateRulesPolicy.For("elephant");
            Assertions.True(elephant.TrampleDiceCount == 2 && elephant.TrampleDieSides == 8 &&
                elephant.TrampleBonus == 15 && elephant.TrampleDc == 25, "Elephant stays 2d8+15, DC 25.");
            var aurochs = UngulateRulesPolicy.For("aurochs");
            Assertions.True(aurochs.HitDice == 3 && aurochs.Strength == 23 && aurochs.TrampleDiceCount == 2 &&
                aurochs.TrampleDieSides == 6, "Aurochs keeps its own row.");
            Assertions.Equal(1, ExpandedSummoningIdentityCatalog.Build().Count(value =>
                value.Symbol == SummonPlaytestR5Policy.MastodonTrampleSymbol && value.PlannedType == "BlueprintAbility"),
                "Exactly one registered Mastodon trample ability.");
        }

        internal static void GriffonHasOneNativeCharge()
        {
            var griffon = ExpandedSummoningNaturalProfiles.For("griffon");
            Assertions.False(griffon.Facts.Contains("NativeCharge"),
                "The Griffon no longer grants the native Charge the engine already gives every unit (two identical Charge actions).");
            Assertions.True(griffon.Facts.Contains("Pounce") && griffon.Facts.Contains("ReducedReach"),
                "Pounce and reach are unchanged.");
        }

        internal static void CloudFogRouteIsRetired()
        {
            var spec = SummonPlaytestWithdrawalPolicy.For("cloud-giant");
            Assertions.Equal("KMG.Summoning.Unit.CloudGiant", spec.UnitSymbol, "Cloud Giant fixer owner.");
            Assertions.True(spec.FactSymbols.SequenceEqual(new[] { "KMG.Summoning.Giant.Sprint29.CloudFogCloud" }) &&
                spec.AiActionSymbols.SequenceEqual(new[] { "KMG.Summoning.Giant.Sprint29.CloudFogAi" }) &&
                spec.ResourceSymbols.SequenceEqual(new[] { "KMG.Summoning.Giant.Sprint29.CloudFogResource" }),
                "Saved giants retire exactly the custom Fog Cloud fact, AI action and resource; the mist identity is reused.");
            Assertions.False(spec.FactSymbols.Contains("KMG.Summoning.Giant.Sprint29.CloudObscuringMist"),
                "The Obscuring Mist identity is not withdrawn: it becomes the provider-backed spell-like.");
        }
    }
}
