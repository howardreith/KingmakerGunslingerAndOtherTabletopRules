using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.Items;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Parts;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;

namespace KingmakerGunslinger.Acquisition.BetterVendors
{
    /// <summary>
    /// Live stock mutations for the optional Better Vendors integration. Every
    /// Harmony callback here is fail-soft: an exception is logged and swallowed
    /// so it can never abort a Better Vendors stock pass (which would also lose
    /// that pass's unrelated stock) or a trading session.
    ///
    /// Additions use the same native shared-table operation Better Vendors'
    /// AddItemsToVendor action performs (ItemsCollection.Add on
    /// SharedVendorTables.GetTable), into the one shared table that both the
    /// capital blacksmith and the throne-room clone read. Nothing is ever
    /// removed, cleared or normalized.
    /// </summary>
    internal static class BetterVendorsStockRuntime
    {
        private static readonly FieldInfo SharedInventoryField =
            typeof(UnitPartVendor).GetField("m_SharedInventory",
                BindingFlags.Instance | BindingFlags.NonPublic |
                BindingFlags.Public);
        private static readonly object FailureGate = new object();
        private static readonly HashSet<string> ReportedFailures =
            new HashSet<string>(StringComparer.Ordinal);
        private static long _stockCalls;
        private static long _catchUps;
        private static long _failures;

        internal static long StockCallsApplied
        {
            get { return System.Threading.Interlocked.Read(ref _stockCalls); }
        }

        internal static long CatchUpsApplied
        {
            get { return System.Threading.Interlocked.Read(ref _catchUps); }
        }

        internal static long Failures
        {
            get { return System.Threading.Interlocked.Read(ref _failures); }
        }

        // ProgressionLogic.AddStock postfix. The verified AddStock swallows
        // every exception raised by its stock calls, so this always runs after
        // any Military call and closes the pass scope.
        internal static void AddStockPostfix()
        {
            BetterVendorsStockPass.EndPass();
        }

        // ProgressionLogic.AddMilitaryStock(int rank) prefix.
        internal static void AddMilitaryStockPrefix(int __0)
        {
            try
            {
                Player player = Game.Instance == null ? null : Game.Instance.Player;
                if (player == null) return;
                BetterVendorsStockPass.BeginMilitaryCall(player, __0,
                    CurrentMilitaryRank(player));
            }
            catch (Exception exception)
            {
                ReportFailure("stock-call.begin", exception);
            }
        }

        // ProgressionLogic.AddMilitaryStock(int rank) postfix; runs only when
        // Better Vendors' own tier operation completed without an exception.
        internal static void AddMilitaryStockPostfix(int __0)
        {
            BetterVendorsStockCall call = BetterVendorsStockPass.OpenCall;
            if (call == null || call.CalledRank != __0 ||
                !BetterVendorsStockPass.CompleteMilitaryCall(call))
                return;
            try
            {
                ApplyStockCall(call);
            }
            catch (Exception exception)
            {
                ReportFailure("stock-call.apply", exception);
            }
        }

        // ProgressionLogic.GetFilterWeapons postfix: read-only observation.
        // The result list is never modified, so no Better Vendors caller —
        // progression, elemental, bow or any future query — sees a change.
        internal static void GetFilterWeaponsPostfix(List<string> __0,
            List<string> __1, List<string> __2, WeaponCategory __3, bool __4,
            List<BlueprintItemWeapon> __result)
        {
            try
            {
                BetterVendorsStockCall call = BetterVendorsStockPass.OpenCall;
                if (call == null || !BetterVendorsStockPass.IsOrdinaryTierQuery(
                        call, __0, __1, __2, __3 == WeaponCategory.Touch, __4))
                    return;
                ProgressionWeaponBlueprintCatalog catalog =
                    BlueprintBootstrap.ProgressionWeapons;
                ProgressionWeaponBlueprintEntry ignored;
                call.ObserveOrdinaryQuery(__result == null || catalog == null
                    ? Enumerable.Empty<string>()
                    : __result.Where(value => value != null &&
                            catalog.TryGet(value.AssetGuid, out ignored))
                        .Select(value => value.AssetGuid).ToArray());
            }
            catch (Exception exception)
            {
                ReportFailure("stock-call.observe", exception);
            }
        }

        // VendorLogic.BeginTrading(UnitEntityData) postfix: one-time catch-up
        // for the shared Military destination only.
        internal static void BeginTradingPostfix(UnitEntityData __0)
        {
            try
            {
                TryCatchUp(__0, "trading-started");
            }
            catch (Exception exception)
            {
                ReportFailure("catch-up", exception);
            }
        }

