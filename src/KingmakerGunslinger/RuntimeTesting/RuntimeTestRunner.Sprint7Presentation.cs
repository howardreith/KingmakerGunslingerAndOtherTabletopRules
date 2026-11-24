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
            return new JObject { ["scenery"] = scenery, ["otherUnits"] = others, ["distance"] = Math.Round(distance, 2) };
        }

        private static readonly float[] S7YawOffsets = { 0f, 15f, -15f, 30f, -30f, 45f, -45f, 60f, -60f };

        private static Vector3 S7ClearDirection(UnitEntityData unit, Bounds bounds, Vector3 nominal, float minimumRadius, bool detail, out JObject measured)
        {
            JObject best = null; Vector3 chosen = nominal; int bestScore = int.MaxValue;
            foreach (float yaw in S7YawOffsets)
            {
                Vector3 direction = Quaternion.Euler(0f, yaw, 0f) * nominal;
                JObject hits = S7Occluders(unit, bounds, direction, minimumRadius);
                int score = (int)hits["scenery"] + (detail ? (int)hits["otherUnits"] : 0);
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
            row["party"] = WriteExpandedSummoningPartyCameraCapture(unit, _request.EvidenceDirectory,
                "s7-" + model.Key + "-" + scene + "-" + state + "-party.png");
            row["dissolve"] = S7MaxDissolve(unit);
            SkinnedMeshRenderer body = S7Body(unit);
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
                // Details must be clear of scenery and of other units; whole-body
                // views of scenery.
                bool clear = new[] { after, before }.All(values => ((JObject)values["occluders"]).Properties().All(value =>
                    (int)value.Value["scenery"] == 0 && (!value.Name.StartsWith("detail-", StringComparison.Ordinal) || (int)value.Value["otherUnits"] == 0)));
                row["clearOfOccluders"] = clear;
                bool moved = (float)motionAfter["moveTravel"] > 1f && (float)motionBefore["moveTravel"] > 1f;
                bool turned = (float)motionAfter["turnDegrees"] > 45f && (float)motionBefore["turnDegrees"] > 45f;
                bool attacked = hostile == null || ((bool)motionAfter["attackActed"] && (bool)motionBefore["attackActed"]);
                bool different = swap.CurrentVertices != swap.BeforeVertices || model.Salamander;
                Sprint25Check("s7-" + model.Key + "-" + scene, shown && settled && framed && clear && moved && turned && attacked && different,
                    (JObject)row.DeepClone(),
                    "The summon cast through the published spell completes its native appearance on its own (no buff removed) and then shows the Sprint 7 export on its enabled body renderer (vertex count bound to the export; every body material reads _Dissolve 0); the pinned pre-Sprint-7 export is shown on the same live unit; whole-body views and every named-bone detail are framed and clear of scenery, details also of other units (each view keeps its nominal bearing or the nearest yaw offset up to 60 degrees that is clear, recorded); and both complete the observed idle, movement, a turn of more than 45 degrees and any attack.");
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
            normal.Dispose();
            int normalAfter = normal.OwnedHooks();
            row["trace"] = new JObject { ["error"] = traceError, ["original"] = traceInner, ["rollback"] = traceRollback == null ? null : traceRollback.DeepClone(),
                ["normalInstalled"] = normalInstalled, ["normalAfterDispose"] = normalAfter };
            bool swapOk = swapError != null && swapError.Contains("rollback complete") && swapInner == "injected failure after comparison-mesh allocation" &&
                meshDestroyed && rendererRestored && S7BodySwap.LastRollback != null && (bool)S7BodySwap.LastRollback["comparisonMeshAllocated"];
            bool traceOk = traceError != null && traceError.Contains("rollback complete") && traceInner == "injected failure after the first patch" &&
                traceRollback != null && (int)traceRollback["hooksBeforeRollback"] >= 1 && (int)traceRollback["hooksAfterRollback"] == 0 &&
                ((JArray)traceRollback["failures"]).Count == 0 && normalInstalled == 4 && normalAfter == 0;
            Sprint25Check("s7-observer-rollback", swapOk && traceOk, (JObject)row.DeepClone(),
                "A body swap that fails after allocating its comparison mesh and putting it on the renderer surfaces the original failure with a complete rollback: the renderer is back on its own mesh and the comparison mesh is destroyed. A projectile trace that fails after its first patch surfaces the original failure with no owned hook left; a normal trace then installs its 4 hooks and removes all of them.");
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
                ["limits"] = "Summons are cast through the published spell as native commands (caster level 20 only); the arrival and appearance complete naturally before the unit is moved to its mark. Body readbacks name the enabled body renderer; the party capture's own readback (shared helper) may name another renderer. Commanded motion with brains inactive. Images are presentation evidence only; aesthetic approval is the owner's." };
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
                    ["hit"] = evt.AttackRoll != null && evt.AttackRoll.IsHit, ["utc"] = DateTime.UtcNow.ToString("o"), ["frame"] = Time.frameCount });
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

        private static bool S7IsTail(JToken row) { return ((string)row["weapon"] ?? "").IndexOf("Tail", StringComparison.OrdinalIgnoreCase) >= 0; }

        private static int S7TailHits(S7AttackLog log, UnitEntityData unit, int from)
        {
            return log.Rows.Skip(from).Count(value => (string)value["initiator"] == unit.UniqueId && (bool)value["hit"] && S7IsTail(value));
        }

        private static JObject S7Cue(SummonCastCueRecord cue)
        {
            return new JObject { ["event"] = cue.EventName, ["playingId"] = cue.PlayingId, ["frame"] = cue.Frame, ["utc"] = cue.Utc.ToString("o"),
                ["endUtc"] = cue.EndUtc.HasValue ? cue.EndUtc.Value.ToString("o") : null, ["stopReason"] = cue.StopReason,
                ["stopRequestedUtc"] = cue.StopRequestedUtc.HasValue ? cue.StopRequestedUtc.Value.ToString("o") : null,
                ["stopResult"] = cue.StopResult, ["seconds"] = cue.EndUtc.HasValue ? (cue.EndUtc.Value - cue.Utc).TotalSeconds : (double?)null };
        }

        private IEnumerable<int> S7AttackUntil(UnitEntityData unit, UnitEntityData target, Func<bool> done, int attempts)
        {
            for (int attempt = 0; attempt < attempts && !done(); attempt++)
            {
                target.Descriptor.Damage = 0;
                var attack = new UnitAttack(target);
                unit.Commands.Run(attack);
                DateTime deadline = DateTime.UtcNow.AddSeconds(8);
                while (DateTime.UtcNow < deadline && !attack.IsFinished) yield return 0;
                unit.Commands.InterruptAll(command => true); unit.Commands.RemoveFinishedAndUpdateQueue();
                foreach (int step in R6Seconds(.6f)) yield return step;
            }
        }

        // The production tail handler on the unit's own fact, called again
        // with a recorded native rule: the handler's own owner, weapon, target
        // and first-observation checks decide.
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
            UnitEntityData hostile, JObject record, Action<string> mark, Func<Vector3, Vector3> ground)
        {
            var row = new JObject(); record["ankylosaurus"] = row;
            var log = new S7AttackLog(); EventBus.Subscribe(log);
            int armor = hostile.Descriptor.Stats.AC.BaseValue;
            try
            {
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
                var pairs = new JArray(hits.Select(hit => new JObject { ["hit"] = hit.DeepClone(), ["listenerDistance"] = S7ListenerDistance(first),
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
                    var roll = new RuleAttackRoll(first, rollTarget, weapon, 0);
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
                    Kingmaker.Items.ItemEntityWeapon tail = qualifying.Weapon, unarmed = first.Body.EmptyHandWeapon;
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
                    (int)crafted[name]["newCues"] == count && crafted[name]["error"].Type == JTokenType.Null;
                Sprint25Check("s7-anky-production-controls", posted("valid", 1) && posted("foreignWeapon", 0) && posted("mismatchedWrapper", 0) &&
                    posted("sameRollValidWrapper", 1) && posted("removedTarget", 0), (JObject)crafted.DeepClone(),
                    "Through the owner's own production handler: a valid fresh resolved tail roll posts one cue; the same owner's unarmed weapon posts none; a wrapper naming another creature than its roll's target posts none and does not consume the roll, whose valid wrapper then posts its one cue; an out-of-game target posts none.");

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
                first.Commands.Run(cancelled);
                yield return 0;
                first.Commands.InterruptAll(command => true); first.Commands.RemoveFinishedAndUpdateQueue();
                foreach (int step in R6Seconds(2f)) yield return step;
                mark("anky-cancel-end");
                int cancelAttacks = log.Rows.Skip(logFrom).Count(value => (string)value["initiator"] == first.UniqueId);
                int cancelCues = S7ImpactCues(first).Length - cuesFrom;
                Sprint25Check("s7-anky-cancel-no-cue", cancelAttacks == 0 && cancelCues == 0,
                    new JObject { ["attacks"] = cancelAttacks, ["cues"] = cancelCues }, "An attack interrupted before it resolves posts no cue.");

                // 5. Concurrent sources; one retired while its own cue plays.
                int firstFrom = S7ImpactCues(first).Length, secondCueFrom = S7ImpactCues(second).Length;
                mark("anky-concurrent-start");
                first.Commands.Run(new UnitAttack(hostile));
                second.Commands.Run(new UnitAttack(hostile));
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
                DateTime retiredAt = DateTime.UtcNow;
                mark("anky-retire");
                DisposeExpandedSummoningUnits(fixture.Created, new[] { second });
                DateTime ends = DateTime.UtcNow.AddSeconds(10);
                while (DateTime.UtcNow < ends && !(retiring != null && retiring.EndUtc.HasValue && survivor != null && survivor.EndUtc.HasValue)) yield return 0;
                row["cuesAfterRetirement"] = new JObject { ["first"] = new JArray(S7ImpactCues(first).Select(S7Cue)), ["second"] = new JArray(S7ImpactCues(second).Select(S7Cue)), ["frame"] = Time.frameCount };
                first.Commands.InterruptAll(command => true); first.Commands.RemoveFinishedAndUpdateQueue();
                mark("anky-concurrent-end");
                row["retired"] = retiring == null ? null : S7Cue(retiring);
                row["survivor"] = survivor == null ? null : S7Cue(survivor);
                row["overlappingAtRetirement"] = overlapping; row["retiredUtc"] = retiredAt.ToString("o");
                // The R6 retirement rule (the strike had resolved: its cue exists).
                bool retiredStopped = retiring != null && SummonPlaytestR6Policy.CueRetirementObserved(retiring, retiredAt, true, overlapping, retiring.PlayingId, 1d);
                bool survivorNatural = survivor != null && survivor.EndUtc.HasValue && survivor.EndUtc.Value > retiredAt && survivor.StopReason == null;
                Sprint25Check("s7-anky-concurrent-retirement", overlapping && retiredStopped && survivorNatural,
                    new JObject { ["retired"] = row["retired"] == null ? null : row["retired"].DeepClone(), ["survivor"] = row["survivor"] == null ? null : row["survivor"].DeepClone(),
                        ["overlappingAtRetirement"] = overlapping, ["retiredUtc"] = retiredAt.ToString("o") },
                    "With both Ankylosaurus cues live at once, retiring one stops its own playing ID (stop requested and native end within a second) while the other plays past the retirement and ends naturally.");
            }
            finally
            {
                hostile.Descriptor.Stats.AC.BaseValue = armor;
                EventBus.Unsubscribe(log);
            }
        }

        // Read-only record of every projectile creation and the effects
        // spawned inside it. Native CreateView spawns the visible cone (the
        // projectile's cast effect) through FxHelper.SpawnFxOnPoint. A first
        // postfix on the spawn reads the instance before the product's
        // postfix; a last postfix reads it as the product left it; the
        // creation is closed after CreateView returns, with the launch point,
        // the controller's previous-launch-point state at entry and the
        // effect's orientation. Instances are kept to read their restoration.
        private sealed class S7ProjectileTrace : IDisposable
        {
            private static S7ProjectileTrace active;
            private static Kingmaker.Controllers.Projectiles.Projectile current;
            private static JObject creation;
            private static readonly System.Reflection.FieldInfo PreviousLaunch = typeof(Kingmaker.Controllers.Projectiles.ProjectileController)
                .GetField("m_PrevLaunchPos", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            private readonly Harmony12.HarmonyInstance harmony;
            private readonly string id = "KMG.S7.Projectile." + Guid.NewGuid().ToString("N");
            private readonly System.Reflection.MethodInfo create = typeof(Kingmaker.Controllers.Projectiles.ProjectileController)
                .GetMethod("CreateView", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic |
                    System.Reflection.BindingFlags.Instance, null,
                    new[] { typeof(Kingmaker.Controllers.Projectiles.Projectile), typeof(Vector3?) }, null);
            private readonly System.Reflection.MethodInfo spawn = typeof(Kingmaker.Visual.Particles.FxHelper).GetMethod("SpawnFxOnPoint",
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static, null,
                new[] { typeof(GameObject), typeof(Vector3), typeof(Quaternion) }, null);
            internal readonly JArray Creations = new JArray();
            internal readonly List<KeyValuePair<JObject, GameObject>> Instances = new List<KeyValuePair<JObject, GameObject>>();

            // Bounded fault injection for the rollback check only.
            internal static string Inject;
            internal static JObject LastRollback;

            internal S7ProjectileTrace()
            {
                if (active != null || create == null || spawn == null || PreviousLaunch == null) throw new InvalidOperationException("Projectile readers unavailable.");
                harmony = Harmony12.HarmonyInstance.Create(id);
                var flags = System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic;
                try
                {
                    harmony.Patch(create, new Harmony12.HarmonyMethod(typeof(S7ProjectileTrace).GetMethod("Before", flags)) { prioritiy = Harmony12.Priority.First },
                        new Harmony12.HarmonyMethod(typeof(S7ProjectileTrace).GetMethod("AfterCreate", flags)) { prioritiy = Harmony12.Priority.Last });
                    if (Inject == "after-first-patch") throw new InvalidOperationException("injected failure after the first patch");
                    harmony.Patch(spawn, null, new Harmony12.HarmonyMethod(typeof(S7ProjectileTrace).GetMethod("BeforeProduct", flags)) { prioritiy = Harmony12.Priority.First });
                    harmony.Patch(spawn, null, new Harmony12.HarmonyMethod(typeof(S7ProjectileTrace).GetMethod("AfterSpawn", flags)) { prioritiy = Harmony12.Priority.Last });
                    active = this;
                }
                catch (Exception original)
                {
                    // No owned hook survives a failed set-up; an incomplete
                    // rollback is reported with the original cause.
                    int installed = OwnedHooks();
                    List<string> failures = Unhook();
                    int remaining = OwnedHooks();
                    LastRollback = new JObject { ["id"] = id, ["hooksBeforeRollback"] = installed, ["hooksAfterRollback"] = remaining, ["failures"] = new JArray(failures) };
                    throw Sprint7ObserverPolicy.SetupFailure("S7ProjectileTrace", original, failures, remaining);
                }
            }

            private List<string> Unhook()
            {
                return Sprint7ObserverPolicy.RunReleases(new[] {
                    new KeyValuePair<string, Action>("CreateView hooks", () => harmony.Unpatch(create, Harmony12.HarmonyPatchType.All, id)),
                    new KeyValuePair<string, Action>("SpawnFxOnPoint hooks", () => harmony.Unpatch(spawn, Harmony12.HarmonyPatchType.All, id)) });
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

            internal static int HookCount(System.Reflection.MethodBase method, string owner)
            {
                var info = Harmony12.HarmonyInstance.Create(owner).GetPatchInfo(method);
                if (info == null) return 0;
                return info.Prefixes.Concat(info.Postfixes).Concat(info.Transpilers).Count(value => value.owner == owner);
            }

            internal int OwnedHooks() { return HookCount(create, id) + HookCount(spawn, id); }

            private static void Before(Kingmaker.Controllers.Projectiles.ProjectileController __instance, Kingmaker.Controllers.Projectiles.Projectile __0)
            {
                current = __0; creation = null;
                if (active == null || __0 == null || __0.Launcher == null) return;
                Vector3 previous = (Vector3)PreviousLaunch.GetValue(__instance);
                creation = new JObject { ["frame"] = Time.frameCount, ["utc"] = DateTime.UtcNow.ToString("o"),
                    ["launcher"] = __0.Launcher.UniqueId, ["launcherBlueprint"] = __0.Launcher.Blueprint.AssetGuid,
                    ["projectile"] = __0.Blueprint == null ? null : __0.Blueprint.AssetGuid,
                    ["executingAbility"] = DustMephitBreathExecutionScope.ExecutingAbility(__0.Launcher),
                    ["previousLaunchPoint"] = S7Vector(previous), ["previousLaunchPointCleared"] = previous == Vector3.zero,
                    ["unadoptedBefore"] = DustMephitBreathPresentation.Unadopted, ["effects"] = new JArray() };
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

            private static void AfterSpawn(GameObject __result)
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
                closing["unadopted"] = DustMephitBreathPresentation.Unadopted - (int)closing["unadoptedBefore"];
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

        private IEnumerable<int> S7WaitDustReleased()
        {
            DateTime deadline = DateTime.UtcNow.AddSeconds(20);
            while (DateTime.UtcNow < deadline && DustMephitBreathTint.Live > 0) yield return 0;
            foreach (int step in R6Seconds(1f)) yield return step;
        }

        private IEnumerable<int> S7Dust(ExpandedSummoningCorrectionFixture fixture, Func<string, UnitEntityData> summon, UnitEntityData hostile,
            JObject record, Action<string> mark, Func<Vector3, Vector3> ground, R4MephitRecorder recorder,
            Func<UnitEntityData, BlueprintAbility, AbilityData> abilityData, IDictionary<string, BlueprintScriptableObject> symbols, Sprint26Saves hostileSaves)
        {
            var row = new JObject(); record["dust"] = row;
            var breath = (BlueprintAbility)symbols["KMG.Summoning.Special.DustMephit.Breath"];
            var iceBreath = (BlueprintAbility)symbols["KMG.Summoning.Special.IceMephit.Breath"];
            var sickened = ResourcesLibrary.TryGetBlueprint<BlueprintBuff>("4e42460798665fd4cb9173ffa7ada323");
            if (sickened == null) throw new InvalidOperationException("Native Sickened buff missing.");
            UnitEntityData dust = summon("dust-mephit"), ice = summon("ice-mephit"), spare = summon("dust-mephit");
            UnitEntityData outside = null; Sprint26Saves outsideSaves = null; bool grantedIce = false;
            var saveLog = new S7SaveLog(); EventBus.Subscribe(saveLog);
            S7ProjectileTrace trace = null;
            Func<UnitEntityData, string> label = unit => unit == null ? null : ReferenceEquals(unit, hostile) ? "enemy-inside" :
                ReferenceEquals(unit, outside) ? "enemy-outside" : ReferenceEquals(unit, ice) ? "ally-inside" :
                ReferenceEquals(unit, fixture.Caster) ? "caster-behind" : ReferenceEquals(unit, dust) ? "dust" : unit.Blueprint.name;
            // Only Sickened instances a Dust breath of this cell applied (by
            // execution context) and the observer's own control are removed.
            var breathSickened = new List<Buff>();
            Buff foreignSickened = null;
            Action clearSickened = () => { foreach (UnitEntityData unit in new[] { hostile, ice, outside, fixture.Caster }.Where(value => value != null && !value.Destroyed))
                foreach (Buff buff in Sprint7ObserverPolicy.Acquired(unit.Descriptor.Buffs.Enumerable, breathSickened)) buff.Remove(); };
            try
            {
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
                Sprint28SetBrainActivePreservingActions(outside, false);
                outside.Stats.HitPoints.BaseValue = 100000;
                outsideSaves = new Sprint26Saves(outside) { DesiredRoll = 1 };
                // A control: Sickened from another context on the ally, which
                // every reset and the cleanup must leave in place.
                foreignSickened = ice.Descriptor.AddBuff(sickened, fixture.Caster, TimeSpan.FromMinutes(30));
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
                trace = new S7ProjectileTrace();

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
                    using (var clip = new R6Clip(_request.EvidenceDirectory, "s7-dust-cast-" + index, () => R6RenderBounds(dust, hostile, ice), null))
                    {
                        foreach (int step in R6RunPassive(dust, abilityData(dust, breath), new TargetWrapper(hostile), cast, clip)) yield return step;
                        cast["sceneryHitsOnBearing"] = clip.Occluded; cast["outOfFrame"] = clip.OutOfFrame; cast["frames"] = clip.Count;
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
                    breathSickened.AddRange(castSickened.Select(value => value.Buff));
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
                row["foreignSickenedAfterResets"] = foreignSickened != null && ice.Descriptor.Buffs.Enumerable.Contains(foreignSickened);
                Func<JObject, IEnumerable<JObject>> effects = cast => ((JArray)cast["creations"]).OfType<JObject>().SelectMany(value => ((JArray)value["effects"]).OfType<JObject>());
                bool samePlace = castRows.All(cast => Vector3.Distance(new Vector3((float)cast["unitPosition"][0], (float)cast["unitPosition"][1], (float)cast["unitPosition"][2]),
                    new Vector3((float)castRows[0]["unitPosition"][0], (float)castRows[0]["unitPosition"][1], (float)castRows[0]["unitPosition"][2])) < .001f);
                bool presented = castRows.All(cast => (int)cast["executions"] == 1 && ((JArray)cast["creations"]).Count == 1 &&
                    (string)cast["creations"][0]["executingAbility"] == breath.AssetGuid &&
                    effects(cast).Count(effect => (bool)effect["isCastFx"]) == 1 && effects(cast).All(effect => (bool?)effect["tinted"] == true &&
                        (int)effect["recolouredMaterials"] + (int)effect["recolouredParticleColours"] > 0 && (float)effect["orientationDegrees"] < 1f) &&
                    ((JArray)cast["restored"]).All(value => (bool)value["exists"] && !(bool)value["tintPresent"] && (bool)value["matchesBefore"]) && (int)cast["liveAfter"] == 0);
                bool visible = (int)castRows[0]["sceneryHitsOnBearing"] == 0 && (int)castRows[0]["outOfFrame"] == 0;
                Sprint25Check("s7-dust-casts", samePlace && presented && visible, new JObject { ["casts"] = casts.DeepClone() },
                    "Three consecutive Dust breaths executed from one unchanged position each produce one execution, one creation bound to the Dust breath, and exactly one cast effect oriented with the projectile and recoloured; on release each instance reads back exactly its pre-tint materials and particle colours; the first is framed on an unobstructed bearing with the targets.");
                bool neverCarried = castRows.All(cast => ((JArray)cast["creations"]).OfType<JObject>().All(value => (bool)value["previousLaunchPointCleared"] && (int)value["unadopted"] == 0));
                Sprint25Check("s7-dust-repeat-launch-point", samePlace && neverCarried, new JObject { ["launchPoints"] = new JArray(castRows.Select(cast => ((JArray)cast["creations"]).Count == 1 ? cast["creations"][0]["launchPoint"].DeepClone() : null)),
                        ["previousLaunchPointAtEntry"] = new JArray(castRows.Select(cast => ((JArray)cast["creations"]).Count == 1 ? cast["creations"][0]["previousLaunchPoint"].DeepClone() : null)) },
                    "At each repeated breath the native controller's previous launch point is already cleared (ProjectileController.Tick resets it every frame), so native creation spawns every execution's own effect and none is left unadopted.");
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
                Sprint25Check("s7-dust-mechanics", layout && mechanics, new JObject { ["layout"] = row["layout"].DeepClone(),
                        ["casts"] = new JArray(castRows.Select(cast => new JObject { ["desiredReflexD20"] = cast["desiredReflexD20"], ["selected"] = cast["selected"].DeepClone(),
                            ["damage"] = cast["damage"].DeepClone(), ["saves"] = cast["saves"].DeepClone(), ["sickened"] = cast["sickened"].DeepClone(), ["otherDamage"] = cast["otherDamage"] })) },
                    "Correlated to each breath's own execution context: the enemy inside the cone rolls one Reflex save; on a natural 1 it takes full 1d4 slashing and is Sickened (3 rounds), on a natural 20 half damage and no condition; the ally inside the cone takes no damage and gains no condition (its Reflex save, rolled by the released breath for every selected creature, is recorded); the enemy outside and the caster behind are never selected, roll nothing and take nothing.");
                Sprint25Check("s7-dust-action", castRows.All(cast => (string)cast["actionType"] == breath.ActionType.ToString() &&
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
                foreach (int step in R6RunPassive(dust, abilityData(dust, iceBreath), new TargetWrapper(hostile), foreign, null)) yield return step;
                foreach (int step in R6Seconds(1f)) yield return step;
                mark("dust-foreign-ability-end");
                var foreignCreations = trace.Creations.Skip(foreignFrom).OfType<JObject>().Where(value => (string)value["launcher"] == dust.UniqueId).ToList();
                foreign["creations"] = new JArray(foreignCreations.Select(value => value.DeepClone()));
                bool foreignNative = foreignCreations.Count == 1 && (string)foreignCreations[0]["executingAbility"] == iceBreath.AssetGuid &&
                    (string)foreignCreations[0]["projectile"] == MephitBreathPolicy.ColdConeProjectileGuid &&
                    ((JArray)foreignCreations[0]["effects"]).OfType<JObject>().Count() == 1 &&
                    ((JArray)foreignCreations[0]["effects"]).OfType<JObject>().All(effect => (bool?)effect["tinted"] == false && (string)effect["before"]["hash"] == (string)effect["after"]["hash"]) &&
                    DustMephitBreathTint.Applied == foreignTints;
                Sprint25Check("s7-dust-foreign-ability", foreignNative, foreign, "The Dust Mephit executing the Ice breath (same launcher, same cold cone, different ability) gets the native cone, untouched.");
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
                foreach (int step in R6RunPassive(ice, abilityData(ice, iceBreath), new TargetWrapper(hostile), iceAgain, null)) yield return step;
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
                Sprint25Check("s7-dust-pool-reuse", reusedRows.Count >= 1 && allNative, new JObject { ["dustInstances"] = new JArray(dustInstances), ["dustOriginalSignatures"] = new JArray(originals),
                        ["laterConsumers"] = new JArray(laterNative.Select(value => value.DeepClone())), ["reusedDustInstance"] = reusedRows.Count },
                    "A later consumer that is not the Dust breath claims the very pooled instance the Dust breaths used (same instance ID), and before any product code it reads exactly the native signature recorded before the first Dust tint; every later consumer stays untouched.");
                foreach (int step in S7WaitDustReleased()) yield return step;

                // 4. Cancelled before execution: no effect, no damage.
                int cancelApplied = DustMephitBreathTint.Applied, cancelFrom = trace.Creations.Count, cancelDamage = recorder.Damage.Count;
                mark("dust-cancel-start");
                var cancelled = new UnitUseAbility(abilityData(dust, breath), new TargetWrapper(hostile));
                dust.Commands.Run(cancelled);
                yield return 0;
                dust.Commands.InterruptAll(command => true); dust.Commands.RemoveFinishedAndUpdateQueue();
                foreach (int step in R6Seconds(3f)) yield return step;
                mark("dust-cancel-end");
                Sprint25Check("s7-dust-cancel", DustMephitBreathTint.Applied == cancelApplied &&
                    trace.Creations.Skip(cancelFrom).All(value => (string)value["launcher"] != dust.UniqueId) &&
                    recorder.Damage.Skip(cancelDamage).All(value => !ReferenceEquals(value.Initiator, dust)),
                    new JObject { ["applied"] = DustMephitBreathTint.Applied - cancelApplied }, "A breath interrupted before execution creates nothing and deals nothing.");

                // 5. Concurrent donor: the Ice Mephit's cold cone live at the same time.
                PlaceExpandedSummoningUnit(ice, ground(hostile.Position + Vector3.right * 6f + Vector3.back * 4f));
                foreach (int step in R6Wait(4)) yield return step;
                int concurrentFrom = trace.Creations.Count;
                mark("dust-concurrent-start");
                var dustCast = new JObject(); var iceCast = new JObject();
                IEnumerator<int> dustRun = R6RunPassive(dust, abilityData(dust, breath), new TargetWrapper(hostile), dustCast, null).GetEnumerator();
                IEnumerator<int> iceRun = R6RunPassive(ice, abilityData(ice, iceBreath), new TargetWrapper(hostile), iceCast, null).GetEnumerator();
                bool dustMore = true, iceMore = true;
                int overlapFrames = 0, restoredAt = DustMephitBreathTint.Restored, sampleFrame = 0;
                var donorTimeline = new JArray();
                DateTime concurrentDeadline = DateTime.UtcNow.AddSeconds(40);
                while ((dustMore || iceMore || DustMephitBreathTint.Live > 0) && DateTime.UtcNow < concurrentDeadline)
                {
                    if (dustMore) dustMore = dustRun.MoveNext();
                    if (iceMore) iceMore = iceRun.MoveNext();
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
                Sprint25Check("s7-dust-donor-untouched", iceNative && dustOwn && overlapFrames > 0 && donorSteady && dustRestored.All(value => (bool)value["matchesBefore"]) && DustMephitBreathTint.Live == 0,
                    new JObject { ["ice"] = new JArray(iceViews.Select(value => value.DeepClone())), ["dust"] = new JArray(dustViews.Select(value => value.DeepClone())),
                        ["overlapFrames"] = overlapFrames, ["donorTimeline"] = donorTimeline, ["dustRestored"] = new JArray(dustRestored) },
                    "With both cones live at the same time, the Dust Mephit's own instance is recoloured and restored, while the Ice Mephit's separate live instance reads its native signature at every sample, including while the Dust instance is tinted and at the moment it is restored.");

                // 5b. Restoring one Dust instance leaves another active tint alone.
                PlaceExpandedSummoningUnit(spare, ground(hostile.Position + Vector3.left * 2.6f));
                foreach (int step in R6Wait(4)) yield return step;
                int pairFrom = trace.Creations.Count;
                mark("dust-own-restore-start");
                dust.Commands.Run(new UnitUseAbility(abilityData(dust, breath), new TargetWrapper(hostile)));
                spare.Commands.Run(new UnitUseAbility(abilityData(spare, breath), new TargetWrapper(hostile)));
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
                Sprint25Check("s7-dust-own-instance-restore", chosen != null && (bool?)ownRestore["chosenNative"] == true && (bool?)ownRestore["otherStillTinted"] == true &&
                    (bool?)ownRestore["otherRestoredOnRelease"] == true, ownRestore,
                    "With two Dust cones live, restoring one exact instance returns it to its native signature and leaves the other instance's tint in place until its own release.");

                // 6. Source loss while the effect is active.
                int lossApplied = DustMephitBreathTint.Applied, lossFrom = trace.Creations.Count;
                mark("dust-loss-start");
                PlaceExpandedSummoningUnit(spare, ground(hostile.Position + Vector3.left * 2.6f));
                foreach (int step in R6Wait(4)) yield return step;
                spare.Commands.Run(new UnitUseAbility(abilityData(spare, breath), new TargetWrapper(hostile)));
                DateTime waitView = DateTime.UtcNow.AddSeconds(12);
                while (DateTime.UtcNow < waitView && DustMephitBreathTint.Applied == lossApplied) yield return 0;
                bool launched = DustMephitBreathTint.Applied > lossApplied;
                mark("dust-loss-despawn");
                DisposeExpandedSummoningUnits(fixture.Created, new[] { spare });
                foreach (int step in S7WaitDustReleased()) yield return step;
                mark("dust-loss-end");
                var lossEffects = trace.Creations.Skip(lossFrom).OfType<JObject>().SelectMany(value => ((JArray)value["effects"]).OfType<JObject>()).ToArray();
                var lossRestored = lossEffects.Select(effect => trace.Restoration(effect)).ToArray();
                Sprint25Check("s7-dust-source-loss", launched && DustMephitBreathTint.Live == 0 && lossEffects.Length == 1 &&
                    lossRestored.All(value => (bool)value["exists"] && (bool)value["matchesBefore"] && !(bool)value["tintPresent"]) &&
                    UnityEngine.Object.FindObjectsOfType<DustMephitBreathTint>().Length == 0,
                    new JObject { ["launched"] = launched, ["live"] = DustMephitBreathTint.Live, ["effects"] = new JArray(lossEffects.Select(value => value.DeepClone())), ["restored"] = new JArray(lossRestored) },
                    "A Dust Mephit removed while its breath effect is active leaves the instance reading its native signature once the effect ends.");

                // 7. A native-look comparison of the same Dust cast: the
                // observer restores the recolour as soon as it is applied.
                var nativeLook = new JObject(); row["nativeLook"] = nativeLook;
                int lookApplied = DustMephitBreathTint.Applied;
                mark("dust-native-look-start");
                using (var clip = new R6Clip(_request.EvidenceDirectory, "s7-dust-native-look", () => R6RenderBounds(dust, hostile), null))
                {
                    var command = new UnitUseAbility(abilityData(dust, breath), new TargetWrapper(hostile));
                    dust.Commands.Run(command);
                    DateTime deadline = DateTime.UtcNow.AddSeconds(20);
                    bool restored = false;
                    int lookFrom = trace.Creations.Count;
                    while (DateTime.UtcNow < deadline && !(command.IsFinished && (command.ExecutionProcess == null || command.ExecutionProcess.IsEnded)))
                    {
                        // Only this comparison cast's own instance is restored.
                        JObject own = trace.Creations.Skip(lookFrom).OfType<JObject>().Where(value => (string)value["launcher"] == dust.UniqueId)
                            .SelectMany(value => ((JArray)value["effects"]).OfType<JObject>()).FirstOrDefault(effect => (bool?)effect["tinted"] == true);
                        if (!restored && own != null && trace.Instance(own) != null)
                        {
                            DustMephitBreathTint tint = trace.Instance(own).GetComponent<DustMephitBreathTint>();
                            if (tint != null) tint.Restore();
                            restored = true;
                            nativeLook["instance"] = own["instance"];
                        }
                        clip.Shot(R6AnimationState(dust, command));
                        yield return 0;
                    }
                    nativeLook["restoredByObserver"] = restored; nativeLook["sceneryHitsOnBearing"] = clip.Occluded;
                }
                mark("dust-native-look-end");
            }
            finally
            {
                if (trace != null) { row["ownedHooksBeforeDispose"] = trace.OwnedHooks(); trace.Dispose(); row["ownedHooksAfterDispose"] = trace.OwnedHooks(); row["disposeFailures"] = new JArray(trace.DisposeFailures); }
                EventBus.Unsubscribe(saveLog);
                if (outsideSaves != null) outsideSaves.Dispose();
                hostileSaves.DesiredRoll = 1;
                if (grantedIce && !dust.Destroyed && dust.Descriptor.HasFact(iceBreath)) dust.Descriptor.RemoveFact(iceBreath);
                clearSickened();
                bool foreignKept = foreignSickened != null && !ice.Destroyed && ice.Descriptor.Buffs.Enumerable.Contains(foreignSickened);
                if (foreignSickened != null && !ice.Destroyed && foreignKept) foreignSickened.Remove();
                Sprint25Check("s7-dust-foreign-sickened-kept", foreignSickened != null && (bool?)row["foreignSickenedAfterResets"] == true && foreignKept,
                    new JObject { ["afterResets"] = row["foreignSickenedAfterResets"], ["afterCleanup"] = foreignKept },
                    "A Sickened condition from another context on the ally survives every between-cast reset and the cell's cleanup; only breath-owned instances are removed (the observer then removes its own control).");
                if (outside != null && !outside.Destroyed) CleanupExpandedSummoningUnit(outside);
            }
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

        private IEnumerable<int> ReviewSprint7Effects(ExpandedSummoningCorrectionFixture fixture, string phase, Cleanup2NativeDefaultScope mode)
        {
            if (!IsSprint32Review || !_request.ExitAfterCompletion || !ReferenceEquals(fixture, _sprint25Fixture) ||
                fixture?.Scene == null || fixture.Caster == null || fixture.Hostile == null || mode == null || !mode.Current ||
                !_context.FeatureModules.Active.ExpandedSummoning || phase != ExpandedSummoningNativeProfilePolicy.Sprint39S7EffectsPhase ||
                !Cleanup2NoNarrativeOverlay())
                throw new InvalidOperationException("Exact fresh Sprint 7 effects fixture in owned outdoor native Default required.");
            IDictionary<string, BlueprintScriptableObject> symbols = ExpandedSummoningBlueprints.Registered.BySymbol;
            var record = new JObject { ["phase"] = phase, ["runId"] = _request.RunId,
                ["limits"] = "Commanded attacks and casts with brains inactive in real time. Cue records are native playing IDs and end-of-event callbacks; audibility and suitability need listening. Samples are observer-posted for comparison only." };
            var timeline = new JArray(); record["timeline"] = timeline;
            Action<string> mark = name => timeline.Add(new JObject { ["mark"] = name, ["utc"] = DateTime.UtcNow.ToString("o"), ["frame"] = Time.frameCount });
            var owned = new List<UnitEntityData>();
            Vector3 centre = fixture.Caster.Position;
            UnitEntityData hostile = fixture.Hostile;
            Vector3 hostilePosition = hostile.Position;
            int hostileHitPoints = hostile.Descriptor.Stats.HitPoints.BaseValue;
            hostile.Descriptor.Stats.HitPoints.BaseValue = 100000;
            var recorder = new R4MephitRecorder(); EventBus.Subscribe(recorder);
            var saves = new List<Sprint26Saves> { new Sprint26Saves(hostile) { DesiredRoll = 1 } };
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
                PlaceExpandedSummoningUnit(hostile, ground(centre + Vector3.forward * 5f));
                foreach (int step in R6Wait(10)) yield return step;
                mark("baseline-start");
                foreach (int step in R6Seconds(3f)) yield return step;
                mark("baseline-end");
                foreach (int step in R6Cell("s7-ankylosaurus", () => S7Ankylosaurus(fixture, summon, hostile, record, mark, ground))) yield return step;
                foreach (int step in R6Cell("s7-dust", () => S7Dust(fixture, summon, hostile, record, mark, ground, recorder, abilityData, symbols, saves[0]))) yield return step;
                foreach (int step in R6Cell("s7-samples", () =>
                {
                    // The caster is the emitter: a summon can reach its native expiry during the samples.
                    UnitEntityData anchor = fixture.Caster;
                    UnitEntityData trumpet = summon("trumpet-archon");
                    PlaceExpandedSummoningUnit(trumpet, ground(centre + Vector3.right * 2f));
                    return S7Samples(anchor, record, mark, "impact", new[] { AnkylosaurusTailImpactPolicy.ImpactCue, "Physical_Hit_03_Punch", "Earthblast_Hit", "DustCritHeavy", "MediumBluntNatural_Flesh" })
                        .Concat(S7Samples(trumpet, record, mark, "trumpet", new[] { SummonPlaytestR6Policy.TrumpetBlastCue, "InspireGreatness", "CacophonousCall", "AoE_Sonic_Large", "Sweetener_High_04_Celestial", "HolyBomb", "Heroism" }));
                })) yield return step;
            }
            finally
            {
                var errors = new JArray();
                Action<string, Action> restore = (name, action) => { try { action(); } catch (Exception error) { errors.Add(name + ":" + error.Message); } };
                restore("saves", () => { foreach (Sprint26Saves value in saves) value.Dispose(); });
                restore("recorder", () => EventBus.Unsubscribe(recorder));
                restore("combat", () => {
                    foreach (UnitEntityData unit in owned.Concat(new[] { fixture.Hostile, fixture.Caster }).Where(value => value != null && !value.Destroyed))
                    {
                        unit.Commands.InterruptAll(command => true); unit.Commands.RemoveFinishedAndUpdateQueue();
                        if (unit.CombatState.IsInCombat) unit.CombatState.LeaveCombat();
                    }
                });
                restore("hostile", () => { if (!fixture.Hostile.Destroyed) { fixture.Hostile.Descriptor.Stats.HitPoints.BaseValue = hostileHitPoints; fixture.Hostile.Descriptor.Damage = 0; PlaceExpandedSummoningUnit(fixture.Hostile, hostilePosition); } });
                PlaceExpandedSummoningUnit(fixture.Caster, centre);
                DisposeExpandedSummoningUnits(fixture.Created, owned.Where(value => !value.Destroyed).ToArray());
                Sprint25Check("s7-effects-owned-state-restored", errors.Count == 0 && DustMephitBreathTint.Live == 0,
                    new JObject { ["errors"] = errors, ["liveRecolours"] = DustMephitBreathTint.Live }, "Readers, combat, positions and recolours restore.");
                record["restoreErrors"] = errors;
                RuntimeTestResultWriter.WriteAtomic(Path.Combine(_request.EvidenceDirectory, phase + ".json"), record.ToString(Formatting.Indented));
            }
        }
    }
}
