using System;
using System.Collections.Generic;
using KingmakerGunslinger.BrownFur;

namespace KingmakerGunslinger.DomainTests
{
    internal static class BrownFurCastTests
    {
        internal static void PowerfulChangeSupportsEachAbilityScore()
        {
            foreach (BrownFurAbilityScore score in new[] {
                BrownFurAbilityScore.Strength, BrownFurAbilityScore.Dexterity,
                BrownFurAbilityScore.Constitution, BrownFurAbilityScore.Intelligence,
                BrownFurAbilityScore.Wisdom, BrownFurAbilityScore.Charisma })
            {
                BrownFurCastRequest request = Valid();
                request.PowerfulChangeRequested = true;
                request.SelectedAbilityScore = score;
                request.PositiveAbilityBonuses = new HashSet<BrownFurAbilityScore>
                    { score };
                BrownFurCastDecision decision = BrownFurCastPolicy.Decide(request);
                Assertions.True(decision.Eligible && decision.PowerfulChange &&
                    decision.ReservoirCost == 1,
                    "Powerful Change rejected " + score + ".");
            }
        }

        internal static void PowerfulChangeRejectsInvalidStatWithoutCost()
        {
            BrownFurCastRequest request = Valid();
            request.PowerfulChangeRequested = true;
            request.SelectedAbilityScore = BrownFurAbilityScore.Wisdom;
            request.PositiveAbilityBonuses = new HashSet<BrownFurAbilityScore>
                { BrownFurAbilityScore.Strength, BrownFurAbilityScore.Dexterity };
            BrownFurCastDecision decision = BrownFurCastPolicy.Decide(request);
            Assertions.True(!decision.Eligible && decision.ReservoirCost == 0 &&
                decision.Failure == "powerful-stat-not-granted",
                "A non-granted selected stat must reject before debit.");
        }

        internal static void PowerfulChangeRequiresArcanistSpellSlot()
        {
            BrownFurCastRequest request = Valid();
            request.PowerfulChangeRequested = true;
            request.SelectedAbilityScore = BrownFurAbilityScore.Strength;
            request.PositiveAbilityBonuses = new HashSet<BrownFurAbilityScore>
                { BrownFurAbilityScore.Strength };
            request.UsesArcanistSpellSlot = false;
            Assertions.Equal("powerful-not-arcanist-slot",
                BrownFurCastPolicy.Decide(request).Failure,
                "Another real spellbook must not qualify for Powerful Change.");
            request.UsesArcanistSpellSlot = true;
            request.SourceKind = BrownFurCastSourceKind.Item;
            Assertions.Equal("not-genuine-spell",
                BrownFurCastPolicy.Decide(request).Failure,
                "Items and non-spell activations must not qualify.");
        }

        internal static void ShareTransmutationUsesWillingCreatureContract()
        {
            BrownFurCastRequest request = Valid();
            request.ShareTransmutationRequested = true;
            request.UsesArcanistSpellSlot = false;
            BrownFurCastDecision decision = BrownFurCastPolicy.Decide(request);
            Assertions.True(decision.Eligible && decision.ShareTransmutation &&
                decision.ReservoirCost == 1 &&
                decision.ShareDelivery == BrownFurShareDelivery.Touch,
                "A genuine non-Arcanist spellbook must qualify for Share Transmutation.");
            request.ShareTarget.Relationship =
                BrownFurShareTargetRelationship.Enemy;
            Assertions.Equal("share-target-unwilling",
                BrownFurCastPolicy.Decide(request).Failure,
                "An unwilling target must be rejected.");
            request.ShareTarget.Relationship =
                BrownFurShareTargetRelationship.PartyMember;
            request.ShareTarget.IsCreature = false;
            Assertions.Equal("share-target-not-creature",
                BrownFurCastPolicy.Decide(request).Failure,
                "An object target must be rejected.");
            request.ShareTarget.IsCreature = true;
            request.HasShareThirtyFootCapstone = true;
            request.ShareTarget.DistanceFeet = 30.01d;
            Assertions.Equal("share-target-out-of-range",
                BrownFurCastPolicy.Decide(request).Failure,
                "A target beyond Touch delivery or the exact 30-foot cap must reject.");
        }

