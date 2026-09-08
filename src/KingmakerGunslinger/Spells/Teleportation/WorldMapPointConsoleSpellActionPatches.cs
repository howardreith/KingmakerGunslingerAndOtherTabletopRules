using System;
using System.Linq;
using System.Reflection;
using Harmony12;
using Kingmaker.UI._ConsoleUI.Common;
using Kingmaker.UI._ConsoleUI.GlobalMap;
using Kingmaker.UI._ConsoleUI.Utils.MultiNavigationTool;
using KingmakerGunslinger.Bootstrap;
using UnityEngine;

namespace KingmakerGunslinger.Spells.Teleportation
{
    // Native gamepad presentation has its own composition/navigation boundaries.
    // These hooks are installed only for the enabled Teleportation module.
    internal static class WorldMapPointConsoleSpellActionPatches
    {
        internal static bool Installed { get; private set; }
        internal static readonly FieldInfo DialogField = Field("m_Dialog", typeof(CanvasGroup));
        internal static readonly FieldInfo ConfirmField = Field("m_ConfirmButton", typeof(ConsoleButton));
        internal static readonly FieldInfo NavigationField = Field("m_NavigationCollection", typeof(ConsoleMultiNavigationCollection));
        internal static readonly PropertyInfo ViewModelProperty = typeof(GlobalMapMessageBoxView).BaseType.GetProperty("ViewModel", BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly string[] Methods = { "SetFromLocation", "FillDialogInfoLocation", "UpdateNavigation", "DestroyViewImplementation" };
        internal static void Install(ModContext context)
        {
            if (!context.FeatureModules.Active.TeleportationSpells || Installed) return;
            try
            {
                if (DialogField == null || ConfirmField == null || NavigationField == null || ViewModelProperty == null ||
                    ViewModelProperty.PropertyType != typeof(GlobalMapMessageBoxVM) || !TeleportationConfirmationSurface.ConsoleContractValid)
                    throw new InvalidOperationException("Native gamepad destination/confirmation fields differ.");
                var methods = Methods.Select(name => typeof(GlobalMapMessageBoxView).GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                    .Single(value => value.Name == name)).ToArray();
                for (int i = 0; i < methods.Length; i++)
                    if (methods[i].ReturnType != typeof(void) || methods[i].GetParameters().Length != (i == 1 ? 1 : 0) ||
                        i == 1 && methods[i].GetParameters()[0].ParameterType != typeof(bool))
                        throw new InvalidOperationException("Native gamepad destination method signature differs.");
                context.Harmony.Patch(methods[0], Callback("ClearPrefix"), null, null);
                context.Harmony.Patch(methods[1], null, Callback("FillPostfix"), null);
                context.Harmony.Patch(methods[2], null, Callback("NavigationPostfix"), null);
                context.Harmony.Patch(methods[3], Callback("ClearPrefix"), null, null);
                Installed = true;
                context.Logger.Info("teleportation", "destination.console-hooks-installed", "presenter=GlobalMapMessageBoxView;nativeAcceptUnpatched=true;nativeNavigationRetained=true");
            }
            catch (Exception exception)
            {
                Installed = false;
                foreach (string name in Methods)
                {
                    var method = typeof(GlobalMapMessageBoxView).GetMethod(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    if (method != null) try { context.Harmony.Unpatch(method, HarmonyPatchType.All, context.ModId); }
                        catch (Exception cleanup) { context.Logger.Failure("teleportation", "destination.console-unpatch-failed", "Callbacks remain inert.", cleanup); }
                }
                context.Logger.Failure("teleportation", "destination.console-hooks-unavailable", "Native gamepad destination interaction retained; magical rows omitted.", exception);
            }
        }
        internal static GlobalMapMessageBoxVM Model(GlobalMapMessageBoxView view)
        { return (GlobalMapMessageBoxVM)ViewModelProperty.GetValue(view, null); }
        private static FieldInfo Field(string name, Type type)
        {
            var field = typeof(GlobalMapMessageBoxView).GetField(name, BindingFlags.Instance | BindingFlags.NonPublic);
            return field != null && field.FieldType == type ? field : null;
        }
        private static HarmonyMethod Callback(string name)
        { return new HarmonyMethod(typeof(WorldMapPointConsoleSpellActionPatches).GetMethod(name, BindingFlags.Static | BindingFlags.NonPublic)); }
        private static void ClearPrefix(GlobalMapMessageBoxView __instance)
        { WorldMapPointConsoleSpellActionRuntime.Clear(__instance); }
        private static void FillPostfix(GlobalMapMessageBoxView __instance)
        { if (Installed) WorldMapPointConsoleSpellActionRuntime.Append(__instance); }
        private static void NavigationPostfix(GlobalMapMessageBoxView __instance)
        { if (Installed) WorldMapPointConsoleSpellActionRuntime.Register(__instance); }
    }
}