        internal static bool IsMilitaryDestinationVendor(UnitEntityData vendorUnit)
        {
            if (vendorUnit == null || vendorUnit.Descriptor == null ||
                SharedInventoryField == null)
                return false;
            UnitPartVendor part = vendorUnit.Descriptor.Get<UnitPartVendor>();
            BlueprintSharedVendorTable table = part == null ? null :
                SharedInventoryField.GetValue(part) as BlueprintSharedVendorTable;
            return table != null && string.Equals(table.AssetGuid,
                BetterVendorsContract.MilitaryDestinationTableGuid,
                StringComparison.Ordinal);
        }

        /// <summary>
        /// Applies missing initial grants for every reached milestone, exactly
        /// once per entry per save. Returns the number of entries granted.
        /// </summary>
        internal static int TryCatchUp(UnitEntityData vendorUnit, string checkpoint)
        {
            if (!IsMilitaryDestinationVendor(vendorUnit)) return 0;
            BetterVendorsCompatibilityCoordinator.EnsureResolved(checkpoint);
            string inactive;
            if (!BetterVendorsCompatibilityCoordinator.TryGetActiveProgression(
                    out inactive))
                return 0;
            Player player = Game.Instance == null ? null : Game.Instance.Player;
            ProgressionWeaponBlueprintCatalog catalog =
                BlueprintBootstrap.ProgressionWeapons;
            if (player == null || player.Kingdom == null || catalog == null ||
                !catalog.Status.IsUsable)
                return 0;
            int rank = CurrentMilitaryRank(player);
            LedgerAccess ledger = LedgerAccess.For(player);
            if (ledger == null) return 0;
            BetterVendorsGrantPlan plan = BetterVendorsGrantPlanner.PlanCatchUp(
                catalog.Specs, CurrentModules(), ledger.Has, true, rank);
            if (plan.Grants.Length == 0) return 0;
            ItemsCollection inventory = ResolveDestination(player);
            if (inventory == null) return 0;
            BetterVendorsGrantOutcome outcome = ApplyGrants(plan.Grants, catalog,
                inventory, ledger);
            System.Threading.Interlocked.Increment(ref _catchUps);
            Log("better-vendors", "catch-up.applied", string.Format(
                CultureInfo.InvariantCulture,
                "checkpoint={0};militaryRank={1};tiers={2};{3};suppressedByModule={4};ledger={5}",
                checkpoint, rank, string.Join(",", plan.Grants.Select(value =>
                    value.Tier.Tier.ToString(CultureInfo.InvariantCulture))
                    .Distinct().ToArray()), outcome, plan.ModuleSuppressed.Length,
                ledger.Count));
            return outcome.Granted;
        }

        internal static void ApplyStockCall(BetterVendorsStockCall call)
        {
            if (call == null || call.Tier == null) return;
            ProgressionWeaponBlueprintCatalog catalog =
                BlueprintBootstrap.ProgressionWeapons;
            string inactive;
            Player player = Game.Instance == null ? null : Game.Instance.Player;
            if (catalog == null || !catalog.Status.IsUsable || player == null ||
                player.Kingdom == null ||
                !BetterVendorsCompatibilityCoordinator.TryGetActiveProgression(
                    out inactive))
                return;
            LedgerAccess ledger = LedgerAccess.For(player);
            if (ledger == null) return;
            BetterVendorsGrantPlan plan = BetterVendorsGrantPlanner.PlanStockCall(
                catalog.Specs, CurrentModules(), ledger.Has, call);
            foreach (ProgressionWeaponSpec delivered in plan.NativelyDelivered)
                ledger.Record(delivered.Guid);
            var outcome = new BetterVendorsGrantOutcome();
            if (plan.Grants.Length != 0)
            {
                ItemsCollection inventory = ResolveDestination(player);
                if (inventory == null) return;
                outcome = ApplyGrants(plan.Grants, catalog, inventory, ledger);
            }
            System.Threading.Interlocked.Increment(ref _stockCalls);
            Log("better-vendors", "stock-call.applied", string.Format(
                CultureInfo.InvariantCulture,
                "calledRank={0};currentMilitaryRank={1};sequence={2};kind={3};tier={4};quantity={5};{6};nativelyDelivered={7};suppressedByModule={8};ordinaryQueryObserved={9};ledger={10}",
                call.CalledRank, call.CurrentMilitaryRank, call.Sequence,
                call.Kind, call.Tier.Tier, call.Tier.Quantity, outcome,
                plan.NativelyDelivered.Length, plan.ModuleSuppressed.Length,
                call.OrdinaryQueryObserved, ledger.Count));
        }

        internal static int CurrentMilitaryRank(Player player)
        {
            if (player == null || player.Kingdom == null ||
                player.Kingdom.Stats == null || player.Kingdom.Stats.Military == null)
                return -1;
            return player.Kingdom.Stats.Military.Rank;
        }

        internal static ProgressionModuleState CurrentModules()
        {
            ModContext context;
            if (!ModContext.TryGet(out context) || context.FeatureModules == null)
                return new ProgressionModuleState(false, false, false);
            FeatureModules.FeatureModuleConfiguration active =
                context.FeatureModules.Active;
            return new ProgressionModuleState(active.Gunslinger,
                active.EasternWeapons, active.ElvenBranchedSpears);
        }

