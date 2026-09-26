using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Parts;
using Newtonsoft.Json;

namespace KingmakerGunslinger.Summoning
{
    /// <summary>
    /// One link a summon holds: which of its limbs took which target, and
    /// whether that target has since been engulfed. Nothing live is kept: the
    /// limb is a semantic slot - the primary hand, or an additional limb by
    /// index - resolved against the holder's body when it is read, and the
    /// target is its unit id.
    /// </summary>
    public sealed class SummonGrappleLinkRecord
    {
        public string TargetId { get; set; }
        public int Limb { get; set; }
        public int AdditionalIndex { get; set; }
        public string WeaponName { get; set; }
        public bool Engulfed { get; set; }
        /// <summary>The record was rebuilt for a link that had none, not established by a grab.</summary>
        public bool Repaired { get; set; }

        public SummonGrappleLinkRecord() { AdditionalIndex = -1; }

        /// <summary>
        /// One line of text per link, so the records stay plain data: target
        /// id, limb kind, limb index, the weapon blueprint's name, engulfed,
        /// repaired. Kingmaker writes a unit part on these summons by type
        /// without its contents, so this text is not carried across a save;
        /// the encoding keeps the store simple and inspectable, not durable.
        /// </summary>
        internal string Encode()
        {
            return string.Join(Separator, new[] { TargetId ?? string.Empty,
                Limb.ToString(System.Globalization.CultureInfo.InvariantCulture),
                AdditionalIndex.ToString(System.Globalization.CultureInfo.InvariantCulture),
                WeaponName ?? string.Empty, Engulfed ? "1" : "0", Repaired ? "1" : "0" });
        }

        internal static SummonGrappleLinkRecord Decode(string line)
        {
            if (string.IsNullOrEmpty(line)) return null;
            string[] parts = line.Split(new[] { Separator }, StringSplitOptions.None);
            if (parts.Length < 6 || string.IsNullOrEmpty(parts[0])) return null;
            int limb, index;
            if (!int.TryParse(parts[1], System.Globalization.NumberStyles.Integer,
                    System.Globalization.CultureInfo.InvariantCulture, out limb) ||
                !int.TryParse(parts[2], System.Globalization.NumberStyles.Integer,
                    System.Globalization.CultureInfo.InvariantCulture, out index))
                return null;
            return new SummonGrappleLinkRecord {
                TargetId = parts[0],
                Limb = limb,
                AdditionalIndex = index,
                WeaponName = string.IsNullOrEmpty(parts[3]) ? null : parts[3],
                Engulfed = parts[4] == "1",
                Repaired = parts[5] == "1"
            };
        }

        private const string Separator = "|";
    }

    /// <summary>
    /// The summon's own record of which limb holds or has engulfed which
    /// target, for the life of that hold.
    ///
    /// It replaces a process-local table keyed by buff instances, which the
    /// maintain could lose within a session: the damage then fell back to the
    /// holder's first grab limb, which is not equivalent for a Tiger or a
    /// Smilodon (bite and foreclaw deal different dice) and loses which of the
    /// Giant Flytrap's four mouths owns which target. It is also the mouth
    /// occupancy the one-target-per-mouth rule needs: a mouth that holds or
    /// has engulfed a target may not attack another one, and that has to
    /// outlive the engulf, which ends the held state.
    ///
    /// The store is session-scoped, which the owner accepted on 2026-09-26
    /// (OwnerAcceptedEngineLimitation:
    /// ACTIVE_SUMMON_GRAPPLES_RESET_SAFELY_ON_RELOAD). Kingmaker carries no
    /// active grapple across a save and writes a unit part on these summons by
    /// type without its contents, so a reload has neither a hold nor a record,
    /// and must simply come back clean.
    ///
    /// Reading filters the records against the game's own state without
    /// writing: a link whose target is gone, freed, spat out or no longer held
    /// by this summon answers nothing, so escape, death, dismissal, expiry, an
    /// area transition, a module-disabled load and a reloaded game all free
    /// precisely the mouth they should without a hook in each path. Records
    /// leave the store through the release paths, which know the link has
    /// ended, or when their limb takes a new victim; a read taken before the
    /// game has re-linked its grapple parts therefore cannot empty it.
    /// </summary>
    public sealed class UnitPartSummonGrappleLinks : UnitPart
    {
        [JsonProperty]
        private List<string> _links = new List<string>();

        internal IEnumerable<SummonGrappleLinkRecord> Links
        {
            get
            {
                return (_links ?? new List<string>()).Select(SummonGrappleLinkRecord.Decode)
                    .Where(value => value != null).ToArray();
            }
        }

