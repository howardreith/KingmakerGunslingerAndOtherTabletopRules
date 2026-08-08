using System;
using System.Reflection;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.Localization;
using KingmakerGunslinger.Blueprints;
using UnityEngine;

namespace KingmakerGunslinger.Enchantments
{
    internal static class SeekingBlueprints
    {
        internal const string Symbol = "KMG.Enchantments.Seeking";
        internal const string DisplayName = "Seeking";
        internal const string Description = "Attacks made with this ranged weapon ignore miss chances caused by concealment. Seeking does not reveal unseen creatures or bypass other defenses.";

        private const BindingFlags Fields = BindingFlags.Instance |
            BindingFlags.NonPublic;

        internal static BlueprintWeaponEnchantment Register(BlueprintRegistry registry)
        {
            if (registry == null) throw new ArgumentNullException("registry");
            return registry.Register<BlueprintWeaponEnchantment>(Symbol,
                delegate
                {
                    BlueprintWeaponEnchantment value =
                        ScriptableObject.CreateInstance<BlueprintWeaponEnchantment>();
                    value.name = "KMG_Seeking_WeaponEnchantment";
                    Set(value, "m_EnchantName", LocalizationService.Create(
                        "KMG.Enchantments.Seeking.Name", DisplayName));
                    Set(value, "m_Description", LocalizationService.Create(
                        "KMG.Enchantments.Seeking.Description", Description));
                    Set(value, "m_EnchantmentCost", 1);
                    SeekingWeaponEnchantmentComponent marker =
                        SeekingWeaponEnchantmentComponent.Create();
                    marker.name = "$KMG_Seeking_ExactItem";
                    value.ComponentsArray = new BlueprintComponent[] { marker };
                    return value;
                });
        }

        internal static void Validate(BlueprintWeaponEnchantment value)
        {
            if (value == null || value.EnchantmentCost != 1 ||
                !string.Equals(value.Name, DisplayName, StringComparison.Ordinal) ||
                !string.Equals(value.Description, Description, StringComparison.Ordinal) ||
                value.ComponentsArray == null || value.ComponentsArray.Length != 1 ||
                !(value.ComponentsArray[0] is SeekingWeaponEnchantmentComponent))
            {
                throw new InvalidOperationException(
                    "The project Seeking enchantment contract is malformed.");
            }
        }

        private static void Set(object target, string name, object value)
        {
            FieldInfo field = typeof(BlueprintItemEnchantment).GetField(name, Fields);
            if (field == null || (value != null && !field.FieldType.IsInstanceOfType(value)))
            {
                throw new MissingFieldException(typeof(BlueprintItemEnchantment).FullName, name);
            }
            field.SetValue(target, value);
        }
    }
}
