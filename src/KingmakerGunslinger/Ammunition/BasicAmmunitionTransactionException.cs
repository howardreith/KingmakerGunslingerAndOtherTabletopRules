using System;

namespace KingmakerGunslinger.Ammunition
{
    /// <summary>
    /// Reports a mutation failure while preserving whether rollback also failed.
    /// </summary>
    internal sealed class BasicAmmunitionTransactionException : InvalidOperationException
    {
        internal BasicAmmunitionTransactionException(
            string message,
            Exception mutationException,
            Exception rollbackException)
            : base(message, mutationException)
        {
            MutationException = mutationException ?? throw new ArgumentNullException("mutationException");
            RollbackException = rollbackException;
        }

        internal Exception MutationException { get; private set; }

        internal Exception RollbackException { get; private set; }

        internal bool RollbackFailed
        {
            get { return RollbackException != null; }
        }
    }
}
