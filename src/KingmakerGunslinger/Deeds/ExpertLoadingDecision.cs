namespace KingmakerGunslinger.Deeds
{
    public sealed class ExpertLoadingDecision
    {
        internal ExpertLoadingDecision(bool consumeMarker, bool suppressExplosion)
        {
            ConsumeMarker = consumeMarker;
            SuppressExplosion = suppressExplosion;
        }

        public bool ConsumeMarker { get; private set; }
        public bool SuppressExplosion { get; private set; }
        public int GritCost { get { return SuppressExplosion ? 1 : 0; } }
    }
}
