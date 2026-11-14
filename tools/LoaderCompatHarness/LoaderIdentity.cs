using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using Harmony12;
using Newtonsoft.Json.Linq;
using UnityModManagerNet;

namespace KmgLoaderCompatHarness
{
    /// <summary>
    /// What this process actually loaded: the loader, both Harmony generations, every mod entry and its assembly,
    /// each identified by the file it was loaded from and that file's SHA-256. Placing files in a directory does not
    /// prove the game loaded them; this record does.
    /// </summary>
    internal static class LoaderIdentity
    {
        public static JObject Capture()
        {
            JObject identity = new JObject
            {
                ["capturedUtc"] = DateTime.UtcNow.ToString("o"),
                ["unityModManager"] = Describe(typeof(UnityModManager).Assembly),
                ["unityModManagerReportedVersion"] = ReportedVersion(),
                ["harmony12"] = Describe(typeof(HarmonyInstance).Assembly)
            };

            Assembly[] loaded = AppDomain.CurrentDomain.GetAssemblies();
            identity["harmony2"] = new JArray(loaded.Where(a => a.GetName().Name == "0Harmony").Select(a => (object)Describe(a)).ToArray());
            identity["unityModManagerAssemblies"] = new JArray(loaded.Where(a => a.GetName().Name == "UnityModManager").Select(a => (object)Describe(a)).ToArray());

            JArray mods = new JArray();
            foreach (UnityModManager.ModEntry entry in UnityModManager.modEntries)
            {
                JObject mod = new JObject
                {
                    ["id"] = entry.Info?.Id,
                    ["version"] = entry.Info?.Version,
                    ["managerVersion"] = entry.Info?.ManagerVersion,
                    ["enabled"] = entry.Enabled,
                    ["active"] = entry.Active,
                    ["loaded"] = entry.Loaded,
                    ["errorOnLoading"] = entry.ErrorOnLoading,
                    ["hasAssembly"] = entry.HasAssembly,
                    ["path"] = entry.Path
                };
                if (entry.Assembly != null) mod["assembly"] = Describe(entry.Assembly);
                mods.Add(mod);
            }

            identity["modEntries"] = mods;
            return identity;
        }

        public static JObject Describe(Assembly assembly)
        {
            JObject description = new JObject
            {
                ["fullName"] = assembly.FullName,
                ["version"] = assembly.GetName().Version?.ToString(),
                ["mvid"] = assembly.ManifestModule.ModuleVersionId.ToString()
            };
            string location = null;
            try { location = assembly.Location; } catch (Exception) { }
            description["location"] = location;
            if (!string.IsNullOrEmpty(location) && File.Exists(location)) description["sha256"] = Sha256(location);
            AssemblyInformationalVersionAttribute informational = assembly.GetCustomAttributes(typeof(AssemblyInformationalVersionAttribute), false)
                .OfType<AssemblyInformationalVersionAttribute>().FirstOrDefault();
            if (informational != null) description["informationalVersion"] = informational.InformationalVersion;
            AssemblyMetadataAttribute commit = assembly.GetCustomAttributes(typeof(AssemblyMetadataAttribute), false)
                .OfType<AssemblyMetadataAttribute>().FirstOrDefault(a => a.Key == "GitCommit");
            if (commit != null) description["gitCommit"] = commit.Value;
            return description;
        }

        public static string Sha256(string path)
        {
            using (SHA256 sha = SHA256.Create())
            using (FileStream stream = File.OpenRead(path))
            {
                return BitConverter.ToString(sha.ComputeHash(stream)).Replace("-", string.Empty).ToLowerInvariant();
            }
        }

        private static string ReportedVersion()
        {
            const BindingFlags Any = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
            object value = typeof(UnityModManager).GetField("version", Any)?.GetValue(null)
                ?? typeof(UnityModManager).GetProperty("version", Any)?.GetValue(null, null);
            return value?.ToString();
        }
    }
}
