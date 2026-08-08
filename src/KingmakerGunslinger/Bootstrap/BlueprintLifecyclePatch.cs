using System;
using Harmony12;
using Kingmaker.Blueprints;
using KingmakerGunslinger.Compatibility;

namespace KingmakerGunslinger.Bootstrap
{
    /// <summary>
    /// Observes Kingmaker's zero-argument blueprint dictionary load. The postfix
    /// never lets a mod exception escape into the game's loading pipeline.
    /// </summary>
    [HarmonyPatch(typeof(LibraryScriptableObject), "LoadDictionary")]
    [HarmonyPatch(typeof(LibraryScriptableObject), "LoadDictionary", new Type[0])]
    internal static class BlueprintLifecyclePatch
    {
        private static void Prefix(LibraryScriptableObject __instance)
        {
            ClassCatalogDiagnostics.BeginLoadDictionary(__instance);
        }

        private static void Postfix(LibraryScriptableObject __instance)
        {
            try
            {
                BlueprintBootstrap.Observe(__instance);
            }
            catch (Exception exception)
            {
                ModContext context;
                if (ModContext.TryGet(out context))
                {
                    context.MarkFailed(exception);
                    context.Logger.Failure(
                        "blueprints",
                        "lifecycle.unhandled",
                        "An unexpected exception reached the blueprint lifecycle patch; content initialization is disabled.",
                        exception);
                }
            }
            finally
            {
                ClassCatalogDiagnostics.Capture("gunslinger-postfix-return", __instance);
            }
        }
    }
}
