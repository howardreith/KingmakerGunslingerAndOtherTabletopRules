using System;

namespace KingmakerGunslinger.BrownFur
{
    internal static class BrownFurCastPolicy
    {
        internal static BrownFurCastDecision Decide(BrownFurCastRequest request)
        {
            if (request == null) return Reject("request-missing");
            bool requested = request.PowerfulChangeRequested ||
                request.ShareTransmutationRequested;
            if (!request.CasterOwnsBrownFur)
                return requested ? Reject("caster-not-brown-fur") : Unmodified();
            bool genuine = request.IsGenuineSpell &&
                request.SourceKind == BrownFurCastSourceKind.Spellbook;
            if (!genuine)
                return requested ? Reject("not-genuine-spell") : Unmodified();
            if (!request.IsTransmutation)
                return requested ? Reject("not-transmutation") : Unmodified();

            if (request.PowerfulChangeRequested)
            {
                if (!request.HasPowerfulChange) return Reject("powerful-not-owned");
                if (!request.UsesArcanistSpellSlot)
                    return Reject("powerful-not-arcanist-slot");
                if (request.SelectedAbilityScore == BrownFurAbilityScore.None)
                    return Reject("powerful-stat-not-selected");
                if (request.PositiveAbilityBonuses == null ||
                    !request.PositiveAbilityBonuses.Contains(
                        request.SelectedAbilityScore))
                    return Reject("powerful-stat-not-granted");
                if (!request.BonusAdapterAvailable)
                    return Reject("powerful-adapter-unavailable");
            }

            if (request.ShareTransmutationRequested)
            {
                if (!request.HasShareTransmutation) return Reject("share-not-owned");
                if (request.OriginalRange != BrownFurOriginalRange.Personal)
                    return Reject("share-not-personal");
                BrownFurShareTargetDecision target =
                    BrownFurShareTargetPolicy.Decide(request.ShareTarget,
                        request.HasShareThirtyFootCapstone);
                if (!target.Eligible) return Reject(target.Failure);
                if (!request.TargetAdapterAvailable)
                    return Reject("share-adapter-unavailable");
            }

            int cost = (request.PowerfulChangeRequested ? 1 : 0) +
                (request.ShareTransmutationRequested ? 1 : 0);
            if (request.ReservoirPoints < cost)
                return Reject("reservoir-insufficient");
            bool supremacy = request.HasTransmutationSupremacy &&
                request.DurationKind == BrownFurDurationKind.Timed &&
                !request.AlreadyExtended && request.DurationAdapterAvailable;
            BrownFurShareDelivery delivery = BrownFurShareDelivery.None;
            if (request.ShareTransmutationRequested)
                delivery = BrownFurShareTargetPolicy.Decide(
                    request.ShareTarget,
                    request.HasShareThirtyFootCapstone).Delivery;
            return new BrownFurCastDecision(true, string.Empty, cost,
                request.PowerfulChangeRequested,
                request.ShareTransmutationRequested, supremacy,
                request.PowerfulChangeRequested ?
                    (request.HasPowerfulChangeCapstone ? 4 : 2) : 0,
                delivery);
        }

        private static BrownFurCastDecision Reject(string failure)
        { return new BrownFurCastDecision(false, failure, 0, false, false, false,
            0, BrownFurShareDelivery.None); }

        private static BrownFurCastDecision Unmodified()
        { return new BrownFurCastDecision(true, string.Empty, 0, false, false, false,
            0, BrownFurShareDelivery.None); }
    }
}
