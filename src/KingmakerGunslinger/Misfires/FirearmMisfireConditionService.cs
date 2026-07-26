using System;
using KingmakerGunslinger.Firearms;

namespace KingmakerGunslinger.Misfires
{
    /// <summary>
    /// Pure deterministic Sprint 24 policy. It evaluates only an already-empty
    /// post-discharge state and delegates the canonical damage mutation to the
    /// existing firearm state machine.
    /// </summary>
    internal sealed class FirearmMisfireConditionService
    {
        internal FirearmMisfireConditionDecision Evaluate(
            FirearmMisfireDecision misfire,
            FirearmState postDischargeState)
        {
            if (misfire == null)
            {
                throw new ArgumentNullException("misfire");
            }

            if (postDischargeState == null)
            {
                throw new ArgumentNullException("postDischargeState");
            }

            if (!postDischargeState.IsEmpty)
            {
                throw new ArgumentException(
                    "Sprint 24 condition handling requires the exact firearm's loaded round to be discharged first.",
                    "postDischargeState");
            }

            if (postDischargeState.Condition == FirearmCondition.Wrecked)
            {
                throw new ArgumentException(
                    "A Wrecked firearm cannot be an eligible successfully discharged attack.",
                    "postDischargeState");
            }

            if (!misfire.IsMisfire)
            {
                return new FirearmMisfireConditionDecision(
                    misfire,
                    postDischargeState,
                    postDischargeState,
                    FirearmMisfireConditionTransition.None);
            }

            FirearmState after = FirearmStateMachine.ApplyMisfireDamage(
                postDischargeState);
            FirearmMisfireConditionTransition transition =
                postDischargeState.Condition == FirearmCondition.Normal
                    ? FirearmMisfireConditionTransition.NormalToBroken
                    : FirearmMisfireConditionTransition.BrokenToWrecked;
            return new FirearmMisfireConditionDecision(
                misfire,
                postDischargeState,
                after,
                transition);
        }
    }
}
