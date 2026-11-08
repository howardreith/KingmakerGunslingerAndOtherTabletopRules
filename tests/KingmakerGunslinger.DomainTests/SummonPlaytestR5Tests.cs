using System;
using System.Collections.Generic;
using System.IO;
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

        // ---- Repair R5 review: atomic, independent optional bindings ----

        // One owned optional ability driven through the production binding
        // path (SummonPlaytestR5Policy.BindOptional) with the binders' steps:
        // prepared components; donor field copy (which brings the donor's
        // visible state); hidden publication; verification; reveal last;
        // restoration of the snapshot placeholder in a finally.
        private sealed class FakeBinding
        {
            internal readonly bool Dance;
            internal string Name = "KMG_Owned", Range = "Placeholder";
            internal bool Hidden = true, Bound;
            internal string[] Components;
            internal readonly string SnapshotName, SnapshotRange;
            internal readonly string[] SnapshotComponents;
            internal string Disposition; internal Exception Failure;
            internal readonly List<string> RevealedWith = new List<string>();

            internal FakeBinding(bool dance)
            {
                Dance = dance;
                Components = dance ? new[] { "guard:dance", "cost:1" } : new[] { "guard:mist" };
                SnapshotName = Name; SnapshotRange = Range; SnapshotComponents = Components;
            }

            private string Guard { get { return Dance ? "guard:dance" : "guard:mist"; } }

            internal bool Guarded()
            {
                string[] components = Components ?? new string[0];
                bool guard = components.Count(value => value.StartsWith("guard:", StringComparison.Ordinal)) == 1 && components.Contains(Guard);
                bool cost = !Dance || components.Count(value => value.StartsWith("cost:", StringComparison.Ordinal)) == 1 && components.Contains("cost:1");
                return Hidden && guard && cost;
            }

            // A caster can use it only when visible, guarded by a binding
            // that reported active, and (Dance) spending its one use.
            internal bool Usable
            {
                get
                {
                    string[] components = Components ?? new string[0];
                    return !Hidden && Bound && components.Contains(Guard) && (!Dance || components.Contains("cost:1"));
                }
            }

            // Visible with no guard or (Dance) no cost: an unrestricted path.
            internal bool Unrestricted
            {
                get
                {
                    string[] components = Components ?? new string[0];
                    return !Hidden && (!components.Contains(Guard) || Dance && !components.Contains("cost:1"));
                }
            }

            internal void Run(params string[] failAt)
            {
                Func<string, bool> fails = point => failAt.Contains(point);
                string[] prepared = null;
                Exception failure;
                Disposition = SummonPlaytestR5Policy.BindOptional(
                    prepare: () =>
                    {
                        if (fails("prepare")) throw new InvalidOperationException("provider contract changed");
                        var components = new List<string> { "donor:effect", "donor:spawn" };
                        if (!fails("guard-missing")) components.Add(Guard);
                        if (Dance && !fails("cost-missing")) components.Add("cost:1");
                        components.Add("params");
                        prepared = components.ToArray();
                    },
                    publishHidden: () =>
                    {
                        Hidden = false; Range = "Donor"; Components = new[] { "donor:effect", "donor:spawn" };
                        if (fails("copy")) throw new InvalidOperationException("field copy failed");
                        Name = "KMG_Owned"; Hidden = true; Components = prepared;
                        if (fails("publish")) throw new InvalidOperationException("cache reset failed");
                    },
                    publishedGuarded: Guarded,
                    reveal: () =>
                    {
                        RevealedWith.Add(string.Join(",", Components));
                        Hidden = false;
                        if (fails("reveal")) throw new InvalidOperationException("reveal failed");
                    },
                    restorePlaceholder: () =>
                    {
                        try
                        {
                            if (fails("restore")) throw new InvalidOperationException("snapshot copy failed");
                            Name = SnapshotName; Range = SnapshotRange;
                        }
                        finally { Hidden = true; Components = SnapshotComponents; }
                    },
                    placeholderGuarded: Guarded,
                    failure: out failure);
                Failure = failure;
                Bound = Disposition == SummonPlaytestR5Policy.BindingActive;
            }
        }

        internal static void OptionalBindingFailureRestoresTheGuardedPlaceholder()
        {
            foreach (bool dance in new[] { false, true })
            {
                string label = dance ? "Dance" : "Mist";
                var success = new FakeBinding(dance);
                success.Run();
                Assertions.True(success.Disposition == SummonPlaytestR5Policy.BindingActive && success.Failure == null &&
                    success.Usable && !success.Unrestricted && success.Range == "Donor",
                    label + ": a successful binding publishes the provider replacement, guarded, and reveals it last.");
                Assertions.True(success.RevealedWith.Count == 1 && success.RevealedWith[0].Contains(dance ? "guard:dance" : "guard:mist") &&
                    (!dance || success.RevealedWith[0].Contains("cost:1")),
                    label + ": at reveal the replacement already carries its guard (and Dance its one-use cost).");
                var cases = new List<string[]> {
                    new[] { "prepare" }, new[] { "copy" }, new[] { "publish" }, new[] { "guard-missing" },
                    new[] { "reveal" }, new[] { "copy", "restore" }, new[] { "reveal", "restore" } };
                if (dance) cases.Add(new[] { "cost-missing" });
                foreach (string[] failAt in cases)
                {
                    string at = label + " failing at " + string.Join("+", failAt);
                    var binding = new FakeBinding(dance);
                    binding.Run(failAt);
                    Assertions.True(binding.Disposition == SummonPlaytestR5Policy.BindingOmitted && binding.Failure != null,
                        at + " is an omission with its failure retained.");
                    Assertions.True(!failAt.Contains("restore") || binding.Failure is AggregateException,
                        at + " retains both the binding and the restoration failure.");
                    Assertions.True(binding.Hidden && binding.Guarded() && !binding.Bound && !binding.Usable && !binding.Unrestricted,
                        at + " leaves the hidden, guarded, unavailable placeholder (no unrestricted path).");
                    Assertions.True(binding.Components.SequenceEqual(binding.SnapshotComponents),
                        at + " restores the placeholder's own guarded components.");
                    Assertions.True(failAt.Contains("restore") || binding.Name == binding.SnapshotName && binding.Range == binding.SnapshotRange,
                        at + " restores the placeholder's fields under its stable identity.");
                    Assertions.True(failAt[0] != "prepare" || binding.Range == "Placeholder" && binding.RevealedWith.Count == 0,
                        at + ": a preparation failure never touches the owned ability.");
                }
            }
        }

        internal static void OneFailedBindingDoesNotBlockTheOther()
        {
            foreach (string failAt in new[] { "prepare", "copy", "guard-missing", "reveal" })
            {
                var mist = new FakeBinding(false); var dance = new FakeBinding(true);
                mist.Run(failAt); dance.Run();
                Assertions.True(!mist.Usable && mist.Hidden && mist.Guarded() && dance.Usable, "Mist failing at " + failAt + " leaves Dance bound.");
                mist = new FakeBinding(false); dance = new FakeBinding(true);
                dance.Run(failAt); mist.Run();
                Assertions.True(!dance.Usable && dance.Hidden && dance.Guarded() && mist.Usable, "Dance failing at " + failAt + " leaves Mist bound.");
            }
            // The production binders and driver use exactly this path.
            string binders = Source("src/KingmakerGunslinger/Blueprints/ExpandedSummoningSpecialBuilder.SpellsR5.cs");
            string driver = Source("src/KingmakerGunslinger/Summoning/R5CotwSpells.cs");
            string[] reveals = binders.Split('\n').Where(line => line.Contains("Hidden = false")).ToArray();
            Assertions.True(reveals.Length == 2 && reveals.All(line => line.Contains("reveal:")),
                "The only visible publication of either binding is its final reveal step.");
            Assertions.Equal(2, Count(binders, "SummonPlaytestR5Policy.BindOptional("), "Both provider binders use the atomic binding.");
            Assertions.Equal(2, Count(binders, "OneUseCostR5(resource)"), "Dance's one-use cost is in its placeholder and its prepared replacement only.");
            int mistCall = driver.IndexOf("BindCotwObscuringMistR5(", StringComparison.Ordinal);
            int danceCall = driver.IndexOf("BindCotwIrresistibleDanceR5(", StringComparison.Ordinal);
            string between = mistCall < 0 || danceCall < mistCall ? null : driver.Substring(mistCall, danceCall - mistCall);
            Assertions.True(between != null && !between.Contains("return") && !between.Contains("throw"),
                "The Dance binding is attempted whatever the Mist binding's outcome.");
            Assertions.True(driver.Contains("MistBound = MistDisposition == SummonPlaytestR5Policy.BindingActive;") &&
                driver.Contains("DanceBound = DanceDisposition == SummonPlaytestR5Policy.BindingActive;"),
                "A provider guard can open only for a binding that reported active.");
        }

        private static int Count(string text, string value)
        {
            int count = 0;
            for (int index = text.IndexOf(value, StringComparison.Ordinal); index >= 0;
                index = text.IndexOf(value, index + value.Length, StringComparison.Ordinal)) count++;
            return count;
        }

        private static string Source(string relative)
        {
            return File.ReadAllText(Path.Combine(Environment.CurrentDirectory, relative.Replace('/', Path.DirectorySeparatorChar)));
        }
    }
}