        internal static void ShareWillingnessAndDeliveryAreExact()
        {
            foreach (BrownFurShareTargetRelationship relationship in new[] {
                BrownFurShareTargetRelationship.Self,
                BrownFurShareTargetRelationship.PartyMember,
                BrownFurShareTargetRelationship.ControlledCompanion,
                BrownFurShareTargetRelationship.AnimalCompanion,
                BrownFurShareTargetRelationship.ControlledSummon,
                BrownFurShareTargetRelationship.FriendlyUnattackable })
            {
                BrownFurCastRequest request = Valid();
                request.ShareTransmutationRequested = true;
                request.ShareTarget.Relationship = relationship;
                Assertions.True(BrownFurCastPolicy.Decide(request).Eligible,
                    "Authorized willing relationship rejected: " + relationship);
            }
            foreach (BrownFurShareTargetRelationship relationship in new[] {
                BrownFurShareTargetRelationship.Unknown,
                BrownFurShareTargetRelationship.Enemy,
                BrownFurShareTargetRelationship.HostileNeutral,
                BrownFurShareTargetRelationship.FriendlyAttackable,
                BrownFurShareTargetRelationship.Ambiguous })
            {
                BrownFurCastRequest request = Valid();
                request.ShareTransmutationRequested = true;
                request.ShareTarget.Relationship = relationship;
                Assertions.Equal("share-target-unwilling",
                    BrownFurCastPolicy.Decide(request).Failure,
                    "Ambiguous or attackable relationship must reject: " +
                    relationship);
            }

            BrownFurCastRequest boundary = Valid();
            boundary.ShareTransmutationRequested = true;
            boundary.HasShareThirtyFootCapstone = true;
            boundary.ShareTarget.DistanceFeet = 30d;
            BrownFurCastDecision decision = BrownFurCastPolicy.Decide(boundary);
            Assertions.True(decision.Eligible && decision.ShareDelivery ==
                BrownFurShareDelivery.ThirtyFeet,
                "The capstone must accept exactly 30 feet as a fixed range.");
            boundary.ShareTarget.DistanceFeet = 30.001d;
            Assertions.Equal("share-target-out-of-range",
                BrownFurCastPolicy.Decide(boundary).Failure,
                "The capstone must reject targets beyond exactly 30 feet.");
        }

        internal static void PowerfulChangeIncreaseUsesCapstoneValue()
        {
            BrownFurCastRequest request = Valid();
            request.PowerfulChangeRequested = true;
            request.SelectedAbilityScore = BrownFurAbilityScore.Charisma;
            request.PositiveAbilityBonuses = new HashSet<BrownFurAbilityScore>
                { BrownFurAbilityScore.Charisma };
            Assertions.Equal(2, BrownFurCastPolicy.Decide(request)
                .PowerfulChangeIncrease,
                "Powerful Change must add two before the capstone.");
            request.HasPowerfulChangeCapstone = true;
            Assertions.Equal(4, BrownFurCastPolicy.Decide(request)
                .PowerfulChangeIncrease,
                "Powerful Change must add four at the capstone.");
        }

        internal static void CombinedUseCostsExactlyTwo()
        {
            BrownFurCastRequest request = Valid();
            request.PowerfulChangeRequested = true;
            request.SelectedAbilityScore = BrownFurAbilityScore.Constitution;
            request.PositiveAbilityBonuses = new HashSet<BrownFurAbilityScore>
                { BrownFurAbilityScore.Constitution };
            request.ShareTransmutationRequested = true;
            BrownFurCastDecision decision = BrownFurCastPolicy.Decide(request);
            Assertions.True(decision.Eligible && decision.ReservoirCost == 2 &&
                decision.PowerfulChange && decision.ShareTransmutation,
                "Combined use must apply each modification once for exactly two points.");
            request.ReservoirPoints = 1;
            decision = BrownFurCastPolicy.Decide(request);
            Assertions.True(!decision.Eligible && decision.ReservoirCost == 0 &&
                decision.Failure == "reservoir-insufficient",
                "Insufficient combined resources must reject before any debit.");
        }

