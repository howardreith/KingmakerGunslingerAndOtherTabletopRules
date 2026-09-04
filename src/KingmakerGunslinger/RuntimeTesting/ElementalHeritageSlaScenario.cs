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
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Blueprints.Root;
using Kingmaker.Controllers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Items;
using Kingmaker.PubSubSystem;
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
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Parts;
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
    /// Save-free Release A qualification of every alternate heritage SLA
    /// through the native player command and delivery boundaries.
    /// </summary>
    internal static class ElementalHeritageSlaScenario
    {
        internal const string EvidenceFileName =
            "elemental-heritage-slas.json";
        private const string FighterClassGuid =
            "48ac8db94d5de7645906c7d0ad3bcfbd";
        private const string ShortSwordGuid =
            "57c8994d1f1becf49ac4f642e5d8ca9d";
        private const string UndeadTypeGuid =
            "734a29b693e9ec346ba2951b27987e33";

        [ThreadStatic] private static UnitEntityData _attackProbeCaster;
        [ThreadStatic] private static int _attackProbeEvents;

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
            public string Result { get; set; }
            public bool ProcessPresent { get; set; }
            public bool ProcessEnded { get; set; }
            public bool ProcessDetached { get; set; }
            public int FallbackEffects { get; set; }
            public bool ArcaneFailureInapplicable { get; set; }
            public bool AvailableAfterCast { get; set; }
            public bool FreshAbilityAvailable { get; set; }
            public bool SecondCommandCanStart { get; set; }
            public bool SecondPlayerPathAvailable { get; set; }
            public int ResourceAfterSecondGate { get; set; }
            public int ResourceAfterRest { get; set; }
            public int AttackEvents { get; set; }
            [JsonIgnore] public AbilityExecutionContext Context { get; set; }

            public bool Pass()
            {
                return Targetable && Available && CanStart &&
                    CancelInstalled && !CancelStarted &&
                    ResourceBeforeCancel == 1 && ResourceAfterCancel == 1 &&
                    ResourceBeforeCast == 1 && ResourceAfterCast == 0 &&
                    string.Equals(Result, "Success",
                        StringComparison.Ordinal) && ProcessPresent &&
                    (ProcessEnded || ProcessDetached) &&
                    ArcaneFailureInapplicable && !AvailableAfterCast &&
                    !FreshAbilityAvailable &&
                    !SecondPlayerPathAvailable &&
                    ResourceAfterSecondGate == 0 && ResourceAfterRest == 1;
            }

            public string Summary()
            {
                return "target=" + Targetable + ";available=" + Available +
                    ";canStart=" + CanStart + ";cancel=" + CancelInstalled +
                    "/" + CancelStarted + ";resource=" +
                    ResourceBeforeCancel + "->" + ResourceAfterCancel + "->" +
                    ResourceAfterCast + "->" + ResourceAfterSecondGate + "->" +
                    ResourceAfterRest + ";result=" + Result + ";process=" +
                    ProcessPresent + "/" + ProcessEnded + "/" +
                    ProcessDetached + ";fallback=" + FallbackEffects +
                    ";attacks=" + AttackEvents + ";zeroGate=" +
                    AvailableAfterCast + "/" + FreshAbilityAvailable + "/" +
                    SecondCommandCanStart + "/" +
                    SecondPlayerPathAvailable;
            }
        }

        private sealed class NativeSlaEvidence
        {
            public string Race { get; set; }
            public string Heritage { get; set; }
            public string AbilityGuid { get; set; }
            public string DonorGuid { get; set; }
            public string AbilityType { get; set; }
            public int CasterLevel { get; set; }
            public int SpellLevel { get; set; }
            public int DifficultyClass { get; set; }
            public bool DonorGraphExact { get; set; }
            public string ActionGraph { get; set; }
            public CommandEvidence Command { get; set; }
            public string NewCasterBuffs { get; set; }
            public string NewTargetBuffs { get; set; }
            public int CasterSpeedBefore { get; set; }
            public int CasterSpeedAfter { get; set; }
            public int TargetDamageBefore { get; set; }
            public int TargetDamageAfter { get; set; }
            public bool TouchDeliveryExact { get; set; }
            public bool TouchDeliveryTargetable { get; set; }
            public string TouchDeliveryResult { get; set; }
            public int TouchDeliveryFallbackEffects { get; set; }
            public int TouchDeliveryAttackEvents { get; set; }
            public bool EffectObserved { get; set; }

            public string Summary()
            {
                return Race + "/" + Heritage + ":ability=" + AbilityGuid +
                    ";donor=" + DonorGuid + ";type=" + AbilityType +
                    ";params=" + CasterLevel + "/" + SpellLevel + "/" +
                    DifficultyClass + ";donorExact=" + DonorGraphExact +
                    ";command={" + (Command == null ? "<null>" :
                        Command.Summary()) + "};casterBuffs=" +
                    NewCasterBuffs + ";targetBuffs=" + NewTargetBuffs +
                    ";speed=" + CasterSpeedBefore + "->" + CasterSpeedAfter +
                    ";damage=" + TargetDamageBefore + "->" +
                    TargetDamageAfter + ";touch=" + TouchDeliveryExact + "/" +
                    TouchDeliveryTargetable + "/" + TouchDeliveryResult +
                    "/fallback=" + TouchDeliveryFallbackEffects +
                    "/attacks=" + TouchDeliveryAttackEvents +
                    ";effect=" + EffectObserved;
            }
        }

        private sealed class UnerringEvidence
        {
            public CommandEvidence Command { get; set; }
            public string ParentGuid { get; set; }
            public string VariantGuid { get; set; }
            public string EnchantmentGuid { get; set; }
            public bool PrimaryTargetable { get; set; }
            public bool ExactPrimaryEnchantment { get; set; }
            public int PrimaryEnchantmentCount { get; set; }
            public int SecondaryEnchantmentCount { get; set; }
            public double DurationSeconds { get; set; }
            public bool PersistsWhenUnequipped { get; set; }
            public int PrimaryConfirmationBefore { get; set; }
            public int PrimaryConfirmationAfter { get; set; }
            public int SecondaryConfirmationBefore { get; set; }
            public int SecondaryConfirmationAfter { get; set; }
            public int ExpectedBonus { get; set; }

            public bool Pass()
            {
                return Command != null && Command.Pass() && PrimaryTargetable &&
                    ExactPrimaryEnchantment && PrimaryEnchantmentCount == 1 &&
                    SecondaryEnchantmentCount == 0 &&
                    DurationSeconds > 119d && DurationSeconds <= 121d &&
                    PersistsWhenUnequipped &&
                    PrimaryConfirmationAfter - PrimaryConfirmationBefore ==
                        ExpectedBonus &&
                    SecondaryConfirmationAfter ==
                        SecondaryConfirmationBefore &&
                    ExpectedBonus == 7;
            }

            public string Summary()
            {
                return "parent=" + ParentGuid + ";variant=" + VariantGuid +
                    ";enchantment=" + EnchantmentGuid + ";target=" +
                    PrimaryTargetable + ";counts=" +
                    PrimaryEnchantmentCount + "/" +
                    SecondaryEnchantmentCount + ";duration=" +
                    DurationSeconds.ToString("F3") + ";unequipped=" +
                    PersistsWhenUnequipped + ";confirmation=" +
                    PrimaryConfirmationBefore + "->" +
                    PrimaryConfirmationAfter + "/" +
                    SecondaryConfirmationBefore + "->" +
                    SecondaryConfirmationAfter + ";expected=" +
                    ExpectedBonus + ";command={" +
                    (Command == null ? "<null>" : Command.Summary()) + "}";
            }
        }

        private sealed class ChillTouchEvidence
        {
            public string TargetKind { get; set; }
            public CommandEvidence Command { get; set; }
            public bool TargetClassificationExact { get; set; }
            public bool ParentTouchInstalled { get; set; }
            public int RemainingBeforeDelivery { get; set; }
            public bool DeliveryAbilityExact { get; set; }
            public bool DeliveryTargetable { get; set; }
            public string DeliveryResult { get; set; }
            public int DeliveryFallbackEffects { get; set; }
            public bool TouchControllerInvoked { get; set; }
            public bool RetentionStatePresent { get; set; }
            public string RetentionHeldGuid { get; set; }
            public string RetentionExecutingGuid { get; set; }
            public bool RetentionExactMatch { get; set; }
            public bool TouchPresentAfterController { get; set; }
            public bool StatePresentAfterController { get; set; }
            public int DeliveryAttackEvents { get; set; }
            public int RemainingAfterDelivery { get; set; }
            public int ResourceAfterDelivery { get; set; }
            public int DamageBefore { get; set; }
            public int DamageAfter { get; set; }
            public int StrengthBefore { get; set; }
            public int StrengthAfter { get; set; }
            public bool FrightenedApplied { get; set; }
            public double FrightenedDurationSeconds { get; set; }
            public int CasterLevel { get; set; }

            public bool PassLiving()
            {
                return Command != null && Command.Pass() &&
                    TargetClassificationExact && ParentTouchInstalled &&
                    DeliveryAbilityExact && DeliveryTargetable &&
                    string.Equals(DeliveryResult, "Success",
                        StringComparison.Ordinal) &&
                    DeliveryAttackEvents > 0 && RemainingAfterDelivery == 19 &&
                    ResourceAfterDelivery == 0 && DamageAfter > DamageBefore &&
                    StrengthAfter == StrengthBefore - 1 &&
                    !FrightenedApplied && CasterLevel == 20;
            }

            public bool PassUndead()
            {
                return Command != null && Command.Pass() &&
                    TargetClassificationExact && ParentTouchInstalled &&
                    DeliveryAbilityExact && DeliveryTargetable &&
                    string.Equals(DeliveryResult, "Success",
                        StringComparison.Ordinal) &&
                    DeliveryAttackEvents > 0 && RemainingAfterDelivery == 19 &&
                    ResourceAfterDelivery == 0 && DamageAfter == DamageBefore &&
                    StrengthAfter == StrengthBefore && FrightenedApplied &&
                    FrightenedDurationSeconds > 125d &&
                    FrightenedDurationSeconds <= 145d && CasterLevel == 20;
            }

            public string Summary()
            {
                return TargetKind + ":classification=" +
                    TargetClassificationExact + ";parentTouch=" +
                    ParentTouchInstalled + ";remaining=" +
                    RemainingBeforeDelivery + "->" +
                    RemainingAfterDelivery + ";delivery=" +
                    DeliveryAbilityExact + "/" + DeliveryTargetable + "/" +
                    DeliveryResult + ";fallback=" +
                    DeliveryFallbackEffects + ";controller=" +
                    TouchControllerInvoked + ";retention=" +
                    RetentionStatePresent + "/" + RetentionHeldGuid + "/" +
                    RetentionExecutingGuid + "/" + RetentionExactMatch +
                    "/" + TouchPresentAfterController + "/" +
                    StatePresentAfterController + ";attacks=" +
                    DeliveryAttackEvents + ";resource=" +
                    ResourceAfterDelivery + ";damage=" + DamageBefore + "->" +
                    DamageAfter + ";strength=" + StrengthBefore + "->" +
                    StrengthAfter + ";frightened=" + FrightenedApplied + "/" +
                    FrightenedDurationSeconds.ToString("F3") + ";cl=" +
                    CasterLevel + ";command={" +
                    (Command == null ? "<null>" : Command.Summary()) + "}";
            }
        }

        private sealed class DeliveryEvidence
        {
            internal bool AbilityExact;
            internal bool Targetable;
            internal string Result;
            internal int FallbackEffects;
            internal bool TouchControllerInvoked;
            internal bool RetentionStatePresent;
            internal string RetentionHeldGuid;
            internal string RetentionExecutingGuid;
            internal bool RetentionExactMatch;
            internal bool TouchPresentAfterController;
            internal bool StatePresentAfterController;
            internal int AttackEvents;
        }

        private sealed class Evidence
        {
            public int SchemaVersion { get; set; }
            public bool SaveStateTouched { get; set; }
            public List<NativeSlaEvidence> NativeSlas { get; set; }
            public UnerringEvidence UnerringWeapon { get; set; }
            public List<ChillTouchEvidence> ChillTouch { get; set; }
            public bool ChillTouchPatchesInstalled { get; set; }
            public string ChillTouchPatchAudit { get; set; }
            public bool CleanupExact { get; set; }
        }

        internal static RuntimeTestResult Run(ModContext context,
            RuntimeTestRequest request)
        {
            DateTime started = DateTime.UtcNow;
            var assertions = new List<RuntimeTestAssertion>();
            var diagnostics = new List<string>();
            var evidenceFiles = new List<string>();
            var evidence = new Evidence
            {
                SchemaVersion = 1,
                SaveStateTouched = false,
                NativeSlas = new List<NativeSlaEvidence>(),
                ChillTouch = new List<ChillTouchEvidence>()
            };
            evidence.ChillTouchPatchAudit = DescribeChillTouchPatches(
                context, out bool chillTouchPatchesInstalled);
            evidence.ChillTouchPatchesInstalled =
                chillTouchPatchesInstalled;
            var createdUnits = new List<UnitEntityData>();
            var transient = new List<UnityEngine.Object>();
            var items = new List<ItemEntityWeapon>();
            UnitEntityData[] unitsBefore = Game.Instance.State.Units.All
                .ToArray();
            string stage = "resolve-production-contract";
            string exceptionSummary = string.Empty;
            try
            {
                ElementalRaceBlueprintSet set = BlueprintBootstrap
                    .ElementalRaces;
                if (set == null)
                    throw new InvalidOperationException(
                        "The Elemental Races graph is unavailable.");
                BlueprintCharacterClass fighter = BlueprintLibraryLookup
                    .RequireExact<BlueprintCharacterClass>(
                        BlueprintBootstrap.Library, FighterClassGuid,
                        "alternate heritage SLA Fighter fixture");
                BlueprintFaction actorFaction;
                BlueprintFaction targetFaction;
                CreateFactionPair(transient, out actorFaction,
                    out targetFaction);
                BlueprintFeature attackProbe = CreateAttackProbe(transient);

                stage = "lavasoul-firebelly";
                evidence.NativeSlas.Add(ExerciseNative(set.Ifrit,
                    set.Ifrit.Heritages.Require(ElementalHeritageId.Lavasoul),
                    fighter, actorFaction, targetFaction, attackProbe,
                    createdUnits, transient));
                stage = "sunsoul-flare-burst";
                evidence.NativeSlas.Add(ExerciseNative(set.Ifrit,
                    set.Ifrit.Heritages.Require(ElementalHeritageId.Sunsoul),
                    fighter, actorFaction, targetFaction, attackProbe,
                    createdUnits, transient));
                stage = "gemsoul-color-spray";
                evidence.NativeSlas.Add(ExerciseNative(set.Oread,
                    set.Oread.Heritages.Require(ElementalHeritageId.Gemsoul),
                    fighter, actorFaction, targetFaction, attackProbe,
                    createdUnits, transient));
                stage = "smokesoul-expeditious-retreat";
                evidence.NativeSlas.Add(ExerciseNative(set.Sylph,
                    set.Sylph.Heritages.Require(ElementalHeritageId.Smokesoul),
                    fighter, actorFaction, targetFaction, attackProbe,
                    createdUnits, transient));
                stage = "stormsoul-shocking-grasp";
                evidence.NativeSlas.Add(ExerciseNative(set.Sylph,
                    set.Sylph.Heritages.Require(ElementalHeritageId.Stormsoul),
                    fighter, actorFaction, targetFaction, attackProbe,
                    createdUnits, transient));
                stage = "mistsoul-blur";
                evidence.NativeSlas.Add(ExerciseNative(set.Undine,
                    set.Undine.Heritages.Require(ElementalHeritageId.Mistsoul),
                    fighter, actorFaction, targetFaction, attackProbe,
                    createdUnits, transient));

                stage = "ironsoul-unerring-weapon";
                evidence.UnerringWeapon = ExerciseUnerring(set.Oread,
                    set.Oread.Heritages.Require(ElementalHeritageId.Ironsoul),
                    fighter, actorFaction, targetFaction, attackProbe,
                    createdUnits, transient, items);
                stage = "rimesoul-chill-touch-living";
                evidence.ChillTouch.Add(ExerciseChillTouch(set.Undine,
                    set.Undine.Heritages.Require(ElementalHeritageId.Rimesoul),
                    fighter, actorFaction, targetFaction, attackProbe, false,
                    createdUnits, transient));
                stage = "rimesoul-chill-touch-undead";
                evidence.ChillTouch.Add(ExerciseChillTouch(set.Undine,
                    set.Undine.Heritages.Require(ElementalHeritageId.Rimesoul),
                    fighter, actorFaction, targetFaction, attackProbe, true,
                    createdUnits, transient));
            }
            catch (Exception exception)
            {
                exceptionSummary = "stage=" + stage + ";" + exception;
                diagnostics.Add(exceptionSummary);
            }
            finally
            {
                _attackProbeCaster = null;
                _attackProbeEvents = 0;
                foreach (UnitEntityData unit in createdUnits.AsEnumerable()
                    .Reverse().ToArray())
                {
                    if (unit == null) continue;
                    unit.Commands.InterruptAll(true);
                    if (unit.Body != null && unit.Body.PrimaryHand != null &&
                        unit.Body.PrimaryHand.MaybeItem != null)
                        unit.Body.PrimaryHand.RemoveItem(false);
                    if (unit.Body != null && unit.Body.SecondaryHand != null &&
                        unit.Body.SecondaryHand.MaybeItem != null)
                        unit.Body.SecondaryHand.RemoveItem(false);
                }
                foreach (ItemEntityWeapon item in items.Where(value =>
                    value != null).Distinct().ToArray()) item.Dispose();
                foreach (UnitEntityData unit in createdUnits.AsEnumerable()
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
                    Game.Instance.State.Units.All.ToArray()) &&
                    createdUnits.All(value => value == null ||
                        !Game.Instance.State.Units.All.Contains(value));
            }

            AddAssertions(assertions, evidence);
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
            evidenceFiles.Add(path);
            diagnostics.Add("elementalHeritageSlasSha256=" + Hash(path));
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
                EvidenceFiles = evidenceFiles,
                AutomaticExitRequested = request.ExitAfterCompletion,
                EvidenceDirectory = request.EvidenceDirectory
            };
        }

        private static NativeSlaEvidence ExerciseNative(
            ElementalRaceBlueprints race,
            ElementalHeritageBlueprints heritage,
            BlueprintCharacterClass fighter, BlueprintFaction actorFaction,
            BlueprintFaction targetFaction, BlueprintFeature attackProbe,
            ICollection<UnitEntityData> created,
            ICollection<UnityEngine.Object> transient)
        {
            string suffix = heritage.Definition.Id.ToString();
            BlueprintFaction casterFaction = actorFaction;
            if (heritage.Definition.Id == ElementalHeritageId.Mistsoul)
            {
                casterFaction = BlueprintRoot.Instance
                    .DefaultPlayerCharacter.Faction;
                if (casterFaction == null)
                    throw new InvalidOperationException(
                        "The exact native player faction is unavailable.");
            }
            UnitEntityData caster = CreateUnit(race.Race, casterFaction,
                created, transient, new Vector3(created.Count * 4f, 0f, 0f),
                suffix + "_Caster");
            UnitEntityData target = CreateUnit(null, targetFaction, created,
                transient, caster.Position + new Vector3(0f, 0f, 2f),
                suffix + "_Target");
            PrepareHeritage(caster, race, heritage, fighter, 5);
            PrepareTarget(target);
            EnsureFact(caster.Descriptor, attackProbe);
            AbilityData data = RequireAbility(caster, heritage.SlaAbility);
            BlueprintAbility donor = BlueprintLibraryLookup.RequireExact<
                BlueprintAbility>(BlueprintBootstrap.Library,
                    heritage.Definition.DonorAbilityGuid,
                    heritage.Definition.Name + " native SLA donor");
            var result = new NativeSlaEvidence
            {
                Race = race.Definition.DisplayName,
                Heritage = heritage.Definition.Name,
                AbilityGuid = heritage.SlaAbility.AssetGuid,
                DonorGuid = donor.AssetGuid,
                AbilityType = heritage.SlaAbility.Type.ToString(),
                DonorGraphExact = NativeDonorGraphExact(heritage, donor),
                ActionGraph = RuntimeTestRunner.DescribeNestedObject(
                    heritage.SlaAbility, 9)
            };
            TargetWrapper commandTarget = IsSelfAbility(
                heritage.Definition.Id) ? new TargetWrapper(caster) :
                new TargetWrapper(target);
            UnitEntityData effectTarget = IsSelfAbility(
                heritage.Definition.Id) ? caster : target;
            Buff[] casterBefore = Buffs(caster);
            Buff[] targetBefore = Buffs(target);
            result.CasterSpeedBefore =
                caster.Descriptor.Stats.Speed.ModifiedValue;
            result.TargetDamageBefore = target.Descriptor.Damage;
            Func<bool> observed = () => NewBuffs(casterBefore, caster).Any() ||
                NewBuffs(targetBefore, target).Any() ||
                caster.Descriptor.Stats.Speed.ModifiedValue !=
                    result.CasterSpeedBefore ||
                target.Descriptor.Damage != result.TargetDamageBefore ||
                caster.Get<UnitPartTouch>() != null;
            Func<AbilityData> factory = () =>
                RequireAbility(caster, heritage.SlaAbility);
            result.Command = ExerciseCommand(caster, factory,
                heritage.SlaResource, commandTarget, effectTarget, observed);
            AbilityExecutionContext parameterContext =
                result.Command.Context ?? data.CreateExecutionContext(
                    commandTarget);
            result.CasterLevel = parameterContext.Params.CasterLevel;
            result.SpellLevel = parameterContext.Params.SpellLevel;
            result.DifficultyClass = parameterContext.Params.DC;

            if (heritage.Definition.Id == ElementalHeritageId.Stormsoul &&
                target.Descriptor.Damage == result.TargetDamageBefore)
            {
                BlueprintAbility delivery = heritage.AuxiliaryBlueprints
                    .OfType<BlueprintAbility>().Single();
                DeliveryEvidence deliveryResult = ExecuteTouchDelivery(caster,
                    target, delivery, () => target.Descriptor.Damage !=
                        result.TargetDamageBefore);
                result.TouchDeliveryExact = deliveryResult.AbilityExact;
                result.TouchDeliveryTargetable = deliveryResult.Targetable;
                result.TouchDeliveryResult = deliveryResult.Result;
                result.TouchDeliveryFallbackEffects =
                    deliveryResult.FallbackEffects;
                result.TouchDeliveryAttackEvents =
                    deliveryResult.AttackEvents;
            }
            else if (heritage.Definition.Id ==
                ElementalHeritageId.Stormsoul)
            {
                result.TouchDeliveryExact = true;
                result.TouchDeliveryTargetable = true;
                result.TouchDeliveryResult = "Success";
                result.TouchDeliveryAttackEvents =
                    result.Command.AttackEvents;
            }

            result.NewCasterBuffs = BuffSummary(NewBuffs(casterBefore,
                caster));
            result.NewTargetBuffs = BuffSummary(NewBuffs(targetBefore,
                target));
            result.CasterSpeedAfter =
                caster.Descriptor.Stats.Speed.ModifiedValue;
            result.TargetDamageAfter = target.Descriptor.Damage;
            result.EffectObserved = NativeEffectObserved(result,
                heritage.Definition.Id);
            RestoreResource(result.Command, caster.Descriptor,
                heritage.SlaResource);
            return result;
        }

        private static UnerringEvidence ExerciseUnerring(
            ElementalRaceBlueprints race,
            ElementalHeritageBlueprints heritage,
            BlueprintCharacterClass fighter, BlueprintFaction actorFaction,
            BlueprintFaction targetFaction, BlueprintFeature attackProbe,
            ICollection<UnitEntityData> created,
            ICollection<UnityEngine.Object> transient,
            ICollection<ItemEntityWeapon> items)
        {
            UnitEntityData caster = CreateUnit(race.Race, actorFaction,
                created, transient, new Vector3(created.Count * 4f, 0f, 0f),
                "Ironsoul_Caster");
            UnitEntityData target = CreateUnit(null, targetFaction, created,
                transient, caster.Position + new Vector3(0f, 0f, 2f),
                "Ironsoul_Target");
            PrepareHeritage(caster, race, heritage, fighter, 20);
            PrepareTarget(target);
            EnsureFact(caster.Descriptor, attackProbe);
            BlueprintItemWeapon shortSword = BlueprintLibraryLookup
                .RequireExact<BlueprintItemWeapon>(BlueprintBootstrap.Library,
                    ShortSwordGuid, "Unerring Weapon native shortsword");
            var primary = new ItemEntityWeapon(shortSword);
            var secondary = new ItemEntityWeapon(shortSword);
            items.Add(primary);
            items.Add(secondary);
            caster.Body.PrimaryHand.InsertItem(primary);
            caster.Body.SecondaryHand.InsertItem(secondary);
            if (!ReferenceEquals(caster.Body.PrimaryHand.MaybeWeapon,
                    primary) ||
                !ReferenceEquals(caster.Body.SecondaryHand.MaybeWeapon,
                    secondary))
                throw new InvalidOperationException(
                    "The Unerring Weapon fixture rejected a held shortsword.");

            BlueprintAbility variant = heritage.SlaAbility.ComponentsArray
                .OfType<AbilityVariants>().Single().Variants[0];
            BlueprintWeaponEnchantment enchantmentBlueprint =
                heritage.AuxiliaryBlueprints.OfType<
                    BlueprintWeaponEnchantment>().Single();
            Func<AbilityData> factory = () =>
            {
                Ability root = caster.Descriptor.Abilities.GetAbility(
                    heritage.SlaAbility);
                if (root == null)
                    throw new InvalidOperationException(
                        "The Ironsoul parent SLA is absent.");
                return new AbilityData(new AbilityData(root), variant);
            };
            var wrapped = new TargetWrapper(caster);
            int primaryBefore = ConfirmationBonus(caster, target, primary);
            int secondaryBefore = ConfirmationBonus(caster, target,
                secondary);
            var result = new UnerringEvidence
            {
                ParentGuid = heritage.SlaAbility.AssetGuid,
                VariantGuid = variant.AssetGuid,
                EnchantmentGuid = enchantmentBlueprint.AssetGuid,
                PrimaryTargetable = factory().CanTarget(wrapped),
                PrimaryConfirmationBefore = primaryBefore,
                SecondaryConfirmationBefore = secondaryBefore,
                ExpectedBonus =
                    ElementalHeritageSlaPolicy.UnerringConfirmationBonus(20)
            };
            Func<bool> observed = () => primary.Enchantments.Any(value =>
                value != null && ReferenceEquals(value.Blueprint,
                    enchantmentBlueprint));
            result.Command = ExerciseCommand(caster, factory,
                heritage.SlaResource, wrapped, caster, observed);
            ItemEnchantment[] primaryEnchantments = primary.Enchantments
                .Where(value => value != null && ReferenceEquals(
                    value.Blueprint, enchantmentBlueprint)).ToArray();
            ItemEnchantment[] secondaryEnchantments = secondary.Enchantments
                .Where(value => value != null && ReferenceEquals(
                    value.Blueprint, enchantmentBlueprint)).ToArray();
            result.PrimaryEnchantmentCount = primaryEnchantments.Length;
            result.SecondaryEnchantmentCount = secondaryEnchantments.Length;
            ItemEnchantment exact = primaryEnchantments.SingleOrDefault();
            result.ExactPrimaryEnchantment = exact != null &&
                ReferenceEquals(exact.Owner, primary) &&
                exact.ParentContext != null &&
                exact.ParentContext.Params.CasterLevel == 20;
            result.DurationSeconds = exact == null ? -1d :
                (exact.EndTime -
                    Game.Instance.TimeController.GameTime).TotalSeconds;
            result.PrimaryConfirmationAfter = ConfirmationBonus(caster,
                target, primary);
            result.SecondaryConfirmationAfter = ConfirmationBonus(caster,
                target, secondary);
            caster.Body.PrimaryHand.RemoveItem(false);
            result.PersistsWhenUnequipped = primary.Enchantments.Any(value =>
                value != null && ReferenceEquals(value.Blueprint,
                    enchantmentBlueprint));
            caster.Body.PrimaryHand.InsertItem(primary);
            RestoreResource(result.Command, caster.Descriptor,
                heritage.SlaResource);
            return result;
        }

        private static ChillTouchEvidence ExerciseChillTouch(
            ElementalRaceBlueprints race,
            ElementalHeritageBlueprints heritage,
            BlueprintCharacterClass fighter, BlueprintFaction actorFaction,
            BlueprintFaction targetFaction, BlueprintFeature attackProbe,
            bool undead, ICollection<UnitEntityData> created,
            ICollection<UnityEngine.Object> transient)
        {
            string label = undead ? "Undead" : "Living";
            UnitEntityData caster = CreateUnit(race.Race, actorFaction,
                created, transient, new Vector3(created.Count * 4f, 0f, 0f),
                "Rimesoul_" + label + "_Caster");
            UnitEntityData target = CreateUnit(null, targetFaction, created,
                transient, caster.Position + new Vector3(0f, 0f, 1.5f),
                "Rimesoul_" + label + "_Target");
            PrepareHeritage(caster, race, heritage, fighter, 20);
            PrepareTarget(target);
            EnsureFact(caster.Descriptor, attackProbe);
            if (undead)
            {
                BlueprintFeature undeadType = BlueprintLibraryLookup
                    .RequireExact<BlueprintFeature>(
                        BlueprintBootstrap.Library, UndeadTypeGuid,
                        "native undead type feature");
                EnsureFact(target.Descriptor, undeadType);
            }
            int damageBefore = target.Descriptor.Damage;
            int strengthBefore =
                target.Descriptor.Stats.Strength.ModifiedValue;
            BlueprintBuff frightenedBlueprint = BlueprintRoot.Instance
                .SystemMechanics.FrightenedBuff;
            var result = new ChillTouchEvidence
            {
                TargetKind = label,
                TargetClassificationExact =
                    target.Descriptor.IsUndead == undead,
                DamageBefore = damageBefore,
                StrengthBefore = strengthBefore
            };
            TargetWrapper wrapped = new TargetWrapper(target);
            Func<AbilityData> factory = () =>
                RequireAbility(caster, heritage.SlaAbility);
            Func<bool> parentObserved = () =>
                caster.Get<UnitPartTouch>() != null ||
                target.Descriptor.Damage != damageBefore ||
                target.Descriptor.Stats.Strength.ModifiedValue !=
                    strengthBefore ||
                ExactBuff(target, frightenedBlueprint) != null;
            result.Command = ExerciseCommand(caster, factory,
                heritage.SlaResource, wrapped, target, parentObserved);
            result.CasterLevel = result.Command.Context == null ? 0 :
                result.Command.Context.Params.CasterLevel;
            UnitPartTouch touch = caster.Get<UnitPartTouch>();
            UnitPartElementalChillTouch state = caster.Get<
                UnitPartElementalChillTouch>();
            result.ParentTouchInstalled = touch != null && state != null;
            result.RemainingBeforeDelivery =
                state == null ? 0 : state.RemainingTouches;
            bool effectAlreadyApplied =
                target.Descriptor.Damage != damageBefore ||
                target.Descriptor.Stats.Strength.ModifiedValue !=
                    strengthBefore ||
                ExactBuff(target, frightenedBlueprint) != null;
            if (!effectAlreadyApplied)
            {
                BlueprintAbility delivery = heritage.AuxiliaryBlueprints
                    .OfType<BlueprintAbility>().Single();
                DeliveryEvidence delivered = ExecuteTouchDelivery(caster,
                    target, delivery, () =>
                        target.Descriptor.Damage != damageBefore ||
                        target.Descriptor.Stats.Strength.ModifiedValue !=
                            strengthBefore ||
                        ExactBuff(target, frightenedBlueprint) != null);
                result.DeliveryAbilityExact = delivered.AbilityExact;
                result.DeliveryTargetable = delivered.Targetable;
                result.DeliveryResult = delivered.Result;
                result.DeliveryFallbackEffects =
                    delivered.FallbackEffects;
                result.TouchControllerInvoked =
                    delivered.TouchControllerInvoked;
                result.RetentionStatePresent =
                    delivered.RetentionStatePresent;
                result.RetentionHeldGuid = delivered.RetentionHeldGuid;
                result.RetentionExecutingGuid =
                    delivered.RetentionExecutingGuid;
                result.RetentionExactMatch =
                    delivered.RetentionExactMatch;
                result.TouchPresentAfterController =
                    delivered.TouchPresentAfterController;
                result.StatePresentAfterController =
                    delivered.StatePresentAfterController;
                result.DeliveryAttackEvents = delivered.AttackEvents;
            }
            else
            {
                result.DeliveryAbilityExact = true;
                result.DeliveryTargetable = true;
                result.DeliveryResult = "Success";
                result.DeliveryAttackEvents = result.Command.AttackEvents;
            }
            state = caster.Get<UnitPartElementalChillTouch>();
            result.RemainingAfterDelivery =
                state == null ? 0 : state.RemainingTouches;
            result.ResourceAfterDelivery = caster.Descriptor.Resources
                .GetResourceAmount(heritage.SlaResource);
            result.DamageAfter = target.Descriptor.Damage;
            result.StrengthAfter =
                target.Descriptor.Stats.Strength.ModifiedValue;
            Buff frightened = ExactBuff(target, frightenedBlueprint);
            result.FrightenedApplied = frightened != null;
            result.FrightenedDurationSeconds = frightened == null ? 0d :
                frightened.TimeLeft.TotalSeconds;
            RestoreResource(result.Command, caster.Descriptor,
                heritage.SlaResource);
            return result;
        }

        private static CommandEvidence ExerciseCommand(
            UnitEntityData caster, Func<AbilityData> dataFactory,
            BlueprintAbilityResource resource, TargetWrapper commandTarget,
            UnitEntityData effectTarget, Func<bool> effectObserved)
        {
            AbilityData data = dataFactory();
            var result = new CommandEvidence
            {
                Targetable = data.CanTarget(commandTarget),
                Available = data.IsAvailable,
                ArcaneFailureInapplicable =
                    !data.IsAffectedByArcaneSpellFailure
            };
            UnitUseAbility canceled = CreateCommand(data, commandTarget,
                caster);
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

            data = dataFactory();
            UnitUseAbility command = CreateCommand(data, commandTarget,
                caster);
            result.ResourceBeforeCast = caster.Descriptor.Resources
                .GetResourceAmount(resource);
            BeginAttackProbe(caster);
            try
            {
                UnityEngine.Random.InitState(FindNativeD20Seed(10));
                object action = InvokeCommandAction(command);
                result.Result = action == null ? string.Empty :
                    action.ToString();
                result.ResourceAfterCast = caster.Descriptor.Resources
                    .GetResourceAmount(resource);
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
                    result.Context = process.Context;
                }
                if (!effectObserved())
                {
                    AbilityExecutionContext execution = result.Context ??
                        data.CreateExecutionContext(commandTarget);
                    UnityEngine.Random.InitState(FindNativeD20Seed(10));
                    result.FallbackEffects += ApplyFallbackEffects(data,
                        execution, effectTarget);
                }
                if (process != null && !process.IsEnded)
                {
                    process.Detach();
                    result.ProcessDetached = true;
                }
                result.ProcessEnded = process != null && process.IsEnded;
                InvokeCommandEnded(command, false);
            }
            finally
            {
                result.AttackEvents = EndAttackProbe();
            }
            result.AvailableAfterCast = data.IsAvailable;
            AbilityData fresh = dataFactory();
            result.FreshAbilityAvailable = fresh.IsAvailable;
            UnitUseAbility second = CreateCommand(fresh, commandTarget,
                caster);
            result.SecondCommandCanStart = second.CanStart;
            result.SecondPlayerPathAvailable = fresh.IsAvailable &&
                result.SecondCommandCanStart;
            result.ResourceAfterSecondGate = caster.Descriptor.Resources
                .GetResourceAmount(resource);
            InvokeCommandEnded(second, true);
            return result;
        }

        private static DeliveryEvidence ExecuteTouchDelivery(
            UnitEntityData caster, UnitEntityData target,
            BlueprintAbility expected, Func<bool> effectObserved)
        {
            UnitPartTouch touch = caster.Get<UnitPartTouch>();
            AbilityData data = touch == null || touch.Ability == null ? null :
                touch.Ability.Data;
            var result = new DeliveryEvidence
            {
                AbilityExact = data != null &&
                    ReferenceEquals(data.Blueprint, expected)
            };
            if (data == null) return result;
            var wrapped = new TargetWrapper(target);
            result.Targetable = data.CanTarget(wrapped);
            UnitUseAbility command = CreateCommand(data, wrapped, caster);
            AbilityExecutionContext execution = null;
            int remainingBefore = RemainingChillTouches(caster);
            BeginAttackProbe(caster);
            try
            {
                UnityEngine.Random.InitState(FindNativeD20Seed(10));
                object action = InvokeCommandAction(command);
                result.Result = action == null ? string.Empty :
                    action.ToString();
                AbilityExecutionProcess process = command.ExecutionProcess;
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
                    execution = process.Context;
                }
                if (!effectObserved())
                {
                    execution = execution ??
                        data.CreateExecutionContext(wrapped);
                    UnityEngine.Random.InitState(FindNativeD20Seed(10));
                    result.FallbackEffects += ApplyFallbackEffects(data,
                        execution, target);
                }
                if (execution != null &&
                    caster.Get<UnitPartTouch>() != null &&
                    (remainingBefore == 0 ||
                     RemainingChillTouches(caster) == remainingBefore))
                {
                    UnitPartTouch retainedTouch = caster.Get<UnitPartTouch>();
                    UnitPartElementalChillTouch retainedState = caster.Get<
                        UnitPartElementalChillTouch>();
                    BlueprintAbility held = retainedTouch == null ||
                        retainedTouch.Ability == null ||
                        retainedTouch.Ability.Data == null ? null :
                        retainedTouch.Ability.Data.Blueprint;
                    BlueprintAbility executing = execution.Ability == null ?
                        null : execution.Ability.Blueprint;
                    result.RetentionStatePresent = retainedState != null;
                    result.RetentionHeldGuid = held == null ? string.Empty :
                        held.AssetGuid;
                    result.RetentionExecutingGuid = executing == null ?
                        string.Empty : executing.AssetGuid;
                    result.RetentionExactMatch = retainedState != null &&
                        retainedState.Matches(held, executing);
                    new TouchSpellsController().OnAbilityEffectApplied(
                        execution);
                    result.TouchControllerInvoked = true;
                    result.TouchPresentAfterController =
                        caster.Get<UnitPartTouch>() != null;
                    result.StatePresentAfterController = caster.Get<
                        UnitPartElementalChillTouch>() != null;
                }
                if (process != null && !process.IsEnded) process.Detach();
                InvokeCommandEnded(command, false);
            }
            finally
            {
                result.AttackEvents = EndAttackProbe();
            }
            return result;
        }

        private static int ApplyFallbackEffects(AbilityData data,
            AbilityExecutionContext context, UnitEntityData effectTarget)
        {
            int count = 0;
            TargetWrapper wrapped = new TargetWrapper(effectTarget);
            foreach (AbilityEffectRunAction effect in data.Blueprint
                .ComponentsArray.OfType<AbilityEffectRunAction>())
            {
                effect.Apply(context, wrapped);
                count++;
            }
            foreach (AbilityEffectStickyTouch sticky in data.Blueprint
                .ComponentsArray.OfType<AbilityEffectStickyTouch>())
            {
                sticky.Apply(context, wrapped);
                count++;
            }
            return count;
        }

        private static void RestoreResource(CommandEvidence result,
            UnitDescriptor owner, BlueprintAbilityResource resource)
        {
            Kingmaker.Controllers.Rest.RestController.ApplyRest(owner);
            result.ResourceAfterRest = owner.Resources.GetResourceAmount(
                resource);
        }

        private static bool NativeDonorGraphExact(
            ElementalHeritageBlueprints heritage, BlueprintAbility donor)
        {
            BlueprintComponent[] expected = (donor.ComponentsArray ??
                Array.Empty<BlueprintComponent>()).Where(
                    ElementalRaceAbilityFactory.IsSafeNativeEffect).ToArray();
            BlueprintComponent[] actual = (heritage.SlaAbility
                .ComponentsArray ?? Array.Empty<BlueprintComponent>())
                .Where(value => !(value is AbilityResourceLogic)).ToArray();
            if (expected.Length != actual.Length ||
                donor.Type != AbilityType.Spell ||
                heritage.SlaAbility.Type != AbilityType.SpellLike ||
                ReferenceEquals(donor, heritage.SlaAbility)) return false;
            for (int index = 0; index < expected.Length; index++)
            {
                if (heritage.Definition.Id ==
                        ElementalHeritageId.Stormsoul &&
                    expected[index] is AbilityEffectStickyTouch)
                {
                    AbilityEffectStickyTouch replacement = actual[index] as
                        AbilityEffectStickyTouch;
                    BlueprintAbility delivery = heritage.AuxiliaryBlueprints
                        .OfType<BlueprintAbility>().SingleOrDefault();
                    if (replacement == null || delivery == null ||
                        ReferenceEquals(expected[index], replacement) ||
                        !ReferenceEquals(replacement.TouchDeliveryAbility,
                            delivery) ||
                        !ReferenceEquals(delivery.Parent,
                            heritage.SlaAbility))
                        return false;
                }
                else if (!ReferenceEquals(expected[index], actual[index]))
                    return false;
            }
            return true;
        }

        private static bool NativeEffectObserved(NativeSlaEvidence value,
            ElementalHeritageId id)
        {
            bool casterBuff = !string.IsNullOrEmpty(value.NewCasterBuffs);
            bool targetBuff = !string.IsNullOrEmpty(value.NewTargetBuffs);
            switch (id)
            {
                case ElementalHeritageId.Lavasoul:
                    return casterBuff;
                case ElementalHeritageId.Sunsoul:
                case ElementalHeritageId.Gemsoul:
                    return targetBuff;
                case ElementalHeritageId.Smokesoul:
                    return casterBuff &&
                        value.CasterSpeedAfter > value.CasterSpeedBefore;
                case ElementalHeritageId.Stormsoul:
                    return value.TouchDeliveryExact &&
                        value.TouchDeliveryTargetable &&
                        string.Equals(value.TouchDeliveryResult, "Success",
                            StringComparison.Ordinal) &&
                        value.TouchDeliveryAttackEvents > 0 &&
                        value.TargetDamageAfter >
                            value.TargetDamageBefore;
                case ElementalHeritageId.Mistsoul:
                    return casterBuff;
                default:
                    return false;
            }
        }

        private static bool IsSelfAbility(ElementalHeritageId id)
        {
            return id == ElementalHeritageId.Lavasoul ||
                id == ElementalHeritageId.Smokesoul ||
                id == ElementalHeritageId.Mistsoul;
        }

        private static Buff[] Buffs(UnitEntityData unit)
        {
            return unit.Descriptor.Buffs.RawFacts.OfType<Buff>().ToArray();
        }

        private static Buff[] NewBuffs(IList<Buff> before,
            UnitEntityData unit)
        {
            return Buffs(unit).Where(value => !before.Any(old =>
                ReferenceEquals(old, value))).ToArray();
        }

        private static string BuffSummary(IEnumerable<Buff> buffs)
        {
            return string.Join(",", buffs.Select(value =>
                value.Blueprint.AssetGuid + ":" +
                (value.Blueprint.name ?? string.Empty)).ToArray());
        }

        private static Buff ExactBuff(UnitEntityData unit,
            BlueprintBuff blueprint)
        {
            return unit.Descriptor.Buffs.RawFacts.OfType<Buff>()
                .SingleOrDefault(value =>
                    ReferenceEquals(value.Blueprint, blueprint));
        }

        private static int RemainingChillTouches(UnitEntityData caster)
        {
            UnitPartElementalChillTouch state = caster.Get<
                UnitPartElementalChillTouch>();
            return state == null ? 0 : state.RemainingTouches;
        }

        private static int ConfirmationBonus(UnitEntityData caster,
            UnitEntityData target, ItemEntityWeapon weapon)
        {
            RuleAttackRoll roll = Rulebook.Trigger(new RuleAttackRoll(caster,
                target, weapon, 0));
            return roll.CriticalConfirmationBonus;
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

        private static void PrepareHeritage(UnitEntityData unit,
            ElementalRaceBlueprints race,
            ElementalHeritageBlueprints heritage,
            BlueprintCharacterClass fighter, int levels)
        {
            UnitDescriptor owner = unit.Descriptor;
            owner.Stats.Strength.BaseValue = 10;
            owner.Stats.Dexterity.BaseValue = 10;
            owner.Stats.Constitution.BaseValue = 10;
            owner.Stats.Intelligence.BaseValue = 10;
            owner.Stats.Wisdom.BaseValue = 10;
            owner.Stats.Charisma.BaseValue = 18;
            EnsureFact(owner, race.Race);
            foreach (BlueprintFeature feature in race.Race.Features)
                EnsureFact(owner, feature);
            EnsureFact(owner, heritage.Marker);
            if (!owner.HasFact(heritage.SlaFeature) ||
                owner.Abilities.GetAbility(heritage.SlaAbility) == null)
                throw new InvalidOperationException(
                    heritage.Definition.Name +
                    " did not become the active SLA provider.");
            Advance(owner, fighter, levels);
        }

        private static void PrepareTarget(UnitEntityData unit)
        {
            unit.Descriptor.Stats.HitPoints.BaseValue = 500;
            unit.Descriptor.Stats.Strength.BaseValue = 10;
            unit.Descriptor.Stats.Dexterity.BaseValue = 10;
            unit.Descriptor.Stats.Constitution.BaseValue = 10;
            unit.Descriptor.Stats.Wisdom.BaseValue = 10;
            unit.Descriptor.Stats.GetStat(StatType.SaveFortitude).BaseValue =
                -100;
            unit.Descriptor.Stats.GetStat(StatType.SaveReflex).BaseValue =
                -100;
            unit.Descriptor.Stats.GetStat(StatType.SaveWill).BaseValue = -100;
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

        private static void EnsureFact(UnitDescriptor owner,
            BlueprintUnitFact blueprint)
        {
            if (owner.HasFact(blueprint)) return;
            if (owner.AddFact(blueprint) == null || !owner.HasFact(blueprint))
                throw new InvalidOperationException(
                    "Disposable unit rejected fact " + blueprint.AssetGuid +
                    ".");
        }

        private static BlueprintFeature CreateAttackProbe(
            ICollection<UnityEngine.Object> transient)
        {
            var feature = ScriptableObject.CreateInstance<BlueprintFeature>();
            feature.name = "KMG_Runtime_ElementalHeritageSla_AttackProbe";
            feature.Ranks = 1;
            feature.IsClassFeature = false;
            feature.HideInUI = true;
            feature.Groups = Array.Empty<FeatureGroup>();
            var probe = ScriptableObject.CreateInstance<
                ElementalHeritageSlaAttackProbe>();
            feature.ComponentsArray = new BlueprintComponent[] { probe };
            transient.Add(feature);
            transient.Add(probe);
            return feature;
        }

        private static void BeginAttackProbe(UnitEntityData caster)
        {
            _attackProbeCaster = caster;
            _attackProbeEvents = 0;
        }

        private static int EndAttackProbe()
        {
            int result = _attackProbeEvents;
            _attackProbeCaster = null;
            _attackProbeEvents = 0;
            return result;
        }

        internal static void ObserveAttack(RuleAttackRoll evt)
        {
            if (evt == null || _attackProbeCaster == null ||
                !ReferenceEquals(evt.Initiator, _attackProbeCaster)) return;
            evt.AutoHit = true;
            evt.AutoMiss = false;
            _attackProbeEvents++;
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
            actor.name = "KMG_Runtime_HeritageSla_ActorFaction";
            target.name = "KMG_Runtime_HeritageSla_TargetFaction";
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
            BlueprintFaction faction, ICollection<UnitEntityData> created,
            ICollection<UnityEngine.Object> transient, Vector3 position,
            string suffix)
        {
            BlueprintUnit donor = BlueprintRoot.Instance
                .DefaultPlayerCharacter;
            if (donor == null)
                throw new InvalidOperationException(
                    "The default character blueprint is unavailable.");
            BlueprintUnit blueprint = UnityEngine.Object.Instantiate(donor);
            blueprint.name = "KMG_Runtime_HeritageSla_" + suffix;
            if (race != null) blueprint.Race = race;
            blueprint.Faction = faction;
            blueprint.Brain = null;
            // AbilityResourceLogic intentionally bypasses spending for cheater
            // units. A player-command resource test must use the ordinary path.
            blueprint.IsCheater = false;
            transient.Add(blueprint);
            UnitEntityData result = new Kingmaker.UI.LevelUp.ChargenUnit(
                blueprint).Unit;
            if (result == null || result.Descriptor == null ||
                result.Descriptor.Resources == null)
                throw new InvalidOperationException(
                    "A disposable heritage SLA unit was unavailable.");
            result.Descriptor.Stats.HitPoints.BaseValue = 500;
            result.Descriptor.State.Immortality.Retain();
            SetExactProperty(result, "Position", position);
            if (!Game.Instance.State.Units.All.Add(result))
            {
                result.Dispose();
                throw new InvalidOperationException(
                    "A disposable heritage SLA unit could not be registered.");
            }
            created.Add(result);
            return result;
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
                    "The native heritage SLA level-up surface is unavailable.");
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
                            "Disposable heritage SLA class selection failed.");
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

        private static int FindNativeD20Seed(int expected)
        {
            for (int seed = 0; seed < 100000; seed++)
            {
                UnityEngine.Random.InitState(seed);
                if (UnityEngine.Random.Range(1, 21) == expected)
                    return seed;
            }
            throw new InvalidOperationException(
                "No deterministic Unity d20 seed produced " + expected + ".");
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

        private static void AddAssertions(
            ICollection<RuntimeTestAssertion> assertions, Evidence evidence)
        {
            Add(assertions, "elemental-heritage-chill-touch-patches",
                "exact project prefixes installed on native apply and removal boundaries",
                evidence.ChillTouchPatchAudit,
                evidence.ChillTouchPatchesInstalled,
                "Harmony 1.2 exact target patch registry");
            Add(assertions, "elemental-heritage-native-sla-count", "6",
                evidence.NativeSlas.Count.ToString(),
                evidence.NativeSlas.Count == 6,
                "six exact native/Owlcat substitute production exercises");
            foreach (NativeSlaEvidence sla in evidence.NativeSlas)
            {
                string key = sla.Heritage.ToLowerInvariant()
                    .Replace(" ", "-");
                string observed = sla.Summary();
                Add(assertions, "elemental-heritage-" + key +
                    "-player-command",
                    "SpellLike; cancellation 1->1; accepted command 1->0; zero-use command blocked; rest ->1",
                    observed, sla.Command != null && sla.Command.Pass() &&
                        string.Equals(sla.AbilityType,
                            AbilityType.SpellLike.ToString(),
                            StringComparison.Ordinal),
                    "native UnitUseAbility, AbilityExecutionProcess, resource gate, and ApplyRest");
                Add(assertions, "elemental-heritage-" + key +
                    "-native-effect",
                    "exact safe donor graph and observable native effect through its production SLA",
                    observed, sla.DonorGraphExact && sla.EffectObserved &&
                        sla.CasterLevel == 5,
                    "exact donor component references plus live buff, speed, damage, or held-touch delivery");
            }
            Add(assertions, "elemental-heritage-unerring-command",
                "accepted primary-hand variant spends exactly one use and rest restores one",
                evidence.UnerringWeapon == null ? "<null>" :
                    evidence.UnerringWeapon.Summary(),
                evidence.UnerringWeapon != null &&
                    evidence.UnerringWeapon.Command != null &&
                    evidence.UnerringWeapon.Command.Pass(),
                "native AbilityVariants/AbilityData and UnitUseAbility");
            Add(assertions, "elemental-heritage-unerring-exact-item",
                "CL20 grants +7 confirmation to exactly the selected primary weapon for 20 rounds and survives unequip",
                evidence.UnerringWeapon == null ? "<null>" :
                    evidence.UnerringWeapon.Summary(),
                evidence.UnerringWeapon != null &&
                    evidence.UnerringWeapon.Pass(),
                "live ContextActionEnchantWornItem, ItemEnchantment, equipment slot, and RuleAttackRoll");
            foreach (ChillTouchEvidence chill in evidence.ChillTouch)
            {
                bool living = string.Equals(chill.TargetKind, "Living",
                    StringComparison.Ordinal);
                Add(assertions, "elemental-heritage-chill-touch-" +
                    chill.TargetKind.ToLowerInvariant(),
                    living
                        ? "CL20 command spends once; exact held-touch delivery deals negative-energy damage and 1 Strength damage on failed Fortitude; 19 touches remain"
                        : "CL20 command spends once; exact held-touch delivery deals no damage to undead and applies 1d4+20 rounds frightened on failed Will; 19 touches remain",
                    chill.Summary(),
                    living ? chill.PassLiving() : chill.PassUndead(),
                    "native UnitUseAbility/AbilityDeliverTouch, exact save and damage rule events, persistent UnitPart state");
            }
            Add(assertions, "elemental-heritage-slas-save-free", "false",
                evidence.SaveStateTouched.ToString(),
                !evidence.SaveStateTouched,
                "request-local units only; no save or player-party mutation");
            Add(assertions, "elemental-heritage-slas-cleanup",
                "exact pre-run global-unit reference sequence",
                evidence.CleanupExact.ToString(), evidence.CleanupExact,
                "finally interruption, item detachment, removal, disposal, and exact comparison");
        }

        private static void Add(
            ICollection<RuntimeTestAssertion> assertions, string name,
            string expected, string observed, bool passed, string source)
        {
            assertions.Add(new RuntimeTestAssertion
            {
                Name = name,
                Expected = expected,
                Observed = observed,
                Status = passed ? RuntimeTestStatuses.Pass :
                    RuntimeTestStatuses.Fail,
                Evidence = source
            });
        }

        private static string DescribeChillTouchPatches(ModContext context,
            out bool installed)
        {
            MethodInfo applied = typeof(TouchSpellsController).GetMethod(
                "OnAbilityEffectApplied", BindingFlags.Instance |
                    BindingFlags.Public, null,
                new[] { typeof(AbilityExecutionContext) }, null);
            MethodInfo removed = typeof(UnitPartTouch).GetMethod("OnRemove",
                BindingFlags.Instance | BindingFlags.Public, null,
                Type.EmptyTypes, null);
            Patches appliedPatches = applied == null ? null :
                context.Harmony.GetPatchInfo(applied);
            Patches removedPatches = removed == null ? null :
                context.Harmony.GetPatchInfo(removed);
            Patch[] appliedPrefixes = appliedPatches == null ?
                new Patch[0] : appliedPatches.Prefixes.ToArray();
            Patch[] removedPrefixes = removedPatches == null ?
                new Patch[0] : removedPatches.Prefixes.ToArray();
            Patch appliedProject = appliedPrefixes.FirstOrDefault(value =>
                value.owner == context.ModId && value.patch != null &&
                value.patch.DeclaringType ==
                    typeof(ElementalChillTouchAppliedPatch));
            bool removedExact = removedPrefixes.Any(value =>
                value.owner == context.ModId && value.patch != null &&
                value.patch.DeclaringType ==
                    typeof(ElementalChillTouchRemovedPatch));
            bool callOfTheWildInstalled = appliedPrefixes.Any(value =>
                string.Equals(value.owner, "CallOfTheWild",
                    StringComparison.Ordinal));
            bool beforeCallOfTheWild = appliedProject != null &&
                appliedProject.before != null &&
                appliedProject.before.Any(value => string.Equals(value,
                    "CallOfTheWild", StringComparison.Ordinal));
            installed = appliedProject != null && removedExact &&
                (!callOfTheWildInstalled || beforeCallOfTheWild);
            return "applied=" + DescribePatches(appliedPrefixes) +
                ";removed=" + DescribePatches(removedPrefixes);
        }

        private static string DescribePatches(IEnumerable<Patch> patches)
        {
            return string.Join("|", patches.Select(value =>
                value.owner + "/" + value.priority + "/before=" +
                string.Join(",", value.before ?? new string[0]) + "/after=" +
                string.Join(",", value.after ?? new string[0]) + "/" +
                (value.patch == null || value.patch.DeclaringType == null ?
                    "<missing>" : value.patch.DeclaringType.FullName + "." +
                        value.patch.Name)).ToArray());
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
            AssemblyMetadataAttribute value = assembly.GetCustomAttributes(
                typeof(AssemblyMetadataAttribute), false)
                .OfType<AssemblyMetadataAttribute>().SingleOrDefault(
                    item => string.Equals(item.Key, key,
                        StringComparison.Ordinal));
            return value == null ? string.Empty : value.Value;
        }

        private static string Hash(string path)
        {
            using (var stream = new FileStream(path, FileMode.Open,
                FileAccess.Read, FileShare.ReadWrite | FileShare.Delete))
            using (var hash = SHA256.Create())
                return BitConverter.ToString(hash.ComputeHash(stream))
                    .Replace("-", string.Empty).ToLowerInvariant();
        }
    }

    public sealed class ElementalHeritageSlaAttackProbe :
        RuleInitiatorLogicComponent<RuleAttackRoll>
    {
        public override void OnEventAboutToTrigger(RuleAttackRoll evt)
        {
            ElementalHeritageSlaScenario.ObserveAttack(evt);
        }

        public override void OnEventDidTrigger(RuleAttackRoll evt) { }
    }
}
