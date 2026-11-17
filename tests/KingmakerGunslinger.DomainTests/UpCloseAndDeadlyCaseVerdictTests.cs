using KingmakerGunslinger.RuntimeTesting;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.DomainTests
{
    /// <summary>
    /// REP-02 / S4 T2: the Up Close and Deadly case evaluator keeps outcome,
    /// damage, cost and consumption apart, and nothing missing passes. Every
    /// control goes through the same Evaluate / EvaluateCancellation the
    /// product scenario and the loader-compat probe call.
    /// </summary>
    internal static class UpCloseAndDeadlyCaseVerdictTests
    {
        private static JObject Expect(string outcome, string damage, int charge, bool consume)
        {
            return new JObject
            {
                ["outcome"] = outcome, ["damage"] = damage, ["dice"] = 1,
                ["charge"] = charge, ["consume"] = consume, ["completedNonMisfire"] = true
            };
        }

        private static JObject FullPacket(int rolled, int? criticalModifier = null)
        {
            return new JObject
            {
                ["kind"] = "physical", ["diceRolls"] = 1, ["diceSides"] = 6, ["bonus"] = 0,
                ["criticalModifier"] = criticalModifier.HasValue ? (JToken)criticalModifier.Value : JValue.CreateNull(),
                ["rolled"] = rolled, ["final"] = rolled, ["applied"] = rolled
            };
        }

        private static JObject HalfPacket(int amount)
        {
            return new JObject
            {
                ["kind"] = "direct", ["diceRolls"] = 0, ["diceSides"] = 6, ["bonus"] = amount,
                ["criticalModifier"] = JValue.CreateNull(), ["rolled"] = 0, ["final"] = amount, ["applied"] = amount
            };
        }

        /// <summary>The exact instance removed after the roll (2), before the deed's damage (4), before its deadline.</summary>
        private static JObject RemovedInDelivery()
        {
            return new JObject
            {
                ["identified"] = true, ["deliveryStartSequence"] = 1, ["removalSequence"] = 2,
                ["deedDamageSequence"] = 3, ["deliveryEndSequence"] = 4,
                ["removalGameSeconds"] = 100.0, ["expiryDeadlineGameSeconds"] = 106.0
            };
        }

        private static JObject Observed(bool hit, bool critical, JObject packet, int before, int restored, int after,
            JObject armed, string attribution = UpCloseAndDeadlyCaseVerdict.NoRecoveryApplied, int[] missDice = null)
        {
            var value = new JObject
            {
                ["hit"] = hit, ["criticalConfirmed"] = critical, ["criticalSource"] = critical ? "engine" : "none",
                ["completedNonMisfire"] = true,
                ["deedPackets"] = packet == null ? new JArray() : new JArray(packet),
                ["gritBefore"] = before, ["gritRestored"] = restored, ["gritAfter"] = after,
                ["gritRestoredAttribution"] = attribution,
                ["armedInstance"] = armed
            };
            if (missDice != null) value["missRoll"] = new JObject { ["dice"] = new JArray(missDice) };
            return value;
        }

        private static bool Pass(JObject verdict, string part) => (bool)verdict[part]["pass"];

        private static JObject Judge(JObject expectation, JObject observation) =>
            UpCloseAndDeadlyCaseVerdict.Evaluate(expectation, observation);

        internal static void CorrectCasesPass()
        {
            JObject verdict = Judge(Expect("hit", "full", 1, true), Observed(true, false, FullPacket(4), 5, 0, 4, RemovedInDelivery()));
            Assertions.True((bool)verdict["pass"], "A correct ordinary hit must pass: " + verdict);
            verdict = Judge(Expect("miss", "half", 1, true), Observed(false, false, HalfPacket(2), 5, 0, 4, RemovedInDelivery(), missDice: new[] { 5 }));
            Assertions.True((bool)verdict["pass"], "A correct miss must pass: " + verdict);
        }

        internal static void FailedConsumptionFailsOnlyConsumption()
        {
            JObject retained = RemovedInDelivery();
            retained["removalSequence"] = JValue.CreateNull();
            retained["removalGameSeconds"] = JValue.CreateNull();
            JObject verdict = Judge(Expect("hit", "full", 1, true), Observed(true, false, FullPacket(4), 5, 0, 4, retained));
            Assertions.True(!Pass(verdict, UpCloseAndDeadlyCaseVerdict.Consumption), "A retained state must fail consumption.");
            Assertions.True(Pass(verdict, UpCloseAndDeadlyCaseVerdict.Damage) && Pass(verdict, UpCloseAndDeadlyCaseVerdict.Cost),
                "Correct damage and cost must still pass beside a failed consumption: " + verdict);
            Assertions.True(!(bool)verdict["pass"], "The aggregate must fail.");
        }

        internal static void WrongDamageFailsOnlyDamage()
        {
            JObject multiplied = Judge(Expect("critical", "full", 1, true), Observed(true, true, FullPacket(4, 4), 5, 0, 4, RemovedInDelivery()));
            Assertions.True(!Pass(multiplied, UpCloseAndDeadlyCaseVerdict.Damage), "A multiplied deed packet must fail damage.");
            Assertions.True(Pass(multiplied, UpCloseAndDeadlyCaseVerdict.Consumption) && Pass(multiplied, UpCloseAndDeadlyCaseVerdict.Cost),
                "Correct consumption and cost must still pass beside wrong damage: " + multiplied);
            JObject doubled = Judge(Expect("hit", "full", 1, true),
                new JObject(Observed(true, false, FullPacket(4), 5, 0, 4, RemovedInDelivery())) { ["deedPackets"] = new JArray(FullPacket(4), FullPacket(3)) });
            Assertions.True(!Pass(doubled, UpCloseAndDeadlyCaseVerdict.Damage), "Two deed packets must fail damage.");
        }

        internal static void ExactHalfOfTheRecordedRoll()
        {
            JObject five = Judge(Expect("miss", "half", 1, true), Observed(false, false, HalfPacket(2), 5, 0, 4, RemovedInDelivery(), missDice: new[] { 5 }));
            Assertions.True(Pass(five, UpCloseAndDeadlyCaseVerdict.Damage), "Full roll 5, emitted 2 must pass: " + five);
            foreach (int wrong in new[] { 0, 3 })
            {
                JObject verdict = Judge(Expect("miss", "half", 1, true),
                    Observed(false, false, HalfPacket(wrong), 5, 0, 4, RemovedInDelivery(), missDice: new[] { 5 }));
                Assertions.True(!Pass(verdict, UpCloseAndDeadlyCaseVerdict.Damage), "Full roll 5, emitted " + wrong + " must fail damage: " + verdict);
                Assertions.True(Pass(verdict, UpCloseAndDeadlyCaseVerdict.Cost) && Pass(verdict, UpCloseAndDeadlyCaseVerdict.Consumption),
                    "Correct cost and consumption must still pass beside a wrong half: " + verdict);
            }
            JObject one = Judge(Expect("miss", "half", 1, true), Observed(false, false, HalfPacket(0), 5, 0, 4, RemovedInDelivery(), missDice: new[] { 1 }));
            Assertions.True(Pass(one, UpCloseAndDeadlyCaseVerdict.Damage), "Full roll 1, emitted 0 must pass: " + one);
            JObject missing = Judge(Expect("miss", "half", 1, true), Observed(false, false, HalfPacket(2), 5, 0, 4, RemovedInDelivery()));
            Assertions.True(!Pass(missing, UpCloseAndDeadlyCaseVerdict.Damage), "A missing full roll must fail: " + missing);
            JObject unreadable = Judge(Expect("miss", "half", 1, true),
                new JObject(Observed(false, false, HalfPacket(2), 5, 0, 4, RemovedInDelivery())) { ["missRoll"] = new JObject { ["dice"] = new JArray("five") } });
            Assertions.True(!Pass(unreadable, UpCloseAndDeadlyCaseVerdict.Damage), "An unreadable full roll must fail: " + unreadable);
            JObject wrongCount = Judge(Expect("miss", "half", 1, true),
                Observed(false, false, HalfPacket(4), 5, 0, 4, RemovedInDelivery(), missDice: new[] { 4, 4 }));
            Assertions.True(!Pass(wrongCount, UpCloseAndDeadlyCaseVerdict.Damage), "A roll with the wrong number of dice must fail: " + wrongCount);
        }

        internal static void ConsumptionNeedsRemovalInsideDelivery()
        {
            JObject inside = Judge(Expect("hit", "full", 1, true), Observed(true, false, FullPacket(4), 5, 0, 4, RemovedInDelivery()));
            Assertions.True(Pass(inside, UpCloseAndDeadlyCaseVerdict.Consumption), "Removal inside delivery before expiry must pass: " + inside);

            // Retained through delivery, then gone only by natural expiry: a
            // later read finds no buff, which must not count.
            JObject expired = RemovedInDelivery();
            expired["removalSequence"] = 9;
            expired["removalGameSeconds"] = 106.0;
            JObject expiry = Judge(Expect("hit", "full", 1, true), Observed(true, false, FullPacket(4), 5, 0, 4, expired));
            Assertions.True(!Pass(expiry, UpCloseAndDeadlyCaseVerdict.Consumption), "Natural expiry after delivery must fail consumption: " + expiry);

            // Removed after the deed's damage event: outside the handler's order.
            JObject late = RemovedInDelivery();
            late["removalSequence"] = 3;
            late["deedDamageSequence"] = 2;
            Assertions.True(!Pass(Judge(Expect("hit", "full", 1, true), Observed(true, false, FullPacket(4), 5, 0, 4, late)), UpCloseAndDeadlyCaseVerdict.Consumption),
                "Removal after the deed's damage event must fail consumption.");

            // Another instance was removed; the exact one was not seen to go.
            JObject other = RemovedInDelivery();
            other["removalSequence"] = JValue.CreateNull();
            other["removalGameSeconds"] = JValue.CreateNull();
            other["otherInstanceRemovals"] = 1;
            Assertions.True(!Pass(Judge(Expect("hit", "full", 1, true), Observed(true, false, FullPacket(4), 5, 0, 4, other)), UpCloseAndDeadlyCaseVerdict.Consumption),
                "Another instance's removal must not pass consumption.");

            foreach (string key in new[] { "identified", "deliveryStartSequence", "removalGameSeconds", "expiryDeadlineGameSeconds" })
            {
                JObject partial = RemovedInDelivery();
                partial.Remove(key);
                Assertions.True(!Pass(Judge(Expect("hit", "full", 1, true), Observed(true, false, FullPacket(4), 5, 0, 4, partial)), UpCloseAndDeadlyCaseVerdict.Consumption),
                    "Missing " + key + " must fail consumption.");
            }
            JObject unbounded = RemovedInDelivery();
            unbounded.Remove("deedDamageSequence");
            unbounded.Remove("deliveryEndSequence");
            Assertions.True(!Pass(Judge(Expect("hit", "full", 1, true), Observed(true, false, FullPacket(4), 5, 0, 4, unbounded)), UpCloseAndDeadlyCaseVerdict.Consumption),
                "A removal with no bound on the delivery window must fail consumption.");
        }

        internal static void ObservedMissIsNotAHit()
        {
            JObject verdict = Judge(Expect("hit", "full", 1, true), Observed(false, false, HalfPacket(3), 5, 0, 4, RemovedInDelivery(), missDice: new[] { 6 }));
            Assertions.True(!Pass(verdict, UpCloseAndDeadlyCaseVerdict.Outcome), "An observed miss must fail a hit outcome.");
            Assertions.True(!Pass(verdict, UpCloseAndDeadlyCaseVerdict.Damage), "A half packet must fail full damage.");
            JObject flagged = Judge(Expect("critical", "full", 1, true),
                new JObject(Observed(true, true, FullPacket(4), 5, 0, 4, RemovedInDelivery())) { ["criticalSource"] = "set-after-resolution" });
            Assertions.True(!Pass(flagged, UpCloseAndDeadlyCaseVerdict.Outcome),
                "A critical flag set after resolution must not count as an engine critical.");
        }

        internal static void RecoveryIsSeparatedFromTheCharge()
        {
            string actor = UpCloseAndDeadlyCaseVerdict.RecoveryOfTheActor;
            JObject offset = Judge(Expect("critical", "full", 1, true), Observed(true, true, FullPacket(4), 3, 1, 3, RemovedInDelivery(), actor));
            Assertions.True(Pass(offset, UpCloseAndDeadlyCaseVerdict.Cost), "Attributed recovery offsetting a correct charge must pass: " + offset);
            JObject concealed = Judge(Expect("critical", "full", 1, true), Observed(true, true, FullPacket(4), 3, 1, 4, RemovedInDelivery(), actor));
            Assertions.True(!Pass(concealed, UpCloseAndDeadlyCaseVerdict.Cost), "Recovery must not conceal a missing charge.");
            JObject capped = Judge(Expect("critical", "full", 1, true), Observed(true, true, FullPacket(4), 5, 0, 4, RemovedInDelivery(), actor));
            Assertions.True(Pass(capped, UpCloseAndDeadlyCaseVerdict.Cost), "A capped recovery must not fail a correct charge: " + capped);
            JObject none = Judge(Expect("hit", "full", 1, true), Observed(true, false, FullPacket(4), 5, 0, 4, RemovedInDelivery()));
            Assertions.True(Pass(none, UpCloseAndDeadlyCaseVerdict.Cost), "No recovery anywhere is a valid zero: " + none);

            // Another unit's recovery, read from the global diagnostics, would
            // make this actor's missing charge (3 -> 3) look like 3 + 1 - 1.
            JObject foreign = Judge(Expect("critical", "full", 1, true), Observed(true, true, FullPacket(4), 3, 1, 3, RemovedInDelivery(), "unattributed"));
            Assertions.True(!Pass(foreign, UpCloseAndDeadlyCaseVerdict.Cost), "Unattributed recovery must not make a missing charge pass: " + foreign);
            JObject claimedZero = Judge(Expect("hit", "full", 1, true), Observed(true, false, FullPacket(4), 5, 1, 5, RemovedInDelivery()));
            Assertions.True(!Pass(claimedZero, UpCloseAndDeadlyCaseVerdict.Cost), "'None applied' with a nonzero amount must fail.");
        }

        internal static void CancellationNeedsAnAdmittedInterruptedOrder()
        {
            var correct = new JObject
            {
                ["orderTakenByTheUnit"] = true, ["orderStarted"] = false, ["orderActed"] = false, ["orderResult"] = "Interrupt",
                ["attackRollsResolved"] = 0, ["deedPacketsFromShooterInWindow"] = 0, ["armedInstanceRetainedAfterCancel"] = true,
                ["gritBeforeOrder"] = 1, ["gritAfterCancel"] = 1, ["gritAfterExpiry"] = 1, ["endedAfterGameSeconds"] = 6.03
            };
            JObject verdict = UpCloseAndDeadlyCaseVerdict.EvaluateCancellation(correct);
            Assertions.True((bool)verdict["pass"], "An admitted, interrupted order must pass: " + verdict);

            // Never admitted: no roll, no damage and no grit change, and still an unmet prerequisite.
            JObject rejected = UpCloseAndDeadlyCaseVerdict.EvaluateCancellation(new JObject(correct) { ["orderTakenByTheUnit"] = false });
            Assertions.True(!(bool)rejected["prerequisite"]["pass"] && !(bool)rejected["pass"], "A never-admitted order must fail: " + rejected);
            Assertions.True((bool)rejected["noShot"]["pass"] && (bool)rejected["cost"]["pass"],
                "The rejected order's quiet outcome is still recorded separately.");
            foreach (var change in new[] { new JProperty("orderStarted", true), new JProperty("orderActed", true), new JProperty("orderResult", "Success") })
            {
                var copy = new JObject(correct);
                copy[change.Name] = change.Value;
                Assertions.True(!(bool)UpCloseAndDeadlyCaseVerdict.EvaluateCancellation(copy)["prerequisite"]["pass"],
                    change.Name + "=" + change.Value + " must fail the prerequisite.");
            }
            var missing = new JObject(correct);
            missing.Remove("orderResult");
            Assertions.True(!(bool)UpCloseAndDeadlyCaseVerdict.EvaluateCancellation(missing)["pass"], "A missing order result must fail.");
            Assertions.True(!(bool)UpCloseAndDeadlyCaseVerdict.EvaluateCancellation(new JObject(correct) { ["endedAfterGameSeconds"] = 12.0 })["expiry"]["pass"],
                "A retained state that outlived its round must fail the expiry check.");
            string json = UpCloseAndDeadlyCaseVerdict.EvaluateCancellationJson(correct.ToString());
            Assertions.True((bool)JObject.Parse(json)["pass"], "The reflection entry must agree with EvaluateCancellation.");
        }

        internal static void MissingObservationsFail()
        {
            JObject verdict = Judge(Expect("hit", "none", 0, false), new JObject());
            foreach (string part in new[] { UpCloseAndDeadlyCaseVerdict.Outcome, UpCloseAndDeadlyCaseVerdict.Damage,
                UpCloseAndDeadlyCaseVerdict.Cost, UpCloseAndDeadlyCaseVerdict.Consumption })
                Assertions.True(!Pass(verdict, part), "A missing observation must fail " + part + ", not read as zero: " + verdict);
            JObject noRecovery = Judge(Expect("hit", "full", 1, true),
                new JObject(Observed(true, false, FullPacket(4), 5, 0, 4, RemovedInDelivery())) { ["gritRestored"] = JValue.CreateNull() });
            Assertions.True(!Pass(noRecovery, UpCloseAndDeadlyCaseVerdict.Cost), "Unreadable recovery must fail the cost.");
            string json = UpCloseAndDeadlyCaseVerdict.EvaluateJson(Expect("hit", "full", 1, true).ToString(),
                Observed(true, false, FullPacket(4), 5, 0, 4, RemovedInDelivery()).ToString());
            Assertions.True((bool)JObject.Parse(json)["pass"], "The reflection entry must agree with Evaluate.");
            var unarmed = new JObject(Expect("hit", "none", 0, false)) { ["armed"] = false };
            Assertions.True(Pass(Judge(unarmed, Observed(true, false, null, 1, 0, 1, new JObject { ["identified"] = false })), UpCloseAndDeadlyCaseVerdict.Consumption),
                "An unarmed shot with no instance passes consumption.");
            Assertions.True(!Pass(Judge(unarmed, Observed(true, false, null, 1, 0, 1, null)), UpCloseAndDeadlyCaseVerdict.Consumption),
                "An unarmed shot with no instance evidence must fail consumption.");
        }
    }
}
