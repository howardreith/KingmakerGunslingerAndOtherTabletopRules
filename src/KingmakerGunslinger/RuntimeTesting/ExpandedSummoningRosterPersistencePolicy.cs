using System;
using System.Collections.Generic;
using System.Linq;
using KingmakerGunslinger.Summoning;

namespace KingmakerGunslinger.RuntimeTesting
{
    // Bounded request-local observation, never a source of unit state.
    internal sealed class RosterReadinessTimeline
    {
        internal static readonly string[] SnakePredicates = {
            "EntityFadedIn", "DissolveSettled", "CanAct", "CanMove", "AppearanceBuffAbsent"
        };
        private readonly Dictionary<string, int> _firstTrue = new Dictionary<string, int>(StringComparer.Ordinal);
        private Dictionary<string, bool> _last = new Dictionary<string, bool>(StringComparer.Ordinal);
        internal readonly int? SpawnedFrame;
        internal int? FirstAllSnakeFrame, FirstAllObservedFrame;
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
            _last = new Dictionary<string, bool>(predicates, StringComparer.Ordinal); SampleCount++;
        }
        internal string[] FailedSnakePredicates()
        { return SnakePredicates.Where(p => !_last.ContainsKey(p) || !_last[p]).ToArray(); }
        internal string[] FailedObservedPredicates()
        { return _last.Where(p => !p.Value).Select(p => p.Key).OrderBy(p => p, StringComparer.Ordinal).ToArray(); }
        internal void Clear()
        { _firstTrue.Clear(); _last.Clear(); FirstAllSnakeFrame = null; FirstAllObservedFrame = null; SampleCount = 0; }
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
