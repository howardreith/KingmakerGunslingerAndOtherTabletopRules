using System;
using System.Collections.Generic;
using System.Linq;

namespace KingmakerGunslinger.FavoredClass
{
    /// <summary>
    /// O01: the live areas of one owner's performance, widened only as a
    /// whole, so every live area and every description of it agree (all
    /// widened, or all native). A member may attempt widening only while
    /// every other member is widened and none is unresolved; a member
    /// recorded widened beside a native or unresolved sibling is rolled back.
    /// A member that ends up native (failed, deferred, held or rolled back)
    /// must be verified native and narrows every widened sibling; a member or
    /// sibling that cannot be verified native is ended.
    /// A member is forgotten only when its liveness probe positively reports
    /// it no longer live. An ending that throws, does nothing or cannot be
    /// verified, and a liveness probe that throws, keep the member tracked as
    /// unresolved: while any member is unresolved the group allows no
    /// widening and its descriptions stay native, a diagnostic is reported,
    /// and the next mechanics lifecycle point (Settle) retries normalizing or
    /// ending it. The read-only lifecycle points (Purge) never narrow or end.
    /// </summary>
    internal sealed class FavoredClassRangeGroup<TMember> where TMember : class
    {
        private sealed class Entry
        {
            internal TMember Member;
            internal FavoredClassWideningOutcome Outcome;
            internal int Feet;

            /// <summary>Why the member's actual state may differ from its outcome (neither verified native nor verified ended), or null.</summary>
            internal string StateProblem;

            /// <summary>Why the member had to be ended (the retry keeps the same reason).</summary>
            internal string Reason;

            /// <summary>Why the member's liveness could not be read, or null.</summary>
            internal string LivenessProblem;

            internal bool Unresolved
            {
                get { return StateProblem != null || LivenessProblem != null; }
            }
        }

        private readonly List<Entry> _entries = new List<Entry>();
        private readonly Func<TMember, bool> _isLive;
        private readonly Func<TMember, bool> _verifyNative;
        private readonly Func<TMember, bool> _narrow;
        private readonly Action<TMember> _end;
        private readonly Action<TMember, string> _diagnostic;

        /// <summary>
        /// isLive reports whether a member is still live (it may throw);
        /// verifyNative reports whether a member is native (radius and ring);
        /// narrow restores a widened member and reports whether it is then
        /// verified native; end attempts to end a member that cannot be made
        /// native (it may throw or do nothing: the ending is verified with
        /// isLive); diagnostic receives each unresolved and resolved member.
        /// </summary>
        internal FavoredClassRangeGroup(Func<TMember, bool> isLive, Func<TMember, bool> verifyNative,
            Func<TMember, bool> narrow, Action<TMember> end, Action<TMember, string> diagnostic)
        {
            if (isLive == null) throw new ArgumentNullException("isLive");
            if (verifyNative == null) throw new ArgumentNullException("verifyNative");
            if (narrow == null) throw new ArgumentNullException("narrow");
            if (end == null) throw new ArgumentNullException("end");
            _isLive = isLive;
            _verifyNative = verifyNative;
            _narrow = narrow;
            _end = end;
            _diagnostic = diagnostic;
        }

        internal int Count
        {
            get { return _entries.Count; }
        }

        /// <summary>Tracked members whose state or liveness is not verified.</summary>
        internal int UnresolvedCount
        {
            get { return _entries.Count(entry => entry.Unresolved); }
        }

        internal bool Contains(TMember member)
        {
            return Find(member) != null;
        }

        internal FavoredClassWideningOutcome? OutcomeOf(TMember member)
        {
            Entry entry = Find(member);
            return entry == null ? (FavoredClassWideningOutcome?)null : entry.Outcome;
        }

        /// <summary>Why a tracked member is unresolved, or null (evidence).</summary>
        internal string ProblemOf(TMember member)
        {
            Entry entry = Find(member);
            return entry == null ? null : entry.StateProblem ?? entry.LivenessProblem;
        }

