using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.Enums;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Items;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics.Components;
using KingmakerGunslinger.Summoning;

namespace KingmakerGunslinger.RuntimeTesting
{
    /// <summary>
    /// Sprint 14's runtime work, as one batched family rather than a near-copy
    /// of the runner per creature.
    ///
    /// <para>The tranche donor census established that Kingmaker has no beetle
    /// and no ant, and that the Giant Spider is the only compact many-legged
    /// arthropod in the game. That makes it the best available donor, which is
    /// not the same as proving one rig can carry both a six-legged ant walking
    /// and a beetle flying. The owner's order is explicit that the hypothesis
    /// has to survive two minimal vertical slices before five models are
    /// authored against it, so this pack's first job is to bring back the one
    /// thing that cannot be obtained offline: the donor's measured bind
    /// frame.</para>
    ///
    /// <para>Only bone names and a measured bind frame leave the game. No donor
    /// vertices, triangles, materials, textures or animation data enter the
    /// evidence or the repository.</para>
    /// </summary>
    internal sealed partial class RuntimeTestRunner
    {
        /// <summary>
        /// The donors Sprint 14 and 15 need, named by the project creature that
        /// already rides each one. Keeping this a short table rather than a
        /// method per creature is what lets Sprint 15 add the stag beetle
        /// without another exercise.
        /// </summary>
        private static readonly string[][] Sprint14DonorRigs =
        {
            // The Giant Spider carries the whole insect family hypothesis, so
            // its frame is the one the ground and flying slices are authored
            // against.
            new[] { "giant-spider", "NaturesAlly", "2", "giant-spider" }
        };

        private static void ExerciseExpandedSummoningSprint14RulesPack(
            BlueprintScriptableObject[] blueprints, UnitEntityData caster,
            UnitEntityData hostile, List<UnitEntityData> created,
            ExpandedSummoningMechanicalEvidence evidence,
            string evidenceDirectory)
        {
            ExerciseSprint14DonorRigs(blueprints, caster, created, evidence,
                evidenceDirectory);
            ExerciseSprint14Profiles(blueprints, caster, hostile, created,
                evidence);
            ExerciseSprint14TripDefence(blueprints, caster, hostile, created,
                evidence);
            ExerciseSprint14PoisonDiscrimination(blueprints, caster, hostile,
                created, evidence);
        }

