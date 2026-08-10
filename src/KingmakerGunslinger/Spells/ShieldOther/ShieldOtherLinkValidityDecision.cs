namespace KingmakerGunslinger.Spells.ShieldOther
{
    internal sealed class ShieldOtherLinkValidityDecision
    {
        internal ShieldOtherLinkValidityDecision(bool valid, string status,
            int maximumRangeFeet)
        { Valid = valid; Status = status; MaximumRangeFeet = maximumRangeFeet; }

        internal bool Valid { get; private set; }
        internal string Status { get; private set; }
        internal int MaximumRangeFeet { get; private set; }
    }
}
