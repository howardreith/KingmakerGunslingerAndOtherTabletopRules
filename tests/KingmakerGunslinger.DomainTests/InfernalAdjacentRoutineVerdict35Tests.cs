using System;
using System.Linq;
using KingmakerGunslinger.RuntimeTesting;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.DomainTests
{
    // Sprint 3 adjacent natural-attack acceptance. The records mirror the
    // observer's receipt; the plan and command-end rows come from the native
    // event trace (s3a/s3b raw traces have these exact shapes).
    internal static class InfernalAdjacentRoutineVerdict35Tests
    {
        private const string Bite = "a54ab76372484c088cfe028fe7854694";
        private const string Claw = "d0f2530de475451a815b1861c618776d";
        private const string Sting = "e341953286ac453a8adc1747671e025d";
        private const string Owner = "bone-source-a";
        private const string Target = "fixture-hostile";
        private const int Reference = 490795008;

        private static JObject Attack(string weapon, bool hit)
        {
            return new JObject { ["owner"] = Owner, ["target"] = Target, ["attackCommand"] = Reference,
                ["weapon"] = weapon, ["hit"] = hit };
        }

        private static JObject End(int index, bool planned, string caller)
        {
            return new JObject { ["owner"] = Owner, ["target"] = Target, ["commandReference"] = Reference,
                ["attackIndex"] = index, ["plannedAttackPresent"] = planned, ["interruptionCaller"] = caller };
        }

        // A complete routine ending Success (the s3a shape), with all
        // prerequisites the corrected observer records.
        private static JObject Complete()
        {
            return new JObject {
                ["owner"] = Owner, ["target"] = Target, ["commandReference"] = Reference,
                ["profile"] = new JObject { ["bite"] = Bite, ["claw"] = Claw, ["sting"] = Sting },
                ["plan"] = new JArray(Bite, Claw, Claw, Sting),
                ["attacks"] = new JArray(Attack(Bite, true), Attack(Claw, true), Attack(Claw, true), Attack(Sting, true)),
                ["commandEnds"] = new JArray(End(4, false, null)),
                ["commandFinished"] = true, ["commandResult"] = "Success",
                ["ownerLifetimePresentAfter"] = true, ["ownerAliveAfter"] = true,
                ["properFlanking2"] = new JObject { ["assemblyLoaded"] = true, ["modActive"] = true,
                    ["reachWeaponsDeadZone"] = true, ["getTargetPointPatched"] = true, ["isReachPatched"] = true },
                ["insideDeadZone"] = true, ["threatens"] = true, ["ownerTravel"] = 0d, ["targetTravel"] = 0d };
        }

        private static void Fails(JObject record, string expected, string message)
        {
            Assertions.Equal(expected, InfernalAdjacentRoutineVerdict35.Failure(record), message);
        }

        internal static void CompleteRoutinesPass()
        {
            Assertions.Equal<string>(null, InfernalAdjacentRoutineVerdict35.Failure(Complete()),
                "A complete four-slot routine ending Success passes.");
            var exhausted = Complete();
            exhausted["commandResult"] = "Interrupt";
            exhausted["commandEnds"] = new JArray(End(4, false,
                "  at Kingmaker.UnitLogic.Commands.UnitAttack.OnTick () [0x00000]"));
            Assertions.Equal<string>(null, InfernalAdjacentRoutineVerdict35.Failure(exhausted),
                "The installed stack's exhausted-routine Interrupt (final index, no next attack, UnitAttack.OnTick) passes.");
            // Slot order is the native plan's business; the verdict compares
            // the multiset of resolved weapons with the plan.
            var reordered = Complete();
            reordered["plan"] = new JArray(Claw, Bite, Sting, Claw);
            Assertions.Equal<string>(null, InfernalAdjacentRoutineVerdict35.Failure(reordered),
                "The four planned slots pass in any native order.");
        }

        internal static void MissesAreResolvedAttacks()
        {
            var misses = Complete();
            misses["attacks"] = new JArray(Attack(Bite, false), Attack(Claw, true), Attack(Claw, false), Attack(Sting, false));
            Assertions.Equal<string>(null, InfernalAdjacentRoutineVerdict35.Failure(misses),
                "A miss is a resolved attack: three misses and one hit still complete the routine.");
            var allMiss = Complete();
            foreach (JObject row in ((JArray)allMiss["attacks"]).OfType<JObject>()) row["hit"] = false;
            Assertions.Equal<string>(null, InfernalAdjacentRoutineVerdict35.Failure(allMiss),
                "No hit is required.");
        }

        internal static void PartialOrLifetimeInterruptedRoutinesFail()
        {
            // s3b: bite and one claw, then the command ended Interrupt at
            // index 2 with the next attack still planned, the Bone Devil's
            // summon lifetime having expired.
            var s3b = Complete();
            s3b["attacks"] = new JArray(Attack(Bite, true), Attack(Claw, true));
            s3b["commandResult"] = "Interrupt";
            s3b["commandEnds"] = new JArray(End(2, true, "  at Kingmaker.UnitLogic.Buffs.Buff.OnDetach ()"));
            s3b["ownerLifetimePresentAfter"] = false;
            Fails(s3b, "planned-slot-unresolved", "The s3b partial routine fails.");
            var interruptedLate = Complete();
            interruptedLate["commandResult"] = "Interrupt";
            interruptedLate["commandEnds"] = new JArray(End(4, false, "  at Kingmaker.UnitLogic.Buffs.Buff.OnDetach ()"));
            Fails(interruptedLate, "interrupted-without-native-exhaustion",
                "An interrupt raised elsewhere fails even after four attacks.");
            var stillPlanned = Complete();
            stillPlanned["commandResult"] = "Interrupt";
            stillPlanned["commandEnds"] = new JArray(End(4, true, "  at Kingmaker.UnitLogic.Commands.UnitAttack.OnTick ()"));
            Fails(stillPlanned, "interrupted-without-native-exhaustion", "A next planned attack means the routine was not exhausted.");
            var otherCommand = Complete();
            otherCommand["commandResult"] = "Interrupt";
            var foreign = End(4, false, "  at Kingmaker.UnitLogic.Commands.UnitAttack.OnTick ()");
            foreign["commandReference"] = Reference + 1;
            otherCommand["commandEnds"] = new JArray(foreign);
            Fails(otherCommand, "interrupted-without-native-exhaustion",
                "Exhaustion evidence from another command does not count.");
            var foreignActor = Complete();
            foreignActor["commandResult"] = "Interrupt";
            var actorEnd = End(4, false, "  at Kingmaker.UnitLogic.Commands.UnitAttack.OnTick ()");
            actorEnd["owner"] = "another-unit";
            foreignActor["commandEnds"] = new JArray(actorEnd);
            Fails(foreignActor, "interrupted-without-native-exhaustion",
                "Exhaustion evidence from another actor does not count.");
            var lifetimeLost = Complete();
            lifetimeLost["ownerLifetimePresentAfter"] = false;
            Fails(lifetimeLost, "attacker-lifetime-lost", "Premature lifetime loss fails a complete routine.");
            var unfinished = Complete();
            unfinished["commandFinished"] = false;
            Fails(unfinished, "command-not-finished", "An unfinished command fails.");
            var cancelled = Complete();
            cancelled["commandResult"] = "Fail";
            Fails(cancelled, "command-result-Fail", "Any other native result fails.");
        }

        internal static void MissingRolesOrEvidenceFail()
        {
            var noSting = Complete();
            noSting["profile"]["sting"] = null;
            Fails(noSting, "profile-weapon-missing", "A missing profile weapon fails.");
            var noProfile = Complete();
            noProfile.Remove("profile");
            Fails(noProfile, "profile-weapon-missing", "A missing profile fails.");
            var noPlan = Complete();
            noPlan["plan"] = new JArray();
            Fails(noPlan, "no-native-plan", "A truncated trace without the native plan fails.");
            var threeSlots = Complete();
            threeSlots["plan"] = new JArray(Bite, Claw, Claw);
            Fails(threeSlots, "plan-not-bite-two-claws-sting", "A plan missing the sting fails.");
            var oneClaw = Complete();
            oneClaw["plan"] = new JArray(Bite, Claw, Sting, Sting);
            Fails(oneClaw, "plan-not-bite-two-claws-sting", "A plan with one claw fails.");
            var wrongWeapon = Complete();
            wrongWeapon["attacks"] = new JArray(Attack(Bite, true), Attack(Claw, true), Attack(Bite, true), Attack(Sting, true));
            Fails(wrongWeapon, "resolved-slots-differ-from-plan", "Resolved weapons must match the planned slots.");
            var truncated = Complete();
            truncated["attacks"] = new JArray(Attack(Bite, true), Attack(Claw, false), Attack(Claw, true));
            Fails(truncated, "planned-slot-unresolved", "A missing attack row fails.");
            var extra = Complete();
            ((JArray)extra["attacks"]).Add(Attack(Bite, true));
            Fails(extra, "extra-attack", "An extra attack under the command fails.");
            var wrongTarget = Complete();
            ((JArray)wrongTarget["attacks"])[3]["target"] = "another-unit";
            Fails(wrongTarget, "attack-outside-measured-command", "Every slot must resolve against the exact target.");
            var wrongCommand = Complete();
            ((JArray)wrongCommand["attacks"])[1]["attackCommand"] = Reference + 1;
            Fails(wrongCommand, "attack-outside-measured-command", "Every slot must resolve under the exact command.");
            var noIdentity = Complete();
            noIdentity.Remove("commandReference");
            Fails(noIdentity, "command-identity-missing", "The command identity is required.");
            var noEnds = Complete();
            noEnds["commandResult"] = "Interrupt";
            noEnds["commandEnds"] = new JArray();
            Fails(noEnds, "interrupted-without-native-exhaustion", "An interrupt without completion evidence fails.");
        }

        internal static void ProperFlanking2AndGeometryPreconditionsFail()
        {
            var absent = Complete();
            absent["properFlanking2"]["assemblyLoaded"] = false;
            Fails(absent, "properflanking2-assembly-absent", "ProperFlanking2 must be loaded.");
            var inactive = Complete();
            inactive["properFlanking2"]["modActive"] = false;
            Fails(inactive, "properflanking2-mod-inactive", "A loaded but inactive mod is not the dead zone.");
            var disabled = Complete();
            disabled["properFlanking2"]["reachWeaponsDeadZone"] = false;
            Fails(disabled, "properflanking2-dead-zone-not-enabled", "Assembly presence alone is not the dead zone.");
            var unread = Complete();
            unread["properFlanking2"]["reachWeaponsDeadZone"] = null;
            Fails(unread, "properflanking2-dead-zone-not-enabled", "An unread setting is not a pass.");
            var unpatched = Complete();
            unpatched["properFlanking2"]["isReachPatched"] = false;
            Fails(unpatched, "properflanking2-dead-zone-patch-absent", "Both ProperFlanking2 postfixes must be installed.");
            var outside = Complete();
            outside["insideDeadZone"] = false;
            Fails(outside, "target-outside-dead-zone", "The target must stand inside the dead zone.");
            var notThreatened = Complete();
            notThreatened["threatens"] = false;
            Fails(notThreatened, "target-not-threatened", "The target must be threatened.");
            var backedOff = Complete();
            backedOff["ownerTravel"] = 1.2d;
            Fails(backedOff, "unit-moved", "The Bone Devil must not back off.");
            var untracked = Complete();
            untracked.Remove("targetTravel");
            Fails(untracked, "unit-moved", "Unrecorded travel fails.");
        }
    }
}
