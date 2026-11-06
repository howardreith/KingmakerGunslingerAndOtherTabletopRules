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
    ///   under the exact command: as a weapon attack, or, only where the
    ///   attacker held an active non-charge ProperFlanking2 attack-replacement
    ///   toggle before the command, as that installed combat maneuver (a
    ///   standalone roll with a planned weapon at an act with no weapon
    ///   attack);
    /// - the command ends Success, or with an Interrupt at the exhausted
    ///   routine: a command-end row for this command and actor at the final
    ///   index with no next planned attack, raised either by
    ///   UnitAttack.OnTick (the installed stack's ordinary end) or by the
    ///   routine's own terminal grab. A terminal grab needs evidence: the
    ///   attacker gained the grapple hold and the target became grappled
    ///   during the command (neither before), the end came at the final
    ///   act's instant, and the final attack was a hit with an expected
    ///   weapon. Any other interruption fails;
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
            var replaced = (record["replacedSlots"] as JArray ?? new JArray()).OfType<JObject>().ToList();
            if (attacks.Concat(replaced).Any(row => (string)row["owner"] != owner || (string)row["target"] != target ||
                    (int?)row["attackCommand"] != reference))
                return "attack-outside-measured-command";
            if (replaced.Count > 0)
            {
                bool toggle = (record["attackReplacements"] as JArray ?? new JArray()).OfType<JObject>().Any(value =>
                    (bool?)value["active"] == true && (bool?)value["onlyCharge"] != true && !string.IsNullOrEmpty((string)value["maneuver"]));
                if (!toggle || replaced.Any(row => (bool?)row["standaloneRoll"] != true))
                    return "replacement-without-installed-toggle";
            }
            int resolved = attacks.Count + replaced.Count;
            if (resolved < plan.Count) return "planned-slot-unresolved";
            if (resolved > plan.Count) return "extra-attack";
            if (!SameMultiset(attacks.Concat(replaced).Select(row => (string)row["weapon"]).ToList(), plan))
                return "resolved-slots-differ-from-plan";
            if ((bool?)record["commandFinished"] != true) return "command-not-finished";
            string result = (string)record["commandResult"];
            if (result == "Interrupt")
            {
                var ends = (record["commandEnds"] as JArray ?? new JArray()).OfType<JObject>().Where(row =>
                    (string)row["owner"] == owner && (int?)row["commandReference"] == reference).ToList();
                bool grab = TerminalGrab(record["terminalGrab"] as JObject, expected);
                if (!ends.Any(row => (int?)row["attackIndex"] == plan.Count &&
                        (bool?)row["plannedAttackPresent"] == false &&
                        (((string)row["interruptionCaller"] ?? "").Contains("UnitAttack.OnTick") || grab)))
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

        private static bool TerminalGrab(JObject evidence, List<string> expected)
        {
            return evidence != null && (bool?)evidence["attackerHoldBefore"] == false &&
                (bool?)evidence["attackerHoldAfter"] == true && (bool?)evidence["targetGrappledBefore"] == false &&
                (bool?)evidence["targetGrappledAfter"] == true && (bool?)evidence["endAtFinalAct"] == true &&
                (bool?)evidence["finalAttackHit"] == true && expected.Contains((string)evidence["finalAttackWeapon"]);
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
