using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace KingmakerGunslinger.BrownFur
{
    internal sealed class BrownFurSupremacyScopeTracker<TAbility, TContext>
        where TAbility : class where TContext : class
    {
        private sealed class Scope
        {
            internal TAbility Ability;
            internal readonly HashSet<TContext> Observed =
                new HashSet<TContext>(ContextReferenceComparer.Instance);
            internal readonly HashSet<TContext> Modified =
                new HashSet<TContext>(ContextReferenceComparer.Instance);
        }

        private readonly object _gate = new object();
        private readonly Dictionary<string, Scope> _scopes =
            new Dictionary<string, Scope>(StringComparer.Ordinal);

        internal int ActiveScopeCount
        { get { lock (_gate) return _scopes.Count; } }

        internal bool Begin(string transactionIdentity, TAbility ability)
        {
            if (string.IsNullOrWhiteSpace(transactionIdentity) || ability == null)
                return false;
            lock (_gate)
            {
                if (_scopes.ContainsKey(transactionIdentity) ||
                    _scopes.Values.Any(value => ReferenceEquals(
                        value.Ability, ability))) return false;
                _scopes.Add(transactionIdentity, new Scope { Ability = ability });
                return true;
            }
        }

        internal bool TryResolve(TAbility ability, TContext context,
            bool alreadyExtended, out bool addExtend)
        {
            addExtend = false;
            if (ability == null || context == null) return false;
            lock (_gate)
            {
                Scope scope = _scopes.Values.FirstOrDefault(value =>
                    ReferenceEquals(value.Ability, ability));
                if (scope == null || !scope.Observed.Add(context)) return false;
                if (!alreadyExtended)
                {
                    scope.Modified.Add(context);
                    addExtend = true;
                }
                return true;
            }
        }

        internal int ModifiedContextCount(string transactionIdentity)
        {
            if (string.IsNullOrWhiteSpace(transactionIdentity)) return 0;
            lock (_gate)
            {
                Scope scope;
                return _scopes.TryGetValue(transactionIdentity, out scope) ?
                    scope.Modified.Count : 0;
            }
        }

        internal bool Release(string transactionIdentity)
        {
            if (string.IsNullOrWhiteSpace(transactionIdentity)) return false;
            lock (_gate) return _scopes.Remove(transactionIdentity);
        }

        internal void Clear()
        { lock (_gate) _scopes.Clear(); }

        private sealed class ContextReferenceComparer :
            IEqualityComparer<TContext>
        {
            internal static readonly ContextReferenceComparer Instance =
                new ContextReferenceComparer();
            public bool Equals(TContext left, TContext right)
            { return ReferenceEquals(left, right); }
            public int GetHashCode(TContext value)
            { return RuntimeHelpers.GetHashCode(value); }
        }
    }
}
