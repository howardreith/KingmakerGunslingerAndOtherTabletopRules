using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.UI;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.View;
using Kingmaker.Visual.Animation.Kingmaker;
using KingmakerGunslinger.Summoning;
using UnityEngine;
using UnityEngine.UI;

namespace KingmakerGunslinger.RuntimeTesting
{
    /// <summary>
    /// In-game images of the Pteranodon for internal review, taken from the
    /// party camera inside the proven persistence lifecycle.
    ///
    /// The synchronous scenarios cast, exercise and dispose a creature within
    /// one update, so anything rendered there shows the spawn frame - the
    /// summoning effect, a renderer the fader has not yet enabled - and never
    /// the creature a player sees. A standalone capture scenario was tried
    /// earlier and withdrawn; the persistence stages already hold the real
    /// summon across frames, freshly cast in prepare and freshly deserialized
    /// in verify-cleanup, which is exactly the view worth reviewing.
    ///
    /// The review is bounded: it waits for the screen and the creature to
    /// have faded in (the first images were a black screen and a half-
    /// dissolved silhouette, both fades still running after the load), scrolls
    /// the party camera to the creature, renders the game camera itself to a
    /// file (what the player sees, minus the mod manager's overlay), commands
    /// one short move and one attack animation, renders again at fixed
    /// frames, then interrupts everything it started. It takes about thirty
    /// updates past the fades and changes nothing the stage goes on to
    /// measure.
    /// </summary>
    internal sealed partial class RuntimeTestRunner
    {
        private const int MotionReviewCaptureWidth = 1280;
        private const int MotionReviewCaptureHeight = 720;
        private const int MotionReviewMoveFrames = 12;
        private const int MotionReviewAttackFrame = 30;
        /// <summary>
        /// Updates the review will wait for the load and screen fades and for
        /// the creature's own dissolve-in to reach intact; the first captures
        /// after the material joined the game's fades showed it at 1.0 to
        /// 0.83, a flat silhouette in the dissolve colour.
        /// </summary>
        private const int MotionReviewFadeBudget = 600;
        private const float MotionReviewIntactDissolve = 0.02f;

        private string _motionReviewSubjectName =
            ExpandedSummoningPteranodonViewPatch.PteranodonBlueprintName;
        private string _motionReviewFilePrefix = "pteranodon-review";
        private int _motionReviewFrame = -1;
        private int _motionReviewWaited;
        private bool _motionReviewComplete;
        private bool _motionReviewSubjectResolved;
        private UnitEntityData _motionReviewSubject;
        private UnitAnimationActionHandle _motionReviewAttack;
        private Vector3 _motionReviewMoveOrigin;
        private Vector3 _motionReviewMoveDestination;
        private bool _motionReviewMoveAccepted;
        private bool _motionReviewMoveCanStart;
        private bool _motionReviewAppearanceCleared;
        private UnitMoveTo _motionReviewMoveCommand;
        private bool _motionReviewMoveStarted;
        private bool _motionReviewMoveRunning;
        private bool _motionReviewMoveFinished;
        private bool _motionReviewUnitInGame;
        private bool _motionReviewViewInGame;
        private UnitEntityData[] _motionReviewAwakeBefore;
        private bool _motionReviewAwakeAdded;
        private bool _motionReviewAwakeRestored;
        private float _motionReviewMaxPlanarTravel;
        private float _motionReviewMaxDestinationApproach;
        private float _motionReviewMinDestinationGap;
        private float _motionReviewMaxViewPlanarTravel;
        private float _motionReviewMaxVelocity;
        private float _motionReviewMaxDeltaTime;
        private bool _motionReviewAgentWantsMove;
        private bool _motionReviewWasPaused;
        private bool _motionReviewChangedPause;
        private bool _motionReviewTravelValid;
        private bool _motionReviewOverlayWasOpen;
        private readonly List<string> _motionReviewCaptures = new List<string>();
        private readonly List<float> _motionReviewFrameSeconds = new List<float>();
        private string _motionReviewSummary = "<not run>";
        private bool _motionReviewValid;

        internal string MotionReviewSummary { get { return _motionReviewSummary; } }
        internal bool MotionReviewValid { get { return _motionReviewValid; } }
        internal bool MotionReviewTravelValid { get { return _motionReviewTravelValid; } }

