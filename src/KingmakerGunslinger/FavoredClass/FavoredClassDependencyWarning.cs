using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using KingmakerGunslinger.Bootstrap;

namespace KingmakerGunslinger.FavoredClass
{
    /// <summary>
    /// L06: a save whose builds reference Favored Class (or Call of the Wild)
    /// blueprints that are not loaded fails in the native loader: the
    /// blueprint converter throws, nothing is read into the game, nothing is
    /// written, and the game returns to the main menu with a generic "Cannot
    /// load game" message. KMG never substitutes a missing blueprint. It
    /// records the references the native converter could not resolve during
    /// that load, classifies the ones it can prove (the host's favored class
    /// choice and its per-class favored progressions and bonus selections by
    /// their verified identity rule, and the Call of the Wild content its
    /// manifests name) and puts a precise recovery warning before the native
    /// message.
    /// </summary>
    internal static class FavoredClassDependencyWarning
    {
        internal const int MaximumRecorded = 512;

        private static readonly object Gate = new object();
        private static readonly List<string> s_Missing = new List<string>();
        private static readonly HashSet<string> s_Seen = new HashSet<string>(StringComparer.Ordinal);
        private static ModLogger s_Logger;

        internal static void Initialize(ModLogger logger)
        {
            lock (Gate) s_Logger = logger;
        }

        /// <summary>A save load begins (Game.LoadGame): earlier records are dropped.</summary>
        internal static void BeginLoad()
        {
            lock (Gate)
            {
                s_Missing.Clear();
                s_Seen.Clear();
            }
        }

        /// <summary>The native blueprint converter could not resolve this reference (any thread).</summary>
        internal static void RecordMissing(string guid)
        {
            if (string.IsNullOrEmpty(guid))
                return;
            lock (Gate)
                if (s_Missing.Count < MaximumRecorded && s_Seen.Add(guid))
                    s_Missing.Add(guid);
        }

        /// <summary>The references recorded since the load began, in order.</summary>
        internal static string[] Missing
        {
            get { lock (Gate) return s_Missing.ToArray(); }
        }

        /// <summary>
        /// The native return-to-menu message after a failed load, preceded by
        /// KMG's warning when the failure involves the favored-class
        /// dependencies; any other message is returned unchanged. The records
        /// are consumed.
        /// </summary>
        internal static string Augment(string nativeMessage)
        {
            string[] missing;
            ModLogger logger;
            lock (Gate)
            {
                missing = s_Missing.ToArray();
                s_Missing.Clear();
                s_Seen.Clear();
                logger = s_Logger;
            }
            if (nativeMessage == null || missing.Length == 0)
                return nativeMessage;
            FavoredClassDependencyReport report = FavoredClassDependencyPolicy.Classify(missing,
                FavoredClassIntegrationStatusRegistry.Current.Availability, KnownClassGuids());
            if (!report.Relevant)
                return nativeMessage;
            if (logger != null)
                logger.Warning("favored-class", "missing-dependency.warning", report.ToString());
            return FavoredClassDependencyPolicy.Compose(report) + "\n\n" + nativeMessage;
        }

        /// <summary>Every class in the loaded library and the Call of the Wild Oracle.</summary>
        internal static IEnumerable<string> KnownClassGuids()
        {
            var classes = new HashSet<string>(StringComparer.Ordinal)
            {
                FavoredClassRevelationManifest.OracleClassGuid
            };
            LibraryScriptableObject library = BlueprintBootstrap.Library;
            if (library != null && library.BlueprintsByAssetId != null)
                foreach (BlueprintScriptableObject blueprint in library.BlueprintsByAssetId.Values)
                {
                    var characterClass = blueprint as BlueprintCharacterClass;
                    if (characterClass != null && !string.IsNullOrEmpty(characterClass.AssetGuid))
                        classes.Add(characterClass.AssetGuid);
                }
            return classes;
        }

    }
}
