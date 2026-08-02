using System;
using System.Globalization;
using KingmakerGunslinger.Firearms;

namespace KingmakerGunslinger.Misfires
{
    /// <summary>
    /// Immutable Sprint 24 result joining one natural-roll classification to the
    /// exact post-discharge firearm-state transition it requires.
    /// </summary>
    internal sealed class FirearmMisfireConditionDecision
    {
        internal FirearmMisfireConditionDecision(
            FirearmMisfireDecision misfire,
            FirearmState before,
            FirearmState after,
            FirearmMisfireConditionTransition transition)
            : this(misfire, before, after,
                before == null ? FirearmCondition.Wrecked : before.Condition,
                transition)
        {
        }

        internal FirearmMisfireConditionDecision(
            FirearmMisfireDecision misfire,
            FirearmState before,
            FirearmState after,
            FirearmCondition effectiveCondition,
            FirearmMisfireConditionTransition transition)
        {
            Misfire = misfire ?? throw new ArgumentNullException("misfire");
            Before = before ?? throw new ArgumentNullException("before");
            After = after ?? throw new ArgumentNullException("after");

            if (!Enum.IsDefined(typeof(FirearmMisfireConditionTransition), transition))
            {
                throw new ArgumentOutOfRangeException("transition", transition, "A defined condition transition is required.");
            }

            Validate(misfire, before, after, effectiveCondition, transition);
            EffectiveCondition = effectiveCondition;
            Transition = transition;
        }

        internal FirearmMisfireDecision Misfire { get; private set; }

        internal FirearmState Before { get; private set; }

        internal FirearmState After { get; private set; }

        internal FirearmCondition EffectiveCondition { get; private set; }

        internal FirearmMisfireConditionTransition Transition { get; private set; }

        internal bool ChangesCondition
        {
            get { return Transition != FirearmMisfireConditionTransition.None; }
        }

        public override string ToString()
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "conditionTransition={0}; conditionBefore={1}; effectiveCondition={2}; conditionAfter={3}; stateBefore=[{4}]; stateAfter=[{5}]",
                Transition,
                Before.Condition,
                EffectiveCondition,
                After.Condition,
                Before,
                After);
        }

        private static void Validate(
            FirearmMisfireDecision misfire,
            FirearmState before,
            FirearmState after,
            FirearmCondition effectiveCondition,
            FirearmMisfireConditionTransition transition)
        {
            switch (transition)
            {
                case FirearmMisfireConditionTransition.None:
                    if (misfire.IsMisfire)
                    {
                        throw new ArgumentException(
                            "A detected misfire requires one of the bounded Sprint 24 condition transitions.",
                            "transition");
                    }

                    if (before != after)
                    {
                        throw new ArgumentException(
                            "An ordinary firearm roll cannot change item-owned firearm state.",
                            "after");
                    }

                    return;

                case FirearmMisfireConditionTransition.NormalToBroken:
                    RequireMisfire(misfire);
                    if (effectiveCondition != FirearmCondition.Normal ||
                        before.Condition != FirearmCondition.Normal ||
                        after.Condition != FirearmCondition.Broken ||
                        before.LoadedRounds != after.LoadedRounds ||
                        before.LoadedAmmunition != after.LoadedAmmunition)
                    {
                        throw new ArgumentException(
                            "NormalToBroken requires an empty Normal state followed by an empty Broken state.");
                    }

                    return;

                case FirearmMisfireConditionTransition.BrokenToWrecked:
                    RequireMisfire(misfire);
                    if (effectiveCondition != FirearmCondition.Broken ||
                        (before.Condition != FirearmCondition.Normal &&
                         before.Condition != FirearmCondition.Broken) ||
                        after.Condition != FirearmCondition.Wrecked || !after.IsEmpty)
                    {
                        throw new ArgumentException(
                            "BrokenToWrecked requires an empty Broken state followed by an empty Wrecked state.");
                    }

                    return;

                case FirearmMisfireConditionTransition.AdvancedBrokenRemainsBroken:
                case FirearmMisfireConditionTransition.ExpertLoadingBrokenRemainsBroken:
                    RequireMisfire(misfire);
                    if (effectiveCondition != FirearmCondition.Broken ||
                        (before.Condition != FirearmCondition.Normal &&
                         before.Condition != FirearmCondition.Broken) ||
                        before != after)
                    {
                        throw new ArgumentException(
                            "A remains-Broken transition requires an unchanged Broken state.");
                    }
                    return;

                default:
                    throw new ArgumentOutOfRangeException("transition", transition, "Unsupported condition transition.");
            }
        }

        private static void RequireMisfire(FirearmMisfireDecision decision)
        {
            if (!decision.IsMisfire || decision.FinalSuccess)
            {
                throw new ArgumentException(
                    "A firearm condition transition requires a detected misfire whose final attack result is a miss.",
                    "misfire");
            }
        }
    }
}
