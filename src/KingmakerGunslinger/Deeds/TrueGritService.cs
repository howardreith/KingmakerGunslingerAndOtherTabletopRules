using System;

namespace KingmakerGunslinger.Deeds
{
    internal sealed class TrueGritService
    {
        internal TrueGritDecision Evaluate(TrueGritRequest request)
        {
            if (request == null) throw new ArgumentNullException("request");
            if (request.PositiveGritNoSpend)
            {
                bool requiresPositive = !request.Selected;
                return new TrueGritDecision(!requiresPositive ||
                    request.CurrentGrit > 0, 0, requiresPositive);
            }

            int cost = request.Selected ? Math.Max(0,
                request.OrdinaryCost - 1) : request.OrdinaryCost;
            bool reducedToZero = request.Selected && request.OrdinaryCost > 0 &&
                cost == 0;
            bool available = request.CurrentGrit >= cost &&
                (!reducedToZero || request.CurrentGrit > 0);
            return new TrueGritDecision(available, cost, reducedToZero);
        }
    }
}
