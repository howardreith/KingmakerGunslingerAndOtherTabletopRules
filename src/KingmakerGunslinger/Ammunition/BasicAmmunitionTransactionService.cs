using System;

namespace KingmakerGunslinger.Ammunition
{
    /// <summary>
    /// Consumes exactly one powder charge and one projectile as one verified transaction.
    /// A rejected request performs no writes. A partial runtime failure triggers a best-effort
    /// restoration to the exact pre-transaction counts and is never reported as success.
    /// </summary>
    internal sealed class BasicAmmunitionTransactionService
    {
        internal BasicAmmunitionTransactionResult TryConsumeOneLoad(
            IBasicAmmunitionInventory inventory)
        {
            return TryConsumeLoads(inventory, 1);
        }

        internal BasicAmmunitionTransactionResult TryConsumeLoads(
            IBasicAmmunitionInventory inventory,
            int loads)
        {
            if (inventory == null)
            {
                throw new ArgumentNullException("inventory");
            }
            if (loads <= 0)
            {
                throw new ArgumentOutOfRangeException("loads");
            }

            BasicAmmunitionInventorySnapshot before =
                BasicAmmunitionInventorySnapshot.Capture(inventory);
            if (before.BlackPowderCharges < loads || before.LeadBalls < loads)
            {
                return new BasicAmmunitionTransactionResult(
                    BasicAmmunitionTransactionStatus.InsufficientComponents,
                    before,
                    before);
            }

            try
            {
                inventory.Remove(BasicAmmunitionComponent.BlackPowderCharge, loads);
                inventory.Remove(BasicAmmunitionComponent.LeadBall, loads);

                BasicAmmunitionInventorySnapshot after =
                    BasicAmmunitionInventorySnapshot.Capture(inventory);
                BasicAmmunitionInventorySnapshot expected =
                    new BasicAmmunitionInventorySnapshot(
                        before.BlackPowderCharges - loads,
                        before.LeadBalls - loads);
                if (!expected.Equals(after))
                {
                    throw new InvalidOperationException(
                        "The inventory did not contain the exact expected counts after consumption. " +
                        "Expected [" + expected + "]; observed [" + after + "].");
                }

                return new BasicAmmunitionTransactionResult(
                    BasicAmmunitionTransactionStatus.Consumed,
                    before,
                    after);
            }
            catch (Exception mutationException)
            {
                Exception rollbackException = null;
                try
                {
                    RestoreExact(inventory, before);
                }
                catch (Exception exception)
                {
                    rollbackException = exception;
                }

                throw new BasicAmmunitionTransactionException(
                    rollbackException == null
                        ? "Basic-ammunition consumption failed and the original counts were restored."
                        : "Basic-ammunition consumption failed and rollback could not restore the original counts.",
                    mutationException,
                    rollbackException);
            }
        }

        internal void RestoreExact(
            IBasicAmmunitionInventory inventory,
            BasicAmmunitionInventorySnapshot expected)
        {
            if (inventory == null)
            {
                throw new ArgumentNullException("inventory");
            }

            if (expected == null)
            {
                throw new ArgumentNullException("expected");
            }

            RestoreOne(inventory, BasicAmmunitionComponent.BlackPowderCharge, expected.BlackPowderCharges);
            RestoreOne(inventory, BasicAmmunitionComponent.LeadBall, expected.LeadBalls);

            BasicAmmunitionInventorySnapshot restored =
                BasicAmmunitionInventorySnapshot.Capture(inventory);
            if (!expected.Equals(restored))
            {
                throw new InvalidOperationException(
                    "Rollback verification failed. Expected [" + expected + "]; observed [" + restored + "].");
            }
        }

        private static void RestoreOne(
            IBasicAmmunitionInventory inventory,
            BasicAmmunitionComponent component,
            int expectedCount)
        {
            int current = inventory.Count(component);
            if (current < 0)
            {
                throw new InvalidOperationException(
                    "Cannot restore a component whose inventory count is negative.");
            }

            if (current < expectedCount)
            {
                inventory.Add(component, expectedCount - current);
            }
            else if (current > expectedCount)
            {
                inventory.Remove(component, current - expectedCount);
            }
        }
    }
}
