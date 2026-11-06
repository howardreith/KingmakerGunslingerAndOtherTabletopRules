using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.RuntimeTesting
{
    /// <summary>
    /// Sprint 3 adjacent natural-attack acceptance over the recorded receipt.
    /// The Bone Devil's complete native routine is bite, two claws and sting.
    /// It passes only when:
    /// - the profile names all three natural weapons and the command's native
    ///   plan is exactly those four slots;
    /// - every planned slot resolves (hit or miss) against the exact target
    ///   under the exact measured command;
    /// - the command reaches its normal native terminal boundary: Success, or
    ///   the installed stack's exhausted-routine Interrupt (command-end at the
    ///   final attack index, no next planned attack, raised by
    ///   UnitAttack.OnTick);
    /// - the attacker keeps its summon lifetime and is alive at the end;
    /// - ProperFlanking2 is loaded, active, patched and its
    ///   reach_weapons_dead_zone option is actually enabled;
    /// - the target stands inside that dead zone, is threatened, and neither
    ///   unit moves away.
    /// Hits are not required; a miss is a resolved attack.
    /// </summary>
    internal static class InfernalAdjacentRoutineVerdict35
    {
        internal static string Failure(JObject record)
        {
            if (record == null) return "no-record";
            var profile = record["profile"] as JObject;
            string bite = (string)profile?["bite"], claw = (string)profile?["claw"], sting = (string)profile?["sting"];
            if (string.IsNullOrEmpty(bite) || string.IsNullOrEmpty(claw) || string.IsNullOrEmpty(sting))
                return "profile-weapon-missing";
            var expected = new List<string> { bite, claw, claw, sting };
            var plan = (record["plan"] as JArray ?? new JArray()).Select(value => (string)value).ToList();
            if (plan.Count == 0) return "no-native-plan";
            if (!SameMultiset(plan, expected)) return "plan-not-bite-two-claws-sting";
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
            if ((bool?)record["insideDeadZone"] != true) return "target-outside-dead-zone";
            if ((bool?)record["threatens"] != true) return "target-not-threatened";
            if (((double?)record["ownerTravel"] ?? 99d) >= .25d || ((double?)record["targetTravel"] ?? 99d) >= .25d)
                return "unit-moved";
            return null;
        }

        internal static bool Complete(JObject record) { return Failure(record) == null; }

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
