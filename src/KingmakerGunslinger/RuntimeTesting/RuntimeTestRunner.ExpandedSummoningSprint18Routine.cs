using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker;
using Kingmaker.Controllers;
using Kingmaker.Controllers.Combat;
using Kingmaker.Controllers.Units;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Items;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic.Commands;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json.Linq;
using TurnBased.Controllers;

namespace KingmakerGunslinger.RuntimeTesting
{
    /// <summary>
    /// The printed routines and the rend, driven as real commands.
    ///
    /// <para>Every attack here is a native full attack issued against a
    /// disposable hostile, resolved by the native rulebook. The fixture
    /// decides only whether a given roll can reach the target, by moving this
    /// one disposable attacker's accuracy before the roll and putting it back
    /// after; it never writes a result, never replays a rule and never adds a
    /// damage event of its own.</para>
    /// </summary>
    internal sealed partial class RuntimeTestRunner
    {
        /// <summary>Hit or miss for one upcoming roll, chosen by limb.</summary>
        private sealed class Sprint18Accuracy
        {
            internal UnitEntityData Owner;
            internal ItemEntityWeapon[] Hit = new ItemEntityWeapon[0];
            internal int Restore;

            internal void Apply(RuleAttackRoll evt)
            {
                bool wanted = Hit.Any(weapon => ReferenceEquals(weapon, evt.Weapon));
                Owner.Descriptor.Stats.AdditionalAttackBonus.BaseValue = wanted ? 100 : -100;
            }

            /// <summary>
            /// The resting value between rolls. A roll the fixture does not
            /// hook must miss rather than inherit the last decision.
            /// </summary>
            internal void Rest()
            { Owner.Descriptor.Stats.AdditionalAttackBonus.BaseValue = -100; }
        }

        /// <summary>
        /// The full attack: exactly the printed limbs, each one primary, each
        /// one at full Strength, in both combat modes.
        /// </summary>
        private IEnumerable<int> ReviewSprint18Routine(
            ExpandedSummoningCorrectionFixture fixture, UnitEntityData owner,
            string key, bool turnBased)
        {
            PrimateLiveProfile expected = PrimateReviewPolicy.For(key);
            ItemEntityWeapon[] limbs = Sprint18Limbs(owner);
            var observer = new Sprint16RuleObserver { Owner = owner, Target = fixture.Hostile };
            var accuracy = new Sprint18Accuracy { Owner = owner, Hit = limbs };
            int restore = owner.Descriptor.Stats.AdditionalAttackBonus.BaseValue;
            observer.BeforeAttackRollForFixture = accuracy.Apply;
            EventBus.Subscribe(observer);
            UnitAttack attack = null;
            try
            {
                foreach (int step in Sprint18RunFullAttack(fixture, owner, result => attack = result))
                    yield return step;
                var byLimb = new JArray();
                var counted = new Dictionary<int, int>();
                foreach (RuleAttackRoll roll in observer.Attacks)
                {
                    int index = Array.FindIndex(limbs, weapon =>
                        ReferenceEquals(weapon, roll.Weapon));
                    counted[index] = counted.ContainsKey(index) ? counted[index] + 1 : 1;
                    byLimb.Add(new JObject {
                        ["limb"] = index,
                        ["weapon"] = roll.Weapon == null ? null : roll.Weapon.Blueprint.AssetGuid,
                        ["hit"] = roll.IsHit,
                        ["secondary"] = roll.Weapon != null && roll.Weapon.IsSecondary });
                }
                // Exactly the printed limbs, each exactly once, none of
                // them resolved as a secondary attack, in the mode asked for.
                bool exact = observer.Attacks.Count ==
                        PrimateReviewPolicy.ExpectedAttacksInAFullAttack(key) &&
                    limbs.Length == expected.Limbs.Length &&
                    !counted.ContainsKey(-1) &&
                    Enumerable.Range(0, limbs.Length).All(index =>
                        counted.ContainsKey(index) && counted[index] == 1) &&
                    observer.Attacks.All(roll => roll.Weapon != null &&
                        !roll.Weapon.IsSecondary) &&
                    CombatController.IsInTurnBasedCombat() == turnBased;
                Sprint18Check(_primateAssertions, _primateRows,
                    key + (turnBased ? "-turn-based" : "-real-time") + "-full-attack", exact,
                    new JObject {
                        ["mode"] = turnBased ? "turn-based" : "real-time",
                        ["observedMode"] = CombatController.IsInTurnBasedCombat(),
                        ["attacks"] = observer.Attacks.Count,
                        ["expected"] = PrimateReviewPolicy.ExpectedAttacksInAFullAttack(key),
                        ["perLimb"] = byLimb,
                        ["commandFinished"] = attack != null && attack.IsFinished,
                    }, "one full attack is exactly the printed routine, every limb primary");
            }
            finally
            {
                EventBus.Unsubscribe(observer);
                owner.Descriptor.Stats.AdditionalAttackBonus.BaseValue = restore;
                accuracy.Restore = restore;
            }
        }

