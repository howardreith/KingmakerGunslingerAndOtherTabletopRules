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
            var visual = new WeaponVisualParameters();
            foreach (FieldInfo field in typeof(WeaponVisualParameters).GetFields(Fields))
            {
                if (!field.IsStatic && !field.IsInitOnly)
                    field.SetValue(visual, field.GetValue(source));
            }

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
            if (profile.Animation.HasValue)
                Set(visual, "m_WeaponAnimationStyle", profile.Animation.Value);

            Set(owner, "m_VisualParameters", visual);
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
