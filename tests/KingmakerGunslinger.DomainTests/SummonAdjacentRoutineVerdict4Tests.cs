using KingmakerGunslinger.RuntimeTesting;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.DomainTests
{
    // Sprint 4 generalized adjacent-routine verdict: the expected routine is
    // data (here a salamander: two spear iteratives plus its tail), not the
    // Bone Devil's four slots.
    internal static class SummonAdjacentRoutineVerdict4Tests
    {
        private const string Spear = "99394d453c6f425f84d4b92f7a8deea0", Tail = "93e097b8d3db42d3a37656502899e1a9";
        private const string Owner = "salamander-unit", Target = "fixture-hostile";
        private const int Reference = 1234;

        private static JObject Attack(string weapon, bool hit)
        {
            return new JObject { ["owner"] = Owner, ["target"] = Target, ["attackCommand"] = Reference, ["weapon"] = weapon, ["hit"] = hit };
        }

        private static JObject Complete()
        {
            return new JObject {
                ["owner"] = Owner, ["target"] = Target, ["commandReference"] = Reference,
                ["expected"] = new JArray(Spear, Spear, Tail), ["plan"] = new JArray(Spear, Spear, Tail),
                ["attacks"] = new JArray(Attack(Spear, true), Attack(Spear, false), Attack(Tail, true)),
                ["commandEnds"] = new JArray(new JObject { ["owner"] = Owner, ["commandReference"] = Reference,
                    ["attackIndex"] = 3, ["plannedAttackPresent"] = false, ["interruptionCaller"] = null }),
                ["commandFinished"] = true, ["commandResult"] = "Success",
                ["ownerLifetimePresentAfter"] = true, ["ownerAliveAfter"] = true,
                ["properFlanking2"] = new JObject { ["assemblyLoaded"] = true, ["modActive"] = true,
                    ["reachWeaponsDeadZone"] = true, ["getTargetPointPatched"] = true, ["isReachPatched"] = true },
                ["edgeGap"] = .15d, ["threatens"] = true, ["ownerTravel"] = 0d, ["targetTravel"] = 0d };
        }

        private static void Fails(JObject record, string expected, string message)
        {
            Assertions.Equal(expected, SummonAdjacentRoutineVerdict4.Failure(record), message);
        }

        internal static void Cases()
        {
            Assertions.Equal<string>(null, SummonAdjacentRoutineVerdict4.Failure(Complete()),
                "A complete expected routine passes; a miss counts as a resolved attack.");
            var exhausted = Complete();
            exhausted["commandResult"] = "Interrupt";
            exhausted["commandEnds"][0]["interruptionCaller"] = "  at Kingmaker.UnitLogic.Commands.UnitAttack.OnTick ()";
            Assertions.Equal<string>(null, SummonAdjacentRoutineVerdict4.Failure(exhausted), "The exhausted-routine Interrupt passes.");

            // s4d Salamander: the final tail hit grabbed, the grapple made the
            // attacker unable to act, and UnitActionController interrupted the
            // exhausted command in the same instant.
            var grab = Complete();
            grab["commandResult"] = "Interrupt";
            grab["commandEnds"][0]["interruptionCaller"] = "  at Kingmaker.Controllers.Units.UnitActionController.TickCommand ()";
            grab["terminalGrab"] = new JObject { ["attackerHoldBefore"] = false, ["attackerHoldAfter"] = true,
                ["targetGrappledBefore"] = false, ["targetGrappledAfter"] = true, ["endAtFinalAct"] = true,
                ["finalAttackHit"] = true, ["finalAttackWeapon"] = Tail };
            Assertions.Equal<string>(null, SummonAdjacentRoutineVerdict4.Failure(grab),
                "A terminal grab after every slot resolved is a native completion boundary.");
            foreach (string missing in new[] { "attackerHoldAfter", "targetGrappledAfter", "endAtFinalAct", "finalAttackHit" })
            {
                var weak = (JObject)grab.DeepClone();
                weak["terminalGrab"][missing] = false;
                Fails(weak, "interrupted-without-native-exhaustion", "A terminal grab without " + missing + " fails.");
            }
            var preexisting = (JObject)grab.DeepClone();
            preexisting["terminalGrab"]["attackerHoldBefore"] = true;
            Fails(preexisting, "interrupted-without-native-exhaustion", "A grapple held before the command is not this routine's grab.");
            var otherWeapon = (JObject)grab.DeepClone();
            otherWeapon["terminalGrab"]["finalAttackWeapon"] = "unexpected";
            Fails(otherWeapon, "interrupted-without-native-exhaustion", "The grabbing hit must be an expected slot.");
            var earlyGrab = (JObject)grab.DeepClone();
            earlyGrab["commandEnds"][0]["attackIndex"] = 2; earlyGrab["commandEnds"][0]["plannedAttackPresent"] = true;
            Fails(earlyGrab, "interrupted-without-native-exhaustion", "A grab before the routine is exhausted cannot pass.");

            var partial = Complete();
            partial["attacks"] = new JArray(Attack(Spear, true), Attack(Tail, true));
            Fails(partial, "planned-slot-unresolved", "A partial routine fails even though it hit.");
            var withoutTail = Complete();
            withoutTail["plan"] = new JArray(Spear, Spear);
            Fails(withoutTail, "plan-differs-from-expected-routine", "Dropping the expected tail cannot pass.");
            var noExpected = Complete();
            noExpected["expected"] = new JArray();
            Fails(noExpected, "expected-routine-missing", "An unspecified expected routine fails.");
            var foreignInterrupt = Complete();
            foreignInterrupt["commandResult"] = "Interrupt";
            foreignInterrupt["commandEnds"][0]["interruptionCaller"] = "  at Kingmaker.UnitLogic.Buffs.Buff.OnDetach ()";
            Fails(foreignInterrupt, "interrupted-without-native-exhaustion", "An unrelated interruption fails.");
            var lifetime = Complete();
            lifetime["ownerLifetimePresentAfter"] = false;
            Fails(lifetime, "attacker-lifetime-lost", "Lifetime loss fails.");
            var wrongTarget = Complete();
            wrongTarget["attacks"][2]["target"] = "another";
            Fails(wrongTarget, "attack-outside-measured-command", "Every attack is tied to the exact target.");
            var disabled = Complete();
            disabled["properFlanking2"]["reachWeaponsDeadZone"] = false;
            Fails(disabled, "properflanking2-dead-zone-not-enabled", "ProperFlanking2 must be enabled.");
            var far = Complete();
            far["edgeGap"] = 1.2d;
            Fails(far, "target-not-adjacent", "The target must stand adjacent.");
            var moved = Complete();
            moved["ownerTravel"] = .6d;
            Fails(moved, "unit-moved", "The attacker must not relocate.");
        }
    }
}
