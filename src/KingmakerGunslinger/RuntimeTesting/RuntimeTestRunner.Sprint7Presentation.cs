using System;
using System.Collections.Generic;
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
            private readonly SkinnedMeshRenderer _renderer;
            private readonly Material _material;
            private readonly Mesh _current, _before;
            private readonly Texture _currentAlbedo;
            private readonly Texture2D _beforeAlbedo;
            internal readonly int CurrentVertices, BeforeVertices;

            internal S7BodySwap(UnitEntityData unit, S7Model model)
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
                CurrentVertices = _current.vertexCount; BeforeVertices = _before.vertexCount;
                if (!model.Salamander)
                {
                    string reason;
                    _beforeAlbedo = PteranodonAssetRuntime.LoadAlbedo(Path.GetDirectoryName(meshPath), requirement, out reason);
                    if (_beforeAlbedo == null) throw new InvalidDataException("Before albedo: " + reason);
                    _material = _renderer.material;
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

        private IEnumerable<int> S7Stills(UnitEntityData unit, S7Model model, string state, string scene, JObject row)
        {
            foreach (int step in SummonOverhaulSettlePartyCamera(unit)) yield return step;
            row["party"] = WriteExpandedSummoningPartyCameraCapture(unit, _request.EvidenceDirectory,
                "s7-" + model.Key + "-" + scene + "-" + state + "-party.png");
            row["dissolve"] = S7MaxDissolve(unit);
            Quaternion facing = unit.View.transform.rotation;
            foreach (var view in new[] { new { Name = "front", Local = new Vector3(.55f, .32f, .78f) }, new { Name = "rear", Local = new Vector3(-.55f, .32f, -.78f) } })
                using (var clip = new R6Clip(_request.EvidenceDirectory, "s7-" + model.Key + "-" + scene + "-" + state + "-" + view.Name,
                    () => R6RenderBounds(unit), facing * view.Local.normalized))
                { clip.Shot(new JObject { ["state"] = state }); row[view.Name] = clip.OutOfFrame == 0; }
            foreach (string[] detail in model.Details)
            {
                float radius = R7Number(detail[2]);
                var local = new Vector3(R7Number(detail[3]), R7Number(detail[4]), R7Number(detail[5])).normalized;
                string bone = detail[1];
                using (var clip = new R6Clip(_request.EvidenceDirectory, "s7-" + model.Key + "-" + scene + "-" + state + "-detail-" + detail[0],
                    () => R7BoneBounds(unit, bone, radius), facing * local, radius * unit.View.transform.lossyScale.y))
                { clip.Shot(new JObject { ["state"] = state }); row["detail-" + detail[0]] = clip.OutOfFrame == 0; }
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
            UnitEntityData unit = CastExpandedSummoningOwnTier(fixture, model.Key); owned.Add(unit);
            Sprint28SetBrainActivePreservingActions(unit, false);
            unit.Commands.InterruptAll(command => true); unit.Commands.RemoveFinishedAndUpdateQueue();
            PlaceExpandedSummoningUnit(unit, start);
            // Arrival effect, its rim and the appearance fade settle first.
            foreach (int step in R6Wait(R6ArrivalFrames + 60)) yield return step;
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
                bool settled = (float)row["settledDissolve"] <= .02f && (float)after["dissolve"] <= .02f;
                bool framed = new[] { after, before }.All(values => values.Properties()
                    .Where(value => value.Name != "party" && value.Name != "dissolve").All(value => (bool)value.Value));
                bool moved = (float)motionAfter["moveTravel"] > 1f && (float)motionBefore["moveTravel"] > 1f;
                bool turned = (float)motionAfter["turnDegrees"] > 45f && (float)motionBefore["turnDegrees"] > 45f;
                bool attacked = hostile == null || ((bool)motionAfter["attackActed"] && (bool)motionBefore["attackActed"]);
                bool different = swap.CurrentVertices != swap.BeforeVertices || model.Salamander;
                Sprint25Check("s7-" + model.Key + "-" + scene, shown && settled && framed && moved && turned && attacked && different,
                    (JObject)row.DeepClone(),
                    "The published summon shows the Sprint 7 export after its appearance settles; the pinned pre-Sprint-7 export is shown on the same live unit; both are framed in stills and details, and both complete the observed idle, movement, a turn of more than 45 degrees and any attack.");
            }
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
                ["limits"] = "Commanded motion with brains inactive. Images are presentation evidence only; aesthetic approval is the owner's." };
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

        private UnitEntityData S7OrdinarySummon(ExpandedSummoningCorrectionFixture fixture, string key)
        {
            // The real published summon through a native UnitUseAbility
            // command; the arrival and appearance buffs are kept and the unit
            // stays where the summon put it.
            UnitEntityData unit = CastExpandedSummoningVariant(fixture.Blueprints, fixture.Caster,
                ExpandedSummoningOwnTierVariant(key, SummonMultiplicity.One), null, fixture.Evidence).Single();
            fixture.Created.Add(unit);
            return unit;
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
            try
            {
                UnitEntityData first = S7OrdinarySummon(fixture, "ghaele-azata"); owned.Add(first);
                Sprint28SetBrainActivePreservingActions(first, false);
                subjects.Add(new S7LifeSubject { Label = "ghaele-a", Unit = first, Spawned = Game.Instance.Player.GameTime });
                UnitEntityData sibling = S7OrdinarySummon(fixture, "bralani-azata"); owned.Add(sibling);
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
                        if (attacked && attackRow["acted"] == null && t > 30 && firstAlive)
                        {
                            attackRow["acted"] = first.Commands.Empty || first.Commands.Raw.Any(value => value != null && value.IsActed);
                            first.Commands.InterruptAll(command => true); first.Commands.RemoveFinishedAndUpdateQueue();
                            if (hostile != null) hostile.Descriptor.Damage = 0;
                        }
                        // A second Ghaele later in the first one's life.
                        if (!secondSpawned && t > 36)
                        {
                            secondSpawned = true;
                            UnitEntityData other = S7OrdinarySummon(fixture, "ghaele-azata"); owned.Add(other);
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
                Sprint25Check("s7-ghaele-" + scene + "-visible-lifetime", visible && expired && walked, new JObject { ["a"] = rows[0].DeepClone(), ["move"] = moveRow.DeepClone(), ["attack"] = attackRow.DeepClone() },
                    "An ordinarily summoned Ghaele's body renderer (the enabled skinned renderer with the most vertices) has no surface-removing (cutout) dissolve above 0.02 from appearance completion until its native lifetime ends, an ordinary move travels more than 2 m, and the summon is removed at its genuine expiry. The raw _Dissolve, which also drives the native non-cutout edge band, is recorded beside it.");
                // Attribution: any other renderer whose dissolve rises in life,
                // compared with the same renderer on the native donor that was
                // spawned, never summoned, at the same game times.
                S7LifeSubject nativeDonor = subjects.FirstOrDefault(value => value.Label == "native-donor");
                var shared = new JArray(); int compared = 0, matched = 0;
                if (nativeDonor != null)
                    foreach (JObject sample in a.Samples.OfType<JObject>().Where(value => value["renderers"] != null && value["renderers"].Type == JTokenType.Object))
                    {
                        double at = (double)sample["t"] + (a.Spawned - nativeDonor.Spawned).TotalSeconds;
                        JObject donorSample = nativeDonor.Samples.OfType<JObject>().Where(value => value["renderers"] != null && value["renderers"].Type == JTokenType.Object)
                            .OrderBy(value => Math.Abs((double)value["t"] - at)).FirstOrDefault();
                        if (donorSample == null || Math.Abs((double)donorSample["t"] - at) > .6) continue;
                        foreach (JProperty level in ((JObject)sample["renderers"]).Properties().Where(value => (double)value.Value > .02))
                        {
                            JToken donorLevel = donorSample["renderers"][level.Name];
                            compared++;
                            if (donorLevel != null && Math.Abs((double)donorLevel - (double)level.Value) <= .1) matched++;
                            if (shared.Count < 12) shared.Add(new JObject { ["t"] = sample["t"], ["renderer"] = level.Name, ["summon"] = level.Value, ["donor"] = donorLevel });
                        }
                    }
                row["loopAttribution"] = new JObject { ["compared"] = compared, ["matchedDonor"] = matched, ["examples"] = shared };
                Sprint25Check("s7-ghaele-" + scene + "-dissolve-attribution", nativeDonor != null && compared > 0 && matched >= compared * 9 / 10,
                    (JObject)row["loopAttribution"].DeepClone(),
                    "Every non-body renderer whose _Dissolve rises during the summon's life carries the same value at the same moment on the native donor, which was spawned and never summoned: a native looping material, not the summon's appearance or lifetime.");
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
                ["limits"] = "Ordinary published summon through a native command at caster level 20, appearance kept, unpaused native time; brains inactive and moves and attacks issued as a player would. Lifetime is native game time; images are presentation evidence only." };
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

        private IEnumerable<int> S7OpenWindow(Cleanup2NativeDefaultScope mode, NativeIconScreenEvidence screens, string label, Ability fact,
            bool hudPath, JObject result)
        {
            var ui = Game.Instance.UI; var window = ui.DescriptionController.DescWindow;
            if (R3UnrelatedModal()) throw new InvalidOperationException("Unrelated native modal shown before opening a description.");
            if (window.gameObject.activeInHierarchy || ui.TooltipsController.ContextTooltipData != null)
                throw new InvalidOperationException("A foreign description window or hover is open.");
            // The fact path is the character-sheet/description window; the
            // HUD path is the action bar slot's AbilityData.
            var data = hudPath ? new TooltipData { Type = TooltipType.Ability, AbilityData = new AbilityData(fact) }
                : new TooltipData { Type = TooltipType.Ability, Ability = fact };
            ui.DescriptionController.HandleOpenDescription(data);
            Func<bool> owns = () => ExpandedSummoningNativeProfilePolicy.SummonPlaytestR3SurfaceOwned(mode.Current, Cleanup2NoNarrativeOverlay(),
                R3UnrelatedModal(), window != null && window.gameObject.activeInHierarchy && data.Type == TooltipType.Ability &&
                ReferenceEquals(window.Data, data) && fact.Owner != null && !fact.Owner.Unit.Destroyed && ui.TooltipsController.ContextTooltipData == null);
            for (int frame = 0; frame < 600 && !owns(); frame++) yield return 0;
            if (!owns()) throw new InvalidOperationException("Exact owned description window did not become visible: " + label);
            for (int frame = 0; frame < 20; frame++) { yield return 0; if (!owns()) throw new InvalidOperationException("Description ownership lost: " + label); }
            JObject card = S7WindowCard();
            foreach (int frame in screens.Capture("s7-description-" + label, card, owns)) yield return frame;
            card = S7WindowCard();
            card["path"] = hudPath ? "AbilityData (HUD action bar)" : "Ability fact (description window)";
            card["blueprint"] = fact.Blueprint.AssetGuid; card["owner"] = fact.Owner.Blueprint.AssetGuid;
            card["nativeTarget"] = fact.Blueprint.GetTarget();
            result[label] = card;
            ui.DescriptionController.HandleCloseDescriptionWindow(window);
            for (int frame = 0; frame < 120 && window.gameObject.activeInHierarchy; frame++) yield return 0;
            if (window.gameObject.activeInHierarchy) throw new InvalidOperationException("Native description window did not close.");
        }

        private IEnumerable<int> ReviewSprint7Ui(ExpandedSummoningCorrectionFixture fixture, string phase, Cleanup2NativeDefaultScope mode)
        {
            if (!IsSprint32Review || !_request.ExitAfterCompletion || !ReferenceEquals(fixture, _sprint25Fixture) ||
                fixture?.Scene == null || fixture.Caster == null || mode == null || !mode.Current ||
                !_context.FeatureModules.Active.ExpandedSummoning || phase != ExpandedSummoningNativeProfilePolicy.Sprint39S7UiPhase ||
                !Cleanup2NoNarrativeOverlay())
                throw new InvalidOperationException("Exact fresh Sprint 7 UI fixture in owned outdoor native Default required.");
            var record = new JObject { ["phase"] = phase, ["runId"] = _request.RunId,
                ["limits"] = "Desktop description windows (fact path) and the HUD AbilityData path, rendered and read back. The controller UI shares DescriptionBuilder/AbilityHeader/GetTarget but was not rendered in controller mode." };
            IDictionary<string, BlueprintScriptableObject> symbols = ExpandedSummoningBlueprints.Registered.BySymbol;
            var owned = new List<UnitEntityData>();
            var granted = new List<KeyValuePair<UnitEntityData, BlueprintAbility>>();
            Vector3 centre = fixture.Caster.Position;
            var windows = new JObject(); record["windows"] = windows;
            try
            {
                var screens = new NativeIconScreenEvidence(_request);
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
                // Controls: the native overrun these actions are cloned from
                // (same DoubleMove point template, not owned) and an unrelated
                // native spell, granted for the observation only.
                var nativeOverrun = ResourcesLibrary.TryGetBlueprint<BlueprintAbility>("1a3b471ecea51f7439a946b23577fd70");
                var magicMissile = ResourcesLibrary.TryGetBlueprint<BlueprintAbility>("4ac47ddb9fa1eaf43a1b6809980cfbd2");
                Ability nativeFact = null, spellFact = null;
                if (nativeOverrun != null) { nativeFact = brachiosaurus.Descriptor.AddFact(nativeOverrun) as Ability; granted.Add(new KeyValuePair<UnitEntityData, BlueprintAbility>(brachiosaurus, nativeOverrun)); }
                if (magicMissile != null) { spellFact = fixture.Caster.Descriptor.AddFact(magicMissile) as Ability; granted.Add(new KeyValuePair<UnitEntityData, BlueprintAbility>(fixture.Caster, magicMissile)); }
                var targets = new List<KeyValuePair<string, Ability>> {
                    new KeyValuePair<string, Ability>("brachiosaurus-overrun", fact(brachiosaurus, "KMG.Summoning.Special.Brachiosaurus.Overrun")),
                    new KeyValuePair<string, Ability>("brachiosaurus-trample", fact(brachiosaurus, "KMG.Summoning.Special.Brachiosaurus.Trample")),
                    new KeyValuePair<string, Ability>("mastodon-trample", fact(mastodon, "KMG.Summoning.Special.Mastodon.Trample")) };
                if (nativeFact != null) targets.Add(new KeyValuePair<string, Ability>("control-native-overrun", nativeFact));
                if (spellFact != null) targets.Add(new KeyValuePair<string, Ability>("control-native-spell", spellFact));
                foreach (var target in targets)
                    foreach (bool hud in new[] { false, true })
                    {
                        string label = target.Key + (hud ? "-hud" : "");
                        var entry = target;
                        foreach (int step in R6Cell("s7-ui-" + label, () => S7OpenWindow(mode, screens, label, entry.Value, hud, windows))) yield return step;
                    }
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
                        all(native).Any(value => value.Contains((string)native["nativeTarget"] == null ? "\u0000" : ((string)native["nativeTarget"]).Trim()));
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
                restore("window", () => { var window = Game.Instance.UI.DescriptionController.DescWindow; if (window != null && window.gameObject.activeInHierarchy) Game.Instance.UI.DescriptionController.HandleCloseDescriptionWindow(window); });
                restore("facts", () => { foreach (var entry in granted) if (!entry.Key.Destroyed && entry.Key.Descriptor.HasFact(entry.Value)) entry.Key.Descriptor.RemoveFact(entry.Value); });
                PlaceExpandedSummoningUnit(fixture.Caster, centre);
                DisposeExpandedSummoningUnits(fixture.Created, owned.Where(value => !value.Destroyed).ToArray());
                Sprint25Check("s7-ui-owned-state-restored", errors.Count == 0, new JObject { ["errors"] = errors }, "Windows, granted control facts and summons restore.");
                record["restoreErrors"] = errors;
                RuntimeTestResultWriter.WriteAtomic(Path.Combine(_request.EvidenceDirectory, phase + ".json"), record.ToString(Formatting.Indented));
            }
        }

        // ---------------------------------------------------------------
        // T5, T6, T7: Ankylosaurus impact cue, Dust breath, listening samples
        // ---------------------------------------------------------------

        private sealed class S7AttackLog : IGlobalRulebookHandler<RuleAttackWithWeapon>
        {
            internal readonly JArray Rows = new JArray();
            public void OnEventAboutToTrigger(RuleAttackWithWeapon evt) { }
            public void OnEventDidTrigger(RuleAttackWithWeapon evt)
            {
                if (evt == null || evt.Initiator == null) return;
                Rows.Add(new JObject { ["initiator"] = evt.Initiator.UniqueId, ["weapon"] = evt.Weapon == null ? null : evt.Weapon.Blueprint.name,
                    ["hit"] = evt.AttackRoll != null && evt.AttackRoll.IsHit, ["utc"] = DateTime.UtcNow.ToString("o"), ["frame"] = Time.frameCount });
            }
        }

        private static SummonCastCueRecord[] S7ImpactCues(UnitEntityData unit)
        {
            return SummonAbilityCastCueRuntime.Snapshot().Where(value => value.CasterId == unit.UniqueId &&
                value.Ability == AnkylosaurusTailImpactPolicy.ImpactCueLabel).ToArray();
        }

        private static int S7TailHits(S7AttackLog log, UnitEntityData unit, int from)
        {
            return log.Rows.Skip(from).Count(value => (string)value["initiator"] == unit.UniqueId && (bool)value["hit"] &&
                ((string)value["weapon"] ?? "").IndexOf("Tail", StringComparison.OrdinalIgnoreCase) >= 0);
        }

        private static JObject S7Cue(SummonCastCueRecord cue)
        {
            return new JObject { ["event"] = cue.EventName, ["playingId"] = cue.PlayingId, ["utc"] = cue.Utc.ToString("o"),
                ["endUtc"] = cue.EndUtc.HasValue ? cue.EndUtc.Value.ToString("o") : null, ["stopReason"] = cue.StopReason,
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

                // 1. Qualifying impacts: one cue per resolved tail hit.
                int logFrom = log.Rows.Count, cuesFrom = S7ImpactCues(first).Length;
                mark("anky-hits-start");
                foreach (int step in S7AttackUntil(first, hostile, () => S7TailHits(log, first, logFrom) >= 2, 6)) yield return step;
                foreach (int step in R6Seconds(4f)) yield return step;
                mark("anky-hits-end");
                int tailHits = S7TailHits(log, first, logFrom);
                SummonCastCueRecord[] hitCues = S7ImpactCues(first).Skip(cuesFrom).ToArray();
                row["attacks"] = new JArray(log.Rows.Skip(logFrom));
                row["tailHits"] = tailHits; row["hitCues"] = new JArray(hitCues.Select(S7Cue));
                Sprint25Check("s7-anky-one-cue-per-impact", tailHits >= 1 && hitCues.Length == tailHits &&
                    hitCues.All(value => value.EventName == AnkylosaurusTailImpactPolicy.ImpactCue && value.PlayingId != 0 && value.EndUtc.HasValue && value.StopReason == null),
                    new JObject { ["tailHits"] = tailHits, ["cues"] = row["hitCues"].DeepClone(), ["attacks"] = row["attacks"].DeepClone() },
                    "Each resolved tail hit by the exact owned Ankylosaurus posts exactly one accepted impact cue, which completes naturally; its other attacks post none.");

                // 2. A miss: the hostile's armor is raised out of reach.
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

                // 3. Cancelled before the strike resolves.
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

                // 4. Concurrent sources; one retired while its cue plays.
                int firstFrom = S7ImpactCues(first).Length, secondFrom = S7ImpactCues(second).Length;
                mark("anky-concurrent-start");
                first.Commands.Run(new UnitAttack(hostile));
                second.Commands.Run(new UnitAttack(hostile));
                DateTime wait = DateTime.UtcNow.AddSeconds(16);
                while (DateTime.UtcNow < wait && !(S7ImpactCues(second).Length > secondFrom && S7ImpactCues(first).Length > firstFrom)) yield return 0;
                SummonCastCueRecord retiring = S7ImpactCues(second).Skip(secondFrom).FirstOrDefault();
                bool playingAtRetirement = retiring != null && !retiring.EndUtc.HasValue;
                DateTime retiredAt = DateTime.UtcNow;
                mark("anky-retire");
                DisposeExpandedSummoningUnits(fixture.Created, new[] { second });
                foreach (int step in R6Seconds(5f)) yield return step;
                first.Commands.InterruptAll(command => true); first.Commands.RemoveFinishedAndUpdateQueue();
                mark("anky-concurrent-end");
                SummonCastCueRecord[] survivors = S7ImpactCues(first).Skip(firstFrom).ToArray();
                row["retired"] = retiring == null ? null : S7Cue(retiring);
                row["survivors"] = new JArray(survivors.Select(S7Cue));
                row["playingAtRetirement"] = playingAtRetirement;
                bool retiredStopped = retiring != null && (!playingAtRetirement || retiring.StopReason != null && retiring.EndUtc.HasValue);
                bool survivorNatural = survivors.Length >= 1 && survivors.All(value => value.EndUtc.HasValue && value.StopReason == null);
                Sprint25Check("s7-anky-concurrent-retirement", retiring != null && retiredStopped && survivorNatural,
                    new JObject { ["retired"] = row["retired"] == null ? null : row["retired"].DeepClone(), ["survivors"] = row["survivors"].DeepClone(),
                        ["playingAtRetirement"] = playingAtRetirement, ["retiredUtc"] = retiredAt.ToString("o") },
                    "With two Ankylosaurs striking together, retiring one stops only its own cue (if still playing) by its playing ID while the other's cue completes naturally.");
            }
            finally
            {
                hostile.Descriptor.Stats.AC.BaseValue = armor;
                EventBus.Unsubscribe(log);
            }
        }

        // Read-only record of each projectile's cast effect, the visible
        // cone: native CreateView spawns it through FxHelper.SpawnFxOnPoint.
        // The prefix notes the launching projectile; the last-running postfix
        // on the spawn records the instance as the product left it.
        private sealed class S7ProjectileTrace : IDisposable
        {
            private static S7ProjectileTrace active;
            private static Kingmaker.Controllers.Projectiles.Projectile current;
            private readonly Harmony12.HarmonyInstance harmony;
            private readonly string id = "KMG.S7.Projectile." + Guid.NewGuid().ToString("N");
            private readonly System.Reflection.MethodInfo create = typeof(Kingmaker.Controllers.Projectiles.ProjectileController)
                .GetMethod("CreateView", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic |
                    System.Reflection.BindingFlags.Instance, null,
                    new[] { typeof(Kingmaker.Controllers.Projectiles.Projectile), typeof(Vector3?) }, null);
            private readonly System.Reflection.MethodInfo spawn = typeof(Kingmaker.Visual.Particles.FxHelper).GetMethod("SpawnFxOnPoint",
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static, null,
                new[] { typeof(GameObject), typeof(Vector3), typeof(Quaternion) }, null);
            internal readonly JArray Rows = new JArray();

            internal S7ProjectileTrace()
            {
                if (active != null || create == null || spawn == null) throw new InvalidOperationException("Projectile readers unavailable.");
                harmony = Harmony12.HarmonyInstance.Create(id);
                var flags = System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic;
                harmony.Patch(create, new Harmony12.HarmonyMethod(typeof(S7ProjectileTrace).GetMethod("Before", flags)) { prioritiy = Harmony12.Priority.First },
                    new Harmony12.HarmonyMethod(typeof(S7ProjectileTrace).GetMethod("AfterCreate", flags)) { prioritiy = Harmony12.Priority.Last });
                harmony.Patch(spawn, null, new Harmony12.HarmonyMethod(typeof(S7ProjectileTrace).GetMethod("AfterSpawn", flags)) { prioritiy = Harmony12.Priority.Last });
                active = this;
            }

            private static void Before(Kingmaker.Controllers.Projectiles.Projectile __0) { current = __0; }
            private static void AfterCreate() { current = null; }

            private static void AfterSpawn(GameObject __result, GameObject __0)
            {
                var value = active; var projectile = current;
                if (value == null || projectile == null || projectile.Launcher == null || __result == null) return;
                DustMephitBreathTint tint = __result.GetComponent<DustMephitBreathTint>();
                var renderers = __result.GetComponentsInChildren<Renderer>(true);
                value.Rows.Add(new JObject { ["frame"] = Time.frameCount, ["utc"] = DateTime.UtcNow.ToString("o"),
                    ["launcher"] = projectile.Launcher.UniqueId, ["launcherBlueprint"] = projectile.Launcher.Blueprint.AssetGuid,
                    ["projectile"] = projectile.Blueprint == null ? null : projectile.Blueprint.AssetGuid,
                    ["castFx"] = __0 == null ? null : __0.name, ["view"] = __result.GetInstanceID(),
                    ["tinted"] = tint != null, ["recolouredMaterials"] = tint == null ? 0 : tint.RecolouredMaterials,
                    ["recolouredParticleColours"] = tint == null ? 0 : tint.RecolouredParticleColours,
                    ["materials"] = new JArray(renderers.SelectMany(renderer => renderer.sharedMaterials).Where(material => material != null)
                        .Select(material => material.name + "|" + (material.shader == null ? "" : material.shader.name)).Distinct().Take(16)) });
            }

            public void Dispose()
            {
                if (create != null) harmony.Unpatch(create, Harmony12.HarmonyPatchType.All, id);
                if (spawn != null) harmony.Unpatch(spawn, Harmony12.HarmonyPatchType.All, id);
                if (ReferenceEquals(active, this)) active = null;
                current = null;
            }
        }

        private static bool S7InCone(UnitEntityData caster, Vector3 aim, UnitEntityData unit, float feet)
        {
            Vector3 toUnit = unit.Position - caster.Position; toUnit.y = 0f;
            Vector3 axis = aim - caster.Position; axis.y = 0f;
            float reach = feet * .3048f + unit.View.Corpulence;
            return toUnit.magnitude <= reach && Vector3.Angle(axis, toUnit) <= 45f;
        }

        private IEnumerable<int> S7Dust(ExpandedSummoningCorrectionFixture fixture, Func<string, UnitEntityData> summon, UnitEntityData hostile,
            JObject record, Action<string> mark, Func<Vector3, Vector3> ground, R4MephitRecorder recorder,
            Func<UnitEntityData, BlueprintAbility, AbilityData> abilityData, IDictionary<string, BlueprintScriptableObject> symbols)
        {
            var row = new JObject(); record["dust"] = row;
            var breath = (BlueprintAbility)symbols["KMG.Summoning.Special.DustMephit.Breath"];
            var iceBreath = (BlueprintAbility)symbols["KMG.Summoning.Special.IceMephit.Breath"];
            var sickened = ResourcesLibrary.TryGetBlueprint<Kingmaker.UnitLogic.Buffs.Blueprints.BlueprintBuff>("4e42460798665fd4cb9173ffa7ada323");
            UnitEntityData dust = summon("dust-mephit"), ice = summon("ice-mephit"), spare = summon("dust-mephit");
            Vector3 aim = hostile.Position;
            PlaceExpandedSummoningUnit(dust, ground(hostile.Position + Vector3.back * 2.6f));
            PlaceExpandedSummoningUnit(ice, ground(hostile.Position + Vector3.right * 6f + Vector3.back * 4f));
            PlaceExpandedSummoningUnit(spare, ground(hostile.Position + Vector3.left * 6f + Vector3.back * 4f));
            // The caster stands behind the Dust Mephit, outside its cone, as an
            // ally that must never be affected.
            PlaceExpandedSummoningUnit(fixture.Caster, ground(dust.Position + Vector3.back * 3f));
            foreach (int step in R6Seconds(2f)) yield return step;
            Vector3 bearing = Quaternion.Euler(0f, 60f, 0f) * Vector3.back;
            Func<Bounds> frame = () => R6RenderBounds(dust, hostile);
            using (var trace = new S7ProjectileTrace())
            {
                // 1. Committed casts at normal speed: one bounded dust effect each.
                var casts = new JArray(); row["casts"] = casts;
                for (int index = 0; index < 3; index++)
                {
                    var cast = new JObject(); casts.Add(cast);
                    int damageFrom = recorder.Damage.Count, appliedFrom = DustMephitBreathTint.Applied, rowsFrom = trace.Rows.Count;
                    hostile.Descriptor.Damage = 0;
                    if (sickened != null) foreach (Buff buff in hostile.Descriptor.Buffs.Enumerable.Where(value => ReferenceEquals(value.Blueprint, sickened)).ToArray()) buff.Remove();
                    PlaceExpandedSummoningUnit(dust, ground(hostile.Position + Vector3.back * 2.6f + Vector3.right * (.3f * index)));
                    foreach (int step in R6Wait(4)) yield return step;
                    mark("dust-cast-" + index + "-start");
                    using (var clip = new R6Clip(_request.EvidenceDirectory, "s7-dust-cast-" + index, frame, bearing))
                        foreach (int step in R6RunPassive(dust, abilityData(dust, breath), new TargetWrapper(hostile), cast, clip)) yield return step;
                    int liveAtEnd = DustMephitBreathTint.Live;
                    DateTime release = DateTime.UtcNow, releaseDeadline = DateTime.UtcNow.AddSeconds(20);
                    while (DateTime.UtcNow < releaseDeadline && DustMephitBreathTint.Live > 0) yield return 0;
                    cast["secondsToRelease"] = (DateTime.UtcNow - release).TotalSeconds;
                    foreach (int step in R6Seconds(1f)) yield return step;
                    mark("dust-cast-" + index + "-end");
                    var damage = recorder.Damage.Skip(damageFrom).Where(value => ReferenceEquals(value.Initiator, dust)).ToArray();
                    cast["damage"] = new JArray(damage.Select(value => new JObject { ["target"] = value.Target.UniqueId,
                        ["isHostile"] = ReferenceEquals(value.Target, hostile), ["total"] = value.Damage,
                        ["types"] = string.Join(",", value.DamageBundle.Select(part => part.Type.ToString()).ToArray()) }));
                    cast["projectiles"] = new JArray(trace.Rows.Skip(rowsFrom).Where(value => (string)value["launcher"] == dust.UniqueId));
                    cast["applied"] = DustMephitBreathTint.Applied - appliedFrom; cast["liveAtCommandEnd"] = liveAtEnd;
                    cast["recolouredMaterials"] = DustMephitBreathTint.LastMaterials;
                    cast["recolouredParticleColours"] = DustMephitBreathTint.LastParticleColours;
                    cast["recolouredRenderers"] = new JArray(DustMephitBreathTint.LastRenderers);
                    cast["liveAfter"] = DustMephitBreathTint.Live;
                    cast["hostileInCone"] = S7InCone(dust, aim, hostile, 15f);
                    cast["allyInCone"] = S7InCone(dust, aim, fixture.Caster, 15f);
                    cast["allyDamaged"] = damage.Any(value => ReferenceEquals(value.Target, fixture.Caster));
                    cast["hostileSickened"] = sickened != null && hostile.Descriptor.Buffs.Enumerable.Any(value => ReferenceEquals(value.Blueprint, sickened));
                }
                bool oneEach = casts.All(value => (int)value["applied"] == 1 && ((JArray)value["projectiles"]).Count == 1 &&
                    (bool)((JArray)value["projectiles"])[0]["tinted"] && (int)value["liveAfter"] == 0 &&
                    (int)value["recolouredMaterials"] + (int)value["recolouredParticleColours"] > 0);
                bool mechanics = casts.All(value => (bool)value["hostileInCone"] && ((JArray)value["damage"]).Any(part => (bool)part["isHostile"]) &&
                    !(bool)value["allyDamaged"]);
                Sprint25Check("s7-dust-casts", oneEach && mechanics, new JObject { ["casts"] = casts.DeepClone() },
                    "Each committed Dust breath at normal speed launches one cold-cone instance whose actual renderers or particle colours the product recolours as dust, deals its damage to the hostile inside the forward 15-foot cone and none to the ally behind, and the recolour is restored after the effect ends.");

                // 2. Cancelled before execution: no effect, no damage.
                int cancelApplied = DustMephitBreathTint.Applied, cancelRows = trace.Rows.Count, cancelDamage = recorder.Damage.Count;
                mark("dust-cancel-start");
                var cancelled = new UnitUseAbility(abilityData(dust, breath), new TargetWrapper(hostile));
                dust.Commands.Run(cancelled);
                yield return 0;
                dust.Commands.InterruptAll(command => true); dust.Commands.RemoveFinishedAndUpdateQueue();
                foreach (int step in R6Seconds(3f)) yield return step;
                mark("dust-cancel-end");
                Sprint25Check("s7-dust-cancel", DustMephitBreathTint.Applied == cancelApplied &&
                    trace.Rows.Skip(cancelRows).All(value => (string)value["launcher"] != dust.UniqueId) &&
                    recorder.Damage.Skip(cancelDamage).All(value => !ReferenceEquals(value.Initiator, dust)),
                    new JObject { ["applied"] = DustMephitBreathTint.Applied - cancelApplied }, "A breath interrupted before execution launches nothing and deals nothing.");

                // 3. Concurrent donor: the Ice Mephit's cold cone at the same time.
                int concurrentRows = trace.Rows.Count;
                mark("dust-concurrent-start");
                var dustCast = new JObject(); var iceCast = new JObject();
                IEnumerator<int> dustRun = R6RunPassive(dust, abilityData(dust, breath), new TargetWrapper(hostile), dustCast, null).GetEnumerator();
                IEnumerator<int> iceRun = R6RunPassive(ice, abilityData(ice, iceBreath), new TargetWrapper(hostile), iceCast, null).GetEnumerator();
                bool dustMore = true, iceMore = true;
                int bothLive = 0;
                while (dustMore || iceMore)
                {
                    if (dustMore) dustMore = dustRun.MoveNext();
                    if (iceMore) iceMore = iceRun.MoveNext();
                    bothLive = Math.Max(bothLive, UnityEngine.Object.FindObjectsOfType<DustMephitBreathTint>().Length);
                    yield return 0;
                }
                DateTime concurrentDeadline = DateTime.UtcNow.AddSeconds(20);
                while (DateTime.UtcNow < concurrentDeadline && DustMephitBreathTint.Live > 0) yield return 0;
                foreach (int step in R6Seconds(1f)) yield return step;
                mark("dust-concurrent-end");
                var concurrent = trace.Rows.Skip(concurrentRows).ToArray();
                var iceViews = concurrent.Where(value => (string)value["launcher"] == ice.UniqueId).ToArray();
                var dustViews = concurrent.Where(value => (string)value["launcher"] == dust.UniqueId).ToArray();
                bool iceNative = iceViews.Length == 1 && !(bool)iceViews[0]["tinted"] &&
                    !((JArray)iceViews[0]["materials"]).Values<string>().Any(value => value.Contains("_KMG_Dust"));
                Sprint25Check("s7-dust-donor-untouched", iceNative && dustViews.Length == 1 && (bool)dustViews[0]["tinted"] && bothLive <= 1 && DustMephitBreathTint.Live == 0,
                    new JObject { ["ice"] = new JArray(iceViews), ["dust"] = new JArray(dustViews), ["maxLiveRecolours"] = bothLive },
                    "Cast together, the Dust Mephit's cone is recoloured and the Ice Mephit's instance of the same native cone keeps native materials and colours.");

                // 4. Source loss while the effect is active.
                int lossApplied = DustMephitBreathTint.Applied, lossRows = trace.Rows.Count;
                mark("dust-loss-start");
                var lossCast = new UnitUseAbility(abilityData(spare, breath), new TargetWrapper(hostile));
                PlaceExpandedSummoningUnit(spare, ground(hostile.Position + Vector3.left * 2.6f));
                foreach (int step in R6Wait(4)) yield return step;
                spare.Commands.Run(lossCast);
                DateTime waitView = DateTime.UtcNow.AddSeconds(12);
                while (DateTime.UtcNow < waitView && DustMephitBreathTint.Applied == lossApplied) yield return 0;
                bool launched = DustMephitBreathTint.Applied > lossApplied;
                mark("dust-loss-despawn");
                DisposeExpandedSummoningUnits(fixture.Created, new[] { spare });
                DateTime lossDeadline = DateTime.UtcNow.AddSeconds(20);
                while (DateTime.UtcNow < lossDeadline && DustMephitBreathTint.Live > 0) yield return 0;
                foreach (int step in R6Seconds(1f)) yield return step;
                mark("dust-loss-end");
                Sprint25Check("s7-dust-source-loss", launched && DustMephitBreathTint.Live == 0 &&
                    UnityEngine.Object.FindObjectsOfType<DustMephitBreathTint>().Length == 0,
                    new JObject { ["launched"] = launched, ["live"] = DustMephitBreathTint.Live, ["rows"] = new JArray(trace.Rows.Skip(lossRows)) },
                    "A Dust Mephit removed while its breath effect is active leaves no recolour behind once the effect ends.");

                // 5. Pool reuse after recolours: a later Ice cone instance is native.
                int reuseRows = trace.Rows.Count;
                var iceAgain = new JObject();
                foreach (int step in R6RunPassive(ice, abilityData(ice, iceBreath), new TargetWrapper(hostile), iceAgain, null)) yield return step;
                foreach (int step in R6Seconds(2f)) yield return step;
                var reuse = trace.Rows.Skip(reuseRows).Where(value => (string)value["launcher"] == ice.UniqueId).ToArray();
                var dustIds = trace.Rows.Take(reuseRows).Where(value => (bool)value["tinted"]).Select(value => (int)value["view"]).ToArray();
                Sprint25Check("s7-dust-pool-reuse", reuse.Length == 1 && !(bool)reuse[0]["tinted"] &&
                    !((JArray)reuse[0]["materials"]).Values<string>().Any(value => value.Contains("_KMG_Dust")),
                    new JObject { ["ice"] = new JArray(reuse), ["reusedADustInstance"] = reuse.Length == 1 && dustIds.Contains((int)reuse[0]["view"]) },
                    "A later Ice Mephit cone, including a pooled instance a Dust breath used before, shows native materials.");

                // 6. A native-look comparison of the same Dust cast: the
                // observer restores the recolour as soon as it is applied.
                var nativeLook = new JObject(); row["nativeLook"] = nativeLook;
                int lookApplied = DustMephitBreathTint.Applied;
                mark("dust-native-look-start");
                using (var clip = new R6Clip(_request.EvidenceDirectory, "s7-dust-native-look", frame, bearing))
                {
                    var command = new UnitUseAbility(abilityData(dust, breath), new TargetWrapper(hostile));
                    dust.Commands.Run(command);
                    DateTime deadline = DateTime.UtcNow.AddSeconds(20);
                    bool restored = false;
                    while (DateTime.UtcNow < deadline && !(command.IsFinished && (command.ExecutionProcess == null || command.ExecutionProcess.IsEnded)))
                    {
                        if (!restored && DustMephitBreathTint.Applied > lookApplied)
                        {
                            foreach (DustMephitBreathTint tint in UnityEngine.Object.FindObjectsOfType<DustMephitBreathTint>()) tint.Restore();
                            restored = true;
                        }
                        clip.Shot(R6AnimationState(dust, command));
                        yield return 0;
                    }
                    nativeLook["restoredByObserver"] = restored;
                }
                mark("dust-native-look-end");
            }
        }

        private IEnumerable<int> S7Samples(UnitEntityData anchor, JObject record, Action<string> mark, string group, string[] events)
        {
            var rows = new JArray(); record["samples-" + group] = rows;
            foreach (string name in events)
            {
                mark("sample-" + group + "-" + name);
                uint id = AkSoundEngine.PostEvent(name, anchor.View.gameObject);
                rows.Add(new JObject { ["event"] = name, ["playingId"] = id, ["utc"] = DateTime.UtcNow.ToString("o"),
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
                foreach (int step in R6Cell("s7-dust", () => S7Dust(fixture, summon, hostile, record, mark, ground, recorder, abilityData, symbols))) yield return step;
                foreach (int step in R6Cell("s7-samples", () =>
                {
                    UnitEntityData anchor = owned.FirstOrDefault(value => !value.Destroyed) ?? summon("ankylosaurus");
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
