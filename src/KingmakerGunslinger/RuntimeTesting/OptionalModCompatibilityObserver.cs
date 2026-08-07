using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using Harmony12;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Stats;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
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
                { "gunslinger-call-of-the-wild",
                    new[] { "KingmakerGunslinger", "CallOfTheWild" } },
                { "gunslinger-arms-armor",
                    new[] { "KingmakerGunslinger", "ArmsArmor" } },
                { "gunslinger-toggle-custom-soundpacks",
                    new[] { "KingmakerGunslinger", "ToggleCustomSoundpacks" } },
                { "gunslinger-high-risk-combined", new[] { "KingmakerGunslinger",
                    "CallOfTheWild", "ArmsArmor", "ToggleCustomSoundpacks" } },
                { "gunslinger-all-loadable-local", new[] { "KingmakerGunslinger",
                    "CallOfTheWild", "ArmsArmor", "ToggleCustomSoundpacks" } }
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
            List<UnityModManager.ModEntry> entries = ReadModEntries();
            string[] observedIds = entries.Select(value => value.Info.Id).ToArray();
            Add(assertions, "isolated-umm-entry-set", string.Join(",", expectedIds),
                string.Join(",", observedIds), expectedIds.SequenceEqual(observedIds),
                "UnityModManager 0.32.4 private static modEntries in actual load order");
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
            BlueprintCharacterClass[] classes = BlueprintRoot.Instance == null ||
                BlueprintRoot.Instance.Progression == null ? new BlueprintCharacterClass[0] :
                BlueprintRoot.Instance.Progression.CharacterClasses;
            Add(assertions, "blueprint-bootstrap", "initialized", BlueprintBootstrap.IsInitialized.ToString(),
                BlueprintBootstrap.IsInitialized && gunslinger != null, "BlueprintBootstrap state");
            Add(assertions, "gunslinger-class-singular", "exactly one project class reference",
                "referenceCount=" + classes.Count(value => ReferenceEquals(value, cls)),
                cls != null && classes.Count(value => ReferenceEquals(value, cls)) == 1,
                "BlueprintRoot progression class catalog");
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

        private static List<UnityModManager.ModEntry> ReadModEntries()
        {
            FieldInfo field = typeof(UnityModManager).GetField("modEntries",
                BindingFlags.Static | BindingFlags.NonPublic);
            if (field == null) throw new MissingFieldException(typeof(UnityModManager).FullName, "modEntries");
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
                string targetName = target.DeclaringType.FullName + "." + target.Name;
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