        /// <summary>
        /// The rend, case by case. Only two claws landing on one target in one
        /// sequence may emit it, and it may emit exactly once per sequence.
        /// </summary>
        private IEnumerable<int> ReviewSprint18Rend(
            ExpandedSummoningCorrectionFixture fixture, UnitEntityData owner, bool turnBased)
        {
            ItemEntityWeapon[] limbs = Sprint18Limbs(owner);
            ItemEntityWeapon bite = limbs[0], first = limbs[1], second = limbs[2];
            PrimateLiveProfile expected = PrimateReviewPolicy.For(PrimateRulesPolicy.DireApeKey);
            int restore = owner.Descriptor.Stats.AdditionalAttackBonus.BaseValue;
            foreach (string[] scenario in PrimateReviewPolicy.RendCases)
            {
                string name = scenario[0];
                bool qualifies = scenario[1].StartsWith("qualifies", StringComparison.Ordinal);
                var observer = new Sprint16RuleObserver { Owner = owner, Target = fixture.Hostile };
                var accuracy = new Sprint18Accuracy { Owner = owner };
                observer.BeforeAttackRollForFixture = accuracy.Apply;
                EventBus.Subscribe(observer);
                int sequences = name == "claws-across-two-commands" ||
                    name == "claws-across-two-turns" ||
                    name == "claws-on-two-targets" ||
                    name == "second-sequence-after-a-rend" ? 2 : 1;
                var emissions = new JArray();
                UnitEntityData secondTarget = null;
                Kingmaker.Blueprints.BlueprintUnit secondBlueprint = null;
                Sprint16RuleObserver secondObserver = null;
                int trials = 0;
                bool clean = false;
                int firstRendFrom = 0, secondRendFrom = 0;
                int secondTargetDamage = 0;
                string secondReset = "second=none";
                try
                {
                    // One case is one atomic trial. A natural one always
                    // misses and a natural twenty always hits, whatever the
                    // bonus, so a trial in which exactly the chosen limbs did
                    // not land is a roll of the dice and not a result; it is
                    // abandoned whole and retried from nothing.
                    //
                    // Retrying a single sequence instead was wrong, and the
                    // series closure showed how - the same way Sprint 19's
                    // ninth guarded run did. The rend tracker is scoped to the
                    // attack command rather than to the review's idea of an
                    // attempt, so claws that landed in an abandoned attempt
                    // still counted, and the two-target case eventually landed
                    // both claws on one target although no single attempt did.
                    // Clearing between whole trials is sound where clearing
                    // between sequences would not be: what is under test is
                    // accumulation within a sequence and across sequences, and
                    // a discarded trial is neither.
                    while (!clean && ++trials <= 6)
                    {
                        DireApeRendGate.ForgetObserved(owner);
                        ResetExpandedSummoningHostile(fixture);
                        secondReset = Sprint18ResetSecondTarget(
                            secondTarget, secondTargetDamage);
                        firstRendFrom = observer.Damage.Count;
                        secondRendFrom = secondObserver == null ? 0 :
                            secondObserver.Damage.Count;
                        emissions = new JArray();
                        clean = true;
                        for (int sequence = 0; sequence < sequences; sequence++)
                        {
                            accuracy.Hit = Sprint18HitSet(name, sequence, bite, first, second);
                            accuracy.Rest();
                            // A native full attack costs the whole turn, so in
                            // turn-based combat every sequence needs a fresh one.
                            // Nothing here clears the rend tracker: the native
                            // round boundary is what does that, and the case that
                            // measures it says so.
                            if (CombatController.IsInTurnBasedCombat() ||
                                (name == "claws-across-two-turns" && sequence == 1))
                            {
                                foreach (int step in Sprint18AdvanceRound(owner)) yield return step;
                            }
                            UnitAttack ignored = null;
                            int before = observer.Attacks.Count +
                                (secondObserver == null ? 0 : secondObserver.Attacks.Count);
                            if (name == "claws-on-two-targets" && sequence == 0)
                            {
                                // Created and subscribed once, across every trial.
                                // A second observer per trial would leave the
                                // earlier ones subscribed and double-count the
                                // rolls this case depends on separating.
                                if (secondTarget == null)
                                {
                                    secondTarget = Sprint18SecondTarget(fixture,
                                        out secondBlueprint);
                                    secondObserver = new Sprint16RuleObserver {
                                        Owner = owner, Target = secondTarget };
                                    // The hit chooser has to follow the target the
                                    // sequence is actually aimed at, or every limb
                                    // lands and the case earns a rend it is not
                                    // owed.
                                    secondObserver.BeforeAttackRollForFixture =
                                        accuracy.Apply;
                                    EventBus.Subscribe(secondObserver);
                                    secondRendFrom = secondObserver.Damage.Count;
                                    secondTargetDamage =
                                        secondTarget.Descriptor.Damage;
                                }
                                foreach (int step in Sprint18RunFullAttack(fixture, owner,
                                    result => ignored = result, secondTarget)) yield return step;
                            }
                            else
                                foreach (int step in Sprint18RunFullAttack(fixture, owner,
                                    result => ignored = result)) yield return step;
                            bool landed = !Sprint18ChosenSequenceMissed(
                                observer, secondObserver, before, accuracy);
                            emissions.Add(new JObject {
                                ["sequence"] = sequence,
                                ["chosenLimbs"] = accuracy.Hit.Length,
                                ["exactlyTheChosenLimbsLanded"] = landed,
                                ["rolls"] = new JArray(Sprint18Rolls(
                                    observer, secondObserver, before)),
                                ["commandFinished"] = ignored != null && ignored.IsFinished });
                            if (!landed) { clean = false; break; }
                        }
                    }
                    // The measured trial's own rends, not every trial's.
                    RuleDealDamage[] rends = observer.Damage.Skip(firstRendFrom)
                        .Concat(secondObserver == null ? Enumerable.Empty<RuleDealDamage>() :
                                secondObserver.Damage.Skip(secondRendFrom))
                        .Where(value => ReferenceEquals(value.Initiator, owner) &&
                            value.AttackRoll == null).ToArray();
                    int wanted = qualifies ? (name == "second-sequence-after-a-rend" ? 2 : 1) : 0;
                    bool shape = clean && rends.Length == wanted && rends.All(rend =>
                        rend.DamageBundle.First().Dice.Rolls == expected.RendRolls &&
                        (int)rend.DamageBundle.First().Dice.Dice == expected.RendDieSides &&
                        rend.DamageBundle.First().Bonus == expected.RendBonus);
                    Sprint18Check(_primateAssertions, _primateRows,
                        "rend-" + name + (turnBased ? "-turn-based" : "-real-time"),
                        shape, new JObject {
                            ["mode"] = turnBased ? "turn-based" : "real-time",
                            ["disposition"] = scenario[1],
                            ["expectedRends"] = wanted,
                            ["observedRends"] = rends.Length,
                            ["trials"] = trials,
                            ["everyChosenLimbLandedInTheMeasuredTrial"] = clean,
                            ["secondTargetReset"] = secondReset,
                            ["rendDamage"] = new JArray(rends.Select(rend =>
                                Sprint16DamageLine(rend.DamageBundle.First()))),
                            ["sequences"] = emissions,
                            ["trackerAfter"] = DireApeRendGate.ObservedTracker(owner) == null ?
                                "cleared" : "live",
                            ["secondTarget"] = secondTarget == null ? null :
                                secondTarget.UniqueId,
                            ["note"] = name != "claws-on-two-targets" ? null :
                                "A native full attack has one target, so two " +
                                "targets necessarily means two commands; this " +
                                "case therefore also crosses the sequence boundary.",
                            ["turnNote"] = !turnBased ? null :
                                "A native full attack costs the whole turn, so " +
                                "in turn-based combat every sequence is also a " +
                                "new turn. Only the real-time cases separate a " +
                                "new command from a new round.",
                        }, qualifies
                            ? "exactly one rend per qualifying sequence, at the live Strength"
                            : "no rend at all from a sequence that does not qualify");
                }
                finally
                {
                    EventBus.Unsubscribe(observer);
                    if (secondObserver != null) EventBus.Unsubscribe(secondObserver);
                    owner.Descriptor.Stats.AdditionalAttackBonus.BaseValue = restore;
                    DireApeRendGate.ForgetObserved(owner);
                    ResetExpandedSummoningHostile(fixture);
                    if (secondTarget != null && !secondTarget.Destroyed)
                        secondTarget.Destroy();
                    if (secondBlueprint != null)
                        UnityEngine.Object.Destroy(secondBlueprint);
                }
                yield return 0;
            }
        }

