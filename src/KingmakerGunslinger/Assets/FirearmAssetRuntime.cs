using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Firearms;
using UnityEngine;

namespace KingmakerGunslinger.Assets
{
    internal static class FirearmAssetRuntime
    {
        private static readonly object Sync = new object();
        private static AssetBundle _bundle;
        private static readonly Dictionary<FirearmKind, GameObject> Prefabs = new Dictionary<FirearmKind, GameObject>();
        private static readonly Dictionary<FirearmKind, GameObject> BeltPrefabs = new Dictionary<FirearmKind, GameObject>();
        internal static bool IsLoaded { get { lock (Sync) return _bundle != null; } }

        internal static void Configure(ModContext context)
        {
            if (context == null) throw new ArgumentNullException("context");
            string path = Path.Combine(context.ModEntry.Path, "assets", "bundles",
                "kingmakergunslinger.firearms");
            if (!File.Exists(path))
            {
                context.Logger.Warning("assets", "bundle.missing",
                    "Firearm bundle unavailable; cloned native weapon models remain active: " + path);
                return;
            }

            AssetBundle candidate = null;
            try
            {
                candidate = AssetBundle.LoadFromFile(path);
                if (candidate == null)
                    throw new InvalidDataException("Unity rejected the firearm bundle.");
                string[] names = candidate.GetAllAssetNames();
                var prefabs = new Dictionary<FirearmKind, GameObject>();
                var beltPrefabs = new Dictionary<FirearmKind, GameObject>();

                TryLoadPrefab(candidate, names, prefabs, FirearmKind.Pistol, "pistol", context);
                TryLoadPrefab(candidate, names, prefabs, FirearmKind.Musket, "musket", context);
                TryLoadPrefab(candidate, names, prefabs, FirearmKind.Blunderbuss, "blunderbuss", context);
                TryLoadPrefab(candidate, names, prefabs, FirearmKind.Revolver, "revolver", context);
                TryLoadPrefab(candidate, names, prefabs, FirearmKind.Rifle, "rifle", context);
                TryLoadPrefab(candidate, names, beltPrefabs, FirearmKind.Pistol,
                    "pistolbelt", context);
                TryLoadPrefab(candidate, names, beltPrefabs, FirearmKind.Musket,
                    "musketbelt", context);
                TryLoadPrefab(candidate, names, beltPrefabs, FirearmKind.Blunderbuss,
                    "blunderbussbelt", context);

                AssetBundle previous;
                lock (Sync)
                {
                    previous = _bundle;
                    _bundle = candidate;
                    candidate = null;
                    Replace(Prefabs, prefabs);
                    Replace(BeltPrefabs, beltPrefabs);
                }
                if (previous != null) previous.Unload(false);
                context.Logger.Info("assets", "bundle.loaded",
                    "Published firearm bundle transactionally; equippedPrefabs=" +
                    prefabs.Count + ";beltPrefabs=" + beltPrefabs.Count +
                    ". Missing or rejected capabilities retain native presentation fallbacks.");
            }
            catch (Exception exception)
            {
                context.Logger.Failure("assets", "bundle.load-failed",
                    "Firearm bundle was not published; cloned native weapon models remain active.",
                    exception);
            }
            finally
            {
                if (candidate != null) candidate.Unload(false);
            }
        }

        private static void TryLoadPrefab(AssetBundle bundle, string[] names,
            IDictionary<FirearmKind, GameObject> destination,
            FirearmKind kind, string name, ModContext context)
        {
            string suffix = "/" + name + ".prefab";
            string[] matches = names.Where(value => value.EndsWith(
                suffix, StringComparison.OrdinalIgnoreCase)).ToArray();
            if (matches.Length != 1)
            {
                context.Logger.Warning("assets", "prefab.skipped",
                    "kind=" + kind + ";name=" + name +
                    ";matches=" + matches.Length + ";nativeFallback=true");
                return;
            }
            GameObject prefab = bundle.LoadAsset<GameObject>(matches[0]);
            Renderer[] renderers = prefab == null
                ? Array.Empty<Renderer>()
                : prefab.GetComponentsInChildren<Renderer>(true);
            bool renderable = renderers.Any(renderer => renderer != null &&
                renderer.sharedMaterials != null &&
                renderer.sharedMaterials.Any(material => material != null &&
                    material.shader != null));
            if (!renderable)
            {
                context.Logger.Warning("assets", "prefab.skipped",
                    "kind=" + kind + ";name=" + name +
                    ";renderable=false;nativeFallback=true");
                return;
            }
            destination[kind] = prefab;
        }

        private static void Replace<T>(IDictionary<FirearmKind, T> destination,
            IDictionary<FirearmKind, T> source)
        {
            destination.Clear();
            foreach (KeyValuePair<FirearmKind, T> entry in source)
                destination[entry.Key] = entry.Value;
        }
        internal static GameObject InstantiatePrefab(FirearmKind kind)
        {
            lock (Sync) { GameObject prefab; return Prefabs.TryGetValue(kind, out prefab) && prefab != null ? UnityEngine.Object.Instantiate(prefab) : null; }
        }
        internal static GameObject GetPrefab(FirearmKind kind)
        {
            lock (Sync)
            {
                GameObject prefab;
                return Prefabs.TryGetValue(kind, out prefab) ? prefab : null;
            }
        }
        internal static GameObject GetBeltPrefab(FirearmKind kind)
        {
            lock (Sync)
            {
                GameObject prefab;
                return BeltPrefabs.TryGetValue(kind, out prefab) ? prefab : null;
            }
        }
    }
}
