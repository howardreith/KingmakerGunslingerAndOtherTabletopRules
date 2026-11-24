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
    /// delivery, area, orientation and timing; only the cone the owned Dust
    /// breath itself launches is presented as dust. The visible cone is the
    /// projectile blueprint's cast effect, which native
    /// <c>ProjectileController.CreateView</c> spawns through
    /// <c>FxHelper.SpawnFxOnPoint</c> beside the renderer-less projectile
    /// view. The executing ability is read from the native
    /// <c>AbilityExecutionProcess.Tick</c> that delivers the projectile (the
    /// launch, CreateView and the spawn run synchronously inside it), so the
    /// recolour requires the owned Dust breath execution, not merely the Dust
    /// unit. The instance spawned from that projectile's own cast-effect prefab
    /// in that CreateView gets an owned component which clones its materials
    /// and recolours the clones and its particle colours from ice to dust; when
    /// the instance is released to the native pool or destroyed, the component
    /// restores the originals, destroys its clones and removes itself. Native
    /// CreateView skips a cast effect only for a second launch from the same
    /// point within one frame (the controller clears its previous launch point
    /// at the end of every tick), so separate executions are never suppressed
    /// and nothing is spawned here. No shared prefab, projectile blueprint or
    /// material is written.
    /// </summary>
    [HarmonyPatch(typeof(Kingmaker.Controllers.AbilityExecutionProcess), "Tick")]
    internal static class DustMephitBreathExecutionScope
    {
        internal static Kingmaker.Controllers.AbilityExecutionProcess Current;
        internal static int Frame = -1;
        internal static readonly DustCreationTracker Creations = new DustCreationTracker();

        private sealed class Saved { internal Kingmaker.Controllers.AbilityExecutionProcess Process; internal int Frame; internal object Scope; }

        // A nested execution tick restores the enclosing one when it returns.
        // The native tick catches every exception its delivery raises, so the
        // postfix always runs and ends the creation scope.
        private static void Prefix(Kingmaker.Controllers.AbilityExecutionProcess __instance, out object __state)
        {
            __state = new Saved { Process = Current, Frame = Frame, Scope = Creations.BeginScope(Time.frameCount) };
            Current = __instance; Frame = Time.frameCount;
        }

        private static void Postfix(object __state)
        {
            var saved = (Saved)__state;
            Creations.EndScope(saved.Scope);
            Current = saved.Process; Frame = saved.Frame;
        }

        /// <summary>The ability blueprint executing this frame for this launcher, if any.</summary>
        internal static string ExecutingAbility(Kingmaker.EntitySystem.Entities.UnitEntityData launcher)
        {
            var process = Current;
            if (process == null || Frame != Time.frameCount || process.Context == null ||
                !ReferenceEquals(process.Context.MaybeCaster, launcher) || process.Context.AbilityBlueprint == null)
                return null;
            return process.Context.AbilityBlueprint.AssetGuid;
        }
    }

    [HarmonyPatch(typeof(ProjectileController), "CreateView", new[] { typeof(Projectile), typeof(Vector3?) })]
    internal static class DustMephitBreathPresentation
    {
        internal static int Unadopted;

        // Every creation inside an execution scope is tracked (owned or not),
        // so a nested foreign creation can never lend an enclosing Dust identity.
        private static void Prefix(Projectile projectile, out object __state)
        {
            __state = null;
            try
            {
                bool owned = projectile != null && projectile.Launcher != null && projectile.Launcher.Blueprint != null &&
                    projectile.Blueprint != null && MephitBreathPolicy.PresentsDustBreath(projectile.Launcher.Blueprint.AssetGuid,
                        DustMephitBreathExecutionScope.ExecutingAbility(projectile.Launcher), projectile.Blueprint.AssetGuid);
                __state = DustMephitBreathExecutionScope.Creations.BeginCreation(projectile, owned, Time.frameCount);
            }
            catch (Exception error) { Debug.LogException(error); }
        }

        // A creation whose cast effect was not adopted (native skipped it or
        // the blueprint has none) is only counted.
        private static void Postfix(object __state)
        {
            if (DustMephitBreathExecutionScope.Creations.EndCreation(__state)) Unadopted++;
        }
    }

    [HarmonyPatch(typeof(Kingmaker.Visual.Particles.FxHelper), "SpawnFxOnPoint",
        new[] { typeof(GameObject), typeof(Vector3), typeof(Quaternion) })]
    internal static class DustMephitBreathCastFxPatch
    {
        private static void Postfix(GameObject prefab, Vector3 point, GameObject __result)
        {
            try
            {
                if (__result == null || prefab == null) return;
                var creations = DustMephitBreathExecutionScope.Creations;
                var projectile = creations.Peek(Time.frameCount) as Projectile;
                // Only this creation's own cast effect, at its own launch point.
                if (projectile == null || !ReferenceEquals(prefab, projectile.Blueprint.CastFx.Load()) ||
                    point != projectile.LaunchPosition) return;
                if (creations.Adopt(Time.frameCount) == null) return;
                DustMephitBreathTint.Adopt(__result, projectile.Launcher.UniqueId);
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

        internal static void Adopt(GameObject instance, string launcher)
        {
            DustMephitBreathTint existing = instance.GetComponent<DustMephitBreathTint>();
            if (existing != null) existing.Restore();
            instance.AddComponent<DustMephitBreathTint>().Apply(launcher);
        }

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
