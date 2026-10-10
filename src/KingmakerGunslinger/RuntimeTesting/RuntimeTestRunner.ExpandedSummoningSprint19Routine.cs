using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker;
using Kingmaker.Controllers.Combat;
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
    /// The Sprint 19 printed routines, the four-claw rend and the Xill's
    /// paralysis, driven as real commands.
    ///
    /// <para>Every attack here is a native full attack issued against a
    /// disposable hostile and resolved by the native rulebook. The fixture
    /// decides only whether a given roll can reach the target, and - for the
    /// paralysis cases alone - whether this one disposable target's Fortitude
    /// save can succeed. Both are moved before the roll and put back after.
    /// Nothing writes a result, replays a rule or adds a damage event.</para>
    ///
    /// <para>The turn-order driver, the hit chooser, the bounded re-attempt on
    /// a natural one and the second-target builder are Sprint 18's and are
    /// reused rather than copied. Their names say Sprint 18 because that is
    /// where they were written and proved; what they do is creature-agnostic,
    /// and a second copy under a new name would be two things to keep
    /// right.</para>
    /// </summary>
    internal sealed partial class RuntimeTestRunner
    {
        /// <summary>
        /// Captures the saving throws this owner forced on this target, so the
        /// paralysis cases can state the difficulty class the game actually
        /// rolled against rather than the one the policy hoped for.
        /// </summary>
        private sealed class Sprint19SaveObserver :
            IGlobalRulebookHandler<RuleSavingThrow>
        {
            internal UnitEntityData Target;
            internal readonly List<RuleSavingThrow> Saves =
                new List<RuleSavingThrow>();

            public void OnEventAboutToTrigger(RuleSavingThrow evt) { }

            public void OnEventDidTrigger(RuleSavingThrow evt)
            {
                if (evt != null && ReferenceEquals(evt.Initiator, Target))
                    Saves.Add(evt);
            }
        }

        /// <summary>
        /// The full attack: exactly the printed limbs, each one primary, each
        /// one at full Strength and at its own printed attack bonus, in both
        /// combat modes.
        /// </summary>
        private IEnumerable<int> ReviewSprint19Routine(
            ExpandedSummoningCorrectionFixture fixture, UnitEntityData owner,
            string key, bool turnBased)
        {
            Sprint19LiveProfile expected = Sprint19ReviewPolicy.For(key);
            ItemEntityWeapon[] limbs = Sprint19Limbs(owner);
            var observer = new Sprint16RuleObserver {
                Owner = owner, Target = fixture.Hostile };
            var accuracy = new Sprint18Accuracy { Owner = owner, Hit = limbs };
            int restore = owner.Descriptor.Stats.AdditionalAttackBonus.BaseValue;
            observer.BeforeAttackRollForFixture = accuracy.Apply;
            EventBus.Subscribe(observer);
            UnitAttack attack = null;
            try
            {
                foreach (int step in Sprint18RunFullAttack(fixture, owner,
                    result => attack = result)) yield return step;
                var byLimb = new JArray();
                var counted = new Dictionary<int, int>();
                foreach (RuleAttackRoll roll in observer.Attacks)
                {
                    int index = Array.FindIndex(limbs, weapon =>
                        ReferenceEquals(weapon, roll.Weapon));
                    counted[index] = counted.ContainsKey(index) ?
                        counted[index] + 1 : 1;
                    byLimb.Add(new JObject {
                        ["limb"] = index,
                        ["weapon"] = roll.Weapon == null ? null :
                            roll.Weapon.Blueprint.AssetGuid,
                        ["attackBonus"] = roll.AttackBonus,
                        ["hit"] = roll.IsHit,
                        ["secondary"] = roll.Weapon != null &&
                            roll.Weapon.IsSecondary });
                }
                // Exactly the printed limbs, each exactly once, none of them
                // resolved as a secondary attack, in the mode asked for.
                bool exact = observer.Attacks.Count ==
                        Sprint19ReviewPolicy.ExpectedAttacksInAFullAttack(key) &&
                    limbs.Length == expected.Limbs.Length &&
                    !counted.ContainsKey(-1) &&
                    Enumerable.Range(0, limbs.Length).All(index =>
                        counted.ContainsKey(index) && counted[index] == 1) &&
                    observer.Attacks.All(roll => roll.Weapon != null &&
                        !roll.Weapon.IsSecondary) &&
                    CombatController.IsInTurnBasedCombat() == turnBased;
                Sprint19Check(_sprint19Assertions, _sprint19Rows,
                    key + (turnBased ? "-turn-based" : "-real-time") +
                        "-full-attack", exact,
                    new JObject {
                        ["mode"] = turnBased ? "turn-based" : "real-time",
                        ["observedMode"] = CombatController.IsInTurnBasedCombat(),
                        ["attacks"] = observer.Attacks.Count,
                        ["expected"] = Sprint19ReviewPolicy
                            .ExpectedAttacksInAFullAttack(key),
                        ["perLimb"] = byLimb,
                        ["commandFinished"] = attack != null && attack.IsFinished,
                    },
                    "one full attack is exactly the printed routine - a bite "
                    + "and four claws - every limb primary");
            }
            finally
            {
                EventBus.Unsubscribe(observer);
                owner.Descriptor.Stats.AdditionalAttackBonus.BaseValue = restore;
                accuracy.Restore = restore;
            }
        }

        /// <summary>
        /// The Girallon's rend, case by case. All four claws must land on one
        /// target in one sequence, and the rend may emit exactly once per
        /// sequence.
        ///
        /// <para>The case that matters most is three claws of four: a gate
        /// written for the Dire Ape's two would rend there, and the printed
        /// Girallon line does not.</para>
        /// </summary>
        private IEnumerable<int> ReviewSprint19Rend(
            ExpandedSummoningCorrectionFixture fixture, UnitEntityData owner,
            bool turnBased)
        {
            ItemEntityWeapon[] limbs = Sprint19Limbs(owner);
            ItemEntityWeapon bite = limbs[0];
            ItemEntityWeapon[] claws = limbs.Skip(1).ToArray();
            Sprint19LiveProfile expected = Sprint19ReviewPolicy.For(
                GirallonRulesPolicy.GirallonKey);
            int restore = owner.Descriptor.Stats.AdditionalAttackBonus.BaseValue;
            foreach (string[] scenario in Sprint19ReviewPolicy.RendCases)
            {
                string name = scenario[0];
                bool qualifies = scenario[1].StartsWith("qualifies",
                    StringComparison.Ordinal);
                if (!Sprint19ReviewPolicy.RendCaseRunsInThisMode(name, turnBased))
                {
                    // Recorded rather than silently skipped. In real time the
                    // engine merges a second attack on the same target into
                    // the live command - UnitAttack.TryMergeInto - so the two
                    // sequences are one command and the tracker is never
                    // asked to survive a round. Separating them needs a whole
                    // six-second idle round, through which the engine ends
                    // the fight and despawns the target's view; two guarded
                    // runs died there. The round-scoped behaviour is proved
                    // in turn-based combat, which has rounds.
                    Sprint19Check(_sprint19Assertions, _sprint19Rows,
                        "rend-" + name + "-real-time", true, new JObject {
                            ["mode"] = "real-time",
                            ["disposition"] = scenario[1],
                            ["ranInThisMode"] = false,
                            ["provedIn"] = "turn-based",
                            ["reason"] =
                                "In real time the engine merges a second "
                                + "attack on one target into the live command, "
                                + "so these two sequences are one command and "
                                + "this case cannot differ from "
                                + "claws-across-two-commands, which does run "
                                + "in both modes. Forcing them apart needs a "
                                + "six-second idle round, through which combat "
                                + "ends and the target's view is despawned.",
                        },
                        "a round-scoped rend case is proved in the mode that "
                        + "has rounds");
                    continue;
                }
                var observer = new Sprint16RuleObserver {
                    Owner = owner, Target = fixture.Hostile };
                var accuracy = new Sprint18Accuracy { Owner = owner };
                observer.BeforeAttackRollForFixture = accuracy.Apply;
                EventBus.Subscribe(observer);
                int sequences = name == "claws-across-two-commands" ||
                    name == "claws-across-two-turns" ||
                    name == "four-claws-split-targets" ||
                    name == "second-sequence-after-a-rend" ? 2 : 1;
                var emissions = new JArray();
                UnitEntityData secondTarget = null;
                Kingmaker.Blueprints.BlueprintUnit secondBlueprint = null;
                Sprint16RuleObserver secondObserver = null;
                try
                {
                    for (int sequence = 0; sequence < sequences; sequence++)
                    {
                        accuracy.Hit = Sprint19HitSet(name, sequence, bite, claws);
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
                            (secondObserver == null ? 0 :
                                secondObserver.Attacks.Count);
                        if (name == "four-claws-split-targets" && sequence == 0)
                        {
                            if (secondTarget == null)
                                secondTarget = Sprint18SecondTarget(fixture,
                                    out secondBlueprint);
                            secondObserver = new Sprint16RuleObserver {
                                Owner = owner, Target = secondTarget };
                            // The hit chooser has to follow the target the
                            // sequence is actually aimed at, or every limb
                            // lands and the case earns a rend it is not owed.
                            secondObserver.BeforeAttackRollForFixture =
                                accuracy.Apply;
                            EventBus.Subscribe(secondObserver);
                            foreach (int step in Sprint18RunFullAttack(fixture,
                                owner, result => ignored = result, secondTarget))
                                yield return step;
                        }
                        else
                            foreach (int step in Sprint18RunFullAttack(fixture,
                                owner, result => ignored = result))
                                yield return step;
                        // A natural one always misses, whatever the bonus, so a
                        // sequence whose chosen limbs did not all land is a roll
                        // of the dice and not a result. Re-attempt it, bounded,
                        // and record every attempt. Nothing is forced: the rolls
                        // stay native and only the precondition the case needs
                        // is waited for. Four chosen claws make this far more
                        // likely than Sprint 18's two, so the bound is wider.
                        int attempts = 1;
                        var rolls = new JArray(Sprint18Rolls(observer,
                            secondObserver, before));
                        while (attempts < 8 && Sprint18ChosenLimbMissed(
                            observer, secondObserver, before, accuracy))
                        {
                            attempts++;
                            int retryFrom = observer.Attacks.Count +
                                (secondObserver == null ? 0 :
                                    secondObserver.Attacks.Count);
                            if (CombatController.IsInTurnBasedCombat())
                                foreach (int step in Sprint18AdvanceRound(owner))
                                    yield return step;
                            accuracy.Rest();
                            UnitAttack retry = null;
                            foreach (int step in Sprint18RunFullAttack(fixture,
                                owner, result => retry = result,
                                name == "four-claws-split-targets" && sequence == 0
                                    ? secondTarget : null)) yield return step;
                            ignored = retry;
                            rolls = new JArray(Sprint18Rolls(observer,
                                secondObserver, retryFrom));
                            before = retryFrom;
                        }
                        emissions.Add(new JObject {
                            ["sequence"] = sequence,
                            ["chosenClaws"] = accuracy.Hit.Length,
                            ["attacks"] = observer.Attacks.Count,
                            ["hits"] = observer.Attacks.Count(roll => roll.IsHit),
                            ["attempts"] = attempts,
                            ["lastAttemptRolls"] = rolls,
                            ["commandFinished"] = ignored != null &&
                                ignored.IsFinished });
                    }
                    RuleDealDamage[] rends = observer.Damage.Concat(
                            secondObserver == null ?
                                Enumerable.Empty<RuleDealDamage>() :
                                secondObserver.Damage)
                        .Where(value => ReferenceEquals(value.Initiator, owner) &&
                            value.AttackRoll == null).ToArray();
                    int wanted = qualifies ?
                        (name == "second-sequence-after-a-rend" ? 2 : 1) : 0;
                    bool shape = rends.Length == wanted && rends.All(rend =>
                        rend.DamageBundle.First().Dice.Rolls ==
                            expected.RendRolls &&
                        (int)rend.DamageBundle.First().Dice.Dice ==
                            expected.RendDieSides &&
                        rend.DamageBundle.First().Bonus == expected.RendBonus);
                    Sprint19Check(_sprint19Assertions, _sprint19Rows,
                        "rend-" + name +
                            (turnBased ? "-turn-based" : "-real-time"),
                        shape, new JObject {
                            ["mode"] = turnBased ? "turn-based" : "real-time",
                            ["disposition"] = scenario[1],
                            ["clawsRequired"] = GirallonRulesPolicy.ClawCount,
                            ["expectedRends"] = wanted,
                            ["observedRends"] = rends.Length,
                            ["rendDamage"] = new JArray(rends.Select(rend =>
                                Sprint16DamageLine(rend.DamageBundle.First()))),
                            ["sequences"] = emissions,
                            ["trackerAfter"] =
                                DireApeRendGate.ObservedTracker(owner) == null ?
                                    "cleared" : "live",
                            ["secondTarget"] = secondTarget == null ? null :
                                secondTarget.UniqueId,
                            ["note"] = name != "four-claws-split-targets" ? null :
                                "A native full attack has one target, so two "
                                + "targets necessarily means two commands; this "
                                + "case therefore also crosses the sequence "
                                + "boundary.",
                            ["turnNote"] = !turnBased ? null :
                                "A native full attack costs the whole turn, so "
                                + "in turn-based combat every sequence is also a "
                                + "new turn. Only the real-time cases separate a "
                                + "new command from a new round.",
                        }, qualifies
                            ? "exactly one rend per qualifying sequence, at the "
                              + "live Strength"
                            : "no rend at all from a sequence that does not "
                              + "qualify");
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
        /// The Xill's paralysis: the bite paralyzes on a failed Fortitude save
        /// at the printed derived difficulty class, a claw never paralyzes, and
        /// a made save leaves the target free.
        ///
        /// <para>The fixture moves this one disposable target's Fortitude save
        /// to force each outcome, and puts it back. The difficulty class is not
        /// moved and not asserted from the policy alone: the review records the
        /// number the game actually rolled against.</para>
        /// </summary>
        private IEnumerable<int> ReviewSprint19Paralysis(
            ExpandedSummoningCorrectionFixture fixture, UnitEntityData owner,
            bool turnBased)
        {
            ItemEntityWeapon[] limbs = Sprint19Limbs(owner);
            ItemEntityWeapon bite = limbs[0];
            ItemEntityWeapon claw = limbs[1];
            int restoreAttack =
                owner.Descriptor.Stats.AdditionalAttackBonus.BaseValue;
            int restoreSave = fixture.Hostile.Descriptor.Stats
                .GetStat(StatType.SaveFortitude).BaseValue;
            foreach (string[] scenario in new[] {
                new[] { "bite-save-failed", "paralyzed" },
                new[] { "bite-save-made", "free" },
                new[] { "claw-only", "free" } })
            {
                string name = scenario[0];
                bool wantParalyzed = scenario[1] == "paralyzed";
                var observer = new Sprint16RuleObserver {
                    Owner = owner, Target = fixture.Hostile };
                var saves = new Sprint19SaveObserver { Target = fixture.Hostile };
                var accuracy = new Sprint18Accuracy {
                    Owner = owner,
                    Hit = name == "claw-only" ? new[] { claw } : new[] { bite } };
                observer.BeforeAttackRollForFixture = accuracy.Apply;
                EventBus.Subscribe(observer);
                EventBus.Subscribe(saves);
                try
                {
                    // Start from a free target. The first case correctly
                    // paralyzes it, and paralysis bounded to the summon's
                    // lifetime outlives the case that caused it, so without
                    // this the later cases measure the leftover rather than
                    // their own outcome. Request-local, on the disposable
                    // hostile only, and asserted rather than assumed.
                    Sprint19ClearParalysis(fixture.Hostile);
                    bool startedFree = !fixture.Hostile.Descriptor.State
                        .HasCondition(Kingmaker.UnitLogic.UnitCondition.Paralyzed);
                    // Only the disposable target's own save moves, and only to
                    // choose which side of the printed difficulty class this
                    // case is on. The difficulty class itself is the game's.
                    fixture.Hostile.Descriptor.Stats
                        .GetStat(StatType.SaveFortitude).BaseValue =
                            name == "bite-save-made" ? 100 : -100;
                    accuracy.Rest();
                    if (CombatController.IsInTurnBasedCombat())
                        foreach (int step in Sprint18AdvanceRound(owner)) yield return step;
                    UnitAttack ignored = null;
                    int before = observer.Attacks.Count;
                    foreach (int step in Sprint18RunFullAttack(fixture, owner,
                        result => ignored = result)) yield return step;
                    int attempts = 1;
                    while (attempts < 8 && Sprint18ChosenLimbMissed(
                        observer, null, before, accuracy))
                    {
                        attempts++;
                        before = observer.Attacks.Count;
                        if (CombatController.IsInTurnBasedCombat())
                            foreach (int step in Sprint18AdvanceRound(owner)) yield return step;
                        accuracy.Rest();
                        UnitAttack retry = null;
                        foreach (int step in Sprint18RunFullAttack(fixture, owner,
                            result => retry = result)) yield return step;
                        ignored = retry;
                    }
                    for (int settle = 0; settle < 8; settle++) yield return 0;
                    bool paralyzed = fixture.Hostile.Descriptor.State
                        .HasCondition(Kingmaker.UnitLogic.UnitCondition.Paralyzed);
                    RuleSavingThrow[] fortitude = saves.Saves.Where(save =>
                        save.StatType == StatType.SaveFortitude).ToArray();
                    int expectedDc = Sprint19ReviewPolicy.For(
                        XillRulesPolicy.XillKey).ParalysisDifficultyClass;
                    // A claw must force no save at all: paralysis rides the
                    // bite alone, and a claw that forced one would mean the
                    // carrier had been put on the wrong weapon.
                    bool dcExact = name == "claw-only"
                        ? fortitude.Length == 0
                        : fortitude.Length > 0 && fortitude.All(save =>
                            save.DifficultyClass == expectedDc);
                    bool shape = paralyzed == wantParalyzed && dcExact &&
                        startedFree;
                    Sprint19Check(_sprint19Assertions, _sprint19Rows,
                        "paralysis-" + name +
                            (turnBased ? "-turn-based" : "-real-time"),
                        shape, new JObject {
                            ["mode"] = turnBased ? "turn-based" : "real-time",
                            ["disposition"] = scenario[1],
                            ["startedFree"] = startedFree,
                            ["paralyzed"] = paralyzed,
                            ["expectedParalyzed"] = wantParalyzed,
                            ["fortitudeSaves"] = new JArray(fortitude.Select(save =>
                                new JObject {
                                    ["dc"] = save.DifficultyClass,
                                    ["passed"] = save.IsPassed })),
                            ["expectedDc"] = expectedDc,
                            ["attempts"] = attempts,
                            ["commandFinished"] = ignored != null &&
                                ignored.IsFinished,
                            // The honest bound, recorded on every run: the
                            // printed duration is 1d4 hours and a summon lives
                            // for rounds, so the condition is bounded to the
                            // summon's own lifetime.
                            ["printedDuration"] = "1d4 hours",
                            ["implementedDuration"] = "bounded to the summon lifetime",
                        }, wantParalyzed
                            ? "a bite that beats the printed Fortitude save "
                              + "paralyzes the target"
                            : "nothing is paralyzed when the save is made or "
                              + "when only a claw lands");
                }
                finally
                {
                    EventBus.Unsubscribe(observer);
                    EventBus.Unsubscribe(saves);
                    owner.Descriptor.Stats.AdditionalAttackBonus.BaseValue =
                        restoreAttack;
                    fixture.Hostile.Descriptor.Stats
                        .GetStat(StatType.SaveFortitude).BaseValue = restoreSave;
                    ResetExpandedSummoningHostile(fixture);
                }
                yield return 0;
            }
        }

        /// <summary>
        /// Remove any paralysis this review's own earlier case applied, so a
        /// case measures its own outcome. Only the disposable hostile is
        /// touched, and only this project's own paralysis buff is removed.
        /// </summary>
        private static void Sprint19ClearParalysis(UnitEntityData target)
        {
            if (target == null || target.Descriptor == null) return;
            foreach (Kingmaker.UnitLogic.Buffs.Buff buff in
                target.Descriptor.Buffs.Enumerable.ToArray())
            {
                if (buff == null || buff.Blueprint == null) continue;
                if (buff.Blueprint.name.IndexOf("Xill_Paralysis",
                        StringComparison.Ordinal) >= 0)
                    buff.Remove();
            }
        }

        /// <summary>Which limbs may reach the target in this sequence.</summary>
        private static ItemEntityWeapon[] Sprint19HitSet(string name,
            int sequence, ItemEntityWeapon bite, ItemEntityWeapon[] claws)
        {
            switch (name)
            {
                case "four-claws-one-target":
                case "second-sequence-after-a-rend":
                    return claws;
                case "three-claws-one-target":
                    return claws.Take(3).ToArray();
                case "two-claws-one-target":
                    return claws.Take(2).ToArray();
                case "one-claw-only":
                    return claws.Take(1).ToArray();
                case "four-claws-split-targets":
                    // Three claws land on another target and one lands here.
                    // Neither target is hit by all four, so neither is owed a
                    // rend, and the tracker is never cleared in between.
                    return sequence == 0 ? claws.Take(3).ToArray() :
                        claws.Skip(3).ToArray();
                case "bite-and-three-claws":
                    return new[] { bite }.Concat(claws.Take(3)).ToArray();
                case "claws-across-two-commands":
                case "claws-across-two-turns":
                    return sequence == 0 ? claws.Take(3).ToArray() :
                        claws.Skip(3).ToArray();
                default:
                    throw new ArgumentOutOfRangeException("name", name,
                        "unreviewed rend case");
            }
        }
    }
}
