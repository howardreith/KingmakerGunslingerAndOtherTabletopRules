using System;

namespace KingmakerGunslinger.Reloading
{
    /// <summary>
    /// Reports a cross-resource reload failure without concealing failures that occurred
    /// while restoring the firearm or shared inventory to their exact pre-operation state.
    /// </summary>
    internal sealed class FirearmReloadTransactionException : InvalidOperationException
    {
        internal FirearmReloadTransactionException(
            string message,
            Exception operationException,
            Exception stateRollbackException,
            Exception inventoryRollbackException)
            : base(message, operationException)
        {
            OperationException = operationException ?? throw new ArgumentNullException("operationException");
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
                return StateRollbackException == null && InventoryRollbackException == null;
            }
        }
    }
}
