using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Designers.Mechanics.Buffs;
using Kingmaker.Enums;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Items;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using KingmakerGunslinger.Summoning;

namespace KingmakerGunslinger.RuntimeTesting
{
    /// <summary>
    /// Sprint 15's runtime work: the Giant Ant Drone and the Giant Stag
    /// Beetle, measured on live creatures.
    ///
    /// <para>Both ride seams other sprints already qualified, which is what
    /// makes this pack short. The Drone's grab, sting, poison and multi-legged
    /// trip defence are the Soldier's, and its own contribution is a template:
    /// every ability score four higher, and every number that derives from
    /// them. The Stag Beetle's trample is the carrier the Sprint 11 ungulates
    /// proved, so it is exercised through that machine rather than asserted
    /// here from the policy that computes its numbers.</para>
    ///
    /// <para>What is genuinely new is therefore arithmetic that no offline
    /// test can confirm: whether the live engine, given a profile written from
    /// a template, produces the printed creature. A derived number read off a
    /// blueprint is the builder's intent; read off a spawned unit it is the
    /// creature a player gets.</para>
    /// </summary>
    internal sealed partial class RuntimeTestRunner
    {
        private const string DroneKey = "giant-ant-drone";
        private const string StagBeetleKey = "giant-stag-beetle";
        // Both creatures' lowest parent tier, which is the one a cast uses.
        private const int Sprint15DroneTier = 4;
        private const int Sprint15StagBeetleTier = 4;

        /// <summary>
        /// The Drone's printed poison difficulty class. It is not written into
        /// the creature anywhere: the shared Giant Ant poison computes it from
        /// the caster's own Constitution, so the advanced 21 has to produce 16
        /// unaided, exactly as the Soldier's 17 produces 14.
        /// </summary>
        private const int Sprint15DronePoisonDc = 16;

        /// <summary>
        /// The Drone's Perception. Derived rather than printed: the racial +4
        /// the whole species carries, plus the +3 from the Wisdom of 17 the
        /// template produces, and no ranks and no flat template bonus.
        /// </summary>
        private const int Sprint15DronePerception = 7;

        private static void ExerciseExpandedSummoningSprint15RulesPack(
            BlueprintScriptableObject[] blueprints, UnitEntityData caster,
            UnitEntityData hostile, List<UnitEntityData> created,
            ExpandedSummoningMechanicalEvidence evidence,
            string evidenceDirectory)
        {
            ExerciseSprint15Profiles(blueprints, caster, hostile, created,
                evidence);
            ExerciseSprint15Defences(blueprints, caster, hostile, created,
                evidence);
            ExerciseSprint15DronePoison(blueprints, caster, hostile, created,
                evidence);
        }

