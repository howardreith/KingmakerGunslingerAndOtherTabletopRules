using System;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Controllers.Brain;
using Kingmaker.Controllers.Brain.Blueprints.Considerations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.Utility;

namespace KingmakerGunslinger.Summoning
{
    /// <summary>
    /// A choice filter for Cloud's real all-creature fog. It changes no area
    /// effect or immunity: the native spell still obscures any creature that
    /// enters it. An uncertain target, nearby ally, or existing owner area
    /// makes the AI choose its ordinary attack instead.
    /// </summary>
    public sealed class Sprint29FogAiSafety : Consideration
    {
        public BlueprintUnit OwningBlueprint;
        public BlueprintAbilityAreaEffect FogArea;
        public bool CenterOnCaster;

        public override float Score(DecisionContext context)
        {
            UnitEntityData actor = context == null ? null : context.Unit;
            UnitEntityData target = context == null || context.Target == null ?
                null : context.Target.Unit;
            if (actor == null || actor.Blueprint == null ||
                !ReferenceEquals(actor.Blueprint, OwningBlueprint) ||
                Game.Instance == null || Game.Instance.State == null ||
                Game.Instance.State.Units == null ||
                Game.Instance.State.AreaEffects == null || FogArea == null)
                return 0f;
            if (CenterOnCaster)
            {
                // Self mist is useful against an approaching distant foe;
                // hiding the immediate melee target would waste the turn.
                bool distantEnemy = Game.Instance.State.Units.All.Any(unit =>
                    unit != null && !unit.Destroyed && actor.IsEnemy(unit) &&
                    actor.DistanceTo(unit) > new Feet(20).Meters &&
                    actor.DistanceTo(unit) <= new Feet(120).Meters);
                if (!distantEnemy) return 0f;
            }
            else if (target == null || !actor.IsEnemy(target))
                return 0f;
            var center = CenterOnCaster ? actor.Position : target.Position;
            float radius = new Feet(20).Meters;
            if (Game.Instance.State.Units.All.Any(unit =>
                unit != null && !unit.Destroyed &&
                !ReferenceEquals(unit, actor) && actor.IsAlly(unit) &&
                (unit.Position - center).magnitude <= radius + unit.Corpulence))
                return 0f;
            // Area ownership, not a global fog switch: a sibling or a
            // different caster may maintain an independent area.
            if (Game.Instance.State.AreaEffects.All.Any(area =>
                area != null && ReferenceEquals(area.Blueprint, FogArea) &&
                area.Context != null &&
                ReferenceEquals(area.Context.MaybeCaster, actor)))
                return 0f;
            return 1f;
        }
    }
}
