using System;
using KingmakerGunslinger.Firearms;

namespace KingmakerGunslinger.Firing
{
    /// <summary>
    /// Pure deterministic decision for one firearm attack roll. A loaded firearm
    /// consumes exactly one round; an empty or wrecked firearm must be forced to miss.
    /// </summary>
    internal sealed class FirearmDischargeService
    {
        internal FirearmDischargeResult Evaluate(FirearmState state)
        {
            return Evaluate(state, state == null ? FirearmCondition.Unknown :
                state.Condition);
        }

        internal FirearmDischargeResult Evaluate(FirearmState state,
            FirearmCondition effectiveCondition)
        {
            if (state == null)
            {
                throw new ArgumentNullException("state");
            }

            if (effectiveCondition == FirearmCondition.Unknown ||
                !Enum.IsDefined(typeof(FirearmCondition), effectiveCondition))
                throw new ArgumentOutOfRangeException("effectiveCondition");

            if (effectiveCondition == FirearmCondition.Wrecked)
            {
                return new FirearmDischargeResult(
                    FirearmDischargeStatus.Wrecked,
                    state,
                    state,
                    0,
                    true,
                    effectiveCondition);
            }

            if (state.IsEmpty)
            {
                return new FirearmDischargeResult(
                    FirearmDischargeStatus.Empty,
                    state,
                    state,
                    0,
                    true,
                    effectiveCondition);
            }

            return new FirearmDischargeResult(
                FirearmDischargeStatus.Fired,
                state,
                FirearmStateMachine.Fire(state),
                1,
                false,
                effectiveCondition);
        }
    }
}
