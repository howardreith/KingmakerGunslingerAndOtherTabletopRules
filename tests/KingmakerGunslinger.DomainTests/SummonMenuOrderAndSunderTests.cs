using System;
using System.Collections.Generic;
using System.Linq;
using KingmakerGunslinger.RuntimeTesting;
using KingmakerGunslinger.Summoning;

namespace KingmakerGunslinger.DomainTests
{
    /// <summary>
    /// Owner corrections of 2026-11-10: quantity-first summon menus, no Sunder
    /// on any supported summon, and the "Azata, Lillend" label. Historical
    /// name-first and Storm Giant verdicts stay with their own candidates.
    /// </summary>
    internal static class SummonMenuOrderAndSunderTests
    {
        private sealed class Row
        {
            internal Row(string id, string name, SummonMultiplicity? quantity, SpiderPlacement spider)
            { Id = id; Name = name; Quantity = quantity; Spider = spider; }
            internal string Id { get; private set; }
            internal string Name { get; private set; }
            internal SummonMultiplicity? Quantity { get; private set; }
            internal SpiderPlacement Spider { get; private set; }
            internal SummonMenuOrderKey Key;
        }

        private static readonly string[] Roman = { "I", "II", "III", "IV", "V", "VI", "VII", "VIII", "IX" };

        private static int Group(SummonMultiplicity? quantity)
        { return !quantity.HasValue ? 3 : (int)quantity.Value; }

        // The canonical published rows of one menu, keyed as the publisher keys them.
        private static List<Row> Canonical(SummonFamily family, int tier)
        {
            return ExpandedSummoningCatalog.GenerateVariants(family)
                .Where(value => value.ParentTier == tier && SummonVisibilityCatalog.IsPublished(value))
                .Select(value =>
                {
                    SummonChoicePresentation presentation = SummonChoicePresentationPolicy.For(
                        value.Creature.Key, value.Multiplicity);
                    return new Row(value.StableKey, presentation.Label, value.Multiplicity, null)
                    { Key = SummonMenuOrderKey.Owned(presentation, value.StableKey) };
                }).ToList();
        }

        // Spider rows as the integration publishes them: an original keeps its
        // Races Unleashed label and identity, and takes its quantity from the
        // matrix; a generated row is owned and keyed by its placement.
        private static List<Row> Spiders(SummonFamily family, int tier)
        {
            return SpiderSummonerCatalog.Placements(family).Where(value => value.Tier == tier).Select(value =>
            {
                if (value.IsOriginal)
                {
                    string identity = SpiderSummonerCatalog.RowIdentity(family, value.Species);
                    string label = (family == SummonFamily.Monster ? "Summon Monster " : "Summon Nature's Ally ") +
                        Roman[tier - 1] + " (" + value.Species.DisplayName + ")";
                    SummonMultiplicity? quantity = SpiderSummonerCatalog.OriginalRowQuantity(family, tier, identity);
                    return new Row(identity, label, quantity, value)
                    { Key = SummonMenuOrderKey.Host(label, quantity, identity, true) };
                }
                SummonChoicePresentation presentation = SummonChoicePresentationPolicy.For(value.Species.Key, value.Quantity);
                return new Row(value.Symbol, presentation.Label, value.Quantity, value)
                { Key = SummonMenuOrderKey.Owned(presentation, value.StableKey) };
            }).ToList();
        }

        private static void AssertGrouped(IList<Row> ordered, string scope)
        {
            for (int index = 1; index < ordered.Count; index++)
            {
                int previous = Group(ordered[index - 1].Quantity), current = Group(ordered[index].Quantity);
                Assertions.True(previous < current || (previous == current && string.Compare(
                    ordered[index - 1].Key.Name, ordered[index].Key.Name, StringComparison.OrdinalIgnoreCase) <= 0),
                    scope + " is out of quantity-then-name order at " + index + ": " +
                    ordered[index - 1].Name + " / " + ordered[index].Name);
            }
        }

