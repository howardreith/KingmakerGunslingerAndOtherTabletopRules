using System;

namespace KingmakerGunslinger.BrownFur
{
    internal enum BrownFurShareTargetRelationship
    {
        Unknown = 0,
        Self = 1,
        PartyMember = 2,
        ControlledCompanion = 3,
        AnimalCompanion = 4,
        ControlledSummon = 5,
        FriendlyUnattackable = 6,
        Enemy = 7,
        HostileNeutral = 8,
        FriendlyAttackable = 9,
        Ambiguous = 10
    }

    internal enum BrownFurShareDelivery
    {
        None = 0,
        Touch = 1,
        ThirtyFeet = 2
    }

    internal sealed class BrownFurShareTargetRequest
    {
        internal bool IsValid { get; set; }
        internal bool IsCreature { get; set; }
        internal bool IsAlive { get; set; }
        internal BrownFurShareTargetRelationship Relationship { get; set; }
        internal bool HasThirtyFootCapstone { get; set; }
        internal double DistanceFeet { get; set; }
    }

    internal sealed class BrownFurShareTargetDecision
    {
        internal BrownFurShareTargetDecision(bool eligible, string failure,
            BrownFurShareDelivery delivery)
        {
            Eligible = eligible;
            Failure = failure ?? string.Empty;
            Delivery = delivery;
        }

        internal bool Eligible { get; private set; }
        internal string Failure { get; private set; }
        internal BrownFurShareDelivery Delivery { get; private set; }
    }

    internal static class BrownFurShareTargetPolicy
    {
        private const double ThirtyFootTolerance = 0.0001d;

        internal static BrownFurShareTargetDecision Decide(
            BrownFurShareTargetRequest request)
        { return Decide(request, request != null &&
            request.HasThirtyFootCapstone); }

        internal static BrownFurShareTargetDecision Decide(
            BrownFurShareTargetRequest request, bool hasThirtyFootCapstone)
        {
            if (request == null || !request.IsValid)
                return Reject("share-target-invalid");
            if (!request.IsCreature)
                return Reject("share-target-not-creature");
            if (!request.IsAlive)
                return Reject("share-target-dead");
            if (double.IsNaN(request.DistanceFeet) ||
                double.IsInfinity(request.DistanceFeet) ||
                request.DistanceFeet < 0d)
                return Reject("share-target-distance-invalid");
            if (!IsWilling(request.Relationship))
                return Reject("share-target-unwilling");
            if (hasThirtyFootCapstone &&
                request.DistanceFeet > 30d + ThirtyFootTolerance)
                return Reject("share-target-out-of-range");
            return new BrownFurShareTargetDecision(true, string.Empty,
                hasThirtyFootCapstone ?
                    BrownFurShareDelivery.ThirtyFeet :
                    BrownFurShareDelivery.Touch);
        }

        internal static bool IsWilling(
            BrownFurShareTargetRelationship relationship)
        {
            return relationship == BrownFurShareTargetRelationship.Self ||
                relationship == BrownFurShareTargetRelationship.PartyMember ||
                relationship == BrownFurShareTargetRelationship.ControlledCompanion ||
                relationship == BrownFurShareTargetRelationship.AnimalCompanion ||
                relationship == BrownFurShareTargetRelationship.ControlledSummon ||
                relationship == BrownFurShareTargetRelationship.FriendlyUnattackable;
        }

        private static BrownFurShareTargetDecision Reject(string failure)
        { return new BrownFurShareTargetDecision(false, failure,
            BrownFurShareDelivery.None); }
    }
}
