using System;
using System.Globalization;
using System.Reflection;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Items.Weapons;

namespace KingmakerGunslinger.Blueprints
{
    /// <summary>
    /// Authored versus effective item weight.
    /// <para>
    /// Authored weight is the serialized <c>m_Weight</c> a blueprint carries:
    /// the value this mod writes from its catalogs, or a donor's own stored
    /// field when an item deliberately inherits the donor's weight. It is what
    /// construction copies and what validation judges.
    /// </para>
    /// <para>
    /// Effective weight is what the game's public <c>Weight</c> getters return
    /// after every Harmony patch. Bag of Tricks' No Item Weight setting
    /// postfixes the Kingmaker 2.1.7b getters <c>BlueprintItem.Weight</c>,
    /// <c>BlueprintItemWeapon.Weight</c>, <c>BlueprintItemArmor.Weight</c>,
    /// <c>BlueprintItemShield.Weight</c>, <c>BlueprintWeaponType.Weight</c> and
    /// <c>BlueprintArmorType.Weight</c> to return zero while the setting is on;
    /// that is a valid, deliberate runtime override, not corrupted data. The
    /// effective getters are therefore never an input to construction or to a
    /// fatal check, and this class never reads them.
    /// </para>
    /// </summary>
    internal static class AuthoredItemWeight
    {
        private const BindingFlags Fields =
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        private static readonly FieldInfo ItemWeight =
            Require(typeof(BlueprintItem), "m_Weight");

        private static readonly FieldInfo WeaponTypeWeight =
            Require(typeof(BlueprintWeaponType), "m_Weight");

        private static readonly FieldInfo ItemWeaponType =
            Require(typeof(BlueprintItemWeapon), "m_Type");

        /// <summary>The item's own serialized <c>m_Weight</c>.</summary>
        internal static float Stored(BlueprintItem item)
        {
            if (item == null) throw new ArgumentNullException("item");
            return (float)ItemWeight.GetValue(item);
        }

        /// <summary>The weapon type's own serialized <c>m_Weight</c>.</summary>
        internal static float Stored(BlueprintWeaponType type)
        {
            if (type == null) throw new ArgumentNullException("type");
            return (float)WeaponTypeWeight.GetValue(type);
        }

        /// <summary>
        /// The weight the unpatched Kingmaker 2.1.7b getter would return, from
        /// serialized fields only: <c>BlueprintItemWeapon.Weight</c> is its own
        /// <c>m_Weight</c> when that is above zero and otherwise its type's
        /// <c>m_Weight</c>; every other item this mod registers uses the base
        /// <c>BlueprintItem.Weight</c>, which is <c>m_Weight</c>.
        /// </summary>
        internal static float Intended(BlueprintItem item)
        {
            float stored = Stored(item);
            var weapon = item as BlueprintItemWeapon;
            if (weapon == null || stored > 0f) return stored;
            var type = (BlueprintWeaponType)ItemWeaponType.GetValue(weapon);
            return type == null ? stored : Stored(type);
        }

        /// <summary>True when the item's stored weight is exactly <paramref name="expected"/>.</summary>
        internal static bool StoredEquals(BlueprintItem item, float expected)
        {
            return Stored(item).Equals(expected);
        }

        /// <summary>True when the weapon type's stored weight is exactly <paramref name="expected"/>.</summary>
        internal static bool StoredEquals(BlueprintWeaponType type, float expected)
        {
            return Stored(type).Equals(expected);
        }

        internal static string Text(float value)
        {
            return value.ToString("R", CultureInfo.InvariantCulture);
        }

        private static FieldInfo Require(Type type, string name)
        {
            FieldInfo field = type.GetField(name, Fields);
            if (field == null) throw new MissingFieldException(type.FullName, name);
            return field;
        }
    }
}
