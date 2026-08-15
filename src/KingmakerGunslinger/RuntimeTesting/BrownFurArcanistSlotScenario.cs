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
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.RuleSystem.Rules.Abilities;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.Utility;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.BrownFur;
using Newtonsoft.Json;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal static class BrownFurArcanistSlotScenario
    {
        private const string FileName = "brown-fur-arcanist-slot.json";
        // The spellbook contains the Beast Shape I variant wrapper; the wolf
        // leaf inherits level three but is not itself a spell-list entry.
        private const string CanonicalSpellGuid =
            "61a7ed778dd93f344a5dacdbad324cc9";
        private const string SelectedSpellGuid =
            "3481906baed9487e8403e91a2e9d010a";
        private const string BuffGuid =
            "00d8fbe9cf61dc24298be8d95500c84b";
        private const int SpellLevel = 3;

        [JsonObject(MemberSerialization.OptIn)]
        private sealed class Evidence
        {
            [JsonProperty("classGuid", Order = 1)] public string ClassGuid { get; set; }
            [JsonProperty("classLevel", Order = 2)] public int ClassLevel { get; set; }
            [JsonProperty("castingBlueprintGuid", Order = 3)] public string CastingGuid { get; set; }
            [JsonProperty("preparationBlueprintGuid", Order = 4)] public string PreparationGuid { get; set; }
            [JsonProperty("ownedSpellbookCount", Order = 5)] public int OwnedBookCount { get; set; }
            [JsonProperty("castingCasterLevel", Order = 6)] public int CasterLevel { get; set; }
            [JsonProperty("castingSpontaneous", Order = 7)] public bool Spontaneous { get; set; }
            [JsonProperty("spellGuid", Order = 8)] public string Spell { get; set; }
            [JsonProperty("selectedSpellGuid", Order = 9)] public string SelectedSpell { get; set; }
            [JsonProperty("spellLevel", Order = 10)] public int Level { get; set; }
            [JsonProperty("sourceSpellbookGuid", Order = 11)] public string SourceBook { get; set; }
            [JsonProperty("canSpendBefore", Order = 12)] public bool CanSpendBefore { get; set; }
            [JsonProperty("slotsBefore", Order = 13)] public int SlotsBefore { get; set; }
            [JsonProperty("reservoirBefore", Order = 14)] public int ReservoirBefore { get; set; }
            [JsonProperty("boundaryBegan", Order = 15)] public bool BoundaryBegan { get; set; }
            [JsonProperty("commitTracked", Order = 16)] public bool CommitTracked { get; set; }
            [JsonProperty("commitProceed", Order = 17)] public bool CommitProceed { get; set; }
            [JsonProperty("nativeSlotSpend", Order = 18)] public bool NativeSlotSpend { get; set; }
            [JsonProperty("slotsAfter", Order = 19)] public int SlotsAfter { get; set; }
            [JsonProperty("reservoirAfter", Order = 20)] public int ReservoirAfter { get; set; }
            [JsonProperty("rollbackReservoir", Order = 21)] public int RollbackReservoir { get; set; }
            [JsonProperty("raceBoundaryBegan", Order = 22)] public bool RaceBegan { get; set; }
            [JsonProperty("raceCommitTracked", Order = 23)] public bool RaceTracked { get; set; }
            [JsonProperty("raceCommitProceed", Order = 24)] public bool RaceProceed { get; set; }
            [JsonProperty("raceSlotsBefore", Order = 25)] public int RaceSlotsBefore { get; set; }
            [JsonProperty("raceSlotsAfter", Order = 26)] public int RaceSlotsAfter { get; set; }
            [JsonProperty("raceReservoirBefore", Order = 27)] public int RaceReservoirBefore { get; set; }
            [JsonProperty("raceReservoirAfter", Order = 28)] public int RaceReservoirAfter { get; set; }
            [JsonProperty("suppressedBefore", Order = 29)] public int SuppressedBefore { get; set; }
            [JsonProperty("suppressedAfter", Order = 30)] public int SuppressedAfter { get; set; }
            [JsonProperty("finalActive", Order = 31)] public int FinalActive { get; set; }
            [JsonProperty("finalReservations", Order = 32)] public int FinalReservations { get; set; }
            [JsonProperty("finalScopes", Order = 33)] public string FinalScopes { get; set; }
            [JsonProperty("resourceRemoved", Order = 34)] public bool ResourceRemoved { get; set; }
            [JsonProperty("unitRemoved", Order = 35)] public bool UnitRemoved { get; set; }
            [JsonProperty("automaticIntentArmed", Order = 36)] public bool AutomaticArmed { get; set; }
            [JsonProperty("automaticIntentOutcome", Order = 37)] public string AutomaticOutcome { get; set; }
            [JsonProperty("automaticIntentScopes", Order = 38)] public string AutomaticScopes { get; set; }
            [JsonProperty("automaticIntentCleared", Order = 39)] public bool AutomaticCleared { get; set; }
            [JsonProperty("automaticCommitTracked", Order = 40)] public bool AutomaticTracked { get; set; }
            [JsonProperty("automaticCommitProceed", Order = 41)] public bool AutomaticProceed { get; set; }
            [JsonProperty("automaticReservoirBeforeAfter", Order = 42)] public string AutomaticReservoir { get; set; }
            [JsonProperty("automaticSlotsBeforeAfter", Order = 43)] public string AutomaticSlots { get; set; }
            [JsonProperty("invalidCommandMarked", Order = 44)] public bool InvalidMarked { get; set; }
            [JsonProperty("invalidCommandResult", Order = 45)] public string InvalidResult { get; set; }
            [JsonProperty("invalidCommandNoSpend", Order = 46)] public bool InvalidNoSpend { get; set; }
            [JsonProperty("automaticSelectedAbilityGuid", Order = 47)] public string AutomaticSelectedGuid { get; set; }
            [JsonProperty("automaticSelectedSpellbookGuid", Order = 48)] public string AutomaticSelectedBook { get; set; }
            [JsonProperty("automaticRootAbilityGuid", Order = 49)] public string AutomaticRootGuid { get; set; }
            [JsonProperty("automaticRootSpellbookGuid", Order = 50)] public string AutomaticRootBook { get; set; }
            [JsonProperty("automaticRootCanSpendBefore", Order = 51)] public bool AutomaticRootCanSpend { get; set; }
            [JsonProperty("automaticSelectedCanSpendBefore", Order = 52)] public bool AutomaticSelectedCanSpend { get; set; }
            [JsonProperty("automaticSuppressionBeforeAfter", Order = 53)] public string AutomaticSuppression { get; set; }
        }

        internal static RuntimeTestResult Run(ModContext context,
            RuntimeTestRequest request)
        {
            var assertions = new List<RuntimeTestAssertion>();
            var diagnostics = new List<string>();
            var evidenceFiles = new List<string>();
            var evidence = new Evidence { Spell = CanonicalSpellGuid,
                SelectedSpell = SelectedSpellGuid, Level = SpellLevel };
            UnitEntityData caster = null;
            CotwArcanistContract contract = null;
            Spellbook casting = null;
            BrownFurBlueprintSet blueprints = null;
            object controller = null;
            bool registered = false;
            string stage = "contract";
            try
            {
                BrownFurCastExecutionRuntime.Clear();
                CotwArcanistResolution resolution =
                    BrownFurOptionalExtensionCoordinator.Current;
                if (resolution == null || !resolution.Decision.IsCompatible ||
                    resolution.Contract == null)
                    throw new InvalidOperationException(
                        "Compatible CotW Arcanist contract is unavailable.");
                contract = resolution.Contract;
                blueprints = BrownFurOptionalExtensionCoordinator.Blueprints;
                if (blueprints == null || blueprints.Count != 19)
                    throw new InvalidOperationException(
                        "Registered Brown-Fur blueprints are unavailable.");
                evidence.ClassGuid = contract.ArcanistClass.AssetGuid;
                BlueprintAbility spell = ResourcesLibrary.TryGetBlueprint<
                    BlueprintAbility>(CanonicalSpellGuid);
                BlueprintAbility selected = ResourcesLibrary.TryGetBlueprint<
                    BlueprintAbility>(SelectedSpellGuid);
                AbilityVariants variants = spell == null ? null :
                    (spell.ComponentsArray ?? Array.Empty<BlueprintComponent>())
                        .OfType<AbilityVariants>().SingleOrDefault();
                if (spell == null || spell.School != SpellSchool.Transmutation ||
                    contract.CastingSpellbook.SpellList.GetLevel(spell) !=
                        SpellLevel || selected == null || variants == null ||
                    !(variants.Variants ?? Array.Empty<BlueprintAbility>())
                        .Any(value => ReferenceEquals(value, selected)))
                    throw new InvalidOperationException(
                        "Exact level-three Arcanist Transmutation wrapper and " +
                        "selected variant are unavailable.");

                stage = "level-up";
                caster = new Kingmaker.UI.LevelUp.ChargenUnit(
                    BlueprintRoot.Instance.DefaultPlayerCharacter).Unit;
                caster.Descriptor.Stats.Intelligence.BaseValue = 30;
                registered = Game.Instance.State.Units.All.Add(caster);
                if (!registered) throw new InvalidOperationException(
                    "Disposable Arcanist caster was not registered.");
                Advance(caster.Descriptor, contract.ArcanistClass, 5,
                    ref controller);
                evidence.ClassLevel = caster.Descriptor.Progression
                    .GetClassLevel(contract.ArcanistClass);
                Spellbook[] books = caster.Descriptor.Spellbooks.Where(value =>
                    value != null && value.Blueprint != null).ToArray();
                evidence.OwnedBookCount = books.Length;
                casting = books.SingleOrDefault(value => ReferenceEquals(
                    value.Blueprint, contract.CastingSpellbook));
                Spellbook preparation = books.SingleOrDefault(value =>
                    ReferenceEquals(value.Blueprint,
                        contract.MemorizationSpellbook));
                if (casting == null || preparation == null)
                    throw new InvalidOperationException(
                        "Native Arcanist level-up did not create both resolved spellbooks.");
                evidence.CastingGuid = casting.Blueprint.AssetGuid;
                evidence.PreparationGuid = preparation.Blueprint.AssetGuid;
                while (casting.CasterLevel < evidence.ClassLevel)
                    casting.AddCasterLevel();
                while (preparation.CasterLevel < evidence.ClassLevel)
                    preparation.AddCasterLevel();
                casting.UpdateAllSlotsSize(false);
                preparation.UpdateAllSlotsSize(false);
                casting.Rest();
                preparation.Rest();
                casting.AddKnown(SpellLevel, spell, true);
                preparation.AddKnown(SpellLevel, spell, true);
                evidence.CasterLevel = casting.CasterLevel;
                evidence.Spontaneous = casting.Blueprint.Spontaneous;

                stage = "committed-slot";
                var rootData = new AbilityData(spell, casting);
                var data = new AbilityData(rootData, selected);
                evidence.SourceBook = data.Spellbook == null ? string.Empty :
                    data.Spellbook.Blueprint.AssetGuid;
                evidence.CanSpendBefore = casting.CanSpend(rootData, false);
                evidence.SlotsBefore = AvailableSlots(casting, SpellLevel);
                caster.Descriptor.Resources.Add(contract.Reservoir, true);
                evidence.ReservoirBefore = caster.Descriptor.Resources
                    .GetResourceAmount(contract.Reservoir);
                var target = new TargetWrapper(caster);
                var command = new UnitUseAbility(data, target);
                BrownFurCastTransaction transaction = Transaction(
                    "arcanist-slot-success");
                evidence.BoundaryBegan = BrownFurCastExecutionRuntime.Begin(
                    contract, command, data, target, transaction, Plan());
                var rule = new RuleCastSpell(data, target);
                bool proceed;
                evidence.CommitTracked = BrownFurCastExecutionRuntime.TryCommit(
                    rule, out proceed);
                evidence.CommitProceed = proceed;
                InvokeAbilitySpend(data);
                evidence.SlotsAfter = AvailableSlots(casting, SpellLevel);
                evidence.NativeSlotSpend = evidence.SlotsAfter ==
                    evidence.SlotsBefore - 1;
                evidence.ReservoirAfter = caster.Descriptor.Resources
                    .GetResourceAmount(contract.Reservoir);
                BrownFurCastExecutionRuntime.RuleFailed(rule);
                evidence.RollbackReservoir = caster.Descriptor.Resources
                    .GetResourceAmount(contract.Reservoir);

                stage = "rejected-slot";
                casting.Rest();
                var raceData = new AbilityData(
                    new AbilityData(spell, casting), selected);
                evidence.RaceSlotsBefore = AvailableSlots(casting, SpellLevel);
                var raceCommand = new UnitUseAbility(raceData, target);
                BrownFurCastTransaction race = Transaction(
                    "arcanist-slot-race");
                evidence.RaceBegan = BrownFurCastExecutionRuntime.Begin(
                    contract, raceCommand, raceData, target, race, Plan());
                caster.Descriptor.Resources.Spend(contract.Reservoir,
                    evidence.RollbackReservoir - 1);
                evidence.RaceReservoirBefore = caster.Descriptor.Resources
                    .GetResourceAmount(contract.Reservoir);
                var raceRule = new RuleCastSpell(raceData, target);
                evidence.RaceTracked = BrownFurCastExecutionRuntime.TryCommit(
                    raceRule, out proceed);
                evidence.RaceProceed = proceed;
                evidence.SuppressedBefore =
                    BrownFurCastExecutionRuntime.SuppressedSpendCount;
                InvokeAbilitySpend(raceData);
                evidence.SuppressedAfter =
                    BrownFurCastExecutionRuntime.SuppressedSpendCount;
                evidence.RaceSlotsAfter = AvailableSlots(casting, SpellLevel);
                evidence.RaceReservoirAfter = caster.Descriptor.Resources
                    .GetResourceAmount(contract.Reservoir);
                BrownFurCastExecutionRuntime.EndCommand(raceCommand);

                stage = "automatic-intent";
                caster.Descriptor.Resources.Restore(contract.Reservoir,
                    evidence.ReservoirBefore - evidence.RaceReservoirAfter);
                casting.Rest();
                if (caster.Descriptor.AddFact(blueprints.PowerfulChange) == null ||
                    caster.Descriptor.AddFact(blueprints.ShareTransmutation) == null ||
                    caster.Descriptor.AddFact(
                        blueprints.TransmutationSupremacy) == null ||
                    caster.Descriptor.AddFact(blueprints.ScoreBuffs[0]) == null)
                    throw new InvalidOperationException(
                        "Real Brown-Fur command-intent facts could not be granted.");
                ActivatableAbility share = caster.Descriptor
                    .ActivatableAbilities.Enumerable.SingleOrDefault(value =>
                        value != null && ReferenceEquals(value.Blueprint,
                            blueprints.ShareTransmutationAbility));
                if (share == null) throw new InvalidOperationException(
                    "Real Share Transmutation activatable was not granted.");
                share.IsOn = true;
                int automaticReservoirBefore = caster.Descriptor.Resources
                    .GetResourceAmount(contract.Reservoir);
                int automaticSlotsBefore = AvailableSlots(casting, SpellLevel);
                var automaticData = new AbilityData(
                    new AbilityData(spell, casting), selected);
                AbilityData automaticRoot = automaticData.ConvertedFrom;
                evidence.AutomaticSelectedGuid = AbilityGuid(automaticData);
                evidence.AutomaticSelectedBook = SpellbookGuid(automaticData);
                evidence.AutomaticRootGuid = AbilityGuid(automaticRoot);
                evidence.AutomaticRootBook = SpellbookGuid(automaticRoot);
                evidence.AutomaticRootCanSpend = automaticRoot != null &&
                    casting.CanSpend(automaticRoot, false);
                evidence.AutomaticSelectedCanSpend =
                    casting.CanSpend(automaticData, false);
                var automaticCommand = new UnitUseAbility(automaticData,
                    new TargetWrapper(caster));
                evidence.AutomaticArmed =
                    BrownFurCastExecutionRuntime.ActiveTransactionCount == 1 &&
                    BrownFurCastExecutionRuntime.ReservationCount == 1;
                evidence.AutomaticOutcome =
                    BrownFurCastIntentRuntime.LastOutcome;
                evidence.AutomaticScopes = "share=" +
                    BrownFurShareTargetingRuntime.ActiveScopeCount +
                    ";supremacy=" +
                    BrownFurSupremacyRuntime.ActiveScopeCount;
                evidence.AutomaticCleared =
                    !blueprints.ScoreBuffs.Any(value =>
                        caster.Descriptor.HasFact(value)) &&
                    !caster.Descriptor.HasFact(
                        blueprints.ShareTransmutationBuff) && !share.IsOn;
                var automaticRule = new RuleCastSpell(automaticData,
                    new TargetWrapper(caster));
                evidence.AutomaticTracked =
                    BrownFurCastExecutionRuntime.TryCommit(automaticRule,
                        out proceed);
                evidence.AutomaticProceed = proceed;
                int automaticSuppressionBefore =
                    BrownFurCastExecutionRuntime.SuppressedSpendCount;
                InvokeAbilitySpend(automaticData);
                int automaticSuppressionAfter =
                    BrownFurCastExecutionRuntime.SuppressedSpendCount;
                int automaticReservoirAfter = caster.Descriptor.Resources
                    .GetResourceAmount(contract.Reservoir);
                int automaticSlotsAfter = AvailableSlots(casting, SpellLevel);
                evidence.AutomaticReservoir = automaticReservoirBefore + "->" +
                    automaticReservoirAfter;
                evidence.AutomaticSlots = automaticSlotsBefore + "->" +
                    automaticSlotsAfter;
                evidence.AutomaticSuppression = automaticSuppressionBefore +
                    "->" + automaticSuppressionAfter;
                BrownFurCastExecutionRuntime.RuleFailed(automaticRule);

                stage = "invalid-intent";
                casting.Rest();
                if (caster.Descriptor.AddFact(blueprints.ScoreBuffs[5]) == null)
                    throw new InvalidOperationException(
                        "Invalid Charisma marker could not be granted.");
                int invalidReservoirBefore = caster.Descriptor.Resources
                    .GetResourceAmount(contract.Reservoir);
                int invalidSlotsBefore = AvailableSlots(casting, SpellLevel);
                var invalidData = new AbilityData(
                    new AbilityData(spell, casting), selected);
                var invalidCommand = new UnitUseAbility(invalidData,
                    new TargetWrapper(caster));
                evidence.InvalidMarked =
                    BrownFurCastExecutionRuntime.RejectedCommandCount == 1 &&
                    BrownFurCastExecutionRuntime.ActiveTransactionCount == 0 &&
                    BrownFurCastIntentRuntime.LastOutcome ==
                        "rejected:powerful-stat-not-granted";
                MethodInfo onAction = typeof(UnitUseAbility).GetMethod(
                    "OnAction", BindingFlags.Instance | BindingFlags.Public |
                    BindingFlags.NonPublic, null, Type.EmptyTypes, null);
                if (onAction == null) throw new MissingMethodException(
                    typeof(UnitUseAbility).FullName, "OnAction");
                evidence.InvalidResult = Convert.ToString(onAction.Invoke(
                    invalidCommand, null));
                evidence.InvalidNoSpend =
                    evidence.InvalidResult == UnitCommand.ResultType.Fail.ToString() &&
                    BrownFurCastExecutionRuntime.RejectedCommandCount == 0 &&
                    caster.Descriptor.Resources.GetResourceAmount(
                        contract.Reservoir) == invalidReservoirBefore &&
                    AvailableSlots(casting, SpellLevel) == invalidSlotsBefore &&
                    !blueprints.ScoreBuffs.Any(value =>
                        caster.Descriptor.HasFact(value));
            }
            catch (Exception exception)
            {
                diagnostics.Add("stage=" + stage + ";exception=" +
                    exception.GetType().FullName + ":" + exception.Message);
            }
            finally
            {
                if (controller != null) TryCancel(controller);
                if (caster != null && blueprints != null)
                {
                    BrownFurPlayerIntentRuntime.Clear(caster.Descriptor,
                        blueprints);
                    RemoveFeature(caster.Descriptor,
                        blueprints.TransmutationSupremacy);
                    RemoveFeature(caster.Descriptor,
                        blueprints.ShareTransmutation);
                    RemoveFeature(caster.Descriptor,
                        blueprints.PowerfulChange);
                }
                BrownFurCastExecutionRuntime.Clear();
                evidence.FinalActive =
                    BrownFurCastExecutionRuntime.ActiveTransactionCount;
                evidence.FinalReservations =
                    BrownFurCastExecutionRuntime.ReservationCount;
                evidence.FinalScopes = "share=" +
                    BrownFurShareTargetingRuntime.ActiveScopeCount +
                    ";supremacy=" +
                    BrownFurSupremacyRuntime.ActiveScopeCount +
                    ";modifier=" +
                    BrownFurModifierAdjustmentRuntime.ActiveScopeCount +
                    ";suppressed=" +
                    BrownFurCastExecutionRuntime.SuppressedSpendCount;
                if (caster != null && contract != null &&
                    caster.Descriptor.Resources.ContainsResource(
                        contract.Reservoir))
                    caster.Descriptor.Resources.Remove(contract.Reservoir);
                evidence.ResourceRemoved = caster == null || contract == null ||
                    !caster.Descriptor.Resources.ContainsResource(
                        contract.Reservoir);
                if (registered) Game.Instance.State.Units.All.Remove(caster);
                if (caster != null) caster.Dispose();
                evidence.UnitRemoved = caster == null ||
                    !Game.Instance.State.Units.All.Contains(caster);
            }

            Add(assertions, "arcanist-slot-spellbooks",
                "native level-five CotW Arcanist owns both resolved books",
                "level=" + evidence.ClassLevel + ";books=" +
                    evidence.OwnedBookCount + ";casting=" +
                    evidence.CastingGuid + ";preparation=" +
                    evidence.PreparationGuid + ";casterLevel=" +
                    evidence.CasterLevel,
                evidence.ClassLevel == 5 && evidence.CasterLevel >= 5 &&
                    evidence.CastingGuid == ContractGuid(contract, true) &&
                    evidence.PreparationGuid == ContractGuid(contract, false),
                "native CotW class-level and spellbook creation");
            Add(assertions, "arcanist-slot-source",
                "AbilityData source is exact CotW casting spellbook",
                evidence.SourceBook + ";level=" + evidence.Level +
                    ";canSpend=" + evidence.CanSpendBefore +
                    ";spontaneous=" + evidence.Spontaneous +
                    ";canonical=" + evidence.Spell + ";selected=" +
                    evidence.SelectedSpell,
                evidence.SourceBook == evidence.CastingGuid &&
                    evidence.Spell == CanonicalSpellGuid &&
                    evidence.SelectedSpell == SelectedSpellGuid &&
                    evidence.Level == SpellLevel && evidence.CanSpendBefore,
                "real spellbook-backed AbilityData, not item or SLA");
            Add(assertions, "arcanist-slot-combined-commit",
                "combined intent spends two reservoir and one level-three slot",
                "begin=" + evidence.BoundaryBegan + ";tracked=" +
                    evidence.CommitTracked + ";proceed=" +
                    evidence.CommitProceed + ";reservoir=" +
                    evidence.ReservoirBefore + "->" +
                    evidence.ReservoirAfter + ";slots=" +
                    evidence.SlotsBefore + "->" + evidence.SlotsAfter +
                    ";spent=" + evidence.NativeSlotSpend,
                evidence.BoundaryBegan && evidence.CommitTracked &&
                    evidence.CommitProceed && evidence.NativeSlotSpend &&
                    evidence.ReservoirAfter == evidence.ReservoirBefore - 2 &&
                    evidence.SlotsAfter == evidence.SlotsBefore - 1,
                "production boundary plus native AbilityData.Spend");
            Add(assertions, "arcanist-slot-exception-rollback",
                "rule failure restores reservoir but not an already spent slot",
                evidence.RollbackReservoir.ToString(),
                evidence.RollbackReservoir == evidence.ReservoirBefore,
                "reservoir rollback is exact and independent of native slot");
            Add(assertions, "arcanist-slot-rejected-no-spend",
                "post-reservation shortage rejects and suppresses real slot spend",
                "begin=" + evidence.RaceBegan + ";tracked=" +
                    evidence.RaceTracked + ";proceed=" + evidence.RaceProceed +
                    ";reservoir=" + evidence.RaceReservoirBefore + "->" +
                    evidence.RaceReservoirAfter + ";slots=" +
                    evidence.RaceSlotsBefore + "->" +
                    evidence.RaceSlotsAfter + ";suppression=" +
                    evidence.SuppressedBefore + "->" +
                    evidence.SuppressedAfter,
                evidence.RaceBegan && evidence.RaceTracked &&
                    !evidence.RaceProceed && evidence.RaceReservoirBefore == 1 &&
                    evidence.RaceReservoirAfter == 1 &&
                    evidence.RaceSlotsAfter == evidence.RaceSlotsBefore &&
                    evidence.SuppressedBefore == 1 &&
                    evidence.SuppressedAfter == 0,
                "live AbilityData.Spend Harmony suppression on a real spellbook");
            Add(assertions, "arcanist-slot-automatic-intent",
                "native command constructor snapshots and arms combined owner intent",
                "armed=" + evidence.AutomaticArmed + ";outcome=" +
                    evidence.AutomaticOutcome + ";scopes=" +
                    evidence.AutomaticScopes + ";cleared=" +
                    evidence.AutomaticCleared,
                evidence.AutomaticArmed && evidence.AutomaticOutcome != null &&
                    evidence.AutomaticOutcome.StartsWith("armed:brown-fur-") &&
                    evidence.AutomaticOutcome.EndsWith(";cost=2") &&
                    evidence.AutomaticScopes == "share=1;supremacy=1" &&
                    evidence.AutomaticCleared,
                "actual UnitUseAbility constructor and registered owner facts");
            Add(assertions, "arcanist-slot-automatic-commit",
                "automatically derived intent debits two and spends one slot",
                "tracked=" + evidence.AutomaticTracked + ";proceed=" +
                    evidence.AutomaticProceed + ";reservoir=" +
                    evidence.AutomaticReservoir + ";slots=" +
                    evidence.AutomaticSlots + ";selected=" +
                    evidence.AutomaticSelectedGuid + "@" +
                    evidence.AutomaticSelectedBook + ";root=" +
                    evidence.AutomaticRootGuid + "@" +
                    evidence.AutomaticRootBook + ";canSpend=" +
                    evidence.AutomaticSelectedCanSpend + "/" +
                    evidence.AutomaticRootCanSpend + ";suppression=" +
                    evidence.AutomaticSuppression,
                evidence.AutomaticTracked && evidence.AutomaticProceed &&
                    Delta(evidence.AutomaticReservoir) == -2 &&
                    Delta(evidence.AutomaticSlots) == -1,
                "production constructor bridge, rule commit, and native spend");
            Add(assertions, "arcanist-slot-invalid-intent-pre-action",
                "invalid selected stat rejects before reservoir or slot expenditure",
                "marked=" + evidence.InvalidMarked + ";result=" +
                    evidence.InvalidResult + ";noSpend=" +
                    evidence.InvalidNoSpend,
                evidence.InvalidMarked && evidence.InvalidNoSpend,
                "exact command-scoped OnAction rejection consumed once");
            Add(assertions, "arcanist-slot-cleanup",
                "all Brown-Fur state, resource, and disposable unit removed",
                "active=" + evidence.FinalActive + ";reservations=" +
                    evidence.FinalReservations + ";scopes=" +
                    evidence.FinalScopes + ";resource=" +
                    evidence.ResourceRemoved + ";unit=" + evidence.UnitRemoved,
                evidence.FinalActive == 0 && evidence.FinalReservations == 0 &&
                    evidence.FinalScopes ==
                        "share=0;supremacy=0;modifier=0;suppressed=0" &&
                    evidence.ResourceRemoved && evidence.UnitRemoved,
                "bounded save-free fixture cleanup");

            string path = Path.Combine(request.EvidenceDirectory, FileName);
            File.WriteAllText(path, JsonConvert.SerializeObject(evidence,
                Formatting.Indented));
            evidenceFiles.Add(path);
            diagnostics.Add("arcanistSlotSha256=" + Hash(path));
            bool pass = assertions.All(value =>
                value.Status == RuntimeTestStatuses.Pass);
            Assembly assembly = context.Assembly;
            return new RuntimeTestResult {
                SchemaVersion = 1, RunId = request.RunId,
                Scenario = request.Scenario,
                Status = pass ? RuntimeTestStatuses.Pass : RuntimeTestStatuses.Fail,
                LoadedModVersion = context.ModEntry.Info.Version,
                RuntimeIdentity = assembly.FullName + ";mvid=" +
                    assembly.ManifestModule.ModuleVersionId + ";sha256=" +
                    Hash(assembly.Location) + ";pid=" +
                    Process.GetCurrentProcess().Id,
                GitCommit = Metadata(assembly, "GitCommit"),
                GameVersion = Application.version ?? string.Empty,
                StartUtc = DateTime.UtcNow.ToString("o"), EndUtc = string.Empty,
                Assertions = assertions, Diagnostics = diagnostics,
                Warnings = new List<string>(), ExceptionSummary = string.Empty,
                EvidenceFiles = evidenceFiles,
                AutomaticExitRequested = request.ExitAfterCompletion,
                EvidenceDirectory = request.EvidenceDirectory
            };
        }

        private static void Advance(UnitDescriptor owner,
            BlueprintCharacterClass characterClass, int levels,
            ref object activeController)
        {
            Type type = typeof(
                Kingmaker.UnitLogic.Class.LevelUp.LevelUpController);
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
            object charGen = Enum.Parse(start.GetParameters()[4].ParameterType,
                "CharGen", false);
            for (int index = 0; index < levels; index++)
            {
                activeController = start.Invoke(null,
                    new object[] { owner, false, null, null, charGen });
                if (!(bool)select.Invoke(activeController,
                    new object[] { characterClass, false }))
                    throw new InvalidOperationException(
                        "Disposable Arcanist class selection failed at level " +
                        (index + 1) + ".");
                mechanics.Invoke(activeController, null);
                apply.Invoke(activeController, new object[] { owner });
                cancel.Invoke(activeController, null);
                activeController = null;
            }
        }

        private static int AvailableSlots(Spellbook book, int level)
        {
            if (book.Blueprint.Spontaneous)
                return book.GetSpontaneousSlots(level);
            return book.GetMemorizedSpellSlots(level).Count(value =>
                value != null && value.Available);
        }

        private static void InvokeAbilitySpend(AbilityData ability)
        {
            MethodInfo spend = typeof(AbilityData).GetMethod("Spend",
                BindingFlags.Instance | BindingFlags.Public |
                BindingFlags.NonPublic, null, Type.EmptyTypes, null);
            if (spend == null) throw new MissingMethodException(
                typeof(AbilityData).FullName, "Spend");
            spend.Invoke(ability, new object[0]);
        }

        private static string AbilityGuid(AbilityData ability)
        {
            return ability == null || ability.Blueprint == null ? string.Empty :
                ability.Blueprint.AssetGuid;
        }

        private static string SpellbookGuid(AbilityData ability)
        {
            return ability == null || ability.Spellbook == null ||
                ability.Spellbook.Blueprint == null ? string.Empty :
                ability.Spellbook.Blueprint.AssetGuid;
        }

        private static int Delta(string transition)
        {
            string[] values = (transition ?? string.Empty).Split(new[] { "->" },
                StringSplitOptions.None);
            int before;
            int after;
            return values.Length == 2 && int.TryParse(values[0], out before) &&
                int.TryParse(values[1], out after) ? after - before : int.MaxValue;
        }

        private static void RemoveFeature(UnitDescriptor owner,
            Kingmaker.Blueprints.Facts.BlueprintUnitFact feature)
        {
            if (owner != null && feature != null && owner.HasFact(feature))
                owner.RemoveFact(feature);
        }

        private static BrownFurCastTransaction Transaction(string identity)
        {
            var intent = new BrownFurCastIntent(identity, "cotw-arcanist",
                CanonicalSpellGuid, SelectedSpellGuid,
                "cotw-casting-spellbook", "self", true,
                BrownFurAbilityScore.Strength, true, true, 2,
                "share-exact-target", "polymorph-modifier",
                "native-extend");
            var transaction = new BrownFurCastTransaction(intent);
            transaction.Validate(new BrownFurCastDecision(true, string.Empty,
                2, true, true, true, 2, BrownFurShareDelivery.Touch));
            return transaction;
        }

        private static BrownFurBonusAdapterPlan Plan()
        {
            return new BrownFurBonusAdapterPlan(
                BrownFurBonusAdapterPlanStatus.Supported, string.Empty,
                new[] { BrownFurAbilityScore.Strength },
                new[] { BuffGuid }, new[] { "Polymorph" });
        }

        private static string ContractGuid(CotwArcanistContract contract,
            bool casting)
        {
            BlueprintSpellbook book = contract == null ? null :
                (casting ? contract.CastingSpellbook :
                    contract.MemorizationSpellbook);
            return book == null ? string.Empty : book.AssetGuid;
        }

        private static void TryCancel(object controller)
        {
            try
            {
                controller.GetType().GetMethod("Cancel", BindingFlags.Public |
                    BindingFlags.Instance).Invoke(controller, null);
            }
            catch { }
        }

        private static void Add(List<RuntimeTestAssertion> assertions,
            string name, string expected, string observed, bool pass,
            string evidence)
        {
            assertions.Add(new RuntimeTestAssertion { Name = name,
                Expected = expected, Observed = observed,
                Status = pass ? RuntimeTestStatuses.Pass : RuntimeTestStatuses.Fail,
                Evidence = evidence });
        }

        private static string Hash(string path)
        {
            using (SHA256 sha = SHA256.Create())
            using (FileStream stream = File.OpenRead(path))
                return BitConverter.ToString(sha.ComputeHash(stream))
                    .Replace("-", string.Empty);
        }

        private static string Metadata(Assembly assembly, string key)
        {
            AssemblyMetadataAttribute value = assembly.GetCustomAttributes(
                typeof(AssemblyMetadataAttribute), false)
                .Cast<AssemblyMetadataAttribute>().FirstOrDefault(item =>
                    item.Key == key);
            return value == null ? string.Empty : value.Value;
        }
    }
}
