using System;
using System.IO;
using System.Linq;
using UnityEngine;

namespace KingmakerGunslinger.Assets
{
    internal static class WeaponPresentationDonorFrames
    {
        // Guarded runtime evidence from the exact native OH_SpearShortCommon
        // PiercingOneHanded control. Local +Y is the physical point-leading
        // shaft axis; local +Z is its stable secondary/head-up axis.
        internal static readonly Vector3 PiercingOneHandedHeldPosition =
            new Vector3(-0.00299995276f, -0.0209996216f, 0.00199998566f);
        internal static readonly Vector3 PiercingOneHandedHeldEuler =
            new Vector3(9.712032f, 123.546196f, 178.825317f);

        internal static Quaternion PiercingOneHandedHeldRotation
        {
            get { return Quaternion.Euler(PiercingOneHandedHeldEuler); }
        }

        internal static Vector3 PiercingOneHandedHeldForward
        {
            get
            {
                return PiercingOneHandedHeldRotation * Vector3.up;
            }
        }

        internal static Vector3 PiercingOneHandedHeldUp
        {
            get
            {
                return PiercingOneHandedHeldRotation * Vector3.forward;
            }
        }

        // The native held basis is the attachment-frame donor. Guarded live
        // UnitAttack evidence for all four production handguns, sampled after
        // the rendered actor body reached its target-facing direction, measured
        // the remaining action-frame bias. Express the correction in that
        // semantic basis instead of serializing an unexplained Euler offset:
        // -0.468 donor-up and +0.184 donor-right predicts a 0.994 minimum acted
        // muzzle/target dot while retaining a 0.901 minimum native low-ready
        // actor-forward dot, the native roll, and the donor grip anchor.
        internal static Vector3 PiercingOneHandedFirearmForward
        {
            get
            {
                Vector3 forward = PiercingOneHandedHeldForward;
                Vector3 up = PiercingOneHandedHeldUp;
                Vector3 right = Vector3.Cross(up, forward).normalized;
                return (forward - 0.468f * up + 0.184f * right).normalized;
            }
        }

        internal static Vector3 PiercingOneHandedFirearmUp
        {
            get
            {
                Vector3 forward = PiercingOneHandedFirearmForward;
                Vector3 donorUp = PiercingOneHandedHeldUp;
                return (donorUp - Vector3.Dot(donorUp, forward) * forward)
                    .normalized;
            }
        }
    }

    internal struct WeaponPresentationSemanticFrame
    {
        internal WeaponPresentationSemanticFrame(Vector3 grip, Vector3 tip,
            Vector3 butt, Vector3 secondary, bool hasSupport,
            Vector3 support)
            : this(grip, tip, butt, secondary, hasSupport, support,
                tip - grip)
        {
        }

        internal WeaponPresentationSemanticFrame(Vector3 grip, Vector3 tip,
            Vector3 butt, Vector3 secondary, bool hasSupport,
            Vector3 support, Vector3 forwardAxis)
        {
            Grip = grip;
            Tip = tip;
            Butt = butt;
            Secondary = secondary;
            HasSupport = hasSupport;
            Support = support;

            Forward = forwardAxis.normalized;
            Vector3 rawSecondary = secondary - grip;
            Up = (rawSecondary - Vector3.Dot(rawSecondary, Forward) *
                Forward).normalized;
            Right = Vector3.Cross(Up, Forward).normalized;
        }

        internal Vector3 Grip { get; private set; }
        internal Vector3 Tip { get; private set; }
        internal Vector3 Butt { get; private set; }
        internal Vector3 Secondary { get; private set; }
        internal bool HasSupport { get; private set; }
        internal Vector3 Support { get; private set; }
        internal Vector3 Forward { get; private set; }
        internal Vector3 Up { get; private set; }
        internal Vector3 Right { get; private set; }
    }

    internal struct WeaponPresentationProjection
    {
        internal WeaponPresentationProjection(float minimum, float maximum,
            int sourceCount)
        {
            Minimum = minimum;
            Maximum = maximum;
            SourceCount = sourceCount;
        }

