using System;

namespace KingmakerGunslinger.CraftMagicItemsCompatibility
{
    internal enum CraftMagicItemsCompatibilityAvailability
    {
        NotInstalled = 0,
        InstalledDisabled = 1,
        Incompatible = 2,
        Active = 3,
        Pending = 4
    }

    internal sealed class CraftMagicItemsCompatibilityStatus
    {
        internal CraftMagicItemsCompatibilityStatus(
            CraftMagicItemsCompatibilityAvailability availability,
            string detail, int itemTypes, int creationBases, int recipes)
        {
            Availability = availability;
            Detail = detail ?? string.Empty;
            ItemTypes = itemTypes;
            CreationBases = creationBases;
            Recipes = recipes;
        }

        internal CraftMagicItemsCompatibilityAvailability Availability
        { get; private set; }
        internal string Detail { get; private set; }
        internal int ItemTypes { get; private set; }
        internal int CreationBases { get; private set; }
        internal int Recipes { get; private set; }

        internal string Display
        {
            get
            {
                return Availability == CraftMagicItemsCompatibilityAvailability
                    .Active ? "active" :
                    Availability == CraftMagicItemsCompatibilityAvailability
                    .NotInstalled ? "not installed" :
                    Availability == CraftMagicItemsCompatibilityAvailability
                    .InstalledDisabled ? "installed but disabled" :
                    Availability == CraftMagicItemsCompatibilityAvailability
                    .Pending ? "initializing" : "incompatible, see log";
            }
        }
    }

    internal static class CraftMagicItemsCompatibilityStatusRegistry
    {
        private static readonly object Gate = new object();
        private static CraftMagicItemsCompatibilityStatus _current =
            new CraftMagicItemsCompatibilityStatus(
                CraftMagicItemsCompatibilityAvailability.NotInstalled,
                "CraftMagicItems UMM entry was not detected.", 0, 0, 0);

        internal static CraftMagicItemsCompatibilityStatus Current
        { get { lock (Gate) return _current; } }

        internal static void Update(CraftMagicItemsCompatibilityStatus value)
        {
            if (value == null) throw new ArgumentNullException("value");
            lock (Gate) _current = value;
        }
    }
}
