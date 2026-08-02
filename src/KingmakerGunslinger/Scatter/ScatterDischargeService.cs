using System;
using KingmakerGunslinger.Firearms;
using KingmakerGunslinger.Firing;

namespace KingmakerGunslinger.Scatter
{
    /// <summary>
    /// Pure transaction boundary: all target/delivery prerequisites must be
    /// established before the firearm performs its single canonical discharge.
    /// </summary>
    internal sealed class ScatterDischargeService
    {
        private readonly FirearmDischargeService _discharge = new FirearmDischargeService();

        internal ScatterDischargeDecision Evaluate(
            FirearmDefinition definition,
            FirearmState state,
            ScatterTargetPlan plan,
            bool deliveryPrerequisitesSatisfied)
        {
            return Evaluate(definition, state, state == null
                ? FirearmCondition.Wrecked : state.Condition, plan,
                deliveryPrerequisitesSatisfied);
        }

        internal ScatterDischargeDecision Evaluate(
            FirearmDefinition definition,
            FirearmState state,
            FirearmCondition effectiveCondition,
            ScatterTargetPlan plan,
            bool deliveryPrerequisitesSatisfied)
        {
            if (definition == null) throw new ArgumentNullException("definition");
            if (!definition.IsScatter)
                throw new ArgumentException("Only a scatter firearm can use scatter discharge.", "definition");
            if (state == null) throw new ArgumentNullException("state");
            if (plan == null) throw new ArgumentNullException("plan");

            if (!deliveryPrerequisitesSatisfied)
            {
                return new ScatterDischargeDecision(
                    ScatterDischargeStatus.RejectedBeforeDelivery,
                    state, state, plan.TargetCount, 0, false);
            }

            FirearmDischargeResult result = _discharge.Evaluate(state,
                effectiveCondition);
            ScatterDischargeStatus status = result.Status == FirearmDischargeStatus.Fired
                ? ScatterDischargeStatus.Fired
                : result.Status == FirearmDischargeStatus.Empty
                    ? ScatterDischargeStatus.Empty
                    : ScatterDischargeStatus.Wrecked;
            return new ScatterDischargeDecision(
                status,
                result.Before,
                result.After,
                plan.TargetCount,
                result.RoundsConsumed,
                result.ShouldForceMiss);
        }
    }
}
