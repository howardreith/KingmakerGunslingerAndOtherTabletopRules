using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using KingmakerGunslinger.Deeds;
using KingmakerGunslinger.Firearms;
using KingmakerGunslinger.Firing;
using KingmakerGunslinger.Gunsmithing;
using KingmakerGunslinger.Recovery;

namespace KingmakerGunslinger.DomainTests
{
    /// <summary>
    /// POL-01 / S4 T3, owner decision B: a successful recovery of the exact
    /// firearm to Normal releases its misfire locks and permits fresh
    /// automatic decisions; nothing captured before the break ever resumes.
    /// The item transitions run through the production state machine, Quick
    /// Clear service, repair transaction and rest policy; each recovery path
    /// then releases through the production ledger exactly as its runtime
    /// does (the committed condition, after its rollback boundary).
    /// </summary>
    internal static class FirearmMisfireRecoveryTests
    {
        private sealed class MemoryStore : IFirearmRepairStateStore
        {
            internal FirearmState State;
            public FirearmState Read() { return State; }
            public void Replace(FirearmState expectedCurrent, FirearmState replacement)
            {
                if (State != expectedCurrent) throw new InvalidOperationException("unexpected current state");
                State = replacement;
            }
        }

        /// <summary>Kit counts reported in turn; the last repeats.</summary>
        private sealed class Kits : IRepairKitInventory
        {
            private readonly Queue<int> counts;
            private int last;
            internal Kits(params int[] counts) { this.counts = new Queue<int>(counts); last = counts.Length == 0 ? 0 : counts[0]; }
            public int Count() { if (counts.Count > 0) last = counts.Dequeue(); return last; }
            public void Add(int amount) { throw new InvalidOperationException("repair adds nothing"); }
            public void Remove(int amount) { throw new InvalidOperationException("repair consumes nothing"); }
        }

        private sealed class World
        {
            internal readonly FirearmAttackOrderLedger Ledger = new FirearmAttackOrderLedger();
            internal readonly object Actor = new object(), Gun = new object(), Target = new object();
            internal FirearmState State = FirearmState.CreateEmpty();

            internal FirearmAttackOrderLedger.Order Order(object actor = null, object target = null)
            {
                actor = actor ?? Actor;
                var order = Ledger.Propose(actor, Gun, target ?? Target);
                Assertions.True(Ledger.Accept(order, Ledger.Submit(actor)), "A submitted player order is accepted.");
                return order;
            }
            internal void Misfire(object actor = null)
            {
                State = FirearmStateMachine.ApplyMisfireDamage(State);
                Ledger.Break(actor ?? Actor, Gun);
            }
            /// <summary>Quick Clear: the service decides, the state machine repairs, then the release.</summary>
            internal int QuickClear()
            {
                var decision = new QuickClearService().Evaluate(new QuickClearRequest(
                    QuickClearMode.Standard, true, State.Condition, true, 1));
                if (!decision.ShouldRepair) return Ledger.Recover(Gun, State.Condition);
                State = FirearmStateMachine.Repair(State);
                return Ledger.Recover(Gun, State.Condition);
            }
            /// <summary>Repair Firearm: the production transaction; release only on its committed success.</summary>
            internal int RepairFirearm(IRepairKitInventory kits, out bool threw)
            {
                var store = new MemoryStore { State = State };
                threw = false;
                FirearmRepairResult result = null;
                try { result = new FirearmRepairTransactionService().TryRepairToNormal(store, kits); }
                catch (FirearmRepairTransactionException) { threw = true; }
                State = store.State;
                return result != null && result.Succeeded ? Ledger.Recover(Gun, store.Read().Condition) : 0;
            }
            /// <summary>Completed rest: the production rest and item policies; a per-item fault stops before the release.</summary>
            internal int Rest(bool succeeded, bool encounter, bool skipTime, bool repairer, bool kit, bool itemFaults)
            {
                if (CompletedRestMaintenancePolicy.EvaluateRest(succeeded, encounter, skipTime, repairer, kit) !=
                    CompletedRestMaintenanceStatus.MaintenanceDue) return 0;
                if (CompletedRestMaintenancePolicy.EvaluateItem(State.Condition) == CompletedRestItemDecision.SkipNormal)
                    return 0;
                if (itemFaults) return 0;
                State = FirearmStateMachine.Repair(State);
                return Ledger.Recover(Gun, State.Condition);
            }
        }