        // The SM VII acceptance example: the single group leads, Bebelith is in
        // it, and Ankylosaurus (1d4+1) is in the last group, not first.
        internal static void SummonMonsterSevenStartsWithItsSingleGroup()
        {
            List<Row> rows = Canonical(SummonFamily.Monster, 7);
            IReadOnlyList<Row> ordered = SummonDisplayOrderPolicy.Order(new Row[0], rows, value => value.Key);
            AssertGrouped(ordered.ToList(), "Summon Monster VII");
            Assertions.Equal(SummonMultiplicity.One.ToString(), ordered[0].Quantity.ToString(),
                "Summon Monster VII starts with a single summon.");
            Row bebelith = ordered.Single(value => value.Name == "Bebelith");
            Row ankylosaurus = ordered.Single(value => value.Name == "Ankylosaurus (1d4+1)");
            int lastSingle = ordered.ToList().FindLastIndex(value => value.Quantity == SummonMultiplicity.One);
            int firstGroup = ordered.ToList().FindIndex(value => value.Quantity != SummonMultiplicity.One);
            Assertions.True(ordered.ToList().IndexOf(bebelith) <= lastSingle && lastSingle < firstGroup,
                "Bebelith sits in the single group, which precedes every group summon.");
            Assertions.True(ordered.ToList().IndexOf(ankylosaurus) > ordered.ToList().FindLastIndex(
                value => value.Quantity == SummonMultiplicity.OneD3), "Ankylosaurus (1d4+1) sorts in the 1d4+1 group.");
            Assertions.False(ReferenceEquals(ordered[0], ankylosaurus), "Ankylosaurus no longer leads the menu.");
            Assertions.Equal(rows.Count, ordered.Distinct().Count(), "Ordering lost or repeated a choice.");
            IReadOnlyList<Row> reversed = SummonDisplayOrderPolicy.Order(new Row[0],
                rows.AsEnumerable().Reverse().ToArray(), value => value.Key);
            Assertions.True(ordered.SequenceEqual(reversed), "The order is independent of input order.");
        }

        // All 46 placements: each original and generated row lands in its own
        // matrix quantity group, with membership and identities unchanged.
        internal static void SpiderPlacementsSortIntoTheirQuantityGroups()
        {
            foreach (SummonFamily family in SpiderSummonerCatalog.Families)
            {
                int spiders = 0, originals = 0;
                for (int tier = 1; tier <= 9; tier++)
                {
                    List<Row> spiderRows = Spiders(family, tier);
                    List<Row> all = Canonical(family, tier).Concat(spiderRows).ToList();
                    // The late pass orders an array that already holds the appended rows.
                    Row[] ordered = SummonDisplayOrderPolicy.Order(all, new Row[0], value => value.Key).ToArray();
                    AssertGrouped(ordered, family + " " + tier);
                    Assertions.Equal(all.Count, ordered.Distinct().Count(), "Spider ordering lost or repeated a row.");
                    Assertions.True(SummonDisplayOrderPolicy.ReorderedOrNull(ordered, value => value.Key) == null,
                        "A repeated late pass changes nothing.");
                    foreach (Row spider in spiderRows)
                    {
                        Assertions.Equal(spider.Spider.Quantity.ToString(), spider.Quantity.ToString(),
                            "A spider row is ordered by its own matrix quantity: " + spider.Id);
                        Assertions.Equal(spider.Spider.IsOriginal ? SpiderSummonerCatalog.RowIdentity(family, spider.Spider.Species) :
                            spider.Spider.Symbol, spider.Id, "A spider row keeps its own identity.");
                    }
                    spiders += spiderRows.Count; originals += spiderRows.Count(value => value.Spider.IsOriginal);
                }
                Assertions.Equal(23, spiders, family + " keeps 23 spider placements.");
                Assertions.Equal(4, originals, family + " keeps its four original rows.");
            }
            Assertions.Equal(46, SpiderSummonerCatalog.AllPlacements.Count(), "The matrix keeps 46 placements.");
            Assertions.Equal(38, SpiderSummonerCatalog.Generated.Count(), "38 placements are generated.");
        }

