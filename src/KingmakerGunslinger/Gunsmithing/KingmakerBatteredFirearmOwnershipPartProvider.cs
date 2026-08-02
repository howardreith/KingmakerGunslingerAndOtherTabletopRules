using System;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Utility;
using KingmakerGunslinger.Firearms;

namespace KingmakerGunslinger.Gunsmithing
{
    internal sealed class KingmakerBatteredFirearmOwnershipPartProvider
    {
        internal bool TryGetExisting(out UnitPartBatteredFirearmOwnership part)
        {
            UnitEntityData host;
            if (!KingmakerFirearmStateVaultPartProvider.TryResolveMainCharacter(
                out host))
            {
                part = null;
                return false;
            }
            part = host.Get<UnitPartBatteredFirearmOwnership>();
            return part != null;
        }

        internal UnitPartBatteredFirearmOwnership RequireForWrite()
        {
            UnitEntityData host;
            if (!KingmakerFirearmStateVaultPartProvider.TryResolveMainCharacter(
                out host))
                throw new InvalidOperationException(
                    "No active main-character persistence host is available for battered firearm ownership.");
            UnitPartBatteredFirearmOwnership part =
                host.Ensure<UnitPartBatteredFirearmOwnership>();
            if (part == null)
                throw new InvalidOperationException(
                    "Kingmaker did not return the battered firearm ownership UnitPart.");
            return part;
        }
    }
}
