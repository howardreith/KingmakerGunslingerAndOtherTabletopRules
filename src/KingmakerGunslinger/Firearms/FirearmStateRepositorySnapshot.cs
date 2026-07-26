using System;
using System.Globalization;

namespace KingmakerGunslinger.Firearms
{
    /// <summary>
    /// Immutable diagnostic copy of one process-local repository entry. It retains
    /// no reference to the runtime item key.
    /// </summary>
    internal sealed class FirearmStateRepositorySnapshot
    {
        internal FirearmStateRepositorySnapshot(
            long entryId,
            int revision,
            string runtimeTypeName,
            int runtimeReferenceHash,
            FirearmState state)
        {
            if (entryId <= 0)
            {
                throw new ArgumentOutOfRangeException("entryId");
            }

            if (revision < 0)
            {
                throw new ArgumentOutOfRangeException("revision");
            }

            if (string.IsNullOrWhiteSpace(runtimeTypeName))
            {
                throw new ArgumentException("A runtime type name is required.", "runtimeTypeName");
            }

            EntryId = entryId;
            Revision = revision;
            RuntimeTypeName = runtimeTypeName.Trim();
            RuntimeReferenceHash = runtimeReferenceHash;
            State = state ?? throw new ArgumentNullException("state");
        }

        internal long EntryId { get; private set; }

        internal int Revision { get; private set; }

        internal string RuntimeTypeName { get; private set; }

        internal int RuntimeReferenceHash { get; private set; }

        internal FirearmState State { get; private set; }

        internal string RepositoryIdentity
        {
            get
            {
                return string.Format(
                    CultureInfo.InvariantCulture,
                    "kmg-item-{0:D6}",
                    EntryId);
            }
        }

        public override string ToString()
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "repositoryIdentity={0}; revision={1}; runtimeType={2}; referenceHash=0x{3:x8}; state=[{4}]",
                RepositoryIdentity,
                Revision,
                RuntimeTypeName,
                RuntimeReferenceHash,
                State);
        }
    }
}