        internal static void TierFiveMixedSpiderMenuGroupsEachSpecies()
        {
            SpiderSpeciesSpec ogre = SpiderSummonerCatalog.Find("ogre-spider");
            string ogreIdentity = SpiderSummonerCatalog.RowIdentity(SummonFamily.Monster, ogre);
            Assertions.Equal(SummonMultiplicity.One.ToString(),
                SpiderSummonerCatalog.OriginalRowQuantity(SummonFamily.Monster, 5, ogreIdentity).ToString(),
                "The original Ogre Spider row is a single summon at its base tier.");
            Assertions.False(SpiderSummonerCatalog.OriginalRowQuantity(SummonFamily.Monster, 6, ogreIdentity).HasValue,
                "An original identity is recognized only at its own tier.");
            Assertions.False(SpiderSummonerCatalog.OriginalRowQuantity(SummonFamily.NaturesAlly, 5, ogreIdentity).HasValue,
                "An original identity is recognized only in its own family.");
            Assertions.False(SpiderSummonerCatalog.OriginalRowQuantity(SummonFamily.Monster, 5, "not-a-spider").HasValue ||
                SpiderSummonerCatalog.OriginalRowQuantity(SummonFamily.Monster, 5, null).HasValue,
                "Any other row has no spider quantity.");
            foreach (SummonFamily family in SpiderSummonerCatalog.Families)
            {
                Row[] ordered = SummonDisplayOrderPolicy.Order(Canonical(family, 5).Concat(Spiders(family, 5)).ToList(),
                    new Row[0], value => value.Key).ToArray();
                Row single = ordered.Single(value => value.Spider != null && value.Spider.Species.Key == "ogre-spider");
                Row widow = ordered.Single(value => value.Spider != null && value.Spider.Species.Key == "giant-black-widow");
                Row crab = ordered.Single(value => value.Spider != null && value.Spider.Species.Key == "giant-crab-spider");
                Assertions.Equal("Spider, Giant Black Widow (1d3)", widow.Name, "Widow label at tier V.");
                Assertions.Equal("Spider, Giant Crab (1d4+1)", crab.Name, "Crab label at tier V.");
                Assertions.True(single.Quantity == SummonMultiplicity.One && widow.Quantity == SummonMultiplicity.OneD3 &&
                    crab.Quantity == SummonMultiplicity.OneD4PlusOne, "Each species sits in its own quantity group.");
                int s = Array.IndexOf(ordered, single), w = Array.IndexOf(ordered, widow), c = Array.IndexOf(ordered, crab);
                Assertions.True(s < w && w < c, "Single, then 1d3, then 1d4+1 at " + family + " V.");
                foreach (Row row in new[] { single, widow, crab })
                {
                    int index = Array.IndexOf(ordered, row);
                    Row[] group = ordered.Where(value => value.Quantity == row.Quantity).ToArray();
                    Assertions.True(group.Select(value => value.Key.Name).SequenceEqual(group.Select(value => value.Key.Name)
                        .OrderBy(value => value, StringComparer.OrdinalIgnoreCase)), "Group of " + row.Name + " is alphabetical.");
                    Assertions.True(index >= Array.IndexOf(ordered, group.First()) && index <= Array.IndexOf(ordered, group.Last()),
                        row.Name + " stays inside its group.");
                }
            }
        }

        // Druid (Nature's Ally) and Herald (Summon Monster) conversion popups
        // reuse the parent's positions per quantity conversion, so each slot's
        // flattened list follows the same quantity groups, including spiders.
        internal static void SpiderConversionsFollowTheParentGroups()
        {
            foreach (SummonFamily family in SpiderSummonerCatalog.Families)
                for (int tier = 2; tier <= 9; tier++)
                {
                    Row[] parent = SummonDisplayOrderPolicy.Order(Canonical(family, tier).Concat(Spiders(family, tier)).ToList(),
                        new Row[0], value => value.Key).ToArray();
                    // Each native quantity conversion projects its own quantity
                    // only; the projection concatenates them in conversion order.
                    Row[] grouped = new[] { SummonMultiplicity.OneD4PlusOne, SummonMultiplicity.One, SummonMultiplicity.OneD3 }
                        .SelectMany(quantity => parent.Where(value => value.Quantity == quantity).Reverse()).ToArray();
                    int scopedTier = tier;
                    Row[] flattened = DruidSummonConversionPolicy.OrderProjectedChoices(grouped,
                        row => scopedTier * 100000 + Array.IndexOf(parent, row));
                    Assertions.True(flattened.SequenceEqual(parent.Where(value => value.Quantity.HasValue)),
                        family + " " + tier + " conversion order differs from its parent.");
                    Assertions.Equal(flattened.Length, flattened.Distinct().Count(), "A conversion row is repeated.");
                }
        }

