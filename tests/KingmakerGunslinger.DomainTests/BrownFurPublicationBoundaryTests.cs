using System;
using System.Collections.Generic;
using System.Linq;
using KingmakerGunslinger.BrownFur;
using KingmakerGunslinger.FeatureModules;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.DomainTests
{
    /// <summary>
    /// The Brown-Fur publication boundary, on the classes the runtime
    /// observer uses: the publication transaction itself, the selector
    /// reader and the audit that judges the reading as named results.
    ///
    /// Each test builds an archetype selector, lets the real transaction
    /// publish into it the way the coordinator composes it, lets other
    /// "mods" change the selector afterwards, and judges what is read
    /// back. The identities are the ones the game held beside Call of the
    /// Wild and Favored Class in the disposable copy.
    /// </summary>
    internal static class BrownFurPublicationBoundaryTests
    {
        private sealed class Fake
        {
            internal Fake(string id, string name) { Id = id; Name = name; }
            internal string Id;
            internal string Name;
            internal bool IdentityThrows;
        }

        private static readonly string[][] Standalone = {
            new[] { "6ed134267c2f45debc77050d52581d01", "SchoolSavantArchetype" },
            new[] { "712cbe46ce0f49dfb80aa0bb55fe6dc8", "BloodArcanistArchetype" },
            new[] { "6ba8dccb41c245b59283097297b53f6d", "UnletteredArcanistArchetype" },
            new[] { "3dbd81a35cf74aefb4dba140a0a22079", "OccultistArchetype" },
            new[] { "efb2b9178a884d55a1e3ce19dadcbfa0", "CollegiateArcanistArchetype" },
            new[] { "dfc6a2228a3247829e9d6dcb4d4cdc9f", "SpellSpecialistArcanistArchetype" } };

        // Favored Class builds these three from Brown-Fur itself, after
        // Brown-Fur is published. Nothing in this project declares them.
        private static readonly string[][] BuiltFromBrownFur = {
            new[] { "1e94108befebb3728ec03be1a6ee70db", "UnletteredArcanistArchetypeSpellSpecialistArcanistArchetypeKMG_BrownFurTransmuter_Archetype" },
            new[] { "c152b2a965d9f4f0105d562aeba2ac44", "UnletteredArcanistArchetypeKMG_BrownFurTransmuter_Archetype" },
            new[] { "753ccc40ae29f6c71c433293315b4fb6", "SpellSpecialistArcanistArchetypeKMG_BrownFurTransmuter_Archetype" } };

        private static string BrownFurGuid
        {
            get
            {
                return BrownFurIdentityCatalog.All.Single(value =>
                    value.Symbol == BrownFurIdentityCatalog.Archetype).Guid;
            }
        }

        /// <summary>A selector, the module's one reconciliation of it, and what is read back afterwards.</summary>
        private sealed class World
        {
            internal List<Fake> Selector;
            internal readonly Fake BrownFur = new Fake(BrownFurGuid, "KMG_BrownFurTransmuter_Archetype");
            internal BrownFurPublicationTransaction Transaction;
            internal string[] Reconciled;
            internal bool Active;

            internal World(int standalone = 6)
            {
                Selector = Standalone.Take(standalone).Select(value => new Fake(value[0], value[1])).ToList();
            }

            /// <summary>What the coordinator does once: record the selector, then publish or not.</summary>
            internal World Reconcile(bool active)
            {
                Active = active;
                Reconciled = Selector.Select(value => value.Id).ToArray();
                if (!active) return this;
                Transaction = new BrownFurPublicationTransaction()
                    .Step("brown-fur-registered-identities", () => { }, () => { })
                    .InsertBefore(BrownFurPublicationAudit.ArchetypeSurface,
                        () => (IList<Fake>)Selector.ToList(), value => Selector = value.ToList(),
                        new[] { BrownFur }, value => value.Id,
                        value => BrownFurArchetypeOrdering.IsKnownCombinedArchetype(value.Id));
                Transaction.Commit();
                return this;
            }

            internal World AddCombined()
            {
                Selector.AddRange(BrownFurArchetypeOrdering.KnownCombinedArchetypeGuids.Select((value, index) =>
                    new Fake(value, "Combined" + index)));
                return this;
            }

            internal World AddBuiltFromBrownFur()
            {
                Selector.AddRange(BuiltFromBrownFur.Select(value => new Fake(value[0], value[1])));
                return this;
            }

            internal BrownFurPublicationReading Read(bool requested)
            {
                var reading = new BrownFurPublicationReading
                {
                    Requested = requested, Active = Active,
                    Availability = BrownFurDependencyAvailability.Available,
                    StatusPublished = Active, ContractCompatible = true,
                    RegisteredIdentities = BrownFurIdentityCatalog.IdentityCount,
                    ArchetypeGuid = BrownFurGuid,
                    ReferencesInAnyClass = Selector.Count(value => value != null && !value.IdentityThrows &&
                        value.Id == BrownFurGuid),
                    Evidence = Transaction == null ? new List<string>() : Transaction.Evidence.ToList(),
                    Observations = Transaction == null ? new List<BrownFurSurfaceObservation>() :
                        Transaction.Observations.ToList(),
                    ReconciledSelector = Reconciled
                };
                string fault;
                reading.Selector = BrownFurSelectorEntry.Read<Fake>(() => Selector,
                    value =>
                    {
                        if (value.IdentityThrows) throw new NullReferenceException();
                        return value.Id;
                    },
                    value => value.Name, value => ReferenceEquals(value, BrownFur), out fault);
                reading.SelectorFault = fault;
                return reading;
            }
        }

        private static BrownFurPublicationAudit Judge(BrownFurPublicationReading reading)
        { return BrownFurPublicationAudit.Evaluate(reading); }

        private static void OnlyFails(BrownFurPublicationAudit audit, string message, params string[] names)
        {
            string[] failed = audit.Results.Where(value => !value.Passed).Select(value => value.Name)
                .OrderBy(value => value, StringComparer.Ordinal).ToArray();
            Assertions.True(failed.SequenceEqual(names.OrderBy(value => value, StringComparer.Ordinal)) &&
                !audit.Passed, message + " Failed: [" + string.Join(", ", failed) + "] " + audit.DescribeFailures());
        }

        internal static void PublishedThenLaterForeignAdditionsPassAndAreListed()
        {
            // Brown-Fur publishes into the six standalone archetypes; Favored
            // Class then adds its five combined archetypes and three more it
            // builds from Brown-Fur. This is what the disposable copy held.
            World world = new World().Reconcile(true).AddCombined().AddBuiltFromBrownFur();
            BrownFurPublicationReading reading = world.Read(true);
            BrownFurPublicationAudit audit = Judge(reading);
            Assertions.True(audit.Passed, "A valid publication followed by later foreign additions failed: " +
                audit.DescribeFailures());

            // The transaction ran when there were six, and says so. The
            // number the selector holds now is not what it found.
            Assertions.True(reading.Evidence.Any(value => value.Contains("before=6;after=7;boundary=False;index=6")) &&
                !reading.Evidence.Any(value => value.Contains("before=14;after=15")),
                "The transaction's evidence is not the six-to-seven record this case is about.");
            Assertions.True(audit.Result(BrownFurPublicationAudit.Transaction).Observed.Contains(
                "action=published;before=6;after=7;boundary=False;index=6"),
                "The transaction result does not report the transaction's own record.");

            // The later additions are listed by identity, in order.
            Assertions.True(audit.LaterAdditions.Select(value => value.Guid).SequenceEqual(
                BrownFurArchetypeOrdering.KnownCombinedArchetypeGuids.Concat(BuiltFromBrownFur.Select(value => value[0]))),
                "The later foreign additions were not listed by identity in selector order.");
            string preservation = audit.Result(BrownFurPublicationAudit.Preservation).Observed;
            Assertions.True(preservation.Contains("atReconciliation=6;stillPresent=6;missing=0;sameOrder=True;" +
                "foreignNow=14;laterAdditions=8") && preservation.Contains(BuiltFromBrownFur[1][0]) &&
                preservation.Contains("known-combined"),
                "The preservation result does not separate what was found from what was added later: " + preservation);
            JObject ledger = audit.ToJson();
            string[] stages = ledger["finalSelector"]["entries"].Select(value => (string)value["stage"]).ToArray();
            Assertions.True(stages.Take(6).All(value => value == "present-at-reconciliation") &&
                stages[6] == "brown-fur" && stages.Skip(7).All(value => value == "added-later") && stages.Length == 15 &&
                ledger["transactionTime"]["surfaces"][0]["before"].Count() == 6,
                "The ledger does not give each entry of the final selector its stage.");

            // The same publication with nothing added afterwards.
            audit = Judge(new World().Reconcile(true).Read(true));
            Assertions.True(audit.Passed && audit.LaterAdditions.Count == 0,
                "A publication with no later addition failed: " + audit.DescribeFailures());

            // And where the combined archetypes were there first: the
            // archetype goes in ahead of them, and the record says eleven.
            World after = new World().AddCombined().Reconcile(true);
            audit = Judge(after.Read(true));
            Assertions.True(audit.Passed && audit.Result(BrownFurPublicationAudit.Transaction).Observed.Contains(
                "before=11;after=12;boundary=True;index=6") && after.Selector[6] == after.BrownFur,
                "A publication ahead of combined archetypes already present failed: " + audit.DescribeFailures());
        }

        internal static void TransactionIsJudgedByItsOwnRecord()
        {
            Func<BrownFurPublicationReading> published = () =>
                new World().Reconcile(true).AddCombined().AddBuiltFromBrownFur().Read(true);

            // No evidence of any transaction beside a published archetype.
            BrownFurPublicationReading reading = published();
            reading.Evidence = new List<string>();
            reading.Observations = new List<BrownFurSurfaceObservation>();
            OnlyFails(Judge(reading), "A published archetype with no transaction evidence passed.",
                BrownFurPublicationAudit.Transaction);

            // The surface was written but the transaction never committed.
            reading = published();
            reading.Evidence = reading.Evidence.Where(value => !value.Contains("action=committed")).ToList();
            BrownFurPublicationAudit audit = Judge(reading);
            OnlyFails(audit, "A transaction with no committed record passed.", BrownFurPublicationAudit.Transaction);
            Assertions.True(audit.Result(BrownFurPublicationAudit.Transaction).Observed.Contains("no committed record"),
                "The missing commit was not named.");

            // The evidence text and the identity record disagree.
            reading = published();
            reading.Evidence = reading.Evidence.Select(value => value.Replace("before=6;after=7", "before=14;after=15")).ToList();
            audit = Judge(reading);
            OnlyFails(audit, "Evidence text that disagrees with the transaction's record passed.",
                BrownFurPublicationAudit.Transaction);
            Assertions.True(audit.Result(BrownFurPublicationAudit.Transaction).Observed.Contains(
                "the evidence text does not say"), "The disagreement was not named.");

            // The record itself is not an insertion: an entry it found is gone afterwards.
            reading = published();
            BrownFurSurfaceObservation real = reading.Observations.Single();
            reading.Observations = new List<BrownFurSurfaceObservation> { new BrownFurSurfaceObservation(real.Key,
                real.Action, real.Before, real.After.Where(value => value != Standalone[2][0]), real.BoundaryFound,
                real.Index) };
            OnlyFails(Judge(reading), "A record that lost an entry it found passed.",
                BrownFurPublicationAudit.Transaction);

            // The archetype was already in the selector the transaction found.
            reading = published();
            reading.Observations = new List<BrownFurSurfaceObservation> { new BrownFurSurfaceObservation(real.Key,
                "unchanged", real.After, real.After, real.BoundaryFound, real.Index) };
            OnlyFails(Judge(reading), "A transaction that published nothing passed.",
                BrownFurPublicationAudit.Transaction);

            // The selector recorded at reconciliation is not the one the transaction found.
            reading = published();
            reading.ReconciledSelector = real.Before.Take(5).ToArray();
            audit = Judge(reading);
            OnlyFails(audit, "A transaction whose starting selector differs from the reconciled one passed.",
                BrownFurPublicationAudit.Transaction);
            reading = published();
            reading.ReconciledSelector = null;
            OnlyFails(Judge(reading), "A transaction with no reconciled selector recorded passed.",
                BrownFurPublicationAudit.Transaction);

            // The recorded position does not follow the ordering rule.
            reading = published();
            string[] moved = new[] { BrownFurGuid }.Concat(real.Before).ToArray();
            reading.Observations = new List<BrownFurSurfaceObservation> { new BrownFurSurfaceObservation(real.Key,
                real.Action, real.Before, moved, false, 0) };
            reading.Evidence = reading.Evidence.Select(value => value.Replace("index=6", "index=0")).ToList();
            audit = Judge(reading);
            OnlyFails(audit, "An insertion at a position the ordering rule does not give passed.",
                BrownFurPublicationAudit.Transaction);
        }

        internal static void LostOrReorderedForeignArchetypesFailByIdentity()
        {
            // One archetype that was there is replaced by another: the
            // selector is as long as before and holds as many foreign
            // archetypes as before.
            World world = new World().Reconcile(true).AddCombined();
            int index = world.Selector.FindIndex(value => value.Id == Standalone[3][0]);
            world.Selector[index] = new Fake("0f0e0d0c0b0a09080706050403020100", "SomeOtherArchetype");
            BrownFurPublicationAudit audit = Judge(world.Read(true));
            OnlyFails(audit, "A foreign archetype replaced by another passed.", BrownFurPublicationAudit.Preservation);
            Assertions.True(audit.MissingForeign.SequenceEqual(new[] { Standalone[3][0] }) &&
                audit.Result(BrownFurPublicationAudit.Preservation).Observed.Contains("stillPresent=5;missing=1: " +
                    Standalone[3][0]) &&
                audit.LaterAdditions.Any(value => value.Guid == "0f0e0d0c0b0a09080706050403020100"),
                "The lost archetype was not named, or its replacement was not listed.");

            // Two archetypes that were there change places.
            world = new World().Reconcile(true).AddCombined();
            Fake first = world.Selector[0];
            world.Selector[0] = world.Selector[1];
            world.Selector[1] = first;
            audit = Judge(world.Read(true));
            OnlyFails(audit, "Reordered foreign archetypes passed.", BrownFurPublicationAudit.Preservation);
            Assertions.True(audit.Result(BrownFurPublicationAudit.Preservation).Observed.Contains("missing=0;sameOrder=False"),
                "The reordering was not reported.");

            // Fewer standalone archetypes than Call of the Wild provides.
            audit = Judge(new World(5).Reconcile(true).Read(true));
            OnlyFails(audit, "Fewer than the six standalone archetypes passed.", BrownFurPublicationAudit.Preservation);
        }

        internal static void OccurrencesDuplicatesAndUnreadableEntriesAreNamed()
        {
            // The registered archetype twice.
            World world = new World().Reconcile(true).AddCombined();
            world.Selector.Add(world.BrownFur);
            BrownFurPublicationAudit audit = Judge(world.Read(true));
            OnlyFails(audit, "The archetype twice in the selector passed.", BrownFurPublicationAudit.Occurrences,
                BrownFurPublicationAudit.Integrity);
            Assertions.True(audit.Result(BrownFurPublicationAudit.Occurrences).Observed.StartsWith(
                "byReference=2;byIdentity=2", StringComparison.Ordinal) &&
                audit.Result(BrownFurPublicationAudit.Integrity).Observed.Contains(BrownFurGuid + " x2"),
                "The repeated archetype was not counted and named.");

            // Another object carrying the archetype's identity.
            world = new World().Reconcile(true);
            world.Selector.Add(new Fake(BrownFurGuid, "Impostor"));
            audit = Judge(world.Read(true));
            Assertions.True(!audit.Result(BrownFurPublicationAudit.Occurrences).Passed &&
                audit.Result(BrownFurPublicationAudit.Occurrences).Observed.StartsWith(
                    "byReference=1;byIdentity=2", StringComparison.Ordinal),
                "A second object with the archetype's identity passed.");

            // The archetype in another class's selector as well.
            BrownFurPublicationReading reading = new World().Reconcile(true).Read(true);
            reading.ReferencesInAnyClass = 2;
            OnlyFails(Judge(reading), "The archetype in a second class passed.", BrownFurPublicationAudit.Occurrences);

            // An empty position and one whose identity cannot be read: each
            // is named with its place, and everything else is still judged.
            world = new World().Reconcile(true).AddCombined();
            world.Selector.Insert(2, null);
            world.Selector.Add(new Fake("unused", "Broken") { IdentityThrows = true });
            audit = Judge(world.Read(true));
            OnlyFails(audit, "Unreadable selector positions passed.", BrownFurPublicationAudit.Integrity);
            string integrity = audit.Result(BrownFurPublicationAudit.Integrity).Observed;
            Assertions.True(integrity.Contains("unreadable=2") && integrity.Contains("[2] <no identity> <unnamed> (null entry)") &&
                integrity.Contains("Broken") && integrity.Contains("NullReferenceException"),
                "The unreadable positions were not named with place and reason: " + integrity);
            Assertions.True(audit.Result(BrownFurPublicationAudit.Ordering).Observed.StartsWith(
                "brownFurIndex=7;firstKnownCombined=8", StringComparison.Ordinal) &&
                audit.Result(BrownFurPublicationAudit.Preservation).Observed.Contains("stillPresent=6;missing=0"),
                "An unreadable position hid the readable entries around it.");

            // No selector at all.
            reading = new World().Reconcile(true).Read(true);
            string fault;
            reading.Selector = BrownFurSelectorEntry.Read<Fake>(() => { throw new InvalidOperationException("gone"); },
                value => value.Id, value => value.Name, value => false, out fault);
            reading.SelectorFault = fault;
            audit = Judge(reading);
            Assertions.True(!audit.Result(BrownFurPublicationAudit.Integrity).Passed &&
                audit.Result(BrownFurPublicationAudit.Integrity).Observed.Contains("selector unavailable") &&
                !audit.Result(BrownFurPublicationAudit.Occurrences).Passed &&
                audit.Result(BrownFurPublicationAudit.Transaction).Passed,
                "An unavailable selector passed, or took the transaction's own result with it.");
        }

        internal static void OrderingAgainstKnownCombinedArchetypes()
        {
            // A later provider puts a known combined archetype ahead of
            // Brown-Fur. The transaction was right when it ran; the final
            // order is wrong, and only the ordering result says so.
            World world = new World().Reconcile(true);
            world.Selector.Insert(0, new Fake(BrownFurArchetypeOrdering.KnownCombinedArchetypeGuids[0], "CombinedAhead"));
            BrownFurPublicationAudit audit = Judge(world.Read(true));
            OnlyFails(audit, "A known combined archetype ahead of Brown-Fur passed.", BrownFurPublicationAudit.Ordering);
            Assertions.True(audit.Result(BrownFurPublicationAudit.Ordering).Observed ==
                "brownFurIndex=7;firstKnownCombined=0;knownCombined=1",
                "The final order was not reported.");

            // Combined archetypes after it are in order, however many.
            audit = Judge(new World().Reconcile(true).AddCombined().Read(true));
            Assertions.True(audit.Passed && audit.Result(BrownFurPublicationAudit.Ordering).Observed ==
                "brownFurIndex=6;firstKnownCombined=7;knownCombined=5", "The ordinary final order failed.");
        }

        internal static void ModuleOffContract()
        {
            // The module off beside the same providers: nothing of
            // Brown-Fur in the selector, no transaction, and what Favored
            // Class adds later is listed against the reconciled selector.
            World world = new World().Reconcile(false).AddCombined();
            BrownFurPublicationAudit audit = Judge(world.Read(false));
            Assertions.True(audit.Passed && audit.LaterAdditions.Count == 5 &&
                audit.Result(BrownFurPublicationAudit.Transaction).Expected.Contains("no publication transaction") &&
                audit.Result(BrownFurPublicationAudit.Preservation).Observed.Contains("atReconciliation=6;stillPresent=6"),
                "A clean module-off configuration failed: " + audit.DescribeFailures());

            // Published although the module is requested off.
            audit = Judge(new World().Reconcile(true).AddCombined().Read(false));
            OnlyFails(audit, "A publication with the module requested off passed.",
                BrownFurPublicationAudit.ModuleState, BrownFurPublicationAudit.Occurrences,
                BrownFurPublicationAudit.Ordering, BrownFurPublicationAudit.Transaction);

            // The archetype in the selector with no transaction to account for it.
            world = new World().Reconcile(false);
            world.Selector.Add(world.BrownFur);
            audit = Judge(world.Read(false));
            OnlyFails(audit, "The archetype in the selector with the module off passed.",
                BrownFurPublicationAudit.Occurrences, BrownFurPublicationAudit.Ordering);

            // Requested on, and the module did not publish.
            audit = Judge(new World().Reconcile(false).Read(true));
            OnlyFails(audit, "A requested publication that did not happen passed.",
                BrownFurPublicationAudit.ModuleState, BrownFurPublicationAudit.Occurrences,
                BrownFurPublicationAudit.Ordering, BrownFurPublicationAudit.Transaction);

            // A foreign archetype lost while the module is off is still seen.
            world = new World().Reconcile(false);
            world.Selector.RemoveAt(4);
            OnlyFails(Judge(world.Read(false)), "A lost foreign archetype passed with the module off.",
                BrownFurPublicationAudit.Preservation);
        }

        internal static void AbsentProviderContract()
        {
            Func<BrownFurPublicationReading> absent = () => new BrownFurPublicationReading
            {
                ProviderAbsent = true, Requested = true, Active = true,
                Availability = BrownFurDependencyAvailability.Unavailable, ContractObjectsAbsent = true,
                ArchetypeGuid = BrownFurGuid
            };
            BrownFurPublicationAudit audit = Judge(absent());
            Assertions.True(audit.Passed && audit.Results.Select(value => value.Name).SequenceEqual(new[] {
                BrownFurPublicationAudit.Provider, BrownFurPublicationAudit.ModuleState,
                BrownFurPublicationAudit.Identities, BrownFurPublicationAudit.Occurrences,
                BrownFurPublicationAudit.Transaction }),
                "A clean absent-provider configuration failed: " + audit.DescribeFailures());

            BrownFurPublicationReading reading = absent();
            reading.RegisteredIdentities = BrownFurIdentityCatalog.IdentityCount;
            OnlyFails(Judge(reading), "Identities registered without the provider passed.",
                BrownFurPublicationAudit.Identities);
            reading = absent();
            reading.StatusPublished = true;
            OnlyFails(Judge(reading), "A published status without the provider passed.",
                BrownFurPublicationAudit.ModuleState);
            reading = absent();
            reading.ReferencesInAnyClass = 1;
            OnlyFails(Judge(reading), "The archetype in a class selector without the provider passed.",
                BrownFurPublicationAudit.Occurrences);
            reading = absent();
            reading.Evidence = new List<string> { "transaction;action=committed;surfaces=2" };
            OnlyFails(Judge(reading), "A publication transaction without the provider passed.",
                BrownFurPublicationAudit.Transaction);
            reading = absent();
            reading.Availability = BrownFurDependencyAvailability.Blocked;
            OnlyFails(Judge(reading), "A blocked dependency was read as an absent provider.",
                BrownFurPublicationAudit.Provider);
            reading = absent();
            reading.ContractObjectsAbsent = false;
            OnlyFails(Judge(reading), "A resolved contract without the provider passed.",
                BrownFurPublicationAudit.Provider);
        }

        internal static void TransactionRecordsIdentities()
        {
            // No boundary: appended after what was found.
            World world = new World().Reconcile(true);
            BrownFurSurfaceObservation record = world.Transaction.Observations.Single();
            Assertions.True(record.Key == BrownFurPublicationAudit.ArchetypeSurface && record.Action == "published" &&
                record.Before.SequenceEqual(Standalone.Select(value => value[0])) &&
                record.After.SequenceEqual(Standalone.Select(value => value[0]).Concat(new[] { BrownFurGuid })) &&
                !record.BoundaryFound && record.Index == 6,
                "The transaction did not record what it found and what it left, by identity.");
            // A second commit is the same transaction and records nothing more.
            world.Transaction.Commit();
            Assertions.Equal(1, world.Transaction.Observations.Count, "A repeated commit recorded a second observation.");

            // A boundary: inserted ahead of it, and recorded there.
            world = new World().AddCombined().Reconcile(true);
            record = world.Transaction.Observations.Single();
            Assertions.True(record.BoundaryFound && record.Index == 6 && record.Before.Length == 11 &&
                record.After[6] == BrownFurGuid &&
                record.After[7] == BrownFurArchetypeOrdering.KnownCombinedArchetypeGuids[0],
                "An insertion ahead of the combined archetypes was not recorded at its position.");

            // A fresh transaction over a selector that already holds the
            // archetype changes nothing and says so.
            IList<Fake> selector = world.Selector;
            var again = new BrownFurPublicationTransaction().InsertBefore(BrownFurPublicationAudit.ArchetypeSurface,
                () => selector, value => selector = value, new[] { world.BrownFur }, value => value.Id,
                value => BrownFurArchetypeOrdering.IsKnownCombinedArchetype(value.Id));
            again.Commit();
            record = again.Observations.Single();
            Assertions.True(record.Action == "unchanged" && record.Before.SequenceEqual(record.After) &&
                record.Before.Length == 12, "An unchanged surface was not recorded as unchanged.");

            // An appended surface records its identities too.
            IList<Fake> plain = new List<Fake> { new Fake("a", "A") };
            var append = new BrownFurPublicationTransaction().Append("other", () => plain, value => plain = value,
                new[] { new Fake("b", "B") }, value => value.Id);
            append.Commit();
            record = append.Observations.Single();
            Assertions.True(record.Before.SequenceEqual(new[] { "a" }) && record.After.SequenceEqual(new[] { "a", "b" }) &&
                record.Index == 1 && !record.BoundaryFound, "An appended surface was not recorded by identity.");
        }
    }
}