        /// <summary>
        /// The live profile of all three creatures: what the player actually
        /// gets, read off the spawned unit rather than off the blueprint we
        /// hoped we built.
        ///
        /// <para>Unit type is the one worth naming. The builder reconstructs
        /// class levels, facts, body, stats and brain, but it leaves
        /// BlueprintUnitType alone unless a creature asks for its own, and all
        /// three of these clone the Giant Spider. Reading it live is the only
        /// way to know a beetle is not a spider.</para>
        /// </summary>
        private static void ExerciseSprint14Profiles(
            BlueprintScriptableObject[] blueprints, UnitEntityData caster,
            UnitEntityData hostile, List<UnitEntityData> created,
            ExpandedSummoningMechanicalEvidence evidence)
        {
            var rows = new List<string>();
            bool valid = true;

            UnitEntityData beetle = CastExpandedSummoningCombatUnit(blueprints,
                caster, SummonFamily.NaturesAlly, "fire-beetle", 1, created,
                evidence);
            RemoveExpandedSummoningAppearanceBuffs(beetle);
            BlueprintUnit.UnitBody beetleBody = beetle.Blueprint.Body;
            BlueprintItemWeapon beetleBite = beetleBody == null ? null :
                beetleBody.PrimaryHand as BlueprintItemWeapon;
            string beetleType = beetle.Blueprint.Type == null ? "<none>" :
                beetle.Blueprint.Type.name;
            bool beetleShape =
                beetle.Descriptor.State.Size == Size.Small &&
                beetleBite != null && DescribeWeaponDice(beetleBite) == "1d4" &&
                (beetleBody.AdditionalLimbs == null ||
                    beetleBody.AdditionalLimbs.Length == 0) &&
                (beetleBody.AdditionalSecondaryLimbs == null ||
                    beetleBody.AdditionalSecondaryLimbs.Length == 0) &&
                beetleType.IndexOf("Spider", StringComparison.OrdinalIgnoreCase) < 0 &&
                beetleType.IndexOf("FireBeetle", StringComparison.Ordinal) >= 0;
            // Luminescence is present and carries nothing: the whole point is
            // that it grants and denies nothing, because the rules layer has
            // nothing for it to grant or deny against.
            BlueprintFeature luminescence = blueprints.OfType<BlueprintFeature>()
                .FirstOrDefault(value => value != null && value.name ==
                    "KMG_Summoning_Natural_FireBeetle_Luminescence");
            bool luminescenceCarried = luminescence != null &&
                beetle.Descriptor.HasFact(luminescence);
            bool luminescenceInert = luminescenceCarried &&
                (luminescence.ComponentsArray == null ||
                    luminescence.ComponentsArray.Length == 0);
            rows.Add("fire-beetle[size=" + beetle.Descriptor.State.Size +
                ";bite=" + DescribeWeaponDice(beetleBite) + ";limbs=" +
                string.Join("/", DescribeLimbs(beetleBody)) + ";type=" +
                beetleType + ";luminescence=" + luminescenceCarried +
                ";luminescenceComponents=" + (luminescence == null ? -1 :
                    luminescence.ComponentsArray == null ? 0 :
                    luminescence.ComponentsArray.Length) + "]");
            valid = valid && beetleShape && luminescenceInert;

            UnitEntityData worker = CastExpandedSummoningCombatUnit(blueprints,
                caster, SummonFamily.NaturesAlly, "giant-ant-worker", 2,
                created, evidence);
            RemoveExpandedSummoningAppearanceBuffs(worker);
            BlueprintUnit.UnitBody workerBody = worker.Blueprint.Body;
            BlueprintItemWeapon workerBite = workerBody == null ? null :
                workerBody.PrimaryHand as BlueprintItemWeapon;
            string workerType = worker.Blueprint.Type == null ? "<none>" :
                worker.Blueprint.Type.name;
            // The Worker template removes the sting and the grab, which leaves
            // a bite alone: one limb, and nothing else anywhere on the body.
            bool workerShape =
                worker.Descriptor.State.Size == Size.Medium &&
                workerBite != null && DescribeWeaponDice(workerBite) == "1d6" &&
                (workerBody.AdditionalLimbs == null ||
                    workerBody.AdditionalLimbs.Length == 0) &&
                (workerBody.AdditionalSecondaryLimbs == null ||
                    workerBody.AdditionalSecondaryLimbs.Length == 0) &&
                workerType.IndexOf("Spider", StringComparison.OrdinalIgnoreCase) < 0 &&
                workerType.IndexOf("GiantAnt", StringComparison.Ordinal) >= 0;
            bool workerCarriesNoPoison = SummonGrabComponent.Find(worker) == null;
            rows.Add("giant-ant-worker[size=" + worker.Descriptor.State.Size +
                ";bite=" + DescribeWeaponDice(workerBite) + ";limbs=" +
                string.Join("/", DescribeLimbs(workerBody)) + ";type=" +
                workerType + ";grab=" + !workerCarriesNoPoison + "]");
            valid = valid && workerShape && workerCarriesNoPoison;

            UnitEntityData soldier = CastExpandedSummoningCombatUnit(blueprints,
                caster, SummonFamily.NaturesAlly, "giant-ant-soldier", 3,
                created, evidence);
            RemoveExpandedSummoningAppearanceBuffs(soldier);
            BlueprintUnit.UnitBody soldierBody = soldier.Blueprint.Body;
            BlueprintItemWeapon soldierBite = soldierBody == null ? null :
                soldierBody.PrimaryHand as BlueprintItemWeapon;
            BlueprintItemWeapon soldierSting =
                soldierBody == null || soldierBody.AdditionalLimbs == null ||
                soldierBody.AdditionalLimbs.Length != 1 ? null :
                soldierBody.AdditionalLimbs[0];
            string soldierType = soldier.Blueprint.Type == null ? "<none>" :
                soldier.Blueprint.Type.name;
            int soldierBiteBonus = ProbeWeaponAttackBonus(soldier, hostile,
                soldierBite);
            int soldierStingBonus = soldierSting == null ? int.MinValue :
                ProbeWeaponAttackBonus(soldier, hostile, soldierSting);
            // Printed: "bite +3 (1d6+2 plus grab), sting +3 (1d4+2 plus
            // poison)" - both at the same bonus, so both are primary.
            bool soldierShape =
                soldier.Descriptor.State.Size == Size.Medium &&
                soldierBite != null && DescribeWeaponDice(soldierBite) == "1d6" &&
                soldierSting != null &&
                DescribeWeaponDice(soldierSting) == "1d4" &&
                soldierSting.name == "KMG_Summoning_Natural_AntSting1d4" &&
                !ReferenceEquals(soldierSting, soldierBite) &&
                soldierSting.Type != soldierBite.Type &&
                (soldierBody.AdditionalSecondaryLimbs == null ||
                    soldierBody.AdditionalSecondaryLimbs.Length == 0) &&
                soldierBiteBonus != int.MinValue &&
                soldierBiteBonus == soldierStingBonus &&
                soldierType == workerType;
            // The grab is on the primary limb alone, which is what keeps it off
            // the sting; the sting's weapon type is what keeps the poison off
            // the bite.
            SummonGrabComponent grab = SummonGrabComponent.Find(soldier);
            bool grabOnBiteOnly = grab != null && grab.GrabWithPrimaryHand &&
                grab.GrabAdditionalLimbCount == 0 && grab.RakeLimbCount == 0 &&
                grab.IsGrabLimb(soldier, SummonLimbs.PrimaryWeapon(soldier));
            rows.Add("giant-ant-soldier[size=" + soldier.Descriptor.State.Size +
                ";bite=" + DescribeWeaponDice(soldierBite) + "@" +
                soldierBiteBonus + ";sting=" +
                DescribeWeaponDice(soldierSting) + "@" + soldierStingBonus +
                ";distinctWeaponTypes=" + (soldierSting != null &&
                    soldierBite != null &&
                    soldierSting.Type != soldierBite.Type) +
                ";type=" + soldierType + ";grabPrimaryOnly=" + grabOnBiteOnly +
                "]");
            valid = valid && soldierShape && grabOnBiteOnly;

            evidence.Sprint14Profiles = valid;
            evidence.Sprint14ProfilesDetail = string.Join(";", rows.ToArray());
        }

