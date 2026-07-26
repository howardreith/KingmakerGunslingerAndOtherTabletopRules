using System;
using System.Globalization;

namespace KingmakerGunslinger.Firearms
{
    /// <summary>
    /// Signals that a Sprint 13 direct-reference record and a Sprint 14 identity
    /// record claim different states for the same engine-issued item identity.
    /// </summary>
    internal sealed class FirearmStateIdentityMigrationConflictException : InvalidOperationException
    {
        internal FirearmStateIdentityMigrationConflictException(FirearmItemId itemId)
            : base(string.Format(
                CultureInfo.InvariantCulture,
                "Conflicting Sprint 13 and Sprint 14 firearm states exist for engine item identity '{0}'. Both carriers were preserved for diagnosis.",
                itemId == null ? "<null>" : itemId.Value))
        {
            ItemId = itemId;
        }

        internal FirearmItemId ItemId { get; private set; }
    }
}
