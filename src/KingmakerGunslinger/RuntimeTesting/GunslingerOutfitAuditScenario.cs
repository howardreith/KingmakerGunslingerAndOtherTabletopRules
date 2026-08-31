using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.Blueprints.Root;
using Kingmaker.ResourceLinks;
using Kingmaker.Visual.CharacterSystem;
using KingmakerGunslinger.Bootstrap;
using Newtonsoft.Json;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    /// <summary>
    /// Guarded, save-free native clothing inventory. Its request-local catalog
    /// is discovery evidence, never aesthetic proof or production selection.
    /// </summary>
    internal static class GunslingerOutfitAuditScenario
    {
        internal const string EvidenceFileName =
            "gunslinger-outfit-catalog.json";
        private const string ExpectedGameVersion = "2.1.7b";
        private const string ExpectedAssemblySha256 =
            "3b6450ffec440e296e586f71c711b195aed144b28d53e1cbb29406d18fef5afb";
        private const string ExpectedAssemblyMvid =
            "07fa1e4d-8618-41b3-9b8d-faa17d3b26f7";

        private static readonly string[] RawCandidateTerms =
        {
            "leather", "coat", "cape", "cloak", "scarf", "sash",
            "belt", "pouch", "bracer", "glove", "boot", "gambeson",
            "padded", "duelist", "aldori", "bard", "rogue", "ranger",
            "alchemist", "guard", "officer", "noble", "pirate",
            "bandit", "explorer", "traveler", "merchant"
        };

        private static readonly string[] BakedWeaponTerms =
        {
            "weapon", "sword", "scabbard", "shield", "bow", "dagger",
            "mace", "staff", "axe", "spear", "gun", "pistol", "rifle",
            "musket"
        };

        private sealed class AuditEvidence
        {
            [JsonProperty("schemaVersion", Order = 1)]
            public int SchemaVersion { get; set; }
            [JsonProperty("gameVersion", Order = 2)]
            public string GameVersion { get; set; }
            [JsonProperty("unityApplicationVersion", Order = 3)]
            public string UnityApplicationVersion { get; set; }
            [JsonProperty("gameAssemblySha256", Order = 4)]
            public string GameAssemblySha256 { get; set; }
            [JsonProperty("gameAssemblyMvid", Order = 5)]
            public string GameAssemblyMvid { get; set; }
            [JsonProperty("candidateSetId", Order = 6)]
            public string CandidateSetId { get; set; }
            [JsonProperty("classSources", Order = 7)]
            public List<OwnerEvidence> ClassSources { get; set; }
            [JsonProperty("itemSources", Order = 8)]
            public List<OwnerEvidence> ItemSources { get; set; }
            [JsonProperty("rawSources", Order = 9)]
            public List<RawSourceEvidence> RawSources { get; set; }
            [JsonProperty("entities", Order = 10)]
            public List<EntityEvidence> Entities { get; set; }
            [JsonProperty("supportedRaces", Order = 11)]
            public List<RaceEvidence> SupportedRaces { get; set; }
            [JsonProperty("totals", Order = 12)]
            public TotalsEvidence Totals { get; set; }
            [JsonProperty("errors", Order = 13)]
            public List<string> Errors { get; set; }
            [JsonProperty("saveStateTouched", Order = 14)]
            public bool SaveStateTouched { get; set; }
        }

        private sealed class RaceEvidence
        {
            [JsonProperty("guid", Order = 1)] public string Guid { get; set; }
            [JsonProperty("name", Order = 2)] public string Name { get; set; }
            [JsonProperty("raceId", Order = 3)] public string RaceId { get; set; }
        }

        private sealed class OwnerEvidence
        {
            [JsonProperty("sourceClassification", Order = 1)]
            public string SourceClassification { get; set; }
            [JsonProperty("ownerGuid", Order = 2)]
            public string OwnerGuid { get; set; }
            [JsonProperty("ownerName", Order = 3)]
            public string OwnerName { get; set; }
            [JsonProperty("displayName", Order = 4)]
            public string DisplayName { get; set; }
            [JsonProperty("dlcType", Order = 5)]
            public string DlcType { get; set; }
            [JsonProperty("primaryColor", Order = 6)]
            public int? PrimaryColor { get; set; }
            [JsonProperty("secondaryColor", Order = 7)]
            public int? SecondaryColor { get; set; }
            [JsonProperty("wrapperGuids", Order = 8)]
            public List<string> WrapperGuids { get; set; }
            [JsonProperty("itemOwners", Order = 9)]
            public List<string> ItemOwners { get; set; }
            [JsonProperty("matrix", Order = 10)]
            public List<MatrixEvidence> Matrix { get; set; }
        }

        private sealed class MatrixEvidence
        {
            [JsonProperty("gender", Order = 1)]
            public string Gender { get; set; }
            [JsonProperty("race", Order = 2)]
            public string Race { get; set; }
            [JsonProperty("raceId", Order = 3)]
            public string RaceId { get; set; }
            [JsonProperty("links", Order = 4)]
            public List<LinkEvidence> Links { get; set; }
        }

        private sealed class LinkEvidence
        {
            [JsonProperty("order", Order = 1)] public int Order { get; set; }
            [JsonProperty("assetId", Order = 2)]
            public string AssetId { get; set; }
            [JsonProperty("resourceName", Order = 3)]
            public string ResourceName { get; set; }
            [JsonProperty("origins", Order = 4)]
            public List<string> Origins { get; set; }
            [JsonProperty("loaded", Order = 5)] public bool Loaded { get; set; }
            [JsonProperty("loadError", Order = 6)]
            public string LoadError { get; set; }
        }

        private sealed class RawSourceEvidence
        {
            [JsonProperty("sourceClassification", Order = 1)]
            public string SourceClassification { get; set; }
            [JsonProperty("assetId", Order = 2)]
            public string AssetId { get; set; }
            [JsonProperty("resourceName", Order = 3)]
            public string ResourceName { get; set; }
            [JsonProperty("loaded", Order = 4)] public bool Loaded { get; set; }
            [JsonProperty("loadError", Order = 5)]
            public string LoadError { get; set; }
        }

        private sealed class EntityEvidence
        {
            [JsonProperty("assetId", Order = 1)]
            public string AssetId { get; set; }
            [JsonProperty("resourceName", Order = 2)]
            public string ResourceName { get; set; }
            [JsonProperty("entityName", Order = 3)]
            public string EntityName { get; set; }
            [JsonProperty("layer", Order = 4)] public int Layer { get; set; }
            [JsonProperty("hideBodyParts", Order = 5)]
            public string HideBodyParts { get; set; }
            [JsonProperty("showLowerMaterials", Order = 6)]
            public bool ShowLowerMaterials { get; set; }
            [JsonProperty("colorsProfile", Order = 7)]
            public string ColorsProfile { get; set; }
            [JsonProperty("primaryRampCount", Order = 8)]
            public int PrimaryRampCount { get; set; }
            [JsonProperty("secondaryRampCount", Order = 9)]
            public int SecondaryRampCount { get; set; }
            [JsonProperty("bodyParts", Order = 10)]
            public List<PartEvidence> BodyParts { get; set; }
            [JsonProperty("outfitParts", Order = 11)]
            public List<PartEvidence> OutfitParts { get; set; }
            [JsonProperty("structuralRisks", Order = 12)]
            public List<string> StructuralRisks { get; set; }
        }

        private sealed class PartEvidence
        {
            [JsonProperty("index", Order = 1)] public int Index { get; set; }
            [JsonProperty("type", Order = 2)] public string Type { get; set; }
            [JsonProperty("prefab", Order = 3)]
            public string Prefab { get; set; }
            [JsonProperty("material", Order = 4)]
            public string Material { get; set; }
            [JsonProperty("special", Order = 5)]
            public string Special { get; set; }
            [JsonProperty("onlyInDollRoom", Order = 6)]
            public bool? OnlyInDollRoom { get; set; }
            [JsonProperty("staysInPeacefulMode", Order = 7)]
            public bool? StaysInPeacefulMode { get; set; }
        }

        private sealed class TotalsEvidence
        {
            [JsonProperty("classes", Order = 1)] public int Classes { get; set; }
            [JsonProperty("itemWrappers", Order = 2)]
            public int ItemWrappers { get; set; }
            [JsonProperty("rawCandidates", Order = 3)]
            public int RawCandidates { get; set; }
            [JsonProperty("uniqueEntities", Order = 4)]
            public int UniqueEntities { get; set; }
            [JsonProperty("matrixRows", Order = 5)]
            public int MatrixRows { get; set; }
            [JsonProperty("resolvedLinks", Order = 6)]
            public int ResolvedLinks { get; set; }
            [JsonProperty("unresolvedLinks", Order = 7)]
            public int UnresolvedLinks { get; set; }
        }

        internal static RuntimeTestResult Run(ModContext context,
            RuntimeTestRequest request)
        {
            if (context == null) throw new ArgumentNullException("context");
            if (request == null) throw new ArgumentNullException("request");
            DateTime started = DateTime.UtcNow;
            var assertions = new List<RuntimeTestAssertion>();
            var diagnostics = new List<string>();
            var entities = new Dictionary<string, EntityEvidence>(
                StringComparer.Ordinal);
            Assembly gameAssembly = typeof(BlueprintRoot).Assembly;
            string gameHash = HashFile(gameAssembly.Location);
            string gameMvid = gameAssembly.ManifestModule.ModuleVersionId
                .ToString("D");
            var evidence = new AuditEvidence
            {
                SchemaVersion = 1,
                GameVersion = ExpectedGameVersion,
                UnityApplicationVersion = Application.version ?? string.Empty,
                GameAssemblySha256 = gameHash,
                GameAssemblyMvid = gameMvid,
                ClassSources = new List<OwnerEvidence>(),
                ItemSources = new List<OwnerEvidence>(),
                RawSources = new List<RawSourceEvidence>(),
                Entities = new List<EntityEvidence>(),
                SupportedRaces = new List<RaceEvidence>(),
                Totals = new TotalsEvidence(),
                Errors = new List<string>(),
                SaveStateTouched = false
            };

            BlueprintRoot root = BlueprintRoot.Instance;
            BlueprintRace[] races = root == null ||
                root.Progression == null ||
                root.Progression.CharacterRaces == null
                ? new BlueprintRace[0]
                : root.Progression.CharacterRaces
                    .Where(value => value != null)
                    .GroupBy(value => value.RaceId)
                    .Select(group => group.OrderBy(value => value.AssetGuid,
                        StringComparer.Ordinal).First())
                    .OrderBy(value => value.RaceId.ToString(),
                        StringComparer.Ordinal).ToArray();
            foreach (BlueprintRace race in races)
                evidence.SupportedRaces.Add(new RaceEvidence
                {
                    Guid = race.AssetGuid,
                    Name = race.name ?? string.Empty,
                    RaceId = race.RaceId.ToString()
                });

            BlueprintCharacterClass[] classes = root == null ||
                root.Progression == null ||
                root.Progression.CharacterClasses == null
                ? new BlueprintCharacterClass[0]
                : root.Progression.CharacterClasses
                    .Where(value => value != null)
                    .OrderBy(value => value.AssetGuid,
                        StringComparer.Ordinal).ToArray();
            foreach (BlueprintCharacterClass characterClass in classes)
                evidence.ClassSources.Add(InspectClass(
                    characterClass, races, entities, evidence));

            InspectItems(races, entities, evidence);
            InspectRawResources(entities, evidence);

            evidence.ClassSources = evidence.ClassSources
                .OrderBy(value => value.OwnerGuid,
                    StringComparer.Ordinal).ToList();
            evidence.ItemSources = evidence.ItemSources
                .OrderBy(value => value.OwnerGuid,
                    StringComparer.Ordinal).ToList();
            evidence.RawSources = evidence.RawSources
                .OrderBy(value => value.AssetId,
                    StringComparer.Ordinal).ToList();
            evidence.Entities = entities.Values
                .OrderBy(value => value.AssetId,
                    StringComparer.Ordinal).ToList();
            evidence.CandidateSetId = CandidateSetId(
                evidence.Entities.Select(value => value.AssetId));
            evidence.Totals.Classes = evidence.ClassSources.Count;
            evidence.Totals.ItemWrappers = evidence.ItemSources.Count;
            evidence.Totals.RawCandidates = evidence.RawSources.Count;
            evidence.Totals.UniqueEntities = evidence.Entities.Count;
            evidence.Totals.MatrixRows = evidence.ClassSources.Sum(
                value => value.Matrix.Count) +
                evidence.ItemSources.Sum(value => value.Matrix.Count);

            AddAssertions(context, request, classes, races, evidence,
                assertions);
            string path = Path.Combine(request.EvidenceDirectory,
                EvidenceFileName);
            RuntimeTestResultWriter.WriteAtomic(path,
                JsonConvert.SerializeObject(evidence, Formatting.Indented) +
                Environment.NewLine);
            diagnostics.Add("catalogSha256=" + HashFile(path));
            diagnostics.Add("candidateSetId=" + evidence.CandidateSetId);
            diagnostics.Add("totals=" + JsonConvert.SerializeObject(
                evidence.Totals));
            foreach (string error in evidence.Errors)
                diagnostics.Add("auditError=" + error);

            bool pass = assertions.All(value => value.Status ==
                RuntimeTestStatuses.Pass);
            RuntimeBuildIdentity identity = RuntimeBuildIdentity.Capture(
                context.Assembly, context.ModEntry.Info.Version);
            return CreateResult(context, request, evidence, started,
                assertions, diagnostics, identity, path, pass);
        }

        private static RuntimeTestResult CreateResult(ModContext context,
            RuntimeTestRequest request, AuditEvidence evidence,
            DateTime started, List<RuntimeTestAssertion> assertions,
            List<string> diagnostics, RuntimeBuildIdentity identity,
            string path, bool pass)
        {
            return new RuntimeTestResult
            {
                SchemaVersion = 1,
                RunId = request.RunId,
                Scenario = request.Scenario,
                Status = pass ? RuntimeTestStatuses.Pass :
                    RuntimeTestStatuses.Fail,
                LoadedModVersion = context.ModEntry.Info.Version,
                RuntimeIdentity = context.Assembly.FullName + ";pid=" +
                    Process.GetCurrentProcess().Id,
                GitCommit = identity.GitCommit,
                GameVersion = evidence.GameVersion,
                StartUtc = started.ToString("o"),
                EndUtc = string.Empty,
                Assertions = assertions,
                Diagnostics = diagnostics,
                Warnings = new List<string>
                {
                    "Structural labels are hypotheses; rendered images remain the aesthetic authority."
                },
                ExceptionSummary = evidence.Errors.FirstOrDefault() ??
                    string.Empty,
                EvidenceFiles = new List<string> { path },
                AutomaticExitRequested = request.ExitAfterCompletion,
                EvidenceDirectory = request.EvidenceDirectory
            };
        }

        private static OwnerEvidence InspectClass(
            BlueprintCharacterClass characterClass, BlueprintRace[] races,
            IDictionary<string, EntityEvidence> entities,
            AuditEvidence evidence)
        {
            KingmakerEquipmentEntity[] wrappers =
                characterClass.EquipmentEntities ??
                new KingmakerEquipmentEntity[0];
            var owner = new OwnerEvidence
            {
                SourceClassification = "class-clothing",
                OwnerGuid = characterClass.AssetGuid,
                OwnerName = characterClass.name ?? string.Empty,
                DisplayName = characterClass.Name ?? string.Empty,
                DlcType = characterClass.DlcType.ToString(),
                PrimaryColor = characterClass.PrimaryColor,
                SecondaryColor = characterClass.SecondaryColor,
                WrapperGuids = wrappers.Where(value => value != null)
                    .Select(value => value.AssetGuid)
                    .OrderBy(value => value, StringComparer.Ordinal)
                    .ToList(),
                ItemOwners = new List<string>(),
                Matrix = new List<MatrixEvidence>()
            };
            foreach (Gender gender in new[] { Gender.Male, Gender.Female })
                foreach (BlueprintRace race in races)
                {
                    Dictionary<string, List<string>> origins =
                        ClassOrigins(characterClass, wrappers, gender,
                            race.RaceId);
                    List<EquipmentEntityLink> links;
                    try
                    {
                        links = characterClass.GetClothesLinks(
                            gender, race.RaceId) ??
                            new List<EquipmentEntityLink>();
                    }
                    catch (Exception exception)
                    {
                        evidence.Errors.Add("class-links owner=" +
                            characterClass.AssetGuid + ";gender=" + gender +
                            ";race=" + race.RaceId + ";" +
                            ExceptionText(exception));
                        links = new List<EquipmentEntityLink>();
                    }
                    owner.Matrix.Add(InspectMatrix(gender, race, links,
                        origins, entities, evidence));
                }
            return owner;
        }

        private static Dictionary<string, List<string>> ClassOrigins(
            BlueprintCharacterClass characterClass,
            IEnumerable<KingmakerEquipmentEntity> wrappers,
            Gender gender, Race race)
        {
            var result = new Dictionary<string, List<string>>(
                StringComparer.Ordinal);
            EquipmentEntityLink[] direct = gender == Gender.Male
                ? characterClass.MaleEquipmentEntities
                : characterClass.FemaleEquipmentEntities;
            foreach (EquipmentEntityLink link in direct ??
                new EquipmentEntityLink[0])
                AddOrigin(result, link, gender == Gender.Male
                    ? "class-male-direct" : "class-female-direct");
            foreach (KingmakerEquipmentEntity wrapper in wrappers)
            {
                if (wrapper == null) continue;
                foreach (EquipmentEntityLink link in
                    wrapper.GetLinks(gender, race) ??
                    new EquipmentEntityLink[0])
                    AddOrigin(result, link, "class-shared-wrapper:" +
                        wrapper.AssetGuid);
            }
            return result;
        }

        private static void AddOrigin(
            IDictionary<string, List<string>> origins,
            EquipmentEntityLink link, string origin)
        {
            string assetId = LinkAssetId(link);
            List<string> values;
            if (!origins.TryGetValue(assetId, out values))
            {
                values = new List<string>();
                origins.Add(assetId, values);
            }
            if (!values.Contains(origin, StringComparer.Ordinal))
                values.Add(origin);
        }

        private static void InspectItems(BlueprintRace[] races,
            IDictionary<string, EntityEvidence> entities,
            AuditEvidence evidence)
        {
            var wrappers = new Dictionary<string, KingmakerEquipmentEntity>(
                StringComparer.Ordinal);
            var owners = new Dictionary<string, List<string>>(
                StringComparer.Ordinal);
            foreach (BlueprintItemEquipment item in
                ResourcesLibrary.GetBlueprints<BlueprintItemEquipment>()
                    .Where(value => value != null)
                    .OrderBy(value => value.AssetGuid,
                        StringComparer.Ordinal))
            {
                var itemWrappers = new List<KingmakerEquipmentEntity>();
                if (item.EquipmentEntity != null)
                    itemWrappers.Add(item.EquipmentEntity);
                itemWrappers.AddRange(item.EquipmentEntityAlternatives ??
                    new KingmakerEquipmentEntity[0]);
                foreach (KingmakerEquipmentEntity wrapper in itemWrappers)
                {
                    if (wrapper == null ||
                        string.IsNullOrWhiteSpace(wrapper.AssetGuid)) continue;
                    wrappers[wrapper.AssetGuid] = wrapper;
                    List<string> values;
                    if (!owners.TryGetValue(wrapper.AssetGuid, out values))
                    {
                        values = new List<string>();
                        owners.Add(wrapper.AssetGuid, values);
                    }
                    string owner = item.AssetGuid + "/" +
                        (item.name ?? string.Empty);
                    if (!values.Contains(owner, StringComparer.Ordinal))
                        values.Add(owner);
                }
            }

            foreach (KeyValuePair<string, KingmakerEquipmentEntity> pair in
                wrappers.OrderBy(value => value.Key, StringComparer.Ordinal))
                evidence.ItemSources.Add(InspectItemWrapper(pair.Value,
                    owners[pair.Key], races, entities, evidence));
        }

        private static OwnerEvidence InspectItemWrapper(
            KingmakerEquipmentEntity wrapper, IEnumerable<string> owners,
            BlueprintRace[] races,
            IDictionary<string, EntityEvidence> entities,
            AuditEvidence evidence)
        {
            var owner = new OwnerEvidence
            {
                SourceClassification = "item-linked",
                OwnerGuid = wrapper.AssetGuid,
                OwnerName = wrapper.name ?? string.Empty,
                DisplayName = wrapper.name ?? string.Empty,
                DlcType = string.Empty,
                WrapperGuids = new List<string> { wrapper.AssetGuid },
                ItemOwners = owners.OrderBy(value => value,
                    StringComparer.Ordinal).ToList(),
                Matrix = new List<MatrixEvidence>()
            };
            foreach (Gender gender in new[] { Gender.Male, Gender.Female })
                foreach (BlueprintRace race in races)
                {
                    EquipmentEntityLink[] links;
                    try
                    {
                        links = wrapper.GetLinks(gender, race.RaceId) ??
                            new EquipmentEntityLink[0];
                    }
                    catch (Exception exception)
                    {
                        evidence.Errors.Add("item-links wrapper=" +
                            wrapper.AssetGuid + ";gender=" + gender +
                            ";race=" + race.RaceId + ";" +
                            ExceptionText(exception));
                        links = new EquipmentEntityLink[0];
                    }
                    var origins = new Dictionary<string, List<string>>(
                        StringComparer.Ordinal);
                    foreach (EquipmentEntityLink link in links)
                        AddOrigin(origins, link, "item-wrapper:" +
                            wrapper.AssetGuid);
                    owner.Matrix.Add(InspectMatrix(gender, race,
                        links, origins, entities, evidence));
                }
            return owner;
        }

        private static MatrixEvidence InspectMatrix(Gender gender,
            BlueprintRace race, IEnumerable<EquipmentEntityLink> links,
            IDictionary<string, List<string>> origins,
            IDictionary<string, EntityEvidence> entities,
            AuditEvidence evidence)
        {
            var row = new MatrixEvidence
            {
                Gender = gender.ToString(),
                Race = race.name ?? string.Empty,
                RaceId = race.RaceId.ToString(),
                Links = new List<LinkEvidence>()
            };
            int index = 0;
            foreach (EquipmentEntityLink link in links ??
                new EquipmentEntityLink[0])
            {
                string assetId = LinkAssetId(link);
                List<string> sourceOrigins;
                if (!origins.TryGetValue(assetId, out sourceOrigins))
                    sourceOrigins = new List<string> { "unclassified" };
                var linkEvidence = new LinkEvidence
                {
                    Order = index++,
                    AssetId = assetId,
                    ResourceName = ResourceName(assetId),
                    Origins = sourceOrigins.OrderBy(value => value,
                        StringComparer.Ordinal).ToList(),
                    LoadError = string.Empty
                };
                try
                {
                    EquipmentEntity entity = link == null
                        ? null : link.Load(false);
                    linkEvidence.Loaded = entity != null;
                    if (entity == null)
                    {
                        evidence.Totals.UnresolvedLinks++;
                        linkEvidence.LoadError = "resolved-null";
                    }
                    else
                    {
                        evidence.Totals.ResolvedLinks++;
                        if (!entities.ContainsKey(assetId))
                            entities.Add(assetId, InspectEntity(assetId,
                                linkEvidence.ResourceName, entity));
                    }
                }
                catch (Exception exception)
                {
                    evidence.Totals.UnresolvedLinks++;
                    linkEvidence.LoadError = ExceptionText(exception);
                    evidence.Errors.Add("link-load assetId=" + assetId +
                        ";" + linkEvidence.LoadError);
                }
                row.Links.Add(linkEvidence);
            }
            return row;
        }

        private static string LinkAssetId(EquipmentEntityLink link)
        {
            if (link == null) return "<null-link>";
            return string.IsNullOrWhiteSpace(link.AssetId)
                ? "<empty-asset-id>" : link.AssetId;
        }

        private static string ResourceName(string assetId)
        {
            string value;
            return ResourcesLibrary.LibraryObject != null &&
                ResourcesLibrary.LibraryObject.ResourceNamesByAssetId != null &&
                ResourcesLibrary.LibraryObject.ResourceNamesByAssetId
                    .TryGetValue(assetId, out value)
                ? value ?? string.Empty : "<unmapped>";
        }

        private static void InspectRawResources(
            IDictionary<string, EntityEvidence> entities,
            AuditEvidence evidence)
        {
            Dictionary<string, string> names =
                ResourcesLibrary.LibraryObject == null ? null :
                ResourcesLibrary.LibraryObject.ResourceNamesByAssetId;
            if (names == null)
            {
                evidence.Errors.Add("resource-name-map-missing");
                return;
            }
            KeyValuePair<string, string>[] candidates = names
                .Where(value => LooksLikeRawCandidate(value.Value))
                .OrderBy(value => value.Key, StringComparer.Ordinal)
                .ToArray();
            foreach (KeyValuePair<string, string> candidate in candidates)
            {
                var row = new RawSourceEvidence
                {
                    SourceClassification = "raw-resource",
                    AssetId = candidate.Key,
                    ResourceName = candidate.Value ?? string.Empty,
                    LoadError = string.Empty
                };
                try
                {
                    EquipmentEntity entity =
                        ResourcesLibrary.TryGetResource<EquipmentEntity>(
                            candidate.Key, true);
                    row.Loaded = entity != null;
                    if (entity == null) continue;
                    if (!entities.ContainsKey(candidate.Key))
                        entities.Add(candidate.Key, InspectEntity(
                            candidate.Key, row.ResourceName, entity));
                    evidence.RawSources.Add(row);
                }
                catch (Exception exception)
                {
                    row.LoadError = ExceptionText(exception);
                    evidence.Errors.Add("raw-load assetId=" + candidate.Key +
                        ";" + row.LoadError);
                    evidence.RawSources.Add(row);
                }
            }
        }

        private static bool LooksLikeRawCandidate(string resourceName)
        {
            if (string.IsNullOrWhiteSpace(resourceName)) return false;
            string lower = resourceName.ToLowerInvariant();
            bool equipmentShape = lower.StartsWith("ee_",
                    StringComparison.Ordinal) ||
                lower.Contains("equipment") ||
                lower.Contains("/ee_") || lower.Contains("\\ee_");
            return equipmentShape && RawCandidateTerms.Any(lower.Contains);
        }

        private static EntityEvidence InspectEntity(string assetId,
            string resourceName, EquipmentEntity entity)
        {
            var result = new EntityEvidence
            {
                AssetId = assetId,
                ResourceName = resourceName ?? string.Empty,
                EntityName = entity.name ?? string.Empty,
                Layer = entity.Layer,
                HideBodyParts = entity.HideBodyParts.ToString(),
                ShowLowerMaterials = entity.ShowLowerMaterials,
                ColorsProfile = entity.ColorsProfile == null
                    ? "<none>" : entity.ColorsProfile.name,
                PrimaryRampCount = entity.PrimaryRamps == null
                    ? 0 : entity.PrimaryRamps.Count,
                SecondaryRampCount = entity.SecondaryRamps == null
                    ? 0 : entity.SecondaryRamps.Count,
                BodyParts = InspectParts(entity.BodyParts),
                OutfitParts = InspectParts(entity.OutfitParts),
                StructuralRisks = new List<string>()
            };
            AddStructuralRisks(result);
            return result;
        }

        private static List<PartEvidence> InspectParts(IEnumerable parts)
        {
            var result = new List<PartEvidence>();
            if (parts == null) return result;
            int index = 0;
            foreach (object part in parts)
            {
                if (part == null) continue;
                object prefab = ReadMember(part, "Prefab") ??
                    ReadMember(part, "RendererPrefab");
                result.Add(new PartEvidence
                {
                    Index = index++,
                    Type = Describe(ReadMember(part, "Type")),
                    Prefab = Describe(prefab),
                    Material = Describe(ReadMember(part, "Material")),
                    Special = Describe(ReadMember(part, "Special")),
                    OnlyInDollRoom = NullableBool(ReadMember(
                        part, "OnlyInDollRoom")),
                    StaysInPeacefulMode = NullableBool(ReadMember(
                        part, "StaysInPeacefulMode"))
                });
            }
            return result;
        }

        private static object ReadMember(object instance, string name)
        {
            if (instance == null) return null;
            for (Type type = instance.GetType(); type != null;
                type = type.BaseType)
            {
                PropertyInfo property = type.GetProperty(name,
                    BindingFlags.Instance | BindingFlags.Public |
                    BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
                if (property != null && property.GetIndexParameters().Length == 0)
                    return property.GetValue(instance, null);
                FieldInfo field = type.GetField(name,
                    BindingFlags.Instance | BindingFlags.Public |
                    BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
                if (field != null) return field.GetValue(instance);
            }
            return null;
        }

        private static string Describe(object value)
        {
            if (value == null) return "<none>";
            var unity = value as UnityEngine.Object;
            if (unity != null)
                return unity.GetType().FullName + "/" +
                    (unity.name ?? string.Empty);
            object assetId = ReadMember(value, "AssetId");
            if (assetId is string && !string.IsNullOrWhiteSpace(
                (string)assetId))
                return value.GetType().FullName + "/" + assetId;
            return value.ToString();
        }

        private static bool? NullableBool(object value)
        {
            return value is bool ? (bool?)value : null;
        }

        private static void AddStructuralRisks(EntityEvidence entity)
        {
            string searchable = (entity.ResourceName + "|" +
                entity.EntityName + "|" + entity.HideBodyParts + "|" +
                string.Join("|", entity.BodyParts.Concat(
                    entity.OutfitParts).Select(value => value.Type + "|" +
                        value.Prefab + "|" + value.Material + "|" +
                        value.Special))).ToLowerInvariant();
            foreach (string term in BakedWeaponTerms)
                if (searchable.Contains(term))
                    entity.StructuralRisks.Add(
                        "possible-baked-weapon-name:" + term);
            string hidden = entity.HideBodyParts.ToLowerInvariant();
            if (new[] { "head", "hair", "horn", "cap", "ear" }
                .Any(hidden.Contains))
                entity.StructuralRisks.Add("head-or-hair-hiding:" +
                    entity.HideBodyParts);
            foreach (PartEvidence part in entity.OutfitParts)
            {
                string special = part.Special.ToLowerInvariant();
                if (special.Contains("cloak") ||
                    special.Contains("backpack"))
                    entity.StructuralRisks.Add("special-outfit-part:" +
                        part.Special);
                if (part.OnlyInDollRoom == true)
                    entity.StructuralRisks.Add("doll-room-only-part:" +
                        part.Index);
            }
            entity.StructuralRisks = entity.StructuralRisks.Distinct(
                StringComparer.Ordinal).OrderBy(value => value,
                    StringComparer.Ordinal).ToList();
        }

        private static void AddAssertions(ModContext context,
            RuntimeTestRequest request, BlueprintCharacterClass[] classes,
            BlueprintRace[] races, AuditEvidence evidence,
            ICollection<RuntimeTestAssertion> assertions)
        {
            Add(assertions, "guarded-request",
                RuntimeTestScenarioCatalog.GunslingerOutfitAudit,
                request.Scenario,
                string.Equals(request.Scenario,
                    RuntimeTestScenarioCatalog.GunslingerOutfitAudit,
                    StringComparison.Ordinal),
                "validated -kmgRuntimeTestRequest allowlist");
            Add(assertions, "loaded-mod-version",
                request.ExpectedModVersion, context.ModEntry.Info.Version,
                string.Equals(request.ExpectedModVersion,
                    context.ModEntry.Info.Version,
                    StringComparison.Ordinal),
                "Unity Mod Manager ModEntry.Info.Version");
            Add(assertions, "installed-game-version",
                "Kingmaker 2.1.7b Assembly-CSharp exact SHA-256 and MVID",
                "supportedVersion=" + evidence.GameVersion +
                    ";unityApplicationVersion=" +
                    evidence.UnityApplicationVersion + ";sha256=" +
                    evidence.GameAssemblySha256 + ";mvid=" +
                    evidence.GameAssemblyMvid,
                string.Equals(evidence.GameAssemblySha256,
                    ExpectedAssemblySha256, StringComparison.Ordinal) &&
                string.Equals(evidence.GameAssemblyMvid,
                    ExpectedAssemblyMvid,
                    StringComparison.OrdinalIgnoreCase),
                "live loaded Assembly-CSharp identity");
            Add(assertions, "dynamic-player-races",
                "nonempty distinct BlueprintRoot player-race catalog",
                string.Join(",", evidence.SupportedRaces.Select(
                    value => value.RaceId).ToArray()),
                races.Length > 0 && races.Select(value => value.RaceId)
                    .Distinct().Count() == races.Length,
                "BlueprintRoot.Instance.Progression.CharacterRaces");
            string gunslingerGuid = BlueprintBootstrap.GunslingerClass
                .CharacterClass.AssetGuid;
            OwnerEvidence gunslinger = evidence.ClassSources.SingleOrDefault(
                value => string.Equals(value.OwnerGuid, gunslingerGuid,
                    StringComparison.Ordinal));
            bool targetMatrix = gunslinger != null &&
                gunslinger.Matrix.Count == races.Length * 2 &&
                gunslinger.Matrix.All(value => value.Links.Count > 0 &&
                    value.Links.All(link => link.Loaded));
            Add(assertions, "gunslinger-current-matrix",
                "every installed player race and both genders resolve current class clothes",
                gunslinger == null ? "<missing>" :
                    "rows=" + gunslinger.Matrix.Count + ";links=" +
                    gunslinger.Matrix.Sum(value => value.Links.Count),
                targetMatrix,
                "live BlueprintCharacterClass.GetClothesLinks plus link.Load");
            string[] benchmarks = { "Fighter", "Barbarian", "Paladin" };
            bool benchmarkCoverage = benchmarks.All(label =>
                classes.Any(value => value.name != null &&
                    value.name.IndexOf(label,
                        StringComparison.OrdinalIgnoreCase) >= 0));
            Add(assertions, "native-benchmark-classes",
                string.Join(",", benchmarks), benchmarkCoverage.ToString(),
                benchmarkCoverage,
                "live player class catalog");
            bool sourceStreams = evidence.ClassSources.Count > 0 &&
                evidence.ItemSources.Count > 0 &&
                evidence.RawSources.Count > 0 &&
                evidence.Entities.Count > 0;
            Add(assertions, "candidate-source-streams",
                "class-clothing,item-linked,raw-resource",
                "classes=" + evidence.ClassSources.Count +
                    ";items=" + evidence.ItemSources.Count +
                    ";raw=" + evidence.RawSources.Count +
                    ";entities=" + evidence.Entities.Count,
                sourceStreams,
                "live library inventory; raw stream is keyword-bounded");
            Add(assertions, "audit-errors",
                "zero inspection exceptions", evidence.Errors.Count.ToString(),
                evidence.Errors.Count == 0,
                "per-owner, link, and raw-resource exception capture");
            Add(assertions, "no-save-owned-state",
                "no save, inventory, progression, or avatar mutation",
                "saveStateTouched=" + evidence.SaveStateTouched,
                !evidence.SaveStateTouched,
                "read-only library access and resource loads only");
        }

        private static void Add(ICollection<RuntimeTestAssertion> assertions,
            string name, string expected, string observed, bool pass,
            string source)
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

        private static string CandidateSetId(IEnumerable<string> assetIds)
        {
            string canonical = string.Join("\n", assetIds
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Distinct(StringComparer.Ordinal)
                .OrderBy(value => value, StringComparer.Ordinal).ToArray());
            using (SHA256 algorithm = SHA256.Create())
                return Hex(algorithm.ComputeHash(
                    new UTF8Encoding(false).GetBytes(canonical)));
        }

        private static string HashFile(string path)
        {
            using (SHA256 algorithm = SHA256.Create())
            using (FileStream stream = File.OpenRead(path))
                return Hex(algorithm.ComputeHash(stream));
        }

        private static string Hex(byte[] bytes)
        {
            var builder = new StringBuilder(bytes.Length * 2);
            foreach (byte value in bytes)
                builder.Append(value.ToString("x2"));
            return builder.ToString();
        }

        private static string ExceptionText(Exception exception)
        {
            return exception == null ? string.Empty :
                exception.GetType().FullName + ": " + exception.Message;
        }
    }
}
