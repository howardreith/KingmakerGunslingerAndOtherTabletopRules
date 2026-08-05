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
        private static readonly Dictionary<FirearmKind, GameObject> BeltPrefabs = new Dictionary<FirearmKind, GameObject>();
        private static readonly Dictionary<FirearmKind, AudioClip> Shots = new Dictionary<FirearmKind, AudioClip>();
        private static long _shotEvents;
        private static bool _lastEmitterReady;
        private static string _lastClipName;
        private static bool _lastListenerPresent;
        private static bool _lastSourcePlaying;
        internal static long ShotEvents { get { lock (Sync) return _shotEvents; } }
        internal static bool LastEmitterReady { get { lock (Sync) return _lastEmitterReady; } }
        internal static string LastClipName { get { lock (Sync) return _lastClipName; } }
        internal static bool LastListenerPresent { get { lock (Sync) return _lastListenerPresent; } }
        internal static bool LastSourcePlaying { get { lock (Sync) return _lastSourcePlaying; } }
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
                var shots = new Dictionary<FirearmKind, AudioClip>();

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
                TryLoadShot(candidate, names, shots, FirearmKind.Pistol,
                    "gunantq_flintlock fire_cs_usc.wav", context);
                TryLoadShot(candidate, names, shots, FirearmKind.Musket,
                    "gunantq_musket shots_cs_usc.wav", context);
                TryLoadShot(candidate, names, shots, FirearmKind.Blunderbuss,
                    "gunshotg_classic western shotgun blast with reverb_cs_usc.wav", context);
                TryLoadShot(candidate, names, shots, FirearmKind.Revolver,
                    "gunpis_exterior pistol shot_cs_usc.wav", context);
                TryLoadShot(candidate, names, shots, FirearmKind.Rifle,
                    "gunantq_flintlock rifle fire_cs_usc.wav", context);

                AssetBundle previous;
                lock (Sync)
                {
                    previous = _bundle;
                    _bundle = candidate;
                    candidate = null;
                    Replace(Prefabs, prefabs);
                    Replace(BeltPrefabs, beltPrefabs);
                    Replace(Shots, shots);
                }
                if (previous != null) previous.Unload(false);
                context.Logger.Info("assets", "bundle.loaded",
                    "Published firearm bundle transactionally; equippedPrefabs=" +
                    prefabs.Count + ";beltPrefabs=" + beltPrefabs.Count +
                    ";audioClips=" + shots.Count +
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

        private static void TryLoadShot(AssetBundle bundle, string[] names,
            IDictionary<FirearmKind, AudioClip> destination,
            FirearmKind kind, string name, ModContext context)
        {
            string suffix = "/" + name;
            string[] matches = names.Where(value => value.EndsWith(
                suffix, StringComparison.OrdinalIgnoreCase)).ToArray();
            if (matches.Length != 1)
            {
                context.Logger.Warning("assets", "audio.skipped",
                    "kind=" + kind + ";name=" + name +
                    ";matches=" + matches.Length);
                return;
            }
            AudioClip clip = bundle.LoadAsset<AudioClip>(matches[0]);
            if (clip == null)
            {
                context.Logger.Warning("assets", "audio.skipped",
                    "kind=" + kind + ";name=" + name + ";loaded=false");
                return;
            }
            destination[kind] = clip;
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
                // Kingmaker's camera/listener and detached unit views make fully
                // spatial raw clips unreliable. Use an audible 2D SFX fallback
                // until the approved clips are authored into a native Wwise bank.
                source.spatialBlend = 0f;
                source.volume = 1f;
                source.PlayOneShot(clip, 1f);
                // Headless/guarded Unity runs can report isPlaying=false when no
                // audio output device is available.  Record that the persistent
                // emitter accepted the invocation without treating hardware
                // playback state as mechanical evidence.
                _lastEmitterReady = source.enabled && emitter.activeInHierarchy;
                _lastListenerPresent = UnityEngine.Object.FindObjectOfType<AudioListener>() != null;
                _lastSourcePlaying = source.isPlaying;
                _lastClipName = clip.name;
                _shotEvents++; return true;
            }
        }
    }
}
