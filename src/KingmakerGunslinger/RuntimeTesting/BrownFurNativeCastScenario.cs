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
            "5d4028eb28a106d4691ed1b92bbb1915";
        private const string SelectedSpellGuid =
            "6ceb82df566a42c8a77ccb7b76b09c1b";
        private const string BuffGuid =
            "8dc6510d31614345a8c718208fbac1f8";
        private const int SpellLevel = 4;

        [JsonObject(MemberSerialization.OptIn)]
        private sealed class SharedSpellEvidence
        {
            [JsonProperty("name", Order = 1)] public string Name { get; set; }
            [JsonProperty("spellGuid", Order = 2)] public string SpellGuid { get; set; }
            [JsonProperty("buffGuid", Order = 3)] public string BuffGuid { get; set; }
            [JsonProperty("targetAnchor", Order = 4)] public string TargetAnchor { get; set; }
            [JsonProperty("targetable", Order = 5)] public bool Targetable { get; set; }
            [JsonProperty("intentPreservedBeforeCommit", Order = 6)] public bool IntentPreserved { get; set; }
            [JsonProperty("commandResult", Order = 7)] public string CommandResult { get; set; }
            [JsonProperty("processEnded", Order = 8)] public bool ProcessEnded { get; set; }
            [JsonProperty("allyBuffCount", Order = 9)] public int AllyBuffCount { get; set; }
            [JsonProperty("casterBuffCount", Order = 10)] public int CasterBuffCount { get; set; }
            [JsonProperty("reservoirBeforeAfter", Order = 11)] public string Reservoir { get; set; }
            [JsonProperty("slotsBeforeAfter", Order = 12)] public string Slots { get; set; }
            [JsonProperty("shareOffAfterCommit", Order = 13)] public bool ShareOffAfterCommit { get; set; }
            [JsonProperty("resourceCounterBeforeAfter", Order = 14)] public string ResourceCounter { get; set; }
        }

        [JsonObject(MemberSerialization.OptIn)]
        private sealed class AbilityBonusCastEvidence
        {
            [JsonProperty("name", Order = 1)] public string Name { get; set; }
            [JsonProperty("spellGuid", Order = 2)] public string SpellGuid { get; set; }
            [JsonProperty("score", Order = 3)] public string Score { get; set; }
            [JsonProperty("armed", Order = 4)] public bool Armed { get; set; }
            [JsonProperty("targetAnchor", Order = 5)] public string TargetAnchor { get; set; }
            [JsonProperty("targetable", Order = 6)] public bool Targetable { get; set; }
            [JsonProperty("transactionCountAfterCommand", Order = 7)] public int TransactionCountAfterCommand { get; set; }
            [JsonProperty("commandResult", Order = 8)] public string CommandResult { get; set; }
            [JsonProperty("processEnded", Order = 9)] public bool ProcessEnded { get; set; }
            [JsonProperty("modifierValue", Order = 10)] public int ModifierValue { get; set; }
            [JsonProperty("modifierDescriptor", Order = 11)] public string ModifierDescriptor { get; set; }
            [JsonProperty("reservoirBeforeAfter", Order = 12)] public string Reservoir { get; set; }
            [JsonProperty("resourceCounterBeforeAfter", Order = 13)] public string ResourceCounter { get; set; }
            [JsonProperty("slotsBeforeAfter", Order = 14)] public string Slots { get; set; }
            [JsonProperty("toggleOffAfter", Order = 15)] public bool ToggleOffAfter { get; set; }
            [JsonProperty("sourceSpellExact", Order = 16)] public bool SourceSpellExact { get; set; }
        }

        [JsonObject(MemberSerialization.OptIn)]
        private sealed class ShareOffCastEvidence
        {
            [JsonProperty("spellGuid", Order = 1)] public string SpellGuid { get; set; }
            [JsonProperty("targetAnchor", Order = 2)] public string TargetAnchor { get; set; }
            [JsonProperty("transactionCountAfterCommand", Order = 3)] public int TransactionCountAfterCommand { get; set; }
            [JsonProperty("commandResult", Order = 4)] public string CommandResult { get; set; }
            [JsonProperty("processEnded", Order = 5)] public bool ProcessEnded { get; set; }
            [JsonProperty("casterBuffCount", Order = 6)] public int CasterBuffCount { get; set; }
            [JsonProperty("allyBuffCount", Order = 7)] public int AllyBuffCount { get; set; }
            [JsonProperty("reservoirBeforeAfter", Order = 8)] public string Reservoir { get; set; }
            [JsonProperty("slotsBeforeAfter", Order = 9)] public string Slots { get; set; }
            [JsonProperty("shareRemainedOff", Order = 10)] public bool ShareRemainedOff { get; set; }
        }

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
            [JsonProperty("interruptionIntentPreservedBeforeCommit", Order = 36)] public bool InterruptionPreserved { get; set; }
            [JsonProperty("interruptionTargetable", Order = 37)] public bool InterruptionTargetable { get; set; }
            [JsonProperty("interruptionCanStart", Order = 38)] public bool InterruptionCanStart { get; set; }
            [JsonProperty("interruptionStarted", Order = 39)] public bool InterruptionStarted { get; set; }
            [JsonProperty("interruptionResult", Order = 40)] public string InterruptionResult { get; set; }
            [JsonProperty("interruptionFinished", Order = 41)] public bool InterruptionFinished { get; set; }
            [JsonProperty("interruptionReservoirBeforeAfter", Order = 42)] public string InterruptionReservoir { get; set; }
            [JsonProperty("interruptionSlotsBeforeAfter", Order = 43)] public string InterruptionSlots { get; set; }
            [JsonProperty("interruptionFinalState", Order = 44)] public string InterruptionFinalState { get; set; }
            [JsonProperty("interruptionIntentPreservedAfterCancellation", Order = 45)] public bool InterruptionPreservedAfter { get; set; }
            [JsonProperty("requiredSharedSpells", Order = 46)] public List<SharedSpellEvidence> RequiredSharedSpells { get; set; }
            [JsonProperty("combinedIntentPreservedBeforeCommit", Order = 47)] public bool CombinedIntentPreserved { get; set; }
            [JsonProperty("combinedTogglesOffAfterCommit", Order = 48)] public bool CombinedTogglesOff { get; set; }
            [JsonProperty("combinedResourceCountersBeforeAfter", Order = 49)] public string CombinedResourceCounters { get; set; }
            [JsonProperty("abilityBonusCasts", Order = 50)] public List<AbilityBonusCastEvidence> AbilityBonusCasts { get; set; }
            [JsonProperty("shareOffSelfCast", Order = 51)] public ShareOffCastEvidence ShareOffSelfCast { get; set; }
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
                ModifierDescriptor = string.Empty,
                RequiredSharedSpells = new List<SharedSpellEvidence>(),
                AbilityBonusCasts = new List<AbilityBonusCastEvidence>()
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
            ActivatableAbility share = null;
            ActivatableAbility powerful = null;
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
                if (blueprints == null ||
                    blueprints.Count != BrownFurIdentityCatalog.IdentityCount)
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
                        "Exact Beast Shape II wrapper, dire-wolf variant, buff, and " +
                        "level-four spell-list entry were not available.");

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
                EnsureFeature(caster.Descriptor,
                    blueprints.PowerfulChange);
                EnsureFeature(caster.Descriptor,
                    blueprints.ShareTransmutation);
                powerful = BrownFurPlayerIntentRuntime.Find(
                    caster.Descriptor, blueprints.ScoreActivatables[0]);
                share = BrownFurPlayerIntentRuntime.Find(caster.Descriptor,
                    blueprints.ShareTransmutationAbility);
                if (powerful == null || share == null)
                    throw new InvalidOperationException(
                        "Real combined Brown-Fur toggles were unavailable.");
                powerful.IsOn = true;
                share.IsOn = true;
                var rootData = new AbilityData(root, casting);
                var data = new AbilityData(rootData, selected);
                evidence.SourceBook = data.Spellbook == null ? string.Empty :
                    data.Spellbook.Blueprint.AssetGuid;
                evidence.Target = ally.UniqueId;
                evidence.ReservoirBefore = caster.Descriptor.Resources
                    .GetResourceAmount(contract.Reservoir);
                evidence.SlotsBefore = AvailableSlots(casting, SpellLevel);
                evidence.CombinedResourceCounters =
                    ResourceCount(powerful) + "/" + ResourceCount(share);
                var target = new TargetWrapper(ally);
                var cutscene = new Kingmaker.AreaLogic.Cutscenes
                    .CutsceneParametersContext();
                using (cutscene.Data)
                    command = new UnitUseAbility(data, target);
                evidence.CombinedIntentPreserved = powerful.IsOn && share.IsOn &&
                    caster.Descriptor.HasFact(blueprints.ScoreBuffs[0]) &&
                    caster.Descriptor.HasFact(
                        blueprints.ShareTransmutationBuff);
                if (BrownFurCastExecutionRuntime.ActiveTransactionCount != 1 ||
                    BrownFurCastExecutionRuntime.ReservationCount != 1)
                    throw new InvalidOperationException(
                        "Automatic combined Brown-Fur cast did not retain exactly one transaction.");
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
                evidence.TransactionState =
                    BrownFurCastExecutionRuntime.LastTerminalState;
                evidence.ReservoirAfter = caster.Descriptor.Resources
                    .GetResourceAmount(contract.Reservoir);
                evidence.SlotsAfter = AvailableSlots(casting, SpellLevel);
                evidence.CombinedResourceCounters += "->" +
                    ResourceCount(powerful) + "/" + ResourceCount(share);
                evidence.CombinedTogglesOff = !powerful.IsOn && !share.IsOn &&
                    !caster.Descriptor.HasFact(blueprints.ScoreBuffs[0]) &&
                    !caster.Descriptor.HasFact(
                        blueprints.ShareTransmutationBuff);

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

                stage = "required-shared-spells";
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
                share = BrownFurPlayerIntentRuntime.Find(caster.Descriptor,
                    blueprints.ShareTransmutationAbility);
                if (share == null) throw new InvalidOperationException(
                    "Real Share Transmutation toggle was unavailable.");
                evidence.RequiredSharedSpells.Add(RunSharedSpell(caster, ally,
                    casting, contract, blueprints, share,
                    "Undead Anatomy I",
                    "8d535e198bb44ba2b6cf6ea603753fe4",
                    "229a8152555b4d5da573113ee03983b3", 3));
                evidence.RequiredSharedSpells.Add(RunSharedSpell(caster, ally,
                    casting, contract, blueprints, share, "Resinous Skin",
                    "41ceee31b77741e99d3b0990bbe40a2a",
                    "72067851f2904755a372f4ea4818345e", 3));

                stage = "share-off-native-self-cast";
                evidence.ShareOffSelfCast = RunShareOffSelfCast(caster, ally,
                    casting, contract, blueprints, share,
                    "41ceee31b77741e99d3b0990bbe40a2a",
                    "72067851f2904755a372f4ea4818345e", 3);

                stage = "bulls-strength-and-cats-grace";
                casting.Rest();
                currentReservoir = caster.Descriptor.Resources
                    .GetResourceAmount(contract.Reservoir);
                if (currentReservoir < evidence.ReservoirBefore)
                    caster.Descriptor.Resources.Restore(contract.Reservoir,
                        evidence.ReservoirBefore - currentReservoir);
                evidence.AbilityBonusCasts.Add(RunAbilityBonusSpell(
                    caster, ally, casting, contract, blueprints,
                    "Bull's Strength OFF",
                    "4c3d08935262b6544ae97599b3a9556d", 2,
                    StatType.Strength, 0, false, 4));
                evidence.AbilityBonusCasts.Add(RunAbilityBonusSpell(
                    caster, ally, casting, contract, blueprints,
                    "Bull's Strength ON",
                    "4c3d08935262b6544ae97599b3a9556d", 2,
                    StatType.Strength, 0, true, 6));
                evidence.AbilityBonusCasts.Add(RunAbilityBonusSpell(
                    caster, ally, casting, contract, blueprints,
                    "Cat's Grace ON",
                    "de7a025d48ad5da4991e7d3c682cf69d", 2,
                    StatType.Dexterity, 1, true, 6));

                stage = "native-interruption";
                casting.Rest();
                caster.Commands.InterruptAll(true);
                currentReservoir = caster.Descriptor.Resources
                    .GetResourceAmount(contract.Reservoir);
                if (currentReservoir < evidence.ReservoirBefore)
                    caster.Descriptor.Resources.Restore(contract.Reservoir,
                        evidence.ReservoirBefore - currentReservoir);
                EnsureFeature(caster.Descriptor,
                    blueprints.PowerfulChange);
                EnsureFeature(caster.Descriptor,
                    blueprints.ShareTransmutation);
                EnsureFeature(caster.Descriptor,
                    blueprints.TransmutationSupremacy);
                if (!caster.Descriptor.HasFact(blueprints.PowerfulChange) ||
                    !caster.Descriptor.HasFact(blueprints.ShareTransmutation) ||
                    !caster.Descriptor.HasFact(
                        blueprints.TransmutationSupremacy))
                    throw new InvalidOperationException(
                        "Real Brown-Fur interruption facts could not be granted.");
                powerful = BrownFurPlayerIntentRuntime.Find(
                    caster.Descriptor, blueprints.ScoreActivatables[0]);
                if (powerful == null) throw new InvalidOperationException(
                    "Real Strength interruption toggle was unavailable.");
                powerful.IsOn = true;
                share = BrownFurPlayerIntentRuntime.Find(caster.Descriptor,
                    blueprints.ShareTransmutationAbility);
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
                evidence.InterruptionPreserved = powerful.IsOn && share.IsOn &&
                    caster.Descriptor.HasFact(blueprints.ScoreBuffs[0]) &&
                    caster.Descriptor.HasFact(
                        blueprints.ShareTransmutationBuff);
                evidence.InterruptionTargetable =
                    interruptionData.CanTarget(target);
                interruptedCommand.IgnoreCooldown(TimeSpan.Zero);
                evidence.InterruptionCanStart = interruptedCommand.CanStart;
                if (!evidence.InterruptionTargetable ||
                    !evidence.InterruptionCanStart ||
                    interruptionSlotsBefore <= 0)
                    throw new InvalidOperationException(
                        "Automatic Brown-Fur interruption command was unavailable.");
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
                evidence.InterruptionPreservedAfter = powerful.IsOn &&
                    share.IsOn &&
                    caster.Descriptor.HasFact(blueprints.ScoreBuffs[0]) &&
                    caster.Descriptor.HasFact(
                        blueprints.ShareTransmutationBuff);
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
                    evidence.TransactionState.EndsWith(":" +
                        BrownFurCastTransactionState.Completed),
                "native UnitUseAbility, RuleCastSpell, and process Tick");
            Add(assertions, "native-cast-accounting",
                "combined cost two and exactly one level-four slot; both toggles clear and both counters track the shared debit",
                "reservoir=" + evidence.ReservoirBefore + "->" +
                    evidence.ReservoirAfter + ";slots=" +
                    evidence.SlotsBefore + "->" + evidence.SlotsAfter,
                evidence.ReservoirAfter == evidence.ReservoirBefore - 2 &&
                    evidence.SlotsAfter == evidence.SlotsBefore - 1 &&
                    evidence.CombinedIntentPreserved &&
                    evidence.CombinedTogglesOff &&
                    ResourcePairDelta(evidence.CombinedResourceCounters) == -2,
                "real CotW reservoir plus native AbilityData.Spend");
            Add(assertions, "native-cast-ally-effect",
                "one dire-wolf buff on ally, none on caster, +6 Polymorph Strength",
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
                    evidence.ModifierValue == 6 &&
                    evidence.ModifierDescriptor ==
                        ModifierDescriptor.Polymorph.ToString() &&
                    evidence.BuffCasterExact && evidence.BuffTargetExact &&
                    evidence.BuffSourceSpellExact &&
                    evidence.BuffTimeSeconds > 0d,
                "real Personal spell effect redirected by exact cast scope");
            Add(assertions, "native-cast-required-share-spells",
                "Undead Anatomy I and Resinous Skin target the ally, cost one, spend one slot, and disarm Share",
                string.Join("|", evidence.RequiredSharedSpells.Select(value =>
                    value.Name + ":anchor=" + value.TargetAnchor +
                    ":target=" + value.Targetable + ":preserved=" +
                    value.IntentPreserved + ":result=" +
                    value.CommandResult + ":ended=" + value.ProcessEnded +
                    ":buffs=" + value.AllyBuffCount + "/" +
                    value.CasterBuffCount + ":reservoir=" +
                    value.Reservoir + ":slots=" + value.Slots +
                    ":counter=" + value.ResourceCounter + ":off=" +
                    value.ShareOffAfterCommit).ToArray()),
                evidence.RequiredSharedSpells.Count == 2 &&
                    evidence.RequiredSharedSpells.All(value =>
                        value.TargetAnchor == "Unit" && value.Targetable &&
                        value.IntentPreserved && value.ProcessEnded &&
                        value.AllyBuffCount == 1 &&
                        value.CasterBuffCount == 0 &&
                        Delta(value.Reservoir) == -1 &&
                        Delta(value.ResourceCounter) == -1 &&
                        Delta(value.Slots) == -1 &&
                        value.ShareOffAfterCommit),
                "actual action-bar-equivalent AbilityData, UnitUseAbility, RuleCastSpell, and selected ally effects");
            Add(assertions, "native-cast-ability-bonus-human-findings",
                "Bull's Strength OFF is +4/cost 0; Strength ON is +6 Enhancement/cost 1/off; Cat's Grace Dexterity ON is +6 Enhancement/cost 1/off",
                string.Join("|", evidence.AbilityBonusCasts.Select(value =>
                    value.Name + ":armed=" + value.Armed + ":anchor=" +
                    value.TargetAnchor + ":target=" + value.Targetable +
                    ":transaction=" + value.TransactionCountAfterCommand +
                    ":result=" + value.CommandResult + ":ended=" +
                    value.ProcessEnded + ":modifier=" +
                    value.ModifierValue + "/" + value.ModifierDescriptor +
                    ":reservoir=" + value.Reservoir + ":counter=" +
                    value.ResourceCounter + ":slots=" + value.Slots +
                    ":off=" + value.ToggleOffAfter + ":source=" +
                    value.SourceSpellExact).ToArray()),
                AbilityBonusCasesPass(evidence.AbilityBonusCasts),
                "actual spellbook casts, native score activatables, typed buff modifiers, reservoir, slots, and automatic cleanup");
            Add(assertions, "native-cast-share-off-self-cast",
                "Share OFF preserves native Owner self-cast, costs no reservoir, spends one slot, and affects only caster",
                Describe(evidence.ShareOffSelfCast),
                ShareOffCasePass(evidence.ShareOffSelfCast),
                "actual Personal spell AbilityData, native owner command, RuleCastSpell, and buff target");
            Add(assertions, "native-cast-interruption-no-spend",
                "submitted command interruption preserves intent with no debit or slot spend",
                "armed=" + evidence.InterruptionArmed + ";outcome=" +
                    evidence.InterruptionOutcome + ";preserved=" +
                    evidence.InterruptionPreserved + "/" +
                    evidence.InterruptionPreservedAfter + ";targetable=" +
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
                    evidence.InterruptionPreserved &&
                    evidence.InterruptionPreservedAfter &&
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

        private static SharedSpellEvidence RunSharedSpell(
            UnitEntityData caster, UnitEntityData ally, Spellbook casting,
            CotwArcanistContract contract, BrownFurBlueprintSet blueprints,
            ActivatableAbility share, string name, string spellGuid,
            string buffGuid, int level)
        {
            BlueprintAbility spell = ResourcesLibrary.TryGetBlueprint<
                BlueprintAbility>(spellGuid);
            BlueprintBuff buff = ResourcesLibrary.TryGetBlueprint<
                BlueprintBuff>(buffGuid);
            if (spell == null || buff == null ||
                spell.Range != AbilityRange.Personal ||
                spell.School != SpellSchool.Transmutation)
                throw new InvalidOperationException(
                    "Required shared spell fixture is unavailable: " + name);
            if (!casting.IsKnown(spell)) casting.AddKnown(level, spell, true);
            share.IsOn = true;
            var data = new AbilityData(spell, casting);
            var target = new TargetWrapper(ally);
            var row = new SharedSpellEvidence {
                Name = name, SpellGuid = spellGuid, BuffGuid = buffGuid,
                TargetAnchor = data.TargetAnchor.ToString(),
                Targetable = data.CanTarget(target),
                Reservoir = caster.Descriptor.Resources.GetResourceAmount(
                    contract.Reservoir).ToString(),
                Slots = AvailableSlots(casting, level).ToString(),
                ResourceCounter = share.ResourceCount.HasValue ?
                    share.ResourceCount.Value.ToString() : "none"
            };
            if (row.TargetAnchor != "Unit" || !row.Targetable)
                throw new InvalidOperationException(
                    name + " did not expose a willing-creature target path.");
            UnitUseAbility command;
            var cutscene = new Kingmaker.AreaLogic.Cutscenes
                .CutsceneParametersContext();
            using (cutscene.Data)
                command = new UnitUseAbility(data, target);
            row.IntentPreserved = share.IsOn &&
                caster.Descriptor.HasFact(
                    blueprints.ShareTransmutationBuff);
            if (BrownFurCastExecutionRuntime.ActiveTransactionCount != 1)
                throw new InvalidOperationException(
                    name + " did not arm exactly one cast transaction.");
            command.IgnoreCooldown(TimeSpan.Zero);
            caster.Commands.Run(command);
            command.Start();
            if (!command.IsRunning)
                throw new InvalidOperationException(
                    name + " command did not start.");
            if (command.Animation != null) command.Animation.IsActed = true;
            command.Tick();
            row.CommandResult = command.Result.ToString();
            if (command.ExecutionProcess == null)
                throw new InvalidOperationException(
                    name + " did not create an execution process.");
            for (int tick = 0; tick < 5000 &&
                !command.ExecutionProcess.IsEnded; tick++)
                command.ExecutionProcess.Tick();
            row.ProcessEnded = command.ExecutionProcess.IsEnded;
            FinishAnimation(command);
            if (!command.IsFinished) command.Tick();
            Buff[] allyBuffs = ally.Descriptor.Buffs.RawFacts.OfType<Buff>()
                .Where(value => ReferenceEquals(value.Blueprint, buff))
                .ToArray();
            Buff[] casterBuffs = caster.Descriptor.Buffs.RawFacts.OfType<Buff>()
                .Where(value => ReferenceEquals(value.Blueprint, buff))
                .ToArray();
            row.AllyBuffCount = allyBuffs.Length;
            row.CasterBuffCount = casterBuffs.Length;
            row.Reservoir += "->" + caster.Descriptor.Resources
                .GetResourceAmount(contract.Reservoir);
            row.ResourceCounter += "->" +
                (share.ResourceCount.HasValue ?
                    share.ResourceCount.Value.ToString() : "none");
            row.Slots += "->" + AvailableSlots(casting, level);
            row.ShareOffAfterCommit = !share.IsOn &&
                !caster.Descriptor.HasFact(
                    blueprints.ShareTransmutationBuff);
            foreach (Buff applied in allyBuffs.Concat(casterBuffs).ToArray())
                applied.Remove();
            if (BrownFurCastExecutionRuntime.ActiveTransactionCount != 0 ||
                BrownFurShareTargetingRuntime.ActiveScopeCount != 0)
                throw new InvalidOperationException(
                    name + " retained cast state after process completion.");
            return row;
        }

        private static AbilityBonusCastEvidence RunAbilityBonusSpell(
            UnitEntityData caster, UnitEntityData target, Spellbook casting,
            CotwArcanistContract contract, BrownFurBlueprintSet blueprints,
            string name, string spellGuid, int level, StatType stat,
            int scoreIndex, bool armed, int expectedModifier)
        {
            BlueprintAbility spell = ResourcesLibrary.TryGetBlueprint<
                BlueprintAbility>(spellGuid);
            if (spell == null || spell.School != SpellSchool.Transmutation)
                throw new InvalidOperationException(
                    "Required ability-bonus spell is unavailable: " + name);
            if (!casting.IsKnown(spell)) casting.AddKnown(level, spell, true);
            BrownFurPlayerIntentRuntime.Clear(caster.Descriptor, blueprints);
            ActivatableAbility toggle = BrownFurPlayerIntentRuntime.Find(
                caster.Descriptor, blueprints.ScoreActivatables[scoreIndex]);
            if (toggle == null) throw new InvalidOperationException(
                "Required Powerful Change toggle is unavailable: " + name);
            toggle.IsOn = armed;
            var data = new AbilityData(spell, casting);
            var castTarget = new TargetWrapper(target);
            var row = new AbilityBonusCastEvidence {
                Name = name, SpellGuid = spellGuid, Score = stat.ToString(),
                Armed = armed, TargetAnchor = data.TargetAnchor.ToString(),
                Targetable = data.CanTarget(castTarget),
                Reservoir = caster.Descriptor.Resources.GetResourceAmount(
                    contract.Reservoir).ToString(),
                ResourceCounter = ResourceCount(toggle),
                Slots = AvailableSlots(casting, level).ToString(),
                ModifierDescriptor = string.Empty
            };
            if (!row.Targetable) throw new InvalidOperationException(
                name + " did not accept the allied creature target.");
            UnitUseAbility command;
            var cutscene = new Kingmaker.AreaLogic.Cutscenes
                .CutsceneParametersContext();
            using (cutscene.Data)
                command = new UnitUseAbility(data, castTarget);
            row.TransactionCountAfterCommand =
                BrownFurCastExecutionRuntime.ActiveTransactionCount;
            if (row.TransactionCountAfterCommand != (armed ? 1 : 0))
                throw new InvalidOperationException(
                    name + " produced the wrong automatic transaction count.");
            command.IgnoreCooldown(TimeSpan.Zero);
            caster.Commands.Run(command);
            command.Start();
            if (!command.IsRunning) throw new InvalidOperationException(
                name + " command did not start.");
            if (command.Animation != null) command.Animation.IsActed = true;
            command.Tick();
            row.CommandResult = command.Result.ToString();
            if (command.ExecutionProcess == null)
                throw new InvalidOperationException(
                    name + " did not create an execution process.");
            for (int tick = 0; tick < 5000 &&
                !command.ExecutionProcess.IsEnded; tick++)
                command.ExecutionProcess.Tick();
            row.ProcessEnded = command.ExecutionProcess.IsEnded;
            FinishAnimation(command);
            if (!command.IsFinished) command.Tick();

            ModifiableValue targetStat = target.Descriptor.Stats.GetStat(stat);
            ModifiableValue.Modifier[] modifiers = targetStat.Modifiers.Where(
                value => value.Source is Buff &&
                    ((Buff)value.Source).Context != null &&
                    ReferenceEquals(((Buff)value.Source).Context.SourceAbility,
                        spell)).ToArray();
            ModifiableValue.Modifier modifier = modifiers.SingleOrDefault();
            if (modifier != null)
            {
                row.ModifierValue = modifier.ModValue;
                row.ModifierDescriptor = modifier.ModDescriptor.ToString();
                row.SourceSpellExact = true;
            }
            row.Reservoir += "->" + caster.Descriptor.Resources
                .GetResourceAmount(contract.Reservoir);
            row.ResourceCounter += "->" + ResourceCount(toggle);
            row.Slots += "->" + AvailableSlots(casting, level);
            row.ToggleOffAfter = !toggle.IsOn &&
                !caster.Descriptor.HasFact(blueprints.ScoreBuffs[scoreIndex]);
            foreach (Buff buff in target.Descriptor.Buffs.RawFacts.OfType<Buff>()
                .Where(value => value.Context != null &&
                    ReferenceEquals(value.Context.SourceAbility, spell))
                .ToArray()) buff.Remove();
            if (row.ModifierValue != expectedModifier ||
                row.ModifierDescriptor !=
                    ModifierDescriptor.Enhancement.ToString())
                throw new InvalidOperationException(name +
                    " produced an unexpected typed modifier: " +
                    row.ModifierValue + "/" + row.ModifierDescriptor + ".");
            return row;
        }

        private static ShareOffCastEvidence RunShareOffSelfCast(
            UnitEntityData caster, UnitEntityData ally, Spellbook casting,
            CotwArcanistContract contract, BrownFurBlueprintSet blueprints,
            ActivatableAbility share, string spellGuid, string buffGuid,
            int level)
        {
            BlueprintAbility spell = ResourcesLibrary.TryGetBlueprint<
                BlueprintAbility>(spellGuid);
            BlueprintBuff buff = ResourcesLibrary.TryGetBlueprint<
                BlueprintBuff>(buffGuid);
            if (spell == null || buff == null)
                throw new InvalidOperationException(
                    "Share-OFF self-cast fixture is unavailable.");
            BrownFurPlayerIntentRuntime.Clear(caster.Descriptor, blueprints);
            if (!casting.IsKnown(spell)) casting.AddKnown(level, spell, true);
            var data = new AbilityData(spell, casting);
            var target = new TargetWrapper(caster);
            var row = new ShareOffCastEvidence {
                SpellGuid = spellGuid,
                TargetAnchor = data.TargetAnchor.ToString(),
                Reservoir = caster.Descriptor.Resources.GetResourceAmount(
                    contract.Reservoir).ToString(),
                Slots = AvailableSlots(casting, level).ToString()
            };
            UnitUseAbility command;
            var cutscene = new Kingmaker.AreaLogic.Cutscenes
                .CutsceneParametersContext();
            using (cutscene.Data)
                command = new UnitUseAbility(data, target);
            row.TransactionCountAfterCommand =
                BrownFurCastExecutionRuntime.ActiveTransactionCount;
            command.IgnoreCooldown(TimeSpan.Zero);
            caster.Commands.Run(command);
            command.Start();
            if (!command.IsRunning) throw new InvalidOperationException(
                "Share-OFF native self-cast command did not start.");
            if (command.Animation != null) command.Animation.IsActed = true;
            command.Tick();
            row.CommandResult = command.Result.ToString();
            if (command.ExecutionProcess == null)
                throw new InvalidOperationException(
                    "Share-OFF native self-cast did not create a process.");
            for (int tick = 0; tick < 5000 &&
                !command.ExecutionProcess.IsEnded; tick++)
                command.ExecutionProcess.Tick();
            row.ProcessEnded = command.ExecutionProcess.IsEnded;
            FinishAnimation(command);
            if (!command.IsFinished) command.Tick();
            Buff[] casterBuffs = caster.Descriptor.Buffs.RawFacts.OfType<Buff>()
                .Where(value => ReferenceEquals(value.Blueprint, buff)).ToArray();
            Buff[] allyBuffs = ally.Descriptor.Buffs.RawFacts.OfType<Buff>()
                .Where(value => ReferenceEquals(value.Blueprint, buff)).ToArray();
            row.CasterBuffCount = casterBuffs.Length;
            row.AllyBuffCount = allyBuffs.Length;
            row.Reservoir += "->" + caster.Descriptor.Resources
                .GetResourceAmount(contract.Reservoir);
            row.Slots += "->" + AvailableSlots(casting, level);
            row.ShareRemainedOff = share != null && !share.IsOn &&
                !caster.Descriptor.HasFact(
                    blueprints.ShareTransmutationBuff);
            foreach (Buff applied in casterBuffs.Concat(allyBuffs).ToArray())
                applied.Remove();
            return row;
        }

        private static bool AbilityBonusCasesPass(
            IList<AbilityBonusCastEvidence> cases)
        {
            if (cases == null || cases.Count != 3) return false;
            AbilityBonusCastEvidence off = cases[0];
            AbilityBonusCastEvidence bull = cases[1];
            AbilityBonusCastEvidence cat = cases[2];
            return !off.Armed && off.TransactionCountAfterCommand == 0 &&
                off.CommandResult == UnitCommand.ResultType.Success.ToString() &&
                off.ProcessEnded && off.ModifierValue == 4 &&
                off.ModifierDescriptor == "Enhancement" &&
                Delta(off.Reservoir) == 0 && Delta(off.ResourceCounter) == 0 &&
                Delta(off.Slots) == -1 && off.ToggleOffAfter &&
                off.SourceSpellExact &&
                bull.Armed && bull.TransactionCountAfterCommand == 1 &&
                bull.CommandResult == UnitCommand.ResultType.Success.ToString() &&
                bull.ProcessEnded && bull.ModifierValue == 6 &&
                bull.ModifierDescriptor == "Enhancement" &&
                Delta(bull.Reservoir) == -1 &&
                Delta(bull.ResourceCounter) == -1 &&
                Delta(bull.Slots) == -1 && bull.ToggleOffAfter &&
                bull.SourceSpellExact &&
                cat.Armed && cat.TransactionCountAfterCommand == 1 &&
                cat.CommandResult == UnitCommand.ResultType.Success.ToString() &&
                cat.ProcessEnded && cat.ModifierValue == 6 &&
                cat.ModifierDescriptor == "Enhancement" &&
                Delta(cat.Reservoir) == -1 &&
                Delta(cat.ResourceCounter) == -1 &&
                Delta(cat.Slots) == -1 && cat.ToggleOffAfter &&
                cat.SourceSpellExact;
        }

        private static bool ShareOffCasePass(ShareOffCastEvidence value)
        {
            return value != null && value.TargetAnchor == "Owner" &&
                value.TransactionCountAfterCommand == 0 &&
                value.CommandResult == UnitCommand.ResultType.Success.ToString() &&
                value.ProcessEnded && value.CasterBuffCount == 1 &&
                value.AllyBuffCount == 0 && Delta(value.Reservoir) == 0 &&
                Delta(value.Slots) == -1 && value.ShareRemainedOff;
        }

        private static string Describe(ShareOffCastEvidence value)
        {
            return value == null ? "missing" : "anchor=" +
                value.TargetAnchor + ";transaction=" +
                value.TransactionCountAfterCommand + ";result=" +
                value.CommandResult + ";ended=" + value.ProcessEnded +
                ";buffs=" + value.CasterBuffCount + "/" +
                value.AllyBuffCount + ";reservoir=" + value.Reservoir +
                ";slots=" + value.Slots + ";off=" +
                value.ShareRemainedOff;
        }

        private static void EnsureFeature(UnitDescriptor owner,
            Kingmaker.Blueprints.Facts.BlueprintUnitFact feature)
        {
            if (owner == null || feature == null)
                throw new ArgumentNullException();
            if (!owner.HasFact(feature) && owner.AddFact(feature) == null)
                throw new InvalidOperationException(
                    "Required Brown-Fur feature could not be granted: " +
                    feature.name);
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

        private static string ResourceCount(ActivatableAbility ability)
        {
            return ability != null && ability.ResourceCount.HasValue ?
                ability.ResourceCount.Value.ToString() : "none";
        }

        private static int ResourcePairDelta(string transition)
        {
            string[] sides = (transition ?? string.Empty).Split(
                new[] { "->" }, StringSplitOptions.None);
            if (sides.Length != 2) return int.MaxValue;
            string[] before = sides[0].Split('/');
            string[] after = sides[1].Split('/');
            int beforePowerful;
            int beforeShare;
            int afterPowerful;
            int afterShare;
            if (before.Length != 2 || after.Length != 2 ||
                !int.TryParse(before[0], out beforePowerful) ||
                !int.TryParse(before[1], out beforeShare) ||
                !int.TryParse(after[0], out afterPowerful) ||
                !int.TryParse(after[1], out afterShare) ||
                beforePowerful != beforeShare || afterPowerful != afterShare)
                return int.MaxValue;
            return afterPowerful - beforePowerful;
        }

        private static void RemoveFeature(UnitDescriptor owner,
            Kingmaker.Blueprints.Facts.BlueprintUnitFact feature)
        {
            if (owner != null && feature != null && owner.HasFact(feature))
                owner.RemoveFact(feature);
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
