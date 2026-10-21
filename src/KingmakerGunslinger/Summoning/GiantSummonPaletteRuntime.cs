using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.View;
using UnityEngine;

namespace KingmakerGunslinger.Summoning
{
    /// <summary>
    /// Inactive until a curated proof is registered by product source. One
    /// proven body albedo on one exact body/material slot is transformed in
    /// memory. No native texture, shared material, mesh or transform is written.
    /// This helper installs no hook and registers no default proof.
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
            internal bool Foreign;
            internal string Outcome;
        }

        private static readonly Dictionary<string, GiantPaletteAlbedoProof> Proofs =
            new Dictionary<string, GiantPaletteAlbedoProof>(StringComparer.Ordinal);
        private static readonly ConditionalWeakTable<UnitEntityView, Ownership> States =
            new ConditionalWeakTable<UnitEntityView, Ownership>();
        private static readonly List<Ownership> Live = new List<Ownership>();
        private const string OwnedPrefix = "KMG_GiantPalette_";

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
            return view != null && States.TryGetValue(view, out state)
                ? state.Outcome : "palette:not-applied;proofs=" + Proofs.Count;
        }

        internal static string Apply(UnitEntityView view)
        {
            if (!ExpandedSummoningVisualVariantPatch.IsModuleActive) return "palette:module-off";
            UnitEntityData unit = view == null ? null : view.EntityData;
            if (unit == null || unit.Blueprint == null) return "palette:no-unit";
            GiantSummonPresentationProfile profile = GiantSummonPresentationPolicy.Resolve(true,
                unit.Blueprint.AssetGuid, SummonedSourceGuid(unit));
            if (profile == null) return "palette:not-owned";
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
            if (!Proofs.TryGetValue(profile.Key, out proof)) return "palette:missing-albedo-proof;key=" + profile.Key;
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
            try { pixels = source.GetPixels32(); }
            catch (Exception) { return "palette:source-albedo-not-readable"; }
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
                Mesh = renderer.sharedMesh, RendererPath = proof.RendererPath,
                Slot = proof.MaterialSlot, Original = original,
                Shader = original.shader,
                Normal = original.HasProperty("_BumpMap") ? original.GetTexture("_BumpMap") : null,
                Parameters = original.HasProperty("_Parameters") ? original.GetTexture("_Parameters") : null,
                TextureScale = original.GetTextureScale(GiantSummonPalettePolicy.AlbedoSlot),
                TextureOffset = original.GetTextureOffset(GiantSummonPalettePolicy.AlbedoSlot) };
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
                return state.Outcome = "palette:applied;key=" + profile.Key + ";renderer=" + proof.RendererPath +
                    ";slot=" + proof.MaterialSlot + ";identityEvidence=" + proof.IdentityEvidence +
                    ";wholeBodyPaletteEvidence=" + proof.BodyPaletteEvidence;
            }
            catch (Exception exception)
            {
                Release(view, "attach-failed", true);
                return "palette:attach-failed:" + exception.GetType().Name;
            }
        }

        internal static string Release(UnitEntityView view, string reason, bool restore)
        {
            Ownership state;
            if (ReferenceEquals(view, null) || !States.TryGetValue(view, out state)) return "palette:not-owned";
            bool owned = StillOwnsSlot(state);
            if (restore && owned)
            {
                Material[] current = state.Renderer.sharedMaterials;
                current[state.Slot] = state.Original;
                state.Renderer.sharedMaterials = current;
                ExpandedSummoningPteranodonViewPatch.ReinitializeMaterialController(view);
            }
            // Foreign changes are preserved. Do not destroy any owned object
            // still bound elsewhere; retain the record so teardown can retry.
            Renderer[] renderers = Resources.FindObjectsOfTypeAll<Renderer>();
            int retained = 0;
            foreach (Material material in state.Materials.ToArray())
            {
                if (material == null) { state.Materials.Remove(material); continue; }
                bool used = renderers.Any(value => value != null &&
                    !(!restore && ReferenceEquals(value, state.Renderer) && owned) &&
                    value.sharedMaterials.Any(candidate => ReferenceEquals(candidate, material)));
                if (used) { retained++; continue; }
                Destroy(material); state.Materials.Remove(material);
            }
            if (state.Texture != null)
            {
                bool used = Resources.FindObjectsOfTypeAll<Material>().Any(material => material != null &&
                    new[] { "_MainTex", "_BumpMap", "_Parameters" }.Any(slot => material.HasProperty(slot) &&
                        ReferenceEquals(material.GetTexture(slot), state.Texture)));
                if (used) retained++;
                else { Destroy(state.Texture); state.Texture = null; }
            }
            state.Outcome = "palette:" + reason + ";restored=" + (restore && owned) +
                ";foreignPreserved=" + (!owned) + ";retainedBoundObjects=" + retained;
            if (retained == 0) { States.Remove(view); Live.Remove(state); }
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
        private void OnDestroy()
        {
            try { GiantSummonPaletteRuntime.Release(View, "view-destroyed", false); }
            catch (Exception) { }
            View = null;
        }
    }
}
