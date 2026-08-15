using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.Utility;
using KingmakerGunslinger.Blueprints;
using UnityEngine;

namespace KingmakerGunslinger.BrownFur
{
    /// <summary>
    /// Converts one native command and the owner's one-shot facts into an
    /// immutable, validated cast transaction. It has no fallback for spells
    /// absent from the structurally qualified inventory.
    /// </summary>
    internal static class BrownFurCastIntentRuntime
    {
        private const double MetersPerFoot = 0.3048d;
        private static readonly object Gate = new object();
        private static CotwArcanistContract _inventoryContract;
        private static Dictionary<string, BrownFurSpellInventoryRecord>
            _inventory;
        private static long _nextIdentity;
        private static string _lastOutcome = string.Empty;

        internal static string LastOutcome
        { get { lock (Gate) return _lastOutcome; } }

        internal static void Arm(UnitUseAbility command, AbilityData ability,
            TargetWrapper target)
        {
            if (command == null || ability == null || ability.Caster == null)
                return;
            CotwArcanistResolution resolution =
                BrownFurOptionalExtensionCoordinator.Current;
            BrownFurBlueprintSet blueprints =
                BrownFurOptionalExtensionCoordinator.Blueprints;
            if (resolution == null || !resolution.Decision.IsCompatible ||
                resolution.Contract == null || blueprints == null)
                return;

            UnitDescriptor owner = ability.Caster;
            BrownFurPlayerIntentDecision playerIntent = null;
            try
            {
                playerIntent = BrownFurPlayerIntentRuntime.Observe(owner,
                    blueprints);
                if (!playerIntent.Valid)
                {
                    Reject(command, playerIntent.Failure);
                    return;
                }
                if (!playerIntent.CasterOwnsBrownFur) return;

                BlueprintAbility selected = ability.Blueprint;
                BrownFurSpellInventoryRecord record = FindRecord(
                    resolution.Contract, ability);
                bool requested = playerIntent.PowerfulChangeRequested ||
                    playerIntent.ShareTransmutationRequested;
                bool transmutation = selected != null &&
                    selected.School == SpellSchool.Transmutation;
                if (record == null && (requested ||
                    (playerIntent.HasTransmutationSupremacy && transmutation)))
                {
                    Reject(command, "spell-inventory-unqualified:" +
                        GuidOf(selected));
                    return;
                }

                BrownFurBonusAdapterPlan bonusPlan = record == null ? null :
                    BrownFurBonusAdapterPlanPolicy.Create(
                        record.AbilityScoreBonuses,
                        record.AppliedBuffs);
                BrownFurCastRequest request = BuildRequest(
                    resolution.Contract, playerIntent, ability, target,
                    record, bonusPlan);
                BrownFurCastDecision decision =
                    BrownFurCastPolicy.Decide(request);
                if (!decision.Eligible)
                {
                    if (requested) Reject(command, decision.Failure);
                    return;
                }
                if (!decision.PowerfulChange &&
                    !decision.ShareTransmutation &&
                    !decision.TransmutationSupremacy)
                    return;

                BrownFurCastIntent intent = BuildIntent(command, ability,
                    target, record, playerIntent, decision, bonusPlan);
                var transaction = new BrownFurCastTransaction(intent);
                if (!transaction.Validate(decision) ||
                    !BrownFurCastExecutionRuntime.Begin(resolution.Contract,
                        command, ability, target, transaction, bonusPlan))
                {
                    Reject(command, "cast-reservation-rejected");
                    return;
                }
                Outcome("armed:" + intent.TransactionIdentity + ";cost=" +
                    decision.ReservoirCost);
            }
            catch (Exception exception)
            {
                if (playerIntent != null &&
                    playerIntent.CasterOwnsBrownFur)
                    Reject(command, "intent-exception:" +
                        exception.GetType().FullName);
                BrownFurCastExecutionRuntime.RecordPatchFailure(
                    "intent-arm", exception);
            }
            finally
            {
                if (playerIntent != null &&
                    (playerIntent.PowerfulChangeRequested ||
                     playerIntent.ShareTransmutationRequested ||
                     !playerIntent.Valid))
                    BrownFurPlayerIntentRuntime.Clear(owner, blueprints);
            }
        }

