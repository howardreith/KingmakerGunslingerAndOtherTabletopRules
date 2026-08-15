using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.Utility;

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
            if (!Scopes.TryResolveAnchor(ability)) return false;
            anchor = AbilityTargetAnchor.Unit;
            return true;
        }

        internal static bool TryOverrideTarget(AbilityData ability,
            TargetWrapper target, out bool allowed)
        {
            UnitEntityData unit = target == null ? null : target.Unit;
            return Scopes.TryResolveTarget(ability,
                ability == null ? null : ability.Caster, unit, out allowed);
        }

        internal static bool TryOverrideApproachDistance(AbilityData ability,
            UnitEntityData target, float nativeDistance, out float distance)
        {
            distance = nativeDistance;
            BrownFurShareDelivery delivery;
            if (!Scopes.TryGetDelivery(ability,
                ability == null ? null : ability.Caster, target, out delivery) ||
                delivery != BrownFurShareDelivery.ThirtyFeet) return false;
            distance = nativeDistance + ThirtyFeetMeters;
            return true;
        }
    }
}