        /// <summary>
        /// Points the review at another creature and forgets the previous one,
        /// so one request can review several summons in turn. The persistence
        /// stages never call this and keep reviewing the Pteranodon.
        /// </summary>
        private void ResetExpandedSummoningMotionReview(string subjectBlueprintName,
            string filePrefix)
        {
            if (string.IsNullOrWhiteSpace(subjectBlueprintName))
                throw new ArgumentException("subjectBlueprintName");
            if (string.IsNullOrWhiteSpace(filePrefix))
                throw new ArgumentException("filePrefix");
            _motionReviewSubjectName = subjectBlueprintName;
            _motionReviewFilePrefix = filePrefix;
            _motionReviewFrame = -1;
            _motionReviewWaited = 0;
            _motionReviewComplete = false;
            _motionReviewSubjectResolved = false;
            _motionReviewSubject = null;
            _motionReviewAttack = null;
            _motionReviewMoveOrigin = Vector3.zero;
            _motionReviewMoveDestination = Vector3.zero;
            _motionReviewMoveAccepted = false;
            _motionReviewMoveCanStart = false;
            _motionReviewAppearanceCleared = false;
            _motionReviewMoveCommand = null;
            _motionReviewMoveStarted = false;
            _motionReviewMoveRunning = false;
            _motionReviewMoveFinished = false;
            _motionReviewUnitInGame = false;
            _motionReviewViewInGame = false;
            _motionReviewAwakeBefore = null;
            _motionReviewAwakeAdded = false;
            _motionReviewAwakeRestored = false;
            _motionReviewMaxPlanarTravel = 0f;
            _motionReviewMaxDestinationApproach = 0f;
            _motionReviewMinDestinationGap = float.MaxValue;
            _motionReviewMaxViewPlanarTravel = 0f;
            _motionReviewMaxVelocity = 0f;
            _motionReviewMaxDeltaTime = 0f;
            _motionReviewAgentWantsMove = false;
            _motionReviewWasPaused = false;
            _motionReviewChangedPause = false;
            _motionReviewTravelValid = false;
            _motionReviewOverlayWasOpen = false;
            _motionReviewCaptures.Clear();
            _motionReviewFrameSeconds.Clear();
            _motionReviewSummary = "<not run>";
            _motionReviewValid = false;
        }

