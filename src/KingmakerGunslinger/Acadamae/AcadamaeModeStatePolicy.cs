namespace KingmakerGunslinger.Acadamae
{
    internal sealed class AcadamaeEffectiveModeState
    {
        internal AcadamaeEffectiveModeState(bool active, string status,
            bool hasFeat, bool hasActivatable, bool activatableIsOn,
            bool markerPresent)
        {
            Active = active;
            Status = status;
            HasFeat = hasFeat;
            HasActivatable = hasActivatable;
            ActivatableIsOn = activatableIsOn;
            MarkerPresent = markerPresent;
        }

        internal bool Active { get; private set; }
        internal string Status { get; private set; }
        internal bool HasFeat { get; private set; }
        internal bool HasActivatable { get; private set; }
        internal bool ActivatableIsOn { get; private set; }
        internal bool MarkerPresent { get; private set; }
    }

    internal static class AcadamaeModeStatePolicy
    {
        internal static AcadamaeEffectiveModeState Decide(bool hasFeat,
            bool hasActivatable, bool activatableIsOn, bool markerPresent)
        {
            if (!hasFeat)
                return new AcadamaeEffectiveModeState(false, "feat-absent",
                    false, hasActivatable, activatableIsOn, markerPresent);
            if (!hasActivatable)
                return new AcadamaeEffectiveModeState(false,
                    markerPresent ? "activatable-missing-marker-present" :
                        "activatable-missing",
                    true, false, false, markerPresent);
            if (!activatableIsOn)
                return new AcadamaeEffectiveModeState(false,
                    markerPresent ? "off-marker-lingering" : "off",
                    true, true, false, markerPresent);
            return new AcadamaeEffectiveModeState(true,
                markerPresent ? "on" : "on-marker-pending",
                true, true, true, markerPresent);
        }
    }
}