        internal float Minimum { get; private set; }
        internal float Maximum { get; private set; }
        internal int SourceCount { get; private set; }
        internal float Span { get { return Maximum - Minimum; } }
    }

    internal static class WeaponPresentationFrameContract
    {
        internal const string GripMarker = "Grip";
        internal const string ButtMarker = "Butt";
        internal const string SupportMarker = "SupportHandTarget";
        internal const string WeaponUpMarker = "WeaponUp";
        internal const string WeaponForwardMarker = "WeaponForward";
        internal const string HeadUpMarker = "HeadUp";
        internal const string BladeNormalMarker = "BladeNormal";

        private const float Epsilon = 0.0001f;

        internal static WeaponPresentationSemanticFrame Require(
            Transform root, string label, string tipMarker,
            string secondaryMarker, bool requireSupport, float minimumLength,
            float maximumLength)
        {
            return RequireInternal(root, label, tipMarker, secondaryMarker,
                null, requireSupport, false, minimumLength, maximumLength);
        }

        internal static WeaponPresentationSemanticFrame RequireWithForwardMarker(
            Transform root, string label, string tipMarker,
            string secondaryMarker, string forwardMarker,
            bool requireSupport, float minimumLength, float maximumLength)
        {
            if (string.IsNullOrEmpty(forwardMarker))
                throw new ArgumentException(
                    "An explicit semantic forward marker is required.",
                    "forwardMarker");
            return RequireInternal(root, label, tipMarker, secondaryMarker,
                forwardMarker, requireSupport, false, minimumLength,
                maximumLength);
        }

        internal static WeaponPresentationSemanticFrame
            RequireWithForwardMarkerAndButtSupport(Transform root,
                string label, string tipMarker, string secondaryMarker,
                string forwardMarker, float minimumLength,
                float maximumLength)
        {
            if (string.IsNullOrEmpty(forwardMarker))
                throw new ArgumentException(
                    "An explicit semantic forward marker is required.",
                    "forwardMarker");
            return RequireInternal(root, label, tipMarker, secondaryMarker,
                forwardMarker, true, true, minimumLength, maximumLength);
        }

