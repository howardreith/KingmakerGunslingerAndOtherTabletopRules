using System;
using Harmony12;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic;
using KingmakerGunslinger.FavoredClass.Mechanics;

namespace KingmakerGunslinger.FavoredClass.Hooks
{
    /// <summary>
    /// O07/O08: the native SetMaster(null) (also reached by RemoveMaster when
    /// a companion is dismissed) unlinks a pet without raising any event, so
    /// the previous master re-synchronizes its pet-armor projections after
    /// the unlink and the unlinked pet keeps no orphaned bonus. The previous
    /// master travels in this call's own Harmony state; nothing is kept
    /// between calls, and the native unlink is unchanged.
    /// </summary>
    [HarmonyPatch(typeof(UnitDescriptor), "SetMaster")]
    internal static class FavoredClassPetUnlinkPatch
    {
        private static void Prefix(UnitDescriptor __instance, UnitEntityData master, out UnitEntityData __state)
        {
            __state = null;
            try
            {
                if (master == null && __instance != null)
                    __state = __instance.Master.Value;
            }
            catch (Exception)
            {
                __state = null;
            }
        }

        private static void Postfix(UnitEntityData __state)
        {
            if (__state == null)
                return;
            try
            {
                FavoredClassPetArmorProjection.SyncMaster(__state);
            }
            catch (Exception)
            {
                // Fail safe: the native unlink stands.
            }
        }
    }
}