        /// <summary>
        /// Both creatures' live bodies, and the Drone's template arithmetic.
        ///
        /// <para>The Drone must be the Soldier in shape - a 1d6 bite and a
        /// distinct 1d4 sting at the same attack bonus, with the grab on the
        /// primary limb alone - and the Soldier plus four in every ability
        /// score but Intelligence. The Stag Beetle must be a single heavy
        /// limb: Large, one 2d8 bite, nothing else on the body, and none of
        /// the Fire Beetle it shares a donor and a directory with.</para>
        /// </summary>
        private static void ExerciseSprint15Profiles(
            BlueprintScriptableObject[] blueprints, UnitEntityData caster,
            UnitEntityData hostile, List<UnitEntityData> created,
            ExpandedSummoningMechanicalEvidence evidence)
        {
            var rows = new List<string>();
            bool valid = true;

            UnitEntityData soldier = CastExpandedSummoningCombatUnit(blueprints,
                caster, SummonFamily.NaturesAlly, "giant-ant-soldier", 3,
                created, evidence);
            RemoveExpandedSummoningAppearanceBuffs(soldier);
            UnitEntityData drone = CastExpandedSummoningCombatUnit(blueprints,
                caster, SummonFamily.NaturesAlly, DroneKey, Sprint15DroneTier,
                created, evidence);
            RemoveExpandedSummoningAppearanceBuffs(drone);

            BlueprintUnit.UnitBody droneBody = drone.Blueprint.Body;
            BlueprintItemWeapon droneBite = droneBody == null ? null :
                droneBody.PrimaryHand as BlueprintItemWeapon;
            BlueprintItemWeapon droneSting =
                droneBody == null || droneBody.AdditionalLimbs == null ||
                droneBody.AdditionalLimbs.Length != 1 ? null :
                droneBody.AdditionalLimbs[0];
            int droneBiteBonus = ProbeWeaponAttackBonus(drone, hostile,
                droneBite);
            int droneStingBonus = droneSting == null ? int.MinValue :
                ProbeWeaponAttackBonus(drone, hostile, droneSting);
            string droneType = drone.Blueprint.Type == null ? "<none>" :
                drone.Blueprint.Type.name;
            string soldierType = soldier.Blueprint.Type == null ? "<none>" :
                soldier.Blueprint.Type.name;
            bool droneShape =
                drone.Descriptor.State.Size == Size.Medium &&
                droneBite != null &&
                DescribeEffectiveWeaponDice(droneBite) == "1d6" &&
                droneSting != null &&
                DescribeEffectiveWeaponDice(droneSting) == "1d4" &&
                !ReferenceEquals(droneSting, droneBite) &&
                droneSting.Type != droneBite.Type &&
                (droneBody.AdditionalSecondaryLimbs == null ||
                    droneBody.AdditionalSecondaryLimbs.Length == 0) &&
                droneBiteBonus != int.MinValue &&
                droneBiteBonus == droneStingBonus &&
                // The caste shares the species' unit type, which is also what
                // keeps it from still being classified as the Giant Spider.
                droneType == soldierType &&
                droneType.IndexOf("Spider",
                    StringComparison.OrdinalIgnoreCase) < 0;
            SummonGrabComponent droneGrab = SummonGrabComponent.Find(drone);
            bool droneGrabOnBiteOnly = droneGrab != null &&
                droneGrab.GrabWithPrimaryHand &&
                droneGrab.GrabAdditionalLimbCount == 0 &&
                droneGrab.RakeLimbCount == 0 &&
                droneGrab.IsGrabLimb(drone, SummonLimbs.PrimaryWeapon(drone));
            rows.Add(DroneKey + "[size=" + drone.Descriptor.State.Size +
                ";bite=" + DescribeEffectiveWeaponDice(droneBite) + "@" +
                droneBiteBonus + ";sting=" + (droneSting == null ? "<none>" :
                    DescribeEffectiveWeaponDice(droneSting)) + "@" +
                droneStingBonus + ";limbs=" +
                string.Join("/", DescribeLimbs(droneBody)) + ";type=" +
                droneType + ";grabPrimaryOnly=" + droneGrabOnBiteOnly + "]" +
                (droneShape && droneGrabOnBiteOnly ? "=ok" : "=wrong"));
            valid = valid && droneShape && droneGrabOnBiteOnly;

            // The template, measured against the creature it is applied to
            // rather than against six numbers written down twice. Every score
            // but Intelligence is exactly four higher, and Intelligence is
            // untouched because the template excludes it.
            var scores = new List<string>();
            bool templateExact = true;
            foreach (var pair in new[] {
                new { Name = "str", Stat = StatType.Strength, Delta = 4 },
                new { Name = "dex", Stat = StatType.Dexterity, Delta = 4 },
                new { Name = "con", Stat = StatType.Constitution, Delta = 4 },
                new { Name = "int", Stat = StatType.Intelligence, Delta = 0 },
                new { Name = "wis", Stat = StatType.Wisdom, Delta = 4 },
                new { Name = "cha", Stat = StatType.Charisma, Delta = 4 } })
            {
                int before = soldier.Descriptor.Stats.GetStat(pair.Stat)
                    .ModifiedValue;
                int after = drone.Descriptor.Stats.GetStat(pair.Stat)
                    .ModifiedValue;
                bool exact = after - before == pair.Delta;
                templateExact = templateExact && exact;
                scores.Add(pair.Name + "=" + before + "->" + after +
                    "(want+" + pair.Delta + ")" + (exact ? "" : "!"));
            }
            rows.Add("droneAdvancedTemplate[" +
                string.Join(";", scores.ToArray()) + "]" +
                (templateExact ? "=ok" : "=wrong"));
            valid = valid && templateExact;

            UnitEntityData beetle = CastExpandedSummoningCombatUnit(blueprints,
                caster, SummonFamily.NaturesAlly, StagBeetleKey,
                Sprint15StagBeetleTier, created, evidence);
            RemoveExpandedSummoningAppearanceBuffs(beetle);
            BlueprintUnit.UnitBody beetleBody = beetle.Blueprint.Body;
            BlueprintItemWeapon beetleBite = beetleBody == null ? null :
                beetleBody.PrimaryHand as BlueprintItemWeapon;
            string beetleType = beetle.Blueprint.Type == null ? "<none>" :
                beetle.Blueprint.Type.name;
            // The Fire Beetle's luminescence must not have followed the donor
            // or the directory across. Its painting already did once.
            BlueprintFeature luminescence = blueprints.OfType<BlueprintFeature>()
                .FirstOrDefault(value => value != null && value.name ==
                    "KMG_Summoning_Natural_FireBeetle_Luminescence");
            bool carriesLuminescence = luminescence != null &&
                beetle.Descriptor.HasFact(luminescence);
            bool beetleShape =
                beetle.Descriptor.State.Size == Size.Large &&
                beetleBite != null &&
                DescribeEffectiveWeaponDice(beetleBite) == "2d8" &&
                (beetleBody.AdditionalLimbs == null ||
                    beetleBody.AdditionalLimbs.Length == 0) &&
                (beetleBody.AdditionalSecondaryLimbs == null ||
                    beetleBody.AdditionalSecondaryLimbs.Length == 0) &&
                !carriesLuminescence &&
                SummonGrabComponent.Find(beetle) == null &&
                beetleType.IndexOf("Spider",
                    StringComparison.OrdinalIgnoreCase) < 0 &&
                beetleType.IndexOf("FireBeetle", StringComparison.Ordinal) < 0 &&
                beetleType.IndexOf("StagBeetle", StringComparison.Ordinal) >= 0;
            rows.Add(StagBeetleKey + "[size=" + beetle.Descriptor.State.Size +
                ";bite=" + DescribeEffectiveWeaponDice(beetleBite) +
                ";limbs=" + string.Join("/", DescribeLimbs(beetleBody)) +
                ";type=" + beetleType + ";luminescence=" +
                carriesLuminescence + ";grab=" +
                (SummonGrabComponent.Find(beetle) != null) + "]" +
                (beetleShape ? "=ok" : "=wrong"));
            valid = valid && beetleShape;

            evidence.Sprint15Profiles = valid;
            evidence.Sprint15ProfilesDetail = string.Join(";", rows.ToArray());
        }

