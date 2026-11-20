using Kingmaker.Blueprints;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.Base;

namespace KingmakerGunslinger.Deeds
{
    /// <summary>
    /// The grit cost of Gunslinger's Dodge, read by the ability's native
    /// resource logic. It is one of the registered ability's four components,
    /// beside the resource logic, the "not already active" caster check and
    /// the run-action effect that applies the timed buff.
    /// </summary>
    internal sealed class DodgeGritCostCalculator : BlueprintComponent,
        IAbilityResourceCostCalculator
    {
        public int Calculate(AbilityData ability)
        {
            if (ability == null || ability.Caster == null) return 1;
            // True Grit reduces the cost to 0 but still requires grit above 0.
            return TrueGritRuntime.Evaluate(ability.Caster,
                TrueGritDeed.GunslingersDodge, 1, false)
                .NativeCheckCost;
        }
    }
}