        /// <summary>
        /// One step per update. True once the review has finished (or found no
        /// subject); false while a frame is still owed. Never throws: a review
        /// that cannot render records why and lets the stage continue, since
        /// the stage's own assertions are the mechanical proof.
        /// </summary>
        private bool StepExpandedSummoningMotionReview(UnitEntityData[] units,
            string stage)
        {
            if (_motionReviewComplete) return true;
            try
            {
                if (!_motionReviewSubjectResolved)
                {
                    _motionReviewSubjectResolved = true;
                    _motionReviewSubject = (units ?? Array.Empty<UnitEntityData>())
                        .FirstOrDefault(value => value != null &&
                            value.Blueprint != null && value.Blueprint.name ==
                            _motionReviewSubjectName);
                    if (_motionReviewSubject == null || _motionReviewSubject.View == null)
                    {
                        _motionReviewSummary = "stage=" + stage + ";no-subject";
                        _motionReviewComplete = true;
                        return true;
                    }
                }
                UnitEntityData unit = _motionReviewSubject;

                if (_motionReviewFrame < 0)
                {
                    // After a load the screen fades up from black and every
                    // unit dissolves in; a fresh summon dissolves in too. The
                    // camera is parked on the creature meanwhile so the first
                    // render is framed.
                    CameraRig rig = TeleportationCastingCamera();
                    if (rig != null) rig.ScrollToImmediately(unit.Position);
                    if (_motionReviewWaited < MotionReviewFadeBudget &&
                        (LoadingOrScreenFadeActive() || !EntityFadedIn(unit) ||
                            DissolveAmount(unit) > MotionReviewIntactDissolve ||
                            IsSprint9FlightReview(unit) &&
                            unit.Descriptor.Buffs.GetBuff(BlueprintRoot.Instance
                                .SystemMechanics.SummonedUnitAppearBuff) != null))
                    {
                        _motionReviewWaited++;
                        return false;
                    }
                    _motionReviewOverlayWasOpen = SetModManagerOverlay(false);
                    _motionReviewFrame = 0;
                    return false;
                }

                _motionReviewFrameSeconds.Add(Time.unscaledDeltaTime);
                if (_motionReviewFrame == 0)
                {
                    Capture(unit, stage, "idle");
                    bool flight = IsSprint9FlightReview(unit);
                    Vector3 destination = unit.Position +
                        MotionReviewAcross(unit) * 6f;
                    if (flight)
                    {
                        _motionReviewAwakeBefore = Game.Instance.State
                            .AwakeUnits.ToArray();
                        if (!Game.Instance.State.AwakeUnits.Contains(unit))
                        {
                            Game.Instance.State.AwakeUnits.Add(unit);
                            _motionReviewAwakeAdded = true;
                        }
                        _motionReviewWasPaused = Game.Instance.IsPaused;
                        if (_motionReviewWasPaused)
                        {
                            Game.Instance.IsPaused = false;
                            _motionReviewChangedPause = true;
                        }
                        destination = PrepareSprint9FlightMovement(unit);
                    }
                    _motionReviewMoveOrigin = unit.Position;
                    _motionReviewMoveDestination = destination;
                    var move = new UnitMoveTo(destination, 0.5f);
                    move.Init(unit);
                    _motionReviewMoveCanStart = move.CanStart;
                    _motionReviewAppearanceCleared = unit.Descriptor.Buffs
                        .GetBuff(BlueprintRoot.Instance.SystemMechanics
                            .SummonedUnitAppearBuff) == null;
                    unit.Commands.Run(move);
                    _motionReviewMoveCommand = move;
                    _motionReviewMoveAccepted = unit.Commands.Contains(move) &&
                        ReferenceEquals(move.Executor, unit);
                    _motionReviewMoveStarted |= move.IsStarted;
                    _motionReviewMoveRunning |= move.IsRunning;
                    _motionReviewMoveFinished |= move.IsFinished;
                    if (flight && unit.View.MovementAgent != null)
                        _motionReviewAgentWantsMove =
                            unit.View.MovementAgent.WantsToMove;
                    _motionReviewFrame++;
                    return false;
                }
                // Animation callbacks alone do not establish travel. Sample the
                // native movement agent and the unit's ground-plane position
                // while the real move command is active.
                Vector3 position = unit.Position;
                _motionReviewMoveStarted |= _motionReviewMoveCommand != null &&
                    _motionReviewMoveCommand.IsStarted;
                _motionReviewMoveRunning |= _motionReviewMoveCommand != null &&
                    _motionReviewMoveCommand.IsRunning;
                _motionReviewMoveFinished |= _motionReviewMoveCommand != null &&
                    _motionReviewMoveCommand.IsFinished;
                _motionReviewUnitInGame |= unit.IsInGame;
                _motionReviewViewInGame |= unit.View != null &&
                    unit.View.IsInGame;
                if (IsSprint9FlightReview(unit) && unit.View != null &&
                    unit.View.MovementAgent != null)
                {
                    float delta = Game.Instance.TimeController.DeltaTime;
                    _motionReviewMaxDeltaTime = Mathf.Max(
                        _motionReviewMaxDeltaTime, delta);
                    unit.View.MovementAgent.TickMovement(delta);
                    _motionReviewAgentWantsMove |=
                        unit.View.MovementAgent.WantsToMove;
                    position = unit.Position;
                }
                _motionReviewMaxPlanarTravel = Mathf.Max(
                    _motionReviewMaxPlanarTravel,
                    Vector2.Distance(new Vector2(_motionReviewMoveOrigin.x,
                        _motionReviewMoveOrigin.z), new Vector2(position.x,
                        position.z)));
                float startGap = Vector2.Distance(new Vector2(
                    _motionReviewMoveOrigin.x, _motionReviewMoveOrigin.z),
                    new Vector2(_motionReviewMoveDestination.x,
                        _motionReviewMoveDestination.z));
                float currentGap = Vector2.Distance(new Vector2(position.x,
                    position.z), new Vector2(_motionReviewMoveDestination.x,
                    _motionReviewMoveDestination.z));
                _motionReviewMinDestinationGap = Mathf.Min(
                    _motionReviewMinDestinationGap, currentGap);
                _motionReviewMaxDestinationApproach = Mathf.Max(
                    _motionReviewMaxDestinationApproach, startGap - currentGap);
                if (unit.View != null && unit.View.MovementAgent != null)
                {
                    _motionReviewMaxVelocity = Mathf.Max(_motionReviewMaxVelocity,
                        unit.View.MovementAgent.Velocity.magnitude);
                    Vector3 viewPosition = unit.View.transform.position;
                    _motionReviewMaxViewPlanarTravel = Mathf.Max(
                        _motionReviewMaxViewPlanarTravel,
                        Vector2.Distance(new Vector2(_motionReviewMoveOrigin.x,
                            _motionReviewMoveOrigin.z), new Vector2(
                            viewPosition.x, viewPosition.z)));
                }
                if (_motionReviewFrame == MotionReviewMoveFrames)
                {
                    Capture(unit, stage, "moving-a");
                }
                else if (_motionReviewFrame == MotionReviewMoveFrames * 2)
                {
                    Capture(unit, stage, "moving-b");
                    unit.Commands.InterruptMove();
                    UnitAnimationManager manager = unit.View == null ? null :
                        unit.View.AnimationManager;
                    _motionReviewAttack = manager == null ? null :
                        manager.CreateHandle(UnitAnimationType.MainHandAttack, false);
                    if (_motionReviewAttack != null) manager.Execute(_motionReviewAttack);
                }
                else if (_motionReviewFrame >= MotionReviewAttackFrame)
                {
                    Capture(unit, stage, "attack");
                    if (_motionReviewAttack != null)
                    {
                        _motionReviewAttack.IsActed = true;
                        FinishExpandedSummoningAnimation(_motionReviewAttack);
                        _motionReviewAttack = null;
                    }
                    unit.Commands.InterruptMove();
                    Finish(stage, null);
                    return true;
                }
                _motionReviewFrame++;
                return false;
            }
            catch (Exception error)
            {
                Finish(stage, error);
                return true;
            }
        }