        /// <summary>
        /// Printed defences, skills and vermin immunity on both creatures.
        ///
        /// <para>The Stag Beetle prints CMD 20 and 28 against trip and those
        /// are asserted. The Drone prints neither: the contract requires its
        /// numbers to be derived from the Soldier, so the derivation is stated
        /// here and the engine has to agree with it - CMD 17 from the Soldier's
        /// 13 and the template's two extra points of Strength and Dexterity
        /// modifier, and 25 against trip from the same multi-legged +8 the
        /// other castes carry.</para>
        ///
        /// <para>Skills are measured as totals with their modifier breakdown,
        /// because the question that decided the Drone's contract erratum is
        /// exactly whether an unprinted flat bonus crept in: Perception has to
        /// be 7, and it has to be 7 because of a racial +4 and a Wisdom of 17,
        /// with no ranks and no stray +2 of any descriptor.</para>
        /// </summary>
        private static void ExerciseSprint15Defences(
            BlueprintScriptableObject[] blueprints, UnitEntityData caster,
            UnitEntityData hostile, List<UnitEntityData> created,
            ExpandedSummoningMechanicalEvidence evidence)
        {
            var rows = new List<string>();
            bool valid = true;
            BlueprintBuff mindProbe = FindSprint14MindAffectingBuff(blueprints,
                caster);

            foreach (var creature in new[] {
                // Cmd and Trip are the defences the creature should have when
                // it is not flat-footed. Bab, Str and SizeBonus are the
                // engine components each one must produce, so a total that
                // happens to come out right for the wrong reasons fails.
                new { Key = DroneKey, Tier = Sprint15DroneTier, Cmd = 17,
                    Trip = 25, Perception = Sprint15DronePerception,
                    Printed = false, Bab = 1, Str = 4, SizeBonus = 0,
                    Dex = 2 },
                new { Key = StagBeetleKey, Tier = Sprint15StagBeetleTier,
                    Cmd = 20, Trip = 28, Perception = 0, Printed = true,
                    Bab = 5, Str = 4, SizeBonus = 1, Dex = 0 } })
            {
                UnitEntityData unit = CastExpandedSummoningCombatUnit(
                    blueprints, caster, SummonFamily.NaturesAlly,
                    creature.Key, creature.Tier, created, evidence);
                RemoveExpandedSummoningAppearanceBuffs(unit);
                Sprint14CmdBreakdown bull =
                    ProbeCombatManeuverDefenceParts(hostile, unit,
                        CombatManeuver.BullRush);
                Sprint14CmdBreakdown tripParts =
                    ProbeCombatManeuverDefenceParts(hostile, unit,
                        CombatManeuver.Trip);
                int ordinary = bull.Result;
                int trip = tripParts.Result;
                // A freshly summoned creature has not acted, so the engine
                // treats it as flat-footed and denies it its Dexterity. That
                // is a property of the measurement, not of the creature: the
                // printed defence is what it has once it has acted, so the
                // denied modifier is added back rather than demanded of a
                // creature that cannot show it here.
                // The engine has to say so itself. Inferring the denial
                // from a Dexterity component of zero would make the check
                // self-fulfilling and would hide the real defect it exists to
                // catch: a creature whose Dexterity never reached its stats at
                // all would look identical. The live score is recorded beside
                // the component so the two can be compared.
                bool denied = bull.DexterityDenied || bull.FlatFooted;
                int liveDexterity = unit.Descriptor.Stats.Dexterity
                    .ModifiedValue;
                int recovered = ordinary + (denied ? creature.Dex : 0);
                int recoveredTrip = trip + (denied ? creature.Dex : 0);
                bool componentsExact =
                    bull.Bab == creature.Bab &&
                    bull.Strength == creature.Str &&
                    bull.Size == creature.SizeBonus &&
                    bull.Misc == 0 &&
                    (denied ? bull.Dexterity == 0 :
                        bull.Dexterity == creature.Dex);
                bool defencesExact = componentsExact &&
                    recovered == creature.Cmd &&
                    recoveredTrip == creature.Trip &&
                    // The multi-legged defence is the difference between the
                    // two, and it is never affected by the Dexterity denial.
                    trip - ordinary == 8;

                // A total with its breakdown, so a size or racial modifier
                // cannot pass for a rank and a flat template bonus cannot
                // hide inside a correct-looking total.
                ModifiableValue perception = unit.Descriptor.Stats
                    .GetStat(StatType.SkillPerception);
                bool perceptionExact =
                    perception.ModifiedValue == creature.Perception &&
                    perception.BaseValue == 0;
                string[] skills = new[] {
                    DescribeSprint14Skill("perception", perception),
                    DescribeSprint14Skill("mobility",
                        unit.Descriptor.Stats.GetStat(StatType.SkillMobility)),
                    DescribeSprint14Skill("stealth",
                        unit.Descriptor.Stats.GetStat(StatType.SkillStealth)) };
                bool noRanks = new[] { StatType.SkillPerception,
                        StatType.SkillMobility, StatType.SkillStealth }
                    .All(stat => unit.Descriptor.Stats.GetStat(stat)
                        .BaseValue == 0);

                // One real attack, then the same measurement again.
                ItemEntityWeapon limb = LiveLimbWeapons(unit)
                    .FirstOrDefault(value => value != null);
                string acted = "no-limb";
                if (limb != null)
                {
                    UnityEngine.Random.InitState(FindNativeD20Seed(20));
                    Rulebook.Trigger(new RuleAttackWithWeapon(unit, hostile,
                        limb, 0));
                    acted = ProbeCombatManeuverDefenceParts(hostile, unit,
                        CombatManeuver.BullRush).Describe("afterActing");
                }

                string immunity = DescribeSprint15MindImmunity(mindProbe, unit,
                    caster);
                bool immune = immunity == "refused-on-vermin;accepted-on-caster";

                rows.Add(creature.Key + "[" + bull.Describe("cmd") + "/" +
                    creature.Cmd + ";" + tripParts.Describe("trip") + "/" +
                    creature.Trip + ";recovered=" + recovered + "/" +
                    recoveredTrip + ";liveDex=" + liveDexterity +
                    ";dexAddedBack=" + (denied ? creature.Dex : 0) + ";" +
                    acted + ";" +
                    (creature.Printed ? "printed" : "derived") + ";" +
                    string.Join(";", skills) + ";noRanks=" + noRanks +
                    ";mindAffecting=" + immunity + "]" +
                    (defencesExact && perceptionExact && noRanks && immune ?
                        "=ok" : "=wrong"));
                valid = valid && defencesExact && perceptionExact &&
                    noRanks && immune;
            }

            rows.Add("mindProbe=" + (mindProbe == null ? "<none>" :
                mindProbe.name));
            valid = valid && mindProbe != null;
            evidence.Sprint15Defences = valid;
            evidence.Sprint15DefencesDetail = string.Join(";", rows.ToArray());
        }

