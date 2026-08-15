namespace KingmakerGunslinger.BrownFur
{
    internal enum CotwContractAvailability
    {
        Unavailable = 0,
        Compatible = 1,
        Incompatible = 2
    }

    internal sealed class CotwArcanistContractDecision
    {
        internal CotwArcanistContractDecision(CotwContractAvailability availability,
            CotwProgressionDecision progression, string failedCheck)
        {
            Availability = availability;
            Progression = progression;
            FailedCheck = failedCheck ?? string.Empty;
        }

        internal CotwContractAvailability Availability { get; private set; }
        internal CotwProgressionDecision Progression { get; private set; }
        internal string FailedCheck { get; private set; }
        internal bool IsCompatible
        { get { return Availability == CotwContractAvailability.Compatible; } }
    }
}
