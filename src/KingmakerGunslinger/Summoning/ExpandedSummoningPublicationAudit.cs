using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.Summoning
{
    /// <summary>
    /// What a choice under a canonical summon parent is. Decided by the
    /// choice's identity against the catalogs the publisher reads and the
    /// identity manifest; a name is used only to notice a choice that claims
    /// the project's namespace without a declared identity.
    /// </summary>
    internal enum SummonChoiceClass
    {
        /// <summary>A logical placement (<c>KMG.Summoning.Ability.*</c>).</summary>
        Placement,
        /// <summary>A wrapper carved from a native option (<c>KMG.Summoning.NativeOption.*</c>).</summary>
        NativeExpansion,
        /// <summary>The child that keeps a direct tier-one summon (<c>KMG.Summoning.Native.*</c>).</summary>
        Preservation,
        /// <summary>A generated Spider Summoner row (<c>KMG.Summoning.Spider.*</c>).</summary>
        SpiderGenerated,
        /// <summary>A project identity that no publication catalog places under a parent.</summary>
        OwnedOther,
        /// <summary>Named as the project's, with no declared identity.</summary>
        ProjectNamed,
        /// <summary>One of the game's own options, declared in the native option catalog.</summary>
        NativeOption,
        /// <summary>A Races Unleashed row of its own, declared in the Spider Summoner catalog.</summary>
        ProviderOriginal,
        /// <summary>Declared by nothing this project holds.</summary>
        Foreign,
        Unreadable
    }

    internal enum SummonChoiceVerdict
    {
        /// <summary>Required here in this state, and present once.</summary>
        Required,
        /// <summary>The game's or a known provider's own choice, which publication keeps.</summary>
        Preserved,
        /// <summary>Kept because publication keeps every choice it does not own; whose it is was not established.</summary>
        NotAttributed,
        /// <summary>Present where the declared boundary does not allow it.</summary>
        Unexpected,
        /// <summary>A further occurrence of an identity already seen under the same parent.</summary>
        Duplicate,
        /// <summary>Required here in this state, and absent.</summary>
        Missing,
        Unreadable
    }

    /// <summary>One of the eighteen parents: a spell family and tier.</summary>
    internal sealed class SummonParentKey
    {
        internal SummonParentKey(SummonFamily family, int tier, string guid)
        {
            if (tier < 1 || tier > 9) throw new ArgumentOutOfRangeException("tier");
            if (string.IsNullOrWhiteSpace(guid)) throw new ArgumentException("A parent identity is required.", "guid");
            Family = family; Tier = tier; Guid = guid;
        }

        internal SummonFamily Family { get; private set; }
        internal int Tier { get; private set; }
        internal string Guid { get; private set; }

        /// <summary>The same parent, whichever object names it.</summary>
        internal bool Is(SummonParentKey other)
        { return other != null && string.Equals(Guid, other.Guid, StringComparison.Ordinal); }

        internal string Label
        {
            get
            {
                string[] numerals = { "I", "II", "III", "IV", "V", "VI", "VII", "VIII", "IX" };
                return (Family == SummonFamily.Monster ? "Summon Monster " : "Summon Nature's Ally ") + numerals[Tier - 1];
            }
        }
    }

    /// <summary>One identity the catalogs declare for one parent.</summary>
    internal sealed class SummonDeclaredChoice
    {
        internal string Guid;
        internal SummonChoiceClass Class;
        /// <summary>The manifest symbol, or the catalog's own name for a choice the project does not own.</summary>
        internal string Symbol;
        /// <summary>Which contract establishes the expectation.</summary>
        internal string Source;
        internal SummonParentKey Parent;
        /// <summary>A placement the visibility catalog publishes; false for a registered placement it withholds.</summary>
        internal bool Published = true;
        /// <summary>A native option, or preservation child, that complete replacement coverage removes when the module publishes.</summary>
        internal bool Replaced;
        /// <summary>The Spider Summoner species of a generated or original row.</summary>
        internal string SpeciesKey;

        internal bool Owned
        {
            get
            {
                return Class == SummonChoiceClass.Placement || Class == SummonChoiceClass.NativeExpansion ||
                    Class == SummonChoiceClass.Preservation || Class == SummonChoiceClass.SpiderGenerated;
            }
        }
    }

    /// <summary>The state a reading is judged in.</summary>
    internal sealed class SummonPublicationState
    {
        /// <summary>The module state the request names; the expectation follows this.</summary>
        internal bool Requested;
        /// <summary>The module state the process actually runs.</summary>
        internal bool Active;
        /// <summary>The Races Unleashed Spider Summoner feature is in the loaded library.</summary>
        internal bool SpiderFeaturePresent;
        /// <summary>Keys from <see cref="RowKey"/> for each Races Unleashed original row in the loaded library.</summary>
        internal readonly HashSet<string> SpiderProviderRows = new HashSet<string>(StringComparer.Ordinal);
        /// <summary>The integration's own report. It explains a missing row; it never decides an expectation.</summary>
        internal string SpiderIntegrationOutcome;

        internal static string RowKey(SummonFamily family, string speciesKey)
        { return family + ":" + speciesKey; }

        internal bool ProvidesSpiderRow(SummonFamily family, string speciesKey)
        { return SpiderFeaturePresent && SpiderProviderRows.Contains(RowKey(family, speciesKey)); }

        internal string Describe()
        {
            return "requested=" + (Requested ? "enabled" : "disabled") + ";active=" + (Active ? "enabled" : "disabled") +
                ";spiderSummonerFeature=" + (SpiderFeaturePresent ? "present" : "absent") + ";racesUnleashedRows=" +
                SpiderProviderRows.Count;
        }
    }

    /// <summary>
    /// What the eighteen parents may and must hold, built from the catalogs
    /// the publisher reads: the visibility catalog and identity catalog for
    /// placements, the native expansion catalog, the native option catalog
    /// and its replacement coverage, the two tier-one preservation children
    /// and the Spider Summoner catalog. Identities come from the manifest.
    /// Nothing here is read from a loaded game.
    /// </summary>
    internal sealed class ExpandedSummoningPublicationContract
    {
        internal const string ProjectNamePrefix = "KMG_";
        internal const string PlacementSource =
            "SummonVisibilityCatalog over ExpandedSummoningCatalog.GenerateVariants; identity by ExpandedSummoningIdentityCatalog.AbilitySymbol";
        internal const string NativeExpansionSource = "SummonNativeExpansionCatalog";
        internal const string PreservationSource =
            "ExpandedSummoningIdentityCatalog tier-one preservation child; kept or replaced by SummonNativeOptionCatalog.ReplacementCoverageIsComplete";
        internal const string SpiderSource =
            "SpiderSummonerCatalog.Generated; published by SpiderSummonerIntegration only for a loaded Races Unleashed source row";
        internal const string NativeOptionSource =
            "SummonNativeOptionCatalog; kept or replaced by ReplacementCoverageIsComplete";
        internal const string ProviderSource = "SpiderSummonerCatalog.RowIdentity (Races Unleashed's own row)";
        internal const string MergeSource =
            "SummonVariantMergePolicy.UnownedOriginals: publication keeps every choice it does not own";

        private readonly Dictionary<string, SummonDeclaredChoice> _byGuid =
            new Dictionary<string, SummonDeclaredChoice>(StringComparer.Ordinal);
        private readonly Dictionary<string, string> _ownedSymbols;
        private readonly List<SummonParentKey> _parents = new List<SummonParentKey>();

        private ExpandedSummoningPublicationContract(IDictionary<string, string> ownedSymbolsByGuid)
        {
            _ownedSymbols = new Dictionary<string, string>(ownedSymbolsByGuid, StringComparer.Ordinal);
        }

        internal IList<SummonParentKey> Parents { get { return _parents.AsReadOnly(); } }
        internal IEnumerable<SummonDeclaredChoice> Declared { get { return _byGuid.Values; } }

        internal SummonDeclaredChoice Find(string guid)
        {
            SummonDeclaredChoice found;
            return guid != null && _byGuid.TryGetValue(guid, out found) ? found : null;
        }

        /// <summary>The manifest symbol of a project identity, or null.</summary>
        internal string OwnedSymbol(string guid)
        {
            string symbol;
            return guid != null && _ownedSymbols.TryGetValue(guid, out symbol) ? symbol : null;
        }

        /// <param name="canonicalParents">The eighteen parents in the publisher's order: Summon Monster I to IX, then Summon Nature's Ally I to IX.</param>
        /// <param name="ownedSymbolsByGuid">Every manifest identity and its symbol.</param>
        internal static ExpandedSummoningPublicationContract Build(IList<string> canonicalParents,
            IDictionary<string, string> ownedSymbolsByGuid)
        {
            if (canonicalParents == null) throw new ArgumentNullException("canonicalParents");
            if (ownedSymbolsByGuid == null) throw new ArgumentNullException("ownedSymbolsByGuid");
            if (canonicalParents.Count != 18 || canonicalParents.Distinct(StringComparer.Ordinal).Count() != 18)
                throw new InvalidOperationException("The publication contract needs the eighteen distinct canonical parents.");
            var contract = new ExpandedSummoningPublicationContract(ownedSymbolsByGuid);
            var guidBySymbol = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (KeyValuePair<string, string> pair in ownedSymbolsByGuid) guidBySymbol[pair.Value] = pair.Key;
            Func<string, string> identity = symbol =>
            {
                string guid;
                if (!guidBySymbol.TryGetValue(symbol, out guid))
                    throw new InvalidOperationException("A declared publication symbol has no manifest identity: " + symbol);
                return guid;
            };
            SummonFamily[] families = { SummonFamily.Monster, SummonFamily.NaturesAlly };
            for (int familyIndex = 0; familyIndex < families.Length; familyIndex++)
            {
                SummonFamily family = families[familyIndex];
                IReadOnlyList<SummonVariantSpec> variants = ExpandedSummoningCatalog.GenerateVariants(family);
                for (int tier = 1; tier <= 9; tier++)
                {
                    var parent = new SummonParentKey(family, tier, canonicalParents[familyIndex * 9 + tier - 1]);
                    contract._parents.Add(parent);
                    SummonVariantSpec[] registered = variants.Where(value => value.ParentTier == tier).ToArray();
                    SummonVariantSpec[] published = registered.Where(SummonVisibilityCatalog.IsPublished).ToArray();
                    SummonNativeExpansionSpec[] expansions = SummonNativeExpansionCatalog.For(family, tier).ToArray();
                    foreach (SummonVariantSpec variant in registered)
                    {
                        string symbol = ExpandedSummoningIdentityCatalog.AbilitySymbol(variant);
                        contract.Add(new SummonDeclaredChoice { Guid = identity(symbol), Class = SummonChoiceClass.Placement,
                            Symbol = symbol, Source = PlacementSource, Parent = parent,
                            Published = SummonVisibilityCatalog.IsPublished(variant) });
                    }
                    foreach (SummonNativeExpansionSpec expansion in expansions)
                        contract.Add(new SummonDeclaredChoice { Guid = identity(expansion.Symbol),
                            Class = SummonChoiceClass.NativeExpansion, Symbol = expansion.Symbol,
                            Source = NativeExpansionSource, Parent = parent });
                    string preservationSymbol = tier != 1 ? null : family == SummonFamily.Monster ?
                        ExpandedSummoningIdentityCatalog.NativeMonsterTierOneSymbol :
                        ExpandedSummoningIdentityCatalog.NativeNaturesAllyTierOneSymbol;
                    string preservationGuid = preservationSymbol == null ? null : identity(preservationSymbol);
                    bool preservationInNativeCatalog = false;
                    foreach (SummonNativeOptionSpec native in SummonNativeOptionCatalog.All.Where(value =>
                        value.Family == family && value.Tier == tier))
                    {
                        bool replaced = SummonNativeOptionCatalog.ReplacementCoverageIsComplete(native, published, expansions);
                        bool preservation = string.Equals(native.Guid, preservationGuid, StringComparison.Ordinal);
                        preservationInNativeCatalog |= preservation;
                        contract.Add(new SummonDeclaredChoice { Guid = native.Guid,
                            Class = preservation ? SummonChoiceClass.Preservation : SummonChoiceClass.NativeOption,
                            Symbol = preservation ? preservationSymbol : "native option " + native.Multiplicity +
                                (native.EquivalentCreatureKey == null ? string.Empty : " (" + native.EquivalentCreatureKey + ")"),
                            Source = preservation ? PreservationSource : NativeOptionSource, Parent = parent,
                            Replaced = replaced });
                    }
                    if (preservationGuid != null && !preservationInNativeCatalog)
                        contract.Add(new SummonDeclaredChoice { Guid = preservationGuid, Class = SummonChoiceClass.Preservation,
                            Symbol = preservationSymbol, Source = PreservationSource, Parent = parent });
                    foreach (SpiderPlacement placement in SpiderSummonerCatalog.Placements(family).Where(value =>
                        value.Tier == tier))
                    {
                        if (placement.IsOriginal)
                            contract.Add(new SummonDeclaredChoice {
                                Guid = SpiderSummonerCatalog.RowIdentity(family, placement.Species),
                                Class = SummonChoiceClass.ProviderOriginal, Symbol = placement.Species.RowName(family),
                                Source = ProviderSource, Parent = parent, SpeciesKey = placement.Species.Key });
                        else
                            contract.Add(new SummonDeclaredChoice { Guid = identity(placement.Symbol),
                                Class = SummonChoiceClass.SpiderGenerated, Symbol = placement.Symbol, Source = SpiderSource,
                                Parent = parent, SpeciesKey = placement.Species.Key });
                    }
                }
            }
            return contract;
        }

        private void Add(SummonDeclaredChoice choice)
        {
            if (_byGuid.ContainsKey(choice.Guid))
                throw new InvalidOperationException("Two publication declarations share the identity " + choice.Guid + ".");
            _byGuid.Add(choice.Guid, choice);
        }

        /// <summary>Whether the declared choice must be offered under its parent in this state.</summary>
        internal bool Requires(SummonDeclaredChoice choice, SummonPublicationState state)
        {
            if (!state.Requested) return false;
            switch (choice.Class)
            {
                case SummonChoiceClass.Placement: return choice.Published;
                case SummonChoiceClass.NativeExpansion: return true;
                case SummonChoiceClass.Preservation: return !choice.Replaced;
                case SummonChoiceClass.SpiderGenerated: return state.ProvidesSpiderRow(choice.Parent.Family, choice.SpeciesKey);
                default: return false;
            }
        }

        /// <summary>Why an owned choice that is not required may not be offered; null when it may.</summary>
        internal string Refusal(SummonDeclaredChoice choice, SummonPublicationState state)
        {
            switch (choice.Class)
            {
                case SummonChoiceClass.Placement:
                case SummonChoiceClass.NativeExpansion:
                case SummonChoiceClass.Preservation:
                case SummonChoiceClass.SpiderGenerated:
                    if (!state.Requested) return "a project choice is offered while the module is requested off";
                    if (choice.Class == SummonChoiceClass.Placement && !choice.Published)
                        return "a registered placement the visibility catalog withholds is offered";
                    if (choice.Class == SummonChoiceClass.Preservation && choice.Replaced)
                        return "the direct-summon preservation child is offered although its replacement coverage is complete";
                    if (choice.Class == SummonChoiceClass.SpiderGenerated && !state.SpiderFeaturePresent)
                        return "a generated spider row is offered without the Spider Summoner feature in the library";
                    if (choice.Class == SummonChoiceClass.SpiderGenerated)
                        return "a generated spider row is offered without its Races Unleashed source row in the library";
                    return null;
                case SummonChoiceClass.NativeOption:
                    return state.Requested && choice.Replaced ?
                        "a native option the publisher replaces (complete coverage) is still offered" : null;
                default: return null;
            }
        }
    }

    /// <summary>One position under a parent: the identity there, or why none could be read.</summary>
    internal sealed class SummonChoiceReading
    {
        internal int Index;
        internal string Guid;
        internal string Name;
        internal string Fault;
    }

    /// <summary>What was read of one parent.</summary>
    internal sealed class SummonParentReading
    {
        internal SummonParentReading(SummonParentKey key) { Key = key; }

        internal SummonParentKey Key { get; private set; }
        internal string Name;
        /// <summary>Why the parent, or its list of choices, could not be read; null when it was.</summary>
        internal string Fault;
        /// <summary>The parent carries the module's own published variants component.</summary>
        internal bool Published;
        internal readonly List<SummonChoiceReading> Choices = new List<SummonChoiceReading>();

        /// <summary>
        /// Reads one parent position by position. A position that cannot be
        /// read is kept with its reason and the positions around it are
        /// still read; a parent that cannot be read is reported as such and
        /// does not stop the other parents.
        /// </summary>
        /// <param name="lists">The choice list of each variants component the parent carries: none for a direct summon, one otherwise.</param>
        internal static SummonParentReading Read<TChoice>(SummonParentKey key, Func<string> parentName,
            Func<bool> published, Func<IList<IList<TChoice>>> lists, Func<TChoice, string> guidOf,
            Func<TChoice, string> nameOf) where TChoice : class
        {
            var reading = new SummonParentReading(key);
            IList<IList<TChoice>> components;
            try
            {
                reading.Name = parentName();
                reading.Published = published();
                components = lists();
            }
            catch (Exception exception)
            {
                reading.Fault = "parent unavailable: " + Reason(exception);
                return reading;
            }

            if (components == null) components = new IList<TChoice>[0];
            if (components.Count > 1)
                reading.Fault = components.Count + " variants components; at most one is expected";
            int index = 0;
            foreach (IList<TChoice> list in components)
            {
                if (list == null) continue;
                int count;
                try { count = list.Count; }
                catch (Exception exception)
                {
                    reading.Fault = (reading.Fault == null ? string.Empty : reading.Fault + "; ") +
                        "choice list unavailable: " + Reason(exception);
                    continue;
                }
                for (int position = 0; position < count; position++, index++)
                {
                    var choice = new SummonChoiceReading { Index = index };
                    reading.Choices.Add(choice);
                    TChoice value;
                    try { value = list[position]; }
                    catch (Exception exception) { choice.Fault = "entry unavailable: " + Reason(exception); continue; }
                    if (value == null) { choice.Fault = "null entry"; continue; }
                    try { choice.Guid = guidOf(value); }
                    catch (Exception exception) { choice.Fault = "identity unavailable: " + Reason(exception); }
                    try { choice.Name = nameOf(value); }
                    catch (Exception exception) { choice.Name = "<name unavailable: " + Reason(exception) + ">"; }
                    if (choice.Fault == null && string.IsNullOrWhiteSpace(choice.Guid)) choice.Fault = "entry has no identity";
                }
            }
            return reading;
        }

        private static string Reason(Exception exception)
        {
            return exception.GetType().Name + (string.IsNullOrEmpty(exception.Message) ? string.Empty : ": " + exception.Message);
        }
    }

    /// <summary>One line of the ledger: one choice under one parent, or one required choice that is absent.</summary>
    internal sealed class SummonLedgerEntry
    {
        internal SummonParentKey Parent;
        internal string ParentName;
        /// <summary>The position under the parent; -1 for a choice that is absent, or for the parent itself.</summary>
        internal int Index = -1;
        internal string Guid;
        internal string Name;
        /// <summary>How many times this identity occurs under this parent.</summary>
        internal int Occurrences;
        internal bool Requested;
        internal bool Active;
        internal SummonChoiceClass Class;
        /// <summary>The declared symbol or catalog name, when the identity is declared.</summary>
        internal string Symbol;
        /// <summary>The contract that establishes what is expected of this identity.</summary>
        internal string Source;
        internal SummonChoiceVerdict Verdict;
        internal string Reason;

        internal bool Fails
        {
            get
            {
                return Verdict == SummonChoiceVerdict.Unexpected || Verdict == SummonChoiceVerdict.Duplicate ||
                    Verdict == SummonChoiceVerdict.Missing || Verdict == SummonChoiceVerdict.Unreadable;
            }
        }

        internal string Describe()
        {
            return Parent.Label + (Index < 0 ? string.Empty : "[" + Index.ToString(CultureInfo.InvariantCulture) + "]") + " " +
                (Guid ?? "<no identity>") + " " + (Symbol ?? Name ?? "<unnamed>") + ": " + Verdict + " " + Class +
                (Occurrences > 1 ? " x" + Occurrences.ToString(CultureInfo.InvariantCulture) : string.Empty) +
                (Reason == null ? string.Empty : " (" + Reason + ")");
        }

        internal JObject ToJson(bool full)
        {
            var value = new JObject { ["index"] = Index, ["guid"] = Guid, ["name"] = Name, ["class"] = Class.ToString(),
                ["verdict"] = Verdict.ToString() };
            if (Occurrences != 1) value["occurrences"] = Occurrences;
            if (Symbol != null) value["declared"] = Symbol;
            if (Reason != null) value["reason"] = Reason;
            if (!full) return value;
            value["parentGuid"] = Parent.Guid;
            value["parentName"] = ParentName;
            value["family"] = Parent.Family.ToString();
            value["tier"] = Parent.Tier;
            value["requestedModuleState"] = Requested ? "enabled" : "disabled";
            value["activeModuleState"] = Active ? "enabled" : "disabled";
            value["source"] = Source;
            return value;
        }
    }

    /// <summary>One named result of the audit, ready to become an assertion.</summary>
    internal sealed class SummonAuditOutcome
    {
        internal SummonAuditOutcome(string expected, string observed, bool passed)
        { Expected = expected; Observed = observed; Passed = passed; }

        internal string Expected { get; private set; }
        internal string Observed { get; private set; }
        internal bool Passed { get; private set; }
    }

    /// <summary>
    /// Every choice under the eighteen parents, judged one by one against
    /// the publication contract, with the totals beside them. It speaks
    /// only of choices offered under a parent: not of whether a blueprint
    /// is registered, and not of whether a choice can be cast.
    /// </summary>
    internal sealed class ExpandedSummoningPublicationAudit
    {
        internal const string LedgerFile = "expanded-summoning-publication-ledger.json";
        private const int ListedInAssertion = 6;

        private readonly List<SummonLedgerEntry> _entries = new List<SummonLedgerEntry>();
        private readonly List<SummonParentReading> _readings = new List<SummonParentReading>();
        private ExpandedSummoningPublicationContract _contract;
        private bool? _publisherNameCensusExact;

        internal SummonPublicationState State { get; private set; }
        internal IList<SummonLedgerEntry> Entries { get { return _entries.AsReadOnly(); } }
        internal IEnumerable<SummonLedgerEntry> Mismatches { get { return _entries.Where(value => value.Fails); } }
        internal IEnumerable<SummonLedgerEntry> NotAttributed
        { get { return _entries.Where(value => value.Verdict == SummonChoiceVerdict.NotAttributed); } }

        internal int PublishedParents { get; private set; }
        internal int Placements { get { return Count(SummonChoiceClass.Placement); } }
        internal int NativeExpansions { get { return Count(SummonChoiceClass.NativeExpansion); } }
        internal int Preservation { get { return Count(SummonChoiceClass.Preservation); } }
        internal int SpiderRows { get { return Count(SummonChoiceClass.SpiderGenerated); } }
        /// <summary>Choices in the project's namespace that no publication catalog declares.</summary>
        internal int Unclassified
        { get { return Count(SummonChoiceClass.OwnedOther) + Count(SummonChoiceClass.ProjectNamed); } }
        /// <summary>Choices that are not the project's: the game's, a known provider's, or unattributed.</summary>
        internal int NativeVariants
        {
            get
            {
                return Count(SummonChoiceClass.NativeOption) + Count(SummonChoiceClass.ProviderOriginal) +
                    Count(SummonChoiceClass.Foreign);
            }
        }
        internal int Unreadable { get { return _entries.Count(value => value.Verdict == SummonChoiceVerdict.Unreadable); } }
        internal bool PlacementsExact { get; private set; }

        private int Count(SummonChoiceClass choiceClass)
        { return _entries.Count(value => value.Index >= 0 && value.Class == choiceClass); }

        /// <param name="publisherNameCensusExact">The publisher's own name-based placement census, when it was taken.</param>
        internal static ExpandedSummoningPublicationAudit Evaluate(ExpandedSummoningPublicationContract contract,
            SummonPublicationState state, IEnumerable<SummonParentReading> readings, bool? publisherNameCensusExact)
        {
            if (contract == null) throw new ArgumentNullException("contract");
            if (state == null) throw new ArgumentNullException("state");
            if (readings == null) throw new ArgumentNullException("readings");
            var audit = new ExpandedSummoningPublicationAudit { State = state, _contract = contract,
                _publisherNameCensusExact = publisherNameCensusExact };
            SummonParentReading[] all = readings.ToArray();
            bool placementsExact = true;
            foreach (SummonParentKey parent in contract.Parents)
            {
                SummonParentReading reading = all.FirstOrDefault(value => value != null && value.Key != null &&
                    string.Equals(value.Key.Guid, parent.Guid, StringComparison.Ordinal));
                if (reading == null)
                    reading = new SummonParentReading(parent) { Fault = "parent was not read" };
                audit._readings.Add(reading);
                if (reading.Published) audit.PublishedParents++;
                if (reading.Fault != null)
                {
                    placementsExact = false;
                    audit._entries.Add(audit.Entry(reading, new SummonLedgerEntry { Class = SummonChoiceClass.Unreadable,
                        Verdict = SummonChoiceVerdict.Unreadable, Guid = parent.Guid, Reason = reading.Fault +
                            "; the choices required under this parent could not be checked" }));
                }
                var occurrences = new Dictionary<string, int>(StringComparer.Ordinal);
                foreach (SummonChoiceReading choice in reading.Choices.Where(value => value.Fault == null))
                {
                    int seen;
                    occurrences[choice.Guid] = occurrences.TryGetValue(choice.Guid, out seen) ? seen + 1 : 1;
                }
                var met = new HashSet<string>(StringComparer.Ordinal);
                foreach (SummonChoiceReading choice in reading.Choices)
                {
                    SummonLedgerEntry entry = audit.Entry(reading, new SummonLedgerEntry { Index = choice.Index,
                        Guid = choice.Guid, Name = choice.Name });
                    audit._entries.Add(entry);
                    if (choice.Fault != null)
                    {
                        entry.Class = SummonChoiceClass.Unreadable;
                        entry.Verdict = SummonChoiceVerdict.Unreadable;
                        entry.Reason = choice.Fault;
                        placementsExact = false;
                        continue;
                    }
                    entry.Occurrences = occurrences[choice.Guid];
                    audit.Classify(entry, reading, !met.Add(choice.Guid));
                    if (entry.Class == SummonChoiceClass.Placement && entry.Fails) placementsExact = false;
                }
                if (reading.Fault != null) continue;
                foreach (SummonDeclaredChoice required in contract.Declared.Where(value =>
                    value.Parent.Is(parent) && contract.Requires(value, state) && !met.Contains(value.Guid)))
                {
                    if (required.Class == SummonChoiceClass.Placement) placementsExact = false;
                    audit._entries.Add(audit.Entry(reading, new SummonLedgerEntry { Guid = required.Guid,
                        Class = required.Class, Symbol = required.Symbol, Source = required.Source,
                        Verdict = SummonChoiceVerdict.Missing, Reason = MissingReason(required, state) }));
                }
            }
            audit.PlacementsExact = placementsExact && (publisherNameCensusExact ?? true);
            return audit;
        }

        private static string MissingReason(SummonDeclaredChoice required, SummonPublicationState state)
        {
            string reason = "required under " + required.Parent.Label + " and not offered there";
            return required.Class != SummonChoiceClass.SpiderGenerated ? reason :
                reason + "; its Races Unleashed source row is in the library; the integration reported: " +
                    (state.SpiderIntegrationOutcome ?? "<nothing>");
        }

        private SummonLedgerEntry Entry(SummonParentReading reading, SummonLedgerEntry entry)
        {
            entry.Parent = reading.Key;
            entry.ParentName = reading.Name;
            entry.Requested = State.Requested;
            entry.Active = State.Active;
            return entry;
        }

        private void Classify(SummonLedgerEntry entry, SummonParentReading reading, bool repeated)
        {
            SummonDeclaredChoice declared = _contract.Find(entry.Guid);
            if (declared == null)
            {
                string owned = _contract.OwnedSymbol(entry.Guid);
                if (owned != null)
                {
                    entry.Class = SummonChoiceClass.OwnedOther;
                    entry.Symbol = owned;
                    entry.Source = "identity manifest";
                    entry.Verdict = SummonChoiceVerdict.Unexpected;
                    entry.Reason = "a project identity that no publication catalog places under a summon parent";
                }
                else if (entry.Name != null && entry.Name.StartsWith(
                    ExpandedSummoningPublicationContract.ProjectNamePrefix, StringComparison.Ordinal))
                {
                    entry.Class = SummonChoiceClass.ProjectNamed;
                    entry.Source = "identity manifest";
                    entry.Verdict = SummonChoiceVerdict.Unexpected;
                    entry.Reason = "named as the project's, but its identity is not in the manifest";
                }
                else
                {
                    entry.Class = SummonChoiceClass.Foreign;
                    entry.Source = ExpandedSummoningPublicationContract.MergeSource;
                    entry.Verdict = SummonChoiceVerdict.NotAttributed;
                    entry.Reason = "declared by no catalog of this project; kept as a choice publication does not own; " +
                        "its owner was not established";
                }
            }
            else
            {
                entry.Class = declared.Class;
                entry.Symbol = declared.Symbol;
                entry.Source = declared.Source;
                bool here = declared.Parent.Is(reading.Key);
                if (!here && declared.Owned)
                {
                    entry.Verdict = SummonChoiceVerdict.Unexpected;
                    entry.Reason = "declared for " + declared.Parent.Label + ", offered under " + reading.Key.Label;
                }
                else if (!here)
                {
                    entry.Verdict = SummonChoiceVerdict.NotAttributed;
                    entry.Reason = "declared for " + declared.Parent.Label + ", offered under " + reading.Key.Label +
                        "; not the project's, and nothing declares it here";
                }
                else if (_contract.Requires(declared, State))
                {
                    entry.Verdict = SummonChoiceVerdict.Required;
                }
                else
                {
                    string refusal = _contract.Refusal(declared, State);
                    entry.Verdict = refusal == null ? SummonChoiceVerdict.Preserved : SummonChoiceVerdict.Unexpected;
                    entry.Reason = refusal ?? (declared.Class == SummonChoiceClass.ProviderOriginal ?
                        "Races Unleashed's own row at its base tier" : "the game's own option, not replaced in this state");
                }
            }

            if (!repeated || entry.Fails) return;
            // Publication refuses a list holding one identity twice, so in a
            // published list a repeat is a fault whoever owns the choice. In
            // a list publication has not touched, a repeat of someone else's
            // choice is theirs to explain.
            bool ownedChoice = declared != null && declared.Owned;
            if (ownedChoice || reading.Published)
            {
                entry.Verdict = SummonChoiceVerdict.Duplicate;
                entry.Reason = "occurs " + entry.Occurrences.ToString(CultureInfo.InvariantCulture) +
                    " times under this parent; a published list holds each identity once";
            }
            else
            {
                entry.Reason = (entry.Reason == null ? string.Empty : entry.Reason + "; ") + "occurs " +
                    entry.Occurrences.ToString(CultureInfo.InvariantCulture) + " times in a list publication has not touched";
            }
        }

        // ---- the totals, kept in their established form

        internal string Describe()
        {
            return Totals(PublishedParents, Placements, NativeExpansions, Preservation, Unclassified, PlacementsExact,
                NativeVariants, SpiderRows, Unreadable);
        }

        /// <summary>
        /// The totals the contract requires in a state. Every number comes
        /// from the catalogs and from what the library holds of the spider
        /// provider, never from the choices that were read. The count of
        /// choices that are not the project's is carried through and not
        /// constrained: it belongs to the game and the other mods installed.
        /// </summary>
        internal static string Expectation(ExpandedSummoningPublicationContract contract, SummonPublicationState state,
            int nativeVariants)
        {
            Func<SummonChoiceClass, int> required = choiceClass => contract.Declared.Count(value =>
                value.Class == choiceClass && contract.Requires(value, state));
            return Totals(state.Requested ? contract.Parents.Count : 0, required(SummonChoiceClass.Placement),
                required(SummonChoiceClass.NativeExpansion), required(SummonChoiceClass.Preservation), 0, true,
                nativeVariants, required(SummonChoiceClass.SpiderGenerated), 0);
        }

        private static string Totals(int publishedParents, int placements, int nativeExpansions, int preservation,
            int unclassified, bool placementsExact, int nativeVariants, int spiderRows, int unreadable)
        {
            // The first seven keys are the totals as they have always been
            // written, in the same order; the two after them are new.
            return string.Format(CultureInfo.InvariantCulture,
                "publishedParents={0};placements={1};nativeOptions={2};preservation={3};unclassified={4};" +
                "placementsExact={5};nativeVariants={6};spiderRows={7};unreadable={8}",
                publishedParents, placements, nativeExpansions, preservation, unclassified, placementsExact,
                nativeVariants, spiderRows, unreadable);
        }

        // ---- the individual results

        /// <summary>Every required choice is offered under its parent.</summary>
        internal SummonAuditOutcome RequiredChoices()
        {
            int required = _contract.Declared.Count(value => _contract.Requires(value, State));
            SummonLedgerEntry[] missing = _entries.Where(value => value.Verdict == SummonChoiceVerdict.Missing).ToArray();
            int unchecked_ = _readings.Where(value => value.Fault != null).Sum(reading => _contract.Declared.Count(value =>
                value.Parent.Is(reading.Key) && _contract.Requires(value, State)));
            return new SummonAuditOutcome("each of the " + required.ToString(CultureInfo.InvariantCulture) +
                " choices the contract requires in this state is offered under its declared parent",
                "required=" + required.ToString(CultureInfo.InvariantCulture) + ";missing=" +
                    missing.Length.ToString(CultureInfo.InvariantCulture) + ";notChecked=" +
                    unchecked_.ToString(CultureInfo.InvariantCulture) + List(missing),
                missing.Length == 0 && unchecked_ == 0);
        }

        /// <summary>No choice is offered where the declared boundary does not allow it.</summary>
        internal SummonAuditOutcome Boundary()
        {
            SummonLedgerEntry[] unexpected = _entries.Where(value => value.Verdict == SummonChoiceVerdict.Unexpected).ToArray();
            return new SummonAuditOutcome("no project choice outside its declared parent and module state, none without a " +
                "declared identity, and no native option the publisher replaces",
                "unexpected=" + unexpected.Length.ToString(CultureInfo.InvariantCulture) + List(unexpected),
                unexpected.Length == 0);
        }

        /// <summary>No identity is offered twice where publication holds each once.</summary>
        internal SummonAuditOutcome Duplicates()
        {
            SummonLedgerEntry[] duplicates = _entries.Where(value => value.Verdict == SummonChoiceVerdict.Duplicate).ToArray();
            return new SummonAuditOutcome("no identity offered twice under one parent",
                "duplicates=" + duplicates.Length.ToString(CultureInfo.InvariantCulture) + List(duplicates),
                duplicates.Length == 0);
        }

        /// <summary>Every parent and every position under it could be read.</summary>
        internal SummonAuditOutcome Readable()
        {
            SummonLedgerEntry[] unreadable = _entries.Where(value => value.Verdict == SummonChoiceVerdict.Unreadable).ToArray();
            int parents = unreadable.Count(value => value.Index < 0);
            return new SummonAuditOutcome("all eighteen parents and every position under them readable",
                "unreadableParents=" + parents.ToString(CultureInfo.InvariantCulture) + ";unreadableEntries=" +
                    (unreadable.Length - parents).ToString(CultureInfo.InvariantCulture) + List(unreadable),
                unreadable.Length == 0);
        }

        /// <summary>What is offered that is not the project's, by how it was attributed. Reported, not constrained.</summary>
        internal string DescribeOthers()
        {
            return "nativeOptions=" + Count(SummonChoiceClass.NativeOption).ToString(CultureInfo.InvariantCulture) +
                ";racesUnleashedRows=" + Count(SummonChoiceClass.ProviderOriginal).ToString(CultureInfo.InvariantCulture) +
                ";notAttributed=" + NotAttributed.Count().ToString(CultureInfo.InvariantCulture) + List(NotAttributed.ToArray());
        }

        private static string List(IList<SummonLedgerEntry> entries)
        {
            if (entries.Count == 0) return string.Empty;
            string listed = string.Join(" | ", entries.Take(ListedInAssertion).Select(value => value.Describe()).ToArray());
            return ": " + listed + (entries.Count <= ListedInAssertion ? string.Empty : " | and " +
                (entries.Count - ListedInAssertion).ToString(CultureInfo.InvariantCulture) + " more in " + LedgerFile);
        }

        /// <summary>The whole ledger: every position read, every mismatch in full, and the state it was judged in.</summary>
        internal JObject ToJson()
        {
            var parents = new JArray();
            foreach (SummonParentReading reading in _readings)
            {
                parents.Add(new JObject { ["guid"] = reading.Key.Guid, ["name"] = reading.Name,
                    ["family"] = reading.Key.Family.ToString(), ["tier"] = reading.Key.Tier,
                    ["label"] = reading.Key.Label, ["carriesPublishedComponent"] = reading.Published,
                    ["fault"] = reading.Fault,
                    ["choices"] = new JArray(_entries.Where(value => value.Parent.Is(reading.Key) &&
                        value.Index >= 0).Select(value => (object)value.ToJson(false)).ToArray()) });
            }
            return new JObject {
                ["schemaVersion"] = 1,
                ["claim"] = "choices offered under the eighteen canonical summon parents; not registration, not casting",
                ["state"] = new JObject { ["requestedModuleState"] = State.Requested ? "enabled" : "disabled",
                    ["activeModuleState"] = State.Active ? "enabled" : "disabled",
                    ["spiderSummonerFeaturePresent"] = State.SpiderFeaturePresent,
                    ["racesUnleashedRows"] = new JArray(State.SpiderProviderRows.OrderBy(value => value,
                        StringComparer.Ordinal).Cast<object>().ToArray()),
                    ["spiderIntegrationOutcome"] = State.SpiderIntegrationOutcome },
                ["totals"] = new JObject { ["observed"] = Describe(),
                    ["expected"] = Expectation(_contract, State, NativeVariants),
                    ["publisherNameCensusExact"] = _publisherNameCensusExact,
                    ["others"] = DescribeOthers() },
                ["mismatches"] = new JArray(Mismatches.Select(value => (object)value.ToJson(true)).ToArray()),
                ["notAttributed"] = new JArray(NotAttributed.Select(value => (object)value.ToJson(true)).ToArray()),
                ["parents"] = parents };
        }
    }

    /// <summary>Reads every identity and its symbol from the identity manifest's text.</summary>
    internal static class SummonManifestIdentities
    {
        internal static Dictionary<string, string> Parse(string manifestJson)
        {
            if (manifestJson == null) throw new ArgumentNullException("manifestJson");
            JArray entries = JObject.Parse(manifestJson)["entries"] as JArray;
            if (entries == null) throw new InvalidOperationException("The identity manifest holds no entries.");
            var result = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (JToken entry in entries)
            {
                string guid = (string)entry["guid"], symbol = (string)entry["symbol"];
                if (string.IsNullOrWhiteSpace(guid) || string.IsNullOrWhiteSpace(symbol))
                    throw new InvalidOperationException("The identity manifest holds an entry without an identity or symbol.");
                result.Add(guid, symbol);
            }
            return result;
        }
    }
}
