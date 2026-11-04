using System;
using System.Collections.Generic;
using System.Linq;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.RuntimeTesting
{
    // Request-local cleanup evidence only; no Unity mutation or ownership by name.
    internal sealed class RosterResourceReport
    {
        internal const bool DiagnosisOnly = false;

        internal static bool CleanupSatisfied(JObject row)
        {
            string kind = (string)row["ownershipClass"];
            if (kind == "PRIVATE_VIEW_OWNED") return !(bool)row["aliveAfter"] &&
                !((JArray)row["references"]).Any(r => (bool?)r["retainedAsOwnedAfterDestruction"] == true);
            if (kind != "IMMUTABLE_PROCESS_CACHE" && kind != "BORROWED_NATIVE") return false;
            if (!(bool)row["aliveAfter"] || (int)row["afterCleanupGlobalCountSameTypeName"] !=
                (int)row["beforeCleanupGlobalCountSameTypeName"]) return false;
            // Cache is initialized before the working load. Native borrowed
            // assets may be loaded with the area; require stable cleanup count.
            return kind != "IMMUTABLE_PROCESS_CACHE" ||
                (int?)row["preLoadGlobalCountSameTypeName"] == (int)row["afterCleanupGlobalCountSameTypeName"];
        }
        internal readonly JObject Result;
        internal readonly JArray Rows = new JArray(), Errors = new JArray();
        internal RosterResourceReport(string request, string stage)
        {
            Result = new JObject { ["schemaVersion"] = 1, ["requestId"] = request,
                ["stage"] = stage, ["readOnlyObserver"] = true, ["diagnosisOnly"] = DiagnosisOnly,
                ["resources"] = Rows, ["observerErrors"] = Errors, ["clearedInFinally"] = false };
        }
        internal static string Classify(bool owned, bool cache, bool borrowed)
        {
            if (owned && (cache || borrowed)) return "CONFLICT";
            return owned ? "PRIVATE_VIEW_OWNED" : cache ? "IMMUTABLE_PROCESS_CACHE" :
                borrowed ? "BORROWED_NATIVE" : "UNRESOLVED";
        }
        internal void Error(string key, string unitId, string stage, Exception error)
        { Errors.Add(new JObject { ["key"] = key, ["unitId"] = unitId, ["stage"] = stage,
            ["exceptionType"] = error.GetType().FullName, ["message"] = error.Message }); }
        internal void Add(string key, string unitId, Func<JObject> build)
        {
            try { Rows.Add(build() ?? throw new InvalidOperationException("Null resource receipt.")); }
            catch (Exception error) { Error(key, unitId, "resource-row", error); }
        }
        internal void Finish(Action clear)
        {
            try
            {
                Result["uniqueObjects"] = Rows.Count;
                Result["sharedObjects"] = Rows.Count(r => (int)r["fixtureUnitReferences"] > 1);
                Result["expectedPrivateObjects"] = Rows.Count(r => (string)r["ownershipClass"] == "PRIVATE_VIEW_OWNED");
                Result["expectedCacheOrBorrowedObjects"] = Rows.Count(r => (string)r["ownershipClass"] == "IMMUTABLE_PROCESS_CACHE" || (string)r["ownershipClass"] == "BORROWED_NATIVE");
                Result["survivors"] = new JArray(Rows.Where(r => (bool)r["aliveAfter"]).Select(r => r.DeepClone()));
                Result["survivorGroups"] = new JArray(Rows.Where(r => (bool)r["aliveAfter"])
                    .GroupBy(r => (string)r["type"] + "|" + (string)r["name"] + "|" + (string)r["ownershipClass"])
                    .OrderBy(g => g.Key, StringComparer.Ordinal).Select(g => new JObject {
                        ["typeNameClass"] = g.Key, ["instanceIds"] = new JArray(g.Select(r => (int)r["instanceId"])),
                        ["ownersAndSources"] = new JArray(g.SelectMany(r => (JArray)r["references"]).Select(r => r.DeepClone())) }));
            }
            catch (Exception error) { Error(null, null, "resource-summary", error); }
            finally
            {
                try { clear(); Result["clearedInFinally"] = true; }
                catch (Exception error) { Error(null, null, "resource-clear", error); }
            }
            Result["observerErrorCount"] = Errors.Count;
        }
    }

    // JSON for this request-local fixture only. Never invoke the game's
    // configured serializer for dictionaries or engine-owned objects.
    internal sealed class RosterReadinessReport
    {
        private readonly JArray _rows = new JArray(), _errors = new JArray();
        internal readonly JObject Result;
        internal bool HasErrors { get { return _errors.Count != 0; } }
        internal int ErrorCount { get { return _errors.Count; } }

        internal RosterReadinessReport(string request, string scenario, int expectedUnits)
        {
            Result = new JObject { ["schemaVersion"] = 2, ["requestId"] = request,
                ["scenario"] = scenario, ["unitCount"] = expectedUnits, ["observerReadOnly"] = true,
                ["readinessContract"] = "Whole-roster native appearance/control settlement, not simultaneous camera/fog/invisibility presentation; original visual predicate evidence retained",
                ["units"] = _rows, ["observerErrors"] = _errors,
                ["summaryByFailedPredicate"] = new JObject(), ["failingUnits"] = new JArray(),
                ["spawnFrameSource"] = "Native cast/entity creation returned and receipt assigned;loaded units have no invented spawn frame",
                ["observerClearedInFinally"] = false };
        }

        internal static JObject NullableFrameMap(IEnumerable<KeyValuePair<string, int?>> values)
        {
            var result = new JObject();
            foreach (var pair in values.OrderBy(value => value.Key, StringComparer.Ordinal))
                result[pair.Key] = pair.Value.HasValue ? new JValue(pair.Value.Value) : JValue.CreateNull();
            return result;
        }

        internal void AddError(string key, string unitId, string stage, Exception error)
        {
            _errors.Add(new JObject { ["key"] = key, ["unitId"] = unitId,
                ["exceptionType"] = error.GetType().FullName, ["message"] = error.Message,
                ["observerStage"] = stage });
        }

        internal void AddUnit(string key, string unitId, Func<JObject> buildRow)
        {
            try
            {
                var row = buildRow();
                if (row == null || (string)row["key"] != key || (string)row["unitId"] != unitId ||
                    !(row["failedPredicateNames"] is JArray) || !(row["failedObservedPredicateNames"] is JArray))
                    throw new InvalidOperationException("Complete matching per-unit readiness row required.");
                _rows.Add(row);
            }
            catch (Exception error) { AddError(key, unitId, "per-unit-final-state-row", error); }
        }

        internal void Finish(Action clearTracking)
        {
            try
            {
                var grouped = new SortedDictionary<string, List<string>>(StringComparer.Ordinal);
                var failing = new JArray();
                foreach (var row in _rows.Cast<JObject>())
                {
                    var failed = (JArray)row["failedObservedPredicateNames"];
                    if (failed.Count == 0) continue;
                    foreach (var predicate in failed.Values<string>())
                    {
                        List<string> keys;
                        if (!grouped.TryGetValue(predicate, out keys)) grouped.Add(predicate, keys = new List<string>());
                        keys.Add((string)row["key"]);
                    }
                    failing.Add(new JObject { ["key"] = (string)row["key"], ["unitId"] = (string)row["unitId"],
                        ["failedPredicateNames"] = row["failedPredicateNames"].DeepClone(),
                        ["failedObservedPredicateNames"] = failed.DeepClone(),
                        ["firstFrameAllSnakePredicates"] = row["firstFrameAllSnakePredicates"]?.DeepClone() });
                }
                var summary = new JObject();
                foreach (var pair in grouped)
                {
                    var keys = new JArray();
                    foreach (string key in pair.Value.OrderBy(value => value, StringComparer.Ordinal)) keys.Add(new JValue(key));
                    summary[pair.Key] = new JObject { ["count"] = pair.Value.Count, ["keys"] = keys };
                }
                Result["summaryByFailedPredicate"] = summary;
                Result["failingUnits"] = failing;
            }
            catch (Exception error) { AddError(null, null, "readiness-summary", error); }
            finally
            {
                try { clearTracking(); Result["observerClearedInFinally"] = true; }
                catch (Exception error) { AddError(null, null, "clear-request-local-timelines", error); }
            }
            Result["successfulUnitRows"] = _rows.Count;
            Result["observerErrorCount"] = _errors.Count;
        }
    }

    // Bounded request-local observation, never a source of unit state.
    internal sealed class RosterReadinessTimeline
    {
        internal static readonly string[] SnakePredicates = {
            "EntityFadedIn", "DissolveSettled", "CanAct", "CanMove", "AppearanceBuffAbsent"
        };
        // The native two-second appearance lock is a control/save boundary.
        // EntityFader also dissolves for fog-of-war; natural invisibility and
        // dormant offscreen views need not become visually intact to be saved.
        // Retain those visual measurements above, but do not reuse that visual
        // scenario's simultaneous-visibility contract for 108 saved units.
        internal static readonly string[] PersistencePredicates = {
            "ViewExists", "ViewDataMatches", "ViewIsInGame", "DissolveFinite",
            "CanAct", "CanMove", "AppearanceBuffAbsent"
        };
        private readonly Dictionary<string, int> _firstTrue = new Dictionary<string, int>(StringComparer.Ordinal);
        private Dictionary<string, bool> _last = new Dictionary<string, bool>(StringComparer.Ordinal);
        internal readonly int? SpawnedFrame;
        internal int? FirstAllSnakeFrame, FirstAllObservedFrame, FirstPersistenceReadyFrame;
        internal int SampleCount;
        internal RosterReadinessTimeline(int? spawnedFrame) { SpawnedFrame = spawnedFrame; }
        internal IDictionary<string, int> FirstTrue { get { return new Dictionary<string, int>(_firstTrue); } }
        internal IDictionary<string, int?> FirstTrueIncludingNever
        { get { return _last.Keys.ToDictionary(p => p, p => _firstTrue.ContainsKey(p) ? (int?)_firstTrue[p] : null, StringComparer.Ordinal); } }
        internal void Observe(int frame, IDictionary<string, bool> predicates)
        {
            if (predicates == null || SnakePredicates.Any(p => !predicates.ContainsKey(p)))
                throw new ArgumentException("Complete exact native predicate sample required.");
            if (SpawnedFrame.HasValue && frame < SpawnedFrame.Value) throw new ArgumentOutOfRangeException("frame");
            foreach (var p in predicates) if (p.Value && !_firstTrue.ContainsKey(p.Key)) _firstTrue.Add(p.Key, frame);
            if (!FirstAllSnakeFrame.HasValue && SnakePredicates.All(p => predicates[p])) FirstAllSnakeFrame = frame;
            if (!FirstAllObservedFrame.HasValue && predicates.All(p => p.Value)) FirstAllObservedFrame = frame;
            if (!FirstPersistenceReadyFrame.HasValue && PersistenceReady(predicates)) FirstPersistenceReadyFrame = frame;
            _last = new Dictionary<string, bool>(predicates, StringComparer.Ordinal); SampleCount++;
        }
        internal static bool PersistenceReady(IDictionary<string, bool> predicates)
        { return predicates != null && PersistencePredicates.All(p => predicates.ContainsKey(p) && predicates[p]); }
        internal bool ReadyForSave { get { return FirstPersistenceReadyFrame.HasValue && PersistenceReady(_last); } }
        internal string[] FailedSnakePredicates()
        { return SnakePredicates.Where(p => !_last.ContainsKey(p) || !_last[p]).ToArray(); }
        internal string[] FailedObservedPredicates()
        { return _last.Where(p => !p.Value).Select(p => p.Key).OrderBy(p => p, StringComparer.Ordinal).ToArray(); }
        internal void Clear()
        { _firstTrue.Clear(); _last.Clear(); FirstAllSnakeFrame = null; FirstAllObservedFrame = null; FirstPersistenceReadyFrame = null; SampleCount = 0; }
    }

    // Closed extension of the existing prepare/cleanup/absence trio. It is
    // fixture ownership, not production serialization or grapple restoration.
    internal static class ExpandedSummoningRosterPersistencePolicy
    {
        internal const string Scope = "whole-roster";
        internal const string AnkylosaurusScope = "ankylosaurus";
        internal const string ArmoredDinosaursScope = "armored-dinosaurs";
        internal const string TheropodsScope = "theropods";
        internal const string GiantBodiesScope = "giant-bodies";
        internal const string GriffonScope = "griffon";
        internal static string[] GriffonKeys { get { return new[] { "griffon" }; } }
        internal const string GiantHumanoidsScope = "giant-humanoids";
        internal static string[] GiantHumanoidKeys
        { get { return new[] { GiantHumanoidRulesPolicy.Hill,
            GiantHumanoidRulesPolicy.Stone, GiantHumanoidRulesPolicy.Fire }; } }
        internal const string InfernalOutsidersScope = "infernal-outsiders";
        internal static string[] InfernalOutsiderKeys
        { get { return new[] { "bearded-devil", "kyton-chain-devil" }; } }
        internal const string InfernalReachTwoScope = "infernal-reach-ii";
        internal static string[] InfernalReachTwoKeys
        { get { return new[] { "barbed-devil", "bone-devil" }; } }
        internal const string InfernalDemonsScope = "infernal-demons";
        internal static string[] InfernalDemonKeys
        { get { return new[] { InfernalVisualPolicy36.Babau,
            InfernalVisualPolicy36.Hezrou }; } }
        internal const string VrockScope = "vrock";
        // Two, like the Sprint 32 scope. The printed spore growth lands on an
        // adjacent creature and not on the vrock itself, and this fixture may
        // not write to a party member or any unit it does not own, so the only
        // faithful victim is a second owned vrock. It also makes the saved
        // growth's attribution and the spent single uses demonstrably one
        // owner's rather than merely "a vrock's".
        internal static string[] VrockKeys
        { get { return new[] { VrockVisualPolicy37.Vrock,
            VrockVisualPolicy37.Vrock }; } }
        internal const string GlabrezuScope = "glabrezu";
        // Two, for the same reason the Sprint 37 scope takes two. The printed
        // rend lands on an opponent the glabrezu hit with both pincers this
        // round, and the once-per-day power word stun must spend its single
        // use on a unit that is really there; this fixture may not write to a
        // party member or any unit it does not own, so the only faithful
        // victim is a second owned glabrezu. It also makes the spent daily use
        // demonstrably ONE owner's rather than merely "a glabrezu's", which is
        // the fact the save has to carry.
        //
        // The rend round state is deliberately NOT saved state: it lives in a
        // request-local ConditionalWeakTable keyed by unit and is rebuilt from
        // the round boundary, so the envelope's job is to show that no armed
        // rend survived the load, not to restore one.
        internal static string[] GlabrezuKeys
        { get { return new[] { GlabrezuVisualPolicy38.Glabrezu,
            GlabrezuVisualPolicy38.Glabrezu }; } }
        internal const string NalfeshneeScope = "nalfeshnee";
        // Two owners, for a reason specific to this creature rather than
        // inherited. Unholy Nimbus is the state the envelope exists to carry:
        // a windup pending at the save boundary, with its absolute deadline,
        // and a spent use that must be demonstrably ONE owner's rather than
        // merely "a nalfeshnee's". Two independent owners are what separate
        // those, and they are also what shows a second owner's windup is not
        // reset, refunded or detonated by the first one resolving.
        //
        // The owner-decision limit of one pending nimbus per creature is per
        // CREATURE, so two owners can legitimately hold one windup each; that
        // is the case this scope sets up.
        //
        // Unlike Sprint 38, the pending effect is NOT withdrawn before the
        // save. The Glabrezu envelope had to withdraw its stun because a real
        // stun stops an owned unit acting and the fixture's shared readiness
        // contract requires every owned unit to be able to act at the
        // boundary; a nimbus windup imposes no such restriction on its owner,
        // which may keep acting normally while it is pending. So what crosses
        // this save really is the pending windup itself, and no readiness
        // waiver is needed to get it there.
        internal static string[] NalfeshneeKeys
        { get { return new[] { NalfeshneeVisualPolicy39.Nalfeshnee,
            NalfeshneeVisualPolicy39.Nalfeshnee }; } }
        internal const string ShadowOutsidersScope = "shadow-outsiders";
        internal static string[] ShadowOutsiderKeys
        { get { return new[] { "succubus", "invisible-stalker", "shadow-demon" }; } }
        internal const string FeyCelestialScope = "fey-celestial";
        // Optional finite variants of the existing Working trio. PowerShell
        // admission reads these declarations; the save-free profile stays so.
        internal const string Sprint32SuggestionRetained = "retained";
        internal const string Sprint32SuggestionRemoved = "removed";
        internal const string Sprint32SuggestionModuleOff = "module-off";
        internal static bool Sprint32SuggestionTransition(string value)
        { return value == Sprint32SuggestionRetained || value == Sprint32SuggestionRemoved || value == Sprint32SuggestionModuleOff; }
        // Two of each: identities and receipts remain keyed by actual UnitId.
        internal static string[] FeyCelestialKeys
        { get { return new[] { "satyr", "lillend-azata", "satyr", "lillend-azata" }; } }
        internal const string OutsiderCelestialScope = "outsider-celestial";
        internal static string[] OutsiderCelestialKeys
        { get { return new[] { "lemure", "dretch", "hound-archon", "lantern-archon", "astral-deva", "trumpet-archon" }; } }
        internal const string GiantHumanoidsTwoScope = "giant-humanoids-ii";
        internal static string[] GiantHumanoidsTwoKeys
        { get { return new[] { "ettin", GiantHumanoidRulesPolicy.Cloud,
            GiantHumanoidRulesPolicy.Storm }; } }
        internal const string MagicalBeastsScope = "magical-beasts";
        internal static string[] MagicalBeastKeys { get { return new[] { "gorgon", "bulette" }; } }
        internal static string[] GiantBodyKeys
        { get { return new[] { GiantBodyRulesPolicy.BrachiosaurusKey, GiantBodyRulesPolicy.ElephantKey }; } }
        internal const string ReceiptScope = "KMG_Release143_WholeRoster_v1";
        internal static string[] ArmoredDinosaurKeys
        {
            get { return new[] {
                ArmoredDinosaurRulesPolicy.TriceratopsKey,
                ArmoredDinosaurRulesPolicy.StegosaurusKey };
            }
        }
        internal static string[] TheropodKeys
        {
            get { return new[] {
                TheropodRulesPolicy.DeinonychusKey,
                TheropodRulesPolicy.TyrannosaurusKey };
            }
        }
        // Request-local fixture choices mirror the qualified player-path
        // harness. Select for EACH cast: a native Evil row must not leave the
        // following Celestial execution unavailable. Integers are the native
        // Alignment flags, keeping this policy independently testable.
        internal static int CasterAlignmentFor(SummonVariantSpec variant)
        {
            if (variant.Family == SummonFamily.NaturesAlly) return 17; // ChaoticNeutral
            if (variant.Creature.MonsterTemplated) return 3; // NeutralGood
            switch (variant.Creature.Key)
            {
                case "lemure": return 12; // LawfulEvil
                case "dretch": return 20; // ChaoticEvil
                case "babau":
                case "hezrou": return 20; // ChaoticEvil
                case "hound-archon":
                case "trumpet-archon": return 10; // LawfulGood
                case "astral-deva": return 3; // NeutralGood
                case "lantern-archon":
                case "bralani-azata":
                case "ghaele-azata":
                case "lillend-azata": return 3;
                case "bearded-devil":
                case "kyton-chain-devil":
                case "barbed-devil":
                case "bone-devil":
                case "hell-hound":
                case "erinyes-devil":
                case "shadow-demon":
                case "succubus":
                case "salamander":
                case "bebelith": return 12; // LawfulEvil
                default: return 1; // TrueNeutral
            }
        }
        internal static int CasterAlignmentFor(SummonNativeExpansionSpec native)
        {
            return native.Branch == SummonNativeSpawnBranch.Evil ? 5 : 1;
        }
        /// <summary>
        /// The broad released-roster fixture's coverage list: every project
        /// creature plus the retained native wrappers. It is deliberately the
        /// <b>whole</b> roster and not a published subset, because it is also
        /// the receipt-admission list and a receipt for any roster creature is
        /// legitimate.
        ///
        /// <para>Creating a creature is a separate question from covering it.
        /// <see cref="SelectCreatureVariant"/> refuses a withheld route under
        /// this broad scope on purpose: a creature under qualification is
        /// summoned by its own named scope, not by the released-roster
        /// fixture. So a withheld creature appears here and is refused
        /// there, and <see cref="HasPublishedSingleRoute"/> is what tells the
        /// two apart rather than a hand-maintained list.</para>
        /// </summary>
        internal static string[] Keys
        {
            get { return ExpandedSummoningCatalog.All.Select(c => c.Key)
                .Concat(SummonNativeExpansionCatalog.All.Select(n => "native:" + n.UnitGuid))
                .Distinct(StringComparer.Ordinal).OrderBy(k => k, StringComparer.Ordinal).ToArray(); }
        }

        /// <summary>
        /// Whether this creature has a published quantity-one route, which is
        /// exactly what the broad released-roster scope can summon. Public so
        /// that the scope-coverage test asserts the refusal boundary from the
        /// same predicate the production selection uses, instead of listing
        /// the currently withheld creatures a second time.
        /// </summary>
        internal static bool HasPublishedSingleRoute(string key)
        {
            return SingleRoutes(key).Any(SummonVisibilityCatalog.IsPublished);
        }

        private static IEnumerable<SummonVariantSpec> SingleRoutes(string key)
        {
            return ExpandedSummoningCatalog
                .GenerateVariants(SummonFamily.Monster)
                .Concat(ExpandedSummoningCatalog.GenerateVariants(SummonFamily.NaturesAlly))
                .Where(value => value.Creature.Key == key &&
                    value.Multiplicity == SummonMultiplicity.One);
        }
        /// <summary>
        /// The creatures a persistence scope summons, as one list. There were
        /// four parallel copies of this - the permitted set below, the exact
        /// selected-key check, and the runner's own selection - and the vrock
        /// scope was missing from the runner's, which fell through to the whole
        /// roster and asked this scope for a creature it does not own. A
        /// reflection test now requires every declared scope to round-trip
        /// through this one function.
        /// </summary>
        internal static string[] SelectedKeys(string scope)
        {
            return scope == AnkylosaurusScope ? new[] { "ankylosaurus" } :
                scope == ArmoredDinosaursScope ? ArmoredDinosaurKeys :
                scope == TheropodsScope ? TheropodKeys :
                scope == GiantBodiesScope ? GiantBodyKeys :
                scope == MagicalBeastsScope ? MagicalBeastKeys :
                scope == GriffonScope ? GriffonKeys :
                scope == GiantHumanoidsScope ? GiantHumanoidKeys :
                scope == GiantHumanoidsTwoScope ? GiantHumanoidsTwoKeys :
                scope == OutsiderCelestialScope ? OutsiderCelestialKeys :
                scope == FeyCelestialScope ? FeyCelestialKeys :
                scope == InfernalOutsidersScope ? InfernalOutsiderKeys :
                scope == InfernalReachTwoScope ? InfernalReachTwoKeys :
                scope == InfernalDemonsScope ? InfernalDemonKeys :
                scope == VrockScope ? VrockKeys :
                scope == GlabrezuScope ? GlabrezuKeys :
                scope == NalfeshneeScope ? NalfeshneeKeys :
                scope == ShadowOutsidersScope ? ShadowOutsiderKeys :
                scope == Scope ? Keys : new string[0];
        }

        internal static SummonVariantSpec SelectCreatureVariant(string scope, string key)
        {
            string[] permitted = scope == Scope
                // The broad released-roster fixture selects from the catalog
                // rather than from the save-identity list, as it always has.
                ? ExpandedSummoningCatalog.All.Select(value => value.Key).ToArray()
                : SelectedKeys(scope);
            if (!permitted.Contains(key, StringComparer.Ordinal))
                throw new InvalidOperationException("Persistence scope does not own the requested creature key.");
            IEnumerable<SummonVariantSpec> variants = SingleRoutes(key);
            // A named one-creature qualification scope must be able to create
            // its already-registered identity while its public roots remain
            // deliberately suppressed. The broad released-roster fixture must
            // still refuse unpublished routes.
            if (scope == Scope) variants = variants.Where(SummonVisibilityCatalog.IsPublished);
            SummonVariantSpec[] usable = variants.ToArray();
            // Refuse in words rather than by letting First() report "Sequence
            // contains no elements" from inside a live fixture. The broad
            // scope reaching a withheld creature is a real and expected
            // state - that creature is qualified through its own named scope -
            // and it has to read as a refusal, not as an internal error.
            if (usable.Length == 0)
                throw new InvalidOperationException(
                    "Persistence scope '" + scope + "' has no usable " +
                    "quantity-one route for '" + key + "'" +
                    (scope == Scope && SingleRoutes(key).Any()
                        ? ": its routes are registered but still withheld, so " +
                          "only that creature's own named scope may create it."
                        : "."));
            return usable[0];
        }
        internal static bool ExactSelectedKeys(string scope,
            IEnumerable<string> observed)
        {
            if (observed == null) return false;
            string[] expected = SelectedKeys(scope);
            return expected.Length != 0 && observed.OrderBy(value => value,
                    StringComparer.Ordinal).SequenceEqual(expected.OrderBy(
                    value => value, StringComparer.Ordinal),
                    StringComparer.Ordinal);
        }
        internal static bool ModuleOffArmoredBodyIsClean(string outcome,
            int ownedResourceCount)
        {
            // The view patch deliberately records the asset loader's explicit
            // module-disabled refusal. "not-attempted" would not prove that
            // this registered save-compatible identity reached that gate.
            return outcome == "donor-visual:module-disabled" &&
                ownedResourceCount == 0;
        }
        /// <summary>
        /// The single list of scopes that reach the roster-persistence
        /// observation. The runner used to keep its own parallel disjunction,
        /// and Sprint 37 was admitted by every other gate while missing from
        /// that one - so the vrock scope silently ran the generic persistence
        /// path and reported PASS without observing one Sprint 37 fact. Both
        /// the runner and ObserveLoad now ask this, and a reflection test
        /// requires every declared scope constant to appear here.
        /// </summary>
        internal static bool IsRosterPersistenceScope(string scope)
        {
            return scope == Scope || scope == AnkylosaurusScope ||
                scope == ArmoredDinosaursScope || scope == TheropodsScope ||
                scope == GiantBodiesScope || scope == MagicalBeastsScope ||
                scope == GriffonScope || scope == GiantHumanoidsScope ||
                scope == GiantHumanoidsTwoScope ||
                scope == OutsiderCelestialScope || scope == FeyCelestialScope ||
                scope == ShadowOutsidersScope ||
                scope == InfernalOutsidersScope ||
                scope == InfernalReachTwoScope ||
                scope == InfernalDemonsScope || scope == VrockScope ||
                scope == GlabrezuScope || scope == NalfeshneeScope;
        }

        /// <summary>
        /// A named subset scope rather than the broad released-roster fixture.
        /// With the feature module off, such a scope's saved receipts do
        /// reconstruct natively while publication and private visual
        /// allocation stay disabled; the whole-roster fixture instead expects
        /// not one of this mod's creatures to instantiate. This was a ninth
        /// hand-maintained sprint list and Sprint 37 was missing from it, so
        /// the module-off stage demanded zero units from a scope whose saved
        /// pair had correctly come back.
        /// </summary>
        internal static bool IsTargetedSubsetScope(string scope)
        {
            return scope != Scope && IsRosterPersistenceScope(scope);
        }

        internal static bool ObserveLoad(string scenario, string scope)
        {
            return IsRosterPersistenceScope(scope) &&
                (scenario == RuntimeTestScenarioCatalog.WorkingSaveExpandedSummoningPrepare ||
                scenario == RuntimeTestScenarioCatalog.WorkingSaveExpandedSummoningVerifyCleanup ||
                scenario == RuntimeTestScenarioCatalog.WorkingSaveExpandedSummoningVerifyAbsent);
        }
        internal static bool Owns(string scope, string key, string storedUnit, string actualUnit,
            string storedCaster, string actualCaster, string storedBlueprint, string expectedBlueprint, string actualBlueprint)
        {
            return scope == ReceiptScope && Keys.Contains(key, StringComparer.Ordinal) &&
                !string.IsNullOrWhiteSpace(storedUnit) && storedUnit == actualUnit &&
                !string.IsNullOrWhiteSpace(storedCaster) && storedCaster == actualCaster &&
                !string.IsNullOrWhiteSpace(expectedBlueprint) && storedBlueprint == expectedBlueprint &&
                actualBlueprint == expectedBlueprint;
        }
    }
}