        internal static void LillendUsesItsFamilyFirstLabel()
        {
            Assertions.Equal("Azata, Lillend", SummonChoicePresentationPolicy.For("lillend-azata", SummonMultiplicity.One).Label, "Single label.");
            Assertions.Equal("Azata, Lillend (1d3)", SummonChoicePresentationPolicy.For("lillend-azata", SummonMultiplicity.OneD3).Label, "1d3 label.");
            Assertions.Equal("Azata, Lillend (1d4+1)", SummonChoicePresentationPolicy.For("lillend-azata", SummonMultiplicity.OneD4PlusOne).Label, "1d4+1 label.");
            Assertions.Equal("Azata, Bralani", SummonChoicePresentationPolicy.For("bralani-azata").Label, "Bralani unchanged.");
            Assertions.Equal("Azata, Ghaele", SummonChoicePresentationPolicy.For("ghaele-azata").Label, "Ghaele unchanged.");
            Assertions.Equal("lillend-azata", SummonChoicePresentationPolicy.For("lillend-azata").Key, "The creature key is unchanged.");
            int checkedMenus = 0;
            for (int tier = 6; tier <= 9; tier++)
            {
                Row[] ordered = SummonDisplayOrderPolicy.Order(new Row[0], Canonical(SummonFamily.Monster, tier),
                    value => value.Key).ToArray();
                Row lillend = ordered.SingleOrDefault(value => value.Name.StartsWith("Azata, Lillend", StringComparison.Ordinal));
                if (lillend == null) continue;
                AssertGrouped(ordered, "Summon Monster " + tier);
                Row[] group = ordered.Where(value => value.Quantity == lillend.Quantity).ToArray();
                int position = Array.IndexOf(group, lillend);
                Assertions.True(position == 0 || string.Compare(group[position - 1].Key.Name, "Azata, Lillend", StringComparison.OrdinalIgnoreCase) < 0,
                    "Lillend sorts after its predecessor at tier " + tier + ".");
                checkedMenus++;
            }
            Assertions.Equal(4, checkedMenus, "Lillend appears in Summon Monster VI-IX.");
        }

