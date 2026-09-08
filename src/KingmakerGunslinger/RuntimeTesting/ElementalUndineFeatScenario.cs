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
using Kingmaker.Blueprints.Area;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Blueprints.Root;
using Kingmaker.Controllers;
using Kingmaker.Controllers.Units;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.Items;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Abilities;
using Kingmaker.UI.LevelUp;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Class.LevelUp;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.Utility;
using Kingmaker.View;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.ElementalRaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    /// <summary>
    /// Save-free live qualification for Hydraulic Maneuver and Triton Portal.
    /// Maneuvers execute through UnitUseAbility and Kingmaker's ordinary
    /// RuleCombatManeuver. Portal uses a request-local scene solely because
    /// this guarded scenario runs without a loaded save; native spawn actions
    /// and RuleSummonUnit remain unmodified.
    /// </summary>
    internal static class ElementalUndineFeatScenario
    {
        internal const string EvidenceFileName =
            "elemental-undine-feats.json";
        private const string FighterClassGuid =
            "48ac8db94d5de7645906c7d0ad3bcfbd";
        private const string ShortSwordGuid =
            "57c8994d1f1becf49ac4f642e5d8ca9d";
        private const string SmallWaterElementalGuid =
            "56372b0a2749c224392a5ee74105c534";

        private sealed class BlueprintEvidence
        {
            public bool ParentExact { get; set; }
            public bool VariantContractsExact { get; set; }
            public bool ParametersExact { get; set; }
            public bool PortalContractExact { get; set; }
            public bool NativeDonorIsolated { get; set; }
            public string Summary { get; set; }
        }

        private sealed class ManeuverObservation
        {
            public int Events { get; set; }
            public int AttackEvents { get; set; }
            public int SaveEvents { get; set; }
            public string Type { get; set; }
            public int? ReplaceAttackBonus { get; set; }
            public string ReplaceBaseStat { get; set; }
            public int InitiatorCmb { get; set; }
            public int TargetCmd { get; set; }
            public int D20 { get; set; }
            public int ManeuverValue { get; set; }
            public bool Success { get; set; }
            public bool AutoFailure { get; set; }
            public bool FixtureGetUpBypass { get; set; }
        }

        private sealed class ManeuverCaseEvidence
        {
            public string Name { get; set; }
            public string AbilityGuid { get; set; }
            public string ExpectedType { get; set; }
            public int CharacterLevel { get; set; }
            public int BestMentalModifier { get; set; }
            public int ExpectedBonus { get; set; }
            public int ContextCasterLevel { get; set; }
            public bool Available { get; set; }
            public bool Targetable { get; set; }
            public bool CanStart { get; set; }
            public bool CancellationTested { get; set; }
            public int ResourceBeforeCancel { get; set; }
            public int ResourceAfterCancel { get; set; }
            public int ResourceBeforeCast { get; set; }
            public int ResourceAfterCast { get; set; }
            public bool AvailableAtZero { get; set; }
            public int ResourceAfterRest { get; set; }
            public bool Immune { get; set; }
            public bool TargetProneInitially { get; set; }
            public bool TargetProneBefore { get; set; }
            public bool TargetProneAfter { get; set; }
            public bool TargetProneStateActive { get; set; }
            public bool TargetViewIsGetUp { get; set; }
            public bool ImmunityInstalled { get; set; }
            public bool TargetHeldWeapon { get; set; }
            public int TemporaryWisdomBefore { get; set; }
            public int TemporaryWisdomAfter { get; set; }
            public string CommandResult { get; set; }
            public bool ProcessPresent { get; set; }
            public bool ProcessEndedOrDetached { get; set; }
            public ManeuverObservation Rule { get; set; }

            public string Summary()
            {
                return Name + ":" + ExpectedType + ";level=" +
                    CharacterLevel + ";best=" + BestMentalModifier +
                    ";expected=" + ExpectedBonus + ";context=" +
                    ContextCasterLevel + ";available=" + Available +
                    ";target=" + Targetable + ";cancel=" +
                    ResourceBeforeCancel + "->" + ResourceAfterCancel +
                    ";cast=" + ResourceBeforeCast + "->" +
                    ResourceAfterCast + ";zero=" + AvailableAtZero +
                    ";rest=" + ResourceAfterRest + ";immune=" + Immune +
                    ";prone=" + TargetProneInitially + "/" +
                    TargetProneBefore + "/" + TargetProneAfter +
                    ";proneState=" + TargetProneStateActive +
                    ";getUp=" + TargetViewIsGetUp + "/" +
                    (Rule != null && Rule.FixtureGetUpBypass) +
                    ";immunityInstalled=" + ImmunityInstalled +
                    ";result=" + CommandResult + ";rule=" +
                    (Rule == null ? "missing" : Rule.Type + "/" +
                    Rule.InitiatorCmb + "+" + Rule.D20 + "=" +
                    Rule.ManeuverValue + "/success=" + Rule.Success);
            }
        }

        private sealed class PortalEvidence
        {
            public bool Available { get; set; }
            public bool CanTarget { get; set; }
            public bool RequireFullRound { get; set; }
            public bool BlueprintFullRound { get; set; }
            public string ActionType { get; set; }
            public bool CancelInstalled { get; set; }
            public bool CancelStarted { get; set; }
            public int ResourceBeforeCancel { get; set; }
            public int ResourceAfterCancel { get; set; }
            public bool InvalidTargetRejected { get; set; }
            public bool InvalidCanTarget { get; set; }
            public bool InvalidCanStart { get; set; }
            public int ResourceAfterInvalidTarget { get; set; }
            public string CommandResult { get; set; }
            public bool ProcessPresent { get; set; }
            public bool ProcessEndedOrDetached { get; set; }
            public int SpawnActionCount { get; set; }
            public int SummonRuleCount { get; set; }
            public int SummonCount { get; set; }
            public string[] SummonGuids { get; set; }
            public double[] DurationsSeconds { get; set; }
            public bool ExactFaction { get; set; }
            public bool SummonsPlayerFaction { get; set; }
            public bool ExactContext { get; set; }
            public bool LifecycleBuffsPresent { get; set; }
            public int ResourceBeforeCast { get; set; }
            public int ResourceAfterCast { get; set; }
            public bool PortalAvailableAtZero { get; set; }
            public bool HydraulicAvailableAtZero { get; set; }
            public int ResourceAfterRest { get; set; }
            public bool PortalAvailableAfterRest { get; set; }
            public bool HydraulicAvailableAfterRest { get; set; }
            public bool RequestLocalPlacementObserved { get; set; }
            public bool AreaContextRestored { get; set; }
            public bool PlayerContextRestored { get; set; }

            public string Summary()
            {
                return "available=" + Available + ";target=" + CanTarget +
                    ";full=" + RequireFullRound + "/" +
                    BlueprintFullRound + ";action=" + ActionType +
                    ";cancel=" + ResourceBeforeCancel + "->" +
                    ResourceAfterCancel + ";invalid=" +
                    InvalidTargetRejected + "/" + InvalidCanTarget + "/" +
                    InvalidCanStart + "/" +
                    ResourceAfterInvalidTarget + ";cast=" +
                    ResourceBeforeCast + "->" + ResourceAfterCast +
                    ";result=" + CommandResult + ";spawn=" +
                    SpawnActionCount + "/" + SummonRuleCount + "/" +
                    SummonCount + ";guids=" + string.Join(",",
                        SummonGuids ?? new string[0]) + ";duration=" +
                    string.Join(",", (DurationsSeconds ?? new double[0])
                        .Select(value => value.ToString("R")).ToArray()) +
                    ";faction=" + ExactFaction + "/" +
                    SummonsPlayerFaction + ";context=" +
                    ExactContext + ";lifecycle=" +
                    LifecycleBuffsPresent + ";zero=" +
                    PortalAvailableAtZero + "/" +
                    HydraulicAvailableAtZero + ";rest=" +
                    ResourceAfterRest + "/" + PortalAvailableAfterRest +
                    "/" + HydraulicAvailableAfterRest + ";restored=" +
                    AreaContextRestored + "/" + PlayerContextRestored;
            }
        }

        private sealed class Evidence
        {
            public int SchemaVersion { get; set; }
            public bool ModuleActive { get; set; }
            public bool SaveStateTouched { get; set; }
            public BlueprintEvidence Blueprint { get; set; }
            public List<ManeuverCaseEvidence> Maneuvers { get; set; }
            public PortalEvidence Portal { get; set; }
            public bool CleanupExact { get; set; }
            public bool TransientObjectsDestroyed { get; set; }
        }

        [ThreadStatic] private static UnitEntityData _activeCaster;
        [ThreadStatic] private static UnitEntityData _activeTarget;
        [ThreadStatic] private static ManeuverObservation _activeObservation;
        [ThreadStatic] private static PortalHarness _activePortal;
        [ThreadStatic] private static PortalHarness _nativeDiagnosticPortal;

        internal static RuntimeTestResult Run(ModContext context,
            RuntimeTestRequest request)
        {
            var assertions = new List<RuntimeTestAssertion>();
            var diagnostics = new List<string>();
            var evidenceFiles = new List<string>();
            var evidence = new Evidence
            {
                SchemaVersion = 1,
                ModuleActive = BlueprintBootstrap.ElementalFeats != null,
                SaveStateTouched = false,
                Maneuvers = new List<ManeuverCaseEvidence>()
            };
            UnitEntityData[] unitsBefore = Game.Instance.State.Units.All
                .ToArray();
            var created = new List<UnitEntityData>();
            var transient = new List<UnityEngine.Object>();
            var items = new List<ItemEntity>();
            PortalHarness portal = null;
            string exceptionSummary = string.Empty;
            string stage = "resolve-production-graph";
            try
            {
                ElementalRaceBlueprints undine = BlueprintBootstrap
                    .ElementalRaces.Undine;
                ElementalFeatBlueprintSet feats = BlueprintBootstrap
                    .ElementalFeats;
                BlueprintCharacterClass fighter = BlueprintLibraryLookup
                    .RequireExact<BlueprintCharacterClass>(
                        BlueprintBootstrap.Library, FighterClassGuid,
                        "Undine feat Fighter fixture");
                BlueprintItemWeapon shortSword = BlueprintLibraryLookup
                    .RequireExact<BlueprintItemWeapon>(
                        BlueprintBootstrap.Library, ShortSwordGuid,
                        "Undine feat Disarm target weapon");
                BlueprintFeature hydraulicFeat = feats.RequireFeature(
                    ElementalFeatId.HydraulicManeuver);
                BlueprintFeature portalFeat = feats.RequireFeature(
                    ElementalFeatId.TritonPortal);
                BlueprintAbility parent = GrantedAbility(hydraulicFeat);
                BlueprintAbility[] variants = parent.ComponentsArray.OfType<
                    AbilityVariants>().Single().Variants;
                BlueprintAbility portalAbility = GrantedAbility(portalFeat);
                evidence.Blueprint = InspectBlueprint(undine, hydraulicFeat,
                    portalFeat, parent, variants, portalAbility);

                portal = new PortalHarness(diagnostics);
                portal.Initialize(undine.Race);
                BlueprintFaction actorFaction;
                BlueprintFaction targetFaction;
                CreateFactionPair(transient, out actorFaction,
                    out targetFaction);
                CombatManeuver[] types =
                {
                    CombatManeuver.BullRush,
                    CombatManeuver.Disarm,
                    CombatManeuver.Trip,
                    CombatManeuver.DirtyTrickBlind
                };
                for (int index = 0; index < variants.Length; index++)
                {
                    stage = "maneuver-" + types[index];
                    evidence.Maneuvers.Add(ExerciseManeuver(undine,
                        hydraulicFeat, parent, variants[index], types[index],
                        fighter, shortSword, actorFaction, targetFaction,
                        portal, transient, items, index == 0, false,
                        index == 2));
                }
                stage = "maneuver-native-immunity";
                evidence.Maneuvers.Add(ExerciseManeuver(undine,
                    hydraulicFeat, parent, variants[2], CombatManeuver.Trip,
                    fighter, shortSword, actorFaction, targetFaction,
                    portal, transient, items, false, true, false));

                stage = "triton-portal-native-command";
                evidence.Portal = ExercisePortal(portal, undine,
                    hydraulicFeat, portalFeat, parent, variants[0],
                    portalAbility, fighter);
            }
            catch (Exception exception)
            {
                exceptionSummary = "stage=" + stage + ";" + exception;
                diagnostics.Add(exceptionSummary);
            }
            finally
            {
                _activePortal = null;
                if (portal != null)
                {
                    try
                    {
                        portal.Dispose();
                        if (evidence.Portal != null)
                        {
                            evidence.Portal.AreaContextRestored =
                                portal.AreaContextRestored;
                            evidence.Portal.PlayerContextRestored =
                                portal.PlayerContextRestored;
                        }
                    }
                    catch (Exception cleanup)
                    {
                        diagnostics.Add("portal-cleanup=" + cleanup);
                    }
                }
                EndObservation();
                foreach (UnitEntityData unit in created.AsEnumerable()
                    .Reverse().Where(value => value != null).ToArray())
                {
                    unit.Commands.InterruptAll(true);
                    if (unit.Body != null && unit.Body.PrimaryHand != null &&
                        unit.Body.PrimaryHand.MaybeItem != null)
                        unit.Body.PrimaryHand.RemoveItem(false);
                }
                foreach (ItemEntity item in items.Where(value => value != null)
                    .Distinct().ToArray()) item.Dispose();
                foreach (UnitEntityData unit in created.AsEnumerable()
                    .Reverse().Where(value => value != null).ToArray())
                {
                    Game.Instance.State.Units.All.Remove(unit);
                    unit.Descriptor.State.Immortality.ReleaseAll();
                    unit.Dispose();
                }
                foreach (UnityEngine.Object value in transient.AsEnumerable()
                    .Reverse().Where(value => value != null).ToArray())
                    UnityEngine.Object.DestroyImmediate(value);
                evidence.CleanupExact = SameReferences(unitsBefore,
                    Game.Instance.State.Units.All.ToArray());
                evidence.TransientObjectsDestroyed = transient.All(value =>
                    value == null);
            }

            AddAssertions(assertions, evidence);
            if (portal != null)
                assertions.Add(new RuntimeTestAssertion {
                    Name = "triton-portal-native-diagnostic-lifetime",
                    Expected = "zero native reports from initialization through teardown; observer released",
                    Observed = "exceptions=" + portal.NativeExceptions + ";errors=" + portal.NativeErrors +
                        ";initialization=" + portal.NativeInitializationObserved +
                        ";teardown=" + portal.NativeTeardownObserved +
                        ";released=" + portal.NativeObservationReleased,
                    Status = portal.NativeExceptions == 0 && portal.NativeErrors == 0 &&
                        portal.NativeInitializationObserved && portal.NativeTeardownObserved &&
                        portal.NativeObservationReleased ? RuntimeTestStatuses.Pass : RuntimeTestStatuses.Fail,
                    Evidence = "request-local native error observer, independent of placement overrides"
                });
            string path = Path.Combine(request.EvidenceDirectory,
                EvidenceFileName);
            File.WriteAllText(path, JsonConvert.SerializeObject(evidence,
                Formatting.Indented, EvidenceSettings()));
            evidenceFiles.Add(path);
            diagnostics.Add("elementalUndineFeatSha256=" + Hash(path));
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
                StartUtc = DateTime.UtcNow.ToString("o"),
                EndUtc = string.Empty,
                Assertions = assertions,
                Diagnostics = diagnostics,
                Warnings = new List<string>(),
                ExceptionSummary = exceptionSummary,
                EvidenceFiles = evidenceFiles,
                AutomaticExitRequested = request.ExitAfterCompletion,
                EvidenceDirectory = request.EvidenceDirectory
            };
        }

        private static BlueprintEvidence InspectBlueprint(
            ElementalRaceBlueprints undine, BlueprintFeature hydraulicFeat,
            BlueprintFeature portalFeat, BlueprintAbility parent,
            BlueprintAbility[] variants, BlueprintAbility portal)
        {
            CombatManeuver[] expected =
            {
                CombatManeuver.BullRush,
                CombatManeuver.Disarm,
                CombatManeuver.Trip,
                CombatManeuver.DirtyTrickBlind
            };
            bool parentExact = parent.Type == AbilityType.SpellLike &&
                variants.Length == 4 && variants.All(value =>
                    ReferenceEquals(value.Parent, parent));
            bool variantsExact = variants.Select((value, index) => new
            {
                Ability = value,
                Index = index
            }).All(entry =>
            {
                AbilityEffectRunAction effect = entry.Ability.ComponentsArray
                    .OfType<AbilityEffectRunAction>().Single();
                ContextActionCombatManeuver maneuver = effect.Actions.Actions
                    .OfType<ContextActionCombatManeuver>().Single();
                ElementalHydraulicResourceCommit commit = effect.Actions
                    .Actions.OfType<ElementalHydraulicResourceCommit>()
                    .Single();
                AbilityResourceLogic resource = entry.Ability.ComponentsArray
                    .OfType<AbilityResourceLogic>().Single();
                ElementalHydraulicSharedResourceAvailability availability =
                    entry.Ability.ComponentsArray.OfType<
                        ElementalHydraulicSharedResourceAvailability>()
                        .Single();
                return entry.Ability.Type == AbilityType.SpellLike &&
                    entry.Ability.Range == AbilityRange.Close &&
                    entry.Ability.ActionType ==
                        UnitCommand.CommandType.Standard &&
                    entry.Ability.SpellResistance &&
                    entry.Ability.CanTargetEnemies &&
                    !entry.Ability.CanTargetSelf &&
                    maneuver.Type == expected[entry.Index] &&
                    maneuver.ReplaceStat &&
                    maneuver.UseCasterLevelAsBaseAttack &&
                    maneuver.UseBestMentalStat &&
                    ReferenceEquals(commit.Resource, undine.SlaResource) &&
                    ReferenceEquals(resource.RequiredResource,
                        undine.SlaResource) && resource.IsSpendResource &&
                    ReferenceEquals(availability.Undine, undine.Race) &&
                    ReferenceEquals(availability.HydraulicPushFeature,
                        undine.SlaFeature) &&
                    ReferenceEquals(availability.HydraulicPushAbility,
                        undine.SlaAbility) &&
                    ReferenceEquals(availability.Resource,
                        undine.SlaResource);
            });
            ElementalRacialSpellLikeParameters hydraulicParameters =
                hydraulicFeat.ComponentsArray.OfType<
                    ElementalRacialSpellLikeParameters>().Single();
            ElementalRacialSpellLikeParameters portalParameters = portalFeat
                .ComponentsArray.OfType<ElementalRacialSpellLikeParameters>()
                .Single();
            bool parametersExact = ReferenceEquals(
                    hydraulicParameters.Ability, parent) &&
                hydraulicParameters.SpellLevel == 1 &&
                hydraulicParameters.Stat == StatType.Charisma &&
                ReferenceEquals(portalParameters.Ability, portal) &&
                portalParameters.SpellLevel == 3 &&
                portalParameters.Stat == StatType.Charisma;
            AbilityEffectRunAction portalEffect = portal.ComponentsArray
                .OfType<AbilityEffectRunAction>().Single();
            ContextActionSpawnMonster spawn = portalEffect.Actions.Actions
                .OfType<ContextActionSpawnMonster>().Single();
            AbilityResourceLogic portalResource = portal.ComponentsArray
                .OfType<AbilityResourceLogic>().Single();
            bool portalExact = portal.Type == AbilityType.SpellLike &&
                portal.Range == AbilityRange.Close && portal.CanTargetPoint &&
                !portal.CanTargetSelf && portal.IsFullRoundAction &&
                portal.ActionType == UnitCommand.CommandType.Standard &&
                portal.ComponentsArray.OfType<
                    ElementalTritonPortalGroundTargetChecker>().Count() == 1 &&
                spawn.Blueprint != null && string.Equals(
                    spawn.Blueprint.AssetGuid, SmallWaterElementalGuid,
                    StringComparison.Ordinal) &&
                spawn.CountValue.DiceType == DiceType.D3 &&
                spawn.DurationValue.Rate == DurationRate.Rounds &&
                !spawn.DoNotLinkToCaster &&
                !spawn.IsDirectlyControllable &&
                portalEffect.Actions.Actions.Length == 2 &&
                portalEffect.Actions.Actions[0] is
                    ElementalHydraulicResourceCommit &&
                ReferenceEquals(portalResource.RequiredResource,
                    undine.SlaResource) && portalResource.IsSpendResource;
            BlueprintAbility donor = BlueprintLibraryLookup.RequireExact<
                BlueprintAbility>(BlueprintBootstrap.Library,
                    "107788f47c4481f4db6da06498b28270",
                    "Triton Portal isolation control");
            ContextActionSpawnMonster donorSpawn = donor.ComponentsArray
                .OfType<AbilityEffectRunAction>().Single().Actions.Actions
                .OfType<ContextActionSpawnMonster>().Single();
            bool isolated = !ReferenceEquals(portalEffect,
                    donor.ComponentsArray.OfType<AbilityEffectRunAction>()
                        .Single()) && !ReferenceEquals(spawn, donorSpawn) &&
                donorSpawn.CountValue.DiceType == DiceType.Zero;
            return new BlueprintEvidence
            {
                ParentExact = parentExact,
                VariantContractsExact = variantsExact,
                ParametersExact = parametersExact,
                PortalContractExact = portalExact,
                NativeDonorIsolated = isolated,
                Summary = "parent=" + parentExact + ";variants=" +
                    variantsExact + ";parameters=" + parametersExact +
                    ";portal=" + portalExact + ";isolated=" + isolated
            };
        }

        private static ManeuverCaseEvidence ExerciseManeuver(
            ElementalRaceBlueprints undine, BlueprintFeature feat,
            BlueprintAbility parent, BlueprintAbility variant,
            CombatManeuver expected, BlueprintCharacterClass fighter,
            BlueprintItemWeapon shortSword, BlueprintFaction actorFaction,
            BlueprintFaction targetFaction,
            PortalHarness harness,
            ICollection<UnityEngine.Object> transient,
            ICollection<ItemEntity> items, bool testCancellation, bool immune,
            bool temporaryWisdom)
        {
            string suffix = expected + (immune ? "_Immune" : string.Empty);
            UnitEntityData caster = harness.SpawnFixtureUnit(undine.Race,
                actorFaction,
                new Vector3(harness.FixtureCount * 0.25f, 0f, 0f),
                "Caster_" + suffix);
            UnitEntityData target = harness.SpawnFixtureUnit(null,
                targetFaction, caster.Position + new Vector3(0f, 0f, 2f),
                "Target_" + suffix);
            PrepareCaster(caster, undine, fighter, feat, 18, 10, 10, 7);
            target.Descriptor.Stats.Strength.BaseValue = 1;
            target.Descriptor.Stats.Dexterity.BaseValue = 1;
            target.Descriptor.Stats.Constitution.BaseValue = 10;
            target.Descriptor.Stats.HitPoints.BaseValue = 100;
            bool proneInitially = target.Descriptor.State.HasCondition(
                UnitCondition.Prone);
            if (proneInitially)
                target.Descriptor.State.RemoveCondition(UnitCondition.Prone);
            bool proneBefore = target.Descriptor.State.HasCondition(
                UnitCondition.Prone);
            bool proneStateActive = target.Descriptor.State.Prone.Active;
            bool viewIsGetUp = target.View != null && target.View.IsGetUp;
            bool heldWeapon = false;
            if (expected == CombatManeuver.Disarm)
            {
                var item = new ItemEntityWeapon(shortSword);
                items.Add(item);
                target.Body.PrimaryHand.InsertItem(item);
                heldWeapon = ReferenceEquals(
                    target.Body.PrimaryHand.MaybeWeapon, item);
                if (!heldWeapon)
                    throw new InvalidOperationException(
                        "Native Disarm target rejected its manufactured weapon.");
            }
            if (immune)
                target.Descriptor.State.AddCondition(
                    UnitCondition.ImmuneToCombatManeuvers, null);
            bool immunityInstalled = target.Descriptor.State.HasCondition(
                UnitCondition.ImmuneToCombatManeuvers);
            InstallProbe(caster, transient);
            int wisdomBefore = caster.Descriptor.Stats.Wisdom.Bonus;
            if (temporaryWisdom)
            {
                BlueprintFeature adjustment = CreateStatAdjustment(
                    "KMG_Runtime_UndineFeat_TemporaryWisdom_" + suffix,
                    StatType.Wisdom, 12, transient);
                EnsureFact(caster.Descriptor, adjustment);
            }
            int wisdomAfter = caster.Descriptor.Stats.Wisdom.Bonus;
            AbilityData data = RequireVariant(caster, parent, variant);
            var wrapped = new TargetWrapper(target);
            AbilityExecutionContext execution = data.CreateExecutionContext(
                wrapped);
            var result = new ManeuverCaseEvidence
            {
                Name = expected + (immune ? "-immunity" :
                    temporaryWisdom ? "-temporary-wisdom" : string.Empty),
                AbilityGuid = variant.AssetGuid,
                ExpectedType = expected.ToString(),
                CharacterLevel = caster.Descriptor.Progression
                    .CharacterLevel,
                BestMentalModifier = Math.Max(
                    caster.Descriptor.Stats.Intelligence.Bonus,
                    Math.Max(caster.Descriptor.Stats.Wisdom.Bonus,
                        caster.Descriptor.Stats.Charisma.Bonus)),
                ContextCasterLevel = execution.Params.CasterLevel,
                Available = data.IsAvailable,
                Targetable = data.CanTarget(wrapped),
                CancellationTested = testCancellation,
                ResourceBeforeCancel = -1,
                ResourceAfterCancel = -1,
                ResourceAfterRest = -1,
                Immune = immune,
                TargetProneInitially = proneInitially,
                TargetProneBefore = proneBefore,
                TargetProneStateActive = proneStateActive,
                TargetViewIsGetUp = viewIsGetUp,
                ImmunityInstalled = immunityInstalled,
                TargetHeldWeapon = heldWeapon,
                TemporaryWisdomBefore = wisdomBefore,
                TemporaryWisdomAfter = wisdomAfter
            };
            result.ExpectedBonus = result.CharacterLevel +
                result.BestMentalModifier;

            if (testCancellation)
            {
                UnitUseAbility canceled = CreateCommand(data, wrapped,
                    caster);
                result.CanStart = canceled.CanStart;
                result.ResourceBeforeCancel = Resource(caster, undine);
                caster.Commands.Run(canceled);
                caster.Commands.InterruptAll(true);
                caster.Commands.RemoveFinishedAndUpdateQueue();
                result.ResourceAfterCancel = Resource(caster, undine);
            }
            else result.CanStart = CreateCommand(data, wrapped, caster)
                .CanStart;

            UnitUseAbility command = CreateCommand(data, wrapped, caster);
            result.ResourceBeforeCast = Resource(caster, undine);
            BeginObservation(caster, target);
            try
            {
                UnityEngine.Random.InitState(FindNativeD20Seed(10));
                object commandResult = InvokeCommandAction(command);
                result.CommandResult = commandResult == null ? string.Empty :
                    commandResult.ToString();
                AbilityExecutionProcess process = command.ExecutionProcess;
                result.ProcessPresent = process != null;
                if (process != null)
                {
                    bool detached;
                    CompleteProcess(process, out detached);
                    result.ProcessEndedOrDetached = process.IsEnded || detached;
                }
                InvokeCommandEnded(command, false);
            }
            finally
            {
                result.Rule = EndObservation();
            }
            result.ResourceAfterCast = Resource(caster, undine);
            result.TargetProneAfter = target.Descriptor.State.HasCondition(
                UnitCondition.Prone);
            result.AvailableAtZero = RequireVariant(caster, parent, variant)
                .IsAvailable;
            if (testCancellation)
            {
                Kingmaker.Controllers.Rest.RestController.ApplyRest(
                    caster.Descriptor);
                result.ResourceAfterRest = Resource(caster, undine);
            }
            if (immune)
                target.Descriptor.State.RemoveCondition(
                    UnitCondition.ImmuneToCombatManeuvers);
            return result;
        }

        private static PortalEvidence ExercisePortal(PortalHarness harness,
            ElementalRaceBlueprints undine, BlueprintFeature hydraulicFeat,
            BlueprintFeature portalFeat, BlueprintAbility hydraulicParent,
            BlueprintAbility hydraulicVariant, BlueprintAbility portal,
            BlueprintCharacterClass fighter)
        {
            UnitEntityData caster = harness.Caster;
            _activePortal = harness;
            PrepareCaster(caster, undine, fighter, hydraulicFeat, 14, 12, 16,
                5);
            EnsureFact(caster.Descriptor, portalFeat);
            Ability abilityFact = caster.Descriptor.Abilities.GetAbility(
                portal);
            if (abilityFact == null)
                throw new InvalidOperationException(
                    "Triton Portal feat did not grant its ability.");
            var data = new AbilityData(abilityFact);
            var target = new TargetWrapper(caster.Position +
                new Vector3(2f, 0f, 0f));
            var result = new PortalEvidence
            {
                Available = data.IsAvailable,
                CanTarget = data.CanTarget(target),
                RequireFullRound = data.RequireFullRoundAction,
                BlueprintFullRound = portal.IsFullRoundAction,
                ActionType = portal.ActionType.ToString(),
                SummonGuids = new string[0],
                DurationsSeconds = new double[0]
            };

            UnitUseAbility canceled = CreateCommand(data, target, caster);
            result.ResourceBeforeCancel = Resource(caster, undine);
            caster.Commands.Run(canceled);
            result.CancelInstalled = caster.Commands.Contains(canceled);
            result.CancelStarted = canceled.IsStarted;
            caster.Commands.InterruptAll(true);
            caster.Commands.RemoveFinishedAndUpdateQueue();
            result.ResourceAfterCancel = Resource(caster, undine);

            var invalid = new TargetWrapper(caster.Position +
                new Vector3(0f, 1000f, 2f));
            UnitUseAbility invalidCommand = CreateCommand(data, invalid,
                caster);
            result.InvalidCanTarget = data.CanTarget(invalid);
            result.InvalidCanStart = invalidCommand.CanStart;
            // AbilityData.CanTarget is the player-facing point-selection gate.
            // A raw synthetic UnitUseAbility can still report CanStart because
            // that command assumes its target was already accepted upstream.
            result.InvalidTargetRejected = !result.InvalidCanTarget;
            result.ResourceAfterInvalidTarget = Resource(caster, undine);

            UnitUseAbility command = CreateCommand(data, target, caster);
            result.ResourceBeforeCast = Resource(caster, undine);
            UnityEngine.Random.InitState(FindNativeD20Seed(2));
            object commandResult = InvokeCommandAction(command);
            result.CommandResult = commandResult == null ? string.Empty :
                commandResult.ToString();
            AbilityExecutionProcess process = command.ExecutionProcess;
            result.ProcessPresent = process != null;
            if (process != null)
            {
                bool detached;
                CompleteProcess(process, out detached);
                result.ProcessEndedOrDetached = process.IsEnded || detached;
            }
            InvokeCommandEnded(command, false);
            for (int tick = 0; tick < 16; tick++)
                Game.Instance.EntityCreator.Tick();
            result.ResourceAfterCast = Resource(caster, undine);
            result.SpawnActionCount = harness.SpawnActionCount;
            result.SummonRuleCount = harness.Rules.Count;
            result.SummonCount = harness.Summons.Count;
            result.SummonGuids = harness.Summons.Select(value =>
                value.Blueprint.AssetGuid).ToArray();
            result.DurationsSeconds = harness.Rules.Select(value =>
                (value.Duration.Seconds + value.BonusDuration.Seconds)
                    .TotalSeconds).ToArray();
            result.SummonsPlayerFaction = harness.Summons.Count > 0 &&
                harness.Summons.All(value => value.IsPlayerFaction);
            result.ExactFaction = harness.Summons.Count > 0 &&
                harness.Summons.All(value => !value.IsEnemy(caster) &&
                    !caster.IsEnemy(value));
            result.ExactContext = harness.Rules.Count > 0 &&
                harness.Rules.All(value => value.Context != null &&
                    value.Context.SourceAbilityContext != null &&
                    ReferenceEquals(value.Context.SourceAbilityContext
                        .Ability.Blueprint, portal));
            BlueprintBuff lifecycle = BlueprintRoot.Instance.SystemMechanics
                .SummonedUnitBuff;
            result.LifecycleBuffsPresent = harness.Summons.Count > 0 &&
                harness.Summons.All(value =>
                    value.Descriptor.Buffs.GetBuff(lifecycle) != null);
            result.RequestLocalPlacementObserved =
                harness.PlacementObserved;

            result.PortalAvailableAtZero = new AbilityData(abilityFact)
                .IsAvailable;
            result.HydraulicAvailableAtZero = RequireVariant(caster,
                hydraulicParent, hydraulicVariant).IsAvailable;
            Kingmaker.Controllers.Rest.RestController.ApplyRest(
                caster.Descriptor);
            result.ResourceAfterRest = Resource(caster, undine);
            result.PortalAvailableAfterRest = new AbilityData(abilityFact)
                .IsAvailable;
            result.HydraulicAvailableAfterRest = RequireVariant(caster,
                hydraulicParent, hydraulicVariant).IsAvailable;
            _activePortal = null;
            return result;
        }

        internal static void CompleteProcess(AbilityExecutionProcess process,
            out bool detached)
        {
            detached = false;
            for (int tick = 0; tick < 5000 && !process.IsEnded; tick++)
                process.Tick();
            if (!process.IsEnded)
            {
                process.InstantDeliver();
                for (int tick = 0; tick < 5000 && !process.IsEnded; tick++)
                    process.Tick();
            }
            if (!process.IsEnded)
            {
                process.Detach();
                detached = true;
            }
        }

        private static BlueprintAbility GrantedAbility(BlueprintFeature feat)
        {
            AddFacts add = feat.ComponentsArray.OfType<AddFacts>().Single();
            BlueprintAbility[] abilities = add.Facts.OfType<BlueprintAbility>()
                .ToArray();
            if (abilities.Length != 1)
                throw new InvalidOperationException(feat.name +
                    " must grant exactly one ability.");
            return abilities[0];
        }

        private static AbilityData RequireVariant(UnitEntityData unit,
            BlueprintAbility parent, BlueprintAbility variant)
        {
            Ability root = unit.Descriptor.Abilities.GetAbility(parent);
            if (root == null)
                throw new InvalidOperationException(
                    "Hydraulic Maneuver parent ability is absent.");
            return new AbilityData(new AbilityData(root), variant);
        }

        private static int Resource(UnitEntityData caster,
            ElementalRaceBlueprints undine)
        {
            return caster.Descriptor.Resources.GetResourceAmount(
                undine.SlaResource);
        }

        private static void CreateFactionPair(
            ICollection<UnityEngine.Object> transient,
            out BlueprintFaction actor, out BlueprintFaction target)
        {
            BlueprintFaction donor = BlueprintRoot.Instance
                .DefaultPlayerCharacter.Faction;
            if (donor == null)
                throw new InvalidOperationException(
                    "The default character faction is unavailable.");
            actor = UnityEngine.Object.Instantiate(donor);
            target = UnityEngine.Object.Instantiate(donor);
            actor.name = "KMG_Runtime_UndineFeat_ActorFaction";
            target.name = "KMG_Runtime_UndineFeat_TargetFaction";
            ConfigureFaction(actor, target);
            ConfigureFaction(target, actor);
            transient.Add(actor);
            transient.Add(target);
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

        private static UnitEntityData CreateUnit(BlueprintRace race,
            BlueprintFaction faction,
            ICollection<UnitEntityData> created,
            ICollection<UnityEngine.Object> transient, Vector3 position,
            string suffix)
        {
            BlueprintUnit blueprint = UnityEngine.Object.Instantiate(
                BlueprintRoot.Instance.DefaultPlayerCharacter);
            blueprint.name = "KMG_Runtime_UndineFeat_" + suffix;
            if (race != null) blueprint.Race = race;
            blueprint.Faction = faction;
            blueprint.Brain = null;
            blueprint.IsCheater = true;
            transient.Add(blueprint);
            UnitEntityData result = new ChargenUnit(blueprint).Unit;
            if (result == null || result.Descriptor == null ||
                result.Descriptor.Resources == null || (race != null &&
                    !ReferenceEquals(result.Descriptor.Progression.Race,
                        race)))
                throw new InvalidOperationException(
                    "A disposable Undine feat unit was unavailable.");
            result.Descriptor.Stats.HitPoints.BaseValue = 100;
            result.Descriptor.State.Immortality.Retain();
            SetExactProperty(result, "Position", position);
            if (!Game.Instance.State.Units.All.Add(result))
            {
                result.Dispose();
                throw new InvalidOperationException(
                    "A disposable Undine feat unit could not be registered.");
            }
            created.Add(result);
            return result;
        }

        private static void PrepareCaster(UnitEntityData caster,
            ElementalRaceBlueprints undine,
            BlueprintCharacterClass fighter, BlueprintFeature feat,
            int intelligence, int wisdom, int charisma, int levels)
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
            Advance(owner, fighter, levels);
            EnsureFact(owner, feat);
        }

        private static void InstallProbe(UnitEntityData caster,
            ICollection<UnityEngine.Object> transient)
        {
            var feature = ScriptableObject.CreateInstance<BlueprintFeature>();
            feature.name = "KMG_Runtime_UndineFeat_RuleProbe_" +
                caster.UniqueId;
            feature.Ranks = 1;
            feature.IsClassFeature = false;
            feature.HideInUI = true;
            feature.Groups = Array.Empty<FeatureGroup>();
            var component = ScriptableObject.CreateInstance<
                ElementalUndineFeatRuleProbe>();
            feature.ComponentsArray = new BlueprintComponent[] { component };
            transient.Add(feature);
            transient.Add(component);
            EnsureFact(caster.Descriptor, feature);
        }

        private static BlueprintFeature CreateStatAdjustment(string name,
            StatType stat, int value,
            ICollection<UnityEngine.Object> transient)
        {
            var feature = ScriptableObject.CreateInstance<BlueprintFeature>();
            feature.name = name;
            feature.Ranks = 1;
            feature.IsClassFeature = false;
            feature.HideInUI = true;
            feature.Groups = Array.Empty<FeatureGroup>();
            var bonus = ScriptableObject.CreateInstance<AddStatBonus>();
            bonus.Stat = stat;
            bonus.Value = value;
            bonus.Descriptor = ModifierDescriptor.Enhancement;
            feature.ComponentsArray = new BlueprintComponent[] { bonus };
            transient.Add(feature);
            transient.Add(bonus);
            return feature;
        }

        private static void EnsureFact(UnitDescriptor owner,
            BlueprintUnitFact blueprint)
        {
            if (owner.HasFact(blueprint)) return;
            if (owner.AddFact(blueprint) == null || !owner.HasFact(blueprint))
                throw new InvalidOperationException(
                    "Disposable unit rejected fact " + blueprint.name + ".");
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
                            "Disposable Undine class selection failed.");
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

        internal static UnitUseAbility CreateCommand(AbilityData data,
            TargetWrapper target, UnitEntityData caster)
        {
            UnitUseAbility result;
            var cutscene = new Kingmaker.AreaLogic.Cutscenes
                .CutsceneParametersContext();
            using (cutscene.Data) result = new UnitUseAbility(data, target);
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

        internal static object InvokeCommandAction(UnitUseAbility command)
        {
            MethodInfo method = typeof(UnitUseAbility).GetMethod("OnAction",
                BindingFlags.Instance | BindingFlags.Public |
                    BindingFlags.NonPublic, null, Type.EmptyTypes, null);
            if (method == null)
                throw new MissingMethodException(
                    typeof(UnitUseAbility).FullName, "OnAction()");
            return method.Invoke(command, null);
        }

        internal static void InvokeCommandEnded(UnitUseAbility command,
            bool interrupted)
        {
            MethodInfo method = typeof(UnitUseAbility).GetMethod("OnEnded",
                BindingFlags.Instance | BindingFlags.Public |
                    BindingFlags.NonPublic, null,
                new[] { typeof(bool) }, null);
            if (method != null)
                method.Invoke(command, new object[] { interrupted });
        }

        private static int FindNativeD20Seed(int expected)
        {
            for (int seed = 0; seed < 100000; seed++)
            {
                UnityEngine.Random.InitState(seed);
                if (UnityEngine.Random.Range(1, 21) == expected) return seed;
            }
            throw new InvalidOperationException(
                "No deterministic Unity d20 seed produced " + expected +
                ".");
        }

        private static void BeginObservation(UnitEntityData caster,
            UnitEntityData target)
        {
            if (_activeObservation != null)
                throw new InvalidOperationException(
                    "A prior Undine maneuver observation remains active.");
            _activeCaster = caster;
            _activeTarget = target;
            _activeObservation = new ManeuverObservation();
        }

        private static ManeuverObservation EndObservation()
        {
            ManeuverObservation result = _activeObservation ??
                new ManeuverObservation();
            _activeObservation = null;
            _activeCaster = null;
            _activeTarget = null;
            return result;
        }

        internal static void RecordManeuver(RuleCombatManeuver rule)
        {
            if (rule == null || _activeObservation == null ||
                !ReferenceEquals(rule.Initiator, _activeCaster) ||
                !ReferenceEquals(rule.Target, _activeTarget)) return;
            _activeObservation.Events++;
            _activeObservation.Type = rule.Type.ToString();
            _activeObservation.ReplaceAttackBonus = rule.ReplaceAttackBonus;
            _activeObservation.ReplaceBaseStat = rule.ReplaceBaseStat.HasValue
                ? rule.ReplaceBaseStat.Value.ToString() : string.Empty;
            _activeObservation.InitiatorCmb = rule.InitiatorCMB;
            _activeObservation.TargetCmd = rule.TargetCMD;
            _activeObservation.D20 = rule.InitiatorRoll.Value;
            _activeObservation.ManeuverValue = rule.InitiatorCMValue;
            _activeObservation.Success = rule.Success;
            _activeObservation.AutoFailure = rule.AutoFailure;
        }

        internal static void RecordAttack(RuleAttackRoll rule)
        {
            if (rule != null && _activeObservation != null &&
                ReferenceEquals(rule.Initiator, _activeCaster) &&
                ReferenceEquals(rule.Target, _activeTarget))
                _activeObservation.AttackEvents++;
        }

        internal static void RecordSave(RuleSavingThrow rule)
        {
            if (rule != null && _activeObservation != null &&
                (ReferenceEquals(rule.Initiator, _activeCaster) ||
                 ReferenceEquals(rule.Initiator, _activeTarget)))
                _activeObservation.SaveEvents++;
        }

        // Shared request-local native summon boundary. The trait scenario
        // reuses only placement, observation, command driving and cleanup;
        // ContextActionSpawnMonster and RuleSummonUnit are never replaced.
        internal static PortalHarness OpenSummonFixture(BlueprintRace race,
            ICollection<string> diagnostics)
        {
            if (_activePortal != null)
                throw new InvalidOperationException("A summon fixture is already active.");
            var harness = new PortalHarness(diagnostics);
            try
            {
                harness.Initialize(race);
                _activePortal = harness;
                return harness;
            }
            catch
            {
                harness.Dispose();
                throw;
            }
        }

        internal sealed class PortalHarness : IDisposable
        {
            private readonly ICollection<string> _diagnostics;
            private readonly List<RuleSummonUnit> _rules =
                new List<RuleSummonUnit>();
            private readonly HashSet<BlueprintSummonPool> _observedPools =
                new HashSet<BlueprintSummonPool>();
            private readonly List<UnitEntityData> _summons =
                new List<UnitEntityData>();
            private readonly List<UnitEntityData> _fixtures =
                new List<UnitEntityData>();
            private readonly List<BlueprintUnit> _fixtureBlueprints =
                new List<BlueprintUnit>();
            private SceneEntitiesState _scene;
            private BlueprintUnit _casterBlueprint;
            private UnitEntityData _caster;
            private object[] _partyBefore;
            private UnitReference[] _partyCharactersBefore;
            private object _sceneLoader;
            private PropertyInfo _areaProperty;
            private BlueprintArea _areaBefore;
            private AreaPersistentState _loadedAreaBefore;
            private AreaPersistentState _loadedAreaFixture;
            private AreaPersistentState[] _savedAreasBefore;
            private Vector3[] _positions;
            private bool _disposed;

            internal PortalHarness(ICollection<string> diagnostics)
            {
                if (_nativeDiagnosticPortal != null)
                    throw new InvalidOperationException("A native summon diagnostic scope is already active.");
                _diagnostics = diagnostics;
                _nativeDiagnosticPortal = this;
            }

            internal IReadOnlyList<RuleSummonUnit> Rules
            {
                get { return _rules; }
            }

            internal IReadOnlyList<UnitEntityData> Summons
            {
                get { return _summons; }
            }

            internal int SpawnActionCount { get; private set; }
            internal int FixtureCount
            {
                get { return _fixtures.Count; }
            }
            internal UnitEntityData Caster
            {
                get
                {
                    if (_caster == null)
                        throw new InvalidOperationException(
                            "The Triton Portal harness is not initialized.");
                    return _caster;
                }
            }
            internal bool PlacementObserved { get; private set; }
            internal int LastPlacementCount { get; private set; }
            internal int PlacementCalls { get; private set; }
            internal int PositionRequests { get; private set; }
            internal int RuleCallbacks { get; private set; }
            internal int EmptyRuleCallbacks { get; private set; }
            internal int NativeExceptions { get; private set; }
            internal int NativeErrors { get; private set; }
            internal bool NativeInitializationObserved { get; private set; }
            internal bool NativeTeardownObserved { get; private set; }
            internal bool NativeObservationReleased
            { get { return !ReferenceEquals(_nativeDiagnosticPortal, this); } }
            internal bool AreaContextRestored { get; private set; }
            internal bool PlayerContextRestored { get; private set; }

            internal UnitEntityData Initialize(BlueprintRace race)
            {
                NativeInitializationObserved = ReferenceEquals(_nativeDiagnosticPortal, this);
                if (_caster != null)
                    throw new InvalidOperationException(
                        "The Triton Portal harness was initialized twice.");
                if (race == null) throw new ArgumentNullException("race");
                _partyBefore = Game.Instance.Player.Party.Cast<object>()
                    .ToArray();
                _partyCharactersBefore = Game.Instance.Player
                    .PartyCharacters.ToArray();
                InstallAreaContext();
                _scene = new SceneEntitiesState(
                    "KMG_Elemental_Undine_TritonPortal_Fixture");
                _casterBlueprint = UnityEngine.Object.Instantiate(
                    BlueprintRoot.Instance.DefaultPlayerCharacter);
                _casterBlueprint.name =
                    "KMG_Runtime_TritonPortal_Caster";
                _casterBlueprint.Race = race;
                _casterBlueprint.Brain = null;
                _casterBlueprint.IsCheater = false;
                _caster = Game.Instance.EntityCreator.SpawnUnit(
                    _casterBlueprint, Vector3.zero, Quaternion.identity,
                    _scene);
                Game.Instance.EntityCreator.Tick();
                Register(_caster);
                if (_caster == null || _caster.Descriptor == null ||
                    !_caster.IsInState || _caster.HoldingState == null ||
                    !ReferenceEquals(_caster.Descriptor.Progression.Race,
                        race))
                    throw new InvalidOperationException(
                        "The request-local Triton Portal caster did not enter its scene.");
                UnitReference casterReference = _caster;
                if (!Game.Instance.Player.PartyCharacters.Contains(
                        casterReference))
                    Game.Instance.Player.PartyCharacters.Add(
                        casterReference);
                Game.Instance.Player.InvalidateCharacterLists();
                Game.Instance.Player.UpdateCharacterLists();
                if (!Game.Instance.Player.Party.Contains(_caster) ||
                    !Game.Instance.Player.ControllableCharacters.Contains(
                        _caster) || !_caster.IsPlayerFaction)
                    throw new InvalidOperationException(
                        "The request-local Triton Portal caster did not enter the authoritative player caches.");
                _caster.Descriptor.State.Immortality.Retain();
                _caster.Descriptor.Stats.HitPoints.BaseValue = 100;
                _diagnostics.Add("triton-portal-fixture=caster=" +
                    _caster.UniqueId + ";area=" +
                    Game.Instance.CurrentlyLoadedArea.AssetGuid);
                return _caster;
            }

            internal UnitEntityData SpawnFixtureUnit(BlueprintRace race,
                BlueprintFaction faction, Vector3 position, string suffix)
            {
                if (_scene == null)
                    throw new InvalidOperationException(
                        "The live Undine fixture scene is not initialized.");
                BlueprintUnit blueprint = UnityEngine.Object.Instantiate(
                    BlueprintRoot.Instance.DefaultPlayerCharacter);
                blueprint.name = "KMG_Runtime_UndineFeat_" + suffix;
                if (race != null) blueprint.Race = race;
                blueprint.Faction = faction;
                blueprint.Brain = null;
                blueprint.IsCheater = true;
                _fixtureBlueprints.Add(blueprint);
                UnitEntityData unit = Game.Instance.EntityCreator.SpawnUnit(
                    blueprint, position, Quaternion.identity, _scene);
                Game.Instance.EntityCreator.Tick();
                Register(unit);
                if (unit.Descriptor == null || !unit.IsInState ||
                    unit.HoldingState == null || (race != null &&
                        !ReferenceEquals(unit.Descriptor.Progression.Race,
                            race)))
                    throw new InvalidOperationException(
                        "A disposable live Undine feat unit did not enter its scene.");
                unit.Descriptor.State.Immortality.Retain();
                unit.Descriptor.Stats.HitPoints.BaseValue = 100;
                _fixtures.Add(unit);
                return unit;
            }

            internal void ObserveSpawn(ContextActionSpawnMonster action)
            {
                if (action == null) return;
                SpawnActionCount++;
                if (action.SummonPool != null) _observedPools.Add(action.SummonPool);
                _diagnostics.Add("summon-spawn-action=afterSpawn=" +
                    (action.AfterSpawn == null ? "null" : action.AfterSpawn.Actions == null
                        ? "null-actions" : action.AfterSpawn.Actions.Length.ToString()));
            }

            internal void ObserveSpawnFailure(Exception exception)
            {
                if (exception == null) return;
                NativeExceptions++;
                _diagnostics.Add("summon-native-exception=" + exception);
            }

            internal void ObserveNativeError(UnityEngine.Object source, object message)
            {
                NativeErrors++;
                BlueprintScriptableObject blueprint = source as BlueprintScriptableObject;
                _diagnostics.Add("summon-native-error=source=" +
                    (source == null ? "null" : source.name) + ";guid=" +
                    (blueprint == null ? "none" : blueprint.AssetGuid) + ";message=" + message);
            }

            internal bool HasPoolMembership(UnitEntityData unit)
            {
                return _loadedAreaFixture != null &&
                    _observedPools.Any(pool => _loadedAreaFixture.SummonPoolsManager.HasPool(pool) &&
                        _loadedAreaFixture.SummonPoolsManager.GetPool(pool).Units.Contains(unit));
            }

            internal void ObserveRule(RuleSummonUnit rule)
            {
                RuleCallbacks++;
                _diagnostics.Add("summon-rule-observer=owner=" +
                    (rule != null && ReferenceEquals(rule.Initiator, _caster)) +
                    ";unit=" + (rule == null || rule.SummonedUnit == null
                        ? "null" : rule.SummonedUnit.UniqueId));
                if (rule != null && ReferenceEquals(rule.Initiator, _caster) &&
                    rule.SummonedUnit == null) EmptyRuleCallbacks++;
                if (rule == null || _caster == null ||
                    !ReferenceEquals(rule.Initiator, _caster) ||
                    rule.SummonedUnit == null) return;
                Register(rule.SummonedUnit);
                if (!_rules.Contains(rule)) _rules.Add(rule);
                if (!_summons.Contains(rule.SummonedUnit))
                    _summons.Add(rule.SummonedUnit);
            }

            internal bool TryNearest(Vector3 point,
                out Pathfinding.NNInfo result)
            {
                Vector3 projected = new Vector3(point.x, 0f, point.z);
                result = new Pathfinding.NNInfo(null)
                {
                    clampedPosition = projected,
                    constClampedPosition = projected
                };
                return true;
            }

            internal bool PreparePlaces(int count, float radius,
                Vector3 around)
            {
                if (count <= 0)
                    throw new ArgumentOutOfRangeException("count", count,
                        "Triton Portal requested no summon positions.");
                _positions = new Vector3[count];
                float spacing = Math.Max(1f, radius * 2f);
                for (int index = 0; index < count; index++)
                {
                    if (index == 0) _positions[index] = around;
                    else
                    {
                        float angle = (index - 1) * 2f * Mathf.PI /
                            Math.Max(1, count - 1);
                        _positions[index] = around + new Vector3(
                            Mathf.Cos(angle) * spacing, 0f,
                            Mathf.Sin(angle) * spacing);
                    }
                }
                PlacementObserved = true;
                LastPlacementCount = count;
                PlacementCalls++;
                _diagnostics.Add("triton-portal-placement=count=" + count +
                    ";native-actions-preserved=true");
                return true;
            }

            internal bool TryPosition(int index, out Vector3 result)
            {
                PositionRequests++;
                if (_positions == null || index < 0 ||
                    index >= _positions.Length)
                    throw new InvalidOperationException(
                        "Triton Portal requested an unprepared spawn position.");
                result = _positions[index];
                return true;
            }

            private void Register(UnitEntityData unit)
            {
                if (unit == null)
                    throw new InvalidOperationException(
                        "Triton Portal produced a null unit.");
                if (!Game.Instance.State.Units.All.Contains(unit) &&
                    !Game.Instance.State.Units.All.Add(unit))
                    throw new InvalidOperationException(
                        "Triton Portal unit could not enter live state.");
                unit.IsInGame = true;
                unit.IsInFogOfWar = false;
                if (unit.View != null) unit.View.SetVisible(true, true);
            }

            private void InstallAreaContext()
            {
                FieldInfo field = typeof(Game).GetField("m_SceneLoader",
                    BindingFlags.Instance | BindingFlags.NonPublic);
                if (field == null)
                    throw new MissingFieldException(typeof(Game).FullName,
                        "m_SceneLoader");
                _sceneLoader = field.GetValue(Game.Instance);
                _areaProperty = _sceneLoader == null ? null :
                    _sceneLoader.GetType().GetProperty(
                        "CurrentlyLoadedArea", BindingFlags.Instance |
                        BindingFlags.Public | BindingFlags.NonPublic);
                MethodInfo setter = _areaProperty == null ? null :
                    _areaProperty.GetSetMethod(true);
                if (setter == null || _areaProperty.PropertyType !=
                        typeof(BlueprintArea))
                    throw new MissingMethodException(
                        "SceneLoader.CurrentlyLoadedArea exact setter");
                _areaBefore = (BlueprintArea)_areaProperty.GetValue(
                    _sceneLoader, null);
                BlueprintArea area = BlueprintRoot.Instance.NewGamePreset ==
                    null ? null : BlueprintRoot.Instance.NewGamePreset.Area;
                if (area == null || area.IsCapital)
                    throw new InvalidOperationException(
                        "No safe native request-local area metadata exists.");
                setter.Invoke(_sceneLoader, new object[] { area });
                if (!ReferenceEquals(Game.Instance.CurrentlyLoadedArea, area))
                    throw new InvalidOperationException(
                        "Triton Portal area metadata was not installed.");
                _loadedAreaBefore = Game.Instance.State.LoadedAreaState;
                _savedAreasBefore = Game.Instance.State.SavedAreaStates.ToArray();
                if (_loadedAreaBefore != null)
                    throw new InvalidOperationException(
                        "The save-free summon fixture cannot replace a loaded area state.");
                _loadedAreaFixture = new AreaPersistentState(area);
                Game.Instance.State.LoadedAreaState = _loadedAreaFixture;
                if (!ReferenceEquals(Game.Instance.SummonPools,
                        _loadedAreaFixture.SummonPoolsManager))
                    throw new InvalidOperationException(
                        "The native request-local summon-pool service is missing.");
                _diagnostics.Add("summon-native-pool-service=installed;prior-loaded-area=null");
            }

            public void Dispose()
            {
                if (_disposed) return;
                _disposed = true;
                try
                {
                    NativeTeardownObserved = ReferenceEquals(_nativeDiagnosticPortal, this);
                    DisposeOwnedState();
                }
                finally
                {
                    if (ReferenceEquals(_nativeDiagnosticPortal, this))
                        _nativeDiagnosticPortal = null;
                }
            }

            private void DisposeOwnedState()
            {
                if (ReferenceEquals(_activePortal, this)) _activePortal = null;
                foreach (UnitEntityData unit in _summons.AsEnumerable()
                    .Reverse().Where(value => value != null).Distinct()
                    .ToArray()) DisposeUnit(unit);
                foreach (UnitEntityData unit in _fixtures.AsEnumerable()
                    .Reverse().Where(value => value != null).Distinct()
                    .ToArray())
                {
                    unit.Commands.InterruptAll(true);
                    if (unit.Body != null && unit.Body.PrimaryHand != null &&
                        unit.Body.PrimaryHand.MaybeItem != null)
                        unit.Body.PrimaryHand.RemoveItem(false);
                    unit.Descriptor.State.Immortality.ReleaseAll();
                    DisposeUnit(unit);
                }
                if (_caster != null)
                {
                    _caster.Commands.InterruptAll(true);
                    _caster.Descriptor.State.Immortality.ReleaseAll();
                    RestorePlayerContext();
                    DisposeUnit(_caster);
                }
                Game.Instance.EntityCreator.Tick();
                if (_casterBlueprint != null)
                    UnityEngine.Object.DestroyImmediate(_casterBlueprint);
                foreach (BlueprintUnit blueprint in _fixtureBlueprints
                    .AsEnumerable().Reverse().Where(value => value != null)
                    .ToArray())
                    UnityEngine.Object.DestroyImmediate(blueprint);
                if (_scene != null)
                {
                    _scene.Dispose();
                    _scene = null;
                }
                if (_loadedAreaFixture != null)
                {
                    try
                    {
                        _loadedAreaFixture.Dispose();
                    }
                    finally
                    {
                        Game.Instance.State.LoadedAreaState = _loadedAreaBefore;
                    }
                }
                if (_areaProperty != null && _sceneLoader != null)
                {
                    MethodInfo setter = _areaProperty.GetSetMethod(true);
                    setter.Invoke(_sceneLoader, new object[] { _areaBefore });
                    AreaContextRestored = ReferenceEquals(
                        Game.Instance.CurrentlyLoadedArea, _areaBefore) &&
                        ReferenceEquals(Game.Instance.State.LoadedAreaState, _loadedAreaBefore) &&
                        (_savedAreasBefore == null || Game.Instance.State.SavedAreaStates
                            .SequenceEqual(_savedAreasBefore));
                }
            }

            private void RestorePlayerContext()
            {
                if (_partyCharactersBefore == null) return;
                Game.Instance.Player.PartyCharacters.Clear();
                Game.Instance.Player.PartyCharacters.AddRange(
                    _partyCharactersBefore);
                Game.Instance.Player.InvalidateCharacterLists();
                Game.Instance.Player.UpdateCharacterLists();
                PlayerContextRestored = Game.Instance.Player.PartyCharacters
                    .SequenceEqual(_partyCharactersBefore) &&
                    _partyBefore != null && Game.Instance.Player.Party
                    .Cast<object>().SequenceEqual(_partyBefore);
            }

            private static void DisposeUnit(UnitEntityData unit)
            {
                if (unit == null) return;
                Game.Instance.State.Units.All.Remove(unit);
                unit.Dispose();
            }
        }

        [HarmonyPatch(typeof(Kingmaker.View.UnitEntityView), "get_IsGetUp")]
        private static class UndineManeuverFixtureGetUpPatch
        {
            private static bool Prefix(Kingmaker.View.UnitEntityView __instance,
                ref bool __result)
            {
                if (_activeObservation == null || _activeTarget == null ||
                    !ReferenceEquals(_activeTarget.View, __instance) ||
                    _activeTarget.Descriptor.State.Prone.Active)
                    return true;
                __result = false;
                _activeObservation.FixtureGetUpBypass = true;
                return false;
            }
        }

        [HarmonyPatch(typeof(ObstacleAnalyzer), "GetNearestNode",
            new[] { typeof(Vector3) })]
        private static class TritonPortalNearestNodePatch
        {
            private static bool Prefix(Vector3 pos,
                ref Pathfinding.NNInfo __result)
            {
                PortalHarness active = _activePortal;
                if (active == null) return true;
                return !active.TryNearest(pos, out __result);
            }
        }

        [HarmonyPatch(typeof(FreePlaceSelector), "PlaceSpawnPlaces",
            new[] { typeof(int), typeof(float), typeof(Vector3) })]
        private static class TritonPortalSpawnPlacesPatch
        {
            private static bool Prefix(int count, float radius,
                Vector3 aroundPoint)
            {
                PortalHarness active = _activePortal;
                return active == null ||
                    !active.PreparePlaces(count, radius, aroundPoint);
            }
        }

        [HarmonyPatch(typeof(FreePlaceSelector), "GetRelaxedPosition",
            new[] { typeof(int), typeof(bool) })]
        private static class TritonPortalGroundProjectionPatch
        {
            private static bool Prefix(int index, ref Vector3 __result)
            {
                PortalHarness active = _activePortal;
                if (active == null) return true;
                return !active.TryPosition(index, out __result);
            }
        }

        [HarmonyPatch(typeof(ContextActionSpawnMonster), "RunAction")]
        private static class TritonPortalSpawnObserverPatch
        {
            private static void Prefix(ContextActionSpawnMonster __instance)
            {
                PortalHarness active = _activePortal;
                if (active != null) active.ObserveSpawn(__instance);
            }

        }

        // The installed Harmony12 bridge supports prefixes/postfixes, not
        // finalizers. Observe the two exact native reporting boundaries;
        // never suppress, replace, or rethrow the original report.
        [HarmonyPatch(typeof(UberDebug), "LogException",
            new[] { typeof(Exception), typeof(UnityEngine.Object) })]
        private static class SummonFixtureActionExceptionObserverPatch
        {
            private static void Prefix(Exception __0)
            {
                PortalHarness active = _nativeDiagnosticPortal;
                if (active != null) active.ObserveSpawnFailure(__0);
            }
        }

        [HarmonyPatch(typeof(UberDebug), "LogException", new[] { typeof(Exception) })]
        private static class SummonFixtureRuleExceptionObserverPatch
        {
            private static void Prefix(Exception __0)
            {
                PortalHarness active = _nativeDiagnosticPortal;
                if (active != null) active.ObserveSpawnFailure(__0);
            }
        }

        [HarmonyPatch(typeof(UberDebug), "LogError",
            new[] { typeof(UnityEngine.Object), typeof(object), typeof(object[]) })]
        private static class SummonFixtureNativeErrorAttributionPatch
        {
            private static void Prefix(UnityEngine.Object __0, object __1)
            {
                PortalHarness active = _nativeDiagnosticPortal;
                if (active != null) active.ObserveNativeError(__0, __1);
            }
        }

        [HarmonyPatch(typeof(RuleSummonUnit), "OnTrigger",
            new[] { typeof(RulebookEventContext) })]
        private static class TritonPortalSummonObserverPatch
        {
            [HarmonyPriority(Priority.Last)]
            private static void Postfix(RuleSummonUnit __instance)
            {
                PortalHarness active = _activePortal;
                if (active != null) active.ObserveRule(__instance);
            }
        }

        private static void AddAssertions(
            ICollection<RuntimeTestAssertion> assertions, Evidence evidence)
        {
            BlueprintEvidence blueprint = evidence.Blueprint;
            Add(assertions, "undine-feat-blueprint-contract",
                "four exact native maneuver variants and one isolated native 1d3 Small Water Elemental portal",
                blueprint == null ? "missing" : blueprint.Summary,
                blueprint != null && blueprint.ParentExact &&
                    blueprint.VariantContractsExact &&
                    blueprint.ParametersExact &&
                    blueprint.PortalContractExact &&
                    blueprint.NativeDonorIsolated,
                "production manifest-backed feat graph and untouched native donor");

            ManeuverCaseEvidence[] cases = evidence.Maneuvers == null ?
                new ManeuverCaseEvidence[0] : evidence.Maneuvers.ToArray();
            ManeuverCaseEvidence[] ordinary = cases.Where(value =>
                value != null && !value.Immune).ToArray();
            string maneuverSummary = string.Join(" | ", cases.Where(value =>
                value != null).Select(value => value.Summary()).ToArray());
            string[] expected =
            {
                CombatManeuver.BullRush.ToString(),
                CombatManeuver.Disarm.ToString(),
                CombatManeuver.Trip.ToString(),
                CombatManeuver.DirtyTrickBlind.ToString()
            };
            bool variants = ordinary.Length == 4 && ordinary.Select(value =>
                    value.ExpectedType).SequenceEqual(expected) &&
                ordinary.All(value => value.Available && value.Targetable &&
                    value.CanStart && value.ProcessPresent &&
                    value.ProcessEndedOrDetached && string.Equals(
                        value.CommandResult, "Success",
                        StringComparison.Ordinal) && value.Rule != null &&
                    value.Rule.Events == 1 && string.Equals(value.Rule.Type,
                        value.ExpectedType, StringComparison.Ordinal) &&
                    value.Rule.ReplaceAttackBonus == value.CharacterLevel &&
                    value.Rule.InitiatorCmb == value.ExpectedBonus &&
                    value.Rule.ManeuverValue == value.ExpectedBonus +
                        value.Rule.D20 && value.ResourceBeforeCast == 1 &&
                    value.ResourceAfterCast == 0 &&
                    !value.AvailableAtZero);
            Add(assertions, "hydraulic-maneuver-native-variants",
                "Bull Rush, Disarm, Trip, and Dirty Trick (blind) each execute once through native RuleCombatManeuver",
                maneuverSummary, variants,
                "actual UnitUseAbility and RuleCombatManeuver events");

            Add(assertions, "hydraulic-maneuver-formula-and-current-stat",
                "every variant uses total level plus current best mental modifier; temporary Wisdom is observed",
                maneuverSummary, ordinary.Length == 4 && ordinary.All(value =>
                    value.ContextCasterLevel == value.CharacterLevel &&
                    value.ExpectedBonus == value.CharacterLevel +
                        value.BestMentalModifier) && ordinary.Any(value =>
                    value.TemporaryWisdomAfter ==
                        value.TemporaryWisdomBefore + 6 &&
                    value.Rule != null &&
                    string.Equals(value.Rule.ReplaceBaseStat,
                        StatType.Wisdom.ToString(), StringComparison.Ordinal)),
                "real level-up progression, temporary AddStatBonus, and ability parameters");

            ManeuverCaseEvidence bullRush = ordinary.SingleOrDefault(value =>
                string.Equals(value.ExpectedType,
                    CombatManeuver.BullRush.ToString(),
                    StringComparison.Ordinal));
            Add(assertions, "hydraulic-maneuver-cancel-zero-rest",
                "cancellation 1->1, accepted use 1->0, zero blocks, ordinary rest restores one",
                bullRush == null ? "missing" : bullRush.Summary(),
                bullRush != null && bullRush.CancellationTested &&
                    bullRush.ResourceBeforeCancel == 1 &&
                    bullRush.ResourceAfterCancel == 1 &&
                    bullRush.ResourceBeforeCast == 1 &&
                    bullRush.ResourceAfterCast == 0 &&
                    !bullRush.AvailableAtZero &&
                    bullRush.ResourceAfterRest == 1,
                "native command queue cancellation, shared resource, and RestController");

            ManeuverCaseEvidence disarm = ordinary.SingleOrDefault(value =>
                string.Equals(value.ExpectedType,
                    CombatManeuver.Disarm.ToString(),
                    StringComparison.Ordinal));
            Add(assertions, "hydraulic-maneuver-native-target-contracts",
                "Disarm receives a held manufactured weapon and no variant constructs attack rolls or saving throws",
                maneuverSummary, disarm != null &&
                    disarm.TargetHeldWeapon && ordinary.All(value =>
                    value.Rule != null && value.Rule.AttackEvents == 0 &&
                    value.Rule.SaveEvents == 0),
                "live item slot plus request-local rule handlers");

            ManeuverCaseEvidence immune = cases.SingleOrDefault(value =>
                value != null && value.Immune);
            Add(assertions, "hydraulic-maneuver-native-immunity",
                "combat-maneuver immunity rejects Trip while an accepted attempt spends the shared use",
                immune == null ? "missing" : immune.Summary(),
                immune != null && immune.ImmunityInstalled &&
                    !immune.TargetProneBefore && !immune.TargetProneAfter &&
                    immune.Rule != null && immune.Rule.Events == 1 &&
                    string.Equals(immune.Rule.Type,
                        CombatManeuver.Trip.ToString(),
                        StringComparison.Ordinal) &&
                    immune.Rule.InitiatorCmb == 0 &&
                    immune.Rule.D20 == 0 &&
                    immune.ResourceBeforeCast == 1 &&
                    immune.ResourceAfterCast == 0,
                "native RuleCombatManeuver early-return on UnitCondition.ImmuneToCombatManeuvers plus live outcome");

            PortalEvidence portal = evidence.Portal;
            Add(assertions, "triton-portal-action-and-targeting",
                "full-round point-target SpellLike command; canceled and invalid commands spend nothing",
                portal == null ? "missing" : portal.Summary(),
                portal != null && portal.Available && portal.CanTarget &&
                    portal.RequireFullRound && portal.BlueprintFullRound &&
                    string.Equals(portal.ActionType,
                        UnitCommand.CommandType.Standard.ToString(),
                        StringComparison.Ordinal) && portal.CancelInstalled &&
                    !portal.CancelStarted &&
                    portal.ResourceBeforeCancel == 1 &&
                    portal.ResourceAfterCancel == 1 &&
                    portal.InvalidTargetRejected &&
                    portal.ResourceAfterInvalidTarget == 1,
                "actual AbilityData targeting and UnitUseAbility cancellation gates");

            Add(assertions, "triton-portal-native-summon",
                "accepted cast creates 1d3 exact native Small Water Elementals through one spawn action and one RuleSummonUnit per creature",
                portal == null ? "missing" : portal.Summary(),
                portal != null && string.Equals(portal.CommandResult,
                    "Success", StringComparison.Ordinal) &&
                    portal.ProcessPresent && portal.ProcessEndedOrDetached &&
                    portal.SpawnActionCount == 1 && portal.SummonCount >= 1 &&
                    portal.SummonCount <= 3 &&
                    portal.SummonRuleCount == portal.SummonCount &&
                    portal.SummonGuids.Length == portal.SummonCount &&
                    portal.SummonGuids.All(value => string.Equals(value,
                        SmallWaterElementalGuid, StringComparison.Ordinal)) &&
                    portal.RequestLocalPlacementObserved,
                "native ContextActionSpawnMonster and RuleSummonUnit observers");

            Add(assertions, "triton-portal-lifecycle-faction-duration",
                "summons retain native allied non-hostile faction semantics, linked lifecycle, and exactly five rounds at total level 5",
                portal == null ? "missing" : portal.Summary(),
                portal != null && portal.ExactFaction &&
                    portal.ExactContext && portal.LifecycleBuffsPresent &&
                    portal.DurationsSeconds.Length == portal.SummonCount &&
                    portal.DurationsSeconds.All(value =>
                        Math.Abs(value - 30d) <= 0.001d),
                "live mutually allied summoned units, source context, canonical lifecycle buff, and RuleSummonUnit duration");

            Add(assertions, "triton-portal-shared-resource",
                "Portal spends the same Hydraulic Push use once; both feat paths block at zero and return after ordinary rest",
                portal == null ? "missing" : portal.Summary(),
                portal != null && portal.ResourceBeforeCast == 1 &&
                    portal.ResourceAfterCast == 0 &&
                    !portal.PortalAvailableAtZero &&
                    !portal.HydraulicAvailableAtZero &&
                    portal.ResourceAfterRest == 1 &&
                    portal.PortalAvailableAfterRest &&
                    portal.HydraulicAvailableAfterRest,
                "shared manifest-backed Undine SLA resource and RestController");

            Add(assertions, "undine-feat-module-boundary",
                "Triton Portal uses the native unit/donor graph and remains independent of Expanded Summoning publication",
                blueprint == null ? "missing" : blueprint.Summary,
                blueprint != null && blueprint.NativeDonorIsolated &&
                    blueprint.PortalContractExact,
                "direct exact native blueprint references; no module toggle lookup");
            Add(assertions, "undine-feat-save-free",
                "no save or personal party state is touched",
                "saveStateTouched=" + evidence.SaveStateTouched,
                !evidence.SaveStateTouched,
                "named request-local units and scene only");
            Add(assertions, "undine-feat-cleanup",
                "global unit sequence, area metadata, and transient Unity ownership restore exactly",
                "units=" + evidence.CleanupExact + ";objects=" +
                    evidence.TransientObjectsDestroyed + ";area=" +
                    (portal != null && portal.AreaContextRestored) +
                    ";player=" + (portal != null &&
                        portal.PlayerContextRestored),
                evidence.CleanupExact && evidence.TransientObjectsDestroyed &&
                    portal != null && portal.AreaContextRestored &&
                    portal.PlayerContextRestored,
                "finally-owned unit, scene, item, faction, area, and player-cache restoration");
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
    /// Request-local observer only. It is never registered in the blueprint
    /// manifest and exists solely on disposable guarded-test units.
    /// </summary>
    [Serializable]
    public sealed class ElementalUndineFeatRuleProbe :
        OwnedGameLogicComponent<UnitDescriptor>,
        IInitiatorRulebookHandler<RuleCombatManeuver>,
        IInitiatorRulebookHandler<RuleAttackRoll>,
        IInitiatorRulebookHandler<RuleSavingThrow>
    {
        public void OnEventAboutToTrigger(RuleCombatManeuver evt) { }
        public void OnEventDidTrigger(RuleCombatManeuver evt)
        {
            ElementalUndineFeatScenario.RecordManeuver(evt);
        }
        public void OnEventAboutToTrigger(RuleAttackRoll evt) { }
        public void OnEventDidTrigger(RuleAttackRoll evt)
        {
            ElementalUndineFeatScenario.RecordAttack(evt);
        }
        public void OnEventAboutToTrigger(RuleSavingThrow evt) { }
        public void OnEventDidTrigger(RuleSavingThrow evt)
        {
            ElementalUndineFeatScenario.RecordSave(evt);
        }
    }
}
