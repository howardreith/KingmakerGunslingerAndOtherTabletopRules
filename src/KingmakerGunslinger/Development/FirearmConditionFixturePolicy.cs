using System;
using KingmakerGunslinger.Firearms;

namespace KingmakerGunslinger.Development
{
    internal enum FirearmConditionFixtureOperation
    {
        Break = 0,
        Wreck = 1
    }

    internal sealed class FirearmConditionFixtureDecision
    {
        internal FirearmConditionFixtureDecision(
            FirearmConditionFixtureOperation operation,
            FirearmState before,
            FirearmState after,
            bool accepted,
            string reason)
        {
            Operation = operation;
            Before = before ?? throw new ArgumentNullException("before");
            After = after ?? throw new ArgumentNullException("after");
            Accepted = accepted;
            Reason = reason ?? throw new ArgumentNullException("reason");
        }

        internal FirearmConditionFixtureOperation Operation { get; private set; }
        internal FirearmState Before { get; private set; }
        internal FirearmState After { get; private set; }
        internal bool Accepted { get; private set; }
        internal string Reason { get; private set; }
    }

    /// <summary>
    /// Pure guard for the two deterministic UMM condition-fixture controls. The
    /// accepted transition still delegates to the canonical misfire state machine;
    /// rejected states never reach the item repository.
    /// </summary>
    internal static class FirearmConditionFixturePolicy
    {
        internal static FirearmConditionFixtureDecision Decide(
            FirearmConditionFixtureOperation operation,
            FirearmState current)
        {
            if (current == null) throw new ArgumentNullException("current");
            if (!Enum.IsDefined(typeof(FirearmConditionFixtureOperation),
                    operation))
                throw new ArgumentOutOfRangeException("operation");

            FirearmCondition required = operation ==
                FirearmConditionFixtureOperation.Break
                    ? FirearmCondition.Normal
                    : FirearmCondition.Broken;
            if (current.Condition != required)
            {
                return new FirearmConditionFixtureDecision(operation,
                    current, current, false,
                    Rejection(operation, current.Condition));
            }

            FirearmState after = FirearmStateMachine.ApplyMisfireDamage(
                current);
            FirearmCondition expected = operation ==
                FirearmConditionFixtureOperation.Break
                    ? FirearmCondition.Broken
                    : FirearmCondition.Wrecked;
            if (after.Condition != expected ||
                (operation == FirearmConditionFixtureOperation.Break &&
                    (after.LoadedRounds != current.LoadedRounds ||
                     !Equals(after.LoadedAmmunition,
                         current.LoadedAmmunition))) ||
                (operation == FirearmConditionFixtureOperation.Wreck &&
                    !after.IsEmpty))
                throw new InvalidOperationException(
                    "The canonical firearm state machine did not produce the requested diagnostic condition transition.");

            return new FirearmConditionFixtureDecision(operation, current,
                after, true, operation ==
                    FirearmConditionFixtureOperation.Break
                        ? "Normal -> Broken"
                        : "Broken -> Wrecked (empty)");
        }

        private static string Rejection(
            FirearmConditionFixtureOperation operation,
            FirearmCondition condition)
        {
            if (operation == FirearmConditionFixtureOperation.Break)
            {
                return condition == FirearmCondition.Broken
                    ? "Break rejected: the selected equipped firearm is already Broken; use the Wreck diagnostic next."
                    : "Break rejected: the selected equipped firearm is Wrecked; reset or overhaul it before breaking it.";
            }

            return condition == FirearmCondition.Normal
                ? "Wreck rejected: the selected equipped firearm is Normal; break it first."
                : "Wreck rejected: the selected equipped firearm is already Wrecked.";
        }
    }
}
