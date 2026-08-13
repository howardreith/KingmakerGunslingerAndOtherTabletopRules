using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json;
using UnityEngine;

namespace KingmakerGunslinger.Blueprints
{
    [JsonObject(MemberSerialization.OptIn)]
    internal sealed class ExpandedSummoningRuntimeIconManifest
    {
        [JsonProperty("schemaVersion", Required = Required.Always)]
        public int schemaVersion { get; set; }
        [JsonProperty("count", Required = Required.Always)]
        public int count { get; set; }
        [JsonProperty("icons", Required = Required.Always)]
        public ExpandedSummoningRuntimeIconRow[] icons { get; set; }
    }

    [JsonObject(MemberSerialization.OptIn)]
    internal sealed class ExpandedSummoningRuntimeIconRow
    {
        [JsonProperty("key", Required = Required.Always)]
        public string key { get; set; }
        [JsonProperty("file", Required = Required.Always)]
        public string file { get; set; }
        [JsonProperty("sha256", Required = Required.Always)]
        public string sha256 { get; set; }
        [JsonProperty("width", Required = Required.Always)]
        public int width { get; set; }
        [JsonProperty("height", Required = Required.Always)]
        public int height { get; set; }
        [JsonProperty("format", Required = Required.Always)]
        public string format { get; set; }
        [JsonProperty("scope", Required = Required.Always)]
        public string scope { get; set; }
    }

    internal static class ExpandedSummoningProjectIcons
    {
        private static readonly Dictionary<string, Sprite> Icons =
            new Dictionary<string, Sprite>(StringComparer.Ordinal);
        private static readonly List<Texture2D> Textures = new List<Texture2D>();
        internal static int FallbackCount { get { return 0; } }
        internal static int LoadedCount { get { return Icons.Count; } }

        internal static void Load(ModContext context)
        {
            if (context == null || context.ModEntry == null)
                throw new ArgumentNullException("context");
            SummonIconCatalog.Validate();
            Icons.Clear(); Textures.Clear();
            string directory = Path.Combine(context.ModEntry.Path, "assets",
                "icons", "expanded-summoning");
            string manifestPath = Path.Combine(directory, "icon-manifest.json");
            if (!File.Exists(manifestPath)) throw new FileNotFoundException(
                "Expanded Summoning icon manifest is missing.");
            ExpandedSummoningRuntimeIconManifest manifest;
            try { manifest = JsonConvert.DeserializeObject<
                ExpandedSummoningRuntimeIconManifest>(File.ReadAllText(
                    manifestPath)); }
            catch (JsonException exception) { throw new InvalidDataException(
                "Expanded Summoning icon manifest is invalid.", exception); }
            if (manifest == null || manifest.schemaVersion != 1 ||
                manifest.count != 77 || manifest.icons == null ||
                manifest.icons.Length != 77 || manifest.icons.Select(row =>
                    row.key).Distinct(StringComparer.Ordinal).Count() != 77)
                throw new InvalidDataException(
                    "Expanded Summoning icon manifest contract failed: " +
                    "manifest=" + (manifest == null ? "null" : "present") +
                    ";schema=" + (manifest == null ? -1 :
                        manifest.schemaVersion) + ";count=" +
                    (manifest == null ? -1 : manifest.count) + ";rows=" +
                    (manifest == null || manifest.icons == null ? -1 :
                        manifest.icons.Length) + ";distinct=" +
                    (manifest == null || manifest.icons == null ? -1 :
                        manifest.icons.Where(row => row != null).Select(row =>
                            row.key).Distinct(StringComparer.Ordinal).Count()) +
                    ".");
            foreach (SummonProjectIconSpec spec in SummonIconCatalog.All)
            {
                ExpandedSummoningRuntimeIconRow row = manifest.icons.SingleOrDefault(
                    value => string.Equals(value.key, spec.Key,
                        StringComparison.Ordinal));
                if (row == null || row.file != spec.Key + ".png" ||
                    row.width != 128 || row.height != 128 ||
                    row.format != "RGBA PNG" || row.sha256 == null ||
                    row.sha256.Length != 64) throw new InvalidDataException(
                        "Expanded Summoning icon manifest row failed: " + spec.Key);
                string path = Path.Combine(directory, row.file);
                if (!File.Exists(path) || !string.Equals(Hash(path), row.sha256,
                    StringComparison.Ordinal)) throw new InvalidDataException(
                        "Expanded Summoning icon file failed validation: " +
                        spec.Key);
                var texture = new Texture2D(2, 2, TextureFormat.ARGB32, false);
                if (!LoadImage(texture, File.ReadAllBytes(path)) ||
                    texture.width != 128 || texture.height != 128)
                    throw new InvalidDataException(
                        "Expanded Summoning icon could not be decoded: " + spec.Key);
                texture.name = "KMG_SummonIcon_Texture_" + spec.Key;
                texture.filterMode = FilterMode.Bilinear;
                texture.wrapMode = TextureWrapMode.Clamp;
                texture.hideFlags = HideFlags.DontUnloadUnusedAsset;
                Sprite sprite = Sprite.Create(texture,
                    new Rect(0, 0, 128, 128), new Vector2(0.5f, 0.5f), 100f);
                sprite.name = "KMG_SummonIcon_" + spec.Key;
                sprite.hideFlags = HideFlags.DontUnloadUnusedAsset;
                Textures.Add(texture); Icons.Add(spec.Key, sprite);
            }
            if (Icons.Values.Distinct().Count() != 77)
                throw new InvalidOperationException(
                    "Every summon icon concept requires one distinct sprite.");
        }

        internal static Sprite Require(string key)
        {
            Sprite result;
            if (!Icons.TryGetValue(key, out result) || result == null)
                throw new InvalidOperationException(
                    "Required project summon icon is not loaded: " + key + ".");
            return result;
        }

        private static string Hash(string path)
        {
            using (SHA256 sha = SHA256.Create())
            using (FileStream stream = File.OpenRead(path))
                return string.Concat(sha.ComputeHash(stream).Select(value =>
                    value.ToString("x2")));
        }

        private static bool LoadImage(Texture2D texture, byte[] bytes)
        {
            Type type = Type.GetType(
                "UnityEngine.ImageConversion, UnityEngine.ImageConversionModule",
                false);
            MethodInfo method = type == null ? null : type.GetMethod("LoadImage",
                BindingFlags.Public | BindingFlags.Static, null,
                new[] { typeof(Texture2D), typeof(byte[]), typeof(bool) }, null);
            if (method == null) throw new MissingMethodException(
                "Unity runtime lacks ImageConversion.LoadImage.");
            return (bool)method.Invoke(null, new object[] { texture, bytes, false });
        }
    }
}
