using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.PubSubSystem;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Parts;
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
                    CaptureSprint17BodyEnvironment();
                    Kingmaker.UI.SettingsUI.SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue = false;
                    Game.Instance.TurnBasedCombatController.Activate();
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
                if (_request.Scenario == RuntimeTestScenarioCatalog.DisposableExpandedSummoningSerpentineBodies)
                {
                    foreach (int step in ReviewSprint17HumanSalamander(fixture)) yield return step;
                    yield break; // Closed current hybrid research; old Lizardfolk attempts remain historical evidence.
                }
                if (_request.Scenario == RuntimeTestScenarioCatalog.DisposableExpandedSummoningSnakeFinalReview)
                {
                    foreach (int step in ReviewSprint17SnakeFinalCases(fixture)) yield return step;
                    yield break; // Exact hidden32 routes, native UI and snake view lifecycle; no Salamander.
                }
                if (_request.Scenario == RuntimeTestScenarioCatalog.DisposableExpandedSummoningSnakeCommands)
                {
                    foreach (int step in ReviewSprint17SnakeCommands(fixture)) yield return step;
                    yield break; // Separate closed native-command slice; qualified rules requests unchanged.
                }
                foreach (int step in ReviewSprint17ProductionSnakeViews(fixture)) yield return step;
                if (_request.Scenario == RuntimeTestScenarioCatalog.DisposableExpandedSummoningSnakeSignatures)
                {
                    foreach (int step in ReviewSprint17SnakeProfiles(fixture)) yield return step;
                    foreach (int step in ReviewSprint17SnakeSignatures(fixture)) yield return step;
                    yield break; // Fixed two-snake rules slice; no Salamander or arbitrary assets.
                }
                if (_request.Scenario == RuntimeTestScenarioCatalog.DisposableExpandedSummoningSnakeProfiles)
                {
                    foreach (int step in ReviewSprint17SnakeProfiles(fixture)) yield return step;
                    foreach (int step in ReviewSprint17SalamanderProfile(fixture)) yield return step;
                    yield break; // Three exact Sprint17 profiles; no donor research, arbitrary keys or save writes.
                }
                foreach (string key in SerpentineVisualPolicy.Keys)
                {
                    string donorKey = SerpentineVisualPolicy.IsSnake(key) ? "purple-worm" : "salamander";
                    // Use the whole qualified manual-summon seam, including
                    // its native rule input. Faction/Master alone cannot make
                    // an AI-only UnitPartSummonedMonster controllable.
                    Sprint16ManualSummonControl control;
                    UnitEntityData unit = SummonSprint17BodyCarrier(fixture, key, out control);
                    if (!fixture.Created.Contains(unit)) fixture.Created.Add(unit);
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
                    Mesh borrowedSpear = null;
                    UnityEngine.Object[] borrowedAnimations = new UnityEngine.Object[0];
                    var row = new JObject { ["key"] = key, ["sourceCreature"] = donorKey,
                        ["scope"] = "request-local body/native attack/contact research; not printed profile or final visual qualification",
                        ["prefab"] = nativeBlueprint.Prefab.AssetId };
                    _serpentineBodyRows.Add(row);
                    try
                    {
                        var summonPart = unit.Get<UnitPartSummonedMonster>();
                        row["controlBefore"] = Sprint16ControlObservation(unit);
                        row["manualSummonRules"] = control.Matched;
                        if (control.Matched != 1 || summonPart == null || !summonPart.IsDirectlyControllable)
                            throw new InvalidOperationException("Owned manual-summon rule mismatch: " + row.ToString(Formatting.None));
                        // Only the disposable visual carriers become Medium.
                        // Their donor stats/attacks are NOT claimed as the new
                        // profiles. Native attacks below expose their actual
                        // contact measurements without claiming new profiles.
                        unit.Descriptor.State.Size = Size.Medium;
                        if (SerpentineVisualPolicy.IsSnake(key)) unit.View.transform.localScale = Vector3.one * .2f;
                        string floorEvidence;
                        Vector3 floor = FindExpandedSummoningArtPoint(fixture.Caster, out floorEvidence);
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
                        row["controlAfterSetup"] = Sprint16ControlObservation(unit);

                        // Let appearance/fader settle naturally. No appearance
                        // buff removal, renderer enabling or visibility write.
                        for (int settle = 0; settle < 60; settle++) yield return 0;
                        int frames = 60;
                        while (++frames <= 600 && (!Sprint17BodyIntact(unit.View, SerpentineVisualPolicy.BodyRenderer(key)) ||
                            !unit.IsDirectlyControllable))
                            yield return 0;
                        row["controlAfterNativeSettlement"] = Sprint16ControlObservation(unit);
                        _serpentineBodyAssertions.Add(Assertion("sprint17-native-control-" + key,
                            "one scoped manual summon rule plus settled native control predicate",
                            row["controlAfterNativeSettlement"].ToString(Formatting.None), unit.IsDirectlyControllable,
                            "Native summon-part input; no control-predicate patch, player/party mutation or appearance removal."));
                        if (!unit.IsDirectlyControllable)
                            throw new InvalidOperationException("Native manual-control predicate refused the owned fixture: " + row.ToString(Formatting.None));
                        _serpentineBodyAssertions.Add(Assertion("sprint17-native-appearance-" + key,
                            "native visible/intact frame before attachment", "frames=" + frames,
                            Sprint17BodyIntact(unit.View, SerpentineVisualPolicy.BodyRenderer(key)),
                            "No visibility forcing; native appearance lock/fader."));
                        var originalSkins = unit.View.GetComponentsInChildren<SkinnedMeshRenderer>(true)
                            .Select(value => new { Renderer = value, Mesh = value.sharedMesh,
                                Bones = value.bones, Root = value.rootBone, Quality = value.quality }).ToArray();
                        var originalAnimationSet = unit.View.AnimationManager.AnimationSet;
                        var originalStatics = unit.View.GetComponentsInChildren<MeshFilter>(true)
                            .Select(value => new { Filter = value, Mesh = value.sharedMesh,
                                Renderer = value.GetComponent<MeshRenderer>(),
                                Materials = value.GetComponent<MeshRenderer>().sharedMaterials,
                                Position = value.transform.localPosition, Rotation = value.transform.localRotation,
                                Scale = value.transform.localScale }).ToArray();
                        UnityEngine.Object[] rollbackResources = new UnityEngine.Object[0];
                        UnityEngine.Object[] rollbackBorrowedAnimations = new UnityEngine.Object[0];
                        string outcome;
                        bool failedAttach;
                        try
                        {
                            SerpentineVisualAttachment.PostSwapFaultForTest = () => {
                                rollbackResources = unit.View.GetComponent<SerpentineVisualAttachment>().CaptureOwnedResources();
                                rollbackBorrowedAnimations = unit.View.GetComponent<SerpentineVisualAttachment>().CaptureBorrowedAnimationResources();
                                throw new InvalidOperationException("owned-body rollback drill");
                            };
                            failedAttach = !SerpentineVisualAttachment.TryAttach(unit.View, key, _context, out outcome,
                                nativeSpearResearch: key == "salamander");
                        }
                        finally { SerpentineVisualAttachment.PostSwapFaultForTest = null; }
                        yield return 0; yield return 0;
                        _serpentineBodyAssertions.Add(Assertion("sprint17-body-rollback-" + key,
                            "injected post-swap fault restores every native skin/bone/root/static mesh and destroys exact owned resources",
                            outcome + ";captured=" + rollbackResources.Length,
                            failedAttach && rollbackResources.Length >= 5 &&
                                ReferenceEquals(unit.View.AnimationManager.AnimationSet, originalAnimationSet) &&
                                rollbackBorrowedAnimations.All(value => value != null) &&
                                originalSkins.All(value => value.Renderer.sharedMesh == value.Mesh &&
                                    value.Renderer.bones.SequenceEqual(value.Bones) && value.Renderer.rootBone == value.Root &&
                                    value.Renderer.quality == value.Quality) &&
                                originalStatics.All(value => value.Filter.sharedMesh == value.Mesh &&
                                    value.Renderer.sharedMaterials.SequenceEqual(value.Materials) &&
                                    value.Filter.transform.localPosition == value.Position &&
                                    value.Filter.transform.localRotation == value.Rotation &&
                                    value.Filter.transform.localScale == value.Scale) &&
                                rollbackResources.All(value => value == null),
                            "Exact object references, not a name-only resource count."));
                        if (!SerpentineVisualAttachment.TryAttach(unit.View, key, _context, out outcome,
                            nativeSpearResearch: key == "salamander"))
                            throw new InvalidOperationException("Original body attachment failed: " + outcome);
                        var attachment = unit.View.GetComponent<SerpentineVisualAttachment>();
                        resources = attachment.CaptureOwnedResources();
                        borrowedSpear = attachment.NativeSpearMesh;
                        borrowedAnimations = attachment.CaptureBorrowedAnimationResources();
                        if (key == "salamander" && resources.Any(value => ReferenceEquals(value, borrowedSpear)))
                            throw new InvalidOperationException("Borrowed native spear cannot enter owned-resource cleanup.");
                        row["attachment"] = outcome;
                        if (key == "salamander")
                        {
                            row["nativePiercingBinding"] = attachment.NativeSpearAnimationObservation;
                            _serpentineBodyAssertions.Add(Assertion("sprint17-native-piercing-action-binding",
                                "one exact compatible native piercing hand action; all other actions and native assets unchanged",
                                attachment.NativeSpearAnimationObservation, attachment.NativeSpearAnimationBound &&
                                    borrowedAnimations.Length >= 4 && !resources.Any(value => borrowedAnimations.Contains(value)),
                                "Owned set container only; no style relabel, native clip edit, forced animation or new skeleton."));
                            int support = Array.IndexOf(attachment.DriverNames, SerpentineVisualPolicy.HybridSupport);
                            if (support < 0 || attachment.Body.bones[support] != attachment.Body.transform ||
                                attachment.Body.quality != SkinQuality.Bone4 ||
                                attachment.Body.sharedMesh.bindposes[support] != Matrix4x4.Scale(new Vector3(-1, -1, 1)))
                                throw new InvalidOperationException("Hybrid support is not the exact original renderer-frame binding.");
                        }
                        for (int frame = 0; frame < 90; frame++) yield return 0;
                        _serpentineBodyAssertions.Add(Assertion("sprint17-body-intact-" + key,
                            "original on native body renderer; all audited auxiliary geometry absent; native blueprint unchanged",
                            outcome, Sprint17BodyIntact(unit.View, SerpentineVisualPolicy.BodyRenderer(key)) &&
                                ReferenceEquals(unit.Blueprint, nativeBlueprint) &&
                                unit.View.GetComponentsInChildren<SkinnedMeshRenderer>(true).Where(value => value != attachment.Body)
                                    .All(value => value.sharedMesh != null && value.sharedMesh.vertexCount == 0) &&
                                unit.View.GetComponentsInChildren<MeshFilter>(true)
                                    .All(value => value == attachment.SpearFilter
                                        ? ReferenceEquals(value.sharedMesh, attachment.NativeSpearMesh)
                                        : value.sharedMesh != null && value.sharedMesh.vertexCount == 0),
                            "Exact native spear reference is the only optional nonempty static; no grip/contact claim from attachment."));

                        row["idleSample"] = Sprint17OriginalBodySample(unit, attachment.Body);

                        Vector3 origin = unit.Position, destination = Sprint17BodyMoveDestination(origin);
                        var move = new UnitMoveTo(destination, .3f);
                        move.Init(unit);
                        if (!move.CanStart) throw new InvalidOperationException("Native original-body move cannot start.");
                        row["movementBefore"] = Sprint17NativeCommandState(unit, null, move);
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
                            {
                                var sample = Sprint17OriginalBodySample(unit, attachment.Body);
                                sample["command"] = Sprint17NativeCommandState(unit, null, move);
                                samples.Add(sample);
                            }
                            if (sampleFrame >= 120 && Vector3.Distance(destination, unit.Position) <= .65f) break;
                            if (move.IsFinished && travel < 1f) break;
                        }
                        row["movementAfter"] = Sprint17NativeCommandState(unit, null, move);
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
                        var measured = new[] { (JObject)row["idleSample"] }.Concat(samples.Cast<JObject>()).ToArray();
                        _serpentineBodyAssertions.Add(Assertion("sprint17-measured-floor-and-pose-" + key,
                            "each idle/movement sample has a measured native-mask floor and finite complete current skin transforms",
                            "samples=" + measured.Length + ";minimumClearance=" + measured.Min(value =>
                                (float?)value["lowestVertexFloor"]["clearance"]),
                            measured.Length >= 4 && measured.All(value => (bool)value["poseFinite"] &&
                                value["actorFloor"]["clearance"].Type == JTokenType.Float &&
                                value["lowestVertexFloor"]["clearance"].Type == JTokenType.Float &&
                                ((JArray)value["skinTransforms"]).Count == SerpentineVisualPolicy.Bones(key).Length),
                            "Research completeness only: negative clearance and visible gaps are retained, not waived."));
                        _serpentineBodyAssertions.Add(Assertion("sprint17-corrected-ground-support-" + key,
                            "every measured idle/movement pose has original support within -2..15mm of the actual floor",
                            "minimum=" + measured.Min(value => (float?)value["lowestVertexFloor"]["clearance"]) +
                                ";maximum=" + measured.Max(value => (float?)value["lowestVertexFloor"]["clearance"]),
                            measured.All(value => {
                                float? clearance = (float?)value["lowestVertexFloor"]["clearance"];
                                return clearance.HasValue && clearance.Value >= -.002f && clearance.Value <= .015f;
                            }), "Bounded sampled flat-ground proof only; not attack, slope, death or final visual qualification."));
                        row["supportingFrame"] = CaptureSprint17OriginalBodyFrame(unit, attachment.Body, key);
                        Mesh originalMesh = attachment.Body.sharedMesh;
                        if (!resources.Any(value => ReferenceEquals(value, originalMesh)))
                            throw new InvalidOperationException("Winding research must own this exact original mesh.");
                        int[] originalTriangles = originalMesh.triangles;
                        Vector3[] originalNormals = originalMesh.normals;
                        Material originalMaterial = attachment.Body.sharedMaterial;
                        row["materialResearch"] = Sprint17BodyMaterialResearch(originalMaterial);
                        try
                        {
                            originalMesh.triangles = SerpentineRigSurveyPolicy.ReverseOriginalTriangleOrder(
                                originalTriangles, originalMesh.vertexCount);
                            row["reverseWindingSupportingFrame"] = CaptureSprint17OriginalBodyFrame(
                                unit, attachment.Body, key + "-reverse-winding");
                        }
                        finally { originalMesh.triangles = originalTriangles; }
                        _serpentineBodyAssertions.Add(Assertion("sprint17-original-winding-probe-restored-" + key,
                            "only exact owned triangle order changes for one supporting art comparison, then restores",
                            "triangles=" + originalTriangles.Length / 3 + ";materialReferenceUnchanged=" +
                                ReferenceEquals(originalMaterial, attachment.Body.sharedMaterial),
                            originalMesh.triangles.SequenceEqual(originalTriangles) &&
                                originalMesh.normals.SequenceEqual(originalNormals) &&
                                ReferenceEquals(originalMaterial, attachment.Body.sharedMaterial),
                            "No native geometry, shader/culling, visibility, camera, bone, gameplay or save change. Not visual acceptance."));
                        if (key == "salamander") row["nativeSpearResearch"] = Sprint17NativeSpearResearch(unit);
                        foreach (int step in ReviewSprint17NativeAttacks(fixture, unit, attachment, key, row))
                            yield return step;
                    }
                    finally
                    {
                        SerpentineVisualAttachment.PostSwapFaultForTest = null;
                        if (unit != null && !unit.Destroyed)
                        {
                            InterruptExpandedSummoningFixtureCommands(unit);
                            unit.CombatState.LeaveCombat();
                            Game.Instance.Player.UpdateIsInCombat();
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
                        resources.Length >= 5 && resources.All(value => value == null) &&
                            (key != "salamander" || borrowedSpear != null && borrowedAnimations.Length >= 4 &&
                                borrowedAnimations.All(value => value != null)),
                        "Borrowed native spear/sets/actions/clips remain alive; owned animation container dies; no direct disposal or global sweep."));
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
            int lowest = Enumerable.Range(0, points.Length).OrderBy(index => points[index].y).First();
            Matrix4x4[] bind = body.sharedMesh.bindposes;
            Transform[] bones = body.bones;
            Matrix4x4[] skin = bones.Select((bone, index) => bone.localToWorldMatrix * bind[index]).ToArray();
            bool poseFinite = skin.All(matrix => Enumerable.Range(0, 16)
                .All(index => SerpentineRigSurveyPolicy.Finite(matrix[index / 4, index % 4])));
            var transforms = new JArray(bones.Select((bone, index) => new JObject {
                ["name"] = body.GetComponentInParent<SalamanderHumanVisualAttachment>() != null
                    ? SalamanderHumanBindingPolicy.Names[index]
                    : body.GetComponentInParent<SerpentineVisualAttachment>().DriverNames[index],
                ["nativeTransformName"] = bone.name, ["worldPosition"] = SurveyVector(bone.position),
                ["skinToWorldRowMajor"] = Sprint17SurveyMatrix(skin[index]) }));
            return new JObject { ["frame"] = Time.frameCount, ["finite"] = finite, ["poseFinite"] = poseFinite,
                ["actorPosition"] = SurveyVector(unit.Position), ["vertices"] = points.Length,
                ["lowestAboveActor"] = points.Min(point => point.y) - unit.Position.y,
                ["height"] = points.Max(point => point.y) - points.Min(point => point.y),
                ["lowestVertexIndex"] = lowest, ["lowestWorldPosition"] = SurveyVector(points[lowest]),
                ["actorFloor"] = Sprint17MeasuredFloor(unit, unit.Position),
                ["lowestVertexFloor"] = Sprint17MeasuredFloor(unit, points[lowest]),
                // This static native overload only returns a projected point;
                // it does not assign a Transform or move the actor.
                ["nativeMovementProjection"] = SurveyVector(UnitMovementAgentBase.Move(unit.Position, Vector3.zero, 0)),
                ["viewLocalToWorldRowMajor"] = Sprint17SurveyMatrix(unit.View.transform.localToWorldMatrix),
                ["rendererLocalToWorldRowMajor"] = Sprint17SurveyMatrix(body.transform.localToWorldMatrix),
                ["skinTransforms"] = transforms,
                ["nativeTerrainSnaps"] = new JArray(unit.View.GetComponentsInChildren<Kingmaker.Visual.SnapToTerrain>(true)
                    .Select(snap => new JObject { ["name"] = snap.name,
                        ["parent"] = snap.transform.parent == null ? null : snap.transform.parent.name,
                        ["enabled"] = snap.enabled, ["active"] = snap.gameObject.activeInHierarchy,
                        ["boundsCenter"] = SurveyVector(snap.Bounds.center), ["boundsSize"] = SurveyVector(snap.Bounds.size),
                        ["upShift"] = snap.UpShift, ["noRotationSnap"] = snap.NoRotationSnap,
                        ["fixParentRotation"] = snap.FixParentRotation,
                        ["localPosition"] = SurveyVector(snap.transform.localPosition),
                        ["localToWorldRowMajor"] = Sprint17SurveyMatrix(snap.transform.localToWorldMatrix) })),
                ["headViewPosition"] = SurveyVector(unit.View.transform.InverseTransformPoint(
                    body.bones.Single(bone => bone.name == "Head").position)),
                ["worldPoseMethod"] = "sum(weight * bone.localToWorld * bindpose * originalVertex)" };
        }

        private static JArray Sprint17SurveyMatrix(Matrix4x4 matrix)
        { return new JArray(Enumerable.Range(0, 16).Select(index => matrix[index / 4, index % 4]).ToArray()); }

        private static JObject Sprint17BodyMaterialResearch(Material material)
        {
            var properties = new JObject();
            foreach (string name in new[] { "_Cull", "_CullMode", "_ZWrite", "_ZTest", "_SrcBlend", "_DstBlend",
                "_Cutoff", "_AlphaClip", "_Alpha", "_Opacity", "_Dissolve" })
                properties[name] = material.HasProperty(name) ? new JValue(material.GetFloat(name)) : JValue.CreateNull();
            return new JObject { ["shader"] = material.shader.name, ["renderQueue"] = material.renderQueue,
                ["keywords"] = new JArray(material.shaderKeywords), ["declaredFloatProperties"] = properties,
                ["scope"] = "read only; no visibility/culling override; no shader or texture export" };
        }

        private static JObject Sprint17NativeSpearResearch(UnitEntityData unit)
        {
            var item = unit.Body.PrimaryHand.MaybeWeapon;
            var weapon = item == null ? null : item.Blueprint;
            GameObject model = weapon == null || weapon.VisualParameters == null ? null : weapon.VisualParameters.Model;
            return new JObject { ["scope"] = "read-only existing primary weapon prefab/anchors; no equip, model clone or weapon handling claim",
                ["weapon"] = weapon == null ? null : weapon.AssetGuid,
                ["category"] = weapon == null ? null : weapon.Category.ToString(),
                ["model"] = model == null ? null : model.name,
                ["modelComponents"] = model == null ? JValue.CreateNull() : (JToken)new JArray(
                    model.GetComponentsInChildren<Component>(true).Where(value => value != null)
                        .Select(value => value.GetType().FullName).Distinct().OrderBy(value => value)),
                ["modelMeshes"] = model == null ? JValue.CreateNull() : (JToken)new JArray(
                    model.GetComponentsInChildren<MeshFilter>(true).Select(filter => new JObject {
                        ["name"] = filter.name, ["mesh"] = filter.sharedMesh == null ? null : filter.sharedMesh.name,
                        ["modelLocalFrame"] = Sprint17SurveyMatrix(model.transform.worldToLocalMatrix * filter.transform.localToWorldMatrix),
                        ["boundsCenter"] = filter.sharedMesh == null ? null : SurveyVector(filter.sharedMesh.bounds.center),
                        ["boundsSize"] = filter.sharedMesh == null ? null : SurveyVector(filter.sharedMesh.bounds.size) })),
                ["nativeWeaponSnaps"] = new JArray(unit.View.GetComponentsInChildren<Kingmaker.Assets.Visual.WeaponSnap>(true)
                    .Select(snap => new JObject { ["name"] = snap.name,
                        ["parent"] = snap.transform.parent == null ? null : snap.transform.parent.name,
                        ["target"] = snap.SnapTo == null ? null : snap.SnapTo.name,
                        ["viewLocalFrame"] = Sprint17SurveyMatrix(unit.View.transform.worldToLocalMatrix * snap.transform.localToWorldMatrix) })) };
        }

        private static JObject Sprint17MeasuredFloor(UnitEntityData unit, Vector3 point)
        {
            // Exact primary ray of native UnitMovementAgentBase.Move: two
            // metres up, one hundred down, mask 0x200101. Read-only; expose
            // misses/own-collider hits rather than substituting a nav height.
            RaycastHit hit;
            bool found = Physics.Raycast(point + Vector3.up * 2f, Vector3.down, out hit, 100f, 0x200101);
            bool owned = found && hit.collider != null && hit.collider.transform.IsChildOf(unit.View.transform);
            float? clearance = SerpentineRigSurveyPolicy.MeasuredGroundClearance(point.y, found,
                found ? hit.point.y : float.NaN, found ? hit.normal.y : float.NaN, owned);
            return new JObject { ["queryPoint"] = SurveyVector(point), ["rayHit"] = found,
                ["layerMask"] = "0x200101", ["startHeight"] = 2f, ["distance"] = 100f,
                ["queryTriggers"] = Physics.queriesHitTriggers,
                ["collider"] = found && hit.collider != null ? hit.collider.name : null,
                ["colliderLayer"] = found && hit.collider != null ? (int?)hit.collider.gameObject.layer : null,
                ["ownedCollider"] = owned, ["hitPoint"] = found ? SurveyVector(hit.point) : null,
                ["normal"] = found ? SurveyVector(hit.normal) : null,
                ["clearance"] = clearance.HasValue ? new JValue(clearance.Value) : JValue.CreateNull() };
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
            finally
            {
                if (_serpentineBodyPrototype != null) UnityEngine.Object.Destroy(_serpentineBodyPrototype);
                _serpentineBodyPrototype = null;
                foreach (UnityEngine.Object prototype in _serpentineContactPrototypes)
                    if (prototype != null) UnityEngine.Object.Destroy(prototype);
                _serpentineContactPrototypes.Clear();
                RestoreSprint17BodyEnvironment();
            }
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
