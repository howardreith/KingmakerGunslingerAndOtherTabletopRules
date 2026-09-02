using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Root;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.ElementalRaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using UnityEngine;
using UnityModManagerNet;

namespace KingmakerGunslinger.RuntimeTesting
{
    /// <summary>
    /// Save-free final-catalog proof for KMG and Races Unleashed coexistence.
    /// The observer never invokes third-party publication code.
    /// </summary>
    internal static class ElementalRaceCompatibilityScenario
    {
        internal const string EvidenceFileName =
            "elemental-races-races-unleashed-compatibility.json";

        private const string RacesUnleashedId = "RacesUnleashed";
        private const string RacesUnleashedVersion = "1.0.11";
        private const string RacesUnleashedMvid =
            "e9b9acb5-9b3f-41ad-bbd7-74494d5d7680";
        private const string RacesUnleashedSha256 =
            "6d18168cb90ffe60931addc8ee11e42b3ef647ef0e6d4b7ce8980d44659f4cb0";

        private static readonly string[] NativeRaceGuids =
        {
            "0a5d473ead98b0646b94495af250fdc4",
            "b7f02ba92b363064fb873963bec275ee",
            "5c4e42124dc2b4647af6e36cf2590500",
            "25a5878d125338244896ebd3238226c8",
            "c4faf439f0e70bd40b5e36ee80d06be7",
            "b3646842ffbd01643ab4dac7479b20b0",
            "1dc20e195581a804890ddc74218bfd8e",
            "ef35a22c9a27da345a4528f0d5889157"
        };

        private static readonly string[] RacesUnleashedRaceGuids =
        {
            "d1335380a70e4bd7aa535f36770b93de",
            "c515d06d35d048e79801d07039338cda",
            "cd40ff5a556bcf3419bf7479616cd2ad",
            "a68578b3a2a945a5b8561ec51a0dff5c",
            "3cfdcda8edd74212a58d3b0d9d4041a4",
            "970bb406a3ac42d795a3ef1b5900fdf3",
            "f78db38a553f4f91a10a8e68c91019ad"
        };

        private sealed class CatalogEntryEvidence
        {
            public int Index { get; set; }
            public string Guid { get; set; }
            public string InternalName { get; set; }
            public string DisplayName { get; set; }
            public bool ProjectOwned { get; set; }
            public bool RacesUnleashedOwned { get; set; }
        }

        private sealed class Evidence
        {
            public int SchemaVersion { get; set; }
            public bool SaveStateTouched { get; set; }
            public bool RacesUnleashedLoaded { get; set; }
            public string RacesUnleashedIdentity { get; set; }
            public int CatalogCount { get; set; }
            public List<CatalogEntryEvidence> Catalog { get; set; }
            public List<int> ProjectIndexes { get; set; }
            public List<int> RacesUnleashedIndexes { get; set; }
            public bool FirstReconciliationChanged { get; set; }
            public bool SecondReconciliationChanged { get; set; }
            public bool ArrayReferencePreserved { get; set; }
            public bool EntryReferencesPreserved { get; set; }
        }