        /// <summary>
        /// A second disposable hostile, built by exactly the path the fixture
        /// uses for its first one, so the two-target rend case has a real
        /// other target instead of a party member. The case destroys it.
        /// </summary>
        private static UnitEntityData Sprint18SecondTarget(
            ExpandedSummoningCorrectionFixture fixture,
            out Kingmaker.Blueprints.BlueprintUnit blueprint)
        {
            UnitEntityData pixie = CastExpandedSummoningVariant(fixture.Blueprints,
                fixture.Caster, ExpandedSummoningVariant(SummonFamily.NaturesAlly,
                    "pixie", 9, SummonMultiplicity.One), null, fixture.Evidence).Single();
            fixture.Created.Add(pixie);
            try
            {
                Kingmaker.UnitLogic.Abilities.Blueprints.BlueprintAbility dance =
                    fixture.Blueprints.OfType<
                        Kingmaker.UnitLogic.Abilities.Blueprints.BlueprintAbility>().Single(
                        value => value.name ==
                            "KMG_Summoning_Special_Pixie_IrresistibleDance");
                UnitEntityData target = CreateExpandedSummoningHostileTarget(
                    fixture.Blueprints, pixie, dance,
                    fixture.Caster.Position + UnityEngine.Vector3.right * 4f,
                    fixture.Scene, out blueprint);
                target.Descriptor.Stats.HitPoints.BaseValue = 100000;
                SetExpandedSummoningBrainActive(target, false);
                PlaceExpandedSummoningUnit(target, target.Position);
                return target;
            }
            finally { DisposeExpandedSummoningUnits(fixture.Created, new[] { pixie }); }
        }

