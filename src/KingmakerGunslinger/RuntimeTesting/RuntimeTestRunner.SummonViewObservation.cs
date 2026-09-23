using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Kingmaker.Blueprints;
using Kingmaker.View;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Summoning;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    /// <summary>
    /// Sprint 2 of the Expanded Summoning charter needs the real donor view
    /// contract before any pterosaur asset is authored. Bone-name guessing is
    /// explicitly not evidence, so this observes the installed donor prefab and
    /// reports what actually binds, animates, collides, and anchors.
    ///
    /// The observation is read-only. It instantiates the donor prefab off to
    /// one side, measures it, and destroys it in a finally block. No blueprint,
    /// unit, inventory, campaign, or save state is touched.
    /// </summary>
    internal sealed partial class RuntimeTestRunner
    {
        /// <summary>CR3_GiantEagleStandard, the shared visual/body donor.</summary>
        private const string PteranodonDonorGuid =
            "406c1e1af5400ac4881e330502ccbd9e";

        private RuntimeTestResult RunSummonPteranodonViewContractObservation()
        {
            BlueprintUnit donor = BlueprintLibraryLookup.RequireExact<BlueprintUnit>(
                BlueprintBootstrap.Library, PteranodonDonorGuid,
                "native Pteranodon visual/body donor");

            UnitEntityView prefab = donor.Prefab == null ? null : donor.Prefab.Load();
            UnitEntityView instance = null;
            string report = "<unobserved>";
            bool hasSkinnedMesh = false;
            bool characterAvatarAbsent = false;
            bool hasAnimator = false;
            bool boundToNamedBones = false;
            bool cleaned = false;

            try
            {
                if (prefab == null)
                    throw new InvalidOperationException(
                        "The donor unit exposes no loadable view prefab.");

                // Instantiate inactive and far from play so nothing ticks,
                // renders, or registers while it is measured.
                instance = UnityEngine.Object.Instantiate(prefab,
                    new Vector3(0f, -10000f, 0f), Quaternion.identity);
                instance.gameObject.SetActive(false);

                Observation observed = Describe(instance);
                report = observed.Text;
                hasSkinnedMesh = observed.SkinnedMeshCount > 0;
                characterAvatarAbsent = observed.CharacterAvatarAbsent;
                hasAnimator = observed.HasAnimator;
                boundToNamedBones = observed.BoundBoneCount > 0 &&
                    observed.RootBoneNamed && observed.BindPoseCount ==
                        observed.BoundBoneCount;
            }
            finally
            {
                if (instance != null)
                {
                    UnityEngine.Object.DestroyImmediate(instance.gameObject);
                }

                cleaned = instance == null || instance.Equals(null);
            }

            // The shipped identity must be untouched by an observation.
            SummonCreatureSpec pteranodon = ExpandedSummoningCatalog.All
                .SingleOrDefault(value => value.Key == "pteranodon");
            bool identityIntact = pteranodon != null &&
                pteranodon.MonsterTier == 4 && pteranodon.NaturesAllyTier == 4 &&
                pteranodon.MonsterTemplated;

            var assertions = new List<RuntimeTestAssertion>
            {
                Assertion("pteranodon-donor-resolves",
                    PteranodonDonorGuid + " loads a UnitEntityView prefab",
                    "donor=" + donor.name + ";prefab=" +
                        (prefab == null ? "<null>" : prefab.name),
                    prefab != null,
                    "exact installed BlueprintUnit.Prefab UnitViewLink"),
                Assertion("pteranodon-donor-has-skinned-mesh",
                    "at least one SkinnedMeshRenderer to replace",
                    report, hasSkinnedMesh,
                    "transient donor prefab instance"),
                Assertion("pteranodon-donor-is-not-a-character-doll",
                    "CharacterAvatar absent, so the creature is a plain skinned prefab",
                    "characterAvatarAbsent=" + characterAvatarAbsent,
                    characterAvatarAbsent,
                    "UnitEntityView.CharacterAvatar on the transient instance"),
                Assertion("pteranodon-donor-animator-present",
                    "an Animator drives the donor",
                    "hasAnimator=" + hasAnimator, hasAnimator,
                    "UnitEntityView.Animator on the transient instance"),
                Assertion("pteranodon-donor-bone-binding-is-consistent",
                    "named root bone with bindposes matching the bound bone count",
                    "boundToNamedBones=" + boundToNamedBones, boundToNamedBones,
                    "SkinnedMeshRenderer rootBone, bones and sharedMesh.bindposes"),
                Assertion("pteranodon-identity-unchanged",
                    "pteranodon remains SM 4 / SNA 4 and alignment-templated",
                    "identityIntact=" + identityIntact, identityIntact,
                    "ExpandedSummoningCatalog after observation"),
                Assertion("pteranodon-view-observation-cleanup",
                    "transient donor instance destroyed; no blueprint, unit, inventory or save mutation",
                    "cleaned=" + cleaned, cleaned,
                    "finally cleanup of the single transient view instance"),
                Assertion("loaded-mod-version", _request.ExpectedModVersion,
                    _context.ModEntry.Info.Version,
                    _request.ExpectedModVersion == _context.ModEntry.Info.Version,
                    "Unity Mod Manager ModEntry.Info.Version")
            };

            return CreateResult(assertions.TrueForAll(value =>
                value.Status == "PASS") ? RuntimeTestStatuses.Pass :
                RuntimeTestStatuses.Fail, assertions, null);
        }

        private sealed class Observation
        {
            internal string Text;
            internal int SkinnedMeshCount;
            internal int BoundBoneCount;
            internal int BindPoseCount;
            internal bool RootBoneNamed;
            internal bool CharacterAvatarAbsent;
            internal bool HasAnimator;
        }

        private static Observation Describe(UnitEntityView view)
        {
            var text = new StringBuilder();
            var result = new Observation();

            result.CharacterAvatarAbsent = view.CharacterAvatar == null;
            Animator animator = view.Animator;
            result.HasAnimator = animator != null;

            text.Append("view=").Append(view.name);
            text.Append(";characterAvatar=")
                .Append(result.CharacterAvatarAbsent ? "<null>" : "present");

            if (animator != null)
            {
                RuntimeAnimatorController controller = animator.runtimeAnimatorController;
                text.Append(";animator=").Append(controller == null
                    ? "<no controller>" : controller.name);
                if (controller != null && controller.animationClips != null)
                {
                    string[] clips = controller.animationClips
                        .Where(value => value != null)
                        .Select(value => value.name)
                        .Distinct(StringComparer.Ordinal)
                        .OrderBy(value => value, StringComparer.Ordinal)
                        .ToArray();
                    text.Append(";clips=").Append(Count(clips.Length));
                    // Record a bounded sample; the full list would swamp evidence.
                    text.Append(";clipSample=")
                        .Append(string.Join(",", clips.Take(12).ToArray()));
                }

                text.Append(";avatar=").Append(animator.avatar == null
                    ? "<null>" : animator.avatar.name + (animator.avatar.isHuman
                        ? "/human" : "/generic"));
            }

            SkinnedMeshRenderer[] skinned = view
                .GetComponentsInChildren<SkinnedMeshRenderer>(true);
            result.SkinnedMeshCount = skinned.Length;
            text.Append(";skinnedMeshes=").Append(Count(skinned.Length));

            foreach (SkinnedMeshRenderer renderer in skinned)
            {
                Mesh mesh = renderer.sharedMesh;
                Transform root = renderer.rootBone;
                Transform[] bones = renderer.bones ?? new Transform[0];
                int bindPoses = mesh == null || mesh.bindposes == null
                    ? 0 : mesh.bindposes.Length;
                result.BoundBoneCount += bones.Length;
                result.BindPoseCount += bindPoses;
                result.RootBoneNamed |= root != null &&
                    !string.IsNullOrEmpty(root.name);

                text.Append(";[renderer=").Append(renderer.name)
                    .Append(",mesh=").Append(mesh == null ? "<null>" : mesh.name)
                    .Append(",rootBone=").Append(root == null ? "<null>" : root.name)
                    .Append(",bones=").Append(Count(bones.Length))
                    .Append(",bindposes=").Append(Count(bindPoses))
                    .Append(",materials=").Append(Count(
                        renderer.sharedMaterials == null
                            ? 0 : renderer.sharedMaterials.Length))
                    .Append(",shaders=").Append(string.Join("/",
                        (renderer.sharedMaterials ?? new Material[0])
                            .Where(value => value != null && value.shader != null)
                            .Select(value => value.shader.name)
                            .Distinct(StringComparer.Ordinal).Take(4).ToArray()))
                    .Append(']');

                // The exact bone names an original mesh would have to bind to.
                string[] boneNames = bones.Where(value => value != null)
                    .Select(value => value.name).ToArray();
                text.Append(";boneNames=")
                    .Append(string.Join(",", boneNames.Take(40).ToArray()));
                if (boneNames.Length > 40)
                    text.Append(",+").Append(Count(boneNames.Length - 40));
            }

            MeshRenderer[] plain = view.GetComponentsInChildren<MeshRenderer>(true);
            text.Append(";meshRenderers=").Append(Count(plain.Length));

            // Selection, targeting and footprint surfaces the replacement must
            // keep. The collider types live in UnityEngine.PhysicsModule, which
            // is deliberately outside the qualified private reference bundle,
            // so they are read reflectively rather than widening that set.
            foreach (string field in new[] { "m_SoftCollider", "m_CoreCollider" })
            {
                object collider = ReadField(view, field);
                text.Append(';').Append(field).Append('=')
                    .Append(collider == null || collider.Equals(null)
                        ? "<null>"
                        : collider.GetType().Name + ":" +
                            ((Component)collider).name);
            }

            // These are derived surfaces on a view that has never been attached
            // to a unit, so a throw here is information, not a failure.
            try
            {
                text.Append(";corpulence=").Append(Number(view.Corpulence));
                text.Append(";cameraBounds=")
                    .Append(Number(view.CameraOrientedBoundsSize.x)).Append('x')
                    .Append(Number(view.CameraOrientedBoundsSize.y));
                text.Append(";cameraCoreBounds=")
                    .Append(Number(view.CameraOrientedCoreBoundsSize.x)).Append('x')
                    .Append(Number(view.CameraOrientedCoreBoundsSize.y));
            }
            catch (Exception exception)
            {
                text.Append(";derivedBounds=<unavailable on a detached view: ")
                    .Append(exception.GetType().Name).Append('>');
            }

            Renderer any = skinned.Cast<Renderer>().Concat(plain).FirstOrDefault();
            if (any != null)
            {
                Bounds bounds = any.bounds;
                text.Append(";firstRendererBounds=")
                    .Append(Number(bounds.size.x)).Append('x')
                    .Append(Number(bounds.size.y)).Append('x')
                    .Append(Number(bounds.size.z));
            }

            text.Append(";childTransforms=")
                .Append(Count(view.GetComponentsInChildren<Transform>(true).Length));

            result.Text = text.ToString();
            return result;
        }

        private static string Count(int value)
        { return value.ToString(CultureInfo.InvariantCulture); }

        private static string Number(float value)
        { return value.ToString("0.###", CultureInfo.InvariantCulture); }
    }
}