        /// <summary>
        /// The ants' printed trip defence, and an audit of the beetle's.
        ///
        /// <para>Both ant castes print CMD 13 and 21 against trip. The builder
        /// reconstructs facts from the profile rather than inheriting the
        /// donor's, so this is the only thing that proves the multi-legged
        /// defence actually arrived.</para>
        ///
        /// <para>The beetle is audited rather than asserted. It prints CMD 9
        /// and 17 against trip, but Kingmaker exposes one movement speed and
        /// the project's airborne adaptation may already make it untrippable,
        /// which would be a stronger outcome than the printed one and an
        /// unavoidable consequence of that adaptation rather than a choice. The
        /// measurement is recorded either way.</para>
        /// </summary>
        private static void ExerciseSprint14TripDefence(
            BlueprintScriptableObject[] blueprints, UnitEntityData caster,
            UnitEntityData hostile, List<UnitEntityData> created,
            ExpandedSummoningMechanicalEvidence evidence)
        {
            var rows = new List<string>();
            bool valid = true;
            foreach (string[] row in new[] {
                new[] { "giant-ant-worker", "2" },
                new[] { "giant-ant-soldier", "3" } })
            {
                UnitEntityData ant = CastExpandedSummoningCombatUnit(blueprints,
                    caster, SummonFamily.NaturesAlly, row[0],
                    int.Parse(row[1], CultureInfo.InvariantCulture), created,
                    evidence);
                RemoveExpandedSummoningAppearanceBuffs(ant);
                int ordinary = ProbeCombatManeuverDefence(hostile, ant,
                    CombatManeuver.BullRush);
                int trip = ProbeCombatManeuverDefence(hostile, ant,
                    CombatManeuver.Trip);
                int perception = ant.Descriptor.Stats
                    .GetStat(StatType.SkillPerception).ModifiedValue;
                rows.Add(row[0] + "[cmd=" + ordinary + ";trip=" + trip +
                    ";delta=" + (trip - ordinary) + ";perception=" +
                    perception + "]");
                valid = valid && ordinary == 13 && trip == 21;
            }
            UnitEntityData beetle = CastExpandedSummoningCombatUnit(blueprints,
                caster, SummonFamily.NaturesAlly, "fire-beetle", 1, created,
                evidence);
            RemoveExpandedSummoningAppearanceBuffs(beetle);
            int beetleOrdinary = ProbeCombatManeuverDefence(hostile, beetle,
                CombatManeuver.BullRush);
            int beetleTrip = ProbeCombatManeuverDefence(hostile, beetle,
                CombatManeuver.Trip);
            // Audited, not asserted: the printed CMD 9 and 17 against trip are
            // recorded beside what the engine actually produces, so the
            // decision about whether the airborne adaptation has already made
            // the creature untrippable is made from a measurement. Kingmaker
            // exposes no trip-immunity condition, so a defence far above the
            // printed one is what that would look like here.
            rows.Add("fire-beetle[cmd=" + beetleOrdinary + ";trip=" +
                beetleTrip + ";delta=" + (beetleTrip - beetleOrdinary) +
                ";printedCmd=9;printedTrip=17;audited=true]");
            evidence.Sprint14TripDefence = valid;
            evidence.Sprint14TripDefenceDetail =
                string.Join(";", rows.ToArray());
        }

