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
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
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
    /// Save-free, request-local qualification of the production Undine
    /// Hydraulic Push ability against Kingmaker's ordinary Bull Rush rule.
    /// </summary>
    internal static class HydraulicPushScenario
    {
        internal const string EvidenceFileName = "hydraulic-push.json";
        private const string FighterClassGuid =
            "48ac8db94d5de7645906c7d0ad3bcfbd";
        private const string WizardClassGuid =
            "ba34257984f4c41408ce1dc2004e342e";

        private sealed class ManeuverObservation
        {
            internal int ManeuverEvents;
            internal int AttackEvents;
            internal int SavingThrowEvents;
            internal string Type = string.Empty;
            internal int? ReplaceAttackBonus;
            internal string ReplaceBaseStat = string.Empty;
            internal int InitiatorCmb;
            internal int TargetCmd;
            internal int D20;
            internal int ManeuverValue;
            internal bool Success;
            internal bool AutoFailure;
        }

        private sealed class CaseEvidence
        {
            public string Name { get; set; }
            public int CharacterLevel { get; set; }
            public int FighterLevel { get; set; }
            public int WizardLevel { get; set; }
            public int IntelligenceModifier { get; set; }
            public int WisdomModifier { get; set; }
            public int CharismaModifier { get; set; }
            public int BestMentalModifier { get; set; }
            public int ExpectedManeuverBonus { get; set; }
            public string SelectedMentalStat { get; set; }
            public bool HostileBothWays { get; set; }
            public bool Targetable { get; set; }
            public int ContextCasterLevel { get; set; }
            public int ManeuverEvents { get; set; }
            public string ManeuverType { get; set; }
            public int? ReplaceAttackBonus { get; set; }
            public string ReplaceBaseStat { get; set; }
            public int InitiatorCmb { get; set; }
            public int TargetCmd { get; set; }
            public int D20 { get; set; }
            public int ManeuverValue { get; set; }
            public bool Success { get; set; }
            public bool AutoFailure { get; set; }
            public bool ImmunityInstalled { get; set; }
            public int AttackEvents { get; set; }
            public int SavingThrowEvents { get; set; }
            public int OpportunityCommandsBefore { get; set; }
            public int OpportunityCommandsAfter { get; set; }
            public string TargetPositionBefore { get; set; }
            public string TargetPositionAfter { get; set; }

            public string Summary()
            {
                return Name + ":level=" + CharacterLevel + "(" +
                    FighterLevel + "/" + WizardLevel + ");mental=" +
                    IntelligenceModifier + "/" + WisdomModifier + "/" +
                    CharismaModifier + ";selected=" + ReplaceBaseStat +
                    ";expected=" + ExpectedManeuverBonus + ";cmb=" +
                    InitiatorCmb + ";cmd=" + TargetCmd + ";d20=" + D20 +
                    ";value=" + ManeuverValue + ";success=" + Success +
                    ";immune=" + ImmunityInstalled + ";events=" +
                    ManeuverEvents + "/" + AttackEvents + "/" +
                    SavingThrowEvents + ";aoo=" + OpportunityCommandsBefore +
                    "->" + OpportunityCommandsAfter;
            }
        }

        private sealed class CommandEvidence
        {
            public bool HostileBothWays { get; set; }
            public bool Targetable { get; set; }
            public bool Available { get; set; }
            public bool CanStart { get; set; }
            public bool CancelInstalled { get; set; }
            public bool CancelStarted { get; set; }
            public int ResourceBeforeCancel { get; set; }
            public int ResourceAfterCancel { get; set; }
            public int ResourceBeforeCommit { get; set; }
            public int ResourceAfterCommit { get; set; }
            public string Result { get; set; }
            public bool ProcessPresent { get; set; }
            public bool ProcessEnded { get; set; }
            public bool ProcessDetached { get; set; }
            public int SynchronousNativeEffects { get; set; }
            public int ManeuverEvents { get; set; }
            public int AttackEvents { get; set; }
            public int SavingThrowEvents { get; set; }
            public int OpportunityCommandsBefore { get; set; }
            public int OpportunityCommandsAfter { get; set; }
            public bool AvailableAfterCommit { get; set; }
            public bool FreshAbilityAvailable { get; set; }
            public int ResourceAfterSecondGate { get; set; }
            public int ResourceAfterRest { get; set; }

            public bool Pass()
            {
                return HostileBothWays && Targetable && Available && CanStart &&
                    CancelInstalled && !CancelStarted &&
                    ResourceBeforeCancel == 1 && ResourceAfterCancel == 1 &&
                    ResourceBeforeCommit == 1 && ResourceAfterCommit == 0 &&
                    string.Equals(Result, "Success",
                        StringComparison.Ordinal) && ProcessPresent &&
                    (ProcessEnded || ProcessDetached) && ManeuverEvents == 1 &&
                    AttackEvents == 0 && SavingThrowEvents == 0 &&
                    OpportunityCommandsBefore == OpportunityCommandsAfter &&
                    !AvailableAfterCommit && !FreshAbilityAvailable &&
                    ResourceAfterSecondGate == 0 && ResourceAfterRest == 1;
            }
        }

        private sealed class EngineEvidence
        {
            public string ActionIl { get; set; }
            public string ManeuverIl { get; set; }
            public bool ReplacesAttackBonus { get; set; }
            public bool ReplacesBaseStat { get; set; }
            public bool UsesNativeForceMovePush { get; set; }
            public bool NoAttackSaveOrOpportunityConstruction { get; set; }
        }

        private sealed class Evidence
        {
            public int SchemaVersion { get; set; }
            public bool SaveStateTouched { get; set; }
            public string AbilityGuid { get; set; }
            public string AbilityType { get; set; }
            public string Range { get; set; }
            public bool SpellResistance { get; set; }
            public string SavingThrow { get; set; }
            public bool ComponentContract { get; set; }
            public List<CaseEvidence> Cases { get; set; }
            public CommandEvidence Command { get; set; }
            public EngineEvidence Engine { get; set; }
            public bool CleanupExact { get; set; }
            public bool TransientObjectsDestroyed { get; set; }
        }

        [ThreadStatic] private static UnitEntityData _activeCaster;
        [ThreadStatic] private static UnitEntityData _activeTarget;
        [ThreadStatic] private static ManeuverObservation _observation;

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
                Cases = new List<CaseEvidence>()
            };
            UnitEntityData[] unitsBefore = Game.Instance.State.Units.All
                .ToArray();
            var created = new List<UnitEntityData>();
            var transient = new List<UnityEngine.Object>();
            string stage = "resolve-production-contract";
            try
            {
                ElementalRaceBlueprints undine = BlueprintBootstrap
                    .ElementalRaces.Undine;
                BlueprintCharacterClass fighter = BlueprintLibraryLookup
                    .RequireExact<BlueprintCharacterClass>(
                        BlueprintBootstrap.Library, FighterClassGuid,
                        "Hydraulic Push Fighter fixture");
                BlueprintCharacterClass wizard = BlueprintLibraryLookup
                    .RequireExact<BlueprintCharacterClass>(
                        BlueprintBootstrap.Library, WizardClassGuid,
                        "Hydraulic Push Wizard fixture");
                BlueprintAbility ability = undine.SlaAbility;
                AbilityEffectRunAction effect = ability.ComponentsArray
                    .OfType<AbilityEffectRunAction>().Single();
                ContextActionCombatManeuver maneuver = effect.Actions.Actions
                    .OfType<ContextActionCombatManeuver>().Single();
                ElementalHydraulicResourceCommit spend = effect.Actions.Actions
                    .OfType<ElementalHydraulicResourceCommit>().Single();
                evidence.AbilityGuid = ability.AssetGuid;
                evidence.AbilityType = ability.Type.ToString();
                evidence.Range = ability.Range.ToString();
                evidence.SpellResistance = ability.SpellResistance;
                evidence.SavingThrow = effect.SavingThrowType.ToString();
                evidence.ComponentContract =
                    ability.Type == AbilityType.SpellLike &&
                    ability.Range == AbilityRange.Close &&
                    ability.CanTargetEnemies && !ability.CanTargetSelf &&
                    !ability.CanTargetFriends && !ability.CanTargetPoint &&
                    ability.SpellResistance && !effect.HasSavingThrow &&
                    effect.SavingThrowType == SavingThrowType.Unknown &&
                    ReferenceEquals(spend.Resource, undine.SlaResource) &&
                    effect.Actions.Actions.Length == 2 &&
                    ReferenceEquals(effect.Actions.Actions[0], spend) &&
                    ability.ComponentsArray.OfType<AbilityResourceLogic>()
                        .Single().IsSpendResource &&
                    maneuver.Type == CombatManeuver.BullRush &&
                    maneuver.ReplaceStat &&
                    maneuver.UseCasterLevelAsBaseAttack &&
                    maneuver.UseBestMentalStat;

                BlueprintFaction actorFaction;
                BlueprintFaction targetFaction;
                CreateFactionPair(transient, out actorFaction,
                    out targetFaction);

                stage = "intelligence-positive";
                evidence.Cases.Add(ExerciseCase("intelligence-positive",
                    undine, fighter, wizard, effect, actorFaction,
                    targetFaction, created, transient, 18, 10, 10, 5, 0,
                    false, false));
                stage = "wisdom-positive";
                evidence.Cases.Add(ExerciseCase("wisdom-positive", undine,
                    fighter, wizard, effect, actorFaction, targetFaction,
                    created, transient, 10, 16, 10, 5, 0, false, false));
                stage = "charisma-positive";
                evidence.Cases.Add(ExerciseCase("charisma-positive", undine,
                    fighter, wizard, effect, actorFaction, targetFaction,
                    created, transient, 10, 10, 18, 5, 0, false, false));
                stage = "all-negative";
                evidence.Cases.Add(ExerciseCase("all-negative", undine,
                    fighter, wizard, effect, actorFaction, targetFaction,
                    created, transient, 6, 4, 8, 5, 0, false, false));
                stage = "tie-a";
                evidence.Cases.Add(ExerciseCase("tie-a", undine, fighter,
                    wizard, effect, actorFaction, targetFaction, created,
                    transient, 18, 16, 18, 5, 0, false, false));
                stage = "tie-b";
                evidence.Cases.Add(ExerciseCase("tie-b", undine, fighter,
                    wizard, effect, actorFaction, targetFaction, created,
                    transient, 18, 16, 18, 5, 0, false, false));
                stage = "multiclass";
                evidence.Cases.Add(ExerciseCase("multiclass", undine,
                    fighter, wizard, effect, actorFaction, targetFaction,
                    created, transient, 18, 10, 10, 2, 3, false, false));
                stage = "ordinary-failure";
                evidence.Cases.Add(ExerciseCase("ordinary-failure", undine,
                    fighter, wizard, effect, actorFaction, targetFaction,
                    created, transient, 18, 10, 10, 5, 0, true, false));
                stage = "maneuver-immunity";
                evidence.Cases.Add(ExerciseCase("maneuver-immunity", undine,
                    fighter, wizard, effect, actorFaction, targetFaction,
                    created, transient, 18, 10, 10, 5, 0, false, true));

                stage = "command-lifecycle";
                evidence.Command = ExerciseCommand(undine, fighter, effect,
                    actorFaction, targetFaction, created, transient);
                stage = "native-il-contract";
                evidence.Engine = InspectEngineContract();
            }
            catch (Exception exception)
            {
                diagnostics.Add("stage=" + stage + ";exception=" + exception);
            }
            finally
            {
                EndObservation();
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
                foreach (UnityEngine.Object value in transient.AsEnumerable()
                    .Reverse().ToArray())
                    if (value != null) UnityEngine.Object.DestroyImmediate(value);
                evidence.TransientObjectsDestroyed = transient.All(value =>
                    value == null);
            }

            AddAssertions(assertions, evidence);
            string path = Path.Combine(request.EvidenceDirectory,
                EvidenceFileName);
            File.WriteAllText(path, JsonConvert.SerializeObject(evidence,
                Formatting.Indented, EvidenceSettings()));
            evidenceFiles.Add(path);
            diagnostics.Add("hydraulicPushSha256=" + Hash(path));
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

        private static CaseEvidence ExerciseCase(string name,
            ElementalRaceBlueprints undine,
            BlueprintCharacterClass fighter,
            BlueprintCharacterClass wizard, AbilityEffectRunAction effect,
            BlueprintFaction actorFaction, BlueprintFaction targetFaction,
            ICollection<UnitEntityData> created,
            ICollection<UnityEngine.Object> transient, int intelligence,
            int wisdom, int charisma, int fighterLevels, int wizardLevels,
            bool hardTarget, bool immune)
        {
            UnitEntityData caster = CreateUnit(actorFaction, created,
                transient, new Vector3(created.Count * 0.25f, 0f, 0f),
                "Caster_" + name);
            UnitEntityData target = CreateUnit(targetFaction, created,
                transient, caster.Position + new Vector3(0f, 0f, 2f),
                "Target_" + name);
            PrepareCaster(caster, undine, fighter, wizard, intelligence,
                wisdom, charisma, fighterLevels, wizardLevels);
            target.Descriptor.Stats.Strength.BaseValue = hardTarget ? 100 : 1;
            target.Descriptor.Stats.Dexterity.BaseValue = hardTarget ? 100 : 1;
            target.Descriptor.Stats.Constitution.BaseValue = 10;
            target.Descriptor.Stats.HitPoints.BaseValue = 100;
            InstallProbe(caster);
            InstallProbe(target);
            if (immune)
                target.Descriptor.State.AddCondition(
                    UnitCondition.ImmuneToCombatManeuvers, null);

            AbilityData data = RequireAbility(caster, undine.SlaAbility);
            var wrapped = new TargetWrapper(target);
            AbilityExecutionContext execution = data.CreateExecutionContext(
                wrapped);
            int intelligenceModifier = caster.Descriptor.Stats.Intelligence
                .Bonus;
            int wisdomModifier = caster.Descriptor.Stats.Wisdom.Bonus;
            int charismaModifier = caster.Descriptor.Stats.Charisma.Bonus;
            int best = Math.Max(intelligenceModifier,
                Math.Max(wisdomModifier, charismaModifier));
            var result = new CaseEvidence
            {
                Name = name,
                CharacterLevel = caster.Descriptor.Progression
                    .CharacterLevel,
                FighterLevel = caster.Descriptor.Progression.GetClassLevel(
                    fighter),
                WizardLevel = caster.Descriptor.Progression.GetClassLevel(
                    wizard),
                IntelligenceModifier = intelligenceModifier,
                WisdomModifier = wisdomModifier,
                CharismaModifier = charismaModifier,
                BestMentalModifier = best,
                ExpectedManeuverBonus = caster.Descriptor.Progression
                    .CharacterLevel + best,
                SelectedMentalStat = ExpectedBestMentalStat(caster),
                HostileBothWays = caster.IsEnemy(target) &&
                    target.IsEnemy(caster),
                Targetable = data.CanTarget(wrapped),
                ContextCasterLevel = execution.Params.CasterLevel,
                ImmunityInstalled = target.Descriptor.State.HasCondition(
                    UnitCondition.ImmuneToCombatManeuvers),
                OpportunityCommandsBefore = CountOpportunityCommands(),
                TargetPositionBefore = target.Position.ToString("R")
            };

            ManeuverObservation observed;
            BeginObservation(caster, target);
            try
            {
                UnityEngine.Random.InitState(FindNativeD20Seed(10));
                effect.Apply(execution, wrapped);
            }
            finally
            {
                observed = EndObservation();
            }
            result.ManeuverEvents = observed.ManeuverEvents;
            result.ManeuverType = observed.Type;
            result.ReplaceAttackBonus = observed.ReplaceAttackBonus;
            result.ReplaceBaseStat = observed.ReplaceBaseStat;
            result.InitiatorCmb = observed.InitiatorCmb;
            result.TargetCmd = observed.TargetCmd;
            result.D20 = observed.D20;
            result.ManeuverValue = observed.ManeuverValue;
            result.Success = observed.Success;
            result.AutoFailure = observed.AutoFailure;
            result.AttackEvents = observed.AttackEvents;
            result.SavingThrowEvents = observed.SavingThrowEvents;
            result.OpportunityCommandsAfter = CountOpportunityCommands();
            result.TargetPositionAfter = target.Position.ToString("R");
            if (immune)
                target.Descriptor.State.RemoveCondition(
                    UnitCondition.ImmuneToCombatManeuvers);
            return result;
        }

        private static CommandEvidence ExerciseCommand(
            ElementalRaceBlueprints undine,
            BlueprintCharacterClass fighter, AbilityEffectRunAction effect,
            BlueprintFaction actorFaction, BlueprintFaction targetFaction,
            ICollection<UnitEntityData> created,
            ICollection<UnityEngine.Object> transient)
        {
            UnitEntityData caster = CreateUnit(actorFaction, created,
                transient, new Vector3(8f, 0f, 0f), "CommandCaster");
            UnitEntityData target = CreateUnit(targetFaction, created,
                transient, new Vector3(8f, 0f, 2f), "CommandTarget");
            PrepareCaster(caster, undine, fighter, null, 18, 10, 10, 5, 0);
            target.Descriptor.Stats.Strength.BaseValue = 1;
            target.Descriptor.Stats.Dexterity.BaseValue = 1;
            InstallProbe(caster);
            InstallProbe(target);
            AbilityData data = RequireAbility(caster, undine.SlaAbility);
            var wrapped = new TargetWrapper(target);
            var result = new CommandEvidence
            {
                HostileBothWays = caster.IsEnemy(target) &&
                    target.IsEnemy(caster),
                Targetable = data.CanTarget(wrapped),
                Available = data.IsAvailable,
                OpportunityCommandsBefore = CountOpportunityCommands()
            };

            UnitUseAbility canceled = CreateCommand(data, wrapped, caster);
            result.CanStart = canceled.CanStart;
            result.ResourceBeforeCancel = caster.Descriptor.Resources
                .GetResourceAmount(undine.SlaResource);
            caster.Commands.Run(canceled);
            result.CancelInstalled = caster.Commands.Contains(canceled);
            result.CancelStarted = canceled.IsStarted;
            caster.Commands.InterruptAll(true);
            caster.Commands.RemoveFinishedAndUpdateQueue();
            result.ResourceAfterCancel = caster.Descriptor.Resources
                .GetResourceAmount(undine.SlaResource);

            UnitUseAbility command = CreateCommand(data, wrapped, caster);
            result.ResourceBeforeCommit = caster.Descriptor.Resources
                .GetResourceAmount(undine.SlaResource);
            ManeuverObservation observed;
            BeginObservation(caster, target);
            try
            {
                UnityEngine.Random.InitState(FindNativeD20Seed(10));
                object action = InvokeCommandAction(command);
                result.Result = action == null ? string.Empty :
                    action.ToString();
                AbilityExecutionProcess process = command.ExecutionProcess;
                result.ProcessPresent = process != null;
                if (process != null)
                {
                    for (int tick = 0; tick < 5000 && !process.IsEnded;
                        tick++) process.Tick();
                    if (!process.IsEnded)
                    {
                        process.InstantDeliver();
                        for (int tick = 0; tick < 5000 && !process.IsEnded;
                            tick++) process.Tick();
                    }
                    if (!process.IsEnded && _observation != null &&
                        _observation.ManeuverEvents == 0)
                    {
                        effect.Apply(process.Context, wrapped);
                        result.SynchronousNativeEffects++;
                    }
                    if (!process.IsEnded)
                    {
                        process.Detach();
                        result.ProcessDetached = true;
                    }
                    result.ProcessEnded = process.IsEnded;
                }
                result.ResourceAfterCommit = caster.Descriptor.Resources
                    .GetResourceAmount(undine.SlaResource);
                InvokeCommandEnded(command, false);
            }
            finally
            {
                observed = EndObservation();
            }
            result.ManeuverEvents = observed.ManeuverEvents;
            result.AttackEvents = observed.AttackEvents;
            result.SavingThrowEvents = observed.SavingThrowEvents;
            result.OpportunityCommandsAfter = CountOpportunityCommands();
            result.AvailableAfterCommit = data.IsAvailable;
            AbilityData fresh = RequireAbility(caster, undine.SlaAbility);
            result.FreshAbilityAvailable = fresh.IsAvailable;
            result.ResourceAfterSecondGate = caster.Descriptor.Resources
                .GetResourceAmount(undine.SlaResource);
            Kingmaker.Controllers.Rest.RestController.ApplyRest(
                caster.Descriptor);
            result.ResourceAfterRest = caster.Descriptor.Resources
                .GetResourceAmount(undine.SlaResource);
            return result;
        }

        private static EngineEvidence InspectEngineContract()
        {
            MethodInfo action = typeof(ContextActionCombatManeuver).GetMethod(
                "RunAction", BindingFlags.Instance | BindingFlags.Public |
                    BindingFlags.NonPublic, null, Type.EmptyTypes, null);
            MethodInfo trigger = typeof(RuleCombatManeuver).GetMethod(
                "OnTrigger", BindingFlags.Instance | BindingFlags.Public |
                    BindingFlags.NonPublic);
            if (action == null || trigger == null)
                throw new MissingMethodException(
                    "Native Hydraulic Push maneuver methods are unavailable.");
            string actionIl = string.Join(Environment.NewLine,
                BrownFurIlDisassembler.Describe(action).ToArray());
            string maneuverIl = string.Join(Environment.NewLine,
                BrownFurIlDisassembler.Describe(trigger).ToArray());
            string combined = actionIl + Environment.NewLine + maneuverIl;
            return new EngineEvidence
            {
                ActionIl = actionIl,
                ManeuverIl = maneuverIl,
                ReplacesAttackBonus = actionIl.Contains(
                    "set_ReplaceAttackBonus") && actionIl.Contains(
                    "get_CasterLevel"),
                ReplacesBaseStat = actionIl.Contains(
                    "set_ReplaceBaseStat"),
                UsesNativeForceMovePush = maneuverIl.Contains(
                    "UnitPartForceMove.Push"),
                NoAttackSaveOrOpportunityConstruction =
                    !combined.Contains("RuleAttackRoll..ctor") &&
                    !combined.Contains("RuleSavingThrow..ctor") &&
                    !combined.Contains("UnitAttackOfOpportunity..ctor")
            };
        }

        private static void CreateFactionPair(
            ICollection<UnityEngine.Object> transient,
            out BlueprintFaction actor, out BlueprintFaction target)
        {
            BlueprintUnit donor = BlueprintRoot.Instance
                .DefaultPlayerCharacter;
            BlueprintFaction source = donor == null ? null : donor.Faction;
            if (source == null)
                throw new InvalidOperationException(
                    "The default character faction is unavailable.");
            actor = UnityEngine.Object.Instantiate(source);
            target = UnityEngine.Object.Instantiate(source);
            actor.name = "KMG_Runtime_HydraulicPush_ActorFaction";
            target.name = "KMG_Runtime_HydraulicPush_TargetFaction";
            ConfigureFaction(actor, target);
            ConfigureFaction(target, actor);
            transient.Add(actor);
            transient.Add(target);
            if (ReferenceEquals(actor, source) || ReferenceEquals(target,
                    source) || ReferenceEquals(actor, target))
                throw new InvalidOperationException(
                    "Hydraulic Push faction isolation reused an installed object.");
        }

        private static void ConfigureFaction(BlueprintFaction faction,
            BlueprintFaction enemy)
        {
            faction.Peaceful = false;
            faction.AlwaysEnemy = false;
            faction.Neutral = false;
            faction.IsDirectlyControllable = false;
            faction.Dummy = null;
            faction.AttackFactions = new[] { enemy };
        }

        private static UnitEntityData CreateUnit(BlueprintFaction faction,
            ICollection<UnitEntityData> created,
            ICollection<UnityEngine.Object> transient, Vector3 position,
            string suffix)
        {
            BlueprintUnit donor = BlueprintRoot.Instance
                .DefaultPlayerCharacter;
            if (donor == null)
                throw new InvalidOperationException(
                    "The default character blueprint is unavailable.");
            BlueprintUnit blueprint = UnityEngine.Object.Instantiate(donor);
            blueprint.name = "KMG_Runtime_HydraulicPush_" + suffix;
            blueprint.Faction = faction;
            blueprint.Brain = null;
            blueprint.IsCheater = true;
            transient.Add(blueprint);
            UnitEntityData result = new Kingmaker.UI.LevelUp.ChargenUnit(
                blueprint).Unit;
            if (result == null || result.Descriptor == null ||
                result.Descriptor.Resources == null)
                throw new InvalidOperationException(
                    "A disposable Hydraulic Push unit was unavailable.");
            result.Descriptor.Stats.HitPoints.BaseValue = 100;
            result.Descriptor.State.Immortality.Retain();
            SetExactProperty(result, "Position", position);
            if (!Game.Instance.State.Units.All.Add(result))
            {
                result.Dispose();
                throw new InvalidOperationException(
                    "A disposable Hydraulic Push unit could not be registered.");
            }
            created.Add(result);
            return result;
        }

        private static void PrepareCaster(UnitEntityData caster,
            ElementalRaceBlueprints undine,
            BlueprintCharacterClass fighter,
            BlueprintCharacterClass wizard, int intelligence, int wisdom,
            int charisma, int fighterLevels, int wizardLevels)
        {
            UnitDescriptor owner = caster.Descriptor;
            owner.Stats.Strength.BaseValue = 10;
            owner.Stats.Dexterity.BaseValue = 10;
            owner.Stats.Constitution.BaseValue = 10;
            owner.Stats.Intelligence.BaseValue = intelligence;
            owner.Stats.Wisdom.BaseValue = wisdom;
            owner.Stats.Charisma.BaseValue = charisma;
            EnsureFact(owner, undine.Race);
            foreach (BlueprintFeature feature in undine.Race.Features)
                EnsureFact(owner, feature);
            Advance(owner, fighter, fighterLevels);
            if (wizardLevels > 0)
                Advance(owner, wizard, wizardLevels);
        }

        private static void InstallProbe(UnitEntityData unit)
        {
            var feature = ScriptableObject.CreateInstance<BlueprintFeature>();
            feature.name = "KMG_Runtime_HydraulicPush_Probe_" +
                unit.UniqueId;
            feature.Ranks = 1;
            feature.IsClassFeature = false;
            feature.HideInUI = true;
            feature.Groups = Array.Empty<FeatureGroup>();
            feature.ComponentsArray = new BlueprintComponent[]
            {
                ScriptableObject.CreateInstance<HydraulicPushRuleProbe>()
            };
            EnsureFact(unit.Descriptor, feature);
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

        private static void Advance(UnitDescriptor owner,
            BlueprintCharacterClass characterClass, int levels)
        {
            if (characterClass == null && levels > 0)
                throw new ArgumentNullException("characterClass");
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
                    "The native Hydraulic Push level-up surface is unavailable.");
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
                            "Disposable Hydraulic Push class selection failed at level " +
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
                throw new MissingMethodException(typeof(UnitCommand).FullName,
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

        private static int CountOpportunityCommands()
        {
            return Game.Instance.State.Units.All.Where(value => value != null &&
                    value.Commands != null).Sum(value => value.Commands.Raw
                    .OfType<UnitAttackOfOpportunity>().Count());
        }

        private static string ExpectedBestMentalStat(UnitEntityData caster)
        {
            int best = Math.Max(caster.Descriptor.Stats.Intelligence.Bonus,
                Math.Max(caster.Descriptor.Stats.Wisdom.Bonus,
                    caster.Descriptor.Stats.Charisma.Bonus));
            var names = new List<string>();
            if (caster.Descriptor.Stats.Intelligence.Bonus == best)
                names.Add(StatType.Intelligence.ToString());
            if (caster.Descriptor.Stats.Wisdom.Bonus == best)
                names.Add(StatType.Wisdom.ToString());
            if (caster.Descriptor.Stats.Charisma.Bonus == best)
                names.Add(StatType.Charisma.ToString());
            return string.Join(",", names.ToArray());
        }

        private static int SelectedModifier(CaseEvidence value)
        {
            if (string.Equals(value.ReplaceBaseStat,
                    StatType.Intelligence.ToString(), StringComparison.Ordinal))
                return value.IntelligenceModifier;
            if (string.Equals(value.ReplaceBaseStat,
                    StatType.Wisdom.ToString(), StringComparison.Ordinal))
                return value.WisdomModifier;
            if (string.Equals(value.ReplaceBaseStat,
                    StatType.Charisma.ToString(), StringComparison.Ordinal))
                return value.CharismaModifier;
            return int.MinValue;
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

        private static void BeginObservation(UnitEntityData caster,
            UnitEntityData target)
        {
            if (_activeCaster != null || _activeTarget != null ||
                _observation != null)
                throw new InvalidOperationException(
                    "A prior Hydraulic Push rule observation is active.");
            _activeCaster = caster ?? throw new ArgumentNullException("caster");
            _activeTarget = target ?? throw new ArgumentNullException("target");
            _observation = new ManeuverObservation();
        }

        private static ManeuverObservation EndObservation()
        {
            ManeuverObservation result = _observation ??
                new ManeuverObservation();
            _observation = null;
            _activeCaster = null;
            _activeTarget = null;
            return result;
        }

        internal static void RecordManeuver(RuleCombatManeuver rule)
        {
            if (rule == null || _observation == null ||
                !ReferenceEquals(rule.Initiator, _activeCaster) ||
                !ReferenceEquals(rule.Target, _activeTarget)) return;
            _observation.ManeuverEvents++;
            _observation.Type = rule.Type.ToString();
            _observation.ReplaceAttackBonus = rule.ReplaceAttackBonus;
            _observation.ReplaceBaseStat = rule.ReplaceBaseStat.HasValue ?
                rule.ReplaceBaseStat.Value.ToString() : string.Empty;
            _observation.InitiatorCmb = rule.InitiatorCMB;
            _observation.TargetCmd = rule.TargetCMD;
            _observation.D20 = rule.InitiatorRoll.Value;
            _observation.ManeuverValue = rule.InitiatorCMValue;
            _observation.Success = rule.Success;
            _observation.AutoFailure = rule.AutoFailure;
        }

        internal static void RecordAttack(RuleAttackRoll rule)
        {
            if (rule == null || _observation == null ||
                !ReferenceEquals(rule.Initiator, _activeCaster) ||
                !ReferenceEquals(rule.Target, _activeTarget)) return;
            _observation.AttackEvents++;
        }

        internal static void RecordSavingThrow(RuleSavingThrow rule)
        {
            if (rule == null || _observation == null ||
                (!ReferenceEquals(rule.Initiator, _activeCaster) &&
                 !ReferenceEquals(rule.Initiator, _activeTarget))) return;
            _observation.SavingThrowEvents++;
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
            string all = evidence.Cases == null ? "<none>" :
                string.Join(" | ", evidence.Cases.Select(value =>
                    value.Summary()).ToArray());
            Add(assertions, "hydraulic-blueprint-contract",
                "Close-range hostile SpellLike Bull Rush; no save; SR enabled",
                "guid=" + evidence.AbilityGuid + ";type=" +
                    evidence.AbilityType + ";range=" + evidence.Range +
                    ";sr=" + evidence.SpellResistance + ";save=" +
                    evidence.SavingThrow + ";components=" +
                    evidence.ComponentContract,
                evidence.ComponentContract,
                "production BlueprintAbility and exact native action components");

            CaseEvidence[] formula = evidence.Cases == null ?
                new CaseEvidence[0] : evidence.Cases.Where(value =>
                    value != null && !value.ImmunityInstalled).ToArray();
            bool formulaPass = formula.Length == 8 && formula.All(value =>
                value.HostileBothWays && value.Targetable &&
                value.ContextCasterLevel == value.CharacterLevel &&
                value.ManeuverEvents == 1 &&
                string.Equals(value.ManeuverType,
                    CombatManeuver.BullRush.ToString(),
                    StringComparison.Ordinal) &&
                value.ReplaceAttackBonus.HasValue &&
                value.ReplaceAttackBonus.Value == value.CharacterLevel &&
                SelectedModifier(value) == value.BestMentalModifier &&
                value.InitiatorCmb == value.ExpectedManeuverBonus &&
                value.D20 == 10 && value.ManeuverValue ==
                    value.InitiatorCmb + value.D20);
            Add(assertions, "hydraulic-maneuver-formula",
                "CMB = total character level + best Int/Wis/Cha modifier",
                all, formulaPass,
                "actual AbilityEffectRunAction and RuleCombatManeuver results");

            CaseEvidence intelligence = Find(evidence, "intelligence-positive");
            CaseEvidence wisdom = Find(evidence, "wisdom-positive");
            CaseEvidence charisma = Find(evidence, "charisma-positive");
            bool selection = intelligence != null && wisdom != null &&
                charisma != null &&
                string.Equals(intelligence.ReplaceBaseStat,
                    StatType.Intelligence.ToString(),
                    StringComparison.Ordinal) &&
                string.Equals(wisdom.ReplaceBaseStat,
                    StatType.Wisdom.ToString(), StringComparison.Ordinal) &&
                string.Equals(charisma.ReplaceBaseStat,
                    StatType.Charisma.ToString(), StringComparison.Ordinal);
            Add(assertions, "hydraulic-best-mental-selection",
                "unique Intelligence, Wisdom, and Charisma maxima selected",
                all, selection,
                "production UseBestMentalStat RuleCombatManeuver fields");

            CaseEvidence negative = Find(evidence, "all-negative");
            Add(assertions, "hydraulic-negative-mental-modifiers",
                "highest negative modifier retained without floor at zero",
                negative == null ? "missing" : negative.Summary(),
                negative != null && negative.BestMentalModifier < 0 &&
                    SelectedModifier(negative) ==
                        negative.BestMentalModifier &&
                    negative.InitiatorCmb == negative.CharacterLevel +
                        negative.BestMentalModifier,
                "actual negative unit stats and native maneuver result");

            CaseEvidence tieA = Find(evidence, "tie-a");
            CaseEvidence tieB = Find(evidence, "tie-b");
            Add(assertions, "hydraulic-tie-determinism",
                "identical tied maxima choose the same maximal stat",
                (tieA == null ? "missing" : tieA.Summary()) + " | " +
                    (tieB == null ? "missing" : tieB.Summary()),
                tieA != null && tieB != null &&
                    tieA.BestMentalModifier == tieB.BestMentalModifier &&
                    SelectedModifier(tieA) == tieA.BestMentalModifier &&
                    SelectedModifier(tieB) == tieB.BestMentalModifier &&
                    string.Equals(tieA.ReplaceBaseStat,
                        tieB.ReplaceBaseStat, StringComparison.Ordinal),
                "two independent production action executions");

            CaseEvidence multiclass = Find(evidence, "multiclass");
            Add(assertions, "hydraulic-total-character-level",
                "Fighter 2 / Wizard 3 supplies caster level and base bonus 5",
                multiclass == null ? "missing" : multiclass.Summary(),
                multiclass != null && multiclass.CharacterLevel == 5 &&
                    multiclass.FighterLevel == 2 &&
                    multiclass.WizardLevel == 3 &&
                    multiclass.ContextCasterLevel == 5 &&
                    multiclass.ReplaceAttackBonus == 5,
                "real LevelUpController multiclass fixture");

            CaseEvidence failed = Find(evidence, "ordinary-failure");
            CaseEvidence immune = Find(evidence, "maneuver-immunity");
            CaseEvidence succeeded = intelligence;
            Add(assertions, "hydraulic-native-resolution",
                "ordinary success and failure plus native maneuver immunity",
                all, succeeded != null && succeeded.Success &&
                    failed != null && !failed.Success &&
                    failed.TargetCmd > failed.ManeuverValue &&
                    immune != null && immune.ImmunityInstalled &&
                    immune.ManeuverEvents == 1 && !immune.Success,
                "native Bull Rush RuleCombatManeuver resolution");

            bool noCollateral = evidence.Cases != null &&
                evidence.Cases.Count == 9 && evidence.Cases.All(value =>
                    value.AttackEvents == 0 &&
                    value.SavingThrowEvents == 0 &&
                    value.OpportunityCommandsBefore ==
                        value.OpportunityCommandsAfter);
            Add(assertions, "hydraulic-no-attack-save-or-provoke",
                "no attack roll, saving throw, or attack-of-opportunity command",
                all, noCollateral,
                "request-local rule handlers and global command-count boundary");

            Add(assertions, "hydraulic-command-resource-lifecycle",
                "cancel 1->1; committed cast 1->0; second gated; rest ->1",
                evidence.Command == null ? "missing" :
                    JsonConvert.SerializeObject(evidence.Command),
                evidence.Command != null && evidence.Command.Pass(),
                "exact UnitUseAbility OnAction commitment and ordinary rest");

            EngineEvidence engine = evidence.Engine;
            Add(assertions, "hydraulic-native-engine-path",
                "native replacement fields and UnitPartForceMove.Push without attack/save/AOO construction",
                engine == null ? "missing" : "attack=" +
                    engine.ReplacesAttackBonus + ";stat=" +
                    engine.ReplacesBaseStat + ";push=" +
                    engine.UsesNativeForceMovePush + ";isolated=" +
                    engine.NoAttackSaveOrOpportunityConstruction,
                engine != null && engine.ReplacesAttackBonus &&
                    engine.ReplacesBaseStat && engine.UsesNativeForceMovePush &&
                    engine.NoAttackSaveOrOpportunityConstruction,
                "metadata-only IL of installed Kingmaker 2.1.7b methods");

            Add(assertions, "hydraulic-save-free",
                "no save or player-party mutation", "saveStateTouched=" +
                    evidence.SaveStateTouched, !evidence.SaveStateTouched,
                "request-local disposable registered units only");
            Add(assertions, "hydraulic-cleanup",
                "exact global-unit reference sequence and destroyed clones",
                "cleanupExact=" + evidence.CleanupExact +
                    ";transientDestroyed=" +
                    evidence.TransientObjectsDestroyed,
                evidence.CleanupExact && evidence.TransientObjectsDestroyed,
                "finally interruption, removal, disposal, and clone destruction");
        }

        private static CaseEvidence Find(Evidence evidence, string name)
        {
            return evidence.Cases == null ? null : evidence.Cases
                .SingleOrDefault(value => value != null && string.Equals(
                    value.Name, name, StringComparison.Ordinal));
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
    /// Development-only request-local rule observer. It is never registered
    /// as a production blueprint component.
    /// </summary>
    [Serializable]
    public sealed class HydraulicPushRuleProbe :
        OwnedGameLogicComponent<UnitDescriptor>,
        IInitiatorRulebookHandler<RuleCombatManeuver>,
        IInitiatorRulebookHandler<RuleAttackRoll>,
        IInitiatorRulebookHandler<RuleSavingThrow>
    {
        public void OnEventAboutToTrigger(RuleCombatManeuver evt) { }

        public void OnEventDidTrigger(RuleCombatManeuver evt)
        {
            HydraulicPushScenario.RecordManeuver(evt);
        }

        public void OnEventAboutToTrigger(RuleAttackRoll evt) { }

        public void OnEventDidTrigger(RuleAttackRoll evt)
        {
            HydraulicPushScenario.RecordAttack(evt);
        }

        public void OnEventAboutToTrigger(RuleSavingThrow evt) { }

        public void OnEventDidTrigger(RuleSavingThrow evt)
        {
            HydraulicPushScenario.RecordSavingThrow(evt);
        }
    }
}
