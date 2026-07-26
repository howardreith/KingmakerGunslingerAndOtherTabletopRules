using System;
using System.Globalization;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items.Weapons;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Development;
using KingmakerGunslinger.Firearms;

namespace KingmakerGunslinger.Diagnostics
{
    /// <summary>
    /// Resolves firearm identity from a concrete weapon item and its exact weapon-type marker.
    /// Reused Heavy Crossbow categories or names never establish firearm identity.
    /// </summary>
    internal static class FirearmMarkerLookup
    {
        private static readonly object AccessGate = new object();
        private static WeaponBlueprintAccess _weaponTypeAccess;

        internal static FirearmMarkerSnapshot ReadFromRuleEvent(object ruleEvent)
        {
            object weapon;
            if (!TryResolveWeapon(ruleEvent, out weapon))
            {
                return FirearmMarkerSnapshot.NoWeapon();
            }

            return ReadFromWeapon(weapon);
        }

        internal static bool TryResolveWeapon(object ruleEvent, out object weapon)
        {
            weapon = null;
            if (ruleEvent == null)
            {
                return false;
            }

            object candidate;
            string ignored;
            if (ReflectionAccess.TryGetFirstNonNullMember(
                ruleEvent,
                new[] { "Weapon", "m_Weapon" },
                out candidate,
                out ignored))
            {
                weapon = candidate;
                return true;
            }

            string[] paths =
            {
                "RuleAttackWithWeapon.Weapon",
                "AttackWithWeapon.Weapon",
                "Attack.Weapon",
                "Reason.Rule.Weapon",
                "Reason.Event.Weapon"
            };
            foreach (string path in paths)
            {
                if (ReflectionAccess.TryGetPath(ruleEvent, path, out candidate) && candidate != null)
                {
                    weapon = candidate;
                    return true;
                }
            }

            return false;
        }

        private static FirearmMarkerSnapshot ReadFromWeapon(object weapon)
        {
            object blueprintObject;
            string ignored;
            if (!ReflectionAccess.TryGetFirstNonNullMember(
                weapon,
                new[] { "Blueprint", "m_Blueprint" },
                out blueprintObject,
                out ignored))
            {
                return new FirearmMarkerSnapshot(
                    true,
                    -1,
                    null,
                    DescribeObject(weapon),
                    ReadFirstString(weapon, "UniqueId", "Id", "EntityId"),
                    "<unavailable>",
                    "<unavailable>",
                    "<unavailable>",
                    "<unavailable>");
            }

            BlueprintItemWeapon itemBlueprint = blueprintObject as BlueprintItemWeapon;
            if (itemBlueprint == null)
            {
                return new FirearmMarkerSnapshot(
                    true,
                    -1,
                    null,
                    DescribeObject(weapon),
                    ReadFirstString(weapon, "UniqueId", "Id", "EntityId"),
                    DescribeObject(blueprintObject),
                    ReadBlueprintId(blueprintObject),
                    "<unavailable>",
                    "<unavailable>");
            }

            BlueprintWeaponType weaponType = GetWeaponTypeAccess().Get(itemBlueprint);
            FirearmDefinitionComponent[] markers =
                (weaponType.ComponentsArray ?? Array.Empty<BlueprintComponent>())
                .OfType<FirearmDefinitionComponent>()
                .ToArray();
            FirearmDefinition definition = markers.Length == 1
                ? markers[0].Definition
                : null;

            return new FirearmMarkerSnapshot(
                true,
                markers.Length,
                definition,
                DescribeObject(weapon),
                ReadFirstString(weapon, "UniqueId", "Id", "EntityId"),
                itemBlueprint.name,
                ReadBlueprintId(itemBlueprint),
                weaponType.name,
                ReadBlueprintId(weaponType));
        }

        private static WeaponBlueprintAccess GetWeaponTypeAccess()
        {
            lock (AccessGate)
            {
                if (_weaponTypeAccess == null)
                {
                    _weaponTypeAccess = WeaponBlueprintAccess.Resolve();
                }

                return _weaponTypeAccess;
            }
        }

        private static string ReadBlueprintId(object blueprint)
        {
            return ReadFirstString(blueprint, "AssetGuid", "m_AssetGuid", "AssetId");
        }

        private static string ReadFirstString(object source, params string[] members)
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
            if (value == null)
            {
                return "<unavailable>";
            }

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