        private static WeaponPresentationSemanticFrame RequireInternal(
            Transform root, string label, string tipMarker,
            string secondaryMarker, string forwardMarker,
            bool requireSupport, bool supportTowardButt,
            float minimumLength, float maximumLength)
        {
            if (root == null)
                throw new InvalidDataException(label + " root is null.");
            if (!Approximately(root.localPosition, Vector3.zero) ||
                !Approximately(root.localRotation, Quaternion.identity) ||
                !Approximately(root.localScale, Vector3.one))
                throw new InvalidDataException(label +
                    " equipment root is not identity-transformed.");

            Transform grip = RequireUnique(root, GripMarker, label);
            Transform tip = RequireUnique(root, tipMarker, label);
            Transform butt = RequireUnique(root, ButtMarker, label);
            Transform secondary = RequireUnique(root, secondaryMarker, label);
            Transform explicitForward = string.IsNullOrEmpty(forwardMarker)
                ? null : RequireUnique(root, forwardMarker, label);
            Transform support = FindUnique(root, SupportMarker, label);
            if (requireSupport && support == null)
                throw new InvalidDataException(label +
                    " support-hand target is missing.");
            if (!requireSupport && support != null)
                throw new InvalidDataException(label +
                    " one-handed or stored presentation has an unexpected support-hand target.");

            ValidateMarkerTransform(grip, label);
            ValidateMarkerTransform(tip, label);
            ValidateMarkerTransform(butt, label);
            ValidateMarkerTransform(secondary, label);
            if (explicitForward != null)
                ValidateMarkerTransform(explicitForward, label);
            if (support != null) ValidateMarkerTransform(support, label);
            ValidatePositiveScaleHierarchy(root, label);

            Vector3 forwardVector = explicitForward == null
                ? tip.localPosition - grip.localPosition
                : explicitForward.localPosition - grip.localPosition;
            Vector3 secondaryVector = secondary.localPosition -
                grip.localPosition;
            float forwardLength = forwardVector.magnitude;
            float secondaryLength = secondaryVector.magnitude;
            if (!Finite(grip.localPosition) || !Finite(tip.localPosition) ||
                !Finite(butt.localPosition) ||
                !Finite(secondary.localPosition) ||
                (explicitForward != null &&
                    !Finite(explicitForward.localPosition)) ||
                (support != null && !Finite(support.localPosition)))
                throw new InvalidDataException(label +
                    " semantic frame contains a non-finite marker.");
            if (forwardLength <= Epsilon || secondaryLength <= Epsilon)
                throw new InvalidDataException(label +
                    " semantic frame is degenerate.");

            Vector3 forward = forwardVector / forwardLength;
            Vector3 secondaryNormal = secondaryVector / secondaryLength;
            if (Mathf.Abs(Vector3.Dot(forward, secondaryNormal)) >= 0.98f)
                throw new InvalidDataException(label +
                    " semantic forward and secondary axes are collinear.");
            float buttProjection = Vector3.Dot(
                butt.localPosition - grip.localPosition, forward);
            float tipProjection = Vector3.Dot(
                tip.localPosition - grip.localPosition, forward);
            float semanticLength = Vector3.Distance(tip.localPosition,
                butt.localPosition);
            if (tipProjection <= Epsilon)
                throw new InvalidDataException(label +
                    " tip/muzzle is not ahead of the grip on the semantic forward axis.");
            if (buttProjection >= -Epsilon)
                throw new InvalidDataException(label +
                    " tip/butt polarity is reversed or the grip is not ahead of the butt.");
            if (semanticLength < minimumLength ||
                semanticLength > maximumLength)
                throw new InvalidDataException(label +
                    " semantic tip-to-butt length is implausible: " +
                    semanticLength.ToString("R"));
            if (support != null)
            {
                Vector3 fromGrip = support.localPosition - grip.localPosition;
                float supportProjection = Vector3.Dot(fromGrip, forward);
                float lateral = (fromGrip - supportProjection * forward).magnitude;
                if (supportTowardButt)
                {
                    if (supportProjection <= buttProjection + Epsilon ||
                        supportProjection >= -Epsilon)
                        throw new InvalidDataException(label +
                            " support-hand target is outside the butt-to-grip handle interval.");
                }
                else if (supportProjection <= Epsilon ||
                         supportProjection >= tipProjection - Epsilon)
                    throw new InvalidDataException(label +
                        " support-hand target is outside the grip-to-tip interval.");
                if (lateral > Mathf.Max(0.10f, semanticLength * 0.25f))
                    throw new InvalidDataException(label +
                        " support-hand target is outside the weapon envelope.");
            }

            return new WeaponPresentationSemanticFrame(grip.localPosition,
                tip.localPosition, butt.localPosition,
                secondary.localPosition, support != null,
                support == null ? Vector3.zero : support.localPosition,
                forwardVector);
        }

        internal static Quaternion SolveRotation(
            WeaponPresentationSemanticFrame source,
            Vector3 targetForward, Vector3 targetSecondary)
        {
            Vector3 normalizedTargetForward;
            Vector3 normalizedTargetUp;
            Orthonormalize(targetForward, targetSecondary,
                "target semantic frame", out normalizedTargetForward,
                out normalizedTargetUp);
            Quaternion sourceBasis = Quaternion.LookRotation(source.Forward,
                source.Up);
            Quaternion targetBasis = Quaternion.LookRotation(
                normalizedTargetForward, normalizedTargetUp);
            return targetBasis * Quaternion.Inverse(sourceBasis);
        }

