namespace KingmakerGunslinger.Deeds
{
    internal sealed class TrueGritDecision
    {
        internal TrueGritDecision(bool available, int effectiveCost,
            bool requiresPositiveGrit)
        {
            Available = available;
            EffectiveCost = effectiveCost;
            RequiresPositiveGrit = requiresPositiveGrit;
        }

        internal bool Available { get; private set; }
        internal int EffectiveCost { get; private set; }
        internal bool RequiresPositiveGrit { get; private set; }
    }
}
