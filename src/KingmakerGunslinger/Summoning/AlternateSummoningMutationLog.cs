using System;
using System.Collections.Generic;

namespace KingmakerGunslinger.Summoning
{
    /// <summary>
    /// The exact before/after values of one family's host mutations. Each
    /// write is verified as retained; rollback restores in reverse order and
    /// refuses to overwrite a value some other mutation has changed since,
    /// rather than silently discarding that change.
    /// </summary>
    internal sealed class AlternateSummoningMutationLog<T> where T : class
    {
        private readonly List<Entry> _entries = new List<Entry>();

        internal int Count { get { return _entries.Count; } }

        internal void Apply(string key, Func<T> read, Action<T> write, T after)
        {
            if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException("A key is required.", "key");
            if (read == null) throw new ArgumentNullException("read");
            if (write == null) throw new ArgumentNullException("write");
            if (after == null) throw new ArgumentNullException("after");
            T before = read();
            if (ReferenceEquals(before, after))
                throw new InvalidOperationException("A mutation must replace its value: " + key);
            _entries.Add(new Entry(key, read, write, before, after));
            write(after);
            if (!ReferenceEquals(read(), after))
                throw new InvalidOperationException("A host mutation was not retained: " + key);
        }

        internal void Rollback()
        {
            for (int index = _entries.Count - 1; index >= 0; index--)
            {
                Entry entry = _entries[index];
                T current = entry.Read();
                if (ReferenceEquals(current, entry.Before))
                {
                    _entries.RemoveAt(index);
                    continue;
                }
                if (!ReferenceEquals(current, entry.After))
                    throw new InvalidOperationException(
                        "Rollback refused after unrelated mutation: " + entry.Key);
                entry.Write(entry.Before);
                _entries.RemoveAt(index);
            }
        }

        private sealed class Entry
        {
            internal Entry(string key, Func<T> read, Action<T> write, T before, T after)
            { Key = key; Read = read; Write = write; Before = before; After = after; }
            internal string Key { get; private set; }
            internal Func<T> Read { get; private set; }
            internal Action<T> Write { get; private set; }
            internal T Before { get; private set; }
            internal T After { get; private set; }
        }
    }
}
