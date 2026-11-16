using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using KingmakerGunslinger.Presentation;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.DomainTests
{
    /// <summary>
    /// The named pistols' marker contract, on the production evaluation the
    /// firearm visual-rig scenario uses. The expected positions in these
    /// tests are worked out by hand for quarter-turn placements, where the
    /// result of a rotation can be read off without computing it.
    /// </summary>
    internal static class AuthoredMarkerRigContractTests
    {
        private const double HalfRoot = 0.70710678118654757;

        // A quarter turn about +Y carries the source +Z axis onto +X, so a
        // source point (0, 0, z) lands at Position + (z, 0, 0).
        private static readonly FramePoint Anchor = new FramePoint(0.5, -0.25, 2);

        private static FramePlacement QuarterTurnAboutY(FramePoint position,
            double scale)
        {
            return new FramePlacement(position, 0, HalfRoot, 0, HalfRoot,
                new FramePoint(scale, scale, scale));
        }

        private static AuthoredMarkerRigExpectation Expectation()
        {
            return AuthoredMarkerRigExpectation.NamedPistol(Anchor,
                new FramePoint(1, 0, 0));
        }

        // A prefab that satisfies the contract under the quarter turn: its
        // root markers are written out by hand, not computed.
        private static AuthoredMarkerRigObservation ValidRig(string variant)
        {
            var rig = new AuthoredMarkerRigObservation
            {
                Variant = variant, InstancePresent = true, VisualPresent = true,
                SupportHandTargetPresent = false,
                Visual = QuarterTurnAboutY(Anchor, 1)
            };
            foreach (string marker in AuthoredMarkerRigExpectation.AuthoredMarkers)
                rig.AuthoredMarkerCounts[marker] = 1;
            rig.AuthoredInVisual["KMG_Grip"] = new FramePoint(0, 0, 0);
            rig.AuthoredInVisual["KMG_Support"] = new FramePoint(0, -0.020, 0.145);
            rig.AuthoredInVisual["KMG_Butt"] = new FramePoint(0, 0, -0.075);
            rig.AuthoredInVisual["KMG_Muzzle"] = new FramePoint(0, 0, 0.264);
            rig.RootMarkers["Grip"] = new FramePoint(0.5, -0.25, 2);
            rig.RootMarkers["Muzzle"] = new FramePoint(0.764, -0.25, 2);
            rig.RootMarkers["Butt"] = new FramePoint(0.425, -0.25, 2);
            return rig;
        }

        private static void Contains(string text, string fragment, string message)
        {
            Assertions.True(text != null && text.Contains(fragment),
                message + " Reported: " + text);
        }

        private static void OnlyFails(AuthoredMarkerRigAudit audit,
            params string[] names)
        {
            string[] failed = audit.Outcomes.Where(value => !value.Passed)
                .Select(value => value.Name).ToArray();
            Assertions.True(failed.OrderBy(value => value, StringComparer.Ordinal)
                .SequenceEqual(names.OrderBy(value => value,
                    StringComparer.Ordinal)),
                "Expected exactly [" + string.Join(",", names) + "] to fail; failed: [" +
                string.Join(",", failed) + "]. " + string.Join(" / ",
                    audit.Outcomes.Where(value => !value.Passed)
                        .Select(value => value.Observed).ToArray()));
        }

        internal static void DeclaredSourceMarkersAreTheGeneratorsMarkers()
        {
            // The source coordinates the contract declares against the build
            // report the model generator wrote for the two named pistols.
            JObject report = JObject.Parse(File.ReadAllText(Path.Combine(
                Environment.CurrentDirectory, "assets-source", "original-models",
                "firearm-pistol-variants",
                "firearm-pistol-variants-build-report.json")));
            IDictionary<string, FramePoint> declared = Expectation().Source;
            JObject[] variants = report["variants"].OfType<JObject>().ToArray();
            Assertions.True(variants.Select(value => (string)value["variant"])
                .SequenceEqual(new[] { "Pistol.Duelist", "Pistol.LastWord" }),
                "The generator's report does not cover exactly the two named pistols.");
            foreach (JObject variant in variants)
            {
                var markers = (JObject)variant["markersMeters"];
                Assertions.Equal(declared.Count, markers.Count,
                    (string)variant["variant"] + " does not declare the four markers.");
                foreach (KeyValuePair<string, FramePoint> pair in declared)
                {
                    double[] reported = markers[pair.Key].Select(value =>
                        (double)value).ToArray();
                    Assertions.True(reported.Length == 3 &&
                        Math.Abs(reported[0] - pair.Value.X) < 1e-9 &&
                        Math.Abs(reported[1] - pair.Value.Y) < 1e-9 &&
                        Math.Abs(reported[2] - pair.Value.Z) < 1e-9,
                        (string)variant["variant"] + " " + pair.Key +
                        " differs from the declared source coordinate " +
                        pair.Value.Describe() + ".");
                }
            }
            Assertions.Equal(0.339, (double)report["semanticLengthMeters"],
                "The generator's butt-to-muzzle length is not 0.339 m.");
        }

        internal static void ValidRigInARotatedFramePassesForBothVariants()
        {
            foreach (string variant in new[] { "Pistol.Duelist", "Pistol.LastWord" })
            {
                AuthoredMarkerRigAudit audit = AuthoredMarkerRigAudit.Evaluate(
                    ValidRig(variant), Expectation());
                OnlyFails(audit);
                Assertions.Equal(variant, audit.Variant, "The variant is not reported.");
                Contains(audit.Outcome(AuthoredMarkerRigAudit.RootMarkersName).Observed,
                    "frame=prefab-root;Grip:expected=(0.5,-0.25,2),observed=(0.5,-0.25,2),difference=0;" +
                    "Muzzle:expected=(0.764,-0.25,2),observed=(0.764,-0.25,2),difference=0;" +
                    "Butt:expected=(0.425,-0.25,2),observed=(0.425,-0.25,2),difference=0",
                    "The expected and observed positions are not reported in one frame.");
                Contains(audit.Outcome(AuthoredMarkerRigAudit.ImportedGeometryName)
                    .Observed, "KMG_Grip-KMG_Muzzle:declared=0.264,observed=0.264",
                    "The grip-to-muzzle length is not reported.");
                Contains(audit.Outcome(AuthoredMarkerRigAudit.ImportedGeometryName)
                    .Observed, "KMG_Butt-KMG_Muzzle:declared=0.339,observed=0.339",
                    "The butt-to-muzzle length is not reported.");
                Contains(audit.Outcome(AuthoredMarkerRigAudit.HeldFrameName).Observed,
                    "angleDegrees=0", "The bore angle is not reported.");
            }
        }

        internal static void RawSourceCoordinateIsNotTheRootPosition()
        {
            // The retired comparison: the root Muzzle against the marker's
            // coordinate in the source model's frame. Under a quarter turn
            // that coordinate names a different point of the prefab.
            AuthoredMarkerRigObservation rotated = ValidRig("Pistol.Duelist");
            rotated.RootMarkers["Muzzle"] = new FramePoint(0, 0, 0.264);
            AuthoredMarkerRigAudit wrong = AuthoredMarkerRigAudit.Evaluate(rotated,
                Expectation());
            AuditOutcome roots = wrong.Outcome(AuthoredMarkerRigAudit.RootMarkersName);
            Assertions.False(roots.Passed,
                "A root Muzzle at the raw source coordinate passed in a rotated frame.");
            // (0.764, -0.25, 2) to (0, 0, 0.264): sqrt(0.764^2 + 0.25^2 + 1.736^2).
            Contains(roots.Observed,
                "Muzzle:expected=(0.764,-0.25,2),observed=(0,0,0.264),difference=1.9130844",
                "The two positions and their distance are not reported.");
            Contains(roots.Observed,
                "problems=Muzzle is 1.9130844 m from KMG_Muzzle carried into the prefab root frame",
                "The failed relationship is not named.");
            Assertions.True(wrong.Outcome(AuthoredMarkerRigAudit.MarkerSetName).Passed,
                "The marker counts were affected by the root position.");

            // Only with the Visual placed at the identity do the two frames
            // coincide, which is the one case the retired comparison fitted.
            AuthoredMarkerRigObservation identity = ValidRig("Pistol.Duelist");
            identity.Visual = FramePlacement.Identity;
            identity.RootMarkers["Grip"] = new FramePoint(0, 0, 0);
            identity.RootMarkers["Muzzle"] = new FramePoint(0, 0, 0.264);
            identity.RootMarkers["Butt"] = new FramePoint(0, 0, -0.075);
            AuthoredMarkerRigAudit coincident = AuthoredMarkerRigAudit.Evaluate(
                identity, AuthoredMarkerRigExpectation.NamedPistol(
                    new FramePoint(0, 0, 0), new FramePoint(0, 0, 1)));
            OnlyFails(coincident);
            // The same identity-frame prefab is not the held frame.
            OnlyFails(AuthoredMarkerRigAudit.Evaluate(identity, Expectation()),
                AuthoredMarkerRigAudit.HeldFrameName);
        }

        internal static void DisplacedRootMuzzleFailsDespiteCorrectMarkerCounts()
        {
            // One centimetre further along the bore: only the root-marker
            // relationship can see it.
            AuthoredMarkerRigObservation along = ValidRig("Pistol.LastWord");
            along.RootMarkers["Muzzle"] = new FramePoint(0.774, -0.25, 2);
            AuthoredMarkerRigAudit first = AuthoredMarkerRigAudit.Evaluate(along,
                Expectation());
            OnlyFails(first, AuthoredMarkerRigAudit.RootMarkersName);
            Contains(first.Outcome(AuthoredMarkerRigAudit.RootMarkersName).Observed,
                "Muzzle:expected=(0.764,-0.25,2),observed=(0.774,-0.25,2),difference=0.01",
                "The displacement is not reported with both positions.");
            Contains(first.Outcome(AuthoredMarkerRigAudit.RootMarkersName).Observed,
                "Grip:expected=(0.5,-0.25,2),observed=(0.5,-0.25,2),difference=0",
                "The sound Grip was not evaluated beside the displaced Muzzle.");
            Contains(first.Outcome(AuthoredMarkerRigAudit.MarkerSetName).Observed,
                "KMG_Grip:1,KMG_Support:1,KMG_Butt:1,KMG_Muzzle:1",
                "The marker counts are not reported.");

            // One centimetre to the side: the bore also leaves the held
            // forward, by atan(0.01 / 0.264) = 2.169 degrees.
            AuthoredMarkerRigObservation aside = ValidRig("Pistol.LastWord");
            aside.RootMarkers["Muzzle"] = new FramePoint(0.764, -0.24, 2);
            AuthoredMarkerRigAudit second = AuthoredMarkerRigAudit.Evaluate(aside,
                Expectation());
            OnlyFails(second, AuthoredMarkerRigAudit.RootMarkersName,
                AuthoredMarkerRigAudit.HeldFrameName);
            Contains(second.Outcome(AuthoredMarkerRigAudit.HeldFrameName).Observed,
                "angleDegrees=2.169", "The bore angle is not reported.");

            // Inside the tolerance the check has always used.
            AuthoredMarkerRigObservation close = ValidRig("Pistol.LastWord");
            close.RootMarkers["Muzzle"] = new FramePoint(0.76405, -0.25, 2);
            OnlyFails(AuthoredMarkerRigAudit.Evaluate(close, Expectation()));
        }

        internal static void MissingAndDuplicatedMarkersAreNamed()
        {
            // KMG_Butt absent and KMG_Muzzle present twice. A marker that is
            // not there exactly once has no single position to carry over.
            AuthoredMarkerRigObservation rig = ValidRig("Pistol.Duelist");
            rig.AuthoredMarkerCounts["KMG_Butt"] = 0;
            rig.AuthoredInVisual.Remove("KMG_Butt");
            rig.AuthoredMarkerCounts["KMG_Muzzle"] = 2;
            rig.AuthoredInVisual.Remove("KMG_Muzzle");
            AuthoredMarkerRigAudit audit = AuthoredMarkerRigAudit.Evaluate(rig,
                Expectation());
            OnlyFails(audit, AuthoredMarkerRigAudit.MarkerSetName,
                AuthoredMarkerRigAudit.ImportedGeometryName,
                AuthoredMarkerRigAudit.RootMarkersName);
            Contains(audit.Outcome(AuthoredMarkerRigAudit.MarkerSetName).Observed,
                "KMG_Grip:1,KMG_Support:1,KMG_Butt:0,KMG_Muzzle:2;problems=KMG_Butt is missing|KMG_Muzzle occurs 2 times",
                "The missing and duplicated markers are not named.");
            // The relationship that can still be evaluated is, and holds.
            AuditOutcome roots = audit.Outcome(AuthoredMarkerRigAudit.RootMarkersName);
            Contains(roots.Observed,
                "Grip:expected=(0.5,-0.25,2),observed=(0.5,-0.25,2),difference=0",
                "The Grip was not evaluated beside the unusable markers.");
            Contains(roots.Observed, "Muzzle against KMG_Muzzle not evaluated",
                "The unevaluated Muzzle relationship is not reported as such.");
            Contains(roots.Observed, "Butt against KMG_Butt not evaluated",
                "The unevaluated Butt relationship is not reported as such.");
            Assertions.True(audit.Outcome(AuthoredMarkerRigAudit.HeldFrameName).Passed &&
                audit.Outcome(AuthoredMarkerRigAudit.SupportPolicyName).Passed,
                "A relationship that does not read the authored markers failed.");
        }

        internal static void SupportTargetAndRequiredTransformsAreTheirOwnResults()
        {
            AuthoredMarkerRigObservation supported = ValidRig("Pistol.Duelist");
            supported.SupportHandTargetPresent = true;
            AuthoredMarkerRigAudit first = AuthoredMarkerRigAudit.Evaluate(supported,
                Expectation());
            OnlyFails(first, AuthoredMarkerRigAudit.SupportPolicyName);
            Contains(first.Outcome(AuthoredMarkerRigAudit.SupportPolicyName).Observed,
                "supportHandTarget=True;problems=a one-handed rig carries a support-hand target",
                "The unexpected support target is not reported.");

            AuthoredMarkerRigObservation bare = ValidRig("Pistol.Duelist");
            bare.RootMarkers.Remove("Muzzle");
            AuthoredMarkerRigAudit second = AuthoredMarkerRigAudit.Evaluate(bare,
                Expectation());
            OnlyFails(second, AuthoredMarkerRigAudit.TransformsName,
                AuthoredMarkerRigAudit.RootMarkersName,
                AuthoredMarkerRigAudit.HeldFrameName);
            Contains(second.Outcome(AuthoredMarkerRigAudit.TransformsName).Observed,
                "problems=Muzzle is missing or unusable",
                "The missing root Muzzle is not named.");
            Contains(second.Outcome(AuthoredMarkerRigAudit.RootMarkersName).Observed,
                "Muzzle against KMG_Muzzle not evaluated: the root marker is missing",
                "The missing root marker is not the reported reason.");
            // The grip anchor is still judged without the Muzzle.
            Contains(second.Outcome(AuthoredMarkerRigAudit.HeldFrameName).Observed,
                "grip:expected=(0.5,-0.25,2),observed=(0.5,-0.25,2),difference=0",
                "The grip anchor was not evaluated without the Muzzle.");

            // A prefab that could not be read fails everything and says why.
            AuthoredMarkerRigAudit unread = AuthoredMarkerRigAudit.Evaluate(
                new AuthoredMarkerRigObservation
                {
                    Variant = "Pistol.Duelist", InstancePresent = true,
                    Fault = "MissingReferenceException: destroyed"
                }, Expectation());
            Assertions.True(unread.Outcomes.All(value => !value.Passed &&
                value.Observed.Contains(
                    "unread=the prefab could not be read: MissingReferenceException: destroyed")),
                "An unreadable prefab passed a relationship or lost its reason.");
            Assertions.True(AuthoredMarkerRigAudit.Evaluate(null, Expectation())
                .Outcomes.All(value => !value.Passed),
                "A missing observation passed a relationship.");
        }

        internal static void HeldFrameIsJudgedApartFromMarkerAgreement()
        {
            // A prefab whose root markers agree with its authored markers
            // but which is anchored two centimetres from the held anchor.
            var moved = new FramePoint(0.52, -0.25, 2);
            AuthoredMarkerRigObservation shifted = ValidRig("Pistol.Duelist");
            shifted.Visual = QuarterTurnAboutY(moved, 1);
            shifted.RootMarkers["Grip"] = moved;
            shifted.RootMarkers["Muzzle"] = new FramePoint(0.784, -0.25, 2);
            shifted.RootMarkers["Butt"] = new FramePoint(0.445, -0.25, 2);
            AuthoredMarkerRigAudit first = AuthoredMarkerRigAudit.Evaluate(shifted,
                Expectation());
            OnlyFails(first, AuthoredMarkerRigAudit.HeldFrameName);
            Contains(first.Outcome(AuthoredMarkerRigAudit.HeldFrameName).Observed,
                "grip:expected=(0.5,-0.25,2),observed=(0.52,-0.25,2),difference=0.02",
                "The anchor offset is not reported with both positions.");

            // The same agreement under a quarter turn about +X instead,
            // which carries the source +Z axis onto -Y: the bore is a right
            // angle off the held forward.
            AuthoredMarkerRigObservation turned = ValidRig("Pistol.Duelist");
            turned.Visual = new FramePlacement(Anchor, HalfRoot, 0, 0, HalfRoot,
                new FramePoint(1, 1, 1));
            turned.RootMarkers["Muzzle"] = new FramePoint(0.5, -0.514, 2);
            turned.RootMarkers["Butt"] = new FramePoint(0.5, -0.175, 2);
            AuthoredMarkerRigAudit second = AuthoredMarkerRigAudit.Evaluate(turned,
                Expectation());
            OnlyFails(second, AuthoredMarkerRigAudit.HeldFrameName);
            Contains(second.Outcome(AuthoredMarkerRigAudit.HeldFrameName).Observed,
                "angleDegrees=90;problems=the bore from Grip to Muzzle is 90 degrees off the held forward",
                "The bore's departure from the held forward is not reported.");
        }

        internal static void ImportedGeometryMustKeepTheDeclaredDistances()
        {
            // The model placed at twice its size, with the root markers
            // following it: they agree with each other and both are wrong.
            AuthoredMarkerRigObservation doubled = ValidRig("Pistol.LastWord");
            doubled.Visual = QuarterTurnAboutY(Anchor, 2);
            doubled.RootMarkers["Muzzle"] = new FramePoint(1.028, -0.25, 2);
            doubled.RootMarkers["Butt"] = new FramePoint(0.35, -0.25, 2);
            AuthoredMarkerRigAudit audit = AuthoredMarkerRigAudit.Evaluate(doubled,
                Expectation());
            OnlyFails(audit, AuthoredMarkerRigAudit.ImportedGeometryName);
            Contains(audit.Outcome(AuthoredMarkerRigAudit.ImportedGeometryName).Observed,
                "KMG_Grip-KMG_Muzzle is 0.528 m, declared 0.264 m",
                "The stretched grip-to-muzzle length is not reported.");
            Contains(audit.Outcome(AuthoredMarkerRigAudit.ImportedGeometryName).Observed,
                "KMG_Butt-KMG_Muzzle is 0.678 m, declared 0.339 m",
                "The stretched overall length is not reported.");
        }

        internal static void CombinedOutcomesReportEachPrefab()
        {
            AuthoredMarkerRigObservation displaced = ValidRig("Pistol.LastWord");
            displaced.RootMarkers["Muzzle"] = new FramePoint(0.774, -0.25, 2);
            AuditOutcome aggregate;
            IList<AuditOutcome> combined = AuthoredMarkerRigAudit.Combine(new[]
            {
                AuthoredMarkerRigAudit.Evaluate(ValidRig("Pistol.Duelist"),
                    Expectation()),
                AuthoredMarkerRigAudit.Evaluate(displaced, Expectation())
            }, out aggregate);
            Assertions.True(combined.Select(value => value.Name).SequenceEqual(new[]
            {
                AuthoredMarkerRigAudit.MarkerSetName,
                AuthoredMarkerRigAudit.TransformsName,
                AuthoredMarkerRigAudit.SupportPolicyName,
                AuthoredMarkerRigAudit.ImportedGeometryName,
                AuthoredMarkerRigAudit.RootMarkersName,
                AuthoredMarkerRigAudit.HeldFrameName
            }), "The relationships are not reported under their own names.");
            AuditOutcome roots = combined.Single(value =>
                value.Name == AuthoredMarkerRigAudit.RootMarkersName);
            Assertions.False(roots.Passed, "One displaced prefab passed the pair.");
            Contains(roots.Observed, "Pistol.Duelist[holds;",
                "The sound prefab is not reported as holding.");
            Contains(roots.Observed, "Pistol.LastWord[FAILS;",
                "The displaced prefab is not reported as failing.");
            Assertions.Equal(AuthoredMarkerRigAudit.AggregateName, aggregate.Name,
                "The aggregate lost its established name.");
            Assertions.False(aggregate.Passed, "The aggregate passed.");
            Contains(aggregate.Observed,
                "exact=False;prefabs=Pistol.Duelist,Pistol.LastWord;failed=pistol-root-markers-in-held-frame",
                "The aggregate does not name the one failed relationship.");

            AuthoredMarkerRigAudit.Combine(new[]
            {
                AuthoredMarkerRigAudit.Evaluate(ValidRig("Pistol.Duelist"),
                    Expectation()),
                AuthoredMarkerRigAudit.Evaluate(ValidRig("Pistol.LastWord"),
                    Expectation())
            }, out aggregate);
            Assertions.True(aggregate.Passed, "Two sound prefabs did not pass: " +
                aggregate.Observed);
            Contains(aggregate.Observed, "exact=True;", "The aggregate is not exact.");

            // Nothing inspected is not a pass.
            AuthoredMarkerRigAudit.Combine(new AuthoredMarkerRigAudit[0],
                out aggregate);
            Assertions.False(aggregate.Passed, "No prefab at all passed the contract.");
        }
    }
}
