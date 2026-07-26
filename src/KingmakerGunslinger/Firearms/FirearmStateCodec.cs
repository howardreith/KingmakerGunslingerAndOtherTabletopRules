using System;

namespace KingmakerGunslinger.Firearms
{
    /// <summary>
    /// Strict conversion between immutable state and a primitive DTO. This is not
    /// a Kingmaker save adapter and does not choose where the DTO is persisted.
    /// </summary>
    internal static class FirearmStateCodec
    {
        internal static FirearmStateData ToData(FirearmState state)
        {
            if (state == null)
            {
                throw new ArgumentNullException("state");
            }

            return new FirearmStateData
            {
                SchemaVersion = state.SchemaVersion,
                LoadedRounds = state.LoadedRounds,
                LoadedAmmunitionId = state.LoadedAmmunition == null
                    ? null
                    : state.LoadedAmmunition.Value,
                Condition = ToConditionToken(state.Condition)
            };
        }

        internal static FirearmState FromData(FirearmStateData data, FirearmStateRules rules)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }

            if (rules == null)
            {
                throw new ArgumentNullException("rules");
            }

            if (data.SchemaVersion != FirearmState.CurrentSchemaVersion)
            {
                throw new NotSupportedException(
                    "The firearm-state DTO schema version is not supported by this build.");
            }

            if (data.LoadedRounds < 0 || data.LoadedRounds > rules.Capacity)
            {
                throw new ArgumentOutOfRangeException(
                    "data",
                    data.LoadedRounds,
                    "The DTO loaded-round count is outside the supplied firearm capacity.");
            }

            FirearmCondition condition = ParseConditionToken(data.Condition);
            AmmunitionId ammunition = data.LoadedAmmunitionId == null
                ? null
                : new AmmunitionId(data.LoadedAmmunitionId);

            FirearmState state = new FirearmState(
                FirearmState.CurrentSchemaVersion,
                data.LoadedRounds,
                ammunition,
                condition);

            if (!state.IsEmpty && !rules.IsCompatible(state.LoadedAmmunition))
            {
                throw new ArgumentException(
                    "The DTO ammunition ID is incompatible with the supplied firearm rules.",
                    "data");
            }

            return state;
        }

        private static string ToConditionToken(FirearmCondition condition)
        {
            switch (condition)
            {
                case FirearmCondition.Normal:
                    return "normal";
                case FirearmCondition.Broken:
                    return "broken";
                case FirearmCondition.Wrecked:
                    return "wrecked";
                default:
                    throw new ArgumentOutOfRangeException(
                        "condition",
                        condition,
                        "A defined firearm condition is required.");
            }
        }

        private static FirearmCondition ParseConditionToken(string token)
        {
            if (string.Equals(token, "normal", StringComparison.Ordinal))
            {
                return FirearmCondition.Normal;
            }

            if (string.Equals(token, "broken", StringComparison.Ordinal))
            {
                return FirearmCondition.Broken;
            }

            if (string.Equals(token, "wrecked", StringComparison.Ordinal))
            {
                return FirearmCondition.Wrecked;
            }

            throw new ArgumentException(
                "The DTO condition token must be exactly 'normal', 'broken', or 'wrecked'.",
                "token");
        }
    }
}
