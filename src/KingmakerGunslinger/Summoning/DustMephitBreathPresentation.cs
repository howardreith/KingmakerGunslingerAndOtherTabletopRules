using System;
using System.Collections.Generic;
using System.Reflection;
using Harmony12;
using Kingmaker.Controllers.Projectiles;
using UnityEngine;

namespace KingmakerGunslinger.Summoning
{
    /// <summary>
    /// VIS-05: the Dust Mephit breath keeps the shared native cold cone for
    /// delivery, area, orientation and timing, and only the cone the exact
    /// owned Dust Mephit launches is presented as dust. The visible cone is
    /// the projectile blueprint's cast effect, which native
    /// <c>ProjectileController.CreateView</c> spawns through
    /// <c>FxHelper.SpawnFxOnPoint</c> beside the (renderer-less) projectile
    /// view. While CreateView runs for that exact projectile, the instance
    /// spawned from its own cast-effect prefab gets an owned component which
    /// clones its renderers' materials and recolours the clones and its
    /// particle colours from ice to dust. When the instance is released to the
    /// native pool (deactivated) or destroyed, the component puts back the
    /// original shared materials and colours, destroys its clones and removes
    /// itself, so the next consumer sees the native ice cone. No shared prefab,
    /// projectile blueprint or material is written.
    /// </summary>
    [HarmonyPatch(typeof(ProjectileController), "CreateView", new[] { typeof(Projectile), typeof(Vector3?) })]
    internal static class DustMephitBreathPresentation
    {
        // The exact Dust projectile whose cast effect is being spawned now;
        // set only inside its own CreateView and cleared after one adoption.
        internal static Projectile Pending;

        private static void Prefix(Projectile projectile)
        {
            Pending = null;
            try
            {
                if (projectile != null && projectile.Launcher != null && projectile.Launcher.Blueprint != null &&
                    projectile.Blueprint != null && MephitBreathPolicy.PresentsDustBreath(
                        projectile.Launcher.Blueprint.AssetGuid, projectile.Blueprint.AssetGuid))
                    Pending = projectile;
            }
            catch (Exception error) { Debug.LogException(error); }
        }

        private static void Postfix() { Pending = null; }
    }

    [HarmonyPatch(typeof(Kingmaker.Visual.Particles.FxHelper), "SpawnFxOnPoint",
        new[] { typeof(GameObject), typeof(Vector3), typeof(Quaternion) })]
    internal static class DustMephitBreathCastFxPatch
    {
        private static void Postfix(GameObject prefab, GameObject __result)
        {
            try
            {
                Projectile projectile = DustMephitBreathPresentation.Pending;
                if (projectile == null || __result == null || prefab == null) return;
                GameObject castFx = projectile.Blueprint.CastFx.Load();
                if (!ReferenceEquals(prefab, castFx)) return;
                DustMephitBreathPresentation.Pending = null;
                DustMephitBreathTint existing = __result.GetComponent<DustMephitBreathTint>();
                if (existing != null) existing.Restore();
                __result.AddComponent<DustMephitBreathTint>().Apply(projectile.Launcher.UniqueId);
            }
            catch (Exception error)
            {
                // Presentation only: the breath and its mechanics are unaffected.
                Debug.LogException(error);
            }
        }
    }

    /// <summary>The owned recolour of one Dust Mephit cone instance.</summary>
    internal sealed class DustMephitBreathTint : MonoBehaviour
    {
        private static readonly string[] ColourProperties =
            { "_TintColor", "_Color", "_MainColor", "_ColorTint", "_EmissionColor" };

        // The product builds without UnityEngine.ParticleSystemModule, so the
        // installed particle API is reached by reflection, as the favored
        // class performance ring does.
        private static readonly Type ParticleSystemType = FindType("UnityEngine.ParticleSystem");
        private static readonly PropertyInfo MainProperty = ParticleSystemType == null ? null : ParticleSystemType.GetProperty("main");
        private static readonly PropertyInfo LifetimeProperty = ParticleSystemType == null ? null : ParticleSystemType.GetProperty("colorOverLifetime");
        private static readonly string[] ColourMembers = { "color", "colorMin", "colorMax" };
        private static readonly string[] GradientMembers = { "gradient", "gradientMin", "gradientMax" };

        private sealed class RendererState
        {
            internal Renderer Renderer;
            internal Material[] Original;
            internal Material[] Clones;
        }

        private sealed class ParticleState
        {
            internal object Module;       // boxed MainModule or ColorOverLifetimeModule
            internal PropertyInfo Colour; // startColor or color
            internal object Original;     // boxed MinMaxGradient copy
        }

        private readonly List<RendererState> _renderers = new List<RendererState>();
        private readonly List<ParticleState> _particles = new List<ParticleState>();
        private readonly HashSet<int> _seenRenderers = new HashSet<int>();
        private readonly HashSet<int> _seenSystems = new HashSet<int>();
        private bool _restored;

        // Evidence counters for the native observer: the last restored
        // instance's recolour totals and the renderers it covered.
        internal static int Applied, Restored, Live, LastMaterials, LastParticleColours;
        internal static string LastLauncher;
        internal static string[] LastRenderers = new string[0];
        internal int RecolouredMaterials, RecolouredParticleColours;

        internal void Apply(string launcher)
        {
            Applied++; Live++; LastLauncher = launcher;
            RecolourNew();
        }

