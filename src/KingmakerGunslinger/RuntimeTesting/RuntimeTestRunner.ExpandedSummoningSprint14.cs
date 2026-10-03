using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker;
using Kingmaker.Designers.Mechanics.Buffs;
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
            ExerciseSprint14Senses(blueprints, caster, created, evidence);
            ExerciseSprint14WaspRequalification(blueprints, caster, hostile,
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
                beetleBite != null &&
                DescribeEffectiveWeaponDice(beetleBite) == "1d4" &&
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
                ";bite=" + DescribeEffectiveWeaponDice(beetleBite) + ";limbs=" +
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
                workerBite != null &&
                DescribeEffectiveWeaponDice(workerBite) == "1d6" &&
                (workerBody.AdditionalLimbs == null ||
                    workerBody.AdditionalLimbs.Length == 0) &&
                (workerBody.AdditionalSecondaryLimbs == null ||
                    workerBody.AdditionalSecondaryLimbs.Length == 0) &&
                workerType.IndexOf("Spider", StringComparison.OrdinalIgnoreCase) < 0 &&
                workerType.IndexOf("GiantAnt", StringComparison.Ordinal) >= 0;
            bool workerCarriesNoPoison = SummonGrabComponent.Find(worker) == null;
            rows.Add("giant-ant-worker[size=" + worker.Descriptor.State.Size +
                ";bite=" + DescribeEffectiveWeaponDice(workerBite) + ";limbs=" +
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
                soldierBite != null &&
                DescribeEffectiveWeaponDice(soldierBite) == "1d6" &&
                soldierSting != null &&
                DescribeEffectiveWeaponDice(soldierSting) == "1d4" &&
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
                ";bite=" + DescribeEffectiveWeaponDice(soldierBite) + "@" +
                soldierBiteBonus + ";sting=" +
                DescribeEffectiveWeaponDice(soldierSting) + "@" +
                soldierStingBonus +
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
            // The first guarded audit answered the open question: the beetle
            // measured CMD 9 and trip 9, so the airborne adaptation had not
            // made it untrippable and the printed bonus was simply absent. It
            // now carries the same multi-legged defence the ants do, so this is
            // an assertion rather than an audit.
            rows.Add("fire-beetle[cmd=" + beetleOrdinary + ";trip=" +
                beetleTrip + ";delta=" + (beetleTrip - beetleOrdinary) +
                ";printedCmd=9;printedTrip=17]");
            valid = valid && beetleOrdinary == 9 && beetleTrip == 17;
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
                // Not EndsWith("expected"): "unexpected" ends with
                // "expected", so the first version of this passed while two of
                // its four cases were failing in plain sight.
                valid = rows.All(value => value.EndsWith("=ok",
                    StringComparison.Ordinal));
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
        /// What senses, vermin traits and skill totals the live creatures
        /// actually have, and what the loaded library could ever give them.
        ///
        /// <para>The contract names exact printed totals rather than a racial
        /// component: Perception +0 on the Fire Beetle and +5 on both ant
        /// castes, where the ants' five is a Wisdom point on top of a racial
        /// +4 and nothing else. A total is therefore checked against the
        /// printed number and broken down by modifier descriptor, because a
        /// total alone cannot tell an unprinted class rank from the engine's
        /// own size bonus - the Fire Beetle reads Stealth 4 on no ranks at all,
        /// which is the Small size bonus every Small creature in Pathfinder
        /// gets and not something the generic builder added.</para>
        ///
        /// <para>Mind-affecting immunity is proved rather than inferred. A
        /// feature carrying a BuffDescriptorImmunity component is suggestive
        /// and no more, so a real native mind-affecting buff is applied to the
        /// insect, which must refuse it, and to the caster, which must accept
        /// it. Without that control an immunity assertion passes just as well
        /// when the fixture's buff was never applicable to anything.</para>
        ///
        /// <para>The last part is the bounded native audit the contract asks
        /// for before anything is declared impossible: every component on every
        /// loaded blueprint is searched for anything that could express scent,
        /// darkvision or low-light vision. The engine's own assembly holds
        /// VisionNormal, VisionLowLight and VisionDarkvision beside VisionColor
        /// and VisionRangeInMeters, which is a rendering enum rather than a
        /// creature sense, and the only mechanical sense plumbing it has is
        /// AddBlindsight with UnitPartBlindsense. This measures whether that
        /// reading is right where it matters, which is on the blueprints the
        /// game actually loaded.</para>
        /// </summary>
        private static void ExerciseSprint14Senses(
            BlueprintScriptableObject[] blueprints, UnitEntityData caster,
            List<UnitEntityData> created,
            ExpandedSummoningMechanicalEvidence evidence)
        {
            BlueprintBuff mindAffecting = FindSprint14MindAffectingBuff(
                blueprints, caster);
            var rows = new List<string>();
            bool contract = mindAffecting != null;
            foreach (string[] row in new[] {
                new[] { "fire-beetle", "1", "0" },
                new[] { "giant-ant-worker", "2", "5" },
                new[] { "giant-ant-soldier", "3", "5" } })
            {
                UnitEntityData unit = CastExpandedSummoningCombatUnit(blueprints,
                    caster, SummonFamily.NaturesAlly, row[0],
                    int.Parse(row[1], CultureInfo.InvariantCulture), created,
                    evidence);
                RemoveExpandedSummoningAppearanceBuffs(unit);
                int printedPerception = int.Parse(row[2],
                    CultureInfo.InvariantCulture);

                var components = new List<string>();
                bool darkvision = false;
                string darkvisionDetail = "<none>";
                int features = 0;
                foreach (BlueprintFeature feature in unit.Descriptor.Progression
                    .Features.Enumerable
                    .Select(value => value.Blueprint)
                    .OfType<BlueprintFeature>())
                {
                    features++;
                    foreach (BlueprintComponent component in
                        feature.ComponentsArray ??
                        Array.Empty<BlueprintComponent>())
                    {
                        string type = component.GetType().Name;
                        components.Add(feature.name + ":" + type);
                        if (type.IndexOf("Darkvision",
                                StringComparison.OrdinalIgnoreCase) >= 0 ||
                            type.IndexOf("LowLight",
                                StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            darkvision = true;
                            darkvisionDetail = feature.name + ":" + type;
                        }
                    }
                }

                ModifiableValue perception = unit.Descriptor.Stats
                    .GetStat(StatType.SkillPerception);
                ModifiableValue mobility = unit.Descriptor.Stats
                    .GetStat(StatType.SkillMobility);
                ModifiableValue stealth = unit.Descriptor.Stats
                    .GetStat(StatType.SkillStealth);

                // The printed total, not the racial component on its own.
                bool perceptionExact =
                    perception.ModifiedValue == printedPerception;
                // No class ranks in any of the three, which is what a stat
                // block printing no skill ranks requires. Anything left in the
                // total has to come from an attribute, a racial modifier or a
                // size bonus, and the breakdown below says which.
                bool noRanks = perception.BaseValue == 0 &&
                    mobility.BaseValue == 0 && stealth.BaseValue == 0;

                string immunityOutcome = DescribeSprint14MindImmunity(
                    mindAffecting, unit, caster);
                bool immune = immunityOutcome == "refused-on-vermin;" +
                    "accepted-on-caster";

                contract = contract && perceptionExact && noRanks && immune &&
                    !darkvision;
                rows.Add(row[0] + "[" +
                    DescribeSprint14Skill("perception", perception) + "@printed" +
                    printedPerception + (perceptionExact ? "=exact" : "=WRONG") +
                    ";" + DescribeSprint14Skill("mobility", mobility) +
                    ";" + DescribeSprint14Skill("stealth", stealth) +
                    ";noClassRanks=" + noRanks +
                    ";darkvision=" + darkvision + "(" + darkvisionDetail + ")" +
                    ";mindAffecting=" + immunityOutcome +
                    ";vermin=" + string.Join("/", components.Where(value =>
                        value.IndexOf("Vermin",
                            StringComparison.OrdinalIgnoreCase) >= 0)
                        .Distinct().ToArray()) +
                    ";features=" + features + "]");
            }

            rows.Add("mindAffectingProbe=" + (mindAffecting == null ?
                "<none-found:immunity-unproven>" : mindAffecting.name));
            rows.Add(DescribeSprint14SenseCensus(blueprints));
            evidence.Sprint14Senses = contract;
            evidence.Sprint14SensesDetail = string.Join(";", rows.ToArray());
        }

        /// <summary>
        /// A skill total with the breakdown that explains it.
        /// </summary>
        private static string DescribeSprint14Skill(string label,
            ModifiableValue value)
        {
            string parts = string.Join("+", value.Modifiers
                .Where(modifier => modifier != null)
                .Select(modifier => modifier.ModDescriptor + ":" +
                    modifier.ModValue)
                .ToArray());
            return label + "=" + value.ModifiedValue + "{ranks=" +
                value.BaseValue + ";" +
                (parts.Length == 0 ? "noModifiers" : parts) + "}";
        }

        /// <summary>
        /// A native mind-affecting buff the caster is demonstrably not immune
        /// to, so that a refusal on a vermin means immunity and not that the
        /// buff never applied to anything.
        /// </summary>
        private static BlueprintBuff FindSprint14MindAffectingBuff(
            BlueprintScriptableObject[] blueprints, UnitEntityData caster)
        {
            foreach (BlueprintBuff candidate in blueprints
                .OfType<BlueprintBuff>()
                .Where(value => value != null &&
                    (value.ComponentsArray ??
                        Array.Empty<BlueprintComponent>())
                    .OfType<SpellDescriptorComponent>()
                    .Any(descriptor =>
                        (descriptor.Descriptor.Value &
                            SpellDescriptor.MindAffecting) != 0))
                .OrderBy(value => value.name, StringComparer.Ordinal))
            {
                Buff applied = caster.Descriptor.AddBuff(candidate, caster,
                    null);
                bool took = caster.Descriptor.Buffs.GetBuff(candidate) != null;
                if (applied != null) applied.Remove();
                caster.Descriptor.Buffs.RemoveFact(candidate);
                if (took) return candidate;
            }
            return null;
        }

        /// <summary>
        /// Apply the probe to the vermin and to the caster, and say what each
        /// did. Only "refused-on-vermin;accepted-on-caster" is immunity.
        /// </summary>
        private static string DescribeSprint14MindImmunity(
            BlueprintBuff probe, UnitEntityData vermin, UnitEntityData caster)
        {
            if (probe == null) return "<no-probe>";
            Buff onVermin = vermin.Descriptor.AddBuff(probe, caster, null);
            bool verminTook = vermin.Descriptor.Buffs.GetBuff(probe) != null;
            if (onVermin != null) onVermin.Remove();
            vermin.Descriptor.Buffs.RemoveFact(probe);

            Buff onCaster = caster.Descriptor.AddBuff(probe, caster, null);
            bool casterTook = caster.Descriptor.Buffs.GetBuff(probe) != null;
            if (onCaster != null) onCaster.Remove();
            caster.Descriptor.Buffs.RemoveFact(probe);

            return (verminTook ? "accepted-on-vermin" : "refused-on-vermin") +
                ";" + (casterTook ? "accepted-on-caster" :
                    "refused-on-caster");
        }

        /// <summary>
        /// The bounded native audit: every component on every loaded blueprint
        /// that could express scent, darkvision or low-light vision, with how
        /// many blueprints carry each.
        /// </summary>
        private static string DescribeSprint14SenseCensus(
            BlueprintScriptableObject[] blueprints)
        {
            var counts = new Dictionary<string, int>(StringComparer.Ordinal);
            foreach (BlueprintScriptableObject blueprint in blueprints)
            {
                foreach (BlueprintComponent component in
                    blueprint.ComponentsArray ??
                    Array.Empty<BlueprintComponent>())
                {
                    if (component == null) continue;
                    string type = component.GetType().Name;
                    if (type.IndexOf("Vision", StringComparison.OrdinalIgnoreCase) < 0 &&
                        type.IndexOf("Scent", StringComparison.OrdinalIgnoreCase) < 0 &&
                        type.IndexOf("Blindsen", StringComparison.OrdinalIgnoreCase) < 0 &&
                        type.IndexOf("Blindsight", StringComparison.OrdinalIgnoreCase) < 0 &&
                        type.IndexOf("Darkvis", StringComparison.OrdinalIgnoreCase) < 0 &&
                        type.IndexOf("LowLight", StringComparison.OrdinalIgnoreCase) < 0)
                        continue;
                    int seen;
                    counts[type] = counts.TryGetValue(type, out seen) ?
                        seen + 1 : 1;
                }
            }
            string census = string.Join("/", counts
                .OrderBy(pair => pair.Key, StringComparer.Ordinal)
                .Select(pair => pair.Key + "x" + pair.Value).ToArray());
            return "senseComponentCensus=" +
                (census.Length == 0 ? "<none-in-library>" : census);
        }

        /// <summary>
        /// A weapon's effective damage dice.
        ///
        /// <para>A project-created weapon carries its own dice on the item. A
        /// native one usually does not: the item reads 0d0 and the dice live on
        /// its BlueprintWeaponType, which is why the ants' native 1d6 bite
        /// looked like nothing at all the first time this was measured. Both
        /// are read here, item first, so an override still wins where one
        /// exists.</para>
        /// </summary>
        private static string DescribeEffectiveWeaponDice(
            BlueprintItemWeapon weapon)
        {
            if (weapon == null) return "<none>";
            string own = DescribeWeaponDice(weapon);
            if (own != "0d0") return own;
            if (weapon.Type == null) return own + "(no-type)";
            FieldInfo field = weapon.Type.GetType().GetField("m_BaseDamage",
                BindingFlags.Instance | BindingFlags.Public |
                BindingFlags.NonPublic);
            if (field == null) return own + "(no-type-dice)";
            object value = field.GetValue(weapon.Type);
            if (!(value is DiceFormula)) return own + "(type-not-a-formula)";
            var formula = (DiceFormula)value;
            return formula.Rolls.ToString(CultureInfo.InvariantCulture) + "d" +
                ((int)formula.Dice).ToString(CultureInfo.InvariantCulture);
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
        private static BlueprintBuff[] FindSprint14DamageShields(
            BlueprintScriptableObject[] blueprints)
        {
            return blueprints.OfType<BlueprintBuff>().Where(value =>
                value != null && value.ComponentsArray != null &&
                value.ComponentsArray.OfType<AddDamageResistancePhysical>()
                    .Any(dr => dr != null)).ToArray();
        }

        /// <summary>
        /// Apply a damage reduction to the target and confirm it reduced the
        /// damage to nothing, trying each candidate until one does.
        ///
        /// <para>The first attempt applied one buff and assumed it worked. It
        /// did not: the sting still dealt ten through it, because a buff added
        /// without a mechanics context leaves its ContextValue unresolved and
        /// the reduction evaluates to zero. A fixture that cannot produce the
        /// situation it names must say so rather than quietly measure
        /// something else, so this returns null when nothing reaches zero and
        /// the case then fails on its own situation check.</para>
        /// </summary>
        private static Buff ApplySprint14DamageShield(
            BlueprintScriptableObject[] blueprints, UnitEntityData soldier,
            UnitEntityData target, ItemEntityWeapon weapon, int seed)
        {
            foreach (BlueprintBuff candidate in
                FindSprint14DamageShields(blueprints))
            {
                Buff applied = target.Descriptor.AddFact(candidate) as Buff;
                if (applied == null)
                    applied = target.Descriptor.AddBuff(candidate, target, null);
                if (applied == null) continue;
                UnityEngine.Random.InitState(seed);
                var probe = new RuleAttackWithWeapon(soldier, target, weapon, 0);
                Rulebook.Trigger(probe);
                int dealt = probe.MeleeDamage == null ? 0 :
                    Math.Max(0, probe.MeleeDamage.Damage);
                bool hit = probe.AttackRoll != null && probe.AttackRoll.IsHit;
                if (hit && dealt == 0) return applied;
                applied.Remove();
            }
            return null;
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

        /// <summary>
        /// The Giant Wasp's poison, requalified against the wound gate it now
        /// goes through.
        ///
        /// <para>Sprint 10 proved this poison against OnlyHit and recorded a
        /// pass. Sprint 14 then established that an injury poison delivered on
        /// a hit is wrong, because an attack reduced to zero damage has hit
        /// without wounding, and moved the entire native graph inside
        /// ContextActionOnlyIfWeaponWounded. The wasp shares that carrier, so
        /// the earlier evidence no longer describes the shipping code and the
        /// creature has to earn its pass again.</para>
        ///
        /// <para>Narrow on purpose. The three delivery outcomes across the new
        /// gate, the printed numbers the gate sits on top of and could have
        /// disturbed, and the two failure modes that a gate inserted into a
        /// trigger graph can actually introduce: firing twice for one attack,
        /// and leaving a delivered poison dependent on a source that then
        /// dies. Nothing here reopens the attachment, quantity, visual or
        /// vermin-immunity work Sprint 10 qualified separately.</para>
        /// </summary>
        private static void ExerciseSprint14WaspRequalification(
            BlueprintScriptableObject[] blueprints, UnitEntityData caster,
            UnitEntityData hostile, List<UnitEntityData> created,
            ExpandedSummoningMechanicalEvidence evidence)
        {
            BlueprintBuff venom = blueprints.OfType<BlueprintBuff>()
                .FirstOrDefault(value => value != null && value.name ==
                    "KMG_Summoning_Natural_GiantWasp_Venom");
            if (venom == null)
                throw new InvalidOperationException(
                    "The Giant Wasp venom buff was not loaded.");
            UnitEntityData wasp = CastExpandedSummoningCombatUnit(blueprints,
                caster, SummonFamily.NaturesAlly, "giant-wasp", 4, created,
                evidence);
            RemoveExpandedSummoningAppearanceBuffs(wasp);
            BlueprintItemWeapon sting =
                wasp.Blueprint.Body.PrimaryHand as BlueprintItemWeapon;
            var rows = new List<string>();
            bool valid = sting != null;
            if (!valid)
            {
                evidence.Sprint14WaspRequalification = false;
                evidence.Sprint14WaspRequalificationDetail =
                    "the wasp has no sting on its primary limb";
                return;
            }

            // The printed numbers, read off the live buff rather than the
            // builder's intent: Fortitude DC 18, 1d2 Dexterity, six exposures,
            // one successful save cures.
            BuffPoisonStatDamage poison = (venom.ComponentsArray ??
                Array.Empty<BlueprintComponent>())
                .OfType<BuffPoisonStatDamage>().FirstOrDefault();
            int constitutionBonus = wasp.Descriptor.Stats.Constitution
                .ModifiedValue / 2 - 5;
            int derivedDc = GiantWaspPoisonPolicy.DifficultyClass(
                constitutionBonus);
            bool numbersExact = poison != null &&
                poison.Stat == StatType.Dexterity &&
                poison.Value.Dice == DiceType.D2 &&
                poison.Value.Rolls == 1 &&
                poison.Ticks == GiantWaspPoisonPolicy.Exposures &&
                poison.SuccesfullSaves == GiantWaspPoisonPolicy.SavesToCure &&
                poison.SaveType == SavingThrowType.Fortitude &&
                derivedDc == Sprint14WaspPrintedPoisonDc;
            rows.Add("printed[stat=" + (poison == null ? "<none>" :
                    poison.Stat.ToString()) + ";dice=" + (poison == null ?
                    "<none>" : poison.Value.Rolls + "d" + poison.Value.Dice) +
                ";exposures=" + (poison == null ? -1 : poison.Ticks) +
                ";savesToCure=" + (poison == null ? -1 :
                    poison.SuccesfullSaves) + ";saveType=" + (poison == null ?
                    "<none>" : poison.SaveType.ToString()) + ";liveCon=" +
                wasp.Descriptor.Stats.Constitution.ModifiedValue +
                ";derivedDc=" + derivedDc + ";printedDc=" +
                Sprint14WaspPrintedPoisonDc + "]" +
                (numbersExact ? "=ok" : "=wrong"));

            // The gate itself, through the same three situations the ant's
            // sting is held to.
            rows.Add(DescribeSprint14PoisonCase(blueprints, wasp, hostile,
                sting, venom, true, true, "wasp-sting-wounds", true));
            rows.Add(DescribeSprint14PoisonCase(blueprints, wasp, hostile,
                sting, venom, false, true, "wasp-sting-misses", false));
            rows.Add(DescribeSprint14PoisonCase(blueprints, wasp, hostile,
                sting, venom, true, false, "wasp-sting-hits-no-damage",
                false));

            // One attack, one application. A gate wrapped around a trigger
            // graph is exactly the kind of change that can run the graph twice.
            ClearSprint14Venom(hostile, venom);
            ItemEntityWeapon liveSting = LiveLimbWeapons(wasp).FirstOrDefault(
                value => ReferenceEquals(value.Blueprint, sting));
            int applications = -1;
            int dcOnApplied = -1;
            if (liveSting != null)
            {
                UnityEngine.Random.InitState(FindNativeD20Seed(20));
                Rulebook.Trigger(new RuleAttackWithWeapon(wasp, hostile,
                    liveSting, 0));
                applications = hostile.Descriptor.Buffs.Enumerable
                    .Count(value => value != null &&
                        ReferenceEquals(value.Blueprint, venom));
                Buff applied = hostile.Descriptor.Buffs.GetBuff(venom);
                dcOnApplied = applied == null || applied.Context == null ?
                    -1 : applied.Context.Params.DC;
            }
            bool singleApplication = applications == 1 &&
                dcOnApplied == Sprint14WaspPrintedPoisonDc;
            rows.Add("oneAttackOneApplication[applications=" + applications +
                ";dcOnAppliedBuff=" + dcOnApplied + "]" +
                (singleApplication ? "=ok" : "=wrong"));

            // A delivered poison belongs to the victim. Destroying the wasp
            // must not remove it, retarget it or make ticking it throw.
            string outlives = DescribeSprint14WaspOutlivesSource(wasp, hostile,
                caster, venom);
            rows.Add(outlives);

            valid = rows.All(value => value.EndsWith("=ok",
                StringComparison.Ordinal));
            evidence.Sprint14WaspRequalification = valid;
            evidence.Sprint14WaspRequalificationDetail =
                string.Join(";", rows.ToArray());
        }

        /// <summary>The wasp's printed Fortitude DC.</summary>
        private const int Sprint14WaspPrintedPoisonDc = 18;

        /// <summary>
        /// Destroy the wasp with its venom already in the victim, and read what
        /// the victim is left holding.
        /// </summary>
        private static string DescribeSprint14WaspOutlivesSource(
            UnitEntityData wasp, UnitEntityData hostile, UnitEntityData caster,
            BlueprintBuff venom)
        {
            Buff applied = hostile.Descriptor.Buffs.GetBuff(venom);
            if (applied == null)
                return "outlivesSource[no-venom-to-test]=wrong";
            TimeSpan before = applied.TimeLeft;
            int dcBefore = applied.Context == null ? -1 :
                applied.Context.Params.DC;
            TimeSpan clock = Game.Instance.Player.GameTime;
            wasp.Destroy();
            Game.Instance.EntityDestroyer.Tick();
            bool sourceGone = wasp.Destroyed;
            bool survived;
            bool tickSafe;
            string tickDetail;
            try
            {
                Game.Instance.Player.GameTime = clock +
                    TimeSpan.FromSeconds(12d);
                hostile.Descriptor.Buffs.Tick();
                survived = hostile.Descriptor.HasFact(venom);
                tickSafe = true;
                tickDetail = "ticked";
            }
            catch (Exception exception)
            {
                survived = hostile.Descriptor.HasFact(venom);
                tickSafe = false;
                tickDetail = "threw:" + exception.GetType().Name;
            }
            finally { Game.Instance.Player.GameTime = clock; }
            Buff after = hostile.Descriptor.Buffs.GetBuff(venom);
            UnitEntityData owner = after == null || after.Context == null ?
                null : after.Context.MaybeCaster;
            bool ownerGone = owner == null || owner.Destroyed;
            bool noRetarget = !ReferenceEquals(owner, hostile) &&
                !ReferenceEquals(owner, caster);
            int dcAfter = after == null || after.Context == null ? -1 :
                after.Context.Params.DC;
            bool ok = sourceGone && survived && tickSafe && ownerGone &&
                noRetarget && dcAfter == dcBefore;
            string result = "outlivesSource[sourceDestroyed=" + sourceGone +
                ";venomSurvived=" + survived + ";tick=" + tickDetail +
                ";ownerGone=" + ownerGone + ";retargeted=" + !noRetarget +
                ";dcBefore=" + dcBefore + ";dcAfter=" + dcAfter +
                ";timeLeftBefore=" + before.TotalSeconds.ToString("0.##",
                    CultureInfo.InvariantCulture) + "]" +
                (ok ? "=ok" : "=wrong");
            ClearSprint14Venom(hostile, venom);
            return result;
        }

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
            int seed = FindNativeD20Seed(
                !shouldHit ? 1 : allowDamage ? 20 : 19);
            Buff shield = null;
            if (!allowDamage)
            {
                shield = ApplySprint14DamageShield(blueprints, soldier, target,
                    weapon, seed);
                ClearSprint14Venom(target, venom);
                if (shield == null)
                    return label + "[no-reduction-reached-zero]=wrong";
            }
            bool hit;
            int damage;
            try
            {
                // A natural 20 threatens, and a confirmed critical doubles
                // the damage, which is how the first zero-damage attempt still
                // dealt ten through a reduction that should have absorbed it.
                // The wounding cases keep the 20 because they want a hit; the
                // zero-damage case takes the lowest roll that still hits.
                UnityEngine.Random.InitState(seed);
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
            // A case only counts when the engine actually produced the
            // situation it names. A miss must miss; a wounding case must wound;
            // and the zero-damage case must really have been reduced to zero,
            // because a shield that failed to apply would otherwise look like a
            // passing proof that a wounding hit delivers no venom.
            bool situation = hit == shouldHit &&
                (!shouldHit ? damage == 0 : allowDamage ? damage > 0 :
                    damage == 0);
            bool matches = situation && venomPresent == expectVenom;
            return label + "[hit=" + hit + ";damage=" + damage + ";venom=" +
                venomPresent + ";wanted=" + expectVenom + ";situation=" +
                situation + "]=" + (matches ? "ok" : "wrong");
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