        private void Finish(string stage, Exception error)
        {
            if (_motionReviewAwakeBefore != null)
            {
                if (_motionReviewAwakeAdded)
                    Game.Instance.State.AwakeUnits.Remove(_motionReviewSubject);
                _motionReviewAwakeRestored = Game.Instance.State.AwakeUnits
                    .SequenceEqual(_motionReviewAwakeBefore);
                _motionReviewAwakeBefore = null;
            }
            if (_motionReviewChangedPause)
            {
                Game.Instance.IsPaused = _motionReviewWasPaused;
                _motionReviewChangedPause = false;
            }
            if (_motionReviewOverlayWasOpen) SetModManagerOverlay(true);
            double frameMs = _motionReviewFrameSeconds.Count == 0 ? 0d :
                _motionReviewFrameSeconds.Average() * 1000d;
            bool inFrame = _motionReviewCaptures.Count == 4 &&
                _motionReviewCaptures.All(value =>
                    value.IndexOf(";inFrame=true", StringComparison.Ordinal) >= 0 &&
                    value.IndexOf(";rendererEnabled=true", StringComparison.Ordinal) >= 0 &&
                    value.IndexOf(";screenLit=true", StringComparison.Ordinal) >= 0 &&
                    value.IndexOf(";intact=true", StringComparison.Ordinal) >= 0);
            _motionReviewValid = error == null && inFrame;
            _motionReviewTravelValid = error == null &&
                _motionReviewMoveAccepted &&
                _motionReviewMoveCanStart &&
                _motionReviewAppearanceCleared &&
                _motionReviewAwakeRestored &&
                _motionReviewAgentWantsMove &&
                _motionReviewMaxDeltaTime > 0f &&
                _motionReviewMaxPlanarTravel >= 0.75f &&
                _motionReviewMaxDestinationApproach >= 0.75f &&
                _motionReviewMinDestinationGap <= 2f &&
                _motionReviewMaxVelocity > 0.01f;
            _motionReviewSummary = "stage=" + stage + ";waited=" + _motionReviewWaited +
                ";frames=" + _motionReviewFrame + ";frameMs=" + frameMs.ToString("0.#",
                    CultureInfo.InvariantCulture) + ";moveAccepted=" +
                _motionReviewMoveAccepted + ";moveCanStart=" +
                _motionReviewMoveCanStart + ";appearanceCleared=" +
                _motionReviewAppearanceCleared + ";moveOrigin=" +
                _motionReviewMoveOrigin.ToString("F2") + ";moveDestination=" +
                _motionReviewMoveDestination.ToString("F2") +
                ";moveFinal=" + (_motionReviewSubject == null ? "<null>" :
                    _motionReviewSubject.Position.ToString("F2")) +
                ";agentWantsMove=" +
                _motionReviewAgentWantsMove + ";maxDeltaTime=" +
                _motionReviewMaxDeltaTime.ToString("0.###",
                    CultureInfo.InvariantCulture) + ";maxPlanarTravel=" +
                _motionReviewMaxPlanarTravel.ToString("0.###",
                    CultureInfo.InvariantCulture) + ";maxDestinationApproach=" +
                _motionReviewMaxDestinationApproach.ToString("0.###",
                    CultureInfo.InvariantCulture) + ";minDestinationGap=" +
                _motionReviewMinDestinationGap.ToString("0.###",
                    CultureInfo.InvariantCulture) + ";maxViewPlanarTravel=" +
                _motionReviewMaxViewPlanarTravel.ToString("0.###",
                    CultureInfo.InvariantCulture) + ";maxVelocity=" +
                _motionReviewMaxVelocity.ToString("0.###",
                    CultureInfo.InvariantCulture) + ";travelValid=" +
                _motionReviewTravelValid + ";moveStarted=" +
                _motionReviewMoveStarted + ";moveRunning=" +
                _motionReviewMoveRunning + ";moveFinished=" +
                _motionReviewMoveFinished + ";unitInGame=" +
                _motionReviewUnitInGame + ";viewInGame=" +
                _motionReviewViewInGame + ";pausedBefore=" +
                _motionReviewWasPaused + ";awakeAdded=" +
                _motionReviewAwakeAdded + ";awakeRestored=" +
                _motionReviewAwakeRestored + ";pausedAfter=" +
                Game.Instance.IsPaused + ";captures=" +
                _motionReviewCaptures.Count + (error == null ? "" :
                    ";fault=" + error.GetType().Name + ":" + error.Message) +
                ";" + string.Join("|", _motionReviewCaptures.ToArray());
            _motionReviewComplete = true;
        }

