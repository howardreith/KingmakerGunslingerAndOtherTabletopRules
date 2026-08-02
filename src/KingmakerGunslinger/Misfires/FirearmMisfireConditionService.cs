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
            if (postDischargeState != null && !postDischargeState.IsEmpty)
                throw new ArgumentException(
                    "The capacity-one compatibility path requires an empty post-discharge state.",
                    "postDischargeState");
            return Evaluate(FirearmDefinitions.CreateEarlyMusket(), misfire, postDischargeState);
        }

        internal FirearmMisfireConditionDecision Evaluate(
            FirearmDefinition definition,
            FirearmMisfireDecision misfire,
            FirearmState postDischargeState)
        {
            return Evaluate(definition, misfire, postDischargeState,
                postDischargeState == null
                    ? FirearmCondition.Wrecked
                    : postDischargeState.Condition);
        }

        internal FirearmMisfireConditionDecision Evaluate(
            FirearmMisfireDecision misfire,
            FirearmState postDischargeState,
            FirearmCondition effectiveCondition)
        {
            return Evaluate(FirearmDefinitions.CreateEarlyMusket(), misfire,
                postDischargeState, effectiveCondition);
        }

        internal FirearmMisfireConditionDecision Evaluate(
            FirearmDefinition definition,
            FirearmMisfireDecision misfire,
            FirearmState postDischargeState,
            FirearmCondition effectiveCondition)
        {
            if (definition == null)
                throw new ArgumentNullException("definition");
            if (misfire == null)
            {
                throw new ArgumentNullException("misfire");
            }

            if (postDischargeState == null)
            {
                throw new ArgumentNullException("postDischargeState");
            }

            if (effectiveCondition == FirearmCondition.Wrecked)
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
                    effectiveCondition,
                    FirearmMisfireConditionTransition.None);
            }

            FirearmState after;
            FirearmMisfireConditionTransition transition;
            if (effectiveCondition == FirearmCondition.Normal)
            {
                after = FirearmStateMachine.ApplyMisfireDamage(postDischargeState);
                transition = FirearmMisfireConditionTransition.NormalToBroken;
            }
            else if (definition.Era == FirearmEra.Advanced)
            {
                after = postDischargeState;
                transition = FirearmMisfireConditionTransition.AdvancedBrokenRemainsBroken;
            }
            else
            {
                after = new FirearmState(
                    postDischargeState.SchemaVersion,
                    0,
                    null,
                    FirearmCondition.Wrecked);
                transition = FirearmMisfireConditionTransition.BrokenToWrecked;
            }
            return new FirearmMisfireConditionDecision(
                misfire,
                postDischargeState,
                after,
                effectiveCondition,
                transition);
        }
    }
}
