using System;
using System.Reflection;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items.Weapons;
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
            FirearmDefinition definition, BlueprintProjectile projectile)
        {
            if (weaponType == null) throw new ArgumentNullException("weaponType");
            ApplyCore(weaponType, definition, projectile);
        }

        internal static void Apply(BlueprintItemWeapon weapon,
            FirearmDefinition definition, BlueprintProjectile projectile)
        {
            if (weapon == null) throw new ArgumentNullException("weapon");
            ApplyCore(weapon, definition, projectile);
        }

        internal static bool ApplyItemVariant(BlueprintItemWeapon weapon,
            string blueprintSymbol, FirearmKind kind)
        {
            if (weapon == null) throw new ArgumentNullException("weapon");
            string variant = WeaponVisualVariantCatalog.Require(blueprintSymbol);
            if (!variant.StartsWith(kind.ToString() + ".",
                StringComparison.Ordinal))
                throw new InvalidOperationException(blueprintSymbol +
                    " maps across its qualified firearm family boundary.");
            GameObject prefab = FirearmAssetRuntime.GetItemVariantPrefab(variant);
            if (prefab == null) return false;
            WeaponVisualParameters source = Read(weapon, "m_VisualParameters")
                as WeaponVisualParameters;
            if (source == null || source.Model == null)
                throw new InvalidOperationException(
                    "The firearm item has no visible family fallback.");
            WeaponVisualParameters visual = Clone(source);
            Set(visual, "m_WeaponModel", prefab);
            Set(weapon, "m_VisualParameters", visual);
            if (weapon.VisualParameters == null ||
                !ReferenceEquals(weapon.VisualParameters.Model, prefab))
                throw new InvalidOperationException(
                    "The exact firearm item variant did not round-trip.");
            return true;
        }

        internal static bool HasExactItemVariant(BlueprintItemWeapon weapon,
            string blueprintSymbol, FirearmKind kind)
        {
            if (weapon == null) return false;
            string variant = WeaponVisualVariantCatalog.Require(blueprintSymbol);
            if (!variant.StartsWith(kind.ToString() + ".",
                StringComparison.Ordinal)) return false;
            GameObject prefab = FirearmAssetRuntime.GetItemVariantPrefab(variant);
            return prefab != null && weapon.VisualParameters != null &&
                ReferenceEquals(weapon.VisualParameters.Model, prefab);
        }

        internal static bool HasApprovedItemVariantOrFamilyFallback(
            BlueprintItemWeapon weapon, string blueprintSymbol, FirearmKind kind)
        {
            if (weapon == null) return false;
            string variant = WeaponVisualVariantCatalog.Require(blueprintSymbol);
            if (!variant.StartsWith(kind.ToString() + ".",
                StringComparison.Ordinal)) return false;
            GameObject prefab = FirearmAssetRuntime.GetItemVariantPrefab(variant);
            if (prefab != null)
                return weapon.VisualParameters != null &&
                    ReferenceEquals(weapon.VisualParameters.Model, prefab);
            return weapon.Type != null && weapon.VisualParameters != null &&
                weapon.Type.VisualParameters != null && ReferenceEquals(
                    weapon.VisualParameters.Model,
                    weapon.Type.VisualParameters.Model);
        }

        private static void ApplyCore(object owner, FirearmDefinition definition,
            BlueprintProjectile projectile)
        {
            if (definition == null) throw new ArgumentNullException("definition");
            if (projectile == null) throw new ArgumentNullException("projectile");
            FirearmPresentationProfile profile =
                FirearmPresentationProfile.Require(definition.Kind);
            WeaponVisualParameters source = Read(owner, "m_VisualParameters")
                as WeaponVisualParameters;
            if (source == null)
                throw new InvalidOperationException(
                    "The cloned firearm has no native visual parameters.");

            // Preserve the complete cloned native presentation contract,
            // including Prototype, attachment slots, belt/sheath behavior,
            // animation, and sound.  Earlier repairs detached Prototype and
            // replaced inherited models with null, which made long guns vanish.
            // Only the firearm projectile and individually approved custom art
            // are allowed to override that visible native fallback.
            WeaponVisualParameters visual = Clone(source);

            // Resolve every prototype-backed presentation value that the runtime
            // scenarios protect before removing the crossbow Prototype. This
            // preserves visible/equip/inventory behavior while preventing empty
            // local combat-audio values from falling back to the cloned crossbow.
            Materialize(visual, "m_WeaponModel", source.Model);
            Materialize(visual, "m_WeaponBeltModel", source.BeltModel);
            Materialize(visual, "m_WeaponSheathModel", source.SheathModel);
            Materialize(visual, "m_WeaponAnimationStyle", source.AnimStyle);
            Materialize(visual, "m_PossibleAttachSlots", source.AttachSlots);
            Materialize(visual, "m_SoundSize", source.SoundSize);
            Materialize(visual, "m_SoundType", source.SoundType);
            Materialize(visual, "m_MissSoundType", source.MissSoundType);
            Materialize(visual, "m_EquipSound", source.EquipSound);
            Materialize(visual, "m_UnequipSound", source.UnequipSound);
            Materialize(visual, "m_InventoryEquipSound", source.InventoryEquipSound);
            Materialize(visual, "m_InventoryPutSound", source.InventoryPutSound);
            Materialize(visual, "m_InventoryTakeSound", source.InventoryTakeSound);
            Set(visual, "m_WhooshSound", string.Empty);
            Set(visual, "<Prototype>k__BackingField", null);

            if (source.Model == null)
                throw new InvalidOperationException(
                    "The cloned native firearm presentation has no visible equipped-model fallback.");

            Set(visual, "m_Projectiles", new[] { projectile });
            GameObject equipped = profile.EquippedModel;
            if (equipped != null) Set(visual, "m_WeaponModel", equipped);
            GameObject belt = profile.BeltModel;
            if (belt != null) Set(visual, "m_WeaponBeltModel", belt);
            GameObject sheath = profile.SheathModel;
            if (sheath != null) Set(visual, "m_WeaponSheathModel", sheath);
            if (profile.HideHolsteredModel)
            {
                // Exact project-owned firearm visual only. Native crossbow
                // blueprints and unrelated renderers are never mutated.
                Set(visual, "m_WeaponBeltModel", null);
                Set(visual, "m_WeaponSheathModel", null);
            }
            if (profile.Animation.HasValue)
                Set(visual, "m_WeaponAnimationStyle", profile.Animation.Value);

            Set(owner, "m_VisualParameters", visual);
        }

        private static WeaponVisualParameters Clone(WeaponVisualParameters source)
        {
            var visual = new WeaponVisualParameters();
            foreach (FieldInfo field in typeof(WeaponVisualParameters).GetFields(
                Fields))
                if (!field.IsStatic && !field.IsInitOnly)
                    field.SetValue(visual, field.GetValue(source));
            return visual;
        }

        private static void Materialize(object instance, string name, object value)
        {
            Set(instance, name, value);
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