        private static bool IsSprint9FlightReview(UnitEntityData unit)
        {
            string name = unit == null || unit.Blueprint == null ? null :
                unit.Blueprint.name;
            return name == ExpandedSummoningPteranodonViewPatch.EagleBlueprintName ||
                name == ExpandedSummoningPteranodonViewPatch.DireBatBlueprintName;
        }

        private Vector3 PrepareSprint9FlightMovement(UnitEntityData unit)
        {
            if (AstarPath.active == null || unit.View == null ||
                unit.View.AgentASP == null || unit.View.MovementAgent == null ||
                _creatureReviewCaster == null ||
                !ReferenceEquals(unit.HoldingState,
                    _creatureReviewCaster.HoldingState))
                throw new InvalidOperationException(
                    "Sprint 9 flight review lacks a live party-area navigation anchor.");
            // The summon may initially appear at an edge of the room. Use the
            // party member's visible floor node as a search anchor, then place
            // the summon on a clear nearby node away from all party bodies.
            Pathfinding.NNInfo anchor = AstarPath.active.GetNearest(
                _creatureReviewCaster.Position);
            if (anchor.node == null || !anchor.node.Walkable)
                throw new InvalidOperationException(
                    "Sprint 9 flight review has no walkable party anchor.");
            var graph = AstarPath.active.graphs[
                (int)anchor.node.GraphIndex] as Pathfinding.IRaycastableGraph;
            if (graph == null)
                throw new InvalidOperationException(
                    "Sprint 9 flight review graph cannot verify a clear route.");
            UnitEntityData[] party = Game.Instance.Player.Party.Where(value =>
                value != null && value.IsInGame).ToArray();
            Vector3 across = MotionReviewAcross(unit);
            Vector3 forward = Vector3.Cross(Vector3.up, across);
            Vector3[] directions = { across, -across, forward, -forward,
                (across + forward).normalized,
                (across - forward).normalized,
                (-across + forward).normalized,
                (-across - forward).normalized };
            Pathfinding.NNInfo[] open = new[] { 3f, 4f, 5f }
                .SelectMany(radius => directions.Select(direction =>
                    anchor.clampedPosition + direction * radius))
                .Select(requested => new { requested,
                    nearest = AstarPath.active.GetNearest(requested) })
                .Where(value => value.nearest.node != null &&
                    value.nearest.node.Walkable &&
                    value.nearest.node.Area == anchor.node.Area &&
                    value.nearest.node.GraphIndex == anchor.node.GraphIndex &&
                    Vector3.Distance(value.requested,
                        value.nearest.clampedPosition) <= 0.5f &&
                    party.All(member => Vector3.Distance(member.Position,
                        value.nearest.clampedPosition) >= 2.5f) &&
                    Sprint9FlightLineClear(graph, anchor, value.nearest))
                .Select(value => value.nearest).ToArray();
            if (open.Length < 2)
                throw new InvalidOperationException(
                    "Sprint 9 flight review has fewer than two clear party-area nodes.");
            Pathfinding.NNInfo start = open.OrderBy(value =>
                Vector3.Distance(value.clampedPosition,
                    anchor.clampedPosition)).First();
            Pathfinding.NNInfo[] destinations = open.Where(value =>
                    Vector3.Distance(value.clampedPosition,
                        start.clampedPosition) >= 2f &&
                    Sprint9FlightLineClear(graph, start, value))
                .OrderByDescending(value => Vector3.Distance(
                    value.clampedPosition, start.clampedPosition)).ToArray();
            if (destinations.Length == 0)
                throw new InvalidOperationException(
                    "Sprint 9 flight review has no clear party-free movement span.");
            unit.Position = start.clampedPosition;
            unit.View.transform.position = start.clampedPosition;
            return destinations[0].clampedPosition;
        }

