using System;
using System.Collections.Generic;
using System.Linq;
using Harmony12;
using Kingmaker;
using Kingmaker.Blueprints.Items;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Development;

namespace KingmakerGunslinger.Acquisition
{
    /// <summary>
    /// Idempotent runtime sweep that removes the two retired consumable
    /// maintenance kits from a merchant's already-materialized inventory.
    /// Publishing the blueprint tables only fixes future stock generation;
    /// items an earlier version already placed into a saved vendor inventory
    /// persist until they are removed here. The sweep selects only exact
    /// retired blueprint matches inside the vendor's own inventory, never the
    /// shared player inventory (legacy player-owned kits survive), never
    /// unrelated merchandise, and never replenishes purchased stock.
    /// </summary>
    internal static class RetiredKitVendorStockCleanup
    {
        private static long _postfixRuns;
        private static long _postfixFaults;

        /// <summary>Process-local count of BeginTrading postfix invocations.</summary>
        internal static long PostfixRuns
        {
            get { return System.Threading.Interlocked.Read(ref _postfixRuns); }
        }

        /// <summary>Process-local count of BeginTrading postfix faults.</summary>
        internal static long PostfixFaults
        {
            get { return System.Threading.Interlocked.Read(ref _postfixFaults); }
        }

        [HarmonyPatch(typeof(VendorLogic), "BeginTrading",
            new Type[] { typeof(UnitEntityData) })]
        internal static class RetiredKitVendorTradeOpenPatch
        {
            private static void Postfix(UnitEntityData __0)
            {
                System.Threading.Interlocked.Increment(ref _postfixRuns);
                try
                {
                    CleanVendorInventory(__0);
                }
                catch (Exception exception)
                {
                    System.Threading.Interlocked.Increment(ref _postfixFaults);
                    ModContext context;
                    if (ModContext.TryGet(out context))
                    {
                        context.Logger.Failure(
                            "acquisition",
                            "retired-kit-sweep.failed",
                            "The retired maintenance-kit vendor sweep failed; trading continues with the unmodified vendor inventory.",
                            exception);
                    }
                }
            }
        }

        /// <summary>
        /// Removes every exact retired-kit item from one vendor unit's
        /// inventory. Safe to run repeatedly: a clean inventory selects
        /// nothing. Returns the number of detached item entities.
        /// </summary>
        internal static int CleanVendorInventory(UnitEntityData vendorUnit)
        {
            if (vendorUnit == null)
            {
                throw new ArgumentNullException("vendorUnit");
            }

            ItemsCollection inventory = vendorUnit.Descriptor == null
                ? null
                : vendorUnit.Descriptor.Inventory;
            return Sweep(inventory, ResolveVendorName(vendorUnit));
        }

        /// <summary>
        /// Core sweep over one inventory collection. Player-character units
        /// can expose the shared stash through Descriptor.Inventory, so the
        /// shared-player guard lives here as well as at every caller.
        /// </summary>
        internal static int Sweep(
            ItemsCollection inventory,
            string vendorName)
        {
            BlueprintItem repairKit = BlueprintBootstrap.FirearmRepairKit;
            GunsmithingSupplyBlueprintSet supplies =
                BlueprintBootstrap.GunsmithingSupplies;
            if (repairKit == null || supplies == null ||
                supplies.OverhaulKit == null)
            {
                // The identities are not initialized in this process; there is
                // nothing exact to sweep for and nothing may be removed by guess.
                return 0;
            }

            if (inventory == null || !ReflectionAccess.CanEnumerate(inventory))
            {
                return 0;
            }

            Player player = Game.Instance == null ? null : Game.Instance.Player;
            if (player != null && ReferenceEquals(inventory, player.Inventory))
            {
                // Belt-and-braces scope guard: the shared player inventory is
                // never a sweep target, even if a caller supplies it.
                return 0;
            }

            var retired = new HashSet<BlueprintItem>();
            retired.Add(repairKit);
            retired.Add(supplies.OverhaulKit);

            ItemEntity[] matches = ReflectionAccess.Enumerate(inventory)
                .OfType<ItemEntity>()
                .Where(item => item != null && item.Blueprint != null &&
                    retired.Contains(item.Blueprint))
                .ToArray();
            if (matches.Length == 0)
            {
                return 0;
            }

            int removed = 0;
            foreach (ItemEntity item in matches)
            {
                if (RemoveExact(inventory, item))
                {
                    removed++;
                }
            }

            ModContext context;
            if (ModContext.TryGet(out context))
            {
                context.Logger.Info(
                    "acquisition",
                    "retired-kit-sweep.removed",
                    RetiredVendorStockPolicy.DescribeSweep(
                        vendorName,
                        matches.Length,
                        removed));
            }

            return removed;
        }

        /// <summary>
        /// Detaches one exact item entity from its owning vendor collection
        /// using the documented full-quantity removal boundary.
        /// </summary>
        private static bool RemoveExact(
            ItemsCollection inventory,
            ItemEntity item)
        {
            object ignored;
            string method;
            if (!ReflectionAccess.TryInvokeAny(
                    inventory,
                    new[] { "Remove", "RemoveItem" },
                    new[]
                    {
                        new object[] { item, item.Count },
                        new object[] { item, item.Count, false },
                        new object[] { item }
                    },
                    out ignored,
                    out method))
            {
                throw new InvalidOperationException(
                    "No compatible vendor inventory removal contract was available for a retired maintenance kit.");
            }

            bool survived = ReflectionAccess.Enumerate(inventory)
                .OfType<ItemEntity>()
                .Any(existing => ReferenceEquals(existing, item));
            if (survived)
            {
                throw new InvalidOperationException(
                    "The exact retired vendor item survived the removal call.");
            }

            return true;
        }

        private static string ResolveVendorName(UnitEntityData vendorUnit)
        {
            object value;
            string member;
            string[] names = { "CharacterName", "Name", "name" };
            if (ReflectionAccess.TryGetFirstNonNullMember(
                    vendorUnit,
                    names,
                    out value,
                    out member) &&
                value != null &&
                !string.IsNullOrWhiteSpace(value.ToString()))
            {
                return value.ToString();
            }

            return vendorUnit.Blueprint == null
                ? "<unknown>"
                : vendorUnit.Blueprint.name;
        }
    }
}
