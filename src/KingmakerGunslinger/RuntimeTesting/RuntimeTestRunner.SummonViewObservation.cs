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
            GameObject holder = null;
            bool activeInHierarchy = true;
            string report = "<unobserved>";
            bool hasSkinnedMesh = false;
            bool characterAvatarAbsent = false;
            bool hasAnimator = false;
            bool boundToNamedBones = false;
            bool cleaned = false;
            string animationBinding = "<unobserved>";
            string skinningReport = "<unobserved>";
            bool animationDriverProven = false;
            int clipCount = 0;

            try
            {
                if (prefab == null)
                    throw new InvalidOperationException(
                        "The donor unit exposes no loadable view prefab.");

                // Instantiate INTO an already-inactive holder. Unity inherits
                // the source object's active state, so instantiating first and
                // deactivating afterwards would let Awake and OnEnable run
                // before the deactivation - a clone parented to an inactive
                // object is never active in the hierarchy, so those callbacks
                // never fire at all. Distance from the play area is not a
                // substitute for this; it stops nothing.
                holder = new GameObject("KMG Expanded Summoning Donor Probe");
                holder.SetActive(false);
                instance = UnityEngine.Object.Instantiate(prefab,
                    holder.transform);

                activeInHierarchy = instance.gameObject.activeInHierarchy;
                if (activeInHierarchy)
                    throw new InvalidOperationException(
                        "The donor probe clone became active in the hierarchy.");

                Observation observed = Describe(instance);
                report = observed.Text;
                hasSkinnedMesh = observed.SkinnedMeshCount > 0;
                characterAvatarAbsent = observed.CharacterAvatarAbsent;
                hasAnimator = observed.HasAnimator;
                boundToNamedBones = observed.BoundBoneCount > 0 &&
                    observed.RootBoneNamed && observed.BindPoseCount ==
                        observed.BoundBoneCount;
                animationBinding = observed.AnimationBinding;
                skinningReport = observed.SkinningReport;
                clipCount = observed.ClipCount;
                // Discriminating: a real Animator, a real controller, and real
                // clips. Recording that some string existed proved nothing.
                animationDriverProven = observed.DrivingAnimator != null &&
                    !string.IsNullOrEmpty(observed.ControllerName) &&
                    observed.ClipCount > 0;
            }
            finally
            {
                if (instance != null)
                    UnityEngine.Object.DestroyImmediate(instance.gameObject);
                if (holder != null)
                    UnityEngine.Object.DestroyImmediate(holder);

                // Destroying is necessary but not sufficient on its own; the
                // probe also never became active, so nothing registered.
                cleaned = (instance == null || instance.Equals(null)) &&
                    (holder == null || holder.Equals(null)) && !activeInHierarchy;
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
                // The first run of this scenario demanded an Animator here and
                // failed. That assertion was wrong: the donor prefab carries no
                // Animator of its own, so animation must be bound when the view
                // attaches to a unit. Recording where animation actually comes
                // from is the finding; demanding it on a detached prefab was
                // exactly the assumption the charter warns against.
                Assertion("pteranodon-donor-animation-driver-identified",
                    "a real driving Animator with a named controller and at least one clip",
                    animationBinding, animationDriverProven,
                    "the child Animator actually present, its runtimeAnimatorController and clips"),
                Assertion("pteranodon-donor-clip-events-recorded",
                    "clip timing is observed so a replacement bite can land on the native frames",
                    "clips=" + clipCount, clipCount > 0,
                    "AnimationClip.events across the driving controller"),
                Assertion("pteranodon-donor-skinning-inputs-recorded",
                    "parent-relative bone paths, bone indices, bind-pose shape and renderer space",
                    skinningReport, skinningReport != "<unobserved>",
                    "SkinnedMeshRenderer bones, bindposes and transform hierarchy"),
                Assertion("pteranodon-donor-bone-binding-is-consistent",
                    "named root bone with bindposes matching the bound bone count",
                    "boundToNamedBones=" + boundToNamedBones, boundToNamedBones,
                    "SkinnedMeshRenderer rootBone, bones and sharedMesh.bindposes"),
                Assertion("pteranodon-identity-unchanged",
                    "pteranodon remains SM 4 / SNA 4 and alignment-templated",
                    "identityIntact=" + identityIntact, identityIntact,
                    "ExpandedSummoningCatalog after observation"),
                Assertion("pteranodon-probe-never-activated",
                    "the clone is inactive from creation, so Awake and OnEnable never run",
                    "activeInHierarchy=" + activeInHierarchy, !activeInHierarchy,
                    "instantiated into an already-inactive holder, checked before use"),
                Assertion("pteranodon-view-observation-cleanup",
                    "clone and holder destroyed and never activated; no blueprint, unit, inventory or save mutation",
                    "cleaned=" + cleaned, cleaned,
                    "finally cleanup plus the never-activated invariant"),
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
            internal string AnimationBinding;
            internal Animator DrivingAnimator;
            internal string ControllerName;
            internal int ClipCount;
            internal int ClipsWithEvents;
            internal int EventCount;
            internal string SkinningReport;
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

            // Where animation actually comes from. UnitEntityView.Animator is
            // null on a detached prefab, so inspecting only that property would
            // report nothing and prove nothing. The driving Animator lives in a
            // child, and Sprint 2 must reuse whatever really moves the bones.
            Animator[] childAnimators = view.GetComponentsInChildren<Animator>(true);
            object animationManager = ReadField(view, "m_AnimatorManager");
            Animator driving = animator != null ? animator
                : childAnimators.FirstOrDefault(value => value != null);
            result.DrivingAnimator = driving;

            var binding = new StringBuilder();
            binding.Append("animatorOnView=")
                .Append(animator == null ? "<null>" : animator.name)
                .Append(";animatorsInChildren=").Append(Count(childAnimators.Length));
            if (childAnimators.Length > 0)
            {
                binding.Append(";childAnimatorNames=").Append(string.Join(",",
                    childAnimators.Where(value => value != null)
                        .Select(value => value.name).Take(6).ToArray()));
            }

            binding.Append(";animationManagerField=")
                .Append(animationManager == null || animationManager.Equals(null)
                    ? "<null>" : animationManager.GetType().Name);

            // Inspect the Animator that actually exists, not the null property.
            if (driving != null)
            {
                RuntimeAnimatorController controller = driving.runtimeAnimatorController;
                result.ControllerName = controller == null ? null : controller.name;
                AnimationClip[] clips = controller == null ||
                        controller.animationClips == null
                    ? new AnimationClip[0]
                    : controller.animationClips.Where(value => value != null).ToArray();
                result.ClipCount = clips.Length;

                binding.Append(";drivingAnimator=").Append(driving.name)
                    .Append(";controller=")
                    .Append(controller == null ? "<null>" : controller.name)
                    .Append(";clips=").Append(Count(clips.Length))
                    .Append(";avatar=").Append(driving.avatar == null ? "<null>"
                        : driving.avatar.name + (driving.avatar.isHuman
                            ? "/human" : "/generic"))
                    .Append(";applyRootMotion=").Append(driving.applyRootMotion)
                    .Append(";cullingMode=").Append(driving.cullingMode);

                string[] clipNames = clips.Select(value => value.name)
                    .Distinct(StringComparer.Ordinal)
                    .OrderBy(value => value, StringComparer.Ordinal).ToArray();
                binding.Append(";clipNames=")
                    .Append(string.Join(",", clipNames));

                // Attack and impact timing lives in clip events; Sprint 2's bite
                // has to land on the same frames the native routine expects.
                var events = new List<string>();
                foreach (AnimationClip clip in clips)
                {
                    AnimationEvent[] clipEvents = clip.events;
                    if (clipEvents == null || clipEvents.Length == 0) continue;
                    result.ClipsWithEvents++;
                    foreach (AnimationEvent clipEvent in clipEvents.Take(4))
                    {
                        events.Add(clip.name + "@" +
                            clipEvent.time.ToString("0.###", CultureInfo.InvariantCulture) +
                            ":" + clipEvent.functionName);
                    }
                }

                result.EventCount = events.Count;
                binding.Append(";clipsWithEvents=").Append(Count(result.ClipsWithEvents))
                    .Append(";events=").Append(string.Join(",", events.Take(40).ToArray()));
            }

            result.AnimationBinding = binding.ToString();
            text.Append(';').Append(result.AnimationBinding);

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

                // The exact bone names an original mesh must bind to. The whole
                // list is recorded deliberately: a truncated rig is useless to
                // the modeller, and this is the evidence Sprint 2 authors from.
                string[] boneNames = bones.Where(value => value != null)
                    .Select(value => value.name).ToArray();
                text.Append(";boneCount=").Append(Count(boneNames.Length));
                text.Append(";boneNames=")
                    .Append(string.Join(",", boneNames));

                // Names and counts alone cannot drive an export. The modeller
                // also needs the parent-relative hierarchy, the bone order the
                // mesh indexes against, and the space the renderer works in.
                result.SkinningReport = DescribeSkinning(view, renderer, root, bones, mesh);
                text.Append(';').Append(result.SkinningReport);
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

        /// <summary>
        /// Authoring inputs for an original skinned mesh: the parent-relative
        /// bone hierarchy, the bone order the mesh indexes against, the shape of
        /// the bind poses, and the renderer's working space.
        ///
        /// Bind poses are summarised structurally - uniform scale, handedness,
        /// whether the root pose is identity - rather than dumped as matrices.
        /// The structure is what an exporter must match; the matrix values are
        /// native asset data and stay on the machine.
        /// </summary>
        private static string DescribeSkinning(UnitEntityView view,
            SkinnedMeshRenderer renderer, Transform root, Transform[] bones,
            Mesh mesh)
        {
            var text = new StringBuilder();
            text.Append("skinning=[");

            // Parent-relative path of every bone, so the hierarchy can be
            // rebuilt exactly rather than guessed from a flat name list.
            text.Append("bonePaths=");
            text.Append(string.Join(",", bones.Select((bone, index) =>
                Count(index) + ":" + RelativePath(view.transform, bone)).ToArray()));

            text.Append(";rootBonePath=")
                .Append(RelativePath(view.transform, root));
            text.Append(";rendererPath=")
                .Append(RelativePath(view.transform, renderer.transform));

            // Renderer space: what the mesh vertices are expressed relative to.
            text.Append(";rendererLocalBounds=")
                .Append(Number(renderer.localBounds.size.x)).Append('x')
                .Append(Number(renderer.localBounds.size.y)).Append('x')
                .Append(Number(renderer.localBounds.size.z));
            text.Append(";rendererLocalScale=")
                .Append(Number(renderer.transform.localScale.x)).Append(',')
                .Append(Number(renderer.transform.localScale.y)).Append(',')
                .Append(Number(renderer.transform.localScale.z));
            text.Append(";updateWhenOffscreen=").Append(renderer.updateWhenOffscreen);
            text.Append(";quality=").Append(renderer.quality);

            if (mesh != null)
            {
                text.Append(";meshVertices=").Append(Count(mesh.vertexCount));
                text.Append(";meshSubMeshes=").Append(Count(mesh.subMeshCount));
                text.Append(";meshBoneWeights=")
                    .Append(Count(mesh.boneWeights == null ? 0 : mesh.boneWeights.Length));

                Matrix4x4[] poses = mesh.bindposes;
                if (poses != null && poses.Length > 0)
                {
                    // Structural summary only.
                    bool rootIsIdentity = poses[0].isIdentity;
                    int mirrored = poses.Count(pose => pose.determinant < 0f);
                    bool uniform = poses.All(pose =>
                    {
                        Vector3 scale = pose.lossyScale;
                        return Math.Abs(scale.x - scale.y) < 0.001f &&
                            Math.Abs(scale.y - scale.z) < 0.001f;
                    });
                    text.Append(";bindPoseCount=").Append(Count(poses.Length))
                        .Append(";bindPoseRootIsIdentity=").Append(rootIsIdentity)
                        .Append(";bindPoseUniformScale=").Append(uniform)
                        .Append(";bindPoseMirrored=").Append(Count(mirrored));
                }
            }

            text.Append(']');
            return text.ToString();
        }

        /// <summary>Slash-separated path from an ancestor to a descendant.</summary>
        private static string RelativePath(Transform ancestor, Transform node)
        {
            if (node == null) return "<null>";
            var parts = new List<string>();
            for (Transform cursor = node; cursor != null && cursor != ancestor;
                cursor = cursor.parent)
            {
                parts.Add(cursor.name);
            }

            parts.Reverse();
            return string.Join("/", parts.ToArray());
        }

        private static string Count(int value)
        { return value.ToString(CultureInfo.InvariantCulture); }

        private static string Number(float value)
        { return value.ToString("0.###", CultureInfo.InvariantCulture); }
    }
}