        private static bool Sprint9FlightLineClear(
            Pathfinding.IRaycastableGraph graph, Pathfinding.NNInfo from,
            Pathfinding.NNInfo to)
        {
            Pathfinding.GraphHitInfo hit;
            var trace = new List<Pathfinding.GraphNode>();
            return !graph.Linecast(from.clampedPosition, to.clampedPosition,
                from.node, out hit, trace);
        }

        /// <summary>
        /// The loading process, its screen, or the full-screen fade image the
        /// game brings up from black after a load.
        /// </summary>
        private static bool LoadingOrScreenFadeActive()
        {
            LoadingProcess loading = LoadingProcess.Instance;
            if (loading != null && (loading.IsLoadingInProcess ||
                loading.IsLoadingScreenActive || loading.IsManualLoadingScreenActive))
                return true;
            return ScreenFadeAlpha() > 0.05f;
        }

        private static float ScreenFadeAlpha()
        {
            try
            {
                FadeCanvas canvas = FadeCanvas.Instance;
                if (canvas == null) return 0f;
                FieldInfo field = typeof(FadeCanvas).GetField("m_FadeImage",
                    BindingFlags.Instance | BindingFlags.NonPublic |
                    BindingFlags.Public);
                Image image = field == null ? null : field.GetValue(canvas) as Image;
                if (image == null || !image.gameObject.activeInHierarchy ||
                    !image.enabled) return 0f;
                return image.color.a;
            }
            catch (Exception)
            {
                return 0f;
            }
        }

        /// <summary>The renderer material's dissolve amount; 0 when the property is absent.</summary>
        private static float DissolveAmount(UnitEntityData unit)
        {
            SkinnedMeshRenderer renderer = unit == null || unit.View == null ? null :
                unit.View.GetComponentsInChildren<SkinnedMeshRenderer>(true)
                    .FirstOrDefault(value => value != null && value.sharedMesh != null);
            Material material = renderer == null ? null : renderer.sharedMaterial;
            return material != null && material.HasProperty("_Dissolve")
                ? material.GetFloat("_Dissolve") : 0f;
        }

        private static bool EntityFadedIn(UnitEntityData unit)
        {
            EntityFader fader = unit == null || unit.View == null ? null :
                unit.View.GetComponent<EntityFader>();
            return fader == null || fader.Visible;
        }

