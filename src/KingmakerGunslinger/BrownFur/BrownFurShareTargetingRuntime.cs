using System;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.Utility;
using KingmakerGunslinger.Blueprints;
using UnityEngine;

namespace KingmakerGunslinger.BrownFur
{
    internal static class BrownFurShareTargetingRuntime
    {
        private const float ThirtyFeetMeters = 9.144f;
        private static readonly BrownFurShareTargetingScopeTracker<AbilityData,
            UnitDescriptor, UnitEntityData> Scopes =
                new BrownFurShareTargetingScopeTracker<AbilityData,
                    UnitDescriptor, UnitEntityData>();

        internal static int ActiveScopeCount { get { return Scopes.ActiveScopeCount; } }

        internal static bool Begin(string transactionIdentity, AbilityData ability,
            UnitEntityData target, BrownFurShareDelivery delivery)
        {
            return ability != null && ability.Caster != null && target != null &&
                Scopes.Begin(transactionIdentity, ability, ability.Caster, target,
                    delivery);
        }

        internal static bool Release(string transactionIdentity)
        { return Scopes.Release(transactionIdentity); }

        internal static void Clear()
        { Scopes.Clear(); }

        internal static bool TryOverrideAnchor(AbilityData ability,
            out AbilityTargetAnchor anchor)
        {
            anchor = AbilityTargetAnchor.Owner;
            BrownFurShareDelivery delivery;
            string failure;
            if (!Scopes.TryResolveAnchor(ability) &&
                !TryResolvePendingShareTargeting(ability, out delivery,
                    out failure)) return false;
            anchor = AbilityTargetAnchor.Unit;
            return true;
        }

        internal static bool TryOverrideTarget(AbilityData ability,
            TargetWrapper target, out bool allowed)
        {
            UnitEntityData unit = target == null ? null : target.Unit;
            if (Scopes.TryResolveTarget(ability,
                    ability == null ? null : ability.Caster, unit, out allowed))
                return true;
            BrownFurShareDelivery delivery;
            string failure;
            if (!TryResolvePendingShareTargeting(ability, out delivery,
                    out failure)) return false;
            UnitDescriptor owner = ability.Caster;
            var request = new BrownFurShareTargetRequest {
                IsValid = target != null,
                IsCreature = unit != null,
                IsAlive = unit != null && unit.Descriptor != null &&
                    !unit.Descriptor.State.IsDead,
                Relationship = BrownFurShareRelationshipRuntime.Classify(
                    owner, unit),
                HasThirtyFootCapstone =
                    delivery == BrownFurShareDelivery.ThirtyFeet,
                DistanceFeet = DistanceFeet(owner == null ? null : owner.Unit,
                    unit)
            };
            allowed = BrownFurShareTargetPolicy.Decide(request,
                request.HasThirtyFootCapstone).Eligible;
            return true;
        }

        internal static bool TryOverrideApproachDistance(AbilityData ability,
            UnitEntityData target, float nativeDistance, out float distance)
        {
            distance = nativeDistance;
            BrownFurShareDelivery delivery;
            string failure;
            if (!Scopes.TryGetDelivery(ability,
                    ability == null ? null : ability.Caster, target,
                    out delivery) &&
                !TryResolvePendingShareTargeting(ability, out delivery,
                    out failure)) return false;
            if (delivery != BrownFurShareDelivery.ThirtyFeet) return false;
            // Target legality uses an exact center-to-center 30-foot check.
            // Returning that same fixed distance avoids Close-range scaling and
            // avoids the former native-distance-plus-30-foot overreach.
            distance = ThirtyFeetMeters;
            return true;
        }

        internal static bool TryResolvePendingShareTargeting(
            AbilityData ability, out BrownFurShareDelivery delivery,
            out string failure)
        {
            delivery = BrownFurShareDelivery.None;
            failure = string.Empty;
            if (ability == null || ability.Caster == null ||
                ability.Blueprint == null)
                return Fail("share-pending-ability-missing", out failure);
            CotwArcanistResolution resolution =
                BrownFurOptionalExtensionCoordinator.Current;
            BrownFurBlueprintSet blueprints =
                BrownFurOptionalExtensionCoordinator.Blueprints;
            if (resolution == null || !resolution.Decision.IsCompatible ||
                resolution.Contract == null || blueprints == null)
                return Fail("share-pending-contract-unavailable", out failure);
            BrownFurPlayerIntentDecision intent =
                BrownFurPlayerIntentRuntime.Observe(ability.Caster, blueprints);
            if (!intent.Valid || !intent.HasShareTransmutation ||
                !intent.ShareTransmutationRequested)
                return Fail(intent.Valid ? "share-pending-not-armed" :
                    intent.Failure, out failure);
            if (ability.Blueprint.Type != AbilityType.Spell ||
                ability.Spellbook == null || ability.SourceItem != null)
                return Fail("share-pending-not-genuine-spell", out failure);
            if (ability.Blueprint.School != SpellSchool.Transmutation)
                return Fail("share-pending-not-transmutation", out failure);
            if (ability.Blueprint.Range != AbilityRange.Personal)
                return Fail("share-pending-not-personal", out failure);
            BrownFurSpellInventoryRecord record =
                BrownFurCastIntentRuntime.FindRecord(resolution.Contract,
                    ability);
            if (record == null || string.IsNullOrWhiteSpace(
                    record.ShareTransmutationCompatibility) ||
                !record.ShareTransmutationCompatibility.StartsWith(
                    "Supported by ", StringComparison.Ordinal))
                return Fail("share-pending-adapter-unavailable", out failure);
            delivery = intent.HasTransmutationSupremacy ?
                BrownFurShareDelivery.ThirtyFeet :
                BrownFurShareDelivery.Touch;
            return true;
        }

        private static bool Fail(string value, out string failure)
        {
            failure = value ?? "share-pending-unknown";
            return false;
        }

        private static double DistanceFeet(UnitEntityData caster,
            UnitEntityData target)
        {
            if (caster == null || target == null) return 0d;
            return Vector3.Distance(caster.Position, target.Position) / 0.3048d;
        }
    }
}
