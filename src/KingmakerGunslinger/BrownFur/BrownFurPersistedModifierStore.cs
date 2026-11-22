using System;
using System.Collections.Generic;
using System.Linq;

namespace KingmakerGunslinger.BrownFur
{
    /// <summary>
    /// The persisted Powerful Change records of one unit, split into the active records that can restore an increase
    /// and the retained entries kept only as evidence (RISK-03, A2).
    /// </summary>
    internal sealed class BrownFurPersistedModifierState
    {
        internal BrownFurPersistedModifierState(
            List<BrownFurPersistedModifierRecord> active,
            List<BrownFurPersistedModifierRecord> retained,
            IList<KeyValuePair<string, BrownFurPersistedModifierRecord>> newlyRejected)
        {
            Active = active;
            Retained = retained;
            NewlyRejected = newlyRejected ??
                new KeyValuePair<string, BrownFurPersistedModifierRecord>[0];
        }

        /// <summary>Strictly valid records, at most one per logical modifier.</summary>
        internal List<BrownFurPersistedModifierRecord> Active { get; private set; }

        /// <summary>Retained entries, in the order they were rejected; null when there are none.</summary>
        internal List<BrownFurPersistedModifierRecord> Retained { get; private set; }

        /// <summary>The entries this read moved from the active list to the retained list, with the reason.</summary>
        internal IList<KeyValuePair<string, BrownFurPersistedModifierRecord>> NewlyRejected
        { get; private set; }

        internal int RetainedCount
        { get { return Retained == null ? 0 : Retained.Count; } }
    }

    internal static class BrownFurPersistedModifierStore
    {
        /// <summary>
        /// Reads the persisted pair. Never throws. Retained entries are never re-examined or reactivated. In the
        /// active list:
        /// - an entry that is not a strictly valid released record is retained;
        /// - records with the same logical identity (buff, spell, caster, ability score, carrier) and every other
        ///   field equal are one meaning: the first stays active and each extra copy is retained;
        /// - records with the same logical identity that differ in any other field conflict: all are retained;
        /// - a valid record is retained, not active, when a rejected entry in the same list could refer to the same
        ///   modifier (each identity field the rejected entry states in a usable form equals the record's);
        /// - every other valid record stays active.
        /// Returns the input lists themselves when nothing changes.
        /// </summary>
        internal static BrownFurPersistedModifierState Normalize(
            List<BrownFurPersistedModifierRecord> records,
            List<BrownFurPersistedModifierRecord> retained)
        {
            records = records ?? new List<BrownFurPersistedModifierRecord>();
            var reasons = new string[records.Count];
            var rejected = new List<BrownFurPersistedModifierRecord>();
            for (int index = 0; index < records.Count; index++)
                if (records[index] == null || !records[index].IsValid)
                {
                    reasons[index] = records[index] == null ? "null-entry" :
                        records[index].Rejection;
                    rejected.Add(records[index]);
                }
            var groups = new List<List<int>>();
            for (int index = 0; index < records.Count; index++)
            {
                if (reasons[index] != null) continue;
                List<int> group = groups.FirstOrDefault(value =>
                    SameIdentity(records[value[0]], records[index]));
                if (group == null) groups.Add(group = new List<int>());
                group.Add(index);
            }
            foreach (List<int> group in groups)
            {
                if (group.Count == 1) continue;
                bool exact = group.All(index => SamePayload(records[group[0]],
                    records[index]));
                foreach (int index in group)
                    if (!exact) reasons[index] = "conflicting-duplicate";
                    else if (index != group[0]) reasons[index] = "exact-duplicate";
            }
            for (int index = 0; index < records.Count; index++)
                if (reasons[index] == null && rejected.Any(entry =>
                        MayReferTo(entry, records[index])))
                    reasons[index] = "ambiguous-with-rejected-entry";
            if (reasons.All(value => value == null))
                return new BrownFurPersistedModifierState(records, retained, null);
            var active = new List<BrownFurPersistedModifierRecord>();
            var moved = new List<KeyValuePair<string, BrownFurPersistedModifierRecord>>();
            var kept = retained == null ? new List<BrownFurPersistedModifierRecord>() :
                new List<BrownFurPersistedModifierRecord>(retained);
            for (int index = 0; index < records.Count; index++)
            {
                if (reasons[index] == null)
                {
                    active.Add(records[index]);
                    continue;
                }
                BrownFurPersistedModifierRecord entry = records[index] ??
                    BrownFurPersistedModifierJson.Read(null);
                moved.Add(new KeyValuePair<string, BrownFurPersistedModifierRecord>(
                    reasons[index], entry));
                kept.Add(entry);
            }
            return new BrownFurPersistedModifierState(active, kept, moved);
        }

        /// <summary>
        /// A newly committed record replaces the active record of the same logical modifier. Strict: an invalid
        /// record throws before anything changes. Retained entries are untouched.
        /// </summary>
        internal static BrownFurPersistedModifierState Remember(
            BrownFurPersistedModifierState state,
            BrownFurPersistedModifierRecord record)
        {
            BrownFurPersistedModifierPolicy.Validate(record);
            var active = state.Active.Where(value =>
                !BrownFurPersistedModifierPolicy.SameLogicalModifier(value,
                    record)).ToList();
            active.Add(record);
            return new BrownFurPersistedModifierState(active, state.Retained,
                state.NewlyRejected);
        }

        /// <summary>Removes the active records of one buff, spell and caster; retained entries are untouched.</summary>
        internal static BrownFurPersistedModifierState Forget(
            BrownFurPersistedModifierState state, string buffGuid,
            string spellGuid, string casterId, out int removed)
        {
            var active = state.Active.Where(value =>
                !string.Equals(value.BuffGuid, buffGuid, StringComparison.Ordinal) ||
                !string.Equals(value.SpellGuid, spellGuid, StringComparison.Ordinal) ||
                !string.Equals(value.CasterId, casterId, StringComparison.Ordinal))
                .ToList();
            removed = state.Active.Count - active.Count;
            return removed == 0 ? state : new BrownFurPersistedModifierState(
                active, state.Retained, state.NewlyRejected);
        }

        private static bool SameIdentity(BrownFurPersistedModifierRecord left,
            BrownFurPersistedModifierRecord right)
        {
            return BrownFurPersistedModifierPolicy.SameLogicalModifier(left, right);
        }

        private static bool SamePayload(BrownFurPersistedModifierRecord left,
            BrownFurPersistedModifierRecord right)
        {
            return left.SchemaVersion == right.SchemaVersion &&
                left.Increase == right.Increase &&
                left.OriginalValue == right.OriginalValue &&
                string.Equals(left.OriginalDescriptor, right.OriginalDescriptor,
                    StringComparison.Ordinal) &&
                left.EndTimeTicks == right.EndTimeTicks;
        }

        internal static bool MayReferTo(BrownFurPersistedModifierRecord rejected,
            BrownFurPersistedModifierRecord record)
        {
            string[] stated = rejected == null ?
                BrownFurPersistedModifierJson.Unreadable() : rejected.Identity;
            string[] actual = BrownFurPersistedModifierJson.Identity(record);
            for (int index = 0; index < actual.Length; index++)
                if (stated[index] != null && !string.Equals(stated[index],
                        actual[index], StringComparison.Ordinal))
                    return false;
            return true;
        }
    }
}
