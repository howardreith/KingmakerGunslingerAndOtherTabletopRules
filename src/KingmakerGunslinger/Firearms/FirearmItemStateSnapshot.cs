using System;
using System.Globalization;

namespace KingmakerGunslinger.Firearms
{
    /// <summary>
    /// Immutable diagnostic copy joining resolved firearm metadata to a repository
    /// snapshot. It retains no runtime item, blueprint, unit, or inventory object.
    /// </summary>
    internal sealed class FirearmItemStateSnapshot
    {
        internal FirearmItemStateSnapshot(
            ResolvedFirearmItem firearm,
            FirearmStateRepositorySnapshot repository)
        {
            if (firearm == null)
            {
                throw new ArgumentNullException("firearm");
            }

            Repository = repository ?? throw new ArgumentNullException("repository");
            Definition = firearm.Definition;
            ItemDisplayName = firearm.ItemDisplayName;
            ItemRuntimeId = firearm.ItemRuntimeId;
            ItemBlueprintName = firearm.ItemBlueprintName;
            ItemBlueprintId = firearm.ItemBlueprintId;
            WeaponTypeName = firearm.WeaponTypeName;
            WeaponTypeId = firearm.WeaponTypeId;
        }

        internal FirearmStateRepositorySnapshot Repository { get; private set; }

        internal FirearmDefinition Definition { get; private set; }

        internal string ItemDisplayName { get; private set; }

        internal string ItemRuntimeId { get; private set; }

        internal string ItemBlueprintName { get; private set; }

        internal string ItemBlueprintId { get; private set; }

        internal string WeaponTypeName { get; private set; }

        internal string WeaponTypeId { get; private set; }

        public override string ToString()
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "{0}; item={1}; runtimeId={2}; itemBlueprint={3}/{4}; weaponType={5}/{6}; definition=[{7}]",
                Repository,
                ItemDisplayName,
                ItemRuntimeId,
                ItemBlueprintName,
                ItemBlueprintId,
                WeaponTypeName,
                WeaponTypeId,
                Definition);
        }
    }
}