        internal static RuntimeTestResult Run(ModContext context,
            RuntimeTestRequest request)
        {
            string startUtc = DateTime.UtcNow.ToString("o");
            var assertions = new List<RuntimeTestAssertion>();
            var diagnostics = new List<string>();
            var evidenceFiles = new List<string>();
            var evidence = new Evidence
            {
                SchemaVersion = 1,
                SaveStateTouched = false,
                Catalog = new List<CatalogEntryEvidence>(),
                ProjectIndexes = new List<int>(),
                RacesUnleashedIndexes = new List<int>()
            };
            try
            {
                Exercise(context, assertions, diagnostics, evidence);
            }
            catch (Exception exception)
            {
                diagnostics.Add("stage=exercise;exception=" + exception);
            }

            Add(assertions, "elemental-races-compatibility-save-free",
                "no save selection/load/write API invoked",
                "saveStateTouched=" + evidence.SaveStateTouched,
                !evidence.SaveStateTouched,
                "read-only inspection and idempotent no-op KMG reconciliation");
            string path = Path.Combine(request.EvidenceDirectory,
                EvidenceFileName);
            File.WriteAllText(path, JsonConvert.SerializeObject(evidence,
                Formatting.Indented, new JsonSerializerSettings
                {
                    ContractResolver = new DefaultContractResolver(),
                    PreserveReferencesHandling =
                        PreserveReferencesHandling.None,
                    ReferenceLoopHandling = ReferenceLoopHandling.Error
                }));
            evidenceFiles.Add(path);
            diagnostics.Add("elementalRacesCompatibilitySha256=" +
                HashFile(path));
            bool pass = assertions.Count > 1 && assertions.All(value =>
                value.Status == RuntimeTestStatuses.Pass);
            Assembly assembly = context.Assembly;
            return new RuntimeTestResult
            {
                SchemaVersion = 1, RunId = request.RunId,
                Scenario = request.Scenario,
                Status = pass ? RuntimeTestStatuses.Pass :
                    RuntimeTestStatuses.Fail,
                LoadedModVersion = context.ModEntry.Info.Version,
                RuntimeIdentity = assembly.FullName + ";mvid=" +
                    assembly.ManifestModule.ModuleVersionId + ";sha256=" +
                    HashFile(assembly.Location) + ";pid=" +
                    Process.GetCurrentProcess().Id,
                GitCommit = Metadata(assembly, "GitCommit"),
                GameVersion = Application.version ?? string.Empty,
                StartUtc = startUtc, EndUtc = string.Empty,
                Assertions = assertions, Diagnostics = diagnostics,
                Warnings = new List<string>(),
                ExceptionSummary = string.Empty,
                EvidenceFiles = evidenceFiles,
                AutomaticExitRequested = request.ExitAfterCompletion,
                EvidenceDirectory = request.EvidenceDirectory
            };
        }

