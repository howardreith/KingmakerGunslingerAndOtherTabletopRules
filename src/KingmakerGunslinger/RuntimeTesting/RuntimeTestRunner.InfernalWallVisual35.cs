using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Kingmaker;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.Utility;
using Kingmaker.View;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        // Point-anchored ordinary party-camera capture: the same camera, zoom,
        // resolution and encoding as the unit-anchored capture, scrolled to a
        // world point such as an area's centre instead of a creature.
        private IEnumerable<int> InfernalWallSettleCameraAt35(Vector3 focus)
        {
            CameraRig rig = TeleportationCastingCamera();
            if (rig == null) throw new InvalidOperationException("Native party camera missing.");
            bool waiting = _sprint25WaitingForUnityAppearance;
            try
            {
                _sprint25WaitingForUnityAppearance = true;
                rig.ScrollToImmediately(focus);
                for (int frame = 0; frame < 12; frame++) yield return 0;
            }
            finally { _sprint25WaitingForUnityAppearance = waiting; }
        }

        private JObject InfernalWallCaptureAt35(Vector3 focus, string fileName)
        {
            CameraRig rig = TeleportationCastingCamera();
            Camera camera = rig == null ? Camera.main : rig.Camera;
            if (camera == null) return new JObject { ["png"] = null, ["reason"] = "no-camera" };
            if (rig != null) rig.ScrollToImmediately(focus);
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
                RenderTexture.active = renderTexture;
                output = new Texture2D(MotionReviewCaptureWidth, MotionReviewCaptureHeight,
                    TextureFormat.RGBA32, false, false);
                output.ReadPixels(new Rect(0, 0, MotionReviewCaptureWidth, MotionReviewCaptureHeight), 0, 0);
                output.Apply(false, false);
                byte[] png = EncodeExpandedSummoningPng(output);
                if (png == null || png.Length < 4096)
                    return new JObject { ["png"] = null, ["reason"] = "empty-render" };
                File.WriteAllBytes(Path.Combine(_request.EvidenceDirectory, fileName), png);
                Color32[] pixels = output.GetPixels32();
                double luminance = 0d, centre = 0d;
                int sampled = 0, centreSampled = 0;
                for (int index = 0; index < pixels.Length; index += 997)
                {
                    Color32 pixel = pixels[index];
                    double value = 0.2126 * pixel.r + 0.7152 * pixel.g + 0.0722 * pixel.b;
                    luminance += value; sampled++;
                    int x = index % MotionReviewCaptureWidth, y = index / MotionReviewCaptureWidth;
                    if (Math.Abs(x - MotionReviewCaptureWidth / 2) < MotionReviewCaptureWidth / 6 &&
                        Math.Abs(y - MotionReviewCaptureHeight / 2) < MotionReviewCaptureHeight / 6)
                    { centre += value; centreSampled++; }
                }
                Vector3 viewport = camera.WorldToViewportPoint(focus);
                return new JObject { ["png"] = fileName, ["bytes"] = png.Length,
                    ["focus"] = SurveyVector(focus), ["camera"] = SurveyVector(camera.transform.position),
                    ["viewport"] = new JArray(viewport.x, viewport.y, viewport.z),
                    ["meanLuma"] = sampled == 0 ? 0d : luminance / sampled,
                    ["centreLuma"] = centreSampled == 0 ? 0d : centre / centreSampled };
            }
            catch (Exception error)
            {
                camera.targetTexture = priorTarget;
                return new JObject { ["png"] = null, ["reason"] = error.GetType().Name + ": " + error.Message };
            }
            finally
            {
                RenderTexture.active = priorActive;
                if (renderTexture != null) UnityEngine.Object.Destroy(renderTexture);
                if (output != null) UnityEngine.Object.Destroy(output);
            }
        }

        // A controlled inspection light for one rendered frame. It is not
        // player lighting: it only shows whether an effect has a readable form
        // once the night scene stops hiding it. Removed before the next frame.
        private JObject InfernalWallInspectionCaptureAt35(Vector3 focus, string fileName)
        {
            var light = new GameObject("KMG_S35_InspectionLight");
            try
            {
                Light value = light.AddComponent<Light>();
                value.type = LightType.Directional;
                value.color = Color.white;
                value.intensity = 1.1f;
                value.shadows = LightShadows.None;
                light.transform.rotation = Quaternion.Euler(55f, 30f, 0f);
                float ambient = RenderSettings.ambientIntensity;
                JObject capture = InfernalWallCaptureAt35(focus, fileName);
                capture["controlledInspectionLight"] = "One white directional light (intensity 1.1) for this frame only; not player lighting.";
                capture["sceneAmbientIntensity"] = ambient;
                capture["sceneAmbientLight"] = RenderSettings.ambientLight.ToString();
                return capture;
            }
            finally { UnityEngine.Object.DestroyImmediate(light); }
        }

        // Structured presence of an area's native spawned effect: whether the
        // view spawned it, its particle systems are alive, and what draws it.
        private static JObject InfernalWallAreaFx35(AreaEffectEntityData area)
        {
            var row = new JObject { ["area"] = area?.UniqueId };
            object view = area == null ? null : (object)area.View;
            row["viewPresent"] = view != null;
            var fx = ReadExpandedSummoningOptionalMember(view, "m_SpawnedFx") as GameObject;
            row["spawnedFx"] = fx == null ? null : fx.name;
            row["blueprintFx"] = area?.Blueprint?.Fx?.AssetId;
            if (fx == null) { row["fxPresent"] = false; return row; }
            row["fxPresent"] = true;
            row["fxActive"] = fx.activeInHierarchy;
            row["fxPosition"] = SurveyVector(fx.transform.position);
            // The particle module is not a compile reference; read its public
            // state through the installed runtime type.
            Component[] systems = fx.GetComponentsInChildren<Component>(true).Where(value =>
                value != null && value.GetType().Name == "ParticleSystem").ToArray();
            Func<Component, string, object> read = (value, name) => {
                var property = value.GetType().GetProperty(name);
                return property == null ? null : property.GetValue(value, null); };
            row["particleSystems"] = systems.Length;
            row["playingSystems"] = systems.Count(value => read(value, "isPlaying") as bool? == true);
            row["aliveParticles"] = systems.Sum(value => (read(value, "particleCount") as int?) ?? 0);
            Renderer[] renderers = fx.GetComponentsInChildren<Renderer>(true);
            row["renderers"] = new JArray(renderers.Where(value => value != null).Take(24).Select(value => new JObject {
                ["name"] = value.name, ["type"] = value.GetType().Name, ["enabled"] = value.enabled,
                ["visible"] = value.isVisible,
                ["shader"] = value.sharedMaterial == null || value.sharedMaterial.shader == null ? null : value.sharedMaterial.shader.name,
                ["material"] = value.sharedMaterial == null ? null : value.sharedMaterial.name,
                ["tint"] = value.sharedMaterial != null && value.sharedMaterial.HasProperty("_TintColor") ?
                    value.sharedMaterial.GetColor("_TintColor").ToString() :
                    value.sharedMaterial != null && value.sharedMaterial.HasProperty("_Color") ?
                        value.sharedMaterial.GetColor("_Color").ToString() : null,
                ["boundsSize"] = SurveyVector(value.bounds.size) }));
            row["visibleRenderers"] = renderers.Count(value => value != null && value.enabled && value.isVisible);
            return row;
        }

        // Scenery between the ordinary party camera and a body: physics hits
        // and tall non-creature renderer bounds on the camera ray. Creature
        // views and particle effects are not scenery.
        private static JObject InfernalTailOcclusion35(UnitEntityData owner, UnitEntityData target)
        {
            CameraRig rig = TeleportationCastingCamera();
            Camera camera = rig == null ? Camera.main : rig.Camera;
            if (camera == null) return new JObject { ["occluderCount"] = 999, ["reason"] = "no-camera" };
            Vector3 eye = camera.transform.position;
            var occluders = new HashSet<string>(StringComparer.Ordinal);
            var rays = new JArray();
            Renderer[] scenery = UnityEngine.Object.FindObjectsOfType<Renderer>().Where(value =>
                value != null && value.enabled && value.isVisible && value.GetType().Name != "ParticleSystemRenderer" &&
                value.bounds.size.y >= .6f && value.GetComponentInParent<UnitEntityView>() == null).ToArray();
            Vector3 back = owner.OrientationDirection; back.y = 0f;
            if (back.sqrMagnitude < .01f) back = Vector3.forward;
            back.Normalize();
            var points = new[] {
                new KeyValuePair<string, Vector3>("owner-body", owner.Position + Vector3.up * 1.4f),
                new KeyValuePair<string, Vector3>("owner-tail", owner.Position + Vector3.up * .9f - back * .9f),
                new KeyValuePair<string, Vector3>("target-body", target.Position + Vector3.up * .9f) };
            foreach (var point in points)
            {
                Vector3 direction = point.Value - eye;
                float distance = direction.magnitude;
                var ray = new Ray(eye, direction / distance);
                var hits = new JArray();
                foreach (RaycastHit hit in Physics.RaycastAll(ray, Math.Max(0f, distance - .4f), ~0,
                    QueryTriggerInteraction.Ignore))
                {
                    if (hit.collider == null || hit.collider.GetComponentInParent<UnitEntityView>() != null) continue;
                    string name = "collider:" + hit.collider.name;
                    occluders.Add(name); hits.Add(name);
                }
                foreach (Renderer renderer in scenery)
                {
                    float entry;
                    Bounds bounds = renderer.bounds;
                    if (bounds.Contains(point.Value) || !bounds.IntersectRay(ray, out entry) ||
                        entry >= distance - .5f) continue;
                    string name = "renderer:" + renderer.name;
                    occluders.Add(name); hits.Add(name);
                }
                rays.Add(new JObject { ["point"] = point.Key, ["world"] = SurveyVector(point.Value),
                    ["distance"] = distance, ["hits"] = hits });
            }
            return new JObject { ["camera"] = SurveyVector(eye), ["occluderCount"] = occluders.Count,
                ["occluders"] = new JArray(occluders.OrderBy(value => value, StringComparer.Ordinal)),
                ["rays"] = rays,
                ["method"] = "Physics.RaycastAll plus visible non-creature renderer bounds of at least 0.6 m height intersecting the camera ray before the body." };
        }

        // The tail fight uses an established clear Wall lane that carries no
        // lingering area, chosen so the ordinary party camera sees both bodies.
        // Placement precedes the measured command; nothing moves during it.
        private IEnumerable<int> InfernalTailOpenLane35(ExpandedSummoningCorrectionFixture fixture,
            UnitEntityData owner, UnitEntityData target, int feet, JObject receipt)
        {
            var lanes = new List<KeyValuePair<string, InfernalWallFixturePlan35>>();
            if (_infernalWallExpiryPlan35 != null)
                lanes.Add(new KeyValuePair<string, InfernalWallFixturePlan35>("expiry", _infernalWallExpiryPlan35));
            if (_infernalWallRun35 != null)
            {
                try
                {
                    lanes.Add(new KeyValuePair<string, InfernalWallFixturePlan35>("tail",
                        InfernalWallNextLane35(_infernalWallRun35, "tail", _infernalWallPlans35.ToArray())));
                }
                catch (InvalidOperationException error) { receipt["tailLaneSurveyError"] = error.Message; }
            }
            var rows = new JArray();
            receipt["candidates"] = rows;
            int bestCount = int.MaxValue;
            Vector3 bestOrigin = Vector3.zero, bestTarget = Vector3.zero;
            string bestLane = null;
            float spacing = owner.Corpulence + target.Corpulence + new Feet(feet).Meters;
            foreach (var lane in lanes)
            {
                foreach (bool reverse in new[] { false, true })
                {
                    if (bestCount == 0) break;
                    Vector3 from = reverse ? lane.Value.Destination : lane.Value.Start;
                    Vector3 to = reverse ? lane.Value.Start : lane.Value.Destination;
                    Vector3 direction = to - from; direction.y = 0f; direction.Normalize();
                    Vector3 origin = from + direction * 1.5f;
                    Vector3 targetPoint = origin + direction * spacing;
                    PlaceExpandedSummoningUnit(owner, origin);
                    PlaceExpandedSummoningUnit(target, targetPoint);
                    foreach (int frame in SummonOverhaulSettlePartyCamera(owner)) yield return frame;
                    JObject occlusion = InfernalTailOcclusion35(owner, target);
                    int count = (int?)occlusion["occluderCount"] ?? 999;
                    rows.Add(new JObject { ["lane"] = lane.Key, ["reverse"] = reverse,
                        ["origin"] = SurveyVector(owner.Position), ["target"] = SurveyVector(target.Position),
                        ["occlusion"] = occlusion });
                    if (count >= bestCount) continue;
                    bestCount = count; bestOrigin = origin; bestTarget = targetPoint;
                    bestLane = lane.Key + (reverse ? "-reverse" : "-forward");
                }
            }
            receipt["selected"] = bestLane;
            receipt["selectedOccluders"] = bestLane == null ? (int?)null : bestCount;
            if (bestLane == null)
            {
                InfernalCombatLane34(fixture, owner, target, feet);
                receipt["fallback"] = "native-fixture-lane";
                yield break;
            }
            PlaceExpandedSummoningUnit(owner, bestOrigin);
            PlaceExpandedSummoningUnit(target, bestTarget);
            foreach (var actor in new[] { owner, target })
            {
                actor.Wake(0f);
                if (!Game.Instance.State.AwakeUnits.Contains(actor)) Game.Instance.State.AwakeUnits.Add(actor);
                actor.SightCache.ClearIfNeeded();
            }
            if (owner.HasLOS(target)) owner.Memory.Add(target).Visible = true;
            if (target.HasLOS(owner)) target.Memory.Add(owner).Visible = true;
            owner.JoinCombat(); target.JoinCombat(); Game.Instance.Player.UpdateIsInCombat();
            new Kingmaker.Controllers.Combat.UnitCombatJoinController().Tick();
            new Kingmaker.Controllers.Combat.UnitCombatPrepareController().Tick();
            var probe = new UnitAttack(target); probe.Init(owner);
            bool ready = owner.Descriptor.State.CanAct && owner.IsEnemy(target) && owner.HasLOS(target) &&
                probe.IsUnitEnoughClose;
            Sprint25Check("sprint35-tail-open-lane-" + feet, ready,
                new JObject { ["owner"] = owner.UniqueId, ["target"] = target.UniqueId, ["feetBeyondBodies"] = feet,
                    ["nativeApproachRadius"] = probe.ApproachRadius, ["distance"] = owner.DistanceTo(target),
                    ["lane"] = bestLane, ["occluders"] = bestCount },
                "The tail lane stands on an established clear Wall lane with native attack eligibility unchanged.");
            if (!ready) throw new InvalidOperationException("Open tail lane eligibility is not established.");
        }

        // One ordinary-camera frame of the tail sequence with its state.
        private JObject InfernalTailFrame35(UnitEntityData owner, UnitEntityData target,
            UnitAttack command, BoneDevilTailVisual35 tail,
            string stage, string fileName)
        {
            string image = WriteExpandedSummoningPartyCameraCapture(owner, _request.EvidenceDirectory, fileName);
            // A command that has not started has no attack plan yet; its
            // native getters throw rather than return null.
            string planned = null, clipName = null;
            try
            {
                if (command != null && command.IsStarted)
                {
                    planned = command.PlannedAttack?.Hand?.MaybeWeapon?.Blueprint?.name;
                    clipName = command.Animation?.ActiveAnimation?.GetPlayableClip()?.name;
                }
            }
            catch (Exception error) { planned = "unavailable:" + error.GetType().Name; }
            return new JObject { ["stage"] = stage, ["frame"] = Time.frameCount,
                ["gameSeconds"] = Game.Instance.TimeController.GameTime.TotalSeconds.ToString("0.###", CultureInfo.InvariantCulture),
                ["image"] = image, ["tailResting"] = tail.Resting, ["contacts"] = tail.Contacts,
                ["plannedWeapon"] = planned,
                ["animationClip"] = clipName,
                ["ownerToTarget"] = owner.DistanceTo(target),
                ["occlusion"] = InfernalTailOcclusion35(owner, target) };
        }
    }
}
