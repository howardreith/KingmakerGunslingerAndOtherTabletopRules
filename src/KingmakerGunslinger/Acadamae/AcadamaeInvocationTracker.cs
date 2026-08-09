using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace KingmakerGunslinger.Acadamae
{
    internal sealed class AcadamaeInvocationTracker<TCommand, TAbility>
        where TCommand : class where TAbility : class
    {
        private readonly Dictionary<TCommand, TAbility> _pending =
            new Dictionary<TCommand, TAbility>(ReferenceComparer<TCommand>.Instance);
        [ThreadStatic] private static TCommand _active;

        internal int Count { get { return _pending.Count; } }

        internal bool Arm(TCommand command, TAbility ability)
        {
            if (command == null) throw new ArgumentNullException("command");
            if (ability == null) throw new ArgumentNullException("ability");
            if (_pending.ContainsKey(command)) return false;
            _pending.Add(command, ability);
            return true;
        }

        internal bool Begin(TCommand command)
        {
            if (command == null || !_pending.ContainsKey(command)) return false;
            _active = command;
            return true;
        }

        internal bool ConsumeSuccessful(TAbility ability)
        {
            TCommand command = _active;
            TAbility expected;
            if (command == null || ability == null ||
                !_pending.TryGetValue(command, out expected) ||
                !ReferenceEquals(expected, ability)) return false;
            _pending.Remove(command);
            _active = null;
            return true;
        }

        internal void EndAction(TCommand command)
        {
            if (ReferenceEquals(_active, command)) _active = null;
        }

        internal bool Cancel(TCommand command)
        {
            if (command == null) return false;
            EndAction(command);
            return _pending.Remove(command);
        }

        private sealed class ReferenceComparer<T> : IEqualityComparer<T> where T : class
        {
            internal static readonly ReferenceComparer<T> Instance = new ReferenceComparer<T>();
            public bool Equals(T x, T y) { return ReferenceEquals(x, y); }
            public int GetHashCode(T value) { return RuntimeHelpers.GetHashCode(value); }
        }
    }
}
