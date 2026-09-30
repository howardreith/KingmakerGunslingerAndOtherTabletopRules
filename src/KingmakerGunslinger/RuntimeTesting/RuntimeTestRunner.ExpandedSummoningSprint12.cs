using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
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

        /// <summary>Records only renderer-local bind frames needed to author
        /// project-owned silhouettes. Native vertices, triangles, materials,
        /// textures and animation data never enter the evidence or repository.</summary>
        private static string CaptureSprint12DonorRig(UnitEntityData summon,
            string donorKey, string evidenceDirectory)
        {
            if (summon == null || summon.View == null ||
                string.IsNullOrWhiteSpace(evidenceDirectory))
                throw new InvalidOperationException("The Sprint 12 " + donorKey +
                    " bind-rig capture has no live view or evidence directory.");
            SkinnedMeshRenderer[] renderers = summon.View
                .GetComponentsInChildren<SkinnedMeshRenderer>(true)
                .Where(value => value != null && value.sharedMesh != null)
                .ToArray();
            if (renderers.Length == 0)
                throw new InvalidOperationException("The Sprint 12 " + donorKey +
                    " donor view has no skinned renderer.");
            var document = new JObject {
                ["source"] = "request-local hidden Sprint 12 " + donorKey +
                    " donor view",
                ["space"] = "renderer-local bind frame",
                ["blueprint"] = summon.Blueprint.name
            };
            var entries = new JArray();
            foreach (SkinnedMeshRenderer renderer in renderers)
            {
                Transform[] bones = renderer.bones ?? new Transform[0];
                Matrix4x4[] poses = renderer.sharedMesh.bindposes ??
                    new Matrix4x4[0];
                var entry = new JObject {
                    ["renderer"] = renderer.name,
                    ["mesh"] = renderer.sharedMesh.name,
                    ["vertexCount"] = renderer.sharedMesh.vertexCount,
                    ["rootBone"] = renderer.rootBone == null ? "" :
                        renderer.rootBone.name,
                    ["boneCount"] = bones.Length,
                    ["bindPoseCount"] = poses.Length
                };
                var capturedBones = new JArray();
                for (int index = 0; index < bones.Length; index++)
                {
                    Transform bone = bones[index];
                    if (bone == null || index >= poses.Length) continue;
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
            string fileName = "sprint12-" + donorKey + "-bind-rig.json";
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
