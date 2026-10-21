using System;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Controllers.Brain;
using Kingmaker.Controllers.Brain.Blueprints.Considerations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic;
using Kingmaker.Utility;

namespace KingmakerGunslinger.Summoning
{
    /// <summary>Only target selection. The native electricity spell and its
    /// real saves, resistance, immunity and area effects are not rewritten.</summary>
    public sealed class Sprint29LightningAiSafety : Consideration
    {
        public BlueprintUnit OwningBlueprint;
        public BlueprintFeature KnownElectricityImmunity;

        public override float Score(DecisionContext context)
        {
            UnitEntityData actor = context == null ? null : context.Unit;
            UnitEntityData target = context == null || context.Target == null ?
                null : context.Target.Unit;
            // Call Lightning is a self activation. A hostile-target-only
            // consideration would always veto it before the granted bolt
            // can become available. Require a visible susceptible enemy,
            // while leaving the native activation and bolt actions intact.
            if (actor != null && (target == null ||
                    ReferenceEquals(target, actor)) &&
                ReferenceEquals(actor.Blueprint, OwningBlueprint) &&
                Game.Instance?.State?.Units != null)
                return Game.Instance.State.Units.All.Any(unit =>
                    unit != null && !unit.Destroyed &&
                    unit.Descriptor != null &&
                    !unit.Descriptor.State.IsDead &&
                    actor.IsEnemy(unit) && actor.HasLOS(unit) &&
                    actor.Memory.ContainsVisible(unit) &&
                    actor.DistanceTo(unit) <= new Feet(120).Meters &&
                    (KnownElectricityImmunity == null ||
                        !unit.Descriptor.HasFact(KnownElectricityImmunity)))
                    ? 1f : 0f;
            if (actor == null || target == null ||
                !ReferenceEquals(actor.Blueprint, OwningBlueprint) ||
                target.Descriptor == null || target.Descriptor.State == null ||
                target.Descriptor.State.IsDead || !actor.IsEnemy(target) ||
                !context.Target.InLos(context) ||
                (KnownElectricityImmunity != null &&
                    target.Descriptor.HasFact(KnownElectricityImmunity)))
                return 0f;
            return 1f;
        }
    }
}