        internal static void Clear()
        {
            lock (Gate)
            {
                _inventoryContract = null;
                _inventory = null;
                _lastOutcome = string.Empty;
            }
        }

        private static BrownFurCastRequest BuildRequest(
            CotwArcanistContract contract,
            BrownFurPlayerIntentDecision intent, AbilityData ability,
            TargetWrapper target, BrownFurSpellInventoryRecord record,
            BrownFurBonusAdapterPlan bonusPlan)
        {
            UnitEntityData targetUnit = target == null ? null : target.Unit;
            UnitDescriptor owner = ability.Caster;
            bool spellbookSource = ability.Spellbook != null;
            bool qualified = record != null && (string.Equals(
                    record.QualificationStatus,
                    BrownFurInventoryQualifications.Generic,
                    StringComparison.Ordinal) || string.Equals(
                    record.QualificationStatus,
                    BrownFurInventoryQualifications.Named,
                    StringComparison.Ordinal));
            bool noOpDuration = record != null &&
                record.RequiredAdapter.IndexOf("supremacy=proven-no-op",
                    StringComparison.Ordinal) >= 0;
            BlueprintSpellbook sourceBook = ability.Spellbook == null ? null :
                ability.Spellbook.Blueprint;
            int reservoir = owner.Resources.ContainsResource(contract.Reservoir) ?
                owner.Resources.GetResourceAmount(contract.Reservoir) : 0;
            return new BrownFurCastRequest {
                CasterOwnsBrownFur = intent.CasterOwnsBrownFur,
                IsGenuineSpell = ability.Blueprint != null &&
                    ability.Blueprint.Type == AbilityType.Spell &&
                    spellbookSource && ability.SourceItem == null,
                IsTransmutation = ability.Blueprint != null &&
                    ability.Blueprint.School == SpellSchool.Transmutation,
                SourceKind = spellbookSource && ability.SourceItem == null ?
                    BrownFurCastSourceKind.Spellbook :
                    (ability.SourceItem == null ?
                        BrownFurCastSourceKind.SpellLike :
                        BrownFurCastSourceKind.Item),
                UsesArcanistSpellSlot = sourceBook != null &&
                    ReferenceEquals(sourceBook, contract.CastingSpellbook),
                HasPowerfulChange = intent.HasPowerfulChange,
                HasPowerfulChangeCapstone =
                    intent.HasTransmutationSupremacy,
                HasShareTransmutation = intent.HasShareTransmutation,
                HasShareThirtyFootCapstone =
                    intent.HasTransmutationSupremacy,
                HasTransmutationSupremacy =
                    intent.HasTransmutationSupremacy,
                PowerfulChangeRequested = intent.PowerfulChangeRequested,
                SelectedAbilityScore = intent.SelectedAbilityScore,
                PositiveAbilityBonuses = bonusPlan == null ?
                    new HashSet<BrownFurAbilityScore>() :
                    new HashSet<BrownFurAbilityScore>(bonusPlan.AbilityScores),
                BonusAdapterAvailable = bonusPlan != null &&
                    bonusPlan.Status ==
                        BrownFurBonusAdapterPlanStatus.Supported,
                ShareTransmutationRequested =
                    intent.ShareTransmutationRequested,
                OriginalRange = RangeOf(ability.Blueprint),
                ShareTarget = new BrownFurShareTargetRequest {
                    IsValid = target != null,
                    IsCreature = targetUnit != null,
                    IsAlive = targetUnit != null &&
                        targetUnit.Descriptor != null &&
                        !targetUnit.Descriptor.State.IsDead,
                    Relationship =
                        BrownFurShareRelationshipRuntime.Classify(owner,
                            targetUnit),
                    HasThirtyFootCapstone =
                        intent.HasTransmutationSupremacy,
                    DistanceFeet = DistanceFeet(owner.Unit, targetUnit)
                },
                TargetAdapterAvailable = qualified && record != null &&
                    record.ShareTransmutationCompatibility.StartsWith(
                        "Supported by ", StringComparison.Ordinal),
                DurationKind = noOpDuration ?
                    BrownFurDurationKind.Instantaneous :
                    BrownFurDurationKind.Timed,
                AlreadyExtended = ability.HasMetamagic(Metamagic.Extend),
                DurationAdapterAvailable = qualified && record != null &&
                    record.TransmutationSupremacyCompatibility.StartsWith(
                        "Supported by ", StringComparison.Ordinal),
                ReservoirPoints = reservoir
            };
        }

