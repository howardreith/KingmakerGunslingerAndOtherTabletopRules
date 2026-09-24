using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.View;
using Kingmaker.Visual.Animation.Kingmaker;
using KingmakerGunslinger.Summoning;
using UnityEngine;

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
    /// The review is bounded: it scrolls the party camera to the creature,
    /// renders the game camera itself to a file (what the player sees, minus
    /// the mod manager's overlay), commands one short move and one attack
    /// animation, renders again at fixed frames, then interrupts everything it
    /// started. It takes about thirty updates and changes nothing the stage
    /// goes on to measure.
    /// </summary>
    internal sealed partial class RuntimeTestRunner
    {
        private const int MotionReviewCaptureWidth = 1280;
        private const int MotionReviewCaptureHeight = 720;
        private const int MotionReviewMoveFrames = 12;
        private const int MotionReviewAttackFrame = 30;

        private int _motionReviewFrame = -1;
        private bool _motionReviewComplete;
        private UnitEntityData _motionReviewSubject;
        private UnitAnimationActionHandle _motionReviewAttack;
        private bool _motionReviewOverlayWasOpen;
        private readonly List<string> _motionReviewCaptures = new List<string>();
        private readonly List<float> _motionReviewFrameSeconds = new List<float>();
        private string _motionReviewSummary = "<not run>";
        private bool _motionReviewValid;

        internal string MotionReviewSummary { get { return _motionReviewSummary; } }
        internal bool MotionReviewValid { get { return _motionReviewValid; } }

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
                if (_motionReviewFrame < 0)
                {
                    _motionReviewSubject = (units ?? Array.Empty<UnitEntityData>())
                        .FirstOrDefault(value => value != null &&
                            value.Blueprint != null && value.Blueprint.name ==
                            ExpandedSummoningPteranodonViewPatch
                                .PteranodonBlueprintName);
                    if (_motionReviewSubject == null || _motionReviewSubject.View == null)
                    {
                        _motionReviewSummary = "stage=" + stage + ";no-subject";
                        _motionReviewComplete = true;
                        return true;
                    }
                    _motionReviewOverlayWasOpen = SetModManagerOverlay(false);
                    _motionReviewFrame = 0;
                }
                else
                {
                    _motionReviewFrameSeconds.Add(Time.unscaledDeltaTime);
                    _motionReviewFrame++;
                }

                UnitEntityData unit = _motionReviewSubject;
                if (_motionReviewFrame == 0)
                {
                    Capture(unit, stage, "idle");
                    Vector3 across = MotionReviewAcross(unit);
                    unit.Commands.Run(new UnitMoveTo(unit.Position + across * 6f, 0.5f));
                    return false;
                }
                if (_motionReviewFrame == MotionReviewMoveFrames)
                {
                    Capture(unit, stage, "moving-a");
                    return false;
                }
                if (_motionReviewFrame == MotionReviewMoveFrames * 2)
                {
                    Capture(unit, stage, "moving-b");
                    unit.Commands.InterruptMove();
                    UnitAnimationManager manager = unit.View == null ? null :
                        unit.View.AnimationManager;
                    _motionReviewAttack = manager == null ? null :
                        manager.CreateHandle(UnitAnimationType.MainHandAttack, false);
                    if (_motionReviewAttack != null) manager.Execute(_motionReviewAttack);
                    return false;
                }
                if (_motionReviewFrame < MotionReviewAttackFrame) return false;

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
            catch (Exception error)
            {
                Finish(stage, error);
                return true;
            }
        }

        private void Finish(string stage, Exception error)
        {
            if (_motionReviewOverlayWasOpen) SetModManagerOverlay(true);
            double frameMs = _motionReviewFrameSeconds.Count == 0 ? 0d :
                _motionReviewFrameSeconds.Average() * 1000d;
            bool inFrame = _motionReviewCaptures.Count == 4 &&
                _motionReviewCaptures.All(value =>
                    value.IndexOf(";inFrame=true", StringComparison.Ordinal) >= 0);
            _motionReviewValid = error == null && inFrame;
            _motionReviewSummary = "stage=" + stage + ";frames=" +
                _motionReviewFrame + ";frameMs=" + frameMs.ToString("0.#",
                    CultureInfo.InvariantCulture) + ";captures=" +
                _motionReviewCaptures.Count + (error == null ? "" :
                    ";fault=" + error.GetType().Name + ":" + error.Message) +
                ";" + string.Join("|", _motionReviewCaptures.ToArray());
            _motionReviewComplete = true;
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
            string fileName = "pteranodon-review-" + stage + "-" + moment + ".png";
            _motionReviewCaptures.Add(WriteExpandedSummoningPartyCameraCapture(
                unit, _request.EvidenceDirectory, fileName) + ";moment=" + moment);
        }

        /// <summary>
        /// Renders the game's own camera, scrolled to the unit, into a file:
        /// the party-camera frame the player would see, with the game's
        /// lighting and image effects and without the mod manager's IMGUI
        /// overlay, which a screen capture would include.
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
                return "png=" + fileName + ";bytes=" + png.Length.ToString(
                        CultureInfo.InvariantCulture) + ";viewport=" +
                    viewport.x.ToString("0.###", CultureInfo.InvariantCulture) + "," +
                    viewport.y.ToString("0.###", CultureInfo.InvariantCulture) +
                    ";inFrame=" + (inFrame ? "true" : "false") +
                    ";rendererEnabled=" + (renderer != null && renderer.enabled ?
                        "true" : "false") + ";mesh=" + (renderer == null ||
                        renderer.sharedMesh == null ? "<none>" :
                        renderer.sharedMesh.name) + ";visual=" +
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
