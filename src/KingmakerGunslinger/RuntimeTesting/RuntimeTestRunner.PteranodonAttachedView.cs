using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Kingmaker.View;
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
                RuntimeAnimatorController controller =
                    animator.runtimeAnimatorController;
                text.Append(";controller=")
                    .Append(controller == null ? "<null>" : controller.name);
                text.Append(";avatar=").Append(animator.avatar == null ? "<null>"
                    : animator.avatar.name + (animator.avatar.isHuman
                        ? "/human" : "/generic"));

                AnimationClip[] clips = controller == null ||
                        controller.animationClips == null
                    ? new AnimationClip[0]
                    : controller.animationClips.Where(value => value != null)
                        .ToArray();
                text.Append(";clipCount=").Append(Number(clips.Length));
                text.Append(";clips=").Append(string.Join(",",
                    clips.Select(value => value.name)
                        .Distinct(StringComparer.Ordinal)
                        .OrderBy(value => value, StringComparer.Ordinal)
                        .ToArray()));

                var events = new List<string>();
                foreach (AnimationClip clip in clips)
                {
                    AnimationEvent[] clipEvents = clip.events;
                    if (clipEvents == null) continue;
                    foreach (AnimationEvent clipEvent in clipEvents)
                    {
                        events.Add(clip.name + "@" + clipEvent.time.ToString(
                            "0.###", CultureInfo.InvariantCulture) + ":" +
                            clipEvent.functionName);
                    }
                }

                text.Append(";eventCount=").Append(Number(events.Count));
                text.Append(";events=").Append(string.Join(",",
                    events.OrderBy(value => value, StringComparer.Ordinal)
                        .Take(80).ToArray()));
            }

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
                rigLines.Add("  \"bones\": [");
                for (int index = 0; index < bones.Length; index++)
                {
                    Transform bone = bones[index];
                    if (bone == null) continue;
                    Vector3 localPosition = bone.localPosition;
                    Quaternion localRotation = bone.localRotation;
                    Vector3 localScale = bone.localScale;
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

        private static string Number(int value)
        { return value.ToString(CultureInfo.InvariantCulture); }

        private static string Decimal(float value)
        { return value.ToString("0.#####", CultureInfo.InvariantCulture); }
    }
}
