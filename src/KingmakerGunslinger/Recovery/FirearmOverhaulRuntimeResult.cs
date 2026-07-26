using System;
using System.Globalization;
using KingmakerGunslinger.Firearms;

namespace KingmakerGunslinger.Recovery
{
    /// <summary>
    /// Correlates one domain transaction with the exact item repository snapshots that
    /// prove same-item identity and exactly one successful state revision.
    /// </summary>
    internal sealed class FirearmOverhaulRuntimeResult
    {
        internal FirearmOverhaulRuntimeResult(
            FirearmOverhaulResult transaction,
            FirearmItemStateSnapshot beforeFirearm,
            FirearmItemStateSnapshot afterFirearm)
        {
            Transaction = transaction ?? throw new ArgumentNullException("transaction");
            BeforeFirearm = beforeFirearm ?? throw new ArgumentNullException("beforeFirearm");
            AfterFirearm = afterFirearm ?? throw new ArgumentNullException("afterFirearm");

            if (!string.Equals(
                    BeforeFirearm.Repository.RepositoryIdentity,
                    AfterFirearm.Repository.RepositoryIdentity,
                    StringComparison.Ordinal) ||
                BeforeFirearm.Repository.RuntimeReferenceHash !=
                    AfterFirearm.Repository.RuntimeReferenceHash)
            {
                throw new ArgumentException(
                    "An overhaul runtime result must preserve exact repository and runtime-reference identity.");
            }

            if (BeforeFirearm.Repository.State != Transaction.BeforeState ||
                AfterFirearm.Repository.State != Transaction.AfterState)
            {
                throw new ArgumentException(
                    "Overhaul transaction states must match the exact item repository snapshots.");
            }

            long expectedRevision = Transaction.Succeeded
                ? BeforeFirearm.Repository.Revision + 1
                : BeforeFirearm.Repository.Revision;
            if (AfterFirearm.Repository.Revision != expectedRevision)
            {
                throw new ArgumentException(
                    "A successful overhaul must advance the exact item revision once; a rejection must not advance it.");
            }
        }

        internal FirearmOverhaulResult Transaction { get; private set; }

        internal FirearmItemStateSnapshot BeforeFirearm { get; private set; }

        internal FirearmItemStateSnapshot AfterFirearm { get; private set; }

        internal bool Succeeded
        {
            get { return Transaction.Succeeded; }
        }

        public override string ToString()
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "{0}; repositoryIdentity={1}; referenceHash=0x{2:x8}; revision={3}->{4}; exactItemPreserved=True",
                Transaction,
                AfterFirearm.Repository.RepositoryIdentity,
                AfterFirearm.Repository.RuntimeReferenceHash,
                BeforeFirearm.Repository.Revision,
                AfterFirearm.Repository.Revision);
        }
    }
}
