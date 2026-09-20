using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Spells.Teleportation;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    // Read-only final-live diagnostic (guarded scenario
    // observe-word-of-recall-favored-class) for the optional Favored Class
    // Oracle bonus-spell route.
    // bonus-spell route. Resolves the genuine installed blueprints by exact
    // identity and records each link of the selection chain — the Oracle class
    // list membership, the per-level parametrized feature's own SpellList
    // reference, its LearnSpellParametrized component reference, the native
    // filtered/cached selection state, and the level-7 class-spell-level
    // prerequisite — without mutating any blueprint, cache, unit, or save.
    internal static class WordOfRecallFavoredClassObserver
    {
        internal const string FavoredClassSelectionId =
            "9ba3858327354e2093613efb9de198d7";
        internal const string FavoredClassLevel6FeatureId =
            "7249760f01784ea997afaa9c433c2e68";
        internal const string FavoredClassPartialFeatureId =
            "b19759026d6508b9022f1edb4ec4b31f";
        internal const string FavoredClassOracleClassSelectionId =
            "c6f18fa1194d0bfb35e1913983b8da98";

        private static readonly FieldInfo FilteredCacheField = typeof(SpellLevelList)
            .GetField("m_SpellsFiltered", BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly FieldInfo CachedItemsField =
            typeof(BlueprintParametrizedFeature)
                .GetField("m_CachedItems", BindingFlags.Instance | BindingFlags.NonPublic);

        internal static RuntimeTestResult Run(ModContext context,
            RuntimeTestRequest request)
        {
            var assertions = new List<RuntimeTestAssertion>();
            var diagnostics = new List<string>();
            try
            {
                Observe(context, assertions, diagnostics);
            }
            catch (Exception exception)
            {
                diagnostics.Add("observer-exception=" + exception);
                Add(assertions, "observer-completed", "no exception",
                    exception.GetType().Name, false,
                    "the diagnostic itself must not fail while observing");
            }
            return WordOfRecallFavoredClassObserverResult(context, request,
                assertions, diagnostics);
        }

        private static void Observe(ModContext context,
            List<RuntimeTestAssertion> assertions, List<string> diagnostics)
        {
            var library = BlueprintBootstrap.Library;
            var set = BlueprintBootstrap.Teleportation;
            Add(assertions, "bootstrap-identities", "library and Word of Recall",
                "library=" + (library != null) + ";recall=" +
                    (set == null || set.WordOfRecall == null ? "missing" :
                        set.WordOfRecall.AssetGuid),
                library != null && set != null && set.WordOfRecall != null,
                "Teleportation bootstrap identities at first idle observation");
            if (library == null || set == null || set.WordOfRecall == null) return;
            BlueprintAbility recall = set.WordOfRecall;
            BlueprintCharacterClass oracle =
                TeleportationFinalLiveReconciler.ResolveOracleClass(library);
            Add(assertions, "oracle-class-resolved",
                "Call of the Wild Oracle " +
                    TeleportationFinalLiveReconciler.OracleClassId,
                oracle == null ? "absent" : oracle.AssetGuid,
                oracle != null,
                "production identity+structure resolution used by the reconciler");
            if (oracle == null) return;

            BlueprintFeatureSelection selection = Lookup(library,
                FavoredClassSelectionId) as BlueprintFeatureSelection;
            BlueprintParametrizedFeature level6 = Lookup(library,
                FavoredClassLevel6FeatureId) as BlueprintParametrizedFeature;
            BlueprintFeature partial = Lookup(library,
                FavoredClassPartialFeatureId) as BlueprintFeature;
            BlueprintFeatureSelection classSelection = Lookup(library,
                FavoredClassOracleClassSelectionId) as BlueprintFeatureSelection;
            diagnostics.Add("favored-class-presence=" +
                (selection != null) + "/" + (level6 != null) + "/" +
                (partial != null) + "/" + (classSelection != null));
            Add(assertions, "fcb-oracle-blueprints-present",
                "installed Favored Class Oracle selection/level-6/partial/class-selection",
                Name(selection) + ";" + Name(level6) + ";" + Name(partial) + ";" +
                    Name(classSelection),
                selection != null && level6 != null && partial != null &&
                    classSelection != null,
                "exact installed ZFavoredClass GUIDs from its own blueprint store");
            if (selection == null || level6 == null) return;

            Add(assertions, "fcb-selection-children",
                "exactly the eight per-level parametrized features",
                selection.AllFeatures == null ? "null" :
                    string.Join("|", selection.AllFeatures.Select(value =>
                        (value == null ? "null" : value.name)).ToArray()),
                selection.AllFeatures != null &&
                    selection.AllFeatures.Length == 8 &&
                    selection.AllFeatures.All(value =>
                        value is BlueprintParametrizedFeature),
                "selection AllFeatures membership of the genuine Favored Class route");
            Add(assertions, "fcb-level6-structure",
                "LearnSpell parameter, level 6, specific, no penalty, Oracle caster",
                "type=" + level6.ParameterType + ";level=" + level6.SpellLevel +
                    ";specific=" + level6.SpecificSpellLevel + ";penalty=" +
                    level6.SpellLevelPenalty + ";caster=" +
                    (level6.SpellcasterClass == null ? "null" :
                        level6.SpellcasterClass.AssetGuid) + ";ranks=" + level6.Ranks,
                level6.ParameterType == FeatureParameterType.LearnSpell &&
                    level6.SpellLevel ==
                        TeleportationFinalLiveReconciler.OracleWordOfRecallLevel &&
                    level6.SpecificSpellLevel && level6.SpellLevelPenalty == 0 &&
                    ReferenceEquals(level6.SpellcasterClass, oracle),
                "the cloned native Mystic Theurge template carries the Favored Class per-level contract");

            BlueprintSpellList classList = oracle.Spellbook.SpellList;
            bool featureListShared = ReferenceEquals(level6.SpellList, classList);
            Add(assertions, "fcb-level6-list-identity",
                "the feature's SpellList is the live Oracle class spell-list object",
                "feature=" + (level6.SpellList == null ? "null" :
                        level6.SpellList.AssetGuid + ":" + level6.SpellList.name) +
                    ";class=" + (classList == null ? "null" :
                        classList.AssetGuid + ":" + classList.name),
                featureListShared,
                "reference identity between the selector source and the reconciled class list");

            LearnSpellParametrized[] learn = level6.ComponentsArray == null ?
                new LearnSpellParametrized[0] :
                level6.ComponentsArray.OfType<LearnSpellParametrized>().ToArray();
            LearnSpellParametrized grant = learn.Length == 1 ? learn[0] : null;
            Add(assertions, "fcb-level6-learn-component",
                "exactly one LearnSpellParametrized sharing the class list at level 6",
                "count=" + learn.Length + ";list=" + (grant == null || grant.SpellList == null ? "null" :
                        grant.SpellList.AssetGuid) + ";shared=" +
                    (grant != null && ReferenceEquals(grant.SpellList, classList)) +
                    ";specific=" + (grant != null && grant.SpecificSpellLevel) +
                    ";level=" + (grant == null ? -1 : grant.SpellLevel) +
                    ";caster=" + (grant == null || grant.SpellcasterClass == null ? "null" :
                        grant.SpellcasterClass.AssetGuid),
                learn.Length == 1 && grant.SpellList != null &&
                    ReferenceEquals(grant.SpellList, classList) &&
                    grant.SpecificSpellLevel &&
                    grant.SpellLevel ==
                        TeleportationFinalLiveReconciler.OracleWordOfRecallLevel &&
                    ReferenceEquals(grant.SpellcasterClass, oracle),
                "the component that grants the chosen spell on level-up completion");

            PrerequisiteClassSpellLevel prerequisite = level6.ComponentsArray == null ? null :
                level6.ComponentsArray.OfType<PrerequisiteClassSpellLevel>()
                    .FirstOrDefault();
            Add(assertions, "fcb-level6-prerequisite",
                "class spell level 7 for the Oracle class",
                prerequisite == null ? "absent" :
                    "class=" + (prerequisite.CharacterClass == null ? "null" :
                        prerequisite.CharacterClass.AssetGuid) + ";required=" +
                        prerequisite.RequiredSpellLevel,
                prerequisite != null &&
                    ReferenceEquals(prerequisite.CharacterClass, oracle) &&
                    prerequisite.RequiredSpellLevel ==
                        TeleportationFinalLiveReconciler.OracleWordOfRecallLevel + 1,
                "the genuine Favored Class higher-spell-level prerequisite");

            SpellLevelList level = classList.SpellsByLevel == null ? null :
                classList.SpellsByLevel.SingleOrDefault(value =>
                    value != null && value.SpellLevel ==
                        TeleportationFinalLiveReconciler.OracleWordOfRecallLevel);
            int refs = level == null || level.Spells == null ? 0 :
                level.Spells.Count(value => ReferenceEquals(value, recall));
            int guids = level == null || level.Spells == null ? 0 :
                level.Spells.Count(value => value != null && string.Equals(
                    value.AssetGuid, recall.AssetGuid, StringComparison.Ordinal));
            Add(assertions, "oracle-level6-recall-membership",
                "canonical Word of Recall exactly once in the class list",
                "level=" + (level == null ? "absent" : "present") + ";refs=" +
                    refs + ";guids=" + guids,
                level != null && refs == 1 && guids == 1,
                "production final-live reconciliation result on the live class list");

            var filtered = level == null ? null :
                FilteredCacheField == null ? null :
                    FilteredCacheField.GetValue(level) as List<BlueprintAbility>;
            int filteredRefs = filtered == null ? 0 :
                filtered.Count(value => ReferenceEquals(value, recall));
            Add(assertions, "oracle-level6-filtered-cache",
                "cache absent or current (contains Recall exactly once)",
                "state=" + (filtered == null ? "null" :
                    "materialized=" + filtered.Count + ";recall=" + filteredRefs),
                filtered == null || filteredRefs == 1,
                "the native m_SpellsFiltered cache the level-up extraction reads");

            var cachedItems = CachedItemsField == null ? null :
                CachedItemsField.GetValue(level6) as Array;
            Add(assertions, "fcb-level6-cached-items",
                "native m_CachedItems absent at idle main-menu observation",
                "state=" + (cachedItems == null ? "null" :
                    "materialized=" + cachedItems.Length),
                cachedItems == null,
                "the full-selection item cache; the level-up picker uses the live extraction path");

            bool unlockNotDlc = level != null && level.Spells != null &&
                SpellLevelList.SpellIsNotLocked(recall);
            Add(assertions, "oracle-level6-recall-not-locked",
                "native SpellIsNotLocked accepts canonical Word of Recall",
                "unlocked=" + unlockNotDlc,
                unlockNotDlc,
                "the exact native per-spell filter applied while rebuilding the filtered cache");

            var fullItems = GetItemsSnapshot(level6, recall);
            var variants = level6.BlueprintParameterVariants;
            Add(assertions, "fcb-level6-full-items-recall",
                "the native get_Items/CanSelect source contains canonical Word of Recall",
                "items=" + fullItems.Count + ";recall=" + fullItems.RecallRefs +
                    ";variants=" + (variants == null ? -1 : variants.Length) +
                    ";variantRecall=" + (variants == null ? -1 : variants.Count(value =>
                        ReferenceEquals(value, recall))),
                fullItems.RecallRefs == 1,
                "BlueprintParametrizedFeature.CanSelect admits only items present in get_Items(); this is the pick gate behind the visible extraction");
            diagnostics.Add("fcb-level6-items=" + string.Join("|", fullItems.Names.ToArray()));
            diagnostics.Add("fcb-level6-variants=" + (variants == null ? "null" :
                string.Join("|", variants.Take(60).Select(value =>
                    value == null ? "null" : value.AssetGuid + ":" + value.name).ToArray())));
        }

        private sealed class ItemsSnapshot
        {
            internal int Count;
            internal int RecallRefs;
            internal List<string> Names;
        }

        private static ItemsSnapshot GetItemsSnapshot(
            BlueprintParametrizedFeature feature,
            BlueprintAbility recall)
        {
            var items = feature.Items == null ? new object[0] : feature.Items.Cast<object>().ToArray();
            var snapshot = new ItemsSnapshot { Names = new List<string>() };
            foreach (var item in items)
            {
                var param = item.GetType().GetProperty("Param") == null ? null :
                    item.GetType().GetProperty("Param").GetValue(item, null);
                var value = param == null ? null : param.GetType().GetProperty("Value").GetValue(param, null);
                var blueprint = value == null ? null : value.GetType().GetField("Blueprint").GetValue(value)
                    as BlueprintScriptableObject;
                snapshot.Names.Add(blueprint == null ? "null" :
                    blueprint.AssetGuid + ":" + blueprint.name);
                if (blueprint is BlueprintAbility ability &&
                    ReferenceEquals(ability, recall)) snapshot.RecallRefs++;
            }
            snapshot.Count = items.Length;
            return snapshot;
        }

        private static BlueprintScriptableObject Lookup(
            LibraryScriptableObject library, string assetId)
        {
            BlueprintScriptableObject blueprint;
            return library.BlueprintsByAssetId.TryGetValue(assetId,
                out blueprint) ? blueprint : null;
        }

        private static string Name(BlueprintScriptableObject blueprint)
        {
            return blueprint == null ? "absent" :
                blueprint.AssetGuid + ":" + blueprint.name;
        }

        private static void Add(List<RuntimeTestAssertion> assertions,
            string name, string expected, string observed, bool pass,
            string evidence)
        {
            assertions.Add(new RuntimeTestAssertion { Name = name,
                Expected = expected, Observed = observed,
                Status = pass ? RuntimeTestStatuses.Pass : RuntimeTestStatuses.Fail,
                Evidence = evidence });
        }

        private static RuntimeTestResult WordOfRecallFavoredClassObserverResult(
            ModContext context, RuntimeTestRequest request,
            List<RuntimeTestAssertion> assertions, List<string> diagnostics)
        {
            bool pass = assertions.All(value =>
                value.Status == RuntimeTestStatuses.Pass);
            Assembly assembly = Assembly.GetExecutingAssembly();
            return new RuntimeTestResult
            {
                SchemaVersion = 1,
                RunId = request.RunId,
                Scenario = request.Scenario,
                Status = pass ? RuntimeTestStatuses.Pass : RuntimeTestStatuses.Fail,
                LoadedModVersion = context.ModEntry.Info.Version,
                RuntimeIdentity = assembly.FullName + ";mvid=" +
                    assembly.ManifestModule.ModuleVersionId + ";sha256=" +
                    Hash(assembly.Location) + ";pid=" +
                    Process.GetCurrentProcess().Id,
                GitCommit = Metadata(assembly, "GitCommit"),
                GameVersion = Application.version ?? string.Empty,
                StartUtc = DateTime.UtcNow.ToString("o"),
                EndUtc = DateTime.UtcNow.ToString("o"),
                Assertions = assertions,
                Diagnostics = diagnostics,
                Warnings = new List<string>(),
                ExceptionSummary = string.Empty,
                EvidenceFiles = new List<string>(),
                AutomaticExitRequested = request.ExitAfterCompletion,
                EvidenceDirectory = request.EvidenceDirectory
            };
        }

        private static string Hash(string path)
        {
            using (SHA256 sha = SHA256.Create())
            using (FileStream stream = File.OpenRead(path))
                return BitConverter.ToString(sha.ComputeHash(stream))
                    .Replace("-", string.Empty);
        }

        private static string Metadata(Assembly assembly, string key)
        {
            AssemblyMetadataAttribute value = assembly.GetCustomAttributes(
                typeof(AssemblyMetadataAttribute), false)
                .Cast<AssemblyMetadataAttribute>().FirstOrDefault(item =>
                    item.Key == key);
            return value == null ? string.Empty : value.Value;
        }
    }
}
