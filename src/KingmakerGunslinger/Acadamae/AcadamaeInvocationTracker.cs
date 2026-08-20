using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace KingmakerGunslinger.Acadamae
{
    internal sealed class AcadamaeInvocationTracker<TCommand, TAbility, TRule>
        where TCommand : class where TAbility : class where TRule : class
    {
        private sealed class Entry
        {
            internal TCommand Command;
            internal TAbility Ability;
            internal TRule Rule;
        }

        private readonly Dictionary<TCommand, Entry> _commands =
            new Dictionary<TCommand, Entry>(ReferenceComparer<TCommand>.Instance);
        private readonly Dictionary<TRule, Entry> _rules =
            new Dictionary<TRule, Entry>(ReferenceComparer<TRule>.Instance);
        [ThreadStatic] private static TCommand _active;

        internal int Count { get { return _commands.Count; } }

        internal bool Arm(TCommand command, TAbility ability)
        {
            if (command == null) throw new ArgumentNullException("command");
            if (ability == null) throw new ArgumentNullException("ability");
            if (_commands.ContainsKey(command)) return false;
            _commands.Add(command, new Entry {
                Command = command,
                Ability = ability
            });
            return true;
        }

        internal bool Begin(TCommand command)
        {
            if (command == null || !_commands.ContainsKey(command)) return false;
            _active = command;
            return true;
        }

        internal bool AttachRule(TRule rule, TAbility ability)
        {
            TCommand command = _active;
            Entry entry;
            if (command == null || rule == null || ability == null ||
                !_commands.TryGetValue(command, out entry) ||
                entry.Rule != null || _rules.ContainsKey(rule) ||
                !ReferenceEquals(entry.Ability, ability)) return false;
            entry.Rule = rule;
            _rules.Add(rule, entry);
            _active = null;
            return true;
        }

        internal bool Consume(TRule rule, TAbility ability)
        {
            Entry entry;
            if (rule == null || ability == null ||
                !_rules.TryGetValue(rule, out entry) ||
                !ReferenceEquals(entry.Ability, ability)) return false;
            Release(entry);
            return true;
        }

        internal void EndAction(TCommand command)
        {
            if (ReferenceEquals(_active, command)) _active = null;
        }

        internal bool Cancel(TCommand command)
        {
            Entry entry;
            if (command == null || !_commands.TryGetValue(command, out entry))
                return false;
            EndAction(command);
            Release(entry);
            return true;
        }

        private void Release(Entry entry)
        {
            _commands.Remove(entry.Command);
            if (entry.Rule != null) _rules.Remove(entry.Rule);
        }

        private sealed class ReferenceComparer<T> : IEqualityComparer<T> where T : class
        {
            internal static readonly ReferenceComparer<T> Instance = new ReferenceComparer<T>();
            public bool Equals(T x, T y) { return ReferenceEquals(x, y); }
            public int GetHashCode(T value) { return RuntimeHelpers.GetHashCode(value); }
        }
    }
}
