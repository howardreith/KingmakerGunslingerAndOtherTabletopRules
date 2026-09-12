using System;
using System.Runtime.CompilerServices;

namespace KingmakerGunslinger.Firing
{
    /// <summary>
    /// Concrete-item command ownership. A proposal does not release suppression;
    /// acceptance follows successful native submission. Never serialized.
    /// </summary>
    internal sealed class FirearmAttackOrderLedger
    {
        private readonly ConditionalWeakTable<object, Actor> actors =
            new ConditionalWeakTable<object, Actor>();
        private sealed class Damage
        {
            internal int Epoch;
            internal bool Suppressed;
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
            internal readonly object Actor, Weapon, Target;
            internal readonly int Epoch;
            internal bool Accepted, Cancelled;
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
        internal void Cancel(Order order)
        {
            if (order == null) return;
            order.Cancelled = true;
            Actor actor = For(order.Actor);
            if (!ReferenceEquals(actor.Current, order)) return;
            actor.Current = null;
            Damage damage = actor.Items.GetOrCreateValue(order.Weapon);
            if (damage.Epoch != 0) damage.Suppressed = true;
        }
        internal void Break(object actor, object weapon)
        {
            if (actor == null || weapon == null) return;
            Actor state = For(actor);
            Damage damage = state.Items.GetOrCreateValue(weapon);
            checked { damage.Epoch++; }
            damage.Suppressed = true;
            if (state.Current != null && ReferenceEquals(state.Current.Weapon, weapon))
                Cancel(state.Current);
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
                (order == null || IsCurrent(order));
        }
    }
}
