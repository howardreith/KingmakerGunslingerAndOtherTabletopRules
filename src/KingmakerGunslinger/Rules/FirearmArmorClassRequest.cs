using KingmakerGunslinger.Firearms;

namespace KingmakerGunslinger.Rules
{
    /// <summary>
    /// Immutable, game-object-free input to the firearm armor-class selector.
    /// CurrentTargetArmorClass may already include contextual modifiers such as
    /// cover, flat-footed state, or other rule-event adjustments.
    /// </summary>
    internal sealed class FirearmArmorClassRequest
    {
        internal FirearmArmorClassRequest(
            bool isExactFirearm,
            int markerCount,
            FirearmDefinition definition,
            double distanceMeters,
            int ordinaryArmorClass,
            int touchArmorClass,
            int currentTargetArmorClass,
            bool alreadyApplied)
            : this(isExactFirearm, markerCount, definition, distanceMeters,
                ordinaryArmorClass, touchArmorClass, currentTargetArmorClass,
                alreadyApplied, false, 0)
        {
        }

        internal FirearmArmorClassRequest(
            bool isExactFirearm,
            int markerCount,
            FirearmDefinition definition,
            double distanceMeters,
            int ordinaryArmorClass,
            int touchArmorClass,
            int currentTargetArmorClass,
            bool alreadyApplied,
            bool deadeyeAuthorized)
            : this(isExactFirearm, markerCount, definition, distanceMeters,
                ordinaryArmorClass, touchArmorClass, currentTargetArmorClass,
                alreadyApplied, deadeyeAuthorized, 0)
        {
        }

        internal FirearmArmorClassRequest(
            bool isExactFirearm, int markerCount, FirearmDefinition definition,
            double distanceMeters, int ordinaryArmorClass, int touchArmorClass,
            int currentTargetArmorClass, bool alreadyApplied,
            bool deadeyeAuthorized, int rangeIncrementBonusFeet)
        {
            IsExactFirearm = isExactFirearm;
            MarkerCount = markerCount;
            Definition = definition;
            DistanceMeters = distanceMeters;
            OrdinaryArmorClass = ordinaryArmorClass;
            TouchArmorClass = touchArmorClass;
            CurrentTargetArmorClass = currentTargetArmorClass;
            AlreadyApplied = alreadyApplied;
            DeadeyeAuthorized = deadeyeAuthorized;
            RangeIncrementBonusFeet = rangeIncrementBonusFeet;
        }

        internal bool IsExactFirearm { get; private set; }

        internal int MarkerCount { get; private set; }

        internal FirearmDefinition Definition { get; private set; }

        internal double DistanceMeters { get; private set; }

        internal int OrdinaryArmorClass { get; private set; }

        internal int TouchArmorClass { get; private set; }

        internal int CurrentTargetArmorClass { get; private set; }

        internal bool AlreadyApplied { get; private set; }

        internal bool DeadeyeAuthorized { get; private set; }
        internal int RangeIncrementBonusFeet { get; private set; }
    }
}
