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
            if (state == null)
            {
                throw new ArgumentNullException("state");
            }

            if (state.Condition == FirearmCondition.Wrecked)
            {
                return new FirearmDischargeResult(
                    FirearmDischargeStatus.Wrecked,
                    state,
                    state,
                    0,
                    true);
            }

            if (state.IsEmpty)
            {
                return new FirearmDischargeResult(
                    FirearmDischargeStatus.Empty,
                    state,
                    state,
                    0,
                    true);
            }

            return new FirearmDischargeResult(
                FirearmDischargeStatus.Fired,
                state,
                FirearmStateMachine.Fire(state),
                1,
                false);
        }
    }
}
