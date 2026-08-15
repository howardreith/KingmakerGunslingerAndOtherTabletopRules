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
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.Items;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
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
    internal static class BrownFurNativeCastScenario
    {
        private const string FileName = "brown-fur-native-cast.json";
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
            [JsonProperty("classLevel", Order = 1)] public int ClassLevel { get; set; }
            [JsonProperty("casterInState", Order = 2)] public bool CasterInState { get; set; }
            [JsonProperty("allyInState", Order = 3)] public bool AllyInState { get; set; }
            [JsonProperty("castingSpellbookGuid", Order = 4)] public string CastingBook { get; set; }
            [JsonProperty("preparationSpellbookGuid", Order = 5)] public string PreparationBook { get; set; }
            [JsonProperty("canonicalSpellGuid", Order = 6)] public string CanonicalSpell { get; set; }
            [JsonProperty("selectedSpellGuid", Order = 7)] public string SelectedSpell { get; set; }
            [JsonProperty("sourceSpellbookGuid", Order = 8)] public string SourceBook { get; set; }
            [JsonProperty("targetIdentity", Order = 9)] public string Target { get; set; }
            [JsonProperty("targetable", Order = 10)] public bool Targetable { get; set; }
            [JsonProperty("commandCanStart", Order = 11)] public bool CanStart { get; set; }
            [JsonProperty("commandStarted", Order = 12)] public bool Started { get; set; }
            [JsonProperty("commandResult", Order = 13)] public string CommandResult { get; set; }
            [JsonProperty("processStarted", Order = 14)] public bool ProcessStarted { get; set; }
            [JsonProperty("processEnded", Order = 15)] public bool ProcessEnded { get; set; }
            [JsonProperty("transactionState", Order = 16)] public string TransactionState { get; set; }
            [JsonProperty("reservoirBefore", Order = 17)] public int ReservoirBefore { get; set; }
            [JsonProperty("reservoirAfter", Order = 18)] public int ReservoirAfter { get; set; }
            [JsonProperty("slotsBefore", Order = 19)] public int SlotsBefore { get; set; }
            [JsonProperty("slotsAfter", Order = 20)] public int SlotsAfter { get; set; }
            [JsonProperty("allyBuffCount", Order = 21)] public int AllyBuffCount { get; set; }
            [JsonProperty("casterBuffCount", Order = 22)] public int CasterBuffCount { get; set; }
            [JsonProperty("modifierCount", Order = 23)] public int ModifierCount { get; set; }
            [JsonProperty("modifierValue", Order = 24)] public int ModifierValue { get; set; }
            [JsonProperty("modifierDescriptor", Order = 25)] public string ModifierDescriptor { get; set; }
            [JsonProperty("buffCasterExact", Order = 26)] public bool BuffCasterExact { get; set; }
            [JsonProperty("buffTargetExact", Order = 27)] public bool BuffTargetExact { get; set; }
            [JsonProperty("buffSourceSpellExact", Order = 28)] public bool BuffSourceSpellExact { get; set; }
            [JsonProperty("buffTimeSeconds", Order = 29)] public double BuffTimeSeconds { get; set; }
            [JsonProperty("finalActive", Order = 30)] public int FinalActive { get; set; }
            [JsonProperty("finalReservations", Order = 31)] public int FinalReservations { get; set; }
            [JsonProperty("finalScopes", Order = 32)] public string FinalScopes { get; set; }
            [JsonProperty("unitsRemoved", Order = 33)] public bool UnitsRemoved { get; set; }
            [JsonProperty("interruptionIntentArmed", Order = 34)] public bool InterruptionArmed { get; set; }
            [JsonProperty("interruptionIntentOutcome", Order = 35)] public string InterruptionOutcome { get; set; }
            [JsonProperty("interruptionIntentCleared", Order = 36)] public bool InterruptionCleared { get; set; }
            [JsonProperty("interruptionTargetable", Order = 37)] public bool InterruptionTargetable { get; set; }
            [JsonProperty("interruptionCanStart", Order = 38)] public bool InterruptionCanStart { get; set; }
            [JsonProperty("interruptionStarted", Order = 39)] public bool InterruptionStarted { get; set; }
            [JsonProperty("interruptionResult", Order = 40)] public string InterruptionResult { get; set; }
            [JsonProperty("interruptionFinished", Order = 41)] public bool InterruptionFinished { get; set; }
            [JsonProperty("interruptionReservoirBeforeAfter", Order = 42)] public string InterruptionReservoir { get; set; }
            [JsonProperty("interruptionSlotsBeforeAfter", Order = 43)] public string InterruptionSlots { get; set; }
            [JsonProperty("interruptionFinalState", Order = 44)] public string InterruptionFinalState { get; set; }
        }

        internal static RuntimeTestResult Run(ModContext context,
            RuntimeTestRequest request)
        {
            var assertions = new List<RuntimeTestAssertion>();
            var diagnostics = new List<string>();
            var evidenceFiles = new List<string>();
            var evidence = new Evidence {
                CanonicalSpell = CanonicalSpellGuid,
                SelectedSpell = SelectedSpellGuid,
                CommandResult = string.Empty,
                TransactionState = string.Empty,
                ModifierDescriptor = string.Empty
            };
            CotwArcanistContract contract = null;
            BrownFurBlueprintSet blueprints = null;
            UnitEntityData caster = null;
            UnitEntityData ally = null;
            BlueprintUnit casterBlueprint = null;
            BlueprintUnit allyBlueprint = null;
            object levelController = null;
            UnitUseAbility command = null;
            Buff applied = null;
            bool casterCreated = false;
            bool allyCreated = false;
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
                BlueprintAbility root = ResourcesLibrary.TryGetBlueprint<
                    BlueprintAbility>(CanonicalSpellGuid);
                BlueprintAbility selected = ResourcesLibrary.TryGetBlueprint<
                    BlueprintAbility>(SelectedSpellGuid);
                BlueprintBuff buff = ResourcesLibrary.TryGetBlueprint<
                    BlueprintBuff>(BuffGuid);
                AbilityVariants variants = root == null ? null :
                    (root.ComponentsArray ?? Array.Empty<BlueprintComponent>())
                        .OfType<AbilityVariants>().SingleOrDefault();
                if (root == null || selected == null || buff == null ||
                    variants == null || !(variants.Variants ??
                        Array.Empty<BlueprintAbility>()).Any(value =>
                            ReferenceEquals(value, selected)) ||
                    contract.CastingSpellbook.SpellList.GetLevel(root) !=
                        SpellLevel)
                    throw new InvalidOperationException(
                        "Exact Beast Shape I wrapper, wolf variant, buff, and " +
                        "level-three spell-list entry were not available.");

                stage = "live-units";
                UnitEntityData anchor = Game.Instance.Player.Party
                    .FirstOrDefault(value => value != null &&
                        value.HoldingState != null);
                if (anchor == null) throw new InvalidOperationException(
                    "The guarded working save has no active-area party anchor.");
                casterBlueprint = UnityEngine.Object.Instantiate(
                    BlueprintRoot.Instance.DefaultPlayerCharacter);
                casterBlueprint.name = "KMG_Runtime_BrownFur_NativeCaster";
                casterBlueprint.IsCheater = true;
                allyBlueprint = UnityEngine.Object.Instantiate(
                    BlueprintRoot.Instance.DefaultPlayerCharacter);
                allyBlueprint.name = "KMG_Runtime_BrownFur_NativeAlly";
                allyBlueprint.IsCheater = true;
                caster = Game.Instance.EntityCreator.SpawnUnit(casterBlueprint,
                    anchor.Position, Quaternion.identity, anchor.HoldingState);
                casterCreated = caster != null;
                ally = Game.Instance.EntityCreator.SpawnUnit(allyBlueprint,
                    anchor.Position + new Vector3(0.5f, 0f, 0f),
                    Quaternion.identity, anchor.HoldingState);
                allyCreated = ally != null;
                Game.Instance.EntityCreator.Tick();
                evidence.CasterInState = caster != null && caster.IsInState &&
                    caster.View != null && caster.View.Data == caster;
                evidence.AllyInState = ally != null && ally.IsInState &&
                    ally.View != null && ally.View.Data == ally;
                if (!evidence.CasterInState || !evidence.AllyInState)
                    throw new InvalidOperationException(
                        "Disposable Brown-Fur caster or ally did not enter the live area.");
                caster.Descriptor.Stats.Intelligence.BaseValue = 30;
                caster.Descriptor.Stats.HitPoints.BaseValue = 10000;
                ally.Descriptor.Stats.HitPoints.BaseValue = 10000;

                stage = "arcanist";
                Advance(caster.Descriptor, contract.ArcanistClass, 5,
                    ref levelController);
                evidence.ClassLevel = caster.Descriptor.Progression
                    .GetClassLevel(contract.ArcanistClass);
                Spellbook casting = caster.Descriptor.Spellbooks
                    .SingleOrDefault(value => value != null &&
                        ReferenceEquals(value.Blueprint,
                            contract.CastingSpellbook));
                Spellbook preparation = caster.Descriptor.Spellbooks
                    .SingleOrDefault(value => value != null &&
                        ReferenceEquals(value.Blueprint,
                            contract.MemorizationSpellbook));
                if (casting == null || preparation == null)
                    throw new InvalidOperationException(
                        "Native Arcanist did not own both resolved spellbooks.");
                evidence.CastingBook = casting.Blueprint.AssetGuid;
                evidence.PreparationBook = preparation.Blueprint.AssetGuid;
                while (casting.CasterLevel < evidence.ClassLevel)
                    casting.AddCasterLevel();
                while (preparation.CasterLevel < evidence.ClassLevel)
                    preparation.AddCasterLevel();
                casting.UpdateAllSlotsSize(false);
                preparation.UpdateAllSlotsSize(false);
                casting.Rest();
                preparation.Rest();
                casting.AddKnown(SpellLevel, root, true);
                preparation.AddKnown(SpellLevel, root, true);
                caster.Descriptor.Resources.Add(contract.Reservoir, true);

                stage = "native-cast";
                var rootData = new AbilityData(root, casting);
                var data = new AbilityData(rootData, selected);
                evidence.SourceBook = data.Spellbook == null ? string.Empty :
                    data.Spellbook.Blueprint.AssetGuid;
                evidence.Target = ally.UniqueId;
                evidence.ReservoirBefore = caster.Descriptor.Resources
                    .GetResourceAmount(contract.Reservoir);
                evidence.SlotsBefore = AvailableSlots(casting, SpellLevel);
                var target = new TargetWrapper(ally);
                var cutscene = new Kingmaker.AreaLogic.Cutscenes
                    .CutsceneParametersContext();
                using (cutscene.Data)
                    command = new UnitUseAbility(data, target);
                BrownFurCastTransaction transaction = Transaction(
                    "native-cast-" + request.RunId, caster, ally);
                if (!BrownFurCastExecutionRuntime.Begin(contract, command,
                        data, target, transaction, Plan()))
                    throw new InvalidOperationException(
                        "Validated Brown-Fur native cast could not retain its boundary.");
                evidence.Targetable = data.CanTarget(target);
                evidence.CanStart = data.IsAvailable && command.CanStart;
                if (!evidence.Targetable || !evidence.CanStart)
                    throw new InvalidOperationException(
                        "Scoped native cast was unavailable: targetable=" +
                        evidence.Targetable + ";canStart=" +
                        evidence.CanStart + ".");
                command.IgnoreCooldown(TimeSpan.Zero);
                caster.Commands.Run(command);
                command.Start();
                evidence.Started = command.IsRunning;
                if (!evidence.Started) throw new InvalidOperationException(
                    "Native UnitUseAbility did not enter its running state.");
                if (command.Animation != null)
                    command.Animation.IsActed = true;
                command.Tick();
                evidence.CommandResult = command.Result.ToString();
                evidence.ProcessStarted = command.ExecutionProcess != null;
                if (command.ExecutionProcess == null)
                    throw new InvalidOperationException(
                        "Native cast did not create an execution process; result=" +
                        evidence.CommandResult + ".");
                for (int tick = 0; tick < 5000 &&
                    !command.ExecutionProcess.IsEnded; tick++)
                    command.ExecutionProcess.Tick();
                evidence.ProcessEnded = command.ExecutionProcess.IsEnded;
                FinishAnimation(command);
                if (!command.IsFinished) command.Tick();
                evidence.TransactionState = transaction.State.ToString();
                evidence.ReservoirAfter = caster.Descriptor.Resources
                    .GetResourceAmount(contract.Reservoir);
                evidence.SlotsAfter = AvailableSlots(casting, SpellLevel);

                stage = "effect";
                Buff[] allyBuffs = ally.Descriptor.Buffs.RawFacts.OfType<Buff>()
                    .Where(value => ReferenceEquals(value.Blueprint, buff))
                    .ToArray();
                Buff[] casterBuffs = caster.Descriptor.Buffs.RawFacts
                    .OfType<Buff>().Where(value => ReferenceEquals(
                        value.Blueprint, buff)).ToArray();
                evidence.AllyBuffCount = allyBuffs.Length;
                evidence.CasterBuffCount = casterBuffs.Length;
                applied = allyBuffs.SingleOrDefault();
                if (applied != null)
                {
                    ModifiableValue.Modifier[] modifiers = ally.Descriptor.Stats
                        .Strength.Modifiers.Where(value => ReferenceEquals(
                            value.Source, applied)).ToArray();
                    evidence.ModifierCount = modifiers.Length;
                    ModifiableValue.Modifier modifier = modifiers
                        .SingleOrDefault();
                    if (modifier != null)
                    {
                        evidence.ModifierValue = modifier.ModValue;
                        evidence.ModifierDescriptor = modifier.ModDescriptor
                            .ToString();
                    }
                    evidence.BuffCasterExact = applied.Context != null &&
                        ReferenceEquals(applied.Context.MaybeCaster, caster);
                    evidence.BuffTargetExact = applied.Context != null &&
                        applied.Context.MainTarget.Unit == ally;
                    evidence.BuffSourceSpellExact = applied.Context != null &&
                        ReferenceEquals(applied.Context.SourceAbility, selected);
                    evidence.BuffTimeSeconds = applied.TimeLeft.TotalSeconds;
                }

                stage = "native-interruption";
                if (applied != null)
                {
                    applied.Remove();
                    applied = null;
                }
                casting.Rest();
                if (!casting.IsKnown(root))
                    casting.AddKnown(SpellLevel, root, true);
                int currentReservoir = caster.Descriptor.Resources
                    .GetResourceAmount(contract.Reservoir);
                if (currentReservoir < evidence.ReservoirBefore)
                    caster.Descriptor.Resources.Restore(contract.Reservoir,
                        evidence.ReservoirBefore - currentReservoir);
                if (caster.Descriptor.AddFact(blueprints.PowerfulChange) == null ||
                    caster.Descriptor.AddFact(blueprints.ShareTransmutation) == null ||
                    caster.Descriptor.AddFact(
                        blueprints.TransmutationSupremacy) == null ||
                    caster.Descriptor.AddFact(blueprints.ScoreBuffs[0]) == null)
                    throw new InvalidOperationException(
                        "Real Brown-Fur interruption facts could not be granted.");
                ActivatableAbility share = caster.Descriptor
                    .ActivatableAbilities.Enumerable.SingleOrDefault(value =>
                        value != null && ReferenceEquals(value.Blueprint,
                            blueprints.ShareTransmutationAbility));
                if (share == null) throw new InvalidOperationException(
                    "Real Share Transmutation interruption toggle was unavailable.");
                share.IsOn = true;
                int interruptionReservoirBefore = caster.Descriptor.Resources
                    .GetResourceAmount(contract.Reservoir);
                int interruptionSlotsBefore = AvailableSlots(casting,
                    SpellLevel);
                var interruptionData = new AbilityData(
                    new AbilityData(root, casting), selected);
                UnitUseAbility interruptedCommand;
                using (cutscene.Data)
                    interruptedCommand = new UnitUseAbility(interruptionData,
                        target);
                evidence.InterruptionArmed =
                    BrownFurCastExecutionRuntime.ActiveTransactionCount == 1 &&
                    BrownFurCastExecutionRuntime.ReservationCount == 1 &&
                    BrownFurShareTargetingRuntime.ActiveScopeCount == 1 &&
                    BrownFurSupremacyRuntime.ActiveScopeCount == 1;
                evidence.InterruptionOutcome =
                    BrownFurCastIntentRuntime.LastOutcome;
                evidence.InterruptionCleared =
                    !blueprints.ScoreBuffs.Any(value =>
                        caster.Descriptor.HasFact(value)) &&
                    !caster.Descriptor.HasFact(
                        blueprints.ShareTransmutationBuff) && !share.IsOn;
                evidence.InterruptionTargetable =
                    interruptionData.CanTarget(target);
                evidence.InterruptionCanStart = interruptionData.IsAvailable &&
                    interruptedCommand.CanStart;
                if (!evidence.InterruptionTargetable ||
                    !evidence.InterruptionCanStart)
                    throw new InvalidOperationException(
                        "Automatic Brown-Fur interruption command was unavailable.");
                interruptedCommand.IgnoreCooldown(TimeSpan.Zero);
                caster.Commands.Run(interruptedCommand);
                interruptedCommand.Start();
                evidence.InterruptionStarted = interruptedCommand.IsRunning;
                caster.Commands.InterruptAll(true);
                evidence.InterruptionResult =
                    interruptedCommand.Result.ToString();
                evidence.InterruptionFinished = interruptedCommand.IsFinished;
                int interruptionReservoirAfter = caster.Descriptor.Resources
                    .GetResourceAmount(contract.Reservoir);
                int interruptionSlotsAfter = AvailableSlots(casting,
                    SpellLevel);
                evidence.InterruptionReservoir = interruptionReservoirBefore +
                    "->" + interruptionReservoirAfter;
                evidence.InterruptionSlots = interruptionSlotsBefore + "->" +
                    interruptionSlotsAfter;
                evidence.InterruptionFinalState = "active=" +
                    BrownFurCastExecutionRuntime.ActiveTransactionCount +
                    ";reservations=" +
                    BrownFurCastExecutionRuntime.ReservationCount +
                    ";share=" +
                    BrownFurShareTargetingRuntime.ActiveScopeCount +
                    ";supremacy=" +
                    BrownFurSupremacyRuntime.ActiveScopeCount +
                    ";modifier=" +
                    BrownFurModifierAdjustmentRuntime.ActiveScopeCount;
            }
            catch (Exception exception)
            {
                diagnostics.Add("stage=" + stage + ";exception=" +
                    Describe(exception));
            }
            finally
            {
                if (levelController != null) TryCancel(levelController);
                if (applied != null) applied.Remove();
                if (caster != null) caster.Commands.InterruptAll(true);
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
                CleanupUnit(ally);
                CleanupUnit(caster);
                Game.Instance.EntityDestroyer.Tick();
                Game.Instance.EntityDestroyer.Tick();
                evidence.UnitsRemoved = (!allyCreated || ally == null ||
                    ally.Destroyed) && (!casterCreated || caster == null ||
                    caster.Destroyed) && (ally == null ||
                    !Game.Instance.State.Units.All.Contains(ally)) &&
                    (caster == null ||
                    !Game.Instance.State.Units.All.Contains(caster));
                if (allyBlueprint != null)
                    UnityEngine.Object.Destroy(allyBlueprint);
                if (casterBlueprint != null)
                    UnityEngine.Object.Destroy(casterBlueprint);
            }

            Add(assertions, "native-cast-fixture",
                "live disposable level-five CotW Arcanist and ally; both books",
                "level=" + evidence.ClassLevel + ";caster=" +
                    evidence.CasterInState + ";ally=" +
                    evidence.AllyInState + ";casting=" +
                    evidence.CastingBook + ";preparation=" +
                    evidence.PreparationBook,
                evidence.ClassLevel == 5 && evidence.CasterInState &&
                    evidence.AllyInState && contract != null &&
                    evidence.CastingBook == contract.CastingSpellbook.AssetGuid &&
                    evidence.PreparationBook ==
                        contract.MemorizationSpellbook.AssetGuid,
                "guarded working-save area with disposable native units");
            Add(assertions, "native-cast-source-target",
                "exact casting book, wrapper, wolf variant, and ally target",
                evidence.SourceBook + ";canonical=" +
                    evidence.CanonicalSpell + ";selected=" +
                    evidence.SelectedSpell + ";target=" + evidence.Target +
                    ";targetable=" + evidence.Targetable,
                evidence.SourceBook == evidence.CastingBook &&
                    evidence.CanonicalSpell == CanonicalSpellGuid &&
                    evidence.SelectedSpell == SelectedSpellGuid &&
                    !string.IsNullOrWhiteSpace(evidence.Target) &&
                    evidence.Targetable,
                "spellbook-backed AbilityData parent/variant chain");
            Add(assertions, "native-cast-command-process",
                "command succeeds and execution process reaches terminal completion",
                "canStart=" + evidence.CanStart + ";started=" +
                    evidence.Started + ";result=" + evidence.CommandResult +
                    ";process=" + evidence.ProcessStarted + "->" +
                    evidence.ProcessEnded + ";transaction=" +
                    evidence.TransactionState,
                evidence.CanStart && evidence.Started &&
                    evidence.CommandResult ==
                        UnitCommand.ResultType.Success.ToString() &&
                    evidence.ProcessStarted && evidence.ProcessEnded &&
                    evidence.TransactionState ==
                        BrownFurCastTransactionState.Completed.ToString(),
                "native UnitUseAbility, RuleCastSpell, and process Tick");
            Add(assertions, "native-cast-accounting",
                "combined cost two and exactly one level-three slot",
                "reservoir=" + evidence.ReservoirBefore + "->" +
                    evidence.ReservoirAfter + ";slots=" +
                    evidence.SlotsBefore + "->" + evidence.SlotsAfter,
                evidence.ReservoirAfter == evidence.ReservoirBefore - 2 &&
                    evidence.SlotsAfter == evidence.SlotsBefore - 1,
                "real CotW reservoir plus native AbilityData.Spend");
            Add(assertions, "native-cast-ally-effect",
                "one wolf buff on ally, none on caster, +4 Polymorph Strength",
                "ally=" + evidence.AllyBuffCount + ";caster=" +
                    evidence.CasterBuffCount + ";modifiers=" +
                    evidence.ModifierCount + ";value=" +
                    evidence.ModifierValue + ";descriptor=" +
                    evidence.ModifierDescriptor + ";context=" +
                    evidence.BuffCasterExact + "/" +
                    evidence.BuffTargetExact + "/" +
                    evidence.BuffSourceSpellExact + ";seconds=" +
                    evidence.BuffTimeSeconds,
                evidence.AllyBuffCount == 1 && evidence.CasterBuffCount == 0 &&
                    evidence.ModifierCount == 1 &&
                    evidence.ModifierValue == 4 &&
                    evidence.ModifierDescriptor ==
                        ModifierDescriptor.Polymorph.ToString() &&
                    evidence.BuffCasterExact && evidence.BuffTargetExact &&
                    evidence.BuffSourceSpellExact &&
                    evidence.BuffTimeSeconds > 0d,
                "real Personal spell effect redirected by exact cast scope");
            Add(assertions, "native-cast-interruption-no-spend",
                "submitted command interruption clears intent with no debit or slot spend",
                "armed=" + evidence.InterruptionArmed + ";outcome=" +
                    evidence.InterruptionOutcome + ";cleared=" +
                    evidence.InterruptionCleared + ";targetable=" +
                    evidence.InterruptionTargetable + ";canStart=" +
                    evidence.InterruptionCanStart + ";started=" +
                    evidence.InterruptionStarted + ";result=" +
                    evidence.InterruptionResult + ";finished=" +
                    evidence.InterruptionFinished + ";reservoir=" +
                    evidence.InterruptionReservoir + ";slots=" +
                    evidence.InterruptionSlots + ";state=" +
                    evidence.InterruptionFinalState,
                evidence.InterruptionArmed &&
                    evidence.InterruptionOutcome != null &&
                    evidence.InterruptionOutcome.StartsWith(
                        "armed:brown-fur-") &&
                    evidence.InterruptionOutcome.EndsWith(";cost=2") &&
                    evidence.InterruptionCleared &&
                    evidence.InterruptionTargetable &&
                    evidence.InterruptionCanStart &&
                    evidence.InterruptionStarted &&
                    evidence.InterruptionResult !=
                        UnitCommand.ResultType.Success.ToString() &&
                    evidence.InterruptionFinished &&
                    Delta(evidence.InterruptionReservoir) == 0 &&
                    Delta(evidence.InterruptionSlots) == 0 &&
                    evidence.InterruptionFinalState ==
                        "active=0;reservations=0;share=0;supremacy=0;modifier=0",
                "actual owner facts, native command queue/start/interrupt, and OnEnded cleanup");
            Add(assertions, "native-cast-cleanup",
                "all scopes and disposable units removed",
                "active=" + evidence.FinalActive + ";reservations=" +
                    evidence.FinalReservations + ";scopes=" +
                    evidence.FinalScopes + ";units=" +
                    evidence.UnitsRemoved,
                evidence.FinalActive == 0 && evidence.FinalReservations == 0 &&
                    evidence.FinalScopes ==
                        "share=0;supremacy=0;modifier=0;suppressed=0" &&
                    evidence.UnitsRemoved,
                "process terminal release and bounded fixture cleanup");

            string path = Path.Combine(request.EvidenceDirectory, FileName);
            File.WriteAllText(path, JsonConvert.SerializeObject(evidence,
                Formatting.Indented));
            evidenceFiles.Add(path);
            diagnostics.Add("nativeCastSha256=" + Hash(path));
            bool pass = assertions.All(value =>
                value.Status == RuntimeTestStatuses.Pass);
            Assembly assembly = context.Assembly;
            return new RuntimeTestResult {
                SchemaVersion = 1, RunId = request.RunId,
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
            return book.Blueprint.Spontaneous ?
                book.GetSpontaneousSlots(level) :
                book.GetMemorizedSpellSlots(level).Count(value =>
                value != null && value.Available);
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

        private static BrownFurCastTransaction Transaction(string identity,
            UnitEntityData caster, UnitEntityData ally)
        {
            var intent = new BrownFurCastIntent(identity,
                caster == null ? string.Empty : caster.UniqueId,
                CanonicalSpellGuid, SelectedSpellGuid,
                "cotw-casting-spellbook",
                ally == null ? string.Empty : ally.UniqueId, true,
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

        private static void FinishAnimation(UnitUseAbility command)
        {
            if (command == null || command.Animation == null) return;
            PropertyInfo property = null;
            for (Type type = command.Animation.GetType();
                type != null && property == null; type = type.BaseType)
                property = type.GetProperty("IsFinished",
                    BindingFlags.Public | BindingFlags.NonPublic |
                    BindingFlags.Instance | BindingFlags.DeclaredOnly);
            MethodInfo setter = property == null ? null :
                property.GetSetMethod(true);
            if (setter == null) throw new MissingMethodException(
                command.Animation.GetType().FullName, "set_IsFinished");
            setter.Invoke(command.Animation, new object[] { true });
        }

        private static void CleanupUnit(UnitEntityData unit)
        {
            if (unit == null || unit.Destroyed) return;
            var detached = new List<ItemEntity>();
            if (unit.Body != null && unit.Body.PrimaryHand != null &&
                unit.Body.PrimaryHand.MaybeItem != null)
            {
                detached.Add(unit.Body.PrimaryHand.MaybeItem);
                unit.Body.PrimaryHand.RemoveItem(false);
            }
            if (unit.Body != null && unit.Body.SecondaryHand != null &&
                unit.Body.SecondaryHand.MaybeItem != null)
            {
                detached.Add(unit.Body.SecondaryHand.MaybeItem);
                unit.Body.SecondaryHand.RemoveItem(false);
            }
            if (unit.Descriptor.Inventory != null)
                foreach (ItemEntity item in unit.Descriptor.Inventory.Items
                    .ToArray())
                {
                    unit.Descriptor.Inventory.Remove(item);
                    detached.Add(item);
                }
            foreach (ItemEntity item in detached.Where(value => value != null)
                .Distinct().ToArray()) item.Dispose();
            unit.Destroy();
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

        private static string Describe(Exception exception)
        {
            var values = new List<string>();
            for (Exception current = exception; current != null;
                current = current.InnerException)
                values.Add(current.GetType().FullName + ":" + current.Message);
            return string.Join(" -> ", values.ToArray());
        }

        private static void Add(List<RuntimeTestAssertion> assertions,
            string name, string expected, string observed, bool pass,
            string proof)
        {
            assertions.Add(new RuntimeTestAssertion { Name = name,
                Expected = expected, Observed = observed,
                Status = pass ? RuntimeTestStatuses.Pass :
                    RuntimeTestStatuses.Fail,
                Evidence = proof });
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