        /// <summary>
        /// Read-only lifecycle point: forgets the members whose liveness probe
        /// reports them no longer live. A probe that throws keeps its member
        /// tracked as unresolved; a readable probe clears that problem.
        /// </summary>
        internal void Purge()
        {
            foreach (Entry entry in _entries.ToArray())
            {
                Exception error;
                bool? live = Probe(entry.Member, out error);
                if (live == false)
                {
                    Forget(entry, "no longer live");
                    continue;
                }
                if (live == null)
                {
                    SetLivenessProblem(entry, "its liveness probe threw " + Describe(error));
                    continue;
                }
                if (entry.LivenessProblem != null)
                {
                    entry.LivenessProblem = null;
                    if (!entry.Unresolved)
                        Report(entry.Member, "resolved: its liveness is readable again");
                }
            }
        }

        /// <summary>
        /// Mechanics lifecycle point: Purge, then retry every live member whose
        /// state is unresolved: narrowed and verified native, or else ended and
        /// verified no longer live.
        /// </summary>
        internal void Settle()
        {
            Purge();
            foreach (Entry entry in _entries.ToArray())
            {
                if (entry.StateProblem == null || entry.LivenessProblem != null || !_entries.Contains(entry))
                    continue;
                if (Check(_narrow, entry.Member))
                {
                    if (FavoredClassRangePresentation.IsWidened(entry.Outcome))
                        entry.Outcome = FavoredClassWideningOutcome.Narrowed;
                    entry.StateProblem = null;
                    entry.Reason = null;
                    Report(entry.Member, "resolved: narrowed and verified native");
                    NarrowWidenedSiblings(entry);
                    continue;
                }
                EndOrKeep(entry, entry.Reason ?? "unresolved");
            }
        }

        /// <summary>Whether a member may attempt widening: no member is unresolved and every other member is widened.</summary>
        internal bool MayWiden(TMember member)
        {
            return _entries.All(entry => !entry.Unresolved && (ReferenceEquals(entry.Member, member) ||
                FavoredClassRangePresentation.IsWidened(entry.Outcome)));
        }

        /// <summary>Records a member's outcome (a mechanics lifecycle point: Settle first) and restores the group invariant.</summary>
        internal void Record(TMember member, FavoredClassWideningOutcome outcome, int feet)
        {
            if (member == null) throw new ArgumentNullException("member");
            Settle();
            Entry entry = Find(member);
            if (entry == null)
            {
                entry = new Entry { Member = member };
                _entries.Add(entry);
            }
            entry.Outcome = outcome;
            entry.Feet = feet;
            // A fresh outcome for this member; it is verified again below.
            bool wasUnresolved = entry.StateProblem != null;
            entry.StateProblem = null;
            entry.Reason = null;
            if (FavoredClassRangePresentation.IsWidened(outcome))
            {
                if (entry.LivenessProblem != null || !_entries.All(value => ReferenceEquals(value, entry) ||
                    (!value.Unresolved && FavoredClassRangePresentation.IsWidened(value.Outcome))))
                    // A native or unresolved sibling (or this member's own
                    // unreadable liveness): this member is rolled back.
                    Rollback(entry);
            }
            else
            {
                if (!Check(_verifyNative, member))
                    EndOrKeep(entry, "recorded " + outcome + " but not verified native");
                NarrowWidenedSiblings(entry);
            }
            if (wasUnresolved && entry.StateProblem == null)
                Report(member, _entries.Contains(entry) ? "resolved: recorded " + entry.Outcome + " and verified" :
                    "resolved: ended and verified no longer live");
        }

        /// <summary>
        /// The range the owner's descriptions show (FavoredClassRangePresentation.Feet):
        /// native while any member is unresolved, by its state or its liveness.
        /// </summary>
        internal int? Feet(int? configuredFeet)
        {
            if (_entries.Any(entry => entry.Unresolved))
                return null;
            return FavoredClassRangePresentation.Feet(_entries.Select(entry =>
                new FavoredClassLiveRange(entry.Outcome, entry.Feet)).ToList(), configuredFeet);
        }