        internal static void BreakSuppressesAndAdvancesTheEpoch()
        {
            var w = new World();
            w.Misfire();
            Assertions.True(w.Ledger.IsSuppressed(w.Actor, w.Gun) && w.Ledger.RequiresOrder(w.Actor, w.Gun),
                "A committed break suppresses and requires an order.");
            Assertions.Equal(1, w.Ledger.Epoch(w.Actor, w.Gun), "The break advances the epoch once.");
            Assertions.Equal(1, w.Ledger.LockHolders(w.Gun), "The break records its actor against the exact firearm.");
        }

        internal static void RecoveryKeepsTheEpochAndCreatesNoOrder()
        {
            var w = new World();
            w.Misfire();
            long submission = w.Ledger.Submission(w.Actor);
            Assertions.Equal(1, w.QuickClear(), "Quick Clear to Normal releases the one lock.");
            Assertions.Equal(FirearmCondition.Normal, w.State.Condition, "The item is Normal.");
            Assertions.Equal(1, w.Ledger.Epoch(w.Actor, w.Gun), "Recovery keeps the epoch.");
            Assertions.True(w.Ledger.Current(w.Actor) == null, "Recovery creates no current order.");
            Assertions.Equal(submission, w.Ledger.Submission(w.Actor), "Recovery submits nothing.");
            Assertions.False(w.Ledger.IsSuppressed(w.Actor, w.Gun) || w.Ledger.RequiresOrder(w.Actor, w.Gun),
                "The misfire lock is released.");
        }

        internal static void PreBreakWorkStaysRefusedAfterRecovery()
        {
            var w = new World();
            var before = w.Order();
            var owner = new object();
            Assertions.True(w.Ledger.Own(before, owner), "The pre-break order owned a native command.");
            long submitted = w.Ledger.Submission(w.Actor);
            w.Misfire();
            w.QuickClear();
            Assertions.False(w.Ledger.IsCurrent(before), "The interrupted order stays cancelled.");
            Assertions.False(w.Ledger.MayResume(w.Actor, w.Gun, 0, submitted, before),
                "Its captured continuation stays refused.");
            Assertions.False(w.Ledger.MayResume(w.Actor, w.Gun, 0, submitted, null),
                "Unordered work captured at the old epoch stays refused.");
            Assertions.False(BrokenSequenceInterruptionPolicy.MayResumeCapturedAttack(0, w.Ledger.Epoch(w.Actor, w.Gun)),
                "A captured reload-resume from before the break stays refused.");
            Assertions.False(w.Ledger.Retarget(before, owner, w.Target, new object()) || w.Ledger.Own(before, new object()),
                "The old command can neither retarget nor take ownership back.");
        }

        internal static void AFreshAutomaticConstructionIsPermitted()
        {
            var w = new World();
            w.Misfire();
            Assertions.Equal(BrokenSequenceConstructionDecision.RejectInterrupted,
                BrokenSequenceInterruptionPolicy.EvaluateConstruction(w.Ledger.IsSuppressed(w.Actor, w.Gun), false),
                "Before recovery an automatic construction is refused.");
            w.QuickClear();
            Assertions.Equal(BrokenSequenceConstructionDecision.Allow,
                BrokenSequenceInterruptionPolicy.EvaluateConstruction(w.Ledger.IsSuppressed(w.Actor, w.Gun), false),
                "After recovery a new automatic construction is allowed, with no order borrowed.");
            Assertions.True(w.Ledger.MayResume(w.Actor, w.Gun, w.Ledger.Epoch(w.Actor, w.Gun), w.Ledger.Submission(w.Actor), null),
                "New unordered automatic work at the current epoch may run.");
        }

        internal static void ReleaseIsScopedToTheExactFirearm()
        {
            var w = new World();
            object otherActor = new object(), otherGun = new object();
            w.Misfire();
            w.Ledger.Break(otherActor, otherGun);
            w.Ledger.Break(w.Actor, otherGun);
            Assertions.Equal(1, w.QuickClear(), "Only the exact firearm's lock is released.");
            Assertions.False(w.Ledger.IsSuppressed(w.Actor, w.Gun), "This actor's lock on this gun is released.");
            Assertions.True(w.Ledger.IsSuppressed(w.Actor, otherGun), "The same actor's lock on another gun stays.");
            Assertions.True(w.Ledger.IsSuppressed(otherActor, otherGun), "Another actor's lock on another gun stays.");
            Assertions.Equal(2, w.Ledger.LockHolders(otherGun), "The other gun's provenance is untouched.");
            Assertions.Equal(0, w.Ledger.Recover(new object(), FirearmCondition.Normal), "An unrelated firearm releases nothing.");
            Assertions.False(w.Ledger.IsSuppressed(otherActor, w.Gun), "An actor that never broke this gun was never locked.");
        }

