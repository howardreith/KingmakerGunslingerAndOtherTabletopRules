using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using Harmony12;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Blueprints.Root;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.Enums.Damage;
using Kingmaker.Items;
using Kingmaker.ResourceLinks;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UI.LevelUp;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.ElementalRaces;
using KingmakerGunslinger.Enchantments;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    /// <summary>
    /// Save-free live-rule qualification for Airy Step, Cloud Gazer, and
    /// Inner Breath. Every native inclusion and exclusion is an exact identity
    /// from the guarded KMG-only blueprint audit.
    /// </summary>
    internal static class ElementalSylphFeatScenario
    {
        internal const string EvidenceFileName =
            "elemental-sylph-feats.json";
        private const string ShortSwordGuid =
            "57c8994d1f1becf49ac4f642e5d8ca9d";
        private const string LightningBoltGuid =
            "d2cff9243a7ee804cb6d5be47af30c73";
        private const string SiroccoGuid =
            "093ed1d67a539ad4c939d9d05cfe192c";
        private const string BlurGuid =
            "dd3ad347240624d46a11a092b4dd4674";
        private const string DisplacementGuid =
            "00402bae4442a854081264e498e7a833";
        private const string ObscuringMistGuid =
            "61b312b8f91cc48418768b77cd6dcc02";
        private const string InvisibilityGuid =
            "525f980cb29bc2240b93e953974cb325";
        private const string MirrorImageGuid =
            "a6e5fd0f45730f34fa23f309e06078fa";
        private const string BlindnessGuid =
            "187f88d96a0ef464280706b63635f2af";
        private const string DarknessGuid =
            "64737e33d1d185b4194798e9abee76ca";
        private const string OrdinaryPoisonGuid =
            "ba1ae42c58e228c4da28328ea6b4ae34";
        private const string StinkingCloudGuid =
            "f85351ee696d98246ae5dc182b410447";
        private const string CloudkillGuid =
            "ef126ea92b72946439a4d0faa2369579";
        private const string SwampGasDotGuid =
            "95f87c9e430e5314fab51d882babdecf";

        private sealed class SaveCaseEvidence
        {
            public string Name { get; set; }
            public string AbilityGuid { get; set; }
            public int WithoutFeat { get; set; }
            public int WithFeat { get; set; }
            public int Delta { get { return WithFeat - WithoutFeat; } }
        }

        private sealed class AiryStepEvidence
        {
            public SaveCaseEvidence Nonmatching { get; set; }
            public SaveCaseEvidence Electricity { get; set; }
            public SaveCaseEvidence Air { get; set; }
            public SaveCaseEvidence ParentAir { get; set; }
            public SaveCaseEvidence Overlap { get; set; }
            public int DirectElectricityWithoutFeat { get; set; }
            public int DirectElectricityWithFeat { get; set; }
            public int DirectElectricityDelta
            {
                get
                {
                    return DirectElectricityWithFeat -
                        DirectElectricityWithoutFeat;
                }
            }
            public int ElectricityWithWings { get; set; }
            public int WingsDelta { get; set; }
            public List<SaveCaseEvidence> ExactAirCatalog { get; set; }
            public bool ModifierLeakAbsent { get; set; }

            public string Summary()
            {
                return "nonmatching=" + Nonmatching.Delta +
                    ";electricity=" + Electricity.Delta + ";air=" +
                    Air.Delta + ";parentAir=" + ParentAir.Delta +
                    ";overlap=" + Overlap.Delta + ";direct=" +
                    DirectElectricityDelta + ";wings=" + WingsDelta +
                    ";catalog=" + string.Join(",", ExactAirCatalog.Select(
                        value => value.AbilityGuid + ":" + value.Delta)) +
                    ";leakAbsent=" + ModifierLeakAbsent;
            }
        }

        private sealed class AttackEvidence
        {
            public string Name { get; set; }
            public bool IsHit { get; set; }
            public bool HasCheck { get; set; }
            public int ForcedRoll { get; set; }
            public bool CheckSuccess { get; set; }
            public string Result { get; set; }
            public int AcProbeConcealmentRoll { get; set; }
            public int AcProbeAttackRoll { get; set; }
            public int AcProbeAttackBonus { get; set; }
            public int AcProbeTargetAc { get; set; }
            public bool ExtraBuffRetained { get; set; }

            public bool WouldHitAc()
            {
                return AcProbeConcealmentRoll == 100 &&
                    AcProbeAttackRoll == 19 && AcProbeAttackRoll +
                    AcProbeAttackBonus >= AcProbeTargetAc;
            }

            public bool Bypassed()
            {
                return IsHit && HasCheck && ForcedRoll == 1 && WouldHitAc();
            }

            public bool Preserved()
            {
                return !IsHit && HasCheck && ForcedRoll == 1 &&
                    !CheckSuccess && WouldHitAc();
            }

            public bool ReachedIndependentDefense(string expected)
            {
                return !IsHit && HasCheck && ForcedRoll == 1 &&
                    string.Equals(Result, expected, StringComparison.Ordinal) &&
                    WouldHitAc();
            }

            public string Summary()
            {
                return Name + "=hit:" + IsHit + ",check:" + HasCheck +
                    ",roll:" + ForcedRoll + ",success:" + CheckSuccess +
                    ",result:" + Result + ",acProbe:" +
                    AcProbeConcealmentRoll + "/" + AcProbeAttackRoll + "+" +
                    AcProbeAttackBonus + "vs" + AcProbeTargetAc +
                    ",extraRetained:" + ExtraBuffRetained;
            }
        }

        private sealed class CloudGazerEvidence
        {
            public AttackEvidence WithoutFeatFog { get; set; }
            public AttackEvidence NativeFog { get; set; }
            public AttackEvidence ProjectFog { get; set; }
            public AttackEvidence Smoke { get; set; }
            public AttackEvidence Blur { get; set; }
            public AttackEvidence Displacement { get; set; }
            public AttackEvidence FogAndBlur { get; set; }
            public AttackEvidence FogAndInvisibility { get; set; }
            public AttackEvidence FogAndBlindness { get; set; }
            public AttackEvidence FogAndDarkness { get; set; }
            public AttackEvidence FogAndMirrorImage { get; set; }

            public bool AcIsolated()
            {
                return All().All(value => value != null &&
                    value.WouldHitAc());
            }

            public IEnumerable<AttackEvidence> All()
            {
                return new[] { WithoutFeatFog, NativeFog, ProjectFog, Smoke,
                    Blur, Displacement, FogAndBlur, FogAndInvisibility,
                    FogAndBlindness, FogAndDarkness, FogAndMirrorImage };
            }

            public string Summary()
            {
                return string.Join(";", All().Select(value =>
                    value.Summary()).ToArray());
            }
        }

        private sealed class InnerBreathEvidence
        {
            public List<string> ExactNativeGuids { get; set; }
            public int ControlExactApplied { get; set; }
            public int ExactBlocked { get; set; }
            public bool ProjectMarkedBlocked { get; set; }
            public bool OrdinaryPoisonApplied { get; set; }
            public bool StinkingCloudApplied { get; set; }
            public bool CloudkillApplied { get; set; }
            public bool SwampGasDotApplied { get; set; }

            public string Summary()
            {
                return "catalog=" + string.Join(",", ExactNativeGuids) +
                    ";controlExact=" + ControlExactApplied +
                    ";blocked=" + ExactBlocked + ";project=" +
                    ProjectMarkedBlocked + ";ordinaryPoison=" +
                    OrdinaryPoisonApplied + ";stinking=" +
                    StinkingCloudApplied + ";cloudkill=" + CloudkillApplied +
                    ";gasDot=" + SwampGasDotApplied;
            }
        }

        private sealed class Evidence
        {
            public int SchemaVersion { get; set; }
            public bool ModuleActive { get; set; }
            public bool SaveStateTouched { get; set; }
            public bool BlueprintContract { get; set; }
            public string BlueprintSummary { get; set; }
            public bool ConcealmentPatchContract { get; set; }
            public string ConcealmentPatchSummary { get; set; }
            public AiryStepEvidence AiryStep { get; set; }
            public CloudGazerEvidence CloudGazer { get; set; }
            public InnerBreathEvidence InnerBreath { get; set; }
            public bool CleanupExact { get; set; }
        }

        internal static RuntimeTestResult Run(ModContext context,
            RuntimeTestRequest request)
        {
            DateTime started = DateTime.UtcNow;
            var assertions = new List<RuntimeTestAssertion>();
            var diagnostics = new List<string>();
            var files = new List<string>();
            var evidence = new Evidence
            {
                SchemaVersion = 1,
                ModuleActive = context.FeatureModules.Active.ElementalRaces,
                SaveStateTouched = false
            };
            var created = new List<UnitEntityData>();
            var transient = new List<UnityEngine.Object>();
            var items = new List<ItemEntity>();
            UnitEntityData[] unitsBefore = Game.Instance.State.Units.All
                .ToArray();
            string stage = "resolve-blueprints";
            string exceptionSummary = string.Empty;
            try
            {
                ElementalRaceBlueprintSet races = BlueprintBootstrap
                    .ElementalRaces;
                ElementalFeatBlueprintSet feats = BlueprintBootstrap
                    .ElementalFeats;
                if (races == null || feats == null)
                    throw new InvalidOperationException(
                        "Elemental race or feat blueprints are unavailable.");
                BlueprintFeature airy = feats.RequireFeature(
                    ElementalFeatId.AiryStep);
                BlueprintFeature wings = feats.RequireFeature(
                    ElementalFeatId.WingsOfAir);
                BlueprintFeature cloud = feats.RequireFeature(
                    ElementalFeatId.CloudGazer);
                BlueprintFeature breath = feats.RequireFeature(
                    ElementalFeatId.InnerBreath);
                ElementalAiryStepSaveBonus airyComponent = airy
                    .ComponentsArray.OfType<ElementalAiryStepSaveBonus>()
                    .Single();
                ElementalInnerBreathImmunity breathComponent = breath
                    .ComponentsArray.OfType<ElementalInnerBreathImmunity>()
                    .Single();
                evidence.BlueprintContract = ReferenceEquals(
                        airyComponent.WingsOfAir, wings) &&
                    breathComponent != null &&
                    ElementalFeatPolicy.ExactNativeAirEffectGuids().Length ==
                        11 && ElementalFeatPolicy
                        .ExactNativeCloudGazerConcealmentGuids().Length == 1 &&
                    ElementalFeatPolicy
                        .ExactNativeRespirationRequiredBuffGuids().Length == 2;
                evidence.BlueprintSummary = "airy=" + airy.AssetGuid +
                    "/component=" + airyComponent.GetType().FullName +
                    "/wings=" + wings.AssetGuid + ";cloud=" +
                    cloud.AssetGuid + ";breath=" + breath.AssetGuid +
                    "/component=" + breathComponent.GetType().FullName +
                    ";catalogs=11/1/2";
                bool patchInstalled;
                evidence.ConcealmentPatchSummary = DescribePatches(context,
                    out patchInstalled);
                evidence.ConcealmentPatchContract = patchInstalled;

                BlueprintAbility lightning = Exact<BlueprintAbility>(
                    LightningBoltGuid, "Airy Step electricity source");
                BlueprintAbility sirocco = Exact<BlueprintAbility>(
                    SiroccoGuid, "Airy Step air source");
                BlueprintAbility nonmatching = FindOrdinaryControl();
                BlueprintAbility[] airCatalog = ElementalFeatPolicy
                    .ExactNativeAirEffectGuids().Select(guid =>
                        Exact<BlueprintAbility>(guid,
                            "Airy Step exact air catalog")).ToArray();

                stage = "airy-step";
                UnitEntityData saver = CreateUnit(races.Sylph.Race, created,
                    transient, "AirySaver");
                UnitEntityData source = CreateUnit(null, created, transient,
                    "AirySource");
                evidence.AiryStep = ExerciseAiryStep(saver, source, airy,
                    wings, nonmatching, lightning, sirocco, airCatalog,
                    transient);

                stage = "cloud-gazer";
                BlueprintItemWeapon sword = Exact<BlueprintItemWeapon>(
                    ShortSwordGuid, "Cloud Gazer attack control");
                UnitEntityData attacker = CreateUnit(races.Sylph.Race,
                    created, transient, "CloudGazerAttacker");
                UnitEntityData target = CreateUnit(null, created, transient,
                    "CloudGazerTarget");
                attacker.Descriptor.Stats.BaseAttackBonus.BaseValue = 20;
                attacker.Descriptor.Stats.Dexterity.BaseValue = 30;
                var weapon = new ItemEntityWeapon(sword);
                items.Add(weapon);
                attacker.Body.PrimaryHand.InsertItem(weapon);
                if (!ReferenceEquals(attacker.Body.PrimaryHand.MaybeWeapon,
                        weapon))
                    throw new InvalidOperationException(
                        "The Cloud Gazer fixture rejected its native weapon.");
                evidence.CloudGazer = ExerciseCloudGazer(attacker, target,
                    weapon, cloud, transient);

                stage = "inner-breath";
                UnitEntityData breather = CreateUnit(races.Sylph.Race,
                    created, transient, "InnerBreath");
                UnitEntityData breathControl = CreateUnit(null, created,
                    transient, "InnerBreathControl");
                evidence.InnerBreath = ExerciseInnerBreath(breather,
                    breathControl, breath, transient);
            }
            catch (Exception exception)
            {
                exceptionSummary = "stage=" + stage + ";" + exception;
                diagnostics.Add(exceptionSummary);
            }
            finally
            {
                SeekingConcealmentRuntime.CancelForcedRoll();
                foreach (UnitEntityData unit in created.AsEnumerable()
                    .Reverse().ToArray())
                {
                    if (unit == null) continue;
                    unit.Commands.InterruptAll(true);
                    if (unit.Body != null && unit.Body.PrimaryHand != null &&
                        unit.Body.PrimaryHand.MaybeItem != null)
                        unit.Body.PrimaryHand.RemoveItem(false);
                }
                foreach (ItemEntity item in items.Where(value => value != null)
                    .Distinct().ToArray()) item.Dispose();
                foreach (UnitEntityData unit in created.AsEnumerable()
                    .Reverse().ToArray())
                {
                    if (unit == null) continue;
                    Game.Instance.State.Units.All.Remove(unit);
                    unit.Descriptor.State.Immortality.ReleaseAll();
                    unit.Dispose();
                }
                foreach (UnityEngine.Object value in transient.AsEnumerable()
                    .Reverse().Where(value => value != null).ToArray())
                    UnityEngine.Object.DestroyImmediate(value);
                evidence.CleanupExact = SameReferences(unitsBefore,
                    Game.Instance.State.Units.All.ToArray());
            }

            AddAssertions(assertions, evidence, request, context);
            string path = Path.Combine(request.EvidenceDirectory,
                EvidenceFileName);
            File.WriteAllText(path, JsonConvert.SerializeObject(evidence,
                Formatting.Indented, new JsonSerializerSettings
                {
                    ContractResolver = new DefaultContractResolver(),
                    PreserveReferencesHandling =
                        PreserveReferencesHandling.None,
                    ReferenceLoopHandling = ReferenceLoopHandling.Error
                }));
            files.Add(path);
            diagnostics.Add("elementalSylphFeatSha256=" + Hash(path));
            bool pass = string.IsNullOrEmpty(exceptionSummary) &&
                assertions.All(value => value.Status ==
                    RuntimeTestStatuses.Pass);
            Assembly assembly = context.Assembly;
            return new RuntimeTestResult
            {
                SchemaVersion = 1,
                RunId = request.RunId,
                Scenario = request.Scenario,
                Status = pass ? RuntimeTestStatuses.Pass :
                    RuntimeTestStatuses.Fail,
                LoadedModVersion = context.ModEntry.Info.Version,
                RuntimeIdentity = assembly.FullName + ";mvid=" +
                    assembly.ManifestModule.ModuleVersionId + ";sha256=" +
                    Hash(assembly.Location) + ";pid=" +
                    Process.GetCurrentProcess().Id,
                GitCommit = Metadata(assembly, "GitCommit"),
                GameVersion = Application.version ?? string.Empty,
                StartUtc = started.ToString("o"),
                EndUtc = string.Empty,
                DurationMilliseconds = (long)(DateTime.UtcNow - started)
                    .TotalMilliseconds,
                Assertions = assertions,
                Diagnostics = diagnostics,
                Warnings = new List<string>(),
                ExceptionSummary = exceptionSummary,
                EvidenceFiles = files,
                AutomaticExitRequested = request.ExitAfterCompletion,
                EvidenceDirectory = request.EvidenceDirectory
            };
        }

        private static AiryStepEvidence ExerciseAiryStep(
            UnitEntityData saver, UnitEntityData source,
            BlueprintFeature airy, BlueprintFeature wings,
            BlueprintAbility nonmatching, BlueprintAbility electricity,
            BlueprintAbility air, BlueprintAbility[] airCatalog,
            ICollection<UnityEngine.Object> transient)
        {
            BlueprintAbility parentAir = CreateVariant(
                "KMG_Runtime_AiryStep_ParentAir", air,
                SpellDescriptor.None, transient);
            BlueprintAbility overlap = CreateVariant(
                "KMG_Runtime_AiryStep_AirElectricityOverlap", air,
                SpellDescriptor.Electricity, transient);
            int reflexBefore = saver.Descriptor.Stats.SaveReflex.ModifiedValue;

            var result = new AiryStepEvidence
            {
                Nonmatching = Baseline("nonmatching", saver, source,
                    nonmatching),
                Electricity = Baseline("electricity", saver, source,
                    electricity),
                Air = Baseline("air", saver, source, air),
                ParentAir = Baseline("parent-air", saver, source,
                    parentAir),
                Overlap = Baseline("air-electricity-overlap", saver, source,
                    overlap),
                DirectElectricityWithoutFeat = SaveWithDirectElectricity(
                    saver, source),
                ExactAirCatalog = airCatalog.Select(value => Baseline(
                    value.name ?? value.AssetGuid, saver, source, value))
                    .ToList()
            };

            EnsureFact(saver.Descriptor, airy);
            Complete(result.Nonmatching, saver, source, nonmatching);
            Complete(result.Electricity, saver, source, electricity);
            Complete(result.Air, saver, source, air);
            Complete(result.ParentAir, saver, source, parentAir);
            Complete(result.Overlap, saver, source, overlap);
            result.DirectElectricityWithFeat = SaveWithDirectElectricity(
                saver, source);
            for (int index = 0; index < airCatalog.Length; index++)
                Complete(result.ExactAirCatalog[index], saver, source,
                    airCatalog[index]);

            EnsureFact(saver.Descriptor, wings);
            result.ElectricityWithWings = SaveWithAbility(saver, source,
                electricity);
            result.WingsDelta = result.ElectricityWithWings -
                result.Electricity.WithoutFeat;
            result.ModifierLeakAbsent = saver.Descriptor.Stats.SaveReflex
                .ModifiedValue == reflexBefore;
            return result;
        }

        private static SaveCaseEvidence Baseline(string name,
            UnitEntityData saver, UnitEntityData source,
            BlueprintAbility ability)
        {
            return new SaveCaseEvidence
            {
                Name = name,
                AbilityGuid = ability == null ? string.Empty :
                    ability.AssetGuid ?? string.Empty,
                WithoutFeat = SaveWithAbility(saver, source, ability)
            };
        }

        private static void Complete(SaveCaseEvidence result,
            UnitEntityData saver, UnitEntityData source,
            BlueprintAbility ability)
        {
            result.WithFeat = SaveWithAbility(saver, source, ability);
        }

        private static BlueprintAbility CreateVariant(string name,
            BlueprintAbility parent, SpellDescriptor descriptor,
            ICollection<UnityEngine.Object> transient)
        {
            var result = ScriptableObject.CreateInstance<BlueprintAbility>();
            result.name = name;
            result.Type = AbilityType.Spell;
            result.Parent = parent;
            var component = ScriptableObject.CreateInstance<
                SpellDescriptorComponent>();
            component.Descriptor = descriptor;
            result.ComponentsArray = new BlueprintComponent[] { component };
            transient.Add(component);
            transient.Add(result);
            return result;
        }

        private static int SaveWithAbility(UnitEntityData saver,
            UnitEntityData source, BlueprintAbility ability)
        {
            var mechanics = new MechanicsContext(source, source.Descriptor,
                ability, null, new TargetWrapper(saver));
            var saving = new RuleSavingThrow(saver, SavingThrowType.Reflex,
                100) { Reason = mechanics };
            mechanics.TriggerRule(saving);
            return saving.StatValue;
        }

        private static int SaveWithDirectElectricity(UnitEntityData saver,
            UnitEntityData source)
        {
            var damage = new RuleDealDamage(source, saver,
                new DamageBundle(new EnergyDamage(
                    new DiceFormula(0, DiceType.D6),
                    DamageEnergyType.Electricity) { PreRolledValue = 1 }));
            var saving = new RuleSavingThrow(saver, SavingThrowType.Reflex,
                100) { Reason = damage };
            Rulebook.Trigger(saving);
            return saving.StatValue;
        }

        private static CloudGazerEvidence ExerciseCloudGazer(
            UnitEntityData attacker, UnitEntityData target,
            ItemEntityWeapon weapon, BlueprintFeature cloud,
            ICollection<UnityEngine.Object> transient)
        {
            BlueprintBuff nativeFog = Exact<BlueprintBuff>(
                ObscuringMistGuid, "Cloud Gazer native fog");
            BlueprintBuff blur = Exact<BlueprintBuff>(BlurGuid,
                "Cloud Gazer Blur exclusion");
            BlueprintBuff displacement = Exact<BlueprintBuff>(
                DisplacementGuid, "Cloud Gazer displacement exclusion");
            BlueprintBuff invisibility = Exact<BlueprintBuff>(
                InvisibilityGuid, "Cloud Gazer invisibility exclusion");
            BlueprintBuff mirrorImage = Exact<BlueprintBuff>(MirrorImageGuid,
                "Cloud Gazer Mirror Image exclusion");
            BlueprintBuff blindness = Exact<BlueprintBuff>(BlindnessGuid,
                "Cloud Gazer blindness exclusion");
            BlueprintBuff darkness = Exact<BlueprintBuff>(DarknessGuid,
                "Cloud Gazer darkness exclusion");
            BlueprintBuff projectFog = CreateConcealment(
                "KMG_Runtime_CloudGazer_Fog",
                ElementalFiresightConcealmentKind.FogMistOrCloud, transient);
            BlueprintBuff smoke = CreateConcealment(
                "KMG_Runtime_CloudGazer_Smoke",
                ElementalFiresightConcealmentKind.Smoke, transient);

            var result = new CloudGazerEvidence();
            result.WithoutFeatFog = Attack("without-feat-fog", attacker,
                target, weapon, new[] { nativeFog }, null);
            EnsureFact(attacker.Descriptor, cloud);
            result.NativeFog = Attack("native-fog", attacker, target, weapon,
                new[] { nativeFog }, null);
            result.ProjectFog = Attack("project-fog", attacker, target,
                weapon, new[] { projectFog }, null);
            result.Smoke = Attack("project-smoke", attacker, target, weapon,
                new[] { smoke }, null);
            result.Blur = Attack("native-blur", attacker, target, weapon,
                new[] { blur }, null);
            result.Displacement = Attack("native-displacement", attacker,
                target, weapon, new[] { displacement }, null);
            result.FogAndBlur = Attack("fog-plus-blur", attacker, target,
                weapon, new[] { nativeFog, blur }, blur);
            result.FogAndInvisibility = Attack("fog-plus-invisibility",
                attacker, target, weapon, new[] { nativeFog, invisibility },
                invisibility);

            Buff blindFact = ApplyBuff(attacker, blindness);
            result.FogAndBlindness = Attack("fog-plus-blindness", attacker,
                target, weapon, new[] { nativeFog }, null);
            RemoveBuff(attacker, blindFact);
            Buff darknessFact = ApplyBuff(attacker, darkness);
            result.FogAndDarkness = Attack("fog-plus-darkness", attacker,
                target, weapon, new[] { nativeFog }, null);
            RemoveBuff(attacker, darknessFact);
            result.FogAndMirrorImage = Attack("fog-plus-mirror-image",
                attacker, target, weapon, new[] { nativeFog, mirrorImage },
                mirrorImage);
            return result;
        }

        private static AttackEvidence Attack(string name,
            UnitEntityData attacker, UnitEntityData target,
            ItemEntityWeapon weapon, IEnumerable<BlueprintBuff> blueprints,
            BlueprintBuff retained)
        {
            var facts = new List<Buff>();
            try
            {
                foreach (BlueprintBuff blueprint in blueprints)
                    facts.Add(ApplyBuff(target, blueprint));
                var roll = new RuleAttackRoll(attacker, target, weapon, -100);
                UnityEngine.Random.InitState(FindNativeD100ThenD20Seed(19));
                SeekingConcealmentRuntime.QueueForcedRoll(weapon, 1);
                Rulebook.Trigger(roll);
                SeekingConcealmentRuntime.CancelForcedRoll();
                RuleConcealmentCheck check = roll.ConcealmentCheck;
                bool extraRetained = retained == null ||
                    target.Descriptor.Buffs.GetBuff(retained) != null;

                var acProbe = new RuleAttackRoll(attacker, target, weapon,
                    -100);
                UnityEngine.Random.InitState(FindNativeD100ThenD20Seed(19));
                SeekingConcealmentRuntime.QueueForcedRoll(weapon, 100);
                Rulebook.Trigger(acProbe);
                SeekingConcealmentRuntime.CancelForcedRoll();
                RuleConcealmentCheck acCheck = acProbe.ConcealmentCheck;
                return new AttackEvidence
                {
                    Name = name,
                    IsHit = roll.IsHit,
                    HasCheck = check != null,
                    ForcedRoll = check == null ? -1 : check.Roll.Value,
                    CheckSuccess = check != null && check.Success,
                    Result = roll.Result.ToString(),
                    AcProbeConcealmentRoll = acCheck == null ? -1 :
                        acCheck.Roll.Value,
                    AcProbeAttackRoll = acProbe.Roll.Value,
                    AcProbeAttackBonus = acProbe.AttackBonus,
                    AcProbeTargetAc = acProbe.TargetAC,
                    ExtraBuffRetained = extraRetained
                };
            }
            finally
            {
                SeekingConcealmentRuntime.CancelForcedRoll();
                foreach (Buff fact in facts.AsEnumerable().Reverse())
                    RemoveBuff(target, fact);
            }
        }

        private static BlueprintBuff CreateConcealment(string name,
            ElementalFiresightConcealmentKind kind,
            ICollection<UnityEngine.Object> transient)
        {
            BlueprintBuff result = CreateBuff(name, transient);
            var concealment = ScriptableObject.CreateInstance<AddConcealment>();
            concealment.Concealment = Concealment.Partial;
            concealment.OnlyForAttacks = false;
            var marker = ScriptableObject.CreateInstance<
                ElementalFiresightConcealmentSource>();
            marker.Kind = kind;
            result.ComponentsArray = new BlueprintComponent[]
                { concealment, marker };
            transient.Add(concealment);
            transient.Add(marker);
            return result;
        }

        private static InnerBreathEvidence ExerciseInnerBreath(
            UnitEntityData breather, UnitEntityData control,
            BlueprintFeature breath,
            ICollection<UnityEngine.Object> transient)
        {
            BlueprintBuff[] exact = ElementalFeatPolicy
                .ExactNativeRespirationRequiredBuffGuids().Select(guid =>
                    Exact<BlueprintBuff>(guid,
                        "Inner Breath native respiration catalog")).ToArray();
            var result = new InnerBreathEvidence
            {
                ExactNativeGuids = exact.Select(value => value.AssetGuid)
                    .ToList()
            };
            foreach (BlueprintBuff blueprint in exact)
                if (ApplyAndRemove(control, blueprint))
                    result.ControlExactApplied++;

            EnsureFact(breather.Descriptor, breath);
            foreach (BlueprintBuff blueprint in exact)
            {
                Buff applied = TryApplyBuff(breather, blueprint);
                bool absent = applied == null &&
                    breather.Descriptor.Buffs.GetBuff(blueprint) == null;
                if (absent) result.ExactBlocked++;
                RemoveBuff(breather, applied);
            }

            BlueprintBuff marked = CreateBuff(
                "KMG_Runtime_RespirationRequired", transient);
            var marker = ScriptableObject.CreateInstance<
                ElementalRespirationRequired>();
            marked.ComponentsArray = new BlueprintComponent[] { marker };
            transient.Add(marker);
            Buff project = TryApplyBuff(breather, marked);
            result.ProjectMarkedBlocked = project == null &&
                breather.Descriptor.Buffs.GetBuff(marked) == null;
            RemoveBuff(breather, project);

            result.OrdinaryPoisonApplied = ApplyAndRemove(breather,
                Exact<BlueprintBuff>(OrdinaryPoisonGuid,
                    "Inner Breath ordinary-poison control"));
            result.StinkingCloudApplied = ApplyAndRemove(breather,
                Exact<BlueprintBuff>(StinkingCloudGuid,
                    "Inner Breath Stinking Cloud control"));
            result.CloudkillApplied = ApplyAndRemove(breather,
                Exact<BlueprintBuff>(CloudkillGuid,
                    "Inner Breath Cloudkill control"));
            result.SwampGasDotApplied = ApplyAndRemove(breather,
                Exact<BlueprintBuff>(SwampGasDotGuid,
                    "Inner Breath non-poison gas control"));
            return result;
        }

        private static bool ApplyAndRemove(UnitEntityData unit,
            BlueprintBuff blueprint)
        {
            Buff applied = TryApplyBuff(unit, blueprint);
            bool present = applied != null &&
                unit.Descriptor.Buffs.GetBuff(blueprint) != null;
            RemoveBuff(unit, applied);
            return present;
        }

        private static BlueprintBuff CreateBuff(string name,
            ICollection<UnityEngine.Object> transient)
        {
            var result = ScriptableObject.CreateInstance<BlueprintBuff>();
            result.name = name;
            result.IsClassFeature = false;
            result.Stacking = StackingType.Replace;
            result.ComponentsArray = Array.Empty<BlueprintComponent>();
            result.FxOnStart = new PrefabLink();
            result.FxOnRemove = new PrefabLink();
            result.ResourceAssetIds = Array.Empty<string>();
            transient.Add(result);
            return result;
        }

        private static BlueprintAbility FindOrdinaryControl()
        {
            BlueprintAbility[] candidates = BlueprintBootstrap.Library
                .GetAllBlueprints().OfType<BlueprintAbility>()
                .Where(value => value != null && value.Type ==
                    AbilityType.Spell && value.Parent == null &&
                    string.Equals(value.name, "MageArmor",
                        StringComparison.Ordinal) &&
                    (value.SpellDescriptor & SpellDescriptor.Electricity) == 0 &&
                    !ElementalFeatPolicy.IsExactNativeAirEffectGuid(
                        value.AssetGuid)).ToArray();
            if (candidates.Length != 1)
                throw new InvalidOperationException(
                    "The exact native Mage Armor control is ambiguous: " +
                    candidates.Length + ".");
            return candidates[0];
        }

        private static Fact EnsureFact(UnitDescriptor owner,
            BlueprintUnitFact blueprint)
        {
            Fact existing = owner.GetFact(blueprint);
            if (existing != null) return existing;
            Fact added = owner.AddFact(blueprint);
            if (added == null || !owner.HasFact(blueprint))
                throw new InvalidOperationException(
                    "Request-local unit rejected fact " +
                    blueprint.AssetGuid + ".");
            return added;
        }

        private static Buff ApplyBuff(UnitEntityData unit,
            BlueprintBuff blueprint)
        {
            Buff result = TryApplyBuff(unit, blueprint);
            if (result == null)
                throw new InvalidOperationException(
                    "Request-local unit rejected buff " +
                    (blueprint == null ? "<null>" : blueprint.AssetGuid) +
                    ".");
            return result;
        }

        private static Buff TryApplyBuff(UnitEntityData unit,
            BlueprintBuff blueprint)
        {
            if (unit == null || unit.Descriptor == null || blueprint == null)
                return null;
            var context = new MechanicsContext(unit, unit.Descriptor,
                blueprint, null, new TargetWrapper(unit));
            return unit.Descriptor.Buffs.AddBuff(blueprint, context,
                TimeSpan.FromSeconds(60d));
        }

        private static void RemoveBuff(UnitEntityData unit, Buff fact)
        {
            if (unit != null && unit.Descriptor != null && fact != null &&
                unit.Descriptor.Buffs.RawFacts.Contains(fact))
                unit.Descriptor.Buffs.RemoveFact(fact);
        }

        private static UnitEntityData CreateUnit(BlueprintRace race,
            ICollection<UnitEntityData> created,
            ICollection<UnityEngine.Object> transient, string suffix)
        {
            BlueprintUnit blueprint = UnityEngine.Object.Instantiate(
                BlueprintRoot.Instance.DefaultPlayerCharacter);
            blueprint.name = "KMG_Runtime_ElementalSylphFeats_" + suffix;
            if (race != null) blueprint.Race = race;
            blueprint.Brain = null;
            blueprint.IsCheater = false;
            transient.Add(blueprint);
            UnitEntityData result = new ChargenUnit(blueprint).Unit;
            if (result == null || result.Descriptor == null ||
                (race != null && !ReferenceEquals(
                    result.Descriptor.Progression.Race, race)))
                throw new InvalidOperationException(
                    "A request-local Sylph feat unit was unavailable.");
            result.Descriptor.Stats.HitPoints.BaseValue = 500;
            result.Descriptor.State.Immortality.Retain();
            if (!Game.Instance.State.Units.All.Add(result))
            {
                result.Dispose();
                throw new InvalidOperationException(
                    "A request-local Sylph feat unit could not be registered.");
            }
            created.Add(result);
            return result;
        }

        private static int FindNativeD100ThenD20Seed(int expectedD20)
        {
            for (int seed = 1; seed <= 100000; seed++)
            {
                UnityEngine.Random.InitState(seed);
                int ignored = RulebookEvent.Dice.D100.Value;
                if (RulebookEvent.Dice.D20.Value == expectedD20) return seed;
            }
            throw new InvalidOperationException(
                "No deterministic native D100/D20 seed was found.");
        }

        private static T Exact<T>(string guid, string purpose)
            where T : BlueprintScriptableObject
        {
            return BlueprintLibraryLookup.RequireExact<T>(
                BlueprintBootstrap.Library, guid, purpose);
        }

        private static string DescribePatches(ModContext context,
            out bool installed)
        {
            PropertyInfo property = typeof(RuleConcealmentCheck).GetProperty(
                "Success", BindingFlags.Instance | BindingFlags.Public |
                    BindingFlags.NonPublic);
            MethodInfo getter = property == null ? null :
                property.GetGetMethod(true);
            Patches patches = getter == null ? null :
                context.Harmony.GetPatchInfo(getter);
            Patch[] postfixes = patches == null ? new Patch[0] :
                patches.Postfixes.ToArray();
            bool cloud = postfixes.Any(value => value.patch != null &&
                value.patch.DeclaringType ==
                    typeof(ElementalCloudGazerConcealmentPatch));
            bool seeking = postfixes.Any(value => value.patch != null &&
                value.patch.DeclaringType ==
                    typeof(SeekingConcealmentSuccessPatch));
            installed = getter != null && cloud && seeking;
            return "target=" + (getter == null ? "<missing>" :
                getter.DeclaringType.FullName + "." + getter.Name) +
                ";cloudGazer=" + cloud + ";seeking=" + seeking +
                ";postfixes=" + string.Join("|", postfixes.Select(value =>
                    value.owner + "/" + value.priority + "/" +
                    (value.patch == null ||
                     value.patch.DeclaringType == null ? "<missing>" :
                        value.patch.DeclaringType.FullName + "." +
                            value.patch.Name)).ToArray());
        }

        private static void AddAssertions(
            ICollection<RuntimeTestAssertion> assertions, Evidence evidence,
            RuntimeTestRequest request, ModContext context)
        {
            Add(assertions, "elemental-sylph-feats-module-active", "true",
                evidence.ModuleActive.ToString(), evidence.ModuleActive,
                "active feature-module snapshot");
            Add(assertions, "elemental-sylph-feats-blueprint-contract",
                "Airy Step and Inner Breath production components with exact 11/1/2 catalogs",
                evidence.BlueprintSummary ?? "<missing>",
                evidence.BlueprintContract,
                "live registered production blueprints");
            Add(assertions, "cloud-gazer-concealment-patch-registry",
                "Cloud Gazer and Seeking postfixes are installed on the exact RuleConcealmentCheck.Success getter",
                evidence.ConcealmentPatchSummary ?? "<missing>",
                evidence.ConcealmentPatchContract,
                "Harmony12 exact target patch registry");

            AiryStepEvidence airy = evidence.AiryStep;
            Add(assertions, "airy-step-descriptor-and-damage-contract",
                "nonmatching +0; electricity descriptor +2; exact air effect +2; direct electricity damage +2",
                airy == null ? "<missing>" : airy.Summary(),
                airy != null && airy.Nonmatching.Delta == 0 &&
                    airy.Electricity.Delta == 2 && airy.Air.Delta == 2 &&
                    airy.DirectElectricityDelta == 2,
                "actual RuleSavingThrow events and RuleDealDamage reason");
            Add(assertions, "airy-step-parent-overlap-deduplication",
                "parent air +2 and air/electricity overlap +2 exactly once",
                airy == null ? "<missing>" : airy.Summary(),
                airy != null && airy.ParentAir.Delta == 2 &&
                    airy.Overlap.Delta == 2,
                "spell-variant parent chain and per-event claim ledger");
            Add(assertions, "airy-step-exact-air-catalog",
                "all 11 audited native Air effects grant +2 exactly once",
                airy == null ? "<missing>" : airy.Summary(),
                airy != null && airy.ExactAirCatalog != null &&
                    airy.ExactAirCatalog.Count == 11 &&
                    airy.ExactAirCatalog.All(value => value.Delta == 2),
                "exact native GUID catalog through actual saving throws");
            Add(assertions, "wings-of-air-replaces-airy-step-save-bonus",
                "+4 total, not +6, with no persistent stat modifier",
                airy == null ? "<missing>" : airy.Summary(),
                airy != null && airy.WingsDelta == 4 &&
                    airy.ModifierLeakAbsent,
                "actual RuleSavingThrow and temporary racial modifier cleanup");

            CloudGazerEvidence cloud = evidence.CloudGazer;
            Add(assertions, "cloud-gazer-attack-roll-isolation",
                "every forced-success control rolls 19 and independently beats native target AC",
                cloud == null ? "<missing>" : cloud.Summary(),
                cloud != null && cloud.AcIsolated(),
                "native RuleAttackRoll roll, attack bonus, and target AC");
            Add(assertions, "cloud-gazer-fog-only-contract",
                "without feat native fog fails; with feat exact native and project fog succeed; smoke fails",
                cloud == null ? "<missing>" : cloud.Summary(),
                cloud != null && cloud.WithoutFeatFog.Preserved() &&
                    cloud.NativeFog.Bypassed() &&
                    cloud.ProjectFog.Bypassed() && cloud.Smoke.Preserved(),
                "actual RuleAttackRoll and RuleConcealmentCheck forced to 1");
            Add(assertions, "cloud-gazer-concealment-exclusions",
                "Blur, displacement, and concurrent unrelated concealment remain effective",
                cloud == null ? "<missing>" : cloud.Summary(),
                cloud != null && cloud.Blur.Preserved() &&
                    cloud.Displacement.Preserved() &&
                    cloud.FogAndBlur.Preserved() &&
                    cloud.FogAndBlur.ExtraBuffRetained,
                "exact native buff GUIDs and native concealment checks");
            Add(assertions, "cloud-gazer-sight-state-exclusions",
                "invisibility, blindness, and darkness prevent fog bypass",
                cloud == null ? "<missing>" : cloud.Summary(),
                cloud != null && cloud.FogAndInvisibility.Preserved() &&
                    cloud.FogAndBlindness.Preserved() &&
                    cloud.FogAndDarkness.Preserved(),
                "native invisibility component, blindness condition, and exact darkness buff");
            Add(assertions, "cloud-gazer-mirror-image-independent",
                "fog concealment succeeds without removing or suppressing Mirror Image",
                cloud == null ? "<missing>" : cloud.Summary(),
                cloud != null && cloud.FogAndMirrorImage
                    .ReachedIndependentDefense("MirrorImage") &&
                    cloud.FogAndMirrorImage.ExtraBuffRetained,
                "actual attack resolution with native AddMirrorImage fact");

            InnerBreathEvidence breath = evidence.InnerBreath;
            Add(assertions, "inner-breath-exact-respiration-catalog",
                "both audited native respiration-required buffs apply to control and are blocked with Inner Breath",
                breath == null ? "<missing>" : breath.Summary(),
                breath != null && breath.ExactNativeGuids != null &&
                    breath.ExactNativeGuids.Count == 2 &&
                    breath.ControlExactApplied == 2 &&
                    breath.ExactBlocked == 2,
                "actual RuleApplyBuff events for exact native buff GUIDs");
            Add(assertions, "inner-breath-project-marker",
                "project-owned respiration-required effects are blocked",
                breath == null ? "<missing>" : breath.Summary(),
                breath != null && breath.ProjectMarkedBlocked,
                "actual RuleApplyBuff event with semantic marker");
            Add(assertions, "inner-breath-narrow-exclusions",
                "ordinary poison, Stinking Cloud, Cloudkill, and non-respiration SwampGasDOT remain applicable",
                breath == null ? "<missing>" : breath.Summary(),
                breath != null && breath.OrdinaryPoisonApplied &&
                    breath.StinkingCloudApplied && breath.CloudkillApplied &&
                    breath.SwampGasDotApplied,
                "actual RuleApplyBuff events for exact excluded GUIDs");

            Add(assertions, "elemental-sylph-feats-save-state-untouched",
                "false", evidence.SaveStateTouched.ToString(),
                !evidence.SaveStateTouched,
                "save-free request-local fixture contract");
            Add(assertions, "elemental-sylph-feats-cleanup-exact", "true",
                evidence.CleanupExact.ToString(), evidence.CleanupExact,
                "exact pre/post Game.State unit references");
            Add(assertions, "loaded-mod-version", request.ExpectedModVersion,
                context.ModEntry.Info.Version,
                request.ExpectedModVersion == context.ModEntry.Info.Version,
                "Unity Mod Manager ModEntry.Info.Version");
        }

        private static void Add(
            ICollection<RuntimeTestAssertion> assertions, string name,
            string expected, string observed, bool pass, string source)
        {
            assertions.Add(new RuntimeTestAssertion
            {
                Name = name,
                Expected = expected,
                Observed = observed,
                Status = pass ? RuntimeTestStatuses.Pass :
                    RuntimeTestStatuses.Fail,
                Evidence = source
            });
        }

        private static bool SameReferences<T>(IList<T> expected,
            IList<T> actual) where T : class
        {
            if (expected == null || actual == null ||
                expected.Count != actual.Count) return false;
            for (int index = 0; index < expected.Count; index++)
                if (!ReferenceEquals(expected[index], actual[index]))
                    return false;
            return true;
        }

        private static string Metadata(Assembly assembly, string key)
        {
            AssemblyMetadataAttribute value = assembly.GetCustomAttributes<
                AssemblyMetadataAttribute>().FirstOrDefault(attribute =>
                    string.Equals(attribute.Key, key,
                        StringComparison.Ordinal));
            return value == null ? string.Empty : value.Value;
        }

        private static string Hash(string path)
        {
            using (var stream = File.OpenRead(path))
            using (SHA256 sha = SHA256.Create())
                return BitConverter.ToString(sha.ComputeHash(stream))
                    .Replace("-", string.Empty).ToLowerInvariant();
        }
    }
}
