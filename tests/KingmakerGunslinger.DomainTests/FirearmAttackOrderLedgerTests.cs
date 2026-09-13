using KingmakerGunslinger.Firing;

namespace KingmakerGunslinger.DomainTests
{
    internal static class FirearmAttackOrderLedgerTests
    {
        private sealed class Fixture
        {
            internal readonly FirearmAttackOrderLedger Ledger = new FirearmAttackOrderLedger();
            internal readonly object Actor = new object(), Weapon = new object(), Target = new object();
            internal FirearmAttackOrderLedger.Order Accept()
            {
                var order = Ledger.Propose(Actor, Weapon, Target);
                Assertions.True(Ledger.Accept(order, Ledger.Submit(Actor)), "Submitted order accepted.");
                return order;
            }
        }
        internal static void ProposalIsNotAcceptance()
        {
            var f = new Fixture();
            f.Ledger.Break(f.Actor, f.Weapon);
            var proposal = f.Ledger.Propose(f.Actor, f.Weapon, f.Target);
            Assertions.True(f.Ledger.IsSuppressed(f.Actor, f.Weapon), "Construction retains suppression.");
            Assertions.False(f.Ledger.Accept(proposal, 0), "An unsubmitted command cannot accept.");
            f.Ledger.Cancel(proposal);
            Assertions.False(f.Ledger.Accept(proposal, f.Ledger.Submit(f.Actor)), "A faulted/rejected proposal cannot accept later.");
            Assertions.True(f.Ledger.IsSuppressed(f.Actor, f.Weapon), "No readiness query releases the old order.");
        }
        internal static void AcceptedReloadSurvivesTimeNotAnotherOrder()
        {
            var f = new Fixture();
            f.Ledger.Break(f.Actor, f.Weapon);
            var order = f.Accept();
            long submitted = f.Ledger.Submission(f.Actor);
            for (int laterFrame = 0; laterFrame < 120; laterFrame++)
                Assertions.True(f.Ledger.MayResume(f.Actor, f.Weapon, 1, submitted, order),
                    "A pending accepted reload has no frame or pause expiry.");
            f.Ledger.Submit(f.Actor);
            Assertions.False(f.Ledger.MayResume(f.Actor, f.Weapon, 1, submitted, order),
                "A later command invalidates the captured callback.");
        }
        internal static void LaterDegradationCancelsAcceptedSequence()
        {
            var f = new Fixture();
            var first = f.Accept();
            f.Ledger.Break(f.Actor, f.Weapon);
            Assertions.False(f.Ledger.IsCurrent(first), "The first break cancels the old accepted order.");
            var second = f.Accept();
            long submitted = f.Ledger.Submission(f.Actor);
            Assertions.True(f.Ledger.MayResume(f.Actor, f.Weapon, 1, submitted, second), "The new Broken order can resume.");
            f.Ledger.Break(f.Actor, f.Weapon);
            Assertions.False(f.Ledger.IsCurrent(second), "The next degradation cancels the new sequence too.");
            Assertions.False(f.Ledger.MayResume(f.Actor, f.Weapon, 1, submitted, second), "No post-wreck callback survives.");
            Assertions.Equal(2, f.Ledger.Epoch(f.Actor, f.Weapon), "Each committed degradation advances once.");
        }
        internal static void ActorAndItemIsolation()
        {
            var f = new Fixture();
            var otherActor = new object(); var otherWeapon = new object();
            f.Ledger.Break(f.Actor, f.Weapon);
            var other = f.Ledger.Propose(otherActor, f.Weapon, f.Target);
            Assertions.True(f.Ledger.Accept(other, f.Ledger.Submit(otherActor)), "Other actor can issue its own order.");
            Assertions.True(f.Ledger.IsSuppressed(f.Actor, f.Weapon), "Another character cannot release suppression.");
            Assertions.False(f.Ledger.IsSuppressed(f.Actor, otherWeapon), "A different item is not marked broken.");
            Assertions.Equal(0, f.Ledger.Epoch(otherActor, f.Weapon), "Ownership isolates epochs too.");
        }
        internal static void SupersededOrdersCannotResume()
        {
            var f = new Fixture(); f.Ledger.Break(f.Actor, f.Weapon);
            var old = f.Accept();
            var target = new object();
            var fresh = f.Ledger.Propose(f.Actor, f.Weapon, target);
            long submitted = f.Ledger.Submit(f.Actor);
            Assertions.True(f.Ledger.Accept(fresh, submitted), "A deliberate different-target order is accepted.");
            Assertions.False(f.Ledger.IsCurrent(old), "Old same-epoch authority is revoked, not shared.");
            Assertions.False(f.Ledger.MayResume(f.Actor, f.Weapon, 1, submitted, old), "Even a matching epoch/submission cannot revive the old order.");
            Assertions.True(f.Ledger.IsCurrent(fresh), "The exact new target retains its order.");
            f.Ledger.Cancel(old);
            Assertions.True(f.Ledger.IsCurrent(fresh), "A stale cancellation cannot cancel a newer order.");
            f.Ledger.Cancel(fresh);
            Assertions.False(f.Ledger.IsCurrent(fresh), "Fault cleanup revokes the exact accepted invocation.");
            Assertions.True(f.Ledger.IsSuppressed(f.Actor, f.Weapon), "A fault cannot leave reattack authority alive.");
            var repeat = f.Accept();
            Assertions.True(f.Ledger.IsCurrent(repeat), "A later real submission can explicitly select the same enemy again.");
            f.Ledger.Cancel(fresh);
            Assertions.True(f.Ledger.IsCurrent(repeat), "Late fault cleanup cannot revoke a newer accepted order.");
        }
        internal static void OwnershipRequiresAcceptedCurrentOrder()
        {
            var f = new Fixture();
            var proposal = f.Ledger.Propose(f.Actor, f.Weapon, f.Target);
            Assertions.False(f.Ledger.Own(proposal, new object()), "Construction cannot take ownership.");
            var order = f.Accept(); var command = new object();
            Assertions.True(f.Ledger.Own(order, command), "Accepted native command owns its order.");
            Assertions.True(ReferenceEquals(command, order.Owner), "Ownership uses exact command identity.");
            var next = f.Accept();
            Assertions.False(f.Ledger.Own(order, new object()), "A superseded order cannot take ownership back.");
            Assertions.True(f.Ledger.IsCurrent(next), "Stale ownership cannot affect the newer order.");
        }
        internal static void OwnershipDoesNotAdvanceSubmission()
        {
            var f = new Fixture(); f.Ledger.Break(f.Actor, f.Weapon);
            var order = f.Accept(); long submission = f.Ledger.Submission(f.Actor);
            Assertions.True(f.Ledger.Own(order, new object()), "Record the retained native reload.");
            Assertions.True(f.Ledger.MayResume(f.Actor, f.Weapon, 1, submission, order),
                "Recording ownership does not expire its pending continuation.");
            f.Ledger.Break(f.Actor, f.Weapon);
            Assertions.False(f.Ledger.Own(order, new object()), "Later degradation still revokes ownership.");
            Assertions.False(f.Ledger.MayResume(f.Actor, f.Weapon, 1, submission, order),
                "No owner record can restore a degraded sequence.");
        }
        internal static void RetargetRequiresCurrentCommandOwnership()
        {
            var f = new Fixture(); var order = f.Accept(); var owner = new object(); var target = new object();
            Assertions.False(f.Ledger.Retarget(order, owner, f.Target, target), "No owner was retained.");
            f.Ledger.Own(order, owner);
            Assertions.False(f.Ledger.Retarget(order, new object(), f.Target, target), "A foreign command cannot retarget.");
            Assertions.False(f.Ledger.Retarget(order, owner, new object(), target), "The old target must be the current target.");
            long submission = f.Ledger.Submission(f.Actor);
            Assertions.True(f.Ledger.Retarget(order, owner, f.Target, target), "The retained native sequence can resolve its next target.");
            Assertions.True(ReferenceEquals(order.Target, target) && ReferenceEquals(order.Owner, owner), "The same owner survives.");
            Assertions.Equal(submission, f.Ledger.Submission(f.Actor), "Retargeting is not a new submitted command.");
        }
        internal static void RetargetCannotReviveDegradedOrSupersededOrders()
        {
            var f = new Fixture(); var order = f.Accept(); var owner = new object();
            f.Ledger.Own(order, owner); f.Ledger.Break(f.Actor, f.Weapon);
            Assertions.False(f.Ledger.Retarget(order, owner, f.Target, new object()), "Native target search cannot undo a committed break.");
            var next = f.Accept(); f.Ledger.Own(next, new object());
            Assertions.False(f.Ledger.Retarget(order, owner, f.Target, new object()), "Stale target search cannot borrow a fresh order.");
            Assertions.True(f.Ledger.IsCurrent(next), "Fresh explicit order is unaffected by stale work.");
        }
        internal static void DelayedAcceptanceFailsClosed()
        {
            var f = new Fixture();
            var old = f.Ledger.Propose(f.Actor, f.Weapon, f.Target);
            long submitted = f.Ledger.Submit(f.Actor);
            f.Ledger.Break(f.Actor, f.Weapon);
            Assertions.False(f.Ledger.Accept(old, submitted), "Degradation between construction and acceptance rejects the order.");
            var fresh = f.Ledger.Propose(f.Actor, f.Weapon, f.Target);
            submitted = f.Ledger.Submit(f.Actor);
            f.Ledger.Submit(f.Actor);
            Assertions.False(f.Ledger.Accept(fresh, submitted), "A reentrant newer submission wins.");
            Assertions.True(f.Ledger.IsSuppressed(f.Actor, f.Weapon), "Neither stale acceptance releases suppression.");
        }
    }
}