        internal static void FailedOrRolledBackRepairReleasesNothing()
        {
            var w = new World();
            w.Misfire();
            bool threw;
            Assertions.Equal(0, w.RepairFirearm(new Kits(0), out threw), "No kit: the repair is rejected and releases nothing.");
            Assertions.True(!threw && w.State.Condition == FirearmCondition.Broken && w.Ledger.IsSuppressed(w.Actor, w.Gun),
                "A rejected repair leaves the gun Broken and locked.");
            // The kit count moves during the transaction: it rolls back and throws.
            Assertions.Equal(0, w.RepairFirearm(new Kits(1, 0), out threw), "A rolled-back repair releases nothing.");
            Assertions.True(threw && w.State.Condition == FirearmCondition.Broken && w.Ledger.IsSuppressed(w.Actor, w.Gun),
                "The rollback restored Broken and the lock stays.");
            Assertions.Equal(1, w.RepairFirearm(new Kits(1), out threw), "A committed repair to Normal releases.");
            Assertions.True(!threw && w.State.Condition == FirearmCondition.Normal && !w.Ledger.IsSuppressed(w.Actor, w.Gun),
                "Repaired and released.");

            var wrecked = new World();
            wrecked.Misfire();
            wrecked.Misfire();
            Assertions.Equal(FirearmCondition.Wrecked, wrecked.State.Condition, "A second misfire wrecks the gun.");
            Assertions.Equal(0, wrecked.RepairFirearm(new Kits(1), out threw), "Repair Firearm refuses a Wrecked gun.");
            Assertions.Equal(0, wrecked.QuickClear(), "Quick Clear on a Wrecked gun releases nothing.");
            Assertions.True(wrecked.Ledger.IsSuppressed(wrecked.Actor, wrecked.Gun), "A Wrecked gun stays locked.");
        }

        internal static void ReloadingABrokenGunReleasesNothing()
        {
            var w = new World();
            w.Misfire();
            var reloaded = new FirearmState(FirearmState.CurrentSchemaVersion, 1,
                FirearmStateTokenCatalog.DiagnosticLeadBall, w.State.Condition);
            Assertions.Equal(FirearmCondition.Broken, reloaded.Condition, "A reload leaves the gun Broken.");
            Assertions.Equal(0, w.Ledger.Recover(w.Gun, reloaded.Condition), "A Broken committed condition releases nothing.");
            Assertions.True(w.Ledger.IsSuppressed(w.Actor, w.Gun) && w.Ledger.LockHolders(w.Gun) == 1,
                "The lock and its provenance stay.");
        }

        internal static void IncompleteRestReleasesNothing()
        {
            foreach (var rest in new[]
            {
                new { Succeeded = false, Encounter = false, Skip = false, Repairer = true, Kit = true, Fault = false, Name = "cancelled or interrupted" },
                new { Succeeded = true, Encounter = true, Skip = false, Repairer = true, Kit = true, Fault = false, Name = "encounter" },
                new { Succeeded = true, Encounter = false, Skip = true, Repairer = true, Kit = true, Fault = false, Name = "skip-time" },
                new { Succeeded = true, Encounter = false, Skip = false, Repairer = false, Kit = true, Fault = false, Name = "no gunsmith" },
                new { Succeeded = true, Encounter = false, Skip = false, Repairer = true, Kit = false, Fault = false, Name = "no kit" },
                new { Succeeded = true, Encounter = false, Skip = false, Repairer = true, Kit = true, Fault = true, Name = "item failed" }
            })
            {
                var w = new World();
                w.Misfire();
                Assertions.Equal(0, w.Rest(rest.Succeeded, rest.Encounter, rest.Skip, rest.Repairer, rest.Kit, rest.Fault),
                    rest.Name + " rest releases nothing.");
                Assertions.True(w.State.Condition == FirearmCondition.Broken && w.Ledger.IsSuppressed(w.Actor, w.Gun),
                    rest.Name + ": the gun stays Broken and locked.");
            }
        }

