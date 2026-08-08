using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Harmony12;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Root;
using Kingmaker.UI.LevelUp.Phase;
using KingmakerGunslinger.Bootstrap;
using UnityModManagerNet;

namespace KingmakerGunslinger.Compatibility
{
    internal static class ClassCatalogDiagnostics
    {
        private static readonly object Gate = new object();
        private static DateTime _loadDictionaryStartedUtc;
        private static LibraryScriptableObject _observedLibrary;
        private static bool _firstUpdatePending;

        internal static void BeginLoadDictionary(LibraryScriptableObject library)
        {
            lock (Gate)
            {
                _loadDictionaryStartedUtc = DateTime.UtcNow;
                _observedLibrary = library;
            }
        }

        internal static void Capture(string checkpoint, LibraryScriptableObject library)
        {
            Capture(checkpoint, library, null);
        }

        internal static void Capture(string checkpoint, LibraryScriptableObject library,
            BlueprintCharacterClass candidate)
        {
            try
            {
                if (library != null)
                {
                    lock (Gate) _observedLibrary = library;
                }

                ModContext context;
                if (!ModContext.TryGet(out context)) return;
                LibraryScriptableObject observed;
                DateTime started;
                lock (Gate)
                {
                    observed = _observedLibrary;
                    started = _loadDictionaryStartedUtc;
                }

                BlueprintRoot staticRoot = BlueprintRoot.Instance;
                BlueprintRoot libraryRoot = observed == null ? null : observed.Root;
                BlueprintRoot gameRoot = Game.Instance == null ? null : Game.Instance.BlueprintRoot;
                BlueprintCharacterClass gunslinger = candidate ??
                    (BlueprintBootstrap.GunslingerClass == null ? null :
                        BlueprintBootstrap.GunslingerClass.CharacterClass);
                context.Logger.Info("compatibility", "class-catalog." + checkpoint,
                    "elapsedMs=" + Elapsed(started) +
                    ";library=" + Identity(observed) +
                    ";staticRoot=" + Identity(staticRoot) +
                    ";libraryRoot=" + Identity(libraryRoot) +
                    ";gameRoot=" + Identity(gameRoot) +
                    ";static=" + DescribeCatalog(staticRoot, observed, gunslinger) +
                    ";library=" + DescribeCatalog(libraryRoot, observed, gunslinger) +
                    ";game=" + DescribeCatalog(gameRoot, observed, gunslinger) +
                    ";bootstrap=" + BlueprintBootstrap.IsInitialized +
                    ";mysteriousStranger=" + DescribeStranger() +
                    ";ummOrder=" + DescribeUmmOrder(context.ModEntry) +
                    ";loadDictionaryPatches=" + DescribeLoadDictionaryPatches(context));
            }
            catch (Exception exception)
            {
                ModContext context;
                if (ModContext.TryGet(out context))
                    context.Logger.Failure("compatibility", "class-catalog.diagnostic-fault",
                        "Class-catalog diagnostics failed at " + checkpoint + ".", exception);
            }
        }

        internal static void AttachFirstUpdate(ModContext context)
        {
            if (context == null) return;
            lock (Gate)
            {
                if (_firstUpdatePending) return;
                _firstUpdatePending = true;
            }
            context.ModEntry.OnUpdate += FirstUpdate;
        }

        internal static void CaptureSelectorResult(BlueprintCharacterClass[] classes)
        {
            ModContext context;
            if (!ModContext.TryGet(out context)) return;
            BlueprintCharacterClass gunslinger = BlueprintBootstrap.GunslingerClass == null
                ? null : BlueprintBootstrap.GunslingerClass.CharacterClass;
            context.Logger.Info("compatibility", "class-catalog.chargen-selector-result",
                DescribeClasses(classes, gunslinger));
        }

        private static void FirstUpdate(UnityModManager.ModEntry entry, float delta)
        {
            ModContext context;
            if (!ModContext.TryGet(out context)) return;
            context.ModEntry.OnUpdate -= FirstUpdate;
            lock (Gate) _firstUpdatePending = false;
            Capture("first-idle-update", null);
        }

