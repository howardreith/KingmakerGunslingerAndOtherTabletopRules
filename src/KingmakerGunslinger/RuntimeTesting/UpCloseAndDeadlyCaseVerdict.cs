using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.RuntimeTesting
{
    /// <summary>
    /// REP-02 / S4 T2: one Up Close and Deadly case judged as four separate
    /// results (the attack's outcome, the deed's damage packet, the deed's
    /// grit charge and the armed state's consumption) from independent
    /// observations. Engine-free: the product's component scenario, the
    /// loader-compat real-route probe (through <see cref="EvaluateJson"/>) and
    /// the domain tests all call this one evaluator.
    ///
    /// Nothing missing or unreadable is taken as zero or false: an absent
    /// observation fails the result that needed it. The deed packet is read
    /// from its own damage event, never from hit-point loss, and an
    /// unmultiplied critical is judged from the packet's own critical
    /// modifier and dice, never by comparing two independent rolls.
    /// </summary>
    internal static class UpCloseAndDeadlyCaseVerdict
    {
        internal const string Outcome = "outcome";
        internal const string Damage = "damage";
        internal const string Cost = "cost";
        internal const string Consumption = "consumption";

        /// <summary>Reflection-friendly entry for the loader-compat harness.</summary>
        internal static string EvaluateJson(string expectation, string observation)
        {
            return Evaluate(JObject.Parse(expectation), JObject.Parse(observation))
                .ToString(Newtonsoft.Json.Formatting.None);
        }

        /// <summary>
        /// expectation: outcome (hit | miss | critical | any), damage (full |
        /// half | none), dice, charge (0 or 1), consume (bool), and optionally
        /// completedNonMisfire (bool).
        /// observation: hit, criticalConfirmed, criticalSource (engine |
        /// set-after-resolution), completedNonMisfire, deedPackets (array),
        /// gritBefore, gritAfter, gritRestored, armedInstanceRemoved.
        /// </summary>
        internal static JObject Evaluate(JObject expectation, JObject observation)
        {
            if (expectation == null) throw new ArgumentNullException("expectation");
            observation = observation ?? new JObject();
            var result = new JObject
            {
                [Outcome] = JudgeOutcome(expectation, observation),
                [Damage] = JudgeDamage(expectation, observation),
                [Cost] = JudgeCost(expectation, observation),
                [Consumption] = JudgeConsumption(expectation, observation)
            };
            result["pass"] = new[] { Outcome, Damage, Cost, Consumption }
                .All(key => (bool)result[key]["pass"]);
            return result;
        }

        private static JObject Verdict(bool pass, string expected, string observed)
        {
            return new JObject { ["pass"] = pass, ["expected"] = expected, ["observed"] = observed };
        }

        private static bool? Bool(JObject value, string key)
        {
            JToken token = value[key];
            return token == null || token.Type != JTokenType.Boolean ? (bool?)null : (bool)token;
        }

        private static int? Int(JObject value, string key)
        {
            JToken token = value[key];
            return token == null || token.Type != JTokenType.Integer ? (int?)null : (int)token;
        }

        private static string Text(object value)
        {
            if (value == null) return "not observed";
            if (value is bool) return (bool)value ? "true" : "false";
            return Convert.ToString(value, CultureInfo.InvariantCulture);
        }

        private static JObject JudgeOutcome(JObject expectation, JObject observation)
        {
            string wanted = (string)expectation["outcome"] ?? "any";
            bool? hit = Bool(observation, "hit");
            bool? critical = Bool(observation, "criticalConfirmed");
            string source = (string)observation["criticalSource"];
            bool? completed = Bool(observation, "completedNonMisfire");
            bool? wantedCompleted = Bool(expectation, "completedNonMisfire");
            string observed = "hit=" + Text(hit) + ";criticalConfirmed=" + Text(critical) +
                ";criticalSource=" + Text(source) + ";completedNonMisfire=" + Text(completed);
            bool pass;
            switch (wanted)
            {
                case "hit": pass = hit == true && critical == false; break;
                case "miss": pass = hit == false; break;
                case "critical": pass = hit == true && critical == true && source == "engine"; break;
                case "any": pass = hit.HasValue; break;
                default: throw new ArgumentException("unknown expected outcome " + wanted);
            }
            if (wantedCompleted.HasValue) pass = pass && completed == wantedCompleted.Value;
            string expected = wanted == "critical" ? "a hit the engine confirmed as a critical" :
                wanted == "hit" ? "an ordinary hit (no confirmed critical)" :
                wanted == "miss" ? "a miss" : "any observed outcome";
            if (wantedCompleted.HasValue)
                expected += wantedCompleted.Value ? ", completed without a misfire" : ", not a completed non-misfire discharge";
            return Verdict(pass, expected, observed);
        }

        private static JObject JudgeDamage(JObject expectation, JObject observation)
        {
            string wanted = (string)expectation["damage"];
            int dice = Int(expectation, "dice") ?? 0;
            var packets = observation["deedPackets"] as JArray;
            if (packets == null)
                return Verdict(false, DescribeDamage(wanted, dice), "deed damage events not observed");
            string observed = "packets=" + packets.ToString(Newtonsoft.Json.Formatting.None);
            if (wanted == "none")
                return Verdict(packets.Count == 0, DescribeDamage(wanted, dice), observed);
            if (packets.Count != 1)
                return Verdict(false, DescribeDamage(wanted, dice), observed);
            var packet = packets[0] as JObject;
            if (packet == null)
                return Verdict(false, DescribeDamage(wanted, dice), observed);
            string kind = (string)packet["kind"];
            int? rolls = Int(packet, "diceRolls"), sides = Int(packet, "diceSides"),
                bonus = Int(packet, "bonus"), rolled = Int(packet, "rolled"),
                final = Int(packet, "final"), applied = Int(packet, "applied");
            JToken modifierToken = packet["criticalModifier"];
            bool modifierRead = modifierToken != null;
            int? modifier = modifierToken == null || modifierToken.Type != JTokenType.Integer
                ? (int?)null : (int)modifierToken;
            bool unmultiplied = modifierRead && (!modifier.HasValue || modifier.Value <= 1);
            bool pass;
            if (wanted == "full")
            {
                pass = kind == "physical" && rolls == dice && sides == 6 && bonus == 0 &&
                    unmultiplied && rolled.HasValue && rolled.Value >= dice &&
                    rolled.Value <= 6 * dice && final == rolled && applied == final;
            }
            else if (wanted == "half")
            {
                // The miss packet is half of one full deed roll, rounded down,
                // carried as a fixed bonus: within [floor(dice/2), 3 * dice].
                pass = kind == "direct" && rolls == 0 && unmultiplied && bonus.HasValue &&
                    bonus.Value >= dice / 2 && bonus.Value <= 3 * dice &&
                    final == bonus && applied == final;
            }
            else throw new ArgumentException("unknown expected damage " + wanted);
            return Verdict(pass, DescribeDamage(wanted, dice), observed);
        }

        private static string DescribeDamage(string wanted, int dice)
        {
            if (wanted == "none") return "no deed damage event";
            if (wanted == "full")
                return "one deed packet: " + dice + "d6 physical, no critical multiplier, applied as rolled";
            return "one deed packet: half of one " + dice + "d6 roll rounded down, as a fixed amount, no critical multiplier";
        }

        private static JObject JudgeCost(JObject expectation, JObject observation)
        {
            int charge = Int(expectation, "charge") ?? -1;
            int? before = Int(observation, "gritBefore"), after = Int(observation, "gritAfter"),
                restored = Int(observation, "gritRestored");
            string observed = "before=" + Text(before) + ";restoredByRecovery=" + Text(restored) +
                ";after=" + Text(after);
            if (!before.HasValue || !after.HasValue || !restored.HasValue)
                return Verdict(false, "deed charge " + charge, observed + ";deedCharge=not observed");
            int deedCharge = before.Value + restored.Value - after.Value;
            return Verdict(deedCharge == charge, "deed charge " + charge + " (separate from any grit recovery)",
                observed + ";deedCharge=" + deedCharge.ToString(CultureInfo.InvariantCulture));
        }

        private static JObject JudgeConsumption(JObject expectation, JObject observation)
        {
            bool consume = Bool(expectation, "consume") ?? false;
            bool? removed = Bool(observation, "armedInstanceRemoved");
            return Verdict(removed.HasValue && removed.Value == consume,
                consume ? "the exact armed instance removed" : "the exact armed instance retained",
                "armedInstanceRemoved=" + Text(removed));
        }
    }
}