        internal static void CompletedRestReleasesTheExactGunWhereverItIs()
        {
            // The ledger has no notion of who carries the gun now: the original
            // wielder, shared inventory or another participant all release the
            // same locks, because they are recorded against the physical item.
            foreach (string where in new[] { "original wielder", "shared inventory", "another participant" })
            {
                var w = new World();
                object otherActor = new object(), otherGun = new object();
                w.Misfire();
                w.Ledger.Break(otherActor, otherGun);
                object holder = where == "another participant" ? new object() : null;
                Assertions.Equal(1, w.Rest(true, false, false, true, true, false), where + ": the completed rest releases the lock.");
                Assertions.False(w.Ledger.IsSuppressed(w.Actor, w.Gun), where + ": the misfiring actor's lock is gone.");
                if (holder != null)
                    Assertions.False(w.Ledger.IsSuppressed(holder, w.Gun), where + ": the new holder was never locked.");
                Assertions.True(w.Ledger.IsSuppressed(otherActor, otherGun), where + ": another actor's other gun stays locked.");
            }
            var wrecked = new World();
            wrecked.Misfire();
            wrecked.Misfire();
            Assertions.Equal(1, wrecked.Rest(true, false, false, true, true, false), "A completed rest restores a Wrecked gun and releases it.");
            Assertions.Equal(FirearmCondition.Normal, wrecked.State.Condition, "Restored to Normal (unloaded).");
        }

        internal static void ASecondMisfireSuppressesAgain()
        {
            var w = new World();
            w.Misfire();
            w.QuickClear();
            w.Misfire();
            Assertions.True(w.Ledger.IsSuppressed(w.Actor, w.Gun) && w.Ledger.RequiresOrder(w.Actor, w.Gun),
                "A later break suppresses again.");
            Assertions.Equal(2, w.Ledger.Epoch(w.Actor, w.Gun), "The epoch keeps counting.");
            Assertions.Equal(1, w.Ledger.LockHolders(w.Gun), "The new break is recorded again.");
        }

        internal static void PlayerOrdersAndStopsStillWork()
        {
            var w = new World();
            w.Misfire();
            var broken = w.Order();
            Assertions.True(w.Ledger.IsCurrent(broken) && !w.Ledger.IsSuppressed(w.Actor, w.Gun),
                "Before recovery a player order still fires the Broken gun (manual-order workaround).");
            w.Ledger.Cancel(broken);
            Assertions.True(w.Ledger.IsSuppressed(w.Actor, w.Gun), "Stopping that Broken order suppresses again.");
            w.QuickClear();
            var after = w.Order();
            Assertions.True(w.Ledger.IsCurrent(after), "After recovery a player order is accepted normally.");
            long submitted = w.Ledger.Submission(w.Actor);
            w.Ledger.Cancel(after);
            Assertions.False(w.Ledger.IsCurrent(after) || w.Ledger.MayResume(w.Actor, w.Gun, 1, submitted, after),
                "A deliberate stop ends that order and its continuation.");
            Assertions.False(w.Ledger.IsSuppressed(w.Actor, w.Gun), "A stop after recovery does not re-lock the gun.");
        }

        internal static void ReleaseIsIdempotentAndLeaksNothing()
        {
            var w = new World();
            w.Misfire();
            Assertions.Equal(0, w.Ledger.Recover(w.Gun, FirearmCondition.Broken), "A non-Normal call releases nothing...");
            Assertions.Equal(1, w.Ledger.LockHolders(w.Gun), "...and keeps the provenance.");
            Assertions.Equal(1, w.QuickClear(), "The first committed recovery releases.");
            Assertions.Equal(0, w.Ledger.Recover(w.Gun, FirearmCondition.Normal), "A repeated recovery releases nothing.");
            Assertions.Equal(0, w.Ledger.LockHolders(w.Gun), "No association remains after release.");

            var ledger = new FirearmAttackOrderLedger();
            object gun = new object();
            BreakWithATransientActor(ledger, gun);
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            Assertions.Equal(0, ledger.LockHolders(gun), "A collected actor is not retained by the provenance.");
            Assertions.Equal(0, ledger.Recover(gun, FirearmCondition.Normal), "Nothing is released for a collected actor.");
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void BreakWithATransientActor(FirearmAttackOrderLedger ledger, object gun)
        {
            ledger.Break(new object(), gun);
        }
    }
}
