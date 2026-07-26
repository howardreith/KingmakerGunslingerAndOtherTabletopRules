using System;

namespace KingmakerGunslinger.Recovery
{
    /// <summary>
    /// Reports an ordinary repair failure without concealing independent rollback
    /// failures for the exact item-owned state or Firearm Repair Kit inventory.
    /// </summary>
    internal sealed class FirearmRepairTransactionException : InvalidOperationException
    {
        internal FirearmRepairTransactionException(
            string message,
            Exception operationException,
            Exception stateRollbackException,
            Exception inventoryRollbackException)
            : base(message, operationException)
        {
            OperationException = operationException ??
                throw new ArgumentNullException("operationException");
            StateRollbackException = stateRollbackException;
            InventoryRollbackException = inventoryRollbackException;
        }

        internal Exception OperationException { get; private set; }

        internal Exception StateRollbackException { get; private set; }

        internal Exception InventoryRollbackException { get; private set; }

        internal bool RollbackSucceeded
        {
            get
            {
                return StateRollbackException == null &&
                    InventoryRollbackException == null;
            }
        }
    }
}
