using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Controllers.Combat;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items.Slots;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Newtonsoft.Json;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.Utility;

namespace KingmakerGunslinger.Summoning
{
    /// <summary>Project-only correction of the installed Cleave selector.
    /// The native ability still spends the action, applies its AC penalty,
    /// and resolves each attack through the native rulebook. This delivery
    /// requires adjacency to the previous hit as PF1 Cleave specifies.</summary>
    [Serializable]
    public sealed class UnitPartSummonGiantCleave : UnitPart
    {
        [JsonProperty] private long _lastAdditionalAttackRound = long.MinValue;

        internal bool TrySpend(long round)
        {
            if (_lastAdditionalAttackRound == round) return false;
            _lastAdditionalAttackRound = round;
            return true;
        }

        internal long LastAdditionalAttackRound
        { get { return _lastAdditionalAttackRound; } }
    }

    public sealed class SummonGiantCleaveDelivery : AbilityCustomCleave
    {
        private const float AdjacentMeters = 5f * 0.3048f;
        private const float GeometryTolerance = 0.02f;

        public override IEnumerator<AbilityDeliveryTarget> Deliver(
            AbilityExecutionContext context, TargetWrapper target)
        {
            UnitEntityData caster = context?.Caster;
            UnitEntityData first = target?.Unit;
            if (caster == null || first == null || first.Descriptor.State.IsDead ||
                !caster.IsEnemy(first))
                yield break;
            WeaponSlot hand = UnitEngagementExtension.GetThreatHand(caster);
            if (hand == null || hand.Weapon == null ||
                !UnitEngagementExtension.IsReach(caster, first, hand))
                yield break;
            bool greater = GreaterFeature != null &&
                caster.Descriptor.Progression.Features.HasFact(GreaterFeature);
            // The native selector searches remembered enemies, keeps the
            // chosen first target, and sorts by distance from it. This copy
            // retains that preference but selects each successor only when
            // adjacent to the previous hit and still within actual reach.
            var remaining = new List<UnitEntityData> { first };
            remaining.AddRange(caster.Memory.Enemies.Select(info => info.Unit)
                .Where(unit => unit != null && !ReferenceEquals(unit, first))
                .OrderBy(unit => unit.SqrDistanceTo(first)));
            var used = new HashSet<UnitEntityData>();
            UnitEntityData previous = null;
            while (remaining.Count > 0 && (greater || used.Count < 2))
            {
                if (!caster.IsInGame || caster.Destroyed ||
                    caster.Descriptor.State.IsDead || hand.Weapon == null)
                    yield break;
                UnitEntityData next = remaining.FirstOrDefault(unit =>
                    !used.Contains(unit) && unit.IsInGame &&
                    !unit.Descriptor.State.IsDead && caster.IsEnemy(unit) &&
                    UnitEngagementExtension.IsReach(caster, unit, hand) &&
                    (previous == null || Adjacent(previous, unit)));
                if (next == null) yield break;
                if (!greater && previous != null &&
                    !caster.Ensure<UnitPartSummonGiantCleave>().TrySpend(
                        ContextActionUngulateTrample.CurrentRound()))
                    yield break;
                remaining.Remove(next);
                used.Add(next);
                var attack = new RuleAttackWithWeapon(caster, next,
                    hand.Weapon, 0) { IsFirstAttack = used.Count == 1 };
                context.TriggerRule(attack);
                if (attack.AttackRoll == null || !attack.AttackRoll.IsHit)
                    yield break;
                yield return new AbilityDeliveryTarget(
                    new TargetWrapper(next));
                previous = next;
            }
        }

        private static bool Adjacent(UnitEntityData previous,
            UnitEntityData next)
        {
            return previous.DistanceTo(next) <= previous.Corpulence +
                next.Corpulence + AdjacentMeters + GeometryTolerance;
        }
    }
}
