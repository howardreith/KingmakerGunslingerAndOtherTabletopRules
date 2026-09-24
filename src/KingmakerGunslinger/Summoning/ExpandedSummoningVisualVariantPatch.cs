using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Harmony12;
using Kingmaker.View;
using UnityEngine;

namespace KingmakerGunslinger.Summoning
{
    /// <summary>
    /// A bounded visual variant for a KMG summon that shares a native rig:
    /// the view's renderer materials are cloned and tinted (and, when the
    /// shader has an emission slot, given an inner glow). The mephits of
    /// Sprint 5 are the first users: six elements on the native mephit
    /// bodies. The colours come from the plain tint profile the domain tests
    /// compile; only this file knows UnityEngine.
    /// </summary>
    internal sealed class SummonVisualVariant
    {
        internal SummonVisualVariant(string blueprintName,
            SummonVisualTintProfile tint)
        {
            if (string.IsNullOrEmpty(blueprintName))
                throw new ArgumentException("A blueprint name is required.",
                    "blueprintName");
            if (tint == null) throw new ArgumentNullException("tint");
            BlueprintName = blueprintName;
            Key = tint.Key;
            Tint = new Color(tint.TintRed, tint.TintGreen, tint.TintBlue, 1f);
            Emission = tint.HasEmission ? new Color(tint.EmissionRed,
                tint.EmissionGreen, tint.EmissionBlue, 1f) : (Color?)null;
        }

        internal string BlueprintName { get; private set; }
        internal string Key { get; private set; }
        internal Color Tint { get; private set; }
        internal Color? Emission { get; private set; }
    }

    /// <summary>
    /// Applies a registered visual variant when a KMG summon's view attaches
    /// (Sprint 5). Everything is validated before a renderer is touched: no
    /// registered variant, no renderer, or a material without a colour slot
    /// leaves the native look untouched and records why. The cloned material
    /// is private to the view, so the donor's shared material - and every
    /// native creature using it - is never changed. Outcomes are kept for the
    /// runtime fixture and the creature review.
    /// </summary>
    [HarmonyPatch(typeof(UnitEntityView), "OnDataAttached")]
    internal static class ExpandedSummoningVisualVariantPatch
    {
        internal const string VariantMaterialName = "KMG_SummonVisualVariant";
        private static readonly string[] ColorSlots = { "_Color", "_TintColor",
            "_BaseColor", "_MainColor" };
        private const string EmissionSlot = "_EmissionColor";

        private static readonly Dictionary<string, SummonVisualVariant> Variants =
            new Dictionary<string, SummonVisualVariant>(StringComparer.Ordinal);
        private static readonly ConditionalWeakTable<UnitEntityView, string>
            Applied = new ConditionalWeakTable<UnitEntityView, string>();
        private static readonly object Sync = new object();
        private static readonly List<string> Outcomes = new List<string>();

        internal static void Register(SummonVisualVariant variant)
        {
            if (variant == null) throw new ArgumentNullException("variant");
            lock (Sync) { Variants[variant.BlueprintName] = variant; }
        }

        internal static IReadOnlyList<string> RegisteredBlueprintNames
        { get { lock (Sync) { return Variants.Keys.OrderBy(value => value,
            StringComparer.Ordinal).ToArray(); } } }

        internal static IReadOnlyList<string> ObservedOutcomes
        { get { lock (Sync) { return Outcomes.ToArray(); } } }

        /// <summary>
        /// "variant:applied;materials=2;slot=_Color;emission=0" and the like,
        /// or the reason nothing was applied; "&lt;none&gt;" for a view this
        /// patch never saw.
        /// </summary>
        internal static string DescribeView(UnitEntityView view)
        {
            string outcome;
            if (view == null || !Applied.TryGetValue(view, out outcome)) return "<none>";
            return outcome;
        }

        private static void Postfix(UnitEntityView __instance)
        {
            try
            {
                if (__instance == null || __instance.EntityData == null ||
                    __instance.EntityData.Blueprint == null) return;
                SummonVisualVariant variant;
                lock (Sync)
                {
                    if (!Variants.TryGetValue(__instance.EntityData.Blueprint.name,
                            out variant)) return;
                }
                string existing;
                if (Applied.TryGetValue(__instance, out existing)) return;
                string outcome;
                try { outcome = Apply(__instance, variant); }
                catch (Exception exception)
                { outcome = "variant:exception:" + exception.GetType().Name; }
                Applied.Add(__instance, outcome);
                lock (Sync)
                {
                    if (Outcomes.Count >= 256) Outcomes.RemoveAt(0);
                    Outcomes.Add(__instance.EntityData.Blueprint.name + "=" + outcome);
                }
            }
            catch (Exception)
            {
                // A visual variant never interrupts the game's own view attach.
            }
        }

        internal static string Apply(UnitEntityView view, SummonVisualVariant variant)
        {
            Renderer[] renderers = view.GetComponentsInChildren<Renderer>(true)
                .Where(value => value != null && value.sharedMaterials != null &&
                    value.sharedMaterials.Length != 0)
                .ToArray();
            if (renderers.Length == 0) return "variant:no-renderer";
            int tinted = 0, glowing = 0;
            string slotUsed = null;
            foreach (Renderer renderer in renderers)
            {
                Material[] originals = renderer.sharedMaterials;
                var replacements = new Material[originals.Length];
                bool changed = false;
                for (int index = 0; index < originals.Length; index++)
                {
                    Material original = originals[index];
                    replacements[index] = original;
                    if (original == null || original.name == VariantMaterialName)
                        continue;
                    string slot = ColorSlots.FirstOrDefault(original.HasProperty);
                    if (slot == null) continue;
                    var material = new Material(original);
                    material.name = VariantMaterialName;
                    material.SetColor(slot, original.GetColor(slot) * variant.Tint);
                    if (variant.Emission.HasValue && material.HasProperty(EmissionSlot))
                    {
                        material.SetColor(EmissionSlot, variant.Emission.Value);
                        glowing++;
                    }
                    replacements[index] = material;
                    slotUsed = slotUsed ?? slot;
                    tinted++;
                    changed = true;
                }
                if (changed) renderer.sharedMaterials = replacements;
            }
            if (tinted == 0) return "variant:no-colour-slot";
            return "variant:applied;key=" + variant.Key + ";materials=" + tinted +
                ";slot=" + slotUsed + ";emission=" + glowing;
        }
    }
}
