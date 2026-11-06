using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.RuntimeTesting
{
    /// <summary>
    /// Sprint 4 adjacent routine acceptance for any representative summon.
    /// It generalizes the Sprint 3 Bone Devil verdict
    /// (InfernalAdjacentRoutineVerdict35, unchanged) by taking the expected
    /// routine as data: "expected" is the weapon multiset established
    /// independently for that creature, loadout and distance. It passes only
    /// when all of the following hold:
    /// - the native plan equals the expected multiset;
    /// - every planned slot resolves, hit or miss, against the exact target
    ///   under the exact command;
    /// - the command ends Success, or with the installed stack's
    ///   exhausted-routine Interrupt: a command-end row for this command and
    ///   actor at the final index, with no next planned attack, raised by
    ///   UnitAttack.OnTick;
    /// - the attacker keeps its summon lifetime and is alive;
    /// - ProperFlanking2 is loaded, active, patched, and its live
    ///   reach_weapons_dead_zone is true;
    /// - the target stood adjacent and threatened before the command, and
    ///   neither unit moved.
    /// </summary>
    internal static class SummonAdjacentRoutineVerdict4
    {
        internal const double MaximumAdjacentEdgeGapMeters = .3d;
        internal const double MaximumTravelMeters = .25d;

        internal static string Failure(JObject record)
        {
            if (record == null) return "no-record";
            var expected = (record["expected"] as JArray ?? new JArray()).Select(value => (string)value).ToList();
            if (expected.Count == 0 || expected.Any(string.IsNullOrEmpty)) return "expected-routine-missing";
            var plan = (record["plan"] as JArray ?? new JArray()).Select(value => (string)value).ToList();
            if (plan.Count == 0) return "no-native-plan";
            if (!SameMultiset(plan, expected)) return "plan-differs-from-expected-routine";
            string target = (string)record["target"], owner = (string)record["owner"];
            int? reference = (int?)record["commandReference"];
            if (string.IsNullOrEmpty(target) || string.IsNullOrEmpty(owner) || reference == null)
                return "command-identity-missing";
            var attacks = (record["attacks"] as JArray ?? new JArray()).OfType<JObject>().ToList();
            if (attacks.Any(row => (string)row["owner"] != owner || (string)row["target"] != target ||
                    (int?)row["attackCommand"] != reference))
                return "attack-outside-measured-command";
            if (attacks.Count < plan.Count) return "planned-slot-unresolved";
            if (attacks.Count > plan.Count) return "extra-attack";
            if (!SameMultiset(attacks.Select(row => (string)row["weapon"]).ToList(), plan))
                return "resolved-slots-differ-from-plan";
            if ((bool?)record["commandFinished"] != true) return "command-not-finished";
            string result = (string)record["commandResult"];
            if (result == "Interrupt")
            {
                var ends = (record["commandEnds"] as JArray ?? new JArray()).OfType<JObject>().Where(row =>
                    (string)row["owner"] == owner && (int?)row["commandReference"] == reference).ToList();
                if (!ends.Any(row => (int?)row["attackIndex"] == plan.Count &&
                        (bool?)row["plannedAttackPresent"] == false &&
                        ((string)row["interruptionCaller"] ?? "").Contains("UnitAttack.OnTick")))
                    return "interrupted-without-native-exhaustion";
            }
            else if (result != "Success") return "command-result-" + (result ?? "missing");
            if ((bool?)record["ownerLifetimePresentAfter"] != true || (bool?)record["ownerAliveAfter"] != true)
                return "attacker-lifetime-lost";
            var flanking = record["properFlanking2"] as JObject;
            if ((bool?)flanking?["assemblyLoaded"] != true) return "properflanking2-assembly-absent";
            if ((bool?)flanking?["modActive"] != true) return "properflanking2-mod-inactive";
            if ((bool?)flanking?["reachWeaponsDeadZone"] != true) return "properflanking2-dead-zone-not-enabled";
            if ((bool?)flanking?["getTargetPointPatched"] != true || (bool?)flanking?["isReachPatched"] != true)
                return "properflanking2-dead-zone-patch-absent";
            double? gap = (double?)record["edgeGap"];
            if (gap == null || gap.Value < 0d || gap.Value > MaximumAdjacentEdgeGapMeters) return "target-not-adjacent";
            if ((bool?)record["threatens"] != true) return "target-not-threatened";
            if (((double?)record["ownerTravel"] ?? 99d) >= MaximumTravelMeters ||
                ((double?)record["targetTravel"] ?? 99d) >= MaximumTravelMeters)
                return "unit-moved";
            return null;
        }

        private static bool SameMultiset(List<string> left, List<string> right)
        {
            if (left.Count != right.Count || left.Any(string.IsNullOrEmpty)) return false;
            var counts = right.GroupBy(value => value).ToDictionary(group => group.Key, group => group.Count());
            foreach (string value in left)
            {
                int count;
                if (!counts.TryGetValue(value, out count) || count == 0) return false;
                counts[value] = count - 1;
            }
            return true;
        }
    }
}
