using System;
using System.Collections.Generic;
using System.Linq;

namespace KingmakerGunslinger.Acquisition.BetterVendors
{
    /// <summary>Which owning content modules are active in this process.</summary>
    internal sealed class ProgressionModuleState
    {
        internal ProgressionModuleState(bool gunslinger, bool easternWeapons,
            bool elvenBranchedSpears)
        {
            Gunslinger = gunslinger;
            EasternWeapons = easternWeapons;
            ElvenBranchedSpears = elvenBranchedSpears;
        }

        internal bool Gunslinger { get; private set; }
        internal bool EasternWeapons { get; private set; }
        internal bool ElvenBranchedSpears { get; private set; }

        internal bool Allows(ProgressionContentModule module)
        {
            switch (module)
            {
                case ProgressionContentModule.Gunslinger: return Gunslinger;
                case ProgressionContentModule.EasternWeapons: return EasternWeapons;
                case ProgressionContentModule.ElvenBranchedSpears:
                    return ElvenBranchedSpears;
                default: return false;
            }
        }

        public override string ToString()
        {
            return "gunslinger=" + Gunslinger + ",eastern-weapons=" +
                EasternWeapons + ",elven-branched-spears=" + ElvenBranchedSpears;
        }
    }

    internal enum BetterVendorsGrantReason
    {
        /// <summary>The entry's one-time progression grant; recorded in the ledger.</summary>
        InitialGrant = 0,

        /// <summary>
        /// A repeat of Better Vendors' own current-tier stock event for an entry
        /// whose initial grant is already recorded.
        /// </summary>
        Replenishment = 1
    }

    internal sealed class BetterVendorsGrant
    {
        internal BetterVendorsGrant(ProgressionWeaponSpec spec,
            BetterVendorsProgressionTier tier, BetterVendorsGrantReason reason)
        {
            Spec = spec ?? throw new ArgumentNullException("spec");
            Tier = tier ?? throw new ArgumentNullException("tier");
            if (spec.UnlockTier != tier.Tier)
                throw new ArgumentException(
                    "A grant must use the entry's own actual-enhancement tier.");
            Reason = reason;
        }

        internal ProgressionWeaponSpec Spec { get; private set; }
        internal BetterVendorsProgressionTier Tier { get; private set; }
        internal int Quantity { get { return Tier.Quantity; } }
        internal BetterVendorsGrantReason Reason { get; private set; }
    }

    internal sealed class BetterVendorsGrantPlan
    {
        internal static readonly BetterVendorsGrantPlan Empty =
            new BetterVendorsGrantPlan(new BetterVendorsGrant[0],
                new ProgressionWeaponSpec[0], new ProgressionWeaponSpec[0]);

        internal BetterVendorsGrantPlan(BetterVendorsGrant[] grants,
            ProgressionWeaponSpec[] nativelyDelivered,
            ProgressionWeaponSpec[] moduleSuppressed)
        {
            Grants = grants ?? new BetterVendorsGrant[0];
            NativelyDelivered = nativelyDelivered ?? new ProgressionWeaponSpec[0];
            ModuleSuppressed = moduleSuppressed ?? new ProgressionWeaponSpec[0];
        }

        /// <summary>Additions this integration must perform, in catalog order.</summary>
        internal BetterVendorsGrant[] Grants { get; private set; }

        /// <summary>
        /// Entries Better Vendors' own query already selected and stocked in
        /// the observed call. They are recorded, never added a second time.
        /// </summary>
        internal ProgressionWeaponSpec[] NativelyDelivered { get; private set; }

        /// <summary>Tier entries withheld because their owning module is off.</summary>
        internal ProgressionWeaponSpec[] ModuleSuppressed { get; private set; }

        internal bool IsEmpty
        {
            get { return Grants.Length == 0 && NativelyDelivered.Length == 0; }
        }
    }

