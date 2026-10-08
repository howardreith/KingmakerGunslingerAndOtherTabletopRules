using System;
using System.Collections.Generic;
using System.Linq;
using KingmakerGunslinger.Summoning;

namespace KingmakerGunslinger.RuntimeTesting
{
    // Closed extension of the existing prepare/cleanup/absence trio. It is
    // fixture ownership, not production serialization or grapple restoration.
    internal static class ExpandedSummoningRosterPersistencePolicy
    {
        internal const string Scope = "whole-roster";
        internal const string ReceiptScope = "KMG_Release143_WholeRoster_v1";
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
