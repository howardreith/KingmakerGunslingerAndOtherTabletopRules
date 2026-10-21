using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Controllers.Brain;
using Kingmaker.Controllers.Brain.Blueprints.Considerations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic;

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
