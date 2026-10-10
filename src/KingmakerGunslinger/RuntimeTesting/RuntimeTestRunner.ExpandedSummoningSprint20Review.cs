using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Controllers.Combat;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.Items;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Commands;
using KingmakerGunslinger.Assets;
using KingmakerGunslinger.Summoning;
using TurnBased.Controllers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.RuntimeTesting
{
    /// <summary>
    /// The Sprint 20 hidden-candidate runtime review: what the Giant Scorpion
    /// actually is in a running game.
    ///
    /// <para>The fixture, the turn-order driver, the hit chooser and the
    /// bounded re-attempt on a natural one are Sprints 18 and 19's and are
    /// reused rather than copied. Their names say Sprint 18 because that is
    /// where they were written and proved; what they do is creature-agnostic.
    /// The fixture decides only which of this creature's own limbs may reach
    /// this disposable target, and - for the poison cases alone - which side
    /// of the printed difficulty class this one disposable target's Fortitude
    /// save falls on. Both move before the roll and go back after. Nothing
    /// writes a result, replays a rule or manufactures a save.</para>
    ///
    /// <para>Two things this review checks that Sprint 19's could not. Grab
    /// and poison ride different limbs of one creature, so a claw that
    /// poisons or a sting that seizes is a defect the review has to be able to
    /// see - and both gates are a weapon comparison a layer above the trigger,
    /// which looks right until something compares. And the printed manoeuvre
    /// defence against trip is twelve higher than the ordinary one, which has
    /// to be the eight-legged carrier doing its job once rather than two
    /// things each doing part of it.</para>
    ///
    /// <para>It also reads back what this creature has instead of skills. No
    /// Intelligence score means no ranks, so every printed total is ability
    /// plus racial bonus, and a single rank anywhere would put the creature
    /// above its stat block - which is how a Giant Ant once read Perception
    /// 7.</para>
    /// </summary>
    internal sealed partial class RuntimeTestRunner
    {
        private const int Sprint20SettleFrames = 240;

        private ExpandedSummoningCorrectionFixture _sprint20Fixture;
        private IEnumerator<int> _sprint20Steps;
        private readonly List<RuntimeTestAssertion> _sprint20Assertions =
            new List<RuntimeTestAssertion>();
        private readonly JArray _sprint20Rows = new JArray();
        private int _sprint20LoadingFrames;
        private bool _sprint20Complete;

        private void PollSprint20Review()
        {
            if (_sprint20Complete) return;
            try
            {
                if (_sprint20Steps == null)
                {
                    string loading;
                    if (ExpandedSummoningLoadingActive(out loading))
                    {
                        if (++_sprint20LoadingFrames <
                            ExpandedSummoningLoadingGateFrames) return;
                        throw new InvalidOperationException(
                            "Native loading did not settle: " + loading);
                    }
                    _sprint20Fixture = BeginExpandedSummoningCorrectionFixture(
                        "KMG_Runtime_Sprint20_Caster");
                    _sprint20Steps =
                        ReviewSprint20(_sprint20Fixture).GetEnumerator();
                }
                if (_sprint20Steps.MoveNext()) return;
            }
            catch (Exception error)
            {
                Sprint20Check("review-exception", false, new JObject {
                        ["exception"] =
                            DescribeExpandedSummoningCorrectionException(error) },
                    "the whole Sprint 20 review completes without an exception");
            }
            FinishSprint20Review();
        }

        private void Sprint20Check(string name, bool passed, JObject row,
            string expected)
        {
            row["case"] = name;
            row["passed"] = passed;
            _sprint20Rows.Add(row);
            _sprint20Assertions.Add(Assertion("sprint20-" + name, expected,
                row.ToString(Formatting.None), passed,
                "request-local live Giant Scorpion; native rules and the "
                + "registered execution"));
        }

        private IEnumerable<int> ReviewSprint20(
            ExpandedSummoningCorrectionFixture fixture)
        {
            bool pause = Game.Instance.IsPaused;
            bool turnBasedSetting = Kingmaker.UI.SettingsUI.SettingsRoot
                .Instance.EnableTurnBasedMode.CurrentValue;
            UnityEngine.Random.State random = UnityEngine.Random.state;
            try
            {
                Sprint20ReviewPolicy.Validate();
                Game.Instance.IsPaused = false;
                CreateExpandedSummoningCorrectionHostile(fixture);
                bool bodyRead = false;
                foreach (bool turnBased in Sprint20ReviewPolicy.CombatModes)
                {
                    Kingmaker.UI.SettingsUI.SettingsRoot.Instance
                        .EnableTurnBasedMode.CurrentValue = turnBased;
                    Game.Instance.TurnBasedCombatController.Activate();
                    UnitEntityData owner = CastExpandedSummoningOwnTier(
                        fixture, Sprint20ReviewPolicy.ScorpionKey);
                    SetExpandedSummoningBrainActive(owner, false);
                    int settle = 0;
                    while (++settle <= Sprint20SettleFrames)
                    {
                        if (Game.Instance.IsPaused) Game.Instance.IsPaused = false;
                        yield return 0;
                        if (owner.View != null &&
                            owner.Descriptor.State.CanAct) break;
                    }
                    try
                    {
                        // The body is read while the creature is freshly
                        // spawned and before anything has been taken off it.
                        if (!bodyRead) ReviewSprint20Body(owner);
                        // Measured with nothing held: a scorpion already
                        // holding its target also carries the separate
                        // maintain bonus, and the sum of two right numbers is
                        // a wrong answer. Sprint 14 learned that the hard way.
                        ReviewSprint20GrabCarrier(fixture, owner, turnBased);
                        ReviewSprint20MindImmunity(owner, turnBased);
                        // The riders, live, one limb at a time.
                        foreach (int step in ReviewSprint20Riders(fixture,
                            owner, turnBased)) yield return step;
                        // Now the creature has acted and is no longer
                        // flat-footed, which is the state a printed stat block
                        // describes. Sprint 18's first run read the profile
                        // before the creature had acted and measured it denied
                        // its Dexterity in three places.
                        ReviewSprint20TripDefence(fixture, owner, turnBased);
                        ReviewSprint20Profile(fixture, owner, turnBased);
                        // Last, because it takes the grab off this creature to
                        // measure the routine, and a creature that has just
                        // seized its target stops swinging.
                        foreach (int step in ReviewSprint20Routine(fixture,
                            owner, turnBased)) yield return step;
                    }
                    finally
                    {
                        ResetExpandedSummoningHostile(fixture);
                        Sprint20ClearVenom(fixture);
                        DisposeExpandedSummoningUnits(fixture.Created,
                            new[] { owner });
                    }
                    // Native views are destroyed at the end of a frame, so let
                    // the teardown run before the next cast.
                    for (int teardown = 0; teardown < 8; teardown++)
                        yield return 0;
                    bodyRead = true;
                }
            }
            finally
            {
                UnityEngine.Random.state = random;
                Kingmaker.UI.SettingsUI.SettingsRoot.Instance
                    .EnableTurnBasedMode.CurrentValue = turnBasedSetting;
                Game.Instance.TurnBasedCombatController.Activate();
                Game.Instance.IsPaused = pause;
            }
        }

        /// <summary>
        /// The printed stat block, read off the live creature.
        ///
        /// <para>This creature is unusually complete here: with no
        /// Intelligence score it has no ranks and no feats, so every number
        /// below falls out of hit dice, ability scores, size and racial
        /// bonuses, and the whole block can be checked rather than a sample of
        /// it.</para>
        /// </summary>
        private void ReviewSprint20Profile(
            ExpandedSummoningCorrectionFixture fixture, UnitEntityData owner,
            bool turnBased)
        {
            Sprint20ReviewPolicy.LiveProfile expected =
                Sprint20ReviewPolicy.Scorpion;
            var stats = owner.Descriptor.Stats;
            int[] scores = new[] { StatType.Strength, StatType.Dexterity,
                StatType.Constitution, StatType.Intelligence, StatType.Wisdom,
                StatType.Charisma }.Select(stat =>
                    stats.GetStat(stat).ModifiedValue).ToArray();

            ItemEntityWeapon[] limbs = Sprint19Limbs(owner);
            var limbRows = new JArray();
            bool limbsExact = limbs.Length == expected.Limbs.Length;
            for (int index = 0; index < limbs.Length; index++)
            {
                RuleCalculateWeaponStats calculated = Rulebook.Trigger(
                    new RuleCalculateWeaponStats(owner, limbs[index], null));
                BaseDamage damage = calculated.DamageDescription[0].CreateDamage();
                int attack = ProbeEntityAttackBonus(owner, fixture.Hostile,
                    limbs[index]);
                int[] want = index < expected.Limbs.Length ?
                    expected.Limbs[index] : new[] { 0, 0, 0, 0 };
                // Every limb primary, at one bonus, adding the whole Strength
                // modifier. A limb reading 1d6+6 against a printed 1d6+4 is
                // the engine's primary-hand rule reaching a creature the
                // full-Strength carrier is supposed to hold.
                bool exact = attack == want[3] &&
                    damage.Dice.Rolls == want[0] &&
                    (int)damage.Dice.Dice == want[1] &&
                    damage.Bonus == want[2] && !limbs[index].IsSecondary &&
                    !calculated.SecondaryWeapon;
                limbsExact &= exact;
                limbRows.Add(new JObject {
                    ["limb"] = index,
                    ["weapon"] = limbs[index].Blueprint.AssetGuid,
                    ["weaponName"] = limbs[index].Blueprint.name,
                    ["attackBonus"] = attack,
                    ["expectedAttackBonus"] = want[3],
                    ["damage"] = Sprint16DamageLine(damage),
                    ["secondaryEntity"] = limbs[index].IsSecondary,
                    ["secondaryRule"] = calculated.SecondaryWeapon,
                    ["matches"] = exact });
            }

            Sprint14CmdBreakdown cmd = ProbeCombatManeuverDefenceParts(
                fixture.Hostile, owner, CombatManeuver.BullRush);
            // The player's difficulty setting puts a Difficulty-descriptor
            // modifier on armour class, and the engine computes manoeuvre
            // defence from touch, so the same term reaches all four defensive
            // numbers exactly once. No creature in the game escapes it and no
            // stat block prints it, so it is subtracted and named. Sprint 18
            // identified this term; it is not a Sprint 20 finding.
            int difficulty = stats.AC.Modifiers.Where(modifier =>
                modifier.ModDescriptor == ModifierDescriptor.Difficulty)
                .Sum(modifier => modifier.ModValue);
            int deniedDex = cmd.DexterityDenied || cmd.FlatFooted ?
                stats.Dexterity.Bonus : 0;

            var skillRows = new JObject();
            bool skillsExact = true;
            foreach (KeyValuePair<string, int> want in expected.Skills)
            {
                StatType stat = Sprint20Skill(want.Key);
                int live = stats.GetStat(stat).ModifiedValue;
                skillsExact &= live == want.Value;
                skillRows[want.Key] = DescribeSprint16Skill(stats.GetStat(stat));
            }

            // No Intelligence score means no ranks. The builder's default
            // three would make this creature better than its stat block.
            bool noRanks = true;
            var rankRows = new JObject();
            foreach (StatType stat in new[] { StatType.SkillPerception,
                StatType.SkillStealth, StatType.SkillMobility,
                StatType.SkillAthletics })
            {
                int baseValue = stats.GetStat(stat).BaseValue;
                rankRows[stat.ToString()] = baseValue;
                noRanks &= baseValue == GiantScorpionRulesPolicy.SkillRanks;
            }

            // Nothing held, on both sides. A scorpion still holding its
            // target carries the separate maintain bonus on top of its grab,
            // and the printed +12 would read 17 - which is what the first
            // guarded run measured, out of two numbers that were each right.
            // ResetExpandedSummoningHostile clears the victim's side of a
            // hold; this clears the holder's.
            string released = Sprint20ReleaseOwnHold(fixture, owner);
            int grapple = Rulebook.Trigger(new RuleCalculateCMB(owner,
                fixture.Hostile, CombatManeuver.Grapple)).Result;
            int ordinaryCmb = Rulebook.Trigger(new RuleCalculateCMB(owner,
                fixture.Hostile, CombatManeuver.Trip)).Result;

            bool exactProfile = scores.SequenceEqual(new[] {
                    expected.Strength, expected.Dexterity,
                    expected.Constitution, expected.Intelligence,
                    expected.Wisdom, expected.Charisma }) &&
                owner.Descriptor.State.Size == Size.Large &&
                owner.Descriptor.Progression.CharacterLevel == expected.HitDice &&
                stats.HitPoints.ModifiedValue == expected.HitPoints &&
                stats.AC.ModifiedValue - difficulty == expected.ArmorClass &&
                stats.AC.Touch - difficulty == expected.Touch &&
                stats.AC.FlatFooted - difficulty == expected.FlatFooted &&
                stats.GetStat(StatType.SaveFortitude).ModifiedValue ==
                    expected.Fortitude &&
                stats.GetStat(StatType.SaveReflex).ModifiedValue ==
                    expected.Reflex &&
                stats.GetStat(StatType.SaveWill).ModifiedValue == expected.Will &&
                cmd.Result + deniedDex - difficulty ==
                    expected.CombatManeuverDefense &&
                ordinaryCmb == GiantScorpionRulesPolicy
                    .PrintedCombatManeuverBonus &&
                grapple == GiantScorpionRulesPolicy.PrintedGrappleBonus &&
                grapple - ordinaryCmb ==
                    GiantScorpionRulesPolicy.GrabGrappleBonus &&
                released.StartsWith("nothing-held",
                    StringComparison.Ordinal) &&
                skillsExact && limbsExact && noRanks;
            Sprint20Check("live-profile-" +
                    (turnBased ? "turn-based" : "real-time"), exactProfile,
                new JObject {
                    ["mode"] = turnBased ? "turn-based" : "real-time",
                    ["scores"] = new JArray(scores),
                    ["size"] = owner.Descriptor.State.Size.ToString(),
                    ["hitDice"] = owner.Descriptor.Progression.CharacterLevel,
                    ["hitPoints"] = stats.HitPoints.ModifiedValue,
                    ["armor"] = stats.AC.ModifiedValue,
                    ["touch"] = stats.AC.Touch,
                    ["flatFooted"] = stats.AC.FlatFooted,
                    ["nativeDifficultyTerm"] = difficulty,
                    ["armorNetOfDifficulty"] = stats.AC.ModifiedValue - difficulty,
                    ["touchNetOfDifficulty"] = stats.AC.Touch - difficulty,
                    ["flatFootedNetOfDifficulty"] =
                        stats.AC.FlatFooted - difficulty,
                    ["fortitude"] =
                        stats.GetStat(StatType.SaveFortitude).ModifiedValue,
                    ["reflex"] = stats.GetStat(StatType.SaveReflex).ModifiedValue,
                    ["will"] = stats.GetStat(StatType.SaveWill).ModifiedValue,
                    ["cmd"] = cmd.Describe("CMD"),
                    ["deniedDexRecovered"] = deniedDex,
                    ["cmdNetOfDifficulty"] = cmd.Result + deniedDex - difficulty,
                    ["maneuverBonus"] = ordinaryCmb,
                    ["grappleBonus"] = grapple,
                    ["holdReleasedBeforeReading"] = released,
                    ["expectedManeuverBonus"] = GiantScorpionRulesPolicy
                        .PrintedCombatManeuverBonus,
                    ["expectedGrappleBonus"] = GiantScorpionRulesPolicy
                        .PrintedGrappleBonus,
                    ["skills"] = skillRows,
                    ["skillRanks"] = rankRows,
                    ["limbs"] = limbRows,
                    ["expectedSpeed"] = expected.SpeedFeet,
                    // What this creature does not represent, as numbers rather
                    // than silences. Nothing stands in for either.
                    ["printedClimbOmitted"] =
                        GiantScorpionRulesPolicy.PrintedClimbSkill,
                    ["athleticsUnraised"] =
                        stats.GetStat(StatType.SkillAthletics).BaseValue,
                    ["printedClawReachUnrepresented"] =
                        GiantScorpionRulesPolicy.PrintedClawReachFeet,
                    ["unitType"] = owner.Blueprint.Type == null ? null :
                        owner.Blueprint.Type.name },
                "every printed Giant Scorpion number reads back from the live "
                + "creature, with no rank it does not print");
        }

        /// <summary>
        /// The Kingmaker skill a printed Giant Scorpion skill is represented
        /// by. Two are reachable; a name outside them is a review bug.
        /// </summary>
        private static StatType Sprint20Skill(string name)
        {
            switch (name)
            {
                case "Perception": return StatType.SkillPerception;
                case "Stealth": return StatType.SkillStealth;
                default:
                    throw new ArgumentOutOfRangeException("name", name,
                        "Sprint 20 represents two Kingmaker skills.");
            }
        }

        /// <summary>
        /// This creature's limbs, each identified by its own blueprint rather
        /// than by where it sits in a list.
        ///
        /// <para>The order happens to be claw, claw, sting today, because that
        /// is the order the profile lists them in. A review that read a sting
        /// as a claw because the order moved would report a defect in the
        /// creature rather than in itself, so the order is checked and then
        /// not relied on.</para>
        /// </summary>
        private static bool Sprint20IsClaw(ItemEntityWeapon limb)
        {
            return limb != null && limb.Blueprint != null &&
                limb.Blueprint.name ==
                    "KMG_Summoning_Natural_GiantScorpion_Claw1d6";
        }

        private static bool Sprint20IsSting(ItemEntityWeapon limb)
        {
            return limb != null && limb.Blueprint != null &&
                limb.Blueprint.name ==
                    "KMG_Summoning_Natural_GiantScorpion_Sting1d6";
        }

        /// <summary>
        /// The grab carrier, before anything is held: which limbs grab, how
        /// much it is worth, and how many foes it can hold.
        ///
        /// <para>A grab bonus applies to the grapple and not to the trip, so
        /// the difference between the two is the bonus itself - and it is the
        /// whole of the printed difference between CMB +8 and grapple +12.
        /// Read with nothing held, because a creature already holding its
        /// target also carries the separate maintain bonus.</para>
        /// </summary>
        private void ReviewSprint20GrabCarrier(
            ExpandedSummoningCorrectionFixture fixture, UnitEntityData owner,
            bool turnBased)
        {
            SummonGrabComponent grab = SummonGrabComponent.Find(owner);
            ItemEntityWeapon[] limbs = Sprint19Limbs(owner);
            int delta = Rulebook.Trigger(new RuleCalculateCMB(owner,
                    fixture.Hostile, CombatManeuver.Grapple)).Result -
                Rulebook.Trigger(new RuleCalculateCMB(owner, fixture.Hostile,
                    CombatManeuver.Trip)).Result;
            var limbRows = new JArray();
            int clawsThatGrab = 0, stingsThatGrab = 0;
            for (int index = 0; index < limbs.Length; index++)
            {
                bool grabs = grab != null && grab.IsGrabLimb(owner, limbs[index]);
                if (grabs && Sprint20IsClaw(limbs[index])) clawsThatGrab++;
                if (grabs && Sprint20IsSting(limbs[index])) stingsThatGrab++;
                limbRows.Add(new JObject {
                    ["limb"] = index,
                    ["weaponName"] = limbs[index].Blueprint.name,
                    ["claw"] = Sprint20IsClaw(limbs[index]),
                    ["sting"] = Sprint20IsSting(limbs[index]),
                    ["grabs"] = grabs });
            }
            // Both claws grab and the sting does not. Counted by what each
            // limb IS rather than by where it sits, because the spec that
            // produces this counts limbs and a changed order would make a
            // right spec look wrong.
            bool shape = grab != null && limbs.Length == 3 &&
                clawsThatGrab == GiantScorpionRulesPolicy.ClawCount &&
                stingsThatGrab == 0 &&
                limbs.Count(Sprint20IsClaw) == GiantScorpionRulesPolicy.ClawCount &&
                limbs.Count(Sprint20IsSting) == GiantScorpionRulesPolicy.StingCount &&
                grab.MaxHeldTargets == GiantScorpionRulesPolicy.ClawCount &&
                grab.RakeLimbCount == 0 &&
                delta == GiantScorpionRulesPolicy.GrabGrappleBonus &&
                delta == GiantScorpionRulesPolicy.PrintedGrappleBonus -
                    GiantScorpionRulesPolicy.PrintedCombatManeuverBonus;
            Sprint20Check("grab-rides-the-claws-" +
                    (turnBased ? "turn-based" : "real-time"), shape,
                new JObject {
                    ["mode"] = turnBased ? "turn-based" : "real-time",
                    ["carrierPresent"] = grab != null,
                    ["limbs"] = limbRows,
                    ["clawsThatGrab"] = clawsThatGrab,
                    ["stingsThatGrab"] = stingsThatGrab,
                    ["maxHeld"] = grab == null ? 0 : grab.MaxHeldTargets,
                    ["rakeLimbs"] = grab == null ? -1 : grab.RakeLimbCount,
                    ["grappleLessTrip"] = delta,
                    ["expectedDelta"] =
                        GiantScorpionRulesPolicy.GrabGrappleBonus },
                "grab is carried by both claws and by neither the sting nor "
                + "anything else, and is worth the printed grapple difference");
        }

        /// <summary>
        /// The riders, live and one limb at a time. A claw that lands may
        /// seize and must never poison; a sting that lands must poison at the
        /// live difficulty class and must never seize.
        /// </summary>
        private IEnumerable<int> ReviewSprint20Riders(
            ExpandedSummoningCorrectionFixture fixture, UnitEntityData owner,
            bool turnBased)
        {
            ItemEntityWeapon[] limbs = Sprint19Limbs(owner);
            ItemEntityWeapon claw = limbs.FirstOrDefault(Sprint20IsClaw);
            ItemEntityWeapon sting = limbs.FirstOrDefault(Sprint20IsSting);
            if (claw == null || sting == null)
                throw new InvalidOperationException(
                    "The live Giant Scorpion carries no claw or no sting: " +
                    string.Join(", ", limbs.Select(limb =>
                        limb.Blueprint.name).ToArray()));
            BlueprintBuff venom = Sprint20Venom(fixture);
            SummonGrabComponent grab = SummonGrabComponent.Find(owner);
            int restoreAttack =
                owner.Descriptor.Stats.AdditionalAttackBonus.BaseValue;
            int restoreSave = fixture.Hostile.Descriptor.Stats
                .GetStat(StatType.SaveFortitude).BaseValue;
            foreach (string[] scenario in Sprint20ReviewPolicy.RiderCases)
            {
                string name = scenario[0];
                bool stings = scenario[1] == "sting";
                bool saveMade = scenario[2] == "save-made";
                var observer = new Sprint16RuleObserver {
                    Owner = owner, Target = fixture.Hostile };
                var saves = new Sprint19SaveObserver { Target = fixture.Hostile };
                var accuracy = new Sprint18Accuracy {
                    Owner = owner,
                    Hit = new[] { stings ? sting : claw } };
                observer.BeforeAttackRollForFixture = accuracy.Apply;
                EventBus.Subscribe(observer);
                EventBus.Subscribe(saves);
                try
                {
                    // Start from a clean target: a hold and a venom both
                    // outlive the case that caused them, and without this a
                    // later case measures the leftover rather than its own
                    // outcome. Request-local, on the disposable hostile only,
                    // and asserted rather than assumed.
                    ResetExpandedSummoningHostile(fixture);
                    Sprint20ClearVenom(fixture);
                    bool startedClean = !Sprint20Poisoned(fixture, venom) &&
                        SummonHeldComponent.HolderOf(fixture.Hostile,
                            grab == null ? null : grab.GrappledBuff) == null;
                    // Only the disposable target's own save moves, and only to
                    // choose which side of the printed difficulty class this
                    // case falls on. The class itself is the game's.
                    fixture.Hostile.Descriptor.Stats
                        .GetStat(StatType.SaveFortitude).BaseValue =
                            saveMade ? 100 : -100;
                    accuracy.Rest();
                    if (CombatController.IsInTurnBasedCombat())
                        foreach (int step in Sprint18AdvanceRound(owner))
                            yield return step;
                    UnitAttack attack = null;
                    int before = observer.Attacks.Count;
                    foreach (int step in Sprint18RunFullAttack(fixture, owner,
                        result => attack = result)) yield return step;
                    int attempts = 1;
                    int savesBefore = 0;
                    int rollsBefore = 0;
                    while (attempts < 8 && (Sprint18ChosenLimbMissed(
                            observer, null, before, accuracy) ||
                        Sprint20UnchosenLimbLanded(observer, rollsBefore,
                            accuracy) ||
                        Sprint20SaveRolledTwenty(saves, savesBefore, saveMade)))
                    {
                        attempts++;
                        before = observer.Attacks.Count;
                        rollsBefore = observer.Attacks.Count;
                        savesBefore = saves.Saves.Count;
                        if (CombatController.IsInTurnBasedCombat())
                            foreach (int step in Sprint18AdvanceRound(owner))
                                yield return step;
                        accuracy.Rest();
                        Sprint20ClearVenom(fixture);
                        UnitAttack retry = null;
                        foreach (int step in Sprint18RunFullAttack(fixture,
                            owner, result => retry = result)) yield return step;
                        attack = retry;
                    }
                    for (int settle = 0; settle < 8; settle++) yield return 0;

                    bool poisoned = Sprint20Poisoned(fixture, venom);
                    UnitEntityData holder = SummonHeldComponent.HolderOf(
                        fixture.Hostile,
                        grab == null ? null : grab.GrappledBuff);
                    bool held = ReferenceEquals(holder, owner);
                    // This creature's own attempts only. The observer
                    // records manoeuvres initiated by the owner OR the target,
                    // so a target that grappled back would otherwise read as
                    // the sting having seized something.
                    RuleCombatManeuver[] grapples = observer.Checks.Where(
                        check => check != null &&
                        check.Type == CombatManeuver.Grapple &&
                        ReferenceEquals(check.Initiator, owner)).ToArray();
                    RuleSavingThrow[] fortitude = saves.Saves.Where(save =>
                        save.StatType == StatType.SaveFortitude).ToArray();
                    int expectedDc = GiantScorpionRulesPolicy
                        .PoisonDifficultyClass(
                            GiantScorpionRulesPolicy.HitDice,
                            owner.Descriptor.Stats.Constitution.Bonus);
                    // A claw must force no Fortitude save at all, and a sting
                    // must attempt no grapple at all. Each of those is a rider
                    // having been put on the wrong weapon, and neither shows up
                    // when only the other direction is looked at.
                    bool riders = stings
                        ? grapples.Length == 0 && !held &&
                            fortitude.Length > 0 && fortitude.All(save =>
                                save.DifficultyClass == expectedDc) &&
                            poisoned == !saveMade
                        : fortitude.Length == 0 && !poisoned &&
                            grapples.Length > 0;
                    bool ok = riders && startedClean;
                    Sprint20Check("rider-" + name +
                            (turnBased ? "-turn-based" : "-real-time"), ok,
                        new JObject {
                            ["mode"] = turnBased ? "turn-based" : "real-time",
                            ["limb"] = stings ? "sting" : "claw",
                            ["disposition"] = scenario[3],
                            ["startedClean"] = startedClean,
                            ["attempts"] = attempts,
                            ["abandonedTrials"] = attempts - 1,
                            ["scoredTrialRolls"] = new JArray(
                                Sprint18AttemptRolls(observer, null, before)
                                    .Select(roll => new JObject {
                                        ["weapon"] = roll.Weapon == null ? null :
                                            roll.Weapon.Blueprint.name,
                                        ["hit"] = roll.IsHit,
                                        ["chosen"] = roll.Weapon != null &&
                                            accuracy.Hit.Any(chosen =>
                                                ReferenceEquals(chosen,
                                                    roll.Weapon)) })),
                            ["rolls"] = new JArray(
                                Sprint18AttemptRolls(observer, null, 0)
                                    .Select(roll => new JObject {
                                        ["weapon"] = roll.Weapon == null ? null :
                                            roll.Weapon.Blueprint.name,
                                        ["hit"] = roll.IsHit })),
                            ["grappleAttempts"] = grapples.Length,
                            ["heldByThisScorpion"] = held,
                            ["holder"] = holder == null ? null :
                                holder.Blueprint == null ? "?" :
                                holder.Blueprint.name,
                            ["fortitudeSaves"] = new JArray(fortitude.Select(
                                save => new JObject {
                                    ["dc"] = save.DifficultyClass,
                                    ["naturalRoll"] = save.D20.Value,
                                    ["passed"] = save.IsPassed })),
                            ["expectedDc"] = expectedDc,
                            ["printedDc"] = GiantScorpionRulesPolicy
                                .PrintedPoisonDifficultyClass,
                            ["poisoned"] = poisoned,
                            ["expectedPoisoned"] = stings && !saveMade,
                            ["commandFinished"] = attack != null &&
                                attack.IsFinished,
                            // The printed graph, recorded on every run: six
                            // rounds of 1d2 Strength, cured by one save. Six
                            // is why this creature needed its own carrier.
                            ["printedPoison"] =
                                GiantScorpionRulesPolicy.PoisonDamage + " " +
                                GiantScorpionRulesPolicy.PoisonDamagedStat +
                                " for " + GiantScorpionRulesPolicy.PoisonRounds +
                                " rounds, cure " +
                                GiantScorpionRulesPolicy.PoisonCureSaves +
                                " save" },
                        stings
                            ? "a sting that lands forces the printed Fortitude "
                              + "save and seizes nothing"
                            : "a claw that lands may seize and forces no "
                              + "Fortitude save at all");
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
                    Sprint20ClearVenom(fixture);
                }
                yield return 0;
            }
        }

        /// <summary>
        /// The printed routine: two claws and a sting, each exactly once, none
        /// of them secondary, all three at one bonus.
        ///
        /// <para>The grab comes off this creature first. A creature that has
        /// just seized its target does not go on swinging - Sprint 14 proved
        /// that on the Giant Ant Soldier - so with the grab live a sequence
        /// can legitimately end after one claw, and the routine could then
        /// never be measured whole. The carrier is removed from this one
        /// request-local creature, which is about to be dismissed, and the
        /// removal is recorded and asserted rather than assumed.</para>
        /// </summary>
        private IEnumerable<int> ReviewSprint20Routine(
            ExpandedSummoningCorrectionFixture fixture, UnitEntityData owner,
            bool turnBased)
        {
            BlueprintBuff traits = fixture.Blueprints.OfType<BlueprintBuff>()
                .FirstOrDefault(value => value != null && value.name ==
                    "KMG_Summoning_Special_GiantScorpion_Traits");
            bool carried = traits != null &&
                owner.Descriptor.Buffs.GetBuff(traits) != null;
            if (carried) owner.Descriptor.Buffs.RemoveFact(traits);
            bool removed = traits != null &&
                owner.Descriptor.Buffs.GetBuff(traits) == null;
            bool grabGone = SummonGrabComponent.Find(owner) == null;

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
                ResetExpandedSummoningHostile(fixture);
                Sprint20ClearVenom(fixture);
                if (CombatController.IsInTurnBasedCombat())
                    foreach (int step in Sprint18AdvanceRound(owner))
                        yield return step;
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
                        ["weaponName"] = roll.Weapon == null ? null :
                            roll.Weapon.Blueprint.name,
                        ["attackBonus"] = roll.AttackBonus,
                        ["hit"] = roll.IsHit,
                        ["secondary"] = roll.Weapon != null &&
                            roll.Weapon.IsSecondary });
                }
                // Exactly the printed limbs, each exactly once, none resolved
                // as a secondary attack, all at one bonus, in the mode asked
                // for.
                bool exact = carried && removed && grabGone &&
                    observer.Attacks.Count == limbs.Length &&
                    limbs.Length == GiantScorpionRulesPolicy.ClawCount +
                        GiantScorpionRulesPolicy.StingCount &&
                    !counted.ContainsKey(-1) &&
                    Enumerable.Range(0, limbs.Length).All(index =>
                        counted.ContainsKey(index) && counted[index] == 1) &&
                    observer.Attacks.All(roll => roll.Weapon != null &&
                        !roll.Weapon.IsSecondary) &&
                    observer.Attacks.Select(roll => roll.AttackBonus)
                        .Distinct().Count() == 1 &&
                    CombatController.IsInTurnBasedCombat() == turnBased;
                Sprint20Check("full-attack-" +
                        (turnBased ? "turn-based" : "real-time"), exact,
                    new JObject {
                        ["mode"] = turnBased ? "turn-based" : "real-time",
                        ["observedMode"] = CombatController.IsInTurnBasedCombat(),
                        ["attacks"] = observer.Attacks.Count,
                        ["expected"] = limbs.Length,
                        ["perLimb"] = byLimb,
                        ["commandFinished"] = attack != null && attack.IsFinished,
                        // Why the grab is off for this one case, recorded on
                        // every run rather than left in a comment.
                        ["grabCarried"] = carried,
                        ["grabRemoved"] = removed,
                        ["grabComponentGone"] = grabGone,
                        ["reason"] =
                            "A creature that has just seized its target stops "
                            + "swinging, so the printed routine is measured "
                            + "with the grab carrier off this one "
                            + "request-local creature. The grab itself is "
                            + "proved in the rider cases above, with the "
                            + "carrier live." },
                    "one full attack is exactly the printed routine - two "
                    + "claws and a sting - every limb primary at one bonus");
            }
            finally
            {
                EventBus.Unsubscribe(observer);
                owner.Descriptor.Stats.AdditionalAttackBonus.BaseValue = restore;
            }
        }

        /// <summary>
        /// The printed manoeuvre defence against trip, which is twelve higher
        /// than the ordinary one. Measured against the engine's own trip
        /// manoeuvre rather than inferred from a feature being present.
        /// </summary>
        private void ReviewSprint20TripDefence(
            ExpandedSummoningCorrectionFixture fixture, UnitEntityData owner,
            bool turnBased)
        {
            Sprint20ReviewPolicy.LiveProfile expected =
                Sprint20ReviewPolicy.Scorpion;
            var stats = owner.Descriptor.Stats;
            int difficulty = stats.AC.Modifiers.Where(modifier =>
                modifier.ModDescriptor == ModifierDescriptor.Difficulty)
                .Sum(modifier => modifier.ModValue);
            Sprint14CmdBreakdown ordinary = ProbeCombatManeuverDefenceParts(
                fixture.Hostile, owner, CombatManeuver.BullRush);
            Sprint14CmdBreakdown trip = ProbeCombatManeuverDefenceParts(
                fixture.Hostile, owner, CombatManeuver.Trip);
            int ordinaryDex = ordinary.DexterityDenied || ordinary.FlatFooted ?
                stats.Dexterity.Bonus : 0;
            int tripDex = trip.DexterityDenied || trip.FlatFooted ?
                stats.Dexterity.Bonus : 0;
            int liveOrdinary = ordinary.Result + ordinaryDex - difficulty;
            int liveTrip = trip.Result + tripDex - difficulty;
            bool exact = liveOrdinary == expected.CombatManeuverDefense &&
                liveTrip == expected.CombatManeuverDefenseVersusTrip &&
                liveTrip - liveOrdinary ==
                    GiantScorpionRulesPolicy.EightLegTripBonus;
            Sprint20Check("eight-legged-trip-defence-" +
                    (turnBased ? "turn-based" : "real-time"), exact,
                new JObject {
                    ["mode"] = turnBased ? "turn-based" : "real-time",
                    ["ordinary"] = ordinary.Describe("CMD"),
                    ["versusTrip"] = trip.Describe("CMD vs trip"),
                    ["liveOrdinary"] = liveOrdinary,
                    ["liveVersusTrip"] = liveTrip,
                    ["expectedOrdinary"] = expected.CombatManeuverDefense,
                    ["expectedVersusTrip"] =
                        expected.CombatManeuverDefenseVersusTrip,
                    ["eightLegBonus"] =
                        GiantScorpionRulesPolicy.EightLegTripBonus },
                "the printed anti-trip defence is the ordinary one plus the "
                + "eight-legged bonus, applied once");
        }

        /// <summary>
        /// The printed immunity to mind-affecting effects.
        ///
        /// <para>Carried as its own fact rather than inferred from an absent
        /// Intelligence score, because Kingmaker cannot hold an absent one and
        /// this creature therefore ships at Intelligence 1 - which the engine
        /// does not treat as mindless. Without the fact the printed immunity
        /// would simply be missing, and nothing else about the creature would
        /// look wrong.</para>
        /// </summary>
        private void ReviewSprint20MindImmunity(UnitEntityData owner,
            bool turnBased)
        {
            BlueprintFeature immunity = _sprint20Fixture.Blueprints
                .OfType<BlueprintFeature>().FirstOrDefault(value =>
                    value != null && value.name ==
                    "KMG_Summoning_Natural_GiantScorpion_MindlessImmunity");
            bool carries = immunity != null &&
                owner.Descriptor.HasFact(immunity);
            // Read from the component the rules layer actually consults. The
            // fact being present proves the creature was granted something;
            // only the descriptor on that component says the something is an
            // immunity to mind-affecting effects. The first version of this
            // also asked whether the creature was confused, which it never
            // was, so that half asserted nothing.
            BuffDescriptorImmunity[] parts =
                immunity == null || immunity.ComponentsArray == null
                    ? new BuffDescriptorImmunity[0]
                    : immunity.ComponentsArray
                        .OfType<BuffDescriptorImmunity>().ToArray();
            bool mindAffecting = parts.Length == 1 &&
                parts[0].Descriptor == SpellDescriptor.MindAffecting &&
                !parts[0].CheckFact;
            bool notConfused = !owner.Descriptor.State.HasCondition(
                Kingmaker.UnitLogic.UnitCondition.Confusion);
            Sprint20Check("mindless-immunity-" +
                    (turnBased ? "turn-based" : "real-time"),
                carries && mindAffecting, new JObject {
                    ["mode"] = turnBased ? "turn-based" : "real-time",
                    ["carriesFact"] = carries,
                    ["factName"] = immunity == null ? null : immunity.name,
                    ["intelligence"] = owner.Descriptor.Stats
                        .GetStat(StatType.Intelligence).ModifiedValue,
                    ["substitutedIntelligence"] =
                        GiantScorpionRulesPolicy.SubstitutedIntelligence,
                    ["notConfused"] = notConfused,
                    ["immunityComponents"] = parts.Length,
                    ["descriptor"] = parts.Length == 1 ?
                        parts[0].Descriptor.ToString() : "absent",
                    ["unconditional"] = parts.Length == 1 && !parts[0].CheckFact },
                "the printed immunity to mind-affecting effects is carried as "
                + "its own fact, never inferred from Intelligence 1");
        }

        /// <summary>
        /// The original body: the live creature wears its own mesh on the
        /// donor rig, the shipped geometry binds all eight legs, and the
        /// metasoma is carried by the abdomen chain.
        /// </summary>
        private void ReviewSprint20Body(UnitEntityData owner)
        {
            string outcome = owner.View == null ? "no-view" :
                ExpandedSummoningPteranodonViewPatch.DescribeView(owner.View);
            string status;
            UnityEngine.Mesh mesh;
            string[] bones;
            UnityEngine.Texture2D albedo;
            bool loaded = PteranodonAssetRuntime.TryGetSprint14InsectVisual(
                Sprint20ReviewPolicy.ScorpionKey, out mesh, out bones,
                out albedo, out status);
            // Eight legs, measured from the bones the shipped mesh actually
            // binds rather than from a manifest field. This is the first
            // creature in the project to weight all four chains a side as real
            // legs, so it is the one thing most worth reading back.
            var names = new HashSet<string>(bones ?? new string[0],
                StringComparer.Ordinal);
            int legs = new[] { 0, 1, 2, 3 }.Count(index =>
                names.Contains("L_Foot" + index) &&
                names.Contains("R_Foot" + index) &&
                names.Contains("L_Leg" + index + "_Upper") &&
                names.Contains("R_Leg" + index + "_Upper"));
            bool metasoma = names.Contains("Tail3_M") &&
                names.Contains("UpperTorso") && !names.Contains("Tail0_M") &&
                !names.Contains("Tail2_M") && !names.Contains("Tail4_M");
            bool chelae = names.Contains("pedipalp7_L") &&
                names.Contains("pedipalp7_R");
            string[] reviewedBones = Sprint14BonePolicy.AllowedBones(
                Sprint20ReviewPolicy.ScorpionKey);
            bool reviewed = bones != null && bones.All(name =>
                reviewedBones.Contains(name, StringComparer.Ordinal));
            bool attached = outcome.StartsWith("visual:attached;",
                StringComparison.Ordinal);
            Sprint20Check("original-body", loaded && attached && legs == 4 &&
                    metasoma && chelae && reviewed && mesh != null &&
                    mesh.vertexCount > 0,
                new JObject {
                    ["outcome"] = outcome,
                    ["status"] = status,
                    ["renderers"] = owner.View == null ? null :
                        DescribePteranodonRenderers(owner.View),
                    ["vertices"] = mesh == null ? 0 : mesh.vertexCount,
                    ["bones"] = bones == null ? 0 : bones.Length,
                    ["legChainsWeighted"] = legs,
                    ["metasomaOnAbdomenChain"] = metasoma,
                    ["chelaeOnPedipalps"] = chelae,
                    ["everyBoneReviewed"] = reviewed,
                    ["albedo"] = albedo == null ? null :
                        albedo.width + "x" + albedo.height,
                    // The measured limitation, recorded on every run rather
                    // than left in a design document.
                    ["abdomenChainDrivers"] = 2,
                    ["metasomaSegmentsPrinted"] = 5,
                    ["limitation"] = "METASOMA_DRIVEN_BY_A_TWO_BONE_CHAIN" },
                "the live creature wears its own body, binds all eight legs, "
                + "carries its tail on the abdomen chain and weights no bone "
                + "outside its reviewed list");
        }

        /// <summary>
        /// The registered surface: twelve roots live, every one withheld, and
        /// nothing v0.0.148 published moved.
        /// </summary>
        private void ReviewSprint20Surface()
        {
            try
            {
                KingmakerGunslinger.Blueprints.BlueprintManifest manifest =
                    KingmakerGunslinger.Blueprints.BlueprintManifest.Load(
                        _context.ModEntry.Path);
                SummonVariantSpec[] all = ExpandedSummoningCatalog
                    .GenerateVariants(SummonFamily.Monster)
                    .Concat(ExpandedSummoningCatalog.GenerateVariants(
                        SummonFamily.NaturesAlly)).ToArray();
                SummonVariantSpec[] mine = all.Where(value =>
                    Sprint20ReviewPolicy.IsSprint20Creature(value.Creature.Key))
                    .ToArray();
                var rows = new JArray();
                var missing = new List<string>();
                var published = new List<string>();
                var withheld = new List<string>();
                foreach (SummonVariantSpec variant in mine
                    .OrderBy(value => value.StableKey, StringComparer.Ordinal))
                {
                    string symbol = ExpandedSummoningIdentityCatalog
                        .AbilitySymbol(variant);
                    BlueprintAbility ability =
                        ResolveSprint18Ability(manifest, symbol);
                    bool hidden = !SummonVisibilityCatalog.IsPublished(variant);
                    if (ability == null) missing.Add(variant.StableKey);
                    if (hidden) withheld.Add(variant.StableKey);
                    else published.Add(variant.StableKey);
                    rows.Add(new JObject {
                        ["root"] = variant.StableKey,
                        ["family"] = variant.Family.ToString(),
                        ["parentTier"] = variant.ParentTier,
                        ["quantity"] = variant.Multiplicity.ToString(),
                        ["symbol"] = symbol,
                        ["resolved"] = ability != null,
                        ["guid"] = ability == null ? null : ability.AssetGuid,
                        ["withheld"] = hidden });
                }
                // Two consistent states and no third. Before publication
                // every root exists and every one is withheld; after it every
                // root exists and every one is published. A run where some
                // are and some are not is a half-published creature, which is
                // the thing this cannot be allowed to pass.
                bool allWithheld = withheld.Count == 12 && published.Count == 0;
                bool allPublished = published.Count == 12 && withheld.Count == 0;
                Sprint20Check("twelve-roots-live",
                    mine.Length == 12 && missing.Count == 0 &&
                    (allWithheld || allPublished),
                    new JObject { ["roots"] = mine.Length,
                        ["missing"] = new JArray(missing),
                        ["withheld"] = withheld.Count,
                        ["published"] = published.Count,
                        ["state"] = allPublished ? "published"
                            : allWithheld ? "withheld" : "split",
                        ["detail"] = rows },
                    "all twelve new roots exist in the live library and are "
                    + "either all withheld or all published, never split");

                int withheldElsewhere = all.Count(value =>
                    !Sprint20ReviewPolicy.IsSprint20Creature(
                        value.Creature.Key) &&
                    !SummonVisibilityCatalog.IsPublished(value));
                // The published count follows from the state above and is
                // arithmetic rather than a second opinion: withheld means the
                // 1044 v0.0.148 published, published means all 1056. Either
                // way nothing outside this creature may be withheld, which is
                // what says no earlier release moved to make room.
                int expectedPublished = allPublished ? 1056 : 1044;
                Sprint20Check("no-other-root-withheld",
                    withheldElsewhere == 0 &&
                    SummonVisibilityCatalog.PublishedLogicalPlacementCount ==
                        expectedPublished &&
                    SummonVisibilityCatalog.RegisteredLogicalPlacementCount ==
                        1056 &&
                    SummonVisibilityCatalog.SuppressedLogicalPlacementCount ==
                        (allPublished ? 0 : 12),
                    new JObject {
                        ["registered"] = SummonVisibilityCatalog
                            .RegisteredLogicalPlacementCount,
                        ["suppressed"] = SummonVisibilityCatalog
                            .SuppressedLogicalPlacementCount,
                        ["published"] = SummonVisibilityCatalog
                            .PublishedLogicalPlacementCount,
                        ["expectedPublished"] = expectedPublished,
                        ["otherWithheld"] = withheldElsewhere },
                    "the live surface matches the state the roots are in, and "
                    + "no root outside this creature is withheld");

                // The module switch: the view patch knows this creature's
                // blueprint, and the module that owns it is the one running.
                bool handled = ExpandedSummoningPteranodonViewPatch
                    .HandlesBlueprintName(ExpandedSummoningPteranodonViewPatch
                        .GiantScorpionBlueprintName);
                Sprint20Check("module-switch",
                    handled && _context.FeatureModules.Active.ExpandedSummoning,
                    new JObject {
                        ["moduleActive"] = _context.FeatureModules.Active
                            .ExpandedSummoning,
                        ["handledByViewPatch"] = handled,
                        ["blueprintName"] =
                            ExpandedSummoningPteranodonViewPatch
                                .GiantScorpionBlueprintName },
                    "the creature's body is reachable through its own module "
                    + "and that module is the one running");
            }
            catch (Exception error)
            {
                Sprint20Check("surface-exception", false, new JObject {
                        ["exception"] =
                            DescribeExpandedSummoningCorrectionException(error) },
                    "the registered surface review completes without an "
                    + "exception");
            }
        }

        /// <summary>
        /// Release any hold this review's own earlier case established, so the
        /// printed manoeuvre figures are read with nothing held.
        ///
        /// <para>A creature holding its target carries the maintain bonus as
        /// well as its grab, and the first guarded run read the printed +12
        /// grapple figure as 17 for exactly that reason. Only this
        /// request-local creature and this disposable hostile are touched, and
        /// only this project's own hold states are removed.</para>
        /// </summary>
        private static string Sprint20ReleaseOwnHold(
            ExpandedSummoningCorrectionFixture fixture, UnitEntityData owner)
        {
            SummonGrabComponent grab = SummonGrabComponent.Find(owner);
            var cleared = new List<string>();
            ResetExpandedSummoningHostile(fixture);
            if (owner.Get<Kingmaker.UnitLogic.Parts.UnitPartGrappleInitiator>()
                    != null)
            {
                owner.Remove<
                    Kingmaker.UnitLogic.Parts.UnitPartGrappleInitiator>();
                cleared.Add("grappleInitiator");
            }
            if (grab != null && grab.HoldBuff != null)
                foreach (Buff buff in owner.Descriptor.Buffs.RawFacts
                    .OfType<Buff>().Where(value => value != null &&
                        ReferenceEquals(value.Blueprint, grab.HoldBuff))
                    .ToArray())
                {
                    cleared.Add(buff.Blueprint.name);
                    buff.Remove();
                }
            bool holding = owner.Get<
                    Kingmaker.UnitLogic.Parts.UnitPartGrappleInitiator>() != null ||
                (grab != null && grab.HoldBuff != null &&
                    owner.Descriptor.Buffs.GetBuff(grab.HoldBuff) != null) ||
                (grab != null && grab.HeldCount(owner) != 0);
            if (holding) return "still-holding";
            return cleared.Count == 0
                ? "nothing-held"
                : "nothing-held;cleared=" + string.Join(",", cleared.ToArray());
        }

        /// <summary>
        /// Whether a limb the fixture did not choose landed anyway.
        ///
        /// <para>The mirror of the chosen limb missing, and the same rule
        /// seen from the other side: a natural twenty always hits, whatever
        /// the modifier, so a limb held at minus a hundred still lands one
        /// roll in twenty. When it does, this trial no longer isolates one
        /// limb - a sting that landed in a claw case poisons, and a claw that
        /// landed in a sting case attempts a grapple - and what it would
        /// measure is the dice rather than the gating. The trial is abandoned
        /// whole and retried, exactly as one whose chosen limb missed is.</para>
        ///
        /// <para>The requirement is untouched: a claw-only sequence must
        /// still force no Fortitude save and a sting-only sequence must still
        /// attempt no grapple. This only refuses to score a sequence that was
        /// never claw-only or sting-only.</para>
        /// </summary>
        private static bool Sprint20UnchosenLimbLanded(
            Sprint16RuleObserver observer, int from, Sprint18Accuracy accuracy)
        {
            return Sprint18AttemptRolls(observer, null, from).Any(roll =>
                roll.IsHit && roll.Weapon != null &&
                !accuracy.Hit.Any(chosen => ReferenceEquals(chosen, roll.Weapon)));
        }

        /// <summary>
        /// Whether the save this trial forced was an automatic success.
        ///
        /// <para>A natural twenty always makes a save, whatever the modifier,
        /// so a trial that wanted a failed save and rolled one has measured
        /// the dice rather than the rule - exactly as a natural one always
        /// misses. The trial is abandoned whole and retried.</para>
        /// </summary>
        private static bool Sprint20SaveRolledTwenty(
            Sprint19SaveObserver saves, int from, bool saveMade)
        {
            if (saveMade) return false;
            RuleSavingThrow[] forced = saves.Saves.Skip(from).Where(save =>
                save.StatType == StatType.SaveFortitude).ToArray();
            return forced.Length > 0 && forced.All(save => save.D20.Value == 20);
        }

        /// <summary>This creature's own venom buff, by its registered name.</summary>
        private static BlueprintBuff Sprint20Venom(
            ExpandedSummoningCorrectionFixture fixture)
        {
            return fixture.Blueprints.OfType<BlueprintBuff>()
                .FirstOrDefault(value => value != null && value.name ==
                    "KMG_Summoning_Natural_GiantScorpion_Venom");
        }

        private static bool Sprint20Poisoned(
            ExpandedSummoningCorrectionFixture fixture, BlueprintBuff venom)
        {
            return venom != null && fixture.Hostile != null &&
                !fixture.Hostile.Destroyed &&
                fixture.Hostile.Descriptor.Buffs.GetBuff(venom) != null;
        }

        /// <summary>
        /// Remove any venom an earlier case applied, so a case measures its
        /// own outcome. Only the disposable hostile is touched, and only this
        /// creature's own venom is removed.
        /// </summary>
        private static void Sprint20ClearVenom(
            ExpandedSummoningCorrectionFixture fixture)
        {
            BlueprintBuff venom = Sprint20Venom(fixture);
            if (venom == null || fixture.Hostile == null ||
                fixture.Hostile.Destroyed) return;
            foreach (Buff buff in fixture.Hostile.Descriptor.Buffs.RawFacts
                .OfType<Buff>().Where(value => value != null &&
                    ReferenceEquals(value.Blueprint, venom)).ToArray())
                buff.Remove();
        }

        private void CleanupSprint20Review()
        {
            IEnumerator<int> steps = _sprint20Steps;
            _sprint20Steps = null;
            ExpandedSummoningCorrectionFixture fixture = _sprint20Fixture;
            _sprint20Fixture = null;
            try { if (steps != null) steps.Dispose(); }
            catch (Exception error)
            {
                _sprint20Assertions.Add(Assertion("sprint20-disposal",
                    "iterator cleanup", error.Message, false, "owned-only"));
            }
            bool cleaned = false;
            try { EndExpandedSummoningCorrectionFixture(fixture, out cleaned); }
            catch (Exception error)
            {
                _sprint20Assertions.Add(Assertion("sprint20-cleanup-error",
                    "native cleanup", error.Message, false, "owned-only"));
            }
            _sprint20Assertions.Add(Assertion("sprint20-fixture-cleanup",
                "exact original unit/party/area references", "cleaned=" + cleaned,
                cleaned, "No save write and no unrelated-unit cleanup."));
        }

        private void FinishSprint20Review()
        {
            ReviewSprint20Surface();
            CleanupSprint20Review();
            _sprint20Assertions.Add(Assertion("loaded-mod-version",
                _request.ExpectedModVersion, _context.ModEntry.Info.Version,
                _request.ExpectedModVersion == _context.ModEntry.Info.Version,
                "Loaded UMM version."));
            File.WriteAllText(Path.Combine(_request.EvidenceDirectory,
                "sprint20-review.json"),
                _sprint20Rows.ToString(Formatting.Indented));
            _sprint20Complete = true;
            Complete(CreateResult(
                _sprint20Assertions.All(value => value.Status == "PASS") ?
                    "PASS" : "FAIL", _sprint20Assertions, null));
        }

        // Called by Complete on an outer timeout or error too, so the
        // iterator's actor, pause and random-state finally blocks run before
        // the working-save sentinels close.
        private void StopSprint20Review(RuntimeTestResult result)
        {
            if (_sprint20Steps == null && _sprint20Fixture == null) return;
            CleanupSprint20Review();
        }
    }
}
