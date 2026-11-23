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
    /// delivery, area, orientation and timing, and only the cone instance the
    /// exact owned Dust Mephit launches is presented as dust. After the native
    /// view is created, that instance gets an owned component which clones its
    /// renderers' materials and recolours the clones and its particle colours
    /// from ice to dust. When the instance is released to the native pool
    /// (deactivated) or destroyed, the component puts back the original shared
    /// materials and colours, destroys its clones and removes itself, so the
    /// next consumer of the pooled cone sees the native ice cone. No shared
    /// prefab, projectile blueprint or material is written.
    /// </summary>
    [HarmonyPatch(typeof(ProjectileController), "CreateView", new[] { typeof(Projectile), typeof(Vector3?) })]
    internal static class DustMephitBreathPresentation
    {
        private static void Postfix(Projectile projectile)
        {
            try
            {
                if (projectile == null || projectile.View == null || projectile.Launcher == null ||
                    projectile.Launcher.Blueprint == null || projectile.Blueprint == null ||
                    !MephitBreathPolicy.PresentsDustBreath(projectile.Launcher.Blueprint.AssetGuid,
                        projectile.Blueprint.AssetGuid))
                    return;
                DustMephitBreathTint existing = projectile.View.GetComponent<DustMephitBreathTint>();
                if (existing != null) existing.Restore();
                projectile.View.AddComponent<DustMephitBreathTint>().Apply(projectile.Launcher.UniqueId);
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
        private bool _restored;

        // Evidence counters for the native observer.
        internal static int Applied, Restored, Live;
        internal static string LastLauncher;
        internal int RecolouredMaterials, RecolouredParticleColours;

        internal void Apply(string launcher)
        {
            Applied++; Live++; LastLauncher = launcher;
            if (ParticleSystemType != null && MainProperty != null)
                foreach (Component system in GetComponentsInChildren(ParticleSystemType, true))
                {
                    if (system == null) continue;
                    TintModule(MainProperty.GetValue(system, null), "startColor", false);
                    if (LifetimeProperty != null) TintModule(LifetimeProperty.GetValue(system, null), "color", true);
                }
            foreach (Renderer renderer in GetComponentsInChildren<Renderer>(true))
            {
                if (renderer == null) continue;
                Material[] original = renderer.sharedMaterials;
                var clones = new Material[original.Length];
                for (int index = 0; index < original.Length; index++)
                {
                    if (original[index] == null) continue;
                    clones[index] = new Material(original[index]) { name = original[index].name + "_KMG_Dust" };
                    foreach (string property in ColourProperties)
                        if (clones[index].HasProperty(property))
                            clones[index].SetColor(property, Tint(clones[index].GetColor(property)));
                    RecolouredMaterials++;
                }
                renderer.sharedMaterials = clones;
                _renderers.Add(new RendererState { Renderer = renderer, Original = original, Clones = clones });
            }
        }

        // The module structs write through to their particle system, so a
        // boxed copy is enough to read and assign the colour.
        private void TintModule(object module, string member, bool onlyIfEnabled)
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
                    value.SetValue(tinted, Tint((Color)value.GetValue(tinted, null)), null);
            }
            foreach (string name in GradientMembers)
            {
                PropertyInfo value = gradientType.GetProperty(name);
                if (value != null && value.CanWrite && value.PropertyType == typeof(Gradient))
                    value.SetValue(tinted, Tint((Gradient)value.GetValue(tinted, null)), null);
            }
            _particles.Add(new ParticleState { Module = module, Colour = colour, Original = original });
            colour.SetValue(module, tinted, null);
            RecolouredParticleColours++;
        }

        internal void Restore()
        {
            if (_restored) return;
            _restored = true; Restored++; Live--;
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

        private static Color Tint(Color value)
        {
            float[] dust = MephitBreathPolicy.DustTint(value.r, value.g, value.b, value.a);
            return new Color(dust[0], dust[1], dust[2], dust[3]);
        }

        private static Gradient Tint(Gradient value)
        {
            if (value == null) return null;
            GradientColorKey[] keys = value.colorKeys;
            for (int index = 0; index < keys.Length; index++) keys[index].color = Tint(keys[index].color);
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
