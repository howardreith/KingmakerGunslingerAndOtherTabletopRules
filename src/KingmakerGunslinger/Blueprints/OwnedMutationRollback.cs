using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace KingmakerGunslinger.Blueprints
{
    internal enum OwnedMutationRollbackStatus
    {
        /// <summary>The slot held the published reference and now holds the captured original.</summary>
        Restored = 0,

        /// <summary>The slot already held the captured original; nothing was written.</summary>
        AlreadyOriginal = 1,

        /// <summary>The slot holds a reference this transaction neither captured nor published; it was left in place.</summary>
        ForeignChangePreserved = 2,

        /// <summary>Reading, writing or verifying the slot failed.</summary>
        Failed = 3
    }

    internal sealed class OwnedMutationRollbackEntry
    {
        internal OwnedMutationRollbackEntry(string name,
            OwnedMutationRollbackStatus status, string detail)
        {
            Name = name; Status = status; Detail = detail ?? string.Empty;
        }

        internal string Name { get; private set; }
        internal OwnedMutationRollbackStatus Status { get; private set; }
        internal string Detail { get; private set; }

        internal bool IsResolved
        {
            get
            {
                return Status == OwnedMutationRollbackStatus.Restored ||
                    Status == OwnedMutationRollbackStatus.AlreadyOriginal;
            }
        }
    }

    internal sealed class OwnedMutationRollbackReport
    {
        internal OwnedMutationRollbackReport(
            IEnumerable<OwnedMutationRollbackEntry> entries)
        {
            Entries = new List<OwnedMutationRollbackEntry>(entries).AsReadOnly();
        }

        internal IReadOnlyList<OwnedMutationRollbackEntry> Entries { get; private set; }

        internal int ResolvedCount { get { return Entries.Count(entry => entry.IsResolved); } }

        internal int UnresolvedCount { get { return Entries.Count - ResolvedCount; } }

        /// <summary>True only when every owned mutation is back to its captured original.</summary>
        internal bool IsComplete { get { return UnresolvedCount == 0; } }

        internal string Describe()
        {
            string unresolved = string.Join("|", Entries
                .Where(entry => !entry.IsResolved)
                .Select(entry => entry.Name + ":" + Token(entry.Status) +
                    (entry.Detail.Length == 0 ? string.Empty : "(" + entry.Detail + ")"))
                .ToArray());
            return string.Format(CultureInfo.InvariantCulture,
                "owned-mutations={0};restored={1};already-original={2};unresolved={3};unresolved-detail={4}",
                Entries.Count,
                Entries.Count(entry => entry.Status == OwnedMutationRollbackStatus.Restored),
                Entries.Count(entry => entry.Status == OwnedMutationRollbackStatus.AlreadyOriginal),
                UnresolvedCount,
                unresolved.Length == 0 ? "<none>" : unresolved);
        }

        private static string Token(OwnedMutationRollbackStatus status)
        {
            switch (status)
            {
                case OwnedMutationRollbackStatus.ForeignChangePreserved:
                    return "foreign-change-preserved";
                case OwnedMutationRollbackStatus.Failed: return "failed";
                case OwnedMutationRollbackStatus.AlreadyOriginal: return "already-original";
                default: return "restored";
            }
        }
    }

    /// <summary>
    /// Exact restoration of reference slots one transaction replaced. Restoring writes back the captured original
    /// object itself, whatever it is: null, or a value that displays as nothing, was the installation's own state and
    /// is not validated as if it were newly published content. A slot is written only while it still holds the
    /// reference this transaction published, so a later change by another mod is never overwritten. Every entry is
    /// attempted; one conflict or failure does not stop the others.
    /// </summary>
    internal sealed class OwnedMutationRollback
    {
        private readonly List<OwnedMutationRollbackEntry> _entries =
            new List<OwnedMutationRollbackEntry>();

        internal void Restore<T>(string name, Func<T> read, Action<T> write,
            T published, T original) where T : class
        {
            if (read == null) throw new ArgumentNullException("read");
            if (write == null) throw new ArgumentNullException("write");
            string label = string.IsNullOrWhiteSpace(name) ? "<unnamed>" : name;
            try
            {
                T current = read();
                if (ReferenceEquals(current, published) &&
                    !ReferenceEquals(published, original))
                {
                    write(original);
                    if (ReferenceEquals(read(), original))
                        Add(label, OwnedMutationRollbackStatus.Restored, null);
                    else
                        Add(label, OwnedMutationRollbackStatus.Failed,
                            "the restored reference did not verify");
                }
                else if (ReferenceEquals(current, original))
                    Add(label, OwnedMutationRollbackStatus.AlreadyOriginal, null);
                else
                    Add(label, OwnedMutationRollbackStatus.ForeignChangePreserved,
                        "changed after publication");
            }
            catch (Exception exception)
            {
                Add(label, OwnedMutationRollbackStatus.Failed,
                    exception.GetType().Name + ": " + exception.Message);
            }
        }

        internal OwnedMutationRollbackReport Complete()
        {
            return new OwnedMutationRollbackReport(_entries);
        }

        private void Add(string name, OwnedMutationRollbackStatus status, string detail)
        {
            _entries.Add(new OwnedMutationRollbackEntry(name, status, detail));
        }
    }
}