        internal void Put(SummonGrappleLinkRecord record)
        {
            if (record == null || string.IsNullOrEmpty(record.TargetId)) return;
            // One record per target, and one per limb: a limb that takes a new
            // victim no longer owns the old one.
            List<SummonGrappleLinkRecord> kept = Links.Where(value =>
                value.TargetId != record.TargetId &&
                !(value.Limb == record.Limb &&
                    value.AdditionalIndex == record.AdditionalIndex)).ToList();
            kept.Add(record);
            _links = kept.Select(value => value.Encode()).ToList();
        }

        internal SummonGrappleLinkRecord Find(string targetId)
        {
            if (string.IsNullOrEmpty(targetId)) return null;
            return Links.FirstOrDefault(value => value.TargetId == targetId);
        }

        internal bool Drop(string targetId)
        {
            if (_links == null || string.IsNullOrEmpty(targetId)) return false;
            int before = _links.Count;
            _links = Links.Where(value => value.TargetId != targetId)
                .Select(value => value.Encode()).ToList();
            return _links.Count != before;
        }

        internal void Keep(IEnumerable<SummonGrappleLinkRecord> records)
        {
            _links = (records ?? Enumerable.Empty<SummonGrappleLinkRecord>())
                .Where(value => value != null).Select(value => value.Encode()).ToList();
        }

        internal int Count { get { return _links == null ? 0 : _links.Count; } }
    }

    /// <summary>
    /// The link store: which limb of a summon established the hold on each
    /// target, and which mouths are occupied, for as long as the game holds
    /// them. Every read reconciles the records against the game's own state
    /// first, so a stale link never answers a question, and a reload - which
    /// keeps neither hold nor record - answers nothing at all.
    /// </summary>
    internal static class SummonGrappleLinks
    {
        /// <summary>Records the limb that established a hold on this target.</summary>
        internal static void Record(UnitEntityData holder, UnitEntityData target,
            ItemEntityWeapon weapon)
        {
            if (holder == null || holder.Descriptor == null || target == null) return;
            int index;
            SummonLimbKind kind = SummonLimbs.Classify(holder, weapon, out index);
            holder.Ensure<UnitPartSummonGrappleLinks>().Put(new SummonGrappleLinkRecord {
                TargetId = target.UniqueId,
                Limb = (int)kind,
                AdditionalIndex = index,
                WeaponName = weapon == null || weapon.Blueprint == null ? null : weapon.Blueprint.name,
                Engulfed = false,
                Repaired = false
            });
        }

        /// <summary>
        /// The link outlives the engulf: the held state ends there, but the
        /// mouth stays shut on that victim until it is spat out or freed.
        /// </summary>
        internal static void MarkEngulfed(UnitEntityData holder, UnitEntityData target)
        {
            if (holder == null || holder.Descriptor == null || target == null) return;
            UnitPartSummonGrappleLinks part = holder.Get<UnitPartSummonGrappleLinks>();
            SummonGrappleLinkRecord record = part == null ? null : part.Find(target.UniqueId);
            if (record == null) return;
            record.Engulfed = true;
            part.Put(record);
        }

        internal static void Release(UnitEntityData holder, UnitEntityData target)
        {
            if (holder == null || holder.Descriptor == null || target == null) return;
            UnitPartSummonGrappleLinks part = holder.Get<UnitPartSummonGrappleLinks>();
            if (part != null) part.Drop(target.UniqueId);
        }

        internal static void ReleaseAll(UnitEntityData holder)
        {
            if (holder == null || holder.Descriptor == null) return;
            UnitPartSummonGrappleLinks part = holder.Get<UnitPartSummonGrappleLinks>();
            if (part != null) part.Keep(null);
        }

        /// <summary>
        /// The weapon entity of the limb that established the hold on this
        /// target, read from the holder's body as it stands, or null when this
        /// summon holds no such link - which is also what a reloaded game
        /// answers, because neither the hold nor the record is carried across
        /// a save.
        /// </summary>
        internal static ItemEntityWeapon EstablishingWeapon(UnitEntityData holder,
            UnitEntityData target)
        {
            SummonGrappleLinkRecord record = LiveRecord(holder, target);
            return record == null ? null : Resolve(holder, record);
        }

