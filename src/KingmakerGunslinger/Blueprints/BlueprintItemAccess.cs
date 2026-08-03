using System;
using System.Reflection;
using Kingmaker.Blueprints.Items;
using Kingmaker.Localization;
using UnityEngine;

namespace KingmakerGunslinger.Blueprints
{
    /// <summary>
    /// Exact private-field adapter for immutable item-blueprint construction. The fields are
    /// resolved once against the installed Kingmaker assembly and every assignment is verified
    /// through the blueprint's public read surface.
    /// </summary>
    internal sealed class BlueprintItemAccess
    {
        private const BindingFlags Fields =
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        private readonly FieldInfo _displayName;
        private readonly FieldInfo _description;
        private readonly FieldInfo _flavor;
        private readonly FieldInfo _nonIdentifiedName;
        private readonly FieldInfo _nonIdentifiedDescription;
        private readonly FieldInfo _isStackable;
        private readonly FieldInfo _cost;
        private readonly FieldInfo _weight;
        private readonly FieldInfo _isNotable;
        private readonly FieldInfo _miscellaneousType;
        private readonly FieldInfo _enchantmentsCollected;
        private readonly FieldInfo _cachedEnchantments;
        private readonly FieldInfo _icon;

        private BlueprintItemAccess()
        {
            Type type = typeof(BlueprintItem);
            _displayName = Require(type, "m_DisplayNameText", typeof(LocalizedString));
            _description = Require(type, "m_DescriptionText", typeof(LocalizedString));
            _flavor = Require(type, "m_FlavorText", typeof(LocalizedString));
            _nonIdentifiedName = Require(type, "m_NonIdentifiedNameText", typeof(LocalizedString));
            _nonIdentifiedDescription = Require(
                type,
                "m_NonIdentifiedDescriptionText",
                typeof(LocalizedString));
            _isStackable = Require(type, "m_IsStackable", typeof(bool));
            _cost = Require(type, "m_Cost", typeof(int));
            _weight = Require(type, "m_Weight", typeof(float));
            _isNotable = Require(type, "m_IsNotable", typeof(bool));
            _miscellaneousType = Require(type, "m_MiscellaneousType", null);
            _enchantmentsCollected = Optional(type, "m_EnchantmentsCollected", typeof(bool));
            _cachedEnchantments = Optional(type, "m_CachedEnchantments", null);
            _icon = Require(type, "m_Icon", typeof(Sprite));

            if (!_miscellaneousType.FieldType.IsEnum)
            {
                throw new InvalidOperationException(
                    "BlueprintItem.m_MiscellaneousType is not an enum in the installed Kingmaker build.");
            }
        }

        internal static BlueprintItemAccess Resolve()
        {
            return new BlueprintItemAccess();
        }

        internal void SetIcon(BlueprintItem item, Sprite icon)
        {
            if (item == null) throw new ArgumentNullException("item");
            if (icon == null) throw new ArgumentNullException("icon");
            _icon.SetValue(item, icon);
            if (!ReferenceEquals(item.Icon, icon))
                throw new InvalidOperationException("The item icon assignment did not verify.");
        }

        internal void Configure(
            BlueprintItem item,
            LocalizedString name,
            LocalizedString description,
            LocalizedString flavor,
            int cost,
            float weight)
        {
            ConfigureCore(item, name, description, flavor, cost, weight, true);
        }

        internal void ConfigureWeapon(
            BlueprintItem item,
            LocalizedString name,
            LocalizedString description,
            LocalizedString flavor,
            int cost,
            float weight)
        {
            ConfigureCore(item, name, description, flavor, cost, weight, false);
        }

        private void ConfigureCore(
            BlueprintItem item,
            LocalizedString name,
            LocalizedString description,
            LocalizedString flavor,
            int cost,
            float weight,
            bool isStackable)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }

            if (name == null)
            {
                throw new ArgumentNullException("name");
            }

            if (description == null)
            {
                throw new ArgumentNullException("description");
            }

            if (flavor == null)
            {
                throw new ArgumentNullException("flavor");
            }

            if (cost < 0)
            {
                throw new ArgumentOutOfRangeException("cost", cost, "Item cost cannot be negative.");
            }

            if (float.IsNaN(weight) || float.IsInfinity(weight) || weight < 0f)
            {
                throw new ArgumentOutOfRangeException("weight", weight, "Item weight must be finite and nonnegative.");
            }

            _displayName.SetValue(item, name);
            _description.SetValue(item, description);
            _flavor.SetValue(item, flavor);
            _nonIdentifiedName.SetValue(item, name);
            _nonIdentifiedDescription.SetValue(item, description);
            _isStackable.SetValue(item, isStackable);
            _cost.SetValue(item, cost);
            _weight.SetValue(item, weight);
            _isNotable.SetValue(item, false);
            _miscellaneousType.SetValue(item, Enum.ToObject(_miscellaneousType.FieldType, 0));
            if (_enchantmentsCollected != null)
            {
                _enchantmentsCollected.SetValue(item, false);
            }

            if (_cachedEnchantments != null)
            {
                _cachedEnchantments.SetValue(item, null);
            }
        }

        internal BlueprintItemSnapshot Capture(BlueprintItem item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }

            return new BlueprintItemSnapshot(
                _displayName.GetValue(item),
                _description.GetValue(item),
                _flavor.GetValue(item),
                (bool)_isStackable.GetValue(item),
                (int)_cost.GetValue(item),
                (float)_weight.GetValue(item),
                _miscellaneousType.GetValue(item),
                item.ComponentsArray);
        }

        private static FieldInfo Require(Type type, string name, Type expectedType)
        {
            FieldInfo field = type.GetField(name, Fields);
            if (field == null || (expectedType != null && field.FieldType != expectedType))
            {
                throw new MissingFieldException(type.FullName, name);
            }

            return field;
        }

        private static FieldInfo Optional(Type type, string name, Type expectedType)
        {
            FieldInfo field = type.GetField(name, Fields);
            if (field != null && expectedType != null && field.FieldType != expectedType)
            {
                throw new MissingFieldException(type.FullName, name);
            }

            return field;
        }
    }

    internal sealed class BlueprintItemSnapshot
    {
        internal BlueprintItemSnapshot(
            object displayName,
            object description,
            object flavor,
            bool isStackable,
            int cost,
            float weight,
            object miscellaneousType,
            object componentsArray)
        {
            DisplayName = displayName;
            Description = description;
            Flavor = flavor;
            IsStackable = isStackable;
            Cost = cost;
            Weight = weight;
            MiscellaneousType = miscellaneousType;
            ComponentsArray = componentsArray;
        }

        internal object DisplayName { get; private set; }

        internal object Description { get; private set; }

        internal object Flavor { get; private set; }

        internal bool IsStackable { get; private set; }

        internal int Cost { get; private set; }

        internal float Weight { get; private set; }

        internal object MiscellaneousType { get; private set; }

        internal object ComponentsArray { get; private set; }

        internal bool Matches(BlueprintItemSnapshot other)
        {
            return other != null &&
                ReferenceEquals(DisplayName, other.DisplayName) &&
                ReferenceEquals(Description, other.Description) &&
                ReferenceEquals(Flavor, other.Flavor) &&
                IsStackable == other.IsStackable &&
                Cost == other.Cost &&
                Weight.Equals(other.Weight) &&
                Equals(MiscellaneousType, other.MiscellaneousType) &&
                ReferenceEquals(ComponentsArray, other.ComponentsArray);
        }
    }
}
