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
        internal const string ReceiptScope = "KMG_Release143_WholeRoster_v1";
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
                case "lantern-archon":
                case "bralani-azata":
                case "ghaele-azata": return 3;
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
        internal static string[] Keys
        {
            get { return ExpandedSummoningCatalog.All.Select(c => c.Key)
                .Concat(SummonNativeExpansionCatalog.All.Select(n => "native:" + n.UnitGuid))
                .Distinct(StringComparer.Ordinal).OrderBy(k => k, StringComparer.Ordinal).ToArray(); }
        }
        internal static bool ObserveLoad(string scenario, string scope)
        {
            return scope == Scope && (scenario == RuntimeTestScenarioCatalog.WorkingSaveExpandedSummoningPrepare ||
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
