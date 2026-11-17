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
    /// observations, and the cancellation case judged on its own terms.
    /// Engine-free: the product's component scenario, the loader-compat
    /// real-route probe (through the *Json entries) and the domain tests all
    /// call this one evaluator.
    ///
    /// Nothing missing or unreadable is taken as zero or false. The deed
    /// packet is read from its own damage event, never from hit-point loss.
    /// The miss packet is compared with half of the full deed roll recorded
    /// where the dice were drawn, never derived from the packet. Consumption
    /// is the exact armed instance removed inside the qualifying delivery and
    /// before its own deadline, never a later read that finds no buff.
    /// Recovery counts only when it is attributed to the actor.
    /// </summary>
    internal static class UpCloseAndDeadlyCaseVerdict
    {
        internal const string Outcome = "outcome";
        internal const string Damage = "damage";
        internal const string Cost = "cost";
        internal const string Consumption = "consumption";

        /// <summary>Recovery attributions a cost verdict accepts.</summary>
        internal const string NoRecoveryApplied = "none-applied";
        internal const string RecoveryOfTheActor = "actor";

        internal static string EvaluateJson(string expectation, string observation)
        {
            return Evaluate(JObject.Parse(expectation), JObject.Parse(observation))
                .ToString(Newtonsoft.Json.Formatting.None);
        }

        internal static string EvaluateCancellationJson(string observation)
        {
            return EvaluateCancellation(JObject.Parse(observation)).ToString(Newtonsoft.Json.Formatting.None);
        }

        /// <summary>
        /// expectation: outcome (hit | miss | critical | any), damage (full |
        /// half | none), dice, charge (0 or 1), consume (bool), armed (bool,
        /// default true), optionally completedNonMisfire (bool).
        /// observation: hit, criticalConfirmed, criticalSource,
        /// completedNonMisfire, deedPackets, missRoll {dice}, gritBefore,
        /// gritAfter, gritRestored, gritRestoredAttribution, armedInstance
        /// {identified, removalSequence, removalGameSeconds,
        /// expiryDeadlineGameSeconds, deliveryStartSequence,
        /// deliveryEndSequence, deedDamageSequence}.
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
            JToken token = value == null ? null : value[key];
            return token == null || token.Type != JTokenType.Boolean ? (bool?)null : (bool)token;
        }

        private static int? Int(JObject value, string key)
        {
            JToken token = value == null ? null : value[key];
            return token == null || token.Type != JTokenType.Integer ? (int?)null : (int)token;
        }

        private static double? Number(JObject value, string key)
        {
            JToken token = value == null ? null : value[key];
            return token == null || (token.Type != JTokenType.Float && token.Type != JTokenType.Integer)
                ? (double?)null : (double)token;
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
            var packet = packets.Count == 1 ? packets[0] as JObject : null;
            if (packet == null)
                return Verdict(false, DescribeDamage(wanted, dice), observed);
            string kind = (string)packet["kind"];
            int? rolls = Int(packet, "diceRolls"), sides = Int(packet, "diceSides"),
                bonus = Int(packet, "bonus"), rolled = Int(packet, "rolled"),
                final = Int(packet, "final"), applied = Int(packet, "applied");
            JToken modifierToken = packet["criticalModifier"];
            int? modifier = modifierToken == null || modifierToken.Type != JTokenType.Integer
                ? (int?)null : (int)modifierToken;
            bool unmultiplied = modifierToken != null && (!modifier.HasValue || modifier.Value <= 1);
            if (wanted == "full")
            {
                bool pass = kind == "physical" && rolls == dice && sides == 6 && bonus == 0 &&
                    unmultiplied && rolled.HasValue && rolled.Value >= dice &&
                    rolled.Value <= 6 * dice && final == rolled && applied == final;
                return Verdict(pass, DescribeDamage(wanted, dice), observed);
            }
            if (wanted != "half") throw new ArgumentException("unknown expected damage " + wanted);

            // Exact half: the full deed roll comes from where the dice were
            // drawn, never from the packet it produced.
            var missRoll = observation["missRoll"] as JObject;
            var drawn = missRoll == null ? null : missRoll["dice"] as JArray;
            if (drawn == null || drawn.Any(token => token.Type != JTokenType.Integer))
                return Verdict(false, DescribeDamage(wanted, dice),
                    observed + ";fullRoll=not observed");
            int[] values = drawn.Select(token => (int)token).ToArray();
            int total = values.Sum();
            observed += ";fullRoll=" + string.Join("+", values.Select(value => value.ToString(CultureInfo.InvariantCulture)).ToArray()) +
                "=" + total.ToString(CultureInfo.InvariantCulture);
            bool drawsValid = values.Length == dice && values.All(value => value >= 1 && value <= 6);
            int half = total / 2;
            bool exact = drawsValid && kind == "direct" && rolls == 0 && unmultiplied &&
                bonus == half && final == half && applied == final;
            return Verdict(exact, DescribeDamage(wanted, dice) + " (floor of the recorded full roll / 2)", observed);
        }

        private static string DescribeDamage(string wanted, int dice)
        {
            if (wanted == "none") return "no deed damage event";
            if (wanted == "full")
                return "one deed packet: " + dice + "d6 physical, no critical multiplier, applied as rolled";
            return "one deed packet: exactly half of the recorded " + dice + "d6 deed roll rounded down, as a fixed amount, no critical multiplier";
        }

        private static JObject JudgeCost(JObject expectation, JObject observation)
        {
            int charge = Int(expectation, "charge") ?? -1;
            int? before = Int(observation, "gritBefore"), after = Int(observation, "gritAfter"),
                restored = Int(observation, "gritRestored");
            string attribution = (string)observation["gritRestoredAttribution"];
            string observed = "before=" + Text(before) + ";restoredByRecovery=" + Text(restored) +
                " (" + Text(attribution) + ");after=" + Text(after);
            bool attributed = restored.HasValue &&
                ((attribution == NoRecoveryApplied && restored.Value == 0) || attribution == RecoveryOfTheActor);
            if (!before.HasValue || !after.HasValue || !attributed)
                return Verdict(false, "deed charge " + charge + " (separate from any recovery attributed to the actor)",
                    observed + ";deedCharge=unknown");
            int deedCharge = before.Value + restored.Value - after.Value;
            return Verdict(deedCharge == charge, "deed charge " + charge + " (separate from any recovery attributed to the actor)",
                observed + ";deedCharge=" + deedCharge.ToString(CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// Whether the exact instance's removal falls inside the qualifying
        /// delivery: after the roll resolved, before the deed's damage event
        /// when there is one (the handler removes the state before it deals
        /// damage), otherwise before the delivery's observed end; and before
        /// the instance's own deadline. Null when the evidence cannot say.
        /// </summary>
        private static bool? RemovedDuringDelivery(JObject armed, out string observed)
        {
            int? removal = Int(armed, "removalSequence"), start = Int(armed, "deliveryStartSequence"),
                end = Int(armed, "deliveryEndSequence"), deed = Int(armed, "deedDamageSequence");
            double? removedAt = Number(armed, "removalGameSeconds"), deadline = Number(armed, "expiryDeadlineGameSeconds");
            observed = "identified=" + Text(Bool(armed, "identified")) + ";removalSequence=" + Text(removal) +
                ";deliveryStart=" + Text(start) + ";deedDamage=" + Text(deed) + ";deliveryEnd=" + Text(end) +
                ";removedAt=" + Text(removedAt) + ";deadline=" + Text(deadline);
            if (!removal.HasValue) return false;
            if (!start.HasValue || !removedAt.HasValue || !deadline.HasValue) return null;
            int? bound = deed ?? end;
            if (!bound.HasValue) return null;
            return removal.Value > start.Value && removal.Value < bound.Value && removedAt.Value < deadline.Value;
        }

        private static JObject JudgeConsumption(JObject expectation, JObject observation)
        {
            bool consume = Bool(expectation, "consume") ?? false;
            bool armedExpected = Bool(expectation, "armed") ?? true;
            var armed = observation["armedInstance"] as JObject;
            bool? identified = Bool(armed, "identified");
            if (!armedExpected)
                return Verdict(identified == false, "no armed instance before the shot, so nothing to consume",
                    "identified=" + Text(identified));
            string observed = "armed instance not observed";
            bool? during = null;
            if (armed != null) during = RemovedDuringDelivery(armed, out observed);
            bool pass = identified == true && during.HasValue &&
                (consume ? during.Value : !during.Value);
            return Verdict(pass,
                consume ? "the exact armed instance removed inside the qualifying delivery, before its own deadline"
                    : "the exact armed instance not removed inside the delivery",
                observed);
        }

        /// <summary>
        /// The cancellation case: an order that was admitted to the actor's
        /// commands and interrupted before it started. A rejected or never
        /// admitted order is an unmet prerequisite, whatever else it shows.
        /// </summary>
        internal static JObject EvaluateCancellation(JObject observation)
        {
            observation = observation ?? new JObject();
            bool? admitted = Bool(observation, "orderTakenByTheUnit"), started = Bool(observation, "orderStarted"),
                acted = Bool(observation, "orderActed");
            string resultText = (string)observation["orderResult"];
            int? rolls = Int(observation, "attackRollsResolved"), deed = Int(observation, "deedPacketsFromShooterInWindow"),
                gritBefore = Int(observation, "gritBeforeOrder"), gritAfter = Int(observation, "gritAfterCancel"),
                gritAfterExpiry = Int(observation, "gritAfterExpiry");
            bool? retained = Bool(observation, "armedInstanceRetainedAfterCancel");
            double? lasted = Number(observation, "endedAfterGameSeconds");
            var result = new JObject
            {
                ["prerequisite"] = Verdict(admitted == true && started == false && acted == false && resultText == "Interrupt",
                    "the exact new order admitted to the actor's commands, not started, not acted, ended Interrupt",
                    "admitted=" + Text(admitted) + ";started=" + Text(started) + ";acted=" + Text(acted) + ";result=" + Text(resultText)),
                ["noShot"] = Verdict(rolls == 0 && deed == 0, "no attack roll and no deed packet from the actor",
                    "attackRolls=" + Text(rolls) + ";deedPackets=" + Text(deed)),
                ["retention"] = Verdict(retained == true, "the exact armed instance still on the actor after the cancellation",
                    "retained=" + Text(retained)),
                ["cost"] = Verdict(gritBefore.HasValue && gritAfter == gritBefore && gritAfterExpiry == gritBefore,
                    "no grit change at the cancellation or at the later expiry",
                    "before=" + Text(gritBefore) + ";afterCancel=" + Text(gritAfter) + ";afterExpiry=" + Text(gritAfterExpiry)),
                ["expiry"] = Verdict(lasted.HasValue && Math.Abs(lasted.Value - 6.0) <= 0.35,
                    "the retained state then expired by itself after one round",
                    "lastedGameSeconds=" + Text(lasted))
            };
            result["pass"] = new[] { "prerequisite", "noShot", "retention", "cost", "expiry" }
                .All(key => (bool)result[key]["pass"]);
            return result;
        }
    }
}
