using System;
using System.Globalization;
using KingmakerGunslinger.Firearms;

namespace KingmakerGunslinger.Firing
{
    /// <summary>
    /// Immutable result of deciding whether one firearm attack roll may discharge.
    /// The result does not know about Kingmaker rule events or inventory ammunition.
    /// </summary>
    internal sealed class FirearmDischargeResult
    {
        internal FirearmDischargeResult(
            FirearmDischargeStatus status,
            FirearmState before,
            FirearmState after,
            int roundsConsumed,
            bool shouldForceMiss)
        {
            if (!Enum.IsDefined(typeof(FirearmDischargeStatus), status))
            {
                throw new ArgumentOutOfRangeException("status");
            }

            Before = before ?? throw new ArgumentNullException("before");
            After = after ?? throw new ArgumentNullException("after");
            if (roundsConsumed < 0 || roundsConsumed > 1)
            {
                throw new ArgumentOutOfRangeException(
                    "roundsConsumed",
                    roundsConsumed,
                    "One attack roll can consume zero or one loaded round.");
            }

            Status = status;
            RoundsConsumed = roundsConsumed;
            ShouldForceMiss = shouldForceMiss;
            Validate();
        }

        internal FirearmDischargeStatus Status { get; private set; }

        internal FirearmState Before { get; private set; }

        internal FirearmState After { get; private set; }

        internal int RoundsConsumed { get; private set; }

        internal bool ShouldForceMiss { get; private set; }

        public override string ToString()
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "status={0}; roundsConsumed={1}; forceMiss={2}; before=[{3}]; after=[{4}]",
                Status,
                RoundsConsumed,
                ShouldForceMiss,
                Before,
                After);
        }

        private void Validate()
        {
            switch (Status)
            {
                case FirearmDischargeStatus.Fired:
                    if (ShouldForceMiss || RoundsConsumed != 1 || Before.IsEmpty)
                    {
                        throw new ArgumentException(
                            "A fired result must consume exactly one loaded round and must not force a miss.");
                    }

                    FirearmState expected = FirearmStateMachine.Fire(Before);
                    if (After != expected)
                    {
                        throw new ArgumentException(
                            "A fired result's after-state must equal the canonical Fire transition.");
                    }

                    return;

                case FirearmDischargeStatus.Empty:
                    if (!ShouldForceMiss || RoundsConsumed != 0 || !Before.IsEmpty ||
                        Before.Condition == FirearmCondition.Wrecked || After != Before)
                    {
                        throw new ArgumentException(
                            "An empty result must preserve an empty non-wrecked state and force a miss.");
                    }

                    return;

                case FirearmDischargeStatus.Wrecked:
                    if (!ShouldForceMiss || RoundsConsumed != 0 ||
                        Before.Condition != FirearmCondition.Wrecked || After != Before)
                    {
                        throw new ArgumentException(
                            "A wrecked result must preserve wrecked state and force a miss.");
                    }

                    return;

                default:
                    throw new ArgumentOutOfRangeException("Status");
            }
        }
    }
}