        internal static void SupremacyExtendsOnlyEligibleDuration()
        {
            BrownFurCastRequest request = Valid();
            BrownFurCastDecision decision = BrownFurCastPolicy.Decide(request);
            Assertions.True(decision.Eligible && decision.TransmutationSupremacy,
                "An ordinary timed Transmutation must receive free Extend.");
            request.AlreadyExtended = true;
            Assertions.False(BrownFurCastPolicy.Decide(request)
                .TransmutationSupremacy,
                "An already Extended spell must not be Extended twice.");
            request.AlreadyExtended = false;
            request.DurationKind = BrownFurDurationKind.Instantaneous;
            Assertions.False(BrownFurCastPolicy.Decide(request)
                .TransmutationSupremacy,
                "An instantaneous spell must remain unchanged.");
            request.DurationKind = BrownFurDurationKind.Permanent;
            Assertions.False(BrownFurCastPolicy.Decide(request)
                .TransmutationSupremacy,
                "A permanent spell must remain unchanged.");
        }

        internal static void TransactionDebitsExactlyOnce()
        {
            BrownFurCastDecision decision = new BrownFurCastDecision(true,
                string.Empty, 2, true, true, true, 2,
                BrownFurShareDelivery.Touch);
            BrownFurCastTransaction transaction = new BrownFurCastTransaction(
                Intent(2));
            int calls = 0;
            int points = 3;
            Assertions.True(transaction.Validate(decision),
                "A valid transaction must validate once.");
            Assertions.True(transaction.Commit(cost => {
                calls++;
                if (points < cost) return false;
                points -= cost;
                return true;
            }), "A fully funded transaction must commit.");
            Assertions.False(transaction.Commit(cost => { calls++; return true; }),
                "A committed transaction must not debit again.");
            Assertions.True(transaction.Complete() && calls == 1 && points == 1 &&
                transaction.DebitedReservoirPoints == 2,
                "Commit must perform one atomic two-point debit.");
        }

        internal static void CancellationAndInterruptionAreAtomic()
        {
            BrownFurCastDecision decision = new BrownFurCastDecision(true,
                string.Empty, 1, true, false, false, 2,
                BrownFurShareDelivery.None);
            BrownFurCastTransaction cancelled = new BrownFurCastTransaction(Intent(1));
            cancelled.Validate(decision);
            Assertions.True(cancelled.Cancel() &&
                cancelled.DebitedReservoirPoints == 0,
                "Cancellation before commitment must spend nothing.");

            BrownFurCastTransaction interrupted =
                new BrownFurCastTransaction(Intent(1));
            interrupted.Validate(decision);
            int calls = 0;
            interrupted.Commit(cost => { calls++; return cost == 1; });
            Assertions.True(interrupted.Interrupt() && calls == 1 &&
                interrupted.DebitedReservoirPoints == 1 &&
                interrupted.State == BrownFurCastTransactionState.Interrupted,
                "Interruption after commitment must retain one, nonduplicated debit.");
        }

        private static BrownFurCastRequest Valid()
        {
            return new BrownFurCastRequest {
                CasterOwnsBrownFur = true,
                IsGenuineSpell = true,
                IsTransmutation = true,
                SourceKind = BrownFurCastSourceKind.Spellbook,
                UsesArcanistSpellSlot = true,
                HasPowerfulChange = true,
                HasShareTransmutation = true,
                HasTransmutationSupremacy = true,
                PositiveAbilityBonuses = new HashSet<BrownFurAbilityScore>(),
                BonusAdapterAvailable = true,
                OriginalRange = BrownFurOriginalRange.Personal,
                ShareTarget = new BrownFurShareTargetRequest {
                    IsValid = true,
                    IsCreature = true,
                    IsAlive = true,
                    Relationship = BrownFurShareTargetRelationship.PartyMember,
                    DistanceFeet = 5d
                },
                TargetAdapterAvailable = true,
                DurationKind = BrownFurDurationKind.Timed,
                DurationAdapterAvailable = true,
                ReservoirPoints = 2
            };
        }

        private static BrownFurCastIntent Intent(int cost)
        {
            return new BrownFurCastIntent("tx-1", "caster", "canonical",
                "variant", "book", "target", cost > 0,
                BrownFurAbilityScore.Strength, cost > 1, true, cost,
                "target-adapter", "bonus-adapter", "duration-adapter");
        }
    }
}
