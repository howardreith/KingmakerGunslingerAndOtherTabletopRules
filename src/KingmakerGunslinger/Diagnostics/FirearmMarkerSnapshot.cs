using System;
using System.Collections.Generic;
using KingmakerGunslinger.Firearms;

namespace KingmakerGunslinger.Diagnostics
{
    /// <summary>
    /// Immutable copy of firearm identity. It contains no item, blueprint, unit, or rule object.
    /// </summary>
    internal sealed class FirearmMarkerSnapshot
    {
        internal FirearmMarkerSnapshot(
            bool hasWeapon,
            int markerCount,
            FirearmDefinition definition,
            string weapon,
            string weaponRuntimeId,
            string itemBlueprint,
            string itemBlueprintId,
            string weaponType,
            string weaponTypeId)
        {
            if (markerCount < -1)
            {
                throw new ArgumentOutOfRangeException("markerCount");
            }

            if (definition != null && markerCount != 1)
            {
                throw new ArgumentException(
                    "A firearm definition can only accompany exactly one marker.",
                    "definition");
            }

            HasWeapon = hasWeapon;
            MarkerCount = markerCount;
            Definition = definition;
            Weapon = Normalize(weapon);
            WeaponRuntimeId = Normalize(weaponRuntimeId);
            ItemBlueprint = Normalize(itemBlueprint);
            ItemBlueprintId = Normalize(itemBlueprintId);
            WeaponType = Normalize(weaponType);
            WeaponTypeId = Normalize(weaponTypeId);
        }

        internal bool HasWeapon { get; private set; }

        internal int MarkerCount { get; private set; }

        internal FirearmDefinition Definition { get; private set; }

        internal string Weapon { get; private set; }

        internal string WeaponRuntimeId { get; private set; }

        internal string ItemBlueprint { get; private set; }

        internal string ItemBlueprintId { get; private set; }

        internal string WeaponType { get; private set; }

        internal string WeaponTypeId { get; private set; }

        internal bool IsExactFirearm
        {
            get { return HasWeapon && MarkerCount == 1 && Definition != null; }
        }

        internal void AddFields(IDictionary<string, string> fields, string source)
        {
            if (fields == null)
            {
                throw new ArgumentNullException("fields");
            }

            fields["markerSource"] = Normalize(source);
            fields["weapon"] = Weapon;
            fields["weaponRuntimeId"] = WeaponRuntimeId;
            fields["itemBlueprint"] = ItemBlueprint;
            fields["itemBlueprintId"] = ItemBlueprintId;
            fields["weaponType"] = WeaponType;
            fields["weaponTypeId"] = WeaponTypeId;
            fields["firearmDefinition"] = Definition == null
                ? "<unavailable>"
                : Definition.ToString();
        }

        internal static FirearmMarkerSnapshot NoWeapon()
        {
            return new FirearmMarkerSnapshot(
                false,
                -1,
                null,
                "<unavailable>",
                "<unavailable>",
                "<unavailable>",
                "<unavailable>",
                "<unavailable>",
                "<unavailable>");
        }

        private static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? "<unavailable>" : value.Trim();
        }
    }
}
