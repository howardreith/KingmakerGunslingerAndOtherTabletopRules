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
using Kingmaker.UnitLogic.FactLogic;
using KingmakerGunslinger.Assets;
using KingmakerGunslinger.Summoning;
using TurnBased.Controllers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.RuntimeTesting
{
    /// <summary>
    /// The Sprint 21 hidden-candidate runtime review: what the Giant Crab
    /// actually is in a running game.
    ///
    /// <para>The fixture, the turn-order driver, the hit chooser and both
    /// bounded re-attempts are Sprints 18 to 20's and are reused rather than
    /// copied. The fixture decides only which of this creature's own limbs may
    /// reach this disposable target; it moves before the roll and goes back
    /// after. Nothing writes a result, replays a rule or manufactures a
    /// save.</para>
    ///
    /// <para>This is the shortest review in the series, because the creature
    /// is the simplest thing in it: two claws, one rider, no second limb type.
    /// Sprint 20 needed three rider cases to keep a poison off a claw and a
    /// grab off a sting; a crab has nothing for a rider to leak onto. What the
    /// review spends its length on instead is the thing that actually went
    /// wrong twice in Sprint 20 - printed lines with a derivation and no
    /// carrier. Both of this creature's own carriers are of that kind, so the
    /// anti-trip defence is measured against the engine's own trip manoeuvre
    /// and the racial bonus is read out of the live skill's modifier list,
    /// named for the feature that supplied it.</para>
    /// </summary>
    internal sealed partial class RuntimeTestRunner
    {
        private const int Sprint21SettleFrames = 240;

        private ExpandedSummoningCorrectionFixture _sprint21Fixture;
        private IEnumerator<int> _sprint21Steps;
        private readonly List<RuntimeTestAssertion> _sprint21Assertions =
            new List<RuntimeTestAssertion>();
        private readonly JArray _sprint21Rows = new JArray();
        private int _sprint21LoadingFrames;
        private bool _sprint21Complete;

        private void PollSprint21Review()
        {
            if (_sprint21Complete) return;
            try
            {
                if (_sprint21Steps == null)
                {
                    string loading;
                    if (ExpandedSummoningLoadingActive(out loading))
                    {
                        if (++_sprint21LoadingFrames <
                            ExpandedSummoningLoadingGateFrames) return;
                        throw new InvalidOperationException(
                            "Native loading did not settle: " + loading);
                    }
                    _sprint21Fixture = BeginExpandedSummoningCorrectionFixture(
                        "KMG_Runtime_Sprint21_Caster");
                    _sprint21Steps =
                        ReviewSprint21(_sprint21Fixture).GetEnumerator();
                }
                if (_sprint21Steps.MoveNext()) return;
            }
            catch (Exception error)
            {
                Sprint21Check("review-exception", false, new JObject {
                        ["exception"] =
                            DescribeExpandedSummoningCorrectionException(error) },
                    "the whole Sprint 21 review completes without an exception");
            }
            FinishSprint21Review();
        }

        private void Sprint21Check(string name, bool passed, JObject row,
            string expected)
        {
            row["case"] = name;
            row["passed"] = passed;
            _sprint21Rows.Add(row);
            _sprint21Assertions.Add(Assertion("sprint21-" + name, expected,
                row.ToString(Formatting.None), passed,
                "request-local live Giant Crab; native rules and the "
                + "registered execution"));
        }

        private IEnumerable<int> ReviewSprint21(
            ExpandedSummoningCorrectionFixture fixture)
        {
            bool pause = Game.Instance.IsPaused;
            bool turnBasedSetting = Kingmaker.UI.SettingsUI.SettingsRoot
                .Instance.EnableTurnBasedMode.CurrentValue;
            UnityEngine.Random.State random = UnityEngine.Random.state;
            try
            {
                Sprint21ReviewPolicy.Validate();
                Game.Instance.IsPaused = false;
                CreateExpandedSummoningCorrectionHostile(fixture);
                bool bodyRead = false;
                foreach (bool turnBased in Sprint21ReviewPolicy.CombatModes)
                {
                    Kingmaker.UI.SettingsUI.SettingsRoot.Instance
                        .EnableTurnBasedMode.CurrentValue = turnBased;
                    Game.Instance.TurnBasedCombatController.Activate();
                    UnitEntityData owner = CastExpandedSummoningOwnTier(
                        fixture, Sprint21ReviewPolicy.CrabKey);
                    SetExpandedSummoningBrainActive(owner, false);
                    int settle = 0;
                    while (++settle <= Sprint21SettleFrames)
                    {
                        if (Game.Instance.IsPaused) Game.Instance.IsPaused = false;
                        yield return 0;
                        if (owner.View != null &&
                            owner.Descriptor.State.CanAct) break;
                    }
                    try
                    {
                        if (!bodyRead) ReviewSprint21Body(owner);
                        // Measured with nothing held, which is the lesson
                        // Sprint 20 paid for twice: a creature already holding
                        // its target carries the maintain bonus as well as its
                        // grab, and the sum of two right numbers is a wrong
                        // answer.
                        ReviewSprint21GrabCarrier(fixture, owner, turnBased);
                        ReviewSprint21OwnCarriers(owner, turnBased);
                        foreach (int step in ReviewSprint21Riders(fixture,
                            owner, turnBased)) yield return step;
                        // Now the creature has acted and is no longer
                        // flat-footed, which is the state a printed stat block
                        // describes.
                        ReviewSprint21TripDefence(fixture, owner, turnBased);
                        ReviewSprint21Profile(fixture, owner, turnBased);
                        // Last, because it takes the grab off this creature to
                        // measure the routine: a creature that has just seized
                        // its target stops swinging, and with two claws and a
                        // grab on both there is no limb left to prove the
                        // routine with.
                        foreach (int step in ReviewSprint21Routine(fixture,
                            owner, turnBased)) yield return step;
                    }
                    finally
                    {
                        ResetExpandedSummoningHostile(fixture);
                        DisposeExpandedSummoningUnits(fixture.Created,
                            new[] { owner });
                    }
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
        /// Every limb this creature has. A crab has two claws and nothing
        /// else, so there is no limb kind to distinguish - which the review
        /// asserts rather than assumes, because a third limb appearing here
        /// would mean the profile had grown one.
        /// </summary>
        private static bool Sprint21LimbsAreTwoIdenticalClaws(
            ItemEntityWeapon[] limbs)
        {
            return limbs.Length == GiantCrabRulesPolicy.ClawCount &&
                limbs.All(limb => limb != null && limb.Blueprint != null) &&
                limbs.Select(limb => limb.Blueprint).Distinct().Count() == 1;
        }

        /// <summary>
        /// The printed stat block, read off the live creature. With no
        /// Intelligence score there are no ranks and no feats, so every number
        /// falls out of hit dice, ability scores, size and racial bonuses, and
        /// the whole block can be checked rather than a sample of it.
        /// </summary>
        private void ReviewSprint21Profile(
            ExpandedSummoningCorrectionFixture fixture, UnitEntityData owner,
            bool turnBased)
        {
            Sprint21ReviewPolicy.LiveProfile expected =
                Sprint21ReviewPolicy.Crab;
            var stats = owner.Descriptor.Stats;
            int[] scores = new[] { StatType.Strength, StatType.Dexterity,
                StatType.Constitution, StatType.Intelligence, StatType.Wisdom,
                StatType.Charisma }.Select(stat =>
                    stats.GetStat(stat).ModifiedValue).ToArray();

            ItemEntityWeapon[] limbs = Sprint19Limbs(owner);
            var limbRows = new JArray();
            bool limbsExact = Sprint21LimbsAreTwoIdenticalClaws(limbs);
            for (int index = 0; index < limbs.Length; index++)
            {
                RuleCalculateWeaponStats calculated = Rulebook.Trigger(
                    new RuleCalculateWeaponStats(owner, limbs[index], null));
                BaseDamage damage = calculated.DamageDescription[0].CreateDamage();
                int attack = ProbeEntityAttackBonus(owner, fixture.Hostile,
                    limbs[index]);
                int[] want = index < expected.Limbs.Length ?
                    expected.Limbs[index] : new[] { 0, 0, 0, 0 };
                // Both claws primary, at one bonus, adding the whole Strength
                // modifier. A claw reading 1d4+3 against a printed 1d4+2 is
                // the engine's primary-hand rule reaching a creature the
                // released full-Strength carrier is supposed to hold.
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
            // numbers exactly once. No creature escapes it and no stat block
            // prints it, so it is subtracted and named. Sprint 18 identified
            // this term; it is not a Sprint 21 finding.
            int difficulty = stats.AC.Modifiers.Where(modifier =>
                modifier.ModDescriptor == ModifierDescriptor.Difficulty)
                .Sum(modifier => modifier.ModValue);
            int deniedDex = cmd.DexterityDenied || cmd.FlatFooted ?
                stats.Dexterity.Bonus : 0;

            var skillRows = new JObject();
            bool skillsExact = true;
            foreach (KeyValuePair<string, int> want in expected.Skills)
            {
                StatType stat = Sprint21Skill(want.Key);
                int live = stats.GetStat(stat).ModifiedValue;
                skillsExact &= live == want.Value;
                skillRows[want.Key] = DescribeSprint16Skill(stats.GetStat(stat));
            }

            bool noRanks = true;
            var rankRows = new JObject();
            foreach (StatType stat in new[] { StatType.SkillPerception,
                StatType.SkillStealth, StatType.SkillMobility,
                StatType.SkillAthletics })
            {
                int baseValue = stats.GetStat(stat).BaseValue;
                rankRows[stat.ToString()] = baseValue;
                noRanks &= baseValue == GiantCrabRulesPolicy.SkillRanks;
            }

            string released = Sprint21ReleaseOwnHold(fixture, owner);
            int grapple = Rulebook.Trigger(new RuleCalculateCMB(owner,
                fixture.Hostile, CombatManeuver.Grapple)).Result;
            int ordinaryCmb = Rulebook.Trigger(new RuleCalculateCMB(owner,
                fixture.Hostile, CombatManeuver.Trip)).Result;

            bool exactProfile = scores.SequenceEqual(new[] {
                    expected.Strength, expected.Dexterity,
                    expected.Constitution, expected.Intelligence,
                    expected.Wisdom, expected.Charisma }) &&
                owner.Descriptor.State.Size == Size.Medium &&
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
                ordinaryCmb == GiantCrabRulesPolicy
                    .PrintedCombatManeuverBonus &&
                grapple == GiantCrabRulesPolicy.PrintedGrappleBonus &&
                grapple - ordinaryCmb ==
                    GiantCrabRulesPolicy.GrabGrappleBonus &&
                released.StartsWith("nothing-held", StringComparison.Ordinal) &&
                skillsExact && limbsExact && noRanks;
            Sprint21Check("live-profile-" +
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
                    ["expectedManeuverBonus"] = GiantCrabRulesPolicy
                        .PrintedCombatManeuverBonus,
                    ["expectedGrappleBonus"] = GiantCrabRulesPolicy
                        .PrintedGrappleBonus,
                    ["skills"] = skillRows,
                    ["skillRanks"] = rankRows,
                    ["limbs"] = limbRows,
                    ["expectedSpeed"] = expected.SpeedFeet,
                    // What this creature does not represent, as numbers rather
                    // than silences. Nothing stands in for any of them.
                    ["printedSwimSpeedOmitted"] =
                        GiantCrabRulesPolicy.PrintedSwimSpeedFeet,
                    ["waterDependencyOmitted"] = true,
                    ["athleticsUnraised"] =
                        stats.GetStat(StatType.SkillAthletics).BaseValue,
                    ["mobilityUnraised"] =
                        stats.GetStat(StatType.SkillMobility).BaseValue,
                    ["unitType"] = owner.Blueprint.Type == null ? null :
                        owner.Blueprint.Type.name },
                "every printed Giant Crab number reads back from the live "
                + "creature, with no rank and no substituted swim");
        }

        /// <summary>
        /// The one Kingmaker skill a printed Giant Crab skill is represented
        /// by. A name outside it is a review bug, not a creature.
        /// </summary>
        private static StatType Sprint21Skill(string name)
        {
            if (name == "Perception") return StatType.SkillPerception;
            throw new ArgumentOutOfRangeException("name", name,
                "Sprint 21 represents one Kingmaker skill.");
        }

        /// <summary>
        /// The two carriers this creature owns, read from the engine rather
        /// than from the builder.
        ///
        /// <para>Both are printed lines whose arithmetic closes without them,
        /// which is the shape of defect Sprint 20 shipped twice: the anti-trip
        /// defence and the racial bonus each derived correctly and were each
        /// delivered by nothing. So the racial bonus is read out of the live
        /// skill's own modifier list and must be named for this creature's
        /// feature - a Racial-descriptor +4 from somewhere else would be the
        /// wrong creature's carrier, and a +4 from nowhere would be a
        /// coincidence this check refuses to accept.</para>
        /// </summary>
        private void ReviewSprint21OwnCarriers(UnitEntityData owner,
            bool turnBased)
        {
            BlueprintFeature trip = _sprint21Fixture.Blueprints
                .OfType<BlueprintFeature>().FirstOrDefault(value =>
                    value != null && value.name ==
                    "KMG_Summoning_Natural_GiantCrab_TripDefense");
            BlueprintFeature racial = _sprint21Fixture.Blueprints
                .OfType<BlueprintFeature>().FirstOrDefault(value =>
                    value != null && value.name ==
                    "KMG_Summoning_Natural_GiantCrab_RacialSkills");
            BlueprintFeature immunity = _sprint21Fixture.Blueprints
                .OfType<BlueprintFeature>().FirstOrDefault(value =>
                    value != null && value.name ==
                    "KMG_Summoning_Natural_GiantCrab_MindlessImmunity");
            bool carriesAll = trip != null && racial != null &&
                immunity != null && owner.Descriptor.HasFact(trip) &&
                owner.Descriptor.HasFact(racial) &&
                owner.Descriptor.HasFact(immunity);

            // The racial bonus, in the live skill rather than in the feature.
            ModifiableValue perception =
                owner.Descriptor.Stats.GetStat(StatType.SkillPerception);
            var racialTerms = perception.Modifiers.Where(modifier =>
                modifier.ModDescriptor == ModifierDescriptor.Racial).ToArray();
            bool racialExact = racialTerms.Length == 1 &&
                racialTerms[0].ModValue ==
                    GiantCrabRulesPolicy.RacialPerceptionBonus &&
                racial != null && racialTerms[0].Source != null &&
                racialTerms[0].Source.ToString().IndexOf("GiantCrab",
                    StringComparison.Ordinal) >= 0;

            // The mind-affecting immunity, read from the component the rules
            // layer consults rather than from the fact being present.
            BuffDescriptorImmunity[] parts =
                immunity == null || immunity.ComponentsArray == null
                    ? new BuffDescriptorImmunity[0]
                    : immunity.ComponentsArray
                        .OfType<BuffDescriptorImmunity>().ToArray();
            bool mindAffecting = parts.Length == 1 &&
                parts[0].Descriptor == SpellDescriptor.MindAffecting &&
                !parts[0].CheckFact;

            // Not the scorpion's, and not the shared native one. A crab
            // wearing either would read as the wrong creature on inspection,
            // and the shared one is two points light besides.
            bool notBorrowed = !owner.Descriptor.Progression.Features.Enumerable
                .Any(fact => fact != null && fact.Blueprint != null &&
                    (fact.Blueprint.name.IndexOf("GiantScorpion",
                        StringComparison.Ordinal) >= 0 ||
                     fact.Blueprint.name == "TripDefenseEightLegs"));

            Sprint21Check("own-carriers-" +
                    (turnBased ? "turn-based" : "real-time"),
                carriesAll && racialExact && mindAffecting && notBorrowed,
                new JObject {
                    ["mode"] = turnBased ? "turn-based" : "real-time",
                    ["carriesTripDefense"] = trip != null &&
                        owner.Descriptor.HasFact(trip),
                    ["carriesRacialSkills"] = racial != null &&
                        owner.Descriptor.HasFact(racial),
                    ["carriesMindlessImmunity"] = immunity != null &&
                        owner.Descriptor.HasFact(immunity),
                    ["perception"] = DescribeSprint16Skill(perception),
                    ["racialTerms"] = racialTerms.Length,
                    ["racialValue"] = racialTerms.Length == 1 ?
                        racialTerms[0].ModValue : 0,
                    ["expectedRacialValue"] =
                        GiantCrabRulesPolicy.RacialPerceptionBonus,
                    ["immunityComponents"] = parts.Length,
                    ["descriptor"] = parts.Length == 1 ?
                        parts[0].Descriptor.ToString() : "absent",
                    ["wearsNoBorrowedCarrier"] = notBorrowed,
                    ["sharedNativeTripBonus"] =
                        GiantCrabRulesPolicy.SharedNativeTripBonus,
                    ["printedTripBonus"] =
                        GiantCrabRulesPolicy.EightLegTripBonus },
                "the creature carries its own three facts, the racial bonus is "
                + "in the live skill and named for its own feature, and it "
                + "wears neither the scorpion's carrier nor the shared "
                + "eight-leg fact");
        }

        /// <summary>
        /// The grab carrier, before anything is held: both claws grab, it is
        /// worth the printed grapple difference, and it can hold one foe in
        /// each claw.
        /// </summary>
        private void ReviewSprint21GrabCarrier(
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
            int grabbing = 0;
            for (int index = 0; index < limbs.Length; index++)
            {
                bool grabs = grab != null && grab.IsGrabLimb(owner, limbs[index]);
                if (grabs) grabbing++;
                limbRows.Add(new JObject {
                    ["limb"] = index,
                    ["weaponName"] = limbs[index].Blueprint.name,
                    ["grabs"] = grabs });
            }
            // Every limb, not some of them. A crab has two claws and both
            // grab, so "all of them" and "the printed count" are the same
            // number here - and the review states it as the printed count, so
            // a third limb appearing would fail rather than be absorbed.
            bool shape = grab != null &&
                Sprint21LimbsAreTwoIdenticalClaws(limbs) &&
                grabbing == GiantCrabRulesPolicy.ClawCount &&
                grab.MaxHeldTargets == GiantCrabRulesPolicy.ClawCount &&
                grab.RakeLimbCount == 0 &&
                delta == GiantCrabRulesPolicy.GrabGrappleBonus &&
                delta == GiantCrabRulesPolicy.PrintedGrappleBonus -
                    GiantCrabRulesPolicy.PrintedCombatManeuverBonus;
            Sprint21Check("grab-rides-both-pincers-" +
                    (turnBased ? "turn-based" : "real-time"), shape,
                new JObject {
                    ["mode"] = turnBased ? "turn-based" : "real-time",
                    ["carrierPresent"] = grab != null,
                    ["limbs"] = limbRows,
                    ["limbsThatGrab"] = grabbing,
                    ["printedClawCount"] = GiantCrabRulesPolicy.ClawCount,
                    ["maxHeld"] = grab == null ? 0 : grab.MaxHeldTargets,
                    ["rakeLimbs"] = grab == null ? -1 : grab.RakeLimbCount,
                    ["grappleLessTrip"] = delta,
                    ["expectedDelta"] = GiantCrabRulesPolicy.GrabGrappleBonus },
                "grab is carried by both pincers and is worth the printed "
                + "grapple difference, with nothing held");
        }

        /// <summary>
        /// The rider, live. A claw that lands may seize - and since both limbs
        /// are claws there is no limb that must not, which is the whole
        /// difference from Sprint 20's three cases.
        /// </summary>
        private IEnumerable<int> ReviewSprint21Riders(
            ExpandedSummoningCorrectionFixture fixture, UnitEntityData owner,
            bool turnBased)
        {
            ItemEntityWeapon[] limbs = Sprint19Limbs(owner);
            SummonGrabComponent grab = SummonGrabComponent.Find(owner);
            int restoreAttack =
                owner.Descriptor.Stats.AdditionalAttackBonus.BaseValue;
            foreach (string[] scenario in Sprint21ReviewPolicy.RiderCases)
            {
                string name = scenario[0];
                var observer = new Sprint16RuleObserver {
                    Owner = owner, Target = fixture.Hostile };
                var accuracy = new Sprint18Accuracy {
                    Owner = owner, Hit = new[] { limbs[0] } };
                observer.BeforeAttackRollForFixture = accuracy.Apply;
                EventBus.Subscribe(observer);
                try
                {
                    ResetExpandedSummoningHostile(fixture);
                    bool startedClean = SummonHeldComponent.HolderOf(
                        fixture.Hostile,
                        grab == null ? null : grab.GrappledBuff) == null;
                    accuracy.Rest();
                    if (CombatController.IsInTurnBasedCombat())
                        foreach (int step in Sprint18AdvanceRound(owner))
                            yield return step;
                    UnitAttack attack = null;
                    int before = observer.Attacks.Count;
                    foreach (int step in Sprint18RunFullAttack(fixture, owner,
                        result => attack = result)) yield return step;
                    int attempts = 1;
                    while (attempts < 8 && Sprint18ChosenLimbMissed(
                        observer, null, before, accuracy))
                    {
                        attempts++;
                        before = observer.Attacks.Count;
                        if (CombatController.IsInTurnBasedCombat())
                            foreach (int step in Sprint18AdvanceRound(owner))
                                yield return step;
                        accuracy.Rest();
                        UnitAttack retry = null;
                        foreach (int step in Sprint18RunFullAttack(fixture,
                            owner, result => retry = result)) yield return step;
                        attack = retry;
                    }
                    for (int settle = 0; settle < 8; settle++) yield return 0;

                    UnitEntityData holder = SummonHeldComponent.HolderOf(
                        fixture.Hostile,
                        grab == null ? null : grab.GrappledBuff);
                    bool held = ReferenceEquals(holder, owner);
                    RuleCombatManeuver[] grapples = observer.Checks.Where(
                        check => check != null &&
                        check.Type == CombatManeuver.Grapple &&
                        ReferenceEquals(check.Initiator, owner)).ToArray();
                    // The attempt is required; the hold is reported. Whether a
                    // grapple check beats the target's manoeuvre defence is
                    // the dice, and a review that required it would fail on a
                    // roll rather than on a rule.
                    bool ok = startedClean && grapples.Length > 0;
                    Sprint21Check("rider-" + name +
                            (turnBased ? "-turn-based" : "-real-time"), ok,
                        new JObject {
                            ["mode"] = turnBased ? "turn-based" : "real-time",
                            ["limb"] = "claw",
                            ["disposition"] = scenario[2],
                            ["startedClean"] = startedClean,
                            ["attempts"] = attempts,
                            ["rolls"] = new JArray(
                                Sprint18AttemptRolls(observer, null, 0)
                                    .Select(roll => new JObject {
                                        ["weapon"] = roll.Weapon == null ? null :
                                            roll.Weapon.Blueprint.name,
                                        ["hit"] = roll.IsHit })),
                            ["grappleAttempts"] = grapples.Length,
                            ["heldByThisCrab"] = held,
                            ["holder"] = holder == null ? null :
                                holder.Blueprint == null ? "?" :
                                holder.Blueprint.name,
                            ["commandFinished"] = attack != null &&
                                attack.IsFinished },
                        "a pincer that lands attempts the game's own grapple "
                        + "check, and whether it takes hold is the dice");
                }
                finally
                {
                    EventBus.Unsubscribe(observer);
                    owner.Descriptor.Stats.AdditionalAttackBonus.BaseValue =
                        restoreAttack;
                    ResetExpandedSummoningHostile(fixture);
                }
                yield return 0;
            }
        }

        /// <summary>
        /// The printed routine: two claws, each exactly once, neither
        /// secondary, both at one bonus.
        ///
        /// <para>The grab comes off this creature first, and here that is not
        /// a convenience but the only way the routine can be measured at all:
        /// a creature that has just seized its target stops swinging, and with
        /// a grab on both of its two limbs the first landing claw would end
        /// every sequence. The carrier is removed from this one request-local
        /// creature, which is about to be dismissed, and the removal is
        /// recorded and asserted rather than assumed.</para>
        /// </summary>
        private IEnumerable<int> ReviewSprint21Routine(
            ExpandedSummoningCorrectionFixture fixture, UnitEntityData owner,
            bool turnBased)
        {
            BlueprintBuff traits = fixture.Blueprints.OfType<BlueprintBuff>()
                .FirstOrDefault(value => value != null && value.name ==
                    "KMG_Summoning_Special_GiantCrab_Traits");
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
                bool exact = carried && removed && grabGone &&
                    observer.Attacks.Count == limbs.Length &&
                    Sprint21LimbsAreTwoIdenticalClaws(limbs) &&
                    !counted.ContainsKey(-1) &&
                    Enumerable.Range(0, limbs.Length).All(index =>
                        counted.ContainsKey(index) && counted[index] == 1) &&
                    observer.Attacks.All(roll => roll.Weapon != null &&
                        !roll.Weapon.IsSecondary) &&
                    observer.Attacks.Select(roll => roll.AttackBonus)
                        .Distinct().Count() == 1 &&
                    CombatController.IsInTurnBasedCombat() == turnBased;
                Sprint21Check("full-attack-" +
                        (turnBased ? "turn-based" : "real-time"), exact,
                    new JObject {
                        ["mode"] = turnBased ? "turn-based" : "real-time",
                        ["observedMode"] = CombatController.IsInTurnBasedCombat(),
                        ["attacks"] = observer.Attacks.Count,
                        ["expected"] = limbs.Length,
                        ["perLimb"] = byLimb,
                        ["commandFinished"] = attack != null && attack.IsFinished,
                        ["grabCarried"] = carried,
                        ["grabRemoved"] = removed,
                        ["grabComponentGone"] = grabGone,
                        ["reason"] =
                            "A creature that has just seized its target stops "
                            + "swinging, and this creature's grab is on both "
                            + "of its two limbs, so the printed routine is "
                            + "measured with the carrier off this one "
                            + "request-local creature. The grab itself is "
                            + "proved in the rider case above, with the "
                            + "carrier live." },
                    "one full attack is exactly the printed routine - two "
                    + "pincers - both primary at one bonus");
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
        /// manoeuvre rather than inferred from a feature being present - which
        /// is exactly how Sprint 20 found the shared native carrier delivering
        /// eight.
        /// </summary>
        private void ReviewSprint21TripDefence(
            ExpandedSummoningCorrectionFixture fixture, UnitEntityData owner,
            bool turnBased)
        {
            Sprint21ReviewPolicy.LiveProfile expected =
                Sprint21ReviewPolicy.Crab;
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
                    GiantCrabRulesPolicy.EightLegTripBonus &&
                liveTrip - liveOrdinary !=
                    GiantCrabRulesPolicy.SharedNativeTripBonus;
            Sprint21Check("eight-legged-trip-defence-" +
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
                    ["printedBonus"] = GiantCrabRulesPolicy.EightLegTripBonus,
                    ["sharedNativeBonus"] =
                        GiantCrabRulesPolicy.SharedNativeTripBonus },
                "the printed anti-trip defence is the ordinary one plus twelve, "
                + "applied once, and is not the shared native eight");
        }

        /// <summary>
        /// The original body: the live creature wears its own mesh on the
        /// donor rig and binds all eight legs.
        /// </summary>
        private void ReviewSprint21Body(UnitEntityData owner)
        {
            string outcome = owner.View == null ? "no-view" :
                ExpandedSummoningPteranodonViewPatch.DescribeView(owner.View);
            string status;
            UnityEngine.Mesh mesh;
            string[] bones;
            UnityEngine.Texture2D albedo;
            bool loaded = PteranodonAssetRuntime.TryGetSprint14InsectVisual(
                Sprint21ReviewPolicy.CrabKey, out mesh, out bones,
                out albedo, out status);
            var names = new HashSet<string>(bones ?? new string[0],
                StringComparer.Ordinal);
            int legs = new[] { 0, 1, 2, 3 }.Count(index =>
                names.Contains("L_Foot" + index) &&
                names.Contains("R_Foot" + index) &&
                names.Contains("L_Leg" + index + "_Upper") &&
                names.Contains("R_Leg" + index + "_Upper"));
            bool chelae = names.Contains("pedipalp7_L") &&
                names.Contains("pedipalp7_R");
            // A crab has no tail. The abdomen chain carries the back of the
            // carapace instead, so the chain is used but nothing arches off
            // it - which is the one way this body differs structurally from
            // the Sprint 20 scorpion's on the same rig.
            bool carapaceOnAbdomen = names.Contains("Tail3_M") &&
                names.Contains("UpperTorso");
            string[] reviewedBones = Sprint14BonePolicy.AllowedBones(
                Sprint21ReviewPolicy.CrabKey);
            bool reviewed = bones != null && bones.All(name =>
                reviewedBones.Contains(name, StringComparer.Ordinal));
            bool attached = outcome.StartsWith("visual:attached;",
                StringComparison.Ordinal);
            Sprint21Check("original-body", loaded && attached && legs == 4 &&
                    chelae && carapaceOnAbdomen && reviewed && mesh != null &&
                    mesh.vertexCount > 0,
                new JObject {
                    ["outcome"] = outcome,
                    ["status"] = status,
                    ["renderers"] = owner.View == null ? null :
                        DescribePteranodonRenderers(owner.View),
                    ["vertices"] = mesh == null ? 0 : mesh.vertexCount,
                    ["bones"] = bones == null ? 0 : bones.Length,
                    ["legChainsWeighted"] = legs,
                    ["chelaeOnPedipalps"] = chelae,
                    ["carapaceOnAbdomenChain"] = carapaceOnAbdomen,
                    ["everyBoneReviewed"] = reviewed,
                    ["albedo"] = albedo == null ? null :
                        albedo.width + "x" + albedo.height,
                    ["carriesNoTail"] = true },
                "the live creature wears its own body, binds all eight legs, "
                + "carries its pincers on the pedipalp chains and weights no "
                + "bone outside its reviewed list");
        }

        /// <summary>
        /// The registered surface: seven roots, all in one consistent state,
        /// and nothing v0.0.149 published moved.
        /// </summary>
        private void ReviewSprint21Surface()
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
                    Sprint21ReviewPolicy.IsSprint21Creature(value.Creature.Key))
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
                // Seven, and every one of them on the Nature's Ally side: this
                // creature is not on the Summon Monster table at all, which is
                // why it owns no execution children.
                bool oneFamily = mine.All(value =>
                    value.Family == SummonFamily.NaturesAlly);
                bool allWithheld = withheld.Count == 7 && published.Count == 0;
                bool allPublished = published.Count == 7 && withheld.Count == 0;
                Sprint21Check("seven-roots-live",
                    mine.Length == 7 && missing.Count == 0 && oneFamily &&
                    (allWithheld || allPublished),
                    new JObject { ["roots"] = mine.Length,
                        ["missing"] = new JArray(missing),
                        ["withheld"] = withheld.Count,
                        ["published"] = published.Count,
                        ["natureAllyOnly"] = oneFamily,
                        ["state"] = allPublished ? "published"
                            : allWithheld ? "withheld" : "split",
                        ["detail"] = rows },
                    "all seven new roots exist in the live library, all on the "
                    + "Nature's Ally side, and are either all withheld or all "
                    + "published - never split");

                int withheldElsewhere = all.Count(value =>
                    !Sprint21ReviewPolicy.IsSprint21Creature(
                        value.Creature.Key) &&
                    !SummonVisibilityCatalog.IsPublished(value));
                int expectedPublished = allPublished ? 1063 : 1056;
                Sprint21Check("no-other-root-withheld",
                    withheldElsewhere == 0 &&
                    SummonVisibilityCatalog.PublishedLogicalPlacementCount ==
                        expectedPublished &&
                    SummonVisibilityCatalog.RegisteredLogicalPlacementCount ==
                        1063 &&
                    SummonVisibilityCatalog.SuppressedLogicalPlacementCount ==
                        (allPublished ? 0 : 7),
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

                // The released Bebelith, whose body this sprint also owns. Its
                // three roots were published in an earlier phase and this
                // sprint must not have moved them.
                SummonVariantSpec[] bebelith = all.Where(value =>
                    value.Creature.Key == "bebelith").ToArray();
                bool bebelithIntact = bebelith.Length == 3 &&
                    bebelith.All(value =>
                        value.Family == SummonFamily.Monster &&
                        SummonVisibilityCatalog.IsPublished(value));
                Sprint21Check("released-bebelith-intact", bebelithIntact,
                    new JObject {
                        ["roots"] = bebelith.Length,
                        ["allMonster"] = bebelith.All(value =>
                            value.Family == SummonFamily.Monster),
                        ["allPublished"] = bebelith.All(
                            SummonVisibilityCatalog.IsPublished),
                        ["tiers"] = new JArray(bebelith
                            .Select(value => value.ParentTier)
                            .OrderBy(value => value)) },
                    "the released Bebelith still holds its three published "
                    + "Summon Monster roots: this sprint owns its body and "
                    + "nothing else");
            }
            catch (Exception error)
            {
                Sprint21Check("surface-exception", false, new JObject {
                        ["exception"] =
                            DescribeExpandedSummoningCorrectionException(error) },
                    "the registered surface review completes without an "
                    + "exception");
            }
        }

        /// <summary>
        /// Release any hold this review's own earlier case established, so the
        /// printed manoeuvre figures are read with nothing held. Sprint 20
        /// read its printed grapple figure five points high for want of this.
        /// </summary>
        private static string Sprint21ReleaseOwnHold(
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

        private void CleanupSprint21Review()
        {
            IEnumerator<int> steps = _sprint21Steps;
            _sprint21Steps = null;
            ExpandedSummoningCorrectionFixture fixture = _sprint21Fixture;
            _sprint21Fixture = null;
            try { if (steps != null) steps.Dispose(); }
            catch (Exception error)
            {
                _sprint21Assertions.Add(Assertion("sprint21-disposal",
                    "iterator cleanup", error.Message, false, "owned-only"));
            }
            bool cleaned = false;
            try { EndExpandedSummoningCorrectionFixture(fixture, out cleaned); }
            catch (Exception error)
            {
                _sprint21Assertions.Add(Assertion("sprint21-cleanup-error",
                    "native cleanup", error.Message, false, "owned-only"));
            }
            _sprint21Assertions.Add(Assertion("sprint21-fixture-cleanup",
                "exact original unit/party/area references", "cleaned=" + cleaned,
                cleaned, "No save write and no unrelated-unit cleanup."));
        }

        private void FinishSprint21Review()
        {
            ReviewSprint21Surface();
            CleanupSprint21Review();
            _sprint21Assertions.Add(Assertion("loaded-mod-version",
                _request.ExpectedModVersion, _context.ModEntry.Info.Version,
                _request.ExpectedModVersion == _context.ModEntry.Info.Version,
                "Loaded UMM version."));
            File.WriteAllText(Path.Combine(_request.EvidenceDirectory,
                "sprint21-review.json"),
                _sprint21Rows.ToString(Formatting.Indented));
            _sprint21Complete = true;
            Complete(CreateResult(
                _sprint21Assertions.All(value => value.Status == "PASS") ?
                    "PASS" : "FAIL", _sprint21Assertions, null));
        }

        // Called by Complete on an outer timeout or error too, so the
        // iterator's actor, pause and random-state finally blocks run before
        // the working-save sentinels close.
        private void StopSprint21Review(RuntimeTestResult result)
        {
            if (_sprint21Steps == null && _sprint21Fixture == null) return;
            CleanupSprint21Review();
        }
    }
}