        /// <summary>
        /// Did a limb the fixture chose to land fail to land? A natural one
        /// misses whatever the bonus, so this is the precondition a rend case
        /// needs, not the thing it measures.
        /// </summary>
        /// <summary>
        /// Did the sequence the fixture set up actually happen?
        ///
        /// <para>Both halves, because the dice break a setup in both
        /// directions. A chosen limb can miss - a natural one always misses,
        /// whatever the +100 says - and a limb told to miss at -100 can land,
        /// because a natural twenty always hits. The series closure measured
        /// the second twice: the Ape's one-claw-only rend case rolled a twenty
        /// on the claw it had told to miss, and the Girallon's
        /// bite-and-three-claws case did the same. Both times every claw
        /// landed, rend correctly fired, and the review recorded a rend
        /// violation for a sequence it had never arranged.</para>
        ///
        /// <para>So the trial is spoiled unless EXACTLY the chosen limbs
        /// landed. No requirement moves: a sequence that does not qualify must
        /// still produce no rend at all, and the rolls stay native - the only
        /// thing waited for is the precondition the case needs. Where the
        /// chosen set is every limb, as in the full-attack routine, the second
        /// half can never fire.</para>
        ///
        /// <para>Sprint 20 found this first and answered it with a second
        /// predicate of its own; this absorbs that one rather than keeping two
        /// copies of the same rule.</para>
        /// </summary>
        private static bool Sprint18ChosenSequenceMissed(Sprint16RuleObserver observer,
            Sprint16RuleObserver second, int from, Sprint18Accuracy accuracy)
        {
            if (accuracy.Hit.Length == 0) return false;
            RuleAttackRoll[] rolls = Sprint18AttemptRolls(observer, second, from);
            if (rolls.Length == 0) return true;
            if (accuracy.Hit.Any(weapon => !rolls.Any(roll =>
                ReferenceEquals(roll.Weapon, weapon) && roll.IsHit))) return true;
            return rolls.Any(roll => roll.IsHit && roll.Weapon != null &&
                !accuracy.Hit.Any(weapon =>
                    ReferenceEquals(weapon, roll.Weapon)));
        }

