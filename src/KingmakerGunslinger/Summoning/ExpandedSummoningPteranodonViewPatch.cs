using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Harmony12;
using Kingmaker.View;
using KingmakerGunslinger.Assets;
using UnityEngine;

namespace KingmakerGunslinger.Summoning
{
    /// <summary>
    /// Attaches the original Pteranodon visual to one live summoned unit.
    ///
    /// The donor prefab is <c>GiantEagle</c>, shared by eagle, dire bat,
    /// pteranodon and roc. Every change here is therefore instance-local: the
    /// shared prefab, its mesh, its materials and its animator are never
    /// touched, and eagle, dire bat and roc are the negative controls that prove
    /// it.
    ///
    /// The invariant is that a unit is always either custom-visual-attached or
    /// donor-visual-intact. It is never invisible, never double-bodied and never
    /// half-initialised. Any failure before the donor renderer is suppressed
    /// simply leaves the donor alone; any failure after it re-enables the donor
    /// and destroys what was added.
    /// </summary>
    [HarmonyPatch(typeof(UnitEntityView), "OnDataAttached")]
    internal static class ExpandedSummoningPteranodonViewPatch
    {
        internal const string PteranodonBlueprintName =
            "KMG_Summoning_Unit_Pteranodon";
        private const string CustomChildName = "KMG_PteranodonMembrane";

        private sealed class Attachment
        {
            internal string Outcome;
            internal GameObject Child;
            internal SkinnedMeshRenderer Donor;
            internal Material Material;
            internal Mesh Mesh;
        }

        private static readonly ConditionalWeakTable<UnitEntityView, Attachment>
            Applied = new ConditionalWeakTable<UnitEntityView, Attachment>();
        private static readonly object Sync = new object();
        private static readonly List<string> Outcomes = new List<string>();

        /// <summary>
        /// Outcome strings in attachment order, for guarded runtime observation.
        /// Bounded so a long session cannot grow it without limit.
        /// </summary>
        internal static IReadOnlyList<string> ObservedOutcomes
        { get { lock (Sync) return Outcomes.ToArray(); } }

        internal static string DescribeView(UnitEntityView view)
        {
            Attachment attachment;
            if (view == null || !Applied.TryGetValue(view, out attachment))
                return "not-attempted";
            return attachment.Outcome;
        }

        private static void Record(string outcome)
        {
            lock (Sync)
            {
                if (Outcomes.Count >= 256) Outcomes.RemoveAt(0);
                Outcomes.Add(outcome);
            }
        }

        private static void Postfix(UnitEntityView __instance)
        {
            if (__instance == null || __instance.EntityData == null ||
                __instance.EntityData.Blueprint == null) return;
            if (!string.Equals(__instance.EntityData.Blueprint.name,
                PteranodonBlueprintName, StringComparison.Ordinal)) return;

            lock (Applied)
            {
                Attachment existing;
                if (Applied.TryGetValue(__instance, out existing)) return;
                Attachment attachment = new Attachment();
                Applied.Add(__instance, attachment);
                attachment.Outcome = Attach(__instance, attachment);
                Record(attachment.Outcome);
            }
        }

        /// <summary>
        /// Everything is validated before the donor renderer is suppressed, so
        /// the common failure is a no-op rather than a rollback.
        /// </summary>
        private static string Attach(UnitEntityView view, Attachment attachment)
        {
            Mesh source;
            string[] boneNames;
            if (!PteranodonAssetRuntime.TryGetMembrane(out source, out boneNames))
                return "donor-visual:" + PteranodonAssetRuntime.Status;

            SkinnedMeshRenderer[] donors = view
                .GetComponentsInChildren<SkinnedMeshRenderer>(true)
                .Where(value => value != null && value.sharedMesh != null)
                .ToArray();
            if (donors.Length != 1)
                return "donor-visual:donor-renderer-count=" + donors.Length;
            SkinnedMeshRenderer donor = donors[0];

            Transform[] bones;
            Matrix4x4[] bindposes;
            string reason;
            if (!PteranodonAssetRuntime.TryResolveDonorBinding(donor, boneNames,
                out bones, out bindposes, out reason))
                return "donor-visual:" + reason;

            Transform rootBone = donor.rootBone;
            if (rootBone == null) return "donor-visual:donor-root-bone-missing";
            Material donorMaterial = donor.sharedMaterial;
            if (donorMaterial == null)
                return "donor-visual:donor-material-missing";

            GameObject child = null;
            Material material = null;
            Mesh mesh = null;
            try
            {
                // A copy, so the cached bundle asset keeps its normalised bind
                // poses and a second unit binds from the same clean source.
                mesh = UnityEngine.Object.Instantiate(source);
                mesh.name = CustomChildName;
                mesh.bindposes = bindposes;

                // Cloned from the donor's material so the membrane is shaded by
                // the game's own pipeline rather than a bundled stand-in.
                material = new Material(donorMaterial);
                material.name = CustomChildName;

                child = new GameObject(CustomChildName);
                child.transform.SetParent(donor.transform.parent, false);
                child.transform.localPosition = Vector3.zero;
                child.transform.localRotation = Quaternion.identity;
                child.transform.localScale = Vector3.one;

                SkinnedMeshRenderer renderer =
                    child.AddComponent<SkinnedMeshRenderer>();
                renderer.sharedMesh = mesh;
                renderer.bones = bones;
                renderer.rootBone = rootBone;
                renderer.sharedMaterial = material;
                renderer.localBounds = donor.localBounds;
                renderer.updateWhenOffscreen = donor.updateWhenOffscreen;
                renderer.quality = donor.quality;
                renderer.shadowCastingMode = donor.shadowCastingMode;
                renderer.receiveShadows = donor.receiveShadows;

                attachment.Child = child;
                attachment.Material = material;
                attachment.Mesh = mesh;
                attachment.Donor = donor;

                // Only now is anything about the donor changed, and only the
                // component's enabled flag on this one instance. The GameObject
                // stays active so anything else parented under it - effects,
                // anchors, colliders - keeps working.
                donor.enabled = false;
                return "membrane:attached;bones=" + bones.Length +
                    ";vertices=" + mesh.vertexCount;
            }
            catch (Exception error)
            {
                Revert(attachment);
                if (child != null) UnityEngine.Object.Destroy(child);
                if (material != null) UnityEngine.Object.Destroy(material);
                if (mesh != null) UnityEngine.Object.Destroy(mesh);
                attachment.Child = null;
                attachment.Material = null;
                attachment.Mesh = null;
                return "donor-visual:attach-failed:" + error.GetType().Name;
            }
        }

        /// <summary>Puts the donor back exactly as it was.</summary>
        private static void Revert(Attachment attachment)
        {
            if (attachment.Donor != null) attachment.Donor.enabled = true;
        }
    }
}