        // SUNDER-01: every supported summon that carried Improved Sunder loses
        // it; the elementals also lose the inherited Sunder AI action; nothing
        // else is withdrawn and the native identities are exact.
        internal static void SunderIsWithdrawnFromEverySummonThatHadIt()
        {
            const string sunder = SummonPlaytestWithdrawalPolicy.NativeImprovedSunderGuid;
            Assertions.Equal("9719015edcbf142409592e2cbaab7fe1", sunder, "Native Improved Sunder identity.");
            Assertions.Equal("23bcb18f2dd735c4d9d2d228f654efce", SummonPlaytestWithdrawalPolicy.NativeSunderArmorAiActionGuid,
                "Native SunderArmorAiAction identity.");
            Assertions.Equal("d8342bca1ce38294bbf47f9566e6e2aa", SummonPlaytestWithdrawalPolicy.NativeGreaterEarthElementalBrainGuid,
                "Native GreaterEarthElementalBrain identity.");
            Assertions.Equal("5abc8884c6f15204c8604cb01a2efbab", SummonPlaytestWithdrawalPolicy.NativeDumbMonsterBrainGuid,
                "Native Dumb Monster Brain identity.");
            foreach (string key in new[] { "fire-giant", "storm-giant", "greater-earth-elemental", "elder-earth-elemental" })
            {
                Assertions.True(SummonPlaytestWithdrawalPolicy.WithdrawsSunderFrom(key), key + " never sunders.");
                Assertions.True(ExpandedSummoningCatalog.All.Any(value => value.Key == key), key + " is a catalog creature.");
            }
            foreach (string key in new[] { "hill-giant", "stone-giant", "cloud-giant", "huge-earth-elemental", "bebelith", "lillend-azata" })
                Assertions.False(SummonPlaytestWithdrawalPolicy.WithdrawsSunderFrom(key), key + " had no Sunder grant to withdraw.");
            Assertions.True(SummonPlaytestWithdrawalPolicy.DonorInheritedSunderKeys.SequenceEqual(
                new[] { "greater-earth-elemental", "elder-earth-elemental" }), "Exactly the two donor-inherited carriers.");
            foreach (string key in SummonPlaytestWithdrawalPolicy.DonorInheritedSunderKeys)
            {
                WithdrawnCapabilitySpec spec = SummonPlaytestWithdrawalPolicy.For(key);
                Assertions.True(spec.NativeAiActionGuids.SequenceEqual(new[] { SummonPlaytestWithdrawalPolicy.NativeSunderArmorAiActionGuid }) &&
                    spec.FactSymbols.Count == 0 && spec.AiActionSymbols.Count == 0 && spec.ResourceSymbols.Count == 0,
                    key + " withdraws only the native feat and the native Sunder AI action.");
            }
            Assertions.Equal(0, SummonPlaytestWithdrawalPolicy.For("storm-giant").NativeAiActionGuids.Count,
                "Storm Giant's brain never held a Sunder AI action.");
            Assertions.Equal(0, SummonPlaytestWithdrawalPolicy.For("fire-giant").NativeAiActionGuids.Count,
                "The earlier Fire Giant cut is unchanged.");
            string[] elemental = { "power-attack", sunder, "toughness", "blind-fight" };
            Assertions.True(SummonPlaytestWithdrawalPolicy.GrantedNativeFeats("greater-earth-elemental", elemental)
                .SequenceEqual(new[] { "power-attack", "toughness", "blind-fight" }), "Only Improved Sunder leaves the elemental.");
            Assertions.True(SummonPlaytestWithdrawalPolicy.GrantedNativeFeats("huge-earth-elemental", elemental)
                .SequenceEqual(elemental), "An unaffected creature keeps its list.");
        }

        // The new fixers have names no unit has applied before, so an older
        // saved summon is reconciled once; the earlier names are unchanged.
        internal static void SunderFixersUseNewStableNames()
        {
            Assertions.Equal("$KMG_R1_WithdrawnCapabilities_StormGiant", SummonPlaytestWithdrawalPolicy.FixerName("storm-giant"), "Storm Giant fixer name.");
            Assertions.Equal("$KMG_R1_WithdrawnCapabilities_GreaterEarthElemental", SummonPlaytestWithdrawalPolicy.FixerName("greater-earth-elemental"), "Greater Earth Elemental fixer name.");
            Assertions.Equal("$KMG_R1_WithdrawnCapabilities_ElderEarthElemental", SummonPlaytestWithdrawalPolicy.FixerName("elder-earth-elemental"), "Elder Earth Elemental fixer name.");
            Assertions.Equal("$KMG_R1_WithdrawnCapabilities_FireGiant", SummonPlaytestWithdrawalPolicy.FixerName("fire-giant"), "The Fire Giant name stays fixed.");
            string[] earlier = { "bone-devil", "ice-devil", "magma-mephit", "lillend-azata", "fire-giant", "cloud-giant" };
            Assertions.False(earlier.Contains("storm-giant") || earlier.Contains("greater-earth-elemental") || earlier.Contains("elder-earth-elemental"),
                "None of the new units carried an earlier fixer whose completion would skip this withdrawal.");
            Assertions.True(earlier.All(key => SummonPlaytestWithdrawalPolicy.For(key) != null), "Earlier withdrawals stay registered.");
            string[] names = SummonPlaytestWithdrawalPolicy.All.Select(value => SummonPlaytestWithdrawalPolicy.FixerName(value.CreatureKey)).ToArray();
            Assertions.Equal(names.Length, names.Distinct(StringComparer.Ordinal).Count(), "Fixer names stay unique.");
        }

        private sealed class Fact { internal Fact(string name) { Name = name; } internal string Name; }

