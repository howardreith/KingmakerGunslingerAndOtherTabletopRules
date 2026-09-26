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

        /// <summary>
        /// The cost a native resource check (HasEnoughResource) must see. The
        /// native check only compares a cost with the pool, so a reduced cost
        /// that is not available (0 while grit is 0) reports at least one
        /// grit, which that check then refuses; an available cost is spent as
        /// is (0 with True Grit while grit is above 0).
        /// </summary>
        internal int NativeCheckCost
        {
            get { return Available ? EffectiveCost : System.Math.Max(1, EffectiveCost); }
        }
    }
}
