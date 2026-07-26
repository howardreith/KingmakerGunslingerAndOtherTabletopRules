using System;

namespace KingmakerGunslinger.Firearms
{
    /// <summary>
    /// Transient resolution result containing the exact runtime item key and immutable
    /// descriptive metadata. It must not be retained beyond the current operation.
    /// </summary>
    internal sealed class ResolvedFirearmItem
    {
        internal ResolvedFirearmItem(
            object itemInstance,
            FirearmDefinition definition,
            string itemDisplayName,
            string itemRuntimeId,
            string itemBlueprintName,
            string itemBlueprintId,
            string weaponTypeName,
            string weaponTypeId)
        {
            if (itemInstance == null)
            {
                throw new ArgumentNullException("itemInstance");
            }

            if (itemInstance.GetType().IsValueType)
            {
                throw new ArgumentException(
                    "A resolved firearm item must be a reference-type runtime object.",
                    "itemInstance");
            }

            ItemInstance = itemInstance;
            Definition = definition ?? throw new ArgumentNullException("definition");
            ItemDisplayName = Normalize(itemDisplayName);
            ItemRuntimeId = Normalize(itemRuntimeId);
            ItemBlueprintName = Normalize(itemBlueprintName);
            ItemBlueprintId = Normalize(itemBlueprintId);
            WeaponTypeName = Normalize(weaponTypeName);
            WeaponTypeId = Normalize(weaponTypeId);
        }

        internal object ItemInstance { get; private set; }

        internal FirearmDefinition Definition { get; private set; }

        internal string ItemDisplayName { get; private set; }

        internal string ItemRuntimeId { get; private set; }

        internal string ItemBlueprintName { get; private set; }

        internal string ItemBlueprintId { get; private set; }

        internal string WeaponTypeName { get; private set; }

        internal string WeaponTypeId { get; private set; }

        private static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? "<unavailable>" : value.Trim();
        }
    }
}