    /// <summary>
    /// Pure grant planning for Better Vendors' Military stock calls and for
    /// the one-time existing-save catch-up. The ledger decides what was granted;
    /// current merchant inventory is never consulted, so buying every copy can
    /// never make an entry look ungranted.
    /// </summary>
    internal static class BetterVendorsGrantPlanner
    {
        internal static BetterVendorsGrantPlan PlanStockCall(
            IEnumerable<ProgressionWeaponSpec> catalog,
            ProgressionModuleState modules, Func<string, bool> isGranted,
            BetterVendorsStockCall call)
        {
            if (catalog == null) throw new ArgumentNullException("catalog");
            if (modules == null) throw new ArgumentNullException("modules");
            if (isGranted == null) throw new ArgumentNullException("isGranted");
            if (call == null || call.Kind == BetterVendorsStockCallKind.Unsupported ||
                call.Tier == null)
                return BetterVendorsGrantPlan.Empty;

            var grants = new List<BetterVendorsGrant>();
            var native = new List<ProgressionWeaponSpec>();
            var suppressed = new List<ProgressionWeaponSpec>();
            foreach (ProgressionWeaponSpec spec in catalog.Where(value =>
                value != null && value.UnlockTier == call.Tier.Tier))
            {
                if (call.WasNativelySelected(spec.Guid))
                {
                    native.Add(spec);
                    continue;
                }
                if (!modules.Allows(spec.Module))
                {
                    suppressed.Add(spec);
                    continue;
                }
                bool granted = isGranted(spec.Guid);
                if (granted && call.Kind == BetterVendorsStockCallKind.CatchUp)
                    continue;
                grants.Add(new BetterVendorsGrant(spec, call.Tier, granted
                    ? BetterVendorsGrantReason.Replenishment
                    : BetterVendorsGrantReason.InitialGrant));
            }
            return new BetterVendorsGrantPlan(grants.ToArray(),
                native.ToArray(), suppressed.ToArray());
        }

        /// <summary>
        /// Missing initial grants for every milestone the save has already
        /// reached. No kingdom means no progression; future tiers and entries
        /// of inactive modules are never granted.
        /// </summary>
        internal static BetterVendorsGrantPlan PlanCatchUp(
            IEnumerable<ProgressionWeaponSpec> catalog,
            ProgressionModuleState modules, Func<string, bool> isGranted,
            bool kingdomExists, int militaryRank)
        {
            if (catalog == null) throw new ArgumentNullException("catalog");
            if (modules == null) throw new ArgumentNullException("modules");
            if (isGranted == null) throw new ArgumentNullException("isGranted");
            if (!kingdomExists || militaryRank < 1)
                return BetterVendorsGrantPlan.Empty;

            BetterVendorsProgressionTier[] unlocked =
                BetterVendorsProgressionSchedule.UnlockedTiers(Math.Min(
                    militaryRank, BetterVendorsProgressionSchedule.MaximumMilitaryRank));
            var grants = new List<BetterVendorsGrant>();
            var suppressed = new List<ProgressionWeaponSpec>();
            foreach (ProgressionWeaponSpec spec in catalog.Where(value =>
                value != null))
            {
                BetterVendorsProgressionTier tier = unlocked.SingleOrDefault(
                    value => value.Tier == spec.UnlockTier);
                if (tier == null) continue;
                if (!modules.Allows(spec.Module))
                {
                    suppressed.Add(spec);
                    continue;
                }
                if (isGranted(spec.Guid)) continue;
                grants.Add(new BetterVendorsGrant(spec, tier,
                    BetterVendorsGrantReason.InitialGrant));
            }
            return new BetterVendorsGrantPlan(grants.ToArray(),
                new ProgressionWeaponSpec[0], suppressed.ToArray());
        }
    }

    internal sealed class BetterVendorsGrantOutcome
    {
        internal int Granted { get; set; }
        internal int Replenished { get; set; }
        internal int Copies { get; set; }
        internal int Partial { get; set; }
        internal int Failed { get; set; }

        /// <summary>Initial grants whose stock changed but whose record failed.</summary>
        internal int Unrecorded { get; set; }

