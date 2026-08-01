using System;
using System.Globalization;

namespace KingmakerGunslinger.Ammunition
{
    internal enum BasicAmmunitionTransactionStatus
    {
        Consumed = 1,
        InsufficientComponents = 2
    }

    /// <summary>
    /// Immutable result of attempting to consume one black-powder charge and one lead ball.
    /// </summary>
    internal sealed class BasicAmmunitionTransactionResult
    {
        internal BasicAmmunitionTransactionResult(
            BasicAmmunitionTransactionStatus status,
            BasicAmmunitionInventorySnapshot before,
            BasicAmmunitionInventorySnapshot after)
        {
            if (!Enum.IsDefined(typeof(BasicAmmunitionTransactionStatus), status))
            {
                throw new ArgumentOutOfRangeException("status", status, "Unknown transaction status.");
            }

            Status = status;
            Before = before ?? throw new ArgumentNullException("before");
            After = after ?? throw new ArgumentNullException("after");

            if (status == BasicAmmunitionTransactionStatus.Consumed)
            {
                int powderConsumed = Before.BlackPowderCharges - After.BlackPowderCharges;
                int ballsConsumed = Before.LeadBalls - After.LeadBalls;
                if (powderConsumed <= 0 || powderConsumed != ballsConsumed)
                {
                    throw new ArgumentException(
                        "A successful ammunition transaction must consume the same positive count of each component.");
                }
            }
            else if (!Before.Equals(After))
            {
                throw new ArgumentException(
                    "An insufficient-components result must not change the inventory.");
            }
        }

        internal BasicAmmunitionTransactionStatus Status { get; private set; }

        internal BasicAmmunitionInventorySnapshot Before { get; private set; }

        internal BasicAmmunitionInventorySnapshot After { get; private set; }

        internal bool Succeeded
        {
            get { return Status == BasicAmmunitionTransactionStatus.Consumed; }
        }

        internal int LoadsConsumed
        {
            get { return Succeeded ? Before.BlackPowderCharges - After.BlackPowderCharges : 0; }
        }

        public override string ToString()
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "status={0}; before=[{1}]; after=[{2}]",
                Status,
                Before,
                After);
        }
    }
}
