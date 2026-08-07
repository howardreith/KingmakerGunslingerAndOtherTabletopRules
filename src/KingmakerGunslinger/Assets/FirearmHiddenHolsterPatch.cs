using System;
using System.Linq;
using System.Reflection;
using Harmony12;
using Kingmaker.View.Equipment;
using KingmakerGunslinger.Firearms;

namespace KingmakerGunslinger.Assets
{
    /// <summary>
    /// Removes only the sheath/quiver instance recreated for an exact production
    /// firearm whose explicit profile is Hidden. It never scans avatar renderers
    /// and never mutates native bow or crossbow visual parameters.
    /// </summary>
    [HarmonyPatch]
    internal static class FirearmHiddenHolsterPatch
    {
        private const BindingFlags InstanceFields = BindingFlags.Instance |
            BindingFlags.Public | BindingFlags.NonPublic;

        private static MethodBase TargetMethod()
        {
            return typeof(UnitViewHandSlotData).GetMethods(InstanceFields)
                .Single(method => method.Name == "ReattachSheath" &&
                    method.GetParameters().Length == 0);
        }

        private static void Postfix(UnitViewHandSlotData __instance)
        {
            if (__instance == null) return;
            object visibleItem = Read(__instance, "VisibleItem");
            ResolvedFirearmItem firearm;
            string reason;
            if (!new KingmakerFirearmRuntimeItemResolver().TryResolve(
                visibleItem, out firearm, out reason)) return;
            FirearmPresentationProfile profile =
                FirearmPresentationProfile.Require(firearm.Definition.Kind);
            if (profile.Holster != FirearmHolsterPolicy.Hidden ||
                !profile.IsLongGun) return;

            MethodInfo destroy = typeof(UnitViewHandSlotData).GetMethod(
                "DestroySheathModel", InstanceFields, null, Type.EmptyTypes, null);
            if (destroy == null)
                throw new MissingMethodException(typeof(UnitViewHandSlotData).FullName,
                    "DestroySheathModel");
            destroy.Invoke(__instance, null);
        }

        private static object Read(object instance, string name)
        {
            Type type = instance.GetType();
            PropertyInfo property = type.GetProperty(name, InstanceFields);
            if (property != null) return property.GetValue(instance, null);
            FieldInfo field = type.GetField(name, InstanceFields);
            return field == null ? null : field.GetValue(instance);
        }
    }
}