        internal List<string> Recorded { get; private set; }
        internal List<Exception> Errors { get; private set; }

        internal BetterVendorsGrantOutcome()
        {
            Recorded = new List<string>();
            Errors = new List<Exception>();
        }

        public override string ToString()
        {
            return string.Format(System.Globalization.CultureInfo.InvariantCulture,
                "granted={0};replenished={1};copies={2};partial={3};failed={4};unrecorded={5}",
                Granted, Replenished, Copies, Partial, Failed, Unrecorded);
        }
    }

    /// <summary>
    /// Applies planned grants through supplied stock operations. An initial
    /// grant is recorded only after its mutation is observed: an addition that
    /// changed nothing stays unrecorded (a later trigger retries it), while one
    /// that changed stock only partially is recorded and never topped up, so a
    /// failure is never answered by blindly repeating an applied addition. The
    /// ledger and the merchant stock live in the same save, so they persist or
    /// are discarded together; the game offers no stronger transaction.
    /// </summary>
    internal static class BetterVendorsGrantApplier
    {
        internal static BetterVendorsGrantOutcome Apply(
            IEnumerable<BetterVendorsGrant> grants,
            Func<ProgressionWeaponSpec, int> count,
            Action<ProgressionWeaponSpec, int> add, Func<string, bool> record)
        {
            if (grants == null) throw new ArgumentNullException("grants");
            if (count == null) throw new ArgumentNullException("count");
            if (add == null) throw new ArgumentNullException("add");
            if (record == null) throw new ArgumentNullException("record");
            var outcome = new BetterVendorsGrantOutcome();
            foreach (BetterVendorsGrant grant in grants)
            {
                int before = count(grant.Spec);
                try
                {
                    add(grant.Spec, grant.Quantity);
                }
                catch (Exception exception)
                {
                    outcome.Errors.Add(exception);
                }
                int added = count(grant.Spec) - before;
                if (added <= 0)
                {
                    outcome.Failed++;
                    continue;
                }
                outcome.Copies += added;
                if (added != grant.Quantity) outcome.Partial++;
                if (grant.Reason == BetterVendorsGrantReason.InitialGrant)
                {
                    outcome.Granted++;
                    try
                    {
                        if (record(grant.Spec.Guid))
                            outcome.Recorded.Add(grant.Spec.Guid);
                    }
                    catch (Exception exception)
                    {
                        // The stock change stands. Report the bookkeeping
                        // failure and keep applying the independent grants.
                        outcome.Unrecorded++;
                        outcome.Errors.Add(exception);
                    }
                }
                else
                {
                    outcome.Replenished++;
                }
            }
            return outcome;
        }
    }

    /// <summary>
    /// Save-local record of which authorized entries have received their
    /// one-time progression grant. It wraps the list persisted by the save's
    /// ledger part, keeps it sorted and duplicate-free, and preserves unknown
    /// identities recorded by other catalog versions untouched.
    /// </summary>
    internal sealed class ProgressionGrantLedger
    {
        private readonly List<string> _entries;

        internal ProgressionGrantLedger(List<string> backing)
        {
            _entries = backing ?? throw new ArgumentNullException("backing");
        }

        internal int Count { get { return _entries.Count; } }

        internal bool Has(string guid)
        {
            return !string.IsNullOrEmpty(guid) &&
                _entries.Contains(guid, StringComparer.Ordinal);
        }

        /// <summary>Returns true only when the identity was newly recorded.</summary>
        internal bool Record(string guid)
        {
            ProgressionWeaponSpec spec;
            if (!ProgressionWeaponCatalog.TryGetByGuid(guid, out spec))
                throw new ArgumentException(
                    "Only authorized progression identities can be recorded.",
                    "guid");
            if (Has(guid)) return false;
            _entries.Add(guid);
            _entries.Sort(StringComparer.Ordinal);
            return true;
        }

        internal string[] Snapshot()
        {
            return _entries.ToArray();
        }
    }
}
