using System;
using System.IO;
using System.Linq;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.DomainTests
{
    internal static class CrabSummonAnimationBindingPolicyTests
    {
        internal static void AllPublishedSourcesRequireExactScope()
        {
            var entries = JObject.Parse(File.ReadAllText(Path.Combine(Environment.CurrentDirectory,
                "blueprints", "blueprints.json")))["entries"];
            string[] manifestSources = entries.Where(value => (string)value["plannedType"] == "BlueprintAbility" &&
                (string)value["status"] == "active" && ((string)value["symbol"]).Contains("GiantCrab"))
                .Select(value => (string)value["guid"]).OrderBy(value => value, StringComparer.Ordinal).ToArray();
            string[] policySources = CrabSummonAnimationBindingPolicy.SourceGuids.OrderBy(value => value, StringComparer.Ordinal).ToArray();
            Assertions.True(manifestSources.Length > 0 && manifestSources.SequenceEqual(policySources),
                "Crab animation admission must cover exactly the released manifest Crab ability/adapter identities.");
            foreach (string source in CrabSummonAnimationBindingPolicy.SourceGuids)
            {
                var scope = Valid(); scope.SourceGuid = source;
                Assertions.True(CrabSummonAnimationBindingPolicy.Permits(scope), "Exact published Crab source refused: " + source);
            }
        }
        internal static void EveryOwnershipGateRejectsIndependently()
        {
            Action<CrabAnimationBindingScope>[] changes = {
                value => value.ModuleActive = false, value => value.MissingResult = false, value => value.RequestedClaw = false,
                value => value.RegisteredUnit = false, value => value.RegisteredSource = false, value => value.ExactManagerView = false,
                value => value.LiveOriginalBody = false, value => value.LivePositiveSummon = false, value => value.ExactClawWeapons = false,
                value => value.NativeReceiptUnchanged = false, value => value.UnitGuid = CrabSummonAnimationBindingPolicy.SpiderGuid,
                value => value.UnitName = "KMG_Summoning_Unit_GiantCrab_Foreign", value => value.PrefabGuid = "foreign",
                value => value.SourceGuid = "foreign", value => value.SourceGuid = null
            };
            foreach (var change in changes)
            {
                var scope = Valid(); change(scope);
                Assertions.False(CrabSummonAnimationBindingPolicy.Permits(scope), "A foreign/off/nonnull/missing ownership gate was waived.");
            }
            Assertions.False(CrabSummonAnimationBindingPolicy.Permits(null), "Missing scope accepted.");
        }
        private static CrabAnimationBindingScope Valid()
        {
            return new CrabAnimationBindingScope { ModuleActive = true, MissingResult = true, RequestedClaw = true, RegisteredUnit = true,
                RegisteredSource = true, ExactManagerView = true, LiveOriginalBody = true, LivePositiveSummon = true, ExactClawWeapons = true,
                NativeReceiptUnchanged = true, UnitGuid = CrabSummonAnimationBindingPolicy.UnitGuid, UnitName = CrabSummonAnimationBindingPolicy.UnitName,
                PrefabGuid = CrabSummonAnimationBindingPolicy.PrefabGuid, SourceGuid = CrabSummonAnimationBindingPolicy.SourceGuids[0] };
        }
    }
}
