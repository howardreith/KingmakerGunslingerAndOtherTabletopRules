using System;
using System.Reflection;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Visual.Sound;
using KingmakerGunslinger.Assets;
using KingmakerGunslinger.Firearms;
using UnityEngine;

namespace KingmakerGunslinger.Blueprints
{
    internal static class FirearmWeaponPresentation
    {
        private const BindingFlags Fields = BindingFlags.Instance |
            BindingFlags.Public | BindingFlags.NonPublic;

        internal static void Apply(BlueprintWeaponType weaponType,
            FirearmDefinition definition)
        {
            if (weaponType == null) throw new ArgumentNullException("weaponType");
            GameObject prefab = FirearmAssetRuntime.GetPrefab(definition.Kind);
            if (prefab == null) return;
            object source = Read(weaponType, "m_VisualParameters");
            if (source == null)
            {
                throw new InvalidOperationException(
                    "The cloned firearm weapon type has no visual parameters.");
            }

            var visual = new WeaponVisualParameters();
            foreach (FieldInfo field in typeof(WeaponVisualParameters).GetFields(Fields))
            {
                if (!field.IsStatic && !field.IsInitOnly)
                {
                    field.SetValue(visual, field.GetValue(source));
                }
            }

            Set(visual, "<Prototype>k__BackingField", null);
            Set(visual, "m_WeaponModel", prefab);
            Set(visual, "m_WeaponBeltModel", null);
            Set(visual, "m_WeaponSheathModel", null);
            Set(visual, "m_SoundType", WeaponSoundType.None);
            Set(visual, "m_MissSoundType", WeaponMissSoundType.None);
            Set(visual, "m_WhooshSound", string.Empty);
            Set(visual, "m_EquipSound", string.Empty);
            Set(visual, "m_UnequipSound", string.Empty);
            Set(visual, "m_InventoryEquipSound", string.Empty);
            Set(visual, "m_InventoryPutSound", string.Empty);
            Set(visual, "m_InventoryTakeSound", string.Empty);
            Set(weaponType, "m_VisualParameters", visual);
        }

        private static object Read(object instance, string name)
        {
            FieldInfo field = Find(instance.GetType(), name);
            return field.GetValue(instance);
        }

        private static void Set(object instance, string name, object value)
        {
            Find(instance.GetType(), name).SetValue(instance, value);
        }

        private static FieldInfo Find(Type type, string name)
        {
            for (Type current = type; current != null; current = current.BaseType)
            {
                FieldInfo field = current.GetField(name, Fields |
                    BindingFlags.DeclaredOnly);
                if (field != null) return field;
            }
            throw new MissingFieldException(type.FullName, name);
        }
    }
}
