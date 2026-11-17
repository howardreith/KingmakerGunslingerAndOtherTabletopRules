using KingmakerGunslinger.RuntimeTesting;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.DomainTests
{
    /// <summary>
    /// REP-02 / S4 T2: the Up Close and Deadly case evaluator keeps outcome,
    /// damage, cost and consumption apart, and nothing missing passes.
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

        private static JObject Observed(bool hit, bool critical, JObject packet, int before, int restored, int after, bool removed)
        {
            return new JObject
            {
                ["hit"] = hit, ["criticalConfirmed"] = critical, ["criticalSource"] = critical ? "engine" : "none",
                ["completedNonMisfire"] = true,
                ["deedPackets"] = packet == null ? new JArray() : new JArray(packet),
                ["gritBefore"] = before, ["gritRestored"] = restored, ["gritAfter"] = after,
                ["armedInstanceRemoved"] = removed
            };
        }

        private static bool Pass(JObject verdict, string part) => (bool)verdict[part]["pass"];

        internal static void CorrectCasesPass()
        {
            JObject verdict = UpCloseAndDeadlyCaseVerdict.Evaluate(Expect("hit", "full", 1, true),
                Observed(true, false, FullPacket(4), 5, 0, 4, true));
            Assertions.True((bool)verdict["pass"], "A correct ordinary hit must pass: " + verdict);
            verdict = UpCloseAndDeadlyCaseVerdict.Evaluate(Expect("miss", "half", 1, true),
                Observed(false, false, HalfPacket(2), 5, 0, 4, true));
            Assertions.True((bool)verdict["pass"], "A correct miss must pass: " + verdict);
        }

        internal static void FailedConsumptionFailsOnlyConsumption()
        {
            JObject verdict = UpCloseAndDeadlyCaseVerdict.Evaluate(Expect("hit", "full", 1, true),
                Observed(true, false, FullPacket(4), 5, 0, 4, false));
            Assertions.True(!Pass(verdict, UpCloseAndDeadlyCaseVerdict.Consumption), "A retained state must fail consumption.");
            Assertions.True(Pass(verdict, UpCloseAndDeadlyCaseVerdict.Damage) && Pass(verdict, UpCloseAndDeadlyCaseVerdict.Cost),
                "Correct damage and cost must still pass beside a failed consumption: " + verdict);
            Assertions.True(!(bool)verdict["pass"], "The aggregate must fail.");
        }

        internal static void WrongDamageFailsOnlyDamage()
        {
            JObject multiplied = UpCloseAndDeadlyCaseVerdict.Evaluate(Expect("critical", "full", 1, true),
                Observed(true, true, FullPacket(4, 4), 5, 0, 4, true));
            Assertions.True(!Pass(multiplied, UpCloseAndDeadlyCaseVerdict.Damage), "A multiplied deed packet must fail damage.");
            Assertions.True(Pass(multiplied, UpCloseAndDeadlyCaseVerdict.Consumption) && Pass(multiplied, UpCloseAndDeadlyCaseVerdict.Cost),
                "Correct consumption and cost must still pass beside wrong damage: " + multiplied);
            JObject doubled = UpCloseAndDeadlyCaseVerdict.Evaluate(Expect("hit", "full", 1, true),
                new JObject(Observed(true, false, FullPacket(4), 5, 0, 4, true)) { ["deedPackets"] = new JArray(FullPacket(4), FullPacket(3)) });
            Assertions.True(!Pass(doubled, UpCloseAndDeadlyCaseVerdict.Damage), "Two deed packets must fail damage.");
        }

        internal static void ObservedMissIsNotAHit()
        {
            // The case is named and expected as a hit; the roll missed and the
            // half packet was applied. Neither the name nor the expectation
            // makes it a hit.
            JObject verdict = UpCloseAndDeadlyCaseVerdict.Evaluate(Expect("hit", "full", 1, true),
                Observed(false, false, HalfPacket(3), 5, 0, 4, true));
            Assertions.True(!Pass(verdict, UpCloseAndDeadlyCaseVerdict.Outcome), "An observed miss must fail a hit outcome.");
            Assertions.True(!Pass(verdict, UpCloseAndDeadlyCaseVerdict.Damage), "A half packet must fail full damage.");
            JObject flagged = UpCloseAndDeadlyCaseVerdict.Evaluate(Expect("critical", "full", 1, true),
                new JObject(Observed(true, true, FullPacket(4), 5, 0, 4, true)) { ["criticalSource"] = "set-after-resolution" });
            Assertions.True(!Pass(flagged, UpCloseAndDeadlyCaseVerdict.Outcome),
                "A critical flag set after resolution must not count as an engine critical.");
        }

        internal static void RecoveryIsSeparatedFromTheCharge()
        {
            // Critical recovery restored one and the deed charged one: net 0, correct.
            JObject offset = UpCloseAndDeadlyCaseVerdict.Evaluate(Expect("critical", "full", 1, true),
                Observed(true, true, FullPacket(4), 3, 1, 3, true));
            Assertions.True(Pass(offset, UpCloseAndDeadlyCaseVerdict.Cost), "Recovery offsetting a correct charge must pass: " + offset);
            // Recovery restored one and the deed charged nothing: net +1, a missing charge.
            JObject concealed = UpCloseAndDeadlyCaseVerdict.Evaluate(Expect("critical", "full", 1, true),
                Observed(true, true, FullPacket(4), 3, 1, 4, true));
            Assertions.True(!Pass(concealed, UpCloseAndDeadlyCaseVerdict.Cost), "Recovery must not conceal a missing charge.");
            // At the cap the recovery restored nothing: the correct charge reads as -1 net.
            JObject capped = UpCloseAndDeadlyCaseVerdict.Evaluate(Expect("critical", "full", 1, true),
                Observed(true, true, FullPacket(4), 5, 0, 4, true));
            Assertions.True(Pass(capped, UpCloseAndDeadlyCaseVerdict.Cost), "A capped recovery must not fail a correct charge: " + capped);
        }

        internal static void MissingObservationsFail()
        {
            var empty = new JObject();
            JObject verdict = UpCloseAndDeadlyCaseVerdict.Evaluate(Expect("hit", "none", 0, false), empty);
            foreach (string part in new[] { UpCloseAndDeadlyCaseVerdict.Outcome, UpCloseAndDeadlyCaseVerdict.Damage,
                UpCloseAndDeadlyCaseVerdict.Cost, UpCloseAndDeadlyCaseVerdict.Consumption })
                Assertions.True(!Pass(verdict, part), "A missing observation must fail " + part + ", not read as zero: " + verdict);
            JObject noRecovery = UpCloseAndDeadlyCaseVerdict.Evaluate(Expect("hit", "full", 1, true),
                new JObject(Observed(true, false, FullPacket(4), 5, 0, 4, true)) { ["gritRestored"] = JValue.CreateNull() });
            Assertions.True(!Pass(noRecovery, UpCloseAndDeadlyCaseVerdict.Cost), "Unreadable recovery must fail the cost.");
            string json = UpCloseAndDeadlyCaseVerdict.EvaluateJson(Expect("hit", "full", 1, true).ToString(),
                Observed(true, false, FullPacket(4), 5, 0, 4, true).ToString());
            Assertions.True((bool)JObject.Parse(json)["pass"], "The reflection entry must agree with Evaluate.");
        }
    }
}