        /// <summary>
        /// Did the saving throw land on the side the fixture chose?
        ///
        /// <para>The same shape as the limbs, on the other side of a save. A
        /// natural one always fails and a natural twenty always makes it,
        /// whatever modifier the fixture set on the disposable target, so a
        /// trial that chose one side and measured the other has measured the
        /// dice rather than the rule. The series closure measured it on the
        /// Xill: a paralysis case that set the target's own Fortitude to +100
        /// read a failed save at the printed difficulty class.</para>
        ///
        /// <para>Both directions, unlike the Sprint 20 predicate this
        /// replaces, which covered only the natural twenty because that is the
        /// one its own run hit. Stated as the side rather than as the d20
        /// face, so a fixture whose modifier is not actually decisive fails
        /// its bound instead of passing on a trial it never established -
        /// which is the right outcome, not a silent pass.</para>
        ///
        /// <para>A trial that forced no save at all is not spoiled here. A
        /// claw forcing no save is a REQUIREMENT of these cases, and a bite
        /// that never landed is already the limb predicate's business.</para>
        /// </summary>
        private static bool Sprint18ChosenSaveSideMissed(
            Sprint19SaveObserver saves, int from, bool wantMade)
        {
            RuleSavingThrow[] forced = saves.Saves.Skip(from).Where(save =>
                save != null && save.StatType == StatType.SaveFortitude)
                .ToArray();
            if (forced.Length == 0) return false;
            return forced.Any(save => save.IsPassed != wantMade);
        }

        /// <summary>
        /// Heal a split-target case's second target back to the state it was
        /// created in, as ResetExpandedSummoningHostile does for the first.
        ///
        /// <para>Only the first was reset between trials, so the second
        /// accumulated damage across them and eventually died - and a command
        /// aimed at a unit that is gone throws inside the engine's own merge
        /// path, which is the NullReferenceException from
        /// UnitAttack.GetApproachRadius the series closure recorded.
        /// Request-local, on a disposable unit the review created and
        /// destroys.</para>
        /// </summary>
        private static string Sprint18ResetSecondTarget(
            UnitEntityData target, int createdDamage)
        {
            if (target == null || target.Destroyed) return "second=absent";
            target.Descriptor.Damage = createdDamage;
            return "second=reset";
        }

        private static RuleAttackRoll[] Sprint18AttemptRolls(
            Sprint16RuleObserver observer, Sprint16RuleObserver second, int from)
        {
            IEnumerable<RuleAttackRoll> all = observer.Attacks;
            if (second != null) all = all.Concat(second.Attacks);
            return all.Skip(from).ToArray();
        }

        private static JObject[] Sprint18Rolls(Sprint16RuleObserver observer,
            Sprint16RuleObserver second, int from)
        {
            return Sprint18AttemptRolls(observer, second, from).Select(roll =>
                new JObject {
                    ["weapon"] = roll.Weapon == null ? null :
                        roll.Weapon.Blueprint.AssetGuid,
                    ["d20"] = roll.Roll.Value,
                    ["bonus"] = roll.AttackBonus,
                    ["targetArmor"] = roll.TargetAC,
                    ["hit"] = roll.IsHit }).ToArray();
        }

        /// <summary>Which limbs may reach the target in this sequence.</summary>
        private static ItemEntityWeapon[] Sprint18HitSet(string name, int sequence,
            ItemEntityWeapon bite, ItemEntityWeapon first, ItemEntityWeapon second)
        {
            switch (name)
            {
                case "both-claws-one-target":
                    return new[] { first, second };
                case "one-claw-only":
                    return new[] { first };
                case "claws-on-two-targets":
                    // One claw lands on another target, the other lands here.
                    // Neither target is hit by both claws, so neither is owed
                    // a rend, and the tracker is never cleared in between.
                    return sequence == 0 ? new[] { first } : new[] { second };
                case "bite-and-one-claw":
                    return new[] { bite, first };
                case "claws-across-two-commands":
                case "claws-across-two-turns":
                    return sequence == 0 ? new[] { first } : new[] { second };
                case "second-sequence-after-a-rend":
                    return new[] { first, second };
                default:
                    throw new ArgumentOutOfRangeException("name", name,
                        "unreviewed rend case");
            }
        }