        private static void Exercise(ModContext context,
            ICollection<RuntimeTestAssertion> assertions,
            ICollection<string> diagnostics, Evidence evidence)
        {
            ElementalRaceBlueprintSet set = BlueprintBootstrap.ElementalRaces;
            bool bootstrap = BlueprintBootstrap.IsInitialized &&
                BlueprintBootstrap.Library != null && set != null &&
                set.Count == ElementalRaceIdentityCatalog.IdentityCount;
            Add(assertions, "elemental-races-bootstrap",
                "all stable elemental identities registered at catalog count",
                set == null ? "missing" : "count=" + set.Count,
                bootstrap, "live BlueprintBootstrap.ElementalRaces");
            Add(assertions, "elemental-races-module-active",
                "Elemental Races enabled for compatibility publication",
                context.FeatureModules.Active.ElementalRaces.ToString(),
                context.FeatureModules.Active.ElementalRaces,
                "restart-bound live FeatureModuleConfiguration");
            if (!bootstrap)
                throw new InvalidOperationException(
                    "The production elemental race blueprint set is unavailable.");

            BlueprintRoot root = BlueprintRoot.Instance;
            if (root == null || root.Progression == null ||
                root.Progression.CharacterRaces == null)
                throw new InvalidOperationException(
                    "The live CharacterRaces catalog is unavailable.");
            BlueprintRace[] catalogReference =
                root.Progression.CharacterRaces;
            BlueprintRace[] before =
                (BlueprintRace[])catalogReference.Clone();
            BlueprintRace[] project = set.OrderedRaces();
            List<UnityModManager.ModEntry> entries =
                ReadModEntries(context.ModEntry);
            UnityModManager.ModEntry[] ruEntries = entries.Where(value =>
                value.Info != null && string.Equals(value.Info.Id,
                    RacesUnleashedId, StringComparison.Ordinal)).ToArray();
            UnityModManager.ModEntry ru = ruEntries.Length == 1
                ? ruEntries[0] : null;
            evidence.RacesUnleashedLoaded = ru != null && ru.Loaded &&
                !ru.ErrorOnLoading && ru.HasAssembly;
            evidence.RacesUnleashedIdentity = Describe(ru);
            evidence.CatalogCount = before.Length;
            for (int index = 0; index < before.Length; index++)
            {
                BlueprintRace race = before[index];
                string guid = race == null ? string.Empty : race.AssetGuid;
                evidence.Catalog.Add(new CatalogEntryEvidence
                {
                    Index = index,
                    Guid = guid,
                    InternalName = race == null ? string.Empty :
                        race.name ?? string.Empty,
                    DisplayName = SafeName(race),
                    ProjectOwned = project.Any(value =>
                        ReferenceEquals(value, race)),
                    RacesUnleashedOwned =
                        RacesUnleashedRaceGuids.Contains(guid,
                            StringComparer.Ordinal)
                });
            }
            evidence.ProjectIndexes.AddRange(Indexes(before,
                project.Select(value => value.AssetGuid)));
            evidence.RacesUnleashedIndexes.AddRange(Indexes(before,
                RacesUnleashedRaceGuids));

            bool validCatalog = before.Length > 0 &&
                before.All(value => value != null &&
                    !string.IsNullOrWhiteSpace(value.AssetGuid)) &&
                before.Select(value => value.AssetGuid).Distinct(
                    StringComparer.Ordinal).Count() == before.Length &&
                HasUniqueReferences(before);
            Add(assertions, "shared-race-catalog-singular",
                "non-null entries with unique GUIDs and object references",
                "count=" + before.Length,
                validCatalog,
                "complete final BlueprintRoot.Progression.CharacterRaces snapshot");
            diagnostics.Add("catalog=" + string.Join("|",
                evidence.Catalog.Select(value => value.Index + ":" +
                    value.InternalName + "[" + value.Guid + "]").ToArray()));

            bool nativeExact = NativeRaceGuids.All(guid =>
                before.Count(value => value != null && string.Equals(
                    value.AssetGuid, guid, StringComparison.Ordinal)) == 1);
            Add(assertions, "native-races-preserved",
                "all eight audited native race GUIDs exactly once",
                "matches=" + NativeRaceGuids.Count(guid => before.Any(
                    value => value != null && string.Equals(
                        value.AssetGuid, guid,
                        StringComparison.Ordinal))) + "/8",
                nativeExact,
                "full CharacterRaces snapshot by stable native identity");

            bool projectExact = evidence.ProjectIndexes.Count == 4 &&
                evidence.ProjectIndexes.Select((value, index) =>
                    value == evidence.ProjectIndexes[0] + index).All(
                        value => value) &&
                project.Select((race, index) =>
                    before.Count(value => ReferenceEquals(value, race)) == 1 &&
                    before.Count(value => value != null && string.Equals(
                        value.AssetGuid, race.AssetGuid,
                        StringComparison.Ordinal)) == 1 &&
                    ReferenceEquals(before[evidence.ProjectIndexes[index]],
                        race)).All(value => value);
            Add(assertions, "elemental-races-published-once",
                "Ifrit, Oread, Sylph, Undine contiguous in that order",
                "indexes=" + string.Join(",",
                    evidence.ProjectIndexes.Select(value =>
                        value.ToString()).ToArray()),
                projectExact,
                "exact project references and stable GUIDs in final CharacterRaces");

            bool ruUmmExact = ruEntries.Length <= 1 &&
                (ru == null || evidence.RacesUnleashedLoaded &&
                    string.Equals(ru.Info.Version,
                        RacesUnleashedVersion, StringComparison.Ordinal) &&
                    string.Equals(ru.Assembly.GetName().Name,
                        RacesUnleashedId, StringComparison.Ordinal) &&
                    string.Equals(ru.Assembly.ManifestModule.ModuleVersionId
                        .ToString(), RacesUnleashedMvid,
                        StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(HashFile(ru.Assembly.Location),
                        RacesUnleashedSha256,
                        StringComparison.OrdinalIgnoreCase));
            Add(assertions, "races-unleashed-exact-runtime-identity",
                "absent negative control or exact RacesUnleashed 1.0.11 authority",
                evidence.RacesUnleashedIdentity,
                ruUmmExact,
                "live UMM ModEntry ID plus assembly name, MVID, and SHA-256");

            bool ruCatalogExact = RacesUnleashedCatalogExact(
                BlueprintBootstrap.Library, before,
                evidence.RacesUnleashedLoaded);
            Add(assertions, "races-unleashed-races-preserved",
                evidence.RacesUnleashedLoaded
                    ? "seven exact owned BlueprintRace references each once"
                    : "no Races Unleashed-owned GUID in negative control",
                "loaded=" + evidence.RacesUnleashedLoaded + ";indexes=" +
                    string.Join(",", evidence.RacesUnleashedIndexes.Select(
                        value => value.ToString()).ToArray()),
                ruCatalogExact,
                "six manifest identities plus the live constructed Duergar identity from the authorized local assembly");

            ElementalRacePublication first =
                ElementalRacePublication.Apply(set, true);
            BlueprintRace[] afterFirst = root.Progression.CharacterRaces;
            ElementalRacePublication second =
                ElementalRacePublication.Apply(set, true);
            BlueprintRace[] afterSecond = root.Progression.CharacterRaces;
            evidence.FirstReconciliationChanged = first.Changed;
            evidence.SecondReconciliationChanged = second.Changed;
            evidence.ArrayReferencePreserved =
                ReferenceEquals(catalogReference, afterFirst) &&
                ReferenceEquals(catalogReference, afterSecond);
            evidence.EntryReferencesPreserved =
                SameReferences(before, afterFirst) &&
                SameReferences(before, afterSecond);
            Add(assertions, "elemental-race-reconciliation-idempotent",
                "two no-op observations preserve exact array and entry references",
                "changed=" + first.Changed + "/" + second.Changed +
                    ";array=" + evidence.ArrayReferencePreserved +
                    ";entries=" + evidence.EntryReferencesPreserved,
                !first.Changed && !second.Changed &&
                    evidence.ArrayReferencePreserved &&
                    evidence.EntryReferencesPreserved,
                "two ElementalRacePublication.Apply transactions against the complete final shared catalog");
            Add(assertions, "third-party-race-order-preserved",
                "complete pre-observation reference sequence unchanged",
                "count=" + before.Length + ";preserved=" +
                    evidence.EntryReferencesPreserved,
                evidence.EntryReferencesPreserved,
                "full native, Races Unleashed, and other third-party entry snapshot");
        }

        private static bool RacesUnleashedCatalogExact(
            LibraryScriptableObject library, BlueprintRace[] catalog,
            bool loaded)
        {
            if (library == null || library.BlueprintsByAssetId == null)
                return false;
            if (!loaded)
                return RacesUnleashedRaceGuids.All(guid =>
                    catalog.All(value => value == null || !string.Equals(
                        value.AssetGuid, guid,
                        StringComparison.Ordinal)));
            foreach (string guid in RacesUnleashedRaceGuids)
            {
                BlueprintScriptableObject value;
                if (!library.BlueprintsByAssetId.TryGetValue(guid,
                        out value))
                    return false;
                BlueprintRace race = value as BlueprintRace;
                if (race == null ||
                    catalog.Count(candidate =>
                        ReferenceEquals(candidate, race)) != 1 ||
                    catalog.Count(candidate => candidate != null &&
                        string.Equals(candidate.AssetGuid, guid,
                            StringComparison.Ordinal)) != 1)
                    return false;
            }
            return true;
        }

        private static List<UnityModManager.ModEntry> ReadModEntries(
            UnityModManager.ModEntry currentEntry)
        {
            Type managerType = currentEntry == null ? null :
                currentEntry.GetType().DeclaringType;
            if (managerType == null)
                throw new InvalidOperationException(
                    "The live UMM ModEntry declaring type was unavailable.");
            FieldInfo field = managerType.GetField("modEntries",
                BindingFlags.Static | BindingFlags.Public |
                    BindingFlags.NonPublic);
            if (field == null)
                throw new MissingFieldException(
                    managerType.AssemblyQualifiedName, "modEntries");
            IEnumerable values = field.GetValue(null) as IEnumerable;
            if (values == null)
                throw new InvalidOperationException(
                    "UMM modEntries was unavailable.");
            return values.Cast<object>().Select(value =>
                    value as UnityModManager.ModEntry)
                .Where(value => value != null).ToList();
        }

        private static string Describe(UnityModManager.ModEntry entry)
        {
            if (entry == null) return "absent";
            Assembly assembly = entry.Assembly;
            return "id=" + entry.Info.Id + ";version=" +
                entry.Info.Version + ";loaded=" + entry.Loaded +
                ";error=" + entry.ErrorOnLoading + ";assembly=" +
                (assembly == null ? "missing" :
                    assembly.GetName().Name) + ";mvid=" +
                (assembly == null ? "missing" :
                    assembly.ManifestModule.ModuleVersionId.ToString()) +
                ";sha256=" + (assembly == null ? "missing" :
                    HashFile(assembly.Location));
        }

        private static IEnumerable<int> Indexes(BlueprintRace[] catalog,
            IEnumerable<string> guids)
        {
            var expected = new HashSet<string>(guids,
                StringComparer.Ordinal);
            for (int index = 0; index < catalog.Length; index++)
                if (catalog[index] != null &&
                    expected.Contains(catalog[index].AssetGuid))
                    yield return index;
        }

        private static bool HasUniqueReferences(BlueprintRace[] values)
        {
            for (int left = 0; left < values.Length; left++)
                for (int right = left + 1; right < values.Length; right++)
                    if (ReferenceEquals(values[left], values[right]))
                        return false;
            return true;
        }

        private static bool SameReferences(IList<BlueprintRace> expected,
            IList<BlueprintRace> actual)
        {
            if (expected == null || actual == null ||
                expected.Count != actual.Count) return false;
            for (int index = 0; index < expected.Count; index++)
                if (!ReferenceEquals(expected[index], actual[index]))
                    return false;
            return true;
        }

        private static string SafeName(BlueprintRace race)
        {
            if (race == null) return string.Empty;
            try { return race.Name ?? race.name ?? string.Empty; }
            catch { return race.name ?? string.Empty; }
        }

        private static void Add(
            ICollection<RuntimeTestAssertion> assertions, string name,
            string expected, string observed, bool pass, string source)
        {
            assertions.Add(new RuntimeTestAssertion
            {
                Name = name,
                Expected = expected,
                Observed = observed,
                Status = pass ? RuntimeTestStatuses.Pass :
                    RuntimeTestStatuses.Fail,
                Evidence = source
            });
        }

        private static string HashFile(string path)
        {
            using (SHA256 sha = SHA256.Create())
            using (FileStream stream = File.OpenRead(path))
                return BitConverter.ToString(sha.ComputeHash(stream))
                    .Replace("-", string.Empty).ToLowerInvariant();
        }

        private static string Metadata(Assembly assembly, string key)
        {
            AssemblyMetadataAttribute value = assembly.GetCustomAttributes(
                typeof(AssemblyMetadataAttribute), false)
                .Cast<AssemblyMetadataAttribute>().FirstOrDefault(item =>
                    string.Equals(item.Key, key,
                        StringComparison.Ordinal));
            return value == null ? string.Empty : value.Value;
        }
    }
}
