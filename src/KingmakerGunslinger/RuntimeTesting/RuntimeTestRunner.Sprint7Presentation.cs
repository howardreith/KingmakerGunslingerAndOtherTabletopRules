using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UI.Tooltip;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.Utility;
using Kingmaker.View;
using KingmakerGunslinger.Assets;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    // Sprint 7 (S7 T1-T7 / REP-04, VIS-01 to VIS-06): save-free native
    // observers for the presentation and audio repairs. Each cell observes
    // the product's own behavior; the observer only places units, issues
    // ordinary commands and reads state. Listening samples are posted by the
    // observer itself, clearly labelled, and never stand in for a product cue.
    internal sealed partial class RuntimeTestRunner
    {
        private const string S7BeforeRoot = "C:/Dev/KingmakerGunslingerLab/runtime-evidence/s7/before-assets";

        // ---------------------------------------------------------------
        // T2 / T3: models (VIS-01 Nalfeshnee, VIS-02 Salamander)
        // ---------------------------------------------------------------

        private sealed class S7Model
        {
            internal string Key, Folder, MeshFile, MeshSha;
            internal bool Salamander;
            internal string[][] Details;
        }

        // Each detail: name, bone, radius, then the view direction in the
        // unit's own frame (x right, y up, z forward).
        private static readonly S7Model[] S7Models =
        {
            new S7Model { Key = "nalfeshnee", Folder = "sprint39-nalfeshnee", MeshFile = "nalfeshnee-mesh.json",
                MeshSha = "d34e6c194d5b6c8c798a1ead77b9248e48e44721fccb2491eccee79aa1199aaf",
                Details = new[] { new[] { "wing-root-left", "Torso_Upper", ".72", "-.75", ".45", "-.5" },
                    new[] { "wing-root-right", "Torso_Upper", ".72", ".75", ".45", "-.5" },
                    new[] { "wing-roots-rear", "Torso_Upper", ".82", ".0", ".45", "-.9" } } },
            new S7Model { Key = "salamander", Folder = "sprint17-serpents", MeshFile = "salamander-human-mesh.json", Salamander = true,
                MeshSha = "b5524a694cac0657aaf37ce995e3143f1e0a18e8aa98d799d7fe17ed901c0f19",
                Details = new[] { new[] { "waist-front", "Pelvis", ".5", ".25", ".2", ".95" },
                    new[] { "waist-rear", "Pelvis", ".5", "-.25", ".25", "-.95" },
                    new[] { "left-hand", "L_Hand", ".24", "-.6", ".3", ".75" },
                    new[] { "right-hand", "R_Hand", ".24", ".6", ".3", ".75" } } },
        };

        /// <summary>Shows the pinned pre-Sprint-7 export on one live view, then puts the current export back.</summary>
        private sealed class S7BodySwap : IDisposable
        {
            private SkinnedMeshRenderer _renderer;
            private Material _material;
            private Mesh _current, _before;
            private Texture _currentAlbedo;
            private Texture2D _beforeAlbedo;
            internal int CurrentVertices, BeforeVertices;

            // Bounded fault injection for the rollback check only.
            internal static string Inject;
            internal static Mesh LastRolledBackMesh;
            internal static JObject LastRollback;

            internal S7BodySwap(UnitEntityData unit, S7Model model)
            {
                try { Build(unit, model); }
                catch (Exception original)
                {
                    // Nothing borrowed or allocated survives a failed set-up;
                    // every release runs and its failures are reported with
                    // the original cause.
                    Mesh allocated = _before;
                    List<string> failures = Sprint7ObserverPolicy.RunReleases(new[] {
                        new KeyValuePair<string, Action>("mesh", () => { if (_renderer != null && _current != null) _renderer.sharedMesh = _current; }),
                        new KeyValuePair<string, Action>("albedo", () => { if (_material != null && _currentAlbedo != null) _material.SetTexture(R7MainTexture, _currentAlbedo); }),
                        new KeyValuePair<string, Action>("comparison-mesh", () => { if (_before != null) UnityEngine.Object.Destroy(_before); }),
                        new KeyValuePair<string, Action>("comparison-albedo", () => { if (_beforeAlbedo != null) UnityEngine.Object.Destroy(_beforeAlbedo); }) });
                    bool restored = _renderer == null || _current == null || ReferenceEquals(_renderer.sharedMesh, _current);
                    LastRolledBackMesh = allocated;
                    LastRollback = new JObject { ["failures"] = new JArray(failures), ["rendererRestored"] = restored, ["comparisonMeshAllocated"] = allocated != null };
                    throw Sprint7ObserverPolicy.SetupFailure("S7BodySwap", original, failures, restored ? 0 : 1);
                }
            }

            private void Build(UnitEntityData unit, S7Model model)
            {
                _renderer = unit.View.GetComponentsInChildren<SkinnedMeshRenderer>(true).SingleOrDefault(value =>
                    value.sharedMesh != null && (model.Salamander
                        ? value.sharedMesh.name.StartsWith("KMG_SalamanderHuman_", StringComparison.Ordinal) &&
                          value.sharedMesh.name.EndsWith("_Mesh", StringComparison.Ordinal)
                        : value.sharedMesh.name == "KMG_" + model.Key + "_Original"));
                if (_renderer == null) throw new InvalidOperationException("Owned body renderer absent: " + model.Key);
                string meshPath = Path.Combine(Path.Combine(S7BeforeRoot, model.Folder), model.MeshFile);
                string actual = R7FileSha(meshPath);
                if (actual != model.MeshSha) throw new InvalidDataException("Before mesh hash differs: " + actual);
                string json = File.ReadAllText(meshPath);
                string[] names;
                PteranodonAssetRuntime.AlbedoRequirement requirement;
                _before = PteranodonAssetRuntime.BuildMesh(json, out names, out requirement,
                    model.Salamander ? SalamanderHumanBindingPolicy.Names : InfernalVisualPolicy36.Bones);
                _current = _renderer.sharedMesh;
                if (names.Length != _renderer.bones.Length)
                    throw new InvalidDataException("Before bone count differs from the live renderer.");
                _before.bindposes = _current.bindposes;
                _before.name = "KMG_" + model.Key + "_S7BeforeEvidence";
                if (Inject == "after-comparison-mesh") { _renderer.sharedMesh = _before; throw new InvalidOperationException("injected failure after comparison-mesh allocation"); }
                CurrentVertices = _current.vertexCount; BeforeVertices = _before.vertexCount;
                if (!model.Salamander)
                {
                    string reason;
                    _beforeAlbedo = PteranodonAssetRuntime.LoadAlbedo(Path.GetDirectoryName(meshPath), requirement, out reason);
                    if (_beforeAlbedo == null) throw new InvalidDataException("Before albedo: " + reason);
                    _material = _renderer.sharedMaterial;
                    _currentAlbedo = _material.GetTexture(R7MainTexture);
                }
            }

            internal void ShowBefore() { _renderer.sharedMesh = _before; if (_material != null) _material.SetTexture(R7MainTexture, _beforeAlbedo); }
            internal void ShowCurrent() { _renderer.sharedMesh = _current; if (_material != null) _material.SetTexture(R7MainTexture, _currentAlbedo); }
            internal bool CurrentShown => _renderer != null && ReferenceEquals(_renderer.sharedMesh, _current);

            public void Dispose()
            {
                if (_renderer != null) ShowCurrent();
                if (_before != null) UnityEngine.Object.Destroy(_before);
                if (_beforeAlbedo != null) UnityEngine.Object.Destroy(_beforeAlbedo);
            }
        }

        private static float S7MaxDissolve(UnitEntityData unit)
        {
            float max = 0f;
            if (unit == null || unit.View == null) return -1f;
            foreach (Renderer renderer in unit.View.GetComponentsInChildren<Renderer>(true))
            {
                if (renderer == null || !renderer.enabled || !(renderer is SkinnedMeshRenderer || renderer is MeshRenderer)) continue;
                foreach (Material material in renderer.sharedMaterials)
                    if (material != null && material.HasProperty("_Dissolve"))
                        max = Math.Max(max, material.GetFloat("_Dissolve"));
            }
            return max;
        }

        // The detail bone must exist on the live body; framing never falls
        // back to the unit's origin.
        private static Bounds S7BoneBounds(UnitEntityData unit, string bone, float radius)
        {
            Transform found = unit.View.GetComponentsInChildren<SkinnedMeshRenderer>(true)
                .SelectMany(value => value.bones ?? new Transform[0])
                .FirstOrDefault(value => value != null && value.name == bone);
            if (found == null) throw new InvalidOperationException("Detail bone absent on the live body: " + bone);
            float scale = unit.View.transform.lossyScale.y;
            return new Bounds(found.position, Vector3.one * (radius * scale * 2f / Mathf.Sqrt(3f)));
        }

        // The camera placement R6Clip.Shot uses, and the colliders between the
        // subject and that camera: scenery, and other units than the subject.
        private static JObject S7Occluders(UnitEntityData unit, Bounds bounds, Vector3 direction, float minimumRadius)
        {
            CameraRig rig = TeleportationCastingCamera();
            Camera camera = rig == null ? Camera.main : rig.Camera;
            float radius = Mathf.Max(bounds.extents.magnitude, minimumRadius);
            float fit = Mathf.Sin(Mathf.Deg2Rad * Mathf.Min(camera.fieldOfView, camera.fieldOfView * 16f / 9f) * .5f);
            float distance = Mathf.Max(radius * 1.25f / Mathf.Max(fit, .05f), Mathf.Min(2.4f, radius * 3f));
            RaycastHit[] hits = Physics.RaycastAll(bounds.center, direction.normalized, distance);
            int scenery = 0, others = 0;
            foreach (RaycastHit hit in hits)
            {
                if (hit.collider == null) continue;
                UnitEntityView view = hit.collider.GetComponentInParent<UnitEntityView>();
                if (view == null) scenery++;
                else if (!ReferenceEquals(view, unit.View)) others++;
            }
            // The centre and eight points at half the extents, each toward the
            // camera: a ray blocked by scenery hides part of the body.
            Vector3 cameraAt = bounds.center + direction.normalized * distance;
            int rays = 0, blocked = 0;
            foreach (float x in new[] { 0f, -.5f, .5f })
                foreach (float y in new[] { -.5f, .5f })
                    foreach (float z in new[] { -.5f, .5f })
                    {
                        if (x == 0f && (y != -.5f || z != -.5f)) continue;
                        Vector3 from = x == 0f ? bounds.center : bounds.center + Vector3.Scale(bounds.extents, new Vector3(x, y, z));
                        Vector3 toCamera = cameraAt - from;
                        rays++;
                        if (Physics.RaycastAll(from, toCamera.normalized, toCamera.magnitude).Any(hit => hit.collider != null && hit.collider.GetComponentInParent<UnitEntityView>() == null)) blocked++;
                    }
            return new JObject { ["scenery"] = scenery, ["otherUnits"] = others, ["distance"] = Math.Round(distance, 2), ["rays"] = rays, ["blockedRays"] = blocked };
        }

        private static readonly float[] S7YawOffsets = { 0f, 15f, -15f, 30f, -30f, 45f, -45f, 60f, -60f };

        private static Vector3 S7ClearDirection(UnitEntityData unit, Bounds bounds, Vector3 nominal, float minimumRadius, bool detail, out JObject measured)
        {
            JObject best = null; Vector3 chosen = nominal; int bestScore = int.MaxValue;
            foreach (float yaw in S7YawOffsets)
            {
                Vector3 direction = Quaternion.Euler(0f, yaw, 0f) * nominal;
                JObject hits = S7Occluders(unit, bounds, direction, minimumRadius);
                int score = ((int)hits["scenery"] + (detail ? (int)hits["otherUnits"] : 0)) * 100 + (int)hits["blockedRays"];
                hits["yawOffset"] = yaw;
                if (score < bestScore) { bestScore = score; best = hits; chosen = direction; }
                if (score == 0) break;
            }
            measured = best;
            return chosen;
        }

        private IEnumerable<int> S7Stills(UnitEntityData unit, S7Model model, string state, string scene, JObject row)
        {
            foreach (int step in SummonOverhaulSettlePartyCamera(unit)) yield return step;
            SkinnedMeshRenderer body = S7Body(unit);
            // R1-F3: the gameplay-distance party view is bound to the enabled
            // body renderer (its exact name) and judged on its own readback.
            string partyReadback = body == null ? "png=<none>;reason=no-body;inFrame=false" : WriteExpandedSummoningPartyCameraCapture(unit, _request.EvidenceDirectory,
                "s7-" + model.Key + "-" + scene + "-" + state + "-party.png", body.name);
            string partyReason;
            bool partyValid = Sprint7ObserverPolicy.PartyCaptureValid(partyReadback, body == null || body.sharedMesh == null ? null : body.sharedMesh.name, out partyReason);
            row["party"] = new JObject { ["readback"] = partyReadback, ["renderer"] = body == null ? null : body.name, ["valid"] = partyValid, ["reason"] = partyReason };
            row["dissolve"] = S7MaxDissolve(unit);
            row["body"] = body == null ? null : new JObject { ["renderer"] = S7RendererName(body), ["enabled"] = body.enabled && body.gameObject.activeInHierarchy,
                ["mesh"] = body.sharedMesh == null ? null : body.sharedMesh.name, ["vertices"] = body.sharedMesh == null ? -1 : body.sharedMesh.vertexCount,
                ["dissolve"] = S7RendererDissolve(body), ["cutoutDissolve"] = S7RendererCutoutDissolve(body) };
            var occluders = new JObject(); row["occluders"] = occluders;
            Quaternion facing = unit.View.transform.rotation;
            foreach (var view in new[] { new { Name = "front", Local = new Vector3(.55f, .32f, .78f) }, new { Name = "rear", Local = new Vector3(-.55f, .32f, -.78f) } })
            {
                JObject measured;
                Vector3 direction = S7ClearDirection(unit, R6RenderBounds(unit), facing * view.Local.normalized, .9f, false, out measured);
                using (var clip = new R6Clip(_request.EvidenceDirectory, "s7-" + model.Key + "-" + scene + "-" + state + "-" + view.Name,
                    () => R6RenderBounds(unit), direction))
                {
                    occluders[view.Name] = measured;
                    clip.Shot(new JObject { ["state"] = state, ["occluders"] = occluders[view.Name].DeepClone() });
                    row[view.Name] = clip.OutOfFrame == 0;
                }
            }
            foreach (string[] detail in model.Details)
            {
                float radius = R7Number(detail[2]);
                var local = new Vector3(R7Number(detail[3]), R7Number(detail[4]), R7Number(detail[5])).normalized;
                string bone = detail[1];
                float minimum = radius * unit.View.transform.lossyScale.y;
                Bounds bounds = S7BoneBounds(unit, bone, radius);
                JObject measured;
                Vector3 direction = S7ClearDirection(unit, bounds, facing * local, minimum, true, out measured);
                using (var clip = new R6Clip(_request.EvidenceDirectory, "s7-" + model.Key + "-" + scene + "-" + state + "-detail-" + detail[0],
                    () => S7BoneBounds(unit, bone, radius), direction, minimum))
                {
                    occluders["detail-" + detail[0]] = measured;
                    clip.Shot(new JObject { ["state"] = state, ["bone"] = bone, ["occluders"] = occluders["detail-" + detail[0]].DeepClone() });
                    row["detail-" + detail[0]] = clip.OutOfFrame == 0;
                }
            }
        }

        private IEnumerable<int> S7Motion(UnitEntityData unit, UnitEntityData hostile, S7Model model, string state, string scene,
            Vector3 start, Vector3 target, Vector3 bearing, JObject row)
        {
            Func<Bounds> subject = () => R7BoneBounds(unit, model.Salamander ? "Spine_2" : "Torso_Upper", 1.2f);
            PlaceExpandedSummoningUnit(unit, start);
            foreach (int step in R6Wait(14)) yield return step;
            string prefix = "s7-" + model.Key + "-" + scene + "-" + state + "-";
            using (var clip = new R6Clip(_request.EvidenceDirectory, prefix + "idle", subject, bearing))
                for (int shot = 0; shot < 24; shot++)
                { clip.Shot(R6AnimationState(unit)); foreach (int step in R6Wait(2)) yield return step; }

            Vector3 before = unit.Position;
            var move = new UnitMoveTo(target, .3f); move.Init(unit);
            float peak = 0f;
            if (move.CanStart)
            {
                unit.Commands.Run(move);
                using (var clip = new R6Clip(_request.EvidenceDirectory, prefix + "move", subject, bearing))
                    for (int frame = 0; frame < 150 && !move.IsFinished; frame++)
                    {
                        peak = Math.Max(peak, unit.View.MovementAgent.Velocity.magnitude);
                        if (frame % 2 == 0) clip.Shot(R6AnimationState(unit, move));
                        yield return 0;
                    }
            }
            row["moveTravel"] = Vector3.Distance(before, unit.Position);
            row["movePeakVelocity"] = peak;
            unit.Commands.InterruptAll(command => true); unit.Commands.RemoveFinishedAndUpdateQueue();

            // A turn: a short move at right angles to the creature's facing,
            // filmed from the same bearing.
            float facingBefore = unit.Orientation;
            Vector3 side = Quaternion.Euler(0f, 100f, 0f) * unit.View.transform.forward;
            Vector3 turnTarget = ObstacleAnalyzer.GetNearestNode(unit.Position + side * 2.2f).clampedPosition;
            var turn = new UnitMoveTo(turnTarget, .3f); turn.Init(unit);
            if (turn.CanStart)
            {
                unit.Commands.Run(turn);
                using (var clip = new R6Clip(_request.EvidenceDirectory, prefix + "turn", subject, bearing))
                    for (int frame = 0; frame < 150 && !turn.IsFinished; frame++)
                    {
                        if (frame % 2 == 0) clip.Shot(R6AnimationState(unit, turn));
                        yield return 0;
                    }
            }
            float turned = Mathf.Abs(Mathf.DeltaAngle(facingBefore, unit.Orientation));
            row["turnDegrees"] = turned;
            unit.Commands.InterruptAll(command => true); unit.Commands.RemoveFinishedAndUpdateQueue();

            if (hostile == null) yield break;
            PlaceExpandedSummoningUnit(unit, start);
            foreach (int step in R6Wait(8)) yield return step;
            hostile.Descriptor.Damage = 0;
            var attack = new UnitAttack(hostile);
            unit.Commands.Run(attack);
            using (var clip = new R6Clip(_request.EvidenceDirectory, prefix + "attack", subject, bearing))
            {
                int after = 0;
                for (int frame = 0; frame < 170 && after < 14; frame++)
                {
                    if (frame % 2 == 0) clip.Shot(R6AnimationState(unit, attack));
                    if (attack.IsFinished) after++;
                    yield return 0;
                }
                row["attackClips"] = R6DistinctClips(clip.Rows);
            }
            row["attackActed"] = attack.IsActed;
            row["attackResult"] = attack.Result.ToString();
            row["primaryHand"] = unit.Body.PrimaryHand.MaybeWeapon == null ? null : unit.Body.PrimaryHand.MaybeWeapon.Blueprint.name;
            unit.Commands.InterruptAll(command => true); unit.Commands.RemoveFinishedAndUpdateQueue();
            hostile.Descriptor.Damage = 0;
        }

        private IEnumerable<int> S7ModelCell(ExpandedSummoningCorrectionFixture fixture, S7Model model, string scene, Vector3 start, Vector3 target,
            UnitEntityData hostile, List<UnitEntityData> owned, JObject record)
        {
            var row = new JObject(); record[model.Key] = row;
            var cast = new JObject(); row["cast"] = cast;
            var spawned = new List<UnitEntityData>();
            foreach (int step in S7NaturalSummon(fixture, model.Key, cast, spawned)) yield return step;
            UnitEntityData unit = spawned[0]; owned.Add(unit);
            Sprint28SetBrainActivePreservingActions(unit, false);
            // The native appearance runs to completion on its own: no buff is
            // removed; the unit is moved to its mark only afterwards.
            DateTime appearDeadline = DateTime.UtcNow.AddSeconds(30);
            TimeSpan castAt = Game.Instance.Player.GameTime;
            while (DateTime.UtcNow < appearDeadline && S7Buff(unit, BlueprintRoot.Instance.SystemMechanics.SummonedUnitAppearBuff) != null) yield return 0;
            cast["appearanceEndedAfterSeconds"] = Math.Round((Game.Instance.Player.GameTime - castAt).TotalSeconds, 2);
            cast["appearanceEndedNaturally"] = S7Buff(unit, BlueprintRoot.Instance.SystemMechanics.SummonedUnitAppearBuff) == null;
            foreach (int step in R6Seconds(1f)) yield return step;
            DateTime settleDeadline = DateTime.UtcNow.AddSeconds(15);
            while (DateTime.UtcNow < settleDeadline && S7MaxDissolve(unit) > .02f) yield return 0;
            unit.Commands.InterruptAll(command => true); unit.Commands.RemoveFinishedAndUpdateQueue();
            PlaceExpandedSummoningUnit(unit, start);
            foreach (int step in R6Wait(30)) yield return step;
            row["unit"] = unit.Blueprint.name;
            row["settledDissolve"] = S7MaxDissolve(unit);
            using (var swap = new S7BodySwap(unit, model))
            {
                row["currentVertices"] = swap.CurrentVertices; row["beforeVertices"] = swap.BeforeVertices;
                var after = new JObject(); var before = new JObject();
                row["stillsAfter"] = after; row["stillsBefore"] = before;
                foreach (int step in S7Stills(unit, model, "after", scene, after)) yield return step;
                swap.ShowBefore();
                foreach (int step in R6Wait(4)) yield return step;
                foreach (int step in S7Stills(unit, model, "before", scene, before)) yield return step;
                swap.ShowCurrent();
                var motionAfter = new JObject(); var motionBefore = new JObject();
                row["motionAfter"] = motionAfter; row["motionBefore"] = motionBefore;
                Vector3 bearing = unit.View.transform.rotation * new Vector3(.55f, .32f, .78f).normalized;
                foreach (int step in S7Motion(unit, hostile, model, "after", scene, start, target, bearing, motionAfter)) yield return step;
                swap.ShowBefore();
                foreach (int step in S7Motion(unit, hostile, model, "before", scene, start, target, bearing, motionBefore)) yield return step;
                swap.ShowCurrent();
                bool shown = swap.CurrentShown;
                bool natural = (bool)cast["appearanceEndedNaturally"];
                bool bodyBound = new[] { after, before }.All(values => values["body"] != null && (bool)values["body"]["enabled"]) &&
                    (int)after["body"]["vertices"] == swap.CurrentVertices && (int)before["body"]["vertices"] == swap.BeforeVertices;
                bool settled = natural && bodyBound && (float)row["settledDissolve"] <= .02f && (float)after["dissolve"] <= .02f;
                bool framed = new[] { after, before }.All(values => values.Properties()
                    .Where(value => value.Name != "party" && value.Name != "dissolve" && value.Name != "occluders" && value.Name != "body").All(value => (bool)value.Value));
                bool party = new[] { after, before }.All(values => values["party"] != null && (bool)values["party"]["valid"]);
                row["partyBound"] = party;
                // Details must be clear of scenery and of other units; whole-body
                // views of scenery.
                bool clear = new[] { after, before }.All(values => ((JObject)values["occluders"]).Properties().All(value =>
                    (int)value.Value["scenery"] == 0 && (!value.Name.StartsWith("detail-", StringComparison.Ordinal) || (int)value.Value["otherUnits"] == 0)));
                row["clearOfOccluders"] = clear;
                // Whole-body views are context: any of their sampled body rays
                // blocked by scenery is identified here; anatomical claims rest on
                // the detail views, whose centre must be clear.
                row["obstructedWholeBodyViews"] = new JArray(new[] { new { Name = "after", Values = after }, new { Name = "before", Values = before } }
                    .SelectMany(entry => ((JObject)entry.Values["occluders"]).Properties().Where(value => !value.Name.StartsWith("detail-", StringComparison.Ordinal) &&
                        (int)value.Value["blockedRays"] > 0).Select(value => entry.Name + "-" + value.Name + ":" + (int)value.Value["blockedRays"] + "/" + (int)value.Value["rays"])));
                bool moved = (float)motionAfter["moveTravel"] > 1f && (float)motionBefore["moveTravel"] > 1f;
                bool turned = (float)motionAfter["turnDegrees"] > 45f && (float)motionBefore["turnDegrees"] > 45f;
                bool attacked = hostile == null || ((bool)motionAfter["attackActed"] && (bool)motionBefore["attackActed"]);
                bool different = swap.CurrentVertices != swap.BeforeVertices || model.Salamander;
                Sprint25Check("s7-" + model.Key + "-" + scene, shown && settled && framed && party && clear && moved && turned && attacked && different,
                    (JObject)row.DeepClone(),
                    "The summon cast through the published spell completes its native appearance on its own (no buff removed) and then shows the Sprint 7 export on its enabled body renderer (vertex count bound to the export; every body material reads _Dissolve 0); the pinned pre-Sprint-7 export is shown on the same live unit; whole-body views and every named-bone detail are framed and clear of scenery at their centre, details also of other units (each view keeps its nominal bearing or the yaw offset up to 60 degrees with the fewest blocked rays, recorded; whole-body views with any blocked body ray are identified as context only); the gameplay-distance party view of each state is a successful, lit, in-frame image whose readback names the enabled body renderer and its current mesh, with the native fade cleared and the body intact; and both complete the observed idle, movement, a turn of more than 45 degrees and any attack.");
            }
        }

        // Each observer fails on purpose after it has acquired something: the
        // body swap after allocating its comparison mesh and putting it on the
        // renderer; the projectile trace after its first patch. The original
        // failure must surface with a complete rollback, the renderer must be
        // back on its own mesh, the comparison mesh destroyed and no hook left.
        private IEnumerable<int> S7Rollback(ExpandedSummoningCorrectionFixture fixture, List<UnitEntityData> owned, JObject record)
        {
            var row = new JObject(); record["rollback"] = row;
            UnitEntityData unit = CastExpandedSummoningOwnTier(fixture, "nalfeshnee"); owned.Add(unit);
            Sprint28SetBrainActivePreservingActions(unit, false);
            foreach (int step in R6Wait(R6ArrivalFrames + 30)) yield return step;
            S7Model model = S7Models.First(value => value.Key == "nalfeshnee");
            Mesh meshBefore = S7Body(unit) == null ? null : S7Body(unit).sharedMesh;
            string swapError = null, swapInner = null;
            S7BodySwap.Inject = "after-comparison-mesh";
            try { using (new S7BodySwap(unit, model)) { } }
            catch (Exception error) { swapError = error.Message; swapInner = error.InnerException == null ? null : error.InnerException.Message; }
            finally { S7BodySwap.Inject = null; }
            yield return 0; yield return 0; // Unity destroys at the end of the frame
            bool meshDestroyed = S7BodySwap.LastRolledBackMesh == null;
            bool rendererRestored = meshBefore != null && S7Body(unit) != null && ReferenceEquals(S7Body(unit).sharedMesh, meshBefore);
            row["bodySwap"] = new JObject { ["error"] = swapError, ["original"] = swapInner, ["rollback"] = S7BodySwap.LastRollback == null ? null : S7BodySwap.LastRollback.DeepClone(),
                ["comparisonMeshDestroyed"] = meshDestroyed, ["rendererBackOnOwnMesh"] = rendererRestored };
            string traceError = null, traceInner = null;
            S7ProjectileTrace.Inject = "after-first-patch";
            try { using (new S7ProjectileTrace()) { } }
            catch (Exception error) { traceError = error.Message; traceInner = error.InnerException == null ? null : error.InnerException.Message; }
            finally { S7ProjectileTrace.Inject = null; }
            JObject traceRollback = S7ProjectileTrace.LastRollback;
            var normal = new S7ProjectileTrace();
            int normalInstalled = normal.OwnedHooks();
            var normalRow = new JObject();
            bool normalReleased = S7DisposeTrace(normal, normalRow);
            int normalAfter = normal.OwnedHooks();
            // R3-F4: a normal disposal whose CreateView unpatch fails: the
            // verdict fails, the other releases still run, and a later disposal
            // (injection cleared) removes the remaining hooks.
            var failing = new S7ProjectileTrace();
            var failedRow = new JObject();
            bool failedReleased;
            S7ProjectileTrace.Inject = "fail-normal-unpatch";
            try { failedReleased = S7DisposeTrace(failing, failedRow); }
            finally { S7ProjectileTrace.Inject = null; }
            failedRow["remainingCreateViewHooks"] = S7ProjectileTrace.HookCount(failing.CreateMethod, failing.Id);
            failedRow["remainingSpawnHooks"] = S7ProjectileTrace.HookCount(failing.SpawnMethod, failing.Id);
            failedRow["remainingProductHooks"] = S7ProjectileTrace.HookCount(failing.ProductMethod, failing.Id);
            var finalRow = new JObject();
            bool finalReleased = S7DisposeTrace(failing, finalRow);
            failedRow["afterRetry"] = finalRow;
            row["trace"] = new JObject { ["error"] = traceError, ["original"] = traceInner, ["rollback"] = traceRollback == null ? null : traceRollback.DeepClone(),
                ["normalInstalled"] = normalInstalled, ["normalAfterDispose"] = normalAfter, ["normalReleased"] = normalReleased, ["failedNormalDisposal"] = failedRow };
            bool failedCase = !failedReleased && ((JArray)failedRow["disposeFailures"]).Count == 1 &&
                ((string)failedRow["disposeFailures"][0]).Contains("injected failure of the CreateView unpatch") &&
                (int)failedRow["remainingCreateViewHooks"] == 2 && (int)failedRow["remainingSpawnHooks"] == 0 && (int)failedRow["remainingProductHooks"] == 0 &&
                finalReleased && (int)finalRow["ownedHooksAfterDispose"] == 0;
            bool swapOk = swapError != null && swapError.Contains("rollback complete") && swapInner == "injected failure after comparison-mesh allocation" &&
                meshDestroyed && rendererRestored && S7BodySwap.LastRollback != null && (bool)S7BodySwap.LastRollback["comparisonMeshAllocated"];
            bool traceOk = traceError != null && traceError.Contains("rollback complete") && traceInner == "injected failure after the first patch" &&
                traceRollback != null && (int)traceRollback["hooksBeforeRollback"] >= 1 && (int)traceRollback["hooksAfterRollback"] == 0 &&
                ((JArray)traceRollback["failures"]).Count == 0 && normalInstalled == 4 && normalAfter == 0 && normalReleased && failedCase;
            Sprint25Check("s7-observer-rollback", swapOk && traceOk, (JObject)row.DeepClone(),
                "A body swap that fails after allocating its comparison mesh and putting it on the renderer surfaces the original failure with a complete rollback: the renderer is back on its own mesh and the comparison mesh is destroyed. A projectile trace that fails after its first patch surfaces the original failure with no owned hook left; a normal trace then installs its 4 hooks and removes all of them (released verdict true). A normal disposal whose CreateView unpatch fails yields a failed release verdict while the native-spawn and product-wrapper releases still run (2 CreateView hooks remain, none elsewhere); a later disposal removes them.");
        }

        private IEnumerable<int> ReviewSprint7Models(ExpandedSummoningCorrectionFixture fixture, string phase, Cleanup2NativeDefaultScope mode)
        {
            bool outdoor = phase == ExpandedSummoningNativeProfilePolicy.Sprint39S7ModelsPhase;
            if (!IsSprint32Review || !_request.ExitAfterCompletion || !ReferenceEquals(fixture, _sprint25Fixture) ||
                fixture?.Scene == null || fixture.Caster == null || !_context.FeatureModules.Active.ExpandedSummoning ||
                (!outdoor && phase != ExpandedSummoningNativeProfilePolicy.Sprint39S7ModelsInteriorPhase) ||
                (outdoor && (fixture.Hostile == null || mode == null || !mode.Current || !Cleanup2NoNarrativeOverlay())))
                throw new InvalidOperationException("Exact fresh Sprint 7 model-review fixture with the summoning module enabled required.");
            string scene = outdoor ? "bright" : "dim";
            var record = new JObject { ["phase"] = phase, ["runId"] = _request.RunId, ["scene"] = scene,
                ["area"] = Game.Instance.CurrentlyLoadedArea?.name,
                ["limits"] = "Summons are cast through the published spell as native commands (caster level 20 only); the arrival and appearance complete naturally before the unit is moved to its mark. Body readbacks and the party capture name the enabled body renderer; whole-body views are context, details carry the anatomical claims. Commanded motion with brains inactive. Images are presentation evidence only; aesthetic approval is the owner's." };
            var owned = new List<UnitEntityData>();
            Vector3 centre = fixture.Caster.Position;
            UnitEntityData hostile = outdoor ? fixture.Hostile : null;
            Vector3? hostileHome = hostile?.Position;
            int? hostileHitPoints = hostile == null ? (int?)null : hostile.Descriptor.Stats.HitPoints.BaseValue;
            if (hostile != null) hostile.Descriptor.Stats.HitPoints.BaseValue = 100000;
            int priorCapture = Time.captureFramerate;
            Func<Vector3, Vector3> ground = value => ObstacleAnalyzer.GetNearestNode(value).clampedPosition;
            try
            {
                Vector3 anchor;
                if (outdoor)
                {
                    PlaceExpandedSummoningUnit(hostile, ground(centre + Vector3.forward * 4f + Vector3.left * 2f));
                    foreach (int step in R6Wait(10)) yield return step;
                    anchor = hostile.Position + Vector3.left * 2.3f;
                }
                else anchor = centre + Vector3.forward * 3f;
                Vector3 start = ground(anchor);
                Vector3 target = ground(start + (outdoor ? Vector3.back : Vector3.right) * 3f);
                Time.captureFramerate = 30;
                foreach (S7Model model in S7Models)
                {
                    S7Model current = model;
                    foreach (int step in R6Cell("s7-" + current.Key + "-" + scene, () => S7ModelCell(fixture, current, scene, start, target, hostile, owned, record))) yield return step;
                    foreach (UnitEntityData unit in owned.Where(value => !value.Destroyed).ToArray())
                    {
                        unit.Commands.InterruptAll(command => true); unit.Commands.RemoveFinishedAndUpdateQueue();
                        DisposeExpandedSummoningUnits(fixture.Created, new[] { unit });
                    }
                    foreach (int step in R6Wait(6)) yield return step;
                }
                if (outdoor)
                    foreach (int step in R6Cell("s7-rollback", () => S7Rollback(fixture, owned, record))) yield return step;
            }
            finally
            {
                Time.captureFramerate = priorCapture;
                var errors = new JArray();
                Action<string, Action> restore = (name, action) => { try { action(); } catch (Exception error) { errors.Add(name + ":" + error.Message); } };
                restore("combat", () => {
                    foreach (UnitEntityData unit in owned.Concat(new[] { fixture.Hostile, fixture.Caster }).Where(value => value != null && !value.Destroyed))
                    {
                        unit.Commands.InterruptAll(command => true); unit.Commands.RemoveFinishedAndUpdateQueue();
                        if (unit.CombatState.IsInCombat) unit.CombatState.LeaveCombat();
                    }
                });
                restore("hostile", () => { if (hostileHome.HasValue && !fixture.Hostile.Destroyed) { fixture.Hostile.Descriptor.Stats.HitPoints.BaseValue = hostileHitPoints.Value; fixture.Hostile.Descriptor.Damage = 0; PlaceExpandedSummoningUnit(fixture.Hostile, hostileHome.Value); } });
                PlaceExpandedSummoningUnit(fixture.Caster, centre);
                DisposeExpandedSummoningUnits(fixture.Created, owned.Where(value => !value.Destroyed).ToArray());
                Sprint25Check("s7-models-owned-state-restored", errors.Count == 0 && Time.captureFramerate == priorCapture,
                    new JObject { ["errors"] = errors }, "Commands, combat, positions and the capture framerate restore.");
                record["restoreErrors"] = errors;
                RuntimeTestResultWriter.WriteAtomic(Path.Combine(_request.EvidenceDirectory, phase + ".json"), record.ToString(Formatting.Indented));
            }
        }

        // ---------------------------------------------------------------
        // T1: Ghaele appearance through an ordinary summon's whole lifetime
        // ---------------------------------------------------------------

        private sealed class S7LifeSubject
        {
            internal string Label;
            internal UnitEntityData Unit;
            internal TimeSpan Spawned;
            internal TimeSpan? Ended;
            internal TimeSpan? AppearEnded;
            internal TimeSpan? LifetimeEnd;
            internal float MaxDissolveSettled = -1f;
            internal string MaxDissolveAt;
            internal readonly JArray Samples = new JArray();
            internal int[] MaterialIds = new int[0];
            // The body is the enabled skinned renderer with the most vertices;
            // "first skinned" is what the October observer read.
            internal string BodyRenderer, FirstSkinned;
            internal float MaxBodyDissolveSettled = -1f;
            internal string MaxBodyDissolveAt;
            internal JArray RendererCensus;
            internal readonly JArray Controllers = new JArray();
            internal int SampleCount;
        }

        private static string S7RendererName(Renderer renderer)
        {
            return renderer.GetType().Name + ":" + renderer.name + ":" +
                (renderer is SkinnedMeshRenderer && ((SkinnedMeshRenderer)renderer).sharedMesh != null
                    ? ((SkinnedMeshRenderer)renderer).sharedMesh.name : "");
        }

        private static float S7RendererDissolve(Renderer renderer)
        {
            float max = -1f;
            foreach (Material material in renderer.sharedMaterials)
                if (material != null && material.HasProperty("_Dissolve"))
                    max = Math.Max(max, material.GetFloat("_Dissolve"));
            return max;
        }

        // The native dissolve only removes surface when its cutout is on
        // (StandardMaterialController writes _DissolveCutout from the
        // animation's DissolveCutout); otherwise it draws the coloured edge.
        private static float S7RendererCutoutDissolve(Renderer renderer)
        {
            float max = 0f;
            foreach (Material material in renderer.sharedMaterials)
                if (material != null && material.HasProperty("_Dissolve") && material.IsKeywordEnabled("DISSOLVE_ON") &&
                    material.HasProperty("_DissolveCutout") && material.GetFloat("_DissolveCutout") >= .5f)
                    max = Math.Max(max, material.GetFloat("_Dissolve"));
            return max;
        }

        private static SkinnedMeshRenderer S7Body(UnitEntityData unit)
        {
            return unit.View.GetComponentsInChildren<SkinnedMeshRenderer>(true)
                .Where(value => value != null && value.enabled && value.sharedMesh != null)
                .OrderByDescending(value => value.sharedMesh.vertexCount).FirstOrDefault();
        }

        private static Buff S7Buff(UnitEntityData unit, BlueprintBuff blueprint)
        {
            if (unit == null || unit.Destroyed || blueprint == null) return null;
            return unit.Descriptor.Buffs.Enumerable.FirstOrDefault(value => ReferenceEquals(value.Blueprint, blueprint));
        }

        private static int[] S7MaterialIds(UnitEntityData unit)
        {
            if (unit == null || unit.View == null) return new int[0];
            return unit.View.GetComponentsInChildren<Renderer>(true)
                .Where(value => value is SkinnedMeshRenderer || value is MeshRenderer)
                .SelectMany(value => value.sharedMaterials).Where(value => value != null)
                .Select(value => value.GetInstanceID()).Distinct().OrderBy(value => value).ToArray();
        }

        private static void S7Sample(S7LifeSubject subject, string stage)
        {
            UnitEntityData unit = subject.Unit;
            TimeSpan clock = Game.Instance.Player.GameTime;
            bool alive = unit != null && !unit.Destroyed && unit.View != null;
            Buff summoned = S7Buff(unit, BlueprintRoot.Instance.SystemMechanics.SummonedUnitBuff);
            Buff appear = S7Buff(unit, BlueprintRoot.Instance.SystemMechanics.SummonedUnitAppearBuff);
            if (summoned != null) subject.LifetimeEnd = summoned.EndTime;
            if (alive && appear == null && !subject.AppearEnded.HasValue) subject.AppearEnded = clock;
            float dissolve = alive ? S7MaxDissolve(unit) : -1f;
            // Between appearance completion and the native end of life the
            // body is expected to stay undissolved.
            bool inLife = alive && subject.AppearEnded.HasValue && clock >= subject.AppearEnded.Value + TimeSpan.FromSeconds(1) &&
                (!subject.LifetimeEnd.HasValue || clock < subject.LifetimeEnd.Value - TimeSpan.FromSeconds(.5));
            if (inLife && dissolve > subject.MaxDissolveSettled)
            {
                subject.MaxDissolveSettled = dissolve;
                subject.MaxDissolveAt = stage + "@" + (clock - subject.Spawned).TotalSeconds.ToString("0.0");
            }
            if (!alive && !subject.Ended.HasValue) subject.Ended = clock;
            float body = -1f, first = -1f, bodyCut = -1f;
            JObject renderers = null;
            if (alive)
            {
                SkinnedMeshRenderer bodyRenderer = S7Body(unit);
                SkinnedMeshRenderer firstRenderer = unit.View.GetComponentsInChildren<SkinnedMeshRenderer>(true).FirstOrDefault();
                if (bodyRenderer != null) { body = S7RendererDissolve(bodyRenderer); bodyCut = S7RendererCutoutDissolve(bodyRenderer); subject.BodyRenderer = S7RendererName(bodyRenderer); }
                if (firstRenderer != null) { first = S7RendererDissolve(firstRenderer); subject.FirstSkinned = S7RendererName(firstRenderer); }
                if (subject.RendererCensus == null)
                    subject.RendererCensus = new JArray(unit.View.GetComponentsInChildren<Renderer>(true).Where(value => value != null)
                        .Select(value => new JObject { ["renderer"] = S7RendererName(value), ["enabled"] = value.enabled,
                            ["materials"] = new JArray(value.sharedMaterials.Where(material => material != null).Select(material =>
                                material.name + "|" + (material.shader == null ? "" : material.shader.name) +
                                "|dissolveKeyword=" + material.IsKeywordEnabled("DISSOLVE_ON"))) }));
                if (subject.SampleCount % 4 == 0)
                {
                    renderers = new JObject();
                    foreach (Renderer value in unit.View.GetComponentsInChildren<Renderer>(true))
                    {
                        if (value == null) continue;
                        float level = S7RendererDissolve(value);
                        if (level >= 0f) renderers[S7RendererName(value)] = Math.Round(level, 3);
                    }
                }
                if (inLife && bodyCut > subject.MaxBodyDissolveSettled)
                {
                    subject.MaxBodyDissolveSettled = bodyCut;
                    subject.MaxBodyDissolveAt = stage + "@" + (clock - subject.Spawned).TotalSeconds.ToString("0.0");
                }
            }
            subject.SampleCount++;
            subject.Samples.Add(new JObject { ["t"] = Math.Round((clock - subject.Spawned).TotalSeconds, 2), ["stage"] = stage,
                ["alive"] = alive, ["dissolve"] = dissolve, ["body"] = body, ["bodyCutout"] = bodyCut, ["firstSkinned"] = first, ["appear"] = appear != null,
                ["remaining"] = summoned == null ? (double?)null : Math.Round((summoned.EndTime - clock).TotalSeconds, 2),
                ["position"] = alive ? SurveyVector(unit.Position) : null, ["renderers"] = renderers });
        }

        // The real published summon ability as a native UnitUseAbility command
        // that progresses through the caster's own command queue (no forced
        // start or tick). Caster level 20 is the only override, so the summon
        // lives 20 rounds. Arrival and appearance buffs stay and the unit stays
        // where the summon put it.
        private IEnumerable<int> S7NaturalSummon(ExpandedSummoningCorrectionFixture fixture, string key, JObject row, List<UnitEntityData> into)
        {
            SummonVariantSpec variant = ExpandedSummoningOwnTierVariant(key, SummonMultiplicity.One);
            BlueprintAbility ability = ResolveExpandedSummoningExecution(fixture.Blueprints, variant, null);
            BlueprintUnit expected = fixture.Blueprints.OfType<BlueprintUnit>().Single(value => value.name ==
                ExpandedSummoningInternalName(ExpandedSummoningIdentityCatalog.UnitSymbol(variant.Creature)));
            UnitEntityData caster = fixture.Caster;
            var before = new HashSet<UnitEntityData>(Game.Instance.State.Units);
            // An existing ability is borrowed as it is (its overrides restored
            // afterwards); only an instance this observer granted is released.
            Ability existing = caster.Descriptor.Abilities.GetAbility(ability);
            Ability granted = Sprint7ObserverPolicy.ShouldGrant(existing != null) ? caster.Descriptor.AddFact(ability) as Ability : null;
            Ability used = existing ?? granted;
            AbilityParams priorOverride = used == null ? null : used.OverrideParams;
            row["borrowed"] = existing != null; row["granted"] = granted != null;
            UnitUseAbility command = null;
            try
            {
                if (used == null) throw new InvalidOperationException("The published summon was not granted: " + key);
                used.OverrideParams = new AbilityParams { CasterLevel = 20, SpellLevel = variant.ParentTier };
                command = new UnitUseAbility(new AbilityData(used), new TargetWrapper(caster.Position));
                row["ability"] = ability.name; row["castUtc"] = DateTime.UtcNow.ToString("o"); row["castGameSeconds"] = Game.Instance.Player.GameTime.TotalSeconds;
                caster.Commands.Run(command);
                DateTime deadline = DateTime.UtcNow.AddSeconds(25);
                while (DateTime.UtcNow < deadline && !(command.IsFinished && (command.ExecutionProcess == null || command.ExecutionProcess.IsEnded))) yield return 0;
            }
            finally
            {
                if (used != null) used.OverrideParams = priorOverride;
                if (Sprint7ObserverPolicy.ShouldRelease(granted, caster.Descriptor.Abilities.GetAbility(ability)))
                    caster.Descriptor.Abilities.RemoveFact(granted);
                row["restored"] = existing == null || ReferenceEquals(existing.OverrideParams, priorOverride);
            }
            Game.Instance.EntityCreator.Tick();
            row["commandResult"] = command.Result.ToString(); row["commandFinished"] = command.IsFinished;
            var spawned = Game.Instance.State.Units.Where(value => !before.Contains(value) && ReferenceEquals(value.Blueprint, expected)).ToList();
            row["spawned"] = spawned.Count;
            if (!command.IsFinished || command.Result != UnitCommand.ResultType.Success || spawned.Count != 1)
                throw new InvalidOperationException("The published summon did not complete with one creature: " + key + " result=" + command.Result + " spawned=" + spawned.Count);
            fixture.Created.Add(spawned[0]); into.Add(spawned[0]);
        }

        private IEnumerable<int> S7GhaeleCell(ExpandedSummoningCorrectionFixture fixture, string scene, UnitEntityData hostile,
            List<UnitEntityData> owned, JObject record)
        {
            var row = new JObject(); record["lifetime"] = row;
            TimeSpan start = Game.Instance.Player.GameTime;
            bool wasPaused = Game.Instance.IsPaused;
            Game.Instance.IsPaused = false;
            var subjects = new List<S7LifeSubject>();
            UnitEntityData donor = null;
            var attacks = new S7AttackLog(); EventBus.Subscribe(attacks);
            try
            {
                var casts = new JObject(); row["casts"] = casts;
                var spawned = new List<UnitEntityData>();
                var castA = new JObject(); casts["ghaele-a"] = castA;
                foreach (int step in S7NaturalSummon(fixture, "ghaele-azata", castA, spawned)) yield return step;
                UnitEntityData first = spawned[0]; owned.Add(first);
                Sprint28SetBrainActivePreservingActions(first, false);
                subjects.Add(new S7LifeSubject { Label = "ghaele-a", Unit = first, Spawned = Game.Instance.Player.GameTime });
                var castSibling = new JObject(); casts["bralani-sibling"] = castSibling;
                foreach (int step in S7NaturalSummon(fixture, "bralani-azata", castSibling, spawned)) yield return step;
                UnitEntityData sibling = spawned[1]; owned.Add(sibling);
                Sprint28SetBrainActivePreservingActions(sibling, false);
                subjects.Add(new S7LifeSubject { Label = "bralani-sibling", Unit = sibling, Spawned = Game.Instance.Player.GameTime });
                // The native donor (same prefab and materials), spawned rather
                // than summoned: it shows whether the summons' appearance
                // reaches a shared donor material.
                var donorBlueprint = ResourcesLibrary.TryGetBlueprint<BlueprintUnit>("bc8ca1437c0f48948b317b7e64febf0d");
                if (donorBlueprint != null)
                {
                    donor = Game.Instance.EntityCreator.SpawnUnit(donorBlueprint,
                        ObstacleAnalyzer.GetNearestNode(fixture.Caster.Position + Vector3.back * 3f).clampedPosition,
                        Quaternion.identity, fixture.Caster.HoldingState);
                    Sprint28SetBrainActivePreservingActions(donor, false);
                    subjects.Add(new S7LifeSubject { Label = "native-donor", Unit = donor, Spawned = Game.Instance.Player.GameTime });
                }
                row["donorSpawned"] = donor != null;
                Func<Bounds> frame = () => R6RenderBounds(first);
                S7LifeSubject second = null;
                bool moved = false, attacked = false, secondSpawned = false;
                Vector3 moveFrom = Vector3.zero;
                var moveRow = new JObject(); var attackRow = new JObject();
                row["move"] = moveRow; row["attack"] = attackRow;
                double lastShot = -10, lastSample = -10;
                using (var clip = new R6Clip(_request.EvidenceDirectory, "s7-ghaele-" + scene + "-lifetime", frame, null))
                {
                    var deadline = DateTime.UtcNow.AddMinutes(7);
                    while (DateTime.UtcNow < deadline)
                    {
                        double t = (Game.Instance.Player.GameTime - subjects[0].Spawned).TotalSeconds;
                        if (t - lastSample >= .5)
                        {
                            lastSample = t;
                            foreach (S7LifeSubject subject in subjects) S7Sample(subject, "life");
                        }
                        bool firstAlive = !first.Destroyed && first.View != null;
                        if (firstAlive && t - lastShot >= 2)
                        {
                            lastShot = t;
                            var state = R6AnimationState(first);
                            SkinnedMeshRenderer shotBody = S7Body(first);
                            state["t"] = Math.Round(t, 2); state["dissolve"] = S7MaxDissolve(first);
                            state["body"] = shotBody == null ? -1f : S7RendererDissolve(shotBody);
                            state["bodyCutout"] = shotBody == null ? -1f : S7RendererCutoutDissolve(shotBody);
                            clip.Shot(state);
                        }
                        // The native dissolve-animation controllers, every ten seconds.
                        if (Math.Floor(t / 10) > Math.Floor((t - Time.deltaTime) / 10))
                            foreach (S7LifeSubject subject in subjects.Where(value => value.Unit != null && !value.Unit.Destroyed && value.Unit.View != null))
                                subject.Controllers.Add(new JObject { ["t"] = Math.Round(t, 1), ["state"] = Cleanup2GiantAppearanceState(subject.Unit) });
                        // Ordinary movement after the appearance settles.
                        if (!moved && firstAlive && subjects[0].AppearEnded.HasValue && t > 8)
                        {
                            moved = true; moveFrom = first.Position;
                            Vector3 to = ObstacleAnalyzer.GetNearestNode(first.Position + Vector3.right * 4f).clampedPosition;
                            var move = new UnitMoveTo(to, .3f);
                            first.Commands.Run(move);
                            moveRow["issuedAt"] = Math.Round(t, 2);
                        }
                        if (moved && moveRow["travel"] == null && t > 16 && firstAlive)
                            moveRow["travel"] = Vector3.Distance(moveFrom, first.Position);
                        // Combat: ordinary attacks on the fixture hostile.
                        if (hostile != null && !attacked && firstAlive && t > 20)
                        {
                            attacked = true;
                            hostile.Descriptor.Damage = 0;
                            var attack = new UnitAttack(hostile);
                            first.Commands.Run(attack);
                            attackRow["issuedAt"] = Math.Round(t, 2);
                        }
                        if (attacked && attackRow["rules"] == null && t > 30 && firstAlive)
                        {
                            // The native weapon-attack rules this Ghaele resolved.
                            attackRow["rules"] = new JArray(attacks.Rows.Where(value => (string)value["initiator"] == first.UniqueId));
                            first.Commands.InterruptAll(command => true); first.Commands.RemoveFinishedAndUpdateQueue();
                            if (hostile != null) hostile.Descriptor.Damage = 0;
                        }
                        // A second Ghaele later in the first one's life.
                        if (!secondSpawned && t > 36)
                        {
                            secondSpawned = true;
                            var castB = new JObject(); casts["ghaele-b"] = castB;
                            foreach (int step in S7NaturalSummon(fixture, "ghaele-azata", castB, spawned)) yield return step;
                            UnitEntityData other = spawned[spawned.Count - 1]; owned.Add(other);
                            Sprint28SetBrainActivePreservingActions(other, false);
                            second = new S7LifeSubject { Label = "ghaele-b", Unit = other, Spawned = Game.Instance.Player.GameTime };
                            subjects.Add(second);
                            row["materialsDistinct"] = !S7MaterialIds(first).Intersect(S7MaterialIds(other)).Any();
                            row["donorMaterialsDistinct"] = donor == null || !S7MaterialIds(donor).Intersect(S7MaterialIds(first)).Any();
                        }
                        bool summonsGone = subjects.Where(value => value.Label != "native-donor").All(value => value.Ended.HasValue);
                        if (summonsGone) break;
                        yield return 0;
                    }
                }
                foreach (S7LifeSubject subject in subjects) S7Sample(subject, "final");
                var rows = new JArray();
                foreach (S7LifeSubject subject in subjects)
                    rows.Add(new JObject { ["label"] = subject.Label,
                        ["appearEndedAt"] = subject.AppearEnded.HasValue ? Math.Round((subject.AppearEnded.Value - subject.Spawned).TotalSeconds, 2) : (double?)null,
                        ["lifetimeSeconds"] = subject.LifetimeEnd.HasValue ? Math.Round((subject.LifetimeEnd.Value - subject.Spawned).TotalSeconds, 2) : (double?)null,
                        ["endedAt"] = subject.Ended.HasValue ? Math.Round((subject.Ended.Value - subject.Spawned).TotalSeconds, 2) : (double?)null,
                        ["maxDissolveInLife"] = subject.MaxDissolveSettled, ["maxDissolveAt"] = subject.MaxDissolveAt,
                        ["bodyRenderer"] = subject.BodyRenderer, ["firstSkinned"] = subject.FirstSkinned,
                        ["maxBodyCutoutDissolveInLife"] = subject.MaxBodyDissolveSettled, ["maxBodyCutoutDissolveAt"] = subject.MaxBodyDissolveAt,
                        ["rendererCensus"] = subject.RendererCensus, ["controllers"] = subject.Controllers,
                        ["samples"] = subject.Samples });
                row["subjects"] = rows;
                row["paused"] = Game.Instance.IsPaused;
                S7LifeSubject a = subjects[0];
                bool visible = a.AppearEnded.HasValue && a.MaxBodyDissolveSettled >= 0f && a.MaxBodyDissolveSettled <= .02f;
                bool expired = a.Ended.HasValue && a.LifetimeEnd.HasValue &&
                    a.Ended.Value >= a.LifetimeEnd.Value - TimeSpan.FromSeconds(1) && a.Ended.Value <= a.LifetimeEnd.Value + TimeSpan.FromSeconds(15);
                bool walked = moveRow["travel"] != null && (float)moveRow["travel"] > 2f;
                bool fought = hostile == null || attackRow["rules"] != null && ((JArray)attackRow["rules"]).Count > 0;
                Sprint25Check("s7-ghaele-" + scene + "-visible-lifetime", visible && expired && walked && fought, new JObject { ["a"] = rows[0].DeepClone(), ["move"] = moveRow.DeepClone(), ["attack"] = attackRow.DeepClone() },
                    "An ordinarily summoned Ghaele's body renderer (the enabled skinned renderer with the most vertices) has no surface-removing (cutout) dissolve above 0.02 from appearance completion until its native lifetime ends, an ordinary move travels more than 2 m, and the summon is removed at its genuine expiry. The raw _Dissolve, which also drives the native non-cutout edge band, is recorded beside it.");
                // Attribution: the native dissolve animation controllers on the
                // summon, read every ten seconds in life. Each animation must
                // have a signature (delay, lifetime, loop, cutout, direction)
                // that the native donor, spawned and never summoned, also runs,
                // must not be a cutout dissolve, and every driven material's
                // _Dissolve must equal the animation's own current value. Each
                // unit's loop starts at its own spawn, so values at the same
                // moment differ in phase and are not compared.
                S7LifeSubject nativeDonor = subjects.FirstOrDefault(value => value.Label == "native-donor");
                Func<JToken, string> signature = animation => string.Format(CultureInfo.InvariantCulture, "delay={0:F2} lifetime={1:F2} loop={2} cutout={3} backwards={4}",
                    (double)animation["Delay"], (double)animation["Lifetime"], (bool)animation["LoopAnimation"], (bool)animation["DissolveCutout"], (bool)animation["PlayBackwards"]);
                Func<S7LifeSubject, IEnumerable<JObject>> snapshots = subject => subject.Controllers.OfType<JObject>().Where(value => value["state"] is JObject);
                Func<JObject, IEnumerable<JObject>> animationsOf = state => (state["controllers"] as JArray ?? new JArray()).OfType<JObject>()
                    .SelectMany(controller => (controller["animations"] as JArray ?? new JArray()).OfType<JObject>().Select(animation => { var copy = (JObject)animation.DeepClone(); copy["driven"] = controller["drivenMaterials"]; return copy; }));
                var donorSignatures = new HashSet<string>(nativeDonor == null ? new string[0] : snapshots(nativeDonor).SelectMany(value => animationsOf((JObject)value["state"])).Select(signature));
                double appearedAt = a.AppearEnded.HasValue ? (a.AppearEnded.Value - a.Spawned).TotalSeconds : double.MaxValue;
                double lifeEnd = a.LifetimeEnd.HasValue ? (a.LifetimeEnd.Value - a.Spawned).TotalSeconds : double.MaxValue;
                int inLife = 0, consistent = 0, valueChecks = 0;
                var findings = new JArray();
                foreach (JObject snapshot in snapshots(a).Where(value => (double)value["t"] >= appearedAt + 1 && (double)value["t"] < lifeEnd - .5))
                {
                    inLife++;
                    var state = (JObject)snapshot["state"];
                    var dissolveOf = new Dictionary<long, double>();
                    foreach (JObject material in (state["skins"] as JArray ?? new JArray()).OfType<JObject>().SelectMany(skin => (skin["materials"] as JArray ?? new JArray()).OfType<JObject>()))
                        if (material["instance"] != null && material["properties"] != null && material["properties"]["_Dissolve"] != null && material["properties"]["_Dissolve"].Type != JTokenType.Null)
                            dissolveOf[(long)material["instance"]] = (double)material["properties"]["_Dissolve"];
                    bool ok = true;
                    foreach (JObject animation in animationsOf(state))
                    {
                        string name = signature(animation);
                        if (!donorSignatures.Contains(name) || (bool)animation["DissolveCutout"]) { ok = false; findings.Add(new JObject { ["t"] = snapshot["t"], ["unmatched"] = name }); }
                        if ((bool)animation["IsStarted"] && !(bool)animation["IsDelayed"])
                            foreach (long driven in (animation["driven"] as JArray ?? new JArray()).Values<long>().Where(dissolveOf.ContainsKey))
                            {
                                valueChecks++;
                                if (Math.Abs(dissolveOf[driven] - (double)animation["CurrentDissolve"]) > .001)
                                { ok = false; findings.Add(new JObject { ["t"] = snapshot["t"], ["material"] = driven, ["material_Dissolve"] = dissolveOf[driven], ["animation"] = animation["CurrentDissolve"] }); }
                            }
                    }
                    if (ok) consistent++;
                }
                row["loopAttribution"] = new JObject { ["donorSignatures"] = new JArray(donorSignatures),
                    ["summonSignatures"] = new JArray(snapshots(a).SelectMany(value => animationsOf((JObject)value["state"])).Select(signature).Distinct()),
                    ["snapshotsInLife"] = inLife, ["consistent"] = consistent, ["valueChecks"] = valueChecks, ["findings"] = findings };
                Sprint25Check("s7-ghaele-" + scene + "-dissolve-attribution", nativeDonor != null && donorSignatures.Count > 0 && inLife >= 5 && consistent == inLife && valueChecks >= inLife,
                    (JObject)row["loopAttribution"].DeepClone(),
                    "Through the Ghaele's life every native dissolve animation on it is non-cutout and has a signature the native donor (spawned, never summoned) also runs, and every driven material's _Dissolve equals that animation's own current value: the rising _Dissolve is the native looping edge animation, not the summon's appearance or lifetime.");
                bool secondVisible = second != null && second.AppearEnded.HasValue && second.MaxBodyDissolveSettled >= 0f && second.MaxBodyDissolveSettled <= .02f;
                bool isolated = row["materialsDistinct"] != null && (bool)row["materialsDistinct"] && (bool)row["donorMaterialsDistinct"];
                S7LifeSubject donorSubject = subjects.FirstOrDefault(value => value.Label == "native-donor");
                bool donorClean = donorSubject == null || donorSubject.Samples.All(value => (float)value["bodyCutout"] <= .02f || !(bool)value["alive"]);
                Sprint25Check("s7-ghaele-" + scene + "-isolation", secondVisible && isolated && donorClean,
                    new JObject { ["second"] = second == null ? null : rows[rows.Count - 1].DeepClone(), ["materialsDistinct"] = row["materialsDistinct"],
                        ["donorMaterialsDistinct"] = row["donorMaterialsDistinct"], ["donorClean"] = donorClean },
                    "A second Ghaele summoned during the first's life has no cutout dissolve through its own life; the two summons and the native donor hold distinct material instances, and the donor's body shows no cutout dissolve.");
            }
            finally
            {
                EventBus.Unsubscribe(attacks);
                Game.Instance.IsPaused = wasPaused;
                if (donor != null && !donor.Destroyed) CleanupExpandedSummoningUnit(donor);
            }
        }

        private IEnumerable<int> ReviewSprint7Ghaele(ExpandedSummoningCorrectionFixture fixture, string phase, Cleanup2NativeDefaultScope mode)
        {
            bool outdoor = phase == ExpandedSummoningNativeProfilePolicy.Sprint39S7GhaelePhase;
            if (!IsSprint32Review || !_request.ExitAfterCompletion || !ReferenceEquals(fixture, _sprint25Fixture) ||
                fixture?.Scene == null || fixture.Caster == null || !_context.FeatureModules.Active.ExpandedSummoning ||
                (!outdoor && phase != ExpandedSummoningNativeProfilePolicy.Sprint39S7GhaeleInteriorPhase) ||
                (outdoor && (fixture.Hostile == null || mode == null || !mode.Current || !Cleanup2NoNarrativeOverlay())))
                throw new InvalidOperationException("Exact fresh Sprint 7 Ghaele fixture with the summoning module enabled required.");
            string scene = outdoor ? "bright" : "dim";
            var record = new JObject { ["phase"] = phase, ["runId"] = _request.RunId, ["scene"] = scene,
                ["area"] = Game.Instance.CurrentlyLoadedArea?.name,
                ["limits"] = "Published summon cast as a native UnitUseAbility that progresses through the caster's command queue, caster level 20 only, appearance kept, unpaused native time; brains inactive and moves and attacks issued as a player would; attacks witnessed by their native weapon-attack rules. Lifetime is native game time; images are presentation evidence only." };
            var owned = new List<UnitEntityData>();
            Vector3 centre = fixture.Caster.Position;
            UnitEntityData hostile = outdoor ? fixture.Hostile : null;
            Vector3? hostileHome = hostile?.Position;
            int? hostileHitPoints = hostile == null ? (int?)null : hostile.Descriptor.Stats.HitPoints.BaseValue;
            if (hostile != null) hostile.Descriptor.Stats.HitPoints.BaseValue = 100000;
            int priorCapture = Time.captureFramerate;
            try
            {
                if (hostile != null)
                {
                    PlaceExpandedSummoningUnit(hostile, ObstacleAnalyzer.GetNearestNode(centre + Vector3.forward * 4f).clampedPosition);
                    foreach (int step in R6Wait(10)) yield return step;
                }
                foreach (int step in R6Cell("s7-ghaele-" + scene, () => S7GhaeleCell(fixture, scene, hostile, owned, record))) yield return step;
            }
            finally
            {
                Time.captureFramerate = priorCapture;
                var errors = new JArray();
                Action<string, Action> restore = (name, action) => { try { action(); } catch (Exception error) { errors.Add(name + ":" + error.Message); } };
                restore("combat", () => {
                    foreach (UnitEntityData unit in owned.Concat(new[] { fixture.Hostile, fixture.Caster }).Where(value => value != null && !value.Destroyed))
                    {
                        unit.Commands.InterruptAll(command => true); unit.Commands.RemoveFinishedAndUpdateQueue();
                        if (unit.CombatState.IsInCombat) unit.CombatState.LeaveCombat();
                    }
                });
                restore("hostile", () => { if (hostileHome.HasValue && !fixture.Hostile.Destroyed) { fixture.Hostile.Descriptor.Stats.HitPoints.BaseValue = hostileHitPoints.Value; fixture.Hostile.Descriptor.Damage = 0; PlaceExpandedSummoningUnit(fixture.Hostile, hostileHome.Value); } });
                PlaceExpandedSummoningUnit(fixture.Caster, centre);
                DisposeExpandedSummoningUnits(fixture.Created, owned.Where(value => !value.Destroyed).ToArray());
                Sprint25Check("s7-ghaele-owned-state-restored", errors.Count == 0, new JObject { ["errors"] = errors },
                    "Commands, combat and positions restore; expired summons were removed natively.");
                record["restoreErrors"] = errors;
                RuntimeTestResultWriter.WriteAtomic(Path.Combine(_request.EvidenceDirectory, phase + ".json"), record.ToString(Formatting.Indented));
            }
        }

        // ---------------------------------------------------------------
        // T4: owned Trample/Overrun description windows and controls
        // ---------------------------------------------------------------

        private JObject S7WindowCard()
        {
            var window = Game.Instance.UI.DescriptionController.DescWindow;
            var body = window.Body;
            var header = R3Bricks(body == null ? null : body.HeaderBox);
            var content = R3Bricks(body == null ? null : body.ContentBox);
            Func<DescriptionBrick, string[]> texts = brick => (brick.TextFields ?? new List<TMPro.TextMeshProUGUI>())
                .Where(text => text != null && text.isActiveAndEnabled).Select(text => (text.GetParsedText() ?? "").Trim()).ToArray();
            return new JObject { ["headerTexts"] = new JArray(header.SelectMany(texts)), ["contentTexts"] = new JArray(content.SelectMany(texts).Take(60)) };
        }

        // Opens one window the observer owns (the exact TooltipData it built),
        // captures and reads it, and closes only that window. Any foreign
        // window, hover or modal, or a loss of ownership, stops the phase.
        private IEnumerable<int> S7OpenWindow(Cleanup2NativeDefaultScope mode, NativeIconScreenEvidence screens, string label, Ability fact,
            bool hudPath, JObject result, Action<TooltipData> opened)
        {
            var ui = Game.Instance.UI; var window = ui.DescriptionController.DescWindow;
            if (!Sprint7ObserverPolicy.CanOpenWindow(window.gameObject.activeInHierarchy, ui.TooltipsController.ContextTooltipData != null, R3UnrelatedModal()))
                throw new InvalidOperationException("A foreign description window, hover or modal is open before " + label + "; the observer does not touch it.");
            // The fact path is the character-sheet/description window; the
            // HUD path is the action bar slot's AbilityData.
            var data = hudPath ? new TooltipData { Type = TooltipType.Ability, AbilityData = new AbilityData(fact) }
                : new TooltipData { Type = TooltipType.Ability, Ability = fact };
            opened(data);
            ui.DescriptionController.HandleOpenDescription(data);
            Ability factAtOpen = fact;
            Func<bool> owns = () => ReferenceEquals(fact.Owner.Abilities.GetAbility(fact.Blueprint), factAtOpen) &&
                ExpandedSummoningNativeProfilePolicy.SummonPlaytestR3SurfaceOwned(mode.Current, Cleanup2NoNarrativeOverlay(),
                R3UnrelatedModal(), window != null && window.gameObject.activeInHierarchy && data.Type == TooltipType.Ability &&
                Sprint7ObserverPolicy.OwnsWindow(window.Data, data) && fact.Owner != null && !fact.Owner.Unit.Destroyed && ui.TooltipsController.ContextTooltipData == null);
            for (int frame = 0; frame < 600 && !owns(); frame++) yield return 0;
            if (!owns()) throw new InvalidOperationException("Exact owned description window did not become visible: " + label);
            for (int frame = 0; frame < 20; frame++) { yield return 0; if (!owns()) throw new InvalidOperationException("Description ownership lost: " + label); }
            JObject card = S7WindowCard();
            foreach (int frame in screens.Capture("s7-description-" + label, card, owns)) yield return frame;
            if (!owns()) throw new InvalidOperationException("Description ownership lost during capture: " + label);
            card = S7WindowCard();
            card["path"] = hudPath ? "AbilityData (HUD action bar)" : "Ability fact (description window)";
            card["blueprint"] = fact.Blueprint.AssetGuid; card["owner"] = fact.Owner.Blueprint.AssetGuid;
            card["nativeTarget"] = fact.Blueprint.GetTarget();
            result[label] = card;
            ui.DescriptionController.HandleCloseDescriptionWindow(window);
            for (int frame = 0; frame < 120 && window.gameObject.activeInHierarchy; frame++) yield return 0;
            if (window.gameObject.activeInHierarchy) throw new InvalidOperationException("Native description window did not close.");
            opened(null);
        }

        private IEnumerable<int> ReviewSprint7Ui(ExpandedSummoningCorrectionFixture fixture, string phase, Cleanup2NativeDefaultScope mode)
        {
            if (!IsSprint32Review || !_request.ExitAfterCompletion || !ReferenceEquals(fixture, _sprint25Fixture) ||
                fixture?.Scene == null || fixture.Caster == null || mode == null || !mode.Current ||
                !_context.FeatureModules.Active.ExpandedSummoning || phase != ExpandedSummoningNativeProfilePolicy.Sprint39S7UiPhase ||
                !Cleanup2NoNarrativeOverlay())
                throw new InvalidOperationException("Exact fresh Sprint 7 UI fixture in owned outdoor native Default required.");
            var record = new JObject { ["phase"] = phase, ["runId"] = _request.RunId,
                ["limits"] = "Desktop description windows (fact path) and the HUD AbilityData path, rendered and read back. The controller UI was not rendered: no admitted observer reaches a summoned creature's ability in controller mode (record S7 T4)." };
            IDictionary<string, BlueprintScriptableObject> symbols = ExpandedSummoningBlueprints.Registered.BySymbol;
            var owned = new List<UnitEntityData>();
            var added = new List<KeyValuePair<UnitEntityData, Ability>>();
            Vector3 centre = fixture.Caster.Position;
            var windows = new JObject(); record["windows"] = windows;
            TooltipData openedData = null;
            NativeIconScreenEvidence screens = null;
            try
            {
                screens = new NativeIconScreenEvidence(_request);
                UnitEntityData brachiosaurus = CastExpandedSummoningOwnTier(fixture, "brachiosaurus"); owned.Add(brachiosaurus);
                UnitEntityData mastodon = CastExpandedSummoningOwnTier(fixture, "mastodon"); owned.Add(mastodon);
                foreach (UnitEntityData unit in owned)
                {
                    Sprint28SetBrainActivePreservingActions(unit, false);
                    unit.Commands.InterruptAll(command => true); unit.Commands.RemoveFinishedAndUpdateQueue();
                }
                PlaceExpandedSummoningUnit(brachiosaurus, ObstacleAnalyzer.GetNearestNode(centre + Vector3.forward * 6f).clampedPosition);
                PlaceExpandedSummoningUnit(mastodon, ObstacleAnalyzer.GetNearestNode(centre + Vector3.left * 6f).clampedPosition);
                foreach (int step in R6Wait(R6ArrivalFrames)) yield return step;
                Func<UnitEntityData, string, Ability> fact = (unit, symbol) =>
                    unit.Descriptor.Abilities.Enumerable.Single(value => ReferenceEquals(value.Blueprint, symbols[symbol]));
                // Controls, granted for the observation only and removed only if
                // this observer added them: the native overrun the owned actions
                // are cloned from (same DoubleMove point template, not owned) and
                // an unrelated native spell.
                // An existing control is used as it is; only a fact this
                // observer granted is retained, by instance, for release.
                Func<UnitEntityData, string, Ability> control = (unit, guid) =>
                {
                    var blueprint = ResourcesLibrary.TryGetBlueprint<BlueprintAbility>(guid);
                    if (blueprint == null) throw new InvalidOperationException("Native control ability missing: " + guid);
                    Ability existing = unit.Descriptor.Abilities.GetAbility(blueprint);
                    if (!Sprint7ObserverPolicy.ShouldGrant(existing != null)) return existing;
                    var granted = unit.Descriptor.AddFact(blueprint) as Ability;
                    if (granted == null) throw new InvalidOperationException("Native control ability was not granted: " + guid);
                    added.Add(new KeyValuePair<UnitEntityData, Ability>(unit, granted));
                    return granted;
                };
                Ability nativeFact = control(brachiosaurus, "1a3b471ecea51f7439a946b23577fd70");
                Ability spellFact = control(fixture.Caster, "4ac47ddb9fa1eaf43a1b6809980cfbd2");
                var targets = new List<KeyValuePair<string, Ability>> {
                    new KeyValuePair<string, Ability>("brachiosaurus-overrun", fact(brachiosaurus, "KMG.Summoning.Special.Brachiosaurus.Overrun")),
                    new KeyValuePair<string, Ability>("brachiosaurus-trample", fact(brachiosaurus, "KMG.Summoning.Special.Brachiosaurus.Trample")),
                    new KeyValuePair<string, Ability>("mastodon-trample", fact(mastodon, "KMG.Summoning.Special.Mastodon.Trample")),
                    new KeyValuePair<string, Ability>("control-native-overrun", nativeFact),
                    new KeyValuePair<string, Ability>("control-native-spell", spellFact) };
                // No per-window catch: an unexpected window, modal or ownership
                // loss ends the phase as a failure.
                foreach (var target in targets)
                    foreach (bool hud in new[] { false, true })
                        foreach (int step in S7OpenWindow(mode, screens, target.Key + (hud ? "-hud" : ""), target.Value, hud, windows, value => openedData = value)) yield return step;
                string range = SummonActionTooltipPolicy.MovementTargetRange;
                Func<JToken, IEnumerable<string>> all = card => card == null ? new string[0] :
                    ((JArray)card["headerTexts"]).Values<string>().Concat(((JArray)card["contentTexts"]).Values<string>());
                foreach (string label in new[] { "brachiosaurus-overrun", "brachiosaurus-trample", "mastodon-trample" })
                    foreach (string suffix in new[] { "", "-hud" })
                    {
                        JToken card = windows[label + suffix];
                        var texts = all(card).ToArray();
                        bool truthful = texts.Any(value => value.Contains(range)) && !texts.Any(value => value.Contains("0 ft"));
                        bool noSchool = card != null && !((JArray)card["headerTexts"]).Values<string>().Any(value => value == "None");
                        Sprint25Check("s7-ui-" + label + suffix, card != null && truthful && noSchool, card == null ? new JObject() : (JObject)card.DeepClone(),
                            "The rendered owned window states a point target within twice the creature's speed, shows no 0 ft placeholder and no None school.");
                    }
                foreach (string suffix in new[] { "", "-hud" })
                {
                    JToken native = windows["control-native-overrun" + suffix];
                    JToken spell = windows["control-native-spell" + suffix];
                    bool nativeKept = native != null && !all(native).Any(value => value.Contains(range)) &&
                        all(native).Any(value => value.Contains(((string)native["nativeTarget"] ?? "\u0000").Trim()));
                    bool spellKept = spell != null && !all(spell).Any(value => value.Contains(range)) &&
                        ((JArray)spell["headerTexts"]).Values<string>().Any(value => value.IndexOf("Evocation", StringComparison.OrdinalIgnoreCase) >= 0);
                    Sprint25Check("s7-ui-controls" + suffix, nativeKept && spellKept, new JObject { ["nativeOverrun"] = native == null ? null : native.DeepClone(), ["spell"] = spell == null ? null : spell.DeepClone() },
                        "The native overrun keeps its native target text and the unrelated native spell keeps its school and target.");
                }
            }
            finally
            {
                var errors = new JArray();
                Action<string, Action> restore = (name, action) => { try { action(); } catch (Exception error) { errors.Add(name + ":" + error.Message); } };
                // Only the window this observer opened, and only facts it added.
                restore("window", () => { var window = Game.Instance.UI.DescriptionController.DescWindow;
                    if (window != null && window.gameObject.activeInHierarchy && Sprint7ObserverPolicy.OwnsWindow(window.Data, openedData))
                        Game.Instance.UI.DescriptionController.HandleCloseDescriptionWindow(window); });
                restore("facts", () => { foreach (var entry in added)
                    if (!entry.Key.Destroyed && Sprint7ObserverPolicy.ShouldRelease(entry.Value, entry.Key.Descriptor.Abilities.GetAbility(entry.Value.Blueprint)))
                        entry.Key.Descriptor.Abilities.RemoveFact(entry.Value); });
                restore("screens", () => { if (screens != null) screens.Dispose(); });
                PlaceExpandedSummoningUnit(fixture.Caster, centre);
                DisposeExpandedSummoningUnits(fixture.Created, owned.Where(value => !value.Destroyed).ToArray());
                Sprint25Check("s7-ui-owned-state-restored", errors.Count == 0, new JObject { ["errors"] = errors, ["addedFacts"] = added.Count },
                    "The observer's own window, the control facts it added and its summons restore; nothing it did not create is closed or removed.");
                record["restoreErrors"] = errors;
                RuntimeTestResultWriter.WriteAtomic(Path.Combine(_request.EvidenceDirectory, phase + ".json"), record.ToString(Formatting.Indented));
            }
        }

        private sealed class S7AttackLog : IGlobalRulebookHandler<RuleAttackWithWeapon>
        {
            internal readonly JArray Rows = new JArray();
            internal readonly List<RuleAttackWithWeapon> Events = new List<RuleAttackWithWeapon>();
            public void OnEventAboutToTrigger(RuleAttackWithWeapon evt) { }
            public void OnEventDidTrigger(RuleAttackWithWeapon evt)
            {
                if (evt == null || evt.Initiator == null) return;
                Events.Add(evt);
                Rows.Add(new JObject { ["initiator"] = evt.Initiator.UniqueId, ["target"] = evt.Target == null ? null : evt.Target.UniqueId,
                    ["weapon"] = evt.Weapon == null ? null : evt.Weapon.Blueprint.name, ["weaponGuid"] = evt.Weapon == null ? null : evt.Weapon.Blueprint.AssetGuid,
                    ["roll"] = evt.AttackRoll == null ? 0 : System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(evt.AttackRoll),
                    ["hit"] = evt.AttackRoll != null && evt.AttackRoll.IsHit, ["utc"] = DateTime.UtcNow.ToString("o"), ["frame"] = Time.frameCount,
                    // R1-F1: listener (game camera) to emitter distance in the
                    // resolving frame, the frame the impact cue is posted in.
                    ["listenerDistanceAtEvent"] = S7ListenerDistance(evt.Initiator), ["emitterPosition"] = S7Vector(evt.Initiator.Position) });
            }
        }

        private static SummonCastCueRecord[] S7ImpactCues(UnitEntityData unit)
        {
            return SummonAbilityCastCueRuntime.Snapshot().Where(value => value.CasterId == unit.UniqueId &&
                value.Ability == AnkylosaurusTailImpactPolicy.ImpactCueLabel).ToArray();
        }

        // The Wwise listener follows the game camera.
        private static double S7ListenerDistance(UnitEntityData emitter)
        {
            CameraRig rig = TeleportationCastingCamera();
            Camera camera = rig == null ? Camera.main : rig.Camera;
            return camera == null || emitter == null ? -1d : Math.Round(Vector3.Distance(camera.transform.position, emitter.Position), 2);
        }

        // R4-F5: one native playback query through the installed contract.
        private static string S7Playback(SummonCastCueRecord record)
        {
            if (record == null) return Sprint7ObserverPolicy.PlaybackUnknown;
            bool initialized = false;
            try
            {
                initialized = AkSoundEngine.IsInitialized();
                uint eventId = initialized && record.PlayingId != 0 ? AkSoundEngine.GetEventIDFromPlayingID(record.PlayingId) : 0;
                return Sprint7ObserverPolicy.PlaybackState(initialized, record.PlayingId, eventId, false);
            }
            catch (Exception) { return Sprint7ObserverPolicy.PlaybackState(initialized, record.PlayingId, 0, true); }
        }

        // R4-F5 (observer only, synthetic): the product's own end-of-event callback
        // receiver skips one exact cue record, so the product never learns of that
        // playback's end; every other record's callback passes through. Native
        // playback is untouched and read separately.
        private sealed class S7CallbackWithhold : IDisposable
        {
            private const string Id = "kmg.s7.callback-withhold";
            // Recovery C3 (R1-F6): the exact texts of this observer's injected faults (operation and identity in one string).
            internal const string UnpatchFault = "injected failure of the cue callback unpatch (" + Id + ")";
            internal const string SetupFault = "injected failure after the cue callback hook was installed (" + Id + ")";
            internal const string RollbackFault = "injected failure of the cue callback rollback unpatch (" + Id + ")";
            private bool patchAttempted;
            private static S7CallbackWithhold active;
            private static readonly System.Reflection.MethodInfo Receiver = typeof(SummonAbilityCastCueRuntime).GetMethod("OnEnded",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            private readonly Harmony12.HarmonyInstance harmony;
            internal SummonCastCueRecord Target;
            internal readonly List<DateTime> Withheld = new List<DateTime>();
            internal int PassedThrough;

            internal S7CallbackWithhold()
            {
                if (active != null || Receiver == null) throw new InvalidOperationException("Cue callback receiver unavailable.");
                harmony = Harmony12.HarmonyInstance.Create(Id);
                LastSetup = null;
                try
                {
                    // R1-F5: the attempt is recorded before the patch can partly succeed and throw.
                    patchAttempted = true;
                    harmony.Patch(Receiver, new Harmony12.HarmonyMethod(typeof(S7CallbackWithhold).GetMethod("Before",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)) { prioritiy = Harmony12.Priority.First });
                    // Recovery C3 (R7-F3 control only): the set-up fails after its hook was installed.
                    if (Inject == "fail-setup-and-rollback") throw new InvalidOperationException(SetupFault);
                    active = this;
                }
                catch (Exception original)
                {
                    // R1-F5: the rollback runs whatever the counts read; an unreadable count is unknown, never zero.
                    int? installed, remaining;
                    List<string> failures = Sprint7ObserverPolicy.RollbackSetup(Hooks, Unhook, out installed, out remaining);
                    LastSetup = new JObject { ["hooksBeforeRollback"] = installed, ["hooksAfterRollback"] = remaining, ["rollbackFailures"] = new JArray(failures.ToArray()),
                        ["original"] = original.GetType().Name + ": " + original.Message };
                    throw Sprint7ObserverPolicy.SetupFailure("S7CallbackWithhold", original, failures, remaining);
                }
            }

            private static bool Before(object __0)
            {
                S7CallbackWithhold owner = active;
                if (owner == null) return true;
                if (owner.Target == null || !ReferenceEquals(__0, owner.Target)) { owner.PassedThrough++; return true; }
                owner.Withheld.Add(DateTime.UtcNow);
                return false;
            }

            internal static int Hooks() { return S7ProjectileTrace.HookCount(Receiver, Id); }

            /// <summary>Whether the receiver is in the patched-method listing; null when the listing cannot be read.</summary>
            private static bool? Listed()
            {
                try { return Receiver != null && Harmony12.HarmonyInstance.Create(Id).GetPatchedMethods().Contains(Receiver); }
                catch (Exception) { return null; }
            }

            /// <summary>Recovery C3 (R7-F3): what the last failed set-up installed, rolled back and left.</summary>
            internal static JObject LastSetup;

            // R6-F2 (abandonment check only): the unpatch of this observer's hook fails while set; the hook stays for the
            // outer driver's own repair after the failure has been recorded.
            internal static string Inject;

            private List<string> Unhook()
            {
                // R1-F5: the attempted hook is released by its owner identity, independently of any count.
                var releases = new List<KeyValuePair<string, Action>>();
                if (patchAttempted) releases.Add(new KeyValuePair<string, Action>("cue callback hook", () =>
                {
                    if (Inject == "fail-unpatch") throw new InvalidOperationException(UnpatchFault);
                    if (Inject == "fail-setup-and-rollback") throw new InvalidOperationException(RollbackFault);
                    harmony.Unpatch(Receiver, Harmony12.HarmonyPatchType.All, Id);
                }));
                return Sprint7ObserverPolicy.RunReleases(releases);
            }

            /// <summary>The outer driver's repair of its own injected intervention: removes this observer's hook by its id.</summary>
            internal static List<string> Repair()
            {
                // R1-F5: attempted when the receiver is listed or the listing is unreadable; no count gates it.
                var releases = new List<KeyValuePair<string, Action>>();
                if (Listed() != false) releases.Add(new KeyValuePair<string, Action>("cue callback hook repair",
                    () => Harmony12.HarmonyInstance.Create(Id).Unpatch(Receiver, Harmony12.HarmonyPatchType.All, Id)));
                return Sprint7ObserverPolicy.RunReleases(releases);
            }

            internal List<string> DisposeFailures = new List<string>();

            public void Dispose()
            {
                DisposeFailures = Unhook();
                if (ReferenceEquals(active, this)) active = null;
            }
        }

        // R4-F5 bounded characterization (synthetic callback loss). Two fresh
        // sources (the cell's first ones can reach their 120 s expiry) post
        // overlapping cues; once both playing IDs are confirmed playing, the
        // product's callback for the survivor's record is withheld from the
        // product. The other source is retired; through a window declared here
        // the survivor's native playback, callback and tracking are read
        // separately; then the survivor's own source is retired and the actual
        // destruction of its view and playback component is observed.
        // Owner amendment 3 (R6-F2): the observer registers with the enclosing
        // phase's isolation before it acquires anything, and its finally
        // publishes the hook's cleanup there (a failed unpatch or a remaining
        // hook is a named phase failure) after every independent release has
        // been attempted; the verdict (R6-F3/R6-F4) is the one settled
        // evaluator over the row this collector emits. With an abandon probe
        // the observer holds right after acquiring its hook until its enclosing
        // runner is disposed.
        private IEnumerable<int> S7CallbackLoss(ExpandedSummoningCorrectionFixture fixture, Func<string, UnitEntityData> summon,
            UnitEntityData hostile, JObject cell, Action<string> mark, Func<Vector3, Vector3> ground, Sprint7CellIsolation isolation, JObject abandonProbe = null)
        {
            const string Owner = "callback-withhold";
            const double WindowSeconds = 12, DisposalSeconds = 6, OverlapSeconds = 16;
            // Recovery C1 (R2-F6): the row header names the entry point this row's verdict comes from (CallbackLossRound7).
            JObject row = Sprint7ObserverPolicy.NewCallbackLossRow(WindowSeconds, DisposalSeconds, OverlapSeconds);
            cell["callbackLoss"] = row;
            isolation.Opened(Owner);
            UnitEntityData keeper = null, other = null;
            // Recovery C3 (R7-F4): each cue source receives at most one retirement request; a failed request is a named failure
            // of this observer, and a request whose result is not read settled stays a residual.
            // C3 review R1-F1: the phase's one retirement record and retire action, shared with the phase's and fixture's teardown.
            Sprint7OwnedRequests<UnitEntityData> sources = _s7Retirements ?? new Sprint7OwnedRequests<UnitEntityData>();
            Action<UnitEntityData> retire = _s7Retire ?? (unit => DisposeExpandedSummoningUnits(fixture.Created, new[] { unit }));
            SummonCastCueRecord kept = null, retired = null;
            DateTime retiredAt = DateTime.MinValue;
            S7CallbackWithhold withhold = null;
            // Recovery C3 (R7-F3): the hook attempt is registered before its set-up can partly succeed and throw.
            var hook = new Sprint7HookAttempt<S7CallbackWithhold>("cue callback", S7CallbackWithhold.Hooks);
            var window = new JArray(); row["window"] = window;
            mark("anky-loss-start");
            try
            {
                keeper = summon("ankylosaurus"); other = summon("ankylosaurus");
                PlaceExpandedSummoningUnit(keeper, ground(hostile.Position + Vector3.left * 3f));
                PlaceExpandedSummoningUnit(other, ground(hostile.Position + Vector3.right * 3f));
                foreach (int step in R6Seconds(2f)) yield return step;
                int keeperFrom = S7ImpactCues(keeper).Length, otherFrom = S7ImpactCues(other).Length;
                withhold = hook.Acquire(() => new S7CallbackWithhold());
                if (abandonProbe != null)
                {
                    abandonProbe["hooksAcquired"] = S7CallbackWithhold.Hooks();
                    abandonProbe["sources"] = new JArray(keeper.UniqueId, other.UniqueId);
                    while (abandonProbe != null) yield return 0;
                }
                // Recovery C3: the two-source overlap is reached by declared scheduling of valid producer events (constructed
                // setup, not ordinary play): at most three scheduled deliveries, two seconds apart; each delivers, in one frame,
                // a fresh resolved tail hit of each source on the hostile to that source's own production handler, which posts
                // its real cue (a native playing ID). The latest cue of each source that the native query then confirms
                // playing is selected; the callback is withheld only once both are confirmed playing at the same time.
                var schedule = new JArray(); row["overlapSchedule"] = schedule;
                row["overlapSetup"] = "constructed: declared same-frame delivery of fresh resolved tail hits (attack roll result set to Hit) to each source's own AnkylosaurusTailControl; cues are the production handler's real postings";
                for (int delivery = 0; delivery < 3 && kept == null; delivery++)
                {
                    if (delivery > 0) foreach (int step in R6Seconds(2f)) yield return step;
                    hostile.Descriptor.Damage = 0;
                    int frame = Time.frameCount;
                    int keeperCalls = S7ReplayTail(keeper, S7FreshTailHit(keeper, hostile));
                    int otherCalls = S7ReplayTail(other, S7FreshTailHit(other, hostile));
                    var entry = new JObject { ["delivery"] = delivery, ["frame"] = frame, ["sameFrame"] = Time.frameCount == frame, ["keeperCalls"] = keeperCalls, ["otherCalls"] = otherCalls };
                    schedule.Add(entry);
                    DateTime confirmBy = DateTime.UtcNow.AddSeconds(1);
                    while (DateTime.UtcNow < confirmBy)
                    {
                        SummonCastCueRecord keptLive = S7ImpactCues(keeper).Skip(keeperFrom).LastOrDefault(value => S7Playback(value) == Sprint7ObserverPolicy.PlaybackPlaying);
                        SummonCastCueRecord otherLive = S7ImpactCues(other).Skip(otherFrom).LastOrDefault(value => S7Playback(value) == Sprint7ObserverPolicy.PlaybackPlaying);
                        if (keptLive != null && otherLive != null) { kept = keptLive; retired = otherLive; withhold.Target = kept; break; }
                        yield return 0;
                    }
                    entry["bothPlaying"] = kept != null;
                    entry["keeperPlayingId"] = kept == null ? null : (JToken)kept.PlayingId;
                    entry["otherPlayingId"] = retired == null ? null : (JToken)retired.PlayingId;
                }
                // Observation control (unchanged from the qualified run): the two sources this observer summoned for the overlap are
                // stilled before selection so no further attack adds cues. This is not teardown; the teardown interrupts nothing.
                foreach (UnitEntityData attacker in new[] { keeper, other })
                { attacker.Commands.InterruptAll(command => true); attacker.Commands.RemoveFinishedAndUpdateQueue(); }
                row["cuesBeforeSelection"] = new JObject { ["survivorSource"] = new JArray(S7ImpactCues(keeper).Skip(keeperFrom).Select(S7Cue)),
                    ["otherSource"] = new JArray(S7ImpactCues(other).Skip(otherFrom).Select(S7Cue)) };
                row["before"] = new JObject { ["survivor"] = kept == null ? null : S7Cue(kept), ["survivorPlayback"] = S7Playback(kept),
                    ["other"] = retired == null ? null : S7Cue(retired), ["otherPlayback"] = S7Playback(retired), ["frame"] = Time.frameCount };
                if (kept != null && retired != null)
                {
                    retiredAt = DateTime.UtcNow;
                    row["otherRetiredUtc"] = retiredAt.ToString("o");
                    mark("anky-loss-retire-other");
                    sources.Request(other, "other source " + other.UniqueId, retire);
                    // Same frame as the call: the product's stop runs later, in the retired view's own destroy callback.
                    row["afterOtherRetired"] = new JObject { ["other"] = S7Cue(retired), ["otherStopAttempts"] = retired.StopAttempts, ["survivorStopAttempts"] = kept.StopAttempts,
                        ["note"] = "read in the retirement frame, before the product's stop runs; the verdict reads the settled values at the window end" };
                    DateTime windowEnd = retiredAt.AddSeconds(WindowSeconds), next = DateTime.UtcNow;
                    bool firstInactive = false;
                    while (DateTime.UtcNow < windowEnd)
                    {
                        if (DateTime.UtcNow >= next)
                        {
                            next = DateTime.UtcNow.AddSeconds(.5);
                            string playback = S7Playback(kept);
                            if (playback == Sprint7ObserverPolicy.PlaybackInactive && !firstInactive) { firstInactive = true; row["survivorFirstInactiveUtc"] = DateTime.UtcNow.ToString("o"); }
                            window.Add(new JObject { ["utc"] = DateTime.UtcNow.ToString("o"), ["frame"] = Time.frameCount, ["survivorPlayback"] = playback,
                                ["survivorEndUtc"] = kept.EndUtc.HasValue ? kept.EndUtc.Value.ToString("o") : null, ["survivorTracked"] = S7CueTracked(keeper, kept),
                                ["callbacksWithheld"] = withhold.Withheld.Count, ["otherPlayback"] = S7Playback(retired) });
                        }
                        yield return 0;
                    }
                    // The other source's retirement read once settled: its own cue record with its stop attempts.
                    JObject otherSettled = S7Cue(retired); otherSettled["stopAttempts"] = retired.StopAttempts;
                    row["otherRetirementSettled"] = new JObject { ["other"] = otherSettled, ["survivorStopAttemptsThroughWindow"] = kept.StopAttempts };
                    row["survivorAtWindowEnd"] = new JObject { ["cue"] = S7Cue(kept), ["playback"] = S7Playback(kept), ["tracked"] = S7CueTracked(keeper, kept),
                        ["callbacksWithheld"] = new JArray(withhold.Withheld.Select(value => value.ToString("o"))), ["callbacksPassedThrough"] = withhold.PassedThrough };
                    // The survivor's own source retires; its view and playback component must actually be destroyed.
                    UnitEntityView view = keeper.View;
                    SummonCastCuePlayback component = view == null ? null : view.GetComponent<SummonCastCuePlayback>();
                    row["componentBeforeDisposal"] = component != null;
                    var others = SummonAbilityCastCueRuntime.Snapshot().Where(value => !ReferenceEquals(value, kept)).ToDictionary(value => value, value => value.StopAttempts);
                    mark("anky-loss-retire-survivor");
                    sources.Request(keeper, "survivor source " + keeper.UniqueId, retire);
                    DateTime disposalEnd = DateTime.UtcNow.AddSeconds(DisposalSeconds);
                    while (DateTime.UtcNow < disposalEnd && !(view == null && component == null)) yield return 0;
                    SummonCastCueRecord[] now = SummonAbilityCastCueRuntime.Snapshot();
                    row["survivorAfterDisposal"] = new JObject { ["cue"] = S7Cue(kept), ["stopAttempts"] = kept.StopAttempts, ["playback"] = S7Playback(kept),
                        ["viewDestroyed"] = view == null, ["playbackComponentDestroyed"] = component == null,
                        ["unrelatedRecordsChanged"] = others.Count(entry => now.Contains(entry.Key) && entry.Key.StopAttempts != entry.Value),
                        ["note"] = "a stop request issued by the source's own retirement for an already inactive playing ID is a request, not completion; EndUtc stays unset" };
                }
            }
            finally
            {
                // Every release is attempted; the hook's outcome and every other failure go to the enclosing isolation.
                var releases = new List<KeyValuePair<string, Action>>();
                // R1-F6: each of the hook's release failures stays its own failure (no aggregation).
                if (withhold != null) releases.Add(new KeyValuePair<string, Action>("cue callback hook", () => withhold.Dispose()));
                // C3 review R2-F7: the teardown interrupts no command; the observer's sources are retired below.
                const int commandsInterrupted = 0;
                List<string> failures = Sprint7ObserverPolicy.RunReleases(releases);
                if (withhold != null) isolation.Fail(Owner, withhold.DisposeFailures);
                // R1-F11: sources this observer summoned and did not already request are retired here through the shared
                // operation (one request each; a failure recorded; then failures and settlement published).
                var labelled = new List<KeyValuePair<UnitEntityData, string>>();
                if (keeper != null) labelled.Add(new KeyValuePair<UnitEntityData, string>(keeper, "survivor source " + keeper.UniqueId));
                if (other != null) labelled.Add(new KeyValuePair<UnitEntityData, string>(other, "other source " + other.UniqueId));
                int sourcesRetired = Sprint7ObserverPolicy.ReleaseObserverSources(sources, labelled, retire, isolation, Owner, S7UnitSettled);
                row["releaseFailures"] = new JArray(failures.ToArray());
                row["retirementFailures"] = new JArray(sources.Failures.ToArray());
                row["interventionDisposeFailures"] = new JArray(withhold == null ? new string[0] : withhold.DisposeFailures.ToArray());
                row["independentCleanup"] = new JObject { ["commandsInterrupted"] = commandsInterrupted, ["sourcesRetiredHere"] = sourcesRetired,
                    ["survivorRetirementRequested"] = sources.Requested(keeper), ["otherRetirementRequested"] = sources.Requested(other) };
                isolation.Fail(Owner, failures);
                // The hook attempt's explicit readback, whether or not its set-up returned an observer (R7-F3).
                int? hooksAfter = hook.Publish(isolation, Owner);
                row["hookAttempted"] = hook.Attempted;
                row["interventionHooksAfter"] = hooksAfter;
                row["hookSetupFailure"] = hook.SetupFailure;
                row["hookSetup"] = hook.Attempted && withhold == null && S7CallbackWithhold.LastSetup != null ? S7CallbackWithhold.LastSetup.DeepClone() : null;
                isolation.Reported(Owner);
            }
            mark("anky-loss-end");
            // Recovery C1: the one round-7 entry point, reading the retirement time and settled record from this row itself.
            string verdict = Sprint7ObserverPolicy.CallbackLossRound7(row, SummonCastCueLifecycle.StopRequested);
            row["verdict"] = verdict;
            row["disposition"] = verdict == null
                ? "Established under the synthetic loss: the survivor's playback ended normally (confirmed inactive by the native query, including at the window end and after its own source's disposal), the other source was unaffected, and the only residual was the survivor's tracking on its own source's playback component, which went away when that component was destroyed with the source. Owner amendment 2 section 4 accepts this as a documented development limitation; no product change."
                : "Not established: " + verdict;
            Sprint25Check("s7-anky-callback-loss-characterized", verdict == null, (JObject)row.DeepClone(),
                "Synthetic callback loss (the product's end-of-event callback for one exact survivor record withheld from the product; native playback intact), judged by the one settled evaluator over this row: both playing IDs confirmed playing before retirement; retiring the other source requests a stop for its own ID only (read once settled); the survivor receives no stop; within the declared window the survivor's playback is confirmed inactive by the native query and never playing again, and is inactive at the window end and after its own source's disposal, while the product, never receiving the callback, still tracks it on its own source and records no end; retiring that source destroys its view and playback component (observed, not just requested), no other record is affected, no end is manufactured, and the observer's hook is removed (zero hooks, no failed unpatch).");
        }

        // Recovery C3: the effects phase's cell runner. A cell's exception is a sticky failure of the isolation it reports
        // to; at phase level the cell's completion is also checked, inside a driver's child sequence it is recorded only.
        private Func<string, Func<IEnumerable<int>>, IEnumerable<int>> S7Cells(Sprint7CellIsolation isolation, JObject childRecord = null)
        {
            return (name, body) => Sprint7ObserverPolicy.IsolatedCell(isolation, name, body, (cell, error) =>
            {
                if (childRecord != null) { childRecord[cell] = error == null ? "completed" : error.GetType().Name + ": " + error.Message; return; }
                Sprint25Check("r6-" + cell + "-completed", error == null, new JObject { ["error"] = error == null ? null : error.ToString() },
                    "The cell ran to its end without an exception.");
            });
        }

        private static bool S7SourcesGone(JArray sources)
        {
            return sources != null && sources.Count == 2 && sources.All(id => !Game.Instance.State.Units.Any(unit => unit != null && !unit.Destroyed && unit.UniqueId == (string)id));
        }

        // Owner amendment 3 (R6-F2), recovery C3 (R4-F1, R7-F5): the enclosing runner of the callback-loss observer is
        // disposed while the observer holds its acquired hook, with that hook's unpatch made to fail. The runner is the
        // phase's own cell sequence over a child isolation, with a next cell after it (Sprint7Abandonment). Disposal unwinds
        // the observer's finally: the named failure and the remaining hook reach the child isolation and the other releases
        // (the sources' commands, their retirement) still run. The same admission guard then refuses the next cell. The
        // driver repairs its own injected hook on every exit; the complete child outcome decides: any child failure other
        // than the injected unpatch failure and its remaining hook reaches the phase by name and is never exempted.
        private IEnumerable<int> S7CallbackAbandoned(ExpandedSummoningCorrectionFixture fixture, Func<string, UnitEntityData> summon, UnitEntityData hostile,
            JObject record, Action<string> mark, Func<Vector3, Vector3> ground, Sprint7CellIsolation phase)
        {
            var inner = new JObject(); var probe = new JObject(); var cells = new JObject();
            bool nextStarted = false;
            var driver = new Sprint7Abandonment
            {
                Owner = "s7-callback-abandoned driver", Phase = phase,
                Ready = () => probe["hooksAcquired"] != null,
                Inject = () => S7CallbackWithhold.Inject = "fail-unpatch",
                Clear = () => S7CallbackWithhold.Inject = null,
                Repair = S7CallbackWithhold.Repair,
                Expected = () => new[] { "callback-withhold: cue callback hook: InvalidOperationException: " + S7CallbackWithhold.UnpatchFault,
                    "callback-withhold: cue callback hooks remaining: 1" },
            };
            driver.Unsafe = () =>
            {
                var reasons = new List<string>();
                int hooks;
                try { hooks = S7CallbackWithhold.Hooks(); } catch (Exception) { hooks = -1; }
                driver.Row["hooksAfterRepair"] = hooks;
                if (hooks != 0) reasons.Add(hooks < 0 ? "hook count unreadable" : "hooks remaining after repair: " + hooks);
                bool gone = S7SourcesGone(probe["sources"] as JArray);
                driver.Row["sourcesGone"] = gone;
                if (!gone) reasons.Add("sources not observed gone");
                return reasons;
            };
            JObject row = driver.Row; record["callbackAbandoned"] = row;
            row["probe"] = probe; row["cells"] = cells;
            Func<IEnumerable<int>> next = () => { nextStarted = true; return R6Wait(1); };
            mark("callback-abandon-start");
            try
            {
                foreach (int step in driver.Run(
                    Sprint7ObserverPolicy.Cell("s7-callback-abandoned-inner", () => S7CallbackLoss(fixture, summon, hostile, inner, mark, ground, driver.Child, probe)),
                    Sprint7ObserverPolicy.Cell("s7-callback-abandoned-next", next), S7Cells(driver.Child, cells), TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(30)))
                    yield return step;
            }
            finally
            {
                row["hooksAtAbandon"] = probe["hooksAcquired"];
                row["nextCellStarted"] = nextStarted;
                JToken loss = inner["callbackLoss"];
                row["inner"] = loss == null ? null : new JObject { ["interventionHooksAfter"] = loss["interventionHooksAfter"], ["interventionDisposeFailures"] = loss["interventionDisposeFailures"],
                    ["releaseFailures"] = loss["releaseFailures"], ["retirementFailures"] = loss["retirementFailures"], ["independentCleanup"] = loss["independentCleanup"] };
                mark("callback-abandon-end");
            }
            JToken cleanup = inner["callbackLoss"] == null ? null : inner["callbackLoss"]["independentCleanup"];
            bool otherReleases = cleanup != null && (bool?)cleanup["survivorRetirementRequested"] == true && (bool?)cleanup["otherRetirementRequested"] == true &&
                (int?)cleanup["sourcesRetiredHere"] == 2;
            Sprint25Check("s7-anky-callback-abandoned-unwound", (int?)row["hooksAtAbandon"] == 1 && otherReleases && !nextStarted && driver.Proven, (JObject)row.DeepClone(),
                "With the enclosing cell sequence disposed while the callback-loss observer holds its acquired hook (its unpatch made to fail): the observer's cleanup reports the named unpatch failure and the remaining hook into the child isolation; its other releases still run (both sources retired here, one request each); the same admission guard then refuses the next cell, which never starts; the child failures are exactly the injected ones (anything else would reach the phase by name); the driver removes its own injected hook and observes zero hooks and both sources gone before reporting the fixture safe.");
        }

        // Recovery C3 (R7-F3): the callback-loss observer's hook set-up installs its hook and then fails, and its rollback
        // unpatch fails too. Through the phase's own cell sequence over a child isolation (run to completion): the set-up
        // error and the rollback error reach the child isolation by name with the explicit hook readback (one hook remains),
        // the sources are still retired, and the same admission guard refuses the next cell. The driver repairs its own hook.
        private IEnumerable<int> S7CallbackSetupFailed(ExpandedSummoningCorrectionFixture fixture, Func<string, UnitEntityData> summon, UnitEntityData hostile,
            JObject record, Action<string> mark, Func<Vector3, Vector3> ground, Sprint7CellIsolation phase)
        {
            var inner = new JObject(); var cells = new JObject();
            bool nextStarted = false;
            var driver = new Sprint7Abandonment
            {
                Owner = "s7-callback-setup-failed driver", Phase = phase, Ready = null,
                Inject = () => S7CallbackWithhold.Inject = "fail-setup-and-rollback",
                Clear = () => S7CallbackWithhold.Inject = null,
                Repair = S7CallbackWithhold.Repair,
                Expected = () =>
                {
                    string setupError = Sprint7ObserverPolicy.SetupFailure("S7CallbackWithhold", new InvalidOperationException(S7CallbackWithhold.SetupFault),
                        new List<string> { "cue callback hook: InvalidOperationException: " + S7CallbackWithhold.RollbackFault }, 1).Message;
                    return new[] { "callback-withhold: cue callback setup failed: InvalidOperationException: " + setupError,
                        "s7-callback-setup-failed-inner: cell exception: InvalidOperationException: " + setupError, "callback-withhold: cue callback hooks remaining: 1" };
                },
            };
            driver.Unsafe = () =>
            {
                var reasons = new List<string>();
                int hooks;
                try { hooks = S7CallbackWithhold.Hooks(); } catch (Exception) { hooks = -1; }
                driver.Row["hooksAfterRepair"] = hooks;
                if (hooks != 0) reasons.Add(hooks < 0 ? "hook count unreadable" : "hooks remaining after repair: " + hooks);
                JToken loss = inner["callbackLoss"];
                JToken cleanup = loss == null ? null : loss["independentCleanup"];
                if (cleanup == null || (int?)cleanup["sourcesRetiredHere"] != 2) reasons.Add("sources not retired by the observer's cleanup");
                return reasons;
            };
            JObject row = driver.Row; record["callbackSetupFailed"] = row; row["cells"] = cells;
            Func<IEnumerable<int>> next = () => { nextStarted = true; return R6Wait(1); };
            mark("callback-setup-failed-start");
            try
            {
                foreach (int step in driver.Run(
                    Sprint7ObserverPolicy.Cell("s7-callback-setup-failed-inner", () => S7CallbackLoss(fixture, summon, hostile, inner, mark, ground, driver.Child)),
                    Sprint7ObserverPolicy.Cell("s7-callback-setup-failed-next", next), S7Cells(driver.Child, cells), TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(30)))
                    yield return step;
            }
            finally
            {
                JToken loss = inner["callbackLoss"];
                row["inner"] = loss == null ? null : new JObject { ["hookAttempted"] = loss["hookAttempted"], ["interventionHooksAfter"] = loss["interventionHooksAfter"],
                    ["hookSetupFailure"] = loss["hookSetupFailure"], ["hookSetup"] = loss["hookSetup"], ["releaseFailures"] = loss["releaseFailures"],
                    ["independentCleanup"] = loss["independentCleanup"] };
                row["nextCellStarted"] = nextStarted;
                mark("callback-setup-failed-end");
            }
            JToken innerRow = row["inner"];
            bool readback = innerRow != null && innerRow.Type == JTokenType.Object && (bool?)innerRow["hookAttempted"] == true && (int?)innerRow["interventionHooksAfter"] == 1 &&
                innerRow["hookSetup"] != null && innerRow["hookSetup"].Type == JTokenType.Object && (int?)innerRow["hookSetup"]["hooksBeforeRollback"] == 1 &&
                (int?)innerRow["hookSetup"]["hooksAfterRollback"] == 1;
            Sprint25Check("s7-anky-callback-setup-failed-isolated", readback && !nextStarted && driver.Proven, (JObject)row.DeepClone(),
                "When the callback observer's set-up fails after installing its hook and its rollback unpatch also fails: the set-up error and the rollback error reach the child isolation by name, with the explicit hook readback (one hook installed, one left after the failed rollback, one at the observer's cleanup) although no observer object was returned; the sources are still retired; the same admission guard refuses the next cell, which never starts; the driver removes its own hook and observes zero.");
        }

        // Recovery C3 (R7-F3): the projectile trace's set-up, through the same attempt the Dust cell uses (S7TraceAttempt).
        private IEnumerable<int> S7TraceSetupProbe(Sprint7CellIsolation isolation, JObject record, JObject probe)
        {
            const string Owner = "trace-setup";
            isolation.Opened(Owner);
            var trace = new S7TraceAttempt();
            probe["traceId"] = trace.Id;
            try
            {
                trace.Acquire();
                yield return 0;
            }
            finally
            {
                record["trace"] = trace.Publish(isolation, Owner);
                isolation.Reported(Owner);
            }
        }

        // Recovery C3 (R7-F3): the projectile trace's set-up installs its first hooks and fails, and its rollback unpatch of
        // those hooks fails too. Run to completion through the phase's own cell sequence over a child isolation: the set-up and
        // rollback errors and the explicit readback by the trace's caller-chosen identity reach the child isolation; the same
        // admission guard refuses the next cell; the driver removes the hooks of that identity.
        private IEnumerable<int> S7TraceSetupFailed(JObject record, Action<string> mark, Sprint7CellIsolation phase)
        {
            var inner = new JObject(); var probe = new JObject(); var cells = new JObject();
            bool nextStarted = false;
            var driver = new Sprint7Abandonment
            {
                Owner = "s7-trace-setup-failed driver", Phase = phase, Ready = null,
                Inject = () => S7ProjectileTrace.Inject = "fail-setup-and-rollback",
                Clear = () => S7ProjectileTrace.Inject = null,
                Repair = () => probe["traceId"] == null ? new List<string> { "no trace identity was recorded" } : S7ProjectileTrace.Repair((string)probe["traceId"]),
                Expected = () =>
                {
                    string id = (string)probe["traceId"];
                    string setupError = Sprint7ObserverPolicy.SetupFailure("S7ProjectileTrace", new InvalidOperationException(S7ProjectileTrace.SetupFaultOf(id)),
                        new List<string> { "CreateView hooks: InvalidOperationException: " + S7ProjectileTrace.RollbackFaultOf(id) }, 2).Message;
                    return new[] { "trace-setup: projectile trace setup failed: InvalidOperationException: " + setupError,
                        "s7-trace-setup-failed-inner: cell exception: InvalidOperationException: " + setupError, "trace-setup: projectile trace hooks remaining: 2" };
                },
            };
            driver.Unsafe = () =>
            {
                var reasons = new List<string>();
                int hooks;
                try { hooks = probe["traceId"] == null ? -1 : S7ProjectileTrace.OwnedHooksOf((string)probe["traceId"]); } catch (Exception) { hooks = -1; }
                driver.Row["hooksAfterRepair"] = hooks;
                if (hooks != 0) reasons.Add(hooks < 0 ? "trace hooks unreadable" : "trace hooks remaining after repair: " + hooks);
                return reasons;
            };
            JObject row = driver.Row; record["traceSetupFailed"] = row; row["cells"] = cells; row["probe"] = probe;
            Func<IEnumerable<int>> next = () => { nextStarted = true; return R6Wait(1); };
            mark("trace-setup-failed-start");
            try
            {
                foreach (int step in driver.Run(
                    Sprint7ObserverPolicy.Cell("s7-trace-setup-failed-inner", () => S7TraceSetupProbe(driver.Child, inner, probe)),
                    Sprint7ObserverPolicy.Cell("s7-trace-setup-failed-next", next), S7Cells(driver.Child, cells), TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(15)))
                    yield return step;
            }
            finally
            {
                row["inner"] = inner["trace"];
                row["rollback"] = S7ProjectileTrace.LastRollback == null ? null : S7ProjectileTrace.LastRollback.DeepClone();
                row["nextCellStarted"] = nextStarted;
                mark("trace-setup-failed-end");
            }
            JToken traceRow = row["inner"];
            bool readback = traceRow != null && traceRow.Type == JTokenType.Object && (bool?)traceRow["attempted"] == true && (int?)traceRow["hooksAfter"] == 2 &&
                row["rollback"] != null && row["rollback"].Type == JTokenType.Object && (int?)row["rollback"]["hooksBeforeRollback"] == 2 && (int?)row["rollback"]["hooksAfterRollback"] == 2;
            Sprint25Check("s7-dust-trace-setup-failed-isolated", readback && !nextStarted && driver.Proven, (JObject)row.DeepClone(),
                "When the projectile trace's set-up fails after installing its CreateView hooks and the rollback unpatch of those hooks also fails: the set-up and rollback errors reach the child isolation by name with the explicit readback by the trace's caller-chosen identity (two hooks installed, two left), although no trace object was returned; the same admission guard refuses the next cell, which never starts; the driver removes the hooks of that identity and observes zero.");
        }

        /// <summary>
        /// Recovery C3 (R7-F3): one attempt to install a projectile trace, shared by the Dust cell and the set-up control. The
        /// identity is chosen before the attempt, so the hooks are read back and published by identity whether or not the
        /// constructor returned a trace.
        /// </summary>
        private sealed class S7TraceAttempt
        {
            internal readonly string Id = S7ProjectileTrace.NewId();
            internal readonly Sprint7HookAttempt<S7ProjectileTrace> Attempt;
            internal S7TraceAttempt() { string id = Id; Attempt = new Sprint7HookAttempt<S7ProjectileTrace>("projectile trace", () => S7ProjectileTrace.OwnedHooksOf(id)); }
            internal S7ProjectileTrace Acquire() { string id = Id; return Attempt.Acquire(() => new S7ProjectileTrace(id)); }
            internal JObject Publish(Sprint7CellIsolation isolation, string owner)
            {
                int? after = Attempt.Publish(isolation, owner);
                return new JObject { ["id"] = Id, ["attempted"] = Attempt.Attempted, ["acquired"] = Attempt.Acquired != null, ["setupFailure"] = Attempt.SetupFailure,
                    ["hooksAfter"] = after };
            }
        }

        // Whether the source's own playback component still tracks this cue record.
        private static bool S7CueTracked(UnitEntityData source, SummonCastCueRecord record)
        {
            if (source == null || source.View == null || record == null) return false;
            SummonCastCuePlayback playback = source.View.GetComponent<SummonCastCuePlayback>();
            if (playback == null) return false;
            var active = typeof(SummonCastCuePlayback).GetField("_active", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                .GetValue(playback) as List<SummonCastCueRecord>;
            return active != null && active.Contains(record);
        }

        private static bool S7IsTail(JToken row) { return ((string)row["weapon"] ?? "").IndexOf("Tail", StringComparison.OrdinalIgnoreCase) >= 0; }

        private static int S7TailHits(S7AttackLog log, UnitEntityData unit, int from)
        {
            return log.Rows.Skip(from).Count(value => (string)value["initiator"] == unit.UniqueId && (bool)value["hit"] && S7IsTail(value));
        }

        private static JObject S7Cue(SummonCastCueRecord cue)
        {
            // Recovery C1: the one cue-row producer, shared with the evaluator's tests.
            return Sprint7ObserverPolicy.CueRow(cue.EventName, cue.PlayingId, cue.Frame, cue.Utc, cue.EndUtc, cue.StopReason, cue.StopRequestedUtc, cue.StopResult);
        }

        private IEnumerable<int> S7AttackUntil(UnitEntityData unit, UnitEntityData target, Func<bool> done, int attempts)
        {
            for (int attempt = 0; attempt < attempts && !done(); attempt++)
            {
                target.Descriptor.Damage = 0;
                var attack = new UnitAttack(target);
                S7Run(unit, attack);
                DateTime deadline = DateTime.UtcNow.AddSeconds(8);
                while (DateTime.UtcNow < deadline && !attack.IsFinished) yield return 0;
                S7InterruptIssued(unit, attack);
                foreach (int step in R6Seconds(.6f)) yield return step;
            }
        }

        // The production tail handler on the unit's own fact, called again
        // with a recorded native rule: the handler's own owner, weapon, target
        // and first-observation checks decide.
        /// <summary>
        /// Recovery C3 (R2-F5): a direct retirement inside the effects phase goes through the phase's one retirement record (one
        /// request per unit across every layer; a failure recorded and rethrown); outside the phase it runs directly.
        /// </summary>
        private void S7Retire(UnitEntityData unit, string label, Action<UnitEntityData> retire)
        {
            if (unit == null) return;
            if (_s7Retirements == null) { retire(unit); return; }
            _s7Retirements.Request(unit, label + " " + unit.UniqueId, retire);
        }

        // Recovery C3 (R2-F7, audit A1-F5): the commands the effects runner issued, by reference, for the phase teardown.
        private Sprint7IssuedCommands<UnitEntityData, Kingmaker.UnitLogic.Commands.Base.UnitCommand> _s7Issued;

        /// <summary>Runs a command the effects runner issues, recording it (inside the phase) for the exact teardown.</summary>
        private void S7Run(UnitEntityData unit, Kingmaker.UnitLogic.Commands.Base.UnitCommand command)
        {
            if (_s7Issued != null) _s7Issued.Run(unit, command, (value, issued) => value.Commands.Run(issued));
            else unit.Commands.Run(command);
        }

        /// <summary>
        /// Owner amendment 4 §2B (C3 review R2-F7): the Dust cell's single submission adapter: the shared production operation
        /// registers the exact command with the cell's owner and in the phase record (with that owner's capture), then runs it
        /// natively once. Used by the Dust cell's passive and direct routes and its owning cast.
        /// </summary>
        private void S7SubmitDust(S7DustConditions conditions, UnitEntityData unit, UnitUseAbility command)
        {
            Sprint7ObserverPolicy.SubmitCellCommand(conditions.Commands, _s7Issued, unit, command, (actor, value) => actor.Commands.Run(value));
        }

        /// <summary>
        /// Recovery C3 (R2-F7, audit A1-F6): interrupts exactly one issued command; the actor's queue maintenance (which would
        /// start the queue head) runs only while its queue holds no command outside the runner's record.
        /// </summary>
        private void S7InterruptIssued(UnitEntityData unit, Kingmaker.UnitLogic.Commands.Base.UnitCommand command)
        {
            unit.Commands.InterruptAll(value => ReferenceEquals(value, command));
            if (unit.Commands.Queue.All(value => _s7Issued != null && _s7Issued.Contains(value))) unit.Commands.RemoveFinishedAndUpdateQueue();
        }

        /// <summary>Recovery C3: a unit's terminal state: true when destroyed, false while it is in the game state, null otherwise.</summary>
        private static bool? S7UnitSettled(UnitEntityData unit)
        {
            if (unit == null) return null;
            if (unit.Destroyed) return true;
            return Game.Instance.State.Units.Contains(unit) ? false : (bool?)null;
        }

        /// <summary>
        /// Recovery C3: a fresh resolved tail hit of <paramref name="unit"/> on <paramref name="target"/> (its attack roll's
        /// result set to Hit, as in the crafted production controls), for delivery to the unit's own production handler. The
        /// weapon is the unit's own natural weapon, verified to be the tail its AnkylosaurusTailControl names.
        /// </summary>
        private static RuleAttackWithWeapon S7FreshTailHit(UnitEntityData unit, UnitEntityData target)
        {
            Kingmaker.Items.ItemEntityWeapon weapon = unit.Body.PrimaryHand.MaybeWeapon;
            Kingmaker.Blueprints.Items.Weapons.BlueprintItemWeapon tail = unit.Descriptor.Progression.Features.Enumerable.Cast<Kingmaker.Blueprints.Facts.Fact>()
                .Concat(unit.Descriptor.Buffs.Enumerable.Cast<Kingmaker.Blueprints.Facts.Fact>())
                .SelectMany(fact => fact.Blueprint == null || fact.Blueprint.ComponentsArray == null ? Enumerable.Empty<AnkylosaurusTailControl>() : fact.Blueprint.ComponentsArray.OfType<AnkylosaurusTailControl>())
                .Select(control => control.Tail).FirstOrDefault(value => value != null);
            if (weapon == null || tail == null || !ReferenceEquals(weapon.Blueprint, tail)) throw new InvalidOperationException("The source's tail weapon is not the one its tail control names.");
            var evt = new RuleAttackWithWeapon(unit, target, weapon, 0);
            var roll = new RuleAttackRoll(unit, target, evt.WeaponStats, 0);
            typeof(RuleAttackRoll).GetProperty("Result").GetSetMethod(true).Invoke(roll, new object[] { AttackResult.Hit });
            typeof(RuleAttackWithWeapon).GetProperty("AttackRoll").GetSetMethod(true).Invoke(evt, new object[] { roll });
            return evt;
        }

        private static int S7ReplayTail(UnitEntityData unit, RuleAttackWithWeapon evt)
        {
            int calls = 0;
            IEnumerable<Kingmaker.Blueprints.Facts.Fact> facts = unit.Descriptor.Progression.Features.Enumerable.Cast<Kingmaker.Blueprints.Facts.Fact>()
                .Concat(unit.Descriptor.Buffs.Enumerable.Cast<Kingmaker.Blueprints.Facts.Fact>());
            foreach (var fact in facts.Where(value => value.Blueprint != null && value.Blueprint.ComponentsArray != null &&
                value.Blueprint.ComponentsArray.Any(component => component is AnkylosaurusTailControl)).ToArray())
                fact.CallComponents<AnkylosaurusTailControl>(component => { calls++; component.OnEventDidTrigger(evt); });
            return calls;
        }

        private IEnumerable<int> S7Ankylosaurus(ExpandedSummoningCorrectionFixture fixture, Func<string, UnitEntityData> summon,
            UnitEntityData hostile, JObject record, Action<string> mark, Func<Vector3, Vector3> ground, Sprint7CellIsolation isolation)
        {
            var row = new JObject(); record["ankylosaurus"] = row;
            var log = new S7AttackLog();
            var setup = new Sprint7SetupScope();
            int armor = hostile.Descriptor.Stats.AC.BaseValue;
            try
            {
                setup.Acquire("attack log subscription", () => EventBus.Subscribe(log), () => EventBus.Unsubscribe(log));
                UnitEntityData first = summon("ankylosaurus"), second = summon("ankylosaurus");
                PlaceExpandedSummoningUnit(first, ground(hostile.Position + Vector3.left * 3f));
                PlaceExpandedSummoningUnit(second, ground(hostile.Position + Vector3.right * 3f));
                foreach (int step in R6Seconds(2f)) yield return step;

                // 1. Qualifying impacts: one cue per resolved tail hit, correlated
                // by owner, target, weapon, roll identity, frame and playing ID.
                int logFrom = log.Rows.Count, cuesFrom = S7ImpactCues(first).Length;
                mark("anky-hits-start");
                foreach (int step in S7AttackUntil(first, hostile, () => S7TailHits(log, first, logFrom) >= 2, 6)) yield return step;
                foreach (int step in R6Seconds(4f)) yield return step;
                mark("anky-hits-end");
                var hits = log.Rows.Skip(logFrom).Where(value => (string)value["initiator"] == first.UniqueId && (bool)value["hit"] && S7IsTail(value)).ToArray();
                SummonCastCueRecord[] hitCues = S7ImpactCues(first).Skip(cuesFrom).ToArray();
                double afterSequence = S7ListenerDistance(first);
                var pairs = new JArray(hits.Select(hit => new JObject { ["hit"] = hit.DeepClone(), ["listenerDistanceAtEvent"] = hit["listenerDistanceAtEvent"],
                    ["listenerDistanceAfterSequence"] = afterSequence,
                    ["cues"] = new JArray(hitCues.Where(cue => cue.Frame == (int)hit["frame"]).Select(S7Cue)) }));
                row["attacks"] = new JArray(log.Rows.Skip(logFrom)); row["hitCuePairs"] = pairs;
                bool paired = hits.Length >= 1 && hitCues.Length == hits.Length && pairs.All(value => ((JArray)value["cues"]).Count == 1) &&
                    hits.All(hit => (string)hit["target"] == hostile.UniqueId) &&
                    hitCues.All(value => value.EventName == AnkylosaurusTailImpactPolicy.ImpactCue && value.PlayingId != 0 && value.EndUtc.HasValue && value.StopReason == null);
                Sprint25Check("s7-anky-one-cue-per-impact", paired, new JObject { ["pairs"] = pairs.DeepClone() },
                    "Each resolved tail hit by the exact owned Ankylosaurus on its attack's target posts exactly one accepted impact cue in the same frame, which completes naturally.");

                // 2. The production handler observes the same qualifying roll
                // again, and a foreign owner's roll, through the owner's fact.
                RuleAttackWithWeapon qualifying = log.Events.FirstOrDefault(value => ReferenceEquals(value.Initiator, first) &&
                    value.AttackRoll != null && value.AttackRoll.IsHit && value.Weapon != null && value.Weapon.Blueprint.name.IndexOf("Tail", StringComparison.OrdinalIgnoreCase) >= 0);
                int replayFrom = S7ImpactCues(first).Length;
                int sameCalls = qualifying == null ? 0 : S7ReplayTail(first, qualifying);
                int afterSame = S7ImpactCues(first).Length - replayFrom;
                // A foreign owner's tail hit, produced natively by the second Ankylosaurus.
                int secondFrom = log.Rows.Count;
                foreach (int step in S7AttackUntil(second, hostile, () => S7TailHits(log, second, secondFrom) >= 1, 6)) yield return step;
                RuleAttackWithWeapon foreign = log.Events.LastOrDefault(value => ReferenceEquals(value.Initiator, second) &&
                    value.AttackRoll != null && value.AttackRoll.IsHit);
                int foreignFrom = S7ImpactCues(first).Length;
                int foreignCalls = foreign == null ? 0 : S7ReplayTail(first, foreign);
                int afterForeign = S7ImpactCues(first).Length - foreignFrom;
                // A later real hit still posts its own cue.
                int laterFrom = S7ImpactCues(first).Length, laterLog = log.Rows.Count;
                foreach (int step in S7AttackUntil(first, hostile, () => S7TailHits(log, first, laterLog) >= 1, 6)) yield return step;
                foreach (int step in R6Seconds(2.5f)) yield return step;
                int laterHits = S7TailHits(log, first, laterLog), laterCues = S7ImpactCues(first).Length - laterFrom;
                row["replay"] = new JObject { ["sameRollCalls"] = sameCalls, ["sameRollNewCues"] = afterSame,
                    ["foreignOwnerCalls"] = foreignCalls, ["foreignOwnerNewCues"] = afterForeign,
                    ["laterHits"] = laterHits, ["laterCues"] = laterCues,
                    ["foreignWeapon"] = "the Ankylosaurus has only its tail attack natively; see craftedControls",
                    ["invalidTarget"] = "see craftedControls" };
                // 2b. The production handler on fresh resolved rolls with
                // crafted wrappers: a valid control, a foreign weapon, a wrapper
                // naming another creature than its roll's target (then the
                // valid wrapper of that same roll), and an out-of-game target.
                var crafted = new JObject(); row["craftedControls"] = crafted;
                Func<UnitEntityData, Kingmaker.Items.ItemEntityWeapon, RuleAttackRoll> freshRoll = (rollTarget, weapon) =>
                {
                    var owner = new RuleAttackWithWeapon(first, rollTarget, weapon, 0);
                    var roll = new RuleAttackRoll(first, rollTarget, owner.WeaponStats, 0);
                    typeof(RuleAttackRoll).GetProperty("Result").GetSetMethod(true).Invoke(roll, new object[] { AttackResult.Hit });
                    return roll;
                };
                Func<UnitEntityData, Kingmaker.Items.ItemEntityWeapon, RuleAttackRoll, RuleAttackWithWeapon> wrap = (target, weapon, roll) =>
                {
                    var evt = new RuleAttackWithWeapon(first, target, weapon, 0);
                    typeof(RuleAttackWithWeapon).GetProperty("AttackRoll").GetSetMethod(true).Invoke(evt, new object[] { roll });
                    return evt;
                };
                Func<RuleAttackWithWeapon, JObject> run = evt =>
                {
                    int cueFrom = S7ImpactCues(first).Length; string failure = null; int calls = 0;
                    bool inGame = evt.Target != null && !evt.Target.Destroyed && evt.Target.IsInGame;
                    try { calls = S7ReplayTail(first, evt); } catch (Exception error) { failure = error.GetType().Name + ": " + error.Message; }
                    return new JObject { ["weapon"] = evt.Weapon.Blueprint.AssetGuid, ["weaponName"] = evt.Weapon.Blueprint.name,
                        ["wrapperTarget"] = evt.Target == null ? null : evt.Target.UniqueId, ["rollTarget"] = evt.AttackRoll.Target == null ? null : evt.AttackRoll.Target.UniqueId,
                        ["targetInGame"] = inGame, ["hit"] = evt.AttackRoll.IsHit, ["calls"] = calls, ["newCues"] = S7ImpactCues(first).Length - cueFrom, ["error"] = failure };
                };
                if (qualifying != null)
                {
                    // A foreign weapon: the humanoid caster's own weapon (the Ankylosaurus has no hands).
                    Kingmaker.Items.ItemEntityWeapon tail = qualifying.Weapon,
                        unarmed = fixture.Caster.Body.PrimaryHand.MaybeWeapon ?? fixture.Caster.Body.EmptyHandWeapon;
                    crafted["valid"] = run(wrap(hostile, tail, freshRoll(hostile, tail)));
                    crafted["foreignWeapon"] = run(wrap(hostile, unarmed, freshRoll(hostile, unarmed)));
                    RuleAttackRoll shared = freshRoll(hostile, tail);
                    crafted["mismatchedWrapper"] = run(wrap(second, tail, shared));
                    crafted["sameRollValidWrapper"] = run(wrap(hostile, tail, shared));
                    UnitEntityData ghost = Game.Instance.EntityCreator.SpawnUnit(fixture.HostileBlueprint ?? hostile.Blueprint,
                        ground(hostile.Position + Vector3.forward * 3f), Quaternion.identity, fixture.Scene);
                    try
                    {
                        Sprint28SetBrainActivePreservingActions(ghost, false);
                        ghost.IsInGame = false;
                        crafted["removedTarget"] = run(wrap(ghost, tail, freshRoll(ghost, tail)));
                    }
                    finally { ghost.IsInGame = true; CleanupExpandedSummoningUnit(ghost); }
                }
                Func<string, int, bool> posted = (name, count) => crafted[name] != null && (int)crafted[name]["calls"] >= 1 &&
                    (int)crafted[name]["newCues"] == count && string.IsNullOrEmpty((string)crafted[name]["error"]);
                Sprint25Check("s7-anky-production-controls", posted("valid", 1) && posted("foreignWeapon", 0) && posted("mismatchedWrapper", 0) &&
                    posted("sameRollValidWrapper", 1) && posted("removedTarget", 0), (JObject)crafted.DeepClone(),
                    "Through the owner's own production handler: a valid fresh resolved tail roll posts one cue; a roll with a foreign (non-tail) weapon posts none; a wrapper naming another creature than its roll's target posts none and does not consume the roll, whose valid wrapper then posts its one cue; an out-of-game target posts none.");

                Sprint25Check("s7-anky-replay-dedup", qualifying != null && sameCalls >= 1 && afterSame == 0 && foreign != null && foreignCalls >= 1 &&
                    afterForeign == 0 && laterHits >= 1 && laterCues == laterHits, (JObject)row["replay"].DeepClone(),
                    "The production handler, given the same qualifying roll again, posts nothing; given another owner's tail hit, posts nothing; a later real hit posts its own cue.");

                // 3. A miss: the hostile's armor is raised out of reach.
                hostile.Descriptor.Stats.AC.BaseValue = armor + 200;
                logFrom = log.Rows.Count; cuesFrom = S7ImpactCues(first).Length;
                mark("anky-miss-start");
                foreach (int step in S7AttackUntil(first, hostile, () => false, 1)) yield return step;
                foreach (int step in R6Seconds(2f)) yield return step;
                mark("anky-miss-end");
                hostile.Descriptor.Stats.AC.BaseValue = armor;
                var missRows = log.Rows.Skip(logFrom).Where(value => (string)value["initiator"] == first.UniqueId).ToArray();
                int missCues = S7ImpactCues(first).Length - cuesFrom;
                Sprint25Check("s7-anky-miss-no-cue", missRows.Length > 0 && missRows.All(value => !(bool)value["hit"]) && missCues == 0,
                    new JObject { ["attacks"] = new JArray(missRows), ["cues"] = missCues }, "Missed strikes post no impact cue.");

                // 4. Cancelled before the strike resolves.
                logFrom = log.Rows.Count; cuesFrom = S7ImpactCues(first).Length;
                mark("anky-cancel-start");
                var cancelled = new UnitAttack(hostile);
                S7Run(first, cancelled);
                yield return 0;
                S7InterruptIssued(first, cancelled);
                foreach (int step in R6Seconds(2f)) yield return step;
                mark("anky-cancel-end");
                int cancelAttacks = log.Rows.Skip(logFrom).Count(value => (string)value["initiator"] == first.UniqueId);
                int cancelCues = S7ImpactCues(first).Length - cuesFrom;
                Sprint25Check("s7-anky-cancel-no-cue", cancelAttacks == 0 && cancelCues == 0,
                    new JObject { ["attacks"] = cancelAttacks, ["cues"] = cancelCues }, "An attack interrupted before it resolves posts no cue.");

                // 5. Concurrent sources; one retired while its own cue plays.
                int firstFrom = S7ImpactCues(first).Length, secondCueFrom = S7ImpactCues(second).Length;
                mark("anky-concurrent-start");
                var firstAttack = new UnitAttack(hostile);
                S7Run(first, firstAttack);
                S7Run(second, new UnitAttack(hostile));
                DateTime wait = DateTime.UtcNow.AddSeconds(16);
                SummonCastCueRecord retiring = null, survivor = null;
                // Retire only once both cues are live at the same time.
                while (DateTime.UtcNow < wait)
                {
                    retiring = S7ImpactCues(second).Skip(secondCueFrom).FirstOrDefault(value => !value.EndUtc.HasValue);
                    survivor = S7ImpactCues(first).Skip(firstFrom).FirstOrDefault(value => !value.EndUtc.HasValue);
                    if (retiring != null && survivor != null) break;
                    yield return 0;
                }
                bool overlapping = retiring != null && survivor != null && !retiring.EndUtc.HasValue && !survivor.EndUtc.HasValue;
                // R4-F5: native playback of each playing ID (installed query: confirmed playing,
                // confirmed inactive, or unknown) and the product's tracking of the survivor on its
                // own source, around the retirement; part of the verdict.
                var retirementTrace = new JArray(); row["retirementTrace"] = retirementTrace;
                SummonCastCueRecord retiringRecord = retiring, survivorRecord = survivor;
                Func<string, JObject> sample = moment =>
                {
                    var value = new JObject { ["moment"] = moment, ["frame"] = Time.frameCount, ["utc"] = DateTime.UtcNow.ToString("o"),
                        ["retiringPlayingId"] = retiringRecord == null ? 0 : retiringRecord.PlayingId, ["retiringPlayback"] = S7Playback(retiringRecord),
                        ["retiringEnded"] = retiringRecord != null && retiringRecord.EndUtc.HasValue,
                        ["survivorPlayingId"] = survivorRecord == null ? 0 : survivorRecord.PlayingId, ["survivorPlayback"] = S7Playback(survivorRecord),
                        ["survivorEnded"] = survivorRecord != null && survivorRecord.EndUtc.HasValue,
                        ["survivorStopAttempts"] = survivorRecord == null ? -1 : survivorRecord.StopAttempts,
                        ["survivorTracked"] = S7CueTracked(first, survivorRecord) };
                    retirementTrace.Add(value);
                    return value;
                };
                JObject beforeRetirement = sample("before-retirement");
                DateTime retiredAt = DateTime.UtcNow;
                mark("anky-retire");
                S7Retire(second, "ankylosaurus normal-control source", _s7Retire ?? (unit => DisposeExpandedSummoningUnits(fixture.Created, new[] { unit })));
                sample("after-retirement");
                DateTime ends = DateTime.UtcNow.AddSeconds(10);
                bool sampledMid = false;
                while (DateTime.UtcNow < ends && !(retiring != null && retiring.EndUtc.HasValue && survivor != null && survivor.EndUtc.HasValue))
                {
                    if (!sampledMid && (DateTime.UtcNow - retiredAt).TotalSeconds > 3) { sampledMid = true; sample("retirement+3s"); }
                    yield return 0;
                }
                sample("end-of-wait");
                // Two frames for the source's own playback component to drop an ended cue.
                yield return 0; yield return 0;
                JObject settled = sample("settled");
                row["cuesAfterRetirement"] = new JObject { ["first"] = new JArray(S7ImpactCues(first).Select(S7Cue)), ["second"] = new JArray(S7ImpactCues(second).Select(S7Cue)), ["frame"] = Time.frameCount };
                S7InterruptIssued(first, firstAttack);
                mark("anky-concurrent-end");
                row["retired"] = retiring == null ? null : S7Cue(retiring);
                row["survivor"] = survivor == null ? null : S7Cue(survivor);
                row["overlappingAtRetirement"] = overlapping; row["retiredUtc"] = retiredAt.ToString("o");
                // The R6 retirement rule (the strike had resolved: its cue exists).
                bool retiredStopped = retiring != null && SummonPlaytestR6Policy.CueRetirementObserved(retiring, retiredAt, true, overlapping, retiring.PlayingId, 1d);
                bool survivorNatural = survivor != null && survivor.EndUtc.HasValue && survivor.EndUtc.Value > retiredAt && survivor.StopReason == null;
                string traceVerdict = Sprint7ObserverPolicy.ConcurrentRetirementTrace((string)beforeRetirement["retiringPlayback"], (string)beforeRetirement["survivorPlayback"],
                    (int)settled["survivorStopAttempts"], (bool)settled["survivorEnded"], (string)settled["survivorPlayback"], (bool)settled["survivorTracked"], (string)settled["retiringPlayback"]);
                row["retirementTraceVerdict"] = traceVerdict;
                Sprint25Check("s7-anky-concurrent-retirement", overlapping && retiredStopped && survivorNatural && traceVerdict == null,
                    new JObject { ["retired"] = row["retired"] == null ? null : row["retired"].DeepClone(), ["survivor"] = row["survivor"] == null ? null : row["survivor"].DeepClone(),
                        ["overlappingAtRetirement"] = overlapping, ["retiredUtc"] = retiredAt.ToString("o"), ["traceVerdict"] = traceVerdict,
                        ["beforeRetirement"] = beforeRetirement.DeepClone(), ["settled"] = settled.DeepClone() },
                    "With both Ankylosaurus cues confirmed playing at once, retiring one stops its own playing ID (stop requested and native end within a second) and no other; the other plays past the retirement, ends by its own end-of-event callback, is then confirmed inactive by the native query and is no longer tracked by its source; the retired ID is confirmed inactive.");

                // 6. R4-F5 characterization (synthetic, labelled): callback loss
                // for one exact survivor; step 5 is the normal-callback control.
                foreach (int step in S7CallbackLoss(fixture, summon, hostile, row, mark, ground, isolation)) yield return step;
            }
            finally
            {
                var released = Sprint7ObserverPolicy.RunReleases(new[] { new KeyValuePair<string, Action>("hostile armor", () => hostile.Descriptor.Stats.AC.BaseValue = armor) });
                released.AddRange(setup.ReleaseAll());
                isolation.Fail("s7-ankylosaurus", released);
                if (_s7Retirements != null) _s7Retirements.Publish(isolation, "s7-ankylosaurus", S7UnitSettled);
            }
        }

        // Read-only record of every projectile creation and the effects
        // spawned inside it. Native CreateView spawns the visible cone (the
        // projectile's cast effect) through FxHelper.SpawnFxOnPoint, a call the
        // product makes through its own wrapper (SpawnCastFx). A postfix on the
        // native spawn reads the instance before any product code; a postfix on
        // the product wrapper reads it as the product left it; the creation is
        // closed after CreateView returns, with the launch point, the
        // controller's previous-launch-point state at entry and the effect's
        // orientation. Instances are kept to read their restoration.
        private sealed class S7ProjectileTrace : IDisposable
        {
            private static S7ProjectileTrace active;
            private static Kingmaker.Controllers.Projectiles.Projectile current;
            private static JObject creation;
            private static readonly System.Reflection.FieldInfo PreviousLaunch = typeof(Kingmaker.Controllers.Projectiles.ProjectileController)
                .GetField("m_PrevLaunchPos", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            private readonly Harmony12.HarmonyInstance harmony;
            private readonly string id;
            private static readonly System.Reflection.MethodInfo create = typeof(Kingmaker.Controllers.Projectiles.ProjectileController)
                .GetMethod("CreateView", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic |
                    System.Reflection.BindingFlags.Instance, null,
                    new[] { typeof(Kingmaker.Controllers.Projectiles.Projectile), typeof(Vector3?) }, null);
            private static readonly System.Reflection.MethodInfo spawn = typeof(Kingmaker.Visual.Particles.FxHelper).GetMethod("SpawnFxOnPoint",
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static, null,
                new[] { typeof(GameObject), typeof(Vector3), typeof(Quaternion) }, null);
            private static readonly System.Reflection.MethodInfo product = typeof(KingmakerGunslinger.Summoning.DustMephitBreathPresentation).GetMethod("SpawnCastFx",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            internal readonly JArray Creations = new JArray();
            internal readonly List<KeyValuePair<JObject, GameObject>> Instances = new List<KeyValuePair<JObject, GameObject>>();

            // Bounded fault injection for the rollback check only.
            internal static string Inject;
            internal static JObject LastRollback;
            internal static S7ProjectileTrace Last;

            /// <summary>Recovery C3 (R7-F3): a fresh trace identity, chosen by the caller before the set-up is attempted.</summary>
            internal static string NewId() { return "KMG.S7.Projectile." + Guid.NewGuid().ToString("N"); }

            /// <summary>The hooks owned by the trace of this identity, read without the trace object.</summary>
            internal static int OwnedHooksOf(string owner) { return HookCount(create, owner) + HookCount(spawn, owner) + HookCount(product, owner); }

            // Recovery C3 (R1-F6): the exact texts of the trace's injected set-up faults, bound to its identity.
            internal static string SetupFaultOf(string owner) { return "injected failure after the first patch (" + owner + ")"; }
            internal static string RollbackFaultOf(string owner) { return "injected failure of the CreateView rollback unpatch (" + owner + ")"; }

            /// <summary>
            /// The outer driver's repair of its own injected intervention: removes the hooks of this identity. R1-F5: each
            /// method is attempted when it is listed as patched or the listing cannot be read; no count gates a release.
            /// </summary>
            internal static List<string> Repair(string owner)
            {
                var releases = new List<KeyValuePair<string, Action>>();
                List<System.Reflection.MethodBase> listed;
                try { listed = Harmony12.HarmonyInstance.Create(owner).GetPatchedMethods().ToList(); } catch (Exception) { listed = null; }
                foreach (System.Reflection.MethodInfo method in new[] { create, spawn, product })
                {
                    System.Reflection.MethodInfo target = method;
                    if (listed == null || listed.Contains(target)) releases.Add(new KeyValuePair<string, Action>("trace hook repair " + target.Name,
                        () => Harmony12.HarmonyInstance.Create(owner).Unpatch(target, Harmony12.HarmonyPatchType.All, owner)));
                }
                return Sprint7ObserverPolicy.RunReleases(releases);
            }

            internal S7ProjectileTrace() : this(NewId()) { }

            internal S7ProjectileTrace(string owner)
            {
                id = owner;
                LastRollback = null;
                if (active != null || create == null || spawn == null || product == null || PreviousLaunch == null) throw new InvalidOperationException("Projectile readers unavailable.");
                harmony = Harmony12.HarmonyInstance.Create(id);
                var flags = System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic;
                try
                {
                    patched.Add(create);
                    harmony.Patch(create, new Harmony12.HarmonyMethod(typeof(S7ProjectileTrace).GetMethod("Before", flags)) { prioritiy = Harmony12.Priority.First },
                        new Harmony12.HarmonyMethod(typeof(S7ProjectileTrace).GetMethod("AfterCreate", flags)) { prioritiy = Harmony12.Priority.Last });
                    if (Inject == "after-first-patch") throw new InvalidOperationException("injected failure after the first patch");
                    if (Inject == "fail-setup-and-rollback") throw new InvalidOperationException(SetupFaultOf(id));
                    patched.Add(spawn);
                    harmony.Patch(spawn, null, new Harmony12.HarmonyMethod(typeof(S7ProjectileTrace).GetMethod("BeforeProduct", flags)) { prioritiy = Harmony12.Priority.First });
                    patched.Add(product);
                    harmony.Patch(product, null, new Harmony12.HarmonyMethod(typeof(S7ProjectileTrace).GetMethod("AfterProduct", flags)) { prioritiy = Harmony12.Priority.Last });
                    active = this;
                    Last = this;
                }
                catch (Exception original)
                {
                    // No owned hook survives a failed set-up; an incomplete rollback is reported with the original cause. R1-F5:
                    // the rollback runs whatever the counts read; an unreadable count is unknown (null), never zero.
                    int? installed, remaining;
                    List<string> failures = Sprint7ObserverPolicy.RollbackSetup(OwnedHooks, Unhook, out installed, out remaining);
                    LastRollback = new JObject { ["id"] = id, ["hooksBeforeRollback"] = installed, ["hooksAfterRollback"] = remaining, ["failures"] = new JArray(failures) };
                    throw Sprint7ObserverPolicy.SetupFailure("S7ProjectileTrace", original, failures, remaining);
                }
            }

            // "fail-normal-unpatch" (rollback check only): the CreateView release
            // fails during a normal disposal, so its hooks stay; the other
            // releases still run.
            // Only methods this trace began to patch are unpatched.
            private readonly List<System.Reflection.MethodBase> patched = new List<System.Reflection.MethodBase>();

            private List<string> Unhook()
            {
                var releases = new List<KeyValuePair<string, Action>>();
                if (patched.Contains(create))
                    releases.Add(new KeyValuePair<string, Action>("CreateView hooks", () =>
                    {
                        if (Inject == "fail-normal-unpatch") throw new InvalidOperationException("injected failure of the CreateView unpatch");
                        if (Inject == "fail-setup-and-rollback") throw new InvalidOperationException(RollbackFaultOf(id));
                        harmony.Unpatch(create, Harmony12.HarmonyPatchType.All, id);
                    }));
                if (patched.Contains(spawn)) releases.Add(new KeyValuePair<string, Action>("SpawnFxOnPoint hooks", () => harmony.Unpatch(spawn, Harmony12.HarmonyPatchType.All, id)));
                if (patched.Contains(product)) releases.Add(new KeyValuePair<string, Action>("product spawn hooks", () => harmony.Unpatch(product, Harmony12.HarmonyPatchType.All, id)));
                return Sprint7ObserverPolicy.RunReleases(releases);
            }

            internal GameObject Instance(JObject row)
            {
                return Instances.Where(entry => ReferenceEquals(entry.Key, row)).Select(entry => entry.Value).FirstOrDefault();
            }

            internal JObject Signature(JObject row)
            {
                GameObject instance = Instance(row);
                return instance == null ? null : S7FxSignature(instance);
            }

            // Under the owner profile's Harmony12 shim (over Harmony 2), GetPatchInfo
            // dereferences null for a method that was never patched; only patched
            // methods are read.
            internal static int HookCount(System.Reflection.MethodBase method, string owner)
            {
                var instance = Harmony12.HarmonyInstance.Create(owner);
                if (method == null || !instance.GetPatchedMethods().Contains(method)) return 0;
                var info = instance.GetPatchInfo(method);
                // R1-F5: a listed method whose patch information cannot be read is unknown, never zero.
                if (info == null) throw new InvalidOperationException("patch information unavailable for a listed method");
                return info.Prefixes.Concat(info.Postfixes).Concat(info.Transpilers).Count(value => value.owner == owner);
            }

            internal int OwnedHooks() { return HookCount(create, id) + HookCount(spawn, id) + HookCount(product, id); }
            internal System.Reflection.MethodBase CreateMethod { get { return create; } }
            internal System.Reflection.MethodBase SpawnMethod { get { return spawn; } }
            internal System.Reflection.MethodBase ProductMethod { get { return product; } }
            internal string Id { get { return id; } }

            // R1-F9 (native): armed for one owned Dust creation. While that
            // enclosing creation is in progress, an inner owned creation of the
            // same Dust breath is interrupted (no target: native CreateView
            // throws after setting its launch point, before its spawn); then,
            // before the enclosing call returns, a foreign spawn of the same cast
            // effect at the same point is made directly. The enclosing creation
            // then proceeds as the subsequent valid owned spawn.
            internal static JObject NestedProbe;
            internal static string NestedProbeLauncher;

            private static void RunNestedProbe(Kingmaker.Controllers.Projectiles.ProjectileController controller, Kingmaker.Controllers.Projectiles.Projectile outer, JObject probe)
            {
                var method = typeof(Kingmaker.Controllers.Projectiles.ProjectileController).GetMethod("CreateView", System.Reflection.BindingFlags.NonPublic |
                    System.Reflection.BindingFlags.Instance, null, new[] { typeof(Kingmaker.Controllers.Projectiles.Projectile), typeof(Vector3?) }, null);
                var inner = new Kingmaker.Controllers.Projectiles.Projectile { Launcher = outer.Launcher, Blueprint = outer.Blueprint, Target = null };
                int applied = DustMephitBreathTint.Applied;
                probe["frame"] = Time.frameCount;
                probe["enclosingExecutingAbility"] = DustMephitBreathExecutionScope.ExecutingAbility(outer.Launcher);
                try { method.Invoke(controller, new object[] { inner, null }); probe["innerInterrupted"] = null; }
                catch (System.Reflection.TargetInvocationException error) { probe["innerInterrupted"] = error.InnerException == null ? "unknown" : error.InnerException.GetType().Name; }
                // The inner creation never closes: its own trace entry is dropped.
                current = null; creation = null;
                probe["innerTintsApplied"] = DustMephitBreathTint.Applied - applied;
                probe["innerLaunchPoint"] = S7Vector(inner.LaunchPosition);
                GameObject prefab = outer.Blueprint.CastFx.Load();
                GameObject foreignSpawn = Kingmaker.Visual.Particles.FxHelper.SpawnFxOnPoint(prefab, inner.LaunchPosition);
                probe["foreignSpawned"] = foreignSpawn != null;
                probe["foreignPrefab"] = prefab == null ? null : prefab.name;
                probe["foreignPoint"] = foreignSpawn == null ? null : S7Vector(foreignSpawn.transform.position);
                probe["foreignTinted"] = foreignSpawn != null && foreignSpawn.GetComponent<DustMephitBreathTint>() != null;
                probe["foreignTintsApplied"] = DustMephitBreathTint.Applied - applied;
                if (foreignSpawn != null)
                {
                    probe["foreignSignature"] = S7FxSignature(foreignSpawn);
                    var destroy = foreignSpawn.GetComponent<Kingmaker.Utility.AutoDestroy>() ?? foreignSpawn.AddComponent<Kingmaker.Utility.AutoDestroy>();
                    destroy.Lifetime = 1f;
                }
                probe["appliedBeforeEnclosingSpawn"] = DustMephitBreathTint.Applied;
            }

            private static void Before(Kingmaker.Controllers.Projectiles.ProjectileController __instance, Kingmaker.Controllers.Projectiles.Projectile __0)
            {
                JObject probe = NestedProbe;
                if (probe != null && active != null && __0 != null && __0.Launcher != null && __0.Launcher.UniqueId == NestedProbeLauncher &&
                    __0.Blueprint != null && __0.Blueprint.AssetGuid == MephitBreathPolicy.ColdConeProjectileGuid &&
                    DustMephitBreathExecutionScope.ExecutingAbility(__0.Launcher) == MephitBreathPolicy.DustMephitBreathGuid)
                {
                    NestedProbe = null; NestedProbeLauncher = null;
                    try { RunNestedProbe(__instance, __0, probe); }
                    catch (Exception error) { probe["probeError"] = error.GetType().Name + ": " + error.Message; }
                }
                current = __0; creation = null;
                if (active == null || __0 == null || __0.Launcher == null) return;
                Vector3 previous = (Vector3)PreviousLaunch.GetValue(__instance);
                creation = new JObject { ["frame"] = Time.frameCount, ["utc"] = DateTime.UtcNow.ToString("o"),
                    ["launcher"] = __0.Launcher.UniqueId, ["launcherBlueprint"] = __0.Launcher.Blueprint.AssetGuid,
                    ["projectile"] = __0.Blueprint == null ? null : __0.Blueprint.AssetGuid,
                    ["executingAbility"] = DustMephitBreathExecutionScope.ExecutingAbility(__0.Launcher),
                    ["previousLaunchPoint"] = S7Vector(previous), ["previousLaunchPointCleared"] = previous == Vector3.zero,
                    ["callSiteInstalled"] = DustMephitBreathPresentation.Installed, ["effects"] = new JArray() };
            }

            private static void BeforeProduct(GameObject __result, GameObject __0)
            {
                var value = active; var projectile = current;
                if (value == null || creation == null || projectile == null || __result == null) return;
                var row = new JObject { ["instance"] = __result.GetInstanceID(), ["prefab"] = __0 == null ? null : __0.name,
                    ["isCastFx"] = projectile.Blueprint != null && ReferenceEquals(__0, projectile.Blueprint.CastFx.Load()),
                    ["tintBeforeProduct"] = __result.GetComponent<DustMephitBreathTint>() != null,
                    ["before"] = S7FxSignature(__result), ["launcher"] = projectile.Launcher.UniqueId };
                ((JArray)creation["effects"]).Add(row);
                value.Instances.Add(new KeyValuePair<JObject, GameObject>(row, __result));
            }

            // After the product's wrapper of CreateView's cast-effect spawn.
            private static void AfterProduct(GameObject __result)
            {
                if (active == null || creation == null || __result == null) return;
                JObject row = ((JArray)creation["effects"]).OfType<JObject>().LastOrDefault(value => (int)value["instance"] == __result.GetInstanceID());
                if (row == null) return;
                DustMephitBreathTint tint = __result.GetComponent<DustMephitBreathTint>();
                row["tinted"] = tint != null;
                row["recolouredMaterials"] = tint == null ? 0 : tint.RecolouredMaterials;
                row["recolouredParticleColours"] = tint == null ? 0 : tint.RecolouredParticleColours;
                row["after"] = S7FxSignature(__result);
            }

            private static void AfterCreate()
            {
                var value = active; var projectile = current; var closing = creation;
                current = null; creation = null;
                if (value == null || closing == null || projectile == null) return;
                closing["launchPoint"] = S7Vector(projectile.LaunchPosition);
                foreach (JObject row in ((JArray)closing["effects"]).OfType<JObject>())
                {
                    GameObject instance = value.Instances.Where(entry => ReferenceEquals(entry.Key, row)).Select(entry => entry.Value).FirstOrDefault();
                    if (instance == null || projectile.View == null) continue;
                    row["orientationDegrees"] = Quaternion.Angle(instance.transform.rotation, projectile.View.transform.rotation);
                    var destroy = instance.GetComponent<Kingmaker.Utility.AutoDestroy>();
                    row["lifetime"] = destroy == null ? -1f : destroy.Lifetime;
                }
                value.Creations.Add(closing);
            }

            // The instance after its release: the product's component is gone
            // and its materials and particle colours read as before the tint.
            internal JObject Restoration(JObject row)
            {
                GameObject instance = Instances.Where(entry => ReferenceEquals(entry.Key, row)).Select(entry => entry.Value).FirstOrDefault();
                if (instance == null) return new JObject { ["exists"] = false };
                JObject now = S7FxSignature(instance);
                return new JObject { ["exists"] = true, ["active"] = instance.activeInHierarchy,
                    ["tintPresent"] = instance.GetComponent<DustMephitBreathTint>() != null, ["signature"] = now,
                    ["matchesBefore"] = (string)now["hash"] == (string)row["before"]["hash"] };
            }

            internal bool Live(JObject row)
            {
                GameObject instance = Instances.Where(entry => ReferenceEquals(entry.Key, row)).Select(entry => entry.Value).FirstOrDefault();
                return instance != null && instance.activeInHierarchy;
            }

            internal List<string> DisposeFailures = new List<string>();

            // Repeatable: a later disposal attempts every release again.
            public void Dispose()
            {
                DisposeFailures = Unhook();
                if (ReferenceEquals(active, this)) active = null;
                current = null; creation = null;
            }
        }

        private static JArray S7Vector(Vector3 value)
        {
            return new JArray(Math.Round(value.x, 5), Math.Round(value.y, 5), Math.Round(value.z, 5));
        }

        private static readonly Type S7ParticleSystem = Type.GetType("UnityEngine.ParticleSystem, UnityEngine.ParticleSystemModule", false);

        private static string S7Colour(object value)
        {
            if (value is Color) { var c = (Color)value; return string.Format(CultureInfo.InvariantCulture, "{0:F4},{1:F4},{2:F4},{3:F4}", c.r, c.g, c.b, c.a); }
            var gradient = value as Gradient;
            if (gradient != null)
                return "[" + string.Join(";", gradient.colorKeys.Select(key => S7Colour(key.color) + "@" + key.time.ToString("F3", CultureInfo.InvariantCulture)).ToArray()) + "|" +
                    string.Join(";", gradient.alphaKeys.Select(key => key.alpha.ToString("F4", CultureInfo.InvariantCulture) + "@" + key.time.ToString("F3", CultureInfo.InvariantCulture)).ToArray()) + "]";
            return value == null ? "null" : value.ToString();
        }

        private static string S7MinMaxGradient(object value)
        {
            if (value == null) return "null";
            Type type = value.GetType();
            return string.Join(" ", new[] { "mode", "color", "colorMin", "colorMax", "gradient", "gradientMin", "gradientMax" }
                .Select(name => { var property = type.GetProperty(name); return name + "=" + (property == null ? "-" : S7Colour(property.GetValue(value, null))); }).ToArray());
        }

        // Shared materials (instance and name) per renderer and the start and
        // over-lifetime colours of every particle system, with a hash.
        private static JObject S7FxSignature(GameObject instance)
        {
            var parts = new List<string>();
            foreach (Renderer renderer in instance.GetComponentsInChildren<Renderer>(true))
                parts.Add("R " + renderer.name + ":" + string.Join(",", renderer.sharedMaterials.Select(material => material == null ? "null" :
                    material.GetInstanceID().ToString(CultureInfo.InvariantCulture) + "/" + material.name).ToArray()));
            int systems = 0;
            if (S7ParticleSystem != null)
                foreach (Component system in instance.GetComponentsInChildren(S7ParticleSystem, true))
                {
                    systems++;
                    object main = S7ParticleSystem.GetProperty("main").GetValue(system, null);
                    object lifetime = S7ParticleSystem.GetProperty("colorOverLifetime").GetValue(system, null);
                    parts.Add("P " + system.name + " start{" + S7MinMaxGradient(main.GetType().GetProperty("startColor").GetValue(main, null)) +
                        "} life{" + lifetime.GetType().GetProperty("enabled").GetValue(lifetime, null) + " " +
                        S7MinMaxGradient(lifetime.GetType().GetProperty("color").GetValue(lifetime, null)) + "}");
                }
            string text = string.Join("\n", parts.ToArray());
            string hash;
            using (var sha = System.Security.Cryptography.SHA256.Create())
                hash = BitConverter.ToString(sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(text))).Replace("-", "").ToLowerInvariant();
            return new JObject { ["hash"] = hash, ["renderers"] = parts.Count(value => value.StartsWith("R ", StringComparison.Ordinal)),
                ["particleSystems"] = systems, ["dustClones"] = parts.Count(value => value.Contains("_KMG_Dust")) };
        }

        private sealed class S7SaveLog : IGlobalRulebookHandler<RuleSavingThrow>
        {
            internal readonly List<KeyValuePair<JObject, Kingmaker.UnitLogic.Mechanics.MechanicsContext>> Rows =
                new List<KeyValuePair<JObject, Kingmaker.UnitLogic.Mechanics.MechanicsContext>>();
            public void OnEventAboutToTrigger(RuleSavingThrow evt) { }
            public void OnEventDidTrigger(RuleSavingThrow evt)
            {
                if (evt == null || evt.Initiator == null) return;
                Rows.Add(new KeyValuePair<JObject, Kingmaker.UnitLogic.Mechanics.MechanicsContext>(new JObject {
                    ["unit"] = evt.Initiator.UniqueId, ["stat"] = evt.StatType.ToString(), ["dc"] = evt.DifficultyClass,
                    ["d20"] = (int)evt.D20, ["roll"] = evt.RollResult, ["passed"] = evt.IsPassed, ["frame"] = Time.frameCount },
                    evt.Reason == null ? null : evt.Reason.Context));
            }
        }

        private static bool S7InCone(UnitEntityData caster, Vector3 aim, UnitEntityData unit, float feet)
        {
            Vector3 toUnit = unit.Position - caster.Position; toUnit.y = 0f;
            Vector3 axis = aim - caster.Position; axis.y = 0f;
            float reach = feet * .3048f + unit.View.Corpulence;
            return toUnit.magnitude <= reach && Vector3.Angle(axis, toUnit) <= 45f;
        }

        // R3-F3: the cell's Sickened ledger over every unit in the area.
        // R3-F3: the cell's Sickened ledger over every unit in the area. A
        // condition is the cell's when its context is (or derives from) an
        // ability execution the cell's own units delivered. Native Sickened does
        // not stack: a later application refreshes an existing instance, which
        // keeps its first context, so every delivering execution of the cell's
        // units counts (the Ice breaths the cell commands too), each captured at
        // delivery, while its caster is alive (an expired summon's context later
        // reads no caster).
        private sealed class S7DustConditions : IApplyAbilityEffectHandler, IDisposable
        {
            private readonly BlueprintBuff _sickened;
            private readonly List<AbilityExecutionContext> _executions = new List<AbilityExecutionContext>();
            // Owner amendment 3 (R4-F1), recovery C3 (R7-F2): the commands this cell issued and the exact execution processes
            // they started, retained when first seen and before any interruption can clear a command's field; each command is
            // released on its own.
            private readonly Sprint7OwnedCommands<UnitUseAbility, Kingmaker.Controllers.AbilityExecutionProcess> _owned;
            private readonly Sprint7ConditionOwner<Buff> _conditions;
            private bool _subscribed, _subscribeAttempted;

            /// <param name="publish">Recovery C3 (R1-F3, R2-F8): receives each command-release and condition-release failure the moment it happens.</param>
            internal S7DustConditions(BlueprintBuff sickened, Action<string> publish)
            {
                _sickened = sickened;
                // Owner amendment 4 §2A (C3 review R4-F1): cancellation before execution is established only by a finished command
                // that never acted and holds no process (UnitUseAbility assigns ExecutionProcess only in OnAction, immediately before
                // IsActed is set; Interrupt finishes a command). A missing or destroyed executor establishes nothing.
                _owned = new Sprint7OwnedCommands<UnitUseAbility, Kingmaker.Controllers.AbilityExecutionProcess>(command => command.ExecutionProcess, process => process.IsEnded, publish,
                    command => command.IsFinished && !command.IsActed && command.ExecutionProcess == null);
                _conditions = new Sprint7ConditionOwner<Buff>(Present, Attribution, Remove, Identity, publish);
            }

            /// <summary>R1-F9: the subscription is a separate acquisition; its attempt is recorded before it runs.</summary>
            internal void Subscribe() { _subscribeAttempted = true; EventBus.Subscribe(this); _subscribed = true; }

            internal Sprint7OwnedCommands<UnitUseAbility, Kingmaker.Controllers.AbilityExecutionProcess> Commands { get { return _owned; } }

            internal List<Buff> Acquired { get { return _conditions.Ledger.Acquired; } }
            internal int LastRemoved { get { return _conditions.Ledger.LastRemoved; } }
            internal int OwnedExecutions { get { return _executions.Count; } }
            internal bool Subscribed { get { return _subscribed; } }

            /// <summary>A command this cell issues; registered before it runs.</summary>
            internal void Own(UnitUseAbility command) { _owned.Own(command); }

            /// <summary>Retains the current execution process of an owned command (every owned command when null).</summary>
            internal void Capture(UnitUseAbility command = null) { _owned.Capture(command); }

            internal List<Kingmaker.Controllers.AbilityExecutionProcess> Processes { get { return _owned.Processes; } }

            internal bool Retains(Kingmaker.Controllers.AbilityExecutionProcess process) { return _owned.Retains(process); }

            /// <summary>Whether an effect of this command's own execution has been delivered (by its retained process).</summary>
            internal bool Delivered(UnitUseAbility command)
            {
                // R2-F6: the retained association, not the command's mutable field.
                Capture(command);
                Kingmaker.Controllers.AbilityExecutionProcess process = command == null ? null : _owned.ProcessFor(command);
                return process != null && _executions.Any(value => ReferenceEquals(value, process.Context));
            }

            private static AbilityExecutionContext ExecutionOf(Buff buff)
            {
                Kingmaker.UnitLogic.Mechanics.MechanicsContext context = buff.Context;
                return context as AbilityExecutionContext ?? (context == null ? null : context.SourceAbilityContext);
            }

            /// <summary>Recovery C3 (R2-F3): conditions this cell acquired from the exact execution, and those still present.</summary>
            internal int AcquiredFrom(Kingmaker.Controllers.AbilityExecutionProcess process)
            {
                return process == null ? 0 : _conditions.Ledger.Acquired.Count(buff => ReferenceEquals(ExecutionOf(buff), process.Context));
            }

            /// <summary>
            /// Audit A2-F2: the exact execution's conditions still present, through the condition owner's count: an unreadable
            /// attribution or context makes it unknown (null), never zero.
            /// </summary>
            internal int? PresentFrom(Kingmaker.Controllers.AbilityExecutionProcess process)
            {
                if (process == null) return 0;
                object context;
                try { context = process.Context; } catch (Exception) { return null; }
                return _conditions.PresentFrom(context, buff => ExecutionOf(buff));
            }

            /// <summary>R2-F8 (audit A1-F8): the early release; each failure, bound to its instance, enters the cell's result at once.</summary>
            internal List<string> ReleaseAndPublish(string context) { return _conditions.ReleaseAndPublish(context); }

            /// <summary>True when the exact process has ended, false while it runs, null when it cannot be read.</summary>
            internal static bool? Settled(Kingmaker.Controllers.AbilityExecutionProcess process)
            {
                try { return process == null ? (bool?)null : process.IsEnded; } catch (Exception) { return null; }
            }

            internal static string Identity(Kingmaker.Controllers.AbilityExecutionProcess process)
            {
                string ability = null;
                try { ability = process.Context == null || process.Context.AbilityBlueprint == null ? null : process.Context.AbilityBlueprint.name; } catch (Exception) { }
                return (ability ?? "ability?") + "#" + System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(process);
            }

            // Only the execution of a command this cell issued, captured at delivery.
            public void OnTryToApplyAbilityEffect(AbilityExecutionContext context, TargetWrapper target)
            {
                if (context == null || _executions.Any(value => ReferenceEquals(value, context))) return;
                // R2-F6: retained processes first (each read on its own), then each owned command's field read on its own, so an
                // unreadable command cannot hide the delivery of a known execution.
                if (_owned.RetainsContext(context, process => process.Context)) { _executions.Add(context); return; }
                foreach (UnitUseAbility command in _owned.Commands)
                {
                    bool issued;
                    try { issued = command.ExecutionProcess != null && ReferenceEquals(command.ExecutionProcess.Context, context); } catch (Exception) { continue; }
                    if (issued) { _executions.Add(context); Capture(command); return; }
                }
            }
            public void OnAbilityEffectAppliedToTarget(AbilityExecutionContext context, TargetWrapper target) { }
            public void OnAbilityEffectApplied(AbilityExecutionContext context) { OnTryToApplyAbilityEffect(context, null); }

            /// <summary>
            /// R7-F2: releases each owned command (or the given ones) on its own: its execution process is retained first, then
            /// a command still running is interrupted by reference on its own unit. Returns the named failures; one failing
            /// interruption never skips another command.
            /// </summary>
            internal List<string> EndUnfinished(IEnumerable<UnitUseAbility> only = null)
            {
                Ended = 0;
                return _owned.ReleaseEach(Unfinished, Interrupt, Label, only);
            }

            internal static bool Unfinished(UnitUseAbility command) { return !command.IsFinished && command.Executor != null && !command.Executor.Destroyed; }

            internal void Interrupt(UnitUseAbility command)
            {
                command.Executor.Commands.InterruptAll(value => ReferenceEquals(value, command));
                // Audit A1-F6: queue maintenance starts the queue head; it runs only while the queue holds no command outside this cell's.
                if (command.Executor.Commands.Queue.All(value => value is UnitUseAbility && _owned.Commands.Contains((UnitUseAbility)value))) command.Executor.Commands.RemoveFinishedAndUpdateQueue();
                Ended++;
            }

            internal static string Label(UnitUseAbility command)
            {
                return "owned command " + (command.Executor == null ? "?" : command.Executor.UniqueId) + "#" + System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(command);
            }

            /// <summary>Commands interrupted by the last release.</summary>
            internal int Ended { get; private set; }

            internal int UnfinishedLeft()
            {
                return _owned.Commands.Count(value => !value.IsFinished && value.Executor != null && !value.Executor.Destroyed && value.Executor.Commands.Contains(value));
            }

            /// <summary>R4-F1: retained execution processes not observed ended (running, or unreadable).</summary>
            internal int LiveExecutions() { return _owned.Live().Count; }

            // R4-F1 (abandonment check only): every removal of one owned instance fails while set; the instance is kept
            // for the observer's own repair after the verdict has been read.
            internal static string Inject;
            internal static readonly List<Buff> InjectedLeftovers = new List<Buff>();
            internal static readonly List<string> InjectedIdentities = new List<string>();

            /// <summary>Recovery C3 (R1-F6): one condition instance's identity: blueprint, instance and owner.</summary>
            internal static string Identity(Buff buff)
            {
                string owner;
                try { owner = buff.Owner == null || buff.Owner.Unit == null ? "?" : buff.Owner.Unit.UniqueId; } catch (Exception) { owner = "?"; }
                return (buff.Blueprint == null ? "buff" : buff.Blueprint.name) + "#" + System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(buff) + " on " + owner;
            }

            /// <summary>The injected removal fault's text; the instance's identity is added at the removal boundary (R2-F8).</summary>
            internal const string RemovalFault = "injected failure of an owned Sickened removal";

            private static void Remove(Buff buff)
            {
                if (Inject == "fail-owned-removal")
                {
                    if (InjectedLeftovers.Count == 0) { InjectedLeftovers.Add(buff); InjectedIdentities.Add(Identity(buff)); }
                    if (InjectedLeftovers.Contains(buff)) throw new InvalidOperationException(RemovalFault);
                }
                buff.Remove();
            }

            public void Dispose() { if (_subscribeAttempted) { EventBus.Unsubscribe(this); _subscribed = false; _subscribeAttempted = false; } }

            private IEnumerable<Buff> Present()
            {
                return Game.Instance.State.Units.Where(unit => unit != null && !unit.Destroyed)
                    .SelectMany(unit => unit.Descriptor.Buffs.Enumerable.Where(buff => ReferenceEquals(buff.Blueprint, _sickened))).ToList();
            }

            private bool? Attribution(Buff buff)
            {
                Kingmaker.UnitLogic.Mechanics.MechanicsContext context = buff.Context;
                AbilityExecutionContext execution = context as AbilityExecutionContext ?? (context == null ? null : context.SourceAbilityContext);
                // R3-F2: owned when its execution delivered under observation, or when it is the context of a retained owned
                // process (including one recovered after a missed notification); observed deliveries stay separate (Delivered).
                // Audit A1-F1: an unreadable retained context leaves it unresolved (named, never released, never a clean result).
                return Sprint7ObserverPolicy.AttributedTo(execution, _executions.Cast<object>(), _owned, process => process.Context);
            }

            internal bool Owns(Buff buff) { return buff != null && _conditions.Owned(buff); }

            internal int Observe() { return _conditions.Observe(); }
            internal List<string> Release() { return _conditions.Release(); }
            internal int Remaining() { return _conditions.Remaining(); }

            /// <summary>R1-F2: the identities of the owned conditions still present, and of unresolved attributions.</summary>
            internal List<string> RemainingIdentities() { return _conditions.RemainingIdentities(); }
        }

        // One Dust cast that owns its command and its conditions: however it
        // ends (normally, or abandoned while yielded), the command is ended if
        // still running and the conditions it introduced are released. R4-F1:
        // the release failures also go to the cell's own failure list, which
        // the restoration verdict reads, and a still-running execution is
        // reported.
        private IEnumerable<int> S7DustCastOwningConditions(UnitEntityData dust, AbilityData data, UnitEntityData target, S7DustConditions ledger, JObject row,
            JArray cellFailures)
        {
            var command = new UnitUseAbility(data, new TargetWrapper(target));
            // Owner amendment 3 (R4-F1): this cast's own execution process, retained when first seen.
            Kingmaker.Controllers.AbilityExecutionProcess own = null;
            try
            {
                // Audit A2-F3, amendment 4 §2B: the shared submission registers it with the cell's owner and the phase record
                // (with the owner's capture), then runs it once.
                S7SubmitDust(ledger, dust, command);
                DateTime deadline = DateTime.UtcNow.AddSeconds(20);
                while (DateTime.UtcNow < deadline && !(command.IsFinished && (command.ExecutionProcess == null || command.ExecutionProcess.IsEnded)))
                { if (own == null) own = command.ExecutionProcess; ledger.Capture(command); yield return 0; }
                if (own == null) own = command.ExecutionProcess;
                ledger.Capture(command);
                // One settle frame before the ordinary release, so an owner can see the application and exit here.
                yield return 0;
            }
            finally
            {
                if (own == null) own = command.ExecutionProcess;
                ledger.Capture(command);
                row["commandFinishedAtExit"] = command.IsFinished;
                // Every release is attempted: the command, then the conditions.
                // Recovery C3 (R1-F3, R1-F4): the command through its owner: process retained first, a failure published at once.
                row["commandFailures"] = new JArray(ledger.EndUnfinished(new[] { command }).ToArray());
                var failures = new List<string>();
                row["execution"] = own == null ? null : S7DustConditions.Identity(own);
                row["executionLiveAtExit"] = own != null && S7DustConditions.Settled(own) != true;
                // Audit A1-F8: the shared early release publishes each failure (bound to its instance) once, at once.
                failures.AddRange(ledger.ReleaseAndPublish("early-exit cast: "));
                row["releaseFailures"] = new JArray(failures);
                foreach (string failure in failures) cellFailures.Add("early-exit cast: " + failure);
            }
        }

        // 9. Early exit after application (R4-F1, round 6): one cast owned by
        // the cell's own ledger (no child ledger), whose enumerator is disposed
        // as soon as its Sickened is seen. Its cleanup writes its release
        // failures to the cell's failure list whichever way the cell unwinds.
        // With an abandon probe the cell stays yielded here, the condition
        // applied and the cast's cleanup still pending, until its enclosing
        // runner is disposed.
        private IEnumerable<int> S7DustEarlyExit(UnitEntityData caster, AbilityData data, UnitEntityData target, S7DustConditions conditions,
            JArray cellFailures, JObject early, Func<JObject> diagnostics, JObject abandonProbe)
        {
            early["preexistingCellOwned"] = conditions.Remaining();
            int acquiredFrom = conditions.Acquired.Count;
            IEnumerator<int> run = S7DustCastOwningConditions(caster, data, target, conditions, early, cellFailures).GetEnumerator();
            bool applied = false;
            try
            {
                DateTime deadline = DateTime.UtcNow.AddSeconds(20);
                while (DateTime.UtcNow < deadline)
                {
                    bool more = run.MoveNext();
                    if (conditions.Observe() > 0) { applied = true; break; }
                    if (!more) break;
                    yield return 0;
                }
                early["appliedBeforeExit"] = applied;
                early["acquiredBeforeExit"] = conditions.Acquired.Count - acquiredFrom;
                if (diagnostics != null) early["diagnostics"] = diagnostics();
                if (abandonProbe != null)
                {
                    abandonProbe["applied"] = applied;
                    abandonProbe["caster"] = caster.UniqueId;
                    abandonProbe["ownedLiveAtAbandon"] = conditions.Remaining();
                    while (abandonProbe != null) yield return 0;
                }
            }
            finally { run.Dispose(); }
        }

        // R3-F4: a trace's disposal and its verdict: every release is attempted;
        // a failed release or a remaining owned hook fails the verdict.
        private static bool S7DisposeTrace(S7ProjectileTrace trace, JObject row) { return S7DisposeTrace(trace, row, new List<string>()); }

        /// <summary>
        /// R1-F5: the disposal runs whatever the counts read; each count failure and each release failure is kept individually in
        /// <paramref name="failures"/> for the caller's cumulative result, and any of them fails the verdict.
        /// </summary>
        private static bool S7DisposeTrace(S7ProjectileTrace trace, JObject row, List<string> failures)
        {
            var reads = new List<string>();
            int? before; try { before = trace.OwnedHooks(); } catch (Exception error) { before = null; reads.Add("count before disposal: " + error.GetType().Name + ": " + error.Message); }
            row["ownedHooksBeforeDispose"] = before;
            trace.Dispose();
            int? after; try { after = trace.OwnedHooks(); } catch (Exception error) { after = null; reads.Add("count after disposal: " + error.GetType().Name + ": " + error.Message); }
            row["ownedHooksAfterDispose"] = after;
            row["disposeFailures"] = new JArray(trace.DisposeFailures);
            row["countFailures"] = new JArray(reads.ToArray());
            failures.AddRange(trace.DisposeFailures);
            failures.AddRange(reads);
            bool released = after.HasValue && reads.Count == 0 && Sprint7ObserverPolicy.Released(trace.DisposeFailures, after.Value);
            row["traceReleased"] = released;
            return released;
        }

        private IEnumerable<int> S7WaitDustReleased()
        {
            DateTime deadline = DateTime.UtcNow.AddSeconds(20);
            while (DateTime.UtcNow < deadline && DustMephitBreathTint.Live > 0) yield return 0;
            foreach (int step in R6Seconds(1f)) yield return step;
        }

        private IEnumerable<int> S7Dust(ExpandedSummoningCorrectionFixture fixture, Func<string, UnitEntityData> summon, UnitEntityData hostile,
            JObject record, Action<string> mark, Func<Vector3, Vector3> ground, R4MephitRecorder recorder,
            Func<UnitEntityData, BlueprintAbility, AbilityData> abilityData, IDictionary<string, BlueprintScriptableObject> symbols, Sprint26Saves hostileSaves,
            Sprint7CellIsolation isolation, JObject abandonProbe = null)
        {
            // Owner amendment 3 (R4-F1): registered with the enclosing isolation before anything is acquired.
            isolation.Opened("s7-dust");
            var row = new JObject(); record["dust"] = row;
            // In the abandon case (R4-F1) the cell's verdicts are recorded for the enclosing check instead of reported.
            Action<string, bool, JObject, string> dustCheck = (name, passed, observed, expected) =>
            {
                if (abandonProbe == null) Sprint25Check(name, passed, observed, expected);
                else { var deferred = (JObject)(row["deferredChecks"] ?? (row["deferredChecks"] = new JObject())); deferred[name] = passed; }
            };
            var breath = (BlueprintAbility)symbols["KMG.Summoning.Special.DustMephit.Breath"];
            var iceBreath = (BlueprintAbility)symbols["KMG.Summoning.Special.IceMephit.Breath"];
            var sickened = ResourcesLibrary.TryGetBlueprint<BlueprintBuff>("4e42460798665fd4cb9173ffa7ada323");
            if (sickened == null) throw new InvalidOperationException("Native Sickened buff missing.");
            UnitEntityData dust = summon("dust-mephit"), ice = summon("ice-mephit"), spare = summon("dust-mephit");
            UnitEntityData outside = null; Sprint26Saves outsideSaves = null; bool grantedIce = false;
            var saveLog = new S7SaveLog();
            // Recovery C3 (R1-F9): each set-up acquisition's release is registered before it runs.
            var setup = new Sprint7SetupScope();
            S7ProjectileTrace trace = null;
            // Recovery C3 (R7-F3): the trace attempt is registered (by its caller-chosen identity) before its set-up runs.
            var traceAttempt = new S7TraceAttempt();
            Func<UnitEntityData, string> label = unit => unit == null ? null : ReferenceEquals(unit, hostile) ? "enemy-inside" :
                ReferenceEquals(unit, outside) ? "enemy-outside" : ReferenceEquals(unit, ice) ? "ally-inside" :
                ReferenceEquals(unit, fixture.Caster) ? "caster-behind" : ReferenceEquals(unit, dust) ? "dust" : unit.Blueprint.name;
            // R3-F3: every Sickened instance a Dust breath execution of this
            // cell's own Dust units introduced, however that cast ended, is
            // acquired by the cell's ledger; only those (and, at the very end,
            // the observer's own control) are removed.
            S7DustConditions conditions = null;
            Buff foreignSickened = null, unownedSickened = null;
            bool unownedControlDue = false;
            var releaseFailures = new JArray();
            Action clearSickened = () => { foreach (string failure in conditions.ReleaseAndPublish("")) releaseFailures.Add(failure); };
            // Every command this cell issues is registered with the ledger before it runs (R4-F4). Audit A1-F5, A2-F3, amendment 4
            // §2B: both routes submit through the one production operation, which registers the command with the cell's owner and
            // the phase record (with the owner's capture, which a phase retry runs before interrupting) and performs the single
            // native run (on the passive route, R6RunPassive hands its run to that submission).
            Func<UnitEntityData, AbilityData, TargetWrapper, JObject, R6Clip, IEnumerable<int>> passive = (unit, data, target, cast, clip) =>
                R6RunPassive(unit, data, target, cast, clip, command => S7SubmitDust(conditions, unit, command));
            Func<UnitEntityData, UnitUseAbility, UnitUseAbility> issue = (unit, command) =>
            {
                S7SubmitDust(conditions, unit, command);
                return command;
            };
            Func<UnitEntityData, int, int, JObject> laterCast = (caster, savesFrom, damageFrom) => new JObject {
                ["caster"] = caster.UniqueId, ["casterAlive"] = !caster.Destroyed, ["casterIsEnemyOfHostile"] = caster.IsEnemy(hostile), ["hostileIsEnemyOfCaster"] = hostile.IsEnemy(caster),
                ["hostileSaves"] = new JArray(saveLog.Rows.Skip(savesFrom).Where(value => (string)value.Key["unit"] == hostile.UniqueId).Select(value => { var copy = (JObject)value.Key.DeepClone();
                    var context = value.Value; copy["contextAbility"] = context == null ? null : (context.SourceAbilityContext ?? context as AbilityExecutionContext)?.AbilityBlueprint?.name;
                    copy["contextCaster"] = context == null || context.MaybeCaster == null ? null : context.MaybeCaster.UniqueId; return copy; })),
                ["hostileDamage"] = new JArray(recorder.Damage.Skip(damageFrom).Where(value => ReferenceEquals(value.Target, hostile)).Select(value => new JObject {
                    ["initiator"] = value.Initiator == null ? null : value.Initiator.UniqueId, ["total"] = value.Damage })),
                ["sickened"] = new JArray(Game.Instance.State.Units.Where(unit => unit != null && !unit.Destroyed).SelectMany(unit => unit.Descriptor.Buffs.Enumerable
                    .Where(buff => ReferenceEquals(buff.Blueprint, sickened)).Select(buff => new JObject { ["unit"] = label(unit),
                        ["contextAbility"] = buff.Context == null ? null : (buff.Context.SourceAbilityContext ?? buff.Context as AbilityExecutionContext)?.AbilityBlueprint?.name,
                        ["contextCaster"] = buff.Context == null || buff.Context.MaybeCaster == null ? null : buff.Context.MaybeCaster.UniqueId,
                        ["secondsLeft"] = Math.Round(buff.TimeLeft.TotalSeconds, 2), ["foreignControl"] = ReferenceEquals(buff, foreignSickened) }))) };
            try
            {
                setup.Acquire("save log subscription", () => EventBus.Subscribe(saveLog), () => EventBus.Unsubscribe(saveLog));
                // R1-F3: each command-release failure enters the cell's cumulative result the moment it happens.
                conditions = new S7DustConditions(sickened, failure => isolation.Fail("s7-dust", failure));
                // R1-F9: the ledger exists before its subscription is attempted; its release is the effects release below.
                conditions.Subscribe();
                row["conditionsAcquired"] = true;
                Vector3 spot = ground(hostile.Position + Vector3.back * 2.6f);
                PlaceExpandedSummoningUnit(dust, spot);
                // An ally inside the cone beside the enemy, a second enemy
                // outside it, and the caster behind: position and allegiance
                // are separated.
                PlaceExpandedSummoningUnit(ice, ground(spot + Vector3.forward * 3.2f + Vector3.right * 1.4f));
                PlaceExpandedSummoningUnit(spare, ground(hostile.Position + Vector3.left * 7f + Vector3.back * 5f));
                PlaceExpandedSummoningUnit(fixture.Caster, ground(spot + Vector3.back * 3f));
                outside = Game.Instance.EntityCreator.SpawnUnit(fixture.HostileBlueprint ?? hostile.Blueprint,
                    ground(spot + Vector3.left * 3.6f + Vector3.back * .6f), Quaternion.identity, fixture.Scene);
                // R3-F3: the outside target's retirement obligation is the phase's from acquisition (its teardown retires it if the
                // cell does not), through the one retirement record.
                if (_s7PhaseOwned != null) _s7PhaseOwned.Add(outside);
                Sprint28SetBrainActivePreservingActions(outside, false);
                outside.Stats.HitPoints.BaseValue = 100000;
                // R1-F9: the handler exists before its subscription is attempted; the subscription's release is registered first.
                outsideSaves = new Sprint26Saves(outside, false) { DesiredRoll = 1 };
                Sprint26Saves outsideHandler = outsideSaves;
                setup.Acquire("outside saves subscription", outsideHandler.Subscribe, outsideHandler.Dispose);
                // A control: Sickened from another context on the caster behind
                // the cone (not a summon, so it outlives the cell), which every
                // reset and the cleanup must leave in place.
                foreignSickened = fixture.Caster.Descriptor.AddBuff(sickened, fixture.Caster, TimeSpan.FromMinutes(30));
                foreach (int step in R6Seconds(2f)) yield return step;
                Vector3 aim = hostile.Position;
                row["layout"] = new JObject {
                    ["enemyInside"] = new JObject { ["inCone"] = S7InCone(dust, aim, hostile, 15f), ["enemy"] = dust.IsEnemy(hostile) },
                    ["allyInside"] = new JObject { ["inCone"] = S7InCone(dust, aim, ice, 15f), ["enemy"] = dust.IsEnemy(ice) },
                    ["enemyOutside"] = new JObject { ["inCone"] = S7InCone(dust, aim, outside, 15f), ["enemy"] = dust.IsEnemy(outside) },
                    ["casterBehind"] = new JObject { ["inCone"] = S7InCone(dust, aim, fixture.Caster, 15f), ["enemy"] = dust.IsEnemy(fixture.Caster) } };
                var resource = breath.GetComponent<Kingmaker.UnitLogic.Abilities.Components.AbilityResourceLogic>();
                row["breathBlueprint"] = new JObject { ["actionType"] = breath.ActionType.ToString(), ["resource"] = resource == null ? null : resource.RequiredResource.name,
                    ["range"] = breath.Range.ToString() };
                trace = traceAttempt.Acquire();
                row["traceAcquired"] = true;
                if (abandonProbe != null && (string)abandonProbe["mode"] == "pre-delivery")
                {
                    // Recovery C3 (R1-F2): an ordinary owned Dust cast, its enclosing cell disposed after the execution started
                    // and before its first delivery; the cell's release must account for anything it delivers later.
                    hostileSaves.DesiredRoll = 1;
                    var pending = issue(dust, new UnitUseAbility(abilityData(dust, breath), new TargetWrapper(hostile)));
                    DateTime pendingBy = DateTime.UtcNow.AddSeconds(20);
                    while (DateTime.UtcNow < pendingBy && !pending.IsFinished && pending.ExecutionProcess == null) yield return 0;
                    conditions.Capture(pending);
                    Kingmaker.Controllers.AbilityExecutionProcess pendingProcess = conditions.Commands.ProcessFor(pending);
                    S7DustConditions probeConditions = conditions;
                    abandonProbe["processStarted"] = pendingProcess != null;
                    abandonProbe["execution"] = pendingProcess == null ? null : S7DustConditions.Identity(pendingProcess);
                    abandonProbe["deliveredBeforeDisposal"] = conditions.Delivered(pending);
                    abandonProbe["foreignControl"] = foreignSickened == null ? null : S7DustConditions.Identity(foreignSickened);
                    abandonProbe["readyFrame"] = Time.frameCount;
                    // R2-F3: the driver reads this at the disposal itself, and again after settlement, bound to the exact execution.
                    _s7PreDeliveryState = () => new JObject { ["frame"] = Time.frameCount, ["execution"] = pendingProcess == null ? null : S7DustConditions.Identity(pendingProcess),
                        ["ended"] = S7DustConditions.Settled(pendingProcess), ["delivered"] = probeConditions.Delivered(pending),
                        ["acquiredFromExecution"] = probeConditions.AcquiredFrom(pendingProcess), ["presentFromExecution"] = probeConditions.PresentFrom(pendingProcess) };
                    abandonProbe["ready"] = true;
                    while (abandonProbe != null) yield return 0;
                }
                if (abandonProbe != null)
                {
                    // R4-F1: the cell's own early-exit cast (step 9), held right after it applied Sickened.
                    var abandonedEarly = new JObject(); row["earlyExit"] = abandonedEarly;
                    hostileSaves.DesiredRoll = 1;
                    abandonProbe["hooksInstalled"] = trace.OwnedHooks();
                    foreach (int step in S7DustEarlyExit(dust, abilityData(dust, breath), hostile, conditions, releaseFailures, abandonedEarly, null, abandonProbe)) yield return step;
                    yield break;
                }

                // 1. Consecutive committed breaths from one unchanged position at
                // normal speed; Reflex failure, success, failure.
                var casts = new JArray(); row["casts"] = casts;
                var castRows = new List<JObject>();
                for (int index = 0; index < 3; index++)
                {
                    int desired = index == 1 ? 20 : 1;
                    var cast = new JObject { ["index"] = index, ["desiredReflexD20"] = desired }; casts.Add(cast);
                    hostileSaves.DesiredRoll = desired;
                    hostile.Descriptor.Damage = 0; ice.Descriptor.Damage = 0; outside.Descriptor.Damage = 0; clearSickened();
                    int damageFrom = recorder.Damage.Count, appliedFrom = recorder.Applied.Count, deliveredFrom = recorder.Delivered.Count,
                        savesFrom = saveLog.Rows.Count, creationsFrom = trace.Creations.Count, appliedTints = DustMephitBreathTint.Applied;
                    cast["unitPosition"] = S7Vector(dust.Position);
                    cast["resourceBefore"] = resource == null ? (int?)null : dust.Descriptor.Resources.GetResourceAmount(resource.RequiredResource);
                    foreach (int step in R6Wait(4)) yield return step;
                    mark("dust-cast-" + index + "-start");
                    // Recovery C3 (R1-F12): the mechanics-only selection captures no frames.
                    using (var clip = _s7Mechanics ? null : new R6Clip(_request.EvidenceDirectory, "s7-dust-cast-" + index, () => R6RenderBounds(dust, hostile, ice), null))
                    {
                        foreach (int step in passive(dust, abilityData(dust, breath), new TargetWrapper(hostile), cast, clip)) yield return step;
                        if (clip != null) { cast["sceneryHitsOnBearing"] = clip.Occluded; cast["outOfFrame"] = clip.OutOfFrame; cast["frames"] = clip.Count; }
                        else cast["framing"] = "not captured (mechanics-only selection)";
                    }
                    cast["resourceAfter"] = resource == null ? (int?)null : dust.Descriptor.Resources.GetResourceAmount(resource.RequiredResource);
                    cast["availableAfter"] = abilityData(dust, breath).IsAvailable;
                    // The native notice is raised once per delivery target; the execution is the distinct context.
                    var contexts = recorder.Applied.Skip(appliedFrom).Where(value => ReferenceEquals(value.MaybeCaster, dust) && ReferenceEquals(value.AbilityBlueprint, breath)).Distinct().ToList();
                    AbilityExecutionContext context = contexts.Count == 1 ? contexts[0] : null;
                    Func<Kingmaker.UnitLogic.Mechanics.MechanicsContext, bool> mine = value => value != null && context != null &&
                        (ReferenceEquals(value, context) || ReferenceEquals(value.SourceAbilityContext, context));
                    cast["executions"] = contexts.Count;
                    cast["selected"] = new JArray(recorder.Delivered.Skip(deliveredFrom).Where(value => ReferenceEquals(value.Key, context)).Select(value => label(value.Value)).Distinct());
                    cast["damage"] = new JArray(recorder.Damage.Skip(damageFrom).Where(value => value.Reason != null && mine(value.Reason.Context)).Select(value => new JObject {
                        ["target"] = label(value.Target), ["total"] = value.Damage, ["halfBecauseSave"] = value.HalfBecauseSavingThrow,
                        ["parts"] = new JArray(value.DamageBundle.Select(part => new JObject { ["type"] = part.Type.ToString(),
                            ["form"] = part is Kingmaker.RuleSystem.Rules.Damage.PhysicalDamage ? ((Kingmaker.RuleSystem.Rules.Damage.PhysicalDamage)part).Form.ToString() : null,
                            ["dice"] = part.Dice.Rolls + "d" + part.Dice.Dice.ToString(), ["bonus"] = part.Bonus })) }));
                    cast["otherDamage"] = recorder.Damage.Skip(damageFrom).Count(value => ReferenceEquals(value.Initiator, dust) && !(value.Reason != null && mine(value.Reason.Context)));
                    cast["saves"] = new JArray(saveLog.Rows.Skip(savesFrom).Where(value => mine(value.Value)).Select(value => { var copy = (JObject)value.Key.DeepClone(); copy["unit"] = label(new[] { hostile, ice, outside, fixture.Caster }.FirstOrDefault(unit => unit != null && unit.UniqueId == (string)value.Key["unit"])) ?? (string)value.Key["unit"]; return copy; }));
                    var castSickened = new[] { hostile, ice, outside, fixture.Caster }.SelectMany(unit => unit.Descriptor.Buffs.Enumerable
                        .Where(buff => ReferenceEquals(buff.Blueprint, sickened) && mine(buff.Context)).Select(buff => new { Unit = unit, Buff = buff })).ToList();
                    conditions.Observe();
                    cast["sickened"] = new JArray(castSickened.Select(value => new JObject { ["unit"] = label(value.Unit), ["secondsLeft"] = Math.Round(value.Buff.TimeLeft.TotalSeconds, 2) }));
                    var creations = trace.Creations.Skip(creationsFrom).OfType<JObject>().Where(value => (string)value["launcher"] == dust.UniqueId).ToList();
                    cast["creations"] = new JArray(creations.Select(value => value.DeepClone()));
                    cast["tintsApplied"] = DustMephitBreathTint.Applied - appliedTints;
                    mark("dust-cast-" + index + "-released");
                    foreach (int step in S7WaitDustReleased()) yield return step;
                    cast["restored"] = new JArray(creations.SelectMany(value => ((JArray)value["effects"]).OfType<JObject>()).Select(effect => trace.Restoration(effect)));
                    cast["liveAfter"] = DustMephitBreathTint.Live;
                    mark("dust-cast-" + index + "-end");
                    castRows.Add(cast);
                }
                hostileSaves.DesiredRoll = 1;
                clearSickened();
                row["foreignSickenedAfterResets"] = foreignSickened != null && fixture.Caster.Descriptor.Buffs.Enumerable.Contains(foreignSickened);
                Func<JObject, IEnumerable<JObject>> effects = cast => ((JArray)cast["creations"]).OfType<JObject>().SelectMany(value => ((JArray)value["effects"]).OfType<JObject>());
                bool samePlace = castRows.All(cast => Vector3.Distance(new Vector3((float)cast["unitPosition"][0], (float)cast["unitPosition"][1], (float)cast["unitPosition"][2]),
                    new Vector3((float)castRows[0]["unitPosition"][0], (float)castRows[0]["unitPosition"][1], (float)castRows[0]["unitPosition"][2])) < .001f);
                bool presented = castRows.All(cast => (int)cast["executions"] == 1 && ((JArray)cast["creations"]).Count == 1 &&
                    (string)cast["creations"][0]["executingAbility"] == breath.AssetGuid &&
                    effects(cast).Count(effect => (bool)effect["isCastFx"]) == 1 && effects(cast).All(effect => (bool?)effect["tinted"] == true &&
                        (int)effect["recolouredMaterials"] + (int)effect["recolouredParticleColours"] > 0 && (float)effect["orientationDegrees"] < 1f) &&
                    ((JArray)cast["restored"]).All(value => (bool)value["exists"] && !(bool)value["tintPresent"] && (bool)value["matchesBefore"]) && (int)cast["liveAfter"] == 0);
                bool visible = _s7Mechanics || ((int)castRows[0]["sceneryHitsOnBearing"] == 0 && (int)castRows[0]["outOfFrame"] == 0);
                dustCheck("s7-dust-casts", samePlace && presented && visible, new JObject { ["casts"] = casts.DeepClone(), ["framingMeasured"] = !_s7Mechanics },
                    Sprint7ObserverPolicy.DustCastsExpectation(_s7Mechanics));
                bool neverCarried = castRows.All(cast => ((JArray)cast["creations"]).OfType<JObject>().All(value => (bool)value["previousLaunchPointCleared"] &&
                    (bool)value["callSiteInstalled"] && ((JArray)value["effects"]).OfType<JObject>().Count(effect => (bool)effect["isCastFx"]) == 1));
                dustCheck("s7-dust-repeat-launch-point", samePlace && neverCarried, new JObject { ["launchPoints"] = new JArray(castRows.Select(cast => ((JArray)cast["creations"]).Count == 1 ? cast["creations"][0]["launchPoint"].DeepClone() : null)),
                        ["previousLaunchPointAtEntry"] = new JArray(castRows.Select(cast => ((JArray)cast["creations"]).Count == 1 ? cast["creations"][0]["previousLaunchPoint"].DeepClone() : null)) },
                    "At each repeated breath the native controller's previous launch point is already cleared (ProjectileController.Tick resets it every frame), so native creation spawns every execution's own cast effect, through the product's installed call site.");
                Func<JObject, string, JObject[]> damageTo = (cast, who) => ((JArray)cast["damage"]).OfType<JObject>().Where(value => (string)value["target"] == who).ToArray();
                Func<JObject, string, JObject[]> savesBy = (cast, who) => ((JArray)cast["saves"]).OfType<JObject>().Where(value => (string)value["unit"] == who).ToArray();
                Func<JObject, string, bool> sickenedOn = (cast, who) => ((JArray)cast["sickened"]).OfType<JObject>().Any(value => (string)value["unit"] == who);
                bool mechanics = castRows.All(cast =>
                {
                    bool failed = (int)cast["desiredReflexD20"] == 1;
                    JObject[] hits = damageTo(cast, "enemy-inside"); JObject[] rolled = savesBy(cast, "enemy-inside");
                    bool hit = hits.Length == 1 && ((JArray)hits[0]["parts"]).Count == 1 && (string)hits[0]["parts"][0]["type"] == "Physical" &&
                        ((string)hits[0]["parts"][0]["form"] ?? "").Contains("Slashing") &&
                        ((string)hits[0]["parts"][0]["dice"]).EndsWith("D4", StringComparison.OrdinalIgnoreCase) && (string)hits[0]["parts"][0]["dice"] != null &&
                        (failed ? (int)hits[0]["total"] >= 1 && (int)hits[0]["total"] <= 4 && !(bool)hits[0]["halfBecauseSave"]
                                : (int)hits[0]["total"] >= 0 && (int)hits[0]["total"] <= 2 && (bool)hits[0]["halfBecauseSave"]);
                    bool save = rolled.Length == 1 && (string)rolled[0]["stat"] == "SaveReflex" && (int)rolled[0]["d20"] == (int)cast["desiredReflexD20"] && (bool)rolled[0]["passed"] == !failed;
                    bool condition = sickenedOn(cast, "enemy-inside") == failed && !sickenedOn(cast, "ally-inside") && !sickenedOn(cast, "enemy-outside") && !sickenedOn(cast, "caster-behind");
                    // Released behaviour, unchanged here: the breath's Reflex save is rolled for every creature
                    // the cone selects, allies included; damage and the condition apply to enemies only.
                    bool others = damageTo(cast, "ally-inside").Length == 0 && damageTo(cast, "enemy-outside").Length == 0 && damageTo(cast, "caster-behind").Length == 0 &&
                        savesBy(cast, "enemy-outside").Length == 0 && savesBy(cast, "caster-behind").Length == 0 && (int)cast["otherDamage"] == 0 &&
                        !((JArray)cast["selected"]).Values<string>().Contains("enemy-outside") && !((JArray)cast["selected"]).Values<string>().Contains("caster-behind");
                    return hit && save && condition && others;
                });
                bool layout = (bool)row["layout"]["enemyInside"]["inCone"] && (bool)row["layout"]["enemyInside"]["enemy"] &&
                    (bool)row["layout"]["allyInside"]["inCone"] && !(bool)row["layout"]["allyInside"]["enemy"] &&
                    !(bool)row["layout"]["enemyOutside"]["inCone"] && (bool)row["layout"]["enemyOutside"]["enemy"] && !(bool)row["layout"]["casterBehind"]["inCone"];
                dustCheck("s7-dust-mechanics", layout && mechanics, new JObject { ["layout"] = row["layout"].DeepClone(),
                        ["casts"] = new JArray(castRows.Select(cast => new JObject { ["desiredReflexD20"] = cast["desiredReflexD20"], ["selected"] = cast["selected"].DeepClone(),
                            ["damage"] = cast["damage"].DeepClone(), ["saves"] = cast["saves"].DeepClone(), ["sickened"] = cast["sickened"].DeepClone(), ["otherDamage"] = cast["otherDamage"] })) },
                    "Correlated to each breath's own execution context: the enemy inside the cone rolls one Reflex save; on a natural 1 it takes full 1d4 slashing and is Sickened (3 rounds), on a natural 20 half damage and no condition; the ally inside the cone takes no damage and gains no condition (its Reflex save, rolled by the released breath for every selected creature, is recorded); the enemy outside and the caster behind are never selected, roll nothing and take nothing.");
                dustCheck("s7-dust-action", castRows.All(cast => (string)cast["actionType"] == breath.ActionType.ToString() &&
                        (resource == null ? (bool)cast["availableAfter"] : (int?)cast["resourceAfter"] == (int?)cast["resourceBefore"] - resource.Amount)),
                    new JObject { ["blueprint"] = row["breathBlueprint"].DeepClone(), ["casts"] = new JArray(castRows.Select(cast => new JObject { ["actionType"] = cast["actionType"],
                        ["resourceBefore"] = cast["resourceBefore"], ["resourceAfter"] = cast["resourceAfter"], ["availableAfter"] = cast["availableAfter"] })) },
                    "Each breath uses the released blueprint's action type; a resource, if the breath has one, is spent once per breath, otherwise the breath stays available (the AI cooldown applies only to AI casting).");

                // 2. The same Dust Mephit executing a different ability that
                // launches the same cold cone keeps the native look.
                grantedIce = !dust.Descriptor.HasFact(iceBreath);
                if (grantedIce) dust.Descriptor.AddFact(iceBreath);
                int foreignFrom = trace.Creations.Count, foreignTints = DustMephitBreathTint.Applied;
                mark("dust-foreign-ability-start");
                var foreign = new JObject();
                foreach (int step in passive(dust, abilityData(dust, iceBreath), new TargetWrapper(hostile), foreign, null)) yield return step;
                foreach (int step in R6Seconds(1f)) yield return step;
                mark("dust-foreign-ability-end");
                var foreignCreations = trace.Creations.Skip(foreignFrom).OfType<JObject>().Where(value => (string)value["launcher"] == dust.UniqueId).ToList();
                foreign["creations"] = new JArray(foreignCreations.Select(value => value.DeepClone()));
                bool foreignNative = foreignCreations.Count == 1 && (string)foreignCreations[0]["executingAbility"] == iceBreath.AssetGuid &&
                    (string)foreignCreations[0]["projectile"] == MephitBreathPolicy.ColdConeProjectileGuid &&
                    ((JArray)foreignCreations[0]["effects"]).OfType<JObject>().Count() == 1 &&
                    ((JArray)foreignCreations[0]["effects"]).OfType<JObject>().All(effect => (bool?)effect["tinted"] == false && (string)effect["before"]["hash"] == (string)effect["after"]["hash"]) &&
                    DustMephitBreathTint.Applied == foreignTints;
                dustCheck("s7-dust-foreign-ability", foreignNative, foreign, "The Dust Mephit executing the Ice breath (same launcher, same cold cone, different ability) gets the native cone, untouched.");
                if (grantedIce) { dust.Descriptor.RemoveFact(iceBreath); grantedIce = false; }
                foreach (int step in S7WaitDustReleased()) yield return step;
                var foreignEffects = foreignCreations.SelectMany(value => ((JArray)value["effects"]).OfType<JObject>()).ToList();
                DateTime foreignRelease = DateTime.UtcNow.AddSeconds(20);
                while (DateTime.UtcNow < foreignRelease && foreignEffects.Any(trace.Live)) yield return 0;

                // 3. Reuse: a later native consumer claims the pooled instance the
                // Dust breaths used, and reads it as native before any product code.
                var dustEffects = castRows.SelectMany(effects).ToList();
                var dustInstances = new HashSet<int>(dustEffects.Select(effect => (int)effect["instance"]));
                var originals = new HashSet<string>(dustEffects.Select(effect => (string)effect["before"]["hash"]));
                int reuseFrom = trace.Creations.Count;
                var iceAgain = new JObject();
                mark("dust-reuse-start");
                foreach (int step in passive(ice, abilityData(ice, iceBreath), new TargetWrapper(hostile), iceAgain, null)) yield return step;
                foreach (int step in R6Seconds(1f)) yield return step;
                mark("dust-reuse-end");
                var iceEffects = trace.Creations.Skip(reuseFrom).OfType<JObject>().Where(value => (string)value["launcher"] == ice.UniqueId)
                    .SelectMany(value => ((JArray)value["effects"]).OfType<JObject>()).ToList();
                // Every later consumer that is not the Dust breath: the Dust
                // Mephit's own Ice breath and the Ice Mephit's breath.
                var laterNative = foreignEffects.Concat(iceEffects).ToList();
                var reusedRows = laterNative.Where(effect => dustInstances.Contains((int)effect["instance"])).ToList();
                bool allNative = laterNative.Count == 2 && laterNative.All(effect => originals.Contains((string)effect["before"]["hash"]) && (bool?)effect["tinted"] == false &&
                    (string)effect["before"]["hash"] == (string)effect["after"]["hash"] && (int)effect["before"]["dustClones"] == 0);
                dustCheck("s7-dust-pool-reuse", reusedRows.Count >= 1 && allNative, new JObject { ["dustInstances"] = new JArray(dustInstances), ["dustOriginalSignatures"] = new JArray(originals),
                        ["laterConsumers"] = new JArray(laterNative.Select(value => value.DeepClone())), ["reusedDustInstance"] = reusedRows.Count },
                    "A later consumer that is not the Dust breath claims the very pooled instance the Dust breaths used (same instance ID), and before any product code it reads exactly the native signature recorded before the first Dust tint; every later consumer stays untouched.");
                foreach (int step in S7WaitDustReleased()) yield return step;

                // 4. Cancelled before execution: no effect, no damage.
                int cancelApplied = DustMephitBreathTint.Applied, cancelFrom = trace.Creations.Count, cancelDamage = recorder.Damage.Count;
                mark("dust-cancel-start");
                var cancelled = new UnitUseAbility(abilityData(dust, breath), new TargetWrapper(hostile));
                issue(dust, cancelled);
                yield return 0;
                // R1-F4: the exact command through its owner (its process retained first, if it already started).
                row["cancelFailures"] = new JArray(conditions.EndUnfinished(new[] { cancelled }).ToArray());
                foreach (int step in R6Seconds(3f)) yield return step;
                mark("dust-cancel-end");
                dustCheck("s7-dust-cancel", DustMephitBreathTint.Applied == cancelApplied &&
                    trace.Creations.Skip(cancelFrom).All(value => (string)value["launcher"] != dust.UniqueId) &&
                    recorder.Damage.Skip(cancelDamage).All(value => !ReferenceEquals(value.Initiator, dust)),
                    new JObject { ["applied"] = DustMephitBreathTint.Applied - cancelApplied }, "A breath interrupted before execution creates nothing and deals nothing.");

                // 5. Concurrent donor: the Ice Mephit's cold cone live at the same time.
                PlaceExpandedSummoningUnit(ice, ground(hostile.Position + Vector3.right * 6f + Vector3.back * 4f));
                foreach (int step in R6Wait(4)) yield return step;
                int concurrentFrom = trace.Creations.Count;
                mark("dust-concurrent-start");
                var dustCast = new JObject(); var iceCast = new JObject();
                IEnumerator<int> dustRun = passive(dust, abilityData(dust, breath), new TargetWrapper(hostile), dustCast, null).GetEnumerator();
                IEnumerator<int> iceRun = null;
                int tintsAtStart = DustMephitBreathTint.Applied, iceDelay = -1;
                bool dustMore = true, iceMore = true;
                int overlapFrames = 0, restoredAt = DustMephitBreathTint.Restored, sampleFrame = 0;
                var donorTimeline = new JArray();
                DateTime concurrentDeadline = DateTime.UtcNow.AddSeconds(40);
                bool donorWasLive = false;
                // Runs until both commands end, no tint is live and the donor instance is gone,
                // so the donor is sampled at the moment the Dust instance is restored.
                while ((dustMore || iceRun == null || iceMore || DustMephitBreathTint.Live > 0 || donorWasLive) && DateTime.UtcNow < concurrentDeadline)
                {
                    if (dustMore) dustMore = dustRun.MoveNext();
                    // The Ice breath starts about two seconds after the Dust cone appears, so the Ice
                    // instance is live through the moment the Dust instance is released and restored.
                    if (iceRun == null && DustMephitBreathTint.Applied > tintsAtStart && ++iceDelay >= 60)
                        iceRun = passive(ice, abilityData(ice, iceBreath), new TargetWrapper(hostile), iceCast, null).GetEnumerator();
                    if (iceRun != null && iceMore) iceMore = iceRun.MoveNext();
                    var live = trace.Creations.Skip(concurrentFrom).OfType<JObject>().SelectMany(value => ((JArray)value["effects"]).OfType<JObject>()).ToList();
                    JObject donor = live.FirstOrDefault(effect => (string)effect["launcher"] == ice.UniqueId && trace.Live(effect));
                    bool dustLive = live.Any(effect => (string)effect["launcher"] == dust.UniqueId && trace.Live(effect));
                    if (dustLive && donor != null) overlapFrames++;
                    // The live donor's own signature over time, and at the moment
                    // the Dust instance is released and restored.
                    bool released = DustMephitBreathTint.Restored != restoredAt;
                    if (donor != null && (released || sampleFrame++ % 15 == 0))
                    {
                        JObject now = trace.Signature(donor);
                        donorTimeline.Add(new JObject { ["frame"] = Time.frameCount, ["event"] = released ? "dust-instance-restored" : "sample",
                            ["dustLive"] = dustLive, ["hash"] = now == null ? null : now["hash"], ["renderers"] = now == null ? null : now["renderers"],
                            ["particleSystems"] = now == null ? null : now["particleSystems"] });
                    }
                    restoredAt = DustMephitBreathTint.Restored;
                    donorWasLive = donor != null;
                    yield return 0;
                }
                foreach (int step in S7WaitDustReleased()) yield return step;
                mark("dust-concurrent-end");
                var concurrent = trace.Creations.Skip(concurrentFrom).OfType<JObject>().SelectMany(value => ((JArray)value["effects"]).OfType<JObject>()).ToList();
                var iceViews = concurrent.Where(value => (string)value["launcher"] == ice.UniqueId).ToArray();
                var dustViews = concurrent.Where(value => (string)value["launcher"] == dust.UniqueId).ToArray();
                bool iceNative = iceViews.Length == 1 && (bool?)iceViews[0]["tinted"] == false && (string)iceViews[0]["before"]["hash"] == (string)iceViews[0]["after"]["hash"];
                bool dustOwn = dustViews.Length == 1 && (bool?)dustViews[0]["tinted"] == true && (int)dustViews[0]["instance"] != (iceViews.Length == 1 ? (int)iceViews[0]["instance"] : 0);
                var dustRestored = dustViews.Select(effect => trace.Restoration(effect)).ToArray();
                string donorNative = iceViews.Length == 1 ? (string)iceViews[0]["before"]["hash"] : null;
                bool donorSteady = donorTimeline.Count >= 3 && donorTimeline.All(value => (string)value["hash"] == donorNative) &&
                    donorTimeline.Any(value => (string)value["event"] == "dust-instance-restored") && donorTimeline.Any(value => (bool)value["dustLive"]);
                dustCheck("s7-dust-donor-untouched", iceNative && dustOwn && overlapFrames > 0 && donorSteady && dustRestored.All(value => (bool)value["matchesBefore"]) && DustMephitBreathTint.Live == 0,
                    new JObject { ["ice"] = new JArray(iceViews.Select(value => value.DeepClone())), ["dust"] = new JArray(dustViews.Select(value => value.DeepClone())),
                        ["overlapFrames"] = overlapFrames, ["donorTimeline"] = donorTimeline, ["dustRestored"] = new JArray(dustRestored) },
                    "With both cones live at the same time, the Dust Mephit's own instance is recoloured and restored, while the Ice Mephit's separate live instance reads its native signature at every sample, including while the Dust instance is tinted and at the moment it is restored.");

                // 5b. Restoring one Dust instance leaves another active tint alone.
                PlaceExpandedSummoningUnit(spare, ground(hostile.Position + Vector3.left * 2.6f));
                foreach (int step in R6Wait(4)) yield return step;
                int pairFrom = trace.Creations.Count;
                mark("dust-own-restore-start");
                issue(dust, new UnitUseAbility(abilityData(dust, breath), new TargetWrapper(hostile)));
                issue(spare, new UnitUseAbility(abilityData(spare, breath), new TargetWrapper(hostile)));
                JObject chosen = null, other = null; var ownRestore = new JObject();
                DateTime pairDeadline = DateTime.UtcNow.AddSeconds(15);
                while (DateTime.UtcNow < pairDeadline && chosen == null)
                {
                    var pair = trace.Creations.Skip(pairFrom).OfType<JObject>().SelectMany(value => ((JArray)value["effects"]).OfType<JObject>()).Where(effect => (bool?)effect["tinted"] == true).ToList();
                    JObject a = pair.FirstOrDefault(effect => (string)effect["launcher"] == dust.UniqueId && trace.Live(effect));
                    JObject b = pair.FirstOrDefault(effect => (string)effect["launcher"] == spare.UniqueId && trace.Live(effect));
                    if (a != null && b != null)
                    {
                        chosen = a; other = b;
                        trace.Instance(a).GetComponent<DustMephitBreathTint>().Restore();
                        ownRestore["chosenNative"] = (string)trace.Signature(a)["hash"] == (string)a["before"]["hash"];
                        ownRestore["otherStillTinted"] = trace.Instance(b).GetComponent<DustMephitBreathTint>() != null && (string)trace.Signature(b)["hash"] != (string)b["before"]["hash"];
                    }
                    yield return 0;
                }
                foreach (int step in S7WaitDustReleased()) yield return step;
                mark("dust-own-restore-end");
                ownRestore["otherRestoredOnRelease"] = other != null && (bool)trace.Restoration(other)["matchesBefore"];
                dustCheck("s7-dust-own-instance-restore", chosen != null && (bool?)ownRestore["chosenNative"] == true && (bool?)ownRestore["otherStillTinted"] == true &&
                    (bool?)ownRestore["otherRestoredOnRelease"] == true, ownRestore,
                    "With two Dust cones live, restoring one exact instance returns it to its native signature and leaves the other instance's tint in place until its own release.");

                // 6. Source loss while the effect is active.
                int lossApplied = DustMephitBreathTint.Applied, lossFrom = trace.Creations.Count;
                mark("dust-loss-start");
                PlaceExpandedSummoningUnit(spare, ground(hostile.Position + Vector3.left * 2.6f));
                foreach (int step in R6Wait(4)) yield return step;
                issue(spare, new UnitUseAbility(abilityData(spare, breath), new TargetWrapper(hostile)));
                DateTime waitView = DateTime.UtcNow.AddSeconds(12);
                while (DateTime.UtcNow < waitView && DustMephitBreathTint.Applied == lossApplied) yield return 0;
                bool launched = DustMephitBreathTint.Applied > lossApplied;
                mark("dust-loss-despawn");
                S7Retire(spare, "dust source-loss unit", _s7Retire ?? (unit => DisposeExpandedSummoningUnits(fixture.Created, new[] { unit })));
                foreach (int step in S7WaitDustReleased()) yield return step;
                mark("dust-loss-end");
                var lossEffects = trace.Creations.Skip(lossFrom).OfType<JObject>().SelectMany(value => ((JArray)value["effects"]).OfType<JObject>()).ToArray();
                var lossRestored = lossEffects.Select(effect => trace.Restoration(effect)).ToArray();
                dustCheck("s7-dust-source-loss", launched && DustMephitBreathTint.Live == 0 && lossEffects.Length == 1 &&
                    lossRestored.All(value => (bool)value["exists"] && (bool)value["matchesBefore"] && !(bool)value["tintPresent"]) &&
                    UnityEngine.Object.FindObjectsOfType<DustMephitBreathTint>().Length == 0,
                    new JObject { ["launched"] = launched, ["live"] = DustMephitBreathTint.Live, ["effects"] = new JArray(lossEffects.Select(value => value.DeepClone())), ["restored"] = new JArray(lossRestored) },
                    "A Dust Mephit removed while its breath effect is active leaves the instance reading its native signature once the effect ends.");

                // 8. R1-F9: inside an enclosing owned Dust creation, an inner
                // owned creation is interrupted and, before the enclosing call
                // returns, the same cast effect is spawned at the same point by
                // another caller; the enclosing creation's own spawn follows.
                // The first Dust Mephit nears its native summon expiry (caster
                // level 20: 120 s); the later casts use a fresh one, owned by the
                // same cell ledger.
                UnitEntityData late = summon("dust-mephit");
                PlaceExpandedSummoningUnit(late, spot);
                foreach (int step in R6Wait(30)) yield return step;
                row["laterCastsUnit"] = late.UniqueId;
                var nested = new JObject(); row["nestedInterruption"] = nested;
                var nestedCast = new JObject(); nested["cast"] = nestedCast;
                int nestedFrom = trace.Creations.Count, nestedApplied = DustMephitBreathTint.Applied, nestedSaves = saveLog.Rows.Count, nestedDamage = recorder.Damage.Count;
                mark("dust-nested-start");
                S7ProjectileTrace.NestedProbeLauncher = late.UniqueId; S7ProjectileTrace.NestedProbe = nested;
                try { foreach (int step in passive(late, abilityData(late, breath), new TargetWrapper(hostile), nestedCast, null)) yield return step; }
                finally { S7ProjectileTrace.NestedProbe = null; S7ProjectileTrace.NestedProbeLauncher = null; }
                conditions.Observe();
                nested["diagnostics"] = laterCast(late, nestedSaves, nestedDamage);
                var nestedCreations = trace.Creations.Skip(nestedFrom).OfType<JObject>().Where(value => (string)value["launcher"] == late.UniqueId).ToList();
                nested["creations"] = new JArray(nestedCreations.Select(value => value.DeepClone()));
                nested["tintsApplied"] = DustMephitBreathTint.Applied - nestedApplied;
                foreach (int step in S7WaitDustReleased()) yield return step;
                mark("dust-nested-end");
                var nestedEffects = nestedCreations.SelectMany(value => ((JArray)value["effects"]).OfType<JObject>()).ToList();
                nested["restored"] = new JArray(nestedEffects.Select(effect => trace.Restoration(effect)));
                Func<JToken, Vector3> vector = value => value == null || value.Type != JTokenType.Array ? new Vector3(float.NaN, 0f, 0f) : new Vector3((float)value[0], (float)value[1], (float)value[2]);
                bool nestedOk = (string)nested["innerInterrupted"] == "NullReferenceException" && (string)nested["enclosingExecutingAbility"] == breath.AssetGuid &&
                    (int?)nested["innerTintsApplied"] == 0 && (bool?)nested["foreignSpawned"] == true && (bool?)nested["foreignTinted"] == false &&
                    (int?)nested["foreignTintsApplied"] == 0 && Vector3.Distance(vector(nested["foreignPoint"]), vector(nested["innerLaunchPoint"])) < .001f &&
                    nested["foreignSignature"] != null && originals.Contains((string)nested["foreignSignature"]["hash"]) &&
                    nestedCreations.Count == 1 && (string)nestedCreations[0]["executingAbility"] == breath.AssetGuid &&
                    nestedEffects.Count(effect => (bool)effect["isCastFx"]) == 1 && nestedEffects.All(effect => (bool?)effect["tinted"] == true) &&
                    (int)nested["tintsApplied"] == 1 &&
                    ((JArray)nested["restored"]).All(value => (bool)value["exists"] && !(bool)value["tintPresent"] && (bool)value["matchesBefore"]);
                dustCheck("s7-dust-nested-interruption", nestedOk, (JObject)nested.DeepClone(),
                    "While an owned Dust creation is in progress, an inner owned creation of the same breath is interrupted (native CreateView throws after setting its launch point, before its spawn) and the same cast effect is then spawned at that very point by another caller before the enclosing call returns: that foreign instance stays native (no tint, the native signature) and the interrupted creation tints nothing; the enclosing creation's own cast effect is then tinted exactly once and restored on release.");

                // 9. Early exit after application (R4-F1): owned by the cell's
                // own ledger; see S7DustEarlyExit.
                var early = new JObject(); row["earlyExit"] = early;
                hostileSaves.DesiredRoll = 1;
                clearSickened();
                int earlySaves = saveLog.Rows.Count, earlyDamage = recorder.Damage.Count;
                mark("dust-early-exit-start");
                foreach (int step in S7DustEarlyExit(late, abilityData(late, breath), hostile, conditions, releaseFailures, early,
                    () => laterCast(late, earlySaves, earlyDamage), null)) yield return step;
                early["remainingAfterExit"] = conditions.Remaining();
                early["foreignKept"] = foreignSickened != null && fixture.Caster.Descriptor.Buffs.Enumerable.Contains(foreignSickened);
                early["commandsLeft"] = (late.Commands.HasUnfinished() ? 1 : 0);
                mark("dust-early-exit-end");
                foreach (int step in S7WaitDustReleased()) yield return step;
                early["ownedExecutionsLiveAfterWait"] = conditions.LiveExecutions();
                dustCheck("s7-dust-early-exit-released", (bool)early["appliedBeforeExit"] && (int)early["preexistingCellOwned"] == 0 && (int)early["acquiredBeforeExit"] >= 1 &&
                        (int)early["remainingAfterExit"] == 0 && (bool)early["foreignKept"] && ((JArray)early["releaseFailures"]).Count == 0 && (int)early["commandsLeft"] == 0 &&
                        (int)early["ownedExecutionsLiveAfterWait"] == 0, (JObject)early.DeepClone(),
                    "A Dust cast of the cell, owned by the cell's own ledger, whose owner exits as soon as its Sickened is applied (enumerator disposed mid-cast) interrupts only its own command and removes exactly the Sickened instance its execution introduced (none was owned before it): none remains, its release reports no failure into the cell's failure list, its execution has ended after the settle wait, and the foreign control survives.");

                // 9a. Recovery C3 (R7-F2): an ordinary owned Dust cast released after its execution starts and before its first
                // effect delivery, through the cell's own command release. The exact process is retained before the
                // interruption can clear the command's field, stays under observation and is observed ended.
                var preDelivery = new JObject(); row["preDeliveryRelease"] = preDelivery;
                UnitEntityData early2 = summon("dust-mephit");
                PlaceExpandedSummoningUnit(early2, ground(spot + Vector3.left * 1.5f));
                foreach (int step in R6Wait(20)) yield return step;
                mark("dust-pre-delivery-start");
                var preCommand = issue(early2, new UnitUseAbility(abilityData(early2, breath), new TargetWrapper(hostile)));
                Kingmaker.Controllers.AbilityExecutionProcess preProcess = null;
                DateTime preDeadline = DateTime.UtcNow.AddSeconds(20);
                while (DateTime.UtcNow < preDeadline && !preCommand.IsFinished && preCommand.ExecutionProcess == null) yield return 0;
                preProcess = preCommand.ExecutionProcess;
                preDelivery["processStarted"] = preProcess != null;
                preDelivery["deliveredBeforeRelease"] = conditions.Delivered(preCommand);
                List<string> preFailures = conditions.EndUnfinished(new[] { preCommand });
                preDelivery["releaseFailures"] = new JArray(preFailures.ToArray());
                preDelivery["commandFieldAfterRelease"] = preCommand.ExecutionProcess == null ? "cleared" : ReferenceEquals(preCommand.ExecutionProcess, preProcess) ? "same process" : "other process";
                preDelivery["processRetained"] = conditions.Retains(preProcess);
                preDelivery["endedRightAfterRelease"] = S7DustConditions.Settled(preProcess);
                DateTime preSettle = DateTime.UtcNow.AddSeconds(10);
                while (DateTime.UtcNow < preSettle && S7DustConditions.Settled(preProcess) != true) yield return 0;
                preDelivery["endedAfterWait"] = S7DustConditions.Settled(preProcess);
                preDelivery["liveExecutionsAfterWait"] = conditions.LiveExecutions();
                foreach (int step in S7WaitDustReleased()) yield return step;
                clearSickened();
                mark("dust-pre-delivery-end");
                dustCheck("s7-dust-pre-delivery-release-retained", preProcess != null && !(bool)preDelivery["deliveredBeforeRelease"] && preFailures.Count == 0 &&
                        (bool)preDelivery["processRetained"] && (bool?)preDelivery["endedAfterWait"] == true && (int)preDelivery["liveExecutionsAfterWait"] == 0,
                    (JObject)preDelivery.DeepClone(),
                    "An ordinary owned Dust cast released after its execution process started and before its first effect delivery: the cell's command release retains the exact process before interrupting the command (whatever the command's field reads afterwards), the process stays under observation and is observed ended, and no retained execution is left live.");

                // 9b. R4-F4: a separate execution by the same Dust Mephit that the
                // cell does not issue (standing for another owner's command), on the
                // enemy outside the cone: its Sickened is not the cell's.
                var unowned = new JObject(); row["unownedExecution"] = unowned;
                outsideSaves.DesiredRoll = 1;
                // Beside that enemy, facing away from the hostile and the caster behind.
                PlaceExpandedSummoningUnit(late, ground(outside.Position + Vector3.right * 2f));
                foreach (int step in R6Wait(20)) yield return step;
                unowned["canTarget"] = abilityData(late, breath).CanTarget(new TargetWrapper(outside));
                var separate = new UnitUseAbility(abilityData(late, breath), new TargetWrapper(outside));
                // R3-F3: from here the ordinary path requires the same-caster control at cleanup. R2-F7: this separate owner's
                // command is deliberately not recorded as the runner's.
                unownedControlDue = true;
                late.Commands.Run(separate);
                DateTime separateDeadline = DateTime.UtcNow.AddSeconds(15);
                while (DateTime.UtcNow < separateDeadline && !(separate.IsFinished && (separate.ExecutionProcess == null || separate.ExecutionProcess.IsEnded))) yield return 0;
                AbilityExecutionContext separateContext = separate.ExecutionProcess == null ? null : separate.ExecutionProcess.Context;
                unownedSickened = outside.Descriptor.Buffs.Enumerable.FirstOrDefault(buff => ReferenceEquals(buff.Blueprint, sickened) && buff.Context != null &&
                    (ReferenceEquals(buff.Context, separateContext) || ReferenceEquals(buff.Context.SourceAbilityContext, separateContext)));
                unowned["sickenedApplied"] = unownedSickened != null;
                unowned["ownedByLedger"] = unownedSickened != null && conditions.Owns(unownedSickened);
                foreach (int step in S7WaitDustReleased()) yield return step;
                PlaceExpandedSummoningUnit(late, spot);
                foreach (int step in R6Wait(20)) yield return step;

                if (_s7Mechanics)
                {
                    // Recovery C3 (R1-F12): the mechanics-only selection makes the cell's last cast a plain owned cast (no frames,
                    // no native-look restoration); its own recolour is released natively and its Sickened is live at cleanup.
                    var finalCast = new JObject(); row["finalCast"] = finalCast;
                    mark("dust-final-cast-start");
                    var command = issue(late, new UnitUseAbility(abilityData(late, breath), new TargetWrapper(hostile)));
                    DateTime deadline = DateTime.UtcNow.AddSeconds(20);
                    while (DateTime.UtcNow < deadline && !(command.IsFinished && (command.ExecutionProcess == null || command.ExecutionProcess.IsEnded))) yield return 0;
                    finalCast["finished"] = command.IsFinished;
                    finalCast["ownedLive"] = conditions.Observe() >= 0 ? conditions.Remaining() : -1;
                    mark("dust-final-cast-end");
                }
                else
                {
                // 7. A native-look comparison of the same Dust cast: the
                // observer restores the recolour as soon as it is applied.
                var nativeLook = new JObject(); row["nativeLook"] = nativeLook;
                int lookApplied = DustMephitBreathTint.Applied, lookSaves = saveLog.Rows.Count, lookDamage = recorder.Damage.Count;
                mark("dust-native-look-start");
                using (var clip = new R6Clip(_request.EvidenceDirectory, "s7-dust-native-look", () => R6RenderBounds(late, hostile), null))
                {
                    var command = issue(late, new UnitUseAbility(abilityData(late, breath), new TargetWrapper(hostile)));
                    DateTime deadline = DateTime.UtcNow.AddSeconds(20);
                    bool restored = false;
                    int lookFrom = trace.Creations.Count;
                    while (DateTime.UtcNow < deadline && !(command.IsFinished && (command.ExecutionProcess == null || command.ExecutionProcess.IsEnded)))
                    {
                        // Only this comparison cast's own instance is restored.
                        JObject own = trace.Creations.Skip(lookFrom).OfType<JObject>().Where(value => (string)value["launcher"] == late.UniqueId)
                            .SelectMany(value => ((JArray)value["effects"]).OfType<JObject>()).FirstOrDefault(effect => (bool?)effect["tinted"] == true);
                        if (!restored && own != null && trace.Instance(own) != null)
                        {
                            DustMephitBreathTint tint = trace.Instance(own).GetComponent<DustMephitBreathTint>();
                            if (tint != null) tint.Restore();
                            restored = true;
                            nativeLook["instance"] = own["instance"];
                        }
                        clip.Shot(R6AnimationState(late, command));
                        yield return 0;
                    }
                    nativeLook["restoredByObserver"] = restored; nativeLook["sceneryHitsOnBearing"] = clip.Occluded;
                    nativeLook["diagnostics"] = laterCast(late, lookSaves, lookDamage);
                }
                mark("dust-native-look-end");
                }
            }
            finally
            {
                // Owner amendment 3 (R4-F1): every release is attempted on its own; the cell's cleanup outcome (named
                // failures, and each retained execution not yet observed ended as a residual re-read by the phase) is
                // published to the enclosing isolation, whichever way the cell unwinds.
                var released = new List<KeyValuePair<string, Action>>();
                var traceFailures = new List<string>();
                if (trace != null) released.Add(new KeyValuePair<string, Action>("projectile trace", () =>
                {
                    bool traceReleased = S7DisposeTrace(trace, row, traceFailures);
                    dustCheck("s7-dust-trace-released", traceReleased, new JObject { ["before"] = row["ownedHooksBeforeDispose"], ["after"] = row["ownedHooksAfterDispose"],
                        ["failures"] = row["disposeFailures"].DeepClone(), ["countFailures"] = row["countFailures"].DeepClone() },
                        "The projectile trace's normal disposal removes every owned hook without a failed release or an unreadable count.");
                }));
                released.Add(new KeyValuePair<string, Action>("hostile saves", () => hostileSaves.DesiredRoll = 1));
                released.Add(new KeyValuePair<string, Action>("granted ice breath", () => { if (grantedIce && !dust.Destroyed && dust.Descriptor.HasFact(iceBreath)) dust.Descriptor.RemoveFact(iceBreath); }));
                List<string> cleanupFailures = Sprint7ObserverPolicy.RunReleases(released);
                // R1-F9: the save-log and outside-saves subscriptions, each released on its own.
                cleanupFailures.AddRange(setup.ReleaseAll());
                // R1-F5: each of the trace's release and count failures is its own failure.
                foreach (string failure in traceFailures) isolation.Fail("s7-dust", "projectile trace: " + failure);
                // R7-F3: the trace attempt's explicit readback by identity, also when its set-up returned no trace.
                if (trace == null) row["traceAttempt"] = traceAttempt.Publish(isolation, "s7-dust");
                isolation.Fail("s7-dust", cleanupFailures);
                row["cleanupFailures"] = new JArray(cleanupFailures.ToArray());
                // R2-F4: both foreign controls (the caster-behind control and the same-caster unowned execution's condition) and
                // the outside target stay in place through the owned release and any deferred settlement; they are read by
                // identity afterwards (missing or unreadable is a failure) and only then released, each on its own.
                Func<Buff, UnitEntityData, Func<bool?>> presentOn = (buff, unit) => () => unit == null || unit.Destroyed ? (bool?)false : unit.Descriptor.Buffs.Enumerable.Contains(buff);
                bool foreignPresentAtCleanup = false;
                try { foreignPresentAtCleanup = foreignSickened != null && presentOn(foreignSickened, fixture.Caster)() == true; } catch (Exception) { }
                row["foreignPresentAtCleanup"] = foreignPresentAtCleanup;
                S7DustConditions owner = conditions;
                Func<List<string>> afterSettlement = () =>
                {
                    // R3-F3 (audit A1-F4): the production Dust control settlement declares this path's controls (the caster-behind
                    // control once placed; the same-caster control once the separate execution was due); each is read once, and the
                    // diagnostics, the verdict and the releases all use that one result; a failed read never prevents an independent
                    // release; the outside target's one retirement request and its publication to this cell's isolation follow.
                    var presence = new Dictionary<string, bool?>();
                    List<string> failures = Sprint7ObserverPolicy.DustControlSettlement(isolation, "s7-dust",
                        foreignSickened, row["layout"] != null, buff => presentOn(buff, fixture.Caster)(),
                        unownedSickened, unownedControlDue, buff => presentOn(buff, outside)(), buff => S7DustConditions.Identity(buff), buff => buff.Remove(),
                        outside, unit => !unit.Destroyed, _s7Retirements, _s7Retire ?? (unit => DisposeExpandedSummoningUnits(fixture.Created, new[] { unit })),
                        unit => unit.UniqueId, S7UnitSettled, row, presence);
                    row["foreignKeptAtSettlement"] = presence["caster-behind control"] == true;
                    row["unownedKeptAtSettlement"] = presence["same-caster unowned control"] == true;
                    row["conditionsSubscribedAfterSettlement"] = owner == null ? (bool?)null : owner.Subscribed;
                    row["lateRemovedAtSettlement"] = owner == null ? -1 : owner.LastRemoved;
                    return failures;
                };
                int liveBeforeCleanup = -1;
                JObject effects = null;
                if (owner != null)
                {
                    try { liveBeforeCleanup = owner.Remaining(); } catch (Exception) { liveBeforeCleanup = -1; }
                    // R1-F2, R1-F3, R1-F4: commands released one by one (each process retained first, failures published at once),
                    // owned conditions released, live or unreadable executions kept as residuals; the ledger stays subscribed and
                    // reconciles late deliveries until they settle; then the controls are checked and released.
                    effects = Sprint7ObserverPolicy.ReleaseOwnedEffects(isolation, "s7-dust", owner.Commands, S7DustConditions.Unfinished, owner.Interrupt,
                        S7DustConditions.Label, S7DustConditions.Identity, () => owner.Release(), owner.RemainingIdentities, owner.Dispose, afterSettlement, () => owner.LastRemoved);
                    row["ownedEffects"] = effects;
                    row["ownedExecutions"] = effects["ownedExecutions"];
                    row["ownedExecutionsLive"] = effects["ownedExecutionsLive"];
                    row["ownedCommandsEnded"] = owner.Ended;
                    try { row["ownedCommandsLeft"] = owner.UnfinishedLeft(); } catch (Exception error) { row["ownedCommandsLeft"] = null; isolation.Fail("s7-dust", "owned commands unread: " + error.GetType().Name + ": " + error.Message); }
                    row["ownedCommandFailures"] = new JArray(owner.Commands.Failures.ToArray());
                    row["conditionsSubscribedAfterCleanup"] = owner.Subscribed;
                    if (row["ownedCommandsLeft"].Type == JTokenType.Integer && (int)row["ownedCommandsLeft"] != 0) isolation.Fail("s7-dust", "owned commands left running: " + row["ownedCommandsLeft"]);
                    if ((bool?)effects["ledgerHeldUntilSettlement"] == false && owner.Subscribed) isolation.Fail("s7-dust", "condition ledger still subscribed");
                }
                else
                {
                    isolation.Fail("s7-dust", "condition ledger not acquired");
                    foreach (string failure in afterSettlement()) isolation.Fail("s7-dust", failure);
                }
                if (trace != null && (bool?)row["traceReleased"] != true) isolation.Fail("s7-dust", "projectile trace not released");
                bool unownedKept = (bool?)row["unownedKeptAtSettlement"] == true;
                row["unownedExecutionKept"] = unownedKept;
                JArray remainingAtCleanup = effects == null ? null : effects["remainingAtCleanup"] as JArray;
                int live = effects == null ? -1 : (int)effects["ownedExecutionsLive"];
                row["conditions"] = new JObject { ["acquired"] = owner == null ? -1 : owner.Acquired.Count, ["ownedExecutions"] = owner == null ? -1 : owner.OwnedExecutions,
                    ["liveBeforeCleanup"] = liveBeforeCleanup, ["removedAtCleanup"] = effects == null ? (JToken)(-1) : effects["removedAtCleanup"],
                    ["remainingAfterCleanup"] = remainingAtCleanup == null ? -1 : remainingAtCleanup.Count, ["releaseFailures"] = releaseFailures,
                    ["foreignKeptAtCleanup"] = foreignPresentAtCleanup, ["unownedExecutionKept"] = unownedKept, ["ownedCommandsLeft"] = row["ownedCommandsLeft"],
                    ["ownedCommandFailures"] = row["ownedCommandFailures"], ["ownedExecutionsLive"] = live, ["ledgerHeldUntilSettlement"] = effects == null ? null : effects["ledgerHeldUntilSettlement"],
                    ["note"] = "Sickened lasts 3 rounds; an instance that expired before cleanup is gone natively and is not counted as live." };
                isolation.Reported("s7-dust");
                dustCheck("s7-dust-cell-conditions-released", owner != null && remainingAtCleanup != null && remainingAtCleanup.Count == 0 && releaseFailures.Count == 0 &&
                        liveBeforeCleanup >= 1 && (int?)effects["removedAtCleanup"] >= 1 && owner.Acquired.Count >= 2 && foreignPresentAtCleanup && unownedKept &&
                        (bool?)row["foreignKeptAtSettlement"] == true && row["ownedCommandsLeft"].Type == JTokenType.Integer && (int)row["ownedCommandsLeft"] == 0 && live == 0 &&
                        owner.Commands.Failures.Count == 0 && !owner.Subscribed && cleanupFailures.Count == 0,
                    (JObject)row["conditions"].DeepClone(),
                    "Cleanup after the cell's last cast removes the Sickened instances introduced by the executions of the commands this cell issued (its Dust breaths and the Ice breath controls; native Sickened refreshes an existing instance, keeping its first context) and still live (at least the last cast's): none remains; the foreign control and the Sickened from a separate same-caster execution the cell did not issue are both still present after that owned release (read by identity before the fixture removes them); no owned command is left running and no command release ever failed, every retained execution has ended, every release ran without failure and the ledger's subscription has ended.");
                dustCheck("s7-dust-foreign-sickened-kept", foreignSickened != null && (bool?)row["foreignSickenedAfterResets"] == true && foreignPresentAtCleanup && (bool?)row["foreignKeptAtSettlement"] == true,
                    new JObject { ["afterResets"] = row["foreignSickenedAfterResets"], ["atCleanup"] = foreignPresentAtCleanup, ["atSettlement"] = row["foreignKeptAtSettlement"] },
                    "A Sickened condition from another context on the caster behind the cone survives every between-cast reset, the cell's cleanup and the settlement of its effects; only breath-owned instances are removed (the observer then removes its own control).");
            }
        }

        // R4-F1 (owner amendment 3), recovery C3 (R4-F1, R7-F5): the enclosing runner of a Dust cell is disposed while the
        // cell is yielded inside its own early-exit cast, right after that cast applied Sickened and before the cast's cleanup
        // ran, with every removal of one owned condition made to fail. The runner is the phase's own cell sequence over a child
        // isolation, with a next cell after the Dust cell (Sprint7Abandonment). Disposal unwinds the cast's finally and the
        // cell's cleanup, which publishes to the child isolation the named failure and each exact retained execution not yet
        // ended as a residual. The same admission guard re-reads those executions and refuses the next cell. The driver repairs
        // its own injected leftover on every exit, including its own disposal during that recovery wait, and only the exact
        // leftover (the foreign control is kept). The complete child outcome decides: any child failure other than the injected
        // removal failure and its known remaining condition reaches the phase by name and is never exempted by the repair.
        private IEnumerable<int> S7DustAbandoned(ExpandedSummoningCorrectionFixture fixture, Func<string, UnitEntityData> summon, UnitEntityData hostile,
            JObject record, Action<string> mark, Func<Vector3, Vector3> ground, R4MephitRecorder recorder,
            Func<UnitEntityData, BlueprintAbility, AbilityData> abilityData, IDictionary<string, BlueprintScriptableObject> symbols, Sprint26Saves hostileSaves,
            Sprint7CellIsolation phase)
        {
            var inner = new JObject(); var probe = new JObject(); var cells = new JObject();
            bool nextStarted = false;
            var leftovers = new List<Buff>();
            var leftoverIdentities = new List<string>();
            var driver = new Sprint7Abandonment
            {
                Owner = "s7-dust-abandoned driver", Phase = phase,
                Ready = () => (bool?)probe["applied"] == true,
                Inject = () => S7DustConditions.Inject = "fail-owned-removal",
                Clear = () => S7DustConditions.Inject = null,
                // The recolour of the cell's last cast is released natively; the safety readback follows that wait.
                AfterRepair = S7WaitDustReleased,
            };
            // R1-F6: the injected fault and its known residual, bound to the exact instance (identity captured at injection).
            driver.Expected = () => leftoverIdentities.Count != 1 ? new[] { "exactly one injected leftover expected, saw " + leftoverIdentities.Count } : new[] {
                "s7-dust: release failure: early-exit cast: removal of " + leftoverIdentities[0] + ": InvalidOperationException: " + S7DustConditions.RemovalFault,
                "s7-dust: owned conditions remaining: 1 (" + leftoverIdentities[0] + ")" };
            driver.Repair = () =>
            {
                // Only the exact injected leftover. R1-F7: a removal that returns with the instance still present, or whose result
                // cannot be read, is a failed repair; its identity is kept for settlement after the injection bookkeeping is cleared.
                leftovers.AddRange(S7DustConditions.InjectedLeftovers.Where(value => !leftovers.Contains(value)));
                leftoverIdentities.AddRange(S7DustConditions.InjectedIdentities.Where(value => !leftoverIdentities.Contains(value)));
                List<string> failures = Sprint7ObserverPolicy.RepairExact(leftovers, leftoverIdentities,
                    leftover => leftover.Owner != null && leftover.Owner.Buffs.Enumerable.Contains(leftover), leftover => leftover.Remove());
                driver.Row["injectedLeftoverIdentities"] = new JArray(leftoverIdentities.ToArray());
                S7DustConditions.InjectedLeftovers.Clear(); S7DustConditions.InjectedIdentities.Clear();
                return failures;
            };
            driver.Unsafe = () =>
            {
                var reasons = new List<string>();
                bool gone;
                try { gone = leftovers.All(value => value.Owner == null || !value.Owner.Buffs.Enumerable.Contains(value)); } catch (Exception) { gone = false; }
                JToken dust = inner["dust"];
                JToken conditions = dust == null ? null : dust["conditions"];
                JArray executions = dust == null ? null : dust["ownedExecutions"] as JArray;
                driver.Row["injectedLeftovers"] = leftovers.Count; driver.Row["leftoverGone"] = gone;
                if (leftovers.Count != 1 || !gone) reasons.Add("injected leftover not observed removed (" + leftovers.Count + ")");
                if (executions == null || executions.Count < 1) reasons.Add("no retained execution recorded");
                if (dust == null || (bool?)dust["traceReleased"] != true || (int?)dust["ownedHooksAfterDispose"] != 0) reasons.Add("trace not released");
                if (dust == null || (int?)dust["ownedCommandsLeft"] != 0) reasons.Add("owned commands left");
                if (dust == null || (bool?)dust["conditionsSubscribedAfterSettlement"] != false) reasons.Add("condition ledger subscribed after settlement");
                if (conditions == null || (bool?)conditions["foreignKeptAtCleanup"] != true || (bool?)dust["foreignKeptAtSettlement"] != true) reasons.Add("foreign control not kept through settlement");
                if (DustMephitBreathTint.Live != 0) reasons.Add("recolours live: " + DustMephitBreathTint.Live);
                return reasons;
            };
            JObject row = driver.Row; record["dustAbandoned"] = row; row["probe"] = probe; row["cells"] = cells;
            Func<IEnumerable<int>> next = () => { nextStarted = true; return R6Wait(1); };
            S7DustConditions.InjectedLeftovers.Clear(); S7DustConditions.InjectedIdentities.Clear();
            mark("dust-abandon-start");
            try
            {
                foreach (int step in driver.Run(
                    Sprint7ObserverPolicy.Cell("s7-dust-abandoned-inner", () => S7Dust(fixture, summon, hostile, inner, mark, ground, recorder, abilityData, symbols, hostileSaves, driver.Child, probe)),
                    Sprint7ObserverPolicy.Cell("s7-dust-abandoned-next", next), S7Cells(driver.Child, cells), TimeSpan.FromSeconds(20), TimeSpan.FromSeconds(60)))
                    yield return step;
            }
            finally
            {
                JToken dust = inner["dust"];
                row["dust"] = dust == null ? null : new JObject { ["earlyExit"] = dust["earlyExit"], ["ownedCommandsEnded"] = dust["ownedCommandsEnded"], ["ownedCommandsLeft"] = dust["ownedCommandsLeft"],
                    ["ownedCommandFailures"] = dust["ownedCommandFailures"], ["ownedExecutions"] = dust["ownedExecutions"], ["ownedExecutionsLive"] = dust["ownedExecutionsLive"],
                    ["traceReleased"] = dust["traceReleased"], ["disposeFailures"] = dust["disposeFailures"], ["ownedHooksAfterDispose"] = dust["ownedHooksAfterDispose"],
                    ["cleanupFailures"] = dust["cleanupFailures"], ["conditions"] = dust["conditions"], ["conditionsSubscribedAfterCleanup"] = dust["conditionsSubscribedAfterCleanup"],
                    ["deferredChecks"] = dust["deferredChecks"] };
                row["nextCellStarted"] = nextStarted;
                mark("dust-abandon-end");
            }
            JToken innerDust = inner["dust"];
            JArray retained = innerDust == null ? null : innerDust["ownedExecutions"] as JArray;
            int liveAtCleanup = retained == null ? -1 : retained.Count(value => (bool?)value["endedAtCleanup"] != true);
            bool executionsSettled = retained != null && retained.Count >= 1 && driver.Child.Pending.Count == 0 &&
                driver.Child.Recoveries.Count(value => value.StartsWith("s7-dust: owned execution ", StringComparison.Ordinal)) == liveAtCleanup;
            row["executionsSettledBeforeReuse"] = executionsSettled; row["liveExecutionsAtCleanup"] = liveAtCleanup;
            Sprint25Check("s7-dust-abandoned-unwound", (bool?)row["readyBeforeAbandon"] == true && executionsSettled && !nextStarted && driver.Proven, (JObject)row.DeepClone(),
                "With the enclosing cell sequence disposed while the Dust cell is yielded inside its early-exit cast right after that cast applied Sickened (every removal of that one owned instance made to fail): the cast's and the cell's cleanup publish the named failure into the child isolation with each retained execution not yet ended as a residual; the other releases still run (no owned command left, the trace's hooks removed, the ledger unsubscribed, the foreign control kept); the same admission guard re-reads the exact executions until each is observed ended and then refuses the next cell, which never starts; the child failures are exactly the injected ones (anything else would reach the phase by name); the driver removes only its own leftover and reports the fixture safe only with every retained execution observed ended and no recolour live.");
        }

        // Recovery C3 (C3 review R1-F2): an ordinary Dust cell whose enclosing runner is disposed after one of its own casts
        // started executing and before that execution's first delivery (no injected fault). The cell's release retains the
        // execution, keeps its condition ledger subscribed and reconciles anything it delivers late once it settles; the
        // foreign control survives through that settlement; then the same admission guard admits the next cell.
        private IEnumerable<int> S7DustPreDeliveryDisposed(ExpandedSummoningCorrectionFixture fixture, Func<string, UnitEntityData> summon, UnitEntityData hostile,
            JObject record, Action<string> mark, Func<Vector3, Vector3> ground, R4MephitRecorder recorder,
            Func<UnitEntityData, BlueprintAbility, AbilityData> abilityData, IDictionary<string, BlueprintScriptableObject> symbols, Sprint26Saves hostileSaves,
            Sprint7CellIsolation phase)
        {
            var inner = new JObject(); var probe = new JObject { ["mode"] = "pre-delivery" }; var cells = new JObject();
            bool nextStarted = false;
            var driver = new Sprint7Abandonment
            {
                Owner = "s7-dust-pre-delivery driver", Phase = phase, ExpectAdmitted = true,
                Ready = () => (bool?)probe["ready"] == true,
                AtDisposal = () => probe["atDisposal"] = _s7PreDeliveryState == null ? null : _s7PreDeliveryState(),
                AfterRepair = S7WaitDustReleased,
            };
            driver.Unsafe = () =>
            {
                var reasons = new List<string>();
                JToken dust = inner["dust"];
                JToken effects = dust == null ? null : dust["ownedEffects"];
                if ((bool?)probe["processStarted"] != true) reasons.Add("no owned execution had started at disposal");
                // R2-F3: the state at the disposal itself, in the frame readiness was established, for the exact execution.
                JToken atDisposal = probe["atDisposal"];
                if (atDisposal == null || atDisposal.Type != JTokenType.Object) reasons.Add("no disposal-time readback");
                else
                {
                    if ((int?)atDisposal["frame"] != (int?)probe["readyFrame"]) reasons.Add("disposed in a later frame than readiness");
                    if ((bool?)atDisposal["delivered"] != false || (int?)atDisposal["acquiredFromExecution"] != 0) reasons.Add("the execution had delivered at disposal");
                    if ((string)atDisposal["execution"] != (string)probe["execution"]) reasons.Add("a different execution at disposal");
                }
                JObject afterSettled = _s7PreDeliveryState == null ? null : _s7PreDeliveryState();
                driver.Row["afterSettlement"] = afterSettled;
                // Audit A2-F2: an unknown count (unreadable attribution) is never accepted as zero.
                if (afterSettled == null || !Sprint7ObserverPolicy.ExecutionConditionsReleased((bool?)afterSettled["ended"], (int?)afterSettled["presentFromExecution"]))
                    reasons.Add("the execution's own conditions not settled and released");
                if (effects == null || effects.Type != JTokenType.Object) reasons.Add("no effects release recorded");
                else
                {
                    if ((bool?)effects["ledgerHeldUntilSettlement"] != true) reasons.Add("the condition ledger was not held until settlement");
                    if (!(effects["remainingAfterSettlement"] is JArray) || ((JArray)effects["remainingAfterSettlement"]).Count != 0) reasons.Add("owned conditions after settlement");
                }
                if (dust == null || (bool?)dust["foreignKeptAtSettlement"] != true) reasons.Add("foreign control not kept through settlement");
                if (dust == null || (bool?)dust["conditionsSubscribedAfterSettlement"] != false) reasons.Add("condition ledger subscribed after settlement");
                if (DustMephitBreathTint.Live != 0) reasons.Add("recolours live: " + DustMephitBreathTint.Live);
                return reasons;
            };
            JObject row = driver.Row; record["dustPreDeliveryDisposed"] = row;
            _s7PreDeliveryState = null; row["probe"] = probe; row["cells"] = cells;
            Func<IEnumerable<int>> next = () => { nextStarted = true; return R6Wait(1); };
            mark("dust-pre-delivery-disposal-start");
            try
            {
                foreach (int step in driver.Run(
                    Sprint7ObserverPolicy.Cell("s7-dust-pre-delivery-inner", () => S7Dust(fixture, summon, hostile, inner, mark, ground, recorder, abilityData, symbols, hostileSaves, driver.Child, probe)),
                    Sprint7ObserverPolicy.Cell("s7-dust-pre-delivery-next", next), S7Cells(driver.Child, cells), TimeSpan.FromSeconds(20), TimeSpan.FromSeconds(60)))
                    yield return step;
            }
            finally
            {
                JToken dust = inner["dust"];
                row["dust"] = dust == null ? null : new JObject { ["ownedEffects"] = dust["ownedEffects"], ["ownedCommandsLeft"] = dust["ownedCommandsLeft"],
                    ["ownedCommandFailures"] = dust["ownedCommandFailures"], ["foreignPresentAtCleanup"] = dust["foreignPresentAtCleanup"],
                    ["foreignKeptAtSettlement"] = dust["foreignKeptAtSettlement"], ["conditionsSubscribedAfterSettlement"] = dust["conditionsSubscribedAfterSettlement"],
                    ["lateRemovedAtSettlement"] = dust["lateRemovedAtSettlement"], ["cleanupFailures"] = dust["cleanupFailures"], ["deferredChecks"] = dust["deferredChecks"] };
                row["nextCellStarted"] = nextStarted;
                mark("dust-pre-delivery-disposal-end");
            }
            Sprint25Check("s7-dust-pre-delivery-disposal-accounted", nextStarted && driver.Proven, (JObject)row.DeepClone(),
                "An ordinary Dust cell disposed after its own cast's execution started and before its first delivery: the cell's release retains that exact execution, keeps the condition ledger subscribed and, once the execution has settled, releases any condition it delivered late (none remains), keeps the foreign control through that settlement and then unsubscribes; with nothing failed, the same admission guard admits the next cell, which runs.");
        }

        private IEnumerable<int> S7Samples(UnitEntityData anchor, JObject record, Action<string> mark, string group, string[] events)
        {
            var rows = new JArray(); record["samples-" + group] = rows;
            foreach (string name in events)
            {
                mark("sample-" + group + "-" + name);
                uint id = AkSoundEngine.PostEvent(name, anchor.View.gameObject);
                rows.Add(new JObject { ["event"] = name, ["playingId"] = id, ["utc"] = DateTime.UtcNow.ToString("o"), ["listenerDistance"] = S7ListenerDistance(anchor),
                    ["anchor"] = anchor.Blueprint.name, ["label"] = "observer listening sample, not a product cue" });
                foreach (int step in R6Seconds(4.5f)) yield return step;
            }
        }

        // Recovery C3 (C3 review R1-F1, R1-F12): state of the running effects phase, read by its cells: the phase's one
        // retirement record and retire action (shared with the cells' direct retirements and with the phase's and fixture's
        // teardown), and whether the mechanics-only selection runs. Set when the phase starts and cleared when it ends.
        private Sprint7OwnedRequests<UnitEntityData> _s7Retirements;
        private Action<UnitEntityData> _s7Retire;
        private bool _s7Mechanics;
        // Recovery C3 (R3-F3): the phase's owned units (summons and the Dust outside target), retired at teardown if no cell did.
        private List<UnitEntityData> _s7PhaseOwned;

        // Recovery C3 (R2-F3): the pre-delivery control's readback of its exact execution (set by the Dust cell in that mode).
        private Func<JObject> _s7PreDeliveryState;

        /// <summary>Recovery C3 (R1-F9): one phase set-up acquisition; its failure is named in the phase's own result, then rethrown.</summary>
        private IEnumerable<int> ReviewSprint7Effects(ExpandedSummoningCorrectionFixture fixture, string phase, Cleanup2NativeDefaultScope mode)
        {
            bool mechanics = phase == ExpandedSummoningNativeProfilePolicy.Sprint39S7EffectsMechanicsPhase;
            if (!IsSprint32Review || !_request.ExitAfterCompletion || !ReferenceEquals(fixture, _sprint25Fixture) ||
                fixture?.Scene == null || fixture.Caster == null || fixture.Hostile == null || mode == null || !mode.Current ||
                !_context.FeatureModules.Active.ExpandedSummoning || (phase != ExpandedSummoningNativeProfilePolicy.Sprint39S7EffectsPhase && !mechanics) ||
                !Cleanup2NoNarrativeOverlay())
                throw new InvalidOperationException("Exact fresh Sprint 7 effects fixture in owned outdoor native Default required.");
            IDictionary<string, BlueprintScriptableObject> symbols = ExpandedSummoningBlueprints.Registered.BySymbol;
            string[] required = Sprint7ObserverPolicy.RequiredEffectsCells(mechanics);
            var record = new JObject { ["phase"] = phase, ["runId"] = _request.RunId, ["requiredCells"] = new JArray(required),
                ["selection"] = mechanics
                    ? "mechanics-only: the ownership, command/process/hook, cue, recolour-restoration and foreign-effect cells; no frame captures, no native-look comparison, no listening samples"
                    : "full presentation phase",
                ["limits"] = "Commanded attacks and casts with brains inactive in real time. Cue records are native playing IDs and end-of-event callbacks; audibility and suitability need listening. Samples are observer-posted for comparison only." };
            var timeline = new JArray(); record["timeline"] = timeline;
            Action<string> mark = name => timeline.Add(new JObject { ["mark"] = name, ["utc"] = DateTime.UtcNow.ToString("o"), ["frame"] = Time.frameCount });
            var owned = new List<UnitEntityData>();
            _s7PhaseOwned = owned;
            var isolation = new Sprint7CellIsolation();
            // R1-F9: the phase's set-up acquisitions are owned by this scope before each one runs.
            var setup = new Sprint7SetupScope();
            var retirements = new Sprint7OwnedRequests<UnitEntityData>();
            // R1-F11: the production retirement operation records the attempt in the fixture's collection before the disposal.
            Action<UnitEntityData> retire = unit => Sprint7ObserverPolicy.RetireRecorded(fixture.RetirementAttempted, unit, value => DisposeExpandedSummoningUnits(fixture.Created, new[] { value }));
            _s7Retirements = retirements; _s7Retire = retire; _s7Mechanics = mechanics;
            _s7Issued = new Sprint7IssuedCommands<UnitEntityData, Kingmaker.UnitLogic.Commands.Base.UnitCommand>();
            Vector3 centre = fixture.Caster.Position;
            UnitEntityData hostile = fixture.Hostile;
            Vector3 hostilePosition = hostile.Position;
            int hostileHitPoints = hostile.Descriptor.Stats.HitPoints.BaseValue;
            var recorder = new R4MephitRecorder();
            var saves = new List<Sprint26Saves>();
            Func<Vector3, Vector3> ground = value => ObstacleAnalyzer.GetNearestNode(value).clampedPosition;
            Func<string, UnitEntityData> summon = key =>
            {
                UnitEntityData unit = CastExpandedSummoningOwnTier(fixture, key); owned.Add(unit);
                Sprint28SetBrainActivePreservingActions(unit, false);
                unit.Commands.InterruptAll(command => true); unit.Commands.RemoveFinishedAndUpdateQueue();
                return unit;
            };
            Func<UnitEntityData, BlueprintAbility, AbilityData> abilityData = (unit, blueprint) =>
                unit.Descriptor.Abilities.Enumerable.Single(value => ReferenceEquals(value.Blueprint, blueprint)).Data;
            try
            {
                // R1-F9 (audit A1-F7): the phase's set-up through the shared guarded sequence: each acquisition's release is
                // registered before it runs and its error is published to the phase; the saves handler exists (and is listed)
                // before its subscription; the ground lookup and baseline placement are the last acquisition (release restores
                // the position).
                Sprint26Saves hostileSaves = null;
                Sprint7ObserverPolicy.EffectsPhaseSetup(setup, isolation,
                    () => hostile.Descriptor.Stats.HitPoints.BaseValue = 100000,
                    () => { if (!fixture.Hostile.Destroyed) fixture.Hostile.Descriptor.Stats.HitPoints.BaseValue = hostileHitPoints; },
                    () => EventBus.Subscribe(recorder), () => EventBus.Unsubscribe(recorder),
                    () => { hostileSaves = new Sprint26Saves(hostile, false) { DesiredRoll = 1 }; saves.Add(hostileSaves); },
                    () => hostileSaves.Subscribe(), () => hostileSaves.Dispose(),
                    () => PlaceExpandedSummoningUnit(hostile, ground(centre + Vector3.forward * 5f)),
                    () => { if (!fixture.Hostile.Destroyed) PlaceExpandedSummoningUnit(fixture.Hostile, hostilePosition); });
                foreach (int step in R6Wait(10)) yield return step;
                mark("baseline-start");
                foreach (int step in R6Seconds(3f)) yield return step;
                mark("baseline-end");
                // Owner amendment 3 (R4-F1, R6-F2): one cell sequence. Every cell publishes its cleanup to this one isolation from
                // its own finally; before each cell the residuals are re-read and the cell is admitted only when the isolation is
                // restored. Recovery C3 (R1-F8): the final verdict also requires every declared required cell to have been
                // admitted and completed; a later settlement never conceals a refused or skipped required cell.
                var cells = new List<KeyValuePair<string, Func<IEnumerable<int>>>> {
                    Sprint7ObserverPolicy.Cell("s7-ankylosaurus", () => S7Ankylosaurus(fixture, summon, hostile, record, mark, ground, isolation)),
                    Sprint7ObserverPolicy.Cell("s7-callback-abandoned", () => S7CallbackAbandoned(fixture, summon, hostile, record, mark, ground, isolation)),
                    Sprint7ObserverPolicy.Cell("s7-callback-setup-failed", () => S7CallbackSetupFailed(fixture, summon, hostile, record, mark, ground, isolation)),
                    Sprint7ObserverPolicy.Cell("s7-trace-setup-failed", () => S7TraceSetupFailed(record, mark, isolation)),
                    Sprint7ObserverPolicy.Cell("s7-dust", () => S7Dust(fixture, summon, hostile, record, mark, ground, recorder, abilityData, symbols, saves[0], isolation)),
                    Sprint7ObserverPolicy.Cell("s7-dust-pre-delivery-disposed", () => S7DustPreDeliveryDisposed(fixture, summon, hostile, record, mark, ground, recorder, abilityData, symbols, saves[0], isolation)),
                    Sprint7ObserverPolicy.Cell("s7-dust-abandoned", () => S7DustAbandoned(fixture, summon, hostile, record, mark, ground, recorder, abilityData, symbols, saves[0], isolation)) };
                if (!mechanics)
                    cells.Add(Sprint7ObserverPolicy.Cell("s7-samples", () =>
                    {
                        // The caster is the emitter: a summon can reach its native expiry during the samples.
                        UnitEntityData anchor = fixture.Caster;
                        UnitEntityData trumpet = summon("trumpet-archon");
                        PlaceExpandedSummoningUnit(trumpet, ground(centre + Vector3.right * 2f));
                        return S7Samples(anchor, record, mark, "impact", new[] { AnkylosaurusTailImpactPolicy.ImpactCue, "Physical_Hit_03_Punch", "Earthblast_Hit", "DustCritHeavy", "MediumBluntNatural_Flesh" })
                            .Concat(S7Samples(trumpet, record, mark, "trumpet", new[] { SummonPlaytestR6Policy.TrumpetBlastCue, "InspireGreatness", "CacophonousCall", "AoE_Sonic_Large", "Sweetener_High_04_Celestial", "HolyBomb", "Heroism" }));
                    }));
                foreach (int step in Sprint7ObserverPolicy.Sequence(isolation, cells, S7Cells(isolation), TimeSpan.FromSeconds(20))) yield return step;
                if (isolation.Refused != null) record["stopped"] = isolation.Refused + " refused: isolation not restored; it and later cells not run";
                // The last cell's residuals, re-read before the final restoration decision.
                foreach (int step in Sprint7ObserverPolicy.Settle(isolation, TimeSpan.FromSeconds(20))) yield return step;
            }
            finally
            {
                var errors = new JArray();
                Action<string, Action> restore = (name, action) => { try { action(); } catch (Exception error) { errors.Add(name + ":" + error.Message); } };
                // The fixture's caster and hostile only leave combat (their commands are not the phase's).
                // R2-F7: only the commands this runner issued, by reference; interruption, queue maintenance and combat exit are each
                // attempted on their own per command and per actor; another owner's command on the same actor is not touched.
                // Audit A1-F6: queue maintenance and combat exit (whose native return move can clear the queue; audit-sourced) are
                // each withheld (and reported, never recorded as performed) on an actor whose queue holds a command not issued here;
                // the same guard covers the fixture caster's and hostile's combat exit. Audit A2-F3: before each interruption the
                // command's original owner retains its readable execution process.
                Func<Kingmaker.UnitLogic.Commands.Base.UnitCommand, string> commandName = command => command.GetType().Name + "#" + System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(command);
                Action<UnitEntityData> leaveCombat = unit => { if (unit.CombatState.IsInCombat) unit.CombatState.LeaveCombat(); };
                if (_s7Issued != null)
                {
                    foreach (string failure in _s7Issued.Release(owned, unit => unit != null && !unit.Destroyed,
                        command => command.IsFinished, (unit, command) => unit.Commands.InterruptAll(value => ReferenceEquals(value, command)),
                        unit => unit.Commands.Queue.ToList(), unit => unit.Commands.RemoveFinishedAndUpdateQueue(), leaveCombat,
                        unit => unit.UniqueId, commandName)) errors.Add(failure);
                    foreach (string failure in _s7Issued.ExitCombat(new[] { fixture.Hostile, fixture.Caster }, unit => unit != null && !unit.Destroyed,
                        unit => unit.Commands.Queue.ToList(), leaveCombat, unit => unit.UniqueId, commandName)) errors.Add(failure);
                    record["commandCleanupWithheld"] = new JArray(_s7Issued.Withheld.ToArray());
                    record["commandCleanupPerformed"] = new JArray(_s7Issued.Performed.ToArray());
                }
                else
                    foreach (string failure in Sprint7ObserverPolicy.ReleaseEach(new[] { fixture.Hostile, fixture.Caster }.Where(value => value != null && !value.Destroyed),
                        unit => "combat " + unit.UniqueId, leaveCombat)) errors.Add(failure);
                restore("hostile damage", () => { if (!fixture.Hostile.Destroyed) fixture.Hostile.Descriptor.Damage = 0; });
                restore("caster", () => PlaceExpandedSummoningUnit(fixture.Caster, centre));
                foreach (string failure in setup.ReleaseAll()) errors.Add(failure);
                // R1-F1: each phase-owned unit no layer has requested yet is retired here, each on its own (one request per unit
                // across the cells, this teardown and the fixture's teardown, which skips units whose retirement was attempted);
                // every request's settlement is read by identity into the phase's result.
                // Audit A1-F3: residuals still pending (including a reconciliation a driver handed to the phase, which reads its
                // controls and retires its outside target) are settled or run their bounded teardown first; only then is each
                // unit no layer requested retired, published and accounted for once more.
                int retiredHere = 0;
                foreach (string failure in Sprint7ObserverPolicy.PhaseTeardownSettlement(isolation,
                    () => retiredHere = retirements.ReleaseOutstanding(owned.Where(value => value != null && !value.Destroyed), value => "phase unit " + value.UniqueId, retire),
                    () => retirements.Publish(isolation, "phase teardown", S7UnitSettled))) errors.Add(failure);
                _s7Retirements = null; _s7Retire = null; _s7Mechanics = false; _s7PreDeliveryState = null; _s7Issued = null; _s7PhaseOwned = null;
                record["isolation"] = isolation.Describe();
                record["retirements"] = new JObject { ["requested"] = retirements.Count, ["retiredAtTeardown"] = retiredHere, ["failures"] = new JArray(retirements.Failures.ToArray()) };
                // R1-F8: completion of the declared qualification, kept apart from eventual physical restoration.
                bool requiredComplete = Sprint7ObserverPolicy.RequiredCellsComplete(isolation, required);
                bool physical = errors.Count == 0 && DustMephitBreathTint.Live == 0 && isolation.Restored;
                record["requiredCellsCompleted"] = requiredComplete;
                record["physicalRestoration"] = physical;
                Sprint25Check("s7-effects-required-cells-completed", requiredComplete,
                    new JObject { ["required"] = new JArray(required), ["admitted"] = new JArray(isolation.Admitted.ToArray()), ["completed"] = new JArray(isolation.Completed.ToArray()),
                        ["refused"] = isolation.Refused },
                    "Every declared required cell of the selected effects profile was admitted by the phase's admission guard and ran to its end; none was refused or skipped (a later settlement does not count).");
                Sprint25Check("s7-effects-owned-state-restored", physical,
                    new JObject { ["errors"] = errors, ["liveRecolours"] = DustMephitBreathTint.Live, ["isolation"] = isolation.Describe() },
                    "Readers, combat, positions and recolours restore, and the phase's one cumulative isolation result is restored: every cell reported its cleanup, no named failure, no residual still live or unreadable (the callback hook, the projectile trace's hooks, the cell's conditions, commands, executions and subscriptions, each source's retirement settled, and each driver's proof of its own repair).");
                record["restoreErrors"] = errors;
                // Every cue record at the end of the phase, so a late or missing native end is visible.
                record["cuesAtEnd"] = R6Cues(); record["cuesAtEndUtc"] = DateTime.UtcNow.ToString("o");
                RuntimeTestResultWriter.WriteAtomic(Path.Combine(_request.EvidenceDirectory, phase + ".json"), record.ToString(Formatting.Indented));
            }
        }
    }
}
