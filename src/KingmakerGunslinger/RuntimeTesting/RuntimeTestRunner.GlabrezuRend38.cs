using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kingmaker;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        /// <summary>
        /// The printed rend, observed on a live unit with real attack rules.
        ///
        /// <para>Every cell drives the engine's own
        /// <see cref="RuleAttackWithWeapon"/> against units this fixture owns,
        /// under a real live attack command, and reads the rend flag the gate
        /// actually set. Nothing is simulated: the production component, the
        /// production round state and the production limb classification are
        /// what answer.</para>
        ///
        /// <para>The round boundary is read rather than invented. The gate's
        /// own per-unit state is sampled through its released fixture
        /// accessor, so a boundary is observed when the real
        /// <c>ITickEachRound</c> callback clears it - not when a clock this
        /// observer keeps says a round should have passed.</para>
        /// </summary>
        private IEnumerable<int> ReviewGlabrezuRend38(
            ExpandedSummoningCorrectionFixture fixture)
        {
            var owned = new List<UnitEntityData>();
            var cells = new JArray();
            var receipt = new JObject {
                ["phase"] = ExpandedSummoningNativeProfilePolicy
                    .Sprint38GlabrezuRendPhase,
                ["finalQualification"] = false,
                ["cells"] = cells
            };
            UnitEntityData target = fixture.Hostile;
            int oldTargetAc = target.Stats.AC.BaseValue;
            int oldTargetHp = target.Stats.HitPoints.BaseValue;
            int oldTargetDamage = target.Damage;
            Vector3 oldTargetPosition = target.Position;
            // This phase pauses the engine to own one round; the caller's own
            // pause state is restored before the next review runs.
            bool oldPaused = Game.Instance.IsPaused;
            Action<string, bool, JObject, string> check =
                (id, pass, row, description) => {
                    row["cell"] = id;
                    cells.Add(row);
                    Sprint25Check("sprint38-" + id, pass, row, description);
                    if (!pass) throw new InvalidOperationException(
                        "Sprint38 glabrezu rend cell failed: " + id);
                };

            try
            {
                UnitEntityData glabrezu = CastExpandedSummoningOwnTier(
                    fixture, GlabrezuVisualPolicy38.Glabrezu);
                owned.Add(glabrezu);
                VrockQuiesce37(glabrezu);
                foreach (int frame in Outsider30NativeAppearance(glabrezu,
                    glabrezu.Blueprint.name)) yield return frame;

                // A second victim this fixture owns, so the per-victim
                // bookkeeping can be driven with two real opponents rather
                // than asserted about one. It is another glabrezu for the same
                // reason the persistence scope takes two: this fixture may not
                // write to a party member or any unit it does not own.
                UnitEntityData second = CastExpandedSummoningOwnTier(
                    fixture, GlabrezuVisualPolicy38.Glabrezu);
                owned.Add(second);
                VrockQuiesce37(second);

                // Nothing may die mid-sequence and every attack must land, so
                // both victims are made unkillable and trivially hittable.
                // These are fixture-owned units, and the hostile's own values
                // are restored in the finally block below.
                foreach (UnitEntityData victim in new[] { target, second })
                {
                    victim.Stats.AC.BaseValue = -100;
                    victim.Stats.HitPoints.BaseValue = 100000;
                    victim.Damage = 0;
                }
                PlaceExpandedSummoningUnit(target, glabrezu.Position +
                    Vector3.right *
                    (glabrezu.Corpulence + target.Corpulence + 0.5f));
                // Deliberately far. This is the live attack command's target,
                // and a distant one makes the owner walk rather than attack if
                // the engine is unpaused to cross a round boundary. Distance
                // is irrelevant to the rules this phase triggers directly.
                PlaceExpandedSummoningUnit(second, glabrezu.Position +
                    Vector3.left * 30f);

                ItemEntityWeapon[] pincers;
                ItemEntityWeapon[] others;
                GlabrezuRendLimbs38(glabrezu, out pincers, out others, check);

                JObject combat = GlabrezuEnterCombat38(glabrezu, target);
                check("rend-round-clock-is-live",
                    (bool)combat["ownerInCombat"] &&
                    (bool)combat["victimInCombat"] &&
                    !(bool)combat["ownerBrainActive"],
                    combat,
                    "The owner and its victim are in real combat, so the engine's own round clock runs and the printed once-per-round allowance can be observed being returned; no brain is activated, so nothing attacks on its own.");

                foreach (int frame in GlabrezuRendSequences38(glabrezu, target,
                    second, pincers, others, check)) yield return frame;
            }
            finally
            {
                Game.Instance.IsPaused = oldPaused;
                // The round state is request-local; a later phase in the
                // same transaction must not inherit this one's pairs.
                foreach (UnitEntityData unit in owned)
                    GlabrezuRendGate38.ForgetObserved(unit);
                target.Stats.AC.BaseValue = oldTargetAc;
                target.Stats.HitPoints.BaseValue = oldTargetHp;
                target.Damage = oldTargetDamage;
                PlaceExpandedSummoningUnit(target, oldTargetPosition);
                DisposeExpandedSummoningUnits(fixture.Created,
                    owned.ToArray());
                RuntimeTestResultWriter.WriteAtomic(Path.Combine(
                    _request.EvidenceDirectory, "sprint38-glabrezu-rend.json"),
                    receipt.ToString(Formatting.Indented));
            }
        }

        /// <summary>
        /// The two pincers and the three other limbs, told apart by the live
        /// weapon identities rather than by position, and in the limb order the
        /// production gate itself classifies.
        /// </summary>
        private static void GlabrezuRendLimbs38(UnitEntityData glabrezu,
            out ItemEntityWeapon[] pincers, out ItemEntityWeapon[] others,
            Action<string, bool, JObject, string> check)
        {
            ItemEntityWeapon primary = glabrezu.Body.PrimaryHand.MaybeWeapon;
            ItemEntityWeapon[] limbs = glabrezu.Body.AdditionalLimbs
                .Select(value => value.MaybeWeapon).ToArray();
            ItemEntityWeapon[] pincerLimbs = limbs.Where(value =>
                value != null && value.Blueprint.Type != null &&
                value.Blueprint.Type.CriticalRollEdge ==
                    GlabrezuRules38Policy.PincerCriticalRollEdge).ToArray();
            ItemEntityWeapon[] otherLimbs = limbs.Where(value =>
                    value != null && !pincerLimbs.Contains(value))
                .Concat(primary == null ?
                    Array.Empty<ItemEntityWeapon>() : new[] { primary })
                .ToArray();
            pincers = pincerLimbs;
            others = otherLimbs;
            check("rend-limbs",
                pincerLimbs.Length == GlabrezuRules38Policy.RendPincerCount &&
                otherLimbs.Length == 3 &&
                pincerLimbs.Select(value => value.Blueprint)
                    .Distinct().Count() == 1,
                new JObject {
                    ["owner"] = glabrezu.UniqueId,
                    ["pincerSlots"] = new JArray(pincerLimbs.Select(value =>
                        Array.IndexOf(limbs, value))),
                    ["pincerBlueprint"] = pincerLimbs.Length == 0 ? null :
                        pincerLimbs[0].Blueprint.name,
                    ["otherLimbBlueprints"] = new JArray(otherLimbs
                        .Select(value => value.Blueprint.name)),
                    ["sharedPincerIdentity"] = pincerLimbs
                        .Select(value => value.Blueprint).Distinct().Count(),
                    ["limbOrderMeaning"] =
                        "The two pincers share one weapon blueprint, so the production gate tells them apart by the additional-limb slot they occupy; the claws carry a different blueprint and the bite sits in the primary hand, so neither can reach the gate at all."
                }, "The live body offers exactly two pincers of one shared identity in the first two additional-limb slots, and three other limbs that the rend can never use.");
        }

        private sealed class GlabrezuRendRecord38
        {
            internal string Weapon;
            internal string Victim;
            internal bool Hit;
            internal bool Rend;
        }

        /// <summary>
        /// Triggers one real attack rule and reports what the gate did with
        /// it. A miss disarms and leaves the round's allowance intact, which is
        /// the production contract, so a missed attempt is simply repeated up
        /// to a bounded number of times and the attempts are recorded.
        /// </summary>
        private static GlabrezuRendRecord38 GlabrezuRendStrike38(
            UnitEntityData glabrezu, UnitEntityData victim,
            ItemEntityWeapon weapon, bool requireHit, JArray log)
        {
            GlabrezuRendRecord38 last = null;
            for (int attempt = 0; attempt < 24; attempt++)
            {
                int damage = victim.Descriptor.Damage;
                RuleAttackWithWeapon rule = Rulebook.Trigger(
                    new RuleAttackWithWeapon(glabrezu, victim, weapon, 0));
                // The victims are fixture-owned and must survive the whole
                // sequence; the rend's own damage is not this cell's subject.
                victim.Descriptor.Damage = damage;
                last = new GlabrezuRendRecord38 {
                    Weapon = weapon.Blueprint.name,
                    Victim = victim.UniqueId,
                    Hit = rule.AttackRoll != null && rule.AttackRoll.IsHit,
                    Rend = rule.IsRend
                };
                log.Add(new JObject {
                    ["weapon"] = last.Weapon,
                    ["victim"] = last.Victim,
                    ["attempt"] = attempt,
                    ["roll"] = rule.AttackRoll == null ? null :
                        rule.AttackRoll.Result.ToString(),
                    ["hit"] = last.Hit,
                    ["rend"] = last.Rend
                });
                if (!requireHit || last.Hit) return last;
            }
            return last;
        }

        private static JObject GlabrezuRendStateRow38(UnitEntityData glabrezu)
        {
            GlabrezuRendRound38 round =
                GlabrezuRendGate38.ObservedState(glabrezu);
            return new JObject {
                ["roundStatePresent"] = round != null,
                ["armed"] = round != null && round.Armed,
                ["spentThisRound"] = round != null && round.SpentThisRound,
                ["trackedVictims"] = round == null ? 0 :
                    round.TrackedVictimCount
            };
        }

        /// <summary>
        /// Puts the owner and its victim into real combat, which is what makes
        /// the round clock run at all.
        ///
        /// <para>The second guarded run spent thirty unpaused seconds - 992
        /// frames - waiting for a round boundary that never came, with the
        /// state stuck at spent. Outside combat the engine raises no round
        /// ticks, so <c>ITickEachRound</c> is never called and the rend
        /// allowance is never returned. That is correct for the creature - a
        /// rend only matters in combat - but an observation of the boundary has
        /// to put the units in combat to see one.</para>
        ///
        /// <para>Only enrolment happens here. No brain is activated and no
        /// command is issued, so nothing attacks on its own: the owner's brain
        /// stays quiesced and its live attack command points at the distant
        /// victim.</para>
        /// </summary>
        private static JObject GlabrezuEnterCombat38(UnitEntityData glabrezu,
            UnitEntityData victim)
        {
            glabrezu.Memory.Add(victim).Visible = true;
            victim.Memory.Add(glabrezu).Visible = true;
            if (glabrezu.Group != null)
                glabrezu.Group.Memory.Add(victim).Visible = true;
            if (victim.Group != null)
                victim.Group.Memory.Add(glabrezu).Visible = true;
            new Kingmaker.Controllers.Units.UnitMemoryController().Tick();
            new Kingmaker.Controllers.Combat.UnitCombatJoinController().Tick();
            new Kingmaker.Controllers.Combat.UnitCombatPrepareController()
                .Tick();
            if (!glabrezu.CombatState.IsInCombat)
                glabrezu.CombatState.JoinCombat();
            if (!victim.CombatState.IsInCombat)
                victim.CombatState.JoinCombat();
            new Kingmaker.Controllers.Combat.UnitCombatJoinController().Tick();
            new Kingmaker.Controllers.Combat.UnitCombatPrepareController()
                .Tick();
            return new JObject {
                ["owner"] = glabrezu.UniqueId,
                ["victim"] = victim.UniqueId,
                ["ownerInCombat"] = glabrezu.IsInCombat,
                ["victimInCombat"] = victim.IsInCombat,
                ["ownerBrainActive"] = glabrezu.IsBrainActive,
                ["victimBrainActive"] = victim.IsBrainActive,
                ["whyCombatIsRequired"] =
                    "The engine raises no round ticks outside combat, so ITickEachRound is never called and the round's rend allowance is never returned. The second guarded run waited 992 unpaused frames for a boundary that could not come."
            };
        }

        /// <summary>
        /// Waits for the production round state to be genuinely fresh - no
        /// armed rend, no spent round and no remembered attack pair - and
        /// leaves the game PAUSED when it is.
        ///
        /// <para>Both halves are needed, and the first guarded run proved why.
        /// The live attack command this phase has to hold is a real command:
        /// left running, the owner attacked the adjacent victim on its own and
        /// spent the round's rend before the deterministic sequence began, so
        /// every cell measured an already-spent round. The engine raises the
        /// round boundary from game time, so pausing stops both the ambient
        /// attacking AND any further boundary - which is exactly the window
        /// this phase needs: one round, no other attacker, and
        /// <c>Rulebook.Trigger</c> still answering synchronously.</para>
        ///
        /// <para>The state is re-read after pausing, because a frame can land
        /// an ambient attack between the check and the pause; a stale read is
        /// retried rather than trusted.</para>
        /// </summary>
        private IEnumerable<int> GlabrezuAwaitRound38(UnitEntityData glabrezu,
            JObject record)
        {
            GlabrezuRendRound38 entry =
                GlabrezuRendGate38.ObservedState(glabrezu);
            bool wasSpent = entry != null && entry.SpentThisRound;
            int victimsBefore = entry == null ? 0 : entry.TrackedVictimCount;
            Func<GlabrezuRendRound38, bool> isFresh = state =>
                state == null || (!state.Armed && !state.SpentThisRound &&
                    state.TrackedVictimCount == 0);
            DateTime deadline = DateTime.UtcNow.AddSeconds(30);
            int frames = 0;
            bool fresh = isFresh(entry);
            while (!fresh && DateTime.UtcNow < deadline)
            {
                Game.Instance.IsPaused = false;
                frames++;
                yield return 0;
                if (!isFresh(GlabrezuRendGate38.ObservedState(glabrezu)))
                    continue;
                Game.Instance.IsPaused = true;
                fresh = isFresh(GlabrezuRendGate38.ObservedState(glabrezu));
            }
            // Pausing is best-effort isolation only. The second guarded run
            // recorded IsPaused reading back false immediately after being
            // set, so this phase does not depend on it: the owner's brain is
            // quiesced, its live attack command points at the distant victim,
            // and the near victim's brain is off, which is what actually keeps
            // the sequence free of ambient attacks.
            Game.Instance.IsPaused = true;
            record["roundBoundaryFrames"] = frames;
            record["roundStateIsFresh"] = fresh;
            record["spentBeforeBoundary"] = wasSpent;
            record["crossedFromSpentRound"] = wasSpent && fresh;
            record["trackedVictimsBeforeBoundary"] = victimsBefore;
            record["stateAfterBoundary"] = GlabrezuRendStateRow38(glabrezu);
            record["pauseRequested"] = true;
            record["pauseObserved"] = Game.Instance.IsPaused;
            record["ownerInCombat"] = glabrezu.IsInCombat;
        }

        /// <summary>
        /// Brings the owner to a real live attack command, which is the printed
        /// requirement that a rend belongs to an attack sequence.
        ///
        /// <para>The command is issued through the engine's own queue while the
        /// game is paused. The first guarded run showed that
        /// <c>Commands.Run</c> populates <c>Commands.Attack</c> in the same
        /// frame - zero frames waited - so no time has to pass and nothing
        /// executes. The target is the DISTANT victim on purpose: if the
        /// engine is later unpaused to cross a round boundary, the owner walks
        /// toward it rather than attacking and spending the round's rend
        /// itself, which is what spoiled every cell on the first run.</para>
        /// </summary>
        private IEnumerable<int> GlabrezuLiveAttackCommand38(
            UnitEntityData glabrezu, UnitEntityData distantVictim,
            JObject record)
        {
            bool paused = Game.Instance.IsPaused;
            Game.Instance.IsPaused = true;
            glabrezu.Commands.InterruptAll(true);
            glabrezu.Commands.RemoveFinishedAndUpdateQueue();
            glabrezu.Commands.Run(new UnitAttack(distantVictim));
            int frames = 0;
            while (glabrezu.Commands.Attack == null && frames < 120)
            {
                Game.Instance.IsPaused = false;
                frames++;
                yield return 0;
            }
            Game.Instance.IsPaused = true;
            record["queuedCommandFrames"] = frames;
            record["queuedCommandWorked"] = glabrezu.Commands.Attack != null;
            record["pausedOnEntry"] = paused;
            record["commandTarget"] = distantVictim.UniqueId;
            record["commandTargetDistanceMeters"] = UnityEngine.Vector3
                .Distance(glabrezu.Position, distantVictim.Position);
            record["liveAttackCommand"] = glabrezu.Commands.Attack == null ?
                null : Sprint27AiCommand(glabrezu.Commands.Attack);
            record["hasLiveAttackCommand"] =
                glabrezu.Commands.Attack != null;
        }

        /// <summary>
        /// The four printed facts, each driven as a real sequence of attack
        /// rules inside one observed round:
        ///
        /// <list type="number">
        /// <item>two pincer hits on one victim rend, exactly once;</item>
        /// <item>the claws and the bite never rend, however many land;</item>
        /// <item>the pincer history is per victim, so a pincer that hit a
        /// second victim in between does not spoil the first victim's
        /// pair - the exact production state the released shared tracker
        /// could not hold;</item>
        /// <item>changing the attack command does not re-arm the round's one
        /// rend, and a new round does.</item>
        /// </list>
        /// </summary>
        private IEnumerable<int> GlabrezuRendSequences38(
            UnitEntityData glabrezu, UnitEntityData first,
            UnitEntityData second, ItemEntityWeapon[] pincers,
            ItemEntityWeapon[] others,
            Action<string, bool, JObject, string> check)
        {
            var command = new JObject();
            foreach (int frame in GlabrezuLiveAttackCommand38(glabrezu, second,
                command)) yield return frame;
            check("rend-live-attack-command",
                (bool)command["hasLiveAttackCommand"], command,
                "The owner holds a real live attack command, which is the printed requirement that a rend belongs to an attack sequence rather than to a standalone rule.");

            // 1. The pair, in one round, on one victim.
            var boundary = new JObject();
            foreach (int frame in GlabrezuAwaitRound38(glabrezu, boundary))
                yield return frame;
            var log = new JArray();
            GlabrezuRendRecord38 firstPincer = GlabrezuRendStrike38(glabrezu,
                first, pincers[0], true, log);
            JObject afterFirst = GlabrezuRendStateRow38(glabrezu);
            GlabrezuRendRecord38 secondPincer = GlabrezuRendStrike38(glabrezu,
                first, pincers[1], true, log);
            JObject afterSecond = GlabrezuRendStateRow38(glabrezu);
            GlabrezuRendRecord38 thirdPincer = GlabrezuRendStrike38(glabrezu,
                first, pincers[0], true, log);
            check("rend-pair-rends-once",
                firstPincer.Hit && !firstPincer.Rend &&
                secondPincer.Hit && secondPincer.Rend &&
                thirdPincer.Hit && !thirdPincer.Rend &&
                (bool)afterSecond["spentThisRound"],
                new JObject {
                    ["owner"] = glabrezu.UniqueId,
                    ["victim"] = first.UniqueId,
                    ["roundBoundary"] = boundary,
                    ["attacks"] = log,
                    ["firstPincerRend"] = firstPincer.Rend,
                    ["secondPincerRend"] = secondPincer.Rend,
                    ["thirdPincerRend"] = thirdPincer.Rend,
                    ["stateAfterFirst"] = afterFirst,
                    ["stateAfterSecond"] = afterSecond,
                    ["stateAfterThird"] = GlabrezuRendStateRow38(glabrezu)
                }, "The first pincer hit does not rend, the second pincer hit on the same victim in the same round does, and a third pincer hit in that round does not: once per round, not once per pair.");

            // 2. The limbs that never rend.
            var otherLog = new JArray();
            var otherRecords = new List<GlabrezuRendRecord38>();
            foreach (int frame in GlabrezuAwaitRound38(glabrezu,
                new JObject())) yield return frame;
            foreach (ItemEntityWeapon weapon in others)
            {
                otherRecords.Add(GlabrezuRendStrike38(glabrezu, first, weapon,
                    true, otherLog));
                otherRecords.Add(GlabrezuRendStrike38(glabrezu, first, weapon,
                    true, otherLog));
            }
            check("rend-other-limbs-never",
                otherRecords.All(value => value.Hit && !value.Rend),
                new JObject {
                    ["owner"] = glabrezu.UniqueId,
                    ["victim"] = first.UniqueId,
                    ["attacks"] = otherLog,
                    ["rends"] = otherRecords.Count(value => value.Rend),
                    ["stateAfter"] = GlabrezuRendStateRow38(glabrezu)
                }, "Two hits from each claw and from the bite on one victim in one round produce no rend at all: only the two named pincers carry it.");

            // 3. The per-victim history. pincer1 hits A, pincer1 hits B,
            //    pincer2 hits A. A production state that remembered only the
            //    last victim per pincer would have forgotten A's first hit and
            //    refused the rend.
            var crossLog = new JArray();
            var crossBoundary = new JObject();
            foreach (int frame in GlabrezuAwaitRound38(glabrezu,
                crossBoundary)) yield return frame;
            GlabrezuRendRecord38 firstOnA = GlabrezuRendStrike38(glabrezu,
                first, pincers[0], true, crossLog);
            GlabrezuRendRecord38 firstOnB = GlabrezuRendStrike38(glabrezu,
                second, pincers[0], true, crossLog);
            JObject afterCrossHits = GlabrezuRendStateRow38(glabrezu);
            GlabrezuRendRecord38 secondOnA = GlabrezuRendStrike38(glabrezu,
                first, pincers[1], true, crossLog);
            GlabrezuRendRecord38 secondOnB = GlabrezuRendStrike38(glabrezu,
                second, pincers[1], true, crossLog);
            check("rend-history-is-per-victim",
                firstOnA.Hit && !firstOnA.Rend &&
                firstOnB.Hit && !firstOnB.Rend &&
                secondOnA.Hit && secondOnA.Rend &&
                secondOnB.Hit && !secondOnB.Rend &&
                (int)afterCrossHits["trackedVictims"] == 2,
                new JObject {
                    ["owner"] = glabrezu.UniqueId,
                    ["victimA"] = first.UniqueId,
                    ["victimB"] = second.UniqueId,
                    ["roundBoundary"] = crossBoundary,
                    ["attacks"] = crossLog,
                    ["stateAfterCrossHits"] = afterCrossHits,
                    ["stateAfterRend"] = GlabrezuRendStateRow38(glabrezu),
                    ["whyThisSequence"] =
                        "A production state that kept only the last victim per pincer would have replaced victim A's first-pincer hit with victim B's and refused A's rend. Both victims are tracked at once, A's pair completes, and the round's single allowance is then spent so B's completed pair does not rend."
                }, "The pincer hit history is kept per victim: a pincer that struck a second victim in between does not spoil the first victim's pair, and the round's one rend is still only one.");

            // 4. A command change does not re-arm; a new round does.
            var reissue = new JObject();
            foreach (int frame in GlabrezuLiveAttackCommand38(glabrezu, second,
                reissue)) yield return frame;
            var sameRoundLog = new JArray();
            GlabrezuRendRecord38 afterCommandChange = GlabrezuRendStrike38(
                glabrezu, first, pincers[0], true, sameRoundLog);
            GlabrezuRendRecord38 afterCommandChangePair =
                GlabrezuRendStrike38(glabrezu, first, pincers[1], true,
                    sameRoundLog);
            JObject spentState = GlabrezuRendStateRow38(glabrezu);
            var newRound = new JObject();
            foreach (int frame in GlabrezuAwaitRound38(glabrezu, newRound))
                yield return frame;
            var newRoundLog = new JArray();
            GlabrezuRendRecord38 freshFirst = GlabrezuRendStrike38(glabrezu,
                first, pincers[0], true, newRoundLog);
            GlabrezuRendRecord38 freshSecond = GlabrezuRendStrike38(glabrezu,
                first, pincers[1], true, newRoundLog);
            check("rend-round-not-command",
                !afterCommandChange.Rend && !afterCommandChangePair.Rend &&
                (bool)spentState["spentThisRound"] &&
                (bool)newRound["crossedFromSpentRound"] &&
                !freshFirst.Rend && freshSecond.Rend,
                new JObject {
                    ["owner"] = glabrezu.UniqueId,
                    ["victim"] = first.UniqueId,
                    ["reissuedCommand"] = reissue,
                    ["sameRoundAfterCommandChange"] = sameRoundLog,
                    ["stateWhileSpent"] = spentState,
                    ["roundBoundary"] = newRound,
                    ["newRoundAttacks"] = newRoundLog,
                    ["releasedTrackerContrast"] =
                        "The released shared tracker clears its emitted flag whenever the attack command changes, so this exact sequence would have produced a second rend inside one round. The round allowance is a separate operand from the attack-pair bookkeeping precisely so that cannot happen."
                }, "A fresh attack command does not re-arm the round's spent rend, and the real round boundary does: the limit is scoped to the round, not to the command.");
        }
    }
}
