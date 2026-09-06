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
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Blueprints.Root;
using Kingmaker.Controllers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.Enums.Damage;
using Kingmaker.Items;
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
using Kingmaker.UnitLogic.Class.LevelUp;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
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
    /// Save-free live-rule qualification for Scorching Weapons and Inner
    /// Flame through native commands, item facts, attacks, and saving throws.
    /// </summary>
    internal static partial class ElementalIfritFeatScenario
    {
        internal const string EvidenceFileName =
            "elemental-ifrit-feats.json";
        private const string FighterClassGuid =
            "48ac8db94d5de7645906c7d0ad3bcfbd";
        private const string ShortSwordGuid =
            "57c8994d1f1becf49ac4f642e5d8ca9d";
        private const string FlamingEnchantmentGuid =
            "30f90becaaac51f41bf56641966c4121";
        private const string FlareGuid =
            "f0f8e5b9808f44e4eadd22b138131d52";

        private sealed class BlueprintEvidence
        {
            public string FeatureGuid { get; set; }
            public string InnerGuid { get; set; }
            public string AbilityGuid { get; set; }
            public string MarkerGuid { get; set; }
            public string EnchantmentGuid { get; set; }
            public string Action { get; set; }
            public string AbilityType { get; set; }
            public string Range { get; set; }
            public int SaveComponents { get; set; }
            public int DeliveryComponents { get; set; }
            public int DamageComponents { get; set; }
            public bool ExactReferences { get; set; }

            public bool Pass()
            {
                return Action == UnitCommand.CommandType.Swift.ToString() &&
                    AbilityType == Kingmaker.UnitLogic.Abilities.Blueprints
                        .AbilityType.Extraordinary.ToString() &&
                    Range == AbilityRange.Personal.ToString() &&
                    SaveComponents == 1 && DeliveryComponents == 1 &&
                    DamageComponents == 1 && ExactReferences;
            }

            public string Summary()
            {
                return "feature=" + FeatureGuid + ";inner=" + InnerGuid +
                    ";ability=" + AbilityGuid + ";marker=" + MarkerGuid +
                    ";enchantment=" + EnchantmentGuid +
                    ";action/type/range=" + Action + "/" + AbilityType +
                    "/" + Range + ";components=" + SaveComponents + "/" +
                    DeliveryComponents + "/" + DamageComponents +
                    ";references=" + ExactReferences;
            }
        }

        private sealed class ActivationEvidence
        {
            public bool AvailableBefore { get; set; }
            public bool CancelCanStart { get; set; }
            public bool CancelInstalled { get; set; }
            public bool CancelStarted { get; set; }
            public int CancelEffects { get; set; }
            public string CommandResult { get; set; }
            public bool ProcessPresent { get; set; }
            public bool ProcessEnded { get; set; }
            public bool AvailableWhileActive { get; set; }
            public int MarkerCount { get; set; }
            public double MarkerSeconds { get; set; }
            public int PrimaryCount { get; set; }
            public int SecondaryCount { get; set; }
            public double PrimarySeconds { get; set; }
            public double SecondarySeconds { get; set; }
            public bool TemporaryAndUnequipSafe { get; set; }
            public bool PrimarySurvivesUnequip { get; set; }
            public bool SecondarySurvivesUnequip { get; set; }
            public int ReplacementCount { get; set; }
            public int ReplacementFirePackets { get; set; }
            public bool NonIfritRejected { get; set; }

            public bool Pass()
            {
                return AvailableBefore && CancelCanStart && CancelInstalled &&
                    !CancelStarted && CancelEffects == 0 &&
                    CommandResult == "Success" && ProcessPresent &&
                    ProcessEnded && !AvailableWhileActive && MarkerCount == 1 &&
                    MarkerSeconds > 5d && MarkerSeconds <= 7d &&
                    PrimaryCount == 1 && SecondaryCount == 1 &&
                    PrimarySeconds > 5d && PrimarySeconds <= 7d &&
                    SecondarySeconds > 5d && SecondarySeconds <= 7d &&
                    TemporaryAndUnequipSafe && PrimarySurvivesUnequip &&
                    SecondarySurvivesUnequip && ReplacementCount == 0 &&
                    ReplacementFirePackets == 0 && NonIfritRejected;
            }

            public string Summary()
            {
                return "available=" + AvailableBefore + "->" +
                    AvailableWhileActive + ";cancel=" + CancelCanStart + "/" +
                    CancelInstalled + "/" + CancelStarted + "/effects=" +
                    CancelEffects + ";command=" + CommandResult + "/" +
                    ProcessPresent + "/" + ProcessEnded + ";marker=" +
                    MarkerCount + "/" + MarkerSeconds.ToString("F3") +
                    ";items=" + PrimaryCount + "/" + SecondaryCount + "/" +
                    PrimarySeconds.ToString("F3") + "/" +
                    SecondarySeconds.ToString("F3") + "/temporary=" +
                    TemporaryAndUnequipSafe + "/retained=" +
                    PrimarySurvivesUnequip + "/" +
                    SecondarySurvivesUnequip + ";replacement=" +
                    ReplacementCount + "/fire=" + ReplacementFirePackets +
                    ";nonIfritRejected=" + NonIfritRejected;
            }
        }

        private sealed class ClassificationEvidence
        {
            public string Metal { get; set; }
            public bool MetalQualifies { get; set; }
            public string Nonmetal { get; set; }
            public bool NonmetalManufactured { get; set; }
            public bool NonmetalQualifies { get; set; }
            public int NonmetalEnchantments { get; set; }
            public int NonmetalMarker { get; set; }
            public string Natural { get; set; }
            public bool NaturalFlag { get; set; }
            public bool NaturalQualifies { get; set; }
            public int EmptyHandMarker { get; set; }

            public bool Pass()
            {
                return MetalQualifies && NonmetalManufactured &&
                    !NonmetalQualifies && NonmetalEnchantments == 0 &&
                    NonmetalMarker == 1 && NaturalFlag &&
                    !NaturalQualifies && EmptyHandMarker == 1;
            }

            public string Summary()
            {
                return "metal=" + Metal + "/" + MetalQualifies +
                    ";nonmetal=" + Nonmetal + "/manufactured=" +
                    NonmetalManufactured + "/qualifies=" +
                    NonmetalQualifies + "/enchant=" +
                    NonmetalEnchantments + "/marker=" + NonmetalMarker +
                    ";natural=" + Natural + "/" + NaturalFlag + "/" +
                    NaturalQualifies + ";emptyMarker=" + EmptyHandMarker;
            }
        }

        private sealed class DamageEvidence
        {
            public int BasePackets { get; set; }
            public int BasePreRolled { get; set; }
            public int BaseFinal { get; set; }
            public int BaseAfterReplay { get; set; }
            public int BaseResisted { get; set; }
            public int ResistanceReduction { get; set; }
            public int InnerPackets { get; set; }
            public int InnerDiceCount { get; set; }
            public string InnerDiceType { get; set; }
            public bool InnerPreRolledAbsent { get; set; }
            public int InnerFinal { get; set; }
            public int FlamingPackets { get; set; }
            public int FlamingDiceCount { get; set; }
            public string FlamingDiceType { get; set; }
            public bool FlamingProjectFlatAbsent { get; set; }

            public bool BasePass()
            {
                return BasePackets == 1 && BasePreRolled == 1 &&
                    BaseFinal == 1 && BaseAfterReplay == 1 &&
                    BaseResisted == 0 && ResistanceReduction >= 1;
            }

            public bool InnerPass()
            {
                return InnerPackets == 1 && InnerDiceCount == 1 &&
                    InnerDiceType == DiceType.D6.ToString() &&
                    InnerPreRolledAbsent && InnerFinal >= 1 && InnerFinal <= 6;
            }

            public bool NonstackPass()
            {
                return FlamingPackets == 1 && FlamingDiceCount == 1 &&
                    FlamingDiceType == DiceType.D6.ToString() &&
                    FlamingProjectFlatAbsent;
            }

            public string Summary()
            {
                return "base=" + BasePackets + "/pre=" + BasePreRolled +
                    "/final=" + BaseFinal + "/replay=" + BaseAfterReplay +
                    "/resisted=" + BaseResisted + "/reduction=" +
                    ResistanceReduction + ";inner=" + InnerPackets + "/" +
                    InnerDiceCount + "x" + InnerDiceType + "/preAbsent=" +
                    InnerPreRolledAbsent + "/final=" + InnerFinal +
                    ";flaming=" + FlamingPackets + "/" +
                    FlamingDiceCount + "x" + FlamingDiceType +
                    "/flatAbsent=" + FlamingProjectFlatAbsent;
            }
        }

        private sealed class SaveEvidence
        {
            public string ControlGuid { get; set; }
            public int BaseControl { get; set; }
            public int BaseFire { get; set; }
            public int BaseLight { get; set; }
            public int BaseOverlap { get; set; }
            public int BaseProjectSla { get; set; }
            public int BaseDirectFire { get; set; }
            public int InnerControl { get; set; }
            public int InnerFire { get; set; }

            public bool BasePass()
            {
                return BaseFire == BaseControl + 2 &&
                    BaseLight == BaseControl + 2 &&
                    BaseOverlap == BaseControl + 2 &&
                    BaseProjectSla == BaseControl &&
                    BaseDirectFire == BaseControl + 2;
            }

            public bool InnerPass()
            {
                return InnerFire == InnerControl + 4;
            }

            public string Summary()
            {
                return "control=" + ControlGuid + "/" + BaseControl +
                    ";fire=" + BaseFire + ";light=" + BaseLight +
                    ";overlap=" + BaseOverlap + ";projectSla=" +
                    BaseProjectSla + ";directFire=" + BaseDirectFire +
                    ";inner=" + InnerControl + "->" + InnerFire;
            }
        }

        private sealed class Evidence
        {
            public int SchemaVersion { get; set; }
            public bool ModuleActive { get; set; }
            public bool SaveStateTouched { get; set; }
            public BlueprintEvidence Blueprint { get; set; }
            public ActivationEvidence Activation { get; set; }
            public ClassificationEvidence Classification { get; set; }
            public DamageEvidence Damage { get; set; }
            public SaveEvidence Saves { get; set; }
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
                ModuleActive = context.FeatureModules.Active.ElementalRaces,
                SaveStateTouched = false
            };
            var created = new List<UnitEntityData>();
            var transient = new List<UnityEngine.Object>();
            var items = new List<ItemEntity>();
            UnitEntityData[] unitsBefore = Game.Instance.State.Units.All
                .ToArray();
            string stage = "resolve-production-contract";
            string exceptionSummary = string.Empty;
            try
            {
                ElementalRaceBlueprintSet races = BlueprintBootstrap
                    .ElementalRaces;
                ElementalFeatBlueprintSet feats = BlueprintBootstrap
                    .ElementalFeats;
                if (races == null || feats == null)
                    throw new InvalidOperationException(
                        "The Elemental Race and Feat graphs are unavailable.");
                BlueprintCharacterClass fighter = BlueprintLibraryLookup
                    .RequireExact<BlueprintCharacterClass>(
                        BlueprintBootstrap.Library, FighterClassGuid,
                        "Ifrit feat Fighter fixture");
                BlueprintItemWeapon shortSword = BlueprintLibraryLookup
                    .RequireExact<BlueprintItemWeapon>(
                        BlueprintBootstrap.Library, ShortSwordGuid,
                        "Scorching Weapons native shortsword");
                BlueprintWeaponEnchantment flaming = BlueprintLibraryLookup
                    .RequireExact<BlueprintWeaponEnchantment>(
                        BlueprintBootstrap.Library, FlamingEnchantmentGuid,
                        "Scorching Weapons native Flaming control");
                BlueprintAbility flare = BlueprintLibraryLookup
                    .RequireExact<BlueprintAbility>(
                        BlueprintBootstrap.Library, FlareGuid,
                        "Scorching Weapons native light spell");
                BlueprintAbility ordinaryControl = FindOrdinaryControl();
                BlueprintItemWeapon nonmetal = FindNonmetalWeapon();
                BlueprintItemWeapon natural = FindNaturalWeapon();

                BlueprintFeature scorching = feats.RequireFeature(
                    ElementalFeatId.ScorchingWeapons);
                BlueprintFeature inner = feats.RequireFeature(
                    ElementalFeatId.InnerFlame);
                BlueprintAbility ability = GrantedAbility(scorching);
                ElementalScorchingWeaponsAbilityLogic delivery = ability
                    .ComponentsArray.OfType<
                        ElementalScorchingWeaponsAbilityLogic>().Single();
                BlueprintBuff marker = delivery.Marker;
                BlueprintWeaponEnchantment enchantment = delivery
                    .WeaponEnchantment;
                ElementalScorchingWeaponsSaveBonus saveComponent = scorching
                    .ComponentsArray.OfType<
                        ElementalScorchingWeaponsSaveBonus>().Single();
                ElementalScorchingWeaponsDamage damageComponent = enchantment
                    .ComponentsArray.OfType<
                        ElementalScorchingWeaponsDamage>().Single();
                evidence.Blueprint = BlueprintContract(races.Ifrit.Race,
                    scorching, inner, ability, marker, enchantment, delivery,
                    saveComponent, damageComponent);

                stage = "base-command-snapshot-damage-saves";
                UnitEntityData baseIfrit = CreateUnit(races.Ifrit.Race,
                    created, transient, "Base");
                UnitEntityData target = CreateUnit(null, created, transient,
                    "Target");
                ApplyRaceFacts(baseIfrit.Descriptor, races.Ifrit);
                Advance(baseIfrit.Descriptor, fighter, 7);
                EnsureFact(baseIfrit.Descriptor, scorching);
                var primary = new ItemEntityWeapon(shortSword);
                var secondary = new ItemEntityWeapon(shortSword);
                var replacement = new ItemEntityWeapon(shortSword);
                items.Add(primary);
                items.Add(secondary);
                items.Add(replacement);
                baseIfrit.Body.PrimaryHand.InsertItem(primary);
                baseIfrit.Body.SecondaryHand.InsertItem(secondary);
                RequireHeld(baseIfrit, primary, secondary);
                evidence.Activation = ExerciseActivation(baseIfrit, target,
                    ability, marker, enchantment, primary, secondary,
                    replacement, scorching, created, transient);
                evidence.Damage = ExerciseBaseDamage(baseIfrit, target,
                    primary, enchantment, races.Ifrit.Resistance);
                BlueprintAbility burningHands = BlueprintLibraryLookup
                    .RequireExact<BlueprintAbility>(BlueprintBootstrap.Library,
                        ElementalRaceIdentityCatalog.BurningHandsGuid,
                        "Scorching Weapons native fire spell");
                evidence.Saves = ExerciseSaves(baseIfrit, target, inner,
                    ordinaryControl, burningHands, flare,
                    races.Ifrit.Heritages.Require(
                        ElementalHeritageId.Sunsoul).SlaAbility, transient);

                stage = "classification";
                evidence.Classification = ExerciseClassification(
                    races.Ifrit, scorching, ability, marker, enchantment,
                    shortSword, nonmetal, natural, fighter, created, transient,
                    items);

                stage = "inner-flame-damage-and-save";
                UnitEntityData innerIfrit = CreateUnit(races.Ifrit.Race,
                    created, transient, "Inner");
                ApplyRaceFacts(innerIfrit.Descriptor, races.Ifrit);
                Advance(innerIfrit.Descriptor, fighter, 7);
                EnsureFact(innerIfrit.Descriptor, scorching);
                EnsureFact(innerIfrit.Descriptor, inner);
                var innerWeapon = new ItemEntityWeapon(shortSword);
                items.Add(innerWeapon);
                innerIfrit.Body.PrimaryHand.InsertItem(innerWeapon);
                if (!ReferenceEquals(innerIfrit.Body.PrimaryHand.MaybeWeapon,
                        innerWeapon))
                    throw new InvalidOperationException(
                        "The Inner Flame fixture rejected its shortsword.");
                Execute(innerIfrit, ability);
                ExerciseInnerDamage(innerIfrit, target, innerWeapon, flaming,
                    evidence.Damage);
                evidence.Saves.InnerControl = SaveWithAbility(innerIfrit,
                    target, ordinaryControl);
                evidence.Saves.InnerFire = SaveWithAbility(innerIfrit,
                    target, burningHands);
            }
            catch (Exception exception)
            {
                exceptionSummary = "stage=" + stage + ";" + exception;
                diagnostics.Add(exceptionSummary);
            }
            finally
            {
                foreach (UnitEntityData unit in created.AsEnumerable()
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
            evidenceFiles.Add(path);
            diagnostics.Add("elementalIfritFeatSha256=" + Hash(path));
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

        private static BlueprintEvidence BlueprintContract(
            BlueprintRace ifrit, BlueprintFeature scorching,
            BlueprintFeature inner, BlueprintAbility ability,
            BlueprintBuff marker, BlueprintWeaponEnchantment enchantment,
            ElementalScorchingWeaponsAbilityLogic delivery,
            ElementalScorchingWeaponsSaveBonus save,
            ElementalScorchingWeaponsDamage damage)
        {
            return new BlueprintEvidence
            {
                FeatureGuid = scorching.AssetGuid,
                InnerGuid = inner.AssetGuid,
                AbilityGuid = ability.AssetGuid,
                MarkerGuid = marker == null ? string.Empty : marker.AssetGuid,
                EnchantmentGuid = enchantment == null ? string.Empty :
                    enchantment.AssetGuid,
                Action = ability.ActionType.ToString(),
                AbilityType = ability.Type.ToString(),
                Range = ability.Range.ToString(),
                SaveComponents = scorching.ComponentsArray.OfType<
                    ElementalScorchingWeaponsSaveBonus>().Count(),
                DeliveryComponents = ability.ComponentsArray.OfType<
                    ElementalScorchingWeaponsAbilityLogic>().Count(),
                DamageComponents = enchantment.ComponentsArray.OfType<
                    ElementalScorchingWeaponsDamage>().Count(),
                ExactReferences = ReferenceEquals(delivery.Ifrit, ifrit) &&
                    ReferenceEquals(delivery.Marker, marker) &&
                    ReferenceEquals(delivery.WeaponEnchantment, enchantment) &&
                    ReferenceEquals(save.InnerFlame, inner) &&
                    ReferenceEquals(damage.InnerFlame, inner)
            };
        }

        private static ActivationEvidence ExerciseActivation(
            UnitEntityData unit, UnitEntityData target,
            BlueprintAbility ability, BlueprintBuff marker,
            BlueprintWeaponEnchantment enchantment,
            ItemEntityWeapon primary, ItemEntityWeapon secondary,
            ItemEntityWeapon replacement, BlueprintFeature scorching,
            ICollection<UnitEntityData> created,
            ICollection<UnityEngine.Object> transient)
        {
            Ability fact = unit.Descriptor.Abilities.GetAbility(ability);
            if (fact == null)
                throw new InvalidOperationException(
                    "Scorching Weapons did not grant its activation ability.");
            var result = new ActivationEvidence();
            var data = new AbilityData(fact);
            result.AvailableBefore = data.IsAvailable;
            UnitUseAbility canceled = CreateCommand(data,
                new TargetWrapper(unit), unit);
            result.CancelCanStart = canceled.CanStart;
            unit.Commands.Run(canceled);
            result.CancelInstalled = unit.Commands.Contains(canceled);
            result.CancelStarted = canceled.IsStarted;
            unit.Commands.InterruptAll(true);
            unit.Commands.RemoveFinishedAndUpdateQueue();
            result.CancelEffects = CountBuff(unit, marker) +
                CountEnchantment(primary, enchantment) +
                CountEnchantment(secondary, enchantment);

            ExecuteEvidence execution = Execute(unit, ability);
            result.CommandResult = execution.CommandResult;
            result.ProcessPresent = execution.ProcessPresent;
            result.ProcessEnded = execution.ProcessEnded;
            result.AvailableWhileActive = new AbilityData(fact).IsAvailable;
            Buff active = unit.Descriptor.Buffs.GetBuff(marker);
            result.MarkerCount = CountBuff(unit, marker);
            result.MarkerSeconds = active == null ? 0d :
                active.TimeLeft.TotalSeconds;
            ItemEnchantment primaryEffect = ExactEnchantment(primary,
                enchantment);
            ItemEnchantment secondaryEffect = ExactEnchantment(secondary,
                enchantment);
            result.PrimaryCount = CountEnchantment(primary, enchantment);
            result.SecondaryCount = CountEnchantment(secondary, enchantment);
            TimeSpan now = Game.Instance.TimeController.GameTime;
            result.PrimarySeconds = primaryEffect == null ? 0d :
                (primaryEffect.EndTime - now).TotalSeconds;
            result.SecondarySeconds = secondaryEffect == null ? 0d :
                (secondaryEffect.EndTime - now).TotalSeconds;
            result.TemporaryAndUnequipSafe = primaryEffect != null &&
                secondaryEffect != null && primaryEffect.IsTemporary &&
                secondaryEffect.IsTemporary &&
                !primaryEffect.RemoveOnUnequipItem &&
                !secondaryEffect.RemoveOnUnequipItem;

            unit.Body.PrimaryHand.RemoveItem(false);
            result.PrimarySurvivesUnequip = CountEnchantment(primary,
                enchantment) == 1;
            unit.Body.PrimaryHand.InsertItem(primary);
            unit.Body.SecondaryHand.RemoveItem(false);
            result.SecondarySurvivesUnequip = CountEnchantment(secondary,
                enchantment) == 1;
            unit.Body.SecondaryHand.InsertItem(replacement);
            result.ReplacementCount = CountEnchantment(replacement,
                enchantment);
            result.ReplacementFirePackets = FirePackets(AutoHit(unit, target,
                replacement)).Length;

            UnitEntityData outsider = CreateUnit(null, created, transient,
                "NonIfrit");
            EnsureFact(outsider.Descriptor, scorching);
            Ability outsiderFact = outsider.Descriptor.Abilities.GetAbility(
                ability);
            result.NonIfritRejected = outsiderFact != null &&
                !new AbilityData(outsiderFact).IsAvailable;
            return result;
        }

        private sealed class ExecuteEvidence
        {
            public string CommandResult { get; set; }
            public bool ProcessPresent { get; set; }
            public bool ProcessEnded { get; set; }
        }

        private static ExecuteEvidence Execute(UnitEntityData unit,
            BlueprintAbility ability)
        {
            Ability fact = unit.Descriptor.Abilities.GetAbility(ability);
            if (fact == null)
                throw new InvalidOperationException(
                    "The requested activation ability is absent.");
            var data = new AbilityData(fact);
            if (!data.IsAvailable)
                throw new InvalidOperationException(
                    "The requested activation ability is unavailable.");
            UnitUseAbility command = CreateCommand(data,
                new TargetWrapper(unit), unit);
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
            return new ExecuteEvidence
            {
                CommandResult = commandResult == null ? string.Empty :
                    commandResult.ToString(),
                ProcessPresent = process != null,
                ProcessEnded = process != null && process.IsEnded
            };
        }

        private static DamageEvidence ExerciseBaseDamage(
            UnitEntityData attacker, UnitEntityData target,
            ItemEntityWeapon weapon,
            BlueprintWeaponEnchantment enchantment,
            BlueprintFeature resistance)
        {
            if (CountEnchantment(weapon, enchantment) != 1)
                throw new InvalidOperationException(
                    "The base damage weapon lost its snapshot enchantment.");
            var result = new DamageEvidence();
            RuleAttackWithWeapon attack = AutoHit(attacker, target, weapon);
            EnergyDamage[] packets = FirePackets(attack);
            result.BasePackets = packets.Length;
            result.BasePreRolled = packets.Length == 1 ?
                packets[0].PreRolledValue.GetValueOrDefault(-1) : -1;
            result.BaseFinal = FinalDamage(attack, packets);
            Rulebook.Trigger(new RulePrepareDamage(attack.MeleeDamage));
            result.BaseAfterReplay = FirePackets(attack).Length;

            EnsureFact(target.Descriptor, resistance);
            RuleAttackWithWeapon resisted = AutoHit(attacker, target, weapon);
            EnergyDamage[] resistedPackets = FirePackets(resisted);
            result.BaseResisted = FinalDamage(resisted, resistedPackets);
            result.ResistanceReduction = resisted.MeleeDamage.ResultDamage
                .Where(value => resistedPackets.Any(packet =>
                    ReferenceEquals(value.Source, packet)))
                .Sum(value => value.Reduction);
            target.Descriptor.RemoveFact(resistance);
            return result;
        }

        private static void ExerciseInnerDamage(UnitEntityData attacker,
            UnitEntityData target, ItemEntityWeapon weapon,
            BlueprintWeaponEnchantment flaming, DamageEvidence result)
        {
            RuleAttackWithWeapon attack = AutoHit(attacker, target, weapon);
            EnergyDamage[] packets = FirePackets(attack);
            result.InnerPackets = packets.Length;
            result.InnerDiceCount = packets.Length == 1 ?
                packets[0].Dice.Rolls : -1;
            result.InnerDiceType = packets.Length == 1 ?
                packets[0].Dice.Dice.ToString() : string.Empty;
            result.InnerPreRolledAbsent = packets.Length == 1 &&
                !packets[0].PreRolledValue.HasValue;
            result.InnerFinal = FinalDamage(attack, packets);

            ItemEnchantment native = weapon.AddEnchantment(flaming, null,
                null);
            if (native == null)
                throw new InvalidOperationException(
                    "The native Flaming control could not be installed.");
            RuleAttackWithWeapon nonstack = AutoHit(attacker, target, weapon);
            EnergyDamage[] nonstackPackets = FirePackets(nonstack);
            result.FlamingPackets = nonstackPackets.Length;
            result.FlamingDiceCount = nonstackPackets.Length == 1 ?
                nonstackPackets[0].Dice.Rolls : -1;
            result.FlamingDiceType = nonstackPackets.Length == 1 ?
                nonstackPackets[0].Dice.Dice.ToString() : string.Empty;
            result.FlamingProjectFlatAbsent = nonstackPackets.All(value =>
                value.PreRolledValue.GetValueOrDefault() != 1 ||
                value.Dice.Rolls != 0);
            weapon.RemoveEnchantment(native);
        }

        private static SaveEvidence ExerciseSaves(UnitEntityData source,
            UnitEntityData target, BlueprintFeature inner,
            BlueprintAbility ordinaryControl, BlueprintAbility fire,
            BlueprintAbility light, BlueprintAbility projectSla,
            ICollection<UnityEngine.Object> transient)
        {
            if (source.Descriptor.HasFact(inner))
                source.Descriptor.RemoveFact(inner);
            BlueprintAbility overlap = UnityEngine.Object.Instantiate(light);
            overlap.name = "KMG_Runtime_ScorchingWeapons_FireLightOverlap";
            overlap.Type = AbilityType.Spell;
            var descriptor = ScriptableObject.CreateInstance<
                SpellDescriptorComponent>();
            descriptor.Descriptor = SpellDescriptor.Fire;
            overlap.ComponentsArray = (overlap.ComponentsArray ??
                Array.Empty<BlueprintComponent>())
                .Concat(new BlueprintComponent[] { descriptor }).ToArray();
            transient.Add(overlap);
            return new SaveEvidence
            {
                ControlGuid = ordinaryControl.AssetGuid,
                BaseControl = SaveWithAbility(source, target,
                    ordinaryControl),
                BaseFire = SaveWithAbility(source, target, fire),
                BaseLight = SaveWithAbility(source, target, light),
                BaseOverlap = SaveWithAbility(source, target, overlap),
                BaseProjectSla = SaveWithAbility(source, target, projectSla),
                BaseDirectFire = SaveWithDirectFire(source, target)
            };
        }

        private static int SaveWithAbility(UnitEntityData source,
            UnitEntityData target, BlueprintAbility ability)
        {
            var mechanics = new MechanicsContext(target, target.Descriptor,
                ability, null, new TargetWrapper(source));
            var saving = new RuleSavingThrow(source, SavingThrowType.Reflex,
                100) { Reason = mechanics };
            mechanics.TriggerRule(saving);
            return saving.StatValue;
        }

        private static int SaveWithDirectFire(UnitEntityData source,
            UnitEntityData target)
        {
            var damage = new RuleDealDamage(target, source,
                new DamageBundle(new EnergyDamage(
                    new DiceFormula(0, DiceType.D6),
                    DamageEnergyType.Fire) { PreRolledValue = 1 }));
            var saving = new RuleSavingThrow(source, SavingThrowType.Reflex,
                100) { Reason = damage };
            Rulebook.Trigger(saving);
            return saving.StatValue;
        }

        private static ClassificationEvidence ExerciseClassification(
            ElementalRaceBlueprints race, BlueprintFeature scorching,
            BlueprintAbility ability, BlueprintBuff marker,
            BlueprintWeaponEnchantment enchantment,
            BlueprintItemWeapon metal, BlueprintItemWeapon nonmetal,
            BlueprintItemWeapon natural, BlueprintCharacterClass fighter,
            ICollection<UnitEntityData> created,
            ICollection<UnityEngine.Object> transient,
            ICollection<ItemEntity> items)
        {
            var metalItem = new ItemEntityWeapon(metal);
            var nonmetalItem = new ItemEntityWeapon(nonmetal);
            var naturalItem = new ItemEntityWeapon(natural);
            items.Add(metalItem);
            items.Add(nonmetalItem);
            items.Add(naturalItem);

            UnitEntityData nonmetalIfrit = CreateUnit(race.Race, created,
                transient, "Nonmetal");
            ApplyRaceFacts(nonmetalIfrit.Descriptor, race);
            Advance(nonmetalIfrit.Descriptor, fighter, 1);
            EnsureFact(nonmetalIfrit.Descriptor, scorching);
            nonmetalIfrit.Body.PrimaryHand.InsertItem(nonmetalItem);
            if (!ReferenceEquals(nonmetalIfrit.Body.PrimaryHand.MaybeWeapon,
                    nonmetalItem))
                throw new InvalidOperationException(
                    "The native nonmetal weapon could not be held.");
            Execute(nonmetalIfrit, ability);

            UnitEntityData emptyIfrit = CreateUnit(race.Race, created,
                transient, "EmptyHands");
            ApplyRaceFacts(emptyIfrit.Descriptor, race);
            EnsureFact(emptyIfrit.Descriptor, scorching);
            Execute(emptyIfrit, ability);

            return new ClassificationEvidence
            {
                Metal = metal.AssetGuid + "/" + metal.Category,
                MetalQualifies = ElementalScorchingWeaponsAbilityLogic
                    .IsManufacturedMetalWeapon(metalItem),
                Nonmetal = nonmetal.AssetGuid + "/" + nonmetal.Category,
                NonmetalManufactured = !nonmetal.IsNatural &&
                    !nonmetal.IsUnarmed,
                NonmetalQualifies = ElementalScorchingWeaponsAbilityLogic
                    .IsManufacturedMetalWeapon(nonmetalItem),
                NonmetalEnchantments = CountEnchantment(nonmetalItem,
                    enchantment),
                NonmetalMarker = CountBuff(nonmetalIfrit, marker),
                Natural = natural.AssetGuid + "/" + natural.Category,
                NaturalFlag = natural.IsNatural,
                NaturalQualifies = ElementalScorchingWeaponsAbilityLogic
                    .IsManufacturedMetalWeapon(naturalItem),
                EmptyHandMarker = CountBuff(emptyIfrit, marker)
            };
        }

        private static BlueprintAbility FindOrdinaryControl()
        {
            BlueprintAbility[] candidates = BlueprintBootstrap.Library
                .GetAllBlueprints().OfType<BlueprintAbility>()
                .Where(value => value != null && value.Type ==
                    AbilityType.Spell && value.Parent == null &&
                    string.Equals(value.name, "MageArmor",
                        StringComparison.Ordinal) &&
                    (value.SpellDescriptor & SpellDescriptor.Fire) == 0 &&
                    !ElementalFeatPolicy.IsExactNativeLightSpellGuid(
                        value.AssetGuid)).ToArray();
            if (candidates.Length != 1)
                throw new InvalidOperationException(
                    "The exact native Mage Armor control is ambiguous: " +
                    candidates.Length + ".");
            return candidates[0];
        }

        private static BlueprintItemWeapon FindNonmetalWeapon()
        {
            BlueprintItemWeapon result = BlueprintBootstrap.Library
                .GetAllBlueprints().OfType<BlueprintItemWeapon>()
                .Where(value => value != null && !value.IsNatural &&
                    !value.IsUnarmed && !value.Category.HasSubCategory(
                        WeaponSubCategory.Metal) &&
                    !string.IsNullOrEmpty(value.AssetGuid))
                .OrderBy(value => value.AssetGuid,
                    StringComparer.Ordinal).FirstOrDefault();
            if (result == null)
                throw new InvalidOperationException(
                    "No native manufactured nonmetal weapon was available.");
            return result;
        }

        private static BlueprintItemWeapon FindNaturalWeapon()
        {
            BlueprintItemWeapon result = BlueprintBootstrap.Library
                .GetAllBlueprints().OfType<BlueprintItemWeapon>()
                .Where(value => value != null && value.IsNatural &&
                    !string.IsNullOrEmpty(value.AssetGuid))
                .OrderBy(value => value.AssetGuid,
                    StringComparer.Ordinal).FirstOrDefault();
            if (result == null)
                throw new InvalidOperationException(
                    "No native natural weapon control was available.");
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

        private static void RequireHeld(UnitEntityData unit,
            ItemEntityWeapon primary, ItemEntityWeapon secondary)
        {
            if (!ReferenceEquals(unit.Body.PrimaryHand.MaybeWeapon, primary) ||
                !ReferenceEquals(unit.Body.SecondaryHand.MaybeWeapon,
                    secondary))
                throw new InvalidOperationException(
                    "The two-weapon Scorching Weapons fixture was rejected.");
        }

        private static RuleAttackWithWeapon AutoHit(UnitEntityData attacker,
            UnitEntityData target, ItemEntityWeapon weapon)
        {
            int damage = target.Descriptor.Damage;
            var result = Rulebook.Trigger(new RuleAttackWithWeapon(attacker,
                target, weapon, 0) { AutoHit = true });
            target.Descriptor.Damage = damage;
            if (result.AttackRoll == null || !result.AttackRoll.IsHit ||
                result.MeleeDamage == null)
                throw new InvalidOperationException(
                    "The native Ifrit feat weapon attack did not resolve.");
            return result;
        }

        private static EnergyDamage[] FirePackets(
            RuleAttackWithWeapon attack)
        {
            return attack == null || attack.MeleeDamage == null ?
                Array.Empty<EnergyDamage>() : attack.MeleeDamage.DamageBundle
                    .OfType<EnergyDamage>().Where(value =>
                        value.EnergyType == DamageEnergyType.Fire).ToArray();
        }

        private static int FinalDamage(RuleAttackWithWeapon attack,
            ICollection<EnergyDamage> packets)
        {
            return attack == null || attack.MeleeDamage == null ||
                attack.MeleeDamage.ResultDamage == null ? 0 :
                attack.MeleeDamage.ResultDamage.Where(value => packets.Any(
                    packet => ReferenceEquals(value.Source, packet)))
                    .Sum(value => value.FinalValue);
        }

        private static ItemEnchantment ExactEnchantment(
            ItemEntityWeapon weapon, BlueprintWeaponEnchantment blueprint)
        {
            return weapon.Enchantments.SingleOrDefault(value => value != null &&
                !value.IsEnded && ReferenceEquals(value.Blueprint,
                    blueprint));
        }

        private static int CountEnchantment(ItemEntityWeapon weapon,
            BlueprintWeaponEnchantment blueprint)
        {
            return weapon == null ? 0 : weapon.Enchantments.Count(value =>
                value != null && !value.IsEnded &&
                ReferenceEquals(value.Blueprint, blueprint));
        }

        private static int CountBuff(UnitEntityData unit,
            BlueprintBuff blueprint)
        {
            return unit.Descriptor.Buffs.RawFacts.OfType<Buff>().Count(value =>
                ReferenceEquals(value.Blueprint, blueprint));
        }

        private static void ApplyRaceFacts(UnitDescriptor owner,
            ElementalRaceBlueprints race)
        {
            EnsureFact(owner, race.Race);
            foreach (BlueprintFeature feature in race.Race.Features)
                EnsureFact(owner, feature);
        }

        private static void EnsureFact(UnitDescriptor owner,
            BlueprintUnitFact blueprint)
        {
            if (owner.HasFact(blueprint)) return;
            if (owner.AddFact(blueprint) == null || !owner.HasFact(blueprint))
                throw new InvalidOperationException(
                    "Request-local unit rejected fact " +
                    blueprint.AssetGuid + ".");
        }

        private static UnitEntityData CreateUnit(BlueprintRace race,
            ICollection<UnitEntityData> created,
            ICollection<UnityEngine.Object> transient, string suffix)
        {
            BlueprintUnit blueprint = UnityEngine.Object.Instantiate(
                BlueprintRoot.Instance.DefaultPlayerCharacter);
            blueprint.name = "KMG_Runtime_ElementalIfritFeat_" + suffix;
            if (race != null) blueprint.Race = race;
            blueprint.Brain = null;
            blueprint.IsCheater = false;
            transient.Add(blueprint);
            UnitEntityData result = new ChargenUnit(blueprint).Unit;
            if (result == null || result.Descriptor == null ||
                (race != null && !ReferenceEquals(
                    result.Descriptor.Progression.Race, race)))
                throw new InvalidOperationException(
                    "A request-local Ifrit feat unit was unavailable.");
            result.Descriptor.Stats.HitPoints.BaseValue = 500;
            result.Descriptor.State.Immortality.Retain();
            if (!Game.Instance.State.Units.All.Add(result))
            {
                result.Dispose();
                throw new InvalidOperationException(
                    "A request-local Ifrit feat unit could not be registered.");
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
                    "The native Ifrit feat level-up surface is unavailable.");
            object charGen = Enum.Parse(start.GetParameters()[4]
                .ParameterType, "CharGen", false);
            object controller = null;
            try
            {
                for (int index = 0; index < levels; index++)
                {
                    controller = start.Invoke(null, new object[]
                        { owner, false, null, null, charGen });
                    if (!(bool)select.Invoke(controller,
                            new object[] { characterClass, false }))
                        throw new InvalidOperationException(
                            "Ifrit feat class selection failed.");
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

        private static void AddAssertions(
            ICollection<RuntimeTestAssertion> assertions, Evidence evidence,
            RuntimeTestRequest request, ModContext context)
        {
            Add(assertions, "elemental-ifrit-feat-module-active", "true",
                evidence.ModuleActive.ToString(), evidence.ModuleActive,
                "active feature-module snapshot");
            Add(assertions, "scorching-weapons-blueprint-contract",
                "one exact save/delivery/damage component; swift extraordinary personal",
                evidence.Blueprint == null ? "<missing>" :
                    evidence.Blueprint.Summary(),
                evidence.Blueprint != null && evidence.Blueprint.Pass(),
                "live registered production blueprint graph");
            Add(assertions, "scorching-weapons-player-command-and-snapshot",
                "cancel has no effects; accepted command snapshots two exact items without transfer",
                evidence.Activation == null ? "<missing>" :
                    evidence.Activation.Summary(),
                evidence.Activation != null && evidence.Activation.Pass(),
                "native UnitUseAbility, ItemEntityWeapon, and swap boundaries");
            Add(assertions, "scorching-weapons-classification",
                "manufactured metal only; nonmetal, natural, and empty-hand controls excluded",
                evidence.Classification == null ? "<missing>" :
                    evidence.Classification.Summary(),
                evidence.Classification != null &&
                    evidence.Classification.Pass(),
                "native WeaponSubCategory and live activation");
            Add(assertions, "scorching-weapons-base-damage",
                "one +1 fire packet per hit; replay-safe and normally resisted",
                evidence.Damage == null ? "<missing>" :
                    evidence.Damage.Summary(),
                evidence.Damage != null && evidence.Damage.BasePass(),
                "native RuleAttackWithWeapon/RulePrepareDamage resolution");
            Add(assertions, "inner-flame-damage-replacement",
                "one 1d6 fire packet replaces the base +1 packet",
                evidence.Damage == null ? "<missing>" :
                    evidence.Damage.Summary(),
                evidence.Damage != null && evidence.Damage.InnerPass(),
                "native weapon damage bundle");
            Add(assertions, "scorching-weapons-fire-nonstacking",
                "native Flaming remains one 1d6 packet without project flat damage",
                evidence.Damage == null ? "<missing>" :
                    evidence.Damage.Summary(),
                evidence.Damage != null && evidence.Damage.NonstackPass(),
                "exact native Flaming enchantment and live attack");
            Add(assertions, "scorching-weapons-save-bonus",
                "+2 once for fire/light/overlap/direct fire; control and project SLA +0",
                evidence.Saves == null ? "<missing>" :
                    evidence.Saves.Summary(),
                evidence.Saves != null && evidence.Saves.BasePass(),
                "native RuleSavingThrow with exact source contexts");
            Add(assertions, "inner-flame-save-replacement",
                "+4 total rather than additive +2 and +4",
                evidence.Saves == null ? "<missing>" :
                    evidence.Saves.Summary(),
                evidence.Saves != null && evidence.Saves.InnerPass(),
                "native RuleSavingThrow modifier total");
            Add(assertions, "elemental-ifrit-feat-save-state-untouched",
                "false", evidence.SaveStateTouched.ToString(),
                !evidence.SaveStateTouched,
                "save-free request-local fixture contract");
            Add(assertions, "elemental-ifrit-feat-cleanup-exact", "true",
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
