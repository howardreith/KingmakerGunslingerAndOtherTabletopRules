using System;
using System.Collections.Generic;
using System.Linq;

namespace KingmakerGunslinger.BrownFur
{
    internal sealed class BrownFurShareTargetingScopeTracker<TAbility, TCaster,
        TTarget>
        where TAbility : class
        where TCaster : class
        where TTarget : class
    {
        private sealed class Scope
        {
            internal string TransactionIdentity;
            internal TAbility Ability;
            internal TCaster Caster;
            internal TTarget Target;
            internal BrownFurShareDelivery Delivery;
        }

        private readonly object _gate = new object();
        private readonly Dictionary<string, Scope> _scopes =
            new Dictionary<string, Scope>(StringComparer.Ordinal);

        internal int ActiveScopeCount
        { get { lock (_gate) return _scopes.Count; } }

        internal bool Begin(string transactionIdentity, TAbility ability,
            TCaster caster, TTarget target, BrownFurShareDelivery delivery)
        {
            if (string.IsNullOrWhiteSpace(transactionIdentity) || ability == null ||
                caster == null || target == null ||
                (delivery != BrownFurShareDelivery.Touch &&
                 delivery != BrownFurShareDelivery.ThirtyFeet)) return false;
            lock (_gate)
            {
                if (_scopes.ContainsKey(transactionIdentity) || _scopes.Values.Any(
                    value => ReferenceEquals(value.Ability, ability))) return false;
                _scopes.Add(transactionIdentity, new Scope {
                    TransactionIdentity = transactionIdentity,
                    Ability = ability, Caster = caster, Target = target,
                    Delivery = delivery
                });
                return true;
            }
        }

        internal bool TryResolveAnchor(TAbility ability)
        { lock (_gate) return Find(ability) != null; }

        internal bool TryResolveTarget(TAbility ability, TCaster caster,
            TTarget target, out bool allowed)
        {
            allowed = false;
            lock (_gate)
            {
                Scope scope = Find(ability);
                if (scope == null) return false;
                allowed = ReferenceEquals(scope.Caster, caster) &&
                    ReferenceEquals(scope.Target, target);
                return true;
            }
        }

        internal bool TryGetDelivery(TAbility ability, TCaster caster,
            TTarget target, out BrownFurShareDelivery delivery)
        {
            delivery = BrownFurShareDelivery.None;
            lock (_gate)
            {
                Scope scope = Find(ability);
                if (scope == null || !ReferenceEquals(scope.Caster, caster) ||
                    !ReferenceEquals(scope.Target, target)) return false;
                delivery = scope.Delivery;
                return true;
            }
        }

        internal bool Release(string transactionIdentity)
        {
            if (string.IsNullOrWhiteSpace(transactionIdentity)) return false;
            lock (_gate) return _scopes.Remove(transactionIdentity);
        }

        internal void Clear()
        { lock (_gate) _scopes.Clear(); }

        private Scope Find(TAbility ability)
        {
            if (ability == null) return null;
            return _scopes.Values.FirstOrDefault(value =>
                ReferenceEquals(value.Ability, ability));
        }
    }
}