        /// <summary>
        /// The Drone's venom: the Soldier's graph on a stronger body.
        ///
        /// <para>Nothing about the poison is specific to this caste, which is
        /// the point worth proving. The difficulty class is computed from the
        /// caster's live Constitution, so a creature whose Constitution the
        /// template raised by four must produce a difficulty class two higher
        /// without a second graph, a hard-coded number or an override - and
        /// the stat, dice, exposures and cure must not move at all.</para>
        ///
        /// <para>The wound gate is then re-run on this creature rather than
        /// assumed from the Soldier's pass, because the gate sits on the
        /// sting's weapon type and a caste that acquired its sting through a
        /// template is exactly where that could go wrong.</para>
        /// </summary>
        private static void ExerciseSprint15DronePoison(
            BlueprintScriptableObject[] blueprints, UnitEntityData caster,
            UnitEntityData hostile, List<UnitEntityData> created,
            ExpandedSummoningMechanicalEvidence evidence)
        {
            BlueprintBuff venom = blueprints.OfType<BlueprintBuff>()
                .FirstOrDefault(value => value != null && value.name ==
                    "KMG_Summoning_Natural_GiantAnt_Venom");
            if (venom == null)
                throw new InvalidOperationException(
                    "The Giant Ant venom buff was not loaded.");
            UnitEntityData drone = CastExpandedSummoningCombatUnit(blueprints,
                caster, SummonFamily.NaturesAlly, DroneKey, Sprint15DroneTier,
                created, evidence);
            RemoveExpandedSummoningAppearanceBuffs(drone);
            BlueprintUnit.UnitBody body = drone.Blueprint.Body;
            BlueprintItemWeapon bite = body == null ? null :
                body.PrimaryHand as BlueprintItemWeapon;
            BlueprintItemWeapon sting =
                body == null || body.AdditionalLimbs == null ||
                body.AdditionalLimbs.Length != 1 ? null :
                body.AdditionalLimbs[0];
            if (sting == null || bite == null)
            {
                evidence.Sprint15DronePoison = false;
                evidence.Sprint15DronePoisonDetail =
                    "the drone has no sting and bite to tell apart";
                return;
            }

            var rows = new List<string>();

            // The printed numbers, read off the shared buff and the live body.
            // Strength, not Dexterity: the Giant Wasp's venom is the Dexterity
            // one, and these creatures share a cloned graph, so the pairing is
            // worth re-reading on every caste that joins the species.
            BuffPoisonStatDamage poison = (venom.ComponentsArray ??
                Array.Empty<BlueprintComponent>())
                .OfType<BuffPoisonStatDamage>().FirstOrDefault();
            int constitution = drone.Descriptor.Stats.Constitution
                .ModifiedValue;
            int derivedDc = GiantAntPoisonPolicy.DifficultyClass(
                constitution / 2 - 5);
            bool numbersExact = poison != null &&
                poison.Stat == StatType.Strength &&
                poison.Value.Dice == DiceType.D2 &&
                poison.Value.Rolls == 1 &&
                poison.Ticks == GiantAntPoisonPolicy.Exposures &&
                poison.SuccesfullSaves == GiantAntPoisonPolicy.SavesToCure &&
                poison.SaveType == SavingThrowType.Fortitude &&
                constitution == 21 &&
                derivedDc == Sprint15DronePoisonDc;
            rows.Add("printed[stat=" + (poison == null ? "<none>" :
                    poison.Stat.ToString()) + ";dice=" + (poison == null ?
                    "<none>" : poison.Value.Rolls + "d" + poison.Value.Dice) +
                ";exposures=" + (poison == null ? -1 : poison.Ticks) +
                ";savesToCure=" + (poison == null ? -1 :
                    poison.SuccesfullSaves) + ";liveCon=" + constitution +
                ";derivedDc=" + derivedDc + ";printedDc=" +
                Sprint15DronePoisonDc + "]" +
                (numbersExact ? "=ok" : "=wrong"));

            // The same three situations the Soldier's sting is held to, on
            // this creature's own sting and bite.
            rows.Add(DescribeSprint14PoisonCase(blueprints, drone, hostile,
                sting, venom, true, true, "drone-sting-wounds", true));
            rows.Add(DescribeSprint14PoisonCase(blueprints, drone, hostile,
                sting, venom, false, true, "drone-sting-misses", false));
            rows.Add(DescribeSprint14PoisonCase(blueprints, drone, hostile,
                sting, venom, true, false, "drone-sting-hits-no-damage",
                false));
            rows.Add(DescribeSprint14PoisonCase(blueprints, drone, hostile,
                bite, venom, true, true, "drone-bite-wounds", false));

            // One attack, one application, carrying this creature's own
            // difficulty class rather than the Soldier's.
            ClearSprint14Venom(hostile, venom);
            ItemEntityWeapon liveSting = LiveLimbWeapons(drone).FirstOrDefault(
                value => ReferenceEquals(value.Blueprint, sting));
            int applications = -1;
            int dcOnApplied = -1;
            if (liveSting != null)
            {
                UnityEngine.Random.InitState(FindNativeD20Seed(20));
                Rulebook.Trigger(new RuleAttackWithWeapon(drone, hostile,
                    liveSting, 0));
                applications = hostile.Descriptor.Buffs.Enumerable
                    .Count(value => value != null &&
                        ReferenceEquals(value.Blueprint, venom));
                Kingmaker.UnitLogic.Buffs.Buff applied =
                    hostile.Descriptor.Buffs.GetBuff(venom);
                dcOnApplied = applied == null || applied.Context == null ?
                    -1 : applied.Context.Params.DC;
            }
            rows.Add("oneAttackOneApplication[applications=" + applications +
                ";dcOnAppliedBuff=" + dcOnApplied + "]" +
                (applications == 1 && dcOnApplied == Sprint15DronePoisonDc ?
                    "=ok" : "=wrong"));
            ClearSprint14Venom(hostile, venom);

            evidence.Sprint15DronePoison = rows.All(value =>
                value.EndsWith("=ok", StringComparison.Ordinal));
            evidence.Sprint15DronePoisonDetail =
                string.Join(";", rows.ToArray());
        }

        /// <summary>
        /// Apply the mind-affecting probe to a vermin and to the caster.
        /// Only a refusal on the one and an acceptance on the other is
        /// immunity; a buff that applies to nothing would otherwise read as
        /// immunity on every creature in the game.
        /// </summary>
        private static string DescribeSprint15MindImmunity(BlueprintBuff probe,
            UnitEntityData vermin, UnitEntityData caster)
        {
            return DescribeSprint14MindImmunity(probe, vermin, caster);
        }
    }
}
