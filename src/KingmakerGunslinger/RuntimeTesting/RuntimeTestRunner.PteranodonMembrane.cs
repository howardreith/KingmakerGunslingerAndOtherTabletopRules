using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using KingmakerGunslinger.Assets;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    /// <summary>
    /// Proves the Pteranodon membrane is skinned to the donor rig correctly.
    ///
    /// "The mesh appears" is not the thing worth proving. A sheet bound with the
    /// wrong bind poses still appears, and still moves; it just moves into the
    /// wrong place. Correct skinning means one specific thing - a vertex is
    /// fixed in the frame of the bones that own it - and that is what this
    /// measures:
    ///
    ///   1. skin the membrane at the donor's current pose, and record each
    ///      vertex both in world space and in its owning bone's local frame;
    ///   2. rotate one feather bone by a known angle;
    ///   3. skin again, recording the same two things;
    ///   4. put the bone back.
    ///
    /// A vertex the rotated bone owns must move in world space, and must NOT
    /// move in that bone's local frame. A vertex owned by a bone on the other
    /// wing must not move at all. Watching an idle animation would have shown
    /// movement without separating those two cases, and movement alone is what
    /// a wrongly bound mesh also produces.
    ///
    /// All of it runs on the detached, never-activated probe - the donor prefab
    /// instantiated into an inactive holder so Awake and OnEnable never run - so
    /// no live unit is touched and nothing the game owns is mutated. The probed
    /// bone is restored in a finally block, before anything else, so even a
    /// fault mid-measurement leaves the probe's rig as it was found.
    /// </summary>
    internal sealed partial class RuntimeTestRunner
    {
        /// <summary>The bone perturbed. Its opposite number is the control.</summary>
        private const string MembraneProbeBone = "L_Feather_3";
        private const string MembraneControlPrefix = "R_";
        private const float MembraneProbeDegrees = 25f;

        /// <summary>
        /// Weight above which a vertex is considered owned by one bone. Below
        /// this a vertex is genuinely blended and its motion is a mixture, so it
        /// is excluded from both populations rather than counted as evidence
        /// either way.
        /// </summary>
        private const float MembraneDominantWeight = 0.8f;

        /// <summary>
        /// Movement below this is float noise; above it is real. The
        /// perturbation is 25 degrees on a bone about a unit from the vertices
        /// it owns, so a correctly bound vertex moves far more than this.
        /// </summary>
        private const float MembraneMovedThreshold = 0.05f;
        private const float MembraneStillTolerance = 0.002f;

        private string DescribeMembraneDeformation(GameObject probe)
        {
            Mesh source;
            string[] boneNames;
            if (!PteranodonAssetRuntime.TryGetMembrane(out source, out boneNames))
                return "membrane-unavailable:" + PteranodonAssetRuntime.Status;

            SkinnedMeshRenderer[] donors = probe
                .GetComponentsInChildren<SkinnedMeshRenderer>(true)
                .Where(value => value != null && value.sharedMesh != null)
                .ToArray();
            if (donors.Length != 1)
                return "probe-donor-renderer-count=" + donors.Length;
            SkinnedMeshRenderer donor = donors[0];

            Transform[] bones;
            Matrix4x4[] bindposes;
            string reason;
            if (!PteranodonAssetRuntime.TryResolveDonorBinding(donor, boneNames,
                out bones, out bindposes, out reason))
                return "binding-unresolved:" + reason;

            int probeIndex = Array.FindIndex(boneNames, value =>
                string.Equals(value, MembraneProbeBone, StringComparison.Ordinal));
            if (probeIndex < 0) return "probe-bone-absent:" + MembraneProbeBone;

            Transform probeBone = bones[probeIndex];
            Quaternion originalRotation = probeBone.localRotation;
            Mesh mesh = null;
            GameObject child = null;
            try
            {
                mesh = UnityEngine.Object.Instantiate(source);
                mesh.bindposes = bindposes;

                // A renderer is created because BakeMesh needs one, and because
                // creating it exercises the same assignment the loader performs.
                child = new GameObject("KMG Membrane Deformation Probe");
                child.transform.SetParent(donor.transform.parent, false);
                SkinnedMeshRenderer renderer =
                    child.AddComponent<SkinnedMeshRenderer>();
                renderer.sharedMesh = mesh;
                renderer.bones = bones;
                renderer.rootBone = donor.rootBone;
                renderer.sharedMaterial = donor.sharedMaterial;

                Vector3[] vertices = mesh.vertices;
                BoneWeight[] weights = mesh.boneWeights;

                // Record world AND bone-local positions while the bone is in its
                // original pose. Taking the local reading afterwards, through
                // the rotated transform, would compare two different frames and
                // make the whole test vacuous.
                Vector3[] worldBefore = SkinMembrane(vertices, weights, bones,
                    bindposes);
                Vector3[] localBefore = ToBoneLocal(worldBefore, probeBone);
                float bakeDisagreement = BakeDisagreement(renderer, mesh,
                    worldBefore);

                probeBone.localRotation = originalRotation *
                    Quaternion.AngleAxis(MembraneProbeDegrees, Vector3.right);
                Vector3[] worldAfter = SkinMembrane(vertices, weights, bones,
                    bindposes);
                Vector3[] localAfter = ToBoneLocal(worldAfter, probeBone);

                return CompareMembraneSkins(worldBefore, worldAfter, localBefore,
                    localAfter, weights, boneNames, probeIndex, bakeDisagreement);
            }
            catch (Exception error)
            {
                return "deformation-probe-failed:" + error.GetType().Name;
            }
            finally
            {
                probeBone.localRotation = originalRotation;
                if (child != null) UnityEngine.Object.DestroyImmediate(child);
                if (mesh != null) UnityEngine.Object.DestroyImmediate(mesh);
            }
        }

        /// <summary>
        /// Skinning, evaluated directly:
        ///     v_world = sum_i w_i * bones[i].localToWorldMatrix * bindposes[i] * v
        ///
        /// This rather than <c>BakeMesh</c> because the result has to be in
        /// unambiguous world space: the drift check expresses each vertex in a
        /// bone's local frame, and a space convention that differs by Unity
        /// version would invalidate it silently. What is under test here is the
        /// asset's weights and the donor's bind poses, not the formula.
        /// <c>BakeMesh</c> is still consulted, as a reported corroboration.
        /// </summary>
        private static Vector3[] SkinMembrane(Vector3[] vertices,
            BoneWeight[] weights, Transform[] bones, Matrix4x4[] bindposes)
        {
            var skin = new Matrix4x4[bones.Length];
            for (int index = 0; index < bones.Length; index++)
                skin[index] = bones[index].localToWorldMatrix * bindposes[index];

            var skinned = new Vector3[vertices.Length];
            for (int index = 0; index < vertices.Length; index++)
            {
                BoneWeight weight = weights[index];
                int[] slots = { weight.boneIndex0, weight.boneIndex1,
                    weight.boneIndex2, weight.boneIndex3 };
                float[] values = { weight.weight0, weight.weight1,
                    weight.weight2, weight.weight3 };
                Vector3 total = Vector3.zero;
                for (int slot = 0; slot < 4; slot++)
                {
                    if (values[slot] <= 0f) continue;
                    total += skin[slots[slot]].MultiplyPoint3x4(vertices[index]) *
                        values[slot];
                }

                skinned[index] = total;
            }

            return skinned;
        }

        private static Vector3[] ToBoneLocal(Vector3[] world, Transform bone)
        {
            var local = new Vector3[world.Length];
            for (int index = 0; index < world.Length; index++)
                local[index] = bone.InverseTransformPoint(world[index]);
            return local;
        }

        /// <summary>
        /// How far Unity's own skinning lands from the evaluated result.
        ///
        /// Reported, not asserted. BakeMesh's output space has differed between
        /// Unity versions, and on an object that has never been activated it can
        /// legitimately produce nothing at all, so a verdict here would be a
        /// verdict about the engine rather than about the asset. A large number
        /// is still worth seeing.
        /// </summary>
        private static float BakeDisagreement(SkinnedMeshRenderer renderer,
            Mesh mesh, Vector3[] world)
        {
            var baked = new Mesh();
            try
            {
                renderer.BakeMesh(baked);
                if (baked.vertexCount != mesh.vertexCount) return -1f;
                Vector3[] vertices = baked.vertices;
                Matrix4x4 toWorld = renderer.transform.localToWorldMatrix;
                float worst = 0f;
                for (int index = 0; index < vertices.Length; index++)
                {
                    float gap = Vector3.Distance(
                        toWorld.MultiplyPoint3x4(vertices[index]), world[index]);
                    if (gap > worst) worst = gap;
                }

                return worst;
            }
            catch (Exception)
            {
                return -1f;
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(baked);
            }
        }

        private string CompareMembraneSkins(Vector3[] worldBefore,
            Vector3[] worldAfter, Vector3[] localBefore, Vector3[] localAfter,
            BoneWeight[] weights, string[] boneNames, int probeIndex,
            float bakeDisagreement)
        {
            int movedOwned = 0, stillOwned = 0, driftedOwned = 0;
            int movedControl = 0, stillControl = 0;
            float worstDrift = 0f, worstControlMotion = 0f, largestMotion = 0f;

            for (int index = 0; index < worldBefore.Length; index++)
            {
                float motion = Vector3.Distance(worldBefore[index],
                    worldAfter[index]);
                if (motion > largestMotion) largestMotion = motion;

                int dominant;
                if (DominantInfluence(weights[index], out dominant) <
                    MembraneDominantWeight) continue;

                if (dominant == probeIndex)
                {
                    if (motion > MembraneMovedThreshold) movedOwned++;
                    else stillOwned++;
                    float drift = Vector3.Distance(localBefore[index],
                        localAfter[index]);
                    if (drift > worstDrift) worstDrift = drift;
                    if (drift > MembraneStillTolerance) driftedOwned++;
                }
                else if (boneNames[dominant].StartsWith(MembraneControlPrefix,
                    StringComparison.Ordinal))
                {
                    if (motion > MembraneMovedThreshold) movedControl++;
                    else stillControl++;
                    if (motion > worstControlMotion) worstControlMotion = motion;
                }
            }

            return string.Format(CultureInfo.InvariantCulture,
                "bone={0};degrees={1};ownedMoved={2};ownedStill={3};" +
                "ownedDrifted={4};worstDrift={5:F5};controlMoved={6};" +
                "controlStill={7};worstControlMotion={8:F5};largestMotion={9:F4}" +
                ";bakeDisagreement={10:F5}",
                MembraneProbeBone, MembraneProbeDegrees, movedOwned, stillOwned,
                driftedOwned, worstDrift, movedControl, stillControl,
                worstControlMotion, largestMotion, bakeDisagreement);
        }

        private static float DominantInfluence(BoneWeight weight, out int index)
        {
            index = weight.boneIndex0;
            float best = weight.weight0;
            if (weight.weight1 > best) { best = weight.weight1; index = weight.boneIndex1; }
            if (weight.weight2 > best) { best = weight.weight2; index = weight.boneIndex2; }
            if (weight.weight3 > best) { best = weight.weight3; index = weight.boneIndex3; }
            return best;
        }

        /// <summary>
        /// The membrane deformed correctly: vertices the probed bone owns moved
        /// in world space, none of them drifted in that bone's frame, vertices
        /// on the other wing did not move, and at least one vertex of each
        /// population was examined - so the check cannot pass by finding
        /// nothing.
        /// </summary>
        private static bool MembraneDeformationExact(string observed)
        {
            if (observed == null ||
                observed.IndexOf("ownedMoved=", StringComparison.Ordinal) < 0)
                return false;
            return MembraneField(observed, "ownedMoved") > 0 &&
                MembraneField(observed, "ownedDrifted") == 0 &&
                MembraneField(observed, "controlMoved") == 0 &&
                MembraneField(observed, "controlStill") > 0;
        }

        private static int MembraneField(string observed, string name)
        {
            int start = observed.IndexOf(name + "=", StringComparison.Ordinal);
            if (start < 0) return -1;
            start += name.Length + 1;
            int end = observed.IndexOf(';', start);
            if (end < 0) end = observed.Length;
            int value;
            return int.TryParse(observed.Substring(start, end - start),
                NumberStyles.Integer, CultureInfo.InvariantCulture, out value)
                ? value : -1;
        }
    }
}
