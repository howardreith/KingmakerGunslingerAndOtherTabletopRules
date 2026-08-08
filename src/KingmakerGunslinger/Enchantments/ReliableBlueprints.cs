using System;
using System.Reflection;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items.Ecnchantments;
using KingmakerGunslinger.Blueprints;
using UnityEngine;

namespace KingmakerGunslinger.Enchantments
{
    internal static class ReliableBlueprints
    {
        internal const string Symbol = "KMG.Firearms.ReliableEnchantment";
        internal const string DisplayName = "Reliable";
        internal const string Description = "This firearm reduces its misfire value by 1 after all other increases, to a minimum of 0. A misfire value of 0 prevents misfires, but a natural 1 is still a miss.";
        private const BindingFlags Fields = BindingFlags.Instance | BindingFlags.NonPublic;

        internal static BlueprintWeaponEnchantment Register(BlueprintRegistry registry)
        {
            if (registry == null) throw new ArgumentNullException("registry");
            return registry.Register<BlueprintWeaponEnchantment>(Symbol, delegate
            {
                BlueprintWeaponEnchantment value =
                    ScriptableObject.CreateInstance<BlueprintWeaponEnchantment>();
                value.name = "KMG_Reliable_WeaponEnchantment";
                Set(value, "m_EnchantName", LocalizationService.Create(
                    "KMG.Enchantments.Reliable.Name", DisplayName));
                Set(value, "m_Description", LocalizationService.Create(
                    "KMG.Enchantments.Reliable.Description", Description));
                Set(value, "m_EnchantmentCost", 1);
                FirearmMisfireReductionComponent marker =
                    FirearmMisfireReductionComponent.Create(1);
                marker.name = "$KMG_Reliable_ExactItem";
                value.ComponentsArray = new BlueprintComponent[] { marker };
                return value;
            });
        }

        internal static void Validate(BlueprintWeaponEnchantment value)
        {
            FirearmMisfireReductionComponent marker = value == null ? null :
                value.ComponentsArray == null ? null :
                value.ComponentsArray.Length == 1 ?
                    value.ComponentsArray[0] as FirearmMisfireReductionComponent : null;
            if (value == null || value.EnchantmentCost != 1 ||
                !string.Equals(value.Name, DisplayName, StringComparison.Ordinal) ||
                !string.Equals(value.Description, Description, StringComparison.Ordinal) ||
                marker == null || marker.Reduction != 1)
                throw new InvalidOperationException("The Reliable enchantment contract is malformed.");
        }

        private static void Set(object target, string name, object value)
        {
            FieldInfo field = typeof(BlueprintItemEnchantment).GetField(name, Fields);
            if (field == null || (value != null && !field.FieldType.IsInstanceOfType(value)))
                throw new MissingFieldException(typeof(BlueprintItemEnchantment).FullName, name);
            field.SetValue(target, value);
        }
    }
}
