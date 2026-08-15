using System;
using System.Collections.Generic;

namespace KingmakerGunslinger.BrownFur
{
    internal enum BrownFurProgressionFeature
    {
        PowerfulChange = 0,
        ShareTransmutation = 1,
        TransmutationSupremacy = 2,
        ArcanistExploit = 3,
        MagicalSupremacy = 4
    }

    internal sealed class BrownFurProgressionPlacement
    {
        internal BrownFurProgressionPlacement(int level,
            BrownFurProgressionFeature feature)
        { Level = level; Feature = feature; }
        internal int Level { get; private set; }
        internal BrownFurProgressionFeature Feature { get; private set; }
    }

    internal sealed class BrownFurArchetypePlan
    {
        private BrownFurArchetypePlan(
            IReadOnlyList<BrownFurProgressionPlacement> additions,
            IReadOnlyList<BrownFurProgressionPlacement> removals)
        { Additions = additions; Removals = removals; }

        internal IReadOnlyList<BrownFurProgressionPlacement> Additions
        { get; private set; }
        internal IReadOnlyList<BrownFurProgressionPlacement> Removals
        { get; private set; }

        internal static BrownFurArchetypePlan Create(
            CotwProgressionDecision decision)
        {
            if (decision == null) throw new ArgumentNullException("decision");
            if (!decision.Compatible) throw new InvalidOperationException(
                "Brown-Fur archetype construction requires a compatible CotW progression.");
            return new BrownFurArchetypePlan(
                new[] {
                    new BrownFurProgressionPlacement(3,
                        BrownFurProgressionFeature.PowerfulChange),
                    new BrownFurProgressionPlacement(9,
                        BrownFurProgressionFeature.ShareTransmutation),
                    new BrownFurProgressionPlacement(20,
                        BrownFurProgressionFeature.TransmutationSupremacy)
                },
                new[] {
                    new BrownFurProgressionPlacement(
                        decision.PowerfulChangeReplacementLevel,
                        BrownFurProgressionFeature.ArcanistExploit),
                    new BrownFurProgressionPlacement(
                        decision.ShareTransmutationReplacementLevel,
                        BrownFurProgressionFeature.ArcanistExploit),
                    new BrownFurProgressionPlacement(20,
                        BrownFurProgressionFeature.MagicalSupremacy)
                });
        }
    }
}