        /// <summary>
        /// A direction across the camera's view, on the ground plane, so the
        /// move reads as a walk across the frame rather than into the camera.
        /// </summary>
        private static Vector3 MotionReviewAcross(UnitEntityData unit)
        {
            CameraRig rig = TeleportationCastingCamera();
            Camera camera = rig == null ? null : rig.Camera;
            Vector3 right = camera == null ? Vector3.right : camera.transform.right;
            right.y = 0f;
            return right.sqrMagnitude < 0.0001f ? Vector3.right : right.normalized;
        }

        private void Capture(UnitEntityData unit, string stage, string moment)
        {
            string fileName = _motionReviewFilePrefix + "-" + stage + "-" + moment + ".png";
            _motionReviewCaptures.Add(WriteExpandedSummoningPartyCameraCapture(
                unit, _request.EvidenceDirectory, fileName) + ";moment=" + moment);
        }

        /// <summary>
        /// Renders the game's own camera, scrolled to the unit, into a file:
        /// the party-camera frame the player would see, with the game's
        /// lighting and image effects and without the mod manager's IMGUI
        /// overlay, which a screen capture would include. The record says
        /// whether the screen fade was still up and whether the creature's
        /// renderer was enabled, so a black or half-dissolved frame cannot
        /// pass as a review image.
        /// </summary>
        private static string WriteExpandedSummoningPartyCameraCapture(
            UnitEntityData unit, string evidenceDirectory, string fileName)
        {
            if (unit == null || unit.View == null ||
                string.IsNullOrWhiteSpace(evidenceDirectory))
                return "png=<none>;reason=no-view;inFrame=false";
            CameraRig rig = TeleportationCastingCamera();
            Camera camera = rig == null ? null : rig.Camera;
            if (camera == null) camera = Camera.main;
            if (camera == null) return "png=<none>;reason=no-camera;inFrame=false";
            if (rig != null) rig.ScrollToImmediately(unit.Position);

            RenderTexture renderTexture = null;
            Texture2D output = null;
            RenderTexture priorActive = RenderTexture.active;
            RenderTexture priorTarget = camera.targetTexture;
            try
            {
                renderTexture = new RenderTexture(MotionReviewCaptureWidth,
                    MotionReviewCaptureHeight, 24, RenderTextureFormat.ARGB32);
                camera.targetTexture = renderTexture;
                camera.Render();
                camera.targetTexture = priorTarget;
                Vector3 viewport = camera.WorldToViewportPoint(unit.Position);
                RenderTexture.active = renderTexture;
                output = new Texture2D(MotionReviewCaptureWidth,
                    MotionReviewCaptureHeight, TextureFormat.RGBA32, false, false);
                output.ReadPixels(new Rect(0, 0, MotionReviewCaptureWidth,
                    MotionReviewCaptureHeight), 0, 0);
                output.Apply(false, false);
                byte[] png = EncodeExpandedSummoningPng(output);
                if (png == null || png.Length < 4096)
                    return "png=<none>;reason=empty-render;inFrame=false";
                File.WriteAllBytes(Path.Combine(evidenceDirectory, fileName), png);
                bool inFrame = viewport.z > 0f && viewport.x > 0.02f &&
                    viewport.x < 0.98f && viewport.y > 0.02f && viewport.y < 0.98f;
                SkinnedMeshRenderer renderer = unit.View
                    .GetComponentsInChildren<SkinnedMeshRenderer>(true)
                    .FirstOrDefault(value => value != null && value.sharedMesh != null);
                // Mean luminance of a coarse sample of the frame: a screen
                // still faded to black reads near zero.
                Color32[] pixels = output.GetPixels32();
                double luminance = 0d;
                int sampled = 0;
                for (int index = 0; index < pixels.Length; index += 997)
                {
                    Color32 pixel = pixels[index];
                    luminance += 0.2126 * pixel.r + 0.7152 * pixel.g + 0.0722 * pixel.b;
                    sampled++;
                }
                luminance = sampled == 0 ? 0d : luminance / sampled;
                return "png=" + fileName + ";bytes=" + png.Length.ToString(
                        CultureInfo.InvariantCulture) + ";viewport=" +
                    viewport.x.ToString("0.###", CultureInfo.InvariantCulture) + "," +
                    viewport.y.ToString("0.###", CultureInfo.InvariantCulture) +
                    ";inFrame=" + (inFrame ? "true" : "false") +
                    ";screenFade=" + ScreenFadeAlpha().ToString("0.##",
                        CultureInfo.InvariantCulture) +
                    ";luma=" + luminance.ToString("0.#", CultureInfo.InvariantCulture) +
                    ";screenLit=" + (luminance > 4d ? "true" : "false") +
                    ";rendererEnabled=" + (renderer != null && renderer.enabled ?
                        "true" : "false") + ";faderVisible=" + (EntityFadedIn(unit) ?
                        "true" : "false") + ";mesh=" + (renderer == null ||
                        renderer.sharedMesh == null ? "<none>" :
                        renderer.sharedMesh.name) + ";dissolve=" +
                    DescribeDissolve(renderer) + ";intact=" +
                    (DissolveAmount(unit) <= MotionReviewIntactDissolve ? "true" : "false") +
                    ";material=" + DescribeMaterialSlots(renderer) +
                    ";visual=" +
                    ExpandedSummoningPteranodonViewPatch.DescribeView(unit.View)
                        .Split(';')[0];
            }
            catch (Exception error)
            {
                camera.targetTexture = priorTarget;
                return "png=<none>;reason=" + error.GetType().Name + ";inFrame=false";
            }
            finally
            {
                RenderTexture.active = priorActive;
                if (renderTexture != null)
                {
                    renderTexture.Release();
                    UnityEngine.Object.Destroy(renderTexture);
                }
                if (output != null) UnityEngine.Object.Destroy(output);
            }
        }