        /// <summary>
        /// The limb a stored record names for this target, whatever the game's
        /// own state says now. The persistence leg reads it to show that a
        /// reloaded game holds no record at all, beside a hold it also does
        /// not hold.
        /// </summary>
        internal static ItemEntityWeapon StoredLimbOf(UnitEntityData holder,
            UnitEntityData target)
        {
            if (holder == null || holder.Descriptor == null || target == null) return null;
            UnitPartSummonGrappleLinks part = holder.Get<UnitPartSummonGrappleLinks>();
            SummonGrappleLinkRecord record = part == null ? null : part.Find(target.UniqueId);
            return record == null ? null : Resolve(holder, record);
        }

        /// <summary>True when this limb already holds or has engulfed someone.</summary>
        internal static bool IsLimbOccupied(UnitEntityData holder, ItemEntityWeapon weapon)
        {
            return OccupantOf(holder, weapon) != null;
        }

        /// <summary>The unit this limb holds or has engulfed, or null.</summary>
        internal static UnitEntityData OccupantOf(UnitEntityData holder, ItemEntityWeapon weapon)
        {
            if (holder == null || weapon == null) return null;
            int index;
            SummonLimbKind kind = SummonLimbs.Classify(holder, weapon, out index);
            if (kind == SummonLimbKind.None) return null;
            Dictionary<string, UnitEntityData> units = null;
            foreach (SummonGrappleLinkRecord record in Reconcile(holder))
                if (record.Limb == (int)kind && record.AdditionalIndex == index)
                {
                    if (units == null) units = UnitsById();
                    UnitEntityData unit;
                    return units.TryGetValue(record.TargetId ?? string.Empty, out unit) ? unit : null;
                }
            return null;
        }

        /// <summary>The live records, for the evidence: limb, target and state.</summary>
        internal static string Describe(UnitEntityData holder)
        {
            UnitPartSummonGrappleLinks stored = holder == null || holder.Descriptor == null ?
                null : holder.Get<UnitPartSummonGrappleLinks>();
            string raw = stored == null ? "part=absent" : "part=present,stored=" + stored.Count +
                "[" + string.Join(",", stored.Links.Select(value => (value.TargetId ?? "?") + ":" +
                    (SummonLimbKind)value.Limb + (value.AdditionalIndex >= 0 ? "[" +
                    value.AdditionalIndex + "]" : "") + (value.Engulfed ? ":engulfed" : "")).ToArray()) + "]";
            List<SummonGrappleLinkRecord> records = Reconcile(holder);
            if (records.Count == 0) return "links=0;" + raw;
            Dictionary<string, UnitEntityData> units = UnitsById();
            var parts = new List<string>();
            foreach (SummonGrappleLinkRecord record in records)
            {
                UnitEntityData unit;
                units.TryGetValue(record.TargetId ?? string.Empty, out unit);
                ItemEntityWeapon weapon = Resolve(holder, record);
                parts.Add((unit == null || unit.Blueprint == null ? "?" : unit.Blueprint.name) +
                    ":limb=" + (SummonLimbKind)record.Limb +
                    (record.AdditionalIndex >= 0 ? "[" + record.AdditionalIndex + "]" : "") +
                    ";weapon=" + (weapon == null || weapon.Blueprint == null ? "unresolved" :
                        weapon.Blueprint.name) +
                    ";recorded=" + (record.WeaponName ?? "?") +
                    (record.Engulfed ? ";engulfed" : "") +
                    (record.Repaired ? ";repaired" : ""));
            }
            return "links=" + records.Count + ";" + raw + ";" +
                string.Join("|", parts.ToArray());
        }

        /// <summary>
        /// Adopts a link that exists in the game's state but carries no
        /// record - one established before this store existed, or one a
        /// module-disabled recovery rebuilt - so a mouth is never left owning
        /// a target it cannot name. The adopted record is marked repaired and
        /// uses the holder's first grab limb, which is all that is knowable
        /// then.
        /// </summary>
        internal static int Repair(UnitEntityData holder, SummonGrabComponent grab)
        {
            if (holder == null || holder.Descriptor == null || grab == null) return 0;
            List<SummonGrappleLinkRecord> records = Reconcile(holder);
            var known = new HashSet<string>(records.Select(value => value.TargetId ?? string.Empty));
            int adopted = 0;
            foreach (UnitEntityData target in HeldOrEngulfed(holder, grab))
            {
                if (target == null || known.Contains(target.UniqueId)) continue;
                ItemEntityWeapon weapon = grab.FirstGrabWeapon(holder);
                int index;
                SummonLimbKind kind = SummonLimbs.Classify(holder, weapon, out index);
                holder.Ensure<UnitPartSummonGrappleLinks>().Put(new SummonGrappleLinkRecord {
                    TargetId = target.UniqueId,
                    Limb = (int)kind,
                    AdditionalIndex = index,
                    WeaponName = weapon == null || weapon.Blueprint == null ? null :
                        weapon.Blueprint.name,
                    Engulfed = IsEngulfed(holder, target),
                    Repaired = true
                });
                adopted++;
            }
            return adopted;
        }

