using System;
using System.Collections.Generic;
using System.Linq;

namespace KingmakerGunslinger.Acquisition.BetterVendors
{
    internal enum BetterVendorsStockCallKind
    {
        /// <summary>
        /// A lower-rank call, or any call after the first one, inside one
        /// Better Vendors stock pass. Better Vendors makes these calls only
        /// while catching a save up for the first time, so an entry whose
        /// initial grant is already recorded is never stocked again by them.
        /// </summary>
        CatchUp = 0,

        /// <summary>
        /// The single call for the CURRENT Military rank that starts a pass.
        /// Better Vendors repeats it on every later Arcane, Divine, Military
        /// or Stability improvement; mirroring it preserves that dependency's
        /// own replenishment behavior for the current tier.
        /// </summary>
        CurrentTier = 1,

        /// <summary>A call the verified contract never makes; nothing is stocked.</summary>
        Unsupported = 2
    }

    /// <summary>
    /// One observed ProgressionLogic.AddMilitaryStock invocation.
    /// </summary>
    internal sealed class BetterVendorsStockCall
    {
        private readonly HashSet<string> _nativeSelections =
            new HashSet<string>(StringComparer.Ordinal);

        internal BetterVendorsStockCall(int calledRank, int currentMilitaryRank,
            int sequence, BetterVendorsStockCallKind kind)
        {
            CalledRank = calledRank;
            CurrentMilitaryRank = currentMilitaryRank;
            Sequence = sequence;
            Kind = kind;
            BetterVendorsProgressionTier tier;
            Tier = kind != BetterVendorsStockCallKind.Unsupported &&
                BetterVendorsProgressionSchedule.TryGetTierForMilitaryStockRank(
                    calledRank, out tier) ? tier : null;
        }

        internal int CalledRank { get; private set; }
        internal int CurrentMilitaryRank { get; private set; }

        /// <summary>Zero-based position of this call inside its pass.</summary>
        internal int Sequence { get; private set; }

        internal BetterVendorsStockCallKind Kind { get; private set; }

        /// <summary>The ordinary tier this call stocks, or null for ranks without one.</summary>
        internal BetterVendorsProgressionTier Tier { get; private set; }

        internal bool OrdinaryQueryObserved { get; private set; }
        internal bool Completed { get; private set; }

        /// <summary>
        /// Authorized catalog identities that Better Vendors' own ordinary
        /// query already selected in this call (and therefore stocked itself).
        /// </summary>
        internal string[] NativeSelections
        {
            get
            {
                return _nativeSelections.OrderBy(value => value,
                    StringComparer.Ordinal).ToArray();
            }
        }

        internal bool WasNativelySelected(string guid)
        {
            return guid != null && _nativeSelections.Contains(guid);
        }

        internal void ObserveOrdinaryQuery(IEnumerable<string> selectedCatalogGuids)
        {
            OrdinaryQueryObserved = true;
            if (selectedCatalogGuids == null) return;
            foreach (string guid in selectedCatalogGuids)
                if (!string.IsNullOrEmpty(guid)) _nativeSelections.Add(guid);
        }

        internal void MarkCompleted() { Completed = true; }
    }

    /// <summary>
    /// Thread-local scope of one Better Vendors ProgressionLogic.AddStock pass.
    /// A pass is created lazily by its first Military stock call and is always
    /// ended by the AddStock postfix. The verified AddStock swallows every
    /// exception raised inside its stock calls, so a failed call still reaches
    /// that postfix; a call can therefore never leak into a later pass, save or
    /// thread. The owning player object is recorded so a pass can never be
    /// reused for a different loaded campaign.
    /// </summary>
    internal sealed class BetterVendorsStockPass
    {
        [ThreadStatic]
        private static BetterVendorsStockPass _current;

        private readonly object _owner;
        private int _militaryCalls;
        private BetterVendorsStockCall _openCall;

        private BetterVendorsStockPass(object owner)
        {
            _owner = owner;
        }

        internal static bool HasOpenPass { get { return _current != null; } }

        /// <summary>The Military stock call currently executing, if any.</summary>
        internal static BetterVendorsStockCall OpenCall
        {
            get
            {
                BetterVendorsStockPass pass = _current;
                return pass == null ? null : pass._openCall;
            }
        }

        internal static BetterVendorsStockCall BeginMilitaryCall(object owner,
            int calledRank, int currentMilitaryRank)
        {
            if (owner == null) throw new ArgumentNullException("owner");
            BetterVendorsStockPass pass = _current;
            if (pass == null || !ReferenceEquals(pass._owner, owner))
            {
                pass = new BetterVendorsStockPass(owner);
                _current = pass;
            }
            int sequence = pass._militaryCalls++;
            var call = new BetterVendorsStockCall(calledRank,
                currentMilitaryRank, sequence,
                Classify(calledRank, currentMilitaryRank, sequence));
            pass._openCall = call;
            return call;
        }

        /// <summary>
        /// Closes the exact open call. Returns false (and changes nothing) for
        /// a stale or foreign call object.
        /// </summary>
        internal static bool CompleteMilitaryCall(BetterVendorsStockCall call)
        {
            BetterVendorsStockPass pass = _current;
            if (call == null || pass == null ||
                !ReferenceEquals(pass._openCall, call))
                return false;
            pass._openCall = null;
            call.MarkCompleted();
            return true;
        }

        internal static void EndPass()
        {
            _current = null;
        }

        internal static BetterVendorsStockCallKind Classify(int calledRank,
            int currentMilitaryRank, int sequence)
        {
            if (sequence < 0 || calledRank < 0 || currentMilitaryRank < 0 ||
                calledRank > BetterVendorsProgressionSchedule.MaximumMilitaryRank ||
                currentMilitaryRank > BetterVendorsProgressionSchedule.MaximumMilitaryRank ||
                calledRank > currentMilitaryRank)
                return BetterVendorsStockCallKind.Unsupported;
            return sequence == 0 && calledRank == currentMilitaryRank
                ? BetterVendorsStockCallKind.CurrentTier
                : BetterVendorsStockCallKind.CatchUp;
        }

        /// <summary>
        /// True only for the verified ordinary enhancement-tier query of the
        /// open call: exactly the tier's native enhancement GUID, no second or
        /// third enchantment list, the default (all-category) sentinel and the
        /// default unique-by-name grouping. Elemental, bow-specific,
        /// category-specific and composite/thrown/oversized queries never match.
        /// </summary>
        internal static bool IsOrdinaryTierQuery(BetterVendorsStockCall call,
            IList<string> allowedEnchantments1, object allowedEnchantments2,
            object allowedEnchantments3, bool defaultCategory,
            bool uniqueByName)
        {
            return call != null && call.Tier != null &&
                allowedEnchantments1 != null &&
                allowedEnchantments1.Count == 1 &&
                string.Equals(allowedEnchantments1[0],
                    BetterVendorsContract.EnhancementGuid(call.Tier.Tier),
                    StringComparison.Ordinal) &&
                allowedEnchantments2 == null && allowedEnchantments3 == null &&
                defaultCategory && uniqueByName;
        }
    }
}