        /// <summary>
        /// Shader name and the colour/tint slots the renderer's material
        /// declares, so a creature-specific tint can be designed against the
        /// slots that exist rather than guessed. Unity 2018 cannot enumerate
        /// a shader's properties, so this is a probe list.
        /// </summary>
        private static string DescribeMaterialSlots(SkinnedMeshRenderer renderer)
        {
            if (renderer == null || renderer.sharedMaterial == null) return "<none>";
            Material material = renderer.sharedMaterial;
            var slots = new List<string>();
            foreach (string slot in new[] { "_Color", "_MainColor", "_TintColor",
                "_Tint", "_BaseColor", "_EmissionColor", "_Emissive",
                "_ColorMask", "_TintMask", "_MainTex", "_DissolveColor",
                "_Dissolve", "_Metallic", "_Glossiness", "_Smoothness" })
                if (material.HasProperty(slot)) slots.Add(slot);
            return (material.shader == null ? "<no-shader>" : material.shader.name)
                .Replace(';', ',').Replace('|', '/') + "[" +
                string.Join(",", slots.ToArray()) + "]";
        }

        /// <summary>
        /// The renderer's material dissolve amount (1 is fully dissolved, i.e.
        /// invisible) and whether the view's material controller lists that
        /// material - the two facts that decide whether the game's fades
        /// reach the visual.
        /// </summary>
        private static string DescribeDissolve(SkinnedMeshRenderer renderer)
        {
            if (renderer == null || renderer.sharedMaterial == null) return "<none>";
            Material material = renderer.sharedMaterial;
            string amount = material.HasProperty("_Dissolve")
                ? material.GetFloat("_Dissolve").ToString("0.###",
                    CultureInfo.InvariantCulture)
                : "<no-property>";
            var controller = renderer.GetComponentInParent<
                Kingmaker.Visual.MaterialEffects.StandardMaterialController>();
            IList<Material> materials =
                ExpandedSummoningPteranodonViewPatch.ControllerMaterials(controller);
            bool listed = materials != null && materials.Contains(material);
            return amount + "/listed=" + (listed ? "true" : "false");
        }

        /// <summary>
        /// Closes or reopens the Unity Mod Manager window. It is drawn over
        /// the game by IMGUI and was covering the whole frame in the first
        /// projected-menu screenshots; nothing the scenarios measure depends
        /// on it. Returns whether it was open before the call.
        /// </summary>
        internal static bool SetModManagerOverlay(bool open)
        {
            try
            {
                UnityModManagerNet.UnityModManager.UI overlay =
                    UnityModManagerNet.UnityModManager.UI.Instance;
                if (overlay == null) return false;
                bool wasOpen = overlay.Opened;
                if (wasOpen != open) overlay.ToggleWindow(open);
                return wasOpen;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
