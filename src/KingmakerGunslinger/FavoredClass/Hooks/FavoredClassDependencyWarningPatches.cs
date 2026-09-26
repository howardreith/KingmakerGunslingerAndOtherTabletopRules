using System;
using System.Reflection;
using Harmony12;
using Kingmaker;
using Kingmaker.Blueprints;
using KingmakerGunslinger.Bootstrap;
using Newtonsoft.Json;

namespace KingmakerGunslinger.FavoredClass.Hooks
{
    /// <summary>
    /// L06: the three read-only observation points of the missing-dependency
    /// warning. A save load drops earlier records (Game.LoadGame); the native
    /// blueprint converter's unresolvable references are recorded before it
    /// throws exactly as before (it is never replaced or suppressed); and the
    /// return to the main menu after a failed load carries KMG's warning
    /// before the native message. Nothing here loads, substitutes or writes.
    /// </summary>
    internal static class FavoredClassDependencyWarningPatches
    {
        internal const string ConverterTypeName = "Kingmaker.EntitySystem.Persistence.JsonUtility.BlueprintConverter";

        /// <summary>Whether the converter observation is installed (set when it applies).</summary>
        internal static bool ConverterInstalled { get; private set; }

        /// <summary>
        /// The internal native converter is patched by hand: when its exact
        /// shape is absent the warning simply has no records and every load
        /// behaves natively.
        /// </summary>
        internal static void Install(HarmonyInstance harmony)
        {
            if (harmony == null) throw new ArgumentNullException("harmony");
            Type converter = typeof(Game).Assembly.GetType(ConverterTypeName, false);
            MethodInfo read = converter == null ? null : converter.GetMethod("ReadJson",
                BindingFlags.Instance | BindingFlags.Public, null,
                new[] { typeof(JsonReader), typeof(Type), typeof(object), typeof(JsonSerializer) }, null);
            MethodInfo prefix = typeof(FavoredClassDependencyWarningPatches).GetMethod("ConverterPrefix",
                BindingFlags.Static | BindingFlags.NonPublic);
            if (read == null || prefix == null)
                return;
            harmony.Patch(read, new HarmonyMethod(prefix), null, null);
            ConverterInstalled = true;
        }

        private static void ConverterPrefix(JsonReader reader)
        {
            try
            {
                var guid = reader == null ? null : reader.Value as string;
                if (guid == null || guid == "null")
                    return;
                // The same library the native lookup reads, without its log line.
                LibraryScriptableObject library = BlueprintBootstrap.Library;
                if (library != null && library.BlueprintsByAssetId != null &&
                    !library.BlueprintsByAssetId.ContainsKey(guid))
                    FavoredClassDependencyWarning.RecordMissing(guid);
            }
            catch (Exception)
            {
                // Observation only: the native conversion runs unchanged.
            }
        }
    }

    [HarmonyPatch(typeof(Game), "LoadGame")]
    internal static class FavoredClassDependencyLoadPatch
    {
        private static void Prefix()
        {
            try
            {
                FavoredClassDependencyWarning.BeginLoad();
            }
            catch (Exception)
            {
            }
        }
    }

    [HarmonyPatch(typeof(Game), "ResetToMainMenu")]
    internal static class FavoredClassDependencyMenuPatch
    {
        private static void Prefix(ref string message)
        {
            try
            {
                message = FavoredClassDependencyWarning.Augment(message);
            }
            catch (Exception)
            {
                // The native message is kept whatever happens here.
            }
        }
    }
}
