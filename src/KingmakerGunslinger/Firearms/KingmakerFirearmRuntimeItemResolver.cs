using System;
using System.Globalization;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items.Weapons;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Development;

namespace KingmakerGunslinger.Firearms
{
    /// <summary>
    /// Kingmaker adapter that resolves only concrete ItemEntityWeapon instances whose
    /// exact BlueprintWeaponType contains one FirearmDefinitionComponent.
    /// </summary>
    internal sealed class KingmakerFirearmRuntimeItemResolver : IFirearmRuntimeItemResolver
    {
        private static readonly string[] ItemBlueprintMembers =
        {
            "Blueprint",
            "m_Blueprint",
            "BlueprintItem",
            "ItemBlueprint"
        };

        private static readonly string[] RuntimeIdMembers =
        {
            "UniqueId",
            "m_UniqueId",
            "Id",
            "m_Id",
            "EntityId"
        };

        private readonly WeaponBlueprintAccess _weaponTypeAccess;

        internal KingmakerFirearmRuntimeItemResolver()
        {
            _weaponTypeAccess = WeaponBlueprintAccess.Resolve();
        }

        public bool TryResolve(
            object candidate,
            out ResolvedFirearmItem firearm,
            out string rejectionReason)
        {
            firearm = null;
            if (candidate == null)
            {
                rejectionReason = "No runtime weapon item was supplied.";
                return false;
            }

            if (candidate is BlueprintScriptableObject)
            {
                rejectionReason = "Blueprint objects are definitions, not concrete firearm item instances.";
                return false;
            }

            if (!IsItemEntityWeapon(candidate.GetType()))
            {
                rejectionReason = string.Format(
                    CultureInfo.InvariantCulture,
                    "Runtime candidate type '{0}' is not an ItemEntityWeapon.",
                    candidate.GetType().FullName);
                return false;
            }

            object blueprintObject;
            string blueprintMember;
            if (!ReflectionAccess.TryGetFirstNonNullMember(
                candidate,
                ItemBlueprintMembers,
                out blueprintObject,
                out blueprintMember))
            {
                rejectionReason = "The runtime ItemEntityWeapon exposes no readable item blueprint.";
                return false;
            }

            BlueprintItemWeapon itemBlueprint = blueprintObject as BlueprintItemWeapon;
            if (itemBlueprint == null)
            {
                rejectionReason = string.Format(
                    CultureInfo.InvariantCulture,
                    "The runtime weapon's {0} member is not a BlueprintItemWeapon.",
                    blueprintMember);
                return false;
            }

            BlueprintWeaponType weaponType;
            try
            {
                weaponType = _weaponTypeAccess.Get(itemBlueprint);
            }
            catch (Exception exception)
            {
                rejectionReason = string.Format(
                    CultureInfo.InvariantCulture,
                    "The runtime weapon's BlueprintWeaponType could not be read: {0}: {1}",
                    exception.GetType().Name,
                    exception.Message);
                return false;
            }

            FirearmDefinitionComponent[] markers =
                (weaponType.ComponentsArray ?? Array.Empty<BlueprintComponent>())
                .OfType<FirearmDefinitionComponent>()
                .ToArray();
            if (markers.Length != 1)
            {
                rejectionReason = string.Format(
                    CultureInfo.InvariantCulture,
                    "Weapon type '{0}' contains {1} firearm markers; exactly one is required.",
                    weaponType.name,
                    markers.Length);
                return false;
            }

            FirearmDefinition definition;
            try
            {
                definition = markers[0].Definition;
            }
            catch (Exception exception)
            {
                rejectionReason = string.Format(
                    CultureInfo.InvariantCulture,
                    "The firearm marker could not produce a valid definition: {0}: {1}",
                    exception.GetType().Name,
                    exception.Message);
                return false;
            }

            firearm = new ResolvedFirearmItem(
                candidate,
                definition,
                DescribeObject(candidate),
                ReadFirstString(candidate, RuntimeIdMembers),
                itemBlueprint.name,
                ReadBlueprintId(itemBlueprint),
                weaponType.name,
                ReadBlueprintId(weaponType));
            rejectionReason = null;
            return true;
        }

        private static bool IsItemEntityWeapon(Type runtimeType)
        {
            for (Type current = runtimeType; current != null; current = current.BaseType)
            {
                if (string.Equals(
                    current.FullName,
                    "Kingmaker.Items.ItemEntityWeapon",
                    StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }

        private static string ReadBlueprintId(object blueprint)
        {
            return ReadFirstString(
                blueprint,
                new[] { "AssetGuid", "m_AssetGuid", "AssetId" });
        }

        private static string ReadFirstString(object source, string[] members)
        {
            object value;
            string ignored;
            if (!ReflectionAccess.TryGetFirstNonNullMember(
                source,
                members,
                out value,
                out ignored))
            {
                return "<unavailable>";
            }

            return ConvertToInvariantString(value);
        }

        private static string DescribeObject(object value)
        {
            object name;
            string ignored;
            if (ReflectionAccess.TryGetFirstNonNullMember(
                value,
                new[] { "Name", "name" },
                out name,
                out ignored))
            {
                return ConvertToInvariantString(name);
            }

            return value.GetType().FullName;
        }

        private static string ConvertToInvariantString(object value)
        {
            if (value == null)
            {
                return "<null>";
            }

            IFormattable formattable = value as IFormattable;
            return formattable == null
                ? value.ToString()
                : formattable.ToString(null, CultureInfo.InvariantCulture);
        }
    }
}
