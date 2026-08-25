using System;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Utility;
using KingmakerGunslinger.Firearms;

namespace KingmakerGunslinger.Gunsmithing
{
    internal sealed class KingmakerBatteredFirearmOwnershipPartProvider
    {
        internal bool TryGetExisting(UnitEntityData fallbackHost,
            out UnitPartBatteredFirearmOwnership part)
        {
            UnitEntityData host;
            if (!TryResolveHost(fallbackHost, out host))
            {
                part = null;
                return false;
            }
            part = host.Get<UnitPartBatteredFirearmOwnership>();
            return part != null;
        }

        internal UnitPartBatteredFirearmOwnership RequireForWrite(
            UnitEntityData fallbackHost)
        {
            UnitEntityData host;
            if (!TryResolveHost(fallbackHost, out host))
                throw new InvalidOperationException(
                    "No active main-character persistence host is available for battered firearm ownership.");
            UnitPartBatteredFirearmOwnership part =
                host.Ensure<UnitPartBatteredFirearmOwnership>();
            if (part == null)
                throw new InvalidOperationException(
                    "Kingmaker did not return the battered firearm ownership UnitPart.");
            return part;
        }

        internal bool RemoveIfEmpty(UnitPartBatteredFirearmOwnership part,
            UnitEntityData fallbackHost)
        {
            if (part == null) throw new ArgumentNullException("part");
            UnitEntityData host;
            if (!TryResolveHost(fallbackHost, out host) || !ReferenceEquals(
                    host.Get<UnitPartBatteredFirearmOwnership>(), part) ||
                part.Count != 0) return false;
            host.Remove<UnitPartBatteredFirearmOwnership>();
            return host.Get<UnitPartBatteredFirearmOwnership>() == null;
        }

        private static bool TryResolveHost(UnitEntityData fallbackHost,
            out UnitEntityData host)
        {
            if (KingmakerFirearmStateVaultPartProvider.TryResolveMainCharacter(
                    out host)) return true;
            host = fallbackHost;
            return host != null && !string.IsNullOrWhiteSpace(host.UniqueId);
        }
    }
}
