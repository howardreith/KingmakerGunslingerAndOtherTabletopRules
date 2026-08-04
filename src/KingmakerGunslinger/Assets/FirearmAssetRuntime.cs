using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        private static readonly Dictionary<FirearmKind, AudioClip> Shots = new Dictionary<FirearmKind, AudioClip>();
        private static long _shotEvents;
        private static bool _lastEmitterReady;
        private static string _lastClipName;
        internal static long ShotEvents { get { lock (Sync) return _shotEvents; } }
        internal static bool LastEmitterReady { get { lock (Sync) return _lastEmitterReady; } }
        internal static string LastClipName { get { lock (Sync) return _lastClipName; } }
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
                    LoadPrefab(FirearmKind.Rifle, "rifle");
                    LoadShot(FirearmKind.Pistol, "gunantq_flintlock fire_cs_usc.wav");
                    LoadShot(FirearmKind.Musket, "gunantq_musket shots_cs_usc.wav");
                    LoadShot(FirearmKind.Blunderbuss, "gunshotg_classic western shotgun blast with reverb_cs_usc.wav");
                    LoadShot(FirearmKind.Revolver, "gunpis_exterior pistol shot_cs_usc.wav");
                    LoadShot(FirearmKind.Rifle, "gunantq_flintlock rifle fire_cs_usc.wav");
                }
                context.Logger.Info("assets", "bundle.loaded", "Loaded five approved firearm prefabs and five approved CC0 shot mappings.");
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
            AudioClip clip = _bundle.LoadAsset<AudioClip>(path);
            if (clip == null) throw new InvalidDataException("Missing firearm audio: " + name);
            Shots[kind] = clip;
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
        internal static bool PlayShot(FirearmKind kind, UnitEntityData wielder)
        {
            lock (Sync)
            {
                AudioClip clip;
                if (wielder == null || !Shots.TryGetValue(kind, out clip) || clip == null) return false;
                GameObject anchor = wielder.View == null ? null : wielder.View.gameObject;
                if (anchor == null) return false;
                Transform emitterTransform = anchor.transform.Find("KMG_FirearmAudio");
                GameObject emitter;
                if (emitterTransform == null)
                {
                    emitter = new GameObject("KMG_FirearmAudio");
                    emitter.transform.SetParent(anchor.transform, false);
                }
                else emitter = emitterTransform.gameObject;
                AudioSource source = emitter.GetComponent<AudioSource>();
                if (source == null) source = emitter.AddComponent<AudioSource>();
                source.playOnAwake = false;
                source.loop = false;
                source.spatialBlend = 1f;
                source.volume = 1f;
                source.PlayOneShot(clip, 1f);
                // Headless/guarded Unity runs can report isPlaying=false when no
                // audio output device is available.  Record that the persistent
                // emitter accepted the invocation without treating hardware
                // playback state as mechanical evidence.
                _lastEmitterReady = source.enabled && emitter.activeInHierarchy;
                _lastClipName = clip.name;
                _shotEvents++; return true;
            }
        }
    }
}
