using System;
using System.Linq;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility;
using KingmakerGunslinger.Bootstrap;

namespace KingmakerGunslinger.Gunsmithing
{
    internal static class BatteredFirearmOriginRuntime
    {
        internal static void Bind(ItemEntityWeapon item, UnitEntityData owner)
        {
            if (item == null) throw new ArgumentNullException("item");
            if (owner == null || string.IsNullOrWhiteSpace(owner.UniqueId))
                throw new ArgumentException(
                    "A battered firearm origin requires an exact stable unit.",
                    "owner");
            if (TryGetOwner(item, out UnitEntityData existing))
            {
                if (SameStableOwner(existing, owner)) return;
                throw new InvalidOperationException(
                    "A battered firearm cannot be rebound to another origin.");
            }

            BlueprintWeaponEnchantment blueprint = RequireBlueprint();
            var context = new MechanicsContext(owner, owner.Descriptor,
                blueprint, null, new TargetWrapper(owner));
            ItemEnchantment added = item.AddEnchantment(blueprint, context, null);
            if (added == null || added.ParentContext == null ||
                !ReferenceEquals(added.ParentContext.MaybeCaster, owner))
                throw new InvalidOperationException(
                    "Kingmaker did not retain the exact battered origin context.");
            RequireSingle(item);
        }

        internal static bool TryGetOwner(ItemEntityWeapon item,
            out UnitEntityData owner)
        {
            owner = null;
            if (item == null) return false;
            ItemEnchantment[] matches = Matches(item);
            if (matches.Length == 0) return false;
            if (matches.Length != 1)
                throw new InvalidOperationException(
                    "A firearm carries multiple battered origin tokens.");
            MechanicsContext context = matches[0].ParentContext;
            owner = context == null ? null : context.MaybeCaster;
            if (owner == null || string.IsNullOrWhiteSpace(owner.UniqueId))
                throw new InvalidOperationException(
                    "The battered origin token exposes no exact stable owner.");
            return true;
        }

        internal static bool IsBattered(ItemEntity item)
        {
            ItemEntityWeapon weapon = item as ItemEntityWeapon;
            if (weapon == null) return false;
            UnitEntityData ignored;
            return TryGetOwner(weapon, out ignored);
        }

        /// <summary>
        /// Read-only inspection for UI and pricing hooks that see every weapon.
        /// When blueprint initialization has not completed, or failed, the
        /// origin token blueprint was never registered in this process, so no
        /// live item can carry it and the weapon is reported as not battered
        /// (its native behavior stays untouched). A completed initialization
        /// without the token is an invariant violation and still throws, as do
        /// malformed tokens on a weapon. Creation, transfer and ownership checks
        /// keep using <see cref="Bind"/> and <see cref="TryGetOwner"/>, which
        /// require the blueprint.
        /// </summary>
        internal static bool IsBatteredForInspection(ItemEntity item)
        {
            ItemEntityWeapon weapon = item as ItemEntityWeapon;
            if (weapon == null) return false;
            BatteredOriginInspection inspection =
                BatteredFirearmOriginInspectionPolicy.Decide(
                    BlueprintBootstrap.IsInitialized,
                    BlueprintBootstrap.IsFailed,
                    BlueprintBootstrap.BatteredOrigin != null);
            if (inspection == BatteredOriginInspection.Unavailable)
            {
                ReportUnavailableOnce();
                return false;
            }
            UnitEntityData ignored;
            return TryGetOwner(weapon, out ignored);
        }

        private static int _unavailableReported;

        internal static bool UnavailableReported
        {
            get
            {
                return System.Threading.Interlocked.CompareExchange(
                    ref _unavailableReported, 0, 0) != 0;
            }
        }

        private static void ReportUnavailableOnce()
        {
            if (System.Threading.Interlocked.Exchange(ref _unavailableReported, 1) != 0)
                return;
            ModContext context;
            if (!ModContext.TryGet(out context)) return;
            context.Logger.Warning("gunsmithing", "battered-origin.inspection-unavailable",
                "Blueprint initialization " + (BlueprintBootstrap.IsFailed
                    ? "failed" : "has not completed") +
                ", so the battered firearm origin token is not registered in this process. Weapon price and tooltip inspection keep native values; battered creation and transfer stay disabled. Reported once.");
        }

        internal static bool SameStableOwner(UnitEntityData left,
            UnitEntityData right)
        {
            return left != null && right != null &&
                !string.IsNullOrWhiteSpace(left.UniqueId) &&
                string.Equals(left.UniqueId, right.UniqueId,
                    StringComparison.Ordinal);
        }

        private static void RequireSingle(ItemEntityWeapon item)
        {
            if (Matches(item).Length != 1)
                throw new InvalidOperationException(
                    "The battered origin token was not attached exactly once.");
        }

        private static ItemEnchantment[] Matches(ItemEntityWeapon item)
        {
            BlueprintWeaponEnchantment blueprint = RequireBlueprint();
            return item.Enchantments.Where(value => value != null &&
                ReferenceEquals(value.Blueprint, blueprint)).ToArray();
        }

        private static BlueprintWeaponEnchantment RequireBlueprint()
        {
            BlueprintWeaponEnchantment value = BlueprintBootstrap.BatteredOrigin;
            if (value == null)
                throw new InvalidOperationException(
                    "The battered origin token blueprint is unavailable" +
                    (BlueprintBootstrap.IsFailed
                        ? " because blueprint initialization failed."
                        : BlueprintBootstrap.IsInitialized
                            ? " although blueprint initialization completed."
                            : " because blueprint initialization has not completed."));
            return value;
        }
    }
}
