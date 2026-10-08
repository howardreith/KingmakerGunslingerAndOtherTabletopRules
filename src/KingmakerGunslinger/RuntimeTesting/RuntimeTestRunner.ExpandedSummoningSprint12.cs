using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Items;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        private static void ExerciseExpandedSummoningSprint12DiseasePack(
            BlueprintScriptableObject[] blueprints, UnitEntityData caster,
            UnitEntityData hostile,
            Kingmaker.EntitySystem.SceneEntitiesState scene,
            List<UnitEntityData> created,
            ExpandedSummoningMechanicalEvidence evidence,
            string evidenceDirectory)
        {
            BlueprintBuff filthFever = blueprints.OfType<BlueprintBuff>()
                .Single(value => value.AssetGuid ==
                    "9545a5550d89feb47a84edaeb4e63d0b");
            BlueprintBuff reaction = blueprints.OfType<BlueprintBuff>()
                .Single(value => value.name ==
                    "KMG_Summoning_Natural_GoblinDog_AllergicReaction");
            BlueprintUnitType goblinType = blueprints.OfType<BlueprintUnitType>()
                .Single(value => value.AssetGuid ==
                    "d524df24b2f38cf4590525b2e7c4f34e");
            BlueprintAbility cure = blueprints.OfType<BlueprintAbility>()
                .Where(value => value.Type == AbilityType.Spell &&
                    value.name.IndexOf("CureLightWounds",
                        StringComparison.OrdinalIgnoreCase) >= 0)
                .OrderBy(value => value.Parent == null ? 0 : 1)
                .ThenBy(value => value.AssetGuid, StringComparer.Ordinal)
                .FirstOrDefault();
            BlueprintAbility removeDisease = blueprints.OfType<BlueprintAbility>()
                .Where(value => value.Type == AbilityType.Spell &&
                    value.name.IndexOf("RemoveDisease",
                        StringComparison.OrdinalIgnoreCase) >= 0)
                .OrderBy(value => value.Parent == null ? 0 : 1)
                .ThenBy(value => value.AssetGuid, StringComparer.Ordinal)
                .FirstOrDefault();
            if (cure == null || removeDisease == null)
                throw new InvalidOperationException(
                    "The native Cure Light Wounds or Remove Disease spell was not loaded.");

            int hostileFortitude = hostile.Descriptor.Stats.SaveFortitude.BaseValue;
            int hostileDexterity = hostile.Descriptor.Stats.Dexterity.BaseValue;
            int hostileCharisma = hostile.Descriptor.Stats.Charisma.BaseValue;
            int hostileDexterityDamage = hostile.Descriptor.Stats.Dexterity.Damage;
            int hostileCharismaDamage = hostile.Descriptor.Stats.Charisma.Damage;
            int hostileConstitutionDamage =
                hostile.Descriptor.Stats.Constitution.Damage;
            int hostileDamage = hostile.Descriptor.Damage;
            int casterDamage = caster.Descriptor.Damage;
            UnitEntityData secondVictim = null;
            UnitEntityData goblinVictim = null;
            BlueprintUnit secondVictimBlueprint = null;
            BlueprintUnit goblinVictimBlueprint = null;
            try
            {
                UnitEntityData direRat = CastExpandedSummoningCombatUnit(
                    blueprints, caster, SummonFamily.NaturesAlly, "dire-rat", 1,
                    created, evidence);
                RemoveExpandedSummoningAppearanceBuffs(direRat);
                string dogRig = CaptureSprint12DonorRig(direRat, "dog",
                    evidenceDirectory);
                UnitEntityData hyena = CastExpandedSummoningCombatUnit(
                    blueprints, caster, SummonFamily.NaturesAlly, "hyena", 2,
                    created, evidence);
                RemoveExpandedSummoningAppearanceBuffs(hyena);
                string wolfRig = CaptureSprint12DonorRig(hyena, "wolf",
                    evidenceDirectory);
                hostile.Descriptor.Stats.SaveFortitude.BaseValue = -100;
                RemoveSprint12Buff(hostile, filthFever);
                bool direRatHit = ExerciseExpandedSummoningAttack(direRat,
                    hostile, out string direRatHitDetail);
                Buff failedDisease = hostile.Descriptor.Buffs.GetBuff(filthFever);
                int direRatDc = failedDisease == null ||
                    failedDisease.Context == null ? -1 :
                    failedDisease.Context.Params.DC;
                bool direRatSource = failedDisease != null &&
                    failedDisease.Context != null &&
                    ReferenceEquals(failedDisease.Context.MaybeCaster, direRat);
                RemoveSprint12Buff(hostile, filthFever);

                hostile.Descriptor.Stats.SaveFortitude.BaseValue = 100;
                bool direRatSavedHit = ExerciseExpandedSummoningAttack(direRat,
                    hostile, out string direRatSavedDetail);
                bool savePreventedDisease =
                    !hostile.Descriptor.HasFact(filthFever);

                hostile.Descriptor.Stats.SaveFortitude.BaseValue = -100;
                ItemEntityWeapon ratBite = direRat.Body.PrimaryHand.MaybeWeapon;
                if (ratBite == null)
                    throw new InvalidOperationException(
                        "The live Dire Rat fixture has no primary bite weapon.");
                UnityEngine.Random.InitState(FindNativeD20Seed(1));
                var missRule = new RuleAttackWithWeapon(direRat, hostile,
                    ratBite, 0);
                Rulebook.Trigger(missRule);
                bool missPreventedDisease = missRule.AttackRoll != null &&
                    !missRule.AttackRoll.IsHit &&
                    !hostile.Descriptor.HasFact(filthFever);

                UnityEngine.Random.InitState(FindNativeD20Seed(20));
                var replayRule = new RuleAttackWithWeapon(direRat, hostile,
                    ratBite, 0);
                Rulebook.Trigger(replayRule);
                Buff beforeReplay = hostile.Descriptor.Buffs.GetBuff(filthFever);
                SummonInjuryDiseaseComponent ratDelivery =
                    Sprint12DiseaseDelivery(direRat,
                        "KMG_Summoning_Natural_DireRat_Disease");
                ratDelivery.OnEventDidTrigger(replayRule);
                Buff afterReplay = hostile.Descriptor.Buffs.GetBuff(filthFever);
                bool replaySuppressed = beforeReplay != null &&
                    ReferenceEquals(beforeReplay, afterReplay);
                evidence.Sprint12DireRatDisease = direRatHit &&
                    direRatSavedHit && direRatDc ==
                        SummonInjuryDiseasePolicy.DireRatFortitudeDc &&
                    direRatSource && savePreventedDisease &&
                    missPreventedDisease && replaySuppressed;
                evidence.Sprint12DireRatDiseaseDetail = "failed[" +
                    direRatHitDetail + ";dc=" + direRatDc + ";source=" +
                    direRatSource + "];saved[" + direRatSavedDetail +
                    ";absent=" + savePreventedDisease + "];miss=" +
                    missPreventedDisease + ";replaySameBuff=" +
                    replaySuppressed;
                RemoveSprint12Buff(hostile, filthFever);

                UnitEntityData goblinDog = CastExpandedSummoningCombatUnit(
                    blueprints, caster, SummonFamily.NaturesAlly, "goblin-dog",
                    2, created, evidence);
                RemoveExpandedSummoningAppearanceBuffs(goblinDog);
                string worgRig = CaptureSprint12DonorRig(goblinDog, "worg",
                    evidenceDirectory);
                evidence.Sprint12DonorRigs = dogRig.StartsWith("dog:",
                        StringComparison.Ordinal) &&
                    wolfRig.StartsWith("wolf:", StringComparison.Ordinal) &&
                    worgRig.StartsWith("worg:", StringComparison.Ordinal);
                evidence.Sprint12DonorRigsDetail = dogRig + ";" + wolfRig +
                    ";" + worgRig;
                bool direRatVisual = DescribeSprint12OriginalVisual(direRat,
                    "dire-rat", out string direRatVisualDetail);
                bool hyenaVisual = DescribeSprint12OriginalVisual(hyena,
                    "hyena", out string hyenaVisualDetail);
                bool goblinDogVisual = DescribeSprint12OriginalVisual(goblinDog,
                    "goblin-dog", out string goblinDogVisualDetail);
                evidence.Sprint12OriginalVisuals = direRatVisual &&
                    hyenaVisual && goblinDogVisual;
                evidence.Sprint12OriginalVisualsDetail = "direct[" +
                    direRatVisualDetail + ";" + hyenaVisualDetail + ";" +
                    goblinDogVisualDetail + "]";
                RuleApplyBuff reactionImmunity = ApplySprint12Buff(goblinDog,
                    reaction, hostile,
                    TimeSpan.FromSeconds(
                        SummonInjuryDiseasePolicy.GoblinDogAllergyDurationSeconds),
                    SummonInjuryDiseasePolicy.GoblinDogFortitudeDc);
                RuleApplyBuff filthImmunity = ApplySprint12Buff(goblinDog,
                    filthFever, hostile, null,
                    SummonInjuryDiseasePolicy.DireRatFortitudeDc);
                bool diseaseImmune = reactionImmunity.Immunity &&
                    !reactionImmunity.CanApply &&
                    reactionImmunity.AppliedBuff == null &&
                    filthImmunity.Immunity && !filthImmunity.CanApply &&
                    filthImmunity.AppliedBuff == null;

                RemoveSprint12Buff(hostile, reaction);
                hostile.Descriptor.Stats.SaveFortitude.BaseValue = -100;
                int dexterityBefore =
                    hostile.Descriptor.Stats.Dexterity.ModifiedValue;
                int charismaBefore =
                    hostile.Descriptor.Stats.Charisma.ModifiedValue;
                bool allergyHit = ExerciseExpandedSummoningAttack(goblinDog,
                    hostile, out string allergyHitDetail);
                Buff firstReaction = hostile.Descriptor.Buffs.GetBuff(reaction);
                int allergyDc = firstReaction == null ||
                    firstReaction.Context == null ? -1 :
                    firstReaction.Context.Params.DC;
                double allergySeconds = firstReaction == null ? -1d :
                    firstReaction.TimeLeft.TotalSeconds;
                int dexterityAfter =
                    hostile.Descriptor.Stats.Dexterity.ModifiedValue;
                int charismaAfter =
                    hostile.Descriptor.Stats.Charisma.ModifiedValue;
                bool allergyPenalty = dexterityBefore - dexterityAfter == 2 &&
                    charismaBefore - charismaAfter == 2;
                bool secondAllergyHit = ExerciseExpandedSummoningAttack(
                    goblinDog, hostile, out string secondAllergyDetail);
                Buff replacedReaction = hostile.Descriptor.Buffs.GetBuff(reaction);
                int reactionCount = hostile.Descriptor.Buffs.RawFacts
                    .OfType<Buff>().Count(value =>
                        ReferenceEquals(value.Blueprint, reaction));
                bool nonstacking = reactionCount == 1 &&
                    hostile.Descriptor.Stats.Dexterity.ModifiedValue ==
                        dexterityAfter &&
                    hostile.Descriptor.Stats.Charisma.ModifiedValue ==
                        charismaAfter;

                hostile.Descriptor.Damage = Math.Max(20,
                    hostile.Descriptor.Damage);
                RuleHealDamage ordinaryHealing = Rulebook.Trigger(
                    new RuleHealDamage(caster, hostile,
                        new DiceFormula(0, DiceType.D6), 5));
                bool ordinaryPreserved = ordinaryHealing.Value > 0 &&
                    hostile.Descriptor.HasFact(reaction);
                hostile.Descriptor.Damage = Math.Max(20,
                    hostile.Descriptor.Damage);
                var cureContext = new MechanicsContext(caster,
                    hostile.Descriptor, cure, null,
                    new TargetWrapper(hostile));
                var magicalHealing = new RuleHealDamage(caster, hostile,
                    new DiceFormula(0, DiceType.D6), 5) {
                    Reason = new RuleReason(cureContext)
                };
                Rulebook.Trigger(magicalHealing);
                bool magicalRemoved = magicalHealing.Value > 0 &&
                    !hostile.Descriptor.HasFact(reaction);

                RuleApplyBuff removable = ApplySprint12Buff(caster, reaction,
                    goblinDog,
                    TimeSpan.FromSeconds(
                        SummonInjuryDiseasePolicy.GoblinDogAllergyDurationSeconds),
                    SummonInjuryDiseasePolicy.GoblinDogFortitudeDc);
                bool removeDiseaseApplied = removable.AppliedBuff != null;
                caster.Descriptor.AddFact(removeDisease);
                try
                {
                    UnityEngine.Random.InitState(FindNativeD20Seed(20));
                    ExecuteExpandedSummoningRuntimeAbility(caster,
                        removeDisease, 3, new TargetWrapper(caster), true);
                }
                finally
                {
                    if (caster.Descriptor.HasFact(removeDisease))
                        caster.Descriptor.RemoveFact(removeDisease);
                }
                bool removeDiseaseRemoved = removeDiseaseApplied &&
                    !caster.Descriptor.HasFact(reaction);

                goblinVictimBlueprint = UnityEngine.Object.Instantiate(
                    hostile.Blueprint);
                goblinVictimBlueprint.name =
                    "KMG_Runtime_Sprint12_ExactGoblinVictim";
                goblinVictimBlueprint.IsCheater = true;
                goblinVictimBlueprint.Type = goblinType;
                goblinVictim = Game.Instance.EntityCreator.SpawnUnit(
                    goblinVictimBlueprint, hostile.Position + Vector3.right * 2f,
                    Quaternion.identity, scene);
                Game.Instance.EntityCreator.Tick();
                if (goblinVictim == null || goblinVictim.Descriptor == null)
                    throw new InvalidOperationException(
                        "The exact native Goblin-type fixture did not spawn.");
                goblinVictim.Descriptor.Stats.HitPoints.BaseValue = 100000;
                goblinVictim.Descriptor.Stats.SaveFortitude.BaseValue = -100;
                bool goblinHit = ExerciseExpandedSummoningAttack(goblinDog,
                    goblinVictim, out string goblinDetail);
                bool goblinExempt = goblinHit &&
                    ReferenceEquals(goblinVictim.Blueprint.Type, goblinType) &&
                    !goblinVictim.Descriptor.HasFact(reaction);

                bool allergyDuration = allergySeconds > 86398d &&
                    allergySeconds <=
                        SummonInjuryDiseasePolicy.GoblinDogAllergyDurationSeconds;
                evidence.Sprint12GoblinDogAllergy = diseaseImmune &&
                    allergyHit && secondAllergyHit && allergyDc ==
                        SummonInjuryDiseasePolicy.GoblinDogFortitudeDc &&
                    allergyDuration && allergyPenalty && nonstacking &&
                    ordinaryPreserved && magicalRemoved &&
                    removeDiseaseRemoved && goblinExempt;
                evidence.Sprint12GoblinDogAllergyDetail = "immunity[reaction=" +
                    reactionImmunity.CanApply + "/" +
                    reactionImmunity.Immunity + ";filth=" +
                    filthImmunity.CanApply + "/" + filthImmunity.Immunity +
                    "];first[" + allergyHitDetail + ";dc=" + allergyDc +
                    ";seconds=" + allergySeconds.ToString("0.###",
                        System.Globalization.CultureInfo.InvariantCulture) +
                    ";stats=" + dexterityBefore + "/" + charismaBefore +
                    "->" + dexterityAfter + "/" + charismaAfter +
                    "];second[" + secondAllergyDetail + ";count=" +
                    reactionCount + ";nonstacking=" + nonstacking +
                    "];ordinaryHealingPreserved=" + ordinaryPreserved +
                    ";magicalHealingRemoved=" + magicalRemoved +
                    ";removeDiseaseRemoved=" + removeDiseaseRemoved +
                    ";goblin[" + goblinDetail + ";exempt=" + goblinExempt +
                    "]";
                RemoveSprint12Buff(hostile, reaction);

                // Correction order (2026-10-01): the printed allergic reaction
                // has three triggers. The bite above is one; a natural or
                // unarmed attacker that damages the Goblin Dog and a creature
                // that attempts to grapple it are the other two.
                ExerciseSprint12ContactAllergy(blueprints, goblinDog, hyena,
                    caster, hostile, goblinVictim, reaction, evidence);
                RemoveSprint12Buff(hostile, reaction);
                RemoveSprint12Buff(hyena, reaction);
                RemoveSprint12Buff(caster, reaction);

                // The printed Dire Rat has no natural armor and no Weapon
                // Finesse; the Dog's +1 natural armor is the positive control
                // that proves the check can see natural armor at all.
                ExerciseSprint12PrintedDefences(blueprints, caster,
                    created, evidence);

                secondVictimBlueprint = UnityEngine.Object.Instantiate(
                    hostile.Blueprint);
                secondVictimBlueprint.name =
                    "KMG_Runtime_Sprint12_SecondDiseaseVictim";
                secondVictimBlueprint.IsCheater = true;
                secondVictim = Game.Instance.EntityCreator.SpawnUnit(
                    secondVictimBlueprint,
                    hostile.Position + Vector3.left * 2f,
                    Quaternion.identity, scene);
                Game.Instance.EntityCreator.Tick();
                if (secondVictim == null || secondVictim.Descriptor == null)
                    throw new InvalidOperationException(
                        "The second Sprint 12 disease victim did not spawn.");
                secondVictim.Descriptor.Stats.HitPoints.BaseValue = 100000;
                secondVictim.Descriptor.Stats.SaveFortitude.BaseValue = -100;
                hostile.Descriptor.Stats.SaveFortitude.BaseValue = -100;

                UnitEntityData[] rats = CastExpandedSummoningVariant(blueprints,
                    caster, ExpandedSummoningVariant(SummonFamily.NaturesAlly,
                        "dire-rat", 3, SummonMultiplicity.OneD4PlusOne), null,
                    evidence);
                created.AddRange(rats);
                string ratQuantityFirst = "not-run";
                string ratQuantitySecond = "not-run";
                bool ratQuantity = rats.Length >= 2 &&
                    ExerciseExpandedSummoningAttack(rats[0], hostile,
                        out ratQuantityFirst) &&
                    ExerciseExpandedSummoningAttack(rats[1], secondVictim,
                        out ratQuantitySecond);
                Buff firstRatDisease =
                    hostile.Descriptor.Buffs.GetBuff(filthFever);
                Buff secondRatDisease =
                    secondVictim.Descriptor.Buffs.GetBuff(filthFever);
                bool ratIsolation = firstRatDisease != null &&
                    secondRatDisease != null &&
                    firstRatDisease.Context != null &&
                    secondRatDisease.Context != null &&
                    ReferenceEquals(firstRatDisease.Context.MaybeCaster, rats[0]) &&
                    ReferenceEquals(secondRatDisease.Context.MaybeCaster, rats[1]);
                RemoveSprint12Buff(hostile, filthFever);
                RemoveSprint12Buff(secondVictim, filthFever);

                UnitEntityData[] dogs = CastExpandedSummoningVariant(blueprints,
                    caster, ExpandedSummoningVariant(SummonFamily.NaturesAlly,
                        "goblin-dog", 4, SummonMultiplicity.OneD4PlusOne), null,
                    evidence);
                created.AddRange(dogs);
                string dogQuantityFirst = "not-run";
                string dogQuantitySecond = "not-run";
                bool dogQuantity = dogs.Length >= 2 &&
                    ExerciseExpandedSummoningAttack(dogs[0], hostile,
                        out dogQuantityFirst) &&
                    ExerciseExpandedSummoningAttack(dogs[1], secondVictim,
                        out dogQuantitySecond);
                Buff firstDogReaction =
                    hostile.Descriptor.Buffs.GetBuff(reaction);
                Buff secondDogReaction =
                    secondVictim.Descriptor.Buffs.GetBuff(reaction);
                bool dogIsolation = firstDogReaction != null &&
                    secondDogReaction != null &&
                    firstDogReaction.Context != null &&
                    secondDogReaction.Context != null &&
                    ReferenceEquals(firstDogReaction.Context.MaybeCaster, dogs[0]) &&
                    ReferenceEquals(secondDogReaction.Context.MaybeCaster, dogs[1]);
                evidence.Sprint12DiseaseQuantity = ratQuantity &&
                    ratIsolation && dogQuantity && dogIsolation;
                evidence.Sprint12DiseaseQuantityDetail = "rats=" + rats.Length +
                    ";attacks=[" + ratQuantityFirst + "][" +
                    ratQuantitySecond + "];isolated=" + ratIsolation +
                    ";dogs=" + dogs.Length + ";attacks=[" +
                    dogQuantityFirst + "][" + dogQuantitySecond +
                    "];isolated=" + dogIsolation;
                var quantityVisuals = new List<string>();
                bool quantityOriginals = true;
                foreach (UnitEntityData rat in rats)
                {
                    bool valid = DescribeSprint12OriginalVisual(rat,
                        "dire-rat", out string detail);
                    quantityOriginals &= valid;
                    quantityVisuals.Add(detail);
                }
                foreach (UnitEntityData dog in dogs)
                {
                    bool valid = DescribeSprint12OriginalVisual(dog,
                        "goblin-dog", out string detail);
                    quantityOriginals &= valid;
                    quantityVisuals.Add(detail);
                }
                bool dogNativeVisual = !ExpandedSummoningPteranodonViewPatch
                    .HandlesBlueprintName("KMG_Summoning_Unit_Dog");
                evidence.Sprint12OriginalVisuals &= quantityOriginals &&
                    dogNativeVisual;
                evidence.Sprint12OriginalVisualsDetail += ";quantity[" +
                    string.Join("|", quantityVisuals.ToArray()) +
                    "];nativeDogMapped=" +
                    !dogNativeVisual;

                // The disease/allergy lifetime contract: an effect already
                // inflicted on a victim belongs to the victim and must keep
                // ticking, curing and saving after its summoned source is
                // gone, while the source leaves no project-owned residue and
                // can expose nothing further.
                ExerciseSprint12DiseaseOutlivesSource(rats, dogs, secondVictim,
                    filthFever, reaction, cure, caster, evidence);
            }
            finally
            {
                RemoveSprint12Buff(hostile, filthFever);
                RemoveSprint12Buff(hostile, reaction);
                RemoveSprint12Buff(caster, reaction);
                hostile.Descriptor.Stats.SaveFortitude.BaseValue =
                    hostileFortitude;
                hostile.Descriptor.Stats.Dexterity.BaseValue = hostileDexterity;
                hostile.Descriptor.Stats.Charisma.BaseValue = hostileCharisma;
                hostile.Descriptor.Stats.Dexterity.Damage =
                    hostileDexterityDamage;
                hostile.Descriptor.Stats.Charisma.Damage = hostileCharismaDamage;
                hostile.Descriptor.Stats.Constitution.Damage =
                    hostileConstitutionDamage;
                hostile.Descriptor.Damage = hostileDamage;
                caster.Descriptor.Damage = casterDamage;
                if (secondVictim != null)
                {
                    RemoveSprint12Buff(secondVictim, filthFever);
                    RemoveSprint12Buff(secondVictim, reaction);
                    if (!secondVictim.Destroyed) secondVictim.Destroy();
                }
                if (goblinVictim != null)
                {
                    RemoveSprint12Buff(goblinVictim, reaction);
                    if (!goblinVictim.Destroyed) goblinVictim.Destroy();
                }
                Game.Instance.EntityDestroyer.Tick();
                if (secondVictimBlueprint != null)
                    UnityEngine.Object.Destroy(secondVictimBlueprint);
                if (goblinVictimBlueprint != null)
                    UnityEngine.Object.Destroy(goblinVictimBlueprint);
            }
        }

        /// <summary>
        /// The printed Goblin Dog allergic reaction exposes "a non-goblinoid
        /// creature damaged by a goblin dog's bite, who deals damage to a
        /// goblin dog with a natural weapon or unarmed attack, or who
        /// otherwise comes into contact with a goblin dog (including attempts
        /// to grapple or ride the creature)". The bite is proved elsewhere;
        /// this exercises the other two through the real rules, with a
        /// manufactured weapon, a non-grapple maneuver and a goblinoid as
        /// negative controls. The grapple attempt is forced to fail so the
        /// evidence shows the printed attempt exposing its initiator without
        /// leaving a hold behind.
        /// </summary>
        private static void ExerciseSprint12ContactAllergy(
            BlueprintScriptableObject[] blueprints, UnitEntityData goblinDog,
            UnitEntityData naturalAttacker, UnitEntityData caster,
            UnitEntityData hostile, UnitEntityData goblinVictim,
            BlueprintBuff reaction,
            ExpandedSummoningMechanicalEvidence evidence)
        {
            int naturalFortitude =
                naturalAttacker.Descriptor.Stats.SaveFortitude.BaseValue;
            int casterFortitude =
                caster.Descriptor.Stats.SaveFortitude.BaseValue;
            int hostileFortitude =
                hostile.Descriptor.Stats.SaveFortitude.BaseValue;
            int goblinFortitude = goblinVictim == null ? 0 :
                goblinVictim.Descriptor.Stats.SaveFortitude.BaseValue;
            int dogHitPoints = goblinDog.Descriptor.Stats.HitPoints.BaseValue;
            try
            {
                goblinDog.Descriptor.Stats.HitPoints.BaseValue = 100000;
                naturalAttacker.Descriptor.Stats.SaveFortitude.BaseValue = -100;
                caster.Descriptor.Stats.SaveFortitude.BaseValue = -100;
                hostile.Descriptor.Stats.SaveFortitude.BaseValue = -100;
                if (goblinVictim != null)
                    goblinVictim.Descriptor.Stats.SaveFortitude.BaseValue = -100;
                RemoveSprint12Buff(naturalAttacker, reaction);
                RemoveSprint12Buff(caster, reaction);
                RemoveSprint12Buff(hostile, reaction);

                // (b) a natural weapon that deals damage to the Goblin Dog.
                ItemEntityWeapon naturalWeapon =
                    naturalAttacker.Body.PrimaryHand.MaybeWeapon;
                bool weaponIsNatural = naturalWeapon != null &&
                    (naturalWeapon.Blueprint.IsNatural ||
                        naturalWeapon.Blueprint.IsUnarmed);
                string naturalDetail = "attacker-weapon-not-natural";
                bool naturalHit = weaponIsNatural &&
                    ExerciseExpandedSummoningAttack(naturalAttacker, goblinDog,
                        out naturalDetail);
                Buff naturalReaction =
                    naturalAttacker.Descriptor.Buffs.GetBuff(reaction);
                int naturalDc = naturalReaction == null ||
                    naturalReaction.Context == null ? -1 :
                    naturalReaction.Context.Params.DC;
                bool naturalSource = naturalReaction != null &&
                    naturalReaction.Context != null &&
                    ReferenceEquals(naturalReaction.Context.MaybeCaster,
                        goblinDog);
                bool naturalExposed = naturalHit && naturalReaction != null &&
                    naturalDc ==
                        SummonInjuryDiseasePolicy.GoblinDogFortitudeDc &&
                    naturalSource;

                // (c) an attempt to grapple the Goblin Dog. AutoFailure keeps
                // the printed "attempt" exact and leaves no hold behind. This
                // runs immediately after the natural attack: an earlier
                // ordering put the manufactured-weapon control in between and
                // the grapple then stopped exposing its initiator, so the
                // controls are sequenced after the behaviour they control for.
                RemoveSprint12Buff(hostile, reaction);
                int hostileFortitudeAtGrapple =
                    hostile.Descriptor.Stats.SaveFortitude.ModifiedValue;
                bool dogAvailableAtGrapple =
                    SummonDiseaseExposure.IsAvailable(goblinDog);
                var grapple = new RuleCombatManeuver(hostile, goblinDog,
                    CombatManeuver.Grapple) { AutoFailure = true };
                Rulebook.Trigger(grapple);
                Buff grappleReaction =
                    hostile.Descriptor.Buffs.GetBuff(reaction);
                bool grappleExposed = !grapple.Success &&
                    grappleReaction != null &&
                    grappleReaction.Context != null &&
                    grappleReaction.Context.Params.DC ==
                        SummonInjuryDiseasePolicy.GoblinDogFortitudeDc &&
                    ReferenceEquals(grappleReaction.Context.MaybeCaster,
                        goblinDog);

                // Negative control: a manufactured weapon never exposes its
                // wielder, so the trigger is the natural contact and not any
                // blow that lands. Neither the caster nor the hostile is
                // holding one in this fixture, so the control builds a real
                // weapon entity from an exact loaded melee blueprint, chosen
                // by ascending asset id so the choice is reproducible, and
                // swings it through the ordinary attack rule.
                BlueprintItemWeapon manufactured = blueprints
                    .OfType<BlueprintItemWeapon>()
                    .Where(value => value != null && value.IsMelee &&
                        !value.IsNatural && !value.IsUnarmed)
                    .OrderBy(value => value.AssetGuid, StringComparer.Ordinal)
                    .FirstOrDefault();
                string manufacturedDetail = "no-manufactured-weapon-blueprint";
                bool manufacturedSafe = false;
                if (manufactured != null)
                {
                    int hostileBaseAttack =
                        hostile.Descriptor.Stats.BaseAttackBonus.BaseValue;
                    try
                    {
                        RemoveSprint12Buff(hostile, reaction);
                        hostile.Descriptor.Stats.BaseAttackBonus.BaseValue = 100;
                        var held = new ItemEntityWeapon(manufactured);
                        UnityEngine.Random.InitState(FindNativeD20Seed(20));
                        var swing = new RuleAttackWithWeapon(hostile, goblinDog,
                            held, 0);
                        Rulebook.Trigger(swing);
                        bool swingHit = swing.AttackRoll != null &&
                            swing.AttackRoll.IsHit;
                        int swingDamage = swing.MeleeDamage == null ? 0 :
                            Math.Max(0, swing.MeleeDamage.Damage);
                        manufacturedSafe = swingHit && swingDamage > 0 &&
                            !hostile.Descriptor.HasFact(reaction);
                        manufacturedDetail = manufactured.name + ":category=" +
                            manufactured.Category + ";hit=" + swingHit +
                            ";damage=" + swingDamage;
                    }
                    finally
                    {
                        hostile.Descriptor.Stats.BaseAttackBonus.BaseValue =
                            hostileBaseAttack;
                    }
                }

                // Negative control: a maneuver the printed rule does not name
                // is not contact.
                RemoveSprint12Buff(hostile, reaction);
                var trip = new RuleCombatManeuver(hostile, goblinDog,
                    CombatManeuver.Trip) { AutoFailure = true };
                Rulebook.Trigger(trip);
                bool tripSafe = !hostile.Descriptor.HasFact(reaction);

                // Negative control: a goblinoid is exempt from every trigger.
                bool goblinoidSafe = false;
                string goblinoidDetail = "no-goblin-type-fixture";
                if (goblinVictim != null)
                {
                    RemoveSprint12Buff(goblinVictim, reaction);
                    var goblinoidGrapple = new RuleCombatManeuver(goblinVictim,
                        goblinDog, CombatManeuver.Grapple) {
                            AutoFailure = true };
                    Rulebook.Trigger(goblinoidGrapple);
                    goblinoidSafe = !goblinVictim.Descriptor.HasFact(reaction);
                    goblinoidDetail = "grappled=" + !goblinoidGrapple.Success +
                        ";exposed=" + !goblinoidSafe;
                }

                evidence.Sprint12ContactAllergy = naturalExposed &&
                    manufactured != null && manufacturedSafe &&
                    grappleExposed && tripSafe && goblinVictim != null &&
                    goblinoidSafe;
                evidence.Sprint12ContactAllergyDetail = "natural[" +
                    naturalDetail + ";weapon=" + (naturalWeapon == null ?
                        "none" : naturalWeapon.Blueprint.Category.ToString()) +
                    ";dc=" + naturalDc + ";source=" + naturalSource +
                    ";exposed=" + naturalExposed + "];manufactured[" +
                    manufacturedDetail + ";safe=" + manufacturedSafe +
                    "];grappleAttempt[success=" + grapple.Success +
                    ";fort=" + hostileFortitudeAtGrapple + ";dogAvailable=" +
                    dogAvailableAtGrapple + ";buff=" +
                    (grappleReaction == null ? "none" : "applied") +
                    ";exposed=" + grappleExposed + "];trip[safe=" + tripSafe +
                    "];goblinoid[" + goblinoidDetail + ";safe=" +
                    goblinoidSafe + "]";
            }
            finally
            {
                RemoveSprint12Buff(naturalAttacker, reaction);
                RemoveSprint12Buff(caster, reaction);
                RemoveSprint12Buff(hostile, reaction);
                if (goblinVictim != null)
                {
                    RemoveSprint12Buff(goblinVictim, reaction);
                    goblinVictim.Descriptor.Stats.SaveFortitude.BaseValue =
                        goblinFortitude;
                }
                naturalAttacker.Descriptor.Stats.SaveFortitude.BaseValue =
                    naturalFortitude;
                caster.Descriptor.Stats.SaveFortitude.BaseValue =
                    casterFortitude;
                hostile.Descriptor.Stats.SaveFortitude.BaseValue =
                    hostileFortitude;
                goblinDog.Descriptor.Stats.HitPoints.BaseValue = dogHitPoints;
            }
        }

        /// <summary>
        /// The printed Dire Rat is AC 14, touch 14, flat-footed 11 - (+3 Dex,
        /// +1 size) with no natural-armor component - and its feat list is
        /// Skill Focus (Perception) alone, so its printed bite +1 comes from
        /// Strength and size rather than from Weapon Finesse. The earlier
        /// hidden profile gave it +1 natural armor and Weapon Finesse, which
        /// made it one point harder to hit and three points more accurate than
        /// the stat block. A live Dog is summoned as the positive control: it
        /// does have +1 natural armor, so a passing Dire Rat result cannot be
        /// an artefact of the check failing to see natural armor at all.
        /// </summary>
        private static void ExerciseSprint12PrintedDefences(
            BlueprintScriptableObject[] blueprints, UnitEntityData caster,
            List<UnitEntityData> created,
            ExpandedSummoningMechanicalEvidence evidence)
        {
            const string NaturalArmorPlusOneGuid =
                "10c7c5e3c5806bc4ca676e22d6fbf17e";
            const string WeaponFinesseGuid =
                "90e54424d682d104ab36436bd527af09";
            BlueprintUnitFact naturalArmor = blueprints
                .OfType<BlueprintUnitFact>().SingleOrDefault(value =>
                    value.AssetGuid == NaturalArmorPlusOneGuid);
            BlueprintUnitFact weaponFinesse = blueprints
                .OfType<BlueprintUnitFact>().SingleOrDefault(value =>
                    value.AssetGuid == WeaponFinesseGuid);
            if (naturalArmor == null || weaponFinesse == null)
                throw new InvalidOperationException(
                    "The exact native +1 natural armor or Weapon Finesse fact was not loaded.");

            // Both sides must be pristine: the shared attack helper sets an
            // attacker's base attack bonus to 100 and leaves it there, so a
            // creature that has already swung in an earlier gate cannot be
            // measured against one that has not.
            UnitEntityData direRat = CastExpandedSummoningCombatUnit(blueprints,
                caster, SummonFamily.NaturesAlly, "dire-rat", 1, created,
                evidence);
            RemoveExpandedSummoningAppearanceBuffs(direRat);
            UnitEntityData dog = CastExpandedSummoningCombatUnit(blueprints,
                caster, SummonFamily.NaturesAlly, "dog", 1, created, evidence);
            RemoveExpandedSummoningAppearanceBuffs(dog);

            bool ratHasNaturalArmor = direRat.Descriptor.HasFact(naturalArmor);
            bool ratHasWeaponFinesse = direRat.Descriptor.HasFact(weaponFinesse);
            bool dogHasNaturalArmor = dog.Descriptor.HasFact(naturalArmor);
            bool dogHasWeaponFinesse = dog.Descriptor.HasFact(weaponFinesse);

            // Both creatures are Small 1-HD animals with base attack 0 and the
            // same bite weapon, so the gap between their live bite bonuses is
            // decided by whichever ability score the natural attack uses. The
            // printed Dire Rat is Strength 10 and the printed Dog is Strength
            // 13, while their Dexterity scores run the other way (17 against
            // 13). A Strength-based pair therefore differs by the Strength
            // modifiers and a finessed pair by the Dexterity modifiers, and the
            // two predictions cannot be confused. Measuring the gap needs no
            // fact mutation and survives whatever the summon templates add,
            // because both units carry the same template.
            int ratStrength = direRat.Descriptor.Stats.Strength.ModifiedValue;
            int ratDexterity = direRat.Descriptor.Stats.Dexterity.ModifiedValue;
            int dogStrength = dog.Descriptor.Stats.Strength.ModifiedValue;
            int dogDexterity = dog.Descriptor.Stats.Dexterity.ModifiedValue;
            UnityEngine.Random.InitState(FindNativeD20Seed(20));
            var ratProbe = new RuleAttackWithWeapon(direRat, dog,
                direRat.Body.PrimaryHand.MaybeWeapon, 0);
            Rulebook.Trigger(ratProbe);
            UnityEngine.Random.InitState(FindNativeD20Seed(20));
            var dogProbe = new RuleAttackWithWeapon(dog, direRat,
                dog.Body.PrimaryHand.MaybeWeapon, 0);
            Rulebook.Trigger(dogProbe);
            int ratBonus = ratProbe.AttackRoll == null ? int.MinValue :
                ratProbe.AttackRoll.AttackBonus;
            int dogBonus = dogProbe.AttackRoll == null ? int.MinValue :
                dogProbe.AttackRoll.AttackBonus;
            int strengthPrediction = Modifier(ratStrength) -
                Modifier(dogStrength);
            int finessePrediction = Modifier(ratDexterity) -
                Modifier(dogDexterity);
            bool measured = ratBonus != int.MinValue &&
                dogBonus != int.MinValue &&
                strengthPrediction != finessePrediction &&
                direRat.Descriptor.Stats.BaseAttackBonus.BaseValue ==
                    dog.Descriptor.Stats.BaseAttackBonus.BaseValue;
            int observedGap = measured ? ratBonus - dogBonus : int.MinValue;
            bool bonusIsStrengthBased = measured &&
                observedGap == strengthPrediction;

            evidence.Sprint12PrintedDefences = !ratHasNaturalArmor &&
                !ratHasWeaponFinesse && !dogHasWeaponFinesse &&
                dogHasNaturalArmor && bonusIsStrengthBased;
            evidence.Sprint12PrintedDefencesDetail = "direRat[naturalArmor=" +
                ratHasNaturalArmor + ";weaponFinesse=" + ratHasWeaponFinesse +
                ";str=" + ratStrength + ";dex=" + ratDexterity +
                ";attackBonus=" + Describe(ratBonus) +
                "];dogControl[naturalArmor=" + dogHasNaturalArmor +
                ";weaponFinesse=" + dogHasWeaponFinesse + ";str=" +
                dogStrength + ";dex=" + dogDexterity + ";attackBonus=" +
                Describe(dogBonus) + "];gap[observed=" +
                Describe(observedGap) + ";strengthPrediction=" +
                strengthPrediction + ";finessePrediction=" +
                finessePrediction + ";strengthBased=" +
                bonusIsStrengthBased + "]";
        }

        private static int Modifier(int score)
        {
            return (int)Math.Floor((score - 10) / 2.0);
        }

        private static string Describe(int value)
        {
            return value == int.MinValue ? "none" : value.ToString(
                System.Globalization.CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// The disease and allergy lifetime contract in
        /// `planning/EXPANDED-SUMMONING-SPRINT12-DISEASE-LIFETIME-CONTRACT.md`
        /// separates the summon's own lifetime from the lifetime of a rules
        /// effect already inflicted on a victim. The charter forbids the mod's
        /// own state persisting after cleanup; it does not require the printed
        /// disease to be cancelled when the rat dies, and cancelling it would
        /// delete the only mechanic the creature exists for. This proves the
        /// victim-side effect survives its destroyed source, still ticks and
        /// still cures, and that the destroyed source exposes nothing further.
        /// </summary>
        private static void ExerciseSprint12DiseaseOutlivesSource(
            UnitEntityData[] rats, UnitEntityData[] dogs,
            UnitEntityData victim, BlueprintBuff filthFever,
            BlueprintBuff reaction, BlueprintAbility cure,
            UnitEntityData caster,
            ExpandedSummoningMechanicalEvidence evidence)
        {
            TimeSpan clock = Game.Instance.Player.GameTime;
            int victimFortitude =
                victim.Descriptor.Stats.SaveFortitude.BaseValue;
            int victimDamage = victim.Descriptor.Damage;
            try
            {
                victim.Descriptor.Stats.SaveFortitude.BaseValue = -100;
                RemoveSprint12Buff(victim, filthFever);
                RemoveSprint12Buff(victim, reaction);
                UnitEntityData rat = rats.Length > 0 ? rats[0] : null;
                UnitEntityData dog = dogs.Length > 0 ? dogs[0] : null;
                if (rat == null || dog == null)
                    throw new InvalidOperationException(
                        "The Sprint 12 lifetime fixture needs a live quantity rat and dog.");
                bool ratHit = ExerciseExpandedSummoningAttack(rat, victim,
                    out string ratDetail);
                bool dogHit = ExerciseExpandedSummoningAttack(dog, victim,
                    out string dogDetail);
                Buff disease = victim.Descriptor.Buffs.GetBuff(filthFever);
                Buff rash = victim.Descriptor.Buffs.GetBuff(reaction);
                bool inflicted = ratHit && dogHit && disease != null &&
                    rash != null;
                TimeSpan rashBefore = rash == null ? TimeSpan.Zero :
                    rash.TimeLeft;
                int dexterityWhileInfected =
                    victim.Descriptor.Stats.Dexterity.ModifiedValue;

                // Destroy the sources through the native scene path.
                rat.Destroy();
                dog.Destroy();
                Game.Instance.EntityDestroyer.Tick();
                bool sourcesGone = rat.Destroyed && dog.Destroyed;

                // The victim keeps both effects, and nothing throws when a
                // context whose caster is destroyed is read or ticked.
                bool survived = false;
                string tickDetail = "not-run";
                TimeSpan rashAfter = TimeSpan.Zero;
                try
                {
                    Game.Instance.Player.GameTime = clock +
                        TimeSpan.FromSeconds(60d);
                    victim.Descriptor.Buffs.Tick();
                    rashAfter = rash == null ? TimeSpan.Zero : rash.TimeLeft;
                    survived = victim.Descriptor.HasFact(filthFever) &&
                        victim.Descriptor.HasFact(reaction) &&
                        rashAfter < rashBefore &&
                        rashAfter > TimeSpan.Zero &&
                        victim.Descriptor.Stats.Dexterity.ModifiedValue ==
                            dexterityWhileInfected;
                    tickDetail = "before=" + rashBefore.TotalSeconds
                            .ToString("0.##", System.Globalization
                                .CultureInfo.InvariantCulture) +
                        ";after=" + rashAfter.TotalSeconds.ToString("0.##",
                            System.Globalization.CultureInfo.InvariantCulture);
                }
                catch (Exception exception)
                {
                    tickDetail = "threw:" + exception.GetType().Name;
                }

                // Reading a context whose caster has been destroyed must not
                // throw, and it must not silently retarget onto something
                // else. A dangling source is the specific risk of a
                // victim-side effect that outlives its summon.
                bool danglingSourceSafe;
                string danglingDetail;
                try
                {
                    UnitEntityData diseaseCaster = disease == null ||
                        disease.Context == null ? null :
                        disease.Context.MaybeCaster;
                    UnitEntityData rashCaster = rash == null ||
                        rash.Context == null ? null : rash.Context.MaybeCaster;
                    bool diseaseOwnerGone = diseaseCaster == null ||
                        diseaseCaster.Destroyed;
                    bool rashOwnerGone = rashCaster == null ||
                        rashCaster.Destroyed;
                    bool noRetarget =
                        !ReferenceEquals(diseaseCaster, victim) &&
                        !ReferenceEquals(rashCaster, victim) &&
                        !ReferenceEquals(diseaseCaster, caster) &&
                        !ReferenceEquals(rashCaster, caster);
                    danglingSourceSafe = diseaseOwnerGone && rashOwnerGone &&
                        noRetarget;
                    danglingDetail = "diseaseCaster=" + (diseaseCaster == null ?
                            "null" : diseaseCaster.Destroyed ? "destroyed" :
                            "live") + ";rashCaster=" + (rashCaster == null ?
                            "null" : rashCaster.Destroyed ? "destroyed" :
                            "live") + ";retargeted=" + !noRetarget;
                }
                catch (Exception exception)
                {
                    danglingSourceSafe = false;
                    danglingDetail = "threw:" + exception.GetType().Name;
                }

                // The printed rash removal - "remove disease or any magical
                // healing removes the rash instantly" - must still work with
                // the Goblin Dog gone. Positive healing carrying an actual
                // spell context is a rule rather than a command, so it reaches
                // a disposable victim across the room; the native Remove
                // Disease command path is proved separately in this same
                // scenario, on an adjacent target, because it is touch-range.
                victim.Descriptor.Damage = Math.Max(20,
                    victim.Descriptor.Damage);
                var cureContext = new MechanicsContext(caster,
                    victim.Descriptor, cure, null, new TargetWrapper(victim));
                var magicalHealing = new RuleHealDamage(caster, victim,
                    new DiceFormula(0, DiceType.D6), 5) {
                    Reason = new RuleReason(cureContext)
                };
                Rulebook.Trigger(magicalHealing);
                bool rashCured = magicalHealing.Value > 0 &&
                    !victim.Descriptor.HasFact(reaction);
                // Filth fever is not a rash: magical healing does not clear
                // it, so the disease must still be present afterwards. That
                // separates the two printed cure contracts instead of
                // accepting any removal as proof.
                bool diseaseUnaffectedByHealing =
                    victim.Descriptor.HasFact(filthFever);

                evidence.Sprint12DiseaseOutlivesSource = inflicted &&
                    sourcesGone && survived && danglingSourceSafe &&
                    rashCured && diseaseUnaffectedByHealing;
                evidence.Sprint12DiseaseOutlivesSourceDetail = "inflicted[" +
                    ratDetail + ";" + dogDetail + ";disease=" +
                    (disease != null) + ";rash=" + (rash != null) +
                    "];sourcesDestroyed=" + sourcesGone + ";afterDestroy[" +
                    tickDetail + ";survived=" + survived + "];danglingSource[" +
                    danglingDetail + ";safe=" + danglingSourceSafe +
                    "];magicalHealing[rashCured=" + rashCured +
                    ";diseaseKept=" + diseaseUnaffectedByHealing + "]";
            }
            finally
            {
                Game.Instance.Player.GameTime = clock;
                RemoveSprint12Buff(victim, filthFever);
                RemoveSprint12Buff(victim, reaction);
                victim.Descriptor.Stats.SaveFortitude.BaseValue =
                    victimFortitude;
                victim.Descriptor.Damage = victimDamage;
            }
        }

        private static SummonInjuryDiseaseComponent Sprint12DiseaseDelivery(
            UnitEntityData unit, string featureName)
        {
            Feature feature = unit.Descriptor.Progression.Features.Enumerable
                .OfType<Feature>().Single(value =>
                    value.Blueprint != null &&
                    value.Blueprint.name == featureName);
            SummonInjuryDiseaseComponent result = feature.Components
                .OfType<SummonInjuryDiseaseComponent>().SingleOrDefault();
            if (result == null)
                throw new InvalidOperationException(
                    "Missing Sprint 12 disease delivery on " + featureName + ".");
            return result;
        }

        private static bool DescribeSprint12OriginalVisual(UnitEntityData unit,
            string key, out string detail)
        {
            string outcome = unit == null || unit.View == null ? "no-view" :
                ExpandedSummoningPteranodonViewPatch.DescribeView(unit.View);
            SkinnedMeshRenderer[] renderers = unit == null || unit.View == null ?
                new SkinnedMeshRenderer[0] : unit.View
                    .GetComponentsInChildren<SkinnedMeshRenderer>(true)
                    .Where(value => value != null && value.sharedMesh != null)
                    .ToArray();
            string expected = "KMG_" + key + "_Original";
            string observed = renderers.Length == 1 ?
                renderers[0].sharedMesh.name : "count=" + renderers.Length;
            bool valid = outcome.StartsWith("visual:attached;",
                    StringComparison.Ordinal) && renderers.Length == 1 &&
                string.Equals(observed, expected, StringComparison.Ordinal);
            detail = key + "=" + observed + "/" + outcome;
            return valid;
        }

        /// <summary>Records only renderer-local bind frames needed to author
        /// project-owned silhouettes. Native vertices, triangles, materials,
        /// textures and animation data never enter the evidence or repository.</summary>
        private static string CaptureSprint12DonorRig(UnitEntityData summon,
            string donorKey, string evidenceDirectory)
        {
            return CaptureDonorRig(summon, donorKey, evidenceDirectory,
                "Sprint 12");
        }

        /// <summary>
        /// Records only renderer-local bind frames needed to author
        /// project-owned silhouettes, labelled with the sprint that captured
        /// them. Native vertices, triangles, materials, textures and animation
        /// data never enter the evidence or the repository.
        /// </summary>
        private static string CaptureDonorRig(UnitEntityData summon,
            string donorKey, string evidenceDirectory, string sprintLabel)
        {
            if (summon == null || summon.View == null ||
                string.IsNullOrWhiteSpace(evidenceDirectory))
                throw new InvalidOperationException("The " + sprintLabel + " " + donorKey +
                    " bind-rig capture has no live view or evidence directory.");
            SkinnedMeshRenderer[] renderers = summon.View
                .GetComponentsInChildren<SkinnedMeshRenderer>(true)
                .Where(value => value != null && value.sharedMesh != null)
                .ToArray();
            if (renderers.Length != 1)
                throw new InvalidOperationException("The " + sprintLabel + " " + donorKey +
                    " donor view has " + renderers.Length +
                    " skinned renderers; exactly one was required.");
            Transform[] originalBones;
            Matrix4x4[] originalPoses;
            Mesh originalMesh;
            bool originalRetained = ExpandedSummoningPteranodonViewPatch
                .TryGetDonorRig(summon.View, out originalBones,
                    out originalPoses, out originalMesh);
            var document = new JObject {
                ["source"] = "request-local hidden " + sprintLabel + " " + donorKey +
                    " donor view",
                ["space"] = "renderer-local bind frame",
                ["blueprint"] = summon.Blueprint.name
            };
            var entries = new JArray();
            foreach (SkinnedMeshRenderer renderer in renderers)
            {
                Transform[] bones = originalRetained ? originalBones :
                    (renderer.bones ?? new Transform[0]);
                Matrix4x4[] poses = originalRetained ? originalPoses :
                    (renderer.sharedMesh.bindposes ?? new Matrix4x4[0]);
                Mesh sourceMesh = originalRetained ? originalMesh :
                    renderer.sharedMesh;
                if (sourceMesh == null || bones.Length == 0 ||
                    bones.Length != poses.Length ||
                    bones.Any(value => value == null))
                    throw new InvalidOperationException("The " + sprintLabel + " " +
                        donorKey + " donor bind frame is incomplete.");
                var entry = new JObject {
                    ["renderer"] = renderer.name,
                    ["mesh"] = sourceMesh.name,
                    ["vertexCount"] = sourceMesh.vertexCount,
                    ["retainedBeforeOriginalSwap"] = originalRetained,
                    ["rootBone"] = renderer.rootBone == null ? "" :
                        renderer.rootBone.name,
                    ["boneCount"] = bones.Length,
                    ["bindPoseCount"] = poses.Length
                };
                var capturedBones = new JArray();
                for (int index = 0; index < bones.Length; index++)
                {
                    Transform bone = bones[index];
                    Matrix4x4 bind = poses[index].inverse;
                    Vector3 position = bind.MultiplyPoint3x4(Vector3.zero);
                    Quaternion rotation = Quaternion.LookRotation(
                        bind.GetColumn(2), bind.GetColumn(1));
                    capturedBones.Add(new JObject {
                        ["index"] = index,
                        ["name"] = bone.name,
                        ["parent"] = bone.parent == null ? "" : bone.parent.name,
                        ["bindPosition"] = new JArray(position.x, position.y,
                            position.z),
                        ["bindRotation"] = new JArray(rotation.x, rotation.y,
                            rotation.z, rotation.w)
                    });
                }
                entry["bones"] = capturedBones;
                entries.Add(entry);
            }
            document["renderers"] = entries;
            string fileName = sprintLabel.Replace(" ", "")
                .ToLowerInvariant() + "-" + donorKey + "-bind-rig.json";
            File.WriteAllText(Path.Combine(evidenceDirectory, fileName),
                document.ToString(Formatting.Indented));
            return donorKey + ":file=" + fileName + ",renderers=" +
                entries.Count + ",bones=" + string.Join("/",
                    entries.OfType<JObject>().Select(value =>
                        ((int)value["boneCount"]).ToString()).ToArray());
        }

        private static RuleApplyBuff ApplySprint12Buff(UnitEntityData target,
            BlueprintBuff blueprint, UnitEntityData source, TimeSpan? duration,
            int difficultyClass)
        {
            var context = new MechanicsContext(source, target.Descriptor,
                blueprint, null, new TargetWrapper(target));
            context.Params.DC = difficultyClass;
            var rule = new RuleApplyBuff(target, blueprint, context, duration,
                (buff, owner, time) =>
                    target.Descriptor.Buffs.AddBuff(buff, owner, time));
            Rulebook.Trigger(rule);
            return rule;
        }

        private static void RemoveSprint12Buff(UnitEntityData unit,
            BlueprintBuff blueprint)
        {
            if (unit == null || unit.Descriptor == null || blueprint == null)
                return;
            foreach (Buff buff in unit.Descriptor.Buffs.RawFacts.OfType<Buff>()
                .Where(value => ReferenceEquals(value.Blueprint, blueprint))
                .ToArray())
                unit.Descriptor.Buffs.RemoveFact(buff);
        }
    }
}