        // The legacy reconciliation decision: only the exact owner, only the
        // withdrawn references, and nothing more on a second pass. A native
        // hostile elemental sharing the feat and brain action is not an owner.
        internal static void LegacySunderCleanupIsExactAndIdempotent()
        {
            var improvedSunder = new Fact("ImprovedSunder"); var sunderAi = new Fact("SunderArmorAiAction");
            var powerAttack = new Fact("PowerAttack"); var attackAi = new Fact("GreaterEarthElementalAttackAiAction");
            var sameName = new Fact("ImprovedSunder");
            Fact[] savedFacts = { powerAttack, improvedSunder, sameName };
            Fact[] savedActions = { sunderAi, attackAi };
            Fact[] removedFacts = SummonPlaytestWithdrawalPolicy.SelectWithdrawn(true, savedFacts, new[] { improvedSunder });
            Fact[] removedActions = SummonPlaytestWithdrawalPolicy.SelectWithdrawn(true, savedActions, new[] { sunderAi });
            Assertions.True(removedFacts.SequenceEqual(new[] { improvedSunder }), "Only the exact feat object is removed.");
            Assertions.True(removedActions.SequenceEqual(new[] { sunderAi }), "Only the exact Sunder AI action is removed; the attack stays.");
            Fact[] remainingFacts = savedFacts.Except(removedFacts).ToArray();
            Fact[] remainingActions = savedActions.Except(removedActions).ToArray();
            Assertions.Equal(0, SummonPlaytestWithdrawalPolicy.SelectWithdrawn(true, remainingFacts, new[] { improvedSunder }).Length +
                SummonPlaytestWithdrawalPolicy.SelectWithdrawn(true, remainingActions, new[] { sunderAi }).Length,
                "A second reconciliation finds nothing.");
            Assertions.Equal(0, SummonPlaytestWithdrawalPolicy.SelectWithdrawn(false, savedFacts, new[] { improvedSunder }).Length +
                SummonPlaytestWithdrawalPolicy.SelectWithdrawn(false, savedActions, new[] { sunderAi }).Length,
                "A unit that is not the exact KMG owner is never touched.");
        }

        private static SunderUnitState Unit(bool feat, bool action, bool toggle, bool running, int buffActive)
        {
            return new SunderUnitState { Feat = feat, Action = action, TogglePresent = toggle, ToggleOn = toggle,
                ToggleRunning = running, BuffPresent = buffActive, BuffActive = buffActive };
        }

        private static SunderProviderChain Provider(bool active, bool exact)
        {
            return SunderObservationVerdict.ClassifyProvider(active, exact, true, exact, exact, exact, exact, true, exact, exact, exact);
        }

        // Active provider with the exact chain: the claimed pre-state, then absence.
        internal static void SunderVerdictActiveExactChainProvesCleanup()
        {
            SunderProviderChain chain = Provider(true, true);
            Assertions.Equal(SunderProviderChain.ActiveExact.ToString(), chain.ToString(), "Exact wiring with an active provider.");
            SunderUnitState live = Unit(true, true, true, true, 1), gone = Unit(false, false, false, false, 0);
            Assertions.Equal(SunderCleanupOutcome.Proven.ToString(),
                SunderObservationVerdict.ClassifyCleanup(chain, true, live, gone).ToString(), "Live chain before, all gone after.");
            Assertions.True(SunderObservationVerdict.AbsenceAssertable(chain, true, true), "Absence is assertable on a verified chain.");
            Assertions.Equal(SunderCleanupOutcome.NotRemoved.ToString(),
                SunderObservationVerdict.ClassifyCleanup(chain, true, live, Unit(false, false, false, false, 1)).ToString(),
                "A replacement buff left behind is not a removal.");
            Assertions.Equal(SunderCleanupOutcome.Proven.ToString(),
                SunderObservationVerdict.ClassifyCleanup(chain, false, Unit(true, true, true, false, 0), gone).ToString(),
                "Where only feat and action are claimed, a present but idle toggle need not run, yet must be gone after.");
            Assertions.Equal(SunderCleanupOutcome.NotRemoved.ToString(),
                SunderObservationVerdict.ClassifyCleanup(chain, false, Unit(true, true, true, false, 0), Unit(false, false, true, false, 0)).ToString(),
                "A toggle left behind is not a removal.");
        }

