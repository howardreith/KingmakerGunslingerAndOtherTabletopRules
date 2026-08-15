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
                if (!request.TargetIsCreature) return Reject("share-target-not-creature");
                if (!request.TargetIsWilling) return Reject("share-target-unwilling");
                if (!request.TargetWithinShareRange)
                    return Reject("share-target-out-of-range");
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
            return new BrownFurCastDecision(true, string.Empty, cost,
                request.PowerfulChangeRequested,
                request.ShareTransmutationRequested, supremacy);
        }

        private static BrownFurCastDecision Reject(string failure)
        { return new BrownFurCastDecision(false, failure, 0, false, false, false); }

        private static BrownFurCastDecision Unmodified()
        { return new BrownFurCastDecision(true, string.Empty, 0, false, false, false); }
    }
}