        // The native cone attaches its effect children after the view is
        // created, so new renderers and particle systems are recoloured as
        // they appear, for as long as this instance is active.
        private void LateUpdate()
        {
            if (!_restored) RecolourNew();
        }

        private void RecolourNew()
        {
            if (ParticleSystemType != null && MainProperty != null)
                foreach (Component system in GetComponentsInChildren(ParticleSystemType, true))
                {
                    if (system == null || !_seenSystems.Add(system.GetInstanceID())) continue;
                    Renderer drawn = system.GetComponent<Renderer>();
                    bool subtractive = drawn != null && drawn.sharedMaterial != null &&
                        MephitBreathPolicy.IsSubtractiveMaterial(drawn.sharedMaterial.name);
                    TintModule(MainProperty.GetValue(system, null), "startColor", false, subtractive);
                    if (LifetimeProperty != null) TintModule(LifetimeProperty.GetValue(system, null), "color", true, subtractive);
                }
            foreach (Renderer renderer in GetComponentsInChildren<Renderer>(true))
            {
                if (renderer == null || !_seenRenderers.Add(renderer.GetInstanceID())) continue;
                Material[] original = renderer.sharedMaterials;
                var clones = new Material[original.Length];
                for (int index = 0; index < original.Length; index++)
                {
                    if (original[index] == null) continue;
                    clones[index] = new Material(original[index]) { name = original[index].name + "_KMG_Dust" };
                    bool subtractive = MephitBreathPolicy.IsSubtractiveMaterial(original[index].name);
                    foreach (string property in ColourProperties)
                        if (clones[index].HasProperty(property))
                            clones[index].SetColor(property, Tint(clones[index].GetColor(property), subtractive));
                    RecolouredMaterials++;
                }
                renderer.sharedMaterials = clones;
                _renderers.Add(new RendererState { Renderer = renderer, Original = original, Clones = clones });
            }
        }

        // The module structs write through to their particle system, so a
        // boxed copy is enough to read and assign the colour.
        private void TintModule(object module, string member, bool onlyIfEnabled, bool subtractive)
        {
            if (module == null) return;
            Type type = module.GetType();
            if (onlyIfEnabled)
            {
                PropertyInfo enabled = type.GetProperty("enabled");
                if (enabled == null || !(bool)enabled.GetValue(module, null)) return;
            }
            PropertyInfo colour = type.GetProperty(member);
            if (colour == null || !colour.CanRead || !colour.CanWrite) return;
            object original = colour.GetValue(module, null);
            object tinted = colour.GetValue(module, null); // a separate boxed copy
            if (original == null || tinted == null) return;
            Type gradientType = tinted.GetType();
            foreach (string name in ColourMembers)
            {
                PropertyInfo value = gradientType.GetProperty(name);
                if (value != null && value.CanWrite && value.PropertyType == typeof(Color))
                    value.SetValue(tinted, Tint((Color)value.GetValue(tinted, null), subtractive), null);
            }
            foreach (string name in GradientMembers)
            {
                PropertyInfo value = gradientType.GetProperty(name);
                if (value != null && value.CanWrite && value.PropertyType == typeof(Gradient))
                    value.SetValue(tinted, Tint((Gradient)value.GetValue(tinted, null), subtractive), null);
            }
            _particles.Add(new ParticleState { Module = module, Colour = colour, Original = original });
            colour.SetValue(module, tinted, null);
            RecolouredParticleColours++;
        }

        internal void Restore()
        {
            if (_restored) return;
            _restored = true; Restored++; Live--;
            LastMaterials = RecolouredMaterials; LastParticleColours = RecolouredParticleColours;
            var names = new List<string>();
            foreach (RendererState state in _renderers)
                if (state.Renderer != null) names.Add(state.Renderer.GetType().Name + ":" + state.Renderer.name);
            LastRenderers = names.ToArray();
            foreach (ParticleState state in _particles)
                try { state.Colour.SetValue(state.Module, state.Original, null); }
                catch (Exception) { /* the system was destroyed with its object */ }
            foreach (RendererState state in _renderers)
            {
                if (state.Renderer != null) state.Renderer.sharedMaterials = state.Original;
                foreach (Material clone in state.Clones)
                    if (clone != null) Destroy(clone);
            }
            _particles.Clear(); _renderers.Clear();
            Destroy(this);
        }

        // The native pool deactivates a released instance; a destroyed one
        // is disabled first as well.
        private void OnDisable() { Restore(); }
        private void OnDestroy() { Restore(); }

        private static Color Tint(Color value, bool subtractive)
        {
            float[] dust = subtractive ? MephitBreathPolicy.DustTintSubtractive(value.r, value.g, value.b, value.a)
                : MephitBreathPolicy.DustTint(value.r, value.g, value.b, value.a);
            return new Color(dust[0], dust[1], dust[2], dust[3]);
        }

        private static Gradient Tint(Gradient value, bool subtractive)
        {
            if (value == null) return null;
            GradientColorKey[] keys = value.colorKeys;
            for (int index = 0; index < keys.Length; index++) keys[index].color = Tint(keys[index].color, subtractive);
            var tinted = new Gradient { mode = value.mode };
            tinted.SetKeys(keys, value.alphaKeys);
            return tinted;
        }

        private static Type FindType(string fullName)
        {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type type = assembly.GetType(fullName, false);
                if (type != null) return type;
            }
            return null;
        }
    }
}