        // Active provider, any missing or mismatched identity or link: never a removal proof.
        internal static void SunderVerdictActiveMismatchFails()
        {
            for (int index = 0; index < 10; index++)
            {
                bool[] parts = Enumerable.Repeat(true, 10).ToArray();
                parts[index] = false;
                SunderProviderChain chain = SunderObservationVerdict.ClassifyProvider(true, parts[0], parts[1], parts[2], parts[3],
                    parts[4], parts[5], parts[6], parts[7], parts[8], parts[9]);
                Assertions.Equal(SunderProviderChain.ActiveMismatch.ToString(), chain.ToString(), "Missing part " + index + " is a mismatch.");
                Assertions.Equal(SunderCleanupOutcome.ChainUnverified.ToString(), SunderObservationVerdict.ClassifyCleanup(chain, true,
                    Unit(true, true, true, true, 1), Unit(false, false, false, false, 0)).ToString(), "A mismatched chain proves no cleanup.");
                Assertions.False(SunderObservationVerdict.AbsenceAssertable(chain, true, true), "Absence is not assertable on a mismatched chain.");
            }
            Assertions.False(SunderObservationVerdict.AbsenceAssertable(SunderProviderChain.ActiveExact, true, false),
                "An unresolved Sunder action can never read as an absent fact.");
            Assertions.False(SunderObservationVerdict.AbsenceAssertable(SunderProviderChain.ActiveExact, false, true),
                "An unresolved feat can never read as an absent fact.");
        }

        // A genuinely inactive provider is a qualified, fact-level reading only.
        internal static void SunderVerdictInactiveProviderIsQualified()
        {
            SunderProviderChain inactive = SunderObservationVerdict.ClassifyProvider(false, true, true, true, false, false, false, false, false, false, false);
            Assertions.Equal(SunderProviderChain.Inactive.ToString(), inactive.ToString(), "Provider inactive with no identities.");
            Assertions.Equal(SunderCleanupOutcome.ProvenFactLevelProviderInactive.ToString(), SunderObservationVerdict.ClassifyCleanup(inactive, true,
                Unit(true, true, false, false, 0), Unit(false, false, false, false, 0)).ToString(), "Feat and action only, explicitly qualified.");
            SunderProviderChain partial = SunderObservationVerdict.ClassifyProvider(false, true, true, true, true, false, false, false, false, false, false);
            Assertions.Equal(SunderProviderChain.InactiveIdentitiesPresent.ToString(), partial.ToString(), "Inactive yet identities present.");
            Assertions.Equal(SunderCleanupOutcome.ChainUnverified.ToString(), SunderObservationVerdict.ClassifyCleanup(partial, true,
                Unit(true, true, false, false, 0), Unit(false, false, false, false, 0)).ToString(), "Inactive-with-identities is unverified.");
        }

        // The claimed pre-removal state must actually have been observed.
        internal static void SunderVerdictMissingPreStateIsNotAProof()
        {
            SunderProviderChain chain = Provider(true, true);
            SunderUnitState gone = Unit(false, false, false, false, 0);
            foreach (SunderUnitState before in new[] { Unit(false, true, true, true, 1), Unit(true, false, true, true, 1),
                Unit(true, true, false, false, 0), Unit(true, true, true, false, 1), Unit(true, true, true, true, 0), gone })
                Assertions.Equal(SunderCleanupOutcome.PreconditionMissing.ToString(),
                    SunderObservationVerdict.ClassifyCleanup(chain, true, before, gone).ToString(), "An unobserved claimed pre-state is not a cleanup proof.");
            Assertions.Equal(SunderCleanupOutcome.PreconditionMissing.ToString(),
                SunderObservationVerdict.ClassifyCleanup(chain, false, null, gone).ToString(), "No reading is no proof.");
            Assertions.False(SunderObservationVerdict.Passes(SunderCleanupOutcome.PreconditionMissing) ||
                SunderObservationVerdict.Passes(SunderCleanupOutcome.ChainUnverified) || SunderObservationVerdict.Passes(SunderCleanupOutcome.NotRemoved),
                "Only the two removal outcomes pass.");
        }
    }
}
