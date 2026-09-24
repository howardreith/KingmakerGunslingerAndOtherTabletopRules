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

        /// <summary>
        /// Grants confirmed to have changed nothing (or never attempted); an
        /// initial grant among them stays eligible for a later trigger.
        /// </summary>
        internal int Failed { get; set; }

        /// <summary>
        /// Grants whose stock outcome could not be established. An initial
        /// grant among them keeps its ledger claim, so it is never repeated
        /// automatically.
        /// </summary>
        internal int Uncertain { get; set; }

        /// <summary>Initial grants skipped because the ledger already held them.</summary>
        internal int Skipped { get; set; }

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
                "granted={0};replenished={1};copies={2};partial={3};failed={4};uncertain={5};skipped={6}",
                Granted, Replenished, Copies, Partial, Failed, Uncertain, Skipped);
        }
    }

    /// <summary>
    /// Applies planned grants through supplied stock operations.
    ///
    /// An initial grant is claimed in the ledger BEFORE its stock is added
    /// (write-ahead), so no failure after the stock changes can make the
    /// entry look ungranted. The claim is released only when the addition is
    /// positively confirmed to have changed nothing; that grant stays eligible
    /// for a later trigger. Every other outcome keeps the claim: a complete
    /// or partial addition (never topped up), an addition whose result cannot
    /// be observed, and a claim that cannot be released. So a stock change
    /// with failed or uncertain bookkeeping is never repeated automatically.
    /// The ledger and the merchant stock live in the same save, so they
    /// persist or are discarded together; the game offers no stronger
    /// transaction.
    /// </summary>
    internal static class BetterVendorsGrantApplier
    {
        internal static BetterVendorsGrantOutcome Apply(
            IEnumerable<BetterVendorsGrant> grants,
            Func<ProgressionWeaponSpec, int> count,
            Action<ProgressionWeaponSpec, int> add, Func<string, bool> record,
            Func<string, bool> withdraw)
        {
            if (grants == null) throw new ArgumentNullException("grants");
            if (count == null) throw new ArgumentNullException("count");
            if (add == null) throw new ArgumentNullException("add");
            if (record == null) throw new ArgumentNullException("record");
            if (withdraw == null) throw new ArgumentNullException("withdraw");
            var outcome = new BetterVendorsGrantOutcome();
            foreach (BetterVendorsGrant grant in grants)
            {
                bool initial = grant.Reason == BetterVendorsGrantReason.InitialGrant;
                int before;
                if (!TryCount(count, grant.Spec, outcome, out before))
                {
                    // Nothing was claimed or changed; the grant stays eligible.
                    outcome.Failed++;
                    continue;
                }
                if (initial)
                {
                    bool claimed;
                    try
                    {
                        claimed = record(grant.Spec.Guid);
                    }
                    catch (Exception exception)
                    {
                        // The claim failed before any stock changed.
                        outcome.Errors.Add(exception);
                        outcome.Failed++;
                        continue;
                    }
                    if (!claimed)
                    {
                        // Already granted: never add a second initial grant.
                        outcome.Skipped++;
                        continue;
                    }
                }
                try
                {
                    add(grant.Spec, grant.Quantity);
                }
                catch (Exception exception)
                {
                    outcome.Errors.Add(exception);
                }
                int after;
                if (!TryCount(count, grant.Spec, outcome, out after) ||
                    after < before)
                {
                    // The result cannot be established; an initial grant keeps
                    // its claim so it is never repeated automatically.
                    outcome.Uncertain++;
                    continue;
                }
                int added = after - before;
                if (added == 0)
                {
                    // Positively confirmed that nothing was added: release the
                    // claim so a later trigger retries this grant. A claim
                    // that cannot be released keeps the grant ineligible.
                    if (initial && !TryWithdraw(withdraw, grant.Spec.Guid, outcome))
                        outcome.Uncertain++;
                    else
                        outcome.Failed++;
                    continue;
                }
                outcome.Copies += added;
                if (added != grant.Quantity) outcome.Partial++;
                if (initial)
                {
                    outcome.Granted++;
                    outcome.Recorded.Add(grant.Spec.Guid);
                }
                else
                {
                    outcome.Replenished++;
                }
            }
            return outcome;
        }

        private static bool TryCount(Func<ProgressionWeaponSpec, int> count,
            ProgressionWeaponSpec spec, BetterVendorsGrantOutcome outcome,
            out int value)
        {
            try
            {
                value = count(spec);
                return true;
            }
            catch (Exception exception)
            {
                outcome.Errors.Add(exception);
                value = 0;
                return false;
            }
        }

        private static bool TryWithdraw(Func<string, bool> withdraw, string guid,
            BetterVendorsGrantOutcome outcome)
        {
            try
            {
                withdraw(guid);
                return true;
            }
            catch (Exception exception)
            {
                outcome.Errors.Add(exception);
                return false;
            }
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

        /// <summary>
        /// Releases a write-ahead claim. Only the grant applier calls this,
        /// for a claim it made moments earlier, after positively confirming
        /// that the grant added nothing. It never touches merchant stock.
        /// Returns true when the identity was present.
        /// </summary>
        internal bool Withdraw(string guid)
        {
            ProgressionWeaponSpec spec;
            if (!ProgressionWeaponCatalog.TryGetByGuid(guid, out spec))
                throw new ArgumentException(
                    "Only authorized progression identities can be withdrawn.",
                    "guid");
            return _entries.Remove(guid);
        }

        internal string[] Snapshot()
        {
            return _entries.ToArray();
        }
    }
}
