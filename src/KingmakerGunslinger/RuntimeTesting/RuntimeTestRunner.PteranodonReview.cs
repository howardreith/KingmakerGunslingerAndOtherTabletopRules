using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
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
        private bool _motionReviewOverlayWasOpen;
        private readonly List<string> _motionReviewCaptures = new List<string>();
        private readonly List<float> _motionReviewFrameSeconds = new List<float>();
        private string _motionReviewSummary = "<not run>";
        private bool _motionReviewValid;

        internal string MotionReviewSummary { get { return _motionReviewSummary; } }
        internal bool MotionReviewValid { get { return _motionReviewValid; } }

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
                            DissolveAmount(unit) > MotionReviewIntactDissolve))
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
                    Vector3 across = MotionReviewAcross(unit);
                    unit.Commands.Run(new UnitMoveTo(unit.Position + across * 6f, 0.5f));
                    _motionReviewFrame++;
                    return false;
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
            _motionReviewSummary = "stage=" + stage + ";waited=" + _motionReviewWaited +
                ";frames=" + _motionReviewFrame + ";frameMs=" + frameMs.ToString("0.#",
                    CultureInfo.InvariantCulture) + ";captures=" +
                _motionReviewCaptures.Count + (error == null ? "" :
                    ";fault=" + error.GetType().Name + ":" + error.Message) +
                ";" + string.Join("|", _motionReviewCaptures.ToArray());
            _motionReviewComplete = true;
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