        /// <summary>
        /// An injury poison needs an injury, and neither rider may cross to the
        /// other attack.
        ///
        /// <para>The native graph fires on a hit, which is weaker than the
        /// tabletop rule: an attack that connects but whose damage is reduced
        /// to nothing has hit without wounding. Each case below is a real
        /// attack against a live target with the venom's presence measured
        /// afterwards.</para>
        /// </summary>
        private static void ExerciseSprint14PoisonDiscrimination(
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
            UnitEntityData soldier = CastExpandedSummoningCombatUnit(blueprints,
                caster, SummonFamily.NaturesAlly, "giant-ant-soldier", 3,
                created, evidence);
            RemoveExpandedSummoningAppearanceBuffs(soldier);
            BlueprintUnit.UnitBody body = soldier.Blueprint.Body;
            BlueprintItemWeapon bite = body.PrimaryHand as BlueprintItemWeapon;
            BlueprintItemWeapon sting = body.AdditionalLimbs == null ||
                body.AdditionalLimbs.Length != 1 ? null :
                body.AdditionalLimbs[0];
            var rows = new List<string>();
            bool valid = sting != null && bite != null;
            if (valid)
            {
                rows.Add(DescribeSprint14PoisonCase(blueprints, soldier, hostile, sting,
                    venom, true, true, "sting-wounds", true));
                rows.Add(DescribeSprint14PoisonCase(blueprints, soldier, hostile, sting,
                    venom, false, true, "sting-misses", false));
                rows.Add(DescribeSprint14PoisonCase(blueprints, soldier, hostile, sting,
                    venom, true, false, "sting-hits-no-damage", false));
                rows.Add(DescribeSprint14PoisonCase(blueprints, soldier, hostile, bite,
                    venom, true, true, "bite-wounds", false));
                valid = rows.All(value => value.EndsWith("expected", StringComparison.Ordinal));
            }
            evidence.Sprint14PoisonDiscrimination = valid;
            evidence.Sprint14PoisonDiscriminationDetail =
                string.Join(";", rows.ToArray());
        }


        private static void ExerciseSprint14DonorRigs(
            BlueprintScriptableObject[] blueprints, UnitEntityData caster,
            List<UnitEntityData> created,
            ExpandedSummoningMechanicalEvidence evidence,
            string evidenceDirectory)
        {
            var captured = new List<string>();
            bool valid = Sprint14DonorRigs.Length > 0;
            foreach (string[] row in Sprint14DonorRigs)
            {
                SummonFamily family = row[1] == "Monster" ?
                    SummonFamily.Monster : SummonFamily.NaturesAlly;
                int tier = int.Parse(row[2],
                    System.Globalization.CultureInfo.InvariantCulture);
                UnitEntityData rider = CastExpandedSummoningCombatUnit(
                    blueprints, caster, family, row[0], tier, created,
                    evidence);
                RemoveExpandedSummoningAppearanceBuffs(rider);
                string rig = CaptureDonorRig(rider, row[3], evidenceDirectory,
                    "Sprint 14");
                captured.Add(row[3] + "[" + rig + "]");
                // One renderer and a complete bind frame are what an original
                // mesh can actually be authored against; anything else is a
                // donor this pipeline cannot use.
                valid = valid &&
                    rig.IndexOf(",bones=", StringComparison.Ordinal) > 0 &&
                    rig.IndexOf(",renderers=1,", StringComparison.Ordinal) > 0;
            }
            evidence.Sprint14DonorRigs = valid;
            evidence.Sprint14DonorRigsDetail = string.Join(";",
                captured.ToArray());
        }
        /// <summary>
        /// One creature's live combat-manoeuvre defence, without attempting the
        /// manoeuvre. RuleCalculateCMD is the engine's own calculation, so a
        /// trip defence that exists only in a blueprint does not show up here.
        /// </summary>
        private static int ProbeCombatManeuverDefence(UnitEntityData attacker,
            UnitEntityData defender, CombatManeuver type)
        {
            if (attacker == null || defender == null ||
                ReferenceEquals(attacker, defender)) return int.MinValue;
            var rule = new RuleCalculateCMD(attacker, defender, type);
            Rulebook.Trigger(rule);
            return rule.Result;
        }

