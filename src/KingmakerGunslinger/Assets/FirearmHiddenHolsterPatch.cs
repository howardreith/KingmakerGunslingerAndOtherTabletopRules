using System;
using System.Linq;
using System.Reflection;
using Harmony12;
using Kingmaker.View.Equipment;
using KingmakerGunslinger.Firearms;

namespace KingmakerGunslinger.Assets
{
    /// <summary>
    /// Keeps the live held model invisible while an exact production firearm
    /// whose explicit profile is Hidden is attached to a stored slot. Kingmaker
    /// reuses that model when BeltModel and SheathModel are null, so clearing
    /// those blueprint fields alone does not hide it. The native ShowItem path
    /// remains responsible for renderer state and makes the model visible again
    /// after IsInHand changes (and in the inventory doll room).
    ///
    /// This never scans avatar renderers and never mutates native bow or
    /// crossbow visual parameters.
    /// </summary>
    [HarmonyPatch]
    internal static class FirearmHiddenHolsterPatch
    {
        private const BindingFlags InstanceMembers = BindingFlags.Instance |
            BindingFlags.Public | BindingFlags.NonPublic;

        private static MethodBase TargetMethod()
        {
            return typeof(UnitViewHandSlotData).GetMethods(InstanceMembers)
                .Single(method => method.Name == "ShowItem" &&
                    method.GetParameters().Length == 1 &&
                    method.GetParameters()[0].ParameterType == typeof(bool));
        }

        private static void Prefix(UnitViewHandSlotData __instance,
            ref bool isVisible)
        {
            if (__instance == null) return;
            object visibleItem = Read(__instance, "VisibleItem");
            ResolvedFirearmItem firearm;
            string reason;
            if (!new KingmakerFirearmRuntimeItemResolver().TryResolve(
                visibleItem, out firearm, out reason)) return;
            FirearmPresentationProfile profile =
                FirearmPresentationProfile.Require(firearm.Definition.Kind);
            if (profile.Holster == FirearmHolsterPolicy.Hidden &&
                !__instance.IsInHand)
                isVisible = false;
        }

        private static object Read(object instance, string name)
        {
            Type type = instance.GetType();
            PropertyInfo property = type.GetProperty(name, InstanceMembers);
            if (property != null) return property.GetValue(instance, null);
            FieldInfo field = type.GetField(name, InstanceMembers);
            return field == null ? null : field.GetValue(instance);
        }
    }
}
