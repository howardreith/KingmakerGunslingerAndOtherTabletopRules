using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace KingmakerGunslinger.UrbanBarbarian
{
    /// <summary>
    /// Per-tier Controlled Rage allocations. The released persisted form
    /// (v0.0.95 onward, unchanged since) is "tier:STR,DEX,CON" entries joined
    /// by ';', ordered by tier, with the ordinary +4 tier always present.
    ///
    /// <see cref="Parse"/> is the strict released grammar. <see cref="Restore"/>
    /// is the total reader used for saved state: it never throws, keeps every
    /// tier whose meaning is unambiguous, and leaves a tier with an invalid or
    /// conflicting claim blocked (no default, no grant) while the rejected
    /// input is retained verbatim by the owning unit part (RISK-03).
    /// </summary>
    internal sealed class ControlledRageSelectionState
    {
        private static readonly ControlledRageTier[] KnownTiers = {
            ControlledRageTier.Ordinary, ControlledRageTier.Greater,
            ControlledRageTier.Mighty };

        private readonly Dictionary<ControlledRageTier, ControlledRageAllocation>
            _selections = new Dictionary<ControlledRageTier, ControlledRageAllocation>();

        private readonly HashSet<ControlledRageTier> _blocked =
            new HashSet<ControlledRageTier>();

        internal ControlledRageSelectionState()
        {
            Unlock(ControlledRageTier.Ordinary);
        }

        private ControlledRageSelectionState(bool empty)
        {
        }

        internal ControlledRageTier CurrentTier
        {
            get
            {
                if (_selections.Count == 0)
                    throw new InvalidOperationException(
                        "No Controlled Rage tier is active.");
                return _selections.Keys.OrderByDescending(value => (int)value).First();
            }
        }

        internal ControlledRageAllocation CurrentSelection
        {
            get { return _selections[CurrentTier]; }
        }

        internal IReadOnlyList<ControlledRageAllocation> VisibleAllocations
        {
            get { return ControlledRageAllocationPolicy.Generate(CurrentTier); }
        }

        /// <summary>
        /// The persisted input this state was restored from when that input
        /// failed validation, or null. The owning part moves it, unaltered,
        /// into its retained payloads on the next write.
        /// </summary>
        internal string PendingRejected { get; private set; }

        internal IEnumerable<ControlledRageTier> ActiveTiers
        {
            get { return _selections.Keys.OrderBy(value => (int)value); }
        }

        internal IEnumerable<ControlledRageTier> BlockedTiers
        {
            get { return _blocked.OrderBy(value => (int)value); }
        }

        internal bool IsBlocked(ControlledRageTier tier)
        {
            return _blocked.Contains(tier);
        }

        /// <summary>
        /// First unlock of a legitimately absent tier receives that tier's
        /// full-Strength default. An active tier is unchanged, and a blocked
        /// tier is never given a default.
        /// </summary>
        internal bool Unlock(ControlledRageTier tier)
        {
            if (_selections.ContainsKey(tier) || _blocked.Contains(tier))
                return false;
            _selections.Add(tier, ControlledRageAllocationPolicy.Default(tier));
            return true;
        }

        internal bool TrySelect(ControlledRageTier tier,
            ControlledRageAllocation allocation, bool rageActive)
        {
            if (rageActive || _selections.Count == 0 || tier != CurrentTier ||
                !ControlledRageAllocationPolicy.IsLegalForTier(tier, allocation))
                return false;
            _selections[tier] = allocation;
            return true;
        }

        /// <summary>
        /// A validated selection for an active tier, or an explicit new choice
        /// for a blocked tier. The explicit choice replaces the block; the
        /// rejected input it replaces stays retained by the owning part.
        /// </summary>
        internal bool TrySelectExact(ControlledRageTier tier,
            ControlledRageAllocation allocation, bool rageActive)
        {
            if (rageActive ||
                (!_selections.ContainsKey(tier) && !_blocked.Contains(tier)) ||
                !ControlledRageAllocationPolicy.IsLegalForTier(tier, allocation))
                return false;
            _selections[tier] = allocation;
            _blocked.Remove(tier);
            return true;
        }

        internal ControlledRageAllocation SelectionFor(ControlledRageTier tier)
        {
            ControlledRageAllocation value;
            return _selections.TryGetValue(tier, out value) ? value : null;
        }

        /// <summary>
        /// The released grammar for the active tiers only, or null when no tier
        /// is active. Null does not mean every tier is blocked: a higher tier
        /// can be legitimately absent beside a blocked ordinary tier.
        /// </summary>
        internal string Serialize()
        {
            if (_selections.Count == 0) return null;
            return string.Join(";", _selections.OrderBy(value => (int)value.Key)
                .Select(value => string.Format(CultureInfo.InvariantCulture,
                    "{0}:{1},{2},{3}", (int)value.Key, value.Value.Strength,
                    value.Value.Dexterity, value.Value.Constitution)).ToArray());
        }

        /// <summary>
        /// The retained-payload list to persist beside <see cref="Serialize"/>:
        /// the existing list, plus <see cref="PendingRejected"/> verbatim when
        /// it is not already retained. Returns the same instance when nothing
        /// is added, and never removes or edits an entry.
        /// </summary>
        internal List<string> MergeRetained(List<string> retained)
        {
            if (PendingRejected == null || (retained != null &&
                    retained.Contains(PendingRejected, StringComparer.Ordinal)))
                return retained;
            var result = retained == null ? new List<string>() :
                new List<string>(retained);
            result.Add(PendingRejected);
            return result;
        }

        internal static ControlledRageSelectionState Parse(string serialized)
        {
            if (string.IsNullOrWhiteSpace(serialized))
                throw new ArgumentException("Persisted Controlled Rage state is required.",
                    "serialized");
            var result = new ControlledRageSelectionState();
            result._selections.Clear();
            foreach (string entry in serialized.Split(';'))
            {
                string[] parts = entry.Split(':');
                string[] values = parts.Length == 2 ? parts[1].Split(',') :
                    new string[0];
                int tierValue, strength, dexterity, constitution;
                if (values.Length != 3 ||
                    !int.TryParse(parts[0], NumberStyles.None,
                        CultureInfo.InvariantCulture, out tierValue) ||
                    !int.TryParse(values[0], NumberStyles.None,
                        CultureInfo.InvariantCulture, out strength) ||
                    !int.TryParse(values[1], NumberStyles.None,
                        CultureInfo.InvariantCulture, out dexterity) ||
                    !int.TryParse(values[2], NumberStyles.None,
                        CultureInfo.InvariantCulture, out constitution) ||
                    (tierValue != 4 && tierValue != 6 && tierValue != 8))
                    throw new FormatException("Invalid persisted Controlled Rage state.");
                var tier = (ControlledRageTier)tierValue;
                var allocation = new ControlledRageAllocation(strength, dexterity,
                    constitution);
                if (!ControlledRageAllocationPolicy.IsLegalForTier(tier, allocation) ||
                    result._selections.ContainsKey(tier))
                    throw new FormatException("Invalid persisted Controlled Rage allocation.");
                result._selections.Add(tier, allocation);
            }
            if (!result._selections.ContainsKey(ControlledRageTier.Ordinary))
                throw new FormatException("Ordinary Controlled Rage state is missing.");
            return result;
        }

        /// <summary>
        /// Total reader for saved state. <paramref name="persisted"/> is the
        /// released field; <paramref name="retained"/> are earlier rejected
        /// inputs kept verbatim by the part. Never throws.
        ///
        /// Classification of one persisted string, entry by entry (';'):
        /// - "k:..." with k in {4, 6, 8} claims that tier. A tier with exactly
        ///   one meaning (one valid entry, or identical valid duplicates) is
        ///   unambiguous. A tier with any invalid entry or two different
        ///   allocations is tainted: it stays inactive and is not defaulted.
        /// - "k:..." with another integer k is an unknown-tier entry: retained,
        ///   it grants nothing and does not change the known tiers' meaning.
        /// - An entry without an integer tier key cannot be attributed, so the
        ///   meaning of the whole input is unknown and every tier is tainted.
        /// - The released writer always records the ordinary tier, so an input
        ///   with no ordinary claim taints the ordinary tier.
        ///
        /// Null with no retained payload is legitimate absence and receives the
        /// released first-unlock default. Retained payloads mean the part was
        /// already used, so a missing current allocation is never invented:
        /// the ordinary tier, and every tier a retained payload claims or
        /// taints, stays blocked unless the current state holds it. Retained
        /// text only blocks; it is never replayed as an allocation. A tier no
        /// retained payload mentions (for example Greater beside a migrated
        /// "4:bad") stays legitimately absent. Every non-null string, including
        /// an empty one, is classified. Input that the released strict grammar
        /// accepts restores exactly as <see cref="Parse"/> would.
        /// </summary>
        internal static ControlledRageSelectionState Restore(string persisted,
            IEnumerable<string> retained)
        {
            var blockedByRetained = new HashSet<ControlledRageTier>();
            bool hasHistory = false;
            if (retained != null)
                foreach (string payload in retained)
                {
                    hasHistory = true;
                    Reading prior = Classify(payload);
                    blockedByRetained.UnionWith(prior.TaintedTiers);
                    blockedByRetained.UnionWith(prior.ClaimedTiers);
                }
            if (hasHistory)
                blockedByRetained.Add(ControlledRageTier.Ordinary);

            if (persisted == null)
            {
                if (!hasHistory)
                    return new ControlledRageSelectionState();
                var blockedOnly = new ControlledRageSelectionState(true);
                blockedOnly._blocked.UnionWith(blockedByRetained);
                return blockedOnly;
            }

            Reading reading = Classify(persisted);
            var result = new ControlledRageSelectionState(true);
            foreach (KeyValuePair<ControlledRageTier, ControlledRageAllocation>
                    pair in reading.Unambiguous)
                result._selections.Add(pair.Key, pair.Value);
            bool acceptable = !reading.Unattributable &&
                reading.InvalidTiers.Count == 0 && reading.UnknownTierEntries == 0 &&
                reading.DuplicateEntries == 0 &&
                (reading.HasOrdinaryClaim ||
                    blockedByRetained.Contains(ControlledRageTier.Ordinary));
            if (!acceptable)
            {
                result.PendingRejected = persisted;
                result._blocked.UnionWith(reading.TaintedTiers);
            }
            result._blocked.UnionWith(blockedByRetained);
            result._blocked.ExceptWith(result._selections.Keys);
            return result;
        }

        private static Reading Classify(string raw)
        {
            var reading = new Reading();
            if (raw == null) return reading;
            var claims = new Dictionary<ControlledRageTier,
                List<ControlledRageAllocation>>();
            foreach (string entry in raw.Split(';'))
            {
                int colon = entry.IndexOf(':');
                int key;
                if (colon <= 0 || !int.TryParse(entry.Substring(0, colon),
                        NumberStyles.None, CultureInfo.InvariantCulture, out key))
                {
                    reading.Unattributable = true;
                    continue;
                }
                if (key != 4 && key != 6 && key != 8)
                {
                    reading.UnknownTierEntries++;
                    continue;
                }
                var tier = (ControlledRageTier)key;
                reading.ClaimedTiers.Add(tier);
                List<ControlledRageAllocation> group;
                if (!claims.TryGetValue(tier, out group))
                    claims.Add(tier, group = new List<ControlledRageAllocation>());
                group.Add(TryReadAllocation(tier, entry.Substring(colon + 1)));
            }
            foreach (KeyValuePair<ControlledRageTier,
                    List<ControlledRageAllocation>> claim in claims)
            {
                if (claim.Value.Count > 1) reading.DuplicateEntries +=
                    claim.Value.Count - 1;
                if (claim.Value.Any(value => value == null) ||
                    claim.Value.Distinct().Count() != 1)
                    reading.InvalidTiers.Add(claim.Key);
                else if (!reading.Unattributable)
                    reading.Unambiguous.Add(claim.Key, claim.Value[0]);
            }
            reading.HasOrdinaryClaim = claims.ContainsKey(ControlledRageTier.Ordinary);
            if (reading.Unattributable)
            {
                reading.Unambiguous.Clear();
                reading.TaintedTiers.UnionWith(KnownTiers);
            }
            else
            {
                reading.TaintedTiers.UnionWith(reading.InvalidTiers);
                if (!reading.HasOrdinaryClaim)
                    reading.TaintedTiers.Add(ControlledRageTier.Ordinary);
            }
            return reading;
        }

        private static ControlledRageAllocation TryReadAllocation(
            ControlledRageTier tier, string text)
        {
            string[] values = text.Split(',');
            if (values.Length != 3) return null;
            var parsed = new int[3];
            for (int index = 0; index < 3; index++)
                if (!int.TryParse(values[index], NumberStyles.None,
                        CultureInfo.InvariantCulture, out parsed[index]) ||
                    parsed[index] > (int)ControlledRageTier.Mighty ||
                    parsed[index] % 2 != 0)
                    return null;
            if (parsed[0] + parsed[1] + parsed[2] != (int)tier) return null;
            var allocation = new ControlledRageAllocation(parsed[0], parsed[1],
                parsed[2]);
            return ControlledRageAllocationPolicy.IsLegalForTier(tier, allocation) ?
                allocation : null;
        }

        private sealed class Reading
        {
            internal readonly Dictionary<ControlledRageTier, ControlledRageAllocation>
                Unambiguous = new Dictionary<ControlledRageTier, ControlledRageAllocation>();
            internal readonly HashSet<ControlledRageTier> InvalidTiers =
                new HashSet<ControlledRageTier>();
            internal readonly HashSet<ControlledRageTier> TaintedTiers =
                new HashSet<ControlledRageTier>();
            internal readonly HashSet<ControlledRageTier> ClaimedTiers =
                new HashSet<ControlledRageTier>();
            internal bool Unattributable;
            internal bool HasOrdinaryClaim;
            internal int UnknownTierEntries;
            internal int DuplicateEntries;
        }
    }
}
