namespace KingmakerGunslinger.Deeds
{
    internal sealed class BleedingWoundDecision
    {
        internal BleedingWoundDecision(BleedingWoundKind kind,
            bool consumeMarker, bool apply, int gritCost, int bleedAmount,
            string reason)
        {
            Kind = kind;
            ConsumeMarker = consumeMarker;
            Apply = apply;
            GritCost = gritCost;
            BleedAmount = bleedAmount;
            Reason = reason;
        }

        internal BleedingWoundKind Kind { get; private set; }
        internal bool ConsumeMarker { get; private set; }
        internal bool Apply { get; private set; }
        internal int GritCost { get; private set; }
        internal int BleedAmount { get; private set; }
        internal string Reason { get; private set; }
    }
}
