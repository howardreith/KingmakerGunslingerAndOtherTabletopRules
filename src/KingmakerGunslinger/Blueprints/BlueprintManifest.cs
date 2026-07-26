using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace KingmakerGunslinger.Blueprints
{
    /// <summary>
    /// Strict loader and in-memory index for the checked-in blueprint identifier manifest.
    /// The manifest is content, not a source of runtime-generated identifiers.
    /// </summary>
    internal sealed class BlueprintManifest
    {
        internal const int SupportedSchemaVersion = 1;
        internal const string RequiredNamespace = "KMG";
        internal const string RelativeManifestPath = "blueprints/blueprints.json";

        private const long MaximumManifestBytes = 1024 * 1024;
        private const string RequiredGuidFormat = "lowercase 32-character hexadecimal";
        private static readonly Regex SymbolPattern = new Regex(
            "^KMG\\.[A-Za-z0-9_.]+$",
            RegexOptions.CultureInvariant);

        private readonly Dictionary<string, BlueprintManifestEntry> _entriesBySymbol;

        private BlueprintManifest(
            string filePath,
            Dictionary<string, BlueprintManifestEntry> entriesBySymbol)
        {
            FilePath = filePath;
            _entriesBySymbol = entriesBySymbol;
        }

        internal string FilePath { get; private set; }

        internal int Count
        {
            get { return _entriesBySymbol.Count; }
        }

        internal static BlueprintManifest Load(string modDirectory)
        {
            if (string.IsNullOrWhiteSpace(modDirectory))
            {
                throw new ArgumentException("The installed mod directory is required.", "modDirectory");
            }

            string fullModDirectory = Path.GetFullPath(modDirectory);
            string manifestPath = Path.GetFullPath(
                Path.Combine(fullModDirectory, "blueprints", "blueprints.json"));
            EnsurePathIsUnderModDirectory(fullModDirectory, manifestPath);

            FileInfo file = new FileInfo(manifestPath);
            if (!file.Exists)
            {
                throw new FileNotFoundException(
                    "The deployed blueprint manifest is missing.",
                    manifestPath);
            }

            if (file.Length <= 0 || file.Length > MaximumManifestBytes)
            {
                throw new InvalidDataException(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "Blueprint manifest length {0} is outside the accepted range 1..{1} bytes.",
                        file.Length,
                        MaximumManifestBytes));
            }

            ManifestDocument document;
            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                CheckAdditionalContent = true,
                Culture = CultureInfo.InvariantCulture,
                DateParseHandling = DateParseHandling.None,
                MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Error,
                TypeNameHandling = TypeNameHandling.None
            };

            UTF8Encoding strictUtf8 = new UTF8Encoding(false, true);
            using (StreamReader reader = new StreamReader(manifestPath, strictUtf8, false))
            {
                string json = reader.ReadToEnd();
                document = JsonConvert.DeserializeObject<ManifestDocument>(json, settings);
            }

            if (document == null)
            {
                throw new InvalidDataException("The blueprint manifest deserialized to null.");
            }

            return ValidateAndIndex(manifestPath, document);
        }

        internal BlueprintManifestEntry ResolveActive(string symbol, Type expectedType)
        {
            if (string.IsNullOrWhiteSpace(symbol))
            {
                throw new ArgumentException("A blueprint symbol is required.", "symbol");
            }

            if (expectedType == null)
            {
                throw new ArgumentNullException("expectedType");
            }

            BlueprintManifestEntry entry;
            if (!_entriesBySymbol.TryGetValue(symbol, out entry))
            {
                throw new KeyNotFoundException(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "Blueprint symbol '{0}' is not present in the deployed manifest.",
                        symbol));
            }

            if (!string.Equals(entry.Status, "active", StringComparison.Ordinal))
            {
                throw new InvalidDataException(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "Blueprint symbol '{0}' has status '{1}' and cannot be registered.",
                        symbol,
                        entry.Status));
            }

            if (!string.Equals(entry.PlannedType, expectedType.Name, StringComparison.Ordinal))
            {
                throw new InvalidDataException(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "Blueprint symbol '{0}' declares type '{1}', but registration requested '{2}'.",
                        symbol,
                        entry.PlannedType,
                        expectedType.Name));
            }

            return entry;
        }

        private static BlueprintManifest ValidateAndIndex(
            string manifestPath,
            ManifestDocument document)
        {
            if (document.SchemaVersion != SupportedSchemaVersion)
            {
                throw new InvalidDataException(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "Unsupported blueprint manifest schema version {0}; expected {1}.",
                        document.SchemaVersion,
                        SupportedSchemaVersion));
            }

            if (!string.Equals(document.Namespace, RequiredNamespace, StringComparison.Ordinal))
            {
                throw new InvalidDataException(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "Blueprint manifest namespace '{0}' does not match required namespace '{1}'.",
                        document.Namespace,
                        RequiredNamespace));
            }

            if (document.Policy == null)
            {
                throw new InvalidDataException("Blueprint manifest policy is required.");
            }

            if (document.Policy.RuntimeGenerationAllowed)
            {
                throw new InvalidDataException("Blueprint runtime GUID generation must remain disabled.");
            }

            if (!document.Policy.RetiredIdsRemainReserved)
            {
                throw new InvalidDataException("Retired blueprint identifiers must remain reserved.");
            }

            if (!string.Equals(document.Policy.Format, RequiredGuidFormat, StringComparison.Ordinal))
            {
                throw new InvalidDataException(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "Blueprint identifier format policy '{0}' does not match '{1}'.",
                        document.Policy.Format,
                        RequiredGuidFormat));
            }

            if (document.Entries == null || document.Entries.Count == 0)
            {
                throw new InvalidDataException("Blueprint manifest must contain at least one entry.");
            }

            Dictionary<string, BlueprintManifestEntry> entriesBySymbol =
                new Dictionary<string, BlueprintManifestEntry>(StringComparer.Ordinal);
            Dictionary<string, string> symbolsByGuid =
                new Dictionary<string, string>(StringComparer.Ordinal);

            for (int index = 0; index < document.Entries.Count; index++)
            {
                ManifestEntryDocument source = document.Entries[index];
                if (source == null)
                {
                    throw new InvalidDataException(
                        string.Format(
                            CultureInfo.InvariantCulture,
                            "Blueprint manifest entry {0} is null.",
                            index));
                }

                ValidateEntryText(index, source);
                BlueprintId id = BlueprintId.Parse(source.Guid, "guid");

                if (entriesBySymbol.ContainsKey(source.Symbol))
                {
                    throw new InvalidDataException(
                        string.Format(
                            CultureInfo.InvariantCulture,
                            "Duplicate blueprint symbol '{0}'.",
                            source.Symbol));
                }

                string existingSymbol;
                if (symbolsByGuid.TryGetValue(id.Value, out existingSymbol))
                {
                    throw new InvalidDataException(
                        string.Format(
                            CultureInfo.InvariantCulture,
                            "Duplicate blueprint GUID '{0}' is assigned to both '{1}' and '{2}'.",
                            id.Value,
                            existingSymbol,
                            source.Symbol));
                }

                BlueprintManifestEntry entry = new BlueprintManifestEntry(
                    source.Symbol,
                    id,
                    source.PlannedType,
                    source.Status,
                    source.Milestone,
                    source.Notes);
                entriesBySymbol.Add(entry.Symbol, entry);
                symbolsByGuid.Add(entry.Id.Value, entry.Symbol);
            }

            return new BlueprintManifest(manifestPath, entriesBySymbol);
        }

        private static void ValidateEntryText(int index, ManifestEntryDocument entry)
        {
            if (string.IsNullOrWhiteSpace(entry.Symbol) || !SymbolPattern.IsMatch(entry.Symbol))
            {
                throw new InvalidDataException(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "Blueprint manifest entry {0} has invalid symbol '{1}'.",
                        index,
                        entry.Symbol));
            }

            if (string.IsNullOrWhiteSpace(entry.PlannedType))
            {
                throw new InvalidDataException(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "Blueprint symbol '{0}' has no plannedType.",
                        entry.Symbol));
            }

            if (!string.Equals(entry.Status, "reserved", StringComparison.Ordinal) &&
                !string.Equals(entry.Status, "active", StringComparison.Ordinal) &&
                !string.Equals(entry.Status, "retired", StringComparison.Ordinal))
            {
                throw new InvalidDataException(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "Blueprint symbol '{0}' has invalid status '{1}'.",
                        entry.Symbol,
                        entry.Status));
            }

            if (string.IsNullOrWhiteSpace(entry.Milestone))
            {
                throw new InvalidDataException(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "Blueprint symbol '{0}' has no milestone.",
                        entry.Symbol));
            }

            if (entry.Notes == null)
            {
                throw new InvalidDataException(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "Blueprint symbol '{0}' has null notes.",
                        entry.Symbol));
            }
        }

        private static void EnsurePathIsUnderModDirectory(string modDirectory, string filePath)
        {
            string root = modDirectory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                + Path.DirectorySeparatorChar;
            if (!filePath.StartsWith(root, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidDataException("Blueprint manifest path escaped the installed mod directory.");
            }
        }

        [JsonObject(MemberSerialization.OptIn)]
        private sealed class ManifestDocument
        {
            [JsonProperty("$schema")]
            public string Schema { get; set; }

            [JsonProperty("schemaVersion", Required = Required.Always)]
            public int SchemaVersion { get; set; }

            [JsonProperty("namespace", Required = Required.Always)]
            public string Namespace { get; set; }

            [JsonProperty("policy", Required = Required.Always)]
            public ManifestPolicyDocument Policy { get; set; }

            [JsonProperty("entries", Required = Required.Always)]
            public List<ManifestEntryDocument> Entries { get; set; }
        }

        [JsonObject(MemberSerialization.OptIn)]
        private sealed class ManifestPolicyDocument
        {
            [JsonProperty("runtimeGenerationAllowed", Required = Required.Always)]
            public bool RuntimeGenerationAllowed { get; set; }

            [JsonProperty("format", Required = Required.Always)]
            public string Format { get; set; }

            [JsonProperty("retiredIdsRemainReserved", Required = Required.Always)]
            public bool RetiredIdsRemainReserved { get; set; }
        }

        [JsonObject(MemberSerialization.OptIn)]
        private sealed class ManifestEntryDocument
        {
            [JsonProperty("symbol", Required = Required.Always)]
            public string Symbol { get; set; }

            [JsonProperty("guid", Required = Required.Always)]
            public string Guid { get; set; }

            [JsonProperty("plannedType", Required = Required.Always)]
            public string PlannedType { get; set; }

            [JsonProperty("status", Required = Required.Always)]
            public string Status { get; set; }

            [JsonProperty("milestone", Required = Required.Always)]
            public string Milestone { get; set; }

            [JsonProperty("notes", Required = Required.Always)]
            public string Notes { get; set; }
        }
    }

    /// <summary>
    /// Validated immutable manifest entry used by the runtime registry.
    /// </summary>
    internal sealed class BlueprintManifestEntry
    {
        internal BlueprintManifestEntry(
            string symbol,
            BlueprintId id,
            string plannedType,
            string status,
            string milestone,
            string notes)
        {
            Symbol = symbol;
            Id = id;
            PlannedType = plannedType;
            Status = status;
            Milestone = milestone;
            Notes = notes;
        }

        internal string Symbol { get; private set; }

        internal BlueprintId Id { get; private set; }

        internal string PlannedType { get; private set; }

        internal string Status { get; private set; }

        internal string Milestone { get; private set; }

        internal string Notes { get; private set; }
    }
}
