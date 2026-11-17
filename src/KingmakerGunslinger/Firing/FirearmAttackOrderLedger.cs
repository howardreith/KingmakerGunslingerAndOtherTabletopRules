using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using KingmakerGunslinger.Firearms;

namespace KingmakerGunslinger.Firing
{
    /// <summary>
    /// Concrete-item command ownership. A proposal does not release suppression;
    /// acceptance follows successful native submission, and a committed
    /// recovery of the exact item to Normal releases its misfire locks
    /// (POL-01). Never serialized.
    /// </summary>
    internal sealed class FirearmAttackOrderLedger
    {
        private readonly ConditionalWeakTable<object, Actor> actors =
            new ConditionalWeakTable<object, Actor>();
        // Misfire provenance by physical firearm: which actors hold a
        // misfire lock on this exact item. Weak in both directions (the
        // table does not keep the item alive, the entries do not keep the
        // actors alive), never serialized, pruned whenever it is read.
        private readonly ConditionalWeakTable<object, MisfireLocks> locks =
            new ConditionalWeakTable<object, MisfireLocks>();
        private sealed class MisfireLocks
        {
            internal readonly List<WeakReference> Actors = new List<WeakReference>();
        }
        private sealed class Damage
        {
            // Monotonic generation: rejects work captured before a committed
            // degradation. It is history, never the item's current condition.
            internal int Epoch;
            internal bool Suppressed;
            // Broken-sequence authority: automatic work needs an accepted
            // explicit order from a committed degradation until such an order
            // completes normally on the repaired (Normal) firearm.
            internal bool RequiresOrder;
        }
        private sealed class Actor
        {
            internal readonly ConditionalWeakTable<object, Damage> Items =
                new ConditionalWeakTable<object, Damage>();
            internal Order Current;
            internal long Submission;
        }
        internal sealed class Order
        {
            internal readonly object Actor, Weapon;
            internal object Target;
            internal readonly int Epoch;
            internal bool Accepted, Cancelled;
            internal object Owner, CommandContainer;
            internal bool OwnerSlotRemoved;
            internal Order(object actor, object weapon, object target, int epoch)
            {
                Actor = actor; Weapon = weapon; Target = target; Epoch = epoch;
            }
        }
        private Actor For(object actor)
        {
            if (actor == null) throw new ArgumentNullException("actor");
            return actors.GetOrCreateValue(actor);
        }
        internal int Epoch(object actor, object weapon)
        {
            return actor == null || weapon == null ? 0 :
                For(actor).Items.GetOrCreateValue(weapon).Epoch;
        }
        internal bool IsSuppressed(object actor, object weapon)
        {
            return actor != null && weapon != null &&
                For(actor).Items.GetOrCreateValue(weapon).Suppressed;
        }
        internal bool RequiresOrder(object actor, object weapon)
        {
            return actor != null && weapon != null &&
                For(actor).Items.GetOrCreateValue(weapon).RequiresOrder;
        }
        internal Order Current(object actor)
        {
            if (actor == null) return null;
            Order order = For(actor).Current;
            return IsCurrent(order) ? order : null;
        }
        internal Order Propose(object actor, object weapon, object target)
        {
            if (weapon == null) throw new ArgumentNullException("weapon");
            if (target == null) throw new ArgumentNullException("target");
            return new Order(actor, weapon, target, Epoch(actor, weapon));
        }
        internal bool Accept(Order order, long submittedAt)
        {
            if (order == null || submittedAt <= 0 || order.Cancelled || order.Accepted ||
                order.Epoch != Epoch(order.Actor, order.Weapon) ||
                Submission(order.Actor) != submittedAt) return false;
            Actor actor = For(order.Actor);
            Cancel(actor.Current);
            actor.Current = order;
            order.Accepted = true;
            actor.Items.GetOrCreateValue(order.Weapon).Suppressed = false;
            return true;
        }
        internal bool IsCurrent(Order order)
        {
            return order != null && order.Accepted && !order.Cancelled &&
                ReferenceEquals(For(order.Actor).Current, order) &&
                order.Epoch == Epoch(order.Actor, order.Weapon);
        }
        // A retained native command owns continuation, independently of other
        // coexisting command slots. Proposals and stale orders cannot take it.
        internal bool Own(Order order, object command, object container = null)
        {
            if (command == null || !IsCurrent(order)) return false;
            order.Owner = command;
            order.CommandContainer = container;
            order.OwnerSlotRemoved = false;
            return true;
        }
        internal bool Retarget(Order order, object owner, object previousTarget, object target)
        {
            if (!IsCurrent(order) || owner == null || target == null ||
                !ReferenceEquals(order.Owner, owner) || !ReferenceEquals(order.Target, previousTarget)) return false;
            order.Target = target;
            return true;
        }
        // Normal completion, not a fault: no native command carries the order
        // and its target is defeated, so nothing remains to continue. Only a
        // currently Normal firearm completes; it then returns to ordinary
        // automatic combat without suppression, while the epoch still rejects
        // pre-degradation work. A Broken firearm keeps its explicit order.
        internal bool Complete(Order order, bool carried, bool targetDefeated,
            FirearmCondition condition)
        {
            if (!IsCurrent(order) || carried || !targetDefeated ||
                condition != FirearmCondition.Normal) return false;
            Actor actor = For(order.Actor);
            actor.Current = null;
            actor.Items.GetOrCreateValue(order.Weapon).RequiresOrder = false;
            Forget(order.Actor, order.Weapon);
            return true;
        }
        internal void Cancel(Order order)
        {
            if (order == null) return;
            order.Cancelled = true;
            Actor actor = For(order.Actor);
            if (!ReferenceEquals(actor.Current, order)) return;
            actor.Current = null;
            Damage damage = actor.Items.GetOrCreateValue(order.Weapon);
            if (damage.RequiresOrder) damage.Suppressed = true;
        }
        internal void Break(object actor, object weapon)
        {
            if (actor == null || weapon == null) return;
            Actor state = For(actor);
            Damage damage = state.Items.GetOrCreateValue(weapon);
            checked { damage.Epoch++; }
            damage.Suppressed = true;
            damage.RequiresOrder = true;
            Remember(actor, weapon);
            if (state.Current != null && ReferenceEquals(state.Current.Weapon, weapon))
                Cancel(state.Current);
        }
        /// <summary>
        /// POL-01 (owner decision B): a successful recovery of this exact
        /// physical firearm, already committed to Normal, releases the
        /// misfire locks every actor holds on it, wherever the item now is.
        /// The epoch is kept, so every order, reload continuation and
        /// callback captured before the break stays refused; no order is
        /// created, accepted, cancelled or resumed, and nothing is submitted.
        /// A later attack needs a new construction, so a fresh automatic
        /// decision or a new player order. Any other committed condition
        /// releases nothing. Idempotent. Returns the locks released.
        /// </summary>
        internal int Recover(object weapon, FirearmCondition committedCondition)
        {
            if (weapon == null || committedCondition != FirearmCondition.Normal) return 0;
            MisfireLocks held;
            if (!locks.TryGetValue(weapon, out held)) return 0;
            int released = 0;
            foreach (WeakReference reference in held.Actors.ToArray())
            {
                object owner = reference.Target;
                Actor state;
                Damage damage;
                if (owner != null && actors.TryGetValue(owner, out state) &&
                    state.Items.TryGetValue(weapon, out damage) &&
                    (damage.Suppressed || damage.RequiresOrder))
                {
                    damage.Suppressed = false;
                    damage.RequiresOrder = false;
                    released++;
                }
            }
            held.Actors.Clear();
            locks.Remove(weapon);
            return released;
        }
        /// <summary>Live actors still recorded against this exact firearm (diagnostics and tests).</summary>
        internal int LockHolders(object weapon)
        {
            MisfireLocks held;
            if (weapon == null || !locks.TryGetValue(weapon, out held)) return 0;
            held.Actors.RemoveAll(reference => reference.Target == null);
            return held.Actors.Count;
        }
        private void Remember(object actor, object weapon)
        {
            MisfireLocks held = locks.GetOrCreateValue(weapon);
            held.Actors.RemoveAll(reference => reference.Target == null);
            foreach (WeakReference reference in held.Actors)
                if (ReferenceEquals(reference.Target, actor)) return;
            held.Actors.Add(new WeakReference(actor));
        }
        private void Forget(object actor, object weapon)
        {
            MisfireLocks held;
            if (actor == null || weapon == null || !locks.TryGetValue(weapon, out held)) return;
            held.Actors.RemoveAll(reference => reference.Target == null ||
                ReferenceEquals(reference.Target, actor));
            if (held.Actors.Count == 0) locks.Remove(weapon);
        }
        internal long Submit(object actor)
        {
            Actor state = For(actor);
            checked { state.Submission++; }
            return state.Submission;
        }
        internal long Submission(object actor)
        {
            return actor == null ? 0 : For(actor).Submission;
        }
        internal bool MayResume(object actor, object weapon, int epoch,
            long submission, Order order)
        {
            return actor != null && weapon != null && epoch == Epoch(actor, weapon) &&
                submission == Submission(actor) && !IsSuppressed(actor, weapon) &&
                (order == null ? !RequiresOrder(actor, weapon) : IsCurrent(order));
        }
    }
}