        internal static Vector3 SolveTranslation(Quaternion rotation,
            float scale, Vector3 sourceGrip, Vector3 targetGrip)
        {
            if (!Finite(rotation) || !Finite(scale) || scale <= 0f ||
                !Finite(sourceGrip) || !Finite(targetGrip))
                throw new InvalidDataException(
                    "Semantic-frame translation inputs are invalid.");
            return targetGrip - rotation * (sourceGrip * scale);
        }

        internal static WeaponPresentationProjection ValidateRendererEndpoints(
            Transform root, Transform visual,
            WeaponPresentationSemanticFrame frame, string label,
            float endpointToleranceFraction)
        {
            if (visual == null)
                throw new InvalidDataException(label + " visual is missing.");
            if (endpointToleranceFraction <= 0f ||
                endpointToleranceFraction > 0.35f)
                throw new ArgumentOutOfRangeException(
                    "endpointToleranceFraction");

            WeaponPresentationProjection projection = MeasureProjection(root,
                visual, frame.Forward, label);
            float tolerance = Mathf.Max(0.025f,
                projection.Span * endpointToleranceFraction);
            float gripProjection = Vector3.Dot(frame.Grip, frame.Forward);
            float rendererMaximum = projection.Maximum - gripProjection;
            float rendererMinimum = projection.Minimum - gripProjection;
            float tip = Vector3.Dot(frame.Tip - frame.Grip, frame.Forward);
            float butt = Vector3.Dot(frame.Butt - frame.Grip, frame.Forward);
            if (Mathf.Abs(rendererMaximum - tip) > tolerance)
                throw new InvalidDataException(label +
                    " semantic tip/muzzle does not correspond to the renderer-bound forward end: marker=" +
                    tip.ToString("R") + ";renderer=" +
                    rendererMaximum.ToString("R") + ";tolerance=" +
                    tolerance.ToString("R") + ".");
            if (Mathf.Abs(rendererMinimum - butt) > tolerance)
                throw new InvalidDataException(label +
                    " semantic butt/pommel does not correspond to the renderer-bound rear end: marker=" +
                    butt.ToString("R") + ";renderer=" +
                    rendererMinimum.ToString("R") + ";tolerance=" +
                    tolerance.ToString("R") + ".");
            return projection;
        }

        internal static void ValidateSecondaryAsPlaneNormal(Transform root,
            Transform visual, WeaponPresentationSemanticFrame frame,
            string label, float maximumNormalToForwardRatio)
        {
            WeaponPresentationProjection forward = MeasureProjection(root,
                visual, frame.Forward, label);
            WeaponPresentationProjection normal = MeasureProjection(root,
                visual, frame.Up, label);
            if (forward.Span <= Epsilon || normal.Span <= Epsilon ||
                normal.Span > forward.Span * maximumNormalToForwardRatio)
                throw new InvalidDataException(label +
                    " secondary axis is not a plausible blade/head plane normal.");
        }

        internal static WeaponPresentationProjection MeasureProjection(
            Transform root, Transform visual, Vector3 axis, string label)
        {
            if (root == null || visual == null || !Finite(axis) ||
                axis.sqrMagnitude <= Epsilon * Epsilon)
                throw new InvalidDataException(label +
                    " renderer projection inputs are invalid.");
            Vector3 normalized = axis.normalized;
            float minimum = float.PositiveInfinity;
            float maximum = float.NegativeInfinity;
            int count = 0;

            foreach (MeshFilter filter in visual.GetComponentsInChildren<
                MeshFilter>(true))
            {
                if (filter == null || filter.sharedMesh == null) continue;
                IncludeBounds(root, filter.transform, filter.sharedMesh.bounds,
                    normalized, ref minimum, ref maximum);
                count++;
            }
            foreach (SkinnedMeshRenderer renderer in
                visual.GetComponentsInChildren<SkinnedMeshRenderer>(true))
            {
                if (renderer == null || renderer.sharedMesh == null) continue;
                IncludeBounds(root, renderer.transform, renderer.localBounds,
                    normalized, ref minimum, ref maximum);
                count++;
            }
            if (count == 0 || !Finite(minimum) || !Finite(maximum) ||
                maximum - minimum <= Epsilon)
                throw new InvalidDataException(label +
                    " has no finite renderer-local geometry bounds.");
            return new WeaponPresentationProjection(minimum, maximum, count);
        }

