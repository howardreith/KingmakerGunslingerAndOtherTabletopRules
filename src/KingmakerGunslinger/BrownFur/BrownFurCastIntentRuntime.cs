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
        private sealed class PreparedCast
        {
            internal bool Accepted;
            internal bool RejectNativeCommand;
            internal string Failure;
            internal CotwArcanistContract Contract;
            internal BrownFurSpellInventoryRecord Record;
            internal BrownFurPlayerIntentDecision PlayerIntent;
            internal BrownFurCastDecision Decision;
            internal BrownFurBonusAdapterPlan BonusPlan;
        }

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
            try
            {
                PreparedCast prepared = Prepare(ability, target);
                if (!prepared.Accepted)
                {
                    if (prepared.RejectNativeCommand)
                        Reject(command, prepared.Failure);
                    return;
                }
                BrownFurCastIntent intent = BuildIntent(command, ability,
                    target, prepared.Record, prepared.PlayerIntent,
                    prepared.Decision, prepared.BonusPlan);
                var transaction = new BrownFurCastTransaction(intent);
                if (!transaction.Validate(prepared.Decision) ||
                    !BrownFurCastExecutionRuntime.Begin(prepared.Contract,
                        command, ability, target, transaction,
                        prepared.BonusPlan))
                {
                    Reject(command, "cast-reservation-rejected");
                    return;
                }
                Outcome("armed:" + intent.TransactionIdentity + ";cost=" +
                    prepared.Decision.ReservoirCost);
            }
            catch (Exception exception)
            {
                try
                {
                    BrownFurBlueprintSet blueprints =
                        BrownFurOptionalExtensionCoordinator.Blueprints;
                    BrownFurPlayerIntentDecision player =
                        blueprints == null ? null :
                            BrownFurPlayerIntentRuntime.Observe(
                                ability.Caster, blueprints);
                    if (player != null && player.CasterOwnsBrownFur)
                        Reject(command, "intent-exception:" +
                            exception.GetType().FullName);
                }
                catch (Exception) { }
                BrownFurCastExecutionRuntime.RecordPatchFailure(
                    "intent-arm", exception);
            }
        }

        internal static BrownFurDirectCastStatus ValidateDirect(
            AbilityData ability, TargetWrapper target)
        {
            try
            {
                PreparedCast prepared = Prepare(ability, target);
                return prepared.Accepted
                    ? BrownFurDirectCastStatus.PreflightAccepted(
                        prepared.Decision.ReservoirCost)
                    : BrownFurDirectCastStatus.Rejected(prepared.Failure);
            }
            catch (Exception exception)
            {
                BrownFurCastExecutionRuntime.RecordPatchFailure(
                    "direct-preflight", exception);
                return BrownFurDirectCastStatus.Rejected(
                    "direct-preflight-exception:" +
                    exception.GetType().FullName);
            }
        }

        internal static BrownFurDirectCastHandle BeginDirect(
            AbilityData ability, TargetWrapper target)
        {
            PreparedCast prepared;
            try { prepared = Prepare(ability, target); }
            catch (Exception exception)
            {
                BrownFurCastExecutionRuntime.RecordPatchFailure(
                    "direct-intent", exception);
                return BrownFurDirectCastHandle.Rejected(
                    "direct-intent-exception:" +
                    exception.GetType().FullName);
            }
            if (!prepared.Accepted)
                return BrownFurDirectCastHandle.Rejected(prepared.Failure);
            BrownFurCastIntent intent = BuildIntent(ability, ability, target,
                prepared.Record, prepared.PlayerIntent, prepared.Decision,
                prepared.BonusPlan);
            var transaction = new BrownFurCastTransaction(intent);
            if (!transaction.Validate(prepared.Decision))
                return BrownFurDirectCastHandle.Rejected(
                    "direct-transaction-validation-rejected");
            BrownFurDirectCastHandle handle =
                BrownFurDirectCastHandle.CreateAccepted(ability, target,
                    intent.TransactionIdentity,
                    prepared.Decision.ReservoirCost);
            if (!BrownFurCastExecutionRuntime.BeginDirect(prepared.Contract,
                    ability, target, transaction, prepared.BonusPlan, handle))
            {
                handle.MarkBeginRejected(
                    "direct-cast-reservation-rejected");
                return handle;
            }
            Outcome("direct-armed:" + intent.TransactionIdentity + ";cost=" +
                prepared.Decision.ReservoirCost);
            return handle;
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

        private static BrownFurCastIntent BuildIntent(object identityAnchor,
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
                RuntimeHelpers.GetHashCode(identityAnchor).ToString("x8");
            return new BrownFurCastIntent(identity,
                UnitIdentity(ability.Caster == null ? null :
                    ability.Caster.Unit), canonical, GuidOf(selected),
                GuidOf(sourceBook), UnitIdentity(targetUnit),
                decision.PowerfulChange,
                decision.PowerfulChange ? decision.SelectedAbilityScore :
                    BrownFurAbilityScore.None,
                decision.ShareTransmutation,
                decision.TransmutationSupremacy,
                decision.ReservoirCost,
                decision.ShareTransmutation ? decision.ShareDelivery.ToString() :
                    "none",
                decision.PowerfulChange && bonusPlan != null ?
                    string.Join("+", bonusPlan.CarrierFamilies) : "none",
                decision.TransmutationSupremacy && record != null ?
                    record.RequiredAdapter : "none");
        }

        private static PreparedCast Prepare(AbilityData ability,
            TargetWrapper target)
        {
            var prepared = new PreparedCast {
                Failure = "brown-fur-direct-cast-unavailable"
            };
            if (ability == null || ability.Caster == null)
            {
                prepared.Failure = "direct-ability-or-caster-missing";
                prepared.RejectNativeCommand = true;
                return prepared;
            }
            CotwArcanistResolution resolution =
                BrownFurOptionalExtensionCoordinator.Current;
            BrownFurBlueprintSet blueprints =
                BrownFurOptionalExtensionCoordinator.Blueprints;
            if (resolution == null || !resolution.Decision.IsCompatible ||
                resolution.Contract == null || blueprints == null)
            {
                prepared.Failure = "brown-fur-contract-unavailable";
                return prepared;
            }
            UnitDescriptor owner = ability.Caster;
            BrownFurPlayerIntentDecision playerIntent =
                BrownFurPlayerIntentRuntime.Observe(owner, blueprints);
            if (!playerIntent.Valid)
            {
                prepared.Failure = playerIntent.Failure;
                prepared.RejectNativeCommand = true;
                return prepared;
            }
            if (!playerIntent.CasterOwnsBrownFur)
            {
                prepared.Failure = "caster-does-not-own-brown-fur";
                return prepared;
            }
            BlueprintAbility selected = ability.Blueprint;
            BrownFurSpellInventoryRecord record = FindRecord(
                resolution.Contract, ability);
            bool requested = playerIntent.PowerfulChangeRequested ||
                playerIntent.ShareTransmutationRequested;
            bool transmutation = selected != null &&
                selected.School == SpellSchool.Transmutation;
            if (record == null && transmutation && (requested ||
                playerIntent.HasTransmutationSupremacy))
            {
                prepared.Failure = "spell-inventory-unqualified:" +
                    GuidOf(selected);
                prepared.RejectNativeCommand = true;
                return prepared;
            }
            BrownFurBonusAdapterPlan bonusPlan = record == null ? null :
                BrownFurBonusAdapterPlanPolicy.Create(
                    record.AbilityScoreBonuses, record.AppliedBuffs);
            BrownFurCastDecision decision = BrownFurCastPolicy.Decide(
                BuildRequest(resolution.Contract, playerIntent, ability,
                    target, record, bonusPlan));
            if (!decision.Eligible)
            {
                prepared.Failure = decision.Failure;
                prepared.RejectNativeCommand = requested;
                return prepared;
            }
            if (!decision.PowerfulChange && !decision.ShareTransmutation &&
                !decision.TransmutationSupremacy)
            {
                prepared.Failure = "brown-fur-direct-intent-not-applicable";
                return prepared;
            }
            prepared.Accepted = true;
            prepared.Contract = resolution.Contract;
            prepared.Record = record;
            prepared.PlayerIntent = playerIntent;
            prepared.Decision = decision;
            prepared.BonusPlan = bonusPlan;
            prepared.Failure = string.Empty;
            return prepared;
        }

        internal static BrownFurSpellInventoryRecord FindRecord(
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
