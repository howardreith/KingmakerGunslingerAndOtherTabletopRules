using System;

namespace KingmakerGunslinger.BrownFur
{
    internal static class BrownFurCastPolicy
    {
        internal static BrownFurCastDecision Decide(BrownFurCastRequest request)
        {
            if (request == null) return Reject("request-missing");
            if (!request.CasterOwnsBrownFur)
                return Unmodified();
            bool genuine = request.IsGenuineSpell &&
                request.SourceKind == BrownFurCastSourceKind.Spellbook;
            if (!genuine)
                return Unmodified();
            if (!request.IsTransmutation)
                return Unmodified();

            bool powerful = request.PowerfulChangeRequested &&
                request.HasPowerfulChange && request.UsesArcanistSpellSlot &&
                request.SelectedAbilityScore != BrownFurAbilityScore.None &&
                request.PositiveAbilityBonuses != null &&
                request.PositiveAbilityBonuses.Contains(
                    request.SelectedAbilityScore) &&
                request.BonusAdapterAvailable;

            bool share = false;
            BrownFurShareDelivery delivery = BrownFurShareDelivery.None;
            if (request.ShareTransmutationRequested &&
                request.HasShareTransmutation &&
                request.OriginalRange == BrownFurOriginalRange.Personal &&
                request.TargetAdapterAvailable)
            {
                BrownFurShareTargetDecision target =
                    BrownFurShareTargetPolicy.Decide(request.ShareTarget,
                        request.HasShareThirtyFootCapstone);
                if (!target.Eligible)
                    return Reject(target.Failure);
                share = true;
                delivery = target.Delivery;
            }

            int cost = (powerful ? 1 : 0) + (share ? 1 : 0);
            if (request.ReservoirPoints < cost)
                return Reject("reservoir-insufficient");
            bool supremacy = request.HasTransmutationSupremacy &&
                request.DurationKind == BrownFurDurationKind.Timed &&
                !request.AlreadyExtended && request.DurationAdapterAvailable;
            return new BrownFurCastDecision(true, string.Empty, cost,
                powerful, share, supremacy,
                powerful ?
                    (request.HasPowerfulChangeCapstone ? 4 : 2) : 0,
                delivery, powerful ? request.SelectedAbilityScore :
                    BrownFurAbilityScore.None);
        }

        private static BrownFurCastDecision Reject(string failure)
        { return new BrownFurCastDecision(false, failure, 0, false, false, false,
            0, BrownFurShareDelivery.None); }

        private static BrownFurCastDecision Unmodified()
        { return new BrownFurCastDecision(true, string.Empty, 0, false, false, false,
            0, BrownFurShareDelivery.None); }
    }
}
