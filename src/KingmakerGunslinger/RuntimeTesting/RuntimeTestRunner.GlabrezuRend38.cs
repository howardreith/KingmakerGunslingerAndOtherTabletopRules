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
                PlaceExpandedSummoningUnit(second, glabrezu.Position +
                    Vector3.left *
                    (glabrezu.Corpulence + second.Corpulence + 0.5f));

                ItemEntityWeapon[] pincers;
                ItemEntityWeapon[] others;
                GlabrezuRendLimbs38(glabrezu, out pincers, out others, check);

                foreach (int frame in GlabrezuRendSequences38(glabrezu, target,
                    second, pincers, others, check)) yield return frame;
            }
            finally
            {
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
        /// Waits for the production round boundary - the real
        /// <c>ITickEachRound</c> callback clearing this unit's state - rather
        /// than for a duration this observer decides is a round. The game is
        /// unpaused while waiting, because the boundary is raised by the
        /// engine's own tick controllers.
        /// </summary>
        private IEnumerable<int> GlabrezuAwaitRound38(UnitEntityData glabrezu,
            JObject record)
        {
            GlabrezuRendRound38 round =
                GlabrezuRendGate38.ObservedState(glabrezu);
            bool wasSpent = round != null && round.SpentThisRound;
            int victimsBefore = round == null ? 0 : round.TrackedVictimCount;
            DateTime deadline = DateTime.UtcNow.AddSeconds(30);
            int frames = 0;
            bool cleared = false;
            while (DateTime.UtcNow < deadline)
            {
                Game.Instance.IsPaused = false;
                frames++;
                yield return 0;
                GlabrezuRendRound38 now =
                    GlabrezuRendGate38.ObservedState(glabrezu);
                if (now != null && !now.SpentThisRound &&
                    now.TrackedVictimCount == 0 &&
                    (wasSpent || victimsBefore > 0))
                { cleared = true; break; }
            }
            record["roundBoundaryFrames"] = frames;
            record["roundBoundaryObserved"] = cleared;
            record["spentBeforeBoundary"] = wasSpent;
            record["trackedVictimsBeforeBoundary"] = victimsBefore;
            record["stateAfterBoundary"] = GlabrezuRendStateRow38(glabrezu);
        }

        /// <summary>
        /// Brings the owner to a real live attack command, which is the printed
        /// requirement that a rend belongs to an attack sequence. The command
        /// is issued through the engine's own queue; if that does not populate
        /// <c>Commands.Attack</c>, the released brain fixture is driven until a
        /// real one appears, and which path succeeded is recorded.
        /// </summary>
        private IEnumerable<int> GlabrezuLiveAttackCommand38(
            UnitEntityData glabrezu, UnitEntityData victim, JObject record)
        {
            glabrezu.Commands.InterruptAll(true);
            glabrezu.Commands.RemoveFinishedAndUpdateQueue();
            glabrezu.Commands.Run(new UnitAttack(victim));
            int frames = 0;
            while (glabrezu.Commands.Attack == null && frames < 120)
            {
                Game.Instance.IsPaused = false;
                frames++;
                yield return 0;
            }
            record["queuedCommandFrames"] = frames;
            record["queuedCommandWorked"] = glabrezu.Commands.Attack != null;
            if (glabrezu.Commands.Attack == null)
            {
                JObject brain = VrockPrepareBrain37(glabrezu, victim);
                foreach (int frame in VrockDriveBrain37(glabrezu,
                    () => glabrezu.Commands.Attack != null, 30, brain))
                    yield return frame;
                record["brainFallback"] = brain;
                Sprint28SetBrainActivePreservingActions(glabrezu, false);
            }
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
            foreach (int frame in GlabrezuLiveAttackCommand38(glabrezu, first,
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
            foreach (int frame in GlabrezuLiveAttackCommand38(glabrezu, first,
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
                (bool)newRound["roundBoundaryObserved"] &&
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
