using System;
using System.Collections.Generic;
using System.Linq;
using KingmakerGunslinger.Firearms;

namespace KingmakerGunslinger.Ammunition
{
    /// <summary>
    /// Immutable project-owned rules identity for one reload ammunition choice.
    /// Inventory blueprints are bound later by the Kingmaker adapter; this domain
    /// profile owns the stable loaded identity, source shape, compatibility and
    /// action/misfire modifiers used by every consumer.
    /// </summary>
    internal sealed class ReloadAmmunitionProfile
    {
        private readonly FirearmKind[] _compatibleKinds;

        internal ReloadAmmunitionProfile(AmmunitionId loadedAmmunition,
            ReloadAmmunitionSourceKind sourceKind, string displayName,
            FirearmEra? compatibleEra, IEnumerable<FirearmKind> compatibleKinds,
            int roundsPerLoad, int reloadStepReduction, int misfireModifier)
        {
            if (loadedAmmunition == null) throw new ArgumentNullException("loadedAmmunition");
            if (!Enum.IsDefined(typeof(ReloadAmmunitionSourceKind), sourceKind) ||
                sourceKind == ReloadAmmunitionSourceKind.Unknown)
                throw new ArgumentOutOfRangeException("sourceKind");
            if (string.IsNullOrWhiteSpace(displayName))
                throw new ArgumentException("A player-facing ammunition name is required.", "displayName");
            if (compatibleEra.HasValue && (!Enum.IsDefined(typeof(FirearmEra),
                    compatibleEra.Value) || compatibleEra.Value == FirearmEra.Unknown))
                throw new ArgumentOutOfRangeException("compatibleEra");
            if (compatibleKinds == null) throw new ArgumentNullException("compatibleKinds");
            _compatibleKinds = compatibleKinds.Distinct().OrderBy(value => value).ToArray();
            if (_compatibleKinds.Any(value => !Enum.IsDefined(typeof(FirearmKind), value) ||
                    value == FirearmKind.Unknown))
                throw new ArgumentException("Compatible kinds must be defined non-Unknown values.",
                    "compatibleKinds");
            if (compatibleEra.HasValue && _compatibleKinds.Length == 0)
                throw new ArgumentException("An era-restricted profile requires compatible kinds.",
                    "compatibleKinds");
            if (roundsPerLoad != 1) throw new ArgumentOutOfRangeException("roundsPerLoad",
                "The current ammunition profiles load exactly one chamber.");
            if (reloadStepReduction < 0 || reloadStepReduction > 3)
                throw new ArgumentOutOfRangeException("reloadStepReduction");
            if (misfireModifier < 0 || misfireModifier > 20)
                throw new ArgumentOutOfRangeException("misfireModifier");

            LoadedAmmunition = loadedAmmunition;
            SourceKind = sourceKind;
            DisplayName = displayName;
            CompatibleEra = compatibleEra;
            RoundsPerLoad = roundsPerLoad;
            ReloadStepReduction = reloadStepReduction;
            MisfireModifier = misfireModifier;
        }

        internal AmmunitionId LoadedAmmunition { get; private set; }
        internal ReloadAmmunitionSourceKind SourceKind { get; private set; }
        internal string DisplayName { get; private set; }
        internal FirearmEra? CompatibleEra { get; private set; }
        internal int RoundsPerLoad { get; private set; }
        internal int ReloadStepReduction { get; private set; }
        internal int MisfireModifier { get; private set; }
        internal FirearmKind[] CompatibleKinds
        { get { return (FirearmKind[])_compatibleKinds.Clone(); } }

        internal bool IsCompatible(FirearmDefinition definition)
        {
            if (definition == null) return false;
            if (CompatibleEra.HasValue && definition.Era != CompatibleEra.Value)
                return false;
            return _compatibleKinds.Length == 0 ||
                Array.IndexOf(_compatibleKinds, definition.Kind) >= 0;
        }

        internal string CompatibilityRejection(FirearmDefinition definition)
        {
            if (definition == null) return "No exact firearm definition is available.";
            return IsCompatible(definition) ? string.Empty :
                DisplayName + " is incompatible with this firearm family.";
        }
    }
}