        private void NarrowWidenedSiblings(Entry native)
        {
            foreach (Entry sibling in _entries.ToArray())
                if (!ReferenceEquals(sibling, native) && _entries.Contains(sibling) &&
                    FavoredClassRangePresentation.IsWidened(sibling.Outcome))
                    Rollback(sibling);
        }

        private void Rollback(Entry entry)
        {
            if (Check(_narrow, entry.Member))
            {
                entry.Outcome = FavoredClassWideningOutcome.Narrowed;
                if (entry.StateProblem != null)
                {
                    entry.StateProblem = null;
                    entry.Reason = null;
                    Report(entry.Member, "resolved: narrowed and verified native");
                }
                return;
            }
            EndOrKeep(entry, "its narrowing was not verified native");
        }

        /// <summary>Ends a member; forgets it only when it is then verified no longer live, else keeps it unresolved.</summary>
        private void EndOrKeep(Entry entry, string why)
        {
            Exception ending = null;
            try
            {
                _end(entry.Member);
            }
            catch (Exception exception)
            {
                ending = exception;
            }
            Exception error;
            bool? live = Probe(entry.Member, out error);
            if (live == false)
            {
                Forget(entry, "ended (" + why + ")");
                return;
            }
            if (live == null)
                SetLivenessProblem(entry, "its liveness probe threw " + Describe(error));
            string problem = why + "; " + (ending != null ? "ending it threw " + Describe(ending) :
                live == null ? "its ending could not be verified" : "it is still live after ending");
            entry.Reason = why;
            if (problem != entry.StateProblem)
            {
                entry.StateProblem = problem;
                Report(entry.Member, "unresolved: " + problem);
            }
        }

        private void Forget(Entry entry, string how)
        {
            _entries.Remove(entry);
            if (entry.Unresolved)
                Report(entry.Member, "resolved: " + how);
        }

        private void SetLivenessProblem(Entry entry, string problem)
        {
            if (problem == entry.LivenessProblem)
                return;
            entry.LivenessProblem = problem;
            Report(entry.Member, "unresolved: " + problem);
        }

        private bool? Probe(TMember member, out Exception error)
        {
            error = null;
            try
            {
                return _isLive(member);
            }
            catch (Exception exception)
            {
                error = exception;
                return null;
            }
        }

        private void Report(TMember member, string message)
        {
            Action<TMember, string> diagnostic = _diagnostic;
            if (diagnostic == null)
                return;
            try
            {
                diagnostic(member, message);
            }
            catch (Exception)
            {
                // A diagnostic never changes the group.
            }
        }

        private static string Describe(Exception exception)
        {
            return exception == null ? "" : exception.GetType().Name + ": " + exception.Message;
        }

        private Entry Find(TMember member)
        {
            return _entries.FirstOrDefault(entry => ReferenceEquals(entry.Member, member));
        }

        private static bool Check(Func<TMember, bool> check, TMember member)
        {
            try
            {
                return check(member);
            }
            catch (Exception)
            {
                return false;
            }
        }
    }

    /// <summary>
    /// O01: a performance's area runs under a clone of its buff's context
    /// (AreaEffectsController.Spawn: parentContext.CloneFor), so the toggle
    /// whose own current buff runs an area is found among the ancestors of
    /// the area's context, never by the area's own context.
    /// </summary>
    internal static class FavoredClassContextLineage
    {
        private const int MaxDepth = 64;

        /// <summary>Whether ancestor is the context itself or one of its ancestors (bounded, so a malformed chain ends).</summary>
        internal static bool Descends<TContext>(TContext context, TContext ancestor, Func<TContext, TContext> parent)
            where TContext : class
        {
            if (parent == null) throw new ArgumentNullException("parent");
            if (ancestor == null)
                return false;
            TContext cursor = context;
            for (int depth = 0; cursor != null && depth < MaxDepth; depth++)
            {
                if (ReferenceEquals(cursor, ancestor))
                    return true;
                cursor = parent(cursor);
            }
            return false;
        }
    }
}
