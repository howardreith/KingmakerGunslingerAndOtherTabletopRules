using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace KingmakerGunslinger.Presentation
{
    /// <summary>A point or direction, with the frame it is in said by whoever holds it.</summary>
    internal struct FramePoint
    {
        internal FramePoint(double x, double y, double z)
        {
            X = x; Y = y; Z = z;
        }

        internal readonly double X;
        internal readonly double Y;
        internal readonly double Z;

        internal double Length { get { return Math.Sqrt(Dot(this)); } }
        internal bool Finite
        {
            get
            {
                return !(double.IsNaN(X) || double.IsInfinity(X) || double.IsNaN(Y) ||
                    double.IsInfinity(Y) || double.IsNaN(Z) || double.IsInfinity(Z));
            }
        }

        internal FramePoint Plus(FramePoint other)
        {
            return new FramePoint(X + other.X, Y + other.Y, Z + other.Z);
        }

        internal FramePoint Minus(FramePoint other)
        {
            return new FramePoint(X - other.X, Y - other.Y, Z - other.Z);
        }

        internal double Dot(FramePoint other)
        {
            return X * other.X + Y * other.Y + Z * other.Z;
        }

        internal FramePoint Cross(FramePoint other)
        {
            return new FramePoint(Y * other.Z - Z * other.Y,
                Z * other.X - X * other.Z, X * other.Y - Y * other.X);
        }

        internal FramePoint Times(double factor)
        {
            return new FramePoint(X * factor, Y * factor, Z * factor);
        }

        internal double DistanceTo(FramePoint other)
        {
            return Minus(other).Length;
        }

        internal string Describe()
        {
            return "(" + Number(X) + "," + Number(Y) + "," + Number(Z) + ")";
        }

        internal static string Number(double value)
        {
            return value.ToString("0.#######", CultureInfo.InvariantCulture);
        }

        /// <summary>A number a measurement can be judged by: not NaN and not infinite.</summary>
        internal static bool IsFinite(double value)
        {
            return !(double.IsNaN(value) || double.IsInfinity(value));
        }
    }

    /// <summary>
    /// Where one frame sits inside its parent: a point p of the child frame
    /// is at Position + Rotation * (Scale * p) in the parent frame. The
    /// rotation is a unit quaternion.
    /// </summary>
    internal struct FramePlacement
    {
        internal FramePlacement(FramePoint position, double qx, double qy,
            double qz, double qw, FramePoint scale)
        {
            Position = position; QX = qx; QY = qy; QZ = qz; QW = qw; Scale = scale;
        }

        internal readonly FramePoint Position;
        internal readonly double QX;
        internal readonly double QY;
        internal readonly double QZ;
        internal readonly double QW;
        internal readonly FramePoint Scale;

        internal static FramePlacement Identity
        {
            get
            {
                return new FramePlacement(new FramePoint(0, 0, 0), 0, 0, 0, 1,
                    new FramePoint(1, 1, 1));
            }
        }

        internal bool Valid
        {
            get
            {
                double norm = QX * QX + QY * QY + QZ * QZ + QW * QW;
                return Position.Finite && Scale.Finite && !double.IsNaN(norm) &&
                    Math.Abs(norm - 1.0) < 0.001 && Scale.X > 0 && Scale.Y > 0 &&
                    Scale.Z > 0;
            }
        }

        internal FramePoint Rotate(FramePoint value)
        {
            var axis = new FramePoint(QX, QY, QZ);
            FramePoint first = axis.Cross(value);
            FramePoint second = axis.Cross(first);
            return value.Plus(first.Times(2.0 * QW)).Plus(second.Times(2.0));
        }

        /// <summary>The child-frame point expressed in the parent frame.</summary>
        internal FramePoint ToParent(FramePoint child)
        {
            return Position.Plus(Rotate(new FramePoint(child.X * Scale.X,
                child.Y * Scale.Y, child.Z * Scale.Z)));
        }

        internal string Describe()
        {
            return "position=" + Position.Describe() + ";rotation=(" +
                FramePoint.Number(QX) + "," + FramePoint.Number(QY) + "," +
                FramePoint.Number(QZ) + "," + FramePoint.Number(QW) + ");scale=" +
                Scale.Describe();
        }
    }

    /// <summary>
    /// What was read from one marker-authored firearm prefab. Two frames are
    /// involved and each field says which it is in: the prefab root frame,
    /// which is the frame the game attaches to the hand, and the Visual
    /// frame, which is the imported source model placed inside the root.
    /// </summary>
    internal sealed class AuthoredMarkerRigObservation
    {
        internal AuthoredMarkerRigObservation()
        {
            AuthoredMarkerCounts = new Dictionary<string, int>(StringComparer.Ordinal);
            AuthoredInVisual = new Dictionary<string, FramePoint>(StringComparer.Ordinal);
            RootMarkers = new Dictionary<string, FramePoint>(StringComparer.Ordinal);
        }

        internal string Variant;
        internal bool InstancePresent;
        internal bool VisualPresent;
        /// <summary>How many transforms under Visual carry each authored marker name.</summary>
        internal IDictionary<string, int> AuthoredMarkerCounts { get; private set; }
        /// <summary>The root has a SupportHandTarget child.</summary>
        internal bool SupportHandTargetPresent;
        /// <summary>The Visual frame's placement in the prefab root frame.</summary>
        internal FramePlacement? Visual;
        /// <summary>Each authored marker found exactly once, in the Visual frame.</summary>
        internal IDictionary<string, FramePoint> AuthoredInVisual { get; private set; }
        /// <summary>The root's own Muzzle, Grip and Butt children, in the prefab root frame.</summary>
        internal IDictionary<string, FramePoint> RootMarkers { get; private set; }
        /// <summary>Set when reading the prefab raised.</summary>
        internal string Fault;
    }

    /// <summary>
    /// What a marker-authored firearm prefab is held to. The source
    /// coordinates are those the model's generator declares, in the source
    /// model's own frame. The held anchor and forward are where the adapter
    /// that builds the prefab is declared to put the grip and the bore, in
    /// the prefab root frame. Nothing here is taken from a prefab.
    /// </summary>
    internal sealed class AuthoredMarkerRigExpectation
    {
        internal const string GripMarker = "KMG_Grip";
        internal const string SupportMarker = "KMG_Support";
        internal const string ButtMarker = "KMG_Butt";
        internal const string MuzzleMarker = "KMG_Muzzle";
        internal const string RootMuzzle = "Muzzle";
        internal const string RootGrip = "Grip";
        internal const string RootButt = "Butt";

        internal static readonly string[] AuthoredMarkers =
            { GripMarker, SupportMarker, ButtMarker, MuzzleMarker };

        /// <summary>Each root marker and the authored marker it stands for.</summary>
        internal static readonly KeyValuePair<string, string>[] RootToAuthored =
        {
            new KeyValuePair<string, string>(RootGrip, GripMarker),
            new KeyValuePair<string, string>(RootMuzzle, MuzzleMarker),
            new KeyValuePair<string, string>(RootButt, ButtMarker)
        };

        internal IDictionary<string, FramePoint> Source =
            new Dictionary<string, FramePoint>(StringComparer.Ordinal);
        /// <summary>A one-handed rig has no support-hand target.</summary>
        internal bool SupportTargetExpected;
        internal FramePoint HeldAnchor;
        internal FramePoint HeldForward;
        /// <summary>Metres. The tolerance the check has always used.</summary>
        internal double PositionTolerance = 0.0001;
        /// <summary>Degrees. The bundle builder's own solved-rotation tolerance.</summary>
        internal double AngleToleranceDegrees = 0.05;

        /// <summary>
        /// The named pistols' source markers, as
        /// assets-source/original-models/firearm-pistol-variants/generate_firearm_pistol_variants.py
        /// declares them (MARKERS, metres, +Z muzzle, grip at the origin) and
        /// its build report records them for both variants.
        /// </summary>
        internal static AuthoredMarkerRigExpectation NamedPistol(
            FramePoint heldAnchor, FramePoint heldForward)
        {
            var expectation = new AuthoredMarkerRigExpectation
            {
                SupportTargetExpected = false, HeldAnchor = heldAnchor,
                HeldForward = heldForward
            };
            expectation.Source[GripMarker] = new FramePoint(0, 0, 0);
            expectation.Source[SupportMarker] = new FramePoint(0, -0.020, 0.145);
            expectation.Source[ButtMarker] = new FramePoint(0, 0, -0.075);
            expectation.Source[MuzzleMarker] = new FramePoint(0, 0, 0.264);
            return expectation;
        }
    }

    /// <summary>
    /// The marker contract of one marker-authored firearm prefab, as
    /// separate relationships. A position is only ever compared with
    /// another position in the same frame: an authored marker is first
    /// carried through the Visual placement into the prefab root frame, and
    /// only then compared with the root marker that stands for it.
    ///
    /// A coordinate, distance or angle that is not a finite number is not a
    /// measurement. It is never compared with a tolerance: the relationship
    /// that needs it fails and names it, and the relationships that do not
    /// need it are judged as usual.
    /// </summary>
    internal sealed class AuthoredMarkerRigAudit
    {
        internal const string MarkerSetName = "pistol-authored-marker-set";
        internal const string TransformsName = "pistol-required-transforms";
        internal const string SupportPolicyName = "pistol-support-target-policy";
        internal const string ImportedGeometryName = "pistol-imported-marker-geometry";
        internal const string RootMarkersName = "pistol-root-markers-in-held-frame";
        internal const string HeldFrameName = "pistol-held-donor-frame";
        internal const string AggregateName = "pistol-authored-marker-contract";

        private AuthoredMarkerRigAudit() { }

        internal string Variant { get; private set; }
        internal IList<AuditOutcome> Outcomes { get; private set; }
        internal bool Passed { get { return Outcomes.All(value => value.Passed); } }

        internal AuditOutcome Outcome(string name)
        {
            return Outcomes.Single(value => value.Name == name);
        }

        /// <summary>Never throws: an unreadable or absent input fails the relationship that needs it.</summary>
        internal static AuthoredMarkerRigAudit Evaluate(
            AuthoredMarkerRigObservation observed,
            AuthoredMarkerRigExpectation expected)
        {
            if (expected == null) throw new ArgumentNullException("expected");
            var rig = observed ?? new AuthoredMarkerRigObservation();
            string unread = observed == null ? "no observation" :
                rig.Fault != null ? "the prefab could not be read: " + rig.Fault :
                !rig.InstancePresent ? "the prefab did not instantiate" : null;
            var outcomes = new List<AuditOutcome>();

            // 1. Exactly one of each authored marker under Visual.
            var counts = new List<string>();
            var wrongCounts = new List<string>();
            foreach (string marker in AuthoredMarkerRigExpectation.AuthoredMarkers)
            {
                int count;
                rig.AuthoredMarkerCounts.TryGetValue(marker, out count);
                counts.Add(marker + ":" + count);
                if (count != 1)
                    wrongCounts.Add(marker + (count == 0 ? " is missing" :
                        " occurs " + count + " times"));
            }
            outcomes.Add(Outcome(MarkerSetName,
                "exactly one each of " + string.Join(", ",
                    AuthoredMarkerRigExpectation.AuthoredMarkers) + " under Visual",
                string.Join(",", counts.ToArray()), wrongCounts, unread));

            // 2. The transforms the rest of the contract reads.
            var missing = new List<string>();
            if (!rig.VisualPresent || !rig.Visual.HasValue) missing.Add("Visual");
            else if (!rig.Visual.Value.Valid)
                missing.Add("Visual placement is not a finite positive-scale placement");
            var unusableRoots = new List<string>();
            foreach (KeyValuePair<string, string> pair in
                AuthoredMarkerRigExpectation.RootToAuthored)
            {
                FramePoint rootMarker;
                if (!rig.RootMarkers.TryGetValue(pair.Key, out rootMarker))
                    missing.Add(pair.Key);
                else if (!rootMarker.Finite)
                    unusableRoots.Add(NotFiniteRoot(pair.Key, rootMarker));
            }
            List<string> transformProblems = missing.Select(value => value +
                " is missing or unusable").ToList();
            transformProblems.AddRange(unusableRoots);
            outcomes.Add(Outcome(TransformsName,
                "Visual and the root's Grip, Muzzle and Butt are present",
                "visual=" + (rig.Visual.HasValue ? rig.Visual.Value.Describe() :
                    "<absent>") + ";rootMarkers=" + string.Join(",",
                        rig.RootMarkers.Keys.OrderBy(value => value,
                            StringComparer.Ordinal).ToArray()),
                transformProblems, unread));

            // 3. The support-hand target policy.
            var support = new List<string>();
            if (rig.SupportHandTargetPresent != expected.SupportTargetExpected)
                support.Add(expected.SupportTargetExpected
                    ? "the support-hand target is missing"
                    : "a one-handed rig carries a support-hand target");
            outcomes.Add(Outcome(SupportPolicyName,
                expected.SupportTargetExpected ? "a SupportHandTarget on the root"
                    : "no SupportHandTarget on the root",
                "supportHandTarget=" + rig.SupportHandTargetPresent, support, unread));

            // The authored markers carried into the prefab root frame. A
            // marker whose coordinates are not finite, or which does not
            // stay finite through the placement, has no position to carry:
            // it is kept out, with the reason, for the relationships that
            // would have read it.
            bool placed = rig.Visual.HasValue && rig.Visual.Value.Valid;
            var authoredInRoot = new Dictionary<string, FramePoint>(
                StringComparer.Ordinal);
            var unusableAuthored = new Dictionary<string, string>(
                StringComparer.Ordinal);
            foreach (KeyValuePair<string, FramePoint> pair in rig.AuthoredInVisual)
            {
                if (!pair.Value.Finite)
                {
                    unusableAuthored[pair.Key] = pair.Key +
                        " is not finite in the Visual frame " + pair.Value.Describe();
                    continue;
                }
                if (!placed) continue;
                FramePoint carried = rig.Visual.Value.ToParent(pair.Value);
                if (!carried.Finite)
                {
                    unusableAuthored[pair.Key] = pair.Key +
                        " is not finite once carried into the prefab root frame " +
                        carried.Describe();
                    continue;
                }
                authoredInRoot[pair.Key] = carried;
            }

            // 4. Import kept the authored geometry. Distances between
            //    markers do not depend on the frame they are measured in,
            //    so the generator's source coordinates can be used directly.
            var geometry = new List<string>();
            var geometryDetail = new List<string>();
            string[][] spans =
            {
                new[] { AuthoredMarkerRigExpectation.GripMarker,
                    AuthoredMarkerRigExpectation.MuzzleMarker },
                new[] { AuthoredMarkerRigExpectation.GripMarker,
                    AuthoredMarkerRigExpectation.ButtMarker },
                new[] { AuthoredMarkerRigExpectation.ButtMarker,
                    AuthoredMarkerRigExpectation.MuzzleMarker }
            };
            foreach (string[] span in spans)
            {
                string label = span[0] + "-" + span[1];
                FramePoint sourceFrom, sourceTo, from, to;
                if (!expected.Source.TryGetValue(span[0], out sourceFrom) ||
                    !expected.Source.TryGetValue(span[1], out sourceTo))
                {
                    geometry.Add(label + " has no declared source coordinates");
                    continue;
                }
                double declared = sourceFrom.DistanceTo(sourceTo);
                if (!FramePoint.IsFinite(declared))
                {
                    geometry.Add(label + " not evaluated: its declared source distance is not finite");
                    geometryDetail.Add(label + ":declared=" +
                        FramePoint.Number(declared) + ",observed=<not-evaluated>");
                    continue;
                }
                string unusableEnd;
                if (unusableAuthored.TryGetValue(span[0], out unusableEnd) ||
                    unusableAuthored.TryGetValue(span[1], out unusableEnd))
                {
                    geometry.Add(label + " not evaluated: " + unusableEnd);
                    geometryDetail.Add(label + ":declared=" +
                        FramePoint.Number(declared) + ",observed=<unusable>");
                    continue;
                }
                if (!authoredInRoot.TryGetValue(span[0], out from) ||
                    !authoredInRoot.TryGetValue(span[1], out to))
                {
                    geometry.Add(label + " not evaluated: a marker or the Visual placement is unavailable");
                    geometryDetail.Add(label + ":declared=" +
                        FramePoint.Number(declared) + ",observed=<unavailable>");
                    continue;
                }
                double measured = from.DistanceTo(to);
                double gap = Math.Abs(measured - declared);
                geometryDetail.Add(label + ":declared=" + FramePoint.Number(declared) +
                    ",observed=" + FramePoint.Number(measured) + ",difference=" +
                    FramePoint.Number(gap));
                if (!FramePoint.IsFinite(measured) || !FramePoint.IsFinite(gap))
                    geometry.Add(label + " distance is not finite (" +
                        FramePoint.Number(measured) + " m)");
                else if (!(gap <= expected.PositionTolerance))
                    geometry.Add(label + " is " + FramePoint.Number(measured) +
                        " m, declared " + FramePoint.Number(declared) + " m");
            }
            outcomes.Add(Outcome(ImportedGeometryName,
                "the imported markers are the declared distances apart, measured in the prefab root frame",
                string.Join(";", geometryDetail.ToArray()) + ";importedInVisualFrame=" +
                    Points(rig.AuthoredInVisual), geometry, unread));

            // 5. Each root marker is at its authored marker, both in the
            //    prefab root frame.
            var roots = new List<string>();
            var rootDetail = new List<string>();
            foreach (KeyValuePair<string, string> pair in
                AuthoredMarkerRigExpectation.RootToAuthored)
            {
                FramePoint root, authored;
                bool hasRoot = rig.RootMarkers.TryGetValue(pair.Key, out root);
                bool rootFinite = hasRoot && root.Finite;
                bool hasAuthored = authoredInRoot.TryGetValue(pair.Value, out authored);
                string unusableMarker;
                unusableAuthored.TryGetValue(pair.Value, out unusableMarker);
                if (!hasRoot || !rootFinite || unusableMarker != null || !hasAuthored)
                {
                    roots.Add(pair.Key + " against " + pair.Value + " not evaluated: " +
                        (!hasRoot ? "the root marker is missing"
                            : !rootFinite ? NotFiniteRoot(pair.Key, root)
                            : unusableMarker != null ? unusableMarker
                            : "the authored marker or the Visual placement is unavailable"));
                    rootDetail.Add(pair.Key + ":expected=" + (hasAuthored
                        ? authored.Describe() : unusableMarker != null
                            ? "<unusable>" : "<unavailable>") + ",observed=" +
                        (hasRoot ? root.Describe() : "<missing>"));
                    continue;
                }
                double difference = root.DistanceTo(authored);
                rootDetail.Add(pair.Key + ":expected=" + authored.Describe() +
                    ",observed=" + root.Describe() + ",difference=" +
                    FramePoint.Number(difference));
                if (!FramePoint.IsFinite(difference))
                    roots.Add(pair.Key + " distance from " + pair.Value +
                        " carried into the prefab root frame is not finite");
                else if (!(difference <= expected.PositionTolerance))
                    roots.Add(pair.Key + " is " + FramePoint.Number(difference) +
                        " m from " + pair.Value + " carried into the prefab root frame");
            }
            outcomes.Add(Outcome(RootMarkersName,
                "the root's Grip, Muzzle and Butt are at KMG_Grip, KMG_Muzzle and KMG_Butt, each carried through the Visual placement into the prefab root frame",
                "frame=prefab-root;" + string.Join(";", rootDetail.ToArray()),
                roots, unread));

            // 6. The held frame the adapter is declared to produce.
            var held = new List<string>();
            var heldDetail = new List<string>();
            FramePoint grip, muzzle;
            bool hasGrip = rig.RootMarkers.TryGetValue(
                AuthoredMarkerRigExpectation.RootGrip, out grip);
            bool hasMuzzle = rig.RootMarkers.TryGetValue(
                AuthoredMarkerRigExpectation.RootMuzzle, out muzzle);
            double forwardLength = expected.HeldForward.Length;
            if (!hasGrip)
                held.Add("grip anchor not evaluated: the root Grip is missing");
            else if (!grip.Finite)
                held.Add("grip anchor not evaluated: " + NotFiniteRoot(
                    AuthoredMarkerRigExpectation.RootGrip, grip));
            else if (!expected.HeldAnchor.Finite)
                held.Add("grip anchor not evaluated: the declared held anchor is not finite " +
                    expected.HeldAnchor.Describe());
            else
            {
                double offset = grip.DistanceTo(expected.HeldAnchor);
                heldDetail.Add("grip:expected=" + expected.HeldAnchor.Describe() +
                    ",observed=" + grip.Describe() + ",difference=" +
                    FramePoint.Number(offset));
                if (!FramePoint.IsFinite(offset))
                    held.Add("the root Grip's distance from the held anchor is not finite");
                else if (!(offset <= expected.PositionTolerance))
                    held.Add("the root Grip is " + FramePoint.Number(offset) +
                        " m from the held anchor");
            }
            if (!hasGrip || !hasMuzzle)
                held.Add("bore not evaluated: the root Grip or Muzzle is missing");
            else if (!grip.Finite || !muzzle.Finite)
                held.Add("bore not evaluated: " + (!grip.Finite
                    ? NotFiniteRoot(AuthoredMarkerRigExpectation.RootGrip, grip)
                    : NotFiniteRoot(AuthoredMarkerRigExpectation.RootMuzzle, muzzle)));
            else if (!expected.HeldForward.Finite || !FramePoint.IsFinite(forwardLength))
                held.Add("bore not evaluated: the declared held forward is not finite " +
                    expected.HeldForward.Describe());
            else if (!FramePoint.IsFinite(muzzle.DistanceTo(grip)))
                held.Add("bore not evaluated: the distance from the root Grip to the root Muzzle is not finite");
            else if (forwardLength < 0.5 || muzzle.DistanceTo(grip) < 0.000001)
                held.Add("bore not evaluated: the declared forward or the observed bore is degenerate");
            else
            {
                FramePoint bore = muzzle.Minus(grip);
                double cosine = bore.Dot(expected.HeldForward) /
                    (bore.Length * forwardLength);
                double angle = FramePoint.IsFinite(cosine)
                    ? Math.Acos(Math.Max(-1.0, Math.Min(1.0, cosine))) * 180.0 / Math.PI
                    : double.NaN;
                heldDetail.Add("bore:expectedDirection=" + expected.HeldForward
                        .Times(1.0 / forwardLength).Describe() +
                    ",observedDirection=" + bore.Times(1.0 / bore.Length).Describe() +
                    ",angleDegrees=" + FramePoint.Number(angle));
                if (!FramePoint.IsFinite(angle))
                    held.Add("the angle between the bore and the held forward is not finite");
                else if (!(angle <= expected.AngleToleranceDegrees))
                    held.Add("the bore from Grip to Muzzle is " +
                        FramePoint.Number(angle) + " degrees off the held forward");
            }
            outcomes.Add(Outcome(HeldFrameName,
                "the root Grip is at the held donor anchor and the bore from Grip to Muzzle points along the held forward",
                "frame=prefab-root;" + string.Join(";", heldDetail.ToArray()),
                held, unread));

            return new AuthoredMarkerRigAudit
            {
                Variant = rig.Variant ?? "<unnamed>", Outcomes = outcomes
            };
        }

        /// <summary>
        /// The same relationship reported for several prefabs as one
        /// outcome under its own name, then the aggregate under the name the
        /// check has always had.
        /// </summary>
        internal static IList<AuditOutcome> Combine(
            IEnumerable<AuthoredMarkerRigAudit> audits, out AuditOutcome aggregate)
        {
            if (audits == null) throw new ArgumentNullException("audits");
            AuthoredMarkerRigAudit[] all = audits.ToArray();
            var combined = new List<AuditOutcome>();
            foreach (string name in new[] { MarkerSetName, TransformsName,
                SupportPolicyName, ImportedGeometryName, RootMarkersName,
                HeldFrameName })
            {
                AuditOutcome[] parts = all.Select(value => value.Outcome(name))
                    .ToArray();
                combined.Add(new AuditOutcome(name,
                    parts.Length == 0 ? "at least one prefab" : parts[0].Expected,
                    parts.Length == 0 ? "noPrefabInspected" : string.Join(";",
                        all.Select((value, index) => value.Variant + "[" +
                            (parts[index].Passed ? "holds" : "FAILS") + ";" +
                            parts[index].Observed + "]").ToArray()),
                    parts.Length != 0 && parts.All(value => value.Passed)));
            }
            string[] failed = combined.Where(value => !value.Passed)
                .Select(value => value.Name).ToArray();
            aggregate = new AuditOutcome(AggregateName,
                "every inspected prefab has its authored marker set, its required transforms, its support-target policy, its imported marker geometry, its root markers at the authored markers in the prefab root frame, and the declared held frame",
                "exact=" + (failed.Length == 0) + ";prefabs=" + string.Join(",",
                    all.Select(value => value.Variant).ToArray()) + ";failed=" +
                    (failed.Length == 0 ? "<none>" : string.Join("|", failed)),
                failed.Length == 0);
            return combined;
        }

        private static AuditOutcome Outcome(string name, string expected,
            string detail, ICollection<string> problems, string unread)
        {
            bool passed = unread == null && problems.Count == 0;
            return new AuditOutcome(name, expected, detail +
                (unread == null ? string.Empty : ";unread=" + unread) +
                (problems.Count == 0 ? string.Empty : ";problems=" +
                    string.Join("|", problems.ToArray())), passed);
        }

        private static string NotFiniteRoot(string name, FramePoint value)
        {
            return "the root " + name + " is not finite in the prefab root frame " +
                value.Describe();
        }

        private static string Points(IDictionary<string, FramePoint> points)
        {
            return points.Count == 0 ? "<none>" : string.Join(",", points
                .OrderBy(pair => pair.Key, StringComparer.Ordinal)
                .Select(pair => pair.Key + "=" + pair.Value.Describe()).ToArray());
        }
    }
}
