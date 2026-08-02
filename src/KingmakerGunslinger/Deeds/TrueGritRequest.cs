using System;

namespace KingmakerGunslinger.Deeds
{
    internal sealed class TrueGritRequest
    {
        internal TrueGritRequest(int currentGrit, int ordinaryCost,
            bool selected, bool positiveGritNoSpend)
        {
            if (currentGrit < 0) throw new ArgumentOutOfRangeException("currentGrit");
            if (ordinaryCost < 0) throw new ArgumentOutOfRangeException("ordinaryCost");
            if (positiveGritNoSpend && ordinaryCost != 0)
                throw new ArgumentException(
                    "A positive-grit/no-spend gate must have zero ordinary cost.");
            CurrentGrit = currentGrit;
            OrdinaryCost = ordinaryCost;
            Selected = selected;
            PositiveGritNoSpend = positiveGritNoSpend;
        }

        internal int CurrentGrit { get; private set; }
        internal int OrdinaryCost { get; private set; }
        internal bool Selected { get; private set; }
        internal bool PositiveGritNoSpend { get; private set; }
    }
}