        private static string DescribeCatalog(BlueprintRoot root,
            LibraryScriptableObject library, BlueprintCharacterClass gunslinger)
        {
            ProgressionRoot progression = root == null ? null : root.Progression;
            BlueprintCharacterClass[] classes = progression == null
                ? null : progression.CharacterClasses;
            bool libraryContains = false;
            if (library != null && library.BlueprintsByAssetId != null && gunslinger != null)
            {
                BlueprintScriptableObject value;
                libraryContains = library.BlueprintsByAssetId.TryGetValue(
                    gunslinger.AssetGuid, out value) && ReferenceEquals(value, gunslinger);
            }
            return "root=" + Identity(root) + ",progression=" + Identity(progression) +
                ",array=" + Identity(classes) + ",libraryHasExact=" + libraryContains +
                "," + DescribeClasses(classes, gunslinger);
        }

        private static string DescribeClasses(BlueprintCharacterClass[] classes,
            BlueprintCharacterClass gunslinger)
        {
            if (classes == null) return "count=<null>,byReference=0,byGuid=0,classes=<null>";
            int byReference = classes.Count(value => ReferenceEquals(value, gunslinger));
            int byGuid = gunslinger == null ? 0 : classes.Count(value => value != null &&
                string.Equals(value.AssetGuid, gunslinger.AssetGuid, StringComparison.Ordinal));
            string ordered = string.Join("|", classes.Select((value, index) =>
                index.ToString(CultureInfo.InvariantCulture) + ":" +
                (value == null ? "<null>" : value.AssetGuid + "/" + value.name)).ToArray());
            return "count=" + classes.Length + ",byReference=" + byReference +
                ",byGuid=" + byGuid + ",classes=" + ordered;
        }

        private static string DescribeStranger()
        {
            var set = BlueprintBootstrap.GunslingerClass;
            return set == null || set.MysteriousStranger == null ||
                set.MysteriousStranger.Archetype == null ? "missing" :
                set.MysteriousStranger.Archetype.AssetGuid + "/" +
                set.MysteriousStranger.Archetype.name;
        }

        private static string DescribeUmmOrder(UnityModManager.ModEntry current)
        {
            Type manager = current == null ? null : current.GetType().DeclaringType;
            FieldInfo field = manager == null ? null : manager.GetField("modEntries",
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            IEnumerable entries = field == null ? null : field.GetValue(null) as IEnumerable;
            if (entries == null) return "<unavailable>";
            var values = new List<string>();
            foreach (object value in entries)
            {
                UnityModManager.ModEntry entry = value as UnityModManager.ModEntry;
                if (entry != null) values.Add(entry.Info.Id + "@" + entry.Info.Version);
            }
            return string.Join("|", values.ToArray());
        }

        private static string DescribeLoadDictionaryPatches(ModContext context)
        {
            MethodInfo target = typeof(LibraryScriptableObject).GetMethod("LoadDictionary",
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,
                null, Type.EmptyTypes, null);
            Patches patches = target == null ? null : context.Harmony.GetPatchInfo(target);
            if (patches == null) return "<unavailable>";
            return "prefixes=" + DescribePatches(patches.Prefixes) +
                ",postfixes=" + DescribePatches(patches.Postfixes) +
                ",transpilers=" + DescribePatches(patches.Transpilers);
        }

        private static string DescribePatches(IEnumerable<Patch> values)
        {
            return string.Join("|", values.Select((value, index) => index + ":" +
                value.owner + "/" + value.priority + "/" +
                (value.patch == null ? "<missing>" : value.patch.DeclaringType.FullName +
                    "." + value.patch.Name)).ToArray());
        }

        private static string Identity(object value)
        {
            return value == null ? "<null>" : value.GetType().FullName + "#" +
                RuntimeHelpers.GetHashCode(value).ToString("X8", CultureInfo.InvariantCulture);
        }

        private static string Elapsed(DateTime started)
        {
            return started == default(DateTime) ? "<unavailable>" :
                Math.Max(0, (DateTime.UtcNow - started).TotalMilliseconds)
                    .ToString("F0", CultureInfo.InvariantCulture);
        }
    }

    [HarmonyPatch(typeof(CharBPhaseClassInChargen), "get_m_ClassesCollection")]
    internal static class CharacterCreationClassCatalogDiagnosticPatch
    {
        private static void Prefix()
        {
            ClassCatalogDiagnostics.Capture("before-chargen-selector", null);
        }

        private static void Postfix(BlueprintCharacterClass[] __result)
        {
            ClassCatalogDiagnostics.CaptureSelectorResult(__result);
        }
    }
}