        /// <summary>
        /// Access to the loaded campaign's ledger. The persisted part is
        /// created only just before the first stock addition that must be
        /// recorded, so campaigns that never receive a grant carry no new save
        /// data.
        /// </summary>
        internal sealed class LedgerAccess
        {
            private readonly UnitDescriptor _owner;
            private ProgressionGrantLedger _ledger;

            private LedgerAccess(UnitDescriptor owner)
            {
                _owner = owner;
                UnitPartBetterVendorsProgressionGrants part =
                    owner.Get<UnitPartBetterVendorsProgressionGrants>();
                _ledger = part == null ? null : part.Ledger;
            }

            internal static LedgerAccess For(Player player)
            {
                UnitEntityData main = player == null ? null :
                    player.MainCharacter.Value;
                UnitDescriptor descriptor = main == null ? null : main.Descriptor;
                return descriptor == null ? null : new LedgerAccess(descriptor);
            }

            internal int Count { get { return _ledger == null ? 0 : _ledger.Count; } }

            internal bool Has(string guid)
            {
                return _ledger != null && _ledger.Has(guid);
            }

            /// <summary>
            /// Creates the persisted part if it does not exist yet. Called
            /// before any stock mutation that will need recording, so a
            /// failure here leaves the merchant untouched instead of leaving
            /// added copies unrecorded.
            /// </summary>
            internal void EnsureWritable()
            {
                if (_ledger == null)
                    _ledger = _owner.Ensure<UnitPartBetterVendorsProgressionGrants>()
                        .Ledger;
            }

            internal bool Record(string guid)
            {
                EnsureWritable();
                return _ledger.Record(guid);
            }

            internal string[] Snapshot()
            {
                return _ledger == null ? new string[0] : _ledger.Snapshot();
            }
        }

        internal static ItemsCollection ResolveDestination(Player player)
        {
            LibraryScriptableObject library = BlueprintBootstrap.Library;
            if (player == null || library == null) return null;
            BlueprintSharedVendorTable table =
                BlueprintLibraryLookup.RequireExact<BlueprintSharedVendorTable>(
                    library, BetterVendorsContract.MilitaryDestinationTableGuid,
                    "Better Vendors Military stock destination");
            if (!string.Equals(table.name, CapitalVendorBlueprints.ExpectedTableName,
                    StringComparison.Ordinal) ||
                !string.Equals(table.AssetGuid, CapitalVendorBlueprints.TableGuid,
                    StringComparison.Ordinal))
                throw new InvalidOperationException(
                    "The Better Vendors Military destination is not the capital blacksmith shared table.");
            return player.SharedVendorTables.GetTable(table);
        }

        /// <summary>
        /// Adds each planned grant with the native shared-table operation and
        /// records an initial grant only after its stock mutation is observed
        /// (see <see cref="BetterVendorsGrantApplier"/>).
        /// </summary>
        private static BetterVendorsGrantOutcome ApplyGrants(
            BetterVendorsGrant[] grants, ProgressionWeaponBlueprintCatalog catalog,
            ItemsCollection inventory, LedgerAccess ledger)
        {
            if (grants.Any(value =>
                    value.Reason == BetterVendorsGrantReason.InitialGrant))
                ledger.EnsureWritable();
            BetterVendorsGrantOutcome outcome = BetterVendorsGrantApplier.Apply(
                grants,
                spec => inventory.Count(catalog.Require(spec.Guid).Item),
                (spec, quantity) => inventory.Add(catalog.Require(spec.Guid).Item,
                    quantity),
                ledger.Record);
            foreach (Exception failure in outcome.Errors)
                ReportFailure("grant", failure);
            return outcome;
        }

        private static void Log(string phase, string eventName, string message)
        {
            ModContext context;
            if (ModContext.TryGet(out context))
                context.Logger.Info(phase, eventName, message);
        }

        internal static void ReportFailure(string phase, Exception exception)
        {
            System.Threading.Interlocked.Increment(ref _failures);
            string key = phase + "|" + (exception == null ? string.Empty :
                exception.GetType().FullName + ":" + exception.Message);
            bool first;
            lock (FailureGate) first = ReportedFailures.Add(key);
            ModContext context;
            if (!ModContext.TryGet(out context)) return;
            if (first)
                context.Logger.Failure("better-vendors", phase + ".failed",
                    "The Better Vendors progression integration failed closed for this event; Better Vendors stock and trading continue unmodified.",
                    exception);
            else
                context.Logger.Warning("better-vendors", phase + ".repeated",
                    "A previously reported integration failure recurred; failures=" +
                    Failures.ToString(CultureInfo.InvariantCulture) + ".");
        }

    }
}
