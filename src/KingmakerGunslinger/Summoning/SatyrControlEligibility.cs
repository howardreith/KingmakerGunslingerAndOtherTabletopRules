using System;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Controllers.Brain;
using Kingmaker.Controllers.Brain.Blueprints.Considerations;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using UnityEngine;

namespace KingmakerGunslinger.Summoning
{
    /// <summary>Creature-owned choice policy. Native saves, immunity, spell
    /// effects and action/resource costs remain responsible for resolution.</summary>
    public sealed class SatyrControlEligibility : Consideration
    {
        public BlueprintUnit OwningBlueprint;
        public BlueprintAbility Ability;
        public int MaximumHitDice;
        public bool FearCone;

        public override float Score(DecisionContext context)
        {
            var actor = context?.Unit;
            var target = context?.Target?.Unit;
            if (actor == null || target == null || Ability == null ||
                !ReferenceEquals(actor.Blueprint, OwningBlueprint) ||
                !actor.IsEnemy(target) || target.Destroyed || !target.IsInState ||
                target.Descriptor.State.IsDead || !actor.HasLOS(target) ||
                !actor.Memory.ContainsVisible(target) ||
                MaximumHitDice > 0 && target.Descriptor.Progression.CharacterLevel > MaximumHitDice ||
                target.Descriptor.State.HasCondition(UnitCondition.Frightened) ||
                target.Descriptor.State.HasCondition(UnitCondition.Sleeping)) return 0f;
            var granted = actor.Descriptor.Abilities.GetAbility(Ability);
            if (granted == null) return 0f;
            var data = new AbilityData(granted);
            if (!data.IsAvailable || !data.CanTarget(new TargetWrapper(target))) return 0f;
            var effect = new MechanicsContext(actor, actor.Descriptor, Ability,
                null, new TargetWrapper(target));
            var immunity = target.Get<UnitPartSpellResistance>();
            if (immunity != null && immunity.IsImmune(effect)) return 0f;
            if (FearCone)
            {
                if (target.Descriptor.State.HasConditionImmunity(UnitCondition.Frightened) ||
                    Game.Instance?.State?.Units == null) return 0f;
                // Conservative footprint includes each ally's collision radius.
                // Refusal never removes an ally from the native spell's targets.
                var direction = target.Position - actor.Position;
                direction.y = 0;
                if (direction.sqrMagnitude < 0.001f) return 0f;
                direction.Normalize();
                float reach = new Feet(30).Meters;
                if (Game.Instance.State.Units.All.Any(ally => {
                    if (ally == null || ally.Destroyed || !ally.IsInState ||
                        ReferenceEquals(ally, actor) || actor.IsEnemy(ally) ||
                        ally.Descriptor.State.IsDead) return false;
                    var delta = ally.Position - actor.Position; delta.y = 0;
                    float distance = delta.magnitude;
                    float radius = Math.Max(0f, ally.Corpulence);
                    return distance <= reach + radius && (distance <= radius ||
                        Vector3.Angle(direction, delta) <= 45f +
                        Mathf.Asin(Mathf.Clamp01(radius / distance)) * Mathf.Rad2Deg);
                })) return 0f;
            }
            return 1f;
        }
    }
}
