namespace KingmakerGunslinger.Deeds
{
    internal sealed class PistolWhipDecision
    {
        internal PistolWhipDecision(PistolWhipStatus status, bool twoHanded,
            int damageDieSides, int gritCost)
        {
            Status = status;
            TwoHanded = twoHanded;
            DamageDieSides = damageDieSides;
            GritCost = gritCost;
        }

        internal PistolWhipStatus Status { get; private set; }
        internal bool TwoHanded { get; private set; }
        internal int DamageDieSides { get; private set; }
        internal int GritCost { get; private set; }
        internal bool ShouldAttack { get { return Status == PistolWhipStatus.Eligible; } }
    }
}
