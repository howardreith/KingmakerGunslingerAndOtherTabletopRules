using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Kingmaker.EntitySystem.Entities;
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
        private static readonly Dictionary<FirearmKind, UnityEngine.Object> Shots = new Dictionary<FirearmKind, UnityEngine.Object>();
        private static long _shotEvents;
        internal static long ShotEvents { get { lock (Sync) return _shotEvents; } }
        internal static bool IsLoaded { get { lock (Sync) return _bundle != null; } }

        internal static void Configure(ModContext context)
        {
            if (context == null) throw new ArgumentNullException("context");
            string path = Path.Combine(context.ModEntry.Path, "assets", "bundles", "kingmakergunslinger.firearms");
            try
            {
                if (!File.Exists(path)) { context.Logger.Warning("assets", "bundle.missing", "Firearm bundle unavailable; safe native fallbacks remain active: " + path); return; }
                AssetBundle bundle = AssetBundle.LoadFromFile(path);
                if (bundle == null) throw new InvalidDataException("Unity rejected the firearm bundle.");
                lock (Sync)
                {
                    _bundle = bundle;
                    LoadPrefab(FirearmKind.Pistol, "pistol"); LoadPrefab(FirearmKind.Musket, "musket");
                    LoadPrefab(FirearmKind.Blunderbuss, "blunderbuss"); LoadPrefab(FirearmKind.Revolver, "revolver");
                    LoadShot(FirearmKind.Pistol, "gunantq_flintlock fire_cs_usc.wav");
                    LoadShot(FirearmKind.Musket, "gunantq_musket shots_cs_usc.wav");
                    LoadShot(FirearmKind.Blunderbuss, "gunshotg_classic western shotgun blast with reverb_cs_usc.wav");
                    LoadShot(FirearmKind.Revolver, "gunpis_exterior pistol shot_cs_usc.wav");
                    LoadShot(FirearmKind.Rifle, "gunantq_flintlock rifle fire_cs_usc.wav");
                }
                context.Logger.Info("assets", "bundle.loaded", "Loaded four approved firearm prefabs and five approved CC0 shot mappings.");
            }
            catch (Exception exception) { context.Logger.Failure("assets", "bundle.load-failed", "Firearm bundle failed safely; native fallbacks remain active.", exception); }
        }
        private static void LoadPrefab(FirearmKind kind, string name)
        {
            string path = _bundle.GetAllAssetNames().Single(value => value.EndsWith("/" + name + ".prefab", StringComparison.OrdinalIgnoreCase));
            GameObject prefab = _bundle.LoadAsset<GameObject>(path);
            if (prefab == null) throw new InvalidDataException("Missing firearm prefab: " + name);
            Prefabs[kind] = prefab;
        }
        private static void LoadShot(FirearmKind kind, string name)
        {
            string path = _bundle.GetAllAssetNames().Single(value => value.EndsWith("/" + name, StringComparison.OrdinalIgnoreCase));
            UnityEngine.Object clip = _bundle.LoadAsset(path);
            if (clip == null) throw new InvalidDataException("Missing firearm audio: " + name);
            Shots[kind] = clip;
        }
        internal static GameObject InstantiatePrefab(FirearmKind kind)
        {
            lock (Sync) { GameObject prefab; return Prefabs.TryGetValue(kind, out prefab) && prefab != null ? UnityEngine.Object.Instantiate(prefab) : null; }
        }
        internal static bool PlayShot(FirearmKind kind, UnitEntityData wielder)
        {
            lock (Sync)
            {
                UnityEngine.Object clip;
                if (wielder == null || !Shots.TryGetValue(kind, out clip) || clip == null) return false;
                Type source = AppDomain.CurrentDomain.GetAssemblies()
                    .Select(assembly => assembly.GetType("UnityEngine.AudioSource",
                        false, false)).FirstOrDefault(value => value != null);
                if (source == null)
                {
                    Assembly audio = Assembly.Load("UnityEngine.AudioModule");
                    source = audio == null ? null : audio.GetType(
                        "UnityEngine.AudioSource", false, false);
                }
                if (source == null) return false;
                MethodInfo play = source.GetMethods(BindingFlags.Public |
                    BindingFlags.Static).Single(value =>
                        value.Name == "PlayClipAtPoint" &&
                        value.GetParameters().Length == 3);
                play.Invoke(null, new object[] { clip, wielder.Position, 0.75f });
                _shotEvents++; return true;
            }
        }
    }
}
