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

        internal static void CastLifecycleIsExact()
        {
            var tracker = new BrownFurCastLifecycleTracker<object, object,
                object, object, object>();
            object command = new object();
            object ability = new object();
            object rule = new object();
            object context = new object();
            object process = new object();
            BrownFurCastTransaction transaction = Transaction(2);
            int points = 3;
            int debitCalls = 0;
            Assertions.True(tracker.Begin(command, ability, transaction) &&
                tracker.AttachRule(ability, rule, context) &&
                tracker.AttachProcess(rule, process) &&
                tracker.Commit(ability, cost => {
                    debitCalls++;
                    if (points < cost) return false;
                    points -= cost;
                    return true;
                }), "The exact command/rule/context/process chain must commit.");
            BrownFurCastTransaction resolved;
            Assertions.True(tracker.TryGetByContext(context, out resolved) &&
                ReferenceEquals(resolved, transaction) &&
                !tracker.Commit(ability, cost => { debitCalls++; return true; }),
                "The exact context must resolve without a duplicate debit.");
            Assertions.True(tracker.EndCommand(command, false) &&
                tracker.ActiveTransactionCount == 1 &&
                tracker.ProcessTerminal(process, false) &&
                tracker.ActiveTransactionCount == 0 && points == 1 &&
                debitCalls == 1 && transaction.State ==
                    BrownFurCastTransactionState.Completed,
                "Normal command end must retain effects until exact process completion.");
        }

        internal static void CastLifecycleCancellationIsAtomic()
        {
            var tracker = new BrownFurCastLifecycleTracker<object, object,
                object, object, object>();
            BrownFurCastTransaction cancelled = Transaction(1);
            object cancelledCommand = new object();
            Assertions.True(tracker.Begin(cancelledCommand, new object(),
                cancelled) && tracker.EndCommand(cancelledCommand, true) &&
                cancelled.State == BrownFurCastTransactionState.Cancelled &&
                cancelled.DebitedReservoirPoints == 0 &&
                tracker.ActiveTransactionCount == 0,
                "A pre-commit command cancellation must release without debit.");

            BrownFurCastTransaction interrupted = Transaction(1);
            object command = new object();
            object ability = new object();
            object rule = new object();
            object process = new object();
            int calls = 0;
            Assertions.True(tracker.Begin(command, ability, interrupted) &&
                tracker.AttachRule(ability, rule, new object()) &&
                tracker.AttachProcess(rule, process) &&
                tracker.Commit(ability, cost => { calls++; return cost == 1; }) &&
                tracker.EndCommand(command, true) &&
                interrupted.State == BrownFurCastTransactionState.Interrupted &&
                tracker.ActiveTransactionCount == 1 &&
                tracker.ProcessTerminal(process, true) &&
                tracker.ActiveTransactionCount == 0 && calls == 1 &&
                interrupted.DebitedReservoirPoints == 1,
                "A post-commit interruption must retain one debit and release at process end.");
        }

        internal static void CastLifecyclesAreIsolated()
        {
            var tracker = new BrownFurCastLifecycleTracker<object, object,
                object, object, object>();
            object commandA = new object();
            object commandB = new object();
            object abilityA = new object();
            object abilityB = new object();
            object ruleA = new object();
            object ruleB = new object();
            object processA = new object();
            object processB = new object();
            BrownFurCastTransaction first = Transaction(1);
            BrownFurCastTransaction second = Transaction(1);
            Assertions.True(tracker.Begin(commandA, abilityA, first) &&
                tracker.Begin(commandB, abilityB, second) &&
                !tracker.Begin(new object(), abilityA, Transaction(1)) &&
                tracker.AttachRule(abilityA, ruleA, new object()) &&
                tracker.AttachRule(abilityB, ruleB, new object()) &&
                tracker.AttachProcess(ruleA, processA) &&
                tracker.AttachProcess(ruleB, processB) &&
                tracker.Commit(abilityA, cost => true) &&
                tracker.Commit(abilityB, cost => true),
                "Two distinct queued casts must retain isolated identities.");
            tracker.ProcessTerminal(processA, false);
            Assertions.True(tracker.ActiveTransactionCount == 1 &&
                first.State == BrownFurCastTransactionState.Completed &&
                second.State == BrownFurCastTransactionState.Committed,
                "Completing one cast must not release the other.");
            tracker.Clear();
            Assertions.True(tracker.ActiveTransactionCount == 0 &&
                second.State == BrownFurCastTransactionState.Interrupted,
                "Bounded transition cleanup must interrupt and release remaining casts.");
        }

        internal static void ReservoirDebitIsExact()
        {
            int points = 3;
            int spendCalls = 0;
            BrownFurReservoirDebitResult result =
                BrownFurExactDebitPolicy.TryDebitExact(2, () => true,
                    () => points, cost => points >= cost, cost => {
                        spendCalls++;
                        points -= cost;
                    }, amount => points += amount);
            Assertions.True(result.Success && result.Before == 3 &&
                result.ObservedAfterSpend == 1 && result.FinalAmount == 1 &&
                !result.RollbackAttempted && spendCalls == 1 && points == 1,
                "A successful reservoir transaction must debit the total exactly once.");
            result = BrownFurExactDebitPolicy.TryDebitExact(0, null, null,
                null, null, null);
            Assertions.True(result.Success && !result.RollbackAttempted,
                "A zero-cost unmodified cast must not require or touch a resource.");
        }

        internal static void ReservoirDebitRejectsBeforeSpend()
        {
            int points = 1;
            int spendCalls = 0;
            BrownFurReservoirDebitResult result =
                BrownFurExactDebitPolicy.TryDebitExact(2, () => true,
                    () => points, cost => points >= cost,
                    cost => { spendCalls++; points -= cost; },
                    amount => points += amount);
            Assertions.True(!result.Success &&
                result.Failure == "reservoir-insufficient" &&
                spendCalls == 0 && points == 1 && !result.RollbackAttempted,
                "Insufficient reservoir must reject before any partial debit.");
            result = BrownFurExactDebitPolicy.TryDebitExact(1, () => false,
                () => points, cost => true, cost => spendCalls++,
                amount => points += amount);
            Assertions.True(!result.Success &&
                result.Failure == "reservoir-not-owned" && spendCalls == 0,
                "A caster without the exact CotW reservoir must not be debited.");
        }

        internal static void ReservoirDebitRollsBackAnomaly()
        {
            int points = 5;
            BrownFurReservoirDebitResult mismatch =
                BrownFurExactDebitPolicy.TryDebitExact(2, () => true,
                    () => points, cost => true, cost => points -= 1,
                    amount => points += amount);
            Assertions.True(!mismatch.Success &&
                mismatch.Failure == "reservoir-debit-mismatch" &&
                mismatch.ObservedAfterSpend == 4 &&
                mismatch.RollbackAttempted && mismatch.RollbackSucceeded &&
                mismatch.FinalAmount == 5 && points == 5,
                "A non-exact native debit must fail and restore the original amount.");

            points = 5;
            BrownFurReservoirDebitResult exception =
                BrownFurExactDebitPolicy.TryDebitExact(2, () => true,
                    () => points, cost => true, cost => {
                        points -= cost;
                        throw new InvalidOperationException("fixture");
                    }, amount => points += amount);
            Assertions.True(!exception.Success &&
                exception.Failure ==
                    "reservoir-debit-exception:System.InvalidOperationException" &&
                exception.RollbackAttempted && exception.RollbackSucceeded &&
                exception.FinalAmount == 5 && points == 5,
                "An exception after mutation must restore the original amount.");
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

        internal static void CastLifecycleReleasesExactlyOnce()
        {
            var released = new List<BrownFurCastTransaction>();
            var tracker = new BrownFurCastLifecycleTracker<object, object,
                object, object, object>(released.Add);
            object command = new object();
            object ability = new object();
            object rule = new object();
            object context = new object();
            object process = new object();
            BrownFurCastTransaction transaction = Transaction(1);
            Assertions.True(tracker.Begin(command, ability, transaction) &&
                tracker.AttachRule(ability, rule, context) &&
                tracker.AttachProcess(rule, process) &&
                tracker.Commit(ability, value => value == 1) &&
                tracker.ProcessTerminal(process, false),
                "A committed lifecycle must reach one exact terminal release.");
            tracker.EndCommand(command, false);
            Assertions.True(released.Count == 1 &&
                ReferenceEquals(released[0], transaction),
                "Later duplicate terminal signals must not release scopes twice.");

            object cancelledCommand = new object();
            BrownFurCastTransaction cancelled = Transaction(1);
            Assertions.True(tracker.Begin(cancelledCommand, new object(),
                cancelled) && tracker.EndCommand(cancelledCommand, true),
                "An uncommitted cancellation must release its lifecycle.");
            Assertions.True(released.Count == 2 &&
                ReferenceEquals(released[1], cancelled),
                "Cancellation must invoke the same exact release callback once.");

            BrownFurCastTransaction cleared = Transaction(1);
            Assertions.True(tracker.Begin(new object(), new object(), cleared),
                "The clear fixture must retain one validated transaction.");
            tracker.Clear();
            Assertions.True(released.Count == 3 &&
                ReferenceEquals(released[2], cleared) &&
                tracker.ActiveTransactionCount == 0,
                "Bounded transition cleanup must release every retained scope.");
        }

        internal static void ReservoirReservationsAreAtomic()
        {
            var ledger = new BrownFurReservoirReservationLedger<object>();
            object owner = new object();
            object otherOwner = new object();
            Assertions.True(ledger.TryReserve(owner, "queued-a", 1, 2) &&
                ledger.TryReserve(owner, "queued-b", 1, 2),
                "Two queued one-point casts may reserve a two-point reservoir.");
            Assertions.False(ledger.TryReserve(owner, "queued-c", 1, 2),
                "A third queued cast must reject before execution submission.");
            Assertions.False(ledger.TryReserve(owner, "queued-a", 0, 2),
                "Transaction identities must remain globally unique.");
            Assertions.True(ledger.TryReserve(otherOwner, "other", 2, 2),
                "A distinct owner must retain an independent reservation total.");
            int debited = 0;
            Assertions.True(ledger.TryCommit(owner, "queued-a", cost => {
                debited += cost; return true; }) && debited == 1 &&
                ledger.ReservedPoints(owner) == 1 &&
                ledger.ReservedPoints(otherOwner) == 2,
                "Commit must debit one exact reservation and preserve all others.");
            Assertions.True(ledger.Release(owner, "queued-b") &&
                ledger.ReservedPoints(owner) == 0 &&
                ledger.ReservationCount == 1,
                "Cancellation must release only its exact queued reservation.");
            ledger.Clear();
            Assertions.True(ledger.ReservationCount == 0 &&
                ledger.ReservedPoints(otherOwner) == 0,
                "Load or combat cleanup must clear every remaining reservation.");
        }

        internal static void ReservoirReservationsReleaseOnEveryTerminalCommit()
        {
            var ledger = new BrownFurReservoirReservationLedger<object>();
            object owner = new object();
            Assertions.True(ledger.TryReserve(owner, "rejected", 2, 2),
                "The rejected-commit fixture must reserve its combined cost.");
            Assertions.False(ledger.TryCommit(owner, "rejected", cost => false),
                "A failed exact debit must reject commit.");
            Assertions.True(ledger.ReservationCount == 0 &&
                ledger.ReservedPoints(owner) == 0,
                "A failed debit must never strand a queued reservation.");

            Assertions.True(ledger.TryReserve(owner, "exception", 1, 1),
                "The exceptional-commit fixture must reserve one point.");
            bool threw = false;
            try
            {
                ledger.TryCommit(owner, "exception", cost => {
                    throw new InvalidOperationException("synthetic debit failure");
                });
            }
            catch (InvalidOperationException) { threw = true; }
            Assertions.True(threw && ledger.ReservationCount == 0 &&
                ledger.ReservedPoints(owner) == 0,
                "An exceptional debit must release its reservation in finally.");
        }

        internal static void CommitCoordinatorIsAtomic()
        {
            var released = new List<BrownFurCastTransaction>();
            var coordinator = new BrownFurCastCommitCoordinator<object, object,
                object, object, object, object>(released.Add);
            object owner = new object();
            object command = new object();
            object ability = new object();
            object rule = new object();
            object context = new object();
            object process = new object();
            BrownFurCastTransaction transaction = Transaction(2);
            Assertions.True(coordinator.Begin(owner, command, ability,
                transaction, 2) && coordinator.ReservedPoints(owner) == 2 &&
                coordinator.AttachRule(ability, rule, context),
                "A validated combined cast must reserve before its rule runs.");
            Assertions.False(coordinator.Begin(owner, new object(), new object(),
                Transaction(1), 2),
                "A queued cast must reject when earlier reservations consume availability.");
            int debited = 0;
            Assertions.True(coordinator.Commit(owner, ability,
                cost => { debited += cost; return true; }) && debited == 2 &&
                coordinator.ReservationCount == 0 &&
                coordinator.AttachProcess(rule, process) &&
                coordinator.ProcessTerminal(process, false),
                "Commit must debit the full combined cost once and retain the process until completion.");
            Assertions.True(transaction.State ==
                BrownFurCastTransactionState.Completed && released.Count == 1 &&
                ReferenceEquals(released[0], transaction) &&
                coordinator.ActiveTransactionCount == 0,
                "Process completion must release every coupled surface once.");
        }

        internal static void CommitCoordinatorRejectionCleansUp()
        {
            var released = new List<BrownFurCastTransaction>();
            var coordinator = new BrownFurCastCommitCoordinator<object, object,
                object, object, object, object>(released.Add);
            object owner = new object();
            object command = new object();
            object ability = new object();
            BrownFurCastTransaction transaction = Transaction(1);
            Assertions.True(coordinator.Begin(owner, command, ability,
                transaction, 1),
                "The rejected-commit fixture must reserve before commitment.");
            Assertions.False(coordinator.Commit(owner, ability,
                cost => false),
                "A failed exact debit must reject the coupled commit.");
            Assertions.True(transaction.State ==
                BrownFurCastTransactionState.Rejected &&
                coordinator.ReservationCount == 0 &&
                coordinator.ActiveTransactionCount == 0 &&
                released.Count == 1 &&
                ReferenceEquals(released[0], transaction),
                "Commit rejection must release tracker, reservation, and scopes immediately.");
            Assertions.False(coordinator.EndCommand(command, true),
                "A later command-end signal must find no leaked rejected entry.");
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

        internal static void SupremacyScopeIsExactAndNonStacking()
        {
            var tracker = new BrownFurSupremacyScopeTracker<object, object>();
            object ability = new object();
            object context = new object();
            bool addExtend;
            Assertions.True(tracker.Begin("supremacy-1", ability) &&
                tracker.TryResolve(ability, context, false, out addExtend) &&
                addExtend && tracker.ModifiedContextCount("supremacy-1") == 1,
                "An exact ordinary cast context must receive Extend once.");
            Assertions.False(tracker.TryResolve(ability, context, false,
                out addExtend),
                "The same execution context must not receive Extend twice.");
            object preparedExtended = new object();
            Assertions.True(tracker.TryResolve(ability, preparedExtended, true,
                out addExtend) && !addExtend &&
                tracker.ModifiedContextCount("supremacy-1") == 1,
                "An already Extended context must be observed without modification.");
            Assertions.False(tracker.TryResolve(new object(), new object(),
                false, out addExtend),
                "An unrelated AbilityData identity must remain unchanged.");
            Assertions.True(tracker.Release("supremacy-1") &&
                tracker.ActiveScopeCount == 0,
                "Releasing the exact transaction must clear its duration scope.");
        }

        internal static void SupremacyScopesAreIsolated()
        {
            var tracker = new BrownFurSupremacyScopeTracker<object, object>();
            object firstAbility = new object();
            object secondAbility = new object();
            bool addExtend;
            Assertions.True(tracker.Begin("supremacy-a", firstAbility) &&
                tracker.Begin("supremacy-b", secondAbility) &&
                !tracker.Begin("supremacy-c", firstAbility) &&
                tracker.TryResolve(firstAbility, new object(), false,
                    out addExtend) && addExtend &&
                tracker.TryResolve(secondAbility, new object(), false,
                    out addExtend) && addExtend,
                "Queued casts must retain distinct AbilityData identities.");
            tracker.Release("supremacy-a");
            Assertions.True(tracker.ActiveScopeCount == 1 &&
                tracker.ModifiedContextCount("supremacy-b") == 1,
                "Cleaning one cast must not release another cast's duration state.");
            tracker.Clear();
            Assertions.Equal(0, tracker.ActiveScopeCount,
                "Load or combat cleanup must clear all duration scopes.");
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

        private static BrownFurCastTransaction Transaction(int cost)
        {
            var transaction = new BrownFurCastTransaction(Intent(cost));
            var decision = new BrownFurCastDecision(true, string.Empty, cost,
                cost > 0, cost > 1, true, 2,
                cost > 1 ? BrownFurShareDelivery.Touch :
                    BrownFurShareDelivery.None);
            Assertions.True(transaction.Validate(decision),
                "The lifecycle fixture transaction must validate.");
            return transaction;
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
