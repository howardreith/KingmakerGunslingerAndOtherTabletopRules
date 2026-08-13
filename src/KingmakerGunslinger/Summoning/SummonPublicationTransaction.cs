using System;
using System.Collections.Generic;

namespace KingmakerGunslinger.Summoning
{
    internal sealed class SummonPublicationTarget<T> where T : class
    {
        internal SummonPublicationTarget(string key, Func<IList<T>> read,
            Action<IList<T>> write, IEnumerable<T> additions)
        { Key = key; Read = read; Write = write; Additions = additions; }
        internal string Key { get; private set; }
        internal Func<IList<T>> Read { get; private set; }
        internal Action<IList<T>> Write { get; private set; }
        internal IEnumerable<T> Additions { get; private set; }
    }

    internal static class SummonPublicationTransaction
    {
        internal static void Publish<T>(IEnumerable<SummonPublicationTarget<T>> targets,
            Func<T, string> guid) where T : class
        {
            if (targets == null) throw new ArgumentNullException("targets");
            var changed = new List<Record<T>>();
            try
            {
                foreach (SummonPublicationTarget<T> target in targets)
                {
                    if (target == null || string.IsNullOrWhiteSpace(target.Key) ||
                        target.Read == null || target.Write == null)
                        throw new InvalidOperationException("Publication target is malformed.");
                    IList<T> before = target.Read();
                    IList<T> after = SummonVariantMergePolicy.Merge(before,
                        target.Additions, guid);
                    if (ReferenceEquals(before, after)) continue;
                    changed.Add(new Record<T>(target, before, after));
                    target.Write(after);
                    if (!ReferenceEquals(target.Read(), after))
                        throw new InvalidOperationException("Publication write was not retained: " + target.Key);
                }
            }
            catch
            {
                for (int index = changed.Count - 1; index >= 0; index--)
                {
                    Record<T> record = changed[index];
                    IList<T> current = record.Target.Read();
                    if (ReferenceEquals(current, record.Before)) continue;
                    if (!ReferenceEquals(current, record.After))
                        throw new InvalidOperationException("Rollback refused after unrelated mutation: " + record.Target.Key);
                    record.Target.Write(record.Before);
                }
                throw;
            }
        }

        private sealed class Record<T> where T : class
        {
            internal Record(SummonPublicationTarget<T> target, IList<T> before, IList<T> after)
            { Target = target; Before = before; After = after; }
            internal SummonPublicationTarget<T> Target;
            internal IList<T> Before;
            internal IList<T> After;
        }
    }
}
