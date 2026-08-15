using System;
using System.Collections.Generic;
using System.Linq;
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

        internal static void ModifierAdjustmentPreservesDescriptor()
        {
            BrownFurModifierAdjustmentRequest request = ModifierRequest();
            request.OriginalValue = 4;
            request.Increase = 2;
            request.OriginalDescriptor = "Enhancement";
            BrownFurModifierAdjustmentDecision decision =
                BrownFurModifierAdjustmentPolicy.Decide(request);
            Assertions.True(decision.Eligible && decision.AdjustedValue == 6 &&
                decision.RetainedDescriptor == "Enhancement",
                "Powerful Change must enhance the original modifier value while retaining its descriptor.");
            request.Increase = 4;
            decision = BrownFurModifierAdjustmentPolicy.Decide(request);
            Assertions.True(decision.Eligible && decision.AdjustedValue == 8 &&
                decision.RetainedDescriptor == "Enhancement",
                "The capstone must apply +4 to the same descriptor-bearing modifier.");
        }

        internal static void ModifierAdjustmentFailsClosed()
        {
            BrownFurModifierAdjustmentRequest request = ModifierRequest();
            request.ModifierAbilityScore = BrownFurAbilityScore.Dexterity;
            Assertions.Equal("modifier-stat-not-selected-stat",
                BrownFurModifierAdjustmentPolicy.Decide(request).Failure,
                "A different ability score must not be empowered.");
            request = ModifierRequest();
            request.OriginalValue = 0;
            Assertions.Equal("modifier-not-positive-bonus",
                BrownFurModifierAdjustmentPolicy.Decide(request).Failure,
                "A penalty or zero modifier must remain unchanged.");
            request = ModifierRequest();
            request.CarrierFamily = "UnknownFutureCarrier";
            Assertions.Equal("modifier-carrier-unsupported",
                BrownFurModifierAdjustmentPolicy.Decide(request).Failure,
                "An unknown carrier must fail closed.");
            request = ModifierRequest();
            request.SourceFact = new object();
            Assertions.Equal("modifier-source-fact-mismatch",
                BrownFurModifierAdjustmentPolicy.Decide(request).Failure,
                "A foreign source fact must remain unchanged.");
            request = ModifierRequest();
            request.SourceContext = new object();
            Assertions.Equal("modifier-source-context-mismatch",
                BrownFurModifierAdjustmentPolicy.Decide(request).Failure,
                "A foreign cast context must remain unchanged.");
            request = ModifierRequest();
            request.OriginalValue = int.MaxValue;
            Assertions.Equal("modifier-value-overflow",
                BrownFurModifierAdjustmentPolicy.Decide(request).Failure,
                "Overflow must reject instead of wrapping the bonus.");
        }

        internal static void ModifierAdjustmentIsExactlyOnce()
        {
            var tracker = new BrownFurModifierAdjustmentTracker<object>();
            object modifier = new object();
            BrownFurModifierAdjustmentDecision decision;
            Assertions.True(tracker.TryAdjust("tx-1", modifier,
                ModifierRequest(), out decision) && decision.AdjustedValue == 6,
                "The first matching modifier must adjust.");
            Assertions.False(tracker.TryAdjust("tx-1", modifier,
                ModifierRequest(), out decision),
                "The same modifier must not adjust twice in one cast.");
            Assertions.Equal("modifier-already-adjusted", decision.Failure,
                "Duplicate adjustment must report its exact failure.");
            Assertions.True(tracker.AdjustedModifierCount("tx-1") == 1 &&
                tracker.Release("tx-1") && tracker.ActiveTransactionCount == 0,
                "Execution cleanup must release the exact transaction state.");
        }

        internal static void ModifierTransactionsAreIsolated()
        {
            var tracker = new BrownFurModifierAdjustmentTracker<object>();
            object first = new object();
            object second = new object();
            BrownFurModifierAdjustmentDecision decision;
            Assertions.True(tracker.TryAdjust("tx-a", first, ModifierRequest(),
                out decision), "The first queued cast must adjust its modifier.");
            Assertions.True(tracker.TryAdjust("tx-b", second, ModifierRequest(),
                out decision), "The second queued cast must adjust independently.");
            Assertions.True(tracker.ActiveTransactionCount == 2 &&
                tracker.AdjustedModifierCount("tx-a") == 1 &&
                tracker.AdjustedModifierCount("tx-b") == 1,
                "Concurrent cast state must remain transaction-local.");
            tracker.Release("tx-a");
            Assertions.True(tracker.ActiveTransactionCount == 1 &&
                tracker.AdjustedModifierCount("tx-b") == 1,
                "Cleaning one cast must not release another cast's modifier state.");
            tracker.Clear();
            Assertions.Equal(0, tracker.ActiveTransactionCount,
                "Load or combat transition cleanup must clear all retained state.");
        }

        internal static void ShareTargetingScopeIsExact()
        {
            var tracker = new BrownFurShareTargetingScopeTracker<object, object,
                object>();
            object ability = new object();
            object caster = new object();
            object target = new object();
            object otherTarget = new object();
            bool allowed;
            BrownFurShareDelivery delivery;
            Assertions.True(tracker.Begin("share-1", ability, caster, target,
                BrownFurShareDelivery.Touch) &&
                tracker.TryResolveAnchor(ability) &&
                tracker.TryResolveTarget(ability, caster, target, out allowed) &&
                allowed && tracker.TryGetDelivery(ability, caster, target,
                    out delivery) && delivery == BrownFurShareDelivery.Touch,
                "A validated Share scope must resolve only its exact cast identity.");
            Assertions.True(tracker.TryResolveTarget(ability, caster,
                otherTarget, out allowed) && !allowed,
                "The matching ability scope must explicitly reject a different target.");
            Assertions.False(tracker.TryResolveTarget(new object(), caster,
                target, out allowed),
                "An unrelated ability must retain the native or CotW result.");
            Assertions.False(tracker.Begin("share-2", ability, caster, target,
                BrownFurShareDelivery.ThirtyFeet),
                "One AbilityData identity must not own simultaneous scopes.");
            Assertions.True(tracker.Release("share-1") &&
                !tracker.TryResolveAnchor(ability) && tracker.ActiveScopeCount == 0,
                "Releasing the transaction must remove its targeting override.");
        }

        internal static void ShareTargetingScopesAreIsolated()
        {
            var tracker = new BrownFurShareTargetingScopeTracker<object, object,
                object>();
            object firstAbility = new object();
            object secondAbility = new object();
            object firstCaster = new object();
            object secondCaster = new object();
            object firstTarget = new object();
            object secondTarget = new object();
            BrownFurShareDelivery delivery;
            Assertions.True(tracker.Begin("share-a", firstAbility, firstCaster,
                firstTarget, BrownFurShareDelivery.Touch) &&
                tracker.Begin("share-b", secondAbility, secondCaster,
                    secondTarget, BrownFurShareDelivery.ThirtyFeet) &&
                tracker.ActiveScopeCount == 2,
                "Queued Share casts must retain separate reference identities.");
            tracker.Release("share-a");
            Assertions.True(!tracker.TryResolveAnchor(firstAbility) &&
                tracker.TryGetDelivery(secondAbility, secondCaster, secondTarget,
                    out delivery) &&
                delivery == BrownFurShareDelivery.ThirtyFeet,
                "Cleaning one Share cast must not release another cast's delivery state.");
            tracker.Clear();
            Assertions.Equal(0, tracker.ActiveScopeCount,
                "Load or combat cleanup must clear all Share targeting scopes.");
        }

        internal static void StaticBonusAdapterPlanIsExact()
        {
            BrownFurBonusAdapterPlan plan = BrownFurBonusAdapterPlanPolicy.Create(
                new[] { "path=Kingmaker.UnitLogic.FactLogic.AddStatBonus{Descriptor=Enhancement,ScaleByBasicAttackBonus=False,Stat=Strength,Value=4}" },
                new[] { "path=b175001b42b1a02479881b72fe132116/BullsStrengthBuff" });
            Assertions.True(plan.Status ==
                BrownFurBonusAdapterPlanStatus.Supported &&
                plan.Supports(BrownFurAbilityScore.Strength) &&
                !plan.Supports(BrownFurAbilityScore.Dexterity) &&
                plan.AppliedBuffGuids.SequenceEqual(new[] {
                    "b175001b42b1a02479881b72fe132116" }) &&
                plan.CarrierFamilies.SequenceEqual(new[] { "AddStatBonus" }),
                "A static descriptor-bearing bonus must produce one exact stat and buff plan.");
        }

        internal static void PolymorphBonusAdapterPlanIsExact()
        {
            BrownFurBonusAdapterPlan plan = BrownFurBonusAdapterPlanPolicy.Create(
                new[] { "path=Kingmaker.UnitLogic.Buffs.Polymorph{ConstitutionBonus=2,DexterityBonus=4,Size=Large,StrengthBonus=6}" },
                new[] { "path=00d8fbe9cf61dc24298be8d95500c84b/BeastShapeIBuff" });
            Assertions.True(plan.Status ==
                BrownFurBonusAdapterPlanStatus.Supported &&
                plan.AbilityScores.SequenceEqual(new[] {
                    BrownFurAbilityScore.Strength,
                    BrownFurAbilityScore.Dexterity,
                    BrownFurAbilityScore.Constitution }) &&
                plan.Supports(BrownFurAbilityScore.Dexterity),
                "A multi-stat polymorph must expose every and only positive selectable stat.");
        }

        internal static void SizeBonusAdapterPlanIsExact()
        {
            BrownFurBonusAdapterPlan plan = BrownFurBonusAdapterPlanPolicy.Create(
                new[] {
                    "path=Kingmaker.Designers.Mechanics.Buffs.ChangeUnitSize{Size=Fine,SizeDelta=1}",
                    "path=Kingmaker.UnitLogic.Buffs.Components.AddGenericStatBonus{Descriptor=Size,Stat=Strength,Value=2}"
                },
                new[] { "path=4f139d125bb602f48bfaec3d3e1937cb/EnlargePersonBuff" });
            Assertions.True(plan.Status ==
                BrownFurBonusAdapterPlanStatus.Supported &&
                plan.AbilityScores.SequenceEqual(new[] {
                    BrownFurAbilityScore.Strength }) &&
                plan.CarrierFamilies.SequenceEqual(new[] {
                    "AddGenericStatBonus", "ChangeUnitSize" }),
                "Size change must be auxiliary while the original Size descriptor stat modifier is empowered.");
        }

        internal static void BonusAdapterPlanFailsClosed()
        {
            BrownFurBonusAdapterPlan plan = BrownFurBonusAdapterPlanPolicy.Create(
                new string[0], new string[0]);
            Assertions.True(plan.Status ==
                BrownFurBonusAdapterPlanStatus.Ineligible &&
                plan.Failure == "no-positive-ability-bonus-carrier",
                "A spell without a positive carrier must be intentionally ineligible.");
            plan = BrownFurBonusAdapterPlanPolicy.Create(new[] {
                "path=Future.UnknownCarrier{Stat=Strength,Value=4}" },
                new[] { "path=b175001b42b1a02479881b72fe132116/Buff" });
            Assertions.True(plan.Status == BrownFurBonusAdapterPlanStatus.Blocked &&
                plan.Failure == "bonus-carrier-unsupported",
                "An unknown future carrier must block instead of guessing.");
            plan = BrownFurBonusAdapterPlanPolicy.Create(new[] {
                "malformed" }, new[] {
                "path=b175001b42b1a02479881b72fe132116/Buff" });
            Assertions.Equal("bonus-carrier-malformed", plan.Failure,
                "Malformed inventory carrier evidence must block.");
            plan = BrownFurBonusAdapterPlanPolicy.Create(new[] {
                "path=Kingmaker.UnitLogic.Buffs.Polymorph{StrengthBonus=not-a-number}" },
                new[] { "path=b175001b42b1a02479881b72fe132116/Buff" });
            Assertions.Equal("bonus-carrier-fields-invalid", plan.Failure,
                "Malformed recognized-carrier fields must block.");
            plan = BrownFurBonusAdapterPlanPolicy.Create(new[] {
                "path=Kingmaker.UnitLogic.FactLogic.AddStatBonus{Stat=Strength,Value=4}" },
                new[] { "path=NOT-A-GUID/Buff" });
            Assertions.Equal("bonus-applied-buff-malformed", plan.Failure,
                "Malformed source-buff identity must block before casting.");
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

        private static BrownFurModifierAdjustmentRequest ModifierRequest()
        {
            object fact = new object();
            object context = new object();
            return new BrownFurModifierAdjustmentRequest {
                ExecutionCommitted = true,
                SelectedAbilityScore = BrownFurAbilityScore.Strength,
                ModifierAbilityScore = BrownFurAbilityScore.Strength,
                OriginalValue = 4,
                Increase = 2,
                OriginalDescriptor = "Enhancement",
                CarrierFamily = "AddStatBonus",
                SourceFact = fact,
                ExpectedSourceFact = fact,
                SourceContext = context,
                ExpectedSourceContext = context
            };
        }
    }
}
