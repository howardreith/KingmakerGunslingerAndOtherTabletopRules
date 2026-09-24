using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Kingmaker.View;
using KingmakerGunslinger.Assets;
using KingmakerGunslinger.Summoning;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    /// <summary>
    /// Captures the Pteranodon's attached view contract.
    ///
    /// The detached-prefab probe can prove the mesh, the rig and the Avatar, but
    /// not the animation controller: Kingmaker assigns the
    /// runtimeAnimatorController when a view attaches to a unit, so a prefab
    /// instantiated outside that path legitimately reports none.
    ///
    /// A standalone scenario that built its own disposable caster to get an
    /// attached view was written and withdrawn. It died twice inside
    /// EntityDestructionController during its own cleanup, because reproducing
    /// the spawn and teardown lifecycle correctly is genuinely fiddly and the
    /// reimplementation kept diverging from the shipped one. The capture now
    /// rides along with disposable-expanded-summoning, which already summons
    /// every creature - Pteranodon included, across 123 single casts - and
    /// already tears down cleanly. Reusing a proven lifecycle beat debugging a
    /// second copy of it.
    ///
    /// This file therefore holds observation helpers only. They read; they never
    /// spawn, destroy, or mutate.
    /// </summary>
    internal sealed partial class RuntimeTestRunner
    {
        /// <summary>Set once, by the first Pteranodon summoned in a run.</summary>
        private string _pteranodonAttachedContract;

        /// <summary>
        /// One view-patch outcome per Pteranodon view spawned in a run, in
        /// cast order. Every one must be an attachment for the vertical slice
        /// to hold; a fallback here is a fact to report, not a pass.
        /// </summary>
        private readonly List<string> _pteranodonVisualOutcomes = new List<string>();
        private readonly List<string> _pteranodonVisualRenderers = new List<string>();
        private readonly List<string> _pteranodonVisualSteps = new List<string>();
        private readonly List<string> _pteranodonFaultDrill = new List<string>();
        private readonly List<string> _donorIsolationDetail = new List<string>();
        private IDisposable _pteranodonWithdrawal;
        private int _pteranodonCastsSeen;
        private int _pteranodonCrowdMax;
        private int _donorIsolationChecked;
        private int _donorIsolationClean;

        /// <summary>
        /// The creatures that share the Pteranodon's GiantEagle donor prefab.
        /// They are the negative controls for instance isolation: the patch
        /// must never touch them.
        /// </summary>
        private static readonly string[] PteranodonDonorSharers =
        { "eagle", "dire-bat", "roc" };

        /// <summary>
        /// The one skinned renderer on a GiantEagle-donor view, as the swap
        /// leaves it: the Pteranodon's mesh, material and 46 bones when
        /// attached; the eagle's own mesh, material and 72 bones when not. The
        /// component's enabled state is the game's to set - the fader hides a
        /// fresh summon until it fades in - so it is recorded, not judged.
        /// </summary>
        private static string DescribePteranodonRenderers(UnitEntityView view)
        {
            SkinnedMeshRenderer[] renderers = view
                .GetComponentsInChildren<SkinnedMeshRenderer>(true)
                .Where(value => value != null && value.sharedMesh != null)
                .ToArray();
            SkinnedMeshRenderer donor = renderers.FirstOrDefault();
            return "mesh=" + (donor == null || donor.sharedMesh == null ? "<none>" :
                    donor.sharedMesh.name) +
                ";material=" + (donor == null || donor.sharedMaterial == null ? "<none>" :
                    donor.sharedMaterial.name) +
                ";bones=" + Number(donor == null || donor.bones == null ? 0 :
                    donor.bones.Length) +
                ";enabled=" + (donor != null && donor.enabled ? "true" : "false") +
                ";renderers=" + Number(renderers.Length);
        }

        /// <summary>
        /// The swap is in place: the Pteranodon's mesh and material on the
        /// donor's component, bound to the 46 bones its weights index.
        /// </summary>
        private static bool IsPteranodonAttached(string renderers)
        {
            return renderers.StartsWith("mesh=" +
                ExpandedSummoningPteranodonViewPatch.CustomVisualName + ";material=" +
                ExpandedSummoningPteranodonViewPatch.CustomVisualName + ";bones=46;",
                StringComparison.Ordinal);
        }

        /// <summary>
        /// The donor exactly as the prefab gives it: its own mesh and material
        /// on the 72-bone rig, and one renderer.
        /// </summary>
        private static bool IsDonorUntouched(string renderers)
        {
            return renderers.IndexOf("mesh=" +
                    ExpandedSummoningPteranodonViewPatch.CustomVisualName,
                    StringComparison.Ordinal) < 0 &&
                renderers.IndexOf("material=" +
                    ExpandedSummoningPteranodonViewPatch.CustomVisualName,
                    StringComparison.Ordinal) < 0 &&
                renderers.IndexOf(";bones=72;", StringComparison.Ordinal) >= 0 &&
                renderers.EndsWith(";renderers=1", StringComparison.Ordinal);
        }

        /// <summary>
        /// The presentation contract of the attached visual on the donor's own
        /// component: the attached outcome and state, every one of the 46
        /// installed bones a transform of this view's own skeleton with the
        /// root bone kept, nonzero bounds, the material carrying the albedo on
        /// a shader, and the view scale at the catalog multiplier.
        /// </summary>
        private static string DescribePteranodonPresentation(UnitEntityView view,
            out bool satisfied)
        {
            string outcome = ExpandedSummoningPteranodonViewPatch.DescribeView(view);
            SkinnedMeshRenderer[] renderers = view
                .GetComponentsInChildren<SkinnedMeshRenderer>(true)
                .Where(value => value != null && value.sharedMesh != null)
                .ToArray();
            SkinnedMeshRenderer donor = renderers.FirstOrDefault();
            string state = DescribePteranodonRenderers(view);
            bool attached = outcome.StartsWith("visual:attached;",
                StringComparison.Ordinal) && IsPteranodonAttached(state) &&
                renderers.Length == 1;
            int boneCount = donor == null || donor.bones == null ? 0 : donor.bones.Length;
            int bonesOwned = donor == null || donor.bones == null ? 0 :
                donor.bones.Count(value => value != null &&
                    value.IsChildOf(view.transform));
            bool bonesBound = donor != null && boneCount == 46 && bonesOwned == 46 &&
                donor.rootBone != null && donor.rootBone.IsChildOf(view.transform);
            bool bounded = donor != null && donor.localBounds.size.sqrMagnitude > 0.0001f;
            Material material = donor == null ? null : donor.sharedMaterial;
            bool shaded = material != null && material.shader != null &&
                material.mainTexture != null &&
                material.mainTexture.name == "KMG_Pteranodon_Albedo";
            float multiplier = SummonViewScaleCatalog.All.Single(value =>
                value.CreatureKey == "pteranodon").Multiplier;
            float scale = view.transform.localScale.x;
            bool scaled = Mathf.Abs(scale - multiplier) < 0.001f;
            Bounds world = donor == null ? new Bounds() : donor.bounds;
            bool extent = donor != null && world.size.sqrMagnitude > 0.0001f;
            satisfied = attached && bonesBound && bounded && shaded && scaled && extent;
            return "outcome=" + outcome + ";" + state + ";attached=" + attached +
                ";bonesOwned=" + Number(bonesOwned) + "/" + Number(boneCount) +
                ";rootBone=" + (donor == null || donor.rootBone == null ? "<null>" :
                    donor.rootBone.name) +
                ";bounded=" + bounded + ";shader=" + (material == null ||
                    material.shader == null ? "<null>" : material.shader.name) +
                ";shaded=" + shaded + ";viewScale=" + Decimal(scale) + "/" +
                Decimal(multiplier) + ";worldExtent=" + Decimal(world.size.magnitude);
        }

        /// <summary>
        /// Everything Sprint 2 must preserve about a live Pteranodon's view:
        /// the driving animator and its controller, the clips and the frames
        /// their attack and impact events fire on, and the anchors effects and
        /// selection hang from.
        /// </summary>
        private string DescribeAttachedPteranodonView(UnitEntityView view)
        {
            var text = new StringBuilder();
            text.Append("attached=[view=").Append(view.name);

            Animator animator = view.Animator ??
                view.GetComponentsInChildren<Animator>(true)
                    .FirstOrDefault(value => value != null);
            text.Append(";animator=")
                .Append(animator == null ? "<null>" : animator.name);

            var rigLines = new List<string>();
            if (animator != null)
            {
                text.Append(";avatar=").Append(animator.avatar == null ? "<null>"
                    : animator.avatar.name + (animator.avatar.isHuman
                        ? "/human" : "/generic"));
                // Recorded for completeness, and expected to be null: Kingmaker
                // does not drive creatures through Mecanim.
                text.Append(";mecanimController=")
                    .Append(animator.runtimeAnimatorController == null
                        ? "<null>" : animator.runtimeAnimatorController.name);
            }

            // Kingmaker animates units through a Playables-based system, not a
            // RuntimeAnimatorController: UnitAnimationManager owns an ActionSet
            // of UnitAnimationAction objects, each holding its own clips, and
            // drives them through m_LocoMotionHandle and friends. Three earlier
            // revisions of this capture asserted a Mecanim controller and failed
            // on a detached prefab, then on an attached view, then on a live
            // summon - the assumption was wrong, not the game. Read the real
            // source instead, reflectively, so no assumption about the action
            // type is baked in either.
            object animationManager = ReadField(view, "m_AnimatorManager");
            text.Append(";animationManager=")
                .Append(animationManager == null || animationManager.Equals(null)
                    ? "<null>" : animationManager.GetType().Name);

            var actionNames = new List<string>();
            var clipNames = new List<string>();
            var events = new List<string>();
            if (animationManager != null && !animationManager.Equals(null))
            {
                var actionSet = ReadMemberOrNull(animationManager, "ActionSet")
                    as System.Collections.IEnumerable;
                if (actionSet != null)
                {
                    foreach (object action in actionSet)
                    {
                        if (action == null) continue;
                        var behaviour = action as UnityEngine.Object;
                        actionNames.Add(behaviour == null
                            ? action.GetType().Name
                            : behaviour.name + ":" + action.GetType().Name);
                        CollectAnimationClips(action, clipNames, events);
                    }
                }
            }

            text.Append(";actionCount=").Append(Number(actionNames.Count));
            text.Append(";actions=").Append(string.Join(",",
                actionNames.Distinct(StringComparer.Ordinal)
                    .OrderBy(value => value, StringComparer.Ordinal)
                    .Take(60).ToArray()));
            text.Append(";clipCount=").Append(Number(clipNames.Count));
            text.Append(";clips=").Append(string.Join(",",
                clipNames.Distinct(StringComparer.Ordinal)
                    .OrderBy(value => value, StringComparer.Ordinal)
                    .Take(60).ToArray()));
            text.Append(";eventCount=").Append(Number(events.Count));
            text.Append(";events=").Append(string.Join(",",
                events.Distinct(StringComparer.Ordinal)
                    .OrderBy(value => value, StringComparer.Ordinal)
                    .Take(80).ToArray()));

            // Effect and selection anchors the replacement must keep.
            object snapMap = ReadField(view, "m_ParticleSnapMap");
            text.Append(";particlesSnapMap=")
                .Append(snapMap == null || snapMap.Equals(null) ? "<null>"
                    : snapMap.GetType().Name);
            object softCollider = ReadField(view, "m_SoftCollider");
            object coreCollider = ReadField(view, "m_CoreCollider");
            text.Append(";softCollider=").Append(softCollider == null ||
                softCollider.Equals(null) ? "<null>" : softCollider.GetType().Name);
            text.Append(";coreCollider=").Append(coreCollider == null ||
                coreCollider.Equals(null) ? "<null>" : coreCollider.GetType().Name);
            text.Append(";centerTorso=").Append(view.CenterTorso == null
                ? "<null>" : view.CenterTorso.name);
            text.Append(";corpulence=").Append(Decimal(view.Corpulence));
            text.Append(";viewScale=").Append(Decimal(view.transform.localScale.x));

            // The authoring rig: every bone's rest pose, parent-relative, in the
            // order the mesh indexes them. Written to the run's own evidence
            // directory rather than the repository, because these are measured
            // native transforms.
            SkinnedMeshRenderer renderer = view
                .GetComponentsInChildren<SkinnedMeshRenderer>(true)
                .FirstOrDefault(value => value != null && value.sharedMesh != null);
            if (renderer != null)
            {
                Transform[] bones = renderer.bones ?? new Transform[0];
                text.Append(";renderer=").Append(renderer.name)
                    .Append(";boneCount=").Append(Number(bones.Length))
                    .Append(";rootBone=").Append(renderer.rootBone == null
                        ? "<null>" : renderer.rootBone.name);

                rigLines.Add("{");
                rigLines.Add("  \"source\": \"attached summoned Pteranodon view\",");
                rigLines.Add("  \"renderer\": \"" + renderer.name + "\",");
                rigLines.Add("  \"rootBone\": \"" + (renderer.rootBone == null
                    ? string.Empty : renderer.rootBone.name) + "\",");
                // The bind pose is the frame the original mesh's vertices were
                // authored in, and unlike the live local transforms it does not
                // move with animation. bindposes[i] is
                //   bone[i].worldToLocalMatrix * renderer.localToWorldMatrix
                // at bind time, so its inverse gives the bone's bind transform
                // in renderer space. A replacement mesh must be authored here,
                // not against whatever pose the creature happened to be holding.
                Matrix4x4[] bindPoses = renderer.sharedMesh.bindposes ??
                    new Matrix4x4[0];
                rigLines.Add("  \"bindPoseCount\": " + Number(bindPoses.Length) + ",");
                rigLines.Add("  \"bones\": [");
                for (int index = 0; index < bones.Length; index++)
                {
                    Transform bone = bones[index];
                    if (bone == null) continue;
                    Vector3 localPosition = bone.localPosition;
                    Quaternion localRotation = bone.localRotation;
                    Vector3 localScale = bone.localScale;

                    string bindFields = string.Empty;
                    if (index < bindPoses.Length)
                    {
                        Matrix4x4 bind = bindPoses[index].inverse;
                        Vector3 bindPosition = bind.MultiplyPoint3x4(Vector3.zero);
                        Quaternion bindRotation = Quaternion.LookRotation(
                            bind.GetColumn(2), bind.GetColumn(1));
                        bindFields = string.Concat(
                            ", \"bindPosition\": [", Decimal(bindPosition.x), ", ",
                            Decimal(bindPosition.y), ", ", Decimal(bindPosition.z), "]",
                            ", \"bindRotation\": [", Decimal(bindRotation.x), ", ",
                            Decimal(bindRotation.y), ", ", Decimal(bindRotation.z),
                            ", ", Decimal(bindRotation.w), "]");
                    }

                    rigLines.Add(string.Concat(
                        "    { \"index\": ", Number(index),
                        ", \"name\": \"", bone.name, "\"",
                        ", \"parent\": \"", bone.parent == null ? string.Empty
                            : bone.parent.name, "\"",
                        ", \"localPosition\": [", Decimal(localPosition.x), ", ",
                        Decimal(localPosition.y), ", ", Decimal(localPosition.z), "]",
                        ", \"localRotation\": [", Decimal(localRotation.x), ", ",
                        Decimal(localRotation.y), ", ", Decimal(localRotation.z),
                        ", ", Decimal(localRotation.w), "]",
                        ", \"localScale\": [", Decimal(localScale.x), ", ",
                        Decimal(localScale.y), ", ", Decimal(localScale.z), "]",
                        bindFields,
                        " }", index == bones.Length - 1 ? string.Empty : ","));
                }

                rigLines.Add("  ]");
                rigLines.Add("}");
            }

            text.Append(']');

            if (rigLines.Count > 0 &&
                !string.IsNullOrWhiteSpace(_request.EvidenceDirectory))
            {
                string path = Path.Combine(_request.EvidenceDirectory,
                    "pteranodon-attached-rig.json");
                File.WriteAllText(path, string.Join("\n", rigLines.ToArray()));
                text.Append(";rigFile=pteranodon-attached-rig.json");
            }

            return text.ToString();
        }

        /// <summary>
        /// Reads a property or field by name without assuming the declaring
        /// type, so the capture survives a shape it did not anticipate.
        /// </summary>
        private static object ReadMemberOrNull(object instance, string name)
        {
            if (instance == null) return null;
            for (Type type = instance.GetType(); type != null; type = type.BaseType)
            {
                System.Reflection.PropertyInfo property = type.GetProperty(name,
                    System.Reflection.BindingFlags.Instance |
                    System.Reflection.BindingFlags.Public |
                    System.Reflection.BindingFlags.NonPublic |
                    System.Reflection.BindingFlags.DeclaredOnly);
                if (property != null && property.CanRead)
                {
                    try { return property.GetValue(instance, null); }
                    catch (Exception) { return null; }
                }

                System.Reflection.FieldInfo field = type.GetField(name,
                    System.Reflection.BindingFlags.Instance |
                    System.Reflection.BindingFlags.Public |
                    System.Reflection.BindingFlags.NonPublic |
                    System.Reflection.BindingFlags.DeclaredOnly);
                if (field != null)
                {
                    try { return field.GetValue(instance); }
                    catch (Exception) { return null; }
                }
            }

            return null;
        }

        /// <summary>
        /// Pulls every AnimationClip reachable from one animation action, and
        /// the frames its events fire on. Actions expose their clips under
        /// several member names across the hierarchy, so this looks for clips
        /// rather than for a particular shape.
        /// </summary>
        private static void CollectAnimationClips(object action,
            List<string> clipNames, List<string> events)
        {
            var seen = new List<AnimationClip>();
            for (Type type = action.GetType(); type != null; type = type.BaseType)
            {
                foreach (System.Reflection.FieldInfo field in type.GetFields(
                    System.Reflection.BindingFlags.Instance |
                    System.Reflection.BindingFlags.Public |
                    System.Reflection.BindingFlags.NonPublic |
                    System.Reflection.BindingFlags.DeclaredOnly))
                {
                    object value;
                    try { value = field.GetValue(action); }
                    catch (Exception) { continue; }
                    if (value == null) continue;

                    var clip = value as AnimationClip;
                    if (clip != null) { seen.Add(clip); continue; }

                    var many = value as System.Collections.IEnumerable;
                    if (many == null || value is string) continue;
                    foreach (object item in many)
                    {
                        var nested = item as AnimationClip;
                        if (nested != null) seen.Add(nested);
                    }
                }
            }

            foreach (AnimationClip clip in seen)
            {
                if (clip == null) continue;
                clipNames.Add(clip.name);
                AnimationEvent[] clipEvents = clip.events;
                if (clipEvents == null) continue;
                foreach (AnimationEvent clipEvent in clipEvents)
                {
                    events.Add(clip.name + "@" + clipEvent.time.ToString(
                        "0.###", CultureInfo.InvariantCulture) + ":" +
                        clipEvent.functionName);
                }
            }
        }

        private static string Number(int value)
        { return value.ToString(CultureInfo.InvariantCulture); }

        private static string Decimal(float value)
        { return value.ToString("0.#####", CultureInfo.InvariantCulture); }
    }
}
