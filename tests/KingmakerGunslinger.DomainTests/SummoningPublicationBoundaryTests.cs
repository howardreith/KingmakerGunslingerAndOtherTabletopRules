using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.DomainTests
{
    /// <summary>
    /// The Expanded Summoning publication boundary, on the classes the
    /// runtime observer uses: the contract built from the publisher's
    /// catalogs and the identity manifest, the position-by-position reader,
    /// and the audit that judges each choice.
    ///
    /// The lists judged here are not produced by that code. They are what
    /// the loaded game held under the eighteen parents with the published
    /// v0.0.191 and Races Unleashed installed, as listed by the loader
    /// harness, which does not use the product's observers. Each test then
    /// changes that list in one stated way.
    /// </summary>
    internal static class SummoningPublicationBoundaryTests
    {
        // The publisher's eighteen parents, Summon Monster I to IX then
        // Summon Nature's Ally I to IX, as the game identifies them.
        private static readonly string[] Parents = {
            "8fd74eddd9b6c224693d9ab241f25e84", "1724061e89c667045a6891179ee2e8e7",
            "5d61dde0020bbf54ba1521f7ca0229dc", "7ed74a3ec8c458d4fb50b192fd7be6ef",
            "630c8b85d9f07a64f917d79cb5905741", "e740afbab0147944dab35d83faa0ae1c",
            "ab167fd8203c1314bac6568932f1752f", "d3ac756a229830243a72e84f3ab050d0",
            "52b5df2a97df18242aec67610616ded0",
            "c6147854641924442a3bb736080cfeb6", "298148133cdc3fd42889b99c82711986",
            "fdcf7e57ec44f704591f11b45f4acf61", "c83db50513abdf74ca103651931fac4b",
            "8f98a22f35ca6684a983363d32e51bfe", "55bbce9b3e76d4a4a8c8e0698d29002c",
            "051b979e7d7f8ec41b9fa35d04746b33", "ea78c04f0bd13d049a1cce5daf8d83e0",
            "a7469ef84ba50ac4cbf3d145e3173f8e" };

        private const string SummonMonsterII = "1724061e89c667045a6891179ee2e8e7";
        private const string SummonMonsterIV = "7ed74a3ec8c458d4fb50b192fd7be6ef";
        private const string SummonMonsterV = "630c8b85d9f07a64f917d79cb5905741";
        private const string SummonNaturesAllyI = "c6147854641924442a3bb736080cfeb6";
        private const string RacesUnleashed = "130211b7fcb443e9b67d5561d75cf6a0";
        private const string NativeWolf = "7ab27a0d547742741beb5d089f1c3852";

        /// <summary>One position of a recorded list. The flags make it unreadable in a stated way.</summary>
        private sealed class Row
        {
            internal string Guid;
            internal string Name;
            internal bool IdentityThrows;
        }

        private sealed class Graph
        {
            internal readonly Dictionary<string, List<Row>> Choices = new Dictionary<string, List<Row>>(StringComparer.Ordinal);
            internal readonly Dictionary<string, string> Names = new Dictionary<string, string>(StringComparer.Ordinal);
            internal readonly Dictionary<string, string> Components = new Dictionary<string, string>(StringComparer.Ordinal);
            internal readonly HashSet<string> Unavailable = new HashSet<string>(StringComparer.Ordinal);
        }

        private static Dictionary<string, string> _manifest;
        private static ExpandedSummoningPublicationContract _contract;

        private static Dictionary<string, string> Manifest()
        {
            return _manifest ?? (_manifest = SummonManifestIdentities.Parse(File.ReadAllText(
                Path.Combine(Environment.CurrentDirectory, "blueprints", "blueprints.json"))));
        }

        private static ExpandedSummoningPublicationContract Contract()
        {
            return _contract ?? (_contract = ExpandedSummoningPublicationContract.Build(Parents, Manifest()));
        }

        private static string Identity(string symbol)
        {
            return Manifest().Single(pair => pair.Value == symbol).Key;
        }

        private static Graph Recorded(string state)
        {
            JObject document = JObject.Parse(File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "tests",
                "fixtures", "summoning-publication", "races-unleashed-parent-census.json")));
            var graph = new Graph();
            foreach (JToken parent in document[state])
            {
                string guid = (string)parent["guid"];
                graph.Names[guid] = (string)parent["name"];
                graph.Components[guid] = (string)parent["component"];
                graph.Choices[guid] = parent["choices"].Select(choice => choice.Type == JTokenType.String ?
                    new Row { Guid = (string)choice, Name = Manifest()[(string)choice].Replace('.', '_').Replace('-', '_') } :
                    new Row { Guid = (string)choice[0], Name = (string)choice[1] }).ToList();
            }
            return graph;
        }

        /// <summary>Reads a list through the reader the runtime observer uses.</summary>
        private static SummonParentReading[] Read(Graph graph)
        {
            return Contract().Parents.Select(key => SummonParentReading.Read<Row>(key,
                () =>
                {
                    if (graph.Unavailable.Contains(key.Guid)) throw new InvalidOperationException("no such blueprint");
                    return graph.Names[key.Guid];
                },
                () => graph.Components[key.Guid] == "$KMG_ExpandedSummoning_Variants",
                () => graph.Components[key.Guid] == null ? new IList<Row>[0] :
                    new IList<Row>[] { graph.Choices[key.Guid] },
                row =>
                {
                    if (row.IdentityThrows) throw new NullReferenceException();
                    return row.Guid;
                },
                row => row.Name)).ToArray();
        }

        private static SummonPublicationState State(bool requested, bool racesUnleashed)
        {
            var state = new SummonPublicationState { Requested = requested, Active = requested,
                SpiderFeaturePresent = racesUnleashed, SpiderIntegrationOutcome = "test" };
            if (racesUnleashed)
                foreach (SummonFamily family in SpiderSummonerCatalog.Families)
                foreach (SpiderSpeciesSpec species in SpiderSummonerCatalog.All)
                    state.SpiderProviderRows.Add(SummonPublicationState.RowKey(family, species.Key));
            return state;
        }

        private static ExpandedSummoningPublicationAudit Judge(Graph graph, SummonPublicationState state)
        {
            return ExpandedSummoningPublicationAudit.Evaluate(Contract(), state, Read(graph), null);
        }

        private static bool Conforms(ExpandedSummoningPublicationAudit audit)
        {
            return audit.RequiredChoices().Passed && audit.Boundary().Passed && audit.Duplicates().Passed &&
                audit.Readable().Passed && audit.Describe() == ExpandedSummoningPublicationAudit.Expectation(
                    Contract(), audit.State, audit.NativeVariants);
        }

        private static SummonLedgerEntry Under(ExpandedSummoningPublicationAudit audit, string parent, string guid)
        {
            return audit.Entries.Single(value => value.Parent.Guid == parent && value.Guid == guid);
        }

        /// <summary>Removes the project's spider rows and Races Unleashed's own, leaving what the game holds without that mod.</summary>
        private static void RemoveRacesUnleashed(Graph graph)
        {
            foreach (List<Row> choices in graph.Choices.Values)
                choices.RemoveAll(row => row.Guid.StartsWith(RacesUnleashed, StringComparison.Ordinal) ||
                    (Manifest().ContainsKey(row.Guid) && Manifest()[row.Guid].StartsWith("KMG.Summoning.Spider.",
                        StringComparison.Ordinal)));
        }

        internal static void RecordedProviderListsAreAttributedChoiceByChoice()
        {
            ExpandedSummoningPublicationAudit on = Judge(Recorded("moduleOn"), State(true, true));
            Assertions.True(Conforms(on),
                "The lists recorded with Races Unleashed and the module on were not judged conforming: " +
                on.RequiredChoices().Observed + " / " + on.Boundary().Observed + " / " + on.Describe());

            // The thirty-eight choices the old reader could only count: four
            // species with base tiers I, IV, V and VII, each generated at
            // every tier above its base, in both spell families.
            SummonLedgerEntry[] spiders = on.Entries.Where(value => value.Symbol != null &&
                value.Symbol.StartsWith("KMG.Summoning.Spider.", StringComparison.Ordinal)).ToArray();
            Assertions.Equal(2 * ((9 - 1) + (9 - 4) + (9 - 5) + (9 - 7)), spiders.Length,
                "The recorded lists do not hold one generated row per species and tier above its base.");
            Assertions.True(spiders.All(value => value.Class == SummonChoiceClass.SpiderGenerated &&
                value.Verdict == SummonChoiceVerdict.Required && value.Occurrences == 1 &&
                value.Source == ExpandedSummoningPublicationContract.SpiderSource),
                "A generated spider row was not recognized as a required choice with its contract named.");
            Assertions.Equal(spiders.Length, on.SpiderRows, "The spider row total disagrees with the ledger.");
            Assertions.Equal(0, on.Unclassified, "A recorded choice was left unclassified.");

            SummonLedgerEntry crab = Under(on, SummonMonsterII, Identity("KMG.Summoning.Spider.SM.Tier2.CrabSpider.OneD3"));
            Assertions.True(crab.Index == 19 && crab.ParentName == "SummonMonsterIIBase" &&
                crab.Parent.Family == SummonFamily.Monster && crab.Parent.Tier == 2 && crab.Requested && crab.Active,
                "A ledger entry lost its parent, position or module state: " + crab.Describe());

            SummonLedgerEntry placement = Under(on, SummonMonsterII, Identity("KMG.Summoning.Ability.SM.Tier2.Lemure.One"));
            Assertions.True(placement.Class == SummonChoiceClass.Placement &&
                placement.Verdict == SummonChoiceVerdict.Required &&
                placement.Source == ExpandedSummoningPublicationContract.PlacementSource,
                "A published placement was not recognized as required.");
            Assertions.Equal(SummonVisibilityCatalog.PublishedLogicalPlacementCount, on.Placements,
                "The recorded lists do not hold every published placement.");
            Assertions.Equal(SummonNativeExpansionCatalog.All.Count, on.NativeExpansions,
                "The recorded lists do not hold every native expansion wrapper.");

            // With the module off the same game holds its own options and
            // Races Unleashed's rows, and nothing of the project's.
            ExpandedSummoningPublicationAudit off = Judge(Recorded("moduleOff"), State(false, true));
            Assertions.True(Conforms(off), "The lists recorded with the module off were not judged conforming: " +
                off.Boundary().Observed + " / " + off.Describe());
            Assertions.True(off.PublishedParents == 0 && off.Placements == 0 && off.SpiderRows == 0 &&
                off.NativeVariants == 52, "The module-off lists were not read as untouched: " + off.Describe());
        }

        internal static void MissingRequiredChoiceIsReportedWithItsPlace()
        {
            Graph graph = Recorded("moduleOn");
            string lemure = Identity("KMG.Summoning.Ability.SM.Tier2.Lemure.One");
            string widow = Identity("KMG.Summoning.Spider.SM.Tier5.GiantBlackWidow.OneD3");
            graph.Choices[SummonMonsterII].RemoveAll(row => row.Guid == lemure);
            graph.Choices[SummonMonsterV].RemoveAll(row => row.Guid == widow);
            var state = State(true, true);
            state.SpiderIntegrationOutcome = "integrated;repaired=8;generated=37;problems=example";
            ExpandedSummoningPublicationAudit audit = Judge(graph, state);

            SummonAuditOutcome required = audit.RequiredChoices();
            Assertions.False(required.Passed, "Two missing required choices passed.");
            SummonLedgerEntry[] missing = audit.Entries.Where(value => value.Verdict == SummonChoiceVerdict.Missing).ToArray();
            Assertions.True(missing.Length == 2 &&
                missing.Any(value => value.Guid == lemure && value.Parent.Guid == SummonMonsterII &&
                    value.Class == SummonChoiceClass.Placement && value.Index == -1 &&
                    value.Symbol == "KMG.Summoning.Ability.SM.Tier2.Lemure.One") &&
                missing.Any(value => value.Guid == widow && value.Parent.Guid == SummonMonsterV &&
                    value.Class == SummonChoiceClass.SpiderGenerated &&
                    value.Reason.Contains("generated=37;problems=example")),
                "The missing choices were not each reported with identity, parent, class and reason: " + required.Observed);
            Assertions.True(required.Observed.Contains("Summon Monster II") && required.Observed.Contains(lemure) &&
                required.Observed.Contains("Summon Monster V") && required.Observed.Contains(widow),
                "The assertion text does not name the missing choices and their parents: " + required.Observed);
            Assertions.False(Conforms(audit), "The totals still matched with two required choices missing.");
            Assertions.True(audit.Boundary().Passed && audit.Duplicates().Passed && audit.Readable().Passed,
                "A missing choice changed a result that does not depend on it.");
        }

        internal static void SameCountReplacementAndDuplicateStayDetectable()
        {
            // A withheld placement in place of a required one: the list is
            // as long as before and holds as many placements as before.
            Graph swapped = Recorded("moduleOn");
            string pony = Identity("KMG.Summoning.Ability.SNA.Tier1.Pony.One");
            string stirge = Identity("KMG.Summoning.Ability.SNA.Tier1.Stirge.One");
            Row slot = swapped.Choices[SummonNaturesAllyI].Single(row => row.Guid == pony);
            slot.Guid = stirge;
            slot.Name = "KMG_Summoning_Ability_SNA_Tier1_Stirge_One";
            ExpandedSummoningPublicationAudit audit = Judge(swapped, State(true, true));
            Assertions.Equal(SummonVisibilityCatalog.PublishedLogicalPlacementCount, audit.Placements,
                "The replacement was meant to leave the placement count unchanged.");
            Assertions.False(Conforms(audit), "A same-count replacement passed.");
            Assertions.True(Under(audit, SummonNaturesAllyI, pony).Verdict == SummonChoiceVerdict.Missing &&
                Under(audit, SummonNaturesAllyI, stirge).Verdict == SummonChoiceVerdict.Unexpected &&
                Under(audit, SummonNaturesAllyI, stirge).Reason.Contains("withholds"),
                "The replacement was not reported as one missing choice and one withheld choice offered.");
            Assertions.False(audit.PlacementsExact, "The placements were reported exact after a replacement.");

            // A second copy of a required choice in place of another.
            Graph doubled = Recorded("moduleOn");
            string dog = Identity("KMG.Summoning.Ability.SNA.Tier1.Dog.One");
            Row other = doubled.Choices[SummonNaturesAllyI].Single(row => row.Guid == pony);
            other.Guid = dog;
            other.Name = "KMG_Summoning_Ability_SNA_Tier1_Dog_One";
            audit = Judge(doubled, State(true, true));
            Assertions.Equal(SummonVisibilityCatalog.PublishedLogicalPlacementCount, audit.Placements,
                "The duplicate was meant to leave the placement count unchanged.");
            SummonLedgerEntry[] dogs = audit.Entries.Where(value => value.Parent.Guid == SummonNaturesAllyI &&
                value.Guid == dog).ToArray();
            Assertions.True(dogs.Length == 2 && dogs.All(value => value.Occurrences == 2) &&
                dogs.Count(value => value.Verdict == SummonChoiceVerdict.Duplicate) == 1 &&
                dogs.Count(value => value.Verdict == SummonChoiceVerdict.Required) == 1,
                "A repeated identity was not reported once as required and once as a duplicate.");
            Assertions.False(audit.Duplicates().Passed, "A duplicate passed.");
            Assertions.False(audit.RequiredChoices().Passed, "The choice the duplicate displaced was not reported missing.");
            Assertions.False(Conforms(audit), "A same-count duplicate passed.");
        }

        internal static void NativeAndProviderChoicesKeepTheirOwnClassification()
        {
            // The game's own option where publication has not touched the list.
            ExpandedSummoningPublicationAudit off = Judge(Recorded("moduleOff"), State(false, true));
            SummonLedgerEntry wolf = Under(off, SummonMonsterII, NativeWolf);
            Assertions.True(wolf.Class == SummonChoiceClass.NativeOption && wolf.Verdict == SummonChoiceVerdict.Preserved &&
                wolf.Source == ExpandedSummoningPublicationContract.NativeOptionSource,
                "The game's own option was not kept as its own class: " + wolf.Describe());
            string widowRow = RacesUnleashed + "SummonMonsterGiantBlackWidow";
            SummonLedgerEntry widow = Under(off, SummonMonsterIV, widowRow);
            Assertions.True(widow.Class == SummonChoiceClass.ProviderOriginal &&
                widow.Verdict == SummonChoiceVerdict.Preserved &&
                widow.Source == ExpandedSummoningPublicationContract.ProviderSource,
                "Races Unleashed's own row was not kept as its own class: " + widow.Describe());

            // The same row is still its provider's with the module on, and
            // is not counted among the project's choices.
            ExpandedSummoningPublicationAudit on = Judge(Recorded("moduleOn"), State(true, true));
            Assertions.True(Under(on, SummonMonsterIV, widowRow).Verdict == SummonChoiceVerdict.Preserved &&
                on.NativeVariants == 8, "The provider's rows were not preserved beside the published choices.");

            // The publisher replaces that native option once it publishes,
            // so finding it in a published list is a fault.
            Graph kept = Recorded("moduleOn");
            kept.Choices[SummonMonsterII].Add(new Row { Guid = NativeWolf, Name = "SummonMonsterIISingle" });
            ExpandedSummoningPublicationAudit audit = Judge(kept, State(true, true));
            Assertions.True(Under(audit, SummonMonsterII, NativeWolf).Verdict == SummonChoiceVerdict.Unexpected &&
                !audit.Boundary().Passed, "A replaced native option left in a published list passed.");

            // A provider row under a parent nothing declares it for is
            // shown, and is not called the project's.
            Graph moved = Recorded("moduleOn");
            moved.Choices[SummonMonsterII].Add(new Row { Guid = widowRow, Name = "SummonMonsterGiantBlackWidow" });
            audit = Judge(moved, State(true, true));
            SummonLedgerEntry elsewhere = Under(audit, SummonMonsterII, widowRow);
            Assertions.True(elsewhere.Verdict == SummonChoiceVerdict.NotAttributed && !elsewhere.Fails &&
                audit.NotAttributed.Contains(elsewhere) && audit.DescribeOthers().Contains(widowRow),
                "A provider row outside its declared parent was hidden or called a failure of the project's.");

            // Without Races Unleashed there is no source row, so no spider
            // row is required and none may be offered.
            Graph alone = Recorded("moduleOn");
            RemoveRacesUnleashed(alone);
            audit = Judge(alone, State(true, false));
            Assertions.True(Conforms(audit) && audit.SpiderRows == 0 && audit.NativeVariants == 0,
                "The lists without Races Unleashed were not judged conforming: " + audit.Describe());
            audit = Judge(Recorded("moduleOn"), State(true, false));
            SummonLedgerEntry[] orphans = audit.Entries.Where(value => value.Class == SummonChoiceClass.SpiderGenerated).ToArray();
            Assertions.True(orphans.Length == 38 && orphans.All(value => value.Verdict == SummonChoiceVerdict.Unexpected) &&
                !audit.Boundary().Passed, "Generated spider rows passed with no source for them in the library.");
        }

        internal static void UnknownAndWronglyAttributedChoicesStayVisible()
        {
            Graph graph = Recorded("moduleOn");
            const string foreignGuid = "0f0e0d0c0b0a09080706050403020100";
            const string claimedGuid = "00112233445566778899aabbccddeeff";
            string marker = Identity("KMG.Summoning.Spider.DcBonus");
            string elsewhere = Identity("KMG.Summoning.Ability.SM.Tier5.Lemure.OneD4PlusOne");
            graph.Choices[SummonMonsterII].Add(new Row { Guid = foreignGuid, Name = "AnotherModSummonRow" });
            graph.Choices[SummonMonsterII].Add(new Row { Guid = claimedGuid, Name = "KMG_Summoning_Ability_SM_Tier2_Invented_One" });
            graph.Choices[SummonMonsterII].Add(new Row { Guid = marker, Name = "KMG_Summoning_Spider_DcBonus" });
            graph.Choices[SummonMonsterII].Add(new Row { Guid = elsewhere, Name = "KMG_Summoning_Ability_SM_Tier5_Lemure_OneD4PlusOne" });
            ExpandedSummoningPublicationAudit audit = Judge(graph, State(true, true));

            SummonLedgerEntry foreign = Under(audit, SummonMonsterII, foreignGuid);
            Assertions.True(foreign.Class == SummonChoiceClass.Foreign && foreign.Verdict == SummonChoiceVerdict.NotAttributed &&
                !foreign.Fails && foreign.Source == ExpandedSummoningPublicationContract.MergeSource &&
                audit.DescribeOthers().Contains("notAttributed=1") && audit.DescribeOthers().Contains(foreignGuid),
                "Another mod's choice was hidden, or was called acceptable without saying its owner is unknown.");
            Assertions.True(((JArray)audit.ToJson()["notAttributed"]).Any(value => (string)value["guid"] == foreignGuid &&
                (string)value["parentGuid"] == SummonMonsterII), "The ledger does not hold the unattributed choice.");

            SummonLedgerEntry claimed = Under(audit, SummonMonsterII, claimedGuid);
            Assertions.True(claimed.Class == SummonChoiceClass.ProjectNamed && claimed.Verdict == SummonChoiceVerdict.Unexpected,
                "A choice named as the project's with no declared identity was accepted on its name.");
            SummonLedgerEntry other = Under(audit, SummonMonsterII, marker);
            Assertions.True(other.Class == SummonChoiceClass.OwnedOther && other.Verdict == SummonChoiceVerdict.Unexpected &&
                other.Symbol == "KMG.Summoning.Spider.DcBonus",
                "A project identity that is not a publication choice was accepted under a parent.");
            SummonLedgerEntry misplaced = audit.Entries.Single(value => value.Parent.Guid == SummonMonsterII &&
                value.Guid == elsewhere);
            Assertions.True(misplaced.Verdict == SummonChoiceVerdict.Unexpected &&
                misplaced.Reason.Contains("declared for Summon Monster V") &&
                misplaced.Reason.Contains("offered under Summon Monster II"),
                "A placement under the wrong parent was accepted: " + misplaced.Describe());

            Assertions.Equal(2, audit.Unclassified, "The undeclared and non-publication project choices were not both counted.");
            SummonAuditOutcome boundary = audit.Boundary();
            Assertions.True(!boundary.Passed && boundary.Observed.StartsWith("unexpected=3", StringComparison.Ordinal) &&
                boundary.Observed.Contains(claimedGuid) && boundary.Observed.Contains(marker) &&
                boundary.Observed.Contains(elsewhere), "The boundary result does not name each offending choice: " +
                boundary.Observed);
            Assertions.False(Conforms(audit), "The totals still matched with undeclared choices present.");
            Assertions.True(audit.RequiredChoices().Passed && audit.Readable().Passed,
                "An added choice changed a result that does not depend on it.");
        }

        internal static void ProjectChoicesInADisabledStateFailTheBoundary()
        {
            // Everything the module publishes, judged against a request for
            // the module off.
            ExpandedSummoningPublicationAudit audit = Judge(Recorded("moduleOn"), State(false, true));
            int project = SummonVisibilityCatalog.PublishedLogicalPlacementCount +
                SummonNativeExpansionCatalog.All.Count + 38;
            Assertions.True(!audit.Boundary().Passed &&
                audit.Entries.Count(value => value.Verdict == SummonChoiceVerdict.Unexpected) == project &&
                audit.Entries.Where(value => value.Verdict == SummonChoiceVerdict.Unexpected).All(value =>
                    value.Reason.Contains("requested off") && !value.Requested),
                "Project choices offered with the module requested off passed: " + audit.Boundary().Observed);
            Assertions.False(Conforms(audit), "The totals matched a disabled state with everything published.");
            Assertions.True(ExpandedSummoningPublicationAudit.Expectation(Contract(), audit.State, 8).StartsWith(
                "publishedParents=0;placements=0;nativeOptions=0;preservation=0;unclassified=0;", StringComparison.Ordinal),
                "A disabled state expects something published.");

            // One generated row left behind in an otherwise untouched list.
            Graph leftover = Recorded("moduleOff");
            string crab = Identity("KMG.Summoning.Spider.SM.Tier2.CrabSpider.OneD3");
            leftover.Choices[SummonMonsterII].Add(new Row { Guid = crab, Name = "KMG_Summoning_Spider_SM_Tier2_CrabSpider_OneD3" });
            audit = Judge(leftover, State(false, true));
            Assertions.True(Under(audit, SummonMonsterII, crab).Verdict == SummonChoiceVerdict.Unexpected &&
                !audit.Boundary().Passed, "One project choice left in a disabled state passed.");

            // The other direction: the module requested on and nothing published.
            audit = Judge(Recorded("moduleOff"), State(true, true));
            Assertions.True(!audit.RequiredChoices().Passed &&
                audit.Entries.Count(value => value.Verdict == SummonChoiceVerdict.Missing) == project,
                "An enabled state with nothing published passed: " + audit.RequiredChoices().Observed);
        }

        internal static void MalformedEntriesDoNotSuppressReadableOnes()
        {
            Graph graph = Recorded("moduleOn");
            int before = graph.Choices[SummonMonsterII].Count;
            graph.Choices[SummonMonsterII].Insert(3, null);
            graph.Choices[SummonMonsterII].Insert(7, new Row { Name = "Broken", IdentityThrows = true });
            graph.Choices[SummonMonsterII].Add(new Row { Guid = " ", Name = "NoIdentity" });
            graph.Unavailable.Add(SummonMonsterV);
            ExpandedSummoningPublicationAudit audit = Judge(graph, State(true, true));

            SummonLedgerEntry[] unreadable = audit.Entries.Where(value => value.Verdict == SummonChoiceVerdict.Unreadable).ToArray();
            Assertions.True(unreadable.Length == 4 &&
                unreadable.Any(value => value.Parent.Guid == SummonMonsterII && value.Index == 3 && value.Reason == "null entry") &&
                unreadable.Any(value => value.Parent.Guid == SummonMonsterII && value.Index == 7 &&
                    value.Reason.Contains("NullReferenceException") && value.Name == "Broken") &&
                unreadable.Any(value => value.Parent.Guid == SummonMonsterII && value.Index == before + 2 &&
                    value.Reason == "entry has no identity") &&
                unreadable.Any(value => value.Parent.Guid == SummonMonsterV && value.Index == -1 &&
                    value.Reason.Contains("parent unavailable") && value.Reason.Contains("no such blueprint")),
                "Each unreadable position and the unreadable parent were not reported with place and reason.");
            SummonAuditOutcome readable = audit.Readable();
            Assertions.True(!readable.Passed && readable.Observed.StartsWith(
                "unreadableParents=1;unreadableEntries=3", StringComparison.Ordinal),
                "The readability result is wrong: " + readable.Observed);

            // Every readable position of the damaged parent is still judged,
            // and nothing required there is called missing.
            Assertions.Equal(before, audit.Entries.Count(value => value.Parent.Guid == SummonMonsterII &&
                value.Verdict == SummonChoiceVerdict.Required),
                "Readable choices beside an unreadable position were not judged.");
            Assertions.Equal(0, audit.Entries.Count(value => value.Verdict == SummonChoiceVerdict.Missing),
                "A choice was called missing because a position near it, or another parent, could not be read.");
            // The other sixteen parents are judged in full.
            ExpandedSummoningPublicationAudit whole = Judge(Recorded("moduleOn"), State(true, true));
            foreach (string parent in Parents.Where(value => value != SummonMonsterII && value != SummonMonsterV))
                Assertions.Equal(whole.Entries.Count(value => value.Parent.Guid == parent && value.Verdict == SummonChoiceVerdict.Required),
                    audit.Entries.Count(value => value.Parent.Guid == parent && value.Verdict == SummonChoiceVerdict.Required),
                    "An unreadable parent changed the judgement of another parent.");
            // What could not be checked is said, not passed.
            SummonAuditOutcome required = audit.RequiredChoices();
            int unchecked_ = whole.Entries.Count(value => value.Parent.Guid == SummonMonsterV &&
                value.Verdict == SummonChoiceVerdict.Required);
            Assertions.True(!required.Passed && required.Observed.Contains("missing=0;notChecked=" + unchecked_),
                "The choices under an unreadable parent were passed or called missing: " + required.Observed);
            Assertions.False(Conforms(audit), "Unreadable positions passed.");
            Assertions.True(audit.Describe().EndsWith(";unreadable=4", StringComparison.Ordinal),
                "The totals do not carry the unreadable positions: " + audit.Describe());
            JObject ledger = audit.ToJson();
            Assertions.True((string)ledger["parents"].Single(value => (string)value["guid"] == SummonMonsterV)["fault"] != null &&
                ledger["mismatches"].Count(value => (string)value["verdict"] == "Unreadable") == 4,
                "The written ledger does not hold the unreadable positions.");
        }

        internal static void TotalsKeepTheirEstablishedKeysAndFollowTheCatalogs()
        {
            // The seven keys the totals have always had, in their order,
            // then the two new ones.
            ExpandedSummoningPublicationAudit audit = Judge(Recorded("moduleOn"), State(true, true));
            Assertions.Equal("publishedParents=18;placements=" + SummonVisibilityCatalog.PublishedLogicalPlacementCount +
                ";nativeOptions=" + SummonNativeExpansionCatalog.All.Count +
                ";preservation=0;unclassified=0;placementsExact=True;nativeVariants=8;spiderRows=38;unreadable=0",
                audit.Describe(), "The totals changed their form.");
            // What is expected of the spider rows follows what the library
            // holds of their source, row by row.
            var partial = State(true, true);
            partial.SpiderProviderRows.Remove(SummonPublicationState.RowKey(SummonFamily.Monster, "giant-tarantula"));
            Assertions.True(ExpandedSummoningPublicationAudit.Expectation(Contract(), partial, 0).Contains(";spiderRows=36;"),
                "A species with no source row is still expected to be generated.");
            Assertions.True(ExpandedSummoningPublicationAudit.Expectation(Contract(), State(true, false), 0).Contains(";spiderRows=0;"),
                "Spider rows are expected without Races Unleashed.");
            // The publisher's own name census is carried beside the census by identity.
            ExpandedSummoningPublicationAudit disputed = ExpandedSummoningPublicationAudit.Evaluate(Contract(),
                State(true, true), Read(Recorded("moduleOn")), false);
            Assertions.True(!disputed.PlacementsExact && disputed.Describe().Contains("placementsExact=False") &&
                (bool?)disputed.ToJson()["totals"]["publisherNameCensusExact"] == false,
                "A failed name census by the publisher was not carried into the totals.");
        }
    }
}
