using System;
using System.Collections.Generic;
using System.Linq;

namespace KingmakerGunslinger.FavoredClass
{
    /// <summary>
    /// O01: the live areas of one owner's performance, widened only as a
    /// whole, so every live area and every description of it agree (all
    /// widened, or all native). A member may attempt widening only while
    /// every other live member is widened; a member recorded widened while a
    /// native sibling is live is rolled back. A member that ends up native
    /// (failed, deferred, held or rolled back) must be verified native and
    /// narrows every widened sibling; a member or sibling that cannot be
    /// verified native is ended and forgotten, never kept live with a state
    /// the descriptions do not show.
    /// </summary>
    internal sealed class FavoredClassRangeGroup<TMember> where TMember : class
    {
        private sealed class Entry
        {
            internal TMember Member;
            internal FavoredClassWideningOutcome Outcome;
            internal int Feet;
        }

        private readonly List<Entry> _entries = new List<Entry>();

        internal int Count
        {
            get { return _entries.Count; }
        }

        internal FavoredClassWideningOutcome? OutcomeOf(TMember member)
        {
            Entry entry = Find(member);
            return entry == null ? (FavoredClassWideningOutcome?)null : entry.Outcome;
        }

        /// <summary>Forgets members that are no longer live.</summary>
        internal void Purge(Func<TMember, bool> isLive)
        {
            if (isLive == null) throw new ArgumentNullException("isLive");
            _entries.RemoveAll(entry => !Safe(isLive, entry.Member));
        }

        /// <summary>Whether a member may attempt widening: every other live member is widened.</summary>
        internal bool MayWiden(TMember member)
        {
            return _entries.All(entry => ReferenceEquals(entry.Member, member) ||
                FavoredClassRangePresentation.IsWidened(entry.Outcome));
        }

        /// <summary>
        /// Records a member's outcome and restores the group invariant.
        /// verifyNative reports whether a member is native (radius and ring);
        /// narrow restores a widened member and reports whether it is then
        /// verified native; end ends a member that cannot be made native.
        /// </summary>
        internal void Record(TMember member, FavoredClassWideningOutcome outcome, int feet,
            Func<TMember, bool> verifyNative, Func<TMember, bool> narrow, Action<TMember> end)
        {
            if (member == null) throw new ArgumentNullException("member");
            if (verifyNative == null) throw new ArgumentNullException("verifyNative");
            if (narrow == null) throw new ArgumentNullException("narrow");
            if (end == null) throw new ArgumentNullException("end");
            Entry entry = Find(member);
            if (entry == null)
            {
                entry = new Entry { Member = member };
                _entries.Add(entry);
            }
            entry.Outcome = outcome;
            entry.Feet = feet;
            if (FavoredClassRangePresentation.IsWidened(outcome))
            {
                if (_entries.All(value => ReferenceEquals(value, entry) ||
                    FavoredClassRangePresentation.IsWidened(value.Outcome)))
                    return;
                // A native sibling is live: this member is rolled back.
                Settle(entry, narrow, end);
                return;
            }
            if (!Safe(verifyNative, member))
            {
                Safe(end, member);
                _entries.Remove(entry);
            }
            foreach (Entry sibling in _entries.ToArray())
                if (!ReferenceEquals(sibling, entry) && FavoredClassRangePresentation.IsWidened(sibling.Outcome))
                    Settle(sibling, narrow, end);
        }

        /// <summary>The range the owner's descriptions show (FavoredClassRangePresentation.Feet).</summary>
        internal int? Feet(int? configuredFeet)
        {
            return FavoredClassRangePresentation.Feet(_entries.Select(entry =>
                new FavoredClassLiveRange(entry.Outcome, entry.Feet)).ToList(), configuredFeet);
        }

        private void Settle(Entry entry, Func<TMember, bool> narrow, Action<TMember> end)
        {
            if (Safe(narrow, entry.Member))
            {
                entry.Outcome = FavoredClassWideningOutcome.Narrowed;
                return;
            }
            Safe(end, entry.Member);
            _entries.Remove(entry);
        }

        private Entry Find(TMember member)
        {
            return _entries.FirstOrDefault(entry => ReferenceEquals(entry.Member, member));
        }

        private static bool Safe(Func<TMember, bool> check, TMember member)
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

        private static void Safe(Action<TMember> action, TMember member)
        {
            try
            {
                action(member);
            }
            catch (Exception)
            {
                // The member is forgotten either way.
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