        private static BrownFurCastIntent BuildIntent(UnitUseAbility command,
            AbilityData ability, TargetWrapper target,
            BrownFurSpellInventoryRecord record,
            BrownFurPlayerIntentDecision player,
            BrownFurCastDecision decision, BrownFurBonusAdapterPlan bonusPlan)
        {
            BlueprintAbility selected = ability.Blueprint;
            string canonical = record == null ? GuidOf(selected) :
                (string.IsNullOrWhiteSpace(record.ParentGuid) ?
                    record.CanonicalSpellGuid : record.ParentGuid);
            BlueprintSpellbook sourceBook = ability.Spellbook == null ? null :
                ability.Spellbook.Blueprint;
            UnitEntityData targetUnit = target == null ? null : target.Unit;
            long sequence = Interlocked.Increment(ref _nextIdentity);
            string identity = "brown-fur-" + sequence.ToString("x16") + "-" +
                RuntimeHelpers.GetHashCode(command).ToString("x8");
            return new BrownFurCastIntent(identity,
                UnitIdentity(ability.Caster == null ? null :
                    ability.Caster.Unit), canonical, GuidOf(selected),
                GuidOf(sourceBook), UnitIdentity(targetUnit),
                player.PowerfulChangeRequested,
                player.SelectedAbilityScore,
                player.ShareTransmutationRequested,
                decision.TransmutationSupremacy,
                decision.ReservoirCost,
                decision.ShareTransmutation ? decision.ShareDelivery.ToString() :
                    "none",
                decision.PowerfulChange && bonusPlan != null ?
                    string.Join("+", bonusPlan.CarrierFamilies) : "none",
                decision.TransmutationSupremacy && record != null ?
                    record.RequiredAdapter : "none");
        }

        private static BrownFurSpellInventoryRecord FindRecord(
            CotwArcanistContract contract, AbilityData ability)
        {
            Dictionary<string, BrownFurSpellInventoryRecord> inventory;
            lock (Gate)
            {
                if (!ReferenceEquals(_inventoryContract, contract) ||
                    _inventory == null)
                {
                    BrownFurSpellInventoryEvidence observed =
                        BrownFurTransmutationInventory.Observe(contract);
                    _inventory = observed.Records.ToDictionary(
                        value => value.CanonicalSpellGuid,
                        value => value, StringComparer.Ordinal);
                    _inventoryContract = contract;
                }
                inventory = _inventory;
            }
            for (AbilityData current = ability; current != null;
                current = current.ConvertedFrom)
            {
                BrownFurSpellInventoryRecord record;
                if (current.Blueprint != null && inventory.TryGetValue(
                        current.Blueprint.AssetGuid, out record))
                    return record;
            }
            return null;
        }

        private static BrownFurOriginalRange RangeOf(
            BlueprintAbility ability)
        {
            if (ability == null) return BrownFurOriginalRange.Unknown;
            if (ability.Range == AbilityRange.Personal)
                return BrownFurOriginalRange.Personal;
            if (ability.Range == AbilityRange.Touch)
                return BrownFurOriginalRange.Touch;
            return BrownFurOriginalRange.Other;
        }

        private static double DistanceFeet(UnitEntityData caster,
            UnitEntityData target)
        {
            if (caster == null || target == null) return 0d;
            return Vector3.Distance(caster.Position, target.Position) /
                MetersPerFoot;
        }

        private static string GuidOf(Kingmaker.Blueprints.BlueprintScriptableObject value)
        { return value == null ? string.Empty : value.AssetGuid; }

        private static string UnitIdentity(UnitEntityData unit)
        {
            if (unit == null) return string.Empty;
            return (unit.UniqueId ?? string.Empty) + ":" +
                (unit.CharacterName ?? string.Empty);
        }

        private static void Reject(UnitUseAbility command, string failure)
        {
            BrownFurCastExecutionRuntime.RejectCommand(command, failure);
            Outcome("rejected:" + (failure ?? "unknown"));
        }

        private static void Outcome(string value)
        { lock (Gate) _lastOutcome = value ?? string.Empty; }
    }
}
