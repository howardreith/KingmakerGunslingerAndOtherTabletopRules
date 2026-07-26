using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using Kingmaker.Blueprints.Items.Weapons;

namespace KingmakerGunslinger.Blueprints
{
    /// <summary>
    /// Resolves Kingmaker's BlueprintItemWeapon-to-BlueprintWeaponType member once,
    /// with explicit validation. The adapter avoids silently guessing a private field
    /// name and fails before registering any custom weapon when the runtime contract
    /// differs from the expected Kingmaker API.
    /// </summary>
    internal sealed class WeaponBlueprintAccess
    {
        private static readonly string[] PreferredNames =
        {
            "Type",
            "m_Type",
            "WeaponType",
            "m_WeaponType"
        };

        private readonly MemberInfo _member;

        private WeaponBlueprintAccess(MemberInfo member)
        {
            _member = member ?? throw new ArgumentNullException("member");
        }

        internal string MemberDescription
        {
            get
            {
                return string.Format(
                    CultureInfo.InvariantCulture,
                    "{0}.{1} ({2})",
                    _member.DeclaringType == null ? "<unknown>" : _member.DeclaringType.FullName,
                    _member.Name,
                    _member.MemberType);
            }
        }

        internal static WeaponBlueprintAccess Resolve()
        {
            Type itemType = typeof(BlueprintItemWeapon);
            List<MemberInfo> compatibleMembers = FindCompatibleMembers(itemType);

            foreach (string preferredName in PreferredNames)
            {
                foreach (MemberInfo member in compatibleMembers)
                {
                    if (string.Equals(member.Name, preferredName, StringComparison.Ordinal))
                    {
                        return new WeaponBlueprintAccess(member);
                    }
                }
            }

            if (compatibleMembers.Count == 1)
            {
                return new WeaponBlueprintAccess(compatibleMembers[0]);
            }

            string candidates = compatibleMembers.Count == 0
                ? "<none>"
                : string.Join(", ", compatibleMembers.ConvertAll(DescribeMember).ToArray());
            throw new MissingMemberException(
                string.Format(
                    CultureInfo.InvariantCulture,
                    "Could not resolve one readable/writable BlueprintWeaponType member on {0}; candidates={1}.",
                    itemType.FullName,
                    candidates));
        }

        internal BlueprintWeaponType Get(BlueprintItemWeapon item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }

            object value;
            FieldInfo field = _member as FieldInfo;
            if (field != null)
            {
                value = field.GetValue(item);
            }
            else
            {
                PropertyInfo property = (PropertyInfo)_member;
                MethodInfo getter = property.GetGetMethod(true);
                if (getter == null)
                {
                    throw new MissingMethodException(property.DeclaringType.FullName, "get_" + property.Name);
                }

                value = getter.Invoke(item, null);
            }

            BlueprintWeaponType weaponType = value as BlueprintWeaponType;
            if (weaponType == null)
            {
                throw new InvalidOperationException(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "Weapon item '{0}' has no BlueprintWeaponType through member {1}.",
                        item.name,
                        MemberDescription));
            }

            return weaponType;
        }

        internal void Set(BlueprintItemWeapon item, BlueprintWeaponType weaponType)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }

            if (weaponType == null)
            {
                throw new ArgumentNullException("weaponType");
            }

            FieldInfo field = _member as FieldInfo;
            if (field != null)
            {
                field.SetValue(item, weaponType);
            }
            else
            {
                PropertyInfo property = (PropertyInfo)_member;
                MethodInfo setter = property.GetSetMethod(true);
                if (setter == null)
                {
                    throw new MissingMethodException(property.DeclaringType.FullName, "set_" + property.Name);
                }

                setter.Invoke(item, new object[] { weaponType });
            }

            if (!ReferenceEquals(Get(item), weaponType))
            {
                throw new InvalidOperationException(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "Weapon type assignment did not persist for item '{0}' through member {1}.",
                        item.name,
                        MemberDescription));
            }
        }

        private static List<MemberInfo> FindCompatibleMembers(Type itemType)
        {
            var result = new List<MemberInfo>();
            for (Type current = itemType; current != null; current = current.BaseType)
            {
                FieldInfo[] fields = current.GetFields(
                    BindingFlags.Instance |
                    BindingFlags.Public |
                    BindingFlags.NonPublic |
                    BindingFlags.DeclaredOnly);
                foreach (FieldInfo field in fields)
                {
                    if (field.FieldType == typeof(BlueprintWeaponType) &&
                        !field.IsInitOnly &&
                        !field.IsLiteral)
                    {
                        result.Add(field);
                    }
                }

                PropertyInfo[] properties = current.GetProperties(
                    BindingFlags.Instance |
                    BindingFlags.Public |
                    BindingFlags.NonPublic |
                    BindingFlags.DeclaredOnly);
                foreach (PropertyInfo property in properties)
                {
                    if (property.PropertyType == typeof(BlueprintWeaponType) &&
                        property.GetIndexParameters().Length == 0 &&
                        property.GetGetMethod(true) != null &&
                        property.GetSetMethod(true) != null)
                    {
                        result.Add(property);
                    }
                }
            }

            return result;
        }

        private static string DescribeMember(MemberInfo member)
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "{0}.{1}/{2}",
                member.DeclaringType == null ? "<unknown>" : member.DeclaringType.FullName,
                member.Name,
                member.MemberType);
        }
    }
}
