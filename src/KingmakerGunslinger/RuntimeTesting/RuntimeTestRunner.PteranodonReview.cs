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
        private int _motionReviewCaptureFadeWaited;
        private int _motionReviewCaptureFadeTotal;
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
        private string _motionReviewNearbyDoors = "<not scanned>";
        private string _motionReviewFloorGrid = "<not scanned>";
        private bool _motionReviewDoorwayRoute;
        private bool _motionReviewDoorwayCrossed;
        private bool _motionReviewDoorwayValid;
        private float _motionReviewDoorwayCrossingZ;
        private float _motionReviewDoorwayCrossingX;
        private bool _motionReviewDoorwayDirectClear;
        private readonly List<string> _motionReviewCaptures = new List<string>();
        private readonly List<float> _motionReviewFrameSeconds = new List<float>();
        private string _motionReviewSummary = "<not run>";
        private bool _motionReviewValid;

        internal string MotionReviewSummary { get { return _motionReviewSummary; } }
        internal bool MotionReviewValid { get { return _motionReviewValid; } }
        internal bool MotionReviewTravelValid { get { return _motionReviewTravelValid; } }
        internal bool MotionReviewDoorwayValid { get { return _motionReviewDoorwayValid; } }

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
            _motionReviewCaptureFadeWaited = 0;
            _motionReviewCaptureFadeTotal = 0;
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
            _motionReviewNearbyDoors = "<not scanned>";
            _motionReviewFloorGrid = "<not scanned>";
            _motionReviewDoorwayRoute = false;
            _motionReviewDoorwayCrossed = false;
            _motionReviewDoorwayValid = false;
            _motionReviewDoorwayCrossingZ = float.NaN;
            _motionReviewDoorwayCrossingX = float.NaN;
            _motionReviewDoorwayDirectClear = false;
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
                    // A quantity member can leave the awake list when the
                    // preceding group move ends. Keep this reviewed member
                    // awake before waiting for its native fader to become
                    // visible; the snapshot is restored in Finish.
                    if (_creatureReviewQuantity && IsGuidedMotionReview(unit))
                        BeginGuidedMotionReview(unit);
                    // After a load the screen fades up from black and every
                    // unit dissolves in; a fresh summon dissolves in too. The
                    // camera is parked on the creature meanwhile so the first
                    // render is framed.
                    CameraRig rig = TeleportationCastingCamera();
                    if (rig != null) rig.ScrollToImmediately(unit.Position);
                    if (_motionReviewWaited < MotionReviewFadeBudget &&
                        (LoadingOrScreenFadeActive() || !EntityFadedIn(unit) ||
                            DissolveAmount(unit) > MotionReviewIntactDissolve ||
                            IsGuidedMotionReview(unit) &&
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
                    bool guided = IsGuidedMotionReview(unit);
                    Vector3 destination = unit.Position +
                        MotionReviewAcross(unit) * 6f;
                    if (guided)
                    {
                        BeginGuidedMotionReview(unit);
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
                    if (guided && unit.View.MovementAgent != null)
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
                if (IsGuidedMotionReview(unit) && unit.View != null &&
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
                if (_motionReviewDoorwayRoute &&
                    position.x > _motionReviewDoorwayCrossingX &&
                    position.z < _motionReviewDoorwayCrossingZ)
                    _motionReviewDoorwayCrossed = true;
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
                int moveFrames = _motionReviewDoorwayRoute ? 100 :
                    MotionReviewMoveFrames;
                bool crocodilian = _motionReviewSubjectName == "KMG_Summoning_Unit_Crocodile" ||
                    _motionReviewSubjectName == "KMG_Summoning_Unit_DireCrocodile";
                bool snake = _motionReviewSubjectName == "KMG_Summoning_Unit_Viper" ||
                    _motionReviewSubjectName == "KMG_Summoning_Unit_ConstrictorSnake";
                bool salamander = _motionReviewSubjectName == SalamanderRulesPolicy.UnitName;
                bool captureFrame = _motionReviewFrame == moveFrames || _motionReviewFrame == moveFrames * 2 ||
                    _motionReviewFrame >= (_motionReviewDoorwayRoute ? moveFrames * 2 + 6 : MotionReviewAttackFrame);
                // A surveyed doorway route can cross a native fog fade between
                // captures. Wait for that native transition, without overriding
                // visibility/materials or relaxing the intact-frame assertion.
                if ((crocodilian || snake || salamander) && captureFrame && _motionReviewCaptureFadeWaited < MotionReviewFadeBudget &&
                    (!EntityFadedIn(unit) || DissolveAmount(unit) > MotionReviewIntactDissolve))
                {
                    _motionReviewCaptureFadeWaited++;
                    _motionReviewCaptureFadeTotal++;
                    return false;
                }
                if (_motionReviewFrame == moveFrames)
                {
                    Capture(unit, stage, "moving-a");
                }
                else if (_motionReviewFrame == moveFrames * 2)
                {
                    Capture(unit, stage, "moving-b");
                    unit.Commands.InterruptMove();
                    // The new snake crowd rows observe movement/settlement.
                    // Their actual Bite proof belongs to the command matrix;
                    // do not create this historical MainHand presentation probe.
                    if (!snake && !salamander)
                    {
                        UnitAnimationManager manager = unit.View == null ? null :
                            unit.View.AnimationManager;
                        _motionReviewAttack = manager == null ? null :
                            manager.CreateHandle(UnitAnimationType.MainHandAttack, false);
                        if (_motionReviewAttack != null) manager.Execute(_motionReviewAttack);
                    }
                }
                else if (_motionReviewFrame >= (_motionReviewDoorwayRoute ?
                    moveFrames * 2 + 6 : MotionReviewAttackFrame))
                {
                    Capture(unit, stage, snake || salamander ? "post-move" : "attack");
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

        private void BeginGuidedMotionReview(UnitEntityData unit)
        {
            if (_motionReviewAwakeBefore != null) return;
            _motionReviewAwakeBefore = Game.Instance.State.AwakeUnits.ToArray();
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
        }

        private void Finish(string stage, Exception error)
        {
            if (_motionReviewAwakeBefore != null)
            {
                if (_motionReviewAwakeAdded)
                    Game.Instance.State.AwakeUnits.Remove(_motionReviewSubject);
                // Other members of a live quantity group can enter or leave
                // AwakeUnits as their own native commands finish. This review
                // owns only the subject it added; the crowd stage separately
                // checks its exact whole-list restoration.
                _motionReviewAwakeRestored = _creatureReviewQuantity
                    ? !_motionReviewAwakeAdded || !Game.Instance.State
                        .AwakeUnits.Contains(_motionReviewSubject)
                    : Game.Instance.State.AwakeUnits
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
            // The renderer is required to be on whenever the game itself says
            // the unit is visible, and not otherwise. Kingmaker's EntityFader
            // legitimately disables a unit that has left the party's visible
            // area, which a single deliberately framed creature never does but
            // a member of a 1d4+1 crowd walking its own route does: a five-body
            // group spreads far enough that the engine hides some of it. The
            // defect this gate was built to catch - a unit the game considers
            // visible that nonetheless does not render - still fails, and at
            // least one capture must show the creature actually rendering, so
            // a creature that never appears cannot pass by staying hidden.
            bool renderedWhenVisible = _motionReviewCaptures.All(value =>
                value.IndexOf(";faderVisible=true", StringComparison.Ordinal) < 0 ||
                value.IndexOf(";rendererEnabled=true", StringComparison.Ordinal) >= 0);
            bool renderedAtLeastOnce = _motionReviewCaptures.Any(value =>
                value.IndexOf(";faderVisible=true", StringComparison.Ordinal) >= 0 &&
                value.IndexOf(";rendererEnabled=true", StringComparison.Ordinal) >= 0);
            bool inFrame = _motionReviewCaptures.Count == 4 &&
                renderedWhenVisible && renderedAtLeastOnce &&
                _motionReviewCaptures.All(value =>
                    value.IndexOf(";inFrame=true", StringComparison.Ordinal) >= 0 &&
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
            _motionReviewDoorwayValid = _motionReviewTravelValid &&
                _motionReviewDoorwayRoute && _motionReviewDoorwayCrossed &&
                !_motionReviewDoorwayDirectClear;
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
                _motionReviewAwakeRestored + ";awakeScope=" +
                (_creatureReviewQuantity ? "review-owned" : "whole-list") +
                ";pausedAfter=" +
                Game.Instance.IsPaused + ";nearbyDoors=" +
                _motionReviewNearbyDoors + ";doorwayRoute=" +
                _motionReviewDoorwayRoute + ";doorwayCrossed=" +
                _motionReviewDoorwayCrossed + ";doorwayValid=" +
                _motionReviewDoorwayValid + ";doorwayDirectClear=" +
                _motionReviewDoorwayDirectClear + ";floorGrid=" +
                _motionReviewFloorGrid + ";captures=" +
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
                name == ExpandedSummoningPteranodonViewPatch.DireBatBlueprintName ||
                name == ExpandedSummoningPteranodonViewPatch.GiantWaspBlueprintName ||
                name == ExpandedSummoningPteranodonViewPatch.StirgeBlueprintName;
        }

        private static bool IsGuidedMotionReview(UnitEntityData unit)
        {
            if (IsSprint9FlightReview(unit)) return true;
            string name = unit == null || unit.Blueprint == null ? null :
                unit.Blueprint.name;
            return name == ExpandedSummoningPteranodonViewPatch.AurochsBlueprintName ||
                name == ExpandedSummoningPteranodonViewPatch.BisonBlueprintName ||
                name == ExpandedSummoningPteranodonViewPatch.RhinocerosBlueprintName ||
                name == ExpandedSummoningPteranodonViewPatch.WoollyRhinocerosBlueprintName ||
                // Sprint 12's compact quadrupeds need the same treatment the
                // ungulates get: a guided subject is put on the awake list and
                // the game is unpaused for the measurement, which is what makes
                // a movement agent tick at all. Without it the first attempt
                // recorded maxDeltaTime=0, agentWantsMove=False and no travel.
                // Dog is included even though it keeps the native view, because
                // the gate is about navigation rather than about the mesh.
                name == ExpandedSummoningPteranodonViewPatch.DireRatBlueprintName ||
                name == ExpandedSummoningPteranodonViewPatch.HyenaBlueprintName ||
                name == ExpandedSummoningPteranodonViewPatch.GoblinDogBlueprintName ||
                name == "KMG_Summoning_Unit_Dog" ||
                // Sprint 13's three ride the same ground agents and need the
                // same guided measurement.
                name == ExpandedSummoningPteranodonViewPatch.WolverineBlueprintName ||
                name == ExpandedSummoningPteranodonViewPatch.ShadowMastiffBlueprintName ||
                name == ExpandedSummoningPteranodonViewPatch.PoisonousFrogBlueprintName ||
                // The Sprint 16 ground assertions require the same real
                // awake/unpaused native movement measurement as these rigs.
                name == "KMG_Summoning_Unit_Crocodile" ||
                name == "KMG_Summoning_Unit_DireCrocodile" ||
                // The two exact original snakes use this same native
                // awake/unpaused movement and appearance-settlement scope.
                name == "KMG_Summoning_Unit_Viper" ||
                name == "KMG_Summoning_Unit_ConstrictorSnake" ||
                // The exact production hybrid uses the same surveyed native
                // movement probe. Combat actions are observed separately.
                name == SalamanderRulesPolicy.UnitName;
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
            // Flying and large ground summons may appear at an edge. Use the
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
            _motionReviewNearbyDoors = DescribeSprint9NearbyDoors(
                anchor, graph);
            _motionReviewFloorGrid = DescribeSprint9FloorGrid(anchor);
            // The disposable working save's surveyed floor has connected
            // nodes on both sides of the room opening. Let native A* select
            // its path; never install a forced path or relocate the party.
            Vector3 requestedDestination = anchor.clampedPosition +
                new Vector3(9f, 0f, -3f);
            Pathfinding.NNInfo destination = AstarPath.active.GetNearest(
                requestedDestination);
            UnitEntityData[] party = Game.Instance.Player.Party.Where(value =>
                value != null && value.IsInGame).ToArray();
            Pathfinding.NNInfo[] starts = new[]
                {
                    new Vector3(3f, 0f, 0f),
                    new Vector3(-3f, 0f, 0f),
                    new Vector3(-6f, 0f, 0f),
                    new Vector3(0f, 0f, 3f),
                    new Vector3(6f, 0f, 0f)
                }
                .Select(offset => anchor.clampedPosition + offset)
                .Select(requested => new { requested,
                    nearest = AstarPath.active.GetNearest(requested) })
                .Where(value => value.nearest.node != null &&
                    value.nearest.node.Walkable &&
                    value.nearest.node.Area == anchor.node.Area &&
                    value.nearest.node.GraphIndex == anchor.node.GraphIndex &&
                    Vector3.Distance(value.requested,
                        value.nearest.clampedPosition) <= 0.5f &&
                    party.All(member => Vector3.Distance(member.Position,
                        value.nearest.clampedPosition) >= 2.5f))
                .Select(value => value.nearest).ToArray();
            Pathfinding.NNInfo start = starts.FirstOrDefault();
            bool doorLandmark = UnityEngine.Object.FindObjectsOfType<Transform>()
                .Any(value => value != null && value.gameObject.activeInHierarchy &&
                    value.name == "Palace_SmallWall_01_Door_05" &&
                    Vector3.Distance(value.position, anchor.clampedPosition +
                        new Vector3(1f, -0.36f, -4.5f)) <= 1f);
            bool endpoints = start.node != null && destination.node != null &&
                start.node.Walkable && destination.node.Walkable &&
                start.node.Area == anchor.node.Area &&
                destination.node.Area == anchor.node.Area &&
                start.node.GraphIndex == anchor.node.GraphIndex &&
                destination.node.GraphIndex == anchor.node.GraphIndex &&
                Vector3.Distance(requestedDestination,
                    destination.clampedPosition) <= 0.5f &&
                party.All(member => Vector3.Distance(member.Position,
                    destination.clampedPosition) >= 2.5f);
            if (!doorLandmark || !endpoints)
                throw new InvalidOperationException(
                    "Sprint 9 doorway route is not the surveyed connected native path.");
            _motionReviewDoorwayRoute = true;
            _motionReviewDoorwayCrossingX = anchor.clampedPosition.x + 7f;
            _motionReviewDoorwayCrossingZ = anchor.clampedPosition.z - 2f;
            _motionReviewDoorwayDirectClear =
                Sprint9FlightLineClear(graph, start, destination);
            unit.Position = start.clampedPosition;
            unit.View.transform.position = start.clampedPosition;
            return destination.clampedPosition;
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

        /// <summary>Read-only native scene/navmesh survey for a real doorway route.</summary>
        private static string DescribeSprint9NearbyDoors(Pathfinding.NNInfo anchor,
            Pathfinding.IRaycastableGraph graph)
        {
            Transform[] doors = UnityEngine.Object.FindObjectsOfType<Transform>()
                .Where(value => value != null && value.gameObject.activeInHierarchy &&
                    (value.name.IndexOf("_door_", StringComparison.OrdinalIgnoreCase) >= 0 ||
                     value.name.IndexOf("_arch_", StringComparison.OrdinalIgnoreCase) >= 0 ||
                     value.name.StartsWith("door", StringComparison.OrdinalIgnoreCase)) &&
                    Vector3.Distance(value.position, anchor.clampedPosition) <= 25f)
                .OrderBy(value => Vector3.Distance(value.position,
                    anchor.clampedPosition))
                .Take(32).ToArray();
            if (doors.Length == 0) return "<none within 25m>";
            return string.Join("|", doors.Select(door =>
            {
                string[] axes = { "forward", "right" };
                Vector3[] directions = { door.forward, door.right };
                string sides = string.Join(",", axes.Select((axis, index) =>
                {
                    Vector3 direction = directions[index];
                    direction.y = 0f;
                    if (direction.sqrMagnitude < 0.0001f)
                        return axis + ":no-horizontal-axis";
                    direction.Normalize();
                    Vector3 left = door.position - direction * 2.5f;
                    Vector3 right = door.position + direction * 2.5f;
                    Pathfinding.NNInfo a = AstarPath.active.GetNearest(left);
                    Pathfinding.NNInfo b = AstarPath.active.GetNearest(right);
                    bool connected = a.node != null && b.node != null &&
                        a.node.Walkable && b.node.Walkable &&
                        a.node.Area == b.node.Area &&
                        a.node.GraphIndex == anchor.node.GraphIndex &&
                        b.node.GraphIndex == anchor.node.GraphIndex;
                    float gapA = Vector3.Distance(left, a.clampedPosition);
                    float gapB = Vector3.Distance(right, b.clampedPosition);
                    bool straight = connected &&
                        Sprint9FlightLineClear(graph, a, b);
                    return axis + ":connected=" + connected +
                        "/offsets=" + gapA.ToString("0.##",
                            CultureInfo.InvariantCulture) + "/" +
                        gapB.ToString("0.##", CultureInfo.InvariantCulture) +
                        "/areas=" + (a.node == null ? "none" :
                            a.node.Area.ToString()) + "/" +
                        (b.node == null ? "none" : b.node.Area.ToString()) +
                        "/straight=" + straight + "/a=" +
                        a.clampedPosition.ToString("F2") + "/b=" +
                        b.clampedPosition.ToString("F2");
                }).ToArray());
                return door.name + "@" + door.position.ToString("F2") +
                    ";distance=" + Vector3.Distance(door.position,
                        anchor.clampedPosition)
                        .ToString("0.##", CultureInfo.InvariantCulture) +
                    ";" + sides;
            }).ToArray());
        }

        /// <summary>Read-only local native floor connectivity around the party.</summary>
        private static string DescribeSprint9FloorGrid(Pathfinding.NNInfo anchor)
        {
            var rows = new List<string>();
            for (int dz = -18; dz <= 18; dz += 3)
            {
                var cells = new List<string>();
                for (int dx = -18; dx <= 18; dx += 3)
                {
                    Vector3 requested = anchor.clampedPosition +
                        new Vector3(dx, 0f, dz);
                    Pathfinding.NNInfo nearest = AstarPath.active.GetNearest(
                        requested);
                    float gap = Vector2.Distance(
                        new Vector2(requested.x, requested.z),
                        new Vector2(nearest.clampedPosition.x,
                            nearest.clampedPosition.z));
                    string cell = nearest.node == null ||
                        !nearest.node.Walkable || gap > 0.6f ? "X" :
                        nearest.node.GraphIndex != anchor.node.GraphIndex ?
                        "G" : nearest.node.Area.ToString();
                    cells.Add(cell);
                }
                rows.Add(dz + ":" + string.Join(",", cells.ToArray()));
            }
            return "center=" + anchor.clampedPosition.ToString("F2") +
                ";step=3;dx=-18..18;rows=" +
                string.Join("/", rows.ToArray());
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
                unit, _request.EvidenceDirectory, fileName) + ";moment=" + moment +
                ";nativeFadeWaitFrames=" + _motionReviewCaptureFadeWaited +
                ";nativeFadeWaitTotal=" + _motionReviewCaptureFadeTotal);
            _motionReviewCaptureFadeWaited = 0;
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
        internal static string WriteExpandedSummoningPartyCameraCapture(
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

        /// <summary>Request-local overhead or oblique view of live units;
        /// restores the game's exact camera pose and render targets.</summary>
        internal static string WriteExpandedSummoningOverheadStrikeCapture(
            UnitEntityData attacker, UnitEntityData target,
            string evidenceDirectory, string fileName,
            float cameraHeight = 9f, Vector3? cameraOffset = null)
        {
            if (attacker == null || attacker.View == null || target == null ||
                target.View == null || string.IsNullOrWhiteSpace(evidenceDirectory))
                return "png=<none>;reason=missing-unit";
            CameraRig rig = TeleportationCastingCamera();
            Camera camera = rig == null ? null : rig.Camera;
            if (camera == null) camera = Camera.main;
            if (camera == null) return "png=<none>;reason=no-camera";
            Vector3 priorPosition = camera.transform.position;
            Quaternion priorRotation = camera.transform.rotation;
            RenderTexture priorTarget = camera.targetTexture;
            RenderTexture priorActive = RenderTexture.active;
            RenderTexture renderTexture = null;
            Texture2D output = null;
            try
            {
                Vector3 midpoint = (attacker.Position + target.Position) * 0.5f;
                camera.transform.position = midpoint +
                    (cameraOffset ?? Vector3.up * cameraHeight);
                camera.transform.rotation = cameraOffset.HasValue ?
                    Quaternion.LookRotation(midpoint + Vector3.up * 1.2f -
                        camera.transform.position, Vector3.up) :
                    Quaternion.LookRotation(Vector3.down, Vector3.forward);
                renderTexture = new RenderTexture(MotionReviewCaptureWidth,
                    MotionReviewCaptureHeight, 24, RenderTextureFormat.ARGB32);
                camera.targetTexture = renderTexture;
                camera.Render();
                Vector3 attackerViewport = camera.WorldToViewportPoint(
                    attacker.View.transform.position);
                Vector3 targetViewport = camera.WorldToViewportPoint(
                    target.View.transform.position);
                RenderTexture.active = renderTexture;
                output = new Texture2D(MotionReviewCaptureWidth,
                    MotionReviewCaptureHeight, TextureFormat.RGBA32, false, false);
                output.ReadPixels(new Rect(0, 0, MotionReviewCaptureWidth,
                    MotionReviewCaptureHeight), 0, 0);
                output.Apply(false, false);
                byte[] png = EncodeExpandedSummoningPng(output);
                if (png == null || png.Length < 4096)
                    return "png=<none>;reason=empty-render";
                File.WriteAllBytes(Path.Combine(evidenceDirectory, fileName), png);
                return "png=" + fileName + ";bytes=" + png.Length +
                    ";attackerViewport=" + attackerViewport.ToString("F2") +
                    ";targetViewport=" + targetViewport.ToString("F2");
            }
            catch (Exception error)
            { return "png=<none>;reason=" + error.GetType().Name; }
            finally
            {
                camera.targetTexture = priorTarget;
                camera.transform.position = priorPosition;
                camera.transform.rotation = priorRotation;
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
