using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kingmaker;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.View;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        private ExpandedSummoningCorrectionFixture _serpentineBodyFixture;
        private IEnumerator<int> _serpentineBodySteps;
        private readonly List<RuntimeTestAssertion> _serpentineBodyAssertions = new List<RuntimeTestAssertion>();
        private readonly JArray _serpentineBodyRows = new JArray();
        private int _serpentineBodyLoadingFrames;
        private bool _serpentineBodyComplete;

        private void PollSprint17Bodies()
        {
            if (_serpentineBodyComplete) return;
            try
            {
                if (_serpentineBodySteps == null)
                {
                    string loading;
                    if (ExpandedSummoningLoadingActive(out loading))
                    {
                        if (++_serpentineBodyLoadingFrames < ExpandedSummoningLoadingGateFrames) return;
                        throw new InvalidOperationException("Native loading did not settle: " + loading);
                    }
                    _serpentineBodyFixture = BeginExpandedSummoningCorrectionFixture("KMG_Runtime_Sprint17_BodyCaster");
                    _serpentineBodySteps = ReviewSprint17Bodies(_serpentineBodyFixture).GetEnumerator();
                }
                if (_serpentineBodySteps.MoveNext()) return;
            }
            catch (Exception error)
            {
                _serpentineBodyAssertions.Add(Assertion("sprint17-body-exception", "bounded original-body review",
                    DescribeExpandedSummoningCorrectionException(error), false, "No gameplay/publication qualification."));
            }
            FinishSprint17Bodies();
        }

        private IEnumerable<int> ReviewSprint17Bodies(ExpandedSummoningCorrectionFixture fixture)
        {
            bool pause = Game.Instance.IsPaused;
            UnityEngine.Random.State random = UnityEngine.Random.state;
            try
            {
                Game.Instance.IsPaused = false;
                foreach (string key in SerpentineVisualPolicy.Keys)
                {
                    string donorKey = SerpentineVisualPolicy.IsSnake(key) ? "purple-worm" : "salamander";
                    UnitEntityData unit = CastExpandedSummoningVariant(fixture.Blueprints, fixture.Caster,
                        ExpandedSummoningOwnTierVariant(donorKey, SummonMultiplicity.One), null, fixture.Evidence).Single();
                    fixture.Created.Add(unit);
                    if (fixture.UnitsBefore.Any(value => ReferenceEquals(value, unit)))
                        throw new InvalidOperationException("Body review must own every mutated actor.");
                    SetExpandedSummoningBrainActive(unit, false);
                    var nativeBlueprint = unit.Blueprint;
                    var faction = unit.Faction;
                    var attackFactions = unit.AttackFactions.ToArray();
                    var master = unit.Descriptor.Master;
                    var size = unit.Descriptor.State.Size;
                    Vector3 scale = unit.View.transform.localScale;
                    UnityEngine.Object[] resources = new UnityEngine.Object[0];
                    var row = new JObject { ["key"] = key, ["sourceCreature"] = donorKey,
                        ["scope"] = "original body binding/movement/lifetime only; not printed profile or attack/contact qualification" };
                    _serpentineBodyRows.Add(row);
                    try
                    {
                        // Only the disposable visual carriers become Medium.
                        // Their donor stats/attacks are NOT claimed as the new
                        // profiles, and no attack command is issued here.
                        unit.Descriptor.State.Size = Size.Medium;
                        if (SerpentineVisualPolicy.IsSnake(key)) unit.View.transform.localScale = Vector3.one * .2f;
                        string floorEvidence;
                        Vector3 floor = FindExpandedSummoningUngulateArtPoint(out floorEvidence);
                        PlaceExpandedSummoningUnit(unit, floor);
                        if (!Game.Instance.State.AwakeUnits.Contains(unit)) Game.Instance.State.AwakeUnits.Add(unit);
                        row["floorSurvey"] = floorEvidence;
                        row["researchViewScale"] = SurveyVector(unit.View.transform.localScale);
                        // Native capital/manual identity adaptation already
                        // qualified on the laptop. No player/party/area edit.
                        if (Game.Instance.CurrentlyLoadedArea != null && Game.Instance.CurrentlyLoadedArea.IsCapital)
                        {
                            if (!unit.IsDirectlyControllable) unit.Descriptor.Master = Game.Instance.Player.MainCharacter;
                        }
                        else if (!unit.Faction.IsDirectlyControllable)
                        {
                            var main = Game.Instance.Player.MainCharacter.Value;
                            if (main == null || !main.Faction.IsDirectlyControllable)
                                throw new InvalidOperationException("No native controllable faction for owned movement fixture.");
                            unit.Descriptor.SwitchFactions(main.Faction, false);
                        }
                        if (!unit.IsDirectlyControllable) throw new InvalidOperationException("Native manual-control predicate refused the owned fixture.");

                        // Let appearance/fader settle naturally. No appearance
                        // buff removal, renderer enabling or visibility write.
                        for (int settle = 0; settle < 60; settle++) yield return 0;
                        int frames = 60;
                        while (++frames <= 600 && !Sprint17BodyIntact(unit.View, SerpentineVisualPolicy.BodyRenderer(key)))
                            yield return 0;
                        _serpentineBodyAssertions.Add(Assertion("sprint17-native-appearance-" + key,
                            "native visible/intact frame before attachment", "frames=" + frames,
                            Sprint17BodyIntact(unit.View, SerpentineVisualPolicy.BodyRenderer(key)),
                            "No visibility forcing; native appearance lock/fader."));
                        var originalSkins = unit.View.GetComponentsInChildren<SkinnedMeshRenderer>(true)
                            .Select(value => new { Renderer = value, Mesh = value.sharedMesh,
                                Bones = value.bones, Root = value.rootBone }).ToArray();
                        var originalStatics = unit.View.GetComponentsInChildren<MeshFilter>(true)
                            .Select(value => new { Filter = value, Mesh = value.sharedMesh }).ToArray();
                        UnityEngine.Object[] rollbackResources = new UnityEngine.Object[0];
                        string outcome;
                        bool failedAttach;
                        try
                        {
                            SerpentineVisualAttachment.PostSwapFaultForTest = () => {
                                rollbackResources = unit.View.GetComponent<SerpentineVisualAttachment>().CaptureOwnedResources();
                                throw new InvalidOperationException("owned-body rollback drill");
                            };
                            failedAttach = !SerpentineVisualAttachment.TryAttach(unit.View, key, _context, out outcome);
                        }
                        finally { SerpentineVisualAttachment.PostSwapFaultForTest = null; }
                        yield return 0; yield return 0;
                        _serpentineBodyAssertions.Add(Assertion("sprint17-body-rollback-" + key,
                            "injected post-swap fault restores every native skin/bone/root/static mesh and destroys exact owned resources",
                            outcome + ";captured=" + rollbackResources.Length,
                            failedAttach && rollbackResources.Length >= 5 &&
                                originalSkins.All(value => value.Renderer.sharedMesh == value.Mesh &&
                                    value.Renderer.bones.SequenceEqual(value.Bones) && value.Renderer.rootBone == value.Root) &&
                                originalStatics.All(value => value.Filter.sharedMesh == value.Mesh) &&
                                rollbackResources.All(value => value == null),
                            "Exact object references, not a name-only resource count."));
                        if (!SerpentineVisualAttachment.TryAttach(unit.View, key, _context, out outcome))
                            throw new InvalidOperationException("Original body attachment failed: " + outcome);
                        var attachment = unit.View.GetComponent<SerpentineVisualAttachment>();
                        resources = attachment.CaptureOwnedResources();
                        row["attachment"] = outcome;
                        for (int frame = 0; frame < 90; frame++) yield return 0;
                        _serpentineBodyAssertions.Add(Assertion("sprint17-body-intact-" + key,
                            "original on native body renderer; all audited auxiliary geometry absent; native blueprint unchanged",
                            outcome, Sprint17BodyIntact(unit.View, SerpentineVisualPolicy.BodyRenderer(key)) &&
                                ReferenceEquals(unit.Blueprint, nativeBlueprint) &&
                                unit.View.GetComponentsInChildren<SkinnedMeshRenderer>(true).Where(value => value != attachment.Body)
                                    .All(value => value.sharedMesh != null && value.sharedMesh.vertexCount == 0) &&
                                unit.View.GetComponentsInChildren<MeshFilter>(true)
                                    .All(value => value.sharedMesh != null && value.sharedMesh.vertexCount == 0),
                            "Body-only hybrid: no claim that missing club/shield constitutes a spear."));

                        Vector3 origin = unit.Position, destination = Sprint17BodyMoveDestination(origin);
                        var move = new UnitMoveTo(destination, .3f);
                        move.Init(unit);
                        if (!move.CanStart) throw new InvalidOperationException("Native original-body move cannot start.");
                        unit.Commands.Run(move);
                        if (!unit.Commands.Contains(move)) throw new InvalidOperationException("Native move was not queued.");
                        var samples = new JArray();
                        float travel = 0, speed = 0;
                        int sampleFrame = 0;
                        DateTime deadline = DateTime.UtcNow.AddSeconds(30);
                        while (DateTime.UtcNow < deadline && sampleFrame < 1200)
                        {
                            yield return 0;
                            sampleFrame++;
                            travel = Math.Max(travel, Vector3.Distance(origin, unit.Position));
                            var agent = unit.View.MovementAgent as UnitMovementAgent;
                            if (agent != null) speed = Math.Max(speed, agent.Velocity.magnitude);
                            if (sampleFrame % 10 == 0 && samples.Count < 80)
                                samples.Add(Sprint17OriginalBodySample(unit, attachment.Body));
                            if (sampleFrame >= 120 && Vector3.Distance(destination, unit.Position) <= .65f) break;
                        }
                        unit.Commands.InterruptMove();
                        row["movementSamples"] = samples;
                        row["travel"] = travel; row["maxVelocity"] = speed;
                        bool finite = samples.Count >= 3 && samples.Cast<JObject>().All(value =>
                            (bool)value["finite"] && (float)value["height"] > .10f);
                        _serpentineBodyAssertions.Add(Assertion("sprint17-native-body-movement-" + key,
                            "real native movement travels at least one metre with nonzero agent velocity and finite skinned geometry",
                            "travel=" + travel + ";velocity=" + speed + ";samples=" + samples.Count,
                            travel >= 1f && speed > .05f && finite,
                            "Weighted world vertices recorded separately from actor movement; not attack/contact acceptance."));
                        row["supportingFrame"] = CaptureSprint17OriginalBodyFrame(unit, attachment.Body, key);
                    }
                    finally
                    {
                        SerpentineVisualAttachment.PostSwapFaultForTest = null;
                        if (unit != null && !unit.Destroyed)
                        {
                            unit.Commands.InterruptMove();
                            unit.Descriptor.Master = master;
                            unit.Descriptor.SwitchFactions(faction, false);
                            unit.AttackFactions.Match(attackFactions);
                            unit.Descriptor.State.Size = size;
                            if (unit.View != null) unit.View.transform.localScale = scale;
                            unit.Destroy();
                            Game.Instance.EntityDestroyer.Tick();
                        }
                    }
                    yield return 0; yield return 0;
                    _serpentineBodyAssertions.Add(Assertion("sprint17-body-owned-resource-cleanup-" + key,
                        "native unit destruction releases every exact project-owned resource",
                        "captured=" + resources.Length + ";remaining=" + resources.Count(value => value != null),
                        resources.Length >= 5 && resources.All(value => value == null),
                        "No direct view disposal, native resource destruction or global name sweep."));
                }
            }
            finally
            { SerpentineVisualAttachment.PostSwapFaultForTest = null; Game.Instance.IsPaused = pause; UnityEngine.Random.state = random; }
        }

        private static bool Sprint17BodyIntact(UnitEntityView view, string bodyName)
        {
            if (view == null) return false;
            var body = view.GetComponentsInChildren<SkinnedMeshRenderer>(true).SingleOrDefault(value => value.name == bodyName);
            return body != null && body.enabled && body.gameObject.activeInHierarchy && body.sharedMesh != null &&
                body.sharedMaterials.Length > 0 && body.sharedMaterials.All(material => material != null &&
                    (!material.HasProperty("_Dissolve") || material.GetFloat("_Dissolve") < .02f));
        }

        private static Vector3 Sprint17BodyMoveDestination(Vector3 origin)
        {
            if (AstarPath.active == null) throw new InvalidOperationException("No native path graph.");
            var start = AstarPath.active.GetNearest(origin);
            foreach (Vector3 direction in CompassOffsets)
            {
                Vector3 requested = origin + direction * 2.5f;
                var end = AstarPath.active.GetNearest(requested);
                if (start.node != null && end.node != null && end.node.Walkable &&
                    end.node.Area == start.node.Area && end.node.GraphIndex == start.node.GraphIndex &&
                    Vector3.Distance(requested, end.clampedPosition) < .5f) return end.clampedPosition;
            }
            throw new InvalidOperationException("No connected original-body movement route.");
        }

        private static JObject Sprint17OriginalBodySample(UnitEntityData unit, SkinnedMeshRenderer body)
        {
            Vector3[] points = Sprint17OriginalWorldVertices(body);
            bool finite = points.All(point => !float.IsNaN(point.x + point.y + point.z) &&
                !float.IsInfinity(point.x + point.y + point.z));
            return new JObject { ["frame"] = Time.frameCount, ["finite"] = finite,
                ["actorPosition"] = SurveyVector(unit.Position), ["vertices"] = points.Length,
                ["lowestAboveActor"] = points.Min(point => point.y) - unit.Position.y,
                ["height"] = points.Max(point => point.y) - points.Min(point => point.y),
                ["headViewPosition"] = SurveyVector(unit.View.transform.InverseTransformPoint(
                    body.bones.Single(bone => bone.name == "Head").position)),
                ["worldPoseMethod"] = "sum(weight * bone.localToWorld * bindpose * originalVertex)" };
        }

        private static Vector3[] Sprint17OriginalWorldVertices(SkinnedMeshRenderer body)
        {
            Mesh mesh = body.sharedMesh; Transform[] bones = body.bones;
            Matrix4x4[] bind = mesh.bindposes;
            Matrix4x4[] skin = bones.Select((bone, index) => bone.localToWorldMatrix * bind[index]).ToArray();
            Vector3[] vertices = mesh.vertices; BoneWeight[] weights = mesh.boneWeights;
            return vertices.Select((vertex, index) => {
                BoneWeight w = weights[index];
                return skin[w.boneIndex0].MultiplyPoint3x4(vertex) * w.weight0 +
                    skin[w.boneIndex1].MultiplyPoint3x4(vertex) * w.weight1 +
                    skin[w.boneIndex2].MultiplyPoint3x4(vertex) * w.weight2 +
                    skin[w.boneIndex3].MultiplyPoint3x4(vertex) * w.weight3;
            }).ToArray();
        }

        private string CaptureSprint17OriginalBodyFrame(UnitEntityData unit, SkinnedMeshRenderer body, string key)
        {
            Camera source = Camera.main;
            if (source == null) return "not-captured:no-native-camera";
            Vector3[] points = Sprint17OriginalWorldVertices(body);
            var bounds = new Bounds(points[0], Vector3.zero);
            foreach (Vector3 point in points) bounds.Encapsulate(point);
            var carrier = new GameObject("KMG_Sprint17OwnedReviewCamera");
            RenderTexture target = null; Texture2D texture = null;
            RenderTexture prior = RenderTexture.active;
            try
            {
                Camera camera = carrier.AddComponent<Camera>(); camera.CopyFrom(source); camera.enabled = false;
                camera.orthographic = true; camera.aspect = 1.5f;
                camera.orthographicSize = Math.Max(bounds.size.magnitude * .62f, .6f);
                camera.nearClipPlane = .01f; camera.farClipPlane = 100f;
                carrier.transform.position = bounds.center + new Vector3(1, .65f, 1).normalized *
                    Math.Max(bounds.size.magnitude * 2, 3f);
                carrier.transform.LookAt(bounds.center);
                target = new RenderTexture(1200, 800, 24); camera.targetTexture = target;
                camera.Render(); RenderTexture.active = target;
                texture = new Texture2D(1200, 800, TextureFormat.RGBA32, false);
                texture.ReadPixels(new Rect(0, 0, 1200, 800), 0, 0); texture.Apply(false, false);
                string name = "sprint17-" + key + "-owned-body.png";
                File.WriteAllBytes(Path.Combine(_request.EvidenceDirectory, name), EncodeExpandedSummoningPng(texture));
                return name + ";supporting-art-only;native-camera-untouched";
            }
            finally
            {
                RenderTexture.active = prior;
                if (texture != null) UnityEngine.Object.DestroyImmediate(texture);
                if (target != null) { target.Release(); UnityEngine.Object.DestroyImmediate(target); }
                UnityEngine.Object.DestroyImmediate(carrier);
            }
        }

        private void CleanupSprint17Bodies()
        {
            var steps = _serpentineBodySteps;
            _serpentineBodySteps = null;
            var fixture = _serpentineBodyFixture;
            _serpentineBodyFixture = null;
            try { if (steps != null) steps.Dispose(); }
            catch (Exception error) { _serpentineBodyAssertions.Add(Assertion("sprint17-body-disposal", "iterator cleanup", error.Message, false, "owned-only")); }
            bool cleaned = false;
            try { EndExpandedSummoningCorrectionFixture(fixture, out cleaned); }
            catch (Exception error) { _serpentineBodyAssertions.Add(Assertion("sprint17-body-cleanup-error", "native cleanup", error.Message, false, "owned-only")); }
            _serpentineBodyAssertions.Add(Assertion("sprint17-body-fixture-cleanup", "exact original unit/party/area references",
                "cleaned=" + cleaned, cleaned, "No save write or unrelated-unit cleanup."));
        }

        // Called by Complete on an outer timeout/error too. Disposal executes
        // the iterator's actor/control/pause/random finally blocks before the
        // working-save sentinels and evidence trace close.
        private void StopSprint17Bodies(RuntimeTestResult result)
        {
            if (_serpentineBodySteps == null && _serpentineBodyFixture == null) return;
            CleanupSprint17Bodies();
            result.Assertions.AddRange(_serpentineBodyAssertions);
            result.Diagnostics.Add("Original-body review interrupted; iterator and owned fixture cleaned.");
            if (_serpentineBodyAssertions.Any(value => value.Status != "PASS")) result.Status = "FAIL";
            try { WriteSprint17BodyReview(); }
            catch (Exception error) { result.Status = "ERROR"; result.Diagnostics.Add(error.ToString()); }
            _serpentineBodyComplete = true;
        }

        private void WriteSprint17BodyReview()
        {
            File.WriteAllText(Path.Combine(_request.EvidenceDirectory, "sprint17-original-body-review.json"),
                _serpentineBodyRows.ToString(Formatting.Indented));
        }

        private void FinishSprint17Bodies()
        {
            CleanupSprint17Bodies();
            _serpentineBodyAssertions.Add(Assertion("loaded-mod-version", _request.ExpectedModVersion,
                _context.ModEntry.Info.Version, _request.ExpectedModVersion == _context.ModEntry.Info.Version, "Loaded UMM version."));
            WriteSprint17BodyReview();
            _serpentineBodyComplete = true;
            Complete(CreateResult(_serpentineBodyAssertions.All(value => value.Status == "PASS") ? "PASS" : "FAIL",
                _serpentineBodyAssertions, null));
        }
    }
}
