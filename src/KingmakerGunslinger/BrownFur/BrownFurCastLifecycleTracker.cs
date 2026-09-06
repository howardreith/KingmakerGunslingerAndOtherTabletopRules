using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace KingmakerGunslinger.BrownFur
{
    internal sealed class BrownFurCastLifecycleTracker<TCommand, TAbility,
        TRule, TContext, TProcess>
        where TCommand : class where TAbility : class where TRule : class
        where TContext : class where TProcess : class
    {
        private sealed class Entry
        {
            internal TCommand Command;
            internal TAbility Ability;
            internal TRule Rule;
            internal TContext Context;
            internal TProcess Process;
            internal BrownFurCastTransaction Transaction;
        }

        private readonly object _gate = new object();
        private readonly Dictionary<TCommand, Entry> _commands =
            new Dictionary<TCommand, Entry>(ReferenceComparer<TCommand>.Instance);
        private readonly Dictionary<TAbility, Entry> _abilities =
            new Dictionary<TAbility, Entry>(ReferenceComparer<TAbility>.Instance);
        private readonly Dictionary<TRule, Entry> _rules =
            new Dictionary<TRule, Entry>(ReferenceComparer<TRule>.Instance);
        private readonly Dictionary<TContext, Entry> _contexts =
            new Dictionary<TContext, Entry>(ReferenceComparer<TContext>.Instance);
        private readonly Dictionary<TProcess, Entry> _processes =
            new Dictionary<TProcess, Entry>(ReferenceComparer<TProcess>.Instance);
        private readonly Action<BrownFurCastTransaction> _onRelease;

        internal BrownFurCastLifecycleTracker() : this(null) { }

        internal BrownFurCastLifecycleTracker(
            Action<BrownFurCastTransaction> onRelease)
        { _onRelease = onRelease; }

        internal int ActiveTransactionCount
        { get { lock (_gate) return _abilities.Count; } }

        internal bool Begin(TCommand command, TAbility ability,
            BrownFurCastTransaction transaction)
        {
            if (command == null || ability == null || transaction == null ||
                transaction.State != BrownFurCastTransactionState.Validated)
                return false;
            lock (_gate)
            {
                if (_commands.ContainsKey(command) ||
                    _abilities.ContainsKey(ability)) return false;
                var entry = new Entry { Command = command, Ability = ability,
                    Transaction = transaction };
                _commands.Add(command, entry);
                _abilities.Add(ability, entry);
                return true;
            }
        }

        internal bool BeginDirect(TAbility ability,
            BrownFurCastTransaction transaction)
        {
            if (ability == null || transaction == null ||
                transaction.State != BrownFurCastTransactionState.Validated)
                return false;
            lock (_gate)
            {
                if (_abilities.ContainsKey(ability)) return false;
                _abilities.Add(ability, new Entry { Ability = ability,
                    Transaction = transaction });
                return true;
            }
        }

        internal bool AttachRule(TAbility ability, TRule rule,
            TContext context)
        {
            if (ability == null || rule == null || context == null) return false;
            lock (_gate)
            {
                Entry entry;
                if (!_abilities.TryGetValue(ability, out entry) ||
                    entry.Rule != null || _rules.ContainsKey(rule) ||
                    _contexts.ContainsKey(context)) return false;
                entry.Rule = rule;
                entry.Context = context;
                _rules.Add(rule, entry);
                _contexts.Add(context, entry);
                return true;
            }
        }

        internal bool AttachProcess(TRule rule, TProcess process)
        {
            if (rule == null || process == null) return false;
            lock (_gate)
            {
                Entry entry;
                if (!_rules.TryGetValue(rule, out entry) ||
                    entry.Process != null || _processes.ContainsKey(process))
                    return false;
                entry.Process = process;
                _processes.Add(process, entry);
                return true;
            }
        }

        internal bool Commit(TAbility ability, Func<int, bool> tryDebitExactly)
        {
            if (ability == null || tryDebitExactly == null) return false;
            lock (_gate)
            {
                Entry entry;
                if (!_abilities.TryGetValue(ability, out entry)) return false;
                BrownFurCastTransactionState before = entry.Transaction.State;
                if (entry.Transaction.Commit(tryDebitExactly)) return true;
                if (before == BrownFurCastTransactionState.Validated &&
                    entry.Transaction.State ==
                        BrownFurCastTransactionState.Rejected)
                    Release(entry);
                return false;
            }
        }

        internal bool EndCommand(TCommand command, bool interrupted)
        {
            if (command == null) return false;
            lock (_gate)
            {
                Entry entry;
                if (!_commands.TryGetValue(command, out entry)) return false;
                BrownFurCastTransactionState state = entry.Transaction.State;
                if (state == BrownFurCastTransactionState.Created ||
                    state == BrownFurCastTransactionState.Validated)
                {
                    entry.Transaction.Cancel();
                    Release(entry);
                }
                else if (state == BrownFurCastTransactionState.Committed &&
                    interrupted)
                {
                    entry.Transaction.Interrupt();
                    if (entry.Process == null) Release(entry);
                }
                else if (state == BrownFurCastTransactionState.Committed &&
                    entry.Process == null)
                {
                    entry.Transaction.Complete();
                    Release(entry);
                }
                else if (state == BrownFurCastTransactionState.Rejected ||
                    state == BrownFurCastTransactionState.Failed ||
                    state == BrownFurCastTransactionState.Cancelled ||
                    state == BrownFurCastTransactionState.Interrupted ||
                    state == BrownFurCastTransactionState.Completed)
                    Release(entry);
                return true;
            }
        }

        internal bool CompleteDirect(TAbility ability)
        {
            if (ability == null) return false;
            lock (_gate)
            {
                Entry entry;
                if (!_abilities.TryGetValue(ability, out entry) ||
                    entry.Command != null || entry.Process != null ||
                    entry.Transaction.State !=
                        BrownFurCastTransactionState.Committed) return false;
                entry.Transaction.Complete();
                Release(entry);
                return true;
            }
        }

        internal bool CancelDirect(TAbility ability)
        {
            if (ability == null) return false;
            lock (_gate)
            {
                Entry entry;
                if (!_abilities.TryGetValue(ability, out entry) ||
                    entry.Command != null) return false;
                BrownFurCastTransactionState state = entry.Transaction.State;
                if (state != BrownFurCastTransactionState.Created &&
                    state != BrownFurCastTransactionState.Validated)
                    return false;
                entry.Transaction.Cancel();
                Release(entry);
                return true;
            }
        }

        internal bool FailDirect(TAbility ability)
        {
            if (ability == null) return false;
            lock (_gate)
            {
                Entry entry;
                if (!_abilities.TryGetValue(ability, out entry) ||
                    entry.Command != null) return false;
                entry.Transaction.Fail();
                Release(entry);
                return true;
            }
        }

        internal bool DirectProcessAttached(TAbility ability)
        {
            if (ability == null) return false;
            lock (_gate)
            {
                Entry entry;
                return _abilities.TryGetValue(ability, out entry) &&
                    entry.Command == null && entry.Process != null;
            }
        }

        internal bool ProcessTerminal(TProcess process, bool failed)
        {
            if (process == null) return false;
            lock (_gate)
            {
                Entry entry;
                if (!_processes.TryGetValue(process, out entry)) return false;
                if (entry.Transaction.State ==
                    BrownFurCastTransactionState.Committed)
                {
                    if (failed) entry.Transaction.Fail();
                    else entry.Transaction.Complete();
                }
                Release(entry);
                return true;
            }
        }

        internal bool TryGetByContext(TContext context,
            out BrownFurCastTransaction transaction)
        {
            transaction = null;
            if (context == null) return false;
            lock (_gate)
            {
                Entry entry;
                if (!_contexts.TryGetValue(context, out entry)) return false;
                transaction = entry.Transaction;
                return true;
            }
        }

        internal bool TryGetByAbility(TAbility ability,
            out BrownFurCastTransaction transaction)
        {
            transaction = null;
            if (ability == null) return false;
            lock (_gate)
            {
                Entry entry;
                if (!_abilities.TryGetValue(ability, out entry)) return false;
                transaction = entry.Transaction;
                return true;
            }
        }

        internal bool TryGetByRule(TRule rule,
            out BrownFurCastTransaction transaction)
        {
            transaction = null;
            if (rule == null) return false;
            lock (_gate)
            {
                Entry entry;
                if (!_rules.TryGetValue(rule, out entry)) return false;
                transaction = entry.Transaction;
                return true;
            }
        }

        internal bool FailRule(TRule rule)
        {
            if (rule == null) return false;
            lock (_gate)
            {
                Entry entry;
                if (!_rules.TryGetValue(rule, out entry)) return false;
                entry.Transaction.Fail();
                Release(entry);
                return true;
            }
        }

        internal void Clear()
        {
            lock (_gate)
            {
                var failures = new List<Exception>();
                foreach (Entry entry in new List<Entry>(_abilities.Values))
                {
                    if (entry.Transaction.State ==
                        BrownFurCastTransactionState.Committed)
                        entry.Transaction.Interrupt();
                    else if (entry.Transaction.State ==
                        BrownFurCastTransactionState.Created ||
                        entry.Transaction.State ==
                            BrownFurCastTransactionState.Validated)
                        entry.Transaction.Cancel();
                    try { Release(entry); }
                    catch (Exception exception) { failures.Add(exception); }
                }
                if (failures.Count != 0) throw new AggregateException(
                    "One or more Brown-Fur lifecycle release callbacks failed.",
                    failures);
            }
        }

        private void Release(Entry entry)
        {
            if (entry.Command != null) _commands.Remove(entry.Command);
            _abilities.Remove(entry.Ability);
            if (entry.Rule != null) _rules.Remove(entry.Rule);
            if (entry.Context != null) _contexts.Remove(entry.Context);
            if (entry.Process != null) _processes.Remove(entry.Process);
            if (_onRelease != null) _onRelease(entry.Transaction);
        }

        private sealed class ReferenceComparer<T> : IEqualityComparer<T>
            where T : class
        {
            internal static readonly ReferenceComparer<T> Instance =
                new ReferenceComparer<T>();
            public bool Equals(T left, T right)
            { return ReferenceEquals(left, right); }
            public int GetHashCode(T value)
            { return RuntimeHelpers.GetHashCode(value); }
        }
    }
}
