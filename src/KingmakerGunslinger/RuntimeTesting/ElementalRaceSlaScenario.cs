using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Root;
using Kingmaker.Controllers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Items;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Class.LevelUp;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.Utility;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.ElementalRaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    /// <summary>
    /// Save-free native delivery qualification for the three donor-backed
    /// elemental racial SLAs. Hydraulic Push has its own combat scenario.
    /// </summary>
    internal static class ElementalRaceSlaScenario
    {
        internal const string EvidenceFileName = "elemental-race-slas.json";
        private const string FighterClassGuid =
            "48ac8db94d5de7645906c7d0ad3bcfbd";

        private sealed class CommandEvidence
        {
            public bool Targetable { get; set; }
            public bool Available { get; set; }
            public bool CanStart { get; set; }
            public bool CancelInstalled { get; set; }
            public bool CancelStarted { get; set; }
            public int ResourceBeforeCancel { get; set; }
            public int ResourceAfterCancel { get; set; }
            public int ResourceBeforeCast { get; set; }
            public int ResourceAfterCast { get; set; }
            public bool ActionInvoked { get; set; }
            public string Result { get; set; }
            public bool ProcessPresent { get; set; }
            public bool ProcessEnded { get; set; }
            public bool ProcessDetached { get; set; }
            public int SynchronousNativeEffects { get; set; }
            public bool ArcaneFailureInapplicable { get; set; }
            public bool AvailableAfterCast { get; set; }
            public bool SecondAbilityAvailable { get; set; }
            public bool SecondCommandCanStart { get; set; }
            public int ResourceAfterSecond { get; set; }
            public int ResourceAfterRest { get; set; }

            public bool Pass()
            {
                return Targetable && Available && CanStart && CancelInstalled &&
                    !CancelStarted && ResourceBeforeCancel == 1 &&
                    ResourceAfterCancel == 1 && ResourceBeforeCast == 1 &&
                    ResourceAfterCast == 0 && ActionInvoked &&
                    string.Equals(Result, "Success",
                        StringComparison.Ordinal) && ProcessPresent &&
                    (ProcessEnded || ProcessDetached) &&
                    ArcaneFailureInapplicable && !AvailableAfterCast &&
                    !SecondAbilityAvailable &&
                    ResourceAfterSecond == 0 && ResourceAfterRest == 1;
            }
        }

        private sealed class SlaEvidence
        {
            public string Race { get; set; }
            public string AbilityGuid { get; set; }
            public string AbilityType { get; set; }
            public CommandEvidence Command { get; set; }
            public string DeliveryType { get; set; }
            public string EffectType { get; set; }
            public string NativeBuffGuid { get; set; }
            public string NativeBuffName { get; set; }
            public string NativeBuffComponents { get; set; }
            public double DurationLevelOneSeconds { get; set; }
            public double DurationLevelFiveSeconds { get; set; }
            public bool DurationScaled { get; set; }
            public bool BuffApplied { get; set; }
            public bool BuffExpired { get; set; }
            public string EmptyHandBefore { get; set; }
            public string EmptyHandDuring { get; set; }
            public bool NativeUnarmedContract { get; set; }
            public string ConeLength { get; set; }
            public string ConeWidth { get; set; }
            public string ProjectileGuid { get; set; }
            public bool NativeConeExact { get; set; }
            public int FailedSaveDamage { get; set; }
            public int SuccessfulSaveDamage { get; set; }
            public int FailedSaveModifier { get; set; }
            public int SuccessfulSaveModifier { get; set; }
            public int FailedSaveEvents { get; set; }
            public int SuccessfulSaveEvents { get; set; }
            public int FailedSaveD20 { get; set; }
            public int SuccessfulSaveD20 { get; set; }
            public bool FailedSavePassed { get; set; }
            public bool SuccessfulSavePassed { get; set; }
            public int FailedContextCasterLevel { get; set; }
            public int SuccessfulContextCasterLevel { get; set; }
            public int FailedContextDifficultyClass { get; set; }
            public int SuccessfulContextDifficultyClass { get; set; }
            public string NativeActionGraph { get; set; }
            public bool NativeDamageAndSave { get; set; }

            public string Summary()
            {
                return Race + ":command=" + Command.Result + ";resource=" +
                    Command.ResourceBeforeCancel + "->" +
                    Command.ResourceAfterCancel + "->" +
                    Command.ResourceAfterCast + "->" +
                    Command.ResourceAfterRest + ";delivery=" + DeliveryType +
                    ";effect=" + EffectType + ";buff=" + NativeBuffName +
                    "[" + NativeBuffGuid + "];duration=" +
                    DurationLevelOneSeconds.ToString("F2") + "->" +
                    DurationLevelFiveSeconds.ToString("F2") + ";damage=" +
                    FailedSaveDamage + "/" + SuccessfulSaveDamage +
                    ";reflex=" + FailedSaveModifier + "/" +
                    SuccessfulSaveModifier + ";second=" +
                    Command.SecondAbilityAvailable + "/" +
                    Command.ResourceAfterSecond + ";save=" +
                    FailedSaveEvents + ":" + FailedSaveD20 + ":" +
                    FailedSavePassed + "/" + SuccessfulSaveEvents + ":" +
                    SuccessfulSaveD20 + ":" + SuccessfulSavePassed +
                    ";params=" + FailedContextCasterLevel + ":" +
                    FailedContextDifficultyClass + "/" +
                    SuccessfulContextCasterLevel + ":" +
                    SuccessfulContextDifficultyClass;
            }
        }

        private sealed class SaveObservation
        {
            internal int Count;
            internal int D20;
            internal int Modifier;
            internal int DifficultyClass;
            internal bool Passed;
        }

        [ThreadStatic] private static UnitEntityData _saveTarget;
        [ThreadStatic] private static SaveObservation _saveObservation;

        private sealed class Evidence
        {
            public int SchemaVersion { get; set; }
            public bool SaveStateTouched { get; set; }
            public List<SlaEvidence> Slas { get; set; }
            public bool CleanupExact { get; set; }
        }

        internal static RuntimeTestResult Run(ModContext context,
            RuntimeTestRequest request)
        {
            var assertions = new List<RuntimeTestAssertion>();
            var diagnostics = new List<string>();
            var evidenceFiles = new List<string>();
            var evidence = new Evidence
            {
                SchemaVersion = 1,
                SaveStateTouched = false,
                Slas = new List<SlaEvidence>()
            };
            var created = new List<UnitEntityData>();
            UnitEntityData[] unitsBefore = Game.Instance.State.Units.All
                .ToArray();
            string stage = "resolve-production-contract";
            try
            {
                ElementalRaceBlueprintSet set = BlueprintBootstrap
                    .ElementalRaces;
                BlueprintCharacterClass fighter = BlueprintLibraryLookup
                    .RequireExact<BlueprintCharacterClass>(
                        BlueprintBootstrap.Library, FighterClassGuid,
                        "elemental SLA Fighter fixture");
                stage = "ifrit-burning-hands";
                evidence.Slas.Add(ExerciseIfrit(set.Ifrit, fighter, created));
                stage = "oread-stone-fist";
                evidence.Slas.Add(ExerciseBuff(set.Oread, fighter, created,
                    true));
                stage = "sylph-feather-step";
                evidence.Slas.Add(ExerciseBuff(set.Sylph, fighter, created,
                    false));
            }
            catch (Exception exception)
            {
                diagnostics.Add("stage=" + stage + ";exception=" + exception);
            }
            finally
            {
                foreach (UnitEntityData unit in created.AsEnumerable()
                    .Reverse().ToArray())
                {
                    if (unit == null) continue;
                    unit.Commands.InterruptAll(true);
                    Game.Instance.State.Units.All.Remove(unit);
                    unit.Dispose();
                }
                evidence.CleanupExact = SameReferences(unitsBefore,
                    Game.Instance.State.Units.All.ToArray()) &&
                    created.All(value => value == null ||
                        !Game.Instance.State.Units.All.Contains(value));
            }

            AddAssertions(assertions, evidence);
            string path = Path.Combine(request.EvidenceDirectory,
                EvidenceFileName);
            File.WriteAllText(path, JsonConvert.SerializeObject(evidence,
                Formatting.Indented, EvidenceSettings()));
            evidenceFiles.Add(path);
            diagnostics.Add("elementalSlasSha256=" + Hash(path));
            bool pass = assertions.All(value => value.Status ==
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
                StartUtc = DateTime.UtcNow.ToString("o"),
                EndUtc = string.Empty,
                Assertions = assertions,
                Diagnostics = diagnostics,
                Warnings = new List<string>(),
                ExceptionSummary = string.Empty,
                EvidenceFiles = evidenceFiles,
                AutomaticExitRequested = request.ExitAfterCompletion,
                EvidenceDirectory = request.EvidenceDirectory
            };
        }

        private static SlaEvidence ExerciseIfrit(
            ElementalRaceBlueprints blueprints,
            BlueprintCharacterClass fighter,
            ICollection<UnitEntityData> created)
        {
            UnitEntityData caster = CreateUnit(created, Vector3.zero);
            UnitEntityData failed = CreateUnit(created,
                new Vector3(0f, 0f, 2f));
            UnitEntityData succeeded = CreateUnit(created,
                new Vector3(0f, 0f, 2.5f));
            PrepareRace(caster, blueprints, fighter, 5);
            AbilityData data = RequireAbility(caster, blueprints.SlaAbility);
            var point = new TargetWrapper(new Vector3(0f, 0f, 4f));

            AbilityEffectRunAction effect = blueprints.SlaAbility
                .ComponentsArray.OfType<AbilityEffectRunAction>().Single();
            AbilityDeliverProjectile delivery = blueprints.SlaAbility
                .ComponentsArray.OfType<AbilityDeliverProjectile>().Single();
            BlueprintAbility donor = BlueprintLibraryLookup.RequireExact<
                BlueprintAbility>(BlueprintBootstrap.Library,
                    blueprints.Definition.DonorAbilityGuid,
                    "native Burning Hands delivery control");
            AbilityDeliverProjectile donorDelivery = donor.ComponentsArray
                .OfType<AbilityDeliverProjectile>().Single();
            InstallSavingThrowProbe(failed);
            InstallSavingThrowProbe(succeeded);
            var result = BaseEvidence(blueprints);
            result.DeliveryType = delivery.GetType().FullName;
            result.EffectType = effect.GetType().FullName;
            result.ConeLength = delivery.Length.ToString();
            result.ConeWidth = delivery.LineWidth.ToString();
            result.ProjectileGuid = delivery.Projectiles == null ||
                delivery.Projectiles.Length != 1 ||
                delivery.Projectiles[0] == null ? string.Empty :
                    delivery.Projectiles[0].AssetGuid;
            result.NativeConeExact = delivery.Type == donorDelivery.Type &&
                delivery.Length.Equals(donorDelivery.Length) &&
                delivery.LineWidth.Equals(donorDelivery.LineWidth) &&
                !delivery.NeedAttackRoll &&
                SameBlueprints(delivery.Projectiles,
                    donorDelivery.Projectiles);
            result.NativeActionGraph = RuntimeTestRunner
                .DescribeNestedObject(effect, 10);

            result.Command = ExerciseCommand(caster, data,
                blueprints.SlaResource, point, failed);
            failed.Damage = 0;
            succeeded.Damage = 0;
            SetExactProperty(failed.Descriptor.Stats.GetStat(
                Kingmaker.EntitySystem.Stats.StatType.SaveReflex),
                "BaseValue", -100);
            SetExactProperty(succeeded.Descriptor.Stats.GetStat(
                Kingmaker.EntitySystem.Stats.StatType.SaveReflex),
                "BaseValue", 100);
            result.FailedSaveModifier = failed.Descriptor.Stats.GetStat(
                Kingmaker.EntitySystem.Stats.StatType.SaveReflex).ModifiedValue;
            result.SuccessfulSaveModifier = succeeded.Descriptor.Stats.GetStat(
                Kingmaker.EntitySystem.Stats.StatType.SaveReflex).ModifiedValue;

            int seed = FindNativeD20Seed(10);
            UnityEngine.Random.InitState(seed);
            AbilityExecutionContext failedContext = data.CreateExecutionContext(
                new TargetWrapper(failed));
            result.FailedContextCasterLevel = failedContext.Params.CasterLevel;
            result.FailedContextDifficultyClass = failedContext.Params.DC;
            int failedHpBefore = failed.HPLeft;
            SaveObservation failedSave;
            BeginSaveObservation(failed);
            try
            {
                effect.Apply(failedContext, new TargetWrapper(failed));
            }
            finally
            {
                failedSave = EndSaveObservation();
            }
            result.FailedSaveDamage = failedHpBefore - failed.HPLeft;
            UnityEngine.Random.InitState(seed);
            AbilityExecutionContext successfulContext = data
                .CreateExecutionContext(new TargetWrapper(succeeded));
            result.SuccessfulContextCasterLevel =
                successfulContext.Params.CasterLevel;
            result.SuccessfulContextDifficultyClass =
                successfulContext.Params.DC;
            int successfulHpBefore = succeeded.HPLeft;
            SaveObservation successfulSave;
            BeginSaveObservation(succeeded);
            try
            {
                effect.Apply(successfulContext, new TargetWrapper(succeeded));
            }
            finally
            {
                successfulSave = EndSaveObservation();
            }
            result.SuccessfulSaveDamage = successfulHpBefore -
                succeeded.HPLeft;
            result.FailedSaveEvents = failedSave.Count;
            result.SuccessfulSaveEvents = successfulSave.Count;
            result.FailedSaveD20 = failedSave.D20;
            result.SuccessfulSaveD20 = successfulSave.D20;
            result.FailedSaveModifier = failedSave.Modifier;
            result.SuccessfulSaveModifier = successfulSave.Modifier;
            result.FailedSavePassed = failedSave.Passed;
            result.SuccessfulSavePassed = successfulSave.Passed;
            result.NativeDamageAndSave = effect.HasSavingThrow &&
                effect.SavingThrowType == SavingThrowType.Reflex &&
                result.FailedSaveModifier <= -100 &&
                result.SuccessfulSaveModifier >= 100 &&
                result.FailedContextCasterLevel == 5 &&
                result.SuccessfulContextCasterLevel == 5 &&
                result.FailedContextDifficultyClass ==
                result.SuccessfulContextDifficultyClass &&
                failedSave.DifficultyClass ==
                    result.FailedContextDifficultyClass &&
                successfulSave.DifficultyClass ==
                    result.SuccessfulContextDifficultyClass &&
                result.FailedSaveEvents == 1 &&
                result.SuccessfulSaveEvents == 1 &&
                !result.FailedSavePassed &&
                result.SuccessfulSavePassed &&
                result.FailedSaveD20 == result.SuccessfulSaveD20 &&
                result.FailedSaveDamage > 0 &&
                result.FailedSaveDamage <= 30 &&
                result.SuccessfulSaveDamage == result.FailedSaveDamage / 2 &&
                result.NativeActionGraph.Contains("Energy=Fire") &&
                result.NativeActionGraph.Contains("DiceType=D6") &&
                result.NativeActionGraph.Contains("HalfIfSaved=True");
            failed.Damage = 0;
            succeeded.Damage = 0;
            return result;
        }

        private static SlaEvidence ExerciseBuff(
            ElementalRaceBlueprints blueprints,
            BlueprintCharacterClass fighter,
            ICollection<UnitEntityData> created, bool stoneFist)
        {
            UnitEntityData levelFive = CreateUnit(created,
                new Vector3(1f, 0f, 0f));
            UnitEntityData levelOne = CreateUnit(created,
                new Vector3(2f, 0f, 0f));
            PrepareRace(levelFive, blueprints, fighter, 5);
            PrepareRace(levelOne, blueprints, fighter, 1);
            AbilityData fiveData = RequireAbility(levelFive,
                blueprints.SlaAbility);
            AbilityData oneData = RequireAbility(levelOne,
                blueprints.SlaAbility);
            AbilityEffectRunAction effect = blueprints.SlaAbility
                .ComponentsArray.OfType<AbilityEffectRunAction>().Single();
            ContextActionApplyBuff apply = effect.Actions.Actions
                .OfType<ContextActionApplyBuff>().Single();
            BlueprintBuff expected = apply.Buff;
            if (expected == null)
                throw new InvalidOperationException(
                    blueprints.Definition.SlaName +
                    " has no native buff target.");

            var result = BaseEvidence(blueprints);
            result.DeliveryType = "direct-unit";
            result.EffectType = effect.GetType().FullName;
            result.NativeBuffGuid = expected.AssetGuid;
            result.NativeBuffName = expected.name;
            result.NativeBuffComponents = string.Join(",",
                (expected.ComponentsArray ?? Array.Empty<BlueprintComponent>())
                    .Where(value => value != null)
                    .Select(value => value.GetType().FullName).ToArray());
            result.EmptyHandBefore = WeaponIdentity(
                levelFive.Body.EmptyHandWeapon);
            result.Command = ExerciseCommand(levelFive, fiveData,
                blueprints.SlaResource, new TargetWrapper(levelFive),
                levelFive);
            Buff fiveBuff = ExactBuff(levelFive, expected);
            result.BuffApplied = fiveBuff != null;
            result.DurationLevelFiveSeconds = fiveBuff == null ? -1d :
                fiveBuff.TimeLeft.TotalSeconds;
            result.EmptyHandDuring = WeaponIdentity(
                levelFive.Body.EmptyHandWeapon);
            result.NativeUnarmedContract = !stoneFist ||
                !string.IsNullOrEmpty(result.NativeBuffComponents) &&
                (result.NativeBuffComponents.IndexOf("AdditionalLimb",
                    StringComparison.OrdinalIgnoreCase) >= 0 ||
                 result.NativeBuffComponents.IndexOf("Unarmed",
                    StringComparison.OrdinalIgnoreCase) >= 0 ||
                 !string.Equals(result.EmptyHandBefore,
                    result.EmptyHandDuring, StringComparison.Ordinal));

            effect.Apply(oneData.CreateExecutionContext(
                new TargetWrapper(levelOne)), new TargetWrapper(levelOne));
            Buff oneBuff = ExactBuff(levelOne, expected);
            result.DurationLevelOneSeconds = oneBuff == null ? -1d :
                oneBuff.TimeLeft.TotalSeconds;
            result.DurationScaled = result.DurationLevelOneSeconds > 0d &&
                result.DurationLevelFiveSeconds >
                    result.DurationLevelOneSeconds &&
                Math.Abs(result.DurationLevelFiveSeconds -
                    (result.DurationLevelOneSeconds * 5d)) <= 2d;

            if (fiveBuff != null)
            {
                SetPrivateNullableTimeSpan(fiveBuff, "m_EndTime",
                    Game.Instance.TimeController.GameTime -
                        TimeSpan.FromSeconds(1d));
                levelFive.Descriptor.Buffs.UpdateNextEvent();
                levelFive.Descriptor.Buffs.Tick();
            }
            result.BuffExpired = ExactBuff(levelFive, expected) == null;
            if (oneBuff != null) oneBuff.Remove();
            return result;
        }

        private static SlaEvidence BaseEvidence(
            ElementalRaceBlueprints blueprints)
        {
            return new SlaEvidence
            {
                Race = blueprints.Definition.DisplayName,
                AbilityGuid = blueprints.SlaAbility.AssetGuid,
                AbilityType = blueprints.SlaAbility.Type.ToString(),
                NativeBuffGuid = string.Empty,
                NativeBuffName = string.Empty,
                NativeBuffComponents = string.Empty,
                EmptyHandBefore = string.Empty,
                EmptyHandDuring = string.Empty,
                ConeLength = string.Empty,
                ConeWidth = string.Empty,
                ProjectileGuid = string.Empty
            };
        }

        private static CommandEvidence ExerciseCommand(
            UnitEntityData caster, AbilityData data,
            BlueprintAbilityResource resource, TargetWrapper target,
            UnitEntityData synchronousEffectTarget)
        {
            var result = new CommandEvidence
            {
                Targetable = data.CanTarget(target),
                Available = data.IsAvailable,
                ArcaneFailureInapplicable =
                    !data.IsAffectedByArcaneSpellFailure
            };

            UnitUseAbility canceled = CreateCommand(data, target, caster);
            result.CanStart = canceled.CanStart;
            result.ResourceBeforeCancel = caster.Descriptor.Resources
                .GetResourceAmount(resource);
            caster.Commands.Run(canceled);
            result.CancelInstalled = caster.Commands.Contains(canceled);
            result.CancelStarted = canceled.IsStarted;
            caster.Commands.InterruptAll(true);
            caster.Commands.RemoveFinishedAndUpdateQueue();
            result.ResourceAfterCancel = caster.Descriptor.Resources
                .GetResourceAmount(resource);

            UnitUseAbility command = CreateCommand(data, target, caster);
            result.ResourceBeforeCast = caster.Descriptor.Resources
                .GetResourceAmount(resource);
            object actionResult = InvokeCommandAction(command);
            result.ActionInvoked = true;
            result.Result = actionResult == null ? string.Empty :
                actionResult.ToString();
            result.ResourceAfterCast = caster.Descriptor.Resources
                .GetResourceAmount(resource);
            AbilityExecutionProcess process = command.ExecutionProcess;
            result.ProcessPresent = process != null;
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
                if (!process.IsEnded && synchronousEffectTarget != null)
                {
                    foreach (AbilityEffectRunAction effect in data.Blueprint
                        .ComponentsArray.OfType<AbilityEffectRunAction>())
                    {
                        effect.Apply(process.Context,
                            new TargetWrapper(synchronousEffectTarget));
                        result.SynchronousNativeEffects++;
                    }
                    process.Detach();
                    result.ProcessDetached = true;
                }
                result.ProcessEnded = process.IsEnded;
            }
            InvokeCommandEnded(command, false);

            result.AvailableAfterCast = data.IsAvailable;
            AbilityData secondData = RequireAbility(caster, data.Blueprint);
            result.SecondAbilityAvailable = secondData.IsAvailable;
            UnitUseAbility second = CreateCommand(secondData, target, caster);
            result.SecondCommandCanStart = second.CanStart;
            result.ResourceAfterSecond = caster.Descriptor.Resources
                .GetResourceAmount(resource);
            InvokeCommandEnded(second, true);
            Kingmaker.Controllers.Rest.RestController.ApplyRest(
                caster.Descriptor);
            result.ResourceAfterRest = caster.Descriptor.Resources
                .GetResourceAmount(resource);
            return result;
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

        private static void PrepareRace(UnitEntityData unit,
            ElementalRaceBlueprints blueprints,
            BlueprintCharacterClass fighter, int level)
        {
            UnitDescriptor owner = unit.Descriptor;
            owner.Stats.Strength.BaseValue = 10;
            owner.Stats.Dexterity.BaseValue = 10;
            owner.Stats.Constitution.BaseValue = 10;
            owner.Stats.Intelligence.BaseValue = 12;
            owner.Stats.Wisdom.BaseValue = 14;
            owner.Stats.Charisma.BaseValue = 18;
            EnsureFact(owner, blueprints.Race);
            foreach (BlueprintFeature feature in blueprints.Race.Features)
                EnsureFact(owner, feature);
            Advance(owner, fighter, level);
        }

        private static AbilityData RequireAbility(UnitEntityData unit,
            BlueprintAbility blueprint)
        {
            Ability ability = unit.Descriptor.Abilities.GetAbility(blueprint);
            if (ability == null)
                throw new InvalidOperationException(
                    "Disposable unit did not receive " + blueprint.name +
                    ".");
            return new AbilityData(ability);
        }

        private static UnitEntityData CreateUnit(
            ICollection<UnitEntityData> created, Vector3 position)
        {
            UnitEntityData result = new Kingmaker.UI.LevelUp.ChargenUnit(
                BlueprintRoot.Instance.DefaultPlayerCharacter).Unit;
            if (result == null || result.Descriptor == null)
                throw new InvalidOperationException(
                    "A disposable elemental SLA unit was unavailable.");
            result.Descriptor.Stats.HitPoints.BaseValue = 100;
            result.Descriptor.State.Immortality.Retain();
            SetExactProperty(result, "Position", position);
            if (!Game.Instance.State.Units.All.Add(result))
            {
                result.Dispose();
                throw new InvalidOperationException(
                    "A disposable elemental SLA unit could not be registered.");
            }
            created.Add(result);
            return result;
        }

        private static void EnsureFact(UnitDescriptor owner,
            BlueprintUnitFact blueprint)
        {
            if (owner.HasFact(blueprint)) return;
            if (owner.AddFact(blueprint) == null || !owner.HasFact(blueprint))
                throw new InvalidOperationException(
                    "Disposable unit rejected fact " + blueprint.AssetGuid +
                    ".");
        }

        private static void Advance(UnitDescriptor owner,
            BlueprintCharacterClass characterClass, int levels)
        {
            Type type = typeof(LevelUpController);
            MethodInfo start = type.GetMethods(BindingFlags.Public |
                BindingFlags.NonPublic | BindingFlags.Static).Single(value =>
                    value.Name == "StartWithoutAssigningStaticInstance" &&
                    value.GetParameters().Length == 5);
            MethodInfo select = type.GetMethod("SelectClass",
                BindingFlags.Public | BindingFlags.Instance, null,
                new[] { typeof(BlueprintCharacterClass), typeof(bool) }, null);
            MethodInfo mechanics = type.GetMethod("ApplyClassMechanics",
                BindingFlags.Public | BindingFlags.Instance);
            MethodInfo apply = type.GetMethod("ApplyLevelup",
                BindingFlags.Public | BindingFlags.NonPublic |
                    BindingFlags.Instance);
            MethodInfo cancel = type.GetMethod("Cancel", BindingFlags.Public |
                BindingFlags.Instance);
            if (select == null || mechanics == null || apply == null ||
                cancel == null)
                throw new MissingMethodException(
                    "The native elemental SLA level-up surface is unavailable.");
            object charGen = Enum.Parse(start.GetParameters()[4]
                .ParameterType, "CharGen", false);
            object controller = null;
            try
            {
                for (int index = 0; index < levels; index++)
                {
                    controller = start.Invoke(null, new object[]
                    {
                        owner, false, null, null, charGen
                    });
                    if (!(bool)select.Invoke(controller,
                            new object[] { characterClass, false }))
                        throw new InvalidOperationException(
                            "Disposable SLA class selection failed at level " +
                            (index + 1) + ".");
                    mechanics.Invoke(controller, null);
                    apply.Invoke(controller, new object[] { owner });
                    cancel.Invoke(controller, null);
                    controller = null;
                }
            }
            finally
            {
                if (controller != null) cancel.Invoke(controller, null);
            }
        }

        private static Buff ExactBuff(UnitEntityData unit,
            BlueprintBuff blueprint)
        {
            return unit.Descriptor.Buffs.RawFacts.OfType<Buff>()
                .SingleOrDefault(value =>
                    ReferenceEquals(value.Blueprint, blueprint));
        }

        private static void InstallSavingThrowProbe(UnitEntityData unit)
        {
            var feature = ScriptableObject.CreateInstance<BlueprintFeature>();
            feature.name = "KMG_Runtime_ElementalSlaSavingThrowProbe_" +
                unit.UniqueId;
            feature.Ranks = 1;
            feature.IsClassFeature = false;
            feature.HideInUI = true;
            feature.Groups = Array.Empty<FeatureGroup>();
            feature.ComponentsArray = new BlueprintComponent[]
            {
                ScriptableObject.CreateInstance<
                    ElementalRaceSlaSavingThrowProbe>()
            };
            EnsureFact(unit.Descriptor, feature);
        }

        private static void BeginSaveObservation(UnitEntityData target)
        {
            if (_saveTarget != null || _saveObservation != null)
                throw new InvalidOperationException(
                    "A prior elemental SLA saving-throw observation is active.");
            _saveTarget = target ?? throw new ArgumentNullException("target");
            _saveObservation = new SaveObservation();
        }

        private static SaveObservation EndSaveObservation()
        {
            SaveObservation result = _saveObservation ??
                new SaveObservation();
            _saveObservation = null;
            _saveTarget = null;
            return result;
        }

        internal static void RecordSavingThrow(RuleSavingThrow rule)
        {
            if (rule == null || _saveTarget == null ||
                _saveObservation == null ||
                !ReferenceEquals(rule.Initiator, _saveTarget)) return;
            _saveObservation.Count++;
            _saveObservation.D20 = rule.D20.Value;
            _saveObservation.Modifier = rule.StatValue;
            _saveObservation.DifficultyClass = rule.DifficultyClass;
            _saveObservation.Passed = rule.IsPassed;
        }

        private static string WeaponIdentity(ItemEntityWeapon weapon)
        {
            return weapon == null || weapon.Blueprint == null ?
                string.Empty : weapon.Blueprint.name + "[" +
                    weapon.Blueprint.AssetGuid + "]";
        }

        private static bool SameBlueprints<T>(IList<T> expected,
            IList<T> actual) where T : BlueprintScriptableObject
        {
            if (expected == null || actual == null ||
                expected.Count != actual.Count) return false;
            for (int index = 0; index < expected.Count; index++)
            {
                if (expected[index] == null || actual[index] == null ||
                    !string.Equals(expected[index].AssetGuid,
                        actual[index].AssetGuid, StringComparison.Ordinal))
                    return false;
            }
            return true;
        }

        private static int FindNativeD20Seed(int expected)
        {
            for (int seed = 0; seed < 100000; seed++)
            {
                UnityEngine.Random.InitState(seed);
                if (UnityEngine.Random.Range(1, 21) == expected)
                    return seed;
            }
            throw new InvalidOperationException(
                "No deterministic Unity d20 seed produced " + expected +
                ".");
        }

        private static void SetPrivateNullableTimeSpan(object owner,
            string fieldName, TimeSpan value)
        {
            FieldInfo field = owner == null ? null : owner.GetType().GetField(
                fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            if (field == null || field.FieldType != typeof(TimeSpan?))
                throw new MissingFieldException(owner == null ? string.Empty :
                    owner.GetType().FullName, fieldName);
            field.SetValue(owner, (TimeSpan?)value);
        }

        private static void SetExactProperty(object value, string name,
            object propertyValue)
        {
            PropertyInfo property = value.GetType().GetProperty(name,
                BindingFlags.Public | BindingFlags.NonPublic |
                    BindingFlags.Instance);
            MethodInfo setter = property == null ? null :
                property.GetSetMethod(true);
            if (setter == null)
                throw new MissingMemberException(value.GetType().FullName,
                    name);
            setter.Invoke(value, new[] { propertyValue });
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

        private static void AddAssertions(
            ICollection<RuntimeTestAssertion> assertions, Evidence evidence)
        {
            Add(assertions, "elemental-sla-count", "3",
                evidence.Slas.Count.ToString(), evidence.Slas.Count == 3,
                "three donor-backed production racial SLAs");
            foreach (SlaEvidence sla in evidence.Slas)
            {
                string key = sla.Race.ToLowerInvariant();
                string observed = sla.Summary();
                Add(assertions, "elemental-" + key + "-native-command",
                    "queued cancel spends zero; exact OnAction spends one; " +
                    "fresh AbilityData unavailable at zero resource; ordinary rest restores one",
                    observed, sla.AbilityType == AbilityType.SpellLike
                        .ToString() && sla.Command != null &&
                        sla.Command.Pass(),
                    "native UnitUseAbility OnAction, AbilityData.Spend, " +
                    "AbilityExecutionProcess, and ApplyRest");
                if (sla.Race == "Ifrit")
                {
                    Add(assertions, "elemental-ifrit-native-cone",
                        "exact native Burning Hands cone and projectile",
                        observed, sla.NativeConeExact,
                        "production and native AbilityDeliverProjectile fields");
                    Add(assertions, "elemental-ifrit-native-damage-save",
                        "5d6 Fire at CL5; Reflex success halves identical roll",
                        observed, sla.NativeDamageAndSave,
                        "actual cloned AbilityEffectRunAction and native save/damage rules");
                }
                else
                {
                    Add(assertions, "elemental-" + key + "-native-buff",
                        "exact native buff applied for a positive duration",
                        observed, sla.BuffApplied &&
                            sla.DurationLevelFiveSeconds > 0d &&
                            !string.IsNullOrEmpty(sla.NativeBuffGuid) &&
                            !string.IsNullOrEmpty(sla.NativeBuffComponents),
                        "actual UnitUseAbility execution and referenced BlueprintBuff");
                    Add(assertions, "elemental-" + key + "-duration-expiry",
                        "total-level duration scales 1x to 5x and expires natively",
                        observed, sla.DurationScaled && sla.BuffExpired,
                        "actual level-1/level-5 contexts and BuffCollection Tick");
                    if (sla.Race == "Oread")
                        Add(assertions, "elemental-oread-stone-fist-unarmed",
                            "native Stone Fist unarmed weapon contract",
                            observed, sla.NativeUnarmedContract,
                            "applied native buff components and empty-hand state");
                }
            }
            Add(assertions, "elemental-sla-save-free",
                "no save or player-party mutation", "saveStateTouched=" +
                evidence.SaveStateTouched, !evidence.SaveStateTouched,
                "request-local registered disposable units only");
            Add(assertions, "elemental-sla-cleanup",
                "exact pre-run global-unit reference sequence",
                "cleanupExact=" + evidence.CleanupExact,
                evidence.CleanupExact,
                "finally interruption, removal, disposal, and exact comparison");
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

        private static JsonSerializerSettings EvidenceSettings()
        {
            return new JsonSerializerSettings
            {
                ContractResolver = new DefaultContractResolver(),
                PreserveReferencesHandling = PreserveReferencesHandling.None,
                ReferenceLoopHandling = ReferenceLoopHandling.Error
            };
        }

        private static string Hash(string path)
        {
            using (SHA256 sha = SHA256.Create())
            using (FileStream stream = File.OpenRead(path))
                return BitConverter.ToString(sha.ComputeHash(stream))
                    .Replace("-", string.Empty).ToLowerInvariant();
        }

        private static string Metadata(Assembly assembly, string key)
        {
            AssemblyMetadataAttribute value = assembly.GetCustomAttributes(
                typeof(AssemblyMetadataAttribute), false)
                .Cast<AssemblyMetadataAttribute>().FirstOrDefault(item =>
                    string.Equals(item.Key, key,
                        StringComparison.Ordinal));
            return value == null ? string.Empty : value.Value;
        }
    }

    /// <summary>
    /// Development-only component installed on request-local disposable units.
    /// It is never registered or attached to a production blueprint.
    /// </summary>
    public sealed class ElementalRaceSlaSavingThrowProbe :
        RuleInitiatorLogicComponent<RuleSavingThrow>
    {
        public override void OnEventAboutToTrigger(RuleSavingThrow evt) { }

        public override void OnEventDidTrigger(RuleSavingThrow evt)
        {
            ElementalRaceSlaScenario.RecordSavingThrow(evt);
        }
    }
}
