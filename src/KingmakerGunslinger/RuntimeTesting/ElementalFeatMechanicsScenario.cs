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
using Kingmaker.Blueprints.Items.Armors;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Blueprints.Root;
using Kingmaker.Controllers;
using Kingmaker.ElementsSystem;
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
using Kingmaker.UnitLogic.Class.LevelUp;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
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
    /// Save-free live-rule qualification for Release B feat mechanics. Feature
    /// behavior remains here; the central runner only dispatches the scenario.
    /// </summary>
    internal static class ElementalFeatMechanicsScenario
    {
        internal const string EvidenceFileName =
            "elemental-feat-mechanics.json";
        private const string FighterClassGuid =
            "48ac8db94d5de7645906c7d0ad3bcfbd";
        private const string StandardLongspearGuid =
            "f28f6031c2908d84d945865a80f67177";
        private const string StandardHeavyCrossbowGuid =
            "19a5092244dcf99478dcd73c974828b1";

        private sealed class StrikeEvidence
        {
            public string Race { get; set; }
            public int CharacterLevel { get; set; }
            public int ExpectedBonus { get; set; }
            public string ExpectedEnergy { get; set; }
            public string AbilityAction { get; set; }
            public bool CancelCanStart { get; set; }
            public bool CancelInstalled { get; set; }
            public bool CancelStarted { get; set; }
            public bool CancelAppliedBuff { get; set; }
            public string CommandResult { get; set; }
            public bool ProcessPresent { get; set; }
            public bool ProcessEnded { get; set; }
            public int BuffCount { get; set; }
            public double BuffSeconds { get; set; }
            public int EnergyPacketCount { get; set; }
            public int EnergyPacketValue { get; set; }
            public int FinalEnergyDamage { get; set; }
            public int PacketCountAfterReplay { get; set; }
            public int ResistedEnergyDamage { get; set; }
            public int ResistedReduction { get; set; }
            public int SpellSourcePacketCount { get; set; }
            public int UnrelatedPacketCount { get; set; }

            public bool CommandPass()
            {
                return AbilityAction == UnitCommand.CommandType.Swift.ToString() &&
                    CancelCanStart && CancelInstalled && !CancelStarted &&
                    !CancelAppliedBuff && CommandResult == "Success" &&
                    ProcessPresent && ProcessEnded && BuffCount == 1 &&
                    BuffSeconds > 5d && BuffSeconds <= 7d;
            }

            public bool DamagePass()
            {
                return EnergyPacketCount == 1 &&
                    EnergyPacketValue == ExpectedBonus &&
                    FinalEnergyDamage == ExpectedBonus &&
                    PacketCountAfterReplay == 1 &&
                    ResistedEnergyDamage == 0 &&
                    ResistedReduction >= ExpectedBonus &&
                    SpellSourcePacketCount == 0 &&
                    UnrelatedPacketCount == 1;
            }

            public string Summary()
            {
                return Race + "@" + CharacterLevel + ":" +
                    ExpectedEnergy + "+" + ExpectedBonus + ";command=" +
                    AbilityAction + "/" + CancelCanStart + "/" +
                    CancelInstalled + "/" + CancelStarted + "/" +
                    CancelAppliedBuff + "/" + CommandResult + "/" +
                    ProcessPresent + "/" + ProcessEnded + ";buff=" +
                    BuffCount + "/" + BuffSeconds.ToString("F3") +
                    ";packet=" + EnergyPacketCount + "/" +
                    EnergyPacketValue + "/final=" + FinalEnergyDamage +
                    "/replay=" + PacketCountAfterReplay + ";resisted=" +
                    ResistedEnergyDamage + "/" + ResistedReduction +
                    ";spell=" + SpellSourcePacketCount + ";unrelated=" +
                    UnrelatedPacketCount;
            }
        }

        private sealed class WingsEvidence
        {
            public string FlightBuffGuid { get; set; }
            public string ComponentTypes { get; set; }
            public int ControllerCount { get; set; }
            public int BuffCountUnarmored { get; set; }
            public int MeleeAcBefore { get; set; }
            public int MeleeSourceBefore { get; set; }
            public bool MeleeFlatFootedBefore { get; set; }
            public int MeleeAcUnarmored { get; set; }
            public int MeleeSourceUnarmored { get; set; }
            public bool MeleeFlatFootedUnarmored { get; set; }
            public int RangedAcBefore { get; set; }
            public int RangedSourceBefore { get; set; }
            public int RangedAcUnarmored { get; set; }
            public int RangedSourceUnarmored { get; set; }
            public bool MeleeWeaponClassified { get; set; }
            public bool RangedWeaponClassified { get; set; }
            public bool DifficultTerrainBlocked { get; set; }
            public bool GroundBuffBlocked { get; set; }
            public bool NeutralBuffAllowed { get; set; }
            public bool ProneAllowed { get; set; }
            public string LightArmorGuid { get; set; }
            public int BuffCountLight { get; set; }
            public int MeleeAcLight { get; set; }
            public int MeleeSourceLight { get; set; }
            public int BuffCountLightWithoutWings { get; set; }
            public int MeleeAcLightWithoutWings { get; set; }
            public int MeleeSourceLightWithoutWings { get; set; }
            public string MediumArmorGuid { get; set; }
            public int BuffCountMedium { get; set; }
            public int MeleeAcMedium { get; set; }
            public int MeleeSourceMedium { get; set; }
            public bool DifficultTerrainAllowedInMedium { get; set; }
            public bool GroundBuffAllowedInMedium { get; set; }
            public int BuffCountRestored { get; set; }
            public int MeleeAcRestored { get; set; }
            public int MeleeSourceRestored { get; set; }

            public bool BlueprintPass()
            {
                return ControllerCount == 1 &&
                    ComponentTypes ==
                    "Kingmaker.Designers.Mechanics.Facts.ACBonusAgainstAttacks,Kingmaker.UnitLogic.FactLogic.AddConditionImmunity,Kingmaker.UnitLogic.FactLogic.BuffDescriptorImmunity";
            }

            public bool MechanicsPass()
            {
                return BuffCountUnarmored == 1 &&
                    MeleeSourceBefore == 0 &&
                    MeleeAcUnarmored == MeleeAcBefore + 3 &&
                    MeleeSourceUnarmored == 0 &&
                    !MeleeFlatFootedBefore &&
                    !MeleeFlatFootedUnarmored &&
                    MeleeWeaponClassified && RangedWeaponClassified &&
                    RangedAcUnarmored == RangedAcBefore &&
                    RangedSourceBefore == 0 &&
                    RangedSourceUnarmored == 0 &&
                    DifficultTerrainBlocked && GroundBuffBlocked &&
                    NeutralBuffAllowed && ProneAllowed &&
                    BuffCountLight == 1 && MeleeSourceLight == 0 &&
                    BuffCountLightWithoutWings == 0 &&
                    MeleeSourceLightWithoutWings == 0 &&
                    MeleeAcLight == MeleeAcLightWithoutWings + 3 &&
                    BuffCountMedium == 0 && MeleeSourceMedium == 0 &&
                    DifficultTerrainAllowedInMedium &&
                    GroundBuffAllowedInMedium && BuffCountRestored == 1 &&
                    MeleeAcRestored == MeleeAcUnarmored &&
                    MeleeSourceRestored == 0;
            }

            public string Summary()
            {
                return "controller=" + ControllerCount + ";buff=" +
                    BuffCountUnarmored + "->" + BuffCountLight + "->" +
                    BuffCountMedium + "->" + BuffCountRestored + ";melee=" +
                    MeleeAcBefore + "->" + MeleeAcUnarmored + "/" +
                    MeleeAcLight + "/" + MeleeAcMedium + "/" +
                    MeleeAcRestored + ";meleeSource=" + MeleeSourceBefore +
                    "->" + MeleeSourceUnarmored + "/" + MeleeSourceLight +
                    "/" + MeleeSourceMedium + "/" + MeleeSourceRestored +
                    ";flatFooted=" + MeleeFlatFootedBefore + "->" +
                    MeleeFlatFootedUnarmored + ";weaponClass=" +
                    MeleeWeaponClassified + "/" + RangedWeaponClassified +
                    ";lightWithoutWings=" + BuffCountLightWithoutWings +
                    "/" + MeleeAcLightWithoutWings + "/" +
                    MeleeSourceLightWithoutWings +
                    ";ranged=" + RangedAcBefore + "->" +
                    RangedAcUnarmored + ";rangedSource=" +
                    RangedSourceBefore + "->" + RangedSourceUnarmored +
                    ";terrain=" +
                    DifficultTerrainBlocked + "/" +
                    DifficultTerrainAllowedInMedium + ";ground=" +
                    GroundBuffBlocked + "/" + GroundBuffAllowedInMedium +
                    ";neutral=" + NeutralBuffAllowed + ";prone=" +
                    ProneAllowed + ";components=" + ComponentTypes;
            }
        }

        private sealed class AttackAcEvidence
        {
            public int TargetAc { get; set; }
            public int FlightBonus { get; set; }
            public bool TargetFlatFooted { get; set; }
            public bool WeaponIsMelee { get; set; }
            public bool WeaponIsRanged { get; set; }
        }

        private sealed class Evidence
        {
            public int SchemaVersion { get; set; }
            public bool ModuleActive { get; set; }
            public bool SaveStateTouched { get; set; }
            public List<StrikeEvidence> ElementalStrike { get; set; }
            public WingsEvidence WingsOfAir { get; set; }
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
                SaveStateTouched = false,
                ElementalStrike = new List<StrikeEvidence>()
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
                if (races == null || feats == null ||
                    feats.RegisteredCount !=
                        ElementalRaceIdentityCatalog.FeatIdentityCount)
                    throw new InvalidOperationException(
                        "The complete Elemental Feat graph is unavailable.");
                BlueprintCharacterClass fighter = BlueprintLibraryLookup
                    .RequireExact<BlueprintCharacterClass>(
                        BlueprintBootstrap.Library, FighterClassGuid,
                        "Elemental Feat Fighter fixture");
                BlueprintItemWeapon weapon = BlueprintLibraryLookup
                    .RequireExact<BlueprintItemWeapon>(
                        BlueprintBootstrap.Library, StandardLongspearGuid,
                        "Elemental Strike native weapon");
                BlueprintItemWeapon rangedWeapon = BlueprintLibraryLookup
                    .RequireExact<BlueprintItemWeapon>(
                        BlueprintBootstrap.Library,
                        StandardHeavyCrossbowGuid,
                        "Wings of Air native ranged weapon");
                ElementalRaceBlueprints[] strikeRaces =
                {
                    races.Ifrit, races.Oread, races.Sylph, races.Undine,
                    races.Ifrit
                };
                int[] levels = { 1, 5, 10, 15, 20 };
                for (int index = 0; index < levels.Length; index++)
                {
                    stage = "elemental-strike-" +
                        strikeRaces[index].Definition.Kind + "-" +
                        levels[index];
                    evidence.ElementalStrike.Add(ExerciseStrike(
                        strikeRaces[index], levels[index], feats, fighter,
                        weapon, created, transient, items));
                }
                stage = "wings-of-air";
                evidence.WingsOfAir = ExerciseWings(races.Sylph, feats,
                    fighter, weapon, rangedWeapon, created, transient, items);
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
                    LeaveCombat(unit);
                    if (unit.Body != null && unit.Body.PrimaryHand != null &&
                        unit.Body.PrimaryHand.MaybeItem != null)
                        unit.Body.PrimaryHand.RemoveItem(false);
                    if (unit.Body != null && unit.Body.Armor != null &&
                        unit.Body.Armor.MaybeItem != null)
                        unit.Body.Armor.RemoveItem(false);
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
            diagnostics.Add("elementalFeatMechanicsSha256=" + Hash(path));
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

        private static StrikeEvidence ExerciseStrike(
            ElementalRaceBlueprints race, int level,
            ElementalFeatBlueprintSet feats,
            BlueprintCharacterClass fighter, BlueprintItemWeapon weapon,
            ICollection<UnitEntityData> created,
            ICollection<UnityEngine.Object> transient,
            ICollection<ItemEntity> items)
        {
            UnitEntityData attacker = CreateUnit(race.Race, created,
                transient, "Strike_" + race.Definition.Kind + "_" + level);
            UnitEntityData target = CreateUnit(null, created, transient,
                "StrikeTarget_" + race.Definition.Kind + "_" + level);
            ApplyRaceFacts(attacker.Descriptor, race);
            Advance(attacker.Descriptor, fighter, level);
            if (attacker.Descriptor.Progression.CharacterLevel != level)
                throw new InvalidOperationException(
                    "The Elemental Strike fixture level drifted.");

            BlueprintFeature feature = feats.RequireFeature(
                ElementalFeatId.ElementalStrike);
            BlueprintAbility ability = GrantedAbility(feature);
            BlueprintBuff buff = AppliedBuff(ability);
            EnsureFact(attacker.Descriptor, feature);
            Ability fact = attacker.Descriptor.Abilities.GetAbility(ability);
            if (fact == null)
                throw new InvalidOperationException(
                    "Elemental Strike did not grant its activation ability.");
            var data = new AbilityData(fact);
            var wrapped = new TargetWrapper(attacker);
            var result = new StrikeEvidence
            {
                Race = race.Definition.DisplayName,
                CharacterLevel = level,
                ExpectedBonus = ElementalFeatPolicy.ElementalStrikeBonus(level),
                ExpectedEnergy = Energy(race.Definition.Kind).ToString(),
                AbilityAction = ability.ActionType.ToString()
            };

            UnitUseAbility canceled = CreateCommand(data, wrapped, attacker);
            result.CancelCanStart = canceled.CanStart;
            attacker.Commands.Run(canceled);
            result.CancelInstalled = attacker.Commands.Contains(canceled);
            result.CancelStarted = canceled.IsStarted;
            attacker.Commands.InterruptAll(true);
            attacker.Commands.RemoveFinishedAndUpdateQueue();
            result.CancelAppliedBuff = attacker.Descriptor.Buffs.GetBuff(buff) !=
                null;

            data = new AbilityData(fact);
            UnitUseAbility command = CreateCommand(data, wrapped, attacker);
            object commandResult = InvokeCommandAction(command);
            result.CommandResult = commandResult == null ? string.Empty :
                commandResult.ToString();
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
                result.ProcessEnded = process.IsEnded;
                if (!process.IsEnded) process.Detach();
            }
            InvokeCommandEnded(command, false);
            Buff active = attacker.Descriptor.Buffs.GetBuff(buff);
            result.BuffCount = attacker.Descriptor.Buffs.RawFacts.OfType<Buff>()
                .Count(value => ReferenceEquals(value.Blueprint, buff));
            result.BuffSeconds = active == null ? 0d :
                active.TimeLeft.TotalSeconds;

            var item = new ItemEntityWeapon(weapon);
            items.Add(item);
            attacker.Body.PrimaryHand.InsertItem(item);
            if (!ReferenceEquals(attacker.Body.PrimaryHand.MaybeWeapon, item))
                throw new InvalidOperationException(
                    "The Elemental Strike fixture rejected its weapon.");
            RuleAttackWithWeapon attack = AutoHit(attacker, target, item);
            EnergyDamage[] packets = EnergyPackets(attack, Energy(
                race.Definition.Kind));
            result.EnergyPacketCount = packets.Length;
            result.EnergyPacketValue = packets.Length == 1 ?
                packets[0].PreRolledValue.GetValueOrDefault() : -1;
            result.FinalEnergyDamage = FinalDamage(attack, packets);

            Rulebook.Trigger(new RulePrepareDamage(attack.MeleeDamage));
            result.PacketCountAfterReplay = EnergyPackets(attack,
                Energy(race.Definition.Kind)).Length;

            EnsureFact(target.Descriptor, race.Resistance);
            RuleAttackWithWeapon resisted = AutoHit(attacker, target, item);
            EnergyDamage[] resistedPackets = EnergyPackets(resisted,
                Energy(race.Definition.Kind));
            result.ResistedEnergyDamage = FinalDamage(resisted,
                resistedPackets);
            result.ResistedReduction = resisted.MeleeDamage.ResultDamage
                .Where(value => resistedPackets.Any(packet =>
                    ReferenceEquals(value.Source, packet)))
                .Sum(value => value.Reduction);
            target.Descriptor.RemoveFact(race.Resistance);

            BlueprintAbility nativeSpell = BlueprintLibraryLookup
                .RequireExact<BlueprintAbility>(BlueprintBootstrap.Library,
                    ElementalRaceIdentityCatalog.BurningHandsGuid,
                    "Elemental Strike spell-source negative control");
            var spellRule = new RuleDealDamage(attacker, target,
                new DamageBundle(item, attack.WeaponStats.WeaponSize,
                    new DirectDamage(new DiceFormula(0, DiceType.D6), 1)))
            {
                AttackRoll = attack.AttackRoll,
                SourceAbility = nativeSpell
            };
            Rulebook.Trigger(new RulePrepareDamage(spellRule));
            result.SpellSourcePacketCount = spellRule.DamageBundle
                .OfType<EnergyDamage>().Count();

            var unrelatedRule = new RuleDealDamage(attacker, target,
                new DamageBundle(new EnergyDamage(
                    new DiceFormula(0, DiceType.D6),
                    Energy(race.Definition.Kind)) { PreRolledValue = 1 }));
            Rulebook.Trigger(new RulePrepareDamage(unrelatedRule));
            result.UnrelatedPacketCount = unrelatedRule.DamageBundle
                .OfType<EnergyDamage>().Count();
            return result;
        }

        private static WingsEvidence ExerciseWings(
            ElementalRaceBlueprints sylph, ElementalFeatBlueprintSet feats,
            BlueprintCharacterClass fighter,
            BlueprintItemWeapon meleeBlueprint,
            BlueprintItemWeapon rangedBlueprint,
            ICollection<UnitEntityData> created,
            ICollection<UnityEngine.Object> transient,
            ICollection<ItemEntity> items)
        {
            UnitEntityData defender = CreateUnit(sylph.Race, created,
                transient, "Wings_Defender");
            UnitEntityData attacker = CreateUnit(null, created, transient,
                "Wings_Attacker");
            SetUnitPosition(defender, Vector3.zero);
            SetUnitPosition(attacker, new Vector3(1.5f, 0f, 0f));
            EnterCombat(defender);
            EnterCombat(attacker);
            if (!defender.Memory.Contains(attacker))
                defender.Memory.Add(attacker);
            if (!attacker.Memory.Contains(defender))
                attacker.Memory.Add(defender);
            ApplyRaceFacts(defender.Descriptor, sylph);
            Advance(defender.Descriptor, fighter, 9);
            BlueprintFeature airy = feats.RequireFeature(
                ElementalFeatId.AiryStep);
            BlueprintFeature wings = feats.RequireFeature(
                ElementalFeatId.WingsOfAir);
            ElementalWingsOfAirController controller = wings.ComponentsArray
                .OfType<ElementalWingsOfAirController>().Single();
            BlueprintBuff flight = controller.FlightBuff;
            EnsureFact(defender.Descriptor, airy);
            var meleeWeapon = new ItemEntityWeapon(meleeBlueprint);
            var rangedWeapon = new ItemEntityWeapon(rangedBlueprint);
            items.Add(meleeWeapon);
            items.Add(rangedWeapon);
            AttackAcEvidence meleeBefore = AttackAc(attacker, defender,
                meleeWeapon, flight);
            AttackAcEvidence rangedBefore = AttackAc(attacker, defender,
                rangedWeapon, flight);
            EnsureFact(defender.Descriptor, wings);
            AttackAcEvidence meleeUnarmored = AttackAc(attacker, defender,
                meleeWeapon, flight);
            AttackAcEvidence rangedUnarmored = AttackAc(attacker, defender,
                rangedWeapon, flight);

            var result = new WingsEvidence
            {
                FlightBuffGuid = flight.AssetGuid,
                ComponentTypes = string.Join(",", flight.ComponentsArray
                    .Select(value => value.GetType().FullName).ToArray()),
                ControllerCount = wings.ComponentsArray.OfType<
                    ElementalWingsOfAirController>().Count(),
                BuffCountUnarmored = CountBuff(defender, flight),
                MeleeAcBefore = meleeBefore.TargetAc,
                MeleeSourceBefore = meleeBefore.FlightBonus,
                MeleeFlatFootedBefore = meleeBefore.TargetFlatFooted,
                MeleeAcUnarmored = meleeUnarmored.TargetAc,
                MeleeSourceUnarmored = meleeUnarmored.FlightBonus,
                MeleeFlatFootedUnarmored =
                    meleeUnarmored.TargetFlatFooted,
                RangedAcBefore = rangedBefore.TargetAc,
                RangedSourceBefore = rangedBefore.FlightBonus,
                RangedAcUnarmored = rangedUnarmored.TargetAc,
                RangedSourceUnarmored = rangedUnarmored.FlightBonus,
                MeleeWeaponClassified = meleeUnarmored.WeaponIsMelee &&
                    !meleeUnarmored.WeaponIsRanged,
                RangedWeaponClassified = rangedUnarmored.WeaponIsRanged &&
                    !rangedUnarmored.WeaponIsMelee
            };

            defender.Descriptor.State.AddCondition(
                UnitCondition.DifficultTerrain, null);
            result.DifficultTerrainBlocked = !defender.Descriptor.State
                .HasCondition(UnitCondition.DifficultTerrain);
            BlueprintBuff ground = CreateDescriptorBuff(
                "KMG_Runtime_Wings_Ground", SpellDescriptor.Ground,
                transient);
            BlueprintBuff neutral = CreateDescriptorBuff(
                "KMG_Runtime_Wings_Neutral", SpellDescriptor.None,
                transient);
            result.GroundBuffBlocked = !TryBuff(defender, ground);
            result.NeutralBuffAllowed = TryBuff(defender, neutral);
            RemoveBuff(defender, neutral);
            defender.Descriptor.State.AddCondition(UnitCondition.Prone, null);
            result.ProneAllowed = defender.Descriptor.State.HasCondition(
                UnitCondition.Prone);
            defender.Descriptor.State.RemoveCondition(UnitCondition.Prone);

            BlueprintItemArmor light = FindArmor(
                ArmorProficiencyGroup.Light);
            BlueprintItemArmor medium = FindArmor(
                ArmorProficiencyGroup.Medium);
            result.LightArmorGuid = light.AssetGuid;
            result.MediumArmorGuid = medium.AssetGuid;
            LeaveCombat(attacker);
            LeaveCombat(defender);
            ItemEntityArmor armor = EquipArmor(defender, light, items);
            EnterCombat(defender);
            EnterCombat(attacker);
            defender.Descriptor.RemoveFact(wings);
            AttackAcEvidence lightWithoutWings = AttackAc(attacker, defender,
                meleeWeapon, flight);
            result.BuffCountLightWithoutWings = CountBuff(defender, flight);
            result.MeleeAcLightWithoutWings = lightWithoutWings.TargetAc;
            result.MeleeSourceLightWithoutWings =
                lightWithoutWings.FlightBonus;
            EnsureFact(defender.Descriptor, wings);
            AttackAcEvidence lightAc = AttackAc(attacker, defender,
                meleeWeapon, flight);
            result.BuffCountLight = CountBuff(defender, flight);
            result.MeleeAcLight = lightAc.TargetAc;
            result.MeleeSourceLight = lightAc.FlightBonus;
            LeaveCombat(attacker);
            LeaveCombat(defender);
            RemoveArmor(defender, armor, items);

            armor = EquipArmor(defender, medium, items);
            EnterCombat(defender);
            EnterCombat(attacker);
            AttackAcEvidence mediumAc = AttackAc(attacker, defender,
                meleeWeapon, flight);
            result.BuffCountMedium = CountBuff(defender, flight);
            result.MeleeAcMedium = mediumAc.TargetAc;
            result.MeleeSourceMedium = mediumAc.FlightBonus;
            defender.Descriptor.State.AddCondition(
                UnitCondition.DifficultTerrain, null);
            result.DifficultTerrainAllowedInMedium = defender.Descriptor.State
                .HasCondition(UnitCondition.DifficultTerrain);
            defender.Descriptor.State.RemoveCondition(
                UnitCondition.DifficultTerrain);
            result.GroundBuffAllowedInMedium = TryBuff(defender, ground);
            RemoveBuff(defender, ground);
            LeaveCombat(attacker);
            LeaveCombat(defender);
            RemoveArmor(defender, armor, items);
            EnterCombat(defender);
            EnterCombat(attacker);
            AttackAcEvidence restoredAc = AttackAc(attacker, defender,
                meleeWeapon, flight);
            result.BuffCountRestored = CountBuff(defender, flight);
            result.MeleeAcRestored = restoredAc.TargetAc;
            result.MeleeSourceRestored = restoredAc.FlightBonus;
            return result;
        }

        private static BlueprintAbility GrantedAbility(
            BlueprintFeature feature)
        {
            if (feature == null)
                throw new ArgumentNullException("feature");
            return feature.ComponentsArray.OfType<AddFacts>()
                .SelectMany(value => value.Facts ??
                    Array.Empty<BlueprintUnitFact>())
                .OfType<BlueprintAbility>().Single();
        }

        private static BlueprintBuff AppliedBuff(BlueprintAbility ability)
        {
            if (ability == null)
                throw new ArgumentNullException("ability");
            return ability.ComponentsArray.OfType<AbilityEffectRunAction>()
                .SelectMany(value => value.Actions.Actions ??
                    Array.Empty<GameAction>())
                .OfType<ContextActionApplyBuff>()
                .Select(value => value.Buff).Single();
        }

        private static DamageEnergyType Energy(ElementalRaceKind race)
        {
            switch (race)
            {
                case ElementalRaceKind.Ifrit: return DamageEnergyType.Fire;
                case ElementalRaceKind.Oread: return DamageEnergyType.Acid;
                case ElementalRaceKind.Sylph:
                    return DamageEnergyType.Electricity;
                case ElementalRaceKind.Undine: return DamageEnergyType.Cold;
                default: throw new ArgumentOutOfRangeException("race");
            }
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
                    "The native Elemental Strike weapon attack did not resolve.");
            return result;
        }

        private static EnergyDamage[] EnergyPackets(
            RuleAttackWithWeapon attack, DamageEnergyType energy)
        {
            return attack == null || attack.MeleeDamage == null ?
                Array.Empty<EnergyDamage>() : attack.MeleeDamage.DamageBundle
                    .OfType<EnergyDamage>().Where(value =>
                        value.EnergyType == energy).ToArray();
        }

        private static int FinalDamage(RuleAttackWithWeapon attack,
            ICollection<EnergyDamage> packets)
        {
            if (attack == null || attack.MeleeDamage == null ||
                attack.MeleeDamage.ResultDamage == null || packets == null)
                return 0;
            return attack.MeleeDamage.ResultDamage.Where(value =>
                packets.Any(packet => ReferenceEquals(value.Source, packet)))
                .Sum(value => value.FinalValue);
        }

        private static AttackAcEvidence AttackAc(UnitEntityData attacker,
            UnitEntityData defender, ItemEntityWeapon weapon,
            BlueprintBuff flight)
        {
            int damage = defender.Descriptor.Damage;
            ItemEntity previous = attacker.Body.PrimaryHand.MaybeItem;
            bool changed = !ReferenceEquals(previous, weapon);
            RuleAttackWithWeapon attack;
            try
            {
                if (changed)
                {
                    if (previous != null)
                        attacker.Body.PrimaryHand.RemoveItem(false);
                    attacker.Body.PrimaryHand.InsertItem(weapon);
                    if (!ReferenceEquals(
                            attacker.Body.PrimaryHand.MaybeWeapon, weapon))
                        throw new InvalidOperationException(
                            "The native Wings attacker rejected its weapon.");
                }
                attack = Rulebook.Trigger(new RuleAttackWithWeapon(attacker,
                    defender, weapon, 0));
            }
            finally
            {
                defender.Descriptor.Damage = damage;
                if (changed)
                {
                    if (ReferenceEquals(attacker.Body.PrimaryHand.MaybeItem,
                            weapon))
                        attacker.Body.PrimaryHand.RemoveItem(false);
                    if (previous != null)
                        attacker.Body.PrimaryHand.InsertItem(previous);
                }
            }
            if (attack.AttackRoll == null || attack.AttackRoll.ACRule == null ||
                attack.AttackRoll.ACRule.BonusSources == null)
                throw new InvalidOperationException(
                    "The native Wings attack exposed no AC source list.");
            int bonus = attack.AttackRoll.ACRule.BonusSources.Where(value =>
                value.Source != null && value.Source.Blueprint != null &&
                (ReferenceEquals(value.Source.Blueprint, flight) ||
                 string.Equals(value.Source.Blueprint.AssetGuid,
                    flight.AssetGuid, StringComparison.Ordinal)))
                .Sum(value => value.Bonus);
            return new AttackAcEvidence
            {
                TargetAc = attack.AttackRoll.TargetAC,
                FlightBonus = bonus,
                TargetFlatFooted = attack.AttackRoll.ACRule
                    .IsTargetFlatFooted,
                WeaponIsMelee = weapon.Blueprint.IsMelee,
                WeaponIsRanged = weapon.Blueprint.IsRanged
            };
        }

        private static BlueprintItemArmor FindArmor(
            ArmorProficiencyGroup group)
        {
            BlueprintItemArmor result = BlueprintBootstrap.Library
                .GetAllBlueprints().OfType<BlueprintItemArmor>().Where(value =>
                    value != null && value.Type != null && value.Type.IsArmor &&
                    value.Type.ProficiencyGroup == group)
                .OrderBy(value => value.AssetGuid, StringComparer.Ordinal)
                .FirstOrDefault();
            if (result == null)
                throw new InvalidOperationException(
                    "No native " + group + " armor fixture is available.");
            return result;
        }

        private static ItemEntityArmor EquipArmor(UnitEntityData unit,
            BlueprintItemArmor blueprint, ICollection<ItemEntity> items)
        {
            var result = new ItemEntityArmor(blueprint);
            items.Add(result);
            unit.Body.Armor.InsertItem(result);
            if (!ReferenceEquals(unit.Body.Armor.Armor, result))
                throw new InvalidOperationException(
                    "The Wings fixture rejected native armor " +
                    blueprint.AssetGuid + ".");
            return result;
        }

        private static void RemoveArmor(UnitEntityData unit,
            ItemEntityArmor armor, ICollection<ItemEntity> items)
        {
            if (!ReferenceEquals(unit.Body.Armor.MaybeItem, armor) ||
                !unit.Body.Armor.RemoveItem(false) ||
                unit.Body.Armor.MaybeItem != null)
                throw new InvalidOperationException(
                    "The Wings fixture could not remove its armor legally.");
            armor.Dispose();
            items.Remove(armor);
        }

        private static BlueprintBuff CreateDescriptorBuff(string name,
            SpellDescriptor descriptor,
            ICollection<UnityEngine.Object> transient)
        {
            var result = ScriptableObject.CreateInstance<BlueprintBuff>();
            result.name = name;
            result.IsClassFeature = false;
            result.Stacking = StackingType.Replace;
            result.FxOnStart = new PrefabLink();
            result.FxOnRemove = new PrefabLink();
            result.ResourceAssetIds = Array.Empty<string>();
            if (descriptor == SpellDescriptor.None)
                result.ComponentsArray = Array.Empty<BlueprintComponent>();
            else
            {
                var component = ScriptableObject.CreateInstance<
                    SpellDescriptorComponent>();
                component.Descriptor = descriptor;
                result.ComponentsArray = new BlueprintComponent[]
                    { component };
                transient.Add(component);
            }
            transient.Add(result);
            return result;
        }

        private static bool TryBuff(UnitEntityData unit,
            BlueprintBuff blueprint)
        {
            var context = new MechanicsContext(unit, unit.Descriptor,
                blueprint, null, new TargetWrapper(unit));
            Buff applied = unit.Descriptor.Buffs.AddBuff(blueprint, context,
                TimeSpan.FromSeconds(60d));
            return applied != null && ReferenceEquals(
                unit.Descriptor.Buffs.GetBuff(blueprint), applied);
        }

        private static void RemoveBuff(UnitEntityData unit,
            BlueprintBuff blueprint)
        {
            Buff fact = unit.Descriptor.Buffs.GetBuff(blueprint);
            if (fact != null) unit.Descriptor.Buffs.RemoveFact(fact);
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
            BlueprintUnit donor = BlueprintRoot.Instance
                .DefaultPlayerCharacter;
            BlueprintUnit blueprint = UnityEngine.Object.Instantiate(donor);
            blueprint.name = "KMG_Runtime_ElementalFeat_" + suffix;
            if (race != null) blueprint.Race = race;
            blueprint.Brain = null;
            blueprint.IsCheater = false;
            transient.Add(blueprint);
            UnitEntityData result = new ChargenUnit(blueprint).Unit;
            if (result == null || result.Descriptor == null ||
                (race != null && !ReferenceEquals(
                    result.Descriptor.Progression.Race, race)))
                throw new InvalidOperationException(
                    "A request-local Elemental Feat unit was unavailable.");
            result.Descriptor.Stats.HitPoints.BaseValue = 500;
            result.Descriptor.State.Immortality.Retain();
            if (!Game.Instance.State.Units.All.Add(result))
            {
                result.Dispose();
                throw new InvalidOperationException(
                    "A request-local Elemental Feat unit could not be registered.");
            }
            created.Add(result);
            return result;
        }

        private static void SetUnitPosition(UnitEntityData unit,
            Vector3 position)
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

        private static void EnterCombat(UnitEntityData unit)
        {
            if (!unit.CombatState.IsInCombat) unit.CombatState.JoinCombat();
            unit.CombatState.OnNewRound();
        }

        private static void LeaveCombat(UnitEntityData unit)
        {
            if (unit.CombatState.IsInCombat) unit.CombatState.LeaveCombat();
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
                    "The native Elemental Feat level-up surface is unavailable.");
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
                            "Elemental Feat class selection failed.");
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
            ICollection<RuntimeTestAssertion> assertions, Evidence evidence)
        {
            Add(assertions, "elemental-feat-module-active", "true",
                evidence.ModuleActive.ToString(), evidence.ModuleActive,
                "existing elemental-races module setting");
            Add(assertions, "elemental-strike-breakpoint-cases",
                "1,5,10,15,20 across Fire, Acid, Electricity, Cold",
                string.Join(" | ", evidence.ElementalStrike.Select(value =>
                    value.Summary()).ToArray()),
                evidence.ElementalStrike.Count == 5 &&
                    evidence.ElementalStrike.Select(value =>
                        value.CharacterLevel).SequenceEqual(
                            new[] { 1, 5, 10, 15, 20 }) &&
                    evidence.ElementalStrike.Select(value =>
                        value.ExpectedBonus).SequenceEqual(
                            new[] { 1, 2, 3, 4, 5 }),
                "native multiclass-safe CharacterLevel plus pure breakpoint policy");
            foreach (StrikeEvidence strike in evidence.ElementalStrike)
            {
                string key = strike.Race.ToLowerInvariant() + "-" +
                    strike.CharacterLevel;
                Add(assertions, "elemental-strike-command-" + key,
                    "canceled Swift command applies nothing; accepted command creates one 1-round buff",
                    strike.Summary(), strike.CommandPass(),
                    "production AbilityData and native UnitUseAbility process");
                Add(assertions, "elemental-strike-damage-" + key,
                    "one exact typed flat packet; replay deduped; native resistance; spell and unrelated damage excluded",
                    strike.Summary(), strike.DamagePass(),
                    "live RuleAttackWithWeapon -> RulePrepareDamage -> RuleDealDamage pipeline");
            }
            Add(assertions, "wings-of-air-blueprint-contract",
                "one armor controller plus exact audited draconic-flight component trio",
                evidence.WingsOfAir == null ? "<missing>" :
                    evidence.WingsOfAir.Summary(),
                evidence.WingsOfAir != null &&
                    evidence.WingsOfAir.BlueprintPass(),
                "production project-owned feature and buff components");
            Add(assertions, "wings-of-air-native-mechanics",
                "+3 melee Dodge AC, +0 ranged; terrain/Ground immunity but not prone; active in no/light armor, suppressed in medium, restored on removal",
                evidence.WingsOfAir == null ? "<missing>" :
                    evidence.WingsOfAir.Summary(),
                evidence.WingsOfAir != null &&
                    evidence.WingsOfAir.MechanicsPass(),
                "live RuleAttackWithWeapon AC BonusSources, condition immunity, buff descriptor immunity, and armor-slot events");
            Add(assertions, "elemental-feat-mechanics-save-free", "false",
                evidence.SaveStateTouched.ToString(),
                !evidence.SaveStateTouched,
                "request-local units only; no save or party APIs");
            Add(assertions, "elemental-feat-mechanics-cleanup",
                "exact pre-run global-unit reference sequence",
                evidence.CleanupExact.ToString(), evidence.CleanupExact,
                "finally interruption, item removal, disposal, and exact comparison");
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
                .OfType<AssemblyMetadataAttribute>().SingleOrDefault(item =>
                    string.Equals(item.Key, key, StringComparison.Ordinal));
            return value == null ? string.Empty : value.Value;
        }

        private static string Hash(string path)
        {
            using (var stream = new FileStream(path, FileMode.Open,
                FileAccess.Read, FileShare.ReadWrite | FileShare.Delete))
            using (var algorithm = SHA256.Create())
                return BitConverter.ToString(algorithm.ComputeHash(stream))
                    .Replace("-", string.Empty).ToLowerInvariant();
        }
    }
}
