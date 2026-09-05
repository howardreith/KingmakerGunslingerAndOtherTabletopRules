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
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Blueprints.Root;
using Kingmaker.Controllers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.Enums.Damage;
using Kingmaker.Items;
using Kingmaker.ResourceLinks;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UI.LevelUp;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
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
    /// Save-free live-rule qualification for Blazing Aura and Firesight.
    /// Transient fire/smoke facts are explicitly project-marked; native
    /// excluded controls are exact blueprints from the guarded KMG-only audit.
    /// </summary>
    internal static class ElementalIfritAdvancedFeatScenario
    {
        internal const string EvidenceFileName =
            "elemental-ifrit-advanced-feats.json";
        private const string ShortSwordGuid =
            "57c8994d1f1becf49ac4f642e5d8ca9d";
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
        private const string DazzledGuid =
            "df6d1025da07524429afbae248845ecc";

        private sealed class ActivationEvidence
        {
            public bool AvailableBefore { get; set; }
            public bool CancelCanStart { get; set; }
            public bool CancelInstalled { get; set; }
            public bool CancelStarted { get; set; }
            public int CancelAuraCount { get; set; }
            public string CommandResult { get; set; }
            public bool ProcessPresent { get; set; }
            public bool ProcessEnded { get; set; }
            public int AuraCount { get; set; }
            public double AuraSeconds { get; set; }
            public bool AvailableWhileActive { get; set; }
            public bool NonIfritRejected { get; set; }

            public bool Pass()
            {
                return AvailableBefore && CancelCanStart && CancelInstalled &&
                    !CancelStarted && CancelAuraCount == 0 &&
                    CommandResult == "Success" && ProcessPresent &&
                    ProcessEnded && AuraCount == 1 && AuraSeconds > 5d &&
                    AuraSeconds <= 7d && !AvailableWhileActive &&
                    NonIfritRejected;
            }

            public string Summary()
            {
                return "available=" + AvailableBefore + "->" +
                    AvailableWhileActive + ";cancel=" + CancelCanStart + "/" +
                    CancelInstalled + "/" + CancelStarted + "/aura=" +
                    CancelAuraCount + ";command=" + CommandResult + "/" +
                    ProcessPresent + "/" + ProcessEnded + ";aura=" +
                    AuraCount + "/" + AuraSeconds.ToString("F3") +
                    ";nonIfritRejected=" + NonIfritRejected;
            }
        }

        private sealed class AuraEvidence
        {
            public bool Friendly { get; set; }
            public int AdjacentRules { get; set; }
            public int FirePackets { get; set; }
            public int DiceCount { get; set; }
            public string DiceType { get; set; }
            public int AdjacentDamage { get; set; }
            public int DuplicateRules { get; set; }
            public int DuplicateDamage { get; set; }
            public int FarRules { get; set; }
            public int FarDamage { get; set; }
            public int SelfRules { get; set; }
            public int SelfDamage { get; set; }
            public int ResistantRules { get; set; }
            public int ResistantDamage { get; set; }

            public bool Pass()
            {
                return Friendly && AdjacentRules == 1 && FirePackets == 1 &&
                    DiceCount == 1 && DiceType ==
                        Kingmaker.RuleSystem.DiceType.D6.ToString() &&
                    AdjacentDamage >= 1 && AdjacentDamage <= 6 &&
                    DuplicateRules == 0 && DuplicateDamage == 0 &&
                    FarRules == 0 && FarDamage == 0 && SelfRules == 0 &&
                    SelfDamage == 0 && ResistantRules == 10 &&
                    ResistantDamage >= 0 && ResistantDamage <= 10;
            }

            public string Summary()
            {
                return "friendly=" + Friendly + ";adjacent=" +
                    AdjacentRules + "/packets=" + FirePackets + "/dice=" +
                    DiceCount + DiceType + "/damage=" + AdjacentDamage +
                    ";duplicate=" + DuplicateRules + "/" + DuplicateDamage +
                    ";far=" + FarRules + "/" + FarDamage + ";self=" +
                    SelfRules + "/" + SelfDamage + ";resistant=" +
                    ResistantRules + "/" + ResistantDamage;
            }
        }

        private sealed class AttackEvidence
        {
            public string Name { get; set; }
            public bool IsHit { get; set; }
            public bool HasCheck { get; set; }
            public string Concealment { get; set; }
            public int ForcedRoll { get; set; }
            public bool CheckSuccess { get; set; }
            public bool ExtraBuffRetained { get; set; }
            public string Result { get; set; }
            public int AttackRoll { get; set; }
            public int AttackBonus { get; set; }
            public int TargetAc { get; set; }
            public string AcProbeResult { get; set; }
            public int AcProbeConcealmentRoll { get; set; }
            public int AcProbeAttackRoll { get; set; }
            public int AcProbeAttackBonus { get; set; }
            public int AcProbeTargetAc { get; set; }

            public bool WouldHitAc()
            {
                return AcProbeConcealmentRoll == 100 &&
                    AcProbeAttackRoll == 19 && AcProbeAttackRoll +
                    AcProbeAttackBonus >= AcProbeTargetAc;
            }

            public bool Bypassed()
            {
                return IsHit && HasCheck && ForcedRoll == 1 &&
                    WouldHitAc();
            }

            public bool Preserved()
            {
                return !IsHit && HasCheck && ForcedRoll == 1 &&
                    !CheckSuccess && WouldHitAc();
            }

            public bool ReachedIndependentDefense(string expectedResult)
            {
                return !IsHit && HasCheck && ForcedRoll == 1 &&
                    string.Equals(Result, expectedResult,
                        StringComparison.Ordinal) && WouldHitAc();
            }

            public string Summary()
            {
                return Name + "=hit:" + IsHit + ",check:" + HasCheck +
                    ",kind:" + Concealment + ",roll:" + ForcedRoll +
                    ",postEventNativeSuccess:" + CheckSuccess +
                    ",result:" + Result + ",attack:" + AttackRoll + "+" +
                    AttackBonus + "vs" + TargetAc + ",acProbe:" +
                    AcProbeResult + "/concealment=" +
                    AcProbeConcealmentRoll + "/attack=" +
                    AcProbeAttackRoll + "+" + AcProbeAttackBonus + "vs" +
                    AcProbeTargetAc + ",extraRetained:" +
                    ExtraBuffRetained;
            }
        }

        private sealed class FiresightEvidence
        {
            public AttackEvidence WithoutFeatSmoke { get; set; }
            public AttackEvidence Smoke { get; set; }
            public AttackEvidence Fire { get; set; }
            public AttackEvidence ProjectFog { get; set; }
            public AttackEvidence Blur { get; set; }
            public AttackEvidence Displacement { get; set; }
            public AttackEvidence NativeFog { get; set; }
            public AttackEvidence SmokeAndBlur { get; set; }
            public AttackEvidence SmokeAndInvisibility { get; set; }
            public AttackEvidence SmokeAndBlindness { get; set; }
            public AttackEvidence SmokeAndDarkness { get; set; }
            public AttackEvidence SmokeAndMirrorImage { get; set; }
            public bool DazzledBlocked { get; set; }
            public bool DazzledControlApplied { get; set; }

            public bool AcIsolated()
            {
                return new[] { WithoutFeatSmoke, Smoke, Fire, ProjectFog,
                    Blur, Displacement, NativeFog, SmokeAndBlur,
                    SmokeAndInvisibility, SmokeAndBlindness,
                    SmokeAndDarkness, SmokeAndMirrorImage }
                    .All(value => value != null && value.WouldHitAc());
            }

            public string Summary()
            {
                return string.Join(";", new[]
                {
                    WithoutFeatSmoke.Summary(), Smoke.Summary(),
                    Fire.Summary(), ProjectFog.Summary(), Blur.Summary(),
                    Displacement.Summary(), NativeFog.Summary(),
                    SmokeAndBlur.Summary(), SmokeAndInvisibility.Summary(),
                    SmokeAndBlindness.Summary(), SmokeAndDarkness.Summary(),
                    SmokeAndMirrorImage.Summary(), "dazzled=" +
                    DazzledBlocked + "/" + DazzledControlApplied
                });
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
            public ActivationEvidence Activation { get; set; }
            public AuraEvidence Aura { get; set; }
            public FiresightEvidence Firesight { get; set; }
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
                BlueprintItemWeapon sword = Exact<BlueprintItemWeapon>(
                    ShortSwordGuid, "advanced Ifrit attack control");
                BlueprintBuff blur = Exact<BlueprintBuff>(BlurGuid, "Blur");
                BlueprintBuff displacement = Exact<BlueprintBuff>(
                    DisplacementGuid, "Displacement");
                BlueprintBuff nativeFog = Exact<BlueprintBuff>(
                    ObscuringMistGuid, "Obscuring Mist");
                BlueprintBuff invisibility = Exact<BlueprintBuff>(
                    InvisibilityGuid, "Invisibility");
                BlueprintBuff mirrorImage = Exact<BlueprintBuff>(
                    MirrorImageGuid, "Mirror Image");
                BlueprintBuff blindness = Exact<BlueprintBuff>(
                    BlindnessGuid, "Blindness");
                BlueprintBuff darkness = Exact<BlueprintBuff>(
                    DarknessGuid, "Touch of Darkness");
                BlueprintBuff dazzled = Exact<BlueprintBuff>(
                    DazzledGuid, "Dazzled");

                BlueprintFeature blazing = feats.RequireFeature(
                    ElementalFeatId.BlazingAura);
                BlueprintFeature firesight = feats.RequireFeature(
                    ElementalFeatId.Firesight);
                BlueprintAbility auraAbility = GrantedAbility(blazing);
                ElementalBlazingAuraAbilityLogic auraLogic = auraAbility
                    .ComponentsArray.OfType<
                        ElementalBlazingAuraAbilityLogic>().Single();
                BlueprintBuff aura = auraLogic.Aura;
                BlueprintBuff scorching = auraLogic
                    .ScorchingWeaponsMarker;
                AddConditionImmunity dazzledImmunity = firesight
                    .ComponentsArray.OfType<AddConditionImmunity>().Single(
                        value => value.Condition == UnitCondition.Dazzled);
                evidence.BlueprintContract =
                    auraAbility.ActionType == UnitCommand.CommandType.Free &&
                    auraAbility.Type == AbilityType.Extraordinary &&
                    auraAbility.Range == AbilityRange.Personal &&
                    ReferenceEquals(auraLogic.Ifrit, races.Ifrit.Race) &&
                    aura != null && scorching != null &&
                    dazzledImmunity != null && ElementalFeatPolicy
                        .ExactNativeFiresightConcealmentGuids().Length == 0;
                evidence.BlueprintSummary = "blazing=" + blazing.AssetGuid +
                    ";ability=" + auraAbility.AssetGuid + ";aura=" +
                    aura.AssetGuid + ";action/type/range=" +
                    auraAbility.ActionType + "/" + auraAbility.Type + "/" +
                    auraAbility.Range + ";scorching=" + scorching.AssetGuid +
                    ";firesight=" + firesight.AssetGuid +
                    ";dazzledImmunity=" + dazzledImmunity.Condition +
                    ";nativeCatalog=" + ElementalFeatPolicy
                        .ExactNativeFiresightConcealmentGuids().Length;
                bool concealmentPatchContract;
                evidence.ConcealmentPatchSummary =
                    DescribeConcealmentPatches(context,
                        out concealmentPatchContract);
                evidence.ConcealmentPatchContract =
                    concealmentPatchContract;

                stage = "activation-and-aura";
                UnitEntityData owner = CreateUnit(races.Ifrit.Race, created,
                    transient, "AuraOwner");
                UnitEntityData ally = CreateUnit(null, created, transient,
                    "FriendlyAdjacent");
                UnitEntityData far = CreateUnit(null, created, transient,
                    "FarCreature");
                UnitEntityData resistant = CreateUnit(null, created,
                    transient, "ResistantAdjacent");
                UnitEntityData outsider = CreateUnit(null, created,
                    transient, "NonIfrit");
                SetPosition(owner, Vector3.zero);
                SetPosition(ally, new Vector3(1f, 0f, 0f));
                SetPosition(resistant, new Vector3(1f, 0f, 0f));
                SetPosition(far, new Vector3(20f, 0f, 0f));
                EnsureFact(owner.Descriptor, blazing);
                EnsureFact(outsider.Descriptor, blazing);
                Buff scorchingFact = ApplyBuff(owner, scorching);
                evidence.Activation = ExerciseActivation(owner, outsider,
                    auraAbility, aura);
                evidence.Aura = ExerciseAura(owner, ally, far, resistant,
                    races.Ifrit.Resistance);

                stage = "firesight-attacks";
                UnitEntityData attacker = CreateUnit(races.Ifrit.Race,
                    created, transient, "FiresightAttacker");
                UnitEntityData target = CreateUnit(null, created, transient,
                    "FiresightTarget");
                attacker.Descriptor.Stats.BaseAttackBonus.BaseValue = 20;
                attacker.Descriptor.Stats.Dexterity.BaseValue = 30;
                var weapon = new ItemEntityWeapon(sword);
                items.Add(weapon);
                attacker.Body.PrimaryHand.InsertItem(weapon);
                if (!ReferenceEquals(attacker.Body.PrimaryHand.MaybeWeapon,
                        weapon))
                    throw new InvalidOperationException(
                        "The Firesight fixture rejected its native weapon.");
                BlueprintBuff smoke = CreateConcealment(
                    "KMG_Runtime_Firesight_Smoke",
                    ElementalFiresightConcealmentKind.Smoke, transient);
                BlueprintBuff fire = CreateConcealment(
                    "KMG_Runtime_Firesight_Fire",
                    ElementalFiresightConcealmentKind.Fire, transient);
                BlueprintBuff projectFog = CreateConcealment(
                    "KMG_Runtime_Firesight_Fog",
                    ElementalFiresightConcealmentKind.FogMistOrCloud,
                    transient);
                evidence.Firesight = ExerciseFiresight(attacker, target,
                    weapon, firesight, smoke, fire, projectFog, blur,
                    displacement, nativeFog, invisibility, mirrorImage,
                    blindness, darkness, dazzled);
                if (scorchingFact == null)
                    throw new InvalidOperationException(
                        "The activation prerequisite marker disappeared.");
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
            diagnostics.Add("elementalIfritAdvancedFeatSha256=" + Hash(path));
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

        private static ActivationEvidence ExerciseActivation(
            UnitEntityData owner, UnitEntityData outsider,
            BlueprintAbility ability, BlueprintBuff aura)
        {
            Ability fact = owner.Descriptor.Abilities.GetAbility(ability);
            if (fact == null)
                throw new InvalidOperationException(
                    "The Blazing Aura granted ability is absent.");
            var data = new AbilityData(fact);
            var result = new ActivationEvidence
            {
                AvailableBefore = data.IsAvailable
            };
            UnitUseAbility canceled = CreateCommand(data,
                new TargetWrapper(owner), owner);
            result.CancelCanStart = canceled.CanStart;
            owner.Commands.Run(canceled);
            result.CancelInstalled = owner.Commands.Contains(canceled);
            result.CancelStarted = canceled.IsStarted;
            owner.Commands.InterruptAll(true);
            result.CancelAuraCount = CountBuff(owner, aura);

            UnitUseAbility command = CreateCommand(data,
                new TargetWrapper(owner), owner);
            object commandResult = InvokeCommandAction(command);
            AbilityExecutionProcess process = command.ExecutionProcess;
            if (process != null)
            {
                for (int tick = 0; tick < 5000 && !process.IsEnded; tick++)
                    process.Tick();
                if (!process.IsEnded)
                {
                    process.InstantDeliver();
                    for (int tick = 0; tick < 5000 && !process.IsEnded;
                        tick++) process.Tick();
                }
                if (!process.IsEnded) process.Detach();
            }
            InvokeCommandEnded(command, false);
            Buff auraFact = owner.Descriptor.Buffs.GetBuff(aura);
            result.CommandResult = commandResult == null ? string.Empty :
                commandResult.ToString();
            result.ProcessPresent = process != null;
            result.ProcessEnded = process != null && process.IsEnded;
            result.AuraCount = CountBuff(owner, aura);
            result.AuraSeconds = auraFact == null ? -1d :
                auraFact.TimeLeft.TotalSeconds;
            result.AvailableWhileActive = new AbilityData(fact).IsAvailable;

            Ability outsiderFact = outsider.Descriptor.Abilities.GetAbility(
                ability);
            result.NonIfritRejected = outsiderFact != null &&
                !new AbilityData(outsiderFact).IsAvailable;
            return result;
        }

        private static AuraEvidence ExerciseAura(UnitEntityData owner,
            UnitEntityData ally, UnitEntityData far,
            UnitEntityData resistant, BlueprintFeature resistance)
        {
            var result = new AuraEvidence
            {
                Friendly = !owner.IsEnemy(ally)
            };
            int before = ally.Descriptor.Damage;
            object adjacentTurn = new object();
            RuleDealDamage[] adjacent = ElementalBlazingAuraRuntime
                .HandleCreatureTurnStarted(ally, adjacentTurn);
            result.AdjacentRules = adjacent.Length;
            result.AdjacentDamage = ally.Descriptor.Damage - before;
            EnergyDamage[] packets = adjacent.SelectMany(value =>
                value.DamageBundle == null ? Enumerable.Empty<EnergyDamage>() :
                value.DamageBundle.OfType<EnergyDamage>()).Where(value =>
                    value.EnergyType == DamageEnergyType.Fire).ToArray();
            result.FirePackets = packets.Length;
            result.DiceCount = packets.Length == 1 ? packets[0].Dice.Rolls :
                -1;
            result.DiceType = packets.Length == 1 ?
                packets[0].Dice.Dice.ToString() : string.Empty;

            before = ally.Descriptor.Damage;
            result.DuplicateRules = ElementalBlazingAuraRuntime
                .HandleCreatureTurnStarted(ally, adjacentTurn).Length;
            result.DuplicateDamage = ally.Descriptor.Damage - before;

            before = far.Descriptor.Damage;
            result.FarRules = ElementalBlazingAuraRuntime
                .HandleCreatureTurnStarted(far, new object()).Length;
            result.FarDamage = far.Descriptor.Damage - before;

            before = owner.Descriptor.Damage;
            result.SelfRules = ElementalBlazingAuraRuntime
                .HandleCreatureTurnStarted(owner, new object()).Length;
            result.SelfDamage = owner.Descriptor.Damage - before;

            EnsureFact(resistant.Descriptor, resistance);
            before = resistant.Descriptor.Damage;
            for (int index = 0; index < 10; index++)
                result.ResistantRules += ElementalBlazingAuraRuntime
                    .HandleCreatureTurnStarted(resistant, new object()).Length;
            result.ResistantDamage = resistant.Descriptor.Damage - before;
            return result;
        }

        private static FiresightEvidence ExerciseFiresight(
            UnitEntityData attacker, UnitEntityData target,
            ItemEntityWeapon weapon, BlueprintFeature firesight,
            BlueprintBuff smoke, BlueprintBuff fire,
            BlueprintBuff projectFog, BlueprintBuff blur,
            BlueprintBuff displacement, BlueprintBuff nativeFog,
            BlueprintBuff invisibility, BlueprintBuff mirrorImage,
            BlueprintBuff blindness, BlueprintBuff darkness,
            BlueprintBuff dazzled)
        {
            var result = new FiresightEvidence();
            result.WithoutFeatSmoke = Attack("without-feat-smoke", attacker,
                target, weapon, new[] { smoke }, null);
            Fact firesightFact = EnsureFact(attacker.Descriptor, firesight);
            result.Smoke = Attack("project-smoke", attacker, target, weapon,
                new[] { smoke }, null);
            result.Fire = Attack("project-fire", attacker, target, weapon,
                new[] { fire }, null);
            result.ProjectFog = Attack("project-fog", attacker, target,
                weapon, new[] { projectFog }, null);
            result.Blur = Attack("native-blur", attacker, target, weapon,
                new[] { blur }, null);
            result.Displacement = Attack("native-displacement", attacker,
                target, weapon, new[] { displacement }, null);
            result.NativeFog = Attack("native-fog", attacker, target, weapon,
                new[] { nativeFog }, null);
            result.SmokeAndBlur = Attack("smoke-plus-blur", attacker, target,
                weapon, new[] { smoke, blur }, blur);
            result.SmokeAndInvisibility = Attack(
                "smoke-plus-invisibility", attacker, target, weapon,
                new[] { smoke, invisibility }, invisibility);

            Buff blindFact = ApplyBuff(attacker, blindness);
            result.SmokeAndBlindness = Attack("smoke-plus-blindness",
                attacker, target, weapon, new[] { smoke }, null);
            RemoveBuff(attacker, blindFact);
            Buff darknessFact = ApplyBuff(attacker, darkness);
            result.SmokeAndDarkness = Attack("smoke-plus-darkness",
                attacker, target, weapon, new[] { smoke }, null);
            RemoveBuff(attacker, darknessFact);
            result.SmokeAndMirrorImage = Attack("smoke-plus-mirror-image",
                attacker, target, weapon, new[] { smoke, mirrorImage },
                mirrorImage);

            Buff blockedDazzle = TryApplyBuff(attacker, dazzled);
            result.DazzledBlocked = !attacker.Descriptor.State.HasCondition(
                UnitCondition.Dazzled);
            RemoveBuff(attacker, blockedDazzle);
            attacker.Descriptor.RemoveFact(firesightFact);
            Buff controlDazzle = ApplyBuff(attacker, dazzled);
            result.DazzledControlApplied = attacker.Descriptor.State
                .HasCondition(UnitCondition.Dazzled);
            RemoveBuff(attacker, controlDazzle);
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
                // The established Seeking fixture uses a negative attack
                // penalty to make the native AC result unambiguously succeed.
                // Concealment is therefore the only remaining miss source.
                var roll = new RuleAttackRoll(attacker, target, weapon, -100);
                UnityEngine.Random.InitState(FindNativeD100ThenD20Seed(19));
                SeekingConcealmentRuntime.QueueForcedRoll(weapon, 1);
                Rulebook.Trigger(roll);
                SeekingConcealmentRuntime.CancelForcedRoll();
                RuleConcealmentCheck check = roll.ConcealmentCheck;
                bool extraBuffRetained = retained == null ||
                    target.Descriptor.Buffs.GetBuff(retained) != null;

                // Native concealment failures short-circuit before the attack
                // roll, leaving Roll/AttackBonus/TargetAC at their default
                // values. Run an otherwise identical forced-success control
                // while every case-specific fact is still active so the
                // evidence independently proves that armor class is not the
                // source of the expected miss.
                var acProbe = new RuleAttackRoll(attacker, target, weapon,
                    -100);
                UnityEngine.Random.InitState(FindNativeD100ThenD20Seed(19));
                SeekingConcealmentRuntime.QueueForcedRoll(weapon, 100);
                Rulebook.Trigger(acProbe);
                SeekingConcealmentRuntime.CancelForcedRoll();
                RuleConcealmentCheck acProbeCheck = acProbe.ConcealmentCheck;
                return new AttackEvidence
                {
                    Name = name,
                    IsHit = roll.IsHit,
                    HasCheck = check != null,
                    Concealment = check == null ? "None" :
                        check.Concealment.ToString(),
                    ForcedRoll = check == null ? -1 : check.Roll.Value,
                    CheckSuccess = check != null && check.Success,
                    ExtraBuffRetained = extraBuffRetained,
                    Result = roll.Result.ToString(),
                    AttackRoll = roll.Roll.Value,
                    AttackBonus = roll.AttackBonus,
                    TargetAc = roll.TargetAC,
                    AcProbeResult = acProbe.Result.ToString(),
                    AcProbeConcealmentRoll = acProbeCheck == null ? -1 :
                        acProbeCheck.Roll.Value,
                    AcProbeAttackRoll = acProbe.Roll.Value,
                    AcProbeAttackBonus = acProbe.AttackBonus,
                    AcProbeTargetAc = acProbe.TargetAC
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
            var result = ScriptableObject.CreateInstance<BlueprintBuff>();
            result.name = name;
            result.IsClassFeature = false;
            result.Stacking = StackingType.Replace;
            result.FxOnStart = new PrefabLink();
            result.FxOnRemove = new PrefabLink();
            result.ResourceAssetIds = Array.Empty<string>();
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
            transient.Add(result);
            return result;
        }

        private static BlueprintAbility GrantedAbility(
            BlueprintFeature feature)
        {
            return feature.ComponentsArray.OfType<AddFacts>()
                .SelectMany(value => value.Facts ??
                    Array.Empty<BlueprintUnitFact>())
                .OfType<BlueprintAbility>().Single();
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

        private static int CountBuff(UnitEntityData unit,
            BlueprintBuff blueprint)
        {
            return unit.Descriptor.Buffs.RawFacts.OfType<Buff>().Count(value =>
                ReferenceEquals(value.Blueprint, blueprint));
        }

        private static UnitEntityData CreateUnit(BlueprintRace race,
            ICollection<UnitEntityData> created,
            ICollection<UnityEngine.Object> transient, string suffix)
        {
            BlueprintUnit blueprint = UnityEngine.Object.Instantiate(
                BlueprintRoot.Instance.DefaultPlayerCharacter);
            blueprint.name = "KMG_Runtime_ElementalIfritAdvanced_" + suffix;
            if (race != null) blueprint.Race = race;
            blueprint.Brain = null;
            blueprint.IsCheater = false;
            transient.Add(blueprint);
            UnitEntityData result = new ChargenUnit(blueprint).Unit;
            if (result == null || result.Descriptor == null ||
                (race != null && !ReferenceEquals(
                    result.Descriptor.Progression.Race, race)))
                throw new InvalidOperationException(
                    "A request-local advanced Ifrit feat unit was unavailable.");
            result.Descriptor.Stats.HitPoints.BaseValue = 500;
            result.Descriptor.State.Immortality.Retain();
            if (!Game.Instance.State.Units.All.Add(result))
            {
                result.Dispose();
                throw new InvalidOperationException(
                    "A request-local advanced Ifrit unit could not be registered.");
            }
            created.Add(result);
            return result;
        }

        private static void SetPosition(UnitEntityData unit, Vector3 position)
        {
            PropertyInfo property = typeof(UnitEntityData).GetProperty(
                "Position", BindingFlags.Instance | BindingFlags.Public |
                    BindingFlags.NonPublic);
            if (property == null || !property.CanWrite)
                throw new MissingMemberException(typeof(UnitEntityData)
                    .FullName, "Position");
            property.SetValue(unit, position, null);
            if (unit.View != null) unit.View.transform.position = position;
        }

        private static UnitUseAbility CreateCommand(AbilityData data,
            TargetWrapper target, UnitEntityData caster)
        {
            UnitUseAbility result;
            var cutscene = new Kingmaker.AreaLogic.Cutscenes
                .CutsceneParametersContext();
            using (cutscene.Data)
                result = new UnitUseAbility(data, target);
            PropertyInfo executor = typeof(UnitCommand).GetProperty(
                "Executor", BindingFlags.Instance | BindingFlags.Public |
                    BindingFlags.NonPublic);
            MethodInfo setter = executor == null ? null :
                executor.GetSetMethod(true);
            if (setter == null)
                throw new MissingMethodException(
                    typeof(UnitCommand).FullName,
                    "set_Executor(UnitEntityData)");
            setter.Invoke(result, new object[] { caster });
            result.IgnoreCooldown(TimeSpan.Zero);
            return result;
        }

        private static object InvokeCommandAction(UnitUseAbility command)
        {
            MethodInfo method = typeof(UnitUseAbility).GetMethod("OnAction",
                BindingFlags.Instance | BindingFlags.Public |
                    BindingFlags.NonPublic, null, Type.EmptyTypes, null);
            if (method == null)
                throw new MissingMethodException(
                    typeof(UnitUseAbility).FullName, "OnAction()");
            return method.Invoke(command, null);
        }

        private static void InvokeCommandEnded(UnitUseAbility command,
            bool interrupted)
        {
            MethodInfo method = typeof(UnitUseAbility).GetMethod("OnEnded",
                BindingFlags.Instance | BindingFlags.Public |
                    BindingFlags.NonPublic, null,
                new[] { typeof(bool) }, null);
            if (method != null)
                method.Invoke(command, new object[] { interrupted });
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

        private static string DescribeConcealmentPatches(
            ModContext context, out bool installed)
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
            bool firesight = postfixes.Any(value => value.patch != null &&
                value.patch.DeclaringType ==
                    typeof(ElementalFiresightConcealmentPatch));
            bool seeking = postfixes.Any(value => value.patch != null &&
                value.patch.DeclaringType ==
                    typeof(SeekingConcealmentSuccessPatch));
            installed = getter != null && firesight && seeking;
            return "target=" + (getter == null ? "<missing>" :
                getter.DeclaringType.FullName + "." + getter.Name) +
                ";firesight=" + firesight + ";seeking=" + seeking +
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
            Add(assertions, "elemental-ifrit-advanced-module-active", "true",
                evidence.ModuleActive.ToString(), evidence.ModuleActive,
                "active feature-module snapshot");
            Add(assertions, "blazing-firesight-blueprint-contract",
                "stable free-action aura graph, native dazzled immunity, empty audited native fire/smoke catalog",
                evidence.BlueprintSummary ?? "<missing>",
                evidence.BlueprintContract,
                "live registered production blueprints");
            Add(assertions, "firesight-concealment-patch-registry",
                "Firesight and Seeking postfixes are installed on the exact RuleConcealmentCheck.Success getter",
                evidence.ConcealmentPatchSummary ?? "<missing>",
                evidence.ConcealmentPatchContract,
                "Harmony12 exact target patch registry");
            Add(assertions, "blazing-aura-command",
                "cancellation applies nothing; accepted free action applies one 1-round aura only while Scorching Weapons is active",
                evidence.Activation == null ? "<missing>" :
                    evidence.Activation.Summary(),
                evidence.Activation != null && evidence.Activation.Pass(),
                "native UnitUseAbility and AbilityExecutionProcess");
            Add(assertions, "blazing-aura-turn-start",
                "one native 1d6 fire rule to an adjacent friendly creature; exact-turn dedupe; self/far excluded",
                evidence.Aura == null ? "<missing>" :
                    evidence.Aura.Summary(),
                evidence.Aura != null && evidence.Aura.Pass(),
                "production turn-start handler and RuleDealDamage");
            FiresightEvidence sight = evidence.Firesight;
            Add(assertions, "firesight-attack-roll-isolation",
                "every forced-success control rolls 19 and independently beats native target AC",
                sight == null ? "<missing>" : sight.Summary(),
                sight != null && sight.AcIsolated(),
                "native RuleAttackRoll roll, attack bonus, and target AC");
            Add(assertions, "firesight-project-fire-smoke-only",
                "without feat smoke fails; exact project smoke/fire succeed; project fog fails",
                sight == null ? "<missing>" : sight.Summary(),
                sight != null && sight.WithoutFeatSmoke.Preserved() &&
                    sight.Smoke.Bypassed() && sight.Fire.Bypassed() &&
                    sight.ProjectFog.Preserved(),
                "native RuleAttackRoll/RuleConcealmentCheck forced to 1");
            Add(assertions, "firesight-native-exclusions",
                "Blur, displacement, Obscuring Mist, and concurrent Blur remain effective",
                sight == null ? "<missing>" : sight.Summary(),
                sight != null && sight.Blur.Preserved() &&
                    sight.Displacement.Preserved() &&
                    sight.NativeFog.Preserved() &&
                    sight.SmokeAndBlur.Preserved() &&
                    sight.SmokeAndBlur.ExtraBuffRetained,
                "exact native buff GUIDs and native concealment checks");
            Add(assertions, "firesight-sight-state-exclusions",
                "invisibility, blindness, and darkness prevent smoke bypass",
                sight == null ? "<missing>" : sight.Summary(),
                sight != null && sight.SmokeAndInvisibility.Preserved() &&
                    sight.SmokeAndBlindness.Preserved() &&
                    sight.SmokeAndDarkness.Preserved(),
                "native invisibility component, blindness condition, and exact darkness buff");
            Add(assertions, "firesight-mirror-image-independent",
                "smoke concealment succeeds without removing or suppressing Mirror Image",
                sight == null ? "<missing>" : sight.Summary(),
                sight != null && sight.SmokeAndMirrorImage
                    .ReachedIndependentDefense("MirrorImage") &&
                    sight.SmokeAndMirrorImage.ExtraBuffRetained,
                "actual attack resolution with native AddMirrorImage fact");
            Add(assertions, "firesight-dazzled-immunity",
                "Dazzled is blocked with Firesight and applies after Firesight removal",
                sight == null ? "<missing>" : sight.Summary(),
                sight != null && sight.DazzledBlocked &&
                    sight.DazzledControlApplied,
                "native AddConditionImmunity and exact Dazzled buff");
            Add(assertions, "elemental-ifrit-advanced-save-state-untouched",
                "false", evidence.SaveStateTouched.ToString(),
                !evidence.SaveStateTouched,
                "save-free request-local fixture contract");
            Add(assertions, "elemental-ifrit-advanced-cleanup-exact", "true",
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