        private static void IncludeBounds(Transform root, Transform owner,
            Bounds bounds, Vector3 axis, ref float minimum,
            ref float maximum)
        {
            Vector3 min = bounds.min;
            Vector3 max = bounds.max;
            for (int index = 0; index < 8; index++)
            {
                Vector3 point = new Vector3(
                    (index & 1) == 0 ? min.x : max.x,
                    (index & 2) == 0 ? min.y : max.y,
                    (index & 4) == 0 ? min.z : max.z);
                Vector3 rootLocal = root.InverseTransformPoint(
                    owner.TransformPoint(point));
                float projection = Vector3.Dot(rootLocal, axis);
                minimum = Mathf.Min(minimum, projection);
                maximum = Mathf.Max(maximum, projection);
            }
        }

        private static Transform RequireUnique(Transform root, string name,
            string label)
        {
            Transform result = FindUnique(root, name, label);
            if (result == null)
                throw new InvalidDataException(label + " semantic marker " +
                    name + " is missing.");
            return result;
        }

        private static Transform FindUnique(Transform root, string name,
            string label)
        {
            Transform[] matches = root.GetComponentsInChildren<Transform>(true)
                .Where(value => value != null && value.name == name).ToArray();
            if (matches.Length > 1)
                throw new InvalidDataException(label + " semantic marker " +
                    name + " is duplicated.");
            if (matches.Length == 1 && matches[0].parent != root)
                throw new InvalidDataException(label + " semantic marker " +
                    name + " is not a direct child of the equipment root.");
            return matches.Length == 0 ? null : matches[0];
        }

        private static void ValidateMarkerTransform(Transform marker,
            string label)
        {
            if (!Approximately(marker.localRotation, Quaternion.identity) ||
                !Approximately(marker.localScale, Vector3.one))
                throw new InvalidDataException(label + " semantic marker " +
                    marker.name + " has a nonidentity rotation or scale.");
        }

        private static void ValidatePositiveScaleHierarchy(Transform root,
            string label)
        {
            foreach (Transform value in root.GetComponentsInChildren<Transform>(
                true))
                if (!Finite(value.localScale) || value.localScale.x <= 0f ||
                    value.localScale.y <= 0f || value.localScale.z <= 0f)
                    throw new InvalidDataException(label +
                        " contains a reflected, zero, or non-finite local scale.");
        }

        private static void Orthonormalize(Vector3 forward,
            Vector3 secondary, string label, out Vector3 normalizedForward,
            out Vector3 normalizedUp)
        {
            if (!Finite(forward) || !Finite(secondary) ||
                forward.sqrMagnitude <= Epsilon * Epsilon ||
                secondary.sqrMagnitude <= Epsilon * Epsilon)
                throw new InvalidDataException(label + " is degenerate.");
            normalizedForward = forward.normalized;
            Vector3 projected = secondary - Vector3.Dot(secondary,
                normalizedForward) * normalizedForward;
            if (projected.sqrMagnitude <= Epsilon * Epsilon)
                throw new InvalidDataException(label +
                    " has collinear forward and secondary axes.");
            normalizedUp = projected.normalized;
        }

        private static bool Approximately(Vector3 left, Vector3 right)
        {
            return (left - right).sqrMagnitude <= 0.000001f;
        }

        private static bool Approximately(Quaternion left, Quaternion right)
        {
            return Mathf.Abs(Quaternion.Dot(left, right)) >= 0.999999f;
        }

        private static bool Finite(Vector3 value)
        {
            return Finite(value.x) && Finite(value.y) && Finite(value.z);
        }

        private static bool Finite(Quaternion value)
        {
            return Finite(value.x) && Finite(value.y) && Finite(value.z) &&
                Finite(value.w);
        }

        private static bool Finite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }
    }
}
