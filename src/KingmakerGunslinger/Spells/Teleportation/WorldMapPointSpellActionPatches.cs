using System;
using System.Linq;
using System.Reflection;
using Harmony12;
using Kingmaker.UI;
using Kingmaker.UI.GlobalMap;
using KingmakerGunslinger.Bootstrap;
using TMPro;
using UnityEngine;

namespace KingmakerGunslinger.Spells.Teleportation
{
    // Deliberately manual registration: module OFF attaches no destination hooks.
    internal static class WorldMapPointSpellActionPatches
    {
        internal static bool Installed { get; private set; }
        internal static readonly FieldInfo DialogField = Field("m_Dialog", typeof(CanvasGroup));
        internal static readonly FieldInfo LocationField = Field("m_Location", typeof(Kingmaker.Globalmap.GlobalMapLocation));
        internal static readonly FieldInfo AcceptTextField = Field("m_AcceptText", typeof(TextMeshProUGUI));
        internal static readonly FieldInfo TeleportControllersField = Field("m_TeleportControllers", typeof(GameObject));
        internal static readonly FieldInfo ConfirmationCallbackField = typeof(DialogMessageBox).GetField("m_OnClose", BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly string[] Methods = { "OnLocationSelect", "FillDialogInfoLocation", "Hide", "Dispose" };
        internal static void Install(ModContext context)
        {
            if (!context.FeatureModules.Active.TeleportationSpells || Installed) return;
            try
            {
                if (DialogField == null || LocationField == null || AcceptTextField == null || TeleportControllersField == null ||
                    ConfirmationCallbackField == null || ConfirmationCallbackField.FieldType != typeof(Action<DialogMessageBoxBase.BoxButton>))
                    throw new InvalidOperationException("Native destination/confirmation presenter fields differ.");
                var methods = Methods.Select(name => typeof(GlobalMapMessageBox).GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                    .Single(value => value.Name == name)).ToArray();
                if (methods[1].ReturnType != typeof(void) || methods[1].GetParameters().Length != 1 ||
                    methods[1].GetParameters()[0].ParameterType != typeof(bool)) throw new InvalidOperationException("Native destination fill contract differs.");
                context.Harmony.Patch(methods[0], Callback("ClearPrefix"), null, null);
                context.Harmony.Patch(methods[1], null, Callback("FillPostfix"), null);
                context.Harmony.Patch(methods[2], Callback("ClearPrefix"), null, null);
                context.Harmony.Patch(methods[3], Callback("ClearPrefix"), null, null);
                Installed = true;
                context.Logger.Info("teleportation", "destination.hooks-installed", "presenter=GlobalMapMessageBox;appendAfter=FillDialogInfoLocation;nativeAcceptUnpatched=true");
            }
            catch (Exception exception)
            {
                Installed = false;
                foreach (string name in Methods)
                {
                    MethodInfo method = typeof(GlobalMapMessageBox).GetMethod(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    if (method != null) try { context.Harmony.Unpatch(method, HarmonyPatchType.All, context.ModId); }
                        catch (Exception cleanup) { context.Logger.Failure("teleportation", "destination.unpatch-failed", "Callbacks remain inert.", cleanup); }
                }
                context.Logger.Failure("teleportation", "destination.hooks-unavailable", "Native destination interaction retained; magical actions omitted.", exception);
            }
        }
        private static FieldInfo Field(string name, Type type)
        {
            FieldInfo field = typeof(GlobalMapMessageBox).GetField(name, BindingFlags.Instance | BindingFlags.NonPublic);
            return field != null && field.FieldType == type ? field : null;
        }
        private static HarmonyMethod Callback(string name)
        { return new HarmonyMethod(typeof(WorldMapPointSpellActionPatches).GetMethod(name, BindingFlags.Static | BindingFlags.NonPublic)); }
        private static void ClearPrefix(GlobalMapMessageBox __instance)
        { WorldMapPointSpellActionRuntime.Clear(__instance); }
        private static void FillPostfix(GlobalMapMessageBox __instance)
        { if (Installed) WorldMapPointSpellActionRuntime.Append(__instance); }
    }
}
