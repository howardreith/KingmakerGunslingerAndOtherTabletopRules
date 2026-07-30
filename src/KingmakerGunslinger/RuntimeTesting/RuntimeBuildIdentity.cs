using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using Newtonsoft.Json;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed class RuntimeBuildIdentity
    {
        [JsonProperty("schemaVersion", Order = 1)] public int SchemaVersion { get; set; }
        [JsonProperty("semanticVersion", Order = 2)] public string SemanticVersion { get; set; }
        [JsonProperty("runtimeIdentity", Order = 3)] public string RuntimeIdentity { get; set; }
        [JsonProperty("informationalVersion", Order = 4)] public string InformationalVersion { get; set; }
        [JsonProperty("gitCommit", Order = 5)] public string GitCommit { get; set; }
        [JsonProperty("moduleVersionId", Order = 6)] public string ModuleVersionId { get; set; }
        [JsonProperty("loadedModulePath", Order = 7)] public string LoadedModulePath { get; set; }
        [JsonProperty("loadedModuleSha256", Order = 8)] public string LoadedModuleSha256 { get; set; }
        [JsonProperty("processId", Order = 9)] public int ProcessId { get; set; }
        [JsonProperty("recordedAtUtc", Order = 10)] public string RecordedAtUtc { get; set; }

        internal static RuntimeBuildIdentity Capture(Assembly assembly, string semanticVersion)
        {
            string path = SafeModulePath(assembly);
            return new RuntimeBuildIdentity
            {
                SchemaVersion = 1,
                SemanticVersion = semanticVersion ?? string.Empty,
                RuntimeIdentity = assembly == null ? string.Empty : assembly.FullName,
                InformationalVersion = AttributeValue<AssemblyInformationalVersionAttribute>(
                    assembly, x => x.InformationalVersion),
                GitCommit = MetadataValue(assembly, "GitCommit"),
                ModuleVersionId = assembly == null ? string.Empty :
                    assembly.ManifestModule.ModuleVersionId.ToString("D"),
                LoadedModulePath = path,
                LoadedModuleSha256 = Sha256(path),
                ProcessId = Process.GetCurrentProcess().Id,
                RecordedAtUtc = DateTime.UtcNow.ToString("o")
            };
        }

        private static string SafeModulePath(Assembly assembly)
        {
            try
            {
                if (assembly == null || string.IsNullOrWhiteSpace(assembly.Location))
                    return string.Empty;
                return Path.GetFullPath(assembly.Location);
            }
            catch { return string.Empty; }
        }

        private static string Sha256(string path)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
                    return string.Empty;
                using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read,
                    FileShare.ReadWrite | FileShare.Delete))
                using (var hash = SHA256.Create())
                    return BitConverter.ToString(hash.ComputeHash(stream))
                        .Replace("-", "").ToLowerInvariant();
            }
            catch { return string.Empty; }
        }

        private static string MetadataValue(Assembly assembly, string key)
        {
            if (assembly == null) return string.Empty;
            var value = assembly.GetCustomAttributes(typeof(AssemblyMetadataAttribute), false)
                .OfType<AssemblyMetadataAttribute>()
                .FirstOrDefault(x => string.Equals(x.Key, key, StringComparison.Ordinal));
            return value == null ? string.Empty : value.Value ?? string.Empty;
        }

        private static string AttributeValue<T>(Assembly assembly, Func<T, string> read)
            where T : Attribute
        {
            if (assembly == null) return string.Empty;
            T value = assembly.GetCustomAttributes(typeof(T), false).OfType<T>()
                .FirstOrDefault();
            return value == null ? string.Empty : read(value) ?? string.Empty;
        }
    }
}
