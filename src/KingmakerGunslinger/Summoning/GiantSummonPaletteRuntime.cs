using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.View;
using Kingmaker.Visual.MaterialEffects;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.Summoning
{
    /// <summary>
    /// One proven body albedo on one exact body/material slot is transformed in
    /// memory. No native texture, shared material, mesh or transform is written.
    /// The giant attach/buff hook schedules bounded settling before application.
    /// </summary>
    internal static class GiantSummonPaletteRuntime
    {
        private sealed class Ownership
        {
            internal WeakReference View;
            internal UnitEntityData Unit;
            internal SkinnedMeshRenderer Renderer;
            internal Mesh Mesh;
            internal string RendererPath;
            internal int Slot;
            internal Material Original;
            internal Material Applied;
            internal Shader Shader;
            internal Texture2D Texture;
            internal Texture Normal;
            internal Texture Parameters;
            internal Vector2 TextureScale;
            internal Vector2 TextureOffset;
            internal readonly List<Material> Materials = new List<Material>();
            internal ProtectedSurfaces Protected;
            internal bool Foreign;
            internal bool Production;
            internal string Key;
            internal string Outcome;
            internal string Failure;
        }

        private sealed class ProtectedSurfaces
        {
            private sealed class Surface
            {
                internal Renderer Renderer;
                internal Material[] Materials;
                internal Mesh Mesh;
            }

            private sealed class Map
            {
                internal string Slot;
                internal bool Present;
                internal Texture Texture;
                internal Vector2 Scale, Offset;
            }

            private sealed class MaterialState
            {
                internal Material Material;
                internal Shader Shader;
                internal string[] Keywords;
                internal int Queue;
                internal string RenderType;
                private readonly StandardMaterialController[] _controllers;
                private readonly JArray _controllerIntake;
                internal Map[] Maps;
                internal readonly Dictionary<string, Color> Colors = new Dictionary<string, Color>();
                internal bool HasDissolve;
                internal float Dissolve;

                internal MaterialState(Material material, StandardMaterialController[] controllers)
                {
                    Material = material; Shader = material.shader;
                    Keywords = material.shaderKeywords.OrderBy(value => value, StringComparer.Ordinal).ToArray();
                    Queue = material.renderQueue;
                    RenderType = material.GetTag("RenderType", false);
                    _controllers = controllers;
                    _controllerIntake = ControllerEvidence(material);
                    Maps = new[] { "_MainTex", "_BumpMap", "_Parameters" }.Select(slot => {
                        bool present = material.HasProperty(slot);
                        return new Map { Slot = slot, Present = present,
                            Texture = present ? material.GetTexture(slot) : null,
                            Scale = present ? material.GetTextureScale(slot) : Vector2.zero,
                            Offset = present ? material.GetTextureOffset(slot) : Vector2.zero };
                    }).ToArray();
                    foreach (string slot in new[] { "_TintColor", "_Color", "_BaseColor", "_MainColor" })
                        if (material.HasProperty(slot)) Colors.Add(slot, material.GetColor(slot));
                    HasDissolve = material.HasProperty("_Dissolve");
                    Dissolve = HasDissolve ? material.GetFloat("_Dissolve") : 0f;
                    if (HasDissolve && (float.IsNaN(Dissolve) || float.IsInfinity(Dissolve) || Dissolve < 0f || Dissolve > .001f))
                        throw new InvalidOperationException("A protected material has unsettled or non-finite dissolve: " + material.name);
                }

                internal bool Matches(Material current, Texture ownedAlbedo, out string reason)
                {
                    reason = null;
                    if (current == null || !ReferenceEquals(current.shader, Shader)) reason = "shader";
                    else if (current.renderQueue != Queue || current.GetTag("RenderType", false) != RenderType ||
                        !current.shaderKeywords.OrderBy(value => value, StringComparer.Ordinal).SequenceEqual(Keywords))
                        reason = "keywords-or-renderQueue:" + QueueKeywordDifferences(current).ToString(Formatting.None);
                    else
                    {
                        foreach (Map map in Maps)
                        {
                            if (current.HasProperty(map.Slot) != map.Present) { reason = "map-presence:" + map.Slot; break; }
                            if (!map.Present) continue;
                            Texture expected = ownedAlbedo != null && map.Slot == "_MainTex" ? ownedAlbedo : map.Texture;
                            if (!ReferenceEquals(current.GetTexture(map.Slot), expected) || !Same(current.GetTextureScale(map.Slot), map.Scale) ||
                                !Same(current.GetTextureOffset(map.Slot), map.Offset)) { reason = "map-or-UV:" + map.Slot; break; }
                        }
                        if (reason == null)
                            foreach (var color in Colors)
                                if (!current.HasProperty(color.Key) || !current.GetColor(color.Key).Equals(color.Value))
                                { reason = "color:" + color.Key; break; }
                        if (reason == null && (current.HasProperty("_Dissolve") != HasDissolve ||
                            (HasDissolve && !current.GetFloat("_Dissolve").Equals(Dissolve)))) reason = "dissolve";
                    }
                    return reason == null;
                }

                // Failure-only exact native ownership/cache evidence. A queue
                // change is not admitted merely because native fade can write
                // queues: the same material and controller must be observed.
                private JObject QueueKeywordDifferences(Material current)
                {
                    string[] keywords = current.shaderKeywords.OrderBy(value => value, StringComparer.Ordinal).ToArray();
                    return new JObject { ["materialInstance"] = current.GetInstanceID(), ["materialName"] = current.name,
                        ["sameProtectedMaterialReference"] = ReferenceEquals(current, Material),
                        ["shaderInstance"] = current.shader == null ? 0 : current.shader.GetInstanceID(),
                        ["queueAtIntake"] = Queue, ["queueCurrent"] = current.renderQueue,
                        ["renderTypeAtIntake"] = RenderType, ["renderTypeCurrent"] = current.GetTag("RenderType", false),
                        ["keywordCountAtIntake"] = Keywords.Length, ["keywordCountCurrent"] = keywords.Length,
                        ["keywordsAtIntake"] = new JArray(Keywords.Take(64)), ["keywordsCurrent"] = new JArray(keywords.Take(64)),
                        ["hasDissolveAtIntake"] = HasDissolve, ["dissolveAtIntake"] = Dissolve,
                        ["dissolveCurrent"] = current.HasProperty("_Dissolve") ? (JToken)new JValue(current.GetFloat("_Dissolve")) : JValue.CreateNull(),
                        ["nativeControllerCountAtIntake"] = _controllers.Length,
                        ["nativeControllersAtIntake"] = _controllerIntake,
                        ["nativeControllersCurrent"] = ControllerEvidence(current), ["comparisonWeakened"] = false };
                }

                private JArray ControllerEvidence(Material material)
                {
                    var rows = new JArray();
                    foreach (StandardMaterialController controller in _controllers.Take(8))
                    {
                        var row = new JObject { ["controllerInstance"] = controller == null ? 0 : controller.GetInstanceID(),
                            ["alive"] = controller != null };
                        rows.Add(row);
                        if (controller == null) continue;
                        try
                        {
                            IList<Material> driven = ExpandedSummoningPteranodonViewPatch.ControllerMaterials(controller);
                            row["drivenExactMaterialCount"] = driven == null ? -1 : driven.Count(candidate => ReferenceEquals(candidate, material));
                            FieldInfo field = typeof(StandardMaterialController).GetField("m_MaterialSettings",
                                BindingFlags.Instance | BindingFlags.NonPublic);
                            var settings = field == null ? null : field.GetValue(controller) as IDictionary;
                            row["cacheAvailable"] = settings != null;
                            bool cached = settings != null && settings.Contains(material);
                            row["exactMaterialCached"] = cached;
                            FieldInfo driverField = typeof(StandardMaterialController).GetField("m_DissolveController",
                                BindingFlags.Instance | BindingFlags.NonPublic);
                            object driver = driverField == null ? null : driverField.GetValue(controller);
                            row["dissolveDriverType"] = driver == null ? null : driver.GetType().FullName;
                            FieldInfo animationsField = driver == null ? null : driver.GetType().GetField("Animations",
                                BindingFlags.Instance | BindingFlags.Public);
                            var animations = animationsField == null ? null : animationsField.GetValue(driver) as IList;
                            row["dissolveAnimationCount"] = animations == null ? -1 : animations.Count;
                            var animationRows = new JArray();
                            if (animations != null)
                                foreach (object animation in animations.Cast<object>().Take(8))
                                {
                                    var animationRow = new JObject();
                                    foreach (string name in new[] { "IsFinished", "Layer", "DissolveCutout" })
                                    {
                                        FieldInfo member = animation.GetType().GetField(name, BindingFlags.Instance | BindingFlags.Public);
                                        if (member == null) throw new NotSupportedException("Native dissolve animation field unavailable: " + name);
                                        animationRow[name] = JToken.FromObject(member.GetValue(animation));
                                    }
                                    PropertyInfo delayed = animation.GetType().GetProperty("IsDelayed", BindingFlags.Instance | BindingFlags.Public);
                                    if (delayed != null) animationRow["IsDelayed"] = JToken.FromObject(delayed.GetValue(animation, null));
                                    animationRows.Add(animationRow);
                                }
                            row["dissolveAnimations"] = animationRows;
                            if (!cached) continue;
                            object value = settings[material];
                            if (value == null || value.GetType().FullName !=
                                "Kingmaker.Visual.MaterialEffects.StandardMaterialController+MaterialSettings")
                                throw new NotSupportedException("Exact native material cache entry unavailable.");
                            var cache = new JObject();
                            foreach (string name in new[] { "RenderQueue", "RenderTypeTagValue", "IsDissolveEnabled",
                                "DissolveCutoutEnabled", "DissolveEmissionEnabled", "Dissolve" })
                            {
                                FieldInfo member = value.GetType().GetField(name, BindingFlags.Instance | BindingFlags.Public);
                                if (member == null) throw new NotSupportedException("Native cached material field unavailable: " + name);
                                object scalar = member.GetValue(value);
                                cache[name] = scalar == null ? JValue.CreateNull() : JToken.FromObject(scalar);
                            }
                            row["cachedDefaults"] = cache;
                        }
                        catch (Exception error) { row["observationError"] = error.GetType().Name + ":" + error.Message; }
                    }
                    return rows;
                }
            }

            private readonly Surface[] _surfaces;
            private readonly List<MaterialState> _materials = new List<MaterialState>();

            internal ProtectedSurfaces(UnitEntityView view)
            {
                StandardMaterialController[] controllers = view.GetComponentsInChildren<StandardMaterialController>(true);
                _surfaces = view.GetComponentsInChildren<Renderer>(true).Select(renderer => new Surface {
                    Renderer = renderer, Materials = renderer.sharedMaterials,
                    Mesh = renderer is SkinnedMeshRenderer ? ((SkinnedMeshRenderer)renderer).sharedMesh :
                        (renderer.GetComponent<MeshFilter>() == null ? null : renderer.GetComponent<MeshFilter>().sharedMesh) }).ToArray();
                foreach (Material material in _surfaces.SelectMany(surface => surface.Materials).Where(value => value != null))
                    if (!_materials.Any(value => ReferenceEquals(value.Material, material))) _materials.Add(new MaterialState(material, controllers));
            }

            internal bool Verify(Ownership owner, bool allowOwnedSlot, out string reason)
            {
                // Detached originals remain protected too: native ReinitMaterials
                // first resets its previous material collection before Awake.
                foreach (MaterialState material in _materials)
                    if (!material.Matches(material.Material, null, out reason))
                    { reason = "original-material:" + (material.Material == null ? "<destroyed>" : material.Material.name) + ":" + reason; return false; }
                foreach (Surface surface in _surfaces)
                {
                    if (surface.Renderer == null) { reason = "source-renderer-destroyed"; return false; }
                    Mesh mesh = surface.Renderer is SkinnedMeshRenderer ? ((SkinnedMeshRenderer)surface.Renderer).sharedMesh :
                        (surface.Renderer.GetComponent<MeshFilter>() == null ? null : surface.Renderer.GetComponent<MeshFilter>().sharedMesh);
                    if (!ReferenceEquals(mesh, surface.Mesh)) { reason = "source-renderer-mesh"; return false; }
                    Material[] current = surface.Renderer.sharedMaterials;
                    if (current.Length != surface.Materials.Length) { reason = "source-material-array-length"; return false; }
                    for (int i = 0; i < current.Length; i++)
                    {
                        bool ownSlot = allowOwnedSlot && ReferenceEquals(surface.Renderer, owner.Renderer) && i == owner.Slot;
                        Material expected = ownSlot ? owner.Applied : surface.Materials[i];
                        if (!ReferenceEquals(current[i], expected)) { reason = "source-material-reference:" + surface.Renderer.name + ":" + i; return false; }
                        if (ownSlot)
                        {
                            MaterialState original = _materials.Single(value => ReferenceEquals(value.Material, owner.Original));
                            if (!original.Matches(current[i], owner.Texture, out reason))
                            { reason = "owned-body-material:" + reason; return false; }
                        }
                    }
                }
                reason = null; return true;
            }
        }

        private static readonly Dictionary<string, GiantPaletteAlbedoProof> Proofs =
            new Dictionary<string, GiantPaletteAlbedoProof>(StringComparer.Ordinal);
        private static readonly ConditionalWeakTable<UnitEntityView, Ownership> States =
            new ConditionalWeakTable<UnitEntityView, Ownership>();
        private static readonly List<Ownership> Live = new List<Ownership>();
        private const string OwnedPrefix = "KMG_GiantPalette_";
        private static string _gammaCopyProof = "not-observed";

        internal static string ProveGammaCopy()
        {
            if (_gammaCopyProof != "not-observed") return _gammaCopyProof;
            if (QualitySettings.activeColorSpace != ColorSpace.Gamma)
                return "unproven-non-gamma";
            Texture2D sample = null;
            try
            {
                // A project-owned non-symmetric RGBA pattern proves channel,
                // alpha and row orientation on this actual graphics context.
                var expected = new[] { new Color32(7, 53, 211, 17), new Color32(239, 31, 83, 53),
                    new Color32(41, 193, 11, 123), new Color32(101, 61, 227, 255) };
                sample = new Texture2D(2, 2, TextureFormat.RGBA32, false, false);
                sample.SetPixels32(expected); sample.Apply(false, false);
                Color32[] actual = ReadGammaPixels(sample);
                bool same = actual.Length == expected.Length && actual.Select((pixel, index) =>
                    Math.Abs(pixel.r - expected[index].r) <= 1 && Math.Abs(pixel.g - expected[index].g) <= 1 &&
                    Math.Abs(pixel.b - expected[index].b) <= 1 && pixel.a == expected[index].a).All(value => value);
                return _gammaCopyProof = same ? "gamma-rgba-copy-proved" : "gamma-rgba-copy-mismatch";
            }
            catch (Exception error) { return _gammaCopyProof = "gamma-rgba-copy-failed:" + error.GetType().Name; }
            finally { Destroy(sample); }
        }

        private static Color32[] ReadGammaPixels(Texture2D source)
        {
            if (QualitySettings.activeColorSpace != ColorSpace.Gamma)
                throw new InvalidOperationException("Gamma readback cannot infer a Linear texture encoding.");
            RenderTexture previous = RenderTexture.active, target = null;
            bool previousSrgbWrite = GL.sRGBWrite;
            Texture2D readable = null;
            try
            {
                target = RenderTexture.GetTemporary(source.width, source.height, 0,
                    RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
                if (target == null || target.sRGB) throw new InvalidOperationException("No non-sRGB copy target.");
                GL.sRGBWrite = false;
                Graphics.Blit(source, target);
                RenderTexture.active = target;
                readable = new Texture2D(source.width, source.height, TextureFormat.RGBA32, false, false);
                readable.ReadPixels(new Rect(0, 0, source.width, source.height), 0, 0, false);
                readable.Apply(false, false);
                return readable.GetPixels32();
            }
            finally
            {
                RenderTexture.active = previous;
                GL.sRGBWrite = previousSrgbWrite;
                Destroy(readable);
                if (target != null) RenderTexture.ReleaseTemporary(target);
            }
        }

        internal static void RegisterProof(GiantPaletteAlbedoProof proof)
        {
            string decision = GiantSummonPalettePolicy.ValidateProof(proof);
            if (decision != "proven-body-albedo")
                throw new InvalidOperationException("Giant palette proof refused: " + decision);
            GiantPaletteAlbedoProof existing;
            if (Proofs.TryGetValue(proof.Key, out existing))
            {
                if (ReferenceEquals(existing, proof)) return;
                throw new InvalidOperationException("Giant palette proof already fixed: " + proof.Key);
            }
            Proofs.Add(proof.Key, proof);
        }

        internal static string Describe(UnitEntityView view)
        {
            Ownership state;
            if (view != null && States.TryGetValue(view, out state)) return state.Outcome;
            GiantSummonPaletteLifetime lifetime = view == null ? null : view.GetComponent<GiantSummonPaletteLifetime>();
            return lifetime != null && lifetime.Outcome != null ? lifetime.Outcome :
                "palette:not-applied;proofs=" + Proofs.Count;
        }

        internal static void Schedule(UnitEntityView view, GiantSummonPresentationProfile profile)
        {
            if (view == null || profile == null ||
                GiantSummonPalettePolicy.ProductionProof(profile.Key) == null ||
                !ExpandedSummoningVisualVariantPatch.IsModuleActive) return;
            GiantSummonPaletteLifetime lifetime = view.GetComponent<GiantSummonPaletteLifetime>();
            if (lifetime == null) lifetime = view.gameObject.AddComponent<GiantSummonPaletteLifetime>();
            lifetime.Schedule(view);
        }

        // Observation only: no ownership acquisition, material writes,
        // controller reinitialization or cleanup lease is created here.
        internal static string VerifyProductionOwnership(UnitEntityView view)
        {
            try
            {
                if (!ExpandedSummoningVisualVariantPatch.IsModuleActive) return "palette:module-off";
                Ownership state;
                if (view == null || !States.TryGetValue(view, out state)) return "palette:no-production-owner";
                if (!state.Production || state.Foreign || !ReferenceEquals(view.EntityData, state.Unit))
                    return "palette:production-owner-identity-mismatch";
                string source = SummonedSourceGuid(state.Unit);
                GiantSummonPresentationProfile profile = GiantSummonPresentationPolicy.Resolve(true,
                    state.Unit.Blueprint.AssetGuid, source);
                if (source == null || profile == null || profile.Key != state.Key ||
                    GiantSummonPalettePolicy.ProductionProof(profile.Key) == null)
                    return "palette:production-summon-provenance-mismatch";
                if (!StillOwnsSlot(state)) return "palette:production-live-slot-mismatch";
                string reason;
                if (!state.Protected.Verify(state, true, out reason))
                    return "palette:production-protected-surface-mismatch:" + reason;
                return "palette:verified-production-owner;key=" + state.Key +
                    ";renderer=" + state.RendererPath + ";slot=" + state.Slot +
                    ";material=" + state.Applied.name + ";texture=" + state.Texture.name +
                    ";dimensions=" + state.Texture.width + "x" + state.Texture.height;
            }
            catch (Exception error) { return "palette:production-verification-exception:" + error.GetType().Name; }
        }

        internal static string Apply(UnitEntityView view)
        {
            if (!ExpandedSummoningVisualVariantPatch.IsModuleActive) return "palette:module-off";
            UnitEntityData unit = view == null ? null : view.EntityData;
            if (unit == null || unit.Blueprint == null) return "palette:no-unit";
            GiantSummonPresentationProfile profile = GiantSummonPresentationPolicy.Resolve(true,
                unit.Blueprint.AssetGuid, SummonedSourceGuid(unit));
            if (profile == null) return "palette:not-owned";
            if (SummonedSourceGuid(unit) == null) return "palette:no-live-summon-context";
            Ownership state;
            if (States.TryGetValue(view, out state))
            {
                if (state.Foreign) return state.Outcome = "palette:foreign-ownership-refused";
                if (!ReferenceEquals(state.Unit, unit) || !StillOwnsSlot(state))
                {
                    state.Foreign = true;
                    Release(view, "foreign-refresh", true);
                    // Keep a refusal marker after disposing unbound owned
                    // objects; retries cannot adopt a foreign model refresh.
                    Ownership remaining;
                    if (!States.TryGetValue(view, out remaining)) States.Add(view, state);
                    return state.Outcome = "palette:foreign-renderer-or-material-refresh-refused";
                }
                return state.Outcome = "palette:already-applied";
            }
            GiantPaletteAlbedoProof proof;
            bool production = !Proofs.TryGetValue(profile.Key, out proof);
            if (production) proof = GiantSummonPalettePolicy.ProductionProof(profile.Key);
            if (proof == null) return "palette:missing-albedo-proof;key=" + profile.Key;
            GiantPaletteSelector selector = GiantSummonPalettePolicy.Selector(profile.Key);
            if (selector == null || unit.Blueprint.Prefab == null ||
                unit.Blueprint.Prefab.AssetId != proof.PrefabId) return "palette:prefab-identity-mismatch";
            // The installed Texture API exposes no color-space getter. CPU
            // encoded-channel treatment is admitted only in Gamma rendering;
            // Linear sampling needs a separately proven import/encoding seam.
            if (QualitySettings.activeColorSpace != ColorSpace.Gamma)
                return "palette:encoded-albedo-color-space-unproven";
            // Never stack over the old all-renderer variant owner. The integrator
            // must remove a superseded giant registration before activation.
            if (ExpandedSummoningVisualVariantPatch.DescribeOwnership(view) != "<none>")
                return "palette:existing-variant-owner-refused";
            // Reinitializing the native material controller resets every
            // renderer it drives. Wait for the complete source view to settle,
            // rather than accidentally cancelling another renderer's spawn FX.
            foreach (Renderer sourceRenderer in view.GetComponentsInChildren<Renderer>(true))
                foreach (Material sourceMaterial in sourceRenderer.sharedMaterials)
                    if (sourceMaterial != null && sourceMaterial.HasProperty("_Dissolve"))
                    {
                        float dissolve = sourceMaterial.GetFloat("_Dissolve");
                        if (float.IsNaN(dissolve) || float.IsInfinity(dissolve) || dissolve < 0f)
                            return "palette:invalid-source-dissolve";
                        if (dissolve > .001f) return "palette:await-settled-dissolve";
                    }
            SkinnedMeshRenderer[] matches = view.GetComponentsInChildren<SkinnedMeshRenderer>(true)
                .Where(value => value != null && value.name == selector.Renderer &&
                    Path(view.transform, value.transform) == proof.RendererPath).ToArray();
            if (matches.Length != 1) return "palette:exact-renderer-not-unique";
            SkinnedMeshRenderer renderer = matches[0];
            Material[] originals = renderer.sharedMaterials;
            if (renderer.sharedMesh == null || originals == null || proof.MaterialSlot >= originals.Length)
                return "palette:missing-mesh-or-material-slot";
            Material original = originals[proof.MaterialSlot];
            if (original == null || !GiantSummonPalettePolicy.MaterialNameMatches(original.name, selector.Material) ||
                original.shader == null || original.shader.name != GiantSummonPalettePolicy.ShaderName ||
                !original.HasProperty(GiantSummonPalettePolicy.AlbedoSlot) ||
                !original.HasProperty(GiantSummonPalettePolicy.TintSlot)) return "palette:material-identity-mismatch";
            Color tint = original.GetColor(GiantSummonPalettePolicy.TintSlot);
            if (tint.r != 1f || tint.g != 1f || tint.b != 1f || tint.a != 1f)
                return "palette:nonneutral-or-active-tint-refused";
            if (original.HasProperty("_Dissolve") && original.GetFloat("_Dissolve") > 0.001f)
                return "palette:await-settled-dissolve";
            Texture2D source = original.GetTexture(GiantSummonPalettePolicy.AlbedoSlot) as Texture2D;
            if (source == null || source.name != proof.TextureName || source.width != proof.Width ||
                source.height != proof.Height || source.format.ToString() != proof.TextureFormat)
                return "palette:albedo-identity-mismatch";
            Color32[] pixels;
            string readMode;
            try
            {
                if (source.isReadable) { pixels = source.GetPixels32(); readMode = "cpu-readable"; }
                else
                {
                    if (ProveGammaCopy() != "gamma-rgba-copy-proved") return "palette:gamma-copy-unproved";
                    pixels = ReadGammaPixels(source); readMode = "gpu-gamma-copy";
                }
            }
            catch (Exception) { return "palette:source-albedo-copy-failed"; }
            if (pixels == null || pixels.Length != proof.Width * proof.Height)
                return "palette:source-pixel-count-mismatch";
            for (int index = 0; index < pixels.Length; index++)
            {
                Color32 pixel = pixels[index];
                GiantPalettePixel result = GiantSummonPalettePolicy.Transform(profile.Key,
                    new GiantPalettePixel(pixel.r / 255f, pixel.g / 255f, pixel.b / 255f, pixel.a / 255f));
                // Assign original alpha byte verbatim; no opacity adjustment.
                pixels[index] = new Color32(Byte(result.Red), Byte(result.Green), Byte(result.Blue), pixel.a);
            }
            state = new Ownership { View = new WeakReference(view), Unit = unit, Renderer = renderer,
                Production = production, Key = profile.Key,
                Mesh = renderer.sharedMesh, RendererPath = proof.RendererPath,
                Slot = proof.MaterialSlot, Original = original,
                Shader = original.shader,
                Normal = original.HasProperty("_BumpMap") ? original.GetTexture("_BumpMap") : null,
                Parameters = original.HasProperty("_Parameters") ? original.GetTexture("_Parameters") : null,
                TextureScale = original.GetTextureScale(GiantSummonPalettePolicy.AlbedoSlot),
                TextureOffset = original.GetTextureOffset(GiantSummonPalettePolicy.AlbedoSlot),
                Protected = new ProtectedSurfaces(view) };
            States.Add(view, state); Live.Add(state);
            try
            {
                // Install teardown before any owned allocation or renderer swap,
                // including a failed attach that leaves a foreign-bound object.
                GiantSummonPaletteLifetime lifetime = view.GetComponent<GiantSummonPaletteLifetime>();
                if (lifetime == null) lifetime = view.gameObject.AddComponent<GiantSummonPaletteLifetime>();
                lifetime.View = view;
                state.Texture = new Texture2D(source.width, source.height, TextureFormat.RGBA32,
                    source.mipmapCount > 1);
                string identity = OwnedPrefix + Guid.NewGuid().ToString("N");
                state.Texture.name = identity + "_Albedo";
                state.Texture.filterMode = source.filterMode;
                state.Texture.wrapMode = source.wrapMode;
                state.Texture.anisoLevel = source.anisoLevel;
                state.Texture.SetPixels32(pixels);
                state.Texture.Apply(source.mipmapCount > 1, false);
                var replacement = new Material(original);
                state.Materials.Add(replacement);
                replacement.name = identity;
                replacement.SetTexture(GiantSummonPalettePolicy.AlbedoSlot, state.Texture);
                // Material cloning retains all UV transforms, maps, tint and
                // other native properties. Only this albedo reference changes.
                state.Applied = replacement;
                var replacements = (Material[])originals.Clone();
                replacements[state.Slot] = replacement;
                renderer.sharedMaterials = replacements;
                string controller = ExpandedSummoningPteranodonViewPatch.ReinitializeMaterialController(view);
                Material[] driven = renderer.sharedMaterials;
                Material current = driven != null && state.Slot < driven.Length ? driven[state.Slot] : null;
                // Adopt only an instance of this unique clone with its private
                // albedo. A prefix alone never establishes foreign ownership.
                if (current != null && (ReferenceEquals(current, replacement) ||
                    (current.name == identity + " (Instance)" &&
                    ReferenceEquals(current.GetTexture(GiantSummonPalettePolicy.AlbedoSlot), state.Texture) &&
                    ReferenceEquals(current.shader, original.shader))))
                {
                    state.Applied = current;
                    if (!state.Materials.Contains(current)) state.Materials.Add(current);
                }
                if (controller != "reinitialized" || !StillOwnsSlot(state) || driven.Length != originals.Length ||
                    Enumerable.Range(0, originals.Length).Any(index => index != state.Slot &&
                        !ReferenceEquals(originals[index], driven[index])))
                    throw new InvalidOperationException("Native controller did not retain exact slot ownership.");
                string protectionFailure;
                if (!state.Protected.Verify(state, true, out protectionFailure))
                    throw new InvalidOperationException("Protected source state changed during palette apply: " + protectionFailure);
                return state.Outcome = "palette:applied;key=" + profile.Key + ";renderer=" + proof.RendererPath +
                    ";readMode=" + readMode + ";gammaCopy=" + _gammaCopyProof +
                    ";slot=" + proof.MaterialSlot + ";identityEvidence=" + proof.IdentityEvidence +
                    ";wholeBodyPaletteEvidence=" + proof.BodyPaletteEvidence;
            }
            catch (Exception exception)
            {
                state.Failure = "attach-failed:" + exception.GetType().Name + ":" + exception.Message;
                string cleanup = Release(view, "attach-failed", true);
                return state.Outcome = "palette:" + state.Failure + ";cleanup=" + cleanup;
            }
        }

        internal static string Release(UnitEntityView view, string reason, bool restore)
        {
            Ownership state;
            if (ReferenceEquals(view, null) || !States.TryGetValue(view, out state)) return "palette:not-owned";
            bool owned = StillOwnsSlot(state);
            bool restored = false;
            string verificationFailure = null;
            if (restore && owned)
            {
                // A global native reinit must not reset foreign edits on any
                // protected source material merely to restore our one slot.
                if (!state.Protected.Verify(state, true, out verificationFailure))
                    verificationFailure = "before-release:" + verificationFailure;
                else
                {
                    try
                    {
                        Material[] current = state.Renderer.sharedMaterials;
                        current[state.Slot] = state.Original;
                        state.Renderer.sharedMaterials = current;
                        string controller = ExpandedSummoningPteranodonViewPatch.ReinitializeMaterialController(view);
                        if (controller != "reinitialized") verificationFailure = "controller:" + controller;
                        else if (!state.Protected.Verify(state, false, out verificationFailure))
                            verificationFailure = "after-release:" + verificationFailure;
                        else restored = true;
                    }
                    catch (Exception exception) { verificationFailure = "release-exception:" + exception.GetType().Name + ":" + exception.Message; }
                }
            }
            else if (restore) verificationFailure = "foreign-or-missing-owned-slot";
            if (restore && !restored && state.Failure == null) state.Failure = verificationFailure ?? "restoration-unverified";
            // A failed native reinit may still have cached these materials even
            // when the renderer swap succeeded. Retain all ownership until an
            // explicit verified release or the owned view's teardown.
            bool retainFailedRestore = restore && !restored;
            // Foreign changes are preserved. Do not destroy any owned object
            // still bound elsewhere; retain the record so teardown can retry.
            Renderer[] renderers = Resources.FindObjectsOfTypeAll<Renderer>();
            int retained = 0;
            foreach (Material material in state.Materials.ToArray())
            {
                if (material == null) { state.Materials.Remove(material); continue; }
                bool used = retainFailedRestore || renderers.Any(value => value != null &&
                    !(!restore && ReferenceEquals(value, state.Renderer) && owned) &&
                    value.sharedMaterials.Any(candidate => ReferenceEquals(candidate, material)));
                if (used) { retained++; continue; }
                Destroy(material); state.Materials.Remove(material);
            }
            if (state.Texture != null)
            {
                bool used = retainFailedRestore || Resources.FindObjectsOfTypeAll<Material>().Any(material => material != null &&
                    new[] { "_MainTex", "_BumpMap", "_Parameters" }.Any(slot => material.HasProperty(slot) &&
                        ReferenceEquals(material.GetTexture(slot), state.Texture)));
                if (used) retained++;
                else { Destroy(state.Texture); state.Texture = null; }
            }
            state.Outcome = "palette:" + reason + ";restored=" + restored +
                ";foreignPreserved=" + (!owned) + ";retainedBoundObjects=" + retained +
                ";verificationFailure=" + (verificationFailure ?? "<none>") + ";firstFailure=" + (state.Failure ?? "<none>");
            if (retained == 0 && !retainFailedRestore) { States.Remove(view); Live.Remove(state); }
            return state.Outcome;
        }

        internal static string ReleaseAll(string reason)
        {
            foreach (Ownership state in Live.ToArray())
            {
                UnitEntityView view = state.View.Target as UnitEntityView;
                if (!ReferenceEquals(view, null)) Release(view, reason, view != null);
            }
            return "palette:remainingOwnerships=" + Live.Count;
        }

        private static bool StillOwnsSlot(Ownership state)
        {
            if (state.Renderer == null || state.Original == null ||
                !ReferenceEquals(state.Renderer.sharedMesh, state.Mesh)) return false;
            UnitEntityView view = state.View.Target as UnitEntityView;
            if (view == null || Path(view.transform, state.Renderer.transform) != state.RendererPath) return false;
            Material[] current = state.Renderer.sharedMaterials;
            return current != null && state.Slot < current.Length &&
                state.Applied != null && ReferenceEquals(current[state.Slot], state.Applied) &&
                ReferenceEquals(state.Applied.shader, state.Shader) &&
                ReferenceEquals(state.Applied.GetTexture(GiantSummonPalettePolicy.AlbedoSlot), state.Texture) &&
                Same(state.Applied.GetTextureScale(GiantSummonPalettePolicy.AlbedoSlot), state.TextureScale) &&
                Same(state.Applied.GetTextureOffset(GiantSummonPalettePolicy.AlbedoSlot), state.TextureOffset) &&
                (!state.Applied.HasProperty("_BumpMap") || ReferenceEquals(state.Applied.GetTexture("_BumpMap"), state.Normal)) &&
                (!state.Applied.HasProperty("_Parameters") || ReferenceEquals(state.Applied.GetTexture("_Parameters"), state.Parameters));
        }

        private static byte Byte(float channel)
        { return (byte)Mathf.Clamp(Mathf.RoundToInt(channel * 255f), 0, 255); }

        private static bool Same(Vector2 left, Vector2 right)
        { return left.x == right.x && left.y == right.y; }

        private static void Destroy(UnityEngine.Object value)
        { if (value != null) UnityEngine.Object.DestroyImmediate(value); }

        private static string Path(Transform root, Transform child)
        {
            var nodes = new List<string>();
            for (Transform node = child; node != null && node != root; node = node.parent)
                nodes.Add(node.name + "[" + node.GetSiblingIndex() + "]");
            nodes.Reverse();
            return string.Join("/", nodes.ToArray());
        }

        private static string SummonedSourceGuid(UnitEntityData unit)
        {
            BlueprintRoot root = BlueprintRoot.Instance;
            var summoned = root == null || root.SystemMechanics == null ? null : root.SystemMechanics.SummonedUnitBuff;
            if (summoned == null || unit.Descriptor == null) return null;
            Buff match = null;
            foreach (Buff buff in unit.Descriptor.Buffs)
            {
                if (buff == null || buff.Blueprint == null || buff.Blueprint.AssetGuid != summoned.AssetGuid) continue;
                if (match != null) return null;
                match = buff;
            }
            return match == null || match.Context == null || match.Context.SourceAbility == null
                ? null : match.Context.SourceAbility.AssetGuid;
        }
    }

    internal sealed class GiantSummonPaletteLifetime : MonoBehaviour
    {
        internal UnitEntityView View;
        internal string Outcome;
        private UnitEntityData _owner;
        private float _deadline, _next;
        private bool _pending, _moduleOffReleased;

        internal void Schedule(UnitEntityView view)
        {
            if (ReferenceEquals(View, view) && ReferenceEquals(_owner, view.EntityData)) return;
            // A reused component cannot grant ownership of a new model/unit.
            if (View != null) return;
            View = view; _owner = view.EntityData;
            _deadline = Time.realtimeSinceStartup + 15f;
            _next = 0f; _pending = true;
            Outcome = "palette:await-native-settlement";
        }

        private void LateUpdate()
        {
            if (View == null || !ReferenceEquals(View.EntityData, _owner)) { _pending = false; return; }
            if (!ExpandedSummoningVisualVariantPatch.IsModuleActive)
            {
                _pending = false;
                if (!_moduleOffReleased)
                {
                    _moduleOffReleased = true;
                    Outcome = GiantSummonPaletteRuntime.Release(View, "module-off", true);
                }
                return;
            }
            if (_moduleOffReleased)
            {
                _moduleOffReleased = false; _pending = true;
                _deadline = Time.realtimeSinceStartup + 15f;
                Outcome = "palette:await-native-settlement";
            }
            if (!_pending || Time.realtimeSinceStartup < _next) return;
            _next = Time.realtimeSinceStartup + .25f;
            try { Outcome = GiantSummonPaletteRuntime.Apply(View); }
            catch (Exception error) { Outcome = "palette:apply-exception:" + error.GetType().Name; }
            // Only native spawn settlement is retried. Identity/foreign-state
            // refusals stay observable and never become a forceful repair.
            _pending = Outcome == "palette:await-settled-dissolve" ||
                Outcome == "palette:no-live-summon-context";
            if (_pending && Time.realtimeSinceStartup >= _deadline)
            { _pending = false; Outcome += ";settlement-timeout"; }
        }

        private void OnDestroy()
        {
            try { GiantSummonPaletteRuntime.Release(View, "view-destroyed", false); }
            catch (Exception) { }
            View = null;
        }
    }
}
