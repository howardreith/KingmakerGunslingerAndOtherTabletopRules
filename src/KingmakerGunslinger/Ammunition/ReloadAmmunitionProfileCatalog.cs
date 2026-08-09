using System;
using System.Collections.Generic;
using KingmakerGunslinger.Firearms;

namespace KingmakerGunslinger.Ammunition
{
    internal static class ReloadAmmunitionProfileCatalog
    {
        internal const string PaperCartridgeIdValue =
            "kmg.ammunition.paper-cartridge";

        internal static readonly ReloadAmmunitionProfile LooseBasic =
            new ReloadAmmunitionProfile(
                FirearmStateTokenCatalog.DiagnosticLeadBall,
                ReloadAmmunitionSourceKind.LooseBasic,
                "Loose Black Powder and Lead Ball",
                null,
                Array.Empty<FirearmKind>(),
                1,
                0,
                0);

        internal static readonly ReloadAmmunitionProfile PaperCartridge =
            new ReloadAmmunitionProfile(
                new AmmunitionId(PaperCartridgeIdValue),
                ReloadAmmunitionSourceKind.PaperCartridge,
                "Paper Cartridge",
                FirearmEra.Early,
                new[] { FirearmKind.Pistol, FirearmKind.Musket,
                    FirearmKind.Blunderbuss },
                1,
                1,
                1);

        private static readonly Dictionary<string, ReloadAmmunitionProfile> ById =
            new Dictionary<string, ReloadAmmunitionProfile>(StringComparer.Ordinal)
            {
                { LooseBasic.LoadedAmmunition.Value, LooseBasic },
                { PaperCartridge.LoadedAmmunition.Value, PaperCartridge }
            };

        internal static bool TryResolve(AmmunitionId ammunition,
            out ReloadAmmunitionProfile profile)
        {
            profile = null;
            return ammunition != null && ById.TryGetValue(ammunition.Value, out profile);
        }

        internal static ReloadAmmunitionProfile Require(AmmunitionId ammunition)
        {
            ReloadAmmunitionProfile profile;
            if (!TryResolve(ammunition, out profile))
                throw new KeyNotFoundException("Unknown reload ammunition identity '" +
                    (ammunition == null ? "<null>" : ammunition.Value) + "'.");
            return profile;
        }
    }
}
