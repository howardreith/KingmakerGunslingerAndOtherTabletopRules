using System;
using System.Globalization;

namespace KingmakerGunslinger.Firearms
{
    /// <summary>
    /// Pure deterministic transitions for one firearm state. Every operation
    /// either returns a new state or throws without modifying its input.
    /// </summary>
    internal static class FirearmStateMachine
    {
        internal static FirearmState Load(
            FirearmState state,
            FirearmStateRules rules,
            AmmunitionId ammunition,
            int rounds)
        {
            RequireState(state);
            RequireRules(rules);
            if (ammunition == null)
            {
                throw new ArgumentNullException("ammunition");
            }

            if (rounds <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    "rounds",
                    rounds,
                    "A load transition requires at least one round.");
            }

            ValidateStateAgainstRules(state, rules);
            if (state.Condition == FirearmCondition.Wrecked)
            {
                throw Rejected(FirearmStateTransitionError.Wrecked, "A wrecked firearm cannot be loaded.");
            }

            if (!rules.IsCompatible(ammunition))
            {
                throw Rejected(
                    FirearmStateTransitionError.IncompatibleAmmunition,
                    "The selected ammunition is not compatible with this firearm.");
            }

            if (!state.IsEmpty && state.LoadedAmmunition != ammunition)
            {
                throw Rejected(
                    FirearmStateTransitionError.MixedAmmunition,
                    "A firearm cannot mix ammunition IDs in one loaded state.");
            }

            int total;
            try
            {
                total = checked(state.LoadedRounds + rounds);
            }
            catch (OverflowException)
            {
                throw Rejected(
                    FirearmStateTransitionError.CapacityExceeded,
                    "The load transition exceeds firearm capacity.");
            }

            if (total > rules.Capacity)
            {
                throw Rejected(
                    FirearmStateTransitionError.CapacityExceeded,
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "The load transition would produce {0} rounds for capacity {1}.",
                        total,
                        rules.Capacity));
            }

            return new FirearmState(
                FirearmState.CurrentSchemaVersion,
                total,
                ammunition,
                state.Condition);
        }

        internal static FirearmState Fire(FirearmState state)
        {
            RequireState(state);
            if (state.Condition == FirearmCondition.Wrecked)
            {
                throw Rejected(FirearmStateTransitionError.Wrecked, "A wrecked firearm cannot fire.");
            }

            if (state.IsEmpty)
            {
                throw Rejected(FirearmStateTransitionError.Empty, "An empty firearm cannot fire.");
            }

            int remaining = state.LoadedRounds - 1;
            return new FirearmState(
                FirearmState.CurrentSchemaVersion,
                remaining,
                remaining == 0 ? null : state.LoadedAmmunition,
                state.Condition);
        }

        internal static FirearmState ApplyMisfireDamage(FirearmState state)
        {
            RequireState(state);
            if (state.Condition == FirearmCondition.Normal)
            {
                return new FirearmState(
                    FirearmState.CurrentSchemaVersion,
                    state.LoadedRounds,
                    state.LoadedAmmunition,
                    FirearmCondition.Broken);
            }

            if (state.Condition == FirearmCondition.Broken)
            {
                return new FirearmState(
                    FirearmState.CurrentSchemaVersion,
                    0,
                    null,
                    FirearmCondition.Wrecked);
            }

            throw Rejected(
                FirearmStateTransitionError.Wrecked,
                "A wrecked firearm cannot receive another misfire-damage transition.");
        }

        /// <summary>
        /// Unified maintenance transition: both Broken and Wrecked firearms return
        /// to Normal in one step. Surviving loaded ammunition is preserved exactly;
        /// a Wrecked firearm (which is always empty) stays empty.
        /// </summary>
        internal static FirearmState Repair(FirearmState state)
        {
            RequireState(state);
            if (state.Condition == FirearmCondition.Normal)
            {
                throw Rejected(
                    FirearmStateTransitionError.NotBroken,
                    "Only a Broken or Wrecked firearm can be repaired.");
            }

            return new FirearmState(
                FirearmState.CurrentSchemaVersion,
                state.LoadedRounds,
                state.LoadedAmmunition,
                FirearmCondition.Normal);
        }

        internal static FirearmState Wreck(FirearmState state)
        {
            RequireState(state);
            if (state.Condition == FirearmCondition.Wrecked)
            {
                return state;
            }

            return new FirearmState(
                FirearmState.CurrentSchemaVersion,
                0,
                null,
                FirearmCondition.Wrecked);
        }

        private static void RequireState(FirearmState state)
        {
            if (state == null)
            {
                throw new ArgumentNullException("state");
            }
        }

        private static void RequireRules(FirearmStateRules rules)
        {
            if (rules == null)
            {
                throw new ArgumentNullException("rules");
            }
        }

        private static void ValidateStateAgainstRules(FirearmState state, FirearmStateRules rules)
        {
            if (state.LoadedRounds > rules.Capacity)
            {
                throw new ArgumentException(
                    "The supplied state already exceeds the supplied firearm capacity.",
                    "state");
            }

            if (!state.IsEmpty && !rules.IsCompatible(state.LoadedAmmunition))
            {
                throw new ArgumentException(
                    "The supplied state contains ammunition that is incompatible with the supplied rules.",
                    "state");
            }
        }

        private static FirearmStateTransitionException Rejected(
            FirearmStateTransitionError error,
            string message)
        {
            return new FirearmStateTransitionException(error, message);
        }
    }
}