        /// <summary>
        /// Issue one native full attack and wait for the native command to
        /// finish, bounded by frames rather than by any result.
        ///
        /// <para>In turn-based combat a command issued outside the owner's
        /// turn never resolves: the first run measured zero attacks in that
        /// mode for exactly that reason. So the review waits for the native
        /// turn controller to hand this unit its turn, ending other units'
        /// finished turns the way the qualified Sprint 16 cells do, and only
        /// then issues the command. No action, movement or resource is
        /// requested of the working-save party.</para>
        /// </summary>
        private IEnumerable<int> Sprint18RunFullAttack(
            ExpandedSummoningCorrectionFixture fixture, UnitEntityData owner,
            Action<UnitAttack> record, UnitEntityData target = null)
        {
            UnitEntityData victim = target ?? fixture.Hostile;
            owner.Memory.Add(victim);
            victim.Memory.Add(owner);
            foreach (UnitEntityData unit in new[] { owner, victim })
                if (!Game.Instance.State.AwakeUnits.Contains(unit))
                    Game.Instance.State.AwakeUnits.Add(unit);
            int waited = 0;
            while (CombatController.IsInTurnBasedCombat() &&
                ++waited <= Sprint18AttackFrames)
            {
                if (Game.Instance.IsPaused) Game.Instance.IsPaused = false;
                if (Sprint18OwnerIsActing(owner)) break;
                yield return 0;
            }
            var attack = new UnitAttack(victim) { ForceFullAttack = true };
            record(attack);
            owner.Commands.Run(attack);
            int frames = 0;
            while (++frames <= Sprint18AttackFrames)
            {
                if (Game.Instance.IsPaused) Game.Instance.IsPaused = false;
                if (CombatController.IsInTurnBasedCombat())
                    Sprint18OwnerIsActing(owner);
                yield return 0;
                if (attack.IsFinished) break;
            }
        }

        private readonly UnitCombatJoinController _primateJoin =
            new UnitCombatJoinController();
        private readonly UnitCombatPrepareController _primatePrepare =
            new UnitCombatPrepareController();
        private readonly HashSet<TurnController> _primatePrepared =
            new HashSet<TurnController>();

        /// <summary>
        /// Drive the native turn order until this unit is the one acting.
        /// Other units' finished turns are ended; none is asked to act.
        /// </summary>
        private bool Sprint18OwnerIsActing(UnitEntityData owner)
        {
            _primateJoin.Tick();
            _primatePrepare.Tick();
            TurnController turn = Game.Instance.TurnBasedCombatController.CurrentTurn;
            if (turn == null) return false;
            if (turn.Status == TurnController.TurnStatus.None &&
                _primatePrepared.Add(turn)) turn.Prepare();
            bool acting = turn.Status == TurnController.TurnStatus.Preparing ||
                turn.Status == TurnController.TurnStatus.Acting;
            if (ReferenceEquals(turn.Unit, owner)) return acting;
            if (acting) Sprint18EndTurn(turn);
            return false;
        }

        private static void Sprint18EndTurn(TurnController turn)
        {
            turn.ForceToEnd(true);
            System.Reflection.MethodInfo end = typeof(TurnController).GetMethod("End",
                System.Reflection.BindingFlags.Public |
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance, null, Type.EmptyTypes, null);
            if (end != null) end.Invoke(turn, null);
        }

        /// <summary>
        /// Let the native round controller turn the round over. The rend
        /// tracker is cleared by the game's own round boundary, not here.
        /// </summary>
        private IEnumerable<int> Sprint18AdvanceRound(UnitEntityData owner)
        {
            TurnController start = Game.Instance.TurnBasedCombatController.CurrentTurn;
            int frames = 0;
            while (++frames <= Sprint18AttackFrames)
            {
                if (Game.Instance.IsPaused) Game.Instance.IsPaused = false;
                if (CombatController.IsInTurnBasedCombat())
                {
                    TurnController turn =
                        Game.Instance.TurnBasedCombatController.CurrentTurn;
                    // End this unit's own turn too: a new round is what the
                    // rend tracker is scoped against, and only the native
                    // round boundary clears it.
                    if (turn != null && (turn.Status == TurnController.TurnStatus.Acting ||
                        turn.Status == TurnController.TurnStatus.Preparing))
                        Sprint18EndTurn(turn);
                }
                yield return 0;
                TurnController current =
                    Game.Instance.TurnBasedCombatController.CurrentTurn;
                if (!ReferenceEquals(current, start) &&
                    (current == null || ReferenceEquals(current.Unit, owner))) break;
                if (frames > 240 && !CombatController.IsInTurnBasedCombat()) break;
            }
        }
    }
}
