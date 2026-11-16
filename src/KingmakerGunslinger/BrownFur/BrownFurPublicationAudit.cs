using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using KingmakerGunslinger.FeatureModules;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.BrownFur
{
    /// <summary>One position of the Arcanist archetype selector as it stands now.</summary>
    internal sealed class BrownFurSelectorEntry
    {
        internal int Index;
        internal string Guid;
        internal string Name;
        /// <summary>The entry is the very archetype object this module registered.</summary>
        internal bool IsRegisteredArchetype;
        /// <summary>Why the position could not be read; null when it was.</summary>
        internal string Fault;

        /// <summary>
        /// Reads a selector position by position. A position that cannot be
        /// read is kept with its reason and the positions around it are
        /// still read; a selector that cannot be obtained is reported
        /// through <paramref name="fault"/>.
        /// </summary>
        internal static List<BrownFurSelectorEntry> Read<T>(Func<IList<T>> selector,
            Func<T, string> guidOf, Func<T, string> nameOf,
            Func<T, bool> isRegisteredArchetype, out string fault) where T : class
        {
            var result = new List<BrownFurSelectorEntry>();
            fault = null;
            IList<T> values;
            int count;
            try
            {
                values = selector();
                count = values == null ? 0 : values.Count;
            }
            catch (Exception exception)
            {
                fault = "selector unavailable: " + Reason(exception);
                return result;
            }

            for (int index = 0; index < count; index++)
            {
                var entry = new BrownFurSelectorEntry { Index = index };
                result.Add(entry);
                T value;
                try { value = values[index]; }
                catch (Exception exception) { entry.Fault = "entry unavailable: " + Reason(exception); continue; }
                if (value == null) { entry.Fault = "null entry"; continue; }
                try { entry.IsRegisteredArchetype = isRegisteredArchetype(value); }
                catch (Exception exception) { entry.Fault = "entry unavailable: " + Reason(exception); }
                try { entry.Guid = guidOf(value); }
                catch (Exception exception) { entry.Fault = "identity unavailable: " + Reason(exception); }
                try { entry.Name = nameOf(value); }
                catch (Exception exception) { entry.Name = "<name unavailable: " + Reason(exception) + ">"; }
                if (entry.Fault == null && string.IsNullOrWhiteSpace(entry.Guid)) entry.Fault = "entry has no identity";
            }
            return result;
        }

        private static string Reason(Exception exception)
        {
            return exception.GetType().Name + (string.IsNullOrEmpty(exception.Message) ? string.Empty : ": " + exception.Message);
        }

        internal string Describe()
        {
            return "[" + Index.ToString(CultureInfo.InvariantCulture) + "] " + (Guid ?? "<no identity>") + " " +
                (Name ?? "<unnamed>") + (Fault == null ? string.Empty : " (" + Fault + ")");
        }
    }

    /// <summary>Everything the Brown-Fur publication check reads, gathered before any of it is judged.</summary>
    internal sealed class BrownFurPublicationReading
    {
        /// <summary>Call of the Wild is not loaded in this process.</summary>
        internal bool ProviderAbsent;
        /// <summary>The module state the request names; the expectation follows this.</summary>
        internal bool Requested;
        /// <summary>The module state the process actually runs.</summary>
        internal bool Active;
        internal BrownFurDependencyAvailability Availability;
        /// <summary>The module's own status says it published.</summary>
        internal bool StatusPublished;
        /// <summary>A compatible contract was resolved and the full identity set was built.</summary>
        internal bool ContractCompatible;
        /// <summary>Neither a resolution nor a blueprint set exists.</summary>
        internal bool ContractObjectsAbsent;
        /// <summary>How many of the module's declared identities the library holds.</summary>
        internal int RegisteredIdentities;
        /// <summary>The archetype's declared identity.</summary>
        internal string ArchetypeGuid;
        /// <summary>The Arcanist selector now, after every provider has finished.</summary>
        internal IList<BrownFurSelectorEntry> Selector = new List<BrownFurSelectorEntry>();
        internal string SelectorFault;
        /// <summary>Occurrences of any Brown-Fur identity in any class's archetype selector.</summary>
        internal int ReferencesInAnyClass;
        /// <summary>The transaction's own evidence text.</summary>
        internal IList<string> Evidence = new List<string>();
        /// <summary>What the transaction held of each list surface when it applied.</summary>
        internal IList<BrownFurSurfaceObservation> Observations = new List<BrownFurSurfaceObservation>();
        /// <summary>The selector as first reconciled, before any Brown-Fur change; null when never reconciled.</summary>
        internal IList<string> ReconciledSelector;
    }

    /// <summary>One named result of the check.</summary>
    internal sealed class BrownFurNamedResult
    {
        internal BrownFurNamedResult(string name, string expected, string observed, bool passed)
        { Name = name; Expected = expected; Observed = observed; Passed = passed; }

        /// <summary>The suffix that names this result beside the aggregate.</summary>
        internal string Name { get; private set; }
        internal string Expected { get; private set; }
        internal string Observed { get; private set; }
        internal bool Passed { get; private set; }
    }

    /// <summary>
    /// The Brown-Fur publication boundary, judged as separate named results.
    ///
    /// Two moments are kept apart. The transaction speaks of the selector
    /// at the moment this module published: what was there, and where the
    /// archetype went. The final selector is what stands after every other
    /// mod has finished, which may hold archetypes added later. The
    /// transaction is judged against its own record; the final selector is
    /// judged for what must still be true of it, and what was added later
    /// is listed, not mistaken for a failed transaction.
    /// </summary>
    internal sealed class BrownFurPublicationAudit
    {
        internal const string ArchetypeSurface = "cotw-arcanist-archetypes";
        internal const string LedgerFile = "brown-fur-publication-ledger.json";
        /// <summary>Call of the Wild's own standalone Arcanist archetypes, the floor the check has always held.</summary>
        internal const int StandaloneArchetypeFloor = 6;

        internal const string Provider = "provider";
        internal const string ModuleState = "module-state";
        internal const string Identities = "identities";
        internal const string Occurrences = "selector-occurrences";
        internal const string Integrity = "selector-integrity";
        internal const string Ordering = "ordering";
        internal const string Transaction = "transaction";
        internal const string Preservation = "foreign-preservation";

        private readonly List<BrownFurNamedResult> _results = new List<BrownFurNamedResult>();
        private BrownFurPublicationReading _reading;
        private readonly List<BrownFurSelectorEntry> _later = new List<BrownFurSelectorEntry>();
        private readonly List<string> _missing = new List<string>();

        internal IList<BrownFurNamedResult> Results { get { return _results.AsReadOnly(); } }
        internal bool Passed { get { return _results.Count != 0 && _results.All(value => value.Passed); } }
        /// <summary>Foreign archetypes in the final selector that were not there when this module reconciled.</summary>
        internal IList<BrownFurSelectorEntry> LaterAdditions { get { return _later.AsReadOnly(); } }
        /// <summary>Identities that were there when this module reconciled and are gone from the final selector.</summary>
        internal IList<string> MissingForeign { get { return _missing.AsReadOnly(); } }

        internal BrownFurNamedResult Result(string name)
        { return _results.SingleOrDefault(value => value.Name == name); }

        internal string DescribeFailures()
        {
            return string.Join("; ", _results.Where(value => !value.Passed).Select(value => value.Name + ": " +
                value.Observed).ToArray());
        }

        internal static BrownFurPublicationAudit Evaluate(BrownFurPublicationReading reading)
        {
            if (reading == null) throw new ArgumentNullException("reading");
            var audit = new BrownFurPublicationAudit { _reading = reading };
            if (reading.ProviderAbsent) audit.JudgeAbsent();
            else audit.JudgePresent();
            return audit;
        }

        private void Add(string name, string expected, string observed, bool passed)
        { _results.Add(new BrownFurNamedResult(name, expected, observed, passed)); }

        private static string Text(int value) { return value.ToString(CultureInfo.InvariantCulture); }

        // ---- Call of the Wild is not installed: nothing may exist

        private void JudgeAbsent()
        {
            BrownFurPublicationReading r = _reading;
            Add(Provider, "provider absent: dependency unavailable and no contract resolved",
                "provider=absent;availability=" + r.Availability + ";contract=" +
                    (r.ContractObjectsAbsent ? "none" : "present"),
                r.Availability == BrownFurDependencyAvailability.Unavailable && r.ContractObjectsAbsent);
            Add(ModuleState, "not published, whatever is requested, because the provider is absent",
                DescribeState(), !r.StatusPublished);
            Add(Identities, "none of the module's identities registered",
                "registered=" + Text(r.RegisteredIdentities), r.RegisteredIdentities == 0);
            int byGuid = r.Selector.Count(value => value.Fault == null && value.Guid == r.ArchetypeGuid);
            Add(Occurrences, "the archetype in no class's selector",
                "inAnyClass=" + Text(r.ReferencesInAnyClass) + ";inProviderSelector=" + Text(byGuid),
                r.ReferencesInAnyClass == 0 && byGuid == 0);
            Add(Transaction, "no publication transaction",
                DescribeTransactionRecord(), r.Evidence.Count == 0 && r.Observations.Count == 0);
        }

        // ---- Call of the Wild is installed

        private void JudgePresent()
        {
            BrownFurPublicationReading r = _reading;
            bool expectPublished = r.Requested;
            Add(Provider, "provider present: compatible contract resolved and dependency available",
                "provider=present;availability=" + r.Availability + ";contractCompatible=" + r.ContractCompatible,
                r.ContractCompatible && r.Availability == BrownFurDependencyAvailability.Available);
            Add(ModuleState, expectPublished ? "requested on: published" : "requested off: not published",
                DescribeState(), r.StatusPublished == expectPublished);
            Add(Identities, Text(BrownFurIdentityCatalog.IdentityCount) + " identities registered in either module state",
                "registered=" + Text(r.RegisteredIdentities),
                r.RegisteredIdentities == BrownFurIdentityCatalog.IdentityCount);

            BrownFurSelectorEntry[] readable = r.Selector.Where(value => value.Fault == null).ToArray();
            int byReference = readable.Count(value => value.IsRegisteredArchetype);
            int byGuid = readable.Count(value => value.Guid == r.ArchetypeGuid);
            int expectedOccurrences = expectPublished ? 1 : 0;
            Add(Occurrences, "the archetype " + (expectPublished ? "exactly once" : "nowhere") +
                    " in the Arcanist selector, by reference and by identity, and in no other class's selector",
                "byReference=" + Text(byReference) + ";byIdentity=" + Text(byGuid) + ";inAnyClass=" +
                    Text(r.ReferencesInAnyClass),
                byReference == expectedOccurrences && byGuid == expectedOccurrences &&
                    r.ReferencesInAnyClass == expectedOccurrences);

            BrownFurSelectorEntry[] unreadable = r.Selector.Where(value => value.Fault != null).ToArray();
            string[] repeated = readable.GroupBy(value => value.Guid, StringComparer.Ordinal)
                .Where(group => group.Count() > 1).Select(group => group.Key + " x" + Text(group.Count())).ToArray();
            Add(Integrity, "the selector readable, with no empty position and no identity twice",
                (r.SelectorFault == null ? "entries=" + Text(r.Selector.Count) : r.SelectorFault) +
                    ";unreadable=" + Text(unreadable.Length) + List(unreadable.Select(value => value.Describe())) +
                    ";duplicates=" + Text(repeated.Length) + List(repeated),
                r.SelectorFault == null && unreadable.Length == 0 && repeated.Length == 0);

            int index = Array.FindIndex(r.Selector.ToArray(), value => value.Fault == null && value.IsRegisteredArchetype);
            int firstCombined = Array.FindIndex(r.Selector.ToArray(), value => value.Fault == null &&
                BrownFurArchetypeOrdering.IsKnownCombinedArchetype(value.Guid));
            int knownCombined = readable.Count(value => BrownFurArchetypeOrdering.IsKnownCombinedArchetype(value.Guid));
            Add(Ordering, expectPublished ?
                    "the archetype ahead of every known combined archetype in the final selector" :
                    "the archetype not in the final selector",
                "brownFurIndex=" + Text(index) + ";firstKnownCombined=" + Text(firstCombined) + ";knownCombined=" +
                    Text(knownCombined),
                expectPublished ? index >= 0 && (firstCombined < 0 || index < firstCombined) : index < 0);

            if (expectPublished) JudgeTransaction();
            else
                Add(Transaction, "no publication transaction while the module is off",
                    DescribeTransactionRecord(), r.Evidence.Count == 0 && r.Observations.Count == 0);
            JudgePreservation(expectPublished, readable);
        }

        /// <summary>
        /// The transaction against its own record, at the moment it ran.
        /// Nothing here is derived from the final selector.
        /// </summary>
        private void JudgeTransaction()
        {
            BrownFurPublicationReading r = _reading;
            const string expected = "committed; the archetype inserted once into the selector as the transaction " +
                "found it, at the recorded position, with every entry found there kept in order; the evidence text " +
                "agreeing with that record";
            var problems = new List<string>();
            if (!r.Evidence.Any(value => value != null && value.Contains("transaction;action=committed")))
                problems.Add("no committed record");
            BrownFurSurfaceObservation[] surfaces = r.Observations.Where(value => value != null &&
                value.Key == ArchetypeSurface).ToArray();
            if (surfaces.Length != 1)
            {
                problems.Add(Text(surfaces.Length) + " records of the archetype surface; one is expected");
                Add(Transaction, expected, DescribeTransactionRecord() + List(problems), false);
                return;
            }

            BrownFurSurfaceObservation surface = surfaces[0];
            if (surface.Action != "published") problems.Add("the surface was " + surface.Action + ", not published");
            if (surface.Before.Contains(r.ArchetypeGuid, StringComparer.Ordinal))
                problems.Add("the archetype was already in the selector the transaction found");
            if (surface.Before.Any(string.IsNullOrWhiteSpace) ||
                surface.Before.Distinct(StringComparer.Ordinal).Count() != surface.Before.Length)
                problems.Add("the selector the transaction found held an empty or repeated identity");
            bool inserted = surface.Index >= 0 && surface.Index <= surface.Before.Length &&
                surface.After.Length == surface.Before.Length + 1 &&
                surface.After[surface.Index] == r.ArchetypeGuid &&
                surface.After.Where((value, position) => position != surface.Index)
                    .SequenceEqual(surface.Before, StringComparer.Ordinal);
            if (!inserted)
                problems.Add("the selector after is not the selector before with the archetype at position " +
                    Text(surface.Index));
            int boundary = Array.FindIndex(surface.Before, BrownFurArchetypeOrdering.IsKnownCombinedArchetype);
            if (surface.BoundaryFound != (boundary >= 0) ||
                surface.Index != (boundary >= 0 ? boundary : surface.Before.Length))
                problems.Add("the recorded position does not follow the ordering rule: first known combined archetype " +
                    "the transaction found was at " + Text(boundary));
            string text = "surface=" + ArchetypeSurface + ";action=published;before=" + Text(surface.Before.Length) +
                ";after=" + Text(surface.After.Length) + ";boundary=" + surface.BoundaryFound + ";index=" +
                Text(surface.Index);
            if (!r.Evidence.Any(value => value != null && value.Contains(text)))
                problems.Add("the evidence text does not say '" + text + "'");
            if (r.ReconciledSelector == null)
                problems.Add("no selector was recorded at reconciliation");
            else if (!r.ReconciledSelector.SequenceEqual(surface.Before, StringComparer.Ordinal))
                problems.Add("the selector recorded at reconciliation is not the selector the transaction found");
            Add(Transaction, expected, DescribeTransactionRecord() + List(problems), problems.Count == 0);
        }

        /// <summary>
        /// The final selector against the selector as this module first
        /// found it: every archetype that was there is still there, in the
        /// same order. What was added since is listed.
        /// </summary>
        private void JudgePreservation(bool expectPublished, BrownFurSelectorEntry[] readable)
        {
            BrownFurPublicationReading r = _reading;
            BrownFurSelectorEntry[] foreign = readable.Where(value => value.Guid != r.ArchetypeGuid).ToArray();
            const string expected = "every foreign archetype found at reconciliation still in the final selector, in " +
                "the same order; at least the six standalone Call of the Wild archetypes; later additions listed";
            // With the module on, the transaction's own record of what it
            // found is the reference; with it off there is no transaction,
            // and the selector recorded at reconciliation is.
            BrownFurSurfaceObservation surface = !expectPublished ? null : r.Observations.FirstOrDefault(value =>
                value != null && value.Key == ArchetypeSurface);
            IList<string> then = surface != null ? surface.Before : r.ReconciledSelector;
            if (then == null)
            {
                Add(Preservation, expected, "foreignNow=" + Text(foreign.Length) +
                    ";no record of the selector at reconciliation, so preservation cannot be judged", false);
                return;
            }

            var before = new HashSet<string>(then.Where(value => value != null && value != r.ArchetypeGuid),
                StringComparer.Ordinal);
            string[] survivors = foreign.Where(value => before.Contains(value.Guid)).Select(value => value.Guid).ToArray();
            _missing.AddRange(then.Where(value => value != null && value != r.ArchetypeGuid &&
                !survivors.Contains(value, StringComparer.Ordinal)));
            bool sameOrder = survivors.SequenceEqual(then.Where(value => value != null && value != r.ArchetypeGuid &&
                survivors.Contains(value, StringComparer.Ordinal)), StringComparer.Ordinal);
            _later.AddRange(foreign.Where(value => !before.Contains(value.Guid)));
            Add(Preservation, expected,
                "atReconciliation=" + Text(before.Count) + ";stillPresent=" + Text(survivors.Length) + ";missing=" +
                    Text(_missing.Count) + List(_missing) + ";sameOrder=" + sameOrder + ";foreignNow=" +
                    Text(foreign.Length) + ";laterAdditions=" + Text(_later.Count) +
                    List(_later.Select(value => value.Describe() +
                        (BrownFurArchetypeOrdering.IsKnownCombinedArchetype(value.Guid) ? " known-combined" : string.Empty))),
                _missing.Count == 0 && sameOrder && foreign.Length >= StandaloneArchetypeFloor);
        }

        private string DescribeState()
        {
            BrownFurPublicationReading r = _reading;
            return "requested=" + (r.Requested ? "enabled" : "disabled") + ";active=" +
                (r.Active ? "enabled" : "disabled") + ";statusPublished=" + r.StatusPublished;
        }

        private string DescribeTransactionRecord()
        {
            BrownFurPublicationReading r = _reading;
            BrownFurSurfaceObservation surface = r.Observations.FirstOrDefault(value => value != null &&
                value.Key == ArchetypeSurface);
            return "evidence=" + Text(r.Evidence.Count) + ";surfaceRecords=" + Text(r.Observations.Count) +
                (surface == null ? string.Empty : ";action=" + surface.Action + ";before=" + Text(surface.Before.Length) +
                    ";after=" + Text(surface.After.Length) + ";boundary=" + surface.BoundaryFound + ";index=" +
                    Text(surface.Index));
        }

        private static string List(IEnumerable<string> values)
        {
            string[] all = values.ToArray();
            return all.Length == 0 ? string.Empty : ": " + string.Join(" | ", all);
        }

        /// <summary>Every result, the final selector with each entry's stage, and the transaction's record.</summary>
        internal JObject ToJson()
        {
            BrownFurPublicationReading r = _reading;
            BrownFurSurfaceObservation surface = r.Observations.FirstOrDefault(value => value != null &&
                value.Key == ArchetypeSurface);
            IList<string> then = surface != null ? surface.Before : r.ReconciledSelector;
            var selector = new JArray();
            foreach (BrownFurSelectorEntry entry in r.Selector)
            {
                string stage = entry.Fault != null ? "unreadable" :
                    entry.Guid == r.ArchetypeGuid ? "brown-fur" :
                    then == null ? "not-judged" :
                    then.Contains(entry.Guid, StringComparer.Ordinal) ? "present-at-reconciliation" : "added-later";
                selector.Add(new JObject { ["index"] = entry.Index, ["guid"] = entry.Guid, ["name"] = entry.Name,
                    ["stage"] = stage,
                    ["knownCombined"] = entry.Fault == null && BrownFurArchetypeOrdering.IsKnownCombinedArchetype(entry.Guid),
                    ["isRegisteredArchetype"] = entry.IsRegisteredArchetype, ["fault"] = entry.Fault });
            }
            return new JObject {
                ["schemaVersion"] = 1,
                ["claim"] = "the Brown-Fur archetype in the Call of the Wild Arcanist selector; not selection in play",
                ["state"] = new JObject { ["providerAbsent"] = r.ProviderAbsent,
                    ["requestedModuleState"] = r.Requested ? "enabled" : "disabled",
                    ["activeModuleState"] = r.Active ? "enabled" : "disabled",
                    ["availability"] = r.Availability.ToString(), ["statusPublished"] = r.StatusPublished,
                    ["contractCompatible"] = r.ContractCompatible, ["registeredIdentities"] = r.RegisteredIdentities,
                    ["archetypeGuid"] = r.ArchetypeGuid, ["referencesInAnyClass"] = r.ReferencesInAnyClass },
                ["passed"] = Passed,
                ["results"] = new JArray(_results.Select(value => (object)new JObject { ["name"] = value.Name,
                    ["expected"] = value.Expected, ["observed"] = value.Observed, ["passed"] = value.Passed }).ToArray()),
                ["transactionTime"] = new JObject {
                    ["reconciledSelector"] = r.ReconciledSelector == null ? null :
                        new JArray(r.ReconciledSelector.Cast<object>().ToArray()),
                    ["evidence"] = new JArray(r.Evidence.Cast<object>().ToArray()),
                    ["surfaces"] = new JArray(r.Observations.Where(value => value != null).Select(value =>
                        (object)new JObject { ["key"] = value.Key, ["action"] = value.Action,
                            ["boundaryFound"] = value.BoundaryFound, ["index"] = value.Index,
                            ["before"] = new JArray(value.Before.Cast<object>().ToArray()),
                            ["after"] = new JArray(value.After.Cast<object>().ToArray()) }).ToArray()) },
                ["finalSelector"] = new JObject { ["fault"] = r.SelectorFault, ["entries"] = selector,
                    ["missingForeign"] = new JArray(_missing.Cast<object>().ToArray()) } };
        }
    }
}
