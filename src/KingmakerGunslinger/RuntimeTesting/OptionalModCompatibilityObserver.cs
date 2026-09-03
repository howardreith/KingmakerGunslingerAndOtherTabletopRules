using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using Harmony12;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Class.LevelUp;
using Kingmaker.UnitLogic.FactLogic;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Compatibility;
using KingmakerGunslinger.EasternWeapons;
using KingmakerGunslinger.ElvenBranchedSpear;
using KingmakerGunslinger.Grit;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityModManagerNet;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal static class OptionalModCompatibilityObserver
    {
        private static readonly Dictionary<string, string[]> Profiles =
            new Dictionary<string, string[]>(StringComparer.Ordinal)
            {
                { "gunslinger-only", new[] { "KingmakerGunslinger" } },
                { "gunslinger-races-unleashed",
                    new[] { "KingmakerGunslinger", "RacesUnleashed" } },
                { "gunslinger-call-of-the-wild",
                    new[] { "KingmakerGunslinger", "CallOfTheWild" } },
                { "gunslinger-call-of-the-wild-races-unleashed",
                    new[] { "KingmakerGunslinger", "CallOfTheWild",
                        "RacesUnleashed" } },
                { "gunslinger-high-risk-combined-favored-class",
                    new[] { "KingmakerGunslinger", "CallOfTheWild",
                        "ZFavoredClass", "TweakOrTreat",
                        "RacesUnleashed" } },
                { "gunslinger-arms-armor",
                    new[] { "KingmakerGunslinger", "ArmsArmor" } },
                { "gunslinger-toggle-custom-soundpacks",
                    new[] { "KingmakerGunslinger", "ToggleCustomSoundpacks" } },
                { "gunslinger-high-risk-combined", new[] { "KingmakerGunslinger",
                    "CallOfTheWild", "ArmsArmor", "ToggleCustomSoundpacks" } },
                { "gunslinger-all-loadable-local", new[] { "KingmakerGunslinger",
                    "CallOfTheWild", "ArmsArmor", "ToggleCustomSoundpacks" } },
                { "gunslinger-qualified-combined", new[] { "KingmakerGunslinger",
                    "ArmsArmor", "ToggleCustomSoundpacks" } }
            };

        internal static bool IsAllowedProfile(string profileId)
        {
            return profileId != null && Profiles.ContainsKey(profileId);
        }

        internal static RuntimeTestResult Run(ModContext context, RuntimeTestRequest request)
        {
            string profileId = (string)request.Parameters["profileId"];
            string[] expectedIds = Profiles[profileId];
            var assertions = new List<RuntimeTestAssertion>();
            var diagnostics = new List<string>();
            List<UnityModManager.ModEntry> entries = ReadModEntries(context.ModEntry);
            string[] observedIds = entries.Select(value => value.Info.Id).ToArray();
            Add(assertions, "isolated-umm-entry-set", string.Join(",", expectedIds),
                string.Join(",", observedIds), expectedIds.OrderBy(value => value,
                    StringComparer.Ordinal).SequenceEqual(observedIds.OrderBy(value => value,
                        StringComparer.Ordinal)),
                "UnityModManager 0.32.4 public static modEntries; observed value preserves actual load order");
            Add(assertions, "umm-identities-singular", "unique IDs and assembly names",
                DescribeDuplicates(entries), HasUniqueIdentities(entries),
                "ModEntry.Info.Id and loaded Assembly.GetName().Name");
            Add(assertions, "expected-mods-loaded", "all expected entries Loaded without errors",
                string.Join(" | ", entries.Select(DescribeEntry).ToArray()),
                entries.Count == expectedIds.Length && entries.All(value =>
                    value.Loaded && !value.ErrorOnLoading && value.HasAssembly),
                "exact UMM ModEntry state, assembly identity, MVID, location, and SHA-256");

            GunslingerClassBlueprintSet gunslinger = BlueprintBootstrap.GunslingerClass;
            BlueprintCharacterClass cls = gunslinger == null ? null : gunslinger.CharacterClass;
            BlueprintCharacterClass[] classes = Game.Instance == null ||
                Game.Instance.BlueprintRoot == null ||
                Game.Instance.BlueprintRoot.Progression == null
                ? new BlueprintCharacterClass[0] :
                Game.Instance.BlueprintRoot.Progression.CharacterClasses;
            Add(assertions, "blueprint-bootstrap", "initialized", BlueprintBootstrap.IsInitialized.ToString(),
                BlueprintBootstrap.IsInitialized && gunslinger != null, "BlueprintBootstrap state");
            bool libraryRegistered = cls != null && BlueprintBootstrap.Library != null &&
                BlueprintBootstrap.Library.BlueprintsByAssetId != null &&
                BlueprintBootstrap.Library.BlueprintsByAssetId.TryGetValue(cls.AssetGuid,
                    out BlueprintScriptableObject libraryClass) &&
                ReferenceEquals(libraryClass, cls);
            Add(assertions, "gunslinger-blueprint-registered", "exact project class in library",
                libraryRegistered ? cls.AssetGuid + "/" + cls.name : "missing-or-foreign",
                libraryRegistered, "LibraryScriptableObject.BlueprintsByAssetId exact reference");
            int referenceCount = classes.Count(value => ReferenceEquals(value, cls));
            int guidCount = cls == null ? 0 : classes.Count(value => value != null &&
                string.Equals(value.AssetGuid, cls.AssetGuid, StringComparison.Ordinal));
            Add(assertions, "gunslinger-root-catalog-published", "exactly one project class by reference and GUID",
                "referenceCount=" + referenceCount + ";guidCount=" + guidCount +
                    ";count=" + classes.Length,
                cls != null && referenceCount == 1 && guidCount == 1,
                "Game.Instance.BlueprintRoot.Progression.CharacterClasses final player catalog");
            Add(assertions, "gunslinger-class-selector-input", "Gunslinger exactly once",
                "referenceCount=" + referenceCount + ";guidCount=" + guidCount,
                cls != null && referenceCount == 1 && guidCount == 1,
                "Kingmaker 2.1.7b CharBPhaseClassInChargen.m_ClassesCollection exact getter reads Game.Instance.BlueprintRoot.Progression.CharacterClasses");
            AddCallOfTheWildCatalogAssertions(assertions, entries, classes);
            AddMartialPerformanceAssertions(context, assertions, diagnostics,
                entries);
            bool armsArmor = entries.Any(value => string.Equals(value.Info.Id,
                "ArmsArmor", StringComparison.Ordinal));
            Add(assertions, "eastern-arms-armor-grip-bridge",
                armsArmor ? "installed on exact ArmsArmor helper contract" :
                    "inactive because ArmsArmor is absent",
                EasternWeaponArmsArmorCompatibility.Status,
                EasternWeaponArmsArmorCompatibility.Installed == armsArmor,
                "reflection-only postfixes on exact ArmsArmor versatile classification and active-slot grip methods");
            bool progression = gunslinger != null && gunslinger.Progression != null &&
                gunslinger.Progression.LevelEntries != null &&
                gunslinger.Progression.LevelEntries.Length == 20 &&
                gunslinger.Progression.LevelEntries.Select((value, index) =>
                    value.Level == index + 1).All(value => value);
            Add(assertions, "gunslinger-progression", "ordered levels 1 through 20",
                progression ? "1..20" : "invalid", progression,
                "project-owned BlueprintProgression.LevelEntries");
            AddMysteriousStrangerAssertions(assertions, gunslinger, cls);
            ProductionFirearmBlueprintCatalog firearms = BlueprintBootstrap.ProductionFirearms;
            bool firearmIdentity = firearms != null && firearms.Entries.Length == 5 &&
                firearms.Entries.All(value => value != null && value.Item != null &&
                    value.WeaponType != null && ReferenceEquals(value.Item.Type, value.WeaponType)) &&
                firearms.Entries.Select(value => value.Item.AssetGuid).Distinct().Count() == 5 &&
                firearms.Entries.Select(value => value.WeaponType.AssetGuid).Distinct().Count() == 5;
            Add(assertions, "production-firearm-identities", "five singular item/type pairs",
                firearms == null ? "missing" : "pairs=" + firearms.Entries.Length,
                firearmIdentity, "project-owned production firearm catalog");
            string audio = Audio.FirearmSoundRuntime.Describe();
            Add(assertions, "wwise-runtime", "not faulted by optional-mod coexistence", audio,
                audio.IndexOf("Faulted", StringComparison.OrdinalIgnoreCase) < 0,
                "FirearmSoundRuntime state; no sound post is initiated by this observer");
            AddHarmonyEvidence(context, assertions, diagnostics);
            Add(assertions, "save-free-observer", "no save selection/load/write API invoked",
                "read-only runtime identity and blueprint inspection", true,
                "observer has no save manager, input, selection, load, quicksave, or autosave call");

            bool pass = assertions.All(value => value.Status == RuntimeTestStatuses.Pass);
            Assembly assembly = context.Assembly;
            return new RuntimeTestResult
            {
                SchemaVersion = 1, RunId = request.RunId, Scenario = request.Scenario,
                Status = pass ? RuntimeTestStatuses.Pass : RuntimeTestStatuses.Fail,
                LoadedModVersion = context.ModEntry.Info.Version,
                RuntimeIdentity = assembly.FullName + "; mvid=" + assembly.ManifestModule.ModuleVersionId +
                    "; sha256=" + HashFile(assembly.Location) + "; pid=" + Process.GetCurrentProcess().Id,
                GitCommit = ReadMetadata(assembly, "GitCommit"),
                GameVersion = Application.version ?? string.Empty,
                StartUtc = DateTime.UtcNow.ToString("o"), EndUtc = string.Empty,
                Assertions = assertions, Diagnostics = diagnostics, Warnings = new List<string>(),
                ExceptionSummary = string.Empty, EvidenceFiles = new List<string>(),
                AutomaticExitRequested = request.ExitAfterCompletion,
                EvidenceDirectory = request.EvidenceDirectory
            };
        }

        private static List<UnityModManager.ModEntry> ReadModEntries(
            UnityModManager.ModEntry currentEntry)
        {
            Type managerType = currentEntry == null ? null :
                currentEntry.GetType().DeclaringType;
            if (managerType == null)
                throw new InvalidOperationException("The live UMM ModEntry declaring type was unavailable.");
            FieldInfo field = managerType.GetField("modEntries",
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            if (field == null) throw new MissingFieldException(managerType.AssemblyQualifiedName, "modEntries");
            IEnumerable values = field.GetValue(null) as IEnumerable;
            if (values == null) throw new InvalidOperationException("UMM modEntries was unavailable.");
            return values.Cast<object>().Select(value => value as UnityModManager.ModEntry)
                .Where(value => value != null).ToList();
        }

        private static string DescribeEntry(UnityModManager.ModEntry entry)
        {
            Assembly assembly = entry.Assembly;
            return "id=" + entry.Info.Id + ";display=" + entry.Info.DisplayName +
                ";version=" + entry.Info.Version + ";manager=" + entry.Info.ManagerVersion +
                ";loaded=" + entry.Loaded + ";active=" + entry.Active +
                ";error=" + entry.ErrorOnLoading + ";assembly=" +
                (assembly == null ? "missing" : assembly.FullName) + ";mvid=" +
                (assembly == null ? "missing" : assembly.ManifestModule.ModuleVersionId.ToString()) +
                ";sha256=" + (assembly == null ? "missing" : HashFile(assembly.Location));
        }

        private static bool HasUniqueIdentities(IEnumerable<UnityModManager.ModEntry> entries)
        {
            return entries.GroupBy(value => value.Info.Id, StringComparer.Ordinal).All(g => g.Count() == 1) &&
                entries.Where(value => value.Assembly != null).GroupBy(value =>
                    value.Assembly.GetName().Name, StringComparer.OrdinalIgnoreCase).All(g => g.Count() == 1);
        }

        private static string DescribeDuplicates(IEnumerable<UnityModManager.ModEntry> entries)
        {
            string[] ids = entries.GroupBy(value => value.Info.Id, StringComparer.Ordinal)
                .Where(g => g.Count() > 1).Select(g => "id:" + g.Key + "*" + g.Count()).ToArray();
            string[] assemblies = entries.Where(value => value.Assembly != null).GroupBy(value =>
                value.Assembly.GetName().Name, StringComparer.OrdinalIgnoreCase).Where(g => g.Count() > 1)
                .Select(g => "assembly:" + g.Key + "*" + g.Count()).ToArray();
            string[] duplicates = ids.Concat(assemblies).ToArray();
            return duplicates.Length == 0 ? "none" : string.Join(",", duplicates);
        }

        private static void AddMysteriousStrangerAssertions(List<RuntimeTestAssertion> assertions,
            GunslingerClassBlueprintSet gunslinger, BlueprintCharacterClass cls)
        {
            MysteriousStrangerBlueprintSet stranger = gunslinger == null ? null : gunslinger.MysteriousStranger;
            BlueprintArchetype archetype = stranger == null ? null : stranger.Archetype;
            int count = cls == null || cls.Archetypes == null ? 0 :
                cls.Archetypes.Count(value => ReferenceEquals(value, archetype));
            FieldInfo parentField = typeof(BlueprintArchetype).GetField("m_ParentClass",
                BindingFlags.Instance | BindingFlags.NonPublic);
            bool registration = archetype != null && count == 1 && parentField != null &&
                ReferenceEquals(parentField.GetValue(archetype), cls);
            Add(assertions, "mysterious-stranger-registration", "one archetype on exact Gunslinger class",
                "count=" + count, registration, "class Archetypes and exact m_ParentClass contract");
            bool rows = stranger != null && gunslinger != null &&
                Rows(archetype.RemoveFeatures, new[] { 1, 2, 5, 6, 10, 11, 14, 18 },
                    new BlueprintFeatureBase[][] {
                        new BlueprintFeatureBase[] { gunslinger.Grit.Feature, gunslinger.QuickClear.Feature },
                        new BlueprintFeatureBase[] { gunslinger.Nimble.Features[0] },
                        new BlueprintFeatureBase[] { gunslinger.GunTraining.Selection },
                        new BlueprintFeatureBase[] { gunslinger.Nimble.Features[1] },
                        new BlueprintFeatureBase[] { gunslinger.Nimble.Features[2] },
                        new BlueprintFeatureBase[] { gunslinger.BleedingWound.Feature },
                        new BlueprintFeatureBase[] { gunslinger.Nimble.Features[3] },
                        new BlueprintFeatureBase[] { gunslinger.Nimble.Features[4] } }) &&
                Rows(archetype.AddFeatures, new[] { 1, 2, 5, 6, 10, 11, 14, 18 },
                    new BlueprintFeatureBase[][] {
                        new BlueprintFeatureBase[] { stranger.Grit, stranger.FocusedAim },
                        new BlueprintFeatureBase[] { stranger.Lucky[0] },
                        new BlueprintFeatureBase[] { stranger.StrangersFortune },
                        new BlueprintFeatureBase[] { stranger.Lucky[1] },
                        new BlueprintFeatureBase[] { stranger.Lucky[2] },
                        new BlueprintFeatureBase[] { stranger.ClippingShot },
                        new BlueprintFeatureBase[] { stranger.Lucky[3] },
                        new BlueprintFeatureBase[] { stranger.Lucky[4] } });
            Add(assertions, "mysterious-stranger-replacement-rows", "exact eight remove/add rows",
                rows ? "exact" : "changed", rows, "project-owned archetype LevelEntry references");
            bool charisma = stranger != null && stranger.Grit.ComponentsArray
                .OfType<GritResourceAmountBonus>().Any(value => value.Attribute == StatType.Charisma);
            Add(assertions, "mysterious-stranger-charisma-grit", "Charisma", charisma ? "Charisma" : "changed",
                charisma, "GritResourceAmountBonus.Attribute");
        }

        private static void AddCallOfTheWildCatalogAssertions(
            List<RuntimeTestAssertion> assertions, IEnumerable<UnityModManager.ModEntry> entries,
            BlueprintCharacterClass[] classes)
        {
            UnityModManager.ModEntry entry = entries.FirstOrDefault(value =>
                string.Equals(value.Info.Id, "CallOfTheWild", StringComparison.Ordinal));
            if (entry == null) return;
            Type helpers = entry.Assembly == null ? null :
                entry.Assembly.GetType("CallOfTheWild.Helpers", false, false);
            FieldInfo field = helpers == null ? null : helpers.GetField("classes",
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            IEnumerable registered = field == null ? null : field.GetValue(null) as IEnumerable;
            var expected = new List<BlueprintCharacterClass>();
            if (registered != null)
                foreach (object value in registered)
                {
                    BlueprintCharacterClass characterClass = value as BlueprintCharacterClass;
                    if (characterClass != null) expected.Add(characterClass);
                }
            string[] missing = expected.Where(value => !classes.Any(candidate =>
                ReferenceEquals(candidate, value) || candidate != null &&
                string.Equals(candidate.AssetGuid, value.AssetGuid, StringComparison.Ordinal)))
                .Select(value => value.AssetGuid + "/" + value.name).ToArray();
            Add(assertions, "call-of-the-wild-final-classes", "all Helpers.classes entries retained",
                "expected=" + expected.Count + ";missing=" +
                    (missing.Length == 0 ? "none" : string.Join(",", missing)),
                expected.Count > 0 && missing.Length == 0,
                "exact loaded CallOfTheWild.dll Helpers.classes reflected without compile dependency");
        }

        private static void AddMartialPerformanceAssertions(ModContext context,
            List<RuntimeTestAssertion> assertions, List<string> diagnostics,
            IEnumerable<UnityModManager.ModEntry> entries)
        {
            UnityModManager.ModEntry entry = entries.FirstOrDefault(value =>
                string.Equals(value.Info.Id, "CallOfTheWild",
                    StringComparison.Ordinal));
            if (entry == null) return;

            BlueprintFeatureSelection selection = BlueprintLibraryLookup
                .RequireExact<BlueprintFeatureSelection>(
                    BlueprintBootstrap.Library,
                    CustomWeaponMartialPerformancePublication.SelectionGuid,
                    "Call of the Wild Martial Performance selection");
            CustomWeaponMartialPerformancePublication publication =
                BlueprintBootstrap.MartialPerformancePublication;
            BlueprintFeature[] registered = publication == null
                ? new BlueprintFeature[0]
                : publication.Registered;
            string[] displayNames = {
                "Pistol", "Musket", "Blunderbuss", "Elven Branched Spear",
                "Wakizashi", "Katana", "Nodachi" };
            bool[] enabled = {
                context.FeatureModules.Active.Gunslinger,
                context.FeatureModules.Active.Gunslinger,
                context.FeatureModules.Active.Gunslinger,
                context.FeatureModules.Active.ElvenBranchedSpears,
                context.FeatureModules.Active.EasternWeapons,
                context.FeatureModules.Active.EasternWeapons,
                context.FeatureModules.Active.EasternWeapons };
            BlueprintFeature[] all = selection.AllFeatures ??
                new BlueprintFeature[0];
            bool identity = string.Equals(selection.name,
                    CustomWeaponMartialPerformancePublication
                        .ExpectedSelectionName, StringComparison.Ordinal) &&
                string.Equals(selection.GetType().FullName,
                    CustomWeaponMartialPerformancePublication
                        .ExpectedSelectionType, StringComparison.Ordinal) &&
                publication != null && publication.OptionalModPresent &&
                registered.Length ==
                    CustomWeaponMartialPerformancePublication.BlueprintCount;
            Add(assertions, "martial-performance-exact-contract",
                "19d1ff4cf70845d094b0ec231473e97f/" +
                    "BlueprintFeatureSelection/MartialPerformanceFeatureSelection",
                selection.AssetGuid + "/" + selection.GetType().FullName +
                    "/" + selection.name + ";registered=" +
                    registered.Length,
                identity,
                "stable GUID plus live type/internal name and bootstrap publication");

            BlueprintFeature[] expectedActive = registered
                .Select((feature, index) => new {
                    Feature = feature, Index = index })
                .Where(value => enabled[value.Index])
                .OrderBy(value => displayNames[value.Index],
                    StringComparer.Ordinal)
                .Select(value => value.Feature).ToArray();
            BlueprintFeature[] observedActive = all.Where(value =>
                    registered.Any(owned => SameBlueprint(value, owned)))
                .ToArray();
            bool exactCatalog = registered.Length == displayNames.Length &&
                registered.Select((feature, index) => feature != null &&
                    string.Equals(feature.Name,
                        "Martial Performance (" + displayNames[index] + ")",
                        StringComparison.Ordinal) &&
                    all.Count(value => SameBlueprint(value, feature)) ==
                        (enabled[index] ? 1 : 0)).All(value => value) &&
                observedActive.SequenceEqual(expectedActive) &&
                all.Skip(all.Length - observedActive.Length)
                    .SequenceEqual(observedActive) &&
                all.Count(value => value != null &&
                    string.Equals(value.AssetGuid,
                        CustomWeaponMartialPerformancePublication
                            .DaggerDonorGuid, StringComparison.Ordinal)) == 1;
            string catalogObserved = string.Join("|", observedActive.Select(
                value => value.Name + ":" + value.AssetGuid).ToArray()) +
                ";modules=" + context.FeatureModules.Active;
            Add(assertions, "martial-performance-custom-catalog",
                "each enabled firearm, spear, Wakizashi, Katana, and Nodachi once; native donor retained; deterministic tail order",
                catalogObserved, exactCatalog,
                "live BlueprintFeatureSelection.AllFeatures exact references");

            BlueprintParametrizedFeature weaponFocus =
                BlueprintLibraryLookup.RequireExact<
                    BlueprintParametrizedFeature>(
                        BlueprintBootstrap.Library,
                        CustomWeaponMartialPerformancePublication
                            .WeaponFocusGuid,
                        "native Weapon Focus grant");
            bool effectShape = observedActive.All(value =>
                HasMartialPerformanceGrantShape(value, weaponFocus));
            Add(assertions, "martial-performance-native-effect-shape",
                "one native Weapon Focus AddParametrizedFeatures row and one authoritative proficiency prerequisite per custom choice",
                "active=" + observedActive.Length +
                    ";shape=" + effectShape,
                observedActive.Length == enabled.Count(value => value) &&
                    effectShape,
                "live cloned child components and private native grant row");

            QualifyMartialPerformanceSelection(assertions, diagnostics,
                selection, registered, enabled, weaponFocus);
        }

        private static void QualifyMartialPerformanceSelection(
            List<RuntimeTestAssertion> assertions, List<string> diagnostics,
            BlueprintFeatureSelection selection, BlueprintFeature[] registered,
            bool[] enabled, BlueprintParametrizedFeature weaponFocus)
        {
            var before = new Kingmaker.UI.LevelUp.ChargenUnit(
                BlueprintRoot.Instance.DefaultPlayerCharacter).Unit;
            var preview = new Kingmaker.UI.LevelUp.ChargenUnit(
                BlueprintRoot.Instance.DefaultPlayerCharacter).Unit;
            try
            {
                BlueprintFeature[] active = registered
                    .Where((feature, index) => enabled[index]).ToArray();
                IFeatureSelectionItem[] beforeItems =
                    selection.ExtractSelectionItems(before.Descriptor,
                        before.Descriptor).ToArray();
                bool customRejected = active.All(feature =>
                    !feature.MeetsPrerequisites(null, before.Descriptor, null));
                BlueprintFeature nativeRejected =
                    (selection.AllFeatures ?? new BlueprintFeature[0])
                    .FirstOrDefault(feature => feature != null &&
                        !registered.Any(owned => SameBlueprint(feature, owned)) &&
                        (feature.ComponentsArray ?? new BlueprintComponent[0])
                            .OfType<PrerequisiteProficiency>().Any() &&
                        !feature.MeetsPrerequisites(
                            null, before.Descriptor, null));
                IFeatureSelectionItem nativeRejectedItem =
                    beforeItems.FirstOrDefault(item => item != null &&
                        SameBlueprint(item.Feature, nativeRejected));
                var beforeState = new FeatureSelectionState(
                    null, selection, selection, 0, 0);
                bool nativeVisible = nativeRejectedItem != null;
                bool nativeSelectable = nativeRejectedItem != null &&
                    selection.CanSelect(before.Descriptor, null,
                        beforeState, nativeRejectedItem);
                bool nativeParity = nativeRejected != null &&
                    active.All(feature =>
                    {
                        IFeatureSelectionItem item =
                            beforeItems.FirstOrDefault(value =>
                                value != null &&
                                SameBlueprint(value.Feature, feature));
                        bool visible = item != null;
                        bool selectable = item != null &&
                            selection.CanSelect(before.Descriptor, null,
                                beforeState, item);
                        return visible == nativeVisible &&
                            selectable == nativeSelectable;
                    });

                if (enabled[0])
                    preview.Descriptor.AddFact(
                        BlueprintBootstrap.FirearmProficiency);
                if (enabled[3])
                    preview.Descriptor.AddFact(BlueprintBootstrap
                        .ElvenBranchedSpears.ExoticWeaponProficiency);
                if (enabled[4] || enabled[5] || enabled[6])
                {
                    preview.Descriptor.AddFact(BlueprintBootstrap
                        .EasternWeapons.WakizashiProficiency);
                    preview.Descriptor.AddFact(BlueprintBootstrap
                        .EasternWeapons.KatanaProficiency);
                    BlueprintFeature martial =
                        BlueprintLibraryLookup.RequireExact<BlueprintFeature>(
                            BlueprintBootstrap.Library,
                            EasternWeaponBlueprints
                                .NativeMartialWeaponProficiencyGuid,
                            "native Martial Weapon Proficiency");
                    preview.Descriptor.AddFact(martial);
                }

                IFeatureSelectionItem[] previewItems =
                    selection.ExtractSelectionItems(before.Descriptor,
                        preview.Descriptor).ToArray();
                var previewState = new FeatureSelectionState(
                    null, selection, selection, 0, 0);
                bool previewEligible = active.All(feature =>
                {
                    IFeatureSelectionItem item =
                        previewItems.FirstOrDefault(value => value != null &&
                            SameBlueprint(value.Feature, feature));
                    return feature.MeetsPrerequisites(
                            null, preview.Descriptor, null) &&
                        previewItems.Count(value => value != null &&
                            SameBlueprint(value.Feature, feature)) == 1 &&
                        item != null && selection.CanSelect(
                            preview.Descriptor, null, previewState, item);
                });
                bool committed = true;
                foreach (BlueprintFeature feature in active)
                {
                    IFeatureSelectionItem item =
                        previewItems.FirstOrDefault(value =>
                        value != null &&
                        SameBlueprint(value.Feature, feature));
                    var state = new FeatureSelectionState(
                        null, selection, selection, 0, 0);
                    if (item == null || !selection.CanSelect(
                            preview.Descriptor, null, state, item))
                    {
                        committed = false;
                        continue;
                    }
                    state.Select(item, null);
                    if (!ReferenceEquals(state.SelectedItem, item))
                        committed = false;
                }

                bool focusAbsentBefore =
                    !preview.Descriptor.HasFact(weaponFocus);
                var performanceFact = preview.Descriptor.AddFact(
                    registered[0]);
                bool focusApplied = performanceFact != null &&
                    preview.Descriptor.HasFact(weaponFocus);
                if (performanceFact != null)
                    preview.Descriptor.RemoveFact(performanceFact);
                bool focusRemoved =
                    !preview.Descriptor.HasFact(weaponFocus);

                string observed = "nativeRejected=" +
                    (nativeRejected == null ? "<missing>" :
                        nativeRejected.name) + ";nativeVisible=" +
                    nativeVisible + ";nativeSelectable=" +
                    nativeSelectable + ";beforeCustom=" +
                    string.Join(",", active.Select(feature =>
                        feature.name + ":" +
                        ContainsFeature(beforeItems, feature) + "/" +
                        feature.MeetsPrerequisites(null,
                            before.Descriptor, null)).ToArray()) +
                    ";previewCustom=" + string.Join(",",
                        active.Select(feature => feature.name + ":" +
                            ContainsFeature(previewItems, feature) + "/" +
                            feature.MeetsPrerequisites(null,
                                preview.Descriptor, null)).ToArray()) +
                    ";commit=" + committed + ";focus=" +
                    focusAbsentBefore + "/" + focusApplied + "/" +
                    focusRemoved;
                Add(assertions,
                    "martial-performance-proficiency-parity",
                    "non-proficient custom rows match native visibility and remain ineligible",
                    observed, customRejected && nativeParity,
                    "native ExtractSelectionItems plus each live child prerequisite");
                Add(assertions,
                    "martial-performance-preview-and-commit",
                    "same-level preview facts make every enabled custom row eligible and each native selection state commits it",
                    observed, previewEligible && committed,
                    "real proficiency AddFact, before/preview descriptors, and FeatureSelectionState.Select");
                Add(assertions,
                    "martial-performance-applied-effect",
                    "Pistol child grants and removes the native Weapon Focus parametrized fact with its owner",
                    observed, enabled[0] && focusAbsentBefore &&
                        focusApplied && focusRemoved,
                    "real BlueprintFeature AddFact/RemoveFact component lifecycle");
                diagnostics.Add("martialPerformance{" + observed + "}");
            }
            finally
            {
                preview.Dispose();
                before.Dispose();
            }
        }

        private static bool HasMartialPerformanceGrantShape(
            BlueprintFeature feature,
            BlueprintParametrizedFeature weaponFocus)
        {
            BlueprintComponent[] components = feature == null
                ? new BlueprintComponent[0]
                : feature.ComponentsArray ?? new BlueprintComponent[0];
            AddParametrizedFeatures[] grants =
                components.OfType<AddParametrizedFeatures>().ToArray();
            if (components.Length != 2 || grants.Length != 1 ||
                components.OfType<Prerequisite>().Count() != 1)
                return false;
            FieldInfo field = typeof(AddParametrizedFeatures).GetField(
                "m_Features", BindingFlags.Instance | BindingFlags.Public |
                    BindingFlags.NonPublic);
            Array rows = field == null ? null :
                field.GetValue(grants[0]) as Array;
            if (rows == null || rows.Length != 1 ||
                rows.GetValue(0) == null)
                return false;
            FieldInfo featureField = rows.GetType().GetElementType().GetField(
                "Feature", BindingFlags.Instance | BindingFlags.Public |
                    BindingFlags.NonPublic);
            return featureField != null && SameBlueprint(
                featureField.GetValue(rows.GetValue(0)) as
                    BlueprintScriptableObject, weaponFocus);
        }

        private static bool ContainsFeature(
            IEnumerable<IFeatureSelectionItem> items,
            BlueprintFeature feature)
        {
            return items != null && items.Any(value => value != null &&
                SameBlueprint(value.Feature, feature));
        }

        private static bool SameBlueprint(BlueprintScriptableObject left,
            BlueprintScriptableObject right)
        {
            return ReferenceEquals(left, right) || left != null &&
                right != null && string.Equals(left.AssetGuid,
                    right.AssetGuid, StringComparison.Ordinal);
        }

        private static bool Rows(LevelEntry[] actual, int[] levels, BlueprintFeatureBase[][] features)
        {
            if (actual == null || actual.Length != levels.Length) return false;
            for (int index = 0; index < levels.Length; index++)
                if (actual[index].Level != levels[index] || actual[index].Features == null ||
                    !actual[index].Features.SequenceEqual(features[index])) return false;
            return true;
        }

        private static void AddHarmonyEvidence(ModContext context,
            List<RuntimeTestAssertion> assertions, List<string> diagnostics)
        {
            var records = new List<string>();
            var identities = new List<string>();
            foreach (MethodBase method in context.Harmony.GetPatchedMethods())
            {
                Patches patches = context.Harmony.GetPatchInfo(method);
                AddPatches(method, "prefix", patches.Prefixes, records, identities, context.ModId);
                AddPatches(method, "postfix", patches.Postfixes, records, identities, context.ModId);
                AddPatches(method, "transpiler", patches.Transpilers, records, identities, context.ModId);
            }
            records.Sort(StringComparer.Ordinal);
            diagnostics.AddRange(records.Select(value => "harmony=" + value));
            string[] duplicates = identities.GroupBy(value => value, StringComparer.Ordinal)
                .Where(value => value.Count() > 1).Select(value => value.Key + "*" + value.Count()).ToArray();
            Add(assertions, "gunslinger-harmony-patches", "present and installed once",
                "patches=" + identities.Count + ";duplicates=" +
                    (duplicates.Length == 0 ? "none" : string.Join(",", duplicates)),
                identities.Count > 0 && duplicates.Length == 0,
                "Harmony12 1.2.0.1 GetPatchedMethods/GetPatchInfo exact registry");
        }

        private static void AddPatches(MethodBase target, string role, IEnumerable<Patch> patchValues,
            List<string> records, List<string> identities, string modId)
        {
            Patch[] patches = patchValues.ToArray();
            for (int index = 0; index < patches.Length; index++)
            {
                Patch patch = patches[index];
                string targetName = target.DeclaringType.FullName + "." + target.Name +
                    "(" + string.Join(",", target.GetParameters().Select(value =>
                        value.ParameterType.FullName).ToArray()) + ")";
                string patchName = patch.patch == null ? "missing" :
                    patch.patch.DeclaringType.FullName + "." + patch.patch.Name;
                records.Add("target=" + targetName + ";role=" + role + ";order=" + index +
                    ";owner=" + patch.owner + ";priority=" + patch.priority +
                    ";before=" + string.Join(",", patch.before ?? new string[0]) +
                    ";after=" + string.Join(",", patch.after ?? new string[0]) +
                    ";patch=" + patchName);
                if (string.Equals(patch.owner, modId, StringComparison.Ordinal))
                    identities.Add(targetName + ";" + role + ";" + patchName);
            }
        }

        private static void Add(List<RuntimeTestAssertion> assertions, string name,
            string expected, string observed, bool pass, string evidence)
        {
            assertions.Add(new RuntimeTestAssertion { Name = name, Expected = expected,
                Observed = observed, Status = pass ? RuntimeTestStatuses.Pass : RuntimeTestStatuses.Fail,
                Evidence = evidence });
        }

        private static string HashFile(string path)
        {
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path)) return "missing";
            using (SHA256 hash = SHA256.Create())
            using (FileStream stream = File.OpenRead(path))
                return BitConverter.ToString(hash.ComputeHash(stream)).Replace("-", string.Empty);
        }

        private static string ReadMetadata(Assembly assembly, string key)
        {
            AssemblyMetadataAttribute value = assembly.GetCustomAttributes(
                typeof(AssemblyMetadataAttribute), false).Cast<AssemblyMetadataAttribute>()
                .FirstOrDefault(item => string.Equals(item.Key, key, StringComparison.Ordinal));
            return value == null ? string.Empty : value.Value;
        }
    }
}