        /// <summary>
        /// An already-qualified project buff that carries enough physical
        /// damage reduction to absorb a 1d4+2 sting entirely.
        ///
        /// <para>Reused rather than created: the ledger forbids runtime
        /// blueprint generation, and a hit that deals no damage is exactly what
        /// the wound gate exists to refuse, so the fixture has to produce one
        /// honestly rather than by asserting it happened.</para>
        /// </summary>
        private static BlueprintBuff FindSprint14DamageShield(
            BlueprintScriptableObject[] blueprints)
        {
            return blueprints.OfType<BlueprintBuff>().FirstOrDefault(value =>
                value != null && value.ComponentsArray != null &&
                value.ComponentsArray.OfType<AddDamageResistancePhysical>()
                    .Any(dr => dr != null && dr.Value != null &&
                        dr.Value.Value >= 20));
        }

        /// <summary>
        /// One real attack with one named limb, and whether the venom followed.
        ///
        /// <para>The d20 is seeded so the hit or miss is the one the case
        /// needs, and the zero-damage case is produced by a real damage
        /// reduction on the target rather than by assuming the engine would
        /// have reduced it. The venom is cleared before and after so each case
        /// starts from nothing.</para>
        /// </summary>
        private static string DescribeSprint14PoisonCase(
            BlueprintScriptableObject[] blueprints, UnitEntityData soldier,
            UnitEntityData target, BlueprintItemWeapon blueprint,
            BlueprintBuff venom, bool shouldHit, bool allowDamage,
            string label, bool expectVenom)
        {
            ItemEntityWeapon weapon = LiveLimbWeapons(soldier).FirstOrDefault(
                value => ReferenceEquals(value.Blueprint, blueprint));
            if (weapon == null) return label + "[no-weapon]=unexpected";
            ClearSprint14Venom(target, venom);
            Buff shield = null;
            if (!allowDamage)
            {
                BlueprintBuff reduction = FindSprint14DamageShield(blueprints);
                if (reduction == null)
                    return label + "[no-damage-shield]=unexpected";
                shield = target.Descriptor.AddBuff(reduction, target, null);
                if (shield == null)
                    return label + "[shield-refused]=unexpected";
            }
            bool hit;
            int damage;
            try
            {
                UnityEngine.Random.InitState(
                    FindNativeD20Seed(shouldHit ? 20 : 1));
                var attack = new RuleAttackWithWeapon(soldier, target, weapon,
                    0);
                Rulebook.Trigger(attack);
                hit = attack.AttackRoll != null && attack.AttackRoll.IsHit;
                damage = attack.MeleeDamage == null ? 0 :
                    Math.Max(0, attack.MeleeDamage.Damage);
            }
            finally
            {
                if (shield != null) shield.Remove();
            }
            bool venomPresent = target.Descriptor.Buffs.GetBuff(venom) != null;
            ClearSprint14Venom(target, venom);
            bool matches = venomPresent == expectVenom && hit == shouldHit &&
                (allowDamage ? damage > 0 : damage == 0);
            return label + "[hit=" + hit + ";damage=" + damage + ";venom=" +
                venomPresent + ";wanted=" + expectVenom + "]=" +
                (matches ? "expected" : "unexpected");
        }

        private static void ClearSprint14Venom(UnitEntityData target,
            BlueprintBuff venom)
        {
            if (target == null || venom == null) return;
            Buff applied = target.Descriptor.Buffs.GetBuff(venom);
            if (applied != null) applied.Remove();
        }

    }
}
