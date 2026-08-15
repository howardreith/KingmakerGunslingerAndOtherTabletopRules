using System;

namespace KingmakerGunslinger.BrownFur
{
    internal sealed class CotwProgressionDecision
    {
        private CotwProgressionDecision(bool compatible,
            CotwProgressionShape shape, int powerfulChangeReplacementLevel,
            int shareTransmutationReplacementLevel, string reason)
        {
            Compatible = compatible;
            Shape = shape;
            PowerfulChangeReplacementLevel = powerfulChangeReplacementLevel;
            ShareTransmutationReplacementLevel = shareTransmutationReplacementLevel;
            Reason = reason ?? string.Empty;
        }

        internal bool Compatible { get; private set; }
        internal CotwProgressionShape Shape { get; private set; }
        internal int PowerfulChangeReplacementLevel { get; private set; }
        internal int ShareTransmutationReplacementLevel { get; private set; }
        internal string Reason { get; private set; }

        internal static CotwProgressionDecision Accept(
            CotwProgressionShape shape, int powerfulChangeReplacementLevel,
            int shareTransmutationReplacementLevel)
        {
            if (shape == CotwProgressionShape.Unknown)
                throw new ArgumentException("A compatible progression requires a known shape.",
                    "shape");
            return new CotwProgressionDecision(true, shape,
                powerfulChangeReplacementLevel,
                shareTransmutationReplacementLevel, "compatible");
        }

        internal static CotwProgressionDecision Reject(string reason)
        {
            return new CotwProgressionDecision(false, CotwProgressionShape.Unknown,
                0, 0, reason);
        }
    }
}