        /// <summary>
        /// The records that still describe the game's own state. This never
        /// writes: a read taken before the game has re-linked its grapple
        /// parts - an attack roll in the first frames of a load reaches the
        /// grab component's handler - would otherwise empty the store for
        /// good. A record whose hold has ended is ignored by every answer and
        /// leaves the store through the release paths, or when its limb takes
        /// a new victim.
        /// </summary>
        internal static List<SummonGrappleLinkRecord> Reconcile(UnitEntityData holder)
        {
            var kept = new List<SummonGrappleLinkRecord>();
            if (holder == null || holder.Descriptor == null || holder.Destroyed) return kept;
            UnitPartSummonGrappleLinks part = holder.Get<UnitPartSummonGrappleLinks>();
            if (part == null || part.Count == 0) return kept;
            SummonGrabComponent grab = SummonGrabComponent.Find(holder);
            Dictionary<string, UnitEntityData> units = UnitsById();
            foreach (SummonGrappleLinkRecord record in part.Links)
            {
                UnitEntityData target;
                if (record == null || string.IsNullOrEmpty(record.TargetId) ||
                    !units.TryGetValue(record.TargetId, out target) ||
                    target == null || target.Destroyed) continue;
                if (record.Engulfed)
                {
                    if (IsEngulfed(holder, target)) kept.Add(record);
                    continue;
                }
                if (grab == null) continue;
                bool held = grab.MultiLink
                    ? ReferenceEquals(SummonHeldComponent.HolderOf(target, grab.GrappledBuff), holder)
                    : ReferenceEquals(SummonHoldComponent.HeldTarget(holder), target);
                if (held) kept.Add(record);
            }
            return kept;
        }

        private static SummonGrappleLinkRecord LiveRecord(UnitEntityData holder,
            UnitEntityData target)
        {
            if (target == null) return null;
            return Reconcile(holder).FirstOrDefault(value => value.TargetId == target.UniqueId);
        }

        private static ItemEntityWeapon Resolve(UnitEntityData holder,
            SummonGrappleLinkRecord record)
        {
            if (holder == null || record == null) return null;
            return SummonLimbs.WeaponAt(holder, (SummonLimbKind)record.Limb,
                record.AdditionalIndex);
        }

        private static bool IsEngulfed(UnitEntityData holder, UnitEntityData target)
        {
            UnitPartSwallowWhole part = holder == null ? null : holder.Get<UnitPartSwallowWhole>();
            if (part == null || part.SwallowedUnits == null || target == null) return false;
            return part.SwallowedUnits.Any(value => value != null &&
                ReferenceEquals(value.Value, target));
        }

        private static IEnumerable<UnitEntityData> HeldOrEngulfed(UnitEntityData holder,
            SummonGrabComponent grab)
        {
            var result = new List<UnitEntityData>();
            if (holder == null || grab == null) return result;
            if (grab.MultiLink)
                result.AddRange(SummonMultiHoldComponent.HeldTargets(holder, grab.GrappledBuff));
            else
            {
                UnitEntityData held = SummonHoldComponent.HeldTarget(holder);
                if (held != null) result.Add(held);
            }
            UnitPartSwallowWhole part = holder.Get<UnitPartSwallowWhole>();
            if (part != null && part.SwallowedUnits != null)
                foreach (UnitReference reference in part.SwallowedUnits)
                    if (reference != null && reference.Value != null &&
                        !result.Contains(reference.Value))
                        result.Add(reference.Value);
            return result;
        }

        private static Dictionary<string, UnitEntityData> UnitsById()
        {
            var units = new Dictionary<string, UnitEntityData>(StringComparer.Ordinal);
            if (Game.Instance == null || Game.Instance.State == null ||
                Game.Instance.State.Units == null) return units;
            foreach (UnitEntityData unit in Game.Instance.State.Units.All)
                if (unit != null && !string.IsNullOrEmpty(unit.UniqueId) &&
                    !units.ContainsKey(unit.UniqueId))
                    units[unit.UniqueId] = unit;
            return units;
        }
    }
}
