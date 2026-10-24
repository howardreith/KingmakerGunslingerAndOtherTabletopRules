using System;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.Utility;

namespace KingmakerGunslinger.Summoning
{
    /// <summary>
    /// Lanterns have no ordinary weapon. Native AbilityData otherwise routes
    /// hostile unit targeting through UnitEntityData.CanAttack and refuses the
    /// light ray before its ranged-touch delivery can run. The blueprint uses
    /// the engine's permissive unit-target relation flag and this checker keeps
    /// the actual player and AI target surface enemy-only.
    /// </summary>
    [Serializable]
    public sealed class LanternRayEnemyTargetChecker : BlueprintComponent,
        IAbilityTargetChecker
    {
        public bool CanTarget(UnitEntityData caster, TargetWrapper target)
        {
            UnitEntityData unit = target == null ? null : target.Unit;
            return caster != null && unit != null && unit.IsInGame &&
                !unit.Destroyed && caster.IsEnemy(unit);
        }
    }
}
