using System;
using System.Linq;
using System.Reflection;
using Harmony12;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Items;
using Kingmaker.UnitLogic;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.EasternWeapons;

namespace KingmakerGunslinger.Compatibility
{
    /// <summary>
    /// Arms and Armor replaces Kingmaker's versatile-weapon grip calculation
    /// with an exact category allowlist. Extend that mod's own classification
    /// seam for the exact KMG Katana type so its grip authority remains the one
    /// used by proficiency and Moonlit Crossing when both mods are active.
    /// </summary>
    internal static class EasternWeaponArmsArmorCompatibility
    {
        private const string AssemblyName = "ArmsArmor";
        private const string HelperTypeName = "ArmsArmor.Helpers";
        private const string MethodName = "IsExoticTwoHandedMartialWeapon";
        private const string GripTypeName = "ArmsArmor.ItemEntityWeaponPatch";
        private const string GripMethodName = "IsTwoHanded";
        private static bool _installed;
        private static string _status = "not-evaluated";

        internal static bool Installed { get { return _installed; } }
        internal static string Status { get { return _status; } }

        internal static void Install(HarmonyInstance harmony)
        {
            if (harmony == null) throw new ArgumentNullException("harmony");
            Assembly[] matches = AppDomain.CurrentDomain.GetAssemblies()
                .Where(value => string.Equals(value.GetName().Name,
                    AssemblyName, StringComparison.Ordinal)).ToArray();
            if (matches.Length == 0)
            {
                _status = "absent";
                return;
            }
            if (matches.Length != 1)
                throw new InvalidOperationException(
                    "Arms and Armor assembly identity is ambiguous.");
            Type helpers = matches[0].GetType(HelperTypeName, false, false);
            MethodInfo classification = helpers == null ? null : helpers.GetMethod(
                MethodName, BindingFlags.Static | BindingFlags.Public |
                    BindingFlags.NonPublic, null,
                new[] { typeof(BlueprintItemWeapon) }, null);
            Type gripType = matches[0].GetType(GripTypeName, false, false);
            MethodInfo grip = gripType == null ? null : gripType.GetMethod(
                GripMethodName, BindingFlags.Static | BindingFlags.Public |
                    BindingFlags.NonPublic, null,
                new[] { typeof(ItemEntityWeapon), typeof(UnitDescriptor) },
                null);
            if (classification == null ||
                classification.ReturnType != typeof(bool) ||
                classification.GetParameters().Length != 1 || grip == null ||
                grip.ReturnType != typeof(bool) ||
                grip.GetParameters().Length != 2)
                throw new InvalidOperationException(
                    "Arms and Armor versatile-weapon grip contract changed.");
            MethodInfo classificationPostfix = typeof(
                EasternWeaponArmsArmorCompatibility).GetMethod(
                    "ClassificationPostfix", BindingFlags.Static |
                    BindingFlags.NonPublic);
            MethodInfo gripPostfix = typeof(
                EasternWeaponArmsArmorCompatibility).GetMethod(
                    "GripPostfix", BindingFlags.Static |
                    BindingFlags.NonPublic);
            if (classificationPostfix == null || gripPostfix == null)
                throw new MissingMethodException(typeof(
                    EasternWeaponArmsArmorCompatibility).FullName,
                    "compatibility postfix");
            harmony.Patch(classification, null,
                new HarmonyMethod(classificationPostfix), null);
            harmony.Patch(grip, null, new HarmonyMethod(gripPostfix), null);
            _installed = true;
            _status = "installed:" + matches[0].GetName().Version;
        }

        private static void ClassificationPostfix(BlueprintItemWeapon weapon,
            ref bool __result)
        {
            if (!__result && IsExactKatana(weapon)) __result = true;
        }

        private static void GripPostfix(ItemEntityWeapon weapon,
            UnitDescriptor owner, ref bool __result)
        {
            if (owner == null || weapon == null ||
                !IsExactKatana(weapon.Blueprint) || owner.Body == null ||
                owner.Body.PrimaryHand == null ||
                !ReferenceEquals(owner.Body.PrimaryHand.MaybeWeapon, weapon))
                return;
            __result = owner.Body.SecondaryHand == null ||
                owner.Body.SecondaryHand.MaybeItem == null;
        }

        private static bool IsExactKatana(BlueprintItemWeapon weapon)
        {
            if (weapon == null || weapon.Type == null) return false;
            EasternWeaponBlueprintSet set = BlueprintBootstrap.EasternWeapons;
            EasternWeaponFamilyBlueprintSet katana = set == null ? null :
                set.Families.SingleOrDefault(value =>
                    value.Family == EasternWeaponFamily.Katana);
            return katana != null && ReferenceEquals(weapon.Type,
                katana.WeaponType);
        }
    }
}
