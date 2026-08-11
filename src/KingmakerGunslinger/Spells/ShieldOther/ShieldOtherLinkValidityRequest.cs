namespace KingmakerGunslinger.Spells.ShieldOther
{
    internal sealed class ShieldOtherLinkValidityRequest
    {
        internal bool SubjectPresent { get; set; }
        internal bool CasterPresent { get; set; }
        internal bool CasterAlive { get; set; }
        internal bool SameArea { get; set; }
        internal int CasterLevel { get; set; }
        internal float DistanceFeet { get; set; }
    }
}
