using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.DomainTests
{
    /// <summary>
    /// Alternate summoning sources: one roster projected onto every caller,
    /// stable caller-specific identities, canonical-equivalent menu
    /// composition, and fail-closed lifecycle and rollback.
    /// </summary>
    internal static class AlternateSummoningTests
    {
        private static readonly AlternateSummoningRoster Production =
            AlternateSummoningRoster.Production();

        /// <summary>The adapter block appended to the frozen ledger: every
        /// family's projected identities. Earlier sprints locate their own
        /// blocks by counting back past it.</summary>
        /// <summary>Adapter identities are created by the projection, not
        /// the canonical identity catalog; canonical ledger scans leave them
        /// to <see cref="IdentitiesAreDisjointDeterministicAndRegistered"/>.</summary>
        internal static bool IsAdapterIdentity(string symbol)
        {
            return symbol != null && symbol.StartsWith(
                AlternateSummoningProjection.SymbolPrefix, StringComparison.Ordinal);
        }

        internal static int AppendedLedgerIdentities
        {
            get
            {
                return AlternateSummoningSourceCatalog.Identities.Sum(family =>
                    AlternateSummoningProjection.Identities(family,
                        AlternateSummoningRoster.Production()).Count);
            }
        }

        internal static void CatalogIsExactAndComplete()
        {
            AlternateSummoningSourceCatalog.Validate();
            string[] keys = AlternateSummoningSourceCatalog.All.Select(value =>
                value.Key).ToArray();
            Assertions.True(keys.SequenceEqual(new[] { "MonsterTactician",
                "Summoner", "MasterSummoner", "Occultist", "Naturalist",
                "FeralHunter", "Preservationist" }), "Adapter families changed: " +
                string.Join(",", keys));
            foreach (AlternateSummoningFamilySpec family in
                AlternateSummoningSourceCatalog.All)
            {
                foreach (AlternateSummoningRootSpec root in family.Roots)
                {
                    if (family.Provider == AlternateSummoningProvider.Native)
                    {
                        Assertions.True(root.RootGuid == null &&
                            root.FeatureGuid == AlternateSummoningSourceCatalog
                                .MonsterTacticianFeatureGuids[root.Tier - 1],
                            "Native roots are resolved only through the exact tactician feature.");
                        continue;
                    }
                    Assertions.Equal(root.Tier == 1, root.IsDirect,
                        family.Key + " tier " + root.Tier + " direct shape.");
                    Assertions.True(root.Templates.Select(value =>
                        value.Multiplicity).SequenceEqual(AlternateSummoningSourceCatalog
                            .Quantities(root.Tier).Where(value => root.Tier > 1)),
                        family.Key + " tier " + root.Tier + " must pin one template per quantity.");
                }
            }
        }

        internal static void PreservationistKeepsTheExtractToTierMapping()
        {
            AlternateSummoningFamilySpec family = AlternateSummoningSourceCatalog
                .Find(AlternateSummoningSourceCatalog.PreservationistKey);
            Assertions.Equal(SummonFamily.NaturesAlly, family.Family,
                "Bottled Ally summons Nature's Ally creatures.");
            Assertions.True(family.Roots.Select(value => value.Ordinal + ":" +
                value.Tier).SequenceEqual(new[] { "1:1", "2:2", "3:4", "4:5", "5:7",
                    "6:9" }), "Extract levels 1-6 must cast SNA I, II, IV, V, VII and IX.");
            Assertions.True(family.Roots.Single(value => value.Ordinal == 1).IsDirect,
                "The first extract copies the direct Summon Nature's Ally I spell.");
        }

        /// <summary>
        /// Every caller offers, at every tier and quantity, exactly the
        /// published creatures the ordinary spell of that family offers there.
        /// </summary>
        internal static void EveryCallerMatchesTheOrdinarySpellAtEveryTier()
        {
            foreach (AlternateSummoningFamilyIdentity family in
                AlternateSummoningSourceCatalog.Identities)
            foreach (int tier in family.Tiers)
            foreach (SummonMultiplicity quantity in AlternateSummoningSourceCatalog
                .Quantities(tier))
            {
                string[] expected = ExpandedSummoningCatalog.GenerateVariants(
                        family.Family).Where(value => value.ParentTier == tier &&
                        value.Multiplicity == quantity &&
                        SummonVisibilityCatalog.IsPublished(value))
                    .Select(value => value.Creature.Key).ToArray();
                string[] observed = AlternateSummoningProjection.Exposed(family,
                        Production, tier, quantity)
                    .Select(value => value.Placement.Creature.Key).ToArray();
                Assertions.True(expected.Length > 0 || quantity !=
                    SummonMultiplicity.One, family.Key + " tier " + tier +
                    " offers no single creature.");
                Assertions.True(expected.SequenceEqual(observed), family.Key +
                    " tier " + tier + " " + quantity + " diverged from the ordinary spell.");
            }
        }

        internal static void NewPlacementReachesOnlyEligibleCallers()
        {
            var beast = new SummonCreatureSpec("test-only-sna-beast", "Test Beast",
                null, false, 4, null);
            var fiend = new SummonCreatureSpec("test-only-sm-fiend", "Test Fiend",
                3, true, null, null);
            AlternateSummoningRoster roster = WithCreatures(beast, fiend);
            foreach (AlternateSummoningFamilyIdentity family in
                AlternateSummoningSourceCatalog.Identities)
            {
                AlternateSummoningChoice[] beasts = AlternateSummoningProjection
                    .Identities(family, roster).Where(value =>
                        value.Kind == AlternateSummoningChoiceKind.Creature &&
                        value.Placement.Creature.Key == beast.Key).ToArray();
                AlternateSummoningChoice[] fiends = AlternateSummoningProjection
                    .Identities(family, roster).Where(value =>
                        value.Kind == AlternateSummoningChoiceKind.Creature &&
                        value.Placement.Creature.Key == fiend.Key).ToArray();
                if (family.Family == SummonFamily.Monster)
                {
                    Assertions.Equal(0, beasts.Length, family.Key +
                        " gained a Summon Monster route for an SNA-only creature.");
                    Assertions.Equal(family.Tiers.Count(value => value >= 3),
                        fiends.Length, family.Key + " fiend placements.");
                }
                else
                {
                    Assertions.Equal(0, fiends.Length, family.Key +
                        " gained an SNA route for an SM-only creature.");
                    Assertions.Equal(family.Tiers.Count(value => value >= 4),
                        beasts.Length, family.Key + " beast placements.");
                    foreach (AlternateSummoningChoice choice in beasts)
                        Assertions.Equal(choice.Tier == 4 ? SummonMultiplicity.One :
                            choice.Tier == 5 ? SummonMultiplicity.OneD3 :
                                SummonMultiplicity.OneD4PlusOne, choice.Multiplicity,
                            "The ordinary quantity rule must decide the beast's count.");
                    Assertions.True(beasts.All(value => value.Symbol ==
                        "KMG.Summoning.Adapter." + family.Key + ".Tier" + value.Tier +
                        ".TestOnlySnaBeast." + value.Multiplicity), "Unstable beast symbol.");
                }
            }
        }

        internal static void WithholdingChangesExposureButNotIdentity()
        {
            var beast = new SummonCreatureSpec("test-only-sna-beast", "Test Beast",
                null, false, 4, null);
            AlternateSummoningRoster published = WithCreatures(beast);
            AlternateSummoningRoster withheld = WithCreatures(new[] { beast },
                value => value.Creature.Key != beast.Key &&
                    SummonVisibilityCatalog.IsPublished(value));
            foreach (AlternateSummoningFamilyIdentity family in
                AlternateSummoningSourceCatalog.Identities.Where(value =>
                    value.Family == SummonFamily.NaturesAlly))
            {
                string[] before = AlternateSummoningProjection.Identities(family,
                    published).Select(value => value.Symbol).ToArray();
                string[] after = AlternateSummoningProjection.Identities(family,
                    withheld).Select(value => value.Symbol).ToArray();
                Assertions.True(before.SequenceEqual(after), family.Key +
                    " moved an identity when a creature was withheld.");
                foreach (int tier in family.Tiers.Where(value => value >= 4))
                {
                    Assertions.True(AlternateSummoningProjection.Exposed(family,
                        published, tier, null).Any(value =>
                            value.Placement.Creature.Key == beast.Key),
                        family.Key + " did not expose a published placement.");
                    Assertions.False(AlternateSummoningProjection.Exposed(family,
                        withheld, tier, null).Any(value =>
                            value.Placement.Creature.Key == beast.Key),
                        family.Key + " exposed a withheld placement.");
                }
            }
        }

        /// <summary>A later fidelity update changes a creature's body, art or
        /// name - never its key or tiers - so its routes stay where they are.</summary>
        internal static void FidelityUpdateKeepsPublishedRoutes()
        {
            SummonCreatureSpec original = ExpandedSummoningCatalog.All.Single(value =>
                value.Key == "dire-wolf");
            var revised = new SummonCreatureSpec(original.Key, "Dire Wolf (revised)",
                original.MonsterTier, original.MonsterTemplated,
                original.NaturesAllyTier, "A new body");
            AlternateSummoningRoster roster = new AlternateSummoningRoster(
                Production.Placements.Select(value => value.Creature.Key == original.Key ?
                    new SummonVariantSpec(value.Family, value.ParentTier, revised,
                        value.SourceTier, value.Multiplicity) : value),
                value => SummonVisibilityCatalog.IsPublished(value),
                SummonNativeExpansionCatalog.All);
            foreach (AlternateSummoningFamilyIdentity family in
                AlternateSummoningSourceCatalog.Identities)
            {
                string[] before = AlternateSummoningProjection.Identities(family,
                    Production).Where(value => value.Published).Select(value =>
                        value.Symbol).ToArray();
                string[] after = AlternateSummoningProjection.Identities(family,
                    roster).Where(value => value.Published).Select(value =>
                        value.Symbol).ToArray();
                Assertions.True(before.SequenceEqual(after), family.Key +
                    " lost or moved a published route across a fidelity update.");
            }
        }

        internal static void IdentitiesAreDisjointDeterministicAndRegistered()
        {
            var adapter = new List<AlternateSummoningChoice>();
            foreach (AlternateSummoningFamilyIdentity family in
                AlternateSummoningSourceCatalog.Identities)
                adapter.AddRange(AlternateSummoningProjection.Identities(family,
                    Production));
            HashSet<string> canonical = new HashSet<string>(
                ExpandedSummoningIdentityCatalog.Build().Select(value => value.Symbol),
                StringComparer.Ordinal);
            Assertions.Equal(adapter.Count, adapter.Select(value => value.Symbol)
                .Distinct(StringComparer.Ordinal).Count(), "Adapter symbols collide.");
            Assertions.False(adapter.Any(value => canonical.Contains(value.Symbol)),
                "An adapter symbol entered the canonical namespace.");
            Assertions.True(adapter.Where(value => value.Kind !=
                AlternateSummoningChoiceKind.Preserved).All(value =>
                    canonical.Contains(value.CanonicalSymbol)),
                "Every adapter child must compose from a registered canonical child.");

            JArray entries = (JArray)JObject.Parse(File.ReadAllText(Path.Combine(
                Environment.CurrentDirectory, "blueprints", "blueprints.json")))["entries"];
            var bySymbol = entries.ToDictionary(value => (string)value["symbol"],
                StringComparer.Ordinal);
            string[] guids = entries.Select(value => (string)value["guid"]).ToArray();
            Assertions.Equal(guids.Length, guids.Distinct(StringComparer.Ordinal)
                .Count(), "The ledger contains a duplicate GUID.");
            foreach (AlternateSummoningChoice choice in adapter)
            {
                JToken entry;
                Assertions.True(bySymbol.TryGetValue(choice.Symbol, out entry),
                    "Adapter identity is not in the ledger: " + choice.Symbol);
                Assertions.True((string)entry["status"] == "active" &&
                    (string)entry["plannedType"] == "BlueprintAbility",
                    "Adapter identity is not an active ability: " + choice.Symbol);
                Assertions.Equal(NameBasedGuid(choice.Symbol), (string)entry["guid"],
                    "Adapter GUID is not the tool's deterministic allocation for " +
                    choice.Symbol + ".");
            }
            Assertions.Equal(adapter.Count, bySymbol.Keys.Count(value =>
                value.StartsWith(AlternateSummoningProjection.SymbolPrefix,
                    StringComparison.Ordinal)),
                "The ledger holds adapter identities with no catalog source.");
        }

        internal static void MenuCompositionMatchesTheCanonicalContract()
        {
            // Summon Monster V as Call of the Wild's Master Summoner builds it:
            // a single umbrella, a 1d3 Dire Wolf and a 1d4+1 Monitor Lizard copy,
            // plus one option another mod added.
            var single = new Fake("host-single", "0964bf88b582bed41b74e79596c4f6d9",
                SummonMultiplicity.One);
            var d3 = new Fake("host-d3", "715f208d545be2f4aa2d3693e6347a5a",
                SummonMultiplicity.OneD3);
            var d4 = new Fake("host-d4", "e331ef5f6b235f54aaab5f2bf7a64eb6",
                SummonMultiplicity.OneD4PlusOne);
            var foreign = new Fake("KMG_Summoning_Ability_SM_Tier5_DireWolf_OneD3",
                null, null);
            AlternateSummoningFamilyIdentity master = AlternateSummoningSourceCatalog
                .Identities.Single(value => value.Key == "MasterSummoner");
            IReadOnlyList<AlternateSummoningChoice> exposed =
                AlternateSummoningProjection.Exposed(master, Production, 5, null);
            var children = exposed.ToDictionary(value => value.Symbol,
                value => new Fake(value.Symbol, null, value.Multiplicity));
            var natives = new Dictionary<string, Fake>();
            AlternateSummoningMenuPlan<Fake> plan = AlternateSummoningMenuPolicy.Compose(
                SummonFamily.Monster, 5, null, new[] { single, d3, foreign, d4 },
                value => value.Lineage, value => value.Quantity, exposed,
                choice => children[choice.Symbol], SummonNativeExpansionCatalog.All,
                native => natives.ContainsKey(native.Symbol) ? natives[native.Symbol] :
                    (natives[native.Symbol] = new Fake(native.Symbol, null,
                        native.Multiplicity)));
            Assertions.True(plan.RemovedHost.Contains(single) &&
                plan.RemovedHost.Contains(d3) && plan.RemovedHost.Contains(d4),
                "The umbrella and both published duplicates must be replaced.");
            Assertions.True(plan.KeptHost.Count == 1 && ReferenceEquals(
                plan.KeptHost[0], foreign) && ReferenceEquals(plan.Final.Last(),
                    foreign), "An unknown option was removed or reordered by its name.");
            Assertions.Equal(1, natives.Count,
                "Exactly the Redcap split option replaces the SM V umbrella.");
            Assertions.Equal(exposed.Count + 1 + 1, plan.Final.Count,
                "The menu must hold every published creature, the split option and the foreign one.");
            int lastSingle = plan.Final.ToList().FindLastIndex(value =>
                value.Quantity == SummonMultiplicity.One);
            int firstGroup = plan.Final.ToList().FindIndex(value =>
                value.Quantity == SummonMultiplicity.OneD3);
            Assertions.True(lastSingle < firstGroup, "Singles must precede groups.");

            // Withholding the KMG Dire Wolf keeps the host's 1d3 duplicate.
            AlternateSummoningRoster withheld = WithCreatures(new SummonCreatureSpec[0],
                value => value.Creature.Key != "dire-wolf" &&
                    SummonVisibilityCatalog.IsPublished(value));
            IReadOnlyList<AlternateSummoningChoice> fewer =
                AlternateSummoningProjection.Exposed(master, withheld, 5, null);
            AlternateSummoningMenuPlan<Fake> kept = AlternateSummoningMenuPolicy.Compose(
                SummonFamily.Monster, 5, null, new[] { single, d3, d4 },
                value => value.Lineage, value => value.Quantity, fewer,
                choice => children[choice.Symbol], SummonNativeExpansionCatalog.All,
                native => natives[native.Symbol]);
            Assertions.True(kept.KeptHost.Contains(d3) && !kept.RemovedHost.Contains(d3),
                "A withheld replacement must leave the host duplicate in place.");
        }

        internal static void DirectQuantityRootOffersOnlyItsQuantity()
        {
            AlternateSummoningFamilyIdentity tactician = AlternateSummoningSourceCatalog
                .Identities.Single(value => value.Key == "MonsterTactician");
            var preserved = new Fake("preserved", "28f49845fad6a534b95a65b6cea8f8d6",
                SummonMultiplicity.OneD3);
            IReadOnlyList<AlternateSummoningChoice> exposed =
                AlternateSummoningProjection.Exposed(tactician, Production, 3,
                    SummonMultiplicity.OneD3);
            AlternateSummoningMenuPlan<Fake> plan = AlternateSummoningMenuPolicy.Compose(
                SummonFamily.Monster, 3, SummonMultiplicity.OneD3, new[] { preserved },
                value => value.Lineage, value => value.Quantity, exposed,
                choice => new Fake(choice.Symbol, null, choice.Multiplicity),
                SummonNativeExpansionCatalog.All, native => null);
            Assertions.True(plan.Added.All(value => value.Quantity ==
                SummonMultiplicity.OneD3) && plan.Added.Count == exposed.Count &&
                exposed.Count > 0, "A 1d3 root offered another quantity.");
            Assertions.True(plan.RemovedHost.Contains(preserved),
                "The native 1d3 Wolf copy collapses into the published KMG Wolf.");
            Assertions.Equal(0, AlternateSummoningProjection.Identities(tactician,
                Production).Count(value => value.Kind ==
                    AlternateSummoningChoiceKind.Preserved && value.Tier == 1 &&
                    value.Multiplicity != SummonMultiplicity.One),
                "Tier I has only a single-creature root.");
        }

        internal static void LineageNeedsAnExactUniqueSignature()
        {
            var candidates = new[] {
                new AlternateSummoningLineageCandidate("a", SummonMultiplicity.One,
                    new[] { "u1", "u2" }),
                new AlternateSummoningLineageCandidate("b", SummonMultiplicity.OneD3,
                    new[] { "u3" }),
                new AlternateSummoningLineageCandidate("c", SummonMultiplicity.OneD4PlusOne,
                    new[] { "u3" }) };
            Assertions.Equal("a", AlternateSummoningLineagePolicy.Match(
                new[] { "u2", "u1", "u2" }, null, candidates).OptionGuid,
                "Order and repeats must not change a spawn signature.");
            Assertions.True(AlternateSummoningLineagePolicy.Match(new[] { "u3" }, null,
                candidates) == null, "An ambiguous signature must stay unknown.");
            Assertions.Equal("c", AlternateSummoningLineagePolicy.Match(new[] { "u3" },
                SummonMultiplicity.OneD4PlusOne, candidates).OptionGuid,
                "The quantity must disambiguate within a tier.");
            Assertions.True(AlternateSummoningLineagePolicy.Match(new[] { "u1" }, null,
                candidates) == null, "A partial signature is not a lineage.");
            Assertions.True(AlternateSummoningLineagePolicy.Match(new string[0], null,
                candidates) == null, "A host that spawns nothing has no lineage.");
        }

        internal static void LifecycleWaitsBlocksAndStaysAbsent()
        {
            AlternateSummoningLifecycleDecision decision;
            decision = Decide(AlternateSummoningProvider.CallOfTheWild, false, false,
                AlternateSummoningDependencyState.Active, true, true);
            Assertions.Equal(AlternateSummoningLifecycleAction.Wait, decision.Action,
                "Reconciliation must wait for the package roster.");
            decision = Decide(AlternateSummoningProvider.Native, true, true,
                AlternateSummoningDependencyState.Absent, true, true);
            Assertions.Equal(AlternateSummoningLifecycleAction.Block, decision.Action,
                "A failed package lifecycle must block.");
            decision = Decide(AlternateSummoningProvider.CallOfTheWild, true, false,
                AlternateSummoningDependencyState.Absent, true, true);
            Assertions.Equal(AlternateSummoningLifecycleAction.Absent, decision.Action,
                "Without CotW its callers are absent, not failed.");
            decision = Decide(AlternateSummoningProvider.Native, true, false,
                AlternateSummoningDependencyState.Absent, true, true);
            Assertions.True(decision.Action == AlternateSummoningLifecycleAction.Reconcile &&
                decision.Publish, "The native tactician must not depend on CotW.");
            foreach (AlternateSummoningDependencyState state in new[] {
                AlternateSummoningDependencyState.Inactive,
                AlternateSummoningDependencyState.Ambiguous })
                Assertions.Equal(AlternateSummoningLifecycleAction.Block, Decide(
                    AlternateSummoningProvider.CallOfTheWild, true, false, state, true,
                    true).Action, "An unverifiable CotW state must block: " + state);
            decision = Decide(AlternateSummoningProvider.CallOfTheWild, true, false,
                AlternateSummoningDependencyState.Active, false, false);
            Assertions.True(decision.Action == AlternateSummoningLifecycleAction.Reconcile &&
                !decision.Publish, "Module OFF registers identities without changing menus.");
            Assertions.Equal(AlternateSummoningLifecycleAction.Block, Decide(
                AlternateSummoningProvider.Native, true, false,
                AlternateSummoningDependencyState.Absent, true, false).Action,
                "Module ON without the canonical publication must block.");
        }

        internal static void FamilyRollbackRestoresAndRefusesForeignChanges()
        {
            var roots = new[] { new Box("first"), new Box("second"), new Box("third") };
            string[] originals = roots.Select(value => value.Value).ToArray();
            var log = new AlternateSummoningMutationLog<string>();
            log.Apply("first", () => roots[0].Value, value => roots[0].Value = value,
                "first+kmg");
            log.Apply("second", () => roots[1].Value, value => roots[1].Value = value,
                "second+kmg");
            Assertions.Throws<InvalidOperationException>(() => log.Apply("third",
                () => roots[2].Value, value => { }, "third+kmg"),
                "An unretained write must fail its family.");
            log.Rollback();
            Assertions.True(roots.Select(value => value.Value).SequenceEqual(originals),
                "Family rollback did not restore every root exactly.");
            Assertions.Equal(0, log.Count, "A completed rollback keeps no records.");
            log.Rollback();

            var foreign = new AlternateSummoningMutationLog<string>();
            foreign.Apply("first", () => roots[0].Value, value => roots[0].Value = value,
                "first+kmg");
            roots[0].Value = "first+another-mod";
            Assertions.Throws<InvalidOperationException>(() => foreign.Rollback(),
                "Rollback must refuse to overwrite another mod's later change.");
            Assertions.Equal("first+another-mod", roots[0].Value,
                "A refused rollback must leave the foreign change in place.");
        }

        private static AlternateSummoningLifecycleDecision Decide(
            AlternateSummoningProvider provider, bool initialized, bool failed,
            AlternateSummoningDependencyState state, bool module, bool published)
        {
            return AlternateSummoningLifecyclePolicy.Decide(provider, initialized,
                failed, initialized, state, module, published);
        }

        private static AlternateSummoningRoster WithCreatures(
            params SummonCreatureSpec[] added)
        {
            return WithCreatures(added, value => added.Any(creature =>
                creature.Key == value.Creature.Key) ||
                SummonVisibilityCatalog.IsPublished(value));
        }

        private static AlternateSummoningRoster WithCreatures(
            SummonCreatureSpec[] added, Func<SummonVariantSpec, bool> published)
        {
            var placements = new List<SummonVariantSpec>(Production.Placements);
            foreach (SummonCreatureSpec creature in added)
            foreach (SummonFamily family in new[] { SummonFamily.Monster,
                SummonFamily.NaturesAlly })
            {
                int? source = family == SummonFamily.Monster ? creature.MonsterTier :
                    creature.NaturesAllyTier;
                if (!source.HasValue) continue;
                for (int tier = source.Value; tier <= 9; tier++)
                    placements.Add(new SummonVariantSpec(family, tier, creature,
                        source.Value, tier == source.Value ? SummonMultiplicity.One :
                            tier == source.Value + 1 ? SummonMultiplicity.OneD3 :
                                SummonMultiplicity.OneD4PlusOne));
            }
            return new AlternateSummoningRoster(placements, published,
                SummonNativeExpansionCatalog.All);
        }

        /// <summary>RFC 4122 version 5, as the manifest tool allocates.</summary>
        private static string NameBasedGuid(string symbol)
        {
            byte[] space = new Guid("5f1c8e0b-6d63-4c55-9a41-2b7e3f9c0d18")
                .ToByteArray();
            SwapNetworkOrder(space);
            byte[] name = Encoding.UTF8.GetBytes(symbol);
            byte[] hash;
            using (SHA1 sha1 = SHA1.Create())
                hash = sha1.ComputeHash(space.Concat(name).ToArray());
            var bytes = new byte[16];
            Array.Copy(hash, bytes, 16);
            bytes[6] = (byte)((bytes[6] & 0x0F) | 0x50);
            bytes[8] = (byte)((bytes[8] & 0x3F) | 0x80);
            return string.Concat(bytes.Select(value => value.ToString("x2")));
        }

        private static void SwapNetworkOrder(byte[] guid)
        {
            Array.Reverse(guid, 0, 4);
            Array.Reverse(guid, 4, 2);
            Array.Reverse(guid, 6, 2);
        }

        private sealed class Fake
        {
            internal Fake(string name, string lineage, SummonMultiplicity? quantity)
            { Name = name; Lineage = lineage; Quantity = quantity; }
            internal string Name { get; private set; }
            internal string Lineage { get; private set; }
            internal SummonMultiplicity? Quantity { get; private set; }
            public override string ToString() { return Name; }
        }

        private sealed class Box
        {
            internal Box(string value) { Value = value; }
            internal string Value { get; set; }
        }
    }
}
