using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Kingmaker;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.Visual.Animation.Kingmaker;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        // Implemented by the integrator's independent palette partial. An
        // absent optional implementation cannot silently qualify Cloud/Storm.
        partial void Cleanup2ApplyPrototypePalette(UnitEntityData actor, ref IDisposable lease, ref JObject evidence);

        private sealed class Cleanup2ScaleSource
        {
            internal UnitEntityData Actor;
            internal int ViewId;
            internal Vector3 RootScale;
            internal Renderer[] Renderers;
            internal Material[][] Materials;
            internal bool[] Enabled;
            internal SkinnedMeshRenderer[] Skins;
            internal Transform[][] Bones;
            internal Transform[] RootBones;
            internal Mesh[] Meshes;
            internal Bounds[] Bounds;
            internal MeshFilter[] Filters;
            internal Mesh[] StaticMeshes;
            internal Cloth[] Cloths;
            internal string[] ClothConfigurations;
            internal HashSet<int> ColliderIds;
            internal string ColliderConfiguration, Mechanics;
            internal double GameTime, Remaining;
        }

        private sealed class Cleanup2ScaleClothMotion
        {
            internal Cloth Cloth;
            internal int Samples, ValidSamples, Count;
            internal Vector3[] First;
            internal float MaximumDisplacement;
        }

        private IEnumerable<int> ReviewSummonCleanup2ScalePrototype(ExpandedSummoningCorrectionFixture fixture,
            IList<UnitEntityData> actors, IList<string> keys, IList<Vector3> groundPoints,
            IList<Cleanup2ActorMetric[]> beforeSamples, bool standingProof)
        {
            string[] expectedKeys = { "human-six-foot-art-anchor", "hill-giant", "stone-giant", "frost-giant", "fire-giant", "cloud-giant", "storm-giant" };
            bool admitted = IsSummonCleanup2Review && _request.ExitAfterCompletion &&
                (string)_request.Parameters?["reviewPhase"] == "scale-prototype" && standingProof &&
                actors.Count == 7 && keys.SequenceEqual(expectedKeys) && ReferenceEquals(actors[0], fixture.Caster) &&
                groundPoints.Count == actors.Count && beforeSamples.Count == 4 &&
                beforeSamples.All(sample => sample.Length == actors.Count && sample.All(value => value.Valid));
            Cleanup2Check("scale-prototype-admission", admitted, new JObject { ["standingProof"] = standingProof,
                ["actorCount"] = actors.Count, ["baselineSampleCount"] = beforeSamples.Count,
                ["baselineImage"] = "cleanup2-scale-prototype-before.png", ["productionActivation"] = false },
                "Only the exact canonical common-plane lineup with four finite stable current-body samples admits this relative render-only experiment; the Human remains an unchanged control.");
            if (!admitted) throw new InvalidOperationException("UNKNOWN: scale prototype refused without a complete standing/current-pose baseline.");
            Cleanup2ActorMetric[] before = beforeSamples.Last();
            var original = actors.Select(Cleanup2CaptureScaleSource).ToArray();
            var prototypes = new List<Cleanup2GiantRenderScalePrototype>();
            var palettes = new List<IDisposable>();
            var clothMotion = new List<Cleanup2ScaleClothMotion>();
            var factors = new float[actors.Count];
            var targets = new float[actors.Count];
            var watch = Stopwatch.StartNew();
            bool waiting = _sprint25WaitingForUnityAppearance, paused = Game.Instance.IsPaused;
            UnitAnimationActionHandle animation = null;
            Cleanup2GiantRenderScalePrototype.DisabledCapsuleContactProbe contact = null;
            try
            {
                for (int i = 1; i < actors.Count; i++)
                {
                    var profile = GiantSummonPresentationPolicy.All.Single(value => value.Key == keys[i] && value.UnitGuid == actors[i].Blueprint.AssetGuid);
                    targets[i] = profile.ReferenceHeightFeet;
                    factors[i] = profile.RelativeVisualFactor;
                    if (!Cleanup2Finite(factors[i]) || factors[i] <= 0f)
                        throw new InvalidOperationException("Non-finite/nonpositive measured prototype factor: " + keys[i]);
                }
                for (int i = 1; i < actors.Count; i++)
                {
                    IDisposable palette = null; JObject paletteEvidence = null;
                    try { Cleanup2ApplyPrototypePalette(actors[i], ref palette, ref paletteEvidence); }
                    finally { if (palette != null) palettes.Add(palette); }
                    bool paletteReady = paletteEvidence != null && (bool?)paletteEvidence["passed"] == true;
                    Cleanup2Check("scale-prototype-palette-" + keys[i], paletteReady, new JObject {
                        ["key"] = keys[i], ["unit"] = actors[i].UniqueId, ["paletteLease"] = palette != null,
                        ["evidence"] = paletteEvidence,
                        ["limit"] = "Stone/Fire/Cloud/Storm require exact ordinary production palette ownership before helper intake. Hill/Frost retain their native palette. The observer never releases a production owner; art acceptance remains separate." },
                        "Palette ownership/proof is settled before the scale helper captures material identities.");
                    if (!paletteReady) throw new NotSupportedException("UNKNOWN: exact prototype palette proof absent for " + keys[i]);
                    Cleanup2GiantRenderScalePrototype owner = Cleanup2TakeScalePrototype(fixture, actors, actors[i], keys[i], factors[i], beforeSamples.Average(values => values[i].Sole.y));
                    prototypes.Add(owner);
                    foreach (Cloth cloth in owner.OwnedCloths) clothMotion.Add(new Cleanup2ScaleClothMotion { Cloth = cloth });
                    Cleanup2Check("scale-prototype-take-" + keys[i], true, new JObject {
                        ["key"] = keys[i], ["unit"] = actors[i].UniqueId,
                        ["beforeBodyHeightWorldUnits"] = before[i].Height, ["unchangedHumanBodyHeightWorldUnits"] = before[0].Height,
                        ["relativeFamilyBaseline"] = "hill-giant", ["proposedRelativeHeight"] = factors[i], ["absoluteUniformFactor"] = factors[i],
                        ["fireProposal"] = keys[i] == "fire-giant" ? "14 ft is a provisional art choice within the owner's 12-16 ft range, not a uniquely specified PF height." : null,
                        ["helper"] = owner.Describe() },
                        "The selected relative family ratio applies once to an owned visual skeleton; native root/skeleton/collider dimensions remain governed by invariance checks.");
                }
                contact = Cleanup2GiantRenderScalePrototype.DisabledCapsuleContactProbe.Take(prototypes[keys.IndexOf("frost-giant") - 1]);
                _sprint25WaitingForUnityAppearance = true;
                Game.Instance.IsPaused = false;
                // Let newly owned native Cloth solvers receive bounded Unity
                // updates before asking for their first nonempty particle set.
                for (int frame = 0; frame < 12; frame++)
                {
                    if (watch.Elapsed.TotalSeconds > 120) throw new TimeoutException("Scale prototype wall bound.");
                    yield return 0;
                    contact.Observe();
                }
                var afterSamples = new List<Cleanup2ActorMetric[]>();
                for (int sample = 0; sample < 4; sample++)
                {
                    for (int frame = 0; frame < 12; frame++)
                    {
                        if (watch.Elapsed.TotalSeconds > 120) throw new TimeoutException("Scale prototype wall bound.");
                        Cleanup2ScaleObserveCloths(clothMotion); contact.Observe(); yield return 0;
                    }
                    foreach (var owner in prototypes) Cleanup2RequireScalePrototype(owner);
                    var measured = actors.Select((actor, i) => Cleanup2MeasureActor(actor,
                        "prototype-" + keys[i], groundPoints[0], Vector3.up)).ToArray();
                    afterSamples.Add(measured);
                    Cleanup2Check("scale-prototype-current-pose-" + sample, measured.All(value => value.Valid),
                        new JObject { ["sample"] = sample, ["worldUp"] = SurveyVector(Vector3.up),
                            ["basisOrigin"] = SurveyVector(groundPoints[0]), ["actors"] = new JArray(measured.Select(value => value.Document)) },
                        "Actual current-pose transient baked surfaces, with verified skin-to-world convention, provide post-prototype crown/sole/height; no culling bounds substitute.");
                }
                JObject contactResult = contact.Describe();
                Cleanup2Check("scale-prototype-disabled-capsule-contact", (bool?)contactResult["passed"] == true,
                    contactResult, "Two original no-seam authored cloth grids prove a stable particle frame, positive motion and a disabled-capsule response against an otherwise identical no-collider control; no enabled physics collider is permitted.");
                contact.Dispose();
                yield return 0;
                JObject contactRelease = contact.Describe();
                Cleanup2Check("scale-prototype-contact-control-released",
                    (bool?)contactRelease["allOwnedResourcesReleased"] == true && (bool?)contactRelease["borrowedNativeMaterialAlive"] == true,
                    contactRelease, "The bounded authored control releases its own grids/components only and preserves the borrowed native material.");
                var ratios = new JArray(); bool ratiosValid = true;
                for (int i = 0; i < actors.Count; i++)
                {
                    float height = afterSamples.Average(values => values[i].Height);
                    float range = afterSamples.Max(values => values[i].Height) - afterSamples.Min(values => values[i].Height);
                    float groundGap = afterSamples.Max(values => Mathf.Abs(values[i].Sole.y - groundPoints[i].y));
                    float baselineSoleOffset = beforeSamples.Average(values => values[i].Sole.y - groundPoints[i].y);
                    float ownFactor = i == 0 ? 1f : factors[i];
                    float expectedSoleOffset = baselineSoleOffset;
                    float soleOffsetError = afterSamples.Max(values => Mathf.Abs(values[i].Sole.y - groundPoints[i].y - expectedSoleOffset));
                    float expected = i == 0 ? before[i].Height : before[i].Height * factors[i];
                    float tolerance = .05f * (i == 0 ? 1f : factors[i]) + .01f;
                    bool exact = afterSamples.All(values => values[i].Valid) && Mathf.Abs(height - expected) <= tolerance &&
                        range <= .05f * ownFactor && soleOffsetError <= tolerance;
                    ratiosValid &= exact;
                    ratios.Add(new JObject { ["key"] = keys[i], ["height"] = height, ["poseHeightRange"] = range,
                        ["expectedUsingMeasuredFactor"] = expected, ["heightError"] = height - expected,
                        ["poseComparisonTolerance"] = tolerance, ["maximumOwnGroundContactGap"] = groundGap,
                        ["nativeBaselineSoleOffset"] = baselineSoleOffset, ["expectedScaledSoleOffset"] = expectedSoleOffset,
                        ["maximumSoleOffsetError"] = soleOffsetError,
                        ["heightOverCurrentHuman"] = height / afterSamples.Average(values => values[0].Height),
                        ["heightOverCurrentHill"] = height / afterSamples.Average(values => values[1].Height),
                        ["proposedRelativeHeight"] = ownFactor, ["currentPoseCompatible"] = exact });
                }
                bool humanUnmodified = Cleanup2ScaleSourceRestored(original[0]);
                Cleanup2Check("scale-prototype-measured-height-ratios", ratiosValid && humanUnmodified,
                    new JObject { ["actors"] = ratios, ["humanUnmodified"] = humanUnmodified,
                        ["toleranceMeaning"] = "Existing5cm native idle-pose variation scales with the selected visual ratio, plus1cm skinning allowance. The stable owned sole pivot must preserve the measured baseline sole plane rather than amplify its navmesh gap. Physical ground contact still requires actual native frame review." },
                    "Post-prototype heights follow the selected family ratios while the stable owned sole pivot preserves the observed native foot plane; Human and native gameplay roots remain unchanged.");
                if (!ratiosValid || !humanUnmodified) throw new InvalidOperationException("UNKNOWN: post-prototype current-pose height/ground or unchanged Human proof failed.");
                JObject afterPicture=null;
                foreach(int captureFrame in Cleanup2CaptureLineup(fixture, actors, keys, afterSamples.Last(), value=>afterPicture=value,
                    "cleanup2-scale-prototype-after.png", true))yield return captureFrame;
                Cleanup2Check("scale-prototype-after-lineup", (bool?)afterPicture?["captureQualified"]==true, afterPicture,
                    "A distinct actual native-camera after image retains Human and all six experimental families; before evidence is preserved.");

                _sprint25WaitingForUnityAppearance = false;
                for (int i = 1; i < actors.Count; i++)
                {
                    var hand = actors[i].Body.PrimaryHand;
                    if (hand == null || !hand.HasWeapon) throw new NotSupportedException("No actual primary weapon animation input: " + keys[i]);
                    var info = new AttackHandInfo(hand, 0, 0); info.CreateAnimationHandleForAttack();
                    animation = info.AnimationHandle;
                    if (animation != null) actors[i].View.AnimationManager.Execute(animation);
                    Cleanup2Check("scale-prototype-native-pose-" + keys[i], animation != null,
                        new JObject { ["key"] = keys[i], ["unit"] = actors[i].UniqueId,
                            ["weapon"] = info.Weapon.Blueprint.AssetGuid, ["action"] = animation?.Action?.GetType().FullName,
                            ["attackWeaponStyle"] = animation?.AttackWeaponStyle.ToString(), ["attackCommandOrRuleIssued"] = false },
                        "The actual native weapon-specific animation supplies a pose stimulus, not mechanical combat acceptance.");
                    if (animation == null) throw new InvalidOperationException("UNKNOWN: native weapon pose handle absent for " + keys[i]);
                    for (int frame = 0; frame < 66; frame++)
                    {
                        if (watch.Elapsed.TotalSeconds > 120) throw new TimeoutException("Scale prototype wall bound.");
                        foreach (var owner in prototypes) Cleanup2RequireScalePrototype(owner);
                        Cleanup2ScaleObserveCloths(clothMotion);
                        if (frame == 30)
                        {
                            var pose = actors.Select((actor, index) => Cleanup2MeasureActor(actor,
                                "prototype-weapon-pose-" + keys[index], groundPoints[0], Vector3.up)).ToArray();
                            JObject posePicture=null;
                            foreach(int captureFrame in Cleanup2CaptureLineup(fixture, actors, keys, pose, value=>posePicture=value,
                                "cleanup2-scale-prototype-weapon-" + keys[i] + ".png", true))yield return captureFrame;
                            Cleanup2Check("scale-prototype-native-pose-frame-" + keys[i], pose.All(value => value.Valid) && (bool?)posePicture?["captureQualified"]==true, posePicture,
                                "An actual native-camera weapon-pose frame supports visual inspection; native weapon motion is not a combat-resolution test.");
                        }
                        Game.Instance.IsPaused = false; yield return 0;
                    }
                    Cleanup2ScaleFinishAnimation(ref animation);
                }
                var mechanical = new JArray(); bool nativeExact = true;
                for (int i = 0; i < actors.Count; i++)
                {
                    bool same = Cleanup2ScaleMechanics(actors[i]).ToString() == original[i].Mechanics &&
                        Cleanup2ScaleOriginalColliders(original[i]) == original[i].ColliderConfiguration && actors[i].View.transform.localScale == original[i].RootScale;
                    nativeExact &= same;
                    mechanical.Add(new JObject { ["key"] = keys[i], ["unchangedNativeMechanicsRootAndColliderConfiguration"] = same,
                        ["mechanics"] = Cleanup2ScaleMechanics(actors[i]), ["beforeRemainingSeconds"] = original[i].Remaining,
                        ["afterRemainingSeconds"] = Cleanup2ScaleRemaining(actors[i]),
                        ["naturalFixtureGameTimeDelta"] = Game.Instance.Player.GameTime.TotalSeconds - original[i].GameTime });
                }
                bool clothPositive = clothMotion.Count > 0 && clothMotion.All(value => value.Samples >= 4 && value.ValidSamples == value.Samples && value.MaximumDisplacement > .0001f);
                Cleanup2Check("scale-prototype-native-invariants-and-cloth-observation", nativeExact && clothPositive,
                    new JObject { ["actors"] = mechanical,
                        ["helpers"] = new JArray(prototypes.Select(value => value.Describe())),
                        ["ownedCloths"] = new JArray(clothMotion.Select(value => new JObject {
                            ["instance"] = value.Cloth == null ? 0 : value.Cloth.GetInstanceID(), ["samples"] = value.Samples,
                            ["validSamples"] = value.ValidSamples, ["particleCount"] = value.Count,
                            ["positiveRawPoseDisplacement"] = value.MaximumDisplacement, ["positiveUpdateProved"] = value.MaximumDisplacement > .0001f })),
                        ["disabledCapsuleCollisionResponse"] = "UNKNOWN_PENDING_BOUNDED_NATIVE_CONTACT_AND_VISUAL_INSPECTION",
                        ["clothDistanceScaling"] = "Linear factor applied only to owned solver maxDistance/collisionSphereDistance as an explicit art proposal; calibration UNKNOWN, not accepted.",
                        ["nativeDonorControls"] = "PENDING_SEPARATE_UNTOUCHED_DONOR_AND_NEGATIVE_PROVENANCE_OBSERVATION",
                        ["visualAndUsabilityAcceptance"] = "PENDING_ACTUAL_FRAME_INSPECTION; component presence and numeric invariants do not prove appearance/collision response." },
                    "Original mechanics/root/collider configuration stay exact while owned Cloth has finite stable-count positive updates; collision and distance acceptance remain explicitly unknown.");
            }
            finally
            {
                var failures = new List<string>();
                Action<string, Action> restore = (label, action) => { try { action(); } catch (Exception error) { failures.Add(label + ":" + error.GetType().Name + ":" + error.Message); } };
                bool restored = false;
                var disposed = new JArray();
                try
                {
                    restore("animation", () => Cleanup2ScaleFinishAnimation(ref animation));
                    restore("contact-control", () => { if (contact != null) contact.Dispose(); });
                    foreach (var actor in actors.Skip(1)) restore("owned-commands-" + actor.UniqueId, () => actor.Commands.InterruptAll(true));
                    for (int i = prototypes.Count - 1; i >= 0; i--) { var owner = prototypes[i]; restore("prototype-" + i, owner.Dispose); }
                    for (int i = palettes.Count - 1; i >= 0; i--) { var palette = palettes[i]; restore("palette-" + i, palette.Dispose); }
                    restore("original-state-observation", () => restored = original.All(Cleanup2ScaleSourceRestored));
                    foreach (var owner in prototypes) restore("disposed-helper-report", () => {
                        JObject report = owner.Describe(); disposed.Add(report);
                        var issues = report["restoreIssues"] as JArray;
                        if ((bool?)report["disposed"] != true || issues == null || issues.Count > 0)
                            failures.Add("helper-disposal-not-proven:" + report.ToString());
                    });
                }
                finally
                {
                    _sprint25WaitingForUnityAppearance = waiting;
                    restore("pause", () => Game.Instance.IsPaused = paused);
                }
                Cleanup2Check("scale-prototype-original-state-restored", restored && failures.Count == 0,
                    new JObject { ["failures"] = new JArray(failures), ["allOriginalRefsRootColliderMechanicsRestored"] = restored,
                        ["disposedHelpers"] = disposed,
                        ["productionCalibrationRegistered"] = false },
                    "Reverse prototype disposal precedes palette lease release, restoring original materials/skins/root/collider configuration even after partial refusal.");
                if (!restored || failures.Count > 0) throw new InvalidOperationException("Scale prototype cleanup/ref restoration failed: " + string.Join("|", failures.ToArray()));
            }
        }

        private Cleanup2GiantRenderScalePrototype Cleanup2TakeScalePrototype(ExpandedSummoningCorrectionFixture fixture,
            IList<UnitEntityData> actors, UnitEntityData actor, string key, float factor, float nativeSoleWorldY)
        {
            var inputs = new JArray();
            foreach (Cloth cloth in actor.View.GetComponentsInChildren<Cloth>(true))
            {
                var coefficients = cloth.coefficients;
                var fields = new JObject();
                foreach (string field in new[] { "maxDistance", "collisionSphereDistance" })
                {
                    float[] values = coefficients.Select(value => field == "maxDistance" ? value.maxDistance : value.collisionSphereDistance).ToArray();
                    float[] enabled = values.Where(value => Cleanup2Finite(value) && value >= 0f && value != float.MaxValue).ToArray();
                    fields[field] = new JObject {
                        ["disabledExactFloatMaxValueCount"] = values.Count(value => value == float.MaxValue),
                        ["zeroCount"] = values.Count(value => value == 0f),
                        ["negativeOrNonfiniteCount"] = values.Count(value => !Cleanup2Finite(value) || value < 0f),
                        ["enabledFiniteCount"] = enabled.Length,
                        ["enabledMinimumRoundTrip"] = enabled.Length == 0 ? null : enabled.Min().ToString("R", System.Globalization.CultureInfo.InvariantCulture),
                        ["enabledMaximumRoundTrip"] = enabled.Length == 0 ? null : enabled.Max().ToString("R", System.Globalization.CultureInfo.InvariantCulture) };
                }
                inputs.Add(new JObject { ["cloth"] = cloth.GetInstanceID(), ["name"] = cloth.name, ["enabled"] = cloth.enabled,
                    ["coefficientCount"] = coefficients.Length, ["fingerprint"] = Cleanup2ClothCoefficientFingerprint(coefficients),
                    ["fields"] = fields });
            }
            Cleanup2Check("scale-prototype-native-cloth-constraints-" + key, true, new JObject {
                ["key"] = key, ["unit"] = actor.UniqueId, ["factor"] = factor, ["nativeCloths"] = inputs,
                ["disabledSentinelRoundTrip"] = float.MaxValue.ToString("R", System.Globalization.CultureInfo.InvariantCulture),
                ["disabledSentinelBits"] = "0x7f7fffff", ["ownedSolverCreated"] = false,
                ["scope"] = "Read-only scalar/count/fingerprint intake before helper admission; no per-vertex geometry exported and no collision, scaling or art acceptance implied." },
                "Native coefficient fields, exact disabled-sentinel counts and enabled finite extrema are retained before any proposed arithmetic or owned solver mutation.");
            try
            {
                return Cleanup2GiantRenderScalePrototype.Take(_request, actor, fixture.Caster, actors.Skip(1).ToList(), factor,
                    new Cleanup2GiantRenderScalePrototype.ProposedClothDistances(factor, factor, "cleanup2-linear-cloth-distance-proposal-v1"), nativeSoleWorldY);
            }
            catch (Exception error)
            {
                Cleanup2Check("scale-prototype-refused-" + key, false, new JObject { ["key"] = key, ["unit"] = actor.UniqueId,
                    ["factor"] = factor, ["verdict"] = "UNKNOWN_UNSUPPORTED_OR_FOREIGN_INPUT_REFUSED",
                    ["reason"] = error.GetType().Name + ":" + error.Message },
                    "Unknown renderer/Cloth/ownership inputs refuse without a fallback or waived invariant.");
                throw;
            }
        }

        private static void Cleanup2RequireScalePrototype(Cleanup2GiantRenderScalePrototype owner)
        {
            owner.Synchronize(); JObject report = owner.Describe();
            if ((bool?)report["disposed"] == true || !((string)report["status"] ?? "").StartsWith("EXPERIMENT_ACTIVE", StringComparison.Ordinal) ||
                (bool?)report["nativeSourceTransformsSameFrameExact"] != true ||
                (bool?)report["nativeColliderMatricesSameFrameExact"] != true || (bool?)report["nativeColliderGeometryExact"] != true)
                throw new InvalidOperationException("Scale prototype refused or lost native invariance: " + report.ToString());
        }

        private static void Cleanup2ScaleObserveCloths(IEnumerable<Cleanup2ScaleClothMotion> values)
        {
            foreach (var value in values)
            {
                value.Samples++;
                if (value.Cloth == null || !value.Cloth.enabled) continue;
                Vector3[] points = value.Cloth.vertices;
                if (value.First == null) { value.First = points; value.Count = points.Length; }
                if (points.Length == 0 || points.Length != value.Count || points.Any(point => !Cleanup2Finite(point))) continue;
                value.ValidSamples++;
                for (int i = 0; i < points.Length; i++) value.MaximumDisplacement = Mathf.Max(value.MaximumDisplacement, Vector3.Distance(points[i], value.First[i]));
            }
        }

        private static Cleanup2ScaleSource Cleanup2CaptureScaleSource(UnitEntityData actor)
        {
            Renderer[] renderers = actor.View.GetComponentsInChildren<Renderer>(true);
            var skins = renderers.OfType<SkinnedMeshRenderer>().ToArray();
            var state = new Cleanup2ScaleSource { Actor = actor, ViewId = actor.View.GetInstanceID(), RootScale = actor.View.transform.localScale,
                Renderers = renderers, Materials = renderers.Select(value => value.sharedMaterials).ToArray(), Enabled = renderers.Select(value => value.enabled).ToArray(),
                Skins = skins, Bones = skins.Select(value => value.bones).ToArray(), RootBones = skins.Select(value => value.rootBone).ToArray(),
                Meshes = skins.Select(value => value.sharedMesh).ToArray(), Bounds = skins.Select(value => value.localBounds).ToArray(),
                Filters = actor.View.GetComponentsInChildren<MeshFilter>(true),
                Cloths = actor.View.GetComponentsInChildren<Cloth>(true),
                ColliderIds = new HashSet<int>(actor.View.GetComponentsInChildren<Collider>(true).Select(value => value.GetInstanceID())),
                Mechanics = Cleanup2ScaleMechanics(actor).ToString(), GameTime = Game.Instance.Player.GameTime.TotalSeconds,
                Remaining = Cleanup2ScaleRemaining(actor) };
            state.StaticMeshes = state.Filters.Select(value => value.sharedMesh).ToArray();
            state.ClothConfigurations = state.Cloths.Select(value => Cleanup2ClothConfiguration(value).ToString()).ToArray();
            state.ColliderConfiguration = Cleanup2ScaleOriginalColliders(state);
            return state;
        }

        private static bool Cleanup2ScaleSourceRestored(Cleanup2ScaleSource state)
        {
            if (state.Actor.View == null || state.Actor.View.GetInstanceID() != state.ViewId || state.Actor.View.transform.localScale != state.RootScale ||
                Cleanup2ScaleMechanics(state.Actor).ToString() != state.Mechanics || Cleanup2ScaleOriginalColliders(state) != state.ColliderConfiguration) return false;
            for (int i = 0; i < state.Renderers.Length; i++)
                if (state.Renderers[i] == null || state.Renderers[i].enabled != state.Enabled[i] || !state.Renderers[i].sharedMaterials.SequenceEqual(state.Materials[i])) return false;
            for (int i = 0; i < state.Skins.Length; i++)
                if (state.Skins[i] == null || state.Skins[i].sharedMesh != state.Meshes[i] || state.Skins[i].rootBone != state.RootBones[i] ||
                    !state.Skins[i].bones.SequenceEqual(state.Bones[i]) ||
                    (state.Skins[i].GetComponent<Cloth>() == null && !state.Skins[i].localBounds.Equals(state.Bounds[i]))) return false;
            for (int i = 0; i < state.Filters.Length; i++)
                if (state.Filters[i] == null || state.Filters[i].sharedMesh != state.StaticMeshes[i]) return false;
            for (int i = 0; i < state.Cloths.Length; i++)
                if (state.Cloths[i] == null || Cleanup2ClothConfiguration(state.Cloths[i]).ToString() != state.ClothConfigurations[i]) return false;
            return true;
        }

        private static string Cleanup2ScaleOriginalColliders(Cleanup2ScaleSource state)
        { return new JArray(Cleanup2ClothColliderState(state.Actor, false).OfType<JObject>().Where(value => state.ColliderIds.Contains((int)value["instance"]))).ToString(); }

        private static double Cleanup2ScaleRemaining(UnitEntityData actor)
        {
            Buff marker = actor.Descriptor.Buffs.RawFacts.OfType<Buff>().SingleOrDefault(value => value.Blueprint == BlueprintRoot.Instance.SystemMechanics.SummonedUnitBuff);
            return marker == null ? 0d : marker.TimeLeft.TotalSeconds;
        }

        private static JObject Cleanup2ScaleMechanics(UnitEntityData actor)
        {
            Component[] seekers = actor.View.GetComponentsInChildren<Component>(true).Where(value => value != null &&
                value.GetType().FullName == "Kingmaker.TurnBasedMode.Pathfinding.SeekerWithClearance").ToArray();
            if (seekers.Length != 1) throw new NotSupportedException("Exactly one native movement clearance seeker required.");
            FieldInfo clearance = seekers[0].GetType().GetField("clearance", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (clearance == null || clearance.FieldType != typeof(Vector3)) throw new MissingFieldException("Native seeker clearance Vector3 unavailable.");
            Buff marker = actor.Descriptor.Buffs.RawFacts.OfType<Buff>().SingleOrDefault(value => value.Blueprint == BlueprintRoot.Instance.SystemMechanics.SummonedUnitBuff);
            return new JObject { ["unit"] = actor.UniqueId, ["size"] = actor.Descriptor.State.Size.ToString(),
                ["corpulence"] = actor.Corpulence, ["viewCorpulence"] = actor.View.Corpulence,
                ["agentCorpulence"] = actor.View.MovementAgent?.Corpulence,
                ["nativeClearance"] = SurveyVector((Vector3)clearance.GetValue(seekers[0])),
                ["strength"] = actor.Stats.Strength.ModifiedValue, ["dexterity"] = actor.Stats.Dexterity.ModifiedValue,
                ["constitution"] = actor.Stats.Constitution.ModifiedValue, ["intelligence"] = actor.Stats.Intelligence.ModifiedValue,
                ["wisdom"] = actor.Stats.Wisdom.ModifiedValue, ["charisma"] = actor.Stats.Charisma.ModifiedValue,
                ["maximumHP"] = actor.Stats.HitPoints.ModifiedValue, ["damage"] = actor.Descriptor.Damage,
                ["AC"] = actor.Stats.AC.ModifiedValue, ["touchAC"] = actor.Stats.AC.Touch, ["flatAC"] = actor.Stats.AC.FlatFooted,
                ["BAB"] = actor.Stats.BaseAttackBonus.ModifiedValue, ["speed"] = actor.Stats.Speed.ModifiedValue,
                ["fortitude"] = actor.Stats.SaveFortitude.ModifiedValue, ["reflex"] = actor.Stats.SaveReflex.ModifiedValue,
                ["will"] = actor.Stats.SaveWill.ModifiedValue, ["summonEndTimeTicks"] = marker?.EndTime.Ticks,
                ["summonSource"] = marker?.MaybeContext?.SourceAbility?.AssetGuid, ["summoner"] = marker?.MaybeContext?.MaybeCaster?.UniqueId };
        }

        private static void Cleanup2ScaleFinishAnimation(ref UnitAnimationActionHandle handle)
        { if (handle == null) return; try { handle.IsActed = true; FinishExpandedSummoningAnimation(handle); } finally { handle = null; } }
    }
}
